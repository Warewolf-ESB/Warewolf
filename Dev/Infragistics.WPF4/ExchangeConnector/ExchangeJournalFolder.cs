using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	internal class ExchangeJournalFolder : ExchangeFolder
	{
		#region Constants






		public const int EndPropertyId = 0x8708;

		public const string FolderClass = "IPF.Journal";
		public const string ItemClass = "IPM.Activity";
		public const string PropertySetId = "0006200a-0000-0000-c000-000000000046";






		public const int StartPropertyId = 0x8706;

		#endregion  // Constants

		#region Member Variables

		private FolderType _journalFolder;

		#endregion  // Member Variables

		#region Constructors

		public ExchangeJournalFolder(ExchangeService service, FolderType journalFolder)
			: base(service)
		{
			Debug.Assert(
				journalFolder.FolderClass == ExchangeJournalFolder.FolderClass,
				"Incorrect folder type for journals: " + journalFolder.FolderClass);

			_journalFolder = journalFolder;
		} 

		#endregion  // Constructors

		#region Base Class Overrides

		#region CreatePathToItemEndTime

		protected override BasePathToElementType CreatePathToItemEndTime()
		{
			return EWSUtilities.CreateExtendedProperty(
				ExchangeJournalFolder.PropertySetId,
				ExchangeJournalFolder.EndPropertyId,
				MapiPropertyTypeType.SystemTime);
		}

		#endregion  // CreatePathToItemEndTime

		#region CreatePathToItemStartTime

		protected override BasePathToElementType CreatePathToItemStartTime()
		{
			return EWSUtilities.CreateExtendedProperty(
				ExchangeJournalFolder.PropertySetId,
				ExchangeJournalFolder.StartPropertyId,
				MapiPropertyTypeType.SystemTime);
		}

		#endregion  // CreatePathToItemStartTime

		#region CreateRestrictionForDateRange

		public override RestrictionType CreateRestrictionForDateRange(DateRange dateRange)
		{
			return ExchangeFolder.CreateExtendedPropertyDateFilter(
				dateRange,
				ExchangeJournalFolder.PropertySetId,
				ExchangeJournalFolder.StartPropertyId,
				ExchangeJournalFolder.EndPropertyId,
				false);
		} 

		#endregion  // CreateRestrictionForDateRange

		#region Folder

		public override BaseFolderType Folder
		{
			get { return _journalFolder; }
		}

		#endregion  // Folder

		#region GetItemPropertyName

		public override string GetItemPropertyName(UnindexedFieldURIType propertyType)
		{
			string propertyName = base.GetItemPropertyName(propertyType);

			if (propertyName != null)
				return propertyName;

			propertyName = Enum.GetName(typeof(UnindexedFieldURIType), propertyType);

			if (propertyName.StartsWith("message"))
				return propertyName.Substring(7);

			return null;
		} 

		#endregion  // GetItemPropertyName

		#region GetNeededProperties

		public override void GetNeededProperties(List<BasePathToElementType> neededProperties)
		{
			base.GetNeededProperties(neededProperties);

			EWSUtilities.AppendAdditionalProperty(neededProperties,
				ExchangeJournalFolder.PropertySetId,
				ExchangeJournalFolder.StartPropertyId,
				MapiPropertyTypeType.SystemTime);

			EWSUtilities.AppendAdditionalProperty(neededProperties,
				ExchangeJournalFolder.PropertySetId,
				ExchangeJournalFolder.EndPropertyId,
				MapiPropertyTypeType.SystemTime);
		} 

		#endregion  // GetNeededProperties

		#region ShouldIncludeItem

		public override bool ShouldIncludeItem(string itemClass)
		{
			return itemClass == ExchangeJournalFolder.ItemClass;
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