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
    internal class RadialAreaSeriesView : AnchoredRadialSeriesView
    {
        protected RadialAreaSeries RadialAreaModel { get; set; }
        public RadialAreaSeriesView(RadialAreaSeries model)
            : base(model)
        {
            RadialAreaModel = model;
        }

        internal Path polygon0 = new Path();
        internal Path polyline0 = new Path();
        internal Path polygon1 = new Path();
        internal Path polyline1 = new Path();

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            polygon0.Detach();
            polygon1.Detach();
            polyline0.Detach();
            polyline1.Detach();

            rootCanvas.Children.Add(polygon0);
            polygon0.SetBinding(Shape.FillProperty, new Binding(RadialAreaSeries.ActualBrushPropertyName) { Source = this.RadialAreaModel });

            rootCanvas.Children.Add(polygon1);
            polygon1.SetBinding(Shape.FillProperty, new Binding(RadialAreaSeries.ActualBrushPropertyName) { Source = this.RadialAreaModel });
            polygon1.Opacity = 0.5;
            VisualInformationManager.SetIsTranslucentPortionVisual(polygon1, true);

            rootCanvas.Children.Add(polyline0);
            polyline0.SetBinding(Shape.StrokeProperty, new Binding(RadialAreaSeries.ActualOutlinePropertyName) { Source = this.RadialAreaModel });
            polyline0.SetBinding(Shape.StrokeThicknessProperty, new Binding(RadialAreaSeries.ThicknessPropertyName) { Source = this.RadialAreaModel });
            polyline0.SetBinding(Shape.StrokeDashCapProperty, new Binding(RadialAreaSeries.DashCapPropertyName) { Source = this.RadialAreaModel });
            polyline0.SetBinding(Shape.StrokeDashArrayProperty, new Binding(RadialAreaSeries.DashArrayPropertyName)
            {
                Source = this.RadialAreaModel,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsOutlineVisual(polyline0, true);

            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(RadialAreaSeries.LineJoinPropertyName) { Source = this.RadialAreaModel });
            polyline0.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(RadialAreaSeries.MiterLimitPropertyName) { Source = this.RadialAreaModel });
            polyline0.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(RadialAreaSeries.StartCapPropertyName) { Source = this.RadialAreaModel });
            polyline0.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(RadialAreaSeries.EndCapPropertyName) { Source = this.RadialAreaModel });
            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(RadialAreaSeries.LineJoinPropertyName) { Source = this.RadialAreaModel });

            rootCanvas.Children.Add(polyline1);
            polyline1.SetBinding(Shape.StrokeProperty, new Binding(RadialAreaSeries.ActualOutlinePropertyName) { Source = this.RadialAreaModel });
            polyline1.SetBinding(Shape.StrokeThicknessProperty, new Binding(RadialAreaSeries.ThicknessPropertyName) { Source = this.RadialAreaModel });
            polyline1.SetBinding(Shape.StrokeDashCapProperty, new Binding(RadialAreaSeries.DashCapPropertyName) { Source = this.RadialAreaModel });
            polyline1.SetBinding(Shape.StrokeDashArrayProperty, new Binding(RadialAreaSeries.DashArrayPropertyName)
            {
                Source = this.RadialAreaModel,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsOutlineVisual(polyline1, true);
            VisualInformationManager.SetIsMainGeometryVisual(polyline1, true);

            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(RadialAreaSeries.LineJoinPropertyName) { Source = this.RadialAreaModel });
            polyline1.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(RadialAreaSeries.MiterLimitPropertyName) { Source = this.RadialAreaModel });
            polyline1.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(RadialAreaSeries.StartCapPropertyName) { Source = this.RadialAreaModel });
            polyline1.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(RadialAreaSeries.EndCapPropertyName) { Source = this.RadialAreaModel });
            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(RadialAreaSeries.LineJoinPropertyName) { Source = this.RadialAreaModel });

            if (!IsThumbnailView)
            {
                this.RadialAreaModel.RenderSeries(false);
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