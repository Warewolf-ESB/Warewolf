using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.Excel2007;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// A filter condition used in a <see cref="CustomFilter"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The CustomFilterCondition contains a comparison operator and a value. The value of each cell in the data range is compared against 
	/// the condition value using the comparison operator.
	/// </p>
	/// </remarks>
	/// <seealso cref="CustomFilter.Condition1"/>
	/// <seealso cref="CustomFilter.Condition2"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class CustomFilterCondition
	{
		#region Constants

		internal const string WildcardNChars = "*";
		internal const string WildcardSingleChar = "?";

		#endregion // Constants

		#region Member Variables

		private readonly ExcelComparisonOperator _comparisonOperator;
		private readonly bool _hasWildcards;
		private readonly Regex _searchRegex;
		private readonly string _stringValue;
		private readonly object _value;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="CustomFilterCondition"/> instance.
		/// </summary>
		/// <param name="comparisonOperator">
		/// The operator which describes how the cell values should be compared against <paramref name="value"/>.
		/// </param>
		/// <param name="value">
		/// The number against which the cell values should be compared.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// For numbers, the <paramref name="comparisonOperator"/> cannot be BeginsWith, DoesNotBeginWith, EndsWith, DoesNotEndWith, 
		/// Contains, or DoesNotContain.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="comparisonOperator"/> is not defined in the <see cref="ExcelComparisonOperator"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value"/> is infinity or NaN.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="comparisonOperator"/> is BeginsWith, DoesNotBeginWith, EndsWith, DoesNotEndWith, Contains, or DoesNotContain.
		/// </exception>
		public CustomFilterCondition(ExcelComparisonOperator comparisonOperator, double value)
			: this(comparisonOperator, (object)value)
		{
			if (Double.IsNaN(value) || Double.IsInfinity(value))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidCustomFilterOperandNumber"), "value");
		}

		/// <summary>
		/// Creates a new <see cref="CustomFilterCondition"/> instance.
		/// </summary>
		/// <param name="comparisonOperator">
		/// The operator which describes how the cell values should be compared against <paramref name="value"/>.
		/// </param>
		/// <param name="value">
		/// The date against which the cell values should be compared.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// For dates, the <paramref name="comparisonOperator"/> cannot be BeginsWith, DoesNotBeginWith, EndsWith, DoesNotEndWith, 
		/// Contains, or DoesNotContain.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="comparisonOperator"/> is not defined in the <see cref="ExcelComparisonOperator"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value"/> cannot be expression as a date in Excel.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="comparisonOperator"/> is BeginsWith, DoesNotBeginWith, EndsWith, DoesNotEndWith, Contains, or DoesNotContain.
		/// </exception>
		public CustomFilterCondition(ExcelComparisonOperator comparisonOperator, DateTime value)
			: this(comparisonOperator, (object)value)
		{
			if (ExcelCalcValue.DateTimeToExcelDate(null, value).HasValue == false)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidExcelDate"), "value");
		}

		/// <summary>
		/// Creates a new <see cref="CustomFilterCondition"/> instance.
		/// </summary>
		/// <param name="comparisonOperator">
		/// The operator which describes how the cell values should be compared against <paramref name="value"/>.
		/// </param>
		/// <param name="value">
		/// The time against which the cell values should be compared.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// For times, the <paramref name="comparisonOperator"/> cannot be BeginsWith, DoesNotBeginWith, EndsWith, DoesNotEndWith, 
		/// Contains, or DoesNotContain.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="comparisonOperator"/> is not defined in the <see cref="ExcelComparisonOperator"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="comparisonOperator"/> is BeginsWith, DoesNotBeginWith, EndsWith, DoesNotEndWith, Contains, or DoesNotContain.
		/// </exception>
		public CustomFilterCondition(ExcelComparisonOperator comparisonOperator, TimeSpan value)
			: this(comparisonOperator, (object)value) { }

		/// <summary>
		/// Creates a new <see cref="CustomFilterCondition"/> instance.
		/// </summary>
		/// <param name="comparisonOperator">
		/// The operator which describes how the cell values should be compared against <paramref name="value"/>.
		/// </param>
		/// <param name="value">
		/// The string against which the cell values should be compared. The string can contains wild cards for any character (?) or for 
		/// zero or more characters (*).
		/// </param>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> If the value is longer than 255 characters in length and the workbook is saved in one of the 2003 formats,
		/// the correct rows will be hidden in the saved file, but the filter will be missing from the column.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="comparisonOperator"/> is not defined in the <see cref="ExcelComparisonOperator"/> enumeration.
		/// </exception>
		public CustomFilterCondition(ExcelComparisonOperator comparisonOperator, string value)
			: this(comparisonOperator, (object)value) { }

		internal CustomFilterCondition(ExcelComparisonOperator comparisonOperator, object value)
		{
			Utilities.VerifyEnumValue(comparisonOperator);

			if (value == null)
				throw new ArgumentNullException("value");

			if ((value is string) == false &&
				CustomFilterCondition.MustComparisonOperatorBeUsedWithStrings(comparisonOperator))
			{
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidCustomFilterOperator"), "value");
			}

			_comparisonOperator = comparisonOperator;
			_value = value;
			_stringValue = WorksheetCellBlock.GetDefaultValueText(value);
			
			// MD 3/20/12 - TFS105483
			// Leading a trailing whitespace is not considered when comparing using custom filters.
			_stringValue = _stringValue.Trim();

			_searchRegex = Utilities.CreateWildcardRegex(this.GetResolvedSearchString(), out _hasWildcards);
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="CustomFilterCondition"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			CustomFilterCondition other = obj as CustomFilterCondition;
			if (other == null)
				return false;

			return
				_comparisonOperator == other._comparisonOperator &&
				Object.Equals(_value, other._value);
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="CustomFilterCondition"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			return _comparisonOperator.GetHashCode() ^ _value.GetHashCode();
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

		#region Methods

		#region CreateCustomFilterCondition

		internal static CustomFilterCondition CreateCustomFilterCondition(ST_FilterOperator operatorValue, object value)
		{
			string strValue = value as string;

			ExcelComparisonOperator comparisonOperator;
			object conditionValue;
			bool hasStartingWildCard = strValue != null && strValue.StartsWith(CustomFilterCondition.WildcardNChars);
			bool hasEndingWildCard = strValue != null && strValue.EndsWith(CustomFilterCondition.WildcardNChars);

			switch (operatorValue)
			{
				case ST_FilterOperator.equal:
					if (strValue == null || strValue.Length == 1 ||
						(hasStartingWildCard == false && hasEndingWildCard == false))
					{
						conditionValue = value;
						comparisonOperator = ExcelComparisonOperator.Equals;
					}
					else if (hasStartingWildCard)
					{
						if (hasEndingWildCard)
						{
							conditionValue = strValue.Substring(1, strValue.Length - 2);
							comparisonOperator = ExcelComparisonOperator.Contains;
						}
						else
						{
							conditionValue = strValue.Substring(1);
							comparisonOperator = ExcelComparisonOperator.EndsWith;
						}
					}
					else
					{
						conditionValue = strValue.Substring(0, strValue.Length - 1);
						comparisonOperator = ExcelComparisonOperator.BeginsWith;
					}
					break;

				case ST_FilterOperator.notEqual:
					if (strValue == null || strValue.Length == 1 ||
						(hasStartingWildCard == false && hasEndingWildCard == false))
					{
						conditionValue = value;
						comparisonOperator = ExcelComparisonOperator.NotEqual;
					}
					else if (hasStartingWildCard)
					{
						if (hasEndingWildCard)
						{
							conditionValue = strValue.Substring(1, strValue.Length - 2);
							comparisonOperator = ExcelComparisonOperator.DoesNotContain;
						}
						else
						{
							conditionValue = strValue.Substring(1);
							comparisonOperator = ExcelComparisonOperator.DoesNotEndWith;
						}
					}
					else
					{
						conditionValue = strValue.Substring(0, strValue.Length - 1);
						comparisonOperator = ExcelComparisonOperator.DoesNotBeginWith;
					}
					break;

				case ST_FilterOperator.greaterThan:
					conditionValue = value;
					comparisonOperator = ExcelComparisonOperator.GreaterThan;
					break;

				case ST_FilterOperator.greaterThanOrEqual:
					conditionValue = value;
					comparisonOperator = ExcelComparisonOperator.GreaterThanOrEqual;
					break;

				case ST_FilterOperator.lessThan:
					conditionValue = value;
					comparisonOperator = ExcelComparisonOperator.LessThan;
					break;

				case ST_FilterOperator.lessThanOrEqual:
					conditionValue = value;
					comparisonOperator = ExcelComparisonOperator.LessThanOrEqual;
					break;

				default:
					Utilities.DebugFail("Invalid ST_FilterOperator: " + operatorValue);
					return null;
			}

			return new CustomFilterCondition(comparisonOperator, conditionValue);
		}

		#endregion // CreateCustomFilterCondition

		#region GetResolvedSearchString

		internal string GetResolvedSearchString()
		{
			string result = _stringValue;

			switch (this.ComparisonOperator)
			{
				case ExcelComparisonOperator.Equals:
				case ExcelComparisonOperator.NotEqual:
				case ExcelComparisonOperator.GreaterThan:
				case ExcelComparisonOperator.GreaterThanOrEqual:
				case ExcelComparisonOperator.LessThan:
				case ExcelComparisonOperator.LessThanOrEqual:
					return result;

				case ExcelComparisonOperator.BeginsWith:
					if (result.EndsWith(CustomFilterCondition.WildcardNChars) == false)
						result += CustomFilterCondition.WildcardNChars;
					return result;

				case ExcelComparisonOperator.DoesNotBeginWith:
					if (result.EndsWith(CustomFilterCondition.WildcardNChars) == false)
						result += CustomFilterCondition.WildcardNChars;
					return result;

				case ExcelComparisonOperator.EndsWith:
					if (result.StartsWith(CustomFilterCondition.WildcardNChars) == false)
						result = CustomFilterCondition.WildcardNChars + result;
					return result;

				case ExcelComparisonOperator.DoesNotEndWith:
					if (result.StartsWith(CustomFilterCondition.WildcardNChars) == false)
						result = CustomFilterCondition.WildcardNChars + result;
					return result;

				case ExcelComparisonOperator.Contains:
					if (result.StartsWith(CustomFilterCondition.WildcardNChars) == false)
						result = CustomFilterCondition.WildcardNChars + result;
					if (result.EndsWith(CustomFilterCondition.WildcardNChars) == false)
						result += CustomFilterCondition.WildcardNChars;
					return result;

				case ExcelComparisonOperator.DoesNotContain:
					if (result.StartsWith(CustomFilterCondition.WildcardNChars) == false)
						result = CustomFilterCondition.WildcardNChars + result;
					if (result.EndsWith(CustomFilterCondition.WildcardNChars) == false)
						result += CustomFilterCondition.WildcardNChars;
					return result;

				default:
					Utilities.DebugFail("Unknown ComparisonOperator: " + this.ComparisonOperator);
					return result;
			}
		}

		#endregion // GetResolvedSearchString

		#region GetSaveValues

		internal void GetSaveValues(WorkbookSerializationManager manager, out ST_FilterOperator operatorValue, out object value)
		{
			value = this.Value;

			double numericValue;
			if (Utilities.TryGetNumericValue(manager.Workbook, value, out numericValue))
				value = numericValue;

			string resolvedSearchString = this.GetResolvedSearchString();

			switch (this.ComparisonOperator)
			{
				case ExcelComparisonOperator.Equals:
					operatorValue = ST_FilterOperator.equal;
					break;
				case ExcelComparisonOperator.NotEqual:
					operatorValue = ST_FilterOperator.notEqual;
					break;
				case ExcelComparisonOperator.GreaterThan:
					operatorValue = ST_FilterOperator.greaterThan;
					break;
				case ExcelComparisonOperator.GreaterThanOrEqual:
					operatorValue = ST_FilterOperator.greaterThanOrEqual;
					break;
				case ExcelComparisonOperator.LessThan:
					operatorValue = ST_FilterOperator.lessThan;
					break;
				case ExcelComparisonOperator.LessThanOrEqual:
					operatorValue = ST_FilterOperator.lessThanOrEqual;
					break;

				case ExcelComparisonOperator.BeginsWith:
				case ExcelComparisonOperator.EndsWith:
				case ExcelComparisonOperator.Contains:
					operatorValue = ST_FilterOperator.equal;
					value = resolvedSearchString;
					break;

				case ExcelComparisonOperator.DoesNotBeginWith:
				case ExcelComparisonOperator.DoesNotEndWith:
				case ExcelComparisonOperator.DoesNotContain:
					operatorValue = ST_FilterOperator.notEqual;
					value = resolvedSearchString;
					break;

				default:
					Utilities.DebugFail("Unknown ComparisonOperator: " + this.ComparisonOperator);
					operatorValue = ST_FilterOperator.equal;
					value = resolvedSearchString;
					break;
			}
		}

		#endregion // GetSaveValues

		#region MeetsCriteria

		internal bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex, object testValue)
		{
			bool testValueIsString = WorksheetCellBlock.IsStringValue(testValue);

			double? doubleValue = null;
			double numericValue;
			if (Utilities.TryGetNumericValue(worksheet.Workbook, this.Value, out numericValue))
				doubleValue = numericValue;

			switch (this.ComparisonOperator)
			{
				case ExcelComparisonOperator.Equals:
				case ExcelComparisonOperator.NotEqual:
				case ExcelComparisonOperator.GreaterThan:
				case ExcelComparisonOperator.GreaterThanOrEqual:
				case ExcelComparisonOperator.LessThan:
				case ExcelComparisonOperator.LessThanOrEqual:
					{
						double? doubleTestValue = null;
						if (doubleValue.HasValue)
						{
							if (testValueIsString)
							{
								// MD 4/9/12 - TFS101506
								//if (Double.TryParse(testValue.ToString(), NumberStyles.Float, CultureInfo.CurrentCulture, out numericValue))
								if (MathUtilities.DoubleTryParse(testValue.ToString(), worksheet.Culture, out numericValue))
									doubleTestValue = numericValue;
							}
							else
							{
								if (Utilities.TryGetNumericValue(worksheet.Workbook, testValue, out numericValue))
									doubleTestValue = numericValue;
							}
						}

						if (doubleTestValue.HasValue == false || doubleValue.HasValue == false)
						{
							if (testValueIsString == false || (this.Value is string) == false)
							{
								// When doing string comparisons, these comparisons must only be done with values that are actually strings.
								switch (this.ComparisonOperator)
								{
									case ExcelComparisonOperator.GreaterThan:
									case ExcelComparisonOperator.GreaterThanOrEqual:
									case ExcelComparisonOperator.LessThan:
									case ExcelComparisonOperator.LessThanOrEqual:
										return false;
								}
							}

							GetCellTextParameters parameters = new GetCellTextParameters(columnIndex);
							parameters.TextFormatMode = TextFormatMode.IgnoreCellWidth;
							parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.None;
							string stringTestValue = WorksheetCellBlock.GetCellTextInternal(worksheet, row, parameters, testValue);

							// MD 3/20/12 - TFS105483
							// Leading a trailing whitespace is not considered when comparing using custom filters.
							stringTestValue = stringTestValue.Trim();

							int result;
							if (_hasWildcards)
							{
								if (this.ComparisonOperator == ExcelComparisonOperator.Equals ||
									this.ComparisonOperator == ExcelComparisonOperator.NotEqual)
								{
									result = _searchRegex.Match(stringTestValue).Success ? 0 : 1;
								}
								else
								{
									// With the less than/greater than (or equals) type comparison and wildcards are used, everything is shown apparently.
									return true;
								}
							}
							else
							{
								result = String.Compare(stringTestValue, _stringValue, StringComparison.InvariantCultureIgnoreCase);
							}

							switch (this.ComparisonOperator)
							{
								case ExcelComparisonOperator.Equals:
									return result == 0;

								case ExcelComparisonOperator.NotEqual:
									return result != 0;

								case ExcelComparisonOperator.GreaterThan:
									return result > 0;

								case ExcelComparisonOperator.GreaterThanOrEqual:
									return result >= 0;

								case ExcelComparisonOperator.LessThan:
									return result < 0;

								case ExcelComparisonOperator.LessThanOrEqual:
									return result <= 0;
							}
						}
						else
						{
							if (testValueIsString || (this.Value is string))
							{
								// When doing number comparisons, these comparisons must only be done with values that are actually not strings.
								switch (this.ComparisonOperator)
								{
									case ExcelComparisonOperator.GreaterThan:
									case ExcelComparisonOperator.GreaterThanOrEqual:
									case ExcelComparisonOperator.LessThan:
									case ExcelComparisonOperator.LessThanOrEqual:
										return false;
								}
							}

							double displayTestValue = MathUtilities.RoundToExcelDisplayValue(doubleTestValue.Value);

							// MD 3/23/12 - TFS105487
							// We should also be using the rounded reference value when comparing below.
							double displayValue = MathUtilities.RoundToExcelDisplayValue(doubleValue.Value);

							switch (this.ComparisonOperator)
							{
								case ExcelComparisonOperator.Equals:
									return displayTestValue == displayValue;

								case ExcelComparisonOperator.NotEqual:
									return displayTestValue != displayValue;

								case ExcelComparisonOperator.GreaterThan:
									return displayTestValue > displayValue;

								case ExcelComparisonOperator.GreaterThanOrEqual:
									return displayTestValue >= displayValue;

								case ExcelComparisonOperator.LessThan:
									return displayTestValue < displayValue;

								case ExcelComparisonOperator.LessThanOrEqual:
									return displayTestValue <= displayValue;
							}
						}

						Utilities.DebugFail("This is unexpected.");
						return false;
					}

				case ExcelComparisonOperator.BeginsWith:
				case ExcelComparisonOperator.DoesNotBeginWith:
				case ExcelComparisonOperator.EndsWith:
				case ExcelComparisonOperator.DoesNotEndWith:
				case ExcelComparisonOperator.Contains:
				case ExcelComparisonOperator.DoesNotContain:
					{
						if (testValueIsString == false)
						{
							return
								this.ComparisonOperator == ExcelComparisonOperator.DoesNotBeginWith ||
								this.ComparisonOperator == ExcelComparisonOperator.DoesNotContain ||
								this.ComparisonOperator == ExcelComparisonOperator.DoesNotEndWith;
						}

						GetCellTextParameters parameters = new GetCellTextParameters(columnIndex);
						parameters.TextFormatMode = TextFormatMode.IgnoreCellWidth;
						parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.None;
						string stringTestValue = WorksheetCellBlock.GetCellTextInternal(worksheet, row, parameters, testValue);

						// MD 3/20/12 - TFS105483
						// Leading a trailing whitespace is not considered when comparing using custom filters.
						stringTestValue = stringTestValue.Trim();

						bool foundMatch = _searchRegex.Match(stringTestValue).Success;

						switch (this.ComparisonOperator)
						{
							case ExcelComparisonOperator.BeginsWith:
							case ExcelComparisonOperator.EndsWith:
							case ExcelComparisonOperator.Contains:
								return foundMatch;

							case ExcelComparisonOperator.DoesNotBeginWith:
							case ExcelComparisonOperator.DoesNotEndWith:
							case ExcelComparisonOperator.DoesNotContain:
								return foundMatch == false;
						}

						Utilities.DebugFail("This is unexpected.");
						return false;
					}

				default:
					Utilities.DebugFail("Unknown ComparisonOperator: " + this.ComparisonOperator);
					return false;
			}
		}

		#endregion // MeetsCriteria

		#region MustComparisonOperatorBeUsedWithStrings

		internal static bool MustComparisonOperatorBeUsedWithStrings(ExcelComparisonOperator comparisonOperator)
		{
			switch (comparisonOperator)
			{
				case ExcelComparisonOperator.BeginsWith:
				case ExcelComparisonOperator.DoesNotBeginWith:
				case ExcelComparisonOperator.Contains:
				case ExcelComparisonOperator.DoesNotContain:
				case ExcelComparisonOperator.EndsWith:
				case ExcelComparisonOperator.DoesNotEndWith:
					return true;
			}

			return false;
		}

		#endregion // MustComparisonOperatorBeUsedWithStrings

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region ComparisonOperator

		/// <summary>
		/// Gets the operator which describes how the cell values should be compared against <see cref="Value"/>.
		/// </summary>
		/// <seealso cref="Value"/>
		public ExcelComparisonOperator ComparisonOperator
		{
			get { return _comparisonOperator; }
		}

		#endregion // ComparisonOperator

		#region Value

		/// <summary>
		/// Gets the value against which the cell values should be compared.
		/// </summary>
		/// <seealso cref="ComparisonOperator"/>		
		public object Value
		{
			get { return _value; }
		}

		#endregion // Value

		#endregion // Public Properties

		#region Internal Properties

		#region HasWildcards

		internal bool HasWildcards
		{
			get { return _hasWildcards; }
		}

		#endregion // HasWildcards

		#endregion // Internal Properties

		#endregion // Properties
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