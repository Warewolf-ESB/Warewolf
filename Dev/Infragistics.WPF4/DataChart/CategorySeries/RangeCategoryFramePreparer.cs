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
    internal class RangeCategoryFramePreparer
        : CategoryFramePreparerBase
    {





        public RangeCategoryFramePreparer(IIsCategoryBased host)
            : this(host, host as ISupportsMarkers, host as IProvidesViewport, host as ISupportsErrorBars, host as IBucketizer)
        {
        }
        public RangeCategoryFramePreparer(IIsCategoryBased host, ISupportsMarkers markersHost, IProvidesViewport viewportHost, ISupportsErrorBars errorBarsHost, IBucketizer bucketizingHost) 
            : base(host, markersHost, viewportHost, errorBarsHost, bucketizingHost)
        {



            TrendlineHost = new DefaultCategoryTrendlineHost();

            if (host is IHasCategoryTrendline)
            {
                TrendlineHost = host as IHasCategoryTrendline;
            }



            ValuesProvider = new DefaultHighLowValueProvider();

            if (host is IHasHighLowValueCategory)
            {
                ValuesProvider = host as IHasHighLowValueCategory;
            }
        }

        [Weak]
        protected IHasCategoryTrendline TrendlineHost { get; set; }
        [Weak]
        protected IHasHighLowValueCategory ValuesProvider { get; set; }


        protected override bool PrepareMarker(CategoryFrame frame, float[] bucket, IDetectsCollisions collisionAvoider, int itemIndex, int markerCount)
        {
            double x = bucket[0];
            double yLow = bucket[1];
            double yHigh = bucket[2];

            if (!double.IsNaN(x) &&
                !double.IsNaN(yLow) &&
                !double.IsNaN(yHigh))
            {
                frame.Markers.Add(new Point(x, (yLow + yHigh) / 2.0)); // [DN 2/5/2010] range series markers should be placed vertically halfway between the 2 y values.
                MarkersHost.UpdateMarkerTemplate(markerCount, itemIndex);

                return true;
            }
            return false;
        }

        protected override void StoreYValues(ValuesHolder h, int index, bool useTemp, bool isFragment)
        {
            var hl = (HighLowValuesHolder)h;
            IList<double> highValues = hl.HighValues;
            IList<double> lowValues = hl.LowValues;

            float bucketHigh = ConvertToBucket(highValues[index]);
            float bucketLow = ConvertToBucket(lowValues[index]);
            float currentHigh = Math.Max(bucketHigh, bucketLow);
            float currentLow = Math.Min(bucketHigh, bucketLow);

            if (useTemp)
            {
                hl.TempY0 = currentLow;
                hl.TempY1 = currentHigh;
            }
            else
            {
                hl.BucketY1 = currentHigh;
                hl.BucketY0 = currentLow;
            }
        }

        protected override void MinMaxYValues(ValuesHolder h, int index, bool isFragment)
        {
            var hl = (HighLowValuesHolder)h;
            IList<double> highValues = hl.HighValues;
            IList<double> lowValues = hl.LowValues;

            float high = ConvertToBucket(highValues[index]);
            float low = ConvertToBucket(lowValues[index]);

            if (!float.IsNaN(high))
            {
                hl.BucketY1 = Math.Max(hl.BucketY1, high);
                hl.BucketY0 = Math.Min(hl.BucketY0, high);
            }
            if (!float.IsNaN(low))
            {
                hl.BucketY1 = Math.Max(hl.BucketY1, low);
                hl.BucketY0 = Math.Min(hl.BucketY0, low);
            }
        }

        protected override float[] GetBucketSorting(double xVal, ValuesHolder h)
        {
            var hl = (HighLowValuesHolder)h;

            return new float[] { ConvertToBucket(xVal), hl.BucketY0, hl.BucketY1 };
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
            bucket[2] = (float)p.YScaler.GetScaledValue(bucket[2], yParams);
        }

        protected override ValuesHolder GetValues(PreparationParams p)
        {
            var hl = new HighLowValuesHolder();
            hl.HighValues = ValuesProvider.HighColumn;
            hl.LowValues = ValuesProvider.LowColumn;

            return hl;
        }
    }

    internal class HighLowValuesHolder
            : ValuesHolder
    {
        public IList<double> HighValues { get; set; }
        public IList<double> LowValues { get; set; }

        public override int Count
        {
            get
            {
                if (HighValues == null ||
                    LowValues == null)
                {
                    return 0;
                }

                return Math.Min(
                    HighValues.Count,
                    LowValues.Count);
            }
        }
    }

    internal class DefaultHighLowValueProvider
       : IHasHighLowValueCategory
    {
        public IList<double> HighColumn
        {
            get { return new List<double>(); }
        }

        public IList<double> LowColumn
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