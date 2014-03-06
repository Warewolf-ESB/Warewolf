using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Model;
using Infragistics.Windows.DockManager;
using System;
using Infragistics.Collections;

namespace Infragistics.Windows.Design.DockManager
{
	internal static class DALHelpers
	{
		#region AddSplitPane

		internal static ModelItem AddSplitPane(EditingContext context, ModelItem adornedControlModel)
		{
			ModelItem panesModelItem = adornedControlModel.Properties["Panes"].Value;
			if (panesModelItem == null || panesModelItem.ItemType != typeof(ObservableCollectionExtended<SplitPane>))
			{
				// Create a ModelItem & object instance for the property.
				panesModelItem = ModelFactory.CreateItem(context, typeof(ObservableCollectionExtended<SplitPane>), null);

				// Set the value of the property to the newly create ModelItem
				adornedControlModel.Properties["Panes"].SetValue(panesModelItem);
			}

			// Create a new SplitPane and add it to the Panes collection
			ModelItem		newSplitPaneItem	= ModelFactory.CreateItem(context, typeof(SplitPane), null);

			// Add the new SplitPane to the Panes collection
			ModelProperty	panesModelProperty	= adornedControlModel.Properties["Panes"];
			panesModelProperty.Collection.Add(newSplitPaneItem);

			return newSplitPaneItem;
		}

		#endregion //AddSplitPane

		#region CreateTabGroupPaneWithTwoContentPanes

		internal static ModelItem CreateTabGroupPaneWithTwoContentPanes(EditingContext context)
		{
			ModelItem		tabGroupPaneItem				= ModelFactory.CreateItem(context, typeof(TabGroupPane), null);
			ModelProperty	tabGroupPaneItemsModelProperty	= tabGroupPaneItem.Properties["Items"];

			ModelItem		contentPaneItem					= ModelFactory.CreateItem(context, typeof(ContentPane), null);
			contentPaneItem.Properties["Header"].SetValue(SR.GetString("SmartTag_Default_ContentPaneHeader"));
			tabGroupPaneItemsModelProperty.Collection.Add(contentPaneItem);

			contentPaneItem									= ModelFactory.CreateItem(context, typeof(ContentPane), null);
			contentPaneItem.Properties["Header"].SetValue(SR.GetString("SmartTag_Default_ContentPaneHeader"));
			tabGroupPaneItemsModelProperty.Collection.Add(contentPaneItem);

			return tabGroupPaneItem;
		}

		#endregion //CreateTabGroupPaneWithTwoContentPanes

		#region ShowSplitPaneOptionsDialog

		internal static void ShowSplitPaneOptionsDialog(ModelItem splitPane, EditingContext context, Type paneTypeToAdd)
		{
			SplitPaneOptionsDialog dialog	= new SplitPaneOptionsDialog(splitPane, context, paneTypeToAdd);
			dialog.Title					= SR.GetString("SmartTag_T_SplitPaneOptionsDialog");
			dialog.ShowDialog();
		}

		#endregion //ShowSplitPaneOptionsDialog
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