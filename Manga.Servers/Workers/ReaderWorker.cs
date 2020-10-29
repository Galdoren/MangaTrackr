using Manga.Framework;
using Manga.Interfaces;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manga.Servers.Workers
{
    

    public class ReaderWorker : ObservableObject
    {
        #region Members
        bool _isDisposed;
        bool _loading;
        Task _task;
        System.Threading.CancellationTokenSource _cancelToken;
        IServer _server;
        ReaderModel _model;

        AutoResetEvent _signal;
        int _index;
        #endregion

        #region Members

        #region Properties

        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets if current chapter is loading
        /// </summary>
        public bool Loading
        {
            get { return _loading; }
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of Downloadworker object.
        /// </summary>
        /// <param name="model"></param>
        public ReaderWorker(ReaderModel model)
        {
            _model = model;

            Initialize();
        }

        #endregion

        private void Initialize()
        {
            _isDisposed = false;
            _loading = false;
            _server = getPublisher(_model.Publisher);
            _signal = new AutoResetEvent(false);

            _cancelToken = new System.Threading.CancellationTokenSource();

            _task = new Task(() => TaskWork(_model));
        }

        public void Start()
        {
            _task.Start();
        }

        public void Stop()
        {
            _cancelToken.Cancel();
        }

        public void Load(int index)
        {
            _index = index;
            _signal.Set();
        }

        public void Cancel()
        {
            if (_loading == false)
                return;
            _cancelToken.Cancel();
            Initialize();
            Start();
        }

        private void TaskWork(ReaderModel model)
        {
            try
            {
                while (!_cancelToken.Token.IsCancellationRequested)
                {
                    _signal.WaitOne();
                    _loading = true;
                    Database.DatabaseEngine.Instance.LoadChapters(model);
                    _server.GetInfoMinimal(model, _task.IsCanceled);

                    _server.FindImages(model.Items[_index], _task.IsCanceled);
                    _loading = false;
                    _signal.Reset();
                }
            }
            catch (AggregateException e)
            {
                _signal.Reset();
            }
        }

        private IServer getPublisher(Publisher pub)
        {
            IServer item;
            switch (pub)
            {
                case Publisher.Mangafox:
                    item = new Mangafox();
                    break;
                case Publisher.Mangareader:
                    item = new Mangareader();
                    break;
                case Publisher.Mangapanda:
                    item = new Mangapanda();
                    break;
                default:
                    item = null;
                    break;
            }
            return item;
        }

    }
}
