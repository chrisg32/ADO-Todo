using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADOTodo.Models;
using ADOTodo.Services;
using ADOTodo.Util;
using CG.Commons.Extensions;

namespace ADOTodo.Getters
{
    public class ComprehensivePrGetter : IGetter
    {
        public int RunOrder => 1;
        
        public async Task<List<ITodoItem>> GetAsync(Settings settings, AdoService adoService)
        {
            var todos = new HashSet<PrThreadTodoItem>(PrThreadTodoItem.EqualityComparer);

            if (settings.Comprehensive.Level != ThreadFilterLevel.None)
            {
                var pullRequests = await adoService.GetComprehensivePullRequests();
                switch (settings.Comprehensive.Level)
                {
                    case ThreadFilterLevel.Mentions:
                        todos.AddRange(pullRequests.SelectMany(pr => pr.Threads.Where(thread => thread.MentionsUser(adoService.UserId)).Select(t => new ComprehensivePrThreadTodoItem(pr, t, adoService.Uri, settings.Project))));
                        break;
                    case ThreadFilterLevel.Comments:
                        throw new NotImplementedException();
                        break;
                    case ThreadFilterLevel.Threads:
                        throw new NotImplementedException();
                        break;
                    case ThreadFilterLevel.All:
                        todos.AddRange(pullRequests.SelectMany(pr => pr.Threads.Select(t => new ComprehensivePrThreadTodoItem(pr, t, adoService.Uri, settings.Project))));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var userIds = todos.SelectMany(t => t.GetMentions()).Distinct();
            var userMap = new Dictionary<Guid, string?>();
            foreach (var id in userIds)
            {
                //TODO cache this
                var displayName = await adoService.GetUserName(id);
                userMap.Add(id, displayName);
            }

            foreach (var todo in todos)
            {
                todo.UpdateDescriptionTextWithMentions(userMap);
            }

            return todos.Cast<ITodoItem>().ToList();
        }
    }
}