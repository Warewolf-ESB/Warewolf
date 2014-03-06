using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	#region AverageFilterType




	/// <summary>
	/// Represents the various types of the <see cref="AverageFilter"/>.
	/// </summary>
	/// <seealso cref="AverageFilter.Type"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum AverageFilterType
	{
		/// <summary>
		/// Filter in values above the average of the entire range of data being filtered.
		/// </summary>
		AboveAverage,

		/// <summary>
		/// Filter in values below the average of the entire range of data being filtered.
		/// </summary>
		BelowAverage,
	}

	#endregion // AverageFilterType

	#region CalendarType




	/// <summary>
	/// Represents the various calendar types available for the <see cref="FixedValuesFilter"/>
	/// </summary>
	/// <seealso cref="FixedValuesFilter.CalendarType"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum CalendarType
	{
		/// <summary>
		/// The Gregorian calendar should be used.
		/// </summary>
		Gregorian,

		/// <summary>
		/// The Arabic version of the Gregorian calendar should be used.
		/// </summary>
		GregorianArabic,

		/// <summary>
		/// The Middle East French version of the Gregorian calendar should be used.
		/// </summary>
		GregorianMeFrench,

		/// <summary>
		/// The US English version of the Gregorian calendar should be used.
		/// </summary>
		GregorianUs,

		/// <summary>
		/// The transliterated English version of the Gregorian calendar should be used.
		/// </summary>
		GregorianXlitEnglish,

		/// <summary>
		/// The transliterated French version of the Gregorian calendar should be used.
		/// </summary>
		GregorianXlitFrench,

		/// <summary>
		/// The Hebrew lunar calendar, as described by the Gauss formula for Passover and The Complete Restatement of Oral Law, 
		/// should be used.
		/// </summary>
		Hebrew,

		/// <summary>
		/// The Hijri lunar calendar, as described by the Kingdom of Saudi Arabia, Ministry of Islamic Affairs, Endowments, Da�wah 
		/// and Guidance, should be used.
		/// </summary>
		Hijri,

		/// <summary>
		/// The Japanese Emperor Era calendar, as described by Japanese Industrial Standard JIS X 0301, should be used.
		/// </summary>
		Japan,

		/// <summary>
		/// The Korean Tangun Era calendar, as described by Korean Law Enactment No. 4, should be used.
		/// </summary>
		Korea,

		/// <summary>
		/// Specifies that no calendar should be used.
		/// </summary>
		None,

		/// <summary>
		/// The Saka Era calendar, as described by the Calendar Reform Committee of India, as part of the Indian Ephemeris and Nautical 
		/// Almanac, should be used.
		/// </summary>
		Saka,

		/// <summary>
		/// The Taiwanese calendar, as defined by the Chinese National Standard CNS 7648, should be used.
		/// </summary>
		Taiwan,

		/// <summary>
		/// The Thai calendar, as defined by the Royal Decree of H.M. King Vajiravudh (Rama VI) in Royal Gazette B. E. 2456 (1913 A.D.) 
		/// and by the decree of Prime Minister Phibunsongkhram (1941 A.D.) to start the year on the Gregorian January 1 and to map year 
		/// zero to Gregorian year 543 B.C., should be used.
		/// </summary>
		Thai,
	}

	#endregion // CalendarType

	#region ConditionalOperator




	/// <summary>
	/// Represents the various logical operators used to combine the conditions of the <see cref="CustomFilter"/>.
	/// </summary>
	/// <seealso cref="CustomFilter.ConditionalOperator"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum ConditionalOperator
	{
		/// <summary>
		/// Filter in values where only <see cref="CustomFilter.Condition1"/> and <see cref="CustomFilter.Condition2"/> pass.
		/// </summary>
		And = 0,

		/// <summary>
		/// Filter in values where either <see cref="CustomFilter.Condition1"/> or <see cref="CustomFilter.Condition2"/> (or both) pass.
		/// </summary>
		Or = 1
	}

	#endregion // ConditionalOperator

	#region DatePeriodFilterType




	/// <summary>
	/// Represents the various date range types which can be filtered by the <see cref="DatePeriodFilter"/>.
	/// </summary>
	/// <seealso cref="DatePeriodFilter.Type"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum DatePeriodFilterType
	{
		/// <summary>
		/// Filter in dates in a specific month of any year.
		/// </summary>
		Month,

		/// <summary>
		/// Filter in dates in a specific quarter of any year.
		/// </summary>
		Quarter
	}

	#endregion // DatePeriodFilterType

	#region ExcelComparisonOperator




	/// <summary>
	/// Represents the various comparisons which can be used in the <see cref="CustomFilterCondition"/>.
	/// </summary>
	/// <seealso cref="CustomFilterCondition.ComparisonOperator"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum ExcelComparisonOperator
	{
		/// <summary>
		/// Filter in values which are equal to the comparison value.
		/// </summary>
		Equals,

		/// <summary>
		/// Filter in values which are not equal to the comparison value.
		/// </summary>
		NotEqual,

		/// <summary>
		/// Filter in values which are greater than the comparison value.
		/// </summary>
		GreaterThan,

		/// <summary>
		/// Filter in values which are greater than or equal to the comparison value.
		/// </summary>
		GreaterThanOrEqual,

		/// <summary>
		/// Filter in values which are less than the comparison value.
		/// </summary>
		LessThan,

		/// <summary>
		/// Filter in values which are less than or equal to the comparison value.
		/// </summary>
		LessThanOrEqual,

		/// <summary>
		/// Filter in string values which begin with the comparison value.
		/// </summary>
		BeginsWith,

		/// <summary>
		/// Filter in string values which do not begin with the comparison value.
		/// </summary>
		DoesNotBeginWith,

		/// <summary>
		/// Filter in string values which ends with the comparison value.
		/// </summary>
		EndsWith,

		/// <summary>
		/// Filter in string values which do not end with the comparison value.
		/// </summary>
		DoesNotEndWith,

		/// <summary>
		/// Filter in string values which contain the comparison value.
		/// </summary>
		Contains,

		/// <summary>
		/// Filter in string values which do not contain the comparison value.
		/// </summary>
		DoesNotContain,
	}

	#endregion // ExcelComparisonOperator

	#region FixedDateGroupType




	/// <summary>
	/// Represents the various types, or precisions, of a <see cref="FixedDateGroup"/>.
	/// </summary>
	/// <seealso cref="FixedDateGroup.Type"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum FixedDateGroupType
	{
		/// <summary>
		/// The group represents the day in which the <see cref="FixedDateGroup.Value"/> exists.
		/// </summary>
		Day,

		/// <summary>
		/// The group represents the hour in which the <see cref="FixedDateGroup.Value"/> exists.
		/// </summary>
		Hour,

		/// <summary>
		/// The group represents the minute in which the <see cref="FixedDateGroup.Value"/> exists.
		/// </summary>
		Minute,

		/// <summary>
		/// The group represents the month in which the <see cref="FixedDateGroup.Value"/> exists.
		/// </summary>
		Month,

		/// <summary>
		/// The group represents the second in which the <see cref="FixedDateGroup.Value"/> exists.
		/// </summary>
		Second,

		/// <summary>
		/// The group represents the year in which the <see cref="FixedDateGroup.Value"/> exists.
		/// </summary>
		Year,
	}

	#endregion // FixedDateGroupType

	#region RelativeDateRangeDuration




	/// <summary>
	/// Represents the various durations which can be filtered by the <see cref="RelativeDateRangeFilter"/>.
	/// </summary>
	/// <seealso cref="RelativeDateRangeFilter.Duration"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum RelativeDateRangeDuration
	{
		/// <summary>
		/// The duration of accepted values is one day.
		/// </summary>
		Day,

		/// <summary>
		/// The duration of accepted values is one week.
		/// </summary>
		Week,

		/// <summary>
		/// The duration of accepted values is one month.
		/// </summary>
		Month,

		/// <summary>
		/// The duration of accepted values is one quarter.
		/// </summary>
		Quarter,

		/// <summary>
		/// The duration of accepted values is one year.
		/// </summary>
		Year,
	}

	#endregion // RelativeDateRangeDuration

	#region RelativeDateRangeOffset




	/// <summary>
	/// Represents the various relative date offsets which can be filtered by the <see cref="RelativeDateRangeFilter"/>.
	/// </summary>
	/// <seealso cref="RelativeDateRangeFilter.Offset"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum RelativeDateRangeOffset
	{
		/// <summary>
		/// Filter in values in the previous duration relative to the filter's creation date.
		/// </summary>
		Previous,

		/// <summary>
		/// Filter in values in the current duration relative to the filter's creation date.
		/// </summary>
		Current,

		/// <summary>
		/// Filter in values in the next duration relative to the filter's creation date.
		/// </summary>
		Next
	}

	#endregion // RelativeDateRangeOffset

	#region TopOrBottomFilterType




	/// <summary>
	/// Represents the various filter types available for the <see cref="TopOrBottomFilter"/>.
	/// </summary>
	/// <seealso cref="TopOrBottomFilter.Type"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum TopOrBottomFilterType
	{
		/// <summary>
		/// Filter in the top N values in the sorted list of values.
		/// </summary>
		TopValues,

		/// <summary>
		/// Filter in the bottom N values in the sorted list of values.
		/// </summary>
		BottomValues,

		/// <summary>
		/// Filter in the top N percent of values the sorted list of values.
		/// </summary>
		TopPercentage,

		/// <summary>
		/// Filter in the bottom N percent of values the sorted list of values.
		/// </summary>
		BottomPercentage,
	}

	#endregion // TopOrBottomFilterType
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