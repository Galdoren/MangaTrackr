using Manga.Framework;
using Manga.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga.Servers
{
    public class Batoto : ObservableObject, IServer
    {
        #region Members

        #endregion

        #region Properties

        public bool IsRunning
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Constructor

        #endregion

        #region Download

        public void Download(object obj, bool State)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Fetch List

        public void FetchList(object obj, System.Threading.CancellationToken Token)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Info - Info Window

        public void GetInfoExtended(object obj, bool State)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Download

        public void GetInfoMinimal(IMangaModel obj, bool State)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Reader

        public void FindImages(IReaderModel obj, bool State)
        {
            throw new NotImplementedException();
        }

        public void FindImages(IChapterModel obj, bool State)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Update

        public void CheckUpdates(IMangaModel obj, System.Threading.CancellationToken Token)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
