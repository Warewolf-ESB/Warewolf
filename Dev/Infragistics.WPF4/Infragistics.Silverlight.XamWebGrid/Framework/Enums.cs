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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	#region FixedRowAlignment

	/// <summary>
	/// An Enum that describes where a FixedRow should be aligned. 
	/// </summary>
	public enum FixedRowAlignment
	{
		/// <summary>
		/// A FixedRow should be aligned to the top of the <see cref="XamGrid"/>
		/// </summary>
		Top,

		/// <summary>
		/// A FixedRow should be aligned to the bottom of the <see cref="XamGrid"/>
		/// </summary>
		Bottom
	}

	#endregion // FixedRowAlignment

	#region ColumnWidthType
	/// <summary>
	/// An Enum that describes the type of width for the Column.
	/// </summary>
	public enum ColumnWidthType
	{
		/// <summary>
		/// A column's width will size to the largest header or cell in the column. 
		/// Note: while scrolling the width of the column may grow as larger content comes into view, however,
		/// it will never decrease in width.
		/// </summary>
		Auto,

		/// <summary>
		/// A column's width will size to the largest header or cell in the column. However, this will only occur
		/// when the grid first loads. Or when a user double clicks on the edge of a column header to resize. 
		/// </summary>
		InitialAuto,


		/// <summary>
		/// A column's width will size to the header of a column. 
		/// </summary>
		SizeToHeader,

		/// <summary>
		/// A column's width will size to the largest cell in the column. 
		/// Note: while scrolling the width of the column may grow as larger content comes into view, however,
		/// it will never decrease in width.
		/// </summary>
		SizeToCells,

		/// <summary>
		/// A column's width will size to fill any remaing space in the <see cref="XamGrid"/>. 
		/// If more than one column has a star value specified, the remaing width will be split
		/// evenly amongst the columns. 
		/// If other columns already are taking up the majority of the space, the column's width will be zero.
		/// If the <see cref="XamGrid"/>'s width is Infinity, then the column will act as ColumnWidthType.Auto width.
		/// </summary>
		Star,

		/// <summary>
		/// A column's width will size to the value specified. 
		/// </summary>
		Numeric
	}
	#endregion // ColumnWidthType

	#region ColumnLayoutHeaderVisibility
	/// <summary>
	/// An Enum that describes whether a <see cref="ColumnLayout"/>'s header row will be visible. 
	/// </summary>
	public enum ColumnLayoutHeaderVisibility
	{
		/// <summary>
		/// The <see cref="ColumnLayout"/>'s header row will be visible only if the ColumnLayout has siblings.
		/// </summary>
		SiblingsExist,

		/// <summary>
		/// The <see cref="ColumnLayout"/>'s header row will always be visible.
		/// </summary>
		Always,

		/// <summary>
		/// The <see cref="ColumnLayout"/>'s header row will never be visible.
		/// </summary>
		Never
	}
	#endregion // ColumnLayoutHeaderVisibility

	#region ColumnMovingType

	/// <summary>
	/// Describes how Column Moving will work in the <see cref="XamGrid"/>
	/// </summary>
	public enum ColumnMovingType
	{
		/// <summary>
		/// Column moving is turned off.
		/// </summary>
		Disabled = 0,

		/// <summary>
		/// An Indicator will show where a column will be inserted.
		/// </summary>
		Indicator,

		/// <summary>
		/// Moves the column immediately into the column position closest to the mouse pointer.
		/// </summary>
		Immediate

	}

	#endregion // ColumnMovingType

	#region SortDirection
	/// <summary>
	/// Describes options available for sorting in the <see cref="XamGrid"/>.
	/// </summary>
	public enum SortDirection
	{
		/// <summary>
		/// No sort is applied.
		/// </summary>
		None,
		/// <summary>
		/// InvalidateSort in an ascending manner.
		/// </summary>
		Ascending,
		/// <summary>
		/// InvalidateSort in a descending manner.
		/// </summary>
		Descending
	}
	#endregion

	#region KeyBoardNavigation
	/// <summary>
	/// Describes options for Keyboard navigation in the <see cref="XamGrid"/>
	/// </summary>
	public enum KeyboardNavigation
	{
		/// <summary>
		/// When using the UP/Down arrows, the ActiveCell will stay on the current band.
		/// </summary>
		CurrentLayout,

		/// <summary>
		/// When using the UP/Down arrows, the ActiveCell will go into child and parent bands.
		/// </summary>
		AllLayouts
	}
	#endregion // KeyBoardNavigation

	#region CellClickAction
	/// <summary>
	/// Describes how selection should work when a user clicks on a cell in the grid. 
	/// </summary>
	public enum CellSelectionAction
	{
		/// <summary>
		/// The Entire row should be selected. 
		/// </summary>
		SelectRow = 0,

		/// <summary>
		/// The particular cell should be selected. 
		/// </summary>
		SelectCell
	}
	#endregion // CellClickAction

	#region SelectionType

	/// <summary>
	/// Describes the type of selection that should be performed. 
	/// </summary>
	public enum SelectionType
	{
		/// <summary>
		/// Selection should be disabled
		/// </summary>
		None = 0,

		/// <summary>
		/// Only one item should be selected at a given time. 
		/// </summary>
		Single,

		/// <summary>
		/// Multiple items can be selected via the ctrl and shift keys. 
		/// </summary>
		Multiple,
	}

	#endregion // SelectionType

	#region CellAlignment

	/// <summary>
	/// Specifies how a cell should be aligned. 
	/// </summary>
	public enum CellAlignment
	{
		/// <summary>
		/// The Cell should be aligned to the top.
		/// </summary>
		Top,

		/// <summary>
		/// The Cell should be aligned to the bottom. 
		/// </summary>
		Bottom,

		/// <summary>
		/// The Cell can be aligned wherever it needs to. 
		/// </summary>
		NotSet
	}
	#endregion // CellAlignment

	#region InvokeAction

	/// <summary>
	/// An enumeration of different interactions.
	/// </summary>
	public enum InvokeAction
	{
		/// <summary>
		/// An action was raised via the keyboard. 
		/// </summary>
		Keyboard,

		/// <summary>
		/// An action was raised via the mouse moving. 
		/// </summary>
		MouseMove,

		/// <summary>
		/// An action was raised via a MouseDown or Spacebar press.
		/// </summary>
		Click,

		/// <summary>
		/// An action was raised via the API.
		/// </summary>
		Code
	}
	#endregion // InvokeAction

	#region DragSelectType
	/// <summary>
	/// Describes the type of drag selection that should occur in the <see cref="XamGrid"/>
	/// </summary>
	public enum DragSelectType
	{
		/// <summary>
		/// A DragSelect operation shouldn'type occur.
		/// </summary>
		None,

		/// <summary>
		/// A Cell DragSelection should occur. 
		/// </summary>
		Cell,

		/// <summary>
		/// A Row DragSelection should occur. 
		/// </summary>
		Row
	}
	#endregion // DragSelectType

	#region FixedIndicatorDirection

	/// <summary>
	/// An enumeration that describes how a <see cref="Column"/> should be fixed when it's indicator is pressed.
	/// </summary>
	public enum FixedIndicatorDirection
	{
		/// <summary>
		/// Determines what side of the <see cref="XamGrid"/> a <see cref="Column"/>'s will be locked so that it 
		/// can'type be scrolled, when the pinned indicator is pressed
		/// </summary>
		Left,

		/// <summary>
		/// Determines what side of the <see cref="XamGrid"/> a <see cref="Column"/>'s will be locked so that it 
		/// can'type be scrolled, when the pinned indicator is pressed
		/// </summary>
		Right
	}

	#endregion // FixedIndicatorDirection

	#region FixedDropAreaLocation

	/// <summary>
	/// An enumeration that describes where a <see cref="Column"/> can be dragged to be fixed.
	/// </summary>
	public enum FixedDropAreaLocation
	{
		/// <summary>
		/// When a <see cref="Column"/> is dragged, a drop area will be displayed on the left side of the <see cref="XamGrid"/>
		/// that will allow the user to drop the column in order to lock it in place so that it can'type be scrolled.
		/// </summary>
		Left,

		/// <summary>
		/// When a <see cref="Column"/> is dragged, a drop area will be displayed on the right side of the <see cref="XamGrid"/>
		/// that will allow the user to drop the column in order to lock it in place so that it can'type be scrolled.
		/// </summary>
		Right,

		/// <summary>
		/// When a <see cref="Column"/> is dragged, a drop area will be displayed on both the left and right side of the <see cref="XamGrid"/>
		/// that will allow the user to drop the column in order to lock it in place so that it can'type be scrolled.
		/// </summary>
		Both
	}

	#endregion // FixedDropAreaLocation

	#region FixedColumnType

	/// <summary>
	/// An enumeration that describes if fixed columns are turned on, and if so then how it should be used.
	/// </summary>
	public enum FixedColumnType
	{
		/// <summary>
		/// Fixed columns are turned off.
		/// </summary>
		Disabled,

		/// <summary>
		/// Fixed columns are turned on and a <see cref="Column"/> can be fixed by clicking on an indicator.
		/// </summary>
		Indicator,

		/// <summary>
		/// Fixed columns are turned on and a <see cref="Column"/> can be fixed by dragging it to a designated are of the <see cref="XamGrid"/>
		/// </summary>
		DropArea,

		/// <summary>
		/// Fixed columns are turned on and a <see cref="Column"/> can be fixed by both indicator and drop area.
		/// </summary>
		Both
	}

	#endregion // FixedColumnType

	#region FixedState

	/// <summary>
	/// An enumeration that describes if a column is fixed, and if so on what side is it fixed.
	/// </summary>
	public enum FixedState
	{
		/// <summary>
		/// Column is not fixed.
		/// </summary>
		NotFixed,

		/// <summary>
		/// Column is fixed to the left.
		/// </summary>
		Left,

		/// <summary>
		/// Column is fixed to the right. 
		/// </summary>
		Right
	}

	#endregion // FixedState

	#region DropOperationType

	/// <summary>
	/// An enumeration that describes the type of operation that caused a Drop during a Column moving operation.
	/// </summary>
	public enum DropOperationType
	{
		/// <summary>
		/// Column has been moved.
		/// </summary>
		ColumnMoved,

		/// <summary>
		/// Column has been fixed.
		/// </summary>
		ColumnFixed,

		/// <summary>
		/// A fixed column has been moved. 
		/// </summary>
		FixColumnMoved,

		/// <summary>
		/// A Column has been Grouped
		/// </summary>
		ColumnGrouped,

		/// <summary>
		/// A Column's grouped index has changed.
		/// </summary>
		GroupedColumnIndexChanged
	}

	#endregion // DropOperationType

	#region DragCancelType

	/// <summary>
	/// An enumeration that describes why a drop operation was cancelled.
	/// </summary>
	public enum DragCancelType
	{
		/// <summary>
		/// Column has been unpinned.
		/// </summary>
		ColumnUnfixed,

		/// <summary>
		/// A column wasnt moved. 
		/// </summary>
		MoveCanceled,

		/// <summary>
		/// A column was ungrouped.
		/// </summary>
		ColumnUngrouped

	}

	#endregion // DragCancelType

	#region MouseEditingAction

	/// <summary>
	/// Describes the type of action that can cause a <see cref="Cell"/> to enter edit mode with a mouse.
	/// </summary>
	public enum MouseEditingAction
	{
		/// <summary>
		/// Clicking on a <see cref="Cell"/> will cause it to enter edit mode.
		/// </summary>
		SingleClick,

		/// <summary>
		/// Double clicking on a <see cref="Cell"/> will cause it to enter edit mode.
		/// </summary>
		DoubleClick,

		/// <summary>
		/// No mouse action will cause a <see cref="Cell"/> to enter edit mode.
		/// </summary>
		None
	}

	#endregion // MouseEditingAction

	#region EditingType

	/// <summary>
	/// Describes the different types of editing modes.
	/// </summary>
	public enum EditingType
	{
		/// <summary>
		/// Editing will not occur.
		/// </summary>
		None,

		/// <summary>
		/// All editing actions will put a single <see cref="Cell"/> into edit mode.
		/// </summary>
		Cell,

		/// <summary>
		/// All editing actions will put an entire <see cref="Row"/> into edit mode.
		/// </summary>
		Row,

        /// <summary>
        /// Hovering a cell will place either the <see cref="Row"/> or the <see cref="Cell"/> into edit mode, specified by the RowHover Property on the <see cref="XamGrid"/>
        /// </summary>
        Hover
	}

	#endregion // EditingType

	#region PagingLocation
	/// <summary>
	/// Enumeration describing where paging will be on the row island.
	/// </summary>
	public enum PagingLocation
	{
		/// <summary>
		/// PagingLocation is not enabled.
		/// </summary>
		None,
		/// <summary>
		/// PagingLocation is enabled and the pager is on top.
		/// </summary>
		Top,
		/// <summary>
		/// PagingLocation is enabled and the pager is on bottom.
		/// </summary>
		Bottom,
		/// <summary>
		/// PagingLocation is enabled and the pager is on the top and the bottom.
		/// </summary>
		Both,
		/// <summary>
		/// PagingLocation is enabled, without the <see cref="PagerRow"/> rendering. 
		/// </summary>
		Hidden
	}
	#endregion

	#region AddNewRowLocation

	/// <summary>
	/// Enumeration describing where the <see cref="AddNewRow"/> will be displayed on the row island.
	/// </summary>
	public enum AddNewRowLocation
	{
		/// <summary>
		/// The <see cref="AddNewRow"/> will not be displayed.
		/// </summary>
		None,
		/// <summary>
		/// The <see cref="AddNewRow"/> will be displayed at the top of the row island.
		/// </summary>
		Top,
		/// <summary>
		/// The <see cref="AddNewRow"/> will be displayed at the bottom of the row island.
		/// </summary>
		Bottom,
		/// <summary>
		/// The <see cref="AddNewRow"/> will be displayed at the top and the bottom of the row island.
		/// </summary>
		Both
	}

	#endregion // AddNewRowLocation

	#region ColumnResizingType

	/// <summary>
	/// Describes how Column Resizing will work in the <see cref="XamGrid"/>
	/// </summary>
	public enum ColumnResizingType
	{
		/// <summary>
		/// Column resizing is turned off.
		/// </summary>
		Disabled = 0,

		/// <summary>
		/// An Indicator will show where a column is sized.
		/// </summary>
		Indicator,

		/// <summary>
		/// Resizes a column as the mouse is moved.
		/// </summary>
		Immediate

	}

	#endregion // ColumnResizingType

	#region DropAreaIndicatorState
	/// <summary>
	/// An enumeration that describes the states that a DropAreaIndcator can be in. 
	/// </summary>
	public enum DropAreaIndicatorState
	{
		/// <summary>
		/// The indicator is currently being displayed on the left.
		/// </summary>
		Left,

		/// <summary>
		/// The indicator is currently being displayed on the right.
		/// </summary>
		Right,

		/// <summary>
		/// The indicator is curently being hidden.
		/// </summary>
		Hidden,

		/// <summary>
		/// The mouse is currently over the indicator.
		/// </summary>
		MouseOver
	}
	#endregion // DropAreaIndicatorState

	#region RowType

	/// <summary>
	/// An enumeration that contains all the different types of rows that the <see cref="XamGrid"/> contains.
	/// </summary>
	public enum RowType
	{
		/// <summary>
		/// A row that represents a record of data.
		/// </summary>
		DataRow,

		/// <summary>
		/// A row that represents the header of an island of rows.
		/// </summary>
		HeaderRow,

		/// <summary>
		/// A row that represents the footer of an island of rows.
		/// </summary>
		FooterRow,

		/// <summary>
		/// A row that represents a row that represents a grouped item.
		/// </summary>
		GroupByRow,

		/// <summary>
		/// A row that contains the pager.
		/// </summary>
		PagerRow,

		/// <summary>
		/// A row that represents the header of a <see cref="ColumnLayout"/>.
		/// </summary>
		ColumnLayoutHeaderRow,

		/// <summary>
		/// A row that represents the <see cref="AddNewRow"/>.
		/// </summary>
		AddNewRow,

		/// <summary>
		/// A row that represents the <see cref="GroupByAreaRow"/>.
		/// </summary>
		GroupByAreaRow,

		/// <summary>
		/// A row that represents the <see cref="FilterRow"/>
		/// </summary>
		FilterRow,

		/// <summary>
		/// A row that represents the <see cref="ColumnLayoutTemplateRow"/>.
		/// </summary>
		ColumnLayoutTemplateRow,

		/// <summary>
		/// A row that represents the <see cref="SummaryRow"/>.
		/// </summary>
		SummaryRow,

        /// <summary>
        /// A row that represents the <see cref="MergedSummaryRow"/>
        /// </summary>
        MergedSummaryRow
	}

	#endregion // RowType

	#region AllowFilterRow
	/// <summary>
	/// An enumeration that describes where the <see cref="FilterRow"/> will be located.
	/// </summary>
	[Obsolete("These values are obsolete.  They have been superceded by the FilterUIType enumeration.", false)]
	public enum FilterRowLocation
	{
		/// <summary>
		/// The <see cref="FilterRow"/> will not be displayed.
		/// </summary>
		None,
		/// <summary>
		/// The <see cref="FilterRow"/> will be displayed at the top of the row island.
		/// </summary>
		Top,
		/// <summary>
		/// The <see cref="FilterRow"/> will be displayed at the bottom of the row island.
		/// </summary>
		Bottom,
	}
	#endregion // AllowFilterRow

	#region FilterUIType

	/// <summary>
	/// An enumeration that describes which Filtering UI will be used.
	/// </summary>
	public enum FilterUIType
	{
		/// <summary>
		/// A Filtering UI will not be displayed.
		/// </summary>
		None,

		/// <summary>
		/// The <see cref="FilterRow"/> will be displayed at the top of the row island.
		/// The Filter Menu will not be used.
		/// </summary>
		FilterRowTop,

		/// <summary>
		/// The <see cref="FilterRow"/> will be displayed at the bottom of the row island.
		/// The Filter Menu will not be used.
		/// </summary>
		FilterRowBottom,

		/// <summary>
		/// The FilterMenu will be available in the HeaderCell. 
		/// The <see cref="FilterRow"/> will not be used.
		/// </summary>
		FilterMenu
	}

	#endregion // FilterUIType

	#region GroupByAreaLocation

	/// <summary>
	/// The location of the GroupByArea
	/// </summary>
	public enum GroupByAreaLocation
	{
		/// <summary>
		/// The GroupByArea will be hidden, however users will still be able to GroupBy a column via the API.
		/// </summary>
		Hidden,

		/// <summary>
		/// The GroupByArea will be aligned to the Top of the <see cref="XamGrid"/>
		/// </summary>
		Top,

		/// <summary>
		/// The GroupByArea will be aligned to the Bottom of the <see cref="XamGrid"/>
		/// </summary>
		Bottom
	}

	#endregion // GroupByAreaLocation

	#region RowHeightType
	/// <summary>
	/// An Enum that describes the type of height for the Row.
	/// </summary>
	public enum RowHeightType
	{
		/// <summary>
		/// A row's height will size to the tallest cell in the row. 
		/// Note: while scrolling the height of the row may grow as larger content comes into view, however,
		/// it will never decrease in height.
		/// </summary>
		SizeToLargestCell,

		/// <summary>
		/// A row's height will size to the tallest cell in the row, that is currently in view. 
		/// Note: while scrolling the height of the row may grow as larger  or smaller as content comes in and out of view
		/// </summary>
		Dynamic,

		/// <summary>
		/// A rows's height will size to the value specified. 
		/// </summary>
		Numeric
	}
	#endregion // RowHeightType

	#region DeferredScrollingType
	/// <summary>
	/// An Enum that describes the type of DeferredScrolling that the <see cref="XamGrid"/> will use.
	/// </summary>
	public enum DeferredScrollingType
	{
		/// <summary>
		/// DeferredScrolling will be disabled.
		/// </summary>
		None,

		/// <summary>
		/// A template will be displayed when using the thumb of the vertical scrollbar, and display the data of the row that would 
		/// be currently displayed at the very top of the <see cref="XamGrid"/>
		/// </summary>
		Default
	}
	#endregion // DeferredScrollingType

	#region FilteringScope

	/// <summary>
	/// An Enum that describes the type of Filtering that the <see cref="XamGrid"/> will use.
	/// </summary>
	public enum FilteringScope
	{
		/// <summary>
		/// Each child band of data is filtered independently.
		/// </summary>
		ChildBand,

		/// <summary>
		/// All rows which share a <see cref="ColumnLayout"/> will be filtered together.
		/// </summary>
		ColumnLayout
	}

	#endregion // FilteringScope

	#region SummaryScope

	/// <summary>
	/// An Enum that describes the type of summary that the <see cref="XamGrid"/> will use.
	/// </summary>
	public enum SummaryScope
	{
		/// <summary>
		/// Each child band of data is summed independently.
		/// </summary>
		ChildBand,
		/// <summary>
		/// All rows which share a <see cref="ColumnLayout"/> will be summed together.
		/// </summary>
		ColumnLayout
	}

	#endregion // SummaryScope

	#region DeleteKeyAction

	/// <summary>
	/// An Enum that describes the action that will take place when a user presses the Delete Key on the <see cref="XamGrid"/>
	/// </summary>
	public enum DeleteKeyAction
	{
		/// <summary>
		/// Nothing should happen when the Delete key is pressed.
		/// </summary>
		None,

		/// <summary>
		/// All selected rows should be deleted.
		/// </summary>
		DeleteSelectedRows,

		/// <summary>
		/// Only the <see cref="Row"/> of the ActiveCell should be deleted.
		/// </summary>
		DeleteRowOfActiveCell,

		/// <summary>
		/// Only the unique rows of the the selected cells should be deleted.
		/// </summary>
		DeleteRowsOfSelectedCells,

		/// <summary>
		/// The unique rows of the selected cells and all of the selected rows should be deleted.
		/// </summary>
		DeleteRowsOfSelectedCellsAndRows
	}

	#endregion // DeleteKeyAction

	#region RowHoverType

	/// <summary>
	/// An enum that describes the types of hovering that can occur when the mouse is over a Cell.
	/// </summary>
	public enum RowHoverType
	{
		/// <summary>
		/// A hover effect will only be applied to a cell
		/// </summary>
		Cell,

		/// <summary>
		/// A hover effect will be applied to all Cell objects in that Row.
		/// </summary>
		Row,

		/// <summary>
		/// No hover effect will be applied. 
		/// </summary>
		None
	}

	#endregion // RowHoverType

	#region MultiSortingKey
	/// <summary>
	/// An enum that describes which keyboard key will be used to designate multiple additive sorting.
	/// </summary>
	public enum MultiSortingKey
	{
		/// <summary>
		/// The Control (Ctrl) key will be used to designate multiple sorting.
		/// </summary>
		Control,

		/// <summary>
		/// The Shift key will be used to designate multiple sorting.
		/// </summary>
		Shift
	}
	#endregion // MultiSortingKey

	#region SummaryRowLocation
	/// <summary>
	/// An enumeration that describes where the <see cref="SummaryRow"/> will be located.
	/// </summary>
	public enum SummaryRowLocation
	{
		/// <summary>
		/// The <see cref="SummaryRow"/> will not be displayed.
		/// </summary>	
		None,
		/// <summary>
		/// The <see cref="SummaryRow"/> will be displayed at the top of the row island.
		/// </summary>
		Top,
		/// <summary>
		/// The <see cref="SummaryRow"/> will be displayed at the bottom of the row island.
		/// </summary>
		Bottom,
		/// <summary>
		/// The <see cref="SummaryRow"/> will be displayed at the top and the bottom of the row island.
		/// </summary>
		Both
	}
	#endregion // SummaryRowLocation

	#region StyleScope
	/// <summary>
	/// An enumeration that describes which style will be altered during conditional formatting.
	/// </summary>
	public enum StyleScope
	{
		/// <summary>
		/// The individual cell's style will be altered.
		/// </summary>
		Cell,

		/// <summary>
		/// The cell style for all cells in the row will be altered.
		/// </summary>
		Row
	}

	#endregion // StyleScope

	#region ValueType

	/// <summary>
	/// An enumeration that describes what an inputted value in a conditional formatting rule should be interpretted as.
	/// </summary>
	public enum ValueType
	{
		/// <summary>
		/// During processing of the rule, an value will be determined by the XamGrid.
		/// </summary>
		GeneratedValue,

		/// <summary>
		/// A user inputted value will be expected.
		/// </summary>
		Number,

		/// <summary>
		/// The percentage value based on the range of the data.
		/// </summary>
		Percent,

		/// <summary>
		/// The percentile value with respect to where the given value falls in the given set.
		/// </summary>
		Percentile
	}

	#endregion // ValueType

	#region IconGroupOperator

	/// <summary>
	/// An enumeration that describes what operand should be used for Icon based conditional formatting rules.
	/// </summary>
	public enum IconGroupOperator
	{
		/// <summary>
		/// The icon will be used if the value is greater than the inputted value.
		/// </summary>
		GreaterThan,

		/// <summary>
		/// The icon will be used if the value is greater than or equal to the inputted value.
		/// </summary>
		GreaterThanOrEqualTo
	}

	#endregion // IconGroupOperator

	#region IconRuleValueType

	/// <summary>
	/// An enumeration that describes what an inputted value in a conditional formatting rule should be interpretted as for icon rules.
	/// </summary>
	public enum IconRuleValueType
	{
		/// <summary>
		/// A user inputted value will be expected.
		/// </summary>
		Number,

		/// <summary>
		/// The percentage value based on the range of the data.
		/// </summary>
		Percent,

		/// <summary>
		/// The percentile value with respect to where the given value falls in the given set.
		/// </summary>
		Percentile
	}

	#endregion // IconRuleValueType

	#region DataBarPositiveNegative

	/// <summary>
	/// Enumeration that describes if the databar that is displayed should be considered a negative bar for styling.
	/// </summary>
	public enum DataBarPositiveNegative
	{
		/// <summary>
		/// The value is positive or should use the postive styling.
		/// </summary>
		Positive,

		/// <summary>
		/// The value is negative or shoulduse the negative styling
		/// </summary>
		Negative
	}

	#endregion // DataBarPositiveNegative

	#region DataBarDirection

	/// <summary>
	/// Enumeration that describes what direction the data bars will be drawn when using Conditional Formatting.
	/// </summary>
	public enum DataBarDirection
	{	
		/// <summary>
		/// A single directional bar will be drawn from the left edge to the right edge.
		/// </summary>
		UnidirectionalLeftToRight,

		/// <summary>
		/// A single directional bar will be drawn from the right edge to the left edge.
		/// </summary>
		UnidirectionalRightToLeft,

		/// <summary>
		/// The positive bar will be drawn from the center of the cell to the right, the negative bar will be drawn from the center to the left.
		/// </summary>
		Bidirectional
	}

	#endregion // DataBarDirection

	#region AverageFormattingTarget

	/// <summary>
	/// Enumeration describing which cells will have their style set in the AverageValueConditionalFormatRule.
	/// </summary>
	public enum AverageFormattingTarget
	{ 
		/// <summary>
		/// Values above the average will be styled.
		/// </summary>
		Above,

		/// <summary>
		/// Values equal or above the average will be styled.
		/// </summary>
		EqualOrAbove,

		/// <summary>
		/// Values below the average will be styled.
		/// </summary>
		Below,

		/// <summary>
		/// Values equal or below the average will be styled.
		/// </summary>
		EqualOrBelow
	}

	#endregion // AverageFormattingTarget

    #region AllowToolTips
    /// <summary>
    /// An enumeration that describes when tooltips should be displayed.
    /// </summary>
    public enum AllowToolTips
    {
        /// <summary>
        /// Tooltips should never be displayed.
        /// </summary>
        Never, 

        /// <summary>
        /// Tooltips should always be displayed.
        /// </summary>
        Always, 

        /// <summary>
        /// Tooltips should only be displayed if the content is clipped.
        /// </summary>
        Overflow
    }
    #endregion // AllowToolTips

    #region GridClipboardCopyOptions

    /// <summary>
    /// An Enum that defines how Ctrl-C should be handled on the <see cref="XamGrid"/>
    /// </summary>
    public enum GridClipboardCopyOptions
    {
        /// <summary>
        /// Copy to clipboard all selected items, but dont copy the corresponding headers.
        /// </summary>
        ExcludeHeaders,

        /// <summary>
        /// Copy to clipboard all selected items, and include the coresponding headers.
        /// </summary>
        IncludeHeaders
    }

    #endregion // GridClipboardCopyOptions

    #region GridClipboardCopyType

    /// <summary>
    /// An enum that describes what should be copied using Ctrl-c in the <see cref="XamGrid"/>
    /// </summary>
    public enum GridClipboardCopyType
    {
        /// <summary>
        /// Only copy selected cells to the clipboard. For this option, you need to have Cell Selection turned on.
        /// </summary>
        SelectedCells,

        /// <summary>
        /// Only copy selected rows to the clipboard. For this option, you need to have Row Selection turned on.
        /// </summary>
        SelectedRows, 

        /// <summary>
        /// Depending on CellClickAction property, selected cells or rows will be copied.
        /// </summary>
        Default
    }

    #endregion // GridClipboardCopyType

    #region GroupByOperation

    /// <summary>
    /// An enum that describes what type of operation should occur, when you groupby a column
    /// </summary>
    public enum GroupByOperation
    {
        /// <summary>
        /// A standard groupby operation will occur, where expandable rows will be used to show the grouped data.
        /// </summary>
        GroupByRows,

        /// <summary>
        /// The data will still appear flat, however cells in the grouped column that have the same data will be merged into one larger cell that spans
        /// all rows with that data.
        /// </summary>
        MergeCells,
    }

    #endregion // GroupByOperation

    #region FilterMenuCumulativeSelectionList

    /// <summary>
    /// An enum that describes how the selection list on the FilterMenu drop down will be populated.
    /// </summary>
    public enum FilterMenuCumulativeSelectionList
    {
        /// <summary>
        /// With <see cref="FilterMenuCumulativeSelectionList"/>.ExcelStyle, the list generated will be limited based on other filters which are applied.
        /// </summary>
        ExcelStyle,

        /// <summary>
        /// With <see cref="FilterMenuCumulativeSelectionList"/>.CompleteList, this list will have all unique values from the given ItemsSource.
        /// </summary>
        CompleteList
    }

    #endregion // FilterMenuCumulativeSelectionList

    #region EditorDisplayBehaviors

    /// <summary>
    /// An Enum that defines how Editor Control is rendered on the <see cref="XamGrid"/>
    /// </summary>
    public enum EditorDisplayBehaviors
    {
        /// <summary>
        /// Default behavior for the display of the Editor Control.
        /// </summary>
        Default,

        /// <summary>
        /// Editor Control is Always displayed and accessible.
        /// </summary>
        Always,

        /// <summary>
        /// Editor Control is only displayed in Edit Mode.
        /// </summary>
        EditMode
    }

    #endregion // EditorDisplayBehaviors    

    #region DateFilterObjectType
    /// <summary>
    /// An Enum for date filtering.   
    /// </summary>
    /// <remarks>
    /// In the DateFilterSelectionControl, there is a drop down which limits how the search is going to proceed.
    /// This enum can be used to limit the search over a given criteria.
    /// </remarks>
    public enum DateFilterObjectType
    {
        /// <summary>
        /// A special value designating that the value was not set.
        /// </summary>
        None,
        /// <summary>
        /// Searches over all parts of the date time field.
        /// </summary>
        All,
        /// <summary>
        /// Searches over the <see cref="DateTime.Year"/> part of the field.
        /// </summary>
        Year,
        /// <summary>
        /// Searches over the <see cref="DateTime.Month"/> of the field.
        /// </summary>
        Month,
        /// <summary>
        /// Searches over the <see cref="DateTime.Date"/> of the field.
        /// </summary>
        Date,
        /// <summary>
        /// Searches over the <see cref="DateTime.Hour"/> of the field.
        /// </summary>
        Hour,
        /// <summary>
        /// Searches over the <see cref="DateTime.Month"/> of the field.
        /// </summary>
        Minute,
        /// <summary>
        /// Searches over the <see cref="DateTime.Second"/> of the field.
        /// </summary>
        Second
    }
    #endregion // DateFilterObjectType

    #region ClipboardPasteErrorType
    /// <summary>
    /// An Enum for the different types of errors to be raised during excel like paste.
    /// </summary>
    public enum ClipboardPasteErrorType
    {
        /// <summary>
        /// Designates that the error happened because of a bad cast.
        /// </summary>
        CastError,
        /// <summary>
        /// Designates that the error happened because of an issue with truncation.
        /// </summary>
        TruncationError,
        /// <summary>
        /// Designates that the error happened because of data stored in the clipboard is not compatible to paste into the desired area.
        /// </summary>
        InvalidInputSelection,
        /// <summary>
        /// Designates that the error happened because the desired destination for pasting was invalid.
        /// </summary>
        InvalidDestinationSelection,
        /// <summary>
        /// Triggers to let the consumer know that paste operation is being denied due to a Read Only Column.
        /// </summary>
        ReadOnlyColumn,
        /// <summary>
        /// Triggers to let the consumer know that paste operation is being denied, due to an Unbound Column.
        /// </summary>
        UnboundColumn,
        /// <summary>
        /// Designates that an unknown Error happened while attempting to paste a value into a cell.
        /// </summary>
        UnknownError
    }
    #endregion // DateFilterObjectType

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