using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ADOTodo.Getters;
using ADOTodo.Models;
using ADOTodo.Services;
using ADOTodo.Views;
using CG.Commons.Extensions;
using ReactiveUI;

namespace ADOTodo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly Timer _timer;
        public Settings Settings { get; }
        private const string SettingsFileName = "adolist.dat";

        private AdoService? _adoService;
        public ObservableCollection<ITodoItem> TodoItems { get; } = new ObservableCollection<ITodoItem>();

        private readonly List<IGetter> _getters;

        public MainWindowViewModel()
        {
            PersistenceModule.SafeRename<Settings>("prlist.dat", SettingsFileName);
            Settings = PersistenceModule.Load<Settings>(SettingsFileName);
            _timer = new Timer(async e => await Poll(e), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            SetServerCommand = ReactiveCommand.Create(SetServer);
            SetProjectCommand = ReactiveCommand.Create(SetProject);
            SetPATCommand = ReactiveCommand.Create(SetPAT);

            _getters = GetType().Assembly.GetTypes()
                .Where(t => typeof(IGetter).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(Activator.CreateInstance).Where(o => o != null).Cast<IGetter>().ToList();
        }

        private async Task SetServer()
        {
            var result = await TextEntryDialog.Show("Set Server", "Please enter the server URL e.g. https://dev.azure.com/my-company");
            if(string.IsNullOrWhiteSpace(result)) return;
            Settings.Server = result.Trim();
            if (ValidateCredentials())
            {
                PersistenceModule.Save(SettingsFileName, Settings);
            }
        }
        
        private async Task SetProject()
        {
            var result = await TextEntryDialog.Show("Set Project", "Please enter a project name.");
            if(string.IsNullOrWhiteSpace(result)) return;
            Settings.Project = result.Trim();
            if (ValidateCredentials())
            {
                PersistenceModule.Save(SettingsFileName, Settings);
            }
        }
        
        private async Task SetPAT()
        {
            var result = await TextEntryDialog.Show("Set PAT", "Please enter a Personal Access Token.");
            if(string.IsNullOrWhiteSpace(result)) return;
            Settings.Token = result.Trim();
            if (ValidateCredentials())
            {
                PersistenceModule.Save(SettingsFileName, Settings);
            }
        }

        private async Task Poll(object? state)
        {
            //check for PAT and project
            if (!ValidateCredentials()) return;
            
            _adoService ??= new AdoService(Settings);

            var todos = new List<ITodoItem>();

            foreach (var getter in _getters.OrderBy(g => g.RunOrder))
            {
                todos.AddRange(await getter.GetAsync(Settings, _adoService));
            }
            
            TodoItems.Clear();
            TodoItems.AddRange(todos.OrderBy(t => t.ItemTypePriority));
            RefreshedDate = DateTime.Now;
        }

        private bool ValidateCredentials()
        {
            if (string.IsNullOrEmpty(Settings.Server))
            {
                ErrorMessage = "A server is required.";
                return false;
            }
            if (string.IsNullOrEmpty(Settings.Project))
            {
                ErrorMessage = "A project is required.";
                return false;
            }
            if (string.IsNullOrEmpty(Settings.Token))
            {
                ErrorMessage = "A PAT is required.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        public void Dispose()
        {
            _timer.Dispose();
            _adoService?.Dispose();
        }
        
        private DateTime? _refreshedDate;

        public ITodoItem? SelectedItem
        {
            get => null;
            set
            {
                if (value == null) return;
                OpenBrowser(value.Url);
            }
        }

        public DateTime? RefreshedDate
        {
            get => _refreshedDate;
            set => SetProperty(ref _refreshedDate, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
        
        public ICommand SetServerCommand { get; }
        public ICommand SetProjectCommand { get; }
        public ICommand SetPATCommand { get; }
    }
}