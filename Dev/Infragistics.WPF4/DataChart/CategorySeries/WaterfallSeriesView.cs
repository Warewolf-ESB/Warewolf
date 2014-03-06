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
    internal class WaterfallSeriesView
        : AnchoredCategorySeriesView
    {
        protected WaterfallSeries WaterfallModel { get; set; }
        public WaterfallSeriesView(WaterfallSeries model)
            : base(model)
        {
            WaterfallModel = model;
        }

        Path positivePath = new Path() { Data = new GeometryGroup() { FillRule = FillRule.Nonzero } };
        Path negativePath = new Path() { Data = new GeometryGroup() { FillRule = FillRule.Nonzero } };

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            positivePath.Detach();
            negativePath.Detach();

            rootCanvas.Children.Add(positivePath);
            positivePath.SetBinding(Rectangle.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
            positivePath.SetBinding(Rectangle.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = Model });
            positivePath.SetBinding(Rectangle.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            positivePath.SetBinding(Rectangle.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });

            positivePath.SetBinding(Rectangle.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });

            rootCanvas.Children.Add(negativePath);
            negativePath.SetBinding(Rectangle.FillProperty, new Binding(WaterfallSeries.NegativeBrushPropertyName) { Source = Model });
            negativePath.SetBinding(Rectangle.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = Model });
            negativePath.SetBinding(Rectangle.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            negativePath.SetBinding(Rectangle.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsNegativeVisual(negativePath, true);

            negativePath.SetBinding(Rectangle.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });

            if (!IsThumbnailView)
            {
                Model.RenderSeries(false);
            }
        }

        internal void ClearWaterfall()
        {
            GeometryGroup positiveGroup = positivePath.Data as GeometryGroup;
            GeometryGroup negativeGroup = negativePath.Data as GeometryGroup;

            positiveGroup.Reset();
            negativeGroup.Reset();
        }

        internal GeometryGroup GetPositiveGeometryGroup()
        {
            return positivePath.Data as GeometryGroup;
        }

        internal GeometryGroup GetNegativeGeometryGroup()
        {
            return negativePath.Data as GeometryGroup;
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