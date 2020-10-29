using Manga.Interfaces;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manga.Framework;

namespace Manga.Servers.Workers
{
    public class FavouriteWorker : ObservableObject
    {
        #region Members
        bool _isDisposed;
        Task _task;
        System.Threading.CancellationTokenSource _cancelToken;
        IServer _server;
        MangaModel _model;
        #endregion

        #region Properties

        public WorkerFinishedEventHandler FinishedEvent;

        #endregion

        #region Constructor

        public FavouriteWorker(MangaModel model)
        {
            _model = model;

            Initialize();
        }

        private void Initialize()
        {
            _isDisposed = false;

            _server = getPublisher(_model.Publisher);

            FinishedEvent = new Framework.WorkerFinishedEventHandler(workFinished);

            _cancelToken = new System.Threading.CancellationTokenSource();

            if (_model.InfoState != 2)
            {
                _task = new Task(() =>
                    {
                        _server.GetInfoExtended(_model, _task.IsCanceled);
                    });
                _task.ContinueWith((t) =>
                {
                    TaskWork(_model);
                    FinishedEvent.Invoke();
                });
            }

            else
            {
                _task = new Task(() =>
                    {
                        TaskWork(_model);
                        FinishedEvent.Invoke();
                    });
            }
        }

        public void Start()
        {
            _task.Start();
        }

        private void TaskWork(MangaModel model)
        {
            Database.DatabaseEngine.Instance.AddFavourite(model);
            model.IsFavourite = true;
        }

        private void workFinished()
        { }


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

        #endregion
    }
}
