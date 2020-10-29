using Manga.Structures.Models;
using Manga_Trackr.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga_Trackr.Mediators
{
    public class FavouritesMediator
    {
        #region Members

        static readonly FavouritesMediator _instance;
        FavouritesViewModel _viewModel;

        readonly object semaphore;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Instance of FavouritesMediator
        /// </summary>
        public static FavouritesMediator Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Constructor

        static FavouritesMediator()
        {
            _instance = new FavouritesMediator();
        }

        private FavouritesMediator()
        {
            semaphore = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the instance of FavouritesViewModel.
        /// </summary>
        /// <param name="viewModel">ViewModel to register.</param>
        public void Register(FavouritesViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Refresh the Favourites View
        /// </summary>
        public void Refresh()
        {
            lock(semaphore)
                _viewModel.RefreshCommand.Execute(null);
        }

        #endregion
    }
}
