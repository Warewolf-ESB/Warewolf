using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Ribbon.Internal;
using System.Windows.Documents;

namespace Infragistics.Windows.Ribbon
{
	#region IRibbonTool public interface

	/// <summary>
	/// Interface used by implementors of custom tools to add support for advanced <see cref="XamRibbon"/> functionality such as the ability for a copy of the tool 
	/// to be added to <see cref="QuickAccessToolbar"/> or for a tool to appear in multiple locations on a <see cref="XamRibbon"/>, with tool state shared across all locations.  
	/// If you are not creating your own tool types you can ignore this interface. 
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[RibbonTool()]
	public interface IRibbonTool
	{
		/// <summary>
		/// Returns an instance of a class that provides the functionality for cloning and binding the properties of the tool.
		/// </summary>
		RibbonToolProxy ToolProxy { get;}
	}

	#endregion //IRibbonTool public interface

	#region IRibbonToolLocation internal interface

	internal interface IRibbonToolLocation
	{
		ToolLocation Location { get; }
	}

	#endregion //IRibbonToolLocation internal interface	

	// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
	#region IRibbonToolLocationEx
	internal interface IRibbonToolLocationEx : IRibbonToolLocation
	{
		ToolLocation GetLocation(object descendant);
	} 
	#endregion //IRibbonToolLocationEx
    
	#region IRibbonToolPanel
	/// <summary>
	/// An interface implemented by panels that can resize tools.
	/// </summary>
	internal interface IRibbonToolPanel
	{
		/// <summary>
		/// Used to obtain a list of framework elements whose <see cref="RibbonToolHelper.SizingModeProperty"/> property is allowed to go to the specified destination size.
		/// </summary>
		/// <param name="destinationSize">The size to which the tools will be resized.</param>
		/// <returns>A list of tools that may be resized to the specified tool size. The tools should be arranged such that changing their size will result in the 
		/// panel being calculated as a smaller size. E.g. A ToolVerticalWrapPanel would return 3 consecutive large tools when changing the size of large tools to 
		/// a sizing mode of <b>ImageAndTextNormal</b>.</returns>
		IList<FrameworkElement> GetResizableTools(RibbonToolSizingMode destinationSize);
	} 
	#endregion //IRibbonToolPanel

	#region IKeyTipProvider
	/// <summary>
	/// Interface implemented by an object that provides a key tip for keyboard access
	/// </summary>  
	internal interface IKeyTipProvider
	{
		#region Methods

		/// <summary>
		/// Called when the providers key tip has been typed by the user.  
		/// The provider should perform its default action when this is called
		/// </summary> 
		bool Activate();

		/// <summary>
		/// Return the value indicating whether this provider is equal to the specified provider
		/// </summary>  
		bool Equals(IKeyTipProvider provider);

		/// <summary>
		/// Gets the key tip container related to this provider.  
		/// Providers that do not contain other key tips should return null.
		/// </summary>  
		IKeyTipContainer GetContainer();

		#endregion Methods

		#region Properties

		/// <summary>
		/// Gets the alignment of the key tip based on the location.
		/// </summary>  
		KeyTipAlignment Alignment { get; }

		/// <summary>
		/// Gets the prefix to use when auto-generating a key tip for the provider.
		/// </summary>  
		string AutoGeneratePrefix { get; }

		/// <summary>
		/// The caption of the provider.  
		/// This will be used to auto-generate a key tip when <see cref="KeyTipValue"/> and <see cref="Mnemonic"/> is null or empty.
		/// </summary>  
		string Caption { get; }

		// MD 9/28/06 - BR16285
		/// <summary>
		/// Gets the value indicating whether to deactivate parent key tips containers when this provider is activated.
		/// </summary>
		/// <remarks>
		/// This will only be used for providers that return null for <see cref="GetContainer"/>.
		/// Deactivation could mean closing the sub menu where the provider resides.
		/// </remarks>  
		bool DeactivateParentContainersOnActivate { get;}

		/// <summary>
		/// Gets the value indicating whether the key tip should be displayed enabled.
		/// </summary>  
		bool IsEnabled { get;}

		/// <summary>
		/// Gets the value indicating whether the key tip should be displayed.
		/// </summary>  
		bool IsVisible { get;}

		/// <summary>
		/// Gets the key tip value to use for this provider.
		/// </summary>  
		string KeyTipValue { get;}

		/// <summary>
		/// Gets the locaton where the key tip should be displayed in screen coordinates
		/// </summary>  
		Point Location { get;}

		/// <summary>
		/// The mnemonic of the provider.  
		/// This will be used to auto-generate a key tip when <see cref="KeyTipValue"/> is null or empty.
		/// </summary>  
		char Mnemonic { get;}

		/// <summary>
		/// Returns the element that should be adorned with the keytip
		/// </summary>
		UIElement AdornedElement { get; }

		// AS 10/3/07 BR27022
		/// <summary>
		/// This will be used to auto-generate a key tip when there is no caption, mnemonic or auto generation prefix.
		/// </summary>  
		char DefaultPrefix { get;}

		// AS 1/5/10 TFS25626
		// For items in the qat, we only generated keytips for the items using the 
		// pattern that Office uses which is essentially providing one for the first 
		// 44 items. For all others no keytip is provided so we need to avoid auto
		// generating one for the items in the qat.
		//
		/// <summary>
		/// Returns a boolean indicating if a key tip can be generated from the mnemonic, caption or autogeneration/default prefix.
		/// </summary>
		bool CanAutoGenerateKeyTip { get; }

		#endregion Properties
	} 
	#endregion //IKeyTipProvider

	#region IKeyTipContainer
	/// <summary>
	/// Interface implemented by an object that contains a collection of key tips.
	/// </summary>  
	internal interface IKeyTipContainer
	{
		/// <summary>
		/// Called when this container is being displayed and the user presses the ESC key to back out of it.
		/// </summary>  
		void Deactivate();

		/// <summary>
		/// Gets the key tip providers in this container
		/// </summary>  
		IKeyTipProvider[] GetKeyTipProviders();

		/// <summary>
		/// Value indicating whether to look for providers from this container in the parent container, 
		/// and to reuse the key tips created for those providers in this container
		/// </summary>  
		bool ReuseParentKeyTips { get;}
	} 
	#endregion //IKeyTipContainer

	#region IRibbonToolHost
	/// <summary>
	/// Interface implemented by an object that contains elements which implement IRibbonTool
	/// </summary>  
	internal interface IRibbonToolHost
	{
		/// <summary>
		/// The IRibbonTool being hosted.
		/// </summary>  
		IRibbonTool Tool { get;}
	}
	#endregion //IRibbonToolHost

	#region IRibbonPopupOwner
	internal interface IRibbonPopupOwner
	{
		/// <summary>
		/// Returns the associated <see cref="XamRibbon"/>
		/// </summary>
		XamRibbon Ribbon { get; }

		/// <summary>
		/// Used to determine if the popup target can be opened when it is the active item.
		/// </summary>
		bool CanOpen { get; }

		/// <summary>
		/// Returns/sets whether the popup is currently open.
		/// </summary>
		bool IsOpen { get; set; }

		/// <summary>
		/// Used to move focus to the first item within the popup
		/// </summary>
		/// <returns>Returns a boolean indicating if focus was shifted</returns>
		bool FocusFirstItem();

		// AS 10/18/07
		/// <summary>
		/// Returns the element that should be activated when the popup is opened in keytip mode and to reactivated when the dropdown is closed due to hitting escape.
		/// </summary>
		FrameworkElement ParentElementToFocus { get; }

		/// <summary>
		/// Returns the element that represents the templated parent of the popup.
		/// </summary>
		UIElement PopupTemplatedParent { get; }

		/// <summary>
		/// Returns a boolean indicating whether the <see cref="PopupOwnerProxy"/> should hook the KeyDown event on behalf of the owner. If false, the owner must explicitly call the ProcessKeyDown of the proxy.
		/// </summary>
		bool HookKeyDown { get; }
	} 
	#endregion //IRibbonPopupOwner
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