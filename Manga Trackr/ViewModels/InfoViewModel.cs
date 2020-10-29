using Manga.Framework;
using Manga.Servers.Workers;
using Manga.Structures.Models;
using Manga_Trackr.Mediators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    /// <summary>
    /// View Model for Information Tab of the Information Windows'
    /// </summary>
    public class InfoViewModel : ObservableObject
    {
        #region Members

        MangaModel _model;

        String _description;

        #endregion

        #region Properties

        public MangaModel Model
        {
            get { return _model; }
        }

        public String Description
        {
            get { return _description; }
            private set { _description = _model.Description; RaisePropertyChanged("Description"); }
        }

        #endregion

        #region Constructor

        public InfoViewModel(MangaModel model)
        {
            _model = model;
            _description = _model.Description;
        }

        #endregion

        #region Commands

        #region Download Command

        void Download()
        {
            DownloadWorker worker = new DownloadWorker(_model, true);
            DownloadsMediator.Instance.Add(worker.Model);
            worker.Start();
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        bool CanDownload()
        {
            return IsConnectedToInternet();
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
