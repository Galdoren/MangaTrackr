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
    public class CatalogWorker : IDisposable
    {
        #region Members
        bool _isDisposed;
        bool _initializeRequired;
        Task _task;
        System.Threading.CancellationTokenSource _cancelTokenSource;
        System.Threading.CancellationToken _cancelToken;

        IServer _server;
        ThreadSafeObservableCollection<MangaModel> _collection;
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

        public CatalogWorker(ThreadSafeObservableCollection<MangaModel> collection, Publisher publisher)
        {
            _collection = collection;
            Initialize(publisher);
        }

        private void Initialize(Publisher publisher)
        {
            _isDisposed = false;
            _initializeRequired = false;                       

            InitializeTask(publisher);            
        }

        private void InitializeTask(Publisher publisher)
        {
            _server = GetPublisher(publisher); 

            _cancelTokenSource = new System.Threading.CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;            

            _task = new Task(() => TaskWork(_collection),
                _cancelToken);

            _task.ContinueWith((t) =>
            {
                Database.DatabaseEngine.Instance.AddRange(_collection, publisher, _cancelToken);
            }, _cancelToken, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
        }

        public void Start()
        {            
            _task.Start();
        }

        public void Start(Publisher publisher)
        {
            if (_initializeRequired)
                InitializeTask(publisher);
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

        private void TaskWork(ICollection<MangaModel> collection)
        {
            try
            {
                lock (collection)
                {
                    _server.FetchList(collection, _cancelToken);
                }
            }
            catch (AggregateException)
            {

            }
            _initializeRequired = true;
        }

        public IServer GetPublisher(Publisher publisher)
        {
            IServer item;
            switch (publisher)
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

        public void Dispose()
        {
            _cancelTokenSource.Cancel();
            _task.Wait();

            _cancelTokenSource.Dispose();
            _task.Dispose();
            _server = null;
        }
    }
}
