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
    /// <summary>
    /// Download worker class for handling downloading operations
    /// </summary>
    public class DownloadWorker : ObservableObject, IDisposable
    {
        #region Members
        bool _isDisposed;
        Task _task;
        System.Threading.CancellationTokenSource _cancelToken;
        IServer _server;
        DownloadModel _model;
        #endregion

        #region Properties

        public DownloadModel Model
        {
            get { return _model; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of Downloadworker object.
        /// </summary>
        /// <param name="model"></param>
        public DownloadWorker(MangaModel model)
        {
            _model = new DownloadModel();

            _model.Name = model.Name;
            _model.Link = model.Link;
            _model.Publisher = model.Publisher;

            Initialize(model, false);
        }

        public DownloadWorker(MangaModel model, bool downloadAll)
        {
            _model = new DownloadModel();
            
            _model.Name = model.Name;
            _model.Link = model.Link;
            _model.Publisher = model.Publisher;

            Initialize(model, true);
        }
        #endregion

        private void Initialize(MangaModel model, bool DownloadAll)
        {
            _isDisposed = false;
            _server = getPublisher(_model.Publisher);

            _cancelToken = new System.Threading.CancellationTokenSource();

            if (model.InfoState == 0)
            {
                _task = new Task(() => 
                {
                    Manga.Structures.Properties.Settings.Default.DownloadCount++;
                    _model.IsBusy = true;
                    _server.GetInfoMinimal(model, _task.IsCanceled);

                    for (int i = 0; i < model.Items.Count; i++)
                    {
                        DownloadItem item = new DownloadItem();
                        item.Name = model.Items[i].Name;
                        item.Link = model.Items[i].Link;

                        _model.Items.Add(item);
                    }

                    TaskWork(_model);
                    Manga.Structures.Properties.Settings.Default.DownloadCount--;
                });
            }
            else
            {
                _task = new Task(() => 
                {
                    Manga.Structures.Properties.Settings.Default.DownloadCount++;
                    _model.IsBusy = true;
                    if (DownloadAll)
                    {
                        for (int i = 0; i < model.Items.Count; i++)
                        {
                            DownloadItem item = new DownloadItem();
                            item.Name = model.Items[i].Name;
                            item.Link = model.Items[i].Link;

                            _model.Items.Add(item);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < model.Items.Count; i++)
                        {
                            if (model.Items[i].IsSelected)
                            {
                                DownloadItem item = new DownloadItem();
                                item.Name = model.Items[i].Name;
                                item.Link = model.Items[i].Link;

                                _model.Items.Add(item);
                            }
                        }
                    }
                    TaskWork(_model);
                    Manga.Structures.Properties.Settings.Default.DownloadCount--;
                },
                _cancelToken.Token,
                TaskCreationOptions.LongRunning);
            }
        }

        public void Start()
        {
            _task.Start();
        }

        private void TaskWork(DownloadModel model)
        {            
            _server.Download(model, _task.IsCanceled);
            model.IsBusy = false;
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
