using Manga.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Manga.Interfaces
{
    public interface IChapterModel : INotifyPropertyChanged, IPrioritizable
    {
        long Id { get; set; }
        String Name { get; set; }
        String Link { get; set; }
        String Date { get; set; }
        String Volume { get; set; }
        String Path { get; set; }
        IMangaModel Parent { get; set; }
        bool IsSelected { get; set; }
        int Size { get; set; }
        int IsDownloaded { get; set; }
        int IsRead { get; set; }
        ThreadSafeObservableCollection<IImageModel> Items { get; set; }
    }
}
