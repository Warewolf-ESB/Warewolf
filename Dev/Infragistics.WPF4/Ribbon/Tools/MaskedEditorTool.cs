using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Ribbon.Events;
using Infragistics.Windows.Ribbon.Internal;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Windows.Input;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// The MaskedEditorTool allows editing of text using masks to constraint user input.  
	/// </summary>
    /// <remarks>
    /// <para class="note"><b>Note:</b> since the tool is derived from <see cref="Infragistics.Windows.Editors.XamMaskedEditor"/> 
    /// the usage semantics of the tool are the same as a XamMaskedEditor with the addition of certain common tool properties, e.g. <see cref="Caption"/>, <see cref="SmallImage"/>, <see cref="Id"/>, <see cref="KeyTip"/> etc.</para>
    /// </remarks>
    /// <seealso cref="TextEditorTool"/>
    /// <seealso cref="ComboEditorTool"/>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,           GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateActive,              GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,            GroupName = VisualStateUtilities.GroupActive)]

    [TemplateVisualState(Name = VisualStateUtilities.StateMenu,                GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateRibbon,              GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateQAT,                 GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenu,             GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuFooterToolbar,GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuRecentItems,  GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuSubMenu,      GroupName = VisualStateUtilities.GroupLocation)]

    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class MaskedEditorTool : XamMaskedEditor, IRibbonTool
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="MaskedEditorTool"/> class.
		/// </summary>
		public MaskedEditorTool()
		{
		}

		static MaskedEditorTool()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MaskedEditorTool), new FrameworkPropertyMetadata(typeof(MaskedEditorTool)));
			RibbonGroup.MaximumSizeProperty.OverrideMetadata(typeof(MaskedEditorTool), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextNormalBox));
			RibbonGroup.MinimumSizeProperty.OverrideMetadata(typeof(MaskedEditorTool), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextNormalBox));

			
			// Set the FocusVisualStyle on the editor tool to empty so it doesn't draw the dashed border
			// to indicate it has the focus. It would do so when it's not in edit mode and it has focus.
			// This is undesirable so set the FocusVisualStyleProperty to empty style.
			// 
			FrameworkElement.FocusVisualStyleProperty.OverrideMetadata( typeof( MaskedEditorTool ), new FrameworkPropertyMetadata( new Style( ) ) );

			ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(MaskedEditorTool), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

			// AS 9/17/09 TFS20559
			EventManager.RegisterClassHandler(typeof(MaskedEditorTool), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(TextEditorTool.OnEditorAccessKeyPressed));

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            XamRibbon.IsActiveProperty.OverrideMetadata(typeof(MaskedEditorTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), XamRibbon.IsActivePropertyKey);
            XamRibbon.LocationProperty.OverrideMetadata(typeof(MaskedEditorTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), XamRibbon.LocationPropertyKey);
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(MaskedEditorTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		#endregion //Constructor

		#region Base class overrides

		#region OnAccessKey
		/// <summary>
		/// Responds when the System.Windows.Controls.AccessText.AccessKey for this control is invoked.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnAccessKey(AccessKeyEventArgs e)
		{
			if (e.IsMultiple)
				base.OnAccessKey(e);
			else
			{
				// AS 9/17/09 TFS20559
				// See TextEditorTool.OnAccessKey for details.
				//
				this.StartEditMode();
			}
		}
		#endregion //OnAccessKey

		// AS 6/24/09 TFS18346
		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="MaskedEditorTool"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.MaskedEditorToolAutomationPeer"/></returns>
		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new MaskedEditorToolAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer
    
			#region OnMouseEnter

		/// <summary>
		/// Raised when the mouse enters the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);


            // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

			#endregion //OnMouseEnter	
    
			#region OnMouseLeave

		/// <summary>
		/// Raised when the mouse leaves the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseLeave(e);


            // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

		}

			#endregion //OnMouseLeave	

        #region SetVisualState


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected override void SetVisualState(bool useTransitions)
        {
            base.SetVisualState(useTransitions);

            RibbonToolHelper.SetToolVisualState(this, useTransitions, this.IsActive);
        }

        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static new void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            MaskedEditorTool tool = target as MaskedEditorTool;

            if (tool != null)
                tool.UpdateVisualStates();
        }


        #endregion //SetVisualState	

		#endregion //Base class overrides

		#region Events

			#region Common Tool Events

				#region Activated

		/// <summary>
		/// Event ID for the <see cref="Activated"/> routed event
		/// </summary>
		/// <seealso cref="Activated"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		internal static readonly RoutedEvent ActivatedEvent = MenuTool.ActivatedEvent.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Occurs after a tool has been activated
		/// </summary>
		/// <seealso cref="XamRibbon.ActiveItem"/>
		/// <seealso cref="XamRibbon.IsActiveProperty"/>
		/// <seealso cref="XamRibbon.GetIsActive(DependencyObject)"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="ActivatedEvent"/>
		/// <seealso cref="ItemActivatedEventArgs"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		//[Description("Occurs after a tool has been activated")]
		//[Category("Ribbon Properties")]
		internal event EventHandler<ItemActivatedEventArgs> Activated
		{
			add
			{
				base.AddHandler(MenuTool.ActivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.ActivatedEvent, value);
			}
		}

				#endregion //Activated

				#region Cloned

		/// <summary>
		/// Event ID for the <see cref="Cloned"/> routed event
		/// </summary>
		/// <seealso cref="Cloned"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent ClonedEvent = MenuTool.ClonedEvent.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Occurs after a tool has been cloned.
		/// </summary>
		/// <remarks>
		/// <para class="body">Fired when a tool is cloned to enable its placement in an additional location in the XamRibbon.  For example, when a tool is added to the QuickAccessToolbar, the XamRibbon clones the instance of the tool that appears on the Ribbon and places the cloned instance on the QAT.  This event is fired after the cloning takes place and is a convenient point hook up event listeners for events on the cloned tool.</para>
		/// </remarks>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="ClonedEvent"/>
		/// <seealso cref="ToolClonedEventArgs"/>
		//[Description("Occurs after a tool has been cloned")]
		//[Category("Ribbon Events")]
		public event EventHandler<ToolClonedEventArgs> Cloned
		{
			add
			{
				base.AddHandler(MenuTool.ClonedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.ClonedEvent, value);
			}
		}

				#endregion //Cloned

				#region CloneDiscarded

		/// <summary>
		/// Event ID for the <see cref="CloneDiscarded"/> routed event
		/// </summary>
		/// <seealso cref="CloneDiscarded"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent CloneDiscardedEvent = MenuTool.CloneDiscardedEvent.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Occurs when a clone of a tool is being discarded.
		/// </summary>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="CloneDiscardedEvent"/>
		/// <seealso cref="ToolCloneDiscardedEventArgs"/>
		//[Description("Occurs when a clone of a tool is being discarded.")]
		//[Category("Ribbon Events")]
		public event EventHandler<ToolCloneDiscardedEventArgs> CloneDiscarded
		{
			add
			{
				base.AddHandler(MenuTool.CloneDiscardedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.CloneDiscardedEvent, value);
			}
		}

				#endregion //CloneDiscarded

				#region Deactivated

		/// <summary>
		/// Event ID for the <see cref="Deactivated"/> routed event
		/// </summary>
		/// <seealso cref="Deactivated"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		internal static readonly RoutedEvent DeactivatedEvent = MenuTool.DeactivatedEvent.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Occurs after a tool has been de-activated
		/// </summary>
		/// <seealso cref="XamRibbon.ActiveItem"/>
		/// <seealso cref="XamRibbon.IsActiveProperty"/>
		/// <seealso cref="XamRibbon.GetIsActive(DependencyObject)"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="DeactivatedEvent"/>
		/// <seealso cref="ItemDeactivatedEventArgs"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		//[Description("Occurs after a tool has been de-activated")]
		//[Category("Ribbon Properties")]
		internal event EventHandler<ItemDeactivatedEventArgs> Deactivated
		{
			add
			{
				base.AddHandler(MenuTool.DeactivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.DeactivatedEvent, value);
			}
		}

				#endregion //Deactivated

			#endregion //Common Tool Events

		#endregion //Events

		#region Methods

			#region Protected methods (common tool methods)

				#region OnRaiseToolEvent

		/// <summary>
		/// Occurs when the tool event is about to be raised
		/// </summary>
		/// <remarks><para class="body">The base class implemenation simply calls the RaiseEvent method.</para></remarks>
		protected virtual void OnRaiseToolEvent(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

				#endregion //OnRaiseToolEvent

			#endregion //Protected methods (common tool methods)

			#region Protected methods

			#endregion //Protected methods

		#endregion //Methods

		#region Common Tool Properties

			#region Caption

		/// <summary>
		/// Identifies the Caption dependency property.
		/// </summary>
		/// <seealso cref="Caption"/>
		/// <seealso cref="HasCaptionProperty"/>
		/// <seealso cref="HasCaption"/>
		public static readonly DependencyProperty CaptionProperty = RibbonToolHelper.CaptionProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns/sets the caption associated with the tool.
		/// </summary>
		/// <seealso cref="CaptionProperty"/>
		/// <seealso cref="HasCaptionProperty"/>
		/// <seealso cref="HasCaption"/>
		//[Description("Returns/sets the caption associated with the tool.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Caption
		{
			get
			{
				return (string)this.GetValue(MaskedEditorTool.CaptionProperty);
			}
			set
			{
				this.SetValue(MaskedEditorTool.CaptionProperty, value);
			}
		}

			#endregion //Caption

			#region HasCaption

		/// <summary>
		/// Identifies the HasCaption dependency property.
		/// </summary>
		/// <seealso cref="HasCaption"/>
		/// <seealso cref="CaptionProperty"/>
		/// <seealso cref="Caption"/>
		public static readonly DependencyProperty HasCaptionProperty = RibbonToolHelper.HasCaptionProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns true if the tool has a caption with a length greater than zero, otherwise returns false. (read only)
		/// </summary>
		/// <seealso cref="HasCaptionProperty"/>
		/// <seealso cref="CaptionProperty"/>
		/// <seealso cref="Caption"/>
		//[Description("Returns true if the tool has a caption with a length greater than zero, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool HasCaption
		{
			get
			{
				return (bool)this.GetValue(MaskedEditorTool.HasCaptionProperty);
			}
		}

			#endregion //HasCaption

			#region HasImage

		/// <summary>
		/// Identifies the HasImage dependency property.
		/// </summary>
		/// <seealso cref="HasImage"/>
		/// <seealso cref="SmallImageProperty"/>
		/// <seealso cref="SmallImage"/>
		public static readonly DependencyProperty HasImageProperty = RibbonToolHelper.HasImageProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns true if the tool has a <see cref="SmallImage"/> or <see cref="LargeImage"/> specified, otherwise returns false. (read only)
		/// </summary>
		/// <seealso cref="HasImageProperty"/>
		/// <seealso cref="SmallImageProperty"/>
		/// <seealso cref="SmallImage"/>
		//[Description("Returns true if the tool has a SmallImage specified, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool HasImage
		{
			get
			{
				return (bool)this.GetValue(MaskedEditorTool.HasImageProperty);
			}
		}

			#endregion //HasImage

			#region Id

		/// <summary>
		/// Identifies the Id dependency property.
		/// </summary>
		/// <seealso cref="Id"/>
		public static readonly DependencyProperty IdProperty = RibbonToolHelper.IdProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns/sets the Id associated with the tool.
		/// </summary>
		/// <seealso cref="IdProperty"/>
		//[Description("Returns/sets the Id associated with the tool.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Id
		{
			get
			{
				return (string)this.GetValue(MaskedEditorTool.IdProperty);
			}
			set
			{
				this.SetValue(MaskedEditorTool.IdProperty, value);
			}
		}

			#endregion //Id

			#region ImageResolved

		/// <summary>
		/// Identifies the <see cref="ImageResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageResolvedProperty = RibbonToolHelper.ImageResolvedProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns the image to be used by the tool based on its current location and sizing mode.
		/// </summary>
		/// <seealso cref="ImageResolvedProperty"/>
		//[Description("Returns the image to be used by the tool based on its current location and sizing mode.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public ImageSource ImageResolved
		{
			get
			{
				return (ImageSource)this.GetValue(MaskedEditorTool.ImageResolvedProperty);
			}
		}

			#endregion //ImageResolved

			#region IsActive

		/// <summary>
		/// Identifies the IsActive dependency property.
		/// </summary>
		/// <seealso cref="IsActive"/>
		public static readonly DependencyProperty IsActiveProperty = XamRibbon.IsActiveProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns true if the tool is the current active item, otherwise returns false. (read only)
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		//[Description("Returns true if the tool is the current active item, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(MaskedEditorTool.IsActiveProperty);
			}
		}

			#endregion //IsActive

			#region IsQatCommonTool

		/// <summary>
		/// Identifies the IsQatCommonTool dependency property.
		/// </summary>
		/// <seealso cref="IsQatCommonTool"/>
		public static readonly DependencyProperty IsQatCommonToolProperty = RibbonToolHelper.IsQatCommonToolProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns true if the tool should be shown in the list of 'common tools' displayed in the <see cref="QuickAccessToolbar"/>'s Quick Customize Menu. 
		/// </summary>
		/// <seealso cref="IsQatCommonToolProperty"/>
		/// <seealso cref="QuickAccessToolbar"/>
		//[Description("Returns true if the tool should be shown in the list of 'common tools' displayed in the QuickAccessToolbar's Quick Customize Menu.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsQatCommonTool
		{
			get
			{
				return (bool)this.GetValue(MaskedEditorTool.IsQatCommonToolProperty);
			}
			set
			{
				this.SetValue(MaskedEditorTool.IsQatCommonToolProperty, value);
			}
		}

			#endregion //IsQatCommonTool

			#region IsOnQat

		/// <summary>
		/// Identifies the IsOnQat dependency property.
		/// </summary>
		/// <seealso cref="IsOnQat"/>
		public static readonly DependencyProperty IsOnQatProperty = RibbonToolHelper.IsOnQatProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns true if the tool (or an instance of the tool with the same Id) exists on the <see cref="QuickAccessToolbar"/>, otherwise returns false. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">To determine whether a specific tool instance is directly on the Qat, check the <see cref="Location"/> property.</p>
		/// </remarks>
		/// <seealso cref="IsOnQatProperty"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="QatPlaceholderTool"/>
		/// <seealso cref="Location"/>
		//[Description("Returns true if the tool (or an instance of the tool with the same Id) exists on the QuickAccessToolbar, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsOnQat
		{
			get
			{
				return (bool)this.GetValue(MaskedEditorTool.IsOnQatProperty);
			}
		}

			#endregion //IsOnQat

			#region KeyTip

		/// <summary>
		/// Identifies the KeyTip dependency property.
		/// </summary>
		/// <seealso cref="KeyTip"/>
		public static readonly DependencyProperty KeyTipProperty = RibbonToolHelper.KeyTipProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// A string with a maximum length of 3 characters that is used to navigate to the item when keytips.
		/// </summary>
		/// <remarks>
		/// <p class="body">Key tips are displayed when the ribbon is showing and the Alt key is pressed.</p>
		/// <p class="note"><br>Note: </br>If the key tip for the item conflicts with another item in the same container, this key tip may be changed.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">The value assigned has more than 3 characters.</exception>
		/// <seealso cref="KeyTipProperty"/>
		//[Description("A string with a maximum length of 3 characters that is used to navigate to the item when keytips.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string KeyTip
		{
			get
			{
				return (string)this.GetValue(MaskedEditorTool.KeyTipProperty);
			}
			set
			{
				this.SetValue(MaskedEditorTool.KeyTipProperty, value);
			}
		}

			#endregion //KeyTip

			#region LargeImage

		/// <summary>
		/// Identifies the LargeImage dependency property.
		/// </summary>
		/// <seealso cref="LargeImage"/>
		public static readonly DependencyProperty LargeImageProperty = RibbonToolHelper.LargeImageProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns/sets the 32x32 image to be used when the tool is sized to ImageAndTextLarge.
		/// </summary>
		/// <seealso cref="LargeImageProperty"/>
		/// <seealso cref="SmallImage"/>
		/// <seealso cref="SizingMode"/>
		/// <seealso cref="RibbonToolSizingMode"/>
		//[Description("Returns/sets the 32x32 image to be used when the tool is sized to ImageAndTextLarge.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ImageSource LargeImage
		{
			get
			{
				return (ImageSource)this.GetValue(MaskedEditorTool.LargeImageProperty);
			}
			set
			{
				this.SetValue(MaskedEditorTool.LargeImageProperty, value);
			}
		}

			#endregion //LargeImage

			#region Location

		/// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		/// <seealso cref="Location"/>
		public static readonly DependencyProperty LocationProperty = XamRibbon.LocationProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns an enumeration that indicates the location of the tool. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">Possible tool locations include: Ribbon, Menu, QuickAccessToolbar, ApplicationMenu, ApplicationMenuFooterToolbar, ApplicationMenuRecentItems.</p>
		/// </remarks>
		/// <seealso cref="LocationProperty"/>
		/// <seealso cref="ToolLocation"/>
		//[Description("Returns an enumeration that indicates the location of the tool. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ToolLocation Location
		{
			get
			{
				return (ToolLocation)this.GetValue(MaskedEditorTool.LocationProperty);
			}
		}

			#endregion //Location

			#region SizingMode

		/// <summary>
		/// Identifies the SizingMode dependency property.
		/// </summary>
		/// <seealso cref="SizingMode"/>
		public static readonly DependencyProperty SizingModeProperty = RibbonToolHelper.SizingModeProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns an enumeration that indicates the current size of the tool. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">Possible sizes include: ImageOnly, ImageAndTextNormal, ImageAndTextLarge.</p>
		/// </remarks>
		/// <seealso cref="SizingModeProperty"/>
		/// <seealso cref="RibbonToolSizingMode"/>
		//[Description("Returns an enumeration that indicates the current size of the tool. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public RibbonToolSizingMode SizingMode
		{
			get
			{
				return (RibbonToolSizingMode)this.GetValue(MaskedEditorTool.SizingModeProperty);
			}
		}

			#endregion //SizingMode

			#region SmallImage

		/// <summary>
		/// Identifies the SmallImage dependency property.
		/// </summary>
		/// <seealso cref="SmallImage"/>
		public static readonly DependencyProperty SmallImageProperty = RibbonToolHelper.SmallImageProperty.AddOwner(typeof(MaskedEditorTool));

		/// <summary>
		/// Returns/sets the 16x16 image to be used when the tool is sized to ImageOnly or ImageAndTextNormal.
		/// </summary>
		/// <seealso cref="SmallImageProperty"/>
		/// <seealso cref="LargeImage"/>
		/// <seealso cref="SizingMode"/>
		/// <seealso cref="RibbonToolSizingMode"/>
		//[Description("Returns/sets the 16x16 image to be used when the tool is sized to ImageOnly or ImageAndTextNormal.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ImageSource SmallImage
		{
			get
			{
				return (ImageSource)this.GetValue(MaskedEditorTool.SmallImageProperty);
			}
			set
			{
				this.SetValue(MaskedEditorTool.SmallImageProperty, value);
			}
		}

			#endregion //SmallImage

		#endregion //Common Tool Properties

		#region Common Editor Tool Properties

		#region EditAreaWidth

		/// <summary>
		/// Identifies the <see cref="EditAreaWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EditAreaWidthProperty = 
			TextEditorTool.EditAreaWidthProperty.AddOwner( typeof( MaskedEditorTool ) );

		/// <summary>
		/// Specifies the width of the edit area, excluding the label. Default value is 100.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This is useful when the editor tool is displaying a label next to it. <b>EditAreaWidth</b>
		/// controls the width of the edit portion, not including the label portion.
		/// </para>
		/// </remarks>
		//[Description( "Specifies the width of the edit area, excluding the label." )]
		//[Category("Ribbon Properties")]
		[Bindable( true )]
		public double EditAreaWidth
		{
			get
			{
				return (double)this.GetValue( EditAreaWidthProperty );
			}
			set
			{
				this.SetValue( EditAreaWidthProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the EditAreaWidth property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeEditAreaWidth( )
		{
			return Utilities.ShouldSerialize( EditAreaWidthProperty, this );
		}

		/// <summary>
		/// Resets the EditAreaWidth property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetEditAreaWidth( )
		{
			this.ClearValue( EditAreaWidthProperty );
		}

		#endregion // EditAreaWidth

		#endregion // Common Editor Tool Properties

		#region IRibbonTool Members

		RibbonToolProxy IRibbonTool.ToolProxy
		{
			get { return MaskedEditorToolProxy.Instance; }
		}

		#endregion

		#region MaskedEditorToolProxy
		/// <summary>
		/// Derived <see cref="RibbonToolProxy"/> for <see cref="MaskedEditorTool"/> instances
		/// </summary>
		protected class MaskedEditorToolProxy : RibbonToolProxy<MaskedEditorTool>
		{
			// AS 5/16/08 BR32980 - See the ToolProxyTests.NoInstanceVariablesOnProxies proxy for details.
			//[ThreadStatic()]
			internal static readonly MaskedEditorToolProxy Instance = new MaskedEditorToolProxy();

			#region Bind

			private static DependencyProperty[] _bindProperties = 
			{ 
				XamMaskedEditor.ClipModeProperty, 
				XamMaskedEditor.DataModeProperty, 
				XamMaskedEditor.DisplayModeProperty, 
				XamMaskedEditor.InsertModeProperty,
				XamMaskedEditor.MaskProperty, 
				XamMaskedEditor.PadCharProperty,
				XamMaskedEditor.PromptCharProperty,
				XamMaskedEditor.SelectAllBehaviorProperty,
				XamMaskedEditor.SpinWrapProperty, 
				XamMaskedEditor.TabNavigationProperty,
			};

			/// <summary>
			/// Binds properties of the target tool to corresponding properties on the specified 
			/// source tool.  The specific properties that are bound are implementation details of the tool.  Generally, any property that 
			/// represents �tool state� and whose value is changeable and should be shared across instances of a given tool, should be bound 
			/// in tool�s implementation of this interface method.
			/// </summary>
			/// <param name="sourceTool">The tool that this tool is being bound to.</param>
			/// <param name="targetTool">The tool whose properties are being bound to the properties of <paramref name="sourceTool"/></param>
			protected override void Bind(MaskedEditorTool sourceTool, MaskedEditorTool targetTool)
			{
				base.Bind(sourceTool, targetTool);

				RibbonToolProxy.BindToolProperties(_bindProperties, sourceTool, targetTool);

				// ValueEditor base class properties
				RibbonToolProxy.BindToolProperties( TextEditorTool.TextEditorBaseProperties, sourceTool, targetTool );
				RibbonToolProxy.BindToolProperties(TextEditorTool.CommonEditorProperties, sourceTool, targetTool);
			}

			#endregion //Bind	
    
			#region GetMenuItemDisplayMode

			/// <summary>
			/// Returns display mode for this tool when it is inside a menu.
			/// </summary>
			/// <returns>Returns 'EmbedToolInCaptionArea'</returns>
			/// <param name="tool">The tool instance whose display mode is being queried.</param>
			protected override ToolMenuItemDisplayMode GetMenuItemDisplayMode(MaskedEditorTool tool)
			{
				return ToolMenuItemDisplayMode.EmbedToolInCaptionArea;
			}

			#endregion //GetMenuItemDisplayMode	

			// JM 11-08-07
			#region GetRecentItemFocusBehavior

			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			#endregion //GetRecentItemFocusBehavior
    
			#region RaiseToolEvent

			/// <summary>
			/// Called by the <b>Ribbon</b> to raise one of the common tool events. 
			/// </summary>
			/// <remarks>
			/// <para class="body">This method will be called to raise a commmon tool event, e.g. <see cref="MaskedEditorTool.Cloned"/>, <see cref="MaskedEditorTool.CloneDiscarded"/>.</para>
			/// <para class="note"><b>Note:</b> the implementation of this method calls a protected virtual method named <see cref="MaskedEditorTool.OnRaiseToolEvent"/> that simply calls the RaiseEvent method. This allows derived classes the opportunity of adding custom logic.</para>
			/// </remarks>
			/// <param name="sourceTool">The tool for which the event should be raised.</param>
			/// <param name="args">The event arguments</param>
			/// <seealso cref="XamRibbon"/>
			/// <seealso cref="ToolClonedEventArgs"/>
			/// <seealso cref="ToolCloneDiscardedEventArgs"/>
			protected override void RaiseToolEvent(MaskedEditorTool sourceTool, RoutedEventArgs args)
			{
				sourceTool.OnRaiseToolEvent(args);
			}

			#endregion //RaiseToolEvent	
    
		}
		#endregion //MaskedEditorToolProxy
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