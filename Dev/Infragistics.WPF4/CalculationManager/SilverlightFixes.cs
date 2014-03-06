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



using ExcelResources = SR;



namespace Infragistics.Calculations.Engine





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



#region Infragistics Source Cleanup (Region)





















































#endregion // Infragistics Source Cleanup (Region)


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



#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)


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

        /// <summary>
        /// Get the values from enumeration
        /// </summary>
        /// <typeparam name="T">The type of the enumeration</typeparam>
        /// <param name="sameAsTypeParameter">The same as type parameter.</param>
        /// <returns>Array of all the values in enumaration</returns>
        public static T[] EnumGetValueTypes<T>(T sameAsTypeParameter)
        {
            Type enumType = sameAsTypeParameter.GetType();
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_NotEnum", enumType.Name));
            }

            List<T> values = new List<T>();

            var fields = from field in enumType.GetFields()
                         where field.IsLiteral
                         select field;

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(enumType);
                values.Add((T)value);
            }

            return values.ToArray();
        }

        /// <summary>
        /// Get the values from enumeration
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns>array of values in the enumeration</returns>
        public static object[] EnumGetValues(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_NotEnum", enumType.Name));
            }

            List<object> values = new List<object>();

            var fields = from field in enumType.GetFields()
                         where field.IsLiteral
                         select field;

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(enumType);
                values.Add(value);
            }

            return values.ToArray();
        }

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

        internal static DateTime DateAndTimeDateAdd(DateInterval interval, double number, DateTime dateValue)
        {       
            int years = (int)Math.Round(ConversionFix(number));
            switch (interval)
            {
                case DateInterval.Year:
                    return Thread.CurrentThread.CurrentCulture.Calendar.AddYears(dateValue, years);

                case DateInterval.Quarter:
                    return dateValue.AddMonths(years * 3);

                case DateInterval.Month:
                    return Thread.CurrentThread.CurrentCulture.Calendar.AddMonths(dateValue, years);

                case DateInterval.DayOfYear:
                case DateInterval.Day:
                case DateInterval.Weekday:
                    return dateValue.AddDays(years);

                case DateInterval.WeekOfYear:
                    return dateValue.AddDays(years * 7.0);

                case DateInterval.Hour:
                    return dateValue.AddHours(years);

                case DateInterval.Minute:
                    return dateValue.AddMinutes(years);

                case DateInterval.Second:
                    return dateValue.AddSeconds(years);
            }

            throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_Interval"), "interval");
        }

        internal static DateInterval DateIntervalFromString(string interval)
        {
            if (interval != null)
            {
                interval = interval.ToUpper(CultureInfo.InvariantCulture);
            }

            switch (interval)
            {
                case "YYYY":
                    return DateInterval.Year;

                case "Y":
                    return DateInterval.DayOfYear;

                case "M":
                    return DateInterval.Month;

                case "D":
                    return DateInterval.Day;

                case "H":
                    return DateInterval.Hour;

                case "N":
                    return DateInterval.Minute;

                case "S":
                    return DateInterval.Second;

                case "WW":
                    return DateInterval.WeekOfYear;

                case "W":
                    return DateInterval.Weekday;

                default:
                    if (interval != "Q")
                    {
                        throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_IntervalStr"), "interval");
                    }
                    break;
            }

            return DateInterval.Quarter;
        }        

        /// <summary>
        /// Dates the and time date diff.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <param name="date1">The date1.</param>
        /// <param name="date2">The date2.</param>
        /// <param name="firstDayOfWeek">The first day of week.</param>
        /// <param name="firstWeekOfYear">The first week of year.</param>
        /// <returns>The difference between dates in required interval</returns>
        internal static long DateAndTimeDateDiff(DateInterval interval, DateTime date1, DateTime date2, FirstDayOfWeek firstDayOfWeek, FirstWeekOfYear firstWeekOfYear)
        {
            Calendar currentCalendar;
            TimeSpan span = date2.Subtract(date1);
            switch (interval)
            {
                case DateInterval.Year:
                    currentCalendar = Thread.CurrentThread.CurrentCulture.Calendar;
                    return (long)(currentCalendar.GetYear(date2) - currentCalendar.GetYear(date1));

                case DateInterval.Quarter:
                    currentCalendar = Thread.CurrentThread.CurrentCulture.Calendar;
                    return (long)((((currentCalendar.GetYear(date2) - currentCalendar.GetYear(date1)) * 4) + ((currentCalendar.GetMonth(date2) - 1) / 3)) - ((currentCalendar.GetMonth(date1) - 1) / 3));

                case DateInterval.Month:
                    currentCalendar = Thread.CurrentThread.CurrentCulture.Calendar;
                    return (long)((((currentCalendar.GetYear(date2) - currentCalendar.GetYear(date1)) * 12) + currentCalendar.GetMonth(date2)) - currentCalendar.GetMonth(date1));

                case DateInterval.DayOfYear:
                case DateInterval.Day:
                    return (long)Math.Round(ConversionFix(span.TotalDays));

                case DateInterval.WeekOfYear:
                    date1 = date1.AddDays((double)(0 - GetDayOfWeek(date1, firstDayOfWeek)));
                    date2 = date2.AddDays((double)(0 - GetDayOfWeek(date2, firstDayOfWeek)));
                    return ((long)Math.Round(ConversionFix(date2.Subtract(date1).TotalDays))) / 7L;

                case DateInterval.Weekday:
                    return ((long)Math.Round(ConversionFix(span.TotalDays))) / 7L;

                case DateInterval.Hour:
                    return (long)Math.Round(ConversionFix(span.TotalHours));

                case DateInterval.Minute:
                    return (long)Math.Round(ConversionFix(span.TotalMinutes));

                case DateInterval.Second:
                    return (long)Math.Round(ConversionFix(span.TotalSeconds));
            }

            throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_Interval"), "interval");
        }

        private static double ConversionFix(double number)
        {
            if (number >= 0.0)
            {
                return Math.Floor(number);
            }

            return -Math.Floor(-number);
        }

        /// <summary>
        /// Gets the day of week.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="weekdayFirst">The weekday first.</param>
        /// <returns>The number of day in a week</returns>
        private static int GetDayOfWeek(DateTime dateTime, FirstDayOfWeek weekdayFirst)
        {
            if ((weekdayFirst < FirstDayOfWeek.System) || (weekdayFirst > FirstDayOfWeek.Saturday))
            {
                throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_WeekdayFirst"), "weekdayFirst");
            }

            if (weekdayFirst == FirstDayOfWeek.System)
            {
                weekdayFirst = (FirstDayOfWeek)(Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek + 1);
            }

            return ((int)(((dateTime.DayOfWeek - ((DayOfWeek)((int)weekdayFirst))) + 8) % (int)(DayOfWeek.Saturday | DayOfWeek.Monday))) + 1;
        }

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