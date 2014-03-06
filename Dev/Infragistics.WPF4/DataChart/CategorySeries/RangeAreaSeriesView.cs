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

namespace Infragistics.Controls.Charts
{
    internal class RangeAreaSeriesView
        : RangeCategorySeriesView
    {
        protected RangeAreaSeries RangeAreaModel { get; set; }
        public RangeAreaSeriesView(RangeAreaSeries model)
            : base(model)
        {
            RangeAreaModel = model;
        }

        private Polyline polyline0 = new Polyline();
        private Polygon polygon01 = new Polygon();
        private Polyline polyline1 = new Polyline();

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            polygon01.Detach();
            polyline0.Detach();
            polyline1.Detach();

            rootCanvas.Children.Add(polygon01);
            polygon01.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });

            rootCanvas.Children.Add(polyline0);
            polyline0.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = Model });
            polyline0.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            polyline0.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            polyline0.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsOutlineVisual(polyline0, true);

            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            polyline0.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            polyline0.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(Series.StartCapPropertyName) { Source = Model });
            polyline0.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(Series.EndCapPropertyName) { Source = Model });

            rootCanvas.Children.Add(polyline1);
            polyline1.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = Model });
            polyline1.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            polyline1.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            polyline1.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsOutlineVisual(polyline1, true);

            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            polyline1.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            polyline1.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(Series.StartCapPropertyName) { Source = Model });
            polyline1.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(Series.EndCapPropertyName) { Source = Model });

            if (!IsThumbnailView)
            {
                Model.RenderSeries(false);
            }
        }

        internal void ClearRangeArea()
        {
            polygon01.Points.Clear();
            polyline0.Points.Clear();
            polyline1.Points.Clear();
        }

        internal void RasterizeRangeArea(int count, Func<int, double> x0, Func<int, double> y0, Func<int, double> x1, Func<int, double> y1)
        {
            RasterizePolygon(polyline0, polygon01, polyline1, count, x0, y0, x1, y1);
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