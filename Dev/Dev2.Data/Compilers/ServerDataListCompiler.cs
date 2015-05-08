
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

// ReSharper disable CheckNamespace
namespace Dev2.Server.Datalist
// ReSharper restore CheckNamespace
{

    /// <summary>
    /// Server DataList compiler
    /// </summary>
    internal class ServerDataListCompiler : IEnvironmentModelDataListCompiler
    {
        // ReSharper disable InconsistentNaming
        // DataList Server
        private readonly IDataListServer _dlServer;

        internal ServerDataListCompiler(IDataListServer dlServ)
        {
            _dlServer = dlServ;
        }

        #region Private Method

        #endregion

        public Guid CloneDataList(Guid curDLID, out ErrorResultTO errors)
        {
            var allErrors = new ErrorResultTO();
            string error;

            Guid res = GlobalConstants.NullDataListID;

            IBinaryDataList tmpDl = TryFetchDataList(curDLID, out error);
            if(error != string.Empty)
            {
                allErrors.AddError(error);
            }
            else
            {
                // Ensure we have a non-null tmpDL

                IBinaryDataList result = tmpDl.Clone(Dev2.DataList.Contract.enTranslationDepth.Data, out errors, false);
                if(result != null)
                {
                    allErrors.MergeErrors(errors);
                    TryPushDataList(result, out error);
                    allErrors.AddError(error);

                    res = result.UID;
                }

            }

            errors = allErrors;

            return res;
        }

        public IBinaryDataList FetchBinaryDataList(NetworkContext ctx, Guid curDLID, out ErrorResultTO errors)
        {

            string error;

            IBinaryDataList result = TryFetchDataList(curDLID, out error);
            errors = new ErrorResultTO();

            if(result == null)
            {
                errors.AddError(error);
            }

            return result;
        }

        public Guid Merge(NetworkContext ctx, Guid leftID, Guid rightID, enDataListMergeTypes mergeType, Dev2.DataList.Contract.enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {

            string error;
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid returnVal = Guid.Empty;

            IBinaryDataList left = TryFetchDataList(leftID, out error);
            if(left == null)
            {
                allErrors.AddError(error);
            }

            IBinaryDataList right = TryFetchDataList(rightID, out error);
            if(right == null)
            {
                allErrors.AddError(error);
            }

            // alright to merge
            if(right != null && left != null)
            {
                IBinaryDataList result = left.Merge(right, mergeType, depth, createNewList, out errors);
                if(errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
                else
                {
                    // Push back into the server now ;)
                    if(!TryPushDataList(result, out error))
                    {
                        allErrors.AddError(error);
                    }
                }
                returnVal = result.UID;
            }
            else
            {
                allErrors.AddError("Cannot merge since both DataList cannot be found!");
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public Guid ConditionalMerge(NetworkContext ctx, DataListMergeFrequency conditions,
            Guid destinationDatalistID, Guid sourceDatalistID, DataListMergeFrequency datalistMergeFrequency,
            enDataListMergeTypes datalistMergeType, Dev2.DataList.Contract.enTranslationDepth datalistMergeDepth)
        {
            Guid mergeId = Guid.Empty;
            if(conditions.HasFlag(datalistMergeFrequency) && destinationDatalistID != Guid.Empty && sourceDatalistID != Guid.Empty)
            {
                ErrorResultTO errors;
                mergeId = Merge(ctx, destinationDatalistID, sourceDatalistID, datalistMergeType, datalistMergeDepth, false, out errors);
            }

            return mergeId;
        }

        public Guid TransferSystemTags(NetworkContext ctx, Guid parentDLID, Guid childDLID, bool parentToChild, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(NetworkContext ctx, Guid id, StateType direction)
        {
            return null;
        }

        public bool DeleteDataListByID(Guid curDLID, bool onlyIfNotPersisted)
        {
            bool result = _dlServer.DeleteDataList(curDLID, onlyIfNotPersisted);

            return result;
        }

        /// <summary>
        /// Populates the data list.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="input">The input.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="targetDLID">The target dlid.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PopulateDataList(NetworkContext ctx, DataListFormat typeOf, object input, string outputDefs, Guid targetDLID, out ErrorResultTO errors)
        {

            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if(t != null)
                {
                    returnVal = t.Populate(input, targetDLID, outputDefs, out errors);
                    allErrors.MergeErrors(errors);
                }
                else
                {
                    allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public Guid ConvertAndOnlyMapInputs(NetworkContext ctx, DataListFormat typeOf, byte[] payload, StringBuilder shape, out ErrorResultTO errors)
        {
            // _repo
            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if(t != null)
                {
                    IBinaryDataList result = t.ConvertAndOnlyMapInputs(payload, shape, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }

                    if(result != null)
                    {
                        // set the uid and place in cache
                        returnVal = result.UID;

                        string error;
                        if(!TryPushDataList(result, out error))
                        {
                            allErrors.AddError(error);
                        }
                    }
                }
                else
                {
                    allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public Guid ConvertTo(NetworkContext ctx, DataListFormat typeOf, object payload, StringBuilder shape, out ErrorResultTO errors)
        {
            // _repo
            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if(t != null)
                {
                    IBinaryDataList result = t.ConvertTo(payload, shape, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }

                    if(result != null)
                    {
                        // set the uid and place in cache
                        returnVal = result.UID;

                        string error;
                        if(!TryPushDataList(result, out error))
                        {
                            allErrors.AddError(error);
                        }
                    }
                }
                else
                {
                    allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public Guid ConvertTo(NetworkContext ctx, DataListFormat typeOf, byte[] payload, StringBuilder shape, out ErrorResultTO errors)
        {

            // _repo
            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if(t != null)
                {
                    IBinaryDataList result = t.ConvertTo(payload, shape, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }

                    if(result != null)
                    {
                        // set the uid and place in cache
                        returnVal = result.UID;

                        string error;
                        if(!TryPushDataList(result, out error))
                        {
                            allErrors.AddError(error);
                        }
                    }
                }
                else
                {
                    allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public DataListTranslatedPayloadTO ConvertFrom(NetworkContext ctx, Guid curDLID, Dev2.DataList.Contract.enTranslationDepth depth, DataListFormat typeOf, out ErrorResultTO errors)
        {
            DataListTranslatedPayloadTO returnVal = null;
            ErrorResultTO allErrors = new ErrorResultTO();
            string error;

            IBinaryDataList result = TryFetchDataList(curDLID, out error);

            if(result != null)
            {
                try
                {

                    IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                    if(t != null)
                    {
                        returnVal = t.ConvertFrom(result, out errors);
                        if(errors.HasErrors())
                        {
                            allErrors.MergeErrors(errors);
                        }
                    }
                    else
                    {
                        allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                    }
                }
                catch(Exception e)
                {
                    allErrors.AddError(e.Message);
                }
            }
            else
            {
                allErrors.AddError(error);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public StringBuilder ConvertAndFilter(NetworkContext ctx, Guid curDLID, StringBuilder filterShape, DataListFormat typeOf, out ErrorResultTO errors)
        {
            var res = new StringBuilder();
            ErrorResultTO allErrors = new ErrorResultTO();
            string error;

            IBinaryDataList result = TryFetchDataList(curDLID, out error);

            if(result != null)
            {
                try
                {

                    IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                    if(t != null)
                    {
                        res = t.ConvertAndFilter(result, filterShape, out errors);
                        allErrors.MergeErrors(errors);
                    }
                    else
                    {
                        allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                    }
                }
                catch(Exception e)
                {
                    allErrors.AddError(e.Message);
                }
            }
            else
            {
                allErrors.AddError(error);
            }

            // assign error var
            errors = allErrors;

            return res;
        }

        public void SetParentUID(Guid curDLID, Guid parentID, out ErrorResultTO errors)
        {
            string error;
            errors = new ErrorResultTO();
            IBinaryDataList bdl = TryFetchDataList(curDLID, out error);
            if(error != string.Empty)
            {
                errors.AddError(error);
            }
            if(bdl != null)
            {
                bdl.ParentUID = parentID;
                TryPushDataList(bdl, out error);
                if(error != string.Empty)
                {
                    errors.AddError(error);
                }
            }
        }

        public IList<DataListFormat> FetchTranslatorTypes()
        {
            return _dlServer.FetchTranslatorTypes();
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(DataListFormat.CreateFormat(GlobalConstants._DATATABLE));
                return t.ConvertToDataTable(input, recsetName, out errors, populateOptions);
            }
            catch(Exception e)
            {
                errors.AddError(e.Message);
            }
            return null;
        }

        #region Private Methods

        public Func<IDev2Definition, string> BuildInputExpressionExtractor(enDev2ArgumentType typeOf)
        {
            switch(typeOf)
            {
                case enDev2ArgumentType.Input:
                    return def =>
                    {
                        string expression = string.Empty;
                        if(def.Value == string.Empty)
                        {
                            if(def.DefaultValue != string.Empty)
                            {
                                expression = def.DefaultValue;
                            }
                        }
                        else
                        {
                            expression = def.RawValue;
                        }

                        return expression;
                    };
                case enDev2ArgumentType.Output:
                case enDev2ArgumentType.DB_ForEach:
                    return def =>
                            {
                                string expression;

                                if(def.RecordSetName != string.Empty)
                                {
                                    expression = def.RecordSetName + "(*)." + def.Name; // star because we are fetching all to place into the parentDataList
                                }
                                else
                                {
                                    expression = def.Name;
                                }

                                return "[[" + expression + "]]";
                            };
                case enDev2ArgumentType.Output_Append_Style:
                    return def =>
                            {
                                string expression;

                                if(def.RecordSetName != string.Empty)
                                {
                                    expression = def.RecordSetName + "()." + def.Name; // () because we are fetching last row to append
                                }
                                else
                                {
                                    expression = def.Name;
                                }

                                return "[[" + expression + "]]";
                            };
                default:
                    throw new ArgumentOutOfRangeException("typeOf");
            }

        }

        public Func<IDev2Definition, string> BuildOutputExpressionExtractor(enDev2ArgumentType typeOf)
        {
            switch(typeOf)
            {
                case enDev2ArgumentType.Input:
                    return def =>
                            {
                                string expression;

                                if(string.IsNullOrEmpty(def.RecordSetName))
                                {
                                    expression = def.Name;
                                }
                                else
                                {
                                    expression = def.RecordSetName + "(*)." + def.Name;
                                }

                                return DataListUtil.AddBracketsToValueIfNotExist(expression);
                            };
                case enDev2ArgumentType.Output:
                case enDev2ArgumentType.Output_Append_Style:
                case enDev2ArgumentType.DB_ForEach:
                    return def =>
                        {
                            string expression = string.Empty;

                            if(def.Value != string.Empty)
                            {
                                expression = def.RawValue;
                            }

                            return expression;
                        };
                default:
                    throw new ArgumentOutOfRangeException("typeOf");
            }

        }

        /// <summary>
        /// Tries the fetch data list.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private IBinaryDataList TryFetchDataList(Guid id, out string error)
        {

            ErrorResultTO errors;
            error = string.Empty;

            IBinaryDataList result = _dlServer.ReadDatalist(id, out errors);

           return result;
        }

        /// <summary>
        /// Tries the push data list.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryPushDataList(IBinaryDataList payload, out string error)
        {
            bool r = true;
            error = string.Empty;
            ErrorResultTO errors;

            if(!_dlServer.WriteDataList(payload.UID, payload, out errors))
            {
                error = "Failed to write DataList";
                r = false; 
            }

            return r;
        }

        /// <summary>
        /// Determines whether the specified payload is evaluated.
        /// NOTE: This method also exist in the DataListUtil class
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is evaluated; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEvaluated(string payload)
        {
            if(payload == null)
            {
                return false;
            }

            var result = payload.IndexOf("[[", StringComparison.Ordinal) >= 0;

            return result;
        }

        #endregion

        // ReSharper restore InconsistentNaming

    }
}
