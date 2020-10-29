using Manga.Framework;
using Manga_Trackr.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    /// <summary>
    /// Main Window ViewModel
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        #region members

        CatalogPage _catalogPage;
        FavouritesPage _libraryPage;
        DownloadsPage _downloadsPage;
        UpdatesPage _updatesPage;

        int _selectedIndex;
        int _oldIndex;
        String _transitionDirection;

        #endregion

        /// <summary>
        /// Tab Items of TabControl
        /// </summary>
        public TabItemData[] Tabs { get; set; }

        public String SelectedHeader
        {
            get 
            {
                if (_selectedIndex > -1)
                    return Tabs[_selectedIndex].Header;
                else
                    return String.Empty;
            }

        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                if (_selectedIndex != _oldIndex)
                {
                    RaisePropertyChanged("TransitionDirection");
                }
                RaisePropertyChanged("SelectedIndex");
                RaisePropertyChanged("SelectedHeader");
            }
        }

        public String TransitionDirection
        {
            get
            {
                if (_selectedIndex < _oldIndex)
                    _transitionDirection = "RightTransition";
                else
                    _transitionDirection = "LeftTransition";
                _oldIndex = _selectedIndex;
                return _transitionDirection;
            }
        }

        #region Constructor

        public MainViewModel()
        {
            Tabs = new TabItemData[4];

            _libraryPage = new FavouritesPage();
            _catalogPage = new CatalogPage();
            _downloadsPage = new DownloadsPage();
            _updatesPage = new UpdatesPage();

            Tabs[0] = new TabItemData() { Header = "Library", Content = _libraryPage, ResourceKey = "Lib" };
            Tabs[1] = new TabItemData() { Header = "Catalog", Content = _catalogPage, ResourceKey = "Cat" };
            Tabs[2] = new TabItemData() { Header = "Download Queue", Content = _downloadsPage, ResourceKey = "DownQ" };
            Tabs[3] = new TabItemData() { Header = "Updates", Content = _updatesPage, ResourceKey = "Updt" };
        }

        #endregion

        #region Commands

        #region Open Settings Command

        void OpenSettings()
        {
            SettingsWindow window = new SettingsWindow();
            Manga_Trackr.Mediators.SettingsMediator.Instance.RegisterWindow(window);
            window.ShowDialog();
        }

        public ICommand SettingsCommand { get { return new RelayCommand(OpenSettings); } }

        #endregion        

        #endregion
    }

    public class TabItemData
    {
        public String Header { get; set; }
        public String ResourceKey { get; set; }
        public object Content { get; set; }
    }
}
