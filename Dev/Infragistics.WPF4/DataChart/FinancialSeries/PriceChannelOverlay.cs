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
using System.Windows.Data;
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart Financial Price Channel Overlay series.
    /// </summary>
    /// <remarks>
    /// Default required members: High, Low
    /// </remarks>
    public class PriceChannelOverlay : FinancialOverlay
    {
        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a new instance of the PriceChannelOverlay class. 
        /// </summary>
        public PriceChannelOverlay()
        {
            DefaultStyleKey = typeof(PriceChannelOverlay);
            PreviousFrame = new CategoryFrame(3);
            TransitionFrame = new CategoryFrame(3);
            CurrentFrame = new CategoryFrame(3);
        }

        #endregion

        #region Period Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current PriceChannelOverlay object.
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
        public static readonly DependencyProperty PeriodProperty = DependencyProperty.Register(PeriodPropertyName, typeof(int), typeof(PriceChannelOverlay),
            new PropertyMetadata(14, (sender, e) =>
            {
                (sender as PriceChannelOverlay).RaisePropertyChanged(PeriodPropertyName, e.OldValue, e.NewValue);
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
            switch (propertyName)
            {
                case HighColumnPropertyName:
                case LowColumnPropertyName:
                    OverlayValid = false;
                    break;
            }

            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case PeriodPropertyName:
                    OverlayValid = false;
                    RenderSeries(false);
                    break;
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

            if (HighColumn == null ||
                LowColumn == null)
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Ensures the data for the overlay is calculated and valid before rendering.
        /// </summary>
        /// <returns>If rerendering is required.</returns>
        protected override bool ValidateOverlay()
        {
            ChannelTopColumn.Clear();
            ChannelBottomColumn.Clear();

            int period = (int)MathUtil.Clamp(Period, 0, FastItemsSource.Count);
            int count = Math.Min(HighColumn.Count, LowColumn.Count);

            IList<double> safeHigh = MakeReadOnlyAndEnsureSorted(HighColumn);
            IList<double> safeLow = MakeReadOnlyAndEnsureSorted(LowColumn);

            for (int i = 0; i < count; i++)
            {
                int ago = Math.Min(period, i);
                double highestHigh = double.MinValue;
                double lowestLow = double.MaxValue;
                for (int j = 0; j < ago; j++)
                {
                    if (!double.IsNaN(safeHigh[i - j]))
                    {
                        highestHigh = Math.Max(highestHigh, safeHigh[i - j]);
                    }
                    if (!double.IsNaN(safeLow[i - j]))
                    {
                        lowestLow = Math.Min(lowestLow, safeLow[i - j]);
                    }
                }

                if (i == 0)
                {
                    lowestLow = safeLow[0];
                    highestHigh = safeHigh[0];
                }

                ChannelTopColumn.Add(highestHigh);
                ChannelBottomColumn.Add(lowestLow);
            }

            return true;
        }

        internal override void PrepareFrame(CategoryFrame frame, FinancialSeriesView view)
        {
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted);

            CategoryAxisBase xaxis = XAxis;
            NumericYAxis yaxis = YAxis;

            frame.Buckets.Clear();
            frame.Markers.Clear();
            frame.Trend.Clear();

            double offset = 0.0;            // offset (pels) to the center of this categorySeries

            ISortingAxis sortingXAxis = XAxis as ISortingAxis;
            if (sortingXAxis != null && sortingXAxis.SortedIndices.Count != FastItemsSource.Count)
            {
                //mismatch in series and axis data sources.
                return;
            }

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
                    float bucketTop = Convert.ToSingle(ChannelTopColumn[i]);
                    float bucketBottom = Convert.ToSingle(ChannelBottomColumn[i]);

                    float currentTop = bucketTop;
                    float currentBottom = bucketBottom;

                    double currentX = bucketX;

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

                        //in a cluster of points, when bucketing, 
                        //we want to keep the smallest ChannelBottom value
                        //and the largest ChannelTop value among all points in the cluster
                        currentTop = Math.Max(bucketTop, Convert.ToSingle(ChannelTopColumn[i]));
                        currentBottom = Math.Min(bucketBottom, Convert.ToSingle(ChannelBottomColumn[i]));
                    }

                    if (!float.IsInfinity(currentBottom) &&
                    !float.IsInfinity(currentTop))
                    {
                        double xVal = double.NaN;
                        if (!double.IsNaN(bucketX))
                        {
                            xVal = this.XAxis.GetScaledValue(bucketX, xParams);
                        }

                        bucket = new float[] { Convert.ToSingle(xVal), currentBottom, currentTop };
                    }
                    else
                    {
                        bucket = new float[] { float.NaN, float.NaN, float.NaN };
                    }
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

            var priceChannelView = (PriceChannelOverlayView)view;
            if (priceChannelView != null)
            {
                priceChannelView.ClearRendering();
            }
        }

        internal override void RenderFrame(CategoryFrame frame, FinancialSeriesView view)
        {
            PriceChannelOverlayView priceChannelView = view as PriceChannelOverlayView;
            if (priceChannelView == null)
            {
                return;
            }
            priceChannelView.ClearRendering();

            int count = frame.Buckets.Count;
            Func<int, double> px = (i) => frame.Buckets[i][0];
            Func<int, double> nx = (i) => frame.Buckets[count - 1 - i][0];
            Func<int, double> y0 = (i) => frame.Buckets[i][1];
            Func<int, double> y1 = (i) => frame.Buckets[count - 1 - i][2];

            priceChannelView.Render(count, px, nx, y0, y1);

        }

        internal List<double> ChannelTopColumn = new List<double>();
        internal List<double> ChannelBottomColumn = new List<double>();
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new PriceChannelOverlayView(this);
        }
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            this.PriceChannelOverlayView = view as PriceChannelOverlayView;
        }
        private PriceChannelOverlayView PriceChannelOverlayView { get; set; }
     
    }

    internal class PriceChannelBucketCalculator
        : FinancialBucketCalculator
    {
        public PriceChannelBucketCalculator(FinancialSeriesView view)
            : base(view)
        {
            PriceChannelView = (PriceChannelOverlayView)view;
        }

        protected PriceChannelOverlayView PriceChannelView { get; set; }

        public override float[] GetBucket(int index)
        {
            int i0 = index * BucketSize;
            int i1 = Math.Min(i0 + BucketSize - 1, View.FinancialModel.FastItemsSource.Count - 1);

            if (i0 <= i1)
            {
                double highestHigh = double.MinValue;
                double lowestLow = double.MaxValue;
                int cnt = 0;

                for (int i = i0; i <= i1; ++i)
                {
                    if (!double.IsNaN(PriceChannelView.PriceChannelOverlayModel.ChannelTopColumn[i]) && !double.IsNaN(PriceChannelView.PriceChannelOverlayModel.ChannelBottomColumn[i]))
                    {
                        highestHigh = Math.Max(highestHigh, PriceChannelView.PriceChannelOverlayModel.ChannelTopColumn[i]);
                        lowestLow = Math.Min(lowestLow, PriceChannelView.PriceChannelOverlayModel.ChannelBottomColumn[i]);
                        ++cnt;
                    }
                }

                if (cnt > 0 &&
                    lowestLow != double.MaxValue &&
                    highestHigh != double.MinValue)
                {
                    return new float[]
                    { 
                        (float)(0.5 * (i0 + i1)),
                        (float)(lowestLow),
                        (float)(highestHigh),
                    };
                }
            }

            return new float[] { float.NaN, float.NaN, float.NaN };
        }
    }

    internal class PriceChannelOverlayView : FinancialSeriesView
    {
        private Polygon polygon = new Polygon();
        private Polyline polyline0 = new Polyline();
        private Polyline polyline1 = new Polyline();
        protected internal PriceChannelOverlay PriceChannelOverlayModel { get; set; }
        internal PriceChannelOverlayView(PriceChannelOverlay model)
            : base(model)
        {
            this.PriceChannelOverlayModel = model;
        }

        protected override FinancialBucketCalculator CreateBucketCalculator()
        {
            return new PriceChannelBucketCalculator(this);
        }

        internal void ClearRendering()
        {
            polygon.Points.Clear();
            polyline0.Points.Clear();
            polyline1.Points.Clear();
        }
        internal void Render(int count, Func<int, double> px, Func<int, double> nx, Func<int, double> y0, Func<int, double> y1)
        {


            foreach (int i in Flattener.Flatten(count, px, y0, this.Model.Resolution))
            {
                polygon.Points.Add(new Point(px(i), y0(i)));
                polyline0.Points.Add(new Point(px(i), y0(i)));
            }

            // strokePath fill and top line

            foreach (int i in Flattener.Flatten(count, nx, y1, this.Model.Resolution))
            {
                polygon.Points.Add(new Point(nx(i), y1(i)));
                polyline1.Points.Add(new Point(nx(i), y1(i)));
            }
        }
        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);
            polygon.Detach();
            polyline0.Detach();
            polyline1.Detach();

            rootCanvas.Children.Add(polygon);
            polygon.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = this.Model });
            polygon.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });

            rootCanvas.Children.Add(polyline0);
            polyline0.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsOutlineVisual(polyline0, true);

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