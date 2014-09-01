using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Binary_Objects.Structs;
using Dev2.Data.Storage;
using Dev2.Data.Storage.Binary_Objects;
using Dev2.Data.Storage.ProtocolBuffers;

namespace Dev2.DataList.Contract.Binary_Objects.Structs
{
    [Serializable]
    public struct SBinaryDataListEntry
    {
        private IndexList _myKeys;
        private BinaryDataListStorageLayer _itemStorage;
        public int AppendIndex;
        IDictionary<string, int> _strToColIdx;
        bool _isEmpty;

        // Travis Mods ;) - Build the row TO for fetching as per the tooling ;)
        IList<IBinaryDataListItem> _internalReturnValue;
        private IDictionary<string, BinaryDataListAlias> _keyToAliasMap;

        #region Properties
        public bool IsEditable { get; set; }

        public enDev2ColumnArgumentDirection ColumnIODirection { get; set; }

        public bool IsRecordset { get; set; }

        public bool IsEvaluationScalar { get; set; }

        public bool IsManagmentServicePayload { get; set; }

        public IList<Dev2Column> Columns { get; set; }

        public string Namespace { get; set; }

        public string Description { get; set; }

        public int Count
        {
            get
            {
                if(_isEmpty)
                {
                    return 0;
                }

                return _myKeys.Count();
            }
        }

        public IIndexIterator Keys
        {
            get { return _myKeys.FetchIterator(); }
        }

        public Guid DataListKey { get; set; }

        #endregion

        #region Indexor

        public IList<IBinaryDataListItem> this[int key, bool removeFromGaps = true]
        {
            get
            {
                if(key >= 0)
                {
                    ScrubInternalTo();

                    // Generate entry 1 key, generate entry 2 key.
                    var fetchKeys = GenerateFederatedKey(key);

                    short colCnt = 1;
                    if(Columns != null)
                    {
                        colCnt = (short)Columns.Count;
                    }

                    // I am going to look up all the different pieces then, push them together?!
                    foreach(var fedKey in fetchKeys.FetchAsList())
                    {
                        BinaryDataListRow theRow;
                        _itemStorage.TryGetValue(fedKey.TheKey, colCnt, out theRow);

                        if(theRow != null)
                        {
                            var myCols = fedKey.ImpactedColumns;

                            if(myCols != null)
                            {
                                foreach(var col in myCols)
                                {
                                    // Fetch index value 
                                    var internalIdx = InternalFetchColumnIndex(col);

                                    // adjust if there is a mapping ;)
                                    // The datalist in the studio used to sort
                                    // hence we always had the correct ordering in the inner and outer xml shape
                                    // Now that this is not happening we need to account for swapped shapes
                                    // This code resolves an issue for certain cases but causes others to fail.
                                    // Need to find a better solution for the case it is trying to solve.
         

                                    // FOR : Bug_10247_Outter
                                    // if -1 skip and try next key ;) 
                                    if(internalIdx != -1)
                                    {
                                        IBinaryDataListItem tmp = _internalReturnValue[internalIdx];

                                        //                                        if(keyAlias != null)
                                        //                                        {
                                        //                                            tmp.UpdateField(col);
                                        //                                        }

                                        // normal object build
                                        tmp.UpdateValue(theRow.FetchValue(internalIdx, colCnt));
                                        tmp.UpdateIndex(key);
                                    }
                                }
                            }
                            else
                            {
                                // we have a scalar value we are dealing with ;)
                                IBinaryDataListItem tmp = _internalReturnValue[0];
                                tmp.UpdateValue(theRow.FetchValue(0, 1));
                            }
                        }
                    }

                    return _internalReturnValue;
                }

                return null;
            }

            set
            {
                if(key >= 0)
                {
                    // we need to fetch federated parts if possible ;)
                    short colCnt = 1;
                    if(Columns != null)
                    {
                        colCnt = (short)Columns.Count;
                    }

                    var fetchKeys = GenerateFederatedKey(key);

                    BinaryDataListRow parentRow = null; // Master location
                    BinaryDataListRow childRow; // Child location

                    // Fetch master location
                    if(fetchKeys.HasParentKey())
                    {
                        if(!_itemStorage.TryGetValue(fetchKeys.ParentKey.TheKey, colCnt, out parentRow))
                        {
                            throw new Exception("Fatal Internal DataList Storage Error");
                        }
                    }

                    // fetch child location
                    if(!_itemStorage.TryGetValue(fetchKeys.ChildKey.TheKey, colCnt, out childRow))
                    {
                        throw new Exception("Fatal Internal DataList Storage Error");
                    }

                    // we got the row object, now update it ;)
                    foreach(IBinaryDataListItem itm in value)
                    {
                        if(!string.IsNullOrEmpty(itm.FieldName))
                        {
                            int idx = InternalFetchColumnIndex(itm.FieldName); // Fetch correct index 
                            BinaryDataListAlias keyAlias;

                            // adjust if there is a mapping ;)
                            if(_keyToAliasMap.TryGetValue(itm.FieldName, out keyAlias))
                            {
                                var parentColumns = keyAlias.MasterEntry.Columns;
                                var parentColumn = keyAlias.MasterColumn;

                                idx = InternalParentFetchColumnIndex(parentColumn, parentColumns);

                                colCnt = (short)parentColumns.Count;
                            }

                            if(idx == -1 && !IsRecordset)
                            {
                                idx = 0; // adjust for scalar
                            }

                            if(idx >= 0)
                            {
                                // it is an alias mapping ;)
                                if(keyAlias != null)
                                {
                                    // alias update, use row 1
                                    if(parentRow != null)
                                    {
                                        parentRow.UpdateValue(itm.TheValue, idx, colCnt);
                                    }
                                }
                                else
                                {
                                    // normal update ;)
                                    childRow.UpdateValue(itm.TheValue, idx, colCnt);
                                }
                            }
                        }
                    }

                    // adjust correctly ;)
                    if((parentRow != null && !parentRow.IsEmpty) || (!childRow.IsEmpty))
                    {
                        _myKeys.SetMaxValue(key, IsEmtpy);
                    }
                    else
                    {
                        //we removed it?!
                        _myKeys.AddGap(key);
                    }

                    // update federated values ;)
                    if(parentRow != null)
                    {
                        // TODO : Make Faster, AND Adjust for when my view is different to the row's view!

                        // we need to signal master entry to update its view ;)
                        var me = fetchKeys.MasterEntry;
                        if(me != null)
                        {
                            me.AdjustIndexView(_myKeys.Gaps, _myKeys.GetMinIndex(), _myKeys.GetMaxIndex());
                        }

                        _itemStorage.TrySetValue(fetchKeys.ParentKey.TheKey, colCnt, parentRow);
                    }

                    if(childRow != null)
                    {
                        _itemStorage.TrySetValue(fetchKeys.ChildKey.TheKey, colCnt, childRow);
                    }

                }
            }
        }

        #endregion

        #region Public Methods

        public void AddAlias(Guid dlId, string parentColumn, string parentNamespace, string childColumn, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            // TODO : This needs to change so we can track at all levels what the root alias is ;)
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            Guid masterId = dlId;
            string masterRs = parentNamespace;
            string masterCol = parentColumn;
            Guid searchId = dlId;

            IBinaryDataListEntry masterEntry = null;

            int aliasSearchRounds = 0;
            BinaryDataListAlias binaryDataListAlias = null;

            while(searchId != Guid.Empty)
            {
                ErrorResultTO invokeErrors;
                var bdl = compiler.FetchBinaryDataList(searchId, out invokeErrors);
                errors.MergeErrors(invokeErrors);


                if(bdl != null)
                {
                    string error;
                    bdl.TryGetEntry(masterRs, out masterEntry, out error);
                    errors.AddError(error);

                    if(masterEntry != null)
                    {
                        var aliases = masterEntry.FetchAlias();

                        if(aliases.TryGetValue(masterCol, out binaryDataListAlias))
                        {
                            // we have a hit ;)
                            masterId = binaryDataListAlias.MasterKeyID;
                            searchId = masterId;
                            masterRs = binaryDataListAlias.MasterNamespace;
                            masterCol = binaryDataListAlias.MasterColumn;
                            aliasSearchRounds++;
                        }
                        else
                        {
                            // ensure we copy over the alias entry's keys ;)
                            if(IsEmtpy)
                            {
                                var keyItr = masterEntry.FetchRecordsetIndexes();

                                _myKeys = new IndexList(keyItr.FetchGaps(), keyItr.MaxIndex(), keyItr.MinIndex());

                                IsEmtpy = false;
                            }

                            searchId = Guid.Empty; // signal end ;)
                        }
                    }
                    else
                    {
                        if(aliasSearchRounds == 0)
                        {
                            throw new Exception("Missing Entry");
                        }
                        // we hit the bottom earlier, handle it ;)
                        if(binaryDataListAlias != null)
                        {
                            masterEntry = binaryDataListAlias.MasterEntry;
                        }
                        searchId = Guid.Empty; // signal end ;)
                    }

                }
                else
                {
                    throw new Exception("Missing DataList");
                }
            }


            // Check MasterKeyID to see if it contains an alias, if so keep bubbling until we at end ;)
            _keyToAliasMap[childColumn] = new BinaryDataListAlias { MasterKeyID = masterId, ChildKey = GenerateKeyPrefix(Namespace, DataListKey), MasterKey = GenerateKeyPrefix(masterRs, masterId), MasterColumn = masterCol, MasterNamespace = masterRs, MasterEntry = masterEntry };
        }

        public void AddGap(int idx)
        {
            _myKeys.AddGap(idx);
        }

        public void ReInstateMinValue(int idx)
        {
            _myKeys.MinValue = idx;
        }

        public void ReInstateMaxValue(int idx)
        {
            _myKeys.MaxValue = idx;
            _myKeys.RemoveGap(idx);
        }

        public void MoveIndexDataForClone(int min, int max, HashSet<int> gaps, bool onMasterEntry)
        {
            // signal that we have data here ;)
            AppendIndex = 2;
            _isEmpty = false;

            _myKeys.MinValue = min;
            _myKeys.MaxValue = max;
            _myKeys.SetGapsCollection(new HashSet<int>(gaps));
        }

        public HashSet<int> FetchGaps()
        {
            return _myKeys.Gaps;
        }

        public IDictionary<string, BinaryDataListAlias> FetchAlias()
        {
            return _keyToAliasMap;
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public void Init(int colCnt, bool createNewStorage = true)
        {
            if(createNewStorage)
            {
                _itemStorage = new BinaryDataListStorageLayer();
            }

            // Construct the alias map for this entry ;)
            _keyToAliasMap = new Dictionary<string, BinaryDataListAlias>();

            _myKeys = new IndexList(null, 1);

            _isEmpty = true;
            _internalReturnValue = new List<IBinaryDataListItem>(colCnt); // build the object we require to return data in ;)

            for(int i = 0; i < colCnt; i++)
            {
                _internalReturnValue.Add(new BinaryDataListItem(string.Empty, string.Empty));

                if(Columns != null)
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

            // boot strap column cache ;)
            short cnt = 1;
            if(Columns != null)
            {
                cnt = (short)Columns.Count;
            }
            _strToColIdx = new Dictionary<string, int>(cnt);

        }

        public int InternalParentFetchColumnIndex(string column, IList<Dev2Column> parentColumns)
        {
            int result = -1;
            if(IsRecordset)
            {

                Dev2Column colToFind = parentColumns.FirstOrDefault(c => c.ColumnName == column);

                if(colToFind != null)
                {
                    result = parentColumns.IndexOf(colToFind);
                }
                else
                {
                    result = -1; // it failed, default back to non-valid index
                }

            }

            return result;
        }

        public int InternalFetchColumnIndex(string column)
        {
            int result = -1;
            if(IsRecordset)
            {
                if(!_strToColIdx.TryGetValue(column, out result))
                {
                    Dev2Column colToFind = Columns.FirstOrDefault(c => c.ColumnName == column);

                    if(colToFind != null)
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

        #region Disposal

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            _itemStorage.Dispose();
        }

        /// <summary>
        /// Disposes the cache.
        /// </summary>
        public int DisposeCache()
        {
            return _itemStorage.DisposeCache(DataListKey.ToString());
        }

        #endregion

        public bool ContainsRow(int idx)
        {

            if(IsEmtpy)
            {
                return false;
            }

            return _myKeys.Contains(idx);
        }

        /// <summary>
        /// Adds the specified idx.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="cols">The cols.</param>
        public void Add(int idx, IList<IBinaryDataListItem> cols)
        {
            var colCnt = (short)cols.Count;

            BinaryDataListRow row = new BinaryDataListRow(colCnt);

            foreach(IBinaryDataListItem itm in cols)
            {
                row.UpdateValue(itm.TheValue, idx, colCnt);
            }
        }

        /// <summary>
        /// Removes the specified idx.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="blankData"></param>
        public void Remove(int idx, bool blankData = false)
        {
            _myKeys.AddGap(idx);
            if(blankData)
            {
                IList<IBinaryDataListItem> binaryDataListItems = this[idx];
                foreach(var dev2Column in binaryDataListItems)
                {
                    dev2Column.UpdateValue(string.Empty);
                }

                this[idx, false] = binaryDataListItems;
            }
        }


        /// <summary>
        /// Applies the sort action.
        /// </summary>
        public void ApplySortAction(IDictionary<int, IList<IBinaryDataListItem>> payload)
        {
            // Apply IDic back into my object ;)
            foreach(int i in payload.Keys)
            {
                // TODO : What if they are in different locations?

                StorageKey sk = new StorageKey(DataListKey, i + GenerateKeyPrefix(Namespace, DataListKey));

                BinaryDataListRow row;

                short colCnt = (short)Columns.Count;

                if(_itemStorage.TryGetValue(sk, colCnt, out row))
                {
                    IList<IBinaryDataListItem> cols;
                    if(payload.TryGetValue(i, out cols))
                    {
                        foreach(IBinaryDataListItem c in cols)
                        {
                            int idx = InternalFetchColumnIndex(c.FieldName);
                            row.UpdateValue(c.TheValue, idx, colCnt);
                        }

                        _itemStorage.TrySetValue(sk, colCnt, row);
                    }
                }
            }
        }

        public IDictionary<int, IList<IBinaryDataListItem>> FetchSortData()
        {
            IDictionary<int, IList<IBinaryDataListItem>> result = new Dictionary<int, IList<IBinaryDataListItem>>(Count);
            IIndexIterator ii = Keys;
            if(IsRecordset)
            {
                short colCnt = (short)Columns.Count;

                while(ii.HasMore())
                {
                    // TODO : What if they are in different locations?

                    IList<IBinaryDataListItem> tmp = new List<IBinaryDataListItem>(Columns.Count);
                    BinaryDataListRow row;
                    int next = ii.FetchNextIndex();

                    // Hi-jack lookup

                    StorageKey sk = new StorageKey(DataListKey, next + GenerateKeyPrefix(Namespace, DataListKey));

                    if(_itemStorage.TryGetValue(sk, colCnt, out row))
                    {
                        for(int i = 0; i < Columns.Count; i++)
                        {
                            tmp.Add(new BinaryDataListItem(row.FetchValue(i, colCnt), Namespace, Columns[i].ColumnName, next));
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
                // - New
                if(_isEmpty && _myKeys.Count() > 1)
                {
                    return false;
                }


                return _isEmpty;
            }
            set
            {
                _isEmpty = value;
            }
        }

        #endregion

        public List<int> GetDistinctRows(List<string> filterCols)
        {
            var filterColIndexes = new List<int>();
            var tmpThis = this;
            filterCols.ForEach(filterCol => filterColIndexes.Add(tmpThis.InternalFetchColumnIndex(filterCol)));

            // TODO : Hi-jack lookup

            StorageKey sk = new StorageKey(DataListKey, GenerateKeyPrefix(Namespace, DataListKey));

            var distinctBinaryDataListRows = _itemStorage.DistinctGetRows(sk, Keys, filterColIndexes);
            return distinctBinaryDataListRows;
        }

        /// <summary>
        /// Copies the data from the clonable instance into the cloned instance ;)
        /// </summary>
        /// <param name="thisObj">The this object.</param>
        public void CopyTo(SBinaryDataListEntry thisObj)
        {

            // This is dangerous. We need to check for alias keys and use them instead ;)
            var keys = thisObj._myKeys;

            HashSet<int> gaps = keys.Gaps;
            int min = keys.MinValue;
            int max = keys.MaxValue;

            Columns = thisObj.Columns;
            _itemStorage = thisObj._itemStorage;
            // avoid referencing issues ;)
            _myKeys = new IndexList(gaps, max, min);
            DataListKey = thisObj.DataListKey;
            IsEmtpy = thisObj.IsEmtpy;
            IsRecordset = thisObj.IsRecordset;
            IsEvaluationScalar = thisObj.IsEvaluationScalar;
            Namespace = thisObj.Namespace;
            IsManagmentServicePayload = thisObj.IsManagmentServicePayload;
            _strToColIdx = thisObj._strToColIdx;
            _keyToAliasMap = thisObj._keyToAliasMap;


        }

        #region Private Methods

        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <returns></returns>
        private InternalFederatedTO GenerateFederatedKey(int idx)
        {
            InternalFederatedTO result = new InternalFederatedTO();

            // we need two keys ;)
            var firstKey = _keyToAliasMap.Keys.FirstOrDefault();
            ICollection<string> entry1Columns = null;

            // fetch alias key first ;)
            if(firstKey != null)
            {
                var magicKey = _keyToAliasMap[firstKey];

                var sk1 = new StorageKey(magicKey.MasterKeyID, idx + magicKey.MasterKey);
                entry1Columns = _keyToAliasMap.Keys;
                result.ParentKey = new FederatedStorageKey { TheKey = sk1, ImpactedColumns = entry1Columns };

                // set master entry for the federation ;)
                result.MasterEntry = magicKey.MasterEntry;
            }

            // now create the child key
            var sk2 = new StorageKey(DataListKey, idx + GenerateKeyPrefix(Namespace, DataListKey));

            result.ChildKey = new FederatedStorageKey { TheKey = sk2, ImpactedColumns = FilterColumns(entry1Columns) };

            return result;
        }

        private IList<string> FilterColumns(ICollection<string> entry1Columns)
        {
            IList<string> result = new List<string>();

            if(Columns != null)
            {
                if(entry1Columns != null)
                {
                    foreach(var col in Columns)
                    {
                        if(!entry1Columns.Contains(col.ColumnName))
                        {
                            result.Add(col.ColumnName);
                        }
                    }
                }
                else
                {
                    // we want all of them ;)
                    foreach(var col in Columns)
                    {
                        result.Add(col.ColumnName);
                    }
                }
            }
            else
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Generates the storage key.
        /// </summary>
        /// <param name="ns">The constant.</param>
        /// <param name="id">The unique identifier.</param>
        /// <returns></returns>
        private string GenerateKeyPrefix(string ns, Guid id)
        {
            return ns + id;
        }

        /// <summary>
        /// Scrubs the internal TO.
        /// </summary>
        private void ScrubInternalTo()
        {
            if(_internalReturnValue == null)
            {
                int cnt = 1;

                if(Columns != null)
                {
                    cnt = Columns.Count;
                }

                _internalReturnValue = new List<IBinaryDataListItem>(cnt);
            }

            foreach(IBinaryDataListItem t in _internalReturnValue)
            {
                t.ToClear();
            }
        }

        #endregion
    }

}
