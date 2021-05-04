using System.Collections.Generic;
using ADOTodo.Models;

namespace ADOTodo.Util
{
    internal sealed class PrItemTodoItemEqualityComparer : IEqualityComparer<PrThreadTodoItem>
    {
        public bool Equals(PrThreadTodoItem x, PrThreadTodoItem y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Equals(y);
        }
    
        public int GetHashCode(PrThreadTodoItem obj)
        {
            return obj.GetHashCode();
        }
    }
}