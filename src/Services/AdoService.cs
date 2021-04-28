using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;
using ADOTodo.Models;

namespace ADOTodo.Services
{
    public class AdoService : IDisposable
    {
        private readonly string _projectName;
        private readonly VssConnection _connection;
        private readonly GitHttpClient _client;
        private readonly IdentityHttpClient _userClient;
        
        public AdoService(ConnectionSettings settings)
        {
            _projectName = settings.Project;
            var credentials = new VssBasicCredential(string.Empty, settings.Token);
            _connection = new VssConnection(new Uri(settings.Server), credentials);
            _client = _connection.GetClient<GitHttpClient>();
            _userClient = _connection.GetClient<IdentityHttpClient>();
            UserId = _connection.AuthenticatedIdentity.Id;
        }
        
        public Guid UserId { get; }
        public Uri Uri => _connection.Uri;

        public async Task<List<PrSummary>> GetAllPullRequestsOpenedByMe(bool activeOnly = true)
        {
            var pullRequests = await _client.GetPullRequestsByProjectAsync(_projectName, new GitPullRequestSearchCriteria
            {
                CreatorId = UserId,
                Status = PullRequestStatus.Active
            });
            
            return await AddThreads(pullRequests, activeOnly);
        }

        private async Task<List<PrSummary>> AddThreads(IEnumerable<GitPullRequest> pullRequests, bool activeOnly)
        {
            var list = new List<PrSummary>();
            foreach (var pr in pullRequests)
            {
                var threads = await _client.GetThreadsAsync(_projectName, pr.Repository.Id, pr.PullRequestId);
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
            _client.Dispose();
            _connection.Dispose();
            _userClient.Dispose();
        }
    }
}