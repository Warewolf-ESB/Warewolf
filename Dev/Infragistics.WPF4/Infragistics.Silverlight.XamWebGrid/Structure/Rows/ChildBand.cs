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
using System.Windows.Data;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// The row that is displayed when a <see cref="ColumnLayout"/>'s header is Visible.
	/// This row also holds any child rows for a particular ColumnLayout of a particular <see cref="Row"/>.
	/// </summary>
	public class ChildBand : ExpandableRowBase
	{
		#region Members

		static Type _recyclingElementType = typeof(ChildBandCellsPanel);
		ColumnLayout _columnLayout;
		FillerColumn _fillerColumn;
		SingleCellBaseCollection<ChildBandCell> _cells;
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChildBand"/> class.
		/// </summary>
		/// <param propertyName="manager">The <see cref="ChildBandRowsManager"/> that owns type his particular row.</param>
		/// <param propertyName="columnLayout">The <see cref="ColumnLayout"/> object that this row represents.</param>
		/// <param propertyName="parentRow">The <see cref="Row"/> that owns this particular row.</param>
		protected internal ChildBand(ChildBandRowsManager manager, ColumnLayout columnLayout, Row parentRow)
			: base(manager)
		{
			this._columnLayout = columnLayout;
			this.ParentRow = parentRow;
			this.Data = parentRow.Data;
			this.ChildRowsManager = new RowsManager(manager.Level, columnLayout, this);
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region ParentRow

		/// <summary>
		/// The <see cref="Row"/> that is the direct parent of this particular row.
		/// </summary>
		public Row ParentRow
		{
			get;
			protected set;
		}

		#endregion // ParentRow

		#region ResolveIsVisible

		/// <summary>
		/// Resolves if the <see cref="ChildBand"/> is actually Visible.
		/// </summary>
		public virtual bool ResolveIsVisible
		{
			get
			{
                if (this.ColumnLayout.Visibility == Visibility.Collapsed)
                    return false;

				if (this.ChildRowsManager.FullRowCount > 0)
				{
					ColumnLayoutHeaderVisibility headerVisbility = this.ColumnLayout.ColumnLayoutHeaderVisibilityResolved;
					if (headerVisbility == ColumnLayoutHeaderVisibility.Always)
						return true;
					else if (headerVisbility == ColumnLayoutHeaderVisibility.SiblingsExist)
						return (this.ParentRow.ChildRowsManager.Rows.Count > 1);
				}

				return false;
			}
		}

		#endregion // ResolveIsVisible

		#region Rows

		/// <summary>
		/// A reference to all of the child <see cref="Row"/>s that this particular row owns.
		/// </summary>
		public RowCollection Rows
		{
			get
			{
				return ((RowCollection)this.ChildRowsManager.Rows.ActualCollection);
			}
		}

		#endregion // Rows

		#region CurrentPageIndex
		/// <summary>
		/// Gets / sets the page of data for the given data island.
		/// </summary>
		public int CurrentPageIndex
		{
			get
			{
				return ((RowsManager)this.ChildRowsManager).CurrentPageIndex;
			}
			set
			{
				if (((RowsManager)this.ChildRowsManager).CurrentPageIndex != value)
					((RowsManager)this.ChildRowsManager).CurrentPageIndex = value;
			}
		}
		#endregion // CurrentPageIndex

		#region RowFiltersCollection

		/// <summary>
		/// Gets a <see cref="RowFiltersCollection"/> object that contains the filters being applied to this <see cref="ColumnLayout"/>.
		/// </summary>
		public RowFiltersCollection RowFiltersCollection
		{
			get
			{
				return ((RowsManager)this.ChildRowsManager).RowFiltersCollection;
			}
		}

		#endregion // RowFiltersCollection

		#region SummaryDefinitionCollection
		/// <summary>
		/// Gets the <see cref="SummaryDefinitionCollection"/> object that contains the summaries being applied to this <see cref="ChildBand"/>
		/// </summary>
		public SummaryDefinitionCollection SummaryDefinitionCollection
		{
			get
			{
				return ((RowsManager)this.ChildRowsManager).SummaryDefinitionCollection;
			}
		}
		#endregion // SummaryDefinitionCollection

		#region SummaryResultCollection
		/// <summary>
		/// Gets the <see cref="SummaryResultCollection"/> object that contains the results of the summaries being applied to this <see cref="ChildBand"/>
		/// </summary>
		public ReadOnlyCollection<SummaryResult> SummaryResultCollection
		{
			get
			{
				return ((RowsManager)this.ChildRowsManager).SummaryResultCollection;
			}
		}
		#endregion // SummaryResultCollection

		#endregion // Public

		#region Internal

		#region FillerColumn

		/// <summary>
		/// Gets the FillerColumn of the <see cref="ChildBand"/>
		/// </summary>
		protected internal ColumnBase FillerColumn
		{
			get
			{
				if (this._fillerColumn == null)
					this._fillerColumn = new FillerColumn();
				return this._fillerColumn;
			}
		}

		#endregion FillerColumn

		#endregion // Internal

		#endregion // Properties

		#region Overrides

		#region Properties

		#region HasChildren

		/// <summary>
		/// Determines if the <see cref="ChildBand"/> has any children.
		/// </summary>
		public override bool HasChildren
		{
			get { return true; }
		}

		#endregion // HasChildren

		#region ColumnLayout
		/// <summary>
		/// The <see cref="ColumnLayout"/> that this row represents.
		/// </summary>
		public override ColumnLayout ColumnLayout
		{
			get
			{
				return this._columnLayout;
			}
		}
		#endregion // ColumnLayout

		#region ChildRowsManager

		/// <summary>
		/// The <see cref="RowsManager"/> of the <see cref="ChildBand"/>'s children.
		/// </summary>
		protected internal override RowsManagerBase ChildRowsManager
		{
			get;
			set;
		}

		#endregion // ChildRowsManager

		#region RecyclingElementType

		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="ChildBand"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return ChildBand._recyclingElementType;
			}
		}
		#endregion // RecyclingElementType

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

		#region Cells

		/// <summary>
		/// Gets the <see cref="CellBaseCollection"/> that belongs to the <see cref="ChildBand"/>.
		/// </summary>
		public override CellBaseCollection Cells
		{
			get
			{
				if (this._cells == null)
					this._cells = new SingleCellBaseCollection<ChildBandCell>(((ChildBandRowsManager)this.Manager).Column, this.Columns, this);
				return this._cells;
			}
		}

		#endregion // Cells

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="RowBase"/>
		/// </summary>
		public override RowType RowType
		{
			get { return RowType.ColumnLayoutHeaderRow; }
		}

		#endregion // RowType

		#region CanScrollHorizontally
		/// <summary>
		/// Gets whether or not a row will ever need to scroll horizontally. 
		/// </summary>
		protected internal override bool CanScrollHorizontally
		{
			get { return false; }
		}

		#endregion // CanScrollHorizontally

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
					return this.ColumnLayout.ChildBandHeaderHeightResolved;
			}
		}
		#endregion // HeightResolved

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

		#endregion // Properties

		#region Methods

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of the <see cref="ChildBandCellsPanel"/>. 
		/// Note: this method is only meant to be invoked via the RecyclingManager.
		/// </summary>
		/// <returns>A new <see cref="ChildBandCellsPanel"/></returns>
		protected override CellsPanel CreateInstanceOfRecyclingElement()
		{
			return new ChildBandCellsPanel();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#endregion // Methods

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