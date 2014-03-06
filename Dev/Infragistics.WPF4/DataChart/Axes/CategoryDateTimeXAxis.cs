using System;



using System.Linq;

using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents an category-based horizontal X axis that uses a DateTime scale.
    /// </summary>
    [WidgetModule("CategoryChart")]
    [WidgetModule("RangeCategoryChart")]
    [WidgetModule("FinancialChart")]
    public class CategoryDateTimeXAxis : CategoryAxisBase, ISortingAxis
    {
        #region Constructor
        /// <summary>
        /// Creates an new instance of CategoryDateTimeXAxis class.
        /// </summary>
        public CategoryDateTimeXAxis():base() 
        {


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion

        internal override AxisView CreateView()
        {
            return new CategoryDateTimeXAxisView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);

            DateTimeView = (CategoryDateTimeXAxisView)view;
        }
        internal CategoryDateTimeXAxisView DateTimeView { get; set; }

        #region DisplayType Dependency property
        internal const string DisplayTypePropertyName = "DisplayType";

        /// <summary>
        /// Identifies the DisplayType dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayTypeProperty = DependencyProperty.Register(DisplayTypePropertyName, typeof(TimeAxisDisplayType), typeof(CategoryDateTimeXAxis),
            new PropertyMetadata(TimeAxisDisplayType.Continuous, (sender, e) =>
            {
                (sender as CategoryDateTimeXAxis).RaisePropertyChanged(DisplayTypePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the axis display type. 
        /// Continuous display type divides the axis into even intervals, where labels will not necessarily be aligned with data points.
        /// Discrete display type will not use a constant interval, but will align each label with its data point.
        /// </summary>
        public TimeAxisDisplayType DisplayType
        {
            get
            {
                return (TimeAxisDisplayType)GetValue(DisplayTypeProperty);
            }
            set
            {
                SetValue(DisplayTypeProperty, value);
            }
        }
        #endregion

        #region MinimumValue Dependency Property
        internal const string MinimumValuePropertyName = "MinimumValue";
        /// <summary>
        /// Identifies the MinimumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumValueProperty = DependencyProperty.Register(MinimumValuePropertyName, typeof(DateTime), typeof(CategoryDateTimeXAxis),
            new PropertyMetadata(



                (sender, e) =>
            {
                (sender as CategoryDateTimeXAxis).RaisePropertyChanged(MinimumValuePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the axis MinimumValue.
        /// </summary>

        [TypeConverter(typeof(DateTimeConverter))]

        [DontObfuscate]
        public DateTime MinimumValue
        {
            get
            {
                return (DateTime)GetValue(MinimumValueProperty);
            }
            set
            {
                SetValue(MinimumValueProperty, value);
            }
        }
        #endregion

        #region MaximumValue Dependency Property

        internal const string MaximumValuePropertyName = "MaximumValue";

        /// <summary>
        /// Identifies the MaximumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumValueProperty = DependencyProperty.Register(MaximumValuePropertyName, typeof(DateTime), typeof(CategoryDateTimeXAxis),
            new PropertyMetadata(



                (sender, e) =>
            {
                (sender as CategoryDateTimeXAxis).RaisePropertyChanged(MaximumValuePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the axis MaximumValue.
        /// </summary>

        [TypeConverter(typeof(DateTimeConverter))]

        [DontObfuscate]
        public DateTime MaximumValue
        {
            get
            {
                return (DateTime)GetValue(MaximumValueProperty);
            }
            set
            {
                SetValue(MaximumValueProperty, value);
            }
        }
        #endregion

        #region Interval Dependency Property
        internal const string IntervalPropertyName = "Interval";
        /// <summary>
        /// Identifies the Interval dependency property.
        /// </summary>
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(
            IntervalPropertyName, 



            typeof(TimeSpan), 

            typeof(CategoryDateTimeXAxis),
            new PropertyMetadata(



                (sender, e) =>
            {
                (sender as CategoryDateTimeXAxis).RaisePropertyChanged(IntervalPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the X axis time interval.
        /// </summary>

        [TypeConverter(typeof(TimeSpanConverter))]

        public 



            TimeSpan 

            Interval
        {
            get
            {
                return 



                    (TimeSpan)

                    GetValue(IntervalProperty);
            }
            set
            {
                SetValue(IntervalProperty, value);
            }
        }
        #endregion

        internal override AxisLabelPanelBase CreateLabelPanel()
        {
            return new HorizontalAxisLabelPanel();
        }

        internal override double GetCategorySize(Rect windowRect, Rect viewportRect)
        {
            return viewportRect.Width / (ItemsCount * windowRect.Width);
        }

        internal override double GetGroupCenter(int groupIndex, Rect windowRect, Rect viewportRect)
        {
            //ignore Mode2GroupCount, as it is irrelevant with a date time axis.
            return GetCategorySize(windowRect, viewportRect) * 0.5;
        }

        internal override double GetGroupSize(Rect windowRect, Rect viewportRect)
        {
            double gap = !double.IsNaN(Gap) ? MathUtil.Clamp(Gap, 0.0, 1.0) : 0.0;            
            double categorySpace = 1.0 - 0.5 * gap;

            double ret = GetCategorySize(windowRect, viewportRect) * categorySpace;

            return Math.Max(1.0, ret);



        }

        private List<int> _sorderDateTimeIndices;
        private List<int> SortedDateTimeIndices 
        {
            get 
            { 
                return _sorderDateTimeIndices; 
            }
            set
            {
                _sorderDateTimeIndices = value;
            }
        }

        /// <summary>
        /// Creates or updates the visual elements of the Axis.
        /// </summary>
        /// <param name="animate">Indicates if the visual changes should be animated.</param>
        protected override void RenderAxisOverride(bool animate)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = !windowRect.IsEmpty ? ViewportRect : Rect.Empty;
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, IsInverted);
            GeometryCollection axisGeometry = View.GetAxisLinesGeometry();
            GeometryCollection stripsGeometry = View.GetStripsGeometry();
            GeometryCollection majorGeometry = View.GetMajorLinesGeometry();
            GeometryCollection minorGeometry = View.GetMinorLinesGeometry();

            UpdateLineVisibility();

            ClearMarks(axisGeometry);
            ClearMarks(stripsGeometry);
            ClearMarks(majorGeometry);
            ClearMarks(minorGeometry);

            LabelDataContext.Clear();
            LabelPositions.Clear();

            this.LabelPanel.Axis = this;
            this.LabelPanel.WindowRect = windowRect;
            this.LabelPanel.ViewportRect = viewportRect;
            if (windowRect.IsEmpty || viewportRect.IsEmpty)
            {
                TextBlocks.Count = 0;
            }
            if (TextBlocks.Count == 0)
            {
                LabelPanel.Children.Clear();
            }
            //foreach (TextBlock tb in
            //  LabelPanel.Children.OfType<TextBlock>())
            //{
            //    AxisLabelManager.UnbindLabel(tb);
            //}
            //this.LabelPanel.Children.Clear();
            if (this.LabelSettings != null)
            {
                this.LabelSettings.Axis = this;
            }
            this.InitializeActualMinimumAndMaximum();



            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && DateTimeColumn != null)
            {
                #region Draw Axis Line
                //axis is always at the bottom unless crossing axis is set.
                double crossingValue = viewportRect.Bottom;
                if (this.CrossingAxis != null)
                {
                    NumericYAxis yAxis = this.CrossingAxis as NumericYAxis;
                    if (yAxis != null)
                    {
                        ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yAxis.IsInverted);



                        crossingValue = Convert.ToDouble(CrossingValue);

                        crossingValue = yAxis.GetScaledValue(crossingValue, yParams);
                        if (crossingValue < viewportRect.Top)
                        {
                            crossingValue = viewportRect.Top;
                        }
                        else if (crossingValue > viewportRect.Bottom)
                        {
                            crossingValue = viewportRect.Bottom;
                        }
                    }
                }

                //make sure crossingValue didn't get scaled to a NaN
                if (double.IsNaN(crossingValue))
                {
                    crossingValue = 0;
                }

                HorizontalLine(axisGeometry, crossingValue, viewportRect);
                this.LabelPanel.CrossingValue = crossingValue;
                #endregion

                #region Linear Strips and Lines
                if (this.DisplayType == TimeAxisDisplayType.Discrete)
                {
                    //No stripes, no minor gridlines. Draw a major gridline for each data point.
                    int first = ((ISortingAxis)this).GetFirstVisibleIndex(windowRect, viewportRect);
                    int last = ((ISortingAxis)this).GetLastVisibleIndex(windowRect, viewportRect);

                    //this axis is indexed, so first and last have to be non-negative
                    if (first < 0 || last < 0)
                    {
                        return;
                    }

                    //DataTemplate labelTemplate = this.Label as DataTemplate;
                    //StringFormatter labelFormatter = labelTemplate != null ? null : new StringFormatter() { FormatString = this.Label as string };

                    for (int i = first; i <= last; i++)
                    {
                        int sortedIndex = SortedDateTimeIndices == null ? i : SortedDateTimeIndices[i];
                        double majorValue = GetScaledValue(DateTimeColumn[sortedIndex].Ticks, xParams);

                        if (CategoryMode == CategoryMode.Mode2)
                        {
                            majorValue += IsInverted ? -GetGroupCenter(i, windowRect, viewportRect) : GetGroupCenter(i, windowRect, viewportRect);
                        }

                        if (majorValue < viewportRect.Left || majorValue > viewportRect.Right)
                        {
                            continue;
                        }

                        VerticalLine(majorGeometry, majorValue, viewportRect);

                        if (this.FastItemsSource != null && i < this.FastItemsSource.Count)
                        {
                            object dataItem = this.FastItemsSource[sortedIndex];
                            object labelText = base.GetLabel(dataItem);

                            if (!double.IsNaN(majorValue) && !double.IsInfinity(majorValue))
                            {
                                LabelDataContext.Add(labelText);
                                LabelPositions.Add(new LabelPosition(majorValue));
                            }
                        }
                    }
                }
                else
                {
                    //Display gridlines at even intervals.
                    double visibleMinimum = GetUnscaledValue(viewportRect.Left, xParams);
                    double visibleMaximum = GetUnscaledValue(viewportRect.Right, xParams);

                    //DateTimeSnapper dateSnapper = new DateTimeSnapper(visibleDateMinimum, visibleDateMaximum, viewportRect.Width);

                    double trueVisibleMinimum = Math.Min(visibleMinimum, visibleMaximum);
                    double trueVisibleMaximum = Math.Max(visibleMinimum, visibleMaximum);
                    LinearNumericSnapper snapper = new LinearNumericSnapper(trueVisibleMinimum, trueVisibleMaximum, viewportRect.Width);

                    double interval = HasUserInterval ? GetUserIntervalTicks() : snapper.Interval;

                    int first = (int)Math.Floor((trueVisibleMinimum - ActualMinimumValue.Ticks) / interval);
                    int last = (int)Math.Ceiling((trueVisibleMaximum - ActualMinimumValue.Ticks) / interval);

                    double offset = 0.0;
                    if (CategoryMode == CategoryMode.Mode2)
                    {
                        offset = GetGroupCenter(0, windowRect, viewportRect);
                        offset = IsInverted ? -offset : offset;
                    }

                    int viewportPixelRight = (int)Math.Ceiling(viewportRect.Right);
                    int viewportPixelLeft = (int)Math.Floor(viewportRect.Left);
                    double majorValue = GetScaledValue(ActualMinimumValue.Ticks + first * interval, xParams) + offset;

                    for (int i = first; i <= last; i++)
                    {
                        double nextMajorValue = GetScaledValue(ActualMinimumValue.Ticks + (i + 1) * interval, xParams) + offset;

                        if (!double.IsNaN(majorValue) && !double.IsInfinity(majorValue))
                        {
                            int categoryPixelValue = (int)Math.Round(majorValue);
                            if (categoryPixelValue <= viewportPixelRight)
                            {
                                if (i % 2 == 0)
                                {
                                    VerticalStrip(stripsGeometry, majorValue, nextMajorValue, viewportRect);
                                }

                                VerticalLine(majorGeometry, majorValue, viewportRect);

                            for (int j = 1; j < snapper.MinorCount; ++j)
                            {
                                double minorValue = GetScaledValue(ActualMinimumValue.Ticks + i * interval + (j * interval) / snapper.MinorCount, xParams) + offset;

                                    VerticalLine(minorGeometry, minorValue, viewportRect);
                                }
                            }

                            if (categoryPixelValue >= viewportPixelLeft && categoryPixelValue <= viewportPixelRight)
                            {
                                double majorX = ActualMinimumValue.Ticks + i * interval;
                                long ticks_ = (long)Math.Floor(majorX);



                                DateTime dateValue = new DateTime(ticks_);


                                object labelText = this.GetLabel(dateValue);

                                LabelDataContext.Add(labelText);
                                LabelPositions.Add(new LabelPosition(majorValue));
                            }
                        }

                        majorValue = nextMajorValue;
                    }
                }
                #endregion

                #region Draw Floating Axis Panel
                if ((this.LabelSettings == null || this.LabelSettings.Visibility == Visibility.Visible) && this.CrossingAxis != null)
                {
                    if (this.LabelSettings != null && (this.LabelSettings.Location == AxisLabelsLocation.InsideTop || this.LabelSettings.Location == AxisLabelsLocation.InsideBottom))
                    {
                        SeriesViewer.InvalidatePanels();
                    }
                }
                #endregion

                #region Linear Labels

                this.LabelPanel.LabelDataContext = LabelDataContext;
                this.LabelPanel.LabelPositions = LabelPositions;
                RenderLabels();
                #endregion
            }
        }

        private double GetUserIntervalTicks()
        {



            return ActualInterval.Ticks;

        }

        internal void InitializeActualMinimumAndMaximum()
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = !windowRect.IsEmpty ? ViewportRect : Rect.Empty;



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            DateTime newActualMinimum = DateTime.MinValue;
            DateTime newActualMaximum = DateTime.MaxValue;

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && DateTimeColumn != null)
            {

                var fastDateColumn = this.DateTimeColumn as FastItemDateTimeColumn;
                if (fastDateColumn != null)
                {
                    if (this.SortedDateTimeIndices == null)
                    {
                        this.SortedDateTimeIndices = fastDateColumn.GetSortedIndices();
                    }
                }
                else
                {
                    this.SortedDateTimeIndices = null;
                }

                if (DateTimeColumn.Count > 0)
                {
                    int firstIndex = this.SortedDateTimeIndices == null ? 0 : this.SortedDateTimeIndices[0];
                    int lastIndex = this.SortedDateTimeIndices == null
                                        ? this.DateTimeColumn.Count - 1
                                        : this.SortedDateTimeIndices[this.DateTimeColumn.Count - 1];
                    newActualMinimum = this.DateTimeColumn[firstIndex];
                    newActualMaximum = this.DateTimeColumn[lastIndex];
                    HasUserInterval = false;

                    //for column series apply a time offset to axis min and max. 
                    //1.25 is used to add a small margin for the first and last points, similar to non-time based column series.
                    if (CategoryMode == CategoryMode.Mode2)
                    {
                        long timeSpan = newActualMaximum.Ticks - newActualMinimum.Ticks;



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                        TimeSpan timeOffset = new TimeSpan((long)(timeSpan * 1.25 / ItemsCount / 2.0));
                        if (timeOffset.Ticks == 0)
                        {
                            timeOffset = TimeSpan.FromMilliseconds(1); // enforce a minimum offset, so that a single data value may be rendered
                        }
                        newActualMinimum = newActualMinimum.Subtract(timeOffset);
                        newActualMaximum = newActualMaximum.Add(timeOffset);

                    }

                    if (MinimumValueIsSet())
                    {
                        newActualMinimum = MinimumValue;
                    }

                    if (MaximumValueIsSet())
                    {
                        newActualMaximum = MaximumValue;
                    }

                    if (IntervalIsSet())
                    {
                        ActualInterval = Interval;

                        //a few sanity checks for the interval. 
                        //Should be non-zero.
                        //Shouldn't produce more intervals than the number of pixels in the viewport and that's pretty generous.

                        long span = Math.Abs(newActualMaximum.Ticks - newActualMinimum.Ticks);
                        HasUserInterval = ActualIntervalIsEmpty() ||
                                          (DisplayType == TimeAxisDisplayType.Discrete) ||
                                          (1.0 * span / GetUserIntervalTicks() > viewportRect.Width)
                                              ? false
                                              : true;
                    }
                }
            }
            this.ActualMinimumValue = newActualMinimum;
            this.ActualMaximumValue = newActualMaximum;
        }

        private bool IntervalIsSet()
        {



            return ReadLocalValue(IntervalProperty) != DependencyProperty.UnsetValue;

        }


        private bool ActualIntervalIsEmpty()
        {



            return (ActualInterval == TimeSpan.Zero);

        }

        private bool MinimumValueIsSet()
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            return ReadLocalValue(MinimumValueProperty) != DependencyProperty.UnsetValue;

        }
        private bool MaximumValueIsSet()
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            return ReadLocalValue(MaximumValueProperty) != DependencyProperty.UnsetValue;

        }

        private const string ActualMinimumValuePropertyName = "ActualMinimumValue";
        private DateTime _actualMinimumValue;

        /// <summary>
        /// Gets the coerced minimum value.
        /// </summary>
        public DateTime ActualMinimumValue
        {
            get
            {
                return _actualMinimumValue;
            }
            internal set
            {
                bool changed = _actualMinimumValue != value;
                if (changed)
                {
                    object oldValue = this._actualMinimumValue;
                    _actualMinimumValue = value;
                    this.RaisePropertyChanged(ActualMinimumValuePropertyName, oldValue, value);
                }
            }
        }
        private const string ActualMaximumValuePropertyName = "ActualMaximumValue";
        private DateTime _actualMaximumValue;

        /// <summary>
        /// Gets the coerced maximum value.
        /// </summary>
        public DateTime ActualMaximumValue
        {
            get
            {
                return _actualMaximumValue;
            }
            internal set
            {
                bool changed = _actualMaximumValue != value;
                if (changed)
                {
                    object oldValue = this._actualMaximumValue;
                    _actualMaximumValue = value;
                    this.RaisePropertyChanged(ActualMaximumValuePropertyName, oldValue, value);
                }
            }
        }

        internal bool HasUserInterval { get; set; }





        private TimeSpan _actualInterval;
        internal TimeSpan ActualInterval

        {
            get
            {
                return _actualInterval;
            }
            set
            {
                _actualInterval = value;
            }
        }

        //private TimeSpan GetLengthOfOnePixel()
        //{
        //    return TimeSpan.MaxValue;
        //}
        /// <summary>
        /// Gets a scaled value inside the viewport.
        /// </summary>
        /// <param name="unscaledValue">Value to scale.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>Scaled value inside the viewport.</returns>
        public override double GetScaledValue(double unscaledValue, ScalerParams p)
        {
            double scaledValue;

            if (ActualMaximumValue == ActualMinimumValue)
            {
                scaledValue = -1;
            }
            else
            {
                scaledValue = (unscaledValue - ActualMinimumValue.Ticks) / (ActualMaximumValue.Ticks - ActualMinimumValue.Ticks);
            }

            double offset = 0.0;
            if (CategoryMode == CategoryMode.Mode2)
            {
                offset = GetGroupCenter(0, p.WindowRect, p.ViewportRect);
            }

            if (IsInverted)
            {
                scaledValue = 1.0 - scaledValue;
                offset = -offset;
            }

            return p.ViewportRect.Left + p.ViewportRect.Width * (scaledValue - p.WindowRect.Left) / p.WindowRect.Width - offset;
        }



        /// <summary>
        /// Returns an unscaled value from a scaled value based on the amount of data.
        /// </summary>
        /// <param name="scaledValue">Scaled value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>Unscaled value from a scaled value based on the amount of data.</returns>
        public override double GetUnscaledValue(double scaledValue, ScalerParams p)
        {
            return GetUnscaledValue(scaledValue, p.WindowRect, p.ViewportRect, CategoryMode);
        }

        internal override double GetUnscaledValue(double scaledValue, Rect windowRect, Rect viewportRect, CategoryMode categoryMode)
        {
            double unscaledValue = windowRect.Left + windowRect.Width * (scaledValue - viewportRect.Left) / viewportRect.Width;

            if (IsInverted)
            {
                unscaledValue = 1.0 - unscaledValue;
            }

            return (long)Math.Floor(ActualMinimumValue.Ticks + unscaledValue * (ActualMaximumValue.Ticks - ActualMinimumValue.Ticks));

        }

        private DateTime GetDate(int index)
        {
            return this.DateTimeColumn == null ? DateTime.MinValue : this.DateTimeColumn[index];
        }


#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)

        private const string DateTimeMemberPathPropertyName = "DateTimeMemberPath";

        /// <summary>
        /// Identifies the DateTimeMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty DateTimeMemberPathProperty = DependencyProperty.Register(DateTimeMemberPathPropertyName, typeof(string), typeof(CategoryDateTimeXAxis), new PropertyMetadata(null,
            (sender, e) =>
            {
                (sender as CategoryDateTimeXAxis).RaisePropertyChanged(DateTimeMemberPathPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the DateTime mapping property for the CategoryDateTimeXAxis.
        /// </summary>
        public string DateTimeMemberPath
        {
            get
            {
                return (string)this.GetValue(DateTimeMemberPathProperty);
            }
            set
            {
                this.SetValue(DateTimeMemberPathProperty, value);
            }
        }

        /// <summary>
        /// Gets column of date time values for the axis.
        /// </summary>
        protected internal IFastItemColumn<DateTime> DateTimeColumn
        {
            get { return dateTimeColumn; }
            private set
            {
                if (dateTimeColumn != value)
                {
                    IFastItemColumn<DateTime> oldDateTimeColumn = dateTimeColumn;

                    dateTimeColumn = value;
                    RaisePropertyChanged(DateTimeColumnPropertyName, oldDateTimeColumn, dateTimeColumn);
                }
            }
        }
        private IFastItemColumn<DateTime> dateTimeColumn;
        internal const string DateTimeColumnPropertyName = "DateTimeColumn";



        /// <summary>
        /// Registers a date time column for the series from the assigned fast items source.
        /// </summary>
        /// <param name="memberPath">The path to use to obtain values for the column.</param>
        /// <returns>The fast items column reference.</returns>
        protected IFastItemColumn<DateTime> RegisterDateTimeColumn(string memberPath)
        {


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            return FastItemsSource.RegisterColumnDateTime(memberPath);

        }


        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the axis. Gives the axis a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
            switch (propertyName)
            {
                case FastItemsSourcePropertyName:
                    FastItemsSource oldFastItemsSource = oldValue as FastItemsSource;
                    if (oldFastItemsSource != null)
                    {
                        oldFastItemsSource.DeregisterColumn(DateTimeColumn);
                        this.DateTimeColumn = null;
                        oldFastItemsSource.Event -= new EventHandler<FastItemsSourceEventArgs>(fastItemsSource_Event);
                    }

                    FastItemsSource newFastItemsSource = newValue as FastItemsSource;
                    if (newFastItemsSource != null)
                    {
                        SortedDateTimeIndices = null;
                        this.DateTimeColumn = RegisterDateTimeColumn(this.DateTimeMemberPath);
                        newFastItemsSource.Event += new EventHandler<FastItemsSourceEventArgs>(fastItemsSource_Event);
                    }

                    RenderAxisAndSeries(false);

                    break;
                case DateTimeMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(DateTimeColumn);
                        DateTimeColumn = RegisterDateTimeColumn(DateTimeMemberPath);
                        SortedDateTimeIndices = null;
                    }

                    break;
                case DisplayTypePropertyName:
                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    break;

                case MinimumValuePropertyName:
                    UpdateRange();
                    RenderAxisAndSeries(false);
                    break;

                case MaximumValuePropertyName:
                    UpdateRange();
                    RenderAxisAndSeries(false);
                    break;

                case IntervalPropertyName:
                    MustInvalidateLabels = true;
                    RenderAxis(false);
                    break;
            }
        }

        private void fastItemsSource_Event(object sender, FastItemsSourceEventArgs e)
        {
            this.SortedDateTimeIndices = null;
        }

        /// <summary>
        /// Renders the this date time axis and all series that belong to this axis.
        /// </summary>
        /// <param name="animate"></param>
        private void RenderAxisAndSeries(bool animate)
        {
            //shouldn't try rendering axes or seres when the itemsource becomes null.
            if (FastItemsSource == null)
                return;

            RenderAxisOverride(animate);

            foreach (Series currentSeries in DirectSeries())
            {
                currentSeries.RenderSeries(animate);
            }
        }

        internal override bool UpdateRangeOverride()
        {
            long oldMin = ActualMinimumValue.Ticks;
            long oldMax = ActualMaximumValue.Ticks;

            long newMin = !MinimumValueIsSet()
                              ? ActualMinimumValue.Ticks
                              : MinimumValue.Ticks;

            long newMax = !MaximumValueIsSet()
                              ? ActualMaximumValue.Ticks
                              : MaximumValue.Ticks;

            AxisRangeChangedEventArgs ea =
                new AxisRangeChangedEventArgs(oldMin, newMin, oldMax, newMax);

            RaiseRangeChanged(ea);

            return true;
        }

        #region ISortingAxis Members

        List<int> ISortingAxis.SortedIndices
        {
            get
            {
                if (this.SortedDateTimeIndices == null)
                {
                    FastItemDateTimeColumn fastDateColumn = this.DateTimeColumn as FastItemDateTimeColumn;
                    if (fastDateColumn != null)
                    {
                        this.SortedDateTimeIndices = fastDateColumn.GetSortedIndices();
                    }
                    else
                    {
                        this.SortedDateTimeIndices = null;
                    }
                }
                return this.SortedDateTimeIndices;
            }
        }
        int ISortingAxis.GetFirstVisibleIndex(Rect windowRect, Rect viewportRect)
        {
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, IsInverted);
            double x0, x1;
            if (this.IsInverted)
            {
                x1 = this.GetUnscaledValue(viewportRect.Left, xParams);
                x0 = this.GetUnscaledValue(viewportRect.Right, xParams);
            }
            else
            {
                x0 = this.GetUnscaledValue(viewportRect.Left, xParams);
                x1 = this.GetUnscaledValue(viewportRect.Right, xParams);
            }
            int result = 0;
            for (int i = 0; i < this.SortedDateTimeIndices.Count; i++)
            {
                if (DateTimeColumn == null)
                {
                    break;
                }
                DateTime currentDateTime = this.DateTimeColumn[this.SortedDateTimeIndices[i]];
                //[MR - 10/22/10]: Changing comparison from > to >= because there can be an edge case,
                //where there are multiple values at the identical start date. Previously, only the last such value was used and others were ignored.
                //The correct behavior is to use the first point and let the series form a cluster.
                if (currentDateTime.Ticks >= x0)
                {
                    break;
                }
                result = i;
            }
            return result;
        }
        int ISortingAxis.GetLastVisibleIndex(Rect windowRect, Rect viewportRect)
        {
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, IsInverted);
            double x0, x1;
            if (this.IsInverted)
            {
                x1 = this.GetUnscaledValue(viewportRect.Left, xParams);
                x0 = this.GetUnscaledValue(viewportRect.Right, xParams);
            }
            else
            {
                x0 = this.GetUnscaledValue(viewportRect.Left, xParams);
                x1 = this.GetUnscaledValue(viewportRect.Right, xParams);
            }
            int last = this.SortedDateTimeIndices.Count - 1;
            int result = last;
            for (int i = last; i >= 0; i--)
            {
                if (DateTimeColumn == null || SortedDateTimeIndices.Count <= i)
                {
                    break;
                }

                int sortedIndex = SortedDateTimeIndices[i];
                if (sortedIndex >= DateTimeColumn.Count)
                {
                    break;
                }

                DateTime currentDateTime = DateTimeColumn[sortedIndex];
                if (currentDateTime.Ticks < x1)
                {
                    break;
                }
                result = Math.Min(i + 1, this.SortedDateTimeIndices.Count - 1);
            }
            return result;
        }
        double ISortingAxis.GetUnscaledValueAt(int index)
        {
            return this.DateTimeColumn == null ? Double.NaN : this.DateTimeColumn[index].Ticks;
        }

        int ISortingAxis.GetIndexClosestToUnscaledValue(double unscaledValue)
        {
            ISortingAxis sorting = this;
            if (this.DateTimeColumn == null || sorting.SortedIndices == null)
            {
                return -1;
            }
            var view = new SortedListView<DateTime>(this.DateTimeColumn, sorting.SortedIndices);
            




            DateTime target = new DateTime((long)unscaledValue);

            
            int res = -1;
            var result = view.BinarySearch((item) => 
                {
                    if (target < item) 
                    {
                        return -1;
                    }
                    if (target > item)
                    {
                        return 1;
                    }
                    return 0;
                });
            if (result >= 0)
            {
                res = result;
            }
            else
            {
                res = ~result;
            }

            if (res >= 0 && res < sorting.SortedIndices.Count &&
                res - 1 >= 0 && res - 1 < sorting.SortedIndices.Count)
            {
                TimeSpan diff1 = target - view[res - 1];
                TimeSpan diff2 = view[res] - target;
                if (diff1 < diff2)
                {
                    res = res - 1;
                }
            }

            if (res >= 0 && res < sorting.SortedIndices.Count)
            {
                return sorting.SortedIndices[res];
            }

            return -1;
        }
        #endregion

        /// <summary>
        /// Updates the axis when the data has been changed.
        /// </summary>
        public void NotifyDataChanged()
        {
            this.SortedDateTimeIndices = null;
            RenderAxis();
        }

        /// <summary>
        /// Gets the axis orientation.
        /// </summary>
        protected internal override AxisOrientation Orientation
        {
            get { return AxisOrientation.Horizontal; }
        }
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