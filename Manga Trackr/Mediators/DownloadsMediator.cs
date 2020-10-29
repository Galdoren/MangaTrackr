using Manga.Structures.Models;
using Manga_Trackr.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga_Trackr.Mediators
{
    public class DownloadsMediator
    {
        #region Members

        static readonly DownloadsMediator _instance;
        DownloadQueueViewModel _viewModel;

        private readonly object semaphore;

        #endregion

        #region Properties

        public static DownloadsMediator Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Constructor

        static DownloadsMediator()
        {
            _instance = new DownloadsMediator();
        }

        private DownloadsMediator()
        {
            semaphore = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the instance of DownloadQueueViewModel
        /// </summary>
        /// <param name="viewModel">Object to register</param>
        public void Register(DownloadQueueViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Add the Download Model to the Queue View
        /// </summary>
        /// <param name="model">Model to add</param>
        public void Add(DownloadModel model)
        {
            lock(semaphore)
                _viewModel.Items.Add(model);
        }

        #endregion
    }
}
