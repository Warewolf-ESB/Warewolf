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
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents one part of a StackedBarSeries.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class BarFragment : ColumnFragment
    {
        internal BarFragment()
        {
            DefaultStyleKey = typeof(BarFragment);
        }

        internal new StackedBarSeries ParentSeries
        {
            get
            {
                return base.ParentSeries as StackedBarSeries;
            }
            set
            {
                base.ParentSeries = value;
            }
        }
        /// <summary>
        /// The X-Axis for this BarFragment.
        /// </summary>
        public new NumericXAxis XAxis
        {
            get
            {
                return ParentSeries != null ? ParentSeries.XAxis : null;
            }
        }
        /// <summary>
        /// The Y-Axis for this BarFragment.
        /// </summary>
        public new CategoryYAxis YAxis
        {
            get
            {
                return ParentSeries != null ? ParentSeries.YAxis : null;
            }
        }

        internal override void PrepareMarker(ISupportsMarkers markersHost, CategoryFrame frame, float[] bucket, IDetectsCollisions collisionAvoider, double value, int itemIndex, int markerCount)
        {
            double y = bucket[0];
            double x = value < 0 ? bucket[2] : bucket[1];

            Rect markerRect = new Rect(x - 5, y - 5, 11, 11);

            if (!double.IsNaN(x) && !double.IsNaN(y) && !double.IsInfinity(x) && !double.IsInfinity(y) && collisionAvoider.TryAdd(markerRect))
            {
                frame.Markers.Add(new Point(x, y));
                markersHost.UpdateMarkerTemplate(markerCount, itemIndex);
            }
        }
        /// <summary>
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected override object GetItem(Point world)
        {
            int index = 0;

            if (this.YAxis is ISortingAxis)
            {
                index = GetItemIndexSorted(world);
                if (index == -1)
                {
                    return null;
                }
            }
            else
            {
                index = GetItemIndex(world);
            }

            return index >= 0
                && FastItemsSource != null
                && index < FastItemsSource.Count ? FastItemsSource[index] : null;
        }
        /// <summary>
        /// Gets the index of the item based on world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The index of the item for the specified coordinates.</returns>
        protected internal override int GetItemIndexSorted(Point world)
        {
            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            if (windowRect.IsEmpty || viewportRect.IsEmpty)
            {
                return -1;
            }
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted);

            ISortingAxis sorting = YAxis as ISortingAxis;

            double top = YAxis.GetUnscaledValue(viewportRect.Top, yParams);
            double bottom = YAxis.GetUnscaledValue(viewportRect.Bottom, yParams);
            double windowY = (world.Y - windowRect.Top) / windowRect.Height;
            double axisValue = top + ((bottom - top) * windowY);

            if ((long)axisValue <= DateTime.MinValue.Ticks || (long)axisValue >= DateTime.MaxValue.Ticks)
            {
                return -1;
            }
            int itemIndex = sorting.GetIndexClosestToUnscaledValue(axisValue);
            return itemIndex;
        }

        internal override double GetWorldZeroValue(CategorySeriesView view)
        {
            double value = 0.0;

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && XAxis != null)
            {
                value = XAxis.GetScaledValue(0.0, xParams);
            }

            return value;
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

            if (ParentSeries == null || ParentSeries.XAxis == null) return;

            NumericAxisBase xAxis = ParentSeries.XAxis;

            switch (propertyName)
            {
                case AnchoredCategorySeries.ValueColumnPropertyName:
                    this.AnchoredView.TrendLineManager.Reset();

                    if (xAxis != null && !xAxis.UpdateRange())
                    {
                        this.ParentSeries.GetSeriesView().BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;
            }
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