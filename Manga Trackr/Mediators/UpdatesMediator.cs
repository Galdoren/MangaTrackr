using Manga_Trackr.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga_Trackr.Mediators
{
    public class UpdatesMediator
    {
        #region Members

        static readonly UpdatesMediator _instance;
        UpdatesViewModel _viewModel;

        readonly object semaphore;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Instance of UpdatesMediator
        /// </summary>
        public static UpdatesMediator Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Constructor

        static UpdatesMediator()
        {
            _instance = new UpdatesMediator();
        }

        private UpdatesMediator()
        {
            semaphore = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the instance of UpdatesViewModel.
        /// </summary>
        /// <param name="viewModel">ViewModel to register.</param>
        public void Register(UpdatesViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Check the Updates
        /// </summary>
        public void Refresh()
        {
            lock (semaphore)
                _viewModel.RefreshCommand.Execute(null);
        }

        #endregion
    }
}
