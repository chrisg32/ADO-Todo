using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADOTodo.Models;
using ADOTodo.Services;

namespace ADOTodo.Getters
{
    public class WorkItemGetter : IGetter
    {
        public int RunOrder => 1;

        public async Task<List<ITodoItem>> GetAsync(Settings settings, AdoService adoService)
        {
            const string query = @"SELECT
[System.Id]
FROM workitems
WHERE (([System.WorkItemType] IN ('Product BackLog Item','Bug', 'Fire') AND [System.AssignedTo] = @Me)
OR ([System.WorkItemType] = 'Fire' and [System.AssignedTo] = ''))
AND [System.State] IN ('New', 'Committed')
AND [System.IterationPath] = @CurrentIteration";
            
            var workItems = await adoService.QueryWorkItems(query);
            var todoItems =  workItems.Select(wi => new WorkItemTodoItem(wi, adoService.Uri, settings.Project)).OrderBy(i => i.ItemTypePriority).ThenBy(i => i.BacklogPriority).Cast<ITodoItem>().ToList();
            return todoItems;
        }
    }

}