using Manga.Framework;
using Manga.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga.Structures.Models
{
    /// <summary>
    /// Model for downloading purposes
    /// </summary>
    public sealed class DownloadModel : ObservableObject, IDisposable
    {
        #region Members

        private String _name;
        private String _link;

        private List<Boolean> _selectionList;

        private ThreadSafeObservableCollection<DownloadItem> _items;

        private bool _isBusy;

        private bool _isDisposed;

        private int _imagesProgress;
        private int _imagesSize;

        private int _chaptersProgress;
        private int _chaptersSize;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the Name of the Manga
        /// </summary>
        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or Sets the Homepage link of the manga
        /// </summary>
        public String Link
        {
            get { return _link; }
            set
            {
                _link = value;
                RaisePropertyChanged("Link");
            }
        }

        /// <summary>
        /// Gets or Sets the List of selected chapters index
        /// </summary>
        public List<Boolean> SelectionList
        {
            get { return _selectionList; }
            set
            {
                _selectionList = value;
                RaisePropertyChanged("SelectionList");
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ThreadSafeObservableCollection<DownloadItem> Items
        {
            get { return _items; }
            set 
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        /// <summary>
        /// Gets the Disposed Flag
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
            private set
            {
                _isDisposed = value;
                RaisePropertyChanged("IsDisposed");
            }
        }

        public Publisher Publisher { get; set; }

        /// <summary>
        /// Gets or Sets the Progress of download in current chapter
        /// </summary>
        public int ImagesProgress
        {
            get { return _imagesProgress; }
            set
            {
                _imagesProgress = value;
                RaisePropertyChanged("ImagesProgress");
            }
        }

        /// <summary>
        /// Gets or Sets the Number of images in current chapter
        /// </summary>
        public int ImagesSize
        {
            get { return _imagesSize; }
            set
            {
                _imagesSize = value;
                RaisePropertyChanged("ImagesSize");
            }
        }

        /// <summary>
        /// Gets or Sets the Progress of total download
        /// </summary>
        public int ChaptersProgress
        {
            get { return _chaptersProgress; }
            set
            {
                _chaptersProgress = value;
                RaisePropertyChanged("ChaptersProgress");
            }
        }

        /// <summary>
        /// Gets or Sets the Number of chapters in current task
        /// </summary>
        public int ChaptersSize
        {
            get { return _chaptersSize; }
            set
            {
                _chaptersSize = value;
                RaisePropertyChanged("ChaptersSize");
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates an Instance of DownloadModel object.
        /// </summary>
        public DownloadModel()
        {
            _items = new ThreadSafeObservableCollection<DownloadItem>();
            _selectionList = new List<Boolean>();
            _isDisposed = false;
            _isBusy = false;
        }

        ~DownloadModel()
        {
            if (!_isDisposed)
                Dispose();
        }

        #endregion

        #region Dispose
        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            _selectionList.Clear();
            _isDisposed = true;
        }

        #endregion
    }

    public class DownloadItem : ObservableObject
    {
        #region Members

        String _name;
        String _link;

        int _size;
        int _progress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the Name of the Downloading Item
        /// </summary>
        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public String Link
        {
            get { return _link; }
            set { _link = value; }
        }
        public int Size
        {
            get { return _size; }
            set { _size = value; RaisePropertyChanged("Size"); }
        }
        public int Progress
        {
            get { return _progress; }
            set { _progress = value; }
        }

        #endregion

        /// <summary>
        /// Creates an instance of DownloadItem object.
        /// </summary>
        public DownloadItem()
        {
            _name = string.Empty;
            _link = string.Empty;

            _size = -1;
            _progress = -1;
        }
    }
}
