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
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    internal class PolarSplineAreaSeriesView
        : PolarLineSeriesBaseView
    {
        protected PolarSplineAreaSeries PolarSplineAreaModel { get; set; }
        public PolarSplineAreaSeriesView(PolarSplineAreaSeries model)
            : base(model)
        {
            PolarSplineAreaModel = model;
        }

        private Path polyline = new Path() { Data = new PathGeometry() };
        private Path polygon = new Path() { Data = new PathGeometry() };

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            polygon.Detach();
            rootCanvas.Children.Add(polygon);
            polygon.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });

            polyline.Detach();
            rootCanvas.Children.Add(polyline);

            polyline.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = Model });
            polyline.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            polyline.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            polyline.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });

            polyline.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            polyline.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(Series.StartCapPropertyName) { Source = Model });
            polyline.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(Series.EndCapPropertyName) { Source = Model });
            polyline.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });

            if (!IsThumbnailView)
            {
                Model.RenderSeries(false);
            }
        }

        internal void ClearPolarSplineArea()
        {
            (polygon.Data as PathGeometry).Reset();
            (polyline.Data as PathGeometry).Reset();
        }

        internal void RenderPolarSplineArea(List<Point> points, double stiffness)
        {
            (polyline.Data as PathGeometry).Figures =
                Numeric.Spline2D(points.Count + 1,
                (i) => i < points.Count ? points[i].X : points[i - points.Count].X,
                (i) => i < points.Count ? points[i].Y : points[i - points.Count].Y, stiffness);

            (polygon.Data as PathGeometry).Figures =
                Numeric.Spline2D(points.Count + 1,
                (i) => i < points.Count ? points[i].X : points[i - points.Count].X,
                (i) => i < points.Count ? points[i].Y : points[i - points.Count].Y, stiffness);
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