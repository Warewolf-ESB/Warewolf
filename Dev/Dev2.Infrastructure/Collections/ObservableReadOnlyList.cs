
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using Dev2.Common.Interfaces.Core.Collections;

namespace Dev2.Collections
{
    public class ObservableReadOnlyList<T> : IList<T>, IObservableReadOnlyList<T>
    {
        readonly ObservableCollection<T> _list;
        readonly Dispatcher _dispatcher;

        #region CTOR

        public ObservableReadOnlyList()
            : this((IEnumerable<T>)null)
        {
        }

        public ObservableReadOnlyList(List<T> list)
            : this((IEnumerable<T>)list)
        {
        }

        public ObservableReadOnlyList(IEnumerable<T> collection)
        {
            // Save dispatcher so that we always fire CollectionChanged on it's thread
            _dispatcher = Dispatcher.CurrentDispatcher;

            _list = collection == null ? new ObservableCollection<T>() : new ObservableCollection<T>(collection);
            InitCollectionChanged();
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out T>

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public int Count { get { return _list.Count; } }

        public bool IsReadOnly { get { return true; } }

        #endregion

        #region Implementation of IReadOnlyList<out T>

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index] { get { return _list[index]; } set { _list[index] = value; } }

        #endregion

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region InitCollectionChanged

        void InitCollectionChanged()
        {
            // Post the CollectionChanged event on the creator thread
            _list.CollectionChanged += (sender, args) =>
            {
                if(!_dispatcher.CheckAccess())
                {
                    _dispatcher.BeginInvoke(new Action(() => RaiseCollectionChanged(args)), DispatcherPriority.Normal, new object[] { });
                }
                else
                {
                    RaiseCollectionChanged(args);
                }
            };
        }

        internal DispatcherFrame TestDispatcherFrame { get; set; }

        void RaiseCollectionChanged(object param)
        {
            // MUST be called on the dispatcher thread!
            if(CollectionChanged != null)
            {
                CollectionChanged(this, (NotifyCollectionChangedEventArgs)param);
                if(TestDispatcherFrame != null)
                {
                    TestDispatcherFrame.Continue = false;
                }
            }
        }

        #endregion

    }
}
