using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Ribbon.Events;
using Infragistics.Windows.Ribbon.Internal;
using System.Windows.Input;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// The ButtonTool is a push button type tool that fires a click event when pushed.  Since the tool is derived from the WPF ButtonBase element, 
	/// the usage semantics of the tool are the same as a WPF Button.
	/// </summary>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,                GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,                  GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,               GroupName = VisualStateUtilities.GroupCommon)]

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
    public class ButtonTool : ButtonBase,
							  IRibbonTool
	{
        #region Private Members


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


        #endregion //Private Members	
    
		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="ButtonTool"/> class.
		/// </summary>
		public ButtonTool()
		{
		}

		static ButtonTool()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonTool), new FrameworkPropertyMetadata(typeof(ButtonTool)));

			RibbonGroup.MaximumSizeProperty.OverrideMetadata(typeof(ButtonTool), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextNormalBox));
			FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(ButtonTool), new FrameworkPropertyMetadata(new Style()));
			ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(ButtonTool), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

			// AS 1/2/08
			ButtonTool.ContentProperty.OverrideMetadata(typeof(ButtonTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(ButtonTool.OnContentChanged)));

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            XamRibbon.IsActiveProperty.OverrideMetadata(typeof(ButtonTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), XamRibbon.IsActivePropertyKey);
            XamRibbon.LocationProperty.OverrideMetadata(typeof(ButtonTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), XamRibbon.LocationPropertyKey);
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(ButtonTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		#endregion //Constructor

		#region Base Class Overrides

			#region OnAccessKey
		/// <summary>
		/// Responds when the System.Windows.Controls.AccessText.AccessKey for this control is invoked.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnAccessKey(AccessKeyEventArgs e)
		{
			base.OnAccessKey(e);

			// AS 9/17/09 TFS20559
			// Focus is not within the button so when the OnClick of the button is invoked, the 
			// the containing menu is not closed because focus is not shifted out of the ribbon.
			//
			if (!e.IsMultiple)
				XamRibbon.TransferFocusOutOfRibbon(this);
		} 
			#endregion //OnAccessKey

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	

			#region OnClick
		/// <summary>
		/// Invoked when the button has been clicked.
		/// </summary>
		protected override void OnClick()
		{
			base.OnClick();

			// AS 10/18/07
			XamRibbon.TransferFocusOutOfRibbonHelper(this);
		} 
			#endregion //OnClick

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="ButtonTool"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.ButtonToolAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ButtonToolAutomationPeer(this);
        }
            #endregion

			#region OnKeyDown

		/// <summary>
		/// Called when the element has input focus and a key is pressed.
		/// </summary>
		/// <param name="e">An instance of KeyEventArgs that contains information about the key that was pressed.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			base.OnKeyDown(e);
		}

			#endregion //OnKeyDown	
    
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

			#region ToString

		/// <summary>
		/// Returns a string representation of the tool.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string caption = this.Caption;

			if (false == string.IsNullOrEmpty(caption))
				return caption;

			if (this.Content is string)
				caption = this.Content as string;

			return base.ToString();
		}

			#endregion //ToString
    
		#endregion //Base Class Overrides

		#region Common Tool Properties

			#region Caption

		/// <summary>
		/// Identifies the Caption dependency property.
		/// </summary>
		/// <seealso cref="Caption"/>
		/// <seealso cref="HasCaptionProperty"/>
		/// <seealso cref="HasCaption"/>
		public static readonly DependencyProperty CaptionProperty = RibbonToolHelper.CaptionProperty.AddOwner(typeof(ButtonTool));

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
				return (string)this.GetValue(ButtonTool.CaptionProperty);
			}
			set
			{
				this.SetValue(ButtonTool.CaptionProperty, value);
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
		public static readonly DependencyProperty HasCaptionProperty = RibbonToolHelper.HasCaptionProperty.AddOwner(typeof(ButtonTool));

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
				return (bool)this.GetValue(ButtonTool.HasCaptionProperty);
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
		public static readonly DependencyProperty HasImageProperty = RibbonToolHelper.HasImageProperty.AddOwner(typeof(ButtonTool));

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
				return (bool)this.GetValue(ButtonTool.HasImageProperty);
			}
		}

				#endregion //HasImage

			#region Id

		/// <summary>
		/// Identifies the Id dependency property.
		/// </summary>
		/// <seealso cref="Id"/>
		public static readonly DependencyProperty IdProperty = RibbonToolHelper.IdProperty.AddOwner(typeof(ButtonTool));

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
				return (string)this.GetValue(ButtonTool.IdProperty);
			}
			set
			{
				this.SetValue(ButtonTool.IdProperty, value);
			}
		}

			#endregion //Id

			#region ImageResolved

		/// <summary>
		/// Identifies the <see cref="ImageResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageResolvedProperty = RibbonToolHelper.ImageResolvedProperty.AddOwner(typeof(ButtonTool));

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
				return (ImageSource)this.GetValue(ButtonTool.ImageResolvedProperty);
			}
		}

			#endregion //ImageResolved

			#region IsActive

		/// <summary>
		/// Identifies the IsActive dependency property.
		/// </summary>
		/// <seealso cref="IsActive"/>
		public static readonly DependencyProperty IsActiveProperty = XamRibbon.IsActiveProperty.AddOwner(typeof(ButtonTool));

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
				return (bool)this.GetValue(ButtonTool.IsActiveProperty);
			}
		}

			#endregion //IsActive

			#region IsQatCommonTool

		/// <summary>
		/// Identifies the IsQatCommonTool dependency property.
		/// </summary>
		/// <seealso cref="IsQatCommonTool"/>
		public static readonly DependencyProperty IsQatCommonToolProperty = RibbonToolHelper.IsQatCommonToolProperty.AddOwner(typeof(ButtonTool));

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
				return (bool)this.GetValue(ButtonTool.IsQatCommonToolProperty);
			}
			set
			{
				this.SetValue(ButtonTool.IsQatCommonToolProperty, value);
			}
		}

			#endregion //IsQatCommonTool

			#region IsOnQat

		/// <summary>
		/// Identifies the IsOnQat dependency property.
		/// </summary>
		/// <seealso cref="IsOnQat"/>
		public static readonly DependencyProperty IsOnQatProperty = RibbonToolHelper.IsOnQatProperty.AddOwner(typeof(ButtonTool));

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
				return (bool)this.GetValue(ButtonTool.IsOnQatProperty);
			}
		}

			#endregion //IsOnQat

			#region KeyTip

		/// <summary>
		/// Identifies the KeyTip dependency property.
		/// </summary>
		/// <seealso cref="KeyTip"/>
		public static readonly DependencyProperty KeyTipProperty = RibbonToolHelper.KeyTipProperty.AddOwner(typeof(ButtonTool));

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
				return (string)this.GetValue(ButtonTool.KeyTipProperty);
			}
			set
			{
				this.SetValue(ButtonTool.KeyTipProperty, value);
			}
		}

			#endregion //KeyTip

			#region LargeImage

		/// <summary>
		/// Identifies the LargeImage dependency property.
		/// </summary>
		/// <seealso cref="LargeImage"/>
		public static readonly DependencyProperty LargeImageProperty = RibbonToolHelper.LargeImageProperty.AddOwner(typeof(ButtonTool));

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
				return (ImageSource)this.GetValue(ButtonTool.LargeImageProperty);
			}
			set
			{
				this.SetValue(ButtonTool.LargeImageProperty, value);
			}
		}

			#endregion //LargeImage

			#region Location

		/// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		/// <seealso cref="Location"/>
		public static readonly DependencyProperty LocationProperty = XamRibbon.LocationProperty.AddOwner(typeof(ButtonTool));

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
				return (ToolLocation)this.GetValue(ButtonTool.LocationProperty);
			}
		}

			#endregion //Location

			#region SizingMode

		/// <summary>
		/// Identifies the SizingMode dependency property.
		/// </summary>
		/// <seealso cref="SizingMode"/>
		public static readonly DependencyProperty SizingModeProperty = RibbonToolHelper.SizingModeProperty.AddOwner(typeof(ButtonTool));

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
				return (RibbonToolSizingMode)this.GetValue(ButtonTool.SizingModeProperty);
			}
		}

			#endregion //SizingMode

			#region SmallImage

		/// <summary>
		/// Identifies the SmallImage dependency property.
		/// </summary>
		/// <seealso cref="SmallImage"/>
		public static readonly DependencyProperty SmallImageProperty = RibbonToolHelper.SmallImageProperty.AddOwner(typeof(ButtonTool));

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
				return (ImageSource)this.GetValue(ButtonTool.SmallImageProperty);
			}
			set
			{
				this.SetValue(ButtonTool.SmallImageProperty, value);
			}
		}

			#endregion //SmallImage

		#endregion //Common Tool Properties

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
		internal static readonly RoutedEvent ActivatedEvent = MenuTool.ActivatedEvent.AddOwner(typeof(ButtonTool));

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
		public static readonly RoutedEvent ClonedEvent = MenuTool.ClonedEvent.AddOwner(typeof(ButtonTool));

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
		public static readonly RoutedEvent CloneDiscardedEvent = MenuTool.CloneDiscardedEvent.AddOwner(typeof(ButtonTool));

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
		internal static readonly RoutedEvent DeactivatedEvent = MenuTool.DeactivatedEvent.AddOwner(typeof(ButtonTool));

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

                #region VisualState... Methods


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            RibbonToolHelper.SetToolVisualState(this, useTransitions, this.IsActive);

        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ButtonTool tool = target as ButtonTool;

            if ( tool != null )
                tool.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



                #endregion //VisualState... Methods	

			#endregion //Protected methods

			#region Internal Methods

				// AS 9/17/09 TFS20136
				#region AutomationButtonClick
		internal void AutomationButtonClick()
		{
			this.OnClick();
		} 
				#endregion //AutomationButtonClick

				#region BindCommandProperties

		internal static void BindCommandProperties(ToolMenuItem toolMenuItem, ButtonBase tool)
		{
			// AS 7/16/09 TFS19235
			// Moved down. The menu item synchronously calls the canexecute so don't binding it until after the command properties.
			// 
			//toolMenuItem.SetBinding(ToolMenuItem.CommandProperty, Utilities.CreateBindingObject(ButtonBase.CommandProperty, BindingMode.OneWay, tool));
			toolMenuItem.SetBinding(ToolMenuItem.CommandParameterProperty, Utilities.CreateBindingObject(ButtonBase.CommandParameterProperty, BindingMode.OneWay, tool));
			toolMenuItem.SetBinding(ToolMenuItem.CommandTargetProperty, Utilities.CreateBindingObject(ButtonBase.CommandTargetProperty, BindingMode.OneWay, tool));
			// AS 7/16/09 TFS19235
			toolMenuItem.SetBinding(ToolMenuItem.CommandProperty, Utilities.CreateBindingObject(ButtonBase.CommandProperty, BindingMode.OneWay, tool));
		}

				#endregion //BindCommandProperties

				// AS 1/2/08
				// No need to override OnPropertyChanged just to handle the ContentProperty. Its more
				// effecient to just handle that property's changed callback.
				//
				#region OnContentChanged
		internal static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is string)
				d.SetValue(ButtonTool.CaptionProperty, e.NewValue);
		} 
				#endregion //OnContentChanged
    
			#endregion //Internal Methods	
        
		#endregion //Methods

		#region IRibbonTool Members

		RibbonToolProxy IRibbonTool.ToolProxy
		{
			get { return ButtonToolProxy.Instance; }
		}
     
		#endregion

		#region ButtonToolProxy
		/// <summary>
		/// Derived <see cref="RibbonToolProxy"/> for <see cref="ButtonTool"/> instances
		/// </summary>
		protected class ButtonToolProxy : RibbonToolProxy<ButtonTool>
		{
			// AS 5/16/08 BR32980 - See the ToolProxyTests.NoInstanceVariablesOnProxies proxy for details.
			//[ThreadStatic()]
			internal static readonly ButtonToolProxy Instance = new ButtonToolProxy();

			#region Constructor
			static ButtonToolProxy()
			{
				// AS 6/10/08
				// See RibbonToolProxy<T>.Clone. Basically bind the command properties instead of cloning them.
				//
				RibbonToolProxy.RegisterPropertiesToIgnore(typeof(ButtonToolProxy),
					ButtonBase.CommandProperty,
					ButtonBase.CommandTargetProperty,
					ButtonBase.CommandParameterProperty
					);
			} 
			#endregion //Constructor

			#region Bind

			private static DependencyProperty[] _bindProperties = 
			{ 
				ContentControl.ContentProperty,
				// AS 6/10/08
				// See RibbonToolProxy<T>.Clone. Basically bind the command properties instead of cloning them.
				//
				// AS 7/16/09 TFS19235
				// Moved command down.
				//
				//ButtonBase.CommandProperty,
				ButtonBase.CommandTargetProperty,
				ButtonBase.CommandParameterProperty,
				ButtonBase.CommandProperty, // AS 7/16/09 TFS19235
			};

			/// <summary>
			/// Binds properties of the target tool to corresponding properties on the specified 
			/// source tool.  The specific properties that are bound are implementation details of the tool.  Generally, any property that 
			/// represents �tool state� and whose value is changeable and should be shared across instances of a given tool, should be bound 
			/// in tool�s implementation of this interface method.
			/// </summary>
			/// <param name="sourceTool">The tool that this tool is being bound to.</param>
			/// <param name="targetTool">The tool whose properties are being bound to the properties of <paramref name="sourceTool"/></param>
			protected override void Bind(ButtonTool sourceTool, ButtonTool targetTool)
			{
				base.Bind(sourceTool, targetTool);

				RibbonToolProxy.BindToolProperties(_bindProperties, sourceTool, targetTool);
			}

			#endregion //Bind	
    
			#region OnMenuItemClick

			/// <summary>
			/// Called by a <b>ToolMenuItem</b> when it is clicked.
			/// </summary>
			/// <param name="tool">The tool represented by the ToolMenuItem.</param>
			/// <remarks>
			/// <para class="body">The method is called from the <see cref="ToolMenuItem"/>'s OnClick method.</para>
			/// </remarks>
			/// <seealso cref="ToolMenuItem"/>
			protected override void OnMenuItemClick(ButtonTool tool)
			{
				tool.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			}

			#endregion //OnMenuItemClick

			#region PerformAction
			/// <summary>
			/// Performs a tool's default action.
			/// </summary>
			/// <param name="tool">The tool whose action should be performed.</param>
			/// <returns>A boolean indicating whether the action was performed.</returns>
			public override bool PerformAction(ButtonTool tool)
			{
				tool.OnClick();
				return true;
			} 
			#endregion //PerformAction 

			#region PrepareToolMenuItem

			/// <summary>
			/// Prepares the container <see cref="ToolMenuItem"/>to 'host' the tool.
			/// </summary>
			/// <param name="toolMenuItem">The container that wraps the tool.</param>
			/// <param name="tool">The tool that is being wrapped.</param>
			protected override void PrepareToolMenuItem(ToolMenuItem toolMenuItem, ButtonTool tool)
			{
				base.PrepareToolMenuItem(toolMenuItem, tool);

				ButtonTool.BindCommandProperties(toolMenuItem, tool);
			}

			#endregion //PrepareToolMenuItem	
    
			#region RaiseToolEvent

			/// <summary>
			/// Called by the <b>Ribbon</b> to raise one of the common tool events. 
			/// </summary>
			/// <remarks>
			/// <para class="body">This method will be called to raise a commmon tool event, e.g. <see cref="ButtonTool.Cloned"/>, <see cref="ButtonTool.CloneDiscarded"/>.</para>
			/// <para class="note"><b>Note:</b> the implementation of this method calls a protected virtual method named <see cref="ButtonTool.OnRaiseToolEvent"/> that simply calls the RaiseEvent method. This allows derived classes the opportunity of adding custom logic.</para>
			/// </remarks>
			/// <param name="sourceTool">The tool for which the event should be raised.</param>
			/// <param name="args">The event arguments</param>
			/// <seealso cref="XamRibbon"/>
			/// <seealso cref="ToolClonedEventArgs"/>
			/// <seealso cref="ToolCloneDiscardedEventArgs"/>
			protected override void RaiseToolEvent(ButtonTool sourceTool, RoutedEventArgs args)
			{
				sourceTool.OnRaiseToolEvent(args);
			}

			#endregion //RaiseToolEvent	

			#region RetainFocusOnPerformAction
			/// <summary>
			/// Returns false so that the containing popups will be closed when the PerformAction method is called.
			/// </summary>
			public override bool RetainFocusOnPerformAction
			{
				get { return false; }
			}
			#endregion //RetainFocusOnPerformAction
		} 
		#endregion //ButtonToolProxy
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