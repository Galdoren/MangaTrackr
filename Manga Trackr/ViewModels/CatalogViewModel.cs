using Manga.Framework;
using Manga.Interfaces;
using Manga.Servers.Workers;
using Manga.Structures.Models;
using Manga_Trackr.Mediators;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    public sealed class CatalogViewModel : ObservableObject
    {
        #region Members

        CollectionViewSource _itemsSource;
        ThreadSafeObservableCollection<MangaModel> _items;
        MangaModel _selectedItem;

        List<String> _pages;

        String _searchText;

        bool _isLoading;

        Publisher _selectedPublisher;
        Publisher _oldPublisher;

        CatalogWorker _worker;

        bool _clearList;

        #endregion

        #region Properties

        /// <summary>
        /// The thread safe Collection to hold the selected publisher's catalog
        /// </summary>
        public ThreadSafeObservableCollection<MangaModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        /// <summary>
        /// The Viewsource for listbox or other views which supports binding viewsource
        /// </summary>
        public CollectionViewSource ItemsSource
        {
            get
            {
                // Check if source is connected to the Items collection
                if (_itemsSource.Source != Items)
                    _itemsSource.Source = Items;
                return _itemsSource;
            }
            set
            {
                _itemsSource = value;
                RaisePropertyChanged("ItemsSource");
            }
        }

        /// <summary>
        /// Hold the selected item from the catalog, hold the information about manga, chapters and publisher
        /// </summary>
        public MangaModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        /// <summary>
        /// The string to hold the search query
        /// </summary>
        public String SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged("SearchText");
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = true; RaisePropertyChanged("IsLoading"); }
        }

        /// <summary>
        /// Gets or Sets the Selected Publisher
        /// </summary>
        public Publisher SelectedPublisher
        {
            get { return _selectedPublisher; }
            set
            {
                if (_selectedPublisher != value)
                    _clearList = true;
                _selectedPublisher = value;
                RaisePropertyChanged("SelectedPublisher");
                Refresh();
                _clearList = false;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// View Model for Catalog Page
        /// </summary>
        public CatalogViewModel()
        {
            _isLoading = true;
            _clearList = false;
            
            _pages = new List<String>();
            _items = new ThreadSafeObservableCollection<MangaModel>();

            _selectedPublisher = Publisher.Mangafox;
            _oldPublisher = _selectedPublisher;

            Refresh();
            
            
            _itemsSource = new CollectionViewSource();
            _itemsSource.Filter += new FilterEventHandler(ItemsSource_Filter);

            _itemsSource.Source = Items;
        }

        #endregion

        #region Search Filter

        /// <summary>
        /// Search Filter for Catalog which matches the search query with the catalog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Provides information and event data that is associated 
        /// with the System.Windows.Data.CollectionViewSource.Filter event</param>
        void ItemsSource_Filter(object sender, FilterEventArgs e)
        {
            if (String.IsNullOrEmpty(SearchText))
                e.Accepted = true;
            else
            {
                MangaModel model = e.Item as MangaModel;
                if (model.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    e.Accepted = true;
                else
                    e.Accepted = false;
            }
        }

        #endregion

        #region Commands

        #region Open Command

        /// <summary>
        /// Opens the Detailed Information Window for reading extended information about Manga and selecting specific chapters
        /// </summary>
        void OpenInformationWindow()
        {
            InfoWindow window = new InfoWindow(SelectedItem);
            window.Show();
            // TODO
        }

        /// <summary>
        /// Checks if an item is selected from the catalog
        /// </summary>
        /// <returns></returns>
        bool CanOpenInformationWindow()
        {
            if (SelectedItem == null)
                return false;
            return true;
        }

        /// <summary>
        /// The open command is used to open the detailed information window
        /// </summary>
        public ICommand OpenCommand 
        { 
            get 
            {
                return new RelayCommand(OpenInformationWindow, CanOpenInformationWindow);
            }
        }

        #endregion

        #region Download Command

        /// <summary>
        /// Downloads the selected Manga-Chapter
        /// </summary>
        void Download()
        {
            DownloadWorker worker = new DownloadWorker(SelectedItem, true);
            DownloadsMediator.Instance.Add(worker.Model);
            worker.Start();
            // TODO
        }

        /// <summary>
        /// Checks if an item is selected from catalog or not
        /// </summary>
        /// <returns>Boolean</returns>
        bool CanDownload()
        {
            return (SelectedItem != null) && IsConnectedToInternet();
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        /// <summary>
        /// Download Command is used for starting the process for downloading selected Manga-Chapter
        /// </summary>
        public ICommand DownloadCommand { get { return new RelayCommand(Download, CanDownload); } }

        #endregion

        #region Update Command

        /// <summary>
        /// Updates the catalog
        /// </summary>
        void Update()
        {
            // TODO
            Items.Clear();
            CatalogWorker worker = new CatalogWorker(Items, Publisher.Mangareader);
        }

        /// <summary>
        /// Checks if internet is established or not
        /// </summary>
        /// <returns>Boolean</returns>
        bool CanUpdate()
        {
            // Check internet connection
            // TODO
            return true;
        }

        /// <summary>
        /// The update command is used to start the updating the catalog process
        /// </summary>
        public ICommand UpdateCommand { get { return new RelayCommand(Update, CanUpdate); } }

        #endregion

        #region Search Command

        void Search()
        {
            ItemsSource.View.Refresh();
        }

        bool CanSearch()
        {
            if (Items != null)
            {
                if (Items.Count != 0)
                    return true;
            }
            return false;
        }

        public ICommand SearchCommand { get { return new RelayCommand(Search, CanSearch); } }

        #endregion

        #region Refresh Command

        /// <summary>
        /// Refreshes the catalog
        /// </summary>
        void Refresh()
        {
            if (_worker == null)
                _worker = new CatalogWorker(Items, SelectedPublisher);
            else if (_worker.IsWorking)
            {
                _worker.Cancel();
                _worker.Wait();
            }

            if (_clearList)
                Items.Clear();
            Manga.Database.DatabaseEngine.Instance.LoadList(_items, _selectedPublisher);
            _worker.Start(SelectedPublisher);
        }

        bool CanRefresh()
        {
            return true;
        }

        public ICommand RefreshCommand { get { return new RelayCommand(Refresh, CanRefresh); } }

        #endregion

        #region Favourite Command

        void Favourite()
        {
            if (!SelectedItem.IsFavourite)
            {
                FavouriteWorker worker = new FavouriteWorker(SelectedItem);
                worker.FinishedEvent += () =>
                { FavouritesMediator.Instance.Refresh(); };
                worker.Start();
            }
            /*else
                Manga.Database.DatabaseEngine.Instance.RemoveFavourite(SelectedItem);*/
        }

        bool CanFavourite()
        {
            return (SelectedItem != null);
        }

        public ICommand FavouriteCommand { get { return new RelayCommand(Favourite, CanFavourite); } }

        #endregion

        #region Library Command

        void Library()
        {

        }

        public ICommand LibrayCommand { get { return new RelayCommand(Library); } }

        #endregion

        #region Read Command

        void Read()
        {
            Manga.Reader.ReaderWindow window = new Manga.Reader.ReaderWindow(SelectedItem);
            window.Show();
        }

        public ICommand ReadCommand { get { return new RelayCommand(Read); } }

        #endregion

        #endregion
    }
}
