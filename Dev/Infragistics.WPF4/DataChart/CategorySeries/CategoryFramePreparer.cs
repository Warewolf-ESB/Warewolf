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
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    internal class CategoryFramePreparer
        : CategoryFramePreparerBase
    {





        public CategoryFramePreparer(IIsCategoryBased host)
            : this(host, host as ISupportsMarkers, host as IProvidesViewport, host as ISupportsErrorBars, host as IBucketizer)
        {
        }
        public CategoryFramePreparer(IIsCategoryBased host, ISupportsMarkers markersHost, IProvidesViewport viewportHost, ISupportsErrorBars errorBarsHost, IBucketizer bucketizingHost) 
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

        [Weak]
        protected IHasCategoryTrendline TrendlineHost { get; set; }
        [Weak]
        protected IHasSingleValueCategory ValuesProvider { get; set; }

        protected override bool PrepareMarker(
            CategoryFrame frame, 
            float[] bucket, 
            IDetectsCollisions collisionAvoider, 
            int itemIndex, int markerCount)
        {
            double x = bucket[0];
            double y = bucket[1];

            Rect markerRect = new Rect(x - 5, y - 5, 11, 11);

            
            if (!double.IsNaN(x) && !double.IsNaN(y) && !double.IsInfinity(x) && !double.IsInfinity(y) && collisionAvoider.TryAdd(markerRect))
            {
                frame.Markers.Add(new Point(x, y));
                MarkersHost.UpdateMarkerTemplate(markerCount, itemIndex); 
                
                return true;
            }
            return false;
        }

        protected override ValuesHolder GetValues(PreparationParams p)
        {
            SingleValuesHolder h = new SingleValuesHolder();
            IList<double> values = ValuesProvider.ValueColumn;
            h.Values = values;

            return h;
        }
      
        protected override void ScaleBucketValues(PreparationParams p, float[] bucket, double offset, bool isSortingScaler, ScalerParams xParams, ScalerParams yParams)
        {
            if (isSortingScaler)
            {
                bucket[0] = (float)(bucket[0] + offset);
            }
            else
            {
                bucket[0] = (float)(p.Scaler.GetScaledValue(bucket[0], xParams) + offset);
            }
            bucket[1] = (float)p.YScaler.GetScaledValue(bucket[1], yParams);

            if (p.BucketSize > 1 || isSortingScaler)
            {
                bucket[2] = (float)p.YScaler.GetScaledValue(bucket[2], yParams);
            }
            else
            {
                bucket[2] = bucket[1];
            }
        }

       

        protected override void PrepareTrendline(PreparationParams p, ValuesHolder h, double offset)
        {
            if (TrendlineHost.TrendLineType == TrendLineType.None ||
                TrendlineHost.TrendlinePreparer == null ||
                TrendlineHost.TrendLinePeriod < 1)
            {
                return;
            }

            ScalerParams xParams = new ScalerParams(p.WindowRect, p.ViewportRect, p.Scaler.IsInverted);
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
                if (TrendlineHost is IBarSeries)
                {
                    TrendlineHost.TrendlinePreparer.PrepareLine(
                        p.Frame.Trend,
                        TrendlineHost.TrendLineType,
                        values,
                        TrendlineHost.TrendLinePeriod,
                        (y) => p.YScaler.GetScaledValue(y, yParams),
                        (x) => p.Scaler.GetScaledValue(x, xParams),
                        trendResolutionParams);
                }
                else
                {
                    TrendlineHost.TrendlinePreparer.PrepareLine(
                        p.Frame.Trend,
                        TrendlineHost.TrendLineType,
                        values,
                        TrendlineHost.TrendLinePeriod,
                        (x) => p.Scaler.GetScaledValue(x, xParams),
                        (y) => p.YScaler.GetScaledValue(y, yParams),
                        trendResolutionParams);
                }
            }
        }

        

        protected override void StoreYValues(ValuesHolder h, int index, bool useTemp, bool isFragment)
        {
            SingleValuesHolder s = (SingleValuesHolder)h;
            IList<double> values = s.Values;

            float bucketY0 = ConvertToBucket(values[index]);
            float bucketY1 = bucketY0;


            if (isFragment)
            {
                float[] bucket = BucketizingHost.GetBucket(index);
                bucketY0 = bucket[1];
                bucketY1 = bucket[1];
            }


            if (useTemp)
            {
                s.TempY0 = bucketY0;
                s.TempY1 = bucketY1;
            }
            else
            {
                s.BucketY0 = bucketY0;
                s.BucketY1 = bucketY1;
            }
        }

        protected override void MinMaxYValues(ValuesHolder h, int index, bool isFragment)
        {
            SingleValuesHolder s = (SingleValuesHolder)h;
            IList<double> values = s.Values;

            if (index < values.Count)
            {
                float y;

                if (isFragment)
                {
                    float[] bucket = BucketizingHost.GetBucket(index);
                    y = bucket[1];
                }
                else

                {
                    y = ConvertToBucket(values[index]);
                }
                s.BucketY0 = Math.Min(s.BucketY0, y);
                s.BucketY1 = Math.Max(s.BucketY1, y);
            }
        }

        protected override float[] GetBucketSorting(double xVal, ValuesHolder h)
        {
            SingleValuesHolder s = (SingleValuesHolder)h;

            return new float[] { ConvertToBucket(xVal), s.BucketY0, s.BucketY1 };
        }
    }

    internal class SingleValuesHolder
            : ValuesHolder
    {
        public IList<double> Values { get; set; }

        public override int Count
        {
            get
            {
                if (Values != null)
                {
                    return Values.Count;
                }
                return 0;
            }
        }
    }

    internal class DefaultSingleValueProvider
        : IHasSingleValueCategory
    {
        public IList<double> ValueColumn
        {
            get { return new List<double>(); }
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