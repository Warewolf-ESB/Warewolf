using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Collections.Generic;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Grids.Primitives;
using System.Windows.Media.Imaging;
using System.Text;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// Displays data in a table like structure. 
	/// </summary>
	[TemplatePart(Name = "DefaultDeferredScrollingTemplate", Type = typeof(DataTemplate))]
	[TemplatePart(Name = "RowsPanel", Type = typeof(RowsPanel))]
	[TemplatePart(Name = "VerticalScrollBar", Type = typeof(ScrollBar))]
	[TemplatePart(Name = "HorizontalScrollBar", Type = typeof(ScrollBar))]
	[StyleTypedProperty(Property = "CellStyle", StyleTargetType = typeof(CellControl))]
	[StyleTypedProperty(Property = "HeaderStyle", StyleTargetType = typeof(HeaderCellControl))]
	[StyleTypedProperty(Property = "FooterStyle", StyleTargetType = typeof(FooterCellControl))]
	[StyleTypedProperty(Property = "ChildBandHeaderStyle", StyleTargetType = typeof(ChildBandCellControl))]
	[StyleTypedProperty(Property = "FixedRowSeparatorStyle", StyleTargetType = typeof(FixedRowSeparator))]
    [ComplexBindingProperties("ItemsSource")]

    
    

	public class XamGrid : Control, IProvideScrollInfo, IDisposable, INotifyPropertyChanged, IProvidePropertyPersistenceSettings, ISupportScrollHelper
	{
		#region Members

        private static char[] ClipboardCopyEscapeChars = new char[]{ '\r', '\n', '\t' };

        
        WeakReference _lastKeyboardObject;
        

        bool _mouseIsOver;
        private bool _isColumnResizing;
		ScrollBar _verticalScrollbar, _horizontalScrollbar;
		RowsPanel _panel;
		InternalRowsCollection _internalRows;
		ColumnLayoutCollection _columnLayouts;
		XamGridRowsManager _rowsManager;

		RowSelectorSettings _rowSelectorSettings;
		ExpansionIndicatorSettings _expansionIndicators;
		ColumnMovingSettings _columnMovingSettings;
		SelectionSettings _selectionSettings;
		FixedColumnSettings _fixedColumnSettings;
		EditingSettings _editingSettings;
		ColumnResizingSettings _columnResizingSettings;
		GroupBySettings _groupBySettings;
		DeferredScrollingSettings _deferredScrollingSettings;
		AddNewRowSettings _addNewRowSettings;
		FilteringSettings _filteringSettings;
		SummaryRowSettings _summaryRowSettings;
		FillerColumnSettings _fillerColumnSettings;
		ConditionalFormattingSettings _conditionalFormattingSettings;
		ColumnChooserSettings _columnChooserSettings;
		ClipboardSettings _clipBoardSettings;

		Popup _deferredPopup;
		ContentControl _deferredContentControl;

		CellBase _activeCell;

		SortingSettings _sortingSettings;

		DragSelectType _dragSelectType;
		PagerSettings _pagerSettings;
		CellBase _mouseOverCell;
		DispatcherTimer _selectRowsCellsTimer;
		Point _mousePosition;
		CellBase _mouseDownCell, _doubleClickCell;
		long _doubleClickTimeStamp;
		ColumnTypeMappingsCollection _columnTypeMappings;
		DropAreaIndicator _dropAreaIndicatorLeft, _dropAreaIndicatorRight;
		Dictionary<string, object> _editCellValues, _originalCellValues;
		Canvas _dropIndicatorPanel;
		DataTemplate _defaultDeferredScrollingTemplate, _defaultGroupByDeferredScrollingTemplate;

		Dictionary<ComparisonOperator, DataTemplate> _filterIcons;
		bool _isLoaded;
		bool _suspendConditionalFormatUpdates;
        ColumnChooserDialog _columnChooserDialog;
        HeaderDropDownControl _openDropDownControl;
        KeyboardNavigationMode? _tabMode;

        List<XamGridRenderAdorner> _adorners;
        bool _ignoreActiveItemChanging;
        DataManagerProvider _dataManagerProvider;
        List<Cell> _cellsThatCancelledEditMode;       
        FrameworkElement _rootElement;

        WeakReference _rootVis;

        bool _currentlyEnteringEditMode;
        private bool _cachedIsAlternateRowsEnabled = true;

        TouchScrollHelper _scrollHelper;





        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="XamGrid"/> class.
        /// </summary>
        static XamGrid()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamGrid), new FrameworkPropertyMetadata(typeof(XamGrid)));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="XamGrid"/> class.
		/// </summary>
		public XamGrid()
		{



			this._rowsManager = this.CreateRootRowsManager();

			// Instead of using the overrides, lets use this, so that we're notified of these events, not matter if they're handled by
			// something internally. 
			this.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.XamWebGrid_KeyDown), true);
			this.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.XamWebGrid_MouseLeftButtonDown), true);
			this.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.XamWebGrid_MouseLeftButtonUp), true);


			this._editCellValues = new Dictionary<string, object>();
			this._originalCellValues = new Dictionary<string, object>();

			this.Loaded += new RoutedEventHandler(XamWebGrid_Loaded);
            this.Unloaded += new RoutedEventHandler(XamGrid_Unloaded);


            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamGrid), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


            // Allows any dervied class of the xamGrid, to have a chance to plug in their own XamGridRenderAdorners
            this.RegisterRenderAdorners(this.RenderAdorners);
            this.UseLayoutRounding = true;
		}

		#endregion // Constructor

		#region Overrides

		#region OnApplyTemplate

		/// <summary>
		/// Builds the visual tree for the <see cref="XamGrid"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

            if (this._panel != null)
            {
                this._panel.ResetRows();
                this._panel.Children.Clear();
                this._panel.Grid = null;

                // Foreach XamGridRenderAdorner, make sure we unregister it. 
                foreach (XamGridRenderAdorner adorner in this._adorners)
                    adorner.UnregisterRowsPanel();
            }

			this._panel = base.GetTemplateChild("RowsPanel") as RowsPanel;
			if (this._panel != null)
			{
				this._panel.Grid = this;
                this._rowsManager.Grid = this;
				this._rowsManager.InvalidateTopAndBottomRows();

				this._dropIndicatorPanel = new Canvas();
				Canvas.SetZIndex(this._dropIndicatorPanel, 500);
				this._panel.Children.Add(this._dropIndicatorPanel);

				this._dropAreaIndicatorLeft = new DropAreaIndicator();
				this._dropIndicatorPanel.Children.Add(this._dropAreaIndicatorLeft);

				this._dropAreaIndicatorRight = new DropAreaIndicator();
				this._dropIndicatorPanel.Children.Add(this._dropAreaIndicatorRight);

				this._panel.CustomFilterDialogControl = new ColumnFilterDialogControl();
                this._panel.CompoundFilterDialogControl = new CompoundFilterDialogControl();

                // Foreach XamGridRenderAdorner, register it with this panel.
                foreach (XamGridRenderAdorner adorner in this._adorners)
                    adorner.RegisterRowsPanel(this._panel);
                
                _scrollHelper = new TouchScrollHelper(this._panel, this);
                _scrollHelper.IsEnabled = this.IsTouchSupportEnabled;

				this._panel.SetCurrentValue(FrameworkElement.IsManipulationEnabledProperty, this.IsTouchSupportEnabled);

            }

			if (this._verticalScrollbar != null)
				this._verticalScrollbar.Scroll -= new ScrollEventHandler(VerticalScrollbar_Scroll);

			this._verticalScrollbar = base.GetTemplateChild("VerticalScrollBar") as ScrollBar;
			if (this._verticalScrollbar != null)
			{
				this._verticalScrollbar.Scroll += new ScrollEventHandler(VerticalScrollbar_Scroll);
				this._verticalScrollbar.Orientation = Orientation.Vertical;
			}

			if (this._horizontalScrollbar != null)
				this._horizontalScrollbar.Scroll -= new ScrollEventHandler(HorizontalScrollbar_Scroll);

			this._horizontalScrollbar = base.GetTemplateChild("HorizontalScrollBar") as ScrollBar;
			if (this._horizontalScrollbar != null)
			{
				this._horizontalScrollbar.Orientation = Orientation.Horizontal;
				this._horizontalScrollbar.Scroll += new ScrollEventHandler(HorizontalScrollbar_Scroll);
			}

            this._rootElement = base.GetTemplateChild("Root") as FrameworkElement;

            if (this._rootElement != null)
            {
                this._defaultDeferredScrollingTemplate = this._rootElement.Resources["DefaultDeferredScrollingTemplate"] as DataTemplate;
                this._defaultGroupByDeferredScrollingTemplate = this._rootElement.Resources["DefaultGroupByDeferredScrollingTemplate"] as DataTemplate;
            }

			
			this.LoadFilterIconFromControlTemplate("EqualsOperand", ComparisonOperator.Equals);
			this.LoadFilterIconFromControlTemplate("NotEqualsOperand", ComparisonOperator.NotEquals);
			this.LoadFilterIconFromControlTemplate("StartsWithOperand", ComparisonOperator.StartsWith);
			this.LoadFilterIconFromControlTemplate("EndsWithOperand", ComparisonOperator.EndsWith);
			this.LoadFilterIconFromControlTemplate("DoesNotStartWithOperand", ComparisonOperator.DoesNotStartWith);
			this.LoadFilterIconFromControlTemplate("DoesNotEndWithOperand", ComparisonOperator.DoesNotEndWith);
			this.LoadFilterIconFromControlTemplate("ContainsOperand", ComparisonOperator.Contains);
			this.LoadFilterIconFromControlTemplate("DoesNotContainOperand", ComparisonOperator.DoesNotContain);
			this.LoadFilterIconFromControlTemplate("GreaterThanOperand", ComparisonOperator.GreaterThan);
			this.LoadFilterIconFromControlTemplate("GreaterThanOrEqualOperand", ComparisonOperator.GreaterThanOrEqual);
			this.LoadFilterIconFromControlTemplate("LessThanOperand", ComparisonOperator.LessThan);
			this.LoadFilterIconFromControlTemplate("LessThanOrEqualOperand", ComparisonOperator.LessThanOrEqual);

            this.LoadFilterIconFromControlTemplate("TodayOperand", ComparisonOperator.DateTimeToday);
            this.LoadFilterIconFromControlTemplate("YesterdayOperand", ComparisonOperator.DateTimeYesterday);
            this.LoadFilterIconFromControlTemplate("TomorrowOperand", ComparisonOperator.DateTimeTomorrow);

            this.LoadFilterIconFromControlTemplate("YearToDateOperand", ComparisonOperator.DateTimeYearToDate);

            this.LoadFilterIconFromControlTemplate("NextMonthOperand", ComparisonOperator.DateTimeNextMonth);
            this.LoadFilterIconFromControlTemplate("ThisMonthOperand", ComparisonOperator.DateTimeThisMonth);
            this.LoadFilterIconFromControlTemplate("LastMonthOperand", ComparisonOperator.DateTimeLastMonth);

            this.LoadFilterIconFromControlTemplate("NextWeekOperand", ComparisonOperator.DateTimeNextWeek);
            this.LoadFilterIconFromControlTemplate("ThisWeekOperand", ComparisonOperator.DateTimeThisWeek);
            this.LoadFilterIconFromControlTemplate("LastWeekOperand", ComparisonOperator.DateTimeLastWeek);

            this.LoadFilterIconFromControlTemplate("NextQuarterOperand", ComparisonOperator.DateTimeNextQuarter);
            this.LoadFilterIconFromControlTemplate("ThisQuarterOperand", ComparisonOperator.DateTimeThisQuarter);
            this.LoadFilterIconFromControlTemplate("LastQuarterOperand", ComparisonOperator.DateTimeLastQuarter);

            this.LoadFilterIconFromControlTemplate("NextYearOperand", ComparisonOperator.DateTimeNextYear);
            this.LoadFilterIconFromControlTemplate("ThisYearOperand", ComparisonOperator.DateTimeThisYear);
            this.LoadFilterIconFromControlTemplate("LastYearOperand", ComparisonOperator.DateTimeLastYear);

            this.LoadFilterIconFromControlTemplate("DateAfterOperand", ComparisonOperator.DateTimeAfter);
            this.LoadFilterIconFromControlTemplate("DateBeforeOperand", ComparisonOperator.DateTimeBefore);

            this.LoadFilterIconFromControlTemplate("Quarter1Operand", ComparisonOperator.DateTimeQuarter1);
            this.LoadFilterIconFromControlTemplate("Quarter2Operand", ComparisonOperator.DateTimeQuarter2);
            this.LoadFilterIconFromControlTemplate("Quarter3Operand", ComparisonOperator.DateTimeQuarter3);
            this.LoadFilterIconFromControlTemplate("Quarter4Operand", ComparisonOperator.DateTimeQuarter4);

            this.LoadFilterIconFromControlTemplate("JanuaryOperand", ComparisonOperator.DateTimeJanuary);
            this.LoadFilterIconFromControlTemplate("FebruaryOperand", ComparisonOperator.DateTimeFebruary);
            this.LoadFilterIconFromControlTemplate("MarchOperand", ComparisonOperator.DateTimeMarch);
            this.LoadFilterIconFromControlTemplate("AprilOperand", ComparisonOperator.DateTimeApril);
            this.LoadFilterIconFromControlTemplate("MayOperand", ComparisonOperator.DateTimeMay);
            this.LoadFilterIconFromControlTemplate("JuneOperand", ComparisonOperator.DateTimeJune);
            this.LoadFilterIconFromControlTemplate("JulyOperand", ComparisonOperator.DateTimeJuly);
            this.LoadFilterIconFromControlTemplate("AugustOperand", ComparisonOperator.DateTimeAugust);
            this.LoadFilterIconFromControlTemplate("SeptemberOperand", ComparisonOperator.DateTimeSeptember);
            this.LoadFilterIconFromControlTemplate("OctoberOperand", ComparisonOperator.DateTimeOctober);
            this.LoadFilterIconFromControlTemplate("NovemberOperand", ComparisonOperator.DateTimeNovember);
            this.LoadFilterIconFromControlTemplate("DecemberOperand", ComparisonOperator.DateTimeDecember);
		}

		#endregion // OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the Silverlight automation infrastructure.
		/// </summary>
		/// <returns>
		/// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
		/// </returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new XamGridAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#region MeasureOverride

		/// <summary>
		/// Provides the behavior for the "measure" pass of the <see cref="XamGrid"/>.
		/// </summary>
		/// <param propertyName="availableSize">The available size that this object can give to child objects. Infinity can be specified
		/// as a value to indicate the object will size to whatever content is available.</param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			UIElement rootElement = (VisualTreeHelper.GetChildrenCount(this) > 0) ? VisualTreeHelper.GetChild(this, 0) as UIElement : null;
			if (rootElement != null)
			{
				rootElement.Measure(availableSize);
				return rootElement.DesiredSize;
			}
			return base.MeasureOverride(availableSize);
		}

		#endregion // MeasureOverride

		#region OnInitialized

		/// <summary>
		/// Invoked when the control is first initialized.
		/// </summary>
		/// <param name="e">Provides information for the event.</param>
		protected override void OnInitialized(EventArgs e)
		{
			// AS 3/28/12 TFS107071
			// We want to load the data before the grid is actually displayed. This happens 
			// in SL because the Loaded event is raised before the initial measure/arrange.
			//
			if (!this._isLoaded)
			{
			    this.LoadData();
			}

			base.OnInitialized(e);
		}

		#endregion //OnInitialized

		#region OnMouseEnter
		/// <summary>
		/// Called before the <see cref="UIElement.MouseEnter"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			this._mouseIsOver = true;
			base.OnMouseEnter(e);

			if (this._selectRowsCellsTimer != null)
				this._selectRowsCellsTimer.Stop();
		}

		#endregion // OnMouseEnter

		#region OnMouseLeave

		/// <summary>
		/// Called before the <see cref="UIElement.MouseLeave"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			this._mouseIsOver = false;

			if (this._mouseOverCell != null)
			{
				this._mouseOverCell.Row.IsMouseOver = false;
				if (this._mouseOverCell.Row.Control != null)
                    this._mouseOverCell.Row.Control.InternalCellMouseLeave(this._mouseOverCell, null);
				this._mouseOverCell = null;
			}



#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


			base.OnMouseLeave(e);
		}

		#endregion // OnMouseLeave

		#region OnGotFocus
		/// <summary>
		/// Called before the <see cref="UIElement.GotFocus"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (this.CurrentEditCell == null && this.ActiveCell == null)
            {
                if (this.Rows.Count > 0 && this.Rows[0].Cells.Count > 0)
                {
                    bool setFocus = true;

                    // The control inside of us, must be our own, so lets not steal focus away from it if we don't have to.
                    Control c = e.OriginalSource as Control;
                    if (c != null)
                        setFocus = false;

                    Row r = this.Rows[0];

                    foreach (CellBase cb in r.Cells)
                    {
                        if (cb.Column.Visibility == Visibility.Visible)
                        {
                            this.SetActiveCell(cb, CellAlignment.NotSet, InvokeAction.Code, true, setFocus, false);
                            break;
                        }
                    }
                }
                else
                {
                    if (!SetFocusToSpecialRow(this.RowsManager.RegisteredTopRows, e.OriginalSource as Control))
                    {
                        SetFocusToSpecialRow(this.RowsManager.RegisteredBottomRows, e.OriginalSource as Control);
                    }
                }
            }
        }

		#endregion // OnGotFocus

		#region OnMouseMove
		/// <summary>
		/// Called before the <see cref="UIElement.MouseMove"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

            if (!this._mouseIsOver && this._dragSelectType != DragSelectType.None)
            {

                this._mousePosition = e.GetPosition(this);



            }

			CellControlBase cell = this.GetCellFromSource(e.OriginalSource as DependencyObject);


            // WPF only returns the control itself as the e.OriginalSource when the mouse is captured...
            // In this case, e.OrginalSource will always return the XamDataTree
            // So that means we need to look the element we're currently over, and try to resolve the node that way.
            if (this.IsMouseCaptured)
            {

                // VTH.HitTest, will return collapsed elements....WHY??
                // So, now lets walk through all possible elements, not just the root one. 
                IEnumerable<UIElement> hitElements = PlatformProxy.GetElementsFromPoint(e.GetPosition(this), this);
                foreach (UIElement elem in hitElements)
                {
                    if (elem.Visibility != System.Windows.Visibility.Collapsed)
                    {
                        cell = this.GetCellFromSource(elem);

                        if (cell != null)
                            break;
                    }
                }

                HitTestResult result = VisualTreeHelper.HitTest(this, e.GetPosition(this));

                // WPF does not raise the MouseEnter and MouseLeave event when the mouse is capture... another awesome difference between the 2 
                // platforms. so we have to do the logic for DragScrolling in MouseMove instead.
                if (result == null)
                {
                    this._mouseIsOver = false;

                    // MouseLeave
                    if (this._dragSelectType != DragSelectType.None)
                    {
                        if (this.SelectionSettings.CellSelection == SelectionType.Multiple || this.SelectionSettings.RowSelection == SelectionType.Multiple)
                        {
                            if (this._selectRowsCellsTimer == null)
                            {
                                this._selectRowsCellsTimer = new DispatcherTimer();
                                this._selectRowsCellsTimer.Tick += new EventHandler(SelectRowsCellsTimer_Tick);
                                this._selectRowsCellsTimer.Interval = TimeSpan.FromMilliseconds(0);
                            }
                            this._selectRowsCellsTimer.Start();
                        }
                    }
                }
                else
                {
                    this._mouseIsOver = true;

                    // MouseEnter
                    if (this._selectRowsCellsTimer != null)
                        this._selectRowsCellsTimer.Stop();
                }



            }

                        
			if (cell != null)
			{
				if (cell.Cell != this._mouseOverCell)
				{
                    bool invalidate = false; 

					if (this._mouseOverCell != null)
					{
                        if (this._mouseOverCell.Row.MergeData != null && this._mouseOverCell.Row.IsMouseOver)
                            invalidate = true;

						this._mouseOverCell.Row.IsMouseOver = false;
						if (this._mouseOverCell.Row.Control != null)
                            this._mouseOverCell.Row.Control.InternalCellMouseLeave(this._mouseOverCell, cell.Cell);
					}
					if (cell.Cell != null)
					{
						this._mouseOverCell = cell.Cell;

                        if (this._mouseOverCell.Row.MergeData != null && !this._mouseOverCell.Row.IsMouseOver)
                            invalidate = true;

						this._mouseOverCell.Row.IsMouseOver = true;
						if (this._mouseOverCell.Row.Control != null)
							this._mouseOverCell.Row.Control.InternalCellMouseEnter(this._mouseOverCell);
					}

                    if (invalidate && this.RowHover == RowHoverType.Row)
                    {
                        this.InvalidateScrollPanel(false);
                    }
				}
				if (this._dragSelectType != DragSelectType.None && cell.Cell != null)
					cell.Cell.OnCellDragging(this._dragSelectType);

				if (cell.Cell != null)
					cell.Cell.OnCellMouseMove(e);
			}
			else
			{
				if (this._mouseOverCell != null)
				{   
					this._mouseOverCell.Row.IsMouseOver = false;
					if (this._mouseOverCell.Row.Control != null)
						this._mouseOverCell.Row.Control.InternalCellMouseLeave(this._mouseOverCell, null);

                    if (this._mouseOverCell.Row.MergeData != null)
                        this.InvalidateScrollPanel(false);

					this._mouseOverCell = null;
				}
			}
		}
		#endregion // OnMouseMove

		#region OnMouseWheel

		/// <summary>
		/// Called when the MouseWheel is raised on the <see cref="XamGrid"/>
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (this._mouseIsOver)
			{
				bool handled = false;
				int multiplier = (e.Delta < 0) ? 1 : -1;

				if (this._verticalScrollbar != null && this._verticalScrollbar.Visibility == Visibility.Visible)
				{
                    double originalValue = this._verticalScrollbar.Value;
					this._verticalScrollbar.Value += this._verticalScrollbar.SmallChange * multiplier;

                    if (originalValue != this._verticalScrollbar.Value)
                        handled = true;
				}
				else if (this._horizontalScrollbar != null && this._horizontalScrollbar.Visibility == Visibility.Visible)
				{
                    double originalValue = this._horizontalScrollbar.Value;
					this._horizontalScrollbar.Value += this._horizontalScrollbar.SmallChange * multiplier;

                    if (originalValue != this._horizontalScrollbar.Value)
                        handled = true;
				}

				if (handled)
				{
					this.InvalidateScrollPanel(false);
					e.Handled = true;
				}
			}
		}

		#endregion // OnMouseWheel

        #region OnLostMouseCapture
        /// <summary>
        /// Called before the LostMouseCapture event is raised.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            this.EndSelectionDrag(true);

            base.OnLostMouseCapture(e);
        }
        #endregion // OnLostMouseCapture

		#endregion // Overrides

		#region Properties

		#region Public

		#region Columns

		/// <summary>
		/// Gets a reference to all of the Columns on the root level of the <see cref="XamGrid"/>.
		/// </summary>
		public ColumnBaseCollection Columns
		{
			get
			{
				return this._rowsManager.ColumnLayout.Columns;
			}
		}

		#endregion // Columns

		#region Rows

		/// <summary>
		/// Gets the rows on the root level of the <see cref="XamGrid"/>.
		/// </summary>
        [Browsable(false)]
		public RowCollection Rows
		{
			get
			{
				return ((RowCollection)this._rowsManager.Rows.ActualCollection);
			}
		}

		#endregion // Rows

		#region ItemsSource

		/// <summary>
		/// Identifies the <see cref="ItemsSource"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(ItemsSourceChanged)));

		/// <summary>
		/// Gets/sets the <see cref="IEnumerable"/> for the <see cref="XamGrid"/>.
		/// </summary>
		/// <remarks>
		/// This property won't take effect until the Grid has Loaded.
		/// </remarks>
		public IEnumerable ItemsSource
		{
			get { return this._rowsManager.ItemsSource; }
			set { this.SetValue(ItemsSourceProperty, value); }
		}

		private static void ItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;

			if (grid.IsLoaded)
			{
                if (grid.Panel != null)
                {
                    if (grid.CurrentEditCell != null || grid.CurrentEditRow != null)
                    {
                        



                        

                        if (!grid.ExitEditMode(true))
                            return;
                    }

                    grid.Panel.ResetRows(true);
                }
				grid.ApplyItemSource((IEnumerable)e.NewValue);
			}
		}

		#endregion // ItemsSource

		#region ColumnLayouts

		/// <summary>
		/// Gets a collection of ColumnLayouts that will be used when setting up the data of 
		/// the <see cref="XamGrid"/>.
		/// </summary>
		public ColumnLayoutCollection ColumnLayouts
		{
			get
			{
				if (this._columnLayouts == null)
				{
					this._columnLayouts = new ColumnLayoutCollection(this);
					this._columnLayouts.CollectionChanged += ColumnLayouts_CollectionChanged;
				}
				return this._columnLayouts;
			}
		}

		#endregion // ColumnLayouts

		#region MaxDepth

		/// <summary>
		/// Identifies the <see cref="MaxDepth"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MaxDepthProperty = DependencyProperty.Register("MaxDepth", typeof(int), typeof(XamGrid), new PropertyMetadata(int.MaxValue, new PropertyChangedCallback(MaxDepthChanged)));

		/// <summary>
		/// Gets/sets the maximum allowed depth of the hierarchy of the <see cref="XamGrid"/>.
		/// This property is useful for when the data source provided as an infinite recursion.
		/// </summary>
		/// <remarks>
		/// This property is essentially zero based, as the root level would be considered a MaxDepth of 0. 
		/// For example: setting this property to 1, would mean that you would only have 1 level other than the root level. 
		/// </remarks>
		public int MaxDepth
		{
			get { return (int)this.GetValue(MaxDepthProperty); }
			set { this.SetValue(MaxDepthProperty, value); }
		}

		private static void MaxDepthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion // MaxDepth

		#region HeaderVisibility

		/// <summary>
		/// Identifies the <see cref="HeaderVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.Register("HeaderVisibility", typeof(Visibility), typeof(XamGrid), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(HeaderVisibilityChanged)));

		/// <summary>
		/// Gets/sets the visibility of the Header for all ColumnLayouts in the <see cref="XamGrid"/>.
		/// </summary>
		public Visibility HeaderVisibility
		{
			get { return (Visibility)this.GetValue(HeaderVisibilityProperty); }
			set { this.SetValue(HeaderVisibilityProperty, value); }
		}

		private static void HeaderVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("HeaderVisibility");
		}

		#endregion // HeaderVisibility

		#region FooterVisibility

		/// <summary>
		/// Identifies the <see cref="FooterVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FooterVisibilityProperty = DependencyProperty.Register("FooterVisibility", typeof(Visibility), typeof(XamGrid), new PropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(FooterVisibilityChanged)));

		/// <summary>
		/// Gets/sets the visibility of the Footer for all ColumnLayouts in the <see cref="XamGrid"/>.
		/// </summary>
		public Visibility FooterVisibility
		{
			get { return (Visibility)this.GetValue(FooterVisibilityProperty); }
			set { this.SetValue(FooterVisibilityProperty, value); }
		}

		private static void FooterVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("FooterVisibility");
		}

		#endregion // FooterVisibility

		#region AutoGenerateColumns

		/// <summary>
		/// Identifies the <see cref="AutoGenerateColumns"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AutoGenerateColumnsProperty = DependencyProperty.Register("AutoGenerateColumns", typeof(bool), typeof(XamGrid), new PropertyMetadata(true, new PropertyChangedCallback(AutoGenerateColumnsChanged)));

		/// <summary>
		/// Gets/sets whether the columns of all <see cref="ColumnLayout"/> objects of this <see cref="XamGrid"/> should be generated if otherwise not specified.
		/// </summary>
		public bool AutoGenerateColumns
		{
			get { return (bool)this.GetValue(AutoGenerateColumnsProperty); }
			set { this.SetValue(AutoGenerateColumnsProperty, value); }
		}

		private static void AutoGenerateColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("AutoGenerateColumns");
		}

		#endregion // AutoGenerateColumns

		#region Indentation

		/// <summary>
		/// Identifies the <see cref="Indentation"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation", typeof(double), typeof(XamGrid), new PropertyMetadata(30.0, new PropertyChangedCallback(IndentationChanged)));

		/// <summary>
		/// Gets/Sets the Indentation of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public double Indentation
		{
			get { return (double)this.GetValue(IndentationProperty); }
			set { this.SetValue(IndentationProperty, value); }
		}

		private static void IndentationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("Indentation");
		}

		#endregion // Indentation

		#region ColumnLayoutHeaderVisibility

		/// <summary>
		/// Identifies the <see cref="ColumnLayoutHeaderVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnLayoutHeaderVisibilityProperty = DependencyProperty.Register("ColumnLayoutHeaderVisibility", typeof(ColumnLayoutHeaderVisibility), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(ColumnLayoutHeaderVisibilityChanged)));

		/// <summary>
		/// Gets/sets when a header should be displayed for all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>. 
		/// The header generally contains the propertyName of the data field that owns the collection that the <see cref="ColumnLayout"/> is displaying.
		/// </summary>
		public ColumnLayoutHeaderVisibility ColumnLayoutHeaderVisibility
		{
			get { return (ColumnLayoutHeaderVisibility)this.GetValue(ColumnLayoutHeaderVisibilityProperty); }
			set { this.SetValue(ColumnLayoutHeaderVisibilityProperty, value); }
		}

		private static void ColumnLayoutHeaderVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("ColumnLayoutHeaderVisibility");
		}

		#endregion // ColumnLayoutHeaderVisibility

		#region IsAlternateRowsEnabled

		/// <summary>
		/// Identifies the <see cref="IsAlternateRowsEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsAlternateRowsEnabledProperty = DependencyProperty.Register("IsAlternateRowsEnabled", typeof(bool), typeof(XamGrid), new PropertyMetadata(true, new PropertyChangedCallback(IsAlternateRowsEnabledChanged)));

		/// <summary>
		/// Gets/sets whether Alternate Row styling is enabled.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool IsAlternateRowsEnabled
		{
			get { return (bool)this.GetValue(IsAlternateRowsEnabledProperty); }
			set { this.SetValue(IsAlternateRowsEnabledProperty, value); }
		}

		private static void IsAlternateRowsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
            grid.CachedIsAlternateRowsEnabled = (bool)e.NewValue;
			grid.OnPropertyChanged("IsAlternateRowsEnabled");
		}

		#endregion // IsAlternateRowsEnabled

		#region CellStyle

		/// <summary>
		/// Identifies the <see cref="CellStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty CellStyleProperty = DependencyProperty.Register("CellStyle", typeof(Style), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(CellStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to all <see cref="CellControl"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style CellStyle
		{
			get { return (Style)this.GetValue(CellStyleProperty); }
			set { this.SetValue(CellStyleProperty, value); }
		}

		private static void CellStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
            ControlTemplate controlTemplate = null;
            grid.StrippedCellStyleForConditionalFormatting = XamGrid.CloneStyleWithoutControlTemplate(e.NewValue as Style, out controlTemplate);
            grid.ControlTemplateForConditionalFormatting = controlTemplate;
			grid.ResetPanelRows();
			grid.OnPropertyChanged("CellStyle");
		}

		#endregion // CellStyle

		#region HeaderStyle

		/// <summary>
		/// Identifies the <see cref="HeaderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(HeaderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to all <see cref="HeaderCellControl"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style HeaderStyle
		{
			get { return (Style)this.GetValue(HeaderStyleProperty); }
			set { this.SetValue(HeaderStyleProperty, value); }
		}

		private static void HeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.ResetPanelRows();
			grid.OnPropertyChanged("HeaderStyle");
		}

		#endregion // HeaderStyle

		#region FooterStyle

		/// <summary>
		/// Identifies the <see cref="FooterStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FooterStyleProperty = DependencyProperty.Register("FooterStyle", typeof(Style), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(FooterStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to all <see cref="FooterCellControl"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style FooterStyle
		{
			get { return (Style)this.GetValue(FooterStyleProperty); }
			set { this.SetValue(FooterStyleProperty, value); }
		}

		private static void FooterStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.ResetPanelRows();
			grid.OnPropertyChanged("FooterStyle");
		}

		#endregion // FooterStyle

		#region FixedRowSeparatorStyle

		/// <summary>
		/// Identifies the <see cref="FixedRowSeparatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedRowSeparatorStyleProperty = DependencyProperty.Register("FixedRowSeparatorStyle", typeof(Style), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(FixedRowSeparatorStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied <see cref="FixedRowSeparator"/> objects that separate fixed rows in the <see cref="XamGrid"/>
		/// </summary>
		public Style FixedRowSeparatorStyle
		{
			get { return (Style)this.GetValue(FixedRowSeparatorStyleProperty); }
			set { this.SetValue(FixedRowSeparatorStyleProperty, value); }
		}

		private static void FixedRowSeparatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("FixedRowSeparatorStyle");
		}

		#endregion // FixedRowSeparatorStyle

		#region ChildBandHeaderStyle

		/// <summary>
		/// Identifies the <see cref="ChildBandHeaderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ChildBandHeaderStyleProperty = DependencyProperty.Register("ChildBandHeaderStyle", typeof(Style), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(ChildBandHeaderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to all <see cref="ChildBandCellControl"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style ChildBandHeaderStyle
		{
			get { return (Style)this.GetValue(ChildBandHeaderStyleProperty); }
			set { this.SetValue(ChildBandHeaderStyleProperty, value); }
		}

		private static void ChildBandHeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.ResetPanelRows();
			grid.OnPropertyChanged("ChildBandHeaderStyle");
		}

		#endregion // ChildBandHeaderStyle

		#region ColumnsHeaderTemplate

		/// <summary>
		/// Identifies the <see cref="ColumnsHeaderTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("ColumnsHeaderTemplate", typeof(DataTemplate), typeof(XamGrid), new PropertyMetadata(null, new PropertyChangedCallback(ColumnsHeaderTemplateChanged)));

		/// <summary>
		/// Defines the <see cref="DataTemplate"/> that should be applied to every header in the <see cref="XamGrid"/>
		/// </summary>
		public DataTemplate ColumnsHeaderTemplate
		{
			get { return (DataTemplate)this.GetValue(HeaderTemplateProperty); }
			set { this.SetValue(HeaderTemplateProperty, value); }
		}

		private static void ColumnsHeaderTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("ColumnsHeaderTemplate");
		}

		#endregion // ColumnsHeaderTemplate

		#region ColumnsFooterTemplate

		/// <summary>
		/// Identifies the <see cref="ColumnsFooterTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnsFooterTemplateProperty = DependencyProperty.Register("ColumnsFooterTemplate", typeof(DataTemplate), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(ColumnsFooterTemplateChanged)));

		/// <summary>
		/// Defines the <see cref="DataTemplate"/> that should be applied to every footer in the <see cref="XamGrid"/>
		/// </summary>
		public DataTemplate ColumnsFooterTemplate
		{
			get { return (DataTemplate)this.GetValue(ColumnsFooterTemplateProperty); }
			set { this.SetValue(ColumnsFooterTemplateProperty, value); }
		}

		private static void ColumnsFooterTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("ColumnsFooterTemplate");
		}

		#endregion // ColumnsFooterTemplate

		#region RowSelectorSettings

		/// <summary>
		/// Gets a reference to the <see cref="RowSelectorSettings"/> object that controls all the properties of the <see cref="RowSelectorColumn"/> of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public RowSelectorSettings RowSelectorSettings
		{
			get
			{
				if (this._rowSelectorSettings == null)
					this._rowSelectorSettings = new RowSelectorSettings();

				this._rowSelectorSettings.Grid = this;

				return this._rowSelectorSettings;
			}
			set
			{
				if (value != this._rowSelectorSettings)
				{
					this._rowSelectorSettings = value;
					this.OnPropertyChanged("RowSelectorSettings");
				}
			}
		}

		#endregion // RowSelectorSettings

		#region ExpansionIndicatorSettings

		/// <summary>
		/// Gets a reference to the <see cref="ExpansionIndicatorSettings"/> object that controls all the properties of the <see cref="ExpansionIndicatorColumn"/> of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public ExpansionIndicatorSettings ExpansionIndicatorSettings
		{
			get
			{
				if (this._expansionIndicators == null)
					this._expansionIndicators = new ExpansionIndicatorSettings();

				this._expansionIndicators.Grid = this;

				return this._expansionIndicators;
			}
			set
			{
				if (value != this._expansionIndicators)
				{
					this._expansionIndicators = value;
					this.OnPropertyChanged("ExpansionIndicatorSettings");
				}
			}
		}

		#endregion // ExpansionIndicatorSettings

		#region AddNewRowSettings
		/// <summary>
		/// Gets a reference to the <see cref="AddNewRowSettings"/> object that controls all the properties of the <see cref="AddNewRow"/> of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public AddNewRowSettings AddNewRowSettings
		{
			get
			{
				if (this._addNewRowSettings == null)
					this._addNewRowSettings = new AddNewRowSettings();

				this._addNewRowSettings.Grid = this;

				return this._addNewRowSettings;
			}
			set
			{
				if (value != this._addNewRowSettings)
				{
					this._addNewRowSettings = value;
					this.OnPropertyChanged("AddNewRowSettings");
				}
			}

		}
		#endregion // AddNewRowSettings

		#region SummaryRowSettings
		/// <summary>
		/// Gets a reference to the <see cref="SummaryRowSettings"/> object that controls all the properties of the <see cref="SummaryRow"/> of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public SummaryRowSettings SummaryRowSettings
		{
			get
			{
				if (this._summaryRowSettings == null)
					this._summaryRowSettings = new SummaryRowSettings();

				this._summaryRowSettings.Grid = this;

				return this._summaryRowSettings;
			}
			set
			{
				if (value != this._summaryRowSettings)
				{
					this._summaryRowSettings = value;
					this.OnPropertyChanged("SummaryRowSettings");
				}
			}
		}
		#endregion // SummaryRowSettings

		#region FilteringSettings
		/// <summary>
		/// Gets a reference to the <see cref="FilteringSettings"/> object that controls all the properties of the <see cref="FilterRow"/> of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public FilteringSettings FilteringSettings
		{
			get
			{
				if (this._filteringSettings == null)
				{
					this._filteringSettings = new FilteringSettings();
				}

				this._filteringSettings.Grid = this;

				return this._filteringSettings;
			}
			set
			{
				if (this._filteringSettings != value)
				{
					this._filteringSettings = value;
					this.OnPropertyChanged("FilteringSettings");
				}
			}
		}
		#endregion // FilteringSettings

		#region ColumnMovingSettings


		/// <summary>
		/// Gets a reference to the <see cref="ColumnMovingSettings"/> object that controls all the properties concerning column moving of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public ColumnMovingSettings ColumnMovingSettings
		{
			get
			{
				if (this._columnMovingSettings == null)
					this._columnMovingSettings = new ColumnMovingSettings();

				this._columnMovingSettings.Grid = this;

				return this._columnMovingSettings;
			}
			set
			{
				if (value != this._columnMovingSettings)
				{
					this._columnMovingSettings = value;
					this.OnPropertyChanged("ColumnMovingSettings");
				}
			}
		}

		#endregion // ColumnMovingSettings

		#region EditingSettings

		/// <summary>
		/// Gets a reference to the <see cref="EditingSettings"/> object that controls all the properties concerning editing of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public EditingSettings EditingSettings
		{
			get
			{
				if (this._editingSettings == null)
					this._editingSettings = new EditingSettings();

				this._editingSettings.Grid = this;

				return this._editingSettings;
			}
			set
			{
				if (value != this._editingSettings)
				{
					this._editingSettings = value;
					this.OnPropertyChanged("EditingSettings");
				}
			}
		}
		#endregion // EditingSettings

		#region FixedColumnSettings

		/// <summary>
		/// Gets a reference to the <see cref="FixedColumnSettings"/> object that controls all the properties concerning locking <see cref="Column"/> objects to the left or right side of the <see cref="XamGrid"/>
		/// </summary>
		public FixedColumnSettings FixedColumnSettings
		{
			get
			{
				if (this._fixedColumnSettings == null)
					this._fixedColumnSettings = new FixedColumnSettings();

				this._fixedColumnSettings.Grid = this;

				return this._fixedColumnSettings;
			}
			set
			{
				if (value != this._fixedColumnSettings)
				{
					this._fixedColumnSettings = value;
					this.OnPropertyChanged("FixedColumnSettings");
				}
			}
		}

		#endregion // FixedColumnSettings

		#region SortingSettings
		/// <summary>
		/// Gets a reference to the <see cref="SortingSettings"/> object that controls all the properties concerning sorting of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public SortingSettings SortingSettings
		{
			get
			{
				if (this._sortingSettings == null)
					this._sortingSettings = new SortingSettings();

				this._sortingSettings.Grid = this;

				return _sortingSettings;
			}
			set
			{
				if (_sortingSettings != value)
				{
					this._sortingSettings = value;
					this.OnPropertyChanged("SortingSettings");
				}
			}
		}
		#endregion // SortingSettings

		#region GroupBySettings
		/// <summary>
		/// Gets a reference to the <see cref="GroupBySettings"/> object that controls all the properties concerning organizing data of all <see cref="ColumnLayout"/> objects by grouping their columns in this <see cref="XamGrid"/>.
		/// </summary>
		public GroupBySettings GroupBySettings
		{
			get
			{
				if (this._groupBySettings == null)
					this._groupBySettings = new GroupBySettings();

				this._groupBySettings.Grid = this;

				return this._groupBySettings;
			}
			set
			{
				if (this._groupBySettings != value)
				{
					this._groupBySettings = value;

					this.OnPropertyChanged("GroupBySettings");

					if (this._groupBySettings != null)
					{
						this._groupBySettings.Grid = this;
						this.OnPropertyChanged("GroupByInvalidated");
						this.OnPropertyChanged("AllowGroupByArea");
					}
				}
			}
		}
		#endregion // GroupBySettings

		#region PagingSettings
		/// <summary>
		/// Gets a reference to the <see cref="PagerSettings"/> object that controls all the properties concerning paging of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public PagerSettings PagerSettings
		{
			get
			{
				if (this._pagerSettings == null)
					this._pagerSettings = new PagerSettings();

				this._pagerSettings.Grid = this;

				return _pagerSettings;
			}
			set
			{
				if (this._pagerSettings != value)
				{
					this._pagerSettings = value;
					this.OnPropertyChanged("PagingSettings");
				}
			}
		}
		#endregion // PagingSettings

		#region ActiveCell

		/// <summary>
		/// Gets/sets the cell that is currently active. 
		/// </summary>
        [Browsable(false)]
		public CellBase ActiveCell
		{
			get { return this._activeCell; }
			set
			{
				this.SetActiveCell(value, CellAlignment.NotSet, InvokeAction.Code);
			}
		}

		#endregion // ActiveCell

		#region KeyboardNavigation

		/// <summary>
		/// Identifies the <see cref="KeyboardNavigation"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty KeyboardNavigationProperty = DependencyProperty.Register("KeyboardNavigation", typeof(KeyboardNavigation), typeof(XamGrid), new PropertyMetadata(KeyboardNavigation.CurrentLayout));

		/// <summary>
		/// Gets/Sets how keyboard navigation will work in the <see cref="XamGrid"/>
		/// </summary>
		public KeyboardNavigation KeyboardNavigation
		{
			get { return (KeyboardNavigation)this.GetValue(KeyboardNavigationProperty); }
			set { this.SetValue(KeyboardNavigationProperty, value); }
		}

		#endregion // KeyboardNavigation

		#region SelectionSettings

		/// <summary>
        /// Gets a reference to the <see cref="SelectionSettings"/> object that controls all the properties of Selection.
		/// </summary>
		public SelectionSettings SelectionSettings
		{
			get
			{
				if (this._selectionSettings == null)
					this._selectionSettings = new SelectionSettings();

				this._selectionSettings.Grid = this;

				return this._selectionSettings;
			}
			set
			{
				if (value != this._selectionSettings)
				{
					this._selectionSettings = value;
					this.OnPropertyChanged("SelectionSettings");
				}
			}
		}

		#endregion // SelectionSettings

		#region DeferredScrollingSettings

		/// <summary>
        /// Gets a reference to the <see cref="DeferredScrollingSettings"/> object that controls all the properties of Deferred Scrolling.
		/// </summary>
		public DeferredScrollingSettings DeferredScrollingSettings
		{
			get
			{
				if (this._deferredScrollingSettings == null)
					this._deferredScrollingSettings = new DeferredScrollingSettings();

				this._deferredScrollingSettings.Grid = this;

				return this._deferredScrollingSettings;
			}
			set
			{
				if (value != this._deferredScrollingSettings)
				{
					this._deferredScrollingSettings = value;
					this.OnPropertyChanged("DeferredScrollingSettings");
				}
			}
		}

		#endregion // DeferredScrollingSettings

		#region ColumnMapping

		/// <summary>
		/// Gets a collection of <see cref="ColumnTypeMapping"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks>
		/// This collection should be used to map a specific data type to a certain Column type for the AutoGeneration of columns
		/// in the <see cref="XamGrid"/>.
		/// </remarks>
        [Browsable(false)]
		public ColumnTypeMappingsCollection ColumnTypeMappings
		{
			get
			{
				if (this._columnTypeMappings == null)
				{
					this._columnTypeMappings = new ColumnTypeMappingsCollection();
					this._columnTypeMappings.Add(new ColumnTypeMapping() { DataType = typeof(bool), ColumnType = typeof(CheckBoxColumn) });
					this._columnTypeMappings.Add(new ColumnTypeMapping() { DataType = typeof(bool?), ColumnType = typeof(CheckBoxColumn) });
					this._columnTypeMappings.Add(new ColumnTypeMapping() { DataType = typeof(Uri), ColumnType = typeof(HyperlinkColumn) });
					this._columnTypeMappings.Add(new ColumnTypeMapping() { DataType = typeof(IEnumerable), ColumnType = typeof(ColumnLayout) });
					this._columnTypeMappings.Add(new ColumnTypeMapping() { DataType = typeof(DateTime), ColumnType = typeof(DateColumn) });
					this._columnTypeMappings.Add(new ColumnTypeMapping() { DataType = typeof(DateTime?), ColumnType = typeof(DateColumn) });
					this._columnTypeMappings.Add(new ColumnTypeMapping() { DataType = typeof(BitmapImage), ColumnType = typeof(ImageColumn) });
				}
				return this._columnTypeMappings;
			}
		}

		#endregion // ColumnMapping

		#region ColumnResizingSettings

		/// <summary>
		/// Gets a reference to the <see cref="ColumnResizingSettings"/> object that controls all the properties concerning column resizing of all <see cref="ColumnLayout"/> objects in this <see cref="XamGrid"/>.
		/// </summary>
		public ColumnResizingSettings ColumnResizingSettings
		{
			get
			{
				if (this._columnResizingSettings == null)
					this._columnResizingSettings = new ColumnResizingSettings();

				this._columnResizingSettings.Grid = this;

				return this._columnResizingSettings;
			}
			set
			{
				if (value != this._columnResizingSettings)
				{
					this._columnResizingSettings = value;
					this.OnPropertyChanged("ColumnResizingSettings");
				}
			}
		}

		#endregion // ColumnMovingSettings

		#region RowHeight

		/// <summary>
		/// Identifies the <see cref="RowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register("RowHeight", typeof(RowHeight), typeof(XamGrid), new PropertyMetadata(RowHeight.SizeToLargestCell, new PropertyChangedCallback(RowHeightChanged)));

		/// <summary>
		/// Gets/Sets the Height that will be applied to every row in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight RowHeight
		{
			get { return (RowHeight)this.GetValue(RowHeightProperty); }
			set { this.SetValue(RowHeightProperty, value); }
		}

		private static void RowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("RowHeight");
		}

		#endregion // RowHeight

		#region MinimumRowHeight

		/// <summary>
		/// Identifies the <see cref="MinimumRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MinimumRowHeightProperty = DependencyProperty.Register("MinimumRowHeight", typeof(double), typeof(XamGrid), new PropertyMetadata(26.0, new PropertyChangedCallback(MinimumRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the Minimum Height for every <see cref="RowBase"/> for every <see cref="ColumnLayout"/> in the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks>
		/// This value is ignored if RowHeight is of Type Numeric.
		/// </remarks>
		public double MinimumRowHeight
		{
			get { return (double)this.GetValue(MinimumRowHeightProperty); }
			set { this.SetValue(MinimumRowHeightProperty, value); }
		}

		private static void MinimumRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("MinimumRowHeight");
		}

		#endregion // MinimumRowHeight

		#region DeleteKeyAction

		/// <summary>
		/// Identifies the <see cref="DeleteKeyAction"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty DeleteKeyActionProperty = DependencyProperty.Register("DeleteKeyAction", typeof(DeleteKeyAction), typeof(XamGrid), new PropertyMetadata(DeleteKeyAction.None));

		/// <summary>
		/// Gets/Sets the action that should be taken when the Delete Key is pressed for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>.
		/// </summary>
		public DeleteKeyAction DeleteKeyAction
		{
			get { return (DeleteKeyAction)this.GetValue(DeleteKeyActionProperty); }
			set { this.SetValue(DeleteKeyActionProperty, value); }
		}

		#endregion // DeleteKeyAction

		#region HeaderRowHeight

		/// <summary>
		/// Identifies the <see cref="HeaderRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty HeaderRowHeightProperty = DependencyProperty.Register("HeaderRowHeight", typeof(RowHeight), typeof(XamGrid), new PropertyMetadata(RowHeight.SizeToLargestCell, new PropertyChangedCallback(HeaderRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="RowHeight"/> for the <see cref="HeaderRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight HeaderRowHeight
		{
			get { return (RowHeight)this.GetValue(HeaderRowHeightProperty); }
			set { this.SetValue(HeaderRowHeightProperty, value); }
		}

		private static void HeaderRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("HeaderRowHeight");
		}

		#endregion // HeaderRowHeight

		#region FooterRowHeight

		/// <summary>
		/// Identifies the <see cref="FooterRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FooterRowHeightProperty = DependencyProperty.Register("FooterRowHeight", typeof(RowHeight), typeof(XamGrid), new PropertyMetadata(RowHeight.SizeToLargestCell, new PropertyChangedCallback(FooterRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="RowHeight"/> for the <see cref="FooterRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
        [TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight FooterRowHeight
		{
			get { return (RowHeight)this.GetValue(FooterRowHeightProperty); }
			set { this.SetValue(FooterRowHeightProperty, value); }
		}

		private static void FooterRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("FooterRowHeight");
		}

		#endregion // FooterRowHeight

		#region ChildBandHeaderHeight

		/// <summary>
		/// Identifies the <see cref="ChildBandHeaderHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ChildBandHeaderHeightProperty = DependencyProperty.Register("ChildBandHeaderHeight", typeof(RowHeight), typeof(XamGrid), new PropertyMetadata(RowHeight.SizeToLargestCell, new PropertyChangedCallback(ChildBandHeaderHeightChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="RowHeight"/> for the <see cref="ChildBand"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight ChildBandHeaderHeight
		{
			get { return (RowHeight)this.GetValue(ChildBandHeaderHeightProperty); }
			set { this.SetValue(ChildBandHeaderHeightProperty, value); }
		}

		private static void ChildBandHeaderHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("ChildBandHeaderHeight");
		}

		#endregion // ChildBandHeaderHeight

		#region ColumnWidth

		/// <summary>
		/// Identifies the <see cref="ColumnWidth"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register("ColumnWidth", typeof(ColumnWidth), typeof(XamGrid), new PropertyMetadata(ColumnWidth.InitialAuto, new PropertyChangedCallback(ColumnWidthChanged)));

		/// <summary>
		/// Gets/Sets the Width that will be applied to every <see cref="Column"/> in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(ColumnWidthTypeConverter))]
		public ColumnWidth ColumnWidth
		{
			get { return (ColumnWidth)this.GetValue(ColumnWidthProperty); }
			set { this.SetValue(ColumnWidthProperty, value); }
		}

		private static void ColumnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamGrid grid = (XamGrid)obj;
			grid.OnPropertyChanged("ColumnWidth");
		}

		#endregion // ColumnWidth

		#region RowHover

		/// <summary>
		/// Identifies the <see cref="RowHover"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowHoverProperty = DependencyProperty.Register("RowHover", typeof(RowHoverType), typeof(XamGrid), new PropertyMetadata(new PropertyChangedCallback(RowHoverChanged)));

		/// <summary>
		/// Gets/Sets what should happen when the mouse is over a <see cref="Row"/>.
		/// </summary>
		public RowHoverType RowHover
		{
			get { return (RowHoverType)this.GetValue(RowHoverProperty); }
			set { this.SetValue(RowHoverProperty, value); }
		}

		private static void RowHoverChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion // RowHover

		#region FillerColumnSettings

		/// <summary>
		/// Gets an object that contains settings that pertain to the <see cref="FillerColumn"/> of all <see cref="ColumnLayout"/> objects of the <see cref="XamGrid"/>.
		/// </summary>
		public FillerColumnSettings FillerColumnSettings
		{
			get
			{
				if (this._fillerColumnSettings == null)
					this._fillerColumnSettings = new FillerColumnSettings();

				this._fillerColumnSettings.Grid = this;

				return this._fillerColumnSettings;
			}
			set
			{
				if (value != this._fillerColumnSettings)
				{
					this._fillerColumnSettings = value;
					this.OnPropertyChanged("FillerColumnSettings");
				}
			}
		}

		#endregion // FillerColumnSettings

        #region HeaderTextHorizontalAlignment

        /// <summary>
        /// Identifies the <see cref="HeaderTextHorizontalAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTextHorizontalAlignmentProperty = DependencyProperty.Register("HeaderTextHorizontalAlignment", typeof(HorizontalAlignment), typeof(XamGrid), new PropertyMetadata(HorizontalAlignment.Left, new PropertyChangedCallback(HeaderTextHorizontalAlignmentChanged)));

        /// <summary>
        /// Gets/sets  the <see cref="HorizontalAlignment"/> of the content for each <see cref="Column"/> in the <see cref="XamGrid"/>
        /// </summary>
        public HorizontalAlignment HeaderTextHorizontalAlignment
        {
            get { return (HorizontalAlignment)this.GetValue(HeaderTextHorizontalAlignmentProperty); }
            set { this.SetValue(HeaderTextHorizontalAlignmentProperty, value); }
        }

        private static void HeaderTextHorizontalAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamGrid grid = (XamGrid)obj;
            grid.OnPropertyChanged("HeaderTextHorizontalAlignment");
        }

        #endregion // HeaderTextHorizontalAlignment

        #region HeaderTextVerticalAlignment

        /// <summary>
        /// Identifies the <see cref="HeaderTextVerticalAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTextVerticalAlignmentProperty = DependencyProperty.Register("HeaderTextVerticalAlignment", typeof(VerticalAlignment), typeof(XamGrid), new PropertyMetadata(VerticalAlignment.Center, new PropertyChangedCallback(HeaderTextVerticalAlignmentChanged)));

        /// <summary>
        /// Gets/sets  the <see cref="VerticalAlignment"/> of the content for each <see cref="Column"/> in the <see cref="XamGrid"/>
        /// </summary>
        public VerticalAlignment HeaderTextVerticalAlignment
        {
            get { return (VerticalAlignment)this.GetValue(HeaderTextVerticalAlignmentProperty); }
            set { this.SetValue(HeaderTextVerticalAlignmentProperty, value); }
        }

        private static void HeaderTextVerticalAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamGrid grid = (XamGrid)obj;
            grid.OnPropertyChanged("HeaderTextVerticalAlignment");
        }

        #endregion // HeaderTextVerticalAlignment

		#region ConditionalFormattingSettings

		/// <summary>
		/// Gets an object that contains settings that pertain to the Conditional Formatting of all <see cref="ColumnLayout"/> objects of the <see cref="XamGrid"/>.
		/// </summary>
		public ConditionalFormattingSettings ConditionalFormattingSettings
		{
			get
			{
				if (this._conditionalFormattingSettings == null)
				{
					this._conditionalFormattingSettings = new ConditionalFormattingSettings();
					this._conditionalFormattingSettings.PropertyChanged += ConditionalFormattingSettings_PropertyChanged;
				}

				this._conditionalFormattingSettings.Grid = this;

				return this._conditionalFormattingSettings;
			}
			set
			{
				if (value != this._conditionalFormattingSettings)
				{
					if (this._conditionalFormattingSettings != null)
						this._conditionalFormattingSettings.PropertyChanged -= ConditionalFormattingSettings_PropertyChanged;

					this._conditionalFormattingSettings = value;

					if (this._conditionalFormattingSettings != null)
						this._conditionalFormattingSettings.PropertyChanged += ConditionalFormattingSettings_PropertyChanged;

					this.OnPropertyChanged("ConditionalFormattingSettings");
				}
			}
		}

		#endregion // ConditionalFormattingSettings

        #region ColumnChooserSettings

        /// <summary>
        /// Gets an object that contains settings that pertain to the ColumnChooser of all <see cref="ColumnLayout"/> objects of the <see cref="XamGrid"/>.
        /// </summary>
        public ColumnChooserSettings ColumnChooserSettings
        {
            get
            {
                if (this._columnChooserSettings == null)
                {
                    this._columnChooserSettings = new ColumnChooserSettings();
                }

                if (this._columnChooserSettings.Grid != this)
                {
                    this._columnChooserSettings.Grid = this;

                    if(this.ColumnChooserDialog.Style != this._columnChooserSettings.Style)
                        this.ColumnChooserDialog.Style = this._columnChooserSettings.Style;
                }
                                
                return this._columnChooserSettings;
            }
            set
            {
                if (value != this._columnChooserSettings)
                {
                    this._columnChooserSettings = value;

                    if (this._columnChooserSettings != null)
                    {
                        if (this.ColumnChooserDialog.Style != this._columnChooserSettings.Style)
                            this.ColumnChooserDialog.Style = this._columnChooserSettings.Style;
                    }

                    this.OnPropertyChanged("ColumnChooserSettings");
                }
            }
        }

        #endregion // ColumnChooserSettings

		#region ClipboardSettings

		/// <summary>
		/// Gets an object that contains settings that pertain to Clipboard actions.
		/// </summary>
		public ClipboardSettings ClipboardSettings
		{
			get
			{
				if (this._clipBoardSettings == null)
				{
					this._clipBoardSettings = new ClipboardSettings();
				}

				if (this._clipBoardSettings.Grid != this)
				{
					this._clipBoardSettings.Grid = this;
				}

				return this._clipBoardSettings;
			}

			set
			{
				if (value != this._clipBoardSettings)
				{
					this._clipBoardSettings = value;

					this.OnPropertyChanged("ClipboardSettings");
				}
			}
		}

		#endregion // ClipboardSettings

        #region ActiveItem

        /// <summary>
        /// Identifies the <see cref="ActiveItem"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ActiveItemProperty = DependencyProperty.Register("ActiveItem", typeof(object), typeof(XamGrid), new PropertyMetadata(null, new PropertyChangedCallback(ActiveItemChanged)));

        /// <summary>
        /// Gets/sets the underlying data object of the <see cref="ActiveCell"/>.
        /// </summary>
        /// <remarks>
        /// If a <see cref="Cell"/> in a childband is the ActiveCell, the ActiveItem will be null.
        /// </remarks>
        public object ActiveItem
        {
            get { return (object)this.GetValue(ActiveItemProperty); }
            set { this.SetValue(ActiveItemProperty, value); }
        }

        private static void ActiveItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamGrid grid = (XamGrid)obj;
            grid.OnPropertyChanged("ActiveItem");

            if (grid.RowsManager != null)
                grid.RowsManager.UpdateCurrentItem(e.NewValue);

            if (!grid._ignoreActiveItemChanging)
            {
                object data = e.NewValue;

                Row activeRow = null;

                if (grid.GroupBySettings.GroupByColumns[grid.RowsManager.ColumnLayout].Count > 0)
                {
                    // If GroupByMerging is on
                    if (grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                    {
                        // And this ColumnLayout has a Merged row, then the data in the DataManager is going to be of type
                        // MergedRowInfo, which means we can't look at the data straight up. 
                        // So instead, created a new MergedRowInfo, which should work, b/c we override equals to look at the 
                        // MergedRowInfo.Data property for comparison purposes. 

                        data = new MergedRowInfo() { Data = data };
                    }
                    else
                    {
                        // GroupByRows are trickier, so we need walk through all child rowsmanagers and find the one with this particular data
                        activeRow = grid.WalkDownGroupByRowsForDataItem(grid.Rows, data);
                    }
                }

                if (activeRow == null)
                {
                    DataManagerBase dmb = grid.RowsManager.DataManager;
                    if (dmb != null)
                    {
                        int index = dmb.ResolveIndexForRecord(data);
                        if (index > -1)
                        {
                            activeRow = grid.Rows[index];
                        }
                        else
                        {
                            grid._ignoreActiveItemChanging = true;

                            if (grid.ActiveCell != null && e.NewValue != null)
                            {
                                grid.ActiveItem = grid.ActiveCell.Row.Data;
                            }
                            else
                            {
                                grid.ActiveItem = null;
                                grid.ActiveCell = null;
                            }
                            grid._ignoreActiveItemChanging = false;
                        }
                    }
                }
                
                if(activeRow != null)
                {
                    if (grid.ActiveCell == null || grid.ActiveCell.Row != activeRow)
                    {
                        ReadOnlyKeyedColumnBaseCollection<Column> columns = activeRow.Columns.AllVisibleChildColumns;
                        if (columns.Count > 0)
                        {
                            CellBase ac = activeRow.Cells[columns[0]];
                            ac.IsActive = true;

                            // If it's not active...then it got cancelled, so just revert it back to it's old value.
                            if (grid.ActiveCell != ac)
                            {
                                grid._ignoreActiveItemChanging = true;
                                grid.ActiveItem = e.OldValue;
                                grid._ignoreActiveItemChanging = false;
                            }
                        }
                    }
                }
            }
        }

        #endregion // ActiveItem 

        #region DataManagerProvider

        /// <summary>
        /// Gets/Sets the <see cref="DataManagerProvider"/> that should be used when generating a <see cref="DataManagerBase"/> that should be
        /// used to handle the data.
        /// </summary>
        /// <remarks>
        /// This should only be set at design time, or before an ItemsSource has been set. Otherwise, the property will be ignored.
        /// </remarks>
        public DataManagerProvider DataManagerProvider
        {
            get { return this._dataManagerProvider; }
            set
            {
                if (this._dataManagerProvider != value)
                {
                    // We will only allow this to be set, if we haven't loaded our ItemsSource yet.
                    if (this.ItemsSource == null || !this.IsLoaded)
                        this._dataManagerProvider = value;
                }
            }
        }

        #endregion // DataManagerProvider

        #region IsTouchSupportEnabled

        /// <summary>
        /// Identifies the <see cref="IsTouchSupportEnabled"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsTouchSupportEnabledProperty = DependencyPropertyUtilities.Register("IsTouchSupportEnabled",
               typeof(bool), typeof(XamGrid),
               DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsTouchSupportEnabledChanged))
               );

        private static void OnIsTouchSupportEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamGrid instance = (XamGrid)d;

            if (instance._scrollHelper != null)
                instance._scrollHelper.IsEnabled = (bool)e.NewValue;


            if (instance._panel!=null)
                instance._panel.SetCurrentValue(FrameworkElement.IsManipulationEnabledProperty, KnownBoxes.FromValue((bool)e.NewValue));

        }

        /// <summary>
        /// Returns or sets whether touch support is enabled for this control
        /// </summary>
        /// <seealso cref="IsTouchSupportEnabledProperty"/>
        public bool IsTouchSupportEnabled
        {
            get
            {
                return (bool)this.GetValue(XamGrid.IsTouchSupportEnabledProperty);
            }
            set
            {
                this.SetValue(XamGrid.IsTouchSupportEnabledProperty, value);
            }
        }

        #endregion //IsTouchSupportEnabled


		#endregion // Public

		#region Protected

		#region VerticalScrollBar

		/// <summary>
		/// Gets a reference to the vertical scrollbar that is attached to the <see cref="XamGrid"/>.
		/// If a vertical scrollbar was not specified, this property will return null.
		/// </summary>
		protected virtual ScrollBar VerticalScrollBar
		{
			get { return this._verticalScrollbar; }
		}
		#endregion // VerticalScrollBar

		#region HorizontalScrollBar

		/// <summary>
		/// Gets a reference to the horizontal scrollbar that is attached to the <see cref="XamGrid"/>.
		/// If a horizontal scrollbar was not specified, this property will return null.
		/// </summary>
		protected virtual ScrollBar HorizontalScrollBar
		{
			get { return this._horizontalScrollbar; }
		}
		#endregion // HorizontalScrollBar

		#region RowsManager
		/// <summary>
		/// Gets a reference to the RowsManager that is used by the <see cref="XamGrid"/>.
		/// </summary>
		protected internal XamGridRowsManager RowsManager
		{
			get
			{
				return _rowsManager;
			}
		}
		#endregion // RowsManager

		#region CurrentEditCell
		/// <summary>
		/// Gets/sets the <see cref="Cell"/> that is currently in edit mode.
		/// </summary>
		protected internal Cell CurrentEditCell
		{
			get;
			set;
		}
		#endregion // CurrentEditCell

		#region CurrentEditRow
		/// <summary>
		/// Gets/sets the <see cref="Row"/> that is currently in edit mode.
		/// </summary>
		protected internal Row CurrentEditRow
		{
			get;
			set;
		}
		#endregion // CurrentEditRow

		#region Panel
		/// <summary>
		/// The visual container that holds the rows.
		/// </summary>
		protected internal RowsPanel Panel
		{
			get
			{
				return this._panel;
			}
		}
		#endregion // Panel

		#region DropAreaIndicatorLeft

		/// <summary>
		/// Gets the <see cref="DropAreaIndicator"/> that is displayed when pinning a column left by dragging.
		/// </summary>
		protected internal DropAreaIndicator DropAreaIndicatorLeft
		{
			get { return this._dropAreaIndicatorLeft; }
		}
		#endregion // DropAreaIndicatorLeft

		#region DropAreaIndicatorRight

		/// <summary>
		/// Gets the <see cref="DropAreaIndicator"/> that is displayed when pinning a column right by dragging.
		/// </summary>
		protected internal DropAreaIndicator DropAreaIndicatorRight
		{
			get { return this._dropAreaIndicatorRight; }
		}
		#endregion // DropAreaIndicatorRight

		#region MouseOverCell
		/// <summary>
		/// Gets the <see cref="CellBase"/> that the mouse is currently over.
		/// </summary>
		protected internal CellBase MouseOverCell
		{
			get { return this._mouseOverCell; }
		}
		#endregion // MouseOverCell

		#region DeferredScrollingPopup

		/// <summary>
		/// Gets the <see cref="Popup"/> used for DeferredScrolling.
		/// </summary>
		protected Popup DeferredScrollingPopup
		{
			get { return this._deferredPopup; }
		}

		#endregion // DeferredScrollingPopup

		#region IsColumnResizing

	    /// <summary>
		/// Gets / sets if there is currently a cell being resized.
		/// </summary>
		protected internal bool IsColumnResizing
	    {
	        get
            {
                return this._isColumnResizing; 
            }
            set
            {
                this._isColumnResizing = value;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            }
	    }

	    #endregion

		#region FilterIcons

		/// <summary>
		/// Gets the dictionary mapping the Filter ComparisonOperator to DataTemplate which act as Icons.
		/// </summary>
		protected internal Dictionary<ComparisonOperator, DataTemplate> FilterIcons
		{
			get
			{
				if (this._filterIcons == null)
					this._filterIcons = new Dictionary<ComparisonOperator, DataTemplate>();
				return this._filterIcons;
			}
		}

		#endregion // FilterIcons

		#region SuspendConditionalFormatUpdates


		/// <summary>
		/// Gets / set if the conditional formatting needs to be updated.
		/// </summary>
		/// <remarks>
		/// This is intended to be used during editing, so that the cell can update it's value and notify the grid that the change should be noted 
		/// by conditional formatting.
		/// </remarks>
		protected internal bool SuspendConditionalFormatUpdates
		{
			get
			{
				return this._suspendConditionalFormatUpdates;
			}
			set
			{
				if (this._suspendConditionalFormatUpdates != value)
				{
					this._suspendConditionalFormatUpdates = value;

					if (!value && this.NeedConditionalFormatUpdate)
					{
						this.InvalidateData();
						this.ResetPanelRows(true);
					}

					this.NeedConditionalFormatUpdate = false;
				}
			}
		}

		#endregion // SuspendConditionalFormatUpdates

		#region NeedConditionalFormatUpdate

		/// <summary>
		/// True if the NotifyPropertyChanged from the object was raised and we need an update.
		/// </summary>
		protected internal bool NeedConditionalFormatUpdate
		{
			get;
			set;
		}

		#endregion // NeedConditionalFormatUpdate

        #region ColumnChooserDialog

        /// <summary>
        /// Gets the <see cref="ColumnChooserDialog"/> that is used to show/hide Columns at different levels of the <see cref="XamGrid"/>
        /// </summary>
        protected internal ColumnChooserDialog ColumnChooserDialog
        {
            get
            {
                if (this._columnChooserDialog == null)
                    this._columnChooserDialog = new ColumnChooserDialog();

                return this._columnChooserDialog;
            }
        }
        #endregion // ColumnChooserDialog

        #region RenderAdorners

        /// <summary>
        /// Gets a list of <see cref="XamGridRenderAdorner"/> that will plug into the RowsPanel and perform custom renderings.
        /// </summary>
        protected internal List<XamGridRenderAdorner> RenderAdorners
        {
            get 
            {
                if (this._adorners == null)
                    this._adorners = new List<XamGridRenderAdorner>();
                return this._adorners; 
            }
        }

        #endregion // RenderAdorners

        #endregion // Protected

        #region Internal

        internal InternalRowsCollection InternalRows
		{
			get
			{
				if (this._internalRows == null)
					this._internalRows = new InternalRowsCollection() { RootRowsManager = this._rowsManager };
				return this._internalRows;
			}
		}

		internal Dictionary<string, object> EditCellValues
		{
			get { return this._editCellValues; }
		}

		internal Canvas DropAreaIndicatorPanel
		{
			get { return this._dropIndicatorPanel; }
		}

		internal bool IsDeferredScrollingCurrently
		{
			get { return (this._deferredPopup != null && this._deferredPopup.IsOpen); }
		}

		internal bool IsLoaded
		{
			get
			{
				return _isLoaded;
			}
			set
			{

				_isLoaded = value;
			}
		}







        internal Style StrippedCellStyleForConditionalFormatting 
        { 
            get; 
            set; 
        }

        internal bool IgnoreActiveItemChanging
        {
            get { return this._ignoreActiveItemChanging; }
            set { this._ignoreActiveItemChanging = value; }
        }

        internal bool LoadDataFromPersistence { get; set; }

        internal ControlTemplate ControlTemplateForConditionalFormatting
        {
            get;
            set;
        }

        internal List<Cell> CellsThatCancelledEditMode
        {
            get
            {
                if (this._cellsThatCancelledEditMode == null)
                    this._cellsThatCancelledEditMode = new List<Cell>();
                return this._cellsThatCancelledEditMode;
            }

        }






        internal bool GridIsGoingToBeDestroyed { get; set; }

        internal bool CachedIsAlternateRowsEnabled
	    {
            get { return this._cachedIsAlternateRowsEnabled; }
            private set { this._cachedIsAlternateRowsEnabled = value; }
	    }

	    #endregion // Internal

		#endregion // Properties

		#region Methods

		#region Protected

		#region InvalidateScrollPanel

		/// <summary>
		/// Invalidates the content of the <see cref="RowsPanel"/>.
		/// </summary>
		/// <param propertyName="reset">True if the internal scroll information should be invalidated.</param>
		protected internal void InvalidateScrollPanel(bool reset)
		{
			this.InvalidateScrollPanel(reset, false, false);
		}

		/// <summary>
		/// Invalidates the content of the <see cref="RowsPanel"/>.
		/// </summary>
		/// <param name="reset">True if the internal scroll information should be invalidated.</param>
        /// <param name="resetVisibleRows">True if the visible rows should be released.</param>
		protected internal void InvalidateScrollPanel(bool reset, bool resetVisibleRows)
		{
			this.InvalidateScrollPanel(reset, false, resetVisibleRows);
		}

		/// <summary>
		/// Invalidates the content of the <see cref="RowsPanel"/>.
		/// </summary>
		/// <param name="reset">True if the internal scroll information should be invalidated.</param>
		/// <param name="resetScrollPosition">True if the grid should return to a zero horizontal and zero vertical position.</param>
        /// <param name="resetVisibleRows">True if the visible rows should be released.</param>
		protected internal void InvalidateScrollPanel(bool reset, bool resetScrollPosition, bool resetVisibleRows)
		{
            this.InvalidateScrollPanel(reset, resetScrollPosition, resetVisibleRows, null);
        }

        /// <summary>
        /// Invalidates the content of the <see cref="RowsPanel"/>.
        /// </summary>
        /// <param name="reset">True if the internal scroll information should be invalidated.</param>
        /// <param name="resetScrollPosition">True if the grid should return to a zero horizontal and zero vertical position.</param>
        /// <param name="resetVisibleRows">True if the visible rows should be released.</param>
        /// <param name="callback">Callback invoked after the scroll panel is invalidated.</param>
        internal void InvalidateScrollPanel(bool reset, bool resetScrollPosition, bool resetVisibleRows, EmptyDelegate callback)
        {
			if (this._panel != null && this._isLoaded)
			{
                if (callback != null)
                {
                    this._panel.InvalidateMeasure(callback);
                }
                else
                {
                    this._panel.InvalidateMeasure();
                }

				if (reset)
					this._panel.ResetCachedScrollInfo(resetVisibleRows);

				if (resetScrollPosition)
				{
					if (this._horizontalScrollbar != null)
					{
						this._horizontalScrollbar.Value = 0;
					}
					if (this._verticalScrollbar != null)
					{
						this._verticalScrollbar.Value = 0;
					}
				}
			}
		}

		#endregion // InvalidateScrollPanel

		#region ResetPanelRows

		/// <summary>
		/// Tells the panel that it should recycle all of it's rows, so that i can be completely reloaded. 
		/// </summary>
		protected internal void ResetPanelRows()
		{
			this.ResetPanelRows(false);
		}

		/// <summary>
		/// Tells the panel that it should recycle all of it's rows, so that i can be completely reloaded. 
		/// </summary>
		/// <param name="releaseCellsPanels">True if the CellsPanels should be released by the RecyclingManager.</param>
		protected internal void ResetPanelRows(bool releaseCellsPanels)
		{
			if (this.Panel != null)
				this.Panel.ResetRows(releaseCellsPanels);
		}

		#endregion // ResetPanelRows

		#region SelectRow

		/// <summary>
		/// Selects the specified <see cref="Row"/>.
		/// </summary>
		/// <param propertyName="row">The row that should be selected.</param>
		/// <param propertyName="action">The action that invoked the selection.</param>
		/// <returns>Whether something occurred during the selecting of the row that should interrupt the selection.</returns>
		protected internal virtual bool SelectRow(Row row, InvokeAction action)
		{
			bool interrupt = false;

			if (row != null)
			{
				SelectionType selectionType = this.SelectionSettings.RowSelection;
				SelectedRowsCollection previouslySelectedRows = new SelectedRowsCollection();
				previouslySelectedRows.InternalAddRangeSilently(this.SelectionSettings.SelectedRows);

				bool shiftKey = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) || (action == InvokeAction.MouseMove);
				bool ctrlKey = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

				if (!ctrlKey && !shiftKey)
				{
					this.SelectionSettings.SelectedCells.Clear();
					this.SelectionSettings.SelectedColumns.Clear();
				}

				if (!row.AllowSelection)
				{
					if (shiftKey || ctrlKey)
						return false;
				}

				if (selectionType == SelectionType.None && action == InvokeAction.Code)
					selectionType = SelectionType.Single;

				if (selectionType != SelectionType.None)
				{
					SelectedRowsCollection selectedRows = this.SelectionSettings.SelectedRows;
					if (selectionType == SelectionType.Single)
					{
						selectedRows.InternalResetItemsSilently();
						if (row.AllowSelection)
							selectedRows.SelectItem(row, false);
					}
					else
					{
						if (shiftKey)
						{
							if (selectedRows.Count > 0)
							{
								Row lastSelectedRow = selectedRows.PivotItem;
								if (lastSelectedRow == null)
									lastSelectedRow = selectedRows[selectedRows.Count - 1];

								int indexOfLastSelectedRow = this.InternalRows.IndexOf(lastSelectedRow);
								int indexOfNewSelectedRow = this.InternalRows.IndexOf(row);

								if (selectedRows.PivotItem != null)
								{
                                    selectedRows.UnselectShiftSelectedItems();
								}

								if (indexOfLastSelectedRow < indexOfNewSelectedRow)
								{
									for (int i = indexOfLastSelectedRow; i <= indexOfNewSelectedRow; i++)
									{
										Row r = this.InternalRows[i] as Row;
										if (r != null && r.AllowSelection)
											selectedRows.SelectItem(r, true);
									}
								}
								else
								{
									for (int i = indexOfLastSelectedRow; i >= indexOfNewSelectedRow; i--)
									{
										Row r = this.InternalRows[i] as Row;
										if (r != null && r.AllowSelection)
											selectedRows.SelectItem(r, true);
									}
								}
							}
							else if (row.AllowSelection)
								selectedRows.SelectItem(row, true);
						}
						else
						{
							if (ctrlKey)
							{
								if (action == InvokeAction.Click)
								{
									if (selectedRows.Contains(row))
									{
										int index = selectedRows.IndexOf(row);
										selectedRows.InternalRemoveItemSilently(index);
										interrupt = true;
									}
									else if (row.AllowSelection)
										selectedRows.SelectItem(row, false);
								}
							}
							else
							{
								selectedRows.InternalResetItemsSilently();
								if (row.AllowSelection)
									selectedRows.SelectItem(row, false);
							}
						}
					}
					this.OnSelectedRowsCollectionChanged(previouslySelectedRows, this.SelectionSettings.SelectedRows);
					this.InvalidateScrollPanel(false);
				}
			}
			return interrupt;
		}

		#endregion // SelectRow

		#region UnselectRow

		/// <summary>
		/// Unselects the specified row. 
		/// </summary>
		/// <param propertyName="row"></param>
		protected internal virtual void UnselectRow(Row row)
		{
			if (this.SelectionSettings.SelectedRows.Contains(row))
				this.SelectionSettings.SelectedRows.Remove(row);
		}

		#endregion // UnselectRow

		#region SelectCell

		/// <summary>
		/// Selects the specified <see cref="Cell"/>.
		/// </summary>
		/// <param propertyName="cell">The cell that should be selected.</param>
		/// <param propertyName="action">The action that invoked the selection.</param>
		/// <returns>Whether something occurred during the selecting of the cell that should interrupt the selection.</returns>
		protected internal virtual bool SelectCell(Cell cell, InvokeAction action)
		{
			if (cell.Column is FillerColumn || cell is GroupByCell)
				return false;

			bool interrupt = false;
			




			if (cell != null)
			{
				if (this.IsColumnResizing)
					return false;

				SelectionType selectionType = this.SelectionSettings.CellSelection;

				SelectedCellsCollection previouslySelectedCells = new SelectedCellsCollection();
				previouslySelectedCells.InternalAddRangeSilently(this.SelectionSettings.SelectedCells);

				if (action != InvokeAction.Code && cell.Row.AllowSelection)
				{
					if (this.SelectionSettings.CellClickAction == CellSelectionAction.SelectRow)
						interrupt = this.SelectRow(cell.Row as Row, action);
				}

				bool shiftKey = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) || (action == InvokeAction.MouseMove);
				bool ctrlKey = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

				if (!cell.Row.AllowSelection && selectionType == SelectionType.Multiple)
				{
					if (shiftKey || ctrlKey)
						return false;
				}

				if (action != InvokeAction.Code && this.SelectionSettings.CellClickAction == CellSelectionAction.SelectRow)
					return interrupt;

				if (!ctrlKey && !shiftKey)
				{
					this.SelectionSettings.SelectedRows.Clear();
					this.SelectionSettings.SelectedColumns.Clear();
				}

				if (selectionType == SelectionType.None && action == InvokeAction.Code)
					selectionType = SelectionType.Single;

				if (selectionType != SelectionType.None)
				{
					SelectedCellsCollection selectedCells = this.SelectionSettings.SelectedCells;
					if (selectionType == SelectionType.Single)
					{
						selectedCells.InternalResetItemsSilently();
						if (cell.Row.AllowSelection)
							selectedCells.SelectItem(cell, false);
					}
					else
					{
						if (shiftKey && selectionType == SelectionType.Multiple)
						{
							if (selectedCells.Count > 0)
							{
								Cell lastSelectedCell = selectedCells.PivotItem;
								if (lastSelectedCell == null)
									lastSelectedCell = selectedCells[selectedCells.Count - 1];

								Collection<CellBase> cells = lastSelectedCell.Row.VisibleCells;
								int indexOfLastSelectedCell = cells.IndexOf(lastSelectedCell);

								int indexOfLastSelectedRow = this.InternalRows.IndexOf(lastSelectedCell.Row);
								int indexOfNewSelectedRow = this.InternalRows.IndexOf(cell.Row);

								if (selectedCells.PivotItem != null)
								{
                                    selectedCells.UnselectShiftSelectedItems();
								}

								if (indexOfLastSelectedCell != -1)
								{
									if (indexOfLastSelectedRow <= indexOfNewSelectedRow)
									{
										int start = indexOfLastSelectedCell;
										cells = cell.Row.VisibleCells;
										int end = cells.IndexOf(cell);

										for (int i = indexOfLastSelectedRow; i <= indexOfNewSelectedRow; i++)
										{
											Row r = this.InternalRows[i] as Row;
											if (r != null)
											{
												cells = r.VisibleCells;
												int count = cells.Count - 1;
												int tempStart = (start > count) ? count : start;
												int tempEnd = (end > count) ? count : end;
												if (start < end)
												{
													for (int j = tempStart; j <= tempEnd; j++)
													{
														Cell c = cells[j] as Cell;
														if (c != null)
															if (c.Row.AllowSelection)
																selectedCells.SelectItem(c, true);
													}
												}
												else
												{
													for (int j = tempStart; j >= tempEnd; j--)
													{
														Cell c = cells[j] as Cell;
														if (c != null)
															if (c.Row.AllowSelection)
																selectedCells.SelectItem(c, true);
													}
												}
											}
										}
									}
									else
									{
										int start = indexOfLastSelectedCell;
										cells = cell.Row.VisibleCells;
										int end = cells.IndexOf(cell);

										for (int i = indexOfLastSelectedRow; i >= indexOfNewSelectedRow; i--)
										{
											Row r = this.InternalRows[i] as Row;
											if (r != null)
											{
												cells = r.VisibleCells;
												int count = cells.Count - 1;
												int tempStart = (start > count) ? count : start;
												int tempEnd = (end > count) ? count : end;
												if (start < end)
												{
													for (int j = tempStart; j <= tempEnd; j++)
													{
														Cell c = cells[j] as Cell;
														if (c != null)
															if (c.Row.AllowSelection)
																selectedCells.SelectItem(c, true);
													}
												}
												else
												{
													for (int j = tempStart; j >= tempEnd; j--)
													{
														Cell c = cells[j] as Cell;
														if (c != null)
															if (c.Row.AllowSelection)
																selectedCells.SelectItem(c, true);
													}
												}
											}
										}
									}
								}
								else
									if (cell.Row.AllowSelection)
										selectedCells.SelectItem(cell, true);
							}
							else
								if (cell.Row.AllowSelection)
									selectedCells.SelectItem(cell, true);
						}
						else
						{
							if (ctrlKey)
							{
								if (action == InvokeAction.Click)
								{
									if (selectedCells.Contains(cell))
									{
										int index = selectedCells.IndexOf(cell);
										selectedCells.InternalRemoveItemSilently(index);
										interrupt = true;
									}
									else
										if (cell.Row.AllowSelection)
											selectedCells.SelectItem(cell, false);

								}
							}
							else
							{
								selectedCells.InternalResetItemsSilently();
								if (cell.Row.AllowSelection)
									selectedCells.SelectItem(cell, false);
							}
						}
					}
					this.OnSelectedCellsCollectionChanged(previouslySelectedCells, this.SelectionSettings.SelectedCells);
					this.InvalidateScrollPanel(false);
				}
			}

			return interrupt;
		}

		#endregion // SelectCell

		#region UnselectCell

		/// <summary>
		/// Unselects the specified cell. 
		/// </summary>
		/// <param propertyName="cell"></param>
		protected internal virtual void UnselectCell(Cell cell)
		{
			if (this.SelectionSettings.SelectedCells.Contains(cell))
				this.SelectionSettings.SelectedCells.Remove(cell);
		}

		#endregion // UnselectCell

		#region SetActiveCell
		/// <summary>
		/// Actually sets the <see cref="XamGrid.ActiveCell"/> of the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks> The ActiveCell must be a <see cref="Cell"/> or <see cref="ChildBandCell"/>
		/// If both alignedTop and bottom are set, aligned top will win. 
		/// </remarks>
		/// <param propertyName="cell">The <see cref="CellBase"/> that should be marked as Active</param>
		/// <param propertyName="alignment">If the active cell should be aligned to the top or bottom. </param>
		/// <param propertyName="action">The action that caused this method to be invoked. </param>
		protected internal virtual void SetActiveCell(CellBase cell, CellAlignment alignment, InvokeAction action)
		{
			this.SetActiveCell(cell, alignment, action, true, true);
		}

		/// <summary>
		/// Actually sets the <see cref="XamGrid.ActiveCell"/> of the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks> The ActiveCell must be a <see cref="Cell"/> or <see cref="ChildBandCell"/>
		/// If both alignedTop and bottom are set, aligned top will win. 
		/// </remarks>
		/// <param propertyName="cell">The <see cref="CellBase"/> that should be marked as Active</param>
		/// <param propertyName="alignment">If the active cell should be aligned to the top or bottom. </param>
		/// <param propertyName="action">The action that caused this method to be invoked. </param>
		/// <param propertyName="allowSelection">If selection should occur when setting the active cell.</param>
		protected internal virtual void SetActiveCell(CellBase cell, CellAlignment alignment, InvokeAction action, bool allowSelection)
		{
			this.SetActiveCell(cell, alignment, action, allowSelection, true);
		}

		/// <summary>
		/// Actually sets the <see cref="XamGrid.ActiveCell"/> of the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks> The ActiveCell must be a <see cref="Cell"/> or <see cref="ChildBandCell"/>
		/// If both alignedTop and bottom are set, aligned top will win. 
		/// </remarks>
		/// <param propertyName="cell">The <see cref="CellBase"/> that should be marked as Active</param>
		/// <param propertyName="alignment">If the active cell should be aligned to the top or bottom. </param>
		/// <param propertyName="action">The action that caused this method to be invoked. </param>
		/// <param propertyName="allowSelection">If selection should occur when setting the active cell.</param>
		/// <param propertyName="scrollIntoView">Determines if the cell should be scrolled into view</param>
		protected internal virtual void SetActiveCell(CellBase cell, CellAlignment alignment, InvokeAction action, bool allowSelection, bool scrollIntoView)
		{
			this.SetActiveCell(cell, alignment, action, allowSelection, true, true);
		}

		/// <summary>
		/// Actually sets the <see cref="XamGrid.ActiveCell"/> of the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks> The ActiveCell must be a <see cref="Cell"/> or <see cref="ChildBandCell"/>
		/// If both alignedTop and bottom are set, aligned top will win. 
		/// </remarks>
		/// <param propertyName="cell">The <see cref="CellBase"/> that should be marked as Active</param>
		/// <param propertyName="alignment">If the active cell should be aligned to the top or bottom. </param>
		/// <param propertyName="action">The action that caused this method to be invoked. </param>
		/// <param propertyName="allowSelection">If selection should occur when setting the active cell.</param>
		/// <param propertyName="setFocus"> Determines if focus should be set to the cell.</param>
		/// <param propertyName="scrollIntoView">Determines if the cell should be scrolled into view</param>
		protected internal virtual void SetActiveCell(CellBase cell, CellAlignment alignment, InvokeAction action, bool allowSelection, bool setFocus, bool scrollIntoView)
		{
			if (cell != null)
			{
				if (cell.Column.Visibility == Visibility.Collapsed || cell.Row.ColumnLayout.Visibility == Visibility.Collapsed)
				{
					throw new InvalidActiveCellException();
				}
			}
			else if (this.ActiveCell == null)
			{
				
				return;
			}

            // Check to see if a particular cell can even be activated.
            if (cell != null && !cell.SupportsActivation)
                return;

			if (this.ActiveCell == cell)
			{
				bool focus = false;
				if (cell.Control != null)
				{
					DependencyObject currentFocusElem = PlatformProxy.GetFocusedElement(cell.Control) as DependencyObject;
					if (currentFocusElem != null)
					{
						while (focus == false && currentFocusElem != null)
						{
							if (currentFocusElem == cell.Control)
								focus = true;
							else
                                currentFocusElem = PlatformProxy.GetParent(currentFocusElem);
						}
					}

					if (!focus)
					{
						Control elem = cell.Control.Content as Control;
						if (elem != null && elem.IsHitTestVisible)
						{
						    focus = elem.Focus();


                            // NZ 8 March 2012 - TFS103887
                            focus = focus || elem.IsKeyboardFocusWithin;

						}
					}

					if (!focus)
						cell.Control.Focus();
				}
				return;
			}

			if (this.CurrentEditRow != null)
			{
				if (cell != null)
				{
					if (cell.Row != this.CurrentEditRow)
					{
						if (!this.ExitEditModeInternal(false))
						{
							this.ScrollCellIntoView(this.CurrentEditCell);
							return;
						}
					}
				}
			}
			else if (this.CurrentEditCell != null)
			{
			    Cell currentEditCell = this.CurrentEditCell;

				// If someone cancels the exiting of edit mode, lets scroll it into view, and avoid setting the active cell.
				if (!this.ExitEditModeInternal(false))
				{
                    this.ScrollCellIntoView(currentEditCell);
					return;
				}

                // We don't want to activate a cell that's about to be filtered out.
                if (currentEditCell.Row is FilterRow && cell != null && currentEditCell.Row.Manager is RowsManager)
                {
                    DataManagerBase dataManager = ((RowsManager)currentEditCell.Row.Manager).DataManager;

                    if (dataManager != null)
                    {
                        IList filteredItems = dataManager.FilterItems(new List<object> { cell.Row.Data });

                        if (filteredItems.Count == 0)
                        {
                            return;
                        }
                    }
                }
			}

			Cell newCell = cell as Cell;

			if (newCell == null && !(cell is ChildBandCell))
				cell = null;

			if (action != InvokeAction.Code && allowSelection)
				this.SelectCell(newCell, action);

			ActiveCellChangingEventArgs args = new ActiveCellChangingEventArgs() { NewActiveCell = cell, PreviousActiveCell = this.ActiveCell };
			this.OnActiveCellChanging(args);

			if (!args.Cancel)
			{
				CellBase prevCell = this._activeCell;
				this._activeCell = cell;

				if (prevCell != null)
				{
					RowBase currentRow = null;
					if (cell != null)
						currentRow = cell.Row;
					prevCell.EnsureCurrentState();
					if (prevCell.Row != currentRow)
					{
						prevCell.Row.IsActive = false;
						CellBase rowSelector = prevCell.Row.Cells[prevCell.Row.Columns.RowSelectorColumn];
						if (rowSelector != null)
							rowSelector.EnsureCurrentState();
					}
				}

				if (cell != null)
				{
					if (prevCell == null || prevCell.Row != cell.Row)
					{
						cell.Row.IsActive = true;
						CellBase rowSelector = cell.Row.Cells[cell.Row.Columns.RowSelectorColumn];
						if (rowSelector != null)
							rowSelector.EnsureCurrentState();
					}

					cell.EnsureCurrentState();
					if (setFocus && cell.Control != null)
					{
						bool focus = false;

						DependencyObject currentFocusElem = PlatformProxy.GetFocusedElement(this) as DependencyObject;
						if (currentFocusElem != null)
						{
							while (focus == false && currentFocusElem != null)
							{
                                if (currentFocusElem == cell.Control)
                                    focus = true;
                                else
                                {
                                    currentFocusElem = PlatformProxy.GetParent(currentFocusElem);
                                }
							}
						}

						if (!focus)
						{
							Control elem = cell.Control.Content as Control;
							
                            if (elem != null && elem.IsHitTestVisible)
							{
							    focus = elem.Focus();


                                // NZ 8 March 2012 - TFS103887
                                focus = focus || elem.IsKeyboardFocusWithin;

							}
						}

						if (!focus)
							cell.Control.Focus();
					}

					if (scrollIntoView)
						this.ScrollCellIntoView(cell, alignment, null);
				}

				this.OnActiveCellChanged();

				if (cell != null)
				{
					EditingSettingsBaseOverride settings = null;

					if (cell.Row.RowType == RowType.AddNewRow)
						settings = (EditingSettingsBaseOverride)cell.Row.ColumnLayout.AddNewRowSettings;
					else if (cell.Row.RowType == RowType.FilterRow)
						settings = (EditingSettingsBaseOverride)cell.Row.ColumnLayout.FilteringSettings;
					else
						settings = (EditingSettingsBaseOverride)cell.Row.ColumnLayout.EditingSettings;

					if (this.CurrentEditRow != null)
					{
						if (cell.Row == this.CurrentEditRow)
						{
							this.ExitCellFromEditModeInternalRow(false);
							if (newCell.IsEditable && this._originalCellValues.ContainsKey(newCell.Column.Key))
								this.CurrentEditCell = newCell;
						}
					}
					else if (settings.IsOnCellActiveEditingEnabledResolved)
					{
						if (cell.Row.AllowEditing == EditingType.Cell)
							this.EnterEditMode(cell);
						else if (cell.Row.AllowEditing == EditingType.Row)
							this.EnterEditMode((Row)cell.Row, cell);
					}

                    RowBase r = cell.Row;

                    this._ignoreActiveItemChanging = true;
                    if (r.Manager == this.RowsManager || (r.Manager.ParentRow != null &&  r.Manager.ParentRow is GroupByRow))
                        this.ActiveItem = r.Data;
                    else
                        this.ActiveItem = null;
                    this._ignoreActiveItemChanging = false;
				}
                else
                {
                    this._ignoreActiveItemChanging = true;
                    this.ActiveItem = null;
                    this._ignoreActiveItemChanging = false;
                }
			}
		}
		#endregion // SetActiveCell

		#region SelectColumn

		/// <summary>
		/// Selects the specified <see cref="Column"/>.
		/// </summary>
		/// <param propertyName="column">The Column that should be selected.</param>
		/// <param propertyName="action">The action that invoked the selection.</param>
		/// <returns>Whether something occurred during the selecting of the Column that should interrupt the selection.</returns>
		protected internal virtual bool SelectColumn(Column column, InvokeAction action)
		{
			bool interrupt = false;
			if (column != null && !(column is FillerColumn) && column.SupportsActivationAndSelection)
			{
				SelectionType selectionType = this.SelectionSettings.ColumnSelection;
				SelectedColumnsCollection previouslySelectedColumns = new SelectedColumnsCollection();
				previouslySelectedColumns.InternalAddRangeSilently(this.SelectionSettings.SelectedColumns);

				bool shiftKey = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
				bool ctrlKey = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

				if (!ctrlKey && !shiftKey)
				{
					this.SelectionSettings.SelectedCells.Clear();
					this.SelectionSettings.SelectedRows.Clear();
				}

				if (selectionType == SelectionType.None && action == InvokeAction.Code)
					selectionType = SelectionType.Single;

				if (selectionType != SelectionType.None)
				{
					SelectedColumnsCollection selectedColumns = this.SelectionSettings.SelectedColumns;
					if (selectionType == SelectionType.Single)
					{
						selectedColumns.InternalResetItemsSilently();
						selectedColumns.SelectItem(column, false);
					}
					else
					{
						if (shiftKey)
						{
							if (selectedColumns.Count > 0)
							{
								Column lastSelectedColumn = selectedColumns.PivotItem;
								if (lastSelectedColumn == null)
									lastSelectedColumn = selectedColumns[selectedColumns.Count - 1];

								if (lastSelectedColumn.ColumnLayout == column.ColumnLayout)
								{
									ColumnBaseCollection cbc = lastSelectedColumn.ColumnLayout.Columns;
									List<Column> visibleColumns = new List<Column>();
									FixedColumnsCollection fixedCols = cbc.FixedColumnsLeft;

                                    foreach (Column fixedCol in fixedCols)
                                    {
                                        if (fixedCol.Visibility == Visibility.Visible)
                                        {
                                            if (fixedCol.SupportsActivationAndSelection)
                                            {
                                                visibleColumns.Add(fixedCol);
                                            }

                                            ReadOnlyKeyedColumnBaseCollection<Column> allCols = fixedCol.AllColumns;
                                            if (allCols.Count > 0)
                                            {
                                                foreach (Column col in allCols)
                                                {
                                                    if (col.SupportsActivationAndSelection && col.Visibility == Visibility.Visible)
                                                    {
                                                        visibleColumns.Add(col);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    foreach (Column col in cbc.VisibleColumns)
                                    {
                                        if (col != this.Columns.FillerColumn)
                                        {
                                            if (col.SupportsActivationAndSelection)
                                            {
                                                visibleColumns.Add(col);
                                            }

                                            ReadOnlyKeyedColumnBaseCollection<Column> allCols = col.AllColumns;
                                            if (allCols.Count > 0)
                                            {
                                                foreach (Column c in allCols)
                                                {
                                                    if (c.SupportsActivationAndSelection && c.Visibility == Visibility.Visible)
                                                    {
                                                        visibleColumns.Add(c);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    FixedColumnsCollection right = cbc.FixedColumnsRight;
                                    for (int i = right.Count - 1; i >= 0; i--)
                                    {
                                        Column col = right[i];

                                        if (col.Visibility == Visibility.Visible)
                                        {
                                            if (col.SupportsActivationAndSelection)
                                            {
                                                visibleColumns.Add(col);
                                            }

                                            ReadOnlyKeyedColumnBaseCollection<Column> allCols = col.AllColumns;
                                            if (allCols.Count > 0)
                                            {
                                                foreach (Column c in allCols)
                                                {
                                                    if (c.SupportsActivationAndSelection && c.Visibility == Visibility.Visible)
                                                    {
                                                        visibleColumns.Add(c);
                                                    }
                                                }
                                            }
                                        }
                                    }

									int indexOfLastSelectedColumn = visibleColumns.IndexOf(lastSelectedColumn);

									if (indexOfLastSelectedColumn != -1)
									{
										int indexOfNewSelectedColumn = visibleColumns.IndexOf(column);

										if (selectedColumns.PivotItem != null)
										{
											int count = selectedColumns.ShiftSelectedItems.Count;
											for (int i = count - 1; i >= 0; i--)
											{
												int index = selectedColumns.IndexOf(selectedColumns.ShiftSelectedItems[i]);
												selectedColumns.InternalRemoveItemSilently(index);
											}
										}

										if (indexOfLastSelectedColumn < indexOfNewSelectedColumn)
										{
											for (int i = indexOfLastSelectedColumn; i <= indexOfNewSelectedColumn; i++)
											{
												Column r = visibleColumns[i];
												if (r != null)
													selectedColumns.SelectItem(r, true);
											}
										}
										else
										{
											for (int i = indexOfLastSelectedColumn; i >= indexOfNewSelectedColumn; i--)
											{
												Column r = visibleColumns[i];
												if (r != null)
													selectedColumns.SelectItem(r, true);
											}
										}
									}
									else
										selectedColumns.SelectItem(column, true);
								}
								else
								{
									selectedColumns.InternalResetItemsSilently();
									selectedColumns.SelectItem(column, false);
								}
							}
							else
								selectedColumns.SelectItem(column, true);
						}
						else
						{
							if (ctrlKey)
							{
								if (action == InvokeAction.Click)
								{
									if (selectedColumns.Contains(column))
									{
										int index = selectedColumns.IndexOf(column);
										selectedColumns.InternalRemoveItemSilently(index);
										interrupt = true;
									}
									else
										selectedColumns.SelectItem(column, false);
								}
							}
							else
							{
								selectedColumns.InternalResetItemsSilently();
								selectedColumns.SelectItem(column, false);
							}
						}
					}
					this.OnSelectedColumnsCollectionChanged(previouslySelectedColumns, this.SelectionSettings.SelectedColumns);
					this.InvalidateScrollPanel(false);
				}
			}
			return interrupt;
		}

		#endregion // SelectColumn

		#region UnselectColumn

		/// <summary>
		/// Unselects the specified Column. 
		/// </summary>
		/// <param propertyName="column"></param>
		protected internal virtual void UnselectColumn(Column column)
		{
			if (this.SelectionSettings.SelectedColumns.Contains(column))
				this.SelectionSettings.SelectedColumns.Remove(column);
		}

		#endregion // UnselectColumn

		#region EnterEditModeInternal

		/// <summary>
		/// Places the specified <see cref="Row"/> or <see cref="Cell"/> into edit mode.
		/// </summary>
		/// <param propertyName="row">If not specifiec the cell will be put into edit mode.</param>
		/// <param propertyName="cell">If both this and the row are specified, the row will be put into edit mode, starting with this cell.</param>
		/// <returns>True if the <see cref="Cell"/> was able to enter edit mode.</returns>
		protected virtual bool EnterEditModeInternal(Row row, Cell cell)
		{
			if (row == null)
			{
				if (cell != null && cell.IsEditable && !cell.IsEditing)
				{
                    row = cell.Row as Row;
                    if (row != null && row.RowType == RowType.DataRow)
                    {
                        RowsManager manager = row.Manager as RowsManager;
                        if (manager != null)
                        {
                            if (!manager.DataManager.SupportsEditing)
                                return false;
                        }

                    }
					if (this.ExitEditModeInternal(false))
					{
						if (!this.OnCellEnteringEditMode(cell))
						{
                            bool handled = false;

                            if (cell.Row.RowType == RowType.DataRow)
                            {
                                RowsManager manager = cell.Row.Manager as RowsManager;
                                if (manager != null)
                                    handled = manager.DataManager.EditItem(cell.Row.Data);
                            }

                            if (!handled)
                            {
                                IEditableObject obj = cell.Row.Data as IEditableObject;
                                if (obj != null)
                                    obj.BeginEdit();
                            }

                            // Set the activeCell before clearing the cell values
                            // B/c setting the activeCell could potentially trigger another enter edit mode call.
                            this.ActiveCell = cell;

							this.EditCellValues.Clear();
							this._originalCellValues.Clear();

							if (cell.Control != null && !cell.Control.IsEnabled)
								return false;

							object val = cell.Value;
							this.EditCellValues.Add(cell.Column.Key, val);
							this._originalCellValues.Add(cell.Column.Key, val);

                            if (cell.Row.Control == null && this.Panel != null)
                            {
                                this.Panel.RenderRow(cell.Row);
                            }

                            if (cell.Row.Control == null)
                                return false;

							// Make sure there is a control. 
							if (cell.Control == null)
							{
								cell.Row.Control.RenderCell(cell);
							}

							cell.EnterEditMode(true);

							this.OnCellEnteredEditMode(cell, cell.Control.Content as FrameworkElement);


                            this.Dispatcher.BeginInvoke(new Action(this.AttachCellMouseDownToRootVis));



							this.InvalidateScrollPanel(false);

							this.SuspendConditionalFormatUpdates = true;

							return true;
						}
					}
				}
			}
			else
			{
                if (row != null && row.RowType == RowType.DataRow)
                {
                    RowsManager manager = row.Manager as RowsManager;
                    if (manager != null)
                    {
                        if (!manager.DataManager.SupportsEditing)
                            return false;
                    }
                }

				if (this.CurrentEditRow == row)
					return true;

				if (cell != null && !cell.IsEditable)
					return false;

				// No cell was specified. lets find the first editable cell
				if (cell == null && row.Cells.Count > 0)
				{
					foreach (CellBase cb in row.VisibleCells)
					{
						if (cb.Column.Visibility != Visibility.Collapsed)
						{
							Cell c = cb as Cell;
							if (c.IsEditable)
							{
								cell = c;
								break;
							}
						}
					}
				}

				// If the row has an editable cell
				if (cell != null)
				{
					if (this.ExitEditModeInternal(false))
					{   
						if (!this.OnRowEnteringEditMode(row))
						{
                            bool handled = false;
                            if (row.RowType == RowType.DataRow)
                            {
                                RowsManager manager = row.Manager as RowsManager;
                                if (manager != null)
                                    handled = manager.DataManager.EditItem(row.Data);
                            }

                            if (!handled)
                            {
                                IEditableObject obj = row.Data as IEditableObject;
                                if (obj != null)
                                    obj.BeginEdit();
                            }

							this.EditCellValues.Clear();
							this._originalCellValues.Clear();

							this.ScrollCellIntoView(cell);

                            this.CellsThatCancelledEditMode.Clear();

							// Put every cell into a pseudo edit mode.
							foreach (CellBase cb in row.VisibleCells)
							{
								if (cb.Column.Visibility != Visibility.Collapsed)
								{
									Cell c = cb as Cell;
									if (c != null && c != cell)
									{
                                        if (this.EnterCellToEditModeInternalRow(c, false) == false)
                                        {
                                            
                                            if (c.IsEditable)
                                            {
                                                
                                                this.CellsThatCancelledEditMode.Add(c);
                                            }
                                        }
									}
								}
							}

							// Put the specified into edit mode last, this cell will actually be in full edit mode.
							this.EnterCellToEditModeInternalRow(cell, true);

							if (this.EditCellValues.Count == 0)
								return false;

                            this.CurrentEditRow = row;


                            this.Dispatcher.BeginInvoke(new Action(this.AttachRowMouseDownToRootVis));




							this.OnRowEnteredEditMode(row);

							this.InvalidateScrollPanel(false);

							this.SuspendConditionalFormatUpdates = true;

							return true;
						}
					}
				}
			}

			return false;
		}
		#endregion // EnterEditModeInternal

		#region EnterCellToEditModeInternalRow

		/// <summary>
		/// For rows in edit mode, this displays the editor in the specified cell.
		/// </summary>
		/// <param propertyName="cell"></param>
		/// <param propertyName="currentEditCell">Whether this cell is actually in edit mode.</param>
		/// <returns>True if a cell was put into edit mode.</returns>
		protected internal bool EnterCellToEditModeInternalRow(Cell cell, bool currentEditCell)
		{
			if (cell.IsEditable && !this.OnCellEnteringEditMode(cell))
			{
				if (cell.Row.Control == null)
				{
					cell.Row.ColumnLayout.Grid.Panel.RenderRow(cell.Row);
				}

				if (cell.Control == null)
				{
					cell.Row.Control.RenderCell(cell);
				}

				if (!cell.Control.IsEnabled)
					return false;

				object val = cell.Value;
				this._originalCellValues.Add(cell.Column.Key, val);
				this.EditCellValues.Add(cell.Column.Key, val);

				cell.EnterEditMode(currentEditCell);

				this.OnCellEnteredEditMode(cell, cell.Control.Content as FrameworkElement);

				return true;
			}
			return false;
		}
		#endregion // EnterCellToEditModeInternalRow

		#region ExitCellFromEditModeInternalRow
		/// <summary>
		/// When rows are in edit mode, this method removes the current cell from edit mode, but leaves the editor inside of it.
		/// </summary>
		/// <returns>True if the operation was successful</returns>
		protected internal bool ExitCellFromEditModeInternalRow(bool cancel)
		{
		    return this.ExitCellFromEditModeInternalRow(cancel, false);
		}

        internal bool ExitCellFromEditModeInternalRow(bool cancel, bool forceExit)
		{
			if (this.CurrentEditRow != null)
			{
				if (this.CurrentEditCell == null)
					return true;

				Cell cell = this.CurrentEditCell;

				FrameworkElement editor = cell.Control.Content as FrameworkElement;
				this.EditCellValues[cell.Column.Key] = cell.Control.ContentProvider.ResolveValueFromEditor(cell);

				object newValue = this.EditCellValues[cell.Column.Key];

				ExitEditingCellEventArgs eventArgs = new ExitEditingCellEventArgs() { Cell = this.CurrentEditCell, NewValue = newValue, EditingCanceled = cancel, Editor = editor };

			    if (!forceExit)
				    this.OnCellExitingEditMode(eventArgs);

				if (!eventArgs.Cancel)
				{
                    if (cancel)
                    {
                        if (this._originalCellValues.ContainsKey(cell.Column.Key))
                        {
                            if (!cell.ResetDataValue(this._originalCellValues[cell.Column.Key]))
                            {
                                this.CurrentEditCell = cell;
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (!((CellControl)cell.Control).EvaluateEditingBindings())
                            return false;
                    }

					this.EditCellValues[cell.Column.Key] = eventArgs.NewValue;

					this.CurrentEditCell = null;

					this.OnExitedEditMode(cell);

					return true;
				}
			}

			return false;
		}
		#endregion // ExitCellFromEditModeInternalRow

		#region ExitEditModeInternal

		/// <summary>
		/// Processes a request to stop editing of a cell.
		/// </summary>
		/// <param propertyName="cancel">True if the input value should be discarded and the original value restored.</param>
		/// <returns>False if the CellExitingEditMode event is cancelled, stopping the exiting of edit mode.</returns>
		protected internal bool ExitEditModeInternal(bool cancel)
		{
		    return this.ExitEditModeInternal(cancel, false);
		}
	
        internal bool ExitEditModeInternal(bool cancel, bool forceExit)
		{
			if (this.CurrentEditRow != null)
			{
                bool invalidateData = false;

                // If we're dealing with mergedCells, and there is a merged cell for this particualr row
                // Then invalidate the invalidate the data, when this operation is completed.
                if (this.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                {
                    


                    if (this.CurrentEditRow.RowType == RowType.DataRow &&
                        this.GroupBySettings.GroupByColumns[this.CurrentEditRow.ColumnLayout].Count > 0)
                    {
                        invalidateData = true;
                    }
                }

				Cell original = this.CurrentEditCell;
				if (this.ExitCellFromEditModeInternalRow(cancel, forceExit))
				{
                    Dictionary<string, object> editCellValues = new Dictionary<string, object>(this.EditCellValues);

                    // NZ 21 April 2012 - TFS104478 - Evaluate EditCellValues for RowExitingEditMode.
				    foreach (var editCellValue in editCellValues)
				    {
                        Column column = this.CurrentEditRow.Columns.AllColumns[editCellValue.Key] as Column;

                        if (column == null)
                        {
                            continue;
                        }

                        Cell cell = this.CurrentEditRow.Cells[column] as Cell;

                        if (cell != null && cell != original && cell.Control.ContentProvider.CanResolveValueFromEditor)
				        {
				            this.EditCellValues[cell.Column.Key] = cell.Control.ContentProvider.ResolveValueFromEditor(cell);
				        }
				    }

					if (forceExit || !this.OnRowExitingEditMode(this.CurrentEditRow, this._originalCellValues, this.EditCellValues, cancel))
					{
						Row row = this.CurrentEditRow;

                        // First loop through, and make sure everything validates
                        // We need to do this, even if we were canceled, b/c they could have initially had invalid data.
                        foreach (CellBase cb in row.VisibleCells)
                        {
                            if (cb.Column.Visibility != Visibility.Collapsed)
                            {
                                Cell c = cb as Cell;
                                if (c != null && c.IsEditable && c.Control != null && c != original)
                                {
                                    if (cancel)
                                    {
                                        if (this._originalCellValues.ContainsKey(c.Column.Key))
                                        {
                                            if (!c.ResetDataValue(this._originalCellValues[c.Column.Key], !forceExit))
                                            {
                                                this.CurrentEditCell = c;
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!((CellControl)c.Control).EvaluateEditingBindings())
                                        {
                                            this.CurrentEditCell = c;
                                            return false;
                                        }
                                    }
                                }
                            }
                        }

						if (cancel)
						{
							this.NeedConditionalFormatUpdate = false;
						}

						this.CurrentEditRow = null;
						this.CurrentEditCell = null;

                        ReadOnlyKeyedColumnBaseCollection<ColumnBase> columns = row.ColumnLayout.Columns.AllColumns;

                        foreach (ColumnBase cb in columns)
                        {
                            Column col = cb as Column;
                            if (col != null)
                            {
                                Cell c = row.Cells[col] as Cell;

                                // Combined if stmts. 
                                // If there is no original value then we must never have entered edit mode.
                                // Also, we could be hidden, but we weren't hidden when we went into edit mode, so use the originaCellValues to determine that.
                                if (c != null && c.IsEditable && this._originalCellValues.ContainsKey(c.Column.Key))
                                {
                                    object value = null;
                                    if (cancel)
                                        value = this._originalCellValues[c.Column.Key];
                                    else
                                    {
                                        object originalVal = this._originalCellValues[c.Column.Key];
                                        object currentVal = c.ResolveValueFromCell();

                                        if (originalVal != currentVal && c.Control.ContentProvider.CanResolveValueFromEditor)
                                            value = currentVal;
                                        else
                                            value = this.EditCellValues[c.Column.Key];
                                    }

                                    c.ExitEditMode(value, cancel, false);
                                }
                            }

                        }

                        this.DetachRowMouseDownToRootVis();

						if (row.RowType == RowType.AddNewRow && !cancel)
						{
							if (((AddNewRow)row).IsRowDirty)
							{
								RowsManager rm = (RowsManager)row.Manager;

								rm.AddItem(rm.CreateItem(row.Data));

								rm.ResetAddNewRows(true);
							}
						}
                        						
                        if (row.RowType == RowType.DataRow)
                        {
                            bool handled = false;

                            RowsManager manager = row.Manager as RowsManager;
                            if (manager != null)
                            {
                                if (cancel)
                                    handled = manager.DataManager.CancelEdit();
                                else
                                    handled = manager.DataManager.CommitEdit();
                            }

                            if (!handled)
                            {
                                IEditableObject obj = row.Data as IEditableObject;
                                if (obj != null)
                                {
                                    if (cancel)
                                        obj.CancelEdit();
                                    else
                                        obj.EndEdit();
                                }
                            }
                        }

						this.OnRowExitedEditMode(row);
						this.EditCellValues.Clear();
						this._originalCellValues.Clear();

						this.SuspendConditionalFormatUpdates = false;

                        if (invalidateData)
                            this.InvalidateData();

						return true;
					}
					else
					{
						// If the event is canceled, lets set the previous cell as the edit cell
						// in case it was the cell that the event was cancelled for.
						this.CurrentEditCell = original;
						this.NeedConditionalFormatUpdate = false;
					}
				}

				//this.InvalidateConditionalFormatting();

				return false;
			}
			else if (this.CurrentEditCell != null && this.CurrentEditCell.Control != null)
			{
                bool invalidateData = false;

                // If we're dealing with mergedCells, and the CurrentEditCell belongs to a MergedColumn
                // Then invalidate the invalidate the data, when this operation is completed.
                if (this.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                {
                    


                    if (this.CurrentEditCell.Row != null &&
                        this.CurrentEditCell.Row.RowType == RowType.DataRow &&
                        this.CurrentEditCell.Column.IsGroupBy)
                    {
                        invalidateData = true;
                    }
                }

				Cell cell = this.CurrentEditCell;

				FrameworkElement editor = cell.Control.Content as FrameworkElement;

				this.EditCellValues[cell.Column.Key] = cell.Control.ContentProvider.ResolveValueFromEditor(cell);

				object newValue = this.EditCellValues[cell.Column.Key];

				if (cancel)
				{
                    if (this._originalCellValues.ContainsKey(cell.Column.Key))
                    {
                        if (!cell.ResetDataValue(this._originalCellValues[cell.Column.Key], !forceExit))
                        {
                            this.CurrentEditCell = cell;
                            return false;
                        }
                    }
					newValue = this._originalCellValues[cell.Column.Key];
					this.NeedConditionalFormatUpdate = false;
				}
				ExitEditingCellEventArgs eventArgs = new ExitEditingCellEventArgs() { Cell = this.CurrentEditCell, NewValue = newValue, EditingCanceled = cancel, Editor = editor };
				
                if(!forceExit)
                    this.OnCellExitingEditMode(eventArgs);

				if (!eventArgs.Cancel)
				{
					this.CurrentEditCell = null;

					if (!cell.ExitEditMode(eventArgs.NewValue, cancel, true))
					{
                        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                        FrameworkElement rootVis2 = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                        if (rootVis2 != null)
                        {
                            rootVis2.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_CellEditing_MouseLeftButtonDown));
                            rootVis2.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_CellEditing_MouseLeftButtonDown), true);
                        }

						this.CurrentEditCell = cell;
						this.ScrollCellIntoView(this.CurrentEditCell);                        
						return false;
					}

                    bool handled = false; 

                    if (cell.Row.RowType == RowType.DataRow)
                    {
                        RowsManager manager = cell.Row.Manager as RowsManager;
                        if (manager != null)
                        {
                            if (cancel)
                                handled = manager.DataManager.CancelEdit();
                            else
                                handled = manager.DataManager.CommitEdit();
                        }
                    }

                    if (!handled)
                    {
                        IEditableObject obj = cell.Row.Data as IEditableObject;
                        if (obj != null)
                        {
                            if (cancel)
                                obj.CancelEdit();
                            else
                                obj.EndEdit();
                        }
                    }

					this.OnExitedEditMode(cell);

                    this.DetachCellMouseDownToRootVis();

					if (cell.Row.RowType == RowType.FilterRow && (cancel))
					{
						FilterRow fr = (FilterRow)cell.Row;
						FilterRowCell frc = cell as FilterRowCell;

						if (frc.FilteringOperandResolved.RequiresFilteringInput)
						{
							if (frc.FilterCellValueResolved == null)
							{
								fr.RemoveFilters(frc);
							}
							else if (frc.FilterCellValueResolved is string && string.IsNullOrEmpty((string)frc.FilterCellValueResolved))
							{
								fr.RemoveFilters(frc);
							}
							else
							{
								fr.BuildFilters(frc, frc.FilterCellValueResolved, !cancel);
                                if (!cancel)
                                    fr.RaiseFilteredEvent();   
							}
						}
						else
						{
							if (!cancel)
							{
								fr.BuildFilters(frc, frc.FilterCellValueResolved, true);
                                fr.RaiseFilteredEvent();   
							}
						}
					}
				}
				else
				{
					this.NeedConditionalFormatUpdate = false;
                    
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                    FrameworkElement rootVis2 = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                    if (rootVis2 != null)
                    {
                        rootVis2.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_CellEditing_MouseLeftButtonDown));
                        rootVis2.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_CellEditing_MouseLeftButtonDown), true);
                    }

					this.ScrollCellIntoView(this.CurrentEditCell);
					return false;
				}

                if (invalidateData)
                    this.InvalidateData();
			}

            this.InvalidateScrollPanel(false);

			this.SuspendConditionalFormatUpdates = false;            

			return true;
		}
		#endregion // ExitEditModeInternal

        #region IsControlInsideGrid

        /// <summary>
        /// Returns true if a control is inside of the grid
        /// </summary>
        /// <param name="control">the control</param>
        /// <returns>true if a control is inside of the grid; otherwise, false.</returns>
        protected internal bool IsDependancyObjectInsideGrid(DependencyObject control)
        {
            DependencyObject dp = control;

            while (dp != null)
            {
                if (dp == this)
                    return true;

                dp = PlatformProxy.GetParent(dp);
            }
            return false;
        }

        #endregion // IsControlInsideGrid

        #region RegisterRenderAdorners

        /// <summary>
        /// Allows derived classes to insert their own custom XamGridRenderAdorner that plug into the RowsPanel.
        /// </summary>
        /// <param name="adorners"></param>
        protected virtual void RegisterRenderAdorners(List<XamGridRenderAdorner> adorners)
        {
            adorners.Add(new MergedCellsRenderAdorner(this));
        }

        #endregion // RegisterRenderAdorners

        #endregion // Protected

        #region Public

        #region ScrollCellIntoView

        /// <summary>
		/// Scrolls the specified cell into view. 
		/// </summary>
		/// <param propertyName="cell"></param>
		public void ScrollCellIntoView(CellBase cell)
		{
			this.ScrollCellIntoView(cell, CellAlignment.NotSet, null);
		}

		private void ScrollCellIntoView(CellBase cell, CellAlignment alignment, EmptyDelegate callback)
		{
			if (this.Panel != null && cell != null)
				this.Panel.ScrollCellIntoView(cell, alignment, callback);
		}

		#endregion // ScrollCellIntoView

		#region EnterEditMode

		/// <summary>
		/// Places the <see cref="Cell"/> that is currently active, into edit mode.
		/// </summary>
		/// <returns>True if the <see cref="Cell"/> was able to enter edit mode. </returns>
		public virtual bool EnterEditMode()
		{
			return this.EnterEditMode(this.ActiveCell);
		}

		/// <summary>
		/// Places the specified <see cref="Cell"/> into edit mode.
		/// </summary>
		/// <param propertyName="cell"></param>
		/// <returns>True if the <see cref="Cell"/> was able to enter edit mode. </returns>
		public virtual bool EnterEditMode(CellBase cell)
		{
            if (_currentlyEnteringEditMode)
                return true;

            _currentlyEnteringEditMode = true;

            var result = this.EnterEditModeInternal(null, cell as Cell);

            _currentlyEnteringEditMode = false;

            return result;
		}

		/// <summary>
		/// Puts the specified <see cref="Row"/> into edit mode.
		/// </summary>
		/// <param propertyName="row"></param>
		///<remarks>If there are no editable cells, the row will not enter edit mode.</remarks>
		/// <returns>True if the <see cref="Row"/> was able to enter edit mode. </returns>
		public virtual bool EnterEditMode(Row row)
		{
			return this.EnterEditModeInternal(row, null);
		}

		/// <summary>
		/// Puts the specified <see cref="Cell"/> and <see cref="Row"/> into edit mode.
		/// </summary>
		/// <param propertyName="row"></param>
		/// <param propertyName="cell"></param>
		/// <remarks>The <see cref="Cell"/> must be in the <see cref="Row"/>'s cells collection.</remarks>
		/// <returns>True if the <see cref="Row"/> and <see cref="Cell"/> were able to enter edit mode. </returns>
		public virtual bool EnterEditMode(Row row, CellBase cell)
		{
			if (row.VisibleCells.Contains(cell))
				return this.EnterEditModeInternal(row, cell as Cell);

			return false;
		}

		#endregion // EnterEditMode

		#region ExitEditMode

		/// <summary>
		/// If a <see cref="Cell"/> is currently in edit mode, it removes it from edit mode.
		/// </summary>
		/// <param propertyName="cancel">Whether or not the value that has been entered should be ignored.</param>
		/// <returns>False if  the editor could not exit edit mode, due to the cancelation in the <see cref="XamGrid.CellExitingEditMode"/> event. </returns>
		public virtual bool ExitEditMode(bool cancel)
		{
			return this.ExitEditModeInternal(cancel);
		}
		#endregion // ExitEditMode

		#region InvalidateData

		/// <summary>
		/// Triggers all Data operations such as sorting and GroupBy to be invalidated. 
		/// </summary>
		public void InvalidateData()
		{
			this.RowsManager.ColumnLayout.InvalidateData();
			this.ResetPanelRows();
		}

		#endregion // InvalidateData

        #region ShowColumnChooser

        /// <summary>
        /// Shows the <see cref="ColumnChooserDialog"/> for the root level of the <see cref="XamGrid"/>
        /// </summary>
        public void ShowColumnChooser()
        {
            this.ShowColumnChooser(this.RowsManager.ColumnLayout);
        }

        /// <summary>
        /// Shows the <see cref="ColumnChooserDialog"/> for the root level of the <see cref="XamGrid"/>
        /// </summary>
        /// <param name="initialLocation">The initial location of the ColumnChoooserDialog. The point is relateive to the <see cref="XamGrid"/></param>
        public void ShowColumnChooser(Point initialLocation)
        {
            this.ShowColumnChooser(this.RowsManager.ColumnLayout, initialLocation);
        }

        /// <summary>
        /// Shows the <see cref="ColumnChooserDialog"/> for the specified <see cref="ColumnLayout"/>.
        /// </summary>
        /// <param name="layout">The ColumnLayout that should be used to populate the ColumnChooserDialog</param>
        public void ShowColumnChooser(ColumnLayout layout)
        {
            this.ShowColumnChooserInternal(layout, this.ColumnChooserSettings.InitialLocation);
        }

        /// <summary>
        /// Shows the <see cref="ColumnChooserDialog"/> for the specified <see cref="ColumnLayout"/>.
        /// </summary>
        /// <param name="layout">The ColumnLayout that should be used to populate the ColumnChooserDialog</param>
        /// <param name="initialLocation">The initial location of the ColumnChoooserDialog. The point is relateive to the <see cref="XamGrid"/></param>
        public void ShowColumnChooser(ColumnLayout layout, Point initialLocation)
        {
            this.ShowColumnChooserInternal(layout, initialLocation);
        }

        /// <summary>
        /// Shows the <see cref="ColumnChooserDialog"/> for the specified <see cref="Column"/>.
        /// </summary>
        /// <param name="column">The column must have children columns such as a <see cref="GroupColumn"/> in order to display the dialog.</param>
        public void ShowColumnChooser(Column column)
        {
            this.ShowColumnChooserInternal(column, this.ColumnChooserSettings.InitialLocation);
        }

        /// <summary>
        /// Shows the <see cref="ColumnChooserDialog"/> for the specified <see cref="Column"/>.
        /// </summary>
        /// <param name="column">The column must have children columns such as a <see cref="GroupColumn"/> in order to display the dialog.</param>
        /// <param name="initialLocation">The initial location of the ColumnChoooserDialog. The point is relateive to the <see cref="XamGrid"/></param>
        public void ShowColumnChooser(Column column, Point initialLocation)
        {
            this.ShowColumnChooserInternal(column, initialLocation);
        }

        private void ShowColumnChooserInternal(ColumnBase col, Point? initialLocation)
        {
            if (col != null && this._isLoaded)
            {
                if (this.Panel != null && !this.Panel.Children.Contains(this.ColumnChooserDialog))
                {
                    this.Panel.Children.Add(this.ColumnChooserDialog);
                    // Neccessary in order for OnApplyTemplate to be called on the ColumnChooserDialog
                    this.ColumnChooserDialog.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                }

                ColumnLayout layout = col as ColumnLayout;
                if (layout != null)
                {
                    this.ColumnChooserDialog.ColumnLayout = layout;
                    this.ColumnChooserDialog.Column = null;
                }
                else
                {
                    Column column = col as Column;
                    if (column != null && column.ResolveChildColumns() != null)
                    {
                        this.ColumnChooserDialog.ColumnLayout = column.ColumnLayout;
                        this.ColumnChooserDialog.Column = column;
                    }
                    else
                    {
                        return;
                    }
                }

				this.ColumnChooserDialog.InitialLocation = initialLocation;

				// Make sure its closed first, so that it isn't stuck.
				this.ColumnChooserDialog.IsOpen = false;

				this.ColumnChooserDialog.IsOpen = true;
			}
		}

		#endregion // ShowColumnChooser

		#region HideColumnChooser

        /// <summary>
        /// Hides the <see cref="ColumnChooserDialog"/> if it's currently being displayed.
        /// </summary>
        public void HideColumnChooser()
        {
            if (this._panel != null)
            {
                this.Panel.Children.Remove(this.ColumnChooserDialog);
            }

			this.ColumnChooserDialog.IsOpen = false;
			this.ColumnChooserDialog.ColumnLayout = null;
			this.ColumnChooserDialog.Column = null;
		}

        #endregion // HideColumnChooser

        #endregion // Public

        #region Private

        #region PageUpCallback
        
        /// <summary>
		/// Since we actually need to perform a measure halfway through the calculation of the
		/// cell that should be set as the ActiveCell in a PageUp operation, we have a  callback
		/// method that allows us to finish the calculation when the layout operation is complete.
		/// </summary>
		private void PageUpCallback()
		{
            int index = this.InternalRows.IndexOf(this.Panel.VisibleRows[0]);
            int indexOfLast = this.InternalRows.IndexOf(this.Panel.VisibleRows[this.Panel.VisibleRows.Count - 1]);

            this.PageUpActivateCellInRowRange(index, indexOfLast);
		}

		#endregion // PageUpCallback

        #region PageUpActivateCellInRowRange

	    /// <summary>
	    /// Activates a cell in given row range.
	    /// </summary>
	    /// <param name="indexOfFirst">The index of first row in the range.</param>
	    /// <param name="indexOfLast">The index of last row in the range.</param>
	    /// <returns>true if a cell was activated; otherwise, false.</returns>
	    private bool PageUpActivateCellInRowRange(int indexOfFirst, int indexOfLast)
	    {
	        Row row = null;
	        bool isInTheSameLayout = false;
	        bool align = false;

	        int index = indexOfFirst;

            CellBase activeCell = this.ActiveCell;
            RowBase activeCellRow = null;

            if (activeCell != null)
            {
                activeCellRow = activeCell.Row;
            }

	        // Traverse the visible rows and look for a the topmost row
	        // that belongs to the ColumnLayout of the ActiveCell.
	        // If there is no ActiveCell, just take the topmost row.
	        while (row == null && index <= indexOfLast)
	        {
	            var tempRow = this.InternalRows[index] as Row;

	            // If we have an ActiveCell
                if (activeCell != null)
	            {
	                if (tempRow != null &&
                        activeCellRow != null &&
	                    tempRow.ColumnLayout != null &&
                        tempRow.ColumnLayout == activeCellRow.ColumnLayout &&
                        tempRow.RowType == activeCellRow.RowType)
	                {
	                    row = tempRow;
	                    isInTheSameLayout = true;
	                    break;
	                }
	            }
	            else
	            {
	                // We don't have an ActiveCell. Let's just take a row and then we'll activate it's firts cell.
	                row = tempRow;
	            }

	            index++;
	        }

            if (activeCellRow != null)
            {
                RowBase firstRow = this.InternalRows[indexOfFirst];

                if (firstRow != null && activeCellRow.ColumnLayout == firstRow.ColumnLayout)
                {
                    align = true;
                }
            }

	        if (row != null)
	        {
	            Collection<CellBase> cells = row.VisibleCells;
	            CellBase cellToActivate;

	            if (isInTheSameLayout)
	            {
                    cellToActivate = row.Cells[activeCell.Column];

                    if (cellToActivate != activeCell)
	                {
                        this.SetActiveCell(cellToActivate, align ? CellAlignment.Top : CellAlignment.NotSet, InvokeAction.Keyboard);
	                    return true;
	                }
	            }
	            else if (cells.Count > 0)
	            {
	                cellToActivate = cells[0];

                    if (cellToActivate != activeCell)
	                {
	                    this.SetActiveCell(cellToActivate, CellAlignment.Top, InvokeAction.Keyboard);
	                    return true;
	                }
	            }
	        }

	        return false;
	    }

        #endregion // PageUpActivateCellInRowRange

        #region PageDownCallback
        
        /// <summary>
        /// Since we actually need to perform a measure halfway through the calculation of the
        /// cell that should be set as the ActiveCell in a PageDown operation, we have a  callback
        /// method that allows us to finish the calculation when the layout operation is complete.
        /// </summary>
        private void PageDownCallback()
        {
            int index = this.InternalRows.IndexOf(this.Panel.VisibleRows[this.Panel.VisibleRows.Count - 1]);
            int indexOfFirst = this.InternalRows.IndexOf(this.Panel.VisibleRows[0]);

            this.PageDownActivateCellInRowRange(indexOfFirst, index);
        }

        #endregion // PageDownCallback

        #region PageDownActivateCellInRowRange

	    /// <summary>
	    /// Activates a cell in given row range.
	    /// </summary>
	    /// <param name="indexOfFirst">The index of first row in the range.</param>
	    /// <param name="indexOfLast">The index of last row in the range.</param>
	    /// <returns>true if a cell was activated; otherwise, false.</returns>
	    private bool PageDownActivateCellInRowRange(int indexOfFirst, int indexOfLast)
	    {
	        Row row = null;
	        bool isInTheSameLayout = false;
            bool align = false;

	        int index = indexOfLast;

            CellBase activeCell = this.ActiveCell;
            RowBase activeCellRow = null;

            if (activeCell != null)
            {
                activeCellRow = activeCell.Row;
            }

	        // Traverse the visible rows and look for a the bottommost row
	        // that belongs to the ColumnLayout of the ActiveCell
	        while (row == null && index >= indexOfFirst)
	        {
	            var tempRow = this.InternalRows[index] as Row;

	            // If we have an ActiveCell
                if (activeCell != null)
	            {
	                if (tempRow != null &&
                        activeCellRow != null &&
	                    tempRow.ColumnLayout != null &&
                        tempRow.ColumnLayout == activeCellRow.ColumnLayout &&
                        tempRow.RowType == activeCellRow.RowType)
	                {
	                    row = tempRow;
	                    isInTheSameLayout = true;
	                    break;
	                }
	            }
	            else
	            {
	                // We don't have an ActiveCell. Let's just take a row and then we'll activate it's firts cell.
	                row = tempRow;
	            }

	            index--;
	        }

            if (activeCellRow != null)
            {
                RowBase lastRow = this.InternalRows[indexOfLast];

                if (lastRow != null && activeCellRow.ColumnLayout == lastRow.ColumnLayout)
                {
                    align = true;
                }
            }

	        if (row != null)
	        {
	            Collection<CellBase> cells = row.VisibleCells;
	            CellBase cellToActivate;

	            if (isInTheSameLayout)
	            {
                    cellToActivate = row.Cells[activeCell.Column];

                    if (cellToActivate != activeCell)
	                {
                        this.SetActiveCell(cellToActivate, align ? CellAlignment.Bottom : CellAlignment.NotSet, InvokeAction.Keyboard);
	                    return true;
	                }
                    
	            }
	            else if (cells.Count > 0)
	            {
	                cellToActivate = cells[0];

                    if (cellToActivate != activeCell)
	                {
	                    this.SetActiveCell(cells[0], CellAlignment.Bottom, InvokeAction.Keyboard);
	                    return true;
	                }
	            }
	        }

	        return false;
	    }

	    #endregion // Name

		#region GetCellFromSource

		/// <summary>
		///  Walks up the parent chain of the specified until an <see cref="CellControlBase"/> is found.
		/// </summary>
		/// <param propertyName="obj"></param>
		/// <returns></returns>
		private CellControlBase GetCellFromSource(DependencyObject obj)
		{
			CellControlBase ccb = obj as CellControlBase;

			while (obj != null && ccb == null)
			{
                obj = PlatformProxy.GetParent(obj);

				ccb = obj as CellControlBase;
				if (ccb != null && ccb.Cell != null && ccb.Cell.Row.ColumnLayout != null && ccb.Cell.Row.ColumnLayout.Grid == this)
				{
					break;
				}
				else
				{
					ccb = null;
				}
			}

			return ccb;
		}
		#endregion // GetCellFromSource

		#region ApplyItemSource

		private void ApplyItemSource(IEnumerable itemSource)
		{
            if (this._rowsManager.ItemsSource != itemSource)
            {
                this._rowsManager.ItemsSource = itemSource;

                if (this.HorizontalScrollBar != null)
                {
                    this.HorizontalScrollBar.Value = 0;
                    this.HorizontalScrollBar.Visibility = Visibility.Collapsed;
                }

                if (this.VerticalScrollBar != null)
                {
                    this.VerticalScrollBar.Value = 0;
                    this.VerticalScrollBar.Visibility = Visibility.Collapsed;
                }

                ICollectionView icv = itemSource as ICollectionView;
                if (icv != null)
                    this.ActiveItem = icv.CurrentItem;
            }

			this.InvalidateScrollPanel(true);
		}
		#endregion // ApplyItemSource

		#region OnLoadedCatchUp

		private void OnLoadedColumnsCatchUp(ColumnLayout rootLayout, ReadOnlyKeyedColumnBaseCollection<ColumnBase> columns)
		{
			foreach (ColumnBase cb in columns)
			{
				Column col = cb as Column;
				if (col != null)
				{
					col.FilterColumnSettings.OnLoadedCatchUp();
					col.SummaryColumnSettings.OnLoadedCatchUp();
				}
				else
				{
					ColumnLayout columnLayout = cb as ColumnLayout;
					if (columnLayout != null)
					{
                        if (rootLayout != columnLayout)
						{
							columnLayout.Grid = this;
							this.OnLoadedColumnsCatchUp(columnLayout, columnLayout.Columns.AllColumns);
						}
					}
				}
			}
		}

		#endregion // OnLoadedCatchUp

        #region EndSelectionDrag
        private void EndSelectionDrag(bool resetMouseDownCell)
        {
            CellBase mdc = this._mouseDownCell;

            this._dragSelectType = DragSelectType.None;
            this.ReleaseMouseCapture();

            if (this._selectRowsCellsTimer != null)
                this._selectRowsCellsTimer.Stop();

            if (resetMouseDownCell)
                this._mouseDownCell = null;
            else
                this._mouseDownCell = mdc;
        }
        #endregion // EndSelectionDrag

		#region LoadOperandFromControlTemplate
		private void LoadFilterIconFromControlTemplate(string key, ComparisonOperator comOperator)
		{
            if (this._rootElement != null)
            {
                DataTemplate dt = this._rootElement.Resources[key] as DataTemplate;

                if (dt != null)
                {
                    if (!this.FilterIcons.ContainsKey(comOperator))
                    {
                        this.FilterIcons.Add(comOperator, dt);
                    }
                    else if (!dt.Equals(this.FilterIcons[comOperator]))
                    {
                        this.FilterIcons[comOperator] = dt;
                    }
                }
            }
		}
		#endregion // LoadOperandFromControlTemplate

		#region CopyToClipboard

		/// <summary>
		/// Copies data to the clipboard
		/// </summary>
		public virtual void CopyToClipboard()
		{
			StringBuilder clipboardBuilder = new StringBuilder();
			List<RowBase> rows = new List<RowBase>();
			Dictionary<ColumnLayout, List<Column>> layouts = new Dictionary<ColumnLayout, List<Column>>();
			bool renderAllCells = true;
			List<CellBase> selectedItems = new List<CellBase>();
            GridClipboardCopyType copyType = this.ClipboardSettings.CopyType;

            if (copyType == GridClipboardCopyType.Default)
            {
                if (this.SelectionSettings.CellClickAction == CellSelectionAction.SelectCell)
                {
                    copyType = GridClipboardCopyType.SelectedCells;
                }
                else
                {
                    copyType = GridClipboardCopyType.SelectedRows;
                }
            }

			if (copyType == GridClipboardCopyType.SelectedCells)
			{
				renderAllCells = false;
				Dictionary<RowBase, List<Cell>> rowSelection = new Dictionary<RowBase, List<Cell>>();

				// Loop through all of the selected cells and group them for organization
				// Since cells can be selected in random order.
				foreach (Cell cell in this.SelectionSettings.SelectedCells)
				{
                    if(cell.GetType().Equals(typeof(GroupByCell)))
                    {
                        continue;
                    }

                    RowBase row = cell.Row;
                    Column col = cell.Column;
                    ColumnLayout colLayout = row.ColumnLayout;

					// Organize cells by Row
					List<Cell> tempSelectedCells = null;
					if (rowSelection.ContainsKey(row))
					{
						tempSelectedCells = rowSelection[row];
					}
					else
					{
						tempSelectedCells = new List<Cell>();
						rowSelection.Add(row, tempSelectedCells);
					}
					tempSelectedCells.Add(cell);

					// Keep track of unique rows.
                    if (!rows.Contains(row))
                    {
                        // Cache the index here, so that we don't need to call InternalRows.IndexOf multiple times when we sort this collection.
                        row.CachedClipboardIndex = this.InternalRows.IndexOf(row);
                        rows.Add(row);
                    }

					// Keep track of what columns are being used on a particular ColumnLayout.
					List<Column> columns = null;
					if (layouts.ContainsKey(colLayout))
					{
                        columns = layouts[colLayout];
					}
					else
					{
						columns = new List<Column>();
                        layouts.Add(colLayout, columns);
					}                                        
                                       
                    if (!columns.Contains(col))
                    {
                        // Cache the index here, so that we don't need to call AllColumns.IndexOf multiple times when we sort this collection.
                        col.CachedClipboardIndex = row.VisibleCells.IndexOf(cell);
                        columns.Add(col);                        
                    }                    
				}
			}
			else
			{
				// Loop through all of the selected rows and group them for organization
				// Since rows can be selected in random order.
				foreach (Row row in this.SelectionSettings.SelectedRows)
				{
                    if (row.GetType().Equals(typeof(GroupByRow)))
                    {
                        continue;
                    }

                    // Cache the index here, so that we don't need to call InternalRows.IndexOf multiple times when we sort this collection.
                    row.CachedClipboardIndex = this.InternalRows.IndexOf(row);
					rows.Add(row);

                    ColumnLayout colLayout = row.ColumnLayout;

					// Keep track of what columns are being used on a particular ColumnLayout.                                    
                    if (!layouts.ContainsKey(colLayout))
					{
						List<Column> columns = new List<Column>();
                        Collection<CellBase> visibleCells = row.VisibleCells;

                        for (int i = 0; i < visibleCells.Count; i++)
                        {
                            Column col = visibleCells[i].Column;

                            if (col.Visibility == Visibility.Visible && !(col is GroupColumn))
                            {
                                // Cache the index here, so that we don't need to call AllColumns.IndexOf multiple times when we sort this collection.
                                col.CachedClipboardIndex = i;
                                columns.Add(col);
                            }
                        }

                        layouts.Add(colLayout, columns);
					}
				}
			}


			// Determine the indentation needed for a particular column layout, and sort all of the
			// Columns for a particular ColumnLayout so they are also in an ordinal order.
			int offsetLayoutIndentationIndex = int.MaxValue;
			ClipboardSortColumnComparer columnComparer = new ClipboardSortColumnComparer();
			Dictionary<ColumnLayout, int> indentationLayouts = new Dictionary<ColumnLayout, int>();
			foreach (KeyValuePair<ColumnLayout, List<Column>> keyPair in layouts)
			{
				// Sort Columns
                if (copyType == GridClipboardCopyType.SelectedCells)
                {
                    keyPair.Value.Sort(columnComparer);
                }

				// Determine Indentation of a particular ColumnLayout.
				int level = 0;
				ColumnLayout layout = keyPair.Key.ColumnLayout;
				while (layout != null)
				{
					layout = layout.ColumnLayout;
					level++;
				}
				indentationLayouts.Add(keyPair.Key, level);
				offsetLayoutIndentationIndex = Math.Min(offsetLayoutIndentationIndex, level);
			}

			// Now sort the rows, so that we can add the rows to the clipboard in an ordinal order.
			rows.Sort(new ClipboardSortRowComparer());

			ColumnLayout currentLayout = null;

		    string newRowString = System.Environment.OSVersion.Platform == PlatformID.MacOSX ? "\r" : "\r\n";

            GridClipboardCopyOptions copyOptions = this.ClipboardSettings.CopyOptions;

			// Finally, we have all the information we need, lets build the clipboard string. 
			foreach (RowBase row in rows)
			{
				List<Column> columns = layouts[row.ColumnLayout];

				// If the first level of a ColumnLayout isn't zero, then we should make that the first indent level. 
				// For example, if we only copied rows from the first ChildBand, we don't want to start off with an indentation
				// So this logic adjusts for that.
				int indentation = indentationLayouts[row.ColumnLayout] - offsetLayoutIndentationIndex;

                // So each time we come across a different ColumnLayout, we should render if IncludeHeader is specified. 
                // Note, we can render a header multiple times for the same ColumnLayout if there is another
                // row from a different ColumnLayout between two rows of the same ColumnLayout.
                if (copyOptions == GridClipboardCopyOptions.IncludeHeaders && currentLayout != row.ColumnLayout)
				{
					// Add identations for ChildBands
                    clipboardBuilder.Append('\t', indentation);

					// Render the Header Text.
					for (int i = 0; i < columns.Count; i++)
					{
						Column column = columns[i];
														
                        if(column is ColumnLayoutTemplateColumn)
                        {
                            continue;
                        }

						if (!string.IsNullOrEmpty(column.HeaderText))
						{
							string headerText = column.HeaderText;
							if (!this.OnClipboardCopyingItem(((RowsManager)(row.Manager)).HeaderRow.Cells[column], ref headerText))
							{
								clipboardBuilder.Append(headerText);
								selectedItems.Add(((RowsManager)(row.Manager)).HeaderRow.Cells[column]);
							}
						}
						else
						{
							string columnKey = column.Key;
							if (!this.OnClipboardCopyingItem(((RowsManager)(row.Manager)).HeaderRow.Cells[column], ref columnKey))
							{
								clipboardBuilder.Append(columnKey);
								selectedItems.Add(((RowsManager)(row.Manager)).HeaderRow.Cells[column]);
							}
						}

						if (i != columns.Count - 1)
						{
							clipboardBuilder.Append('\t');
						}							
					}

					// New Row
                    clipboardBuilder.Append(newRowString);

					currentLayout = row.ColumnLayout;
				}

				// Now add the indentation for the row. 
				clipboardBuilder.Append('\t', indentation);

				// Render cell's value.               
				for (int i = 0; i < columns.Count; i++)
				{
					Column column = columns[i];
					CellBase cell = (CellBase)row.Cells[column];					
                    
                    if (cell is ColumnLayoutTemplateCell)
                    {
                        continue;
                    }

					// Only render a cell's value if it's been selected. 
					if (renderAllCells || cell.IsSelected)
					{
						string cellValue = null;
                        object cellValObj = cell.Value;
                        if (cellValObj != null)
						{
                            string formatString = column.FormatStringResolved;

                            string strVal = "";

                            if (formatString != null)
                                strVal = string.Format(formatString, cellValObj);
                            else
                                strVal = cellValObj.ToString();

                            cellValue = this.EscapeSpecialSymbols(strVal);
						}

						if (!this.OnClipboardCopyingItem(cell, ref cellValue))
						{
							clipboardBuilder.Append(cellValue);
							selectedItems.Add(cell);
						}
					}

					// Always render the space though, even if a cell for this particular row isn't selected                       
                    if (i != columns.Count - 1)
                    {
                        clipboardBuilder.Append('\t');
                    }										
				}

				// New Row
				clipboardBuilder.Append(newRowString);
			}

			if (System.Environment.OSVersion.Platform == PlatformID.MacOSX)
			{
                clipboardBuilder.Replace("\r\n", "\r");
                
                // Remove the last CR
                if (clipboardBuilder[clipboardBuilder.Length - 1] == '\r')
                {
                    clipboardBuilder.Length -= 1;
                }
			}

            string clipboardText = clipboardBuilder.ToString();

			if (this.OnClipboardCopying(new ReadOnlyCollection<CellBase>(selectedItems), ref clipboardText))
			{
				return;
			}

			if (!string.IsNullOrEmpty(clipboardText))
			{
				try
				{
					Clipboard.SetText(clipboardText);
				}
				catch (System.Security.SecurityException)
				{
				}
			}
		}

		#endregion // CopyToClipboard

		#region PasteFromClipboard

		/// <summary>
		/// Pastes data from the clipboard
		/// </summary>
		public virtual void PasteFromClipboard()
		{
            const char separatorChar = '\t';
            const char quoteChar = '"';

			string clipboardText = string.Empty;
			List<List<string>> excelTable = new List<List<string>>();			

			try
			{
				clipboardText = Clipboard.GetText();
			}
			catch (System.Security.SecurityException)
			{
			}

			//If the clipboard is empty
			if (string.IsNullOrEmpty(clipboardText))
			{
				return;
			}

            char[] input = clipboardText.ToCharArray();
            int inputSize = input.Length;
            int pos = 0;

            while (pos < inputSize)
            {
                List<string> cells = new List<string>();

                while (pos < inputSize)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();

                    bool inQuotesValue = input[pos] == quoteChar;

                    // If the sb is in quotes move the pos one step to skip the first double-quote
                    int start = inQuotesValue ? ++pos : pos;
                    bool quoted = inQuotesValue;
                    bool escaped = false;
                    bool endOfCellReached = false;
                    bool endOfLine = false;

                    while (pos < inputSize)
                    {
                        char c = input[pos];

                        // Begin: Quoted sb specific cases
                        if (escaped)
                        {
                            escaped = false;
                            start = pos;
                        }
                        else if (inQuotesValue && quoted && c == quoteChar && (pos + 1 < inputSize && input[pos + 1] == quoteChar))
                        {
                            sb.Append(input, start, pos - start);

                            escaped = true;
                        }
                        else if (inQuotesValue && quoted && c == quoteChar)
                        {
                            sb.Append(input, start, pos - start);

                            // If this is malformed excel output we need to set a new 'start' so we can skip
                            // the double-quote and be able to parse the rest of the cell sb.
                            //
                            // For example excel can produce the following output:
                            //  _01_234567
                            // "\"A\" BCDE" should produce "A BCDE"
                            // When _pos = 2 is reached 'quoted' is set to false and 'start' is set to 3
                            // in order to skip the double-quote at [2].
                            //
                            // Or:
                            //  _01_23_4_56_7_89.....
                            // "\"A\" \"\"B\"\" \"C\"" should produce "A \"\"B\"\" \"C\""
                            //
                            // In this case only the double-quote at [2] is skipped. The rest of the double-quotes won't
                            // be skipped. This behaviour mimics excel.
                            quoted = false;
                            start = pos + 1;
                        }
                        // End: Quoted sb specific cases
                        else if (!quoted && c == separatorChar)
                        {
                            sb.Append(input, start, pos - start);

                            // Move the pos to the beginning of the next cell
                            pos++;

                            endOfCellReached = true;
                            break;
                        }
                        else if (!quoted && (c == '\r' || c == '\n'))
                        {
                            sb.Append(input, start, pos - start);

                            // Move the pos to the beginning of the next cell
                            if (pos < inputSize)
                            {
                                char newLineChar = input[pos];

                                if (newLineChar == '\r')
                                {
                                    pos++;

                                    if (pos < inputSize && input[pos] == '\n')
                                    {
                                        pos++;
                                    }
                                }
                                else if (newLineChar == '\n')
                                {
                                    pos++;
                                }
                            }

                            endOfLine = true;
                            endOfCellReached = true;
                            break;
                        }

                        pos++;
                    }

                    // Handles the case when we haven't reached a separator (\t) or a row separator (\r or \n).
                    // This can happen if the last row doesn't end with row separator (\r or \n)
                    // or if we are dealing with malformed excel output that is not escaped properly. 
                    if (!endOfCellReached)
                    {
                        sb.Append(input, start, pos - start);
                    }

                    string value;

                    // If the sb is not quoted we have to clear the leading and trailing whitespace-characters
                    if (!inQuotesValue)
                    {
                        value = sb.ToString().Trim();
                    }
                    else
                    {
                        value = sb.ToString();
                    }

                    cells.Add(value);

                    if(endOfLine)
                    {
                        break;
                    }
                }

                excelTable.Add(cells);
            }

			this.OnClipboardPasting(excelTable, clipboardText);
		}

		#endregion PasteFromClipboard

        #region EscapeSpecialSymbols

        /// <summary>
        /// Escapes all special characters.
        /// </summary>
        /// <param name="s">The string that will be converted.</param>
        /// <returns>The converted string</returns>
        private string EscapeSpecialSymbols(string s)
        {
            if (s.IndexOf('"') != -1)
            {
                s = s.Replace("\"", "\"\"");
                s = '"' + s + '"';
            }
            else if (s.IndexOfAny(ClipboardCopyEscapeChars) != -1)
            {
                s = '"' + s + '"';                
            }
            else if (!string.IsNullOrEmpty(s) && (s[0].Equals(' ') || s[s.Length - 1].Equals(' ')))
            {
                s = '"' + s + '"';                
            }
            
            return s;
        }

        #endregion // EscapeSpecialSymbols

        #region SetFocusToSpecialRow
        private bool SetFocusToSpecialRow(List<RowBase> rows, Control focusedControl)
        {
            bool cellSetActive = false;

            if (rows.Count > 0)
            {
                foreach (RowBase row in rows)
                {
                    if (row.RowType == RowType.AddNewRow || row.RowType == RowType.FilterRow)
                    {
                        if (row.Cells.Count > 0)
                        {
                            bool setFocus = true;

                            // The control inside of us, must be our own, so lets not steal focus away from it if we don't have to.
                            Control c = focusedControl;
                            if (c != null)
                                setFocus = false;

                            foreach (CellBase cb in row.Cells)
                            {
                                if (cb.Column.Visibility == Visibility.Visible)
                                {
                                    this.SetActiveCell(cb, CellAlignment.NotSet, InvokeAction.Code, true, setFocus, false);
                                    cellSetActive = true;
                                    break;
                                }
                            }

                            if (cellSetActive)
                                break;
                        }
                    }
                }
            }
            return cellSetActive;
        }

        #endregion // SetFocusToSpecialRow

        #region WalkDownGroupByRowsForDataItem

        private Row WalkDownGroupByRowsForDataItem(RowCollection rows, object data)
        {
            Row r = null;

            if (rows.Count > 0 && rows[0] is GroupByRow)
            {
                foreach (GroupByRow gbr in rows)
                {
                    RowsManager rm = gbr.ChildRowsManager as RowsManager;

                    if (rm != null)
                    {
                        if (rm.GroupedColumn == null)
                        {
                            int groupByIndex = rm.DataManager.ResolveIndexForRecord(data);
                            if (groupByIndex != -1)
                            {
                                r = rm.Rows[groupByIndex] as Row;
                            }
                        }
                        else
                        {
                            r = this.WalkDownGroupByRowsForDataItem(gbr.Rows, data);
                        }
                    }

                    if (r != null)
                        break;
                }
            }
            return r;
        }

        #endregion // WalkDownGroupByRowsForDataItem

        #region AttachMouseDownToRootVis

        private void AttachCellMouseDownToRootVis()
        {
            if (this.CurrentEditCell != null || this.CurrentEditRow != null)
            {
                FrameworkElement rootVis = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                if (rootVis != null)
                {
                    _rootVis = new WeakReference(rootVis);
                    rootVis.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_CellEditing_MouseLeftButtonDown), true);
                }
            }
        }

        private void AttachRowMouseDownToRootVis()
        {
            if (this.CurrentEditCell != null || this.CurrentEditRow != null)
            {
                FrameworkElement rootVis = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                if (rootVis != null)
                {
                    _rootVis = new WeakReference(rootVis);
                    rootVis.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_RowEditing_MouseLeftButtonDown), true);
                }
            }
        }

        #endregion // AttachMouseDownToRootVis

        #region DetachMouseDownToRootVis

        private void DetachCellMouseDownToRootVis()
        {
            FrameworkElement rootVis = (this._rootVis != null && this._rootVis.IsAlive) ? this._rootVis.Target as FrameworkElement : null;

            if (rootVis == null)
                rootVis = PlatformProxy.GetRootVisual(this) as FrameworkElement;

            if (rootVis != null)
                rootVis.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_CellEditing_MouseLeftButtonDown));

            this._rootVis = null;
        }

        private void DetachRowMouseDownToRootVis()
        {
            FrameworkElement rootVis = (this._rootVis != null && this._rootVis.IsAlive) ? this._rootVis.Target as FrameworkElement : null;

            if (rootVis == null)
                rootVis = PlatformProxy.GetRootVisual(this) as FrameworkElement;

            if (rootVis != null)
                rootVis.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_RowEditing_MouseLeftButtonDown));

            this._rootVis = null;
        }

        #endregion // DetachMouseDownToRootVis

        #region GetCellControlFromElement
        private CellControlBase GetCellControlFromElement(UIElement elementDirectlyOver)
        {
            DependencyObject dependObject = elementDirectlyOver;
            while (dependObject != null && dependObject != this)
            {
                if (dependObject is CellControlBase || typeof(CellControlBase).IsAssignableFrom(dependObject.GetType()))
                {
                    return dependObject as CellControlBase;
                }
                dependObject = PlatformProxy.GetParent(dependObject);
            }

            return null;
        }
        #endregion // GetCellControlFromElement

        #region InUtilityControl
        private bool InUtilityControl(UIElement elementDirectlyOver)
        {
            DependencyObject dependObject = elementDirectlyOver;
            while (dependObject != null && dependObject != this)
            {
                if (dependObject is CompoundFilterDialogControl || typeof(CompoundFilterDialogControl).IsAssignableFrom(dependObject.GetType()))
                {
                    return true;
                }
                if (dependObject is FilterSelectionControl || typeof(FilterSelectionControl).IsAssignableFrom(dependObject.GetType()))
                {
                    return true;
                }
                if (dependObject is ColumnChooserDialog || typeof(ColumnChooserDialog).IsAssignableFrom(dependObject.GetType()))
                {
                    return true;
                }

                dependObject = PlatformProxy.GetParent(dependObject);
            }

            return false;
        }
        #endregion //InUtilityControl

        #endregion // Private

        #region Internal

        internal void Unload()
        {
            this._isLoaded = false;

            this.ExitEditMode(true);

            if (this.CurrentEditRow != null)
            {
                this.DetachRowMouseDownToRootVis();
            }
            else if (this.CurrentEditCell != null)
            {
                this.DetachCellMouseDownToRootVis();
            }

            if (this._panel != null && this._panel.CustomFilterDialogControl != null)
                this._panel.CustomFilterDialogControl.Hide();

            VisualStateManager.GoToState(this, "Active", false);

            if (this.ColumnChooserDialog != null && this.ColumnChooserDialog.IsOpen)
                this.HideColumnChooser();

            this._rowsManager.Grid = null;

            this._cellsThatCancelledEditMode = null;
        }

        internal void RegisterOpenHeaderDropDownControl(HeaderDropDownControl hddc)
		{
			if (this._openDropDownControl != null && this._openDropDownControl != hddc)
			{
				this._openDropDownControl.IsOpen = false;
			}

			this._openDropDownControl = hddc;
		}

		internal void CloseOpenHeaderDropDownControl()
		{
			if (this._openDropDownControl != null)
			{
				this._openDropDownControl.IsOpen = false;
				this._openDropDownControl = null;
			}
		}

        #region CloneStyleWithoutControlTemplate

        internal static Style CloneStyleWithoutControlTemplate(Style sourceStyle, out ControlTemplate controlTemplate)
        {
            controlTemplate = null;
            Style generatedStyle = null;
            if (sourceStyle != null)
            {
                List<SetterBase> cellSetters = new List<SetterBase>();
                cellSetters.AddRange(sourceStyle.Setters);
                if (cellSetters.Count > 0)
                {
                    generatedStyle = new Style(sourceStyle.TargetType);

                    for (int i = cellSetters.Count - 1; i >= 0; i--)
                    {
                        Setter tempSetter = new Setter();
                        Setter currentSetter = (Setter)cellSetters[i];
                        tempSetter.Property = currentSetter.Property;
                        object setterValue = currentSetter.Value;
                        
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

                        if (setterValue is uint)
                        {
                            setterValue = setterValue.ToString();
                        }
                        if (setterValue is ControlTemplate)
                        {
                            controlTemplate = (ControlTemplate)setterValue;
                            continue;
                        }
                        tempSetter.Value = setterValue;
                        generatedStyle.Setters.Add(tempSetter);
                    }
                }
            }
            return generatedStyle;
        }

        #endregion // CloneStyleWithoutControlTemplate

		// AS 2/27/12 NA 12.2 Gantt
		#region CreateRootRowsManager
		internal virtual XamGridRowsManager CreateRootRowsManager()
		{
			return new XamGridRowsManager(this);
		} 
		#endregion //CreateRootRowsManager






        internal void LoadData()
        {
            this.IsLoaded = true;

            this._rowsManager.Grid = this;

            foreach (ColumnLayout layout in this.ColumnLayouts)
                layout.Grid = this;

            IEnumerable itemSource = (IEnumerable)this.GetValue(ItemsSourceProperty);

            if (itemSource != null)
            {
                this.ApplyItemSource(itemSource);
            }

            foreach (ColumnBase cb in this.Columns.AllColumns)
            {
                Column col = cb as Column;
                if (col != null)
                {
                    if (col.IsGroupBy)
                    {
                        GroupByColumnsCollection gbcs = this.GroupBySettings.GroupByColumns;
                        if (!gbcs.Contains(col))
                            gbcs.Add(col);
                    }

                    if (col.IsSelected)
                    {
                        SelectedColumnsCollection sccs = this.SelectionSettings.SelectedColumns;
                        if (!sccs.Contains(col))
                            sccs.InternalAddItemSilently(sccs.Count, col);
                    }
                }
            }

            this.OnLoadedColumnsCatchUp(this.Columns.ColumnLayout, this.Columns.AllColumns);

            if (this.CurrentEditRow != null)
            {
                FrameworkElement rootVis = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                if (rootVis != null)
                    rootVis.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_RowEditing_MouseLeftButtonDown), true);

                this.ScrollCellIntoView(this.CurrentEditCell);
            }
            else if (this.CurrentEditCell != null)
            {
                FrameworkElement rootVis = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                if (rootVis != null)
                    rootVis.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_CellEditing_MouseLeftButtonDown), true);

                this.ScrollCellIntoView(this.CurrentEditCell);
            }
        }

        internal void ThrowInvalidColumnKeyException(string columnKey)
        {



            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                throw new InvalidColumnKeyException(columnKey);
            }));

        }

		// AS 3/15/12 NA 12.2 Gantt
		#region ShouldAddColumnLayouts
		/// <summary>
		/// Returns a boolean indicating if the RowsManager should add in ColumnLayout instances for any DataField's for a Columnlayout that doesn't support autogenerated columns.
		/// </summary>
		/// <param name="columnLayout">The column layout being initialized</param>
		/// <returns>Returns true to indicate that the DataField's should be enumerated to determine if there are any ColumnLayouts that need to be added into the ColumnLayout being initialized.</returns>
		internal virtual bool ShouldAddColumnLayouts(ColumnLayout columnLayout)
		{
			return true;
		} 
		#endregion //ShouldAddColumnLayouts

		#endregion // Internal

        #region Static

        #region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

        #endregion // RegisterResources

        #region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }
        #endregion // UnregisterResources

        #endregion // Static

        #endregion // Methods

        #region EventHandlers

        #region HorizontalScrollBar Scroll

        private void HorizontalScrollbar_Scroll(object sender, ScrollEventArgs e)
		{
			this.InvalidateScrollPanel(true, false);
		}

		#endregion // HorizontalScrollBar Scroll

		#region VerticalScrollBar Scroll

		private void VerticalScrollbar_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.ScrollEventType != ScrollEventType.ThumbTrack || this.DeferredScrollingSettings.AllowDeferredScrolling == DeferredScrollingType.None)
			{
				if (this._deferredPopup != null && this._deferredPopup.IsOpen)
				{
					if (this._verticalScrollbar.Value != this._verticalScrollbar.Maximum)
					{
						// Make sure that the value currently being displayed in the InternalTemplate is fully visible. 
						this._verticalScrollbar.Value = (int)this._verticalScrollbar.Value;
					}
					this._deferredPopup.IsOpen = false;
				}
				this.InvalidateScrollPanel(false);
			}
			else
			{
				if (this._deferredPopup == null)
				{
					this._deferredPopup = new Popup();
					this._deferredContentControl = new ContentControl();
					this._deferredPopup.Child = this._deferredContentControl;
					this._deferredContentControl.HorizontalContentAlignment = HorizontalAlignment.Center;
					this._deferredContentControl.VerticalContentAlignment = VerticalAlignment.Center;


                    this._deferredPopup.Placement = PlacementMode.Relative;
                    this._deferredPopup.PlacementTarget = this.Panel;
                    this._deferredPopup.AllowsTransparency = true;

				}

				RowBase row = this.InternalRows[(int)this.VerticalScrollBar.Value];
				if (row != null)
				{
					DataTemplate template = null;

					if (row.RowType == RowType.GroupByRow)
					{
						template = row.ColumnLayout.DeferredScrollingSettings.GroupByDeferredScrollTemplateResolved;
						if (template == null)
							template = this._defaultGroupByDeferredScrollingTemplate;
					}
					else if (row.RowType == RowType.DataRow)
					{
						template = row.ColumnLayout.DeferredScrollingSettings.DeferredScrollTemplateResolved;
						if (template == null)
							template = this._defaultDeferredScrollingTemplate;
					}

					if (template != null)
					{
						FrameworkElement element = template.LoadContent() as FrameworkElement;
						if (element != null)
						{

							ScrollTipInfo info = new ScrollTipInfo() { Row = row };
							string key = null;

							key = row.ColumnLayout.DeferredScrollingSettings.DefaultColumnKeyResolved;

							if (key == null && row.Columns.VisibleColumns.Count > 0)
							{
                                Column col = row.Columns.VisibleColumns[0];

                                ICollectionBase icb = col.ResolveChildColumns();
                                while (icb != null && icb.Count > 0)
                                {
                                    foreach (Column childCol in icb)
                                    {
                                        if (childCol.Visibility == System.Windows.Visibility.Visible)
                                        {
                                            col = childCol;
                                            break;
                                        }
                                    }
                                    icb = col.ResolveChildColumns();
                                }

								key = col.Key;
							}

							if (key != null)
							{
								Binding b = new Binding(key);
								b.Source = row.Data;
								info.SetBinding(ScrollTipInfo.FirstColumnValueProperty, b);
								info.Column = row.ColumnLayout.Columns[key] as Column;
							}

							if (row.RowType == RowType.GroupByRow)
								info.Column = ((RowsManager)row.Manager).GroupedColumn;

							element.DataContext = info;
							this._deferredContentControl.Content = element;




                            Point point = this.Panel.TransformToVisual(this).Transform(new Point(0, 0));


							double fixedRowHeight = 0;
							foreach (RowBase r in this.Panel.FixedRowsTop)
							{
								fixedRowHeight += r.ActualHeight;
							}
							element.Height = this.Panel.ActualHeight - fixedRowHeight;
							element.Width = this.Panel.ActualWidth;

							this._deferredPopup.VerticalOffset = point.Y + fixedRowHeight;
							this._deferredPopup.HorizontalOffset = point.X;

                            this._deferredPopup.FlowDirection = this.FlowDirection;

							this._deferredPopup.IsOpen = true;
						}
					}
				}
			}
		}

		#endregion // VerticalScrollBar Scroll

		#region SelectRowsCellsTimer_Tick
		private void SelectRowsCellsTimer_Tick(object sender, EventArgs e)
		{
			// Get Bounds of the RowsPanel
			Rect r = Rect.Empty;
			try
			{

                GeneralTransform gt = this._panel.TransformToVisual(this);
                Rect bounds = LayoutInformation.GetLayoutSlot(this._panel);
                r = gt.TransformBounds(bounds);





			}
			catch (ArgumentException) { };

			if (r.IsEmpty)
			{
				this._selectRowsCellsTimer.Stop();
				return;
			}

			double top = r.Y;
			double bottom = top + this._panel.ActualHeight;
			double left = r.X;
			double right = left + this._panel.ActualWidth;

			// Calculate the Y position we should pretend we're on inside of the RowsPanel
			double y = this._mousePosition.Y;
			if (y < top)
			{
				y = top;
				foreach (RowBase topRow in this._panel.FixedRowsTop)
				{
					if (topRow.Control != null)
						y += topRow.Control.ActualHeight + 2;
				}
			}
			else if (y > bottom)
			{
				y = bottom - 1;
				foreach (RowBase bottomRow in this._panel.FixedRowsBottom)
				{
					if (bottomRow.Control != null)
						y -= bottomRow.Control.ActualHeight;
				}
			}

			// To Calculate X properly we need to figure out which row we're representing.
            IEnumerable<UIElement> elements = PlatformProxy.GetElementsFromPoint(new Point(left, y), this._panel);
			CellsPanel cellsPanel = null;
			foreach (UIElement elem in elements)
			{
				cellsPanel = elem as CellsPanel;
				if (cellsPanel != null)
					break;
			}

			// Now that we have the Row, we can adjust the left, and possibly the right for the fixed cells.
			if (cellsPanel != null)
			{
				foreach (CellBase leftCell in cellsPanel.VisibleFixedLeftCells)
				{
					if (leftCell.Control != null)
					{
						Column col = leftCell.Column;
						if (col == null || (col.IsFixed == FixedState.NotFixed && !col.IsGroupBy))
							left += leftCell.Control.ActualWidth + 2;
					}
				}

                RowBase cpr = cellsPanel.Row;
                CellBase filler = cpr.Cells[cpr.ColumnLayout.Columns.FillerColumn];
                if (cellsPanel.VisibleCells.Contains(filler))
                {
                    right -= filler.Column.ActualWidth;    
                }
			}

			// Calculate the X position that we're representing inside of the RowsPanel.
			double x = this._mousePosition.X;
			if (x < left)
				x = left;
			else if (x > right)
				x = right - 1;

			// Only Scroll Horizontally if we're doing Cell Selection.
			if (this._dragSelectType == DragSelectType.Cell)
			{
				if (this._horizontalScrollbar != null && this._horizontalScrollbar.Visibility == Visibility.Visible)
				{
					if (this._mousePosition.X < left)
						this._horizontalScrollbar.Value--;
					else if (this._mousePosition.X > right)
						this._horizontalScrollbar.Value++;
				}
			}

			// Cell and Row Selection are allowed to scroll vertically. 
			if (this._verticalScrollbar != null)
			{
				if (this._mousePosition.Y < top)
					this._verticalScrollbar.Value--;
				else if (this._mousePosition.Y > bottom)
					this._verticalScrollbar.Value++;
			}

			// Now that we have a proper X and Y coordinate, lets figure out what cell we'll need to select or unselect.
            elements = PlatformProxy.GetElementsFromPoint(new Point(x, y), this._panel);
			CellControlBase cellControl = null;
			foreach (UIElement elem in elements)
			{
				cellControl = elem as CellControl;
				if (cellControl != null && cellControl.Cell != null && !(cellControl.Cell.Column is FillerColumn))
					break;
			}

			// If We have a cell, lets select or unselect it or its row.
			if (cellControl != null && cellControl.Cell != null)
			{
				if (this._dragSelectType == DragSelectType.Cell)
					this.SelectCell(cellControl.Cell as Cell, InvokeAction.MouseMove);
				else
					this.SelectRow(cellControl.Cell.Row as Row, InvokeAction.MouseMove);

			}

			// Tell the Grid to redraw.
			this.InvalidateScrollPanel(false);

		}
		#endregion // SelectRowsCellsTimer_Tick

		#region XamWebGrid_KeyDown
		void XamWebGrid_KeyDown(object sender, KeyEventArgs e)
		{
            if (_tabMode != null)
            {
                PlatformProxy.SetTabNavigation(this, (KeyboardNavigationMode)this._tabMode);
            }

			if (e.Handled)
				return;

            int platformKey = 0; 






            




            if (e.Key == Key.Tab || 
                e.Key == Key.Down || 
                e.Key == Key.Up || 
                e.Key == Key.Left || 
                e.Key == Key.Right ||
                e.Key == Key.PageDown ||
                e.Key == Key.PageUp)
            {
                if (this._lastKeyboardObject != null)
                {
                    if (e.OriginalSource == this._lastKeyboardObject.Target)
                    {
                        return;
                    }
                }
                DependencyObject source = e.OriginalSource as DependencyObject;

                while (source != null)
                {
                    if (source is HeaderDropDownControl)
                    {
                        this._lastKeyboardObject = new WeakReference(source);
                        return;
                    }

                    if (source is Infragistics.Controls.Menus.XamMenuItem)
                    {
                        this._lastKeyboardObject = new WeakReference(source);
                        return;
                    }

                    if (source is ColumnFilterDialogControl)
                    {
                        this._lastKeyboardObject = new WeakReference(source);
                        return;
                    }

                    source = PlatformProxy.GetParent(source);
                }
            }
            
             this._lastKeyboardObject = null;
            


            if (this.CurrentEditCell != null)
			{
                e.Handled = this.CurrentEditCell.HandleKeyDown(e.Key, platformKey);
			}
			else if (this.ActiveCell != null)
			{
                e.Handled = this.ActiveCell.HandleKeyDown(e.Key, platformKey);
			}

            // Ok, this means that we've reached the end (either first or last cell) and we should leave the control
            // However, b/c other elements are focusable (i.e. IsTabStop == true) the focus then goes to other elements inside of the grid. 
            // So, if we set the TabNavigation to ONce, that will tell the SL framework to leave the grid. 
            // But we're going to store the original tabNavigation mode of the grid, and restore it back in xamwebGrid's key down method, so
            // that TabNavigation will continue to work as normal, when it gets triggered again.
            if (e.Key == Key.Tab && !e.Handled)
            {
                this._tabMode = PlatformProxy.GetTabNavigation(this);
                PlatformProxy.SetTabNavigation(this, KeyboardNavigationMode.Once);
            }

			if (!e.Handled)
			{
				switch (e.Key)
				{
					case Key.PageUp:
						if (this.Panel != null && this.Panel.VisibleRows.Count > 0)
						{
                            int indexOfFirstRow = this.InternalRows.IndexOf(this.Panel.VisibleRows[0]);
                            RowBase firstVisibleRow = this.InternalRows[indexOfFirstRow];
                            double step = this.VerticalScrollBar.LargeChange;
						    bool isFirstRowPartiallyCut = false;

                            // If the first visible row is partially visible,
                            // modify the step so that we won't jump over it.
                            if (firstVisibleRow != null && firstVisibleRow.Control != null)
                            {
                                Rect rowLayout = LayoutInformation.GetLayoutSlot(firstVisibleRow.Control);
                                double topHeight = 0;

                                foreach (RowBase r in this.Panel.FixedRowsTop)
                                {
                                    if (r.Control != null)
                                    {
                                        topHeight += LayoutInformation.GetLayoutSlot(r.Control).Height;
                                    }
                                }

                                if (rowLayout.Top - topHeight < 0)
                                {
                                    // So, the row is partially cut...
                                    // Lets make sure that we won't jump over it when paging.
                                    step--;
                                    isFirstRowPartiallyCut = true;
                                }
                            }

                            if (this.ActiveCell != null)
                            {
                                RowBase fullRow = firstVisibleRow;

                                if (isFirstRowPartiallyCut && (indexOfFirstRow + 1) <= (this.InternalRows.Count - 1))
                                {
                                    fullRow = this.InternalRows[indexOfFirstRow + 1];
                                }
                                
                                RowBase activeCellRow = this.ActiveCell.Row;

                                if (fullRow != null && activeCellRow != fullRow && activeCellRow.RowType == fullRow.RowType)
                                {
                                    int indexOfFirstFullyVisibleRow = this.InternalRows.IndexOf(fullRow);
                                    int indexOfLastRow = this.InternalRows.IndexOf(this.Panel.VisibleRows[this.Panel.VisibleRows.Count - 1]);
                                    if(this.PageUpActivateCellInRowRange(indexOfFirstFullyVisibleRow, indexOfLastRow))
                                    {
                                        break;
                                    }
                                }
                            }

                            var newValue = this.VerticalScrollBar.Value - step;
                            newValue = Math.Max(newValue, this.VerticalScrollBar.Minimum);

                            this.VerticalScrollBar.Value = newValue;
                            this.InvalidateScrollPanel(false, false, false, this.PageUpCallback);
						}
						break;

					case Key.PageDown:
						if (this.Panel != null && this.Panel.VisibleRows.Count > 0)
						{
                            int indexOfLastRow = this.InternalRows.IndexOf(this.Panel.VisibleRows[this.Panel.VisibleRows.Count - 1]);
                            RowBase lastVisibleRow = this.InternalRows[indexOfLastRow];
                            double step = this.VerticalScrollBar.LargeChange;
						    bool isLastRowPartiallyCut = false;

                            // If the last visible row is partially visible,
                            // modify the step so that we won't jump over it.
                            if (lastVisibleRow != null && lastVisibleRow.Control != null)
                            {
                                Rect panelLayout = LayoutInformation.GetLayoutSlot(this.Panel);
                                Rect rowLayout = LayoutInformation.GetLayoutSlot(lastVisibleRow.Control);
                                double bottomHeight = 0;

                                foreach (RowBase r in this.Panel.FixedRowsBottom)
                                {
                                    if (r.Control != null)
                                    {
                                        bottomHeight += LayoutInformation.GetLayoutSlot(r.Control).Height;
                                    }
                                }

                                if ((rowLayout.Height + rowLayout.Top + bottomHeight) > panelLayout.Height)
                                {
                                    // So, the row is partially cut...
                                    // Lets make sure that we won't jump over it when paging.
                                    step--;
                                    isLastRowPartiallyCut = true;
                                }
                            }

						    if (this.ActiveCell != null)
						    {
						        RowBase fullRow = lastVisibleRow;

						        if (isLastRowPartiallyCut && (indexOfLastRow - 1) >= 0)
						        {
						            fullRow = this.InternalRows[indexOfLastRow - 1];
						        }

						        RowBase activeCellRow = this.ActiveCell.Row;

                                if (fullRow != null && activeCellRow != fullRow && activeCellRow.RowType == fullRow.RowType)
						        {
                                    int indexOfFirstRow = this.InternalRows.IndexOf(this.Panel.VisibleRows[0]);
                                    int indexOfLastFullyVisibleRow = this.InternalRows.IndexOf(fullRow);
                                    if(this.PageDownActivateCellInRowRange(indexOfFirstRow, indexOfLastFullyVisibleRow))
						                break;
						        }
						    }

                            var newValue = this.VerticalScrollBar.Value + step;
                            newValue = Math.Min(newValue, this.VerticalScrollBar.Maximum);

                            this.VerticalScrollBar.Value = newValue;
                            this.InvalidateScrollPanel(false, false, false, this.PageDownCallback);
						}
						break;

					case Key.Delete:

						if (this.CurrentEditCell == null && this.CurrentEditRow == null)
						{
							Cell prevActiveCell = this.ActiveCell as Cell;
							int newActiveCellRowIndex = 0;
							int newActiveCellIndex = 0;

							bool clearSelectedCells = false, clearSelectedRows = false;
							Dictionary<RowsManager, IList<Row>> rowsToDeleteHT = new Dictionary<RowsManager, IList<Row>>();

							SelectedCellsCollection selectedCells = this.SelectionSettings.SelectedCells;
							SelectedRowsCollection selectedRows = this.SelectionSettings.SelectedRows;

							// Loop through Selected Cells and Find unique rows to delete.
							if (selectedCells.Count > 0)
							{
								foreach (Cell c in selectedCells)
								{
									Row r = c.Row as Row;
									if (r != null)
									{
										RowsManager manager = (RowsManager)r.Manager;

										if (!rowsToDeleteHT.ContainsKey(manager))
											rowsToDeleteHT.Add(manager, new List<Row>());

										IList<Row> rowsToDelete = rowsToDeleteHT[manager];

										if ((r.ColumnLayout.DeleteKeyActionResolved == DeleteKeyAction.DeleteRowsOfSelectedCells ||
											r.ColumnLayout.DeleteKeyActionResolved == DeleteKeyAction.DeleteRowsOfSelectedCellsAndRows) &&
											!rowsToDelete.Contains(r))
										{
                                            if (r.CanBeDeleted)
                                            {
                                                rowsToDelete.Add(r);
                                                clearSelectedCells = true;
                                            }
										}
									}
								}
							}

							// Loop through Selected rows to delete.
							if (selectedRows.Count > 0)
							{
								foreach (Row row in selectedRows)
								{
									RowsManager manager = (RowsManager)row.Manager;

									if (!rowsToDeleteHT.ContainsKey(manager))
										rowsToDeleteHT.Add(manager, new List<Row>());

									IList<Row> rowsToDelete = rowsToDeleteHT[manager];

									if ((row.ColumnLayout.DeleteKeyActionResolved == DeleteKeyAction.DeleteSelectedRows ||
										row.ColumnLayout.DeleteKeyActionResolved == DeleteKeyAction.DeleteRowsOfSelectedCellsAndRows) &&
										!rowsToDelete.Contains(row))
									{
                                        if (row.CanBeDeleted)
                                        {
                                            rowsToDelete.Add(row);
                                            clearSelectedRows = true;
                                        }
									}
								}
							}

							// Determine if the ActiveRow should be deleted.
							if (prevActiveCell != null)
							{
								newActiveCellRowIndex = Cell.ResolveRowIndex(this.ActiveCell.Row, false);
								newActiveCellIndex = this.ActiveCell.Row.VisibleCells.IndexOf(this.ActiveCell);

								if (this.ActiveCell.Row.ColumnLayout.DeleteKeyActionResolved == DeleteKeyAction.DeleteRowOfActiveCell)
								{
									Row r = this.ActiveCell.Row as Row;
									if (r != null)
									{
										RowsManager manager = (RowsManager)r.Manager;

										if (!rowsToDeleteHT.ContainsKey(manager))
											rowsToDeleteHT.Add(manager, new List<Row>());

										IList<Row> rowsToDelete = rowsToDeleteHT[manager];

										if (!rowsToDelete.Contains(r) && r.CanBeDeleted)
										{
											rowsToDelete.Add(r);
											this.ActiveCell = null;
										}
									}
								}
							}

                            // Loop through all legitimate rows, and delete them.
                            foreach (KeyValuePair<RowsManager, IList<Row>> pair in rowsToDeleteHT)
                            {
                                ((RowCollection)pair.Key.Rows.ActualCollection).RemoveRange(pair.Value);
                            }

                            if (clearSelectedCells)
                                selectedCells.Clear();

                            if (clearSelectedRows)
                                selectedRows.Clear();

                            // If possible, lets reset the active cell. 
                            if (this.ActiveCell == null && prevActiveCell != null)
                            {

                                // Invoke an update so that when the Dispatcher gets invoked we'll be in a proper state.
                                this.UpdateLayout();

                                // The cleanest way to make sure the correct cell becomes active, is to delay it. 
                                // That way we're waiting until the rows have a chance to re-expand and register, after they've been deleted. 
                                this.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        RowBase row = Cell.ResolveRow(prevActiveCell.Row, newActiveCellRowIndex, true, KeyboardNavigation.AllLayouts);
                                        if (row != null)
                                        {
                                            Collection<CellBase> cells = row.VisibleCells;

                                            if (newActiveCellIndex < cells.Count)
                                                this.ActiveCell = cells[newActiveCellIndex];
                                            else
                                            {
                                                foreach (CellBase cell in cells)
                                                {
                                                    if (cell is Cell)
                                                    {
                                                        this.ActiveCell = cell;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }));
                            }
						}

						break;

					// Copy uisng Ctrl-C
					case Key.C:

						bool ctrlKey = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control

                            );




						if (ctrlKey && this.ClipboardSettings.AllowCopy)
						{
							this.CopyToClipboard();
						}

						break;

					// Paste using Ctrl-V
					case Key.V:

						bool ctrlKey1 = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control

                                        );




						if (ctrlKey1 && this.ClipboardSettings.AllowPaste)
						{
							this.PasteFromClipboard();
						}

						break;
				}
			}
		}
		#endregion // XamWebGrid_KeyDown

		#region XamWebGrid_MouseLeftButtonUp
		void XamWebGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.OnGridMouseLeftButtonUp(e);
		}

		internal virtual void OnGridMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			this.EndSelectionDrag(false);

            if (this._mouseDownCell != null)
            {




                CellControlBase cellControl = null;
                IEnumerable<UIElement> elems = PlatformProxy.GetElementsFromPoint(e.GetPosition(this), this);
                foreach (UIElement elem in elems)
                {
                    CellControlBase ccb = elem as CellControlBase;
                    if (ccb != null)
                        cellControl = ccb;
                }

                if (cellControl != null && cellControl.Cell == this._mouseDownCell)
                    this._mouseDownCell.OnCellClick(e);

                this._mouseDownCell = null;
            }
		}
		#endregion // XamWebGrid_MouseLeftButtonUp

		#region XamWebGrid_MouseLeftButtonDown
		void XamWebGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.OnGridMouseLeftButtonDown(e);
		}

		internal virtual void OnGridMouseLeftButtonDown(MouseButtonEventArgs e)
		{





            if (!e.Handled)
            {

                this.Dispatcher.BeginInvoke(new Action(() => { this.Focus(); }));



            }

			bool doubleClick = false;
			long currentTicks = DateTime.Now.Ticks;

			CellControlBase cellControl = this.GetCellFromSource(e.OriginalSource as DependencyObject);

            if (cellControl != null && cellControl.Cell != null)
            {
                if ((currentTicks - this._doubleClickTimeStamp) <= 4000000 && cellControl.Cell == this._doubleClickCell)
                    doubleClick = true;

                this._doubleClickTimeStamp = DateTime.Now.Ticks;

                Cell cell = cellControl.Cell as Cell;

                if (doubleClick)
                {
                    if (cell == null || !cell.IsEditing)
                        cellControl.Cell.OnCellDoubleClick();
                    this._doubleClickCell = this._mouseDownCell = null;
                    return;
                }

                if (cell == null || !cell.IsEditing)
                {
                    if (cell is ColumnLayoutTemplateCell)
                    {
                        if (!cell.IsSelected)
                            cellControl.Cell.OnCellMouseDown(e);
                        this._dragSelectType = DragSelectType.None;
                    }
                    else
                    {
                        CustomDisplayEditableColumn col = cellControl.Cell.Column as CustomDisplayEditableColumn;

                        if (col == null || ((cell != null && !cell.EnableCustomEditorBehaviors) || col.EditorDisplayBehavior != EditorDisplayBehaviors.Always))
                        {
                            // NZ 8 March 2012 - TFS101205 - Get the value of e.Handled before invoking 
                            // CellBase.OnCellMouseDown(e), because it can mark the event as handled.
                            bool isHandled = e.Handled;
                            this._dragSelectType = DragSelectType.None;
                            
                            // In WPF calling CaptureMouse triggers a MouseMove call
                            // Which causes selection to be fired twice, onece for MouseDown, and the other
                            // For Dragging, so don't set the DragSelectType until after the CaptureMouse call. 
                            DragSelectType dst = cellControl.Cell.OnCellMouseDown(e);

                            this._doubleClickCell = this._mouseDownCell = cellControl.Cell;

                            // NZ 8 March 2012 - TFS101205 - Do not capture the mouse if you do not need to.
                            // If the event is handled steling the mouse capture can prevent subsequent events
                            // from firing, for example in WPF if the mouse is captured in MouseLeftButtonDown
                            // the Click event won't be fired.
                            if (dst != DragSelectType.None && !cellControl.IsResizing && !isHandled)
                            {
                                this.CaptureMouse();
                            }

                            this._dragSelectType = dst;
                        }
                    }
                }

                if (this.EditingSettings.AllowEditing == EditingType.Hover && (cell != null && (cell.IsEditing && !cell.IsActive)))
                {
                    SetActiveCell(cell, CellAlignment.NotSet, InvokeAction.Click, false);
                }
                else if (cell != null && cell.SupportsActivation && !cell.IsActive && cell.EnableCustomEditorBehaviors)
                {
                    // NZ 8 June 2012 - TFS112015
                    CustomDisplayEditableColumn column = cell.Column as CustomDisplayEditableColumn;

                    if (column != null && column.EditorDisplayBehavior == EditorDisplayBehaviors.Always)
                    {
                        SetActiveCell(cell, CellAlignment.NotSet, InvokeAction.Click, true);
                    }
                }
			}
			else
            {
                this._doubleClickCell = this._mouseDownCell = null;
            }
	}
		#endregion // XamWebGrid_MouseLeftButtonDown

		#region RootVis_CellEditing_MouseLeftButtonDown
		void RootVis_CellEditing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			CellControlBase cellControl = this.GetCellFromSource(e.OriginalSource as DependencyObject);

			if (cellControl == null || cellControl.Cell == null || this.CurrentEditCell != cellControl.Cell)
				this.ExitEditMode(false);
		}
		#endregion // RootVis_CellEditing_MouseLeftButtonDown

		#region RootVis_RowEditing_MouseLeftButtonDown

		void RootVis_RowEditing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			CellControlBase cellControl = this.GetCellFromSource(e.OriginalSource as DependencyObject);

			if (cellControl == null)
			{
				DependencyObject obj = e.OriginalSource as DependencyObject;

				while (obj != null && !(obj == this.HorizontalScrollBar))
                    obj = PlatformProxy.GetParent(obj);

				if (obj == this.HorizontalScrollBar)
					return;
			}

			if (cellControl == null || cellControl.Cell == null || this.CurrentEditRow != cellControl.Cell.Row)
				this.ExitEditMode(false);
		}
		#endregion // RootVis_RowEditing_MouseLeftButtonDown

		#region XamWebGrid_Loaded
		void XamWebGrid_Loaded(object sender, RoutedEventArgs e)
		{

		    // AS 3/28/12 TFS107071
		    // See the notes in OnInitialized.
		    //
		    if (!this._isLoaded)
		        this.LoadData();





            if (this._panel != null && this._panel.VisibleRows.Count == 0)
		        this.InvalidateScrollPanel(false);

        }

        #endregion // XamWebGrid_Loaded

        #region XamGrid_Unloaded
        void XamGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unload();
        }
        #endregion // XamGrid_Unloaded

        #region ColumnLayouts_CollectionChanged

        void ColumnLayouts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				foreach (ColumnLayout c in e.NewItems)
				{
					c.Grid = this;
				}
			}
			else if (e.OldItems != null)
			{
				foreach (ColumnLayout c in e.OldItems)
				{
                    c.OnColumnLayoutDisposing();
					c.Grid = null;
				}
			}
		}
		#endregion // ColumnLayouts_CollectionChanged

		#region ConditionalFormattingSettings_PropertyChanged

		void ConditionalFormattingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.RowsManager.ColumnLayout.ClearCachedRows();
			this.ResetPanelRows(true);
		}

		#endregion // ConditionalFormattingSettings_PropertyChanged

        #endregion // EventHandlers

        #region IProvideScrollInfo Members

        ScrollBar IProvideScrollInfo.VerticalScrollBar
		{
			get { return this.VerticalScrollBar; }
		}

		ScrollBar IProvideScrollInfo.HorizontalScrollBar
		{
			get { return this.HorizontalScrollBar; }
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="XamGrid"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._internalRows != null)
				this._internalRows.Dispose();

			if (this._rowsManager != null)
				this._rowsManager.Dispose();

			if (this._selectionSettings != null)
				this._selectionSettings.Dispose();

			if (this._columnTypeMappings != null)
				this._columnTypeMappings.Dispose();

			if (this._columnLayouts != null)
				this._columnLayouts.Dispose();

			if (this._groupBySettings != null)
				this._groupBySettings.Dispose();

			if (this._columnLayouts != null)
				this._columnLayouts.CollectionChanged -= ColumnLayouts_CollectionChanged;

			if (this._summaryRowSettings != null)
				this._summaryRowSettings.Dispose();
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="XamGrid"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Fired when a property changes on the <see cref="XamGrid"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Invoked when a property changes on the <see cref="XamGrid"/> object.
		/// </summary>
		/// <param propertyName="propertyName">The propertyName of the property that has changed.</param>
		protected virtual void OnPropertyChanged(string name)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion

		#region Events

		#region PageIndexChanging
		/// <summary>
		/// Event raised prior to a new page index being assigned.
		/// </summary>
		public event EventHandler<CancellablePageChangingEventArgs> PageIndexChanging;

		/// <summary>
        /// Raises the <see cref="PageIndexChanging"/> event.
		/// </summary>
		/// <param propertyName="newPageIndex">the index of the new page to be selected</param>
		/// <param propertyName="level">The <see cref="RowsManager"/> level value for the <see cref="RowCollection"/></param>
		/// <param propertyName="rows">The <see cref="RowCollection"/> which is being paged. </param>
		/// <param propertyName="layout">The <see cref="ColumnLayout"/> of the rows collection.</param>
		/// <returns></returns>
		protected internal virtual bool OnPageIndexChanging(int newPageIndex, int level, RowCollection rows, ColumnLayout layout)
		{
			if (this.PageIndexChanging != null)
			{
				CancellablePageChangingEventArgs e = new CancellablePageChangingEventArgs() { NextPageIndex = newPageIndex, ColumnLayout = layout, Rows = rows, Level = level };

				this.PageIndexChanging(this, e);

				return e.Cancel;
			}
			return false;
		}
		#endregion // PageIndexChanging
        
		#region PageIndexChanged

		/// <summary>
		/// This event is raised before the PageIndex of a particular data island has changed.
		/// </summary>
		public event EventHandler<PageChangedEventArgs> PageIndexChanged;

		/// <summary>
		/// Raises the <see cref="PageIndexChanged"/> event.
		/// </summary>
		/// <param propertyName="oldPageIndex"></param>
		protected internal virtual void OnPageIndexChanged(int oldPageIndex)
		{
			if (this.PageIndexChanged != null)
			{
				PageChangedEventArgs e = new PageChangedEventArgs() { OldPageIndex = oldPageIndex };
				this.PageIndexChanged(this, e);
			}
		}

		#endregion // PageIndexChanged

		#region ColumnSorting

		/// <summary>
		/// The ColumnSorting event is raised when the user changes the sort on the <see cref="XamGrid"/> via the GUI.
		/// </summary>
		public event EventHandler<SortingCancellableEventArgs> ColumnSorting;

		/// <summary>
		/// Raised when a column is sorted via the user interface.
		/// </summary>
		/// <param propertyName="sortDirection">The new <see cref="SortDirection"/> to be assigned to the column.</param>
		/// <param propertyName="sortedColumn">The <see cref="Column"/> being sorted.</param>
		protected internal virtual bool OnColumnSorting(Column sortedColumn, SortDirection sortDirection)
		{
			if (this.ColumnSorting != null)
			{
				SortingCancellableEventArgs e = new SortingCancellableEventArgs() { Column = sortedColumn, NewSortDirection = sortDirection, PreviousSortDirection = sortedColumn.IsSorted };

				this.ColumnSorting(this, e);

				return e.Cancel;
			}
			return false;
		}

		#endregion // ColumnSorting

		#region ColumnSorted

		/// <summary>
		/// The ColumnSorted event is raised after the <see cref="XamGrid"/> has completed its sort.
		/// </summary>
		public event EventHandler<SortedColumnEventArgs> ColumnSorted;

		/// <summary>
		/// Raised after the sort is applied to the XamGrid.
		/// </summary>
		/// <param propertyName="sortDirection">The old <see cref="SortDirection"/> that was assigned to the column.</param>
		/// <param propertyName="sortedColumn">The <see cref="Column"/> that was sorted.</param>
		protected internal virtual void OnColumnSorted(Column sortedColumn, SortDirection sortDirection)
		{
			if (this.ColumnSorted != null)
			{
				this.ColumnSorted(this, new SortedColumnEventArgs() { Column = sortedColumn, PreviousSortDirection = sortDirection });
			}
		}

		#endregion // ColumnSorted

		#region ActiveCellChanging

		/// <summary>
		/// The <see cref="ActiveCellChanging"/> event is raised before the currently <see cref="XamGrid.ActiveCell"/> is changed. 
		/// </summary>
		/// <remarks>
		/// This event is Cancellable.
		/// </remarks>
		public event EventHandler<ActiveCellChangingEventArgs> ActiveCellChanging;

		/// <summary>
		/// Raised before the currently <see cref="XamGrid.ActiveCell"/> is changed.
		/// </summary>
		/// <param propertyName="e"></param>
		protected internal virtual void OnActiveCellChanging(ActiveCellChangingEventArgs e)
		{
			if (this.ActiveCellChanging != null)
			{
				this.ActiveCellChanging(this, e);
			}
		}

		#endregion // ActiveCellChanging

		#region ActiveCellChanged

		/// <summary>
		/// The <see cref="ActiveCellChanged"/> event is raised after the <see cref="XamGrid.ActiveCell"/> has changed.
		/// </summary>
		public event EventHandler<EventArgs> ActiveCellChanged;

		/// <summary>
		/// Raised after the <see cref="XamGrid.ActiveCell"/> has changed.
		/// </summary>
		protected internal virtual void OnActiveCellChanged()
		{
			if (this.ActiveCellChanged != null)
			{
				this.ActiveCellChanged(this, EventArgs.Empty);
			}
		}

		#endregion // ActiveCellChanged

		#region SelectedCellsCollectionChanged

		/// <summary>
		/// The <see cref="SelectedCellsCollectionChanged"/> event is raised after a <see cref="Cell"/> has been added or removed to the SelectedCells collection.
		/// </summary>
		public event EventHandler<SelectionCollectionChangedEventArgs<SelectedCellsCollection>> SelectedCellsCollectionChanged;

		/// <summary>
		/// Raised after the <see cref="XamGrid"/>'s <see cref="SelectedCellsCollection"/> has changed.
		/// </summary>
		/// <param propertyName="previouslySelectedCells"></param>
		/// <param propertyName="newSelectedCells"></param>
		protected internal virtual void OnSelectedCellsCollectionChanged(SelectedCellsCollection previouslySelectedCells, SelectedCellsCollection newSelectedCells)
		{
			if (this.SelectedCellsCollectionChanged != null)
			{
				bool difference = false;
				if (previouslySelectedCells.Count != newSelectedCells.Count)
					difference = true;
				else
				{
					foreach (Cell cell in previouslySelectedCells)
					{
						if (!newSelectedCells.Contains(cell))
						{
							difference = true;
							break;
						}
					}
				}
				if (difference)
					this.SelectedCellsCollectionChanged(this, new SelectionCollectionChangedEventArgs<SelectedCellsCollection>() { NewSelectedItems = newSelectedCells, PreviouslySelectedItems = previouslySelectedCells });
			}
		}

		#endregion // SelectedCellsCollectionChanged

		#region SelectedRowsCollectionChanged

		/// <summary>
		/// The <see cref="SelectedRowsCollectionChanged"/> event is raised after a <see cref="Row"/> has been added or removed to the SelectedRows collection.
		/// </summary>
		public event EventHandler<SelectionCollectionChangedEventArgs<SelectedRowsCollection>> SelectedRowsCollectionChanged;

		/// <summary>
		/// Raised after the <see cref="XamGrid"/>'s <see cref="SelectedRowsCollection"/> has changed.
		/// </summary>
		/// <param propertyName="previouslySelectedRows"></param>
		/// <param propertyName="newSelectedRows"></param>
		protected internal virtual void OnSelectedRowsCollectionChanged(SelectedRowsCollection previouslySelectedRows, SelectedRowsCollection newSelectedRows)
		{
			if (this.SelectedRowsCollectionChanged != null)
			{
				bool difference = false;
				if (previouslySelectedRows.Count != newSelectedRows.Count)
					difference = true;
				else
				{
					foreach (Row row in previouslySelectedRows)
					{
						if (!newSelectedRows.Contains(row))
						{
							difference = true;
							break;
						}
					}
				}
				if (difference)
					this.SelectedRowsCollectionChanged(this, new SelectionCollectionChangedEventArgs<SelectedRowsCollection>() { NewSelectedItems = newSelectedRows, PreviouslySelectedItems = previouslySelectedRows });
			}
		}

		#endregion // SelectedRowsCollectionChanged

		#region SelectedColumnsCollectionChanged

		/// <summary>
		/// The <see cref="SelectedColumnsCollectionChanged"/> event is raised after a <see cref="Column"/> has been added or removed to the SelectedColumns collection.
		/// </summary>
		public event EventHandler<SelectionCollectionChangedEventArgs<SelectedColumnsCollection>> SelectedColumnsCollectionChanged;

		/// <summary>
		/// Raised after the <see cref="XamGrid"/>'s <see cref="SelectedColumnsCollection"/> has changed.
		/// </summary>
		/// <param propertyName="previouslySelectedColumns"></param>
		/// <param propertyName="newSelectedColumns"></param>
		protected internal virtual void OnSelectedColumnsCollectionChanged(SelectedColumnsCollection previouslySelectedColumns, SelectedColumnsCollection newSelectedColumns)
		{
			if (this.SelectedColumnsCollectionChanged != null)
			{
				bool difference = false;
				if (previouslySelectedColumns.Count != newSelectedColumns.Count)
					difference = true;
				else
				{
					foreach (Column Column in previouslySelectedColumns)
					{
						if (!newSelectedColumns.Contains(Column))
						{
							difference = true;
							break;
						}
					}
				}
				if (difference)
					this.SelectedColumnsCollectionChanged(this, new SelectionCollectionChangedEventArgs<SelectedColumnsCollection>() { NewSelectedItems = newSelectedColumns, PreviouslySelectedItems = previouslySelectedColumns });
			}
		}

		#endregion // SelectedColumnsCollectionChanged

		#region CellControlAttached

		/// <summary>
		/// This event is raised when a <see cref="Cell"/> comes into the viewport of the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks>
		/// This event is useful for updating the style of a cell conditionally. 
		/// </remarks>
		public event EventHandler<CellControlAttachedEventArgs> CellControlAttached;

		/// <summary>
		/// Raised when a Cell comes into the Viewport of the <see cref="XamGrid"/>
		/// </summary>
		/// <param propertyName="cell">The cell that has come into view.</param>
		/// <returns>True if the control attached to the cell should be disposed of when it leaves the viewport.</returns>
		protected internal virtual bool OnCellControlAttached(Cell cell)
		{
			if (this.CellControlAttached != null && cell != null && !cell.SuppressCellControlAttached)
			{
				CellControlAttachedEventArgs args = new CellControlAttachedEventArgs() { Cell = cell };
				this.CellControlAttached(this, args);
				return args.IsDirty;
			}
			return false;
		}

		#endregion // CellControlAttached

		#region InitializeRow

		/// <summary>
		/// This event is raised when a <see cref="Row"/> is created.
		/// </summary>
		public event EventHandler<InitializeRowEventArgs> InitializeRow;

		/// <summary>
		/// Raised when a Row is created.
		/// </summary>
		/// <param propertyName="row">The row that was just created.</param>
		protected internal virtual void OnInitializeRow(Row row)
		{
			if (this.InitializeRow != null)
				this.InitializeRow(this, new InitializeRowEventArgs() { Row = row });
		}

		#endregion // CellControlAttached

		#region RowSelectorClicked

		/// <summary>
		/// This event is raised when a RowSelector is clicked.
		/// </summary>
		public event EventHandler<RowSelectorClickedEventArgs> RowSelectorClicked;

		/// <summary>
		/// Raised when a RowSelector is clicked.
		/// </summary>
		/// <param propertyName="row">The row whose rowselector was just clicked.</param>
		protected internal virtual void OnRowSelectorClicked(RowBase row)
		{
			if (this.RowSelectorClicked != null)
				this.RowSelectorClicked(this, new RowSelectorClickedEventArgs() { Row = row });
		}

		#endregion // RowSelectorClicked

		#region CellClicked

		/// <summary>
		/// This event is raised when a <see cref="Cell"/> is clicked.
		/// </summary>
		public event EventHandler<CellClickedEventArgs> CellClicked;

		/// <summary>
		/// Raised when a <see cref="Cell"/> is clicked.
		/// </summary>
		/// <param propertyName="cell">The cell that was clicked.</param>
		protected internal virtual void OnCellClicked(Cell cell)
		{
			if (this.CellClicked != null)
				this.CellClicked(this, new CellClickedEventArgs() { Cell = cell });
		}

		#endregion // CellClicked

		#region CellDoubleClicked

		/// <summary>
		/// This event is raised when a <see cref="Cell"/> is double clicked.
		/// </summary>
		public event EventHandler<CellClickedEventArgs> CellDoubleClicked;

		/// <summary>
		/// Raised when a <see cref="Cell"/> is double clicked.
		/// </summary>
		/// <param propertyName="cell">The cell that was double clicked.</param>
		protected internal virtual void OnCellDoubleClicked(Cell cell)
		{
			if (this.CellDoubleClicked != null)
				this.CellDoubleClicked(this, new CellClickedEventArgs() { Cell = cell });
		}

		#endregion // CellDoubleClicked

		#region ColumnResizing
		/// <summary>
		/// Event raised when a user initiated column resizing is begun.
		/// </summary>
		public event EventHandler<CancellableColumnResizingEventArgs> ColumnResizing;

		/// <summary>
		/// Raised when a user driven resizing is begun.
		/// </summary>
		/// <param propertyName="columns"></param>
		/// <param propertyName="newWidth"></param>
		/// <returns></returns>
		protected internal virtual bool OnColumnResizing(Collection<Column> columns, double newWidth)
		{
			if (this.ColumnResizing != null)
			{
				CancellableColumnResizingEventArgs args = new CancellableColumnResizingEventArgs() { Columns = columns, Width = newWidth };

				this.ColumnResizing(this, args);

				return args.Cancel;
			}
			return false;
		}
		#endregion // ColumnResizing

		#region ColumnResized
		/// <summary>
		/// Event raised when a user driven resizing is completed.
		/// </summary>
		public event EventHandler<ColumnResizedEventArgs> ColumnResized;

		/// <summary>
		/// Raised when a resizing is completed.
		/// </summary>
		/// <param propertyName="columns"></param>
		protected internal virtual void OnColumnResized(Collection<Column> columns)
		{
			if (this.ColumnResized != null)
			{
				List<Column> cols = new List<Column>(columns);

				this.ColumnResized(this, new ColumnResizedEventArgs() { Columns = new ReadOnlyCollection<Column>(cols) });
			}
		}

		#endregion // ColumnResized

		#region ColumnFixedStateChanging

		/// <summary>
		/// This event is raised when a <see cref="Column"/>'s fixed state is changing.
		/// </summary>
		public event EventHandler<CancellableColumnFixedStateEventArgs> ColumnFixedStateChanging;

		/// <summary>
		/// Raises the <see cref="ColumnFixedStateChanging"/> event.
		/// </summary>
		/// <param propertyName="column">The column whose fixed state is changing.</param>
		/// /// <param propertyName="newState">The state that the column is changing to.</param>
		protected internal virtual bool OnColumnFixedStateChanging(Column column, FixedState newState)
		{
			if (this.ColumnFixedStateChanging != null)
			{
				CancellableColumnFixedStateEventArgs args = new CancellableColumnFixedStateEventArgs() { Column = column, FixedState = newState };
				this.ColumnFixedStateChanging(this, args);
				return args.Cancel;
			}
			return false;
		}

		#endregion // ColumnFixedStateChanging

		#region ColumnFixedStateChanged

		/// <summary>
		/// This event is raised when a <see cref="Column"/>'s fixed state has changed.
		/// </summary>
		public event EventHandler<ColumnFixedStateEventArgs> ColumnFixedStateChanged;

		/// <summary>
		/// Raises the <see cref="ColumnFixedStateChanged"/> event.
		/// </summary>
		/// <param propertyName="column">The column whose fixed state has changed.</param>
		/// /// <param propertyName="previous">The state that the column previously was.</param>
		protected internal virtual void OnColumnFixedStateChanged(Column column, FixedState previous)
		{
			if (this.ColumnFixedStateChanged != null)
				this.ColumnFixedStateChanged(this, new ColumnFixedStateEventArgs() { Column = column, PreviousFixedState = previous });
		}

		#endregion // ColumnFixedStateChanged

		#region ColumnLayoutAssigned

		/// <summary>
		/// This event is raised when a <see cref="ColumnLayout"/> is assigned to an Island of rows. 
		/// </summary>
		/// <remarks>
		/// A developer can change the ColumnLayout being used by assigning a different ColumnLayout to the <see cref="ColumnLayoutAssignedEventArgs.ColumnLayout"/> property
		/// </remarks>
		public event EventHandler<ColumnLayoutAssignedEventArgs> ColumnLayoutAssigned;

		/// <summary>
		/// Raises the <see cref="ColumnLayoutAssigned"/> event.
		/// </summary>
		///<param propertyName="e"></param>
		protected internal virtual void OnColumnLayoutAssigned(ColumnLayoutAssignedEventArgs e)
		{
			if (this.ColumnLayoutAssigned != null)
				this.ColumnLayoutAssigned(this, e);
		}

		#endregion // ColumnLayoutAssigned

		#region RowExpansionChanging

		/// <summary>
		/// This event is raised when a row is expanding or collapsing.
		/// </summary>
		public event EventHandler<CancellableRowExpansionChangedEventArgs> RowExpansionChanging;

		/// <summary>
		/// Raises the <see cref="RowExpansionChanging"/> event.
		/// </summary>
		/// <param propertyName="row">The row that is being expanded or collapsed</param>
		protected internal virtual bool OnRowExpansionChanging(ExpandableRowBase row)
		{
			if (this.RowExpansionChanging != null)
			{
				CancellableRowExpansionChangedEventArgs args = new CancellableRowExpansionChangedEventArgs() { Row = row };
				this.RowExpansionChanging(this, args);
				return args.Cancel;
			}
			return false;
		}

		#endregion // RowExpansionChanging

		#region RowExpansionChanged

		/// <summary>
		/// This event is raised when a row is expanded or collapsed.
		/// </summary>
		public event EventHandler<RowExpansionChangedEventArgs> RowExpansionChanged;

		/// <summary>
		/// Raises the <see cref="RowExpansionChanged"/> event.
		/// </summary>
		/// <param propertyName="row">The row that was just expanded or collapsed</param>
		protected internal virtual void OnRowExpansionChanged(ExpandableRowBase row)
		{
			if (this.RowExpansionChanged != null)
				this.RowExpansionChanged(this, new RowExpansionChangedEventArgs() { Row = row });
		}

		#endregion // RowExpansionChanged

		#region ColumnDragStart

		/// <summary>
		/// This event is raised when a column drag operation is about to begin.
		/// </summary>
		public event EventHandler<ColumnDragStartEventArgs> ColumnDragStart;

		/// <summary>
		/// Raises the <see cref="ColumnDragStart"/> event.
		/// </summary>
		/// <param propertyName="column"></param>
		protected internal virtual bool OnColumnDragStart(Column column)
		{
			if (this.ColumnDragStart != null)
			{
				ColumnDragStartEventArgs e = new ColumnDragStartEventArgs() { Column = column };
				this.ColumnDragStart(this, e);
				return e.Cancel;
			}
			return false;
		}

		#endregion // ColumnDragStart

		#region ColumnMoving

		/// <summary>
		/// This event is raised when the mouse moves while dragging a <see cref="Column"/>.
		/// </summary>
		public event EventHandler<ColumnMovingEventArgs> ColumnMoving;

		/// <summary>
		/// Raises the <see cref="ColumnMoving"/> event.
		/// </summary>
		/// <param propertyName="column"></param>
		/// <param propertyName="header"></param>
		/// <param propertyName="args"></param>
		protected internal virtual void OnColumnMoving(Column column, HeaderCellControl header, MouseEventArgs args)
		{
			if (this.ColumnMoving != null)
				this.ColumnMoving(this, new ColumnMovingEventArgs() { Column = column, DraggingHeader = header, MouseArgs = args });
		}

		#endregion // ColumnMoving

		#region ColumnDropped

		/// <summary>
		/// This event is raised when a successful drag operation occurs while moving a <see cref="Column"/>.
		/// </summary>
		public event EventHandler<ColumnDroppedEventArgs> ColumnDropped;

		/// <summary>
		/// Raises the <see cref="ColumnDropped"/> event.
		/// </summary>
		/// <param propertyName="column"></param>
		/// <param propertyName="type"></param>
		/// <param propertyName="newIndex"></param>
		/// <param propertyName="previousIndex"></param>
		protected internal virtual void OnColumnDropped(Column column, DropOperationType type, int newIndex, int previousIndex)
		{
			if (this.ColumnDropped != null)
				this.ColumnDropped(this, new ColumnDroppedEventArgs() { Column = column, DropType = type, NewIndex = newIndex, PreviousIndex = previousIndex });
		}

		#endregion // ColumnDropped

		#region ColumnDragCanceled

		/// <summary>
		/// This event is raised when a drag operation is canceled.
		/// </summary>
		public event EventHandler<ColumnDragCanceledEventArgs> ColumnDragCanceled;

		/// <summary>
		/// Raises the <see cref="ColumnDragCanceled"/> event.
		/// </summary>
		/// <param propertyName="column"></param>
		/// <param propertyName="type"></param>
		protected internal virtual void OnColumnDragCanceled(Column column, DragCancelType type)
		{
			if (this.ColumnDragCanceled != null)
				this.ColumnDragCanceled(this, new ColumnDragCanceledEventArgs() { Column = column, CancelType = type });
		}

		#endregion // ColumnDragCanceled

		#region ColumnDragEnded

		/// <summary>
		/// This event is raised when a drag operation has completed, whether it was successful or canceled.
		/// </summary>
		public event EventHandler<ColumnDragEndedEventArgs> ColumnDragEnded;

		/// <summary>
		/// Raises the <see cref="ColumnDragEnded"/> event.
		/// </summary>
		/// <param propertyName="column"></param>
		protected internal virtual void OnColumnDragEnded(Column column)
		{
			if (this.ColumnDragEnded != null)
				this.ColumnDragEnded(this, new ColumnDragEndedEventArgs() { Column = column });
		}

		#endregion // ColumnDragEnded

		#region CellEnteringEditMode

		/// <summary>
		/// This event is raised beore a <see cref="Cell"/> enters edit mode.
		/// </summary>
		public event EventHandler<BeginEditingCellEventArgs> CellEnteringEditMode;

		/// <summary>
		/// Raises the <see cref="CellEnteringEditMode"/> event.
		/// </summary>
		/// <param propertyName="cell"></param>
		protected internal virtual bool OnCellEnteringEditMode(Cell cell)
		{
			if (this.CellEnteringEditMode != null)
			{
				BeginEditingCellEventArgs eventArgs = new BeginEditingCellEventArgs() { Cell = cell };
				this.CellEnteringEditMode(this, eventArgs);
				return eventArgs.Cancel;
			}
			return false;
		}
		#endregion // CellEnteringEditMode

		#region CellEnteredEditMode

		/// <summary>
		/// This event is raised after a <see cref="Cell"/> has entered edit mode.
		/// </summary>
		public event EventHandler<EditingCellEventArgs> CellEnteredEditMode;

		/// <summary>
		/// Raises the <see cref="CellEnteredEditMode"/> event.
		/// </summary>
		/// <param propertyName="cell"></param>
		/// <param propertyName="editor"></param>
		protected internal virtual void OnCellEnteredEditMode(Cell cell, FrameworkElement editor)
		{
			if (this.CellEnteredEditMode != null)
			{
				this.CellEnteredEditMode(this, new EditingCellEventArgs() { Cell = cell, Editor = editor });
			}
		}
		#endregion // CellEnteredEditMode

		#region CellExitingEditMode

		/// <summary>
		/// This event is raised before a cell has exited edit mode.
		/// </summary>
		public event EventHandler<ExitEditingCellEventArgs> CellExitingEditMode;

		/// <summary>
		/// Raises the <see cref="CellExitingEditMode"/> event.
		/// </summary>
		/// <param propertyName="e"></param>
		protected internal virtual void OnCellExitingEditMode(ExitEditingCellEventArgs e)
		{
			if (this.CellExitingEditMode != null)
			{
				this.CellExitingEditMode(this, e);
			}
		}
		#endregion // CellExitingEditMode

		#region PopulatingColumnFilters

		/// <summary>
		/// Event raised when the filter drop down is being populated.
		/// </summary>
		public event EventHandler<PopulatingFiltersEventArgs> PopulatingColumnFilters;

		/// <summary>
		/// Raises the PopulatingColumnFilters event.
		/// </summary>
		/// <param propertyName="args"></param>
		protected internal virtual void OnShowAvailableColumnFilterOperands(PopulatingFiltersEventArgs args)
		{
			if (this.PopulatingColumnFilters != null)
			{
				this.PopulatingColumnFilters(this, args);
			}
		}

		#endregion // PopulatingColumnFilters

		#region CellExitedEditMode

		/// <summary>
		/// This event is raised after a <see cref="Cell"/> has exited edit mode.
		/// </summary>
		public event EventHandler<CellExitedEditingEventArgs> CellExitedEditMode;

		/// <summary>
		/// Raises the <see cref="CellExitedEditMode"/> event.
		/// </summary>
		/// <param propertyName="cell"></param>
		protected internal virtual void OnExitedEditMode(Cell cell)
		{
			if (this.CellExitedEditMode != null)
			{
				this.CellExitedEditMode(this, new CellExitedEditingEventArgs() { Cell = cell });
			}
		}
		#endregion // CellExitedEditMode

		#region RowEnteringEditMode

		/// <summary>
		/// This event is raised beore a <see cref="Row"/> enters edit mode.
		/// </summary>
		public event EventHandler<BeginEditingRowEventArgs> RowEnteringEditMode;

		/// <summary>
		/// Raises the <see cref="RowEnteringEditMode"/> event.
		/// </summary>
		/// <param propertyName="row"></param>
		protected internal virtual bool OnRowEnteringEditMode(Row row)
		{
			if (this.RowEnteringEditMode != null)
			{
				BeginEditingRowEventArgs eventArgs = new BeginEditingRowEventArgs() { Row = row };
				this.RowEnteringEditMode(this, eventArgs);
				return eventArgs.Cancel;
			}
			return false;
		}
		#endregion // RowEnteringEditMode

		#region RowEnteredEditMode

		/// <summary>
		/// This event is raised after a <see cref="Row"/> has entered edit mode.
		/// </summary>
		public event EventHandler<EditingRowEventArgs> RowEnteredEditMode;

		/// <summary>
		/// Raises the <see cref="RowEnteredEditMode"/> event.
		/// </summary>
		/// <param propertyName="row"></param>
		protected internal virtual void OnRowEnteredEditMode(Row row)
		{
			if (this.RowEnteredEditMode != null)
			{
				this.RowEnteredEditMode(this, new EditingRowEventArgs() { Row = row });
			}
		}
		#endregion // RowEnteredEditMode

		#region RowExitingEditMode

		/// <summary>
		/// This event is raised before a cell has exited edit mode.
		/// </summary>
		public event EventHandler<ExitEditingRowEventArgs> RowExitingEditMode;

		/// <summary>
		/// Raises the <see cref="RowExitingEditMode"/> event.
		/// </summary>
		/// <param propertyName="row"></param>
		/// <param propertyName="originalCellValues"></param>
		/// <param propertyName="newCellValues"></param>
		/// <param propertyName="cancel"></param>
		protected internal virtual bool OnRowExitingEditMode(Row row, Dictionary<string, object> originalCellValues, Dictionary<string, object> newCellValues, bool cancel)
		{
			if (this.RowExitingEditMode != null)
			{
				ExitEditingRowEventArgs eventArgs = new ExitEditingRowEventArgs() { Row = row, EditingCanceled = cancel, NewCellValues = newCellValues, OriginalCellValues = originalCellValues };
				this.RowExitingEditMode(this, eventArgs);
				return eventArgs.Cancel;
			}
			return false;
		}
		#endregion // RowExitingEditMode

		#region RowExitedEditMode

		/// <summary>
		/// This event is raised after a <see cref="Row"/> has exited edit mode.
		/// </summary>
		public event EventHandler<EditingRowEventArgs> RowExitedEditMode;

		/// <summary>
		/// Raises the <see cref="RowExitedEditMode"/> event.
		/// </summary>
		/// <param propertyName="row"></param>
		protected internal virtual void OnRowExitedEditMode(Row row)
		{
			if (this.RowExitedEditMode != null)
			{
				this.RowExitedEditMode(this, new EditingRowEventArgs() { Row = row });
			}
		}
		#endregion // RowExitedEditMode

		#region RowAdding
		/// <summary>
		/// This event is raised prior to a <see cref="Row"/> being added or inserted into the <see cref="XamGrid"/>.
		/// </summary>
		public event EventHandler<CancellableRowAddingEventArgs> RowAdding;

		/// <summary>
		/// Raises the <see cref="RowAdding"/> event.
		/// </summary>
		/// <param propertyName="row"></param>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		protected internal virtual bool OnRowAdding(Row row, int index)
		{
			if (this.RowAdding != null)
			{
				CancellableRowAddingEventArgs args = new CancellableRowAddingEventArgs() { Row = row, InsertionIndex = index };
				this.RowAdding(this, args);
				return args.Cancel;
			}

			return false;
		}
		#endregion

		#region RowAdded
		/// <summary>
		/// This event is raised after a <see cref="Row"/> is added or inserted into the <see cref="XamGrid"/>.
		/// </summary>
		public event EventHandler<RowEventArgs> RowAdded;

		/// <summary>
		/// Raises the <see cref="RowAdded"/> event.
		/// </summary>
		/// <param propertyName="row"></param>
		protected internal virtual void OnRowAdded(Row row)
		{
			if (this.RowAdded != null)
			{
				RowEventArgs args = new RowEventArgs() { Row = row };
				this.RowAdded(this, args);
			}
		}
		#endregion

		#region RowDeleting
		/// <summary>
		/// Event raised when a <see cref="Row"/> object is being deleted.
		/// </summary>
		public event EventHandler<CancellableRowEventArgs> RowDeleting;

		/// <summary>
		/// Raises the <see cref="RowDeleting"/> event.
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected internal virtual bool OnRowDeleting(Row row)
		{
			if (this.RowDeleting != null)
			{
				CancellableRowEventArgs args = new CancellableRowEventArgs() { Row = row };
				this.RowDeleting(this, args);
				return args.Cancel;
			}

			return false;
		}

		#endregion // RowDeleting

		#region RowDeleted
		/// <summary>
		/// Event raised after a <see cref="Row"/> object is deleted.
		/// </summary>
		public event EventHandler<RowEventArgs> RowDeleted;

		/// <summary>
		/// Raises the <see cref="RowDeleted"/> event.
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected internal virtual void OnRowDeleted(Row row)
		{
			if (this.RowDeleted != null)
			{
				RowEventArgs args = new RowEventArgs() { Row = row };
				this.RowDeleted(this, args);
			}
		}
		#endregion // RowDeleted

		#region GroupByCollectionChanged

		/// <summary>
        /// The <see cref="GroupByCollectionChanged"/> event is raised after a <see cref="Column"/> has been grouped, ungrouped, or rearranged in the GroupByColumnsCollection.
		/// </summary>
		public event EventHandler<GroupByCollectionChangedEventArgs> GroupByCollectionChanged;

		/// <summary>
		/// Raised after the <see cref="XamGrid"/>'s <see cref="GroupByColumnsCollection"/> has changed.
		/// </summary>
		/// <param propertyName="previousGroupedColumns"></param>
		/// <param propertyName="newGroupedColumns"></param>
		protected internal virtual void OnGroupByCollectionChanged(IList<Column> previousGroupedColumns, IList<Column> newGroupedColumns)
		{
			if (this.GroupByCollectionChanged != null)
			{
				bool difference = false;
				if (previousGroupedColumns.Count != newGroupedColumns.Count)
					difference = true;
				else
				{
					foreach (Column column in previousGroupedColumns)
					{
						if (!newGroupedColumns.Contains(column))
						{
							difference = true;
							break;
						}
					}
				}

				if (difference)
					this.GroupByCollectionChanged(this, new GroupByCollectionChangedEventArgs() { NewGroupedColumns = new ReadOnlyCollection<Column>(newGroupedColumns), PreviousGroupedColumns = new ReadOnlyCollection<Column>(previousGroupedColumns) });
			}
		}

		#endregion // GroupByCollectionChanged

		#region CellEditingValidationFailed

		/// <summary>
		/// This event is raised when validation fails while editing a <see cref="Cell"/>.
		/// </summary>
		public event EventHandler<CellValidationErrorEventArgs> CellEditingValidationFailed;

		/// <summary>
		/// Raises the <see cref="CellEditingValidationFailed"/> event.
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="args"></param>
        /// <remarks>When the <see cref="XamGrid"/> is exiting edit mode, the <see cref="XamGrid"/> will attach to the BindingValidationError and forward the error, if raised, through this event.
        /// However it will not raise this event if secondary errors are raised on the data object via the <see cref="INotifyDataErrorInfo"/> interface.
        /// </remarks>
		protected internal virtual bool OnCellEditingValidationFailed(Cell cell, ValidationErrorEventArgs args)
		{
			if (this.CellEditingValidationFailed != null)
			{
				CellValidationErrorEventArgs cve = new CellValidationErrorEventArgs() { ValidationErrorEventArgs = args, Cell = cell };
				this.CellEditingValidationFailed(this, cve);
				return cve.Handled;
			}

			return false;
		}

		#endregion // CellEditingValidationFailed

		#region DataObjectRequested

		/// <summary>
		/// Event raised when the <see cref="XamGrid"/> is requesting a new dataobject.
		/// </summary>
		public event EventHandler<DataObjectCreationEventArgs> DataObjectRequested;

		/// <summary>
		/// Raises the DataObjectRequested event.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="columnLayout"></param>
		/// <param name="parentRow"></param>
        /// <param name="typeOfRowPopulated"></param>
        protected internal virtual void OnDataObjectRequested(HandleableObjectGenerationEventArgs args, ColumnLayout columnLayout, Row parentRow, RowType? typeOfRowPopulated)
        {
            DataObjectCreationEventArgs newArgs = new DataObjectCreationEventArgs();
            newArgs.ObjectType = args.ObjectType;
            newArgs.ColumnLayout = columnLayout;
            newArgs.ParentRow = parentRow;
            newArgs.CollectionType = args.CollectionType;
            newArgs.RowTypeCreated = typeOfRowPopulated;

			if (this.DataObjectRequested != null)
			{
				this.DataObjectRequested(this, newArgs);
			}

			args.NewObject = newArgs.NewObject;
			if (args.NewObject != null)
				args.Handled = true;
		}

		#endregion // DataObjectRequested

		#region Filtering
		/// <summary>
		/// Event raised when the <see cref="XamGrid"/> is filtering data.
		/// </summary>
		public event EventHandler<CancellableFilteringEventArgs> Filtering;

		/// <summary>
        /// Raises the <see cref="Filtering"/> event.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="operand"></param>
		/// <param name="filters"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		protected internal virtual bool OnFiltering(Column column, FilterOperand operand, RowFiltersCollection filters, object newValue)
		{
			if (this.Filtering != null)
			{
				CancellableFilteringEventArgs args = new CancellableFilteringEventArgs();
				args.Column = column;				
				args.FilteringOperand = operand;
				args.RowFiltersCollection = filters;
				args.FilterValue = newValue;

				this.Filtering(this, args);

				return args.Cancel;
			}
			return false;
		}
		#endregion // Filtering

		#region Filtered

		/// <summary>
		/// Event raised after the grid is filtered.
		/// </summary>
		public event EventHandler<FilteredEventArgs> Filtered;

		/// <summary>
        /// Raises the <see cref="Filtered"/> event.
		/// </summary>
		/// <param name="filters"></param>
		protected internal virtual void OnFiltered(RowFiltersCollection filters)
		{
			if (this.Filtered != null)
			{
				this.Filtered(this, new FilteredEventArgs() { RowFiltersCollection = filters });
			}
		}

		#endregion // Filtered

		#region DataResolution

		/// <summary>
		/// Raises the <see cref="DataResolution"/> event.
		/// </summary>
		/// <param name="e"></param>
		protected internal virtual void OnDataResolution(DataLimitingEventArgs e)
		{
			if (this.DataResolution != null)
				this.DataResolution(this, e);
		}

		/// <summary>
		/// Event raised when the sorting, paging, filtering and groupby is executed.
		/// </summary>
		public event EventHandler<DataLimitingEventArgs> DataResolution;

		#endregion // DataResolution

		#region ClipboardCopying

		/// <summary>
		/// This event is raised prior to selected cells or rows being copied to the clipboard.
		/// </summary>
		public event EventHandler<ClipboardCopyingEventArgs> ClipboardCopying;

		/// <summary>
		/// Raises the <see cref="ClipboardCopying"/> event.
		/// </summary>
		/// <param name="selectedItems"></param>
		/// <param name="clipboardValue"></param>
		/// <returns></returns>
		protected internal virtual bool OnClipboardCopying(ReadOnlyCollection<CellBase> selectedItems, ref string clipboardValue)
		{
			if (this.ClipboardCopying != null)
			{
				ClipboardCopyingEventArgs args = new ClipboardCopyingEventArgs(this) { SelectedItems = selectedItems, ClipboardValue = clipboardValue };
				this.ClipboardCopying(this, args);

				if (args.ClipboardValue != clipboardValue)
				{
					clipboardValue = args.ClipboardValue;
				}

				return args.Cancel;
			}

			return false;
		}

		#endregion ClipboardCopying

		#region ClipboardCopyingItem

		/// <summary>
		/// This event is raised prior to each selected cell being copied to the clipboard.
		/// </summary>
		public event EventHandler<ClipboardCopyingItemEventArgs> ClipboardCopyingItem;

		/// <summary>
		/// Raises the <see cref="ClipboardCopyingItem"/> event.
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="clipboardValue"></param>
		/// <returns></returns>
		protected internal virtual bool OnClipboardCopyingItem(CellBase cell, ref string clipboardValue)
		{
			if (this.ClipboardCopyingItem != null)
			{
				ClipboardCopyingItemEventArgs args = new ClipboardCopyingItemEventArgs() { Cell = cell, ClipboardValue = clipboardValue };
				this.ClipboardCopyingItem(this, args);

				if (args.ClipboardValue != clipboardValue)
				{
					clipboardValue = args.ClipboardValue;
				}

				return args.Cancel;
			}

			return false;
		}

		#endregion ClipboardCopyingItem

		#region ClipboardPasting

		/// <summary>
		/// This event is raised when paste operation is initiated.
		/// </summary>
		public event EventHandler<ClipboardPastingEventArgs> ClipboardPasting;

		/// <summary>
		/// Raises the <see cref="ClipboardPasting"/> event.
		/// </summary>
		/// <param name="parsedValues"></param>
		/// <param name="clipboardValue"></param>  
		protected internal virtual void OnClipboardPasting(List<List<string>> parsedValues, string clipboardValue)
		{
			if (this.ClipboardPasting != null)
			{
				ClipboardPastingEventArgs args = new ClipboardPastingEventArgs(this) { Values = parsedValues, ClipboardValue = clipboardValue };
				this.ClipboardPasting(this, args);
			}
		}

		#endregion ClipboardPasting

        #region ClipboardPasteError

        /// <summary>
        /// This event is raised when paste operation is initiated.
        /// </summary>
        public event EventHandler<ClipboardPasteErrorEventArgs> ClipboardPasteError;

        /// <summary>
        /// Raises the <see cref="ClipboardPasteError"/> event.
        /// </summary>
        /// <param name="errorType">The type of error that trigger the event</param>
        /// <param name="isRecoverable">Whether or not the consumer can decided to continue past the error</param>  
        protected internal virtual bool OnClipboardPasteError(ClipboardPasteErrorType errorType, bool isRecoverable)
        {
            if (this.ClipboardPasteError != null)
            {
                ClipboardPasteErrorEventArgs args = new ClipboardPasteErrorEventArgs() { ErrorType = errorType, IsRecoverable = isRecoverable };
                this.ClipboardPasteError(this, args);
                return args.AttemptRecover;
            }

            //By default if they aren't handling errors, we should just try to keep going
            return true;
        }

        #endregion ClipboardPasteError
        
        #region ColumnAutoGenerated

        /// <summary>
        /// This event is raised when a column has been generated.
        /// </summary>
        public event EventHandler<ColumnAutoGeneratedEventArgs> ColumnAutoGenerated;

        /// <summary>
        /// Raises the <see cref="ColumnAutoGenerated"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Infragistics.Controls.Grids.ColumnAutoGeneratedEventArgs"/> instance containing the event data.</param>
        protected internal virtual void OnColumnAutoGenerated(ColumnAutoGeneratedEventArgs e)
        {
            if (this.ColumnAutoGenerated != null)
            {
                this.ColumnAutoGenerated(this, e);
            }
        }

        #endregion ColumnAutoGenerated

        #region ColumnVisibilityChanged

        /// <summary>
        /// This event is raised when the <see cref="ColumnBase.Visibility"/> property of a <see cref="ColumnBase"/> object is changed.
        /// </summary>
        /// <remarks>
        /// The event will be fired for all objects deriving from <see cref="ColumnBase"/> (<seealso cref="Column"/> and <see cref="ColumnLayout"/> types).
        /// </remarks>
        public event EventHandler<ColumnVisibilityChangedEventArgs> ColumnVisibilityChanged;

        /// <summary>
        /// Raises the <see cref="ColumnVisibilityChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Infragistics.Controls.Grids.ColumnVisibilityChangedEventArgs"/> instance containing the event data.</param>
        protected internal virtual void OnColumnVisibilityChanged(ColumnVisibilityChangedEventArgs e)
        {
            var handler = this.ColumnVisibilityChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

	    #endregion ColumnVisibilityChanged

        #endregion // Events

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        List<string> _propertiesThatShouldntBePersisted;

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
                        "ItemsSource",
						"Rows",
                        "Columns",
                        "ColumnLayouts",
						"ActiveCell"
					};
				}

				return this._propertiesThatShouldntBePersisted;
			}
		}

		List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
		{
			get
			{
				return this.PropertiesToIgnore;
			}
		}

		#endregion // PropertiesToIgnore

		#region PriorityProperties
		List<string> _prioirtyProperties;

		/// <summary>
		/// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
		/// </summary>
		protected virtual List<string> PriorityProperties
		{
			get
			{
				if (this._prioirtyProperties == null)
				{
					this._prioirtyProperties = new List<string>()
					{
						"ColumnLayouts"
					};
				}

				return this._prioirtyProperties;
			}
		}
		List<string> IProvidePropertyPersistenceSettings.PriorityProperties
		{
			get { return this.PriorityProperties; }
		}

		#endregion // PriorityProperties

		#region FinishedLoadingPersistence

		/// <summary>
		/// Allows an object to perform an operation, after it's been loaded.
		/// </summary>
		protected virtual void FinishedLoadingPersistence()
		{
			if (this.VerticalScrollBar != null)
				this.VerticalScrollBar.Value = 0;

			if (this.HorizontalScrollBar != null)
				this.HorizontalScrollBar.Value = 0;

            if (this.RowsManager.ColumnLayout.DataFields == null)
                this.RowsManager.InitData();
		}

		void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
		{
			this.FinishedLoadingPersistence();
		}

		#endregion // FinishedLoadingPersistence

		#endregion

        #region ISupportScrollHelper Members

        ScrollType ISupportScrollHelper.VerticalScrollType
        {
            get { return ScrollType.Item; }
        }

        ScrollType ISupportScrollHelper.HorizontalScrollType
        {
            get { return ScrollType.Item; }
        }

        double ISupportScrollHelper.GetFirstItemHeight()
        {
            if (this._panel != null && this._panel.VisibleRows.Count > 0)
                return this._panel.VisibleRows[0].Control.DesiredSize.Height;
            else
                return 0;
        }

        double ISupportScrollHelper.GetFirstItemWidth()
        {
            return 100; 
        }

        void ISupportScrollHelper.InvalidateScrollLayout()
        {
            this.InvalidateScrollPanel(false);
        }

        double ISupportScrollHelper.VerticalValue
        {
            get
            {
                if (this.VerticalScrollBar != null)
                    return this.VerticalScrollBar.Value;

                return 0;
            }
            set
            {
                if (this.VerticalScrollBar != null)
                    this.VerticalScrollBar.Value = value;
            }
        }

        double ISupportScrollHelper.VerticalMax
        {
            get
            {
                if (this.VerticalScrollBar != null)
                    return this.VerticalScrollBar.Maximum;

                return 0;
            }
        }

        double ISupportScrollHelper.HorizontalValue
        {
            get
            {
                if (this.HorizontalScrollBar != null)
                    return this.HorizontalScrollBar.Value;

                return 0;
            }
            set
            {
                if (this.HorizontalScrollBar != null)
                    this.HorizontalScrollBar.Value = value;
            }
        }

        double ISupportScrollHelper.HorizontalMax
        {
            get
            {
                if (this.HorizontalScrollBar != null)
                    return this.HorizontalScrollBar.Maximum;

                return 0;
            }
        }

        TouchScrollMode ISupportScrollHelper.GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver)
        {
            if (this.IsColumnResizing)
                return TouchScrollMode.None;

            if (this.ColumnChooserDialog.IsOpen)
                return TouchScrollMode.None;

            CellControlBase baseControl = this.GetCellControlFromElement(elementDirectlyOver);

            if (baseControl != null)
            {
                Type type = baseControl.GetType();

                if (type == typeof(CellControl))
                {
                    Column column = baseControl.Column;
                    if (column != null)
                    {
                        if (column.IsMoving)
                        {
                            return TouchScrollMode.None;
                        }

                        if (column.IsFixed != FixedState.NotFixed)
                        {
                            return TouchScrollMode.Vertical;
                        }
                    }
                }
                if (type == typeof(PagerCellControl))
                {
                    return TouchScrollMode.None;
                }
                if (type == typeof(GroupByAreaCellControl))
                {
                    return TouchScrollMode.None;
                }
            }

            if (InUtilityControl(elementDirectlyOver))
            {
                return TouchScrollMode.None;
            }

            return TouchScrollMode.Both;
        }
        
		void ISupportScrollHelper.OnStateChanged(TouchState newState, TouchState oldState)
        {            


#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

        }

        void ISupportScrollHelper.OnPanComplete()
        {
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