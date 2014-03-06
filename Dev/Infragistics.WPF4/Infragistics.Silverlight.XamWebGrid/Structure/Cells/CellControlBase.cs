using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// The base control class for all <see cref="CellBase"/> objects.
    /// </summary>
	[TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
	[TemplateVisualState(GroupName = "CommonStates", Name = "Alternate")]
	[TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
	public abstract class CellControlBase : ContentControl, IRecyclableElement, INotifyPropertyChanged
	{
		#region Members
		CellBase _cell;
		bool _isResizing;
        CellsPanel _panel;

		#region ColumnResizing Members

		Collection<Column> _resizedColumns;
		Column _resizedColumn;
		Point _dragStartPoint;
		double _originalWidthValue;
		ColumnWidth? _originalWidth;
		double _currentWidth;

		#endregion // ColumnResizing Members

		#endregion // Members

		#region Constructor


        static CellControlBase()
        {
            Style style = new Style();
            style.Seal();
            Control.FocusVisualStyleProperty.OverrideMetadata(typeof(CellControlBase), new FrameworkPropertyMetadata(style));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="CellControlBase"/> class.
		/// </summary>
		protected CellControlBase()
		{
			this.Loaded += new RoutedEventHandler(CellControlBase_Loaded);
		}

		#endregion // CellControlBase

		#region Properties

		#region Public

		#region ContentProvider
		/// <summary>
		/// Resolves the <see cref="ColumnContentProviderBase"/> for this <see cref="CellControl"/>.
		/// </summary>
		public virtual ColumnContentProviderBase ContentProvider
		{
			get { return null; }
		}

		#endregion // ContentProvider

        #region Cell

        /// <summary>
        /// The <see cref="CellBase"/> that owns the <see cref="CellControlBase"/>.
        /// </summary>
        public CellBase Cell
        {
            get 
            {
                if (this._cell != null)
                {
                    // A cellcontrol will only ever belong to a single rowcontrol, so if we have a panel, store it off. 
                    this._panel = this._cell.Row.Control;

                    return this._cell;
                }
                else if (this.Column != null && this._panel != null && this._panel.Row != null)
                {
                    return this._panel.Row.Cells[Column];
                }
                return null;
            }
            set
            {
                if (this._cell != value)
                {
                    this._cell = value;
                    this.OnPropertyChanged("Cell");
                }
            }
        }
        #endregion // Cell

        #region Column

        /// <summary>
        ///  Gets the Column that this <see cref="CellControlBase"/> represents. 
        /// </summary>
        public Column Column
        {
            get;
            private set;
        }
        #endregion //Column

        #region ResizingThreshold

        /// <summary>
        /// Identifies the <see cref="ResizingThreshold"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ResizingThresholdProperty = DependencyProperty.Register("ResizingThreshold", typeof(int), typeof(CellControlBase), new PropertyMetadata((int)5, new PropertyChangedCallback(ResizingThresholdChanged)));

        /// <summary>
        /// Gets or sets the size of the area for which the resizing indicator will be displayed.
        /// </summary>
        /// <value>
        /// The resizing threshold.
        /// </value>
        public int ResizingThreshold
        {
            get { return (int)this.GetValue(ResizingThresholdProperty); }
            set { this.SetValue(ResizingThresholdProperty, value); }
        }

        private static void ResizingThresholdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion // ResizingThreshold 

        #endregion // Public

        #region Protected

        #region AllowUserResizing
        /// <summary>
		/// Internal flag used to get / set if the user's mouse is in position to allow for drag resizing of a column.
		/// </summary>
		/// <remarks>True if we determine the column is in a resize area and that the column allows resizing. This already takes into account if the column itself allows for resizing	</remarks>
		protected internal bool AllowUserResizing
		{
			get;
			set;
		}
		#endregion // AllowUserResizing

		#region IsResizing
		/// <summary>
		///  Internal flag that gets / sets when the mouse is down and the column is being resized.
		/// </summary>
		protected internal bool IsResizing
		{
			get
			{
				return this._isResizing;
			}
			set
			{
				this._isResizing = value;
				if (this.Cell != null && this.Cell.Column != null && this.Cell.Column.ColumnLayout != null && this.Cell.Column.ColumnLayout.Grid != null)
					this.Cell.Column.ColumnLayout.Grid.IsColumnResizing = value;
			}
		}
		#endregion // IsResizing

		#region ShowResizingArrow
		/// <summary>
		/// Gets if a resizing arrow should be shown.
		/// </summary>
		protected virtual bool ShowResizingArrow
		{
			get
			{
				return true;
			}
		}
		#endregion // ShowResizingArrow

		#region DelayRecycling
		/// <summary>
		/// Get / set if the object should be recycled.
		/// </summary>
		protected virtual bool DelayRecycling
		{
			get;
			set;
		}
		#endregion // DelayRecycling

        #region OwnerPanel

        /// <summary>
        /// Gets/sets the <see cref="CellsPanel"/> that owns this element. 
        /// </summary>
        protected virtual Panel OwnerPanel
        {
            get { return this._panel; }
            set
            {
                CellsPanel cp = value as CellsPanel;
                if(cp != null)
                    this._panel = cp;

                if (value == null)
                    this.Column = null;
            }
        }

        #endregion // OwnerPanel

        #region ValidateWidthInArrange
        /// <summary>
		/// Gets whether the Control should validate it's width in ArrangeOverride. 
		/// </summary>
		protected virtual bool ValidateWidthInArrange
		{
			get { return true; }
		}
		#endregion // ValidateWidthInArrange

		#endregion // Protected

        #region Internal

        internal bool MeasureRaised
        {
            get;
            set;
        }

        internal bool IsCellLoaded
        {
            get;
            set;
        }

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Protected

        #region OnAttached

        /// <summary>
		/// Called when the <see cref="CellBase"/> is attached to the <see cref="CellControlBase"/>.
		/// </summary>
		/// <param propertyName="cell">The <see cref="CellBase"/> that is being attached to the <see cref="CellControlBase"/></param>
		protected internal virtual void OnAttached(CellBase cell)
		{
            // NZ 11 May 2012 - TFS103567
            this.Cell = cell;
            this.Column = this._cell.Column;

            // A new cell is being attached. Make sure our DataContext is clear, so that we can properly inherit it from the CellsPanel.
            // Cell's such as UnboundCells set the DataContext themselves, so we shouldn't clear it, otherwise we'll get binding errors in the output window.
            if(cell.ShouldClearDataContext)
                this.ClearValue(CellControlBase.DataContextProperty);

			this.AttachContent();
		}

		#endregion // OnAttached

        #region OnReleasing

        /// <summary>
        /// Invoked when a <see cref="CellControlBase"/> is being released from an object.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>False, if the control shouldn't be released.</returns>
        protected internal virtual bool OnReleasing(CellBase cell)
        {
            return !this.IsResizing;
        }

        #endregion // OnReleasing

        #region OnReleased

        /// <summary>
		/// Called when the <see cref="CellBase"/> releases the <see cref="CellControlBase"/>.
		/// </summary>
		protected internal virtual void OnReleased(CellBase cell)
		{
			if (!this.IsResizing)
			{
				ReleaseObjectForRecycling(cell);

                // Setting this to null will break our inheritance chain to the DataContext of the CellsPanel
                // Which is good, b/c we aren't attached to a cell anymore, so we don't want to get notified when the DataContext changes
                // while vertical scrolling.
                this.DataContext = null;
			}
			else
			{
				this.DelayRecycling = true;
			}
		}
		#endregion // OnReleased

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected virtual void AttachContent()
		{

		}
		#endregion // AttachContent

		#region ReleaseContent

		/// <summary>
		/// Invoked before content is released from the control.
		/// </summary>
		protected virtual void ReleaseContent()
		{
			
		}
		#endregion // ReleaseContent

		#region OnLoaded

		/// <summary>
		/// Raised when the <see cref="CellControlBase"/> is Loaded. 
		/// </summary>
		protected virtual void OnLoaded()
		{
            this.IsCellLoaded = true;
		}

		#endregion // OnLoaded

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		internal protected virtual void EnsureContent()
		{

		}

		#endregion // EnsureContent

		#region ReleaseObjectForRecycling
		/// <summary>
		/// Releases this object so that it can be recycled.
		/// </summary>
		/// <param propertyName="cell"></param>
		protected virtual void ReleaseObjectForRecycling(CellBase cell)
		{
            if (cell != null)
            {
                cell.EnsureCurrentState();
            }

			this.ReleaseContent();
			this.Cell = null;
		}
		#endregion // ReleaseObjectForRecycling

		#region ResolveBinding
		/// <summary>
		/// In a derived class this method should be implemented to resolve the binding that should be used for a bound <see cref="Column"/>
		/// </summary>
		/// <remarks>This will be called during the ContentProvider ResolveBinding and is not to be called directly otherwise.</remarks>
		/// <returns></returns>
		protected internal virtual Binding ResolveBinding()
		{
			return null;
		}
		#endregion // ResolveBinding

		#region ResolveEditorBinding

		/// <summary>
		/// Creates a <see cref="Binding"/> that can be applied to an editor.
		/// </summary>
		/// <remarks>This will be called during the ContentProvider ResolveBinding and is not to be called directly otherwise.</remarks>
		/// <returns></returns>
		protected internal virtual Binding ResolveEditorBinding()
		{
			return null;
		}

		#endregion // ResolveEditorBinding

		#region DetermineCursorHelper

		/// <summary>
		/// Determines if the cursor should be changed for column resizing.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="bounds"></param>
		/// <param name="xPosition"></param>
		/// <returns></returns>
		protected virtual bool DetermineCursorHelper(Column column, Rect bounds, double xPosition)
		{

            if (this.Cell != null && this.Cell.Row != null && this.Cell.Row.ColumnLayout != null && this.Cell.Row.ColumnLayout.Grid.FlowDirection == System.Windows.FlowDirection.RightToLeft)
            {
                double rightPos = bounds.Left - bounds.Width;
                return column.IsResizable && xPosition > (rightPos - this.ResizingThreshold) && xPosition < (rightPos + this.ResizingThreshold);
            }

            return column.IsResizable && xPosition > (bounds.Right - this.ResizingThreshold) && xPosition < bounds.Right;
        }

		#endregion // DetermineCursorHelper

		#region Refresh

		/// <summary>
		/// Refreshes the control with it's current bindings.
		/// </summary>
		protected internal void Refresh()
		{
			this.AttachContent();
		}

		#endregion // Refresh

		#region EnsureCurrentState
		/// <summary>
		/// Ensures that <see cref="CellControlBase"/> is in the correct state.
		/// </summary>
		protected internal virtual void EnsureCurrentState()
		{

		}
		#endregion // EnsureCurrentState

        #region ManuallyInvokeMeasure

        /// <summary>
        /// Invoked when a CellControl's measure was called, but its MeasureOverride was not invoked. 
        /// This method should be overriden on a derived class, if it's neccessary to take some extra steps to ensure Measure has been called.
        /// </summary>
        /// <param name="sizeToBeMeasured"></param>
        protected internal virtual void ManuallyInvokeMeasure(Size sizeToBeMeasured)
        {

        }

        #endregion // ManuallyInvokeMeasure

        #endregion // Protected

        #region Internal

        internal bool GoToState(string state, bool transition)
		{
			return VisualStateManager.GoToState(this, state, transition);
		}

		#endregion // Internal

		#region Public
		/// <summary>
		/// Gets a template child object by the propertyName of the child.
		/// </summary>
		/// <param propertyName="propertyName">The propertyName of the template child object.</param>
		/// <returns>A DependacyObject with the propertyName of the inputted string.</returns>
		public DependencyObject GetTemplateChildByName(string name)
		{
			return base.GetTemplateChild(name);
		}
		#endregion

		#region Private

		#region ValidateColumnWidth
		/// <summary>
		/// Used during resizing, ensures that the width that the user resizes the column to is legal according to the limitations of the column.
		/// </summary>
		/// <param propertyName="column"></param>
		/// <param propertyName="width"></param>
		/// <returns></returns>
		private static double ValidateColumnWidth(Column column, double width)
		{
			if (width < column.MinimumWidth)
			{
				return column.MinimumWidth;
			}
			if (width > column.MaximumWidth)
			{
				return column.MaximumWidth;
			}

		    GroupColumn parentColumn = column.ParentColumn as GroupColumn;

		    // NZ 8 May 2012 - TFS110390 - Prevent childColumn placed inside star-sized GroupColumn from exceeding 
            // the available width inside the GroupColumn.
            if (parentColumn != null && parentColumn.WidthResolved.WidthType == ColumnWidthType.Star)
            {
                double sizedColumnsWidth = 0;

                foreach (var childColumn in parentColumn.Columns)
                {
                    if (childColumn != column && childColumn.WidthResolved.WidthType != ColumnWidthType.Star)
                    {
                        sizedColumnsWidth += childColumn.ActualWidth;
                    }
                }

                if (width > (parentColumn.ActualWidth - sizedColumnsWidth))
		        {
                    return parentColumn.ActualWidth - sizedColumnsWidth;
		        }
		    }

			return width;
		}
		#endregion // ValidateColumnWidth

		#region ResizeColumns
		/// <summary>
		/// Used during column resizing, loops through the resized columns collection and sets the new width to the column.
		/// </summary>
		/// <param propertyName="width"></param>
		private void ResizeColumns(double width)
		{
			foreach (Column c in this._resizedColumns)
			{
				if (c.IsResizable)
					ResizeColumn(width, c);
			}
		}
		#endregion // ResizeColumns

		#region ResizeColumn
		/// <summary>
		/// Used during column resizing, sets the width of the column to a validated width.
		/// </summary>
		/// <param propertyName="width"></param>
		/// <param propertyName="column"></param>
		private static void ResizeColumn(double width, Column column)
		{
			column.Width = new ColumnWidth(CellControlBase.ValidateColumnWidth(column, width), false);
		}
		#endregion // ResizeColumn

		#region DetermineCursor
		/// <summary>
		/// Sets the cursor based on the mouse position and what features are enabled.
		/// </summary>
		/// <param propertyName="e"></param>
		/// <returns></returns>
		private bool DetermineCursor(MouseEventArgs e)
		{
			if (this.Cell != null)
			{
				Column column = this.Cell.Column;
				if (column != null &&
					column.IsResizable &&
					this.ShowResizingArrow &&
                    column.ColumnLayout != null &&
					column.ColumnLayout.ColumnResizingSettings.AllowColumnResizingResolved != ColumnResizingType.Disabled)
				{
					Rect bounds = HeaderCellControl.GetBoundsOfElement(this, null);
					Point position = e.GetPosition(null);
					double xPosition = position.X;
					try
					{
                        xPosition *= PlatformProxy.GetZoomFactor();
					}
					catch
					{
					}

					if (DetermineCursorHelper(column, bounds, xPosition))
					{
                        ReadOnlyKeyedColumnBaseCollection<Column> childColumns = column.AllColumns;
                        if (childColumns != null)
                        {
                            foreach (Column col in childColumns)
                            {
                                if (col.IsMoving || col.IsDragging)
                                    return false;
                            }
                        }

						this.Cursor = Cursors.SizeWE;
						return true;
					}
				}
                if (this.Cursor != null)
                {
                    this.Cursor = null;
                    this.AllowUserResizing = false;
                }
			}

			return false;
		}
		#endregion // DetermineCursor

		#endregion // Private

		#endregion // Methods

		#region Overrides

        #region MeasureOverride

        /// <summary>
        /// Allows a Cell to ensure it was propely measured. 
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            this.MeasureRaised = true;

            return base.MeasureOverride(availableSize);
        }
        #endregion // MeasureOverride

        #region ArrangeOverride
        /// <summary>
		/// Validates the Arranging of the cell, and makes sure that the width it will arrange at, matches the column's ActualWidth
		/// </summary>
		/// <param propertyName="finalSize"></param>
		/// <returns></returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (this.ValidateWidthInArrange && this.Cell != null && this.Cell.Column != null && this.Cell.Control != null)
			{
				// If the width is greater than the column's actual width, then we need re-evaluate it so that
				// such things as borders and rounded corners are correctly realized. 
				if (finalSize.Width > this.Cell.Column.ActualWidth)
				{
                    double width = this.Cell.Column.ActualWidth;
                    if (width == 0 && this.Cell.Column.WidthResolved.WidthType == ColumnWidthType.Numeric)
                    {
                        width = this.Cell.Column.WidthResolved.Value;
                    }

                    this.Cell.MeasuringSize = new Size(width, double.PositiveInfinity);
                    this.Cell.Row.ColumnLayout.Grid.Panel.RemeasuredCells.Add(this.Cell);
				}
			}

			return base.ArrangeOverride(finalSize);
		}
		#endregion // ArrangeOverride

        #region OnCreateAutomationPeer
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new CellControlBaseAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer
        
		#region OnMouseMove
		/// <summary>
		///  Called before the System.Windows.UIElement.MouseMove event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			OnMouseMoveColumnResizing(e);

			OnMouseMoveColumnMoving(e);

			base.OnMouseMove(e);
		}
		#endregion // OnMouseMove

		#region OnMouseLeave
		



		/// <summary>
		/// Called before the System.Windows.UIElement.MouseLeave event occurs
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
            if (this.Cursor != null && !this.IsResizing)
            {
                this.Cursor = null;
                this.AllowUserResizing = false;
            }
            
			base.OnMouseLeave(e);
		}

		#endregion // OnMouseLeave

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Called before the System.Windows.UIElement.MouseLeftButtonDown event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (!OnMouseLeftButtonDownColumnResizing(e))
				OnMouseLeftButtonDownColumnMoving(e);

			base.OnMouseLeftButtonDown(e);
		}
		#endregion // OnMouseLeftButtonDown

		#region OnMouseLeftButtonUp
		/// <summary>
		/// Called before the System.Windows.UIElement.MouseLeftButtonUp event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			OnMouseLeftButtonUpColumnResizing(e);

			OnMouseLeftButtonUpColumnMoving(e);

			base.OnMouseLeftButtonUp(e);
		}
		#endregion // OnMouseLeftButtonUp

		#region OnKeyDown
		/// <summary>
		/// Called before the <see cref="UIElement.KeyDown"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (this.IsResizing)
			{
				if (e.Key == Key.Escape)
					this.CancelDragResize(true);
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
            if (this.IsResizing)
                this.EndDragResize(true);

            base.OnLostMouseCapture(e);
        }
        #endregion // OnLostMouseCapture

        #region OnApplyTemplate
        /// <summary>
        /// Builds the visual tree for the <see cref="CellControlBase"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Cell != null)
                this.Cell.EnsureCurrentState();
        }
        #endregion // OnApplyTemplate

		#endregion // Overrides

		#region EventHandlers

		#region CellControlBase_Loaded

		private void CellControlBase_Loaded(object sender, RoutedEventArgs e)
		{
			if (this.Cell != null)
				this.GoToState(this.Cell.NormalState, false);

			this.OnLoaded();
		}

		#endregion // CellControlBase_Loaded

		#endregion // EventHandlers

		#region ColumnMoving MouseEvents

		#region OnMouseLeftButtonDownColumnMoving
		/// <summary>
		/// Called during <see cref="OnMouseLeftButtonDown"/> to process the control for Column Moving.
		/// </summary>
		/// <param propertyName="e"></param>
		/// <returns></returns>
		protected virtual bool OnMouseLeftButtonDownColumnMoving(MouseButtonEventArgs e)
		{
			return false;
		}
		#endregion // OnMouseLeftButtonDownColumnMoving

		#region OnMouseMoveColumnMoving
		/// <summary>
		/// Called during <see cref="OnMouseMove"/> to process the control for Column Moving.
		/// </summary>
		/// <param propertyName="e"></param>
		protected virtual void OnMouseMoveColumnMoving(MouseEventArgs e)
		{
		}
		#endregion // OnMouseMoveColumnMoving

		#region OnMouseLeftButtonUpColumnMoving
		/// <summary>
		/// Called during <see cref="OnMouseLeftButtonUp"/> to process the control for Column Moving.
		/// </summary>
		/// <param propertyName="e"></param>
		protected virtual void OnMouseLeftButtonUpColumnMoving(MouseButtonEventArgs e)
		{
		}
		#endregion // OnMouseLeftButtonUpColumnMoving

		#endregion // ColumnMoving MouseEvents

		#region ColumnResizing MouseEvents

		#region OnMouseLeftButtonDownColumnResizing
		/// <summary>
		/// Called during <see cref="OnMouseLeftButtonDown"/> to process the control for Column IsResizing.
		/// </summary>
		/// <param propertyName="e"></param>
		/// <returns></returns>
		protected virtual bool OnMouseLeftButtonDownColumnResizing(MouseButtonEventArgs e)
		{
			if (this.AllowUserResizing)
			{
				this.IsResizing = this.InitializeDragResize(e);
			}
			else
			{
				this.IsResizing = false;
			}

            e.Handled = this.IsResizing;

			return this.IsResizing;
		}
		#endregion // OnMouseLeftButtonDownColumnResizing

		#region OnMouseLeftButtonUpColumnResizing
		/// <summary>
		/// Called during <see cref="OnMouseLeftButtonDown"/> to process the control for Column IsResizing.
		/// </summary>
		/// <param propertyName="e"></param>
		/// <returns></returns>
		protected virtual void OnMouseLeftButtonUpColumnResizing(MouseButtonEventArgs e)
		{
            if (this.Cell == null || this.Cell.Row == null || this.Cell.Row.ColumnLayout == null)
                return;

            ColumnResizingSettingsOverride columnResizeSettings = this.Cell.Row.ColumnLayout.ColumnResizingSettings;

            if (columnResizeSettings != null)
			{
                ColumnResizingType resizingResolved = columnResizeSettings.AllowColumnResizingResolved;
                bool allowMultipleColumnResizing = columnResizeSettings.AllowMultipleColumnResizeResolved;

				if (this.IsResizing)
				{
					bool deltaX = this._dragStartPoint.X != e.GetPosition(null).X;

                    Column resizeCol = this.Cell.Column.ResizeColumnResolved; ;

					if (
                        (resizingResolved != ColumnResizingType.Immediate && resizeCol.ActualWidth != this._originalWidthValue) ||
						(resizingResolved == ColumnResizingType.Indicator && deltaX) ||
						(resizingResolved == ColumnResizingType.Immediate && this._resizedColumns != null && this._resizedColumns.Count > 1 && allowMultipleColumnResizing && deltaX)
					)
					{
						VerifyResizedColumns();

						this.ResizeColumns(this._currentWidth);
					}
					this.EndDragResize(true);

                    this.ReleaseMouseCapture();
				}
				
                this.AllowUserResizing = this.DetermineCursor(e);
			}
		}
		#endregion // OnMouseLeftButtonUpColumnResizing

		#region OnMouseMoveColumnResizing
		/// <summary>
		/// Called during <see cref="OnMouseMove"/> to process the control for Column IsResizing.
		/// </summary>
		/// <param propertyName="e"></param>
		protected virtual void OnMouseMoveColumnResizing(MouseEventArgs e)
		{
            if (this.Cell == null || this.Cell.Row == null || this.Cell.Row.ColumnLayout == null)
                return;

            ColumnResizingSettingsOverride columnResizeSettings = this.Cell.Row.ColumnLayout.ColumnResizingSettings;

			if (columnResizeSettings.AllowColumnResizingResolved == ColumnResizingType.Disabled)
			{
				return;
			}

			if (!this.IsResizing)
			{
				// set cursor up always
				this.AllowUserResizing = DetermineCursor(e);  // the return value is if we detected ourselves to be in a resize area.
			}
			else
			{
				// ok we have detemined that we are actually resizing the column, so move it.
				this.Focus();
				RowBase rowBase = this.Cell.Row;
				XamGrid grid = rowBase.ColumnLayout.Grid;
				Panel gridPanel = grid.Panel;
				// Grap the points of the grid
				Point rootPoint = e.GetPosition(null);
				Point elemPoint = e.GetPosition(gridPanel);
                Popup columnResizingIndicatorContainer = columnResizeSettings.IndicatorContainer;

                UIElement relativeTo = null;


                relativeTo = PlatformProxy.GetRootVisual(this);


                MatrixTransform mt = this.TransformToVisual(relativeTo) as MatrixTransform;

                double scaleX = 1.0;

                if (mt != null)
                {
                    scaleX = Math.Abs(mt.Matrix.M11) / PlatformProxy.GetZoomFactor();
                }

                this._currentWidth = (this._originalWidthValue + (rootPoint.X - this._dragStartPoint.X) / scaleX);

                
                    if (grid.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                    {
                        this._currentWidth = (this._originalWidthValue - (rootPoint.X - this._dragStartPoint.X) / scaleX);
                    }
                
				// i guess i should validate the width here
				this._currentWidth = ValidateColumnWidth(this._resizedColumn, this._currentWidth);

                if (columnResizeSettings.AllowColumnResizingResolved == ColumnResizingType.Immediate)
					ResizeColumn(this._currentWidth, this._resizedColumn);

				CellsPanel cellsPanel = rowBase.Control;

				if (cellsPanel != null)
					cellsPanel.Measure(cellsPanel.DesiredSize);

                if (columnResizeSettings.AllowColumnResizingResolved == ColumnResizingType.Indicator)
					columnResizingIndicatorContainer.HorizontalOffset = elemPoint.X;
			}
		}
		#endregion // OnMouseMoveColumnResizing

		#endregion // ColumnResizing MouseEvents

		#region InitializeDragResize
		/// <summary>
		/// Sets up the grid to use column resizing.
		/// </summary>
		/// <param propertyName="e"></param>
		/// <returns></returns>
		protected virtual bool InitializeDragResize(MouseEventArgs e)
		{
			XamGrid grid = this.Cell.Row.ColumnLayout.Grid;
			RowsPanel gridPanel = grid.Panel;

            ColumnResizingSettingsOverride columnResizeSettings = this.Cell.Row.ColumnLayout.ColumnResizingSettings;

			this._resizedColumn = this.Cell.Column.ResizeColumnResolved;
			// Determine if a DragOperation can occur.
			if (this._resizedColumn == null || !this.ShowResizingArrow || (this._resizedColumn != null && !this._resizedColumn.IsResizable))
				return false;

			// Setup Globals
			this._dragStartPoint = e.GetPosition(null);
			this._originalWidthValue = this._resizedColumn.ActualWidth;
			this._originalWidth = this._resizedColumn.Width;

			_resizedColumns = new Collection<Column>();

            if (columnResizeSettings.AllowMultipleColumnResizeResolved)
			{
				if (grid.SelectionSettings.SelectedColumns.Contains(this.Cell.Column))
				{
					ColumnLayout colLayot = this.Cell.Column.ColumnLayout;

					foreach (Column c in grid.SelectionSettings.SelectedColumns)
					{
						if (c.ColumnLayout == colLayot)
						{
							_resizedColumns.Add(c.ResizeColumnResolved);
						}
					}
				}
				else
				{
					_resizedColumns.Add(this._resizedColumn);
				}
			}
			else
			{
                _resizedColumns.Add(this._resizedColumn);
			}

			if (grid.OnColumnResizing(_resizedColumns, _originalWidthValue))
			{
				this.IsResizing = false;
				return false;
			}

			VerifyResizedColumns();

			// Make sure that nothing else can listen to the mouse now.
			this.CaptureMouse();

            if (columnResizeSettings.AllowColumnResizingResolved == ColumnResizingType.Indicator)
			{
				// Determine where to position the indicator. 
                Popup columnResizingIndicatorContainer = columnResizeSettings.IndicatorContainer;

				// Setup popup positional values, base if off the bottom of the top of the header and the right edge of the header.
				// Calculate where this HeaderCellControl is inside of the Grid and set the Y coordinate of the 
				// popup, as the popup should only move horizontally, and never vertically.
				Rect bounds = HeaderCellControl.GetBoundsOfElement(this, gridPanel);

                columnResizingIndicatorContainer.HorizontalOffset = bounds.Left + (bounds.Width / PlatformProxy.GetZoomFactor());

				RowsManagerBase rowsManagerBase = this.Cell.Row.Manager;

				columnResizingIndicatorContainer.VerticalOffset = DetermineOffsetHeight(rowsManagerBase, gridPanel);

				// ensure we on the panel
				if (!gridPanel.Children.Contains(columnResizingIndicatorContainer))
				{
					gridPanel.Children.Add(columnResizingIndicatorContainer);
				}

				// Set up the dimensions of the drag indicator.
                columnResizeSettings.Indicator.Height = DetermineIndicatorHeight(rowsManagerBase, gridPanel);

				// Open the popup
				columnResizingIndicatorContainer.IsOpen = true;
			}
			return true;
		}

		private void VerifyResizedColumns()
		{
			if (this._resizedColumns == null ||
				this._resizedColumns.Count == 0 ||
				!this._resizedColumns.Contains(this._resizedColumn))
			{
				throw new ResizingColumnCannotBeRemovedException();
			}
		}

		private static double DetermineOffsetHeight(RowsManagerBase mgr, RowsPanel panel)
		{
			if (mgr.Level == 0)
			{
				Rect bounds = HeaderCellControl.GetBoundsOfElement(panel, panel);
				return bounds.Top;
			}
			foreach (RowBase rb in panel.VisibleRows)
			{
				if (mgr == rb.Manager)
				{
					Rect bounds = HeaderCellControl.GetBoundsOfElement(rb.Control, panel);
					return bounds.Top;
				}
			}
			return 0.0;
		}

		private static double DetermineIndicatorHeight(RowsManagerBase mgr, RowsPanel panel)
		{
			if (mgr.Level == 0)
			{
				return panel.ActualHeight;
			}

			double height = 0.0;
			bool foundLevel = false;
			foreach (RowBase rb in panel.VisibleRows)
			{
				if (mgr == rb.Manager)
				{
					foundLevel = true;
					height += rb.Control.ActualHeight;
				}
				else if (foundLevel)
				{
					break;
				}
			}
			return height;
		}

		#endregion // InitializeDragResize

		#region CancelDragResize
		/// <summary>
		/// Executed when user driven column resizing is cancelled.
		/// </summary>
		/// <param propertyName="fireEvent"></param>
		protected virtual void CancelDragResize(bool fireEvent)
		{
			if (this._resizedColumn != null)
			{
				this._resizedColumn.Width = this._originalWidth;
			}

			this.EndDragResize(fireEvent);
		}
		#endregion // CancelDragResize

		#region EndDragResize
		/// <summary>
		/// Executed when resizing is done, both successfully and cancelled, to clean up when completed.
		/// </summary>
		protected internal virtual void EndDragResize(bool fireEvent)
		{
			XamGrid grid = this.Cell.Column.ColumnLayout.Grid;
            ColumnResizingSettingsOverride columnResizeSettings = this.Cell.Row.ColumnLayout.ColumnResizingSettings;

			if (fireEvent)
				grid.OnColumnResized(this._resizedColumns);

			this.IsResizing = false;

			this._resizedColumn = null;

            if (columnResizeSettings.AllowColumnResizingResolved == ColumnResizingType.Indicator)
			{
				RowsPanel gridPanel = grid.Panel;
                Popup columnResizingIndicatorContainer = columnResizeSettings.IndicatorContainer;
				columnResizingIndicatorContainer.IsOpen = false;
				if (gridPanel.Children.Contains(columnResizingIndicatorContainer))
				{
					gridPanel.Children.Remove(columnResizingIndicatorContainer);
				}
			}

			this.ReleaseMouseCapture();

			if (this.DelayRecycling)
			{
				ReleaseObjectForRecycling(this.Cell);
				this.DelayRecycling = false;
			}
			this.Cursor = null;
			this._currentWidth = 0.0;
		}
		#endregion // EndDragResize

		#region IRecyclableElement Members

		bool IRecyclableElement.DelayRecycling
		{
			get
			{
				return this.DelayRecycling;
			}
			set
			{
				this.DelayRecycling = value;
			}
		}

        Panel IRecyclableElement.OwnerPanel
        {
            get
            {
                return this.OwnerPanel;
            }
            set
            {
                this.OwnerPanel = value;
            }
        }

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Event raised when a property on this object changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the PropertyChanged event.
		/// </summary>
		/// <param name="propName"></param>
		protected virtual void OnPropertyChanged(string propName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
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