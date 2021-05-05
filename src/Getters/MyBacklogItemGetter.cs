using System.Collections.Generic;
using System.Threading.Tasks;
using ADOTodo.Models;
using ADOTodo.Services;

namespace ADOTodo.Getters
{
    public class MyBacklogItemGetter : IGetter
    {
        public async Task<List<ITodoItem>> GetAsync(Settings settings, AdoService adoService)
        {
            await adoService.GetCurrentSprintItems();
            return new List<ITodoItem>();
        }
    }
}