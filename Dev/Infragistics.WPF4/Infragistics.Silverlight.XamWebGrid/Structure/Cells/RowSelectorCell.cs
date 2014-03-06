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

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A cell that represents the row selector of a <see cref="RowBase"/>.
	/// </summary>
	public class RowSelectorCell : CellBase
	{
		#region Static

		static Type _recyclingElementType = typeof(RowSelectorCellControl);

		#endregion // Static

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="RowSelectorCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="RowBase"/> object that owns the <see cref="RowSelectorCell"/></param>
		/// <param propertyName="column">The <see cref="Column"/> object that the <see cref="RowSelectorCell"/> represents.</param>
		protected internal RowSelectorCell(RowBase row, Column column)
			: base(row, column)
		{
		}

		#endregion // Constructor

		#region Overrides

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="RowSelectorCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				if (this.Style == null)
					return this.Row.ColumnLayout.RowSelectorSettings.StyleResolved;
				else
					return this.Style;
			}
		}

		#endregion // ResolveStyle

		#region RecyclingElementType
		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="RowSelectorCell"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return RowSelectorCell._recyclingElementType;
			}
		}
		#endregion // RecyclingElementType

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of a <see cref="RowSelectorCellControl"/> for the <see cref="RowSelectorCell"/>.
		/// </summary>
		/// <returns>A new <see cref="RowSelectorCellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new RowSelectorCellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#region EnsureCurrentState

		/// <summary>
		/// Ensures that <see cref="CellBase"/> is in the correct state.
		/// </summary>
		protected internal override void EnsureCurrentState()
		{
			base.EnsureCurrentState();

			if (this.Control != null)
			{
				XamGrid grid = this.Row.ColumnLayout.Grid;

				if (grid.CurrentEditRow == this.Row || (grid.CurrentEditCell != null && grid.CurrentEditCell.Row == this.Row))
					this.Control.GoToState("Editing", false);
				else if (this.Row.ColumnLayout.Grid.ActiveCell != null && this.Row == this.Row.ColumnLayout.Grid.ActiveCell.Row)
					this.Control.GoToState("Active", false);
				else
					this.Control.GoToState("InActive", false);

				Row r = this.Row as Row;
				if (r != null)
				{
					if (r.IsSelected)
						this.Control.GoToState("Selected", false);
					else
						this.Control.GoToState("NotSelected", false);
				}
			}
		}

		#endregion // EnsureCurrentState

		#region OnCellMouseDown

		/// <summary>
		/// Invoked when a cell is clicked.
		/// </summary>
		/// <returns>Whether or not the method was handled.</returns>
		protected internal override DragSelectType OnCellMouseDown(MouseEventArgs e)
		{
			XamGrid grid = this.Row.ColumnLayout.Grid;
			Row r = this.Row as Row;
			if (r != null && r.Cells.Count > 0)
			{
				if (!r.IsActive)
				{
					Collection<CellBase> visCells = r.VisibleCells;
					if(visCells.Count > 0)
					{
						grid.SetActiveCell(visCells[0], CellAlignment.NotSet, InvokeAction.Click, false);
					}
				}

				if (grid.SelectRow(r, InvokeAction.Click))
					return DragSelectType.None;

				return DragSelectType.Row;
			}

			return base.OnCellMouseDown(e);
		}

		#endregion // OnCellMouseDown

		#region OnCellDragging

		/// <summary>
		/// Invoked when dragging the mouse over a cell. 
		/// </summary>
		protected internal override void OnCellDragging(DragSelectType type)
		{
			base.OnCellDragging(type);

			XamGrid grid = this.Row.ColumnLayout.Grid;
			Row r = this.Row as Row;
			Collection<CellBase> cells = r.VisibleCells;
			if (r != null && cells.Count > 0 && r.RowType != RowType.AddNewRow && r.RowType != RowType.FilterRow)
			{
				if (type == DragSelectType.Row)
					grid.SelectRow(r, InvokeAction.MouseMove);
				else
					grid.SelectCell(cells[0] as Cell, InvokeAction.MouseMove);
			}
		}

		#endregion // OnCellDragging

		#region OnCellClick

        /// <summary>
        /// Invoked when a cell is clicked.
        /// </summary>
        /// <param name="e"></param>
        protected internal override void OnCellClick(MouseButtonEventArgs e)
		{
			this.Row.ColumnLayout.Grid.OnRowSelectorClicked(this.Row);
		}

		#endregion // OnCellClick

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