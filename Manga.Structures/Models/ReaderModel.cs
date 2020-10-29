using Manga.Framework;
using Manga.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga.Structures.Models
{
    public delegate void CreatedEventHandler();

    public sealed class ReaderModel : ObservableObject, IReaderModel
    {
        #region Members

        private String _name;
        private String _link;

        private ThreadSafeObservableCollection<IChapterModel> _items;

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
        /// TODO
        /// </summary>
        public ThreadSafeObservableCollection<IChapterModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public ICollection<IPrioritizable> PriorityQueue
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Status { get; set; }

        public Publisher Publisher { get; set; }

        public long Id
        { get; set; }

        public int InfoState
        { get; set; }

        public int Count
        {
            get;
            set;
        }

        public CreatedEventHandler ItemsCreated;

        #endregion

        #region Constructor

        public ReaderModel()
        {
            _items = new ThreadSafeObservableCollection<IChapterModel>();
            ItemsCreated = new CreatedEventHandler(() => { });
        }

        #endregion        
    }
}
