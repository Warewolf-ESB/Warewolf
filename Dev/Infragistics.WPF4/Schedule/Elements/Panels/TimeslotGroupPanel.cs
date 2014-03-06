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
using System.Collections.Generic;
using Infragistics.Controls.Primitives;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom panel used to display elements for groups of timeslots.
	/// </summary>
	public class TimeslotGroupPanel : TimeslotPanelBase
		, IRecyclableElementHost
	{
		#region Member Variables

		private TimeslotPositionInfo _positionInfo;
		private List<Group> _groups;
		private ITimeslotGroupProvider _groupProvider;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotGroupPanel"/>
		/// </summary>
		public TimeslotGroupPanel()
		{
			_positionInfo = new TimeslotPositionInfo(this);
			_groups = new List<Group>();
		} 
		#endregion // Constructor

		#region Base class overrides

		#region ArrangeOverrideImpl
		internal override Size ArrangeOverrideImpl(Size finalSize)
		{
			DebugHelper.DebugLayout(this, false, true, "ArrangeOverride Start", "FinalSize: {0}", finalSize);

			// arrange any elements that are out of view now...
			PanelHelper.ArrangeReleasedElements(this);

			#region Position the groups

			Rect arrangeRect = Rect.Empty;
			Rect finalRect = new Rect(0, 0, finalSize.Width, finalSize.Height);
			bool isVertical = this.IsVertical;
			Rect firstTimeslotRect = PanelHelper.GetStartingArrangeRect(isVertical, finalSize, this.ActualTimeslotExtent, this.InterItemSpacing, this.FirstTimeslotIndex, this.TimeslotCount);
			double offset = isVertical ? firstTimeslotRect.Y : firstTimeslotRect.X;

			DebugHelper.DebugLayout(this, false, true, "Items Start", null);

			for (int i = 0, count = _groups.Count; i < count; i++)
			{
				Group group = _groups[i];
				FrameworkElement container = group.Item.AttachedElement;

				if (container == null)
					continue;

				Rect itemRect;

				if (group.Position == null)
				{
					// we have an element but not a position which means we tried to 
					// release it but couldn't so just position it out of view
					Size outOfViewSize = PanelHelper.GetOutOfViewMeasureSize(container);
					itemRect = new Rect(0, 0, outOfViewSize.Width, outOfViewSize.Height);
				}
				else
				{
					ItemPosition position = group.Position.Value;
					itemRect = isVertical
						? new Rect(0, position.Start, finalSize.Width, position.Extent)
						: new Rect(position.Start, 0, position.Extent, finalSize.Height);
				}

				container.Arrange(itemRect);

				DebugHelper.DebugLayout(container, "Arranging Child", "ItemRect:{0}", itemRect);

				arrangeRect.Union(itemRect);

			}

			DebugHelper.DebugLayout(this, true, false, "Items End", null); 

			#endregion // Position the groups

			// to ensure the activity is clipped to the panel union the rects of the arranged 
			// portions with that of the available
			arrangeRect.Union(finalRect);

			Size arrangeSize = new Size(arrangeRect.Width, arrangeRect.Height);

			DebugHelper.DebugLayout(this, true, false, "ArrangeOverride End", "FinalSize: {0}, ArrangeSize:{1}", finalSize, arrangeSize);

			return arrangeSize;
		}
		#endregion // ArrangeOverrideImpl

		#region MeasureOverrideImpl
		internal override Size MeasureOverrideImpl(Size availableSize, int firstItemIndex, int pageSize)
		{
			bool isVertical = this.IsVertical;
			Size desired = new Size();
			int startIndex, endIndex;
			PanelHelper.GetStackStartAndEnd(availableSize, firstItemIndex, pageSize, isVertical, this.ActualTimeslotExtent, this.InterItemSpacing, false, this.TimeslotCount, out startIndex, out endIndex);

			// ensure the position info is up to date
			_positionInfo.Initialize();

			// now we have the range of timeslots for which we are displaying labels so we can determine which labels we need
			if (_groupProvider != null)
			{
				HashSet<Group> oldGroups = new HashSet<Group>(_groups);
				GroupMeasureInfo measureInfo = new GroupMeasureInfo(_groups, availableSize, this, startIndex, endIndex);

				_groupProvider.InitializeGroups(measureInfo);

				#region Recycle Released Elements

				// remove all the groups still being used
				foreach (Group group in _groups)
					oldGroups.Remove(group);

				// anything that is left needs to be released
				foreach (Group group in oldGroups)
				{
					// if a group cannot be released then put it back into the list
					// since we will need to position and maintain it
					if (!RecyclingManager.Manager.ReleaseElement(group.Item, this))
						_groups.Add(group);
				} 

				#endregion // Recycle Released Elements

				#region Calculate in view rect

				// we want to constrain the elemnts to the in view rect so we need to calculate it 
				// during the measure to intersect with the item's rect
				Rect firstItemOffset = PanelHelper.GetStartingArrangeRect(isVertical, availableSize, this.ActualTimeslotExtent, this.InterItemSpacing, this.FirstTimeslotIndex, this.TimeslotCount);
				Rect inViewRect;

				if (pageSize != 0)
				{
					Debug.Assert(endIndex >= startIndex && endIndex >= 0, "Invalid end index:" + endIndex.ToString());
					double inViewStart = _positionInfo.GetItemPosition(startIndex).Start;
					double inViewEnd = _positionInfo.GetItemPosition(endIndex).End;

					inViewRect = isVertical
						? new Rect(0, inViewStart + firstItemOffset.Y, availableSize.Width, inViewEnd - inViewStart)
						: new Rect(inViewStart + firstItemOffset.X, 0, inViewEnd - inViewStart, availableSize.Height);
				}
				else
				{
					inViewRect = Rect.Empty;
				}

				// intersect that with the available rect to get the portion of those slots that are in view
				Rect availableRect = new Rect(0, 0, availableSize.Width, availableSize.Height);
				inViewRect.Intersect(availableRect); 

				#endregion // Calculate in view rect

				// sort the groups by the start/end date
				_groups.Sort();

				int groupCount = _groups.Count;
				Rect[] measureRects = new Rect[groupCount];

				#region Calculate measure rect
				// try to release anything that isn't in view so we can reuse them in this same measure pass
				for (int i = groupCount - 1; i >= 0; i--)
				{
					Group group = _groups[i];
					ItemPosition? position = _positionInfo.GetItemPosition(group.Start, group.End);
					Rect measureItemRect;

					if (position != null)
					{
						// we have a position but we still need to know if its in view
						measureItemRect = availableRect;

						if (isVertical)
						{
							measureItemRect.Y = position.Value.Start + firstItemOffset.Y;
							measureItemRect.Height = position.Value.Extent;
						}
						else
						{
							measureItemRect.X = position.Value.Start + firstItemOffset.X;
							measureItemRect.Width = position.Value.Extent;
						}
					}
					else
					{
						measureItemRect = Rect.Empty;
					}

					// find out how much of the element is actually in view
					Rect actualItemRect = measureItemRect;
					actualItemRect.Intersect(inViewRect);

					// if its completely out of view we can release the element
					if (!actualItemRect.IsEmpty)
					{
						measureRects[i] = measureItemRect;
					}
					else
					{
						// if its not going to be in view then store an empty rect so we know to skip it below
						measureRects[i] = Rect.Empty;

						// don't store a position - we'll position it offscreen if we can't release the element
						group.Position = null;

						FrameworkElement container = group.Item.AttachedElement;

						// if there is an element then we need to try and release it
						if (null != container)
						{
							if (!RecyclingManager.Manager.ReleaseElement(group.Item, this))
							{
								// if the element couldn't be released then just measure it 
								// with the last measure size and clear the position so we 
								// position it out of view later
								Size measureSize = PanelHelper.GetOutOfViewMeasureSize(container);
								container.Measure(measureSize);
							}
						}
					}
				} 
				#endregion // Calculate measure rect

				for (int i = groupCount - 1; i >= 0; i--)
				{
					Rect measureItemRect = measureRects[i];

					// skip anything we released
					if (measureItemRect.IsEmpty)
						continue;

					Rect originalItemRect = measureItemRect;
					Rect actualItemRect = measureItemRect;
					actualItemRect.Intersect(inViewRect);

					Group group = _groups[i];

					FrameworkElement container = group.Item.AttachedElement;

					// otherwise make sure we have an element and measure it with the size of the rect
					// make sure it has an element associated with it
					if (container == null)
					{
						bool isNewlyRealized = RecyclingManager.Manager.AttachElement(group.Item, this);
						container = group.Item.AttachedElement;
					}

					// clear the property since the element's size may be affected by this
					container.ClearValue(IsClippedPropertyKey);
					container.ClearValue(IsEndInViewPropertyKey);
					container.ClearValue(IsStartInViewPropertyKey);

					// measure with the measure size. note I could use infinity but then 
					// the element wouldn't have the option of using the measure size to 
					// alter its contents
					Size itemSize = new Size(measureItemRect.Width, measureItemRect.Height);
					PanelHelper.MeasureElement(container, itemSize);

					// track the non-arrange orientation extent
					Size childDesired = container.DesiredSize;

					DebugHelper.DebugLayout(container, "Measured Child Initial", "MeasureSize:{0}, DesiredSize:{1}", itemSize, container.DesiredSize);

					// see if the element will be clipped
					bool isClipped = (isVertical && CoreUtilities.GreaterThan(childDesired.Height, actualItemRect.Height))
						|| (!isVertical && CoreUtilities.GreaterThan(childDesired.Width, actualItemRect.Width));

					// set a flag so the element knows whether its fully in view or not

					if (isClipped)
					{
						container.SetValue(IsClippedPropertyKey, KnownBoxes.TrueBox);

						// if the element is clipped then we have a couple of options. we can set 
						// an attached property on the element to indicate its clipped or we can 
						// position it based on its desired size and let it get clipped
						if (!isVertical)
						{
							// if its clipped on the right then constrain the width so its left aligned
							if (CoreUtilities.LessThan(actualItemRect.Right, measureItemRect.Right))
							{
								actualItemRect.Width = childDesired.Width;
							}
							else // otherwise shift the left so the element will be right aligned
							{
								actualItemRect.X = actualItemRect.Right - childDesired.Width;
								actualItemRect.Width = childDesired.Width;
							}
						}
						else
						{
							// if its clipped on the bottom then constrain the width so its top aligned
							if (CoreUtilities.LessThan(actualItemRect.Bottom, measureItemRect.Bottom))
							{
								actualItemRect.Height = childDesired.Height;
							}
							else // otherwise shift the up so the element will be bottom aligned
							{
								actualItemRect.Y = actualItemRect.Bottom - childDesired.Height;
								actualItemRect.Height = childDesired.Height;
							}
						}
					}

					if ((!isVertical && !CoreUtilities.AreClose(originalItemRect.Left, actualItemRect.Left)) ||
						(isVertical && !CoreUtilities.AreClose(originalItemRect.Top, actualItemRect.Top)))
					{
						container.SetValue(IsStartInViewPropertyKey, KnownBoxes.FalseBox);
					}

					if ((!isVertical && !CoreUtilities.AreClose(originalItemRect.Right, actualItemRect.Right)) ||
						(isVertical && !CoreUtilities.AreClose(originalItemRect.Bottom, actualItemRect.Bottom)))
					{
						container.SetValue(IsEndInViewPropertyKey, KnownBoxes.FalseBox);
					}

					double offset = isVertical ? actualItemRect.Y : actualItemRect.X;
					double extent = isVertical ? actualItemRect.Height : actualItemRect.Width;

					// note we're storing the shifted position so we can just use this value in the arrange
					group.Position = new ItemPosition(offset, extent);

					if (isVertical && childDesired.Width > desired.Width)
						desired.Width = childDesired.Width;
					else if (!isVertical && childDesired.Height > desired.Height)
						desired.Height = childDesired.Height;
				}
			}

			return desired;
		}
		#endregion // MeasureOverrideImpl

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region IsClipped

		private static readonly DependencyPropertyKey IsClippedPropertyKey = DependencyPropertyUtilities.RegisterAttachedReadOnly("IsClipped",
			typeof(bool), typeof(TimeslotGroupPanel), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only IsClipped attached dependency property
		/// </summary>
		/// <seealso cref="GetIsClipped"/>
		public static readonly DependencyProperty IsClippedProperty = IsClippedPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the value of the attached IsClipped DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="IsClippedProperty"/>
		public static bool GetIsClipped(DependencyObject d)
		{
			return (bool)d.GetValue(TimeslotGroupPanel.IsClippedProperty);
		}

		#endregion //IsClipped

		#region IsStartInView

		private static readonly DependencyPropertyKey IsStartInViewPropertyKey = DependencyPropertyUtilities.RegisterAttachedReadOnly("IsStartInView",
			typeof(bool), typeof(TimeslotGroupPanel), KnownBoxes.TrueBox, null);

		/// <summary>
		/// Identifies the read-only IsStartInView attached dependency property
		/// </summary>
		/// <seealso cref="GetIsStartInView"/>
		public static readonly DependencyProperty IsStartInViewProperty = IsStartInViewPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets a boolean indicating if the leading edge of the element is in view.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="IsStartInViewProperty"/>
		public static bool GetIsStartInView(DependencyObject d)
		{
			return (bool)d.GetValue(TimeslotGroupPanel.IsStartInViewProperty);
		}

		#endregion //IsStartInView

		#region IsEndInView

		private static readonly DependencyPropertyKey IsEndInViewPropertyKey = DependencyPropertyUtilities.RegisterAttachedReadOnly("IsEndInView",
			typeof(bool), typeof(TimeslotGroupPanel), KnownBoxes.TrueBox, null);

		/// <summary>
		/// Identifies the read-only IsEndInView attached dependency property
		/// </summary>
		/// <seealso cref="GetIsEndInView"/>
		public static readonly DependencyProperty IsEndInViewProperty = IsEndInViewPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets a boolean indicating if the trailing edge of the element is in view.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="IsEndInViewProperty"/>
		public static bool GetIsEndInView(DependencyObject d)
		{
			return (bool)d.GetValue(TimeslotGroupPanel.IsEndInViewProperty);
		}

		#endregion //IsEndInView

		#endregion // Public Properties

		#region Internal Properties

		#region GroupProvider
		internal ITimeslotGroupProvider GroupProvider
		{
			get { return _groupProvider; }
			set
			{
				Debug.Assert(null == _groupProvider || value == _groupProvider, "This is not setup to handle reuse right now");
				_groupProvider = value;
				this.InvalidateMeasure();
			}
		}
		#endregion // GroupProvider

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods
		private void OnElementRemoved(ISupportRecycling item, FrameworkElement element)
		{
			for (int i = 0, count = _groups.Count; i < count; i++)
			{
				Group group = _groups[i];

				if (group.Item == item)
				{
					_groups.RemoveAt(i);
					break;
				}
			}
		} 
		#endregion // Methods

		#region IRecyclableElementHost Members

		void IRecyclableElementHost.OnElementAttached(ISupportRecycling item, FrameworkElement element, bool isNewlyRealized)
		{
			if (isNewlyRealized)
				ScheduleControlBase.SetControl(element, ScheduleControlBase.GetControl(this));
		}

		void IRecyclableElementHost.OnElementReleased(ISupportRecycling item, FrameworkElement element, bool isRemoved)
		{
			if (isRemoved)
			{
				this.OnElementRemoved(item, element);

				element.ClearValue(ScheduleControlBase.ControlProperty);
			}
		}

		bool IRecyclableElementHost.ShouldRemove(ISupportRecycling item, FrameworkElement element)
		{
			return false;
		}

		#endregion //IRecyclableElementHost Members

		#region GroupMeasureInfo class
		internal class GroupMeasureInfo
		{
			internal readonly Size AvailableSize;
			internal readonly int FirstIndex;
			internal readonly int LastIndex;
			internal readonly TimeslotGroupPanel Panel;
			internal readonly List<Group> Groups;
			internal readonly TimeslotPositionInfo PositionInfo;

			#region Constructor
			internal GroupMeasureInfo(List<Group> groups, Size availableSize, TimeslotGroupPanel panel, int firstIndex, int lastIndex)
			{
				this.AvailableSize = availableSize;
				this.Groups = groups;
				this.Panel = panel;
				this.FirstIndex = firstIndex;
				this.LastIndex = lastIndex;
				this.PositionInfo = panel._positionInfo;
			}
			#endregion // Constructor
		} 
		#endregion // GroupMeasureInfo class

		#region Group class
		internal class Group : IComparable<Group>
		{
			internal readonly ISupportRecycling Item;
			internal DateTime Start;
			internal DateTime End;
			internal ItemPosition? Position;

			#region Constructor
			internal Group(ISupportRecycling item)
			{
				CoreUtilities.ValidateNotNull(item, "item");
				this.Item = item;
			} 
			#endregion // Constructor

			#region IComparable<Group> Members

			int IComparable<Group>.CompareTo(Group other)
			{
				if (other == null)
					return 1;

				int result = this.Start.CompareTo(other.Start);

				if (result == 0)
				{
					result = other.End.CompareTo(this.End);
				}

				return result;
			}

			#endregion
		} 
		#endregion // Group class

		#region ITimeslotGroupProvider interface
		internal interface ITimeslotGroupProvider
		{
			void InitializeGroups(GroupMeasureInfo measureInfo);
		} 
		#endregion // ITimeslotGroupProvider interface
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