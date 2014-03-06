using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules
{
	internal class ExchangeCalendarFolder : ExchangeFolder
	{
		#region Constants

		public const string CategoryListClass = "IPM.Configuration.CategoryList";
		public const string FolderClass = "IPF.Appointment";
		public const string ItemClass = "IPM.Appointment";
		public const string PropertySetId = "00062002-0000-0000-c000-000000000046";






		public const int RecurPropertyId = 0x8216;

		#endregion  // Constants

		#region Member Variables

		private CalendarFolderType _calendarFolder;

		#endregion  // Member Variables

		#region Constructors

		public ExchangeCalendarFolder(ExchangeService service, CalendarFolderType calendarFolder)
			: base(service)
		{
			Debug.Assert(
				calendarFolder.FolderClass == ExchangeCalendarFolder.FolderClass,
				"Incorrect folder type for appointments: " + calendarFolder.FolderClass);

			_calendarFolder = calendarFolder;
		}

		#endregion  // Constructors

		#region Base Class Overrides

		#region CleanItemForAdd

		public override void CleanItemForAdd(ItemType item)
		{
			base.CleanItemForAdd(item);

			CalendarItemType calendarItem = item as CalendarItemType;
			if (calendarItem == null)
				return;

			calendarItem.CalendarItemType1Specified = false;
			calendarItem.MeetingRequestWasSentSpecified = false;
		} 

		#endregion //CleanItemForAdd

		#region CreatePathToItemEndTime

		protected override BasePathToElementType CreatePathToItemEndTime()
		{
			return EWSUtilities.CreateProperty(UnindexedFieldURIType.calendarEnd);
		}

		#endregion  // CreatePathToItemEndTime

		#region CreatePathToItemStartTime

		protected override BasePathToElementType CreatePathToItemStartTime()
		{
			return EWSUtilities.CreateProperty(UnindexedFieldURIType.calendarStart);
		}

		#endregion  // CreatePathToItemStartTime

		#region CreateViewForDateRange

		public override BasePagingType CreateViewForDateRange(DateRange dateRange)
		{
			CalendarViewType calendarView = new CalendarViewType();
			calendarView.StartDate = dateRange.Start;
			calendarView.EndDate = dateRange.End;
			return calendarView;
		} 

		#endregion  // CreateViewForDateRange

		#region Folder
      
		public override BaseFolderType Folder
		{
			get { return _calendarFolder; }
		} 
    
		#endregion  // Folder

		#region GetItemPropertyName

		public override string GetItemPropertyName(UnindexedFieldURIType propertyType)
		{
			string propertyName = base.GetItemPropertyName(propertyType);

			if (propertyName != null)
				return propertyName;

			propertyName = Enum.GetName(typeof(UnindexedFieldURIType), propertyType);

			if (propertyName.StartsWith("calendar"))
			{
				propertyName = propertyName.Substring(8);

				switch (propertyName)
				{
					case "CalendarItemType":
						propertyName += "1";
						break;
				}

				return propertyName;
			}

			return null;
		} 

		#endregion  // GetItemPropertyName

		#region GetNeededProperties

		public override void GetNeededProperties(List<BasePathToElementType> neededProperties)
		{
			base.GetNeededProperties(neededProperties);

			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarCalendarItemType);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarEnd);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarIsAllDayEvent);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarLocation);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarMeetingRequestWasSent);

			
			

			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarRecurrence);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarStart);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarTimeZone);

			if (this.Service.Connector.RequestedServerVersionResolved != ExchangeVersion.Exchange2007_SP1)
			{
				EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarEndTimeZone);
				EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.calendarStartTimeZone);
			}
		} 

		#endregion  // GetNeededProperties

		#region ShouldIncludeItem

		public override bool ShouldIncludeItem(string itemClass)
		{
			return
				itemClass == ExchangeCalendarFolder.ItemClass ||
				itemClass == ExchangeCalendarFolder.CategoryListClass;
		}

		#endregion //ShouldIncludeItem

		#endregion  // Base Class Overrides
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