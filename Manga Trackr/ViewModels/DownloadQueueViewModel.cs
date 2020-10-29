using Manga.Framework;
using Manga.Structures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    /// <summary>
    /// View-Model for Download Queue Page
    /// </summary>
    public class DownloadQueueViewModel : ObservableObject
    {
        #region Members

        ThreadSafeObservableCollection<DownloadModel> _items;

        CollectionViewSource _itemsSource;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Collection of Download Model
        /// </summary>
        public ThreadSafeObservableCollection<DownloadModel> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Get the CollectionViewSource
        /// </summary>
        public CollectionViewSource ItemsSource
        {
            get { return _itemsSource; }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Creates an Instance of DownloadQueueViewModel object
        /// </summary>
        public DownloadQueueViewModel()
        {
            _items = new ThreadSafeObservableCollection<DownloadModel>();
            _itemsSource = new CollectionViewSource();

            _itemsSource.Source = Items;
        }

        #endregion

        #region Destructor & Dispose

        #endregion

        #region Commands

        void Clear()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (!Items[i].IsBusy)
                    Items.RemoveAt(i);
            }
        }

        public ICommand ClearCommand { get { return new RelayCommand(Clear); } }

        #endregion
    }
}
