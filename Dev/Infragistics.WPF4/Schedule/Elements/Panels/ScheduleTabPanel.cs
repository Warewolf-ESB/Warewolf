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
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom items panel used to display items with their preferred size unless there isn't enough room.
	/// </summary>
	public class ScheduleTabPanel : ScheduleItemsPanel
	{
		#region Member Variables

		private List<TabItemInfo> _infos;
		private bool _releasingElements; // AS 12/13/10 TFS61517

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ScheduleTabPanel"/>
		/// </summary>
		public ScheduleTabPanel()
		{
			_infos = new List<TabItemInfo>();
		}
		#endregion // Constructor

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

			// out of view elements
			PanelHelper.ArrangeReleasedElements(this);

			Size arrangeSize = finalSize;

			#region Arrange Elements
			bool isVertical = this.IsVertical;
			Rect arrangeRect = new Rect(new Point(), finalSize);
			double interTabSpacing = this.InterTabSpacing;

			foreach (TabItemInfo item in _infos)
			{
				UIElement container = item.Element;

				if (null != container)
				{
					// use the desired size for the arrange orientation and the arrange extent for the other
					if (isVertical)
						arrangeRect.Height = item.Extent;
					else
						arrangeRect.Width = item.Extent;

					container.Arrange(arrangeRect);

					if (isVertical)
						arrangeRect.Y += arrangeRect.Height + interTabSpacing;
					else
						arrangeRect.X += arrangeRect.Width + interTabSpacing;
				}
			}
			#endregion // Arrange Elements

			DebugHelper.DebugLayout(this, true, false, "ArrangeOverride End", "FinalSize: {0}, ArrangeSize:{1}", finalSize, arrangeSize);

			return arrangeSize;
		}
		#endregion //ArrangeOverride

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			DebugHelper.DebugLayout(this, false, true, "MeasureOverride Start", "AvailableSize: {0}", availableSize);

			Size desired = new Size();
			IList<ISupportRecycling> items = this.RecyclableItems;
			HashSet<UIElement> oldElements = new HashSet<UIElement>();

			foreach (TabItemInfo oldInfo in _infos)
				oldElements.Add(oldInfo.Element);

			_infos.Clear();

			if (null != items)
			{
				bool isVertical = this.IsVertical;
				Size measureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
				int itemsAdded = 0;
				double otherExtent;

				int lastIndex = items.Count - 1;

				for(int i = 0, count = items.Count; i < count; i++)
				{
					ISupportRecycling item = items[i];
					FrameworkElement container = item.AttachedElement;

					if (container == null)
					{
						bool isNewlyRealized = RecyclingManager.Manager.AttachElement(item, this);
						container = item.AttachedElement;
					}

					ScheduleUtilities.SetBoolTrueProperty(container, ScheduleItemsPanel.IsFirstItemProperty, i == 0);
					ScheduleUtilities.SetBoolTrueProperty(container, ScheduleItemsPanel.IsLastItemProperty, i == lastIndex);

					// remove the items from the old items
					oldElements.Remove(container);

					if (container.Visibility == Visibility.Collapsed)
						continue;

					PanelHelper.MeasureElement(container, measureSize);

					Size itemDesired = container.DesiredSize;

					// we're measuing with infinity because a calendargroupheader contains a path with stretch
					// and when a shape with a stretch is measured with a non-infinite value it will just 
					// return that value. so to deal with that we'll measure with infinity so the shape can 
					// return its natural size. if and only if that natural size needs to be clipped will we 
					// remeasure with the constrained size that was provided
					if (isVertical && itemDesired.Width > availableSize.Width)
					{
						PanelHelper.MeasureElement(container, new Size(availableSize.Width, measureSize.Height));
						itemDesired = container.DesiredSize;
						otherExtent = availableSize.Width;
					}
					else if (!isVertical && itemDesired.Height > availableSize.Height)
					{
						PanelHelper.MeasureElement(container, new Size(measureSize.Width, availableSize.Height));
						itemDesired = container.DesiredSize;
						otherExtent = availableSize.Height;
					}
					else
					{
						otherExtent = double.PositiveInfinity;
					}

					TabItemInfo tabInfo = new TabItemInfo(container, i);
					tabInfo.MinExtent = isVertical ? container.MinHeight : container.MinWidth;
					tabInfo.Extent = isVertical ? itemDesired.Height : itemDesired.Width;
					tabInfo.OtherExtent = otherExtent;

					_infos.Add(tabInfo);

					if (isVertical)
					{
						desired.Height += itemDesired.Height;
						desired.Width = Math.Max(desired.Width, itemDesired.Width);
					}
					else
					{
						desired.Width += itemDesired.Width;
						desired.Height = Math.Max(desired.Height, itemDesired.Height);
					}

					itemsAdded++;
				}

				double spacing = itemsAdded == 0 ? 0 : (itemsAdded - 1) * this.InterTabSpacing;

				if (isVertical)
					desired.Height += spacing;
				else
					desired.Width += spacing;

				// if using the preferred size we cannot fit everything then we need to 
				// reduce the elements down towards their minimum
				double excess = isVertical
					? availableSize.Height - desired.Height
					: availableSize.Width - desired.Width;

				if (!CoreUtilities.AreClose(excess, 0)
					&& !double.IsInfinity(excess) // AS 5/9/12 TFS104555
					)
				{
					switch (this.TabLayoutStyle)
					{
						case ScheduleTabLayoutStyle.SingleRowAutoSize:
							break;
						case ScheduleTabLayoutStyle.SingleRowJustified:
							if (excess < 0)
								this.AdjustTabsProportionally(excess, ref desired);
							break;
						case ScheduleTabLayoutStyle.SingleRowSizeToFit:
							this.AdjustTabsProportionally(excess, ref desired);
							break;
					}
				}
			}

			#region Release Unused Elements
			// AS 12/13/10 TFS61517
			// Added a flag so we know when we are explicitly releasing to avoid walking 
			// over the children.
			//
			bool wasReleasing = _releasingElements;
			_releasingElements = true;

			try
			{
			foreach ( FrameworkElement element in oldElements )
			{
				ISupportRecycling item = RecyclingManager.Manager.ItemFromElement(element);

				Debug.Assert(null != item, "The element is not associated with an item");
				Debug.Assert(null != item || !this.Children.Contains(element));

				if ( null != item )
				{
					bool released = RecyclingManager.Manager.ReleaseElement(item, this);
					Debug.Assert(released);
				}
			}
			}
			finally
			{
				_releasingElements = wasReleasing;
			}
			#endregion // Release Unused Elements

			DebugHelper.DebugLayout(this, true, false, "MeasureOverride End", "AvailableSize: {0}, Desired: {1}", availableSize, desired);

			return desired;
		}

		#endregion //MeasureOverride

		#region OnElementReleased
		internal override void OnElementReleased( ISupportRecycling item, FrameworkElement element, bool isRemoved )
		{
			// AS 12/13/10 TFS61517
			// We need to remvoe an item whenever an element is removed or released.
			//
			//if ( isRemoved )
			if ( !_releasingElements )
			{
				for ( int i = 0, count = _infos.Count; i < count; i++ )
				{
					if ( _infos[i].Element == element )
					{
						_infos.RemoveAt(i);
						break;
					}
				}
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
			_releasingElements = true;

			try
			{
				for ( int i = _infos.Count - 1; i >= 0; i-- )
				{
					var info = _infos[i];
					var element = info.Element;

					if ( element != null )
					{
						var item = RecyclingManager.Manager.ItemFromElement(element);

						if ( item == null || !RecyclingManager.Manager.ReleaseElement(item, this) )
						{
							Debug.Assert(false, "Unable to release element but the list is being released.");
							continue;
						}
					}

					_infos.RemoveAt(i);
				}
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

		#region InterTabSpacing

		/// <summary>
		/// Identifies the <see cref="InterTabSpacing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTabSpacingProperty = DependencyProperty.Register("InterTabSpacing",
			typeof(double), typeof(ScheduleTabPanel),
			DependencyPropertyUtilities.CreateMetadata(0d, new PropertyChangedCallback(OnInterTabSpacingChanged))
			);

		private static void OnInterTabSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTabPanel item = (ScheduleTabPanel)d;
			item.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the amount of space between each item.
		/// </summary>
		/// <seealso cref="InterTabSpacingProperty"/>
		public double InterTabSpacing
		{
			get
			{
				return (double)this.GetValue(ScheduleTabPanel.InterTabSpacingProperty);
			}
			set
			{
				this.SetValue(ScheduleTabPanel.InterTabSpacingProperty, value);
			}
		}

		#endregion //InterTabSpacing

		#endregion // Public Properties

		#region Internal Properties

		#region TabLayoutStyle

		/// <summary>
		/// Identifies the <see cref="TabLayoutStyle"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty TabLayoutStyleProperty = DependencyProperty.Register("TabLayoutStyle",
			typeof(ScheduleTabLayoutStyle), typeof(ScheduleTabPanel),
			DependencyPropertyUtilities.CreateMetadata(ScheduleTabLayoutStyle.SingleRowJustified, new PropertyChangedCallback(OnTabLayoutStyleChanged))
			);

		private static void OnTabLayoutStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTabPanel item = (ScheduleTabPanel)d;
		}

		/// <summary>
		/// Returns or sets an enumeration that determines how the items are arranged.
		/// </summary>
		/// <seealso cref="TabLayoutStyleProperty"/>
		internal ScheduleTabLayoutStyle TabLayoutStyle
		{
			get
			{
				return (ScheduleTabLayoutStyle)this.GetValue(ScheduleTabPanel.TabLayoutStyleProperty);
			}
			set
			{
				this.SetValue(ScheduleTabPanel.TabLayoutStyleProperty, value);
			}
		}

		#endregion //TabLayoutStyle

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region AdjustTabsProportionallyImpl

		private double AdjustTabsProportionallyImpl(List<TabItemInfo> sortedTabs,
			bool hasMultiplePriorities,
			bool isVertical,
			double adjustment,
			double maxPerTabAdjustment)
		{
			bool expanding = adjustment > 0;
			int tabCount = sortedTabs.Count;
			double totalAdjustmentsMade = 0;
			int startingIndex = 0;
			int tabsWithPriority;
			int? priority;

			// AS 1/9/08 BR29523
			//while (adjustment != 0 && startingIndex < tabCount)
			while (false == CoreUtilities.AreClose(adjustment, 0) && startingIndex < tabCount)
			{
				priority = null;
				tabsWithPriority = 0;
				int i = startingIndex;

				// AS 10/25/07
				double currentExtent = 0d;
				double? nextExtent = null;

				for (; i < tabCount; i++)
				{
					TabItemInfo tab = sortedTabs[i];

					if (double.IsNaN(tab.Extent))
						continue;
					else if (priority == null)
					{
						priority = tab.Priority;
						tabsWithPriority++;

						// AS 10/25/07
						// Keep track of the current extent of this tab since we only
						// want to process the tabs with this size so that we can decrease
						// the largest tabs first and then continue with these tabs and the 
						// next set of tabs.
						//
						currentExtent = tab.Extent;
					}
					else if (priority != tab.Priority)
						break;
					// AS 10/25/07
					else if (expanding == false && CoreUtilities.AreClose(tab.Extent, currentExtent) == false)
					{
						nextExtent = tab.Extent;
						break;
					}
					else
						tabsWithPriority++;
				}

				if (tabsWithPriority > 0)
				{
					// AS 10/25/07
					// Resize this set of tabs down to the next extent.
					//
					double sign = adjustment < 0 ? -1d : 1d;
					// AS 1/4/08 BR29413
					// Since we're treating the adjustment as absolute and then we also need to do that with 
					// the difference between the extent being processed and the next extent or we could
					// end up trying to adjust more than what we were asked to adjust.
					//
					//double currentAdjustment = expanding == false && nextExtent != null ? sign * Math.Min(Math.Abs(adjustment), (currentExtent - (double)nextExtent) * tabsWithPriority) : adjustment;
					double currentAdjustment = expanding == false && nextExtent != null ? sign * Math.Min(Math.Abs(adjustment), Math.Abs(currentExtent - (double)nextExtent) * tabsWithPriority) : adjustment;

					double adjustmentMade = AdjustTabsProportionallyImpl(sortedTabs,
						isVertical,
						startingIndex,
						tabsWithPriority,
						// AS 10/25/07
						//adjustment,
						currentAdjustment,
						expanding,
						maxPerTabAdjustment);

					// then reduce the adjustment and see if there is any adjustment that 
					// can be made to the other priority level.
					adjustment -= adjustmentMade;
					totalAdjustmentsMade += adjustmentMade;

					// AS 10/25/07
					// If we're reducing the tabs then we're just processing all the items with 
					// a particular size and since we're not done with this priority keep starting
					// with the current starting index.
					//
					// AS 1/9/08 BR29523
					//if (adjustmentMade != 0d && expanding == false && nextExtent != null)
					if (expanding == false && nextExtent != null && false == CoreUtilities.AreClose(adjustmentMade, 0))
						continue;
				}

				startingIndex = i;
			}

			return totalAdjustmentsMade;
		}

		private double AdjustTabsProportionallyImpl(List<TabItemInfo> tabs,
			bool isVertical,
			int startIndex,
			int tabsToAdjust,
			double adjustment,
			bool expanding,
			double maxPerTabAdjustment)
		{
			int tabsAdjusted = 0;
			double tabAdjustment = 0;
			double tabExtent, minExtent;
			int tabCount = tabs.Count;
			double totalAdjustmentMade = 0;

			int count = tabs.Count;

			for (int i = startIndex; i < tabCount; i++)
			{
				TabItemInfo tab = tabs[i];

				if (double.IsNaN(tab.Extent))
					continue;

				tabExtent = tab.Extent;
				minExtent = tab.MinExtent;

				tabAdjustment = adjustment / (tabsToAdjust - tabsAdjusted);

				if (expanding && maxPerTabAdjustment > 0 && tabAdjustment > maxPerTabAdjustment)
					tabAdjustment = maxPerTabAdjustment;
				else if (false == expanding && maxPerTabAdjustment < 0 && tabAdjustment < maxPerTabAdjustment)
					tabAdjustment = maxPerTabAdjustment;

				tabExtent += tabAdjustment;

				// enforce the max tab width
				if (tabExtent < minExtent)
				{
					tabAdjustment += minExtent - tabExtent;
					tabExtent += minExtent - tabExtent;
				}

				tab.Extent = tabExtent;
				tab.AdjustmentMade += tabAdjustment;

				totalAdjustmentMade += tabAdjustment;

				adjustment -= tabAdjustment;
				tabsAdjusted++;

				if (tabsAdjusted == tabsToAdjust || adjustment == 0)
					return totalAdjustmentMade;

				// JJD 3/25/03
				// If we are expanding return when adjustement goes negative
				// otherwise return when adjustement goes positiove
				if (expanding == true)
				{
					if (adjustment < 0)
						return totalAdjustmentMade;
				}
				else
				{
					if (adjustment > 0)
						return totalAdjustmentMade;
				}
			}

			return totalAdjustmentMade;

		}

		#endregion //AdjustTabsProportionallyImpl

		#region AdjustTabsProportionally
		private void AdjustTabsProportionally(double excess, ref Size desired)
		{
			List<TabItemInfo> infos = new List<TabItemInfo>(_infos);
			bool isVertical = this.IsVertical;
			IList<ISupportRecycling> items = this.RecyclableItems;

			// sort to get the largest tabs first
			CoreUtilities.SortMergeGeneric(infos, Comparer<TabItemInfo>.Default);

			double adjustmentMade = AdjustTabsProportionallyImpl(infos, false, isVertical, excess, 0);

			if (!ScheduleUtilities.AreClose(adjustmentMade, 0))
			{
				#region Measure with the adjusted size

				Size measureSize = new Size();

				foreach (TabItemInfo info in infos)
				{
					if (info.AdjustmentMade != 0)
					{
						UIElement element = info.Element;

						if (isVertical)
						{
							measureSize.Height = info.Extent;
							measureSize.Width = info.OtherExtent;
						}
						else
						{
							measureSize.Width = info.Extent;
							measureSize.Height = info.OtherExtent;
						}

						PanelHelper.MeasureElement(element, measureSize);

						if (isVertical)
							desired.Width = Math.Max(desired.Width, element.DesiredSize.Width);
						else
							desired.Height = Math.Max(desired.Height, element.DesiredSize.Height);
					}
				}

				if (isVertical)
					desired.Height = Math.Max(0, desired.Height + adjustmentMade);
				else
					desired.Width = Math.Max(0, desired.Width + adjustmentMade);

				#endregion // Measure with the adjusted size
			}
		}
		#endregion // AdjustTabsProportionally

		#endregion // Methods

		#region TabItemInfo class
		private class TabItemInfo : IComparable<TabItemInfo>
		{
			internal TabItemInfo(FrameworkElement element, int childIndex)
			{
				Debug.Assert(element != null, "The TabItemInfo MUST have an element");

				this.ChildIndex = childIndex;
				this.Element = element;
			}

			internal int ChildIndex;
			internal double Extent;
			internal double OtherExtent;
			internal double AdjustmentMade;
			internal double MinExtent;
			internal int Priority = 0;
			internal FrameworkElement Element;

			#region IComparable<TabItemInfo> Members

			int IComparable<TabItemInfo>.CompareTo(TabItemInfo other)
			{
				// larger items first
				int diff = other.Extent.CompareTo(this.Extent);

				if (diff == 0)
					diff = this.ChildIndex.CompareTo(other.ChildIndex);

				return diff;
			}

			#endregion
		}
		#endregion //TabItemInfo class
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