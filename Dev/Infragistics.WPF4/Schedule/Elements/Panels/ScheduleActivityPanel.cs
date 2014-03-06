using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.Collections;
using Infragistics.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Input;

namespace Infragistics.Controls.Schedules.Primitives
{
	
	

	/// <summary>
	/// A custom panel for arranging activity based on a set of time slots
	/// </summary>
	public class ScheduleActivityPanel : TimeslotPanelBase
		, IPropertyChangeListener
		, IReceivePropertyChange<object>
	{
		#region Member Variables

		private List<ActivityIsland> _islands;
		private List<ActivityItem> _items;

		// right now this is the default for dayview's timeslots. for everything else
		// we'll set that based on the heights of some elements (e.g. moreactivityindicator)
		internal const double DefaultGutterExtent = 10;

		private double _minEmptySize = DefaultGutterExtent;

		private double _preferredNonScrollingExtent = double.NaN; // AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight

		private TimeslotPositionInfo _positionInfo;
		private int _verifiedPositionInfoVersion;

		// outlook always makes the appointments 3 pixels or taller (top/bottom border plus 1 pixel of content)
		private const double MinActivityExtent = 3;
		private const double MinColumnExtent = 5;

		private ClickToAddHelper _clickToAddHelper;

		private Size _lastDesiredSize;

		private List<MoreActivityIndicator> _timeslotIndicators;

		private MoreActivityIndicator _nearIndicator;
		private MoreActivityIndicator _farIndicator;

		private ScrollInfo _activityScrollInfo;

		private ScrollInfo _activityScrollOffsetProvider;
		private ClickToAddType _clickToAddType;

		private InternalFlags _flags;
		private DateRange? _logicalDayRange;
		private long? _logicalDayDurationInTicks; // AS 1/13/12 TFS80802

		private bool _usesLayoutRounding; // AS 4/1/11 TFS43004

		// AS 5/9/12 TFS104555
		private Size _lastArrangeSize = Size.Empty;
		private int _infiniteExtentFullItemCount;
		private double _lastInfiniteFullItemExtent;

		#endregion // Member Variables

		#region Constructor
		static ScheduleActivityPanel()
		{

			DataContextHelper.RegisterType(typeof(ScheduleActivityPanel));

		}

		/// <summary>
		/// Initializes a new <see cref="ScheduleActivityPanel"/>
		/// </summary>
		public ScheduleActivityPanel()
		{




			_items = new List<ActivityItem>();
			_positionInfo = new TimeslotPositionInfo(this);
		} 
		#endregion // Constructor

		#region Base class overrides

		#region ArrangeOverrideImpl
		internal override Size ArrangeOverrideImpl(Size finalSize)
		{
			// AS 5/9/12 TFS104555
			if (!_lastArrangeSize.IsEmpty 
				&& _infiniteExtentFullItemCount > 0 
				&& !CoreUtilities.AreClose(finalSize, _lastArrangeSize)
				&& !CoreUtilities.AreClose(_lastInfiniteFullItemExtent, (this.IsVertical ? finalSize.Width : finalSize.Height) - _minEmptySize)
				)
				this.InvalidateMeasure();

			_lastArrangeSize = finalSize; // AS 5/9/12 TFS104555

			DebugHelper.DebugLayout(this, false, true, "ArrangeOverride Start", "FinalSize: {0}", finalSize);

			// out of view items
			PanelHelper.ArrangeReleasedElements(this);

			bool isVertical = this.IsVertical;

			// get the non-arrangement orientation extent
			double availableColumnExtent = this.GetAvailableColumnExtent(finalSize);

			Rect arrangeRect = Rect.Empty;

			#region Arrange items

			int firstColumn = 0;
			int maxColumnsInView = -1;

			if (_items.Count > 0)
			{
				DebugHelper.DebugLayout(this, false, true, "Items Start", null);

				Rect firstTimeslotRect = PanelHelper.GetStartingArrangeRect(isVertical, finalSize, this.ActualTimeslotExtent, this.InterItemSpacing, this.FirstTimeslotIndex, this.TimeslotCount);
				double offset = isVertical ? firstTimeslotRect.Y : firstTimeslotRect.X;
				Rect itemRect = firstTimeslotRect;

				if (GetFlag(InternalFlags.IsActivityScrollInfoEnabled))
				{
					if (_activityScrollOffsetProvider != null)
						firstColumn = (int)_activityScrollOffsetProvider.Offset;
					else
						firstColumn = (int)_activityScrollInfo.Offset;

					maxColumnsInView = (int)_activityScrollInfo.Viewport;
				}

				for (int i = 0, count = _items.Count; i < count; i++)
				{
					ActivityItem item = _items[i];
					ActivityBaseAdapter adapter = item.Adapter;
					ItemPosition itemPosition = item.DisplayPosition ?? item.Position;
					double topOffset = itemPosition.Start + offset;
					Size itemSize = this.GetMeasureSize(item, isVertical, availableColumnExtent);
					Debug.Assert(!double.IsPositiveInfinity(itemSize.Width) && !double.IsPositiveInfinity(itemSize.Height), "ItemSize has infinity in arrange");

					FrameworkElement container = ((ISupportRecycling)adapter).AttachedElement;

					if (double.IsPositiveInfinity(itemSize.Width))
						itemSize.Width = container.DesiredSize.Width;

					if (double.IsPositiveInfinity(itemSize.Height))
						itemSize.Height = container.DesiredSize.Height;

					double columnOffset = (item.Column - firstColumn) * (isVertical ? itemSize.Width : itemSize.Height);

					Rect rect = new Rect(
						isVertical ? columnOffset : topOffset,
						isVertical ? topOffset : columnOffset,
						itemSize.Width,
						itemSize.Height);

					container.Arrange(rect);

					DebugHelper.DebugLayout(container, "Arranging Child", "ItemRect:{0}", rect);

					arrangeRect.Union(rect);
				}

				DebugHelper.DebugLayout(this, true, false, "Items End", null);
			}
			#endregion // Arrange items

			#region Position Timeslot MoreActivityIndicators
			if (null != _timeslotIndicators)
			{
				ArrangeTimeslotIndicators(ref finalSize, isVertical, ref arrangeRect);
			} 
			#endregion // Position Timeslot MoreActivityIndicators

			#region Position Near/Far Indicators

			ArrangeNearFarIndicator(ref finalSize, isVertical, ref arrangeRect); 

			#endregion // Position Near/Far Indicators

			if (_clickToAddHelper != null)
				_clickToAddHelper.Arrange(finalSize, firstColumn, maxColumnsInView, availableColumnExtent, ref arrangeRect);

			// to ensure the activity is clipped to the panel union the rects of the arranged 
			// portions with that of the available
			arrangeRect.Union(new Rect(0, 0, finalSize.Width, finalSize.Height));

			Size arrangeSize = new Size(arrangeRect.Width, arrangeRect.Height);

			DebugHelper.DebugLayout(this, true, false, "ArrangeOverride End", "FinalSize: {0}, ArrangeSize:{1}", finalSize, arrangeSize);

			return arrangeSize;
		}
		#endregion // ArrangeOverrideImpl

		#region MeasureOverrideImpl
		internal override Size MeasureOverrideImpl(Size availableSize, int firstItemIndex, int pageSize)
		{
			DebugHelper.DebugLayout(this, false, true, "MeasureOverride Start", "AvailableSize: {0}", availableSize);

			_infiniteExtentFullItemCount = 0; // AS 5/9/12 TFS104555
			_usesLayoutRounding = this.UseLayoutRounding; // AS 4/1/11 TFS43004

			Size desired = new Size();
			bool isVertical = this.IsVertical;
			double timeslotExtent = this.ActualTimeslotExtent;

			// AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
			if ( !double.IsNaN(_preferredNonScrollingExtent) )
			{
				if ( isVertical )
					availableSize.Width = Math.Min(_preferredNonScrollingExtent, availableSize.Width);
				else
					availableSize.Height = Math.Min(_preferredNonScrollingExtent, availableSize.Height);
			}

			// helper object that caches some size information for position calculations
			TimeslotPositionInfo positionInfo = _positionInfo;

			// be sure it reinitialize it before the islands are requested
			positionInfo.Initialize();

			// the collection of activities broken up into blocks of overlapping activity
			List<ActivityIsland> islands = GetActivityIslands(positionInfo);
			int islandCount = islands.Count;
			int originalFirstTimeslotIndex = firstItemIndex;
			int lastItemIndex = firstItemIndex;

			// AS 5/9/12 TFS104555
			var isInfiniteExtent = double.IsInfinity(isVertical ? availableSize.Width : availableSize.Height);
			var deferredFullExtentContainers = isInfiniteExtent ? new List<ActivityItem>() : null;

			// comparer used to find an island based on an ItemPosition
			IslandBinaryComparer comparer = new IslandBinaryComparer();

			// the list of all activities
			var activities = (IList<ActivityBase>)_activityProvider ?? new ActivityBase[0];

			var ctrl = ScheduleUtilities.GetControl(this);

			if (ctrl is XamMonthView)
				this.SetFlag(InternalFlags.AlwaysUseSingleLine, true);

			// get the non-arrangement orientation extent
			double availableColumnExtent = this.GetAvailableColumnExtent(availableSize);

			#region Calculate Island In View Info

			ItemPosition inViewPosition = new ItemPosition();
			int firstIslandIndex = -1;
			int lastIslandIndex = -1;

			ItemPosition? constrainedInViewPosition = null;

			if (pageSize > 0)
			{
				double availableExtent = isVertical ? availableSize.Height : availableSize.Width;

				// use the same routine as the timeslotpanel to find out the first and last timeslot that is considered in view
				PanelHelper.GetStackStartAndEnd(availableSize, firstItemIndex, pageSize, isVertical, timeslotExtent, this.InterItemSpacing, false, this.TimeslotCount, out firstItemIndex, out lastItemIndex);

				// find the islands that intersect this range
				ItemPosition startPosition = positionInfo.GetItemPosition(firstItemIndex);
				comparer.Position = new ItemPosition(startPosition.Start, 0);
				firstIslandIndex = islands.BinarySearch(null, comparer);

				ItemPosition endPosition = positionInfo.GetItemPosition(lastItemIndex);
				comparer.Position = new ItemPosition(endPosition.End, 0);
				lastIslandIndex = islands.BinarySearch(null, comparer);

				// calculate the total in view range
				inViewPosition = startPosition;
				inViewPosition.Union(endPosition);

				if (GetFlag(InternalFlags.ConstrainToInViewRect))
				{
					Size finalSize = availableSize;
					
					if ( (isVertical && !double.IsPositiveInfinity(finalSize.Height)) || (!isVertical && !double.IsPositiveInfinity(finalSize.Width)) )
					{
						Rect firstTimeslotRect = PanelHelper.GetStartingArrangeRect(isVertical, availableSize, timeslotExtent, this.InterItemSpacing, this.FirstTimeslotIndex, this.TimeslotCount);
						double offset = -(isVertical ? firstTimeslotRect.Y : firstTimeslotRect.X);
						double extent = isVertical ? availableSize.Height : availableSize.Width;
						constrainedInViewPosition = new ItemPosition(offset, extent);
					}
				}
			} 

			#endregion // Calculate Island In View Info

			#region Setup Timeslot Indicator Info

			bool showTimeslotIndicators = !isVertical && islandCount > 0; 
			int firstMajorStartIndex = firstItemIndex;
			int firstIndicatorIslandIndex = firstIslandIndex;

			// if we're going to show out of view indicators in the timeslots then we need to get the start of the 
			// major timeslot containing the firstTimeslotIndex since the last major will show an indicator if 
			// any timeslot in that range has out of view activity including ones we are not positioning
			if (showTimeslotIndicators)
			{
				bool isFirstInDay, isLastInDay, isFirstInMajor, isLastInMajor;

				while (firstMajorStartIndex >= 0)
				{
					this.GetIsFirstLastState(firstMajorStartIndex, this.GetTimeslotTime(firstMajorStartIndex).Start, out isFirstInDay, out isLastInDay, out isFirstInMajor, out isLastInMajor);

					if (isFirstInMajor)
						break;

					firstMajorStartIndex--;
				}

				if (firstMajorStartIndex < firstItemIndex)
				{
					ItemPosition startPosition = positionInfo.GetItemPosition(firstMajorStartIndex);
					comparer.Position = new ItemPosition(startPosition.Start, 0);
					firstIndicatorIslandIndex = islands.BinarySearch(null, comparer);
				}
			} 
			#endregion // Setup Timeslot Indicator Info

			double minColExtent = _hasColumnExtent ? _activityColumnExtent : MinColumnExtent;
			int maxColumnsInView = (int)Math.Floor(availableColumnExtent / minColExtent);
			int firstColumnInView = 0;
			int maxColumnCount = 0;
			int maxIndicatorColumnCount = 0;

			if (GetFlag(InternalFlags.IsActivityScrollInfoEnabled))
			{
				// if we're supporting scrolling then make sure we calculate the column count for 
				// all the islands
				for (int i = 0; i < islandCount; i++)
				{
					_islands[i].VerifyColumnInfo();

					maxColumnCount = Math.Max(maxColumnCount, _islands[i].ColumnCount);
				}

				maxIndicatorColumnCount = maxColumnCount;

				// AS 5/9/12 TFS104555
				if (double.IsInfinity(availableColumnExtent))
					maxColumnsInView = maxColumnCount;

				if (_activityScrollInfo != null)
				{
					_activityScrollInfo.Initialize(maxColumnsInView, maxColumnCount, _activityScrollInfo.Offset);

					if (null != _activityScrollOffsetProvider)
						firstColumnInView = (int)_activityScrollOffsetProvider.Offset;
					else
						firstColumnInView = (int)_activityScrollInfo.Offset;
				}
			}

			
			int lastColumnInView = firstColumnInView + maxColumnsInView - 1;


			#region Release Elements Out Of Range

			DebugHelper.DebugLayout(this, false, true, "Measure - Release Elements Start", null);

			// try to reuse the adapters if possible
			List<ActivityBaseAdapter> availableAdapters = ReleaseElementsOutOfRange(isVertical, activities, availableColumnExtent, firstColumnInView, lastColumnInView, inViewPosition);

			DebugHelper.DebugLayout(this, true, false, "Measure - Release Elements End", null);

			#endregion // Release Elements Out Of Range

			if (islandCount > 0)
			{
				// if there is at least one overlapping island...
				if (firstIndicatorIslandIndex >= 0 || lastIslandIndex >= 0 || firstIndicatorIslandIndex != lastIslandIndex)
				{
					var selection = ctrl != null ? ctrl.SelectedActivities : null;
					bool hasSelection = selection != null && selection.Count > 0;
					var token = this.TimeZoneInfoProvider.LocalToken;

					if (firstIslandIndex < 0)
						firstIslandIndex = ~firstIslandIndex;

					if (firstIndicatorIslandIndex < 0)
						firstIndicatorIslandIndex = ~firstIndicatorIslandIndex;

					if (lastIslandIndex < 0)
						lastIslandIndex = Math.Min(~lastIslandIndex, islandCount - 1);

					Debug.Assert(lastIslandIndex >= firstIndicatorIslandIndex, string.Format("Calculated end index {0} is before start {1}", lastIslandIndex, firstIslandIndex));
					Debug.Assert(lastIslandIndex == islandCount - 1 || CoreUtilities.LessThanOrClose(inViewPosition.End, islands[lastIslandIndex + 1].Position.Start), "End is after next start?");

					#region Measure Island

					var selectedCalendar = this.DataContext as ResourceCalendar;

					TimeSpan dayOffset = ScheduleUtilities.GetLogicalDayOffset(ctrl);
					DateRange thisLogicalDayRange = this.LogicalDayRange;
					var visDates = ctrl != null ? ctrl.VisibleDates : null;
					var calendarHelper = ScheduleUtilities.GetDateInfoProvider(ctrl).CalendarHelper;
					var dayOffsetSeconds = (int)dayOffset.TotalSeconds;

					DebugHelper.DebugLayout(this, false, true, "Measure Islands Start", null);

					// enumerate the islands in view
					for (int i = firstIndicatorIslandIndex; i <= lastIslandIndex; i++)
					{
						ActivityIsland island = islands[i];

						// verify the columns...
						island.VerifyColumnInfo();

						// keep separate track of the column extents for use in the indicator calculations 
						// since we may have an island that is not in view but that is part of the first "major"
						// section and therefore impacts whether we show an out of view activity indicator
						maxIndicatorColumnCount = Math.Max(maxIndicatorColumnCount, island.ColumnCount);

						if (i < firstIslandIndex)
							continue;

						maxColumnCount = Math.Max(maxColumnCount, island.ColumnCount);

						#region Measure Island Items

						DebugHelper.DebugLayout(this, false, true, "Measure Island Start", "Index={0}, Position={1}, ColumnCount={2}, ActivityCount={3}", i, island.Position, island.ColumnCount, island.ActivityCount);

						double minExtent = Math.Min(timeslotExtent, MinActivityExtent);

						// measure the activity in view...
						for (int j = 0, count = island.ActivityCount; j < count; j++)
						{
							ActivityItem item = island[j];

							ItemPosition itemPosition = item.Position;

							// if its not within the viewable area we can skip it for now
							if (!itemPosition.IntersectsWith(inViewPosition))
								continue;

							// if its not in the page in view then skip it for now
							if (item.Column > lastColumnInView || item.Column < firstColumnInView)
								continue;

							ActivityBase activity = item.Activity;
							ActivityBaseAdapter adapter = item.Adapter;

							Debug.Assert(adapter == null || _items.Contains(item), "The item is already associated with an adapter but isn't in the collection to be processed");

							if (adapter == null)
							{





								// reuse a recycled/released adapter from above
								if (availableAdapters.Count > 0)
								{
									int index = availableAdapters.Count - 1;
									adapter = availableAdapters[index];
									availableAdapters.RemoveAt(index);
									adapter.Initialize(activity);
								}
								else
								{
									// create a new one...
									adapter = new ActivityBaseAdapter(activity);
								}

								// cache a reference to the associated adapter
								item.Adapter = adapter;

								adapter.IsHiddenDragSource = item.GetFlag(ActivityItemFlags.IsHidden);
								adapter.IsSelected = hasSelection && selection.Contains(activity);

								// store this in the items we position
								_items.Add(item);
							}

							FrameworkElement container = ((ISupportRecycling)adapter).AttachedElement;

							if (container == null)
							{
								bool isNewlyRealized = RecyclingManager.Manager.AttachElement(adapter, this);
								container = ((ISupportRecycling)adapter).AttachedElement;
								Debug.Assert(null != container, "No element for item?");

								DebugHelper.DebugLayout(container, "Generating Child", "IsNewlyRealized:{0}, Activity:{1}", isNewlyRealized, activity);

								container.InvalidateMeasure();
								container.InvalidateArrange();
							}

							var presenter = container as ActivityPresenter;

							// AS 1/5/11 NA 11.1 Activity Categories
							bool clearIsInitializing = false;

							if (null != presenter)
							{
								// AS 1/5/11 NA 11.1 Activity Categories
								clearIsInitializing = !presenter.IsInitializing;
								presenter.IsInitializing = true;

								#region IsSingleLine

								bool isSingleLine;

								if (isVertical)
								{
									// for vertically arranged activity (like dayview timeslots), we only 
									// want to show it on a single line when it is the extent of one 
									// timeslot (or less)
									isSingleLine = this.IsSingleLineItemVertical(itemPosition, timeslotExtent);
								}
								else
								{
									// for horizonally arranged activity, we want it to 
									// display on a single line if it is at least 24 hours 
									// or it overlaps with another activity
									isSingleLine = this.IsSingleLineItemHorizontal(island.ColumnCount, activity.Duration);
								}

								item.SetFlag(ActivityItemFlags.IsSingleLineItem, isSingleLine);
								presenter.IsSingleLineDisplay = isSingleLine; 

								#endregion // IsSingleLine

								presenter.IsOwningCalendarSelected = activity.OwningCalendar == selectedCalendar;

								#region Notch
								// potentially show the notch for single line items
								ItemPosition? notchPosition = null;

								if (isVertical && isSingleLine)
								{
									// find out the position of the actual time of the activity. note there shouldn't 
									// be a need to fudge the time so i'm not using Get(Start|End)Date
									var range = ScheduleUtilities.ConvertFromUtc(token, activity);

									notchPosition = positionInfo.GetItemPosition(range);
								}

								if (notchPosition != null)
								{
									ItemPosition actualNotchPosition = notchPosition.Value;

									// impose a minimum extent for the notch
									if (actualNotchPosition.Extent < minExtent)
									{
										actualNotchPosition.Extent = minExtent;

										if (actualNotchPosition.End > _positionInfo.FullExtent)
											actualNotchPosition.Start = Math.Max(itemPosition.Start, _positionInfo.FullExtent - minExtent);
									}

									if (CoreUtilities.LessThan(itemPosition.Start, actualNotchPosition.Start))
										presenter.NotchOffset = actualNotchPosition.Start - itemPosition.Start;
									else
										presenter.NotchOffset = 0;

									if (CoreUtilities.GreaterThan(itemPosition.Extent, actualNotchPosition.Extent))
									{
										presenter.NotchExtent = actualNotchPosition.Extent;
									}
									else
									{
										Debug.Assert(CoreUtilities.AreClose(itemPosition.End, actualNotchPosition.End) || CoreUtilities.AreClose(itemPosition.Start, actualNotchPosition.Start) && itemPosition.End <= actualNotchPosition.End, "Item position and notch don't line up?");
										presenter.NotchExtent = 0;
									}
								}
								else
								{
									presenter.NotchExtent = 0;
									presenter.NotchOffset = 0;
								}
								#endregion // Notch

								presenter.IsMultiDay = item.GetFlag(ActivityItemFlags.IsMultiDay);
							}

							bool isNearInPanel = item.GetFlag(ActivityItemFlags.IsNearInPanel);
							bool isFarInPanel = item.GetFlag(ActivityItemFlags.IsFarInPanel);

							if (constrainedInViewPosition == null)
							{
								item.DisplayPosition = null;
							}
							else
							{
								double top = Math.Max(itemPosition.Start, constrainedInViewPosition.Value.Start);
								double bottom = Math.Min(itemPosition.End, constrainedInViewPosition.Value.End);

								item.DisplayPosition = new ItemPosition(top, Math.Max(0, bottom - top));

								if (CoreUtilities.GreaterThan(top, itemPosition.Start))
									isNearInPanel = false;

								if (CoreUtilities.LessThan(bottom, itemPosition.End))
									isFarInPanel = false;
							}

							if (null != presenter)
							{
								DateRange localRange = ScheduleUtilities.ConvertFromUtc(token, item.Activity);

								this.InitializeNeedsFromToIndicators(visDates, localRange, dayOffset, thisLogicalDayRange, item);

								if (dayOffset.Ticks != 0)
								{
									localRange.Start = calendarHelper.AddSeconds(localRange.Start, -dayOffsetSeconds);
									localRange.End = calendarHelper.AddSeconds(localRange.End, -dayOffsetSeconds);
								}

								bool spansDays = localRange.Start.Date != localRange.End.Date;
								item.SetFlag(ActivityItemFlags.SpansLogicalDays, spansDays);
								presenter.SpansLogicalDays = spansDays;

								presenter.IsNearInPanel = isNearInPanel;
								presenter.IsFarInPanel = isFarInPanel;
								presenter.NeedsFromIndicator = item.GetFlag(ActivityItemFlags.NeedsFromIndicator);
								presenter.NeedsToIndicator = item.GetFlag(ActivityItemFlags.NeedsToIndicator);
							}

							// AS 1/5/11 NA 11.1 Activity Categories
							if ( clearIsInitializing )
								presenter.IsInitializing = false;

							// now that we have initialized the is single line display we can get the measure size
							Size measureSize = this.GetMeasureSize(item, isVertical, availableColumnExtent);

							// AS 5/9/12 TFS104555
							//PanelHelper.MeasureElement(container, measureSize);
							if (isInfiniteExtent && double.IsInfinity(isVertical ? measureSize.Width : measureSize.Height))
								deferredFullExtentContainers.Add(item);
							else
								PanelHelper.MeasureElement(container, measureSize);

							DebugHelper.DebugLayout(container, "Measured Child", "MeasureSize:{0}, DesiredSize:{1}, Activity:{2}", measureSize, container.DesiredSize, activity);
						}

						DebugHelper.DebugLayout(this, true, false, "Measure Island End", "Index: {0}", i);

						#endregion // Measure Island Items
					}

					DebugHelper.DebugLayout(this, true, false, "Measure Islands End", null);

					#endregion // Measure Island
				}
			}

			#region Timeslot Indicators

			// out of view indicators
			List<MajorTimeslotRange> majorRanges = null;

			// if we have islands in view and there is at least 1 column out of view...
			if (showTimeslotIndicators && firstIslandIndex >= 0 && lastIslandIndex >= 0)
			{
				if (firstColumnInView > 0 || maxIndicatorColumnCount > maxColumnsInView)
				{
					majorRanges = GetTimeslotIndicatorRanges(positionInfo, islands, firstMajorStartIndex, firstIndicatorIslandIndex, lastIslandIndex, lastItemIndex, firstColumnInView, lastColumnInView);
				}
			}

			// use ranges to create/position indicators
			if (majorRanges != null || _timeslotIndicators != null)
				this.MeasureIndicators(majorRanges); 

			#endregion // Timeslot Indicators

			#region Near/Far Indicators

			// create/release near/far indicators
			bool? needNear, needFar;

			if (!isVertical || islandCount == 0)
				needFar = needNear = false;
			else
			{
				if (firstIslandIndex > 0)
					needNear = true;
				else
					needNear = null;

				if (lastIslandIndex >= 0 && lastIslandIndex < islandCount - 1)
					needFar = true;
				else
					needFar = null;
			}

			if (needNear == null || needFar == null)
			{
				Debug.Assert(pageSize > 0 || originalFirstTimeslotIndex >= 0, "PageSize <= 0 || no original first timeslotindex?");
				ItemPosition fullyInViewPosition = positionInfo.GetItemPosition(originalFirstTimeslotIndex);

				if (pageSize > 0)
					fullyInViewPosition.Union(positionInfo.GetItemPosition(originalFirstTimeslotIndex + pageSize - 1));

				// we need a near if the bottom of an activity is not in view
				if (needNear == null)
					needNear = islands[0].NeedsNearFarIndicator(fullyInViewPosition, true);

				// we need a far if the top of an activity is not in a fully visible timeslot
				if (needFar == null)
					needFar = islands[islandCount - 1].NeedsNearFarIndicator(fullyInViewPosition, false);
			}

			this.VerifyNearFarIndicator(true, needNear.Value, ref _nearIndicator, ref desired);
			this.VerifyNearFarIndicator(false, needFar.Value, ref _farIndicator, ref desired);

			#endregion // Near/Far Indicators

			if (null != _clickToAddHelper)
				_clickToAddHelper.Measure(availableSize, positionInfo, islands, comparer, firstColumnInView, lastColumnInView, availableColumnExtent);

			#region Debug Presenter Creation


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			#endregion // Debug Presenter Creation

			
			if (_hasColumnExtent)
			{
				// don't consider more than we can fit
				int columnCount = Math.Min(maxColumnsInView, maxColumnCount);

				// the default size for a day view all day event area has room for 1 activity
				if (!isVertical)
					columnCount = Math.Max(1, columnCount);

				if (isVertical)
					desired.Width = _activityColumnExtent * columnCount;
				else
					desired.Height = _activityColumnExtent * columnCount;
			}

			// AS 5/9/12 TFS104555
			if (null != deferredFullExtentContainers)
			{
				double desiredColumnExtent = isVertical ? desired.Width : desired.Height;
				double minArrangeExtent = _lastArrangeSize.IsEmpty ? desiredColumnExtent : Math.Max(desiredColumnExtent, (isVertical ? _lastArrangeSize.Width : _lastArrangeSize.Height) - _minEmptySize);

				_lastInfiniteFullItemExtent = minArrangeExtent;

				foreach (var item in deferredFullExtentContainers)
				{
					var measureSize = this.GetMeasureSize(item, isVertical, availableColumnExtent);
					var container = ((ISupportRecycling)item.Adapter).AttachedElement;

					if (null != container)
					{
						PanelHelper.MeasureElement(container, measureSize);

						// keep track of how many of these we had in case our arrange size changes 
						// after without a measure change
						_infiniteExtentFullItemCount++;

						var containerDesired = container.DesiredSize;

						if (isVertical && CoreUtilities.GreaterThan(containerDesired.Width, minArrangeExtent))
							measureSize.Width = minArrangeExtent;
						else if (!isVertical && CoreUtilities.GreaterThan(containerDesired.Height, minArrangeExtent))
							measureSize.Height = minArrangeExtent;
						else
							continue;

						// remeasure with the constrained size
						PanelHelper.MeasureElement(container, measureSize);
					}
				}
			}

			// sort the items so we can position them consistently
			CoreUtilities.SortMergeGeneric(_items, CoreUtilities.CreateComparer<ActivityItem>(CompareActivityItem));

			// add in the gutter space
			if (isVertical)
				desired.Width += _minEmptySize;
			else
			{
				// AS 11/9/10
				// This could cause us to return a size larger than we need. While this may 
				// be ok for some elements, in the case of a vertically arranged activity 
				// panel (horizontal timeslots) like monthview and the all day area, we need 
				// the real area so we can position the out of view activity indicators in 
				// view even if they have to overlap each other or the activity in view.
				// Don't return less than what we have already calculated for the desired 
				// but don't go over the available height when adding in the min empty space.
				//
				//desired.Height += _minEmptySize;
				desired.Height = Math.Min(Math.Max(availableSize.Height, desired.Height), desired.Height + _minEmptySize);
			}

			// AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
			if ( !double.IsNaN(_preferredNonScrollingExtent) )
			{
				if ( isVertical )
					desired.Width = Math.Max(desired.Width, availableSize.Width);
				else
					desired.Height = Math.Max(desired.Height, availableSize.Height);
			}

			if (null != ctrl && ctrl.ActivityBeingResized != null)
			{
				// track that we are using the previous desired size so we know which 
				// panels to dirty when the resize is complete.
				SetFlag(InternalFlags.UsedLastDesiredSize, desired != _lastDesiredSize);

				desired = _lastDesiredSize;
			}
			else
			{
				SetFlag(InternalFlags.UsedLastDesiredSize, false);
				_lastDesiredSize = desired;
			}

			DebugHelper.DebugLayout(this, true, false, "MeasureOverride End", "AvailableSize: {0}, Desired: {1}", availableSize, desired);

			if (GetFlag(InternalFlags.IsActivityScrollInfoEnabled))
			{
				ScrollInfo si = this.ActivityScrollInfo;
				si.Initialize(maxColumnsInView, maxColumnCount, firstColumnInView);
			}

			Debug.Assert((isVertical && !double.IsInfinity(desired.Width)) || (!isVertical && !double.IsInfinity(desired.Height)), "We should not return an infinite extent for the column extent!");

			return desired;
		}

		#endregion // MeasureOverrideImpl 

		#region OnTimeslotsChanged
		internal override void OnTimeslotsChanged(TimeslotCollection oldValue, TimeslotCollection newValue)
		{
			base.OnTimeslotsChanged(oldValue, newValue);

			_logicalDayDurationInTicks = null; // AS 1/13/12 TFS80802
			_logicalDayRange = null;
			this.InvalidateActivityIslands(false);
		} 
		#endregion // OnTimeslotsChanged

		#endregion // Base class overrides

		#region Properties

		#region ActivityColumnExtent

		private double _activityColumnExtent = double.PositiveInfinity;
		private bool _hasColumnExtent = false;

		internal double ActivityColumnExtent
		{
			get { return _activityColumnExtent; }
			set
			{
				if (value != _activityColumnExtent)
				{
					if (double.IsNaN(value) || double.IsNegativeInfinity(value))
						value = double.PositiveInfinity;
					else if (value < 1)
						value = 1;

					_activityColumnExtent = value;
					_hasColumnExtent = !double.IsPositiveInfinity(value);
					this.InvalidateMeasure();
				}
			}
		}

		#endregion // ActivityColumnExtent

		#region ActivityProvider
		private AdapterActivitiesProvider _activityProvider;

		internal AdapterActivitiesProvider ActivityProvider
		{
			get { return _activityProvider; }
			set
			{
				if (value != _activityProvider)
				{
					if (_activityProvider != null)
					{
						ScheduleUtilities.RemoveListener(_activityProvider, this);
						_activityProvider.SortComparer = null;
					}

					// AS 12/13/10 TFS61517
					var oldValue = _activityProvider;

					_activityProvider = value;

					if (_activityProvider != null)
					{
						ScheduleUtilities.AddListener(_activityProvider, this, true);

						// sort them as needed based on the panel settings
						_activityProvider.SortComparer = new ActivitySortComparer(this);
					}

					this.InvalidateActivityIslands(true);

					// AS 12/13/10 TFS61517
					if ( null != oldValue )
						this.ReleaseOldItems();
				}
			}
		} 
		#endregion // ActivityProvider

		#region ActivityScrollInfo
		internal ScrollInfo ActivityScrollInfo
		{
			get
			{
				Debug.Assert(GetFlag(InternalFlags.IsActivityScrollInfoEnabled), "Should not access ActivityScrollInfo if activity scrollinfo is not enabled");

				if (_activityScrollInfo == null)
					_activityScrollInfo = new ElementScrollInfo(this);

				return _activityScrollInfo;
			}
		} 
		#endregion // ActivityScrollInfo

		#region ActivityScrollOffsetProvider
		internal ScrollInfo ActivityScrollOffsetProvider
		{
			get { return _activityScrollOffsetProvider; }
			set
			{
				if (value != _activityScrollOffsetProvider)
				{
					ScheduleUtilities.ManageListenerHelper(ref _activityScrollOffsetProvider, value, this, true);
					this.InvalidateMeasure();
				}
			}
		}
		#endregion // ActivityScrollOffsetProvider

		#region AlignToTimeslot
		/// <summary>
		/// Returns or sets whether an activity is aligned to the leading and trailing edges of the associated timeslots.
		/// </summary>
		internal bool AlignToTimeslot
		{
			get { return GetFlag(InternalFlags.AlignToTimeslot); }
			set
			{
				if (value != AlignToTimeslot)
				{
					SetFlag(InternalFlags.AlignToTimeslot, value);
					_positionInfo.BumpVersion();
					this.InvalidateMeasure();
				}
			}
		} 
		#endregion // AlignToTimeslot

		#region ClickToAddType
		internal ClickToAddType ClickToAddType
		{
			get { return _clickToAddType; }
			set
			{
				if (value != _clickToAddType)
				{
					_clickToAddType = value;

					// if the state changes then release any current helper
					if (_clickToAddHelper != null)
					{
						_clickToAddHelper.Deactivate();

						if (value == ClickToAddType.None)
							_clickToAddHelper = null;
					}
				}
			}
		} 
		#endregion // ClickToAddType

		#region ConstrainToInViewRect
		internal bool ConstrainToInViewRect
		{
			get { return GetFlag(InternalFlags.ConstrainToInViewRect); }
			set
			{
				if (value != ConstrainToInViewRect)
				{
					SetFlag(InternalFlags.ConstrainToInViewRect, value);
					_positionInfo.BumpVersion();
					this.InvalidateMeasure();
				}
			}
		}
		#endregion // ConstrainToInViewRect

		#region IsActivityScrollInfoEnabled
		internal bool IsActivityScrollInfoEnabled
		{
			get { return GetFlag(InternalFlags.IsActivityScrollInfoEnabled); }
			set
			{
				if (value != IsActivityScrollInfoEnabled)
				{
					SetFlag(InternalFlags.IsActivityScrollInfoEnabled, value);
					this.InvalidateMeasure();
				}
			}
		}
		#endregion // IsActivityScrollInfoEnabled

		// AS 1/13/12 TFS80802
		#region LogicalDayDuration
		internal long LogicalDayDuration
		{
			get
			{
				if (_logicalDayDurationInTicks == null)
				{
					var ctrl = ScheduleUtilities.GetControl(this);

					if (null != ctrl)
						_logicalDayDurationInTicks = ctrl.TimeslotInfo.DayDuration.Ticks;
				}

				return _logicalDayDurationInTicks ?? TimeSpan.TicksPerDay;
			}
		} 
		#endregion //LogicalDayDuration

		#region LogicalDayRange
		internal DateRange LogicalDayRange
		{
			get
			{
				if (_logicalDayRange == null)
					_logicalDayRange = this.CalculateLogicalDayRange(ScheduleUtilities.GetControl(this));

				return _logicalDayRange.Value;
			}
		} 
		#endregion // LogicalDayRange

		#region MinEmptySpace
		/// <summary>
		/// The minimum amount of open space to leave between the activity and the edge of the panel in the orientation opposite to the timeslot orientation.
		/// </summary>
		internal double MinEmptySpace
		{
			get { return _minEmptySize; }
			set
			{
				if (value != _minEmptySize)
				{
					if ( double.IsInfinity(value) )
						value = DefaultGutterExtent;

					_minEmptySize = value;
					_positionInfo.BumpVersion();
					this.InvalidateMeasure();
				}
			}
		} 
		#endregion // MinEmptySpace

		// AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
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
					this.InvalidateMeasure();
				}
			}
		}
		#endregion // PreferredNonScrollingExtent

		#region SortByTimeslotCountFirst
		/// <summary>
		/// Used to indicate that elements that occupy multiple timeslots 
		/// </summary>
		internal bool SortByTimeslotCountFirst
		{
			get { return GetFlag(InternalFlags.SortByTimeslotCountFirst); }
			set
			{
				if (value != SortByTimeslotCountFirst)
				{
					SetFlag(InternalFlags.SortByTimeslotCountFirst, value);
					_positionInfo.BumpVersion();
					this.InvalidateMeasure();
				}
			}
		} 
		#endregion // SortByTimeslotCountFirst

		#region UsedLastDesiredSize
		internal bool UsedLastDesiredSize
		{
			get { return GetFlag(InternalFlags.UsedLastDesiredSize); }
		} 
		#endregion // UsedLastDesiredSize

		#region UseTimeslotIntervalAsMin
		internal bool UseTimeslotIntervalAsMin
		{
			get { return GetFlag(InternalFlags.UseTimeslotIntervalAsMin); }
			set 
			{ 
				if (value != UseTimeslotIntervalAsMin)
				{
					SetFlag(InternalFlags.UseTimeslotIntervalAsMin, value);
					_positionInfo.BumpVersion();
					this.InvalidateMeasure();
				}
			}
		} 
		#endregion // UseTimeslotIntervalAsMin

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region BringIntoView
		internal bool BringIntoView(ActivityBase activity, bool completelyInView = false)
		{
			// first make sure everything is up to date
			_positionInfo.Initialize();
			this.GetActivityIslands(_positionInfo);

			ActivityItem item = this.GetActivityItem(activity);

			if (null == item)
				return false;

			ScheduleControlBase ctrl = ScheduleControlBase.GetControl(this);
			Debug.Assert(null != ctrl, "Need state to process scroll operation");
			ScrollInfo actualScrollInfo = ctrl != null ? ctrl.GetTimeslotScrollInfo(this) : null;

			// first make sure the associated timeslots are in view
			if (actualScrollInfo != null)
			{
				// get the timeslot index for the start of the activity
				int startIndex = _positionInfo.GetTimeslotIndex(item.Position.Start);
				int firstScrollIndex = (int)Math.Ceiling(actualScrollInfo.Offset);

				// if the top is out of view above then bring it down
				if (startIndex != firstScrollIndex)
				{
					if ( startIndex < firstScrollIndex )
						actualScrollInfo.Offset = startIndex;
					else
					{
						int fullTimeslotsCount = (int)Math.Max(0, Math.Floor(actualScrollInfo.Viewport - 1));

						if ( completelyInView == false )
						{
							// if the start timeslot is not completely in view...
							if ( startIndex > firstScrollIndex + fullTimeslotsCount )
							{
								actualScrollInfo.Offset = startIndex - fullTimeslotsCount;
							}
						}
						else
						{
							int endIndex = Math.Max(startIndex, _positionInfo.GetTimeslotIndex(Math.Floor(item.Position.End)));

							if ( endIndex > firstScrollIndex + fullTimeslotsCount )
							{
								actualScrollInfo.Offset = (endIndex - startIndex > fullTimeslotsCount)
									? startIndex
									: endIndex - fullTimeslotsCount;
							}
						}
					}
				}
			}

			// then potentially scroll that column of appts into view
			if (GetFlag(InternalFlags.IsActivityScrollInfoEnabled))
			{
				// get the scroll info that we need to shift to bring the activity into view
				var activityScrollInfo = _activityScrollOffsetProvider ?? _activityScrollInfo;

				if (activityScrollInfo != null)
				{
					if (item.Column < activityScrollInfo.Offset)
						activityScrollInfo.Offset = item.Column;
					else if (item.Column >= activityScrollInfo.Offset + activityScrollInfo.Viewport)
						activityScrollInfo.Offset = item.Column + 1 - activityScrollInfo.Viewport;
				}
			}

			return true;
		}
		#endregion // BringIntoView

		#region GetActivityPresenter
		internal ActivityPresenter GetActivityPresenter(ActivityBase activity)
		{
			ActivityItem item = this.GetActivityItem(activity);
			ISupportRecycling adapter = item != null ? item.Adapter : null;

			return adapter != null ? adapter.AttachedElement as ActivityPresenter : null;
		}
		#endregion // GetActivityPresenter

		#region GetNextActivity
		internal ActivityBase GetNextActivity( ActivityBase activity, bool next, DateRange? rangeWithin, out bool containsActivity )
		{
			ActivityIsland island;
			int islandIndex;

			_positionInfo.Initialize();
			containsActivity = false;

			var islands = this.GetActivityIslands(_positionInfo);

			if (islands.Count == 0)
				return null;

			if (activity != null)
			{
				ActivityItem item = this.GetActivityItem(activity, out island, out islandIndex);

				if (item == null)
					return null;

				return this.GetNextActivityImpl(next, islands, islandIndex, rangeWithin, out containsActivity, startingItem: item);
			}
			else
			{
				islandIndex = next ? 0 : islands.Count - 1;
				island = islands[islandIndex];

				double startPos = island.Position.Start;
				startPos += next ? -1 : island.Position.Extent + 1;

				return this.GetNextActivityImpl(next, islands, islandIndex, rangeWithin, out containsActivity, startPosition: startPos);
			}
		}

		internal ActivityBase GetNextActivity( DateTime dateTime, DateRange? rangeWithin, bool next )
		{
			var timeslots = this.Timeslots;

			if (timeslots == null || timeslots.Count == 0)
				return null;

			_positionInfo.Initialize();
			var islands = this.GetActivityIslands(_positionInfo);

			if (islands.Count == 0)
				return null;

			int index = timeslots.BinarySearch(dateTime);
			ItemPosition? position;

			if (index >= 0)
			{
				position = _positionInfo.GetItemPosition(dateTime, dateTime);
			}
			else
			{
				index = ~index;

				if (index == 0)
					position = new ItemPosition(0, 0);
				else if (index >= timeslots.Count)
					position = new ItemPosition(_positionInfo.FullExtent, 0);
				else
				{
					var tempDate = timeslots.GetDateRange(index).Value.Start;
					position = _positionInfo.GetItemPosition(tempDate, tempDate);
				}
			}

			Debug.Assert(position != null, "Even though we adjusted the date time, we still didn't find a position?");

			if (position == null)
				return null;

			var comparer = new IslandBinaryComparer();
			comparer.Position = position.Value;
			int islandIndex = islands.BinarySearch(null, comparer);

			if (islandIndex < 0)
				islandIndex = Math.Min(~islandIndex, islands.Count - 1);

			bool foundItem;
			return GetNextActivityImpl(next, islands, islandIndex, rangeWithin, out foundItem, startPosition: comparer.Position.Start);
		}

		private ActivityBase GetNextActivityImpl( bool next, List<ActivityIsland> islands, int islandIndex, DateRange? rangeWithin, out bool foundStartingItem, double startPosition = double.NaN, ActivityItem startingItem = null)
		{
			foundStartingItem = false;
			int start = islandIndex;
			int end = next ? islands.Count : -1;
			int adjustment = next ? 1 : -1;
			var ctrl = ScheduleUtilities.GetControl(this);
			ActivateActivityHelper helper = new ActivateActivityHelper
			{
				_visibleDates = ctrl != null ? ctrl.VisibleDates : null,
				_next = next,
				_dayOffset = ScheduleUtilities.GetLogicalDayOffset(ctrl),
				_localToken = this.TimeZoneInfoProvider.LocalToken,
				_panelRange = this.LogicalDayRange,
				Position = startPosition,
				_startingItem = startingItem
			};

			if (null != rangeWithin)
			{
				helper._constrainingRange = _positionInfo.GetItemPosition(rangeWithin.Value);
				Debug.Assert(null != helper._constrainingRange, "Is it expected that the constraining range doesn't exist within the panel?");
			}

			for (int i = start; i != end; i += adjustment)
			{
				var activity = islands[i].GetActivity(next, helper.IsMatch);

				if ( activity != null )
				{
					foundStartingItem = helper.FoundStartingItem;
					return activity.Activity;
				}
			}

			foundStartingItem = helper.FoundStartingItem;
			return null;
		}
		#endregion // GetNextActivity

		#region HasIntersectingActivity
		internal bool HasIntersectingActivity(DateRange range, ActivityType? activityType)
		{
			TimeslotPositionInfo posInfo = _positionInfo;
			posInfo.Initialize();

			ItemPosition tsPosition;
			if (!posInfo.TryGetItemPosition(range.Start, range.End, out tsPosition))
			{
				return false;
			}

			foreach (ActivityIsland island in GetIslands(this.GetActivityIslands(posInfo), new IslandBinaryComparer(), range, posInfo))
			{
				foreach (ActivityItem item in island.GetIntersectingActivities(tsPosition))
				{
					if (activityType != null && item.Activity.ActivityType != activityType)
						continue;

					// AS 1/13/12 TFS80802
					// The IsMultiDay flag is based on the logical day duration so we should just consider that.
					//
					//if (item.Activity.Duration.Ticks < TimeSpan.TicksPerDay)
					if (!item.GetFlag(ActivityItemFlags.IsMultiDay))
					{
						return true;
					}
				}
			}

			return false;
		} 
		#endregion // HasIntersectingActivity

		#region OnActivityBeingDraggedVisibilityChanged
		internal void OnActivityBeingDraggedVisibilityChanged(ActivityBase activityBeingDragged, bool hide)
		{
			ActivityItem item = GetActivityItem(activityBeingDragged);

			if (item != null)
			{
				item.SetFlag(ActivityItemFlags.IsHidden, hide);

				if (item.Adapter != null)
					item.Adapter.IsHiddenDragSource = hide;
			}
		}
		#endregion // OnActivityBeingDraggedVisibilityChanged

		#region OnCurrentUserChanged
		internal void OnCurrentUserChanged()
		{
			foreach (var item in _items)
			{
				ActivityPresenter ap = ((ISupportRecycling)item.Adapter).AttachedElement as ActivityPresenter;
				ap.OnCurrentUserChanged();
			}
		}
		#endregion // OnCurrentUserChanged

		#region OnMouseEnterTimeslot
		internal void OnMouseEnterTimeslot(TimeRangePresenterBase presenter)
		{
			if (_clickToAddType == ClickToAddType.None)
				return;

			if (_clickToAddHelper == null)
				_clickToAddHelper = new ClickToAddHelper(this);

			_clickToAddHelper.OnEnterTimeslot(presenter);
		}
		#endregion // OnMouseEnterTimeslot

		#region OnMouseLeaveTimeslot
		internal void OnMouseLeaveTimeslot(TimeRangePresenterBase presenter)
		{
			if (_clickToAddHelper != null)
				_clickToAddHelper.OnLeaveTimeslot(presenter);
		}
		#endregion // OnMouseLeaveTimeslot

		#region OnVisibleDatesChanged
		internal void OnVisibleDatesChanged()
		{
			var ctrl = ScheduleUtilities.GetControl(this);
			var visDates = ctrl != null ? ctrl.VisibleDates : null;
			var token = ScheduleUtilities.GetTimeZoneInfoProvider(ctrl).LocalToken;
			var dayOffset = ScheduleUtilities.GetLogicalDayOffset(ctrl);
			var panelRange = this.LogicalDayRange;

			foreach (var item in _items)
			{
				DateRange localRange = ScheduleUtilities.ConvertFromUtc(token, item.Activity);

				this.InitializeNeedsFromToIndicators(visDates, localRange, dayOffset, panelRange, item);
			}
		}
		#endregion // OnVisibleDatesChanged

		#region VerifySelectionState
		internal void VerifySelectionState(ScheduleControlBase ctrl)
		{
			var selection = ctrl.SelectedActivities;
			bool hasSelection = selection.Count > 0;

			foreach (ActivityItem item in _items)
			{
				item.Adapter.IsSelected = hasSelection && selection.Contains(item.Activity);
			}
		}
		#endregion // VerifySelectionState

		#endregion // Internal Methods		

		#region Private Methods

		#region ArrangeNearFarIndicator
		private void ArrangeNearFarIndicator(ref Size finalSize, bool isVertical, ref Rect arrangeRect)
		{
			for (int i = 0; i < 2; i++)
			{
				bool near = i == 0;
				MoreActivityIndicator indicator = near ? _nearIndicator : _farIndicator;

				if (null == indicator)
					continue;

				Rect rect = new Rect(new Point(), indicator.DesiredSize);

				if (isVertical)
				{
					rect.Width = finalSize.Width;

					if (!near)
						rect.Y = finalSize.Height - rect.Height;
				}
				else
				{
					rect.Height = finalSize.Height;

					if (!near)
						rect.X = finalSize.Width - rect.Width;
				}

				indicator.Arrange(rect);

				arrangeRect.Union(rect);
			}
		}
		#endregion // ArrangeNearFarIndicator

		#region ArrangeTimeslotIndicators
		private void ArrangeTimeslotIndicators(ref Size finalSize, bool isVertical, ref Rect arrangeRect)
		{
			if (_timeslotIndicators == null)
				return;

			Debug.Assert(isVertical == false, "We shouldn't have timeslot indicators when vertical");
			Rect firstTimeslotRect = PanelHelper.GetStartingArrangeRect(isVertical, finalSize, this.ActualTimeslotExtent, this.InterItemSpacing, this.FirstTimeslotIndex, this.TimeslotCount);
			double offset = isVertical ? firstTimeslotRect.Y : firstTimeslotRect.X;

			foreach (MoreActivityIndicator indicator in _timeslotIndicators)
			{
				MajorTimeslotRange range = indicator.Context as MajorTimeslotRange;

				if (range == null)
					PanelHelper.ArrangeOutOfView(indicator);
				else
				{
					ItemPosition position = range.LastMajorPosition;
					Size itemSize = PanelHelper.GetOutOfViewMeasureSize(indicator);

					Rect indicatorRect = new Rect(
						position.Start + offset,
						indicator.Direction == MoreActivityIndicatorDirection.Up ? 0 : firstTimeslotRect.Height - itemSize.Height,
						position.Extent,
						itemSize.Height);

					indicator.Arrange(indicatorRect);

					arrangeRect.Union(indicatorRect);
				}
			}
		}
		#endregion // ArrangeTimeslotIndicators

		#region CalculateLogicalDayRange
		private DateRange CalculateLogicalDayRange(ScheduleControlBase ctrl)
		{
			var ts = this.Timeslots;
			var visDates = ctrl == null ? null : ctrl.VisibleDates;

			if (ts == null || visDates == null || ts.Count == 0)
				return new DateRange();

			TimeSpan dayOffset = ScheduleUtilities.GetLogicalDayOffset(ctrl);

			DateTime start = ScheduleUtilities.GetLogicalDayRange(ts.GetDateRange(0).Value.Start, dayOffset).Start;
			DateTime end = ScheduleUtilities.GetLogicalDayRange(ScheduleUtilities.GetNonInclusiveEnd(ts.GetDateRange(ts.Count - 1).Value.End), dayOffset).Start;

			return new DateRange(start, end);
		}
		#endregion // CalculateLogicalDayRange

		#region CalculateNeedsFromToIndicator
		private static void CalculateNeedsFromToIndicator( DateCollection visDates, DateRange localActivityRange, TimeSpan dayOffset, DateRange panelLogicalDayRange, ActivityItem item, out bool needsFromIndicator, out bool needsToIndicator )
		{
			needsFromIndicator = needsToIndicator = false;
			bool nearEdgeInPanel = item.GetFlag(ActivityItemFlags.IsNearInPanel);
			bool farEdgeInPanel = item.GetFlag(ActivityItemFlags.IsFarInPanel);

			if (!nearEdgeInPanel || !farEdgeInPanel)
			{
				if (null != visDates && visDates.Count > 0)
				{
					#region Near
					if (!nearEdgeInPanel)
					{
						DateTime logicalDay = localActivityRange.Start.Subtract(dayOffset).Date;
						int index = visDates.BinarySearch(logicalDay);

						// we only care if the end date is not in view
						if (index < 0)
						{
							index = Math.Min(visDates.Count - 1, ~index);

							// and then only if this object contains the closest date
							if (panelLogicalDayRange.Contains(logicalDay) || visDates[index] == panelLogicalDayRange.Start)
							{
								needsFromIndicator = true;
							}
						}
					}
					#endregion // Near

					#region Far
					if (!farEdgeInPanel)
					{
						DateTime logicalDay = ScheduleUtilities.GetNonInclusiveEnd(localActivityRange.End).Subtract(dayOffset).Date;
						int index = visDates.BinarySearch(logicalDay);

						// we only care if the end date is not in view
						if (index < 0)
						{
							index = Math.Min(visDates.Count - 1, ~index);

							// and then only if this object contains the closest date or this
							// is the closest visible
							if (panelLogicalDayRange.Contains(logicalDay) || visDates[index] == panelLogicalDayRange.End)
							{
								needsToIndicator = true;
							}
						}
					}
					#endregion // Far
				}
			}
		}
		#endregion // CalculateNeedsFromToIndicator 

		#region CompareActivityItem
		private static int CompareActivityItem(ActivityItem item1, ActivityItem item2)
		{
			if (item1 == item2)
				return 0;

			if (item1 == null)
				return -1;
			else if (item2 == null)
				return 1;

			ItemPosition pos1 = item1.Position;
			ItemPosition pos2 = item2.Position;

			if (pos1.Start < pos2.Start)
				return -1;
			else if (pos2.Start < pos1.Start)
				return 1;

			if (pos1.End > pos2.End)
				return -1;
			else if (pos1.End < pos2.End)
				return 1;

			// lastly sort by column
			return item1.Column.CompareTo(item2.Column);
		}
		#endregion // CompareActivityItem

		#region CreateActivityIslands
		private void CreateActivityIslands(TimeslotPositionInfo positionInfo)
		{
			List<ActivityIsland> islands = new List<ActivityIsland>();
			AdapterActivitiesProvider activityProvider = _activityProvider;

			if (null != activityProvider)
				activityProvider.VerifySort();

			if (activityProvider != null && activityProvider.Count > 0)
			{
				Dictionary<ActivityBase, ActivityItem> oldItems = null;

				if (_oldIslands != null)
				{
					oldItems = new Dictionary<ActivityBase,ActivityItem>();

					for (int i = 0, count = _oldIslands.Count; i < count; i++)
					{
						_oldIslands[i].GetActivityTable(oldItems);
					}
				}

				#region Build "Islands" of activity

				ActivityIsland currentIsland = new ActivityIsland(this);
				var dm = ScheduleUtilities.GetDataManager(this);

				// we weren't doing this previously but if we had a situation where an item was in view 
				// and then we got in here (outside measure) and it wasn't in view any more that we would 
				// pull it out. so when we dirtied the islands again and cached the old islands, the old 
				// islands would not have included the item that we have in the _items and so we would 
				// have created a new one instead. since we always want to try and reuse the items 
				// that we are using for positioning, we'll put these into the old list from which we 
				// pull out items when rebuilding the islands below
				if ( _items.Count > 0 )
				{
					if ( oldItems == null )
						oldItems = new Dictionary<ActivityBase, ActivityItem>();

					foreach ( ActivityItem item in _items )
					{
						Debug.Assert(!oldItems.ContainsKey(item.Activity) || oldItems[item.Activity] == item, "The old item isn't the same as the item in use?");
						oldItems[item.Activity] = item;
					}
				}

				foreach (ActivityBase activity in activityProvider)
				{
					ActivityItem item = null;

					if ( oldItems != null )
						oldItems.TryGetValue(activity, out item);

					this.CreateActivityItem(activity, positionInfo, dm, ref item);

					if (null != item)
					{
						if (!currentIsland.AddActivity(item))
						{
							// we know it has activity so keep it
							islands.Add(currentIsland);

							currentIsland = new ActivityIsland(this);
							currentIsland.AddActivity(item);
						}
					}
				}

				// if the last island we were working with has activity
				// then keep a reference to it
				if (currentIsland.ActivityCount > 0)
					islands.Add(currentIsland);

				#endregion // Build "Islands" of activity
			}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			_oldIslands = null;
			_islands = islands;
		}
		#endregion // CreateActivityIslands

		#region CreateActivityItem
		private ActivityItem CreateActivityItem(ActivityBase activity, TimeslotPositionInfo positionInfo)
		{
			ActivityItem item = null;
			var dm = ScheduleUtilities.GetDataManager(this);
			this.CreateActivityItem(activity, positionInfo, dm, ref item);
			return item;
		}

		private void CreateActivityItem(ActivityBase activity, TimeslotPositionInfo positionInfo, XamScheduleDataManager dm, ref ActivityItem item)
		{
			// get the range for the activity
			var token = this.TimeZoneInfoProvider.LocalToken;
			var localRange = ScheduleUtilities.ConvertFromUtc(token, activity);
			DateTime start = localRange.Start;
			DateTime end = localRange.End;

			Debug.Assert(end >= start, "Should the end of the activity be before the start?");

			if (end < start)
				end = start;

			int startIndex, endIndex;

			// if its not in a timeslot then skip it
			if (!this.GetTimeslotIndex(start, end, out startIndex, out endIndex))
			{
				if (item != null)
				{
					item.Island = null;
					item = null;
				}
				return;
			}

			DateTime tsStart = this.GetTimeslotTime(startIndex).Start;
			DateTime tsEnd = this.GetTimeslotTime(endIndex).End;

			bool nearEdgeInPanel = tsStart <= start;
			bool farEdgeInPanel = tsEnd >= end;
			// AS 1/13/12 TFS80802
			// We should consinder the logical day duration since this might be an all day event.
			//
			//bool isMultiDay = end.Subtract(start).Ticks >= TimeSpan.TicksPerDay;
			bool isMultiDay = end.Subtract(start).Ticks >= this.LogicalDayDuration;

			// if we need to align to the timeslot then update the start date and end date
			// to align with the containing timeslot
			if (GetFlag(InternalFlags.AlignToTimeslot))
			{
				start = tsStart;
				end = tsEnd;
			}
			else
			{
				// constrain to the overlapping timeslots
				if (start < tsStart)
					start = tsStart;

				if (end > tsEnd)
					end = tsEnd;
			}

			Debug.Assert(item == null || item.Activity == activity, "The item has a different activity?");

			if (item == null)
				item = new ActivityItem { Activity = activity };

			ItemPosition position = positionInfo.GetItemPosition(start, end).Value;

			double minExtent = Math.Min(this.ActualTimeslotExtent, this.PreferredTimeslotExtent);

			// if the owner wants an activity to be at least as tall as the timeslot then massage the end date
			// we will also force this for the case of journal items since they are always 0 minute activity
			bool useMin = false;

			if (CoreUtilities.LessThan(position.Extent, minExtent))
			{
				if (GetFlag(InternalFlags.UseTimeslotIntervalAsMin))
					useMin = true;
				else if ( activity is Journal )
				{
					// we decided that since journals are a special type of activity 
					// that is likely to be used as a 0 minute activity and they 
					// may well use our journal entity object (in which case end 
					// would be supported by default) that we would still make them
					// at least 1 timeslot wide
					useMin = true;
				}
				else if ( end <= start )
				{
					if ( null != dm && !dm.IsEndDateSupportedByActivity(activity) )
						useMin = true;
				}

				if (useMin)
					position.Extent = minExtent;
			}


			if (!useMin && position.Extent < MinActivityExtent)
			{
				// the item needs to be at least as tall as the min extent...
				position.Extent = Math.Min(this.ActualTimeslotExtent, MinActivityExtent);
			}

			// make sure its in view within the panel
			if (position.End > positionInfo.FullExtent)
				position.Start = Math.Max(positionInfo.FullExtent - position.Extent, 0);

			// AS 11/15/10 - AlignToTimeslot Optimization
			item.StartTimeslotIndex = startIndex;
			item.EndTimeslotIndex = endIndex;

			item.Position = position;
			item.SetFlag(ActivityItemFlags.IsNearInPanel, nearEdgeInPanel);
			item.SetFlag(ActivityItemFlags.IsFarInPanel, farEdgeInPanel);
			item.SetFlag(ActivityItemFlags.NeedsFromIndicator, false);
			item.SetFlag(ActivityItemFlags.NeedsToIndicator, false);
			item.DisplayPosition = null;
			item.SetFlag(ActivityItemFlags.IsMultiDay, isMultiDay);
		}
		#endregion // CreateActivityItem

		#region GetActivityItem
		private ActivityItem GetActivityItem( ActivityBase activity )
		{
			ActivityIsland island;
			int islandIndex;

			return GetActivityItem( activity, out island, out islandIndex );
		}

		private ActivityItem GetActivityItem(ActivityBase activity, out ActivityIsland island, out int islandIndex)
		{
			island = null;
			islandIndex = -1;

			if (_activityProvider != null && activity != null)
			{
				if (_activityProvider.CouldContainActivity(activity))
				{
					if (this.Children.Count > 0)
					{
						List<ActivityIsland> islands = GetActivityIslands(_positionInfo);

						if (null != islands)
						{
							ActivityItem tempItem = this.CreateActivityItem(activity, _positionInfo);

							if (null != tempItem)
							{
								IslandBinaryComparer comparer = new IslandBinaryComparer();
								comparer.Position = tempItem.Position;
								islandIndex = islands.BinarySearch(null, comparer);

								if (islandIndex >= 0 && islandIndex < islands.Count)
								{
									island = islands[islandIndex];
									return island.GetItem(activity);
								}
							}
						}
					}
				}
			}

			return null;
		}
		#endregion // GetActivityItem

		#region GetActivityIslands
		private List<ActivityIsland> GetActivityIslands(TimeslotPositionInfo positionInfo)
		{
			Debug.Assert(positionInfo == _positionInfo, "This is not the position info used by the panel");

			if (positionInfo.Version != _verifiedPositionInfoVersion)
			{
				_verifiedPositionInfoVersion = positionInfo.Version;
				this.InvalidateActivityIslands(false);
			}

			// the activity provider may have changes queued which when processed would dirty 
			// the islands so make sure it processes those first so we build the islands from the 
			// latest info
			if ( _activityProvider != null )
				_activityProvider.ProcessPendingOperations();

			if ( null == _islands )
			{
				this.CreateActivityIslands(positionInfo);
			}

			return _islands;
		}
		#endregion // GetActivityIslands

		// AS 11/9/10
		// Created a helper method since the measure/arrange both need the same calculation.
		//
		#region GetAvailableColumnExtent
		private double GetAvailableColumnExtent( Size size )
		{
			// get the non-arrangement orientation extent
			double availableColumnExtent = this.IsVertical ? size.Width : size.Height;

			// remove the amount that is reserved
			availableColumnExtent = Math.Max(availableColumnExtent - _minEmptySize, 0);

			// AS 11/9/10
			// If we have a column extent then always position at least 1 item. Otherwise 
			// you won't see any activity.
			//
			if ( _hasColumnExtent )
				availableColumnExtent = Math.Max(availableColumnExtent, _activityColumnExtent);

			return availableColumnExtent;
		}
		#endregion // GetAvailableColumnExtent

		#region GetFlag
		/// <summary>
		/// Returns true if any of the specified bits are true.
		/// </summary>
		/// <param name="flag">Flag(s) to evaluate</param>
		/// <returns></returns>
		private bool GetFlag(InternalFlags flag)
		{
			return (_flags & flag) != 0;
		}
		#endregion // GetFlag

		#region GetIslands
		private static IEnumerable<ActivityIsland> GetIslands(List<ActivityIsland> islands, IslandBinaryComparer islandComparer, DateRange range, TimeslotPositionInfo positionInfo)
		{
			ItemPosition? timeslotPos = positionInfo.GetItemPosition(range.Start, range.End);

			if (timeslotPos == null)
				yield break;

			// walk over the islands that intersect the range
			ItemPosition position = timeslotPos.Value;
			islandComparer.Position = new ItemPosition(position.Start, 0);
			int firstIslandIndex = islands.BinarySearch(null, islandComparer);

			if (firstIslandIndex < 0)
				firstIslandIndex = ~firstIslandIndex;

			for (int i = firstIslandIndex, count = islands.Count; i < count; i++)
			{
				ActivityIsland island = islands[i];
				if (CoreUtilities.LessThanOrClose(island.Position.End, position.Start))
					continue;

				// break out once we hit an island the timeslot doesn't overlap with
				if (CoreUtilities.GreaterThanOrClose(island.Position.Start, position.End))
					break;

				yield return island;
			}
		}
		#endregion // GetIslands

		#region GetMeasureSize
		private Size GetMeasureSize(bool isSingleLineItem, int columnCount, int columnSpan, ItemPosition itemPosition, bool isVertical, double availableColumnExtent)
		{
			Debug.Assert(columnSpan >= 0, "Invalid column index?");

			double otherExtent;

			if (availableColumnExtent < 1)
				otherExtent = 0;
			else if (_hasColumnExtent && isSingleLineItem)
				otherExtent = _activityColumnExtent;
			else
				otherExtent = Math.Max(MinColumnExtent, availableColumnExtent / columnCount);

			// AS 4/1/11 TFS43004
			if (_usesLayoutRounding)
				otherExtent = Math.Floor(otherExtent);

			otherExtent *= columnSpan;

			return new Size(
				isVertical ? otherExtent : itemPosition.Extent,
				isVertical ? itemPosition.Extent : otherExtent
				);
		}

		private Size GetMeasureSize(ActivityItem item, bool isVertical, double availableColumnExtent)
		{
			return GetMeasureSize(item.GetFlag(ActivityItemFlags.IsSingleLineItem), item.ColumnCount, 1, item.DisplayPosition ?? item.Position, isVertical, availableColumnExtent);
		}
		#endregion // GetMeasureSize

		#region GetNextIndicator
		private MoreActivityIndicator GetNextIndicator(ref int nextIndicatorIndex)
		{
			if (_timeslotIndicators == null)
				_timeslotIndicators = new List<MoreActivityIndicator>();

			MoreActivityIndicator indicator;

			if (nextIndicatorIndex >= _timeslotIndicators.Count)
			{
				indicator = new MoreActivityIndicator();
				indicator.Action = this.PerformTimeslotIndicatorAction;
				Canvas.SetZIndex(indicator, 5);
				this.Children.Add(indicator);
				_timeslotIndicators.Add(indicator);
			}
			else
			{
				indicator = _timeslotIndicators[nextIndicatorIndex];
			}

			nextIndicatorIndex++;
			return indicator;
		}
		#endregion // GetNextIndicator

		#region GetTimeslotIndicatorRanges
		private List<MajorTimeslotRange> GetTimeslotIndicatorRanges(TimeslotPositionInfo positionInfo, List<ActivityIsland> islands,
			int firstMajorStartIndex, int firstIslandIndex,
			int lastIslandIndex, int lastItemIndex,
			int firstColumnInView, int lastColumnInView)
		{
			#region Get List of Major Sections

			var majorRanges = new List<MajorTimeslotRange>();
			bool isFirstInDay, isLastInDay, isFirstInMajor, isLastInMajor;
			int majorStartIndex = firstMajorStartIndex;
			DateTime startDate = DateTime.MinValue;

			for (int i = firstMajorStartIndex; i <= lastItemIndex; i++)
			{
				// need the ItemPosition for the last major timeslots
				// need the ItemPosition for all the timeslots starting with the first major
				DateRange timeslotRange = this.GetTimeslotTime(i);
				this.GetIsFirstLastState(i, timeslotRange.Start, out isFirstInDay, out isLastInDay, out isFirstInMajor, out isLastInMajor);

				if (isFirstInMajor)
				{
					startDate = timeslotRange.Start;
					majorStartIndex = i;
				}

				if (isLastInMajor)
				{
					MajorTimeslotRange range = new MajorTimeslotRange();

					ItemPosition position = positionInfo.GetItemPosition(i);

					range.DateRange = new DateRange(startDate, timeslotRange.End);
					range.LastMajorPosition = position;

					if (i > majorStartIndex)
						position.Union(positionInfo.GetItemPosition(majorStartIndex));

					range.RangePosition = position;

					majorRanges.Add(range);
				}
			}
			#endregion // Get List of Major Sections

			#region Calculate Which Need Indicators

			int majorRangesCount = majorRanges.Count;

			if (majorRangesCount > 0)
			{
				int currentIslandIndex = firstIslandIndex;

				for (int i = 0; i < majorRangesCount; i++)
				{
					MajorTimeslotRange range = majorRanges[i];

					// if the range encapsulates the island then we can skip to the next island because 
					// the next range can't possible use it
					for (; currentIslandIndex <= lastIslandIndex; currentIslandIndex++)
					{
						ActivityIsland island = islands[currentIslandIndex];

						// if the range is before the island then don't bump the island index
						// and don't consider this range to have indicators
						if (CoreUtilities.LessThanOrClose(range.RangePosition.End, island.Position.Start))
							break;

						// if we got here then the range intersects with the island
						if (firstColumnInView > 0)
							range.IsNearNeeded = true;

						if (island.ColumnCount >= lastColumnInView)
						{
							if (island.NeedsFarMajorIndicator(lastColumnInView, range.RangePosition))
							{
								range.IsFarNeeded = true;
							}
						}

						// if the range ends before the island then don't move to the next island yet
						if (CoreUtilities.LessThan(range.RangePosition.End, island.Position.End))
							break;
					}
				}
			}
			#endregion // Calculate Which Need Indicators

			return majorRanges;
		}
		#endregion // GetTimeslotIndicatorRanges

		#region InitializeNeedsFromToIndicators
		private void InitializeNeedsFromToIndicators(DateCollection visDates, DateRange localActivityRange, TimeSpan dayOffset, DateRange panelLogicalDayRange, ActivityItem item)
		{
			bool needsFromIndicator = false;
			bool needsToIndicator = false;

			// if the near or far portion of the activity is not within a timeslot
			// in this panel then we need to find out if it is within any panel 
			// in this control. note this is only shown when the activity is more 
			// than a day even if it happens to span across logical days.
			if (item.GetFlag(ActivityItemFlags.IsMultiDay) && item.Activity != null)
			{
				CalculateNeedsFromToIndicator(visDates, localActivityRange, dayOffset, panelLogicalDayRange, item, out needsFromIndicator, out needsToIndicator);
			}

			item.SetFlag(ActivityItemFlags.NeedsFromIndicator, needsFromIndicator);
			item.SetFlag(ActivityItemFlags.NeedsToIndicator, needsToIndicator);

			ISupportRecycling adapter = item.Adapter;
			ActivityPresenter ap = null != adapter ? adapter.AttachedElement as ActivityPresenter : null;

			if (null != ap)
			{
				ap.NeedsFromIndicator = needsFromIndicator;
				ap.NeedsToIndicator = needsToIndicator;
			}
		}
		#endregion // InitializeNeedsFromToIndicators

		#region InvalidateActivityIslands
		private List<ActivityIsland> _oldIslands;

		private void InvalidateActivityIslands(bool activitiesChanged)
		{
			if (!GetFlag(InternalFlags.HaveIslandsChanged))
			{
				SetFlag(InternalFlags.HaveIslandsChanged, true);
			}

			if (_islands != null)
			{
				_oldIslands = _islands;
				_islands = null;
				this.InvalidateMeasure();
			}

			if (activitiesChanged)
			{
				SetFlag(InternalFlags.HasActivitiesChanged, true);
			}
		}
		#endregion // InvalidateActivityIslands

		#region IsSingleLineItem
		private bool IsSingleLineItemVertical(ItemPosition position, double timeslotExtent)
		{
			if (this.GetFlag(InternalFlags.AlwaysUseSingleLine))
				return true;

			// for vertically arranged activity (like dayview timeslots), we only 
			// want to show it on a single line when it is the extent of one 
			// timeslot (or less)
			return CoreUtilities.LessThanOrClose(position.Extent, timeslotExtent);
		}

		private bool IsSingleLineItemHorizontal(int islandColumnCount, TimeSpan itemDuration)
		{
			if (this.GetFlag(InternalFlags.AlwaysUseSingleLine))
				return true;

			// for horizonally arranged activity, we want it to 
			// display on a single line if it is at least 24 hours 
			// or it overlaps with another activity
			// AS 1/13/12 TFS80802
			// This was the issue specifically causing this bug.
			//
			//return islandColumnCount > 1 || itemDuration.Ticks >= TimeSpan.TicksPerDay; // TODO should this be logical day duration?
			return islandColumnCount > 1 || itemDuration.Ticks >= this.LogicalDayDuration;
		} 
		#endregion // IsSingleLineItem

		#region MeasureIndicators
		private void MeasureIndicators(List<MajorTimeslotRange> ranges)
		{
			int nextIndicatorIndex = 0;
			Debug.Assert(ranges == null || false == this.IsVertical, "We have ranges but we're arranged vertically?");

			if (ranges != null && !this.IsVertical)
			{
				foreach (MajorTimeslotRange range in ranges)
				{
					for (int i = 0; i < 2; i++)
					{
						if (i == 0 && !range.IsNearNeeded)
							continue;
						else if (i == 1 && !range.IsFarNeeded)
							continue;

						MoreActivityIndicator indicator = GetNextIndicator(ref nextIndicatorIndex);
						indicator.Direction = i == 0 ? MoreActivityIndicatorDirection.Up : MoreActivityIndicatorDirection.Down;
						indicator.Context = range;

						Size measureSize = new Size(range.LastMajorPosition.Extent, double.PositiveInfinity);
						PanelHelper.MeasureElement(indicator, measureSize);
					}
				}
			}

			int indicatorCount = _timeslotIndicators == null ? 0 : _timeslotIndicators.Count;

			// remove any unneeded indicators
			for (int i = nextIndicatorIndex; i < indicatorCount; i++)
			{
				this.Children.Remove(_timeslotIndicators[nextIndicatorIndex]);
				_timeslotIndicators.RemoveAt(nextIndicatorIndex);
			}
		}
		#endregion // MeasureIndicators

		#region OnDataContextChanged
		private void OnDataContextChanged(object oldValue, object newValue)
		{
			if (this.Children.Count > 0)
			{
				var newCalendar = newValue as ResourceCalendar;

				foreach (ActivityItem item in _items)
				{
					Debug.Assert(item.Adapter != null && item.Activity != null, "We don't have an adapter or activity?");

					ResourceCalendar rc = item.Activity.OwningCalendar;
					ActivityPresenter ap = ((ISupportRecycling)item.Adapter).AttachedElement as ActivityPresenter;

					if (null != ap)
						ap.IsOwningCalendarSelected = rc == newCalendar;
				}
			}
		} 
		#endregion // OnDataContextChanged

		#region PerformNearFarIndicatorAction
		private void PerformNearFarIndicatorAction(MoreActivityIndicator indicator)
		{
			Debug.Assert(null != indicator && indicator == _nearIndicator || indicator == _farIndicator, "Indicator provided is not one of the current indicators");

			bool isNear = indicator == _nearIndicator;

			// get the island information
			TimeslotPositionInfo positionInfo = _positionInfo;
			positionInfo.Initialize();
			List<ActivityIsland> islands = this.GetActivityIslands(positionInfo);

			// get the position info for the start/end depending on the direction of the indicator
			ScrollInfo tsScrollInfo = this.TimeslotScrollInfo;
			int startEndIndex = isNear 
				? (int)tsScrollInfo.Offset 
				: (int)(tsScrollInfo.Offset + Math.Max(0, tsScrollInfo.Viewport - 1));
			ItemPosition position = positionInfo.GetItemPosition(startEndIndex);

			if (isNear)
				position = new ItemPosition(position.Start, 0);
			else
				position = new ItemPosition(position.End, 0);

			// now get the index of the island in which we will start the search
			IslandBinaryComparer comparer = new IslandBinaryComparer();
			comparer.Position = position;
			int startingIslandIndex = islands.BinarySearch(null, comparer);

			if (startingIslandIndex < 0)
			{
				startingIslandIndex = ~startingIslandIndex;

				// the index returned is the index at which an item would be inserted
				// so we want to start with the end of the previous island
				if (isNear)
					startingIslandIndex--;
			}

			int offset, end;

			if (isNear)
			{
				end = -1;
				offset = -1;
			}
			else
			{
				end = islands.Count;
				offset = +1;
			}

			for (int i = startingIslandIndex; i != end; i += offset)
			{
				ActivityItem item;
				
				if (isNear)
					item = islands[i].GetNearActivity(position);
				else
					item = islands[i].GetFarActivity(position);

				if (item != null)
				{
					ScheduleControlBase ctrl = ScheduleControlBase.GetControl(this);
					Debug.Assert(null != ctrl, "Need state to process scroll operation");
					ScrollInfo actualScrollInfo = ctrl != null ? ctrl.GetTimeslotScrollInfo(this) : null;

					if (null != actualScrollInfo)
					{
						double itemOffset = isNear ? item.Position.Start : Math.Floor(item.Position.End);
						double index = positionInfo.GetTimeslotIndex(itemOffset);

						// we're going to deviate from outlook. basically regardless of the duration 
						// of the activity that is next they will bring its end into view. we're 
						// going to constrain that such that we keep the start in view
						if (!isNear)
						{
							index -= Math.Max(0, actualScrollInfo.Viewport - 1);
							int startIndex = positionInfo.GetTimeslotIndex(item.Position.Start);
							index = Math.Min(index, startIndex);
						}

						actualScrollInfo.Offset = index;
					}
					break;
				}
			}
		}
		#endregion // PerformNearFarIndicatorAction

		#region PerformTimeslotIndicatorAction
		private void PerformTimeslotIndicatorAction(MoreActivityIndicator indicator)
		{
			var ctrl = ScheduleUtilities.GetControl(this);

			// AS 3/7/12 TFS102945
			// Defer the operation during a touch until the user has a chance to pan/flick.
			//
			if (ctrl != null && ctrl.ShouldQueueTouchActions)
			{
				ctrl.EnqueueTouchAction((a) => 
				{
					if (a == ScrollInfoTouchAction.Click)
						PerformTimeslotIndicatorAction(indicator);
				});
				return;
			}

			if (_activityScrollOffsetProvider != null)
			{
				_activityScrollOffsetProvider.Scroll(indicator.Direction == MoreActivityIndicatorDirection.Up ? -1 : 1);
			}
			else if (GetFlag(InternalFlags.IsActivityScrollInfoEnabled))
			{
				_activityScrollInfo.Scroll(indicator.Direction == MoreActivityIndicatorDirection.Up ? -1 : 1);
			}
			else
			{
				
			}
		}
		#endregion // PerformTimeslotIndicatorAction

		#region ReleaseElementsOutOfRange
		private List<ActivityBaseAdapter> ReleaseElementsOutOfRange(bool isVertical, IList<ActivityBase> activities,
			double availableColumnExtent, int firstColumnInView, int lastColumnInView,
			ItemPosition inViewPosition)
		{
			var availableAdapters = new List<ActivityBaseAdapter>();

			for (int i = _items.Count - 1; i >= 0; i--)
			{
				ActivityItem item = _items[i];
				ActivityBase activity = item.Activity;
				ActivityBaseAdapter adapter = item.Adapter;

				bool remove = false;





				// if the activities collection has changed or we have a new source then 
				// we need to validate if the activities for which we have generated elements 
				// still exist
				if (GetFlag(InternalFlags.HasActivitiesChanged) && !activities.Contains(activity))
				{
					remove = true;




				}
				else
				{
					// if its not associated with a visible timeslot or its not in the viewable area then 
					// try to remove it
					if (item == null || item.Island == null || !item.Position.IntersectsWith(inViewPosition))
						remove = true;
					else
					{
						// verify the column info before asking for it
						item.Island.VerifyColumnInfo();

						if (item.Column > lastColumnInView || item.Column < firstColumnInView)
							remove = true;
					}
				}

				// if the activity is not in view or the activity was removed then try to release the element
				if (remove)
				{
					FrameworkElement container = ((ISupportRecycling)adapter).AttachedElement;
					Debug.Assert(container != null, string.Format("There is no element associated with the {0} activity", activity));

					if (RecyclingManager.Manager.ReleaseElement(adapter, this))
					{
						int last = _items.Count - 1;

						// if this isn't the last item then swap it with the last. we'll resort this list later 
						// so the order isn't important
						if (i < last)
							_items[i] = _items[last];

						_items.RemoveAt(i);

						item.SetFlag(ActivityItemFlags.IsHidden, false);

						// clear the reference to the adapter on the item
						item.Adapter = null;

						// save the adapter for possible reuse with a different element
						availableAdapters.Add(adapter);
					}
					else
					{




						// flip the flag and treat this as an item that wasn't removed. we have to continue to 
						// position this element
						remove = false;
					}
				}

				// if we didn't want to remove the activity (because it was in view) or couldn't (because 
				// the release call returned false) then we need to continue to position the element. in 
				// that case we may need to refresh the activity info
				if (!remove)
				{
					if (item.Island == null)
					{
						item.Position.Start = -10000;
					}

					// if its not in an island that we would process below then we need to measure it now
					if (!item.Position.IntersectsWith(inViewPosition))
					{
						FrameworkElement container = ((ISupportRecycling)adapter).AttachedElement;

						// measure the element
						Size measureSize = this.GetMeasureSize(item, isVertical, availableColumnExtent);
						PanelHelper.MeasureElement(container, measureSize);

						DebugHelper.DebugLayout(container, "Measured Unreleased Child", "MeasureSize:{0}, DesiredSize:{1}", measureSize, container.DesiredSize);
					}
				}
			}

			// since we've validated the items for which we have created elements we can clear this flag now
			SetFlag(InternalFlags.HasActivitiesChanged, false);
			SetFlag(InternalFlags.HaveIslandsChanged, false);

			return availableAdapters;
		}
		#endregion // ReleaseElementsOutOfRange

		// AS 12/13/10 TFS61517
		#region ReleaseOldItems
		private void ReleaseOldItems()
		{
			for ( int i = _items.Count - 1; i >= 0; i-- )
			{
				var item = _items[i];
				var adapter = item.Adapter;

				if ( adapter != null )
				{
					var element = ((ISupportRecycling)adapter).AttachedElement;

					if ( element != null && !RecyclingManager.Manager.ReleaseElement(adapter, this) )
					{
						Debug.Assert(false, "Unable to release activity element when list was changed");
						continue;
					}
				}

				_items.RemoveAt(i);
			}

			// AS 1/19/11 TFS62066
			// Consider the panel dirty and clear the reference to the old islands since they 
			// reference the items and we will think there are elements associated with the 
			// items that have an adapter. anyway if we get here then the provider changed so 
			// we won't likely be reusing any items and it will just add more overhead to verify 
			// them.
			//
			this.InvalidateActivityIslands(true);
			_oldIslands = null;
		}
		#endregion // ReleaseOldItems

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool set)
		{
			if (set)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		#region VerifyNearFarIndicator
		private void VerifyNearFarIndicator(bool isNear, bool needIndicator, ref MoreActivityIndicator indicator, ref Size desired)
		{
			if (!needIndicator && indicator != null)
			{
				
				this.Children.Remove(indicator);
				indicator = null;
			}
			else if (needIndicator && indicator == null)
			{
				
				indicator = new MoreActivityIndicator();
				Canvas.SetZIndex(indicator, 10);
				Debug.Assert(this.IsVertical, "We're using near/far indicators when horizontal?");
				indicator.Direction = isNear ? MoreActivityIndicatorDirection.Up : MoreActivityIndicatorDirection.Down;
				indicator.Action = this.PerformNearFarIndicatorAction;
				this.Children.Add(indicator);
			}

			if (indicator != null)
				indicator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		} 
		#endregion // VerifyNearFarIndicator

		#endregion // Private Methods

		#endregion // Methods

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			if (dataItem == _activityProvider)
			{
				this.InvalidateActivityIslands(true);
			}
			else if (dataItem == _activityScrollOffsetProvider)
			{
				if (string.IsNullOrEmpty(property) || property == "Offset")
					this.InvalidateMeasure();
			}
			else
			{
				var activity = dataItem as ActivityBase;
				if (activity != null)
				{
					bool dirtySort = string.IsNullOrEmpty(property);

					if (!dirtySort)
					{
						switch (property)
						{
							case "Start":
							case "End":
							case "Subject":
							case "IsTimeZoneNeutral": // AS 9/20/10 TFS42810
								dirtySort = true;
								break;
						}
					}

					if (dirtySort)
					{
						// if the activity start/end has changed then the sort order may be different too
						if (null != _activityProvider)
							_activityProvider.InvalidateSort(activity);

						this.InvalidateActivityIslands(false);
					}
				}
			}
		}

		#endregion //ITypedPropertyChangeListener<object,string> Members

		#region IReceivePropertyChange<object> Members

		void IReceivePropertyChange<object>.OnPropertyChanged(DependencyProperty property, object oldValue, object newValue)
		{
			if (property == FrameworkElement.DataContextProperty)
			{
				this.OnDataContextChanged(oldValue, newValue);
			}
		}

		#endregion //IReceivePropertyChange<object> Members

		#region ActivityIsland class
		private class ActivityIsland
		{
			#region Member Variables

			private List<ActivityItem> _activities;
			private ScheduleActivityPanel _panel;
			internal ItemPosition Position;
			private bool _verifiedColumnInfo;
			private int _columnCount = 1;

			#endregion // Member Variables

			#region Constructor
			internal ActivityIsland(ScheduleActivityPanel panel)
			{
				_panel = panel;
				_activities = new List<ActivityItem>();
			}
			#endregion // Constructor

			#region Base class overrides
			public override string ToString()
			{
				return string.Format("Count={0} Position={1}", _activities.Count, this.Position);
			} 
			#endregion // Base class overrides

			#region Properties

			#region ActivityCount
			public int ActivityCount
			{
				get { return _activities.Count; }
			}
			#endregion // ActivityCount

			#region ColumnCount
			public int ColumnCount
			{
				get 
				{ 
					this.VerifyColumnInfo();
					return _columnCount;
				}
			} 
			#endregion // ColumnCount

			#region Indexer
			public ActivityItem this[int index]
			{
				get { return _activities[index]; }
			} 
			#endregion // Indexer

			#endregion // Properties

			#region Methods

			#region AddActivity
			public bool AddActivity(ActivityItem activity)
			{
				ItemPosition position = activity.Position;

				if (this.ActivityCount == 0)
					this.Position = position;
				else if (!this.Position.IntersectsWith(position))
					return false;

				_verifiedColumnInfo = false;

				this.Position.Union(position);

				_activities.Add(activity);
				activity.Island = this;
				return true;
			}
			#endregion // AddActivity

			#region GetActivity
			internal ActivityItem GetActivity( bool next, Predicate<ActivityItem> match )
			{
				int startIndex = next ? -1 : _activities.Count;
				return ScheduleUtilities.FindNextOrDefault(_activities, startIndex, next, 1, false, match);
			} 
			#endregion // GetActivity

			#region GetActivityTable
			internal void GetActivityTable(Dictionary<ActivityBase, ActivityItem> oldItems)
			{
				for (int i = 0, count = _activities.Count; i < count; i++)
				{
					ActivityItem item = _activities[i];
					Debug.Assert(!oldItems.ContainsKey(item.Activity), "There is an item already in the list for this item's activity?");
					oldItems[item.Activity] = item;
				}
			}
			#endregion // GetActivityTable

			#region GetIntersectingActivities
			internal IEnumerable<ActivityItem> GetIntersectingActivities(ItemPosition tsPosition)
			{
				for (int i = 0, count = _activities.Count; i < count; i++)
				{
					ActivityItem item = _activities[i];

					if (item.Position.IntersectsWith(tsPosition))
						yield return item;
				}
			}
			#endregion // GetIntersectingActivities

			#region GetItem
			public ActivityItem GetItem(ActivityBase activity)
			{
				Func<ActivityItem, bool> match = (ActivityItem item) =>
				{
					return item.Activity == activity;
				};

				return this._activities.FirstOrDefault(match);
			}
			#endregion // GetItem

			#region GetFarActivity
			internal ActivityItem GetFarActivity(ItemPosition position)
			{
				double sourceValue = Math.Floor(position.End);

				// we're looking for the first activity whose start is 
				// the specified position or greater
				for (int i = 0, count = _activities.Count; i < count; i++)
				{
					ActivityItem item = _activities[i];

					if (CoreUtilities.GreaterThanOrClose( item.Position.Start, sourceValue ))
						return item;
				}

				return null;
			}
			#endregion // GetFarActivity

			#region GetNearActivity
			internal ActivityItem GetNearActivity(ItemPosition position)
			{
				double sourceValue = position.Start;

				// we're looking for the first activity whose end is greater 
				// than the specified position
				for (int i = _activities.Count - 1; i >= 0; i--)
				{
					ActivityItem item = _activities[i];

					if (CoreUtilities.LessThanOrClose(item.Position.End, sourceValue))
						return item;
				}

				return null;
			} 
			#endregion // GetNearActivity

			#region NeedsFarMajorIndicator
			internal bool NeedsFarMajorIndicator(int lastColumnInView, ItemPosition itemPosition)
			{
				for (int i = 0, count = _activities.Count; i < count; i++)
				{
					ActivityItem item = _activities[i];

					if (item.Column <= lastColumnInView)
						continue;

					if (item.Position.IntersectsWith(itemPosition))
						return true;
				}

				return false;
			}
			#endregion // NeedsFarMajorIndicator

			#region NeedsNearFarIndicator
			internal bool NeedsNearFarIndicator(ItemPosition inViewPosition, bool isNear)
			{
				// if it doesn't intersect then this island is before or after 
				// the in view position
				if (!Position.IntersectsWith(inViewPosition))
				{
					// if this island is before the view position then we need an indicator
					if (isNear)
						return this.Position.Start < inViewPosition.Start;
					else
						return this.Position.Start > inViewPosition.Start;
				}

				// if we got here then the current island intersects with the in view position
				if (isNear)
				{
					double start = inViewPosition.Start;

					// we should return true for near if the bottom of an activity doesn't intersect
					for (int i = 0, count = _activities.Count; i < count; i++)
					{
						if (_activities[i].Position.End <= start)
							return true;
					}
				}
				else
				{
					// we should return true for far if the top of an activity doesn't intersect
					double end = inViewPosition.End;

					// we should return true for near if the bottom of an activity doesn't intersect
					for (int i = _activities.Count - 1; i >= 0; i--)
					{
						if (_activities[i].Position.Start > end)
							return true;
					}
				}

				return false;
			}
			#endregion // NeedsNearFarIndicator

			#region VerifyColumnInfo
			internal void VerifyColumnInfo()
			{
				if (_verifiedColumnInfo)
					return;

				// AS 11/15/10 - AlignToTimeslot Optimization
				// Moved the count check and flag initialization here since we 
				// now have 2 different implementations based on whether we align 
				// to pixels or timeslots.
				//
				_verifiedColumnInfo = true;
				int activityCount = _activities.Count;

				if ( activityCount == 0 )
					return;

				// AS 11/15/10 - AlignToTimeslot Optimization
				// This probably wouldn't happen since we wouldn't have an island with no 
				// activities but just in case we should have been setting the column count.
				//
				_columnCount = 0;

				if ( _panel.AlignToTimeslot )
					this.VerifyColumnInfoImpl_Timeslot();
				else
					this.VerifyColumnInfoImpl_Pixel();

				int columnCount = _columnCount;
				foreach ( ActivityItem item in _activities )
					item.ColumnCount = columnCount;
			}
			#endregion // VerifyColumnInfo

			#region VerifyColumnInfoImpl_Pixel
			// AS 11/15/10 - AlignToTimeslot Optimization
			// Moved the activity count check and setting of the verify flag into the 
			// calling routine and renamed the method to denote the difference in impl.
			//
			//private void VerifyColumnInfoImpl()
			private void VerifyColumnInfoImpl_Pixel()
			{
				int activityCount = _activities.Count;
				List<ActivityColumn> columns = new List<ActivityColumn>();
				columns.Add(new ActivityColumn());

				for ( int i = 0, count = activityCount; i < count; i++ )
				{
					ActivityItem item = _activities[i];

					ActivityColumn itemColumn = null;

					for ( int j = 0; j < columns.Count; j++ )
					{
						if ( columns[j].Add(item.Position) )
						{
							itemColumn = columns[j];
							item.Column = j;
							break;
						}
					}

					if ( itemColumn == null )
					{
						itemColumn = new ActivityColumn();
						itemColumn.Add(item.Position);
						item.Column = columns.Count;
						columns.Add(itemColumn);
					}
				}

				int columnCount = columns.Count;
				_columnCount = columnCount;
			} 
			#endregion // VerifyColumnInfoImpl_Pixel

			// AS 11/15/10 - AlignToTimeslot Optimization
			#region VerifyColumnInfoImpl_Timeslot
			private void VerifyColumnInfoImpl_Timeslot()
			{
				int maxColumn = 0;
				int tsCount = _panel.TimeslotCount;
				LinkedList<IntRange>[] slots = new LinkedList<IntRange>[tsCount];
				LinkedListNode<IntRange>[] workingRanges = new LinkedListNode<IntRange>[tsCount];

				foreach ( ActivityItem item in _activities )
				{
					// AS 11/8/11 TFS94091
					//workingRanges.Initialize();
					Array.Clear(workingRanges, 0, workingRanges.Length);

					Debug.Assert(item.StartTimeslotIndex >= 0 && item.StartTimeslotIndex < slots.Length, "Invalid start timeslot index");
					Debug.Assert(item.EndTimeslotIndex >= 0 && item.EndTimeslotIndex < slots.Length, "Invalid ebd timeslot index");

					int start = Math.Max(Math.Min(item.StartTimeslotIndex, tsCount - 1), 0);
					int end = Math.Max(Math.Min(item.EndTimeslotIndex, tsCount - 1), 0);
					int startingColumn = 0;

					// find the starting columns
					for ( int i = start; i <= end; i++ )
					{
						// for the start column
						var ranges = slots[i];
						LinkedListNode<IntRange> nextRange = ranges == null ? null : ranges.First;

						// AS 11/8/11 TFS94091
						//while ( nextRange != null && nextRange.Value.Start <= end )
						while ( nextRange != null && nextRange.Value.Start <= startingColumn )
							nextRange = nextRange.Next;

						workingRanges[i] = nextRange;

						if ( nextRange != null )
						{
							// AS 11/8/11 TFS94091
							// Moved this if out of the if block above. If we don't have a previous then we 
							// don't want to take the end of the last one.
							//
							if (nextRange.Previous != null)
								startingColumn = Math.Max(startingColumn, nextRange.Previous.Value.End + 1);
						}
						else if ( ranges != null )
							startingColumn = Math.Max(startingColumn, ranges.Last.Value.End + 1);
					}

					if ( end > start )
					{
						// find an opening across all slots
						do
						{
							int originalStartingColumn = startingColumn;

							// now make sure that all the starting ranges are beyond the current startingColumn
							for ( int i = start; i <= end; i++ )
							{
								var range = workingRanges[i];

								while ( range != null && range.Value.Start <= startingColumn )
									range = range.Next;

								workingRanges[i] = range;

								if ( range != null && range.Previous != null )
									startingColumn = Math.Max(startingColumn, range.Previous.Value.End + 1);
							}

							if ( originalStartingColumn == startingColumn )
								break;

						} while ( true );
					}

					item.Column = startingColumn;

					// update the ranges
					for ( int i = start; i <= end; i++ )
					{
						if ( slots[i] == null )
						{
							slots[i] = new LinkedList<IntRange>();
							slots[i].AddFirst(new IntRange { Start = startingColumn, End = startingColumn });
						}
						else
						{
							var range = workingRanges[i];

							if ( range == null )
							{
								range = slots[i].Last;

								if ( range.Value.End == startingColumn - 1 )
									range.Value.End = startingColumn;
								else
									slots[i].AddAfter(range, new IntRange { Start = startingColumn, End = startingColumn });
							}
							else
							{
								if ( range.Previous == null || range.Previous.Value.End < startingColumn - 1 )
								{
									if ( range.Value.Start == startingColumn + 1 )
										range.Value.Start = startingColumn;
									else
										slots[i].AddBefore(range, new IntRange { Start = startingColumn, End = startingColumn });
								}
								else
								{
									// AS 11/8/11 TFS94091
									// If the Start is 2 and the previous End was 0 then we don't want to 
									// end the previous with 1 (e.g. 0-1 follwed by 2-3). We want to combine 
									// the two ranges.
									//
									//if ( range.Value.Start - range.Previous.Value.End > 1 )
									if ( range.Value.Start - range.Previous.Value.End > 2 )
									{
										range.Previous.Value.End = startingColumn;
									}
									else
									{
										range.Previous.Value.End = range.Value.End;
										slots[i].Remove(range);
									}
								}
							}
						}
					}

					maxColumn = Math.Max(startingColumn, maxColumn);
				}

				_columnCount = maxColumn + 1;
			}

			#endregion // VerifyColumnInfoImpl_Timeslot

			#endregion // Methods

			// AS 11/15/10 - AlignToTimeslot Optimization
			#region IntRange class
			private class IntRange
			{
				public int Start;
				public int End;

				public override string ToString()
				{
					return string.Format("Range {0}-{1}", this.Start, this.End);
				}
			}
			#endregion // IntRange class
		} 
		#endregion // ActivityIsland class

		#region ActivityColumn class
		private class ActivityColumn
		{
			private List<ItemPosition> _positions;

			internal ActivityColumn()
			{
				_positions = new List<ItemPosition>();
			}

			public bool Add(ItemPosition position)
			{
				// TOOD optimize?
				for (int i = 0, count = _positions.Count; i < count; i++)
				{
					if (_positions[i].IntersectsWith(position))
						return false;
				}

				_positions.Add(position);
				return true;
			}
		} 
		#endregion // ActivityColumn class

		#region ActivityItem class
		private class ActivityItem
		{
			public ActivityBase Activity;
			public ItemPosition Position;
			public ItemPosition? DisplayPosition;
			public int Column;
			public ActivityIsland Island;
			public int ColumnCount;
			public ActivityBaseAdapter Adapter;
			public ActivityItemFlags Flags;

			// AS 11/15/10 - AlignToTimeslot Optimization
			// These are only guaranteed when aligning to timeslots.
			//
			public int StartTimeslotIndex;
			public int EndTimeslotIndex;

			internal bool GetFlag(ActivityItemFlags flag)
			{
				return (Flags & flag) != 0;
			}

			internal void SetFlag(ActivityItemFlags flag, bool set)
			{
				if (set)
					Flags |= flag;
				else
					Flags &= ~flag;
			}

		}
		#endregion // ActivityItem class

		#region ActivityItemFlags enum
		private enum ActivityItemFlags : byte
		{
			IsNearInPanel = 0x1,
			IsFarInPanel = 0x2,
			IsMultiDay = 0x4,
			IsSingleLineItem = 0x8,
			IsHidden = 0x10,
			NeedsFromIndicator = 0x20,
			NeedsToIndicator = 0x40,
			SpansLogicalDays = 0x80,
		} 
		#endregion // ActivityItemFlags enum

		#region MajorTimeslotRange class
		private class MajorTimeslotRange
		{
			/// <summary>
			/// Returns the date range that the major range represents
			/// </summary>
			public DateRange DateRange;

			/// <summary>
			/// Position of the entire major range (from first in major to last in major)
			/// </summary>
			public ItemPosition RangePosition;
			
			/// <summary>
			/// The position of the last in major timeslot
			/// </summary>
			public ItemPosition LastMajorPosition;

			/// <summary>
			/// Indicates if the near indicator is needed
			/// </summary>
			public bool IsNearNeeded;

			/// <summary>
			/// Indicates if the far indicator is needed
			/// </summary>
			public bool IsFarNeeded;
		} 
		#endregion // MajorTimeslotRange class

		#region IslandBinaryComparer class
		private class IslandBinaryComparer : IComparer<ActivityIsland>
		{
			#region Member Variables

			private ItemPosition _position;
			private bool _isZeroWidth;

			internal ItemPosition Position
			{
				get { return _position; }
				set
				{
					_position = value;
					_isZeroWidth = CoreUtilities.AreClose(value.Extent, 0);
				}
			}

			#endregion // Member Variables

			#region IComparer<ActivityIsland> Members

			int IComparer<ActivityIsland>.Compare(ActivityIsland x, ActivityIsland y)
			{
				Debug.Assert(x != null && y == null, "Expected to be used for a binary search where only x matters");

				// if the start and end position are the same then treat the end point as inclusive
				if (_isZeroWidth)
				{
					if (x.Position.Start > _position.Start)
						return 1;

					if (x.Position.End < _position.Start)
						return -1;
				}
				else
				{
					if (CoreUtilities.GreaterThanOrClose(x.Position.Start, _position.End))
						return 1;

					if (CoreUtilities.LessThanOrClose(x.Position.End, _position.Start))
						return -1;
				}

				return 0;
			}

			#endregion //IComparer<ActivityIsland> Members
		} 
		#endregion // IslandBinaryComparer class

		#region ClickToAddHelper class
		private class ClickToAddHelper
		{
			#region Member Variables

			private DispatcherTimer _timer;
			private ClickToAddActivityElement _clickToAddElement;
			private TimeRangePresenterBase _timeslotPresenter;
			private bool _isOverTimeslot;
			private bool _isOverIndicator;
			private ScheduleActivityPanel _panel;
			private bool _allowIntersectingActivities;
			private double _lastOffset;

			// AS 3/1/11 NA 2011.1 ShowClickToAddPrompt
			//private const ActivityType ClickToAddActivityType = ActivityType.Appointment;
			private ActivityTypes _activityTypes;
			private int _activityTypeCount;

			#endregion // Member Variables

			#region Constructor
			internal ClickToAddHelper(ScheduleActivityPanel panel)
			{
				_panel = panel;
			}
			#endregion // Constructor

			#region Internal Methods

			#region Arrange
			internal void Arrange(Size finalSize, int firstColumn, int maxColumnsInView, double availableColumnExtent, ref Rect arrangeRect)
			{
				if (null != _clickToAddElement)
				{
					bool isVertical = _panel.IsVertical;
					Rect firstTimeslotRect = PanelHelper.GetStartingArrangeRect(isVertical, finalSize, _panel.ActualTimeslotExtent, _panel.InterItemSpacing, _panel.FirstTimeslotIndex, _panel.TimeslotCount);
					double offset = isVertical ? firstTimeslotRect.Y : firstTimeslotRect.X;

					ItemPosition timeslotPos = _panel._positionInfo.GetItemPosition(_timeslotPresenter.LocalRange).Value;

					Size arrangeSize = PanelHelper.GetOutOfViewMeasureSize(_clickToAddElement);

					Rect elementRect = new Rect(
						isVertical ? _lastOffset : timeslotPos.Start + offset,
						isVertical ? timeslotPos.Start + offset : _lastOffset,
						arrangeSize.Width,
						arrangeSize.Height);

					_clickToAddElement.Arrange(elementRect);

					arrangeRect.Union(elementRect);
				}
			} 
			#endregion // Arrange

			#region Deactivate
			internal void Deactivate()
			{
				if (null != _timer)
				{
					_timer.Stop();
					_timer.Tick -= new EventHandler(this.OnTimerTick);
					_timer = null;
				}

				this.RemoveIndicator();
			} 
			#endregion // Deactivate

			#region Measure
			internal void Measure(Size availableSize, TimeslotPositionInfo positionInfo, 
				List<ActivityIsland> islands, IslandBinaryComparer islandComparer,
				int firstColumnInView, int lastColumnInView, double availableColumnExtent)
			{
				if (_timeslotPresenter == null && _clickToAddElement != null)
				{
					this.RemoveIndicator();
				}

				if (null != _clickToAddElement)
				{
					var ts = _timeslotPresenter.Timeslot;
					ItemPosition? actualPosition = null;
					
					if (_isOverIndicator || _isOverTimeslot)
						actualPosition = positionInfo.GetItemPosition(ts.Start, ts.End);

					// AS 10/1/10
					// If the panel gets a measure then the timeslots changed or activities were 
					// added/removed. If we have a click to add indicator we should make sure that 
					// we should still have one.
					//
					if ( !this.CanShowClickToAdd(ts) )
						actualPosition = null;

					// if we don't need it then remove it...
					if (actualPosition == null)
					{
						this.RemoveIndicator();
					}
					else
					{
						ItemPosition tsPosition = actualPosition.Value;

						bool isVertical = _panel.IsVertical;
						Size usedSize = new Size();
						bool isSingleLineItem = isVertical 
							? _panel.IsSingleLineItemVertical(tsPosition, _panel.ActualTimeslotExtent)
							: _panel.IsSingleLineItemHorizontal(1, ts.End.Subtract(ts.Start));

						// AS 5/9/12 TFS104555
						if (double.IsInfinity(availableColumnExtent))
						{
							if (!_panel._lastArrangeSize.IsEmpty)
								availableColumnExtent = (isVertical ? _panel._lastArrangeSize.Width : _panel._lastArrangeSize.Height) - _panel._minEmptySize;
						}

						foreach (ActivityIsland island in GetIslands(islands, islandComparer, new DateRange(ts.Start, ts.End), positionInfo))
						{
							bool hasIntersectingItems = false;
							int maxItemColumn = 0;
							bool hasNonSingleLineItem = false;

							foreach (ActivityItem item in island.GetIntersectingActivities(tsPosition))
							{
								// if any item is not a single line then 
								// AS 11/9/10
								// This flag won't be set on an activity that hasn't been arranged so we have to 
								// ignore this flag if the item isn't associated with an adapter. This should be 
								// safe since if we have an item in a schedule view that spans the height then 
								// this wouldn't be virtualized since there wouldn't be any scrolling.
								//
								//if (!item.GetFlag(ActivityItemFlags.IsSingleLineItem))
								if ( item.Adapter != null && !item.GetFlag(ActivityItemFlags.IsSingleLineItem) ) 
									hasNonSingleLineItem = true;

								maxItemColumn = Math.Max(maxItemColumn, item.Column);

								hasIntersectingItems = true;
							}

							if (hasIntersectingItems)
							{
								if (hasIntersectingItems)
									isSingleLineItem = true;

								Size islandUsedSize = _panel.GetMeasureSize(!hasNonSingleLineItem, 
									Math.Max(1, island.ColumnCount - firstColumnInView), 
									Math.Max(1, Math.Min(lastColumnInView + 1, maxItemColumn + 1 - firstColumnInView)), 
									tsPosition, isVertical, availableColumnExtent);

								if (isVertical)
									usedSize.Width = Math.Max(usedSize.Width, islandUsedSize.Width);
								else
									usedSize.Height = Math.Max(usedSize.Height, islandUsedSize.Height);
							}
						}

						_lastOffset = isVertical ? usedSize.Width : usedSize.Height;

						// get the size of the used portion....
						Size measureSize = isVertical
							? new Size(Math.Max(0, availableColumnExtent - usedSize.Width), tsPosition.Extent)
							: new Size(tsPosition.Extent, Math.Max(0, availableColumnExtent - usedSize.Height));

						if (!isVertical)
						{
							// in horizontal timeslots we can use the gutter height as well if we don't have enough room
							if (_panel._hasColumnExtent)
							{
								if (isSingleLineItem)
									measureSize.Height = _panel._activityColumnExtent;
								else if (measureSize.Height < _panel._activityColumnExtent)
									measureSize.Height = Math.Min(measureSize.Height + availableSize.Height - availableColumnExtent, _panel._activityColumnExtent);
							}
						}

						_clickToAddElement.IsSingleLineDisplay = isSingleLineItem;

						// AS 3/1/11 NA 2011.1 ActivityTypeChooser
						_clickToAddElement.ActivityTypes = _activityTypes;

						PanelHelper.MeasureElement(_clickToAddElement, measureSize);
					}
				}
			} 
			#endregion // Measure

			#region OnEnterTimeslot
			internal void OnEnterTimeslot(TimeRangePresenterBase presenter)
			{
				var ts = presenter.Timeslot;

				bool canShowClickToAdd = this.CanShowClickToAdd(ts);

				if (!canShowClickToAdd)
				{
					this.RemoveIndicator();
				}
				else
				{
					_isOverTimeslot = true;

					// if the mouse went over the click to add element and then back 
					// over the timeslot we may get another enter but we can ignore
					// it until we go over another element or if we leave both the 
					// 
					if (presenter == _timeslotPresenter)
						return;

					_timeslotPresenter = presenter;

					if (_timer != null)
						_timer.Stop();
					else
					{
						_timer = new DispatcherTimer();
						_timer.Interval = TimeSpan.FromSeconds(1);
						_timer.Tick += new EventHandler(this.OnTimerTick);
					}

					_timer.Start();

					if (null != _clickToAddElement)
						_clickToAddElement.Visibility = Visibility.Collapsed;
				}
			}
			#endregion // OnEnterTimeslot

			#region OnLeaveTimeslot
			internal void OnLeaveTimeslot(TimeRangePresenterBase presenter)
			{
				// they can be different in a touch situation
				//Debug.Assert(_timeslotPresenter == presenter || _timeslotPresenter == null, "The cached timeslot presenter is not the same as the instance provided");

				if (_timeslotPresenter == null)
					return;

				// keep a flag so we know that we're not over the timeslot anymore
				_isOverTimeslot = false;

				if (_timer != null && _timer.IsEnabled)
					_timer.Stop();

				if (_clickToAddElement != null)
				{
					// we only want to get rid of it if we're not over the indicator or timeslot
					if (!_isOverIndicator)
						_panel.InvalidateMeasure();
				}
				else
				{
					_timeslotPresenter = null;
				}
			} 
			#endregion // OnLeaveTimeslot

			#endregion // Internal Methods

			#region Private Methods

			#region CanShowClickToAdd
			private bool CanShowClickToAdd(TimeslotBase ts)
			{
				bool canShowClickToAdd = true;

				if (null == ts)
				{
					canShowClickToAdd = false;
				}
				else
				{
					TimeslotPositionInfo posInfo = _panel._positionInfo;
					posInfo.Initialize();
					ItemPosition tsPosition;
					if ( !posInfo.TryGetItemPosition(ts.Start, ts.End, out tsPosition) )
					{
						canShowClickToAdd = false;
					}
					else
					{
						DateRange range = new DateRange(ts.Start, ts.End);

						// AS 3/1/11 NA 2011.1 ShowClickToAddPrompt
						// We'll do this check further down now once we know the activity type(s) that we may 
						// end up creating.
						//
						//// first check our own panel to see if we have intersecting activity
						//if ( !_allowIntersectingActivities && _panel.HasIntersectingActivity(range, ClickToAddActivityType) )
						//    canShowClickToAdd = false;
						
#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)


						if ( canShowClickToAdd )
						{
							ScheduleControlBase ctrl = ScheduleUtilities.GetControl(_panel);
							XamScheduleDataManager dm = ctrl != null ? ctrl.DataManagerResolved : null;
							ResourceCalendar calendar = this.GetCalendar();

							// AS 3/1/11 NA 2011.1 ShowClickToAddPrompt
							//if ( dm == null || calendar == null || !dm.IsActivityOperationAllowedHelper(ClickToAddActivityType, ActivityOperation.Add, calendar) )
							if ( dm == null || calendar == null )
								canShowClickToAdd = false;
							else
							{
								_activityTypes = ctrl.GetNewActivityTypes(calendar, ActivityCreationTrigger.ClickToAdd, out _activityTypeCount);

								if (!_allowIntersectingActivities)
								{
									if ((_activityTypes & ActivityTypes.Appointment) != 0 && _panel.HasIntersectingActivity(range, ActivityType.Appointment))
									{
										_activityTypes &= ~ActivityTypes.Appointment;
										_activityTypeCount--;
									}

									if ((_activityTypes & ActivityTypes.Journal) != 0 && _panel.HasIntersectingActivity(range, ActivityType.Journal))
									{
										_activityTypes &= ~ActivityTypes.Journal;
										_activityTypeCount--;
									}
			
									if ((_activityTypes & ActivityTypes.Task) != 0 && _panel.HasIntersectingActivity(range, ActivityType.Task))
									{
										_activityTypes &= ~ActivityTypes.Task;
										_activityTypeCount--;
									}
								}

								// AS 1/13/12 TFS77443
								// Do not show click to add for disabled timeslots.
								//
								if (_activityTypeCount > 0 && !range.Intersect(ctrl.GetMinMaxRange(true), false ) || range.IsEmpty)
									canShowClickToAdd = false;

								// AS 5/25/11 TFS76367
								if (_activityTypeCount == 0)
									canShowClickToAdd = false;
							}
						}
					}
				}

				if (!canShowClickToAdd)
				{
					_activityTypeCount = 0;
					_activityTypes = ActivityTypes.None;
				}

				// AS 3/1/11 NA 2011.1 ActivityTypeChooser
				if (_clickToAddElement != null && _clickToAddElement.ActivityTypes != _activityTypes)
				{
					_clickToAddElement.ActivityTypes = _activityTypes;
				}

				return canShowClickToAdd;
			}
			#endregion // CanShowClickToAdd

			#region GetCalendar
			private ResourceCalendar GetCalendar()
			{
				return _panel.DataContext as ResourceCalendar;
			}
			#endregion // GetCalendar

			#region OnLeftMouseDownIndicator
			private void OnLeftMouseDownIndicator(object sender, MouseButtonEventArgs e)
			{
				ClickToAddActivityElement indicator = sender as ClickToAddActivityElement;
				ResourceCalendar calendar = this.GetCalendar();
				ScheduleControlBase ctrl = ScheduleUtilities.GetControl(_panel);

				if (null != ctrl)
				{
					var ts = _timeslotPresenter.Timeslot;

					bool isAllDay = indicator.IsAllDayActivity;

					// AS 3/1/11 NA 2011.1 ShowClickToAddPrompt
					//// start a create and remove the indicator
					//ActivityPresenter editPresenter;
					//if (ctrl.EditHelper.AddNew(ts.Start, ts.End, isAllDay, this.GetCalendar(), out editPresenter, ClickToAddActivityType))
					//    this.RemoveIndicator();
					if (_activityTypes == ActivityTypes.None)
					{
						this.RemoveIndicator();
					}
					else if (_activityTypeCount == 1)
					{
						ActivityType activityType = ScheduleUtilities.GetActivityType(_activityTypes).Value;

						// start a create and remove the indicator
						ActivityPresenter editPresenter;
						if (ctrl.EditHelper.AddNew(ts.Start, ts.End, isAllDay, calendar, out editPresenter, activityType))
							this.RemoveIndicator();
					}
					else
					{
						XamScheduleDataManager dm = ctrl.DataManagerResolved;
						Debug.Assert(null != dm, "No datamanager?");
						ActivityTypeChooserDialog.ChooserResult result = new ScheduleDialogBase<ActivityType?>.ChooserResult(null);
						Action<bool?> closedCallback = delegate(bool? chooserDialogResult)
						{
							if (chooserDialogResult == true && result.Choice.HasValue && calendar == this.GetCalendar())
							{
								ActivityPresenter editPresenter;
								ctrl.EditHelper.AddNew(ts.Start, ts.End, isAllDay, calendar, out editPresenter, result.Choice.Value);
							}
						};

						this.RemoveIndicator();
						dm.DisplayActivityTypeChooserDialog(
							ctrl.DialogContainerPanel,
							ScheduleUtilities.GetString("DLG_ActivityTypeChooserDialog_Title_NewActivity"),
							_activityTypes,
							ActivityTypeChooserReason.AddActivityViaClickToAdd,
							calendar,
							result,
							null,
							closedCallback);
					}
				}
			} 
			#endregion // OnLeftMouseDownIndicator

			#region OnMouseEnterIndicator
			private void OnMouseEnterIndicator(object sender, System.Windows.Input.MouseEventArgs e)
			{
				_isOverIndicator = true;
			} 
			#endregion // OnMouseEnterIndicator

			#region OnMouseLeaveIndicator
			private void OnMouseLeaveIndicator(object sender, System.Windows.Input.MouseEventArgs e)
			{
				_isOverIndicator = false;

				// we only want to get rid of it if we're not over the indicator or timeslot
				if (!_isOverTimeslot)
					_panel.InvalidateMeasure();
			} 
			#endregion // OnMouseLeaveIndicator
			
			// AS 1/20/11 TFS62619
			#region OnRightMouseDownIndicator
			private void OnRightMouseDownIndicator(object sender, MouseButtonEventArgs e)
			{
				var tsPresenter = _timeslotPresenter;
				this.RemoveIndicator();

				if (null != tsPresenter)
					tsPresenter.ProcessRightMouseDown(e);
			}
			#endregion //OnRightMouseDownIndicator

			#region OnTimerTick
			private void OnTimerTick(object sender, EventArgs e)
			{
				DispatcherTimer timer = sender as DispatcherTimer;
				timer.Stop();

				// show the indicator
				if (_clickToAddElement == null)
				{
					if (_isOverTimeslot == false)
						return;

					var ctrl = ScheduleUtilities.GetControl(_panel);
					var dm = ctrl != null ? ctrl.DataManagerResolved : null;
					var calendar = this.GetCalendar();

					if (null == dm || null == calendar)
						return;

					_clickToAddElement = new ClickToAddActivityElement();
					_clickToAddElement.MouseEnter += new MouseEventHandler(OnMouseEnterIndicator);
					_clickToAddElement.MouseLeave += new MouseEventHandler(OnMouseLeaveIndicator);
					_clickToAddElement.MouseLeftButtonDown += new MouseButtonEventHandler(OnLeftMouseDownIndicator);
					_clickToAddElement.MouseRightButtonDown += new MouseButtonEventHandler(OnRightMouseDownIndicator); // AS 1/20/11 TFS62619

					// AS 3/1/11 NA 2011.1 ActivityTypeChooser
					_clickToAddElement.ActivityTypes = _activityTypes;

					bool isEvent = _panel.ClickToAddType == ClickToAddType.Event;
					// AS 3/1/11 NA 2011.1 ActivityTypeChooser
					//if (isEvent)
					//{
					//    if (!dm.IsTimeZoneNeutralActivityAllowed(ClickToAddActivityType, calendar))
					//        isEvent = false;
					//}
					//
					//string promptResourceName;
					//
					//if (isEvent)
					//{
					//    _clickToAddElement.IsAllDayActivity = true;
					//    promptResourceName = "ClickToAddEventPrompt";
					//}
					//else
					//{
					//    _clickToAddElement.IsAllDayActivity = false;
					//    promptResourceName = "ClickToAddAppointmentPrompt";
					//}
					//
					//_clickToAddElement.Prompt = ScheduleUtilities.GetString(promptResourceName);
					//_allowIntersectingActivities = _panel.ClickToAddType != ClickToAddType.NonIntersectingAppointment;						
					string promptResourceName;

					if (_activityTypeCount == 1)
					{
						ActivityType activityType = ScheduleUtilities.GetActivityType(_activityTypes).Value;

						if (isEvent)
						{
							if (!dm.IsTimeZoneNeutralActivityAllowed(activityType, calendar))
								isEvent = false;
						}

						if (isEvent)
						{
							_clickToAddElement.IsAllDayActivity = true;
							promptResourceName = "ClickToAddEventPrompt";
						}
						else
						{
							_clickToAddElement.IsAllDayActivity = false;

							switch (activityType)
							{
								default:
								case ActivityType.Appointment:
									Debug.Assert(activityType == ActivityType.Appointment, "Unexpected activity type:" + activityType.ToString());
									promptResourceName = "ClickToAddAppointmentPrompt";
									break;
								case ActivityType.Journal:
									promptResourceName = "ClickToAddJournalPrompt";
									break;
								case ActivityType.Task:
									promptResourceName = "ClickToAddTaskPrompt";
									break;
							}
						}
					}
					else
					{
						if (isEvent)
						{
							_clickToAddElement.IsAllDayActivity = true;
							promptResourceName = "ClickToAddEventPrompt";
						}
						else
						{
							promptResourceName = "ClickToAddActivityPrompt";
						}
					}

					_clickToAddElement.Prompt = ScheduleUtilities.GetString(promptResourceName);
					_allowIntersectingActivities = _panel.ClickToAddType != ClickToAddType.NonIntersectingActivity;

					_panel.Children.Add(_clickToAddElement);
				}

				_panel.InvalidateMeasure();
				_clickToAddElement.Visibility = Visibility.Visible;
			}
			#endregion // OnTimerTick

			#region RemoveIndicator
			private void RemoveIndicator()
			{
				if (null != _clickToAddElement)
				{
					_timeslotPresenter = null;
					_isOverTimeslot = false;

					_clickToAddElement.MouseEnter -= new MouseEventHandler(OnMouseEnterIndicator);
					_clickToAddElement.MouseLeave -= new MouseEventHandler(OnMouseLeaveIndicator);
					_clickToAddElement.MouseLeftButtonDown -= new MouseButtonEventHandler(OnLeftMouseDownIndicator);
					_clickToAddElement.MouseRightButtonDown -= new MouseButtonEventHandler(OnRightMouseDownIndicator); // AS 1/20/11 TFS62619


					// AS 3/12/12 TFS104584
					// While debugging I noticed that when touching/clicking on the element 
					// that we were removing the element while its ismouseover is true. This 
					// hasn't presented an issue previously but I do get some inconsistent 
					// exceptions in the wpf framework when it tries to promote stylus staging 
					// items to mouse events so it could be related to this.
					//
					if (_clickToAddElement.IsMouseOver)
					{
						_panel.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Threading.SendOrPostCallback((t) =>
						{
							_panel.Children.Remove(t as ClickToAddActivityElement);
						}), _clickToAddElement);
						_clickToAddElement.Visibility = Visibility.Collapsed;
						_clickToAddElement = null;
					}
					else

					{
						_panel.Children.Remove(_clickToAddElement);
					}

					_isOverIndicator = false;
					_clickToAddElement = null;
				}
			}
			#endregion // RemoveIndicator

			#endregion // Private Methods
		} 
		#endregion // ClickToAddHelper class

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : short
		{
			HasActivitiesChanged = 0x1,
			HaveIslandsChanged = 0x2,
			AlignToTimeslot = 0x4,
			SortByTimeslotCountFirst = 0x8,
			UseTimeslotIntervalAsMin = 0x10,
			ConstrainToInViewRect = 0x20,
			UsedLastDesiredSize = 0x40,
			IsActivityScrollInfoEnabled = 0x80,
			AlwaysUseSingleLine = 0x100,
		}
		#endregion // InternalFlags enum

		#region ActivitySortComparer class
		private class ActivitySortComparer : IComparer<ActivityBase>
		{
			#region Member Variables

			private ScheduleActivityPanel _panel;

			#endregion // Member Variables

			#region Constructor
			internal ActivitySortComparer(ScheduleActivityPanel panel)
			{
				_panel = panel;
			}
			#endregion // Constructor

			#region IComparer<ActivityBase> Members

			
			int IComparer<ActivityBase>.Compare(ActivityBase a1, ActivityBase a2)
			{
				if (a1 == a2)
					return 0;

				if (a1 == null)
					return -1;
				else if (a2 == null)
					return 1;

				int compare = 0;
				var token = _panel.TimeZoneInfoProvider.LocalToken;

				if (_panel.GetFlag(InternalFlags.AlignToTimeslot) && _panel.GetFlag(InternalFlags.SortByTimeslotCountFirst))
				{
					int startIndex1, endIndex1, startIndex2, endIndex2;

					bool hasSlot1 = _panel.GetTimeslotIndex(ScheduleUtilities.ConvertFromUtc(token, a1), out startIndex1, out endIndex1);
					bool hasSlot2 = _panel.GetTimeslotIndex(ScheduleUtilities.ConvertFromUtc(token, a2), out startIndex2, out endIndex2);

					// commented out for now because this can happen. in this case when the drop ended the async
					// change notifications that the monthview got get processed before the query result is fixed up
					//Debug.Assert(hasSlot1 && hasSlot2, "One of the activities being compared doesn't exist in a timeslot?");

					// this shouldn't happen but just in case...
					if (hasSlot1 != hasSlot2)
					{
						if (hasSlot1)
							return 1;
						else
							return -1;
					}

					// earlier start should be first
					if (startIndex1 != startIndex2)
						return startIndex1.CompareTo(startIndex2);

					// outlook sorts all multislot activities together so if only 1 activity spans 
					// multiple time slots then that one should come first.
					if ((endIndex1 != startIndex1) != (endIndex2 != startIndex2))
					{
						// the one that spans multiple slots should come first
						return endIndex2.CompareTo(endIndex1);
					}

					// if both span multiple slots (even if they span a different # of slots) or if both exist within 
					// the same slot then they just compare the start times and durations...
				}

				// starting date (earliest first)
				var a1Range = ScheduleUtilities.ConvertFromUtc(token, a1);
				var a2Range = ScheduleUtilities.ConvertFromUtc(token, a2);
				compare = a1Range.Start.CompareTo(a2Range.Start);

				if (compare == 0)
				{
					// duration (longer first)
					compare = a2Range.End.CompareTo(a1Range.End);

					if (compare == 0)
					{
						// outlook will keep the activities from the same calendar together
						if (a1.OwningCalendar != a2.OwningCalendar)
						{
							Debug.Assert(null != _panel._activityProvider, "No activity provider when comparing activity?");

							if (null != _panel._activityProvider)
								return _panel._activityProvider.CompareCalendar(a1.OwningCalendar, a2.OwningCalendar);
						}

						// within the same calendar outlook sorts by subject
						compare = string.Compare(a1.Subject, a2.Subject, StringComparison.CurrentCulture);

						if (compare == 0)
						{
							// if the start and end date are the same we need to fall back to checking something. we don't 
							// have an idea of the actual "creation" date or anything similar to compare. using the original 
							// position in the query result isn't ideal either as that is the result of a query and therefore
							// 2 successive executions of the same query could result in different ordered return values.
							// so we'll fall back to doing an ordinal (i.e. byte) comparison of the id's so at least the 
							// position of 2 activities with the same start and end are consistent.
							compare = string.CompareOrdinal(a1.Id, a2.Id);
						}
					}
				}

				return compare;
			}

			#endregion //IComparer<ActivityBase> Members
		} 
		#endregion // ActivitySortComparer class

		#region ActivateActivityHelper class
		private class ActivateActivityHelper
		{
			internal DateCollection _visibleDates;
			internal TimeZoneToken _localToken;
			internal DateRange _panelRange;
			internal bool _next;
			internal TimeSpan _dayOffset;
			internal ItemPosition? _constrainingRange;
			internal ActivityItem _startingItem;
			private bool _foundItem;
			private double _position = double.NaN;
			private bool _hasPosition;

			#region Properties
			internal bool FoundStartingItem
			{
				get { return _foundItem; }
			}

			internal double Position
			{
				get { return _position; }
				set
				{
					if (!double.Equals(_position, value))
					{
						_position = value;
						_hasPosition = !double.IsNaN(value);
					}
				}
			} 
			#endregion // Properties

			#region Methods

			#region IsMatch
			public bool IsMatch( ActivityItem item )
			{
				double itemStart = item.Position.Start;

				// if the start is not within the range then skip the activity. this is primarily
				// used for the all day event area so we can only look for activities within a given
				// day
				if (null != _constrainingRange && !_constrainingRange.Value.IntersectsWith(new ItemPosition(itemStart, 0)))
					return false;

				// if we have a starting item then wait until we find that
				if (_startingItem != null && !_foundItem)
				{
					if (_startingItem == item)
						_foundItem = true;

					return false;
				}

				// once we pass the matching item then we can return the next one that doesn't fail below.
				bool isMatch = true;

				if (_hasPosition)
				{
					// we only care about position if the position was specified
					isMatch = _next
						? CoreUtilities.LessThanOrClose(_position, itemStart)
						: CoreUtilities.GreaterThanOrClose(_position, itemStart);
				}

				if (isMatch)
				{
					// when navigating backwards, we need to ignore the activity where
					// we are just seeing the latter part of it. if it has a part elsewhere
					// in the cotnrol then ignore this and wait until we hit that initial
					// part
					if (!item.GetFlag(ActivityItemFlags.IsNearInPanel))
					{
						if (!IsFirstEdgeInControl(item))
							isMatch = false;
					}
				}

				return isMatch;
			}
			#endregion // IsMatch

			#region IsFirstEdgeInControl
			private bool IsFirstEdgeInControl( ActivityItem item )
			{
				bool needsTo, needsFrom;
				var activity = item.Activity;
				var activityRange = new DateRange(activity.GetStartLocal(_localToken), activity.GetEndLocal(_localToken));
				ScheduleActivityPanel.CalculateNeedsFromToIndicator(_visibleDates, activityRange, _dayOffset, _panelRange, item, out needsFrom, out needsTo);

				if (_next && needsTo)
					return true;

				if (!_next && needsFrom)
					return true;

				return false;
			}
			#endregion // IsFirstEdgeInControl

			#endregion // Methods
		}
		#endregion // ActivateActivityHelper class
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