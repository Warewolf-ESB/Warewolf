using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Infragistics.Windows.Ribbon.Internal;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Globalization;
using Infragistics.Windows.Themes;
using System.Windows.Media;
using System.Collections;
using Infragistics.Shared;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Windows.Automation.Peers;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
    /// A tool that can be placed inside a <see cref="RibbonGroup"/>, <see cref="ApplicationMenu"/>, <see cref="ApplicationMenuFooterToolbar"/> or another <see cref="MenuTool"/>. It contains a set of child tool items, elements that implement the <see cref="IRibbonTool"/> interface. 
	/// </summary>
    /// <remarks>
    /// <para class="body">Tool items, elements that implement the <see cref="IRibbonTool"/> interface, that are added as children of a menu tool will be displayed inside the menu's <see cref="Popup"/> when its <see cref="MenuToolBase.IsOpen"/> property is True.</para>
    /// <para class="body">MenuTools appear as one of 3 types of buttons (see the <see cref="ButtonType"/> property) inside a <see cref="RibbonGroup"/> or <see cref="ApplicationMenuFooterToolbar"/> but will appear as menu items inside another menu.</para>
    /// <para class="note"><b>Note:</b> A MenuTool can optionally contain one, and only one, <see cref="GalleryTool"/>. If the MenuTool is inside a <see cref="RibbonGroup"/> and it contains a <b>GalleryTool</b> and the <see cref="ShouldDisplayGalleryPreview"/> property 
    /// is set to true then a preview of the <see cref="GalleryItem"/>s will be displayed instead of the normal dropdown button.</para>
    /// </remarks>
    /// <seealso cref="MenuToolBase"/>
    /// <seealso cref="ApplicationMenu"/>
    /// <seealso cref="GalleryTool"/>
    /// <seealso cref="ButtonType"/>
    /// <seealso cref="ShouldDisplayGalleryPreview"/>
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class MenuTool : MenuToolBase, IRibbonTool, ICommandSource
	{
		#region Private Members

		private bool		_shouldDisplayGalleryPreview;
		private GalleryTool _galleryTool = null;
		private int			_totalEnabledChildren;
		private ArrayList	_childrenCache;

		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="MenuTool"/>
		/// </summary>
		public MenuTool()
		{
		}

		static MenuTool()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuTool), new FrameworkPropertyMetadata(typeof(MenuTool)));

			XamRibbon.RibbonProperty.OverrideMetadata(typeof(MenuTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRibbonChanged)));
			XamRibbon.LocationPropertyKey.OverrideMetadata(typeof(MenuTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLocationChanged)));
			RibbonToolHelper.SizingModePropertyKey.OverrideMetadata(typeof(MenuTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSizingModeChanged)));

			// AS 11/7/07 BR27990
			MenuTool.SegmentedButtonCommand = new RoutedCommand("SegmentedButton", typeof(MenuTool));
			CommandManager.RegisterClassCommandBinding(typeof(MenuTool), new CommandBinding(MenuTool.SegmentedButtonCommand, new ExecutedRoutedEventHandler(MenuTool.OnExecuteCommand), new CanExecuteRoutedEventHandler(MenuTool.OnCanExecuteCommand)));

			// AS 10/16/09 TFS23117
			// Moved to the base class since this should be applicable to ApplicationMenu as well.
			//
			//// JM 02-03-09 TFS9245
			//ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(MenuTool), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceToolTipServiceIsEnabled)));
		}

		#endregion //Constructors

		#region Commands

		// AS 11/7/07 BR27990
		/// <summary>
		/// Invokes the action associated with the button portion of a segmented menu.
		/// </summary>
		public static readonly RoutedCommand SegmentedButtonCommand;

		#endregion //Commands

		#region Base Class Overrides
 
			#region InitializeMenuToolPresenter

		/// <summary>
		/// Called to initialize the instance of a ToolMenuItem returned from <see cref="MenuToolBase.CreateMenuToolPresenter"/>.
		/// </summary>
		/// <param name="menuToolPresenter">The menuToolPresenter returned from CreateMenuToolPresenter.</param>
		/// <seealso cref="MenuToolBase.CreateMenuToolPresenter"/>
		protected override void InitializeMenuToolPresenter(ToolMenuItem menuToolPresenter)
		{
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			MenuTool.BindCommandProperties(menuToolPresenter, this);

			// bind the IsMenuButtonAreaEnabled to our isEnabled property
			// Note: this value is coerced based on the number of enabled items in the menu
			menuToolPresenter.SetBinding(MenuToolPresenter.IsMenuButtonAreaEnabledProperty, Utilities.CreateBindingObject(IsEnabledProperty, BindingMode.OneWay, this));

			this.BindIsSegmented(menuToolPresenter);
		}

			#endregion //InitializeMenuToolPresenter	
   
			#region MeasureOverride

		/// <summary>
		/// Called to measure the element and its children.
		/// </summary>
		/// <param name="constraint"></param>
		/// <returns>The size that reflects the available size that this element can give to its children.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			return base.MeasureOverride(constraint);
		}

			#endregion //MeasureOverride

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.VerifyOnlyOneGalleryTool(false);
			
			this.VerifyChildrenCache();
			
			this.SynchronizePreviewInPresenter();
		}

			#endregion //OnApplyTemplate

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="MenuTool"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.MenuToolAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MenuToolAutomationPeer(this);
        }
            #endregion

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
					case NotifyCollectionChangedAction.Remove:
                    // JJD 11/07/07
                    // added isFirst/IsLast properties on GalleryToolDropDownPresenter to support triggering of separators
                    // so we have to call this.VerifyOnlyOneGalleryTool even on a remove
                    //this.VerifyChildrenCache();
                        //break;
					case NotifyCollectionChangedAction.Add:
					case NotifyCollectionChangedAction.Replace:
					case NotifyCollectionChangedAction.Reset:
						this.VerifyOnlyOneGalleryTool(true);
						this.VerifyChildrenCache();
						break;
                    // JJD 11/07/07
                    // added isFirst/IsLast properties on GalleryToolDropDownPresenter to support triggering of separators
                    // so we have to call this.VerifyOnlyOneGalleryTool on a Move
                    case NotifyCollectionChangedAction.Move:
						this.VerifyOnlyOneGalleryTool(true);
						break;
				}
			}
		}

			#endregion //OnItemsChanged	

            // JJD 11/20/07 - BR27066
            #region OnPropertyChanged

        /// <summary>
        /// Invoked when a property on the object has been changed.
        /// </summary>
        /// <param name="e">Provides information about the property that was changed.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == MenuToolBase.IsOpenProperty)
            {
                // JJD 11/20/07 - BR27066
                // Let the gallery tool know that the menu has closed
                if (this._galleryTool != null &&
                    (bool)e.NewValue == false)
                    this._galleryTool.OnMenuClosed();


				// AS 10/16/09 TFS23117
				// Moved to the base class since this should be applicable to ApplicationMenu as well.
				//
				//// JM 02-03-09 TFS9245 - Ensure that the MenuTool's Tooltip (if any) does not show when the mouse is over the MenuTool's popup (i.e.
				////						 when it's open)
				//this.CoerceValue(ToolTipService.IsEnabledProperty);
			}
        }

            #endregion //OnPropertyChanged	

    
		#endregion //Base Class Overrides

		#region Events

			#region Checked

		/// <summary>
		/// Event ID for the <see cref="Checked"/> routed event
		/// </summary>
		/// <seealso cref="Checked"/>
		/// <seealso cref="OnChecked"/>
		public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuTool));

		/// <summary>
		/// Occurs after the tool has been checked
		/// </summary>
		/// <seealso cref="Checked"/>
		/// <seealso cref="CheckedEvent"/>
		protected virtual void OnChecked(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseChecked(RoutedEventArgs args)
		{
			args.RoutedEvent = MenuTool.CheckedEvent;
			args.Source = this;
			this.OnChecked(args);
		}

		/// <summary>
		/// Occurs after the tool has been checked
		/// </summary>
		/// <seealso cref="OnChecked"/>
		/// <seealso cref="CheckedEvent"/>
		//[Description("Occurs after this tool has been checked")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler Checked
		{
			add
			{
				base.AddHandler(MenuTool.CheckedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.CheckedEvent, value);
			}
		}

			#endregion //Checked

			#region Click

		/// <summary>
		/// Event ID for the <see cref="Click"/> routed event
		/// </summary>
		/// <seealso cref="Click"/>
		/// <seealso cref="OnClick"/>
		public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuTool));

		/// <summary>
		/// Occurs after the tool has been checked
		/// </summary>
		/// <seealso cref="Click"/>
		/// <seealso cref="ClickEvent"/>
		protected virtual void OnClick(RoutedEventArgs args)
		{
			
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

			this.RaiseClick(true, args);
		}

		/// <summary>
		/// Occurs after the tool has been checked
		/// </summary>
		/// <seealso cref="OnClick"/>
		/// <seealso cref="ClickEvent"/>
		//[Description("Occurs when the button portion of the segmented button has been clicked.")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler Click
		{
			add
			{
				base.AddHandler(MenuTool.ClickEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.ClickEvent, value);
			}
		}

			#endregion //Click

			#region Unchecked

		/// <summary>
		/// Event ID for the <see cref="Unchecked"/> routed event
		/// </summary>
		/// <seealso cref="Unchecked"/>
		public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuTool));

		/// <summary>
		/// Occurs after the tool has been unchecked
		/// </summary>
		/// <seealso cref="Unchecked"/>
		/// <seealso cref="UncheckedEvent"/>
		protected virtual void OnUnchecked(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseUnchecked(RoutedEventArgs args)
		{
			args.RoutedEvent = MenuTool.UncheckedEvent;
			args.Source = this;
			this.OnUnchecked(args);
		}

		/// <summary>
		/// Occurs after the tool has been unchecked
		/// </summary>
		/// <seealso cref="OnUnchecked"/>
		/// <seealso cref="UncheckedEvent"/>
		//[Description("Occurs after this tool has been unchecked")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler Unchecked
		{
			add
			{
				base.AddHandler(MenuTool.UncheckedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.UncheckedEvent, value);
			}
		}

			#endregion //Unchecked

		#endregion //Events

		#region Properties

			#region Public Properties

				#region ButtonType

		/// <summary>
		/// Identifies the <see cref="ButtonType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ButtonTypeProperty = DependencyProperty.Register("ButtonType",
			typeof(MenuToolButtonType), typeof(MenuTool), new FrameworkPropertyMetadata(MenuToolButtonType.DropDown));

		/// <summary>
		/// Gets/sets how this tool is used. Either as a single drop down or segmented into a dropdown area and a button area
		/// </summary>
		/// <seealso cref="ButtonTypeProperty"/>
		//[Description("Gets/sets how this tool is used. Either as a single drop down or segmented into a dropdown area and a button area")]
		//[Category("Ribbon Properties")]
		public MenuToolButtonType ButtonType
		{
			get
			{
				return (MenuToolButtonType)this.GetValue(MenuTool.ButtonTypeProperty);
			}
			set
			{
				this.SetValue(MenuTool.ButtonTypeProperty, value);
			}
		}

				#endregion //ButtonType

				#region Command

		/// <summary>
		/// Identifies the <see cref="Command"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
			typeof(ICommand), typeof(MenuTool), new FrameworkPropertyMetadata(null, 
				new PropertyChangedCallback(OnCommandChanged) 
				));

		// AS 3/24/10 TFS27676
		// We need to hook the CanExecuteChanged event of the associated command.
		//

		// since routedcommands hold onto the delegate via a weakreference we have to hold a strong
		// reference to the delegate like elements like the MenuItem/ButtonBase do. i don't have to 
		// do this but if the command is a custom icommand and it decides to mimic the routedcommand 
		// impl but not use the routed command impl (i.e. commandmanager) then we need to hold a 
		// strong reference to it. since we're using menuitems to represent the tools in the menu 
		// and the menu itself this poses no additional issue then would be present right now
		private EventHandler _canExecuteHandler;

		private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MenuTool menu = d as MenuTool;

			ICommand oldCommand = e.OldValue as ICommand;
			ICommand newCommand = e.NewValue as ICommand;

			if (null != oldCommand && menu._canExecuteHandler != null)
				oldCommand.CanExecuteChanged -= menu._canExecuteHandler;

			if (null == newCommand)
				menu._canExecuteHandler = null;
			else
			{
				menu._canExecuteHandler = new EventHandler(menu.OnCanExecuteChanged);
				newCommand.CanExecuteChanged += menu._canExecuteHandler;
			}

			CommandManager.InvalidateRequerySuggested();
		}

		// AS 3/24/10 TFS27676
		private void OnCanExecuteChanged(object sender, EventArgs e)
		{
			// to try to limit the scope with which we will ask the command manager
			// to requery only do it if the associated command state has changed. also 
			// if the command were a routed command then this may get invoked when someone
			// had called InvalidateRequerySuggested
			bool? lastCanExecute = _lastCanExecuteState;

			if (lastCanExecute != this.CanExecuteSegmentedButton())
				CommandManager.InvalidateRequerySuggested();

			// JM 06-20-10 TFS26088
			MenuToolPresenter mtp = this.Presenter as MenuToolPresenter;
			if (null != mtp)
				mtp.CoerceValue(MenuToolPresenter.IsMenuButtonAreaEnabledProperty);
		}

		/// <summary>
		/// Gets or sets the command to invoke when the button portion of the menu tool is pressed.
		/// </summary>
		/// <seealso cref="CommandProperty"/>
		//[Description("Gets or sets the command to invoke when the button portion of the menu tool is pressed.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[Localizability(LocalizationCategory.NeverLocalize)]
		public ICommand Command
		{
			get
			{
				return (ICommand)this.GetValue(MenuTool.CommandProperty);
			}
			set
			{
				this.SetValue(MenuTool.CommandProperty, value);
			}
		}

				#endregion //Command

				#region CommandParameter

		/// <summary>
		/// Identifies the <see cref="CommandParameter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
			typeof(object), typeof(MenuTool), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the parameter to pass to the Command property. 
		/// </summary>
		/// <seealso cref="CommandParameterProperty"/>
		//[Description("Gets or sets the parameter to pass to the Command property. ")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[Localizability(LocalizationCategory.NeverLocalize)]
		public object CommandParameter
		{
			get
			{
				return (object)this.GetValue(MenuTool.CommandParameterProperty);
			}
			set
			{
				this.SetValue(MenuTool.CommandParameterProperty, value);
			}
		}

				#endregion //CommandParameter

				#region CommandTarget

		/// <summary>
		/// Identifies the <see cref="CommandTarget"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget",
			typeof(IInputElement), typeof(MenuTool), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the element on which to raise the specified command.
		/// </summary>
		/// <seealso cref="CommandTargetProperty"/>
		//[Description("Gets or sets the element on which to raise the specified command.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public IInputElement CommandTarget
		{
			get
			{
				return (IInputElement)this.GetValue(MenuTool.CommandTargetProperty);
			}
			set
			{
				this.SetValue(MenuTool.CommandTargetProperty, value);
			}
		}

				#endregion //CommandTarget

				#region IsChecked

		/// <summary>
		/// Identifies the <see cref="IsChecked"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked",
			typeof(bool), typeof(MenuTool), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, new PropertyChangedCallback(OnIsCheckedChanged)));

		private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MenuTool menu = (MenuTool)d;

			if (menu != null)
			{
				bool newValue = (bool)e.NewValue;

				if (newValue == true)
					menu.RaiseChecked(new RoutedEventArgs());
				else 
					menu.RaiseUnchecked(new RoutedEventArgs());

                // TK Raise property change event
                bool? newVal = (bool?)e.NewValue;
                bool? oldVal = (bool?)e.OldValue;
                menu.RaiseAutomationToggleStatePropertyChanged(oldVal, newVal);
			}
		}

		/// <summary>
		/// Gets/sets whether the tool is checked. 
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless the <see cref="ButtonType"/> is set to �SegmentedState�. Changing its value will cause either the <see cref="Checked"/> or <see cref="Unchecked"/> events to be raised.</para>
		/// </remarks>
		/// <seealso cref="IsCheckedProperty"/>
		//[Description("Gets/sets whether the tool is checked. ")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[DefaultValue(false)]
		public bool IsChecked
		{
			get
			{
				return (bool)this.GetValue(MenuTool.IsCheckedProperty);
			}
			set
			{
				this.SetValue(MenuTool.IsCheckedProperty, value);
			}
		}

				#endregion //IsChecked

				#region KeyTipForSegmentedButton

		/// <summary>
		/// Identifies the <see cref="KeyTipForSegmentedButton"/> dependency property
		/// </summary>
		public static readonly DependencyProperty KeyTipForSegmentedButtonProperty = DependencyProperty.Register("KeyTipForSegmentedButton",
			typeof(string), typeof(MenuTool), new FrameworkPropertyMetadata(null), new ValidateValueCallback(RibbonToolHelper.ValidateKeyTip));

		/// <summary>
		/// Gets/sets a string with a maximum of 3 characters that represents the mnemonic that is displayed for the button portion of the tool.  Note:  this property is ignored unless the ButtonType is set to �Segmented� or �SegmentedState�.  When the mnemonic is entered the Click event will be raised.  The Checked or Unchecked events will also be raised for the �SegmentedState� button type.
		/// </summary>
		/// <seealso cref="KeyTipForSegmentedButtonProperty"/>
		//[Description("Gets/sets a string with a maximum of 3 characters that represents the mnemonic that is displayed for the button portion of the tool.  Note:  this property is ignored unless the ButtonType is set to �Segmented� or �SegmentedState�.  When the mnemonic is entered the Click event will be raised.  The Checked or Unchecked events will also be raised for the �SegmentedState� button type.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string KeyTipForSegmentedButton
		{
			get
			{
				return (string)this.GetValue(MenuTool.KeyTipForSegmentedButtonProperty);
			}
			set
			{
				this.SetValue(MenuTool.KeyTipForSegmentedButtonProperty, value);
			}
		}

				#endregion //KeyTipForSegmentedButton

				#region MenuItemDropDownArrowStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> targeting the <see cref="Control"/> class that is used to represents the dropdown arrow for a <see cref="MenuTool"/> when sited on a menu.
		/// </summary>
		public static readonly ResourceKey MenuItemDropDownArrowStyleKey = new StaticPropertyResourceKey(typeof(MenuTool), "MenuItemDropDownArrowStyleKey");

				#endregion //MenuItemDropDownArrowStyleKey

				#region MenuToolDropDownArrowStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> targeting the <see cref="Control"/> class that is used to represents the dropdown arrow for a <see cref="MenuTool"/> when sited on a toolbar or in a <see cref="RibbonGroup"/>.
		/// </summary>
		public static readonly ResourceKey MenuToolDropDownArrowStyleKey = new StaticPropertyResourceKey(typeof(MenuTool), "MenuToolDropDownArrowStyleKey");

				#endregion //MenuToolDropDownArrowStyleKey

				#region QuickCustomizeMenuDropDownArrowStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> targeting the <see cref="Control"/> class that is used to represents the dropdown arrow for the quick customize menu of the <see cref="QuickAccessToolbar.QuickCustomizeMenu"/>.
		/// </summary>
		public static readonly ResourceKey QuickCustomizeMenuDropDownArrowStyleKey = new StaticPropertyResourceKey(typeof(MenuTool), "QuickCustomizeMenuDropDownArrowStyleKey");

				#endregion //QuickCustomizeMenuDropDownArrowStyleKey

				#region ShouldDisplayGalleryPreview

		/// <summary>
		/// Identifies the <see cref="ShouldDisplayGalleryPreview"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldDisplayGalleryPreviewProperty = DependencyProperty.Register("ShouldDisplayGalleryPreview",
			typeof(bool), typeof(MenuTool), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure, new PropertyChangedCallback(OnShouldDisplayGalleryPreviewChanged)));

		private static void OnShouldDisplayGalleryPreviewChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuTool menuTool = target as MenuTool;

			if (menuTool != null)
			{
				menuTool._shouldDisplayGalleryPreview = (bool)e.NewValue;
				menuTool.SynchronizePreviewInPresenter();
			}
		}
		
		/// <summary>
		/// Gets/sets whether a gallery tool will display its items in the gallery preview area of the menu when it is displayed on a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <seealso cref="ShouldDisplayGalleryPreviewProperty"/>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="RibbonGroup"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.MenuToolPresenter"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.MenuToolPresenter.PreviewGalleryVisibility"/>
		//[Description("Gets/sets whether a gallery tool will display its items in the gallery preview area of the menu when it is displayed on a RibbonGroup")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool ShouldDisplayGalleryPreview
		{
			get
			{
				return (bool)this.GetValue(MenuTool.ShouldDisplayGalleryPreviewProperty);
			}
			set
			{
				this.SetValue(MenuTool.ShouldDisplayGalleryPreviewProperty, value);
			}
		}

				#endregion //ShouldDisplayGalleryPreview

			#endregion //Public Properties

			#region Common Tool Properties

				#region HasImage

		/// <summary>
		/// Identifies the HasImage dependency property.
		/// </summary>
		/// <seealso cref="HasImage"/>
		/// <seealso cref="SmallImageProperty"/>
		/// <seealso cref="SmallImage"/>
		public static readonly DependencyProperty HasImageProperty = RibbonToolHelper.HasImageProperty.AddOwner(typeof(MenuTool));

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
				return (bool)this.GetValue(MenuTool.HasImageProperty);
			}
		}

				#endregion //HasImage

				#region ImageResolved

		/// <summary>
		/// Identifies the <see cref="ImageResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageResolvedProperty = RibbonToolHelper.ImageResolvedProperty.AddOwner(typeof(MenuTool));

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
				return (ImageSource)this.GetValue(MenuTool.ImageResolvedProperty);
			}
		}

				#endregion //ImageResolved

				#region LargeImage

		/// <summary>
		/// Identifies the LargeImage dependency property.
		/// </summary>
		/// <seealso cref="LargeImage"/>
		public static readonly DependencyProperty LargeImageProperty = RibbonToolHelper.LargeImageProperty.AddOwner(typeof(MenuTool));

		/// <summary>
		/// Returns/sets the 32x32 image to be used when the tool is sized to ImageAndTextLarge.
		/// </summary>
		/// <seealso cref="LargeImageProperty"/>
		/// <seealso cref="SmallImage"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.MenuToolBase.SizingMode"/>
		/// <seealso cref="RibbonToolSizingMode"/>
		//[Description("Returns/sets the 32x32 image to be used when the tool is sized to ImageAndTextLarge.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ImageSource LargeImage
		{
			get
			{
				return (ImageSource)this.GetValue(MenuTool.LargeImageProperty);
			}
			set
			{
				this.SetValue(MenuTool.LargeImageProperty, value);
			}
		}

				#endregion //LargeImage

				#region SmallImage

		/// <summary>
		/// Identifies the SmallImage dependency property.
		/// </summary>
		/// <seealso cref="SmallImage"/>
		public static readonly DependencyProperty SmallImageProperty = RibbonToolHelper.SmallImageProperty.AddOwner(typeof(MenuTool), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the 16x16 image to be used when the tool is sized to ImageOnly or ImageAndTextNormal.
		/// </summary>
		/// <seealso cref="SmallImageProperty"/>
		/// <seealso cref="LargeImage"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.MenuToolBase.SizingMode"/>
		/// <seealso cref="RibbonToolSizingMode"/>
		//[Description("Returns/sets the 16x16 image to be used when the tool is sized to ImageOnly or ImageAndTextNormal.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ImageSource SmallImage
		{
			get
			{
				return (ImageSource)this.GetValue(MenuTool.SmallImageProperty);
			}
			set
			{
				this.SetValue(MenuTool.SmallImageProperty, value);
			}
		}

				#endregion //SmallImage

			#endregion //Common Tool Properties

			#region Internal properties

				#region HasGalleryPreview
		internal bool HasGalleryPreview
		{
			get
			{
				return this._shouldDisplayGalleryPreview && this._galleryTool != null;
			}
		} 
				#endregion //HasGalleryPreview

				#region HideGalleryPreview

		internal static readonly DependencyProperty HideGalleryPreviewProperty = DependencyProperty.Register("HideGalleryPreview",
			typeof(bool), typeof(MenuTool), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnHideGalleryPreviewCallback)));

		private static void OnHideGalleryPreviewCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MenuTool menu = d as MenuTool;

			menu.SynchronizePreviewInPresenter();
		}

				#endregion //HideGalleryPreview

				#region IsGalleryPreviewVisible
		internal bool IsGalleryPreviewVisible
		{
			get
			{
				if (this.HasGalleryPreview)
				{
					MenuToolPresenter mtp = this.MenuToolPresenter;

					if (null != mtp)
						return mtp.PreviewGalleryVisibility != Visibility.Collapsed;
				}

				return false;
			}
		} 
				#endregion //IsGalleryPreviewVisible

				#region MenuToolPresenter

		internal MenuToolPresenter MenuToolPresenter
		{
			get { return this.Presenter as MenuToolPresenter; }
		}

				#endregion //MenuToolPresenter

				#region TotalEnabledChildren

		internal int TotalEnabledChildren { get { return this._totalEnabledChildren; } }

				#endregion //TotalEnabledChildren	
    
			#endregion //Internal properties	
        
			#region Private Properties

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				// AS 11/13/07 BR27990
				#region BindCommandProperties

		internal static void BindCommandProperties(ToolMenuItem toolMenuItem, MenuTool tool)
		{
			if (tool != null && tool.ButtonType == MenuToolButtonType.DropDown)
			{
				// AS 7/16/09 TFS19235
				// Moved down. The menu item synchronously calls the canexecute so don't binding it until after the command properties.
				// 
				//toolMenuItem.SetBinding(ToolMenuItem.CommandProperty, Utilities.CreateBindingObject(MenuTool.CommandProperty, BindingMode.OneWay, tool));
				toolMenuItem.SetBinding(ToolMenuItem.CommandParameterProperty, Utilities.CreateBindingObject(MenuTool.CommandParameterProperty, BindingMode.OneWay, tool));
				toolMenuItem.SetBinding(ToolMenuItem.CommandTargetProperty, Utilities.CreateBindingObject(MenuTool.CommandTargetProperty, BindingMode.OneWay, tool));
				// AS 7/16/09 TFS19235
				toolMenuItem.SetBinding(ToolMenuItem.CommandProperty, Utilities.CreateBindingObject(MenuTool.CommandProperty, BindingMode.OneWay, tool));
			}
			else
			{
				BindingOperations.ClearBinding(toolMenuItem, ToolMenuItem.CommandProperty);
				BindingOperations.ClearBinding(toolMenuItem, ToolMenuItem.CommandTargetProperty);
				BindingOperations.ClearBinding(toolMenuItem, ToolMenuItem.CommandParameterProperty);
			}
		}

				#endregion //BindCommandProperties

				// AS 11/7/07 BR27990
				#region CanExecuteSegmentedButton

		// AS 3/24/10 TFS27676
		private bool? _lastCanExecuteState;

		internal bool CanExecuteSegmentedButton()
		{
			// AS 3/24/10 TFS27676
			// Store the state so we can limit the scope under which we will 
			// raise the notification.
			//
			_lastCanExecuteState = CanExecuteSegmentedButtonImpl();
			return _lastCanExecuteState.Value;
		}

		// AS 3/24/10 TFS27676
		// Moved implementation from the CanExecuteSegmentedButton to
		// make it easier to track the state.
		//
		private bool CanExecuteSegmentedButtonImpl()
		{
			if (this.ButtonType == MenuToolButtonType.DropDown)
				return false;

			// AS 11/29/10 TFS59306
			// If this is a routed command then use its CanExecute overload so it can route.
			//
			//if (this.Command != null && false == this.Command.CanExecute(this.CommandParameter))
			//	return false;
			ICommand cmd = this.Command;

			if (null != cmd)
			{
				RoutedCommand rc = cmd as RoutedCommand;
				object param = this.CommandParameter;

				if (rc != null && !rc.CanExecute(param, this.CommandTarget ?? (IInputElement)this.Presenter ?? this))
					return false;
				else if (rc == null && !cmd.CanExecute(param))
					return false;
			}

			// AS 11/7/07 BR27990
			// Do not process the method if the element is disabled.
			//
			MenuToolPresenter mtp = this.Presenter as MenuToolPresenter;

			if (null != mtp && mtp.IsMenuButtonAreaEnabled == false)
				return false;

			return true;
		} 
				#endregion //CanExecuteSegmentedButton

				#region InitializePreviewGalleryVisibility
		internal void InitializePreviewGalleryVisibility(bool visible)
		{
			MenuToolPresenter mtp = this.MenuToolPresenter;

			if (null != mtp)
			{
				if (visible)
					mtp.SetValue(MenuToolPresenter.PreviewGalleryVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
				else
					mtp.ClearValue(MenuToolPresenter.PreviewGalleryVisibilityPropertyKey);
			}
		}
				#endregion //InitializePreviewGalleryVisibility 

				#region OnSegmentedButtonClick
		internal void OnSegmentedButtonClick()
		{
			switch (this.ButtonType)
			{
				case MenuToolButtonType.DropDown:
					return;

				case MenuToolButtonType.Segmented:
				case MenuToolButtonType.SegmentedState:
					// invoke the click
					this.OnClick(new RoutedEventArgs(MenuTool.ClickEvent));
					break;
			}
		} 
				#endregion //OnSegmentedButtonClick

				// AS 11/1/07 BR27990
				// Made a helper method since we need to raise the click from the menubuttonarea also.
				//
				#region RaiseClick
		internal void RaiseClick(bool allowToggleState)
		{
			this.RaiseClick(allowToggleState, new RoutedEventArgs(MenuTool.ClickEvent, this));
		}

		private void RaiseClick(bool allowToggleState, RoutedEventArgs args)
		{
			if (allowToggleState && this.ButtonType == MenuToolButtonType.SegmentedState)
				this.OnToggle();

			this.RaiseEvent(args);

			// then reaise the command if there is one
			Utilities.ExecuteCommand(this);

			// AS 10/18/07
			// AS 11/8/07 BR27990
			// This could be a menu within a menu so use the associated menu item first.
			//
			//XamRibbon.TransferFocusOutOfRibbonHelper(this);
			FrameworkElement elementWithFocus = this;

			// don't look at this if this is a root level menu item
			if (this.Presenter == null)
			{
				ToolMenuItem menuItem = MenuTool.GetToolMenuItem(this);

				// if this tool is represented by a menu item that doesn't close on click, don't shift out focus
				if (null != menuItem)
				{
					if (menuItem.StaysOpenOnClick)
						return;

					if (menuItem.IsKeyboardFocusWithin)
						elementWithFocus = menuItem;
				}
			}

			XamRibbon.TransferFocusOutOfRibbonHelper(elementWithFocus);
		} 
				#endregion //RaiseClick

			#endregion //Internal Methods

			#region Private methods

				#region BindCheckState

		private void BindCheckState(ToolMenuItem toolMenuItem)
		{
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			// AS 1/11/12 TFS30681
			// We need to maintain our own IsCheckable state.
			//
			Binding binding = Utilities.CreateBindingObject(MenuTool.ButtonTypeProperty, BindingMode.OneWay, this);
			binding.Converter = ButtonTypeToIsCheckableConverter.Instance;
			toolMenuItem.SetBinding(ToolMenuItem.IsCheckableInternalProperty, binding);

			toolMenuItem.SetBinding(ToolMenuItem.IsCheckedProperty, Utilities.CreateBindingObject(MenuTool.IsCheckedProperty, BindingMode.TwoWay, this));
		}

				#endregion //BindCheckState	

				#region BindIsSegmented

		private void BindIsSegmented(ToolMenuItem toolMenuItem)
		{
			Binding binding = Utilities.CreateBindingObject(MenuTool.ButtonTypeProperty, BindingMode.OneWay, this);
			binding.Converter = ButtonTypeToIsSegmentedConverter.Instance;
			toolMenuItem.SetBinding(ToolMenuItem.IsSegmentedInternalProperty, binding);
		}

				#endregion //BindIsSegmented	
    
				#region ExecuteCommand

		private void ExecuteCommand()
		{
			ICommand cmd = this.Command;

			if (cmd == null)
				return;

			object param = this.CommandParameter;

			IInputElement target = this.CommandTarget;

			if (cmd is RoutedCommand)
			{
				if (target == null)
					target = this;

				if (((RoutedCommand)cmd).CanExecute(param, target))
					((RoutedCommand)cmd).Execute(param, target);

			}
			else
			{
				if (cmd.CanExecute(param))
					cmd.Execute(param);
			}

		}

				#endregion //ExecuteCommand

				#region GetToolOnMenuByTypeAndId
		
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetToolOnMenuByTypeAndId

				// AS 11/7/07 BR27990
				#region OnCanExecuteCommand

		private static void OnCanExecuteCommand(object target, CanExecuteRoutedEventArgs args)
		{
			MenuTool menuTool = target as MenuTool;
			if (menuTool != null)
			{
				if (args.Command == MenuTool.SegmentedButtonCommand)
				{
					args.CanExecute = menuTool.CanExecuteSegmentedButton();
					args.Handled = true;
				}
			}
		}

				#endregion //OnCanExecuteCommand

				#region OnChildElement_IsEnabledChanged

		private void OnChildElement_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			UIElement element = sender as UIElement;

			if (element != null)
			{
				if ((bool)e.NewValue == true)
					this._totalEnabledChildren++;
				else
				{
					this._totalEnabledChildren--;

					Debug.Assert(this._totalEnabledChildren >= 0);
				}

				// AS 11/9/07 BR27990
				if (this.Presenter is MenuToolPresenter)
					this.Presenter.CoerceValue(MenuToolPresenter.IsMenuButtonAreaEnabledProperty);
			}
		}

				#endregion //OnChildElement_IsEnabledChanged	

				// JM 02-03-09 TFS9245
				#region CoerceToolTipServiceIsEnabled

		// AS 10/16/09 TFS23117
		// Moved to the base class since this should be applicable to ApplicationMenu as well.
		//
		//private static object CoerceToolTipServiceIsEnabled(DependencyObject d, object newValue)
		//{
		//    MenuTool menuTool = d as MenuTool;
		//    if (menuTool != null && menuTool.IsOpen)
		//        return false;
		//
		//    return newValue;
		//}

				#endregion //CoerceToolTipServiceIsEnabled

				// AS 11/7/07 BR27990
				#region OnExecuteCommand

		private static void OnExecuteCommand(object target, ExecutedRoutedEventArgs args)
		{
			MenuTool menuTool = target as MenuTool;
			if (menuTool != null)
			{
				if (args.Command == MenuTool.SegmentedButtonCommand)
				{
					menuTool.OnSegmentedButtonClick();
					args.Handled = true;
				}
			}
		}

				#endregion //OnExecuteCommand

				#region OnLocationChanged
		private static void OnLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MenuTool menu = d as MenuTool;

			if (null != menu)
			{
				menu.SynchronizePreviewInPresenter();
			}
		} 
				#endregion //OnLocationChanged

				#region OnRibbonChanged
		private static void OnRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MenuTool menu = d as MenuTool;

			if (null != menu)
			{
				menu.SynchronizePreviewInPresenter();
			}
		} 
				#endregion //OnRibbonChanged

				#region OnSizingModeChanged
		private static void OnSizingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MenuTool menu = d as MenuTool;

			menu.SynchronizePreviewInPresenter();
		} 
				#endregion //OnSizingModeChanged

				#region OnToggle
		private void OnToggle()
		{
			// AS 10/15/07 BR27377
			//bool? isChecked = this.IsChecked;
			//object newValue = isChecked == true ? null : (isChecked == null ? KnownBoxes.FalseBox : KnownBoxes.TrueBox);
			object newValue = this.IsChecked == true ? KnownBoxes.FalseBox : KnownBoxes.TrueBox;
			this.SetValue(IsCheckedProperty, newValue);
		} 
				#endregion //OnToggle

                #region RaiseAutomationExpandCollapseStateChanged

        private void RaiseAutomationToggleStatePropertyChanged(bool? oldValue, bool? newValue)
        {
            MenuToolAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as MenuToolAutomationPeer;

            if (null != peer)
                peer.RaiseToggleStatePropertyChangedEvent(oldValue, newValue);
        }

                #endregion //RaiseAutomationExpandCollapseStateChanged

				#region SynchronizePreviewInPresenter
		private void SynchronizePreviewInPresenter()
		{
			if (this.IsInitialized == false)
				return;

			XamRibbon ribbon = XamRibbon.GetRibbon(this);
			ToolLocation location = XamRibbon.GetLocation(this);

			MenuToolPresenter mtp = this.MenuToolPresenter;

			if (mtp != null)
			{
				if (this.HasGalleryPreview && 
					ribbon != null && 
					location == ToolLocation.Ribbon &&
					false == (bool)this.GetValue(HideGalleryPreviewProperty) && 
					RibbonToolHelper.GetSizingMode(this) == RibbonToolSizingMode.ImageAndTextLarge)
				{
					mtp.GalleryToolForPreview = this._galleryTool;
					mtp.SetValue(MenuToolPresenter.PreviewGalleryVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
				}
				else
				{
					mtp.GalleryToolForPreview = null;
					mtp.ClearValue(MenuToolPresenter.PreviewGalleryVisibilityPropertyKey);
				}
			}
		} 
				#endregion //SynchronizePreviewInPresenter

				#region VerifyChildrenCache

		private void VerifyChildrenCache()
		{

			MenuToolPresenter mtp = this.Presenter as MenuToolPresenter;

			// since we don't need to cache the children for menutools on a menu we can
			// skip this processing if the this.Presenter property is null
			if (mtp == null)
				return;

			ItemCollection items = this.Items;

			int count = items.Count;

			// temporarily allocate a hashtable and populate it with the old cached items
			Hashtable oldCachedItems = new Hashtable();

			if (this._childrenCache != null)
			{
				int oldCount	= this._childrenCache.Count;

				if (oldCount > 0)
				{
					for (int i = 0; i < oldCount; i++)
					{
						object item = this._childrenCache[i];

						oldCachedItems.Add(item, item);
					}

					this._childrenCache.Clear();
				}
			}
			else
				this._childrenCache = new ArrayList();

			int oldTotalEnabledChildren = this._totalEnabledChildren;

			this._totalEnabledChildren = 0;

			for (int i = 0; i < count; i++)
			{
				UIElement element = items[i] as UIElement;

				if (element != null)
				{
					// any old item we can remove from the old cache, 
					// new items we need to wire up their IsEnabledChanged event
					if (oldCachedItems.ContainsKey(element))
						oldCachedItems.Remove(element);
					else
						element.IsEnabledChanged += new DependencyPropertyChangedEventHandler(this.OnChildElement_IsEnabledChanged);

					this._childrenCache.Add(element);

					if (element.IsEnabled)
						this._totalEnabledChildren++;
				}
				else
					this._totalEnabledChildren++;
			}

			count = oldCachedItems.Count;

			// loop over any old cached items remaining and unwire the IsEnabledChanged event
			if (count > 0)
			{
				// JM 10-10-07
				//foreach (UIElement element in oldCachedItems)
				foreach (DictionaryEntry de in oldCachedItems)
				{
					UIElement element = de.Value as UIElement;
					if ( element != null )
						element.IsEnabledChanged -= new DependencyPropertyChangedEventHandler(this.OnChildElement_IsEnabledChanged);
				}
			}

			// let the menutoolpresenter know what the new status is
			if (oldTotalEnabledChildren < 1)
				mtp.OnMenuTool_IsEnabledChanged();
			else
			{
				// we only need to notify the menutoolpresenter if we
				// went from a positive count to zero
				if (this._totalEnabledChildren < 1)
					mtp.OnMenuTool_IsEnabledChanged();
			}
		}

				#endregion //VerifyChildrenCache	

				#region VerifyOnlyOneGalleryTool

		internal void VerifyOnlyOneGalleryTool(bool synchronizeOnChange)
		{
			GalleryTool gtool = null;

			ItemCollection items = this.Items;

			int count= items.Count;

            // JJD 11/07/07
            // added isFirst/IsLast properties on GalleryToolDropDownPresenter to support triggering of separators
            // IsFirst is initilaized to true while isLast starts out as false
            bool    isGalleryFirstInMenu   = true;
            bool    isGalleryLastInMenu    = false;
            int     visibleItemCount = 0;

			for (int i = 0; i < count; i++)
			{
				//object item = items[i];
				UIElement item = items[i] as UIElement;

                if (item == null || item.Visibility != Visibility.Collapsed)
                {
                    if (gtool != null)
                         isGalleryLastInMenu = false;
                    
                    visibleItemCount++;
                }

				if (item is GalleryTool)
				{
					if (gtool != null)
						throw new NotSupportedException(XamRibbon.GetString("LE_OnlyOneGalleryToolAllowed"));

					gtool = item as GalleryTool;

                    // JJD 11/07/07 
                    // init the isLast flag to true
                    isGalleryLastInMenu = true;


                    // JJD 11/07/07 
                    // if this is not the first visible item then set isFirst flag to false
                    if (visibleItemCount > 1)
                        isGalleryFirstInMenu = false;
				}

			}

			if (gtool != this._galleryTool)
			{
                // JJD 11/07/07 
                // clear the first/last properties on the old item
                if (this._galleryTool != null)
                    this._galleryTool.SetFirstLastProps(false, false);
 
                this._galleryTool = gtool;

                // JJD 11/07/07 
                // Set the first/last properties on the new item
                if (this._galleryTool != null)
                    this._galleryTool.SetFirstLastProps(isGalleryFirstInMenu, isGalleryLastInMenu);

				if (synchronizeOnChange )
					this.SynchronizePreviewInPresenter();
			}
		}

				#endregion //VerifyOnlyOneGalleryTool	

			#endregion //Private methods

		#endregion //Methods

		#region IRibbonTool Members

		RibbonToolProxy IRibbonTool.ToolProxy
		{
			get { return MenuToolProxy.Instance; }
		}

		#endregion

		#region MenuToolProxy
		/// <summary>
		/// Derived <see cref="RibbonToolProxy"/> for <see cref="MenuTool"/> instances
		/// </summary>
		protected class MenuToolProxy : MenuToolBaseProxy
		{
			// AS 5/16/08 BR32980 - See the ToolProxyTests.NoInstanceVariablesOnProxies proxy for details.
			//[ThreadStatic()]
			internal static new readonly MenuToolProxy Instance = new MenuToolProxy();

			#region Constructor
			static MenuToolProxy()
			{
				// AS 6/10/08
				// See RibbonToolProxy<T>.Clone. Basically bind the command properties instead of cloning them.
				//
				RibbonToolProxy.RegisterPropertiesToIgnore(typeof(MenuToolProxy), 
					MenuTool.CommandProperty,
					MenuTool.CommandTargetProperty,
					MenuTool.CommandParameterProperty
					);
			} 
			#endregion //Constructor

			#region Bind

			private static DependencyProperty[] _bindProperties = 
			{ 
				MenuTool.IsCheckedProperty, 
				MenuTool.ButtonTypeProperty,
				// AS 6/10/08
				// See RibbonToolProxy<T>.Clone. Basically bind the command properties instead of cloning them.
				//
				// AS 7/16/09 TFS19235
				// Moved command down.
				//
				//MenuTool.CommandProperty,
				MenuTool.CommandTargetProperty,
				MenuTool.CommandParameterProperty,
				MenuTool.CommandProperty, // AS 7/16/09 TFS19235
			};

			/// <summary>
			/// Binds properties of the target tool to corresponding properties on the specified 
			/// source tool.  The specific properties that are bound are implementation details of the tool.  Generally, any property that 
			/// represents �tool state� and whose value is changeable and should be shared across instances of a given tool, should be bound 
			/// in tool�s implementation of this interface method.
			/// </summary>
			/// <param name="sourceTool">The tool that this tool is being bound to.</param>
			/// <param name="targetTool">The tool whose properties are being bound to the properties of <paramref name="sourceTool"/></param>
			protected override void Bind(MenuToolBase sourceTool, MenuToolBase targetTool)
			{
				base.Bind(sourceTool, targetTool);

				RibbonToolProxy.BindToolProperties(_bindProperties, sourceTool, targetTool);
			}

			#endregion //Bind

			#region GetKeyTipProviders
			internal override IKeyTipProvider[] GetKeyTipProviders(MenuToolBase tool, ToolMenuItem menuItem)
			{
				MenuTool menu = tool as MenuTool;

				IKeyTipProvider[] providers;

				if (null != menu && menu.IsGalleryPreviewVisible)
				{
					providers = new IKeyTipProvider[] { new GalleryPreviewKeyTipProvider(menu) };
				}
				else
				{
					bool includeSegmentedKeyTip = false;

					// if this is a segmented menu...
					if (null != menu &&
						menu.ButtonType != MenuToolButtonType.DropDown)
					{
						// only force its use in an application menu
						ToolLocation location = XamRibbon.GetLocation(tool);

						if (location == ToolLocation.ApplicationMenu ||
							(location != ToolLocation.QuickAccessToolbar && false == string.IsNullOrEmpty(menu.KeyTipForSegmentedButton)))
						{
							includeSegmentedKeyTip = true;
						}
					}

					if (includeSegmentedKeyTip)
					{
						List<IKeyTipProvider> providerList = new List<IKeyTipProvider>();

						providerList.Add(new SegmentedMenuKeyTipProvider(menu, menuItem, false));
						providerList.Add(new SegmentedMenuKeyTipProvider(menu, menuItem, true));
						providers = providerList.ToArray();
					}
					else
						providers = base.GetKeyTipProviders(tool, menuItem);
				}

				return providers;
			} 
			#endregion //GetKeyTipProviders

			#region IsActivatable
			/// <summary>
			/// Returns true if the tool supports activation or false if the tool cannot be activated.  (read-only)
			/// </summary>
			/// <param name="tool">The tool instance whose activable state is being queried.</param>
			protected override bool IsActivatable(MenuToolBase tool)
			{
				MenuToolPresenter mtp = tool.Presenter as MenuToolPresenter;

				// AS 10/25/07 BR27842
				if (null != mtp && mtp.IsMenuButtonAreaEnabled == false)
					return false;

				return base.IsActivatable(tool);
			} 
			#endregion //IsActivatable

			#region PrepareToolMenuItem

			/// <summary>
			/// Prepares the container <see cref="ToolMenuItem"/>to 'host' the tool.
			/// </summary>
			/// <param name="toolMenuItem">The container that wraps the tool.</param>
			/// <param name="tool">The tool that is being wrapped.</param>
			protected override void PrepareToolMenuItem(ToolMenuItem toolMenuItem, MenuToolBase tool)
			{
				base.PrepareToolMenuItem(toolMenuItem, tool);

				MenuTool menuTool = tool as MenuTool;

				if (menuTool != null)
				{
					menuTool.BindIsSegmented(toolMenuItem);
					menuTool.BindCheckState(toolMenuItem);

					// AS 11/13/07 BR27990
					// This also needs to be done for menu tools within a menu.
					//
					MenuTool.BindCommandProperties(toolMenuItem, menuTool);
				}
			}

			#endregion //PrepareToolMenuItem	

		}
		#endregion //MenuToolProxy

		#region SegmentedMenuKeyTipProvider
		private class SegmentedMenuKeyTipProvider : ToolKeyTipProvider
		{
			#region Member Variables

			private bool _isButton;

			#endregion //Member Variables

			#region Constructor
			internal SegmentedMenuKeyTipProvider(MenuTool menu, ToolMenuItem menuItem, bool isButton)
				: base(menu, menuItem)
			{
				this._isButton = isButton;
			}
			#endregion //Constructor

			#region IKeyTipProvider Members

			public override bool Activate()
			{
				if (this._isButton)
				{
					((MenuTool)this.Tool).OnSegmentedButtonClick();
					return true;
				}
				else
					return base.Activate();
			}

			#region Equals
			public override bool Equals(IKeyTipProvider provider)
			{
				SegmentedMenuKeyTipProvider menuProvider = provider as SegmentedMenuKeyTipProvider;

				return null != menuProvider &&
					menuProvider._isButton == this._isButton &&
					menuProvider.Tool == this.Tool;
			}
			#endregion //Equals

			#region GetContainer
			public override IKeyTipContainer GetContainer()
			{
				if (this._isButton)
					return null;

				return base.GetContainer();
			}
			#endregion //GetContainer

			public override bool DeactivateParentContainersOnActivate
			{
				get
				{
					if (this._isButton)
						return true;

					return base.DeactivateParentContainersOnActivate;
				}
			}

			#region IsEnabled
			public override bool IsEnabled
			{
				get
				{
					if (this._isButton)
					{
						bool isEnabled = this.Tool.IsEnabled;

						if (isEnabled)
						{
							// get enabled from command
							ICommandSource ics = this.Tool as ICommandSource;

							if (ics != null && ics.Command != null)
								isEnabled = Utilities.CanExecuteCommand(ics);

							if (isEnabled)
							{
								IRibbonTool ribbonTool = this.Tool as IRibbonTool;

								// AS 10/25/07 BR27842
								if (null != ribbonTool && ribbonTool.ToolProxy.IsActivateable(this.Tool) == false)
									isEnabled = false;
							}
						}

						return isEnabled;
					}

					return base.IsEnabled;
				}
			}
			#endregion //IsEnabled

			#region KeyTipValue
			public override string KeyTipValue
			{
				get
				{
					if (this._isButton)
						return ((MenuTool)this.Tool).KeyTipForSegmentedButton;

					return base.KeyTipValue;
				}
			}
			#endregion //KeyTipValue

			public override Point Location
			{
				get
				{
					ToolLocation location = XamRibbon.GetLocation(this.Tool);
					bool isLargeTool = RibbonToolHelper.GetSizingMode(this.Tool) == RibbonToolSizingMode.ImageAndTextLarge;
					XamRibbon ribbon = XamRibbon.GetRibbon(this.Tool);
					UIElement toolElement = this.MenuItem ?? this.Tool;
					Point pt = new Point();

					// no need for placement elements with a large ribbon tool
					if (location == ToolLocation.Ribbon && isLargeTool)
					{
						// use the horizontal mid point
						pt.X = toolElement.RenderSize.Width / 2;

						if (ribbon != null)
							pt.Y = ribbon.GetRibbonGroupToolVerticalOffset(this._isButton ? 0 : 2, toolElement);
						else
							pt.Y = this._isButton ? 0 : toolElement.RenderSize.Height;
					}
					else
					{
						Debug.Assert(location != ToolLocation.QuickAccessToolbar && location != ToolLocation.Unknown);

						// try to use the placement elements
						KeyTipPlacementType placement = this._isButton ? KeyTipPlacementType.SmallImage : KeyTipPlacementType.DropDownButton;
						Rect placementRect = XamRibbon.GetKeyTipPlacementElementRect(placement, toolElement, toolElement);
						bool hasPlacementElement = placementRect.IsEmpty == false;

						if (hasPlacementElement == false)
							placementRect = new Rect(toolElement.RenderSize);

						// horizontally centered if possible
						pt.X = hasPlacementElement
							? (placementRect.Left + placementRect.Right) / 2
							: this._isButton ? placementRect.Left : placementRect.Right;

						pt.Y = (placementRect.Top + placementRect.Bottom) / 2;
					}

					return pt;
				}
			}

			#endregion
		} 
		#endregion //SegmentedMenuKeyTipProvider

		#region GalleryPreviewKeyTipProvider
		private class GalleryPreviewKeyTipProvider : ToolKeyTipProvider
		{
			#region Constructor
			internal GalleryPreviewKeyTipProvider(MenuTool menu)
				: base(menu, null)
			{
			}
			#endregion //Constructor

			#region Location
			public override Point Location
			{
				get
				{
					Point location = base.Location;

					// horizontally center within the dropdown button
					Rect ddRect = XamRibbon.GetKeyTipPlacementElementRect(KeyTipPlacementType.DropDownButton, this.Tool, this.Tool);

					if (ddRect.IsEmpty == false)
						location.X = (ddRect.Left + ddRect.Right) / 2;

					return location;
				}
			}
			#endregion //Location

			#region Alignment
			public override KeyTipAlignment Alignment
			{
				get
				{
					return KeyTipAlignment.MiddleLeft;
				}
			}
			#endregion //Alignment
		} 
		#endregion //GalleryPreviewKeyTipProvider

		#region ButtonTypeToIsCheckableConverter internal class

		internal class ButtonTypeToIsCheckableConverter : IValueConverter
		{
			private ButtonTypeToIsCheckableConverter() { }

			static internal ButtonTypeToIsCheckableConverter Instance = new ButtonTypeToIsCheckableConverter();

			#region IValueConverter Members

			object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (targetType == typeof(bool) && value is MenuToolButtonType)
				{
					if ((MenuToolButtonType)value == MenuToolButtonType.SegmentedState)
						return KnownBoxes.TrueBox;
				}

				return KnownBoxes.FalseBox;
			}

			object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return Binding.DoNothing;
			}

			#endregion
		}

		#endregion //ButtonTypeToIsCheckableConverter

		#region ButtonTypeToIsSegmentedConverter internal class

		internal class ButtonTypeToIsSegmentedConverter : IValueConverter
		{
			private ButtonTypeToIsSegmentedConverter() { }

			static internal ButtonTypeToIsSegmentedConverter Instance = new ButtonTypeToIsSegmentedConverter();

			#region IValueConverter Members

			object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (targetType == typeof(bool) && value is MenuToolButtonType)
				{
					if ((MenuToolButtonType)value == MenuToolButtonType.DropDown)
						return KnownBoxes.FalseBox;
					else
						return KnownBoxes.TrueBox;
				}

				return KnownBoxes.FalseBox;
			}

			object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return Binding.DoNothing;
			}

			#endregion
		}

		#endregion //ButtonTypeToIsSegmentedConverter
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