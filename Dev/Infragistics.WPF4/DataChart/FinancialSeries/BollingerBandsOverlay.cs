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
using Infragistics.Controls.Charts.Util;
using System.Windows.Data;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart Bollinger Bands financial overlay series.
    /// </summary>
    /// <remarks>
    /// Default required members: High, Low, Close
    /// </remarks>
    public class BollingerBandsOverlay : FinancialOverlay
    {
        #region constructor and initialisation
        /// <summary>
        /// Initializes a new instance of the BollingerBandsOverlay class. 
        /// </summary>
        public BollingerBandsOverlay()
        {
            DefaultStyleKey = typeof(BollingerBandsOverlay);
            PreviousFrame = new CategoryFrame(4);
            TransitionFrame = new CategoryFrame(4);
            CurrentFrame = new CategoryFrame(4);
        }

        #endregion

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current BollingerBandOverlay object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for Bollinger band periods is 14.
        /// </remarks>
        /// </summary>
        public int Period
        {
            get
            {
                return (int)GetValue(PeriodProperty);
            }
            set
            {
                SetValue(PeriodProperty, value);
            }
        }

        internal const string PeriodPropertyName = "Period";

        /// <summary>
        /// Identifies the Period dependency property.
        /// </summary>
        public static readonly DependencyProperty PeriodProperty = DependencyProperty.Register(PeriodPropertyName, typeof(int), typeof(BollingerBandsOverlay),
            new PropertyMetadata(14, (sender, e) =>
            {
                (sender as BollingerBandsOverlay).RaisePropertyChanged(PeriodPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region Multiplier Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current BollingerBandOverlay object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for Bollinger band multipliers is 2.
        /// </remarks>
        /// </summary>
        public double Multiplier
        {
            get
            {
                return (double)GetValue(MultiplierProperty);
            }
            set
            {
                SetValue(MultiplierProperty, value);
            }
        }

        internal const string MultiplierPropertyName = "Multiplier";

        /// <summary>
        /// Identifies the Multiplier dependency property.
        /// </summary>
        public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(MultiplierPropertyName, typeof(double), typeof(BollingerBandsOverlay),
            new PropertyMetadata(2.0, (sender, e) =>
            {
                (sender as BollingerBandsOverlay).RaisePropertyChanged(MultiplierPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            if (GetTypicalBasedOn().Contains(propertyName))
            {
                OverlayValid = false;
            }

            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case PeriodPropertyName:
                    OverlayValid = false;
                    RenderSeries(false);
                    break;

                case MultiplierPropertyName:
                    RenderSeries(false);  // everything's valid, just need to animate to the next frame
                    break;
            }
        }

        private double _maxBandWidth;
        private double _minBandWidth;

        /// <summary>
        /// Ensures the data for the overlay is calculated and valid before rendering.
        /// </summary>
        /// <returns>If rerendering is required.</returns>
        protected override bool ValidateOverlay()
        {
            AverageColumn.Clear();
            DeviationColumn.Clear();

            IEnumerator<double> sma = Series.SMA(new SafeEnumerable(TypicalColumn), Period).GetEnumerator();
            IEnumerator<double> stdev = Series.STDEV(new SafeEnumerable(TypicalColumn), Period).GetEnumerator();

            _minBandWidth = double.MaxValue;
            _maxBandWidth = double.MinValue;

            bool moreSma = true;
            bool moreStdev = true;
            double multiplier = Multiplier;

            while (moreSma || moreStdev)
            {
                if (sma.MoveNext())
                {
                    AverageColumn.Add(sma.Current);
                }
                else
                {
                    moreSma = false;
                }

                if (stdev.MoveNext())
                {
                    DeviationColumn.Add(stdev.Current);
                }
                else
                {
                    moreStdev = false;
                }

                if (moreSma && moreStdev)
                {
                    _minBandWidth = Math.Min(
                        _minBandWidth,
                        sma.Current - stdev.Current * multiplier);

                    _maxBandWidth = Math.Max(
                        _maxBandWidth,
                        sma.Current + stdev.Current * multiplier);
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (AverageColumn == null ||
                DeviationColumn == null ||
                axis == null ||
                FastItemsSource == null ||
                axis != YAxis)
            {
                return null;
            }

            AxisRange range = new AxisRange(
                _minBandWidth,
                _maxBandWidth);

            return range;
        }

        internal List<double> AverageColumn = new List<double>();
        internal List<double> DeviationColumn = new List<double>();

      

        internal override void PrepareFrame(CategoryFrame frame, FinancialSeriesView view)
        {
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            CategoryAxisBase xaxis = XAxis;
            NumericYAxis yaxis = YAxis;

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xaxis.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yaxis.IsInverted);

            frame.Buckets.Clear();
            frame.Markers.Clear();
            frame.Trend.Clear();

            ISortingAxis sortingXAxis = XAxis as ISortingAxis;
            if (sortingXAxis != null && sortingXAxis.SortedIndices.Count != FastItemsSource.Count)
            {
                //mismatch in series and axis data sources.
                return;
            }

            double offset = 0.0;            // offset (pels) to the center of this categorySeries

            #region work out the category mode and offset
            CategoryMode categoryMode = XAxis.CategoryMode;

            switch (categoryMode)
            {
                case CategoryMode.Mode0:    // use bucket.X as-is
                    offset = 0.0;
                    break;

                case CategoryMode.Mode1:    // offset x by half category width
                    offset = 0.5 * XAxis.GetCategorySize(windowRect, viewportRect);
                    break;

                case CategoryMode.Mode2:    // offset x by the appropriate amount for this categorySeries
                    int index = Index;
                    offset = XAxis.GetGroupCenter(Index, windowRect, viewportRect);
                    break;
            }
            #endregion

            if (!OverlayValid)
            {
                OverlayValid = ValidateOverlay();
                if (YAxis != null)
                {
                    YAxis.UpdateRange();
                }
            }

            #region bucketize data
            float singlePixelSpan = Convert.ToSingle(this.XAxis.GetUnscaledValue(2.0, xParams) - this.XAxis.GetUnscaledValue(1.0, xParams));

            for (int i = view.BucketCalculator.FirstBucket; i <= view.BucketCalculator.LastBucket; ++i)
            {
                float[] bucket;
                if (sortingXAxis == null)
                {
                    //index based bucketing
                    bucket = view.BucketCalculator.GetBucket(i);
                }
                else
                {
                    // SortedAxis based bucketing (for CategoryDateTimeXAxis)
                    int index = sortingXAxis.SortedIndices[i];
                    double bucketX = sortingXAxis.GetUnscaledValueAt(index);
                    float bucketAverage = Convert.ToSingle(AverageColumn[i]);
                    float bucketDeviation = Convert.ToSingle(DeviationColumn[i]);

                    float currentAverage = bucketAverage;
                    float currentDeviation = bucketDeviation;

                    double currentX = bucketX;
                    int counter = 1;

                    while (i < view.BucketCalculator.LastBucket)
                    {
                        index = sortingXAxis.SortedIndices[i + 1];
                        currentX = sortingXAxis.GetUnscaledValueAt(index);
                        if (currentX - bucketX > singlePixelSpan)
                        {
                            // next item does not belong in this bucket
                            break;
                        }

                        // add next item to this bucket
                        i++;

                        //calculate the averages for Average and Deviation in the cluster
                        currentAverage += Convert.ToSingle(AverageColumn[i]);
                        currentDeviation += Convert.ToSingle(DeviationColumn[i]);
                        counter++;
                    }

                    currentAverage /= counter;
                    currentDeviation /= counter;

                    float param0 = Convert.ToSingle(currentAverage - currentDeviation * Multiplier);
                    float param1 = Convert.ToSingle(currentAverage);
                    float param2 = Convert.ToSingle(currentAverage + currentDeviation * Multiplier);

                    double xVal = double.NaN;
                    if (!double.IsNaN(bucketX))
                    {
                        xVal = this.XAxis.GetScaledValue(bucketX, xParams);
                    }

                    bucket = new float[] { Convert.ToSingle(xVal), param0, param1, param2 };
                }

                double pp = Math.Max(1.0, singlePixelSpan);

                if (!float.IsNaN(bucket[0]) && 
                    i * pp >= this.IgnoreFirst)
                {
                    if (XAxis != null && XAxis is ISortingAxis)
                    {
                        bucket[0] = (float)(bucket[0] + offset);
                    }
                    else
                    {
                        bucket[0] = (float)(xaxis.GetScaledValue(bucket[0], xParams) + offset);
                    }
                    bucket[1] = (float)yaxis.GetScaledValue(bucket[1], yParams);
                    bucket[2] = (float)yaxis.GetScaledValue(bucket[2], yParams);
                    bucket[3] = (float)yaxis.GetScaledValue(bucket[3], yParams);

                    frame.Buckets.Add(bucket);
                }
            }
            #endregion

        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);
            var bollingerView = (BollingerBandsOverlayView)view;
            if (bollingerView != null)
            {
                bollingerView.ClearRendering();
            }

        }

        /// <summary>
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <param name="view">The SeriesView in context.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal override bool ValidateSeries(Rect viewportRect, Rect windowRect, SeriesView view)
        {
            bool isValid = base.ValidateSeries(viewportRect, windowRect, view);

            if (!ValidateBasedOn(GetTypicalBasedOn()))
            {
                isValid = false;
            }

            return isValid;
        }

        internal override void RenderFrame(CategoryFrame frame, FinancialSeriesView view)
        {
            BollingerBandsOverlayView bollingerBandsView = view as BollingerBandsOverlayView;
            bollingerBandsView.ClearRendering();

            int count = frame.Buckets.Count;

            Func<int, double> px = (i) => frame.Buckets[i][0];
            Func<int, double> nx = (i) => frame.Buckets[count - 1 - i][0];
            Func<int, double> y0 = (i) => frame.Buckets[i][1];
            Func<int, double> y1 = (i) => frame.Buckets[i][2];
            Func<int, double> y2 = (i) => frame.Buckets[count - 1 - i][3];
   
            bollingerBandsView.Render(count, px, nx, y0, y1, y2);
        }
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new BollingerBandsOverlayView(this);
        }
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            this.BollingerBandsView = view as BollingerBandsOverlayView;
        }
        private BollingerBandsOverlayView BollingerBandsView { get; set; }
    }

    internal class BollingerBandsBucketCalculator : FinancialBucketCalculator
    {
        public BollingerBandsBucketCalculator(FinancialSeriesView view)
            : base(view)
        {
            BollingerView = (BollingerBandsOverlayView)view;
        }
        protected BollingerBandsOverlayView BollingerView { get; set; }

        public override float[] GetBucket(int index)
        {
            int i0 = index * BucketSize;
            int i1 = Math.Min(i0 + BucketSize - 1, View.FinancialModel.FastItemsSource.Count - 1);

            if (i0 <= i1)
            {
                double multiplier = BollingerView.BollingerBandsOverlayModel.Multiplier;
                double average = 0.0;
                double deviation = 0.0;
                int cnt = 0;

                for (int i = i0; i <= i1; ++i)
                {
                    if (!double.IsNaN(BollingerView.BollingerBandsOverlayModel.AverageColumn[i]) && !double.IsNaN(BollingerView.BollingerBandsOverlayModel.DeviationColumn[i]))
                    {
                        average += BollingerView.BollingerBandsOverlayModel.AverageColumn[i];
                        deviation += BollingerView.BollingerBandsOverlayModel.DeviationColumn[i];
                        ++cnt;
                    }
                }

                if (cnt > 0)
                {
                    average = average / (double)cnt;
                    deviation = deviation / (double)cnt;

                    return new float[]
                    { 
                        (float)(0.5 * (i0 + i1)),
                        (float)(average-deviation*multiplier),
                        (float)(average),
                        (float)(average+deviation*multiplier),
                    };
                }
            }

            return new float[] { float.NaN, float.NaN, float.NaN, float.NaN };
        }
    }

    internal class BollingerBandsOverlayView : FinancialSeriesView
    {
        protected internal BollingerBandsOverlay BollingerBandsOverlayModel { get; set; }
        internal BollingerBandsOverlayView(BollingerBandsOverlay model)
            : base(model)
        {
            this.BollingerBandsOverlayModel = model;
        }

        protected override FinancialBucketCalculator CreateBucketCalculator()
        {
            return new BollingerBandsBucketCalculator(this);
        }

        internal void Render(int count, Func<int, double> px, Func<int, double> nx, Func<int, double> y0, Func<int, double> y1, Func<int, double> y2)
        {
            foreach (int i in Flattener.Flatten(count, px, y0, this.Model.Resolution))
            {
                polygon.Points.Add(new Point(px(i), y0(i)));
                polyline0.Points.Add(new Point(px(i), y0(i)));
            }

            foreach (int i in Flattener.Flatten(count, px, y1, this.Model.Resolution))
            {
                polyline1.Points.Add(new Point(px(i), y1(i)));
            }

            // strokePath fill and top line

            foreach (int i in Flattener.Flatten(count, nx, y2, this.Model.Resolution))
            {
                polygon.Points.Add(new Point(nx(i), y2(i)));
                polyline2.Points.Add(new Point(nx(i), y2(i)));
            }
        }
        internal void ClearRendering()
        {
            polygon.Points.Clear();
            polyline0.Points.Clear();
            polyline1.Points.Clear();
            polyline2.Points.Clear();
        }
        private Polygon polygon = new Polygon();    // fill
        private Polyline polyline0 = new Polyline(); // bottom line
        private Polyline polyline1 = new Polyline(); // central line
        private Polyline polyline2 = new Polyline(); // top line

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);
            polygon.Detach();
            polyline0.Detach();
            polyline1.Detach();
            polyline2.Detach();

            rootCanvas.Children.Add(polygon);
            polygon.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = this.Model });

            rootCanvas.Children.Add(polyline0);
            polyline0.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = this.Model });
            VisualInformationManager.SetIsOutlineVisual(polyline0, true);
            VisualInformationManager.SetIsSolidOutlineVisual(polyline0, true);
            VisualInformationManager.SetIsMainGeometryVisual(polyline0, true);

            rootCanvas.Children.Add(polyline1);
            polyline1.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsOutlineVisual(polyline1, true);

            rootCanvas.Children.Add(polyline2);
            polyline2.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = this.Model });
            polyline2.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = this.Model });
            VisualInformationManager.SetIsOutlineVisual(polyline2, true);
            VisualInformationManager.SetIsSolidOutlineVisual(polyline2, true);
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