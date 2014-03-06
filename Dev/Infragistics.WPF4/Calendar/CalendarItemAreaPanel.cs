using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Data;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A custom element used to display a <see cref="CalendarItemArea"/> within a 
    /// <see cref="CalendarItemGroup"/> to perform animations.
    /// </summary>
    /// <see cref="CalendarItemArea"/>
    //[System.ComponentModel.ToolboxItem(false)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class CalendarItemAreaPanel : Panel,
        ICalendarItemArea, ICalendarElement
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
		private bool _isInitialized;



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        #endregion //Member Variables

        #region Constructor
        static CalendarItemAreaPanel()
        {
           //CalendarItemGroup.CurrentModePropertyKey.OverrideMetadata(typeof(CalendarItemAreaPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCurrentModeChanged)));
        }

        /// <summary>
        /// Initializes a new <see cref="CalendarItemAreaPanel"/>
        /// </summary>
        public CalendarItemAreaPanel()
        {
			this.Loaded += new RoutedEventHandler(OnLoaded);
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
            Rect arrangeRect = new Rect(new Point(), finalSize);

            for(int i = 0; i < this._areas.Count; i++)
            {
                this._areas[i].Arrange(arrangeRect);
            }

			RectangleGeometry rg = new RectangleGeometry();
			rg.Rect = new Rect(new Point(), finalSize);
			this.Clip = rg;

            return finalSize;
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






			if (_isInitialized == false)
			{
				_isInitialized = true;
				((ICalendarItemArea)this).InitializeItems();
				this.InitializeSizingGroupPanel();
			}

            Size desiredSize = new Size();

			bool initializeDn;
			CalendarBase cal = CalendarUtilities.GetCalendar(this, out initializeDn);

            for (int i = 0, count = this._areas.Count; i < count; i++)
            {
                CalendarItemArea area = this._areas[i];

				// if the initialize flag was set above set the Calendar property on the child areas
				if (initializeDn)
				{
					CalendarBase.SetCalendar(area, cal);
					area.InvalidateMeasure();
				}

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

		#region RenderTransform (static PropertyPath properties)



#region Infragistics Source Cleanup (Region)


































































#endregion // Infragistics Source Cleanup (Region)


		#endregion //RenderTransform (static PropertyPath properties)

		#endregion //Properties

		#region Methods

		#region Internal Methods

		// JJD 3/29/11 - TFS69928 - Optimization
		#region LoadItemsLazily

		internal bool LoadItemsLazily(bool loadDays)
		{
			if (this._group == null || this._group.IsGroupForSizing)
				return false;

			CalendarBase cal = CalendarUtilities.GetCalendar(this);

			if (cal == null)
				return false;

			// get the index of the area that is not the current area
			int index = this._areaIndex == 0 ? 1 : 0;
			
			if (index >= this._areas.Count)
				this._areas.AddRange(new CalendarItemArea[1 + index - this._areas.Count]);

			CalendarItemArea area = this._areas[index];

			bool areaCreated = false;

			if (area == null)
			{
				area = this._areas[index] = new CalendarItemArea();
				((ISupportInitialize)area).BeginInit();

				CalendarBase.SetCalendar(area, cal);

				area.InitializeGroup(this._group);

				this.Children.Add(area);

				((ISupportInitialize)area).EndInit();

				// force hydration of the area's element tree
				// Note: this causes the calendar items to be generated
				area.ApplyTemplate();

				areaCreated = true;
			}

			bool wasWorkDone = area.LoadItemsLazily(loadDays);

			// when we are loading items we want to load them
			// for the current area as well
			if (loadDays == false)
			{
				if (this.CurrentArea.LoadItemsLazily(false))
					wasWorkDone = true;
			}

			// if we created the alternate area then set its Opacity to 0
			// so that it won't show in the UI until the user scrolls or zooms
			if (areaCreated)
			{
				area.Opacity = 0.0;

				// JJD 4/5/11 - TFS66907
				// Mark the area so it isn't visible for hit testing
				area.IsHitTestVisible = false;
			}

			return wasWorkDone;
		}

		#endregion //LoadItemsLazily

		#endregion //Internal Methods	
    
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

        #region AddOpacityAnimation
        private static void AddOpacityAnimation(Storyboard sb, Duration duration)
        {
            // this is used for the old area so that it starts as visible and ends as hidden
            ObjectAnimationUsingKeyFrames visa = new ObjectAnimationUsingKeyFrames();
			
			
            DiscreteObjectKeyFrame visStart = new DiscreteObjectKeyFrame();
			visStart.Value = 1d;
			visStart.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0d));
            DiscreteObjectKeyFrame visEnd = new DiscreteObjectKeyFrame();
			visEnd.Value = 0d;
			visEnd.KeyTime = KeyTime.FromTimeSpan(duration.TimeSpan);
            visa.KeyFrames.Add(visStart);
            visa.KeyFrames.Add(visEnd);

            Storyboard.SetTargetProperty(visa, new PropertyPath(FrameworkElement.OpacityProperty));



            visa.Duration = duration;
            sb.Children.Add(visa);
        }
        #endregion //AddOpacityAnimation

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

		#region CreateChildTranslatePath



#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

		#endregion //CreateChildTranslatePath

		#region CreateItemArea
		private CalendarItemArea CreateItemArea(int index, CalendarZoomMode? mode)
        {
            if (index >= this._areas.Count)
                this._areas.AddRange(new CalendarItemArea[1 + index - this._areas.Count]);

			if (this._areas[index] == null)
			{
				CalendarItemArea area = this._areas[index] = new CalendarItemArea();
				((ISupportInitialize)area).BeginInit();

				CalendarBase cal = CalendarUtilities.GetCalendar(this);

				if (cal != null)
					CalendarBase.SetCalendar(area, cal);

				if (null != mode)
					CalendarItemGroup.SetCurrentMode(area, mode.Value);
				else
				if ( index == 0 )
					CalendarItemGroup.SetCurrentMode(area, CalendarItemGroup.GetCurrentMode(this));

				area.InitializeGroup(this._group);

				this.Children.Add(area);

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

			//if (false == isNew)
			//    AddOpacityAnimation(sb, duration);

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
				AddOpacityAnimation(sb, duration);

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
                
                DoubleAnimation stax = new DoubleAnimation();
				stax.From = isNew ? scaleX : 1;
				stax.To = isNew ? 1 : scaleX;
				stax.Duration = duration;


                Storyboard.SetTargetProperty(stax, new PropertyPath("(0).(1)[0].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, ScaleTransform.ScaleXProperty));



                sb.Children.Add(stax);

                
                DoubleAnimation stay = new DoubleAnimation();
				stay.From = isNew ? scaleY : 1;
				stay.To = isNew ? 1 : scaleY;
				stay.Duration = duration;

                Storyboard.SetTargetProperty(stay, new PropertyPath("(0).(1)[0].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, ScaleTransform.ScaleYProperty));



                sb.Children.Add(stay);

                Point upperLeft = referenceItem.TransformToVisual(area).Transform(new Point());

                if (isNew == zoomOut) // zoomout & new or zoomin and old
                {
                    upperLeft.X *= -scaleX;
                    upperLeft.Y *= -scaleY;
                }

                // as it scales down we want to offset to ultimately center over the resulting item
                
                DoubleAnimation ttax = new DoubleAnimation();
				ttax.From = isNew ? upperLeft.X : 0;
				ttax.To = isNew ? 0 : upperLeft.X;
				ttax.Duration = duration;


                Storyboard.SetTargetProperty(ttax, new PropertyPath("(0).(1)[1].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, TranslateTransform.XProperty));



                sb.Children.Add(ttax);

                
                DoubleAnimation ttay = new DoubleAnimation();
				ttay.From = isNew ? upperLeft.Y : 0;
				ttay.To = isNew ? 0 : upperLeft.Y;
				ttay.Duration = duration;

                Storyboard.SetTargetProperty(ttay, new PropertyPath("(0).(1)[1].(2)", UIElement.RenderTransformProperty, TransformGroup.ChildrenProperty, TranslateTransform.YProperty));



                sb.Children.Add(ttay);
            }

            // and we need to fade it out/in
            AddFadeAnimation(duration, isNew, sb);

			//if (false == isNew)
			//    AddOpacityAnimation(sb, duration);

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

                if (group == PresentationUtilities.GetTemplatedParent( this ))
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
            if (_isInitialized == false)
                return;

            if (null != this._group && this._group.IsGroupForSizing)
            {
                int minMode = (int)this._group.CurrentMode;
                int maxMode = (int)CalendarZoomMode.Centuries;

                // let the first area always just pick up the min mode from the group
                this.CreateItemArea(0, null);

                int maxOffset = maxMode - minMode;

                for (int i = 1; i <= maxOffset; i++)
                {
                    CalendarZoomMode mode = (CalendarZoomMode)(i + minMode);
                    CalendarItemArea area = this.CreateItemArea(i, mode);
                    CalendarItemGroup.SetCurrentMode(area, mode);
                }

                for (int i = this._areas.Count - 1; i > maxOffset; i--)
                {
                    CalendarItemArea oldArea = this._areas[i];
                    this._areas.RemoveAt(i);

					this.Children.Remove(oldArea);
                 }
            }
        } 
        #endregion //InitializeSizingGroupPanel

		#region OnCurrentModeChanged
		internal void OnCurrentModeChanged(CalendarZoomMode newMode, CalendarZoomMode oldMode)
        {
            // AS 10/1/08 TFS8497
            // The sizing group will now be handled specially. It doesn't animate but it will use
            // the area collection to maintain an area for each mode to allow property sizing
            // calculations instead of relying on the size of one mode.
            //
            if (_group != null && _group.IsGroupForSizing)
            {
                this.InitializeSizingGroupPanel();
                return;
            }

            // since we have to set a local value for the current calendar mode of the areas
            // when the animation occurs we have to manually keep the current area's value in
            // sync with the panels. for some reason clearing the local value on the area 
            // does not cause the value to use that of the ancestor again.
            if (this._areas.Count > 1)
            {
                this.CurrentArea.SetValue(CalendarItemGroup.CurrentModePropertyKey, newMode);
            }
        }
        #endregion //OnCurrentModeChanged
		
		#region OnLoaded
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= new RoutedEventHandler(OnLoaded);

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

		}
		#endregion //OnLoaded

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
            if (_isInitialized == false)
                return;

            if (this.HasItemArea(this._areaIndex))
                ((ICalendarItemArea)this.CurrentArea).InitializeItems();
            else // just create it and it will initialize its own items when its created
                this.CreateItemArea(this._areaIndex, null);
        }

        void ICalendarItemArea.InitializeWeekNumbers()
        {
 			foreach (ICalendarItemArea area in _areas)
			{
				if (null != area)
					area.InitializeWeekNumbers();
			}
        }

		void ICalendarItemArea.InitializeDaysOfWeek()
		{
			foreach (ICalendarItemArea area in _areas)
			{
				if (null != area)
					area.InitializeDaysOfWeek();
			}
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
			switch (change)
			{
				// JJD 11/9/11 - TFS85695
				// When resources are being cahanged we need to loop over all of the item areas. This will
				// pick up any area that isn't active currently
				case CalendarItemChange.Resources:
					{
						foreach (CalendarItemArea area in this._areas)
							((ICalendarItemArea)area).ReinitializeItems(change);
					}
					break;
				default:
					((ICalendarItemArea)this.CurrentArea).ReinitializeItems(change);
					break;
			}
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
                this.OldArea.SetValue(CalendarItemGroup.CurrentModePropertyKey, CalendarItemGroup.GetCurrentMode(this));

                // i wanted to just clear the set value assuming that it would once again
                // inherit the value of the ancestor but that does not seem to be case. so
                // we have to set the value here and keep them in sync whenever the mode changes
                //this.CurrentArea.ClearValue(CalendarItemGroup.CurrentModePropertyKey);
                //Debug.Assert(CalendarItemGroup.GetCurrentMode(this) == CalendarItemGroup.GetCurrentMode(this.CurrentArea));
                this.CurrentArea.SetValue(CalendarItemGroup.CurrentModePropertyKey, CalendarItemGroup.GetCurrentMode(this));

                // we don't want to show the current area until the animation is performed
                //this.CurrentArea.Visibility = Visibility.Hidden;
                this.CurrentArea.Opacity = 0;
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
				DateRange? oldRange = oldArea.GroupRange;
				DateRange? newRange = currentArea.GroupRange;

				
				// If the group range wasn't initialized do it now
				if (newRange == null)
				{
					currentArea.VerifyItemsInitialized();
					newRange = currentArea.GroupRange;
				}


				Storyboard sbOld = null;
				Storyboard sbNew = null;

				CalendarBase cal = CalendarUtilities.GetCalendar(this);

				// AS 10/15/09 TFS23858
				//if (null != oldRange && null != newRange && oldRange != newRange && this.IsVisible)
				bool canAnimate = false;

				if (oldRange != null && newRange != null

 && this.IsVisible)



				{
					if (oldRange != newRange)
					{
						canAnimate = true;
					}
					else if (oldArea.CurrentMode != currentArea.CurrentMode &&
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

				// JJD 07/24/12 - TFS113395
				// Check to see if animations have been disabled temporarily
				//if (canAnimate)
				if (canAnimate && !cal.DisableAnimations)
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

							this.UpdateLayout();
							// see whether we are scroll right to left or left to right
							// based on the first dates of the old and current item area
							bool scrollLeft = oldRange.Value.End < newRange.Value.Start;
							Debug.Assert(scrollLeft || oldRange.Value.Start > newRange.Value.End);

							double offset = this.ActualWidth;

							sbOld = CreateScrollStoryboard(oldArea, scrollLeft, false, offset, duration);
							sbNew = CreateScrollStoryboard(currentArea, scrollLeft, true, offset, duration);

							// JJD 9/9/11 - TFS74024  
							// Call VerifySelectedDayStatesAsync to make sure the new areas days get their selected state
							// verified
							if (cal != null)
								cal.VerifySelectedDayStatesAsync();

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
				
				
				oldArea.Opacity = 0;
				oldArea.IsHitTestVisible = false;
				currentArea.Opacity = 1;
				currentArea.ClearValue(IsHitTestVisibleProperty);

				// store the storyboards so we can end them
				this._sbOldArea = sbOld;
				this._sbNewArea = sbNew;

				if (sbOld != null)
					Storyboard.SetTarget(sbOld, oldArea);

				if (sbNew != null)
					Storyboard.SetTarget(sbNew, currentArea);

				if (this._group != null)
				{
					// JJD 4/8/11 - TFS67003 
					// First clear the queue of any old pending animations
					this._group.ClearQueuedAnimations();

					this._group.EnqueueAnimation(sbOld, oldArea);
					this._group.EnqueueAnimation(sbNew, currentArea);
				}
				else
				{
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
        }

        CalendarItem ICalendarItemArea.GetItem(DateTime date)
        {
            return ((ICalendarItemArea)this.CurrentArea).GetItem(date);
        }

		CalendarWeekNumber ICalendarItemArea.GetWeekNumber(int weekNumber)
		{
            return ((ICalendarItemArea)this.CurrentArea).GetWeekNumber(weekNumber);
		}

        #endregion //ICalendarItemAreaPanel Members

		#region ICalendarElement Members

		void ICalendarElement.OnCalendarChanged(CalendarBase newValue, CalendarBase oldValue)
		{
			foreach (CalendarItemArea area in this._areas)
				CalendarBase.SetCalendar(area, newValue);
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