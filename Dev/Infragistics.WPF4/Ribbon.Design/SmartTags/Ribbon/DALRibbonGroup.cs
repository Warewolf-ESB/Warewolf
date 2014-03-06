using System.Text;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design.Services;
using Infragistics.Shared;
using System.Collections.Generic;
using Infragistics.Windows.Ribbon;
using System.Collections.ObjectModel;
using System;
using System.Windows.Media;

namespace Infragistics.Windows.Design.Ribbon
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
    public class DALRibbonGroup : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALRibbonGroup(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_RibbonGroup") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region AddTools Group

			DesignerActionItemGroup groupAddTools = new DesignerActionItemGroup(SR.GetString("SmartTag_G_AddTools"), groupSequence++);
			groupAddTools.IsExpanded = true;
			this.Items.Add(new DesignerActionTextItem("Button Tools", groupAddTools, FontWeights.Bold, Brushes.DarkBlue, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddButtonTool"), SR.GetString("SmartTag_N_AddButtonTool"), groupAddTools, groupSequence++, new object[] { typeof(ButtonTool), SR.GetString("SmartTag_Default_ButtonToolCaption") }));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddCheckBoxTool"), SR.GetString("SmartTag_N_AddCheckBoxTool"), groupAddTools, groupSequence++, new object[] { typeof(CheckBoxTool), SR.GetString("SmartTag_Default_CheckBoxToolCaption") }));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddRadioButtonTool"), SR.GetString("SmartTag_N_AddRadioButtonTool"), groupAddTools, groupSequence++, new object[] { typeof(RadioButtonTool), SR.GetString("SmartTag_Default_RadioButtonToolCaption") }));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddToggleButtonTool"), SR.GetString("SmartTag_N_AddToggleButtonTool"), groupAddTools, groupSequence++, new object[] { typeof(ToggleButtonTool), SR.GetString("SmartTag_Default_ToggleButtonToolCaption") }));

			this.Items.Add(new DesignerActionTextItem("Editor Tools", groupAddTools, FontWeights.Bold, Brushes.DarkBlue, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddComboEditorTool"), SR.GetString("SmartTag_N_AddComboEditorTool"), groupAddTools, groupSequence++, new object[] { typeof(ComboEditorTool), SR.GetString("SmartTag_Default_ComboEditorToolCaption") }));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddMaskedEditorTool"), SR.GetString("SmartTag_N_AddMaskedEditorTool"), groupAddTools, groupSequence++, new object[] { typeof(MaskedEditorTool), SR.GetString("SmartTag_Default_MaskedEditorToolCaption") }));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddTextEditorTool"), SR.GetString("SmartTag_N_AddTextEditorTool"), groupAddTools, groupSequence++, new object[] { typeof(TextEditorTool), SR.GetString("SmartTag_Default_TextEditorToolCaption") }));

			this.Items.Add(new DesignerActionTextItem("Menu Tools", groupAddTools, FontWeights.Bold, Brushes.DarkBlue, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddMenuTool"), SR.GetString("SmartTag_N_AddMenuTool"), groupAddTools, groupSequence++, new object[] { typeof(MenuTool), SR.GetString("SmartTag_Default_MenuToolCaption") }));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddSeparatorTool"), SR.GetString("SmartTag_N_AddSeparatorTool"), groupAddTools, groupSequence++, new object[] { typeof(SeparatorTool), string.Empty }));

			this.Items.Add(new DesignerActionTextItem("Other Tools", groupAddTools, FontWeights.Bold, Brushes.DarkBlue, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddTool", SR.GetString("SmartTag_D_AddLabelTool"), SR.GetString("SmartTag_N_AddLabelTool"), groupAddTools, groupSequence++, new object[] { typeof(LabelTool), SR.GetString("SmartTag_Default_LabelToolCaption") }));

			#endregion //AddTools Group

			#region ToolArrangement Group

			DesignerActionItemGroup groupToolArrangement	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_ToolArrangement"), groupSequence++);
			groupToolArrangement.IsExpanded					= false;
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_ArrangeToolsHorizontally", SR.GetString("SmartTag_D_ArrangeToolsHorizontally"), SR.GetString("SmartTag_N_ArrangeToolsHorizontally"), groupToolArrangement, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_ArrangeToolsVertically", SR.GetString("SmartTag_D_ArrangeToolsVertically"), SR.GetString("SmartTag_N_ArrangeToolsVertically"), groupToolArrangement, groupSequence++));

			#endregion //ToolArrangement Group

			#region Strings Group

			DesignerActionItemGroup groupStrings = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Strings"), groupSequence++);
			groupStrings.IsExpanded = false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(RibbonGroup), groupStrings))
			{
				this.Items.Add(pic.GetPropertyActionItem("Caption"));
				this.Items.Add(pic.GetPropertyActionItem("Id"));
				this.Items.Add(pic.GetPropertyActionItem("KeyTip"));
			}

			#endregion //Strings Group

			#region Images Group

			DesignerActionItemGroup groupImages = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Images"), groupSequence++);
			groupImages.IsExpanded = false;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(ButtonTool), groupImages))
			{
				this.Items.Add(pic.GetPropertyActionItem("SmallImage"));
			}

			#endregion //Images Group

			#region QAT Group

			DesignerActionItemGroup groupQat = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Qat"), groupSequence++);
			groupQat.IsExpanded = false;

			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddToQat", SR.GetString("SmartTag_D_AddToQat"), SR.GetString("SmartTag_N_AddToQat"), groupQat, groupSequence++));

			#endregion //Images Group
		}

        #endregion //Constructors

		#region DesignerActionMethodItem Callbacks

			#region PerformAction_AddTool

		private static void PerformAction_AddTool(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList, object [] parameters)
		{
			if (parameters == null || parameters.Length < 1)
				return;

			string caption = parameters.Length > 1 ? (string)parameters[1] : string.Empty;
			DALRibbonGroup.AddToolHelper(context, adornedControlModel, (Type)parameters[0], caption);
		}

			#endregion //PerformAction_AddTool

			#region PerformAction_AddToQat

		private static void PerformAction_AddToQat(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			DALHelpers.AddItemToQat(context, adornedControlModel);
		}

			#endregion //PerformAction_AddToQat

			#region PerformAction_ArrangeToolsHorizontally

		private static void PerformAction_ArrangeToolsHorizontally(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			DALRibbonGroup.SetItemsPanelPropertyHelper(context, adornedControlModel, typeof(ToolHorizontalWrapPanel));
		}

			#endregion //PerformAction_ArrangeToolsHorizontally

			#region PerformAction_ArrangeToolsVertically

		private static void PerformAction_ArrangeToolsVertically(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			DALRibbonGroup.SetItemsPanelPropertyHelper(context, adornedControlModel, typeof(ToolVerticalWrapPanel));
		}

			#endregion //PerformAction_ArrangeToolsVertically

		#endregion //DesignerActionMethodItem Callbacks

		#region Methods

			#region AddToolHelper

		private static void AddToolHelper(EditingContext context, ModelItem adornedControlModel, Type toolType, string caption)
		{
			// Create a new tool of the requested type
			ModelItem newToolItem = ModelFactory.CreateItem(context, toolType);

			if (false == string.IsNullOrEmpty(caption))
				newToolItem.Properties["Caption"].SetValue(caption);

			// Add the new tool to the RibbonGroup 
			ModelProperty itemsModelProperty = adornedControlModel.Properties["Items"];
			itemsModelProperty.Collection.Add(newToolItem);
		}

			#endregion //AddToolHelper

			#region SetItemsPanelPropertyHelper

		private static void SetItemsPanelPropertyHelper(EditingContext context, ModelItem adornedControlModel, Type itemsPanelType)
		{
			ModelItem itemsPanelTemplateModelItem = ModelFactory.CreateItem(context, typeof(ItemsPanelTemplate));
			adornedControlModel.Properties["ItemsPanel"].SetValue(itemsPanelTemplateModelItem);

			itemsPanelTemplateModelItem.Properties["VisualTree"].SetValue(ModelFactory.CreateItem(context, itemsPanelType));
		}

			#endregion //SetItemsPanelPropertyHelper

		#endregion //Methods
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