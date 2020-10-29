using Manga.Framework;
using Manga.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga.Structures.Models
{
    public class ImageModel : ObservableObject, IImageModel
    {
        #region Members

        String _link;
        LinkType _linkType;

        #endregion

        #region Properties

        public String Link
        {
            get { return _link; }
            set
            {
                _link = value;
                RaisePropertyChanged("Link");
            }
        }

        public LinkType LinkType
        {
            get { return _linkType; }
            set
            {
                _linkType = value;
                RaisePropertyChanged("LinkType");
            }
        }

        #endregion

        #region Constructor

        public ImageModel()
        {
            Link = String.Empty;
            LinkType = LinkType.External;
        }

        #endregion

        #region Methods

        public bool IsNullOrEmpty()
        {
            return String.IsNullOrEmpty(Link);
        }

        #endregion
    }

    
}
