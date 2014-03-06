
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class AxisLabelsVertical : AxisLabelsBase
    {
        #region Methods

        internal AxisLabelsVertical()
        {
        }

        /// <summary>
        /// This method draws axis labels and calculate size and orientation.
        /// </summary>
        internal override void Draw(DrawingContext context)
        {
            // Define a bounding rectangle
            context.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Transparent, 1), new Rect(0, 0, Width, Height));

            CultureInfo cultureToUse = CultureInformation.CultureToUse;

            // Create formated text array
            _formattedLabels = new FormattedText[_labels.Count];
            for (int labelIndx = 0; labelIndx < _labels.Count; labelIndx++)
            {
                FormattedText text = new FormattedText(
                 _labels[labelIndx],
                 cultureToUse,
                 FlowDirection.LeftToRight,
                 new Typeface("Times New Roman"),
                 _fontSize,
                 Brushes.Black);

                // Set Font, Style and max line count. 
                text.MaxLineCount = 1;
                text.SetFontFamily(_fontFamily);
                text.SetFontStyle(_fontStyle);
                text.SetFontWeight(_fontWeight);
                text.SetFontStretch(_fontStretch);
                text.SetForegroundBrush(_fontBrush);
                _formattedLabels[labelIndx] = text;
            }

            double autoAngle = 0;
            double autoFontSize = 0;
            double maxWidths;

            // Calculate auto angle and font size for the labels
            AutoLabels(out autoAngle, out autoFontSize, out maxWidths);

            // Set Auto Font Size
            if (_fontSize != autoFontSize)
            {
                SetFontSize(autoFontSize);
            }

            // Set maximum size of the text
            this.SetMaxTextWidth(maxWidths);

            // Set auto angle
            if (_autoAngleEnabled)
            {
                _textAngle = autoAngle;
            }

            // Set clip region
            context.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));

            // Labels loop
            for (int labelIndx = 0; labelIndx < _labels.Count; labelIndx++)
            {
                FormattedText text = _formattedLabels[labelIndx];

                // Set the position of the coordinate system for the current label.
                if (LeftAxisLabels)
                {
                    context.PushTransform(new TranslateTransform(LabelDistance, GetPosition(labelIndx)));
                }
                else
                {
                    context.PushTransform(new TranslateTransform(Width - LabelDistance, GetPosition(labelIndx)));
                }

                // Rotate the label
                context.PushTransform(new RotateTransform(_textAngle));

                // Draw the text and set position using translated 
                // and rotated coordinate system. Every label orientation 
                // has different positioning of the labels.
                if (LeftAxisLabels)
                {
                    context.DrawText(text, new Point(0, -text.Height / 2));
                }
                else
                {
                    context.DrawText(text, new Point(-text.Width, -text.Height / 2));
                }
          
                context.Pop();
                context.Pop();
            }

            context.Pop();

            if (!this.Is3D)
            {
                // Minor TickMarks
                DrawTickMarks(context, this.Axis.AxisType, false);

                // Major TickMarks
                DrawTickMarks(context, this.Axis.AxisType, true);
            }
        
        }

        /// <summary>
        /// This method calculates automatic axis angle, font size and maximum text size.
        /// </summary>
        /// <param name="autoAngle">Automatic axis angle</param>
        /// <param name="autoFontSize">Auto font size</param>
        /// <param name="maxWidth">Maximum text size</param>
        private void AutoLabels(out double autoAngle, out double autoFontSize, out double maxWidth)
        {
            maxWidth = 0;

            // Recalculate max text width
            HorizontalPosition(out autoAngle, out autoFontSize, out maxWidth);

            LabelsOverlaping(autoAngle, ref autoFontSize);
        }

        /// <summary>
        /// This method calculates the best font size for horizontal labels 
        /// and checks if horizontal labels are possible without overlaping 
        /// and with minimum font size.
        /// </summary>
        /// <param name="autoAngle">Auto angle</param>
        /// <param name="autoFontSize">Auto font size</param>
        /// <param name="maxWidth">Maximum text size</param>
        /// <returns>True if labels can be horizontal without overlaping</returns>
        private bool HorizontalPosition(out double autoAngle, out double autoFontSize, out double maxWidth)
        {
            bool overlap = false;
            autoAngle = 0;
            double oldFontSize = _fontSize;
            double currentFontSize;

            // Find the longest label
            int longestLabelIndx = FindLongestLabel();

            if (longestLabelIndx == -1)
            {
                autoFontSize = _fontSize;

                // Set maximum size of the labels text
                maxWidth = (Width - LabelDistance);
                return false;
            }

            FormattedText longestLabel = _formattedLabels[longestLabelIndx];

            // Font Size loop
            for (currentFontSize = _fontSize; currentFontSize > _minimumFontSize; currentFontSize--)
            {
                longestLabel.SetFontSize(currentFontSize);

                double labelWidth = longestLabel.Width;

                // Check a distance between a label and the rectangle border
                if (labelWidth <= Width - LabelDistance)
                {
                    overlap = false;
                    break;
                }
                else
                {
                    overlap = true;
                }
            }

            // Set maximum size of the labels text
            maxWidth = ( Width - LabelDistance );

            if (maxWidth < 0 || double.IsNaN(maxWidth))
            {
                maxWidth = 0;
            }

            // Set old font size
            longestLabel.SetFontSize(oldFontSize);

            if (_autoFontSizeEnabled)
            {
                autoFontSize = currentFontSize;
            }
            else
            {
                autoFontSize = oldFontSize;
            }
           
            if (overlap)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks if horizontal labels overlap and calculates the best font size 
        /// to avoid overlaping.
        /// </summary>
        /// <param name="labelsAngle">The label font size</param>
        /// <param name="autoFontSize">The best font size</param>
        /// <returns>True if labels overlap</returns>
        private bool LabelsOverlaping(double labelsAngle, ref double autoFontSize)
        {
            double oldFontsize = autoFontSize;

            double currentFontSize;
            bool overlap = false;

            // Font size loop
            for (currentFontSize = autoFontSize; currentFontSize > _minimumFontSize; currentFontSize--)
            {
                overlap = false;
                this.SetFontSize(currentFontSize);

                // Labels loop
                for (int labelIndx = 0; labelIndx < _labels.Count - 1; labelIndx++)
                {
                    double firstLabelPosition = this.GetPosition(labelIndx);
                    double firstLabelHeight = _formattedLabels[labelIndx].Extent;
                    double secondLabelPosition = this.GetPosition(labelIndx + 1);
                    double secondLabelHeight = _formattedLabels[labelIndx + 1].Extent;

                    // Find the distance between two labels
                    double distance = Math.Abs(firstLabelPosition - secondLabelPosition);

                    if (distance < firstLabelHeight / 2 + secondLabelHeight / 2)
                    {
                        overlap = true;
                        break;
                    }
                }
                if (!overlap)
                {
                    break;
                }
            }
            autoFontSize = currentFontSize;

            // Set old font size
            this.SetFontSize(oldFontsize);

            return overlap;
        }

        #endregion Methods
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