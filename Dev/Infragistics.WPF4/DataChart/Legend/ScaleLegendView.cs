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

namespace Infragistics.Controls.Charts
{
    internal class ScaleLegendView
        : LegendBaseView
    {
        public ScaleLegendView(ScaleLegend model)
            : base(model)
        {
            ScaleModel = model;
        }
        internal ScaleLegend ScaleModel { get; set; }

        internal override void OnTemplateProvided()
        {
            base.OnTemplateProvided();

            LegendScaleElement = ScaleModel.GetTemplateElement(LegendScaleElementName) as UIElement;
            MinText = ScaleModel.GetTemplateElement("MinText") as TextBlock;
            MaxText = ScaleModel.GetTemplateElement("MaxText") as TextBlock;

            Shape scaleShape = LegendScaleElement as Shape;
            if (scaleShape != null)
            {
                OriginalScaleBrush = scaleShape.Fill;
            }

            if (MinText != null) OriginalMinText = MinText.Text;
            if (MaxText != null) OriginalMaxText = MaxText.Text;

            ScaleModel.RenderLegend();
        }

        internal Brush OriginalScaleBrush { get; set; }
        internal string OriginalMinText { get; set; }
        internal string OriginalMaxText { get; set; }

        private const string LegendScaleElementName = "LegendScale";

        /// <summary>
        /// Gets the shape that represents the legend scale.
        /// </summary>
        public UIElement LegendScaleElement { get; private set; }

        /// <summary>
        /// Gets the TextBlock that shows the minimum scale value.
        /// </summary>
        public TextBlock MinText { get; private set; }

        /// <summary>
        /// Gets the TextBlock that shows the maximum scale value.
        /// </summary>
        public TextBlock MaxText { get; private set; }

        internal void RestoreOriginalState()
        {
            Shape scaleShape = LegendScaleElement as Shape;
            if (scaleShape != null)
            {
                scaleShape.Fill = OriginalScaleBrush;
            }

            if (OriginalMinText != null)
            {
                MinText.Text = OriginalMinText;
            }

            if (OriginalMaxText != null)
            {
                MaxText.Text = OriginalMaxText;
            }
        }

        internal override void DetachContent()
        {
            
        }

        internal Color GetTransparentBrush()
        {
            return Colors.Transparent;
        }

        internal object BuildGradient()
        {
            LinearGradientBrush finalBrush = new LinearGradientBrush();
            return finalBrush;
        }

        internal void AddGradientStop(object gradient, Color color, double colorOffset)
        {
            ((LinearGradientBrush)gradient).GradientStops.Add(new GradientStop { Color = color, Offset = colorOffset });
        }

        internal void SetScaleFill(System.Windows.Shapes.Shape legendScaleShapeElement, bool useSeriesBrush, object gradient)
        {
            legendScaleShapeElement.Fill = useSeriesBrush ? ScaleModel.Series.ActualMarkerBrush : (Brush)gradient;
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