using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A standard Cell object in the <see cref="XamGrid"/>.
	/// </summary>
	public class Cell : CellBase
	{
		#region Members

		bool _isSelected;
        bool _bindingError;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Cell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="RowBase"/> object that owns the <see cref="Cell"/></param>
		/// <param propertyName="column">The <see cref="Column"/> object that the <see cref="Cell"/> represents.</param>
		public Cell(RowBase row, Column column)
			: base(row, column)
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region IsSelected

		/// <summary>
		/// Gets/Sets whether an item is currently selected. 
		/// </summary>
		public override bool IsSelected
		{
			get
			{
				return this._isSelected;
			}
			set
			{
				if (this.Row.ColumnLayout != null && this.Column.SupportsActivationAndSelection)
				{
					bool oldValue = this._isSelected;

					if (value)
						this.Row.ColumnLayout.Grid.SelectCell(this, InvokeAction.Code);
					else
						this.Row.ColumnLayout.Grid.UnselectCell(this);

					// Raise a PropertyChanged event
					if (oldValue != value)
					{
						this.OnPropertyChanged("IsSelected");
					}

				}
			}
		}
		#endregion // IsSelected

		#region IsEditing

		/// <summary>
		/// Gets whether this <see cref="Cell"/> is currently in edit mode.
		/// </summary>
		public bool IsEditing
		{
			get
			{
				bool isEditing = false;
				if (this.Row != null)
				{
					ColumnLayout colLayout = this.Row.ColumnLayout;
					isEditing = (colLayout != null && colLayout.Grid != null && colLayout.Grid.CurrentEditCell == this);
				}
				return isEditing;
			}
		}
		#endregion // IsEditing

		#region EditorStyle
		/// <summary>
		/// Gets / sets the style that will used on the editor of the Cell.
		/// </summary>
		public Style EditorStyle
		{
			get;
			set;
		}
		#endregion // EditorStyle

		#region EditorStyleResolved
		/// <summary>
		/// Resovles the Style that will be applied to the editor for the cell.
		/// </summary>
		public Style EditorStyleResolved
		{
			get
			{
                if (this.EditorStyle == null)
                {
                    EditableColumn col = this.Column as EditableColumn;
                    if(col != null)
                        return col.EditorStyle;
                }

				return this.EditorStyle;
			}
		}
		#endregion // EditorStyleResolved

		#endregion // Public

		#region Protected

		/// <summary>
		/// Resolves the underlying <see cref="Column"/> object that this <see cref="Cell"/> represents
		/// </summary>
		protected internal virtual Column ResolveColumn
		{
			get
			{
				return this.Column;
			}
		}

        #region Value

        /// <summary>
        /// Gets the the underlying value that the cell represents. 
        /// Note: in order to retrieve the cell's value we use a binding since we don't know about the underlying object. 
        /// The most performant way to retrieve the cell's value is to grab the row's Data (this.Cell.Row.Data), 
        /// cast it as your object and grab the actual value manually. 
        /// </summary>
        /// <remarks>
        /// Unlike Value this does not apply the ValueConverter to the value before returning it.
        /// </remarks>
        internal override object RawValue
        {
            get
            {
                object val = null;
                if (this.Row.Data != null && this.Column.Key != null)
                {
                    CellValueObject cellValueObj = new CellValueObject();
                    Binding b = ColumnContentProviderBase.ResolveBindingInternal(this);
                    if (b != null)
                    {
                        b.Mode = BindingMode.OneTime;

                        b.Source = this.Row.Data;

                        // Reset the Converters, otherwise, we'll raise the CellControlAttached event.
                        b.Converter = null;
                        b.ConverterParameter = null;

                        cellValueObj.SetBinding(CellValueObject.ValueProperty, b);
                        val = cellValueObj.Value;
                    }
                }
                return val;
            }
        }

        #endregion // Value



		#endregion // Protected

		#endregion // Properties

		#region Methods

		#region Private

		#region ResolveRow

		internal static int ResolveRowIndex(RowBase row, bool usePartialCount)
		{
			XamGrid grid = row.ColumnLayout.Grid;
			
			int rowIndex = -1;
			if ((grid.Panel.FixedRowsTop.IndexOf(row) == -1 && grid.Panel.FixedRowsBottom.IndexOf(row) == -1))
			{
				rowIndex = (usePartialCount) ? row.Manager.ResolveIndexForRow(row) : grid.InternalRows.IndexOf(row);
			}

			if (row.Level == 0)
			{
				if (rowIndex > -1)
				{
					rowIndex += grid.Panel.FixedRowsTop.Count;
				}
				else
				{
					rowIndex = grid.Panel.FixedRowsTop.IndexOf(row);

					if (rowIndex < 0)
					{
						int bottomRowHighestIndex = grid.Panel.FixedRowsBottom.Count - 1;
						rowIndex = (bottomRowHighestIndex - grid.Panel.FixedRowsBottom.IndexOf(row)) + grid.Panel.FixedRowsTop.Count + ((usePartialCount) ? row.Manager.FullRowCount : grid.InternalRows.Count);
					}
				}
			}
			return rowIndex;
		}

		internal static RowBase ResolveRow(RowBase currentRow, int index, bool increase, KeyboardNavigation navigation)
		{
			RowBase row = null;
			XamGrid grid = currentRow.ColumnLayout.Grid;
			int fixedRowsTopCount = 0;
			int fixedRowsBottomCount = 0;
			if (currentRow.Level == 0)
			{
				fixedRowsTopCount = grid.Panel.FixedRowsTop.Count;
				fixedRowsBottomCount = grid.Panel.FixedRowsBottom.Count;
			}

			int internalRowsCount = -1;
			if (navigation == KeyboardNavigation.CurrentLayout)
			{
				internalRowsCount = currentRow.Manager.FullRowCount;
			}
			else
			{
				internalRowsCount = grid.InternalRows.Count;
			}

			if (index < fixedRowsTopCount)
			{
				if (index > -1 && index < fixedRowsTopCount)
				{
					row = grid.Panel.FixedRowsTop[index];

					while (row != null && !(row is ChildBand) && !row.AllowKeyboardNavigation)
					{
						if (increase)
							index++;
						else
							index--;

                        if (index > -1 && index < fixedRowsTopCount)
                            row = grid.Panel.FixedRowsTop[index];
                        else
                        {
                            currentRow = row;
                            row = null;
                        }
					}
				}
				if (row == null && index >= fixedRowsTopCount && increase)
				{
					row = ResolveRow(currentRow, index, increase, navigation);
				}
			}
			else if (index >= fixedRowsTopCount + internalRowsCount)
			{
				index = (index - (fixedRowsTopCount + internalRowsCount));

				if (index > -1 && index < fixedRowsBottomCount)
				{
					row = grid.Panel.FixedRowsBottom[fixedRowsBottomCount - 1 - index];

					while (row != null && !(row is Row) && !(row is ChildBand) && !row.AllowKeyboardNavigation)
					{
						if (increase)
							index++;
						else
							index--;

						if (index > -1 && index < fixedRowsBottomCount)
							row = grid.Panel.FixedRowsBottom[index];
						else
							row = null;
					}
				}
				if (row == null && (index < fixedRowsTopCount + internalRowsCount) && !increase)
				{
					row = ResolveRow(currentRow, index, increase, navigation);
				}
			}
			else
			{
				index -= fixedRowsTopCount;

				if (navigation == KeyboardNavigation.CurrentLayout)
				{
					row = currentRow.Manager.ResolveRowForIndex(index);
				}
				else
				{
					row = grid.InternalRows[index];
				}

				while (row != null && (((!(row is Row) && !(row is ChildBand)) && (!row.AllowKeyboardNavigation || row.VisibleCells.Count == 0)) || ((row is Row) && row.VisibleCells.Count == 0 )))
				{
					if (increase)
						index++;
					else
						index--;

					if (navigation == KeyboardNavigation.CurrentLayout)
					{
						row = currentRow.Manager.ResolveRowForIndex(index);
					}
					else
					{
						row = grid.InternalRows[index];
					}
				}
				if (row == null)
					row = ResolveRow(currentRow, index + fixedRowsTopCount, increase, navigation);
			}
			return row;
		}

		#endregion // ResolveRow

		#region ResolveEndTargetCell

		private RowBase ResolveEndTargetCell(RowsManager manager, RowBase row)
		{
			int rowsCount = manager.Rows.Count;
			int registeredTopRowsCount = manager.RegisteredTopRows.Count;
			int registeredBottomRowsCount = manager.RegisteredBottomRows.Count;
			RowBase lastRow = null;

			// determine what the last keyboard accessable row index is
			int lastIndex = -1;
			int indexInRowsCollection = manager.Rows.IndexOf(row);
			if (indexInRowsCollection == (rowsCount - 1) ||
				(manager.RegisteredBottomRows.IndexOf(row) > -1) ||
				(rowsCount == 0 && manager.RegisteredTopRows.IndexOf(row) > -1))
			{
				for (int i = registeredBottomRowsCount - 1; i >= 0; i--)
				{
					RowBase tempRow = manager.RegisteredBottomRows[i];
					if (manager.RegisteredBottomRows[i].AllowKeyboardNavigation)
					{
						lastIndex = registeredTopRowsCount + rowsCount + i;
						lastRow = tempRow;
						break;
					}
				}
			}

			if (lastIndex == -1 && rowsCount > 0)
			{
				lastIndex = rowsCount - 1;
				lastRow = manager.Rows[lastIndex];
				lastIndex += registeredTopRowsCount;
			}
			if (lastIndex == -1)
			{
				for (int i = registeredTopRowsCount - 1; i >= 0; i--)
				{
					RowBase tempRow = manager.RegisteredTopRows[i];
					if (tempRow.AllowKeyboardNavigation)
					{
						lastIndex = i;
						lastRow = tempRow;
						break;
					}
				}
			}

			// now are we on the last row?
			int currentIndex = manager.RegisteredTopRows.IndexOf(row);
			if (currentIndex == -1)
			{
				currentIndex = manager.Rows.IndexOf(row);
				if (currentIndex == -1)
				{
					currentIndex = manager.RegisteredBottomRows.IndexOf(row);
					if (currentIndex == -1)
						throw new InvalidRowIndexException();
					currentIndex += rowsCount;
				}
				currentIndex += registeredTopRowsCount;
			}

			if (currentIndex == lastIndex)
			{
				if (manager.Level != 0)
				{
					RowBase cb = manager.ParentRow as RowBase;
					Row parentRow = cb.Manager.ParentRow as Row;
					manager = parentRow.Manager as RowsManager;
					lastRow = ResolveEndTargetCell(manager, parentRow);
				}
			}
			return lastRow;
		}

		#endregion // ResolveEndTargetCell

		#region ResolveHomeTargetCell
		private RowBase ResolveHomeTargetCell(RowsManager manager, RowBase row)
		{
			return ResolveHomeTargetCell(manager, row, false);
		}

		private RowBase ResolveHomeTargetCell(RowsManager manager, RowBase row, bool fromLowerLevel)
		{
			int rowsCount = manager.Rows.Count;
			int registeredTopRowsCount = manager.RegisteredTopRows.Count;
			int registeredBottomRowsCount = manager.RegisteredBottomRows.Count;
			RowBase firstRow = null;

			// determine what the first keyboard accessable row index is
			int firstIndex = -1;
			int indexInRowsCollection = manager.Rows.IndexOf(row);
			if ((indexInRowsCollection == 0 && !fromLowerLevel) ||
				(manager.RegisteredTopRows.IndexOf(row) > -1) ||
				(rowsCount == 0 && manager.RegisteredBottomRows.IndexOf(row) > -1))
			{
				for (int i = 0; i < registeredTopRowsCount; i++)
				{
					RowBase tempRow = manager.RegisteredTopRows[i];
					if (tempRow.AllowKeyboardNavigation)
					{
						firstIndex = i;
						firstRow = tempRow;
						break;
					}
				}
			}
			if (firstIndex == -1 && rowsCount > 0)
			{
				firstIndex = 0;
				firstRow = manager.Rows[0];
				firstIndex += registeredTopRowsCount;
			}
			if (firstIndex == -1)
			{
				for (int i = 0; i < registeredBottomRowsCount; i++)
				{
					RowBase tempRow = manager.RegisteredBottomRows[i];
					if (manager.RegisteredBottomRows[i].AllowKeyboardNavigation)
					{
						firstIndex = registeredTopRowsCount + rowsCount + i;
						firstRow = tempRow;
						break;
					}
				}
			}

			// now are we on the first row?
			int currentIndex = manager.RegisteredTopRows.IndexOf(row);
			if (currentIndex == -1)
			{
				currentIndex = manager.Rows.IndexOf(row);
				if (currentIndex == -1)
				{
					currentIndex = manager.RegisteredBottomRows.IndexOf(row);
					if (currentIndex == -1)
						throw new InvalidRowIndexException();
					currentIndex += rowsCount;
				}
				currentIndex += registeredTopRowsCount;
			}

			if (currentIndex == firstIndex)
			{
				if (manager.Level != 0)
				{
					RowBase cb = manager.ParentRow as RowBase;
					Row parentRow = cb.Manager.ParentRow as Row;
					manager = parentRow.Manager as RowsManager;
					firstRow = ResolveHomeTargetCell(manager, parentRow, true);
				}
			}
			return firstRow;
		}
		#endregion // ResolveHomeTargetCell

		#endregion // Private

		#region Protected

		#region EnterEditMode

		/// <summary>
		/// Places the specified <see cref="Cell"/> into edit mode.
		/// </summary>
		protected internal virtual void EnterEditMode(bool cellIsEditing)
		{
			if (!this.IsEditable || this.IsEditing || this.Control == null)
				return;

			if (this.Row.ColumnLayout.Grid.ExitEditMode(false))
			{
				if (cellIsEditing)
					this.Row.ColumnLayout.Grid.CurrentEditCell = this;

				((CellControl)this.Control).AddEditorToControl();
			}
		}

		#endregion // EnterEditMode

		#region CreateCellValueBinding

		/// <summary>
		/// Creates the binding used by the CellValueObject for updating
		/// </summary>
		/// <returns></returns>
		protected virtual Binding CreateCellValueBinding(bool addValidation)
		{
			Binding b = new Binding(this.Column.Key);
			b.Source = this.Row.Data;
			b.Mode = BindingMode.TwoWay;
			b.ConverterCulture = CultureInfo.CurrentCulture;

            if (addValidation)
            {
                b.ValidatesOnDataErrors = b.ValidatesOnExceptions = b.NotifyOnValidationError = true;



            }
			return b;
		}

		#endregion // CreateCellValueBinding

		#region ExitEditMode

		/// <summary>
		/// Takes the specified <see cref="Cell"/> out of edit mode.
		/// </summary>
		/// <param propertyName="newValue">The value that should be entered in the <see cref="Cell"/></param>
		/// <param propertyName="editingCanceled">Whether or not we're exiting edit mode, because it was cancelled.</param>
        /// <param propertyName="evaluateBindings">Whether or not we should evaluate the cell's bindings.</param>
		protected internal virtual bool ExitEditMode(object newValue, bool editingCanceled, bool evaluateBindings)
		{
			CellControl control = (CellControl)this.Control;

            if (editingCanceled || !control.HasEditingBindings)
            {
                if (this.Column.ResetCellValueObjectAfterEditing)
                {
                    CellValueObject cellValueObj = new CellValueObject();
                    Binding b = CreateCellValueBinding(false);

                    EditableColumn editableColumn = this.Column as EditableColumn;
                    if (editableColumn != null)
                    {
                        // Need to take into account the EditorValueConverter so that it can properly convertBack.                 
                        b.Converter = editableColumn.EditorValueConverter;
                        b.ConverterParameter = editableColumn.EditorValueConverterParameter;
                    }

                    cellValueObj.SetBinding(CellValueObject.ValueProperty, b);
                    cellValueObj.Value = newValue;
                }
            }
            else if (evaluateBindings)
			{
				if (!control.EvaluateEditingBindings())
					return false;
			}

			control.RemoveEditorFromControl();

            Column column = this.Column;
            while (column.ParentColumn != null)
                column = column.ParentColumn;

            CellBase cell = this.Row.Cells[column];

			// While in edit mode, a cell isn't disposed of. 
			// Since thats the case, then when we aren't in edit mode anymore, we need to make sure
			// the cell gets released, so that it can be properly recycled. 
            if (this.Row.Control != null && (!this.Row.Control.VisibleCells.Contains(cell) && !this.Row.Control.VisibleFixedLeftCells.Contains(cell) && !this.Row.Control.VisibleFixedRightCells.Contains(cell)))
			{
                this.Row.Control.ReleaseCell(cell);
			}
			else
			{
                this.OnElementAttached(this.Control);
				this.EnsureCurrentState();
			}
			return true;
		}

		#endregion // ExitEditMode

        #region ResetDataValue

        /// <summary>
        /// Resets the value for a cell, and triggeres an evalution of it's bindings. 
        /// Called when editing has been canceled.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected internal bool ResetDataValue(object value)
        {
            return ResetDataValue(value, true);
        }

	    /// <summary>
	    /// Resets the value for a cell, and triggeres an evalution of it's bindings. 
	    /// Called when editing has been canceled.
	    /// </summary>
	    /// <param name="value"></param>
	    /// <param name="validateBinding"></param>
	    /// <returns></returns>
	    internal bool ResetDataValue(object value, bool validateBinding)
        {
            // If we aren't adhering to the ReadOnlyFlag, then there is a chance this will always fail, so don't bother doing validation
            if (this.Column.UseReadOnlyFlag)
            {
                CellControl control = (CellControl)this.Control;

                // Editing has been canceled. 
                // So now lets reset the value.
                // But we need to make sure the original cell value is valid too, b/c it could have been invalid from the start.
                CellValueObject cellValueObj = new CellValueObject();
                Binding b = CreateCellValueBinding(validateBinding);
                cellValueObj.SetBinding(CellValueObject.ValueProperty, b);

                this._bindingError = false;


                Validation.AddErrorHandler(cellValueObj, Element_BindingValidationError);
                BindingExpression expression = cellValueObj.GetBindingExpression(CellValueObject.ValueProperty);
                cellValueObj.Value = value;
                if (expression != null)
                {
                    expression.UpdateSource();
                }
                Validation.RemoveErrorHandler(cellValueObj, Element_BindingValidationError);


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


                if (this._bindingError)
                    control.EvaluateEditingBindings();

                return !this._bindingError;
            }

            return true;
        }
        #endregion // ResetDataValue

        #region CreateCellBindingConverter

        /// <summary>
		/// Creates the <see cref="IValueConverter"/> which will be attached to this <see cref="Cell"/>.
		/// </summary>
		/// <returns></returns>
		protected internal virtual IValueConverter CreateCellBindingConverter()
		{
			return new CellBindingConverter();
		}

		#endregion // CreateCellBindingConverter

        #region ResolveIsCellEditable

        /// <summary>
        /// Gets if the particular Cell has a field attribute that says it can edit.
        /// </summary>
        protected virtual bool ResolveIsCellEditable
        {
            get
            {
                return this.Column.DataField.AllowEdit;
            }
        }
        #endregion // ResolveIsCellEditable

        #endregion // Protected

        #region Internal

        #region ResolveValueFromCell

        internal object ResolveValueFromCell()
		{
			object obj = null;

			if (this.Control != null && this.Control.ContentProvider != null)
				obj = this.Control.ContentProvider.ResolveValueFromEditor(this);
			else
				obj = this.Value;

			return obj;
		}
		#endregion // ResolveValueFromCell

		#region RaiseCellControlAttachedEvent

		internal void RaiseCellControlAttachedEvent()
		{
            if (this.Row.ColumnLayout != null)
            {
                this.IsDirty = this.Row.ColumnLayout.Grid.OnCellControlAttached(this);
            }
		}

		#endregion // RaiseCellControlAttachedEvent


		#endregion // Internal

		#endregion // Methods

        #region EventHandlers

        void Element_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                this._bindingError = true;
        }

        #endregion // EventHandlers

        #region Overrides

        #region Properties

        #region RecyclingElementType

        /// <summary>
		/// Gets the Type of control that should be created for the <see cref="Cell"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return null;
			}
		}
		#endregion // RecyclingElementType

		#region RecyclingIdentifier

		/// <summary>
		/// If a <see cref="RecyclingElementType"/> isn't specified, this property can be used to offer another way of identifying 
		/// a reyclable element.
		/// </summary>
		protected override string RecyclingIdentifier
		{
            get
            {
                return this.Row.RowType.ToString() + "_" + this.Column.Key + "_" + this.Column.TypeName + "_" + this.Column.ColumnLayout.Key;
            }
		}
		#endregion // RecyclingIdentifier

		#region Value

		/// <summary>
		/// Gets the the underlying value that the cell represents. 
		/// Note: in order to retrieve the cell's value we use a binding since we don't know about the underlying object. 
		/// The most performant way to retrieve the cell's value is to grab the row's Data (this.Cell.Row.Data), 
		/// cast it as your object and grab the actual value manually. 
		/// </summary>
		public override object Value
		{
			get
			{
				object val = null;
				if (this.Row.Data != null && this.Column.Key != null)
				{
					CellValueObject cellValueObj = new CellValueObject();
                    Binding b = ColumnContentProviderBase.ResolveBindingInternal(this);
					if (b != null)
					{
						b.Mode = BindingMode.OneTime;

                        b.Source = this.Row.Data;
						
						// Reset the Converters, otherwise, we'll raise the CellControlAttached event.
						b.Converter = this.Column.ValueConverter;
						b.ConverterParameter = this.Column.ValueConverterParameter;

						cellValueObj.SetBinding(CellValueObject.ValueProperty, b);
						val = cellValueObj.Value;
					}
				}
				return val;
			}
		}

		#endregion // Value

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="CellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
            get
            {
                if (this.Style != null)
                    return this.Style;
                else
                {
                    Row r = this.Row as Row;

                    if (r != null)
                    {
                        if (r.CellStyle != null)
                            return r.CellStyle;
                    }
                    return this.Column.CellStyleResolved;
                }
            }
		}

		#endregion // ResolveStyle

		#region IsEditable

		/// <summary>
		/// Gets whether a particular <see cref="Cell"/> can enter edit mode.
		/// </summary>
		public override bool IsEditable
		{
			get
			{
				Column col = this.Column;

                bool val = this.ResolveIsCellEditable;

                if (!col.UseReadOnlyFlag)
                    return col.IsEditable;

                if (val && col.CachedPropertyReadOnly == null)
				{
					object data = this.Row.Data;

					if (col.Key != null && data != null)
					{
                        PropertyInfo property = DataManagerBase.ResolvePropertyInfoFromPropertyPath(this.Column.Key, data);

                        col.CachedPropertyReadOnly = (property == null) ? false : (property.GetSetMethod() == null);
					}
				}

                return val && col.IsEditable && (col.CachedPropertyReadOnly.HasValue && !col.CachedPropertyReadOnly.Value);
			}
		}
		#endregion // IsEditable

		#endregion // Properties

		#region Methods

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of a <see cref="CellControl"/> for the <see cref="Cell"/>.
		/// </summary>
		/// <returns>A new <see cref="CellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new CellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#region HandleKeyDown

		/// <summary>
		/// Should be handled by a derived class so that a cell can determine what to do with the given keyboard action.
		/// </summary>
		/// <param propertyName="key">The <see cref="Key"/> that was pressed.</param>
		/// <param propertyName="platformKey">The integer that represents the key pressed</param>
		/// <returns>True if the key is handled.</returns>
		protected internal override bool HandleKeyDown(Key key, int platformKey)
		{
			bool handled = false;

			ExpandableRowBase row = this.Row as ExpandableRowBase;
			XamGrid grid = this.Row.ColumnLayout.Grid;
			KeyboardNavigation mode = grid.KeyboardNavigation;
            bool exitEditMode = false;

			int rowIndex = Cell.ResolveRowIndex(row, (mode == KeyboardNavigation.CurrentLayout && (key == Key.Up || key == Key.Down)));

			int rowCount = (mode == KeyboardNavigation.CurrentLayout) ? row.Manager.FullRowCount : grid.InternalRows.Count;
			if (row.Level == 0)
			{
				rowCount += (grid.Panel.FixedRowsTop.Count + grid.Panel.FixedRowsBottom.Count);
			}

			Collection<CellBase> cells = row.VisibleCells;
			int cellIndex = cells.IndexOf(this);
			List<Column> columns = new List<Column>();

            if (grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
            {
                ReadOnlyCollection<Column> mergedCols = grid.GroupBySettings.GroupByColumns[this.Row.ColumnLayout];
                foreach (Column mergedCol in mergedCols)
                {
                    if (mergedCol.Visibility == Visibility.Visible)
                        columns.Add(mergedCol);
                }
            }

            foreach (Column fixedColumn in row.Columns.FixedColumnsLeft)
            {
                if(fixedColumn.Visibility == Visibility.Visible)
                {
                    if (!(fixedColumn is GroupColumn))
                    {
                        columns.Add(fixedColumn);
                    } 
                    else
                    {
                        columns.AddRange(fixedColumn.AllVisibleChildColumns);
                    }
                }
            }

            ReadOnlyCollection<Column> allCols = row.Columns.AllVisibleChildColumns;

            foreach (Column col in allCols)
            {
                if (!(col is FillerColumn) && col.SupportsActivationAndSelection && col.IsFixed == FixedState.NotFixed && !columns.Contains(col))
                {
                    columns.Add(col);
                }
            }

			FixedColumnsCollection right = row.Columns.FixedColumnsRight;
			int frcount = right.Count;
            for (int i = frcount - 1; i >= 0; i--)
            {
                if(right[i].Visibility == Visibility.Visible)
                {
                    if (!(right[i] is GroupColumn))
                    {
                        columns.Add(right[i]);
                    }
                    else
                    {
                        columns.AddRange(right[i].AllVisibleChildColumns);
                    }
                }
            }

			int columnIndex = columns.IndexOf(this.Column);
			if (columnIndex < 0)
				columnIndex = 0;

			if (platformKey == 187)
				key = Key.Add;
			else if (platformKey == 189)
				key = Key.Subtract;


            if (grid.FlowDirection == FlowDirection.RightToLeft)
            {
                if (key == Key.Left)
                    key = Key.Right;
                else if (key == Key.Right)
                    key = Key.Left;
            }



            if (PlatformProxy.GetTabNavigation(grid) != KeyboardNavigationMode.Once && key == Key.Tab)
			{
				bool shiftKey = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
				if (shiftKey)
					key = Key.Left;
				else
					key = Key.Right;
			}

			CellBase activeCell = null;

			EditingSettingsBaseOverride settings = this.EditingSettings;
			EditingType editingType = settings.ResolveEditingType();

			switch (key)
			{
				case Key.Down:
					if (rowIndex < rowCount - 1)
					{
						RowBase newRow = Cell.ResolveRow(this.Row, rowIndex + 1, true, mode);
						if (newRow != null)
						{
							Collection<CellBase> newRowCells = newRow.VisibleCells;
							if (columnIndex >= newRowCells.Count)
								columnIndex = newRowCells.Count - 1;
							activeCell = newRowCells[columnIndex];
						}
					}
					break;

				case Key.Up:
					if (rowIndex > 0)
					{
						RowBase newRow = Cell.ResolveRow(this.Row, rowIndex - 1, false, mode);
						if (newRow != null)
						{
							Collection<CellBase> newRowCells = newRow.VisibleCells;
							if (columnIndex >= newRowCells.Count)
								columnIndex = newRowCells.Count - 1;
							activeCell = newRowCells[columnIndex];
						}
					}
					break;

				case Key.Right:
					if (cellIndex < cells.Count - 1)
						activeCell = cells[cellIndex + 1];
					else
					{
						if (grid.KeyboardNavigation == KeyboardNavigation.CurrentLayout)
						{
							RowBase newRow = Cell.ResolveRow(this.Row, rowIndex + 1, true, KeyboardNavigation.AllLayouts);
                            if (newRow != null)
                                activeCell = newRow.VisibleCells[0];
                            else
                                exitEditMode = true;
						}
						else
						{
							if (rowIndex < rowCount - 1)
							{
								RowBase newRow = Cell.ResolveRow(this.Row, rowIndex + 1, true, mode);
								if (newRow != null)
									activeCell = newRow.VisibleCells[0];
                                else
                                    exitEditMode = true;
							}
						}
					}
					break;

				case Key.Left:
					if (cellIndex > 0)
						activeCell = cells[cellIndex - 1];
					else
					{
						if (grid.KeyboardNavigation == KeyboardNavigation.CurrentLayout)
						{
							RowBase newRow = Cell.ResolveRow(this.Row, rowIndex - 1, false, KeyboardNavigation.AllLayouts);
							if (newRow != null)
							{
								Collection<CellBase> newCells = newRow.VisibleCells;
								activeCell = newCells[newCells.Count - 1];
							}
                            else
                                exitEditMode = true;
						}
						else
						{
							if (rowIndex > 0)
							{
								RowBase newRow = Cell.ResolveRow(this.Row, rowIndex - 1, false, mode);
								if (newRow != null)
								{
									Collection<CellBase> newCells = newRow.VisibleCells;
									activeCell = newCells[newCells.Count - 1];
								}
                                else
                                    exitEditMode = true;
							}
						}
					}
					break;

				case Key.Home:

					if (cellIndex != 0)
						activeCell = cells[0];
					else
					{
						RowsManager manager = row.Manager as RowsManager;
						if (manager != null)
						{
							RowBase firstRow = ResolveHomeTargetCell(manager, row);
							if (firstRow != null)
							{
								Collection<CellBase> parentRowCells = firstRow.VisibleCells;
								if (parentRowCells.Count > 0)
									activeCell = parentRowCells[0];
							}
						}
						else
						{
							Row parentRow = row.Manager.ParentRow as Row;
							manager = parentRow.Manager as RowsManager;
							RowBase lastRow = ResolveHomeTargetCell(manager, parentRow);

							Collection<CellBase> parentRowCells = lastRow.VisibleCells;
							if (parentRowCells.Count > 0)
								activeCell = parentRowCells[0];
						}
					}
					break;

				case Key.End:

					if (cellIndex != cells.Count - 1)
						activeCell = cells[cells.Count - 1];
					else
					{
						RowsManager manager = row.Manager as RowsManager;
						if (manager != null)
						{
							RowBase lastRow = ResolveEndTargetCell(manager, row);
							if (lastRow != null)
							{
								Collection<CellBase> parentRowCells = lastRow.VisibleCells;
								if (parentRowCells.Count > 0)
									activeCell = parentRowCells[parentRowCells.Count - 1];
							}
						}
						else
						{
							Row parentRow = row.Manager.ParentRow as Row;
							manager = parentRow.Manager as RowsManager;
							RowBase lastRow = ResolveEndTargetCell(manager, parentRow);

							Collection<CellBase> parentRowCells = lastRow.VisibleCells;
							if (parentRowCells.Count > 0)
								activeCell = parentRowCells[parentRowCells.Count - 1];
						}
					}

					break;

				case Key.Add:

					if (row.HasChildren && !this.IsEditing)
					{
						row.IsExpanded = true;
						handled = true;
					}

					break;

				case Key.Subtract:

					if (row.HasChildren && !this.IsEditing)
					{
						handled = true;
						row.IsExpanded = false;
					}

					break;

				case Key.Space:

					if (!this.IsEditing)
					{
						grid.SelectCell(this, InvokeAction.Click);
					}
					break;
				case Key.F2:

					if (!this.IsEditing && settings.IsF2EditingEnabledResolved && editingType != EditingType.None)
					{
						if (editingType == EditingType.Cell)
							grid.EnterEditMode(this);
						else
							grid.EnterEditMode((Row)this.Row, this);

						handled = true;
					}
					break;

				case Key.Enter:

					if (this.IsEditing)
					{
						grid.ExitEditMode(false);
						handled = true;
					}
					else if (!this.IsEditing && settings.IsEnterKeyEditingEnabledResolved && editingType != EditingType.None)
					{
						if (editingType == EditingType.Cell)
							grid.EnterEditMode(this);
						else
							grid.EnterEditMode((Row)this.Row, this);
						handled = true;
					}
					break;
				case Key.Escape:

					if (this.IsEditing)
					{
						grid.ExitEditMode(true);
						handled = true;
					}

					break;
			}

            if (activeCell != null)
            {
                grid.SetActiveCell(activeCell, CellAlignment.NotSet, InvokeAction.Keyboard);
                handled = true;
            }
            else if (exitEditMode)
            {
                if (this.IsEditing)
                {
                    grid.ExitEditMode(false);
                }
            }

			return handled;
		}

		#endregion // HandleKeyDown

		#region EnsureCurrentState

		/// <summary>
		/// Ensures that <see cref="Cell"/> is in the correct state.
		/// </summary>
		protected internal override void EnsureCurrentState()
		{
			if (this.Control != null)
			{
                Row r = this.Row as Row;
				Column col = this.Column;
                bool isSelected = false;

 				if (r != null)
				{
                    if ((col != null && col.IsSelected) || r.IsSelected)
                    {
                        // We always want Active To be applied after Selected, so always set it to InActive first, 
                        // but make sure NotSelected is set before that, so we don't loose any styles that are being set by both active and selected.
                        this.Control.GoToState("NotSelected", false);
                        this.Control.GoToState("InActive", false);
                        this.Control.GoToState("Selected", false);
                        isSelected = true;
                    }
                    else
                    {
                        if (this.IsSelected)
                        {
                            // We always want Active To be applied after Selected, so always set it to InActive first. 
                            this.Control.GoToState("NotSelected", false);
                            this.Control.GoToState("InActive", false);
                            this.Control.GoToState("Selected", false);
                            isSelected = true;
                        }
                        else
                            this.Control.GoToState("NotSelected", false);
                    }
				}
                else
                    this.Control.GoToState(this.NormalState, false);

                if (this.Row != null)
                {
                    if (this.Row.ColumnLayout == null)
                    {
                        return;
                    }

                    // Common States				
                    if (this.Row.IsMouseOver && this.Row.ResolveRowHover != RowHoverType.None && (this.Row.ResolveRowHover == RowHoverType.Row || this == this.Row.ColumnLayout.Grid.MouseOverCell))
                        this.Control.GoToState(this.MouseOverState, true);
                    else
                        this.Control.GoToState(this.NormalState, false);

                    // Active States
                    if (this == this.Row.ColumnLayout.Grid.ActiveCell)
                        this.Control.GoToState("Active", false);
                    else if (!isSelected)
                        this.Control.GoToState("InActive", false);
                }

				if (col != null)
				{
					if (!(col.IsFixed == FixedState.NotFixed))
						this.Control.GoToState("Fixed", false);
                    else
						this.Control.GoToState("Unfixed", false);
				}

				XamGrid grid = this.Row.ColumnLayout.Grid;

				if (grid.CurrentEditRow == this.Row || (grid.CurrentEditCell == this))
					this.Control.GoToState("Editing", false);
				else
					this.Control.GoToState("NotEditing", false);
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
			if (!(this.Column is FillerColumn))
			{
				XamGrid grid = this.Row.ColumnLayout.Grid;

				if (!this.Control.IsResizing)
				{
					grid.SetActiveCell(this, CellAlignment.NotSet, InvokeAction.Click, false);

					// Need to Handle this, otherwise, if we're in a scroll viewer, we can loose focus.
					((MouseButtonEventArgs)e).Handled = true;
				}

				if (grid.SelectCell(this, InvokeAction.Click))
					return DragSelectType.None;

				return DragSelectType.Cell;
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

			if (!(this.Column is FillerColumn))
			{
				if (type == DragSelectType.Row)
					this.Row.ColumnLayout.Grid.SelectRow(this.Row as Row, InvokeAction.MouseMove);
				else
					this.Row.ColumnLayout.Grid.SelectCell(this, InvokeAction.MouseMove);
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
			this.Row.ColumnLayout.Grid.OnCellClicked(this);

			ColumnLayout layout = this.Row.ColumnLayout;

			EditingSettingsBaseOverride settings = this.EditingSettings;

			if (settings.IsMouseActionEditingEnabledResolved == MouseEditingAction.SingleClick)
			{
				Row r = this.Row as Row;
				if (r != null)
				{
					EditingType editingType = settings.ResolveEditingType();

                    //If the Column is a Custom Display Editable Column, and it's behavior is set to Always, we don't want to enter edit mode again.
                    bool preventEnterEditMode = false;

                    var customDisplayColumn = this.Column as CustomDisplayEditableColumn;

                    if ((customDisplayColumn != null) && (this.EnableCustomEditorBehaviors && (customDisplayColumn.EditorDisplayBehavior == EditorDisplayBehaviors.Always)))
                        preventEnterEditMode = true;

                    if (!preventEnterEditMode)
                    {
                        if (editingType == EditingType.Cell)
                            layout.Grid.EnterEditMode(this);
                        else if (editingType == EditingType.Row)
                            layout.Grid.EnterEditMode(r, this);
                    }
				}
			}
		}

		#endregion // OnCellClick

		#region OnCellDoubleClick

		/// <summary>
		/// Invoked when a cell is double clicked.
		/// </summary>
		protected internal override void OnCellDoubleClick()
		{
			this.Row.ColumnLayout.Grid.OnCellDoubleClicked(this);

			ColumnLayout layout = this.Row.ColumnLayout;

			EditingSettingsBaseOverride settings = this.EditingSettings;

			if (settings.IsMouseActionEditingEnabledResolved == MouseEditingAction.DoubleClick)
			{
				Row r = this.Row as Row;
				
				if (r != null)
				{
                    if (this.Control != null && this.Control.AllowUserResizing)
                    {
                        return;
                    }

					EditingType editingType = settings.ResolveEditingType();
					
					if (editingType == EditingType.Cell)
					{
						layout.Grid.EnterEditMode(this);
					}
					else if (editingType == EditingType.Row)
					{
						layout.Grid.EnterEditMode(r, this);
					}
				}
			}
		}

		#endregion // OnCellDoubleClick

		#region SetSelected
		/// <summary>
		/// Sets the selected state of an item. 
		/// </summary>
		/// <param propertyName="isSelected"></param>
		protected internal override void SetSelected(bool isSelected)
		{
			this._isSelected = isSelected;

			if (this.Row.ColumnLayout != null)
				this.Row.ColumnLayout.Grid.InvalidateScrollPanel(false);
		}
		#endregion // SetSelected

		#region OnElementReleasing

		/// <summary>
		/// Invoked when a <see cref="FrameworkElement"/> is being released from an object.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <returns>False, if the element shouldn't be released.</returns>
		protected override bool OnElementReleasing(CellControlBase element)
		{
            if (this.Row.ColumnLayout == null || this.Row.ColumnLayout.Grid == null)
                return true;

			return !(this.IsEditing || (this.IsEditable && this.Row == this.Row.ColumnLayout.Grid.CurrentEditRow));
		}

		#endregion // OnElementReleasing

		#endregion // Methods

		#endregion // Overrides

		#region CellValueObject Class

		/// <summary>
		/// A Class used to store off the value of a Cell.
		/// </summary>
		internal class CellValueObject : FrameworkElement
		{
			#region Value

			/// <summary>
			/// Identifies the <see cref="Value"/> dependency property. 
			/// </summary>
			public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(CellValueObject), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

			public object Value
			{
				get { return (object)this.GetValue(ValueProperty); }
				set { this.SetValue(ValueProperty, value); }
			}

			private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
			{
                
			}

			#endregion // Value
        }

		#endregion // CellValueObject

		#region CellEditingBindingConverter

		internal class CellEditingBindingConverter : IValueConverter
		{
			#region IValueConverter Members

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				Cell cell = (Cell)parameter;
				EditableColumn column = cell.ResolveColumn as EditableColumn;

                if (column != null)
                {
                    if (column.EditorValueConverter != null)
                        return column.EditorValueConverter.Convert(value, targetType, column.EditorValueConverterParameter, culture);

                    if (column.DataField.ApplyFormatStringInEditMode)
                        return cell.Control.ContentProvider.ApplyFormatting(value, column, culture);
                }

				return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				Cell cell = (Cell)parameter;
				EditableColumn column = cell.ResolveColumn as EditableColumn;

				if (column != null && column.EditorValueConverter != null)
					value = column.EditorValueConverter.ConvertBack(value, targetType, column.EditorValueConverterParameter, culture);

				if (targetType != null && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					if (value == null || !targetType.IsAssignableFrom(value.GetType()))
					{
						string str = value as string;
						if (string.IsNullOrEmpty(str))
						{
							value = null;
						}
					}
				}

                if (column != null && column.DataField.ConvertEmptyStringToNull)
                {
                    string val = value as string;
                    if (val != null && val.Length == 0)
                        value = null;
                }

				return value;
			}

			#endregion
		}
		#endregion // CellEditingBindingConverter

		#region CellBindingConverter

		internal class CellBindingConverter : IValueConverter
		{
			#region IValueConverter Members

			public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
                CellControlBase ctrl = (CellControlBase)parameter;
                Cell cell = (Cell)ctrl.Cell;
                if (cell != null)
                {
                    bool noCntrl = (cell.Control == null);
                    if (noCntrl)
                        cell.Control = ctrl;

                    cell.RaiseCellControlAttachedEvent();

                    Column column = cell.ResolveColumn;

                    if (column.ValueConverter != null)
                        value = column.ValueConverter.Convert(value, targetType, column.ValueConverterParameter, culture);
                    else if (ctrl.ContentProvider != null)
                        value = ctrl.ContentProvider.ApplyFormatting(value, column, culture);

                    if (value == null && !string.IsNullOrEmpty(column.DataField.NullDisplayText))
                        value = column.DataField.NullDisplayText;

                    if (noCntrl)
                        cell.Control = null;
                }

				return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
                CellControlBase ctrl = (CellControlBase)parameter;
                Cell cell = (Cell)ctrl.Cell;
                if (cell != null)
                {
                    Column column = cell.ResolveColumn;

                    if (column.ValueConverter != null)
                        value = column.ValueConverter.ConvertBack(value, targetType, column.ValueConverterParameter, culture);

                    if (column.DataField.ConvertEmptyStringToNull)
                    {
                        string val = value as string;
                        if (val != null && val.Length == 0)
                            value = null;
                    }
                }

				return value;
			}

			#endregion
		}
		#endregion // CellBindingConverter
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