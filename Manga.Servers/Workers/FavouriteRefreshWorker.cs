using Manga.Framework;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Servers.Workers
{
    public class FavouriteRefreshWorker : ObservableObject
    {
        #region Members

        bool _isDisposed;
        bool _initializeRequired;
        Task _task;
        System.Threading.CancellationTokenSource _cancelTokenSource;
        System.Threading.CancellationToken _cancelToken;

        ThreadSafeObservableCollection<MangaModel> _model;

        #endregion

        #region Properties

        public bool IsWorking
        {
            get { return _task.Status == TaskStatus.Running; }
        }

        public bool IsCompleted
        {
            get { return _task.IsCompleted; }
        }

        #endregion

        #region Constructor

        public FavouriteRefreshWorker(ThreadSafeObservableCollection<MangaModel> collection)
        {
            _model = collection;

            Initialize();
        }

        private void Initialize()
        {
            _isDisposed = false;
            _initializeRequired = false;

            InitializeTask();
        }

        private void InitializeTask()
        {
            _cancelTokenSource = new System.Threading.CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            _task = new Task(() => TaskWork(_model),
                _cancelToken);
        }

        #endregion

        #region Methods

        public void Start()
        {
            if (_initializeRequired)
                InitializeTask();
            _task.Start();
        }

        private void TaskWork(ThreadSafeObservableCollection<MangaModel> collection)
        {
            Manga.Database.DatabaseEngine.Instance.LoadFavourites(collection);
            _initializeRequired = true;
        }

        #endregion
    }
}
