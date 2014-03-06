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
    internal class SplineFragmentView : SplineSeriesBaseView
    {
        internal SplineFragment SplineFragmentModel { get; private set; }

        public SplineFragmentView(SplineFragment model)
            : base(model)
        {
            SplineFragmentModel = model;
        }

        internal override CategoryBucketCalculator CreateBucketCalculator()
        {
            return new SplineFragmentBucketCalculator(this);
        }

        internal Path polyline0 = new Path();
        internal Path polygon01 = new Path();
        internal Path polyline1 = new Path();

        internal void ClearRendering()
        {
            polygon01.Data = null;
            polyline0.Data = null;
            polyline1.Data = null;
        }

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            polygon01.Detach();
            polyline0.Detach();
            polyline1.Detach();

            rootCanvas.Children.Add(polygon01);
            polygon01.SetBinding(Shape.FillProperty, new Binding(SplineFragment.ActualBrushPropertyName) { Source = this.Model });
            polygon01.Opacity = 0.75;
            VisualInformationManager.SetIsTranslucentPortionVisual(polygon01, true);

            rootCanvas.Children.Add(polyline0);
            polyline0.SetBinding(Shape.StrokeProperty, new Binding(SplineFragment.ActualBrushPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeThicknessProperty, new Binding(SplineFragment.ThicknessPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeDashCapProperty, new Binding(SplineFragment.DashCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeDashArrayProperty, new Binding(SplineFragment.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });

            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(SplineFragment.LineJoinPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(SplineFragment.MiterLimitPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(SplineFragment.StartCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(SplineFragment.EndCapPropertyName) { Source = this.Model });
            polyline0.SetBinding(Shape.StrokeLineJoinProperty, new Binding(SplineFragment.LineJoinPropertyName) { Source = this.Model });

            rootCanvas.Children.Add(polyline1);
            polyline1.SetBinding(Shape.StrokeProperty, new Binding(SplineFragment.ActualBrushPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeThicknessProperty, new Binding(SplineFragment.ThicknessPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeDashCapProperty, new Binding(SplineFragment.DashCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeDashArrayProperty, new Binding(SplineFragment.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });

            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(SplineFragment.LineJoinPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(SplineFragment.MiterLimitPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(SplineFragment.StartCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(SplineFragment.EndCapPropertyName) { Source = this.Model });
            polyline1.SetBinding(Shape.StrokeLineJoinProperty, new Binding(SplineFragment.LineJoinPropertyName) { Source = this.Model });

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