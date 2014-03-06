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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A Collection of <see cref="CellBase"/> objects.
	/// </summary>
	public class CellBaseCollection : CollectionBase<CellBase>
	{
		#region Members

		RowBase _row;
		ColumnBaseCollection _columns;
		Dictionary<ColumnBase, CellBase> _auxColumns;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="CellBaseCollection"/> class.
		/// </summary>
		/// <param propertyName="columns">The <see cref="ColumnBaseCollection"/> associated with the <see cref="CellBaseCollection"/></param>
		/// <param propertyName="row">The <see cref="RowBase"/> that owns the <see cref="CellBaseCollection"/></param>
		public CellBaseCollection(ColumnBaseCollection columns, RowBase row)
		{
			this._row = row;
			this._columns = columns;
			this._auxColumns = new Dictionary<ColumnBase, CellBase>();
		}

		#endregion // Constructor

		#region Overrides

		#region GetCount

		/// <summary>
		/// Returns the total number of <see cref="CellBase"/>s in the <see cref="CellBaseCollection"/>
		/// </summary>
		/// <returns>The total number of cells.</returns>
		protected override int GetCount()
		{
			int count = 0;

			if (this._row != null)
			{
				count = this._columns.DataColumns.Count;
			} 
			return count;
		}

		#endregion // GetCount

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified <see cref="CellBase"/>.
		/// </summary>
		/// <param propertyName="item"></param>
		/// <returns></returns>
		public override int IndexOf(CellBase item)
		{
			return this._columns.DataColumns.IndexOf(item.Column);
		}

		#endregion // IndexOf

        #region GetItem
        /// <summary>
        /// Returns the <see cref="CellBase"/>  item at the index given.  
        /// </summary>
        /// <param propertyName="index">The index of the cell to be retrieved</param>
        /// <returns>The CellBase object at the given index.</returns>
        protected override CellBase GetItem(int index)
        {            
            return this[this._columns.DataColumns[index]];
        }
        #endregion // GetItem

        #endregion // Overrides

        #region Properties

        #region Indexer[ColumnBase]

        /// <summary>
		/// Returns the <see cref="CellBase"/> for the corresponding <see cref="ColumnBase"/>.
		/// </summary>
		/// <param propertyName="column">The column that should be used for reference.</param>
		/// <returns>
		/// The <see cref="CellBase"/> for the corresponding <see cref="ColumnBase"/>.
		/// If no Cell exists, one is created.
		/// If the column doesn't exist, null is returned. 
		/// </returns>
		public CellBase this[Column column]
		{
			get
			{
				foreach (CellBase cell in this.Items)
				{
					if (cell.Column == column)
						return cell;
				}

				if (this._auxColumns.ContainsKey(column))
					return this._auxColumns[column];

				CellBase newCell = column.GenerateCell(this._row);

				this.Items.Add(newCell);

				this._auxColumns.Add(column, newCell);

				return newCell;
			}
		}
		#endregion // Indexer[ColumnBase]

		#region Indexer[string]

		/// <summary>
		/// Returns the <see cref="CellBase"/> for the corresponding <see cref="ColumnBase"/>.
		/// </summary>
		/// <param propertyName="key">The key of the column that should be used for reference.</param>
		/// <returns>
		/// The <see cref="CellBase"/> for the corresponding <see cref="ColumnBase"/>.
		/// If no Cell exists, one is created.
		/// If the column doesn't exist, null is returned. 
		/// </returns>
		public CellBase this[string key]
		{
			get
			{
				return this[this._columns.DataColumns[key]];
			}
		}
		#endregion // Indexer[string]

		#region Row
		/// <summary>
		/// Gets the Row this collection represents.
		/// </summary>
		protected RowBase Row
		{
			get { return this._row; }
		}
		#endregion // Row

		#endregion // Properties

        #region Methods

        #region Internal

        internal void Reset()
        {
            this._auxColumns.Clear();
            this.Items.Clear();
        }

        #endregion // Internal

        #endregion // Methods
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