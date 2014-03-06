using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Controls;
using System.ComponentModel;

namespace Infragistics.Windows.Editors
{
    /// <summary>
    /// Custom element used to create and position <see cref="CalendarItemGroup"/> instances for a <see cref="XamMonthCalendar"/>
    /// </summary>
    /// <remarks>
    /// <p class="body">The CalendarItemGroupPanel is designed to be used within the template of a <see cref="XamMonthCalendar"/>. When 
    /// used in the XamMonthCalendar template, this class will automatically generate and arrange <see cref="CalendarItemGroup"/> instances 
    /// based on the <see cref="XamMonthCalendar.CalendarDimensions"/>. When the <see cref="XamMonthCalendar.AutoAdjustCalendarDimensions"/> is 
    /// true, the panel will generate more CalendarItemGroups if they can fit up to the <see cref="MaxGroups"/>, which defaults to 12. 
    /// You may also retemplate the XamMonthCalendar such that it directly contains CalendarItemGroup instances and set their 
    /// <see cref="CalendarItemGroup.ReferenceGroupOffset"/>.</p>
    /// <p class="body">By default, the <see cref="GroupWidth"/> and <see cref="GroupHeight"/> properties are set to double.NaN. When either 
    /// is left set to the default, the panel will calculate the size required for a group to display its contents and arrange all the groups 
    /// using that size.</p>
    /// </remarks>
    /// <seealso cref="XamMonthCalendar"/>
    /// <seealso cref="CalendarItemGroup"/>
    /// <seealso cref="CalendarItemGroup.ReferenceGroupOffset"/>
    /// <seealso cref="CalendarItemGroupPanel.MaxGroups"/>
    //[System.ComponentModel.ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CalendarItemGroupPanel : FrameworkElement
    {
        #region Member Variables

        private List<CalendarItemGroup> _groups;
        private CalendarItemGroup _itemGroupForSizing;
        private List<object> _logicalChildren;
        private CalendarItemGroup _groupBeingMeasured;
        private const string SizingGroupName = "GroupForSizing";

        #endregion //Member Variables

        #region Constructor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static CalendarItemGroupPanel()
        {
            XamMonthCalendar.MonthCalendarPropertyKey.OverrideMetadata(typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnMonthCalendarChanged)));
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLanguageChanged)));
        }

        /// <summary>
        /// Initializes a new <see cref="CalendarItemGroupPanel"/>
        /// </summary>
        public CalendarItemGroupPanel()
        {
            this._groups = new List<CalendarItemGroup>();
            this._logicalChildren = new List<object>();
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
                sizingGroup.Arrange(new Rect(sizingGroup.DesiredSize));

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
                this._groups.Add(group);
            }

            // remove any unneeded groups
            for (int i = currentGroupCount; i > requiredGroups; i--)
            {
                CalendarItemGroup group = this._groups[i - 1];
                this._groups.RemoveAt(i - 1);
                this.RemoveVisualChild(group);
                this.RemoveLogicalChildHelper(group);
            } 
            #endregion //Add/Remove Groups

            bool showLeadingTrailing = this.ShowLeadingAndTrailing;
            HorizontalAlignment hAlign = this.HorizontalContentAlignment;
            VerticalAlignment vAlign = this.VerticalContentAlignment;

            double xOffset, yOffset;

            CalculateOffset(ref finalSize, ref dims, ref itemSize, hAlign, vAlign, out xOffset, out yOffset); 

            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);
            object scrollButtonVisOn = this.GetValue(XamMonthCalendar.ScrollButtonVisibilityProperty);
            object scrollButtonVisOff = Visibility.Collapsed.Equals(scrollButtonVisOn)
                ? KnownBoxes.VisibilityCollapsedBox
                : KnownBoxes.VisibilityHiddenBox;

            // measure/arrange everyone
            for (int i = 0, last = this._groups.Count - 1; i <= last; i++)
            {
                CalendarItemGroup group = this._groups[i];

                this._groupBeingMeasured = group;

                // initialize leading/trailing
                group.SetValue(CalendarItemGroup.ShowLeadingDatesProperty, KnownBoxes.FromValue(showLeadingTrailing && i == 0));
                group.SetValue(CalendarItemGroup.ShowTrailingDatesProperty, KnownBoxes.FromValue(showLeadingTrailing && i == last));

                // initialize scroll buttons
                group.SetValue(CalendarItemGroup.ScrollPreviousButtonVisibilityProperty, i == 0 ? scrollButtonVisOn : scrollButtonVisOff);
                group.SetValue(CalendarItemGroup.ScrollNextButtonVisibilityProperty, i == dims.Columns - 1 ? scrollButtonVisOn : scrollButtonVisOff);

                // we wait until here to add the group to the visual/logical tree
                // to minimize the number of times that the items will be generated
                if (i >= currentGroupCount)
                {
                    this.AddVisualChild(group);
                    this.AddLogicalChildHelper(group);
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
            if (this._itemGroupForSizing != null)
            {
                if (index == 0)
                    return this._itemGroupForSizing;

                index--;
            }

            return this._groups[index];
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
                return this._logicalChildren.GetEnumerator();
            }
        }
        #endregion //LogicalChildren

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

        #region VisualChildrenCount
        /// <summary>
        /// Returns the number of visual children for the element.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                int count = this._groups.Count;

                if (null != this._itemGroupForSizing)
                    count++;

                return count;
            }
        }
        #endregion //VisualChildrenCount

        #endregion //Base class overrides

        #region Properties

        #region Public

        #region HorizontalContentAlignment

        /// <summary>
        /// Identifies the <see cref="HorizontalContentAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalContentAlignmentProperty = Control.HorizontalContentAlignmentProperty.AddOwner(typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox, FrameworkPropertyMetadataOptions.AffectsArrange));

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

        // AS 10/1/08 TFS8497
        #region GroupHeight

        /// <summary>
        /// Identifies the <see cref="GroupHeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GroupHeightProperty = DependencyProperty.Register("GroupHeight",
            typeof(double), typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidateWidthHeight));

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

        // AS 10/1/08 TFS8497
        #region GroupWidth

        /// <summary>
        /// Identifies the <see cref="GroupWidth"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GroupWidthProperty = DependencyProperty.Register("GroupWidth",
            typeof(double), typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidateWidthHeight));

        private static bool ValidateWidthHeight(object newValue)
        {
            double val = (double)newValue;

            return double.IsNaN(val) ||
                (val >= 0d && !double.IsInfinity(val));
        }

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

        #region MaxGroups

        /// <summary>
        /// Identifies the <see cref="MaxGroups"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaxGroupsProperty = DependencyProperty.Register("MaxGroups",
            typeof(int), typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(12), new ValidateValueCallback(Utils.ValidateIntZeroOrMore));

        /// <summary>
        /// Returns or sets the maximum number of groups that the element may generate.
        /// </summary>
        /// <remarks>
        /// <p class="note">The maximum of this value with the rows * columns of the <see cref="XamMonthCalendar.CalendarDimensions"/> 
        /// will be used to determine the actual maximum value allowed.</p>
        /// </remarks>
        /// <seealso cref="MaxGroupsProperty"/>
        /// <seealso cref="XamMonthCalendar.CalendarDimensions"/>
        /// <seealso cref="XamMonthCalendar.AutoAdjustCalendarDimensions"/>
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

        #region VerticalContentAlignment

        /// <summary>
        /// Identifies the <see cref="VerticalContentAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalContentAlignmentProperty = Control.VerticalContentAlignmentProperty.AddOwner(typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox, FrameworkPropertyMetadataOptions.AffectsArrange));

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

        #region AutoAdjustCalendarDimensionsProperty

        private static readonly DependencyProperty AutoAdjustCalendarDimensionsProperty = XamMonthCalendar.AutoAdjustCalendarDimensionsProperty.AddOwner(typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, FrameworkPropertyMetadataOptions.AffectsArrange));

        private bool AutoAdjustDimensions
        {
            get { return (bool)this.GetValue(AutoAdjustCalendarDimensionsProperty); }
        }

        #endregion //AutoAdjustCalendarDimensionsProperty

        #region CalendarDimensionsProperty

        private static readonly DependencyProperty CalendarDimensionsProperty = XamMonthCalendar.CalendarDimensionsProperty.AddOwner(typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(new CalendarDimensions(1,1), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        private CalendarDimensions Dimensions
        {
            get { return (CalendarDimensions)this.GetValue(CalendarDimensionsProperty); }
        }

        #endregion //CalendarDimensionsProperty

        #region CalendarManager

        internal CalendarManager CalendarManager
        {
            get
            {
                XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);

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
                    this._itemGroupForSizing.Name = SizingGroupName;

                    // AS 9/3/08
                    // There is a bug/behavior in WPF whereby not including this
                    // element in the visual tree is causing its logical children
                    // - the items - to not get the local style.
                    //
                    this._itemGroupForSizing.Visibility = Visibility.Hidden;
                    this.AddVisualChild(this._itemGroupForSizing);

                    this.AddLogicalChildHelper(this._itemGroupForSizing);
                    ((ISupportInitialize)this._itemGroupForSizing).EndInit();
                }

                return this._itemGroupForSizing;
            }
        }
        #endregion //ItemGroupForSizing

        #region ShowLeadingAndTrailingDatesProperty

        private static readonly DependencyProperty ShowLeadingAndTrailingDatesProperty = XamMonthCalendar.ShowLeadingAndTrailingDatesProperty.AddOwner(typeof(CalendarItemGroupPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnShowLeadingAndTrailingDatesChanged)));

        private static void OnShowLeadingAndTrailingDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarItemGroupPanel p = (CalendarItemGroupPanel)d;

            p.InvalidateMeasure();

            // update the groups?
            if (0 < p._groups.Count)
            {
                bool show = (bool)e.NewValue;
                p._groups[0].ShowLeadingDates = show;
                p._groups[p._groups.Count - 1].ShowTrailingDates = show;
            }
        }

        private bool ShowLeadingAndTrailing
        {
            get { return (bool)this.GetValue(ShowLeadingAndTrailingDatesProperty); }
        }
        #endregion //ShowLeadingAndTrailingDatesProperty

        #region ScrollButtonVisibility

        private static readonly DependencyProperty ScrollButtonVisibilityProperty = XamMonthCalendar.ScrollButtonVisibilityProperty.AddOwner(typeof(CalendarItemGroupPanel),
            new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, FrameworkPropertyMetadataOptions.AffectsArrange));

        #endregion //ScrollButtonVisibility

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Private

        #region AddLogicalChildHelper
        private void AddLogicalChildHelper(object newChild)
        {
            if (null != newChild)
            {
                this._logicalChildren.Add(newChild);
                this.AddLogicalChild(newChild);
            }
        }
        #endregion //AddLogicalChildHelper

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
                c.RemoveVisualChild(group);
                c.RemoveLogicalChildHelper(group);
            }

            // we should also remove the group for sizing
            if (null != c._itemGroupForSizing)
            {
                CalendarItemGroup oldSizingGroup = c._itemGroupForSizing;
                c._itemGroupForSizing = null;

                c.RemoveVisualChild(oldSizingGroup);
                c.RemoveLogicalChildHelper(oldSizingGroup);
            }

        } 
        #endregion //OnLanguageChanged

        #region OnMonthCalendarChanged
        private static void OnMonthCalendarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarItemGroupPanel p = (CalendarItemGroupPanel)d;
            XamMonthCalendar cal = e.NewValue as XamMonthCalendar;

            if (null != cal)
            {
                p.SetBinding(CalendarItemGroupPanel.ShowLeadingAndTrailingDatesProperty, Utilities.CreateBindingObject(XamMonthCalendar.ShowLeadingAndTrailingDatesProperty, System.Windows.Data.BindingMode.OneWay, cal));
                p.SetBinding(CalendarItemGroupPanel.CalendarDimensionsProperty, Utilities.CreateBindingObject(XamMonthCalendar.CalendarDimensionsProperty, System.Windows.Data.BindingMode.OneWay, cal));
                p.SetBinding(CalendarItemGroupPanel.AutoAdjustCalendarDimensionsProperty, Utilities.CreateBindingObject(XamMonthCalendar.AutoAdjustCalendarDimensionsProperty, System.Windows.Data.BindingMode.OneWay, cal));
                p.SetBinding(CalendarItemGroupPanel.ScrollButtonVisibilityProperty, Utilities.CreateBindingObject(XamMonthCalendar.ScrollButtonVisibilityProperty, System.Windows.Data.BindingMode.OneWay, cal));
            }
        }
        #endregion //OnMonthCalendarChanged

        // AS 10/1/08 TFS8497
        #region ReleaseSizingGroup
        private void ReleaseSizingGroup()
        {
            if (null != this._itemGroupForSizing)
            {
                CalendarItemGroup oldGroup = this._itemGroupForSizing;
                this._itemGroupForSizing = null;
                this.RemoveVisualChild(oldGroup);
                this.RemoveLogicalChildHelper(oldGroup);
            }
        }
        #endregion //ReleaseSizingGroup

        #region RemoveLogicalChildHelper
        private void RemoveLogicalChildHelper(object oldChild)
        {
            if (null != oldChild)
            {
                this._logicalChildren.Remove(oldChild);
                this.RemoveLogicalChild(oldChild);
            }
        }
        #endregion //RemoveLogicalChildHelper

        #endregion //Private

        #endregion //Methods
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