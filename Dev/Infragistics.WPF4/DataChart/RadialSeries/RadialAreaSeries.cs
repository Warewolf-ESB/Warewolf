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
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart radial area seres.
    /// </summary>
    public sealed class RadialAreaSeries
        : AnchoredRadialSeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The view that was created.</returns>
        protected override SeriesView CreateView()
        {
            return new RadialAreaSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            RadialAreaView = (RadialAreaSeriesView)view;
        }
        internal RadialAreaSeriesView RadialAreaView { get; set; }

        #region Constructor and Initalisation
        /// <summary>
        /// Initializes a new instance of the RadialAreaSeries class. 
        /// </summary>
        public RadialAreaSeries()
        {
            DefaultStyleKey = typeof(RadialAreaSeries);
        }

        #endregion

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode0;
        }


        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var radialAreaView = (RadialAreaSeriesView)view;

            radialAreaView.polygon0.Data = null;
            radialAreaView.polygon1.Data = null;
            radialAreaView.polyline0.Data = null;
            radialAreaView.polyline1.Data = null;
        }

        internal override void RenderFrame(RadialFrame frame, RadialBaseView view)
        {
            base.RenderFrame(frame, view);

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            var areaView = (RadialAreaSeriesView)view;

            if (view.HasSurface() && !windowRect.IsEmpty && !viewportRect.IsEmpty && AngleAxis != null && ValueAxis != null)
            {
                Func<int, double> x0 = delegate(int i)
                {
                    return _axes.GetXValue(frame.Buckets[i][0],
                        frame.Buckets[i][1], windowRect, viewportRect, Math.Cos);
                };
                Func<int, double> y0 = delegate(int i)
                {
                    return _axes.GetYValue(frame.Buckets[i][0],
                        frame.Buckets[i][1], windowRect, viewportRect, Math.Sin);
                };
                Func<int, double> x1 = delegate(int i)
                {
                    return _axes.GetXValue(frame.Buckets[i][0],
                    frame.Buckets[i][2],
                    windowRect, viewportRect, Math.Cos);
                };
                Func<int, double> y1 = delegate(int i)
                {
                    return _axes.GetYValue(frame.Buckets[i][0],
                        frame.Buckets[i][2],
                        windowRect, viewportRect, Math.Sin);
                };

                double centerX = ViewportUtils.TransformXToViewport(.5, windowRect, viewportRect);
                double centerY = ViewportUtils.TransformYToViewport(.5, windowRect, viewportRect);
                _terminationPoint = new Point(centerX, centerY);

                LineRasterizer.RasterizePolygonPaths(areaView.polygon0, areaView.polyline0, areaView.polygon1, areaView.polyline1, frame.Buckets.Count,
                    x0, y0, x1, y1, areaView.BucketCalculator.BucketSize, Resolution, TerminatePolygon, UnknownValuePlotting);
            }
        }

        private const string UnknownValuePlottingPropertyName = "UnknownValuePlotting";
        /// <summary>
        /// Identifies the UnknownValuePlotting dependency property.
        /// </summary>
        public static readonly DependencyProperty UnknownValuePlottingProperty =
            DependencyProperty.Register(
            UnknownValuePlottingPropertyName,
            typeof(UnknownValuePlotting),
            typeof(RadialAreaSeries),
            new PropertyMetadata(UnknownValuePlotting.DontPlot, (sender, e) =>
        {
            (sender as RadialAreaSeries).RaisePropertyChanged(UnknownValuePlottingPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// Determines how unknown values will be plotted on the chart.
        /// </summary>
        /// <remarks>
        /// Null and Double.NaN are two examples of unknown values.
        /// </remarks>
        public UnknownValuePlotting UnknownValuePlotting
        {
            get
            {
                return (UnknownValuePlotting)this.GetValue(UnknownValuePlottingProperty);
            }
            set
            {
                this.SetValue(UnknownValuePlottingProperty, value);
            }
        }

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
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case UnknownValuePlottingPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        /// <summary>
        /// Gets whether the shape of the series polygon is a closed shape.
        /// </summary>
        protected override bool IsClosed
        {
            get
            {
                return true;
            }
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