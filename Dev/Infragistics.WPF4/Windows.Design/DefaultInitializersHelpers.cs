using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design;
using Infragistics.Windows.Controls;
using Infragistics.Shared;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using Microsoft.Windows.Design.Services;

namespace Infragistics.Windows.Design
{
	internal static class DefaultInitializersHelpers
	{
		// NOTE: This static method is also called from pre-VS2010 designers.  Care should be taken when modifying
		// this routine to ensure that no .NET 4.0 code is called.  If 4.0 code is required, this routine may
		// need to be refactored.
		internal static void InitializeDefaultsXamTabControl(ModelItem item, EditingContext editingContext, DependencyObject namescopeRootElement)
		{
			Debug.Assert(item != null, "item is null!");
			Debug.Assert(editingContext != null, "item is null!");

			if (item != null)
			{
				Size size = new Size(200, 100);
				DesigntimeUtilities.SetPropValueHelper(item.Properties["Width"], size.Width);
				DesigntimeUtilities.SetPropValueHelper(item.Properties["Height"], size.Height);

				// Remove any TabItems added by the TabControl DefaultInitializer.
				item.Properties["Items"].Collection.Clear();
			}

            if (item					!= null && 
				editingContext			!= null)
            {
                // Create and add a single TabItemEx
                ModelItem tabItemExModelItem = ModelFactory.CreateItem(editingContext, new TabItemEx());

				DefaultInitializersHelpers.InitializeTabItemEx(editingContext, item, tabItemExModelItem, namescopeRootElement);

                item.Content.Collection.Add(tabItemExModelItem);
            }
		}

		internal static void InitializeTabItemEx(EditingContext editingContext, ModelItem tabControlModelItem, ModelItem tabItemExModelItem, DependencyObject namescopeRootElement)
		{
			string tabItemName = DesigntimeUtilities.GetNameSafe(tabControlModelItem,
																  "TabItemEx_DefaultHeader",
																  tabControlModelItem.Properties["Items"].Collection.Count + 1,
																  namescopeRootElement);

			if (string.IsNullOrEmpty(tabItemName))
			{
				if (tabItemExModelItem.Properties["Header"].Value == null)
					tabItemExModelItem.Properties["Header"].SetValue(SR.GetString("TabItemEx_DefaultHeader", ""));
			}
			else
			{
				tabItemExModelItem.Properties["Header"].SetValue(tabItemName);
				tabItemExModelItem.Properties["Name"].SetValue(tabItemName);
			}


			// Add a Grid element as the Content of the TabItemEx we just created.
			ModelItem gridModelItem = ModelFactory.CreateItem(editingContext, new Grid());
			gridModelItem.Properties["Height"].ClearValue();
			gridModelItem.Properties["Width"].ClearValue();
			gridModelItem.Properties["Name"].ClearValue();
			tabItemExModelItem.Content.SetValue(gridModelItem);
		}
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