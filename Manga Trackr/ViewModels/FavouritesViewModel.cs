using Manga.Framework;
using Manga.Interfaces;
using Manga.Servers.Workers;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    public class FavouritesViewModel : ObservableObject
    {
        #region Members

        FavouriteRefreshWorker _worker;
        ThreadSafeObservableCollection<MangaModel> _items;

        #endregion

        #region Properties

        public ThreadSafeObservableCollection<MangaModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        #endregion

        #region Constructor

        public FavouritesViewModel()
        {
            _items = new ThreadSafeObservableCollection<MangaModel>();
            _worker = new FavouriteRefreshWorker(_items);
            _worker.Start();
        }

        #endregion

        #region Destructor & Dispose

        #endregion

        #region Commands

        #region Open Command

        /// <summary>
        /// Opens the Detailed Information Window for reading extended information about Manga and selecting specific chapters
        /// </summary>
        void OpenInformationWindow(MangaModel model)
        {
            InfoWindow window = new InfoWindow(model);
            window.Show();
            // TODO
        }

        public ICommand OpenCommand
        {
            get { return new RelayCommand<MangaModel>(OpenInformationWindow); }
        }

        #endregion

        #region Refresh Command

        void Refresh()
        {
            if (!_worker.IsWorking)
            {
                _items.Clear();
                _worker.Start();
            }
        }

        public ICommand RefreshCommand { get { return new RelayCommand(Refresh); } }

        #endregion

        #region Remove Command

        void Remove(IMangaModel model)
        {
            Manga.Database.DatabaseEngine.Instance.RemoveFavourite(model);
            ((MangaModel)model).IsFavourite = false;
            Refresh();
        }

        public ICommand RemoveCommand { get { return new RelayCommand<IMangaModel>(Remove); } }

        #endregion

        #endregion
    }
}
