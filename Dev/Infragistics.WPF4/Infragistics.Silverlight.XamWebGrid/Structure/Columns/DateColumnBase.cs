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
using System.Collections.Generic;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A Base Class for Date or DateTime Columns that helps set up filtering.
    /// </summary>
    public abstract class DateColumnBase : CustomDisplayEditableColumn
    {
        #region Members
        const string _filterMenuCustomFilterString = "DateFilters";
        #endregion // Members

        #region Overrides

        #region Properties

        #region FormatStringResolved

        /// <summary>
        /// A format string for formatting data in this column.
        /// </summary>
        protected internal override string FormatStringResolved
        {
            get
            {
                return "{0:d}";
            }
        }

        #endregion // FormatStringResolved

        #region FilterMenuCustomFilterString
        /// <summary>
        /// Gets the default string for the FilterMenu for the CustomFilter text
        /// </summary>
        protected override string FilterMenuCustomFilterString
        {
            get
            {
                return SRGrid.GetString(_filterMenuCustomFilterString);
            }
        }
        #endregion // FilterMenuCustomFilterString

        #endregion // Properties

        #region GenerateFilterSelectionControl

        /// <summary>
        /// Returns a new instance of the <see cref="SelectionControl"/> which will be used by the FilterMenu.
        /// </summary>
        /// <returns></returns>
        protected internal override SelectionControl GenerateFilterSelectionControl()
        {
            return new DateFilterSelectionControl();
        }

        #endregion // GenerateFilterSelectionControl

        #region FillAvailableFilters

        /// <summary>
        /// Fills the <see cref="FilterOperandCollection"/> with the operands that the column expects as filter values.
        /// </summary>
        /// <param name="availableFilters"></param>
        protected internal override void FillAvailableFilters(FilterOperandCollection availableFilters)
        {
            base.FillAvailableFilters(availableFilters);

            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
            {
                Dictionary<ComparisonOperator, DataTemplate> icons = this.ColumnLayout.Grid.FilterIcons;
                if (icons != null && this.DataType != null)
                {
                    //availableFilters.Add(new GreaterThanOperand() { XamWebGrid = this.ColumnLayout.Grid, DisplayName = SRGrid.GetString("After") });
                    availableFilters.Add(new GreaterThanOrEqualOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    //availableFilters.Add(new LessThanOperand() { XamWebGrid = this.ColumnLayout.Grid, DisplayName = SRGrid.GetString("Before") });
                    availableFilters.Add(new LessThanOrEqualOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeAfterFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeBeforeFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeTodayFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeTomorrowFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeYesterdayFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeThisWeekFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeLastWeekFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeNextWeekFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeThisMonthFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeLastMonthFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeNextMonthFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeThisYearFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeLastYearFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeNextYearFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeThisQuarterFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeLastQuarterFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeNextQuarterFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeYearToDateFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });


                    availableFilters.Add(new DateTimeQuarter1FilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeQuarter2FilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeQuarter3FilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeQuarter4FilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeJanuaryFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeFebruaryFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeMarchFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeAprilFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeMayFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeJuneFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeJulyFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeAugustFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeSeptemberFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeOctoberFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeNovemberFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    availableFilters.Add(new DateTimeDecemberFilterOperand() { XamWebGrid = this.ColumnLayout.Grid });
                }
            }
        }
        #endregion // FillAvailableFilters

        #region FillFilterMenuOptions

        /// <summary>
        /// Fills the inputted list with options for the FilterMenu control.
        /// </summary>
        /// <param name="list"></param>
        internal protected override void FillFilterMenuOptions(List<FilterMenuTrackingObject> list)
        {
            base.FillFilterMenuOptions(list);

            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
            {
                if (list != null && list.Count > 0)
                {
                    FilterMenuTrackingObject fmto = list[0];
                    list = fmto.Children;

                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    list.Add(new FilterMenuTrackingObject(new LessThanOperand() { DisplayName = SRGrid.GetString("BeforeEllipsis") }));
                    list.Add(new FilterMenuTrackingObject(new GreaterThanOperand() { DisplayName = SRGrid.GetString("AfterEllipsis") }));
                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    list.Add(new FilterMenuTrackingObject(new DateTimeTomorrowFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeTodayFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeYesterdayFilterOperand()));
                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    list.Add(new FilterMenuTrackingObject(new DateTimeNextWeekFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeThisWeekFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeLastWeekFilterOperand()));
                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    list.Add(new FilterMenuTrackingObject(new DateTimeNextMonthFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeThisMonthFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeLastMonthFilterOperand()));
                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    list.Add(new FilterMenuTrackingObject(new DateTimeNextQuarterFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeThisQuarterFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeLastQuarterFilterOperand()));
                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    list.Add(new FilterMenuTrackingObject(new DateTimeNextYearFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeThisYearFilterOperand()));
                    list.Add(new FilterMenuTrackingObject(new DateTimeLastYearFilterOperand()));
                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    list.Add(new FilterMenuTrackingObject(new DateTimeYearToDateFilterOperand()));

                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    FilterMenuTrackingObject addDatesInPeriod = new FilterMenuTrackingObject() { Label = SRGrid.GetString("AllDatesInPeriod") };

                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeQuarter1FilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeQuarter2FilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeQuarter3FilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeQuarter4FilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeJanuaryFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeFebruaryFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeMarchFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeAprilFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeMayFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeJuneFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeJulyFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeAugustFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeSeptemberFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeOctoberFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeNovemberFilterOperand()));
                    addDatesInPeriod.Children.Add(new FilterMenuTrackingObject(new DateTimeDecemberFilterOperand()));
                    list.Add(addDatesInPeriod);
                    list.Add(new FilterMenuTrackingObject() { IsSeparator = true });

                    string customFilterLabel = string.IsNullOrEmpty(this.FilterColumnSettings.FilterMenuTypeSpecificFiltersString) ? SRGrid.GetString("CustomFiltersWithoutTypeEllipsis") : this.FilterColumnSettings.FilterMenuTypeSpecificFiltersString;
                    list.Add(new FilterMenuTrackingObject(new EqualsOperand() { DisplayName = customFilterLabel }) { IsCustomOption = true });
                }
            }
        }

        #endregion // FillFilterMenuOptions

        #endregion // Overrides

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