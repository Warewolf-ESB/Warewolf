using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;
using Infragistics.Controls.Schedules;
using System.ComponentModel;
using Infragistics.Collections;
using Infragistics;
using System.Windows.Markup;




namespace InfragisticsWPF4.Controls.Schedules.Design

{

	internal partial class MetadataStore : IProvideAttributeTable
	{
		#region AddCustomAttributes

		private void AddCustomAttributes(AttributeTableBuilder builder)
		{
			builder.AddCustomAttributes(typeof(DateRecurrence), new TypeConverterAttribute(typeof(ExpandableObjectConverter)));

			builder.AddCustomAttributes(typeof(DateRecurrence), "Rules", new NewItemTypesAttribute(
				typeof(MonthOfYearRecurrenceRule),
				typeof(WeekOfYearRecurrenceRule),
				typeof(DayOfYearRecurrenceRule),
				typeof(DayOfMonthRecurrenceRule),
				typeof(DayOfWeekRecurrenceRule),
				typeof(HourRecurrenceRule),
				typeof(MinuteRecurrenceRule),
				typeof(SecondRecurrenceRule),
				typeof(SubsetRecurrenceRule)));

			builder.AddCustomAttributes(typeof(ScheduleDaysOfWeek), new TypeConverterAttribute(typeof(ExpandableObjectConverter)));
			builder.AddCustomAttributes(typeof(ScheduleDialogFactoryBase), new TypeConverterAttribute(typeof(ExpandableObjectConverter)));

			builder.AddCustomAttributes(typeof(ScheduleTimeControlBase), "SecondaryTimeZoneId", new TypeConverterAttribute(typeof(TimeZoneIdConverter)));
			builder.AddCustomAttributes(typeof(TimeZoneInfoProvider), "LocalTimeZoneId", new TypeConverterAttribute(typeof(TimeZoneIdConverter)));
			builder.AddCustomAttributes(typeof(TimeZoneInfoProvider), "UtcTimeZoneId", new TypeConverterAttribute(typeof(TimeZoneIdConverter)));

			NewItemTypesAttribute nita = new NewItemTypesAttribute(typeof(TimeRange));
			nita.FactoryType = typeof(TimeRangeFactory);

			builder.AddCustomAttributes(typeof(WorkingHoursCollection), nita);


			DesignTimeHelper.HideInheritedUIProperties(builder, typeof(XamScheduleDataManager));
			DesignTimeHelper.HideInheritedUIProperties(builder, typeof(ScheduleDataConnectorBase));

			// XamScheduleDataManager Callback
			// ===============================
			builder.AddCallback(typeof(XamScheduleDataManager), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				var dialogFactoryAttrib = DesignTimeHelper.CreateNewItemTypesAttribute(typeof(ScheduleDialogFactoryBase));
				
				if (null != dialogFactoryAttrib)
					callbackBuilder.AddCustomAttributes("DialogFactory", dialogFactoryAttrib);

				var connectorAttrib = DesignTimeHelper.CreateNewItemTypesAttribute(typeof(ScheduleDataConnectorBase));

				if (null != connectorAttrib)
					callbackBuilder.AddCustomAttributes("DataConnector", connectorAttrib);
			});
		}

		#endregion //AddCustomAttributes
	}


    #region TimeRangeFactory internal class
    
    // since the base NewItemFactory doesn't work for structure (because the can't have
	// an explicit paramterless ctor which the efault implemantation looks for we need
	// derive a specific class for a structure
	internal class TimeRangeFactory : NewItemFactory
	{
		public override object CreateInstance(Type type)
		{
			if (type == typeof(TimeRange))
				return new TimeRange();

			return base.CreateInstance(type);
		}
	}

   	#endregion //TimeRangeFactory	
    
	#region TimeZoneIdConverter internal class

	internal class TimeZoneIdConverter : TypeConverter
	{
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			object instance = context != null ? context.Instance : null;

			
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


			ScheduleTimeControlBase control = instance != null ? instance as ScheduleTimeControlBase : null;

			TimeZoneInfoProvider provider = null;

			if (control != null && control.DataManager != null && control.DataManager.DataConnector != null)
				provider = control.DataManager.DataConnector.TimeZoneInfoProviderResolved;
			else
				provider = instance != null ? instance as TimeZoneInfoProvider : null;

			if (provider == null)
				provider = TimeZoneInfoProvider.DefaultProvider;

			List<string> ids = new List<string>(provider.TimeZoneTokens.Count);

			foreach (TimeZoneToken token in provider.TimeZoneTokens)
				ids.Add(token.Id);

			return new StandardValuesCollection(ids);
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}
	}

	#endregion //TimeZoneIdConverter internal class
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