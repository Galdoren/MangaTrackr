using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Framework
{
    public class DownloadFile : ObservableObject, IPrioritizable
    {
        #region Members

        Priority _priority;

        #endregion

        public string Url { get; set; }

        public string PathToSave { get; set; }

        public DownloadFinishedEventHandler FinishedEvent;

        public int Index { get; set; }

        public Priority Priority 
        {
            get { return _priority; }
            set
            {
                _priority = value;
                RaisePropertyChanged("Priority");
            }
        }

        public DownloadFile()
        {

        }
    }

    public delegate void DownloadFinishedEventHandler(DownloadFile sender);
    public delegate void WorkerFinishedEventHandler();
    public delegate void DownloadExistsAlreadyEventHandler(DownloadFile sender);

    public class ParallelWorker : IDisposable
    {
        #region Members
        private PriorityQueue<PriorityQueue<DownloadFile, Priority>, Priority> _queueToDownlaod;
        private IList<Task> _downloadingTasks;
        private IList<System.Threading.CancellationTokenSource> _downloadingCancel;
        private System.Timers.Timer _downloadTimer;

        private int _parallelDownloads;

        private static ParallelWorker _downloadInstance;
        private static ParallelWorker _readerInstance;

        private bool _isDisposed;
        #endregion

        #region Instance

        public static ParallelWorker InstanceDownload
        {
            get
            {
                if (_downloadInstance == null)
                    _downloadInstance = new ParallelWorker(3);
                return _downloadInstance;
            }
        }

        #endregion

        #region Constructor
        private ParallelWorker(int parallelDownloads)
        {
            _isDisposed = false;

            _queueToDownlaod = new PriorityQueue<PriorityQueue<DownloadFile, Priority>, Priority>();
            _downloadingTasks = new List<Task>();
            _downloadingCancel = new List<System.Threading.CancellationTokenSource>();
            _downloadTimer = new System.Timers.Timer();

            _parallelDownloads = parallelDownloads;

            _downloadTimer.Elapsed += new System.Timers.ElapsedEventHandler(DownloadTimer_Elapsed);
            _downloadTimer.Interval = 500;
            _downloadTimer.Start();

            ServicePointManager.DefaultConnectionLimit = parallelDownloads;
        }
        #endregion

        #region Destructor

        ~ParallelWorker()
        {
            if (!_isDisposed)
                Dispose();
        }

        #endregion

        #region Dispose

        /// <summary>
        /// NEEDS IMPROVEMENTS
        /// </summary>
        public void Dispose()
        {
            _downloadTimer.Stop();
            _downloadTimer.Dispose();

            lock (_queueToDownlaod)
                while (!_queueToDownlaod.IsEmpty)
                {
                    PriorityQueue<DownloadFile, Priority> q;
                    bool b = _queueToDownlaod.TryDequeue(out q);
                }

            lock (_downloadingTasks)
                while (_downloadingTasks.Count != 0)
                {
                    _downloadingCancel[0].Cancel();
                    _downloadingTasks.RemoveAt(0);
                    _downloadingCancel.RemoveAt(0);
                }

            _isDisposed = true;
        }

        #endregion

        /// <summary>
        /// Enqueue new file for downloading
        /// </summary>
        /// <param name="file">File structure contains information for downloading</param>
        public void EnqueueFileToDownload(PriorityQueue<DownloadFile, Priority> file)
        {
            lock(_queueToDownlaod)
                _queueToDownlaod.Enqueue(file, file.Priority);
        }

        /// <summary>
        /// Check Insert Queue in every tick to decide if there is/are new file(s) waiting for download
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DownloadTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StartDownload();
        }


        private void StartDownload()
        {
            lock (_downloadingTasks)
            {
                if (_downloadingTasks.Count < _parallelDownloads && _queueToDownlaod.Count > 0)
                {
                    PriorityQueue<DownloadFile, Priority> queueToDownload = _queueToDownlaod.Peek();

                    DownloadFile fileToDownload;

                    if (queueToDownload.TryDequeue(out fileToDownload))
                    {
                        System.Threading.CancellationTokenSource source = new System.Threading.CancellationTokenSource();

                        var task = new Task(() =>
                        {
                            bool fin = false;
                            int tryCount = 0;
                            while (!fin && tryCount != 5)
                            {
                                fin = DownloadRemoteImageFile(fileToDownload.Url, fileToDownload.PathToSave);
                                tryCount++;
                            }
                        }, source.Token, TaskCreationOptions.LongRunning);

                        task.ContinueWith((t) =>
                        {
                            DownloadOverCallback(t, source);
                            fileToDownload.FinishedEvent.Invoke(fileToDownload);
                            System.Threading.Thread.Sleep(50);
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);

                        _downloadingCancel.Add(source);
                        _downloadingTasks.Add(task);
                        task.Start();

                        queueToDownload.Size--;

                        if (queueToDownload.Size == 0)
                            _queueToDownlaod.Dequeue();
                    }
                }
            }
        }

        private bool DownloadRemoteImageFile(string uri, string fileName)
        {
            bool fin = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ServicePoint.Expect100Continue = false;
                request.Proxy = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // Check that the remote file was found. The ContentType
                    // check is performed since a request for a non-existent
                    // image file might be redirected to a 404-page, which would
                    // yield the StatusCode "OK", even though the image was not
                    // found.
                    if ((response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Moved ||
                        response.StatusCode == HttpStatusCode.Redirect) &&
                        response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    {

                        // if the remote file was found, download oit
                        using (Stream inputStream = response.GetResponseStream())
                        using (Stream outputStream = File.OpenWrite(fileName))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            do
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                outputStream.Write(buffer, 0, bytesRead);
                            } while (bytesRead != 0);
                            outputStream.Close();
                            inputStream.Close();
                        }
                    }
                    response.Close();
                    fin = true;
                }
            }
            catch(System.Net.WebException)
            {
                fin = false;
            }
            return fin;
        }

        public void DownloadOverCallback(Task downloadingTask, System.Threading.CancellationTokenSource source)
        {
            lock (_downloadingTasks)
            {
                lock (_downloadingCancel)
                {
                    _downloadingTasks.Remove(downloadingTask);
                    _downloadingCancel.Remove(source);
                    source.Dispose();
                }
            }
        }

        public void WaitAll()
        {
            bool finished = false;

            while (!finished)
            {
                lock (_downloadingTasks)
                    lock(_queueToDownlaod)
                    {
                        if (_downloadingTasks.Count == 0 && _queueToDownlaod.Count == 0)
                            finished = true;                    
                    }

                System.Threading.Thread.Sleep(250);
            }
        }
    }
}
