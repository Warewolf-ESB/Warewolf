using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Dev2
{
    public class ObservableReadOnlyList<T> : IList<T>, IObservableReadOnlyList<T>
    {
        readonly ObservableCollection<T> _list;

        #region CTOR

        public ObservableReadOnlyList()
        {
            _list = new ObservableCollection<T>();
            InitCollectionChanged();
        }

        public ObservableReadOnlyList(IEnumerable<T> collection)
        {
            _list = new ObservableCollection<T>(collection);
            InitCollectionChanged();
        }

        public ObservableReadOnlyList(List<T> list)
        {
            _list = new ObservableCollection<T>(list);
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
            _list.CollectionChanged += (sender, args) =>
            {
                if(CollectionChanged != null)
                {
                    CollectionChanged(this, args);
                }
            };
        }

        #endregion

    }
}
