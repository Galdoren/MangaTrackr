using Manga.Framework;
using Manga.Servers.Workers;
using Manga.Structures.Models;
using Manga_Trackr.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga_Trackr.ViewModels
{
    /// <summary>
    /// Manga Information Window View Model
    /// </summary>
    public class InfoWindowViewModel : ObservableObject, IDisposable
    {
        #region Members
        MangaModel _model;
        bool _isDisposed;
        InfoPage _infoPage;
        ChaptersPage _chaptersPage;
        #endregion

        #region Properties

        public TabItemData[] Tabs { get; private set; }

        public MangaModel Model { get { return _model; } }

        #endregion Properties

        #region Constructor
        public InfoWindowViewModel(MangaModel model)
        {
            _model = model;
            _isDisposed = false;

            _infoPage = new InfoPage(model);
            _chaptersPage = new ChaptersPage(model);

            Tabs = new TabItemData[2];

            Tabs[0] = new TabItemData() { Header = "Information", Content = _infoPage, ResourceKey = "InfP" };
            Tabs[1] = new TabItemData() { Header = "Chapters", Content = _chaptersPage, ResourceKey = "Chps" };

            InfoWorker worker = new InfoWorker(_model, InfoType.Extended);
        }
        #endregion

        #region Dispose

        public void Dispose()
        {
            
        }

        #endregion
    }
}
