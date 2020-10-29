using Manga.Framework;
using Manga.Interfaces;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Servers.Workers
{
    public class UpdateWorker : ObservableObject
    {
        #region Members
        Task _task;
        bool _isDisposed;

        bool _initializeRequired;

        Dictionary<Publisher, IServer> _servers;

        System.Threading.CancellationTokenSource _cancelTokenSource;
        System.Threading.CancellationToken _cancelToken;

        ThreadSafeObservableCollection<MangaModel> _model;
        #endregion

        #region Properties

        public bool IsRunning
        { get { return _task.Status == TaskStatus.Running; } }

        #endregion

        #region Constructor

        public UpdateWorker(ThreadSafeObservableCollection<MangaModel> model)
        {
            _model = model;

            Initialize();
        }

        private void Initialize()
        {            
            _servers = new Dictionary<Publisher, IServer>();
            _servers.Add(Publisher.Mangafox, new Mangafox());
            _servers.Add(Publisher.Mangareader, new Mangareader());
            _servers.Add(Publisher.Mangapanda, new Mangapanda());

            InitializeTask();
        }

        private void InitializeTask()
        {
            _cancelTokenSource = new System.Threading.CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            _task = new Task(() => TaskWork(_model),
                _cancelToken);

            _initializeRequired = false;
        }

        #endregion

        #region Methods

        void TaskWork(ThreadSafeObservableCollection<MangaModel> model)
        {
            for (int i = 0; i < model.Count; i++)
            {
                if (_cancelToken.IsCancellationRequested)
                    break;

                _servers[model[i].Publisher].CheckUpdates(model[i], _cancelToken);
            }
            _initializeRequired = true;
        }

        public void Start()
        {
            if (_initializeRequired)
                InitializeTask();
            _task.Start();
        }

        public void Cancel()
        {
            _cancelTokenSource.Cancel();
            _initializeRequired = true;
        }

        public void Wait()
        {
            while (true)
            {
                if (_task.IsCanceled || _task.IsCompleted)
                    break;
            }
            _task.Dispose();
        }

        #endregion
    }
}
