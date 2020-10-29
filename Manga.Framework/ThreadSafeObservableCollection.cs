using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Manga.Framework
{
    /// <summary>
    /// Thread Safe, Observable Collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        bool _suspendCollectionChangeNotification = false;

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var eh = CollectionChanged;
            if (eh != null)
            {
                if (!_suspendCollectionChangeNotification)
                {
                    Dispatcher dispatcher = (from NotifyCollectionChangedEventHandler nh in eh.GetInvocationList()
                                             let dpo = nh.Target as DispatcherObject
                                             where dpo != null
                                             select dpo.Dispatcher).FirstOrDefault();

                    if (dispatcher != null && dispatcher.CheckAccess() == false)
                    {
                        dispatcher.Invoke(DispatcherPriority.DataBind, (Action)(() => OnCollectionChanged(e)));
                    }
                    else
                    {
                        foreach (NotifyCollectionChangedEventHandler nh in eh.GetInvocationList())
                            nh.Invoke(this, e);
                    }
                }
            }
        }

        public void SuspendCollectionChangeNotification()
        {
            _suspendCollectionChangeNotification = true;
        }

        public void ResumeCollectionChangeNotification()
        {
            _suspendCollectionChangeNotification = false;
        }
        /// <summary>
        /// Add a range of collection, notifies the collection changed after all the items are successfully inserted
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            this.SuspendCollectionChangeNotification();
            try
            {
                foreach (var i in items)
                    base.InsertItem(Count, i);
            }
            finally
            {
                this.ResumeCollectionChangeNotification();

                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                this.OnCollectionChanged(arg);
            }
        }

        public void InsertItem(int index, T item)
        {
            base.Insert(index, item);
        }
    }
}
