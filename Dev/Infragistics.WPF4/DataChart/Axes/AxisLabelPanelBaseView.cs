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
using System.Collections.Generic;
using System.Windows.Data;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Defines the view class of an axis label panel.
    /// </summary>
    public class AxisLabelPanelBaseView
    {
        /// <summary>
        /// Gets or sets the axis label pane object for this view.
        /// </summary>
        protected AxisLabelPanelBase Model { get; set; }

        /// <summary>
        /// Initializes a new instance of the view.
        /// </summary>
        /// <param name="model">Instance of an axis label panel</param>
        public AxisLabelPanelBaseView(AxisLabelPanelBase model)
        {
            Model = model;
        }

        internal double GetDesiredWidth(object element)
        {
            FrameworkElement fe = (FrameworkElement)element;






            return fe.DesiredSize.Width;
        }

        internal double GetDesiredHeight(object element)
        {
            FrameworkElement fe = (FrameworkElement)element;






            return fe.DesiredSize.Height;
        }

        internal void DetermineLongestLabel()
        {
            var labels = GetLabels();

            foreach (var child in labels)
            {
                FrameworkElement textElement = child as FrameworkElement;
                if (textElement != null)
                {
                    if (textElement is TextBlock && textElement.ReadLocalValue(TextBlock.WidthProperty) != DependencyProperty.UnsetValue)
                    {
                        textElement.ClearValue(TextBlock.WidthProperty);



                    }
                    Model.OnProcessTextBlock(textElement);






                    textElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Model.ConsiderForLongestTextBlock(textElement);
                }
            }
        }

        private IEnumerable<object> GetLabels()
        {
            var textBlocks = Model.GetTextBlocks();
            var children = Model.GetChildren();

            if (!Model.Axis.UsingTemplate)
            {
                return textBlocks;
            }
            else
            {
                return children;
            }
        }

        internal void ArrangeToBounds(object element, Rect rect)
        {
            FrameworkElement fe = (FrameworkElement)element;
            fe.Arrange(rect);
        }

        internal void ClearTransforms(object element)
        {
            FrameworkElement fe = (FrameworkElement)element;
            fe.RenderTransform = null;
        }

        internal void HandleSetLabelRotationTransform(object label, double angle)
        {
            FrameworkElement fe = label as FrameworkElement;
            double centerX = 0, centerY = 0;
            var transform =
                new RotateTransform()
                {
                    Angle = angle,
                    CenterX = centerX,
                    CenterY = centerY
                };
            fe.RenderTransform = transform;
        }

        internal void HandleMeasureLabel(object element)
        {
            FrameworkElement fe = element as FrameworkElement;
            fe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        internal string TrimTextBlock(TextBlock textblock, double availableWidth)
        {
            string text = textblock.Text;
            int length = text.Length;

            TextBlock currentTextBlock = new TextBlock()
            {
                Text = text,
                FontFamily = textblock.FontFamily,
                FontSize = textblock.FontSize,
                FontStretch = textblock.FontStretch,
                FontWeight = textblock.FontWeight,
                TextTrimming = textblock.TextTrimming,
                TextWrapping = textblock.TextWrapping
            };
            double currentWidth = 0.0;
            currentWidth = currentTextBlock.ActualWidth;

            currentTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            currentWidth = currentTextBlock.DesiredSize.Width;

            while (currentWidth > availableWidth && length > 0)
            {
                length--;
                currentTextBlock.Text = text.Substring(0, length) + "\u2026";
                currentWidth = currentTextBlock.ActualWidth;

                currentTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                currentWidth = currentTextBlock.DesiredSize.Width;

            }

            return currentTextBlock.Text;
        }

        internal virtual void OnInit()
        {
            
        }

        internal void BindExtentToSettings()
        {
            Model.SetBinding(AxisLabelPanelBase.ExtentProperty, 
                new Binding("Extent") { 
                    Source = Model.Axis.LabelSettings, 
                    Mode = BindingMode.OneWay });
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