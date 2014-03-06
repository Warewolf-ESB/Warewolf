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
using Infragistics.Controls.Charts.Util;
using System.Windows.Data;

namespace Infragistics.Controls.Charts
{
    internal class FinancialIndicatorView
        : FinancialSeriesView
    {
        protected internal FinancialIndicator IndicatorModel { get; set; }
        public FinancialIndicatorView(FinancialIndicator model)
            : base(model)
        {
            IndicatorModel = model;

            Columns = new Pool<LineGeometry> { Create = CreateColumn, Destroy = DestroyColumn };

            TrendLineManager = new CategoryTrendLineManager();
        }

        protected override FinancialBucketCalculator CreateBucketCalculator()
        {
            return new FinancialIndicatorBucketCalculator(this);
        }

        private readonly Pool<LineGeometry> Columns;

        private LineGeometry CreateColumn()
        {
            return new LineGeometry();
        }
        private void DestroyColumn(LineGeometry column)
        {
        }

        private Path positivePath0 = new Path() { Data = new PathGeometry() { FillRule = FillRule.Nonzero } };
        private Path negativePath0 = new Path() { Data = new PathGeometry() { FillRule = FillRule.Nonzero } };

        private Path positivePath01 = new Path() { Data = new PathGeometry() { FillRule = FillRule.Nonzero } };
        private Path negativePath01 = new Path() { Data = new PathGeometry() { FillRule = FillRule.Nonzero } };

        private Path positivePath1 = new Path() { Data = new PathGeometry() { FillRule = FillRule.Nonzero } };
        private Path negativePath1 = new Path() { Data = new PathGeometry() { FillRule = FillRule.Nonzero } };

        private Path positiveColumns = new Path() { Data = new GeometryGroup() };
        private Path negativeColumns = new Path() { Data = new GeometryGroup() };

        internal CategoryTrendLineManagerBase TrendLineManager { get; set; }

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            this.TrendLineManager.AttachPolyLine(rootCanvas, Model);

            positivePath01.Detach();
            positivePath0.Detach();
            positivePath1.Detach();
            positiveColumns.Detach();
            negativePath01.Detach();
            negativePath0.Detach();
            negativePath1.Detach();
            negativeColumns.Detach();

            rootCanvas.Children.Add(positivePath01);
            positivePath01.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
            positivePath01.Opacity = 0.8;
            VisualInformationManager.SetIsTranslucentPortionVisual(positivePath01, true);

            rootCanvas.Children.Add(positivePath0);
            positivePath0.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
            positivePath0.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            positivePath0.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            positivePath0.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });

            positivePath0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            positivePath0.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            VisualInformationManager.SetIsMultiColorLineVisual(positivePath0, true);

            rootCanvas.Children.Add(positivePath1);
            positivePath1.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
            positivePath1.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            positivePath1.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            positivePath1.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            positivePath1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            positivePath1.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            VisualInformationManager.SetIsMultiColorLineVisual(positivePath1, true);

            rootCanvas.Children.Add(negativePath01);
            negativePath01.SetBinding(Shape.FillProperty, new Binding(FinancialSeries.NegativeBrushPropertyName) { Source = Model });
            negativePath01.Opacity = 0.8;
            VisualInformationManager.SetIsNegativeVisual(negativePath01, true);
            VisualInformationManager.SetIsTranslucentPortionVisual(negativePath01, true);

            rootCanvas.Children.Add(negativePath0);
            negativePath0.SetBinding(Shape.StrokeProperty, new Binding(FinancialSeries.NegativeBrushPropertyName) { Source = Model });
            negativePath0.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            negativePath0.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            negativePath0.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            negativePath0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            negativePath0.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            VisualInformationManager.SetIsNegativeVisual(negativePath0, true);
            VisualInformationManager.SetIsMultiColorLineVisual(negativePath0, true);

            rootCanvas.Children.Add(negativePath1);
            negativePath1.SetBinding(Shape.StrokeProperty, new Binding(FinancialSeries.NegativeBrushPropertyName) { Source = Model });
            negativePath1.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            negativePath1.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            negativePath1.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            negativePath1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            negativePath1.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            VisualInformationManager.SetIsNegativeVisual(negativePath1, true);
            VisualInformationManager.SetIsMultiColorLineVisual(negativePath1, true);

            rootCanvas.Children.Add(positiveColumns);
            positiveColumns.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
            positiveColumns.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            positiveColumns.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            positiveColumns.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            positiveColumns.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            positiveColumns.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            VisualInformationManager.SetIsMultiColorLineVisual(positiveColumns, true);

            rootCanvas.Children.Add(negativeColumns);
            negativeColumns.SetBinding(Shape.StrokeProperty, new Binding(FinancialSeries.NegativeBrushPropertyName) { Source = Model });
            negativeColumns.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            negativeColumns.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            negativeColumns.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            negativeColumns.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            negativeColumns.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            VisualInformationManager.SetIsNegativeVisual(negativeColumns, true);
            VisualInformationManager.SetIsMultiColorLineVisual(negativeColumns, true);

            if (!IsThumbnailView)
            {
                Model.RenderSeries(false);
            }
        }

        internal void ClearIndicatorVisual(bool wipeClean)
        {
            (positivePath0.Data as PathGeometry).Reset();
            (positivePath01.Data as PathGeometry).Reset();
            (positivePath1.Data as PathGeometry).Reset();

            (negativePath0.Data as PathGeometry).Reset();
            (negativePath01.Data as PathGeometry).Reset();
            (negativePath1.Data as PathGeometry).Reset();

            (positiveColumns.Data as GeometryGroup).Reset();
            (negativeColumns.Data as GeometryGroup).Reset();
            if (wipeClean)
            {
                Columns.Count = 0;
            }

            this.TrendLineManager.ClearPoints();
        }

        internal void UpdateHitTests()
        {
            positivePath0.IsHitTestVisible = (positivePath0.Data as PathGeometry).Figures.Count > 0;
            positivePath01.IsHitTestVisible = (positivePath01.Data as PathGeometry).Figures.Count > 0;
            positivePath1.IsHitTestVisible = (positivePath1.Data as PathGeometry).Figures.Count > 0;

            negativePath0.IsHitTestVisible = (negativePath0.Data as PathGeometry).Figures.Count > 0;
            negativePath01.IsHitTestVisible = (negativePath01.Data as PathGeometry).Figures.Count > 0;
            negativePath1.IsHitTestVisible = (negativePath1.Data as PathGeometry).Figures.Count > 0;
        }



        internal void RasterizeLine(
            int count, 
            Func<int, double> x0, 
            Func<int, double> y0, 
            Func<int, double> x1, 
            Func<int, double> y1, 
            bool colorByGradient)
        {
            IndicatorRenderer.RasterizeLine(
                count,
                x0,
                y0,
                x1,
                y1,
                colorByGradient,
                WindowRect,
                Viewport,
                positivePath0,
                positivePath01,
                positivePath1,
                negativePath0,
                negativePath01,
                negativePath1,
                BucketCalculator.BucketSize,
                Model.Resolution);
        }

        internal void RasterizeArea(
            int count, 
            Func<int, double> x0, 
            Func<int, double> y0, 
            Func<int, double> x1, 
            Func<int, double> y1, 
            bool colorByGradient, 
            double worldZero)
        {
            IndicatorRenderer.RasterizeArea(
                count,
                x0,
                y0,
                x1,
                y1,
                colorByGradient,
                WindowRect,
                Viewport,
                positivePath0,
                positivePath01,
                positivePath1,
                negativePath0,
                negativePath01,
                negativePath1,
                worldZero,
                BucketCalculator.BucketSize,
                Model.Resolution);
        }

        internal void RasterizeColumns(
            int count, 
            Func<int, double> x0, 
            Func<int, double> y0, 
            Func<int, double> x1, 
            Func<int, double> y1, 
            bool colorByGradient,
            double worldZero)
        {
            IndicatorRenderer.RasterizeColumns(
                count,
                x0,
                y0,
                x1,
                y1,
                colorByGradient,
                worldZero,
                Columns,
                positiveColumns,
                negativeColumns);
        }

        internal void UpdateTrendlineBrush()
        {
            Model.ClearValue(FinancialIndicator.ActualTrendLineBrushProperty);
            if (IndicatorModel.TrendLineBrush != null)
            {
                Model.SetBinding(
                    FinancialIndicator.ActualTrendLineBrushProperty, 
                    new Binding(Series.TrendLineBrushPropertyName) { Source = Model });
            }
            else
            {
                Model.SetBinding(
                    FinancialIndicator.ActualTrendLineBrushProperty, 
                    new Binding(Series.ActualBrushPropertyName) { Source = Model });
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