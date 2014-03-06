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

namespace Infragistics.Controls.Charts
{
    abstract class Frame
    {
        public abstract void Interpolate(float p, Frame min, Frame max);

        protected static void Interpolate(List<Point> ret, float p, List<Point> min, List<Point> max)
        {
            int minCount = min.Count;
            int maxCount = max.Count;
            int count = Math.Max(minCount, maxCount);
            double q = 1.0 - p;

            if (ret.Count < count)
            {
                ret.InsertRange(ret.Count, new Point[count - ret.Count]);
            }

            if (ret.Count > count)
            {
                ret.RemoveRange(count, ret.Count - count);
            }

            for (int i = 0; i < Math.Min(minCount, maxCount); ++i)
            {
                ret[i] = new Point(min[i].X * q + max[i].X * p, min[i].Y * q + max[i].Y * p);
            }

            if (minCount < maxCount)
            {
                Point mn = minCount > 0 ? min[minCount - 1] : new Point(0, 0);

                for (int i = minCount; i < maxCount; ++i)
                {
                    ret[i] = new Point(mn.X * q + max[i].X * p, mn.Y * q + max[i].Y * p);
                }
            }

            if (minCount > maxCount)
            {
                Point mx = maxCount > 0 ? max[maxCount - 1] : new Point(0, 0);

                for (int i = maxCount; i < minCount; ++i)
                {
                    ret[i] = new Point(min[i].X * q + mx.X * p, min[i].Y * q + mx.Y * p);
                }
            }
        }

        protected static void Interpolate(List<double> ret, float p, List<double> min, List<double> max)
        {
            int minCount = min.Count;
            int maxCount = max.Count;
            int count = Math.Max(minCount, maxCount);
            double q = 1.0 - p;

            if (ret.Count < count)
            {
                ret.InsertRange(ret.Count, new double[count - ret.Count]);
            }

            if (ret.Count > count)
            {
                ret.RemoveRange(count, ret.Count - count);
            }

            for (int i = 0; i < Math.Min(minCount, maxCount); ++i)
            {
                ret[i] = min[i] * q + max[i] * p;
            }

            if (minCount < maxCount)
            {
                double mn = minCount > 0 ? min[minCount - 1] : 0.0;

                for (int i = minCount; i < maxCount; ++i)
                {
                    ret[i] = mn * q + max[i] * p;
                }
            }

            if (minCount > maxCount)
            {
                double mx = maxCount > 0 ? max[maxCount - 1] : 0.0;

                for (int i = maxCount; i < minCount; ++i)
                {
                    ret[i] = min[i] * q + mx * p;
                }
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