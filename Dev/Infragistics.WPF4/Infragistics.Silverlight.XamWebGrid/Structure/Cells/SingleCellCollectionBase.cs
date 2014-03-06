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

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A Collection that will only ever contain a single <see cref="CellBase"/>.
	/// </summary>
	/// <remarks>
	/// This collection is used on <see cref="Row"/> objects such as the <see cref="PagerRow"/> and <see cref="ChildBand"/>
	/// </remarks>
	public class SingleCellBaseCollection<T> : CellBaseCollection
	{
		#region Members

		Column _owningColumn;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the SingleCellCollectionBase class.
		/// </summary>
		/// <param propertyName="owningColumn">The column that owns the single cell in this collection.</param>
		/// <param propertyName="columns">The <see cref="ColumnBaseCollection"/> associated with the <see cref="CellBaseCollection"/></param>
		/// <param propertyName="row">The <see cref="RowBase"/> that owns the <see cref="CellBaseCollection"/></param>
		public SingleCellBaseCollection(Column owningColumn, ColumnBaseCollection columns, RowBase row)
			: base(columns, row)
		{
			this._owningColumn = owningColumn;
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
			return 1;
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
			if (item is T)
				return 0;
			else
				return -1;
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
			if (index == 0)
				return this[this._owningColumn];
			else
				return null;
		}
		#endregion // GetItem

		#endregion // Overrides
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