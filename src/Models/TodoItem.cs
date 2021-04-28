using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using ADOTodo.Util;

namespace ADOTodo.Models
{
    public class TodoItem
    {
        public static IEqualityComparer<TodoItem> TodoItemEqualityComparer { get; } = new TodoItemEqualityComparer();

        private readonly PrSummary _pr;
        private readonly GitPullRequestCommentThread _thread;

        public int PrId { get; }
        public int ThreadId { get; }

        public string PullRequestTitle => _pr.PullRequest.Title;
        public string PullRequestId => $"!{_pr.PullRequest.PullRequestId}";
        
        public string? CommentText { get; set; }
        
        public DateTime LastUpdated { get; }
        public string Url { get; }

        public TodoItem(PrSummary pr, GitPullRequestCommentThread thread, Uri baseUri, string project)
        {
            _pr = pr;
            _thread = thread;
            PrId = pr.PullRequest.PullRequestId;
            ThreadId = thread.Id;
            var commentText = thread.Comments.OrderBy(c => c.PublishedDate).First().Content;
            commentText = commentText.BlockTruncate(200);
            CommentText = commentText;
            LastUpdated = thread.LastUpdatedDate.ToLocalTime();
            Url = $"{baseUri}/{project}/_git/{project}/pullrequest/{PrId}?discussionId={ThreadId}";
        }
    }
    
    internal sealed class TodoItemEqualityComparer : IEqualityComparer<TodoItem>
    {
        public bool Equals(TodoItem x, TodoItem y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.PrId == y.PrId && x.ThreadId == y.ThreadId;
        }

        public int GetHashCode(TodoItem obj)
        {
            return HashCode.Combine(obj.PrId, obj.ThreadId);
        }
    }
}