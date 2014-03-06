using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Data;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for horizontal axis label panels.
    /// </summary>
    public abstract class HorizontalAxisLabelPanelBase : AxisLabelPanelBase
    {
        internal override AxisLabelPanelBaseView CreateView()
        {
            return new HorizontalAxisLabelPanelBaseView(this);
        }
        internal override void OnViewCreated(AxisLabelPanelBaseView view)
        {
            base.OnViewCreated(view);
            HorizontalView = (HorizontalAxisLabelPanelBaseView)view;
        }
        internal HorizontalAxisLabelPanelBaseView HorizontalView { get; set; }

        internal double LargestHeight { get; set; }

        /// <summary>
        /// Sets up the horizontal axis extent.
        /// </summary>
        protected internal override void BindExtent()
        {
            HorizontalView.BindExtent();
        }

        /// <summary>
        /// Determines whether or not the axis labels should be rotated.
        /// </summary>
        /// <returns>True if the labels should be rotated; otherwise false</returns>
        protected internal virtual bool ShouldRotate()
        {
            return Axis.LabelSettings != null
                 && Axis.LabelSettings.HasUserAngle()
                 && GetEffectiveAngle() % 360 != 0;
        }

        /// <summary>
        /// Determines if the axis labels should be staggered.
        /// </summary>
        /// <returns>True if labels should be staggered; otherwise false</returns>
        protected internal virtual bool ShouldTryStagger()
        {
            return FoundCollisions && !UseRotation;
        }

        /// <summary>
        /// Returns a list of label placeholder rectangles.
        /// </summary>
        /// <returns>List of label bounds</returns>
        protected internal override List<Rect> CreateBoundsRectangles()
        {
            List<Rect> rectangles = new List<Rect>();
            UseStaggering = false;
            UseRotation = false;
            UseWrapping = false;

            double angleDegrees = Math.Abs(GetEffectiveAngle() % 180);
            double angleRadians = Axis.LabelSettings != null ? (Axis.LabelSettings.Angle * Math.PI / 180) : 0.0;

             //Apply rotation if it's explicitly set
            if (ShouldRotate())
            {
                this.UseRotation = true;
            }

            //find the tallest label and create the initial list of label bounds.
            HorizontalView.DetermineTallestLabel(rectangles, angleRadians);

            //check for collisions by checking to see if any of the label rectangles ovelap.
            FoundCollisions = FoundCollisions || DetectCollisions(rectangles);

            if (FoundCollisions)
            {
                HideOptionalLabels(rectangles);
            }
            else
            {
                ShowOptionalLabels();
            }

            //The extent hasn't been set by the user, so we auto-calculate it.
            if (!this.Axis.HasUserExtent())
            {
                if (!FoundCollisions)
                {
                    //the simple case: labels fit without collisions. Use the textblock height as the extent.
                    this.Extent = LargestHeight;
                }
                else
                {
                    double defaultExtent = this.Axis.LabelSettings != null && !double.IsNaN(this.Axis.LabelSettings.Extent) ? this.Axis.LabelSettings.Extent : AxisLabelSettings.ExtentPropertyDefault;
                    //the not-so-simple case. We have to resolve the collisions in a friendly way to determine the height of the labels panel.
                    //let's try using the smaller value between the default extent and the height of the longest label.
                    this.Extent = Math.Min(defaultExtent, GetDesiredHeight(LongestTextBlock));
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
                    currentRect.X = LabelPositions[i].Value;
                    rectangles[i] = currentRect;
                }
            }

            //now fix all collisions. If there's rotation set, do not stagger and vice versa.
            //try staggering first in 2 or 3 levels. If still colliding - wrap instead of staggering. 
            //If not enough room for staggering or wrapping - font scale.
            //If font becomes too small - clip.

            //Stagger logic. Make sure staggering can be successfully applied. 
            //Check for collisions after each stagger operation, because 
            int staggerLevels = 0;
            if (ShouldTryStagger())
            {
                staggerLevels = StaggerLabels(LargestHeight, ref rectangles);
            }

            //if there are still collisions, staggering didn't work and we need to do something else.
            if (FoundCollisions && !UseStaggering)
            {
                if (staggerLevels > 0)
                {
                    //word wrap for category axes
                    //wrapping requires vertical alignment to be set to Stretch.
                    if (HorizontalView.ShouldUseWrapping())
                    {
                        UseWrapping = true;
                    }
                }
            }

            //last resort: clip the labels
            if (FoundCollisions && ShouldClip())
            {



                double actualWidth = ActualWidth;

                double span = LabelPositions.Count > 1 ? LabelPositions[1].Value - LabelPositions[0].Value : actualWidth;
                double clipWidth = Math.Abs(span * 0.8);
                double RectX = 0;
                double startClippingAngle =
                    Math.Abs(Math.Atan(LargestHeight / span) * 180 / Math.PI);

                for (int i = 0; i < rectangles.Count; i++)
                {
                    double clipValueToUse = Math.Min(GetDesiredWidth(TextBlocks[i]), clipWidth);

                    RectX = UseRotation ? LabelPositions[i].Value : LabelPositions[i].Value - clipValueToUse/2;
                    
                    //optimization for rotation
                    bool optimize = angleDegrees >= startClippingAngle && angleDegrees <= 180 - startClippingAngle; 

                    if (UseRotation && optimize)
                    {

                        double textwidth = GetDesiredWidth(TextBlocks[i]);
                        clipWidth = Math.Min(Extent/Math.Abs(Math.Sin(angleRadians)), textwidth);
                    }

                    Rect currentRect = rectangles[i];
                    currentRect.X = RectX;
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
                        
                        if (UseWrapping)
                        {
                            textBlock.Height = Extent;

                            textBlock.TextTrimming = TextTrimming.WordEllipsis;
                            textBlock.TextWrapping = TextWrapping.Wrap;

                        }
                        
                        else if (!textBlock.Text.Equals(newText))
                        {

                            textBlock.TextWrapping = TextWrapping.NoWrap;

                            textBlock.Text = newText;
                            TextBlocks[i].Width = clipWidth;
                        }
                    }
                }
            }

            //alignment is done when the labels are not rotated or staggered.
            //if the extent is smaller than the label's desired height, the label is not displayed.
            if (ShouldVerticalAlign())
            {
                DoVerticalAlignment(rectangles);
            }

            return rectangles;
        }

        private void DoVerticalAlignment(List<Rect> rectangles)
        {
            HorizontalView.HandleVerticalAlignment(rectangles);
        }

        /// <summary>
        /// Hides optional labels
        /// </summary>
        /// <param name="rectangles">list of label rectangles</param>
        protected virtual void HideOptionalLabels(List<Rect> rectangles)
        {
        }

        /// <summary>
        /// Shows optional labels.
        /// </summary>
        protected virtual void ShowOptionalLabels()
        {
        }

        /// <summary>
        /// Sets up a rotate transform object for the labels.
        /// </summary>
        /// <param name="label">label to apply the transformation to</param>
        /// <param name="angle">rotatoin angle</param>
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
        /// Determines if the labels should be vertically aligned.
        /// </summary>
        /// <returns>True if the labels should be vertically aligned; other wise false</returns>
        protected virtual bool ShouldVerticalAlign()
        {
            return !UseStaggering && !UseRotation;
        }

        /// <summary>
        /// Determines is the labels should be clipped.
        /// </summary>
        /// <returns>True if the labels should be clipped; otherwise false</returns>
        protected virtual bool ShouldClip()
        {
            return true;
        }

        /// <summary>
        /// Staggers the axis labels.
        /// </summary>
        /// <param name="largestHeight">largest label's height</param>
        /// <param name="rectangles">list of label bounds</param>
        /// <returns>number of rows after staggering</returns>
        protected override int StaggerLabels(double largestHeight, ref List<Rect> rectangles)
        {
            //note: the scaling and clipping can be applied to staggered labels.
            int staggerLevels = 0;
            List<Rect> tempRectangles = new List<Rect>();

            if (largestHeight * 3 <= this.Extent)
            {
                staggerLevels = 3;
            }
            else if (largestHeight * 2 <= this.Extent)
            {
                staggerLevels = 2;
            }
            else
            {
                return staggerLevels;
            }

            //do an initial staggering
            for (int i = 0; i < rectangles.Count; i++)
            {
                Rect rect = rectangles[i].Duplicate();
                if (i % staggerLevels == 0)
                {
                    rect.Y = 0;
                }
                else if (i % staggerLevels == staggerLevels - 1)
                {
                    rect.Y = this.Extent - rect.Height;
                }
                else
                {
                    rect.Y = this.Extent / 2 - rect.Height / 2;
                }
                tempRectangles.Add(rect);
            }

            FoundCollisions = DetectCollisions(tempRectangles);

            if (!FoundCollisions)
            {
                this.UseStaggering = true;
                rectangles = tempRectangles;
            }

            return staggerLevels;
        }
    }

    /// <summary>
    /// Represents a panel control, containing horizontal axis labels.
    /// </summary>
    public class HorizontalAxisLabelPanel : HorizontalAxisLabelPanelBase
    {
        internal override AxisLabelsLocation GetDefaultLabelsLocation()
        {
            return AxisLabelsLocation.OutsideBottom;
        }

        internal override bool ValidLocation(AxisLabelsLocation location)
        {
            return location == AxisLabelsLocation.InsideBottom ||
                location == AxisLabelsLocation.InsideTop ||
                location == AxisLabelsLocation.OutsideBottom ||
                location == AxisLabelsLocation.OutsideTop;
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