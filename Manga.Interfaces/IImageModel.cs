using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga.Interfaces
{
    public interface IImageModel
    {
        String Link { get; set; }
        LinkType LinkType { get; set; }
        bool IsNullOrEmpty();
    }

    public enum LinkType
    {
        External,
        Local
    };
}
