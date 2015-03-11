
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Enums;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.DataList.Contract.Binary_Objects
{
    public interface IBinaryDataList : IDisposable
    {

        #region Properties
        /// <summary>
        /// Unique ID for datalist
        /// </summary>
        Guid UID { get; }

        /// <summary>
        /// Gets the parent UID.
        /// </summary>
        /// <value>
        /// The parent UID.
        /// </value>
        Guid ParentUID { get; set; }


        #endregion

        #region Methods
        /// <summary>
        /// Create a scalar template
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="description">The description.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool TryCreateScalarTemplate(string theNameSpace, string fieldName, string description, bool overwrite, out string error);

        /// <summary>
        /// Create a scalar template
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="description">The description.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        /// <param name="ioDir">The io dir.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool TryCreateScalarTemplate(string theNameSpace, string fieldName, string description, bool overwrite, bool isEditable, enDev2ColumnArgumentDirection ioDir, out string error);

        /// <summary>
        /// Create a recordset template
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="description">The description.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool TryCreateRecordsetTemplate(string theNameSpace, string description, IList<Dev2Column> columns, bool overwrite, out string error);

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
        bool TryCreateRecordsetTemplate(string theNameSpace, string description, IList<Dev2Column> columns, bool overwrite, bool isEditable, out string error);


        /// <summary>
        /// Tries the create recordset template.
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="description">The description.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        /// <param name="ioDir">The io dir.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool TryCreateRecordsetTemplate(string theNameSpace, string description, IList<Dev2Column> columns, bool overwrite, bool isEditable, enDev2ColumnArgumentDirection ioDir, out string error);

        /// <summary>
        /// Try and create a scalar value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool TryCreateScalarValue(string value, string fieldName, out string error);

        /// <summary>
        /// Try and create a recordset value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool TryCreateRecordsetValue(string value, string fieldName, string theNameSpace, int idx, out string error);

        /// <summary>
        /// Try and fetch an entry
        /// </summary>
        /// <param name="theNameSpace">The name space.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        bool TryGetEntry(string theNameSpace, out IBinaryDataListEntry entry, out string error);

        /// <summary>
        /// Return all recordset templates
        /// </summary>
        /// <returns></returns>
        IList<IBinaryDataListEntry> FetchRecordsetEntries();

        /// <summary>
        /// Return all scalar templates
        /// </summary>
        /// <returns></returns>
        IList<IBinaryDataListEntry> FetchScalarEntries();

        /// <summary>
        /// Return all templates
        /// </summary>
        /// <returns></returns>
        IList<IBinaryDataListEntry> FetchAllEntries();

        /// <summary>
        /// Return all the data list keys
        /// </summary>
        /// <returns></returns>
        IList<string> FetchAllKeys();


        /// <summary>
        /// Fetches all user keys.
        /// </summary>
        /// <returns></returns>
        IList<string> FetchAllUserKeys();

        /// <summary>
        /// Fetches only the recordset user keys
        /// </summary>
        /// <returns></returns>
        IList<string> FetchAllRecordSetKeys();

        /// <summary>
        /// Merges the specified left.
        /// </summary>
        /// <param name="right">The right.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="newList">if set to <c>true</c> [new list].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        IBinaryDataList Merge(IBinaryDataList right, enDataListMergeTypes mergeType, enTranslationDepth depth, bool newList, out ErrorResultTO errors);

        /// <summary>
        /// Clones the specified type of.
        /// </summary>
        /// <param name="depth">The depth.</param>
        /// <param name="errorResult">The error result.</param>
        /// <param name="onlySystemTags">if set to <c>true</c> [only system tags].</param>
        /// <returns></returns>
        IBinaryDataList Clone(enTranslationDepth depth, out ErrorResultTO errorResult, bool onlySystemTags);

        /// <summary>
        /// Fetches the intellisense parts.
        /// </summary>
        /// <returns></returns>
        IList<IDev2DataLanguageIntellisensePart> FetchIntellisenseParts();

        /// <summary>
        /// Determines whether this instance has errors.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </returns>
        bool HasErrors();

        /// <summary>
        /// Fetches the errors.
        /// </summary>
        /// <returns></returns>
        string FetchErrors(bool returnAsXml = false);

        /// <summary>
        /// Clears the errors.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/02/06</date>
        void ClearErrors();

        #endregion
    }
}
