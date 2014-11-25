
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
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Audit;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Storage.ProtocolBuffers;

namespace Dev2.DataList.Contract.Binary_Objects
{
    public interface IBinaryDataListEntry : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is recordset.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is recordset; otherwise, <c>false</c>.
        /// </value>
        bool IsRecordset { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        bool IsEditable { get; }

        /// <summary>
        /// Gets the column IO direction.
        /// </summary>
        /// <value>
        /// The column IO direction.
        /// </value>
        enDev2ColumnArgumentDirection ColumnIODirection { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is evaluation scalar entry
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is evaluation scalar; otherwise, <c>false</c>.
        /// </value>
        bool IsEvaluationScalar { get; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        IList<Dev2Column> Columns { get; }

        /// <summary>
        /// The recordset name
        /// </summary>
        /// <value>
        /// The namespace.
        /// </value>
        string Namespace { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is managment service payload.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is managment service payload; otherwise, <c>false</c>.
        /// </value>
        bool IsManagmentServicePayload { get; }

        /// <summary>
        /// Gets or sets the complex expression auditor.
        /// </summary>
        /// <value>
        /// The complex expression auditor.
        /// </value>
        ComplexExpressionAuditor ComplexExpressionAuditor { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adjusts the index view.
        /// </summary>
        /// <param name="gaps">The gaps.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="onMasterEntry">if set to <c>true</c> [configuration master entry].</param>
        void AdjustIndexView(HashSet<int> gaps, int min, int max, bool onMasterEntry = false);

        /// <summary>
        /// Adjusts for io mapping.
        /// </summary>
        /// <param name="parentDlid">The parent dlid.</param>
        /// <param name="parentColumn">The parent column.</param>
        /// <param name="parentNamespace">The parent namespace.</param>
        /// <param name="childColumn">The child column.</param>
        /// <param name="errors">The errors.</param>
        void AdjustForIOMapping(Guid parentDlid, string parentColumn, string parentNamespace, string childColumn, out ErrorResultTO errors);

        /// <summary>
        /// Adjusts the alias operation for external service populate.
        /// </summary>
        void AdjustAliasOperationForExternalServicePopulate();

        /// <summary>
        /// Return the number of rows in a recordset
        /// </summary>
        /// <returns></returns>
        int ItemCollectionSize();


        /// <summary>
        /// Fetches the row attribute.
        /// </summary>
        /// <param name="idx">The index.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IList<IBinaryDataListItem> FetchRowAt(int idx, out string error);


        /// <summary>
        /// Fetch a record at an index
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IList<IBinaryDataListItem> FetchRecordAt(int idx, out string error);

        /// <summary>
        /// Fetches the record at.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="field">The field.</param>
        /// <param name="error">The error.</param>
        /// <param name="isEntireRow"></param>
        /// <returns></returns>
        IList<IBinaryDataListItem> FetchRecordAt(int idx, string field, out string error, bool isEntireRow = false);

        /// <summary>
        /// Put a column value at an recordset index
        /// </summary>
        /// <param name="item"></param>
        /// <param name="idx"></param>
        /// <param name="error"></param>
        void TryPutRecordItemAtIndex(IBinaryDataListItem item, int idx, out string error);

        /// <summary>
        /// Tries the put record row at.
        /// </summary>
        /// <param name="itms">The itms.</param>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        void TryPutRecordRowAt(IList<IBinaryDataListItem> itms, int idx, out string error);

        /// <summary>
        /// Append a column value at an recordset index
        /// </summary>
        /// <param name="item"></param>
        /// <param name="error"></param>
        void TryAppendRecordItem(IBinaryDataListItem item, out string error);

        /// <summary>
        /// Fetch a scalar value
        /// </summary>
        /// <returns></returns>
        IBinaryDataListItem FetchScalar();

        /// <summary>
        /// Upsert a scalar value
        /// </summary>
        /// <param name="item"></param>
        /// <param name="error"></param>
        void TryPutScalar(IBinaryDataListItem item, out string error);

        /// <summary>
        /// Clones the specified depth.
        /// </summary>
        /// <param name="depth">The depth.</param>
        /// <param name="clonedStorageId">The cloned storage ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        IBinaryDataListEntry Clone(enTranslationDepth depth, Guid clonedStorageId, out string errors);

        /// <summary>
        /// Merges this instance.
        /// </summary>
        /// <param name="mergeWith">The merge with.</param>
        /// <param name="error">The error.</param>
        void Merge(IBinaryDataListEntry mergeWith, out string error);

        /// <summary>
        /// Fetches the index of the recordset.
        /// </summary>
        /// <returns></returns>
        IIndexIterator FetchRecordsetIndexes();

        /// <summary>
        /// Fetches the last index of the recordset.
        /// </summary>
        /// <param name="localOnly">if set to <c>true</c> [local only].</param>
        /// <returns></returns>
        int FetchLastRecordsetIndex(bool localOnly = false);

        /// <summary>
        /// Fetches the index of the append recordset.
        /// </summary>
        /// <returns></returns>
        int FetchAppendRecordsetIndex();

        /// <summary>
        /// Makes the entry evaluate ready.
        /// </summary>
        /// <param name="keepIdx">The keep idx.</param>
        /// <param name="keepCol">The keep col.</param>
        /// <param name="error">The error.</param>
        void MakeRecordsetEvaluateReady(int keepIdx, string keepCol, out string error);

        /// <summary>
        /// Tries the index of the fetch recordset column at.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataListItem TryFetchRecordsetColumnAtIndex(string field, int idx, out string error);

        /// <summary>
        /// Tries the fetch indexed recordset upsert payload.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataListItem TryFetchLastIndexedRecordsetUpsertPayload(out string error, string field ="");

        /// <summary>
        /// Tries the fetch indexed recordset upsert payload.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataListItem TryFetchIndexedRecordsetUpsertPayload(int idx, out string error);

        /// <summary>
        /// Blanks the record set data.
        /// </summary>
        /// <param name="col">The col.</param>
        void BlankRecordSetData(string col);

        /// <summary>
        /// Determines whether the specified cols has columns.
        /// </summary>
        /// <param name="cols">The cols.</param>
        /// <returns>
        ///   <c>true</c> if the specified cols has columns; otherwise, <c>false</c>.
        /// </returns>
        bool HasColumns(IList<Dev2Column> cols);

        /// <summary>
        /// Determines whether the specified field has field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>
        ///   <c>true</c> if the specified field has field; otherwise, <c>false</c>.
        /// </returns>
        bool HasField(string field);

        /// <summary>
        /// Sorts the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="desc">if set to <c>true</c> [desc].</param>
        /// <param name="error">The error.</param>
        void Sort(string field, bool desc, out string error);

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </returns>
        bool IsEmpty();

        /// <summary>
        /// Deletes the rows.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryDeleteRows(string index, out string error);

        /// <summary>
        /// Internals the index of the fetch column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        int InternalFetchColumnIndex(string column);

        /// <summary>
        /// Gets the distinct rows.
        /// </summary>
        /// <param name="filterCols">The filter cols.</param>
        /// <returns></returns>
        List<int> GetDistinctRows(List<string> filterCols);

        /// <summary>
        /// Fetches the alias.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, BinaryDataListAlias> FetchAlias();

        /// <summary>
        /// Disposes the cache.
        /// </summary>
        /// <returns></returns>
        int DisposeCache();

        #endregion


    }
}
