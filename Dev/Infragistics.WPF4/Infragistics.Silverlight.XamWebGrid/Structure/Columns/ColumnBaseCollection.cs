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
using System.Collections;
using System.ComponentModel;
using Infragistics.Controls.Grids.Primitives;
using System.Reflection;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A Collection of <see cref="ColumnBase"/> objects.
	/// </summary>
    public class ColumnBaseCollection : CollectionBase<ColumnBase>, IProvidePersistenceLookupKeys, IProvidePropertyPersistenceSettings
	{
		#region Members

		ColumnLayout _columnLayout;
		ReadOnlyKeyedColumnBaseCollection<ColumnLayout> _columnLayouts;
		List<ColumnLayout> _internalColumnLayouts;
		List<Column> _internalFixedAdornerColumns;
		List<Column> _internalVisibleColumns;
		Collection<Column> _nonLayoutColumns;
        Collection<Column> _starColumns;

		ReadOnlyKeyedColumnBaseCollection<Column> _fixedAdornerColumns;
		ReadOnlyKeyedColumnBaseCollection<Column> _visibleColumns;
		ReadOnlyKeyedColumnBaseCollection<Column> _dataColumns;
        ReadOnlyKeyedColumnBaseCollection<Column> _startColumnsReadOnly;

		ExpansionIndicatorColumn _expansionIndicatorColumn;
		RowSelectorColumn _rowSelectorColumn;
		FillerColumn _fillerColumn;
		FixedBorderColumn _fixedBorderColumnLeft, _fixedBorderColumnRight;
        List<string> _propertiesThatShouldntBePersisted;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnBaseCollection"/> class.
		/// </summary>
		/// <param propertyName="columnLayout">The <see cref="ColumnLayout"/> object that owns the <see cref="ColumnBaseCollection"/>.</param>
		public ColumnBaseCollection(ColumnLayout columnLayout)
		{
			this._columnLayout = columnLayout;

			this._internalColumnLayouts = new List<ColumnLayout>();
			this._columnLayouts = new ReadOnlyKeyedColumnBaseCollection<ColumnLayout>(this._internalColumnLayouts);

			this._internalFixedAdornerColumns = new List<Column>();
			this._fixedAdornerColumns = new ReadOnlyKeyedColumnBaseCollection<Column>(this._internalFixedAdornerColumns);

			this._internalVisibleColumns = new List<Column>();
			this._visibleColumns = new ReadOnlyKeyedColumnBaseCollection<Column>(this._internalVisibleColumns);

			this._fillerColumn = new FillerColumn();
			this._fillerColumn.ColumnLayout = this._columnLayout;

			this._nonLayoutColumns = new Collection<Column>();
			this._dataColumns = new ReadOnlyKeyedColumnBaseCollection<Column>(this._nonLayoutColumns);

            this._starColumns = new Collection<Column>();
            this._startColumnsReadOnly = new ReadOnlyKeyedColumnBaseCollection<Column>(this._starColumns);			
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region ColumnLayout

		/// <summary>
		/// Gets the <see cref="ColumnLayout"/> object that owns the <see cref="ColumnBaseCollection"/>.
		/// </summary>
		public ColumnLayout ColumnLayout
		{
			get { return this._columnLayout; }
		}

		#endregion // ColumnLayout

		#region ColumnLayouts

		/// <summary>
		/// Gets the <see cref="System.Collections.ObjectModel.ReadOnlyCollection&lt;ColumnLayout&gt;"/> of <see cref="ColumnLayout"/>s that it owns.
		/// </summary>
		public ReadOnlyKeyedColumnBaseCollection<ColumnLayout> ColumnLayouts
		{
			get
			{

				return this._columnLayouts;
			}
		}

		#endregion // ColumnLayouts

		#region FixedAdornerColumns

		/// <summary>
		/// A collection of <see cref="ColumnBase"/> objects that should be displayed first and fixed in the <see cref="XamGrid"/>
		/// Note: these columns generally make up such columns that don't represent fields in the underlying data source such as the <see cref="RowSelectorColumn"/> and <see cref="ExpansionIndicatorColumn"/>. 
		/// </summary>
		public ReadOnlyKeyedColumnBaseCollection<Column> FixedAdornerColumns
		{
			get
			{
				return this._fixedAdornerColumns;
			}
		}

		#endregion // FixedAdornerColumns

		#region ExpansionIndicatorColumn

		/// <summary>
		/// Gets the <see cref="Column"/> that represents the ExpansionIndicator area of a specific <see cref="ColumnLayout"/>.
		/// </summary>
		public ExpansionIndicatorColumn ExpansionIndicatorColumn
		{
			get
			{
				if (this._expansionIndicatorColumn == null)
				{
					this._expansionIndicatorColumn = new ExpansionIndicatorColumn();
					this._expansionIndicatorColumn.ColumnLayout = this._columnLayout;
				}

				return this._expansionIndicatorColumn;
			}
		}
		#endregion // ExpansionIndicatorColumn

		#region RowSelectorColumn

		/// <summary>
		/// Gets the <see cref="Column"/> that represents the RowSelector area of a specific <see cref="ColumnLayout"/>.
		/// </summary>
		public RowSelectorColumn RowSelectorColumn
		{
			get
			{
				if (this._rowSelectorColumn == null)
				{
					this._rowSelectorColumn = new RowSelectorColumn();
					this._rowSelectorColumn.ColumnLayout = this._columnLayout;
				}

				return this._rowSelectorColumn;
			}
		}
		#endregion // RowSelectorColumn

		#region Indexer[string]

		/// <summary>
		/// Gets the <see cref="ColumnBase"/> that has the specified key. 
		/// </summary>
		/// <param propertyName="key"></param>
		/// <returns>
		/// The column with the specified Key. 
		/// If more than one <see cref="ColumnBase"/> has the same key, the first Column is returned.
		/// </returns>
		public ColumnBase this[string key]
		{
			get
			{
				foreach (ColumnBase column in this.Items)
				{
					if (column.Key == key)
						return column;
				}
				return null;
			}
		}

		#endregion // Indexer[string]

		#region DataColumns

		/// <summary>
		/// Gets a list of columns that are of type <see cref="Column"/>.
		/// </summary>
		public ReadOnlyKeyedColumnBaseCollection<Column> DataColumns
		{
			get { return this._dataColumns; }
		}

		#endregion // DataColumns

		#region FillerColumn

		/// <summary>
		/// Gets the column that is used to fill up empty space in a row. 
		/// </summary>
		public FillerColumn FillerColumn
		{
			get { return this._fillerColumn; }
		}

		#endregion // FillerColumn

		#region FixedBorderColumnLeft

		/// <summary>
		/// Gets the column that seperates the pinned an unpinned columns.
		/// </summary>
		public FixedBorderColumn FixedBorderColumnLeft
		{
			get
			{
				if (this._fixedBorderColumnLeft == null)
					this._fixedBorderColumnLeft = new FixedBorderColumn();
				return this._fixedBorderColumnLeft;
			}
		}

		#endregion // FixedBorderColumnLeft

		#region FixedBorderColumnRight

		/// <summary>
		/// Gets the column that seperates the pinned an unpinned columns.
		/// </summary>
		public FixedBorderColumn FixedBorderColumnRight
		{
			get
			{
				if (this._fixedBorderColumnRight == null)
					this._fixedBorderColumnRight = new FixedBorderColumn();
				return this._fixedBorderColumnRight;
			}
		}

		#endregion // FixedBorderColumnRight

		#region FixedColumnsLeft

		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that are pinned to the left side of a particular <see cref="ColumnLayout"/>
		/// </summary>
		public FixedColumnsCollection FixedColumnsLeft
		{
			get
			{
				return this.ColumnLayout.FixedColumnSettings.FixedColumnsLeft;
			}
		}

		#endregion // FixedColumnsLeft

		#region GroupByColumns

		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that determine how a data should be grouped for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public GroupByColumnsCollection GroupByColumns
		{
			get
			{
				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
					return this.ColumnLayout.Grid.GroupBySettings.GroupByColumns;

				return null;
			}
		}

		#endregion // GroupByColumns

		#region SelectedColumns

		/// <summary>
		/// Gets a collection of <see cref="Column"/> that are currently selected in the <see cref="XamGrid"/>.
		/// </summary>
		public SelectedColumnsCollection SelectedColumns
		{
			get
			{
				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
					return this.ColumnLayout.Grid.SelectionSettings.SelectedColumns;

				return null;
			}
		}

		#endregion // SelectedColumns

		#region FixedColumnsRight

		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that are pinned to the right side of a particular <see cref="ColumnLayout"/>
		/// </summary>
		public FixedColumnsCollection FixedColumnsRight
		{
			get
			{
				return this.ColumnLayout.FixedColumnSettings.FixedColumnsRight;
			}
		}

		#endregion // FixedColumnsRight

		#region SortedColumns
		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that are sorted in a particular <see cref="ColumnLayout"/>
		/// </summary>
		public SortedColumnsCollection SortedColumns
		{
			get
			{
				return this.ColumnLayout.SortingSettings.SortedColumns;
			}
		}
		#endregion // SortedColumns

        #region AllColumns

        /// <summary>
        /// Gets a recursive collection of all <see cref="ColumnBase"/> objects in particular <see cref="ColumnLayout"/>
        /// </summary>
        public ReadOnlyKeyedColumnBaseCollection<ColumnBase> AllColumns
        {
            get
            {
                List<ColumnBase> allColumns = new List<ColumnBase>();
                foreach (ColumnBase col in this.Items)
                {
                    allColumns.Add(col);
                    Column column = col as Column;
                    if (column != null)
                    {
                        ReadOnlyKeyedColumnBaseCollection<Column> cols = column.AllColumns;
                        foreach (Column c in cols)
                            allColumns.Add(c);
                    }
                }

                return new ReadOnlyKeyedColumnBaseCollection<ColumnBase>(allColumns);
            }

        }

        #endregion // AllColumns

        #region AllVisibleColumns

        /// <summary>
        /// Gets a collection of all visible <see cref="Column"/> objects in particular <see cref="ColumnLayout"/>
        /// </summary>
        [Browsable(false)]
        public ReadOnlyKeyedColumnBaseCollection<Column> AllVisibleColumns
        {
            get
            {
                List<Column> allColumns = new List<Column>();
                foreach (ColumnBase col in this.VisibleColumns)
                {
                    Column column = col as Column;
                    if (column != null)
                    {
                        allColumns.Add(column);
                    }
                }

                return new ReadOnlyKeyedColumnBaseCollection<Column>(allColumns);
            }

        }

        #endregion // AllVisibleColumns

        #region PropertiesToIgnore

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                if (this._propertiesThatShouldntBePersisted == null)
                {
                    this._propertiesThatShouldntBePersisted = new List<string>()
					{
                        "AllColumns",
                        "AllVisibleColumns"
					};
                }

                return this._propertiesThatShouldntBePersisted;
            }
        }
        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected virtual List<string> PriorityProperties
        {
            get { return null; }
        }

        #endregion // PriorityProperties

        #region AllVisibleChildColumns

        /// <summary>
        /// Gets a collection of all visible <see cref="Column"/> objects that have no children a in particular <see cref="ColumnLayout"/>
        /// </summary>
        public ReadOnlyKeyedColumnBaseCollection<Column> AllVisibleChildColumns
        {
            get
            {
                List<Column> allColumns = new List<Column>();
                foreach (ColumnBase col in this.VisibleColumns)
                {
                    Column column = col as Column;
                    if (column != null)
                    {
                        ReadOnlyKeyedColumnBaseCollection<Column> children = column.AllVisibleChildColumns;
                        if (children.Count == 0)
                            allColumns.Add(column);
                        else
                        {
                            allColumns.AddRange(children);
                        }
                    }
                }

                return new ReadOnlyKeyedColumnBaseCollection<Column>(allColumns);
            }

        }

        #endregion // AllVisibleChildColumns

        #endregion // Public

        #region Protected

        #region VisibleColumns

        /// <summary>
		/// Gets a readonly list of <see cref="Column"/> objects that are not hidden and are not <see cref="ColumnLayout"/> objects.
		/// </summary>
		protected internal ReadOnlyCollection<Column> VisibleColumns
		{
			get { return this._visibleColumns; }
		}

		#endregion // VisibleColumns

        #region StarColumns

        /// <summary>
        /// Gets a readonly list of <see cref="Column"/> objects that have a width that are Star.
        /// </summary>
        protected internal ReadOnlyCollection<Column> StarColumns
        {
            get { return this._startColumnsReadOnly; }
        }

        #endregion // StarColumns

        #endregion // Protected

        #region Internal

        internal Type DataType
		{
			get;
			set;
		}

        internal List<ColumnLayout> InternalColumnLayouts
        {
            get { return this._internalColumnLayouts; }
        }

		#endregion // Internal

		#endregion // Properties

		#region Methods

		#region Protected

		#region RegisterFixedAdornerColumn

		/// <summary>
		/// Registers the specified <see cref="ColumnBase"/> as a fixed column.
		/// </summary>
		/// <param propertyName="column"></param>
		/// <param propertyName="first">Specifies whether or not the column should be inserted at the first index position.</param>
		protected virtual void RegisterFixedAdornerColumn(Column column, bool first)
		{
			if (!this._internalFixedAdornerColumns.Contains(column))
			{
				if (first)
					this._internalFixedAdornerColumns.Insert(0, column);
				else
					this._internalFixedAdornerColumns.Add(column);
			}
		}

		#endregion // RegisterFixedAdornerColumn

		#region UnregisterFixedAdornerColumn

		/// <summary>
		/// Unregisters the specified <see cref="ColumnBase"/> as a fixed column.
		/// </summary>
		/// <param propertyName="column"></param>
		protected virtual void UnregisterFixedAdornerColumn(Column column)
		{
			this._internalFixedAdornerColumns.Remove(column);
		}
		#endregion // UnregisterFixedAdornerColumn

		#region InvalidateColumnsCollections

		/// <summary>
		/// Determines which columns are Fixed, Visible or Hidden. 
		/// </summary>
		/// <param propertyName="fullInvalidate"></param>
		protected internal virtual void InvalidateColumnsCollections(bool fullInvalidate)
		{
			this._internalVisibleColumns.Clear();
			this._nonLayoutColumns.Clear();
            this._starColumns.Clear();
            bool starColumnFound = false;
            bool checkForMergedColumns = false;

            XamGrid grid = null;

            if (this.ColumnLayout != null)
            {
                grid = this.ColumnLayout.Grid;
            }

            if (grid != null)
                checkForMergedColumns = (grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells);

			foreach (ColumnBase column in this.Items)
			{
				Column col = column as Column;
				if (col == null)
					continue;

				this._nonLayoutColumns.Add(col);

				bool isFixed = !(col.IsFixed == FixedState.NotFixed);
                bool isMerged = (checkForMergedColumns && col.IsGroupBy);

				if (!isMerged && !isFixed && column.Visibility == Visibility.Visible)
					this._internalVisibleColumns.Add(col);

                if (col.WidthResolved.WidthType == ColumnWidthType.Star)
                {
                    starColumnFound = true;
                    this._starColumns.Add(col);
                }
			}

			if (!starColumnFound)
				this._internalVisibleColumns.Add(this._fillerColumn);

            if (fullInvalidate)
            {
                this.InvalidateCollection(this.FixedColumnsLeft);
                this.InvalidateCollection(this.FixedColumnsRight);
                this.InvalidateCollection(this.SortedColumns);
            }

			if (grid != null)
				grid.InvalidateScrollPanel(true);

		}

		#endregion // InvalidateColumnsCollections

		#region GetLookupKeys

		/// <summary>
		/// Gets a list of keys that each object in the collection has. 
		/// </summary>
		/// <returns></returns>
		protected virtual Collection<string> GetLookupKeys()
		{
			Collection<string> keys = new Collection<string>();

			foreach (ColumnBase col in this.Items)
			{
				keys.Add(col.Key);
			}

			return keys;
		}

		#endregion // GetLookupKeys

		#region CanRehydrate

		/// <summary>
		/// Looks through the keys, and determines that all the keys are in the collection, and that the same about of objects are in the collection.
		/// If this isn't the case, false is returned, and the Control Persistence Framework, will not try to reuse the object that are already in the collection.
		/// </summary>
		/// <param name="lookupKeys"></param>
		/// <returns></returns>
		protected virtual bool CanRehydrate(Collection<string> lookupKeys)
		{
			if (lookupKeys == null || lookupKeys.Count != this.Items.Count)
				return false;

			bool reorderNeeded = false;

			Dictionary<string, ColumnBase> cols = new Dictionary<string, ColumnBase>();

			for (int i = 0; i < lookupKeys.Count; i++)
			{
				string key = lookupKeys[i];

				ColumnBase col = this[key];
				cols.Add(key, col);

				if (this[key] == null)
					return false;
				else
				{
					if (this.IndexOf(col) != i)
					{
						reorderNeeded = true;
					}
				}
			}

			if (reorderNeeded)
			{
				foreach (string key in lookupKeys)
				{
					ColumnBase col = this[0];

					Column column = col as Column;
					if (column != null)
					{
						column.IsMoving = true;
					}

					this.RemoveItemSilently(0);

					if (column != null)
					{
						column.IsMoving = false;
					}
				}

				for (int i = 0; i < lookupKeys.Count; i++)
				{
					ColumnBase col = cols[lookupKeys[i]];
					Column column = col as Column;

					this.AddItemSilently(i, col);
				}

			}

			return true;
		}

		#endregion // CanRehydrate

        #region FinishedLoadingPersistence

        /// <summary>
        /// Allows an object to perform an operation, after it's been loaded.
        /// </summary>
        protected virtual void FinishedLoadingPersistence()
        {

        }

        #endregion // FinishedLoadingPersistence

		#endregion // Protected

		#region Private

        #region InvalidateCollection

        private void InvalidateCollection(IList list)
		{
            ReadOnlyKeyedColumnBaseCollection<ColumnBase> allColumns = this.AllColumns;
            int count = list.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (!allColumns.Contains((ColumnBase)list[i]))
                    list.RemoveAt(i);
            }
		}

        #endregion // InvalidateCollection

        #region InvalidateSelection
        internal void InvalidateSelection(ColumnBase colBase)
		{
			Column c = colBase as Column;
			if (c != null)
				InvalidateSelection(c);
			else
			{
				ColumnLayout cl = colBase as ColumnLayout;
				if (cl != null)
					InvalidateSelection(cl);
			}
		}

        internal void InvalidateSelection(ColumnLayout colLayout)
		{
			if (colLayout != this.ColumnLayout)
			{
				foreach (ColumnBase colBase in colLayout.Columns.AllColumns)
				{
					if (colBase != colLayout)
						InvalidateSelection(colBase);
				}

				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
				{
					SelectedRowsCollection rows = this.ColumnLayout.Grid.SelectionSettings.SelectedRows;
					Collection<Row> rowsToRemove = new Collection<Row>();
					foreach (Row row in rows)
					{
						if (row.ColumnLayout == colLayout)
						{
							rowsToRemove.Add(row);
						}
					}
					foreach (Row row in rowsToRemove)
						rows.Remove(row);
				}
			}
		}

        internal void InvalidateSelection(Column col)
		{
			GroupByColumnsCollection gbCols = this.ColumnLayout.Grid.GroupBySettings.GroupByColumns;
			if (gbCols.Contains(col))
				gbCols.Remove(col);

			SelectedColumnsCollection cols = this.ColumnLayout.Grid.SelectionSettings.SelectedColumns;
			if (cols.Contains(col))
				cols.Remove(col);

			SelectedCellsCollection cells = this.ColumnLayout.Grid.SelectionSettings.SelectedCells;
			Collection<Cell> cellsToRemove = new Collection<Cell>();
			foreach (Cell cell in cells)
			{
				if (cell.Column == col)
					cellsToRemove.Add(cell);
			}

			foreach (Cell cell in cellsToRemove)
				cells.Remove(cell);

		}
		#endregion // InvalidateSelection

        #region InvalidateActiveCell

        internal void InvalidateActiveCell(ColumnBase removedColumnBase)
		{
			CellBase activeCell = this.ColumnLayout.Grid.ActiveCell;

			if (activeCell != null)
			{
				
				if (removedColumnBase is Column)
				{
					if (activeCell.Column.Equals(removedColumnBase))
					{
						this.ColumnLayout.Grid.ActiveCell = null;
					}
				}
				else
				{
					

					ColumnBase activeCellColumnLayout = activeCell.Row.ColumnLayout;
					while (activeCellColumnLayout != null)
					{
						if (activeCellColumnLayout.Equals(removedColumnBase))
						{
							this.ColumnLayout.Grid.ActiveCell = null;
							break;
						}
						activeCellColumnLayout = activeCellColumnLayout.ColumnLayout;
					}

					ChildBandCell activeCellAsChildBandCell = this.ColumnLayout.Grid.ActiveCell as ChildBandCell;
					if (activeCellAsChildBandCell != null)
					{
						activeCellColumnLayout = activeCell.Row.ColumnLayout;
						ColumnLayout activeCellColumnLayoutParentLayout = activeCellColumnLayout.ColumnLayout;
						if (activeCellColumnLayoutParentLayout != null)

							if (activeCellColumnLayoutParentLayout.ColumnLayoutHeaderVisibilityResolved == ColumnLayoutHeaderVisibility.SiblingsExist
							&&
							activeCellColumnLayoutParentLayout.Columns.ColumnLayouts.Count < 2
							)
							{
								this.ColumnLayout.Grid.ActiveCell = null;
							}
					}
				}
			}
		}

        #endregion // InvalidateActiveCell

        #endregion // Private

        #region Internal

        internal void InternalRegisterFixedAdornerColumn(Column column, bool first)
		{
			this.RegisterFixedAdornerColumn(column, first);
		}

		internal void InternalUnregisterFixedAdornerColumn(Column column)
		{
			this.UnregisterFixedAdornerColumn(column);
		}

		#endregion // Internal

		#endregion // Methods

		#region Overrides

		#region InsertItem
		/// <summary>
		/// Inserts the <see cref="ColumnBase"/> at the specified index.
		/// </summary>
		/// <param propertyName="index">The index that the Column should be inserted.</param>
		/// <param propertyName="item">The <see cref="ColumnBase"/> that should be inserted.</param>
        protected override void AddItem(int index, ColumnBase item)
        {
            if(!item.IsMoving)
            {
                this.ColumnLayout.OnColumnAdded(item);
                base.AddItem(index, item);
            }
            else
            {
                base.AddItem(index, item);

                ColumnLayout colLayout = item as ColumnLayout;
                if (colLayout != null)
                    this.ColumnLayout.OnChildColumnLayoutAdded(colLayout);
            }
        }

		/// <summary>
		/// Inserts the specified <see cref="ColumnBase"/> as the specified index. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		public override void Insert(int index, ColumnBase item)
		{
			this.AddItem(index, item);
		}

		#endregion // InsertItem

		#region RemoveItem

		/// <summary>
		/// Removes the <see cref="ColumnBase"/> a the specified index. 
		/// </summary>
		/// <param propertyName="index">The index of the Column that should be removed.</param>
		protected override bool RemoveItem(int index)
		{
            ColumnBase cb = this.Items[index];
			
            if (!cb.IsMoving)
            {
                this.ColumnLayout.OnColumnRemoved(cb);
            }

			if (this.ColumnLayout.Grid != null)
				this.ColumnLayout.Grid.InvalidateScrollPanel(true);

			return base.RemoveItem(index);
		}
		#endregion // RemoveItem

        #region ResetItems
        /// <summary>
		/// Removes all <see cref="ColumnBase"/> objects from the <see cref="ColumnBaseCollection"/>.
		/// </summary>
		protected override void ResetItems()
		{
            ReadOnlyKeyedColumnBaseCollection<ColumnBase> allCols = this.AllColumns;            

			base.ResetItems();

            foreach (ColumnBase col in allCols)
            {
                this.ColumnLayout.OnColumnRemoved(col);
            }

			this.InvalidateSelection(this.ColumnLayout);

			if (this.GroupByColumns != null)
				this.GroupByColumns.Clear();
			this.FixedColumnsLeft.Clear();
			this.FixedColumnsRight.Clear();
			this.SortedColumns.Clear();

            this._internalColumnLayouts.Clear();
			this._nonLayoutColumns.Clear();
			this._internalVisibleColumns.Clear();

			if (this.ColumnLayout.Grid != null)
				this.ColumnLayout.Grid.InvalidateScrollPanel(true);

		}
        #endregion // ResetItems

        #region AddItemSilently
        /// <summary>
		/// Adds the item at the specified index, without triggering any events. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void AddItemSilently(int index, ColumnBase item)
		{
			base.AddItemSilently(index, item);
			this.InvalidateColumnsCollections(true);
		}
		#endregion // AddItemSilently

		#region RemoveItemSilently

		/// <summary>
		/// Removes the item at the specified index, without triggering any events. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		protected override bool RemoveItemSilently(int index)
		{
			ColumnBase colBase = this.Items[index];

			bool val = base.RemoveItemSilently(index);

			// If false, it means we're moving a column in the UI...it will be added back, 
			// so don't undo anything..like selection or sorting
			bool fullInvalidate = !colBase.IsMoving;

			if (fullInvalidate && this.ColumnLayout != null && this.ColumnLayout.Grid != null)
			{
				this.InvalidateSelection(colBase);
				this.InvalidateActiveCell(colBase);
			}

			this.InvalidateColumnsCollections(fullInvalidate);

			return val;
		}
		#endregion // RemoveItemSilently

		#endregion // Overrides        	

		#region IProvidePersistenceLookupKeys Members

		Collection<string> IProvidePersistenceLookupKeys.GetLookupKeys()
		{
			return this.GetLookupKeys();
		}

		bool IProvidePersistenceLookupKeys.CanRehydrate(Collection<string> lookupKeys)
		{
			return this.CanRehydrate(lookupKeys);
		}

		#endregion

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
        {
            get
            {
                return this.PropertiesToIgnore;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        List<string> IProvidePropertyPersistenceSettings.PriorityProperties
        {
            get { return this.PriorityProperties; }
        }

        void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
        {
            this.FinishedLoadingPersistence();
        }
        #endregion // PriorityProperties

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