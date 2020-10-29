using Manga.Framework;
using Manga.Servers.Workers;
using Manga.Structures.Models;
using Manga_Trackr.Mediators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    /// <summary>
    /// View-Model for Chapters Page
    /// </summary>
    public class ChaptersViewModel : ObservableObject
    {
        #region Members
        MangaModel _model;
        CollectionViewSource _items;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the Manga Model
        /// </summary>
        public MangaModel Model
        {
            get { return _model; }
        }

        /// <summary>
        /// Gets the Chapter collection of the manga
        /// </summary>
        public System.ComponentModel.ICollectionView Items
        {
            get { return _items.View; }
        }

        public int SelectedIndex
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of ChaptersViewModel object.
        /// </summary>
        /// <param name="model">MangaModel for showing chapters</param>
        public ChaptersViewModel(MangaModel model)
        {
            _model = model;
            _items = new CollectionViewSource();
            _items.Source = Model.Items;
        }

        #endregion

        #region Dispose

        #endregion

        #region Commands

        #region Download Command

        void Download()
        {
            DownloadWorker worker;
            if (SelectedIndex == -1)
                worker = new DownloadWorker(_model, true);
            else
                worker = new DownloadWorker(_model);
            DownloadsMediator.Instance.Add(worker.Model);
            worker.Start();
        }

        bool CanDownload()
        {
            return (IsConnectedToInternet()) && (_model.Items.Count != 0);
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        public ICommand DownloadCommand { get { return new RelayCommand(Download, CanDownload); } }

        #endregion

        #region Favourite Command

        void Favourite()
        {
            if (!Model.IsFavourite)
            {
                FavouriteWorker worker = new FavouriteWorker(Model);
                worker.FinishedEvent += () =>
                { FavouritesMediator.Instance.Refresh(); };
                worker.Start();
            }
            /*else
                Manga.Database.DatabaseEngine.Instance.RemoveFavourite(SelectedItem);*/
        }

        public ICommand FavouriteCommand { get { return new RelayCommand(Favourite); } }

        #endregion

        #endregion
    }
}
