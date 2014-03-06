using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;


namespace InfragisticsWPF4.Controls.Grids.XamGrid.Design



{
    internal partial class MetadataStore : IProvideAttributeTable
    {
        private void AddCustomAttributes(AttributeTableBuilder builder)
        {
            Type t = typeof(Infragistics.Controls.Grids.Primitives.RowsPanel);
            Assembly controlAssembly = t.Assembly;

            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.XamGrid"), "Columns", new NewItemTypesAttribute(
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.TextColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.DateColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.ImageColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.ColumnLayout"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.UnboundColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.TemplateColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.HyperlinkColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.GroupColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.CheckBoxColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.ComboBoxColumn")
                                                                                       ));


            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.FilterColumnSettings"), "RowFilterOperands", new NewItemTypesAttribute(
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.EqualsOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.NotEqualsOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOrEqualOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOrEqualOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.StartsWithOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.EndsWithOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.ContainsOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotContainOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotStartWithOperand"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotEndWithOperand"),

            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAfterFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeBeforeFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeTodayFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeTomorrowFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeYesterdayFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextWeekFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisWeekFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastWeekFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextMonthFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisMonthFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastMonthFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextQuarterFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisQuarterFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastQuarterFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextYearFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisYearFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastYearFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeYearToDateFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJanuaryFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeFebruaryFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeMarchFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAprilFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeMayFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJuneFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJulyFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAugustFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeSeptemberFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeOctoberFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNovemberFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeDecemberFilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter1FilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter2FilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter3FilterOperand"),
            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter4FilterOperand")
                                                                                       ));

            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.SummaryColumnSettings"), "SummaryOperands", new NewItemTypesAttribute(
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.MaximumSummaryOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.MinimumSummaryOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.CountSummaryOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.SumSummaryOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.AverageSummaryOperand")
                                                                           ));


            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.Column"), "ConditionalFormatCollection", new NewItemTypesAttribute(
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.AverageValueConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.BeginningWithConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.BetweenXandYConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.ContainingConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.EndingWithConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.EqualToConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOrEqualToConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.IconConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.LessThanConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOrEqualToConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.MaximumValueConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.MinimumValueConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.NotBetweenXandYConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.NotContainingConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.NotEqualToConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.ThreeColorScaleConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.TwoColorScaleConditionalFormatRule"),
                                                               controlAssembly.GetType("Infragistics.Controls.Grids.DataBarConditionalFormatRule")
                                                               ));


            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.FilteringSettingsOverride"), "RowFiltersCollection", new NewItemTypesAttribute(
                                                           controlAssembly.GetType("Infragistics.Controls.Grids.RowsFilter")
                                                           ));

            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.FilteringSettings"), "RowFiltersCollection", new NewItemTypesAttribute(
                                                     controlAssembly.GetType("Infragistics.Controls.Grids.RowsFilter")
                                                     ));

            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.ColumnLayout"), "Columns", new NewItemTypesAttribute(
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.TextColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.DateColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.ImageColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.ColumnLayout"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.UnboundColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.TemplateColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.HyperlinkColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.GroupColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.CheckBoxColumn"),
                                                                                       controlAssembly.GetType("Infragistics.Controls.Grids.ComboBoxColumn")
                                                                                       ));

            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.GroupColumn"), "Columns", new NewItemTypesAttribute(
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.TextColumn"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.DateColumn"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.ImageColumn"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.UnboundColumn"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.TemplateColumn"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.HyperlinkColumn"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.GroupColumn"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.CheckBoxColumn"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.ComboBoxColumn")
                                                                           ));

            builder.AddCustomAttributes(controlAssembly.GetType("Infragistics.Controls.Grids.FilterMenuTrackingObject"), "FilterOperands", new NewItemTypesAttribute(
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.EqualsOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.NotEqualsOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOrEqualOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOrEqualOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.StartsWithOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.EndsWithOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.ContainsOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotContainOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotStartWithOperand"),
                                                                           controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotEndWithOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAfterFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeBeforeFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeTodayFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeTomorrowFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeYesterdayFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextWeekFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisWeekFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastWeekFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextMonthFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisMonthFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastMonthFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextQuarterFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisQuarterFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastQuarterFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextYearFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisYearFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastYearFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeYearToDateFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJanuaryFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeFebruaryFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeMarchFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAprilFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeMayFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJuneFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJulyFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAugustFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeSeptemberFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeOctoberFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNovemberFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeDecemberFilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter1FilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter2FilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter3FilterOperand"),
                                                                            controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter4FilterOperand")
                                                                           ));




            Type[] typesInAssembly = controlAssembly.GetTypes();
            foreach (Type typeInAssembly in typesInAssembly)
            {
                PropertyInfo[] propsInType = typeInAssembly.GetProperties();
                foreach (PropertyInfo propertyInType in propsInType)
                {
                    if (propertyInType.PropertyType.Name.Contains("Nullable"))
                    {
                        Type[] genericTypes = propertyInType.PropertyType.GetGenericArguments();
                        if (genericTypes != null && genericTypes.Length > 0)
                        {
                            if (genericTypes[0].IsEnum)
                            {
                                builder.AddCustomAttributes(typeInAssembly, propertyInType.Name, PropertyValueEditor.CreateEditorAttribute(typeof(NullableEnumPropertyEditor)));
                            }
                        }
                    }
                }
            }
        }
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