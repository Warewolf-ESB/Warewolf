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
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    internal class VerticalAxisLabelPanelView
        : AxisLabelPanelBaseView
    {
        protected VerticalAxisLabelPanel VerticalModel { get; set; }
        public VerticalAxisLabelPanelView(VerticalAxisLabelPanel model)
            : base(model)
        {
            VerticalModel = model;
        }

        internal void BindExtent()
        {
            Model.SetBinding(Panel.WidthProperty, new Binding("Extent") { Source = Model });
        }

        internal void HandleHorizontalAlignment(List<Rect> rectangles, double largestWidth)
        {
            for (int i = 0; i < Model.TextBlocks.Count; i++)
            {
                FrameworkElement currentTextBlock = Model.TextBlocks[i];
                double rectWidth = Model.Extent < GetDesiredWidth(currentTextBlock)
                                        ? 0
                                        : GetDesiredWidth(currentTextBlock);

                Rect currentRect = rectangles[i];
                switch (currentTextBlock.HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        currentRect.X = 0;
                        currentRect.Width = rectWidth;
                        break;
                    case HorizontalAlignment.Center:
                        currentRect.X = Model.Extent / 2 - GetDesiredWidth(currentTextBlock) / 2;
                        currentRect.Width = rectWidth;
                        break;
                    case HorizontalAlignment.Right:
                        currentRect.X = Model.Extent - GetDesiredWidth(currentTextBlock);
                        currentRect.Width = rectWidth;
                        break;
                    case HorizontalAlignment.Stretch:
                        currentRect.X = 0;
                        currentRect.Width = rectWidth;

                        if (Model.Extent >= GetDesiredWidth(currentTextBlock))
                        {
                            currentRect.Width = Model.Extent;
                        }
                        break;
                }
                rectangles[i] = currentRect;
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