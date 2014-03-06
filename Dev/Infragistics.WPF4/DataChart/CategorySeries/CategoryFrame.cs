using System;
using System.Collections.Generic;
using System.Windows;

namespace Infragistics.Controls.Charts
{
    internal class CategoryFrame: Frame
    {
        public CategoryFrame(int count)
        {
            cnt = count;
        }

        public readonly List<float[]> Buckets = new List<float[]>();
        public readonly List<float> ErrorBuckets = new List<float>();
        public readonly List<Point> Markers = new List<Point>();
        public readonly List<Point> Trend = new List<Point>();
        public readonly List<Point> ErrorBars = new List<Point>();
        public readonly List<double> ErrorBarSizes = new List<double>();

        private int cnt;

        public override void Interpolate(float p, Frame _min, Frame _max)
        {
            CategoryFrame min = _min as CategoryFrame;
            CategoryFrame max = _max as CategoryFrame;

            int minCount = min.Buckets.Count;
            int maxCount = max.Buckets.Count;
            int count = Math.Max(minCount, maxCount);

            #region resize buckets 
            if (Buckets.Count < count)
            {
                while (Buckets.Count < count)
                {
                    Buckets.Add(new float[cnt]);
                }
            }

            if (Buckets.Count > count)
            {
                Buckets.RemoveRange(count, Buckets.Count - count);  // strip spare buckets
            }
            #endregion

            #region interpolate buckets

            for (int i = 0; i < Math.Min(minCount, maxCount); ++i)
            {
                float[] bucket = Buckets[i];

                for (int j = 0; j < cnt; ++j)
                {
                    bucket[j]=min.Buckets[i][j] + p * (max.Buckets[i][j] - min.Buckets[i][j]);
                }
            }

            if (minCount < maxCount)
            {
                float[] b=new float[cnt];

                for(int j=cnt-1; j>=0; --j)
                {
                    b[j]= min.Buckets.Count > 0 ? min.Buckets[min.Buckets.Count - 1][j] : 0.0f;
                }

                for (int i = minCount; i < maxCount; ++i)
                {
                    float[] bucket = Buckets[i];

                    for (int j = cnt - 1; j >= 0; --j)
                    {
                        bucket[j]=b[j] + p * (max.Buckets[i][j] - b[j]);
                    }
                }
            }

            if (minCount > maxCount)
            {
                float[] e = new float[cnt];

                for (int j = cnt-1; j >= 0; --j)
                {
                    e[j] = max.Buckets.Count > 0 ? max.Buckets[max.Buckets.Count - 1][j] : 0.0f;
                }

                for (int i = maxCount; i < minCount; ++i)
                {
                    float[] bucket = Buckets[i];

                    for (int j = cnt-1; j >= 0; --j)
                    {
                        bucket[j] = min.Buckets[i][j] + p * (e[j] - min.Buckets[i][j]);
                    }
                }
            }
            #endregion



#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

            Interpolate(Markers, p, min.Markers, max.Markers);



#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

            Interpolate(Trend, p, min.Trend, max.Trend);

            Interpolate(ErrorBars, p, min.ErrorBars, max.ErrorBars);
            Interpolate(ErrorBarSizes, p, min.ErrorBarSizes, max.ErrorBarSizes);
            
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