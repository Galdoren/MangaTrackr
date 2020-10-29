using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace Manga.Framework
{
    public class PriorityQueue<TValue, TPriority> : ObservableObject, IEnumerable<TValue>, INotifyCollectionChanged, IPrioritizable
        where TValue : INotifyPropertyChanged, IPrioritizable
    {
        #region Members

        SpinLock _lock;

        int _count;
        int _size;
        LinkedList<TValue> _values;

        #endregion

        #region Properties

        public int Count
        {
            get { return _count; }
        }

        public bool IsEmpty
        {
            get { return _count == 0; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        #endregion

        #region Constructor

        public PriorityQueue()
        {
            _count = 0;
            _size = 0;
            _lock = new SpinLock(false);
            
            _values = new LinkedList<TValue>();
        }

        #endregion

        #region Destructor

        #endregion

        #region Methods

        public void Enqueue(TValue value, Priority priority)
        {
            bool lockTaken = false;
            _lock.Enter(ref lockTaken);

            LinkedListNode<TValue> item = _values.Last;

            while (item != null && item.Value.Priority.CompareTo(priority) < 0)
                item = item.Previous;

            if (item == null)
                _values.AddLast(value);
            else
            {
                if (_count != 1)
                {
                    while (value.Index > item.Value.Index && item.Value.Priority.CompareTo(priority) <= 0)
                    {
                        if (item.Previous == null)
                            break;
                        item = item.Previous;
                    }
                }
                _values.AddBefore(item, value);
            }                

            _count++;
            
            _lock.Exit();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
        }

        public TValue Dequeue()
        {
            if (_lock.IsHeld)
            {
                return default(TValue);
            }

            TValue value;

            bool lockTaken = false;
            _lock.Enter(ref lockTaken);

            value = _values.Last.Value;
            _values.RemoveLast();

            _count--;
            _lock.Exit();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));

            return value;
        }

        public bool TryDequeue(out TValue value)
        {
            if (_lock.IsHeld)
            {
                value = default(TValue);
                return false;
            }

            bool lockTaken = false;
            _lock.Enter(ref lockTaken);
            if (_values.Last != null)
            {
                value = _values.Last.Value;
                _values.RemoveLast();
                _count--;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
            }
            else
            {
                _lock.Exit();
                value = default(TValue);
                return false;
            }
            
            _lock.Exit();            

            return true;
        }

        public TValue Peek()
        {
            return _values.Last.Value;
        }

        public void Clear()
        {
            bool lockTaken = false;
            _lock.Enter(ref lockTaken);

            _values.Clear();

            _lock.Exit();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
        }

        #endregion

        #region Enumeration

        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        #endregion

        public Priority Priority
        { get; set; }

        public int Index
        { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public enum Priority : short
    {
        Low = 0x1,
        Normal = 0x0,
        High = 0x2,
        Read = 0x3,
        Maximum = 0x4
    }
}
