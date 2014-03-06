using System;
using System.Collections.Generic;
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

namespace Infragistics.Controls.Charts
{
    internal class BarFramePreparer:CategoryFramePreparer
    {





        public BarFramePreparer(IIsCategoryBased host)
            : this(host, host as ISupportsMarkers, host as IProvidesViewport, host as ISupportsErrorBars, host as IBucketizer)
        {
        }

        public BarFramePreparer(IIsCategoryBased host, ISupportsMarkers markersHost, IProvidesViewport viewportHost, ISupportsErrorBars errorBarsHost, IBucketizer bucketizingHost)
            : base(host, markersHost, viewportHost, errorBarsHost, bucketizingHost)
        {



            TrendlineHost = new DefaultCategoryTrendlineHost();

            if (host is IHasCategoryTrendline)
            {
                TrendlineHost = host as IHasCategoryTrendline;
            }



            ValuesProvider = new DefaultSingleValueProvider();

            if (host is IHasSingleValueCategory)
            {
                ValuesProvider = host as IHasSingleValueCategory;
            }
        }
       
        protected override bool PrepareMarker(
            CategoryFrame frame, 
            float[] bucket, 
            IDetectsCollisions collisionAvoider, 
            int itemIndex, int markerCount)
        {
            double x = bucket[1];
            double y = bucket[0];

            Rect markerRect = new Rect(x - 5, y - 5, 11, 11);

            if (!double.IsNaN(x) && !double.IsNaN(y)
                && !double.IsInfinity(x) && !double.IsInfinity(y)
                && collisionAvoider.TryAdd(markerRect))
            {
                frame.Markers.Add(new Point(x, y));
                MarkersHost.UpdateMarkerTemplate(markerCount, itemIndex); 
                
                return true;
            }
            return false;
        }

        protected override void PrepareTrendline(PreparationParams p, ValuesHolder h, double offset)
        {
            if (TrendlineHost.TrendLineType == TrendLineType.None ||
                TrendlineHost.TrendlinePreparer == null ||
                TrendlineHost.TrendLinePeriod < 1)
            {
                return;
            }

            ScalerParams sParams = new ScalerParams(p.WindowRect, p.ViewportRect, p.Scaler.IsInverted);
            ScalerParams yParams = new ScalerParams(p.WindowRect, p.ViewportRect, p.YScaler.IsInverted);


            IList<double> values = ((SingleValuesHolder)h).Values;

            if (p.SortingScaler != null &&
                p.SortingScaler.SortedIndices != null)
            {
                values = new SafeSortedReadOnlyDoubleCollection(
                    values, p.SortingScaler.SortedIndices);
            }

            TrendResolutionParams trendResolutionParams =
                new TrendResolutionParams()
                {
                    BucketSize = p.BucketSize,
                    FirstBucket = p.FirstBucket,
                    LastBucket = p.LastBucket,
                    Offset = offset,
                    Resolution = p.Resolution,
                    Viewport = p.ViewportRect
                };

            if (TrendlineHost.TrendLineType != TrendLineType.None)
            {
                TrendlineHost.TrendlinePreparer.PrepareLine(
                    p.Frame.Trend,
                    TrendlineHost.TrendLineType,
                    values,
                    TrendlineHost.TrendLinePeriod,
                    (x) => p.YScaler.GetScaledValue(x, yParams),
                    (y) => p.Scaler.GetScaledValue(y, sParams),
                    trendResolutionParams);
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