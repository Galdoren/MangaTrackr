using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga.Structures
{
    /// <summary>
    /// Manga is a class which holds the manga information, It can be compared with other Mangas
    /// </summary>
    public sealed class Manga : IComparable<Manga>
    {
        #region Members

        private String _name;
        private String _link;
        private String _description;
        private String[] _genres;
        private String _author;
        private String _artist;
        private String _year;
        private String _alternativeName;
        private int _status;
        private long _id;
        private bool _isFavourite;
        private int _count;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the manga
        /// </summary>
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// Link of the manga
        /// </summary>
        public String Link
        {
            get { return _link; }
            set { _link = value; }
        }
        /// <summary>
        /// Summary of manga
        /// </summary>
        public String Description
        {
            get { return _description; }
            set { _description = value; }
        }
        /// <summary>
        /// Genres of manga
        /// </summary>
        public String[] Genres
        {
            get { return _genres; }
            set { _genres = value; }
        }
        /// <summary>
        /// Artist of manga
        /// </summary>
        public String Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }
        /// <summary>
        /// Author of manga
        /// </summary>
        public String Author
        {
            get { return _author; }
            set { _author = value; }
        }
        /// <summary>
        /// Release Year of manga
        /// </summary>
        public String Year
        {
            get { return _year; }
            set { _year = value; }
        }
        /// <summary>
        /// Alternative name of manga
        /// </summary>
        public String AlternativeName
        {
            get { return _alternativeName; }
            set { _alternativeName = value; }
        }
        /// <summary>
        /// Status of the manga
        /// </summary>
        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }
        /// <summary>
        /// Unique id of manga
        /// </summary>
        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }
        /// <summary>
        /// TODO
        /// </summary>
        public bool IsFavourite
        {
            get { return _isFavourite; }
            set { _isFavourite = value; }
        }
        /// <summary>
        /// TODo
        /// </summary>
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        #endregion

        #region Constructor

        public Manga()
        {
            _name = String.Empty;
            _link = String.Empty;
            _description = String.Empty;
            _author = String.Empty;
            _artist = String.Empty;
            _year = String.Empty;
            _alternativeName = String.Empty;
            _status = -1;
            _id = -1;
            _isFavourite = false;
        }

        #endregion

        #region Comparer

        public int CompareTo(Manga other)
        {
            return _name.CompareTo(other.Name);
        }

        #endregion
    }
}