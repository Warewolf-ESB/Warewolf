using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Abstract base class for a panel that positions time slot related elements.
	/// </summary>
	[DesignTimeVisible(false)]
	public abstract class TimeslotPanelBase : Panel
	{
		#region Member Variables

		private double _actualTimeslotExtent;
		private ScrollInfo _timeslotScrollInfo;
		private int _increasedItems;
		private TimeSpan _timeslotInterval = TimeSpan.FromMinutes(15);
		private int _timeslotCount;
		private bool _alwaysUseMajor;
		private TimeSpan _logicalDayOffset;
		private TimeZoneInfoProvider _tzProvider;

		#endregion //Member Variables

		#region Constructor
		internal TimeslotPanelBase()
		{
			_timeslotScrollInfo = new ElementScrollInfo(this);
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected sealed override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			DebugHelper.DebugLayout(this, false, true, "ArrangeOverride Start", "FinalSize: {0}, ActualTimeslotExtent:{1}", finalSize, this.ActualTimeslotExtent);

			Size arrangeSize = this.ArrangeOverrideImpl(finalSize);

			DebugHelper.DebugLayout(this, true, false, "ArrangeOverride End", "FinalSize: {0}, ActualTimeslotExtent:{1}, ArrangeSize:{2}", finalSize, this.ActualTimeslotExtent, arrangeSize);

			return arrangeSize;
		}
		#endregion //ArrangeOverride

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected sealed override Size MeasureOverride(Size availableSize)
		{
			DebugHelper.DebugLayout(this, false, true, "MeasureOverride Start", "AvailableSize: {0}", availableSize);

			Size desired = new Size();
			bool isVertical = this.IsVertical;
			int timeslotCount = this.TimeslotCount;
			double availableExtent = isVertical ? availableSize.Height : availableSize.Width;
			double interItemSpacing = this.InterItemSpacing;
			int excess;

			// calculate and store the extent for each timeslot
			double timeSlotExtent = TimeslotPanelBase.CalculateTimeslotExtent(timeslotCount, availableExtent, this.PreferredTimeslotExtent, double.PositiveInfinity, interItemSpacing, this.UseLayoutRounding, out excess);

			// we want the # of items that will fit in the available area
			this.ActualTimeslotExtent = timeSlotExtent;
			this.IncreasedItems = excess;
			int pageSize;

			if (double.IsPositiveInfinity(availableExtent))
				pageSize = timeslotCount;
			else
			{
				// always try to position at least 1 item if we can
				pageSize = Math.Max(1, (int)Math.Floor((availableExtent + interItemSpacing) / (timeSlotExtent + interItemSpacing)));
				pageSize = Math.Max(Math.Min(timeslotCount, pageSize), 0);
			}

			Debug.Assert(timeslotCount == 0 || ScheduleControlBase.GetIsAttachedElement(this), "The element has timeslots but its IsAttached is false so the element isn't associated with a schedule control.");

			// calculate the top item in view
			int firstItemIndex = Math.Min(timeslotCount - pageSize, this.FirstTimeslotIndex);

			// initialize the scroll info first so derived panels can get the range from the scrollinfo
			_timeslotScrollInfo.Initialize(pageSize, timeslotCount, firstItemIndex);

			long timeslotTicks = this.TimeslotInterval.Ticks;
			_alwaysUseMajor = timeslotTicks >= TimeSpan.TicksPerHour || TimeSpan.TicksPerHour % timeslotTicks != 0;

			// AS 5/9/12 TFS104560
			if (timeslotTicks < TimeSpan.TicksPerMinute * 5)
				_alwaysUseMajor = true;

			desired = this.MeasureOverrideImpl(availableSize, firstItemIndex, pageSize);

			double fullExtent = PanelHelper.CalculateExtent(timeslotCount, interItemSpacing, timeSlotExtent);

			if (isVertical)
				desired.Height = Math.Min(fullExtent, availableSize.Height);
			else
				desired.Width = Math.Min(fullExtent, availableSize.Width);

			DebugHelper.DebugLayout(this, true, false, "MeasureOverride End", "AvailableSize: {0}, ActualTimeslotExtent:{1}, Desired: {2}", availableSize, this.ActualTimeslotExtent, desired);

			return desired;
		}
		#endregion //MeasureOverride

		#region OnChildDesiredSizeChanged

		/// <summary>
		/// Invoked when the desired size of a child element has been changed.
		/// </summary>
		/// <param name="child">The child whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			DebugHelper.DebugLayout(this, "OnChildDesiredSizeChanged", "Child: {0} [{1}]", child.GetType().Name, child.GetHashCode());

			base.OnChildDesiredSizeChanged(child);
		} 

		#endregion //OnChildDesiredSizeChanged

		#endregion //Base class overrides

		#region Properties

		#region Public properties

		#region InterItemSpacing

		/// <summary>
		/// Identifies the <see cref="InterItemSpacing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterItemSpacingProperty = DependencyProperty.Register("InterItemSpacing",
			typeof(double), typeof(TimeslotPanelBase),
			DependencyPropertyUtilities.CreateMetadata(0d, new PropertyChangedCallback(OnInterItemSpacingChanged))
			);

		private static void OnInterItemSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeslotPanelBase item = (TimeslotPanelBase)d;
			item.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the amount of space between each item.
		/// </summary>
		/// <seealso cref="InterItemSpacingProperty"/>
		public double InterItemSpacing
		{
			get
			{
				return (double)this.GetValue(TimeslotPanelBase.InterItemSpacingProperty);
			}
			set
			{
				this.SetValue(TimeslotPanelBase.InterItemSpacingProperty, value);
			}
		}

		#endregion //InterItemSpacing

		#endregion //Public properties

		#region Internal properties

		#region ActualTimeslotExtent
		internal double ActualTimeslotExtent
		{
			get
			{
				return _actualTimeslotExtent;
			}
			private set
			{
				Debug.Assert(value > 0);
				_actualTimeslotExtent = value;
			}
		}
		#endregion //ActualTimeslotExtent

		#region FirstTimeslotIndex
		internal int FirstTimeslotIndex
		{
			get
			{
				return (int)_timeslotScrollInfo.Offset;
			}
			set
			{
				_timeslotScrollInfo.Offset = value;
			}
		}
		#endregion //FirstTimeslotIndex

		
		#region IncreasedItems
		internal int IncreasedItems
		{
			get { return _increasedItems; }
			private set
			{
				Debug.Assert(value >= 0);
				_increasedItems = value;
			}
		} 
		#endregion // IncreasedItems

		#region IsVertical
		internal bool IsVertical
		{
			get { return this.TimeslotOrientation == Orientation.Vertical; }
		}
		#endregion //IsVertical

		#region LogicalDayOffset
		internal TimeSpan LogicalDayOffset
		{
			get { return _logicalDayOffset; }
		}
		#endregion // LogicalDayOffset

		#region PreferredTimeslotExtent
		private double _preferredTimeslotExtent = 20d;

		internal double PreferredTimeslotExtent
		{
			get { return _preferredTimeslotExtent; }
			set
			{
				if (value != _preferredTimeslotExtent)
				{
					_preferredTimeslotExtent = value;

					if (_preferredTimeslotExtent != _actualTimeslotExtent)
						this.InvalidateMeasure();
				}
			}
		}
		#endregion // PreferredTimeslotExtent

		#region TimeslotOrientation

		private Orientation _timeslotOrientation = Orientation.Vertical;

		/// <summary>
		/// Identifies the <see cref="TimeslotOrientation"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty TimeslotOrientationProperty = DependencyPropertyUtilities.Register("TimeslotOrientation",
			typeof(Orientation), typeof(TimeslotPanelBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.OrientationVerticalBox, new PropertyChangedCallback(OnTimeslotOrientationChanged))
			);

		private static void OnTimeslotOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeslotPanelBase item = (TimeslotPanelBase)d;

			item._timeslotOrientation = (Orientation)e.NewValue;
			item.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the orientation in which the items will be arranged.
		/// </summary>
		/// <seealso cref="TimeslotOrientationProperty"/>
		internal Orientation TimeslotOrientation
		{
			get
			{
				return _timeslotOrientation;
			}
			set
			{
				this.SetValue(TimeslotPanelBase.TimeslotOrientationProperty, value);
			}
		}

		#endregion //TimeslotOrientation

		#region TimeslotScrollInfo
		internal ScrollInfo TimeslotScrollInfo
		{
			get { return _timeslotScrollInfo; }
		}
		#endregion //TimeslotScrollInfo

		#region TimeslotCount
		internal int TimeslotCount
		{
			get
			{
				Debug.Assert(_timeslotCount == 0 && _timeslots == null || _timeslotCount == _timeslots.Count);
				return _timeslotCount;
			}
		}
		#endregion //TimeslotCount

		#region TimeslotInterval
		internal TimeSpan TimeslotInterval
		{
			get { return _timeslotInterval; }
		} 
		#endregion // TimeslotInterval

		#region TimeslotRange
		internal DateRange? TimeslotRange
		{
			get
			{
				var ts = this.Timeslots;

				if (ts == null || ts.Count == 0)
					return null;

				return new DateRange(ts.GetDateRange(0).Value.Start, ts.GetDateRange(ts.Count - 1).Value.End);
			}
		} 
		#endregion // TimeslotRange

		#region Timeslots

		private TimeslotCollection _timeslots;

		/// <summary>
		/// Returns or sets the collection of associated timeslots
		/// </summary>
		internal TimeslotCollection Timeslots
		{
			get { return _timeslots; }
			set
			{
				if (value != _timeslots)
				{
					TimeslotCollection old = _timeslots;

					Debug.Assert(value == null || ((IList<TimeslotBase>)value).IsReadOnly);
					_timeslots = value;

					this.OnTimeslotsChanged(old, value);
				}
			}
		}

		internal virtual void OnTimeslotsChanged(TimeslotCollection oldValue, TimeslotCollection newValue)
		{
			_tzProvider = ScheduleUtilities.GetTimeZoneInfoProvider(this);

			this.InvalidateMeasure();

			if (newValue != null)
				_timeslotInterval = newValue.TimeslotInterval;

			_timeslotCount = newValue == null ? 0 : newValue.Count;

			var dm = ScheduleUtilities.GetDataManager(this);
			_logicalDayOffset = dm == null ? TimeSpan.Zero : dm.LogicalDayOffset;
		}

		#endregion //Timeslots

		#endregion //Internal properties

		#endregion //Properties

		#region Methods

		#region Internal methods

		#region ArrangeOverrideImpl
		
		internal abstract Size ArrangeOverrideImpl(Size finalSize);

		#endregion //ArrangeOverrideImpl

		#region CalculateTimeslotExtent
		internal static double CalculateTimeslotExtent(int timeslotCount, double availableExtent, double minItemExtent, double maxItemExtent, double interItemSpacing, bool usesLayoutRounding, out int excess)
		{
			// while the minimum takes precedence over the max (as occurs elsewhere in the framework), 
			// we still cannot let it exceed the available area since we do time slot level scrolling
			double min = Math.Max(1, Math.Min(availableExtent, minItemExtent));

			// similarly we want to make sure the max is at least as much as the constrained 
			// min but no greater than the available extent
			double max = Math.Max(min, Math.Min(availableExtent, maxItemExtent));

			
			if (usesLayoutRounding)
			{
				min = Math.Round(min);
				max = Math.Round(max);
			}

			// assume we will use the minimum
			double timeslotExtent = min;
			excess = 0;

			// if we have timeslots and the min is not the same as the max
			// then see if we need to extend the timeslots to fill the area
			if (timeslotCount > 0 && min < max)
			{
				double totalExtent = PanelHelper.CalculateExtent(timeslotCount, interItemSpacing, timeslotExtent);

				// if we're given an actual value and the available is greater than 
				// the extent then use up to the max for each or what will fill 
				// the extent, whichever is smaller
				if (!double.IsPositiveInfinity(availableExtent) && totalExtent < availableExtent)
				{
					double maxToFill = Math.Max(min, (availableExtent - (interItemSpacing * (timeslotCount - 1))) / timeslotCount);

					// if we can fit more timeslots then we have then just 
					// use the maximum time slot extent
					if (CoreUtilities.GreaterThan(maxToFill, max))
						timeslotExtent = max;
					else // otherwise use the value that will fill the available area
						timeslotExtent = maxToFill;

					if (usesLayoutRounding)
					{
						timeslotExtent = Math.Floor(timeslotExtent);

						// if we're given a constraining size and the items can be increased by at least 1...
						if (!CoreUtilities.LessThan(max - timeslotExtent, 1))
						{
							totalExtent = PanelHelper.CalculateExtent(timeslotCount, interItemSpacing, timeslotExtent);

							// if we have a constraining extent and we're not filling the panel...
							if (CoreUtilities.LessThan(totalExtent, availableExtent))
							{
								excess = (int)Math.Floor((availableExtent - totalExtent) % timeslotCount);
							}
						}
					}
				}
			}

			return timeslotExtent;
		}
		#endregion //CalculateTimeslotExtent

		#region GetIsFirstLastState
		internal void GetIsFirstLastState(int index, DateTime timeslotStart, out bool isFirstInDay, out bool isLastInDay, out bool isFirstInMajor, out bool isLastInMajor)
		{
			TimeSpan dayOffset = this.LogicalDayOffset;

			// note i'm comparing start to start since its technically possible at least right now
			// that a timeslot can span multiple days
			DateTime? previousStart = index == 0 ? (DateTime?)null : this.GetTimeslotTime(index - 1).Start.Subtract(dayOffset);
			DateTime? nextStart = index == this.TimeslotCount - 1 ? (DateTime?)null : this.GetTimeslotTime(index + 1).Start.Subtract(dayOffset);
			DateTime thisStart = timeslotStart.Subtract(dayOffset);

			isFirstInDay = previousStart == null || previousStart.Value.Date != thisStart.Date;
			isLastInDay = nextStart == null || nextStart.Value.Date != thisStart.Date;

			if (_alwaysUseMajor)
			{
				isFirstInMajor = true;
				isLastInMajor = true;
			}
			else
			{
				// AS 4/1/11 TFS60630
				// Shift the time back by the logical day offset. In this way if the day offset was say 2:30 hours and an interval 
				// of 15 minutes, you'd get 1 major for the :30, 1 minor followed by the next major for the next hour.
				//
				//isFirstInMajor = isFirstInDay || previousStart == null || previousStart.Value.Hour != thisStart.Hour;
				//isLastInMajor = isLastInDay || nextStart == null || nextStart.Value.Hour != thisStart.Hour;
				isFirstInMajor = isFirstInDay || previousStart == null || previousStart.Value.Add(dayOffset).Hour != thisStart.Add(dayOffset).Hour;
				isLastInMajor = isLastInDay || nextStart == null || nextStart.Value.Add(dayOffset).Hour != thisStart.Add(dayOffset).Hour;
			}
		}
		#endregion // GetIsFirstLastState

		#region GetTimeslotTimeLocal

		/// <summary>
		/// Returns the date range for the specified timeslot ignoring the date time adjustments (e.g. for the secondary timezone)
		/// </summary>
		/// <param name="index">Index of the timeslot</param>
		/// <returns>The date range for the index</returns>
		internal DateRange GetTimeslotTimeLocal(int index)
		{
			TimeslotCollection slots = this.Timeslots;
			DateRange? range = null;

			if (null != slots)
			{
				range = slots.CalculateDateRange(index, false);

				if (null != range)
					return range.Value;
			}

			Debug.Assert(false, "Invalid index?");
			return DateRange.Infinite;
		}
		#endregion // GetTimeslotTimeLocal

		#region GetTimeslotIndex
		/// <summary>
		/// Returns the start and end indexes for a given range.
		/// </summary>
		/// <param name="range">The range to evaluate</param>
		/// <param name="startIndex">Out parameter set to the index of the timeslot that contains the beginning of the appointment</param>
		/// <param name="endIndex">Out parameter set to the index of the timeslot that contains the end of the appointment</param>
		/// <returns>Returns false if no portion of the range exists within a timeslot</returns>
		internal bool GetTimeslotIndex(DateRange range, out int startIndex, out int endIndex)
		{
			return GetTimeslotIndex(range.Start, range.End, out startIndex, out endIndex);
		}

		/// <summary>
		/// Returns the start and end indexes for a given range.
		/// </summary>
		/// <param name="startTime">The start time for the range</param>
		/// <param name="endTime">The end time for the range. This is expected to be <paramref name="startTime"/> or greater. This value is considered non-inclusive (unless it equals the start time)</param>
		/// <param name="startIndex">Out parameter set to the index of the timeslot that contains the beginning of the appointment</param>
		/// <param name="endIndex">Out parameter set to the index of the timeslot that contains the end of the appointment</param>
		/// <returns>Returns false if no portion of the range exists within a timeslot</returns>
		internal bool GetTimeslotIndex(DateTime startTime, DateTime endTime, out int startIndex, out int endIndex)
		{
			Debug.Assert(startTime <= endTime, "Start must be <= end");
			TimeslotCollection slots = this.Timeslots;

			if (null == slots)
			{
				startIndex = endIndex = -1;
				return false;
			}

			startIndex = slots.BinarySearch(startTime);
			endIndex = slots.BinarySearch(endTime);

			// if the start nor end are in view and they would be at
			// the same index then it won't be in any view
			if (startIndex < 0 && endIndex < 0 && startIndex == endIndex)
				return false;

			// if its only in view by the non-inclusive end then consider it out of view (assuming the start isn't the end)
			if (startIndex < 0 && endIndex == 0 && endTime == this.GetTimeslotTime(0).Start)
				return false;

			if (startIndex < 0)
			{
				startIndex = ~startIndex;
			}

			if (endIndex < 0)
			{
				endIndex = ~endIndex - 1;
			}
			else if (endIndex > startIndex && this.GetTimeslotTime(endIndex).Start == endTime)
			{
				endIndex--;
			}

			Debug.Assert(startIndex <= endIndex, "Calculated end index is before the start?");

			return true;
		}
		#endregion // GetTimeslotIndex

		#region GetTimeslotTime

		internal DateRange GetTimeslotTime(int index, bool useModifyFunc = true)
		{
			TimeslotCollection slots = this.Timeslots;
			DateRange? range = null;

			if (null != slots)
			{
				range = slots.CalculateDateRange(index, useModifyFunc);

				if (null != range)
					return range.Value;
			}

			Debug.Assert(false, "Invalid index?");
			return DateRange.Infinite;
		}
		#endregion // GetTimeslotTime

		#region HasTimeslots
		
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		#endregion // HasTimeslots

		#region MeasureOverrideImpl
		
		internal abstract Size MeasureOverrideImpl(Size availableSize, int firstItemIndex, int pageSize);

		#endregion //MeasureOverrideImpl

		#region SetIsAttachedElement
		internal void SetIsAttachedElement( bool isAttached )
		{
			if ( isAttached )
				this.SetValue(ScheduleControlBase.IsAttachedElementProperty, KnownBoxes.TrueBox);
			else
				this.ClearValue(ScheduleControlBase.IsAttachedElementProperty);
		} 
		#endregion // SetIsAttachedElement

		#region TimeZoneInfoProvider
		internal TimeZoneInfoProvider TimeZoneInfoProvider
		{
			get { return _tzProvider ?? TimeZoneInfoProvider.DefaultProvider; }
		} 
		#endregion // TimeZoneInfoProvider

		#endregion //Internal methods 

		#endregion //Methods

		#region TimeslotPositionInfo class
		internal class TimeslotPositionInfo
		{
			#region Member Variables

			private TimeslotPanelBase _panel;
			private double _timeslotExtent;
			private int _timeslotCount;
			private int _increasedItems;
			private bool _useLayoutRounding;
			private double _fullExtent;
			private double _interItemSpacing;
			private int _version;

			internal long TimeslotIntervalTicks;
			internal long EstimatedTicksPerPixel;

			#endregion // Member Variables

			#region Constructor
			internal TimeslotPositionInfo(TimeslotPanelBase panel)
			{
				_panel = panel;
				this.Initialize();
			}
			#endregion // Constructor

			#region Properties

			#region FullExtent
			public double FullExtent
			{
				get { return _fullExtent; }
			}
			#endregion // FullExtent 

			#region Version
			public int Version
			{
				get { return _version; }
			}
			#endregion // Version

			#endregion // Properties

			#region Methods

			#region BumpVersion
			internal void BumpVersion()
			{
				_version++;
			} 
			#endregion // BumpVersion

			#region GetItemPosition(int)
			/// <summary>
			/// Returns the position of the specified timeslot
			/// </summary>
			/// <param name="timeslotIndex">Index of the timeslot</param>
			/// <returns></returns>
			internal ItemPosition GetItemPosition(int timeslotIndex)
			{
				Debug.Assert(timeslotIndex >= 0 && timeslotIndex < _timeslotCount, "Invalid timeslot index");

				double offset = timeslotIndex * _timeslotExtent;

				// AS 4/20/11 TFS73203
				// This was incorrect. We have to add n pixels for the first 
				// few items including those that are receiving an extra pixel.
				//
				//if (timeslotIndex >= _increasedItems)
				if (_increasedItems > 0)
				{
					offset += Math.Min(timeslotIndex, _increasedItems);
				}

				// consider the interitemspacing
				offset += timeslotIndex * _interItemSpacing;

				double extent = _timeslotExtent;

				if (timeslotIndex < _increasedItems)
				{
					extent++;
				}

				// do not include the extra overlap in any timeslot but the last
				if (_interItemSpacing < 0 && timeslotIndex < _timeslotCount - 1)
					extent = Math.Max(0, extent + _interItemSpacing);

				return new ItemPosition(offset, extent);
			}
			#endregion // GetItemPosition(int)

			#region GetItemPosition(DateTime, DateTime)
			/// <summary>
			/// Helper method for calculating the position of a timeslot/activity based on a given start/end time.
			/// </summary>
			/// <param name="range">The range whose position is to be returned</param>
			/// <returns>The position or null if its not within the time slot range</returns>
			internal ItemPosition? GetItemPosition(DateRange range)
			{
				return GetItemPosition(range.Start, range.End);
			}

			/// <summary>
			/// Helper method for calculating the position of a timeslot/activity based on a given start/end time.
			/// </summary>
			/// <param name="start">The start of the range</param>
			/// <param name="end">The non-inclusive end of the range. Note if its the start value then it will be considered inclusive</param>
			/// <returns>The position or null if its not within the time slot range</returns>
			internal ItemPosition? GetItemPosition(DateTime start, DateTime end)
			{
				int startIndex, endIndex;

				// if its not in a timeslot then skip it
				if (!_panel.GetTimeslotIndex(start, end, out startIndex, out endIndex))
					return null;

				ItemPosition position = GetItemPosition(startIndex);
				DateRange range = _panel.GetTimeslotTime(startIndex);

				// use the start position of the timeslot
				double startOffset = position.Start;

				// if it starts after that ...
				if (start > range.Start)
				{
					long ticks = start.Subtract(range.Start).Ticks;
					startOffset += GetOffset(position.Extent, ticks);
				}

				// if this ends in a different slot then get its position and date range
				if (startIndex != endIndex)
				{
					position = GetItemPosition(endIndex);
					range = _panel.GetTimeslotTime(endIndex);
				}

				double endOffset = position.Start;

				if (end < range.End)
				{
					
					long ticks = end.Subtract(range.Start).Ticks;
					endOffset += GetOffset(position.Extent, ticks);
				}
				else
				{
					// use the end position of the timeslot
					endOffset += position.Extent;
				}

				return new ItemPosition(startOffset, endOffset - startOffset);
			} 
			#endregion // GetItemPosition(DateTime, DateTime)

			#region GetOffset
			private double GetOffset(double extent, long offsetTicks)
			{
				double delta = (double)offsetTicks / (double)TimeslotIntervalTicks * extent;

				if (_useLayoutRounding)
					delta = Math.Round(delta);

				return delta;
			} 
			#endregion // GetOffset

			#region GetTimeslotIndex
			internal int GetTimeslotIndex(double offset)
			{
				if (CoreUtilities.LessThanOrClose(offset, 0))
					return 0;
				else if (CoreUtilities.GreaterThanOrClose(offset, _fullExtent))
					return _timeslotCount - 1;

				int index = 0;
				double originalOffset = offset;
				double fullTimeslotExtent = _timeslotExtent + _interItemSpacing;

				if (_increasedItems > 0)
				{
					// find out where the end point is for the increased items
					double offsetToIncreasedItems = _increasedItems * (fullTimeslotExtent + 1);

					if (offsetToIncreasedItems <= offset)
					{
						index = _increasedItems++;
						offset -= offsetToIncreasedItems;
					}
				}

				index += (int)Math.Floor(offset / fullTimeslotExtent);

				// since we are doing a floor we can have rounding issues so we'll verify that 
				// the calculated index contains the offset and if we chose a slot just before 
				// it return the next one (assuming there is one)
				if ( CoreUtilities.GreaterThanOrClose(originalOffset, this.GetItemPosition(index).End) )
					index = Math.Min(_timeslotCount - 1, index + 1);

				Debug.Assert(index >= 0 && index < _timeslotCount, "Invalid timeslot index calculated");
				Debug.Assert(this.GetItemPosition(index).IntersectsWith(new ItemPosition(offset, double.Epsilon)), "The item position for the calculated index doesn't contain the offset?");

				return index;
			}
			#endregion // GetTimeslotIndex

			#region Initialize
			internal void Initialize()
			{
				this.SetField(ref _timeslotExtent, _panel.ActualTimeslotExtent);
				this.SetField(ref _timeslotCount, _panel.TimeslotCount);
				// AS 4/20/11 TFS73203
				//this.SetField(ref _increasedItems, 0); // _panel.IncreasedItems; TODO
				this.SetField(ref _increasedItems, _panel.IncreasedItems);
				this.SetField(ref _fullExtent, _timeslotExtent * _timeslotCount + _increasedItems + (Math.Max(0, _timeslotCount - 1) * _interItemSpacing));
				this.SetField(ref _useLayoutRounding, _panel.UseLayoutRounding);
				this.SetField(ref TimeslotIntervalTicks, _panel.TimeslotInterval.Ticks);
				this.SetField(ref _interItemSpacing, _panel.InterItemSpacing);

				long ticksPerPixel = _timeslotExtent < 1 ? 0 : (long)Math.Round((decimal)TimeslotIntervalTicks / (decimal)_timeslotExtent);
				this.SetField(ref EstimatedTicksPerPixel, ticksPerPixel);
			}
			#endregion // Initialize

			#region SetField
			private void SetField<T>(ref T field, T value)
			{
				if (!object.Equals(value, field))
				{
					field = value;
					_version++;
				}
			} 
			#endregion // SetField

			#region TryGetItemPosition
			internal bool TryGetItemPosition(DateTime start, DateTime end, out ItemPosition position)
			{
				ItemPosition? pos = GetItemPosition(start, end);

				if (pos != null)
					position = pos.Value;
				else
					position = new ItemPosition();

				return pos != null;
			}
			#endregion // TryGetItemPosition

			#endregion // Methods
		} 
		#endregion // TimeslotPositionInfo class
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