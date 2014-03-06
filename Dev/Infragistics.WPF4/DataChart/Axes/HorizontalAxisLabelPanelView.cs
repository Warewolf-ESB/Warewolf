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
    internal class HorizontalAxisLabelPanelBaseView
        : AxisLabelPanelBaseView
    {
        protected HorizontalAxisLabelPanelBase HorizontalModel { get; set; }
        public HorizontalAxisLabelPanelBaseView(HorizontalAxisLabelPanelBase model)
            : base(model)
        {
            HorizontalModel = model;
        }

        internal void BindExtent()
        {
            Model.SetBinding(Panel.HeightProperty, new Binding("Extent") { Source = Model });
        }

        internal void DetermineTallestLabel(List<Rect> rectangles, double angleRadians)
        {
            HorizontalModel.LargestHeight = double.MinValue;
            for (int i = 0; i < Model.TextBlocks.Count; i++)
            {
                if (Model.LabelPositions == null || i >= Model.LabelPositions.Count) break;

                double x, y, width, height, elementWidth;
                FrameworkElement currentLabel = (FrameworkElement)Model.TextBlocks[i];
                TextBlock currentTextBlock = currentLabel as TextBlock;

                if (currentTextBlock != null && Model.UseRotation)
                {
                    currentTextBlock.TextWrapping = TextWrapping.NoWrap;
                }

                if (currentTextBlock != null &&
                    !Model.UseRotation &&
                    Model.Axis.LabelSettings != null &&
                    Model.Axis.LabelSettings.TextWrapping == TextWrapping.Wrap &&
                    Model.Axis.LabelSettings.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    elementWidth = GetDesiredWidth(currentLabel);
                }
                else
                {
                    elementWidth = GetDesiredWidth(currentLabel);
                }

                if (Model.UseRotation)
                {
                    x = Model.LabelPositions[i].Value;
                }
                else
                {
                    x = Model.LabelPositions[i].Value - elementWidth / 2;
                }

                
                y = 0;
                width = GetDesiredWidth(currentLabel);
                height = GetDesiredHeight(currentLabel);

                HorizontalModel.LargestHeight = Math.Max(height, HorizontalModel.LargestHeight);
                Rect rect = new Rect(x, y, width, height);
                rectangles.Add(rect);

                //collisions occur if text goes outside of the label panel.
                double lengthH = elementWidth * Math.Abs(Math.Cos(angleRadians));
                double lengthV = elementWidth * Math.Abs(Math.Sin(angleRadians));

                Model.FoundCollisions = lengthV > Model.ActualHeight;
            }
        }

        internal bool ShouldUseWrapping()
        {
            return Model.Axis.LabelSettings != null &&
                   Model.Axis.LabelSettings.TextWrapping == TextWrapping.Wrap &&
                   Model.Axis.LabelSettings.VerticalAlignment == VerticalAlignment.Stretch;
        }

        internal void HandleVerticalAlignment(List<Rect> rectangles)
        {
            for (int i = 0; i < Model.TextBlocks.Count; i++)
            {
                FrameworkElement currentTextBlock = Model.TextBlocks[i];
                double rectHeight = Model.Extent < GetDesiredHeight(currentTextBlock)
                                        ? 0
                                        : GetDesiredHeight(currentTextBlock);

                Rect currentRect = rectangles[i];
                switch (currentTextBlock.VerticalAlignment)
                {
                    case VerticalAlignment.Top:
                        currentRect.Y = 0;
                        currentRect.Height = rectHeight;
                        break;
                    case VerticalAlignment.Center:
                        currentRect.Y = Model.Extent / 2 - GetDesiredHeight(currentTextBlock) / 2;
                        currentRect.Height = rectHeight;
                        break;
                    case VerticalAlignment.Bottom:
                        currentRect.Y = Model.Extent - GetDesiredHeight(currentTextBlock);
                        currentRect.Height = rectHeight;
                        break;
                    case VerticalAlignment.Stretch:
                        currentRect.Y = 0;
                        currentRect.Height = rectHeight;

                        if (Model.Extent >= GetDesiredHeight(currentTextBlock))
                        {
                            currentRect.Height = Model.Extent;
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