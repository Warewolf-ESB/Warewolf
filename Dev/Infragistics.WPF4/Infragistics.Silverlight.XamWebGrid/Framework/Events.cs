using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infragistics.Controls.Grids.Primitives;
using System.Globalization;
using System.Windows.Data;

namespace Infragistics.Controls.Grids
{
	internal delegate void EmptyDelegate();

	#region SortingCancellableEventArgs
	/// <summary>
	/// A class listing the information needed during the <see cref="XamGrid.ColumnSorting"/> event.
	/// </summary>
	public class SortingCancellableEventArgs : CancellableColumnEventArgs
	{
		/// <summary>
		/// Gets the <see cref="SortDirection"/> prior to the change being made.
		/// </summary>
		public SortDirection PreviousSortDirection
		{
			get;
			protected internal set;
		}
		/// <summary>
		/// Gets the <see cref="SortDirection"/> as it will be applied.
		/// </summary>
		public SortDirection NewSortDirection
		{
			get;
			protected internal set;
		}

	}
	#endregion // SortingCancellableEventArgs

	#region SortedColumnEventArgs
	/// <summary>
	/// A class listing the <see cref="Column"/> that corresponds to the sorting event that was fired.
	/// </summary>
	public class SortedColumnEventArgs : ColumnEventArgs
	{
		/// <summary>
		/// Gets the <see cref="SortDirection"/>that was previously applied.
		/// </summary>
		public SortDirection PreviousSortDirection
		{
			get;
			internal set;
		}
	}
	#endregion // SortedColumnEventArgs

	#region ActiveCellChangingEventArgs

	/// <summary>
	/// A class listing the information needed during the <see cref="XamGrid.ActiveCellChanging"/> event. 
	/// </summary>
	public class ActiveCellChangingEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// Gets the Cell that was previously Active.  
		/// </summary>
		public CellBase PreviousActiveCell { get; protected internal set; }

		/// <summary>
		/// Gets the Cell that is about to become the ActiveCell. 
		/// </summary>
		public CellBase NewActiveCell { get; protected internal set; }
	}

	#endregion // ActiveCellChangingEventArgs

	#region SelectionCollectionChangedEventArgs

	/// <summary>
	/// A class listing the information needed during selection collection events of the <see cref="XamGrid"/>.
	/// </summary>
	/// <typeparam propertyName="T"></typeparam>
	public class SelectionCollectionChangedEventArgs<T> : EventArgs where T : IEnumerable
	{
		/// <summary>
		/// A collection of items that were previously selected. 
		/// </summary>
		public T PreviouslySelectedItems { get; protected internal set; }

		/// <summary>
		/// A collection of items that are currently selected.
		/// </summary>
		public T NewSelectedItems { get; protected internal set; }
	}

	#endregion // SelectionCollectionChangedEventArgs

	#region CellControlAttachedEventArgs

	/// <summary>
	/// A class listing the information needed during the <see cref="XamGrid.CellControlAttached"/> 
	/// </summary>
	public class CellControlAttachedEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Cell"/> object that was just recently made visible.
		/// </summary>
		public Cell Cell
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets/Sets whether the <see cref="CellControl"/> for the <see cref="Cell"/> should be disposed of when it scrolls out of view.
		/// </summary>
		/// <remarks>
		/// This flag must be set to true when changing a property on the <see cref="CellControl"/> object. Otherwise it will 
		/// be recycled and the properties that were applied to the control will be applied to another cell that you may 
		/// not have wanted the changes to be applied to. 
		/// </remarks>
		public bool IsDirty
		{
			get;
			set;
		}
	}

	#endregion // CellControlAttachedEventArgs

	#region InitializeRowEventArgs

	/// <summary>
	/// A class listing the information needed during the <see cref="XamGrid.InitializeRow"/> 
	/// </summary>
	public class InitializeRowEventArgs : RowEventArgs
	{
	}

	#endregion // InitializeRowEventArgs

	#region RowSelectorClickedEventArgs

	/// <summary>
	/// A class listing the information needed during the <see cref="XamGrid.RowSelectorClicked"/> 
	/// </summary>
	public class RowSelectorClickedEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Row"/> whose RowSelector has been clicked.
		/// </summary>
		public RowBase Row
		{
			get;
			internal set;
		}
	}

	#endregion // RowSelectorClickedEventArgs

	#region CellClickedEventArgs

	/// <summary>
	/// A class listing the information needed when the mouse clicks on a <see cref="Cell"/> 
	/// </summary>
	public class CellClickedEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Cell"/> that was clicked.
		/// </summary>
		public Cell Cell
		{
			get;
			internal set;
		}
	}

	#endregion // CellClickedEventArgs

	#region ColumnFixedStateEventArgs

	/// <summary>
	/// A class listing the <see cref="Column"/> that corresponds to the event that was fired.
	/// </summary>
	public class ColumnFixedStateEventArgs : ColumnEventArgs
	{
		/// <summary>
		/// Gets the old fixed state of the column.
		/// </summary>
		public FixedState PreviousFixedState
		{
			get;
			protected internal set;
		}
	}

	#endregion // ColumnFixedStateEventArgs

	#region CancellableColumnFixedStateEventArgs

	/// <summary>
	/// A class listing the <see cref="Column"/> that corresponds to the event that was fired.
	/// </summary>
	public class CancellableColumnFixedStateEventArgs : CancellableColumnEventArgs
	{
		/// <summary>
		/// Gets the new fixed state of the column.
		/// </summary>
		public FixedState FixedState
		{
			get;
			protected internal set;
		}
	}

	#endregion // CancellableColumnFixedStateEventArgs

	#region ColumnLayoutAssignedEventArgs

	/// <summary>
	/// A class listing the <see cref="ColumnLayout"/> that corresponds to the event that was fired.
	/// </summary>
	public class ColumnLayoutAssignedEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="ColumnLayout"/> that this event was triggered for.
		/// </summary>
		public ColumnLayout ColumnLayout
		{
			get;
			set;
		}

		/// <summary>
		/// The depth at which the <see cref="ColumnLayout"/> is representing. 
		/// </summary>
		/// <remarks>Level 0 is the root level.</remarks>
		public int Level
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the key that this <see cref="ColumnLayout"/> represetns.
		/// </summary>
		/// <remarks>A null key would be the root level.</remarks>
		public string Key
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the type of data that this <see cref="ColumnLayout"/> will represent.
		/// </summary>
		public Type DataType
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the collection of rows or row island that this particular <see cref="ColumnLayout"/> will represent.
		/// </summary>
		public RowCollection Rows
		{
			get;
			protected internal set;
		}
	}

	#endregion // ColumnLayoutAssignedEventArgs

	#region CancellableRowExpansionChangedEventArgs

	/// <summary>
	/// A class listing the information needed for expandable row events.
	/// </summary>
	public class CancellableRowExpansionChangedEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// The <see cref="ExpandableRowBase"/> who has been expanded or collapsed.
		/// </summary>
		public ExpandableRowBase Row
		{
			get;
			internal set;
		}
	}

	#endregion // CancellableRowExpansionChangedEventArgs

	#region RowExpansionChangedEventArgs

	/// <summary>
	/// A class listing the information needed for expandable row events.
	/// </summary>
	public class RowExpansionChangedEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="ExpandableRowBase"/> who has been expanded or collapsed.
		/// </summary>
		public ExpandableRowBase Row
		{
			get;
			internal set;
		}
	}

	#endregion // RowExpansionChangedEventArgs

	#region ColumnDragStartEventArgs

	/// <summary>
	/// A class listing the information needed for starting a column drag.
	/// </summary>
	public class ColumnDragStartEventArgs : CancellableColumnEventArgs
	{
		///// <summary>
		///// The <see cref="Column"/> being Dragged.
		///// </summary>
		//public Column Column
		//{
		//    get;
		//    internal set;
		//}
	}

	#endregion // ColumnDragStartEventArgs

	#region ColumnMovingEventArgs

	/// <summary>
	/// A class listing the information needed for moving a column.
	/// </summary>
	public class ColumnMovingEventArgs : ColumnEventArgs
	{
		///// <summary>
		///// The <see cref="Column"/> being Dragged.
		///// </summary>
		//public Column Column
		//{
		//    get;
		//    internal set;
		//}

		/// <summary>
		/// Gets a reference to the <see cref="HeaderCellControl"/> that is visually being dragged.
		/// </summary>
		public HeaderCellControl DraggingHeader
		{
			get;
			internal set;
		}

		/// <summary>
		/// The <see cref="MouseEventArgs"/> for the event.
		/// </summary>
		protected internal MouseEventArgs MouseArgs
		{
			get;
			set;
		}

		/// <summary>
		/// Returns the x- and y-coordinates of the mouse pointer position, relative to the specified <see cref="UIElement"/>.
		/// </summary>
		/// <param propertyName="relativeTo"></param>
		/// <returns></returns>
		public Point GetPosition(UIElement relativeTo)
		{
			Point p = new Point();
			if (this.MouseArgs != null)
				p = this.MouseArgs.GetPosition(relativeTo);

			return p;
		}
	}

	#endregion // ColumnMovingEventArgs

	#region ColumnDroppedEventArgs

	/// <summary>
	/// A class listing the information needed for dropping a Column
	/// </summary>
	public class ColumnDroppedEventArgs : ColumnEventArgs
	{
		/// <summary>
		/// The index of the column prior to the move.
		/// </summary>
		public int PreviousIndex
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// The index of the column that resulted because of the move.
		/// </summary>
		public int NewIndex
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// The type of operation that occured.
		/// </summary>
		/// <remarks>
		/// <para>ColumnMoved -	A column has been moved, look at the <see cref="PreviousIndex"/> and <see cref="NewIndex"/> properties.</para>
		/// <para>ColumnFixed - A column has been fixed, the <see cref="PreviousIndex"/> and <see cref="NewIndex"/> propreties don't mean anything.	</para>
		/// <para>FixColumnMoved - A fixed column has been moved, the <see cref="PreviousIndex"/> and <see cref="NewIndex"/> are in reference to the FixedColumns collection.</para>
		/// </remarks>
		public DropOperationType DropType
		{
			get;
			protected internal set;

		}
	}

	#endregion // ColumnDroppedEventArgs

	#region ColumnDragCanceledEventArgs

	/// <summary>
	/// A class listing the information needed when a drag operation is canceled. 
	/// </summary>
	public class ColumnDragCanceledEventArgs : ColumnEventArgs
	{
		///// <summary>
		///// The <see cref="Column"/> that was being Dragged.
		///// </summary>
		//public Column Column
		//{
		//    get;
		//    protected internal set;
		//}

		/// <summary>
		/// The type of operation that occured.
		/// </summary>
		public DragCancelType CancelType
		{
			get;
			protected internal set;

		}
	}

	#endregion // ColumnDragCanceledEventArgs

	#region ColumnDragEndedEventArgs

	/// <summary>
	/// A class listing the information needed when a drag operation is finished. 
	/// </summary>
	public class ColumnDragEndedEventArgs : ColumnEventArgs
	{
		///// <summary>
		///// The <see cref="Column"/> that was being Dragged.
		///// </summary>
		//public Column Column
		//{
		//    get;
		//    protected internal set;
		//}
	}

	#endregion // ColumnDragEndedEventArgs

	#region BeginEditingCellEventArgs

	/// <summary>
	/// Provides information needed during the <see cref="XamGrid.CellEnteringEditMode"/> event.
	/// </summary>
	public class BeginEditingCellEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// The <see cref="Cell"/> entering edit mode.
		/// </summary>
		public Cell Cell { get; protected internal set; }
	}

	#endregion // BeginEditingCellEventArgs

	#region EditingCellEventArgs

	/// <summary>
	/// Provides information to editing events.
	/// </summary>
	public class EditingCellEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Cell"/> that is currently being edited.
		/// </summary>
		public Cell Cell { get; protected internal set; }

		/// <summary>
		/// Gets the editor that is being displayed in the <see cref="Cell"/>
		/// </summary>
		public FrameworkElement Editor
		{
			get;
			protected internal set;
		}
	}

	#endregion // EditingCellEventArgs

	#region CellExitedEditingEventArgs

	/// <summary>
	/// Provides information to editing events.
	/// </summary>
	public class CellExitedEditingEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Cell"/> that is currently being edited.
		/// </summary>
		public Cell Cell { get; protected internal set; }

	}

	#endregion // CellExitedEditingEventArgs

	#region ExitEditingCellEventArgs

	/// <summary>
	/// Provides information needed during the <see cref="XamGrid.CellExitingEditMode"/> event.
	/// </summary>
	public class ExitEditingCellEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// The <see cref="Cell"/> exiting edit mode.
		/// </summary>
		public Cell Cell { get; protected internal set; }

		/// <summary>
		/// Gets/sets the value that will be set in the cell.
		/// </summary>
		/// <remarks>
		/// For some columns (like <see cref="TemplateColumn"/> and <see cref="UnboundColumn"/>) the new value cannot
		/// be predetermined becuase of the use of DataTemplates and user-defined bindings. In these cases
		/// the <see cref="NewValue"/> will be <c>null</c>.
		/// </remarks>
		public object NewValue { get; set; }

		/// <summary>
		/// Gets whether editing was canceled in the UI.
		/// </summary>
		/// <remarks>
		/// For example: the escape key was pressed.
		/// </remarks>
		public bool EditingCanceled { get; protected internal set; }

		/// <summary>
		/// Gets the editor that is being displayed in the <see cref="Cell"/>
		/// </summary>
		public FrameworkElement Editor
		{
			get;
			protected internal set;
		}
	}
	#endregion // ExitEditingCellEventArgs

	#region BeginEditingRowEventArgs

	/// <summary>
	/// Provides information needed during the <see cref="XamGrid.RowEnteringEditMode"/> event.
	/// </summary>
	public class BeginEditingRowEventArgs : CancellableRowEventArgs
	{
	}

	#endregion // BeginEditingRowEventArgs

	#region EditingRowEventArgs

	/// <summary>
	/// Provides information to editing events.
	/// </summary>
	public class EditingRowEventArgs : RowEventArgs
	{
	}

	#endregion // EditingRowEventArgs

	#region ExitEditingRowEventArgs

	/// <summary>
	/// Provides information needed during the <see cref="XamGrid.RowExitingEditMode"/> event.
	/// </summary>
	public class ExitEditingRowEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// The <see cref="Row"/> exiting edit mode.
		/// </summary>
		public Row Row { get; protected internal set; }

		/// <summary>
		/// Gets whether editing was canceled in the UI.
		/// </summary>
		/// <remarks>
		/// For example: the escape key was pressed.
		/// </remarks>
		public bool EditingCanceled { get; protected internal set; }

		/// <summary>
		/// A Dictionary of updated cell values.
		/// </summary>
		///<remarks>
		///To change a cell's value, use this Dictionary.
		///The lookup key for the dictionary is the key of the column of a cell. 
		///For example: e.Rows.Cells[0].Column.Key
		///</remarks>
		public Dictionary<string, object> NewCellValues { get; protected internal set; }

		/// <summary>
		/// A Dictionary of original cell values before editing began on the <see cref="Row"/>
		/// </summary>
		///<remarks>
		///The lookup key for the dictionary is the key of the column of a cell. 
		///For example: e.Rows.Cells[0].Column.Key
		///</remarks>
		public Dictionary<string, object> OriginalCellValues { get; protected internal set; }
	}
	#endregion // ExitEditingRowEventArgs

	#region CancellablePageChangingEventArgs
	/// <summary>
	/// A class listing the information needed when a paging operation is started.
	/// </summary>
	public class CancellablePageChangingEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// The PageIndex of the page of data that will become visible.
		/// </summary>
		public int NextPageIndex
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// The <see cref="ColumnLayout"/> that this event was triggered for.
		/// </summary>
		public ColumnLayout ColumnLayout
		{
			get;
			set;
		}

		/// <summary>
		/// The depth at which the <see cref="ColumnLayout"/> is representing. 
		/// </summary>
		/// <remarks>Level 0 is the root level.</remarks>
		public int Level
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the collection of rows or row island that this particular <see cref="ColumnLayout"/> will represent.
		/// </summary>
		public RowCollection Rows
		{
			get;
			protected internal set;
		}
	}
	#endregion // CancellablePageChangingEventArgs

	#region PageChangedEventArgs
	/// <summary>
	/// A class listing the information needed after a pager operation is completed.
	/// </summary>
	public class PageChangedEventArgs : EventArgs
	{
		/// <summary>
		/// The PageIndex of the page that was visible.
		/// </summary>
		public int OldPageIndex
		{
			get;
			protected internal set;
		}
	}
	#endregion // PageChangedEventArgs

	#region ColumnLayoutEventArgs
	/// <summary>
	/// A class listing the <see cref="ColumnLayout"/> that was modified for an event.
	/// </summary>
	public class ColumnLayoutEventArgs : EventArgs
	{
		/// <summary>
		/// The ColumnLayout that was modified in some way.
		/// </summary>
		public ColumnLayout ColumnLayout
		{
			get;
			protected internal set;
		}
	}
	#endregion // ColumnLayoutEventArgs

	#region CancellableColumnResizingEventArgs
	/// <summary>
	/// A class listing the EventArgs for a user driven column resizing.
	/// </summary>
	public class CancellableColumnResizingEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// A Collection of columns that will be resized during this resizing action.
		/// </summary>
		public Collection<Column> Columns
		{
			get;
			internal set;
		}

		/// <summary>
		/// The starting width of the column.
		/// </summary>
		public double Width
		{
			get;
			protected internal set;
		}
	}

	#endregion // CancellableColumnResizingEventArgs

	#region ColumnResizedEventArgs

	/// <summary>
	/// A class listing the EventArgs after a column is finished resizing.
	/// </summary>
	public class ColumnResizedEventArgs : EventArgs
	{
		/// <summary>
		/// A Collection of columns that was resized during the action.
		/// </summary>
		public ReadOnlyCollection<Column> Columns
		{
			get;
			internal set;
		}
	}

	#endregion // ColumnResizedEventArgs

	#region CancellableRowAddingEventArgs
	/// <summary>
	/// A class listing the <see cref="CancellableEventArgs"/> for Adding and Inserting a new row.
	/// </summary>
	public class CancellableRowAddingEventArgs : CancellableRowEventArgs
	{
		/// <summary>
		/// The index at which the row is being inserted.
		/// </summary>
		public int InsertionIndex
		{
			get;
			protected internal set;
		}
	}

	#endregion // CancellableRowAddingEventArgs

	#region RowEventArgs
	/// <summary>
	/// A class listing the EventArgs for an event with a <see cref="Row"/> input.
	/// </summary>
	public class RowEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Row"/> that was modified.
		/// </summary>
		public Row Row
		{
			get;
			protected internal set;
		}
	}

	#endregion // RowEventArgs

	#region CancellableRowEventArgs
	/// <summary>
	/// A class listing the EventArgs for a cancellable event with a row input.
	/// </summary>
	public class CancellableRowEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// The <see cref="Row"/> that was modified.
		/// </summary>
		public Row Row
		{
			get;
			protected internal set;
		}
	}

	#endregion // CancellableRowEventArgs

	#region GroupByCollectionChangedEventArgs

	/// <summary>
	/// A class listing the information needed during GroupBy collection events of the <see cref="XamGrid"/>.
	/// </summary>
	public class GroupByCollectionChangedEventArgs : EventArgs
	{
		/// <summary>
		/// A collection of items that were previously grouped. 
		/// </summary>
		public IList<Column> PreviousGroupedColumns { get; protected internal set; }

		/// <summary>
		/// A collection of items that are currently grouped.
		/// </summary>
		public IList<Column> NewGroupedColumns { get; protected internal set; }
	}

	#endregion // GroupByCollectionChangedEventArgs

	#region CellValidationErrorEventArgs

	/// <summary>
	/// A class listing the information needed when a Cell fails validation.
	/// </summary>
	public class CellValidationErrorEventArgs : EventArgs
	{
		/// <summary>
		/// The actual <see cref="ValidationErrorEventArgs"/>.
		/// </summary>
		public ValidationErrorEventArgs ValidationErrorEventArgs { get; protected internal set; }

		/// <summary>
		/// The <see cref="Cell"/> whose validation failed.
		/// </summary>
		public Cell Cell { get; protected internal set; }

		/// <summary>
		/// Gets/sets whether the event is handled. If true, then the <see cref="Cell"/> will treat the validation as if it had passed.
		/// </summary>
		public bool Handled { get; set; }
	}

	#endregion // CellValidationErrorEventArgs

	#region DataObjectCreationEventArgs

	/// <summary>
	/// A class listing the information needed when a new object needs to be created.
	/// </summary>
	public class DataObjectCreationEventArgs : EventArgs
	{
		/// <summary>
		/// Gets / sets an object of the <see cref="ObjectType"/> which will be used as the newly created object.
		/// </summary>
		public object NewObject { get; set; }

		/// <summary>
		/// Gets the <see cref="Type"/> of object that the DataManager expects to be created.
		/// </summary>
		public Type ObjectType { get; protected internal set; }

		/// <summary>
		/// Gets the <see cref="ColumnLayout"/> which the object would be created under.
		/// </summary>
		public ColumnLayout ColumnLayout { get; protected internal set; }

		/// <summary>
		/// Gets the <see cref="Row"/> object which is the parent for this object.  
		/// </summary>		
		public Row ParentRow { get; protected internal set; }

		/// <summary>
		/// Gets the <see cref="Type"/> which is contained in the underlying collection.
		/// </summary>
		public Type CollectionType { get; protected internal set; }

		/// <summary>
		/// Gets the <see cref="RowType"/> of the row that the newly created object will be associated with.
		/// </summary>
		public RowType? RowTypeCreated { get; protected internal set; }
	}
	#endregion

	#region ColumnEventArgs

	/// <summary>
	/// A class listing the EventArgs for an event with a <see cref="Column"/> input.
	/// </summary>
	public class ColumnEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Column"/> that this event was triggered for.
		/// </summary>
		public Column Column
		{
			get;
			protected internal set;
		}
	}

	#endregion // ColumnEventArgs

	#region CancellableColumnEventArgs

	/// <summary>
	/// A class listing the EventArgs for an cancellable event with a <see cref="Column"/> input.
	/// </summary>
	public class CancellableColumnEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// The <see cref="Column"/> that this event was triggered for.
		/// </summary>
		public Column Column
		{
			get;
			protected internal set;
		}
	}

	#endregion // CancellableColumnEventArgs

	#region PopulatingFiltersEventArgs

	/// <summary>
	/// An event args for the PopulatingFilters Event
	/// </summary>
	public class PopulatingFiltersEventArgs : ColumnEventArgs
	{
		/// <summary>
		/// Gets the <see cref="FilterOperandCollection"/> which represents the items which will be seen in the dropdown.
		/// </summary>
		public FilterOperandCollection FilterOperands
		{
			get;
			protected internal set;
		}
	}

	#endregion // PopulatingFiltersEventArgs

	#region DataLimitingEventArgs

	/// <summary>
	/// Event argument when event is raised that will limit data.
	/// </summary>
	public class DataLimitingEventArgs : EventArgs
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="DataLimitingEventArgs"/> class.
		/// </summary>
		/// <param name="args"></param>
		public DataLimitingEventArgs(DataAcquisitionEventArgs args)
		{
			this._baseEventArgs = args;
		}
		#endregion // Constructor

		#region Members
		DataAcquisitionEventArgs _baseEventArgs;
		#endregion // Members

		#region DataSource

		/// <summary>
		/// Gets / sets the IList that will be applied to the data manager.
		/// </summary>
		public IList DataSource
		{
			get
			{
				if (this._baseEventArgs != null)
					return this._baseEventArgs.DataSource;
				return null;
			}
			set
			{
				this._baseEventArgs.DataSource = value;
			}
		}
		#endregion // DataSource

		#region EnablePaging
		/// <summary>
		/// Gets if the DataManager expects paged data.
		/// </summary>
		public bool EnablePaging
		{
			get
			{
				if (this._baseEventArgs != null)
					return this._baseEventArgs.EnablePaging;
				return false;
			}
		}

		#endregion // EnablePaging

		#region PageSize
		/// <summary>
		/// Gets the maximum number of rows expected by the DataManager.  		
		/// </summary>
		/// <remarks>
		/// Used primarily when EnablePaging is true.
		/// </remarks>
		public int PageSize
		{
			get
			{
				if (this._baseEventArgs != null)
					return this._baseEventArgs.PageSize;
				return 0;
			}
		}

		#endregion // PageSize

		#region CurrentPage
		/// <summary>
		/// Gets the current page index
		/// </summary>
		public int CurrentPage
		{
			get
			{
				if (this._baseEventArgs != null)
					return this._baseEventArgs.CurrentPage;
				return 0;
			}
		}

		#endregion // CurrentPage

		#region CurrentSort
		/// <summary>
		/// Gets a collection <see cref="SortContext"/> which will be applied.
		/// </summary>
		public ObservableCollection<SortContext> CurrentSort
		{
			get
			{
				if (this._baseEventArgs != null)
					return this._baseEventArgs.CurrentSort;
				return null;
			}
		}

		#endregion // CurrentSort

		#region GroupByContext
		/// <summary>
		/// Gets the GroupBy that will be applied to the data.
		/// </summary>
		public GroupByContext GroupByContext
		{
			get
			{
				if (this._baseEventArgs != null)
					return this._baseEventArgs.GroupByContext;
				return null;
			}
		}
		#endregion // GroupByContext

		#region Filters
		/// <summary>
		/// Gets a collection that lists what filters will be applied.
		/// </summary>
		public RecordFilterCollection Filters
		{
			get
			{
				if (this._baseEventArgs != null)
					return this._baseEventArgs.Filters;
				return null;
			}
		}

		#endregion // Filters

		#region ParentRow

		/// <summary>
		/// Gets the <see cref="Row"/> object which is the parent for this object.  
		/// </summary>
		public Row ParentRow
		{
			get;
			protected internal set;
		}

		#endregion // ParentRow

		#region ColumnLayout

		/// <summary>
		/// Gets the <see cref="ColumnLayout"/> which the object would be created under.
		/// </summary>
		public ColumnLayout ColumnLayout { get; protected internal set; }

		#endregion // ColumnLayout
	}

	#endregion // DataLimitingEventArgs

	#region CancellableFilteringEventArgs

	/// <summary>
	/// A class listing the EventArgs when filtering the rows.
	/// </summary>
	public class CancellableFilteringEventArgs : CancellableColumnEventArgs
	{
		/// <summary>
		/// The <see cref="RowFiltersCollection"/> which is going to be modified.
		/// </summary>
		public RowFiltersCollection RowFiltersCollection { get; protected internal set; }

		/// <summary>
		/// The value which will be used by the filter.
		/// </summary>
		public object FilterValue { get; protected internal set; }

		/// <summary>
		/// The <see cref="FilterOperand"></see> which is being used to filter.
		/// </summary>
		public FilterOperand FilteringOperand { get; protected internal set; }
	}

	#endregion // CancellableFilteringEventArgs

	#region FilteredEventArgs

	/// <summary>
	/// A class listing the EventArgs after a filter has been applied.
	/// </summary>
	public class FilteredEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="RowFiltersCollection"/> which was modified.
		/// </summary>
		public RowFiltersCollection RowFiltersCollection { get; protected internal set; }
	}
	#endregion // FilteredEventArgs

	#region ClipboardCopyingEventArgs

	/// <summary>
	/// Provides information about selected cells that will be copied to the clipboard.
	/// </summary>
	public class ClipboardCopyingEventArgs : CancellableEventArgs
	{
        #region Members

        private XamGrid _grid;

        #endregion

        #region Properties 

        #region Public 

        #region SelectedItems

        /// <summary>
        /// The selected cells that will be copied to the clipboard.
        /// </summary>
        public ReadOnlyCollection<CellBase> SelectedItems { get; protected internal set; }

        #endregion

        #region ClipboardValue

        /// <summary>
        /// Gets or sets the text being copied to the clipboard.
        /// </summary>
        public string ClipboardValue { get; set; }


        #endregion

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Internal Constructor used for building the event args and loading the XamGrid instance into it.
        /// </summary>
        /// <param name="grid"></param>
        internal ClipboardCopyingEventArgs(XamGrid grid)
        {
            _grid = grid;
        }

        #endregion

        #region Methods

        #region Public 

        #region ValidateSelectedRectangle

        /// <summary>
        /// Determines whether the selected region is valid for copying.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the selection is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The selection is considered as valid if the selected cells form a rectangular region in a single band.
        /// </remarks>
        public bool ValidateSelectedRectangle()
        {
            //This should never happen
            if (_grid == null)
            {
                return false;
            }

            if (GetCopyTypeResolved() == GridClipboardCopyType.SelectedRows)
            {
                SelectedRowsCollection selectedRows = _grid.SelectionSettings.SelectedRows;

                if (!selectedRows.Any() || IsSelectionCrossBand())
                {
                    return false;
                }

                int minRowIndex = selectedRows.Min(r => r.Index);
                int maxRowIndex = selectedRows.Max(r => r.Index);

                if ((maxRowIndex - minRowIndex + 1) == selectedRows.Count)
                {
                    return true;
                }
            }
            else
            {
                SelectedCellsCollection selectedCells = _grid.SelectionSettings.SelectedCells;

                if (!selectedCells.Any() || IsSelectionCrossBand())
                {
                    return false;
                }

                IList<ColumnBase> allColumns =
                    selectedCells[0].Column.ColumnLayout.Columns.AllColumns.Where(i => i is Column && !(i is GroupColumn)).ToList();

                int minRowIndex = selectedCells.Min(c => c.Row.Index);
                int maxRowIndex = selectedCells.Max(c => c.Row.Index);
                int minColIndex = selectedCells.Min(c => allColumns.IndexOf(c.Column));
                int maxColIndex = selectedCells.Max(c => allColumns.IndexOf(c.Column));

                int expectedCellCount = (maxColIndex - minColIndex + 1) * (maxRowIndex - minRowIndex + 1);

                if (expectedCellCount == selectedCells.Count)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Private 

        #region GetCopyTypeResolved

        /// <summary>
        /// Gets the CopyType used by the <see cref="XamGrid"/>.
        /// </summary>
        /// <returns>The resolved <see cref="GridClipboardCopyType"/>.</returns>
        private GridClipboardCopyType GetCopyTypeResolved()
        {
            if (_grid.ClipboardSettings.CopyType == GridClipboardCopyType.Default)
            {
                if (_grid.SelectionSettings.CellClickAction == CellSelectionAction.SelectCell)
                {
                    return GridClipboardCopyType.SelectedCells;
                }

                return GridClipboardCopyType.SelectedRows;
            }

            return _grid.ClipboardSettings.CopyType;
        }

        #endregion // GetCopyTypeResolved

        #region IsSelectionCrossBand

        /// <summary>
        /// Determines whether the selected cells/rows are in different bands.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the selection is cross-band; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSelectionCrossBand()
        {
            if (GetCopyTypeResolved() == GridClipboardCopyType.SelectedCells)
            {
                SelectedCellsCollection selectedCells = _grid.SelectionSettings.SelectedCells;

                if (!selectedCells.Any())
                {
                    return false;
                }

                int level = selectedCells[0].Row.Level;

                return selectedCells.Any(cell => cell.Row.Level != level);
            }
            else
            {
                SelectedRowsCollection selectedRows = _grid.SelectionSettings.SelectedRows;

                if (!selectedRows.Any())
                {
                    return false;
                }

                int level = selectedRows[0].Level;

                return selectedRows.Any(row => row.Level != level);
            }
        }

        #endregion // IsSelectionCrossBand

        #endregion

        #endregion

        #endregion
    }

	#endregion // ClipboardCopyingEventArgs

	#region ClipboardCopyingItemEventArgs

	/// <summary>
	/// Provides information about a selected cell or a header that will be copied to the clipboard.
	/// </summary>
	public class ClipboardCopyingItemEventArgs : CancellableEventArgs
	{
		/// <summary>
		/// Gets the cell being copied to the clipboard.
		/// </summary>
		public CellBase Cell { get; protected internal set; }

		/// <summary>
		/// Gets or sets the Value of the cell being copied to the clipboard.
		/// </summary>
		public string ClipboardValue { get; set; }		
	}

	#endregion // ClipboardCopyingItemEventArgs

	#region ClipboardPastingEventArgs

	/// <summary>
	/// Provides information about the selected sells's values from Excel that will be pasted.
	/// </summary>
	public class ClipboardPastingEventArgs : EventArgs
    {
        #region Members

        private XamGrid _grid;

        #endregion

        #region Constructor

        /// <summary>
        /// Internal Constructor used for building the event args and loading the XamGrid instance into it.
        /// </summary>
        /// <param name="grid"></param>
        internal ClipboardPastingEventArgs(XamGrid grid)
        {
            _grid = grid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The parsed clipboard value that will be pasted.
        /// </summary>
        public List<List<string>> Values { get; set; }

        /// <summary>
        /// The original clipboard value that will be pasted.
        /// </summary>
        public string ClipboardValue { get; protected internal set; }

        #endregion
        
        #region Methods
        
        #region Public
        
        #region PasteAsExcel

        /// <summary>
        /// Pastes data on the <see cref="XamGrid"/>.
        /// </summary>
        public void PasteAsExcel()
        {
            if (_grid == null)
            {
                return;
            }

            if (Values == null)
            {
                _grid.OnClipboardPasteError(ClipboardPasteErrorType.InvalidInputSelection, false);
                return;
            }

            // We will paste only if we have an ActiveCell as starting point and if the ActiveCell is on a DataRow
            if (_grid.ActiveCell == null || _grid.ActiveCell.Row.RowType != RowType.DataRow || _grid.ActiveCell.Column == null)
            {
                _grid.OnClipboardPasteError(ClipboardPasteErrorType.InvalidDestinationSelection, false);
                return;
            }

            IList<Cell> cellsToSelect = new List<Cell>();

            // NZ 16 March 2012 - TFS99335 - Get the columns in the order they are displayed (includiing LeftFixed,
            // Merged, Right Fixed columns)
            List<Column> allColumns = this.GetAllVisibleChildColumns();

            int maxCellsPerRow = Values.Max(r => r.Count);

            // The index of the column where the ActiveCell is
            int columnIndex = allColumns.IndexOf(_grid.ActiveCell.Column);

            if (columnIndex + maxCellsPerRow > allColumns.Count)
            {
                bool shouldContinue = _grid.OnClipboardPasteError(ClipboardPasteErrorType.TruncationError, true);
                if (!shouldContinue)
                {
                    return;
                }
            }

            // The index of the row where the Active cell is
            int rowIndex = _grid.ActiveCell.Row.Index;

            RowCollection rowCollection = GetRowCollectionResolved();

            // The number of rows we want to paste
            int rowsCount = Values.Count;

            if (rowsCount == 0)
                return;

            string[,] _revertValues = new string[rowsCount, Values.First().Count];

            // The number of rows in the current band
            int bandRowsCount = rowCollection.Count;

            // The index of the last row where we will be able to paste data
            int lastRowIndex = Math.Min(bandRowsCount - 1, rowIndex + rowsCount - 1);

            // NZ 16 March 2012 - TFS103458 - Raise TruncationError if we don't have enough space to paste all rows.
            if (rowIndex + rowsCount > bandRowsCount)
            {
                bool shouldContinue = _grid.OnClipboardPasteError(ClipboardPasteErrorType.TruncationError, true);
                if (!shouldContinue)
                {
                    return;
                }
            }

            bool stopCellProcessing = false;
            int lastSuccessfulRow, lastSuccessfulCol;

            lastSuccessfulRow = -1;
            lastSuccessfulCol = -1;

            // Iterating through the rows and pasting the data
            for (int i = rowIndex; i <= lastRowIndex; i++)
            {
                if (stopCellProcessing)
                {
                    break;
                }

                // The index of the row in the values list
                int parsedRowIndex = i - rowIndex;

                Row row = rowCollection[i];

                // The number of cells we want to paste
                int cellsCount = Values[parsedRowIndex].Count;

                // The index of the last cell where we will be able to paste data
                int lastCellIndex = Math.Min(allColumns.Count - 1, cellsCount + columnIndex - 1);

                for (int j = columnIndex; j <= lastCellIndex; j++)
                {
                    // The index of the cell in the values list
                    int parsedColumnIndex = j - columnIndex;

                    EditableColumn column = allColumns[j] as EditableColumn;

                    bool skipCell = false;

                    // Skipping non-editable columns and UnboundColumns
                    if (column == null || column.IsReadOnly)
                    {
                        bool shouldContinue = _grid.OnClipboardPasteError(ClipboardPasteErrorType.ReadOnlyColumn, true);
                        if (!shouldContinue)
                        {
                            stopCellProcessing = true;
                            break;
                        }

                        skipCell = true;
                    }
                    else if (column is UnboundColumn)
                    {
                        bool shouldContinue = _grid.OnClipboardPasteError(ClipboardPasteErrorType.UnboundColumn, true);
                        if (!shouldContinue)
                        {
                            stopCellProcessing = true;
                            break;
                        }

                        skipCell = true;
                    }

                    // NZ 16 March 2012 - TFS103404 - If the column is not Editable we'll skip it
                    if (column == null || skipCell)
                    {
                        continue;
                    }

                    string parsedCellValue = Values[parsedRowIndex][parsedColumnIndex];

                    _revertValues[parsedRowIndex, parsedColumnIndex] = GetCellValue(row, column);

                    bool isValid = SetCellValue(row, column, parsedCellValue);
                      
                    if (!isValid)
                    {
                        bool shouldContinue = _grid.OnClipboardPasteError(ClipboardPasteErrorType.CastError, true);

                        if (!shouldContinue)
                        {
                            stopCellProcessing = true;
                            break;
                        }
                    }
                    else
                    {
                        lastSuccessfulCol = j;
                        lastSuccessfulRow = i;

                        Cell cell = row.Cells[column] as Cell;

                        if (cell != null)
                        {
                            cellsToSelect.Add(cell);
                        }
                    }
                }
            }

            if (stopCellProcessing)
            {
                this.RevertCellValues(_revertValues, rowIndex, columnIndex, lastSuccessfulRow, lastSuccessfulCol);
            }
            else
            {
                // Select the cells only if the processing was not stopped
                this.SelectCells(cellsToSelect);
            }
        }

        #endregion // PasteAsExcel

        #endregion // Public

        #region Private

        #region RevertCellValues

        /// <summary>
        /// Takes a group of cells, and reverts them to the previous values in the event an error occurred and the user does not wish to recover.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="startingRow"></param>
        /// <param name="startingCol"></param>
        /// <param name="endingRow"></param>
        /// <param name="endingCol"></param>
        /// <returns></returns>
        private bool RevertCellValues(string[,] values, int startingRow, int startingCol, int endingRow, int endingCol)
        {
            // All columns except GroupColumns
            IList<Column> allColumns = this.GetAllVisibleChildColumns();

            RowCollection rowCollection = GetRowCollectionResolved();

            for (int i = startingRow; i <= endingRow; i++)
            {
                // The index of the row in the values list
                int parsedRowIndex = i - startingRow;

                Row row = rowCollection[i];

                // The number of cells we want to paste
                int cellsCount = values.GetUpperBound(1) + 1;

                // The index of the last cell where we will be able to paste data
                int lastCellIndex = Math.Min(allColumns.Count - 1, cellsCount + startingCol - 1);

                for (int j = startingCol; j <= lastCellIndex; j++)
                {
                    if ((i == endingRow) && (j > endingCol))
                    {
                        break;
                    }

                    // The index of the cell in the values list
                    int parsedColumnIndex = j - startingCol;

                    EditableColumn column = allColumns[j] as EditableColumn;

                    if (column == null || column is UnboundColumn)
                    {
                        continue;
                    }

                    string revertValues = values[parsedRowIndex, parsedColumnIndex];
                    
                    bool isValid = SetCellValue(row, column, revertValues);

                    Cell cell = row.Cells[column] as Cell;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                }
            }

            return true;
        }

        #endregion // RevertCellValues

        #region SetCellValue

        /// <summary>
        /// Sets the cell of a value.
        /// </summary>
        /// <param name="row">The <see cref="Row"/> containing the cell.</param>
        /// <param name="column">The <see cref="Column"/> containing the cell.</param>
        /// <param name="parsedCellValue">The parsed cell value.</param>
        /// <returns>
        /// <c>true</c> if the parsed value can be converted and set to the cell; otherwise, <c>false</c>.
        /// </returns>
        private bool SetCellValue(Row row, EditableColumn column, string parsedCellValue)
        {
            bool isValid = true;

            Infragistics.Controls.Grids.Cell.CellValueObject cellValueObj = new Infragistics.Controls.Grids.Cell.CellValueObject();

            Binding binding = new Binding(column.Key)
            {
                Mode = BindingMode.TwoWay,
                Source = row.Data,
                Converter = column.EditorValueConverter,
                ConverterParameter = column.EditorValueConverterParameter,
                ConverterCulture = CultureInfo.CurrentCulture
            };

            BindingOperations.SetBinding(cellValueObj, Infragistics.Controls.Grids.Cell.CellValueObject.ValueProperty, binding);

            if (column.EditorValueConverter != null && column is TextColumn)
            {
                cellValueObj.Value = parsedCellValue;
            }
            else
            {
                object resolvedValue = null;
                bool targetTypeIsNullable = false;

                try
                {
                    // The target data type
                    Type targetType = column.DataType;

                    if (targetType != null)
                    {
                        // If the targetType is Nullable we have to get the underlying type so we can use it for conversion
                        if (targetType.IsGenericType
                            && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            targetType = Nullable.GetUnderlyingType(targetType);
                            targetTypeIsNullable = true;
                        }

                        if (!(targetTypeIsNullable && string.IsNullOrEmpty(parsedCellValue)))
                        {
                            // Convert the parsed value to the target type
                            resolvedValue = Convert.ChangeType(parsedCellValue, targetType, CultureInfo.CurrentCulture);
                        }
                    }
                }
                catch (InvalidCastException)
                {
                    isValid = false;
                }
                catch (FormatException)
                {
                    isValid = false;
                }
                catch (OverflowException)
                {
                    isValid = false;
                }
                catch (ArgumentException)
                {
                    isValid = false;
                }

                if (resolvedValue != null)
                {
                    cellValueObj.Value = resolvedValue;
                }
                else if (targetTypeIsNullable && string.IsNullOrEmpty(parsedCellValue))
                {
                    // We have a nullable target type and empty parsed value,
                    // it looks like an empty cell ... 
                    cellValueObj.Value = null;
                }
            }

            return isValid;
        }

        #endregion // SetCellValue

        #region SelectCells

        /// <summary>
        /// Selects cells of the <see cref="XamGrid"/>.
        /// </summary>
        /// <param name="cellsToSelect">The cells that will be selected.</param>
        private void SelectCells(IEnumerable<Cell> cellsToSelect)
        {
            if (cellsToSelect.Any())
            {
                _grid.SelectionSettings.SelectedCells.Clear();
            }

            using (var selectedCollection = new SelectedCellsCollection())
            {
                foreach (var cell in cellsToSelect)
                {
                    selectedCollection.Add(cell);
                }

                _grid.SelectionSettings.SelectedCells.AddRange(selectedCollection);
            }
        }

        #endregion // SelectCells

        #region GetRowCollectionResolved

        /// <summary>
        /// Gets the <see cref="RowCollection"/> containing the row of the <see cref="XamGrid.ActiveCell"/>.
        /// </summary>
        /// <returns>The <see cref="RowCollection"/> containing the row of the <see cref="XamGrid.ActiveCell"/>.</returns>
        private RowCollection GetRowCollectionResolved()
        {
            ChildBand childBand = ((Row)_grid.ActiveCell.Row).ParentRow as ChildBand;
            GroupByRow groupByRow = ((Row)_grid.ActiveCell.Row).ParentRow as GroupByRow;

            if (childBand != null)
            {
                return childBand.Rows;
            }

            if (groupByRow != null)
            {
                return groupByRow.Rows;
            }

            return _grid.Rows;
        }

        #endregion // GetRowCollectionResolved

        #region GetCellValue

        /// <summary>
        /// Gets the cell of a value.
        /// </summary>
        /// <param name="row">The <see cref="Row"/> containing the cell.</param>
        /// <param name="column">The <see cref="Column"/> containing the cell.</param>
        /// <returns>
        /// The Value in the Cell.
        /// </returns>
        private string GetCellValue(Row row, EditableColumn column)
        {
            Infragistics.Controls.Grids.Cell.CellValueObject cellValueObj = new Infragistics.Controls.Grids.Cell.CellValueObject();

            Binding binding = new Binding(column.Key)
            {
                Mode = BindingMode.TwoWay,
                Source = row.Data,
                Converter = column.EditorValueConverter,
                ConverterParameter = column.EditorValueConverterParameter,
                ConverterCulture = CultureInfo.CurrentCulture
            };
            
            BindingOperations.SetBinding(cellValueObj, Infragistics.Controls.Grids.Cell.CellValueObject.ValueProperty, binding);

            var value = Convert.ChangeType(cellValueObj.Value, typeof(string), CultureInfo.CurrentCulture);
            
            return value as string;
        }

        #endregion // SetCellValue

        #region GetAllVisibleChildColumns

        /// <summary>
        /// Gets all visible child columns in the same order as they are displayed.
        /// </summary>
        /// <returns></returns>
        private List<Column> GetAllVisibleChildColumns()
        {
            List<Column> allColumns = new List<Column>();
            
            RowBase activeCellRow = _grid.ActiveCell.Row;

            if (this._grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
            {
                ReadOnlyCollection<Column> mergedCols = this._grid.GroupBySettings.GroupByColumns[activeCellRow.ColumnLayout];
                
                foreach (Column mergedCol in mergedCols)
                {
                    if (mergedCol.Visibility == Visibility.Visible)
                    {
                        allColumns.Add(mergedCol);
                    }
                }
            }

            foreach (Column fixedColumn in activeCellRow.Columns.FixedColumnsLeft)
            {
                if (fixedColumn.Visibility == Visibility.Visible)
                {
                    if (!(fixedColumn is GroupColumn))
                    {
                        allColumns.Add(fixedColumn);
                    }

                    foreach (var childColumn in fixedColumn.AllColumns)
                    {
                        if (!(childColumn is GroupColumn) && childColumn.Visibility == Visibility.Visible)
                        {
                            allColumns.Add(childColumn);
                        }
                    }
                }
            }

            ReadOnlyKeyedColumnBaseCollection<ColumnBase> allCols = activeCellRow.Columns.AllColumns;

            foreach (ColumnBase cBase in allCols)
            {
                Column col = cBase as Column;
                
                if (col != null)
                {
                    if (col.SupportsActivationAndSelection && col.Visibility == Visibility.Visible && col.IsFixed == FixedState.NotFixed && !allColumns.Contains(col))
                    {
                        allColumns.Add(col);
                    }
                }
            }

            FixedColumnsCollection right = activeCellRow.Columns.FixedColumnsRight;
            
            int frcount = right.Count;

            for (int i = frcount - 1; i >= 0; i--)
            {
                if (right[i].Visibility == Visibility.Visible)
                {
                    if (!(right[i] is GroupColumn))
                    {
                        allColumns.Add(right[i]);
                    }

                    foreach (var childColumn in right[i].AllColumns)
                    {
                        if (!(childColumn is GroupColumn) && childColumn.Visibility == Visibility.Visible)
                        {
                            allColumns.Add(childColumn);
                        }
                    }
                }
            }

            return allColumns;
        }

        #endregion // GetAllVisibleChildColumns

        #endregion // Private

        #endregion // Methods
    }

	#endregion // ClipboardPastingEventArgs

    #region ClipboardPasteErrorEventArgs

    /// <summary>
    /// Provides information about the selected sells's values from Excel that will be pasted.
    /// </summary>
    public class ClipboardPasteErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The constructor for the Clipboard Paste Error Event Args, sets a couple of default values.
        /// </summary>
        public ClipboardPasteErrorEventArgs()
        {
            IsRecoverable = true;
            AttemptRecover = true;
        }

        #region Properties

        /// <summary>
        /// The type of error that occurred during the excel style paste operation
        /// </summary>
        public ClipboardPasteErrorType ErrorType { get; set; }

        /// <summary>
        /// Set by the Paste Process, dictates whether or not this error is recoverable
        /// </summary>
        public bool IsRecoverable { get; internal set; }

        /// <summary>
        /// Set by the handler of the error, allows the paste process to attempt to recover from the error
        /// </summary>
        public bool AttemptRecover { get; set; }

        #endregion
    }

    #endregion // ClipboardPasteErrorEventArgs

    #region ColumnAutoGeneratedEventArgs

    /// <summary>
    /// Provides information about a newly generated column.
    /// </summary>
    public class ColumnAutoGeneratedEventArgs : EventArgs
    {
        /// <summary>
        /// The Instance of the Column that was generated.
        /// </summary>
        public ColumnBase Column { get; set; }
    }

    #endregion // ColumnAutoGeneratedEventArgs 

    #region ColumnVisibilityChangedEventArgs

    /// <summary>
    /// Contains the arguments returned by the event fired when a <see cref="ColumnBase"/> instance's visibility changes.
    /// </summary>
    public class ColumnVisibilityChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The Instance of the Column that was generated.
        /// </summary>
        public ColumnBase Column { get; set; }
    }

    #endregion // ColumnVisibilityChangedEventArgs
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