using System;
using Avalonia.Media;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace ADOTodo.Models
{
    public class ComprehensivePrThreadTodoItem : PrThreadTodoItem
    {
        public ComprehensivePrThreadTodoItem(PrSummary pr, GitPullRequestCommentThread thread, Uri baseUri, string project) : base(pr, thread, baseUri, project)
        {
            
        }
        
        public override Color Color => Color.FromRgb(255, 102, 102);

        public override string ItemType => "Comprehensive";
        public override int ItemTypePriority => 2;
    }
}