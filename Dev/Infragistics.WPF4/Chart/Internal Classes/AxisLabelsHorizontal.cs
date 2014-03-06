
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
    /// <summary>
    /// This class is used to draw labels and to automatically recalculate 
    /// the best orientation, angle and font size for labels. This class 
    /// is used for top and down axes labels only. Label angle and/or font size 
    /// can be set to auto values. 
    /// </summary>
    internal class AxisLabelsHorizontal : AxisLabelsBase
    {
        #region Methods

        internal AxisLabelsHorizontal()
        {
        }

        /// <summary>
        /// This method draws axis labels and calculate size and orientation.
        /// </summary>
        internal override void Draw(DrawingContext context)
        {
            // No labels
            if (_labels.Count == 0)
            {
                return;
            }

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
            LabelOrientation orientation = AutoLabels(out autoAngle, out autoFontSize, out maxWidths);

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
                if (_topAxisLabels)
                {
                    context.PushTransform(new TranslateTransform(GetPosition(labelIndx), LabelDistance));
                }
                else
                {
                    if (_autoAngleEnabled && orientation == LabelOrientation.Vertical)
                    {
                        context.PushTransform(new TranslateTransform(GetPosition(labelIndx), Height - text.Width));
                    }
                    else
                    {
                        context.PushTransform(new TranslateTransform(GetPosition(labelIndx), Height - LabelDistance - text.Height / 2));
                    }
                }

                // Rotate the label
                context.PushTransform(new RotateTransform(_textAngle));

                // Manual Angle
                bool labelPositionShift = false;
                double shift = 0;
                if(!_autoAngleEnabled)
                {
                    if (Math.Abs(_textAngle) > 45)
                    {
                        labelPositionShift = true;
                        if (_topAxisLabels)
                        {
                            shift = -text.Height / 2;
                        }
                        else
                        {
                            shift = -text.Height / 2;
                        }
                    }

                    if (_textAngle == 90)
                    {
                        orientation = LabelOrientation.Vertical;
                    }
                    if (_textAngle == 0)
                    {
                        orientation = LabelOrientation.Horizontal;
                    }
                    else if (_textAngle > 0)
                    {
                        if (_topAxisLabels)
                        {
                            orientation = LabelOrientation.DiagonalRight;
                        }
                        else
                        {
                            orientation = LabelOrientation.DiagonalLeft;
                        }
                    }
                    else if (_textAngle < 0)
                    {
                        if (_topAxisLabels)
                        {
                            orientation = LabelOrientation.DiagonalLeft;
                        }
                        else
                        {
                            orientation = LabelOrientation.DiagonalRight;
                        }
                    }
                }

                // Draw the text and set position using translated 
                // and rotated coordinate system. Every label orientation 
                // has different positioning of the labels.
                if (_topAxisLabels)
                {
                    if (orientation == LabelOrientation.Horizontal)
                    {
                        context.DrawText(text, new Point(-text.Width / 2, 0));
                    }
                    else if (orientation == LabelOrientation.Vertical)
                    {
                        context.DrawText(text, new Point(0, -text.Height / 2));
                    }
                    else if (orientation == LabelOrientation.DiagonalLeft)
                    {
                        if (labelPositionShift)
                        {
                            context.DrawText(text, new Point(-text.Width, shift));
                        }
                        else
                        {
                            context.DrawText(text, new Point(-text.Width, 0));
                        }
                    }
                    else if (orientation == LabelOrientation.DiagonalRight)
                    {
                        if (text.Width > 0)
                        {
                            if (labelPositionShift)
                            {
                                context.DrawText(text, new Point(0, shift));
                            }
                            else
                            {
                                context.DrawText(text, new Point(0, 0));
                            }
                        }
                    }
                }
                else
                {
                    if (orientation == LabelOrientation.Horizontal)
                    {
                        context.DrawText(text, new Point(-text.Width / 2, -text.Height / 2));
                    }
                    else if (orientation == LabelOrientation.Vertical)
                    {
                        context.DrawText(text, new Point(-text.Width + text.Height / 2, -text.Height / 2));
                    }
                    else if (orientation == LabelOrientation.DiagonalLeft)
                    {
                        if (labelPositionShift)
                        {
                            context.DrawText(text, new Point(-text.Width, shift));
                        }
                        else
                        {
                            context.DrawText(text, new Point(-text.Width, -text.Height / 2));
                        }
                    }
                    else if (orientation == LabelOrientation.DiagonalRight)
                    {
                        if (text.Width > 0)
                        {
                            if (labelPositionShift)
                            {
                                context.DrawText(text, new Point(-text.Height / 2, shift));
                            }
                            else
                            {
                                context.DrawText(text, new Point(0, -text.Height / 2));
                            }
                        }
                    }
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
        /// <returns>Auto label orientation which is the best match for specified label text and font size</returns>
        private LabelOrientation AutoLabels(out double autoAngle, out double autoFontSize, out double maxWidth)
        {
            maxWidth = 0;
            double oldFontSize = _minimumFontSize;
            _minimumFontSize = _fontSize - 1;

            // Check if labels can be horizontal without changing the font size
            if (HorizontalPosition(out autoAngle, out autoFontSize))
            {
                _minimumFontSize = oldFontSize;
                CalculateHorizontalHeight(ref autoFontSize);
                return LabelOrientation.Horizontal;
            }
            // Check if labels can be vertical without changing the font size
            else if (VerticalPosition(out autoAngle, out autoFontSize) && _autoAngleEnabled)
            {
                _minimumFontSize = oldFontSize;
                LabelsOverlaping(autoAngle, ref autoFontSize);
                return LabelOrientation.Vertical;
            }

            if (!_autoFontSizeEnabled)
            {
                return LabelOrientation.Vertical;
            }

            _minimumFontSize = oldFontSize;

            // Check if labels can be horizontaly oriented with font size which could be changed.
            if (HorizontalPosition(out autoAngle, out autoFontSize))
            {
                return LabelOrientation.Horizontal;
            }
            // Check if labels can be verticaly oriented with font size which could be changed.
            else if (VerticalPosition(out autoAngle, out autoFontSize))
            {
                LabelsOverlaping(autoAngle, ref autoFontSize);
                return LabelOrientation.Vertical;
            }
            else
            {
                // Check if labels can be diagonal.
                DiagonalPosition(out autoAngle, out autoFontSize, out maxWidth);

                // Left diagonal orientation
                if (this.IsLeftOrientation())
                {
                    if (_topAxisLabels)
                    {
                        autoAngle = -45;
                    }
                    else
                    {
                        autoAngle = 45;
                    }
                    return LabelOrientation.DiagonalLeft;
                }
                // Right diagonal orientation
                else
                {
                    if (_topAxisLabels)
                    {
                        autoAngle = 45;
                    }
                    else
                    {
                        autoAngle = -45;
                    }
                    return LabelOrientation.DiagonalRight;
                }
            }
        }

        /// <summary>
        /// This method calculates the best font size for horizontal labels 
        /// and checks if horizontal labels are possible without overlaping 
        /// and with minimum font size.
        /// </summary>
        /// <param name="autoAngle">Auto angle</param>
        /// <param name="autoFontSize">Auto font size</param>
        /// <returns>True if labels can be horizontal without overlaping</returns>
        private bool HorizontalPosition(out double autoAngle, out double autoFontSize)
        {
            bool fontOverlap = false;
            autoAngle = 0;
            double oldFontSize = _fontSize;
            double currentFontSize;

            // Font Size loop 
            for (currentFontSize = _fontSize; currentFontSize > _minimumFontSize; currentFontSize--)
            {
                SetFontSize(currentFontSize);

                // Labels loop
                bool overlap = false;
                for (int labelIndx = 0; labelIndx < _labels.Count - 1; labelIndx++)
                {
                    double firstLabelPosition = this.GetPosition(labelIndx);
                    double firstLabelWidth = _formattedLabels[labelIndx].Width;
                    double secondLabelPosition = this.GetPosition(labelIndx + 1);
                    double secondLabelWidth = _formattedLabels[labelIndx + 1].Width;

                    double distance = Math.Abs(firstLabelPosition - secondLabelPosition) * 0.8;

                    // Check a distance between a label and the next one
                    if (distance < firstLabelWidth / 2 + secondLabelWidth / 2)
                    {
                        overlap = true;
                        break;
                    }
                }

                // Check a distance between the first label and the rectangle border
                if (GetPosition(0) - _formattedLabels[0].Width / 2 < 0)
                {
                    overlap = true;
                }

                // Check a distance between the last label and the rectangle border
                if (GetPosition(_labels.Count - 1) + _formattedLabels[_labels.Count - 1].Width / 2 > Width)
                {
                    overlap = true;
                }

                // Labels overlap - break the loop
                if (overlap)
                {
                    fontOverlap = true;
                }
            }

            // Set old font size
            SetFontSize(oldFontSize);

            autoFontSize = currentFontSize;

            return !fontOverlap;
        }


        /// <summary>
        /// This method calculates the best font size for vertical labels 
        /// and checks if vertical labels are possible without overlaping 
        /// and with minimum font size.
        /// </summary>
        /// <param name="autoAngle">Auto angle</param>
        /// <param name="autoFontSize">Auto font size</param>
        /// <returns>True if labels can be vertical without overlaping</returns>
        private bool VerticalPosition(out double autoAngle, out double autoFontSize)
        {
            bool overlap = false;
            if (_topAxisLabels)
            {
                autoAngle = 90;
            }
            else
            {
                autoAngle = -90;
            }
            double oldFontSize = _fontSize;
            double currentFontSize;

            // Find the longest label
            int longestLabelIndx = FindLongestLabel();

            if (longestLabelIndx == -1)
            {
                autoFontSize = _fontSize;
                return false;
            }

            FormattedText longestLabel = _formattedLabels[longestLabelIndx];

            // Font Size loop
            for (currentFontSize = _fontSize; currentFontSize > _minimumFontSize; currentFontSize--)
            {
                longestLabel.SetFontSize(currentFontSize);

                double labelWidth = longestLabel.Width;

                // Check a distance between a label and the rectangle border
                if (labelWidth <= base.Height - LabelDistance)
                {
                    overlap = false;
                    break;
                }
                else
                {
                    overlap = true;
                }
            }

            // Set old font size
            longestLabel.SetFontSize(oldFontSize);

            autoFontSize = currentFontSize;
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
        /// This method calculates the best font size for diagonal labels 
        /// and checks if diagonal labels are possible without overlaping 
        /// and with minimum font size.
        /// </summary>
        /// <param name="autoAngle">Auto angle</param>
        /// <param name="autoFontSize">Auto font size</param>
        /// <param name="maxWidth">Maximum text size</param>
        /// <returns>True if labels can be diagonal without overlaping</returns>
        private bool DiagonalPosition(out double autoAngle, out double autoFontSize, out double maxWidth)
        {
            autoAngle = 45;
            bool overlap = false;
            double oldFontSize = _fontSize;
            double currentFontSize;
            maxWidth = 0;

            double distance = Math.Sqrt(2) * (base.Height - LabelDistance);

            // Find the longest label
            int longestLabelIndx = FindLongestLabel();

            if (longestLabelIndx == -1)
            {
                autoFontSize = _fontSize;
                return false;
            }

            FormattedText longestLabel = _formattedLabels[longestLabelIndx];

            // Font Size loop 
            for (currentFontSize = _fontSize; currentFontSize > _minimumFontSize; currentFontSize--)
            {
                longestLabel.SetFontSize(currentFontSize);

                // Check a distance between a label and the rectangle border
                if (longestLabel.Width + longestLabel.Height < distance)
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
            maxWidth = distance - longestLabel.Height;

            if (maxWidth < 0 || double.IsNaN(maxWidth))
            {
                maxWidth = 0;
            }

            // Set old font size
            longestLabel.SetFontSize(oldFontSize);

            autoFontSize = currentFontSize;
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
        /// Find if the labels orientation is left or right. It depends on 
        /// the space between the first label, the last label and 
        /// the border rectangle.
        /// </summary>
        /// <returns>True if the labels are left oriented</returns>
        private bool IsLeftOrientation()
        {
            if (_labels.Count > 0)
            {
                if (this.GetPosition(0) > Width - this.GetPosition(_labels.Count - 1))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This method calculates the best vertical size for horizontal 
        /// labels and prevents labels overlaping with bounding rectangle.
        /// </summary>
        /// <param name="autoFontSize"></param>
        private void CalculateHorizontalHeight(ref double autoFontSize)
        {
            double oldFontsize = autoFontSize;

            double currentFontSize;

            // Font size loop
            for (currentFontSize = autoFontSize; currentFontSize > _minimumFontSize; currentFontSize--)
            {
                if (_formattedLabels[0].Height > base.Height - LabelDistance)
                {
                    _formattedLabels[0].SetFontSize(currentFontSize);
                }
                else
                {
                    break;
                }
            }
            autoFontSize = currentFontSize;

            // Set old font size
            _formattedLabels[0].SetFontSize(oldFontsize);

        }

        /// <summary>
        /// Checks if vertical labels overlap and calculates the best font size 
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

        #endregion
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