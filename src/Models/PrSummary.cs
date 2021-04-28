using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace ADOTodo.Models
{
    [DebuggerDisplay("(!{PullRequest.PullRequestId}) {PullRequest.Title}")]
    public class PrSummary
    {
        public PrSummary(GitPullRequest pr, List<GitPullRequestCommentThread> threads)
        {
            PullRequest = pr;
            Threads = threads;
        }

        public GitPullRequest PullRequest { get; }
        public List<GitPullRequestCommentThread> Threads { get; }
    }
}