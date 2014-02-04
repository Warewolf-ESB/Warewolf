using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Enums;
using Dev2.Data.Storage;
using Dev2.Data.Util;

namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    internal class BinaryDataList : IBinaryDataList
    {

        #region Properties

        public Guid UID { get; set; }
        public Guid ParentUID { get; set; }
        #endregion

        #region Attributes
        private readonly IList<string> _intellisensedNamespace;
        private IList<IDev2DataLanguageIntellisensePart> _intellisenseParts;
        private readonly IDictionary<string, IBinaryDataListEntry> _templateDict;

        #endregion

        #region Ctor
        internal BinaryDataList(Guid parentID)
        {
            UID = Guid.NewGuid();
            ParentUID = parentID;
            _intellisensedNamespace = new List<string>(GlobalConstants.DefaultColumnSizeLvl1);
            _intellisenseParts = new List<IDev2DataLanguageIntellisensePart>();
            _templateDict = new Dictionary<string, IBinaryDataListEntry>();

            // registar with process ID for clean up via the EsbServiceEndpoint's ExecuteRequest ;)
            RegisterScope();


        }

        internal BinaryDataList()
            : this(Guid.Empty)
        {
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Will create a scalar template
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="description">The description.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryCreateScalarTemplate(string theNameSpace, string fieldName, string description, bool overwrite, out string error)
        {
            return TryCreateScalarTemplate(theNameSpace, fieldName, description, overwrite, true, enDev2ColumnArgumentDirection.None, out error);
        }

        /// <summary>
        /// Will create a recordset template
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="description">The description.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryCreateRecordsetTemplate(string theNameSpace, string description, IList<Dev2Column> columns, bool overwrite, out string error)
        {
            return TryCreateRecordsetTemplate(theNameSpace, description, columns, overwrite, true, out error);
        }

        /// <summary>
        /// Create a recordset template
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="description">The description.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryCreateRecordsetTemplate(string theNameSpace, string description, IList<Dev2Column> columns, bool overwrite, bool isEditable, out string error)
        {
            return TryCreateRecordsetTemplate(theNameSpace, description, columns, overwrite, isEditable, enDev2ColumnArgumentDirection.None, out error);
        }

        /// <summary>
        /// Will create a recordset template
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="description">The description.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        /// <param name="ioDir">The io dir.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryCreateRecordsetTemplate(string theNameSpace, string description, IList<Dev2Column> columns, bool overwrite, bool isEditable, enDev2ColumnArgumentDirection ioDir, out string error)
        {
            bool result = false;
            error = string.Empty;

            if(CollectionExist(theNameSpace) && !overwrite)
            {
                error = "Template already exist for recordset [ " + theNameSpace + " ]";
            }
            else
            {
                IBinaryDataListEntry template = new BinaryDataListEntry(theNameSpace, description, columns, isEditable, ioDir, UID);
                _templateDict[theNameSpace] = template;
                result = true;

                if(!theNameSpace.Equals(GlobalConstants.SystemTagNamespace))
                {
                    // create intellisense parts for the recordset ;)
                    CreateIntelliseneResult(theNameSpace, columns);
                }

            }

            return result;
        }


        /// <summary>
        /// Will create a scalar template
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="description">The description.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        /// <param name="ioDir"></param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryCreateScalarTemplate(string theNameSpace, string fieldName, string description, bool overwrite, bool isEditable, enDev2ColumnArgumentDirection ioDir, out string error)
        {
            bool result = false;
            error = string.Empty;
            string key = CreateKey(theNameSpace, fieldName);

            if(CollectionExist(key) && !overwrite)
            {
                error = "Template already exist for scalar [ " + fieldName + " ]";
            }
            else
            {
                IBinaryDataListEntry template = new BinaryDataListEntry(key, description, isEditable, ioDir, UID);
                _templateDict[key] = template;
                result = true;
                // create scalar intellisense part ;)
                if(!theNameSpace.Equals(GlobalConstants.SystemTagNamespace))
                {
                    CreateIntelliseneResult(key);
                }
            }

            return result;
        }



        /// <summary>
        /// Returns all recordset templates
        /// </summary>
        /// <returns></returns>
        public IList<IBinaryDataListEntry> FetchRecordsetEntries()
        {
            return (from kvp in _templateDict
                    where kvp.Value.IsRecordset
                    select kvp.Value).ToList();
        }

        /// <summary>
        /// Fetch all scalar templates
        /// </summary>
        /// <returns></returns>
        public IList<IBinaryDataListEntry> FetchScalarEntries()
        {
            return (from kvp in _templateDict
                    where !kvp.Value.IsRecordset
                    select kvp.Value).ToList();
        }

        public bool TryGetEntry(string theNameSpace, out IBinaryDataListEntry entry, out string error)
        {
            error = string.Empty;

            theNameSpace = DataListUtil.StripBracketsFromValue(theNameSpace);

            bool result = _templateDict.TryGetValue(theNameSpace, out entry);
            if(!result)
            {
                error = theNameSpace + " could not be found in the DataList";
            }
            return result;
        }

        public IList<IBinaryDataListEntry> FetchAllEntries()
        {
            return _templateDict.Values.ToArray();
        }

        public IList<string> FetchAllKeys()
        {
            return (_templateDict.Keys.ToList());
        }

        public IList<string> FetchAllUserKeys()
        {
            return _templateDict.Keys.Where(key => key.IndexOf(GlobalConstants.SystemTagNamespaceSearch, StringComparison.Ordinal) < 0).ToList();
        }

        public bool TryCreateScalarValue(string value, string fieldName, out string error)
        {

            IBinaryDataListEntry tmp;
            bool result = false;
            error = "Could not locate scalar template for [ " + value + " ]";
            string key = CreateKey(string.Empty, fieldName);
            if(_templateDict.TryGetValue(key, out tmp))
            {

                if(!tmp.IsRecordset)
                {
                    tmp.TryPutScalar(new BinaryDataListItem(value, fieldName), out error);
                    _templateDict[key] = tmp; // update dic
                    result = true;
                    error = string.Empty;
                }
                else
                {
                    error = "Cannot add scalar to recordset";
                }
            }

            return result;
        }

        public bool TryCreateRecordsetValue(string value, string fieldName, string theNameSpace, int idx, out string error)
        {
            IBinaryDataListEntry tmp;
            bool result = false;
            string key = theNameSpace;

            if(_templateDict.TryGetValue(key, out tmp))
            {

                if(tmp.IsRecordset)
                {
                    IList<Dev2Column> columns = tmp.Columns;
                    Dev2Column found = columns.FirstOrDefault(column => String.Equals(column.ColumnName, fieldName, StringComparison.Ordinal));

                    if(found != null)
                    {
                        tmp.TryPutRecordItemAtIndex(new BinaryDataListItem(value, theNameSpace, fieldName, idx), idx, out error);
                        _templateDict[key] = tmp; // update dic
                        result = true;
                        error = string.Empty;
                    }
                    else
                    {
                        error = "Cannot locate field [ " + fieldName + " ] on recordset [ " + theNameSpace + " ] ";
                    }
                }
                else
                {
                    error = "Cannot add recordset to scalar";
                }
            }
            else
            {
                error = "Could not locate recordset template for [ " + value + " ]";
            }

            return result;
        }

        /*
         * Adjust Clone to simply copy the object instance
         * 
         * 
         */
        public IBinaryDataList Merge(IBinaryDataList right, enDataListMergeTypes mergeType, enTranslationDepth depth, bool newList, out ErrorResultTO errors)
        {
            IBinaryDataList mergeResult;

            if(newList)
            {
                mergeResult = Clone(depth, out errors, false);
            }
            else
            {
                mergeResult = this;
            }

            // do the merge ;)
            ((BinaryDataList)mergeResult).MergeIntoInstance(right, mergeType, depth, out errors);

            return mergeResult;
        }

        /// <summary>
        /// Clones the specified type of.
        /// </summary>
        /// <param name="depth">The depth.</param>
        /// <param name="errorResult">The error result.</param>
        /// <param name="onlySystemTags">if set to <c>true</c> [only system tags].</param>
        /// <returns></returns>
        public IBinaryDataList Clone(enTranslationDepth depth, out ErrorResultTO errorResult, bool onlySystemTags)
        {
            // set parent child reference
            BinaryDataList result = new BinaryDataList { ParentUID = ParentUID };

            errorResult = new ErrorResultTO();

            // clone the dictionary
            foreach(string e in _templateDict.Keys)
            {

                if((onlySystemTags && e.IndexOf(GlobalConstants.SystemTagNamespaceSearch, StringComparison.Ordinal) >= 0) || !onlySystemTags)
                {
                    string error;
                    // fetch this instance via clone, fetch toClone instance and merge the data
                    IBinaryDataListEntry cloned = _templateDict[e].Clone(depth, UID, out error);
                    // Copy over the intellisesne parts ;)
                    result._intellisenseParts = _intellisenseParts;
                    errorResult.AddError(error);
                    if(error == string.Empty)
                    {
                        // safe to add
                        result._templateDict[e] = cloned;
                    }
                }
            }

            // if only system tags, clean the intellisense parts out ;)
            if(onlySystemTags)
            {
                var parts =
                    result._intellisenseParts.Where(
                        c => c.Name.IndexOf(GlobalConstants.SystemTagNamespaceSearch, StringComparison.Ordinal) >= 0);
                result._intellisenseParts = parts.ToList();
            }


            return result;
        }

        /// <summary>
        /// Fetches the intellisense parts.
        /// </summary>
        /// <returns></returns>
        public IList<IDev2DataLanguageIntellisensePart> FetchIntellisenseParts()
        {
            return _intellisenseParts;
        }

        /// <summary>
        /// Determines whether this instance has errors.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </returns>
        public bool HasErrors()
        {
            string error;
            IBinaryDataListEntry entry;
            bool result = false;

            TryGetEntry(GlobalConstants.ErrorPayload, out entry, out error);
            if(entry != null)
            {
                if(entry.FetchScalar().TheValue != string.Empty)
                {
                    result = true;
                }
            }

            return result;
        }


        /// <summary>
        /// Fetches the errors.
        /// </summary>
        /// <returns></returns>
        public string FetchErrors(bool returnAsXml = false)
        {
            string error;
            IBinaryDataListEntry entry;
            string result = String.Empty;

            TryGetEntry(GlobalConstants.ErrorPayload, out entry, out error);
            if(entry != null)
            {
                result = entry.FetchScalar().TheValue;

                if(!returnAsXml)
                {
                    result = XmlHelper.MakeErrorsUserReadable(result);
                }
            }

            return result;
        }


        /// <summary>
        /// Clears the errors.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/02/06</date>
        public void ClearErrors()
        {
            IBinaryDataListEntry entry;
            string error;
            TryGetEntry(DataListUtil.BuildSystemTagForDataList(enSystemTag.Dev2Error, false), out entry, out error);
            if(entry != null)
            {
                entry.TryPutScalar(new BinaryDataListItem(string.Empty, GlobalConstants.ErrorPayload), out error);
            }

        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Registers the scope.
        /// </summary>
        private void RegisterScope()
        {
            // Have to register with Thread ID, thread ID cha
            DataListRegistar.RegisterDataListInScope(Thread.CurrentThread.ManagedThreadId, UID);

            ServerLogger.LogTrace("CREATED DATALIST [ " + UID + " ] / MEMORY USAGE NOW AT [ " + BinaryDataListStorageLayer.GetUsedMemoryInMB() + " MBs ]");

        }

        /// <summary>
        /// Creates the intellisense result.
        /// </summary>
        /// <param name="rs">The rs.</param>
        /// <param name="cols">The cols.</param>
        private void CreateIntelliseneResult(string rs, IEnumerable<Dev2Column> cols)
        {

            if(!_intellisensedNamespace.Contains(rs))
            {
                IList<IDev2DataLanguageIntellisensePart> children = cols.Select(c => DataListFactory.CreateIntellisensePart(c.ColumnName, string.Empty)).ToList();

                IDev2DataLanguageIntellisensePart p = DataListFactory.CreateIntellisensePart(rs, string.Empty, children);
                _intellisenseParts.Add(p);
                _intellisensedNamespace.Add(rs);
            }
            else
            {
                var foundExistingPart = _intellisenseParts.FirstOrDefault(part => part.Name == rs);
                if(foundExistingPart != null)
                {
                    _intellisenseParts.Remove(foundExistingPart);
                    _intellisensedNamespace.Remove(rs);
                }

                IList<IDev2DataLanguageIntellisensePart> children = cols.Select(c => DataListFactory.CreateIntellisensePart(c.ColumnName, string.Empty)).ToList();
                IDev2DataLanguageIntellisensePart p = DataListFactory.CreateIntellisensePart(rs, string.Empty, children);
                _intellisenseParts.Add(p);
                _intellisensedNamespace.Add(rs);
            }
        }

        /// <summary>
        /// Creates the intellisene result.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private void CreateIntelliseneResult(string field)
        {

            if(!_intellisensedNamespace.Contains(field) && field.IndexOf(GlobalConstants.SystemTagNamespaceSearch, StringComparison.Ordinal) < 0)
            {

                IDev2DataLanguageIntellisensePart p = DataListFactory.CreateIntellisensePart(field, string.Empty);
                _intellisenseParts.Add(p);
                _intellisensedNamespace.Add(field);
            }
        }


        /// <summary>
        /// Merges the into instance.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="errorResult">The error result.</param>
        private void MergeIntoInstance(IBinaryDataList obj, enDataListMergeTypes typeOf, enTranslationDepth depth, out ErrorResultTO errorResult)
        {

            errorResult = new ErrorResultTO();
            BinaryDataList toClone = (BinaryDataList)obj;
            if(obj.ParentUID != UID)
            {
                ParentUID = toClone.ParentUID;
            }
            IList<string> lamdaErrors = new List<string>();
            IList<string> errorList = new List<string>();
            IList<string> unionKeyHits = new List<string>();

            // clone the dictionary
            IList<string> tmp = _templateDict.Keys.ToList();  // must be this way since we modify the collection...
            foreach(string e in tmp)
            {
                string error;
                IBinaryDataListEntry cloned;

                if(typeOf == enDataListMergeTypes.Union)
                {
                    // fetch this instance via clone, fetch toClone instance and merge the data
                    IBinaryDataListEntry fetchTmp;
                    if(toClone._templateDict.TryGetValue(e, out fetchTmp))
                    {
                        unionKeyHits.Add(e);
                        cloned = fetchTmp.Clone(depth, UID, out error);
                        if(error != string.Empty)
                        {
                            lamdaErrors.Add(error);
                        }
                        else
                        {
                            DepthMerge(depth, cloned, e, out lamdaErrors);
                        }

                        // We need to ensure that the intellisense dictionary is populated with this key ;)

                    }
                }
                else if(typeOf == enDataListMergeTypes.Intersection)
                {
                    IBinaryDataListEntry toFetch;
                    if(toClone.TryGetEntry(e, out toFetch, out error))
                    {
                        cloned = toClone._templateDict[e].Clone(depth, UID, out error);
                        if(error != string.Empty)
                        {
                            lamdaErrors.Add(error);
                        }
                        else
                        {
                            DepthMerge(depth, cloned, e, out lamdaErrors);
                        }
                    }
                    else
                    {
                        lamdaErrors.Add("Missing DataList item [ " + e + " ] ");
                    }
                }

                // compile error list ?!
                foreach(string err in lamdaErrors)
                {
                    errorList.Add(err);
                }
                lamdaErrors.Clear();
            }


            // now process key misses for union
            if(typeOf == enDataListMergeTypes.Union)
            {
                //toClone._templateDict.Keys
                foreach(string k in (toClone._templateDict.Keys.ToArray().Except(unionKeyHits)))
                {
                    string error;
                    IBinaryDataListEntry cloned = toClone._templateDict[k].Clone(depth, UID, out error);
                    if(error != string.Empty)
                    {
                        lamdaErrors.Add(error);
                    }
                    else
                    {
                        DepthMerge(depth, cloned, k, out lamdaErrors);
                    }
                }
            }

            // now build the silly composite object since lamba is an daft construct
            // how about proper exception handling MS?!
            foreach(string err in errorList)
            {
                errorResult.AddError(err);
            }
        }

        /// <summary>
        /// Depths the merge.
        /// </summary>
        /// <param name="depth">The depth.</param>
        /// <param name="cloned">The cloned.</param>
        /// <param name="key"></param>
        /// <param name="errors">The errors.</param>
        private void DepthMerge(enTranslationDepth depth, IBinaryDataListEntry cloned, string key, out IList<string> errors)
        {
            errors = new List<string>();

            if(key != null)
            {

                if(depth == enTranslationDepth.Data || depth == enTranslationDepth.Data_With_Blank_OverWrite)
                {

                    // safe to add
                    if(cloned.IsRecordset)
                    {

                        // Inject into the intellisense options...
                        CreateIntelliseneResult(key, cloned.Columns);

                        //Massimo.Guerrera - 21-01-2013 - Added for the DeleteRecordOperation, it need to over write the data with blank values.
                        if(depth == enTranslationDepth.Data_With_Blank_OverWrite)
                        {
                            _templateDict[key] = cloned;
                        }
                        else
                        {
                            // merge all the cloned rows into this reference

#pragma warning disable 219
                            int insertIdx = 1; // always default to start of recordset
#pragma warning restore 219
                            // fetch last row id and build from there
                            IBinaryDataListEntry tmpRec;
                            bool isFound = _templateDict.TryGetValue(key, out tmpRec);
                            // verify that the key exist first ;)

                            IIndexIterator ii = cloned.FetchRecordsetIndexes();
                            while(ii.HasMore())
                            {
                                int next = ii.FetchNextIndex();
                                string error;
                                IList<IBinaryDataListItem> cols = cloned.FetchRecordAt(next, out error);
                                if(error != string.Empty)
                                {
                                    errors.Add(error);
                                }

                                if(!isFound)
                                {
                                    // we need to boot strap the recordset ;)
                                    // intellisense takecare of with template method ;)
                                    TryCreateRecordsetTemplate(cloned.Namespace, cloned.Description, cloned.Columns, true, out error);
                                    if(error != string.Empty)
                                    {
                                        errors.Add(error);
                                    }
                                    isFound = true;
                                }

                                foreach(IBinaryDataListItem itm in cols)
                                {
                                    _templateDict[key].TryPutRecordItemAtIndex(itm, next, out error);
                                    if(error != string.Empty)
                                    {
                                        errors.Add(error);
                                    }
                                }
                                insertIdx++;
                            }

                        }
                    }
                    else
                    {
                        IBinaryDataListEntry thisTmp;
                        // we have an entry, better check clone for empty
                        if(_templateDict.TryGetValue(key, out thisTmp))
                        {
                            if(cloned.FetchScalar().TheValue != string.Empty && depth == enTranslationDepth.Data)
                            {
                                // The clone has data, over write it on the merge ;)
                                _templateDict[key] = cloned;
                                // Inject into the intellisense options...
                                CreateIntelliseneResult(key);
                            }
                            else if(depth == enTranslationDepth.Data_With_Blank_OverWrite)
                            {
                                // The user wants to over-write Blank data on the right with existing data on the left ;)
                                _templateDict[key] = cloned;
                                // Inject into the intellisense options...
                                CreateIntelliseneResult(key);
                            }
                        }
                        else
                        {
                            // no entry, just place it there as there is no harm ;)
                            _templateDict[key] = cloned;
                            // Inject into the intellisense options...
                            CreateIntelliseneResult(key);
                        }
                    }
                }
                else if(depth == enTranslationDepth.Shape)
                {
                    _templateDict[key] = cloned; // set blank data ;)
                    // Inject into the intellisense options...
                    CreateIntelliseneResult(key);
                }
            }
        }

        /// <summary>
        /// Creates the key.
        /// </summary>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private string CreateKey(string nameSpace, string value)
        {
            string result;

            if(nameSpace == string.Empty)
            {
                result = value;
            }
            else
            {
                result = nameSpace + "." + value;
            }

            return result;
        }

        /// <summary>
        /// Collections the exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private bool CollectionExist(string key)
        {
            IBinaryDataListEntry value;

            bool result = _templateDict.TryGetValue(key, out value);

            return result;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Changed from .Dispose() to .DisposeCache() ;)
            // since this silly thing ultimately looks up all entries with the same key we only need to call this once!
            var entries = FetchAllEntries();

            if(entries.Any())
            {
                var cleanEntry = entries.FirstOrDefault();
                if(cleanEntry != null)
                {
                    int remainingEntryCnt = cleanEntry.DisposeCache();

                    this.LogTrace("There are [ " + remainingEntryCnt + " ] entries left for [ " + UID + " ]");
                }
                else
                {
                    this.LogError("Null removal entry for [ " + UID + " ]");
                }
            }
        }

        #endregion
    }
}
