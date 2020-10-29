using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Manga.Framework
{
    public interface IPriorityQueue<TValue, TPriority> : IEnumerable<TValue>, INotifyCollectionChanged, IPrioritizable
        where TValue : INotifyPropertyChanged, IPrioritizable
    {
        int Count { get; set; }
        bool IsEmpty { get; set; }
        int Size { get; set; }
        int Index { get; set; }
        Priority Priority { get; set; }

        void Enqueue(TValue value, TPriority priority);

        TValue Dequeue();
        bool TryDequeue(out TValue value);
        TValue Peek();

        void Clear();

        IEnumerator<TValue> GetValue();

        event NotifyCollectionChangedEventHandler CollectionChanged;
        void OnCollectionChanged(NotifyCollectionChangedEventArgs e);

    }
}
