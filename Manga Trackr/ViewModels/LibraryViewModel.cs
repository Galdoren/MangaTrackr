using Manga.Framework;
using Manga_Trackr.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Manga_Trackr.ViewModels
{
    public class LibraryViewModel : ObservableObject
    {
        #region Members

        AllPage _allPage;
        FavouritesPage _favouritesPage;

        #endregion

        #region Properties

        public TabItemData[] Tabs { get; private set; }

        #endregion

        #region Constructor

        public LibraryViewModel()
        {
            Tabs = new TabItemData[2];

            _allPage = new AllPage();
            _favouritesPage = new FavouritesPage();

            Tabs[0] = new TabItemData() { Header = "All", Content = _allPage, ResourceKey = "All" };
            Tabs[1] = new TabItemData() { Header = "Favourites", Content = _favouritesPage, ResourceKey = "Favs" };
        }

        #endregion

        #region Destructor & Dispose

        #endregion

        #region Download Command

        #endregion
    }
}
