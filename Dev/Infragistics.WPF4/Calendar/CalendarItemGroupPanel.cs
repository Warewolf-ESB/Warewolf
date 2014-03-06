using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Threading;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// Custom element used to create and position <see cref="CalendarItemGroup"/> instances for a <see cref="CalendarBase"/>
    /// </summary>
    /// <remarks>
    /// <p class="body">The CalendarItemGroupPanel is designed to be used within the template of a <see cref="CalendarBase"/>. When 
    /// used in the CalendarBase template, this class will automatically generate and arrange <see cref="CalendarItemGroup"/> instances 
    /// based on the <see cref="CalendarBase.Dimensions"/>. When the <see cref="CalendarBase.AutoAdjustDimensions"/> is 
    /// true, the panel will generate more CalendarItemGroups if they can fit up to the <see cref="MaxGroups"/>, which defaults to 12. 
    /// You may also retemplate the CalendarBase such that it directly contains CalendarItemGroup instances and set their 
    /// <see cref="CalendarItemGroup.ReferenceGroupOffset"/>.</p>
    /// <p class="body">By default, the <see cref="GroupWidth"/> and <see cref="GroupHeight"/> properties are set to double.NaN. When either 
    /// is left set to the default, the panel will calculate the size required for a group to display its contents and arrange all the groups 
    /// using that size.</p>
    /// </remarks>
    /// <seealso cref="CalendarBase"/>
    /// <seealso cref="CalendarItemGroup"/>
    /// <seealso cref="CalendarItemGroup.ReferenceGroupOffset"/>
    /// <seealso cref="CalendarItemGroupPanel.MaxGroups"/>
    //[System.ComponentModel.ToolboxItem(false)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class CalendarItemGroupPanel : Panel, ICalendarElement
    {
        #region Member Variables

        private List<CalendarItemGroup> _groups;
        private CalendarItemGroup _itemGroupForSizing;
        private CalendarItemGroup _groupBeingMeasured;

		// JJD 3/29/11 - TFS69928 - Optimization
		private LazyLoader _loader;
		private const int MinGroupLoadThreshold = 4;

        private const string SizingGroupName = "GroupForSizing";

        #endregion //Member Variables

        #region Constructor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static CalendarItemGroupPanel()
        {

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLanguageChanged)));

        }

        /// <summary>
        /// Initializes a new <see cref="CalendarItemGroupPanel"/>
        /// </summary>
        public CalendarItemGroupPanel()
        {
            this._groups = new List<CalendarItemGroup>();
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
            int currentGroupCount = this._groups.Count;

            // AS 10/1/08 TFS8497
            // We only need the sizing group if we have not been given an explicit width/height
            // for the groups.
            //
            //// ensure that the group for sizing has been arranged
            //this.ItemGroupForSizing.Arrange(new Rect(this.ItemGroupForSizing.DesiredSize));
            double groupWidth = this.GroupWidth;
            double groupHeight = this.GroupHeight;

            bool needsWidth = double.IsNaN(groupWidth);
            bool needsHeight = double.IsNaN(groupHeight);

            if (needsHeight || needsWidth)
            {
                // ensure that the group for sizing has been arranged
                Debug.Assert(this._itemGroupForSizing != null, "The group should have been created in the measure");

                CalendarItemGroup sizingGroup = this.ItemGroupForSizing;
                sizingGroup.Arrange(new Rect(new Point(), sizingGroup.DesiredSize));

                if (needsWidth)
                    groupWidth = sizingGroup.ActualWidth;

                if (needsHeight)
                    groupHeight = sizingGroup.ActualHeight;
            }


            // figure out how many we need
            CalendarDimensions dims = this.Dimensions;
            // AS 10/1/08 TFS8497
            //Size itemSize = new Size(this.ItemGroupForSizing.ActualWidth, this.ItemGroupForSizing.ActualHeight);
            Size itemSize = new Size(groupWidth, groupHeight);

            int requiredGroups = dims.Columns * dims.Rows;

            if (this.AutoAdjustDimensions)
            {
                AdjustDimensions(ref finalSize, ref dims, ref itemSize, ref requiredGroups);
            }

			CalendarBase cal = CalendarUtilities.GetCalendar(this);

            #region Add/Remove Groups

			// create any needed new groups
            for (int i = currentGroupCount; i < requiredGroups; i++)
            {
                CalendarItemGroup group = new CalendarItemGroup();

                // AS 10/1/08 TFS8497
                // Make all the changes to the group within begininit/endinit
                //
                ((ISupportInitialize)group).BeginInit();
                group.ReferenceGroupOffset = i;

				CalendarBase.SetCalendar(group, cal);

                this._groups.Add(group);

				this.Children.Add(group);
            }

            // remove any unneeded groups
            for (int i = currentGroupCount; i > requiredGroups; i--)
            {
                CalendarItemGroup group = this._groups[i - 1];
                this._groups.RemoveAt(i - 1);

				this.Children.Remove(group);
				
				group.ClearValue(CalendarBase.CalendarPropertyKey);
			} 
            #endregion //Add/Remove Groups

            bool showLeadingTrailing = this.ShowLeadingAndTrailing;
            HorizontalAlignment hAlign = this.HorizontalContentAlignment;
            VerticalAlignment vAlign = this.VerticalContentAlignment;

            double xOffset, yOffset;

            CalculateOffset(ref finalSize, ref dims, ref itemSize, hAlign, vAlign, out xOffset, out yOffset); 

            object scrollButtonVisOn = this.GetValue(CalendarItemGroupPanel.ScrollButtonVisibilityProperty);


 

            object scrollButtonVisOff = KnownBoxes.VisibilityCollapsedBox;

			// JJD 3/26/12 - TFS101638
			// Get the allowable date range, reference date and current oom mode
			CalendarManager calmgr = cal.CalendarManager;
			CalendarZoomMode zoomMode = cal.CurrentMode;
			DateTime minDate = cal.MinDateResolved;
			DateTime maxDate = cal.MaxDateResolved;
			DateTime? refDate = cal.ReferenceDate;

			if (refDate == null)
				refDate = DateTime.Today;

            // measure/arrange everyone
            for (int i = 0, last = this._groups.Count - 1; i <= last; i++)
            {
                CalendarItemGroup group = this._groups[i];

				CalendarBase.SetCalendar(group, cal);

                this._groupBeingMeasured = group;

				// JJD 3/26/12 - TFS101638
				// the the offset for the group 
				int groupOffset = group.ReferenceGroupOffset;

				bool isGroupOutOfDateRange = true;
				bool isMinDateBeforeGroup = false;
				bool isMaxDateAfterGroup = false;

				DateTime? groupStart = calmgr.TryAddGroupOffset(refDate.Value, groupOffset, zoomMode, true);
				DateTime? groupEnd = null;

				if (groupStart.HasValue)
				{
					groupEnd = calmgr.GetGroupEndDate(groupStart.Value, zoomMode);

					// JJD 3/26/12 - TFS101638
					// determine if the group has any dates within the allowable date range

					// JJD 06/05/12 - TFS113397
					// If the first or last date is == to the max or min date then include the group
					//isGroupOutOfDateRange = groupStart.Value >= maxDate || groupEnd.Value <= minDate;
					isGroupOutOfDateRange = groupStart.Value > maxDate || groupEnd.Value < minDate;

					// JJD 3/26/12 - TFS101638
					// determine if there are any dates following this group that are
					// in the allowable date range
					isMaxDateAfterGroup = maxDate > groupEnd.Value;

					// JJD 3/26/12 - TFS101638
					// determine if there are any dates prceding this group that are
					// in the allowable date range
					isMinDateBeforeGroup = minDate < groupStart.Value;
				}

                // initialize leading/trailing
                group.SetValue(CalendarItemGroup.ShowLeadingDatesProperty, KnownBoxes.FromValue(showLeadingTrailing && i == 0));
                group.SetValue(CalendarItemGroup.ShowTrailingDatesProperty, KnownBoxes.FromValue(showLeadingTrailing && i == last));


                // initialize scroll buttons
				// JJD 3/26/12 - TFS101638
				// Only show the scroll buttons if there are more dates in the allowable date range that can be scrolled to.
				//group.SetValue(CalendarItemGroup.ScrollPreviousButtonVisibilityProperty, i == 0 ? scrollButtonVisOn : scrollButtonVisOff);
				//group.SetValue(CalendarItemGroup.ScrollNextButtonVisibilityProperty, i == dims.Columns - 1 ? scrollButtonVisOn : scrollButtonVisOff);
				group.SetValue(CalendarItemGroup.ScrollPreviousButtonVisibilityProperty, isMinDateBeforeGroup && i == 0 ? scrollButtonVisOn : scrollButtonVisOff);
				group.SetValue(CalendarItemGroup.ScrollNextButtonVisibilityProperty, isMaxDateAfterGroup && i == dims.Columns - 1 ? scrollButtonVisOn : scrollButtonVisOff);

                // we wait until here to add the group to the visual/logical tree
                // to minimize the number of times that the items will be generated
                if (i >= currentGroupCount)
                {
                    // AS 10/1/08 TFS8497
                    // Make all the changes to the group within begininit/endinit
                    //
                    ((ISupportInitialize)group).EndInit();
                }

                group.Measure(itemSize);

                #region Arrange

                int row = i / dims.Columns;
                int col = i % dims.Columns;

                Rect itemRect = new Rect(col * itemSize.Width, row * itemSize.Height, itemSize.Width, itemSize.Height);

                switch (hAlign)
                {
                    case HorizontalAlignment.Left:
                        break;
                    case HorizontalAlignment.Center:
                    case HorizontalAlignment.Right:
                        itemRect.X += xOffset;
                        break;
                    case HorizontalAlignment.Stretch:
                        // AS 8/14/08
                        //itemRect.X += xOffset * (col * 2 + 1);
                        itemRect.X += xOffset * (col + 1);
                        break;
                }

                switch (vAlign)
                {
                    case VerticalAlignment.Top:
                        break;
                    case VerticalAlignment.Center:
                    case VerticalAlignment.Bottom:
                        itemRect.Y += yOffset;
                        break;
                    case VerticalAlignment.Stretch:
                        // AS 8/14/08
                        //itemRect.Y += yOffset * (row * 2 + 1);
                        itemRect.Y += yOffset * (row + 1);
                        break;
                }

				// JJD 3/26/12 - TFS101638
				// If the group is outside the allowable date range then arrange it out of view
				if (isGroupOutOfDateRange)
				{
					itemRect.X += 10000;
					itemRect.Y += 10000;
				}
                group.Arrange(itemRect);

                #endregion //Arrange

                this._groupBeingMeasured = null;
            }

            // if we added/removed groups then coerce the reference date
            // so we don't have empty groups. note, we need to coerce when
            // removing as well since the reference date may be the result
            // of coercing the referencedate to a lower value as we added
            // groups
            if (requiredGroups != currentGroupCount)
            {
                if (null != cal)
                    cal.OnAutoGeneratedGroupsChanged();
            }

            Size actualFinalSize = new Size(itemSize.Width * dims.Columns, itemSize.Height * dims.Rows);

            if (finalSize.Width < actualFinalSize.Width)
                finalSize.Width = actualFinalSize.Width;
            if (finalSize.Height < actualFinalSize.Height)
                finalSize.Height = actualFinalSize.Height;

			// JJD 3/29/11 - TFS69928 - Optimization
			// If we have more than 3 groups start a timer to lazy load CalendarItems for use
			// in the alternate item areas used during a scroll or zoom
			if (this._groups.Count >= MinGroupLoadThreshold)
			{
				EventHandler handler = new EventHandler(OnLayoutUpdated);
				this.LayoutUpdated -= handler;
				this.LayoutUpdated += handler;
			}
			else
				this.StopLoader();

            return finalSize;
        }

        #endregion //ArrangeOverride


        #region OnChildDesiredSizeChanged
        /// <summary>
        /// Invoked when the desired size of a child has been changed.
        /// </summary>
        /// <param name="child">The child whose size is being changed.</param>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            if (child == this._groupBeingMeasured)
                return;

            // we really don't need to invalidate our measure/arrange
            // for a child group that is not the group we use for 
            // measuring the sizing.
            if (child != this._itemGroupForSizing)
                return;

            base.OnChildDesiredSizeChanged(child);
        }
        #endregion //OnChildDesiredSizeChanged    }

        #region MeasureOverride
        /// <summary>
        /// Invoked to measure the element and its children.
        /// </summary>
        /// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // AS 10/1/08 TFS8497
            // Refactored this block since we only need the sizing group
            // if we have not been given explicit width/heights for the groups.
            //
            #region Refactored
            
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

            #endregion //Refactored
            double groupWidth = this.GroupWidth;
            double groupHeight = this.GroupHeight;

            bool needsWidth = double.IsNaN(groupWidth);
            bool needsHeight = double.IsNaN(groupHeight);

            if (needsHeight || needsWidth)
            {
                // measure the ribbon group that we use for resizing so we can control the height
                // of the content area of the ribbon tabs
                CalendarItemGroup group = this.ItemGroupForSizing;
                Size groupSize = new Size();

                if (null != group)
                {
                    // measure the group for sizing with infinity to find out
                    // how large it wants to be
                    group.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    // use its desired size to calculate how large the panel should be
                    groupSize = group.DesiredSize;
                }

                if (needsWidth)
                    groupWidth = groupSize.Width;

                if (needsHeight)
                    groupHeight = groupSize.Height;
            }
            else
            {
                // make sure we remove the sizing group
                this.ReleaseSizingGroup();
            }

            CalendarDimensions dimensions = this.Dimensions;

            // always measure using the "preferred" dimensions
            Size desiredSize = new Size(dimensions.Columns * groupWidth, dimensions.Rows * groupHeight);

            
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


            // if our desired size is larger we need to return the available
            // size or else we will get handed our returned desired size
            // in the arrange. this is especially important when auto adjusting
            // the size so we get the smaller actual size so we know to remove
            // some group rows/columns but it also impacts non-autoadjust
            // because when aligned center/right, we want to center within the
            // actual available and not within the space that we would have 
            // liked to have had
            if (desiredSize.Width > availableSize.Width)
                desiredSize.Width = availableSize.Width;

            if (desiredSize.Height > availableSize.Height)
                desiredSize.Height = availableSize.Height;

            return desiredSize;
        }
        #endregion //MeasureOverride

        #endregion //Base class overrides

        #region Properties

        #region Public

		#region AutoAdjustDimensions

		/// <summary>
		/// Identifies the <see cref="AutoAdjustDimensions"/> dependency property
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never)]
		public static readonly DependencyProperty AutoAdjustDimensionsProperty = DependencyPropertyUtilities.Register("AutoAdjustDimensions",
			typeof(bool), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnAutoAdjustDimensionsChanged))
			);

		private static void OnAutoAdjustDimensionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupPanel instance = (CalendarItemGroupPanel)d;

			instance.InvalidateArrange();
		}

		/// <summary>
		/// For internal use 
		/// </summary>
		/// <seealso cref="AutoAdjustDimensionsProperty"/>
		[EditorBrowsable( EditorBrowsableState.Never)]
		public bool AutoAdjustDimensions
		{
			get
			{
				return (bool)this.GetValue(CalendarItemGroupPanel.AutoAdjustDimensionsProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroupPanel.AutoAdjustDimensionsProperty, value);
			}
		}

		#endregion //AutoAdjustDimensions

		#region Dimensions

		/// <summary>
		/// For internal use 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly DependencyProperty DimensionsProperty = DependencyPropertyUtilities.Register(" Dimensions",
			typeof(CalendarDimensions), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(new CalendarDimensions(1,1), new PropertyChangedCallback(OnDimensionsChanged))
			);

		private static void OnDimensionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupPanel instance = (CalendarItemGroupPanel)d;

			instance.InvalidateMeasure();
			instance.InvalidateArrange();
		}

		/// <summary>
		/// For internal use 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public CalendarDimensions Dimensions
		{
			get
			{
				return (CalendarDimensions)this.GetValue(CalendarItemGroupPanel.DimensionsProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroupPanel.DimensionsProperty, value);
			}
		}

		#endregion // Dimensions

		#region HorizontalContentAlignment

		/// <summary>
		/// Identifies the <see cref="HorizontalContentAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalContentAlignmentProperty = DependencyPropertyUtilities.Register("HorizontalContentAlignment",
			typeof(HorizontalAlignment), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.HorizontalAlignmentStretchBox, new PropertyChangedCallback(OnHorizontalContentAlignmentChanged))
			);

		private static void OnHorizontalContentAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupPanel instance = (CalendarItemGroupPanel)d;
			instance.InvalidateArrange();
		}

        /// <summary>
        /// Returns or sets the alignment of the items within the element.
        /// </summary>
        /// <seealso cref="HorizontalContentAlignmentProperty"/>
        /// <seealso cref="VerticalContentAlignment"/>
        //[Description("Returns or sets the horizontal alignment of the items within the element")]
        //[Category("MonthCalendar Properties")] // Layout
        [Bindable(true)]
        public HorizontalAlignment HorizontalContentAlignment
        {
            get
            {
                return (HorizontalAlignment)this.GetValue(CalendarItemGroupPanel.HorizontalContentAlignmentProperty);
            }
            set
            {
                this.SetValue(CalendarItemGroupPanel.HorizontalContentAlignmentProperty, value);
            }
        }

        #endregion //HorizontalContentAlignment

		#region GroupHeight

		/// <summary>
		/// Identifies the <see cref="GroupHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupHeightProperty = DependencyPropertyUtilities.Register("GroupHeight",
			typeof(double), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnGroupHeightWidthChanged))
			);

		private static void OnGroupHeightWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupPanel instance = (CalendarItemGroupPanel)d;

			double newVal = (double)e.NewValue;

			if (!double.IsNaN(newVal))
			{
				CoreUtilities.ValidateIsNotInfinity(newVal);
				CoreUtilities.ValidateIsNotNegative(newVal, DependencyPropertyUtilities.GetName(e.Property));
			}

			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the height of the CalendarItemGroups within the panel.
		/// </summary>
		/// <remarks>
		/// <p class="body">The GroupHeight and <see cref="GroupWidth"/> are used to control the size 
		/// of the <see cref="CalendarItemGroup"/> instances that the panel creates. Both of these default 
		/// to double.NaN. When either is left set to NaN, the panel will use a custom hidden 
		/// CalendarItemGroup to calculate the size required for the groups to replace the NaN value.</p>
		/// </remarks>
		/// <seealso cref="GroupHeightProperty"/>
		/// <seealso cref="GroupWidth"/>
		//[Description("Returns or sets the height of the CalendarItemGroups within the panel.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
		public double GroupHeight
		{
			get
			{
				return (double)this.GetValue(CalendarItemGroupPanel.GroupHeightProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroupPanel.GroupHeightProperty, value);
			}
		}

        #endregion //GroupHeight

		#region GroupWidth

		/// <summary>
		/// Identifies the <see cref="GroupWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupWidthProperty = DependencyPropertyUtilities.Register("GroupWidth",
			typeof(double), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnGroupHeightWidthChanged))
			);

		/// <summary>
		/// Returns or sets the width of the CalendarItemGroups within the panel.
		/// </summary>
		/// <remarks>
		/// <p class="body">The GroupWidth and <see cref="GroupHeight"/> are used to control the size 
		/// of the <see cref="CalendarItemGroup"/> instances that the panel creates. Both of these default 
		/// to double.NaN. When either is left set to NaN, the panel will use a custom hidden 
		/// CalendarItemGroup to calculate the size required for the groups to replace the NaN value.</p>
		/// </remarks>
		/// <seealso cref="GroupWidthProperty"/>
		/// <seealso cref="GroupHeight"/>
		//[Description("Returns or sets the width of the CalendarItemGroups within the panel.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
		public double GroupWidth
		{
			get
			{
				return (double)this.GetValue(CalendarItemGroupPanel.GroupWidthProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroupPanel.GroupWidthProperty, value);
			}
		}

        #endregion //GroupWidth

		#region LeadingAndTrailingDatesVisibility

		/// <summary>
		/// For internal use 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly DependencyProperty LeadingAndTrailingDatesVisibilityProperty = DependencyPropertyUtilities.Register("LeadingAndTrailingDatesVisibility",
			typeof(Visibility), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnLeadingAndTrailingDatesVisibilityChanged))
			);

		private static void OnLeadingAndTrailingDatesVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupPanel p = (CalendarItemGroupPanel)d;

			bool show = CalendarUtilities.ValidateNonHiddenVisibility(e);

			p.InvalidateMeasure();

			// update the groups?
			if (0 < p._groups.Count)
			{
				p._groups[0].ShowLeadingDates = show;
				p._groups[p._groups.Count - 1].ShowTrailingDates = show;
			}

		}

		/// <summary>
		/// For internal use 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Visibility LeadingAndTrailingDatesVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CalendarItemGroupPanel.LeadingAndTrailingDatesVisibilityProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroupPanel.LeadingAndTrailingDatesVisibilityProperty, value);
			}
		}

        private bool ShowLeadingAndTrailing
        {
            get { return Visibility.Visible == (Visibility)this.GetValue(LeadingAndTrailingDatesVisibilityProperty); }
        }
        #endregion //LeadingAndTrailingDatesVisibilityProperty

		#region MaxGroups

		/// <summary>
		/// Identifies the <see cref="MaxGroups"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxGroupsProperty = DependencyPropertyUtilities.Register("MaxGroups",
			typeof(int), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(12, new PropertyChangedCallback(OnMaxGroupsChanged))
			);

		private static void OnMaxGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupPanel instance = (CalendarItemGroupPanel)d;
			
			CoreUtilities.ValidateIsNotNegative((int)e.NewValue, DependencyPropertyUtilities.GetName(e.Property));

		}

		/// <summary>
		/// Returns or sets the maximum number of groups that the element may generate.
		/// </summary>
		/// <remarks>
		/// <p class="note">The maximum of this value with the rows * columns of the <see cref="CalendarBase.Dimensions"/> 
		/// will be used to determine the actual maximum value allowed.</p>
		/// </remarks>
		/// <seealso cref="MaxGroupsProperty"/>
		/// <seealso cref="CalendarBase.Dimensions"/>
		/// <seealso cref="CalendarBase.AutoAdjustDimensions"/>
		//[Description("Returns or sets the maximum number of groups that the element may generate.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
		public int MaxGroups
		{
			get
			{
				return (int)this.GetValue(CalendarItemGroupPanel.MaxGroupsProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroupPanel.MaxGroupsProperty, value);
			}
		}

        #endregion //MaxGroups

		#region ScrollButtonVisibility

		/// <summary>
		/// For internal use 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly DependencyProperty ScrollButtonVisibilityProperty = DependencyPropertyUtilities.Register("ScrollButtonVisibility",
			typeof(Visibility), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnScrollButtonVisibilityChanged))
			);

		private static void OnScrollButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupPanel instance = (CalendarItemGroupPanel)d;
			instance.InvalidateArrange();
		}

		/// <summary>
		/// For internal use 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Visibility ScrollButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CalendarItemGroupPanel.ScrollButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroupPanel.ScrollButtonVisibilityProperty, value);
			}
		}

		#endregion //ScrollButtonVisibility

		#region VerticalContentAlignment

		/// <summary>
		/// Identifies the <see cref="VerticalContentAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalContentAlignmentProperty = DependencyPropertyUtilities.Register("VerticalContentAlignment",
			typeof(VerticalAlignment), typeof(CalendarItemGroupPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VerticalAlignmentStretchBox, new PropertyChangedCallback(OnVerticalContentAlignmentChanged))
			);

		private static void OnVerticalContentAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroupPanel instance = (CalendarItemGroupPanel)d;

			instance.InvalidateArrange();
		}

        /// <summary>
        /// Returns or sets the vertical alignment of the items within the element.
        /// </summary>
        /// <seealso cref="VerticalContentAlignmentProperty"/>
        /// <seealso cref="HorizontalContentAlignment"/>
        //[Description("Returns or sets the vertical alignment of the items within the element")]
        //[Category("MonthCalendar Properties")] // Layout
        [Bindable(true)]
        public VerticalAlignment VerticalContentAlignment
        {
            get
            {
                return (VerticalAlignment)this.GetValue(CalendarItemGroupPanel.VerticalContentAlignmentProperty);
            }
            set
            {
                this.SetValue(CalendarItemGroupPanel.VerticalContentAlignmentProperty, value);
            }
        }

        #endregion //VerticalContentAlignment

        #endregion //Public

        #region Private

        #region CalendarManager

        internal CalendarManager CalendarManager
        {
            get
            {
                CalendarBase cal = CalendarUtilities.GetCalendar(this);

                return null != cal ? cal.CalendarManager : CalendarManager.CurrentCulture;
            }
        }
        #endregion //CalendarManager

        #region ItemGroupForSizing
        private CalendarItemGroup ItemGroupForSizing
        {
            get
            {
                if (this._itemGroupForSizing == null)
                {
                    // AS 10/1/08 TFS8497
                    Debug.Assert(double.IsNaN(this.GroupWidth) || double.IsNaN(this.GroupHeight));

                    this._itemGroupForSizing = new CalendarItemGroup(null, true);
                    ((ISupportInitialize)this._itemGroupForSizing).BeginInit();

					// i was going to set the name but Silverlight throws a value not within expected range exception
					this._itemGroupForSizing.Tag = SizingGroupName;

					CalendarBase cal = CalendarUtilities.GetCalendar(this);

					if ( cal != null )
						CalendarBase.SetCalendar(this._itemGroupForSizing, cal);

					((ISupportInitialize)this._itemGroupForSizing).EndInit();

					this._itemGroupForSizing.SizeChanged += new SizeChangedEventHandler(OnItemGroupForSizing_SizeChanged);
                }

                return this._itemGroupForSizing;
            }
        }

        #endregion //ItemGroupForSizing

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Private

        #region AdjustDimensions
        private void AdjustDimensions(ref Size finalSize, ref CalendarDimensions dims, ref Size itemSize, ref int requiredGroups)
        {
            CalendarDimensions originalDims = dims;
            int maxGroupsResolved = this.MaxGroups > 0
                ? Math.Max(originalDims.Columns * originalDims.Rows, this.MaxGroups)
                : 0;

            // AS 10/7/08
            // Found this while debugging TFS8735. Basically the original dimensions
            // should be the minimum.
            //
            //dims.Columns = Math.Max(1, (int)Math.Round(finalSize.Width / itemSize.Width, 8));
            //dims.Rows = Math.Max(1, (int)Math.Round(finalSize.Height / itemSize.Height, 8));
            dims.Columns = Math.Max(1, Math.Max(originalDims.Columns, (int)Math.Round(finalSize.Width / itemSize.Width, 8)));
            dims.Rows = Math.Max(1, Math.Max(originalDims.Rows, (int)Math.Round(finalSize.Height / itemSize.Height, 8)));

            // update this based on the calculated
            requiredGroups = dims.Columns * dims.Rows;

            // if we have a maximum and we have more then we need to start removing
            // rows/columns
            if (maxGroupsResolved > 0 && requiredGroups > maxGroupsResolved)
            {
                int excess = requiredGroups - maxGroupsResolved;

                // make the block square and reduce rows/columns as allowed
                while (excess > 0)
                {
                    // AS 10/7/08 TFS8735
                    int originalExcess = excess;

                    // if we have more rows than columns and we're above the preferred rows
                    // AS 10/7/08 TFS8735
                    // We also want to reduce the rows if the columns are at the minimum.
                    //
                    //if (dims.Rows >= dims.Columns && dims.Rows > originalDims.Rows)
                    // AS 11/4/08 TFS9579
                    // We don't want to remove a row if that would bring us below the maximum.
                    //
                    //if ((dims.Columns == originalDims.Columns || dims.Rows >= dims.Columns) && dims.Rows > originalDims.Rows)
                    if ((dims.Columns == originalDims.Columns || dims.Rows >= dims.Columns) && dims.Rows > originalDims.Rows && excess >= dims.Columns)
                    {
                        dims.Rows--;
                        excess -= Math.Min(excess, dims.Columns);
                    }

                    // AS 10/7/08 TFS8735
                    // We also want to reduce the columns if the rows are at the minimum.
                    //
                    //if (dims.Columns >= dims.Rows && dims.Columns > originalDims.Columns)
                    // AS 11/4/08 TFS9579
                    // We don't want to remove a column if that would bring us below the maximum.
                    //
                    //if ((dims.Rows == originalDims.Rows || dims.Columns >= dims.Rows) && dims.Columns > originalDims.Columns)
                    if ((dims.Rows == originalDims.Rows || dims.Columns >= dims.Rows) && dims.Columns > originalDims.Columns && 
                        excess >= dims.Rows)
                    {
                        dims.Columns--;
                        excess -= Math.Min(excess, dims.Rows);
                    }

                    // AS 11/4/08 TFS9579
                    //Debug.Assert(originalExcess != excess);

                    if (originalExcess == excess)
                        break;
                }

                // restrict the number of groups we need
                requiredGroups = maxGroupsResolved;
            }
        }
        #endregion //AdjustDimensions

        #region CalculateOffset
        private static void CalculateOffset(ref Size finalSize, ref CalendarDimensions dims, ref Size itemSize, HorizontalAlignment hAlign, VerticalAlignment vAlign, out double xOffset, out double yOffset)
        {
            if (dims.Columns > 0)
            {
                switch (hAlign)
                {
                    default:
                    case HorizontalAlignment.Center:
                        // all items gathered in the center
                        xOffset = (finalSize.Width - (itemSize.Width * dims.Columns)) / 2;
                        break;
                    case HorizontalAlignment.Left:
                        // all items arranged on the left
                        xOffset = 0;
                        break;
                    case HorizontalAlignment.Right:
                        // all items right aligned
                        xOffset = finalSize.Width - (itemSize.Width * dims.Columns);
                        break;
                    case HorizontalAlignment.Stretch:
                        // AS 8/14/08
                        // Instead of following what the grid/uniform grid would do which
                        // ends up with twice as much space between items as you have before
                        // the first and after the last, we're going to evenly distribute the
                        // space.
                        //
                        //xOffset = ((finalSize.Width / dims.Columns) - itemSize.Width) / 2;
                        xOffset = (finalSize.Width - (dims.Columns * itemSize.Width)) / (dims.Columns + 1);
                        break;
                }
            }
            else
                xOffset = 0;

            if (dims.Rows > 0)
            {
                switch (vAlign)
                {
                    default:
                    case VerticalAlignment.Center:
                        // all items gathered in the center
                        yOffset = (finalSize.Height - (itemSize.Height * dims.Rows)) / 2;
                        break;
                    case VerticalAlignment.Top:
                        // all items arranged on the top
                        yOffset = 0;
                        break;
                    case VerticalAlignment.Bottom:
                        // all items bottom aligned
                        yOffset = finalSize.Height - (itemSize.Height * dims.Rows);
                        break;
                    case VerticalAlignment.Stretch:
                        // AS 8/14/08
                        // Instead of following what the grid/uniform grid would do which
                        // ends up with twice as much space between items as you have before
                        // the first and after the last, we're going to evenly distribute the
                        // space.
                        //
                        //yOffset = ((finalSize.Height / dims.Rows) - itemSize.Height) / 2;
                        yOffset = (finalSize.Height - (dims.Rows * itemSize.Height)) / (dims.Rows + 1);
                        break;
                }
            }
            else
                yOffset = 0;
        }
        #endregion //CalculateOffset

		// JJD 3/29/11 - TFS69928 - Optimization
		#region OnLayoutUpdated

		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

			// JJD 07/19/12 - TFS108812
			// We only want to start the loader if we are hooked up to a Calendar
			if (CalendarUtilities.GetCalendar(this) != null)
			{
				if (this._groups.Count >= MinGroupLoadThreshold)
					this.StartLoader();
				else
					this.StopLoader();
			}
		}

		#endregion //OnLayoutUpdated	
    
		#region OnItemGroupForSizing_SizeChanged

		private void OnItemGroupForSizing_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.InvalidateMeasure();
		}

		#endregion //OnItemGroupForSizing_SizeChanged	
    

        #region OnLanguageChanged
        private static void OnLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // there seems to be a bug in the wpf framework where a binding is
            // still passing in the original culture even after the language
            // has changed. i saw this when the title of the group was still
            // showing using english (because that's the culture wpf was
            // passing to the converter) even after the language had changed
            CalendarItemGroupPanel c = d as CalendarItemGroupPanel;

            for (int i = c._groups.Count - 1; i >= 0; i--)
            {
                CalendarItemGroup group = c._groups[i];
                c._groups.RemoveAt(i);
				c.Children.Remove(group);
				group.ClearValue(CalendarBase.CalendarPropertyKey);

            }

			c.ReleaseSizingGroup();
 
        } 
        #endregion //OnLanguageChanged


		// AS 10/1/08 TFS8497
		#region ReleaseSizingGroup
		private void ReleaseSizingGroup()
        {
            if (null != this._itemGroupForSizing)
            {
                CalendarItemGroup oldGroup = this._itemGroupForSizing;
                this._itemGroupForSizing = null;
				
				oldGroup.ClearValue(CalendarBase.CalendarPropertyKey);
				oldGroup.SizeChanged -= new SizeChangedEventHandler(this.OnItemGroupForSizing_SizeChanged);
			}
        }
        #endregion //ReleaseSizingGroup

		// JJD 3/29/11 - TFS69928 - Optimization
		#region StartLoader

		private void StartLoader()
		{
			if (this._loader == null)
				this._loader = new LazyLoader(this);
			else
				this._loader.Restart();
		}

		#endregion //StartLoader

		// JJD 3/29/11 - TFS69928 - Optimization
		#region StopLoader

		private void StopLoader()
		{

			if (this._loader != null)
			{
				this._loader.Stop();
				this._loader = null;
			}
		}

		#endregion //StopLoader

		#endregion //Private

		#endregion //Methods

		#region ICalendarElement Members

		void ICalendarElement.OnCalendarChanged(CalendarBase newValue, CalendarBase oldValue)
		{
			if (null != newValue)
			{
				newValue.BindCalendarItemGroupPanel(this);
			}
			else
			{
				// JJD 07/19/12 - TFS108812
				// since the calendar ref is being nulled out stop the loader and
				// release the sizing group so we don't risk rooting any aold elements
				this.StopLoader();
				this.ReleaseSizingGroup();
			}

			if (this._itemGroupForSizing != null)
			{
				CalendarBase.SetCalendar(_itemGroupForSizing, newValue);
				_itemGroupForSizing.InvalidateMeasure();
				
				this.InvalidateMeasure();
			}

			foreach ( CalendarItemGroup group in this._groups )
				CalendarBase.SetCalendar(group, newValue);

		}

		#endregion
		
		// JJD 3/29/11 - TFS69928 - Optimization
		#region LazyLoader private class

		private class LazyLoader
		{
			#region Private Members

			private CalendarItemGroupPanel _panel;
			private DispatcherTimer _timer;
			private DateTime _startTime;
			private int _dayPassGroupIndex;
			private int _itemPassGroupIndex;
			private const int IntervalMSInitial = 200;
			private const int IntervalMS = 80;
			private const int MaxWorkDuration = 30;

			#endregion //Private Members

			#region Constructor

			internal LazyLoader(CalendarItemGroupPanel panel)
			{
				_panel = panel;


				_timer = new DispatcherTimer(DispatcherPriority.Background);




				_timer.Interval = TimeSpan.FromMilliseconds(IntervalMSInitial);
				_timer.Tick += new EventHandler(OnTimerTick);

				_timer.Start();
			}

			#endregion //Constructor

			#region Methods

			#region Private Methods

			#region LoadItemsLazily

			private bool LoadItemsLazily(bool loadDays)
			{
				int i = loadDays ? _dayPassGroupIndex : _itemPassGroupIndex;

				DateTime beginPass = DateTime.UtcNow;
				TimeSpan maxTimeAllowed = TimeSpan.FromMilliseconds(MaxWorkDuration);

				bool wasWorkDone = false;

				for (int count = _panel._groups.Count; i < count; i++)
				{
					if (loadDays)
						_dayPassGroupIndex = i + 1;
					else
						_itemPassGroupIndex = i + 1;

					if (_panel._groups[i].LoadItemsLazily(loadDays))
					{
						wasWorkDone = true;

						TimeSpan elapsed = DateTime.UtcNow.Subtract(beginPass);

						if (elapsed >= maxTimeAllowed)
							break;
					}
				}

				Debug.WriteLine(string.Format("LoadItemsLazily next {0} group index : {1}", loadDays ? "day" : "item", loadDays ? _dayPassGroupIndex : _itemPassGroupIndex ));

				return wasWorkDone;
			}

			#endregion //LoadItemsLazily

			#region OnTimerTick

			private void OnTimerTick(object sender, EventArgs e)
			{
				CalendarBase cal = CalendarUtilities.GetCalendar(_panel);

				if (cal == null || _panel._groups == null || _panel._groups.Count < MinGroupLoadThreshold)
				{
					_panel.StopLoader();
					return;
				}

				Debug.WriteLine("OnTick: " + DateTime.UtcNow.Subtract(_startTime).TotalMilliseconds.ToString());

				bool wasWorkDone = false;

				// first try loading a month's worth of CalendarDays
				if (cal.MinCalendarModeResolved == CalendarZoomMode.Days)
					wasWorkDone = this.LoadItemsLazily(true);

				if (wasWorkDone)
					Debug.WriteLine("LoadDays: " + DateTime.UtcNow.Subtract(_startTime).TotalMilliseconds.ToString());

				// once all the CalendarDays have been loaded create a month's worth of 
				// CalendarItems for the zoom modes
				if (wasWorkDone == false && cal.MaxCalendarMode != CalendarZoomMode.Days)
				{
					wasWorkDone = this.LoadItemsLazily(false);

					if (wasWorkDone)
						Debug.WriteLine("LoadItems: " + DateTime.UtcNow.Subtract(_startTime).TotalMilliseconds.ToString());

				}

				if (wasWorkDone)
				{
					_timer.Stop();
					_timer.Interval = TimeSpan.FromMilliseconds(IntervalMS);
					_timer.Start();
				}
				else
					_panel.StopLoader();
			}

			#endregion //OnTimerTick

			#endregion //Private Methods	
    
			#region Internal Methods

			#region Restart

			internal void Restart()
			{
				Debug.WriteLine("Restart: " + DateTime.UtcNow.Subtract(_startTime).TotalMilliseconds.ToString());

				_dayPassGroupIndex = 0;
				_itemPassGroupIndex = 0;
				_timer.Stop();
				_timer.Interval = TimeSpan.FromMilliseconds(IntervalMSInitial);
				_timer.Start();

				_startTime = DateTime.UtcNow;

			}

			#endregion //Restart

			#region Stop

			internal void Stop()
			{
				Debug.WriteLine("Stop: " + DateTime.UtcNow.Subtract(_startTime).TotalMilliseconds.ToString());

				if (this._timer != null)
				{
					this._timer.Tick -= new EventHandler(OnTimerTick);
					this._timer.Stop();
					this._timer = null;
				}

			}

			#endregion //Stop

			#endregion //Internal Methods	
    
			#endregion //Methods
		}

		#endregion //LazyLoader private class
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