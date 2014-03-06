using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics
{
    /// <summary>
    /// Utility class for platform-specific operations.
    /// </summary>
    public static class Platform
    {
        /// <summary>
        /// Calculates the bounds of a given text string.
        /// </summary>
        /// <param name="text">The text string.</param>
        /// <param name="angle">The angle at which the text is rotated.</param>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="fontStretch">Font stretch.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="fontWeight">Font weight.</param>
        /// <returns>The size of the measured string.</returns>
        public static Size GetStringBounds(string text, double angle, FontFamily fontFamily, double fontSize, FontStretch fontStretch, FontStyle fontStyle, FontWeight fontWeight)
        {
            Size textSize = GetStringSizePixels(text, fontFamily, fontSize, fontStretch, fontStyle, fontWeight);
            double angleInRadians = MathUtil.Radians(angle);
            double width = System.Math.Abs(textSize.Width * System.Math.Cos(angleInRadians) + textSize.Height * System.Math.Sin(angleInRadians));
            double height = System.Math.Abs(textSize.Width * System.Math.Sin(angleInRadians) + textSize.Height * System.Math.Cos(angleInRadians));
            return new Size(width, height);
        }
    
        /// <summary>
        /// Calculates the size of the given text string.
        /// </summary>
        /// <param name="text">The text string.</param>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="fontStretch">Font stretch.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="fontWeight">Font weight.</param>
        /// <returns>The size of the measured string.</returns>
        public static Size GetStringSizePixels(string text, FontFamily fontFamily, double fontSize, FontStretch fontStretch, FontStyle fontStyle, FontWeight fontWeight)
        {
            TextBlock t = new TextBlock();
            t.Text = text;
            t.FontFamily = fontFamily;
            t.FontSize = fontSize;
            t.FontStretch = fontStretch;
            t.FontStyle = fontStyle;
            t.FontWeight = fontWeight;




            return new Size(t.ActualWidth, t.ActualHeight);

        }

        /// <summary>
        /// Returns the number of characters from a given string that will fit within a specified width.
        /// </summary>
        /// <param name="text">The text string.</param>
        /// <param name="width">The available width.</param>
        /// <param name="fontFamily">Font family.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="fontStretch">Font stretch.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="fontWeight">Font weight.</param>
        /// <returns>The number of characters from a given string that will fit within a specified width.</returns>
        public static int GetBestFitStringSize(string text, double width, FontFamily fontFamily, double fontSize, FontStretch fontStretch, FontStyle fontStyle, FontWeight fontWeight)
        {
            int charCount = 0;
            double cumulativeWidth = 0.0;
            char[] chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                char currentCharacter = chars[i];
                cumulativeWidth += Platform.GetStringSizePixels(currentCharacter.ToString(), fontFamily, fontSize, fontStretch, fontStyle, fontWeight).Width;
                if (cumulativeWidth > width)
                {
                    return charCount;
                }
                charCount++;
            }
            return charCount;
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