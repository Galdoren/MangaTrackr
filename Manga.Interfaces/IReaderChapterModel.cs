using Manga.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Manga.Interfaces
{
    public interface IReaderChapterModel<TValue> : IChapterModel
        where TValue : INotifyPropertyChanged, IPrioritizable
    {
        IEnumerable<TValue> DownloadQueue { get; set; }
    }
}
