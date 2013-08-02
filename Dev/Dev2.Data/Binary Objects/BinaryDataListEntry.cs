using Dev2.Common;
using Dev2.Data.Audit;
using Dev2.Data.Binary_Objects;
using Dev2.Data.SystemTemplates;
using Dev2.DataList.Contract.Binary_Objects.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    internal class BinaryDataListEntry : IBinaryDataListEntry
    {
        #region Fields
        SBinaryDataListEntry _internalObj;
        #endregion Fields

        #region Properties

        public Guid DataListKey
        {
            get
            {
                return _internalObj.DataListKey;
            }
            private set
            {
                _internalObj.DataListKey = value;
            }
        }

        public bool IsEditable
        {
            get
            {
                return _internalObj.IsEditable;
            }
            private set
            {
                _internalObj.IsEditable = value;
            }
        }

        public enDev2ColumnArgumentDirection ColumnIODirection
        {
            get
            {
                return _internalObj.ColumnIODirection;
            }
            private set
            {
                _internalObj.ColumnIODirection = value;
            }
        }

        public bool IsRecordset
        {
            get
            {
                return _internalObj.IsRecordset;
            }
            private set
            {
                _internalObj.IsRecordset = value;
            }
        }

        public bool IsEvaluationScalar
        {
            get
            {
                return _internalObj.IsEvaluationScalar;
            }
            private set
            {
                _internalObj.IsEvaluationScalar = value;
            }
        }

        public bool IsManagmentServicePayload
        {
            get
            {
                return _internalObj.IsManagmentServicePayload;
            }
            private set
            {
                _internalObj.IsManagmentServicePayload = value;
            }
        }

        public IList<Dev2Column> Columns
        {
            get
            {
                return _internalObj.Columns;
            }
            private set
            {
                _internalObj.Columns = value;
            }
        }

        public string Namespace
        {
            get
            {
                return _internalObj.Namespace;
            }
            private set
            {
                _internalObj.Namespace = value;
            }
        }

        public string Description
        {
            get
            {
                return _internalObj.Description;
            }
            private set
            {
                _internalObj.Description = value;
            }
        }

        public ComplexExpressionAuditor ComplexExpressionAuditor { get; set; } 

        #endregion Properties

        #region Ctors


        internal BinaryDataListEntry(string nameSpace, string description, IList<Dev2Column> cols, Guid dataListKey)
            : this(nameSpace, description, cols, true, enDev2ColumnArgumentDirection.None, dataListKey)
        {
        }


        internal BinaryDataListEntry(string nameSpace, string description, Guid dataListKey)
            : this(nameSpace, description, true, enDev2ColumnArgumentDirection.None, dataListKey)
        {
        }

        internal BinaryDataListEntry(string nameSpace, string description, IList<Dev2Column> cols, bool isEditable, enDev2ColumnArgumentDirection ioDir, Guid dataListKey)
        {
            IsRecordset = true;
            Columns = cols;
            Namespace = String.IsNullOrEmpty(nameSpace) ? GlobalConstants.NullEntryNamespace : nameSpace;
            Description = description;
            IsEditable = isEditable;
            ColumnIODirection = ioDir;
            DataListKey = dataListKey;
            _internalObj._appendIndex = -1;
            _internalObj.Init(cols.Count);
        }

        internal BinaryDataListEntry(string nameSpace, string description, bool isEditable, enDev2ColumnArgumentDirection ioDir, Guid dataListKey)
        {
            Namespace = String.IsNullOrEmpty(nameSpace) ? GlobalConstants.NullEntryNamespace : nameSpace;
            Description = description;
            IsEditable = isEditable;
            ColumnIODirection = ioDir;
            DataListKey = dataListKey;
            _internalObj._appendIndex = -1;
            _internalObj.Init(1);
        }

        #endregion Ctors

        #region Methods

        /// <summary>
        ///     Fetch the number of records present
        /// </summary>
        /// <returns></returns>
        public int ItemCollectionSize()
        {
            return _internalObj.Count;
        }

        /// <summary>
        ///     Fetches the record at.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public IList<IBinaryDataListItem> FetchRecordAt(int idx, out string error)
        {
            return FetchRecordAt(idx, null, out error);
        }

        /// <summary>
        /// Tries to put an entire row at an index ;)
        /// </summary>
        /// <param name="itms">The itms.</param>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        public void TryPutRecordRowAt(IList<IBinaryDataListItem> itms, int idx, out string error)
        {
            error = "Is not recordset";
            int myIdx = idx;

            if (IsRecordset)
            {
                IList<IBinaryDataListItem> dummy;

                if (idx <= FetchLastRecordsetIndex() && _internalObj.TryGetValue(myIdx, out dummy) && dummy.Any())
                {
                    // entry already exist, so update the row ;)
                    _internalObj[myIdx] = itms; // removed clone since it was no longer needed
                }
                else
                {
                    _internalObj[myIdx] = itms; // removed clone since it was no longer needed
                    _internalObj.IsEmtpy = false;
                }

                error = string.Empty;
            }
        }

        public void TryPutRecordItemAtIndex(IBinaryDataListItem item, int idx, out string error)
        {
            error = "Is not recordset";
            int myIdx = idx;

            if (IsRecordset)
            {
                IList<IBinaryDataListItem> dummy;

                if (idx <= FetchLastRecordsetIndex() && _internalObj.TryGetValue(myIdx, out dummy) && dummy.Any())
                {
                    int colIdx = InternalFetchColumnIndex(item.FieldName);
                    if (colIdx >= 0)
                    {
                        // entry already exist
                        var max = _internalObj.Keys.MaxIndex(); // we need to save this because of Juries refactor ;)
                        _internalObj[myIdx] = new List<IBinaryDataListItem> { item };
                        error = string.Empty;
                        _internalObj.IsEmtpy = false;
                        _internalObj.ReInstateMaxValue(max); // re-instate ;)
                    }
                    else
                    {
                        error = "Mapping error: Column not Found" + item.FieldName;
                    }
                }
                else
                {
                    int colIdx = InternalFetchColumnIndex(item.FieldName);
                    if (colIdx >= 0)
                    {
                        string ns = item.Namespace == string.Empty ? Namespace : item.Namespace;

                        item.UpdateIndex(myIdx);
                        item.UpdateRecordset(ns);
                        IList<IBinaryDataListItem> cols = new List<IBinaryDataListItem> { item };

                        _internalObj[myIdx] = cols;
                        _internalObj.IsEmtpy = false;
                        error = string.Empty;

                    }
                    else
                    {
                        error = "Mapping error: Column not found " + item.FieldName;
                    }
                }
            }
        }

        public void TryAppendRecordItem(IBinaryDataListItem item, out string error)
        {
            error = "Is not recordset";
            if (IsRecordset)
            {
                Dev2Column colToFind = Columns.FirstOrDefault(c => c.ColumnName == item.FieldName);
                if (colToFind != null)
                {
                    _internalObj[ItemCollectionSize()] = new List<IBinaryDataListItem> { item };
                    error = string.Empty;
                    _internalObj.IsEmtpy = false;
                }
                else
                {
                    error = "Mapping error: Column not found " + item.FieldName;
                }
            }
        }

        public IBinaryDataListItem FetchScalar()
        {
            IBinaryDataListItem result = null;
            if (IsRecordset)
            {
                string error;
                return TryFetchLastIndexedRecordsetUpsertPayload(out error);
            }

            if (_internalObj.Count > 0)
            {
                var binaryDataListItems = _internalObj[0];
                if (binaryDataListItems.Count > 0)
                {
                    result = binaryDataListItems[0];
                }
            }

            if (result == null)
            {
                result = new BinaryDataListItem(string.Empty, Namespace);
                // place miss into the collection
                _internalObj[0] = new List<IBinaryDataListItem> { result };
            }

            return result;
        }

        public void TryPutScalar(IBinaryDataListItem item, out string error)
        {
            error = "Not a scalar";
            if (!IsRecordset)
            {
                // set evaluation scalar
                // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                if (item.FieldName == GlobalConstants.EvalautionScalar)
                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                {
                    IsEvaluationScalar = true;
                }
                else
                {
                    IsEvaluationScalar = false;
                }

                // set managment service payload
                // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                if (item.FieldName == GlobalConstants.ManagementServicePayload)
                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                {
                    IsManagmentServicePayload = true;
                }
                else
                {
                    IsManagmentServicePayload = false;
                }

                if (!item.IsDeferredRead)
                {
                    _internalObj.RemoveDeferedRead(item);
                }

                _internalObj[0] = new List<IBinaryDataListItem> { item };

                error = string.Empty;
            }
        }

        public IBinaryDataListEntry Clone(enTranslationDepth depth, Guid clonedStorageId, out string error)
        {
            error = string.Empty;
            BinaryDataListEntry result;
            Guid dlKey = DataListKey;

            if (clonedStorageId != GlobalConstants.NullDataListID)
            {
                dlKey = clonedStorageId;
            }

            if (Columns != null)
            {
                // clone the columns
                IList<Dev2Column> cols = new List<Dev2Column>(Columns.Count);
                foreach (Dev2Column c in Columns)
                {
                    cols.Add(new Dev2Column(c.ColumnName, c.ColumnDescription));
                }
                result = new BinaryDataListEntry(Namespace, Description, cols, dlKey);
            }
            else
            {
                result = new BinaryDataListEntry(Namespace, Description, dlKey);
            }

            if (depth == enTranslationDepth.Data || depth == enTranslationDepth.Data_With_Blank_OverWrite)
            {
                // clone _items

                if (IsRecordset)
                {
                    IIndexIterator ii = _internalObj.Keys;
                    bool isEmtpy = _internalObj.IsEmtpy;
                    result._internalObj.IsEmtpy = isEmtpy;
                    while (ii.HasMore())
                    {
                        int next = ii.FetchNextIndex();
                        // clone the data

                        // Travis.Frisinger - Only do this if there is data present, else leave it as it is

                        // TODO : Internal Object will not have data, we just need to expose this fact!

                        //if (!isEmtpy)
                        //{
                            IList<IBinaryDataListItem> items = _internalObj[next];
                            IList<IBinaryDataListItem> clone = new List<IBinaryDataListItem>();
                            // Bug 8725
                            if (items != null)
                            {
                                foreach (IBinaryDataListItem itm in items)
                                {
                                    clone.Add(itm.Clone());
                                }
                            }

                            // now push back clone
                            result._internalObj[next] = clone;
                        //}
                    }
                }
                else
                {
                    IList<IBinaryDataListItem> items = _internalObj[0];
                    IList<IBinaryDataListItem> clone = items.Select(itm => itm.Clone()).ToList();

                    // now push back clone

                    result._internalObj[0] = clone;
                }
            }
            else // only wanted the shape cloned
            {
                // clone _items
                IList<IBinaryDataListItem> blankItems = new List<IBinaryDataListItem>();
                if (_internalObj.Count > 0)
                {
                    if (IsRecordset)
                    {
                        //int firstKey = _internalObj.Keys.First();
                        int firstKey = _internalObj.Keys.MinIndex();
                        int listLen = _internalObj[firstKey].Count;
                        for (int i = 0; i < listLen; i++)
                        {
                            int idx = i + 1;
                            IBinaryDataListItem itm = DataListConstants.baseItem.Clone();
                            itm.UpdateRecordset(Namespace);
                            if (Columns != null)
                            {
                                itm.UpdateField(Columns[i].ColumnName);
                            }
                            itm.UpdateIndex(idx);
                            blankItems.Add(itm);
                        }

                        result._internalObj[firstKey] = blankItems;
                    }
                    else
                    {
                        //IBinaryDataListItem binaryDataListItem = DataListConstants.emptyItem;
                        blankItems.Add(DataListConstants.emptyItem);
                        result._internalObj[0] = blankItems;
                    }
                }
            }
            result.ComplexExpressionAuditor = this.ComplexExpressionAuditor;
            return result;
        }

        //public IBinaryDataListEntry Clone(enTranslationDepth depth, out string error)
        //{
        //    return Clone(depth, GlobalConstants.NullDataListID, out error);
        //}

        public void Merge(IBinaryDataListEntry toMerge, out string error)
        {
            error = string.Empty;
            if (IsRecordset && toMerge.IsRecordset)
            {
                IIndexIterator ii = toMerge.FetchRecordsetIndexes();
                while (ii.HasMore())
                {
                    int next = ii.FetchNextIndex();
                    // merge toMerge into this
                    foreach (IBinaryDataListItem item in toMerge.FetchRecordAt(next, out error))
                    {
                        TryAppendRecordItem(item, out error);
                    }
                }
            }
            else if (!IsRecordset && !toMerge.IsRecordset)
            {
                TryPutScalar(toMerge.FetchScalar(), out error); // over write this with toMerge
            }
            else
            {
                error = "Type mis-match, one side is Recordset while the other is a scalar";
            }
        }

        /// <summary>
        ///     Fetches the recordset indexs.
        /// </summary>
        /// <returns></returns>
        public IIndexIterator FetchRecordsetIndexes()
        {
            var  result = _internalObj.Keys;
            return result;
        }

        /// <summary>
        ///     Fetches the last index of the recordset.
        /// </summary>
        /// <returns></returns>
        public int FetchLastRecordsetIndex()
        {
            int result = 1;

            if (IsRecordset)
            {
                result = _internalObj.Keys.MaxIndex();
            }

            return result;
        }

        /// <summary>
        /// Sets the last index of the recordset.
        /// </summary>
        /// <param name="idx">The idx.</param>
        public void SetLastRecordsetIndex(int idx)
        {
            if (IsRecordset)
            {
                _internalObj.SetMaxValue(idx);
            }
        }


        /// <summary>
        /// Fetches the index of the append recordset.
        /// </summary>
        /// <returns></returns>
        public int FetchAppendRecordsetIndex()
        {
            int result = FetchLastRecordsetIndex();
            if (result >= 1 && _internalObj._appendIndex > 0)
            {
                result++; // inc for insert if data already present
            }
            else if (result == 1 && _internalObj._appendIndex == -1)
            {
                _internalObj._appendIndex = 2; // first pass
                if (!_internalObj.IsEmtpy)
                {
                    result++;
                }
            }
            else if (result > 1)
            {
                result++;
            }
            return result;
        }

        /// <summary>
        ///     Makes the recordset evaluate ready.
        /// </summary>
        /// <param name="keepIdx">The keep idx.</param>
        /// <param name="keepCol">The keep col.</param>
        /// <param name="error">The error.</param>
        public void MakeRecordsetEvaluateReady(int keepIdx, string keepCol, out string error)
        {
            error = string.Empty;

            // use only wants a specific column retained, not the entire row
            if (keepCol != null)
            {
                IList<Dev2Column> newCols = new List<Dev2Column>(Columns.Count) { new Dev2Column(keepCol, string.Empty) };
                // remove values
                Columns = newCols;
            }

            if (keepIdx == GlobalConstants.AllIndexes || _internalObj.IsEmtpy)
            {
                return;
            }

            var gapToInsert = 0;
            while (_internalObj.Keys.HasMore() && gapToInsert < keepIdx)
            {
                gapToInsert = _internalObj.Keys.FetchNextIndex();
                if (gapToInsert != keepIdx)
                {
                    _internalObj.Keys.AddGap(gapToInsert);
                }
            }

            _internalObj.SetMaxValue(keepIdx);
        }

        public IBinaryDataListItem TryFetchRecordsetColumnAtIndex(string field, int idx, out string error)
        {
            IList<IBinaryDataListItem> cols = FetchRecordAt(idx, field, out error);
            IBinaryDataListItem result = DataListConstants.baseItem.Clone();

            if (cols == null || cols.Count == 0)
            {
                error = "Index [ " + idx + " ] is out of bounds";
            }
            else
            {
                result = cols.FirstOrDefault(c => c.FieldName == field);
            }

            return result;
        }

        public IBinaryDataListItem TryFetchLastIndexedRecordsetUpsertPayload(out string error)
        {

            int idx = FetchLastRecordsetIndex();

            return InternalFetchIndexedRecordsetUpsertPayload(idx, out error);
        }

        public IBinaryDataListItem TryFetchIndexedRecordsetUpsertPayload(int idx, out string error)
        {
            return InternalFetchIndexedRecordsetUpsertPayload(idx, out error);
        }

        public void BlankRecordSetData(string colName)
        {
            IIndexIterator ii = _internalObj.Keys;

            if (colName != null)
            {
                Dev2Column cc = Columns.FirstOrDefault(c => c.ColumnName == colName);

                if (cc != null)
                {
                    while (ii.HasMore())
                    {
                        int next = ii.FetchNextIndex();
                        // now blank all values at this location
                        IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, Namespace, cc.ColumnName, next);
                        string error;
                        TryPutRecordItemAtIndex(itm, next, out error);
                    }
                }
                else
                {
                    ClearAll(ii);
                }
            }
            else
            {
                ClearAll(ii);
            }
        }

        public bool HasColumns(IList<Dev2Column> cols)
        {
            bool result = true;
            IList<Dev2Column> myCols = Columns;
            int i = 0;

            while (i < cols.Count && result)
            {
                if (!myCols.Contains(cols[i]))
                {
                    result = false;
                }
                i++;
            }

            return result;
        }

        public bool HasField(string field)
        {
            bool result = false;

            if (IsRecordset)
            {
                if (Columns.FirstOrDefault(c => c.ColumnName == field) != null)
                {
                    result = true;
                }
            }

            return result;
        }

        public void Sort(string field, bool desc, out string error)
        {
            // INFO : http://stackoverflow.com/questions/925471/c-sharp-help-sorting-a-list-of-objects-in-c-sharp
            error = string.Empty;
            Dev2Column col = Columns.FirstOrDefault(c => c.ColumnName == field);
            IDictionary<int, IList<IBinaryDataListItem>> toSort = _internalObj.FetchSortData();
            IDictionary<int, IList<IBinaryDataListItem>> sortedData = null;

            if (col != null)
            {
                int colIdx = Columns.IndexOf(col);

                // Port of per DLS approach -- Technical debt
                // Int
                try
                {
                    sortedData = IntSort(toSort, colIdx, desc);
                }
                catch (Exception)
                {
                    // DateTime
                    try
                    {
                        sortedData = DateTimeSort(toSort, colIdx, desc);
                    }
                    catch (Exception)
                    {
                        // String
                        try
                        {
                            sortedData = StringSort(toSort, colIdx, desc);
                        }
                        catch (Exception ex)
                        {
                            // Very naughty thing have happened....
                            error = "Invalid format for sorting on field [ " + field + " ] ";
                            ServerLogger.LogError(ex);
                        }
                    }
                }

                // apply the update ;)
                _internalObj.ApplySortAction(sortedData);
            }
        }

        public bool IsEmpty()
        {
            return _internalObj.IsEmtpy;
        }

        public bool TryDeleteRows(string index)
        {
            bool result = false;
            if (IsRecordset && index != null)
            {
                int numericIndex;
                if (!Int32.TryParse(index, out numericIndex))
                {
                    if (string.IsNullOrEmpty(index))
                    {
                        result = DeleteLastRow();
                    }
                    else if (index == "*")
                    {
                        result = DeleteAllRows();
                    }
                }
                else
                {
                    result = DeleteRowAtIndex(numericIndex);
                }
            }
            return result;
        }

        /// <summary>
        ///     Fetches the record at.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="field">The exact column needed to be fetched</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public IList<IBinaryDataListItem> FetchRecordAt(int idx, string field, out string error)
        {
            IList<IBinaryDataListItem> result = null;
            error = string.Empty;

            if (idx != 0 && !_internalObj.TryGetValue(idx, out result))
            {
                // row miss, create it and return it ;)
                result = CorrectRecordsetFetchMiss(idx, out error);
            }
            if (result == null)
            {
                return null;
            }
            IEnumerable<string> colNames;
            if (String.IsNullOrEmpty(field))
            {
                colNames = Columns.Select(column => column.ColumnName);
            }
            else
            {
                colNames = new List<string> { field };
            }
            IList<IBinaryDataListItem> resultList = new List<IBinaryDataListItem>();
            foreach (var colName in colNames)
            {
                List<IBinaryDataListItem> binaryDataListItems = result.Where(item => item.FieldName == colName).ToList();
                if (binaryDataListItems.Count > 0)
                {
                    resultList.Add(binaryDataListItems[0]);
                }
            }
            if (resultList.Count == 0)
            {
                resultList = CorrectRecordsetFetchMiss(idx, out error);
            }
            return resultList;
        }

        public List<int> GetDistinctRows(List<string> filterCols)
        {
            return _internalObj.GetDistinctRows(filterCols);
        } 

        public int InternalFetchColumnIndex(string column)
        {
            return _internalObj.InternalFetchColumnIndex(column);
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        ///     Deletes all the rows.
        /// </summary>
        /// <returns></returns>
        bool DeleteAllRows()
        {
            var tmp = new SBinaryDataListEntry();

            tmp.IsRecordset = _internalObj.IsRecordset;
            tmp.Columns = _internalObj.Columns;
            tmp.Namespace = _internalObj.Namespace;
            tmp.DataListKey = _internalObj.DataListKey;
            tmp.Description = _internalObj.Description;
            tmp.IsEditable = _internalObj.IsEditable;
            tmp.ColumnIODirection = _internalObj.ColumnIODirection;
            tmp._appendIndex = -1;

            tmp.Init(_internalObj.Columns.Count);

            _internalObj = tmp;
            int lastRowIndex = FetchLastRecordsetIndex();
            _internalObj.Remove(lastRowIndex);
            _internalObj.SetMaxValue(-1);
            return true;
        }

        /// <summary>
        ///     Deletes the last row.
        /// </summary>
        /// <returns></returns>
        bool DeleteLastRow()
        {
            int lastRowIndex = FetchLastRecordsetIndex();

            // Bug 8725
            if (!_internalObj.IsEmtpy)
            {
                _internalObj.Remove(lastRowIndex);
            }

            return true;
        }

        /// <summary>
        ///     Deletes the row at a specific index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        bool DeleteRowAtIndex(int index)
        {
            bool result = false;
            int lastIndex = FetchLastRecordsetIndex();
            if (index <= lastIndex && index > 0)
            {
                _internalObj[index] = _internalObj.FetchDeleteRowData();
                _internalObj.Remove(index);
                _internalObj.SetMaxValue(lastIndex);
                

                result = true;
            }

            return result;
        }

        /// <summary>
        ///     Ints the sort.
        /// </summary>
        /// <param name="toSort"></param>
        /// <param name="colIdx"></param>
        /// <param name="desc">
        ///     if set to <c>true</c> [desc].
        /// </param>
        IDictionary<int, IList<IBinaryDataListItem>> IntSort(IDictionary<int, IList<IBinaryDataListItem>> toSort, int colIdx, bool desc)
        {
            IDictionary toSwap = new Dictionary<int, IList<IBinaryDataListItem>>();

            if (!desc)
            {
                var data = toSort.OrderBy(x =>
                {
                    long val;
                    string tmpVal = x.Value[colIdx].TheValue;
                    if (string.IsNullOrWhiteSpace(tmpVal))
                    {
                        val = long.MinValue;
                    }
                    else
                    {
                        if (!long.TryParse(tmpVal, out val))
                        {
                            throw new Exception();
                        }
                    }
                    return val;
                }).ToList();
                int idx = 1;
                foreach (KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    toSwap[idx] = tmp.Value;
                    idx++;
                }
            }
            else
            {
                var data = toSort.OrderByDescending(x =>
                {
                    long val;
                    string tmpVal = x.Value[colIdx].TheValue;
                    if (string.IsNullOrWhiteSpace(tmpVal))
                    {
                        val = long.MinValue;
                    }
                    else
                    {
                        if (!long.TryParse(tmpVal, out val))
                        {
                            throw new Exception();
                        }
                    }
                    return val;
                }).ToList();
                int idx = 1;
                foreach (KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    toSwap[idx] = tmp.Value;
                    idx++;
                }
            }

            toSort.Clear();

            // make the swap
            foreach (int k in toSwap.Keys)
            {
                toSort[k] = (IList<IBinaryDataListItem>)toSwap[k];
            }

            return toSort;
        }

        /// <summary>
        ///     Dates the time sort.
        /// </summary>
        /// <param name="toSort"></param>
        /// <param name="colIdx">The col idx.</param>
        /// <param name="desc">
        ///     if set to <c>true</c> [desc].
        /// </param>
        IDictionary<int, IList<IBinaryDataListItem>> DateTimeSort(IDictionary<int, IList<IBinaryDataListItem>> toSort, int colIdx, bool desc)
        {
            IDictionary toSwap = new Dictionary<int, IList<IBinaryDataListItem>>();

            if (!desc)
            {
                var data = toSort.OrderBy(x =>
                {
                    DateTime val;
                    string tmpVal = x.Value[colIdx].TheValue;
                    if (string.IsNullOrWhiteSpace(tmpVal))
                    {
                        val = DateTime.MinValue;
                    }
                    else
                    {
                        if (!DateTime.TryParse(x.Value[colIdx].TheValue, out val))
                        {
                            throw new Exception();
                        }
                    }
                    return val;
                }).ToList();
                int idx = 1;
                foreach (KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    toSwap[idx] = tmp.Value;
                    idx++;
                }
            }
            else
            {
                var data = toSort.OrderByDescending(x =>
                {
                    DateTime val;
                    string tmpVal = x.Value[colIdx].TheValue;
                    if (string.IsNullOrWhiteSpace(tmpVal))
                    {
                        val = DateTime.MinValue;
                    }
                    else
                    {
                        if (!DateTime.TryParse(x.Value[colIdx].TheValue, out val))
                        {
                            throw new Exception();
                        }
                    }
                    return val;
                }).ToList();
                int idx = 1;
                foreach (KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    toSwap[idx] = tmp.Value;
                    idx++;
                }
            }

            toSort.Clear();

            // make the swap
            foreach (int k in toSwap.Keys)
            {
                toSort[k] = (IList<IBinaryDataListItem>)toSwap[k];
            }

            return toSort;
        }

        /// <summary>
        /// Strings the sort.
        /// </summary>
        /// <param name="toSort">To sort.</param>
        /// <param name="colIdx">The col idx.</param>
        /// <param name="desc">if set to <c>true</c> [desc].</param>
        /// <returns></returns>
        IDictionary<int, IList<IBinaryDataListItem>> StringSort(IDictionary<int, IList<IBinaryDataListItem>> toSort, int colIdx, bool desc)
        {
            //IDictionary _toSwap = new Dictionary<int, IList<IBinaryDataListItem>>();

            //if(!desc)
            //{
            //    var data = toSort.OrderBy(x => x.Value[colIdx].TheValue).ToList();
            //    int idx = 1;
            //    foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
            //    {
            //        _toSwap[idx] = tmp.Value;
            //        idx++;
            //    }
            //}
            //else
            //{
            //    var data = toSort.OrderByDescending(x => x.Value[colIdx].TheValue).ToList();
            //    int idx = 1;
            //    foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
            //    {
            //        _toSwap[idx] = tmp.Value;
            //        idx++;
            //    }
            //}

            //toSort.Clear();

            //// make the swap
            //foreach(int k in _toSwap.Keys)
            //{
            //    toSort[k] = (IList<IBinaryDataListItem>)_toSwap[k];
            //}

            //return toSort;

            IDictionary toSwap = new Dictionary<int, IList<IBinaryDataListItem>>();

            if (!desc)
            {
                var data = toSort.OrderBy(x =>
                {
                    string val = x.Value[colIdx].TheValue;

                    return val;
                }).ToList();

                int idx = 1;

                foreach (KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    toSwap[idx] = tmp.Value;
                    idx++;
                }
            }
            else
            {
                var data = toSort.OrderByDescending(x =>
                {
                    string val = x.Value[colIdx].TheValue;
                    return val;
                }).ToList();

                int idx = 1;
                foreach (KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    toSwap[idx] = tmp.Value;
                    idx++;
                }
            }

            toSort.Clear();

            // make the swap
            foreach (int k in toSwap.Keys)
            {
                toSort[k] = (IList<IBinaryDataListItem>)toSwap[k];
            }

            return toSort;

        }

        /// <summary>
        ///     Tries the fetch indexed recordset upsert payload.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataListItem InternalFetchIndexedRecordsetUpsertPayload(int idx, out string error)
        {
            error = string.Empty;
            IBinaryDataListItem result = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);
            // in this case there is a single row, with a single column's data to extract
            Dev2Column col = Columns.FirstOrDefault();
            result = TryFetchRecordsetColumnAtIndex(col.ColumnName, idx, out error);
            return result;
        }

        /// <summary>
        /// Clears all.
        /// </summary>
        /// <param name="idxItr">The idx itr.</param>
        void ClearAll(IIndexIterator idxItr)
        {
            // miss, clear it all out ;)
            while (idxItr.HasMore())
            {
                int next = idxItr.FetchNextIndex();
                _internalObj.Remove(next);
            }
        }

        /// <summary>
        ///     Corrects the recordset fetch miss.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IList<IBinaryDataListItem> CorrectRecordsetFetchMiss(int idx, out string error)
        {
            IList<IBinaryDataListItem> result = new List<IBinaryDataListItem>();
            error = string.Empty;
            bool isEmtpy = _internalObj[0].Count == 0;
            foreach (Dev2Column c in Columns)
            {
                IBinaryDataListItem tmp = new BinaryDataListItem(string.Empty, Namespace, c.ColumnName, idx);
                if (error == string.Empty)
                {
                    if (!isEmtpy)
                    {
                        TryPutRecordItemAtIndex(tmp, idx, out error);
                    }
                    result.Add(tmp);
                }
            }

            return result;
        }

        #endregion Private Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BinaryDataListEntry()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (!disposing) return;
            _internalObj.Dispose();
        }
    }
}