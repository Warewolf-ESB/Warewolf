using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
    #region DateTimeNoInputBaseFilterOperand
    /// <summary>
    /// A class that defines common behavior for <see cref="DateTime"/> based <see cref="FilterOperand"/>s that do not require user input.
    /// </summary>
    public abstract class DateTimeNoInputBaseFilterOperand : FilterOperand
    {
        #region RequiresFilteringInput
        /// <summary>
        /// Gets if the then filter requires input to be applied or is standalone.
        /// </summary>
        public override bool RequiresFilteringInput
        {
            get
            {
                return false;
            }
        }
        #endregion // RequiresFilteringInput
    }
    #endregion // DateTimeNoInputBaseFilterOperand

    #region DateTimeAfterFilterOperand

    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime After filter.
    /// </summary>
    public class DateTimeAfterFilterOperand : FilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeAfterFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeAfter;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeAfterFilterOperand

    #region DateTimeBeforeFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Before filter.
    /// </summary>
    public class DateTimeBeforeFilterOperand : FilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeBeforeFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeBefore;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeBeforeFilterOperand

    #region DateTimeTodayFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Today filter.
    /// </summary>
    public class DateTimeTodayFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeTodayFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeToday;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeTodayFilterOperand

    #region DateTimeTomorrowFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Tomorrow filter.
    /// </summary>
    public class DateTimeTomorrowFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeTomorrowFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeTomorrow;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeTomorrowFilterOperand

    #region DateTimeYesterdayFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Yesterday filter.
    /// </summary>
    public class DateTimeYesterdayFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeYesterdayFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeYesterday;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeYesterdayFilterOperand

    #region DateTimeNextWeekFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Next Week filter.
    /// </summary>
    public class DateTimeNextWeekFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeNextWeekFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeNextWeek;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeNextWeekFilterOperand

    #region DateTimeThisWeekFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime This Week filter.
    /// </summary>
    public class DateTimeThisWeekFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeThisWeekFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeThisWeek;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeThisWeekFilterOperand

    #region DateTimeLastWeekFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Last Week filter.
    /// </summary>
    public class DateTimeLastWeekFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeLastWeekFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeLastWeek;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeLastWeekFilterOperand

    #region DateTimeNextMonthFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Next Month filter.
    /// </summary>
    public class DateTimeNextMonthFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeNextMonthFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeNextMonth;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeNextMonthFilterOperand

    #region DateTimeThisMonthFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime This Month filter.
    /// </summary>
    public class DateTimeThisMonthFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeThisMonthFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeThisMonth;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeThisMonthFilterOperand

    #region DateTimeLastMonthFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Last Month filter.
    /// </summary>
    public class DateTimeLastMonthFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeLastMonthFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeLastMonth;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeLastMonthFilterOperand

    #region DateTimeNextQuarterFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Next Quarter filter.
    /// </summary>
    public class DateTimeNextQuarterFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeNextQuarterFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeNextQuarter;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeNextQuarterFilterOperand

    #region DateTimeThisQuarterFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime This Quarter filter.
    /// </summary>
    public class DateTimeThisQuarterFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeThisQuarterFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeThisQuarter;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion //DateTimeThisQuarterFilterOperand

    #region DateTimeLastQuarterFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Last Quarter filter.
    /// </summary>
    public class DateTimeLastQuarterFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeLastQuarterFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeLastQuarter;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeLastQuarterFilterOperand

    #region DateTimeNextYearFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Next Year filter.
    /// </summary>
    public class DateTimeNextYearFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeNextYearFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeNextYear;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeNextYearFilterOperand

    #region DateTimeThisYearFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime This Year filter.
    /// </summary>
    public class DateTimeThisYearFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeThisYearFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeThisYear;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeThisYearFilterOperand

    #region DateTimeLastYearFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Last Year filter.
    /// </summary>
    public class DateTimeLastYearFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeLastYearFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeLastYear;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeLastYearFilterOperand

    #region DateTimeAllDatesInPeriodFilterOperand
    internal class DateTimeAllDatesInPeriodFilterOperand : FilterOperand
    {
    }
    #endregion // DateTimeAllDatesInPeriodFilterOperand

    #region DateTimeYearToDateFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> which will do a DateTime Year to Date filter.
    /// </summary>
    public class DateTimeYearToDateFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeYearToDateFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeYearToDate;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeYearToDateFilterOperand

    #region DateTimeJanuaryFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in January.
    /// </summary>
    public class DateTimeJanuaryFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeJanuaryFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeJanuary;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeJanuaryFilterOperand

    #region DateTimeFebruaryFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in February.
    /// </summary>
    public class DateTimeFebruaryFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeFebruaryFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeFebruary;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeFebruaryFilterOperand

    #region DateTimeMarchFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in March.
    /// </summary>
    public class DateTimeMarchFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeMarchFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeMarch;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeMarchFilterOperand

    #region DateTimeAprilFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in April.
    /// </summary>
    public class DateTimeAprilFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeAprilFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeApril;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeAprilFilterOperand

    #region DateTimeMayFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in May.
    /// </summary>
    public class DateTimeMayFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeMayFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeMay;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeMayFilterOperand

    #region DateTimeJuneFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in June.
    /// </summary>
    public class DateTimeJuneFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeJuneFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeJune;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeJuneFilterOperand

    #region DateTimeJulyFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in July.
    /// </summary>
    public class DateTimeJulyFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeJulyFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeJuly;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeJulyFilterOperand

    #region DateTimeAugustFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in August.
    /// </summary>
    public class DateTimeAugustFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeAugustFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeAugust;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeAugustFilterOperand

    #region DateTimeSeptemberFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in September.
    /// </summary>
    public class DateTimeSeptemberFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeSeptemberFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeSeptember;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeSeptemberFilterOperand

    #region DateTimeOctoberFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in October.
    /// </summary>
    public class DateTimeOctoberFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeOctoberFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeOctober;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeOctoberFilterOperand

    #region DateTimeNovemberFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in November.
    /// </summary>
    public class DateTimeNovemberFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeNovemberFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeNovember;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeNovemberFilterOperand

    #region DateTimeDecemberFilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in December.
    /// </summary>
    public class DateTimeDecemberFilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeDecemberFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeDecember;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeDecemberFilterOperand

    #region DateTimeQuarter1FilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in Quarter1.
    /// </summary>
    public class DateTimeQuarter1FilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeQuarter1FilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeQuarter1;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeQuarter1FilterOperand

    #region DateTimeQuarter2FilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in Quarter2.
    /// </summary>
    public class DateTimeQuarter2FilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeQuarter2FilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeQuarter2;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeQuarter2FilterOperand

    #region DateTimeQuarter3FilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in Quarter3.
    /// </summary>
    public class DateTimeQuarter3FilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeQuarter3FilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeQuarter3;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeQuarter3FilterOperand

    #region DateTimeQuarter4FilterOperand
    /// <summary>
    /// A <see cref="FilterOperand"/> that will filter out all dates which are not in Quarter4.
    /// </summary>
    public class DateTimeQuarter4FilterOperand : DateTimeNoInputBaseFilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRGrid.GetString("DateTimeQuarter4FilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region ComparisonOperator
        /// <summary>
        /// Gets the operator that will be associated with this operand.
        /// </summary>
        public override ComparisonOperator? ComparisonOperatorValue
        {
            get
            {
                return ComparisonOperator.DateTimeQuarter4;
            }
        }

        #endregion  // ComparisonOperator

        #endregion // Properties
    }
    #endregion // DateTimeQuarter4FilterOperand
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