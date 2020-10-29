using System;

namespace Manga.Structures
{
    /// <summary>
    /// Chapter is a class which holds the chapter information, It can be compared with other Chapters.
    /// </summary>
    public sealed class Chapter : IComparable<Chapter>
    {
        #region Members

        private String _name;
        private String _link;
        private String _date;
        private int _size;
        private long _id;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the Chapter
        /// </summary>
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// Link of the Chapter
        /// </summary>
        public String Link
        {
            get { return _link; }
            set { _link = value; }
        }
        /// <summary>
        /// Id of manga which contains this chapter
        /// </summary>
        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }
        /// <summary>
        /// Size of the chapter
        /// </summary>
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }
        /// <summary>
        /// Release date of the chapter
        /// </summary>
        public String Date
        {
            get { return _date; }
            set { _date = value; }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public Chapter()
        {
            _size = 0;
            _id = -1;
            _name = String.Empty;
            _link = String.Empty;
        }

        #endregion

        #region Comparer
        /// <summary>
        /// Compare using on the Name property of the chapter
        /// </summary>
        /// <param name="other">Other Chapter to compare</param>
        /// <returns></returns>
        public int CompareTo(Chapter other)
        {
            return _name.CompareTo(other._name);
        }

        #endregion
    }
}
