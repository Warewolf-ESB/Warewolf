#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Threading;

#endregion

namespace Dev2.Studio.Core
{
    public class SortedObservableCollection<T> : ObservableCollection<T>
        where T : IComparable
    {
        //public override event NotifyCollectionChangedEventHandler CollectionChanged;

        //protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        //{
        //    NotifyCollectionChangedEventHandler eh = CollectionChanged;
        //    if (eh != null)
        //    {
        //        Dispatcher dispatcher = (from NotifyCollectionChangedEventHandler nh in eh.GetInvocationList()
        //                                 let dpo = nh.Target as DispatcherObject
        //                                 where dpo != null
        //                                 select dpo.Dispatcher).FirstOrDefault();

        //        if (dispatcher != null && dispatcher.CheckAccess() == false)
        //        {
        //            dispatcher.Invoke(DispatcherPriority.DataBind, (Action)(() => OnCollectionChanged(e)));
        //        }
        //        else
        //        {
        //            foreach (NotifyCollectionChangedEventHandler nh in eh.GetInvocationList())
        //                nh.Invoke(this, e);
        //        }
        //    }
        //}

        protected override void InsertItem(int index, T item)
        {
            for (int i = 0; i < Count; i++)
            {
                switch (Math.Sign(this[i].CompareTo(item)))
                {
                    case 0:
                        //TODO at least log this!
                        //throw new InvalidOperationException("Cannot insert duplicated items");
                        break;
                    case 1:
                        base.InsertItem(i, item);
                        return;
                    case -1:
                        break;
                }
            }

            base.InsertItem(Count, item);
        }
    }
}