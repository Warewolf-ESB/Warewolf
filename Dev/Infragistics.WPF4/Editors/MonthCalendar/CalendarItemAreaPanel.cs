using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace Infragistics.Windows.Editors
{
    /// <summary>
    /// A custom element used to display a <see cref="CalendarItemArea"/> within a 
    /// <see cref="CalendarItemGroup"/> to perform animations.
    /// </summary>
    /// <see cref="CalendarItemArea"/>
    //[System.ComponentModel.ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CalendarItemAreaPanel : FrameworkElement,
        ICalendarItemArea
    {
        #region Member Variables

        private CalendarItemGroup _group;
        // AS 10/1/08 TFS8497
        // Changed from using a fixed set of two items to a collection so the sizing group
        // can have one per available mode.
        //
        private List<CalendarItemArea> _areas = new List<CalendarItemArea>();
        private int _areaIndex = 0;
        private CalendarAnimation? _pendingAnimation;
        private Storyboard _sbOldArea;
        private Storyboard _sbNewArea;

        #endregion //Member Variables

        #region Constructor
        static CalendarItemAreaPanel()
        {
            UIElement.ClipToBoundsProperty.OverrideMetadata(typeof(CalendarItemAreaPanel), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
            CalendarItemGroup.CurrentCalendarModePropertyKey.OverrideMetadata(typeof(CalendarItemAreaPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCurrentCalendarModeChanged)));
        }

        /// <summary>
        /// Initializes a new <see cref="CalendarItemAreaPanel"/>
        /// </summary>
        public CalendarItemAreaPanel()
        {
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
            Rect arrangeRect = new Rect(finalSize);

            for(int i = 0; i < this._areas.Count; i++)
            {
                this._areas[i].Arrange(arrangeRect);
            }

            return finalSize;
        }

        #endregion //ArrangeOverride

        #region GetVisualChild
        /// <summary>
        /// Returns the visual child at the specified index.
        /// </summary>
        /// <param name="index">Integer position of the child to return.</param>
        /// <returns>The child element at the specified position.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is greater than the <see cref="VisualChildrenCount"/></exception>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= this._areas.Count)
                throw new ArgumentOutOfRangeException();

            return this._areas[index];
        }
        #endregion //GetVisualChild

        #region LogicalChildren
        /// <summary>
        /// Returns an enumerator of the logical children
        /// </summary>
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                return this._areas.GetEnumerator();
            }
        }
        #endregion //LogicalChildren

        #region MeasureOverride
        /// <summary>
        /// Invoked to measure the element and its children.
        /// </summary>
        /// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size();

            for (int i = 0, count = this._areas.Count; i < count; i++)
            {
                CalendarItemArea area = this._areas[i];
                area.Measure(availableSize);

                Size childSize = area.DesiredSize;

                if (childSize.Width > desiredSize.Width)
                    desiredSize.Width = childSize.Width;
                if (childSize.Height > desiredSize.Height)
                    desiredSize.Height = childSize.Height;

            }

            return desiredSize;
        }
        #endregion //MeasureOverride

        #region OnInitialized

        /// <summary>
        /// Overriden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInitialized(EventArgs e)
        {
            // AS 10/1/08 TFS8497
            // Let the group do this when we register the item area.
            //
            //((ICalendarItemArea)this).InitializeItems();
            // AS 10/1/08 TFS8497
            // If this is associated with the sizing group then we want to initialize
            // the areas with one for each supported mode.
            //
            if (null != this._group)
            {
                if (this._group.IsGroupForSizing)
                    this.InitializeSizingGroupPanel();
                else
                    ((ICalendarItemArea)this).InitializeItems();
            }

            base.OnInitialized(e);
        }
        #endregion //OnInitialized

        #region OnVisualParentChanged
        /// <summary>
        /// Invoked when the visual parent of the element has been changed.
        /// </summary>
        /// <param name="oldParent">The previous visual parent</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            CalendarItemGroup group = this.TemplatedParent as CalendarItemGroup;

            if (null != group)
                this.InitializeGroup(group);

            base.OnVisualParentChanged(oldParent);
        }
        #endregion //OnVisualParentChanged

        #region VisualChildrenCount
        /// <summary>
        /// Returns the number of visual children for the element.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return this._areas.Count;
            }
        }
        #endregion //VisualChildrenCount

        #endregion //Base class overrides

        #region Properties

        #region CurrentArea
        private CalendarItemArea CurrentArea
        {
            get 
            { 
                return this.CreateItemArea(this._areaIndex, null);
            }
        } 
        #endregion //CurrentArea

        #region OldArea
        private CalendarItemArea OldArea
        {
            get 
            {
                int index = this._areaIndex == 0 ? 1 : 0;
                return this.CreateItemArea(index, null);
            }
        } 
        #endregion //OldArea

        #endregion //Properties

        #region Methods
        
        #region Private

        #region AddFadeAnimation
        private static void AddFadeAnimation(Duration duration, bool isNew, Storyboard sb)
        {
            DoubleAnimation opa = new DoubleAnimation();
            opa.SetValue(isNew ? DoubleAnimation.ToProperty : DoubleAnimation.FromProperty, 1d);
            opa.SetValue(isNew ? DoubleAnimation.FromProperty : DoubleAnimation.ToProperty, 0d);
            opa.Duration = duration;
            Storyboard.SetTargetProperty(opa, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(opa);
        }
        #endregion //AddFadeAnimation

        #region AddVisibilityAnimation
        private static void AddVisibilityAnimation(Storyboard sb, Duration duration)
        {
            // this is used for the old area so that it starts as visible and ends as hidden
            ObjectAnimationUsingKeyFrames visa = new ObjectAnimationUsingKeyFrames();
            DiscreteObjectKeyFrame visStart = new DiscreteObjectKeyFrame(KnownBoxes.VisibilityVisibleBox, KeyTime.FromPercent(0d));
            DiscreteObjectKeyFrame visEnd = new DiscreteObjectKeyFrame(KnownBoxes.VisibilityHiddenBox, KeyTime.FromPercent(1d));
            visa.KeyFrames.Add(visStart);
            visa.KeyFrames.Add(visEnd);
            Storyboard.SetTargetProperty(visa, new PropertyPath(FrameworkElement.VisibilityProperty));
            visa.Duration = duration;
            sb.Children.Add(visa);
        }
        #endregion //AddVisibilityAnimation

        #region ClearPreviousAnimations
        private void ClearPreviousAnimations()
        {
            CalendarItemArea oldArea = this.OldArea;
            CalendarItemArea currentArea = this.CurrentArea;

            RemoveStoryboard(ref this._sbOldArea, oldArea);
            RemoveStoryboard(ref this._sbNewArea, currentArea);

            oldArea.ClearValue(FrameworkElement.RenderTransformProperty);
            currentArea.ClearValue(FrameworkElement.RenderTransformProperty);
            oldArea.ClearValue(FrameworkElement.OpacityProperty);
            currentArea.ClearValue(FrameworkElement.OpacityProperty);
        }
        #endregion //ClearPreviousAnimations

        #region CreateItemArea
        private CalendarItemArea CreateItemArea(int index, CalendarMode? mode)
        {
            if (index >= this._areas.Count)
                this._areas.AddRange(new CalendarItemArea[1 + index - this._areas.Count]);

            if (this._areas[index] == null)
            {
                CalendarItemArea area = this._areas[index] = new CalendarItemArea();
                ((ISupportInitialize)area).BeginInit();

                if (null != mode)
                    CalendarItemGroup.SetCurrentCalendarMode(area, mode.Value);

                area.InitializeGroup(this._group);
                this.AddVisualChild(area);
                this.AddLogicalChild(area);
                ((ISupportInitialize)area).EndInit();

                this.InvalidateMeasure();
            }

            return this._areas[index];
        }
        #endregion //CreateItemArea

        #region CreateFadeStoryboard
        private static Storyboard CreateFadeStoryboard(Duration duration, bool isNew, CalendarItemArea area)
        {
            Storyboard sb = new Storyboard();
            sb.Duration = duration;
            sb.FillBehavior = FillBehavior.Stop;

            // and we need to fade it out/in
            AddFadeAnimation(duration, isNew, sb);

            if (false == isNew)
                AddVisibilityAnimation(sb, duration);

            return sb;
        }
        #endregion //CreateFadeStoryboard

        #region CreateScrollStoryboard
        private static Storyboard CreateScrollStoryboard(CalendarItemArea area, bool scrollLeft, bool isNew, double offset, Duration duration)
        {
            TranslateTransform ttOld = new TranslateTransform();
            area.RenderTransform = ttOld;

            Storyboard sb = new Storyboard();
            sb.FillBehavior = FillBehavior.Stop;

            DoubleAnimation ttax = new DoubleAnimation();
            ttax.From = isNew ? (scrollLeft ? offset : -offset) : 0;
            ttax.To = isNew ? 0 : (scrollLeft ? -offset : offset);
            ttax.Duration = duration;
            Storyboard.SetTargetProperty(ttax, new PropertyPath("(0).(1)", UIElement.RenderTransformProperty, TranslateTransform.XProperty));
            sb.Children.Add(ttax);

            if (isNew == false)
                AddVisibilityAnimation(sb, duration);

            return sb;
        }
        #endregion //CreateScrollStoryboard

        #region CreateZoomStoryboard
        private static Storyboard CreateZoomStoryboard(CalendarItem referenceItem, bool zoomOut, Duration duration, bool isNew, CalendarItemArea area)
        {
            // zoom out:
            // old start=unscaled end=scaled down to where item is
            // new start =scaled up to where reference item would fill new=unscaled
            // zoom in:
            // old start=unscaled end=scaled up to where reference item would fill
            // new start=scaled down to where reference item is new=unscaled

            Storyboard sb = new Storyboard();
            sb.Duration = duration;
            sb.FillBehavior = FillBehavior.Stop;

            if (null != referenceItem)
            {
                // in a zoom out, the old area is the full size and zooms down
                // to fit within where the new one will be
                TransformGroup tg = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                TranslateTransform tt = new TranslateTransform();
                tg.Children.Add(st);
                tg.Children.Add(tt);

                area.RenderTransform = tg;

                double scaleX = referenceItem.ActualWidth / area.ActualWidth;
                double scaleY = referenceItem.ActualHeight / area.ActualHeight;

                if (isNew == zoomOut) // if zoomout & new or zoomin and old
                {
                    scaleX = 1 / scaleX;
                    scaleY = 1 / scaleY;
                }

                // scale to reference item
                DoubleAnimation stax = new DoubleAnimation(isNew ? scaleX : 1, isNew ? 1 : scaleX, duration);
                Storyboard.SetTargetProperty(stax, new PropertyPath("(0).(1)[0].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, ScaleTransform.ScaleXProperty));
                sb.Children.Add(stax);

                DoubleAnimation stay = new DoubleAnimation(isNew ? scaleY : 1, isNew ? 1 : scaleY, duration);
                Storyboard.SetTargetProperty(stay, new PropertyPath("(0).(1)[0].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, ScaleTransform.ScaleYProperty));
                sb.Children.Add(stay);

                Point upperLeft = referenceItem.TransformToVisual(area).Transform(new Point());

                if (isNew == zoomOut) // zoomout & new or zoomin and old
                {
                    upperLeft.X *= -scaleX;
                    upperLeft.Y *= -scaleY;
                }

                // as it scales down we want to offset to ultimately center over the resulting item
                DoubleAnimation ttax = new DoubleAnimation(isNew ? upperLeft.X : 0, isNew ? 0 : upperLeft.X, duration);
                Storyboard.SetTargetProperty(ttax, new PropertyPath("(0).(1)[1].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, TranslateTransform.XProperty));
                sb.Children.Add(ttax);

                DoubleAnimation ttay = new DoubleAnimation(isNew ? upperLeft.Y : 0, isNew ? 0 : upperLeft.Y, duration);
                Storyboard.SetTargetProperty(ttay, new PropertyPath("(0).(1)[1].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, TranslateTransform.YProperty));
                sb.Children.Add(ttay);
            }

            // and we need to fade it out/in
            AddFadeAnimation(duration, isNew, sb);

            if (false == isNew)
                AddVisibilityAnimation(sb, duration);

            return sb;
        }
        #endregion //CreateZoomStoryboard

        // AS 10/1/08 TFS8497
        #region HasItemArea
        private bool HasItemArea(int index)
        {
            return index < this._areas.Count && this._areas[index] != null;
        }
        #endregion //HasItemArea

        #region InitializeGroup
        internal void InitializeGroup(CalendarItemGroup group)
        {
            Debug.Assert(null == this._group || group == this._group);

            if (this._group == group)
                return;

            if (null != this._group)
                this._group.UnregisterItemArea(this);

            this._group = group;

            if (null != this._group)
            {
                for (int i = 0, count = this._areas.Count; i < count; i++)
                    this._areas[i].InitializeGroup(group);

                if (group == this.TemplatedParent)
                    this._group.RegisterItemArea(this);

                // AS 10/1/08 TFS8497
                // Let the group do this when we register the item area.
                //
                //((ICalendarItemArea)this).InitializeItems();

                // AS 10/1/08 TFS8497
                // If this is associated with the sizing group then we want to initialize
                // the areas with one for each supported mode.
                //
                if (group.IsGroupForSizing)
                    this.InitializeSizingGroupPanel();
            }
        }
        #endregion //InitializeGroup

        // AS 10/1/08 TFS8497
        #region InitializeSizingGroupPanel
        private void InitializeSizingGroupPanel()
        {
            if (this.IsInitialized == false)
                return;

            if (null != this._group && this._group.IsGroupForSizing)
            {
                int minMode = (int)this._group.CurrentCalendarMode;
                int maxMode = (int)CalendarMode.Centuries;

                // let the first area always just pick up the min mode from the group
                this.CreateItemArea(0, null);

                int maxOffset = maxMode - minMode;

                for (int i = 1; i <= maxOffset; i++)
                {
                    CalendarMode mode = (CalendarMode)(i + minMode);
                    CalendarItemArea area = this.CreateItemArea(i, mode);
                    CalendarItemGroup.SetCurrentCalendarMode(area, mode);
                }

                for (int i = this._areas.Count - 1; i > maxOffset; i--)
                {
                    CalendarItemArea oldArea = this._areas[i];
                    this._areas.RemoveAt(i);
                    this.RemoveVisualChild(oldArea);
                    this.RemoveLogicalChild(oldArea);
                }
            }
        } 
        #endregion //InitializeSizingGroupPanel

        #region OnCurrentCalendarModeChanged
        private static void OnCurrentCalendarModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarItemAreaPanel p = (CalendarItemAreaPanel)d;

            // AS 10/1/08 TFS8497
            // The sizing group will now be handled specially. It doesn't animate but it will use
            // the area collection to maintain an area for each mode to allow property sizing
            // calculations instead of relying on the size of one mode.
            //
            if (p._group != null && p._group.IsGroupForSizing)
            {
                p.InitializeSizingGroupPanel();
                return;
            }

            // since we have to set a local value for the current calendar mode of the areas
            // when the animation occurs we have to manually keep the current area's value in
            // sync with the panels. for some reason clearing the local value on the area 
            // does not cause the value to use that of the ancestor again.
            if (p._areas.Count > 1)
            {
                p.CurrentArea.SetValue(CalendarItemGroup.CurrentCalendarModePropertyKey, e.NewValue);
            }
        }
        #endregion //OnCurrentCalendarModeChanged

        #region RemoveStoryboard
        private static void RemoveStoryboard(ref Storyboard sb, CalendarItemArea area)
        {
            if (null != sb)
            {
                sb.Stop(area);
                sb.Remove(area);
                sb = null;
            }
        }
        #endregion //RemoveStoryboard

        #endregion //Private

        #endregion //Methods

        #region ICalendarItemAreaPanel Members

        void ICalendarItemArea.InitializeItems()
        {
            // AS 10/1/08 TFS8497
            // To avoid double initialization of the items (which will happen when 
            // the OnInitialized is called if the item is being created), we just
            // need to create the area if its not already created.
            //
            //((ICalendarItemArea)this.CurrentArea).InitializeItems();
            //return;
            if (this.IsInitialized == false)
                return;

            if (this.HasItemArea(this._areaIndex))
                ((ICalendarItemArea)this.CurrentArea).InitializeItems();
            else // just create it and it will initialize its own items when its created
                this.CreateItemArea(this._areaIndex, null);
        }

        void ICalendarItemArea.InitializeWeekNumbers()
        {
            ((ICalendarItemArea)this.CurrentArea).InitializeWeekNumbers();
        }

        void ICalendarItemArea.InitializeFirstItemOffset()
        {
            ((ICalendarItemArea)this.CurrentArea).InitializeFirstItemOffset();
        }

        void ICalendarItemArea.InitializeRowColCount()
        {
			// AS 12/8/09 TFS24207
            //((ICalendarItemArea)this.CurrentArea).InitializeRowColCount();
			foreach (ICalendarItemArea area in _areas)
			{
				if (null != area)
					area.InitializeRowColCount();
			}
        }

        IList<CalendarItem> ICalendarItemArea.Items
        {
            get 
            {
                return ((ICalendarItemArea)this.CurrentArea).Items;
            }
        }

        void ICalendarItemArea.ReinitializeItems(CalendarItemChange change)
        {
            ((ICalendarItemArea)this.CurrentArea).ReinitializeItems(change);
        }

        void ICalendarItemArea.PrepareForAnimationAction(CalendarAnimation action)
        {
            if (null == this._pendingAnimation)
            {
                Debug.Assert(null != this._group && this._group.AllowAnimation);

                // clear the results of any previous animations
                this.ClearPreviousAnimations();

                // swap the areas and 
                this._areaIndex = this._areaIndex == 0 ? 1 : 0;
                this.CreateItemArea(this._areaIndex, null);
                this._pendingAnimation = action;

                // keep the old area using the old calendar mode and let the 
                // new current area get its value from the group
                this.OldArea.SetValue(CalendarItemGroup.CurrentCalendarModePropertyKey, CalendarItemGroup.GetCurrentCalendarMode(this));

                // i wanted to just clear the set value assuming that it would once again
                // inherit the value of the ancestor but that does not seem to be case. so
                // we have to set the value here and keep them in sync whenever the mode changes
                //this.CurrentArea.ClearValue(CalendarItemGroup.CurrentCalendarModePropertyKey);
                //Debug.Assert(CalendarItemGroup.GetCurrentCalendarMode(this) == CalendarItemGroup.GetCurrentCalendarMode(this.CurrentArea));
                this.CurrentArea.SetValue(CalendarItemGroup.CurrentCalendarModePropertyKey, CalendarItemGroup.GetCurrentCalendarMode(this));

                // we don't want to show the current area until the animation is performed
                this.CurrentArea.Visibility = Visibility.Hidden;
            }
        }

        void ICalendarItemArea.PerformAnimationAction(CalendarAnimation action)
        {
            if (action == this._pendingAnimation)
            {
                this._pendingAnimation = null;
                ((ICalendarItemArea)this).InitializeItems();
                CalendarItemArea oldArea = this.OldArea;
                CalendarItemArea currentArea = this.CurrentArea;
                CalendarDateRange? oldRange = oldArea.GroupRange;
                CalendarDateRange? newRange = currentArea.GroupRange;

                Storyboard sbOld = null;
                Storyboard sbNew = null;

                XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);

				// AS 10/15/09 TFS23858
                //if (null != oldRange && null != newRange && oldRange != newRange && this.IsVisible)
				bool canAnimate = false;

				if (oldRange != null && newRange != null && this.IsVisible)
				{
					if (oldRange != newRange)
					{
						canAnimate = true;
					}
					else if (oldArea.CurrentCalendarMode != currentArea.CurrentCalendarMode &&
						(oldArea.FirstItemColumnOffset != currentArea.FirstItemColumnOffset ||
						oldArea.FirstItemRowOffset != currentArea.FirstItemRowOffset ||
						oldArea.ItemRows != currentArea.ItemRows ||
						oldArea.ItemColumns != currentArea.ItemColumns ||
						oldArea.Items.Count != currentArea.Items.Count))
					{
						// We should perform the animation even if the range is the same if the items will look different
						canAnimate = true;
					}
				}

                if (canAnimate)
                {
                    Duration duration = new Duration(new TimeSpan(TimeSpan.TicksPerSecond / 3));

                    switch (action)
                    {
                        case CalendarAnimation.ZoomIn:
                        case CalendarAnimation.ZoomOut:
                            #region Zoom

                            // ensure the elements are positioned
                            this.UpdateLayout();

                            CalendarItem referenceItem = null;
                            bool zoomOut = action == CalendarAnimation.ZoomOut;

                            #region Find Reference Item
                            
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

							// AS 10/15/09 TFS23858
							// If the ranges are the same we need to rely upon the specified type of animation.
							//
							if (newRange == oldRange)
							{
								if (zoomOut)
									referenceItem = currentArea.GetItem(oldRange.Value.Start);
								else
									referenceItem = oldArea.GetItem(newRange.Value.Start);
							}
							else
                            if (newRange.Value.Contains(oldRange.Value) || zoomOut)
                            {
                                Debug.Assert(zoomOut);
                                referenceItem = currentArea.GetItem(oldRange.Value.Start);
                            }
                            else if (oldRange.Value.Contains(newRange.Value) || !zoomOut)
                            {
                                Debug.Assert(false == zoomOut);
                                referenceItem = oldArea.GetItem(newRange.Value.Start);
                            }

                            if (null == referenceItem)
                            {
                                Debug.Assert(zoomOut);

                                // try to find the item in another group
                                if (null != cal)
                                {
                                    DateTime itemDate = zoomOut ? oldRange.Value.Start : newRange.Value.Start;

                                    foreach (CalendarItemGroup group in cal.GetGroups())
                                    {
                                        CalendarItemAreaPanel otherPanel = group.ItemArea as CalendarItemAreaPanel;

                                        if (null != otherPanel && this != otherPanel)
                                        {
                                            CalendarItemArea area = zoomOut ? otherPanel.CurrentArea : otherPanel.OldArea;
                                            referenceItem = area.GetItem(itemDate);

                                            if (null != referenceItem)
                                                break;
                                        }
                                    }
                                }
                            } 
                            #endregion //Find Reference Item

                            // create the appropriate storyboards
                            sbOld = CreateZoomStoryboard(referenceItem, zoomOut, duration, false, oldArea);
                            sbNew = CreateZoomStoryboard(referenceItem, zoomOut, duration, true, currentArea);

                            break; 

                            #endregion //Zoom

                        case CalendarAnimation.Fade:
                            #region Fade
                            sbOld = CreateFadeStoryboard(duration, false, oldArea);
                            sbNew = CreateFadeStoryboard(duration, true, currentArea);
                            break; 
                            #endregion //Fade

                        case CalendarAnimation.Scroll:
                            #region Scroll

                            // see whether we are scroll right to left or left to right
                            // based on the first dates of the old and current item area
                            bool scrollLeft = oldRange.Value.End < newRange.Value.Start;
                            Debug.Assert(scrollLeft || oldRange.Value.Start > newRange.Value.End);

                            double offset = this.ActualWidth;

                            sbOld = CreateScrollStoryboard(oldArea, scrollLeft, false, offset, duration);
                            sbNew = CreateScrollStoryboard(currentArea, scrollLeft, true, offset, duration);
                            break;

                            #endregion //Scroll
                    }
                }

                // AS 2/9/09 TFS11631
                // When animating we use 2 different CalendarItemArea instances. The focus 
                // however would be in the old item area and we cannot give focus to the 
                // element in the new area until it is considered focusable by the wpf 
                // framework (i.e. Enabled, IsVisible, etc.). The IsVisible state will not 
                // be true until the element is displayed within the itemscontrol. To get 
                // around this we will temporarily shift focus into the month calendar and 
                // have it asynchronously focus the active item.
                // 
                if (null != cal && (this.IsKeyboardFocusWithin || cal.IsKeyboardFocused))
                    cal.FocusActiveItemWithDelay();

                // make sure the elements are visible/hidden as needed
                oldArea.Visibility = Visibility.Hidden;
                currentArea.Visibility = Visibility.Visible;

                // store the storyboards so we can end them
                this._sbOldArea = sbOld;
                this._sbNewArea = sbNew;

                if (null != sbOld)
                {
                    sbOld.Begin(oldArea, HandoffBehavior.Compose, true);
                }

                if (null != sbNew)
                {
                    sbNew.Begin(currentArea, HandoffBehavior.Compose, true);
                }
            }
        }

        CalendarItem ICalendarItemArea.GetItem(DateTime date)
        {
            return ((ICalendarItemArea)this.CurrentArea).GetItem(date);
        }
        #endregion //ICalendarItemAreaPanel Members
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