using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Manga.Framework
{
    public interface IPrioritizable : INotifyPropertyChanged
    {
        Priority Priority { get; set; }
        int Index { get; set; }
    }
}
