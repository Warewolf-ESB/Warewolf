using System;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// The PagerRow is what will be used to control pager functionality on the <see cref="XamGrid"/>.
	/// </summary>
	public class PagerRow : RowBase
	{
		#region Static

		static Type _recyclingElementType = typeof(PagerCellControl);

		#endregion // Static

		#region Members
		SingleCellBaseCollection<PagerCell> _cells;
		PagerColumn _column;
		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="PagerRow"/> class.
		/// </summary>
		/// <param propertyName="manager">The <see cref="RowsManager"/> that owns the <see cref="PagerRow"/>.</param>
		protected internal PagerRow(RowsManager manager)
			: base(manager)
		{
			this.FixedPositionSortOrder = 0;
		}

		#endregion // Constructor

		#region Overrides

		#region ResolveRowHover

		/// <summary>
		/// Resolves whether the entire row or just the individual cell should be hovered when the 
		/// mouse is over a cell. 
		/// </summary>
		protected internal override RowHoverType ResolveRowHover
		{
			get
			{
				return RowHoverType.Cell;
			}
		}
		#endregion // ResolveRowHover

		#region RecyclingElementType
		/// <summary>
		/// Gets the System.Type of the System.Windows.FrameworkElement that is being recycled.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return PagerRow._recyclingElementType;
			}
		}
		#endregion

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="RowBase"/>
		/// </summary>
		public override RowType RowType
		{
			get { return RowType.PagerRow; }
		}

		#endregion // RowType

		#region PagerColumn

		/// <summary>
		/// The <see cref="PagerColumn"/> that represents the single <see cref="CellBase"/> in this row.
		/// </summary>
		internal PagerColumn Column
		{
			get
			{
				if (this._column == null)
					this._column = new PagerColumn();

				return this._column;
			}

		}

		#endregion // PagerColumn

		#region Cells

		/// <summary>
		/// Gets the <see cref="CellBaseCollection"/> that belongs to the <see cref="ChildBand"/>.
		/// </summary>
		public override CellBaseCollection Cells
		{
			get
			{
				if (this._cells == null)
					this._cells = new SingleCellBaseCollection<PagerCell>(this.Column, this.Columns, this);
				return this._cells;
			}
		}

		#endregion // Cells

		#region VisibleCells
		/// <summary>
		/// Gets a list of cells that are visible. 
		/// </summary>
		protected internal override Collection<CellBase> VisibleCells
		{
			get
			{
				return new Collection<CellBase>() { this.Cells[0] };
			}
		}
		#endregion // VisibleCells

		#region CreateInstanceOfRecyclingElement
		/// <summary>
		/// Creates a new instance of a <see cref="PagerCellsPanel"/> for the <see cref="PagerRow"/>.
		/// </summary>
		/// <returns>A new <see cref="PagerCellsPanel"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellsPanel CreateInstanceOfRecyclingElement()
		{
			return new PagerCellsPanel();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#region HeightResolved

		/// <summary>
		/// Resolves the <see cref="RowBase.Height"/> property for this Row.
		/// </summary>
		public override RowHeight HeightResolved
		{
			get
			{
				if (this.Height != null)
					return (RowHeight)this.Height;
				else
					return this.ColumnLayout.PagerSettings.PagerRowHeightResolved;
			}
		}
		#endregion // HeightResolved

        #region CanScrollHorizontally
        /// <summary>
        /// Gets whether or not a row will ever need to scroll horizontally. 
        /// </summary>
        protected internal override bool CanScrollHorizontally
        {
            get { return false; }
        }

        #endregion // CanScrollHorizontally

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