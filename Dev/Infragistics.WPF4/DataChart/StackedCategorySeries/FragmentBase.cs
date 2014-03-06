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
using Infragistics.Controls.Charts.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents one part of a stacked series.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class FragmentBase : HorizontalAnchoredCategorySeries
    {
        internal StackedFragmentSeries LogicalSeriesLink { get; set; }

        private StackedSeriesBase _parentSeries = null;
        internal StackedSeriesBase ParentSeries
        {
            get
            {
                return _parentSeries;
            }
            set
            {
                _parentSeries = value;
                if (value != null)
                {
                    SetMarkerCanvas(value.MarkerCanvas);
                }
            }
        }

        internal virtual void PrepareMarker(ISupportsMarkers markersHost, CategoryFrame frame, float[] bucket, IDetectsCollisions collisionAvoider,
            double value, int itemIndex, int markerCount)
        {
            double x = bucket[0];
            double y = bucket[1];

            Rect markerRect = new Rect(x - 5, y - 5, 11, 11);

            if (!double.IsNaN(x) && !double.IsNaN(y) && !double.IsInfinity(x) && !double.IsInfinity(y) && collisionAvoider.TryAdd(markerRect))
            {
                frame.Markers.Add(new Point(x, y));
                markersHost.UpdateMarkerTemplate(markerCount, itemIndex);
            }
        }

        internal override void PrepareFrame(CategoryFrame frame, CategorySeriesView view)
        {
            if (ValueColumn == null || ParentSeries == null || LogicalSeriesLink == null) 
                return;

            if (LogicalSeriesLink.LowValues.Count == 0 || LogicalSeriesLink.HighValues.Count == 0)
                return;

            this.GetFramePreparer(view).PrepareFrame(frame, view);
        }

        internal virtual void TerminatePolygon(PointCollection polygon, Func<int, double> x0, Func<int, double> y1, CategorySeriesView view)
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

                FragmentBase previousSeries = seriesCollection[index - 1] as FragmentBase;
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
            CategoryAxisBase xAxis = ParentSeries.GetXAxis() as CategoryAxisBase;
            Axis yAxis = ParentSeries.GetYAxis();
            if (ParentSeries == null
                || xAxis == null
                || xAxis.ItemsSource == null
                || yAxis == null
                || ParentSeries.FastItemsSource == null
                || xAxis.SeriesViewer == null
                || yAxis.SeriesViewer == null
                )
            {
                isValid = false;
            }

            if (ValueColumn == null)
            {
                return false;
            }

            if (double.IsInfinity(ValueColumn.Minimum) &&
                double.IsInfinity(ValueColumn.Maximum))
            {
                isValid = false;
            }

            if (double.IsNaN(ValueColumn.Minimum) &&
                double.IsNaN(ValueColumn.Maximum))
            {
                isValid = false;
            }

            return isValid;
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
        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            //Fragment series should not try to determine the axis range.
            //The correct range is always determined by the parent series and its GetRange method.
            return null;
        }

        internal int GetLegendItemIndex()
        {
            if (ParentSeries == null) return -1;
            int index = ParentSeries.Index;

            foreach (var series in ParentSeries.ReverseLegendOrder ? ParentSeries.ActualSeries.Reverse() : ParentSeries.ActualSeries)
            {
                if (series.VisualSeriesLink == this)
                {
                    return index;
                }

                if (ParentSeries.ActualLegend == null
                    || series.ActualVisibility != Visibility.Visible
                    || series.ActualLegendItemVisibility != Visibility.Visible)
                {
                    continue;
                }

                index++;
            }

            return -1;
        }

        internal void UpdateLegend(LegendBase legend)
        {
            if (legend == null)
            {

            }
        }

        #region Propety Updates
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

            if (ParentSeries == null) return;

            NumericAxisBase yAxis = ParentSeries.GetYAxis() as NumericAxisBase;

            if (yAxis == null) return;
            
            switch (propertyName)
            {
                case AnchoredCategorySeries.ValueColumnPropertyName:
                    this.AnchoredView.TrendLineManager.Reset();

                    if (yAxis != null && !yAxis.UpdateRange())
                    {
                        this.ParentSeries.GetSeriesView().BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;
            }
        }
        #endregion
        /// <summary>
        /// Renders the thumbnail for the OPD pane.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="surface">The render target.</param>
        protected internal override void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            base.RenderThumbnail(viewportRect, surface);

            if (this.ParentSeries != null && this.ParentSeries.StackedSeriesManager != null && this.ParentSeries.StackedSeriesManager.SeriesVisual != null && this.ThumbnailView != null)
            {
                // original version of this logic is in StackedSeriesManager.RenderSeries.
                this.ThumbnailView.SetZIndex(this.ParentSeries.StackedSeriesManager.SeriesVisual.Count - this.Index);
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