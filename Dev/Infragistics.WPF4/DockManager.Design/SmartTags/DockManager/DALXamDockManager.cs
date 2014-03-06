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
using System.Collections.ObjectModel;
using Infragistics.Windows.DockManager;
using Infragistics.Collections;
using System;

namespace Infragistics.Windows.Design.DockManager
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
    public class DALXamDockManager : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALXamDockManager(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_XamDockManager") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region Tasks Group

			DesignerActionItemGroup groupTasks	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Tasks"), groupSequence++);
			groupTasks.IsExpanded				= true;
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddSplitPaneWithContentPanes", SR.GetString("SmartTag_D_AddSplitPaneWithContentPanes"), SR.GetString("SmartTag_N_AddSplitPaneWithContentPanes"), groupTasks, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddSplitPaneWithTabGroupPanes", SR.GetString("SmartTag_D_AddSplitPaneWithTabGroupPanes"), SR.GetString("SmartTag_N_AddSplitPaneWithTabGroupPanes"), groupTasks, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddDocumentContentHost", SR.GetString("SmartTag_D_AddDocumentContentHost"), SR.GetString("SmartTag_N_AddDocumentContentHost"), groupTasks, groupSequence++));

			#endregion //Tasks Group

			#region Appearance Group

			DesignerActionItemGroup groupAppearance = new DesignerActionItemGroup(SR.GetString("SmartTag_G_Appearance"), groupSequence++);
			groupAppearance.IsExpanded				= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(XamDockManager), groupAppearance))
			{
				this.Items.Add(pic.GetPropertyActionItem("Theme"));
			}

			#endregion //Appearance Group

			#region Options Group

			DesignerActionItemGroup groupOptions	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Options"), groupSequence++);
			groupOptions.IsExpanded					= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(XamDockManager), groupOptions))
			{
				this.Items.Add(pic.GetPropertyActionItem("CloseBehavior"));
				this.Items.Add(pic.GetPropertyActionItem("FloatingWindowDragMode"));
				this.Items.Add(pic.GetPropertyActionItem("FlyoutAnimation"));
				this.Items.Add(pic.GetPropertyActionItem("LayoutMode"));
				this.Items.Add(pic.GetPropertyActionItem("NavigationOrder"));
				this.Items.Add(pic.GetPropertyActionItem("PaneNavigatorButtonDisplayMode"));
				this.Items.Add(pic.GetPropertyActionItem("PinBehavior"));
				this.Items.Add(pic.GetPropertyActionItem("TabItemDragBehavior"));
				this.Items.Add(pic.GetPropertyActionItem("UnpinnedTabHoverAction"));
			}

			#endregion //Options Group
		}

        #endregion //Constructors

		#region DesignerActionMethodItem Callbacks

			#region PerformAction_AddDocumentContentHost

		private static void PerformAction_AddDocumentContentHost(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			ModelItem dchModelItem = adornedControlModel.Properties["Content"].Value;
			if (dchModelItem != null)
			{
				if (dchModelItem.ItemType == typeof(DocumentContentHost))
				{
					MessageBoxResult result = MessageBox.Show(SR.GetString("SmartTag_M_DocumentContentHostAlreadyExists"),
															  SR.GetString("SmartTag_T_TaskError"), 
															  MessageBoxButton.YesNo, 
															  MessageBoxImage.Exclamation);

					if (result == MessageBoxResult.No)
						return;
				}
				else
				{
					string message = string.Format(SR.GetString("SmartTag_M_DockManagerContentAlreadySet"), dchModelItem.ItemType.ToString());
					MessageBoxResult result = MessageBox.Show(message,
															  SR.GetString("SmartTag_T_TaskError"),
															  MessageBoxButton.YesNo,
															  MessageBoxImage.Exclamation);
					if (result == MessageBoxResult.No)
						return;
				}
			}

			// Create a dch ModelItem.
			dchModelItem = ModelFactory.CreateItem(context, typeof(DocumentContentHost), null);

			// Add a SplitPane to the dch
			ModelItem		newSplitPaneItem	= DALHelpers.AddSplitPane(context, dchModelItem);
			ModelProperty	panesModelProperty	= newSplitPaneItem.Properties["Panes"];

			// Add a TabGroupPane with 2 content panes to the SplitPane.
			panesModelProperty.Collection.Add(DALHelpers.CreateTabGroupPaneWithTwoContentPanes(context));

			// Set the XDM's Content to the dch ModelItem
			adornedControlModel.Properties["Content"].SetValue(dchModelItem);
		}

			#endregion //PerformAction_AddDocumentContentHost

			#region PerformAction_AddSplitPaneWithContentPanes

		private static void PerformAction_AddSplitPaneWithContentPanes(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			// Create a new SplitPane and display the SPlitPaneOptionsDialog to gather options and add ContentPanes to the SplitPane
			ModelItem newSplitPaneItem = DALHelpers.AddSplitPane(context, adornedControlModel);

			DALHelpers.ShowSplitPaneOptionsDialog(newSplitPaneItem, context, typeof(ContentPane));
		}

			#endregion //PerformAction_AddSplitPaneWithContentPanes

			#region PerformAction_AddSplitPaneWithTabGroupPanes

		private static void PerformAction_AddSplitPaneWithTabGroupPanes(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			// Create a new SplitPane and display the SPlitPaneOptionsDialog to gather options and add TabGroupPanes to the SplitPane
			ModelItem newSplitPaneItem = DALHelpers.AddSplitPane(context, adornedControlModel);

			DALHelpers.ShowSplitPaneOptionsDialog(newSplitPaneItem, context, typeof(TabGroupPane));
		}

			#endregion //PerformAction_AddSplitPaneWithTabGroupPanes

		#endregion //DesignerActionMethodItem Callbacks
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