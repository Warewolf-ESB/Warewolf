using System;



using System.Linq;

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
    internal class PolarFrame
        : ScatterFrameBase<PolarFrame>
    {
        public PolarFrame()
        {
            Transformed = new List<Point>();
            UseCartesianInterpolation = true;
        }

        public List<Point> Transformed { get; set; }
        public bool UseCartesianInterpolation { get; set; }
        public Func<Point, Point> Retransform { get; set; }

        protected override void InterpolateOverride(float p, Frame minFrame, Frame maxFrame)
        {
            PolarFrame min = minFrame as PolarFrame;
            PolarFrame max = maxFrame as PolarFrame;

            if (min == null || max == null)
            {
                return;
            }

            Interpolate(Transformed, p, min.Transformed, max.Transformed);
        }

        protected override void InterpolateColumnValues(OwnedPoint point, float p, OwnedPoint minPoint, OwnedPoint maxPoint)
        {
            if (UseCartesianInterpolation)
            {
                base.InterpolateColumnValues(point, p, minPoint, maxPoint);
            }
            else
            {
                base.InterpolateColumnValues(point, p, minPoint, maxPoint);
                if (minPoint != null && maxPoint != null &&
                    (minPoint.ColumnValues.X != maxPoint.ColumnValues.X ||
                    minPoint.ColumnValues.Y != maxPoint.ColumnValues.Y))
                {
                    double q = 1.0 - p;
                    point.ColumnValues = new Point(
                        minPoint.ColumnValues.X * q + maxPoint.ColumnValues.X * p,
                        minPoint.ColumnValues.Y * q + maxPoint.ColumnValues.Y * p);
                }
            }
        }

        protected override void InterpolatePointOverride(
            OwnedPoint point, 
            double p, 
            OwnedPoint min, OwnedPoint max)
        {
            if (UseCartesianInterpolation)
            {
                base.InterpolatePointOverride(point, p, min, max);
            }
            else
            {
                if (min != null && max != null &&
                   (min.Point.X != max.Point.X ||
                   min.Point.Y != max.Point.Y) &&
                   point.ColumnValues.IsPlottable())
                {
                    point.Point = Retransform(point.ColumnValues);
                }
                else
                {
                    base.InterpolatePointOverride(point, p, min, max);
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