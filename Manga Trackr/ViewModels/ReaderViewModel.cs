using Manga.Framework;
using Manga.Interfaces;
using Manga.Servers.Workers;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Manga_Trackr.ViewModels
{
    /// <summary>
    /// Old
    /// </summary>
    public class ReaderViewModel : ObservableObject
    {
        #region Members

        ReaderModel _model;

        int _positionChapter;
        int _positionImage;

        ThreadSafeObservableCollection<String> _images;
        ThreadSafeObservableCollection<String> _chapters;

        private PriorityQueue<PriorityQueue<IPrioritizable, Priority>, Priority> _downloadQueue;

        ReaderWorker _worker;

        #endregion

        #region Properties

        public ReaderModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                RaisePropertyChanged("Model");
            }
        }

        public int PositionImage
        {
            get { return _positionImage; }
            set
            {
                _positionImage = value;
                RaisePropertyChanged("PositionImage");
                RaisePropertyChanged("CurrentImage");
            }
        }

        public Manga.Interfaces.IChapterModel CurrentChapter
        {
            get
            {
                return (Model.Items.Count > _positionChapter) ? Model.Items[_positionChapter] ?? ChapterModel.Empty : ChapterModel.Empty;
            }
        }

        public IImageModel CurrentImage
        {
            get { return _positionImage == -1 ? null : CurrentChapter.Items[_positionImage]?? null; }
        }

        public int PositionChapter
        {
            get { return _positionChapter; }
            set
            {
                if (_positionChapter != value)
                    _positionImage = 0;
                _positionChapter = value;
                _worker.Cancel();
                _worker.Load(_positionChapter);
                RaisePropertyChanged("PositionChapter");
                RaisePropertyChanged("Images");
                RaisePropertyChanged("PositionImage");
                RaisePropertyChanged("PositionChapter");
                RaisePropertyChanged("Size");
            }
        }

        public ThreadSafeObservableCollection<String> Images
        {
            get
            {
                _images.Clear();
                for(int i = 0; i < CurrentChapter.Size; i++)
                    _images.Add((i+1).ToString());
                return _images;
            }
        }

        public ThreadSafeObservableCollection<String> Chapters
        {
            get 
            {
                _chapters.Clear();
                _chapters.AddRange(Model.Items.Select(c => c.Name).ToList());
                return _chapters;
            }
        }

        public int Size
        {
            get
            {
                return CurrentChapter.Size;
            }
        }

        public PriorityQueue<PriorityQueue<IPrioritizable, Priority>, Priority> DownloadQueue
        {
            get { return _downloadQueue; }
        }


        #endregion

        #region Constructor

        public ReaderViewModel(MangaModel model)
        {
            _model = new ReaderModel();
            _model.Name = model.Name;
            _model.Link = model.Link;
            _model.Id = model.Id;
            _model.Publisher = model.Publisher;

            _positionChapter = -1;
            _positionImage = -1;

            

            _images = new ThreadSafeObservableCollection<String>();
            _chapters = new ThreadSafeObservableCollection<String>();

            _worker = new ReaderWorker(_model);
            _worker.Load(0);
            _worker.Start();
        }

        void _image_DownloadCompleted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Destructor

        ~ReaderViewModel()
        {
            _model = null;
            _worker.Stop();
        }

        #endregion

        #region Commands

        #region Prevoius Command

        void Previous()
        {
            if (_positionImage == 0)
            {
                _positionChapter--;
                _positionImage = 0;
                _images.Clear();
                RaisePropertyChanged("CurrentChapter");
                RaisePropertyChanged("CurrentImage");
                RaisePropertyChanged("PositionChapter");
                RaisePropertyChanged("Images");
                RaisePropertyChanged("Size");
            }
            else
            {
                PositionImage--;
            }
        }

        bool canPrevious()
        {
            if (_positionChapter == 0)
            {
                if (_positionImage == 0) return false;
                else return true;
            }
            else
            {
                return true;
            }
        }

        public ICommand PreviousCommand { get { return new RelayCommand(Previous, canPrevious); } }

        #endregion

        #region Next Command

        void Next()
        {
            if (_positionImage == CurrentChapter.Size - 1)
            {
                _positionChapter++;
                _positionImage = 0;
                _images.Clear();
                RaisePropertyChanged("CurrentChapter");
                RaisePropertyChanged("CurrentImage");
                RaisePropertyChanged("PositionChapter");
                RaisePropertyChanged("Images");
                RaisePropertyChanged("Size");
            }
            else
            {
                PositionImage++;
            }
        }

        bool canNext()
        {
            if (Model.Count > 1)
            {
                if (_positionChapter == Model.Count - 1)
                {
                    if (_positionImage == CurrentChapter.Size - 1)
                        return false;
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                return false;
        }

        public ICommand NextCommand { get { return new RelayCommand(Next, canNext); } }

        #endregion

        #endregion
    }
}
