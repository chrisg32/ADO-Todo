using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;
using ADOTodo.Models;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace ADOTodo.Services
{
    public class AdoService : IDisposable
    {
        private readonly string _projectName;
        private readonly VssConnection _connection;
        private readonly GitHttpClient _gitClient;
        private readonly IdentityHttpClient _userClient;
        private readonly WorkItemTrackingHttpClient _workItemClient;
        private const string AdoTodoQueryFolder = "ADO-Todo";
        
        public AdoService(Settings settings)
        {
            _projectName = settings.Project;
            var credentials = new VssBasicCredential(string.Empty, settings.Token);
            _connection = new VssConnection(new Uri(settings.Server), credentials);
            _gitClient = _connection.GetClient<GitHttpClient>();
            _userClient = _connection.GetClient<IdentityHttpClient>();
            _workItemClient = _connection.GetClient<WorkItemTrackingHttpClient>();
            UserId = _connection.AuthenticatedIdentity.Id;
        }
        
        public Guid UserId { get; }
        public Uri Uri => _connection.Uri;

        public async Task<List<PrSummary>> GetAllPullRequestsOpenedByMe(bool activeOnly = true)
        {
            var pullRequests = await _gitClient.GetPullRequestsByProjectAsync(_projectName, new GitPullRequestSearchCriteria
            {
                CreatorId = UserId,
                Status = PullRequestStatus.Active
            });
            
            return await AddThreads(pullRequests, activeOnly);
        }

        public async Task<List<WorkItem>> QueryWorkItems(string wiql)
        {
            var wiqlO = new Wiql
            {
                Query = wiql
            };

            var result = await _workItemClient.QueryByWiqlAsync(wiqlO, _projectName);
            var ids = result.WorkItems.Select(item => item.Id).ToList();

            if (!ids.Any())
            {
                return new List<WorkItem>();
            }

            //https://docs.microsoft.com/en-us/azure/devops/boards/work-items/guidance/work-item-field?view=azure-devops
            var fields = new[] {"System.Id", "System.Title", "System.Description", "System.WorkItemType", "Microsoft.VSTS.Common.BacklogPriority", "System.ChangedDate"};

            var workItems = await _workItemClient.GetWorkItemsAsync(ids, fields);

            return workItems;
        }

        private async Task<List<PrSummary>> AddThreads(IEnumerable<GitPullRequest> pullRequests, bool activeOnly)
        {
            var list = new List<PrSummary>();
            foreach (var pr in pullRequests)
            {
                var threads = await _gitClient.GetThreadsAsync(_projectName, pr.Repository.Id, pr.PullRequestId);
                list.Add(new PrSummary(pr, threads.Where(t => t.Status != CommentThreadStatus.Unknown
                && t.Comments.Any(c => c.CommentType == CommentType.Text)
                && (!activeOnly || t.Status == CommentThreadStatus.Active)).ToList()));
            }
            return list;
        }

        public async Task<string?> GetUserName(Guid id)
        {
            var user = await _userClient.ReadIdentityAsync(id);
            return user?.DisplayName;
        }

        public void Dispose()
        {
            _gitClient.Dispose();
            _connection.Dispose();
            _userClient.Dispose();
            _workItemClient.Dispose();
        }

        public async Task<List<PrSummary>> GetComprehensivePullRequests(bool activeOnly = true)
        {
            var pullRequests = await _gitClient.GetPullRequestsByProjectAsync(_projectName, new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active,
                TargetRefName = "refs/heads/master",
            });

            pullRequests = pullRequests.Where(pr => pr.SourceRefName.StartsWith("refs/heads/release/")).ToList();
            
            return await AddThreads(pullRequests, activeOnly);
        }
    }
}