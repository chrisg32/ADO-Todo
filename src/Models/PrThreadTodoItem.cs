using System;
using System.Collections.Generic;
using System.Linq;
using ADOTodo.Util;
using Avalonia.Media;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace ADOTodo.Models
{
    public class PrThreadTodoItem : TodoItem
    {
        public PrThreadTodoItem(PrSummary pr, GitPullRequestCommentThread thread, Uri baseUri, string project)
        {
            PrId = pr.PullRequest.PullRequestId;
            ThreadId = thread.Id;
            Id = $"!{pr.PullRequest.PullRequestId}";
            Title = pr.PullRequest.Title;
            Color = Color.FromRgb(103, 40, 120);
            var commentText = thread.Comments.OrderBy(c => c.PublishedDate).First().Content;
            commentText = commentText.BlockTruncate();
            Description = commentText ?? string.Empty;
            Date = thread.LastUpdatedDate.ToLocalTime();
            Url = $"{baseUri}/{project}/_git/{project}/pullrequest/{pr.PullRequest.PullRequestId}?discussionId={thread.Id}";
        }
        public override string ItemType => "PR";
        public override Color Color { get; }
        public override int ItemTypePriority => 3;
        
        public int PrId { get; }
        public int ThreadId { get; }

        public override bool Equals(object? obj)
        {
            if(obj is PrThreadTodoItem other) return PrId == other.PrId && ThreadId == other.ThreadId;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PrId, ThreadId);
        }

        public static IEqualityComparer<PrThreadTodoItem> EqualityComparer { get; } =
            new PrItemTodoItemEqualityComparer();
    }
}