using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	internal class ExchangeTasksFolder : ExchangeFolder
	{
		#region Constants






		public const int DueDatePropertyId = 0x8105;






		public const int RecurrencePropertyId = 0x8116;

		public const string FolderClass = "IPF.Task";

		public const string PropertySetId = "00062003-0000-0000-c000-000000000046";






		public const int OwnerPropertyId = 0x811f;






		public const int StartDatePropertyId = 0x8104;

		#endregion  // Constants

		#region Member Variables

		private TasksFolderType _tasksFolder; 

		#endregion  // Member Variables

		#region Constructors

		public ExchangeTasksFolder(ExchangeService service, TasksFolderType tasksFolder)
			: base(service)
		{
			Debug.Assert(
				tasksFolder.FolderClass == ExchangeTasksFolder.FolderClass,
				"Incorrect folder type for tasks: " + tasksFolder.FolderClass);

			_tasksFolder = tasksFolder;
		} 

		#endregion  // Constructors

		#region Base Class Overrides

		// MD 6/21/11 - TFS75934
		// When adding tasks to a user's tasks folder, we need to clean out the IsComplete value because that 
		// cannot be specified by the client.
		#region CleanItemForAdd

		public override void CleanItemForAdd(ItemType item)
		{
			base.CleanItemForAdd(item);

			TaskType task = item as TaskType;
			if (task == null)
				return;

			task.IsCompleteSpecified = false;
		}

		#endregion //CleanItemForAdd

		#region CreatePathToItemEndTime

		protected override BasePathToElementType CreatePathToItemEndTime()
		{
			return EWSUtilities.CreateExtendedProperty(
				ExchangeTasksFolder.PropertySetId,
				ExchangeTasksFolder.DueDatePropertyId,
				MapiPropertyTypeType.SystemTime);
		} 

		#endregion  // CreatePathToItemEndTime

		#region CreatePathToItemStartTime

		protected override BasePathToElementType CreatePathToItemStartTime()
		{
			return EWSUtilities.CreateExtendedProperty(
				ExchangeTasksFolder.PropertySetId,
				ExchangeTasksFolder.StartDatePropertyId,
				MapiPropertyTypeType.SystemTime);
		} 

		#endregion  // CreatePathToItemStartTime

		#region CreateRestrictionForDateRange

		public override RestrictionType CreateRestrictionForDateRange(DateRange dateRange)
		{
			return ExchangeFolder.CreateExtendedPropertyDateFilter(
				dateRange,
				ExchangeTasksFolder.PropertySetId,
				ExchangeTasksFolder.StartDatePropertyId,
				ExchangeTasksFolder.DueDatePropertyId,
				true);
		}

		#endregion  // CreateRestrictionForDateRange

		#region CreateRestrictionForGettingReminders

		public override RestrictionType CreateRestrictionForGettingReminders(DateTime baseTime)
		{
			IsEqualToType reminderIsSetCondition = new IsEqualToType();
			reminderIsSetCondition.Item = EWSUtilities.CreateProperty(UnindexedFieldURIType.itemReminderIsSet);
			reminderIsSetCondition.FieldURIOrConstant = EWSUtilities.CreateConstant("true");

			RestrictionType restriction = new RestrictionType();
			restriction.Item = reminderIsSetCondition;
			return restriction;
		}

		#endregion  // CreateRestrictionForGettingReminders

		#region Folder

		public override BaseFolderType Folder
		{
			get { return _tasksFolder; }
		}

		#endregion  // Folder 

		#region GetItemPropertyName

		public override string GetItemPropertyName(UnindexedFieldURIType propertyType)
		{
			string propertyName = base.GetItemPropertyName(propertyType);

			if (propertyName != null)
				return propertyName;

			propertyName = Enum.GetName(typeof(UnindexedFieldURIType), propertyType);

			if (propertyName.StartsWith("task"))
				return propertyName.Substring(4);

			return null;
		} 

		#endregion  // GetItemPropertyName

		#region GetNeededProperties

		public override void GetNeededProperties(List<BasePathToElementType> neededProperties)
		{
			base.GetNeededProperties(neededProperties);

			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.taskDueDate);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.taskIsComplete);
			EWSUtilities.AppendAdditionalProperty(neededProperties, ExchangeTasksFolder.PropertySetId, ExchangeTasksFolder.OwnerPropertyId, MapiPropertyTypeType.String);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.taskPercentComplete);

			
			

			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.taskRecurrence);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.taskStartDate);
		} 

		#endregion  // GetNeededProperties

		#region ShouldIncludeItem

		public override bool ShouldIncludeItem(string itemClass)
		{
			return itemClass == "IPM.Task";
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