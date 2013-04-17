using Dev2.Data.Binary_Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.DataList.Contract.Binary_Objects.Structs
{
    [Serializable]
    public struct SBinaryDataListEntry
    {
        private BinaryDataListStorage _items;
        public int _appendIndex;
        IDictionary<string, int> _strToColIdx;
        bool _isEmpty;
        // Travis Mods ;) - Build the row TO for fetching as per the tooling ;)
        IList<IBinaryDataListItem> _internalReturnValue;
        IDictionary<int, IList<IBinaryDataListItem>> _deferedReads;

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

                IBinaryDataListRow binaryDataListRow = _items[key];

                IList<IBinaryDataListItem> tmpFetch;

                bool deferedRead = _deferedReads.TryGetValue(key, out tmpFetch);

                if (key >= 0 && binaryDataListRow != null && !deferedRead)
                {
                    ScrubInternalTO();

                    if (binaryDataListRow.IsEmpty)
                    {
                        return _internalReturnValue;
                    }

                    // Convert to _internalReturnValue format ;)
                    if (Columns != null)
                    {
                        for (int i = 0; i < Columns.Count; i++)
                        {
                            IBinaryDataListItem tmp = _internalReturnValue[i];


                            // normal object build
                            tmp.UpdateValue(binaryDataListRow.FetchValue(i));
                            if (key > 0)
                            {
                                tmp.UpdateIndex(key);
                            }

                        }
                    }
                    else
                    {
                        // we have a scalar value we are dealing with ;)
                        IBinaryDataListItem tmp = _internalReturnValue[0];
                        tmp.UpdateValue(binaryDataListRow.FetchValue(0));
                    }

                    return _internalReturnValue;

                }
                else if (key >= 0 && binaryDataListRow == null)
                {
                    // miss, add it ;)
                }
                else if (deferedRead)
                {
                    return tmpFetch;
                }

                return null;
            }

            set
            {
                if (key >= 0)
                {
                    // Convert to BinaryDataListRow for persistance ;)

                    int cols = 1;
                    if (Columns != null)
                    {
                        cols = Columns.Count;
                    }

                    IBinaryDataListRow row;

                    if (_items.TryGetValue(key, cols, out row))
                    {
                        // we got the row object, now update it ;)
                        foreach (IBinaryDataListItem itm in value)
                        {
                            int idx = InternalFetchColumnIndex(itm.FieldName); // Fetch correct index 
                            if (idx == -1 && !IsRecordset)
                            {
                                idx = 0; // adjust for scalar
                            }

                            if (!itm.IsDeferredRead && idx >= 0)
                            {
                                row.UpdateValue(itm.TheValue, idx);
                            }
                            else if (itm.IsDeferredRead && idx >= 0)
                            {
                                row.IsDeferredRead(idx);

                                // TODO : Bundle up the 
                                _deferedReads[key] = value;
                            }


                        }

                        _items[key] = row;
                    }
                    else
                    {
                        throw new Exception("Fatal Internal DataList Storage Error");
                    }

                    //this._items[key] = value;
                }
            }
        }

        public IIndexIterator Keys
        {
            get { return _items.Keys; }
        }

        public Guid DataListKey { get; set; }

        #endregion

        #region Public Methods
        public void SetMaxValue(int idx)
        {
            _items.SetMaxValue(idx);
        }

        public void RemoveDeferedRead(IBinaryDataListItem binaryDataListItem)
        {
            foreach (var deferedEntry in _deferedReads.ToList())
            {
                var itemsToRemove = deferedEntry.Value.Where(b => string.CompareOrdinal(b.FieldName, binaryDataListItem.FieldName) == 0).ToList();
                foreach (var item in itemsToRemove)
                {
                    deferedEntry.Value.Remove(item);
                }

                if (deferedEntry.Value.Count == 0)
                {
                    _deferedReads.Remove(deferedEntry);
                }
            }
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public void Init(int colCnt)
        {
            //_items = new Dictionary<int, IList<IBinaryDataListItem>>();
            _items = new BinaryDataListStorage(Namespace, DataListKey);

            _isEmpty = true;
            _internalReturnValue = new List<IBinaryDataListItem>(colCnt); // build the object we require to return data in ;)

            for (int i = 0; i < colCnt; i++)
            {
                _internalReturnValue.Add(new BinaryDataListItem(string.Empty, string.Empty));

                if (Columns != null)
                {
                    // Handle recordset
                    _internalReturnValue[i].UpdateRecordset(Namespace); // always the same namespace ;)
                    _internalReturnValue[i].UpdateField(Columns[i].ColumnName); // always the same column for this entry ;)
                }
                else
                {
                    // Handle scalars
                    _internalReturnValue[i].UpdateField(Namespace); // always the same namespace ;)
                }
            }

            _deferedReads = new Dictionary<int, IList<IBinaryDataListItem>>(2);

            // boot strap column cache ;)
            int cnt = 1;
            if (Columns != null)
            {
                cnt = Columns.Count;
            }
            _strToColIdx = new Dictionary<string, int>(cnt);

            // set the number of columns for storage hints ;)
            _items.ColumnSize = cnt;

            //Added by Mo always need to be set to true on init unless specified
            //IsEditable = true;
        }


        public IList<IBinaryDataListItem> FetchDeleteRowData()
        {
            ScrubInternalTO();
            return _internalReturnValue;
        }

        public int InternalFetchColumnIndex(string column)
        {
            int result = -1;
            if (IsRecordset)
            {
                if (!_strToColIdx.TryGetValue(column, out result))
                {
                    Dev2Column colToFind = Columns.FirstOrDefault(c => c.ColumnName == column);

                    if (colToFind != null)
                    {
                        result = Columns.IndexOf(colToFind);
                        _strToColIdx[column] = result; // save to cache ;)
                    }
                    else
                    {
                        result = -1; // it failed, default back to non-valid index
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            _items.Dispose();
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public bool TryGetValue(int idx, out IList<IBinaryDataListItem> result)
        {

            ScrubInternalTO();
            result = _internalReturnValue;
            IBinaryDataListRow row;
            _items.TryGetValue(idx, _internalReturnValue.Count, out row);
            bool res = (row != null);

            if (res)
            {
                if (!row.IsDeferredRead(idx))
                {

                    if (Columns != null)
                    {
                        // Change to index look up ;)
                        for (int i = 0; i < Columns.Count; i++)
                        {
                            int fetchIdx = InternalFetchColumnIndex(Columns[i].ColumnName);

                            IBinaryDataListItem tmp = _internalReturnValue[fetchIdx];
                            tmp.UpdateValue(row.FetchValue(fetchIdx));
                            tmp.UpdateIndex(idx);
                        }
                    }
                    else
                    {
                        // we have a scalar
                        IBinaryDataListItem tmp = _internalReturnValue[0];
                        tmp.UpdateValue(row.FetchValue(0));
                    }
                }
                else
                {
                    // Return defered read data ;)
                    result = _deferedReads[idx];
                }
            }

            return res;

        }

        /// <summary>
        /// Adds the specified idx.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="cols">The cols.</param>
        public void Add(int idx, IList<IBinaryDataListItem> cols)
        {

            IBinaryDataListRow row = new BinaryDataListRow(cols.Count);

            foreach (IBinaryDataListItem itm in cols)
            {
                row.UpdateValue(itm.TheValue, idx);
            }
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
            //_items.Clear();
            IList<IBinaryDataListItem> cols;
            foreach (int i in payload.Keys)
            {
                IBinaryDataListRow row = _items[i];


                if (payload.TryGetValue(i, out cols))
                {
                    foreach (IBinaryDataListItem c in cols)
                    {
                        int idx = InternalFetchColumnIndex(c.FieldName);
                        row.UpdateValue(c.TheValue, idx);
                    }

                    _items[i] = row;

                }
            }
        }

        public IDictionary<int, IList<IBinaryDataListItem>> FetchSortData()
        {
            IDictionary<int, IList<IBinaryDataListItem>> result = new Dictionary<int, IList<IBinaryDataListItem>>(Count);
            IIndexIterator ii = Keys;
            if (IsRecordset)
            {
                while (ii.HasMore())
                {
                    IList<IBinaryDataListItem> tmp = new List<IBinaryDataListItem>(Columns.Count);
                    IBinaryDataListRow row;
                    int next = ii.FetchNextIndex();
                    if (_items.TryGetValue(next, Columns.Count, out row))
                    {
                        for (int i = 0; i < Columns.Count; i++)
                        {
                            tmp.Add(new BinaryDataListItem(row.FetchValue(i), Namespace, Columns[i].ColumnName, next));
                        }

                        result[next] = tmp;
                    }
                }
            }

            return result;
        }

        public bool IsEmtpy
        {
            get
            {
                return _isEmpty;
            }
            set
            {
                _isEmpty = value;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Scrubs the internal TO.
        /// </summary>
        private void ScrubInternalTO()
        {
            if (_internalReturnValue == null)
            {
                int cnt = 1;

                if (Columns != null)
                {
                    cnt = Columns.Count;
                }

                _internalReturnValue = new List<IBinaryDataListItem>(cnt);
            }

            foreach (IBinaryDataListItem t in _internalReturnValue)
            {
                t.ToClear();
            }
        }

        #endregion
    }

}
