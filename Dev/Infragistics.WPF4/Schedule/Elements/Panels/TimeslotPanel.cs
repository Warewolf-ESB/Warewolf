using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using Infragistics.Controls.Primitives;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom panel used to arrange time slot elements like those in a Day, Week or ScheduleView in Outlook.
	/// </summary>
	[DesignTimeVisible(false)]
	public class TimeslotPanel : TimeslotPanelBase
		, IRecyclableElementHost
	{
		#region Member Variables

		private int _firstVisibleRealizedIndex;
		private List<FrameworkElement> _elementsInView;
		private TimeslotBase _templateItem;
		private NowTimeIndicatorHelper _nowTimeHelper;
		private DateTime? _previousInitializedMajor;
		private bool _releasingElements; // AS 12/13/10 TFS61517
		private Size _lastMeasureSize; // AS 5/11/12 TFS104555

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotPanel"/>
		/// </summary>
		public TimeslotPanel()
		{
			_elementsInView = new List<FrameworkElement>();
		} 
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverrideImpl
		internal override System.Windows.Size ArrangeOverrideImpl(Size finalSize)
		{
			Debug.Assert(_firstVisibleRealizedIndex >= 0 && _firstVisibleRealizedIndex < _elementsInView.Count || (_elementsInView.Count == 0 && _firstVisibleRealizedIndex == 0));

			bool isVertical = this.IsVertical;
			double interItemSpacing = this.InterItemSpacing;

			// AS 4/20/11 TFS73203
			// We need to pass the extra pixels to the arrangement. Note originally I was going to use 
			// the IncreasedItems member which is calculated in the measure but sometimes with layout 
			// rounding we are given a different value for the arrange than the measure and so we 
			// could have an extra pixel left as a gap. To avoid this we'll recalculate the extra 
			// pixel count using the same routine the stack panel uses. This could mean that we could 
			// be off by a pixel in arrangement but this could only come up when all timeslots are in 
			// view so this is unlikely to pose any significant display issues.
			//
			int extraPixels = 0;

			if (!double.IsInfinity(isVertical ? _lastMeasureSize.Height : _lastMeasureSize.Width)) // AS 5/11/12 TFS104555
			{
				if (this.UseLayoutRounding && _firstVisibleRealizedIndex == 0 && CoreUtilities.GreaterThanOrClose(this.TimeslotScrollInfo.Viewport, this.TimeslotScrollInfo.Extent))
					extraPixels = PanelHelper.CalculateStackExtraPixelCount(finalSize, interItemSpacing, isVertical, this.TimeslotCount, this.ActualTimeslotExtent);
			}

			Rect arrangeRect = PanelHelper.ArrangeStack(this, _elementsInView, isVertical, finalSize, this.ActualTimeslotExtent, interItemSpacing, _firstVisibleRealizedIndex, extraPixels);

			if (_templateItem != null)
			{
				FrameworkElement templateElement = ((ISupportRecycling)_templateItem).AttachedElement;

				if (null != templateElement)
				{
					PanelHelper.ArrangeOutOfView(templateElement);
				}
			}

			if (_nowTimeHelper != null)
			{
				_nowTimeHelper.Arrange(finalSize, ref arrangeRect);
			}

			return new Size(arrangeRect.Width, arrangeRect.Height);
		}
		#endregion //ArrangeOverrideImpl

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
				return this.TimeslotOrientation;
			}
		}

		#endregion //LogicalOrientation

		#region MeasureOverrideImpl
		internal override System.Windows.Size MeasureOverrideImpl(System.Windows.Size availableSize, int firstItemIndex, int pageSize)
		{
			_lastMeasureSize = availableSize; // AS 5/11/12 TFS104555
			double timeSlotExtent = this.ActualTimeslotExtent;
			IList<TimeslotBase> timeslots = this.Timeslots;

			// if this isn't small evenly distributed section of an hour then always use the major line separations
			bool isVertical = this.IsVertical;

			int startItemIndex, endItemIndex;

			// clear the flag we use when calculating the am/pm designator
			_previousInitializedMajor = null;

			// AS 12/13/10 TFS61517
			//Size desired = PanelHelper.RealizeStack(availableSize, firstItemIndex, pageSize, this, timeslots, isVertical, 
			//    _elementsInView, timeSlotExtent, true, this.InterItemSpacing, this.InitializeItem,
			//    ref _firstVisibleRealizedIndex, out startItemIndex, out endItemIndex);
			bool wasReleasing = _releasingElements;
			Size desired;
			int extraPixels; // AS 1/13/12 TFS74252

			try
			{
				_releasingElements = true;
				desired = PanelHelper.RealizeStack(availableSize, firstItemIndex, pageSize, this, timeslots, isVertical,
					_elementsInView, ref timeSlotExtent, true, this.InterItemSpacing, this.InitializeItem,
					ref _firstVisibleRealizedIndex, out startItemIndex, out endItemIndex
					, false, out extraPixels // AS 1/13/12 TFS74252
					);
			}
			finally
			{
				_releasingElements = wasReleasing;
			}

			if (_templateItem != null)
				this.MeasureTemplateItem(availableSize, ref desired);

			if (_nowTimeHelper != null)
				_nowTimeHelper.Measure(availableSize, startItemIndex, endItemIndex);

			return desired;
		}
		#endregion //MeasureOverrideImpl

		#region OnTimeslotsChanged
		internal override void OnTimeslotsChanged(TimeslotCollection oldValue, TimeslotCollection newValue)
		{
			base.OnTimeslotsChanged(oldValue, newValue);

			if (_nowTimeHelper != null)
				_nowTimeHelper.OnTimeslotsChanged();

			// AS 12/13/10 TFS61517
			if ( oldValue != null )
			{
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
		} 
		#endregion // OnTimeslotsChanged

		#endregion //Base class overrides

		#region Properties

		#region CurrentTimeIndicatorVisibility
		internal Visibility CurrentTimeIndicatorVisibility
		{
			get { return _nowTimeHelper == null ? Visibility.Collapsed : _nowTimeHelper.Visibility; }
			set
			{
				if (value != this.CurrentTimeIndicatorVisibility)
				{
					if (_nowTimeHelper == null)
						_nowTimeHelper = new NowTimeIndicatorHelper(this);

					_nowTimeHelper.Visibility = value;
				}
			}
		}
		#endregion // CurrentTimeIndicatorVisibility

		#region IsFirstItem

		/// <summary>
		/// Identifies the IsFirstItem attached dependency property
		/// </summary>
		public static readonly DependencyProperty IsFirstItemProperty = DependencyProperty.RegisterAttached("IsFirstItem",
			typeof(bool), typeof(TimeslotPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsBoolPropertyChanged))
			);

		/// <summary>
		/// Returns a boolean indicating if the child is the first item in the source collection.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="IsFirstItemProperty"/>
		/// <seealso cref="SetIsFirstItem"/>

		[AttachedPropertyBrowsableForChildren()]

		public static bool GetIsFirstItem(DependencyObject d)
		{
			return (bool)d.GetValue(TimeslotPanel.IsFirstItemProperty);
		}

		/// <summary>
		/// Sets the value of the attached IsFirstItem DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="IsFirstItemProperty"/>
		/// <seealso cref="GetIsFirstItem"/>
		internal static void SetIsFirstItem(DependencyObject d, bool value)
		{
			d.SetValue(TimeslotPanel.IsFirstItemProperty, value);
		}

		#endregion //IsFirstItem

		#region IsLastItem

		/// <summary>
		/// Identifies the IsLastItem attached dependency property
		/// </summary>
		public static readonly DependencyProperty IsLastItemProperty = DependencyProperty.RegisterAttached("IsLastItem",
			typeof(bool), typeof(TimeslotPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsBoolPropertyChanged))
			);

		/// <summary>
		/// Returns a boolean indicating if the child is the first item in the source collection.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="IsLastItemProperty"/>
		/// <seealso cref="SetIsLastItem"/>

		[AttachedPropertyBrowsableForChildren()]

		public static bool GetIsLastItem(DependencyObject d)
		{
			return (bool)d.GetValue(TimeslotPanel.IsLastItemProperty);
		}

		/// <summary>
		/// Sets the value of the attached IsLastItem DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="IsLastItemProperty"/>
		/// <seealso cref="GetIsLastItem"/>
		internal static void SetIsLastItem(DependencyObject d, bool value)
		{
			d.SetValue(TimeslotPanel.IsLastItemProperty, value);
		}

		#endregion //IsLastItem

		#region TemplateItem
		internal TimeslotBase TemplateItem
		{
			get { return _templateItem; }
			set
			{
				if (value != _templateItem)
				{
					if (_templateItem != null)
					{
						FrameworkElement oldElement = ((ISupportRecycling)_templateItem).AttachedElement;

						if (null != oldElement)
						{
							RecyclingManager.Manager.ReleaseElement(_templateItem, this);
						}
					}

					_templateItem = value;
					this.InvalidateMeasure();
				}
			}
		} 
		#endregion // TemplateItem

		#endregion // Properties

		#region Methods

		#region InitializeHeader

		private void InitializeHeader(TimeslotHeader presenter, int index)
		{
			bool isFirst;

			if (index < this.FirstTimeslotIndex)
			{
				// never true for a partially visible or out of view item
				isFirst = false;
			}
			else if (presenter.IsFirstInMajor == false)
			{
				// never true for minor slots
				isFirst = false;
			}
			// AS 4/1/11 TFS60630
			// Check the time below to see if its at a 0 minute mark.
			//
			//else if (_previousInitializedMajor == null)
			//{
			//    // always true if we haven't hit a major yet since we process them in visual order
			//    isFirst = true;
			//}
			else
			{
				DateInfoProvider provider = ScheduleUtilities.GetDateInfoProvider(ScheduleUtilities.GetControl(this));

				if (provider.DisplayTimeIn24HourFormat)
					isFirst = false;
				else
				{
					// for all other majors we have to compare the am/pm
					TimeslotBase ts = presenter.Timeslot as TimeslotBase;
					DateTime thisStart = ts.Start;

					// AS 4/1/11 TFS60630
					// Only include the item when it represents a 0 minute time.
					//
					if (_previousInitializedMajor == null)
					{
						isFirst = thisStart.Minute == 0;
					}
					else
					{
						DateRange range = this.GetTimeslotTime(index - 1);
						DateTime previousStart = range.Start;

						isFirst = previousStart.Hour < 12 != thisStart.Hour < 12;

						// AS 4/1/11 TFS60630
						if (!isFirst && _previousInitializedMajor != null)
							isFirst = _previousInitializedMajor.Value.Hour < 12 != thisStart.Hour < 12;
					}
				}
			}

			bool currentIsFirst = presenter.ShowAMPMDesignator;

			if (currentIsFirst != isFirst)
			{
				if (!isFirst)
					presenter.ClearValue(TimeslotHeader.ShowAMPMDesignatorPropertyKey);
				else
					presenter.SetValue(TimeslotHeader.ShowAMPMDesignatorPropertyKey, KnownBoxes.TrueBox);
			}

			if (isFirst)
				_previousInitializedMajor = presenter.Start;
		}
		#endregion // InitializeHeader

		#region InitializeItem
		private void InitializeItem(ISupportRecycling item, int index, FrameworkElement container, PanelHelper.InitializeItemState state)
		{
			if (state != PanelHelper.InitializeItemState.ExistingContainer)
			{
				TimeRangePresenterBase presenter = container as TimeRangePresenterBase;

				if (null != presenter)
					InitializePresenter(presenter, index);
			}

			TimeslotHeader header = container as TimeslotHeader;

			if (null != header)
				this.InitializeHeader(header, index);
		}
		#endregion // InitializeItem

		#region InitializePresenter
		private void InitializePresenter(TimeRangePresenterBase presenter, int index)
		{
			Debug.Assert(null != presenter);

			TimeslotBase ts = presenter.Timeslot as TimeslotBase;

			bool isFirstInDay, isLastInDay, isFirstInMajor, isLastInMajor;

			this.GetIsFirstLastState(index, ts.Start, out isFirstInDay, out isLastInDay, out isFirstInMajor, out isLastInMajor);

			if (presenter is TimeslotPresenterBase)
			{
				ScheduleUtilities.SetBoolTrueProperty(presenter, TimeslotPresenterBase.IsFirstInMajorProperty, isFirstInMajor);
				ScheduleUtilities.SetBoolTrueProperty(presenter, TimeslotPresenterBase.IsLastInMajorProperty, isLastInMajor);
				ScheduleUtilities.SetBoolTrueProperty(presenter, TimeslotPresenterBase.IsFirstInDayProperty, isFirstInDay);
				ScheduleUtilities.SetBoolTrueProperty(presenter, TimeslotPresenterBase.IsLastInDayProperty, isLastInDay);
			}

			ScheduleUtilities.SetBoolTrueProperty(presenter, TimeslotPanel.IsFirstItemProperty, index == 0);
			ScheduleUtilities.SetBoolTrueProperty(presenter, TimeslotPanel.IsLastItemProperty, index == this.TimeslotCount - 1);
		}
		#endregion // InitializePresenter

		#region MeasureTemplateItem
		private void MeasureTemplateItem(Size availableSize, ref Size desired)
		{
			if (null == _templateItem)
				return;

			bool isVertical = this.IsVertical;
			ISupportRecycling item = _templateItem;

			FrameworkElement itemElement = item.AttachedElement;

			if (null == itemElement)
			{
				RecyclingManager.Manager.AttachElement(item, this);
				itemElement = item.AttachedElement;

				if (itemElement is TimeslotPresenterBase)
				{
					itemElement.SetValue(TimeslotPresenterBase.IsFirstInMajorProperty, KnownBoxes.TrueBox);
					itemElement.SetValue(TimeslotPresenterBase.IsLastInMajorProperty, KnownBoxes.TrueBox);
					itemElement.SetValue(TimeslotPresenterBase.IsFirstInDayProperty, KnownBoxes.TrueBox);
					itemElement.SetValue(TimeslotPresenterBase.IsLastInDayProperty, KnownBoxes.TrueBox);
				}

				itemElement.Tag = ScheduleControlBase.MeasureOnlyItemId;
			}

			Size measureSize = availableSize;

			if (isVertical)
				measureSize.Height = this.ActualTimeslotExtent;
			else
				measureSize.Width = this.ActualTimeslotExtent;

			itemElement.Measure(measureSize);

			// since the panel virtualizes its contents and in the case of the timeslotheader
			// area only some elements have text displayed, the template item is really there 
			// to ensure we have space reserved. even with that though there is the potential
			// that the size that for the template element is smaller than some other element 
			// in which case the panel will grow and shrink as those elements come into and 
			// out of view. to try and avoid that we'll put in extra space around the template 
			// element. in the future we could consider implementing a grow only strategy but 
			// there isn't necessarily a good trigger point for when to reset the grow only 
			// value (e.g. when the timeslotinterval changes as the format may be different).
			//
			const double ExtraSpace = 3;

			if (isVertical)
				desired.Width = itemElement.DesiredSize.Width + ExtraSpace;
			else
				desired.Height = itemElement.DesiredSize.Height + ExtraSpace;

		}
		#endregion // MeasureTemplateItem

		#region OnIsBoolPropertyChanged
		private static void OnIsBoolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			IReceivePropertyChange<bool> rpc = d as IReceivePropertyChange<bool>;

			if (null != rpc)
				rpc.OnPropertyChanged(e.Property, (bool)e.OldValue, (bool)e.NewValue);
		}
		#endregion // OnIsBoolPropertyChanged

		#region OnMouseEnterTimeslot
		internal void OnMouseEnterTimeslot(TimeRangePresenterBase presenter)
		{
			ITimeRangeArea rangeArea = PresentationUtilities.GetTemplatedParent(this) as ITimeRangeArea;
			ScheduleActivityPanel activityPanel = rangeArea != null ? rangeArea.ActivityPanel as ScheduleActivityPanel : null;

			if (null != activityPanel)
				activityPanel.OnMouseEnterTimeslot(presenter);
		}
		#endregion // OnMouseEnterTimeslot

		#region OnMouseLeaveTimeslot
		internal void OnMouseLeaveTimeslot(TimeRangePresenterBase presenter)
		{
			ITimeRangeArea rangeArea = PresentationUtilities.GetTemplatedParent(this) as ITimeRangeArea;
			ScheduleActivityPanel activityPanel = rangeArea != null ? rangeArea.ActivityPanel as ScheduleActivityPanel : null;

			if (null != activityPanel)
				activityPanel.OnMouseLeaveTimeslot(presenter);
		}
		#endregion // OnMouseLeaveTimeslot

		#endregion // Methods

		#region IRecyclableElementHost Members

		void IRecyclableElementHost.OnElementAttached(ISupportRecycling item, FrameworkElement element, bool isNewlyRealized)
		{
		}

		void IRecyclableElementHost.OnElementReleased(ISupportRecycling item, FrameworkElement element, bool isRemoved)
		{
			// AS 12/13/10 TFS61517
			if ( !_releasingElements )
			{
				_elementsInView.Remove(element);
			}
		}

		bool IRecyclableElementHost.ShouldRemove(ISupportRecycling item, FrameworkElement element)
		{
			if (item == _templateItem)
				return true;

			return false;
		}

		#endregion //IRecyclableElementHost Members

		#region NowTimeIndicatorHelper class
		private class NowTimeIndicatorHelper
		{
			#region Member Variables

			private TimeslotPanel _panel;
			private TimeslotPositionInfo _positionInfo;
			private TimeSpan _logicalDayOffset;
			private int _currentStartIndex;
			private int _currentEndIndex;
			private TimeSpan _nowAdjustment;

			private object _currentTimerToken;
			private DateRange? _rangeBeingWatched;
			private bool? _isOutOfView;

			private CurrentTimeIndicator _indicator;
			private Visibility _visibility;

			#endregion // Member Variables

			#region Constructor
			internal NowTimeIndicatorHelper(TimeslotPanel panel)
			{
				_panel = panel;
				_positionInfo = new TimeslotPositionInfo(panel);

				this.CalculateNowOffset();
			}
			#endregion // Constructor

			#region Properties

			#region Visibility
			internal Visibility Visibility
			{
				get { return _visibility; }
				set
				{
					if (value != _visibility)
					{
						_visibility = value;

						// if we have a timer going then release it
						this.RemoveCurrentTimer();

						if (_visibility == Visibility.Collapsed)
						{
							#region Remove the indicator
							// if the date is not in the visible dates then remove the indicator
							if (_indicator != null)
							{
								_panel.Children.Remove(_indicator);
								_indicator.ClearValue(ScheduleControlBase.ControlProperty);
								_indicator = null;
							}
							#endregion // Remove the indicator
						}
						else
						{
							// just invalidate the measure and wait to handle this
							_panel.InvalidateMeasure();

							if (null != _indicator)
								_indicator.Visibility = value;
						}

						this.CalculateNowOffset();

					}
				}
			}
			#endregion // Visibility

			#region Now
			/// <summary>
			/// Returns the current time based on the range of slots displayed by the control
			/// </summary>
			private DateTime Now
			{
				get
				{
					DateTime now = CurrentTime.Now;

					// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
					// Verify the we have both a local and utc token
					//now = ScheduleUtilities.ConvertFromUtc( _panel.TimeZoneInfoProvider.LocalToken, now);
					TimeZoneInfoProvider provider = _panel.TimeZoneInfoProvider;
					if ( provider != null && provider.LocalToken != null && provider.UtcToken != null )
						now = ScheduleUtilities.ConvertFromUtc(provider.LocalToken, now);

					return now;
				}
			} 
			#endregion // Now

			#endregion // Properties

			#region Methods

			#region Internal Methods

			#region Arrange
			internal void Arrange(Size finalSize, ref Rect arrangeRect)
			{
				if (_indicator == null)
					return;

				this.ArrangeImpl(finalSize, ref arrangeRect);
			}

			private void ArrangeImpl(Size finalSize, ref Rect arrangeRect)
			{
				Rect elementRect = new Rect();

				// if we're not sure if its out of view...
				if (_isOutOfView != true)
				{
					bool isVertical = _panel.IsVertical;
					DateTime now = this.Now;
					TimeSpan dayAdjustment = _nowAdjustment;
					DateTime dateForPosition = now.Add(dayAdjustment);

					// modify from local to the target zone if the panel isn't using the local
					if ( null != _panel.Timeslots.ModifyDateFunc )
						dateForPosition = _panel.Timeslots.ModifyDateFunc(dateForPosition);

					ItemPosition? position = _positionInfo.GetItemPosition(dateForPosition, dateForPosition);

					int startIndex, endIndex;
					_panel.GetTimeslotIndex(dateForPosition, dateForPosition, out startIndex, out endIndex);

					if (position == null)
					{
						#region Current Time in "missing" timeslot block

						// start watching the current "missing" block and get notified when the time leaves this range
						Debug.Assert(startIndex <= 0, "The time exists within a slot?");
						startIndex = ~startIndex;

						// if its no longer in range then just watch for when the time reenters the visible range
						if (startIndex >= _currentEndIndex || startIndex < _currentStartIndex)
						{
							var range = ScheduleUtilities.ConvertToUtc(_panel.TimeZoneInfoProvider.LocalToken, GetInViewRange());
							WatchRange(range, true, this.OnTickInvalidateMeasure);
						}
						else
						{
							DateTime start = startIndex == 0 ? now : _panel.GetTimeslotTime(startIndex - 1, false).End.Subtract(dayAdjustment);
							DateTime end = startIndex == _panel.TimeslotCount ? now : _panel.GetTimeslotTime(startIndex, false).Start.Subtract(dayAdjustment);
							DateRange range = ScheduleUtilities.ConvertToUtc(_panel.TimeZoneInfoProvider.LocalToken, start, end);

							_isOutOfView = true;
							WatchRange(range, false, this.OnTickInvalidateArrange);
						}
						#endregion // Current Time in "missing" timeslot block
					}
					else
					{
						Rect firstTimeslotRect = PanelHelper.GetStartingArrangeRect(isVertical, finalSize, _panel.ActualTimeslotExtent, _panel.InterItemSpacing, _panel.FirstTimeslotIndex, _panel.TimeslotCount);

						Size desired = _indicator.DesiredSize;
						double minExtent = isVertical ? desired.Height : desired.Width;

						#region CalculateRect
						elementRect = new Rect(new Point(), finalSize);

						if (isVertical)
						{
							elementRect.Height = desired.Height;
							elementRect.Y = position.Value.Start + firstTimeslotRect.Y - desired.Height / 2;
						}
						else
						{
							elementRect.Width = desired.Width;
							elementRect.X = position.Value.Start + firstTimeslotRect.X - desired.Width / 2;
						}
						#endregion // CalculateRect

						_isOutOfView = false;
						now = ScheduleUtilities.ConvertToUtc(_panel.TimeZoneInfoProvider.LocalToken, now);
						WatchRange(new DateRange(now, now.AddTicks(_positionInfo.EstimatedTicksPerPixel)), false, this.OnTickInvalidateArrange);
					}
				}

				// the range is out of view so use the last measure size and an out of view position
				if (_isOutOfView == true)
				{
					// AS 1/10/12 TFS98969
					//elementRect = new Rect(new Point(-10000, -10000), PanelHelper.GetOutOfViewMeasureSize(_indicator));
					elementRect = new Rect(-10000, -10000, 0, 0);
				}
				else
				{
					// otherwise union with the current arrange rect in case this is partially out of view
					arrangeRect.Union(elementRect);
				}

				_indicator.Arrange(elementRect);
			}
			#endregion // Arrange

			#region Measure
			internal void Measure(Size availableSize, int startItemIndex, int endItemIndex)
			{
				if (_visibility == Visibility.Collapsed)
				{
					Debug.Assert(null == _indicator, "We shouldn't have an indicator if we don't have a current date");
					return;
				}

				var ctrl = ScheduleControlBase.GetControl(_panel);
				Debug.Assert(null != ctrl, "No state on the panel?");

				if (ctrl == null)
					return;

				if (endItemIndex < 0)
				{
					Debug.Assert(null == _indicator, "We have an indicator but no timelots?");
					return;
				}

				_positionInfo.Initialize();
				_logicalDayOffset = _panel.LogicalDayOffset;

				// store the indexes
				_currentStartIndex = startItemIndex;
				_currentEndIndex = endItemIndex;

				DateRange range = this.GetInViewRange();
				DateTime now = this.Now;

				// if now isn't in view
				if (!range.Contains(now))
				{
					// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
					// Verify the we have both a local and utc token
					//range = ScheduleUtilities.ConvertToUtc(_panel.TimeZoneInfoProvider.LocalToken, range);
					TimeZoneInfoProvider provider = _panel.TimeZoneInfoProvider;
					if (provider != null && provider.LocalToken != null && provider.UtcToken != null)
						range = ScheduleUtilities.ConvertToUtc(provider.LocalToken, range);

					_isOutOfView = true;
					WatchRange(range, true, this.OnTickInvalidateMeasure);
				}
				else
				{
					_isOutOfView = false;

					// the date is in view so make sure we have an indicator
					if (_indicator == null)
					{
						_indicator = new CurrentTimeIndicator();
						_indicator.Orientation = _panel.TimeslotOrientation == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical;
						_indicator.Visibility = _visibility;
						ScheduleControlBase.SetControl(_indicator, ctrl);
						// AS 7/21/10 TFS36040
						// Originally thought the indicator was above the timeslots.
						//
						//Canvas.SetZIndex(_indicator, 10);
						Canvas.SetZIndex(_indicator, -1);
						_panel.Children.Add(_indicator);
					}
				}

				if (null != _indicator)
					PanelHelper.MeasureElement(_indicator, availableSize);
			}

			#endregion // Measure

			#region OnTimeslotsChanged
			internal void OnTimeslotsChanged()
			{
				this.CalculateNowOffset();
			}
			#endregion // OnTimeslotsChanged

			#endregion // Internal Methods

			#region Private Methods

			#region CalculateNowOffset
			/// <summary>
			/// For a control like dayview, the timeslot headers are shared across dates so we need to adjust 
			/// what we consider Now when seeing if a timeslot is in view.
			/// </summary>
			private void CalculateNowOffset()
			{
				TimeSpan offset = TimeSpan.Zero;

				if ( _visibility != Visibility.Collapsed )
				{
					ScheduleControlBase control = ScheduleUtilities.GetControl(_panel);
					Debug.Assert(null != control, "Need control to calculate the now offset");

					if ( null != control )
					{
						offset = control.GetNowTimeAdjustment();
					}
				}

				Debug.Assert(offset.TotalDays == Math.Round(offset.TotalDays), "Expected this to be a 'day' offset");
				_nowAdjustment = offset;
			}
			#endregion // CalculateNowOffset

			#region GetInViewRange
			private DateRange GetInViewRange()
			{
				DateTime start = _panel.GetTimeslotTime(_currentStartIndex, false).Start;
				DateTime end = _panel.GetTimeslotTime(_currentEndIndex, false).End;

				start = start.Subtract(_nowAdjustment);
				end = end.Subtract(_nowAdjustment);

				return new DateRange(start, end);
			}
			#endregion // GetInViewRange

			#region OnTickHelper
			private void OnTickHelper()
			{
				_currentTimerToken = null;
				_rangeBeingWatched = null;
				_isOutOfView = null;
			}
			#endregion // OnTickHelper

			#region OnTickInvalidateMeasure
			private void OnTickInvalidateMeasure()
			{
				this.OnTickHelper();
				_panel.InvalidateMeasure();
			}
			#endregion // OnTickInvalidateMeasure

			#region OnTickInvalidateArrange
			private void OnTickInvalidateArrange()
			{
				this.OnTickHelper();
				_panel.InvalidateArrange();
			}
			#endregion // OnTickInvalidateArrange

			#region WatchRange
			private void WatchRange(DateRange range, bool isEnter, Action action)
			{
				// if we're already watching this range then we don't need to do anything
				if (range != _rangeBeingWatched)
				{
					// if the range is not in view then start a timer and wait for it to be in view
					this.RemoveCurrentTimer();

					// wait to be notified that the current time entered that range
					_currentTimerToken = TimeManager.Instance.AddTimeRange(range, isEnter, action);
					_rangeBeingWatched = range;
				}
			}
			#endregion // WatchRange

			#region RemoveCurrentTimer
			private void RemoveCurrentTimer()
			{
				if (_currentTimerToken != null)
				{
					_rangeBeingWatched = null;
					TimeManager.Instance.Remove(_currentTimerToken);
					_currentTimerToken = null;
				}

				Debug.Assert(_rangeBeingWatched == null, "We have a range being watched but no timer token?");
			}
			#endregion // RemoveCurrentTimer

			#endregion // Private Methods

			#endregion // Methods
		}
		#endregion // NowTimeIndicatorHelper class
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