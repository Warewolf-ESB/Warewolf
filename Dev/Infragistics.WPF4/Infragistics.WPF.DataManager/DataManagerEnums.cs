namespace Infragistics
{

    #region LogicalOperator
    /// <summary>
    /// And enumeration of available operators.
    /// </summary>
    public enum LogicalOperator
    {
        /// <summary>
        /// Performs a logical AND operation.
        /// </summary>
        And,
        /// <summary>
        /// Performs a logical OR operation.
        /// </summary>
        Or
    }
    #endregion // LogicalOperator

    #region ComparisonOperator
    /// <summary>
    /// Enum describing operators which can be used for filtering.
    /// </summary>
    public enum ComparisonOperator
    {
        /// <summary>
        /// An equality compare is executed.  With string values, a case sensitivity flag may effect the result.
        /// </summary>
        Equals,
        /// <summary>
        /// An non equality compare is executed.  With string values, a case sensitivity flag may effect the result.
        /// </summary>
        NotEquals,
        /// <summary>
        /// A GreaterThan compare is executed. With string values, a case sensitivity flag may effect the result.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// A greater than or equal compare is executed.  With string values, a case sensitivity flag may effect the result.
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// A less than compare is executed.  With string values, a case sensitivity flag may effect the result.
        /// </summary>
        LessThan,

        /// <summary>
        /// A less than or equal compare is executed.  With string values, a case sensitivity flag may effect the result.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// Evaluates if a string begins with a compared value. With string values, a case sensitivity flag may effect the result.
        /// </summary>
        StartsWith,

        /// <summary>
        /// Evaluates if a string does not begin with a compared value. With string values, a case sensitivity flag may effect the result.
        /// </summary>
        DoesNotStartWith,

        /// <summary>
        /// Evaluates if a string ends with a compared value. With string values, a case sensitivity flag may effect the result.
        /// </summary>
        EndsWith,

        /// <summary>
        /// Evaluates if a string does not end with a compared value. With string values, a case sensitivity flag may effect the result.
        /// </summary>
        DoesNotEndWith,

        /// <summary>
        /// Evaluates if a string contains a compared value. With string values, a case sensitivity flag may effect the result.
        /// </summary>
        Contains,

        /// <summary>
        /// Evaluates if a string does not contain a compared value. With string values, a case sensitivity flag may effect the result.
        /// </summary>
        DoesNotContain,

        /// <summary>
        /// Evaluates if a DateTime object comes after a given input.
        /// </summary>
        DateTimeAfter,

        /// <summary>
        /// Evaluates if a DateTime object comes prior to a given input.
        /// </summary>
        DateTimeBefore,

        /// <summary>
        /// Evaluates if a DateTime value is today.
        /// </summary>
        DateTimeToday,

        /// <summary>
        /// Evaluates if a DateTime value is tomorrow.
        /// </summary>
        DateTimeTomorrow,

        /// <summary>
        /// Evaluates if a DateTime value is yesterday.
        /// </summary>
        DateTimeYesterday,

        /// <summary>
        /// Evaluates if a DateTime value is in this week.
        /// </summary>
        DateTimeThisWeek,

        /// <summary>
        /// Evaluates if a DateTime value is in next week.
        /// </summary>
        DateTimeNextWeek,

        /// <summary>
        /// Evaluates if a DateTime value is in last week.
        /// </summary>
        DateTimeLastWeek,

        /// <summary>
        /// Evaluates if a DateTime value is in this month.
        /// </summary>
        DateTimeThisMonth,

        /// <summary>
        /// Evaluates if a DateTime value is in last month.
        /// </summary>
        DateTimeLastMonth,

        /// <summary>
        /// Evaluates if a DateTime value is in next month.
        /// </summary>
        DateTimeNextMonth,

        /// <summary>
        /// Evaluates if a DateTime value is in this year.
        /// </summary>
        DateTimeThisYear,

        /// <summary>
        /// Evaluates if a DateTime value is in last year.
        /// </summary>
        DateTimeLastYear,

        /// <summary>
        /// Evaluates if a DateTime value is in next year.
        /// </summary>
        DateTimeNextYear,

        /// <summary>
        /// Evaluates if a DateTime value is in the current year up to today's date.
        /// </summary>
        DateTimeYearToDate,

        /// <summary>
        /// Evaluates if a DateTime value is in the last quarter.
        /// </summary>
        DateTimeLastQuarter,

        /// <summary>
        /// Evaluates if a DateTime value is in this quarter.
        /// </summary>
        DateTimeThisQuarter,

        /// <summary>
        /// Evaluates if a DateTime value is in the next quarter.
        /// </summary>
        DateTimeNextQuarter,

        /// <summary>
        /// Evaluates that a DateTime is in January.
        /// </summary>
        DateTimeJanuary,

        /// <summary>
        /// Evaluates that a DateTime is in February.
        /// </summary>
        DateTimeFebruary,

        /// <summary>
        /// Evaluates that a DateTime is in March.
        /// </summary>
        DateTimeMarch,

        /// <summary>
        /// Evaluates that a DateTime is in April.
        /// </summary>
        DateTimeApril,

        /// <summary>
        /// Evaluates that a DateTime is in May.
        /// </summary>
        DateTimeMay,

        /// <summary>
        /// Evaluates that a DateTime is in June.
        /// </summary>
        DateTimeJune,

        /// <summary>
        /// Evaluates that a DateTime is in July.
        /// </summary>
        DateTimeJuly,

        /// <summary>
        /// Evaluates that a DateTime is in August.
        /// </summary>
        DateTimeAugust,

        /// <summary>
        /// Evaluates that a DateTime is in Sepember.
        /// </summary>
        DateTimeSeptember,

        /// <summary>
        /// Evaluates that a DateTime is in October.
        /// </summary>
        DateTimeOctober,

        /// <summary>
        /// Evaluates that a DateTime is in November.
        /// </summary>
        DateTimeNovember,

        /// <summary>
        /// Evaluates that a DateTime is in December.
        /// </summary>
        DateTimeDecember,

        /// <summary>
        /// Evaluates that a DateTime is in the first quarter.
        /// </summary>
        DateTimeQuarter1,

        /// <summary>
        /// Evaluates that a DateTime is in the second quarter.
        /// </summary>
        DateTimeQuarter2,

        /// <summary>
        /// Evaluates that a DateTime is in the third quarter.
        /// </summary>
        DateTimeQuarter3,

        /// <summary>
        /// Evaluates that a DateTime is in the forth quarter.
        /// </summary>
        DateTimeQuarter4
    }

    #endregion // ComparisonOperator

    #region LinqSummaryOperator

    /// <summary>
    /// An enum used by summary to designate that a LINQ summary should be use.
    /// </summary>
    public enum LinqSummaryOperator
    {
        /// <summary>
        /// Use the LINQ Count summary
        /// </summary>
        Count,

        /// <summary>
        /// Use the LINQ Minimum summary.
        /// </summary>
        Minimum,

        /// <summary>
        /// Use the LINQ Maximum summary.
        /// </summary>
        Maximum,

        /// <summary>
        /// Use the LINQ Sum summary.
        /// </summary>
        Sum,

        /// <summary>
        /// Use the LINQ Average summary.
        /// </summary>
        Average
    }

    #endregion // LinqSummaryOperator

    #region SummaryExecution

    /// <summary>
    /// Enum that is used to determine when a summary should be calculated.
    /// </summary>
    public enum SummaryExecution
    {
        /// <summary>
        /// Summary is executed prior to paging and filtering.
        /// </summary>
        PriorToFilteringAndPaging,

        /// <summary>
        /// Summary is executed prior to paging but after filtering.
        /// </summary>
        AfterFilteringBeforePaging,

        /// <summary>
        /// Summary is executed after paging and filtering.
        /// </summary>
        AfterFilteringAndPaging
    }

    #endregion // SummaryExecution

    #region EvaluationStage

    /// <summary>
    /// Enumeration which lists when conditional formatting data will be gathered.
    /// </summary>
    public enum EvaluationStage
    {
        /// <summary>
        /// GatherData will not be called.
        /// </summary>
        None,

        /// <summary>
        /// GatherData will be called prior to filtering and paging.
        /// </summary>
        PriorToFilteringAndPaging,

        /// <summary>
        /// GatherData will be called after filtering and before paging.
        /// </summary>
        AfterFilteringBeforePaging,

        /// <summary>
        /// GatherData will be called after filtering and after paging.
        /// </summary>
        AfterFilteringAndPaging
    }
    #endregion // EvaluationStage

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