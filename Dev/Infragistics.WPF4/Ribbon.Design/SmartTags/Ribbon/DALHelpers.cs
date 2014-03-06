using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Model;
using Infragistics.Windows.Ribbon;
using System.Windows;

namespace Infragistics.Windows.Design.Ribbon
{
	internal static class DALHelpers
	{
		#region AddStringsGroup

		internal static DesignerActionItemGroup AddStringsGroup(Type owningType, DesignerActionItemCollection items, bool isExpanded, ref int groupSequence)
		{
			DesignerActionItemGroup groupStrings	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Strings"), groupSequence++);
			groupStrings.IsExpanded					= isExpanded;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupStrings))
			{
				items.Add(pic.GetPropertyActionItem("Caption"));
				items.Add(pic.GetPropertyActionItem("KeyTip"));
				items.Add(pic.GetPropertyActionItem("ToolTip"));
			}

			return groupStrings;
		}

		#endregion //AddStringsGroup

		#region AddImagesGroup

		internal static void AddImagesGroup(Type owningType, DesignerActionItemCollection items, bool isExpanded, ref int groupSequence)
		{
			DesignerActionItemGroup groupImages = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Images"), groupSequence++);
			groupImages.IsExpanded				= isExpanded;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupImages))
			{
				items.Add(pic.GetPropertyActionItem("LargeImage"));
				items.Add(pic.GetPropertyActionItem("SmallImage"));
			}
		}

		#endregion //AddImagesGroup

		#region AddQatGroup

		internal static DesignerActionItemGroup AddQatGroup(Type owningType, DesignerActionItemCollection items, bool isExpanded, ref int groupSequence)
		{
			DesignerActionItemGroup groupQat	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Qat"), groupSequence++);
			groupQat.IsExpanded					= isExpanded;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupQat))
			{
				items.Add(pic.GetPropertyActionItem("IsQatCommonTool"));
			}

			return groupQat;
		}

		#endregion //AddQatGroup

		#region AddButtonGroup

		internal static DesignerActionItemGroup AddButtonGroup(Type owningType, DesignerActionItemCollection items, bool isExpanded, ref int groupSequence)
		{
			DesignerActionItemGroup groupButton = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Button"), groupSequence++);
			groupButton.IsExpanded				= isExpanded;
			using (PropertyItemCreator pic = new PropertyItemCreator(owningType, groupButton))
			{
				items.Add(pic.GetPropertyActionItem("ClickMode"));
			}

			return groupButton;
		}

		#endregion //AddButtonGroup

		#region InitializeDALForEditorTool

		internal static void InitializeDALForEditorTool(Type owningType, DesignerActionList actionList, DesignerActionItemCollection items, int groupSequence)
		{
			#region Tasks Group

			//DesignerActionItemGroup groupTasks = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Tasks"), groupSequence++);
			//groupTasks.IsExpanded = true;
			//this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddButtonTool", SR.GetString("SmartTag_D_AddButtonTool"), SR.GetString("SmartTag_N_AddButtonTool"), groupAddTools, groupSequence++));

			#endregion //AddTools Group

			#region Strings Group

			DALHelpers.AddStringsGroup(owningType, items, false, ref groupSequence);

			#endregion //Strings Group

			#region Images Group

			DALHelpers.AddImagesGroup(owningType, items, false, ref groupSequence);

			#endregion //Images Group

			#region QAT Group

			DesignerActionItemGroup groupQat = DALHelpers.AddQatGroup(owningType, items, false, ref groupSequence);

			items.Add(new DesignerActionMethodItem(actionList, "PerformAction_AddToQat", SR.GetString("SmartTag_D_AddToQat"), SR.GetString("SmartTag_N_AddToQat"), groupQat, groupSequence++));

			#endregion //Qat Group
		}

		#endregion //InitializeDALForEditorTool

		#region AddItemToQat

		internal static void AddItemToQat(EditingContext context, ModelItem adornedControlModel)
		{
			// Get the Item's Id
			string toolId = (string)(adornedControlModel.Properties["Id"].ComputedValue);
			if (string.IsNullOrEmpty(toolId))
				return;

			// If the item is aleady on the QAT, exit.
			if (DALHelpers.IsIdOnQat(adornedControlModel, toolId))
			{
				string messageBody = adornedControlModel.ItemType == typeof(RibbonGroup) ? SR.GetString("SmartTag_MessageBody_RibbonGroupAlreadyOnQat") :
																						   SR.GetString("SmartTag_MessageBody_ToolAlreadyOnQat");
				MessageBox.Show(messageBody, SR.GetString("SmartTag_MessageTitle_QatError"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}


			// Set the item's Id property explicitly so it is persisted.
			adornedControlModel.Properties["Id"].SetValue(toolId);


			// Create a QatPlaceholderTool and set its TargetId and TargetType
			ModelItem qatPlaceholderTool = ModelFactory.CreateItem(context, typeof(QatPlaceholderTool), null);
			if (qatPlaceholderTool == null)
				return;
			
			qatPlaceholderTool.Properties["TargetId"].SetValue(toolId);

			if (adornedControlModel.ItemType == typeof(RibbonGroup))
				qatPlaceholderTool.Properties["TargetType"].SetValue(QatPlaceholderToolType.RibbonGroup);


			// If the Qat hasn't been created yet, create on now.
			ModelItem ribbonModelItem = DALHelpers.GetRibbonModelItem(adornedControlModel);
			if (ribbonModelItem == null)
				return;

			ModelItem qatModelItem = ribbonModelItem.Properties["QuickAccessToolbar"].Value;
			if (qatModelItem == null)
			{
				qatModelItem = ModelFactory.CreateItem(context, typeof(QuickAccessToolbar), null);
				if (qatModelItem != null)
					ribbonModelItem.Properties["QuickAccessToolbar"].SetValue(qatModelItem);
				else
					return;
			}


			ModelProperty itemsModelProperty = qatModelItem.Properties["Items"];
			itemsModelProperty.Collection.Add(qatPlaceholderTool);
		}

		#endregion //AddItemToQat

		#region IsIdOnQat

		internal static bool IsIdOnQat(ModelItem adornedControlModel, string id)
		{
			// Get the Ribbon's ModelItem then get the Qat's ModelItem
			ModelItem ribbonModelItem = DALHelpers.GetRibbonModelItem(adornedControlModel);
			if (ribbonModelItem == null)
				return false;

			ModelItem qatModelItem = ribbonModelItem.Properties["QuickAccessToolbar"].Value;
			if (qatModelItem != null)
			{
				ModelProperty items = qatModelItem.Properties["Items"];
				foreach (ModelItem item in items.Collection)
				{
					if (item != null)
					{
						string placeholderTargetId = (string)(item.Properties["TargetId"].ComputedValue);
						if (placeholderTargetId == id)
							return true;
					}
				}
			}

			return false;
		}

		#endregion //IsIdOnQat

		#region GetRibbonModelItem

		private static ModelItem GetRibbonModelItem(ModelItem adornedControlModel)
		{
			ModelItem ribbonModelItem = adornedControlModel.Parent;
			while (ribbonModelItem != null && ribbonModelItem.ItemType != typeof(XamRibbon))
				ribbonModelItem = ribbonModelItem.Parent;

			return ribbonModelItem;
		}

		#endregion //GetRibbonModelItem
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