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
    internal class FinancialPriceSeriesView
        : FinancialSeriesView
    {
        protected FinancialPriceSeries FinancialPriceModel { get; set; }
        public FinancialPriceSeriesView(FinancialPriceSeries model)
            : base(model)
        {
            FinancialPriceModel = model;

            TrendLineManager = new CategoryTrendLineManager();
        }

        protected override FinancialBucketCalculator CreateBucketCalculator()
        {
            return new FinancialPriceBucketCalculator(this);
        }


        private Path positivePath = new Path() { Data = new PathGeometry() { FillRule = FillRule.Nonzero } };
        private Path negativePath = new Path() { Data = new PathGeometry() { FillRule = FillRule.Nonzero } };




        internal CategoryTrendLineManagerBase TrendLineManager { get; set; }

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);
            
            this.TrendLineManager.AttachPolyLine(rootCanvas, Model);

            positivePath.Detach();
            negativePath.Detach();

            rootCanvas.Children.Add(positivePath);
            positivePath.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
            positivePath.SetBinding(Shape.StrokeProperty, new Binding(FinancialPriceModel.DisplayType == PriceDisplayType.OHLC ? FinancialPriceSeries.ActualBrushPropertyName : Series.ActualOutlinePropertyName) { Source = Model });
            positivePath.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            positivePath.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            positivePath.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            if (FinancialPriceModel.DisplayType != PriceDisplayType.OHLC)
            {
                VisualInformationManager.SetIsOutlineVisual(positivePath, true);
            }

            positivePath.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            positivePath.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            VisualInformationManager.SetIsMultiColorLineVisual(positivePath, true);

            rootCanvas.Children.Add(negativePath);
            negativePath.SetBinding(Shape.FillProperty, new Binding(FinancialSeries.NegativeBrushPropertyName) { Source = Model });
            negativePath.SetBinding(Shape.StrokeProperty, new Binding(FinancialPriceModel.DisplayType == PriceDisplayType.OHLC ? FinancialSeries.NegativeBrushPropertyName : Series.ActualOutlinePropertyName) { Source = Model });
            negativePath.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            negativePath.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });
            negativePath.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            if (FinancialPriceModel.DisplayType == PriceDisplayType.OHLC)
            {
                VisualInformationManager.SetIsNegativeVisual(negativePath, true);
            }
            else
            {
                VisualInformationManager.SetIsOutlineVisual(negativePath, true);
            }

            negativePath.SetBinding(Shape.StrokeLineJoinProperty, new Binding(Series.LineJoinPropertyName) { Source = Model });
            negativePath.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(Series.MiterLimitPropertyName) { Source = Model });
            VisualInformationManager.SetIsMultiColorLineVisual(negativePath, true);

            if (!IsThumbnailView)
            {
                Model.RenderSeries(false);
            }
        }

        internal void UpdatePathBrushes()
        {
            positivePath.SetBinding(Shape.StrokeProperty, new Binding(FinancialPriceModel.DisplayType == PriceDisplayType.OHLC ? Series.ActualBrushPropertyName : Series.ActualOutlinePropertyName) { Source = Model });
            negativePath.SetBinding(Shape.StrokeProperty, new Binding(FinancialPriceModel.DisplayType == PriceDisplayType.OHLC ? FinancialSeries.NegativeBrushPropertyName : Series.ActualOutlinePropertyName) { Source = Model });
        }

        internal void ClearPriceSymbols()
        {

            PathGeometry positiveGroup = positivePath.Data as PathGeometry;
            PathGeometry negativeGroup = negativePath.Data as PathGeometry;




            positiveGroup.Reset();
            negativeGroup.Reset();
        }


        internal PathGeometry GetPositiveGroup()
        {
            return positivePath.Data as PathGeometry;
        }








        internal PathGeometry GetNegativeGroup()
        {
            return negativePath.Data as PathGeometry;
        }







        internal void UpdateTrendlineBrush()
        {
            FinancialPriceModel.ClearValue(FinancialPriceSeries.ActualTrendLineBrushProperty);
            if (FinancialPriceModel.TrendLineBrush != null)
            {
                FinancialPriceModel.SetBinding(FinancialPriceSeries.ActualTrendLineBrushProperty, new Binding(Series.TrendLineBrushPropertyName) { Source = Model });
            }
            else
            {
                FinancialPriceModel.SetBinding(FinancialPriceSeries.ActualTrendLineBrushProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
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