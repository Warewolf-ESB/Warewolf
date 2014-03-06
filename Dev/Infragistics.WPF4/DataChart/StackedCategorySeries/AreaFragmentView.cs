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
    internal class AreaFragmentView : AnchoredCategorySeriesView
    {
        internal AreaFragment AreaFragmentModel { get; private set; }

        public AreaFragmentView(AreaFragment model)
            : base(model)
        {
            AreaFragmentModel = model;
        }

        internal Path polygon0 = new Path();
        internal Path polyline0 = new Path();
        internal Path polygon1 = new Path();
        internal Path polyline1 = new Path();

        internal void ClearRendering()
        {
            polygon0.Data = null;
            polygon1.Data = null;
            polyline0.Data = null;
            polyline1.Data = null;
        }

        internal override CategoryBucketCalculator CreateBucketCalculator()
        {
            return new AreaFragmentBucketCalculator(this);
        }

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            polygon0.Detach();
            rootCanvas.Children.Add(polygon0);
            polygon0.SetBinding(Shape.FillProperty, new Binding(AreaFragment.ActualBrushPropertyName) { Source = this.Model });

            polygon1.Detach();
            rootCanvas.Children.Add(polygon1);
            polygon1.SetBinding(Shape.FillProperty, new Binding(AreaFragment.ActualBrushPropertyName) { Source = this.Model });
            polygon1.Opacity = 0.5;
            VisualInformationManager.SetIsTranslucentPortionVisual(polygon1, true);

            polyline0.Detach();
            rootCanvas.Children.Add(polyline0);
            polyline0.SetBinding(Shape.StrokeProperty, new Binding(AreaFragment.ActualOutlinePropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeThicknessProperty, new Binding(AreaFragment.ThicknessPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeDashCapProperty, new Binding(AreaFragment.DashCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeDashArrayProperty, new Binding(AreaFragment.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsOutlineVisual(polyline0, true);

            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(AreaFragment.LineJoinPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(AreaFragment.MiterLimitPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(AreaFragment.StartCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(AreaFragment.EndCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(AreaFragment.LineJoinPropertyName) { Source = this.Model });

            polyline1.Detach();
            rootCanvas.Children.Add(polyline1);
            polyline1.SetBinding(Shape.StrokeProperty, new Binding(AreaFragment.ActualOutlinePropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeThicknessProperty, new Binding(AreaFragment.ThicknessPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeDashCapProperty, new Binding(AreaFragment.DashCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeDashArrayProperty, new Binding(AreaFragment.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });
            VisualInformationManager.SetIsOutlineVisual(polyline1, true);

            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(AreaFragment.LineJoinPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(AreaFragment.MiterLimitPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(AreaFragment.StartCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(AreaFragment.EndCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(AreaFragment.LineJoinPropertyName) { Source = this.Model });

            if (!IsThumbnailView)
            {
                this.Model.RenderSeries(false);
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