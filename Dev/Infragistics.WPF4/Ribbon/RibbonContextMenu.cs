using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Ribbon.Internal;
using System.Collections.Specialized;
using Infragistics.Windows.Helpers;
using Infragistics.Shared;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A context menu that can display XamRibbon tools with Office2007 styling.
	/// </summary>
	/// <seealso cref="XamRibbon"/>
	//[ToolboxItem(false)]	// JM BR28204 11-06-07 - added this here for documentation but commented out and added ToolboxBrowsableAttribute directly to DesignMetadata for the XamRibbon assembly.
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RibbonContextMenu : ContextMenu,
									 IRibbonToolLocation
	{
		#region Constructor

		static RibbonContextMenu()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonContextMenu), new FrameworkPropertyMetadata(typeof(RibbonContextMenu)));

			// JJD 10/12/07
			// Default the IsSharedSizeScopeProperty to true
			Grid.IsSharedSizeScopeProperty.OverrideMetadata(typeof(RibbonContextMenu), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RibbonContextMenu"/> class.
		/// </summary>
		public RibbonContextMenu()
		{
		}

		#endregion //Constructor	
    
		#region Base Class Overrides

			#region ClearContainerForItemOverride

		/// <summary>
		/// Undoes the effects of PrepareContainerForItemOverride.
		/// </summary>
		/// <param name="element">The container element</param>
		/// <param name="item">The item.</param>
		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride(element, item);

			ToolMenuItem		menuItem	= element as ToolMenuItem;
			FrameworkElement	feItem		= item as FrameworkElement;

			if (menuItem != null && feItem != null)
			{
				IRibbonTool tool = item as IRibbonTool;

				if (tool != null)
				{
					RibbonToolProxy proxy = tool.ToolProxy;
					if (proxy == null)
						throw new InvalidOperationException(XamRibbon.GetString("LE_IRibbonToolProxyIsNull"));

					proxy.ClearToolMenuItem(menuItem, feItem);
				}
			}
		}

			#endregion //ClearContainerForItemOverride	
    
			#region GetContainerForItemOverride

		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <returns>The newly created container</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ToolMenuItem();
		}

			#endregion //GetContainerForItemOverride	
    
			#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the item requires a separate container.
		/// </summary>
		/// <param name="item">The item in question.</param>
		/// <returns>True if the item does not require a wrapper</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is ToolMenuItem;
		}

			#endregion //IsItemItsOwnContainerOverride	

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			MenuToolBase.VerifyTypeOfChildren(this);
		}

			#endregion //OnApplyTemplate	
    
			#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="e">Describes the items that changed.</param>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			if (this.IsInitialized)
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Reset:
						MenuToolBase.VerifyTypeOfChildren(this);
						break;
					case NotifyCollectionChangedAction.Add:
					case NotifyCollectionChangedAction.Replace:
						for (int i = e.NewStartingIndex; i < e.NewStartingIndex + e.NewItems.Count; i++)
							MenuToolBase.VerifyTypeOfChild(this.Items[i]);
						break;
				}
			}
		}

			#endregion //OnItemsChanged	

			#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			// AS 6/9/08 BR32242
			ToolMenuItem container = element as ToolMenuItem;

			if (null != container)
				container.SetValue(ToolMenuItem.ToolPropertyKey, item as IRibbonTool);

			base.PrepareContainerForItemOverride(element, item);


			// JM BR27366 10-17-07 - set the DataContext to null if a ToolMenuItem was added directly to the RibbonContextMenu and
			// the ToolMenuItem contains an IRibbonTool.  This will allow click processing to be handled by the base for all tool types
			// (see ToolMenuItem.OnClick)
			if (element == item				&& 
				(element is ToolMenuItem	&& ((ToolMenuItem)element).RibbonTool != null))
				((FrameworkElement)element).DataContext = null;


			ToolMenuItem		menuItem	= element as ToolMenuItem;
			FrameworkElement	feItem		= item as FrameworkElement;

			if (menuItem != null && feItem != null)
			{
				IRibbonTool tool = item as IRibbonTool;
				if (tool != null)
				{
					RibbonToolProxy proxy = tool.ToolProxy;

					if (proxy == null)
						throw new InvalidOperationException(XamRibbon.GetString("LE_IRibbonToolProxyIsNull"));


					proxy.PrepareToolMenuItem(menuItem, feItem);
				}

				// AS 10/12/07
				menuItem.SetValue(XamRibbon.LocationPropertyKey, RibbonKnownBoxes.ToolLocationMenuBox);
			}
		}

			#endregion //PrepareContainerForItemOverride	

		#endregion //Base Class Overrides

		#region IRibbonToolLocation Members

		ToolLocation IRibbonToolLocation.Location
		{
			get { return ToolLocation.Menu; }
		}

		#endregion
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