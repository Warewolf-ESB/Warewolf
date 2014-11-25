/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Common.Interfaces.DataList.Contract
{
    public interface IBinaryDataListItem
    {
        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        string TheValue { get; set; }

        /// <summary>
        ///     Gets the index of the item collection.
        /// </summary>
        /// <value>
        ///     The index of the item collection.
        /// </value>
        int ItemCollectionIndex { get; }

        /// <summary>
        ///     Gets the namespace.
        /// </summary>
        /// <value>
        ///     The namespace.
        /// </value>
        string Namespace { get; }

        /// <summary>
        ///     Gets the name of the field.
        /// </summary>
        /// <value>
        ///     The name of the field.
        /// </value>
        string FieldName { get; }

        /// <summary>
        ///     Gets the display value.
        /// </summary>
        /// <value>
        ///     The display value.
        /// </value>
        string DisplayValue { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is deferred read.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is deferred read; otherwise, <c>false</c>.
        /// </value>
        bool IsDeferredRead { get; set; }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns></returns>
        IBinaryDataListItem Clone();

        /// <summary>
        ///     Ecodes the region brackets in Html.
        /// </summary>
        void HtmlEncodeRegionBrackets();

        /// <summary>
        ///     Updates the value.
        /// </summary>
        /// <param name="val">The val.</param>
        void UpdateValue(string val);

        /// <summary>
        ///     Updates the field.
        /// </summary>
        /// <param name="val">The val.</param>
        void UpdateField(string val);

        /// <summary>
        ///     Updates the recordset.
        /// </summary>
        /// <param name="val">The val.</param>
        void UpdateRecordset(string val);

        /// <summary>
        ///     Updates the index.
        /// </summary>
        /// <param name="idx">The idx.</param>
        void UpdateIndex(int idx);

        /// <summary>
        ///     Fetches the deferred location.
        /// </summary>
        /// <returns></returns>
        string FetchDeferredLocation();

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>
        ///     To the clear.
        /// </summary>
        void ToClear();
    }
}