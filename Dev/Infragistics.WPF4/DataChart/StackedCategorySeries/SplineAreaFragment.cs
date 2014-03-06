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
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents one part of a StackedSplineAreaSeries.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class SplineAreaFragment : SplineFragmentBase
    {
        #region Constructor and Initalisation
        /// <summary>
        /// Initializes a new instance of the SplineAreaFragment class. 
        /// </summary>
        internal SplineAreaFragment()
        {
            DefaultStyleKey = typeof(SplineAreaFragment);
        }
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new SplineAreaFragmentView(this);
        }
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            this.SplineAreaFragmentView = view as SplineAreaFragmentView;
        }
        private SplineAreaFragmentView SplineAreaFragmentView { get; set; }
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RenderSeries(false);
        }
        #endregion

        internal PointCollection Points { get; set; }
        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var splineAreaFragmentView = (SplineAreaFragmentView)view;

            splineAreaFragmentView.ClearRendering();
        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);

            Func<int, double> x0 = delegate(int i) { return frame.Buckets[i][0]; };
            Func<int, double> y0 = delegate(int i) { return frame.Buckets[i][1]; };
            Func<int, double> x1 = delegate(int i) { return frame.Buckets[i][0]; };
            Func<int, double> y1 = delegate(int i) { return frame.Buckets[i][2]; };

            //Make the rasterizer aware of the possible date time axis
            LineRasterizer.IsSortingAxis = XAxis is ISortingAxis ? true : false;

            SplineAreaFragmentView splineView = view as SplineAreaFragmentView;
            int bucketSize = view.BucketCalculator.BucketSize;
            LineRasterizer.RasterizePolygonPaths(splineView.polygon0, splineView.polyline0, splineView.polygon1, splineView.polyline1, frame.Buckets.Count, x0, y0, x1, y1, bucketSize, Resolution, (p0, l0, p1, l1, f) => TerminatePolygon(p0, x0, y1, view), UnknownValuePlotting.DontPlot);
        }

        internal void TerminatePolygon(PointCollection polygon, Func<int,double> x0, Func<int, double> y1, CategorySeriesView view)
        {
            double worldZeroValue = GetWorldZeroValue(view);
            double zero = worldZeroValue;
            bool positive = LogicalSeriesLink.Positive;
            ObservableCollection<AnchoredCategorySeries> seriesCollection = positive ? ParentSeries.StackedSeriesManager.PositiveSeries : ParentSeries.StackedSeriesManager.NegativeSeries;
            int seriesIndex = seriesCollection.IndexOf(this);

            if (polygon.Count == 0) return;
            if (seriesIndex == -1) return;

            //find the previous valid series
            bool foundValidSeries = false;

            for (int index = seriesIndex; index >= 0; index--)
            {
                if (foundValidSeries) break;

                if (index == 0)
                {
                    polygon.Add(new Point(polygon.Last().X, zero));
                    polygon.Add(new Point(polygon.First().X, zero));
                    break;
                }

                SplineFragmentBase previousSeries = seriesCollection[index - 1] as SplineFragmentBase;
                if (previousSeries != null
                    && previousSeries.LineRasterizer != null
                    && previousSeries.LineRasterizer.FlattenedLinePoints.Count > 0
                    && View != null
                    && previousSeries.ValidateSeries(View.Viewport, View.WindowRect, View))
                {
                    foundValidSeries = true;
                    for (int i = previousSeries.LineRasterizer.FlattenedLinePoints.Count - 1; i >= 0; i--)
                    {
                        polygon.Add(previousSeries.LineRasterizer.FlattenedLinePoints[i]);
                    }
                }
            }
        }

        internal override double GetWorldZeroValue(CategorySeriesView view)
        {
            double value = 0.0;

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted);

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && YAxis != null)
            {
                value = YAxis.GetScaledValue(0.0, yParams);
            }

            return value;
        }

    }
    internal class SplineAreaFragmentView : SplineSeriesBaseView
    {
        private SplineAreaFragment SplineAreaFragmentModel { get; set; }
        public SplineAreaFragmentView(SplineAreaFragment model)
            : base(model)
        {
            this.SplineAreaFragmentModel = model;
        }
        internal Path polygon0 = new Path();
        internal Path polyline0 = new Path();
        internal Path polygon1 = new Path();
        internal Path polyline1 = new Path();
        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            polygon0.Detach();
            polygon1.Detach();
            polyline0.Detach();
            polyline1.Detach();

            rootCanvas.Children.Add(polygon0);
            polygon0.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = this.Model });

            rootCanvas.Children.Add(polygon1);
            polygon1.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = this.Model });
            polygon1.Opacity = 0.5;
            VisualInformationManager.SetIsTranslucentPortionVisual(polygon1, true);

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

            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(Series.StartCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(Series.EndCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = this.Model });

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

            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(Series.StartCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(Series.EndCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = this.Model });
        }
        internal void ClearRendering()
        {
            polygon0.Data = null;
            polygon1.Data = null;
            polyline0.Data = null;
            polyline1.Data = null;
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