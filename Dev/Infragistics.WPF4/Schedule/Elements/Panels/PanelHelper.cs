using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Helper class for dealing with recycleable elements.
	/// </summary>
	internal static class PanelHelper
	{
		private const double OutOfViewOffset = -10000d;

		#region Properties

		#region LastMeasureSize

		private static readonly DependencyProperty LastMeasureSizeProperty = DependencyProperty.RegisterAttached("LastMeasureSize",
			typeof(Size), typeof(PanelHelper),
			DependencyPropertyUtilities.CreateMetadata(new Size())
			);

		private static Size GetLastMeasureSize(DependencyObject d)
		{
			return (Size)d.GetValue(PanelHelper.LastMeasureSizeProperty);
		}

		private static void SetLastMeasureSize(DependencyObject d, Size value)
		{
			d.SetValue(PanelHelper.LastMeasureSizeProperty, value);
		}

		#endregion //LastMeasureSize

		#endregion // Properties

		#region Public Methods

		#region ArrangeOutOfView
		internal static void ArrangeOutOfView(UIElement element)
		{
			// AS 1/10/12 TFS98969
			//Size measureSize = PanelHelper.GetOutOfViewMeasureSize(element);
			//element.Arrange(new Rect(new Point(OutOfViewOffset, OutOfViewOffset), measureSize));
			element.Arrange(new Rect(OutOfViewOffset, OutOfViewOffset, 0, 0));
		} 
		#endregion // ArrangeOutOfView

		#region ArrangeReleasedElements
		public static void ArrangeReleasedElements(Panel panel)
		{
			Rect outOfViewRect = new Rect(OutOfViewOffset, OutOfViewOffset, 0, 0);

			DebugHelper.DebugLayout(panel, false, true, "ArrangeStack Out of view Start", null);

			foreach (FrameworkElement element in RecyclingManager.Manager.GetRecentlyAvailableElements(panel, false))
			{
				DebugHelper.DebugLayout(element, "Arranged Out of view", null);

				// AS 1/10/12 TFS98969
				//Size lastMeasureSize = GetOutOfViewMeasureSize(element);
				//
				//outOfViewRect.Width = lastMeasureSize.Width;
				//outOfViewRect.Height = lastMeasureSize.Height;

				element.Arrange(outOfViewRect);
			}

			DebugHelper.DebugLayout(panel, true, false, "ArrangeStack Out of view End", null);
		}
		#endregion // ArrangeReleasedElements

		#region ArrangeStack

		/// <summary>
		/// Arranges the specified elements with a specific extent in a horizontal or vertical stack
		/// </summary>
		/// <param name="panel">The panel whose elements will be arranged</param>
		/// <param name="elementsInView">The elements to arrange</param>
		/// <param name="isVertical">True if the items are to be arranged vertically; otherwise false to arrange them horizontally</param>
		/// <param name="finalSize">The size provided to the arrange</param>
		/// <param name="itemExtent">The extent of each item - for vertical this is the height and for horizontal this is the width of each item</param>
		/// <param name="interItemSpacing">The amount of space between each item</param>
		/// <param name="firstVisibleRealizedIndex">The index of the items in the elements in view that is considered to be the first element in view</param>
		/// <param name="extraPixels">The amount of extra space to distribute to the items. Should only be used when all items are in view</param>
		/// <param name="arrangeCallback">The method to invoke when the element should be arranged</param>
		/// <returns>The rect used.</returns>
		public static Rect ArrangeStack( Panel panel, IList<FrameworkElement> elementsInView, bool isVertical, Size finalSize, double itemExtent, double interItemSpacing, int firstVisibleRealizedIndex, int extraPixels = 0 
			, Action<FrameworkElement, Rect> arrangeCallback = null  )
		{
			PanelHelper.ArrangeReleasedElements(panel);

			int itemCount = elementsInView.Count;

			Rect itemRect = GetStartingArrangeRect(isVertical, finalSize, itemExtent, interItemSpacing, firstVisibleRealizedIndex, itemCount);

			Rect usedRect = new Rect(itemRect.X, itemRect.Y, 0, 0);

			DebugHelper.DebugLayout(panel, false, true, "ArrangeStack Items Start", null);

			// AS 4/14/11 TFS71761
			// Because of layout rounding we may have a few more pixels but not enough to make all the 
			// items use all of the available space and still be the same size. So we'll distribute an 
			// extra pixel to the first n items. This is meant to only be used when there is no scrolling 
			// so there's a bit of validation to try and enforce that.
			//
			Debug.Assert(firstVisibleRealizedIndex == 0 || extraPixels == 0, "Extra pixels should only be used when not scrolling");
			Debug.Assert(extraPixels >= 0 && extraPixels <= itemCount, "Extra pixels must be 0 or positive and shouldn't exceed the number of items since we only give an extra pixel to the initial items");

			if (firstVisibleRealizedIndex == 0 && extraPixels > 0)
			{
				itemExtent++;

				if (isVertical)
					itemRect.Height++;
				else
					itemRect.Width++;
			}
			else
			{
				extraPixels = -1;
			}

			for (int i = 0; i < itemCount; i++)
			{
				// AS 4/14/11 TFS71761
				// Once we get past the first n items, we can restore the 
				// itemRect/Extent such that they don't have the extra 
				// pixel.
				//
				if (i == extraPixels)
				{
					if (isVertical)
						itemRect.Height--;
					else
						itemRect.Width--;

					itemExtent--;
				}

				FrameworkElement child = (FrameworkElement)elementsInView[i];

				DebugHelper.DebugLayout(child, "Arranging Child", "ItemRect:{0}", itemRect);

				// AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				// child.Arrange(itemRect);
				if ( arrangeCallback == null )
					child.Arrange(itemRect);
				else
					arrangeCallback(child, itemRect);

				if (isVertical)
					itemRect.Y += itemExtent + interItemSpacing;
				else
					itemRect.X += itemExtent + interItemSpacing;
			}

			DebugHelper.DebugLayout(panel, true, false, "ArrangeStack Items End", null);

			double usedExtent = (itemCount * itemExtent) + Math.Max((itemCount - 1) * interItemSpacing, 0);

			if (isVertical)
			{
				usedRect.Width = itemRect.Width;
				usedRect.Height = usedExtent;
			}
			else
			{
				usedRect.Height = itemRect.Height;
				usedRect.Width = usedExtent;
			}

			// if an item is partially out of view then it should be clipped
			// we can get this to happen automatically by indicating we used 
			// more space then we needed
			usedRect.Union(new Rect(0, 0, finalSize.Width, finalSize.Height));

			return usedRect;
		}

		#endregion //ArrangeStack

		#region CalculateExtent
		public static double CalculateExtent(int itemCount, double interItemSpacing, double itemExtent)
		{
			double extent = (itemExtent * itemCount);
			
			if (itemCount > 1)
				extent += (itemCount - 1) * interItemSpacing;

			return Math.Max(extent, 0);
		}
		#endregion // CalculateExtent

		#region GetOutOfViewMeasureSize
		internal static Size GetOutOfViewMeasureSize(UIElement element)
		{
			Size lastMeasureSize = GetLastMeasureSize(element);

			if (double.IsPositiveInfinity(lastMeasureSize.Width))
				lastMeasureSize.Width = element.DesiredSize.Width;

			if (double.IsPositiveInfinity(lastMeasureSize.Height))
				lastMeasureSize.Height = element.DesiredSize.Height;

			return lastMeasureSize;
		}
		#endregion // GetOutOfViewMeasureSize

		#region GetStartingArrangeRect
		/// <summary>
		/// Returns the rect for the first item to arrange when using the ArrangeStack method
		/// </summary>
		/// <param name="isVertical">True if the items are arranged vertically otherwise false.</param>
		/// <param name="finalSize">The size provided to the ArrangeOverride</param>
		/// <param name="itemExtent">The extent of each item</param>
		/// <param name="interItemSpacing">The amount of space between each item</param>
		/// <param name="firstVisibleRealizedIndex">The index of the first item relative to the <paramref name="itemCount"/></param>
		/// <param name="itemCount">The number of items being arranged</param>
		/// <returns>The rect for the arrange</returns>
		internal static Rect GetStartingArrangeRect(bool isVertical, Size finalSize, double itemExtent, double interItemSpacing, int firstVisibleRealizedIndex, int itemCount)
		{
			Rect itemRect = new Rect(new Point(), finalSize);

			if (isVertical)
				itemRect.Height = itemExtent;
			else
				itemRect.Width = itemExtent;

			// if we're supposed to fill the available area then we 
			// want to shift everything down so the previous item is visible
			if (firstVisibleRealizedIndex > 0)
			{
				int inViewCount = itemCount - firstVisibleRealizedIndex;
				double extentUsed = CalculateExtent(inViewCount, interItemSpacing, itemExtent);

				if (isVertical && extentUsed < finalSize.Height)
					itemRect.Y += finalSize.Height - extentUsed;
				else if (!isVertical && extentUsed < finalSize.Width)
					itemRect.X += finalSize.Width - extentUsed;

				double offset = (itemExtent * firstVisibleRealizedIndex) + (interItemSpacing * firstVisibleRealizedIndex);

				if (isVertical)
					itemRect.Y -= offset;
				else
					itemRect.X -= offset;
			}

			return itemRect;
		}
		#endregion // GetStartingArrangeRect

		#region GetStackStartAndEnd

		/// <summary>
		/// Gets the adjusted start and end indexes for a stack arrangement
		/// </summary>
		/// <param name="availableSize">The size available for the measure</param>
		/// <param name="firstItemIndex">The index of the first item to arrange</param>
		/// <param name="pageSize">The calculated number of items fully in view</param>
		/// <param name="isVertical">True if the items are arranged vertically</param>
		/// <param name="itemExtent">The extent of the items</param>
		/// <param name="interItemSpacing">The amount of space between each item</param>
		/// <param name="realizePrecedingAndTrailingItems">True if the previous and next items should be considered as well if possible</param>
		/// <param name="itemCount">The total number of items</param>
		/// <param name="startItemIndex">Out parameter set to the adjusted first item index</param>
		/// <param name="endItemIndex">Out parameter set to the adjusted end item index</param>
		/// <param name="allowAdjustStartIndex">True if the <paramref name="startItemIndex"/> may be adjusted if the items don't fill the available space</param>
		internal static void GetStackStartAndEnd(Size availableSize, int firstItemIndex, int pageSize, bool isVertical, double itemExtent, double interItemSpacing, bool realizePrecedingAndTrailingItems, int itemCount, 
			out int startItemIndex, out int endItemIndex
			, bool allowAdjustStartIndex = true // AS 4/18/11 CalendarGroup Sizing/Scrolling
			)
		{
			startItemIndex = firstItemIndex;
			endItemIndex = Math.Min(startItemIndex + pageSize - 1, itemCount - 1);

			double availableExtent = isVertical ? availableSize.Height : availableSize.Width;
			// AS 11/15/10
			// I found this while implementing scrolling in 11.1. We should always have at least 1 item (but not more than the itemcount).
			//
			//int inViewCount = Math.Max(endItemIndex - startItemIndex, 0);
			int inViewCount = Math.Min(Math.Max(1 + endItemIndex - startItemIndex, 0), itemCount);
			bool needExtra = realizePrecedingAndTrailingItems || CalculateExtent(inViewCount, interItemSpacing, itemExtent) < availableExtent;

			if (needExtra)
			{
				// include another after the end
				if (endItemIndex < itemCount - 1)
				{
					endItemIndex++;

					inViewCount++;

					// AS 4/18/11 CalendarGroup Sizing/Scrolling
					// Added the option to not force a leading item such as would occur when one scrolls all the way to the far edge.
					//
					// see if we still need another
					//needExtra = realizePrecedingAndTrailingItems || CalculateExtent(inViewCount, interItemSpacing, itemExtent) < availableExtent;
				}

				// AS 4/18/11 CalendarGroup Sizing/Scrolling
				// see if we still need another
				needExtra = realizePrecedingAndTrailingItems || (allowAdjustStartIndex && CalculateExtent(inViewCount, interItemSpacing, itemExtent) < availableExtent);

				// include one item before the start for scrolling up
				if (needExtra && startItemIndex > 0)
				{
					startItemIndex--;

					inViewCount++;
				}
			}

			Debug.Assert(startItemIndex >= 0);
		}

		#endregion // GetStackStartAndEnd

		#region MeasureElement
		/// <summary>
		/// Helper method for measuring a recycleable element.
		/// </summary>
		/// <param name="element">The element to measure</param>
		/// <param name="availableSize">The size with which to measure the element.</param>
		public static void MeasureElement(UIElement element, Size availableSize)
		{
			element.Measure(availableSize);

			// cache the last size it was measured with
			SetLastMeasureSize(element, availableSize);
		} 
		#endregion // MeasureElement

		#region RealizeStack
		/// <summary>
		/// Provides the creation and measurement for recyclable elements 
		/// </summary>
		/// <typeparam name="T">The type of item which implements ISupportRecycling</typeparam>
		/// <param name="availableSize">The available size from the measure</param>
		/// <param name="firstItemIndex">The index of the item in the items collection that represents the first visible item</param>
		/// <param name="pageSize">The # of items that should fit in view</param>
		/// <param name="panel">The panel where the children will be added</param>
		/// <param name="items">The collection of recycling items that are used to get/create the elements</param>
		/// <param name="isVertical">True if the items are arranged vertically; otherwise false if the items are arranged horizontally</param>
		/// <param name="elementsInView">A collection of the items currently in view</param>
		/// <param name="timeSlotExtent">The extent of each item</param>
		/// <param name="interItemSpacing">The amount of space between each item</param>
		/// <param name="initializer">Action that is invoked when an element is attached. The action is passed the item, its index, the container with which it is being associated and a flag indicating the item state.</param>
		/// <param name="realizePrecedingAndTrailingItems">True to create the item before the first visible item and after the last visible item. This would be needed to let wpf/sl do the keyboard navigation</param>
		/// <param name="firstVisibleRealizedIndex">This represents the current element that is first in view and will be updated if necessary based on what was measured/realized.</param>
		/// <param name="startItemIndex">Out param set to the index of the resolved first item</param>
		/// <param name="endItemIndex">Out param set to the index of the resolved last item</param>
		/// <param name="forceKeepExisting">Boolean indicating whether to keep the existing items</param>
		/// <param name="allowAdjustStartIndex">True if the <paramref name="startItemIndex"/> may be adjusted if the items don't fill the available space</param>
		/// <returns>The desired size</returns>
		public static Size RealizeStack<T>(Size availableSize,
			int firstItemIndex, int pageSize,
			Panel panel, IList<T> items, bool isVertical, IList<FrameworkElement> elementsInView,
			// AS 5/9/12 TFS104555 Changed timeslotExtent to ref parameter
			ref double timeSlotExtent, bool realizePrecedingAndTrailingItems, double interItemSpacing,
			Action<ISupportRecycling, int, FrameworkElement, InitializeItemState> initializer,
			ref int firstVisibleRealizedIndex, out int startItemIndex, out int endItemIndex
			, bool allowExtraPixels, out int extraPixels // AS 1/13/12 TFS74252
			, bool? forceKeepExisting = null // AS 12/16/10 TFS61823
			, bool allowAdjustStartIndex = true // AS 4/18/11 CalendarGroup Sizing/Scrolling
			)
			where T : ISupportRecycling
		{
			// AS 5/9/12 TFS104555
			Debug.Assert(!double.IsInfinity(timeSlotExtent) || double.IsInfinity(timeSlotExtent) == double.IsInfinity(isVertical ? availableSize.Height : availableSize.Width), "If we have an infinite timeslot extent then we should have an infinite available extent");

			Size desired = new Size();
			int itemCount = items == null ? 0 : items.Count;

			#region Calculate indexes for in view items

			// start with what's fully in view based on the scroll offset and page size
			GetStackStartAndEnd(availableSize, firstItemIndex, pageSize, isVertical, timeSlotExtent, interItemSpacing, realizePrecedingAndTrailingItems, itemCount, out startItemIndex, out endItemIndex, allowAdjustStartIndex );

			#endregion //Calculate indexes for in view items

			#region Setup list of items to attach

			// build a list of the elements that we will have used elements for
			HashSet<ISupportRecycling> newItems = new HashSet<ISupportRecycling>();
			List<int> newItemList = new List<int>();

			for (int i = startItemIndex; i <= endItemIndex; i++)
			{
				ISupportRecycling item = items[i] as ISupportRecycling;

				newItems.Add(item);
				newItemList.Add(i);
			}
			#endregion //Setup list of items to attach

			#region Focused Elements
			// get the item that contains the input focus since we want to include the element before/after that
			int focusedIndex = GetInViewFocusedIndex(panel, elementsInView);

			// if there is a focused element...
			if (focusedIndex >= 0 && itemCount > 0)
			{
				FrameworkElement focusedElement = elementsInView[focusedIndex];
				T focusedItem = (T)RecyclingManager.Manager.ItemFromElement(focusedElement);
				int focusItemIndex = items.IndexOf(focusedItem);

				
				if (focusItemIndex >= 0)
				{
					// include the focused item as well as the item before/after
					for (int i = -1; i <= 1; i++)
					{
						int itemIndex = focusItemIndex + i;

						if (itemIndex < 0 || itemIndex >= items.Count)
							continue;

						if (itemIndex < startItemIndex || itemIndex > endItemIndex)
						{
							ISupportRecycling item = items[itemIndex] as ISupportRecycling;

							

							newItems.Add(item);
							newItemList.Add(itemIndex);
						}
					}
				}
			}
			#endregion //Focused Elements

			#region Release reusable elements

			// AS 12/13/10 TFS61517
			bool keepExisting = (isVertical && availableSize.Height == 0) || (!isVertical && availableSize.Width == 0);

			// AS 12/16/10 TFS61823
			if ( forceKeepExisting != null )
				keepExisting = forceKeepExisting.Value;

			// release anything not being reused
			for (int i = elementsInView.Count - 1; i >= 0; i--)
			{
				T item = (T)RecyclingManager.Manager.ItemFromElement(elementsInView[i]);

				Debug.Assert(null != item, "The element is not associated with an item");
				Debug.Assert(null != item || !panel.Children.Contains(elementsInView[i]));

				// this container is associated with an item we won't position
				if (null != item && !newItems.Contains(item))
				{
					FrameworkElement container = item.AttachedElement;

					// AS 12/13/10 TFS61517
					// This isn't the cause of the problem but while debugging this issue I noticed 
					// a performance issue. Essentially when in a gridbagpanel and we have a row 
					// or column with a weight, the child gets measured with a 0 height or width 
					// (for the weighted orientation) so it can find out the extent of the other 
					// orientation (because it is not a weighted value) without causing a virtualizing 
					// panel to create lots of elements for an area that it may not be given. Well 
					// when the panel was measured with a extent previously it would have had a 
					// number of elements realized. When it then gets the 0 height or width we end 
					// up virtualizing (i.e. detaching from the associated object) all of the elements 
					// but one. Then we get another measure with the appropriate value in that 
					// orientation and reassociate the elements with the objects. So we will assume 
					// that if we are measured with a 0 width or height that we may as well keep 
					// the already allocated items.
					//
					//if (!RecyclingManager.Manager.ReleaseElement(item, panel))
					if (keepExisting || !RecyclingManager.Manager.ReleaseElement(item, panel))
					{
						// we're not allowed to release this element yet so store it
						// and position it where it would be
						newItems.Add(item);

						int itemIndex = items.IndexOf(item);
						
						// AS 12/13/10 TFS61517
						// If the item doesn't exist then we should try to release it.
						//
						if ( itemIndex < 0 && keepExisting )
						{
							if ( RecyclingManager.Manager.ReleaseElement(item, panel) )
								continue;
						}

						Debug.Assert(itemIndex >= 0, "Item is no longer in source collection?");
						newItemList.Add(itemIndex);
					}
				}
			}
			#endregion //Release reusable elements

			#region Debug Children


#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

			#endregion // Debug Children

			#region Setup for create/measure

			elementsInView.Clear();
			Size itemSize = availableSize;

			if (isVertical)
			{
				itemSize.Height = timeSlotExtent;
			}
			else
			{
				itemSize.Width = timeSlotExtent;
			}

			// AS 1/13/12 TFS74252
			int extraPixelRestoreIndex = -1;
			extraPixels = 0;

			// as noted in the arrangestack, this is really only when there is no scrolling
			if (itemCount > 0 && allowExtraPixels && startItemIndex == 0 && endItemIndex == itemCount - 1)
			{
				double extentUsed = itemCount * timeSlotExtent;
				double availableExtent = isVertical ? availableSize.Height : availableSize.Width;

				if (!CoreUtilities.AreClose(extentUsed, availableExtent))
				{
					extraPixels = Math.Max((int)Math.Floor(availableExtent - extentUsed), 0);

					if (extraPixels > 0)
					{
						extraPixelRestoreIndex = extraPixels;
						timeSlotExtent++;

						if (isVertical)
							itemSize.Height++;
						else
							itemSize.Width++;
					}
				}
			}

			CoreUtilities.SortMergeGeneric(newItemList, Comparer<int>.Default);

			#endregion //Setup for create/measure

			#region Create/Measure children
			DebugHelper.DebugLayout(panel, false, true, "RealizeStack Measure Start", null);

			// AS 5/9/12 TFS104555
			bool calculateTimeslotExtent = double.IsInfinity(timeSlotExtent);
			double maxDesiredTimeslotExtent = 0;

			for (int i = 0, count = newItemList.Count; i < count; i++)
			{
				// AS 1/13/12 TFS74252
				// Once we get past the first n items, we can restore the itemRect/Extent such 
				// that they don't have the extra pixel.
				//
				if (i == extraPixelRestoreIndex)
				{
					if (isVertical)
						itemSize.Height--;
					else
						itemSize.Width--;

					timeSlotExtent--;
				}

				int index = newItemList[i];
				ISupportRecycling item = items[index] as ISupportRecycling;

				FrameworkElement container = item.AttachedElement;
				bool? isNewlyRealized = null;

				if (container == null)
				{
					isNewlyRealized = RecyclingManager.Manager.AttachElement(item, panel);
					container = item.AttachedElement;
				}

				if (initializer != null)
				{
					InitializeItemState state = isNewlyRealized == null
						? InitializeItemState.ExistingContainer
						: isNewlyRealized == true
							? InitializeItemState.NewContainer
							: InitializeItemState.RecycledContainer;

					initializer(item, index, container, state);
				}


				elementsInView.Add(container);

				PanelHelper.MeasureElement(container, itemSize);

				// track the non-arrange orientation extent
				Size childDesired = container.DesiredSize;

				DebugHelper.DebugLayout(container, "Measured Child", "MeasureSize:{0}, DesiredSize:{1}", itemSize, container.DesiredSize);

				if (isVertical && childDesired.Width > desired.Width)
					desired.Width = childDesired.Width;
				else if (!isVertical && childDesired.Height > desired.Height)
					desired.Height = childDesired.Height;

				// AS 5/9/12 TFS104555
				if (calculateTimeslotExtent)
					maxDesiredTimeslotExtent = Math.Max(isVertical ? childDesired.Height : childDesired.Width, maxDesiredTimeslotExtent);
			}

			// AS 5/9/12 TFS104555
			if (calculateTimeslotExtent)
				timeSlotExtent = maxDesiredTimeslotExtent;

			DebugHelper.DebugLayout(panel, true, false, "RealizeStack Measure End", null);

			#endregion //Create/Measure children

			if (itemCount == 0)
				firstVisibleRealizedIndex = 0;
			else
				firstVisibleRealizedIndex = newItemList.BinarySearch(firstItemIndex);

			Debug.Assert(firstVisibleRealizedIndex >= 0);

			return desired;
		}
		#endregion //RealizeStack

		#endregion // Public Methods

		#region Internal Methods

		// AS 4/20/11 TFS73203
		#region CalculateStackExtraPixelCount
		internal static int CalculateStackExtraPixelCount(Size finalSize, double interItemSpacing, bool isVertical, int itemCount, double columnExtent)
		{
			double extent = PanelHelper.CalculateExtent(itemCount, interItemSpacing, columnExtent);
			double availableExtent = isVertical ? finalSize.Height : finalSize.Width;
			int extraPixels = 0;

			if (CoreUtilities.GreaterThan(availableExtent, extent) && CoreUtilities.GreaterThan(extent, 0))
			{
				extraPixels = (int)Math.Max(Math.Floor(availableExtent - extent), 0);

				Debug.Assert(extraPixels <= itemCount, "The element is arranged more than just a couple of pixels bigger than it was measured?");

				if (extraPixels >= itemCount)
					extraPixels = itemCount;
				else
					extraPixels = extraPixels % itemCount;
			}

			return extraPixels;
		}
		#endregion //CalculateStackExtraPixelCount

		// AS 12/13/10 TFS61517
		#region ReleaseElements
		/// <summary>
		/// Helper method for invoking the RecyclingManager's ReleaseElement method on each item in the specified list of containers for the specified panel.
		/// </summary>
		/// <param name="panel">The panel that is the parent of the specified items</param>
		/// <param name="elementsInView">The elements that are the containers for recyclable items</param>
		internal static void ReleaseElements( Panel panel, IList<FrameworkElement> elementsInView )
		{
			for ( int i = elementsInView.Count - 1; i >= 0; i-- )
			{
				FrameworkElement elem = elementsInView[i];
				var item = RecyclingManager.Manager.ItemFromElement(elem);

				if ( item != null && !RecyclingManager.Manager.ReleaseElement(item, panel) )
				{
					Debug.Assert(false, "Unable to release an element from the old list!");
					continue;
				}

				elementsInView.RemoveAt(i);
			}
		}
		#endregion // ReleaseElements 

		#endregion // Internal Methods

		#region Private Methods

		#region GetInViewFocusedIndex
		private static int GetInViewFocusedIndex(Panel panel, IList<FrameworkElement> elementsInView)
		{
			if (PresentationUtilities.HasFocus(panel))
			{
				var children = elementsInView;

				for (int i = 0, count = children.Count; i < count; i++)
				{
					FrameworkElement child = children[i];

					if (null != child && PresentationUtilities.HasFocus(child))
					{
						return i;
					}
				}
			}

			return -1;
		}
		#endregion //GetInViewFocusedIndex 

		#endregion // Private Methods

		#region InitializeItemState enum
		internal enum InitializeItemState
		{
			/// <summary>
			/// The item was already associated with a container
			/// </summary>
			ExistingContainer,

			/// <summary>
			/// The item was just associated with an existing/recycled container
			/// </summary>
			RecycledContainer,

			/// <summary>
			/// The item was just associated with a new container.
			/// </summary>
			NewContainer,
		} 
		#endregion // InitializeItemState enum
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