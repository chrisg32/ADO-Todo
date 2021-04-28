using System.Runtime.CompilerServices;
using ReactiveUI;

namespace ADOTodo.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected void SetProperty<TValue>(ref TValue property, TValue value, [CallerMemberName] string name = null)
        {
            property = value;
            this.RaisePropertyChanged(name);
        }
    }
}