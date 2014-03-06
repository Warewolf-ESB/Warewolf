using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Windows.Threading;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Visual object for the <see cref="HeaderCell"/> object.
	/// </summary>
	[TemplateVisualState(GroupName = "CommonStates", Name = "Moving")]

	[TemplateVisualState(GroupName = "FixedStates", Name = "Unfixed")]
	[TemplateVisualState(GroupName = "FixedStates", Name = "Fixed")]

	[TemplateVisualState(GroupName = "FixedIndicatorStates", Name = "NotFixable")]
	[TemplateVisualState(GroupName = "FixedIndicatorStates", Name = "Pinned")]
	[TemplateVisualState(GroupName = "FixedIndicatorStates", Name = "Unpinned")]

	[TemplateVisualState(GroupName = "SortedStates", Name = "NotSorted")]
	[TemplateVisualState(GroupName = "SortedStates", Name = "Ascending")]
	[TemplateVisualState(GroupName = "SortedStates", Name = "Descending")]

	[TemplateVisualState(GroupName = "ActiveStates", Name = "Active")]
	[TemplateVisualState(GroupName = "ActiveStates", Name = "InActive")]

	[TemplateVisualState(GroupName = "SelectedStates", Name = "NotSelected")]
	[TemplateVisualState(GroupName = "SelectedStates", Name = "Selected")]

	[TemplateVisualState(GroupName = "FilteredStates", Name = "NoFiltering")]
	[TemplateVisualState(GroupName = "FilteredStates", Name = "NotFiltered")]
	[TemplateVisualState(GroupName = "FilteredStates", Name = "Filtered")]

	[TemplateVisualState(GroupName = "SummableStates", Name = "Summable")]
	[TemplateVisualState(GroupName = "SummableStates", Name = "NotSummable")]

	[TemplateVisualState(GroupName = "HeaderFilterAppliedStates", Name = "Summable")]
	[TemplateVisualState(GroupName = "HeaderFilterAppliedStates", Name = "NotSummable")]

	[TemplateVisualState(GroupName = "HeaderFilterStates", Name = "NoIcon")]
	[TemplateVisualState(GroupName = "HeaderFilterStates", Name = "FilterIcon")]
	public class HeaderCellControl : CellControlBase, ICommandTarget
	{
		#region Members

		DataTemplate _currentTemplate;
		ColumnMovingSettingsOverride _columnMovingSettings;
		FixedColumnSettingsOverride _fixedColumnSettings;
		GroupBySettingsOverride _groupBySettings;
		bool _mouseDown, _dragFixable, _columnMovingEnabled;
		int _originalIndex;
		bool _isDragging;
		bool _isDragObject;
		CellControlBase _dropHeader;
		HeaderCellControl _movingHeader;
		bool _dropBefore;
		Point _offsetPoint;
		Column _columnBeingSwitched;
		Animation _columnMovingAnimation;
		int _columnMovingNewIndex = -1;
		bool _dragGroupBy;
		Column _originalColumn;
		FrameworkElement _root;
		FrameworkElement _contentElement;
		ICollectionBase _cachedCollection;
        DispatcherTimer _scrollTimer;
        bool _scrollDirectionLeft;
		XamGrid _cachedGrid;
        bool _isMerged;

		#endregion // Members

		#region Constructor


        /// <summary>
        /// Static constructor for the <see cref="HeaderCellControl"/> class.
        /// </summary>
        static HeaderCellControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderCellControl), new FrameworkPropertyMetadata(typeof(HeaderCellControl)));
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="HeaderCellControl"/> class.
		/// </summary>
		public HeaderCellControl()
		{




            this._scrollTimer = new DispatcherTimer();
            this._scrollTimer.Tick += new EventHandler(ScrollTimer_Tick);
            this._scrollTimer.Interval = TimeSpan.FromMilliseconds(0);
		}       

		#endregion // Constructor

		#region Overrides

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected override void AttachContent()
		{
			this.EnsureContent();
		}
		#endregion // AttachContent

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		internal protected override void EnsureContent()
		{
			Column column = this.ResolveColumn();
			if (column != null)
			{
				bool notSet = true;

				string key = null;
				if (column.Key != null)
				{
					string[] keys = column.Key.Split('.');
					key = keys[keys.Length - 1];
				}

				if (column.HeaderTemplate != null)
				{
					if (this._currentTemplate != column.HeaderTemplate)
					{
						this._currentTemplate = column.HeaderTemplate;
						this._contentElement = column.HeaderTemplate.LoadContent() as FrameworkElement;						
					}

					this.Content = this._contentElement;
					this.DataContext = key;

					notSet = false;
				}
				else if (column.SupportsHeaderFooterContent)
				{
					DataTemplate template = this.Cell.Row.ColumnLayout.ColumnsHeaderTemplateResolved;

					if (template != null)
					{
						if (this._currentTemplate != template)
						{
							this._contentElement = template.LoadContent() as FrameworkElement;
						}

						this.Content = this._contentElement;
						this.DataContext = key;

						notSet = false;
					}
					this._currentTemplate = template;
				}

				if (notSet || this.Content == null)
				{
					if (string.IsNullOrEmpty(column.HeaderText) && column.DataField.DisplayName == null)
						this.Content = key;
					else
						this.Content = column.HeaderText;
				}

                this.HorizontalContentAlignment = column.HeaderTextHorizontalAlignmentResolved;
                this.VerticalContentAlignment = column.HeaderTextVerticalAlignmentResolved;
			}
		}

		#endregion // EnsureContent

		#region OnReleased
		/// <summary>
		/// Called when the <see cref="HeaderCell"/> releases the <see cref="HeaderCellControl"/>.
		/// </summary>
		protected internal override void OnReleased(CellBase cell)
		{
            if (!this._isDragging)
            {
                base.OnReleased(cell);

                this.DataContext = null;
            }
            else
            {
                this.DelayRecycling = true;
            }
                 
		}
		#endregion // OnReleased

		#region OnMouseEnter
		/// <summary>
		/// Called before the <see cref="UIElement.MouseEnter"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			if (this._mouseDown && !this._isDragging)
				this._mouseDown = false;

			base.OnMouseEnter(e);
		}
		#endregion // OnMouseEnter

		#region OnMouseLeftButtonDown_ColumnMoving

		/// <summary>
		/// Called before the <see cref="UIElement.MouseLeftButtonDown"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event</param>
		protected override bool OnMouseLeftButtonDownColumnMoving(MouseButtonEventArgs e)
		{
            if (this.Cell != null)
            {
                ColumnLayout layout = this.Cell.Row.ColumnLayout;
                this._columnMovingSettings = layout.ColumnMovingSettings;
                this._fixedColumnSettings = layout.FixedColumnSettings;
                this._groupBySettings = layout.GroupBySettings;

                FixedColumnType fixedType = this._fixedColumnSettings.AllowFixedColumnsResolved;
                this._dragFixable = (fixedType == FixedColumnType.Both || fixedType == FixedColumnType.DropArea) && this.Cell.Column.IsFixable;
                this._dragGroupBy = layout.Grid.GroupBySettings.AllowGroupByArea != GroupByAreaLocation.Hidden && (layout.Grid.GroupBySettings.ExpansionIndicatorVisibility == Visibility.Collapsed || layout.Grid.GroupBySettings.IsGroupByAreaExpanded) && layout.GroupBySettings.IsGroupableResolved && this.ResolveColumn().IsGroupable;

                this._columnMovingEnabled = (this._columnMovingSettings.AllowColumnMovingResolved != ColumnMovingType.Disabled) && this.Cell.Column.IsMovable;

                if (this._dragGroupBy && this.Cell.Column.IsGroupBy && layout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                    return false;

                if (!this._isDragObject && (this._columnMovingEnabled || this._dragFixable || this._dragGroupBy))
                {
                    this._mouseDown = true;
                    e.Handled = true;
                }

                if (this._mouseDown)
                    layout.Grid.SuspendConditionalFormatUpdates = true;
            }

			return this._mouseDown;
		}

		#endregion // OnMouseLeftButtonDown_ColumnMoving

		#region OnMouseMove_ColumnMoving

		/// <summary>
		/// Called before the <see cref="UIElement.MouseMove"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event</param>
		protected override void OnMouseMoveColumnMoving(MouseEventArgs e)
		{
			if (this._mouseDown)
			{
                if (this.Cell == null)
                {
                    this.EndDrag();
                    return;
                }

				this.Focus();
				XamGrid grid = this.Cell.Row.ColumnLayout.Grid;
				Panel gridPanel = grid.Panel;
				bool fullDrag = this._dragGroupBy;

				if (!this._isDragging)
				{
					if (!this.InitializeDrag(e))
						return;
				}

				// Grab the points of the grid
				Point rootPoint = e.GetPosition(null);
				Point elemPoint = e.GetPosition(gridPanel);






                Point p1 = e.GetPosition(grid.DropAreaIndicatorLeft);
                this.EnsureIndicatorState(grid.DropAreaIndicatorLeft, p1, true);
                Point p2 = e.GetPosition(grid.DropAreaIndicatorRight);
                this.EnsureIndicatorState(grid.DropAreaIndicatorRight, p2, false);


				Popup dragPopup = this._columnMovingSettings.ContentContainer;

				Column currentColumn = this.ResolveColumn();
				grid.OnColumnMoving(currentColumn, dragPopup.Child as HeaderCellControl, e);

				this._groupBySettings.IndicatorContainer.IsOpen = false;

				if (this._dragGroupBy)
				{
					GroupByAreaCellControl gbc = this.GetCellControlFromPoint(e.GetPosition(null), typeof(GroupByAreaCellControl), false) as GroupByAreaCellControl;
					CellControlBase ccb = grid.RowsManager.GroupByAreaRow.Cells[0].Control;
					if (ccb != null)
					{
						if (gbc != null)
						{
							ccb.GoToState("DraggingOver", false);
						}
						else
						{
							ccb.GoToState("Dragging", false);
						}
					}

				}

                Rect draggingElementBounds = GetBoundsForPopup(dragPopup, grid);

				// Check and see if there is another HeaderCellControl at the location of the element being dragged.
				this._dropHeader = this.ResolveDropHeader(rootPoint, draggingElementBounds);

                if (!this._isMerged || this is GroupByHeaderCellControl)
                {
                    if (this._dropHeader != null && this._dropHeader.Cell != null && this._dropHeader.Cell.Row != null && this._dropHeader.Cell.Row.ColumnLayout == this.Cell.Row.ColumnLayout)
                    {
                        this.ReactToMouseOverDroppableCellControl(e);
                    }
                    else
                    {
                        this._columnMovingSettings.IndicatorContainer.IsOpen = false;
                        this._dropHeader = null;
                    }
                }


                // Update the left position of the dragPopup

                if (SystemParameters.IsMenuDropRightAligned)
                {
                    dragPopup.HorizontalOffset = (elemPoint.X - this._offsetPoint.X + draggingElementBounds.Width);
                }
                else
                {
                    dragPopup.HorizontalOffset = (elemPoint.X - this._offsetPoint.X);
                }




				if (fullDrag)
					dragPopup.VerticalOffset = (elemPoint.Y - this._offsetPoint.Y);

                if (!this._isMerged || this is GroupByHeaderCellControl)
                {
                    if (this._columnMovingSettings.AllowColumnMovingResolved != ColumnMovingType.Disabled)
                    {
                        // Check to see if we're inside of the bounds of the xamGrid. if we aren't, then scroll the grid
                        if (elemPoint.X < 0)
                        {
                            this._scrollDirectionLeft = true;
                            this._scrollTimer.Start();
                        }
                        else if (elemPoint.X > grid.ActualWidth)
                        {
                            this._scrollDirectionLeft = false;
                            this._scrollTimer.Start();
                        }
                        else
                        {
                            this._scrollTimer.Stop();
                        }
                    }
                }
			}

			base.OnMouseMoveColumnMoving(e);

		}        

		#endregion // OnMouseMove_ColumnMoving

		#region OnMouseLeftButtonUp_ColumnMoving

		/// <summary>
		/// Called before the <see cref="UIElement.MouseLeftButtonUp"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event</param>
		protected override void OnMouseLeftButtonUpColumnMoving(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUpColumnMoving(e);
			this._mouseDown = false;
            Column col = this.ResolveColumn();
            if (col == null)
                return;
            if (col.ColumnLayout!=null)
            col.ColumnLayout.Grid.SuspendConditionalFormatUpdates = false;
			if (this._isDragging)
			{
				bool cancel = false;
				FixedState fixedState = col.IsFixed;
				XamGrid grid = this.Cell.Row.ColumnLayout.Grid;	
				DropOperationType type = DropOperationType.ColumnMoved;

				GroupByAreaCellControl gbc = this.GetCellControlFromPoint(e.GetPosition(null), typeof(GroupByAreaCellControl), false) as GroupByAreaCellControl;

				// Are we being dropped on a GroupByArea?
				if (gbc != null && (grid.GroupBySettings.ExpansionIndicatorVisibility == Visibility.Collapsed || grid.GroupBySettings.IsGroupByAreaExpanded) && col.IsGroupable)
				{
					GroupByColumnsCollection columnsCollection = grid.GroupBySettings.GroupByColumns;

					GroupByHeaderCellControl dropHeader = this._dropHeader as GroupByHeaderCellControl;

					if (dropHeader != null)
					{
						Column dropColumn = dropHeader.ResolveColumn();

						int dropIndex = columnsCollection.IndexOf(dropColumn);
						int currentIndex = columnsCollection.IndexOf(col);

						// Determine the index at which we are moving to. 
						if (!this._dropBefore)
							dropIndex++;

						if (currentIndex < dropIndex && currentIndex != -1)
							dropIndex--;

						ICollectionBase list = columnsCollection;
						if (dropIndex != list.IndexOf(col))
						{
							if (col.IsGroupBy)
								type = DropOperationType.GroupedColumnIndexChanged;
							else
								type = DropOperationType.ColumnGrouped;

							if (list.Contains(col))
								list.RemoveItemSilently(list.IndexOf(col));
							list.Insert(dropIndex, col);
						}
					}
					else if (!col.IsGroupBy)
					{
						col.IsGroupBy = true;
						type = DropOperationType.ColumnGrouped;
					}

					cancel = true;

				}// Are we over the Left DropArea?

                else if (HeaderCellControl.IsElementAtPoint(e.GetPosition(this.Cell.Row.ColumnLayout.Grid.DropAreaIndicatorLeft), this.Cell.Row.ColumnLayout.Grid.DropAreaIndicatorLeft))



				{
					type = DropOperationType.ColumnFixed;
					fixedState = FixedState.Left;
					cancel = true;

				}// Are we over the Right DropArea?

                else if (HeaderCellControl.IsElementAtPoint(e.GetPosition(this.Cell.Row.ColumnLayout.Grid.DropAreaIndicatorRight), this.Cell.Row.ColumnLayout.Grid.DropAreaIndicatorRight))



				{
					type = DropOperationType.ColumnFixed;
					fixedState = FixedState.Right;
					cancel = true;

				}// Is there a dropable column below us?
				else if (this._dropHeader != null)
				{
					// If ColumnMoving is of type Indicator, move the column
					if (this._columnMovingSettings.AllowColumnMovingResolved == ColumnMovingType.Indicator)
					{
						if (!this.MoveColumn())
						{
							this.CancelDrag(false, true);
							return;
						}
					}
				}
				else
				{
					Point mousePoint = e.GetPosition(grid);

					// Are we outside the bounds of the grid. If so, cancel the drag
					if (mousePoint.X < 0 || mousePoint.X > grid.ActualWidth)
					{
						cancel = true;
						fixedState = FixedState.NotFixed;
					}
                    else if (this.Cell.Row.Control == null) // 36195 - Added to address an issue, where the user uses the mouse wheel to scroll it out of view
                    {
                        cancel = true;
                    }
                    else
                    {
                        // Determine where we are, and whether or not we should cancel the drag.
                        Rect draggingElementBounds = GetBoundsForPopup(this._columnMovingSettings.ContentContainer, grid);
                        mousePoint = e.GetPosition(null);
                        mousePoint = new Point(mousePoint.X, draggingElementBounds.Y);

                        double zoomFactor = HeaderCellControl.GetZoomFactor();
                        mousePoint.Y /= zoomFactor;

                        // First Check and see if there is a droppable header at this location
                        // If there isn'type, then we might need to cancel.
                        if (this.GetCellControlFromPoint(mousePoint, null, true) == null)
                        {
                            // So, was there actually no droppable header
                            // Or was there nothing there at all (This is mainly for Immediate ColumnMoving) - where there would be nothing.
                            // If so, then we can definitely cancel. 
                            List<UIElement> elements = HeaderCellControl.GetElementsAtPoint(mousePoint, this.Cell.Row.Control);
                            if (elements.Count > 0)
                            {
                                bool cancelDrag = true;
                                bool isAuxColumn = false;

                                foreach (UIElement element in elements)
                                {
                                    HeaderCellControl hcc = element as HeaderCellControl;
                                    if (hcc != null && (hcc.Cell.Column is FillerColumn || hcc.Cell.Column is RowSelectorColumn || hcc.Cell.Column is ExpansionIndicatorColumn))
                                    {
                                        isAuxColumn = true;
                                        break;
                                    }
                                }

                                // If we're dealing with an aux/non-droppable column, don't even bother doing the other checks.
                                if (!isAuxColumn)
                                {
                                    // Need to do a few additional checks.
                                    // There might be an unppined header in the pinned header area thus giving us a false positive.
                                    // So now check to see if we're to the right or left of the fixed border area.
                                    if (col.IsFixed == FixedState.Right)
                                    {
                                        CellBase cell = this.Cell.Row.Cells[this.Cell.Row.Columns.FixedBorderColumnRight];
                                        if (cell != null && cell.Control != null)
                                        {
                                            Rect bounds = GetBoundsOfElement(cell.Control, null);
                                            if (bounds.X < mousePoint.X)
                                            {
                                                cancelDrag = false;
                                            }
                                        }
                                    }
                                    else if (col.IsFixed == FixedState.Left)
                                    {
                                        CellBase cell = this.Cell.Row.Cells[this.Cell.Row.Columns.FixedBorderColumnLeft];
                                        if (cell != null && cell.Control != null)
                                        {
                                            Rect bounds = GetBoundsOfElement(cell.Control, null);
                                            if (bounds.X > mousePoint.X)
                                            {
                                                cancelDrag = false;
                                            }
                                        }
                                    }
                                }

                                if (cancelDrag)
                                {
                                    cancel = true;
                                    fixedState = FixedState.NotFixed;
                                }
                            }
                        }
                    }
				}

				if (type == DropOperationType.ColumnMoved && col.IsFixed != FixedState.NotFixed)
					type = DropOperationType.FixColumnMoved;


				if (cancel)
				{
					bool fireEvent = true;
					if (type == DropOperationType.ColumnFixed || type == DropOperationType.ColumnGrouped || type == DropOperationType.GroupedColumnIndexChanged)
					{
						grid.OnColumnDropped(col, type, this.ResolveCollection().IndexOf(col), this._originalIndex);
						fireEvent = false;
					}

					this.CancelDrag(col.IsFixed != fixedState, fireEvent);
				}
				else
				{
					int index = this.ResolveCollection().IndexOf(col);
					if (index != this._originalIndex)
					{
						grid.OnColumnDropped(col, type, index, this._originalIndex);
						this.EndDrag();
					}
					else
						this.CancelDrag(false, true);
				}


				// We need to delay setting these properties, until the Cancel occurs, so that the correct collection is 
				// is pulled from this.ResolveCollection(), so that the original index is correctly set. 
				if (col.IsFixed != fixedState)
					col.IsFixed = fixedState;
			}
		}

		#endregion // OnMouseLeftButtonUp_ColumnMoving

		#region OnKeyDown

		/// <summary>
		/// Called before the <see cref="UIElement.KeyDown"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (this._isDragging)
			{
				if (e.Key == Key.Escape)
					this.CancelDrag(false, true);
				e.Handled = true;
			}
			base.OnKeyDown(e);
		}
		#endregion // OnKeyDown

		#region OnLostMouseCapture
		/// <summary>
		/// Called before the LostMouseCapture event is raised.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			// So, if a user was to right click, and the SL menu is invoked
			// it could steal focus from the mouse capture, and thus, we might not get notified of 
			// the mouse up, so we get stuck in limbo where we think the drag is still occuring, but its not. 
			this.EndDrag();

			base.OnLostMouseCapture(e);
		}
		#endregion // OnLostMouseCapture

		#region DetermineCursorHelper

		/// <summary>
		/// Determines if the cursor should be changed for column resizing.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="bounds"></param>
		/// <param name="xPosition"></param>
		/// <returns></returns>		
		protected override bool DetermineCursorHelper(Column column, Rect bounds, double xPosition)
		{
			return base.DetermineCursorHelper(column, bounds, xPosition) && !this.IsDragging;
		}
		#endregion // DetermineCursorHelper

		#endregion // Overrides

		#region Properites

		#region Public

		#region IsDragging
		/// <summary>
		/// Gets whether or not the <see cref="Column"/> is curently being dragged.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsDragging
		{
			get { return this._isDragging; }
		}
		#endregion // IsDragging

		#region MovingHeader
		/// <summary>
		/// Gets the visual indicator of a <see cref="Column"/> being moved.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public HeaderCellControl MovingHeader
		{
			get { return this._movingHeader; }
		}
		#endregion // MovingHeader

		#region Indicator
		/// <summary>
		///  Gets the drop indciator <see cref="Popup"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual Popup Indicator
		{
			get
			{
				if (this._columnMovingSettings != null)
					return this._columnMovingSettings.IndicatorContainer;
				return null;
			}
		}
		#endregion // Indicator

		#region AvailableSummariesOperands

		/// <summary>
		/// Gets the <see cref="SummaryOperandCollection"/> which is available from the <see cref="Column"/>.
		/// </summary>
		public SummaryOperandCollection AvailableSummariesOperands
		{
			get
			{
				CellBase c = this.Cell;
				SummaryOperandCollection collection = null;
				if (c != null && c.Column != null)
					collection = this.Cell.Column.SummaryColumnSettings.SummaryOperands;
				return collection;
			}
		}

		#endregion // AvailableSummariesOperands

		#endregion // Public

		#region Protected

		/// <summary>
		/// Gets the <see cref="CellControlBase"/> that we're currently dragging over.
		/// </summary>
		protected CellControlBase DropHeader
		{
			get
			{
				return this._dropHeader;
			}
		}

		#endregion // Protected

		#endregion // Properties

		#region Methods

		#region Protected

		#region SupportsCommand

		/// <summary>
		/// Returns if the object will support a given command type.
		/// </summary>
		/// <param propertyName="command">The command to be validated.</param>
		/// <returns>True if the object recognizes the command as actionable against it.</returns>
		protected virtual bool SupportsCommand(ICommand command)
		{
            if (command is ColumnChooserCommandBase || command is ShowColumnCommand || command is FilterMenuCommands)
                return true;
            else
			    return (command is ColumnCommandBase) && !this.AllowUserResizing;
		}
		#endregion // SupportsCommand

		#region  GetParameter
		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		protected virtual object GetParameter(CommandSource source)
		{
			Column c = null;

            if (source.Command is ColumnCommandBase)
                c = this.ResolveColumn();
            else if (source.Command is ColumnChooserCommandBase)
                return this.Cell.Column;
            else if (source.Command is FilterMenuCommands)
                return this.Cell;
            

			return c;
		}
		#endregion // GetParameter

		#region InitializeDrag

		/// <summary>
		/// Setups up everything that needs to occur to beging a Drag operation in the <see cref="HeaderCellControl"/>
		/// </summary>
		/// <param propertyName="e"></param>
		/// <returns>Returns false if the drag operation couldn'type be initialized.</returns>
		protected virtual bool InitializeDrag(MouseEventArgs e)
		{
			XamGrid grid = this.Cell.Row.ColumnLayout.Grid;
			RowsPanel gridPanel = grid.Panel;
			Column column = this.ResolveColumn();

			this._cachedGrid = grid;
			this._cachedCollection = this.ResolveCollection();

			// Resolve the RootElement of the Grid, we shouldn't assume its the Application.Current.RootVisual
			this._root = grid;
			FrameworkElement parent = VisualTreeHelper.GetParent(this._root) as FrameworkElement;
			while (parent != null)
			{
				this._root = parent;
				parent = VisualTreeHelper.GetParent(this._root) as FrameworkElement;
			}

			if (grid.OnColumnDragStart(column))
				return false;

            this._isMerged = (column.IsGroupBy && grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells);

			this._originalIndex = this.ResolveCollection().IndexOf(column);
			this._originalColumn = column;

			this.MarkColumnAsMoving();

			if (this._dragGroupBy)
			{
				gridPanel.Children.Add(this._groupBySettings.IndicatorContainer);
				CellControlBase ccb = grid.RowsManager.GroupByAreaRow.Cells[0].Control;
				if (ccb != null)
					ccb.GoToState("Dragging", false);
			}

			// Setup Globals
            this._originalColumn.IsDragging = true;
			this._isDragging = true;
			this._offsetPoint = e.GetPosition(this);			

			// Basically Clone this HeaderCellControl so that we have a moving indicator of what we're dragging.
			this._movingHeader = this.GenerateDragHeader();

			this._movingHeader._isDragObject = true;
			this._movingHeader.Width = this.ActualWidth;
			this._movingHeader.Height = this.ActualHeight;
			this._movingHeader.LayoutUpdated += new EventHandler(MovingHeader_LayoutUpdated);

			// Put the drag display object in a popup, parent it to the grid, and show it so that we have a visible indicator of the drag.
			Popup dragPopup = this._columnMovingSettings.ContentContainer;
			dragPopup.Child = this._movingHeader;
			this._movingHeader.IsHitTestVisible = false;
			gridPanel.Children.Add(dragPopup);
			dragPopup.IsOpen = true;

			// Calculate where this HeaderCellControl is inside of the Grid and set the Y coordinate of the 
			// popup, as the popup should only move horizontally, and never vertically.
			Rect bounds = HeaderCellControl.GetBoundsOfElement(this, gridPanel);
			dragPopup.VerticalOffset = bounds.Y;

			if (this._dragFixable)
			{
				FixedDropAreaLocation dropLocation = this._fixedColumnSettings.FixedDropAreaLocationResolved;

				double fixedRowHeight = 0;
				foreach (RowBase row in grid.Panel.FixedRowsTop)
				{
					if (row == this.Cell.Row)
						break;
					fixedRowHeight += row.ActualHeight;
				}

				double fixedRowBottomHeight = 0;
				foreach (RowBase row in grid.Panel.FixedRowsBottom)
				{
					fixedRowBottomHeight += row.ActualHeight;
				}

				if (dropLocation == FixedDropAreaLocation.Left || dropLocation == FixedDropAreaLocation.Both)
				{
					bool hideIndicator = (column.IsFixed == FixedState.Left);

					if (grid.DropAreaIndicatorLeft != null && !hideIndicator)
					{
						grid.DropAreaIndicatorLeft.Style = this.Cell.Row.ColumnLayout.FixedColumnSettings.FixedDropAreaLeftStyleResolved;
						grid.DropAreaIndicatorLeft.Height = grid.Panel.ActualHeight - fixedRowHeight - fixedRowBottomHeight;
						Collection<CellBase> cells = this.Cell.Row.Control.VisibleFixedLeftCells;
						double left = 0;
						if (cells.Count > 0)
						{
							CellBase lastFixedCell = cells[cells.Count - 1];
							Rect lastFixedCellBounds = LayoutInformation.GetLayoutSlot(lastFixedCell.Control);
							left = lastFixedCellBounds.X + lastFixedCellBounds.Width;
						}
						else
						{
							double indentation = this.Cell.Row.Manager.ResolveIndentation(this.Cell.Row);
							Rect rowBounds = LayoutInformation.GetLayoutSlot(this.Cell.Row.Control);
							left = rowBounds.X + indentation;
						}
						Canvas.SetLeft(grid.DropAreaIndicatorLeft, left);
						Canvas.SetTop(grid.DropAreaIndicatorLeft, fixedRowHeight);
						grid.DropAreaIndicatorLeft.DisplayFromLeft();
					}
				}

				if (dropLocation == FixedDropAreaLocation.Right || dropLocation == FixedDropAreaLocation.Both)
				{
					bool hideIndicator = (column.IsFixed == FixedState.Right);

					if (grid.DropAreaIndicatorRight != null && !hideIndicator)
					{
						grid.DropAreaIndicatorRight.Style = this.Cell.Row.ColumnLayout.FixedColumnSettings.FixedDropAreaRightStyleResolved;
						grid.DropAreaIndicatorRight.Height = grid.Panel.ActualHeight - fixedRowHeight - fixedRowBottomHeight;
						Collection<CellBase> cells = this.Cell.Row.Control.VisibleFixedRightCells;
						double right = gridPanel.ActualWidth - grid.DropAreaIndicatorRight.ActualWidth;
						right -= column.ColumnLayout.Columns.FillerColumn.ActualWidth;
						if (cells.Count > 0)
						{
							CellBase lastFixedCell = cells[cells.Count - 1];
							Rect lastFixedCellBounds = LayoutInformation.GetLayoutSlot(lastFixedCell.Control);
							right = lastFixedCellBounds.X - grid.DropAreaIndicatorRight.ActualWidth;
						}
						Canvas.SetLeft(grid.DropAreaIndicatorRight, right);
						Canvas.SetTop(grid.DropAreaIndicatorRight, fixedRowHeight);
						grid.DropAreaIndicatorRight.DisplayFromRight();
					}
				}
			}

            grid.InvalidateScrollPanel(false);

            // Make sure that nothing else can listen to the mouse now.
            this.CaptureMouse();

			return true;
		}

		#endregion // InitializeDrag

		#region CancelDrag

		/// <summary>
		/// Terminates the DragOperation, and resets the columns back to their original order.
		/// </summary>
		protected virtual void CancelDrag(bool columnUnfixed, bool fireEvent)
		{
			Column column = this.ResolveColumn();

		    // NZ 21 April 2012 - TFS108614 - If the drag is canceled because of grouping (w/ GroupByOperation=Merging)
            // we manually move the column to its fixed position and don't want to try to move it back to the original
            // index.
		    if (column.CachedParentColumn == null || column.ParentColumn == column.CachedParentColumn)
		    {
                this.MoveColumn(this._originalIndex);
		    }

			if (fireEvent)
			{
				DragCancelType type = (columnUnfixed) ? DragCancelType.ColumnUnfixed : DragCancelType.MoveCanceled;
				this.Cell.Row.ColumnLayout.Grid.OnColumnDragCanceled(column, type);
			}

			this.EndDrag();
		}
		#endregion // CancelDrag

		#region EndDrag

		/// <summary>
		/// Cleans up after the Drag operation has finished. 
		/// </summary>
		protected virtual void EndDrag()
		{

            this._scrollTimer.Stop();

			XamGrid grid = this._cachedGrid;

			if (grid == null)
				return;
			
			this._dropHeader = null;

            grid.InvalidateScrollPanel(false);

			this._columnMovingSettings.ContentContainer.IsOpen = false;

			if (this._movingHeader != null)
			{
				Panel p = grid.Panel;
				if (p != null)
				{
					p.Children.Remove(this._columnMovingSettings.ContentContainer);
					p.Children.Remove(this._columnMovingSettings.IndicatorContainer);
					p.Children.Remove(this._groupBySettings.IndicatorContainer);
				}

				this._originalColumn.IsMoving = false;
				this._movingHeader.LayoutUpdated -= new EventHandler(this.MovingHeader_LayoutUpdated);
				this._movingHeader = null;
			}

			if (this._dragGroupBy)
			{
				CellControlBase ccb = grid.RowsManager.GroupByAreaRow.Cells[0].Control;
				if (ccb != null)
					ccb.GoToState("NotDragging", false);
			}

			this._columnMovingSettings.IndicatorContainer.IsOpen = false;
			this._groupBySettings.IndicatorContainer.IsOpen = false;

            this._originalColumn.IsDragging = false;
			this._isDragging = false;
			this._mouseDown = false;

			this.ReleaseMouseCapture();

			if (grid.DropAreaIndicatorLeft != null)
				grid.DropAreaIndicatorLeft.Hide();

			if (grid.DropAreaIndicatorRight != null)
				grid.DropAreaIndicatorRight.Hide();

			grid.OnColumnDragEnded(this.ResolveColumn());

			this._originalColumn = null;
			this._cachedCollection = null;
			this._cachedGrid = null;

            if (this._columnMovingAnimation != null && this._columnMovingAnimation.IsPlaying && this._columnMovingAnimation.Time == 0)
            {
                this._columnMovingAnimation.Stop();
            }
		}
		#endregion // EndDrag

		#region MoveColumn

		/// <summary>
		/// Performs the actual moving of the columns.
		/// </summary>
		protected virtual bool MoveColumn()
		{
			Column currentColumn = this.ResolveColumn();
			Column dropColumn = this._dropHeader.Cell.Column;

			// Identify which Columns Collection is being updated. 
			IList columnsCollection = this.ResolveReadOnlyCollection();

			// Determine if the index that we'd be moving to, isn'type already the current index of the column being moved. 
			int dropIndex = columnsCollection.IndexOf(dropColumn);
			int newIndex = (this._dropBefore) ? dropIndex - 1 : dropIndex + 1;
			int currentIndex = columnsCollection.IndexOf(currentColumn);
			if (newIndex == currentIndex)
				return false;

			Column previousBeingSwitchedColumn = this._columnBeingSwitched;
			this._columnBeingSwitched = this._dropHeader.Cell.Column;
			this._columnBeingSwitched.MovingColumnsWidth = currentColumn.ActualWidth;
            
			if (this._columnBeingSwitched != previousBeingSwitchedColumn || this._columnMovingSettings.AllowColumnMovingResolved == ColumnMovingType.Indicator)
			{
				columnsCollection = this.ResolveCollection();
				dropIndex = columnsCollection.IndexOf(dropColumn);
				currentIndex = columnsCollection.IndexOf(currentColumn);

				// Determine the index at which we are moving to. 
				if (!this._dropBefore)
					dropIndex++;

				if (currentIndex < dropIndex)
					dropIndex--;


				this._columnMovingNewIndex = dropIndex;
				this._columnBeingSwitched.ReverseMove = (!this._dropBefore);

				if (this._columnMovingSettings.AllowColumnMovingResolved == ColumnMovingType.Indicator)
				{
					this.MoveColumn(this._columnMovingNewIndex);
                    this.Cell.Row.ColumnLayout.Grid.InvalidateScrollPanel(false);
				}
				else
				{
					// Setup the Animation. 
					if (this._columnMovingAnimation == null)
					{
						this._columnMovingAnimation = new Animation();
						this._columnMovingAnimation.Complete += new EventHandler(ColumnMovingAnimation_Complete);
						this._columnMovingAnimation.Tick += new EventHandler<AnimationEventArgs>(ColumnMovingAnimation_Tick);
					}

					this._columnMovingAnimation.Duration = this._columnMovingSettings.AnimationDurationResolved;
					this._columnMovingAnimation.EasingFunction = this._columnMovingSettings.EasingFunctionResolved;

					if (this._columnMovingAnimation.IsPlaying)
					{
						if (previousBeingSwitchedColumn != this._columnBeingSwitched)
						{
							this._columnMovingAnimation.Stop();
							previousBeingSwitchedColumn.PercentMoved = 0;
						}
					}

                    this._columnMovingAnimation.Play();
				}
			}            
			return true;
		}
		#endregion // MoveColumn

		#region  ResolveColumn

		/// <summary>
		/// Resolves the underlying <see cref="Column"/> that should be references when checking properties such as 
		/// IsMoveable, IsGroupable, etc...
		/// </summary>
		/// <returns></returns>
		protected virtual Column ResolveColumn()
		{
			return this.Column;
		}
		#endregion // ResolveColumn

		#region GenerateDragHeader
		/// <summary>
		/// Generates a new <see cref="HeaderCellControl"/> that will be dragged around for moving operations.
		/// </summary>
		/// <returns></returns>
		protected virtual HeaderCellControl GenerateDragHeader()
		{
			HeaderCellControl hcc = new HeaderCellControl();
			hcc.OnAttached(this.Cell);
			return hcc;
		}
		#endregion // GenerateDragHeader

		#region ResolveDropHeader

		/// <summary>
		/// Looks for a <see cref="CellControlBase"/> under the current mouse position.
		/// </summary>
		/// <param propertyName="rootPoint"></param>
		/// <param propertyName="draggingElementBounds"></param>
		/// <returns></returns>
		protected virtual CellControlBase ResolveDropHeader(Point rootPoint, Rect draggingElementBounds)
		{
			// Check and see if there is another HeaderCellControl at the location of the element being dragged.
			if (this._dragGroupBy)
			{
				CellControlBase ctrl = this.GetCellControlFromPoint(rootPoint, null, false);
				if (ctrl == null)
					ctrl = this.GetCellControlFromPoint(rootPoint, typeof(GroupByHeaderCellControl), false);
				return ctrl;
			}
			else
				return this.GetCellControlFromPoint(new Point(rootPoint.X, draggingElementBounds.Y), null, true);
		}

		#endregion // ResolveDropHeader

		#region ReactToMouseOverDroppableHeader

		/// <summary>
		/// Allows the <see cref="HeaderCellControl"/> to determine how to react when the mouse is over a <see cref="CellControlBase"/>
		/// </summary>
		/// <param propertyName="e"></param>
		protected virtual void ReactToMouseOverDroppableCellControl(MouseEventArgs e)
		{
			XamGrid grid = this.Cell.Row.ColumnLayout.Grid;
			Panel gridPanel = grid.Panel;
			Point elemPoint = e.GetPosition(gridPanel);

			Rect dropHeaderBounds = HeaderCellControl.GetBoundsOfElement(this._dropHeader, gridPanel);
			Rect currentHeaderBounds = HeaderCellControl.GetBoundsOfElement(this, gridPanel);

			// Determine where we're going to need drop the column.
			this._dropBefore = (elemPoint.X <= (this._dropHeader.ActualWidth / 2) + dropHeaderBounds.X);

			if (this._dropHeader is GroupByHeaderCellControl)
				this.ReactToMouseOverDroppableGroupByHeader(e, dropHeaderBounds, currentHeaderBounds);
			else
			{
				if (this._columnMovingEnabled)
					this.ReactToMouseOverDroppableHeader(e, dropHeaderBounds, currentHeaderBounds);
			}
		}

		#endregion // ReactToMouseOverDroppableHeader

		#region ReactToMouseOverDroppableGroupByHeader

		/// <summary>
		/// Allows the control to perform actions such as displaying an indicator, when the mouse is over a <see cref="GroupByHeaderCellControl"/>
		/// </summary>
		/// <param propertyName="e"></param>
		/// <param propertyName="dropHeaderBounds"></param>
		/// <param propertyName="currentHeaderBounds"></param>
		protected virtual void ReactToMouseOverDroppableGroupByHeader(MouseEventArgs e, Rect dropHeaderBounds, Rect currentHeaderBounds)
		{
			GroupByHeaderCellControl dropHeader = (GroupByHeaderCellControl)this._dropHeader;
			ColumnLayout layout = this.Cell.Row.ColumnLayout;

			int dropHeaderLevel = GroupByAreaPanel.GetLevel(dropHeader);
			string dropHeaderLevelKey = GroupByAreaPanel.GetLevelKey(dropHeader);

			if (layout.Level == dropHeaderLevel && layout.Key == dropHeaderLevelKey)
			{
				double horizontalOffset = (this._dropBefore) ? dropHeaderBounds.X : dropHeaderBounds.X + this._dropHeader.ActualWidth;
				Popup columnMovingIndicator = layout.GroupBySettings.IndicatorContainer;
				columnMovingIndicator.IsOpen = true;
				layout.GroupBySettings.Indicator.Height = this._dropHeader.ActualHeight;
				columnMovingIndicator.VerticalOffset = dropHeaderBounds.Y;


			    if (SystemParameters.IsMenuDropRightAligned)
			    {
			        Rect indicatorBounds = GetBoundsForPopup(columnMovingIndicator, this);
			        horizontalOffset += indicatorBounds.Width;
			    }


				columnMovingIndicator.HorizontalOffset = horizontalOffset + layout.GroupBySettings.Indicator.HorizontalOffset;
			}
		}

		#endregion // ReactToMouseOverDroppableGroupByHeader

		#region ReactToMouseOverDroppableHeader

		/// <summary>
		/// Allows the control, to perform the appropriate function, when the mouse is over a droppable area.
		/// </summary>
		/// <param propertyName="e"></param>
		/// <param propertyName="dropHeaderBounds"></param>
		/// <param propertyName="currentHeaderBounds"></param>
		protected virtual void ReactToMouseOverDroppableHeader(MouseEventArgs e, Rect dropHeaderBounds, Rect currentHeaderBounds)
		{
			Column currentColumn = this.ResolveColumn();

			// If we're dealing with Fixed Right Columns, their indexes are in reverse. 
			if (currentColumn.IsFixed == FixedState.Right)
				this._dropBefore = !this._dropBefore;

			// If we're moving immediately. lets do so. 
			if (this._columnMovingSettings.AllowColumnMovingResolved == ColumnMovingType.Immediate)
				this.MoveColumn();
			else if (this._columnMovingSettings.AllowColumnMovingResolved == ColumnMovingType.Indicator)
			{
				double horizontalOffset = (this._dropBefore) ? dropHeaderBounds.X : dropHeaderBounds.X + this._dropHeader.ActualWidth;

                this._columnMovingSettings.Indicator.Height = this.ActualHeight;

				Popup columnMovingIndicator = this._columnMovingSettings.IndicatorContainer;
				columnMovingIndicator.IsOpen = true;

                // If DelayRecycling, it means the dragging HeaderCellControl is no longer in view, so use the DropHeader to find the Y bounds instead.
                if(this.DelayRecycling)
                    columnMovingIndicator.VerticalOffset = dropHeaderBounds.Y;
                else
				    columnMovingIndicator.VerticalOffset = currentHeaderBounds.Y;


                if (SystemParameters.IsMenuDropRightAligned)
                {
                    Rect indicatorBounds = GetBoundsForPopup(columnMovingIndicator, this);
                    horizontalOffset += indicatorBounds.Width;
                }


                columnMovingIndicator.HorizontalOffset = horizontalOffset + this._columnMovingSettings.Indicator.HorizontalOffset;
			}
		}
		#endregion // ReactToMouseOverDroppableHeader

		#region GetBoundsOfElement
		/// <summary>
		/// Returns the bounding rectangle of a element.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <param propertyName="relativeElement"></param>
		/// <returns></returns>
		protected internal static Rect GetBoundsOfElement(FrameworkElement element, UIElement relativeElement)
		{

            if (relativeElement == null)
                relativeElement = PlatformProxy.GetRootParent(element) as UIElement;            


            GeneralTransform t = null;

            try
            {

                t = element.TransformToVisual(relativeElement);

            }
            catch
            {
                Popup popup = PlatformProxy.GetPopupRootVisual(element);
                if (popup != null)
                    t = element.TransformToVisual(popup.Child);
            }

			MatrixTransform mt = t as MatrixTransform;
			
			if (mt != null)
			{
				Matrix matrix = mt.Matrix;
                return new Rect(matrix.OffsetX, matrix.OffsetY, element.ActualWidth * Math.Abs(matrix.M11), element.ActualHeight * Math.Abs(matrix.M22));
			}

			return t.TransformBounds(LayoutInformation.GetLayoutSlot(element));
		}
		#endregion // GetBoundsOfElement

		#region GetCellControlFromPoint
		/// <summary>
		/// Given a point, and a Type, the visual tree walks that point looking for any control, whose type matches the specified type.
		/// </summary>
		/// <param propertyName="point"></param>
		/// <param propertyName="type"></param>
		/// <returns></returns>
		protected CellControlBase GetCellControlFromPoint(Point point, Type type, bool applyZoom)
		{
			UIElement root = this._root;
			if (this._root == null)
				root = PlatformProxy.GetRootVisual(this);

            if (applyZoom)
            {
                double zoomFactor = HeaderCellControl.GetZoomFactor();

                point.Y = Math.Ceiling(point.Y / zoomFactor);
            }

			List<UIElement> elementsAtPoint = HeaderCellControl.GetElementsAtPoint(point, root);
			if (elementsAtPoint.Count > 0)
			{
				foreach (UIElement elem in elementsAtPoint)
				{
					CellControlBase hcc = elem as CellControlBase;
					if (hcc != null && hcc != this)
					{
						Column currentColumn = this.ResolveColumn();
                        Column column = hcc.Column;

                        if (column.ColumnLayout != null && 
                            this.Cell.Row != null &&
                            this.Cell.Row.ColumnLayout != null &&
                            column.ColumnLayout.Grid == this.Cell.Row.ColumnLayout.Grid)
                        {
	                        if (type == null)
	                        {
		                        if (column.IsMovable)
		                        {
			                        if (!(currentColumn.IsFixed == FixedState.NotFixed))
			                        {
				                        if (column.IsFixed == currentColumn.IsFixed)
				                        {
					                        return hcc;
				                        }
			                        }
			                        else if (this.ResolveReadOnlyCollection().IndexOf(column) > -1)
			                        {
				                        return hcc;
			                        }
		                        }
	                        }
	                        else if (hcc.GetType() == type)
	                        {
		                        return hcc;
	                        }
                        }
					}
				}
			}
			return null;
		}
		#endregion // GetCellControlFromPoint

		#region MarkColumnAsMoving
		/// <summary>
		/// Allows the <see cref="HeaderCellControl"/> to react to apply settings regarding marking the column has moving.
		/// </summary>
		protected virtual void MarkColumnAsMoving()
		{
			Column column = this.ResolveColumn();
			if (this._columnMovingEnabled)
			{
				if (this._columnMovingSettings.AllowColumnMovingResolved == ColumnMovingType.Immediate)
					column.IsMoving = true;
				else
				{
					this.Cell.Row.ColumnLayout.Grid.Panel.Children.Add(this._columnMovingSettings.IndicatorContainer);

					this._columnMovingSettings.IndicatorContainer.IsOpen = true;
				}
			}
		}
		#endregion // MarkColumnAsMoving

		#endregion // Protected

		#region Private

        private static Rect GetBoundsForPopup(Popup popup, UIElement relative)
        {




            FrameworkElement elem = (FrameworkElement)popup.Child;
            UIElement rootVis = PlatformProxy.GetRootVisual(relative);
            Point p = popup.TranslatePoint(new Point(popup.HorizontalOffset, popup.VerticalOffset), rootVis);
            return new Rect(p.X, p.Y, elem.ActualWidth, elem.ActualHeight);

        }

		private static List<UIElement> GetElementsAtPoint(Point p, UIElement realtiveElement)
		{
            return new List<UIElement>(PlatformProxy.GetElementsFromPoint(p, realtiveElement));
		}

		private static bool IsElementAtPoint(Point p, FrameworkElement element)
		{
			return (HeaderCellControl.GetElementsAtPoint(p, element).Count > 0);
		}

		private void EnsureIndicatorState(DropAreaIndicator indicator, Point p, bool left)
		{
			Column column = this.Cell.Column;
			if (this._dragFixable && indicator != null)
			{
				FixedState state = (left) ? FixedState.Left : FixedState.Right;
				FixedDropAreaLocation location = (left) ? FixedDropAreaLocation.Left : FixedDropAreaLocation.Right;

				bool hide = column.IsFixed == state;
				FixedDropAreaLocation dropLocation = this._fixedColumnSettings.FixedDropAreaLocationResolved;
				if (dropLocation != location && dropLocation != FixedDropAreaLocation.Both)
					hide = true;

				if (hide)
					indicator.Hide();
				else if (HeaderCellControl.IsElementAtPoint(p, indicator))
					indicator.DisplayMouseOver();
				else if (left)
					indicator.DisplayFromLeft();
				else
					indicator.DisplayFromRight();
			}
		}

		private ICollectionBase ResolveCollection()
		{
			Column currentColumn = this.Cell.Column;

			ICollectionBase collection = null;

			if (this.Cell.Row.ColumnLayout != null)
			{
                if (currentColumn.ParentColumn != null)
                {
                    collection = currentColumn.ParentColumn.ResolveChildColumns();
                }
                else
                {
                    if (currentColumn.IsFixed == FixedState.NotFixed)
                        collection = this.Cell.Row.Columns;
                    else if (currentColumn.IsFixed == FixedState.Left)
                        collection = this.Cell.Row.Columns.FixedColumnsLeft;
                    else if (currentColumn.IsFixed == FixedState.Right)
                        collection = this.Cell.Row.Columns.FixedColumnsRight;
                }
			}
			else
				collection = this._cachedCollection;

			return collection;
		}

		private IList ResolveReadOnlyCollection()
		{
			Column currentColumn = this.Cell.Column;

			IList collection = null;

            if (currentColumn.ParentColumn != null)
            {
                collection = currentColumn.ParentColumn.ResolveChildColumns();
            }
            else
            {
                if (currentColumn.IsFixed == FixedState.NotFixed)
                    collection = this.Cell.Row.Columns.VisibleColumns;
                else if (currentColumn.IsFixed == FixedState.Left)
                    collection = this.Cell.Row.Columns.FixedColumnsLeft;
                else if (currentColumn.IsFixed == FixedState.Right)
                    collection = this.Cell.Row.Columns.FixedColumnsRight;
            }

			return collection;
		}

		private void MoveColumn(int index)
		{
			Column col = this._originalColumn;

			ICollectionBase list = this.ResolveCollection();
            int colIndex = list.IndexOf(col);
			if (index != colIndex)
			{
				col.IsMoving = true;
                if (list is FixedColumnsCollection)
                {
                    list.RemoveItemSilently(colIndex);
                    list.AddItemSilently(index, col);
                }
                else
                {
                    list.RemoveAt(colIndex);
                    list.Insert(index, col);
                }
			}
		}

		private static double GetZoomFactor()
		{
			double zoomFactor = 1;
			try
			{
				zoomFactor = PlatformProxy.GetZoomFactor();
			}
			catch
			{
			}
			return zoomFactor;
		}

		#endregion // Private

		#endregion // Methods

		#region EventHandlers

		#region ColumnMovingAnimation_Tick

        void ColumnMovingAnimation_Tick(object sender, AnimationEventArgs e)
        {
            if (this._columnBeingSwitched != null && this.Cell != null)
            {
                this._columnBeingSwitched.PercentMoved = e.Value;
                if (this._columnMovingNewIndex != -1)
                {
                    this.MoveColumn(this._columnMovingNewIndex);
                    this._columnMovingNewIndex = -1;
                }
                else
                    this.Cell.Row.ColumnLayout.Grid.InvalidateScrollPanel(false);
            }
        }
		#endregion // ColumnMovingAnimation_Tick

		#region ColumnMovingAnimation_Complete

		void ColumnMovingAnimation_Complete(object sender, EventArgs e)
		{
			if (this._columnBeingSwitched != null)
			{
				this._columnBeingSwitched.PercentMoved = 0;
				this._columnBeingSwitched = null;
			}
		}
		#endregion // ColumnMovingAnimation_Complete

		#region MovingHeader_LayoutUpdated

		void MovingHeader_LayoutUpdated(object sender, EventArgs e)
		{
			if (this._movingHeader != null)
				this._movingHeader.GoToState("Moving", true);
		}

		#endregion // MovingHeader_LayoutUpdated

        #region EventHandlers

        void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (this._cachedGrid != null)
            {
                ScrollBar hbar = ((IProvideScrollInfo)this._cachedGrid).HorizontalScrollBar;
                if (hbar.Visibility == System.Windows.Visibility.Visible && hbar.Maximum > 0)
                {
                    if (this._scrollDirectionLeft)
                        hbar.Value -= .1;
                    else
                        hbar.Value += .1;

                    this._cachedGrid.InvalidateScrollPanel(false);
                }
            }
        }

        #endregion // EventHandlers

        #endregion // EventHandlers

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return this.SupportsCommand(command);
		}

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return this.GetParameter(source);
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