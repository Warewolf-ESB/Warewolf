using System.Text;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design.Services;
using Infragistics.Windows.DataPresenter;
using Infragistics.Shared;
using System;
using System.Windows.Media;
using System.Collections.Generic;

namespace Infragistics.Windows.Design.DataPresenter
{
    internal static class DALHelpers
    {
        #region AddBasicItems

		internal static void AddBasicItems(Type owningType, DesignerActionList designerActionList, DesignerActionItemCollection items, EditingContext context, ModelItem modelItem, int groupSequence)
        {
			// You can create one or more groups. Each group has a name and an order number. 
			// You can use these groups in order to group your controls in the smart tag.
    
			#region Appearance Group

			DesignerActionItemGroup groupAppearance = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Appearance"), groupSequence++);
			groupAppearance.IsExpanded				= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupAppearance))
			{
				items.Add(pic.GetPropertyActionItem("AutoFit"));
				items.Add(pic.GetPropertyActionItem("Background"));
				items.Add(pic.GetPropertyActionItem("Foreground"));
				items.Add(pic.GetPropertyActionItem("GroupByAreaLocation"));
				items.Add(pic.GetPropertyActionItem("Theme"));
			}

			#endregion //Appearance Group

			#region Data Group

			DesignerActionItemGroup groupData	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Data"), groupSequence++);
			groupData.IsExpanded				= false;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupData))
			{
				items.Add(pic.GetPropertyActionItem("BindToSampleData"));
				items.Add(pic.GetPropertyActionItem("DataSourceResetBehavior"));
				items.Add(pic.GetPropertyActionItem("IsNestedDataDisplayEnabled"));
				items.Add(pic.GetPropertyActionItem("IsSynchronizedWithCurrentItem"));
				items.Add(pic.GetPropertyActionItem("SortRecordsByDataType"));
				items.Add(pic.GetPropertyActionItem("UpdateMode"));
			}

			#endregion //Data Group	
    
			#region Virtualization Group

			DesignerActionItemGroup groupVirtualization = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Virtualization"), groupSequence++);
			groupVirtualization.IsExpanded				= false;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupVirtualization))
			{
				items.Add(pic.GetPropertyActionItem("CellContainerGenerationMode"));
				items.Add(pic.GetPropertyActionItem("RecordContainerGenerationMode"));
				items.Add(pic.GetPropertyActionItem("RecordLoadMode"));
				items.Add(pic.GetPropertyActionItem("ScrollingMode"));
			}

			#endregion //Virtualization Group	

			#region Clipboard Group

			DesignerActionItemGroup groupClipboard	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Clipboard"), groupSequence++);
			groupClipboard.IsExpanded				= false;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupClipboard))
			{
				items.Add(pic.GetPropertyActionItem("ClipboardCellDelimiter"));
				items.Add(pic.GetPropertyActionItem("ClipboardCellSeparator"));
				items.Add(pic.GetPropertyActionItem("ClipboardRecordSeparator"));
				items.Add(pic.GetPropertyActionItem("IsUndoEnabled"));
				items.Add(pic.GetPropertyActionItem("UndoLimit"));
			}

			#endregion //Clipboard Group

			#region RecordFiltering Group

			DesignerActionItemGroup groupRecordFiltering	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Filtering"), groupSequence++);
			groupRecordFiltering.IsExpanded					= false;

			items.Add(new DesignerActionTextItem(SR.GetString("SmartTag_N_RecordFiltering"), groupRecordFiltering, FontWeights.Bold, Brushes.DarkGray , 1));
			items.Add(new DesignerActionObjectPropertyItem("FieldSettings", SR.GetString("SmartTag_D_FieldSettingsRecordFiltering"), typeof(DALFieldSettings), modelItem, owningType, typeof(FieldSettings), "FieldSettings_RecordFiltering", groupRecordFiltering, new List<string>(new string[] { "RecordFiltering" }), 2));
			items.Add(new DesignerActionObjectPropertyItem("FieldLayoutSettings", SR.GetString("SmartTag_D_FieldLayoutSettingsRecordFiltering"), typeof(DALFieldLayoutSettings), modelItem, owningType, typeof(FieldLayoutSettings), "FieldLayoutSettings_RecordFiltering", groupRecordFiltering, new List<string>(new string[] { "RecordFiltering" }), 3));

			#endregion //RecordFiltering Group

			#region Summaries Group

			DesignerActionItemGroup groupSummaries	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Summaries"), groupSequence++);
			groupSummaries.IsExpanded				= false;

			items.Add(new DesignerActionTextItem(SR.GetString("SmartTag_N_Summaries"), groupSummaries, FontWeights.Bold, Brushes.DarkGray, 1));
			items.Add(new DesignerActionObjectPropertyItem("FieldSettings", SR.GetString("SmartTag_D_FieldSettingsSummaries"), typeof(DALFieldSettings), modelItem, owningType, typeof(FieldSettings), "FieldSettings_Summaries", groupSummaries, new List<string>(new string[] { "Summaries" }), 2));
			items.Add(new DesignerActionObjectPropertyItem("FieldLayoutSettings", SR.GetString("SmartTag_D_FieldLayoutSettingsSummaries"), typeof(DALFieldLayoutSettings), modelItem, owningType, typeof(FieldLayoutSettings), "FieldLayoutSettings_Summaries", groupSummaries, new List<string>(new string[] { "Summaries" }), 3));

			#endregion //RecordFiltering Group

			#region GroupByArea Group

			DesignerActionItemGroup groupGroupByArea	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_GroupByArea"), groupSequence++);
			groupGroupByArea.IsExpanded					= false;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupGroupByArea))
			{
				items.Add(pic.GetPropertyActionItem("GroupByAreaLocation"));
				items.Add(pic.GetPropertyActionItem("GroupByAreaMode"));
				items.Add(pic.GetPropertyActionItem("IsGroupByAreaExpanded"));
			}

			#endregion //GroupByArea Group	
    
			#region Settings Group

			DesignerActionItemGroup groupSettings	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Settings"), groupSequence++);
			groupSettings.IsExpanded				= false;
			items.Add(new DesignerActionObjectPropertyItem("FieldSettings", "FieldSettings", typeof(DALFieldSettings), modelItem, owningType, typeof(FieldSettings), "FieldSettings", groupSettings, null, 1));
			items.Add(new DesignerActionObjectPropertyItem("FieldLayoutSettings", "FieldLayoutSettings", typeof(DALFieldLayoutSettings), modelItem, owningType, typeof(FieldLayoutSettings), "FieldLayoutSettings", groupSettings, null, 2));

			#endregion //Settings Group	
		}

		#endregion //AddBasicItems
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