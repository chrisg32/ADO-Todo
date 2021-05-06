using System.Collections.Generic;
using System.Threading.Tasks;
using ADOTodo.Models;
using ADOTodo.Services;

namespace ADOTodo.Getters
{
    public interface IGetter
    {
        public int RunOrder { get; }
        public Task<List<ITodoItem>> GetAsync(Settings settings, AdoService adoService);
    }
}