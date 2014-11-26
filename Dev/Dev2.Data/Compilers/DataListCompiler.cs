
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
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Enums;
using Dev2.Data.Interfaces;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.TO;
using Dev2.Server.Datalist;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
// ReSharper restore CheckNamespace
{
    internal class DataListCompiler : IDataListCompiler
    {
        #region Attributes

        private readonly object _disposeGuard = new object();
        private bool _isDisposed;

        // New Stuff
        private static readonly IDev2LanguageParser OutputParser = DataListFactory.CreateOutputParser();
        private static readonly IDev2LanguageParser InputParser = DataListFactory.CreateInputParser();
        private static readonly IDev2DataLanguageParser Parser = DataListFactory.CreateLanguageParser();

        // These are tags to strip from the ADL for ExtractShapeFromADLAndCleanWithDefs used with ShapeInput ;)

        private Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>();
        private IEnvironmentModelDataListCompiler _svrCompiler;
        #endregion

        internal DataListCompiler(IEnvironmentModelDataListCompiler svrC)
        {
            // TODO : Allow IP to be sent when using the DataList compiler...
            _svrCompiler = svrC;
        }

        // Travis.Frisinger : 29.10.2012 - New DataListCompiler Methods
        #region New Methods

        /// <summary>
        /// Clones the data list.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid CloneDataList(Guid curDlid, out ErrorResultTO errors)
        {
            return _svrCompiler.CloneDataList(curDlid, out errors);
        }

        /// <summary>
        /// Used to evaluate an expression against a given datalist
        /// </summary>
        /// <param name="curDlid">The cur DL ID.</param>
        /// <param name="typeOf">The type of evaluation.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="toRoot"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public IBinaryDataListEntry Evaluate(Guid curDlid, enActionType typeOf, string expression, bool toRoot, out ErrorResultTO errors)
        {
            return _svrCompiler.Evaluate(null, curDlid, typeOf, expression, out errors, toRoot);
        }

        /// <summary>
        /// Generates the wizard data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <param name="withData"></param>
        /// <returns></returns>
        public StringBuilder GenerateWizardDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors, bool withData = false)
        {
            IList<IDev2Definition> defs;
            IList<IDev2Definition> wizdefs = new List<IDev2Definition>();

            if(defType == enDev2ArgumentType.Output)
            {
                defs = OutputParser.ParseAndAllowBlanks(definitions);

                foreach(IDev2Definition def in defs)
                {
                    wizdefs.Add(def.IsRecordSet ? DataListFactory.CreateDefinition(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue) : DataListFactory.CreateDefinition(def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                }
            }
            else if(defType == enDev2ArgumentType.Input)
            {
                defs = InputParser.Parse(definitions);
                foreach(IDev2Definition def in defs)
                {
                    wizdefs.Add(def.IsRecordSet ? DataListFactory.CreateDefinition(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue) : DataListFactory.CreateDefinition(def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                }
            }

            return GenerateDataListFromDefs(wizdefs, pushToServer, out errors, withData);
        }

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public StringBuilder GenerateDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors)
        {
            IList<IDev2Definition> defs = new List<IDev2Definition>();

            if(defType == enDev2ArgumentType.Output)
            {
                defs = OutputParser.ParseAndAllowBlanks(definitions);
            }
            else if(defType == enDev2ArgumentType.Input)
            {
                defs = InputParser.Parse(definitions);
            }

            return GenerateDataListFromDefs(defs, pushToServer, out errors);
        }

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions as binary objects</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server]. the GUID is returned</param>
        /// <param name="errors">The errors.</param>
        /// <param name="withData"></param>
        /// <returns></returns>
        public StringBuilder GenerateDataListFromDefs(IList<IDev2Definition> definitions, bool pushToServer, out ErrorResultTO errors, bool withData = false)
        {
            errors = new ErrorResultTO();
            var dataList = GenerateDataListFromDefs(definitions, withData);
            StringBuilder result;

            if(pushToServer)
            {
                byte[] data = new byte[0];
                result = new StringBuilder(_svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString());
            }
            else
            {
                result = dataList;
            }

            return result;
        }

        /// <summary>
        /// Shapes the dev2 definitions to data list.
        /// </summary>
        /// <param name="definitions">The definitions as string</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <param name="flipGeneration">if set to <c>true</c> [flip generation].</param>
        /// <returns></returns>
        public StringBuilder ShapeDev2DefinitionsToDataList(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors, bool flipGeneration = false)
        {
            var dataList = ShapeDefinitionsToDataList(definitions, defType, out errors, flipGeneration);
            StringBuilder result;

            if(pushToServer)
            {
                //  Push to server and return GUID
                byte[] data = new byte[0];
                result = new StringBuilder(_svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString());
            }
            else
            {
                result = dataList; // else return the datalist as requested
            }

            return result;
        }

        // Travis.Frisinger - 29.01.2013 : Bug 8412
        /// <summary>
        /// Shapes the dev2 definitions to data list.
        /// </summary>
        /// <param name="definitions">The definitions as binary objects</param>
        /// <param name="defType">Type of the def Input or Output</param>
        /// <param name="pushToServer"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public StringBuilder ShapeDev2DefinitionsToDataList(IList<IDev2Definition> definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors)
        {

            ErrorResultTO allErrors = new ErrorResultTO();
            var dataList = DataListUtil.ShapeDefinitionsToDataList(definitions, defType, out errors);
            allErrors.MergeErrors(errors);
            errors.ClearErrors();

            // ReSharper disable RedundantAssignment
            StringBuilder result = new StringBuilder(GlobalConstants.NullDataListID.ToString());
            // ReSharper restore RedundantAssignment

            if(pushToServer)
            {
                //  Push to server and return GUID
                byte[] data = new byte[0];
                result = new StringBuilder(_svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString());
            }
            else
            {
                result = dataList; // else return the datalist as requested
            }

            return result;
        }

        /// <summary>
        /// Fetches the binary data list.
        /// </summary>
        /// <param name="curDlid">The cur DL ID.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public IBinaryDataList FetchBinaryDataList(Guid curDlid, out ErrorResultTO errors)
        {
            return _svrCompiler.FetchBinaryDataList(null, curDlid, out errors);
        }

        /// <summary>
        /// Upserts the value to the specified cur DL ID's expression.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid Upsert(Guid curDlid, string expression, IBinaryDataListEntry value, out ErrorResultTO errors)
        {
            return _svrCompiler.Upsert(null, curDlid, expression, value, out errors);
        }

        /// <summary>
        /// Upserts the values against the specified cur DL ID's expression list.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="values">The values.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid Upsert(Guid curDlid, IList<string> expressions, IList<IBinaryDataListEntry> values, out ErrorResultTO errors)
        {
            return _svrCompiler.Upsert(null, curDlid, expressions, values, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="values">The values.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDlid, IList<string> expressions, IList<string> values, out ErrorResultTO errors)
        {
            return _svrCompiler.Upsert(null, curDlid, expressions, values, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDlid, string expression, string value, out ErrorResultTO errors)
        {
            //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in regular upsert
            var allRegions = DataListCleaningUtils.SplitIntoRegions(expression);
            var allValues = allRegions.Select(region => value).ToList();
            return _svrCompiler.Upsert(null, curDlid, allRegions, allValues, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDlid, IDev2DataListUpsertPayloadBuilder<string> payload, out ErrorResultTO errors)
        {
            return _svrCompiler.Upsert(null, curDlid, payload, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDlid, IDev2DataListUpsertPayloadBuilder<List<string>> payload, out ErrorResultTO errors)
        {
            return _svrCompiler.Upsert(null, curDlid, payload, out errors);
        }

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid Upsert(Guid curDlid, IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload, out ErrorResultTO errors)
        {
            return _svrCompiler.Upsert(null, curDlid, payload, out errors);
        }

        /// <summary>
        /// Shapes the specified current dlid.
        /// </summary>
        /// <param name="curDlid">The current dlid.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="defs">The defs.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="overrideID">The override unique identifier.</param>
        /// <returns></returns>
        public Guid Shape(Guid curDlid, enDev2ArgumentType typeOf, string defs, out ErrorResultTO errors, Guid overrideID = default(Guid))
        {
            return _svrCompiler.Shape(null, curDlid, typeOf, defs, out errors, overrideID);
        }

        /// <summary>
        /// Shapes the definitions in binary form to create/amended a DL.
        /// </summary>
        /// <param name="curDlid">The cur DL ID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="definitions">The definitions.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid Shape(Guid curDlid, enDev2ArgumentType typeOf, IList<IDev2Definition> definitions, out ErrorResultTO errors)
        {
            return _svrCompiler.Shape(null, curDlid, typeOf, definitions, out errors);
        }

        /// <summary>
        /// Shapes for sub execution.
        /// </summary>
        /// <param name="parentDlid">The parent dlid.</param>
        /// <param name="childDlid">The child dlid.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubExecution(Guid parentDlid, Guid childDlid, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            return _svrCompiler.ShapeForSubExecution(null, parentDlid, childDlid, inputDefs, outputDefs, out errors);
        }


        /// <summary>
        /// Merges the specified left ID with the right ID
        /// </summary>
        /// <param name="leftID">The left ID.</param>
        /// <param name="rightID">The right ID.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="depth"></param>
        /// <param name="createNewList"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid Merge(Guid leftID, Guid rightID, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {
            return _svrCompiler.Merge(null, leftID, rightID, mergeType, depth, createNewList, out errors);
        }

        /// <summary>
        /// Merges the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="createNewList">if set to <c>true</c> [create new list].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList Merge(IBinaryDataList left, IBinaryDataList right, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {
            return (left.Merge(right, mergeType, depth, createNewList, out errors));
        }

        /// <summary>
        /// Conditionals the merge.
        /// </summary>
        /// <param name="conditions">The conditions.</param>
        /// <param name="destinationDatalistID">The destination datalist ID.</param>
        /// <param name="sourceDatalistID">The source datalist ID.</param>
        /// <param name="datalistMergeFrequency">The datalist merge frequency.</param>
        /// <param name="datalistMergeType">Type of the datalist merge.</param>
        /// <param name="datalistMergeDepth">The datalist merge depth.</param>
        public void ConditionalMerge(DataListMergeFrequency conditions,
            Guid destinationDatalistID, Guid sourceDatalistID, DataListMergeFrequency datalistMergeFrequency,
            enDataListMergeTypes datalistMergeType, enTranslationDepth datalistMergeDepth)
        {
            _svrCompiler.ConditionalMerge(null, conditions, destinationDatalistID, sourceDatalistID, datalistMergeFrequency, datalistMergeType, datalistMergeDepth);
        }

        /// <summary>
        /// Upserts the system tag, keep val == string.Empty to erase the tag
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="val">The val.</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid UpsertSystemTag(Guid curDlid, enSystemTag tag, string val, out ErrorResultTO errors)
        {
            return _svrCompiler.UpsertSystemTag(curDlid, tag, val, out errors);
        }

        /// <summary>
        /// Translation types for conversion to and from binary
        /// </summary>
        /// <returns></returns>
        public IList<DataListFormat> TranslationTypes()
        {
            return (_svrCompiler.FetchTranslatorTypes());
        }

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ConvertTo(DataListFormat typeOf, StringBuilder payload, StringBuilder shape, out ErrorResultTO errors)
        {
            byte[] data = Encoding.UTF8.GetBytes(payload.ToString());
            return _svrCompiler.ConvertTo(null, typeOf, data, shape, out errors);
        }

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ConvertTo(DataListFormat typeOf, byte[] payload, StringBuilder shape, out ErrorResultTO errors)
        {
            return _svrCompiler.ConvertTo(null, typeOf, payload, shape, out errors);
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ConvertTo(DataListFormat typeOf, object payload, StringBuilder shape, out ErrorResultTO errors)
        {
            return _svrCompiler.ConvertTo(null, typeOf, payload, shape, out errors);
        }

        /// <summary>
        /// Converts the and only map inputs.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ConvertAndOnlyMapInputs(DataListFormat typeOf, StringBuilder payload, StringBuilder shape, out ErrorResultTO errors)
        {
            byte[] data = Encoding.UTF8.GetBytes(payload.ToString());
            return _svrCompiler.ConvertAndOnlyMapInputs(null, typeOf, data, shape, out errors);
        }

        /// <summary>
        /// Populates the data list.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="input">The input.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="targetDlid">The target dlid.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PopulateDataList(DataListFormat typeOf, object input, string outputDefs, Guid targetDlid, out ErrorResultTO errors)
        {
            return _svrCompiler.PopulateDataList(null, typeOf, input, outputDefs, targetDlid, out errors);
        }

        /// <summary>
        /// Pushes the binary data list.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PushBinaryDataList(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors)
        {
            return PushBinaryDataListInServerScope(dlID, bdl, out errors);

        }

        /// <summary>
        /// Pushes the binary data list in server scope.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PushBinaryDataListInServerScope(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            string error;

            if(_svrCompiler.TryPushDataList(bdl, out error))
            {
                errors.AddError(error);
                return bdl.UID;
            }

            errors.AddError(error);

            return GlobalConstants.NullDataListID;
        }

        /// <summary>
        /// Converts to selected Type from binary
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public StringBuilder ConvertFrom(Guid curDlid, DataListFormat typeOf, enTranslationDepth depth, out ErrorResultTO errors)
        {

            DataListTranslatedPayloadTO tmp = _svrCompiler.ConvertFrom(null, curDlid, depth, typeOf, out errors);

            if(tmp != null)
            {
                return tmp.FetchAsString();
            }

            return new StringBuilder();
        }

        /// <summary>
        /// Converts the and filter.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="filterShape">The filter shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public StringBuilder ConvertAndFilter(Guid curDlid, DataListFormat typeOf, StringBuilder filterShape, out ErrorResultTO errors)
        {
            return _svrCompiler.ConvertAndFilter(null, curDlid, filterShape, typeOf, out errors);
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions = PopulateOptions.IgnoreBlankRows)
        {
            return _svrCompiler.ConvertToDataTable(input, recsetName, out errors, populateOptions);
        }

        /// <summary>
        /// Converts from to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public T ConvertFromJsonToModel<T>(StringBuilder payload)
        {

            T obj = JsonConvert.DeserializeObject<T>(payload.ToString());

            return obj;
        }

        /// <summary>
        /// Converts the model to json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public StringBuilder ConvertModelToJson<T>(T payload)
        {
            var result = new StringBuilder(JsonConvert.SerializeObject(payload));

            return result;
        }

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PushSystemModelToDataList<T>(T model, out ErrorResultTO errors)
        {
            return PushSystemModelToDataList(GlobalConstants.NullDataListID, model, out errors);
        }

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PushSystemModelToDataList<T>(Guid dlID, T model, out ErrorResultTO errors)
        {
            // Serialize the model first ;)
            var jsonModel = ConvertModelToJson(model);
            ErrorResultTO allErrors = new ErrorResultTO();

            // Create a new DL if need be
            Guid pushID = dlID;
            if(pushID == GlobalConstants.NullDataListID)
            {
                IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();
                pushID = PushBinaryDataList(bdl.UID, bdl, out errors);
                allErrors.MergeErrors(errors);
                errors.ClearErrors();
            }

            UpsertSystemTag(pushID, enSystemTag.SystemModel, jsonModel.ToString(), out errors);
            allErrors.MergeErrors(errors);

            errors = allErrors;

            return pushID;
        }

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public T FetchSystemModelFromDataList<T>(Guid dlID, out ErrorResultTO errors)
        {
            string modelData = EvaluateSystemEntry(dlID, enSystemTag.SystemModel, out errors);

            T obj = Activator.CreateInstance<T>();

            if(!string.IsNullOrEmpty(modelData))
            {

                if(!String.IsNullOrEmpty(modelData))
                {
                    obj = ConvertFromJsonToModel<T>(new StringBuilder(modelData));
                }
            }
            else
            {

                errors.AddError("Failed to locate model!");
            }

            return obj;
        }

        /// <summary>
        /// Fetches the system model as web model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string FetchSystemModelAsWebModel<T>(Guid dlID, out ErrorResultTO errors)
        {
            T model = FetchSystemModelFromDataList<T>(dlID, out errors);
            string result = "{}"; // empty data set for injection ;)

            if(!errors.HasErrors())
            {
                var dev2DataModel = model as IDev2DataModel;

                if(dev2DataModel != null) result = dev2DataModel.ToWebModel();

            }

            return result;
        }

        /// <summary>
        /// Evaluates the system entry.
        /// </summary>
        /// <param name="curDlid">The cur DL ID.</param>
        /// <param name="sysTag">The system tag.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string EvaluateSystemEntry(Guid curDlid, enSystemTag sysTag, out ErrorResultTO errors)
        {
            IBinaryDataListEntry binaryDataListEntry = _svrCompiler.Evaluate(null, curDlid, enActionType.System, sysTag.ToString(), out errors) ?? DataListConstants.baseEntry;
            return binaryDataListEntry.FetchScalar().TheValue;
        }

        public IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(Guid id, StateType direction)
        {
            return _svrCompiler.FetchChanges(null, id, direction);
        }

        public bool DeleteDataListByID(Guid curDlid)
        {

            return _svrCompiler.DeleteDataListByID(curDlid, false);
        }

        public bool ForceDeleteDataListByID(Guid curDlid)
        {
            // Do nothing for now, we scope it all ;)
            return _svrCompiler.DeleteDataListByID(curDlid, true);
        }

        public int GetMaxNumberOfExecutions(Guid curDlid, IList<string> expressions)
        {

            int result = 1;
            ErrorResultTO errors;
            IBinaryDataList bdl = FetchBinaryDataList(curDlid, out errors);
            // Loop each expression to find the total number of executions ;)
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var exp in expressions)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                IList<IIntellisenseResult> parts = Parser.ParseExpressionIntoParts(exp, bdl.FetchIntellisenseParts());
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach(IIntellisenseResult p in parts)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    result = Math.Max(result, FetchNumberOfExecutions(p, bdl));
                }
            }

            return result;
        }

        public Guid FetchParentID(Guid curDlid)
        {

            ErrorResultTO errors;
            return (_svrCompiler.FetchBinaryDataList(null, curDlid, out errors).ParentUID);
        }

        public bool HasErrors(Guid curDlid)
        {
            ErrorResultTO errors;
            var binaryDatalist = _svrCompiler.FetchBinaryDataList(null, curDlid, out errors);

            if(binaryDatalist != null)
            {
                return (binaryDatalist.HasErrors());
            }

            errors.AddError("No binary datalist found");
            return true;
        }

        public string FetchErrors(Guid curDlid, bool returnAsXml = false)
        {
            ErrorResultTO errors;
            var binaryDatalist = _svrCompiler.FetchBinaryDataList(null, curDlid, out errors);
            if(binaryDatalist != null)
            {
                return (binaryDatalist.FetchErrors(returnAsXml));
            }

            var sb = new StringBuilder();
            var count = 1;
            var errorList = errors.FetchErrors();
            foreach(var error in errorList)
            {
                sb.AppendFormat("{0} {1}", count, error);
                count++;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Clears the errors.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/02/06</date>
        public void ClearErrors(Guid curDlid)
        {
            ErrorResultTO errors;
            var list = _svrCompiler.FetchBinaryDataList(null, curDlid, out errors);
            if(list != null)
                list.ClearErrors();
        }

        public bool SetParentID(Guid curDlid, Guid newParent)
        {
            bool result = true;
            ErrorResultTO errors;

            _svrCompiler.SetParentUID(curDlid, newParent, out errors);
            if(errors.HasErrors())
            {
                result = false;
            }

            return result;
        }


        /// <summary>
        /// Gets the wizard data list for a service.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <returns>
        /// The string for the data list
        /// </returns>
        /// <exception cref="System.Xml.XmlException">Inputs/Outputs tags were not found in the service definition</exception>
        public string GetWizardDataListForService(string serviceDefinition)
        {
            ErrorResultTO errors;

            string inputs;
            string outputs;
            try
            {
                inputs = DataListUtil.ExtractInputDefinitionsFromServiceDefinition(serviceDefinition);
                outputs = DataListUtil.ExtractOutputDefinitionsFromServiceDefinition(serviceDefinition);
            }
            catch
            {
                throw new XmlException("Inputs/Outputs tags were not found in the service definition");
            }

            var inputDl = GenerateWizardDataListFromDefs(inputs, enDev2ArgumentType.Input, false, out errors);

            var outputDl = GenerateWizardDataListFromDefs(outputs, enDev2ArgumentType.Output, false, out errors);

            Guid inputDlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, inputDl, out errors);
            Guid outputDlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, outputDl, out errors);
            Guid mergedDlID = Merge(inputDlID, outputDlID, enDataListMergeTypes.Union, enTranslationDepth.Shape, true, out errors);
            var result = ConvertFrom(mergedDlID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errors);
            return result.ToString();
        }

        /// <summary>
        /// Gets the wizard data list for a workflow.
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns>
        /// The string for the data list
        /// </returns>
        /// <exception cref="System.Exception">
        /// </exception>
        public string GetWizardDataListForWorkflow(string dataList)
        {
            IBinaryDataList newDl = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors;
            var dataListStringBuilder = new StringBuilder(dataList);
            Guid dlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), dataListStringBuilder, dataListStringBuilder, out errors);
            if(!errors.HasErrors())
            {
                IBinaryDataList dl = FetchBinaryDataList(dlID, out errors);
                if(!errors.HasErrors())
                {
                    IList<IBinaryDataListEntry> entries = dl.FetchAllEntries();
                    foreach(IBinaryDataListEntry entry in entries)
                    {
                        if(entry.IsRecordset)
                        {
                            if(entry.ColumnIODirection != enDev2ColumnArgumentDirection.None)
                            {
                                string tmpError;
                                newDl.TryCreateRecordsetTemplate(entry.Namespace, entry.Description, entry.Columns, true, out tmpError);

                            }
                        }
                        else
                        {
                            if(entry.ColumnIODirection != enDev2ColumnArgumentDirection.None)
                            {
                                string tmpError;
                                IBinaryDataListItem scalar = entry.FetchScalar();
                                newDl.TryCreateScalarTemplate(string.Empty, scalar.FieldName, entry.Description, true, out tmpError);
                            }
                        }
                    }
                    Guid newDlId = PushBinaryDataList(newDl.UID, newDl, out errors);
                    dataList = ConvertFrom(newDlId, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Shape, out errors).ToString();
                }
                else
                {
                    throw new Exception(errors.MakeUserReady());
                }
            }
            else
            {
                throw new Exception(errors.MakeUserReady());
            }
            return dataList;
        }

        public string GenerateSerializableDefsFromDataList(string datalist, enDev2ColumnArgumentDirection direction)
        {
            DefinitionBuilder db = new DefinitionBuilder();

            if(direction == enDev2ColumnArgumentDirection.Input)
            {
                db.ArgumentType = enDev2ArgumentType.Input;
            }
            else if(direction == enDev2ColumnArgumentDirection.Output)
            {
                db.ArgumentType = enDev2ArgumentType.Output;
            }

            db.Definitions = GenerateDefsFromDataList(datalist, direction);

            return db.Generate();
        }

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList)
        {
            return GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.Both);
        }

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if (!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for (int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    var ioDirection = GetDev2ColumnArgumentDirection(tmpNode);

                    if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                    {
                        if (tmpNode.HasChildNodes)
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            // now extract child node defs
                            XmlNodeList childNl = tmpNode.ChildNodes;
                            for (int q = 0; q < childNl.Count; q++)
                            {
                                var xmlNode = childNl[q];
                                var fieldIODirection = GetDev2ColumnArgumentDirection(xmlNode);
                                if (CheckIODirection(dev2ColumnArgumentDirection, fieldIODirection))
                                {
                                    result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                                false, "", false));
                                }
                            }
                        }
                        else
                        {
                            // scalar value, make it as such
                            result.Add(DataListFactory.CreateDefinition(tmpNode.Name, "", "", false, "", false, ""));
                        }
                    }
                }
            }

            return result;
        }


        public IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if(!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for(int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    var ioDirection = GetDev2ColumnArgumentDirection(tmpNode);

                        if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection) && tmpNode.HasChildNodes)
                        {
                            result.Add(DataListFactory.CreateDefinition("", "", "", tmpNode.Name, false, "",
                                                                                false, "", false));
                        }
                        else if (tmpNode.HasChildNodes)
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            // now extract child node defs
                            XmlNodeList childNl = tmpNode.ChildNodes;
                            for(int q = 0; q < childNl.Count; q++)
                            {
                                var xmlNode = childNl[q];
                                var fieldIODirection = GetDev2ColumnArgumentDirection(xmlNode);
                                if(CheckIODirection(dev2ColumnArgumentDirection, fieldIODirection))
                                {
                                    result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                                false, "", false));
                                }
                            }
                        }
                        else if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                        {
                            // scalar value, make it as such
                            result.Add(DataListFactory.CreateDefinition(tmpNode.Name, "", "", false, "", false, ""));
                        }

                }
            }

            return result;
        }

        static bool CheckIODirection(enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, enDev2ColumnArgumentDirection ioDirection)
        {
            return ioDirection == dev2ColumnArgumentDirection ||
                   (ioDirection == enDev2ColumnArgumentDirection.Both &&
                    (dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Input || dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Output));
        }

        static enDev2ColumnArgumentDirection GetDev2ColumnArgumentDirection(XmlNode tmpNode)
        {
            XmlAttribute ioDirectionAttribute = tmpNode.Attributes[GlobalConstants.DataListIoColDirection];

            enDev2ColumnArgumentDirection ioDirection;
            if(ioDirectionAttribute != null)
            {
                ioDirection = (enDev2ColumnArgumentDirection)Dev2EnumConverter.GetEnumFromStringDiscription(ioDirectionAttribute.Value, typeof(enDev2ColumnArgumentDirection));
            }
            else
            {
                ioDirection = enDev2ColumnArgumentDirection.Both;
            }
            return ioDirection;
        }

        //PBI 8435 - Massimo.Guerrera - Added for getting the debug data for the multiAssign

        public List<KeyValuePair<string, IBinaryDataListEntry>> GetDebugData()
        {
            return _svrCompiler.GetDebugItems();
        }

        #region New Private Methods

        private int FetchNumberOfExecutions(IIntellisenseResult part, IBinaryDataList bdl)
        {
            int result = 1;

            if(!part.Option.IsScalar)
            {
                // process the recordset...
                enRecordsetIndexType type = DataListUtil.GetRecordsetIndexType(part.Option.DisplayValue);
                if(type == enRecordsetIndexType.Star)
                {
                    // Fetch entry and find the last index
                    IBinaryDataListEntry entry;
                    string error;
                    if(bdl.TryGetEntry(part.Option.Recordset, out entry, out error))
                    {
                        result = entry.FetchLastRecordsetIndex();
                    }
                }
                else if(type == enRecordsetIndexType.Numeric)
                {
                    // Fetch index out
                    Int32.TryParse(part.Option.RecordsetIndex, out result);
                }
            }

            return result;
        }

        /// <summary>
        /// Generate DL shape from IO defs
        /// </summary>
        /// <param name="defs">The defs.</param>
        /// <param name="withData">if set to <c>true</c> [with data].</param>
        /// <returns></returns>
        private StringBuilder GenerateDataListFromDefs(IList<IDev2Definition> defs, bool withData = false)
        {
            return DataListUtil.GenerateDataListFromDefs(defs, withData);
        }

        /// <summary>
        /// Create a DL shape as per IO mapping
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="flipGeneration">if set to <c>true</c> [flip generation].</param>
        /// <returns></returns>
        private StringBuilder ShapeDefinitionsToDataList(string arguments, enDev2ArgumentType typeOf, out ErrorResultTO errors, bool flipGeneration = false)
        {
            return DataListUtil.ShapeDefinitionsToDataList(arguments, typeOf, out errors, flipGeneration);
        }

        #endregion

        #endregion

        #region Tear Down

        public void Dispose()
        {
            lock(_disposeGuard)
            {
                if(_isDisposed)
                {
                    return;
                }

                _uniqueWorkflowParts.Clear();
                _uniqueWorkflowParts = null;

                _svrCompiler = null;

                _isDisposed = true;
            }
        }

        #endregion Tear Down
    }
}
