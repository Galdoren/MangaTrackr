using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using Manga.Structures.Models;
using System.IO;
using Manga.Framework;
using Manga.Interfaces;
using System.Threading;

namespace Manga.Database
{
    public class DatabaseEngine : IDisposable
    {
        #region Members

        private SQLiteConnection connection = null;
        private ReaderWriterLockSlim _locker = null;

        bool _disposed;

        static DatabaseEngine _instance;

        object _semaphore;

        #endregion
        /// <summary>
        /// Used for connecting to an already existing database.
        /// </summary>
        protected String CONNECTION_STRING = "Data Source=manga.db3;Version=3;New=False;Compress=True;PRAGMA foreign_keys=ON;";
        /// <summary>
        /// Used for connecting to a new database.
        /// </summary>
        protected String CONNECTION_STRING_NEW = "Data Source=manga.db3;Version=3;New=True;Compress=True;foreign_keys=ON;";
        

        #region Queries

        #region CREATE QUERIES

        protected String QUERY_CREATE_PUBLISHER = @"CREATE TABLE IF NOT EXISTS [PUBLISHER](
                                                        [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                        [NAME] TEXT NOT NULL UNIQUE,
                                                        [URL] TEXT NOT NULL UNIQUE);";

        protected String QUERY_CREATE_PERSON = @"CREATE TABLE IF NOT EXISTS [PERSON](
                                                    [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                    [NAME] TEXT UNIQUE);";

        protected String QUERY_CREATE_GENRE = @"CREATE TABLE IF NOT EXISTS [GENRE](
                                                    [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                    [NAME] TEXT UNIQUE);";

        protected String QUERY_CREATE_MANGA_GENRE = @"CREATE TABLE IF NOT EXISTS [MANGA_GENRE](
                                                        [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                        [MANGAID] INTEGER NOT NULL,
                                                        [GENREID] INTEGER NOT NULL,
                                                        FOREIGN KEY(MANGAID) REFERENCES MANGA(ID),
                                                        FOREIGN KEY(GENREID) REFERENCES GENRE(ID));";

        protected String QUERY_CREATE_DESCRIPTION = @"CREATE TABLE IF NOT EXISTS [MANGA_DESCRIPTION](
                                                        [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                        [MANGAID] INTEGER NOT NULL,
                                                        [DESCRIPTION] TEXT,
                                                        FOREIGN KEY(MANGAID) REFERENCES MANGA(ID));";

        protected String QUERY_CREATE_MANGA = @"CREATE TABLE IF NOT EXISTS [MANGA](
                                                    [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                    [PUBLISHERID] INTEGER NOT NULL,
                                                    [AUTHORID] INTEGER,
                                                    [ARTISTID] INTEGER,
                                                    [NAME] TEXT NOT NULL,
                                                    [URL] TEXT NOT NULL UNIQUE,
                                                    [RELEASE] TEXT,
                                                    [STATUS] INTEGER,
                                                    [ALTERNATIVENAME] TEXT,
                                                    FOREIGN KEY(AUTHORID) REFERENCES PERSON(ID),
                                                    FOREIGN KEY(ARTISTID) REFERENCES PERSON(ID),
                                                    FOREIGN KEY(PUBLISHERID) REFERENCES PUBLISHER(ID));";

        protected String QUERY_CREATE_CHAPTER = @"CREATE TABLE IF NOT EXISTS [CHAPTER](
                                                    [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                    [MANGAID] INTEGER NOT NULL,
                                                    [ORDER] INTEGER NOT NULL,
                                                    [NAME] TEXT,
                                                    [URL] TEXT NOT NULL,
                                                    [SIZE] INTEGER,
                                                    [DATE] INTEGER,
                                                    [PATH] TEXT,
                                                    [ISREAD] INTEGER DEFAULT 0,
                                                    [ISDOWNLOADED] INTEGER DEFAULT 0,
                                                    FOREIGN KEY(MANGAID) REFERENCES MANGA(ID),
                                                    UNIQUE(MANGAID,URL));";

        protected String QUERY_CREATE_FAVOURITES = @"CREATE TABLE IF NOT EXISTS [FAVOURITES](
                                                        [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                        [MANGAID] INTEGER NOT NULL,
                                                        [COVERURL] TEXT,
                                                        [DOWNLOADPATH] TEXT,
                                                        FOREIGN KEY(MANGAID) REFERENCES MANGA(ID));";

        #endregion

        #region GET QUERIES
        /// <summary>
        /// Returns MANGA.ID (ID), MANGA.NAME (NAME), MANGA.ALTERNATIVENAME (ALTERNATIVENAME), PERSON.NAME (AUTHOR), PERSON.NAME (ARTIST),
        /// MANGA.URL (URL),  MANGA.RELEASE (RELEASE), MANGA.STATUS (STATUS), FAVOURITES.COVERURL (COVERURL), 
        /// MANGA_DESCRIPTION.DESCRIPTION as DESCRIPTION
        /// </summary>
        protected String QUERY_GET_MANGA = @"SELECT m.ID, m.NAME, m.ALTERNATIVENAME, au.NAME AS AUTHOR, 
                                                        ar.NAME AS ARTIST, m.URL, m.RELEASE, m.STATUS, 
                                                        f.COVERURL, d.DESCRIPTION 
                                                        FROM MANGA m 
                                                        LEFT JOIN PUBLISHER p ON m.PUBLISHERID = p.ID 
                                                        LEFT JOIN FAVOURITES f ON f.MANGAID = m.ID 
                                                        LEFT JOIN MANGA_DESCRIPTION d ON d.MANGAID = m.ID
                                                        LEFT JOIN PERSON au ON au.ID = m.AUTHORID 
                                                        LEFT JOIN PERSON ar ON ar.ID = m.AUTHORID 
                                                        WHERE p.NAME = @publisherName;";

        protected String QUERY_GET_PUBLISHERS = "SELECT * FROM PUBLISHER;";

        /// <summary>
        /// Returns the ID from PUBLISHER table for given NAME field with @name parameter.
        /// </summary>
        protected String QUERY_GET_PUBLISHER_ID = "SELECT ID FROM PUBLISHER WHERE NAME = @name;";

        /// <summary>
        /// Returns PUBLISHER.NAME (NAME), MANGA.ID (ID), MANGA.NAME (NAME), MANGA.ALTERNATIVENAME (ALTERNATIVENAME), 
        /// PERSON.NAME (AUTHOR), PERSON.NAME (ARTIST), MANGA.URL (URL),  MANGA.RELEASE (RELEASE), 
        /// MANGA.STATUS (STATUS), FAVOURITES.COVERURL (COVERURL), MANGA_DESCRIPTION.DESCRIPTION (DESCRIPTION)
        /// </summary>
        protected String QUERY_GET_FAVOURITES = @"SELECT p.NAME, m.ID, m.NAME, m.ALTERNATIVENAME, au.NAME AS AUTHOR, 
                                                        ar.NAME AS ARTIST, m.URL, m.RELEASE, m.STATUS, 
                                                        f.COVERURL, d.DESCRIPTION 
                                                        FROM FAVOURITES f 
                                                        LEFT JOIN MANGA m ON f.MANGAID = m.ID 
                                                        LEFT JOIN PUBLISHER p ON m.PUBLISHERID = p.ID 
                                                        LEFT JOIN MANGA_DESCRIPTION d ON d.MANGAID = m.ID
                                                        LEFT JOIN PERSON au ON au.ID = m.AUTHORID 
                                                        LEFT JOIN PERSON ar ON ar.ID = m.AUTHORID;";

        protected String QUERY_GET_MANGA_ID = @"SELECT m.ID FROM MANGA m 
                                                    LEFT JOIN PUBLISHER p ON m.PUBLISHERID = p.ID 
                                                    WHERE m.NAME = @mangaName AND p.NAME = @publisherName;";

        protected String QUERY_GET_MANGA_ID_LIST = @"SELECT m.ID, m.NAME, m.URL FROM MANGA m
                                                        LEFT JOIN PUBLISHER p ON m.PUBLISHERID = p.ID
                                                        WHERE p.NAME = @publisherName;";

        protected String QUERY_GET_CHAPTER = @"SELECT c.MANGAID, c.NAME, c.URL, c.SIZE, c.DATE, c.PATH, c.ISREAD 
                                                    FROM CHAPTER c 
                                                    LEFT JOIN MANGA m ON c.MANGAID = m.ID
                                                    WHERE m.ID = @mangaID;";

        protected String QUERY_GET_MANGA_AUTHOR = @"SELECT p.NAME FROM PERSON p
                                                        LEFT JOIN MANGA m ON m.AUTHORID = p.ID
                                                        WHERE m.ID = @mangaID;";

        protected String QUERY_GET_MANGA_ARTIST = @"SELECT p.NAME FROM PERSON p
                                                        LEFT JOIN MANGA m ON m.ARTISTID = p.ID
                                                        WHERE m.ID = @mangaID;";

        protected String QUERY_GET_MANGA_DESCRIPTION = @"SELECT d.DESCRIPTION FROM MANGA_DESCRIPTION d
                                                            LEFT JOIN MANGA m ON d.MANGAID = m.ID
                                                            WHERE m.ID = @mangaID;";

        protected String QUERY_GET_PERSON_ID = @"SELECT p.ID FROM PERSON p WHERE p.NAME = @name;";

        protected String QUERY_GET_GENRE = "SELECT ID,NAME FROM GENRE;";

        protected String QUERY_GET_MANGA_GENRE = @"SELECT mg.MANGAID, g.NAME FROM MANGA_GENRE mg
                                                            LEFT JOIN GENRE g ON mg.GENREID = g.ID
                                                            LEFT JOIN MANGA m ON m.ID = mg.MANGAID
                                                            LEFT JOIN PUBLISHER p ON m.PUBLISHERID = p.ID
                                                            WHERE p.NAME = @publisherName;";

        #endregion

        #region ADD QUERIES
        /// <summary>
        /// Inserts into PUBLISHER table NAME column with @name parameter.
        /// </summary>
        protected String QUERY_ADD_PUBLISHER = "INSERT OR IGNORE INTO PUBLISHER (NAME,URL) VALUES (@name,@url);";

        /// <summary>
        /// Inserts into MANGA table PUBLISHERID, NAME, URL columns with @publisherName, @name and @url parameters.
        /// </summary>
        protected String QUERY_ADD_MANGA_WITH_NAME = @"INSERT OR IGNORE INTO MANGA (PUBLISHERID,NAME,URL)
                                                        SELECT p.ID, @name, @url FROM PUBLISHER p
                                                        WHERE p.NAME = @publisherName;";
        /// <summary>
        /// Inserts into MANGA table PUBLISHERID, NAME, URL columns with @publisherName, @name and @url parameters.
        /// </summary>
        protected String QUERY_ADD_MANGA_WITH_NAME_RET = @"INSERT OR IGNORE INTO MANGA (PUBLISHERID,NAME,URL)
                                                        SELECT p.ID, @name, @url FROM PUBLISHER p
                                                        WHERE p.NAME = @publisherName;
                                                        SELECT m.ID FROM MANGA m, PUBLISHER p WHERE
                                                        m.NAME = @name AND m.URL = @url AND p.NAME = @publisherName AND m.PUBLISHERID = p.ID;";

        /// <summary>
        /// Inserts into MANGA table PUBLISHERID, NAME, URL columns with  @publisherID, @name and @url parameters.
        /// </summary>
        protected String QUERY_ADD_MANGA_WITH_ID = @"INSERT OR IGNORE INTO MANGA (PUBLISHERID,NAME,URL) VALUES (@publisherID, @name, @url);";
        /// <summary>
        /// Inserts into MANGA table PUBLISHERID, NAME, URL columns with  @publisherID, @name and @url parameters.
        /// </summary>
        protected String QUERY_ADD_MANGA_WITH_ID_RET = @"INSERT OR IGNORE INTO MANGA (PUBLISHERID,NAME,URL) VALUES (@publisherID, @name, @url);
                                                            SELECT m.ID FROM MANGA m, PUBLISHER p WHERE
                                                                m.NAME = @name AND m.URL = @url AND 
                                                                p.NAME = @publisherName AND 
                                                                m.PUBLISHERID = p.ID;";

        /// <summary>
        /// Inserts into MANGA table PUBLISHERID, AUTHORID, ARTISTID, NAME, URL, RELEASE, STATUS columns with @publisherID, @authorID, 
        /// @artistId, @name, @url, @release and @status parameters.
        /// </summary>
        protected String QUERY_ADD_MANGA_WITH_ID_EX = @"INSERT OR IGNORE INTO MANGA (PUBLISHERID,AUTHORID,ARTISTID,NAME,URL,RELEASE,STATUS) 
                                                        VALUES(@publisherID,@authorID,@artistID,@name,@url,@release,@status);";

        /// <summary>
        /// Inserts into MANGA table PUBLISHERID, AUTHORID, ARTISTID, NAME, URL, RELEASE, STATUS columns with @publisherID, @authorID, 
        /// @artistId, @name, @url, @release and @status parameters.
        /// </summary>
        protected String QUERY_ADD_MANGA_WITH_ID_EX_RET = @"INSERT OR IGNORE INTO MANGA (PUBLISHERID,AUTHORID,ARTISTID,NAME,URL,RELEASE,STATUS) 
                                                        VALUES(@publisherID,@authorID,@artistID,@name,@url,@release,@status);
                                                            SELECT m.ID FROM MANGA m, PUBLISHER p WHERE
                                                             m.NAME = @name AND m.URL = @url AND 
                                                             p.NAME = @publisherName AND 
                                                             m.PUBLISHERID = p.ID; ";
        
        /// <summary>
        /// Inserts into MANGA table PUBLISHERID, AUTHORID, ARTISTID, NAME,URL, RELEASE, STATUS columns with @publisherName, @authorName, 
        /// @artistName, @name, @url, @release, @status parameters.
        /// </summary>
        protected String QUERY_ADD_MANGA_WITH_NAME_EX = @"INSERT OR IGNORE INTO MANGA (PUBLISHERID,AUTHORID,ARTISTID,NAME,URL,RELEASE,STATUS)
                                                            SELECT p.ID,paut.ID,par.ID,'@name,@url,@release,@status FROM PUBLISHER p
                                                            LEFT JOIN PERSON paut ON paut.NAME = @authorName
                                                            LEFT JOIN PERSON par ON par.NAME = @artistName
                                                            WHERE p.NAME = @publisherName;";

        /// <summary>
        /// Inserts into MANGA table PUBLISHERID, AUTHORID, ARTISTID, NAME,URL, RELEASE, STATUS columns with @publisherName, @authorName, 
        /// @artistName, @name, @url, @release, @status parameters.
        /// </summary>
        protected String QUERY_ADD_MANGA_WITH_NAME_EX_RET = @"INSERT OR IGNORE INTO MANGA (PUBLISHERID,AUTHORID,ARTISTID,NAME,URL,RELEASE,STATUS)
                                                            SELECT p.ID,paut.ID,par.ID,'@name,@url,@release,@status FROM PUBLISHER p
                                                            LEFT JOIN PERSON paut ON paut.NAME = @authorName
                                                            LEFT JOIN PERSON par ON par.NAME = @artistName
                                                            WHERE p.NAME = @publisherName;
                                                                SELECT m.ID FROM MANGA m, PUBLISHER p WHERE
                                                                   m.NAME = @name AND m.URL = @url AND 
                                                                   p.NAME = @publisherName AND 
                                                                   m.PUBLISHERID = p.ID;";
        
        /// <summary>
        /// Inserts into PERSON table NAME column with @name parameter.
        /// </summary>
        protected String QUERY_ADD_PERSON = @"INSERT OR IGNORE INTO PERSON (NAME) VALUES (@name);";

        /// <summary>
        /// Inserts into PERSON table NAME column with @name parameter.
        /// </summary>
        protected String QUERY_ADD_PERSON_RET = @"INSERT OR IGNORE INTO PERSON (NAME) VALUES (@name);
                                                    SELECT ID FROM PERSON WHERE NAME = @name;";


        protected String QUERY_ADD_DESCRIPTION = @"INSERT OR IGNORE INTO MANGA_DESCRIPTION (MANGAID,DESCRIPTION) VALUES (@mangaID,@description);";

        protected String QUERY_ADD_DESCRIPTION_RET = @"INSERT OR IGNORE INTO MANGA_DESCRIPTION (MANGAID,DESCRIPTION) VALUES (@mangaID,@description);
                                                        SELECT ID FROM MANGA_DESCRIPTION WHERE MANGAID = @mangaID;";

        protected String QUERY_ADD_FAVOURITE = @"INSERT OR IGNORE INTO FAVOURITE (MANGAID,COVERURL) VALUES (@mangaID,@coverURL);";

        protected String QUERY_ADD_GENRE = @"INSERT OR IGNORE INTO GENRE (NAME) VALUES (@name);";

        protected String QUERY_ADD_GENRE_RET = @"INSERT OR IGNORE INTO GENRE (NAME) VALUES (@name);
                                                    SELECT ID FROM GENRE WHERE NAME = @name;";

        protected String QUERY_ADD_MANGA_GENRE = @"INSERT OR IGNORE INTO MANGA_GENRE (MANGAID,GENREID) VALUES (@mangaID,@genreID);";

        protected String QUERY_ADD_CHAPTER = @"INSERT OR IGNORE INTO CHAPTER (MANGAID,NAME,URL,DATE) VALUES (@mangaID,@name,@url,@date);";

        protected String QUERY_ADD_CHAPTER_RET = @"INSERT OR IGNORE INTO CHAPTER (MANGAID,NAME,URL,DATE) VALUES (@mangaID,@name,@url,@date);
                                                SELECT ID FROM CHAPTER WHERE MANGAID = @mangaID AND NAME = @name AND URL = @url;";

        protected String QUERY_ADD_CHAPTER_EX = @"INSERT OR IGNORE INTO CHAPTER (MANGAID,ORDER,NAME,URL,SIZE,DATE,PATH) VALUES
                                                    (@mangaID,@order,@name,@url,@size,@date,@path);";


        #endregion

        #region UPDATE QUERIES

        protected String QUERY_UPDATE_MANGA_PART_START = "UPDATE MANGA SET";
        protected String QUERY_UPDATE_MANGA_PART_END = "WHERE ID = @mangaID;";
        protected String QUERY_UPDATE_MANGA_PART_AUTHOR = "AUTHORID = @authorID";
        protected String QUERY_UPDATE_MANGA_PART_ARTIST = "ARTISTID = @artistID";
       // protected String QUERY_UPDATE_MANGA_PART_AUTHOR = "AUTHORID = (SELECT ID FROM PERSON WHERE NAME = @authorName)";
       // protected String QUERY_UPDATE_MANGA_PART_ARTIST = "ARTISTID = (SELECT ID FROM PERSON WHERE NAME = @artistName)";
        protected String QUERY_UPDATE_MANGA_PART_RELEASE = "RELEASE = @release";
        protected String QUERY_UPDATE_MANGA_PART_STATUS = "STATUS = @status";

        protected String QUERY_UPDATE_CHAPTER_PART_START = "UPDATE CHAPTER SET";
        protected String QUERY_UPDATE_CHAPTER_PART_END = "WHERE ID = @chapterID;";
        protected String QUERY_UPDATE_CHAPTER_PART_SIZE = "SIZE = @size";
        protected String QUERY_UPDATE_CHAPTER_PART_DATE = "DATE = @date";
        protected String QUERY_UPDATE_CHAPTER_PART_PATH = "PATH = @path";
        protected String QUERY_UPDATE_CHAPTER_PART_ORDER = "ORDER = @order";
        protected String QUERY_UPDATE_CHAPTER_PART_ISREAD = "ISREAD = @isRead";
        protected String QUERY_UPDATE_CHAPTER_PART_ISDOWNLOADED = "ISDOWNLOADED = @isDownloaded";

        #endregion

        #region DELETE QUERIES

        protected String QUERY_DELETE_FAVOURITE = "DELETE FROM FAVOURITES WHERE ID = @mangaID;";

        #endregion

        #region Deprecated

        protected String _createMangafoxTableQuery = @"CREATE TABLE IF NOT EXISTS [Mangafox](
                                                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                    [Name] TEXT UNIQUE,
                                                    [Link] TEXT UNIQUE,
                                                    [Author] TEXT,
                                                    [Artist] TEXT,
                                                    [Genres] INTEGER);";

        protected String _createMangafoxChaptersTableQuery = @"CREATE TABLE IF NOT EXISTS [MangafoxChapters](
                                                                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                                [UId] INTEGER NOT NULL,
                                                                [Name] TEXT,
                                                                [Link] TEXT,
                                                                [Size] INTEGER,
                                                                [Date] TEXT,
                                                                [Path] TEXT);";

        protected String _mangafoxGetQuery = @"Select a.Id,
                                                coalesce(a.Name, '') as 'Name',
                                                coalesce(a.Link, '') as 'Link',
                                                coalesce((SELECT b.Author FROM Favourites b WHERE a.Link = b.Link), a.Author, '') as 'Author',
                                                coalesce((SELECT b.Artist FROM Favourites b WHERE a.Link = b.Link), a.Artist, '') as 'Artist',
                                                coalesce((SELECT b.Alternativename FROM Favourites b WHERE a.Link = b.Link), '') as 'Alternativename',
                                                coalesce((SELECT b.Year FROM Favourites b WHERE a.Link = b.Link), '') as 'Year',
                                                coalesce((SELECT b.Imagesource FROM Favourites b WHERE a.Link = b.Link), '') as 'Imagesource',
                                                coalesce((SELECT b.Description FROM Favourites b WHERE a.Link = b.Link), '') as 'Description',
                                                coalesce((SELECT b.Genres FROM Favourites b WHERE a.Link = b.Link), a.Genres, null) as 'Genres'
                                            FROM Mangafox a
                                            Group by a.Id;";

        protected String _createMangareaderTableQuery = @"CREATE TABLE IF NOT EXISTS [Mangareader](
                                                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                    [Name] TEXT UNIQUE,
                                                    [Link] TEXT UNIQUE,
                                                    [Author] TEXT,
                                                    [Artist] TEXT,
                                                    [Genres] INTEGER);";

        protected String _createMangareaderChaptersTableQuery = @"CREATE TABLE IF NOT EXISTS [MangareaderChapters](
                                                                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                                [UId] INTEGER NOT NULL,
                                                                [Name] TEXT,
                                                                [Link] TEXT,
                                                                [Size] INTEGER,
                                                                [Date] TEXT,
                                                                [Path] TEXT);";

        protected String _mangareaderGetQuery = @"Select a.Id,
                                                coalesce(a.Name, '') as 'Name',
                                                coalesce(a.Link, '') as 'Link',
                                                coalesce((SELECT b.Author FROM Favourites b WHERE a.Link = b.Link), a.Author, '') as 'Author',
                                                coalesce((SELECT b.Artist FROM Favourites b WHERE a.Link = b.Link), a.Artist, '') as 'Artist',
                                                coalesce((SELECT b.Alternativename FROM Favourites b WHERE a.Link = b.Link), '') as 'Alternativename',
                                                coalesce((SELECT b.Year FROM Favourites b WHERE a.Link = b.Link), '') as 'Year',
                                                coalesce((SELECT b.Imagesource FROM Favourites b WHERE a.Link = b.Link), '') as 'Imagesource',
                                                coalesce((SELECT b.Description FROM Favourites b WHERE a.Link = b.Link), '') as 'Description',
                                                coalesce((SELECT b.Genres FROM Favourites b WHERE a.Link = b.Link), a.Genres, null) as 'Genres'
                                            FROM Mangareader a
                                            Group by a.Id";

        protected String _createMangapandaTableQuery = @"CREATE TABLE IF NOT EXISTS [Mangapanda](
                                                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                    [Name] TEXT UNIQUE,
                                                    [Link] TEXT UNIQUE,
                                                    [Author] TEXT,
                                                    [Artist] TEXT,
                                                    [Genres] INTEGER);";

        protected String _createMangapandaChaptersTableQuery = @"CREATE TABLE IF NOT EXISTS [MangapandaChapters](
                                                                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                                [UId] INTEGER NOT NULL,
                                                                [Name] TEXT,
                                                                [Link] TEXT,
                                                                [Size] INTEGER,
                                                                [Date] TEXT,
                                                                [Path] TEXT);";

        protected String _mangapandaGetQuery = @"Select a.Id,
                                                coalesce(a.Name, '') as 'Name',
                                                coalesce(a.Link, '') as 'Link',
                                                coalesce((SELECT b.Author FROM Favourites b WHERE a.Link = b.Link), a.Author, '') as 'Author',
                                                coalesce((SELECT b.Artist FROM Favourites b WHERE a.Link = b.Link), a.Artist, '') as 'Artist',
                                                coalesce((SELECT b.Alternativename FROM Favourites b WHERE a.Link = b.Link), '') as 'Alternativename',
                                                coalesce((SELECT b.Year FROM Favourites b WHERE a.Link = b.Link), '') as 'Year',
                                                coalesce((SELECT b.Imagesource FROM Favourites b WHERE a.Link = b.Link), '') as 'Imagesource',
                                                coalesce((SELECT b.Description FROM Favourites b WHERE a.Link = b.Link), '') as 'Description',
                                                coalesce((SELECT b.Genres FROM Favourites b WHERE a.Link = b.Link), a.Genres, null) as 'Genres'
                                            FROM Mangapanda a
                                            Group by a.Id;";

        protected String _createFavouritesTableQuery = @"CREATE TABLE IF NOT EXISTS [Favourites](
                                                         [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                                         [UId] INTEGER NOT NULL,
                                                         [Name] TEXT,
                                                         [Link] TEXT UNIQUE,
                                                         [Author] TEXT,
                                                         [Artist] TEXT,
                                                         [Alternativename] TEXT,
                                                         [Year] TEXT,
                                                         [Imagesource] TEXT,
                                                         [Description] TEXT,
                                                         [Genres] INTEGER,
                                                         [Publisher] TEXT);";

        protected String _favouritesGetQuery = "SELECT * from Favourites;";

        #endregion

        #endregion

        #region Singleton Instance

        public static DatabaseEngine Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DatabaseEngine();
                return _instance;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates an Instance of DatabaseEngine object.
        /// </summary>
        private DatabaseEngine()
        {
            _locker = new ReaderWriterLockSlim();
            _semaphore = new object();
            _disposed = false;

            using (_locker.AcquireWriteLock())
            {
                if (OpenConnection())
                    Initialize();
            }
            CloseConnection();
        }

        #endregion

        #region Tables Queries
        /// <summary>
        /// Initializes the tables.
        /// </summary>
        void Initialize()
        {
            // Create tables if they do no exists
            ExecuteQuery(QUERY_CREATE_PUBLISHER);
            ExecuteQuery(QUERY_CREATE_PERSON);
            ExecuteQuery(QUERY_CREATE_GENRE);
            ExecuteQuery(QUERY_CREATE_MANGA);
            ExecuteQuery(QUERY_CREATE_CHAPTER);
            ExecuteQuery(QUERY_CREATE_MANGA_GENRE);
            ExecuteQuery(QUERY_CREATE_DESCRIPTION);
            ExecuteQuery(QUERY_CREATE_FAVOURITES);

            InitializeFavourites();
        }

        void InitializeFavourites()
        {
            var values = Enum.GetValues((typeof(Publisher)));
            using(SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = QUERY_ADD_PUBLISHER;
                SQLiteParameter pName = new SQLiteParameter("@name");
                SQLiteParameter pUrl = new SQLiteParameter("@url");
                command.Parameters.Add(pName);
                command.Parameters.Add(pUrl);
                foreach (Publisher value in values)
                {                    
                    command.Parameters[0].Value = value.ToString();
                    command.Parameters[1].Value = value.Address();
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes the database file.
        /// </summary>
        void DeleteDatabase()
        {
            using(_locker.AcquireWriteLock())
            {
                CloseConnection();
                if(File.Exists("manga.db3"))
                    File.Delete("manga.db3");
            }
        }

        #endregion

        #region Load Queries

        /// <summary>
        /// Fills the given collection with manga from given publisher.
        /// </summary>
        /// <param name="model">Collection to fill with manga objects.</param>
        /// <param name="publisher">Publisher to find mangas in database.</param>
        public void LoadList(ThreadSafeObservableCollection<MangaModel> model, Publisher publisher)
        {
            LoadList(model, publisher, true);
        }
        /// <summary>
        /// Fills the given collection with manga from given publisher.
        /// </summary>
        /// <param name="model">Collection to fill with manga objects.</param>
        /// <param name="publisher">Publisher to find mangas in database.</param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        private void LoadList(ThreadSafeObservableCollection<MangaModel> model, Publisher publisher, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireReadLock())
                {
                    _LoadList(model, publisher);
                }
            }
            else
            {
                _LoadList(model, publisher);
            }
        }
        /// <summary>
        /// Fills the given collection with manga from given publisher.
        /// </summary>
        /// <param name="model">Collection to fill with manga objects.</param>
        /// <param name="publisher">Publisher to find mangas in database.</param>
        private void _LoadList(ThreadSafeObservableCollection<MangaModel> model, Publisher publisher)
        {
            List<MangaModel> tempList = new List<MangaModel>();
            Dictionary<long, List<String>> genres = new Dictionary<long, List<string>>();
            SQLiteDataReader reader = null;
            String p = publisher.ToString();
            

            OpenConnection();
            using(SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = QUERY_GET_MANGA_GENRE;
                command.Parameters.AddWithValue("@publisherName", publisher.ToString());
                reader = command.ExecuteReader();
                
                while(reader.Read())
                {
                    object[] row = new object[reader.FieldCount];
                    reader.GetValues(row);
                    long id = (long)row[0];
                    if(!genres.ContainsKey(id))
                    {
                        genres.Add(id, new List<string>());
                    }
                    genres[id].Add(row[1].ToString());
                }
            }
            reader.Dispose();
            reader = null;
            
            
            using(SQLiteCommand command = connection.CreateCommand())
            {
                try
                {
                    command.CommandText = QUERY_GET_MANGA;
                    command.Parameters.Add(new SQLiteParameter("@publisherName"));
                    command.Parameters["@publisherName"].Value = p;
                    // initialize adapter
                    reader = command.ExecuteReader();

                    while(reader.Read())
                    {
                        object[] row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        MangaModel m = new MangaModel()
                        {
                            Id = (long)row[0],
                            Publisher = publisher,
                            Name = row[1].ToString(),
                            AlternativeName = row[2].ToString(),
                            Author = row[3].ToString(),
                            Artist = row[4].ToString(),
                            Link = row[5].ToString(),
                            Year = row[6].ToString(),
                            //status
                            ImageSource = String.IsNullOrEmpty(row[8].ToString()) ? null : new Uri(row[8].ToString()),
                            Description = row[9].ToString()
                        };
                        if(genres.ContainsKey(m.Id))
                            m.Genres = genres[m.Id].ToArray();

                        tempList.Add(m);
                    }
                    model.AddRange(tempList);
                }
                catch (Exception e) { throw e; }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    connection.Close();
                    connection.Dispose();
                    connection = null;
                }
            }
        }
        /// <summary>
        /// Fills the given collection with favourited manga.
        /// </summary>
        /// <param name="model">Collection to fill with manga objects.</param>
        public void LoadFavourites(ThreadSafeObservableCollection<MangaModel> model)
        {
            LoadFavourites(model, true);
        }
        /// <summary>
        /// Fills the given collection with favourited manga.
        /// </summary>
        /// <param name="model">Collection to fill with manga objects.</param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        private void LoadFavourites(ThreadSafeObservableCollection<MangaModel> model, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireReadLock())
                {
                    _LoadFavourites(model);
                }
            }
            else
            {
                _LoadFavourites(model);
            }
        }
        /// <summary>
        /// Fills the given collection with favourited manga.
        /// </summary>
        /// <param name="model">Collection to fill with manga objects.</param>
        private void _LoadFavourites(ThreadSafeObservableCollection<MangaModel> model)
        {
            try
            {
                List<MangaModel> tempList = new List<MangaModel>();

                SQLiteDataReader _reader = null;

                OpenConnection();

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = QUERY_GET_FAVOURITES;

                    _reader = command.ExecuteReader();

                    while (_reader.Read())
                    {
                        // create row
                        object[] row = new object[_reader.FieldCount];
                        // fill row
                        _reader.GetValues(row);

                        MangaModel item = new MangaModel()
                        {
                            Publisher = (Publisher)Enum.Parse(typeof(Publisher), row[0].ToString()),
                            Id = (long)row[1],
                            Name = row[2].ToString(),
                            AlternativeName = row[3].ToString(),
                            Author = row[4].ToString(),
                            Artist = row[5].ToString(),
                            Link = row[6].ToString(),
                            Year = row[7].ToString(),
                            //status
                            ImageSource = String.IsNullOrEmpty(row[9].ToString()) ? null : new Uri(row[9].ToString()),
                            Description = row[10].ToString(),
                            IsFavourite = true
                        };

                        /*
                        MangaModel item = new MangaModel()
                        {
                            Id = int.Parse(row[1].ToString()),
                            Name = row[2].ToString(),
                            Link = row[3].ToString(),
                            Author = row[4].ToString(),
                            Artist = row[5].ToString(),
                            AlternativeName = row[6].ToString(),
                            Year = row[7].ToString(),
                            ImageSource = new Uri(row[8].ToString()),
                            Description = row[9].ToString()                                                             
                        };
                        item.Publisher = (Publisher)Enum.Parse(typeof(Publisher), row[11].ToString());                            
                        */
                        tempList.Add(item);
                    }
                }

                for (int i = 0; i < tempList.Count; i++)
                {
                    LoadChapters(tempList[i], false);
                }

                model.AddRange(tempList);
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                CloseConnection();
            }
        }
        /// <summary>
        /// Loads the chapters of given manga object.
        /// </summary>
        /// <param name="model">Manga object which will contain the list of chapters.</param>
        public void LoadChapters(IMangaModel model)
        {
            LoadChapters(model, true);
        }
        /// <summary>
        /// Loads the chapters of given manga object.
        /// </summary>
        /// <param name="model">Manga object which will contain the list of chapters.</param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        private void LoadChapters(IMangaModel model, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireReadLock())
                {
                    _LoadChapters(model);
                }
            }
            else
            {
                _LoadChapters(model);
            }
        }
        /// <summary>
        /// Loads the chapters of given manga object.
        /// </summary>
        /// <param name="model">Manga object which will contain the list of chapters.</param>
        private void _LoadChapters(IMangaModel model)
        {
            List<ChapterModel> tempList = new List<ChapterModel>();

            SQLiteDataReader _reader = null;

            OpenConnection();

            try
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = QUERY_GET_CHAPTER;
                    command.Parameters.Add(new SQLiteParameter("@mangaID"));
                    command.Parameters["@mangaID"].Value = model.Id;
                    _reader = command.ExecuteReader();

                    while (_reader.Read())
                    {
                        object[] row = new object[_reader.FieldCount];

                        _reader.GetValues(row);

                        ChapterModel chapter = new ChapterModel()
                        {
                            Id = (long)row[0],
                            Index = (int)row[1],
                            Name = row[2].ToString(),
                            Link = row[3].ToString(),
                            Size = (int)row[4],
                            Date = row[5].ToString(),
                            Path = row[6].ToString(),
                            Parent = model
                        };
                        tempList.Add(chapter);

                        /*
                        tempList.Add(new ChapterModel()
                        {
                            Id = model.Id,
                            Name = row[1].ToString(),
                            Link = row[2].ToString(),
                            Size = int.Parse(row[3].ToString()),
                            Date = row[4].ToString(),
                            Path = row[5].ToString(),
                            Parent = model,
                        });
                        */
                    }

                    model.Items.AddRange(tempList);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (_reader != null)
                {
                    _reader.Close();
                    _reader.Dispose();
                }
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }
        /// <summary>
        /// Retrieves the ID field of manga object from database.
        /// </summary>
        /// <param name="model">Manga object to retrieve ID column from database.</param>
        public void LoadMangaID(IMangaModel model)
        {
            LoadMangaID(model, true);
        }
        /// <summary>
        /// Retrieves the ID field of manga object from database.
        /// </summary>
        /// <param name="model">Manga object to retrieve ID column from database.</param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        private void LoadMangaID(IMangaModel model, bool acquireLock)
        {
            if(acquireLock)
            {
                using (_locker.AcquireReadLock())
                {
                    _LoadMangaID(model);
                }
            }
            else
            {
                _LoadMangaID(model);
            }
        }
        /// <summary>
        /// Retrieves the ID field of manga object from database.
        /// </summary>
        /// <param name="model">Manga object to retrieve ID column from database.</param>
        private void _LoadMangaID(IMangaModel model)
        {
            try
            {
                OpenConnection();

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = QUERY_GET_MANGA_ID;
                    command.Parameters.AddWithValue("@mangaName", model.Name);
                    command.Parameters.AddWithValue("@publisherName", model.Publisher.ToString());

                    model.Id = (long)command.ExecuteScalar();
                }
            }
            catch (SQLiteException e)
            {
                throw e;
            }
            finally
            {
                CloseConnection();
            }
        }
        /// <summary>
        /// Retreives the ID column of given publisher.
        /// </summary>
        /// <param name="name">Name of the publisher to lookup from database.</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        private int LoadPublisherID(String name, bool closeConnection)
        {
            return LoadPublisherID(name, closeConnection, true);
        }
        /// <summary>
        /// Retreives the ID column of given publisher.
        /// </summary>
        /// <param name="name">Name of the publisher to lookup from database.</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        /// <returns></returns>
        private int LoadPublisherID(String name, bool closeConnection, bool acquireLock)
        {
            int id;
            if (acquireLock)
            {
                using (_locker.AcquireReadLock())
                {
                    id = _LoadPublisherID(name, closeConnection);
                }
            }
            else
            {
                id = _LoadPublisherID(name, closeConnection);
            }
            return id;
        }
        /// <summary>
        /// Retreives the ID column of given publisher.
        /// </summary>
        /// <param name="name">Name of the publisher to lookup from database.</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        private int _LoadPublisherID(String name, bool closeConnection)
        {
            int id;
            try
            {
                OpenConnection();

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = QUERY_GET_PUBLISHER_ID;
                    command.Parameters.AddWithValue("@name", name);
                    id = (int)command.ExecuteScalar();
                }
            }
            catch(SQLiteException e)
            {
                throw e;
            }
            finally
            {
                if (closeConnection)
                    CloseConnection();
            }
            return id;
        }
        /// <summary>
        /// Retrieves the genres from database.
        /// </summary>
        /// <param name="closeConnection">Flag that determines if connection 
        /// should be closed after add operation is done.</param>
        /// <returns>Dictionary which contains genre names as keys and id of that genre in database as value.</returns>
        private Dictionary<String, long> _LoadGenres(bool closeConnection)
        {
            OpenConnection();

            Dictionary<String, long> genres = new Dictionary<string, long>();

            using(SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = QUERY_GET_GENRE;

                using(SQLiteDataReader reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        object[] row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        genres.Add(row[1].ToString(), (long)row[0]);
                    }
                }
            }

            if (closeConnection)
                CloseConnection();

            return genres;
        }

        #endregion

        #region Add Queries

        /// <summary>
        /// Adds a given manga to database. If it already exists in database, it will be ignored.
        /// </summary>
        /// <param name="model">Manga which will be added to database.</param>
        /// <param name="behaviour">Behaviour enum which points either minimal information of 
        /// manga or all details of manga should be added to database.</param>
        public void Add(MangaModel model, AddBehaviour behaviour)
        {
#if DEBUG
            Console.WriteLine("Adding manga \"{0}\" to database.", model.Name);
#endif
            Add(model, behaviour, true);
        }
        /// <summary>
        /// Adds a given manga to database. If it already exists in database, it will be ignored.
        /// </summary>
        /// <param name="model">Manga which will be added to database.</param>
        /// <param name="behaviour">Behaviour enum which points either minimal information of 
        /// manga or all details of manga should be added to database.</param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        private void Add(MangaModel model, AddBehaviour behaviour, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireWriteLock())
                {
                    _Add(model, behaviour);
                }
            }
            else
            {
                _Add(model, behaviour);
            }
        }
        /// <summary>
        /// Adds a given manga to database. If it already exists in database, it will be ignored.
        /// </summary>
        /// <param name="model">Manga which will be added to database.</param>
        /// <param name="behaviour">Behaviour enum which points either minimal information of 
        /// manga or all details of manga should be added to database.</param>
        private void _Add(MangaModel model, AddBehaviour behaviour)
        {
            long authorId, artistId, publisherId;
            OpenConnection();

            using (SQLiteCommand command = connection.CreateCommand())
            {
                if (behaviour == AddBehaviour.Normal)
                {
                    command.CommandText = QUERY_ADD_MANGA_WITH_NAME_RET;
                    
                }
                else if(behaviour == AddBehaviour.Extended)
                {
                    command.CommandText = QUERY_ADD_MANGA_WITH_NAME_EX;
                    // add author and artist if not existing and get id
                    artistId = _AddPerson(model.Artist, false);
                    authorId = _AddPerson(model.Author, false);
                    publisherId = _LoadPublisherID(model.Publisher.ToString(), false);                    
                }
                command.Parameters.AddWithValue("@publisherName", model.Publisher.ToString());
                command.Parameters.AddWithValue("@name", model.Name);
                command.Parameters.AddWithValue("@url", model.Link);

                model.Id = (long)command.ExecuteScalar();
            }
            if(behaviour == AddBehaviour.Extended && model.Genres != null) // Add genres if needed
            {
                _AddGenres(model, false);
            }

            CloseConnection();
        }

        /// <summary>
        /// Adds or ignores given person to the PERSON table. 
        /// Note operation must be done outside of this method.
        /// </summary>
        /// <param name="name">Name of the person.</param>
        /// <param name="closeConnection">Flag that determines if connection 
        /// should be closed after add operation is done.</param>
        /// <returns></returns>
        private long _AddPerson(String name, bool closeConnection)
        {
#if DEBUG
            Console.WriteLine("Adding new Person \"{0}\" to database.", name);
#endif
            long id;

            OpenConnection();

            using(SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = QUERY_ADD_PERSON_RET;
                command.Parameters.AddWithValue("@name", name);
                object obj = command.ExecuteScalar();
                id = (long)obj;
            }

            if (closeConnection)
                CloseConnection();
            
            return id;
        }

        public void AddRange(ThreadSafeObservableCollection<MangaModel> model, Publisher publisher, CancellationToken token)
        {
#if DEBUG
            Console.WriteLine("Adding a collection of manga to database.");
#endif
            AddRange(model, publisher, token, true);
        }

        public void AddRange(ThreadSafeObservableCollection<MangaModel> model, Publisher publisher, CancellationToken token, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireReadLock())
                {
                    _AddRange(model, publisher, token);
                }
            }
            else
            {
                _AddRange(model, publisher, token);
            }
        }

        private void _AddRange(ThreadSafeObservableCollection<MangaModel> model, Publisher publisher, CancellationToken token)
        {
            lock (model)
            {
                SQLiteTransaction transaction = null;
                SQLiteCommand command = null;
                Dictionary<String, long> dict = new Dictionary<String, long>();
                try
                {
                    OpenConnection();                        

                    token.ThrowIfCancellationRequested();

                    using (transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        using (command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = QUERY_ADD_MANGA_WITH_NAME;

                            SQLiteParameter param_Publisher = new SQLiteParameter("@publisherName", DbType.String);
                            SQLiteParameter param_Name = new SQLiteParameter("@name", DbType.String);
                            SQLiteParameter param_Url = new SQLiteParameter("@url", DbType.String);

                            command.Parameters.Add(param_Publisher);
                            command.Parameters.Add(param_Name);
                            command.Parameters.Add(param_Url);

                            int size = model.Count;
                            for (int i = 0; i < size; i++)
                            {
                                token.ThrowIfCancellationRequested();
                                command.Parameters[0].Value = model[i].Publisher.ToString();
                                command.Parameters[1].Value = model[i].Name;
                                command.Parameters[2].Value = model[i].Link;
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                    }
                    transaction = null;
                    command = null;
                    using(command = connection.CreateCommand())
                    {
                        command.CommandText = QUERY_GET_MANGA_ID_LIST;
                        command.Parameters.AddWithValue("@publisherName", publisher.ToString());
                        SQLiteDataReader reader = command.ExecuteReader();
                        while(reader.Read())
                        {
                            object[] row = new object[reader.FieldCount];
                            reader.GetValues(row);
                            String key = row[2].ToString(); // URL of manga
                            if(!dict.ContainsKey(key))
                                dict.Add(key, (long)row[0]); // ID of manga
                        }
                    }
                    command = null;
                    if(dict.Count > 0)
                        for(int i = 0; i < model.Count; i++)
                        {
                            model[i].Id = dict[model[i].Link];
                        }
                }
                catch (OperationCanceledException)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                        transaction = null;
                    }
                }
                finally
                {
                    if(command != null)
                    {
                        command.Dispose();
                        command = null;
                    }
                    CloseConnection();
                }
            }
        }

        /// <summary>
        /// Adds the given manga to favourites.
        /// </summary>
        /// <param name="model">Manga model to add.</param>
        public void AddFavourite(MangaModel model)
        {
#if DEBUG
            Console.WriteLine("Adding \"{0}\" to favourites.", model.Name);
#endif
            AddFavourite(model, true);
        }

        private void AddFavourite(MangaModel model, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireWriteLock())
                {
                    _AddFavourite(model);
                }
            }
            else
            {
                _AddFavourite(model);
            }
        }

        private void _AddFavourite(MangaModel model)
        {
            SQLiteTransaction transaction = null;
            SQLiteCommand command = null;
            Dictionary<String, int> genres = new Dictionary<string, int>();
            OpenConnection();

            try
            {
                transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                // Add genres
                using(command = connection.CreateCommand())
                {
                    command.CommandText = QUERY_ADD_GENRE;
                    SQLiteParameter param = new SQLiteParameter("@name", DbType.String);

                    int size = model.Genres.Length;
                    for(int i = 0; i < size; i++)
                    {
                        command.Parameters[0].Value = model.Genres[i];
                        int id = (int)command.ExecuteScalar();
                        genres.Add(model.Genres[i], id);
                    }
                }
                // Bind genres to manga
                using (command = connection.CreateCommand())
                {
                    command.CommandText = QUERY_ADD_MANGA_GENRE;
                    command.Parameters.AddWithValue("@mangaID", model.Id);
                    SQLiteParameter param = new SQLiteParameter("@genreID");
                    command.Parameters.Add(param);
                    for(int i = 0; i < genres.Count; i++)
                    {
                        command.Parameters[1].Value = genres[model.Genres[i]];
                        command.ExecuteNonQuery();
                    }
                }
                // Add description
                using(command = connection.CreateCommand())
                {
                    command.CommandText = QUERY_ADD_DESCRIPTION;
                    command.Parameters.AddWithValue("@mangaID", model.Id);
                    command.Parameters.AddWithValue("@description", model.Description);
                }
                // Add favourite
                using(command = connection.CreateCommand())
                {
                    command.CommandText = QUERY_ADD_FAVOURITE;
                    command.Parameters.AddWithValue("@mangaID", model.Id);
                    command.Parameters.AddWithValue("@coverURL", model.ImageSource.ToString());
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                connection = null;                
            }
            catch(OperationCanceledException)
            {
                if(transaction != null)
                {
                    transaction.Rollback();
                }
            }
            catch(SQLiteException e)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw e;
            }
            finally
            {
                if(command != null)
                {
                    command.Dispose();
                    command = null;
                }
                if(transaction != null)
                {
                    transaction.Dispose();
                    transaction = null;
                }
                CloseConnection();
            }
        }

        /// <summary>
        /// Adds new chapter to the database.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="behaviour"></param>
        public void Add(ChapterModel model, AddBehaviour behaviour)
        {
#if DEBUG
            Console.WriteLine("Adding Chapter \"{0}\" to database.", model.Name);
#endif
            Add(model, behaviour, true);
        }

        private void Add(ChapterModel model, AddBehaviour behaviour, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireWriteLock())
                {
                    _Add(model, behaviour);
                }
            }
            else
            {
                _Add(model, behaviour);
            }
        }

        private void _Add(ChapterModel model, AddBehaviour behaviour)
        {
            OpenConnection();

            using(SQLiteCommand command = connection.CreateCommand())
            {
                if(behaviour == AddBehaviour.Normal)
                {
                    command.CommandText = QUERY_ADD_CHAPTER;
                }
                else if(behaviour == AddBehaviour.Extended)
                {
                    command.Parameters.AddWithValue("@size", model.Size);
                    command.Parameters.AddWithValue("@path", model.Path);
                }
                command.Parameters.AddWithValue("@mangaID", model.Parent.Id);
                command.Parameters.AddWithValue("@name", model.Name);
                command.Parameters.AddWithValue("@url", model.Link);
                command.Parameters.AddWithValue("@date", model.Date);

                long id = (long)command.ExecuteScalar();
                model.Id = id;
            }

            CloseConnection();
        }

        public void AddRange(ThreadSafeObservableCollection<IChapterModel> model, AddBehaviour behaviour, CancellationToken token)
        {
            AddRange(model, behaviour, token, true, true);
        }

        private void AddRange(ThreadSafeObservableCollection<IChapterModel> model, AddBehaviour behaviour, CancellationToken token, bool closeConnection, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireWriteLock())
                {
                    _AddRange(model, behaviour, token, closeConnection);
                }
            }
            else
            {
                _AddRange(model, behaviour, token, closeConnection);
            }
        }

        private void _AddRange(ThreadSafeObservableCollection<IChapterModel> model, AddBehaviour behaviour, CancellationToken token, bool closeConnection)
        {
            SQLiteTransaction transaction = null;
            SQLiteCommand command = null;
            
            lock(model)
            {
                try
                {
                    OpenConnection();
                    //transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                    using (command = connection.CreateCommand())
                    {
                        command.Parameters.Add("@mangaID");
                        command.Parameters.Add("@name", DbType.String);
                        command.Parameters.Add("@url");
                        command.Parameters.Add("@date");

                        if (behaviour == AddBehaviour.Normal)
                        {
                            command.CommandText = QUERY_ADD_CHAPTER;
                            for (int i = 0; i < model.Count; i++)
                            {
                                command.Parameters[0].Value = model[i].Parent.Id;
                                command.Parameters[1].Value = model[i].Name;
                                command.Parameters[2].Value = model[i].Link;
                                command.Parameters[3].Value = model[i].Date;
                                long id = (long)command.ExecuteScalar();
                                model[i].Id = id;
                            }
                        }
                        else
                        {
                            command.CommandText = QUERY_ADD_CHAPTER_EX;
                            command.Parameters.Add("@size");
                            command.Parameters.Add("@path");

                            for (int i = 0; i < model.Count; i++)
                            {
                                command.Parameters[0].Value = model[i].Parent.Id;
                                command.Parameters[1].Value = model[i].Name;
                                command.Parameters[2].Value = model[i].Link;
                                command.Parameters[3].Value = model[i].Date;
                                command.Parameters[4].Value = model[i].Size;
                                command.Parameters[5].Value = model[i].Path;
                                long id = (long)command.ExecuteScalar();
                                model[i].Id = id;
                            }
                        }
                    }
                    using(command = connection.CreateCommand()) // get manga id's
                    {
                        
                    }
                }
                catch(OperationCanceledException e)
                {
                    if (transaction != null)
                        transaction.Rollback();
                }
                catch(SQLiteException e)
                {
                    if (transaction != null)
                        transaction.Rollback();
                    throw e;
                }
                finally
                {
                    if (closeConnection)
                        CloseConnection();
                }
            }
        }
        /// <summary>
        /// Inserts the given genres into the GENRE table of database.
        /// </summary>
        /// <param name="genres"></param>
        /// <param name="closeConnection">Flag that determines if connection 
        /// should be closed after add operation is done.</param>
        private void _AddGenres(String[] genres, bool closeConnection)
        {
            if (genres == null)
                return;
            SQLiteTransaction transaction = null;
            SQLiteCommand command = null;
            OpenConnection();
            try
            {
                using (transaction = connection.BeginTransaction())
                {
                    using (command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = QUERY_ADD_GENRE;
                        SQLiteParameter param = new SQLiteParameter("@name");
                        command.Parameters.Add(param);
                        for (int i = 0; i < genres.Length; i++)
                        {
                            command.Parameters[0].Value = genres[i];
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                command = null;
                transaction = null;
            }
            catch (SQLiteException e)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    transaction = null;
                }
                if (command != null)
                {
                    command.Dispose();
                    command = null;
                }
                throw e;
            }
            finally
            {
                if (closeConnection)
                    CloseConnection();
            }
        }
        /// <summary>
        /// Inserts genres of given manga to database. Also does the binding between tables.
        /// </summary>
        /// <param name="model">Manga which contains genres that will be added to database.</param>
        /// <param name="closeConnection">Flag that determines if connection 
        /// should be closed after add operation is done.</param>
        private void _AddGenres(MangaModel model, bool closeConnection)
        {
            if (model.Genres == null)
                return;

            OpenConnection();

            SQLiteTransaction transaction = null;
            SQLiteCommand command = null;

            command = null;
            try
            {
                _AddGenres(model.Genres, false);
                Dictionary<String, long> genres = _LoadGenres(false);

                using (transaction = connection.BeginTransaction())
                {
                    using(command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = QUERY_ADD_MANGA_GENRE;
                        command.Parameters.AddWithValue("@mangaID", model.Id);
                        SQLiteParameter param = new SQLiteParameter("@genreID");
                        command.Parameters.Add(param);

                        for (int i = 0; i < model.Genres.Length; i++ )
                        {
                            command.Parameters[1].Value = genres[model.Genres[i]];
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
                command = null;
                transaction = null;
            }
            catch(SQLiteException)
            {
                if(transaction != null) 
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    transaction = null;
                }
                if(command != null)
                {
                    command.Dispose();
                    command = null;
                }
            }

            if (closeConnection)
                CloseConnection();
        }

        #endregion

        #region Update Queries
        // <summary>
        /// Updates the given manga fields to database.
        /// </summary>
        /// <param name="model">Manga object to reflect changes in database.</param>
        public void Update(MangaModel model)
        {
            Update(model, true, true);
        }
        /// <summary>
        /// Updates the given manga fields to database.
        /// </summary>
        /// <param name="model">Manga object to reflect changes in database.</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        private void Update(MangaModel model, bool closeConnection, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireWriteLock())
                {
                    _Update(model, closeConnection);
                }
            }
            else
            {
                _Update(model, closeConnection);
            }
        }
        /// <summary>
        /// Updates the given manga fields to database.
        /// </summary>
        /// <param name="model">Manga object to reflect changes in database.</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        private void _Update(MangaModel model, bool closeConnection)
        {
            long authorId = -1, artistId = -1;
            int flag = 0x00;
            StringBuilder builder = new StringBuilder();
            builder.Append(QUERY_UPDATE_MANGA_PART_START).Append(" ");
            if (!String.IsNullOrWhiteSpace(model.Author))
            {
                builder.Append(QUERY_UPDATE_MANGA_PART_AUTHOR).Append(", ");
                flag |= 0x01;
                authorId = _AddPerson(model.Author, false);
            }
            if (!String.IsNullOrWhiteSpace(model.Artist))
            {
                builder.Append(QUERY_UPDATE_MANGA_PART_ARTIST).Append(", ");
                flag |= 0x02;
                artistId = _AddPerson(model.Artist, false);
            }
            if (!String.IsNullOrWhiteSpace(model.Year))
            {
                builder.Append(QUERY_UPDATE_MANGA_PART_RELEASE).Append(", ");
                flag |= 0x04;
            }
            if(model.Status != -1) 
            {
                builder.Append(QUERY_UPDATE_MANGA_PART_STATUS).Append(", ");
                flag |= 0x08;
            }
            if(model.Genres != null)
            {
                _AddGenres(model, false);
            }
            builder.Length--;
            builder.Length--;
            builder.Append(" ");
            builder.Append(QUERY_UPDATE_MANGA_PART_END);
            OpenConnection();
            
            
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = builder.ToString();
                command.Parameters.AddWithValue("@mangaID", model.Id);
                if ((flag & 0x01) == 0x01)
                    command.Parameters.AddWithValue("@authorID", authorId);
                if ((flag & 0x02) == 0x02)
                    command.Parameters.AddWithValue("@artistID", artistId);
                if ((flag & 0x04) == 0x04)
                    command.Parameters.AddWithValue("@release", model.Year);
                if((flag & 0x08) == 0x08)
                    command.Parameters.AddWithValue("@status", model.Status);

                command.ExecuteNonQuery();
            }

            if(closeConnection)
                CloseConnection();
        }

        /// <summary>
        /// Updates the chapter and reflects the changes to database.
        /// </summary>
        /// <param name="model">Chapter to update changes from.</param>
        public void Update(IChapterModel model)
        {
            Update(model, true, true);
        }
        /// <summary>
        /// Updates the chapter and reflects the changes to database.
        /// </summary>
        /// <param name="model">Chapter to update changes from.</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        private void Update(IChapterModel model, bool closeConnection, bool acquireLock)
        {
            if (acquireLock)
            {
                using (_locker.AcquireWriteLock())
                {
                    _Update(model, closeConnection);
                }
            }
            else
            {
                _Update(model, closeConnection);
            }
        }

        /// <summary>
        /// Updates the chapter and reflects the changes to database.
        /// </summary>
        /// <param name="model">Chapter to update changes from</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        private void _Update(IChapterModel model, bool closeConnection)
        {
            int flag = 0x00;
            StringBuilder builder = new StringBuilder();

            builder.Append(QUERY_UPDATE_CHAPTER_PART_START).Append(" ");
            if(model.Size != -1)
            {
                builder.Append(QUERY_UPDATE_CHAPTER_PART_SIZE);
                flag |= 0x0001;
            }
            if(!String.IsNullOrWhiteSpace(model.Date))
            {
                builder.Append(QUERY_UPDATE_CHAPTER_PART_DATE).Append(" ");
                flag |= 0x0002;
            }
            if(!String.IsNullOrWhiteSpace(model.Path))
            {
                builder.Append(QUERY_UPDATE_CHAPTER_PART_PATH).Append(" ");
                flag |= 0x0004;
            }
            if(model.Index != -1)
            {
                builder.Append(QUERY_UPDATE_CHAPTER_PART_ORDER).Append(" ");
                flag |= 0x0008;
            }
            if(model.IsDownloaded != -1)
            {
                builder.Append(QUERY_UPDATE_CHAPTER_PART_ISDOWNLOADED).Append(" ");
                flag |= 0x0010;
            }
            if(model.IsRead != -1)
            {
                builder.Append(QUERY_UPDATE_CHAPTER_PART_ISREAD).Append(" ");
                flag |= 0x0020;
            }
            builder.Length--;
            builder.Append(QUERY_UPDATE_CHAPTER_PART_END);

            OpenConnection();
            using(SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = builder.ToString();
                if ((flag & 0x0001) == 0x0001)
                    command.Parameters.AddWithValue("@size", model.Size);
                if ((flag & 0x0002) == 0x0002)
                    command.Parameters.AddWithValue("@date", model.Date);
                if ((flag & 0x0004) == 0x0004)
                    command.Parameters.AddWithValue("@path", model.Path);
                if ((flag & 0x0008) == 0x0008)
                    command.Parameters.AddWithValue("@order", model.Index);
                if ((flag & 0x0010) == 0x0010)
                    command.Parameters.AddWithValue("@isDownloaded", model.IsDownloaded);
                if ((flag & 0x0020) == 0x0020)
                    command.Parameters.AddWithValue("@isRead", model.IsRead);

                command.ExecuteNonQuery();
            }
            if(closeConnection)
                CloseConnection();
        }

        #endregion

        #region Delete Queries
        /// <summary>
        /// Removes the given manga object from favourites from database.
        /// </summary>
        /// <param name="model">Manga object to remove from favourites table in database.</param>
        public void RemoveFavourite(IMangaModel model)
        {
            RemoveFavourite(model, true, true);
        }
        /// <summary>
        /// Removes the given manga object from favourites from database.
        /// </summary>
        /// <param name="model">Manga object to remove from favourites table in database.</param>
        /// <param name="model"></param>
        /// <param name="acquireLock">Flag for locking database or not for current operation. 
        /// Useful for nested operations where lock is acquired already thus avoiding deadlock.</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        private void RemoveFavourite(IMangaModel model, bool acquireLock, bool closeConnection)
        {
            if (acquireLock)
            {
                using (_locker.AcquireWriteLock())
                {
                    _RemoveFavourite(model, closeConnection);
                }
            }
            else
            {
                _RemoveFavourite(model, closeConnection);
            }
        }
        /// <summary>
        /// Removes the given manga object from favourites from database.
        /// </summary>
        /// <param name="model">Manga object to remove from favourites table in database.</param>
        /// <param name="closeConnection">Flag for closing connection to database or not, 
        /// after current operation is completed. Useful for nested operations where multiple operations 
        /// are done sequentially to increase performance.</param>
        private void _RemoveFavourite(IMangaModel model, bool closeConnection)
        {
            OpenConnection();

            using(SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = QUERY_DELETE_FAVOURITE;
                command.Parameters.AddWithValue("@mangaID", model.Id);
                command.ExecuteNonQuery();
            }

            if (closeConnection)
                CloseConnection();
        }

        #endregion

        #region Database connectivity

        /// <summary>
        /// Connects to the database.
        /// <returns>True if new database is created, false otherwise.</returns>
        /// </summary>
        bool OpenConnection()
        {
            // check if connection is created or not
            // if created then return, no need to create another connection
            // otherwise continue
            if (connection != null)
                if (connection.State == ConnectionState.Open)
                    return false;

            // Check if database file exists
            if (File.Exists("manga.db3"))
            {
                // if it exists open the connection
                connection = new SQLiteConnection(CONNECTION_STRING);
                // Open the connection
                connection.Open();
                return false;
            }
            // Create if database file does not exists then open the connection
            connection = new SQLiteConnection(CONNECTION_STRING_NEW);
            // Open the connection
            connection.Open();
            return true;
        }

        void CloseConnection()
        {
            if (connection != null)
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                    connection = null;
                }            
        }

        #endregion

        #region Execute Query

        void ExecuteQuery(String query)
        {
            // establish connection to the database
            OpenConnection();

            // create command
            using (SQLiteCommand command = connection.CreateCommand())
            {
                // set command query from 'query' parameter
                command.CommandText = query;
                // execute command
                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Destructor & Dispose

        public void Dispose()
        {

        }

        #endregion
    }

    public enum AddBehaviour
    {
        Normal,
        Extended
    };
}
