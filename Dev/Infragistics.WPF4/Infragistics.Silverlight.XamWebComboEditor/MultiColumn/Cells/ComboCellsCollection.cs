using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Collections;
using System.Collections.Generic;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A Collection of <see cref="ComboCell"/> objects.
    /// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class ComboCellsCollection : CollectionBase<ComboCellBase>
    {
        #region Members

        ComboRowBase _row;
        ComboColumnCollection _columns;
        Dictionary<ComboColumn, ComboCellBase> _auxColumns;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboCellsCollection"/> class.
        /// </summary>
        /// <param propertyName="columns">The <see cref="ComboColumnCollection"/> associated with the <see cref="ComboCellsCollection"/></param>
        /// <param propertyName="row">The <see cref="ComboRowBase"/> that owns the <see cref="ComboCellsCollection"/></param>
        public ComboCellsCollection(ComboColumnCollection columns, ComboRowBase row)
        {
            this._row = row;
            this._columns = columns;
            this._auxColumns = new Dictionary<ComboColumn, ComboCellBase>();
        }

        #endregion // Constructor

        #region Overrides

        #region GetCount

        /// <summary>
        /// Returns the total number of <see cref="ComboCellBase"/>s in the <see cref="ComboCellsCollection"/>
        /// </summary>
        /// <returns>The total number of cells.</returns>
        protected override int GetCount()
        {
            int count = 0;

            if (this._row != null)
            {
                count = this._columns.Count;
            }
            return count;
        }

        #endregion // GetCount

        #region IndexOf

        /// <summary>
        /// Gets the index of the specified <see cref="ComboCellBase"/>.
        /// </summary>
        /// <param propertyName="item"></param>
        /// <returns></returns>
        public override int IndexOf(ComboCellBase item)
        {
            return this._columns.IndexOf(item.Column);
        }

        #endregion // IndexOf

        #region GetItem
        /// <summary>
        /// Returns the <see cref="ComboCellBase"/>  item at the index given.  
        /// </summary>
        /// <param propertyName="index">The index of the cell to be retrieved</param>
        /// <returns>The ComboCellBase object at the given index.</returns>
        protected override ComboCellBase GetItem(int index)
        {
            return this[this._columns[index]];
        }
        #endregion // GetItem

        #endregion // Overrides

        #region Properties

        #region Indexer[ColumnBase]

        /// <summary>
        /// Returns the <see cref="ComboCellBase"/> for the corresponding <see cref="ComboColumn"/>.
        /// </summary>
        /// <param propertyName="column">The column that should be used for reference.</param>
        /// <returns>
        /// The <see cref="ComboCellBase"/> for the corresponding <see cref="ComboColumn"/>.
        /// If no Cell exists, one is created.
        /// If the column doesn't exist, null is returned. 
        /// </returns>
        public ComboCellBase this[ComboColumn column]
        {
            get
            {
                foreach (ComboCellBase cell in this.Items)
                {
                    if (cell.Column == column)
                        return cell;
                }

                if (this._auxColumns.ContainsKey(column))
                    return this._auxColumns[column];

                ComboCellBase newCell = column.GenerateCell(this._row);

                this.Items.Add(newCell);

                this._auxColumns.Add(column, newCell);

                return newCell;
            }
        }
        #endregion // Indexer[ColumnBase]

        #region Indexer[string]

        /// <summary>
        /// Returns the <see cref="ComboCellBase"/> for the corresponding <see cref="ComboColumn"/>.
        /// </summary>
        /// <param propertyName="key">The key of the column that should be used for reference.</param>
        /// <returns>
        /// The <see cref="ComboCellBase"/> for the corresponding <see cref="ComboColumn"/>.
        /// If no Cell exists, one is created.
        /// If the column doesn't exist, null is returned. 
        /// </returns>
        public ComboCellBase this[string key]
        {
            get
            {
                return this[this._columns[key]];
            }
        }
        #endregion // Indexer[string]

        #region Row
        /// <summary>
        /// Gets the Row this collection represents.
        /// </summary>
        protected ComboRowBase Row
        {
            get { return this._row; }
        }
        #endregion // Row

        #endregion // Properties
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved