using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infragistics.Shared;

namespace Infragistics.Windows.Ribbon.Internal
{
	/// <summary>
	/// Exposes the key tip information and placement for ribbon tools.
	/// </summary>
	internal class ToolKeyTipProvider : IKeyTipProvider
	{
		#region Member Variables

		private FrameworkElement _tool;
		private RibbonToolProxy _proxy;
		private ToolMenuItem _menuItem;

		#endregion //Member Variables

		#region Constructor
		internal ToolKeyTipProvider(FrameworkElement tool, ToolMenuItem menuItem)
		{
			IRibbonTool irt = tool as IRibbonTool;

			if (null == irt)
				throw new InvalidOperationException("The tool must be an 'IRibbonTool'.");

			this._tool = tool;
			this._proxy = irt.ToolProxy;
			this._menuItem = menuItem;

			if (this._proxy == null)
				throw new InvalidOperationException(XamRibbon.GetString("LE_IRibbonToolProxyIsNull"));
		}
		#endregion //Constructor

		#region Base class overrides

		#region ToString
		public override string ToString()
		{
			return this._tool.ToString();
		}
		#endregion //ToString 

		#endregion //Base class overrides

		#region Properties

		public FrameworkElement Tool
		{
			get { return this._tool; }
		}

		public ToolMenuItem MenuItem
		{
			get { return this._menuItem; }
		}

		protected bool IsOnQat
		{
			get { return XamRibbon.GetLocation(this._tool) == ToolLocation.QuickAccessToolbar; }
		}

		#endregion //Properties

		#region Methods

			#region GetToolAlignment
		internal static KeyTipAlignment GetToolAlignment(ToolLocation location, RibbonToolSizingMode sizingMode)
		{
			switch (location)
			{
				case ToolLocation.Ribbon:
					if (sizingMode == RibbonToolSizingMode.ImageAndTextLarge)
						return KeyTipAlignment.MiddleCenter;
					else
						return KeyTipAlignment.MiddleLeft;
				case ToolLocation.QuickAccessToolbar:
					return KeyTipAlignment.TopCenter;
				default:
				case ToolLocation.ApplicationMenu:
				case ToolLocation.ApplicationMenuFooterToolbar:
				case ToolLocation.ApplicationMenuRecentItems:
				case ToolLocation.Menu:
				case ToolLocation.ApplicationMenuSubMenu:
					return KeyTipAlignment.TopLeft;
			}
		}
			#endregion //GetToolAlignment

			#region GetToolKeyTipLocation
		internal static Point GetToolKeyTipLocation(FrameworkElement toolElement, ToolLocation location, RibbonToolSizingMode sizingMode)
		{
			double toolHeight = toolElement.ActualHeight;
			double toolWidth = toolElement.ActualWidth;
			double y, x;

			switch (location)
			{
				default:
				case ToolLocation.Menu:
				case ToolLocation.ApplicationMenu:
				case ToolLocation.ApplicationMenuRecentItems:
				case ToolLocation.ApplicationMenuFooterToolbar:
				case ToolLocation.ApplicationMenuSubMenu:
					// middle of the item
					y = toolHeight / 2;
					break;
				case ToolLocation.QuickAccessToolbar:
					// show the top 2/3 of the tool
					y = toolHeight * 2 / 3;
					break;
				case ToolLocation.Ribbon:
					// base y on row that tool should be on
					XamRibbon ribbon = XamRibbon.GetRibbon(toolElement);

					if (null != ribbon)
					{
						int row;

						if (sizingMode == RibbonToolSizingMode.ImageAndTextLarge)
							row = 2;
						else if (RibbonGroup.GetIsDialogBoxLauncherTool(toolElement))
							row = 3;
						else
						{
							// use a control point to find the closest "row"
							row = ribbon.GetRibbonGroupSmallToolRow(toolElement);
						}

						y = ribbon.GetRibbonGroupToolVerticalOffset(row, toolElement);
					}
					else
						y = toolHeight / 2;
					break;
			}

			// for large tools on a ribbon and the dialog box launcher the horizontal point is the mid point
			if (location == ToolLocation.Ribbon && (sizingMode == RibbonToolSizingMode.ImageAndTextLarge || RibbonGroup.GetIsDialogBoxLauncherTool(toolElement)))
				x = toolWidth / 2;
			else if (location == ToolLocation.ApplicationMenuFooterToolbar)
				x = toolWidth / 2;
			else
			{
				// see if there is a checkmark or image
				Rect rect = XamRibbon.GetKeyTipPlacementElementRect(KeyTipPlacementType.CheckIndicator, toolElement, toolElement);

				// otherwise see if there is an image
				if (rect.IsEmpty)
					rect = XamRibbon.GetKeyTipPlacementElementRect(KeyTipPlacementType.SmallImage, toolElement, toolElement);

				if (rect.IsEmpty)
					rect = new Rect(0, 0, toolElement.ActualWidth, toolElement.ActualHeight);

				// use the mid point of that
				x = (rect.Left + rect.Right) / 2;
			}

			return new Point(x, y);
		} 
			#endregion //GetToolKeyTipLocation

		#endregion //Methods

		#region IKeyTipProvider Members

		public virtual bool Activate()
		{
			// AS 10/22/07 BR27625
			// Added if check. Only bring the tool into view if it wants the parent container to remain open.
			//
			if (this.DeactivateParentContainersOnActivate == false)
				this._tool.BringIntoView();

			return this._proxy.PerformAction(this._tool);
		}

		public virtual bool Equals(IKeyTipProvider provider)
		{
			if (this == provider)
				return true;

			ToolKeyTipProvider toolProvider = provider as ToolKeyTipProvider;

			return toolProvider != null &&
				toolProvider._proxy == this._proxy &&
				toolProvider._tool == this._tool &&
				toolProvider._menuItem == this._menuItem;
		}

		public virtual IKeyTipContainer GetContainer()
		{
			// let the tool implement the interface if it is a keytip container
			return this._tool as IKeyTipContainer;
		}

		public virtual KeyTipAlignment Alignment
		{
			get
			{
				ToolLocation location = XamRibbon.GetLocation(this._tool);

				if (location == ToolLocation.Ribbon && RibbonGroup.GetIsDialogBoxLauncherTool(this._tool))
					return KeyTipAlignment.TopCenter;

				return GetToolAlignment(location, RibbonToolHelper.GetSizingMode(this._tool));
			}
		}

		public virtual string AutoGeneratePrefix
		{
			get { return null; }
		}

		public virtual string Caption
		{
			get { return RibbonToolHelper.GetCaption(this._tool); }
		}

		public virtual bool DeactivateParentContainersOnActivate
		{
			get { return this._proxy.RetainFocusOnPerformAction == false; }
		}

		public virtual bool IsEnabled
		{
			get 
			{
				// AS 10/25/07 BR27842
				// Also use the IsActivateable of the proxy.
				//
				if (null != this._proxy)
				{
					if (!this._proxy.IsActivateable(this._tool))
						return false;
				}

				return this._tool.IsEnabled || (this._menuItem != null && this._menuItem.IsEnabled); 
			}
		}

		public virtual bool IsVisible
		{
			get { return this._tool.IsVisible || (this._menuItem != null && this._menuItem.IsVisible); }
		}

		public virtual string KeyTipValue
		{
			get
			{
				// handle qat tools specially since the key tips start with numbers
				if (this.IsOnQat)
				{
					XamRibbon ribbon = XamRibbon.GetRibbon(this._tool);
					QuickAccessToolbar qat = ribbon != null ? ribbon.QuickAccessToolbar : null;

					if (null != qat)
					{
						int index = qat.VisibleIndexOf(this._tool);
						// AS 12/20/07
						//return QuickAccessToolbar.GetKeyTipAtIndex(index);
						return qat.GetKeyTipAtIndex(index);
					}

				}

				return RibbonToolHelper.GetKeyTip(this._tool);
			}
		}

		public virtual Point Location
		{
			get
			{
				if (this._menuItem != null)
				{
					if (null != this._proxy && this._proxy.GetMenuItemDisplayMode(this.Tool) == RibbonToolProxy.ToolMenuItemDisplayMode.UseToolForEntireArea)
						return ToolKeyTipProvider.GetToolKeyTipLocation(this.Tool, XamRibbon.GetLocation(this.Tool), RibbonToolSizingMode.ImageAndTextNormal);

					return ToolKeyTipProvider.GetToolKeyTipLocation(this._menuItem, XamRibbon.GetLocation(this.Tool), RibbonToolSizingMode.ImageAndTextNormal);
				}

				return GetToolKeyTipLocation(this._tool, XamRibbon.GetLocation(this._tool), RibbonToolHelper.GetSizingMode(this._tool));
			}
		}

		public virtual char Mnemonic
		{
			get { return Utilities.GetFirstMnemonicChar(this.Caption); }
		}

		public virtual UIElement AdornedElement
		{
			get { return this._menuItem ?? this._tool; }
		}

		// AS 10/3/07 BR27022
		public virtual char DefaultPrefix
		{
			get { return this._tool.GetType().Name[0]; }
		}

		// AS 1/5/10 TFS25626
		public virtual bool CanAutoGenerateKeyTip
		{
			get { return !this.IsOnQat; }
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