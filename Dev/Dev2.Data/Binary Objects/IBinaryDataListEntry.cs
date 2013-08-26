using Dev2.Data.Audit;
using Dev2.Data.Binary_Objects;
using System;
using System.Collections.Generic;

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
        /// Return the number of rows in a recordset
        /// </summary>
        /// <returns></returns>
        int ItemCollectionSize();

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
        /// <returns></returns>
        IList<IBinaryDataListItem> FetchRecordAt(int idx, string field, out string error);

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
        /// Clones the specified to clone.
        /// </summary>
        /// <param name="toClone">To clone.</param>
        //IBinaryDataListEntry Clone(enTranslationDepth depth, out string errors);

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
        void Merge(IBinaryDataListEntry mergeWith, out string error);

        /// <summary>
        /// Fetches the index of the recordset.
        /// </summary>
        /// <returns></returns>
        IIndexIterator FetchRecordsetIndexes();

        /// <summary>
        /// Fetches the last index of the recordset.
        /// </summary>
        /// <returns></returns>
        int FetchLastRecordsetIndex();

        /// <summary>
        /// Sets the last index of the recordset.
        /// </summary>
        /// <param name="idx">The idx.</param>
        //void SetLastRecordsetIndex(int idx);

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
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataListItem TryFetchLastIndexedRecordsetUpsertPayload(out string error);

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
        /// <returns></returns>
        bool TryDeleteRows(string index);

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
        /// Disposes the cache.
        /// </summary>
        /// <returns></returns>
        int DisposeCache();

        #endregion


    }
}
