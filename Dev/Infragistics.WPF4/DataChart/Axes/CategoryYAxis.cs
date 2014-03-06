using System;



using System.Linq;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart category Y axis.
    /// </summary>
    [WidgetModule("CategoryChart")]
    public class CategoryYAxis : CategoryAxisBase
    {
        internal override AxisView CreateView()
        {
            return new CategoryYAxisView(this);
        }
        internal override void OnViewCreated(AxisView view)
        {
            base.OnViewCreated(view);
            YView = (CategoryYAxisView)view;
        }
        internal CategoryYAxisView YView { get; set; }

        #region Constructor and Initalization
        /// <summary>
        /// Initializes a new instance of the CategoryYAxis class. 
        /// </summary>
        public CategoryYAxis()
        {
            this.LabelSettings = new AxisLabelSettings() { Location = AxisLabelsLocation.OutsideLeft, Axis = this };
        }
        #endregion

        #region Interval dependency property
        private const string IntervalPropertyName = "Interval";
        /// <summary>
        /// Identifies the Interval dependency property.
        /// </summary>
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(IntervalPropertyName, typeof(double), typeof(CategoryYAxis), new PropertyMetadata(double.NaN,
            (sender, e) =>
            {
                (sender as CategoryYAxis).RaisePropertyChanged(IntervalPropertyName, e.OldValue, e.NewValue);
                (sender as CategoryYAxis).RenderAxis(false);
            }));

        /// <summary>
        /// Gets or sets the frequency of displayed labels.
        /// </summary>
        /// <remarks>
        /// The set value is a factor that determines which labels will be hidden. For example, an interval of 2 will display every other label.
        /// </remarks>
        public double Interval
        {
            get
            {
                return (double)this.GetValue(IntervalProperty);
            }
            set
            {
                this.SetValue(IntervalProperty, value);
            }
        }
        #endregion

        #region Internal and private properties
        private int _actualMinimum = 1;
        internal int ActualMinimum
        {
            get
            {
                return _actualMinimum;
            }
            set
            {
                _actualMinimum = value;
            }
        }

        private int _actualMaximum = 1;
        internal int ActualMaximum
        {
            get
            {
                return _actualMaximum;
            }
            set
            {
                _actualMaximum = value;
            }
        }
        #endregion

        #region Method overrides

        /// <summary>
        /// Gets the scaled viewport coordinate value from the unscaled axis value.
        /// </summary>
        /// <param name="unscaledValue">The unscaled axis value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>The scaled value.</returns>
        public override double GetScaledValue(double unscaledValue, ScalerParams p)
        {
            int itemCount = this.CategoryMode == CategoryMode.Mode0 ? this.ItemsCount - 1 : this.ItemsCount;
            if (itemCount < 0)
            {
                itemCount = 0;
            }

            double scaledValue =
                itemCount >= 1 ? (unscaledValue) / (double)(itemCount)
                : itemCount == 0 ? 0.5
                : double.NaN;

            if (!IsInvertedCached)
            {
                scaledValue = 1.0 - scaledValue;
            }

            return p.ViewportRect.Top + p.ViewportRect.Height * (scaledValue - p.WindowRect.Top) / p.WindowRect.Height;
        }

        /// <summary>
        /// Gets the unscaled axis value from a scaled viewport value.
        /// </summary>
        /// <param name="scaledValue">The scaled viewport value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>The unscaled axis value.</returns>
        public override double GetUnscaledValue(double scaledValue, ScalerParams p)
        {
            return this.GetUnscaledValue(scaledValue, p.WindowRect, p.ViewportRect, this.CategoryMode);
        }

        internal override double GetUnscaledValue(double scaledValue, Rect windowRect, Rect viewportRect, CategoryMode categoryMode)
        {
            double unscaledValue = windowRect.Top + (scaledValue - viewportRect.Top) * windowRect.Height / viewportRect.Height;

            if (!IsInvertedCached)
            {
                unscaledValue = 1.0 - unscaledValue;
            }

            int itemCount = categoryMode == CategoryMode.Mode0 ? this.ItemsCount - 1 : this.ItemsCount;
            if (itemCount < 0)
            {
                itemCount = 0;
            }
            return unscaledValue * itemCount;
        }

        internal override AxisLabelPanelBase CreateLabelPanel()
        {
            return new VerticalAxisLabelPanel();
        }

        internal override double GetCategorySize(Rect windowRect, Rect viewportRect)
        {
            return viewportRect.Height / (ItemsCount * windowRect.Height);
        }
        internal override double GetGroupSize(Rect windowRect, Rect viewportRect)
        {
            double gap = 0.0;
            if (!double.IsNaN(Gap))
            {
                gap = MathUtil.Clamp(Gap, 0.0, 1.0);
            }
            double overlap = 0.0;
            if (!double.IsNaN(Overlap))
            {
                overlap = Math.Min(Overlap, 1);
            }
            double categorySpace = 1.0 - 0.5 * gap;

            double ret = GetCategorySize(windowRect, viewportRect) * categorySpace / (Mode2GroupCount - (Mode2GroupCount - 1) * overlap);

            return Math.Max(ret, 1.0);



        }
        internal override double GetGroupCenter(int groupIndex, Rect windowRect, Rect viewportRect)
        {
            double groupCenter = 0.5;

            if (Mode2GroupCount > 1)
            {
                double gap = 0.0;
                if (!double.IsNaN(Gap))
                {
                    gap = MathUtil.Clamp(Gap, 0.0, 1.0);
                }
                double overlap = 0.0;
                if (!double.IsNaN(Overlap))
                {
                    overlap = Math.Min(Overlap, 1);
                }
                double categorySpace = 1.0 - 0.5 * gap;
                double groupWidth = categorySpace / (Mode2GroupCount - (Mode2GroupCount - 1) * overlap);
                double groupSep = (categorySpace - groupWidth) / (Mode2GroupCount - 1);

                groupCenter = 0.25 * gap + 0.5 * groupWidth + groupIndex * groupSep;
            }

            return GetCategorySize(windowRect, viewportRect) * groupCenter;
        }

        /// <summary>
        /// Scrolls the specified item into view.
        /// </summary>
        /// <param name="item">Data item to scroll into view</param>
        public void ScrollIntoView(object item)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = ViewportRect;
            Rect unitRect = new Rect(0, 0, 1, 1);
            ScalerParams yParams = new ScalerParams(unitRect, unitRect, IsInverted);

            int index = !windowRect.IsEmpty
                && !viewportRect.IsEmpty
                && FastItemsSource != null ? FastItemsSource.IndexOf(item) : -1;
            double cy = index > -1 ? GetScaledValue(index, yParams) : double.NaN;

            if (!double.IsNaN(cy) && SeriesViewer.IsSyncReady)
            {
                if (!double.IsNaN(cy))
                {
                    if (cy < windowRect.Top + 0.1 * windowRect.Height)
                    {
                        cy = cy + 0.4 * windowRect.Height;
                        windowRect.Y = cy - 0.5 * windowRect.Height;
                    }

                    if (cy > windowRect.Bottom - 0.1 * windowRect.Height)
                    {
                        cy = cy - 0.4 * windowRect.Height;
                        windowRect.Y = cy - 0.5 * windowRect.Height;
                    }
                }

                SeriesViewer.WindowNotify(windowRect);
            }
        }

        internal override bool UpdateRangeOverride()
        {
            if (FastItemsSource == null)
            {
                return false;
            }

            int max = FastItemsSource.Count;

            if (max != ActualMaximum)
            {
                AxisRangeChangedEventArgs ea = new AxisRangeChangedEventArgs(1, 1, ActualMaximum, max);
                ActualMaximum = max;
                RaiseRangeChanged(ea);

                return true;
            }

            return false;
        }

        internal override bool ShouldShareMode(SeriesViewer chart)
        {
            if (chart == null)
            {
                return false;
            }

            SyncSettings settings = GetSyncSettings();
            if (settings == null)
            {
                return false;
            }

            return settings.SynchronizeVertically;
        }
        #endregion

        #region Render override
        /// <summary>
        /// Renders or updates the axis visuals.
        /// </summary>
        /// <param name="animate">Whether the visual changes should be animated.</param>
        protected override void RenderAxisOverride(bool animate)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = ViewportRect;

            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, IsInverted);

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

            View.UpdateLabelPanel(this, windowRect, viewportRect);
            if (windowRect.IsEmpty || viewportRect.IsEmpty)
            {
                TextBlocks.Count = 0;
            }
            if (TextBlocks.Count == 0)
            {
                View.ClearLabelPanel();
            }
            //foreach (TextBlock tb in
            //   LabelPanel.Children.OfType<TextBlock>())
            //{
            //    AxisLabelManager.UnbindLabel(tb);
            //}
            //this.LabelPanel.Children.Clear();

            if (this.LabelSettings != null)
            {
                this.LabelSettings.Axis = this;
            }

            if (ItemsSource == null) return;

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty)
            {
                double visibleMinimum = GetUnscaledValue(viewportRect.Top, yParams);
                double visibleMaximum = GetUnscaledValue(viewportRect.Bottom, yParams);

                if (!IsInverted)
                {
                    visibleMinimum = Math.Ceiling(visibleMinimum);
                    visibleMaximum = Math.Floor(visibleMaximum);
                }
                else
                {
                    visibleMinimum = Math.Floor(visibleMinimum);
                    visibleMaximum = Math.Ceiling(visibleMaximum);
                }

                #region Draw Axis Line
                //double crossingValue = (this.LabelSettings.Location == AxisLabelsLocation.OutsideTop
                //    || this.LabelSettings.Location == AxisLabelsLocation.InsideTop) ?
                //    viewportRect.Top : viewportRect.Bottom;

                //this axis is always at the bottom unless crossing axis is set
                double crossingValue = viewportRect.Left;


                if (CrossingAxis != null)
                {
                    NumericXAxis xAxis = this.CrossingAxis as NumericXAxis;
                    if (xAxis != null)
                    {
                        ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xAxis.IsInverted);



                        crossingValue = Convert.ToDouble(CrossingValue);

                        crossingValue = xAxis.GetScaledValue(crossingValue, xParams);
                        if (crossingValue < viewportRect.Left)
                        {
                            crossingValue = viewportRect.Left;
                        }
                        else if (crossingValue > viewportRect.Right)
                        {
                            crossingValue = viewportRect.Right;
                        }
                    }
                }
                VerticalLine(axisGeometry, crossingValue, viewportRect);
                View.SetLabelPanelCrossingValue(crossingValue);
                #endregion

                #region Linear Strips and Lines
                double trueVisibleMinimum = Math.Min(visibleMinimum, visibleMaximum);
                double trueVisibleMaximum = Math.Max(visibleMinimum, visibleMaximum);

                LinearCategorySnapper snapper = new LinearCategorySnapper(
                    trueVisibleMinimum, 
                    trueVisibleMaximum, 
                    viewportRect.Height, 
                    Interval, 
                    CategoryMode);

                double firstValue = Math.Floor((trueVisibleMinimum - 0) / snapper.Interval);
                double lastValue = Math.Ceiling((trueVisibleMaximum - 0) / snapper.Interval);

                if (!double.IsNaN(firstValue) && !double.IsNaN(lastValue))
                {
                    int first = (int)firstValue;
                    int last = (int)lastValue;

                    double majorValue = GetScaledValue(0 + first * snapper.Interval, yParams);

                    this.LabelPanel.Interval = GetScaledValue(snapper.Interval, yParams);

                    //DataTemplate labelTemplate = this.Label as DataTemplate;
                    //StringFormatter labelFormatter = labelTemplate != null ? null : new StringFormatter() { FormatString = this.Label as string };

                    for (int i = first; i <= last; ++i)
                    {
                        double nextMajorValue = GetScaledValue(0 + (i + 1) * snapper.Interval, yParams);

                        if (majorValue <= viewportRect.Bottom)
                        {
                            if (i % 2 == 0)
                            {
                                HorizontalStrip(stripsGeometry, majorValue, nextMajorValue, viewportRect);
                            }

                            HorizontalLine(majorGeometry, majorValue, viewportRect);

                            if (CategoryMode != Charts.CategoryMode.Mode0 && Mode2GroupCount != 0)
                            {
                                for (int categoryNumber = 0; categoryNumber < (int)snapper.Interval; categoryNumber++)
                                {
                                    //display a minor line in te middle of each group.
                                    for (int groupNumber = 0; groupNumber < Mode2GroupCount; groupNumber++)
                                    {
                                        double center = GetGroupCenter(groupNumber, windowRect, viewportRect);
                                        if (!IsInverted) center = -center;
                                        double minorValue = GetScaledValue(categoryNumber + i * snapper.Interval, yParams) + center;
                                        HorizontalLine(minorGeometry, minorValue, viewportRect);
                                    }
                                }
                            }
                        }

                        double categoryValue = majorValue;
                        if (CategoryMode != CategoryMode.Mode0)
                        {
                            double nextCategoryValue = GetScaledValue(i * snapper.Interval + 1, yParams);
                            categoryValue = (majorValue + nextCategoryValue) / 2;
                        }

                        if (categoryValue <= viewportRect.Bottom && categoryValue >= viewportRect.Top)
                        {
                            int itemIndex = 0;
                            if (snapper.Interval >= 1)
                            {
                                itemIndex = i * (int)Math.Floor(snapper.Interval);
                            }
                            else
                            {
                                if ((i * snapper.Interval) * 2 % 2 == 0)
                                {
                                    itemIndex = (int)Math.Floor(i * snapper.Interval);
                                }
                                else
                                {
                                    itemIndex = -1;
                                }
                            }

                            if (this.FastItemsSource != null && itemIndex < this.FastItemsSource.Count && itemIndex >= 0)
                            {
                                object dataItem = FastItemsSource[itemIndex];






                                object labelText = GetLabel(dataItem);


                                if (!double.IsNaN(categoryValue) && !double.IsInfinity(categoryValue))
                                {
                                    LabelDataContext.Add(labelText);
                                    LabelPositions.Add(new LabelPosition(categoryValue));
                                }
                            }
                        }

                        majorValue = nextMajorValue;
                    }
                }
                #endregion

                #region Draw Floating Axis Panel
                if ((LabelSettings == null 
                    || this.LabelSettings.Visibility == Visibility.Visible)
                    && this.CrossingAxis != null)
                {
                    if (LabelSettings != null &&
                        (this.LabelSettings.Location == AxisLabelsLocation.InsideLeft ||
                        this.LabelSettings.Location == AxisLabelsLocation.InsideRight))
                    {
                        this.SeriesViewer.InvalidatePanels();
                    }
                }
                #endregion

                #region Linear Labels

                View.UpdateLabelPanelContent(LabelDataContext, LabelPositions);
                RenderLabels();
                #endregion
            }
        }
        #endregion

        #region Property Overrides
        /// <summary>
        /// Gets the axis orientation.
        /// </summary>
        protected internal override AxisOrientation Orientation
        {
            get { return AxisOrientation.Vertical; }
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