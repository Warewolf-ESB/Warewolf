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
    /// Represents a panel control, containing vertical axis labels.
    /// </summary>
    public class VerticalAxisLabelPanel
        : AxisLabelPanelBase
    {
        internal override AxisLabelPanelBaseView CreateView()
        {
            return new VerticalAxisLabelPanelView(this);
        }
        internal override void OnViewCreated(AxisLabelPanelBaseView view)
        {
            base.OnViewCreated(view);
            VerticalView = (VerticalAxisLabelPanelView)view;
        }
        internal VerticalAxisLabelPanelView VerticalView { get; set; }

        internal double LargestWidth { get; set; }
        internal double LargestHeight { get; set; }

        /// <summary>
        /// Makes sure the extent is bound to the settings.
        /// </summary>
        protected internal override void BindExtent()
        {
            VerticalView.BindExtent();
        }

        /// <summary>
        /// Creates the bounds rectangles for the labels in the panel.
        /// </summary>
        /// <returns>A list of bounds rectangles.</returns>
        protected internal override List<Rect> CreateBoundsRectangles()
        {
            List<Rect> rectangles = new List<Rect>();
            UseStaggering = false;
            UseRotation = false;

            //Apply rotation if it's explicitly set
            if (this.Axis.LabelSettings != null && this.Axis.LabelSettings.HasUserAngle()
                && GetEffectiveAngle() % 360 != 0)
            {
                this.UseRotation = true;
            }

            //find the widest label and create the initial list of label bounds.
            LargestWidth = double.MinValue;
            LargestHeight = double.MinValue;

            for (int i = 0; i < TextBlocks.Count; i++)
            {
                FrameworkElement currentTextBlock = TextBlocks[i];
                double desiredHeight = GetDesiredHeight(currentTextBlock);
                double desiredWidth = GetDesiredWidth(currentTextBlock);
                double x = 0;
                double y = LabelPositions[i].Value - desiredHeight / 2;
                
                LargestWidth = Math.Max(desiredWidth, LargestWidth);
                LargestHeight = Math.Max(desiredHeight, LargestHeight);
                Rect rect = new Rect(x, y, desiredWidth, desiredHeight);
                rectangles.Add(rect);
            }

            //The extent hasn't been set by the user, so we auto-calculate it.
            double angleDegrees = 0;
            if (UseRotation)
            {
                angleDegrees = GetEffectiveAngle() % 360;
                if (angleDegrees < 0)
                {
                    angleDegrees += 360;
                }
            }
            if (!this.Axis.HasUserExtent())
            {
                if (UseRotation)
                {
                    if (angleDegrees >= 90 && angleDegrees <= 270)
                    {
                        this.Extent = LargestHeight;
                    }
                    else
                    {
                        double rad = angleDegrees * (Math.PI / 180);
                        double rotatedLabelWidth = Math.Abs(LargestWidth * Math.Cos(rad)) + Math.Abs(LargestHeight * Math.Sin(rad));
                        this.Extent = rotatedLabelWidth;
                    }
                }
                else
                {
                    this.Extent = LargestWidth;
                }
            }
            else
            {
                View.BindExtentToSettings();
            }

            if (this.UseRotation)
            {
                for (int i = 0; i < rectangles.Count; i++)
                {
                    //sets each label's starting point to be at the tickmark.
                    Rect currentRect = rectangles[i];
                    currentRect.Y = LabelPositions[i].Value - LargestHeight/2;
                    rectangles[i] = currentRect;
                }
            }

            bool skipClipping = false;
            if (!this.UseRotation && LargestWidth <= Extent)
            {
                skipClipping = true;
            }

            if (!skipClipping)
            {



                double actualHeight = ActualHeight;


                double clipWidth = Extent;
                double span = LabelPositions.Count > 1 ? (double)Math.Abs(LabelPositions[1].Value - LabelPositions[0].Value) : actualHeight;
                double startClippingAngle = Math.Abs(Math.Asin(span / LargestWidth) * (180 / Math.PI));
                bool optimize = (angleDegrees >= startClippingAngle && angleDegrees <= 180 - startClippingAngle) ||
                                (angleDegrees - 180 >= startClippingAngle && angleDegrees - 180 <= 180 - startClippingAngle);

                for (int i = 0; i < rectangles.Count; i++)
                {
                    if (UseRotation)
                    {
                        double angleRadians = angleDegrees * (Math.PI / 180);
                        double textwidth = GetDesiredWidth(TextBlocks[i]);
                        clipWidth = Math.Min(Extent / Math.Abs(Math.Cos(angleRadians)), textwidth);

                        if (optimize)
                        {
                            double rad;
                            if (angleDegrees > 0 && angleDegrees < 90)
                            {
                                rad = (Math.PI / 180) * (90 - angleDegrees);
                            }
                            else if (angleDegrees > 270 && angleDegrees < 360)
                            {
                                rad = (Math.PI / 180) * (angleDegrees - 270);
                            }
                            else
                            {
                                rad = 0;
                            }
                            clipWidth = span / Math.Cos(rad);
                        }
                    }

                    Rect currentRect = rectangles[i];
                    currentRect.X = 0;
                    currentRect.Width = clipWidth;
                    rectangles[i] = currentRect;
                    if (!(TextBlocks[i] is TextBlock))
                    {
                        TextBlocks[i].Width = clipWidth;
                    }

                    TextBlock textBlock = TextBlocks[i] as TextBlock;
                    if (textBlock != null)
                    {
                        string newText = TrimTextBlock(i, textBlock, clipWidth);
                        if (!textBlock.Text.Equals(newText))
                        {
                            textBlock.Text = newText;
                            TextBlocks[i].Width = clipWidth;
                        }
                    }
                }
        }


#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

            //alignment is done when the labels are not rotated or staggered.
            //if the extent is smaller than the label's desired height, the label is not displayed.
            if (ShouldHorizontalAlign())
            {
                DoHorizontalAlignment(rectangles);
            }

            return rectangles;
        }

        private void DoHorizontalAlignment(List<Rect> rectangles)
        {
            VerticalView.HandleHorizontalAlignment(rectangles, LargestWidth);
        }

        /// <summary>
        /// Sets the appropriate rotation transform on a label.
        /// </summary>
        /// <param name="label">The label to set.</param>
        /// <param name="angle">The angle to transform.</param>
        protected internal override void SetLabelRotationTransform(FrameworkElement label, double angle)
        {
            double centerX = 0;
            double centerY = LargestHeight / 2;

            var transform =
                new RotateTransform()
                {
                    Angle = GetEffectiveAngle(),
                    CenterX = centerX,
                    CenterY = centerY
                };
            label.RenderTransform = transform;
        }

        /// <summary>
        /// Returns true if horizontal alignment should be performed.
        /// </summary>
        /// <returns>True if horizontal alignment should be performed.</returns>
        protected virtual bool ShouldHorizontalAlign()
        {
            return !UseStaggering && !UseRotation;
        }

        internal override AxisLabelsLocation GetDefaultLabelsLocation()
        {
            return AxisLabelsLocation.OutsideLeft;
        }

        internal override bool ValidLocation(AxisLabelsLocation location)
        {
            return location == AxisLabelsLocation.OutsideLeft ||
                location == AxisLabelsLocation.OutsideRight ||
                location == AxisLabelsLocation.InsideLeft ||
                location == AxisLabelsLocation.InsideRight;
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