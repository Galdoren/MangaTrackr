using Manga.Framework;
using Manga.Servers.Workers;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    public class UpdatesViewModel : ObservableObject
    {
        #region Members

        ThreadSafeObservableCollection<MangaModel> _favourites;

        CollectionViewSource _itemsSource;
        System.Windows.Threading.DispatcherTimer _timer;

        UpdateWorker _worker;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public UpdatesViewModel()
        {
            _favourites = new ThreadSafeObservableCollection<MangaModel>();
            _worker = new UpdateWorker(_favourites);

            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.Parse(Manga.Structures.Properties.Settings.Default.UpdateInterval);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }        

        #endregion

        #region Timer

        void _timer_Tick(object sender, EventArgs e)
        {
            if (!_worker.IsRunning)
            {
                _favourites.Clear();
                Manga.Database.DatabaseEngine.Instance.LoadFavourites(_favourites);
                _worker.Start();
            }
            _timer.Stop();
            _timer.Interval = TimeSpan.Parse(Manga.Structures.Properties.Settings.Default.UpdateInterval);
            _timer.Start();
        }

        #endregion

        #region Command

        void Refresh()
        {
            _timer.Stop();
            _timer.Interval = TimeSpan.Parse(Manga.Structures.Properties.Settings.Default.UpdateInterval);
            _timer.Start();
        }

        public ICommand RefreshCommand { get { return new RelayCommand(Refresh); } }

        #endregion
    }
}
