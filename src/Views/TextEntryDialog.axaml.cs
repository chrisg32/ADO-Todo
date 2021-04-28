using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace ADOTodo.Views
{
    public class TextEntryDialog : Window
    {
        public TextEntryDialog()
        {
            InitializeComponent();
            DoneCommand = ReactiveCommand.Create<string>(OnDone);
            DataContext = this;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void OnDone(string param)
        {
            var boolVal = bool.Parse(param);
            Close(boolVal ? Value : null);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private string? Value { get; set; }
        private string Hint { get; set; }

        private ICommand DoneCommand { get; }
        
        public static async Task<string?> Show(string title, string text)
        {
            var dialog = new TextEntryDialog
            {
                Title = title,
                Hint = text
            };
            return await dialog.ShowDialog<string?>(MainWindow.Instance);
        }
    }
}