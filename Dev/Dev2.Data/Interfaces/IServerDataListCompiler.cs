using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.TO;

namespace Dev2.Server.Datalist
{

    public interface IEnvironmentModelDataListCompiler
    {

        #region Evaluation Operations

        /// <summary>
        /// Used to evaluate an expression against a given datalist
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="typeOf">The type of evaluation.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="returnExpressionIfNoMatch">if set to <c>true</c> [return expression if no match].</param>
        /// <returns></returns>
        IBinaryDataListEntry Evaluate(NetworkContext ctx, Guid curDLID, enActionType typeOf, string expression, out ErrorResultTO errors, bool returnExpressionIfNoMatch = false);

        /// <summary>
        /// Builds the input expression extractor.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <returns></returns>
        Func<IDev2Definition, string> BuildInputExpressionExtractor(enDev2ArgumentType typeOf);

        /// <summary>
        /// Builds the output expression extractor.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <returns></returns>
        Func<IDev2Definition, string> BuildOutputExpressionExtractor(enDev2ArgumentType typeOf);

        #endregion

        #region Internal Binary Operations

        /// <summary>
        /// Fetches the binary data list.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        IBinaryDataList FetchBinaryDataList(NetworkContext ctx, Guid curDLID, out ErrorResultTO errors);

        /// <summary>
        /// Clones the data list.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid CloneDataList(Guid curDLID, out ErrorResultTO errors);

        #endregion

        #region Manipulation Operations
        /// <summary>
        /// Upserts the value to the specified cur DL ID's expression.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(NetworkContext ctx, Guid curDLID, string expression, IBinaryDataListEntry value, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="values">The values.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(NetworkContext ctx, Guid curDLID, IList<string> expression, IList<IBinaryDataListEntry> values, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the values against the specified cur DL ID's expression list.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="values">The values.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(NetworkContext ctx, Guid curDLID, IList<string> expressions, IList<string> values, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<string> payload, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the specified cur DLID.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload, out ErrorResultTO errors);


        /// <summary>
        /// Upserts the specified CTX.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The current dlid.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Upsert(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<List<string>> payload, out ErrorResultTO errors);

        /// <summary>
        /// Shapes the definitions in string form to create/amended a DL.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DL ID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="defs">The defs.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="overrideID">The override unique identifier.</param>
        /// <returns></returns>
        Guid Shape(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, string defs, out ErrorResultTO errors, Guid overrideID = default(Guid));

        /// <summary>
        /// Shapes the specified CTX.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The current dlid.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="definitions">The definitions.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Shape(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, IList<IDev2Definition> definitions, out ErrorResultTO errors);

        /// <summary>
        /// Shapes for sub execution.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="parentDLID">The parent dlid.</param>
        /// <param name="childDLID">The child dlid.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubExecution(NetworkContext ctx, Guid parentDLID, Guid childDLID, string inputDefs, string outputDefs, out ErrorResultTO errors);

        /// <summary>
        /// Merges the specified left ID with the right ID
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="leftID">The left ID.</param>
        /// <param name="rightID">The right ID.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="createNewList">if set to <c>true</c> [create new list].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Merge(NetworkContext ctx, Guid leftID, Guid rightID, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors);

        /// <summary>
        /// Conditionals the merge.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="destinationDatalistID">The destination datalist unique identifier.</param>
        /// <param name="sourceDatalistID">The source datalist unique identifier.</param>
        /// <param name="datalistMergeFrequency">The datalist merge frequency.</param>
        /// <param name="datalistMergeType">Type of the datalist merge.</param>
        /// <param name="datalistMergeDepth">The datalist merge depth.</param>
        Guid ConditionalMerge(NetworkContext ctx, DataListMergeFrequency conditions, Guid destinationDatalistID, Guid sourceDatalistID, DataListMergeFrequency datalistMergeFrequency, enDataListMergeTypes datalistMergeType, enTranslationDepth datalistMergeDepth);

        /// <summary>
        /// Transfer system tags from parent into child if parentToChild = true, else child to Parent if false
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="parentDLID">The parent DLID.</param>
        /// <param name="childDLID">The child DLID.</param>
        /// <param name="parentToChild">if set to <c>true</c> [parent to child].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid TransferSystemTags(NetworkContext ctx, Guid parentDLID, Guid childDLID, bool parentToChild, out ErrorResultTO errors);

        #endregion

        #region External Translation

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
        Guid PopulateDataList(NetworkContext ctx, DataListFormat typeOf, object input, string outputDefs, Guid targetDLID, out ErrorResultTO errors);

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertTo(NetworkContext ctx, DataListFormat typeOf, byte[] payload, string shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertTo(NetworkContext ctx, DataListFormat typeOf, object payload, string shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts to selected Type from binary
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        DataListTranslatedPayloadTO ConvertFrom(NetworkContext ctx, Guid curDLID, enTranslationDepth depth, DataListFormat typeOf, out ErrorResultTO errors);

        /// <summary>
        /// Converts the and filter.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="filterShape">The filter shape.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string ConvertAndFilter(NetworkContext ctx, Guid curDLID, string filterShape, DataListFormat typeOf, out ErrorResultTO errors);

        /// <summary>
        /// Converts the and only map inputs.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="tyoeOf">The tyoe of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertAndOnlyMapInputs(NetworkContext ctx, DataListFormat tyoeOf, byte[] payload, string shape, out ErrorResultTO errors);

        /// <summary>
        /// Fetches the translator types.
        /// </summary>
        /// <returns></returns>
        IList<DataListFormat> FetchTranslatorTypes();

        /// <summary>
        /// Tries the push data list.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool TryPushDataList(IBinaryDataList payload, out string error);


        #endregion

        #region Admin Operations

        /// <summary>
        /// Fetches the DebugItems created during a upsert
        /// </summary>
        List<KeyValuePair<string, IBinaryDataListEntry>> GetDebugItems();

        /// <summary>
        /// Fetches the change log for pre ( inputs ) or post execute ( outputs )
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="id">The id.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(NetworkContext ctx, Guid id, StateType direction);

        /// <summary>
        /// Deletes the data list by ID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="onlyIfNotPersisted">if set to <c>true</c> [only if not persisted].</param>
        /// <returns></returns>
        bool DeleteDataListByID(Guid curDLID, bool onlyIfNotPersisted);

        /// <summary>
        /// Sets the parent UID.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="parentID">The parent ID.</param>
        /// <param name="errors">The errors.</param>
        void SetParentUID(Guid curDLID, Guid parentID, out ErrorResultTO errors);


        /// <summary>
        /// Upserts the system tag.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="val">The val.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, string val, out ErrorResultTO errors);

        /// <summary>
        /// Upserts the system tag.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="val">The val.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, IBinaryDataListEntry val, out ErrorResultTO errors);

        #endregion

        DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions);
    }
}
