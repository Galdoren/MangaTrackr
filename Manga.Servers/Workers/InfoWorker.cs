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
    public class InfoWorker : ObservableObject, IDisposable
    {
        #region Members
        MangaModel _model;
        Task _task;
        bool _isDisposed;
        System.Threading.CancellationTokenSource _cancelToken;
        IServer _server;
        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Instance of InfoWorker object.
        /// </summary>
        /// <param name="model">Model to update information.</param>
        /// <param name="type">Minimal or Extended.</param>
        public InfoWorker(MangaModel model, InfoType type)
        {
            this._model = model;

            Initialize(type);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts Enumeration to Server object
        /// </summary>
        /// <param name="pub">Publisher object</param>
        /// <returns>Returns a new Instance of Server object</returns>
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

        #region Initialize

        private void Initialize(InfoType type)
        {
            _isDisposed = false;
            _server = getPublisher(_model.Publisher);
            
            _cancelToken = new System.Threading.CancellationTokenSource();

            if (type == InfoType.Minimal)
                _task = new Task(() => TaskWorkMinimal(_model),
                    _cancelToken.Token, TaskCreationOptions.LongRunning);
            else
                _task = new Task(() => TaskWorkExtended(_model),
                    _cancelToken.Token, TaskCreationOptions.LongRunning);

            try
            {
                _task.Start(TaskScheduler.Default);
            }
            catch (AggregateException e)
            { Console.WriteLine(e.Message); }
        }

        #endregion

        private void TaskWorkMinimal(MangaModel model)
        {
            lock (model)
            {
                _server.GetInfoMinimal(model, _task.IsCanceled);
                Manga.Database.DatabaseEngine.Instance.Update(model);
            }
        }

        private void TaskWorkExtended(MangaModel model)
        {
            lock (model)
            {
                _server.GetInfoExtended(model, _task.IsCanceled);
                Manga.Database.DatabaseEngine.Instance.Update(model);
            }
        }

        #endregion

        #region Destructor & Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _server = null;
                }
            }
            _isDisposed = true;
        }

        #endregion
    }

    public enum InfoType
    {
        Minimal,
        Extended
    };
}
