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

namespace Infragistics.Controls.Charts
{
    internal class RangeCategorySeriesView
        : CategorySeriesView
    {
        protected RangeCategorySeries RangeModel { get; set; }
        public RangeCategorySeriesView(RangeCategorySeries model)
            : base(model)
        {
            RangeModel = model;
        }

        internal void RasterizePolygon(
            Polyline polyline0, 
            Polygon polygon01, 
            Polyline polyline1, 
            int count, 
            Func<int, double> x0, 
            Func<int, double> y0, 
            Func<int, double> x1, 
            Func<int, double> y1)
        {
            polyline0.Points.Clear();
            polygon01.Points.Clear();
            polyline1.Points.Clear();

            foreach (int i in Flattener.Flatten(count, x0, y0, Model.Resolution))
            {
                polyline0.Points.Add(new Point(x0(i), y0(i)));
                polygon01.Points.Add(new Point(x0(i), y0(i)));
            }

            foreach (int i in Flattener.Flatten(count, x1, y1, Model.Resolution))
            {
                polyline1.Points.Add(new Point(x1(i), y1(i)));
                polygon01.Points.Add(new Point(x1(i), y1(i)));
            }

            polyline0.IsHitTestVisible = polyline0.Points.Count > 0;
            polygon01.IsHitTestVisible = polygon01.Points.Count > 0;
            polyline1.IsHitTestVisible = polyline1.Points.Count > 0;
        }
        internal override CategoryBucketCalculator CreateBucketCalculator()
        {
            return new RangeCategoryBucketCalculator(this);
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