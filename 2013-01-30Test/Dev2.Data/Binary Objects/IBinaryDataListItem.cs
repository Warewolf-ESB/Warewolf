using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract.Binary_Objects
{
    public interface IBinaryDataListItem
    {

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        string TheValue { get; }

        /// <summary>
        /// Gets the index of the item collection.
        /// </summary>
        /// <value>
        /// The index of the item collection.
        /// </value>
        int ItemCollectionIndex { get; }

        /// <summary>
        /// Gets the namespace.
        /// </summary>
        /// <value>
        /// The namespace.
        /// </value>
        string Namespace { get; }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        string FieldName { get; }

        /// <summary>
        /// Gets the display value.
        /// </summary>
        /// <value>
        /// The display value.
        /// </value>
        string DisplayValue { get; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        IBinaryDataListItem Clone();

        /// <summary>
        /// Ecodes the region brackets in Html.
        /// </summary>      
        void HtmlEncodeRegionBrackets();

    }
}
