using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using Infragistics.Windows.Controls;
using System.Globalization;
using Infragistics.Shared;

namespace Infragistics.Windows.Design
{
	/// <summary>
	/// PrimarySelection ContentMenuProvider for the XamTabControl.
	/// </summary>
	public class XamTabControlContextMenuProvider : PrimarySelectionContextMenuProvider
	{
		#region Member Variables

		private MenuAction _addTabMenuAction;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of XamTabControlContextMenuProvider
		/// </summary>
		public XamTabControlContextMenuProvider()
		{
			base.UpdateItemStatus			+= new EventHandler<MenuActionEventArgs>(this.TabControlContextMenuProvider_UpdateItemStatus);
			this._addTabMenuAction			= new MenuAction(SR.GetString("AddTabItemEx_ContextMenuDescription"));

			
//			this._addTabMenuAction.ImageUri = new Uri("pack://application:,,,/Infragistics.Windows.VisualStudio.Design;component/Images/AddTabItemEx.bmp", UriKind.Absolute);

			this._addTabMenuAction.Execute	+= new EventHandler<MenuActionEventArgs>(this.AddTabMenuItemExAction_Execute);
			base.Items.Add(this._addTabMenuAction);
		}

		#endregion //Constructor

		#region Methods

			#region AddTabMenuItemExAction_Execute

		private void AddTabMenuItemExAction_Execute(object sender, MenuActionEventArgs e)
		{
			Type		itemType			= e.Selection.PrimarySelection.ItemType;
			ModelItem	primarySelection	= null;

			if (typeof(XamTabControl).IsAssignableFrom(itemType))
				primarySelection = e.Selection.PrimarySelection;
			else if (typeof(TabItemEx).IsAssignableFrom(itemType))
			{
				ModelItem parent = e.Selection.PrimarySelection.Parent;
				if ((parent != null) && typeof(XamTabControl).IsAssignableFrom(parent.ItemType))
					primarySelection = parent;
			}

			if (primarySelection != null)
			{
				using (ModelEditingScope scope = primarySelection.BeginEdit(SR.GetString("AddTabItemEx_ContextMenuDescription")))
				{
					ModelItem newTabItemEx = ModelFactory.CreateItem(e.Context, typeof(TabItemEx), CreateOptions.InitializeDefaults, new object[0]);
					newTabItemEx.Properties["Height"].ClearValue();
					newTabItemEx.Properties["Width"].ClearValue();

					DefaultInitializersHelpers.InitializeTabItemEx(e.Context, primarySelection, newTabItemEx, null);

					primarySelection.Content.Collection.Add(newTabItemEx);

					Microsoft.Windows.Design.Interaction.Selection selection = new Microsoft.Windows.Design.Interaction.Selection(new ModelItem[] { newTabItemEx });
					e.Context.Items.SetValue(selection);
					scope.Complete();
				}
			}
		}

			#endregion //AddTabMenuItemExAction_Execute

			#region TabControlContextMenuProvider_UpdateItemStatus

		private void TabControlContextMenuProvider_UpdateItemStatus(object sender, MenuActionEventArgs e)
		{
			if (e.Selection.SelectionCount == 1)
			{
				ModelItem	primarySelection	= e.Selection.PrimarySelection;
				Type		itemType			= primarySelection.ItemType;

				if (typeof(TabItemEx).IsAssignableFrom(itemType))
				{
					ModelItem parent = primarySelection.Parent;
					this._addTabMenuAction.Visible = (parent != null) && typeof(XamTabControl).IsAssignableFrom(parent.ItemType);
				}
				else if (typeof(XamTabControl).IsAssignableFrom(itemType))
					this._addTabMenuAction.Visible = true;
				else
				{
					ModelItem selectedTabItemEx = primarySelection.Parent;
					if ((selectedTabItemEx != null) && typeof(TabItemEx).IsAssignableFrom(selectedTabItemEx.ItemType))
					{
						ModelItem selectedXamTabControl = (selectedTabItemEx != null) ? selectedTabItemEx.Parent : null;
						this._addTabMenuAction.Visible = (selectedXamTabControl != null) && typeof(XamTabControl).IsAssignableFrom(selectedXamTabControl.ItemType);
					}
				}
			}
			else
				this._addTabMenuAction.Visible = false;
		}

			#endregion TabControlContextMenuProvider_UpdateItemStatus

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