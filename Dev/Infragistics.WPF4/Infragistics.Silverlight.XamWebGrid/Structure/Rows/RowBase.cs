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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// The base class for all row objects in the <see cref="XamGrid"/>.
	/// </summary>
	public abstract class RowBase : RecyclingContainer<CellsPanel>, IDisposable
	{
		#region Members

		CellsPanel _control;
		CellBaseCollection _cells;
		RowHeight? _height;
		double? _minRowHeight;
		FixedRowSeparator _fixedSeparator;
		IEnumerable _itemSource;
		
        #endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="RowBase"/> class.
		/// </summary>
		/// <param propertyName="manager">The <see cref="RowsManagerBase"/> that owns the <see cref="RowBase"/>.</param>
		protected RowBase(RowsManagerBase manager)
			: base()
		{
			this.Manager = manager;
			this.Index = -1;
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region Data

		/// <summary>
		/// Gets the underlying data associated with the <see cref="RowBase"/>.
		/// </summary>
		public object Data
		{
			get;
			protected internal set;
		}

		#endregion // Data

		#region ColumnLayout

		/// <summary>
		/// Gets the <see cref="ColumnLayout"/> that is associated with the <see cref="RowBase"/>.
		/// </summary>
		public virtual ColumnLayout ColumnLayout
		{
			get
			{
				return this.Manager.ColumnLayout;
			}
		}
		#endregion // ColumnLayout

		#region Manager

		/// <summary>
		/// The <see cref="RowsManagerBase"/> that owns this particular row. 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public RowsManagerBase Manager
		{
			get;
			protected internal set;
		}
		#endregion // Manager

		#region Cells

		/// <summary>
		/// Gets the <see cref="CellBaseCollection"/> that belongs to the <see cref="RowBase"/>.
		/// </summary>
		public virtual CellBaseCollection Cells
		{
			get
			{
				if (this._cells == null)
					this._cells = new CellBaseCollection(this.Columns, this);
				return this._cells;
			}
		}

		#endregion // Cells

		#region Control

		/// <summary>
		/// Gets the <see cref="CellsPanel"/> that is attached to the <see cref="RowBase"/>
		/// </summary>
		/// <remarks>A Control is only assoicated with a Row when it's in the viewport of the <see cref="RowsPanel"/></remarks>
		public CellsPanel Control
		{
			get { return this._control; }
		}

		#endregion // Control

		#region Columns

		/// <summary>
		/// Gets the <see cref="ColumnBaseCollection"/> that is assocated with the <see cref="RowBase"/>.
		/// </summary>
		public ColumnBaseCollection Columns
		{
			get
			{
				return this.ColumnLayout.Columns;
			}
		}

		#endregion // Columns

		#region IsAlternateRow

		/// <summary>
		/// Determines if the currentIndex <see cref="RowBase"/> is an Alternate row.
		/// </summary>
		public virtual bool IsAlternateRow
		{
			get
			{
				return false;
			}
		}

		#endregion // IsAlternateRow

		#region ActualHeight

		/// <summary>
		/// Gets the physical height of the <see cref="RowBase"/>.
		/// Note: this only applies if the <see cref="RowBase"/> had an attached <see cref="CellsPanel"/> at some point.
		/// </summary>
		public double ActualHeight
		{
			get;
			internal set;
		}

		#endregion // ActualHeight

		#region IsMouseOver
		/// <summary>
		/// Gets whether or not the Mouse is currently over the <see cref="RowBase"/>
		/// </summary>
		public bool IsMouseOver
		{
			get;
			protected internal set;
		}

		#endregion // IsMouseOver

		#region IsActive

		/// <summary>
		/// Gets whether or not a cell is the <see cref="RowBase"/> is active.
		/// </summary>
		public bool IsActive
		{
			get;
			protected internal set;
		}

		#endregion // IsActive

		#region Tag

		/// <summary>
		/// Allows a user to store additional information about a <see cref="RowBase"/>
		/// </summary>
		public object Tag
		{
			get;
			set;
		}

		#endregion // Tag

		#region Index

		/// <summary>
		/// Gets the currentIndex index of the <see cref="RowBase"/>
		/// </summary>
		public int Index
		{
			get;
			protected internal set;
		}

		#endregion // Index

		#region Level

		/// <summary>
		/// Gets the level in the hierarchy of the <see cref="Row"/>.
		/// </summary>
		public int Level
		{
			get
			{
				return this.Manager.Level;
			}
		}

		#endregion // Level

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="RowBase"/>
		/// </summary>
		public abstract RowType RowType
		{
			get;
		}

		#endregion // RowType

		#region Height
		/// <summary>
		/// Gets/Sets the Height that will be applied to this particular <see cref="RowBase"/>.
		/// </summary>
		public RowHeight? Height
		{
			get { return this._height; }
			set
			{
				this._height = value;
				this.OnPropertyChanged("Height");
				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
					this.ColumnLayout.Grid.InvalidateScrollPanel(false);
			}
		}

		#endregion // Height

		#region HeightResolved

		/// <summary>
		/// Resolves the <see cref="RowBase.Height"/> property for this Row.
		/// </summary>
		public virtual RowHeight HeightResolved
		{
			get
			{
				if (this.Height == null)
					return this.ColumnLayout.RowHeightResolved;
				else
					return (RowHeight)this.Height;
			}
		}

		#endregion // HeightResolved

		#region MinimumRowHeight

		/// <summary>
		/// Gets/Sets the Minimum Height of a <see cref="RowBase"/>. 
		/// </summary>
		/// <remarks>
		/// This value is ignored if RowHeight is of Type Numeric.
		/// </remarks>
		public double? MinimumRowHeight
		{
			get { return this._minRowHeight; }
			set
			{
				this._minRowHeight = value;
				this.OnPropertyChanged("MinimumRowHeight");
			}
		}

		#endregion // MinimumRowHeight

		#region MinimumRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="RowBase.MinimumRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public double MinimumRowHeightResolved
		{
			get
			{
				if (this.MinimumRowHeight == null)
					return this.ColumnLayout.MinimumRowHeightResolved;
				else
					return (double)this.MinimumRowHeight;
			}
		}

		#endregion // MinimumRowHeightResolved

        #region MergeData

        /// <summary>
        /// Gets the <see cref="MergedRowInfo"/> for the <see cref="RowBase"/> if the row contains any Merged Columns, otherwise null.
        /// </summary>
        public MergedRowInfo MergeData
        {
            get;
            protected internal set;
        }

        #endregion // MergeData

        #endregion // Public

        #region Protected

        #region ResolveRowHover

        /// <summary>
		/// Resolves whether the entire row or just the individual cell should be hovered when the 
		/// mouse is over a cell. 
		/// </summary>
		protected internal virtual RowHoverType ResolveRowHover
		{
			get
			{
				return RowHoverType.None;
			}
		}

		#endregion // ResolveRowHover

	    #region VisibleCells
		/// <summary>
		/// Gets a list of cells that are visible. 
		/// </summary>
		protected internal virtual Collection<CellBase> VisibleCells
		{
			get
			{
				Collection<CellBase> cells = new Collection<CellBase>();

                if (this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                {
                    ReadOnlyCollection<Column> mergedCols = this.ColumnLayout.Grid.GroupBySettings.GroupByColumns[this.ColumnLayout];
                    foreach (Column mergedCol in mergedCols)
                    {
                        if (mergedCol.Visibility == Visibility.Visible)
                        {
                            if (mergedCol.SupportsActivationAndSelection)
                            {
                                cells.Add(this.Cells[mergedCol]);
                            }
                        }
                    }
                }

                foreach (Column fixedCol in this.Columns.FixedColumnsLeft)
                {
                    if (fixedCol.Visibility == Visibility.Visible)
                    {
                        if (fixedCol.SupportsActivationAndSelection)
                        {
                            cells.Add(this.Cells[fixedCol]);
                        }

                        ReadOnlyKeyedColumnBaseCollection<Column> allCols = fixedCol.AllColumns;
                        if (allCols.Count > 0)
                        {
                            foreach (Column column in allCols)
                            {
                                if (column.SupportsActivationAndSelection && column.Visibility == Visibility.Visible)
                                {
                                    cells.Add(this.Cells[column]);
                                }
                            }
                        }
                    }
                }

				foreach (Column col in this.Columns.VisibleColumns)
				{
                    if (col != this.Columns.FillerColumn)
                    {
                        if (col.SupportsActivationAndSelection)
                        {
                            cells.Add(this.Cells[col]);
                        }

                        ReadOnlyKeyedColumnBaseCollection<Column> allCols = col.AllColumns;
                        if (allCols.Count > 0)
                        {
                            foreach (Column column in allCols)
                            {
                                if (column.SupportsActivationAndSelection && column.Visibility == Visibility.Visible)
                                {
                                    cells.Add(this.Cells[column]);
                                }
                            }
                        }
                    }
				}

				FixedColumnsCollection right = this.Columns.FixedColumnsRight;
				int count = right.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    Column col = right[i];

                    if (col.Visibility == Visibility.Visible)
                    {
                        if (col != this.Columns.FillerColumn)
                        {
                            if (col.SupportsActivationAndSelection)
                            {
                                cells.Add(this.Cells[col]);
                            }

                            ReadOnlyKeyedColumnBaseCollection<Column> allCols = col.AllColumns;
                            if (allCols.Count > 0)
                            {
                                foreach (Column column in allCols)
                                {
                                    if (column.SupportsActivationAndSelection && column.Visibility == Visibility.Visible)
                                    {
                                        cells.Add(this.Cells[column]);
                                    }
                                }
                            }
                        }
                    }
                }

				return cells;
			}
		}
		#endregion // VisibleCells

		#region FixedPositionSortOrder
		/// <summary>
		/// Gets / sets the sort position.
		/// </summary>
		protected internal int FixedPositionSortOrder
		{
			get;
			set;
		}
		#endregion

		#region ItemSource

		/// <summary>
		/// Gets/Sets the children data that the row owns. 
		/// </summary>
		/// <remarks>
		/// This property is only used in such cases as <see cref="GroupByRow"/> where it's data 
		/// does not contain an explicit IEnumerable.
		/// </remarks>
		protected internal IEnumerable ItemSource
		{
			get { return this._itemSource; }
			set
			{
				if (value != this._itemSource)
				{
					this._itemSource = value;
					this.OnItemSourceChanged();	
				}
			}
		}
		#endregion // ItemSource

		#region CanScrollHorizontally
		/// <summary>
		/// Gets whether or not a row will ever need to scroll horizontally. 
		/// </summary>
		protected internal virtual bool CanScrollHorizontally
		{
			get { return true; }
		}

		#endregion // CanScrollHorizontally

		#region AllowEditing
		/// <summary>
		/// Gets whether editing will be allowed on the <see cref="RowBase"/>.
		/// </summary>
		protected internal virtual EditingType AllowEditing
		{
			get
			{
				return EditingType.None;
			}
		}
		#endregion // AllowEditing

		#region AllowKeyboardNavigation
		/// <summary>
		/// Gets whether the <see cref="RowBase"/> will allow keyboard navigation.
		/// </summary>
		protected internal virtual bool AllowKeyboardNavigation
		{
			get
			{
				return false;
			}
		}
		#endregion // AllowKeyboardNavigation

		#region AllowSelection
		/// <summary>
		/// Gets whether selection will be allowed on the <see cref="RowBase"/>.
		/// </summary>
		protected internal virtual bool AllowSelection
		{
			get
			{
				return false;
			}
		}
		#endregion // AllowSelection

		#region IsStandAloneRow
		/// <summary>
		/// Gets whether this <see cref="Row"/> can stand alone, when there are no other data rows.
		/// </summary>
		protected internal virtual bool IsStandAloneRow
		{
			get
			{
				return false;
			}
		}
		#endregion // IsStandAloneRow

		#region IsStandAloneResolved
		/// <summary>
		/// Resolves whether this <see cref="Row"/> will stand alone, based on the state of the grid.
		/// </summary>
		protected internal virtual bool IsStandAloneRowResolved
		{
			get
			{
				return false;
			}
		}
		#endregion // IsStandAloneResolved

		#region RequiresFixedRowSeparator
		/// <summary>
		/// Used to determine if a FixedRow separator is neccessary for this <see cref="RowBase"/>
		/// </summary>
		protected internal virtual bool RequiresFixedRowSeparator
		{
			get
			{
				return false;
			}
		}
		#endregion //RequiresFixedRowSeparator

		#endregion // Protected

        #region Internal

        #region CachedClipboardIndex

        internal int CachedClipboardIndex
        {
            get;
            set;
        }

        #endregion // CachedClipboardIndex

        #region InCellConverter

        internal bool InCellConverter { get; set; }

        #endregion // InCellConverter

        #region IgnoreCellControlAttached

        internal bool IgnoreCellControlAttached { get; set; }

        #endregion // IgnoreCellControlAttached

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Protected

        #region ResolveCell

        /// <summary>
		/// Returns the <see cref="CellBase"/> for the specified <see cref="ColumnBase"/>
		/// </summary>
		/// <param propertyName="column">The Column in which to resolve the cell.</param>
		/// <returns>The cell at the given column location.</returns>
		protected internal virtual CellBase ResolveCell(Column column)
		{
			return this.Cells[column];
		}
		#endregion // ResolveCell

		#region CellEditorValueChanged
		/// <summary>
		/// Called while the cell in a row is edited. 		
		/// </summary>
		/// <remarks>
		/// Designed to be used by rows who need to do actions while the editor control is being edited.
		/// </remarks>
		/// <param propertyName="cellBase"></param>
		/// <param propertyName="newValue"></param>
		protected internal virtual void CellEditorValueChanged(CellBase cellBase, object newValue)
		{

		}
		#endregion // CellEditorValueChanged

		#region OnItemSourceChanged

		/// <summary>
		/// Invoked when the ItemSource property changes.
		/// </summary>
		protected virtual void OnItemSourceChanged()
		{

		}

		#endregion // OnItemSourceChanged

        #region GetCellValue
        /// <summary>
        /// Performs the cell.Value without forcing the cell to be made.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected internal virtual object GetCellValue(Column column)
        {
            return null;
        }
        #endregion // GetCellValue

        #endregion // Protected

        #region Internal

        internal FixedRowSeparator ResolveSeparator()
		{
			if (this._fixedSeparator == null)
				this._fixedSeparator = new FixedRowSeparator();
			return this._fixedSeparator;
		}

		#endregion // Internal

		#endregion // Methods

		#region Overrides

		#region OnElementAttached

		/// <summary>
		/// Called when the <see cref="CellsPanel"/> is attached to the <see cref="RowBase"/>
		/// </summary>
		/// <param propertyName="element">A <see cref="CellsPanel"/></param>
		protected override void OnElementAttached(CellsPanel element)
		{
			this._control = element;
			this._control.OnAttached(this);
		}
		#endregion // OnElementAttached

		#region OnElementReleased

		/// <summary>
		/// Called when the <see cref="CellsPanel"/> is removed from the <see cref="RowBase"/>
		/// </summary>
		/// <param propertyName="element">A <see cref="CellsPanel"/></param>
		protected override void OnElementReleased(CellsPanel element)
		{
			this._control = null;

			element.OnReleased();

			this.IsMouseOver = false;
		}
		#endregion // OnElementReleased

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of the CellsPanel. 
		/// Note: this method is only meant to be invoked via the RecyclingManager.
		/// </summary>
		/// <returns>A new <see cref="CellsPanel"/></returns>
		protected override CellsPanel CreateInstanceOfRecyclingElement()
		{
			return new CellsPanel();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region ToString



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		#endregion // ToString

		#endregion // Overrides

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="RowBase"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._cells != null)
				this._cells.Dispose();
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="RowBase"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
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