using System;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{
    public class SortedObservableCollection<T> : ObservableCollection<T>
        where T : IComparable
    {
        protected override void InsertItem(int index, T item)
        {
            for(int i = 0; i < Count; i++)
            {
                switch(Math.Sign(this[i].CompareTo(item)))
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