using Manga.Framework;
using Manga.Interfaces;
using System;

namespace Manga.Structures.Models
{
    public sealed class ChapterModel : ObservableObject, IComparable<ChapterModel>, IChapterModel
    {
        #region Members

        Chapter _chapter;
        IMangaModel _parent;
        bool _isSelected;
        bool _isNew;
        int _isDowloaded;
        int _isRead;        
        long _id;
        int _index;

        String _path;

        String _volume;

        ThreadSafeObservableCollection<IImageModel> _items;

        #endregion

        #region Properties

        /// <summary>
        /// Chapter
        /// </summary>
        public Chapter Chapter
        {
            get { return _chapter; }
            set
            {
                _chapter = value;
                RaisePropertyChanged("Chapter");
            }
        }

        /// <summary>
        /// Name of the chapter
        /// </summary>
        public String Name
        {
            get { return _chapter.Name; }
            set
            {
                _chapter.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        /// Homepage link of the chapter
        /// </summary>
        public String Link
        {
            get { return _chapter.Link; }
            set
            {
                _chapter.Link = value;
                RaisePropertyChanged("Link");
            }
        }

        /// <summary>
        /// Release year of the chapter
        /// </summary>
        public String Date
        {
            get { return _chapter.Date; }
            set
            {
                _chapter.Date = value;
                RaisePropertyChanged("Date");
            }
        }

        /// <summary>
        /// Total chapter included in manga
        /// </summary>
        public int Size
        {
            get { return _chapter.Size; }
            set
            {
                _chapter.Size = value;
                RaisePropertyChanged("Size");
            }
        }

        public Boolean IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public IMangaModel Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                RaisePropertyChanged("Parent");
            }
        }

        /// <summary>
        /// Gets or Sets the Id of the Chapter
        /// </summary>
        public long Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        /// <summary>
        /// Gets or Sets the Local Path of the Chapter
        /// </summary>
        public String Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged("Path");
            }
        }

        /// <summary>
        /// Gets the Image links of the current chapter model
        /// </summary>
        public ThreadSafeObservableCollection<IImageModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public static ChapterModel Empty
        {
            get { return new ChapterModel(); }
        }

        /// <summary>
        /// Gets or Sets the Volume of the Chapter
        /// </summary>
        public String Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                RaisePropertyChanged("Volume");
            }
        }

        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                _isNew = value;
                RaisePropertyChanged("IsNew");
            }
        }

        public Priority Priority
        {
            get;
            set;
        }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                RaisePropertyChanged("Index");
            }
        }

        public int IsDownloaded 
        {
            get { return _isDowloaded; }
            set
            {
                _isDowloaded = value;
                RaisePropertyChanged("IsDownloaded");
            }
        }

        public int IsRead
        {
            get { return _isRead; }
            set
            {
                _isRead = value;
                RaisePropertyChanged("IsRead");
            }
        }

        #endregion

        #region Constructor

        public ChapterModel()
        {
            _chapter = new Structures.Chapter();
            _isSelected = false;
            _path = String.Empty;
            _volume = String.Empty;
            _items = new ThreadSafeObservableCollection<IImageModel>();
            _isNew = false;
            _index = -1;
            _isDowloaded = -1;
            _isRead = -1;
        }        

        #endregion

        #region Comparer

        public int CompareTo(ChapterModel other)
        {
            return _chapter.Name.CompareTo(other.Name);
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() == obj.GetType())
                return (Name == (obj as ChapterModel).Name);
            return base.Equals(obj);
        }

        #endregion        
    }
}
