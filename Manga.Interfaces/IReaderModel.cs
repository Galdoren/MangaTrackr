using Manga.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga.Interfaces
{
    public interface IReaderModel : IMangaModel
    {
        ICollection<IPrioritizable> PriorityQueue { get; set; }
    }
}
