//#define DEBUG_RESIZING





using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Collections;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;
using Infragistics.Shared;
using System.Windows.Threading;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A <see cref="Panel"/> derived element used to arrange <see cref="RibbonGroup"/> instances within a <see cref="RibbonTabItem"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">The RibbonGroupPanel</p> is used by template for the <see cref="RibbonGroupCollection"/> and is responsible for 
	/// positioning the <see cref="RibbonGroup"/> instances as well as drive the resize logic using the <see cref="RibbonGroup.Variants"/>.
	/// </remarks>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RibbonGroupPanel : Panel, IScrollInfo
	{
		#region Member Variables

		private const double InterGroupSpacing = 0;
		private VariantManager _variantManager;
		private List<double> _groupExtents = new List<double>();
		private bool _isInMeasure = false;

		// AS 10/1/09 TFS20404
		private Dictionary<RibbonGroup, RibbonTabItem> _groupsToRestore = new Dictionary<RibbonGroup, RibbonTabItem>();

		// AS 10/8/09 TFS23328
		private Dictionary<RibbonGroup, TempValueReplacement> _groupValues = new Dictionary<RibbonGroup, TempValueReplacement>();

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RibbonGroupPanel"/>
		/// </summary>
		public RibbonGroupPanel()
		{
			this._variantManager = new VariantManager(this);

			// AS 10/1/09 TFS20404
			this.Loaded += new RoutedEventHandler(OnLoaded);
		}

		static RibbonGroupPanel()
		{
			// AS 10/9/07
			// Allow focus to leave the ribbon group area.
			//
			//KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(RibbonGroupPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
			XamRibbon.RibbonProperty.OverrideMetadata(typeof(RibbonGroupPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRibbonChanged)));
		}

		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			this.ScrollDataInfo.VerifyScrollData(finalSize, this.ScrollDataInfo._extent);

			Rect finalRect = new Rect(finalSize);
			UIElementCollection children = this.InternalChildren;

			finalRect.Location = new Point(-this.ScrollDataInfo._offset.X, -this.ScrollDataInfo._offset.Y);

			for (int i = 0, count = children.Count; i < count; i++)
			{
				UIElement child = children[i];

				if (null != child)
				{
					finalRect.Width = this._groupExtents[i];

					child.Arrange(finalRect);

					// offset for the next group
					finalRect.X += finalRect.Width + InterGroupSpacing;
				}
			}

			return finalSize;
		}
		#endregion //ArrangeOverride

		#region HasLogicalOrientation
		/// <summary>
		/// Indicates if the panel arranges its children in a single dimension.
		/// </summary>
		/// <value>This property always returns true.</value>
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
		/// The <see cref="Orientation"/> of the panel.
		/// </summary>
		/// <value>This property always returns <b>Horizontal</b>.</value>
		protected override Orientation LogicalOrientation
		{
			get
			{
				return Orientation.Horizontal;
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
			bool wasInMeasure = this._isInMeasure;

			try
			{
				this._isInMeasure = true;
				return MeasureOverrideImpl(availableSize);
			}
			finally
			{
				this._isInMeasure = wasInMeasure;
			}
		}

		private Size MeasureOverrideImpl(Size availableSize)
		{
			this.OutputResizeMessage("Start Measure:" + availableSize.ToString());

			#region Setup

			Size availableChildSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

			UIElementCollection childCollection = this.InternalChildren;
			UIElement[] children = new UIElement[childCollection.Count];
			childCollection.CopyTo(children, 0);

			// if the variant information has changed then we need to reset
			// all the group variants to large and restart the sizing operation
			bool reset = this._variantManager.IsVariantInfoDirty ||
				double.IsPositiveInfinity(availableSize.Width);

			#endregion //Setup			

			#region Reset VariantSize
			if (reset)
			{
				this.OutputResizeMessage("Resetting VariantInfo");
				this._variantManager.ResetVariantInfo();
			} 
			#endregion //Reset VariantSize

			int oldRibbonGroupSizeVersion = (int)this.GetValue(RibbonGroupSizeVersionProperty);

			// measure and see if everything fits
			Size desiredSizeMax, desiredSize;
			this.MeasureChildren(children, availableChildSize, out desiredSizeMax, out desiredSize);

			int newRibbonGroupSizeVersion = (int)this.GetValue(RibbonGroupSizeVersionProperty);

			// if the ribbon group info changed during the initial measure then something
			// must have been registered so reset the variant info and measure again
			if (newRibbonGroupSizeVersion != oldRibbonGroupSizeVersion)
			{
				reset = true;
				this.OutputResizeMessage("Resetting VariantInfo");
				this._variantManager.ResetVariantInfo();

				this.MeasureChildren(children, availableChildSize, out desiredSizeMax, out desiredSize);
			}

			if (reset)
			{
				this.OutputResizeMessage("Unresized Size:" + desiredSize.ToString());

				// if everything is at its default size then store this value
				// so we know how big the groups will be when moving from
				// the first scaled down size to the full size
				this._variantManager.CacheTotalExtent(desiredSize.Width);
			}

			#region Perform Resize
			// we need a valid extent to perform a resize operation
			// AS 10/24/07 IsRibbonGroupResizingEnabled
			//if (double.IsInfinity(availableSize.Width) == false)
			if (double.IsInfinity(availableSize.Width) == false && true.Equals(this.GetValue(IsRibbonGroupResizingEnabledProperty)))
			{
				// see how much is left to allocate
				double remaining = availableSize.Width - desiredSize.Width;
				bool isMeasureNeeded = false;

				if (remaining < 0)
				{
					this.OutputResizeMessage("Reducing Size - Remaining=" + remaining.ToString());

					while (remaining < 0)
					{
						bool isNewOperation;
						bool resized = this._variantManager.ProcessVariantChange(true, out isNewOperation);

						// if there are no more variants to process then exit
						if (resized == false)
							break;

						double newRemaining;

						if (isNewOperation)
						{
							// otherwise remeasure and see if there is enough room
							this.MeasureChildren(children, availableChildSize, out desiredSizeMax, out desiredSize);

							newRemaining = availableSize.Width - desiredSize.Width;
							isMeasureNeeded = false;
						}
						else
						{
							newRemaining = availableSize.Width - this._variantManager.CurrentTotalExtent;
							isMeasureNeeded = true;
						}

						// check the remaining after a process and if it causes
						// there to be less remaining space (or was a no-op) then undo 
						// it and mark it as invalid and continue processing variants
						if (newRemaining <= remaining)
						{
							if (isNewOperation)
							{
								Debug.Assert(isNewOperation);

								this._variantManager.DisableCurrentVariant();

								isMeasureNeeded = true;
							}
						}
						else
						{
							// store the resulting value so we can peek ahead
							// when increasing the size
							if (isNewOperation)
								this._variantManager.CacheTotalExtent(desiredSize.Width);

							// update the new remaining
							remaining = newRemaining;
						}
					}
				}
				else if (remaining > 0)
				{
					this.OutputResizeMessage("Increasing Size - Remaining=" + remaining.ToString());

					while (this._variantManager.CanIncreaseInSize)
					{
						// if we cannot resize larger then exit
						if (this._variantManager.ExtentIfIncreased - availableSize.Width >= 0)
							break;

						this.OutputResizeMessage("Processing Undo - ExtentIfIncreased=" + this._variantManager.ExtentIfIncreased);

						bool isNewOperation;
						bool changed = this._variantManager.ProcessVariantChange(false, out isNewOperation);

						isMeasureNeeded |= changed;
					}
				}

				// if there is extra space then see if we should restore
				// one or more of the ribbon group variants
				if (isMeasureNeeded)
					this.MeasureChildren(children, availableChildSize, out desiredSizeMax, out desiredSize);
			} 
			#endregion //Perform Resize

			// cache the extents of each group for us in the arrange. we cannot
			// use the desired sze there because when we are adjusting for a 
			// clipped caption that could result in inconsistent extra space
			// on the right edge - i.e. the sum of the desiredsize widths may
			// not equal the arrange size
			this._groupExtents.Clear();

			for (int i = 0; i < children.Length; i++)
				this._groupExtents.Add(children[i].DesiredSize.Width);

			#region With Caption > Without
			// if the size with the caption is larger than the size without
			// then use that as our desired size
			if (desiredSizeMax.Width > desiredSize.Width)
			{
				// if we are not given a constraining width or if there are cases where the 
				// caption is wider than the tools but we have more than enough space we 
				// shouldn't have to do anything except use the full size as our desired
				if (double.IsInfinity(availableSize.Width) || availableSize.Width >= desiredSizeMax.Width)
					desiredSize.Width = desiredSizeMax.Width;
				else if (desiredSize.Width < availableSize.Width)
				{
					// if there is extra space but less than what we need to show all the 
					// captions of all the groups then we need to increase the size of the 
					// groups whose caption is larger than the area available for the tools

					// increase the desired size up to the maximum of what is available.
					double diff = desiredSizeMax.Width - availableSize.Width;
					desiredSize.Width = availableSize.Width;

					this.AdjustGroupsForCaption(diff, children);
				}
				else
				{
					// the smallest size is larger than what is needed for
					// the groups captions so remeasure them all with the non-
					// caption width
					for (int i = 0; i < children.Length; i++)
					{
						UIElement child = children[i];
						object captionWidth = child != null ? child.ReadLocalValue(RibbonGroupPanel.ExtraCaptionExtentProperty) : null;

						if (captionWidth is double)
						{
							Size childSize = new Size(child.DesiredSize.Width - (double)captionWidth, availableChildSize.Height);
							child.Measure(childSize);
							this._groupExtents[i] = childSize.Width;
						}
					}
				}
			} 
			#endregion //With Caption > Without

			// verify the scroll extent
			this.ScrollDataInfo.VerifyScrollData(availableSize, desiredSize);

			// if we need more room than we are offered, just indicate that we
			// need the space returned since we will be scrolling the rest
			if (availableSize.Width < desiredSize.Width)
				desiredSize.Width = availableSize.Width;
			if (availableSize.Height < desiredSize.Height)
				desiredSize.Height = availableSize.Height;

			this.OutputResizeMessage("End Measure - DesiredSize=" + desiredSize.ToString());

			return desiredSize;
		}
		#endregion //MeasureOverride

		#region OnChildDesiredSizeChanged
		/// <summary>
		/// Overriden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
		/// </summary>
		/// <param name="child">The child element whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			// there are cases when after a resize operation, we get a desired size change
			// for the child because their height has changed. rather than always regenerating
			// the layout, we'll only do that if we haven't loaded or the width has changed
			// from what we want to arrange the group at
			bool invalidateVariantInfo = this.IsLoaded == false || child.GetValue(FrameworkElement.TagProperty) == XamRibbon.RibbonGroupForResizingTag;

			// if this is not the ribbon group we use for calculating the sizing then see if its width has changed
			if (invalidateVariantInfo == false)
			{
				int index = this.Children.IndexOf(child);
				Debug.Assert(index >= 0 && index < this.Children.Count);
				// AS 7/1/10 TFS34815
				// This could get invoked before the measure which is where the _groupExtents is 
				// synchronized with the actual children and their respective desiredsize values.
				//
				//invalidateVariantInfo = index < 0 || index >= _groupExtents.LastIndexOf child.DesiredSize.Width != this._groupExtents[index];
				invalidateVariantInfo = index < 0 || index >= _groupExtents.Count || child.DesiredSize.Width != this._groupExtents[index];
			}

			if (invalidateVariantInfo)
			{
				this.OutputResizeMessage(string.Format("Child Desired Size Changed - Child={0}, DesiredSize={1}", child.GetHashCode(), child.DesiredSize));
				this._variantManager.InvalidateVariantInfo();
			}

			base.OnChildDesiredSizeChanged(child);
		} 
		#endregion //OnChildDesiredSizeChanged

		#region OnVisualChildrenChanged
		/// <summary>
		/// Invoked when a child element has been added or removed.
		/// </summary>
		/// <param name="visualAdded">Visual being added</param>
		/// <param name="visualRemoved">Visual being removed</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);

			RibbonGroup groupAdded = visualAdded as RibbonGroup;
			RibbonGroup groupRemoved = visualRemoved as RibbonGroup;

			if (null != groupAdded)
			{
				this.OutputResizeMessage("Group Added - Group=" + groupAdded.GetHashCode() + " : " + groupAdded.Caption);
				this._variantManager.AddGroup(groupAdded);

				// AS 10/1/09 TFS20404
				// If a loaded group is added to the panel when the panel is not loaded then 
				// we need to temporarily pull the group out of its logical tree so that 
				// when the loaded event is broadcast for the panel the group remains loaded.
				// Otherwise some bug/behavior in the framework is causing the group to be 
				// unloaded and not reloaded when the panel is reloaded.
				//
				if (groupAdded.IsLoaded && !this.IsLoaded)
				{
					RibbonTabItem tab = groupAdded.Parent as RibbonTabItem;

					if (null != tab)
					{
						// AS 10/8/09 TFS23328
						// We need to store the current data context value.
						//
						_groupValues[groupAdded] = new TempValueReplacement(groupAdded, FrameworkElement.DataContextProperty);

						tab.RemoveLogicalChildHelper(groupAdded);
						_groupsToRestore[groupAdded] = tab;
					}
				}
			}

			if (null != groupRemoved)
			{
				this.OutputResizeMessage("Group Removed - Group=" + groupRemoved.GetHashCode() + " : " + groupRemoved.Caption);
				this._variantManager.RemoveGroup(groupRemoved);

				// AS 10/1/09 TFS20404
				// If the group should be removed before we get our loaded we will 
				// re-add the group to the tab as its logical child.
				//
				RibbonTabItem tab;

				if (_groupsToRestore.TryGetValue(groupRemoved, out tab))
				{
					_groupsToRestore.Remove(groupRemoved);

					if (groupRemoved.Parent == null)
						tab.AddLogicalChildHelper(groupRemoved);

					// AS 10/8/09 TFS23328
					// If we stored a temporary value for the datacontext we can clean it up.
					//
					TempValueReplacement replacement;

					if (_groupValues.TryGetValue(groupRemoved, out replacement))
					{
						_groupValues.Remove(groupRemoved);
						replacement.Dispose();
					}
				}
			}
		}
		#endregion //OnVisualChildrenChanged

		#endregion //Base class overrides

		#region Properties

		#region ExtraCaptionExtent

		private static readonly DependencyPropertyKey ExtraCaptionExtentPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ExtraCaptionExtent",
			typeof(double), typeof(RibbonGroupPanel), new FrameworkPropertyMetadata(0d));

		/// <summary>
		/// Identifies the ExtraCaptionExtent" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetExtraCaptionExtent"/>
		internal static readonly DependencyProperty ExtraCaptionExtentProperty =
			ExtraCaptionExtentPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the value of the 'ExtraCaptionExtent' attached readonly property
		/// </summary>
		/// <seealso cref="ExtraCaptionExtentProperty"/>
		internal static double GetExtraCaptionExtent(DependencyObject d)
		{
			return (double)d.GetValue(RibbonGroupPanel.ExtraCaptionExtentProperty);
		}

		#endregion //ExtraCaptionExtent

		#region IsInMeasure
		internal bool IsInMeasure
		{
			get { return this._isInMeasure; }
		} 
		#endregion //IsInMeasure

		// AS 10/24/07 IsRibbonGroupResizingEnabled
		#region IsRibbonGroupResizingEnabled
		internal static readonly DependencyProperty IsRibbonGroupResizingEnabledProperty = XamRibbon.IsRibbonGroupResizingEnabledProperty.AddOwner(typeof(RibbonGroupPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsRibbonGroupResizingEnabledChanged)));

		private static void OnIsRibbonGroupResizingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroupPanel panel = d as RibbonGroupPanel;

			if (null != panel)
			{
				if (panel._isInMeasure == false)
					panel._variantManager.InvalidateVariantInfo();
			}
		} 
		#endregion //IsRibbonGroupResizingEnabled

		#region RibbonGroupSizeVersion

		internal static readonly DependencyProperty RibbonGroupSizeVersionProperty = XamRibbon.RibbonGroupSizeVersionProperty.AddOwner(typeof(RibbonGroupPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRibbonGroupSizeVersionChanged)));

		private static void OnRibbonGroupSizeVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroupPanel panel = d as RibbonGroupPanel;

			if (null != panel)
			{
				if (panel._isInMeasure == false)
					panel._variantManager.InvalidateVariantInfo();
			}
		}

		#endregion //RibbonGroupSizeVersion

		#region SizingModeVersion

		/// <summary>
		/// Identifies the SizingModeVersion attached dependency property
		/// </summary>
		/// <remarks>
		/// <p class="body">This property is set on tools when their <see cref="RibbonToolHelper.SizingModeProperty"/> value is changed by the <see cref="RibbonGroupPanel"/>.</p>
		/// </remarks>
		/// <seealso cref="GetSizingModeVersion"/>
		/// <seealso cref="SetSizingModeVersion"/>
		public static readonly DependencyProperty SizingModeVersionProperty = DependencyProperty.RegisterAttached("SizingModeVersion",
			typeof(int), typeof(RibbonGroupPanel), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnSizingModeVersionChanged)));

		private static void OnSizingModeVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// don't bother walking up if we explicitly set the sizing mode
			if (d.ReadLocalValue(RibbonToolHelper.SizingModeProperty) != DependencyProperty.UnsetValue)
				return;

			do
			{
				UIElement element = d as UIElement;

				if (null != element)
					element.InvalidateMeasure();

				d = VisualTreeHelper.GetParent(d);

			} while (d != null && d is RibbonGroupPanel == false && false == object.ReferenceEquals(e.NewValue, d.GetValue(SizingModeVersionProperty)));
		}

		/// <summary>
		/// Gets the value of the 'SizingModeVersion' attached property
		/// </summary>
		/// <seealso cref="SizingModeVersionProperty"/>
		/// <seealso cref="SetSizingModeVersion"/>
		public static int GetSizingModeVersion(DependencyObject d)
		{
			return (int)d.GetValue(RibbonGroupPanel.SizingModeVersionProperty);
		}

		/// <summary>
		/// Sets the value of the 'SizingModeVersion' attached property
		/// </summary>
		/// <seealso cref="SizingModeVersionProperty"/>
		/// <seealso cref="GetSizingModeVersion"/>
		public static void SetSizingModeVersion(DependencyObject d, int value)
		{
			d.SetValue(RibbonGroupPanel.SizingModeVersionProperty, value);
		}

		#endregion //SizingModeVersion

		#region ScrollDataInfo

		private ScrollData _scrollDataInfo;

		private ScrollData ScrollDataInfo
		{
			get
			{
				if (this._scrollDataInfo == null)
					this._scrollDataInfo = new ScrollData();

				return this._scrollDataInfo;
			}
		}
		#endregion //ScrollDataInfo

		#endregion //Properties

		#region Methods

		#region AdjustGroupsForCaption
		private void AdjustGroupsForCaption(double excess, UIElement[] children)
		{
			Debug.Assert(excess > 0, "We should only get here where we have some space above the tool only size of the group and want to distribute the rest to the groups with larger captions.");

			#region Setup
			// first get a list of the children who have extra caption space
			List<GroupIndex> groups = new List<GroupIndex>();

			for (int i = 0; i < children.Length; i++)
			{
				RibbonGroup group = children[i] as RibbonGroup;

				if (group != null)
				{
					double extra = RibbonGroupPanel.GetExtraCaptionExtent(group);

					if (0 != extra)
						groups.Add(new GroupIndex(group, i, extra));
				}
			}
			#endregion //Setup

			#region Sort
			Debug.Assert(groups.Count > 0, "We should not get here if there are no groups with extra captions!");

			// then sort it based on the tool-only size
			groups.Sort(new Comparison<GroupIndex>(delegate(GroupIndex a, GroupIndex b)
			{
				// sort by desired size with the largest being first
				int comparison = b.Group.DesiredSize.Width.CompareTo(a.Group.DesiredSize.Width);

				if (comparison == 0)
				{
					comparison = b.Index.CompareTo(a.Index);
				}

				return comparison;
			}));
			#endregion //Sort

			// then start reducing the size of the groups down to the next smallest group
			// until we've regained just enough space.
			int groupCount = groups.Count;
			int startIndex = 0;

			#region Allocate Excess

			// AS 12/4/09 TFS25337
			int previousStartIndex = -1;
			int previousStopIndex = -1;

			while (excess > 0)
			{
				#region Find startIndex

				// find the first allocatable group
				for (; startIndex < groupCount; startIndex++)
				{
					if (groups[startIndex].CanReduceSize)
						break;
				}

				#endregion //Find startIndex

				// get the width of that group since we will resize
				// all groups with that same width
				double groupWidth = groups[startIndex].NewDesiredWidth;
				int stopIndex = groupCount;
				double nextWidth = 0;
				int groupsToResize = 1;

				#region Find Groups To Resize
				// then find out how many groups we need to adjust 
				// in this turn - i.e. find all the groups with the 
				// same width so we can reduce them down to the next
				// width
				for (int i = startIndex + 1; i < groupCount; i++)
				{
					GroupIndex group = groups[i];

					// if the group is fully allocated then ignore it
					if (group.CanReduceSize)
					{
						// if the adjusted width for this group is different
						// AS 12/4/09 TFS25337
						// The difference could be negligible in which case we get stuff making 
						// lots and lots of little changes which because of rounding issues never 
						// adjusts the group enough to actually finish the changes.
						//
						//if (group.NewDesiredWidth != groupWidth)
						if (!RibbonGroupPanel.AreClose(group.NewDesiredWidth, groupWidth))
						{
							stopIndex = i;
							nextWidth = group.NewDesiredWidth;
							break;
						}

						groupsToResize++;
					}
				}
				#endregion //Find Groups To Resize

				// AS 12/4/09 TFS25337
				// As a safety catch, we'll get out if we're not procesing 
				// a different set of groups on this iteration.
				//
				if (startIndex == previousStartIndex &&
					stopIndex == previousStopIndex)
				{
					Debug.Fail("Attempting to process the same groups again.");
					break;
				}

				previousStartIndex = startIndex;
				previousStopIndex = stopIndex;

				// now find out the total possible allocation
				double totalPossibleToRemove = 0;
				double adjustmentToNextGroup = groupWidth - nextWidth;
				double minPossibleToRemove = double.MaxValue;

				#region Calculate Sizes
				for (int i = startIndex; i < stopIndex; i++)
				{
					GroupIndex group = groups[i];

					// if the group is fully allocated then ignore it
					if (group.CanReduceSize)
					{
						double possibleAllocation = Math.Min(adjustmentToNextGroup, group.AmountAvailableForReduce);

						// keep track of the smallest amount we can allocate to all of these groups
						if (minPossibleToRemove > possibleAllocation)
							minPossibleToRemove = possibleAllocation;

						totalPossibleToRemove += possibleAllocation;
					}
				}
				#endregion //Calculate Sizes

				// if we have more than enough than we can just allocate it all
				if (totalPossibleToRemove <= excess)
				{
					#region Enough room to allocate max for all groups

					for (int i = startIndex; i < stopIndex; i++)
					{
						GroupIndex group = groups[i];

						if (group.CanReduceSize)
							group.AmountToRemove += Math.Min(adjustmentToNextGroup, group.AmountAvailableForReduce);
					}

					excess -= totalPossibleToRemove;

					#endregion //Enough room to allocate max for all groups
				}
				else if (minPossibleToRemove * groupsToResize >= excess)
				{
					#region Enough room to give minimum to all groups

					// if every group can take its full minimum allocation
					double amountPerGroup = excess / groupsToResize;
					double extra = excess - (amountPerGroup * groupsToResize);

					for (int i = startIndex; i < stopIndex; i++)
					{
						GroupIndex group = groups[i];

						if (group.CanReduceSize)
						{
							double amountToRemove = amountPerGroup;
							if (extra > 0)
							{
								double actualExtra = Math.Min(1, extra);
								amountToRemove += actualExtra;
								extra -= actualExtra;
							}

							group.AmountToRemove += amountToRemove;
						}
					}

					excess = 0;

					#endregion //Enough room to give minimum to all groups
				}
				else
				{
					#region Iteratively allocate the remaining excess

					// allocate the minimum first
					double amountToRemove = minPossibleToRemove;

					// this will be the last round of allocations
					while (excess > 0)
					{
						for (int i = startIndex; i < stopIndex; i++)
						{
							GroupIndex group = groups[i];

							// if the group is fully allocated then ignore it
							if (group.CanReduceSize == false)
							{
								// if this is the first or last item then update
								// the index so we skip it on the next iteration
								if (i == startIndex)
									startIndex++;
								else if (i == stopIndex)
									stopIndex--;

								continue;
							}

							group.AmountToRemove += amountToRemove;

							// once we allocate a group remove it
							if (group.CanReduceSize == false)
								groupsToResize--;

							excess -= amountToRemove;
						}

						if (groupsToResize == 1)
							amountToRemove = excess;
						else
						{
							// then just allcate 1 pixel at a time
							amountToRemove = 1;
						}
					}
					#endregion //Iteratively allocate the remaining excess
				}
			}
			#endregion //Allocate Excess

			#region Remeasure resized groups
			// lastly, measure the groups that we have adjusted
			for (int i = 0; i < groupCount; i++)
			{
				GroupIndex group = groups[i];

				// we don't want to force a measure if we're not changing a group
				if (group.AmountToRemove != 0)
				{
					Size availableSize = group.Group.DesiredSize;
					availableSize.Width = group.NewDesiredWidth;
					this._groupExtents[group.Index] = group.NewDesiredWidth;
					group.Group.Measure(availableSize);
				}
			}
			#endregion //Remeasure resized groups
		}
		#endregion //AdjustGroupsForCaption

		#region ApplyTemplateRecursively
		internal static void ApplyTemplateRecursively(DependencyObject dependencyObject)
		{
			FrameworkElement fe = dependencyObject as FrameworkElement;

			if (null != fe)
			{
				fe.ApplyTemplate();
			}

			for (int i = 0, count = VisualTreeHelper.GetChildrenCount(dependencyObject); i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);

				if (null != child)
					ApplyTemplateRecursively(child);
			}
		}
		#endregion //ApplyTemplateRecursively

		#region AreClose
		internal static bool AreClose(double value1, double value2)
		{
			if (value1 == value2)
				return true;

			return Math.Abs(value1 - value2) < .0000000001;
		}
		#endregion //AreClose

		#region BumpSizingModeVersion
		internal static void BumpSizingModeVersion(DependencyObject d)
		{
			d.SetValue(RibbonGroupPanel.SizingModeVersionProperty, 1 + (int)d.GetValue(RibbonGroupPanel.SizingModeVersionProperty));
		} 
		#endregion //BumpSizingModeVersion

		#region EnsureIsANumber
		internal static void EnsureIsANumber(double number)
		{
			if (double.IsNaN(number))
				throw new ArgumentOutOfRangeException("number", number, XamRibbon.GetString("LE_DoubleNanNotAllowed"));
		}
		#endregion //EnsureIsANumber

		#region InvalidateMeasure


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static void InvalidateMeasure(UIElement descendant, UIElement ancestor)
		{
			while (true)
			{
				UIElement parent = VisualTreeHelper.GetParent(descendant) as UIElement;

				if (parent == null || parent == ancestor)
					break;

				parent.InvalidateMeasure();
				descendant = parent;
			}
		}
		#endregion //InvalidateMeasure

		#region MeasureChildren
		private void MeasureChildren(UIElement[] children, Size availableSize, out Size desiredSizeMax, out Size desiredSizeMin)
		{
			this.OutputResizeMessage("MeasureChildren [Start] - Available=" + availableSize.ToString());

			desiredSizeMax = new Size();
			desiredSizeMin = new Size();

			for (int i = 0; i < children.Length; i++)
			{
				UIElement child = children[i];

				if (null != child)
				{
					// measure the child with all the room available
					child.Measure(availableSize);

					Size childSize = child.DesiredSize;

					this.OutputResizeMessage(string.Format("MeasureChildren Group={0}, DesiredSize={1}", child.GetHashCode(), childSize));

					if (desiredSizeMax.Width > 0)
					{
						desiredSizeMax.Width += InterGroupSpacing;
						desiredSizeMin.Width += InterGroupSpacing;
					}

					// we need to measure the group without the caption width
					RibbonGroup group = child as RibbonGroup;

					FrameworkElement captionElement = group != null && group.IsCollapsed == false ? group.CaptionElement : null;
					Visibility captionVisibility = captionElement != null
						? group.CaptionElement.Visibility
						: Visibility.Collapsed;

					if (captionVisibility != Visibility.Collapsed)
					{
						// hide the caption
						object oldValue = captionElement.ReadLocalValue(UIElement.VisibilityProperty);
						captionElement.SetValue(UIElement.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);

						// we have to explicitly invalidate the ancestor chain or else
						// the measure will just return and we'll get the same desired size
						// as above
						RibbonGroupPanel.InvalidateMeasure(group.CaptionElement, this);

						// remeasure so we can track the delta
						child.Measure(availableSize);

						// update the minimumze size we'll return
						double widthNoCaption = child.DesiredSize.Width;
						desiredSizeMin.Width += widthNoCaption;

						this.OutputResizeMessage(string.Format("MeasureChildren Group={0}, WidthWithoutCaption={1}", group.GetHashCode(), widthNoCaption));

						double diff = childSize.Width - widthNoCaption;

						if (diff == 0)
							child.ClearValue(ExtraCaptionExtentPropertyKey);
						else
							child.SetValue(ExtraCaptionExtentPropertyKey, diff);

						// restore the visibility and remeasure to make sure the measure is valid.
						RibbonGroupPanel.InvalidateMeasure(group.CaptionElement, this);
						if (oldValue is Visibility)
							captionElement.SetValue(UIElement.VisibilityProperty, oldValue);
						else if (oldValue is BindingBase)
							captionElement.SetBinding(UIElement.VisibilityProperty, (BindingBase)oldValue);
						else
							captionElement.ClearValue(UIElement.VisibilityProperty);

						// re-measure with the original size
						child.Measure(availableSize);
					}
					else
					{
						// use the max size and clear any cached extra extent
						desiredSizeMin.Width += childSize.Width;
						child.ClearValue(ExtraCaptionExtentPropertyKey);
					}

					desiredSizeMax.Width += childSize.Width;
					desiredSizeMax.Height = Math.Max(desiredSizeMax.Height, childSize.Height);
					desiredSizeMin.Height = desiredSizeMax.Height;
				}
			}

			this.OutputResizeMessage(string.Format("MeasureChildren [End] - Min={0}, Max={1}", desiredSizeMin, desiredSizeMax));
		}
		#endregion //MeasureChildren

		// AS 10/1/09 TFS20404
		#region OnLoaded
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (_groupsToRestore.Count > 0)
			{
				Dictionary<RibbonGroup, RibbonTabItem> groups = _groupsToRestore;
				_groupsToRestore = new Dictionary<RibbonGroup, RibbonTabItem>();

				// AS 10/8/09 TFS23328
				Debug.Assert(groups.Count == _groupValues.Count);
				Dictionary<RibbonGroup, TempValueReplacement> replacements = _groupValues;
				_groupValues = new Dictionary<RibbonGroup, TempValueReplacement>();

				foreach (KeyValuePair<RibbonGroup, RibbonTabItem> pair in groups)
				{
					if (pair.Key.Parent == null)
						pair.Value.AddLogicalChildHelper(pair.Key);

					// AS 10/8/09 TFS23328
					TempValueReplacement replacement;

					if (replacements.TryGetValue(pair.Key, out replacement))
						replacement.Dispose();
				}
			}
		}
		#endregion //OnLoaded

		#region OnRibbonChanged
		private static void OnRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroupPanel panel = d as RibbonGroupPanel;

			if (null != panel && e.NewValue is XamRibbon)
			{
				panel.SetBinding(RibbonGroupSizeVersionProperty, Utilities.CreateBindingObject(XamRibbon.RibbonGroupSizeVersionProperty, BindingMode.OneWay, e.NewValue));
				// AS 10/24/07 IsRibbonGroupResizingEnabled
				panel.SetBinding(IsRibbonGroupResizingEnabledProperty, Utilities.CreateBindingObject(XamRibbon.IsRibbonGroupResizingEnabledProperty, BindingMode.OneWay, e.NewValue));
				return;
			}

			panel.ClearValue(RibbonGroupPanel.RibbonGroupSizeVersionProperty);
			// AS 10/24/07 IsRibbonGroupResizingEnabled
			panel.ClearValue(RibbonGroupPanel.IsRibbonGroupResizingEnabledProperty);
		}
		#endregion //OnRibbonChanged

		#region OutputResizeMessage
		[Conditional("DEBUG_RESIZING")]
		internal void OutputResizeMessage(string message)
		{
			Debug.WriteLine(message, string.Format("{0} - {1}", this.GetHashCode(), DateTime.Now.ToString("hh:mm:ss:ffffff")));
		} 
		#endregion //OutputResizeMessage

		#region PrepareCollapsedRibbonGroup
		internal void PrepareCollapsedRibbonGroup(RibbonGroup group)
		{
			this._variantManager.PrepareCollapsedRibbonGroup(group);
		} 
		#endregion //PrepareCollapsedRibbonGroup

		#endregion //Methods

		#region ScrollData class
		internal class ScrollData
		{
			#region Member Variables

			internal ScrollViewer _scrollOwner = null;
			internal Size _extent = new Size();
			internal Size _viewport = new Size();
			internal Vector _offset = new Vector();
			internal bool _canHorizontallyScroll = false;
			internal bool _canVerticallyScroll = false;

			#endregion //Member Variables

			#region Methods

			#region Reset

			internal void Reset()
			{
				this._offset = new Vector();
				this._extent = new Size();
				this._viewport = new Size();
			}

			#endregion //Reset

			#region VerifyScrollData
			internal void VerifyScrollData(Size viewPort, Size extent)
			{
				// if we have endless space use the space we need
				if (double.IsInfinity(viewPort.Width))
					viewPort.Width = extent.Width;
				if (double.IsInfinity(viewPort.Height))
					viewPort.Height = extent.Height;

				bool isDifferent = false == RibbonGroupPanel.AreClose(this._viewport.Width, viewPort.Width) ||
					false == RibbonGroupPanel.AreClose(this._viewport.Height, viewPort.Height) ||
					false == RibbonGroupPanel.AreClose(this._extent.Width, extent.Width) ||
					false == RibbonGroupPanel.AreClose(this._extent.Width, extent.Width);

				this._viewport = viewPort;
				this._extent = extent;

				isDifferent |= this.VerifyOffset();

				// dirty the scroll viewer if something has changed
				if (null != this._scrollOwner && isDifferent)
				{
					this._scrollOwner.InvalidateScrollInfo();
				}
			}
			#endregion //VerifyScrollData

			#region VerifyOffset
			private bool VerifyOffset()
			{
				double offsetX = Math.Max(Math.Min(this._offset.X, this._extent.Width - this._viewport.Width), 0);
				double offsetY = Math.Max(Math.Min(this._offset.Y, this._extent.Height - this._viewport.Height), 0);
				Vector oldOffset = this._offset;
				this._offset = new Vector(offsetX, offsetY);

				// return true if the offset has changed
				return false == RibbonGroupPanel.AreClose(this._offset.X, oldOffset.X) ||
					false == RibbonGroupPanel.AreClose(this._offset.Y, oldOffset.Y);
			}
			#endregion //VerifyOffset

			#endregion //Methods
		}
		#endregion //ScrollData class

		#region IScrollInfo Members

		const double LineOffset = 16;

		private void AdjustVerticalOffset(double adjustment)
		{
			((IScrollInfo)this).SetVerticalOffset(adjustment + ((IScrollInfo)this).VerticalOffset);
		}

		private void AdjustHorizontalOffset(double adjustment)
		{
			((IScrollInfo)this).SetHorizontalOffset(adjustment + ((IScrollInfo)this).HorizontalOffset);
		}

		bool IScrollInfo.CanHorizontallyScroll
		{
			get
			{
				return this.ScrollDataInfo._canHorizontallyScroll;
			}
			set
			{
				this.ScrollDataInfo._canHorizontallyScroll = value;
			}
		}

		bool IScrollInfo.CanVerticallyScroll
		{
			get
			{
				return this.ScrollDataInfo._canVerticallyScroll;
			}
			set
			{
				this.ScrollDataInfo._canVerticallyScroll = value;
			}
		}

		double IScrollInfo.ExtentHeight
		{
			get { return this.ScrollDataInfo._extent.Height; }
		}

		double IScrollInfo.ExtentWidth
		{
			get { return this.ScrollDataInfo._extent.Width; }
		}

		double IScrollInfo.HorizontalOffset
		{
			get { return this.ScrollDataInfo._offset.X; }
		}

		void IScrollInfo.LineDown()
		{
			this.AdjustVerticalOffset(LineOffset);
		}

		void IScrollInfo.LineLeft()
		{
			this.AdjustHorizontalOffset(-LineOffset);
		}

		void IScrollInfo.LineRight()
		{
			this.AdjustHorizontalOffset(LineOffset);
		}

		void IScrollInfo.LineUp()
		{
			this.AdjustVerticalOffset(-LineOffset);
		}

		Rect IScrollInfo.MakeVisible(System.Windows.Media.Visual visual, Rect rectangle)
		{
			if (rectangle.IsEmpty || visual == null || this.IsAncestorOf(visual) == false)
				return Rect.Empty;

			Rect visualRect = visual.TransformToAncestor(this).TransformBounds(rectangle);

			Rect availableRect = new Rect(this.RenderSize);
			Rect intersection = Rect.Intersect(visualRect, availableRect);

			if (intersection.Width != visualRect.Width)
			{
				double offsetX = 0;

				// try to get the right side in view
				if (visualRect.Right > availableRect.Right)
					offsetX = visualRect.Right - availableRect.Right;

				// make sure that the left side is in view
				if (visualRect.Left - offsetX - availableRect.Left < 0)
					offsetX += visualRect.Left - offsetX - availableRect.Left;

				visualRect.X -= offsetX;

				offsetX += ((IScrollInfo)this).HorizontalOffset;

				((IScrollInfo)this).SetHorizontalOffset(offsetX);
			}

			if (intersection.Height != visualRect.Height)
			{
				double offsetY = 0;

				// try to get the right side in view
				if (visualRect.Bottom > availableRect.Bottom)
					offsetY = visualRect.Bottom - availableRect.Bottom;

				// make sure that the left side is in view
				if (visualRect.Top - offsetY - availableRect.Top < 0)
					offsetY += visualRect.Top - offsetY - availableRect.Top;

				visualRect.Y -= offsetY;

				offsetY += ((IScrollInfo)this).VerticalOffset;

				((IScrollInfo)this).SetVerticalOffset(offsetY);
			}

			return visualRect;
		}

		void IScrollInfo.MouseWheelDown()
		{
			this.AdjustVerticalOffset(SystemParameters.WheelScrollLines * LineOffset);
		}

		void IScrollInfo.MouseWheelLeft()
		{
			this.AdjustHorizontalOffset(-SystemParameters.WheelScrollLines * LineOffset);
		}

		void IScrollInfo.MouseWheelRight()
		{
			this.AdjustHorizontalOffset(SystemParameters.WheelScrollLines * LineOffset);
		}

		void IScrollInfo.MouseWheelUp()
		{
			this.AdjustVerticalOffset(-SystemParameters.WheelScrollLines * LineOffset);
		}

		void IScrollInfo.PageDown()
		{
			this.AdjustVerticalOffset(-((IScrollInfo)this).ViewportHeight);
		}

		void IScrollInfo.PageLeft()
		{
			this.AdjustHorizontalOffset(-((IScrollInfo)this).ViewportWidth);
		}

		void IScrollInfo.PageRight()
		{
			this.AdjustHorizontalOffset(((IScrollInfo)this).ViewportWidth);
		}

		void IScrollInfo.PageUp()
		{
			this.AdjustVerticalOffset(((IScrollInfo)this).ViewportHeight);
		}

		ScrollViewer IScrollInfo.ScrollOwner
		{
			get
			{
				return this.ScrollDataInfo._scrollOwner;
			}
			set
			{
				this.ScrollDataInfo._scrollOwner = value;
			}
		}

		void IScrollInfo.SetHorizontalOffset(double offset)
		{
			EnsureIsANumber(offset);
			offset = Math.Max(offset, 0);

			if (false == AreClose(offset, this.ScrollDataInfo._offset.X))
			{
				this.ScrollDataInfo._offset.X = offset;
				this.InvalidateArrange();
			}
		}

		void IScrollInfo.SetVerticalOffset(double offset)
		{
			EnsureIsANumber(offset);
			offset = Math.Max(offset, 0);

			if (AreClose(offset, this.ScrollDataInfo._offset.Y) == false)
			{
				this.ScrollDataInfo._offset.Y = offset;
				this.InvalidateArrange();
			}
		}

		double IScrollInfo.VerticalOffset
		{
			get { return this.ScrollDataInfo._offset.Y; }
		}

		double IScrollInfo.ViewportHeight
		{
			get { return this.ScrollDataInfo._viewport.Height; }
		}

		double IScrollInfo.ViewportWidth
		{
			get { return this.ScrollDataInfo._viewport.Width; }
		}

		#endregion // IScrollInfo

		#region GroupIndex





		private class GroupIndex
		{
			internal RibbonGroup Group;
			internal int Index;
			internal double ExtraCaptionWidth;
			internal double AmountToRemove;

			internal GroupIndex(RibbonGroup group, int index, double extraCaptionWidth)
			{
				this.Index = index;
				this.Group = group;
				this.ExtraCaptionWidth = extraCaptionWidth;
				this.AmountToRemove = 0;
			}

			internal double NewDesiredWidth
			{
				get { return this.Group.DesiredSize.Width - this.AmountToRemove; }
			}

			internal bool CanReduceSize
			{
				get { return this.AmountAvailableForReduce > 0; }
			}

			internal double AmountAvailableForReduce
			{
				get { return this.ExtraCaptionWidth - this.AmountToRemove; }
			}
		} 
		#endregion //GroupIndex
	}

	#region VariantManager
	internal class VariantManager : IComparer<RibbonGroupVariant>
	{
		#region Member Variables

		private NotifyCollectionChangedEventHandler _groupVariantsCollectionHandler;
		private EventHandler<ItemPropertyChangedEventArgs> _groupVariantsItemPropChangeHandler;

		private List<RibbonGroup> _groups = new List<RibbonGroup>();
		private int _lastVariantProcessed = -1;
		private RibbonGroupPanel _owner;
		private GroupVariantResizeHistory _resizeHistory;
		private Dictionary<RibbonGroup, List<GroupVariantAction>> _groupHistories;
		private List<GroupActionFactory> _variantActions;

		#endregion //Member Variables

		#region Constructor
		internal VariantManager(RibbonGroupPanel owner)
		{
			Debug.Assert(null != owner);
			this._owner = owner;
			this._groupVariantsCollectionHandler = new NotifyCollectionChangedEventHandler(OnGroupVariantsChanged);
			this._groupVariantsItemPropChangeHandler = new EventHandler<ItemPropertyChangedEventArgs>(OnGroupVariantsItemPropChanged);
			this._resizeHistory = new GroupVariantResizeHistory(owner);
		}
		#endregion //Constructor

		#region Properties

		#region CanIncreaseInSize
		internal bool CanIncreaseInSize
		{
			get { return this._resizeHistory.CanUndo(); }
		} 
		#endregion //CanIncreaseInSize

		#region CurrentTotalExtent
		internal double CurrentTotalExtent
		{
			get { return this._resizeHistory.CurrentExtent; }
		} 
		#endregion //CurrentTotalExtent

		#region ExtentIfIncreased
		internal double ExtentIfIncreased
		{
			get
			{
				return this._resizeHistory.NextLargerExtent;
			}
		} 
		#endregion //ExtentIfIncreased

		#region IsVariantInfoDirty
		internal bool IsVariantInfoDirty
		{
			get { return this._variantActions == null; }
		}
		#endregion //IsVariantInfoDirty 

		#endregion //Properties

		#region Methods

		#region AddGroup
		internal void AddGroup(RibbonGroup group)
		{
			if (null != group)
			{
				this._groups.Add(group);

				this.InvalidateVariantInfo();

				group.Variants.CollectionChanged += this._groupVariantsCollectionHandler;
				group.Variants.ItemPropertyChanged += this._groupVariantsItemPropChangeHandler;
			}
		}
		#endregion //AddGroup

		#region CacheTotalExtent
		internal void CacheTotalExtent(double totalExtent)
		{
			Debug.Assert(double.IsNaN(this._resizeHistory.CurrentExtent) || totalExtent < this._resizeHistory.CurrentExtent);

			double oldNextLargerExtent = this._resizeHistory.NextLargerExtent;

			// we're decreasing in size so the next larger size will be our current size
			this._resizeHistory.NextLargerExtent = this._resizeHistory.CurrentExtent;
			this._resizeHistory.CurrentExtent = totalExtent;

			GroupVariantAction currentAction = this._resizeHistory.UndoActionOnQueue;

			if (null != currentAction)
			{
				currentAction.ResultingExtent = this._resizeHistory.NextLargerExtent;
				currentAction.NextLargerExtent = oldNextLargerExtent;

				Debug.Assert(this._groupHistories[currentAction.Group].Contains(currentAction) == false);
				this._groupHistories[currentAction.Group].Add(currentAction);
			}
		} 
		#endregion //CacheTotalExtent

		#region DisableCurrentVariant
		internal void DisableCurrentVariant()
		{
			bool result = this._resizeHistory.Undo();
			this._resizeHistory.ClearRedo();

			if (this._lastVariantProcessed >= 0 && this._lastVariantProcessed < this._variantActions.Count)
				this._variantActions[this._lastVariantProcessed].InvalidateLastAction();

			Debug.Assert(result);
		}
		#endregion //DisableCurrentVariant

		#region InitializeVariantInfo
		private void InitializeVariantInfo()
		{
			// setup
			this._lastVariantProcessed = 0;
			this._variantActions = new List<GroupActionFactory>();
			List<RibbonGroupVariant> sortedVariants = new List<RibbonGroupVariant>();
			bool hasExplicitVariants = false;

			#region Build Variant List

			foreach (RibbonGroup group in this._groups)
			{
				if (group.Variants.Count > 0)
				{
					hasExplicitVariants = true;
					break;
				}
			}

			// if there are no explicit variants, add fake ones
			if (hasExplicitVariants == false)
			{
				#region Default Variants
				sortedVariants.Clear();

				Array sizes = Enum.GetValues(typeof(GroupVariantResizeAction));

				// iterate from largest size to smallest
				foreach (GroupVariantResizeAction size in sizes)
				{
					GroupVariant variant = GroupVariant.GetDefaultVariant(size);

					for (int i = this._groups.Count - 1; i >= 0; i--)
					{
						sortedVariants.Add(new RibbonGroupVariant(this._groups[i], variant, i));
					}
				}
				#endregion //Default Variants
			}
			else
			{
				#region Explicit Variants Only
				// otherwise add the explicit ones and sort them
				for (int i = 0, count = this._groups.Count; i < count; i++)
				{
					RibbonGroup group = this._groups[i];

					foreach (GroupVariant variant in group.Variants)
					{
						sortedVariants.Add(new RibbonGroupVariant(group, variant, i));
					}
				}

				// then sort the variants
				sortedVariants.Sort(this);

				// remove any variant actions for a group after it has been collapsed
				Dictionary<RibbonGroup, bool> _groups = new Dictionary<RibbonGroup, bool>();

				for (int i = 0, count = sortedVariants.Count; i < count; i++)
				{
					RibbonGroupVariant variant = sortedVariants[i];

					bool isCollapsed;

					if (_groups.TryGetValue(variant.Group, out isCollapsed))
					{
						if (isCollapsed)
						{
							sortedVariants.RemoveAt(i);
							i--;
							count--;
						}
					}
					else if (variant.Variant.ResizeAction == GroupVariantResizeAction.CollapseRibbonGroup)
					{
						_groups.Add(variant.Group, true);
					}
				}

				#endregion //Explicit Variants Only
			} 
			#endregion //Build Variant List

			// now we can populate the list of GroupActionFactories. we need these because
			// we want to be able to iterate over consecutive actions that perform the same
			// variant resize action
			#region Create the ActionFactories
			// start with an invalid last variant size
			GroupVariantResizeAction lastVariantSize = (GroupVariantResizeAction)(-1);

			foreach (RibbonGroupVariant variant in sortedVariants)
			{
				if (variant.Variant.ResizeAction == lastVariantSize)
				{
					// if this variant is the same as the last variant size then just add
					// this group to the last created action factory
					this._variantActions[this._variantActions.Count - 1].AddGroup(variant.Group);
				}
				else
				{
					// keep track of the last variant size
					lastVariantSize = variant.Variant.ResizeAction;

					this._variantActions.Add(new GroupActionFactory(lastVariantSize, variant.Group, this._resizeHistory));

				}
			} 
			#endregion //Create the ActionFactories

			#region Initialize Group Histories

			// lastly, we're going to track the actions for a group so we can undo them
			// when a group is collapsed so initialize that info here
			this._groupHistories = new Dictionary<RibbonGroup, List<GroupVariantAction>>();

			foreach (RibbonGroup group in this._groups)
				this._groupHistories.Add(group, new List<GroupVariantAction>()); 

			#endregion //Initialize Group Histories
		}
		#endregion //InitializeVariantInfo

		#region InvalidateVariantInfo
		internal void InvalidateVariantInfo()
		{
			this._variantActions = null;
			this._lastVariantProcessed = -1;
			this._resizeHistory.Clear();
			this._resizeHistory.NextLargerExtent = double.NaN;
			this._resizeHistory.CurrentExtent = double.NaN;

			this._owner.InvalidateMeasure();
			this._owner.OutputResizeMessage("Invalidating Variant Info");
		} 
		#endregion //InvalidateVariantInfo

		#region OnGroupVariantsChanged
		private void OnGroupVariantsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.InvalidateVariantInfo();
		}
		#endregion //OnGroupVariantsChanged

		#region OnGroupVariantsItemPropChanged
		private void OnGroupVariantsItemPropChanged(object sender, ItemPropertyChangedEventArgs e)
		{
			this.InvalidateVariantInfo();
		}
		#endregion //OnGroupVariantsItemPropChanged

		#region PrepareCollapsedRibbonGroup
		internal void PrepareCollapsedRibbonGroup(RibbonGroup group)
		{
			List<GroupVariantAction> actions;
			if (null != this._groupHistories && this._groupHistories.TryGetValue(group, out actions))
			{
				bool isDropDown = group.IsDropDown;
				int first = isDropDown ? actions.Count - 1 : 0;
				int end = isDropDown ? -1 : actions.Count;
				int increment = isDropDown ? -1 : 1;

				for (int i = first; i != end; i += increment)
				{
					GroupVariantAction action = actions[i];

					// we do not want to change the collapsed state and we do not
					// want to change the number of gallery preview columns - when dropped down
					// we should display the minimum # of columns
					if (action is GalleryPreviewColumnAction == false &&
						action is CollapseGroupAction == false)
					{
						action.ReplayGroupAction(isDropDown == false);
					}
				}

				// AS 11/26/07 BR28607
				// The measure of the contentpresenter is being invalidated but not everything up
				// its parent chain so I had to bind the RibbonGroupPanel.SizingModeVersion of the content
				// presenter to that of its content control and then bind the RibbonGroupPanel.SizingModeVersion 
				// of the contentcontrols in the ribbongroup's template to that of the ribbon group. Then we 
				// can bump the version number and invalidate the measure of everyone in the parent chain
				// of the contentpresenter so we get a valid value when we try to measure the group next.
				//
				RibbonGroupPanel.BumpSizingModeVersion(group);
			}
		} 
		#endregion //PrepareCollapsedRibbonGroup

		#region ProcessVariantChange
		internal bool ProcessVariantChange(bool decrease, out bool isNewOperation)
		{
			isNewOperation = false;

			// make sure the variant info is up to date
			this.VerifyVariantInfo();

			if (this._variantActions.Count > 0)
			{
				if (decrease)
				{
					#region Decrease

					// redo any size decreases
					if (this._resizeHistory.CanRedo())
					{
						this._owner.OutputResizeMessage("Performing Redo:" + this._resizeHistory.RedoActionOnQueue.ToString());

						if (this._resizeHistory.Redo())
							return true;
					}

					// process the next variant action or continue processing the 
					// last one
					while (this._lastVariantProcessed < this._variantActions.Count)
					{
						GroupVariantAction action = this._variantActions[this._lastVariantProcessed].CreateAction();

						if (action == null)
						{
							// if the action factory couldn't do any more with that variant size
							// then move on to the next action factory
							this._lastVariantProcessed++;
						}
						else
						{
							this._owner.OutputResizeMessage("Performing New Resize Operation:" + action.ToString());

							// otherwise perform the action
							this._resizeHistory.PerformAction(action, null);
							isNewOperation = true;
							return true;
						}
					}

					#endregion //Decrease
				}
				else
				{
					// we're increasing so just undo a resize action if we have one
					if (this._resizeHistory.CanUndo())
					{
						this._owner.OutputResizeMessage("Performing Undo:" + this._resizeHistory.UndoActionOnQueue.ToString());

						return this._resizeHistory.Undo();
					}
				}
			}

			// nothing to change
			return false;
		} 
		#endregion //ProcessVariantChange

		#region ResetVariantInfo
		internal void ResetVariantInfo()
		{
			// make sure our state is dirty so variants will be recreated if needed
			this.InvalidateVariantInfo();

			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			foreach (RibbonGroup group in this._groups)
			{
				ResetVariantInfo(group, false, this._owner);
			}

			this.VerifyVariantInfo();
		}

		internal static void ResetVariantInfo(RibbonGroup group, bool isOnQat, UIElement ancestor)
		{
			Debug.Assert(ancestor != null);

			group.ClearValue(RibbonGroup.IsCollapsedPropertyKey);

			// reset the state of all tools, panels, etc.
			foreach (IRibbonToolPanel ribbonPanel in group.RegisteredPanels)
			{
				Panel panel = ribbonPanel as Panel;

				if (null != panel)
				{
					// JJD 9/7/07
					// get the min/max size constraints of the panel
					//RibbonToolSizingMode panelMinSizingMode = RibbonGroup.GetMinimumSize(panel);
					RibbonToolSizingMode panelMaxSizingMode = RibbonGroup.GetMaximumSize(panel);

					foreach (UIElement child in panel.Children)
					{
						if (null != child)
						{
							RibbonToolSizingMode sizingMode = RibbonGroup.GetMaximumSize(child);

							// JJD 9/7/07
							// constrain the size of the tool based on the min/max of the panel
							if (sizingMode > panelMaxSizingMode)
								sizingMode = panelMaxSizingMode;

							// AS 9/7/07
							// do not use the min since the panle may want to be treated as large
							// but the children don't have to be
							//
							//if (sizingMode < panelMinSizingMode)
							//	sizingMode = panelMinSizingMode;

							if (RibbonToolHelper.GetSizingMode(child) != sizingMode)
							{
								child.SetValue(RibbonToolHelper.SizingModePropertyKey, RibbonKnownBoxes.FromValue(sizingMode));

								// AS 9/24/07
								// We need to bump the version number for the tool so it can be dirtied.
								//
								RibbonGroupPanel.BumpSizingModeVersion(child);

								RibbonGroupPanel.InvalidateMeasure(child, ancestor);
							}
						}
					}

					ToolHorizontalWrapPanel horzPanel = panel as ToolHorizontalWrapPanel;

					if (horzPanel != null)
					{
						horzPanel.SetValue(ToolHorizontalWrapPanel.RowCountPropertyKey, horzPanel.MinRows);

						// AS 2/13/08 BR30604
						// The group and group panel may not have had their measure invalidated if there
						// were no changes required to the child tools. However, the row count could be
						// changed in which case the parent chain needs to be invalidated.
						//
						RibbonGroupPanel.InvalidateMeasure(horzPanel, ancestor);
					}
				}
			}

			foreach (MenuTool menu in group.RegisteredMenus)
			{
				// when the tool is on the qat, its menu presenter may not have 
				// been visible so we would not have been able to get to the MenuToolPresenter
				if (menu.HasGalleryPreview == false && isOnQat)
					menu.ApplyTemplate();

				menu.ClearValue(MenuTool.HideGalleryPreviewProperty);

				// reset all preview info
				if (menu.HasGalleryPreview)
				{
					MenuToolPresenter mtp = menu.MenuToolPresenter;

					if (null != mtp)
					{
						if (isOnQat)
							mtp.ApplyTemplate();

						UIElement descendant = mtp;

						if (isOnQat == false)
							mtp.ClearValue(MenuToolPresenter.ResizedColumnCountProperty);
						else if (mtp.GalleryToolForPreview != null)
							mtp.SetValue(MenuToolPresenter.ResizedColumnCountProperty, mtp.GalleryToolForPreview.MinPreviewColumns);

						RibbonGroupPanel.InvalidateMeasure(mtp, ancestor);
					}
				}
			}
		}

		#endregion //ResetVariantInfo

		#region RemoveGroup
		internal void RemoveGroup(RibbonGroup group)
		{
			if (null != group)
			{
				group.Variants.CollectionChanged -= this._groupVariantsCollectionHandler;
				group.Variants.ItemPropertyChanged -= this._groupVariantsItemPropChangeHandler;

				this.InvalidateVariantInfo();

				this._groups.Remove(group);
			}
		}
		#endregion //RemoveGroup

		#region VerifyVariantInfo
		private void VerifyVariantInfo()
		{
			if (this.IsVariantInfoDirty)
				this.InitializeVariantInfo();
		} 
		#endregion //VerifyVariantInfo

		#endregion //Methods

		#region IComparer<RibbonGroupVariant> Members

		int IComparer<RibbonGroupVariant>.Compare(RibbonGroupVariant x, RibbonGroupVariant y)
		{
			if (x.Variant != y.Variant)
			{
				// sort first by priority
				if (x.Variant.Priority != y.Variant.Priority)
					return x.Variant.Priority.CompareTo(y.Variant.Priority);

				// then sort based on the size
				if (x.Variant.ResizeAction != y.Variant.ResizeAction)
					return x.Variant.ResizeAction.CompareTo(y.Variant.ResizeAction);
			}

			// sort by the 
			// AS 10/24/07
			// When the priority is the same then use the resize the right most
			// group first.
			//
			//return x.GroupIndex.CompareTo(y.GroupIndex);
			return y.GroupIndex.CompareTo(x.GroupIndex);
		}

		#endregion
	} 
	#endregion //VariantManager

	#region RibbonGroupVariant





	internal class RibbonGroupVariant
	{
		/// <summary>
		/// The group to be affected by the resize.
		/// </summary>
		internal RibbonGroup Group;

		/// <summary>
		/// The variant whose resize action will affect the ribbon group
		/// </summary>
		internal GroupVariant Variant;

		/// <summary>
		/// The index of the group in the groups collection.
		/// </summary>
		internal int GroupIndex;

		/// <summary>
		/// Maintains information about a ribbon group resize operation.
		/// </summary>
		/// <param name="group">The group that will be affected/resized</param>
		/// <param name="variant">The GroupVariant whose information will be used to perform the resize action.</param>
		/// <param name="groupIndex">The index of the group</param>
		internal RibbonGroupVariant(RibbonGroup group, GroupVariant variant, int groupIndex)
		{
			this.Group = group;
			this.Variant = variant;
			this.GroupIndex = groupIndex;
		}
	}
	#endregion //RibbonGroupVariant

	#region GroupActionFactory
	internal class GroupActionFactory
	{
		#region Member Variables

		private bool _initialized;
		private GroupVariantResizeAction _variantResizeAction;
		private List<RibbonGroup> _groups;
		private List<MenuTool> _menus;
		private List<IRibbonToolPanel> _panels;
		private int _currentGroupIndex;
		private int _currentMenuIndex;
		private int _currentPanelIndex;
		private bool _isComplete;
		private GroupVariantAction _lastAction;
		private int _horizontalRowCount = 2;
		private GroupVariantResizeHistory _history;

		#endregion //Member Variables

		#region Constructor
		internal GroupActionFactory(GroupVariantResizeAction variantResizeAction, RibbonGroup group, GroupVariantResizeHistory history)
		{
			this._variantResizeAction = variantResizeAction;
			this._history = history;
			this._groups = new List<RibbonGroup>();
			this._groups.Add(group);
		}
		#endregion //Constructor

		#region Properties

		#region VariantResizeAction
		/// <summary>
		/// Returns the variant size for which this resizer is operating.
		/// </summary>
		public GroupVariantResizeAction VariantResizeAction
		{
			get { return this._variantResizeAction; }
		}
		#endregion //VariantResizeAction

		#endregion //Properties

		#region Internal Methods

		#region AddGroup
		internal void AddGroup(RibbonGroup group)
		{
			Debug.Assert(this._initialized == false);

			// groups cannot be added one we have performed an action
			if (this._initialized)
				throw new InvalidOperationException();

			this._groups.Add(group);
		}
		#endregion //AddGroup

		#region CreateAction
		/// <summary>
		/// Performs a resize action and returns the 
		/// </summary>
		/// <returns>The next resize action to perform or null if there are no more actions to be performed for the variant</returns>
		internal GroupVariantAction CreateAction()
		{
			// do any set up
			if (this._initialized == false)
			{
				this._initialized = true;
				this.Initialize();
			}

			// get the action that should be performed
			GroupVariantAction action = null;

			if (this._isComplete == false)
			{
				switch (this.VariantResizeAction)
				{
					default:
					case GroupVariantResizeAction.CollapseRibbonGroup:
						{
							#region CollapseRibbonGroup
							RibbonGroup group = this.GetNextGroup();

							if (null != group)
								action = new CollapseGroupAction(group, true, this._history);

							break;
							#endregion //CollapseRibbonGroup
						}
					case GroupVariantResizeAction.HideGalleryPreview:
						{
							#region HideGalleryPreview
							// single pass over the menus
							MenuTool menuTool = this.GetNextMenu();

							while (menuTool != null)
							{
								// check for a menu that is actually showing a gallery
								if (menuTool.HasGalleryPreview)
								{
									action = new GalleryPreviewVisibilityAction(menuTool, false, this._history);
									break;
								}

								menuTool = this.GetNextMenu();
							}
							break;
							#endregion //HideGalleryPreview
						}
					case GroupVariantResizeAction.ReduceGalleryPreviewItems:
						// multiple passes over the menus until a complete pass is done
						int oldColumns;
						MenuTool menu = this.GetNextGalleryColumnMenu(out oldColumns);

						if (menu != null)
							action = new GalleryPreviewColumnAction(menu, oldColumns, oldColumns - 1, this._history);
						break;
					case GroupVariantResizeAction.IncreaseHorizontalWrapRowCount:
						{
							#region IncreaseHorizontalWrapRowCount

							// iterate over the panels going from 1 to 2. then wrap and go from 2 to 3
							ToolHorizontalWrapPanel horzPanel = this.GetNextHorizontalPanel();

							if (null != horzPanel)
								action = new HorizontalWrapRowCountAction(horzPanel, this._horizontalRowCount - 1, this._horizontalRowCount, this._history);
							break;

							#endregion //IncreaseHorizontalWrapRowCount
						}
					case GroupVariantResizeAction.ReduceImageAndTextLargeTools:
						{
							#region ReduceImageAndTextLargeTools

							action = this.CreateToolResizeAction(RibbonToolSizingMode.ImageAndTextNormal, RibbonToolSizingMode.ImageAndTextLarge);
							break;

							#endregion //ReduceImageAndTextLargeTools
						}
					case GroupVariantResizeAction.ReduceImageAndTextNormalTools:
						{
							#region ReduceImageAndTextNormalTools

							action = this.CreateToolResizeAction(RibbonToolSizingMode.ImageOnly, RibbonToolSizingMode.ImageAndTextNormal);
							break;

							#endregion //ReduceImageAndTextNormalTools
						}
				}

				this._lastAction = action;
				this._isComplete = action == null;
			}

			return action;
		}
		#endregion //CreateAction

		#region InvalidateLastAction
		/// <summary>
		/// Performs an undo on the last operation and 
		/// </summary>
		internal void InvalidateLastAction()
		{
			Debug.Assert(this._initialized);

			if (this._lastAction != null)
			{
				// this is used for any iterative action and specifically only those that wrap
				// around within their collection so that we do not try to process that same
				// object for which the action being invalidated had occurred.
				if (this._lastAction is ToolSizingModeAction)
				{
					this._panels.Remove(((ToolSizingModeAction)this._lastAction).Panel);
				}
				else if (this._lastAction is GalleryPreviewColumnAction)
				{
					this._menus.Remove(((GalleryPreviewColumnAction)this._lastAction).Menu);
				}

				this._lastAction = null;
			}
		}

		#endregion //InvalidateLastAction

		#endregion //Internal Methods

		#region Private Methods

		#region GetNextGalleryColumnMenu
		private MenuTool GetNextGalleryColumnMenu(out int currentColumnCount)
		{
			MenuTool menu = this.GetNextMenu();
			currentColumnCount = 0;

			while (menu != null)
			{
				if (menu.HasGalleryPreview)
				{
					MenuToolPresenter mtp = menu.MenuToolPresenter;
					GalleryToolPreviewPresenter gtpp = mtp != null ? mtp.GalleryToolPreviewPresenter : null;

					if (gtpp != null)
					{
						int maxColumns = gtpp.MaxColumns;

						if (maxColumns == 0)
							maxColumns = (int)gtpp.GalleryTool.GetValue(GalleryTool.MaxPossiblePreviewColumnsProperty);

						if (maxColumns > gtpp.MinColumns)
						{
							currentColumnCount = maxColumns;
							return menu;
						}
					}
				}

				// remove the menu so we don't bother with it in another pass
				this._menus.RemoveAt(this._currentMenuIndex);

				menu = this.GetNextMenu();
			}

			// if we didn't find one in the current row then look for one that can go
			// to the next row
			if (this._menus.Count > 0)
			{
				this._currentMenuIndex = this._menus.Count;
				return this.GetNextGalleryColumnMenu(out currentColumnCount);
			}

			return null;
		}
		#endregion //GetNextGalleryColumnMenu

		#region GetNextHorizontalPanel
		private ToolHorizontalWrapPanel GetNextHorizontalPanel()
		{
			IRibbonToolPanel panel = this.GetNextPanel();

			while (panel != null)
			{
				ToolHorizontalWrapPanel horzPanel = panel as ToolHorizontalWrapPanel;

				// if this panel can change its row count...
				if (horzPanel != null && horzPanel.MaxRows >= this._horizontalRowCount)
					return horzPanel;

				panel = this.GetNextPanel();
			}

			// if we didn't find one in the current row then look for one that can go
			// to the next row
			if (this._horizontalRowCount < ToolHorizontalWrapPanel.DefaultMaxRows)
			{
				this._currentPanelIndex = this._panels.Count;
				this._horizontalRowCount++;
				return this.GetNextHorizontalPanel();
			}

			return null;
		}
		#endregion //GetNextHorizontalPanel

		#region CreateToolResizeAction
		private ToolSizingModeAction CreateToolResizeAction(RibbonToolSizingMode newSizingMode, RibbonToolSizingMode oldSizingMode)
		{
			ToolSizingModeAction action = null;
			IRibbonToolPanel panel = this.GetNextPanel();

			while (panel != null)
			{
				IList<FrameworkElement> tools = panel.GetResizableTools(newSizingMode);

				if (null != tools && tools.Count > 0)
				{
					action = new ToolSizingModeAction(panel, tools, newSizingMode, oldSizingMode, this._history);
					break;
				}

				panel = this.GetNextPanel();
			}

			return action;
		}
		#endregion //CreateToolResizeAction

		#region GetNextGroup
		private RibbonGroup GetNextGroup()
		{
			this._currentGroupIndex++;

			return this._currentGroupIndex < this._groups.Count ? this._groups[this._currentGroupIndex] : null;
		}
		#endregion //GetNextGroup

		#region GetNextMenu
		private MenuTool GetNextMenu()
		{
			this._currentMenuIndex--;

			return this._currentMenuIndex >= 0 ? this._menus[this._currentMenuIndex] : null;
		}
		#endregion //GetNextMenu

		#region GetNextPanel
		private IRibbonToolPanel GetNextPanel()
		{
			this._currentPanelIndex--;

			return this._currentPanelIndex >= 0 ? this._panels[this._currentPanelIndex] : null;
		}
		#endregion //GetNextPanel

		#region Initialize
		/// <summary>
		/// Used to perform initialization of the resizer before the resize operation.
		/// </summary>
		private void Initialize()
		{
			switch (this.VariantResizeAction)
			{
				case GroupVariantResizeAction.CollapseRibbonGroup:
					// don't need menus or panels
					break;

				case GroupVariantResizeAction.ReduceGalleryPreviewItems:
				case GroupVariantResizeAction.HideGalleryPreview:
					// just need menus
					this._menus = new List<MenuTool>();
					for(int i = this._groups.Count - 1; i >= 0; i--)
						this._menus.AddRange(this._groups[i].RegisteredMenus);

					break;
				case GroupVariantResizeAction.IncreaseHorizontalWrapRowCount:
				case GroupVariantResizeAction.ReduceImageAndTextLargeTools:
				case GroupVariantResizeAction.ReduceImageAndTextNormalTools:
					// just need panels
					this._panels = new List<IRibbonToolPanel>();
					for(int i = this._groups.Count - 1; i >= 0; i--)
						this._panels.AddRange(this._groups[i].RegisteredPanels);
					break;
			}

			// post process to remove items we can't operate on?
			switch (this.VariantResizeAction)
			{
				case GroupVariantResizeAction.IncreaseHorizontalWrapRowCount:
					// remove other panel types
					for (int i = this._panels.Count - 1; i >= 0; i--)
					{
						if (this._panels[i] is ToolHorizontalWrapPanel == false)
							this._panels.RemoveAt(i);
					}
					break;
				case GroupVariantResizeAction.ReduceGalleryPreviewItems:
				case GroupVariantResizeAction.HideGalleryPreview:
					// remove non-gallery menus
					for (int i = this._menus.Count - 1; i >= 0; i--)
					{
						if (this._menus[i].HasGalleryPreview == false)
							this._menus.RemoveAt(i);
					}
					break;
			}

			// initialize starting indexes. note, we're iterating the groups forwards
			// since the first one added is the lowest priority - i.e. the first to be resized
			this._currentGroupIndex = -1;
			this._currentMenuIndex = this._menus != null ? this._menus.Count : 0;
			this._currentPanelIndex = this._panels != null ? this._panels.Count : 0;

			

			Debug.Assert(this._panels == null || this._menus == null, "The resizer is only meant to deal with iterating menus or panels!");
		}

		#endregion //Initialize

		#endregion //Private Methods
	} 
	#endregion //GroupActionFactory

	#region GroupVariantResizeHistory





	internal class GroupVariantResizeHistory : ActionHistory
	{
		#region Member Variables

		internal static object UndoContextId = new object();
		internal static object RedoContextId = new object();

		private double currentCachedExtent = double.NaN;
		private double nextLargerExtent = double.NaN;
		private RibbonGroupPanel _panel;

		#endregion //Member Variables

		#region Constructor
		internal GroupVariantResizeHistory(RibbonGroupPanel panel)
		{
			this._panel = panel;
		} 
		#endregion //Constructor

		#region Base class overrides
		protected override object RedoContext
		{
			get
			{
				return RedoContextId;
			}
		}

		protected override object UndoContext
		{
			get
			{
				return UndoContextId;
			}
		}

		protected override int MaxUndoDepth
		{
			get
			{
				return 0;
			}
		}
		#endregion //Base class overrides

		#region Properties

		#region CurrentExtent
		/// <summary>
		/// Returns the cached current extent.
		/// </summary>
		internal double CurrentExtent
		{
			get
			{
				return this.currentCachedExtent;
			}
			set
			{
				this.currentCachedExtent = value;
			}
		}
		#endregion //CurrentExtent

		#region NextLargerExtent
		/// <summary>
		/// Returns the cached extent that will occur if an undo operation is performed.
		/// </summary>
		internal double NextLargerExtent
		{
			get
			{
				return this.nextLargerExtent;
			}
			set
			{
				this.nextLargerExtent = value;
			}
		}
		#endregion //NextLargerExtent

		#region Panel
		internal RibbonGroupPanel Panel
		{
			get { return this._panel; }
		} 
		#endregion //Panel

		#region RedoActionOnQueue
		internal GroupVariantAction RedoActionOnQueue
		{
			get { return this.PeekRedo() as GroupVariantAction; }
		} 
		#endregion //RedoActionOnQueue

		#region UndoActionOnQueue
		internal GroupVariantAction UndoActionOnQueue
		{
			get { return this.PeekUndo() as GroupVariantAction; }
		} 
		#endregion //UndoActionOnQueue

		#endregion //Properties
	} 
	#endregion //GroupVariantResizeHistory

	#region GroupVariantAction





	internal abstract class GroupVariantAction : ActionHistory.ActionBase
	{
		#region Member Variables

		private double _resultingExtent;
		private double _nextLargerExtent;
		private GroupVariantResizeHistory _history;
		private RibbonGroup _group; 

		#endregion //Member Variables

		#region Constructor
		protected GroupVariantAction(RibbonGroup associatedGroup, GroupVariantResizeHistory history)
		{
			this._history = history;
			this._group = associatedGroup;
		} 
		#endregion //Constructor

		#region Properties

		public RibbonGroup Group
		{
			get { return this._group; }
		}

		protected GroupVariantResizeHistory History
		{
			get { return this._history; }
		}

		internal double NextLargerExtent
		{
			get { return this._nextLargerExtent; }
			set { this._nextLargerExtent = value; }
		}

		internal double ResultingExtent
		{
			get { return this._resultingExtent; }
			set { this._resultingExtent = value; }
		}

		#endregion //Properties

		#region Base class overrides

		internal protected sealed override ActionHistory.ActionBase Perform(object context)
		{
			GroupVariantAction undoAction = this.CreateUndoAction();

			double oldResultingExtent = this._resultingExtent;
			double oldNextLargerExtent = this._nextLargerExtent;

			// always use the existing history values
			undoAction._resultingExtent = this._history.CurrentExtent;
			undoAction._nextLargerExtent = this._history.NextLargerExtent;

			this.PerformAction(context);

			if (context == GroupVariantResizeHistory.UndoContextId ||
				context == GroupVariantResizeHistory.RedoContextId)
			{
				this._history.CurrentExtent = oldResultingExtent;
				this._history.NextLargerExtent = oldNextLargerExtent;
			}

			return undoAction;
		}

		#endregion //Base class overrides

		#region Methods

		/// <summary>
		/// Used to perform the associated action.
		/// </summary>
		/// <param name="context">The context with which the object is being executed</param>
		protected abstract void PerformAction(object context);

		/// <summary>
		/// Used to create an undo of the current action.
		/// </summary>
		protected virtual GroupVariantAction CreateUndoAction()
		{
			return this;
		}

		/// <summary>
		/// Used to reinitialize the state of a ribbon group that is becoming collapsed or uncollapsed.
		/// </summary>
		/// <param name="invertAction">True to perform an undo of the current action</param>
		public void ReplayGroupAction(bool invertAction)
		{
			GroupVariantAction action = invertAction == false ? this : this.CreateUndoAction();
			object context = invertAction ? GroupVariantResizeHistory.RedoContextId : GroupVariantResizeHistory.UndoContextId;

			if (null != action)
				action.PerformAction(context);
		}

		#endregion //Methods	
	} 
	#endregion //GroupVariantAction

	#region CollapsedGroupVariantAction





	internal class CollapseGroupAction : GroupVariantAction
	{
		private bool _isCollapsed;

		internal CollapseGroupAction(RibbonGroup group, bool isCollapsed, GroupVariantResizeHistory history) : base(group, history)
		{
			this._isCollapsed = isCollapsed;
		}

		internal protected override bool CanPerform(object context)
		{
			return true;
		}

		protected override void PerformAction(object context)
		{
			bool isCollapsed = context == GroupVariantResizeHistory.UndoContextId ? !this._isCollapsed : this._isCollapsed;
			this.Group.SetValue(RibbonGroup.IsCollapsedPropertyKey, isCollapsed);

			RibbonGroupPanel.BumpSizingModeVersion(this.Group);
		}

		public override string ToString()
		{
			return string.Format("Change RibbonGroup IsCollapsed of '{1}' to {0}", this._isCollapsed, this.Group);
		}
	} 
	#endregion //CollapsedGroupVariantAction

	#region GalleryPreviewVisibilityAction





	internal class GalleryPreviewVisibilityAction : GroupVariantAction
	{
		private MenuTool _menuTool;
		private bool _isPreviewVisible;

		internal GalleryPreviewVisibilityAction(MenuTool menu, bool isPreviewVisible, GroupVariantResizeHistory history) : base(RibbonGroup.GetContainingGroup(menu), history)
		{
			this._menuTool = menu;
			this._isPreviewVisible = isPreviewVisible;
		}

		internal protected override bool CanPerform(object context)
		{
			return _menuTool.HasGalleryPreview;
		}

		protected override void PerformAction(object context)
		{
			bool isVisible = context == GroupVariantResizeHistory.UndoContextId ? !this._isPreviewVisible : this._isPreviewVisible;

			if (isVisible)
				this._menuTool.ClearValue(MenuTool.HideGalleryPreviewProperty);
			else
				this._menuTool.SetValue(MenuTool.HideGalleryPreviewProperty, KnownBoxes.TrueBox);

			UIElement descendant = (UIElement)this._menuTool.MenuToolPresenter ?? (UIElement)this._menuTool;
			RibbonGroupPanel.InvalidateMeasure(descendant, this.History.Panel);
		}

		public override string ToString()
		{
			return string.Format("Change GalleryPreviewVisibility of '{1}' to {0}", this._isPreviewVisible, this._menuTool);
		}
	} 
	#endregion //GalleryPreviewVisibilityAction

	#region HorizontalWrapRowCountAction





	internal class HorizontalWrapRowCountAction : GroupVariantAction
	{
		private ToolHorizontalWrapPanel _panel;
		private int _newRowCount;
		private int _oldRowCount;

		internal HorizontalWrapRowCountAction(ToolHorizontalWrapPanel panel, int oldRowCount, int newRowCount, GroupVariantResizeHistory history) : base(RibbonGroup.GetContainingGroup(panel), history)
		{
			this._panel = panel;
			this._newRowCount = newRowCount;
			this._oldRowCount = oldRowCount;
		}

		internal protected override bool CanPerform(object context)
		{
			// if the new row count is within range
			return this._newRowCount <= this._panel.MaxRows &&
				this._newRowCount >= this._panel.MinRows;
		}

		protected override void PerformAction(object context)
		{
			int newRowCount = context == GroupVariantResizeHistory.UndoContextId ? this._oldRowCount : this._newRowCount;
			this._panel.SetValue(ToolHorizontalWrapPanel.RowCountPropertyKey, newRowCount);

			// AS 10/12/07 BR27229
			RibbonGroupPanel.InvalidateMeasure(this._panel, this.History.Panel);
		}

		public override string ToString()
		{
			return string.Format("Change ToolHorizontalWrapPanel RowCount of '{2}' From {0} to {1}", this._oldRowCount, this._newRowCount, this._panel);
		}
	} 
	#endregion //HorizontalWrapRowCountAction

	#region GalleryPreviewColumnAction





	internal class GalleryPreviewColumnAction : GroupVariantAction
	{
		private MenuTool _menu;
		private int _oldColumnCount;
		private int _newColumnCount;

		internal GalleryPreviewColumnAction(MenuTool menu, int oldColumnCount, int newColumnCount, GroupVariantResizeHistory history) : base(RibbonGroup.GetContainingGroup(menu), history)
		{
			this._menu = menu;
			this._oldColumnCount = oldColumnCount;
			this._newColumnCount = newColumnCount;
		}

		internal MenuTool Menu
		{
			get { return this._menu; }
		}

		internal protected override bool CanPerform(object context)
		{
			return this._menu.HasGalleryPreview;
		}

		protected override void PerformAction(object context)
		{
			int columnCount = context == GroupVariantResizeHistory.UndoContextId ? this._oldColumnCount : this._newColumnCount;
			MenuToolPresenter mtp = this._menu.MenuToolPresenter;

			if (null != mtp)
			{
				mtp.SetValue(MenuToolPresenter.ResizedColumnCountProperty, columnCount);
				RibbonGroupPanel.InvalidateMeasure(mtp, this.History.Panel);
			}
		}

		public override string ToString()
		{
			return string.Format("Change GalleryPreviewColumns For {2} From {0} to {1}", this._oldColumnCount, this._newColumnCount, this._menu);
		}
	} 
	#endregion //GalleryPreviewColumnAction

	#region ToolSizingModeAction





	internal class ToolSizingModeAction : GroupVariantAction
	{
		private IList<FrameworkElement> _tools;
		private RibbonToolSizingMode _newSizingMode;
		private RibbonToolSizingMode _previousSizingMode;
		private IRibbonToolPanel _panel;

		// AS 9/18/09 TFS19766
		private RibbonToolSizingMode[] _originalSizingModes;

		internal ToolSizingModeAction(IRibbonToolPanel panel, IList<FrameworkElement> tools, RibbonToolSizingMode newSizingMode, RibbonToolSizingMode oldSizingMode, GroupVariantResizeHistory history) : base(RibbonGroup.GetContainingGroup((DependencyObject)panel), history)
		{
			this._tools = tools;
			this._newSizingMode = newSizingMode;
			this._previousSizingMode = oldSizingMode;
			this._panel = panel;

			// AS 9/18/09 TFS19766
			// Now some tools going to ImageOnly may not have been ImageAndTextNormal so 
			// we need to store their previous sizing mode.
			//
			_originalSizingModes = new RibbonToolSizingMode[tools.Count];

			for (int i = 0, count = tools.Count; i < count; i++)
				_originalSizingModes[i] = RibbonToolHelper.GetSizingMode(tools[i]);
		}

		internal IRibbonToolPanel Panel
		{
			get { return this._panel; }
		}

		internal protected override bool CanPerform(object context)
		{
			return this._tools.Count > 0;
		}

		protected override void PerformAction(object context)
		{
			// AS 9/18/09 TFS19766
			// We also need to know when we are performing an undo because we want to restore
			// to the cached previous sizing mode as opposed to the phase's mode.
			//
			//RibbonToolSizingMode sizingMode = context == GroupVariantResizeHistory.UndoContextId ? this._previousSizingMode : this._newSizingMode;
			bool isUndo = context == GroupVariantResizeHistory.UndoContextId;
			RibbonToolSizingMode sizingMode = isUndo ? this._previousSizingMode : this._newSizingMode;
			
			object newSizingMode = RibbonKnownBoxes.FromValue(sizingMode);

			// update the tools
			// AS 9/18/09 TFS19766
			//foreach (FrameworkElement tool in this._tools)
			for (int i = 0, count = _tools.Count; i < count; i++)
			{
				FrameworkElement tool = _tools[i];

				// AS 9/18/09 TFS19766
				// In order to maintain the column structure when you have some tools that were 
				// ImageAndTextNormal and then you get those tools and some that were large 
				// going to ImageOnly, we need to store the previous size so we know when to 
				// insert a column break. E.g. 2 large tools followed by a column of 3 normal 
				// tools going to image only should result in a column of 2 image only tools 
				// followed by a column of 3 image only tools that were just normal.
				//
				object previousSizingMode = RibbonKnownBoxes.FromValue(_originalSizingModes[i]);

				if (isUndo)
				{
					newSizingMode = previousSizingMode;
					previousSizingMode = DependencyProperty.UnsetValue;
				}

				tool.SetValue(PreviousSizingModeProperty, previousSizingMode);

				tool.SetValue(RibbonToolHelper.SizingModePropertyKey, newSizingMode);
				RibbonGroupPanel.BumpSizingModeVersion(tool);
			}

			if (this._tools.Count > 0)
				RibbonGroupPanel.InvalidateMeasure(this._tools[0], this.History.Panel);
		}

		public override string ToString()
		{
			return string.Format("Change Tool Sizing of {2} tools in RibbonGroup '{3}' From {0} to {1}", this._previousSizingMode, this._newSizingMode, this._tools.Count, this.Group);
		}

		// AS 9/18/09 TFS19766
		#region PreviousSizingMode

		/// <summary>
		/// PreviousSizingMode Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty PreviousSizingModeProperty =
			DependencyProperty.RegisterAttached("PreviousSizingMode", typeof(RibbonToolSizingMode?), typeof(ToolSizingModeAction),
				new FrameworkPropertyMetadata((RibbonToolSizingMode?)null));

		public static RibbonToolSizingMode? GetPreviousSizingMode(DependencyObject d)
		{
			return (RibbonToolSizingMode?)d.GetValue(PreviousSizingModeProperty);
		}

		public static void SetPreviousSizingMode(DependencyObject d, RibbonToolSizingMode? value)
		{
			d.SetValue(PreviousSizingModeProperty, value);
		}

		#endregion //PreviousSizingMode
	} 
	#endregion //ToolSizingModeAction
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