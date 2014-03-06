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
using System.Windows.Media;

namespace Infragistics.Windows.Design.DockManager
{
    /// <summary>
    /// A predefined DesignerActionList class. Here you can specify the smart tag items 
    /// and/or methods which will be executed from the smart tag.
    /// </summary>
    public class DALTabGroupPane : DesignerActionList
    {
        #region Constructors

		/// <summary>
		/// Creates an instance of a DesignerActionList
		/// </summary>
		/// <param name="context"></param>
		/// <param name="modelItem"></param>
		/// <param name="itemsToShow"></param>
		/// <param name="alternateAdornerTitle"></param>
		public DALTabGroupPane(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
			: base(context, modelItem, itemsToShow, alternateAdornerTitle)
        {
			//You can set the title of the smart tag. By default it is "Tasks".
			this.GenericAdornerTitle = string.IsNullOrEmpty(alternateAdornerTitle) ? SR.GetString("SmartTag_T_TabGroupPane") : alternateAdornerTitle;

			//You can specify the MaxHeight of the smart tag. The smart tag has a scrolling capability.
			//this.GenericAdornerMaxHeight = 300;

			int groupSequence = 0;

			#region Tasks Group

			DesignerActionItemGroup groupTasks	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Tasks"), groupSequence++);
			groupTasks.IsExpanded				= true;
			this.Items.Add(new DesignerActionTextItem(SR.GetString("SmartTag_N_Selection"), groupTasks, FontWeights.Bold, Brushes.DarkBlue, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_SelectNextTab", SR.GetString("SmartTag_D_SelectNextTab"), SR.GetString("SmartTag_N_SelectNextTab"), groupTasks, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_SelectPreviousTab", SR.GetString("SmartTag_D_SelectPreviousTab"), SR.GetString("SmartTag_N_SelectPreviousTab"), groupTasks, groupSequence++));

			this.Items.Add(new DesignerActionTextItem(SR.GetString("SmartTag_N_AddPanes"), groupTasks, FontWeights.Bold, Brushes.DarkBlue, groupSequence++));
			this.Items.Add(new DesignerActionMethodItem(this, "PerformAction_AddContentPane", SR.GetString("SmartTag_D_AddContentPane"), SR.GetString("SmartTag_N_AddContentPane"), groupTasks, groupSequence++));

			#endregion //Tasks Group

			#region Appearances Group

			DesignerActionItemGroup groupAppearances	= new DesignerActionItemGroup(SR.GetString("SmartTag_G_Appearance"), groupSequence++);
			groupAppearances.IsExpanded					= true;
			using (PropertyItemCreator pic = new PropertyItemCreator(typeof(TabGroupPane), groupAppearances))
			{
				this.Items.Add(pic.GetPropertyActionItem("IsEnabled"));
				this.Items.Add(pic.GetPropertyActionItem("TabStripPlacement"));
			}

			#endregion //Appearances Group
		}

        #endregion //Constructors

		#region DesignerActionMethodItem Callbacks

			#region PerformAction_AddContentPane

		private static void PerformAction_AddContentPane(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			ModelProperty	tabGroupPaneItemsModelProperty	= adornedControlModel.Properties["Items"];

			ModelItem		contentPaneItem					= ModelFactory.CreateItem(context, typeof(ContentPane), null);
			contentPaneItem.Properties["Header"].SetValue(SR.GetString("SmartTag_Default_ContentPaneHeader"));
			tabGroupPaneItemsModelProperty.Collection.Add(contentPaneItem);
		}

			#endregion //PerformAction_AddContentPane

			#region PerformAction_SelectNextTab

		private static void PerformAction_SelectNextTab(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			DALTabGroupPane.ChangeSelectedIndex(adornedControlModel, true);
		}

			#endregion //PerformAction_SelectNextTab

			#region PerformAction_SelectPreviousTab

		private static void PerformAction_SelectPreviousTab(EditingContext context, ModelItem adornedControlModel, DesignerActionList designerActionList)
		{
			DALTabGroupPane.ChangeSelectedIndex(adornedControlModel, false);
		}

			#endregion //PerformAction_SelectPreviousTab

		#endregion //DesignerActionMethodItem Callbacks

		#region Methods

			#region SetSelectedIndex

		private static void ChangeSelectedIndex(ModelItem adornedControlModel, bool next)
		{
			int			selectedIndex	= 0;
			ModelItem	itemsModelItem	= adornedControlModel.Properties["Items"].Value;
			int			itemCount		= (int)(itemsModelItem.Properties["Count"].ComputedValue);

			if (adornedControlModel.View != null)
			{
				TabGroupPane tgp = adornedControlModel.View.PlatformObject as TabGroupPane;
				if (tgp != null)
					selectedIndex = tgp.SelectedIndex;
			}
			else
			{
				selectedIndex = (int)(adornedControlModel.Properties["SelectedIndex"].ComputedValue);
			}

			if (next)
			{
				if (selectedIndex < itemCount - 1)
					selectedIndex++;
			}
			else
			{
				if (selectedIndex > 0)
					selectedIndex--;
			}
	
			// Try to set the property without going through the ModelItem so it does not get persisted to XAML
			if (adornedControlModel.View != null)
			{
				TabGroupPane tgp = adornedControlModel.View.PlatformObject as TabGroupPane;
				if (tgp != null)
					tgp.SelectedIndex = selectedIndex;
			}
			else
				adornedControlModel.Properties["SelectedIndex"].SetValue(selectedIndex);
		}

			#endregion //SetSelectedIndex

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