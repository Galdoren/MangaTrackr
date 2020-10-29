using Manga.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Manga.Interfaces
{
    public interface IMangaModel
    {
        long Id { get; set; }
        String Name { get; set; }
        String Link { get; set; }
        Publisher Publisher { get; set; }
        ThreadSafeObservableCollection<IChapterModel> Items { get; set; }
        int Count { get; set; }
        int InfoState { get; set; }
        int Status { get; set; }
    }

    public enum Publisher : ushort
    {
        [Description("Mangafox")]
        Mangafox = 0x1,
        [Description("Mangareader")]
        Mangareader = 0x2,
        [Description("Mangapanda")]
        Mangapanda = 0x3
    }

    public static class PublisherExtension
    {
        public static String Address(this Publisher publisher)
        {
            switch (publisher)
            {
                case Publisher.Mangafox:
                    return "http://www.mangafox.me/";
                case Publisher.Mangareader:
                    return "http://www.mangareader.net/";
                case Publisher.Mangapanda:
                    return "http://www.mangapanda.net/";
                default:
                    return null;
            }
        }
    }
}
