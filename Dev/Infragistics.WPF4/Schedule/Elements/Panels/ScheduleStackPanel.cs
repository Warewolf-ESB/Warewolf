using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using Infragistics.Controls.Primitives;
using Infragistics.Collections;
using System.Windows.Media;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom panel that will position a specific # of items to fill the provided space horizontally or vertically based upon the Orientation.
	/// </summary>
	public class ScheduleStackPanel : ScheduleItemsPanel
	{
		#region Member Variables

		private List<FrameworkElement> _elementsInView;
		private int _firstVisibleRealizedIndex;
		private double _columnExtent;
		private ScrollInfo _scrollInfo;
		private bool _releasingElements; // AS 12/13/10 TFS61517
		private double _preferredNonScrollingExtent = double.NaN; // AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
		private Size _lastMeasureSize = Size.Empty; // AS 5/9/12 TFS104555

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		private double _minItemExtent = 0;
		private double _preferredItemExtent = double.NaN;
		private ResizeHelper _resizeHelper;
		private bool _expandToFill = true;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ScheduleStackPanel"/>
		/// </summary>
		public ScheduleStackPanel()
		{
			_elementsInView = new List<FrameworkElement>();
			_scrollInfo = new ElementScrollInfo(this);
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			DebugHelper.DebugLayout(this, false, true, "ArrangeOverride Start", "FinalSize: {0}", finalSize);

			double interItemSpacing = this.InterItemSpacing;

			// AS 5/9/12 TFS104555
			double columnExtent = _columnExtent;
			double lastAvailableExtent = this.IsVertical ? _lastMeasureSize.Height : _lastMeasureSize.Width;

			if (double.IsInfinity(lastAvailableExtent))
			{
				IList<ISupportRecycling> items = this.RecyclableItems;
				int itemCount = items == null ? 0 : items.Count;
				double arrangeExtent = this.IsVertical ? finalSize.Height : finalSize.Width;

				if (itemCount > 0) // AS 5/11/12 TFS111529
					columnExtent = ScheduleUtilities.Max((arrangeExtent + interItemSpacing) / itemCount - interItemSpacing, 0);
			}

			// AS 4/14/11 TFS71761
			int extraPixels = 0;

			// when we are not scrolling and we have layout rounding then the sum of the 
			// extents may be less than the available size so we want to increase the extent 
			// of the first few items such that they are 1 pixel bigger so we use all the 
			// available space
			if (this.UseLayoutRounding && _firstVisibleRealizedIndex == 0 && (double.IsNaN(_preferredItemExtent) || _expandToFill) && !double.IsInfinity(lastAvailableExtent) )
			{
				if (_scrollInfo == null || CoreUtilities.GreaterThanOrClose(_scrollInfo.Viewport, _scrollInfo.Extent))
				{
					IList<ISupportRecycling> items = this.RecyclableItems;
					int itemCount = items == null ? 0 : items.Count;

					// AS 4/20/11 TFS73203
					// Moved the impl into a helper method we could use in the timeslotpanel.
					//
					extraPixels = PanelHelper.CalculateStackExtraPixelCount(finalSize, interItemSpacing, this.IsVertical, itemCount, columnExtent);
				}
			}

			Rect arrangeRect = PanelHelper.ArrangeStack(this, _elementsInView, this.IsVertical, finalSize, columnExtent, interItemSpacing, _firstVisibleRealizedIndex, extraPixels );

			// AS 4/14/11 NA 11.1 - CalendarGroup Sizing/Scrolling
			if (null != _resizeHelper)
				_resizeHelper.Arrange(finalSize);

			DebugHelper.DebugLayout(this, true, false, "ArrangeOverride End", "FinalSize: {0}, ArrangeRect:{1}", finalSize, arrangeRect);

			return new Size(arrangeRect.Width, arrangeRect.Height);
		}
		#endregion //ArrangeOverride

		#region HasLogicalOrientation

		/// <summary>
		/// Returns true to indicate that the panel supports arranging the children in a single orientation.
		/// </summary>
		protected override bool HasLogicalOrientation
		{
			get
			{
				return true;
			}
		}

		#endregion //HasLogicalOrientation

		#region LogicalOrientation

		/// <summary>
		/// Returns the default orientation in which the children are arranged.
		/// </summary>
		protected override Orientation LogicalOrientation
		{
			get
			{
				return this.Orientation;
			}
		}

		#endregion //LogicalOrientation

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			DebugHelper.DebugLayout(this, false, true, "MeasureOverride Start", "AvailableSize: {0}", availableSize);

			_lastMeasureSize = availableSize; // AS 5/9/12 TFS104555

			IList<ISupportRecycling> items = this.RecyclableItems;
			bool isVertical = this.IsVertical;
			int itemCount = items == null ? 0 : items.Count;
			int pageSize = itemCount;
			double availableExtent = isVertical ? availableSize.Height : availableSize.Width;
			double interItemSpacing = this.InterItemSpacing;

			// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			//double scrollPos = this.ScrollInfo.Offset;

			// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			// Cannot calculate this until we have calculated the column extent and updated the page size accordingly.
			//
			//int firstItemIndex = Math.Min(itemCount - pageSize, (int)scrollPos);

			// how wide is each column
			double columnExtent;

			if (double.IsPositiveInfinity(availableExtent))
			{
				// AS 5/9/12 TFS104555
				//columnExtent = 100;
				columnExtent = availableExtent;
			}
			else if (pageSize < 2)
				columnExtent = availableExtent;
			else
				columnExtent = Math.Max((availableExtent + interItemSpacing) / pageSize - interItemSpacing, 0);

			// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			if ( !double.IsNaN(_preferredItemExtent) )
			{
				columnExtent = Math.Min(Math.Max(_preferredItemExtent, _minItemExtent), availableExtent);

				// added the option to ensure there is no dead space
				if (_expandToFill && pageSize > 0 )
				{
					double expectedTotalExtent = PanelHelper.CalculateExtent(itemCount, interItemSpacing, columnExtent);

					// if there would be empty space then increase the extent
					// AS 5/4/11 TFS74447
					// Added infinity check since we don't want to use an infinite column extent.
					//
					if (CoreUtilities.LessThan(expectedTotalExtent, availableExtent) && !double.IsPositiveInfinity(availableExtent))
					{
						columnExtent = Math.Max((availableExtent + interItemSpacing) / pageSize - interItemSpacing, 0);
					}
				}
			}

			// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			if ( _minItemExtent > 0 )
			{
				if ( columnExtent < _minItemExtent && columnExtent < availableExtent )
					columnExtent = _minItemExtent;
			}

			// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			if (!double.IsPositiveInfinity(availableExtent) && ( _minItemExtent > 0 || !double.IsNaN(_preferredItemExtent) ))
			{
				pageSize = CalculatePageSize(itemCount, availableExtent, interItemSpacing, columnExtent);
			}

			
			if (this.UseLayoutRounding)
			{
				columnExtent = Math.Floor(columnExtent);
			}

			// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			// Moved down from above since we need to calculate the page size first.
			//
			int firstItemIndex = CalculateFirstItemIndex(itemCount, pageSize);

			// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
			Size measureSize = availableSize;

			if ( !double.IsNaN(_preferredNonScrollingExtent) )
			{
				if (isVertical)
					measureSize.Width = Math.Min(_preferredNonScrollingExtent, availableSize.Width);
				else
					measureSize.Height = Math.Min(_preferredNonScrollingExtent, availableSize.Height);
			}

			int startItemIndex, endItemIndex;
			// AS 12/13/10 TFS61517
			//Size desired = PanelHelper.RealizeStack(measureSize, firstItemIndex, pageSize, this, items, this.IsVertical, 
			//    this._elementsInView, columnExtent, false, interItemSpacing, this.InitializeItem, 
			//    ref _firstVisibleRealizedIndex, out startItemIndex, out endItemIndex);
			bool wasReleasing = _releasingElements;
			Size desired;
			int extraPixels; // AS 1/13/12 TFS74252

			try
			{
				_releasingElements = true;

				// AS 12/16/10 TFS61823
				bool? forceKeepExisting = null;
				
				if (_resizeHelper != null && _resizeHelper.IsResizing)
					forceKeepExisting = true;

				desired = PanelHelper.RealizeStack(availableSize, firstItemIndex, pageSize, this, items, this.IsVertical,
					this._elementsInView, ref columnExtent, false, interItemSpacing, this.InitializeItem,
					ref _firstVisibleRealizedIndex, out startItemIndex, out endItemIndex
					, this.UseLayoutRounding, out extraPixels // AS 1/13/12 TFS74252
					, forceKeepExisting  // AS 12/16/10 TFS61823
					);

				// AS 5/9/12 TFS104555
				if (double.IsPositiveInfinity(availableExtent) && _minItemExtent > 0 && columnExtent < _minItemExtent)
					columnExtent = _minItemExtent;
			}
			finally
			{
				_releasingElements = wasReleasing;
			}

			_columnExtent = columnExtent;

			// AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			if ( null != _resizeHelper )
				_resizeHelper.Measure(availableSize);

			// verify scroll info
			this.ScrollInfo.Initialize(pageSize, itemCount, firstItemIndex);

			double extent = PanelHelper.CalculateExtent(itemCount, interItemSpacing, columnExtent) + extraPixels ;

			if (isVertical)
			{
				desired.Height = Math.Min(extent, availableSize.Height);

				// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
				if ( !double.IsNaN(_preferredNonScrollingExtent) )
					desired.Width = measureSize.Width;
			}
			else
			{
				desired.Width = Math.Min(extent, availableSize.Width);

				// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
				if ( !double.IsNaN(_preferredNonScrollingExtent) )
					desired.Height = measureSize.Height;
			}

			DebugHelper.DebugLayout(this, true, false, "MeasureOverride End", "AvailableSize: {0}, Desired: {1}, ColumnExtent:{2}", availableSize, desired, columnExtent);

			return desired;
		}
		#endregion //MeasureOverride

		#region OnElementAttached
		internal override void OnElementAttached( ISupportRecycling item, FrameworkElement element, bool isNewlyRealized )
		{
			base.OnElementAttached(item, element, isNewlyRealized);
		} 
		#endregion // OnElementAttached

		#region OnElementReleased
		internal override void OnElementReleased( ISupportRecycling item, FrameworkElement element, bool isRemoved )
		{
			// AS 12/13/10 TFS61517
			// If the elements are released outside the measure then we do want to fix up the elements in view.
			//
			//if ( isRemoved )
			if ( !_releasingElements )
			{
				_elementsInView.Remove(element);
			}

			base.OnElementReleased(item, element, isRemoved);
		}
		#endregion // OnElementReleased

		// AS 12/13/10 TFS61517
		// When the list is changed then we need to detach the recyclable objects from 
		// the elements in this panel or else there will be issues when the objects 
		// are used in another panel.
		//
		#region OnItemsChanged
		internal override void OnItemsChanged( IList oldValue, IList newValue )
		{
			base.OnItemsChanged(oldValue, newValue);

			bool wasReleasing = _releasingElements;

			try
			{
				_releasingElements = true;
				PanelHelper.ReleaseElements(this, _elementsInView);
			}
			finally
			{
				_releasingElements = wasReleasing;
			}
		}
		#endregion // OnItemsChanged

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region InterItemSpacing

		/// <summary>
		/// Identifies the <see cref="InterItemSpacing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterItemSpacingProperty = DependencyProperty.Register("InterItemSpacing",
			typeof(double), typeof(ScheduleStackPanel),
			DependencyPropertyUtilities.CreateMetadata(0d, new PropertyChangedCallback(OnInterItemSpacingChanged))
			);

		private static void OnInterItemSpacingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ScheduleStackPanel item = (ScheduleStackPanel)d;
			item.InvalidateMeasureHelper();
		}

		/// <summary>
		/// Returns or sets the amount of space between each item.
		/// </summary>
		/// <seealso cref="InterItemSpacingProperty"/>
		public double InterItemSpacing
		{
			get
			{
				return (double)this.GetValue(ScheduleStackPanel.InterItemSpacingProperty);
			}
			set
			{
				this.SetValue(ScheduleStackPanel.InterItemSpacingProperty, value);
			}
		}

		#endregion //InterItemSpacing

		#endregion // Public Properties

		#region Internal Properties

		// AS 4/15/11 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region ExpandToFill
		internal bool ExpandToFill
		{
			get { return _expandToFill; }
			set
			{
				if (value != _expandToFill)
				{
					_expandToFill = value;
					this.InvalidateMeasureHelper();
				}
			}
		} 
		#endregion //ExpandToFill

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region MinItemExtent
		internal double MinItemExtent
		{
			get { return _minItemExtent; }
			set
			{
				if ( value != _minItemExtent )
				{
					if ( double.IsInfinity(value) || double.IsNaN(value) )
						value = 0;

					_minItemExtent = value;
					this.InvalidateMeasureHelper();
				}
			}
		} 
		#endregion // MinItemExtent

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region PreferredItemExtent
		internal double PreferredItemExtent
		{
			get { return _preferredItemExtent; }
			set
			{
				if ( value != _preferredItemExtent )
				{
					value = Math.Max(0, value);

					if ( double.IsPositiveInfinity(value) )
						value = double.NaN;

					_preferredItemExtent = value;
					this.InvalidateMeasureHelper();
				}
			}
		}
		#endregion // PreferredItemExtent

		// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
		#region PreferredNonScrollingExtent
		internal double PreferredNonScrollingExtent
		{
			get { return _preferredNonScrollingExtent; }
			set
			{
				if ( value != _preferredNonScrollingExtent )
				{
					value = Math.Max(0, value);

					if ( double.IsPositiveInfinity(value) )
						value = double.NaN;

					_preferredNonScrollingExtent = value;
					this.InvalidateMeasureHelper();
				}
			}
		}
		#endregion // PreferredNonScrollingExtent

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region ResizerBarHost
		internal IResizerBarHost ResizerBarHost
		{
			get
			{
				return _resizeHelper == null ? null : _resizeHelper.ResizerHost;
			}
			set
			{
				if ( value != this.ResizerBarHost )
				{
					if ( value == null )
					{
						if ( null != _resizeHelper )
							_resizeHelper.ResizerHost = null;

						_resizeHelper = null;
					}
					else
					{
						if ( _resizeHelper == null )
							_resizeHelper = new ResizeHelper(this);

						_resizeHelper.ResizerHost = value;
					}
				}
			}
		} 
		#endregion // ResizerBarHost

		#region ScrollInfo
		internal ScrollInfo ScrollInfo
		{
			get { return _scrollInfo; }
		}
		#endregion //ScrollInfo

		#endregion // Internal Properties

		#endregion //Properties

		#region Methods

		// AS 4/15/11 NA 11.1 - CalendarGroup Sizing/Scrolling
		// Moved to a helper method so we can reuse this for the resizing.
		//
		#region CalculateFirstItemIndex

		private int CalculateFirstItemIndex(int itemCount, int pageSize)
		{
			// AS 12/14/10 TFS61536
			// While what we were doing was technically correct and consistent with even what MS 
			// is doing in outlook when scrolling groups in the schedule view, it does feel a little 
			// weird especially when there are only 2 groups and 1 and some portion of the second 
			// is in view. To try and make this feel better we'll divide up the range based on 
			// the number of items.
			// 
			//int firstItemIndex = Math.Min(itemCount - pageSize, (int)scrollPos);
			int firstItemIndex = itemCount - pageSize;

			if (firstItemIndex > 0)
				firstItemIndex = Math.Min(firstItemIndex, (int)(this.ScrollInfo.Offset * ((firstItemIndex + 1) / firstItemIndex)));
			else
				firstItemIndex = 0;

			return firstItemIndex;
		}
		#endregion //CalculateFirstItemIndex

		// AS 4/15/11 NA 11.1 - CalendarGroup Sizing/Scrolling
		// Moved to a helper method so it can be reused for resizing.
		//
		#region CalculatePageSize
		internal static int CalculatePageSize(int itemCount, double availableExtent, double interItemSpacing, double columnExtent)
		{
			// Not all items may be in view so we need to calculate the page size but try to 
			// keep at least 1 item in view.
			int pageSize = Math.Max(1, (int)Math.Floor((availableExtent + interItemSpacing) / (columnExtent + interItemSpacing)));
			pageSize = Math.Max(Math.Min(itemCount, pageSize), 0);
			return pageSize;
		}
		#endregion //CalculatePageSize

		#region InitializeItem
		private void InitializeItem(ISupportRecycling item, int index, FrameworkElement container, PanelHelper.InitializeItemState state)
		{
			int lastItem = this.Items.Count - 1;

			ScheduleUtilities.SetBoolTrueProperty(container, ScheduleItemsPanel.IsFirstItemProperty, index == 0);
			ScheduleUtilities.SetBoolTrueProperty(container, ScheduleItemsPanel.IsLastItemProperty, index == lastItem);
		}
		#endregion // InitializeItem

		// AS 5/4/11 TFS74447
		#region InvalidateMeasureHelper
		private void InvalidateMeasureHelper()
		{
			this.InvalidateMeasure();

			UIElement parent = VisualTreeHelper.GetParent(this) as UIElement;

			if (null != parent)
				parent.InvalidateMeasure();
		} 
		#endregion //InvalidateMeasureHelper

		#endregion // Methods

		// AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region ResizeHelper class
		private class ResizeHelper : IMultiResizerBarHost
		{
			#region Member Variables

			private ScheduleStackPanel _panel;
			private IResizerBarHost _resizerHost;
			private List<ScheduleResizerBar> _bars;
			private const double BarExtent = 4;
			private bool _isResizing;

			private Size _lastAvailableSize;
			private double? _deferredResizeItemExtent;
			private int? _indexBeingResized;
			private double? _originalCancelExtent;

			[ThreadStatic]
			private static WeakDictionary<IResizerBarHost, ResizeHostInfo> _helperTable;

			#endregion // Member Variables

			#region Construction
			internal ResizeHelper( ScheduleStackPanel panel )
			{
				_panel = panel;
				_bars = new List<ScheduleResizerBar>();
			}
			#endregion // Construction

			#region Properties

			#region Internal Properties

			#region IsResizing
			internal bool IsResizing
			{
				get { return _isResizing; }
			} 
			#endregion // IsResizing

			#region ResizerHost
			internal IResizerBarHost ResizerHost
			{
				get { return _resizerHost; }
				set
				{
					if ( value != _resizerHost )
					{
						OnChangeResizeHost(this, _resizerHost, value);

						_resizerHost = value;

						foreach ( ScheduleResizerBar bar in _bars )
							bar.Host = value == null ? null : this;

						// add/remove the resizer for the elements in view
						this.SynchronizeBars();
					}
				}
			}
			#endregion // ResizerHost

			#endregion // Internal Properties

			#endregion // Properties

			#region Methods

			#region Internal Methods

			#region Arrange
			internal void Arrange(Size finalSize)
			{
				double interItemSpacing = _panel.InterItemSpacing;
				Rect finalRect = new Rect(0, 0, finalSize.Width, finalSize.Height);

				if (_deferredResizeItemExtent != null)
				{
					int itemCount = _panel.Items.Count;
					bool isVertical = _panel.IsVertical;
					double columnExtent = _deferredResizeItemExtent.Value;

					// we're using the left edge of the first item before the resize as the anchor
					// point for the resizer bars. while this means that the bars won't necessarily 
					// be indicative of where the groups will actually be but will at least mean that 
					// the bars won't move left as you right to the right when you are scrolled over 
					// to the far right edge
					Rect barRect = PanelHelper.GetStartingArrangeRect(isVertical, finalSize, _panel._columnExtent, interItemSpacing, _panel._firstVisibleRealizedIndex, _panel._elementsInView.Count);

					// AS 5/25/11 TFS74447
					// Changed use of % here to use Mod helper function rather than checking for 0 in each place.
					//

					// note because the leading group could be partially out of view and its extent may be greater than 
					// the extent of the column based on where the resizer bar has been dragged we need to adjust the bar 
					// rect based on the extent
					if (isVertical)
					{
						// AS 5/19/11 TFS76130
						barRect.Y = ScheduleUtilities.Mod(barRect.Y , (_panel._columnExtent + interItemSpacing));

						barRect.Height = columnExtent;
						barRect.Y = ScheduleUtilities.Mod(barRect.Y, columnExtent);
					}
					else
					{
						// AS 5/19/11 TFS76130
						// Since the _firstVisibleRealizedIndex could be > 0 the arrange rect provided would 
						// have considered how many previous elements to include and offset back by that. We 
						// need to readjust to get to the first item that would actually be in view.
						//
						barRect.X = ScheduleUtilities.Mod(barRect.X, (_panel._columnExtent + interItemSpacing));

						barRect.Width = columnExtent;
						barRect.X = ScheduleUtilities.Mod(barRect.X, columnExtent);
					}

					for (int i = 0; i < _bars.Count; i++)
					{
						ArrangeBar(_bars[i], false, barRect, finalRect);

						if (isVertical)
							barRect.Y += barRect.Height;
						else
							barRect.X += barRect.Width;
					}
				}
				else
				{
					var elements = _panel._elementsInView;
					Debug.Assert(_bars.Count <= elements.Count);

					for (int i = 0, count = elements.Count; i < count; i++)
					{
						var element = elements[i];

						var bar = _bars[i];
						Rect elementRect = LayoutInformation.GetLayoutSlot(element);

						ArrangeBar(bar, ScheduleItemsPanel.GetIsLastItem(element), elementRect, finalRect);
					}
				}
			}
			#endregion // Arrange

			#region Measure
			internal void Measure( Size availableSize )
			{
				_lastAvailableSize = availableSize;

				this.SynchronizeBars();

				foreach ( var bar in _bars )
				{
					bar.Measure(availableSize);
				}
			}
			#endregion // Measure

			#endregion // Internal Methods

			#region Private Methods

			#region ArrangeBar
			private void ArrangeBar(ScheduleResizerBar bar, bool isLastItem, Rect elementRect, Rect finalRect)
			{
				double offset = isLastItem
					? -BarExtent
					: (_panel.InterItemSpacing / 2) - (BarExtent / 2);

				if (_resizerHost.ResizerBarOrientation == Orientation.Vertical)
				{
					elementRect.X = elementRect.Right + offset;
					elementRect.Width = BarExtent;
				}
				else
				{
					elementRect.Y = elementRect.Bottom + offset;
					elementRect.Height = BarExtent;
				}

				bool isVertical = this._panel.IsVertical;

				// if the bar will be out of view then don't arrange it at the rect since it could be drawn 
				// over some other control depending on the z-order
				if (isVertical && (elementRect.Bottom < finalRect.Top || elementRect.Top > finalRect.Bottom) ||
					!isVertical && (elementRect.Right < finalRect.Left || elementRect.Left > finalRect.Right))
				{
					PanelHelper.ArrangeOutOfView(bar);
				}
				else
				{
					bar.Arrange(elementRect);
				}
			}
			#endregion //ArrangeBar

			#region CalculateBarCount
			private int CalculateBarCount(double columnExtent)
			{
				// AS 5/25/11 TFS74447
				// In case the extent is 0 make sure we don't divide by 0 which would result in us 
				// returning NaN converted to int which returns int.MinValue.
				//
				if (CoreUtilities.AreClose(columnExtent, 0))
					return _panel.Items.Count;

				bool isVertical = _panel.IsVertical;
				double availableExtent = isVertical ? _lastAvailableSize.Height : _lastAvailableSize.Width;

				// since we may be shifting the bars to the left we'll assume 1 extra item width
				availableExtent += columnExtent;

				return (int)Math.Min(_panel.Items.Count, availableExtent / columnExtent);
			}
			#endregion //CalculateBarCount

			#region CreateResizerBar
			private ScheduleResizerBar CreateResizerBar()
			{
				var bar = new ScheduleResizerBar();

				bar.Host = this;
				bar.IsResizingChanged += this.OnBarIsResizingChanged;
				Canvas.SetZIndex(bar, 5);

				// set alignment/width
				if (_resizerHost.ResizerBarOrientation == Orientation.Vertical)
				{
					bar.Width = BarExtent;
					bar.HorizontalAlignment = HorizontalAlignment.Right;
				}
				else
				{
					bar.Height = BarExtent;
					bar.VerticalAlignment = VerticalAlignment.Bottom;
				}

				_bars.Add(bar);
				_panel.Children.Add(bar);

				return bar;
			}
			#endregion //CreateResizerBar

			#region OnBarIsResizingChanged
			private void OnBarIsResizingChanged( object sender, EventArgs e )
			{
				this.VerifyIsResizingState();
			}
			#endregion // OnBarIsResizingChanged

			#region OnChangeResizeHost
			private static void OnChangeResizeHost(ResizeHelper resizeHelper, IResizerBarHost oldHost, IResizerBarHost newHost)
			{
				if (_helperTable == null)
					_helperTable = new WeakDictionary<IResizerBarHost, ResizeHostInfo>(true, false);

				ResizeHostInfo hostInfo;

				if (oldHost != null)
				{
					if (_helperTable.TryGetValue(oldHost, out hostInfo))
						hostInfo.Remove(resizeHelper);
				}

				if (newHost != null)
				{
					if (!_helperTable.TryGetValue(newHost, out hostInfo))
					{
						_helperTable[newHost] = hostInfo = new ResizeHostInfo();
					}

					hostInfo.Add(resizeHelper);
				}
			}
			#endregion //OnChangeResizeHost

			#region OnIsResizingChanged
			private void OnIsResizingChanged()
			{
				if (!_isResizing && _deferredResizeItemExtent != null)
				{
					double itemExtent = _deferredResizeItemExtent.Value;
					int? indexBeingResized = _indexBeingResized;

					_resizerHost.SetExtent(itemExtent);
					this.SetDeferredResizeItemExtent(null, true);

					// verify that the item being resized is kept in view
					var scrollInfoProvider = _resizerHost as IScrollInfoProvider;
					var scrollInfo = null != scrollInfoProvider ? scrollInfoProvider.ScrollInfo : _panel.ScrollInfo;

					if (!double.IsNaN(itemExtent) && indexBeingResized != null && scrollInfo != null)
					{
						int itemCount = _panel.Items.Count;
						bool isVertical = _panel.IsVertical;
						double availableExtent = isVertical ? _lastAvailableSize.Height : _lastAvailableSize.Width;
						double interItemSpacing = _panel.InterItemSpacing;

						double newTotalExtent = PanelHelper.CalculateExtent(itemCount, interItemSpacing, itemExtent);

						// we only need to adjust the offset if not all the items will be in view
						if (newTotalExtent > availableExtent)
						{
							int index = Math.Max(Math.Min(indexBeingResized.Value, itemCount - 1), 0);
							int pageSize = ScheduleStackPanel.CalculatePageSize(itemCount, availableExtent, interItemSpacing, itemExtent);

							int calculatedFirstIndex = _panel.CalculateFirstItemIndex(itemCount, pageSize);

							if (index < calculatedFirstIndex)
							{
								scrollInfo.Initialize(pageSize, itemCount, index);
							}
							else if (index >= calculatedFirstIndex + pageSize)
							{
								scrollInfo.Initialize(pageSize, itemCount, (index - pageSize) + 1);
							}
						}
					}
				}
				else
				{
					this.SynchronizeBars();
				}

				#region Maintain _indexBeingResized
				if (_isResizing)
				{
					Debug.Assert(_bars.Count == _panel._elementsInView.Count, "Should still be sync'd with the panel's items");

					for (int i = 0; i < _bars.Count; i++)
					{
						var bar = _bars[i];

						if (bar.IsResizing)
						{
							var element = _panel._elementsInView[i];







							var item = RecyclingManager.Manager.ItemFromElement(element);
							Debug.Assert(null != item, "Unable to get item from element for bar?");

							if (item != null)
								_indexBeingResized = _panel.Items.IndexOf(item);
						}
					}
				}
				else
				{
					_indexBeingResized = null;
				}
				#endregion //Maintain _indexBeingResized
			}
			#endregion //OnIsResizingChanged 

			#region RemoveResizerBar
			private ScheduleResizerBar RemoveResizerBar(bool allowIsResizing = false)
			{
				int index;

				for (index = _bars.Count - 1; index >= 0; index--)
				{
					if (allowIsResizing || _bars[index].IsResizing == false)
						break;
				}

				if (index < 0)
					return null;

				var bar = _bars[index];
				bool wasResizing = bar.IsResizing;
				bar.IsResizingChanged -= this.OnBarIsResizingChanged;
				bar.Host = null;
				_bars.Remove(bar);
				_panel.Children.Remove(bar);

				if (wasResizing)
					this.VerifyIsResizingState();

				return bar;
			}
			#endregion //RemoveResizerBar

			#region SetDeferredResizeItemExtent
			private void SetDeferredResizeItemExtent(double? newValue, bool notifyHostInfo)
			{
				if (newValue != _deferredResizeItemExtent)
				{
					bool notifyBars = newValue == null || _deferredResizeItemExtent == null;

					_deferredResizeItemExtent = newValue;
					_panel.InvalidateMeasure();
					_panel.InvalidateArrange();

					if (notifyHostInfo)
					{
						ResizeHostInfo hostInfo;

						if (null != _resizerHost && _helperTable.TryGetValue(_resizerHost, out hostInfo))
							hostInfo.DeferredItemExtent = newValue;
					}

					this.SynchronizeBars();

					if (notifyBars)
					{
						foreach (var bar in _bars)
							bar.NotifyResizeModeChanged();
					}
				}
			}
			#endregion //SetDeferredResizeItemExtent

			#region SynchronizeBars
			private void SynchronizeBars()
			{
				int barCount = _bars.Count;
				int barsNeeded;

				if (_deferredResizeItemExtent != null)
				{
					double columnExtent = _deferredResizeItemExtent.Value;

					barsNeeded = CalculateBarCount(columnExtent);
				}
				else
				{
					barsNeeded = _resizerHost == null ? 0 : _panel._elementsInView.Count;
				}

				int diff = barCount - barsNeeded;

				if (diff != 0)
				{
					for (int i = 0; i < diff; i++)
					{
						// remove any unneeded bars - keep the one being resized
						this.RemoveResizerBar();
					}

					for (int i = diff; i < 0; i++)
					{
						this.CreateResizerBar();
					}
				}
			}
			#endregion //SynchronizeBars

			#region VerifyIsResizingState
			private void VerifyIsResizingState()
			{
				bool isResizing = false;

				foreach ( var bar in _bars )
				{
					if ( bar.IsResizing )
						isResizing = true;
				}

				bool reSyncBars = isResizing != _isResizing;

				_isResizing = isResizing;

				if (reSyncBars)
				{
					OnIsResizingChanged();
				}
			}
			#endregion // VerifyIsResizingState

			#endregion // Private Methods

			#endregion // Methods

			#region IResizerBarHost Members
			Orientation IResizerBarHost.ResizerBarOrientation
			{
				get { return _resizerHost == null ? Orientation.Vertical : _resizerHost.ResizerBarOrientation; }
			}

			bool IResizerBarHost.CanResize()
			{
				return _resizerHost != null && _resizerHost.CanResize();
			}

			void IResizerBarHost.SetExtent(double extent)
			{
				if (_isResizing)
				{
					if (double.IsNaN(extent))
					{
						this.SetDeferredResizeItemExtent(null, true);
					}
					else if (double.IsNegativeInfinity(extent))
					{
						this.SetDeferredResizeItemExtent(null, true);

						if (_originalCancelExtent == null)
							return;

						extent = _originalCancelExtent.Value;
					}
					else
					{
						this.SetDeferredResizeItemExtent(extent, true);
						return;
					}
				}

				if (_resizerHost != null)
					_resizerHost.SetExtent(extent);
			}

			ResizeInfo IResizerBarHost.GetResizeInfo()
			{
				if (_resizerHost == null)
					return null;

				var info = _resizerHost.GetResizeInfo();

				double minimum = info.Minimum;

				if (_panel._expandToFill && info.CanIncreaseMinimum )
				{
					int itemCount = _panel.Items.Count;

					if (itemCount > 0)
					{
						bool isVertical = _panel.IsVertical;
						double availableExtent = isVertical ? _lastAvailableSize.Height : _lastAvailableSize.Width;

						if (!double.IsPositiveInfinity(availableExtent))
						{
							double interItemSpacing = _panel.InterItemSpacing;
							double columnExtent = Math.Max((availableExtent + interItemSpacing) / itemCount - interItemSpacing, 0);

							if (CoreUtilities.GreaterThan(columnExtent, minimum))
								minimum = columnExtent;
						}
					}
				}

				_originalCancelExtent = info.CancelExtent;

				// since we are deferring the resize we want to be able to identify the cancel amount
				info = new ResizeInfo(info.ReferenceElement, info.ActualExtent, double.NegativeInfinity, minimum, info.Maximum);

				return info;
			} 
			#endregion //IResizerBarHost Members

			#region IMultiResizerBarHost Members
			ResizeMode? IMultiResizerBarHost.ResizeMode
			{
				get
				{
					if (_deferredResizeItemExtent != null)
						return ResizeMode.Deferred;

					return null;
				}
			}

			double IMultiResizerBarHost.OffsetPerPixel
			{
				get
				{
					for (int i = 0, count = _bars.Count; i < count; i++)
					{
						if (_bars[i].IsResizing)
							return i + 1;
					}

					Debug.Assert(_isResizing, "OffsetPerPixel can only be known once we know which bar is being moved.");
					return 1;
				}
			}
			#endregion //IMultiResizerBarHost Members

			#region ResizeHostInfo class
			private class ResizeHostInfo
			{
				#region Member Variables

				private WeakSet<ResizeHelper> _helperSet;
				private double? _deferredItemExtent;

				#endregion //Member Variables

				#region Constructor
				internal ResizeHostInfo()
				{
					_helperSet = new WeakSet<ResizeHelper>();
				}
				#endregion //Constructor

				#region Properties

				#region DeferredItemExtent
				internal double? DeferredItemExtent
				{
					get { return _deferredItemExtent; }
					set
					{
						if (value != _deferredItemExtent)
						{
							_deferredItemExtent = value;

							foreach (ResizeHelper helper in _helperSet)
								helper.SetDeferredResizeItemExtent(value, false);
						}
					}
				} 
				#endregion //DeferredItemExtent

				#endregion //Properties

				#region Methods

				#region Add
				internal void Add(ResizeHelper resizeHelper)
				{
					if (_helperSet.Add(resizeHelper))
					{
						resizeHelper.SetDeferredResizeItemExtent(_deferredItemExtent, false);
					}
				}
				#endregion //Add

				#region Remove
				internal void Remove(ResizeHelper resizeHelper)
				{
					if (_helperSet.Remove(resizeHelper))
					{
						resizeHelper.SetDeferredResizeItemExtent(null, false);
					}
				}
				#endregion //Remove

				#endregion //Methods
			} 
			#endregion //ResizeHostInfo class
		} 
		#endregion // ResizeHelper class
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