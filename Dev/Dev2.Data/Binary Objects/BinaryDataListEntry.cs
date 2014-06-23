using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dev2.Common;
using Dev2.Data.Audit;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Storage.ProtocolBuffers;
using Dev2.Data.SystemTemplates;
using Dev2.DataList.Contract.Binary_Objects.Structs;

// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract.Binary_Objects
// ReSharper restore CheckNamespace
{
    [Serializable]
    internal class BinaryDataListEntry : IBinaryDataListEntry
    {
        #region Fields
        SBinaryDataListEntry _internalObj;
        private bool _isEvalauteReady;
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
            _internalObj.AppendIndex = -1;
            _internalObj.Init(cols.Count);
            RegisterScope();
        }

        internal BinaryDataListEntry(string nameSpace, string description, bool isEditable, enDev2ColumnArgumentDirection ioDir, Guid dataListKey)
        {
            Namespace = String.IsNullOrEmpty(nameSpace) ? GlobalConstants.NullEntryNamespace : nameSpace;
            Description = description;
            IsEditable = isEditable;
            ColumnIODirection = ioDir;
            DataListKey = dataListKey;
            _internalObj.AppendIndex = -1;
            _internalObj.Init(1);
            RegisterScope();
        }

        #endregion Ctors

        #region Methods


        #region Alias Methods

        /// <summary>
        /// Makes the flow through entry.
        /// </summary>
        /// <param name="parentDlid">The parent dlid.</param>
        /// <param name="parentColumn">The parent column.</param>
        /// <param name="parentNamespace">The parent namespace.</param>
        /// <param name="childColumn">The child column.</param>
        /// <param name="errors">The errors.</param>
        public void AdjustForIOMapping(Guid parentDlid, string parentColumn, string parentNamespace, string childColumn, out ErrorResultTO errors)
        {
            // Need to adjust the storage layer to retain the parent DLID storage location ;)
            _internalObj.AddAlias(parentDlid, parentColumn, parentNamespace, childColumn, out errors);
        }

        /// <summary>
        /// Adjusts the alias operation for external service populate.
        /// </summary>
        public void AdjustAliasOperationForExternalServicePopulate()
        {
            if (!_internalObj.IsEmtpy && FetchAlias().Count > 0 && _internalObj.IsRecordset && _internalObj.Keys.Count == 1)
            {
                _internalObj.IsEmtpy = true;
            }
        }

        #endregion

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
        /// Fetches the row.
        /// </summary>
        /// <param name="idx">The index.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public IList<IBinaryDataListItem> FetchRowAt(int idx, out string error)
        {
            return FetchRecordAt(idx, null, out error, true);
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

            if(IsRecordset)
            {
                if(idx <= FetchLastRecordsetIndex() && _internalObj.ContainsRow(myIdx))
                {
                    // entry already exist, so update the row ;)
                    _internalObj[myIdx] = itms;
                    _internalObj.IsEmtpy = false;
                    _internalObj.Keys.RemoveGap(myIdx);
                }
                else if(myIdx >= 1)
                {
                    _internalObj[myIdx] = itms;
                    _internalObj.IsEmtpy = false;
                    _internalObj.Keys.RemoveGap(myIdx);
                }

                error = string.Empty;
            }
        }

        public void TryPutRecordItemAtIndex(IBinaryDataListItem item, int idx, out string error)
        {
            error = "Is not recordset";
            int myIdx = idx;

            if(IsRecordset)
            {
                if(idx <= FetchLastRecordsetIndex() && _internalObj.ContainsRow(myIdx))
                {
                    int colIdx = InternalFetchColumnIndex(item.FieldName);
                    if(colIdx >= 0)
                    {
                        _internalObj[myIdx] = new List<IBinaryDataListItem> { item };
                        error = string.Empty;
                        _internalObj.IsEmtpy = false;
                        _internalObj.Keys.RemoveGap(myIdx);
                    }
                    else
                    {
                        error = "Mapping error: Column not Found" + item.FieldName;
                    }
                }
                else if(idx >= 1)
                {
                    int colIdx = InternalFetchColumnIndex(item.FieldName);
                    if(colIdx >= 0)
                    {
                        string ns = item.Namespace == string.Empty ? Namespace : item.Namespace;

                        item.UpdateIndex(myIdx);
                        item.UpdateRecordset(ns);
                        IList<IBinaryDataListItem> cols = new List<IBinaryDataListItem> { item };

                        _internalObj[myIdx] = cols;
                        _internalObj.IsEmtpy = false;
                        _internalObj.Keys.RemoveGap(myIdx);
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
            if(IsRecordset)
            {
                Dev2Column colToFind = Columns.FirstOrDefault(c => c.ColumnName == item.FieldName);
                if(colToFind != null)
                {
                    _internalObj.Keys.RemoveGap(FetchAppendRecordsetIndex());
                    _internalObj[FetchAppendRecordsetIndex()] = new List<IBinaryDataListItem> { item };
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
            if(IsRecordset)
            {
                string error;
                return TryFetchLastIndexedRecordsetUpsertPayload(out error);
            }

            if(_internalObj.Count > 0)
            {
                var binaryDataListItems = _internalObj[0];
                if(binaryDataListItems != null && binaryDataListItems.Count > 0)
                {
                    result = binaryDataListItems[0];
                }
            }

            if(result == null)
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
            if(!IsRecordset)
            {
                // set evaluation scalar
                // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                if(item.FieldName == GlobalConstants.EvalautionScalar)
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
                if(item.FieldName == GlobalConstants.ManagementServicePayload)
                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                {
                    IsManagmentServicePayload = true;
                }
                else
                {
                    IsManagmentServicePayload = false;
                }


                _internalObj[0] = new List<IBinaryDataListItem> { item };
                _internalObj.IsEmtpy = false;

                error = string.Empty;
            }
        }

        public IBinaryDataListEntry Clone(enTranslationDepth depth, Guid clonedStorageId, out string error)
        {
            error = string.Empty;
            BinaryDataListEntry result;
            Guid dlKey = DataListKey;

            if(clonedStorageId != GlobalConstants.NullDataListID)
            {
                dlKey = clonedStorageId;
            }

            if(Columns != null)
            {
                // clone the columns
                IList<Dev2Column> cols = new List<Dev2Column>(Columns.Count);
                foreach(Dev2Column c in Columns)
                {
                    cols.Add(new Dev2Column(c.ColumnName, c.ColumnDescription));
                }
                result = new BinaryDataListEntry(Namespace, Description, cols, dlKey);
            }
            else
            {
                result = new BinaryDataListEntry(Namespace, Description, dlKey);
            }

            // 2013.09.09 - we're the same, just adjust the view and return
            if(clonedStorageId.Equals(DataListKey))
            {
                // manip result's _internalObj aka the view of the data ;)
                result._internalObj.CopyTo(_internalObj);
                // copy express auditing data too ;)
                result.ComplexExpressionAuditor = ComplexExpressionAuditor;
            }

            if(depth == enTranslationDepth.Data || depth == enTranslationDepth.Data_With_Blank_OverWrite)
            {
                // clone _items
                if(IsRecordset)
                {

                    IIndexIterator ii = _internalObj.Keys;
                    bool isEmtpy = _internalObj.IsEmtpy;
                    result._internalObj.IsEmtpy = isEmtpy;
                    while(ii.HasMore())
                    {
                        int next = ii.FetchNextIndex();
                        // clone the data
                        IList<IBinaryDataListItem> items = _internalObj[next];
                        IList<IBinaryDataListItem> clone = new List<IBinaryDataListItem>();
                        // Bug 8725
                        if(items != null)
                        {
                            foreach(IBinaryDataListItem itm in items)
                            {
                                clone.Add(itm.Clone());
                            }
                        }

                        // now push back clone
                        result._internalObj[next] = clone;
                    }

                    // ensure we reset min index if not 1 ;)
                    var keys = _internalObj.Keys;
                    var min = keys.MinIndex();
                    var max = keys.MaxIndex();
                    var gaps = _internalObj.FetchGaps();
                    result._internalObj.MoveIndexDataForClone(min, max, gaps, false);

                }
                else
                {
                    IList<IBinaryDataListItem> items = _internalObj[0];
                    IList<IBinaryDataListItem> clone = items.Select(itm => itm.Clone()).ToList();

                    // now push back clone
                    result._internalObj[0] = clone;
                    result._internalObj.IsEmtpy = false;
                }
            }
            else // only wanted the shape cloned
            {
                var keys = _internalObj.Keys;
                var min = keys.MinIndex();
                var max = keys.MaxIndex();
                var gaps = _internalObj.FetchGaps();
                result._internalObj.MoveIndexDataForClone(min, max, gaps, false);

            }

            result.ComplexExpressionAuditor = ComplexExpressionAuditor;

            return result;
        }

        public void Merge(IBinaryDataListEntry toMerge, out string error)
        {
            error = string.Empty;
            if(IsRecordset && toMerge.IsRecordset)
            {
                IIndexIterator ii = toMerge.FetchRecordsetIndexes();
                while(ii.HasMore())
                {
                    int next = ii.FetchNextIndex();
                    // merge toMerge into this
                    foreach(IBinaryDataListItem item in toMerge.FetchRecordAt(next, out error))
                    {
                        TryAppendRecordItem(item, out error);
                    }
                }
            }
            else if(!IsRecordset && !toMerge.IsRecordset)
            {
                TryPutScalar(toMerge.FetchScalar(), out error); // over write this with toMerge
            }
            else
            {
                error = "Type mis-match, one side is Recordset while the other is a scalar";
            }
        }

        /// <summary>
        /// Fetches the recordset indexs.
        /// </summary>
        /// <returns></returns>
        public IIndexIterator FetchRecordsetIndexes()
        {

            var aliases = FetchAlias();
            var theAlias = aliases.FirstOrDefault();

            if(theAlias.Value != null && !_isEvalauteReady)
            {
                var me = theAlias.Value.MasterEntry;
                if(me != null)
                {
                    var index = me.FetchRecordsetIndexes();

                    return index;
                }
            }

            var result = _internalObj.Keys;
            return result;
        }

        /// <summary>
        /// Fetches the last index of the recordset.
        /// </summary>
        /// <param name="localOnly">if set to <c>true</c> [local only].</param>
        /// <returns></returns>
        public int FetchLastRecordsetIndex(bool localOnly = false)
        {
            int result = 1;

            if(IsRecordset)
            {
                // We need to detect if there is an alias mapping and return its MaxValue ;)

                if(!localOnly)
                {
                    var aliasDic = _internalObj.FetchAlias();

                    if(aliasDic.Count > 0)
                    {
                        var aliasMapping = aliasDic.FirstOrDefault();
                        var aliasMasterEntry = aliasMapping.Value.MasterEntry;
                        if(aliasMasterEntry != null)
                        {
                            result = aliasMasterEntry.FetchRecordsetIndexes().MaxIndex();
                            if(result == 0)
                            {
                                result = 1;
                            }
                            return result;
                        }
                    }
                }
                int tmpIndex = _internalObj.Keys.MaxIndex();
                if(tmpIndex == 0)
                {
                    result = 1;
                }
                else
                {
                    result = tmpIndex;
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches the index of the append recordset.
        /// </summary>
        /// <returns></returns>
        public int FetchAppendRecordsetIndex()
        {
            int result = FetchLastRecordsetIndex();
            if(result >= 1 && _internalObj.AppendIndex > 0)
            {
                if(!_internalObj.IsEmtpy)
                {
                    result++; // inc for insert if data already present    
                }
            }
            else if(result == 1 && _internalObj.AppendIndex == -1)
            {
                _internalObj.AppendIndex = 2; // first pass

                var aliasDic = _internalObj.FetchAlias();

                if(aliasDic.Count > 0)
                {
                    var aliasMapping = aliasDic.FirstOrDefault();
                    var aliasMasterEntry = aliasMapping.Value.MasterEntry;
                    if(aliasMasterEntry != null)
                    {
                        if(!aliasMasterEntry.IsEmpty())
                        {
                            result++;
                        }
                    }

                }
                else
                {
                    if(!_internalObj.IsEmtpy)
                    {
                        result++;
                    }

                    if(_internalObj.Keys.IsEmpty)
                    {
                        result = 1;
                    }
                }

            }
            else if(result > 1)
            {
                result++;
            }

            return result;
        }

        /// <summary>
        /// Adjusts the index view.
        /// </summary>
        /// <param name="gaps">The gaps.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="onMasterEntry">if set to <c>true</c> [configuration master entry].</param>
        public void AdjustIndexView(HashSet<int> gaps, int min, int max, bool onMasterEntry = false)
        {
            _internalObj.MoveIndexDataForClone(min, max, gaps, onMasterEntry);
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
            _isEvalauteReady = true;

            // use only wants a specific column retained, not the entire row
            if(keepCol != null)
            {
                IList<Dev2Column> newCols = new List<Dev2Column>(Columns.Count) { new Dev2Column(keepCol, string.Empty) };
                // remove values
                Columns = newCols;
            }

            if(keepIdx == GlobalConstants.AllIndexes || _internalObj.IsEmtpy)
            {
                return;
            }

            var minIdx = (keepIdx - 1);

            _internalObj.ReInstateMaxValue(keepIdx);

            if(minIdx >= 1)
            {
                _internalObj.ReInstateMinValue(minIdx);
                _internalObj.AddGap(minIdx);
            }
        }

        public IBinaryDataListItem TryFetchRecordsetColumnAtIndex(string field, int idx, out string error)
        {
            IList<IBinaryDataListItem> cols = FetchRecordAt(idx, field, out error);
            IBinaryDataListItem result = DataListConstants.baseItem.Clone();

            if(cols == null || cols.Count == 0)
            {
                error = "Index [ " + idx + " ] is out of bounds";
            }
            else
            {
                result = cols.FirstOrDefault(c => c.FieldName == field);
            }

            return result;
        }

        public IBinaryDataListItem TryFetchLastIndexedRecordsetUpsertPayload(out string error, string field = "")
        {

            int idx = FetchLastRecordsetIndex(true);
            return !string.IsNullOrEmpty(field) ? TryFetchRecordsetColumnAtIndex(field, idx, out error) : InternalFetchIndexedRecordsetUpsertPayload(idx, out error);
        }

        public IBinaryDataListItem TryFetchIndexedRecordsetUpsertPayload(int idx, out string error)
        {
            return InternalFetchIndexedRecordsetUpsertPayload(idx, out error);
        }

        public void BlankRecordSetData(string colName)
        {
            IIndexIterator ii = _internalObj.Keys;

            if(colName != null)
            {
                Dev2Column cc = Columns.FirstOrDefault(c => c.ColumnName == colName);

                if(cc != null)
                {
                    while(ii.HasMore())
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

        public bool HasColumns(IList<Dev2Column> cols) // why refernce equals??
        {
            if (null == cols)
                return false;
            bool result = true;
            if (IsRecordset && Columns != null)
            {

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
            }
            else
            {
                return false;
            }
            return result;
        }

        public bool HasField(string field)
        {
            bool result = false;

            if(IsRecordset)
            {
                if(Columns.FirstOrDefault(c => c.ColumnName == field) != null)
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

            if(col != null)
            {
                int colIdx = Columns.IndexOf(col);

                // Port of per DLS approach -- Technical debt
                // Int
                try
                {
                    sortedData = GenericSort(toSort, colIdx, desc,GetIntSortValue);
                }
                catch(Exception)
                {

                    try
                    {
                        sortedData = GenericSort(toSort, colIdx, desc, GetFloatSortValue);
                    }
                    catch (Exception)
                    {
                        // DateTime
                        try
                        {
                            sortedData = GenericSort(toSort, colIdx, desc, GetDateTimeSortValue);
                        }
                        catch (Exception)
                        {
                            // String
                            try
                            {
                                sortedData = GenericSort(toSort, colIdx, desc, GetStringSortValue);
                            }
                            catch (Exception ex)
                            {
                                // Very naughty thing have happened....
                                error = "Invalid format for sorting on field [ " + field + " ] ";
                                this.LogError(ex);
                            }
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

        public bool TryDeleteRows(string index, out string error)
        {
            error = string.Empty;
            bool result = false;
            if(IsRecordset && index != null)
            {
                int numericIndex;
                if(!Int32.TryParse(index, out numericIndex))
                {
                    if(string.IsNullOrEmpty(index))
                    {
                        result = DeleteLastRow(out error);

                    }
                    else if(index == "*")
                    {
                        result = DeleteAllRows(out error);
                    }
                }
                else
                {
                    result = DeleteRowAtIndex(numericIndex, out error);
                }
            }
            return result;
        }

        /// <summary>
        /// Fetches the record at.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="field">The exact column needed to be fetched</param>
        /// <param name="error">The error.</param>
        /// <param name="isEntireRow"></param>
        /// <returns></returns>
        public IList<IBinaryDataListItem> FetchRecordAt(int idx, string field, out string error, bool isEntireRow = false)
        {
            error = string.Empty;

            IList<IBinaryDataListItem> result = _internalObj[idx];

            if(isEntireRow)
            {
                return result;
            }

            if(result == null)
            {
                return null;
            }

            IEnumerable<string> colNames;
            if(String.IsNullOrEmpty(field))
            {
                colNames = Columns.Select(column => column.ColumnName);
            }
            else
            {
                colNames = new List<string> { field };
            }

            IList<IBinaryDataListItem> resultList = new List<IBinaryDataListItem>();

            // NOTE : Do NOT Convert to LINQ this is the fastest!
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var colName in colNames)
            // ReSharper restore LoopCanBeConvertedToQuery
            {

                var col = result.FirstOrDefault(item => item.FieldName == colName);
                if(col != null)
                {
                    resultList.Add(col);
                }
            }

            return resultList;
        }

        public List<int> GetDistinctRows(List<string> filterCols)
        {
            return _internalObj.GetDistinctRows(filterCols);
        }

        public IDictionary<string, BinaryDataListAlias> FetchAlias()
        {
            return _internalObj.FetchAlias();
        }

        public int InternalFetchColumnIndex(string column)
        {
            return _internalObj.InternalFetchColumnIndex(column);
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Registers the scope.
        /// </summary>
        private void RegisterScope()
        {
            // Have to register with Thread ID
            DataListRegistar.RegisterDataListInScope(Thread.CurrentThread.ManagedThreadId, DataListKey);

        }

        /// <summary>
        ///     Deletes all the rows.
        /// </summary>
        /// <returns></returns>
        bool DeleteAllRows(out string error)
        {
            error = string.Empty;
            if(_internalObj.IsEmtpy)
            {
                error = "Recordset was empty.";
                return false;
            }
            int count = _internalObj.Count;
            for(int i = 1; i <= count; i++)
            {
                _internalObj.Remove(i, true);
            }

            var tmp = new SBinaryDataListEntry { IsRecordset = _internalObj.IsRecordset, Columns = _internalObj.Columns, Namespace = _internalObj.Namespace, DataListKey = _internalObj.DataListKey, Description = _internalObj.Description, IsEditable = _internalObj.IsEditable, ColumnIODirection = _internalObj.ColumnIODirection, AppendIndex = -1 };

            tmp.Init(_internalObj.Columns.Count);

            _internalObj = tmp;

            return true;
        }

        /// <summary>
        ///     Deletes the last row.
        /// </summary>
        /// <returns></returns>
        bool DeleteLastRow(out string error)
        {
            int lastRowIndex = FetchLastRecordsetIndex();
            error = string.Empty;
            // Bug 8725
            if(!_internalObj.IsEmtpy)
            {
                _internalObj.Remove(lastRowIndex, true);

                if(_internalObj.Keys.IsEmpty)
                {
                    _internalObj.IsEmtpy = true;
                }
                return true;
            }
            error = "The specified record does not exist.";
            return false;
        }

        /// <summary>
        /// Deletes the row at a specific index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool DeleteRowAtIndex(int index, out string error)
        {
            error = string.Empty;
            bool result = false;
            int lastIndex = FetchLastRecordsetIndex();
            if(index <= lastIndex && index > 0)
            {
                _internalObj.Remove(index, true);
                if(_internalObj.Keys.IsEmpty)
                {
                    _internalObj.IsEmtpy = true;
                }
                result = true;
            }
            else
            {
                error = "The specified record does not exist.";
            }

            return result;
        }





     public  static  IDictionary<int, IList<IBinaryDataListItem>> GenericSort<T>(IDictionary<int, IList<IBinaryDataListItem>> toSort, int colIdx, bool desc,Func<int , KeyValuePair<int, IList<IBinaryDataListItem>>,T> getValue )
        {
            IDictionary toSwap = new Dictionary<int, IList<IBinaryDataListItem>>();

            if (!desc)
            {
                var data = toSort.OrderBy(x => getValue(colIdx, x)).ToList();
                UpdateToSort(data, toSwap, toSort);
            }
            else
            {
                var data = toSort.OrderByDescending(x => getValue(colIdx, x)).ToList();
 
                UpdateToSort(data, toSwap,toSort);
            }

            toSort.Clear();

            // make the swap
            foreach (int k in toSwap.Keys)
            {
                toSort[k] = (IList<IBinaryDataListItem>)toSwap[k];
            }

            return toSort;
        }

        public static void UpdateToSort(List<KeyValuePair<int, IList<IBinaryDataListItem>>> data, IDictionary toSwap, IDictionary<int, IList<IBinaryDataListItem>> toSort)
        {
        
            int idx = 1;
            foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
            {
                while (!toSort.Keys.Contains(idx))
                    idx++;
                toSwap[idx] = tmp.Value;
                idx++;
            }
        }

        public static long GetIntSortValue(int colIdx, KeyValuePair<int, IList<IBinaryDataListItem>> x)
        {
            long val;
            string tmpVal = x.Value[colIdx].TheValue;
            if(string.IsNullOrWhiteSpace(tmpVal))
            {
                val = long.MinValue;
            }
            else
            {
                if(!long.TryParse(tmpVal, out val))
                {
                    throw new Exception();
                }
            }
            return val;
        }
        public static float GetFloatSortValue(int colIdx, KeyValuePair<int, IList<IBinaryDataListItem>> x)
        {
            float val;
            string tmpVal = x.Value[colIdx].TheValue;
            if (string.IsNullOrWhiteSpace(tmpVal))
            {
                val = float.NegativeInfinity;
            }
            else
            {
                if (!float.TryParse(tmpVal, out val))
                {
                    throw new Exception();
                }
            }
            return val;
        }

        public static DateTime GetDateTimeSortValue(int colIdx, KeyValuePair<int, IList<IBinaryDataListItem>> x)
        {
            DateTime val;
            string tmpVal = x.Value[colIdx].TheValue;
            if(string.IsNullOrWhiteSpace(tmpVal))
            {
                val = DateTime.MinValue;
            }
            else
            {
                if(!DateTime.TryParse(x.Value[colIdx].TheValue, out val))
                {
                    throw new Exception();
                }
            }
            return val;
        }

       public  static string GetStringSortValue(int colIdx, KeyValuePair<int, IList<IBinaryDataListItem>> x)
        {
            string val = x.Value[colIdx].TheValue;

            return val;
        }

        /// <summary>
        ///     Tries the fetch indexed recordset upsert payload.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataListItem InternalFetchIndexedRecordsetUpsertPayload(int idx, out string error)
        {
            // in this case there is a single row, with a single column's data to extract
            Dev2Column col = Columns.FirstOrDefault();
            error = string.Empty;
            if(col != null)
            {
                IBinaryDataListItem result = TryFetchRecordsetColumnAtIndex(col.ColumnName, idx, out error);
                return result;
            }

            return null;
        }

        /// <summary>
        /// Clears all.
        /// </summary>
        /// <param name="idxItr">The idx itr.</param>
        void ClearAll(IIndexIterator idxItr)
        {
            // miss, clear it all out ;)
            while(idxItr.HasMore())
            {
                int next = idxItr.FetchNextIndex();
                _internalObj.Remove(next);
            }
        }

        #endregion Private Methods

        #region Disposal Methods

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
            if(!disposing) return;
            _internalObj.Dispose();
        }

        /// <summary>
        /// Disposes the cache.
        /// </summary>
        public int DisposeCache()
        {
            var result = _internalObj.DisposeCache();
            return result;
        }

        #endregion
    }
}