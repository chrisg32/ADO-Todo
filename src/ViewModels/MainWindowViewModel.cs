using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CG.Commons.Extensions;
using ADOTodo.Models;
using ADOTodo.Services;
using ADOTodo.Util;
using ADOTodo.Views;
using ReactiveUI;

namespace ADOTodo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly Timer _timer;
        private readonly Settings _settings;
        private const string SettingsFileName = "adolist.dat";

        private AdoService? _adoService;
        public ObservableCollection<ITodoItem> TodoItems { get; } = new ObservableCollection<ITodoItem>();

        public MainWindowViewModel()
        {
            PersistenceModule.SafeRename<Settings>("prlist.dat", SettingsFileName);
            _settings = PersistenceModule.Load<Settings>(SettingsFileName);
            _timer = new Timer(async e => await Poll(e), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            SetServerCommand = ReactiveCommand.Create(SetServer);
            SetProjectCommand = ReactiveCommand.Create(SetProject);
            SetPATCommand = ReactiveCommand.Create(SetPAT);
        }

        private async Task SetServer()
        {
            var result = await TextEntryDialog.Show("Set Server", "Please enter the server URL e.g. https://dev.azure.com/my-company");
            if(string.IsNullOrWhiteSpace(result)) return;
            _settings.Server = result.Trim();
            if (ValidateCredentials())
            {
                PersistenceModule.Save(SettingsFileName, _settings);
            }
        }
        
        private async Task SetProject()
        {
            var result = await TextEntryDialog.Show("Set Project", "Please enter a project name.");
            if(string.IsNullOrWhiteSpace(result)) return;
            _settings.Project = result.Trim();
            if (ValidateCredentials())
            {
                PersistenceModule.Save(SettingsFileName, _settings);
            }
        }
        
        private async Task SetPAT()
        {
            var result = await TextEntryDialog.Show("Set PAT", "Please enter a Personal Access Token.");
            if(string.IsNullOrWhiteSpace(result)) return;
            _settings.Token = result.Trim();
            if (ValidateCredentials())
            {
                PersistenceModule.Save(SettingsFileName, _settings);
            }
        }

        private async Task Poll(object? state)
        {
            //check for PAT and project
            if (!ValidateCredentials()) return;
            
            _adoService ??= new AdoService(_settings);
            
            var todos = new HashSet<PrThreadTodoItem>(PrThreadTodoItem.EqualityComparer);

            if (_mine != ThreadFilterLevel.None)
            {
                var pullRequests = await _adoService.GetAllPullRequestsOpenedByMe();
                switch (_mine)
                {
                    case ThreadFilterLevel.Mentions:
                        todos.AddRange(pullRequests.SelectMany(pr => pr.Threads.Where(thread => thread.MentionsUser(_adoService.UserId)).Select(t => new PrThreadTodoItem(pr, t, _adoService.Uri, _settings.Project))));
                        break;
                    case ThreadFilterLevel.Comments:
                        throw new NotImplementedException();
                        break;
                    case ThreadFilterLevel.Threads:
                        throw new NotImplementedException();
                        break;
                    case ThreadFilterLevel.All:
                        todos.AddRange(pullRequests.SelectMany(pr => pr.Threads.Select(t => new PrThreadTodoItem(pr, t, _adoService.Uri, _settings.Project))));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var userIds = todos.SelectMany(t => t.GetMentions()).Distinct();
            var userMap = new Dictionary<Guid, string?>();
            foreach (var id in userIds)
            {
                var displayName = await _adoService.GetUserName(id);
                userMap.Add(id, displayName);
            }

            foreach (var todo in todos)
            {
                todo.UpdateDescriptionTextWithMentions(userMap);
            }
            
            TodoItems.Clear();
            TodoItems.AddRange(todos.OrderBy(t => t.PrId));
            RefreshedDate = DateTime.Now;
        }

        private bool ValidateCredentials()
        {
            if (string.IsNullOrEmpty(_settings.Server))
            {
                ErrorMessage = "A server is required.";
                return false;
            }
            if (string.IsNullOrEmpty(_settings.Project))
            {
                ErrorMessage = "A project is required.";
                return false;
            }
            if (string.IsNullOrEmpty(_settings.Token))
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
        
        //Mine
        private ThreadFilterLevel _mine = ThreadFilterLevel.All;

        private bool _mineAllOpenComments = true;
        public bool MineAllOpenComments
        {
            get => _mineAllOpenComments;
            set
            {
                _mineAllOpenComments = value;
                if(value) _mine = ThreadFilterLevel.All;
            }
        }

        private bool _mineAllOpenCommentsOnThreadsIStarted;
        public bool MineAllOpenCommentsOnThreadsIStarted
        {
            get => _mineAllOpenCommentsOnThreadsIStarted;
            set
            {
                _mineAllOpenCommentsOnThreadsIStarted = value;
                if(value) _mine = ThreadFilterLevel.Threads;
            }
        }

        private bool _mineAllOpenCommentsOnThreadsICommentedOn;
        public bool MineAllOpenCommentsOnThreadsICommentedOn
        {
            get => _mineAllOpenCommentsOnThreadsICommentedOn;
            set
            {
                _mineAllOpenCommentsOnThreadsICommentedOn = value;
                if(value) _mine = ThreadFilterLevel.Comments;
            }
        }

        private bool _mineAllOpenCommentsOnThreadsMentioningMe;
        public bool MineAllOpenCommentsOnThreadsMentioningMe
        {
            get => _mineAllOpenCommentsOnThreadsMentioningMe;
            set
            {
                _mineAllOpenCommentsOnThreadsMentioningMe = value;
                if(value) _mine = ThreadFilterLevel.Mentions;
            }
        }

        private bool _mineNone;
        private DateTime? _refreshedDate;

        public bool MineNone
        {
            get => _mineNone;
            set
            {
                _mineNone = value;
                if(value) _mine = ThreadFilterLevel.None;
            }
        }
        
        //Other
        public bool OtherAllOpenComments { get; set; }
        public bool OtherAllOpenCommentsOnThreadsIStarted { get; set; }
        public bool OtherAllOpenCommentsOnThreadsICommentedOn { get; set; }
        public bool OtherAllOpenCommentsOnThreadsMentioningMe { get; set; } = true;
        public bool OtherNone { get; set; }
        
        //Comprehensive
        public bool ComprehensiveAllOpenComments { get; set; }
        public bool ComprehensiveAllOpenCommentsOnThreadsIStarted { get; set; }
        public bool ComprehensiveAllOpenCommentsOnThreadsICommentedOn { get; set; } = true;
        public bool ComprehensiveAllOpenCommentsOnThreadsMentioningMe { get; set; }
        public bool ComprehensiveNone { get; set; }

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