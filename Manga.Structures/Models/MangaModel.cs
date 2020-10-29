using Manga.Framework;
using Manga.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Manga.Structures.Models
{
    public sealed class MangaModel : ObservableObject, IComparable<MangaModel>, IEquatable<MangaModel>, IMangaModel
    {
        #region Members

        Manga _manga;
        ThreadSafeObservableCollection<IChapterModel> _items;
        Uri _imageSource;
        int _infoState;

        #endregion

        #region Properties

        public Manga Manga
        {
            get { return _manga; }
            set
            {
                _manga = value;
                RaisePropertyChanged("Manga");
            }
        }

        public String Name
        {
            get { return _manga.Name; }
            set
            {
                _manga.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public String Link
        {
            get { return _manga.Link; }
            set
            {
                _manga.Link = value;
                RaisePropertyChanged("Link");
            }
        }

        public String Artist
        {
            get { return _manga.Artist; }
            set
            {
                _manga.Artist = value;
                RaisePropertyChanged("Artist");
            }
        }

        public String Author
        {
            get { return _manga.Author; }
            set
            {
                _manga.Author = value;
                RaisePropertyChanged("Author");
            }
        }

        public String Description
        {
            get { return _manga.Description; }
            set
            {
                _manga.Description = value;
                RaisePropertyChanged("Description");
            }
        }

        public String Year
        {
            get { return _manga.Year; }
            set
            {
                _manga.Year = value;
                RaisePropertyChanged("Year");
            }
        }

        public String[] Genres
        {
            get { return _manga.Genres; }
            set
            {
                _manga.Genres = value;
                RaisePropertyChanged("Genres");
            }
        }
        /// <summary>
        /// Gets or Sets the altenative name of manga. It's generally japanese.
        /// </summary>
        public String AlternativeName
        {
            get { return _manga.AlternativeName; }
            set
            {
                if (_manga.AlternativeName != value)
                {
                    _manga.AlternativeName = value;
                    RaisePropertyChanged("AlternativeName");
                }
            }
        }
        /// <summary>
        /// Gets or Sets Status of the manga. True if completed, False if incomplete.
        /// </summary>
        public int Status
        {
            get { return _manga.Status; }
            set
            {
                if (_manga.Status != value)
                {
                    _manga.Status = value;
                    RaisePropertyChanged("Status");
                }
            }
        }
        /// <summary>
        /// Gets of Sets the Id of manga which represents the ID field from database.
        /// </summary>
        public long Id
        {
            get { return _manga.Id; }
            set
            {
                if (_manga.Id != value)
                {
                    _manga.Id = value;
                    RaisePropertyChanged("Id");
                }
            }
        }
        /// <summary>
        /// Gets or Sets if manga is in favourites.
        /// </summary>
        public bool IsFavourite
        {
            get { return _manga.IsFavourite; }
            set
            {
                if (_manga.IsFavourite != value)
                {
                    _manga.IsFavourite = value;
                    RaisePropertyChanged("IsFavourite");
                }
            }
        }

        public int Count
        {
            get { return _manga.Count; }
            set
            {
                if (_manga.Count != value)
                {
                    _manga.Count = value;
                    RaisePropertyChanged("Count");
                }
            }
        }

        public Uri ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                RaisePropertyChanged("ImageSource");
            }
        }
        

        public ThreadSafeObservableCollection<IChapterModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public Publisher Publisher { get; set; }

        /// <summary>
        /// Represents the Level of Information this Model Contains, 0 for none, 1 for minimal 2 for extended
        /// </summary>
        public int InfoState
        {
            get { return _infoState; }
            set
            {
                _infoState = value;
                RaisePropertyChanged("InfoState");
            }
        }

        #endregion

        #region Constructor

        public MangaModel() : base()
        {
            _manga = new Structures.Manga();
            _items = new ThreadSafeObservableCollection<IChapterModel>();
            _infoState = 0;
        }

        #endregion

        #region Comparer

        public int CompareTo(MangaModel other)
        {
            return Name.CompareTo(other.Name);
        }

        #endregion

        #region IEquatable

        public bool Equals(MangaModel other)
        {
            return this.Name == other.Name &&
                   this.Link == other.Link;
        }

        #endregion
    }
}
