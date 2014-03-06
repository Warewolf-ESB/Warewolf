using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Windows.Controls
{
    #region ComparisonOperator Enum

	/// <summary>
	/// Enum for specifying the comparison operator.
	/// </summary>
	/// <remarks>
	/// Used for specifying <see cref="ComparisonCondition"/>'s <see cref="ComparisonCondition.Operator"/> property.
	/// </remarks>
	/// <seealso cref="ComparisonCondition.Operator"/>
    public enum ComparisonOperator
    {
		/// <summary>
		/// Tests for two values being equal.
		/// </summary>
        Equals              = 0,

		/// <summary>
		/// Tests for two values being not equal.
		/// </summary>
        NotEquals           = 1,

		/// <summary>
		/// Tests for the value being less than the comparison value.
		/// </summary>
        LessThan            = 2,

		/// <summary>
		/// Tests for the value being less than or equal to the comparison value.
		/// </summary>
        LessThanOrEqualTo  = 3,

		/// <summary>
		/// Tests for the value being greater than the comparison value.
		/// </summary>
        GreaterThan         = 4,

		/// <summary>
		/// Tests for the value being greater than or equal to the comparison value.
		/// </summary>
        GreaterThanOrEqualTo = 5,

		/// <summary>
		/// Tests to see if the value contains the comparison value.
		/// </summary>
        Contains            = 6,

		/// <summary>
		/// Complement of Contains.
		/// </summary>
        DoesNotContain      = 7,

		/// <summary>
		/// Will do a wild-card comparison of the value to the comparison value
		/// where the comparison value is the wild card string.
		/// </summary>
        Like                = 8,

		/// <summary>
		/// Complement of Like.
		/// </summary>
        NotLike             = 9,

		/// <summary>
		/// Will do a regular expression comparison of the value to the comparison
		/// value where the comparison value is the regular expression string.
		/// </summary>
        Match               = 10,

		/// <summary>
		/// Complement of Match.
		/// </summary>
        DoesNotMatch        = 11,

		/// <summary>
		/// Tests to see if the value starts with the comparison value.
		/// </summary>
        StartsWith          = 12,

		/// <summary>
		/// Complement of StartsWith.
		/// </summary>
        DoesNotStartWith    = 13,

		/// <summary>
		/// Tests to see if the value ends with the comparison value.
		/// </summary>
        EndsWith            = 14,

		/// <summary>
		/// Complement of EndsWith.
		/// </summary>
        DoesNotEndWith      = 15,

		/// <summary>
		/// Tests to see if the value is in the top 'X' values where 'X' is specified in the operand. 
		/// </summary>
        Top                 = 16,

		/// <summary>
		/// Tests to see if the value is in the bottom 'X' values where 'X' is specified in the operand.
		/// </summary>
        Bottom               = 17,

		/// <summary>
		/// Tests to see if the value is in the top 'X' percentile of values where 'X' is specified in the operand. 
		/// </summary>
        TopPercentile        = 18,

		/// <summary>
		/// Tests to see if the value is in the bottom 'X' percentile of values where 'X' is specified in the operand. 
		/// </summary>
        BottomPercentile        = 19
    }

    #endregion // ComparisonOperator Enum

    #region ComparisonOperatorFlags Enum

	/// <summary>
	/// Flagged enum that mirrors the values of non-flagged <see cref="ComparisonOperator"/> enum. This is used to specify
	/// properties like data presenter's FieldSettings's <b>FilterOperatorDropDownItems</b>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ComparisonOperatorFlags</b> is a flagged enum with values that mirror the values of 
	/// non-flagged <see cref="ComparisonOperator"/> enum. This is used to specify
	/// properties like data presenter's FieldSettings's <b>FilterOperatorDropDownItems</b>.
	/// </para>
	/// </remarks>
	/// <seealso cref="ComparisonOperator"/>
	[Flags]
    public enum ComparisonOperatorFlags
    {
		/// <summary>
		/// None.
		/// </summary>
		None                = 0,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>Equals</b>.
		/// </summary>
        Equals              = 1 << 0,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>NotEquals</b>.
		/// </summary>
        NotEquals           = 1 << 1,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>LessThan</b>.
		/// </summary>
        LessThan            = 1 << 2,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>LessThanOrEqualTo</b>.
		/// </summary>
        LessThanOrEqualsTo  = 1 << 3,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>GreaterThan</b>.
		/// </summary>
        GreaterThan         = 1 << 4,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>GreaterThanOrEqualTo</b>.
		/// </summary>
        GreaterThanOrEqualsTo = 1 << 5,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>Contains</b>.
		/// </summary>
        Contains            = 1 << 6,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>DoesNotContain</b>.
		/// </summary>
        DoesNotContain      = 1 << 7,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>Like</b>.
		/// </summary>
        Like                = 1 << 8,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>NotLike</b>.
		/// </summary>
        NotLike             = 1 << 9,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>Match</b>.
		/// </summary>
        Match               = 1 << 10,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>DoesNotMatch</b>.
		/// </summary>
        DoesNotMatch        = 1 << 11,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>StartsWith</b>.
		/// </summary>
        StartsWith          = 1 << 12,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>DoesNotStartWith</b>.
		/// </summary>
        DoesNotStartWith    = 1 << 13,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>EndsWith</b>.
		/// </summary>
        EndsWith            = 1 << 14,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>DoesNotEndWith</b>.
		/// </summary>
        DoesNotEndWith      = 1 << 15,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>Top</b>.
		/// </summary>
        Top                 = 1 << 16,

		/// <summary>
		/// Corresponds to the <see cref="ComparisonOperator"/> <b>Bottom</b>.
		/// </summary>
        Bottom              = 1 << 17,

		/// <summary>
        /// Corresponds to the <see cref="ComparisonOperator"/> <b>TopPercentile</b>.
		/// </summary>
        TopPercentile       = 1 << 18,

		/// <summary>
        /// Corresponds to the <see cref="ComparisonOperator"/> <b>BottomPercentile</b>.
		/// </summary>
        BottomPercentile    = 1 << 19,

		/// <summary>
		/// All operators.
		/// </summary>
        All                 = 0xfffff
    }

    #endregion // ComparisonOperatorFlags Enum

	// AS - NA 11.2 Excel Style Filtering
	#region DateRangeScope
	/// <summary>
	/// Enumeration used to indicate what part of a date time is considered when comparing dates.
	/// </summary>
	/// <see cref="SpecialFilterOperands.CreateDateRangeOperand"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public enum DateRangeScope
	{
		/// <summary>
		/// Only the year is considered.
		/// </summary>
		Year,

		/// <summary>
		/// The Year and Month are considered.
		/// </summary>
		Month,

		/// <summary>
		/// The date (i.e. year, month and day) are considered.
		/// </summary>
		Day,

		/// <summary>
		/// The hour within a specific date is considered
		/// </summary>
		Hour,

		/// <summary>
		/// The full date ignoring the seconds are considered
		/// </summary>
		Minute,

		/// <summary>
		/// The full date ignoring milliseconds are considered
		/// </summary>
		Second
	} 
	#endregion //DateRangeScope

    #region LogicalOperator Enum

	/// <summary>
	/// Enum for specifying logical operator that should be used to combine multiple conditions.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>LogicalOperator</b> enum is used for specifying <see cref="ConditionGroup"/>'s 
	/// <see cref="ConditionGroup.LogicalOperator"/> that will be used to combine evaluation 
	/// results of multiple conditions.
	/// </para>
	/// </remarks>
	/// <seealso cref="ConditionGroup.LogicalOperator"/>
    public enum LogicalOperator
    {
		/// <summary>
		/// <b>'And'</b> logical operator. In order for a successful match, all conditions have
		/// to pass.
		/// </summary>
        And = 0,

		/// <summary>
		/// <b>'Or'</b> logical operator. In order for a successful match, at least one of the 
		/// conditions have to pass.
		/// </summary>
        Or  = 1
    }

    #endregion // LogicalOperator Enum
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