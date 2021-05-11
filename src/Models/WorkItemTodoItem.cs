using System;
using ADOTodo.Util;
using Avalonia.Media;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace ADOTodo.Models
{
    public class WorkItemTodoItem : ITodoItem
    {
        public WorkItemTodoItem(WorkItem workItem, Uri baseUri, string project)
        {
            Id = $"#{workItem.Fields["System.Id"]}";
            Title = workItem.Fields["System.Title"] as string ?? string.Empty;
            Description = (workItem.Fields["System.Description"] as string).HtmlToPlainText().Truncate();
            
            
            Url = $"{baseUri}/{project}/_workitems/edit/{workItem.Fields["System.Id"]}";
            
            Date = workItem.Fields["System.ChangedDate"] as DateTime?;
            if (workItem.Fields.ContainsKey("Microsoft.VSTS.Common.BacklogPriority"))
            {
                BacklogPriority = workItem.Fields["Microsoft.VSTS.Common.BacklogPriority"] as double?;
            }

            switch (ItemType)
            {
                case "PBI":
                case "Product Backlog Item":
                    //https://docs.microsoft.com/en-us/visualstudio/extensibility/ux-guidelines/images-and-icons-for-visual-studio?view=vs-2019#visual-studio-languages
                    Color = Color.FromRgb(0, 120, 215);
                    ItemTypePriority = 4;
                    ItemType = "PBI";
                    break;
                case "Bug":
                    Color = Color.FromRgb(224, 76, 6);
                    ItemTypePriority = 4;
                    ItemType = "Bug";
                    break;
                case "Fire":
                    Color = Color.FromRgb(189, 30, 45);
                    ItemTypePriority = 1;
                    ItemType = "Fire";
                    break;
                default:
                    Color = Colors.CornflowerBlue;
                    ItemTypePriority = 25;
                    ItemType = workItem.Fields["System.WorkItemType"] as string ?? string.Empty;
                    break;
            }
        }

        public string Id { get; }
        public string Title { get; }
        public string? Description { get; set; }
        public DateTime? Date { get; }
        public string Url { get; }
        public string ItemType { get; }
        public Color Color { get; }
        public int ItemTypePriority { get; }
        public double? BacklogPriority { get; }
    }

}