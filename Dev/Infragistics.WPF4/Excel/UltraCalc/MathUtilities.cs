using System;
using System.Diagnostics;
using System.Collections;
using System.Globalization;






using UltraCalcValue = Infragistics.Documents.Excel.CalcEngine.ExcelCalcValue;

namespace Infragistics.Documents.Excel





{
	internal class MathUtilities
	{
		// MD 4/9/12 - TFS101506
		#region DecimalTryParse

		public static bool DecimalTryParse(string text, IFormatProvider provider, out decimal value)
		{
			if (UltraCalcValue.UseExcelValueCompatibility)
			{
				if (Decimal.TryParse(text, NumberStyles.Any & ~NumberStyles.AllowTrailingSign, provider, out value) == false)
					return false;

				return MathUtilities.EnforceExcelGroupingRules(text, provider, ref value);
			}




		}

		#endregion // DecimalTryParse

		// MD 4/9/12 - TFS101506
		#region DoubleTryParse

		public static bool DoubleTryParse(string text, IFormatProvider provider, out double value)
		{
			if (UltraCalcValue.UseExcelValueCompatibility)
			{
				if (Double.TryParse(text, NumberStyles.Any & ~NumberStyles.AllowTrailingSign, provider, out value) == false)
					return false;

				return MathUtilities.EnforceExcelGroupingRules(text, provider, ref value);
			}




		}

		#endregion // DoubleTryParse

		// MD 4/9/12 - TFS101506
		#region EnforceExcelGroupingRules

		private static bool EnforceExcelGroupingRules<T>(string text, IFormatProvider provider, ref T value)
		{
			text = text.Trim();

			CultureInfo cultureInfo = provider as CultureInfo;
			CompareInfo compareInfo;
			if (cultureInfo != null)
				compareInfo = cultureInfo.CompareInfo;
			else
				compareInfo = CultureInfo.CurrentCulture.CompareInfo;

			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)provider.GetFormat(typeof(NumberFormatInfo)) ?? CultureInfo.CurrentCulture.NumberFormat;

			string numericPortion = text;

			int decimalIndex = compareInfo.IndexOf(text, numberFormatInfo.NumberDecimalSeparator, CompareOptions.IgnoreCase);
			if (0 <= decimalIndex)
			{
				numericPortion = text.Substring(0, decimalIndex);
			}
			else
			{
				int exponentIndex = compareInfo.IndexOf(text, "E", CompareOptions.IgnoreCase);
				if (0 <= exponentIndex)
					numericPortion = text.Substring(0, exponentIndex);
			}

			for (int i = numericPortion.Length - 1; i >= 0; i--)
			{
				bool foundEndOfNumericPortion = false;

				if (compareInfo.Compare(numericPortion, i, numberFormatInfo.NumberGroupSeparator.Length,
					numberFormatInfo.NumberGroupSeparator, 0, numberFormatInfo.NumberGroupSeparator.Length,
					CompareOptions.IgnoreCase) == 0)
				{
					foundEndOfNumericPortion = true;
				}
				else
				{
					char currentChar = numericPortion[i];
					if (Char.IsDigit(currentChar))
					{
						foundEndOfNumericPortion = true;
					}
				}

				if (foundEndOfNumericPortion)
				{
					numericPortion = numericPortion.Substring(0, i + 1);
					break;
				}
			}

			string[] groups = numericPortion.Split(new string[] { numberFormatInfo.NumberGroupSeparator }, StringSplitOptions.None);
			int minGroupSize = numberFormatInfo.NumberGroupSizes[0];

			for (int i = 1; i < groups.Length; i++)
			{
				if (groups[i].Length < minGroupSize)
				{
					value = default(T);
					return false;
				}
			}

			return true;
		}

		#endregion // EnforceExcelGroupingRules

		#region MidpointRoundingAwayFromZero

		public static float MidpointRoundingAwayFromZero(float value)
		{
			return (float)MathUtilities.MidpointRoundingAwayFromZero(value, 0);
		}

		public static double MidpointRoundingAwayFromZero(double value)
		{
			return MathUtilities.MidpointRoundingAwayFromZero(value, 0);
		}



#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)


		// MD 6/7/11 - TFS78166
		public static double MidpointRoundingAwayFromZero(double value, int digits)
		{
			// MD 8/2/11
			// Found while fixing TFS81451
			// When Math.Round is called with more than 15 digits, it causes an exception, so do the rounding manually.
			if (digits > 15)
			{
				double factor = Math.Pow(10, digits);
				int sign = Math.Sign(value);
				return MathUtilities.Truncate(value * factor + 0.5 * sign) / factor;
			}



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			return Math.Round(value, digits, MidpointRounding.AwayFromZero);

		}

		#endregion // MidpointRoundingAwayFromZero

		// MD 6/7/11 - TFS78166
		#region RoundToExcelDisplayValue

		public static double RoundToExcelDisplayValue(double value)
		{
			int roundingDigits = 14 - (int)Math.Floor(Math.Log10(Math.Abs(value)));

			if (roundingDigits > 0)
				return MathUtilities.MidpointRoundingAwayFromZero(value, roundingDigits);

			return value;
		}

		#endregion // RoundToExcelDisplayValue

		#region Truncate

		public static double Truncate(double value)
		{



			return Math.Truncate(value);

		}

		#endregion // Truncate
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