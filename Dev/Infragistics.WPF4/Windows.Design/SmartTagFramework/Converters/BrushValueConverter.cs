using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Infragistics.Windows.Design.SmartTagFramework;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Represents the converter that converts Brush values to and from String values.
    /// </summary>    
    public class BrushValueConverter : IValueConverter
    {
        #region Member Variables

        private const string _nullString = "{x:Null}";

        #endregion //Member Variables

        #region IValueConverter Members

        /// <summary>
        /// Converts a Brush value to a System.String value.
        /// </summary>
        /// <param name="value">The Brush value to convert.</param>
        /// <param name="targetType">This parameter is not used.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns>
        /// System.Windows.Media.Colors predefined color's name if exists or the hexadecimal value of the color. 
        /// If the method returns null, the string "{x:Null}" is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString().Equals("System.Windows.Media.LinearGradientBrush"))
            {
                return _nullString;
            }

            foreach (NamedColor namedColor in EditorDataProvider.DefaultColorNames)
            {
                if (namedColor.Brush.ToString().Equals(value.ToString()))
                {
                    value = namedColor.Name;
                    break;
                }
            }

            return value;
        }

        /// <summary>
        /// Converts a System.String value to a Brush value.
        /// </summary>
        /// <param name="value">System.String value.</param>
        /// <param name="targetType">This parameter is not used.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns>
        /// A Brush value if the string can be converted; otherwise null.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                string stringValue = value.ToString();
                if (string.IsNullOrEmpty(stringValue) || stringValue == _nullString)
                {
                    return null;
                }
                else
                {
                    Brush brush = null;
                    try
                    {
                        brush = Utils.StringToBrush(value.ToString().Trim());
                    }
                    catch
                    {
                        
                    }
                    return brush;
                }
            }
        }

        #endregion //IValueConverter Members
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