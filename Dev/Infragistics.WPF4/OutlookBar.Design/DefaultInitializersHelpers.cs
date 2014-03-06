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
using Infragistics.Windows.OutlookBar;

namespace Infragistics.Windows.Design.OutlookBar
{
	internal static class DefaultInitializersHelpers
	{
		// NOTE: This static method is also called from pre-VS2010 designers.  Care should be taken when modifying
		// this routine to ensure that no .NET 4.0 code is called.  If 4.0 code is required, this routine may
		// need to be refactored.
		internal static void InitializeDefaultsXamOutlookBar(ModelItem item, EditingContext editingContext)
		{
			Debug.Assert(item != null, "item is null!");
			Debug.Assert(editingContext != null, "item is null!");

			DesigntimeUtilities.SetPropValueHelper(item.Properties["Margin"], new Thickness(0));
			DesigntimeUtilities.SetPropValueHelper(item.Properties["HorizontalAlignment"], HorizontalAlignment.Stretch);
			Size size = new Size(144, 280);
			DesigntimeUtilities.SetPropValueHelper(item.Properties["Width"], size.Width);
			DesigntimeUtilities.SetPropValueHelper(item.Properties["Height"], size.Height);

			int nmbGroupsInDesignMode = 2;
			for (int i = 0; i < nmbGroupsInDesignMode; i++)
			{
				// JM 03-12-09 TFS15178
				//string grHeader = "Group " + (i + 1);
				string grHeader = SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_AddGroupWithPredefinedContent", (i + 1));

				// JM 10-22-08
				//item.Properties["Groups"].Collection.Add(new OutlookBarGroup());
				ModelItem groupModelItem = ModelFactory.CreateItem(editingContext, new OutlookBarGroup());
				item.Content.Collection.Add(groupModelItem);

				// JM 10-22-08
				//item.Properties["Groups"].Collection[i].Content.SetValue(new Grid());
				ModelItem gridModelItem = ModelFactory.CreateItem(editingContext, new Grid());
				gridModelItem.Properties["Height"].ClearValue();
				gridModelItem.Properties["Width"].ClearValue();
				gridModelItem.Properties["Name"].ClearValue();
				// JM 10-22-08
				//item.Properties["Groups"].Collection[i].Content.SetValue(gridModelItem);
				groupModelItem.Content.SetValue(gridModelItem);

				// JM 10-22-08
				//item.Properties["Groups"].Collection[i].Properties["Header"].SetValue(grHeader);
				groupModelItem.Properties["Header"].SetValue(grHeader);

				// JM 09-10-09 TFS21125
				if (i == 0)
					groupModelItem.Properties["IsSelected"].SetValue(KnownBoxes.TrueBox);
			}
		}

		internal static void InitializeDefaultsOutlookBarGroup(ModelItem item, EditingContext editingContext)
		{
			Debug.Assert(item != null, "item is null!");
			Debug.Assert(editingContext != null, "item is null!");

			// JM 10-22-08
			//item.Content.SetValue(new Grid());
			ModelItem gridModelItem = ModelFactory.CreateItem(editingContext, new Grid());
			gridModelItem.Properties["Height"].ClearValue();
			gridModelItem.Properties["Width"].ClearValue();
			gridModelItem.Properties["Name"].ClearValue();
			item.Content.SetValue(gridModelItem);
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