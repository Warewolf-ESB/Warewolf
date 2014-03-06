using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Represents the converter that converts System.String values to and from ImageSource values.
    /// </summary>      
    public class ImageSourceValueConverter : IValueConverter
    {
        #region Member Variables

        private const string _nullString = "{x:Null}";

        #endregion //Member Variables


        #region IValueConverter Members

        /// <summary>
        /// Converts an ImageSource to System.String
        /// </summary>
        /// <param name="value">The ImageSource value to convert.</param>
        /// <param name="targetType">This parameter is not used.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns>
        /// Resource relative path.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value									|| 
				value.ToString().Trim().Equals(string.Empty)	||
				value.ToString().Trim().Equals(_nullString))
            {
                return null;
            }
            else
            {
                string path = value.ToString().Trim();
                int index = path.IndexOf(@"?/");
                index = index > 0 ? index + 1 : 0;
                path = path.Substring(index);

                return path;
            }
        }

        /// <summary>
        /// Converts a resource relative path to a ImageSource.
        /// </summary>
        /// <param name="value">Resource relative path.</param>
        /// <param name="targetType">This parameter is not used.</param>
        /// <param name="parameter">This parameter can be null or the desired UriKind.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns>
        /// A BitmapImage value if the image resource can be found; otherwise null.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
                return null;

			if (value is string && (string)value == _nullString)
				return null;

			if (parameter != null && !(parameter is UriKind))
				return null;

			UriKind uriKind = parameter == null ? UriKind.RelativeOrAbsolute : (UriKind)parameter;
            Uri uri = new Uri(value.ToString().Trim(), uriKind);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = uri;
            bitmapImage.EndInit();

            return bitmapImage;
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