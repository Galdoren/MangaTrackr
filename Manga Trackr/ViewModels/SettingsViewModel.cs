using Manga.Framework;
using Manga.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        #region Members

        String _downloadPath;
        String _olddownloadPath;

        String _interval;
        String _oldInterval;

        bool _changed;

        bool _isNotDownloading;

        Manga.Themes.myThemes _selectedTheme;
        Manga.Themes.myThemes _oldselectedTheme;

        System.Windows.Threading.DispatcherTimer _timer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the Download Path of the Manga Tracker
        /// </summary>
        public String DownloadPath
        {
            get { return _downloadPath; }
            set
            {
                _downloadPath = value;
                RaisePropertyChanged("DownloadPath");
            }
        }

        public String Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                RaisePropertyChanged("Interval");
            }
        }

        public Manga.Themes.myThemes SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                _selectedTheme = value;
                RaisePropertyChanged("SelectedTheme");
            }
        }

        public bool IsNotDownloading
        {
            get { return _isNotDownloading; }
            private set
            {
                _isNotDownloading = value;
                RaisePropertyChanged("IsNotDownloading");
            }
        }

        #endregion

        #region Constructor

        public SettingsViewModel()
        {
            Initialize();
        }

        ~SettingsViewModel()
        {
            _timer.Stop();
        }

        void Initialize()
        {
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 5);
            _timer.Tick += new EventHandler(checkDownloadState);

            Interval = Manga.Structures.Properties.Settings.Default.UpdateInterval;
            _oldInterval = _interval;

            _changed = false;
            _isNotDownloading = Manga.Structures.Properties.Settings.Default.DownloadCount == 0;

            _downloadPath = Manga.Structures.Properties.Settings.Default.DownloadPath;
            _olddownloadPath = _downloadPath;

            _selectedTheme = (Manga.Themes.myThemes)Enum.Parse(typeof(Manga.Themes.myThemes), Manga.Structures.Properties.Settings.Default.SelectedTheme);
            _oldselectedTheme = _selectedTheme;

            _timer.Start();
        }

        #endregion

        #region Commands

        #region Apply Command

        void Apply()
        {
            Manga.Structures.Properties.Settings.Default.DownloadPath = _downloadPath;
            Manga.Structures.Properties.Settings.Default.SelectedTheme = _selectedTheme.ToString();
            Manga.Structures.Properties.Settings.Default.UpdateInterval = _interval;
            Manga.Structures.Properties.Settings.Default.Save();
            _changed = false;
            _olddownloadPath = _downloadPath;
            if (_oldselectedTheme != _selectedTheme)
                Manga.Themes.ThemeManager.Instance.ChangeTheme(_selectedTheme);
            _oldselectedTheme = _selectedTheme;

            _oldInterval = _interval;
        }

        bool CanApply()
        {
            if ((_downloadPath != _olddownloadPath) || 
                (_selectedTheme != _oldselectedTheme) ||
                (_interval != _oldInterval) )
                _changed = true;
            else
                _changed = false;
            return _changed;
        }

        public ICommand ApplyCommand { get { return new RelayCommand(Apply, CanApply); } }

        #endregion

        #region Browse Command

        void Browse()
        {
            // TODO
        }

        public ICommand BrowseCommand { get { return new RelayCommand(Browse); } }

        #endregion

        #region Ok Command

        #region Ok Command

        void Ok()
        {
            if (CanApply())
            {
                Apply();
            }
            else
            {

            }
        }

        public ICommand OkCommand { get { return new RelayCommand(Ok); } }

        #endregion

        #region Cancel Command

        void Cancel()
        {
            Manga_Trackr.Mediators.SettingsMediator.Instance.Close();
        }

        public ICommand CancelCommand { get { return new RelayCommand(Cancel); } }  

        #endregion

        #endregion

        #endregion

        #region Methods

        void checkDownloadState(object state, EventArgs e)
        {
            IsNotDownloading = Manga.Structures.Properties.Settings.Default.DownloadCount == 0;
        }

        #endregion
    }
}
