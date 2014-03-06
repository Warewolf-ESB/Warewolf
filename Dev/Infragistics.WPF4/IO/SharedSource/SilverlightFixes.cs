using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;






using System.Linq;









namespace Infragistics.Documents.Shared



{
    /// <summary>
    /// Class for utility functions related with unsupported methods/functions in Silverlight
    /// </summary>



	internal static class SilverlightFixes



    {
        #region Public fields
        /// <summary>
        /// Replace Color.Empty
        /// </summary>
        public static readonly Color ColorEmpty = new Color();
        #endregion Public fields

        #region Public properties
        /// <summary>
        /// Gets default encoding. Replace of the Encoding.Default.
        /// </summary>
        /// <returns>The default encoding</returns>
        public static Encoding EncodingDefault
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        /// <summary>
        /// Gets regex options for compiled. Replace RegexOptions.Compiled.
        /// </summary>
        /// <returns>dummy Regiex Options</returns>
        public static RegexOptions RegexOptionsCompiled
        {
            get
            {
                return RegexOptions.None;
            }
        }
        #endregion Public properties

        #region Public static methods



        /// <summary>
        /// Gets the code page. Replaces Encoding.CodePage;
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The code page of the encoding</returns>
        public static int GetCodePage(Encoding encoding)
        {

			return encoding.CodePage;


#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

        }

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <param name="codePage">The code page.</param>
        /// <returns>Generates the encoding by code page</returns>
        public static Encoding GetEncoding(int codePage)
        {

			return Encoding.GetEncoding(codePage);


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        }



        /// <summary>
        /// Gets culture info by code page. Replace CultureInfo.GetCultureInfo( (int)(uint)value )
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Return culture info by code page</returns>
        public static CultureInfo GetCultureInfo(int culture)
        {
            return CultureInfo.CurrentCulture;
        }
        
        /// <summary>
        /// Replace Math.Round( x, MidpointRounding.AwayFromZero )
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>rounded away zero value </returns>
        public static int MidpointRoundingAwayFromZero(float value)
        {
            return (int)Math.Round(value);
        }

        /// <summary>
        /// Replace Math.Round( x, MidpointRounding.AwayFromZero )
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>rounded away zero value</returns>
        public static int MidpointRoundingAwayFromZero(double value)
        {
            return (int)Math.Round(value);
        }

		// MD 6/7/11 - TFS78166
		/// <summary>
		/// Replace Math.Round( x, d, MidpointRounding.AwayFromZero )
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="digits">The number of fractional digits to round to.</param>
		/// <returns>rounded away zero value</returns>
		public static double MidpointRoundingAwayFromZero(double value, int digits)
		{
			return Math.Round(value, digits);
		}

        /// <summary>
        /// Replace Math.Truncate(x)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>truncated value</returns>
        public static double MathTruncate(double value)
        {
			// MD 6/22/12 - TFS115376
			// This may cause rollover when the value can't fit in an int. There is a better way to truncate the value without 
			// having this problem.
            //return (int)value;
			if (value < 0)
				return Math.Ceiling(value);

			return Math.Floor(value);
        }



#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Replace CultureInfo.CurrentCulture.LCID
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>Throw not implemented exception</returns>
        public static int GetCultureInfoLCID(CultureInfo cultureInfo)
        {
            throw new NotImplementedException(); 
        }


        /// <summary>
        /// Replace Array.ConvertAll
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="fn">The fn.</param>
        /// <returns></returns>
        public static TOut[] ArrayConvertAll<TIn, TOut>(TIn[] input, Func<TIn, TOut> fn)
        {
            TOut[] result = new TOut[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = fn(input[i]);
            }
            return result;
        }

        /// <summary>
        /// Replace Array.ConvertAll
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        internal static List<TOut> ArrayConvertAll<TIn, TOut>(List<TIn> input, Func<TIn, TOut> func)
        {
            List<TOut> result = new List<TOut>();
            for (int i = 0; i < input.Count; i++)
            {
                result[i] = func(input[i]);
            }
            return result;
        }


		/// <summary>
        /// Replace String.ToUpperInvariant().
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static string StringToUpperInvariant(string s)
        {
            return s.ToUpper();
        }

        /// <summary>
        /// Determines whether the specified collection is synchronized.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the collection</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>
        ///     <c>true</c> if the specified collection is synchronized; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSynchronized<T>(ICollection<T> collection)
        {
            return true;
        }

        /// <summary>
        /// Determines whether the specified synchronized is synchronized.
        /// </summary>
        /// <param name="synchronized">The synchronized.</param>
        /// <returns>
        ///     <c>true</c> if the specified synchronized is synchronized; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSynchronized(object synchronized)
        {
            return true;
        }

        /// <summary>
        /// Return an object for sync
        /// </summary>
        /// <typeparam name="T">Type of the elements in the collection</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>object for synchronization</returns>
        public static object SyncRoot<T>(ICollection<T> collection)
        {
            return collection;
        }


#region Infragistics Source Cleanup (Region)




















































#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Replace Color.IsEmpty
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>true if the color is set to ColorEmpty</returns>
        public static bool ColorIsEmpty(Color color)
        {
            return color == ColorEmpty;
        }

        /// <summary>
        /// Generate color from RGB value. Replace Color.FromArgb
        /// </summary>
        /// <param name="rgb">The RGB val.</param>
        /// <returns>Generated color</returns>
        public static Color ColorFromArgb(int rgb)
        {
            long value = rgb & ((long)0xffffffffL);
            return Color.FromArgb(
                (byte)((value >> 0x18) & 0xffL),
                (byte)((value >> 0x10) & 0xffL),
                (byte)((value >> 8) & 0xffL),
                (byte)(value & 0xffL));
        }
        #endregion Public static methods

        #region Internal methods
        /// <summary>
        /// Generate RGB representation for Color. Replace Color.ToArgb
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>RGB representation of the Color</returns>
        internal static int ColorToArgb(Color color)
        {
            return (int)((((color.A << 0x18) | (color.R << 0x10)) | (color.G << 8)) | color.B);
        }        

        /// <summary>
        /// Offset the rectangle position. Replace Rectangle.Offset
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        internal static void RectOffset(ref Rect rect, double x, double y)
        {
            rect.X += x;
            rect.Y += y;
        }

        #region Visual Basic methods


#region Infragistics Source Cleanup (Region)

































































































































































#endregion // Infragistics Source Cleanup (Region)

        #endregion Visual Basic methods
        #endregion Internal methods
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