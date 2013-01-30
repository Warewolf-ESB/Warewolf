#region

using System;
using System.Collections.ObjectModel;

#endregion

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
                        throw new InvalidOperationException("Cannot insert duplicated items");
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