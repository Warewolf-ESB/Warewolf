/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Threading;
using Dev2.Common.Interfaces.Core.Collections;

namespace Dev2.Collections
{
    public class ObservableReadOnlyList<T> : IList<T>, IObservableReadOnlyList<T>
    {
        readonly ObservableCollection<T> _list;
        readonly SynchronizationContext _synchronizationContext;

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
            // Save synchronization context so that we always fire CollectionChanged on its thread
            _synchronizationContext = SynchronizationContext.Current;

            _list = collection == null ? new ObservableCollection<T>() : new ObservableCollection<T>(collection);
            InitCollectionChanged();
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) => _list.Remove(item);

        public int Count => _list.Count;

        public bool IsReadOnly => true;

        #endregion

        #region Implementation of IReadOnlyList<out T>

        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

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
                if (_synchronizationContext != null && _synchronizationContext != SynchronizationContext.Current)
                {
                    _synchronizationContext.Post(_ => RaiseCollectionChanged(args), null);
                }
                else
                {
                    RaiseCollectionChanged(args);
                }
            };
        }

        void RaiseCollectionChanged(object param)
        {
            // MUST be called on the synchronization context thread!
            CollectionChanged?.Invoke(this, (NotifyCollectionChangedEventArgs)param);
        }

        #endregion
    }
}
