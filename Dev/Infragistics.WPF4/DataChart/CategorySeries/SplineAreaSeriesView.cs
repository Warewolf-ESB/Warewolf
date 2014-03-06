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
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    internal class SplineAreaSeriesView
        : SplineSeriesBaseView
    {
        protected SplineAreaSeries SplineAreaModel { get; set; }
        public SplineAreaSeriesView(SplineAreaSeries model)
            : base(model)
        {
            SplineAreaModel = model;
        }

        private Path polygon0 = new Path();
        private Path polyline0 = new Path();
        private Path polygon1 = new Path();
        private Path polyline1 = new Path();

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            polygon0.Detach();
            polygon1.Detach();
            polyline0.Detach();
            polyline1.Detach();

            rootCanvas.Children.Add(polygon0);
            polygon0.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });

            rootCanvas.Children.Add(polygon1);
            polygon1.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
            polygon1.Opacity = 0.5;
            VisualInformationManager.SetIsTranslucentPortionVisual(polygon1, true);

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
            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });

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
            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });

            if (!IsThumbnailView)
            {
                Model.RenderSeries(false);
            }
        }

        internal void ClearSplineArea()
        {
            polygon0.Data = null;
            polygon1.Data = null;
            polyline0.Data = null;
            polyline1.Data = null;
        }

        public void RasterizeSplineArea(
            int count,
            Func<int, double> x0, Func<int, double> y0,
            Func<int, double> x1, Func<int, double> y1,
            int bucketSize, double resolution, 
            Action<PointCollection, PointCollection, PointCollection, PointCollection, bool> terminatePolygon, 
            UnknownValuePlotting unknownValuePlotting)
        {
            //Make the rasterizer aware of the possible date time axis
            AnchoredModel.LineRasterizer.IsSortingAxis = CategoryModel.GetXAxis() is ISortingAxis ? true : false;
            AnchoredModel.LineRasterizer.RasterizePolygonPaths(
                polygon0, polyline0, 
                polygon1, polyline1, 
                count, 
                x0, y0, 
                x1, y1, 
                bucketSize, 
                resolution, 
                terminatePolygon, 
                unknownValuePlotting);
        }

        protected internal override void ExportViewShapes(SeriesVisualData svd)
        {
            base.ExportViewShapes(svd);

            var lowerShape = new PathVisualData("lowerShape", polyline0);
            lowerShape.Tags.Add("Lower");
            var upperShape = new PathVisualData("upperShape", polyline1);
            upperShape.Tags.Add("Upper");
            upperShape.Tags.Add("Main");
            var translucent = new PathVisualData("translucentShape", polygon0);
            translucent.Tags.Add("Translucent");
            var fill = new PathVisualData("fillShape", polygon1);
            fill.Tags.Add("Fill");

            svd.Shapes.Add(lowerShape);
            svd.Shapes.Add(upperShape);
            svd.Shapes.Add(translucent);
            svd.Shapes.Add(fill);
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