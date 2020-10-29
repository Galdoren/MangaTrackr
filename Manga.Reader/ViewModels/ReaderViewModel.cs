using Manga.Framework;
using Manga.Interfaces;
using Manga.Servers.Workers;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace Manga.Reader.ViewModels
{
    /// <summary>
    /// New
    /// </summary>
    class ReaderViewModel : ObservableObject
    {
        #region Members

        ReaderModel _model;
        ReaderWorker _worker;

        CollectionViewSource _imagesSource;
        CollectionViewSource _chaptersSource;

        private IEnumerator<IChapterModel> _chapter;
        private IEnumerator<IImageModel> _image;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the Reader Model of the manga
        /// </summary>
        public ReaderModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                RaisePropertyChanged("Model");
            }
        }

        /// <summary>
        /// Current Chapter
        /// </summary>
        public IChapterModel Chapter
        {
            get { return _chapter.Current; }
        }

        public IImageModel Image
        {
            get { return _image != null ? _image.Current : default(ImageModel); }
        }

        public CollectionViewSource ImagesSource
        {
            get
            {
                return _imagesSource;
            }
        }

        public CollectionViewSource ChaptersSource
        {
            get { return _chaptersSource; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes
        /// </summary>
        /// <param name="model"></param>
        public ReaderViewModel(IMangaModel model)
        {
            _model = new ReaderModel();
            _model.Name = model.Name;
            _model.Link = model.Link;
            _model.Id = model.Id;
            _model.Publisher = model.Publisher;

            Initialize(0);
        }


        public ReaderViewModel(IMangaModel model, int index)
        {
            _model = new ReaderModel();
            _model.Name = model.Name;
            _model.Link = model.Link;
            _model.Id = model.Id;
            _model.Publisher = model.Publisher;

            Initialize(index);
        }

        void Initialize(int index)
        {
            _chapter = _model.Items.GetEnumerator();
            _chapter.Reset();
            if (_chapter.MoveNext())
            {
                _image = _chapter.Current.Items.GetEnumerator();
                _image.Reset();
                if (_image.MoveNext())
                {

                }
            }

            

            _worker = new ReaderWorker(_model);
            _worker.Start();
            _worker.Load(index);
        }

        void ChapterProgress(IChapterModel model, int index)
        {
            if (!model.Equals(Chapter))
                return;

            RaisePropertyChanged("Image");
        }

        #endregion

        #region Methods

        #endregion

        #region Commands

        #region Prevoius Command

        void Previous()
        {
            
        }

        bool canPrevious()
        {
            return true;
        }

        public ICommand PreviousCommand { get { return new RelayCommand(Previous, canPrevious); } }

        #endregion

        #region Next Command

        void Next()
        {
            
        }

        bool canNext()
        {
            return true;
        }

        public ICommand NextCommand { get { return new RelayCommand(Next, canNext); } }

        #endregion

        #endregion
    }
}
