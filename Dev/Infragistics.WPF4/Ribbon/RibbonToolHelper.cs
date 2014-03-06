using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using Infragistics.Shared;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Ribbon
{
	// AS 12/3/09 TFS24919
	// The Id property is set by the xamRibbon on a tool that doesn't have its image set. This 
	// id is used to uniquely identify the tool and when an item is added to the qat, it has 
	// the same id. In this case the tool on the qat didn't have that id. The reason is that 
	// it didn't expose the Id property and since the IdProperty on XamRibbon was internal, it 
	// was not copied (because the MarkupObject or more accurately the TypeDescriptor.GetProperties 
	// does not include that property since its considered internal). All of our tools AddOwner 
	// and reexpose properties on the xamRibbon like the Id so the right thing to do is to expose 
	// these properties. However to avoid cluttering the intellisense, these attached properties 
	// were moved to the RibbonToolHelper class. The ultimate fix was to make these properties 
	// public so even if a custom tool doesn't reexpose the id, it is still available as that 
	// attached property. Note as part of this fix I had to change all the references to these 
	// properties to reference this class as opposed to the xamRibbon.
	//
	/// <summary>
	/// Provides properties used by tools within the <see cref="XamRibbon"/>
	/// </summary>
	public static class RibbonToolHelper
	{
		#region Properties

		#region Public Properties

		#region Caption

		/// <summary>
		/// Identifies the Caption attached dependency property which determines the caption of a tool.
		/// </summary>
		/// <seealso cref="GetCaption"/>
		/// <seealso cref="SetCaption"/>
		/// <seealso cref="GetHasCaption"/>
		public static readonly DependencyProperty CaptionProperty = DependencyProperty.RegisterAttached("Caption",
			typeof(string), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCaptionChanged)));

		private static void OnCaptionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			string caption = e.NewValue as string;

			// maintain the HasCaption property
			if (string.IsNullOrEmpty(caption) == false)
				target.SetValue(HasCaptionPropertyKey, KnownBoxes.TrueBox);
			else
				target.ClearValue(HasCaptionPropertyKey);
		}

		/// <summary>
		/// Gets the string caption associated with a given XamRibbon tool.
		/// </summary>
		/// <param name="d">The tool for which the value is being requested</param>
		/// <returns>The string caption or null if one hasn't been provided.</returns>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static string GetCaption(DependencyObject d)
		{
			return (string)d.GetValue(RibbonToolHelper.CaptionProperty);
		}

		/// <summary>
		/// Sets the strings caption for a given <see cref="XamRibbon"/> tool
		/// </summary>
		/// <param name="d">The tool whose caption is being set</param>
		/// <param name="value">The new caption</param>
		public static void SetCaption(DependencyObject d, string value)
		{
			d.SetValue(RibbonToolHelper.CaptionProperty, value);
		}

		#endregion //Caption

		#region HasCaption

		internal static readonly DependencyPropertyKey HasCaptionPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("HasCaption",
			typeof(bool), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the read-only HasCaption attached dependency property which determines if the <see cref="CaptionProperty"/> has been set for a given tool.
		/// </summary>
		/// <seealso cref="GetCaption"/>
		/// <seealso cref="SetCaption"/>
		/// <seealso cref="GetHasCaption"/>
		public static readonly DependencyProperty HasCaptionProperty =
			HasCaptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the <see cref="CaptionProperty"/> has been set for a given tool.
		/// </summary>
		/// <param name="d">The tool whose value is being evaluated</param>
		/// <returns>True if the Caption property has been set for the specified tool; otherwise false.</returns>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static bool GetHasCaption(DependencyObject d)
		{
			return (bool)d.GetValue(RibbonToolHelper.HasCaptionProperty);
		}

		#endregion //HasCaption

		#region HasImage

		private static readonly DependencyPropertyKey HasImagePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("HasImage",
			typeof(bool), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the read-only HasImage attached dependency property which determines if the ImageResolved property for a tool.
		/// </summary>
		/// <seealso cref="GetHasImage"/>
		/// <seealso cref="GetImageResolved"/>
		/// <seealso cref="GetLargeImage"/>
		/// <seealso cref="SetLargeImage"/>
		/// <seealso cref="GetSmallImage"/>
		/// <seealso cref="SetSmallImage"/>
		public static readonly DependencyProperty HasImageProperty =
			HasImagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the tool has an image associated with it.
		/// </summary>
		/// <param name="d">The tool for which the property is being evaluated</param>
		/// <returns>True if the ImageResolved has been set; otherwise false.</returns>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static bool GetHasImage(DependencyObject d)
		{
			return (bool)d.GetValue(RibbonToolHelper.HasImageProperty);
		}

		#endregion //HasImage

		#region Id

		/// <summary>
		/// Identifies the Id attached dependency property which is used to uniquely identify related tools within a <see cref="XamRibbon"/>.
		/// </summary>
		/// <seealso cref="GetId"/>
		/// <seealso cref="SetId"/>
		public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached("Id",
			typeof(string), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIdChanged)));

		private static void OnIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FrameworkElement tool = d as FrameworkElement;
			IRibbonTool irt = tool as IRibbonTool;

			// all ribbon tools are automatically registered
			if (null != irt && tool is ApplicationMenu == false)
			{
				XamRibbon ribbon = (XamRibbon)tool.GetValue(XamRibbon.RibbonProperty);

				if (null != ribbon && ribbon.ToolInstanceManager.IsRegistered(tool, (string)e.OldValue))
					ribbon.ToolInstanceManager.ChangeToolInstanceRegistrationId(tool, (string)e.OldValue);
			}
		}

		/// <summary>
		/// Gets the string used to uniquely identify instances of the same logical tool within the <see cref="XamRibbon"/>
		/// </summary>
		/// <param name="d">The tool for which the value is being requested</param>
		/// <returns>The string id or null if one hasn't been provided.</returns>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static string GetId(DependencyObject d)
		{
			return (string)d.GetValue(RibbonToolHelper.IdProperty);
		}

		/// <summary>
		/// Provides the string used to uniquely identify instances of the same logical tool within the <see cref="XamRibbon"/>
		/// </summary>
		/// <param name="d">The tool whose id is being changed</param>
		/// <param name="value">The new string used to identify the tool</param>
		public static void SetId(DependencyObject d, string value)
		{
			d.SetValue(RibbonToolHelper.IdProperty, value);
		}

		#endregion //Id

		#region ImageResolved

		private static readonly DependencyPropertyKey ImageResolvedPropertyKey =
			// AS 12/3/09 TFS24919
			//DependencyProperty.RegisterReadOnly("ImageResolved",
			DependencyProperty.RegisterAttachedReadOnly("ImageResolved",
			typeof(ImageSource), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the read-only ImageResolved attached dependency property which returns the large or small image of the tool depending on its current state.
		/// </summary>
		/// <seealso cref="GetHasImage"/>
		/// <seealso cref="GetImageResolved"/>
		/// <seealso cref="GetLargeImage"/>
		/// <seealso cref="SetLargeImage"/>
		/// <seealso cref="GetSmallImage"/>
		/// <seealso cref="SetSmallImage"/>
		public static readonly DependencyProperty ImageResolvedProperty =
			ImageResolvedPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns the resolved image for a given tool within the <see cref="XamRibbon"/>
		/// </summary>
		/// <param name="d">The tool whose image is being requested</param>
		/// <returns>The value of either the <see cref="LargeImageProperty"/> or <see cref="SmallImageProperty"/> based upon the state of the tool or null if neither image has been provided.</returns>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static ImageSource GetImageResolved(DependencyObject d)
		{
			return (ImageSource)d.GetValue(ImageResolvedProperty);
		}

		#endregion //ImageResolved

		#region IsOnQat

		internal static readonly DependencyPropertyKey IsOnQatPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsOnQat",
			typeof(bool), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the read-only IsOnQat attached dependency property which indicates if an instance of the tool is on the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		/// <seealso cref="GetIsOnQat"/>
		public static readonly DependencyProperty IsOnQatProperty =
			IsOnQatPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if an instance of the tool is on the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static bool GetIsOnQat(DependencyObject d)
		{
			return (bool)d.GetValue(RibbonToolHelper.IsOnQatProperty);
		}

		#endregion //IsOnQat

		#region IsQatCommonTool

		/// <summary>
		/// Identifies the IsQatCommonTool attached dependency property which is used to 
		/// </summary>
		/// <seealso cref="GetIsQatCommonTool"/>
		/// <seealso cref="SetIsQatCommonTool"/>
		public static readonly DependencyProperty IsQatCommonToolProperty = DependencyProperty.RegisterAttached("IsQatCommonTool",
			typeof(bool), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));


		/// <summary>
		/// Returns true if the tool should be shown in the list of 'common tools' displayed in the Quick Customize Menu of the <see cref="QuickAccessToolbar"/>. 
		/// </summary>
		/// <param name="d">The tool whose value is being requested</param>
		/// <seealso cref="IsQatCommonToolProperty"/>
		/// <seealso cref="QuickAccessToolbar"/>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static bool GetIsQatCommonTool(DependencyObject d)
		{
			return (bool)d.GetValue(RibbonToolHelper.IsQatCommonToolProperty);
		}

		/// <summary>
		/// Determines if the specified tool is shown in the list of 'common tools' displayed in the Quick Customize Menu of the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		/// <param name="d">The tool being changed</param>
		/// <param name="value">True if an item should be included for the tool in the Quick Customize Menu of the QuickAccessToolbar; otherwise false.</param>
		public static void SetIsQatCommonTool(DependencyObject d, bool value)
		{
			d.SetValue(RibbonToolHelper.IsQatCommonToolProperty, value);
		}

		#endregion //IsQatCommonTool

		#region KeyTip

		/// <summary>
		/// Identifies the KeyTip attached dependency property which is used to identify the tool when navigating within a <see cref="XamRibbon"/> using its keytips.
		/// </summary>
		/// <seealso cref="GetKeyTip"/>
		/// <seealso cref="SetKeyTip"/>
		public static readonly DependencyProperty KeyTipProperty = DependencyProperty.RegisterAttached("KeyTip",
			typeof(string), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(null), new ValidateValueCallback(ValidateKeyTip));

		internal static bool ValidateKeyTip(object newValue)
		{
			// AS 10/10/07 BR27204
			//return newValue == null || ((string)newValue).Length <= 3;
			//
			// validate length
			if (newValue != null)
			{
				string keyTip = (string)newValue;

				if (keyTip.Length > 3)
					throw new ArgumentException(XamRibbon.GetString("LE_KeyTipTooLong"));

				foreach (char c in keyTip)
				{
					if (false == KeyTipManager.IsValidKeyTipChar(c))
						throw new ArgumentException(XamRibbon.GetString("LE_InvalidKeyTipCharacters"));
				}
			}

			return true;
		}

		/// <summary>
		/// Returns a string used when navigating the <see cref="XamRibbon"/> when using keytips.
		/// </summary>
		/// <remarks>
		/// <p class="body">Key tips are displayed when the ribbon is showing and the Alt key is pressed.</p>
		/// <p class="note"><br>Note: </br>If the key tip for the item conflicts with another item in the same container, this key tip may be changed.</p>
		/// </remarks>
		/// <seealso cref="KeyTipProperty"/>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static string GetKeyTip(DependencyObject d)
		{
			return (string)d.GetValue(RibbonToolHelper.KeyTipProperty);
		}

		/// <summary>
		/// Sets the string with a maximum length of 3 characters that is used when navigating the <see cref="XamRibbon"/> when using keytips.
		/// </summary>
		/// <remarks>
		/// <p class="body">Key tips are displayed when the ribbon is showing and the Alt key is pressed.</p>
		/// <p class="note"><br>Note: </br>If the key tip for the item conflicts with another item in the same container, this key tip may be changed.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">The value assigned has more than 3 characters.</exception>
		/// <seealso cref="KeyTipProperty"/>
		public static void SetKeyTip(DependencyObject d, string value)
		{
			d.SetValue(RibbonToolHelper.KeyTipProperty, value);
		}

		#endregion //KeyTip

		#region LargeImage

		/// <summary>
		/// Identifies the LargeImage attached dependency property which determines the large image that represents the tool.
		/// </summary>
		/// <remarks>
		/// <p class="body">The LargeImageProperty and <see cref="SmallImageProperty"/> are used when calculating the 
		/// <see cref="ImageResolvedProperty"/>. Depending upon the state of the tool, the ImageResolved is set to 
		/// either the large or small image. So for example, a tool within a menu will use the LargeImage as its 
		/// ImageResolved when the <see cref="MenuToolBase.UseLargeImages"/> is true. A tool within a <see cref="RibbonGroup"/> 
		/// will use the LargeImage when the <see cref="SizingModeProperty"/> is ImageAndTextLarge.</p>
		/// </remarks>
		/// <seealso cref="GetHasImage"/>
		/// <seealso cref="GetImageResolved"/>
		/// <seealso cref="GetLargeImage"/>
		/// <seealso cref="SetLargeImage"/>
		/// <seealso cref="GetSmallImage"/>
		/// <seealso cref="SetSmallImage"/>
		public static readonly DependencyProperty LargeImageProperty = DependencyProperty.RegisterAttached("LargeImage",
			typeof(ImageSource), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnImagePropertyChanged)));

		/// <summary>
		/// Returns the image that should be used when the tool is in its large state.
		/// </summary>
		/// <param name="d">The tool whose large image is being requested</param>
		/// <returns>The ImageSource set as the large image or null if none was provided.</returns>
		/// <seealso cref="LargeImageProperty"/>
		public static ImageSource GetLargeImage(DependencyObject d)
		{
			return (ImageSource)d.GetValue(RibbonToolHelper.LargeImageProperty);
		}

		/// <summary>
		/// Sets the image that should be used when the tool is in its large state.
		/// </summary>
		/// <param name="d">The tool whose large image is to be changed</param>
		/// <param name="value">The new image to use when the tool is in the large state</param>
		/// <seealso cref="LargeImageProperty"/>
		public static void SetLargeImage(DependencyObject d, ImageSource value)
		{
			d.SetValue(RibbonToolHelper.LargeImageProperty, value);
		}

		#endregion //LargeImage

		#region SizingMode

		internal static readonly DependencyPropertyKey SizingModePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("SizingMode",
			typeof(RibbonToolSizingMode), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextNormalBox, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnSizingModeChanged)));

		private static void OnSizingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonToolHelper.UpdateImageProperties(d);
		}

		/// <summary>
		/// Identifies the read-only SizingMode attached dependency property which indicates the current size of given tool within a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <seealso cref="GetSizingMode"/>
		public static readonly DependencyProperty SizingModeProperty =
			SizingModePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the current sizing mode of a tool within a <see cref="RibbonGroup"/>
		/// </summary>
		/// <param name="d">The tool in a RibbonGroup whose sizing mode is being evaluated</param>
		/// <returns>The current sizing mode that the tool should use to determine how it should be rendered</returns>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static RibbonToolSizingMode GetSizingMode(DependencyObject d)
		{
			return (RibbonToolSizingMode)d.GetValue(RibbonToolHelper.SizingModeProperty);
		}

		#endregion //SizingMode

		#region SmallImage

		/// <summary>
		/// Identifies the SmallImage attached dependency property which determines the small image that represents the tool.
		/// </summary>
		/// <remarks>
		/// <p class="body">The SmallImageProperty and <see cref="LargeImageProperty"/> are used when calculating the 
		/// <see cref="ImageResolvedProperty"/>. Depending upon the state of the tool, the ImageResolved is set to 
		/// either the large or small image. So for example, a tool within a menu will use the LargeImage as its 
		/// ImageResolved when the <see cref="MenuToolBase.UseLargeImages"/> is true. A tool within a <see cref="RibbonGroup"/> 
		/// will use the LargeImage when the <see cref="SizingModeProperty"/> is ImageAndTextLarge.</p>
		/// </remarks>
		/// <seealso cref="GetHasImage"/>
		/// <seealso cref="GetImageResolved"/>
		/// <seealso cref="GetLargeImage"/>
		/// <seealso cref="SetLargeImage"/>
		/// <seealso cref="GetSmallImage"/>
		/// <seealso cref="SetSmallImage"/>
		public static readonly DependencyProperty SmallImageProperty = DependencyProperty.RegisterAttached("SmallImage",
			typeof(ImageSource), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnImagePropertyChanged)));

		private static void OnImagePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			RibbonToolHelper.UpdateImageProperties(target);
		}

		/// <summary>
		/// Returns the image that should be used when the tool is in its normal state.
		/// </summary>
		/// <param name="d">The tool whose small image is being requested</param>
		/// <returns>The ImageSource set as the small image or null if none was provided.</returns>
		/// <seealso cref="SmallImageProperty"/>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static ImageSource GetSmallImage(DependencyObject d)
		{
			return (ImageSource)d.GetValue(RibbonToolHelper.SmallImageProperty);
		}

		/// <summary>
		/// Sets the image that should be used when the tool is in its normal state.
		/// </summary>
		/// <param name="d">The tool whose small image is to be changed</param>
		/// <param name="value">The new image to use when the tool is in the normal state</param>
		/// <seealso cref="SmallImageProperty"/>
		public static void SetSmallImage(DependencyObject d, ImageSource value)
		{
			d.SetValue(RibbonToolHelper.SmallImageProperty, value);
		}

		#endregion //SmallImage

		#endregion //Public Properties

		#region Internal Properties

		#region UseLargeImagesOnMenu

		internal static readonly DependencyProperty UseLargeImagesOnMenuProperty = DependencyProperty.RegisterAttached("UseLargeImagesOnMenu",
			typeof(bool), typeof(RibbonToolHelper), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnUseLargeImagesOnMenuChanged)));

		private static void OnUseLargeImagesOnMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonToolHelper.UpdateImageProperties(d);
		}

		#endregion //UseLargeImagesOnMenu

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region UpdateImageProperties
		internal static void UpdateImageProperties(DependencyObject target)
		{
			// JJD 12/4/07 - BR28873
			// We should bail out if the target is a ToolMenuItem
			if (target is ToolMenuItem)
				return;

			Debug.Assert(target != null);

			bool useLargeImage = false;

			switch (XamRibbon.GetLocation(target))
			{
				// AS 10/12/07 UseLargeImages
				case ToolLocation.Menu:

				case ToolLocation.ApplicationMenu:
				case ToolLocation.ApplicationMenuSubMenu:
					// AS 10/12/07 UseLargeImages
					//useLargeImage = true;
					useLargeImage = (bool)target.GetValue(RibbonToolHelper.UseLargeImagesOnMenuProperty);
					break;

				case ToolLocation.Ribbon:
					useLargeImage = RibbonToolHelper.GetSizingMode(target) == RibbonToolSizingMode.ImageAndTextLarge;
					break;
				default:
					break;
			}

			object imageSource = null;
			DependencyProperty primaryProp = useLargeImage ? RibbonToolHelper.LargeImageProperty : RibbonToolHelper.SmallImageProperty;

			imageSource = target.GetValue(primaryProp);

			if (null == imageSource)
			{
				DependencyProperty secondaryProp = useLargeImage ? RibbonToolHelper.SmallImageProperty : RibbonToolHelper.LargeImageProperty;
				imageSource = target.GetValue(secondaryProp);
			}

			if (null != imageSource)
			{
				target.SetValue(RibbonToolHelper.ImageResolvedPropertyKey, imageSource);
				target.SetValue(RibbonToolHelper.HasImagePropertyKey, KnownBoxes.TrueBox);
			}
			else
			{
				target.ClearValue(RibbonToolHelper.ImageResolvedPropertyKey);
				target.ClearValue(RibbonToolHelper.HasImagePropertyKey);
			}

			// JJD 12/4/07 - BR28873
			// Set the corresponding UseLargeImage on the tool iteself so we can trigger of it
			target.SetValue(ToolMenuItem.UseLargeImagePropertyKey, KnownBoxes.FromValue(useLargeImage));
		}
		#endregion //UpdateImageProperties

        // JJD 4/12/10 - NA2010 Vol 2 - Added support for VisualStateManager
        #region SetToolVisualStates


        internal static void SetToolVisualState(Control tool, bool useTransitions, bool? isActive)
        {
            if (isActive.HasValue)
            {
                // set common state
                if ( tool.IsEnabled == false )
                    VisualStateManager.GoToState(tool, VisualStateUtilities.StateDisabled, useTransitions);
                else
                
                
                
                if ( tool.IsMouseOver )
                    VisualStateManager.GoToState(tool, VisualStateUtilities.StateMouseOver, useTransitions);
                else
                    VisualStateManager.GoToState(tool, VisualStateUtilities.StateNormal, useTransitions);

                // set active state
                if (isActive.Value == true)
                    VisualStateManager.GoToState(tool, VisualStateUtilities.StateActive, useTransitions);
                else
                    VisualStateManager.GoToState(tool, VisualStateUtilities.StateInactive, useTransitions);
            }

            string state = null;

            // set location state
            switch (XamRibbon.GetLocation(tool))
            {
                case ToolLocation.ApplicationMenu:
                    state = VisualStateUtilities.StateAppMenu;
                    break;
                case ToolLocation.ApplicationMenuFooterToolbar:
                    state = VisualStateUtilities.StateAppMenuFooterToolbar;
                    break;
                case ToolLocation.ApplicationMenuRecentItems:
                    state = VisualStateUtilities.StateAppMenuRecentItems;
                    break;
                case ToolLocation.ApplicationMenuSubMenu:
                    state = VisualStateUtilities.StateAppMenuSubMenu;
                    break;
                case ToolLocation.Menu:
                    state = VisualStateUtilities.StateMenu;
                    break;
                case ToolLocation.QuickAccessToolbar:
                    state = VisualStateUtilities.StateQAT;
                    break;
                case ToolLocation.Ribbon:
                    state = VisualStateUtilities.StateRibbon;
                    break;
                default:
                    return;
            }
            
            VisualStateManager.GoToState(tool, state, useTransitions);
        }


        #endregion //SetToolVisualStates	
    
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