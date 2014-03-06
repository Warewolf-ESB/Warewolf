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
    internal class ScatterLineSeriesView : ScatterBaseView
    {
        public ScatterLineSeriesView(ScatterBase model)
            : base(model)
        {

        }

        internal Path Polyline = new Path();

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            Polyline.Detach();

            rootCanvas.Children.Add(Polyline);
            Polyline.SetBinding(Shape.StrokeProperty, new Binding(ScatterLineSeries.ActualBrushPropertyName) { Source = this.Model });
            Polyline.SetBinding(Shape.StrokeThicknessProperty, new Binding(ScatterLineSeries.ThicknessPropertyName) { Source = this.Model });
            Polyline.SetBinding(Shape.StrokeDashCapProperty, new Binding(ScatterLineSeries.DashCapPropertyName) { Source = this.Model });
            Polyline.SetBinding(Shape.StrokeDashArrayProperty, new Binding(ScatterLineSeries.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });

            Polyline.SetBinding(Shape.StrokeLineJoinProperty, new Binding(ScatterLineSeries.LineJoinPropertyName) { Source = this.Model });
            Polyline.SetBinding(Shape.StrokeMiterLimitProperty, new Binding(ScatterLineSeries.MiterLimitPropertyName) { Source = this.Model });
            Polyline.SetBinding(Shape.StrokeStartLineCapProperty, new Binding(ScatterLineSeries.StartCapPropertyName) { Source = this.Model });
            Polyline.SetBinding(Shape.StrokeEndLineCapProperty, new Binding(ScatterLineSeries.StartCapPropertyName) { Source = this.Model });
            Polyline.SetBinding(Shape.StrokeLineJoinProperty, new Binding(ScatterLineSeries.LineJoinPropertyName) { Source = this.Model });

            if (!IsThumbnailView)
            {
                this.Model.RenderSeries(false);
            }
        }
        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        protected internal override void ClearRendering(bool wipeClean)
        {
            base.ClearRendering(wipeClean);
            this.Polyline.Data = null;
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