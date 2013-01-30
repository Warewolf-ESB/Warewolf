using Dev2.Common;
using Dev2.Data.Binary_Objects;
using System;
using System.Collections.Generic;

namespace Dev2.DataList.Contract.Binary_Objects.Structs
{
    [Serializable]
    public struct SBinaryDataListEntry
    {

        //private IDictionary<int, IList<IBinaryDataListItem>> _items;

        private Dev2BinaryDataListDictionary _items;

        public int _appendIndex;

        #region Properties

        public IList<int> PopulatedIndex { get; set; }

        public bool IsEditable { get; set; }

        public enDev2ColumnArgumentDirection ColumnIODirection { get; set; }

        public bool IsRecordset { get; set; }

        public bool IsEvaluationScalar { get; set; }

        public bool IsManagmentServicePayload { get; set; }

        public IList<Dev2Column> Columns { get; set; }

        public string Namespace { get; set; }

        public string Description { get; set; }

        public int Count { get { return _items.Count; } }

        public IList<IBinaryDataListItem> this[int key]
        {
            get
            {
                if (key >= 0 && this._items[key] != null)
                {
                    return this._items[key];
                }
                else if (key >= 0 && this._items[key] == null)
                {
                    // miss, add it ;)

                }

                return null;
            }

            set
            {
                if (key >= 0)
                {
                    this._items[key] = value;
                }
            }
        }

        public IIndexIterator Keys
        {
            get { return _items.Keys; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public void Init()
        {
            //_items = new Dictionary<int, IList<IBinaryDataListItem>>();
            _items = new Dev2BinaryDataListDictionary();
            //Added by Mo always need to be set to true on init unless specified
            //IsEditable = true;
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public bool TryGetValue(int idx, out IList<IBinaryDataListItem> result)
        {
            return (_items.TryGetValue(idx, out result));
        }

        /// <summary>
        /// Adds the specified idx.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="cols">The cols.</param>
        public void Add(int idx, IList<IBinaryDataListItem> cols)
        {
            _items.Add(idx, cols);
        }

        /// <summary>
        /// Removes the specified idx.
        /// </summary>
        /// <param name="idx">The idx.</param>
        public void Remove(int idx)
        {
            _items.Remove(idx);
        }


        /// <summary>
        /// Applies the sort action.
        /// </summary>
        public void ApplySortAction(IDictionary<int, IList<IBinaryDataListItem>> payload)
        {
            // Apply IDic back into my object ;)
            _items.Clear();
            IList<IBinaryDataListItem> cols;
            foreach (int i in payload.Keys)
            {
                if (payload.TryGetValue(i, out cols))
                {
                    _items[i] = cols;
                }
            }
        }

        public IDictionary<int, IList<IBinaryDataListItem>> FetchSortData()
        {
            IDictionary<int, IList<IBinaryDataListItem>> result = new Dictionary<int, IList<IBinaryDataListItem>>(1);
            IIndexIterator ii = Keys;
            while (ii.HasMore())
            {
                IList<IBinaryDataListItem> tmp;
                int next = ii.FetchNextIndex();
                if (_items.TryGetValue(next, out tmp))
                {

                    result[next] = tmp;
                }
            }

            return result;
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Fetches the master index key.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        private int FetchMasterIndexKey(int idx)
        {
            int lookIdx = (idx / GlobalConstants.DefaultCachePageSizeLvl1);

            return lookIdx;
        }

        /// <summary>
        /// Fetches the slave index key.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        private int FetchSlaveIndexKey(int idx)
        {
            int lookIdx = FetchMasterIndexKey(idx);
            int result = 1;

            if (lookIdx > 0)
            {
                result -= (lookIdx * GlobalConstants.DefaultColumnSizeLvl1);
            }
            else
            {
                result = idx;
            }

            return result;
        }

        #endregion
    }

}
