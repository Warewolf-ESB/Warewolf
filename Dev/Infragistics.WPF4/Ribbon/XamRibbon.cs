using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

using Infragistics.Windows;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Licensing;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Ribbon.Events;
using System.Collections;
using Infragistics.Windows.Ribbon.Internal;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Documents;
using System.Media;
using Infragistics.Shared;
using System.Runtime.InteropServices;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Editors.Events;
using System.Windows.Threading;
using Infragistics.Windows.Automation.Peers.Ribbon;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Provides the functionality of a Microsoft Office 2007 Ribbon, including a <see cref="QuickAccessToolbar"/>, <see cref="ApplicationMenu"/>, <see cref="RibbonTabItem"/>s, 
	/// <see cref="RibbonGroup"/>s, <see cref="ContextualTabGroup"/>s and Tools.
	/// </summary>
	/// <remarks>
	/// <p class="body">The XamRibbon is a custom control for providing the look and behavior of the Microsoft Office 2007 Ribbon. The main properties of the Ribbon are 
	/// the <see cref="QuickAccessToolbar"/>, <see cref="ApplicationMenu"/> and the <see cref="Tabs"/>.</p>
	/// <p class="body">The QuickAccessToolbar is a toolbar that is displayed above or below the Ribbon based on the <see cref="XamRibbon.QuickAccessToolbarLocation"/> and 
	/// is used to display commonly used tools to the end user. The end user may add tools to the QAT using the right click context menu or tools may be programatically 
	/// added to the <see cref="Infragistics.Windows.Ribbon.QuickAccessToolbar"/> using a <see cref="QatPlaceholderTool"/>. You can also add tools to the 
	/// <see cref="ToolsNotInRibbon"/> if you have an instance of a tool that will not be visible to the end user within a <see cref="RibbonGroup"/> or the <see cref="ApplicationMenu"/> 
	/// but will be added to the QAT.</p>
	/// <p class="body">The ApplicationMenu is a specialized menu that is displayed as a round button within the upper left corner of the XamRibbon and contains 3 specialized 
	/// areas for containing tools that are used to perform actions that are general to the application or window.</p>
	/// <p class="body">The <see cref="Tabs"/> are a collection of <see cref="RibbonTabItem"/> instances that are used to logically and visually organize the tools used 
	/// to perform various actions within the application. The <see cref="SelectedTab"/> is used to control which tab item's contents are visible to the end user. The 
	/// <see cref="SelectNextTab()"/> and <see cref="SelectPreviousTab()"/> methods may be used to navigate within the tabs or you can set the <see cref="TabItem.IsSelected"/> 
	/// property of a visible tab to true.</p>
	/// <p class="body">The XamRibbon also provides support for creating <see cref="ContextualTabGroup"/> instances. These objects are added to the 
	/// <see cref="XamRibbon.ContextualTabGroups"/> collection and when their <see cref="ContextualTabGroup.IsVisible"/> is true, their <see cref="ContextualTabGroup.Tabs"/> 
	/// are displayed after the non-contextual tabs with a caption above the tab item headers.</p>
	/// <p class="body">The <see cref="XamRibbonWindow"/> may be used to host the XamRibbon to provide the same style rounded forms 
	/// that are used within Microsoft Office 2007. The <see cref="IsWithinRibbonWindow"/> is set to true when the XamRibbon is hosted within the 
	/// <see cref="RibbonWindowContentHost.Ribbon"/> of a XamRibbonWindow.</p>
	/// <p class="body">The XamRibbon supports the ability to minimize using the <see cref="IsMinimized"/> property and may also be 
	/// minimized via the UI using the right click context menu or by pressing <b>Ctrl-F1</b>. When the Ribbon is minimized, the contents of the 
	/// selected tab are hidden and the height of the Ribbon is reduced. When a tab is selected (via the mouse, keyboard, keytips or programatically setting the 
	/// <see cref="SelectedTab"/>), the contents of the selected tab are displayed within a popup above the content of the containing window. The 
	/// <see cref="AllowMinimize"/> property can be used to prevent the minimized state of the ribbon from being changed in the UI.</p>
	/// <p class="body">The look of the XamRibbon may be changed using the <see cref="Theme"/> property and supports values such as 
	/// <b>Office2k7Black</b>, <b>Office2k7Blue</b> and <b>Office2k7Silver</b> to provide the same style color schemes as you would find in Microsoft 
	/// Office 2007. You can also provide modified modified washed versions of these color schemes using the <see cref="ResourceWasher"/>.</p>
	/// <p class="body">The XamRibbon supports the ability to automatically hide when the width and/or height of the containing form fall below 
	/// specified threshold values. This behavior is disabled by default but can be enabled by setting the <see cref="AutoHideEnabled"/> property to true. 
	/// When true, the <see cref="AutoHideState"/> of the Ribbon will be changed to <b>Hidden</b> when either the width or height fall below the 
	/// <see cref="AutoHideHorizontalThreshold"/> or <see cref="AutoHideVerticalThreshold"/> values of the containing form respectively.</p>
	/// </remarks>
	/// <seealso cref="QuickAccessToolbar"/>
	/// <seealso cref="ApplicationMenu"/>
	/// <seealso cref="RibbonTabItem"/>
	/// <seealso cref="Tabs"/>
	/// <seealso cref="ContextualTabGroups"/>
	/// <seealso cref="RibbonGroup"/>
	
	
	[TemplatePart(Name = "PART_ApplicationMenuSite", Type = typeof(ContentControl))]
	[TemplatePart(Name = "PART_RibbonTabControlSite", Type = typeof(ContentControl))]
	[TemplatePart(Name = "PART_QuickAccessToolbarBottomSite", Type = typeof(ContentControl))]
	// AS 10/23/07
	// There really is no reason this has to be a border so changing to frameworkelement.
	//[TemplatePart(Name = "PART_XamRibbonCaption", Type = typeof(Border))]
	[TemplatePart(Name = "PART_XamRibbonCaption", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_RibbonCaptionPanel", Type = typeof(RibbonCaptionPanel))]
    
    // JJD 5/21/10 - NA 2010 Volume 2 - Scenic Ribbon support
    // Added PART_WindowIcon to show the system menu with the scenic theme
    [TemplatePart(Name = "PART_WindowIcon", Type = typeof(FrameworkElement))]
    [ContentProperty("Tabs")]	//JM 11-12-07
	public class XamRibbon : IGControlBase,
							 ICommandHost,
							 IKeyTipContainer
	{
		#region Member Variables

		private UltraLicense								_license;

		private FrameworkElement							_activeItem;
		private XamTabControl								_ribbonTabControl;
		private RibbonCaptionPanel							_captionPanel;
		private ContentControl								_applicationMenuSite;
		private ContentControl								_ribbonTabControlSite;
		private ContentControl								_quickAccessToolbarBottomSite;
		private ToolInstanceManager							_toolInstanceManager;
		private ObservableCollection<RibbonTabItem>			_tabs;
		private ReadOnlyObservableCollection<RibbonTabItem>	_tabsInView;
		// AS 10/25/07 BR27728
		//private ObservableCollection<RibbonTabItem>			_tabsInViewInternal;
		private ObservableCollectionExtended<RibbonTabItem>	_tabsInViewInternal;
		private ToolsNotInRibbonCollection					_toolsNotInRibbon;
		private ApplicationMenu								_applicationMenu;
		private ArrayList									_logicalChildren = new ArrayList(5);
		private UIElement									_ribbonGroupContainerForSizing;
		private RibbonGroup									_ribbonGroupForSizing;
		// AS 6/3/08 BR32772
		//private XamRibbonWindow								_ribbonWindow;
		private IRibbonWindow								_ribbonWindow;
		private ContextualTabGroupCollection				_contextualTabGroups;
		private FrameworkElement							_topLevelVisual;
		private RibbonMode									_ribbonMode = RibbonMode.Normal;

		private Dictionary<string, RibbonGroup>				_registeredGroups = new Dictionary<string, RibbonGroup>();

		private PlacementMode								_originalToolTipPlacement;
		private Rect										_originalToolTipPlacementRectangle;

		private int											_toolRegistrationSuspendCount = 0;
		private Dictionary<FrameworkElement, bool>			_pendingToolRegistrations = null;

		private KeyTipManager								_keyTipManager;
		private DateTime									_f10KeyDownTime = DateTime.MaxValue;
		private bool										_resetRibbonModeOnF10Up;

		private TabControlPopupManager						_tabControlPopupManager;

		private const ModifierKeys							AllModifiers = ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows;

		// AS 12/4/07 HighContrast Support
		private ResourceDictionary							_highContrastDictionary;

		// AS 9/22/09 TFS22390
		private HashSet										_menuToolVerificationList;

		// AS 7/1/10 TFS34457
		private FrameworkElement							_qatTargetPendingLoad;

		// AS 7/1/10 TFS32477
		private HashSet										_toolsToVerify;

		#endregion //Member Variables	
    
		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="XamRibbon"/> class.
		/// </summary>
		public XamRibbon()
		{
			// Verify and cache the license
			// AS 11/7/07 BR21903
			// Always do the license checks.
			//
			//if (DesignerProperties.GetIsInDesignMode(this))
			{
				try
				{
					// We need to pass our type into the method since we do not want to pass in 
					// the derived type.
					this._license = LicenseManager.Validate(typeof(XamRibbon), this) as UltraLicense;
				}
				catch (System.IO.FileNotFoundException) { }
			}

			// AS 1/9/08
			if (null != this._license &&
				(this._license.LicenseOptions & LicenseOptions.Office2007UI) != LicenseOptions.Office2007UI)
			{
				throw new InvalidOperationException(GetString("LE_Office2007UIRequirements"));
			}

			// AS 10/25/07 BR27728
			//this._tabsInViewInternal = new ObservableCollection<RibbonTabItem>();
			this._tabsInViewInternal = new ObservableCollectionExtended<RibbonTabItem>();
			this._toolInstanceManager = new ToolInstanceManager(this);

			// set the inherited property that children can use to get a backward pointer to the ribbon
			this.SetValue(XamRibbon.RibbonProperty, this);


			// JM BR27615 10-22-07 - Don't do this is designmode since Orcas pukes when placing the control on the design surface.
			if (DesignerProperties.GetIsInDesignMode(this) == false)
			{
				// bind the focusable property to the internal IsModeSpecialPropertyKey so that we don't
				// take focus unlees we are in a mode othr than Normal
				this.SetBinding(FocusableProperty, Utilities.CreateBindingObject(IsModeSpecialProperty, BindingMode.OneWay, this));
			}

			// AS 1/3/08 BR29371
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);

            // JJD 4/30/10 - NA 2010 Volumne 2 - Scenic Riboon
            // set the dynamic resource bindings so we know if the scenic ribbon is being used
            this.SetResourceReference(IsScenicThemeInternalProperty, RibbonWindowContentHost.IsScenicThemeKey);

			// AS 6/21/12 TFS114953
			this.SetResourceReference(UseScenicApplicationMenuInternalProperty, RibbonWindowContentHost.UsesScenicApplicationMenuKey);
		}

		static XamRibbon()
		{
			// AS 5/9/08
			// register the groupings that should be applied when the theme property is changed
			ThemeManager.RegisterGroupings(typeof(XamRibbon), new string[] { PrimitivesGeneric.Location.Grouping, EditorsGeneric.Location.Grouping, RibbonGeneric.Location.Grouping });

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamRibbon), new FrameworkPropertyMetadata(typeof(XamRibbon)));
			FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
			Control.IsTabStopProperty.OverrideMetadata(typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			// AS 10/9/07
            // AS 10/14/08 TFS6092
            // We need the mode to start at None so you cannot tab into the control.
            //
			//KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(XamRibbon), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(XamRibbon), new FrameworkPropertyMetadata(KeyboardNavigationMode.None, null, new CoerceValueCallback(CoerceTabNavigation)));

			// AS 10/18/07
			// The arrow keys (specifically using the left arrow key when i was testing BR27510) could cause focus
			// to leave the ribbon. We need focus to cycle within the ribbon.
			//
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(XamRibbon), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

			EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(XamRibbon.OnRibbonToolContextMenuOpening));
			EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.ToolTipOpeningEvent, new ToolTipEventHandler(XamRibbon.OnToolTipOpening), true);

			// AS 10/10/07
			// We should be listening for keyboard focus changes and not logical focus changes.
			//
			//EventManager.RegisterClassHandler(typeof(XamRibbon), FocusManager.GotFocusEvent, new RoutedEventHandler(XamRibbon.OnGotFocus_ClassHandler), true);
			EventManager.RegisterClassHandler(typeof(XamRibbon), Keyboard.GotKeyboardFocusEvent, new RoutedEventHandler(XamRibbon.OnGotFocus_ClassHandler), true);

			EventManager.RegisterClassHandler(typeof(XamRibbon), ValueEditor.EditModeEndedEvent, new EventHandler<EditModeEndedEventArgs>(OnEditModeEnded), true);

			// JM 11-09-07
			EventManager.RegisterClassHandler(typeof(XamRibbon), ValueEditor.EditModeStartedEvent, new EventHandler<EditModeStartedEventArgs>(OnEditModeStarted), true);

			XamRibbon.AddItemToQatCommand		= new RoutedCommand("AddItemToQat", typeof(XamRibbon));
			XamRibbon.RemoveItemFromQatCommand	= new RoutedCommand("RemoveItemFromQat", typeof(XamRibbon));

			CommandManager.RegisterClassCommandBinding(typeof(XamRibbon), new CommandBinding(XamRibbon.AddItemToQatCommand, new ExecutedRoutedEventHandler(XamRibbon.OnExecuteInternalCommand), new CanExecuteRoutedEventHandler(OnCanExecuteInternalCommand)));
			CommandManager.RegisterClassCommandBinding(typeof(XamRibbon), new CommandBinding(XamRibbon.RemoveItemFromQatCommand, new ExecutedRoutedEventHandler(XamRibbon.OnExecuteInternalCommand), new CanExecuteRoutedEventHandler(OnCanExecuteInternalCommand)));

			// AS 10/22/07 BR27620
			EventManager.RegisterClassHandler(typeof(XamRibbon), Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnPreviewMouseDownOutsideCapturedElement));
			EventManager.RegisterClassHandler(typeof(XamRibbon), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));


			// AS 6/22/12 TFS115111
			Infragistics.Windows.Internal.ModalWindowHelper.ModalDialogWindowCountProperty.OverrideMetadata(typeof(XamRibbon), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnModalDialogWindowCountChanged)));

		}

		#endregion //Constructor	

		#region Constants

		internal static readonly Size							DEFAULT_QAT_TOOL_SIZE = new Size(22, 22);

		/// <summary>
		/// Returns the Width of the <see cref="ApplicationMenu"/> button.
		/// </summary>
		public const double										ApplicationMenuButtonWidth = 43;

		internal static readonly Object							RibbonGroupForResizingTag = new object();

		#endregion //Constants

		#region Internal Commands

		internal static readonly RoutedCommand AddItemToQatCommand;
		internal static readonly RoutedCommand RemoveItemFromQatCommand;

		#endregion //Internal Commands

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			return base.ArrangeOverride(finalSize);
		}

			#endregion //ArrangeOverride	
    
			#region Commands

		/// <summary>
		/// Gets the supported commands (read-only) 
		/// </summary>
		/// <value>A static instance of the <see cref="RibbonCommands"/> class.</value>
		/// <remarks>
		/// <p class="body">This class exposes properties that return all of the commands that the control supports. 
		/// </p>
		/// </remarks>
		internal protected override CommandsBase Commands
		{
			get { return RibbonCommands.Instance; }
		}

			#endregion //Commands

			// AS 11/5/07
			// When the ribbon is collapsed, it should be disabled so it doesn't try to be ui interactive.
			//
			#region IsEnabledCore
		/// <summary>
		/// Indicates the resolved enabled state of the control.
		/// </summary>
		protected override bool IsEnabledCore
		{
			get
			{

				// AS 6/22/12 TFS115111
				//// AS 6/22/11 TFS74670
				//// We don't want the ribbon to show the keytips, open the file menu, etc. when there is a modal overlay.
				////
				//if (_ribbonWindow != null && _ribbonWindow.HasModalDialogWindow)
				//    return false;
				if (Infragistics.Windows.Internal.ModalWindowHelper.GetModalDialogWindowCount(this) > 0)
					return false;


				return base.IsEnabledCore && this.AutoHideState == RibbonAutoHideState.NotHidden;
			}
		} 
			#endregion //IsEnabledCore

			#region HitTestCore

		/// <summary>
		/// Determines if the specified point is within the bounds of this element.
		/// </summary>
		/// <param name="hitTestParameters">Specifies the point for the hit test.</param>
		/// <returns>A <see cref="HitTestResult"/> indicating the result of the hit test operation.</returns>
		/// <remarks>
		/// <p class="body">
		/// This method is overridden on this class to make sure the ribbon gets mouse messages
		/// regardless of whether its background is transparent or not.
		/// </p>
		/// </remarks>
		protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
		{
			Rect rect = new Rect( new Point( ), this.RenderSize );
			if ( rect.Contains( hitTestParameters.HitPoint ) )
				return new PointHitTestResult( this, hitTestParameters.HitPoint );

			return base.HitTestCore( hitTestParameters );
		}

			#endregion // HitTestCore

			#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				// AS 11/16/07 BR28515
				// The ToolsNotInRibbon need to be in the logical tree.
				//
				if (this._toolsNotInRibbon != null)
					return new MultiSourceEnumerator(this._logicalChildren.GetEnumerator(), this._toolsNotInRibbon.GetEnumerator());

				return this._logicalChildren.GetEnumerator();
			}
		}

			#endregion //LogicalChildren	
    
			#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			// measure the ribbon group that we use for resizing so we can control the height
			// of the content area of the ribbon tabs
			this.RibbonGroupContainerForSizing.Measure(constraint);
			this.RibbonGroupContainerForSizing.Arrange(new Rect(this.RibbonGroupContainerForSizing.DesiredSize));

			// AS 10/24/07 AutoHide
			bool isCollapsed = false;

			// AS 3/24/10 TFS27869
			// Added check for window state.
			//
			if (this.AutoHideEnabled && this._topLevelVisual != null && WindowState.Normal == GetWindowState(_topLevelVisual))
			{
				double horizontalThreshold = this.AutoHideHorizontalThreshold;
				isCollapsed = horizontalThreshold > 0 && this._topLevelVisual.Width < horizontalThreshold;

				if (isCollapsed == false)
				{
					double verticalThreshold = this.AutoHideVerticalThreshold;

					if (verticalThreshold > 0 && this._topLevelVisual.Height < verticalThreshold)
						isCollapsed = true;
				}
			}

			if (isCollapsed)
				this.SetValue(XamRibbon.AutoHideStatePropertyKey, RibbonAutoHideState.Hidden);
			else
				this.ClearValue(XamRibbon.AutoHideStatePropertyKey);

			return base.MeasureOverride(constraint);
		}
			#endregion //MeasureOverride

			#region OnAccessKey
		/// <summary>
		/// Responds when the System.Windows.Controls.AccessText.AccessKey for this control is invoked.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnAccessKey(AccessKeyEventArgs e)
		{
			// AS 12/8/09 TFS25326
			// See KeyTipManager.RefreshAccessKeys for details.
			//
			switch (this._ribbonMode)
			{
				case RibbonMode.KeyTipsPending:
					this.ProcessPendingKeyTip(e.Key);
					return;
				case RibbonMode.KeyTipsActive:
					this.KeyTipManager.ProcessKey(e.Key[0], false);
					return;
				default:
					Debug.Fail("An access key was received while not in keytip mode.");
					break;
			}

			base.OnAccessKey(e);
		}
			#endregion //OnAccessKey

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.SuspendToolRegistration();

			try
			{
				this.VerifyParts();
			}
			finally
			{
				this.ResumeToolRegistration();
			}

			this.WireTopLevelVisual();

		}

			#endregion //OnApplyTemplate	

            #region OnCreateAutomationPeer

        /// <summary>
        /// Returns an automation peer that exposes the <see cref="XamRibbon"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.XamRibbonAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamRibbonAutomationPeer(this);
        }
            #endregion //OnCreateAutomationPeer

			#region OnKeyDown

		/// <summary>
		/// Called when the element has input focus and a key is pressed.
		/// </summary>
		/// <param name="e">An instance of KeyEventArgs that contains information about the key that was pressed.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Handled)
				return;

			switch (e.Key)
			{
				case Key.System:
					{
						#region SystemKey

						switch (e.SystemKey)
						{
							case Key.LeftAlt:
							case Key.RightAlt:
								#region Alt

								e.Handled = true;
								break;

								#endregion //Alt
						}
						break;

						#endregion //SystemKey
					}

				case Key.Enter:
				case Key.Space:
				case Key.F4:
					#region Enter/Space/F4

					{
						if (this.Mode == RibbonMode.ActiveItemNavigation)
						{
							FrameworkElement activeItem = this.ActiveItem;

							#region Handle RibbonTabItems

							RibbonTabItem tabItem = activeItem as RibbonTabItem;

							if (tabItem != null)
							{
								if (this.AllowMinimize == true)
								{
									// AS 10/18/07
									// Actually we just want to toggle the dropped down state.
									//
									//// toggle the minimized state
									//this.IsMinimized = !this.IsMinimized;
									Debug.Assert(this.RibbonTabControl.IsDropDownOpen == false, "I'm not sure what is leading to this but we may want to review this code. We're expecting to only get here if the tab item is not opened in which case we will open it and focus the first item.");

									// make sure the tab is selected - this should drop it down if needed
									// AS 5/10/12 TFS111333
									//tabItem.IsSelected = true;
									DependencyPropertyUtilities.SetCurrentValue(tabItem, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);

									// then focus the first item if possible
									if (this.RibbonTabControl.IsDropDownOpen)
										((IRibbonPopupOwner)this._tabControlPopupManager).FocusFirstItem();
									else
										tabItem.Focus();

									e.Handled = true;

								}
								else
								{
									// for RibbonTabItems we just want to go back to normal
									this.SetMode(RibbonMode.Normal);

									e.Handled = true;
								}

								return;
							}

							#endregion //Handle RibbonTabItems

							#region Handle MenuTools

							MenuToolBase menuTool = activeItem as MenuToolBase;

							if (menuTool != null)
							{
								// toggle the open state
								// AS 12/19/07 BR29199
								//menuTool.IsOpen = !menuTool.IsOpen;
								if (menuTool.IsOpen)
									menuTool.IsOpen = false;
								else
									menuTool.OpenMenu(PopupOpeningReason.Keyboard);

								e.Handled = true;

								return;
							}

							#endregion //Handle MenuTools

							#region Handle tools

							if (e.Key == Key.Space)
							{
								if (activeItem is IRibbonTool)
								{
									RibbonToolProxy proxy = ((IRibbonTool)activeItem).ToolProxy;

									if (proxy != null)
									{
										proxy.PerformAction(activeItem);
										if (proxy.RetainFocusOnPerformAction == false)
											this.SetMode(RibbonMode.Normal);
									}
									else
										this.SetMode(RibbonMode.Normal);
								}
								else
								{
									this.SetMode(RibbonMode.Normal);
								}

								e.Handled = true;
							}

							#endregion //Handle tools
						}
						break;
					}

					#endregion //Enter/Space/F4

				case Key.Escape:
					#region Escape

					switch (this._ribbonMode)
					{
						case RibbonMode.KeyTipsPending:
						case RibbonMode.ActiveItemNavigation:
							this.SetMode(RibbonMode.Normal);
							e.Handled = true;
							break;
					}

					break;

					#endregion //Escape

				case Key.Up:
				case Key.Down:
					#region Up/Down
					// AS 10/9/07
					// We want to keep the DirectionalNavigationMode set to Cycle (so that the left/right
					// arrow keys will wrap around to the other side). However, this means that you can use
					// the arrow keys to leave the bounds of the ribbon which is not what office does so
					// if an arrow key would cause navigation out of the ribbon then eat the key.
					//
					if (this._ribbonMode == RibbonMode.ActiveItemNavigation)
					{
						FocusNavigationDirection direction = e.Key == Key.Up ? FocusNavigationDirection.Up : FocusNavigationDirection.Down;
						UIElement focusedElement = Keyboard.FocusedElement as UIElement;
						DependencyObject objectToFocus = focusedElement != null
							? focusedElement.PredictFocus(direction)
							: null;

						// AS 11/30/07 BR28769
						// If an element is within a popup then IsAncestorOf will return false. What we
						// are really looking to ensure is that we do not navigate out of the ribbon 
						// so see if the element to navigate to is part of the ribbon.
						//
						//if (objectToFocus == null || false == this.IsAncestorOf(objectToFocus))
						if (objectToFocus == null || this != XamRibbon.GetRibbon(objectToFocus))
							e.Handled = true;
					}
					break; 
					#endregion //Up/Down

				case Key.Tab:
					// AS 10/10/07
					// if we're in normal mode and someone tries to use a navigation key like tab
					// to leave the control then we want to eat the keystroke. i would have preferred
					// to do a predictfocus to find out if the user was leaving their control or not
					// but unfortunately that does not work with next/previous - only with spatial
					// navigation
					//
					if (this.Mode == RibbonMode.Normal)
						e.Handled = true;
					break;
			}
		}

			#endregion //OnKeyDown	
    
			#region OnKeyUp

		/// <summary>
		/// Called when the element has input focus and a key is released.
		/// </summary>
		/// <param name="e">An instance of KeyEventArgs that contains information about the key that was pressed.</param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (e.Handled)
				return;

			switch (e.Key)
			{
				case Key.System:
					{
						#region SystemKey

						switch (e.SystemKey)
						{
							case Key.LeftAlt:
							case Key.RightAlt:
								#region Alt

								e.Handled = true;
								break;

								#endregion //Alt
						}
						break;

						#endregion //SystemKey
					}
			}
		}

			#endregion //OnKeyUp	

			#region OnGotKeyboardFocus

		/// <summary>
		/// Called when the element received keyboard focus
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);

			if (e.OldFocus is DependencyObject)
			{
				Debug.WriteLine("XamRibbon.OnGotKeyboardFocus - oldFocus: " + e.OldFocus.ToString());

				// if the old focused element is outside our focus scope then cache that element so that we
				// can restore focus to it when our mode goes back to normal
				DependencyObject scope = FocusManager.GetFocusScope(e.OldFocus as DependencyObject);
				
				Debug.WriteLine("focus scope: " + scope.ToString());

                // JJD 11/9/07
                // handle the nested menu case (i.e. inside a recent item) and keep walking up the focus scope chain
				//if (scope is MenuToolBase.MenuToolMenu)
				while (scope is MenuToolBase.MenuToolMenu)
					scope = FocusManager.GetFocusScope(((MenuToolBase.MenuToolMenu)scope).Parent);

				if (scope != this)
				{
					// is the mode was just set to KeyTipsActive which caused focus to be set to us then
					// set the selectedTab as the active item
					if (this._ribbonMode != RibbonMode.Normal)
					{
						this.SetActiveItem( this.SelectedTab );
					}
				}

			}

			if (e.NewFocus is DependencyObject)
			{
				Debug.WriteLine("XamRibbon.OnGotKeyboardFocus - newFocus: " + e.NewFocus.ToString());
			}

		}

			#endregion //OnGotKeyboardFocus	
 
			#region OnInitialized

		/// <summary>
		/// Called after the control has been initialized.
		/// </summary>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.UpdateTabsInView();

			if (this.ApplicationMenu == null)
				this.SetValue(XamRibbon.ApplicationMenuProperty, new ApplicationMenu());

			if (this.QuickAccessToolbar == null)
				this.SetValue(XamRibbon.QuickAccessToolbarProperty, new QuickAccessToolbar());

			// AS 12/4/07 HighContrast Support
			this.SetResourceReference(IsSystemHighContrastProperty, SystemParameters.HighContrastKey);
			this.SetResourceReference(ControlColorProperty, SystemColors.ControlColorKey);
			this.SetResourceReference(ControlTextColorProperty, SystemColors.ControlTextColorKey);
		}

			#endregion //OnInitialized	

			#region OnIsKeyboardFocusWithinChanged
		/// <summary>
		/// Overriden. Invoked when the keyboard focus enters/leaves the element.
		/// </summary>
		/// <param name="e">Provides information about the property change.</param>
		protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsKeyboardFocusWithinChanged(e);

			// AS 10/17/07
			// If we were in ActiveItemNavigation (and possibly other reasons) and focus is shifted
			// out of the ribbon then we should probably be leaving that mode. In this case it happened
			// because I had a MenuTool with no child items and when it was activated (in active item
			// navigation mode) and you pressed the enter key, focus was shifted back to the document
			// but the ribbon remaining in active item mode and the tool was still rendered as an
			// active item. Its possible that the menutool should not even have gotten focus in this
			// case but the issue remains - if focus leaves the ribbon while we are in a non-normal
			// mode, we should reset the state back to normal. I'm passing false to transfer focus since
			// in essence focus should already be where it needs to be.
			//
			if (false.Equals(e.NewValue))
			{
				// AS 11/6/07
				// We could be losing focus to a context menu in which case we will ignore the focus
				// change since it will restore focus when it is closed and we want to remain in 
				// active mode to emulate office's behavior.
				//
				ContextMenu contextMenu = Keyboard.FocusedElement as ContextMenu;

				// we'll only do this if the context menu is for something within us. the context menu
				// caches the last element that had focus and will restore focus to that
				if (contextMenu != null && 
					contextMenu.PlacementTarget != null && 
					XamRibbon.GetRibbon(contextMenu.PlacementTarget) == this)
				{
					// exit keytip mode if we are in it. office changes to active item mode
					if (this.Mode != RibbonMode.Normal)
						this.SetMode(RibbonMode.ActiveItemNavigation);

					return;
				}

				this.SetMode(RibbonMode.Normal);

				// AS 10/22/07
				// It is possible that we're in normal mode and that an editor tool within 
				// had and maintained focus (e.g. the combo editor tool) so we need to clear
				// the focused element so that is exits edit mode.
				//
				if (FocusManager.GetIsFocusScope(this))
				{
					IInputElement focusedElement = FocusManager.GetFocusedElement(this);

					if (null != focusedElement && focusedElement is RibbonTabItem == false)
						FocusManager.SetFocusedElement(this, null);
				}
			}
		} 
			#endregion //OnIsKeyboardFocusWithinChanged

			#region OnMouseWheel
		/// <summary>
		/// Invoked when the mouse wheel has been rotated over the element.
		/// </summary>
		/// <param name="e">The <see cref="MouseWheelEventArgs"/> that contains information about the event.</param>
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			base.OnMouseWheel(e);

			this.ProcessMouseWheel(e);
		} 
			#endregion //OnMouseWheel

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ActiveItem

		/// <summary>
		/// Returns the currently active item in the <see cref="XamRibbon"/>.  An active item can be a Tool, a <see cref="RibbonTabItem"/> or a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>Items are active only when the <see cref="XamRibbon"/> is in KeyTip or navigation mode.  At all other times, the ActiveItem property returns null.</p>
		/// </remarks>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="RibbonGroup"/>
		//[Description("Returns the currently active item in the XamRibbon. An active item can be a Tool, a 'RibbonTabItem' or a 'RibbonGroup'.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		public FrameworkElement ActiveItem
		{
			get { return this._activeItem; }
		}

		internal void SetActiveItem(FrameworkElement newActiveItem)
		{
			Debug.Assert(null == newActiveItem || this.Mode != RibbonMode.Normal, "We're changing the ActiveItem while in Normal mode!");

			FrameworkElement previousActiveItem = this._activeItem;

			if (newActiveItem == previousActiveItem)
				return;

			// get the active item as an IRibbonTool here so we can use it below as well
			IRibbonTool irtNewActiveItem = newActiveItem as IRibbonTool;


			// Set the new active tool
			this._activeItem = newActiveItem;

			RoutedPropertyChangedEventArgs<FrameworkElement> args = new RoutedPropertyChangedEventArgs<FrameworkElement>(previousActiveItem, newActiveItem);
			args.RoutedEvent = XamRibbon.ActiveItemChangedEvent;
			args.Source = newActiveItem;
			this.RaiseActiveItemChanged(args);


			// Reset the IsActive property on the previous active tool (if any) and raise the Deactivated event.
			if (previousActiveItem != null)
			{
				XamRibbon.SetIsActive(previousActiveItem, false);

				ItemDeactivatedEventArgs itemDeactivatedEventArgs = new ItemDeactivatedEventArgs(previousActiveItem);
				itemDeactivatedEventArgs.RoutedEvent = MenuTool.DeactivatedEvent;
				itemDeactivatedEventArgs.Source = previousActiveItem;

				IRibbonTool irtPreviousActiveItem = previousActiveItem as IRibbonTool;
				if (irtPreviousActiveItem != null)
					RibbonToolProxy.RaiseToolEvent(irtPreviousActiveItem, itemDeactivatedEventArgs);
				else
					previousActiveItem.RaiseEvent(itemDeactivatedEventArgs);
			}


			// Set the IsActive property on the new active tool (if any) and raise the Activated event.
			if (newActiveItem != null)
			{
				XamRibbon.SetIsActive(newActiveItem, true);

				ItemActivatedEventArgs itemActivatedEventArgs = new ItemActivatedEventArgs(newActiveItem);
				itemActivatedEventArgs.RoutedEvent = MenuTool.ActivatedEvent;
				itemActivatedEventArgs.Source = newActiveItem;

				if (irtNewActiveItem != null)
					RibbonToolProxy.RaiseToolEvent(irtNewActiveItem, itemActivatedEventArgs);
				else
					newActiveItem.RaiseEvent(itemActivatedEventArgs);

				// focus the active item if our mode is not normal
				switch (this._ribbonMode)
				{
					case RibbonMode.ActiveItemNavigation:
					case RibbonMode.KeyTipsActive:
						{
							// AS 10/10/07
							// Do not put focus on the item itself if it already has focus within it.
							//
							if (newActiveItem.IsKeyboardFocusWithin == false)
								newActiveItem.Focus();
							break;
						}
				}
			}
		}
				

				#endregion //ActiveItem	
    	
				#region AllowMinimize

		/// <summary>
		/// Identifies the <see cref="AllowMinimize"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register("AllowMinimize",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets whether the <see cref="XamRibbon"/> can be minimized.  When the <see cref="XamRibbon"/> is minimized the content of the <see cref="RibbonTabItem"/>s is not visible.  
		/// </summary>
		/// <remarks>
		/// <p class="body">When a <see cref="RibbonTabItem"/> is clicked while the <see cref="XamRibbon"/> is minimized, the <see cref="RibbonGroup"/>s that belong to the 
		/// clicked <see cref="RibbonTabItem"/> are displayed in a popup window.</p>
		/// </remarks>
		/// <seealso cref="AllowMinimizeProperty"/>
		/// <seealso cref="XamRibbon"/>
		/// <seealso cref="SelectedTab"/>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="IsMinimized"/>
		//[Description("Returns/sets whether the XamRibbon can be minimized.  When the XamRibbon is minimized only the tabs are visible.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool AllowMinimize
		{
			get
			{
				return (bool)this.GetValue(XamRibbon.AllowMinimizeProperty);
			}
			set
			{
				this.SetValue(XamRibbon.AllowMinimizeProperty, value);
			}
		}

				#endregion //AllowMinimize

				#region ApplicationMenu

		/// <summary>
		/// Identifies the <see cref="ApplicationMenu"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ApplicationMenuProperty = DependencyProperty.Register("ApplicationMenu",
			typeof(ApplicationMenu), typeof(XamRibbon), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnApplicationMenuChanged)));

		private static void OnApplicationMenuChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = target as XamRibbon;

			if (ribbon != null)
			{
				Debug.Assert(ribbon._applicationMenu == e.OldValue);

				// Remove the old menu from the logical tree
				if (ribbon._applicationMenu != null)
				{
					ribbon.RemoveLogicalChildHelper(ribbon._applicationMenu);
				}

				ribbon._applicationMenu = e.NewValue as ApplicationMenu;

				// add the new menu to the logical tree
				if (ribbon._applicationMenu != null)
				{
					ribbon.AddLogicalChildHelper(ribbon._applicationMenu);
				}
			}
		}

		/// <summary>
		/// Returns/sets the <see cref="ApplicationMenu"/> that is displayed when the <see cref="ApplicationMenu"/> button (displayed in the upper left corner of the <see cref="XamRibbon"/>) is clicked.
		/// </summary>
		/// <seealso cref="ApplicationMenuProperty"/>
		/// <seealso cref="ApplicationMenu"/>
		/// <seealso cref="XamRibbon"/>
		//[Description("Returns/sets the ApplicationMenu that is displayed when the ApplicationMenu button (displayed in the upper left corner of the XamRibbon) is clicked.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ApplicationMenu ApplicationMenu
		{
			get
			{
				return (ApplicationMenu)this.GetValue(XamRibbon.ApplicationMenuProperty);
			}
			set
			{
				this.SetValue(XamRibbon.ApplicationMenuProperty, value);
			}
		}

				#endregion //ApplicationMenu                

				// JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region ApplicationMenuMargin

        private static readonly DependencyPropertyKey ApplicationMenuMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("ApplicationMenuMargin",
            typeof(Thickness), typeof(XamRibbon), new FrameworkPropertyMetadata(new Thickness(0, 2, 0, 0)));

        /// <summary>
        /// Identifies the <see cref="ApplicationMenuMargin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ApplicationMenuMarginProperty =
            ApplicationMenuMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the margin to be applied to the ApplicationMenu (read-only)
        /// </summary>
        /// <seealso cref="ApplicationMenuMarginProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [Browsable(false)]
        public Thickness ApplicationMenuMargin
        {
            get
            {
                return (Thickness)this.GetValue(XamRibbon.ApplicationMenuMarginProperty);
            }
        }

                #endregion //ApplicationMenuMargin

				#region AutoHideEnabled

		/// <summary>
		/// Identifies the <see cref="AutoHideEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoHideEnabledProperty = DependencyProperty.Register("AutoHideEnabled",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnAutoHideEnabledChanged)));

		private static void OnAutoHideEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((XamRibbon)d).InvalidateMeasure();
		}

		/// <summary>
		/// Returns a boolean indicating whether the <see cref="XamRibbon"/> will automatically hide its contents when the width/height of the containing root visual (e.g. Window) is below the horizontal (<see cref="AutoHideHorizontalThreshold"/>) and/or vertical (<see cref="AutoHideVerticalThreshold"/>) threshold.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>When the <see cref="XamRibbon"/> 'hides' its contents, the entire ribbon (including <see cref="RibbonTabItem"/>s, <see cref="RibbonGroup"/>s, the <see cref="QuickAccessToolbar"/> and the <see cref="ApplicationMenu"/>) is hidden from view.
		/// When the containing root visual is subsequently resized so that both its width/height exceeds the <see cref="AutoHideHorizontalThreshold"/>) and <see cref="AutoHideVerticalThreshold"/> the <see cref="XamRibbon"/> is displayed.</p>
		/// </remarks>
		/// <seealso cref="AutoHideEnabledProperty"/>
		/// <seealso cref="AutoHideHorizontalThreshold"/>
		/// <seealso cref="AutoHideVerticalThreshold"/>
		/// <seealso cref="AutoHideState"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="ApplicationMenu"/>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="RibbonGroup"/>
		//[Description("Returns a boolean indicating whether the 'XamRibbon' will automatically hide its contents when the width/height of the root containing visual (e.g. Window) is below the horizontal/vertical threshold.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool AutoHideEnabled
		{
			get
			{
				return (bool)this.GetValue(XamRibbon.AutoHideEnabledProperty);
			}
			set
			{
				this.SetValue(XamRibbon.AutoHideEnabledProperty, value);
			}
		}

				#endregion //AutoHideEnabled

				// AS 10/24/07 AutoHide
				#region AutoHideHorizontalThreshold

		/// <summary>
		/// Identifies the <see cref="AutoHideHorizontalThreshold"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoHideHorizontalThresholdProperty = DependencyProperty.Register("AutoHideHorizontalThreshold",
			typeof(double), typeof(XamRibbon), new FrameworkPropertyMetadata(300d), new ValidateValueCallback(ValidateZeroOrPositiveDouble));

		private static bool ValidateZeroOrPositiveDouble(object newValue)
		{
			double value = (double)newValue;

			return double.IsNaN(value) == false && value >= 0;
		}

		/// <summary>
		/// Returns/sets the minimum width of the containing root visual (e.g. the window) at which the contents <see cref="XamRibbon"/> will be visible.
		/// </summary>
		/// <remarks>
		/// <p class="body">When the width of the root visual that contains the ribbon is below the AutoHideHorizontalThreshold or the height is below 
		/// the <see cref="AutoHideVerticalThreshold"/>, the contents of the ribbon (including RibbonGroups, RibbonTabItems, QuickAccessToolbar and ApplicationMenu) will be hidden.</p>
		/// <p class="note"><b>Note:</b> This property is only used if the <see cref="AutoHideEnabled"/> property has been set to true. AutoHideEnabled defaults to false.</p>
		/// </remarks>
		/// <seealso cref="AutoHideHorizontalThresholdProperty"/>
		/// <seealso cref="AutoHideEnabled"/>
		/// <seealso cref="AutoHideState"/>
		/// <seealso cref="AutoHideVerticalThreshold"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="ApplicationMenu"/>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="RibbonGroup"/>
		//[Description("Returns/sets the minimum width of the containing root visual (e.g. the window) at which the contents XamRibbon will be visible.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public double AutoHideHorizontalThreshold
		{
			get
			{
				return (double)this.GetValue(XamRibbon.AutoHideHorizontalThresholdProperty);
			}
			set
			{
				this.SetValue(XamRibbon.AutoHideHorizontalThresholdProperty, value);
			}
		}

				#endregion //AutoHideHorizontalThreshold

				// AS 10/24/07 AutoHide
				#region AutoHideState

		private static readonly DependencyPropertyKey AutoHideStatePropertyKey =
			DependencyProperty.RegisterReadOnly("AutoHideState",
			typeof(RibbonAutoHideState), typeof(XamRibbon), new FrameworkPropertyMetadata(RibbonAutoHideState.NotHidden, new PropertyChangedCallback(OnAutoHideStateChanged)));

		private static void OnAutoHideStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 11/5/07
			// The Ribbon will be enabled/disabled based on the ribbon auto hide state.
			//
			d.CoerceValue(UIElement.IsEnabledProperty);
		}

		/// <summary>
		/// Identifies the <see cref="AutoHideState"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoHideStateProperty =
			AutoHideStatePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the current auto hide state of the <see cref="XamRibbon"/>; that is, whether the contents of the ribbon are hidden because the width and/or height of the containing visual are below the minimum thresholds.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>When the <see cref="XamRibbon"/> 'hides' its contents, the entire ribbon (including <see cref="RibbonTabItem"/>s, <see cref="RibbonGroup"/>s, the <see cref="QuickAccessToolbar"/> and the <see cref="ApplicationMenu"/>) is hidden from view.
		/// When the containing root visual is subsequently resized so that both its width/height exceeds the <see cref="AutoHideHorizontalThreshold"/>) and <see cref="AutoHideVerticalThreshold"/> the <see cref="XamRibbon"/> displayed.</p>
		/// </remarks>
		/// <seealso cref="AutoHideStateProperty"/>
		/// <seealso cref="AutoHideEnabled"/>
		/// <seealso cref="AutoHideHorizontalThreshold"/>
		/// <seealso cref="AutoHideVerticalThreshold"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="ApplicationMenu"/>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="RibbonGroup"/>
		//[Description("Returns the current auto hide state of the ribbon; that is, whether the contents of the ribbon are hidden because the size of the containing visual are below the minimum threshold.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public RibbonAutoHideState AutoHideState
		{
			get
			{
				return (RibbonAutoHideState)this.GetValue(XamRibbon.AutoHideStateProperty);
			}
		}

				#endregion //AutoHideState

				// AS 10/24/07 AutoHide
				#region AutoHideVerticalThreshold

		/// <summary>
		/// Identifies the <see cref="AutoHideVerticalThreshold"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoHideVerticalThresholdProperty = DependencyProperty.Register("AutoHideVerticalThreshold",
			typeof(double), typeof(XamRibbon), new FrameworkPropertyMetadata(300d), new ValidateValueCallback(ValidateZeroOrPositiveDouble));

		/// <summary>
		/// Returns/sets the minimum height of the containing root visual (e.g. the window) at which the contents <see cref="XamRibbon"/> will be visible.
		/// </summary>
		/// <remarks>
		/// <p class="body">When the width of the root visual that contains the ribbon is below the <see cref="AutoHideHorizontalThreshold"/> or the height is below 
		/// the AutoHideVerticalThreshold, the contents of the ribbon (including RibbonGroups, RibbonTabItems, QuickAccessToolbar and ApplicationMenu) will be hidden.</p>
		/// <p class="note"><b>Note:</b> This property is only used if the <see cref="AutoHideEnabled"/> property has been set to true. AutoHideEnabled defaults to false.</p>
		/// </remarks>
		/// <seealso cref="AutoHideVerticalThresholdProperty"/>
		/// <seealso cref="AutoHideState"/>
		/// <seealso cref="AutoHideEnabled"/>
		/// <seealso cref="AutoHideHorizontalThreshold"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="ApplicationMenu"/>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="RibbonGroup"/>
		//[Description("Returns/sets the minimum height of the containing root visual (e.g. the window) at which the contents XamRibbon will be visible.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public double AutoHideVerticalThreshold
		{
			get
			{
				return (double)this.GetValue(XamRibbon.AutoHideVerticalThresholdProperty);
			}
			set
			{
				this.SetValue(XamRibbon.AutoHideVerticalThresholdProperty, value);
			}
		}

				#endregion //AutoHideVerticalThreshold

				// JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region CaptionPanelMargin

        private static readonly DependencyPropertyKey CaptionPanelMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("CaptionPanelMargin",
            typeof(Thickness), typeof(XamRibbon), new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Identifies the <see cref="CaptionPanelMargin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CaptionPanelMarginProperty =
            CaptionPanelMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the margin to be applied to the CaptionPanel (read-only)
        /// </summary>
        /// <seealso cref="CaptionPanelMarginProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [Browsable(false)]
        public Thickness CaptionPanelMargin
        {
            get
            {
                return (Thickness)this.GetValue(XamRibbon.CaptionPanelMarginProperty);
            }
        }

                #endregion //CaptionPanelMargin

                #region ContextualTabGroups

        /// <summary>
	    /// Returns a modifiable collection of <see cref="ContextualTabGroup"/> instances that represents all <see cref="ContextualTabGroup"/>s (visible and not visible)
	    /// in the <see cref="XamRibbon"/>.
	    /// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The individual <see cref="ContextualTabGroup"/> instances in this collection are only visible if the <see cref="ContextualTabGroup.IsVisible"/> property
		/// is set to true on the instance and the <see cref="ContextualTabGroup.Tabs"/> collection of the instance contains at least one visible <see cref="RibbonTabItem"/>.</p>
		/// </remarks>
		/// <seealso cref="ContextualTabGroup"/>
		/// <seealso cref="ContextualTabGroup.IsVisible"/>
		//[Description("Returns a modifiable collection of ContextualTabGroup instances that represents all the ContextualTabGroups (visible and not visible) in the XamRibbon.")]
	    //[Category("Ribbon Properties")]
	    [ReadOnly(true)]
	    public ContextualTabGroupCollection ContextualTabGroups
	    {
		    get
		    {
			    if (this._contextualTabGroups == null)
			    {
				    this._contextualTabGroups						= new ContextualTabGroupCollection();
				    this._contextualTabGroups.CollectionChanged		+= new NotifyCollectionChangedEventHandler(OnContextualTabGroupsCollectionChanged);
				    this._contextualTabGroups.ItemPropertyChanged	+= new EventHandler<ItemPropertyChangedEventArgs>(OnContextualTabGroupsItemPropertyChanged);
			    }

			    return this._contextualTabGroups;
		    }
	    }

				    #endregion //ContextualTabGroups	

				// AS 8/19/11 TFS83576
				#region GlassCaptionItemForeground

		private static readonly DependencyPropertyKey GlassCaptionItemForegroundPropertyKey =
			DependencyProperty.RegisterReadOnly("GlassCaptionItemForeground",
			typeof(Brush), typeof(XamRibbon), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="GlassCaptionItemForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GlassCaptionItemForegroundProperty =
			GlassCaptionItemForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush that should be used as the foreground for an item in the caption area of the window.
		/// </summary>
		/// <seealso cref="GlassCaptionItemForegroundProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public Brush GlassCaptionItemForeground
		{
			get
			{
				return (Brush)this.GetValue(XamRibbon.GlassCaptionItemForegroundProperty);
			}
		}

				#endregion //GlassCaptionItemForeground

				// AS 6/3/08 BR32772
				// Moved from XamRibbonWindow
				//
				#region IsGlassActive

		internal static readonly DependencyPropertyKey IsGlassActivePropertyKey =
			// AS 2/22/08 BR30647
			// Changed to inherited attached property so we can change our bindings to check the attached
			// property instead of incurring an element walk to find the xamribbonwindow which could result
			// in a databinding trace error if its not hosted within a xamribbonwindow.
			//
			//DependencyProperty.RegisterReadOnly("IsGlassActive",
			//typeof(bool), typeof(XamRibbonWindow), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			DependencyProperty.RegisterAttachedReadOnly("IsGlassActive",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior,
				new PropertyChangedCallback(OnIsGlassActiveChanged) // AS 8/19/11 TFS83576
				));

		// AS 8/19/11 TFS83576
		private static void OnIsGlassActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ribbon = d as XamRibbon;

			if (null != ribbon)
				ribbon.OnIsGlassActiveChanged();
		}

		// AS 8/19/11 TFS83576
		private void OnIsGlassActiveChanged()
		{
			this.VerifyIsGlassCaptionGlowVisible();
		}

		/// <summary>
		/// Identifies the IsGlassActive attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsGlassActive"/>
		/// <seealso cref="XamRibbonWindow.IsGlassActive"/>
		public static readonly DependencyProperty IsGlassActiveProperty =
			IsGlassActivePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the window is rendering using Vista Glass.
		/// </summary>
		/// <value>
		/// True if Vista Glass is enabled (read-only)
		/// </value>
		/// <seealso cref="IsGlassActiveProperty"/>
		/// <seealso cref="XamRibbonWindow.IsGlassActive"/>
		public static bool GetIsGlassActive(DependencyObject d)
		{
			return (bool)d.GetValue(IsGlassActiveProperty);
		}

				#endregion //IsGlassActive

				// AS 8/19/11 TFS83576
				#region IsGlassCaptionGlowVisible

		private static readonly DependencyPropertyKey IsGlassCaptionGlowVisiblePropertyKey =
			DependencyProperty.RegisterReadOnly("IsGlassCaptionGlowVisible",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsGlassCaptionGlowVisible"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsGlassCaptionGlowVisibleProperty =
			IsGlassCaptionGlowVisiblePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether items in the caption area should display an outer glow.
		/// </summary>
		/// <seealso cref="IsGlassCaptionGlowVisibleProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsGlassCaptionGlowVisible
		{
			get
			{
				return (bool)this.GetValue(XamRibbon.IsGlassCaptionGlowVisibleProperty);
			}
		}

				#endregion //IsGlassCaptionGlowVisible

				#region IsInHighContrastMode

			private static readonly DependencyPropertyKey IsInHighContrastModePropertyKey =
				DependencyProperty.RegisterReadOnly("IsInHighContrastMode",
				typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsHighContrastModeChanged)));

		private static void OnIsHighContrastModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = (XamRibbon)d;
			ResourceDictionary rd = true.Equals(e.NewValue) ? RibbonHighContrast.Instance : new ResourceDictionary();
			bool addedDictionary = false;
			FrameworkElement resourcesSource = (FrameworkElement)ribbon.RibbonWindow ?? (FrameworkElement)ribbon;

			if (null != ribbon._highContrastDictionary)
			{
				for (int i = 0, count = resourcesSource.Resources.MergedDictionaries.Count; i < count; i++)
				{
					if (resourcesSource.Resources.MergedDictionaries[i] == ribbon._highContrastDictionary)
					{
						resourcesSource.Resources.MergedDictionaries[i] = rd;
						addedDictionary = true;
						break;
					}
				}
			}

			// keep track of the dictionary
			ribbon._highContrastDictionary = rd;

			if (addedDictionary == false)
				resourcesSource.Resources.MergedDictionaries.Insert(0, rd);

			resourcesSource.InvalidateMeasure();
			resourcesSource.UpdateLayout();
		}

			/// <summary>
			/// Identifies the <see cref="IsInHighContrastMode"/> dependency property
			/// </summary>
			public static readonly DependencyProperty IsInHighContrastModeProperty =
				IsInHighContrastModePropertyKey.DependencyProperty;

			/// <summary>
			/// Returns a boolean indicating if the Ribbon is using the system high contrast color scheme.
			/// </summary>
			/// <remarks>
			/// <p class="body">When the <see cref="SystemParameters.HighContrast"/> property is true or the 
			/// <see cref="SystemColors.ControlColor"/> and <see cref="SystemColors.ControlTextColor"/> are black and white, 
			/// the system is considered to be in high contrast mode. When in high contrast mode, the <b>IsInHighContrastMode</b> 
			/// will be set to true and the theme of the XamRibbon is changed to utilize the system colors for accessibility purposes.</p>
			/// </remarks>
			/// <seealso cref="IsInHighContrastModeProperty"/>
			//[Description("Returns a boolean indicating if the Ribbon is using the system high contrast color scheme.")]
			//[Category("Ribbon Properties")]
			[Bindable(true)]
			[ReadOnly(true)]
			public bool IsInHighContrastMode
			{
				get
				{
					return (bool)this.GetValue(XamRibbon.IsInHighContrastModeProperty);
				}
			}

				#endregion //IsInHighContrastMode

				#region IsMinimized

		/// <summary>
		/// Identifies the <see cref="IsMinimized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMinimizedProperty = DependencyProperty.Register("IsMinimized",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets whether the XamRibbon is minimized.  
		/// </summary>
		/// <remarks>
		/// <p class="body">When minimized, the contents of the selected <see cref="RibbonTabItem"/> (<see cref="SelectedTab"/>) will not be visible and the height of the <see cref="XamRibbon"/> will be 
		/// decreased to show only the <see cref="ApplicationMenu"/> button, <see cref="QuickAccessToolbar"/> and the <see cref="RibbonTabItem"/>s.</p>
		/// <p class="body">When a <see cref="RibbonTabItem"/> is clicked while the <see cref="XamRibbon"/> is minimized, the <see cref="RibbonGroup"/>s that belong to the 
		/// clicked tab are displayed in a popup window.</p>
		/// <p class="body">If the <see cref="AllowMinimize"/> property is set to true, the end user can change the minimized state of the XamRibbon using 
		/// either the right click context menu or by pressing <b>Ctrl-F1</b>.</p>
		/// </remarks>
		/// <seealso cref="IsMinimizedProperty"/>
		/// <seealso cref="AllowMinimize"/>
		/// <seealso cref="SelectedTab"/>
		//[Description("Returns/sets whether the XamRibbon is minimized.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsMinimized
		{
			get
			{
				return (bool)this.GetValue(XamRibbon.IsMinimizedProperty);
			}
			set
			{
				this.SetValue(XamRibbon.IsMinimizedProperty, value);
			}
		}

				#endregion //IsMinimized

				#region IsWithinRibbonWindow

		private static readonly DependencyPropertyKey IsWithinRibbonWindowPropertyKey =
			DependencyProperty.RegisterReadOnly("IsWithinRibbonWindow",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsWithinRibbonWindow"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWithinRibbonWindowProperty =
			IsWithinRibbonWindowPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="XamRibbon"/> is being hosted within a <see cref="XamRibbonWindow"/>, otherwise returns false.
		/// </summary>
		/// <remarks>
		/// <p class="body">When the XamRibbon is hosted within the <see cref="RibbonWindowContentHost.Ribbon"/> of a <see cref="XamRibbonWindow"/>, 
		/// the Ribbon will be displayed within the caption area of the window and will display the <see cref="Window.Title"/> of the 
		/// containing window within its caption area.</p>
		/// </remarks>
		/// <seealso cref="IsWithinRibbonWindowProperty"/>
		/// <seealso cref="XamRibbonWindow"/>
		/// <seealso cref="RibbonWindowContentHost"/>
		//[Description("Returns true if the XamRibbon is being hosted within a 'XamRibbonWindow', otherwise returns false.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsWithinRibbonWindow
		{
			get
			{
				return (bool)this.GetValue(XamRibbon.IsWithinRibbonWindowProperty);
			}
		}

				#endregion //IsWithinRibbonWindow

				#region IsRibbonGroupResizingEnabled

		/// <summary>
		/// Identifies the <see cref="IsRibbonGroupResizingEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsRibbonGroupResizingEnabledProperty = DependencyProperty.Register("IsRibbonGroupResizingEnabled",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether the <see cref="RibbonGroup"/> instances in the selected <see cref="RibbonTabItem"/> should be resized when there is not enough room to display the <see cref="RibbonGroup"/>s within the <see cref="XamRibbon"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, the <see cref="RibbonGroup"/> instances within the <see cref="SelectedTab"/> are resized when there is 
		/// not enough room to display all their contents at their preferred sizes. The types and order of the resize operations can be controlled 
		/// using the <see cref="RibbonGroup.Variants"/> collection of the RibbonGroup. This property provides a global mechanism for 
		/// disabling all RibbonGroup resizing regardless of what <see cref="GroupVariant"/> instances have been defined.</p>
		/// </remarks>
		/// <seealso cref="IsRibbonGroupResizingEnabledProperty"/>
		/// <seealso cref="RibbonGroup"/>
		/// <seealso cref="RibbonGroup.Variants"/>
		/// <seealso cref="GroupVariant"/>
		/// <seealso cref="RibbonGroupPanel"/>
		//[Description("Returns/sets a boolean indicating whether the 'RibbonGroup' instances in the selected tab should be resized when there is not enough room to display the groups within the Ribbon.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsRibbonGroupResizingEnabled
		{
			get
			{
				return (bool)this.GetValue(XamRibbon.IsRibbonGroupResizingEnabledProperty);
			}
			set
			{
				this.SetValue(XamRibbon.IsRibbonGroupResizingEnabledProperty, value);
			}
		}

				#endregion //IsRibbonGroupResizingEnabled

                // JJD 05/14/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region IsScenicTheme

         /// <summary>
        /// Identifies the <see cref="IsScenicTheme"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsScenicThemeProperty =
            RibbonWindowContentHost.IsScenicThemePropertyKey.DependencyProperty.AddOwner(typeof(XamRibbon));

        /// <summary>
        /// Returns true if a 'Scenic' theme is being applied (read-only)
        /// </summary>
        /// <seealso cref="IsScenicThemeProperty"/>
        //[Description("Returns true if a 'Scenic' is being applied (read-only)")]
        //[Category("Behavior")]
        [ReadOnly(true)]
        [Browsable(false)]
        public bool IsScenicTheme
        {
            get
            {
                return (bool)this.GetValue(RibbonWindowContentHost.IsScenicThemeProperty);
            }
        }

                #endregion //IsScenicTheme

				#region PopupResizerBarStyleKey

		/// <summary>
		/// The key used to identify the style used for the <see cref="Infragistics.Windows.Controls.PopupResizerBar"/> used within the <see cref="XamRibbon"/>
		/// </summary>
		public static readonly ResourceKey PopupResizerBarStyleKey = new StaticPropertyResourceKey(typeof(XamRibbon), "PopupResizerBarStyleKey");

				#endregion //PopupResizerBarStyleKey

				#region QuickAccessToolbar

		/// <summary>
		/// Identifies the <see cref="QuickAccessToolbar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty QuickAccessToolbarProperty = DependencyProperty.Register("QuickAccessToolbar",
			typeof(QuickAccessToolbar), typeof(XamRibbon), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnQatChanged)));

		private static void OnQatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = d as XamRibbon;

            if (ribbon != null)
            {
                // AS 2/5/09 TFS11796
                // Keep the qat as a logical child of the ribbon like we do for 
                // the ApplicationMenu so we can always get to the ribbon from 
                // the qat to ensure its still used by the ribbon.
                //
                if (e.OldValue != null)
                    ribbon.RemoveLogicalChildHelper(e.OldValue);

                if (e.NewValue != null)
                    ribbon.AddLogicalChildHelper(e.NewValue);

                ribbon.VerifyQatLocation();
            }
		}

		/// <summary>
		/// Returns/sets the <see cref="QuickAccessToolbar"/> for the <see cref="XamRibbon"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="QuickAccessToolbar"/> is a user customizable toolbar that contains often used tools. The tools 
		/// added to the QAT must exist within the <see cref="XamRibbon"/> - in a <see cref="RibbonGroup"/>, <see cref="ApplicationMenu"/>, 
		/// <see cref="ApplicationMenuFooterToolbar"/>, a <see cref="MenuTool"/> within the ribbon or within the <see cref="ToolsNotInRibbon"/> 
		/// collection.</p>
		/// </remarks>
		/// <seealso cref="QuickAccessToolbarProperty"/>
		/// <seealso cref="XamRibbon"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="QuickAccessToolbarLocation"/>
		//[Description("Returns/sets the QuickAccessToolbar for the XamRibbon.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public QuickAccessToolbar QuickAccessToolbar
		{
			get
			{
				return (QuickAccessToolbar)this.GetValue(XamRibbon.QuickAccessToolbarProperty);
			}
			set
			{
				this.SetValue(XamRibbon.QuickAccessToolbarProperty, value);
			}
		}

				#endregion //QuickAccessToolbar

				// JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region QuickAccessToolbarMargin

        private static readonly DependencyPropertyKey QuickAccessToolbarMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("QuickAccessToolbarMargin",
            typeof(Thickness), typeof(XamRibbon), new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Identifies the <see cref="QuickAccessToolbarMargin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty QuickAccessToolbarMarginProperty =
            QuickAccessToolbarMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the margin to be applied to the QuickAccessToolbar (read-only)
        /// </summary>
        /// <seealso cref="QuickAccessToolbarMarginProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [Browsable(false)]
        public Thickness QuickAccessToolbarMargin
        {
            get
            {
                return (Thickness)this.GetValue(XamRibbon.QuickAccessToolbarMarginProperty);
            }
        }

                #endregion //QuickAccessToolbarMargin

				#region QuickAccessToolbarLocation

		/// <summary>
		/// Identifies the <see cref="QuickAccessToolbarLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty QuickAccessToolbarLocationProperty = DependencyProperty.Register("QuickAccessToolbarLocation",
			typeof(QuickAccessToolbarLocation), typeof(XamRibbon), new FrameworkPropertyMetadata(QuickAccessToolbarLocation.AboveRibbon, new PropertyChangedCallback(OnQatLocationChanged)));

		private static void OnQatLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = d as XamRibbon;

			if (null != ribbon)
				ribbon.VerifyQatLocation();
		}

		/// <summary>
		/// Returns/sets a value that indicates where the <see cref="QuickAccessToolbar"/> should be displayed. Possible locations include above the <see cref="RibbonTabItem"/> area to the right of the <see cref="ApplicationMenu"/> or
		/// below the <see cref="XamRibbon"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, the <see cref="QuickAccessToolbar"/> is displayed above the ribbon within the caption area. The location may be 
		/// changed programatically using this property or by the end user using the right click customize menu.</p>
		/// </remarks>
		/// <seealso cref="QuickAccessToolbarLocationProperty"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.QuickAccessToolbar.IsBelowRibbon"/>
		//[Description("Returns/sets a value that indicates where the QuickAccessToolbar should be displayed.  Possible locations include above the ribbon tab area or below the ribbon.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public QuickAccessToolbarLocation QuickAccessToolbarLocation
		{
			get
			{
				return (QuickAccessToolbarLocation)this.GetValue(XamRibbon.QuickAccessToolbarLocationProperty);
			}
			set
			{
				this.SetValue(XamRibbon.QuickAccessToolbarLocationProperty, value);
			}
		}

				#endregion //QuickAccessToolbarLocation

				#region RibbonTabControlStyleKey (static)

		private static ResourceKey _ribbonTabControlStyleKey = new StaticPropertyResourceKey(typeof(XamRibbon), "RibbonTabControlStyleKey");

		/// <summary>
		/// Returns the resource key used to indentify the style for the <see cref="XamTabControl"/> contained within the <see cref="XamRibbon"/>.
		/// </summary>
		public static ResourceKey RibbonTabControlStyleKey
		{
			get { return XamRibbon._ribbonTabControlStyleKey; }
		}

				#endregion //RibbonTabControlStyleKey (static)	

				#region SelectedTab

		/// <summary>
		/// Identifies the <see cref="SelectedTab"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedTabProperty = DependencyProperty.Register("SelectedTab",
			typeof(RibbonTabItem), typeof(XamRibbon), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedTabChanged)));

		private static void OnSelectedTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = d as XamRibbon;

			if (null != ribbon)
			{
				RibbonTabItem tab = e.NewValue as RibbonTabItem;

				if (tab != null && tab.IsInContextualTabGroup)
					ribbon.RibbonTabControl.SetValue(IsSelectedItemInContextualTabGroupPropertyKey, KnownBoxes.TrueBox);
				else
					ribbon.RibbonTabControl.ClearValue(IsSelectedItemInContextualTabGroupPropertyKey);

				// if we are in ActiveItemNavigation mode then set the ActiveItem to the tab
				if (tab != null && ribbon._ribbonMode == RibbonMode.ActiveItemNavigation)
					ribbon.SetActiveItem( tab );

				// AS 10/1/07
				// When we are not in keytip or navigation mode then make sure that the currently selected
				// tab is the focusedelement within the ribbon so that it will receive focus when the ribbon
				// does.
				//
				if (ribbon._ribbonMode == RibbonMode.Normal)
					ribbon.SetSelectedTabAsFocusElement();
			}
		}

		/// <summary>
		/// Returns/sets the currently selected <see cref="RibbonTabItem"/>.
		/// </summary> 
		/// <p class="body">The SelectedTab determines which tab's contents are displayed to the end user. When one tab is 
		/// selected, the previously selected tab will be unselected. The selected tab may be changed using this property, using the 
		/// <see cref="SelectNextTab()"/> method, the <see cref="SelectPreviousTab()"/> method, setting the <see cref="TabItem.IsSelected"/> 
		/// property, using the keyboard (e.g. keytips), using the mouse or by hiding the selected tab.</p>
		/// <p class="body">When the SelectedTab is changed, the <see cref="RibbonTabItemSelected"/> event will be raised.</p>
		/// <seealso cref="SelectedTabProperty"/>
		/// <seealso cref="RibbonTabItemSelected"/>
		/// <seealso cref="TabItem.IsSelected"/>
		/// <seealso cref="SelectNextTab"/>
		/// <seealso cref="SelectPreviousTab"/>
		//[Description("Returns/sets the selected tab.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[Browsable(false)]	// JM 10-24-07
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]  // JM 10-24-07
		public RibbonTabItem SelectedTab
		{
			get
			{
				return (RibbonTabItem)this.GetValue(XamRibbon.SelectedTabProperty);
			}
			set
			{
				this.SetValue(XamRibbon.SelectedTabProperty, value);
			}
		}

				#endregion //SelectedTab

				// JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region TabAreaMargin

        private static readonly DependencyPropertyKey TabAreaMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("TabAreaMargin",
            typeof(Thickness), typeof(XamRibbon), new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Identifies the <see cref="TabAreaMargin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TabAreaMarginProperty =
            TabAreaMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the margin to be applied to the TabArea (read-only)
        /// </summary>
        /// <seealso cref="TabAreaMarginProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [Browsable(false)]
        public Thickness TabAreaMargin
        {
            get
            {
                return (Thickness)this.GetValue(XamRibbon.TabAreaMarginProperty);
            }
        }

                #endregion //TabAreaMargin

				#region Tabs

		/// <summary>
		/// Returns a collection of <see cref="RibbonTabItem"/> instances that represents tabs added directly to the Ribbon. This does not include 
		/// <see cref="RibbonTabItem"/>s added to the <see cref="ContextualTabGroup.Tabs"/> collection.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>To retrieve all the displayed <see cref="RibbonTabItem"/>s from this <see cref="Tabs"/> collection as well as 
		/// the <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Tabs"/> collection, you can use the <see cref="TabsInView"/> property.</p>
		/// </remarks>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="ContextualTabGroup.Tabs"/>
		/// <seealso cref="TabsInView"/>
		//[Description("Returns a collection of RibbonTabItems that represents tabs added directly to the Ribbon.  This does not include RibbonTabItems added to the ContextualTabGroups collection.")]
		//[Category("Ribbon Properties")]
		[ReadOnly(true)]
		public ObservableCollection<RibbonTabItem> Tabs
		{
			get
			{
				if (this._tabs == null)
				{
					this._tabs						= new ObservableCollection<RibbonTabItem>();
					this._tabs.CollectionChanged	+= new NotifyCollectionChangedEventHandler(OnTabsCollectionChanged);
				}

				return this._tabs;
			}
		}

				#endregion //Tabs	

				#region TabsInView

		/// <summary>
		/// Returns a read-only collection of the <see cref="RibbonTabItem"/>s that are currently in view.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>This collection includes <see cref="RibbonTabItem"/>s defined in the <see cref="Tabs"/> collection as well as 
		/// <see cref="RibbonTabItem"/>s defined in the <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Tabs"/> collection of all the 
		/// <see cref="ContextualTabGroups"/> that are currently visible.</p>
		/// </remarks>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="ContextualTabGroup.Tabs"/>
		/// <seealso cref="Tabs"/>
		//[Description("Returns a read-only collection of the RibbonTabItems that are currently in view.")]
		//[Category("Ribbon Properties")]
		[ReadOnly(true)]
		[Browsable(false)]	// JM BR28530 11-20-07
		public ReadOnlyObservableCollection<RibbonTabItem> TabsInView
		{
			get
			{
				if (this._tabsInView == null)
				{
					this._tabsInView		= new ReadOnlyObservableCollection<RibbonTabItem>(this._tabsInViewInternal);

					this.UpdateTabsInView();
				}

				return this._tabsInView;
			}
		}

				#endregion //TabsInView	

				#region Theme

		#region Old Version
		
#region Infragistics Source Cleanup (Region)

























































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //Old Version
		/// <summary>
		/// Identifies the 'Theme' dependency property
		/// </summary>
		public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(XamRibbon), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChanged)));

		/// <summary>
		/// Event ID for the 'ThemeChanged' routed event
		/// </summary>
		public static readonly RoutedEvent ThemeChangedEvent = ThemeManager.ThemeChangedEvent.AddOwner(typeof(XamRibbon));

		private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon control = target as XamRibbon;

			control.UpdateThemeResources();
			control.OnThemeChanged((string)(e.OldValue), (string)(e.NewValue));
		}

		/// <summary>
		/// Gets/sets the default look for the control.
		/// </summary>
		/// <remarks>
		/// <para class="body">If left set to null then the default 'Generic' theme will be used. This property can 
		/// be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
		/// <para></para>
		/// <para class="body">The following themes are pre-registered by this assembly but additional themes can be registered as well.
		/// <ul>
		/// <li>"Aero" - a theme that is compatible with Vista's 'Aero' theme.</li>
		/// <li>"Generic" - the default theme.</li>
		/// <li>"LunaNormal" - a theme that is compatible with XP's 'blue' theme.</li>
		/// <li>"LunaOlive" - a theme that is compatible with XP's 'olive' theme.</li>
		/// <li>"LunaSilver" - a theme that is compatible with XP's 'silver' theme.</li>
		/// <li>"Office2k7Black" - a theme that is compatible with MS Office 2007's 'Black' theme.</li>
		/// <li>"Office2k7Blue" - a theme that is compatible with MS Office 2007's 'Blue' theme.</li>
		/// <li>"Office2k7Silver" - a theme that is compatible with MS Office 2007's 'Silver' theme.</li>
		/// <li>"Office2010Blue" - a theme that is compatible with MS Office 2010's 'Blue' theme.</li>
		/// <li>"Onyx" - a theme that features black and orange highlights.</li>
		/// <li>"Royale" - a theme that features subtle blue highlights.</li>
		/// <li>"RoyaleStrong" - a theme that features strong blue highlights.</li>
		/// <li>"Scenic" - a theme that emulates the Windows 7 Scenic Ribbon.</li>
		/// </ul>
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
		/// <seealso cref="ThemeProperty"/>
		//[Description("Gets/sets the default look for the control.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[DefaultValue((string)null)]
		[TypeConverter(typeof(Infragistics.Windows.Themes.Internal.RibbonThemeTypeConverter))]
		public string Theme
		{
			get
			{
				return (string)this.GetValue(XamRibbon.ThemeProperty);
			}
			set
			{
				this.SetValue(XamRibbon.ThemeProperty, value);
			}
		}

		/// <summary>
		/// Called when property 'Theme' changes
		/// </summary>
		protected virtual void OnThemeChanged(string previousValue, string currentValue)
		{
			RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>(previousValue, currentValue);
			newEvent.RoutedEvent = XamRibbon.ThemeChangedEvent;
			newEvent.Source = this;
			RaiseEvent(newEvent);
		}

		/// <summary>
		/// Invoked when the 'Theme' property has been changed.
		/// </summary>
		//[Description("Invoked when the 'Theme' property has been changed.")]
		//[Category("Ribbon Events")]
		public event RoutedPropertyChangedEventHandler<string> ThemeChanged
		{
			add
			{
				base.AddHandler(XamRibbon.ThemeChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamRibbon.ThemeChangedEvent, value);
			}
		}

				#endregion //Theme

				#region ToolTipStyleKey

		/// <summary>
		/// The key for the <see cref="FrameworkElement.Style"/> property of <see cref="ToolTip"/> instances used in the XamRibbon for objects such as the caption of tab items and contextual tab groups.
		/// </summary>
		public static readonly ResourceKey ToolTipStyleKey = new StaticPropertyResourceKey(typeof(XamRibbon), "ToolTipStyleKey");

				#endregion //ToolTipStyleKey

				#region ToolsNotInRibbon

		/// <summary>
		/// Returns a modifiable collection of the Tools that are not sited on a <see cref="RibbonGroup"/>, <see cref="ApplicationMenuFooterToolbar"/> or <see cref="MenuTool"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="ToolsNotInRibbon"/> collection is a convenient place to store tools that are not initially sited anywhere in the <see cref="XamRibbon"/>
		/// but are available to the end user for placement on the <see cref="QuickAccessToolbar"/> via its Quick Customize Menu.  Tools in this collection that have
		/// their <see cref="ButtonTool.IsQatCommonTool"/> property set to true will appear in the <see cref="QuickAccessToolbar"/>'s Quick Customize Menu.  Developers can also 
		/// use the tools in this collection to populate a list in a developer-written ribbon customization dialog.</p>
		/// </remarks>
		/// <seealso cref="QuickAccessToolbar"/>
		//[Description("Returns a modifiable collection of the Tools that are not sited on a RibbonGroup, ApplicationMenuFooterToolbar or MenuTool.")]
		//[Category("Ribbon Properties")]
		[ReadOnly(true)]
		public ToolsNotInRibbonCollection ToolsNotInRibbon
		{
			get
			{
				if (this._toolsNotInRibbon == null)
				{
					this._toolsNotInRibbon						= new ToolsNotInRibbonCollection(this);
				}

				return this._toolsNotInRibbon;
			}
		}

				#endregion //ToolsNotInRibbon	
	
			#endregion //Public Properties

			#region Internal Properties
    
				#region AutoGenerateKeyTips
		internal bool AutoGenerateKeyTips
		{
			get { return true; }
		} 
				#endregion //AutoGenerateKeyTips

				#region CaptionHeight

				// AS 11/2/07 CaptionHeight - WorkItem #562
				// Changed to a dependency property so we can bind it to the height of the caption element and
				// pick up any changes at runtime.
				//
				/// <summary>
				/// Used to obtain the actual height of the caption element.
				/// </summary>
                // JJD 11/30/07 - BR27696
                // CaptionHeightProperty moved to RibbonWindowContentHost
				//internal static readonly DependencyProperty CaptionHeightProperty = XamRibbonWindow.CaptionHeightProperty.AddOwner(typeof(XamRibbon));
				internal static readonly DependencyProperty CaptionHeightProperty = RibbonWindowContentHost.CaptionHeightProperty.AddOwner(typeof(XamRibbon));

				#endregion //CaptionHeight

				// AS 11/16/07 BR28515
				// We use this to identify tools that are in the ToolsNotInRibbon collection.
				//
				#region IsInToolsNotInRibbon

				/// <summary>
				/// Used to indicate if a tool is part of the ToolsNotInRibbonCollection.
				/// </summary>
				internal static readonly DependencyProperty IsInToolsNotInRibbonProperty = DependencyProperty.Register("IsInToolsNotInRibbon",
					typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

				/// <summary>
				/// Indicates if the tool is part of the tools not in ribbon collection.
				/// </summary>
				/// <seealso cref="IsInToolsNotInRibbonProperty"/>
				internal static bool GetIsInToolsNotInRibbon(DependencyObject d)
				{
					return true.Equals(d.GetValue(IsInToolsNotInRibbonProperty));
				}

				#endregion //IsInToolsNotInRibbon

				#region IsModeSpecial

		private static readonly DependencyPropertyKey IsModeSpecialPropertyKey =
			DependencyProperty.RegisterReadOnly("IsModeSpecial",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		internal static readonly DependencyProperty IsModeSpecialProperty =
			IsModeSpecialPropertyKey.DependencyProperty;

				#endregion //IsModeSpecial

                // JJD 4/30/10 - NA 2010 Volumne 2 - Scenic Ribbon
                #region IsScenicThemeInternal

        internal static readonly DependencyProperty IsScenicThemeInternalProperty = DependencyProperty.Register("IsScenicThemeInternal",
            typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsScenicThemeChanged)));

        private static void OnIsScenicThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamRibbon ribbon = target as XamRibbon;

            ribbon.SetValue(RibbonWindowContentHost.IsScenicThemePropertyKey, e.NewValue);

            if (ribbon._ribbonWindow != null)
                ribbon._ribbonWindow.SynchronizeIsScenicTheme();
        }

        internal bool IsScenicThemeInternal
        {
            get
            {
                return (bool)this.GetValue(XamRibbon.IsScenicThemeInternalProperty);
            }
            set
            {
                this.SetValue(XamRibbon.IsScenicThemeInternalProperty, value);
            }
        }

                #endregion //IsScenicThemeInternal

				#region IsToolRegistrationSuspended
		internal bool IsToolRegistrationSuspended
		{
			get { return this._toolRegistrationSuspendCount > 0; }
		} 
				#endregion //IsToolRegistrationSuspended

				#region KeyTipManager

		internal KeyTipManager KeyTipManager
		{
			get
			{
				if (this._keyTipManager == null)
					this._keyTipManager = new KeyTipManager(this, this);

				return this._keyTipManager;
			}
		}

		internal bool HasKeyTipManager
		{
			get { return this._keyTipManager != null; }
		}
			
				#endregion KeyTipManager

				#region Mode

		internal RibbonMode Mode { get { return this._ribbonMode; } }

				#endregion //Mode	

				#region RibbonGroupItemsHeight

		/// <summary>
		/// Identifies the <see cref="RibbonGroupItemsHeight"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty RibbonGroupItemsHeightProperty = DependencyProperty.Register("RibbonGroupItemsHeight",
			typeof(double), typeof(XamRibbon), new FrameworkPropertyMetadata(double.NaN));

		/// <summary>
		/// Returns the height for the ItemsPresenter within a ribbon group.
		/// </summary>
		internal double RibbonGroupItemsHeight
		{
			get
			{
				return (double)this.GetValue(XamRibbon.RibbonGroupItemsHeightProperty);
			}
			set
			{
				this.SetValue(XamRibbon.RibbonGroupItemsHeightProperty, value);
			}
		}

				#endregion //RibbonGroupItemsHeight

				#region RibbonGroupSizeVersion

		internal static readonly DependencyProperty RibbonGroupSizeVersionProperty = DependencyProperty.Register("RibbonGroupSizeVersion",
			typeof(int), typeof(XamRibbon), new FrameworkPropertyMetadata(0));

				#endregion //RibbonGroupSizeVersion

				#region RibbonTabControl

        internal XamTabControl RibbonTabControl
		{
			get
			{
				if (this._ribbonTabControl == null)
				{
					this._ribbonTabControl					= new XamTabControl();
					this._ribbonTabControl.ItemsSource		= this.TabsInView;
					this._ribbonTabControl.SetResourceReference(FrameworkElement.StyleProperty, XamRibbon.RibbonTabControlStyleKey);

					// always place the tabs on top and use an autosize layout
					this._ribbonTabControl.TabStripPlacement = Dock.Top;
					this._ribbonTabControl.TabLayoutStyle	= TabLayoutStyle.SingleRowSizeToFit;

                    // AS 10/16/08 TFS9193
                    this._ribbonTabControl.Focusable = false;

					this.SetBinding(XamRibbon.SelectedTabProperty, Utilities.CreateBindingObject(TabControl.SelectedItemProperty, BindingMode.TwoWay, this._ribbonTabControl));

					// bind the tab item content height to the height of our
					// test ribbon group
					this._ribbonTabControl.SetBinding(XamTabControl.TabItemContentHeightProperty, Utilities.CreateBindingObject(FrameworkElement.ActualHeightProperty, BindingMode.OneWay, this.RibbonGroupContainerForSizing));

					this._ribbonTabControl.SetBinding(XamTabControl.AllowMinimizeProperty, Utilities.CreateBindingObject(XamRibbon.AllowMinimizeProperty, BindingMode.TwoWay, this));
					
					this._ribbonTabControl.SetBinding(XamTabControl.IsMinimizedProperty, Utilities.CreateBindingObject(XamRibbon.IsMinimizedProperty, BindingMode.TwoWay, this));

					// AS 6/29/12 TFS114953
					this.SetBinding( TabHeaderHeightProperty, new Binding() { Path = new PropertyPath(XamTabControl.TabHeaderHeightProperty), Source = _ribbonTabControl } );

					this._tabControlPopupManager = new TabControlPopupManager(this);

					if (this._ribbonTabControlSite != null)
						this._ribbonTabControlSite.Content = this._ribbonTabControl;

					// JM 10-24-07
					this._ribbonTabControl.DropDownClosed	+= new RoutedEventHandler(OnTabControlDropDownClosed);
					this._ribbonTabControl.DropDownOpened	+= new RoutedEventHandler(OnTabControlDropDownOpened);
					this._ribbonTabControl.DropDownOpening	+= new EventHandler<TabControlDropDownOpeningEventArgs>(OnTabControlDropDownOpening);
					this._ribbonTabControl.SelectionChanged += new SelectionChangedEventHandler(OnTabControlSelectionChanged);
				}

				return this._ribbonTabControl;
			}
		}

				#endregion //RibbonTabControl	
    
				#region RibbonWindow

		private static readonly DependencyPropertyKey RibbonWindowPropertyKey =
			DependencyProperty.RegisterReadOnly("RibbonWindow",
			// AS 6/3/08 BR32772
			//typeof(XamRibbonWindow), typeof(XamRibbon), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnRibbonWindowChanged)));
			typeof(IRibbonWindow), typeof(XamRibbon), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnRibbonWindowChanged)));

		private static void OnRibbonWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = (XamRibbon)d;
			// AS 6/3/08 BR32772
			//XamRibbonWindow oldWindow = e.OldValue as XamRibbonWindow;
			//XamRibbonWindow newWindow = e.NewValue as XamRibbonWindow;
			IRibbonWindow oldWindow = e.OldValue as IRibbonWindow;
			IRibbonWindow newWindow = e.NewValue as IRibbonWindow;

			// we're caching the reference on the ribbon
			ribbon._ribbonWindow = newWindow;

			if (newWindow != null)
				ribbon.SetValue(XamRibbon.IsWithinRibbonWindowPropertyKey, KnownBoxes.TrueBox);
			else
				ribbon.ClearValue(XamRibbon.IsWithinRibbonWindowPropertyKey);

			// AS 8/19/11 TFS83576
			ribbon.VerifyIsGlassCaptionGlowVisible();

			// AS 5/9/08
			//if (ribbon._theme != null)
			if (ribbon.Theme != null)
			{
				ribbon.UpdateThemeResources();
				
				// clear out the resources at the XamRibbon so we don't step on changes at the XamRibbonWindow
				if ( newWindow != null )
					ThemeManager.OnThemeChanged(ribbon, null, new string[] { PrimitivesGeneric.Location.Grouping, EditorsGeneric.Location.Grouping, RibbonGeneric.Location.Grouping });
			}
		}

		/// <summary>
		/// Identifies the <see cref="RibbonWindow"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty RibbonWindowProperty =
			RibbonWindowPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the containing XamRibbonWindow
		/// </summary>
		/// <seealso cref="RibbonWindowProperty"/>
		// AS 6/3/08 BR32772
		//internal XamRibbonWindow RibbonWindow
		internal IRibbonWindow RibbonWindow
		{
			get
			{
				// AS 6/3/08 BR32772
				//return (XamRibbonWindow)this.GetValue(XamRibbon.RibbonWindowProperty);
				return (IRibbonWindow)this.GetValue(XamRibbon.RibbonWindowProperty);
			}
		}

				#endregion //RibbonWindow

				// AS 10/22/07 BR27620
				// Also handle when we are in active mode.
				//
				#region ShouldCaptureMouseForRibbonMode
		internal bool ShouldCaptureMouseForRibbonMode
		{
			get
			{
				return this.IsWithinRibbonWindow == false &&
					(this._ribbonMode == RibbonMode.KeyTipsActive || this._ribbonMode == RibbonMode.ActiveItemNavigation);
			}
		} 
				#endregion //ShouldCaptureMouseForRibbonMode

				#region CaptionHeight

				// AS 6/29/12 TFS114953
				internal static readonly DependencyProperty TabHeaderHeightProperty = RibbonWindowContentHost.TabHeaderHeightProperty.AddOwner(typeof(XamRibbon));

				#endregion //CaptionHeight

				#region TabItemArrangeVersion

		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

				#endregion //TabItemArrangeVersion

				#region ToolInstanceManager

		internal ToolInstanceManager ToolInstanceManager
		{
			get { return this._toolInstanceManager; }
		}

				#endregion //ToolInstanceManager	
    
				// AS 9/23/09 TFS22369
				#region TopLevelVisual
		internal FrameworkElement TopLevelVisual
		{
			get
			{
				return _topLevelVisual;
			}
		} 
				#endregion //TopLevelVisual

				// AS 10/12/07 UseLargeImages
				#region UseLargeImagesOnMenu

		
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

				#endregion //UseLargeImagesOnMenu

			#endregion //Internal Properties

			#region Attached Properties

				#region Caption

		
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

				#endregion //Caption

				#region ClonedFromTool

		internal static readonly DependencyProperty ClonedFromToolProperty = DependencyProperty.RegisterAttached("ClonedFromTool",
			typeof(FrameworkElement), typeof(XamRibbon), new FrameworkPropertyMetadata(null));

		internal static FrameworkElement GetClonedFromTool(DependencyObject d)
		{
			return (FrameworkElement)d.GetValue(XamRibbon.ClonedFromToolProperty);
		}

		internal static void SetClonedFromTool(DependencyObject d, FrameworkElement value)
		{
			d.SetValue(XamRibbon.ClonedFromToolProperty, value);
		}

				#endregion //ClonedFromTool

				#region HasCaption

		
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				#endregion //HasCaption

				#region HasImage

		
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				#endregion //HasImage

				#region Id

		
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

				#endregion //Id

				#region ImageResolved

		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				#endregion //ImageResolved

				#region IsActive

		internal static readonly DependencyPropertyKey IsActivePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsActive",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the IsActive dependency property.
		/// </summary>
		/// <seealso cref="GetIsActive"/>
		public static readonly DependencyProperty IsActiveProperty =
			IsActivePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the specified item is active.
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		/// <seealso cref="ActiveItem"/>
		public static bool GetIsActive(DependencyObject d)
		{
			return (bool)d.GetValue(XamRibbon.IsActiveProperty);
		}

		internal static void SetIsActive(DependencyObject d, bool value)
		{
			d.SetValue(XamRibbon.IsActivePropertyKey, KnownBoxes.FromValue(value));
		}

				#endregion //IsActive

				#region IsOnQat

		
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

				#endregion //IsOnQat

				#region IsQatCommonTool

		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

				#endregion //IsQatCommonTool

				#region IsRegistered

		internal static readonly DependencyProperty IsRegisteredProperty = DependencyProperty.RegisterAttached("IsRegistered",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		internal static bool GetIsRegistered(DependencyObject d)
		{
			return (bool)d.GetValue(XamRibbon.IsRegisteredProperty);
		}

		internal static void SetIsRegistered(DependencyObject d, bool value)
		{
			d.SetValue(XamRibbon.IsRegisteredProperty, value);
		}

				#endregion //IsRegistered

				#region IsSelectedItemInContextualTabGroup

		internal static readonly DependencyPropertyKey IsSelectedItemInContextualTabGroupPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsSelectedItemInContextualTabGroup",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the IsSelectedItemInContextualTabGroup" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsSelectedItemInContextualTabGroup"/>
		public static readonly DependencyProperty IsSelectedItemInContextualTabGroupProperty =
			IsSelectedItemInContextualTabGroupPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns a boolean indicating if the selected item of the specified <see cref="XamTabControl"/> used 
		/// within the <see cref="XamRibbon"/> belongs to a <see cref="ContextualTabGroup"/>.
		/// </summary>
		/// <seealso cref="IsSelectedItemInContextualTabGroupProperty"/>
		[AttachedPropertyBrowsableForType(typeof(XamTabControl))]
		public static bool GetIsSelectedItemInContextualTabGroup(DependencyObject d)
		{
			return (bool)d.GetValue(XamRibbon.IsSelectedItemInContextualTabGroupProperty);
		}

				#endregion //IsSelectedItemInContextualTabGroup

				#region KeyTip

		
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)

				#endregion //KeyTip

				#region KeyTipPlacementList

		private static readonly DependencyProperty KeyTipPlacementListProperty = DependencyProperty.Register("KeyTipPlacementList",
			typeof(IList<WeakReference>), typeof(XamRibbon), new FrameworkPropertyMetadata(null));

				#endregion //KeyTipPlacementList

				
				#region KeyTipPlacementType

		/// <summary>
		/// KeyTipPlacementType is used within a style for a ribbon tool to identify elements for which key tip placement may be based. 
		/// </summary>
		/// <seealso cref="GetKeyTipPlacementType"/>
		/// <seealso cref="SetKeyTipPlacementType"/>
		public static readonly DependencyProperty KeyTipPlacementTypeProperty = DependencyProperty.RegisterAttached("KeyTipPlacementType",
			typeof(KeyTipPlacementType?), typeof(XamRibbon), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnKeyTipPlacementChanged)));

		private static void OnKeyTipPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 6/14/12 TFS113987
			if (DesignerProperties.GetIsInDesignMode(d))
				return;

			// we're only keeping a list of elements so don't do anything if the value is just 
			// changing from one type to another.
			if (e.NewValue == null || e.OldValue == null)
			{
				UIElement placementElement = d as UIElement;

				if (null != placementElement)
				{
					// when the key tip placement changes we need to update our store
					// on the key tip provider
					DependencyObject keyTipProvider = d;

					while (keyTipProvider != null &&
						false == keyTipProvider is IRibbonTool &&
						(false == keyTipProvider is ToolMenuItem || keyTipProvider is MenuToolPresenter) &&
						false == keyTipProvider is IKeyTipProvider)
					{
						DependencyObject previous = keyTipProvider;

						keyTipProvider = (keyTipProvider is FrameworkElement)
							? ((FrameworkElement)keyTipProvider).TemplatedParent
							: null;

						keyTipProvider = keyTipProvider ?? LogicalTreeHelper.GetParent(previous) ?? VisualTreeHelper.GetParent(previous);
					}

					Debug.Assert(e.NewValue == null || null != keyTipProvider || VisualTreeHelper.GetParent(placementElement) == null, "We didn't find an element that would use the keytipplacement info. Make sure it should be used here.");

					if (null != keyTipProvider)
					{
						IList<WeakReference> placementList = (IList<WeakReference>)keyTipProvider.GetValue(KeyTipPlacementListProperty);

						// if its being removed...
						if (null != placementList && e.NewValue == null)
						{
							for (int i = 0; i < placementList.Count; i++)
							{
								WeakReference reference = placementList[i];
								UIElement element = Utilities.GetWeakReferenceTargetSafe(reference) as UIElement;

								if (element == null || element == placementElement)
								{
									placementList.RemoveAt(i);
									i--;

									if (element == placementElement)
										break;
								}
							}
						}
						else if (e.OldValue == null)
						{
							if (null == placementList)
							{
								placementList = new List<WeakReference>();
								keyTipProvider.SetValue(KeyTipPlacementListProperty, placementList);
							}

							placementList.Add(new WeakReference(placementElement));
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the value of the 'KeyTipPlacementType' attached property
		/// </summary>
		/// <seealso cref="KeyTipPlacementTypeProperty"/>
		/// <seealso cref="SetKeyTipPlacementType"/>
		[AttachedPropertyBrowsableForType(typeof(Image))]
		[AttachedPropertyBrowsableForType(typeof(TextBlock))]
		[AttachedPropertyBrowsableForType(typeof(ContentPresenter))]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<KeyTipPlacementType>))] // AS 5/15/08 BR32816
		public static KeyTipPlacementType? GetKeyTipPlacementType(DependencyObject d)
		{
			return (KeyTipPlacementType?)d.GetValue(XamRibbon.KeyTipPlacementTypeProperty);
		}

		/// <summary>
		/// Sets the value of the 'KeyTipPlacementType' attached property
		/// </summary>
		/// <seealso cref="KeyTipPlacementTypeProperty"/>
		/// <seealso cref="GetKeyTipPlacementType"/>
		public static void SetKeyTipPlacementType(DependencyObject d, KeyTipPlacementType? value)
		{
			d.SetValue(XamRibbon.KeyTipPlacementTypeProperty, value);
		}

				#endregion //KeyTipPlacementType

				#region HideAccessKey

		private static readonly DependencyPropertyKey HideAccessKeyPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("HideAccessKey",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Used to determine if the access key for a tool should be hidden.
		/// </summary>
		public static readonly DependencyProperty HideAccessKeyProperty =
			HideAccessKeyPropertyKey.DependencyProperty;

		/// <summary>
		/// Used to determine if the access key for a tool should be hidden. Access keys (i.e. mnemonics) are only displayed for tools within a menu.
		/// </summary>
		/// <seealso cref="HideAccessKeyProperty"/>
		public static bool GetHideAccessKey(DependencyObject d)
		{
			return (bool)d.GetValue(XamRibbon.HideAccessKeyProperty);
		}

				#endregion //HideAccessKey

				#region LargeImage

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

				#endregion //LargeImage

				#region Location

		internal static readonly DependencyPropertyKey LocationPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("Location",
			typeof(ToolLocation), typeof(XamRibbon), new FrameworkPropertyMetadata(RibbonKnownBoxes.ToolLocationUnknownBox, new PropertyChangedCallback(OnLocationChanged)));

		private static void OnLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonToolHelper.UpdateImageProperties(d);

			switch ((ToolLocation)e.NewValue)
			{
				case ToolLocation.QuickAccessToolbar:
				case ToolLocation.Ribbon:
					d.ClearValue(HideAccessKeyPropertyKey);
					break;
				case ToolLocation.ApplicationMenu:
				case ToolLocation.ApplicationMenuFooterToolbar:
				case ToolLocation.ApplicationMenuRecentItems:
				case ToolLocation.ApplicationMenuSubMenu:
				case ToolLocation.Menu:
				case ToolLocation.Unknown:
					d.SetValue(HideAccessKeyPropertyKey, KnownBoxes.FalseBox);
					break;
				default:
					Debug.Fail("unrecognized location!");
					d.SetValue(HideAccessKeyPropertyKey, KnownBoxes.FalseBox);
					break;
			}
		}

		/// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		/// <seealso cref="GetLocation"/>
		public static readonly DependencyProperty LocationProperty =
			LocationPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns an enumeration that indicates the location for the specified item.
		/// </summary>
		/// <seealso cref="LocationProperty"/>
		/// <seealso cref="ToolLocation"/>
		public static ToolLocation GetLocation(DependencyObject d)
		{
			return (ToolLocation)d.GetValue(XamRibbon.LocationProperty);
		}

		internal static void SetLocation(DependencyObject d, ToolLocation value)
		{
			d.SetValue(XamRibbon.LocationPropertyKey, RibbonKnownBoxes.FromValue(value));
		}

				#endregion //Location

                // AS 2/10/09 TFS12207
                #region RegisteredOwner

        internal static readonly DependencyProperty RegisteredOwnerProperty =
            DependencyProperty.RegisterAttached("RegisteredOwner", typeof(XamRibbon), typeof(XamRibbon),
                new FrameworkPropertyMetadata(null));

        internal static XamRibbon GetRegisteredOwner(DependencyObject d)
        {
            return (XamRibbon)d.GetValue(RegisteredOwnerProperty);
        }

        internal static void SetRegisteredOwner(DependencyObject d, XamRibbon value)
        {
            d.SetValue(RegisteredOwnerProperty, value);
        }

                #endregion // RegisteredOwner

				#region Ribbon

		// AS 10/1/09 TFS20404
		// I had to change the RibbonProperty from a readonly inherited attached property to a read-write 
		// property because there seems to be a bug in the framework where the value is not being propogated 
		// to the descendants. We had hit something like this before but managed to work around that at the 
		// time by removing the logical children of the ribbon from its logical tree during the applytemplate. 
		// However in this case the OnApplyTemplate never gets invoked when the theme is changed and so that 
		// workaround wasn't hit. Changing the property seems to address the issue so while end users are 
		// not meant to set this property we had to make it non-readonly.
		//
		//internal static readonly DependencyPropertyKey RibbonPropertyKey =
		//    DependencyProperty.RegisterAttachedReadOnly("Ribbon",
		/// <summary>
		/// Identifies the Ribbon attached readonly inherited dependency property
		/// </summary>
		/// <seealso cref="GetRibbon"/>
		internal static readonly DependencyProperty RibbonProperty =
			DependencyProperty.RegisterAttached("Ribbon",
			typeof(XamRibbon), typeof(XamRibbon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, new PropertyChangedCallback(OnRibbonChanged)));

					#region OnRibbonChanged
		private static void OnRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 10/1/09 TFS20404
			Debug.Assert(d is XamRibbon || e.NewValue == null || DependencyPropertyHelper.GetValueSource(d, e.Property).BaseValueSource == BaseValueSource.Inherited);

			FrameworkElement tool = d as FrameworkElement;
			IRibbonTool irt = tool as IRibbonTool;

			// register all ribbon tools automatically

			// the ribbon tool panels are not tools but I wanted to make use of the 
			// delayed processing of these elements since i don't want to dirty variant
			// info just because a ribbon group's contents are being moved from the ribbon
			// group to the popup.
			//
			//if (null != irt && tool is ApplicationMenu == false)
			if ( (null != irt && tool is ApplicationMenu == false) ||
				tool is IRibbonToolPanel )
			{
				// AS 11/12/07 BR28406
				// When an element is in the visual tree and logical tree and is then removed from the 
				// visual tree, we may get a property change notification for the inherited property
				// even though the value has not changed. If this happens then ignore the property
				// change. We were getting in here while the theme was changed because the templates
				// of the elements were changing (and therefore their visual tree). So while we got a 
				// property change notification indicating the new value was null, it really wasn't null 
				// because the element was still in the logical tree. We processed the null value as an 
				// indication to remove the tool instance but we never got another property change 
				// indicating the value was still the ribbon.
				// 
				if (false == object.ReferenceEquals(d.GetValue(XamRibbon.RibbonProperty), e.NewValue))
					return;

				XamRibbon oldRibbon = (XamRibbon)e.OldValue;
				XamRibbon newRibbon = (XamRibbon)e.NewValue;

				
				if (null != oldRibbon)
					oldRibbon.UnregisterTool(tool);

				if (null != newRibbon)
					newRibbon.RegisterTool(tool);
			}
		}
					#endregion //OnRibbonChanged

		// AS 10/1/09 TFS20404
		///// <summary>
		///// Identifies the Ribbon attached readonly inherited dependency property
		///// </summary>
		///// <seealso cref="GetRibbon"/>
		//public static readonly DependencyProperty RibbonProperty =
		//    RibbonPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns the <see cref="XamRibbon"/> that contains the specified object or null (Nothing in VB) if the object is not contained within a XamRibbon.
		/// </summary>
		/// <seealso cref="RibbonProperty"/>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants=true)]
		public static XamRibbon GetRibbon(DependencyObject d)
		{
			return (XamRibbon)d.GetValue(XamRibbon.RibbonProperty);
		}

				#endregion //Ribbon

				#region SizingMode

		
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

				#endregion //SizingMode

				#region SmallImage

		
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

				#endregion //SmallImage

			#endregion //Attached Properties

			#region Private Properties

				// AS 12/4/07 HighContrast Support
				#region ControlColor

		private static readonly DependencyProperty ControlColorProperty = DependencyProperty.Register("ControlColor",
			typeof(Color), typeof(XamRibbon), new FrameworkPropertyMetadata(Colors.Transparent, new PropertyChangedCallback(VerifyHighContrast)));

				#endregion //ControlColor

				// AS 12/4/07 HighContrast Support
				#region ControlTextColor

		private static readonly DependencyProperty ControlTextColorProperty = DependencyProperty.Register("ControlTextColor",
			typeof(Color), typeof(XamRibbon), new FrameworkPropertyMetadata(Colors.Transparent, new PropertyChangedCallback(VerifyHighContrast)));

				#endregion //ControlTextColor

				// AS 12/4/07 HighContrast Support
				#region IsSystemHighContrast

		private static readonly DependencyProperty IsSystemHighContrastProperty = DependencyProperty.Register("IsSystemHighContrast",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(VerifyHighContrast)));

				#endregion //IsSystemHighContrast

				#region RibbonGroupContainerForSizing
		private UIElement RibbonGroupContainerForSizing
		{
			get
			{
				if (this._ribbonGroupContainerForSizing == null)
				{
					RibbonGroupCollection groups = CreateRibbonGroupsForSizing(out this._ribbonGroupForSizing);
					ContentPresenter cp = new ContentPresenter();
					cp.Name = "RibbonGroupForResizing";
					cp.Content = groups;
					this._ribbonGroupContainerForSizing = cp;

					this.AddLogicalChildHelper(this._ribbonGroupContainerForSizing);
				}

				return this._ribbonGroupContainerForSizing;
			}
		}
				#endregion //RibbonGroupContainerForSizing

				#region RibbonGroupForSizing
		private RibbonGroup RibbonGroupForSizing
		{
			get
			{
				Debug.Assert(this._ribbonGroupContainerForSizing != null);
				return this._ribbonGroupForSizing;
			}
		} 
				#endregion //RibbonGroupForSizing

				// AS 10/24/07 AutoHide
				#region TopLevelVisualActualHeight

		/// <summary>
		/// Identifies the <see cref="TopLevelVisualActualHeight"/> dependency property
		/// </summary>
		private static readonly DependencyProperty TopLevelVisualActualHeightProperty = DependencyProperty.Register("TopLevelVisualActualHeight",
			typeof(double), typeof(XamRibbon), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnTopLevelVisualActualHeightChanged)));

		private static void OnTopLevelVisualActualHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = d as XamRibbon;

			double verticalThreshold = ribbon.AutoHideVerticalThreshold;

			// AS 1/3/08 BR29371
			// The toplevelvisual can be null.
			//
			if (ribbon._topLevelVisual == null)
				return;

			// if the ribbon is not collapsed but the root visual's height goes below the 
			// threshold then invalidate the ribbon so it can hide its contents if needed
			if (verticalThreshold > 0 && ribbon._topLevelVisual.Height < verticalThreshold && ribbon.AutoHideState == RibbonAutoHideState.NotHidden)
				ribbon.InvalidateMeasure();
			else
			{
				double horizontalThreshold = ribbon.AutoHideHorizontalThreshold;

				bool isCollapsedHorizontal = horizontalThreshold > 0 && ribbon._topLevelVisual.Width < horizontalThreshold;

				// if the height goes above the vertical threshold and the ribbon is hidden (but not because
				// the width is too small) then invalidate the ribbon's measure since it should probably
				// show its contents
				//
				if (isCollapsedHorizontal == false && ribbon._topLevelVisual.Height > verticalThreshold && ribbon.AutoHideState == RibbonAutoHideState.Hidden)
					ribbon.InvalidateMeasure();
			}
		}

		/// <summary>
		/// The actual height of the top level visual containing the ribbon.
		/// </summary>
		private double TopLevelVisualActualHeight
		{
			get
			{
				return (double)this.GetValue(XamRibbon.TopLevelVisualActualHeightProperty);
			}
			set
			{
				this.SetValue(XamRibbon.TopLevelVisualActualHeightProperty, value);
			}
		}

				#endregion //TopLevelVisualActualHeight

				// AS 6/21/12 TFS114953
				#region UseScenicApplicationMenuInternal

		/// <summary>
		/// Identifies the <see cref="UseScenicApplicationMenuInternal"/> dependency property
		/// </summary>
		private static readonly DependencyProperty UseScenicApplicationMenuInternalProperty = DependencyProperty.Register("UseScenicApplicationMenuInternal",
			typeof(bool), typeof(XamRibbon), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnUseScenicApplicationMenuInternalChanged)));

		private static void OnUseScenicApplicationMenuInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = d as XamRibbon;
			ribbon.CalculateMargins();
		}

		/// <summary>
		/// The actual height of the top level visual containing the ribbon.
		/// </summary>
		private bool UseScenicApplicationMenuInternal
		{
			get
			{
				return (bool)this.GetValue(XamRibbon.UseScenicApplicationMenuInternalProperty);
			}
			set
			{
				this.SetValue(XamRibbon.UseScenicApplicationMenuInternalProperty, value);
			}
		}

				#endregion //UseScenicApplicationMenuInternal

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region ExecuteCommand

		/// <summary>
		/// Executes the RoutedCommand represented by the specified CommandWrapper.
		/// </summary>
		/// <param name="commandWrapper">The CommandWrapper that contains the RoutedCommand to execute</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="RibbonCommands"/>
		public bool ExecuteCommand(CommandWrapper commandWrapper)
		{
			if (commandWrapper == null)
				throw new ArgumentNullException("commandWrapper");


			return this.ExecuteCommandImpl(commandWrapper.Command, null);
		}

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="RibbonCommands"/>
		public bool ExecuteCommand(RoutedCommand command)
		{
			return this.ExecuteCommandImpl(command, null);
		}

				#endregion //ExecuteCommand	

				// AS 10/29/07 BR27510
				#region GetOriginalTool
		/// <summary>
		/// Returns the original tool from which the specified tool instance was cloned.
		/// </summary>
		/// <param name="tool">An instance of a tool to be evaluated</param>
		/// <returns>If the <paramref name="tool"/> was cloned from an instance of another tool then that original tool will be returned. If the tool was not cloned from another tool then the <paramref name="tool"/> will be returned.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="tool"/> is null.</exception>
		public static FrameworkElement GetOriginalTool(FrameworkElement tool)
		{
			if (null == tool)
				throw new ArgumentNullException("tool");

			return RibbonToolProxy.GetRootSourceTool(tool);
		} 
				#endregion //GetOriginalTool

				#region GetToolById

		/// <summary>
		/// Returns the tool instance with the specified Id.
		/// </summary>
		/// <param name="id">The Id of the tool to find.</param>
		/// <returns>The FrameworkElement derived tool instance or null if no tool instance exists with the specified Id.</returns>
		/// <seealso cref="RibbonToolHelper.IdProperty"/>
		public FrameworkElement GetToolById(string id)
		{
			return this.ToolInstanceManager.GetToolInstanceFromToolId(id);
		}

				#endregion //GetToolById	

				// AS 10/29/07 BR27510
				#region GetToolInEditMode
		/// <summary>
		/// Returns the logical instance of the specified tool that is currently in edit mode.
		/// </summary>
		/// <param name="valueEditor">An instance of a ValueEditor derived tool that is to be evaluated.</param>
		/// <returns>If <paramref name="valueEditor"/> or another instance of that tool within the <see cref="XamRibbon"/> with the same <see cref="RibbonToolHelper.IdProperty"/> is in edit mode, then that instance will be returned. Otherwise, null (Nothing in VB) is returned if there is no instance of the tool currently in edit mode.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="valueEditor"/> is null.</exception>
		public static ValueEditor GetToolInEditMode(ValueEditor valueEditor)
		{
			if (null == valueEditor)
				throw new ArgumentNullException("valueEditor");

			// use the tool specified if its in edit mode
			ValueEditor editorInEditMode = valueEditor.IsInEditMode ? valueEditor : null;

			// get the ribbon so we can get to the instances for this tool
			string id = RibbonToolHelper.GetId(valueEditor);

			if (string.IsNullOrEmpty(id) == false)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(valueEditor);

				if (null != ribbon)
				{
					IEnumerator instanceEnumerator = ribbon.ToolInstanceManager.GetAllInstancesWithId(id);

					if (null != instanceEnumerator)
					{
						while (instanceEnumerator.MoveNext())
						{
							ValueEditor editorInstance = instanceEnumerator.Current as ValueEditor;

							if (editorInstance != null && editorInstance.IsInEditMode)
							{
								editorInEditMode = editorInstance;
								break;
							}
						}
					}
				}
			}

			return editorInEditMode;
		} 
				#endregion //GetToolInEditMode

				#region SelectNextTab

		/// <summary>
		/// Selects the tab that follows the current <see cref="SelectedTab"/>.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>If no tab is currently selected and at least one <see cref="RibbonTabItem"/> exists in the <see cref="TabsInView"/> collection, the first 
		/// <see cref="RibbonTabItem"/> in the list will be selected.  If the <see cref="SelectedTab"/> is currently set to the last tab in the <see cref="TabsInView"/> 
		/// collection, the <see cref="SelectedTab"/> is unchanged.</p>
		/// </remarks>
		/// <seealso cref="SelectPreviousTab"/>
		/// <seealso cref="SelectedTab"/>
		/// <seealso cref="Tabs"/>
		/// <seealso cref="TabsInView"/>
		/// <seealso cref="ContextualTabGroup.Tabs"/>
		/// <seealso cref="RibbonTabItem"/>
		public void SelectNextTab()
		{
			// AS 10/25/07 BR27728
			// According to the remarks, this should select the first tab.
			//
			//RibbonTabItem currentTab = this.SelectedTab;
			//if (currentTab != null)
			{
				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

				this.SelectAdjacentTab(1);
			}
		}

				#endregion //SelectNextTab	

				#region SelectPreviousTab

		/// <summary>
		/// Selects the tab that precedes the current <see cref="SelectedTab"/>.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>If no tab is currently selected and at least one <see cref="RibbonTabItem"/> exists in the <see cref="TabsInView"/> collection, the first 
		/// <see cref="RibbonTabItem"/> in the list will be selected.  If the <see cref="SelectedTab"/> is currently set to the first tab in the <see cref="Tabs"/> 
		/// collection, the <see cref="SelectedTab"/> is unchanged.</p>
		/// </remarks>
		/// <seealso cref="SelectNextTab"/>
		/// <seealso cref="SelectedTab"/>
		/// <seealso cref="Tabs"/>
		/// <seealso cref="TabsInView"/>
		/// <seealso cref="ContextualTabGroup.Tabs"/>
		/// <seealso cref="RibbonTabItem"/>
		public void SelectPreviousTab()
		{
			// AS 10/25/07 BR27728
			// According to the remarks, this should select the first tab.
			//
			//RibbonTabItem currentTab = this.SelectedTab;
			//if (currentTab != null)
			{
				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

				this.SelectAdjacentTab(-1);
			}
		}

				#endregion //SelectPreviousTab	
    
			#endregion //Public Methods

			#region Internal Methods

				// AS 11/16/07 BR28515
				#region AddLogicalChildInternal
		/// <summary>
		/// Exposes the AddLogicalChild method of the ribbon internally.
		/// </summary>
		/// <param name="child">The child to add</param>
		internal void AddLogicalChildInternal(object child)
		{
			this.AddLogicalChild(child);
		}

				#endregion //AddLogicalChildInternal

				#region AddToolToQat

		internal bool AddToolToQat(FrameworkElement tool)
		{
			// For a ToolMenuItem use its RibbonTool property
			if (tool is ToolMenuItem)
				tool = ((ToolMenuItem)tool).RibbonTool as FrameworkElement;

			if (tool == null)
				return false;

			// If it's already on the Qat, just return true.
			if ((bool)tool.GetValue(RibbonToolHelper.IsOnQatProperty) == true)
				return true;

			// SSP 10/17/07 BR27366
			// If the tool is an editor tool, exit the edit mode first. If that fails don't
			// show the context menu. 
			
			// 
			ValueEditor toolAsEditor = tool as ValueEditor;
			if ( null != toolAsEditor && toolAsEditor.IsInEditMode )
				toolAsEditor.EndEditMode( true, false );

			string toolId = tool.GetValue(RibbonToolHelper.IdProperty) as string;
			Debug.Assert(this.QuickAccessToolbar.ContainsId(toolId) == false);

			QatPlaceholderTool qpt = new QatPlaceholderTool(toolId, tool is RibbonGroup ? QatPlaceholderToolType.RibbonGroup : QatPlaceholderToolType.Tool);
			this.QuickAccessToolbar.Items.Add(qpt);

			return true;
		}

				#endregion //AddToolToQat	

				#region BumpRibbonGroupSizeVersion
		internal void BumpRibbonGroupSizeVersion()
		{
			this.SetValue(RibbonGroupSizeVersionProperty, (int)this.GetValue(RibbonGroupSizeVersionProperty) + 1);
		} 
				#endregion //BumpRibbonGroupSizeVersion

				#region CalcKeyTipMinWidth

		internal double CalcKeyTipMinWidth()
		{
			KeyTip kt = new KeyTip();
			this.AddLogicalChild(kt);
			this.AddVisualChild(kt);
			kt.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			kt.Arrange(new Rect(kt.DesiredSize));
			this.RemoveLogicalChild(kt);
			this.RemoveVisualChild(kt);
			return kt.RenderSize.Width;
		}

				#endregion CalcKeyTipMinWidth

                // JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region CalculateMargins

            internal void CalculateMargins()
            {
                RibbonWindowContentHost contentHost = this._ribbonWindow != null ? this._ribbonWindow.ContentHost : null;
                Thickness contentHostBorder = contentHost != null ? contentHost.BorderThickness : new Thickness();
                Thickness contentHostRibbonAreaMargin = contentHost != null ? contentHost.RibbonAreaMargin : new Thickness();
                    
                if (this._ribbonWindow != null &&
                    this._ribbonWindow.IsGlassActiveInternal &&
                    this._ribbonWindow.WindowState == WindowState.Maximized)
                {
                    this.SetValue(CaptionPanelMarginPropertyKey, new Thickness(0, 4, 0, 0));
                }
                else
                {
                    this.ClearValue(CaptionPanelMarginPropertyKey);
                }

				// AS 6/21/12 TFS114953
				// We need to know that we're using a scenic style application menu (i.e. tab and not orb that replaces the control box) 
				// so we can correctly calculate the margins but we don't want to use IsScenicTheme because we don't want all the rendering 
				// implications like the scenic os based border/non-client area appearance.
				//
				//if (this._ribbonWindow != null && this._ribbonWindow.IsScenicThemeInternal)
				bool isScenicTabLayout = this.UseScenicApplicationMenuInternal;
				if (isScenicTabLayout)
                {
                    if (this._ribbonWindow != null &&
                        this._ribbonWindow.IsGlassActiveInternal)
                        this.SetValue(ApplicationMenuMarginPropertyKey, new Thickness(-2, -5, 0, 0));
                    else
                        this.SetValue(ApplicationMenuMarginPropertyKey, new Thickness(Math.Max(contentHostBorder.Left - contentHostRibbonAreaMargin.Left, 0), 1, 0, 0));

                }
                else
                {
                    if (this._ribbonWindow != null &&
                        this._ribbonWindow.IsGlassActiveInternal &&
                        this._ribbonWindow.WindowState == WindowState.Maximized)
                        this.SetValue(ApplicationMenuMarginPropertyKey, new Thickness(0, 6, 0, 0));
                    else
                        this.SetValue(ApplicationMenuMarginPropertyKey, new Thickness(0, 2, 0, 0));

                }

                if (contentHost == null ||
                    this._ribbonWindow.IsGlassActiveInternal ||
					// AS 1/5/12 TFS69350
					// We need to get to the routine below when maximized while in scenic theme.
					//
					// AS 6/21/12 TFS114953
					//(this._ribbonWindow.WindowState != WindowState.Normal && !this._ribbonWindow.IsScenicThemeInternal))
					(this._ribbonWindow.WindowState != WindowState.Normal && !isScenicTabLayout))
                {
                    this.ClearValue(QuickAccessToolbarMarginPropertyKey);
                    this.ClearValue(TabAreaMarginPropertyKey);
                    return;
                }

                Thickness tabAreaMargin = new Thickness(Math.Max(contentHostBorder.Left - contentHostRibbonAreaMargin.Left, 0), 0, Math.Max(contentHostBorder.Right - contentHostRibbonAreaMargin.Right, 0), 0);

                this.SetValue(TabAreaMarginPropertyKey, tabAreaMargin);

                if ( this.QuickAccessToolbarLocation == Ribbon.QuickAccessToolbarLocation.BelowRibbon )
                    this.SetValue(QuickAccessToolbarMarginPropertyKey, tabAreaMargin);
                else
                    this.ClearValue(QuickAccessToolbarMarginPropertyKey);


            }

                #endregion //CalculateMargins	

				// AS 9/23/09 TFS22386
				// Moved here from the QuickAccessToolbar and xamRibbon since the impl is the same except the display text.
				//
				#region CreateMinimizeMenuItem
		internal ToolMenuItem CreateMinimizeMenuItem(string displayTextResourceName)
		{
			ToolMenuItem menuItem = new ToolMenuItem();
			menuItem.Header = GetString(displayTextResourceName);
			menuItem.IsChecked = this.IsMinimized;
			// AS 9/23/09 TFS22386
			// Do not set the command because OnClick (which is invoked by the IInvokePattern will toggle
			// the IsChecked and invoke the command. Since we're handling the checked/unchecked to support 
			// the toggle pattern we only want to process this once.
			//
			//menuItem.Command		= RibbonCommands.ToggleRibbonMinimizedState;
			menuItem.CommandTarget = this;		// JM BR27006 10-3-07

			// AS 9/23/09 TFS22386
			// Setting the IsCheckable so the menu item supports the ITogglePattern. Unfortunately 
			// the ITogglePattern of the menu item only calls the OnToggle of the menu item and 
			// does not invoke the associated command so we need to handle the Checked/Unchecked 
			// events. 
			//
			menuItem.IsCheckable = true;
			RoutedEventHandler handler = delegate(object s, RoutedEventArgs e)
			{
				ToolMenuItem tmi = (ToolMenuItem)e.Source;

				// previously we had set the command on the menu item and so the click
				// and invocation of the associated command would have been invoked 
				// asynchronously. since we're not setting the command and instead relying 
				// on the checked/unchecked and these are synchronous we need to handle 
				// invoking the command asynchronously using the same priority the menu item
				// would have used.
				//
				DispatcherOperationCallback clickCallback = delegate(object param)
				{
					ToolMenuItem paramMenuItem = (ToolMenuItem)param;
					RibbonCommands.ToggleRibbonMinimizedState.Execute(paramMenuItem.CommandParameter, paramMenuItem.CommandTarget);
					return null;
				};

				tmi.Dispatcher.BeginInvoke(DispatcherPriority.Render, clickCallback, tmi);
			};
			Debug.Assert(handler.Target == null, "This should be a static method");
			menuItem.Checked += handler;
			menuItem.Unchecked += handler;

			return menuItem;
		}
				#endregion //CreateMinimizeMenuItem

				// AS 10/5/07 keyboard navigation
				#region EnterNavigationMode





		internal void EnterNavigationMode()
		{
			this.SetMode(RibbonMode.ActiveItemNavigation);
		} 
				#endregion //EnterNavigationMode

                // AS 2/5/09 TFS11796
                // Added a helper method to ensure that we have processed any pending 
                // tool/panel registrations. Otherwise when the ribbon group is trying 
                // to reset the state/size of the tools in its OnIsOpenChanged it will 
                // do so on the old tools because the new tools haven't been processed 
                // and had their containinggroup changed yet.
                //
                #region ForcePendingToolRegistrations
        internal void ForcePendingToolRegistrations()
        {
            if (_pendingToolRegOperation != null)
            {
                // if we had called ProcessPendingToolRegistrationsWithDelay and 
                // we still have a _pendingToolRegOperation then it hasn't been
                // processed yet so cancel it and process it now
                _pendingToolRegOperation.Abort();
                this.ProcessPendingToolRegistrations();
            }
            else if (_implicitResumeRegOperation != null)
            {
                // if we had implicitly started a suspend then we would have posted 
                // a delayed resume. if that operation is not yet started, end it now 
                // and process the resume without a delay
                if (_implicitResumeRegOperation.Status == DispatcherOperationStatus.Pending)
                {
                    _implicitResumeRegOperation.Abort();
                    _implicitResumeRegOperation = null;

                    this.ResumeToolRegistration(false);
                }
            }
        }
                #endregion //ForcePendingToolRegistrations

				#region GetItemToActivate
		
#region Infragistics Source Cleanup (Region)












































#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetItemToActivate	

				#region GetKeyTipPlacementElement
		internal static UIElement GetKeyTipPlacementElement(KeyTipPlacementType placementType, DependencyObject keyTipProvider)
		{
			IList<WeakReference> list = (IList<WeakReference>)keyTipProvider.GetValue(KeyTipPlacementListProperty);

			if (null != list)
			{
				for (int i = 0, count = list.Count; i < count; i++)
				{
					WeakReference reference = list[i];
					UIElement target = Utilities.GetWeakReferenceTargetSafe(reference) as UIElement;

					if (null != target)
					{
						// skip collapsed ones in case there are multiple elements for that type
						if (placementType.Equals(target.GetValue(KeyTipPlacementTypeProperty)) && 
							target.IsVisible)
							return target;
					}
					else
					{
						// remove the dead reference
						list.RemoveAt(i);
						i--;
						count--;
					}
				}
			}

			return null;
		} 
				#endregion //GetKeyTipPlacementElement

				#region GetKeyTipPlacementElementRect
		internal static Rect GetKeyTipPlacementElementRect(KeyTipPlacementType placementType, DependencyObject keyTipProvider, UIElement relativeElement)
		{
			Rect rect = Rect.Empty;
			UIElement element = GetKeyTipPlacementElement(placementType, keyTipProvider);

			if (null != element)
			{
				rect = new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height);

				Thickness margin = (Thickness)element.GetValue(FrameworkElement.MarginProperty);
				Thickness padding = (Thickness)element.GetValue(Control.PaddingProperty);

				if (null != margin)
					rect = SubtractThickness(rect, margin);

				if (null != padding)
					rect = SubtractThickness(rect, padding);

				if (element != relativeElement)
				{
					// AS 3/23/11 TFS69892
					//rect = element.TransformToVisual(relativeElement).TransformBounds(rect);
					rect = XamRibbon.TryTransformToVisual(element, relativeElement, rect) ?? rect;
				}
			}

			return rect;
		} 
				#endregion //GetKeyTipPlacementElement

				#region GetRibbonGroup
		internal RibbonGroup GetRibbonGroup(string id)
		{
			RibbonGroup group;
			return this._registeredGroups.TryGetValue(id, out group)
				? group
				: null;
		} 
				#endregion //GetRibbonGroup

				#region GetRibbonGroupSmallToolRow
		/// <summary>
		/// Gets a number between 0 and 2 that indicates the row line in the ribbon group that a 
		/// small ribbon group tool should use to display its tooltip.
		/// </summary>
		/// <param name="toolElement">The small ribbon group tool element</param>
		/// <returns>A number between 0 and 2</returns>
		internal int GetRibbonGroupSmallToolRow(FrameworkElement toolElement)
		{
			RibbonGroup group = RibbonGroup.GetContainingGroup(toolElement);

			Debug.Assert(null != group, "This method is intended to be used with tools in a RibbonGroup!");
			Debug.Assert(RibbonToolHelper.GetSizingMode(toolElement) != RibbonToolSizingMode.ImageAndTextLarge, "This method shouldn't be used for large tools - only small ones");

			if (group != null)
			{
				Debug.Assert(group.CurrentContentSite != null, "We don't have access to the site that contains the tools");

				// its possible that the part was not specified
				if (group.CurrentContentSite != null)
				{
					Debug.Assert(group.CurrentContentSite.Content != null, "The current site doesn't have the tools!");

					if (group.CurrentContentSite.Content != null)
					{
						GeneralTransform gt = null;
						try
						{
							gt = toolElement.TransformToAncestor(group.CurrentContentSite);
						}
						catch (ArgumentException)
						{
						}
						catch (InvalidOperationException)
						{
						}

						if (null != gt)
						{
							// get the mid point of the tool relative to the content site
							double offsetY = gt.Transform(new Point(0, toolElement.ActualHeight / 2)).Y;

							// get the middle row offset - this should be relative to the content site
							double middleOffset = this.RibbonGroupForSizing.GetToolRowVerticalOffset(1);

							// the midpoint of a tool has to be close to the midpt of the middle row
							if (Math.Abs(middleOffset - offsetY) < 3d)
								return 1;

							// if its below that then assume its on the bottom row
							if (middleOffset < offsetY)
								return 2;

							// otherwise its on the top row
							return 0;
						}
					}
				}
			}

			return 1;
		} 
				#endregion //GetRibbonGroupSmallToolRow

				#region GetRibbonGroupToolVerticalOffset
		/// <summary>
		/// Returns the vertical offset relative to the specified tool where the keytip should be placed for a 
		/// ribbon group tool.
		/// </summary>
		/// <param name="rowNumber">The row on which the tool exists</param>
		/// <param name="toolElement">The tool on that row</param>
		/// <returns>A vertical offset relative to the tool itself where that row line exists</returns>
		internal double GetRibbonGroupToolVerticalOffset(int rowNumber, UIElement toolElement)
		{
			RibbonGroup groupForSizing = this.RibbonGroupForSizing;

			// get the offset relative to the sizing group - this should be relative
			// to the content site (i.e. where the group border and tools actually reside
			double groupOffset = groupForSizing.GetToolRowVerticalOffset(rowNumber);

			// get the group containing the tool
			RibbonGroup containingGroup = RibbonGroup.GetContainingGroup(toolElement);

			Debug.Assert(containingGroup != null);

			// then get the ribbon group containing the tool element and translate that point
			// relative to where the tool is in the ribbon group
			if (null != containingGroup)
			{
				ContentControl cp = containingGroup.CurrentContentSite;

				if (null != cp)
				{
					GeneralTransform gt = null;

					try
					{
						// we need to transform the offset relative to the tool itself
						gt = cp.TransformToVisual(toolElement);
					}
					catch (ArgumentException)
					{
					}
					catch (InvalidOperationException)
					{
					}

					if (null != gt)
					{
						return gt.Transform(new Point(0, groupOffset)).Y;
					}
				}
			}

			// if we don't have enough info to map the points then just use the midpt
			// of the tool as the fallback.
			return toolElement.RenderSize.Height / 2;
		} 
				#endregion //GetRibbonGroupToolVerticalOffset

				#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
				#endregion // GetString

				#region InitializeRibbonWindow

		// AS 6/3/08 BR32772
		//internal void InitializeRibbonWindow(XamRibbonWindow window)
		internal void InitializeRibbonWindow(IRibbonWindow window)
		{
			this.SetValue(RibbonWindowPropertyKey, window);

			this.WireTopLevelVisual();
		}

				#endregion //InitializeRibbonWindow	
    
				#region OnKeyTipsHidden
		internal void OnKeyTipsHidden(bool isDeactivatingParentContainers, bool resetMode)
		{
			if (resetMode && this._ribbonMode != RibbonMode.Normal)
			{
				// AS 10/3/07
				// If we're deactivating the containers - i.e. closing up menus, etc - then we
				// want to restore focus - otherwise we want to leave focus as it is.
				//
				//this.SetMode(RibbonMode.Normal);
				this.SetMode(RibbonMode.Normal, isDeactivatingParentContainers);
			}
		} 
				#endregion //OnKeyTipsHidden

				#region OnRibbonGroupIdChanged
		internal void OnRibbonGroupIdChanged(RibbonGroup group, string oldId, string newId)
		{
			RibbonGroup oldGroup;

			if (string.IsNullOrEmpty(oldId) == false)
			{
				if (this._registeredGroups.TryGetValue(oldId, out oldGroup))
				{
					Debug.Assert(oldGroup == group, "Another group was registered with the specified id!");

					this._registeredGroups.Remove(oldId);
				}
				else
					Debug.Fail(string.Format("The group '{0}' was not previously registered with the specified key '{1}'!", group, oldId));
			}

			// if there is an id, store it as the key for the group
			this.VerifyGroupIsRegistered(group, newId);
		} 
				#endregion //OnRibbonGroupIdChanged

				// AS 8/19/11 TFS83576
				#region OnWindowStateChanged
		internal void OnWindowStateChanged()
		{
			this.VerifyIsGlassCaptionGlowVisible();

			// JJD 11/15/11 - TFS35011
			// Also call CalculateMargins when the window state changes
			this.CalculateMargins();
		} 
				#endregion //OnWindowStateChanged

                #region RegisterTool
                internal void RegisterTool(FrameworkElement tool)
		{
			if (this.IsToolRegistrationSuspended)
			{
			    this.AddToPendingToolRegistrations(tool, true);
			    return;
			}
			else // JM 07-09-09 TFS19110
				this.ForcePendingToolRegistrations();


            // AS 2/10/09 TFS12207
            // If the tool was registered with another ribbon and that ribbon is 
            // still waiting to process its pending registrations then we need 
            // to make sure it processes them now or else it will clear references 
            // to the containing group, isregistered state, etc. when it does 
            // process them.
            //
            XamRibbon oldOwner = XamRibbon.GetRegisteredOwner(tool);

            if (oldOwner != null && oldOwner != this)
                oldOwner.ForcePendingToolRegistrations();

			IRibbonToolLocation locationProvider;
			ToolLocation location = GetToolLocation(tool, out locationProvider);

			// AS 11/16/07 BR28515
			// Unknown is ok if its in the ToolsNotInRibbon
			//
			Debug.Assert(location != ToolLocation.Unknown || XamRibbon.GetIsInToolsNotInRibbon(tool));

			tool.SetValue(XamRibbon.LocationPropertyKey, RibbonKnownBoxes.FromValue(location));

            // AS 2/10/09 TFS12207
            // Keep a pointer back to the owning ribbon so we can clean up 
            // 
            XamRibbon.SetRegisteredOwner(tool, this);

			this.RemovePendingToolRegistration(tool, true);

			if ((bool)tool.GetValue(XamRibbon.IsRegisteredProperty) == true)
				return;

			// AS 11/12/07 BR28406
			// This wasn't directly related to the problem but while verifying the tool instances
			// I noticed that we were registering panels as well as tools but we should not register
			// panels. We only get in here for panels to notify the location.
			//
			if (tool is IRibbonToolPanel == false)
				this.ToolInstanceManager.RegisterToolInstance(tool);

			// menus and ribbon tool panels will be specially handled
			// since the ribbon group needs to know about these to properly handle resizing
			if (tool is IRibbonTool || tool is IRibbonToolPanel)
			{
				if (location == ToolLocation.Ribbon)
				{
					Debug.Assert(locationProvider is RibbonGroup);
					tool.SetValue(RibbonGroup.ContainingGroupPropertyKey, locationProvider as RibbonGroup);
				}
				else if (locationProvider is ToolbarBase)
				{
					Debug.Assert(location == ToolLocation.ApplicationMenuFooterToolbar || location == ToolLocation.QuickAccessToolbar);
					tool.SetValue(ToolbarBase.ContainingToolbarPropertyKey, (ToolbarBase)locationProvider);
				}

				// we're not actually registering the panels as tools - just using the delayed 
				// ribbon change processing
				if (tool is IRibbonToolPanel)
					return;
			}

			// if the tool is on the qat then we need to clear the isonqat flag of all instances
			if (location == ToolLocation.QuickAccessToolbar)
			{
				string id = RibbonToolHelper.GetId(tool);
				Debug.Assert(string.IsNullOrEmpty(id) == false, "A tool being added to the qat doesn't have its id yet?!?");

				this.ToolInstanceManager.SetPropertyValueOnAllRelatedToolInstances(RibbonToolHelper.GetId(tool), RibbonToolHelper.IsOnQatPropertyKey, KnownBoxes.TrueBox);
			}

			Debug.Assert(XamRibbon.GetIsRegistered(tool) == this.ToolInstanceManager.IsRegistered(tool));
		} 
				#endregion //RegisterTool

				// AS 11/16/07 BR28515
				#region RemoveLogicalChildInternal
		/// <summary>
		/// Exposes the RemoveLogicalChild method of the ribbon internally.
		/// </summary>
		/// <param name="child">The child to remove</param>
		internal void RemoveLogicalChildInternal(object child)
		{
			this.RemoveLogicalChild(child);
		}

				#endregion //RemoveLogicalChildInternal

				#region RemoveToolFromQat

		internal bool RemoveToolFromQat(FrameworkElement tool)
		{
			// For a ToolMenuItem use its RibbonTool property
			if (tool is ToolMenuItem)
				tool = ((ToolMenuItem)tool).RibbonTool as FrameworkElement;

			if (tool == null)
				return false;

			// If it's not on the Qat, just return true.
			if ((ToolLocation)tool.GetValue(XamRibbon.LocationProperty) != ToolLocation.QuickAccessToolbar)
				return true;

			// Get the QatPlaceholderTool for the tool being removed.
			this.QuickAccessToolbar.RemoveTool(tool);

			return true;
		}

				#endregion //RemoveToolFromQat	

				// AS 10/22/07 BR27620
				#region RestoreNormalMode
		internal void RestoreNormalMode()
		{
			this.SetMode(RibbonMode.Normal);
		}
				#endregion //RestoreNormalMode

				#region ResumeToolRegistration
		internal void ResumeToolRegistration()
		{
            // AS 2/5/09 TFS11796
            // Call the new overload and continue to process the resume with a 
            // delay as we had previously.
            //
            this.ResumeToolRegistration(true);
        }

        // AS 2/5/09 TFS11796
        // Added overload so we can resume without a delay.
        //
        internal void ResumeToolRegistration(bool delay)
        {
            // AS 2/5/09
            // If this is being called by the implicit resume then clear the dispatcher 
            // operation.
            //
            if (null != _implicitResumeRegOperation)
            {
                if (_implicitResumeRegOperation.Status == DispatcherOperationStatus.Executing ||
                    _implicitResumeRegOperation.Status == DispatcherOperationStatus.Completed)
                    _implicitResumeRegOperation = null;
            }

			int count = Interlocked.Decrement(ref this._toolRegistrationSuspendCount);

			Debug.Assert(count >= 0, "The tool registration has been resumed more often than suspended!");

            if (count == 0)
            {
                // AS 2/5/09 TFS11796
                // Process the resume synchronously if specified.
                //
                //this.ProcessPendingToolRegistrationsWithDelay();
                if (delay)
                    this.ProcessPendingToolRegistrationsWithDelay();
                else
                    this.ProcessPendingToolRegistrations();
            }
		} 
				#endregion //ResumeToolRegistration

				#region SelectAdjacentTab
		// AS 7/20/11 TFS80142
		//private void SelectAdjacentTab(int offset)
		private bool SelectAdjacentTab(int offset)
		{
			TabControl tc = this._ribbonTabControl;

			if (null != tc)
			{
				int startingIndex = tc.SelectedIndex;
				int visibleTabs = 0;
				int tabsToShift = Math.Abs(offset);
				int tabCount = tc.Items.Count;

				// AS 10/25/07 BR27728
				if (startingIndex < 0)
					offset = 1;

				// AS 10/25/07 BR27728
				//if (startingIndex >= 0)
				if (true)
				{
					for (int i = startingIndex + offset; 0 <= i && i < tabCount; i += offset)
					{
						DependencyObject container = tc.ItemContainerGenerator.ContainerFromItem(tc.Items[i]);

						UIElement element = container == null ? tc.Items[i] as UIElement : container as UIElement;

						// AS 10/5/07 BR27122
						// We should skip disabled tabs as well.
						//
						//if (element != null && element.Visibility != Visibility.Collapsed)
						if (element != null && element.Visibility != Visibility.Collapsed && element.IsEnabled)
						{
							visibleTabs++;

							if (visibleTabs == tabsToShift)
							{
								tc.SelectedIndex = i;
								return true; // AS 7/20/11 TFS80142
							}
						}
					}
				}
			}

			return false; // AS 7/20/11 TFS80142
		}
				#endregion //SelectAdjacentTab

				#region SuspendToolRegistration
		internal void SuspendToolRegistration()
		{
			Interlocked.Increment(ref this._toolRegistrationSuspendCount);
		} 
				#endregion //SuspendToolRegistration

				#region TransferFocusOutOfRibbon

		// AS 9/17/09 TFS20559
		internal static void TransferFocusOutOfRibbon(FrameworkElement element)
		{
			XamRibbon ribbon = XamRibbon.GetRibbon(element);

			if (null != ribbon)
				ribbon.TransferFocusOutOfRibbon();
		}

		// AS 10/5/07
		// Changed to internal so elements like the RibbonGroup can use it.
		//
		internal void TransferFocusOutOfRibbon()
		{
			if (!this.IsLoaded)
				return;

			
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)


			// if we have focus then move it out of our control
			if (this.IsKeyboardFocusWithin)
				Keyboard.Focus(null);

			// AS 10/10/07
			// When we are shifting focus out of the ribbon we want to clear the 
			// focused element. otherwise, tools like editor tools will remain in 
			// edit mode.
			//
			if (FocusManager.GetIsFocusScope(this))
				FocusManager.SetFocusedElement(this, null);
		}

				#endregion //TransferFocusOutOfRibbon	

				#region TransferFocusOutOfRibbonHelper
		// AS 10/18/07
		// When a clickable tool has been clicked, we need to make sure that focus is
		// shifted out of the ribbon.
		//
		internal static void TransferFocusOutOfRibbonHelper(FrameworkElement fe)
		{
			if (fe.IsKeyboardFocusWithin)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(fe);

				if (null != ribbon)
				{
					if (ribbon.Mode != RibbonMode.Normal)
						ribbon.SetMode(RibbonMode.Normal);
					else
						ribbon.TransferFocusOutOfRibbon();
				}
			}
		} 
				#endregion //TransferFocusOutOfRibbonHelper

				// AS 3/23/11 TFS69892
				#region TryTransformToVisual
		internal static Rect? TryTransformToVisual(Visual source, Visual relativeVisual, Rect rect)
		{
			try
			{
				GeneralTransform transform = source.TransformToVisual(relativeVisual);
				return transform.TransformBounds(rect);
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		} 
				#endregion //TryTransformToVisual

				#region UnregisterGroup
		internal void UnregisterGroup(RibbonGroup group)
		{
			if (string.IsNullOrEmpty(group.Id) == false && null != group && group.CloneFromGroup == null)
			{
				// AS 10/16/07
				// We may get 2 OnRibbonChanges while the ribbon is being removed so just ignore subsequent unregister calls.
				//
				//Debug.Assert(this._registeredGroups.ContainsKey(group.Id) && this._registeredGroups[group.Id] == group);
				if (this._registeredGroups.ContainsKey(group.Id))
					this._registeredGroups.Remove(group.Id);
			}
		} 
				#endregion //UnregisterGroup

				#region UnregisterTool

        // AS 2/5/09 TFS11796
        private DispatcherOperation _implicitResumeRegOperation;

		internal void UnregisterTool(FrameworkElement tool)
		{
            // AS 10/21/08 TFS9276
            // In this case the theme was being changed and the tools in the qat
            // were temporarily coming out of the logical & visual tree but just
            // in case we should always suspend since a style/template could be 
            // changed without changing the theme.
            //
            if (false == this.IsToolRegistrationSuspended)
            {
                this.SuspendToolRegistration();
                // AS 2/5/09 TFS11796
                // Store the dispatcher operation so we can determine the status in case 
                // we need to force the tool registration early as we do when a ribbon group
                // is being dropped down from the qat.
                //
                //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new MethodInvoker(this.ResumeToolRegistration));
                _implicitResumeRegOperation = this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new MethodInvoker(this.ResumeToolRegistration));
            }

			if (this.IsToolRegistrationSuspended)
			{
			    this.AddToPendingToolRegistrations(tool, false);
			    return;
			}

            UnregisterToolImpl(tool);
        }

        // AS 10/21/08 TFS9276
        private void UnregisterToolImpl(FrameworkElement tool)
        {
            // AS 2/10/09 TFS12207
            // We're now maintaining a backward pointer on the tools to the registered
            // owning Ribbon instance.
            //




            XamRibbon.SetRegisteredOwner(tool, null);

			this.RemovePendingToolRegistration(tool, false);

			// if the tool was in a ribbon group then it can't be any longer
			tool.ClearValue(RibbonGroup.ContainingGroupPropertyKey);
			tool.ClearValue(ToolbarBase.ContainingToolbarPropertyKey);

			if (tool is IRibbonToolPanel)
				return;

			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			// if the tool is on the qat then we need to clear the isonqat flag of all instances
			if (XamRibbon.GetLocation(tool) == ToolLocation.QuickAccessToolbar)
				this.ToolInstanceManager.SetPropertyValueOnAllRelatedToolInstances(RibbonToolHelper.GetId(tool), RibbonToolHelper.IsOnQatPropertyKey, KnownBoxes.FalseBox);
			
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)


			// AS 10/11/07 BR27304
			// Moved this down until after its unregistered since the unregistration needs to know the location.
			//
			// // clear the location since it is no longer known
			//tool.SetValue(XamRibbon.LocationPropertyKey, ToolLocation.Unknown);

			// unregister the tool
			this.ToolInstanceManager.UnRegisterToolInstance(tool);

			// AS 10/11/07 BR27304 
			// Moved down - clear the location since it is no longer known
			tool.SetValue(XamRibbon.LocationPropertyKey, RibbonKnownBoxes.ToolLocationUnknownBox);

			Debug.Assert(XamRibbon.GetIsRegistered(tool) == this.ToolInstanceManager.IsRegistered(tool));
		}	
				#endregion //UnregisterTool

				#region VerifyGroupIsRegistered
		internal void VerifyGroupIsRegistered(RibbonGroup group)
		{
			Debug.Assert(group != null);
			string id = group.Id;

			this.VerifyGroupIsRegistered(group, id);
		} 
				#endregion //VerifyGroupIsRegistered

				// AS 9/22/09 TFS22390
				#region VerifyMenuToolChildrenAsync
		internal void VerifyMenuToolChildrenAsync(MenuToolBase rootMenu)
		{
			if (_menuToolVerificationList == null)
			{
				_menuToolVerificationList = new HashSet();

				this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new MethodInvoker(this.VerifyMenuToolChildren));
			}

			_menuToolVerificationList.Add(rootMenu);
		}

		private void VerifyMenuToolChildren()
		{
			if (_menuToolVerificationList == null)
				return;

			MenuToolBase[] menuTools = _menuToolVerificationList.ToArray<MenuToolBase>();
			_menuToolVerificationList = null;

			for (int i = 0; i < menuTools.Length; i++)
			{
				menuTools[i].VerifyReparentedToolsContainer();
			}
		}
				#endregion //VerifyMenuToolChildrenAsync

				// AS 3/18/11 TFS66436
				// This is a refactored version of what was in UpdateTabsInView
				//
				#region VerifySelectedTab
		internal void VerifySelectedTab()
		{
			// also the selected tab may have been removed in which case we want to select
			// the home/first tab.
			object selectedTab = this.RibbonTabControl.SelectedItem;

			// AS 3/18/11 TFS66436
			// We also want to select the next tab if the currently selected one is collapsed. Note I also 
			// removed some unneeded checks like seeing if the tabcontrol is null or getting the selected 
			// item since we already had it.
			//
			//if (this._ribbonTabControl != null && this.RibbonTabControl.SelectedItem == null && this.IsMinimized == false)
			//    this.SelectNextTab();
			if (this.IsMinimized == false)
			{
				if (selectedTab == null || (selectedTab is TabItem && ((TabItem)selectedTab).Visibility == Visibility.Collapsed))
				{
					// AS 7/20/11 TFS80142
					// We should have selected the previous tab if there was no following tab. Also 
					// if none were selectable then we should clear the selected tab.
					//
					//this.SelectNextTab();
					if (this.SelectAdjacentTab(1) || this.SelectAdjacentTab(-1))
					{
						// AS 12/6/07 BR28967/BR28970
						// Make sure there is only 1 selected tab item. Unselect all others.
						//
						selectedTab = this.RibbonTabControl.SelectedItem;
					}
					else
					{
						// AS 12/6/07 BR28967/BR28970
						selectedTab = null;
					}
				}
			}

			foreach (RibbonTabItem tab in this._tabsInViewInternal)
			{
				if (tab != selectedTab)
				{
					// AS 5/10/12 TFS111333
					//tab.IsSelected = false;
					DependencyPropertyUtilities.SetCurrentValue(tab, TabItem.IsSelectedProperty, KnownBoxes.FalseBox);
				}
			}

			// AS 12/6/07 BR28967/BR28970
			if (null == selectedTab)
				this.SelectedTab = null;
		}
				#endregion //VerifySelectedTab

			#endregion //Internal Methods

			#region Private Methods

				#region AddLogicalChildHelper
		private void AddLogicalChildHelper(object newChild)
		{
			if (null != newChild)
			{
				this._logicalChildren.Add(newChild);
				this.AddLogicalChild(newChild);
			}
		} 
				#endregion //AddLogicalChildHelper

				#region AddToPendingToolRegistrations
		internal void AddToPendingToolRegistrations(FrameworkElement tool, bool isRegistering)
		{
			if (null == this._pendingToolRegistrations)
				this._pendingToolRegistrations = new Dictionary<FrameworkElement, bool>();

			bool currentIsRegistering;

			if (this._pendingToolRegistrations.TryGetValue(tool, out currentIsRegistering))
			{
				// the tool was already being registered/unregistered
				//Debug.Assert(currentIsRegistering != isRegistering);

				if (isRegistering != currentIsRegistering)
				{
					this._pendingToolRegistrations.Remove(tool);

					// AS 7/1/10 TFS32477
					// Even though it was readd it may have been readded elsewhere so we 
					// need to track them and fix them up when we process the pending 
					// registrations.
					//
					if (null == _toolsToVerify)
						_toolsToVerify = new HashSet();

					_toolsToVerify.Add(tool);
				}
			}
			else
			{
				//Debug.Assert(null == _toolsToVerify || !_toolsToVerify.Exists(tool), "Should this tool be removed from the verify list?");

				this._pendingToolRegistrations.Add(tool, isRegistering);
			}
		} 
				#endregion //AddToPendingToolRegistrations

                #region CoerceTabNavigation
        internal static object CoerceTabNavigation(DependencyObject d, object newValue)
        {
            XamRibbon ribbon = (XamRibbon)d;

            switch (ribbon._ribbonMode)
            {
                case RibbonMode.KeyTipsPending:
                case RibbonMode.Normal:
                    return newValue;
                case RibbonMode.ActiveItemNavigation:
                case RibbonMode.KeyTipsActive:
                    // AS 10/14/08 TFS6092
                    // While in activeitemnavigation or keytip mode then
                    // 
                    return KeyboardNavigationMode.Cycle;
                default:
                    Debug.Fail("Unexpected value");
                    return KeyboardNavigationMode.Cycle;
            }
        } 
                #endregion //CoerceTabNavigation

				#region CreateRibbonContextMenu

		// AS 10/11/07
		//private ContextMenu CreateRibbonContextMenu(FrameworkElement targetElement, bool canAddTargetToQat)
		private ContextMenu CreateRibbonContextMenu(FrameworkElement targetElement, bool canAddTargetToQat, object originalSource)
		{
			RibbonContextMenu	menu			= new RibbonContextMenu();
			IInputElement		commandTarget	= this;
			MenuItem			item;
			SeparatorTool		st;


			if (canAddTargetToQat)
			{
				// Add an Add or Remove Tool from Qat menuitem as appropriate
				item					= new ToolMenuItem();
				menu.Items.Add(item);
				item.CommandTarget		= commandTarget;

				// JM BR27369 10-15-07
				//item.CommandParameter	= targetElement;
				if (targetElement is GalleryTool)
				{
					// AS 11/21/07 BR28629
					// The parent of the ribbon will never be a menu tool. :-)
					//
					//DependencyObject containingMenuTool = this.Parent as MenuTool;
					DependencyObject containingMenuTool = targetElement.Parent as MenuTool;

					// AS 11/21/07 BR28629
					// The gallery could be in a nested menu so check for being in a menu item.
					//
					if (containingMenuTool == null)
					{
						ToolMenuItem menuItem = (ToolMenuItem)Utilities.GetAncestorFromType(targetElement, typeof(ToolMenuItem), true);

						if (null != menuItem)
						{
							ToolMenuItem parentItem = menuItem.ParentMenuItem;
							containingMenuTool = parentItem != null ? parentItem.RibbonTool as MenuTool : null;
						}
					}

					if (containingMenuTool == null)
						containingMenuTool = Utilities.GetAncestorFromType(targetElement, typeof(MenuTool), true);

					Debug.Assert(containingMenuTool != null, "Can't find MenuTool that contains GalleryTool!");
					item.CommandParameter = containingMenuTool;
				}
				else
					item.CommandParameter = targetElement;


				// AS 10/11/07
				//if (this.QuickAccessToolbar.ContainsToolInstance(targetElement))
				// AS 11/21/07 BR28629
				// We should be using the command parameter we found above because the target element may
				// not be the element being added.
				//
				//bool useRemoveCommand = this.QuickAccessToolbar.ContainsToolInstance(targetElement);
				bool useRemoveCommand = this.QuickAccessToolbar.ContainsToolInstance(item.CommandParameter as FrameworkElement);

				// if we're trying to remove a ribbon group from the qat and the user is right clicking
				// within the popup show the add to qat as disable instead of showing the remove from qat
				// since that is what office does.
				if (useRemoveCommand && 
					targetElement is RibbonGroup && 
					XamRibbon.GetLocation(targetElement) == ToolLocation.QuickAccessToolbar &&
					originalSource is DependencyObject)
				{
					Popup popup = Utilities.GetAncestorFromType((DependencyObject)originalSource, typeof(Popup), true) as Popup;

					if (popup != null)
					{
						// AS 1/9/08 BR29481
						// The popup could be that of the qat so only change this to false if the 
						// popup is owned by the ribbon group.
						//
						//useRemoveCommand = false;
						if (Utilities.IsDescendantOf(targetElement, popup))
							useRemoveCommand = false;
					}
				}

				if (useRemoveCommand)
				{
					item.Command	= XamRibbon.RemoveItemFromQatCommand;
					item.Header		= GetString("ToolContextMenu_RemoveFromQat");
				}
				else
				{
					item.Command = XamRibbon.AddItemToQatCommand;
					if (targetElement is GalleryTool || (targetElement is MenuTool && ((MenuTool)targetElement).HasGalleryPreview))
						item.Header = GetString("ToolContextMenu_AddGalleryToQat");
					else
						item.Header = GetString("ToolContextMenu_AddToQat");
				}


				// Add a separator.
				st = new SeparatorTool();
				XamRibbon.SetLocation(st, ToolLocation.Menu);
				menu.Items.Add(st);
			}


			// Add the Qat location option.
			item = new ToolMenuItem();
			if (this.QuickAccessToolbarLocation == QuickAccessToolbarLocation.AboveRibbon)
				item.Header		= GetString("ToolContextMenu_ShowQatBelowRibbon");
			else
				item.Header		= GetString("ToolContextMenu_ShowQatAboveRibbon");

			item.Command		= RibbonCommands.ToggleQatLocation;
			item.CommandTarget	= commandTarget;		// JM BR27006 10-3-07
			menu.Items.Add(item);


			// AS 9/23/09 TFS22386
			// Moved into AllowMinimize if block - we shouldn't create the separator if we won't add the menu item.
			//
			//// Add a separator.
			//st = new SeparatorTool();
			//XamRibbon.SetLocation(st, ToolLocation.Menu);
			//menu.Items.Add(st);


			if (this.AllowMinimize == true)
			{
				// AS 9/23/09 TFS22386
				// Moved from above.
				//
				// Add a separator.
				st = new SeparatorTool();
				XamRibbon.SetLocation(st, ToolLocation.Menu);
				menu.Items.Add(st);

				
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

				item = this.CreateMinimizeMenuItem("ToolContextMenu_MinimizeRibbon");
				menu.Items.Add(item);
			}

			return menu;
		}

				#endregion //CreateRibbonContextMenu

				#region CreateRibbonGroupsForSizing
		private static RibbonGroupCollection CreateRibbonGroupsForSizing(out RibbonGroup groupForSizing)
		{
			RibbonGroup group = new RibbonGroup();
			group.Tag = XamRibbon.RibbonGroupForResizingTag;
			groupForSizing = group;

			string dummyText = "Wj0123456789";
			group.SetValue(RibbonToolHelper.CaptionProperty, dummyText);

			// one large tool
			ButtonTool largeTool = new ButtonTool();
			largeTool.Content = dummyText + " " + dummyText;
			largeTool.SetValue(RibbonToolHelper.SizingModePropertyKey, RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox);
            
            
			
            
			ImageSource largeImage = new DrawingImage();
			largeTool.SetValue(RibbonToolHelper.LargeImageProperty, largeImage);
			group.Items.Add(largeTool);

			// one normal tool
			ButtonTool normalTool = new ButtonTool();
			normalTool.Content = dummyText;
			normalTool.SetValue(RibbonToolHelper.SizingModePropertyKey, RibbonKnownBoxes.RibbonToolSizingModeImageAndTextNormalBox);
            
            
			
			
			ImageSource smallImage = new DrawingImage();
			normalTool.SetValue(RibbonToolHelper.SmallImageProperty, smallImage);
			group.Items.Add(normalTool);

			RibbonGroupCollection groups = new RibbonGroupCollection();
			groups.Add(group);
			return groups;
		} 
				#endregion //CreateRibbonGroupsForSizing

                // AS 10/14/08 TFS6076
                // Moved this from the OnWindowIsKeyboardFocusWithinChanged so that we can
                // asynchronously handle the verification.
                //
                #region DelayedTransferFocus
        private void DelayedTransferFocus()
        {
            // AS 10/14/08 TFS6076
            // Since this is being processed asynchronously we may actually enter
            // a ribbon mode or focus may have been shifted out by now. If so then 
            // don't do anything.
            //
            if (this.Mode != RibbonMode.Normal || this.IsKeyboardFocusWithin == false)
                return;

            // AS 10/18/07
            // I found an issue where you change the active application (e.g. by using alt-tab) then 
            // the framework (specifically the hwndkeyboardinputprovider) caches the last FocusedElement
            // and when the form becomes active again will try to refocus that element. If we were in 
            // active mode that means it will try to reactive the tab item, tool, etc. that was last 
            // active. If that's the case then when focus left the window we would have come out of 
            // active mode and cleared our focused element. So if our focused element is still null
            // and they're trying to put focus back within us, shift it out.
            //
            // If there is a focused element (which really must be the case if the window's iskeyboardfocuswithin
            // is becoming true) and that element is within the ribbon...
            // AS 11/15/07 BR28470
            // Cache the focused object as a dependency object since we will need it a couple of times.
            //
            //if (Keyboard.FocusedElement is DependencyObject && this.IsAncestorOf((DependencyObject)Keyboard.FocusedElement))

            DependencyObject focusedObject = Keyboard.FocusedElement as DependencyObject;

            // JM 03-07-08 BR31151 (TFS Bug #2997) - IsAncestorOf requires a Visual - it is possible that we could get a content
            //										 element here (e.g., Hyperlink) so use IsDescendantOf instead.
            //if (null != focusedObject && this.IsAncestorOf(focusedObject))
            if (null != focusedObject && Utilities.IsDescendantOf(this, focusedObject))
            {
                // if we cleared our focus element then we're not expecting this element to be within
                // the ribbon so shift focus elsewhere

                // AS 11/15/07 BR28470
                // Our focused element could be null if the focus was put into a menu in which case
                // the focused element would be in a different focus scope.
                //
                //if (FocusManager.GetFocusedElement(this) == null)
                //	this.TransferFocusOutOfRibbon();
                bool transferFocus = FocusManager.GetFocusedElement(this) == null;

                if (transferFocus)
                {
                    DependencyObject focusScope = FocusManager.GetFocusScope(focusedObject);

                    // do not transfer focus if the focus is to a visible element in a different
                    // focus scope within the ribbon
                    if (focusScope != this && focusScope is UIElement && ((UIElement)focusScope).IsVisible)
                        transferFocus = false;
                }

                if (transferFocus && Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    // AS 10/14/08 TFS6076
                    // If the mouse is explicitly pressed on the focused element then 
                    // we definitely don't want to transfer focus out of the ribbon.
                    //
                    DependencyObject underMouse = Mouse.DirectlyOver as DependencyObject;

                    if (null != underMouse && Utilities.IsDescendantOf(focusedObject, underMouse))
                        transferFocus = false;
                }

                if (transferFocus)
                    this.TransferFocusOutOfRibbon();
            }
        }
                #endregion //DelayedTransferFocus

				#region ExecuteCommandImpl

		private bool ExecuteCommandImpl(RoutedCommand command, object commandParameter)
		{
			// Make sure we have a command to execute.
			if (command == null)
				throw new ArgumentNullException( "command" );


			// Make sure the minimal control state exists to execute the command.
			if (RibbonCommands.IsMinimumStatePresentForCommand(this as ICommandHost, command) == false)
				return false;


			// Fire the 'before executed' cancelable event.
			ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs(command);
			bool proceed = this.RaiseExecutingCommand(beforeArgs);
            if (proceed == false)
            {
                // JJD 06/02/10 - TFS33112
                // Return the inverse of ContinueKeyRouting so that the developer can prevent
                // the original key message from bubbling
                //return false;
                return !beforeArgs.ContinueKeyRouting;
            }


			// Setup some info needed by more than 1 command.
			bool shiftKeyDown	= (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
			bool ctlKeyDown		= (Keyboard.Modifiers & ModifierKeys.Control) != 0;
			bool tabKeyDown		= Keyboard.IsKeyDown(Key.Tab);

			// AS 11/6/07
			// If we were still in active item mode then exit that now. This is so we can mimic the behavior
			// in Office whereby you can be in active item navigation (or keytip mode), show a context menu
			// and when one of these actions is executed the ribbon leaves ribbon mode.
			//
			this.SetMode(RibbonMode.Normal, true);

			// AS 11/15/07 BR28470
			// We need to shift focus out even if we're in normal mode. E.g. when a menu is displayed.
			//
			this.TransferFocusOutOfRibbon();

			// Determine which of our supported commands should be executed and do the associated action.
			bool handled = false;


			if (command == RibbonCommands.ToggleRibbonMinimizedState)
			{
				this.IsMinimized = !this.IsMinimized;
				handled = true;

				goto PostExecute;
			}


			if (command == RibbonCommands.ToggleQatLocation)
			{
				if (this.QuickAccessToolbarLocation == QuickAccessToolbarLocation.BelowRibbon)
					this.QuickAccessToolbarLocation = QuickAccessToolbarLocation.AboveRibbon;
				else
					this.QuickAccessToolbarLocation = QuickAccessToolbarLocation.BelowRibbon;

				handled = true;

				goto PostExecute;
			}


		// =========================================================================================

		PostExecute:
			// If the command was executed, fire the 'after executed' event.
			if ( handled == true )
				this.RaiseExecutedCommand( new ExecutedCommandEventArgs( command ) );

			return handled;
		}

				#endregion //ExecuteCommandImpl
    
				#region GetToolLocation
		private static ToolLocation GetToolLocation(FrameworkElement tool)
		{
			IRibbonToolLocation locationProvider;
			return GetToolLocation(tool, out locationProvider);
		}

		private static ToolLocation GetToolLocation(FrameworkElement tool, out IRibbonToolLocation locationProvider)
		{
			DependencyObject parent = LogicalTreeHelper.GetParent(tool);
			locationProvider = null;

			while (parent != null)
			{
				locationProvider = parent as IRibbonToolLocation;

				if (null != locationProvider)
				{
					// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
					if (locationProvider is IRibbonToolLocationEx)
						return ((IRibbonToolLocationEx)locationProvider).GetLocation(tool);

					return locationProvider.Location;
				}

				parent = LogicalTreeHelper.GetParent(parent);
			}

			locationProvider = (IRibbonToolLocation)Utilities.GetAncestorFromType(tool, typeof(IRibbonToolLocation), true);

			if (null != locationProvider)
				return locationProvider.Location;

			return ToolLocation.Unknown;
		} 
				#endregion //GetToolLocation

				// AS 3/24/10 TFS27869
				#region GetWindowState
		private static WindowState GetWindowState(Visual topLevelVisual)
		{
			Window w = topLevelVisual as Window;

			if (null != w)
				return w.WindowState;

			return WindowState.Normal;
		} 
				#endregion //GetWindowState 

				// AS 7/1/10 TFS34576
				#region OnApplicationMenuSiteLoaded
		private void OnApplicationMenuSiteLoaded(object sender, RoutedEventArgs e)
		{
			ContentControl cc = sender as ContentControl;

			cc.Loaded -= new RoutedEventHandler(OnApplicationMenuSiteLoaded);

			Debug.Assert(cc == _applicationMenuSite);

			if (_applicationMenuSite != null)
				_applicationMenuSite.Content = this.ApplicationMenu;
		}
				#endregion //OnApplicationMenuSiteLoaded

				#region OnCanExecuteInternalCommand

		private static void OnCanExecuteInternalCommand(object target, CanExecuteRoutedEventArgs args)
		{
			XamRibbon xamRibbon = target as XamRibbon;
			if (xamRibbon != null)
			{
				if (args.Command == XamRibbon.AddItemToQatCommand)
				{
					FrameworkElement tool = args.Parameter as FrameworkElement;
					if (tool != null  && RibbonToolHelper.GetIsOnQat(tool) == false)
						args.CanExecute = true;
					else
						args.CanExecute = false;
				}
				else if (args.Command == XamRibbon.RemoveItemFromQatCommand && args.Parameter is FrameworkElement)
				{
					FrameworkElement tool = args.Parameter as FrameworkElement;
					if (tool != null && XamRibbon.GetLocation(tool) == ToolLocation.QuickAccessToolbar)
						args.CanExecute = true;
					else
						args.CanExecute = false;
				}

				args.Handled = true;
			}
		}

				#endregion //OnCanExecuteInternalCommand

				#region OnContextualTabGroupsCollectionChanged

		void OnContextualTabGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// AS 1/4/12 TFS98111
			//this.RefreshTabLogicalChildren();
			//
			//if (this.IsInitialized)
			//    this.UpdateTabsInView();
			this.ProcessTabCollectionChange();
		}

				#endregion //OnContextualTabGroupsCollectionChanged

				#region OnContextualTabGroupsItemPropertyChanged

		private void OnContextualTabGroupsItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
		{
			// AS 1/4/12 TFS98111
			//this.RefreshTabLogicalChildren();
			//
			//if (this.IsInitialized)
			//    this.UpdateTabsInView();
			this.ProcessTabCollectionChange();
		}

				#endregion //OnContextualTabGroupsItemPropertyChanged	

				#region OnGotFocus_ClassHandler

		private static void OnGotFocus_ClassHandler(object sender, RoutedEventArgs e)
		{
			FrameworkElement fe = e.OriginalSource as FrameworkElement;

			Debug.Assert(fe != null);

			if (fe == null)
				return;

			XamRibbon ribbon = XamRibbon.GetRibbon(fe);

			if (ribbon == null)
				return;

			if (ribbon.Mode == RibbonMode.Normal)
				return;

			// AS 12/4/07 BR28887
			// Cache the old active item.
			//
			FrameworkElement oldActiveItem = ribbon.ActiveItem;

			// walk up the parent chain looking for a tool or ribbon group
			while (fe != null)
			{
				if (fe is XamRibbon)
					return;

				IRibbonTool tool = fe as IRibbonTool;

				if (tool != null)
				{
					ribbon.SetActiveItem(fe);
					break;
				}

				RibbonGroup group = fe as RibbonGroup;

				if (group != null)
				{
					// Only activate the group if it is focusable
					if (group.Focusable)
						ribbon.SetActiveItem(group);

					break;
				}

				// AS 10/9/07
				// If a tab gets focus then make it the active item.
				//
				RibbonTabItem tab = fe as RibbonTabItem;

				if (tab != null)
				{
					ribbon.SetActiveItem(tab);
					break;
				}

                // AS 2/25/09 TFS14404
                // The overflow button can be focused/activated in which case we
                // want to change the active item. Otherwise the first tab or the 
                // application menu remains highlighted because it is the active 
                // item.
                //
                if (fe.TemplatedParent is QuickAccessToolbar)
                {
                    if (fe.Name == "PART_OverflowButton")
                    {
                        ribbon.SetActiveItem(fe);
                        break;
                    }
                }

				fe = VisualTreeHelper.GetParent(fe) as FrameworkElement;
			}

			// AS 12/4/07 BR28887
			// If focus changed but the active item didn't then make sure 
			// the active item still contains the focus within its chain.
			//
			if (oldActiveItem == ribbon.ActiveItem && null != oldActiveItem)
			{
				if (null == fe)
					ribbon.SetActiveItem(null);
				// AS 12/5/07 BR28887
				// IsDescendantOf doesn't check to see if the element is the active item.
				//
				//else
				else if (fe != oldActiveItem)
				{
					DependencyObject oldFocusScope = FocusManager.GetFocusScope(oldActiveItem);
					DependencyObject newFocusScope = FocusManager.GetFocusScope(fe);

					if (newFocusScope == oldFocusScope && false == Utilities.IsDescendantOf(oldActiveItem, fe))
						ribbon.SetActiveItem(null);
				}
			}
		}

				#endregion //OnGotFocus_ClassHandler	
    
				#region OnEditModeEnded

		private static void OnEditModeEnded(object sender, EditModeEndedEventArgs e)
		{
			XamRibbon ribbon = sender as XamRibbon;

			Debug.Assert(ribbon != null);

			if (ribbon == null)
				return;

			RibbonMode mode = ribbon.Mode;
			
			// if we aren't in navigation mode then return focus to the last focused element outside our scope
			// AS 10/10/07 BR27234
			// This is actually in addition to a change I made earlier based on a bug I saw while working
			// on navigation. I've rethought this out and basically, we shouldn't be doing this if focus is 
			// being shifted to another element - only if the editor is out of edit mode and retaining the focus.
			//
			//if (mode != RibbonMode.ActiveItemNavigation && ribbon.IsKeyboardFocusWithin)
			//	ribbon.TransferFocusOutOfRibbon();
			//else if (mode == RibbonMode.ActiveItemNavigation)
			// AS 10/18/07
			// Since we're not explicitly setting focus, we should be able to get
			// into this block. If we don't and we were in edit mode and we exited because
			// you clicked elsewhere then the editor remains as the focused element.
			//
			//if (ribbon.IsKeyboardFocusWithin)
			{
				// AS 10/10/07
				// If focus is within the ribbon but editor is still focused then we want to end active item mode. 
				// This came up if an editor within the ribbon got focus while in active item navigation and then
				// pressed either enter or escape.
				//
				if (e.OriginalSource is UIElement && ((UIElement)e.OriginalSource).IsFocused)
				{
					// AS 11/7/07 BR28324
					// We don't want to close a menu though if the user just pressed escape to exit edit mode
					// and the editor is in a menu.
					//
					if (e.ChangesAccepted == false)
					{
						if (null != Utilities.GetAncestorFromType((UIElement)e.OriginalSource, typeof(MenuItem), true))
							return;
					}

					if (ribbon.Mode != RibbonMode.Normal)
						ribbon.SetMode(RibbonMode.Normal);
					else
						ribbon.TransferFocusOutOfRibbon();
				}
			}
		}

				#endregion //OnEditModeEnded	
    
				// JM 11-09-07
				#region OnEditModeStarted

		private static void OnEditModeStarted(object sender, EditModeStartedEventArgs e)
		{
			XamRibbon ribbon = sender as XamRibbon;
			Debug.Assert(ribbon != null);

			if (ribbon == null)
				return;

			RibbonMode mode = ribbon.Mode;

			// If we are in KeyTip mode then revert to normal mode without transferring focus.
			if (mode == RibbonMode.KeyTipsPending || mode == RibbonMode.KeyTipsActive)
				ribbon.SetMode(RibbonMode.Normal, false);
		}

				#endregion //OnEditModeStarted
    
				#region OnExecuteInternalCommand

		private static void OnExecuteInternalCommand(object target, ExecutedRoutedEventArgs args)
		{
			XamRibbon xamRibbon = target as XamRibbon;
			if (xamRibbon != null)
			{
				// AS 11/6/07
				// These commands are used from the context menu for a tool. If we were still in active item
				// mode then exit that now.
				//
				xamRibbon.SetMode(RibbonMode.Normal, true);

				// AS 11/15/07 BR28470
				// We need to shift focus out even if we're in normal mode. E.g. when a menu is displayed.
				xamRibbon.TransferFocusOutOfRibbon();

				if (args.Command == XamRibbon.AddItemToQatCommand && args.Parameter is FrameworkElement)
				{
					xamRibbon.AddToolToQat(args.Parameter as FrameworkElement);
					args.Handled = true;
				}
				else if (args.Command == XamRibbon.RemoveItemFromQatCommand && args.Parameter is FrameworkElement)
				{
					xamRibbon.RemoveToolFromQat(args.Parameter as FrameworkElement);
					args.Handled = true;
				}
			}
		}

				#endregion //OnExecuteInternalCommand

				// AS 1/3/08 BR29371
				#region OnLoaded
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.WireTopLevelVisual();
		} 
				#endregion //OnLoaded

				// AS 10/22/07 BR27620
				#region OnLostMouseCapture
		private static void OnLostMouseCapture(object sender, MouseEventArgs e)
		{
			XamRibbon ribbon = sender as XamRibbon;

			// while in keytip mode we may need to regain capture
			if (ribbon.ShouldCaptureMouseForRibbonMode)
			{
				// if we are not the recipient of the mouse capture
				if (Mouse.Captured != ribbon)
				{
					DependencyObject elementLosingCapture = e.OriginalSource as DependencyObject;

					// if the element losing capture is a descendant of ours
					if (Mouse.Captured == null &&				// nothing else has capture
						elementLosingCapture != null &&			// there is an element losing capture
						elementLosingCapture != ribbon &&		// we're not the one losing capture
						XamRibbon.GetRibbon(elementLosingCapture) == ribbon)	// the element losing capture is within us
					{
						// try to see if a window in another application has capture
						// AS 6/10/08 BR32772
						//IntPtr? windowWithCapture = NativeWindowMethods.GetCaptureApi();
						IntPtr? windowWithCapture = null;

						try
						{
							windowWithCapture = NativeWindowMethods.GetCaptureApi();
						}
						catch (System.Security.SecurityException)
						{
						}

						// if we had to rights to check and no window had capture then retake capture
						if (windowWithCapture == IntPtr.Zero)
						{
							Mouse.Capture(ribbon, CaptureMode.SubTree);
							e.Handled = true;
						}
					}
				}
			}
		} 
				#endregion //OnLostMouseCapture

				// AS 6/22/12 TFS115111
				#region OnModalDialogWindowCountChanged

		private static void OnModalDialogWindowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((XamRibbon)d).CoerceValue(IsEnabledProperty);
		} 

				#endregion //OnModalDialogWindowCountChanged

				// AS 10/22/07 BR27620
				#region OnPreviewMouseDownOutsideCapturedElement
		private static void OnPreviewMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
		{
			XamRibbon ribbon = sender as XamRibbon;

			if (ribbon.ShouldCaptureMouseForRibbonMode && ribbon.IsMouseCaptured)
			{
				ribbon.ReleaseMouseCapture();
				e.Handled = true;
			}
			else
			{
				// AS 11/4/11 TFS88222
				// When the mouse is pressed down on another element within the xamRibbon
				// we want that other element to receive the mouse down. However, when a 
				// popup/dropdown is open, that element has the mouse capture and unless 
				// it releases it during the PreviewMouseDownOutsideCapturedElement, the 
				// mouse down will still be routed to the element with capture. So we'll 
				// centralize handling that here and close the popup as needed.
				//
				var d = e.OriginalSource as DependencyObject;

				// we need to be dealing with a uielement/framework element
				while (d is ContentElement)
					d = LogicalTreeHelper.GetParent(d);

				var ancestor = d;

				while (ancestor != null && ancestor != ribbon)
				{
					// if this is a visual descendant of the ribbon then use the ribbon itself as the hit test point
					if (ribbon.IsAncestorOf(ancestor))
						ancestor = ribbon;
					else
					{
						// otherwise walk up to get the root of the visual tree of the popup
						while (true)
						{
							var temp = VisualTreeHelper.GetParent(ancestor);

							if (temp == null)
								break;

							ancestor = temp;
						}
					}

					UIElement ancestorElement = ancestor as UIElement;
					Debug.Assert(ancestorElement != null, "Ancestor is not a UIElement?");

					if (ancestorElement != null)
					{
						// hit test using that "root" element and see what the mouse is over
						var target = ancestorElement.InputHitTest(e.GetPosition(ancestorElement));

						if (target != null)
						{
							// originally i was going to check if the hit was within the 
							// element with capture but at least in one case the capture 
							// was the combobox within a comboeditortool and the hit result 
							// was the pseudo dropdown button within the combo editor tool
							DependencyObject source = e.OriginalSource as DependencyObject;

							// if the element with capture is a popup then use the templated parent 
							// of the Popup itself.
							if (source != null &&
								Utilities.IsUIElementOrUIElement3D(source) &&
								VisualTreeHelper.GetParent(source) == null &&
								LogicalTreeHelper.GetParent(source) is Popup)
							{
								source = LogicalTreeHelper.GetParent(source);
							}

							do
							{
								DependencyObject tp = Utilities.GetTemplatedParent(source);

								if (null == tp)
									break;

								source = tp;
							} while (true);

							if (Utilities.IsDescendantOf(source, target as DependencyObject))
								return;

							var focused = Keyboard.FocusedElement;
							var captured = Mouse.Captured;

							// if it is over an element let's see if we can shift focus to 
							// it or one of its ancestors. this is definitely needed when 
							// the focus is within a menu since it will call Focus(null)
							// otherwise. alternatively should we try to catch the 
							// focus change while we are setting isopen to false?
							DependencyObject targetObject = target as DependencyObject;

							while (targetObject != null && targetObject != ribbon)
							{
								UIElement targetElement = targetObject as UIElement;

								if (null != targetElement &&
									targetElement.Focusable &&
									targetElement.IsEnabled)
								{
									targetElement.Focus();
									e.Handled = true;
									break;
								}

								// in this case we will walk up the logical tree as well
								targetObject = Utilities.GetParent(targetObject, true);
							}

							// if we didn't shift the focus or the element kept the mouse capture...
							// AS 11/28/11 - TFS88222/TFS96758
							// We need to force the original element to lose mouse capture now even 
							// if we shifted focus.
							//
							//if (focused == Keyboard.FocusedElement || captured == Mouse.Captured)
							{
								// if we don't have a popup reference then force the element
								// with capture to release the capture
								var capture = e.OriginalSource as IInputElement;

								if (null != capture)
								{
									capture.ReleaseMouseCapture();

									// AS 11/28/11 - TFS88222/TFS96758
									// Assuming the element lost mouse capture we should clean up the 
									// old focus scope (unless it is already 
									if (!capture.IsMouseCaptured && capture is DependencyObject)
									{
										DependencyObject captureObject = capture as DependencyObject;
										DependencyObject captureScope = FocusManager.GetFocusScope(captureObject);

										if (captureScope != null && captureScope != FocusManager.GetFocusScope(targetObject))
										{
											FocusManager.SetFocusedElement(captureScope, null);
										}
									}

									e.Handled = true;
								}
							}
							break;
						}
					}

					// if we are processing the ribbon then stop. we want to eat the mouse 
					// down if the mouse is pressed down outside the ribbon
					if (ancestor == ribbon)
						break;

					// continue going up the logical tree
					ancestor = Utilities.GetParent(ancestor, true);
				}
			}
		} 
				#endregion //OnPreviewMouseDownOutsideCapturedElement

				// AS 7/1/10 TFS34457
				#region OnQatTargetLoaded
		private void OnQatTargetLoaded(object sender, RoutedEventArgs e)
		{
			this.VerifyQatLocation();
		}
				#endregion //OnQatTargetLoaded

				#region OnRibbonContextMenuOpening

        // AS 10/16/08 TFS6447
        // We need to cache a reference to the ContextMenuEventArgs so we know that we 
        // specifically skipped the args while we were walking up the ancestor chain.
        // Otherwise if the context menu is set on a tool, we could end up showing 
        // our context menu for the ancestor ribbon group.
        //
        [ThreadStatic]
        private static WeakReference _lastContextMenuArgs = null;

		private static void OnRibbonToolContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
            // AS 10/16/08 TFS15396/TFS6447
            // Do the check to see if we've processed the event args first.
            //
            ContextMenuEventArgs lastArgs = null;

            if (_lastContextMenuArgs != null)
            {
                lastArgs = Utilities.GetWeakReferenceTargetSafe(_lastContextMenuArgs) as ContextMenuEventArgs;

                // if they're the same then we can consider this event "handled"
                if (lastArgs == e)
                    return;

                // if they're different then null out the event args so we don't
                // do this comparison again
                lastArgs = null;
                _lastContextMenuArgs = null;
            }

			XamRibbon		ribbon = null;
			IRibbonTool		tool;
			IRibbonToolHost ribbonToolHost = sender as IRibbonToolHost;
			bool			canAddToQat = false;

			// see if the element represents a ribbon tool
			if (ribbonToolHost != null)
				tool = ribbonToolHost.Tool;
			else
				tool = sender as IRibbonTool;

			if (tool != null)
			{
				RibbonToolProxy rtp = tool.ToolProxy;
				if (rtp != null)
					canAddToQat = rtp.CanAddToQat;
			}
			else if (sender is RibbonGroup)
				canAddToQat = true;

			FrameworkElement	targetItem;
            // AS 10/16/08 TFS15396/TFS6447
            // We should check any dependency object.
            //
			//UIElement			element = sender as UIElement;
            DependencyObject    element = sender as DependencyObject;

            // AS 10/16/08 TFS6447
            // This is just a safety measure. We weren't checking for null.
            //
            if (null == element)
                return;
            
			ribbon = XamRibbon.GetRibbon(element);

			// set the target item 
			if (sender is RibbonGroup	||
				sender is RibbonTabItem	||
				sender is XamTabControl	||
				(sender is QuickAccessToolbar && ribbon != null && ribbon.QuickAccessToolbarLocation == QuickAccessToolbarLocation.BelowRibbon)) 
				targetItem = sender as FrameworkElement;
			else if (sender is ToolMenuItem)	// JM BR27366 10-17-07
				targetItem = ((ToolMenuItem)sender).RibbonTool as FrameworkElement;
			else
				targetItem = tool as FrameworkElement;

            if (targetItem != null)
			{
                // AS 3/13/09 TFS15396/TFS6447
                // If we get here we always want to consider the event handled.
                //
                _lastContextMenuArgs = new WeakReference(e);

                // AS 2/19/09 TFS6747
                //Debug.Assert(ribbon != null);
				Debug.Assert(ribbon != null || Utilities.GetAncestorFromType(targetItem, typeof(XamRibbon), true) == null);
				if (ribbon == null)
					return;

				// don't supply a context menu for the QuickCustomizeMenu of the
				// QuickAccessToolbar
				if (targetItem == ribbon.QuickAccessToolbar.QuickCustomizeMenu)
					return;

                // AS 3/13/09 TFS15396
                // Don't assume its a frameworkelement.
                //
				//ContextMenu menu = ((FrameworkElement)sender).ContextMenu;
				ContextMenu menu = ContextMenuService.GetContextMenu(element);

				// AS 3/23/11 TFS63749/TFS69993
				// When the source element such as a menuitem is bound to the 
				// context menu property of its tool then we want to get the 
				// source tool's menu property and base our decision on that.
				// We have a similar situaton if the tool clicked upon is a 
				// clone of another tool.
				//
				//if (element.ReadLocalValue(ContextMenuProperty) != null)
				object menuLocalValue = element.ReadLocalValue(ContextMenuProperty);

				if (menuLocalValue is BindingExpressionBase)
				{
					FrameworkElement otherSource = null;

					if (element is ToolMenuItem)
						otherSource = targetItem;
					else
						otherSource = XamRibbon.GetClonedFromTool(targetItem);

					if (null != otherSource)
						menuLocalValue = otherSource.ReadLocalValue(ContextMenuProperty);
				}

				if (menuLocalValue != null)
				{
                    // AS 3/13/09 TFS15396
                    // Moved the logic to bail out if the event args were processed to above.
                    //
                    //// AS 10/16/08 TFS6447
                    //// Get the last menu args we received and do not show the menu if we
                    //// already processed these args for a descendant element.
                    ////
                    ////if (menu == null)
                    //ContextMenuEventArgs lastArgs = null != _lastContextMenuArgs ? Utilities.GetWeakReferenceTargetSafe(_lastContextMenuArgs) as ContextMenuEventArgs : null;
                    //
                    //if (menu == null && lastArgs != e)
                    if (menu == null)
					{
						// AS 10/11/07
						//menu					= ribbon.CreateRibbonContextMenu(targetItem, canAddToQat);
						menu					= ribbon.CreateRibbonContextMenu(targetItem, canAddToQat, e.OriginalSource);

                        #region Commented Out
						
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

                        #endregion //Commented Out
						DependencyObject relativeElement = e.OriginalSource as DependencyObject;

						while (relativeElement is ContentElement)
							relativeElement = LogicalTreeHelper.GetParent(relativeElement);

						Debug.Assert(relativeElement is UIElement);

						menu.PlacementTarget	= relativeElement as UIElement;
						menu.Placement			= PlacementMode.RelativePoint;
						menu.HorizontalOffset	= e.CursorLeft;
						menu.VerticalOffset		= e.CursorTop;

						menu.IsOpen				= true;
						e.Handled				= true;
					}

                    // AS 3/13/09 TFS15396
                    // Moved this up since we also want to do this for non-tool elements
                    // that have a context menu.
                    //
                    //// AS 10/16/08 TFS6447
                    //if (lastArgs != e)
                    //    _lastContextMenuArgs = new WeakReference(e);
				}
			}
            else
            {
                // AS 3/13/09 TFS15396
                // If there is a context menu or it was explicitly set to null then let its menu show.
                //
                ContextMenu cm = ContextMenuService.GetContextMenu(element);

				if (null != cm || element.ReadLocalValue(ContextMenuService.ContextMenuProperty) == null)
                    _lastContextMenuArgs = new WeakReference(e);
            }
		}

				#endregion //OnRibbonContextMenuOpening

				#region OnTabControlSelectionChanged

		void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// AS 12/3/09 TFS25257
			// SelectionChanged is a bubble event so this could be raised for a descendant selector.
			//
			if (e.OriginalSource != _ribbonTabControl)
				return;

			RibbonTabItem rtiPrevious			= e.RemovedItems.Count > 0 ? e.RemovedItems[0] as RibbonTabItem : null;
			RibbonTabItem rtiNew				= e.AddedItems.Count > 0 ? e.AddedItems[0] as RibbonTabItem : null;

			RibbonTabItemSelectedEventArgs args = new RibbonTabItemSelectedEventArgs(rtiPrevious, rtiNew);

			this.RaiseRibbonTabItemSelected(args);
		}

				#endregion //OnTabControlSelectionChanged	
    
				#region OnTabControlDropDownOpening

		void OnTabControlDropDownOpening(object sender, TabControlDropDownOpeningEventArgs e)
		{
			RibbonTabItemOpeningEventArgs args = new RibbonTabItemOpeningEventArgs();

			this.RaiseRibbonTabItemOpening(args);
		}

				#endregion //OnTabControlDropDownOpening	
    
				#region OnTabControlDropDownOpened

		void OnTabControlDropDownOpened(object sender, RoutedEventArgs e)
		{
			this.RaiseRibbonTabItemOpened(new RoutedEventArgs());
		}

				#endregion //OnTabControlDropDownOpened	
    
				#region OnTabControlDropDownClosed

		void OnTabControlDropDownClosed(object sender, RoutedEventArgs e)
		{
			this.RaiseRibbonTabItemCloseUp(new RoutedEventArgs());
		}

				#endregion //OnTabControlDropDownClosed	
        
				#region OnTabsCollectionChanged
		private void OnTabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)

			this.ProcessTabCollectionChange();
		}

				#endregion //OnTabsCollectionChanged

				// AS 10/15/10 TFS36676/TFS34576
				#region OnTabControlSiteLoaded
		private void OnTabControlSiteLoaded(object sender, RoutedEventArgs e)
		{
			ContentControl cc = sender as ContentControl;

			cc.Loaded -= new RoutedEventHandler(OnTabControlSiteLoaded);

			Debug.Assert(cc == _ribbonTabControlSite);

			if (_ribbonTabControlSite != null)
				_ribbonTabControlSite.Content = this.RibbonTabControl;
		}
				#endregion //OnTabControlSiteLoaded


				#region OnToolsNotInRibbonCollectionChanged

		private void OnToolsNotInRibbonCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.IsInitialized == false)
				return;


			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					{
						foreach (object item in e.NewItems)
							this.ToolInstanceManager.RegisterToolInstance((FrameworkElement)item);
						break;
					}
				case NotifyCollectionChangedAction.Remove:
					{
						foreach (object item in e.OldItems)
						{
							this.QuickAccessToolbar.RemoveToolId(RibbonToolHelper.GetId((DependencyObject)item));

							this.ToolInstanceManager.UnRegisterToolInstance((FrameworkElement)item);
						}
						break;
					}
				case NotifyCollectionChangedAction.Move:
					// nothing to do
					break;
				case NotifyCollectionChangedAction.Replace:
					{
						// remove the replaced items and add the ones that replaced it
						foreach (object item in e.OldItems)
						{
							this.QuickAccessToolbar.RemoveToolId(RibbonToolHelper.GetId((DependencyObject)item));

							this.ToolInstanceManager.UnRegisterToolInstance((FrameworkElement)item);
						}

						foreach (object item in e.NewItems)
							this.ToolInstanceManager.RegisterToolInstance((FrameworkElement)item);
						break;
					}
				case NotifyCollectionChangedAction.Reset:
					{
						// When the collection is cleared our OnToolsNotInRibbonCollectionClearing method id called - that is
						// where we unregister the cleared tools.
						break;
					}
			}
		}

				#endregion //OnToolsNotInRibbonCollectionChanged	
    
				#region OnToolTipClosed

		private static void OnToolTipClosed(object sender, RoutedEventArgs e)
		{
			XamRibbonScreenTip toolTip = sender as XamRibbonScreenTip;
			if (toolTip != null)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(sender as DependencyObject);

				Debug.Assert(ribbon != null);

				if (ribbon == null)
					return;

				toolTip.Closed -= new RoutedEventHandler(OnToolTipClosed);
				toolTip.PlacementRectangle	= ribbon._originalToolTipPlacementRectangle;
				toolTip.Placement			= ribbon._originalToolTipPlacement;
			}
		}

				#endregion //OnToolTipClosed	
    
				#region OnToolTipOpening

		// This a class handler for the ToolTip opening event.  We are looking at all tooltip openings here so we can properly position XamRibbonScreenTips
		// when they dropdown for tools that are on sited a RibbonGroup which is not collapsed and not on the QAT.
		private static void OnToolTipOpening(object sender, ToolTipEventArgs e)
		{
			// Only do this for tooltips that are dropping down for elements that implement IRibbonTool.
			FrameworkElement targetElement = e.OriginalSource as FrameworkElement;
			// AS 1/2/08 BR29307
			// It doesn't have to be a tool. It could be a gallery item or something else
			// within a ribbon group.
			//
			//if (targetElement == null || (targetElement is IRibbonTool) == false)
			if (targetElement == null)
				return;


			// Get the actual XamRibbonScreenTip from the target element.
            // AS 3/17/09 TFS6239/BT34683
            // We want to reposition tool tips for the ribbon group.
            //
			//XamRibbonScreenTip targetElementToolTip = targetElement.ToolTip as XamRibbonScreenTip;
            ToolTip targetElementToolTip = targetElement.ToolTip as ToolTip;

			if (targetElementToolTip == null)
				return;

            // AS 3/17/09 TFS6239/BT34683
            if (targetElementToolTip is XamRibbonScreenTip == false)
            {
                // If this is not the ribbon screen tip and not for a ribbon group then do not 
                // manipulate its position. I just want to address the issue whereby the clipped 
                // caption text tooltip is not being repositioned.
                //
                if (targetElement.TemplatedParent is RibbonGroup == false)
                    return;
            }

			XamRibbon ribbon = XamRibbon.GetRibbon(sender as DependencyObject);

			Debug.Assert(ribbon != null);

			if (ribbon == null)
				return;


			// Make sure the target element is contained within this XamRibbon instance.
			XamRibbon targetElementRibbon = XamRibbon.GetRibbon(targetElement);
			if (targetElementRibbon != ribbon)
				return;


			// Make the target element is not on the QAT.
			if (XamRibbon.GetLocation(targetElement) == ToolLocation.QuickAccessToolbar)
				return;


			// Get the RibbonGroup that the target element is sited on and make sure it is not collapsed and not on the QAT.
			RibbonGroup targetElementRibbonGroup = Utilities.GetAncestorFromType(targetElement, typeof(RibbonGroup),  true) as RibbonGroup; 
			if (targetElementRibbonGroup == null)
				return;

			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			DependencyObject parent = targetElement;
			DependencyObject previousParent = null;

			while (parent != null && previousParent != ribbon)
			{
				previousParent = parent;
				parent = VisualTreeHelper.GetParent(parent);
			}

			FrameworkElement rootVisual = previousParent as FrameworkElement ?? ribbon;

			// AS 1/2/08 BR29307
			// If this isn't a tool then get its location and make sure its in the ribbon group
			//
			if (XamRibbon.GetLocation(targetElement) == ToolLocation.Unknown)
			{
				// we only want to offset the screentip if its directly within a ribbon group. so if the 
				// root visual is a ribbon or the element is within the ribbon group then we're ok to 
				// adjust it
				if (rootVisual != ribbon &&	targetElementRibbonGroup.IsAncestorOf(targetElement) == false)
				{
					DependencyObject logicalParent = LogicalTreeHelper.GetParent(rootVisual);

					// otherwise, make sure its within a collapsed ribbon group. if not then 
					// don't manipulate its position
					if (null != logicalParent && 
						(targetElementRibbonGroup.IsAncestorOf(logicalParent) == false ||
						targetElement.IsDescendantOf(rootVisual) == false))
					{
						return;
					}
				}
			}

			// AS 2/3/10 TFS26841
			// I can't reproduce the reported issue but in case the target element has since been 
			// removed from the visual tree, we'll wrap this in a try catch. Also since this could 
			// fail I have moved this from below above the point that we hook into the Closed event.
			// If it does fail we'll stop the tooltip from showing. It looks like the root issue is 
			// that the wpf framework starts a timer based on the intialshowdelay and then when that 
			// ticks in they just raise the event ignoring the fact that the item may not be in the 
			// visual tree any more.
			//
			Point targetElementTopLeftOnScreen;
			Point ribbonTopLeftOnScreen;

			try
			{
				targetElementTopLeftOnScreen = Utilities.PointToScreenSafe(targetElement, new Point(0, 0));
				ribbonTopLeftOnScreen = Utilities.PointToScreenSafe(rootVisual, new Point(0, 0));
			}
			catch (InvalidOperationException)
			{
			    e.Handled = true;
			    return;
			}

			// At this point we have determined that the tooltip should be repositioned.  Hook the tooltip's closed event so we can reset its
			// PlacementRectangle and Placement properties which we will set below.  Save the original values of these 2 properties so we can restore
			// them in the Closed event.
			targetElementToolTip.Closed += new RoutedEventHandler(OnToolTipClosed);
			ribbon._originalToolTipPlacementRectangle	= targetElementToolTip.PlacementRectangle;
			ribbon._originalToolTipPlacement			= targetElementToolTip.Placement;

			// Get the screen locations of the target element and the ribbon and compute the PlacementRectangle.
			Rect	targetElementLayoutRect			= LayoutInformation.GetLayoutSlot(targetElement);
            
			// AS 2/3/10 TFS26841
			// Moved up and wrapped in try/catch in case the elements are not in the visual tree.
			//
			//// JJD 11/06/07 - Call PointToScreenSafe so we don't get an exception throw
			//// in XBAP semi-trust applications
			////Point	targetElementTopLeftOnScreen	= targetElement.PointToScreen(new Point(0, 0));
			////Point   ribbonTopLeftOnScreen			= ribbon.PointToScreen(new Point(0, 0));
			//Point	targetElementTopLeftOnScreen	= Utilities.PointToScreenSafe( targetElement, new Point(0, 0));
			//Point   ribbonTopLeftOnScreen			= Utilities.PointToScreenSafe( rootVisual, new Point(0, 0));
			
            Rect	targetElementScreenRect			= new Rect(targetElementTopLeftOnScreen, targetElementLayoutRect.Size);

            // JJD 11/06/07 - Call the Utilities.Point...Safe methods so we don't get an exception throw
            // in XBAP semi-trust applications
            //Point	placementRectTopLeftInTargetElementCoordinates 
            //                                        = targetElement.PointFromScreen(new Point(targetElementTopLeftOnScreen.X, targetElementTopLeftOnScreen.Y - (targetElementTopLeftOnScreen.Y - ribbonTopLeftOnScreen.Y)));
			Point	placementRectTopLeftInTargetElementCoordinates 
													= Utilities.PointFromScreenSafe( targetElement, new Point(targetElementTopLeftOnScreen.X, targetElementTopLeftOnScreen.Y - (targetElementTopLeftOnScreen.Y - ribbonTopLeftOnScreen.Y)));
			Rect	placementRectangle				= new Rect(placementRectTopLeftInTargetElementCoordinates.X,
															   placementRectTopLeftInTargetElementCoordinates.Y,
															   targetElementLayoutRect.Width,
															   rootVisual.ActualHeight);


			// Set the Placement and PlacementRectangle properties to position the tooltip in the proper location.
			targetElementToolTip.Placement			= PlacementMode.Bottom;
			targetElementToolTip.PlacementRectangle = placementRectangle;
		}

				#endregion //OnToolTipOpening

				// AS 1/3/08 BR29371
				#region OnUnloaded
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.WireTopLevelVisual();
		} 
				#endregion //OnUnloaded

				#region OnWindowIsKeyboardFocusWithinChanged

		private void OnWindowIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			// AS 10/18/07
			// We only need to change the mode back to normal if the window is losing focus.
			//
			if (false.Equals(e.NewValue))
			{
				// AS 11/6/07
				if (Keyboard.FocusedElement is ContextMenu)
					return;

				this.SetMode(RibbonMode.Normal);
			}
			else
			{
                
#region Infragistics Source Cleanup (Region)
















































#endregion // Infragistics Source Cleanup (Region)

                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                    new MethodInvoker(this.DelayedTransferFocus));
			}
		}

				#endregion //OnWindowIsKeyboardFocusWithinChanged	
    
				#region OnWindowPreviewKeyDown

		private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Handled) 
				return;

			// AS 11/5/07
			// Do not try to process the keyboard while the ribbon is disabled - which includes when it is collapsed.
			//
			if (this.IsEnabled == false)
				return;

			// AS 10/16/09 TFS23895
			// We should not intercept the window's keys if we are not visible.
			//
			if (this.IsVisible == false)
				return;

			#region F10 Handling

			// F10 Handling - if the F10 key is pressed and released
			// within a certain timeframe or another key is pressed while f10 
			// is down then do not exit keytip active mode
			if (e.Key != Key.System || e.SystemKey != Key.F10)
				this._f10KeyDownTime = DateTime.MaxValue;
			else if (e.IsRepeat == false)
			{
				this._f10KeyDownTime = DateTime.Now;
				this._resetRibbonModeOnF10Up = this._ribbonMode != RibbonMode.Normal;
			}

			#endregion //F10 Handling

			switch (e.Key)
			{
				case Key.System:
					{
						#region SystemKey

						switch (e.SystemKey)
						{
							case Key.LeftAlt:
							case Key.RightAlt:
								if (this._ribbonMode == RibbonMode.Normal &&
									this.IsKeyboardFocusWithin == false)
								{
									this.SetMode(RibbonMode.KeyTipsPending);
								}
								break;
							case Key.F10:
								if (this._ribbonMode == RibbonMode.KeyTipsPending ||
									this._ribbonMode == RibbonMode.Normal)
								{
									// AS 12/6/07 BR28987
									if (0 != (Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt)))
										break;

									this.SetMode(RibbonMode.KeyTipsActive);

									// mark this as handled so the window menu doesn't respond
									e.Handled = true;
								}
								break;
						}
						break;

						#endregion //SystemKey
					}

				case Key.Up:
				case Key.Down:
				case Key.Right:
				case Key.Left:
					this.ProcessNavigationKey(e);
					break;

				case Key.Tab:

					// only process the tab if the alt key is not pressed
					if ( (e.KeyboardDevice.Modifiers & ModifierKeys.Alt ) == 0 )
						this.ProcessNavigationKey(e);
					//else
						//if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
							
					break;

				case Key.Escape:
					#region Escape
					// AS 10/3/07
					// Moved this from the OnKeyDown to the Preview since we need to intercept the escape
					// key before the focused element. In this case, the app menu was open and it was processing
					// the escape key closing itself up but to the keytip manager it was still the active
					// container. I talked it over with Joe and we decided to move it here.
					//
					switch (this._ribbonMode)
					{
						case RibbonMode.KeyTipsActive:
							{
								this.KeyTipManager.ProcessEscKey();

								if (this.KeyTipManager.IsActive == false)
									this.SetMode(RibbonMode.Normal);

								e.Handled = true;
								break;
							}
					}

					break;

					#endregion //Escape

				case Key.F1:
					#region F1
					// AS 10/15/07
					// Toggle the minimized state when using Ctrl-F1
					//
					if (this.AllowMinimize && (e.KeyboardDevice.Modifiers & AllModifiers) == ModifierKeys.Control)
					{
						// if focus is within a popup within the ribbon then do not process the ctrl-f1
						if (this.IsKeyboardFocusWithin)
						{
							if (Keyboard.FocusedElement is DependencyObject && Utilities.GetAncestorFromType(Keyboard.FocusedElement as DependencyObject, typeof(XamRibbon), true, null, typeof(Popup)) != this)
								break;

							this.SetMode(RibbonMode.Normal);

							if (this.IsKeyboardFocusWithin)
								this.TransferFocusOutOfRibbon();
						}

						this.IsMinimized = !this.IsMinimized;
						e.Handled = true;
					}
					break;
					#endregion //F1
			}
		}

				#endregion //OnWindowPreviewKeyDown	
    
				#region OnWindowPreviewKeyUp

		private void OnWindowPreviewKeyUp(object sender, KeyEventArgs e)
		{
			// AS 11/5/07
			// Do not try to process the keyboard while the ribbon is disabled - which includes when it is collapsed.
			//
			if (this.IsEnabled == false)
				return;

			// AS 10/16/09 TFS23895
			// We should not intercept the window's keys if we are not visible.
			//
			if (this.IsVisible == false)
			{
				Debug.Assert(this.Mode == RibbonMode.Normal);
				this.SetMode(RibbonMode.Normal, true);
				return;
			}

			switch (e.Key)
			{
				case Key.System:
					{
						#region SystemKey

						switch (e.SystemKey)
						{
							case Key.LeftAlt:
							case Key.RightAlt:
								#region Alt

								if (this._ribbonMode == RibbonMode.KeyTipsPending)
								{
									this.SetMode(RibbonMode.KeyTipsActive);
									e.Handled = true;
								}
								else if (this._ribbonMode == RibbonMode.KeyTipsActive ||
									this._ribbonMode == RibbonMode.ActiveItemNavigation)
								{
									this.SetMode(RibbonMode.Normal);
									e.Handled = true;
								}
								break;

								#endregion //Alt
							case Key.F10:
								#region F10
								if (this._ribbonMode != RibbonMode.Normal)
								{
									bool cancelRibbonMode = this._resetRibbonModeOnF10Up;

									if (false == cancelRibbonMode)
									{
										TimeSpan ts = DateTime.Now.Subtract(this._f10KeyDownTime);

										// if the f10 key was held down for a long time and no
										// other key was pressed, exit navigation mode
										if (ts.TotalSeconds > 1.5d)
											cancelRibbonMode = true;
									}

									if (cancelRibbonMode)
									{
										this.SetMode(RibbonMode.Normal);
										e.Handled = true;
									}
								}
								break;

								#endregion //F10
						}
						break;

						#endregion //SystemKey					
					}
			}

			// AS 6/8/09 TFS18035
			// If the text was not something we could process then 
			// we should exit pending keytip mode.
			//
			if (!e.Handled && this._ribbonMode == RibbonMode.KeyTipsPending)
				this.SetMode(RibbonMode.Normal);
		}

				#endregion //OnWindowPreviewKeyUp	

				#region OnWindowPreviewMouseDown

		private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			// AS 10/11/07
			// We don't always want to shift focus out of the ribbon or exit the current mode
			// For example, if you mouse down on something in a menu, shifting focus would cause
			// the popup to close and the click to not happen.
			//
			//this.SetMode(RibbonMode.Normal);

			// AS 11/14/07 BR28427
			// This isn't directly related to the bug but is an optimization. We're only processing
			// here to know when to come out of keytip/active mode.
			//
			if (this.Mode == RibbonMode.Normal)
				return;

			DependencyObject d = e.OriginalSource as DependencyObject;

			bool isMouseDownOnRibbon = d != null && d.GetValue(XamRibbon.RibbonProperty) == this;
			bool isMouseDownOnPopup = d != null && Utilities.GetAncestorFromType(d, typeof(Popup), true) != null;

			// AS 11/14/07 BR28427
			bool isMouseDownOnMenuItem = false;

			while (d is Visual || d is System.Windows.Media.Media3D.Visual3D)
			{
				if (d is MenuItem)
				{
					isMouseDownOnMenuItem = true;
					break;
				}

				d = VisualTreeHelper.GetParent(d);
			}

			// If we are mousing down within the ribbon and keytips are active, hide the keytips.
			// AS 11/13/07
			// While fixing BR28417 I noticed that we were not exiting active item navigation when
			// you moused down on another tab.
			//
			//if (isMouseDownOnRibbon && this.Mode == RibbonMode.KeyTipsActive)
			if (isMouseDownOnRibbon && this.Mode != RibbonMode.Normal)
			{
				// If the mouse is not within a popup then also shift focus out of the ribbon.
				// AS 11/14/07 BR28427
				// Do not shift focus if the mouse is down on a menu item either because when the mouse down
				// is processed it will cause the menu item to close but we could cause it to close by 
				// shifting focus which would upset the order of things.
				//
				//this.SetMode(RibbonMode.Normal, false == isMouseDownOnPopup);
				this.SetMode(RibbonMode.Normal, false == isMouseDownOnPopup && false == isMouseDownOnMenuItem);
			}
			else if (isMouseDownOnRibbon == false)
			{
				// when mousing down outside the ribbon always restore focus
				this.SetMode(RibbonMode.Normal);
			}
		}

				#endregion //OnWindowPreviewMouseDown	
        
				// AS 10/12/07 BR27159
				#region OnWindowPreviewMouseWheel
		private void OnWindowPreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			this.ProcessMouseWheel(e);
		}
				#endregion //OnWindowPreviewMouseWheel

				#region OnWindowPreviewTextInput

		private void OnWindowPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (this._ribbonMode == RibbonMode.KeyTipsPending)
			{
				// i don't think this should happen except when the alt
				// key is down but just in case we'll is the text
				// AS 7/16/09 TFS19201
				// Text/SystemText may not return null so also use the other if its an empty length.
				//
				//string text = e.SystemText ?? e.Text;
				//
				//if (text != null && text.Length > 0)
				string text = string.IsNullOrEmpty(e.SystemText) ? e.Text : e.SystemText;

				
#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)

				if (this.ProcessPendingKeyTip(text))
					e.Handled = true;
			}
			else if (this._ribbonMode == RibbonMode.KeyTipsActive)
			{
				// need to be able to process it if the alt key is down or not
				// AS 7/16/09 TFS19201
				// Text/SystemText may not return null so also use the other if its an empty length.
				//
				//string text = e.Text ?? e.SystemText;
				//
				//if (text != null)
				string text = string.IsNullOrEmpty(e.SystemText) ? e.Text : e.SystemText;

				if (!string.IsNullOrEmpty(text))
				{
					foreach (char c in text)
					{
						if (this.KeyTipManager.ProcessKey(c, true) == false)
							break;
					}
				}

				// revert to normal if we hit a leaf key tip
				// AS 10/3/07
				// Do not reset the mode if it was changed to active item.
				//
				//if (this.KeyTipManager.IsActive == false)
				if (this._ribbonMode == RibbonMode.KeyTipsActive && this.KeyTipManager.IsActive == false)
					this.SetMode(RibbonMode.Normal);

				e.Handled = true;
			}
		}

				#endregion //OnWindowPreviewTextInput	
 
				// AS 10/12/07 BR27159
				#region ProcessMouseWheel
		private void ProcessMouseWheel(MouseWheelEventArgs e)
		{
			if (this.IsMouseOver &&
				this.IsEnabled &&	// AS 11/5/07
				this.IsVisible && // AS 10/16/09 TFS23895
				this._ribbonMode == RibbonMode.Normal &&
				this.IsKeyboardFocusWithin == false &&
				this._ribbonTabControl != null &&
				this._ribbonTabControl.IsMinimized == false)
			{
				
				
				
				
				
				
				
				int offset = e.Delta < 0 ? 1 : -1;
				this.SelectAdjacentTab(offset);
				e.Handled = true;
			}
		}
				#endregion //ProcessMouseWheel 

				#region ProcessNavigationKey

		private void ProcessNavigationKey(KeyEventArgs e)
		{
			// AS 10/9/07
			// Just use the navigation keys as a mechanism for starting navigation mode
			// but let the focused control deal with the navigation using the framework's
			// mechanisms.
			//
			if (this._ribbonMode == RibbonMode.KeyTipsActive)
			{
				// AS 11/1/07 BR27964
				// Using a navigation key while in keytip mode is ok if focus is within a menu.
				//
				DependencyObject focusedObject = Keyboard.FocusedElement as DependencyObject;

				if (focusedObject is MenuItem ||  null != Utilities.GetAncestorFromType(focusedObject, typeof(MenuItem), true))
					return;

				this.SetMode(RibbonMode.ActiveItemNavigation);
			}
		}

				#endregion //ProcessNavigationKey	

				// AS 12/8/09 TFS25326
				// Moved the handling of the key press while in pending key tip mode here from 
				// the OnWindowPreviewTextInput so we can use it from the OnAccessKey as well.
				//
				#region ProcessPendingKeyTip
		private bool ProcessPendingKeyTip(string text)
		{
			bool result = false;

			if (!string.IsNullOrEmpty(text))
			{
				char[] chars = text.ToCharArray();

				// if we could process the key...
				if (this.KeyTipManager.EvaluateKeyDuringPendingShow(chars[0]))
				{
					this.SetMode(RibbonMode.KeyTipsActive);

					for (int i = 0; i < chars.Length; i++)
					{
						this.KeyTipManager.ProcessKey(chars[i], false);

						// if it should happen to be a leaf item
						if (this.KeyTipManager.IsActive == false)
						{
							this.SetMode(RibbonMode.Normal);
							break;
						}
					}

					result = true;
				}

				// AS 6/8/09 TFS18035
				// If the text was not something we could process then 
				// we should exit pending keytip mode.
				//
				if (_ribbonMode == RibbonMode.KeyTipsPending)
					this.SetMode(RibbonMode.Normal);
			}

			return result;
		} 
				#endregion //ProcessPendingKeyTip

				#region ProcessPendingToolRegistrations

		internal delegate void MethodInvoker();

        // AS 2/5/09 TFS11796
        private DispatcherOperation _pendingToolRegOperation;

		private void ProcessPendingToolRegistrationsWithDelay()
		{
            // AS 2/5/09 TFS11796
            // Store the dispatcher operation so we can process it synchronously if 
            // needed.
            //
            //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MethodInvoker(this.ProcessPendingToolRegistrations));
            _pendingToolRegOperation = this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MethodInvoker(this.ProcessPendingToolRegistrations));
		}

		private void ProcessPendingToolRegistrations()
		{
            // AS 2/5/09 TFS11796
            // Clear the cached dispatcher operation we stored when we started the process.
            //
            _pendingToolRegOperation = null;

			// AS 9/22/09 TFS22390
			// If we are waiting to verify the logical container of any menu tools then make sure 
			// we fix that up now.
			//
			this.VerifyMenuToolChildren();

			Dictionary<FrameworkElement, bool> pendingRegistrations = this._pendingToolRegistrations;
			this._pendingToolRegistrations = null;

			if (null != pendingRegistrations)
			{
				// AS 6/22/11 TFS77614
				// We need to process any unregistrations first.
				//
				#region Old Code
				//foreach (KeyValuePair<FrameworkElement, bool> item in pendingRegistrations)
				//{
				//    RibbonGroup group = item.Key as RibbonGroup;
				//
				//    if (null != group)
				//    {
				//        group.ProcessPendingRegistrationChange(this, item.Value);
				//        // AS 6/26/08 BR34379
				//        //return;
				//        continue;
				//    }
				//
				//    // if the item is not part of the ribbon...
				//    // AS 10/21/08 TFS9276
				//    // Some of the elements were actually supposed to be unregistered
				//    // but were being registered. A better check is to see if its still
				//    // a logical/visual descendant.
				//    //
				//    //if (XamRibbon.GetRibbon(item.Key) != this)
				//    // AS 2/5/09 TFS11796
				//    // The previous method walked up the visual tree using the logical tree
				//    // only when needed. However, in this case we really care about whether 
				//    // its still in the logical tree so I added an overload that allowed us 
				//    // to specify which to prefer.
				//    //
				//    //if (false == Utilities.IsDescendantOf(this, item.Key))
				//    if (false == Utilities.IsDescendantOf(this, item.Key, true))
				//    {
				//        Debug.Assert(item.Key is IRibbonToolPanel || item.Value == false, "The item was to be registered but its associated with the ribbon!");
				//        // AS 10/21/08 TFS9276
				//        // Call the new method since we don't want the pending registration
				//        // processing to start a new pending operation.
				//        //
				//        //this.UnregisterTool(item.Key);
				//        this.UnregisterToolImpl(item.Key);
				//    }
				//    else 
				//    {
				//        // it is in this ribbon so make sure its registered
				//        //Debug.Assert(item.Value == true, "The item was to be unregistered but its still associated with the ribbon!");
				//
				//        if (this.ToolInstanceManager.IsRegistered(item.Key) == false)
				//            this.RegisterTool(item.Key);
				//    }
				//} 
				#endregion //Old Code
				List<KeyValuePair<FrameworkElement, bool>> pendingRegistrationsList = new List<KeyValuePair<FrameworkElement, bool>>(pendingRegistrations);

				#region Group Removals
				// first process any group removals
				for (int i = pendingRegistrationsList.Count - 1; i >= 0; i--)
				{
					var item = pendingRegistrationsList[i];
				    RibbonGroup group = item.Key as RibbonGroup;
				
				    if (null != group && item.Value == false)
					{
						pendingRegistrationsList.RemoveAt(i);
				        group.ProcessPendingRegistrationChange(this, false);
					}
				} 
				#endregion //Group Removals

				#region Tool Removals
				// then process any tool removals
				for (int i = pendingRegistrationsList.Count - 1; i >= 0; i--)
				{
					var item = pendingRegistrationsList[i];

					if (item.Key is RibbonGroup == false)
					{
						// if the item is not part of the ribbon...
						// AS 10/21/08 TFS9276
						// Some of the elements were actually supposed to be unregistered
						// but were being registered. A better check is to see if its still
						// a logical/visual descendant.
						//
						//if (XamRibbon.GetRibbon(item.Key) != this)
						// AS 2/5/09 TFS11796
						// The previous method walked up the visual tree using the logical tree
						// only when needed. However, in this case we really care about whether 
						// its still in the logical tree so I added an overload that allowed us 
						// to specify which to prefer.
						//
						//if (false == Utilities.IsDescendantOf(this, item.Key))
						if (false == Utilities.IsDescendantOf(this, item.Key, true))
						{
							pendingRegistrationsList.RemoveAt(i);

							Debug.Assert(item.Key is IRibbonToolPanel || item.Value == false, "The item was to be registered but its associated with the ribbon!");
							// AS 10/21/08 TFS9276
							// Call the new method since we don't want the pending registration
							// processing to start a new pending operation.
							//
							//this.UnregisterTool(item.Key);
							this.UnregisterToolImpl(item.Key);
						}
					}
				} 
				#endregion //Tool Removals

				#region Group Additions
				// group additions
				for (int i = pendingRegistrationsList.Count - 1; i >= 0; i--)
				{
					var item = pendingRegistrationsList[i];
				    RibbonGroup group = item.Key as RibbonGroup;
				
				    if (null != group)
					{
						pendingRegistrationsList.RemoveAt(i);
				        group.ProcessPendingRegistrationChange(this, true);
					}
				} 
				#endregion //Group Additions 

				#region Tool Additions
				// group additions
				for (int i = pendingRegistrationsList.Count - 1; i >= 0; i--)
				{
					var item = pendingRegistrationsList[i];

				    if (item.Key is RibbonGroup == false)
					{
						pendingRegistrationsList.RemoveAt(i);

				        // it is in this ribbon so make sure its registered
				        //Debug.Assert(item.Value == true, "The item was to be unregistered but its still associated with the ribbon!");
				
				        if (this.ToolInstanceManager.IsRegistered(item.Key) == false)
				            this.RegisterTool(item.Key);
					}
				} 
				#endregion //Tool Additions 
			}

			// AS 7/1/10 TFS32477
			// A tool could have been removed from one location and added to antoher 
			// in which case the location and other information may be incorrect and 
			// need to be updated. This hashset is a list of the tools added and removed
			// from the pendingtoolregistrations.
			//
			#region Verify Tools
			if (null != _toolsToVerify)
			{
				HashSet toolsToVerify = _toolsToVerify;
				_toolsToVerify = null;

				foreach (object item in toolsToVerify)
				{
					FrameworkElement tool = item as FrameworkElement;

					if (tool == null || tool is RibbonGroup)
						continue;

					IRibbonToolLocation locationProvider;
					ToolLocation location = GetToolLocation(tool, out locationProvider);

					// AS 11/16/07 BR28515
					// Unknown is ok if its in the ToolsNotInRibbon
					//
					Debug.Assert(location != ToolLocation.Unknown || XamRibbon.GetIsInToolsNotInRibbon(tool));

					tool.SetValue(XamRibbon.LocationPropertyKey, RibbonKnownBoxes.FromValue(location));

					// menus and ribbon tool panels will be specially handled
					// since the ribbon group needs to know about these to properly handle resizing
					if (tool is IRibbonTool || tool is IRibbonToolPanel)
					{
						if (location == ToolLocation.Ribbon)
						{
							Debug.Assert(locationProvider is RibbonGroup);
							tool.SetValue(RibbonGroup.ContainingGroupPropertyKey, locationProvider as RibbonGroup);
						}
						else
						{
							tool.ClearValue(RibbonGroup.ContainingGroupPropertyKey);
						}

						if (locationProvider is ToolbarBase)
						{
							Debug.Assert(location == ToolLocation.ApplicationMenuFooterToolbar || location == ToolLocation.QuickAccessToolbar);
							tool.SetValue(ToolbarBase.ContainingToolbarPropertyKey, (ToolbarBase)locationProvider);
						}
						else
						{
							tool.ClearValue(ToolbarBase.ContainingToolbarPropertyKey);
						}

						// we're not actually registering the panels as tools - just using the delayed 
						// ribbon change processing
						if (tool is IRibbonToolPanel)
							return;
					}
				}
			} 
			#endregion //Verify Tools
		} 
				#endregion //ProcessPendingToolRegistrations

				// AS 1/4/12 TFS98111
				// Created a new helper routine used tabs/tabgroups are added/removed so we can add new tabs 
				// to the logical tree before the elements are added to the visual tree and remove any tabs 
				// after they are removed from the visual tree.
				//
				#region ProcessTabCollectionChange
		private void ProcessTabCollectionChange()
		{
			if (this.IsInitialized)
			{
				List<RibbonTabItem> tabsToRemove = new List<RibbonTabItem>();
				this.RefreshTabLogicalChildren(tabsToRemove);

				this.UpdateTabsInView();

				for (int i = tabsToRemove.Count - 1; i >= 0; i--)
					this.RemoveLogicalChildHelper(tabsToRemove[i]);
			}
			else
			{
				this.RefreshTabLogicalChildren(null);
			}
		}
				#endregion //ProcessTabCollectionChange

				#region RefreshTabLogicalChildren
		// AS 1/4/12 TFS98111
		//private void RefreshTabLogicalChildren()
		/// <summary>
		/// Adds and optionally removes tabs as logical children.
		/// </summary>
		/// <param name="tabsToRemoveAfter">A list to populate with the items that should be removed or null to have them removed synchronously</param>
		private void RefreshTabLogicalChildren(List<RibbonTabItem> tabsToRemoveAfter)
		{
			// keep a dictionary where the key is the child and the boolean
			// indicates if it was previously a logical child
			Dictionary<object, bool> logicalChildren = new Dictionary<object, bool>();

			foreach (RibbonTabItem newItem in this.Tabs)
				logicalChildren.Add(newItem, false);

			foreach (ContextualTabGroup group in this.ContextualTabGroups)
			{
				foreach (RibbonTabItem groupTab in group.Tabs)
					logicalChildren.Add(groupTab, false);
			}

			// reverify everything - remove any old that no longer exist
			for (int i = this._logicalChildren.Count - 1; i >= 0; i--)
			{
				object oldChild = this._logicalChildren[i];

				// if the group is no longer in the collection then remove it from the logical tree
				if (oldChild is RibbonTabItem && logicalChildren.ContainsKey(oldChild) == false)
				{
					// AS 1/4/12 TFS98111
					// Allow a list to be passed in so the caller can remove the logical children later.
					//
					//this._logicalChildren.RemoveAt(i);
					//this.RemoveLogicalChild(oldChild);
					if (tabsToRemoveAfter == null)
					{
						this._logicalChildren.RemoveAt(i);
						this.RemoveLogicalChild(oldChild);
					}
					else
					{
						tabsToRemoveAfter.Add(oldChild as RibbonTabItem);
					}
				}
				else // the item was already a logical child so update its state
					logicalChildren[oldChild] = true;
			}

			// iterate the dictionary and anything that wasn't in the logical tree
			// needs to be added now.
			foreach (KeyValuePair<object, bool> entry in logicalChildren)
			{
				if (entry.Value == false)
					this.AddLogicalChildHelper(entry.Key);
			}
		} 
				#endregion //RefreshTabLogicalChildren

				#region RemoveLogicalChildHelper
		private void RemoveLogicalChildHelper(object oldChild)
		{
			if (null != oldChild)
			{
				this._logicalChildren.Remove(oldChild);
				this.RemoveLogicalChild(oldChild);
			}
		} 
				#endregion //RemoveLogicalChildHelper

				#region RemovePendingToolRegistration
		internal void RemovePendingToolRegistration(FrameworkElement tool, bool isRegistering)
		{
			if (this._pendingToolRegistrations != null)
			{
				bool wasRegistering;

				if (this._pendingToolRegistrations.TryGetValue(tool, out wasRegistering))
				{
                    // AS 2/10/09
					//Debug.Assert(wasRegistering != isRegistering, "The tool's old and new registering states are the same!");
					this._pendingToolRegistrations.Remove(tool);
				}
			}
		} 
				#endregion //RemovePendingToolRegistration

				#region SetMode

		private void SetMode(RibbonMode mode)
		{
			this.SetMode(mode, true);
		}

		private void SetMode(RibbonMode mode, bool restoreFocusOnNormal)
		{
			RibbonMode oldMode = this._ribbonMode;

			if (mode == oldMode)
				return;

			this._ribbonMode = mode;

			// set the internal IsModeSpecialPropertyKey so that RibbonTabItem's Focusable property 
			// that is bound to this property can be kept in sync and only focusable when not
			// in normal mode
			this.SetValue(IsModeSpecialPropertyKey, this._ribbonMode != RibbonMode.Normal);

			switch (this._ribbonMode)
			{
				case RibbonMode.ActiveItemNavigation:
					if (this.HasKeyTipManager)
					{
						this.KeyTipManager.HideKeyTips();
					}
					break;
				case RibbonMode.KeyTipsActive:
					this.Focus();

					if (this.IsMinimized)
					{
						// AS 10/18/07
						// The application menu isn't focusable - its presenter is.
						//
						//if (this.ApplicationMenu != null)
						//	this.SetActiveItem(this.ApplicationMenu);
						if (this.ApplicationMenu != null && this.ApplicationMenu.Presenter != null)
							this.ApplicationMenu.Presenter.Focus();
						else
							if (this.SelectedTab == null)
								this.SelectNextTab();
					}

					this.KeyTipManager.ShowKeyTips();

					break;
				case RibbonMode.KeyTipsPending:
					this.KeyTipManager.ShowKeyTipsAfterDelay();
					break;
				case RibbonMode.Normal:
					this.SetActiveItem( null );
					if (this.HasKeyTipManager)
					{
						this.KeyTipManager.HideKeyTips();
					}

					if (restoreFocusOnNormal)
					{
						this.TransferFocusOutOfRibbon();

						// AS 10/1/07
						// When we are coming out of activation mode as a result of a mouse down on a tab
						// item, then the focusable of the tab item would have been false by the time the 
						// tab gets the mouse down so the focusedelement of the focusscope (i.e. the ribbon)
						// would be the previously selected tab. So when we regain focus later, that element
						// will be focused and will make itself the selected item. To preview this, we'll set
						// the focused element to the currently selected tab.
						//
						this.SetSelectedTabAsFocusElement();
					}

					break;
			}

            // AS 10/14/08 TFS6092
            // We only want cycle while in keytip/activeitem nav modes.
            //
            this.CoerceValue(KeyboardNavigation.TabNavigationProperty);

			// AS 10/22/07 BR27620
			// We need to capture the mouse so that we can hide the keytips when the 
			// mouse is pressed down. We'll only do this when not hosted in the ribbon
			// window. When hosted in a ribbon window, we'll let the window handle 
			// resetting the mode if an nc mouse down message comes in so that it doesn't
			// require 2 clicks to move/resize the window.
			//
			if (this.ShouldCaptureMouseForRibbonMode)
			{
				DependencyObject currentCapture = Mouse.Captured as DependencyObject;

				// if the mouse is not captured or is captured by something outside
				// the ribbon, then take capture
				if (Mouse.Captured == null || XamRibbon.GetRibbon(currentCapture) != this)
					Mouse.Capture(this, CaptureMode.SubTree);
			}
			else
			{
				if (this.IsMouseCaptured)
					this.ReleaseMouseCapture();
			}
		}

				#endregion //SetMode	
    
				#region SetSelectedTabAsFocusElement
		// AS 10/1/07
		private void SetSelectedTabAsFocusElement()
		{
			DependencyObject focusScope = FocusManager.GetFocusScope(this);

			if (null != focusScope)
			{
				DependencyObject focusedElement = FocusManager.GetFocusedElement(this) as DependencyObject;

				if (null != focusedElement && this.IsAncestorOf(focusedElement))
					FocusManager.SetFocusedElement(focusScope, this.SelectedTab);
			}
		} 
				#endregion //SetSelectedTabAsFocusElement

				#region SubtractThickness
		private static Rect SubtractThickness(Rect rect, Thickness thickness)
		{
			if (null != thickness)
			{
				rect.X += thickness.Left;
				rect.Width -= Math.Min(rect.Width, thickness.Left + thickness.Right);
				rect.Y += thickness.Top;
				rect.Height -= Math.Min(rect.Height, thickness.Top + thickness.Bottom);
			}

			return rect;
		} 
				#endregion //SubtractThickness

				#region UpdateImageProperties
		
#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)

				#endregion //UpdateImageProperties

				#region UpdateTabsInView

		private void UpdateTabsInView()
		{
			
#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

			List<RibbonTabItem> tabsInView = new List<RibbonTabItem>();

			tabsInView.AddRange(this.Tabs);

			// Add the RibbonTabItems from the currently visible ContextualTabGroups.
			foreach (ContextualTabGroup contextualTabGroup in this.ContextualTabGroups)
			{
				if (contextualTabGroup.IsVisible)
					tabsInView.AddRange(contextualTabGroup.Tabs);
			}

			bool areDifferent = tabsInView.Count != _tabsInViewInternal.Count;

			if (!areDifferent)
			{
				for (int i = 0, count = tabsInView.Count; i < count; i++)
				{
					if (tabsInView[i] != _tabsInViewInternal[i])
					{
						areDifferent = true;
						break;
					}
				}
			}

			if (areDifferent)
			{
				bool reinitialize = true;

				if (tabsInView.Count != 0 && _tabsInViewInternal.Count != 0 && SelectedTab != null)
				{
					// build a list of the new tabs
					Dictionary<RibbonTabItem, int> newIndexes = new Dictionary<RibbonTabItem, int>();

					for (int i = tabsInView.Count - 1; i >= 0; i--)
					{
						newIndexes[tabsInView[i]] = i;
					}

					// if the currently selected tab is already in the list then we want to 
					// avoid the reset notification
					if (newIndexes.ContainsKey(this.SelectedTab))
					{
						reinitialize = false;

						Dictionary<RibbonTabItem, int> oldIndexes = new Dictionary<RibbonTabItem, int>();

						for (int i = _tabsInViewInternal.Count - 1; i >= 0; i--)
						{
							RibbonTabItem oldTab = _tabsInViewInternal[i];

							// remove any that aren't in the new list
							if (!newIndexes.ContainsKey(oldTab))
								_tabsInViewInternal.RemoveAt(i);
							else
								oldIndexes[oldTab] = i;
						}

						int insertedCount = 0;

						for (int i = 0, count = tabsInView.Count; i < count; i++)
						{
							RibbonTabItem tabItem = tabsInView[i];
							int oldIndex;

							if (!oldIndexes.TryGetValue(tabItem, out oldIndex))
							{
								_tabsInViewInternal.Insert(i, tabItem);
								insertedCount++;
							}
							else
							{
								int actualIndex = insertedCount + oldIndex;

								if (actualIndex < 0 ||
									actualIndex >= _tabsInViewInternal.Count ||
									_tabsInViewInternal[actualIndex] != tabItem)
								{
									actualIndex = _tabsInViewInternal.IndexOf(tabItem);
									Debug.Assert(actualIndex >= 0);
								}

								if (actualIndex != i)
									_tabsInViewInternal.Move(actualIndex, i);
							}
						}
					}
				}

				if (reinitialize)
					_tabsInViewInternal.ReInitialize(tabsInView);
			}

			// AS 3/18/11 TFS66436
			// 
			//// also the selected tab may have been removed in which case we want to select
			//// the home/first tab.
			//object selectedTab = this.RibbonTabControl.SelectedItem;
			//
			//if (this._ribbonTabControl != null && this.RibbonTabControl.SelectedItem == null && this.IsMinimized == false)
			//    this.SelectNextTab();
			//
			//// AS 12/6/07 BR28967/BR28970
			//// Make sure there is only 1 selected tab item. Unselect all others.
			////
			//selectedTab = this.RibbonTabControl.SelectedItem;
			//
			//foreach (RibbonTabItem tab in this._tabsInViewInternal)
			//{
			//    if (tab != selectedTab)
			//        tab.IsSelected = false;
			//}
			this.VerifySelectedTab();
		}
				#endregion //UpdateTabsInView	

				#region UpdateThemeResources

		private void UpdateThemeResources()
		{
			string[] groupings = new string[] { PrimitivesGeneric.Location.Grouping, EditorsGeneric.Location.Grouping, RibbonGeneric.Location.Grouping };

			// Call the ThemeManager's OnThemeChanged helpper method to merge the theme resourcesets into our Resources collection in
			// AS 6/11/12 TFS109327
			// The Ribbon may be part of a RibbonWindowContentHost but not yet hooked up with it yet 
			// because it is in the middle of being initialized.
			//
			//if (this._ribbonWindow != null)
			//    // AS 6/3/08 BR32772
			//    //ThemeManager.OnThemeChanged(this._ribbonWindow, this.Theme/* AS 5/9/08 this._theme */, groupings);
			//    ThemeManager.OnThemeChanged(this._ribbonWindow.Window, this.Theme/* AS 5/9/08 this._theme */, groupings);
			//else
			//    ThemeManager.OnThemeChanged(this, this.Theme, groupings);
			FrameworkElement target = this;

			if (_ribbonWindow != null)
				target = _ribbonWindow.Window;
			else
			{
				var rwch = this.Parent as RibbonWindowContentHost;

				if (rwch != null && rwch.Ribbon == this)
				{
					var irw = rwch.Parent as IRibbonWindow;

					// we will assume that if the ribbonwindow contenthost is within a xamRibbonWindow that is being
					// initialized that it is this ribbonwindowcotnenthost that is being used.
					if (irw != null && (irw.ContentHost == rwch || (irw.ContentHost == null && irw.Window != null && !irw.Window.IsInitialized)))
						target = irw.Window;
				}
			}

			ThemeManager.OnThemeChanged(target, this.Theme, groupings);

			// AS 9/4/09 TFS21087
			// Do not call UpdateLayout while the control is being initialized.
			//
			if (this.IsInitialized)
			{
				// JJD 2/26/07
				// we need to call UpdateLayout after we change the merged dictionaries.
				// Otherwise, the styles from the new merged dictionary are not picked
				// up right away. It seems the framework must be caching some information
				// that doesn't get refreshed until the next layout update
				this.InvalidateMeasure();
				this.UpdateLayout();
			}
		}

				#endregion //UpdateThemeResources	
        
				#region VerifyGroupIsRegistered
		private void VerifyGroupIsRegistered(RibbonGroup group, string id)
		{
			Debug.Assert(group != null);

			if (string.IsNullOrEmpty(id))
			{
				group.Id = ToolInstanceManager.GetNewId();
				return;
			}

			RibbonGroup oldGroup;

			if (this._registeredGroups.TryGetValue(id, out oldGroup))
			{
				if (oldGroup == group)
					return;

				if (group.CloneFromGroup == oldGroup)
					return;

				// JM 06-01-10 TFS32650
				RibbonGroup replacedByGroup = oldGroup.GetValue(RibbonGroup.ReplacedByGroupProperty) as RibbonGroup;
				if (replacedByGroup == group)
					return;

				throw new InvalidOperationException(GetString("LE_GroupRegisteredWithSameId", id));
			}

			this._registeredGroups.Add(id, group);
		} 
				#endregion //VerifyGroupIsRegistered 

				// AS 12/4/07 HighContrast Support
				#region VerifyHighContrast
		private static void VerifyHighContrast(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbon ribbon = (XamRibbon)d;
			ribbon.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind, new MethodInvoker(ribbon.VerifyHighContrast));
		}
		
		private void VerifyHighContrast()
		{
			bool useHighContrast = true.Equals(this.GetValue(IsSystemHighContrastProperty)) ||
				(this.GetValue(ControlColorProperty).Equals(Colors.Black) && this.GetValue(ControlTextColorProperty).Equals(Colors.White)) ||
				(this.GetValue(ControlColorProperty).Equals(Colors.White) && this.GetValue(ControlTextColorProperty).Equals(Colors.Black));

			this.SetValue(IsInHighContrastModePropertyKey, KnownBoxes.FromValue(useHighContrast));
		} 
				#endregion //VerifyHighContrast

				// AS 8/19/11 TFS83576
				#region VerifyIsGlassCaptionGlowVisible
		private void VerifyIsGlassCaptionGlowVisible()
		{
			bool useGlassGlow = false;

			if (_ribbonWindow != null && GetIsGlassActive(this))
			{
				// in vista (i.e. 6.0), the glow is not shown when the window is maximized
				Version osVersion = System.Environment.OSVersion.Version;

				if (osVersion.Major >= 7 || osVersion.Minor > 0)
					useGlassGlow = true;
				else if (_ribbonWindow.WindowState == WindowState.Normal)
					useGlassGlow = true;
			}

			this.SetValue(IsGlassCaptionGlowVisiblePropertyKey, KnownBoxes.FromValue(useGlassGlow));
		}
				#endregion //VerifyIsGlassCaptionGlowVisible

				#region VerifyParts

		private void VerifyParts()
		{
			// JM/AS 05-09-08 - As part of theming changes need to do this so that the inherited Ribbon property
			// gets set on all children.
			object[] oldChildren = new object[this._logicalChildren.Count];
			this._logicalChildren.CopyTo(oldChildren, 0);

			foreach (object child in oldChildren)
			{
				// AS 10/8/09 TFS23328
				// Temporarily store the datacontext while we reparent the children.
				//
				using (new TempValueReplacement(child as DependencyObject, FrameworkElement.DataContextProperty))
				{
					this.RemoveLogicalChildHelper(child);
					this.AddLogicalChildHelper(child);
				}
			}

			// AS 10/8/09 TFS23328
			// Temporarily store the datacontext while we reparent the ApplicationMenu.
			//
			ApplicationMenu appMenu = this.ApplicationMenu;
			using (new TempValueReplacement(appMenu, FrameworkElement.DataContextProperty))
			{
				// AS 2/18/09 TFS14108
				// There seems to be a bug in the WPF framework whereby if the element is pulled
				// out of the logical tree and put back in before it is removed from the logical
				// tree that its still getting unloaded - its IsLoaded is false. The workaround is
				// to temporarily pull the appmenu/qat out of the logical tree before removing it
				// from the visual tree. Then put it back into the logical tree before putting it
				// in its associated site.
				//
				bool readdAppMenu = null != appMenu && appMenu.IsLoaded;

				if (readdAppMenu)
					this.RemoveLogicalChildHelper(appMenu);

				// ApplicationMenu Site
				if (this._applicationMenuSite != null && this._applicationMenuSite.Content == appMenu)
					this._applicationMenuSite.Content = null;

				// AS 7/1/10 TFS34576
				if (null != _applicationMenuSite)
					_applicationMenuSite.Loaded -= new RoutedEventHandler(OnApplicationMenuSiteLoaded);

				this._applicationMenuSite = base.GetTemplateChild("PART_ApplicationMenuSite") as ContentControl;

				// AS 2/18/09 TFS14108
				if (readdAppMenu)
					this.AddLogicalChildHelper(appMenu);

				if (this._applicationMenuSite != null && this._applicationMenuSite.Content == null)
				{
					// AS 7/1/10 TFS34576
					//this._applicationMenuSite.Content = this.ApplicationMenu;
					if (_applicationMenuSite.IsLoaded || appMenu == null || !appMenu.IsLoaded)
						_applicationMenuSite.Content = appMenu;
					else
						_applicationMenuSite.Loaded += new RoutedEventHandler(OnApplicationMenuSiteLoaded);
				}
			}


			// RibbonTabControl Site
			if (this._ribbonTabControlSite != null && this._ribbonTabControlSite.Content == this.RibbonTabControl)
				this._ribbonTabControlSite.Content = null;

			// AS 10/15/10 TFS36676/TFS34576
			if (null != _ribbonTabControlSite)
				_ribbonTabControlSite.Loaded -= new RoutedEventHandler(OnTabControlSiteLoaded);

			this._ribbonTabControlSite = base.GetTemplateChild("PART_RibbonTabControlSite") as ContentControl;

			if (this._ribbonTabControlSite != null && this._ribbonTabControlSite.Content == null)
			{
				// AS 10/15/10 TFS36676/TFS34576
				//this._ribbonTabControlSite.Content = this.RibbonTabControl;
				if (_ribbonTabControlSite.IsLoaded || _ribbonTabControl == null || !_ribbonTabControl.IsLoaded)
					_ribbonTabControlSite.Content = this.RibbonTabControl;
				else
					_ribbonTabControlSite.Loaded += new RoutedEventHandler(OnTabControlSiteLoaded);
			}

			// AS 10/8/09 TFS23328
			// Temporarily store the datacontext while we reparent the qat.
			//
			using (new TempValueReplacement(this.QuickAccessToolbar, FrameworkElement.DataContextProperty))
			{
				// AS 2/18/09 TFS14108
				bool readdQat = null != this.QuickAccessToolbar && this.QuickAccessToolbar.IsLoaded;

				if (readdQat)
					this.RemoveLogicalChildInternal(this.QuickAccessToolbar);

				// keep track of the caption panel that should host the qat when its positioned on top
				if (null != this._captionPanel)
					this._captionPanel.QuickAccessToolbar = null;

				this._captionPanel = base.GetTemplateChild("PART_RibbonCaptionPanel") as RibbonCaptionPanel;

				// the site where the qat will be displayed when its location is on the bottom
				if (null != this._quickAccessToolbarBottomSite)
					this._quickAccessToolbarBottomSite.Content = null;

				this._quickAccessToolbarBottomSite = base.GetTemplateChild("PART_QuickAccessToolbarBottomSite") as ContentControl;

				// AS 2/18/09 TFS14108
				if (readdQat)
					this.AddLogicalChildInternal(this.QuickAccessToolbar);
			}

			this.VerifyQatLocation();

			// AS 11/2/07 CaptionHeight - WorkItem #562
			// Bind the height of the caption so we pick up any changes at runtime.
			//
			FrameworkElement captionBorder = this.GetTemplateChild("PART_XamRibbonCaption") as FrameworkElement;

			if (null != captionBorder)
				this.SetBinding(CaptionHeightProperty, Utilities.CreateBindingObject(FrameworkElement.ActualHeightProperty, BindingMode.OneWay, captionBorder));
			else
				this.ClearValue(CaptionHeightProperty);
		}

				#endregion //VerifyParts

				#region VerifyQatLocation
		private void VerifyQatLocation()
		{
			// JM BR29033 12-13-07
			if (this.IsInitialized == false)
				return;

			QuickAccessToolbar qat = this.QuickAccessToolbar;
			bool isAbove = this.QuickAccessToolbarLocation == QuickAccessToolbarLocation.AboveRibbon;

			if (qat.IsBelowRibbon == isAbove)
				qat.SetValue(QuickAccessToolbar.IsBelowRibbonPropertyKey, !qat.IsBelowRibbon);

            // JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
            this.CalculateMargins();

			// nothing to do if we're already synched
			if (isAbove && this._captionPanel != null && this._captionPanel.QuickAccessToolbar == qat)
				return;
			else if (isAbove == false && this._quickAccessToolbarBottomSite != null && this._quickAccessToolbarBottomSite.Content == qat)
				return;

			// AS 7/1/10 TFS34457
			// If the element to which we are going to add the qat is not loaded but 
			// the QAT is loaded then we need to wait for that element's Loaded event.
			//
			FrameworkElement target = isAbove ? (FrameworkElement)_captionPanel : _quickAccessToolbarBottomSite;
			FrameworkElement targetPendingLoad = target != null && !target.IsLoaded ? target : null;

			// if the target has changed then unhook from the old target and hook 
			// the new one if needed
			if (targetPendingLoad != _qatTargetPendingLoad)
			{
				if (_qatTargetPendingLoad != null)
					_qatTargetPendingLoad.Loaded -= new RoutedEventHandler(this.OnQatTargetLoaded);

				_qatTargetPendingLoad = targetPendingLoad;

				if (_qatTargetPendingLoad != null)
					_qatTargetPendingLoad.Loaded += new RoutedEventHandler(this.OnQatTargetLoaded);
			}

			// if we're still waiting then come back when that element has loaded
			if (_qatTargetPendingLoad != null)
				return;

			this.SuspendToolRegistration();

			try
			{
				// since this is basically changing the visual children, we need to clear
				// the old site first
				if (isAbove && this._quickAccessToolbarBottomSite != null)
					this._quickAccessToolbarBottomSite.Content = null;
				else if (isAbove == false && this._captionPanel != null)
					this._captionPanel.QuickAccessToolbar = null;

				// now we can set it on the new site
				if (isAbove == false && this._quickAccessToolbarBottomSite != null)
					this._quickAccessToolbarBottomSite.Content = qat;
				else if (isAbove && this._captionPanel != null)
					this._captionPanel.QuickAccessToolbar = qat;
			}
			finally
			{
				this.ResumeToolRegistration();
			}
		} 
				#endregion //VerifyQatLocation

				#region WireTopLevelVisual

		private void WireTopLevelVisual()
		{
			// AS 6/3/08 BR32772
			//FrameworkElement topVisual = this._ribbonWindow;
			FrameworkElement topVisual = null != this._ribbonWindow ? this._ribbonWindow.Window : null;

			// AS 1/3/08 BR29371
			// Previously we were never unhooking from the toplevelvisual once we attached
			// with it. Now, we only hook once the ribbon has been loaded and then we unhook
			// when the ribbon has not been unloaded. I'm leaving the ribbon to reference
			// the ribbon window as its top visual should this be hosted within a ribbon
			// window.
			//
			//if (topVisual == null)
			if (topVisual == null && this.IsLoaded)
			{
				DependencyObject ancestor = this;

				while (ancestor != null)
				{
					ancestor = VisualTreeHelper.GetParent(ancestor);

					if (ancestor is FrameworkElement)
						topVisual = ancestor as FrameworkElement;
				}
			}

			if (topVisual != this._topLevelVisual)
			{
				if (this._topLevelVisual != null)
				{
					this._topLevelVisual.IsKeyboardFocusWithinChanged -= new DependencyPropertyChangedEventHandler(OnWindowIsKeyboardFocusWithinChanged);
					this._topLevelVisual.PreviewKeyDown -= new KeyEventHandler(OnWindowPreviewKeyDown);
					this._topLevelVisual.PreviewKeyUp -= new KeyEventHandler(OnWindowPreviewKeyUp);
					this._topLevelVisual.PreviewMouseDown -= new MouseButtonEventHandler(OnWindowPreviewMouseDown);
					this._topLevelVisual.PreviewTextInput -= new TextCompositionEventHandler(OnWindowPreviewTextInput);

					// AS 10/12/07 BR27159
					this._topLevelVisual.PreviewMouseWheel -= new MouseWheelEventHandler(OnWindowPreviewMouseWheel);
				}

				this._topLevelVisual = topVisual;

				if (this._topLevelVisual != null)
				{
					this._topLevelVisual.IsKeyboardFocusWithinChanged += new DependencyPropertyChangedEventHandler(OnWindowIsKeyboardFocusWithinChanged);
					this._topLevelVisual.PreviewKeyDown += new KeyEventHandler(OnWindowPreviewKeyDown);
					this._topLevelVisual.PreviewKeyUp += new KeyEventHandler(OnWindowPreviewKeyUp);
					this._topLevelVisual.PreviewMouseDown += new MouseButtonEventHandler(OnWindowPreviewMouseDown);
					this._topLevelVisual.PreviewTextInput += new TextCompositionEventHandler(OnWindowPreviewTextInput);

					// AS 10/12/07 BR27159
					this._topLevelVisual.PreviewMouseWheel += new MouseWheelEventHandler(OnWindowPreviewMouseWheel);
				}

				// AS 10/24/07 AutoHide
				if (this._topLevelVisual != null)
					this.SetBinding(TopLevelVisualActualHeightProperty, Utilities.CreateBindingObject(FrameworkElement.ActualHeightProperty, BindingMode.OneWay, this._topLevelVisual));
				else
					this.ClearValue(TopLevelVisualActualHeightProperty);
			}

		}

				#endregion //WireTopLevelVisual	
            
            #endregion //Private Methods

        #endregion //Methods

        #region Events

        #region ActiveItemChanged

        /// <summary>
		/// Event ID for the <see cref="ActiveItemChanged"/> routed event
		/// </summary>
		/// <seealso cref="ActiveItemChanged"/>
		/// <seealso cref="OnActiveItemChanged"/>
		public static readonly RoutedEvent ActiveItemChangedEvent =
			EventManager.RegisterRoutedEvent("ActiveItemChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<FrameworkElement>), typeof(XamRibbon));

		/// <summary>
		/// Used to raise the <see cref="ActiveItemChanged"/> event.
		/// </summary>
		/// <param name="args">The event arguments indicating the element that has become the active item.</param>
		/// <seealso cref="ActiveItemChanged"/>
		/// <seealso cref="ActiveItemChangedEvent"/>
		protected virtual void OnActiveItemChanged(RoutedPropertyChangedEventArgs<FrameworkElement> args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseActiveItemChanged(RoutedPropertyChangedEventArgs<FrameworkElement> args)
		{
			args.RoutedEvent	= XamRibbon.ActiveItemChangedEvent;
			args.Source			= this;
			this.OnActiveItemChanged(args);
		}

		/// <summary>
		/// Occurs when the <see cref="ActiveItem"/> property has changed.
		/// </summary>
		/// <seealso cref="ActiveItem"/>
		/// <seealso cref="OnActiveItemChanged"/>
		/// <seealso cref="ActiveItemChangedEvent"/>
		//[Description( "Occurs when the 'ActiveItem' property has changed." )]
		//[Category("Ribbon Events")]
		public event RoutedPropertyChangedEventHandler<FrameworkElement> ActiveItemChanged
		{
			add
			{
				base.AddHandler(XamRibbon.ActiveItemChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamRibbon.ActiveItemChangedEvent, value);
			}
		}

			#endregion //ActiveItemChanged

			#region ExecutingCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutingCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		public static readonly RoutedEvent ExecutingCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutingCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutingCommandEventArgs>), typeof(XamRibbon));

		/// <summary>
		/// Used to raise the <see cref="ExecutingCommand"/> event.
		/// </summary>
		/// <param name="args">Event arguments for the event</param>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		protected virtual void OnExecutingCommand(ExecutingCommandEventArgs args)
		{
			this.RaiseEvent( args );
		}

		internal bool RaiseExecutingCommand(ExecutingCommandEventArgs args)
		{
			args.RoutedEvent	= XamRibbon.ExecutingCommandEvent;
			args.Source			= this;
			this.OnExecutingCommand(args);

			return args.Cancel == false;
		}

		/// <summary>
		/// Cancelable event that occurs before the action associated with a command is performed
		/// </summary>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		//[Description("Cancelable event that occurs before the action associated with a command is performed")]
		//[Category("Ribbon Events")]
		public event EventHandler<ExecutingCommandEventArgs> ExecutingCommand
		{
			add
			{
				base.AddHandler(XamRibbon.ExecutingCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamRibbon.ExecutingCommandEvent, value);
			}
		}

			#endregion //ExecutingCommand

			#region ExecutedCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutedCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		public static readonly RoutedEvent ExecutedCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutedCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutedCommandEventArgs>), typeof(XamRibbon));

		/// <summary>
		/// Used to raise the <see cref="ExecutedCommand"/> event.
		/// </summary>
		/// <param name="args">Event arguments for the event being raised</param>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		protected virtual void OnExecutedCommand(ExecutedCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseExecutedCommand(ExecutedCommandEventArgs args)
		{
			args.RoutedEvent	= XamRibbon.ExecutedCommandEvent;
			args.Source			= this;
			this.OnExecutedCommand(args);
		}

		/// <summary>
		/// Occurs after the action associated with a command is performed
		/// </summary>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		//[Description( "Occurs after the action associated with a command is performed" )]
		//[Category("Ribbon Events")]
		public event EventHandler<ExecutedCommandEventArgs> ExecutedCommand
		{
			add
			{
				base.AddHandler(XamRibbon.ExecutedCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamRibbon.ExecutedCommandEvent, value);
			}
		}

			#endregion //ExecutedCommand

			#region RibbonTabItemCloseUp

		/// <summary>
		/// Event ID for the <see cref="RibbonTabItemCloseUp"/> routed event
		/// </summary>
		/// <seealso cref="RibbonTabItemCloseUp"/>
		/// <seealso cref="OnRibbonTabItemCloseUp"/>
		public static readonly RoutedEvent RibbonTabItemCloseUpEvent =
			EventManager.RegisterRoutedEvent("RibbonTabItemCloseUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamRibbon));

		/// <summary>
		/// Used to raise the <see cref="RibbonTabItemCloseUp"/>
		/// </summary>
		/// <param name="args">Event arguments for the event being raised</param>
		/// <seealso cref="RibbonTabItemCloseUp"/>
		/// <seealso cref="RibbonTabItemCloseUpEvent"/>
		protected virtual void OnRibbonTabItemCloseUp(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseRibbonTabItemCloseUp(RoutedEventArgs args)
		{
			args.RoutedEvent	= XamRibbon.RibbonTabItemCloseUpEvent;
			args.Source			= this;
			this.OnRibbonTabItemCloseUp(args);
		}

		/// <summary>
		/// Occurs when the <see cref="IsMinimized"/> of the <see cref="XamRibbon"/> is true and the popup for the <see cref="SelectedTab"/> has closed up.
		/// </summary>
		/// <seealso cref="RibbonTabItemOpening"/>
		/// <seealso cref="RibbonTabItemOpened"/>
		/// <seealso cref="OnRibbonTabItemCloseUp"/>
		/// <seealso cref="RibbonTabItemCloseUpEvent"/>
		//[Description("Occurs when the 'IsMinimized' of the XamRibbon is true and the popup for the 'SelectedTab' has closed up.")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler RibbonTabItemCloseUp
		{
			add
			{
				base.AddHandler(XamRibbon.RibbonTabItemCloseUpEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamRibbon.RibbonTabItemCloseUpEvent, value);
			}
		}

			#endregion //RibbonTabItemCloseUp

			#region RibbonTabItemOpened

		/// <summary>
		/// Event ID for the <see cref="RibbonTabItemOpened"/> routed event
		/// </summary>
		/// <seealso cref="RibbonTabItemOpened"/>
		/// <seealso cref="OnRibbonTabItemOpened"/>
		public static readonly RoutedEvent RibbonTabItemOpenedEvent =
			EventManager.RegisterRoutedEvent("RibbonTabItemOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamRibbon));

		/// <summary>
		/// Used to raise the <see cref="RibbonTabItemOpened"/> event
		/// </summary>
		/// <param name="args">Event arguments for the event being raised</param>
		/// <seealso cref="RibbonTabItemOpened"/>
		/// <seealso cref="RibbonTabItemOpenedEvent"/>
		protected virtual void OnRibbonTabItemOpened(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseRibbonTabItemOpened(RoutedEventArgs args)
		{
			args.RoutedEvent	= XamRibbon.RibbonTabItemOpenedEvent;
			args.Source			= this;
			this.OnRibbonTabItemOpened(args);
		}

		/// <summary>
		/// Occurs when the <see cref="IsMinimized"/> of the <see cref="XamRibbon"/> is true and the popup for the <see cref="SelectedTab"/> has been opened.
		/// </summary>
		/// <seealso cref="RibbonTabItemCloseUp"/>
		/// <seealso cref="RibbonTabItemOpening"/>
		/// <seealso cref="OnRibbonTabItemOpened"/>
		/// <seealso cref="RibbonTabItemOpenedEvent"/>
		//[Description("Occurs when the 'IsMinimized' of the XamRibbon is true and the popup for the 'SelectedTab' has been opened.")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler RibbonTabItemOpened
		{
			add
			{
				base.AddHandler(XamRibbon.RibbonTabItemOpenedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamRibbon.RibbonTabItemOpenedEvent, value);
			}
		}

			#endregion //RibbonTabItemOpened

			#region RibbonTabItemOpening

		/// <summary>
		/// Event ID for the <see cref="RibbonTabItemOpening"/> routed event
		/// </summary>
		/// <seealso cref="RibbonTabItemOpening"/>
		/// <seealso cref="OnRibbonTabItemOpening"/>
		public static readonly RoutedEvent RibbonTabItemOpeningEvent =
			EventManager.RegisterRoutedEvent("RibbonTabItemOpening", RoutingStrategy.Bubble, typeof(EventHandler<RibbonTabItemOpeningEventArgs>), typeof(XamRibbon));

		/// <summary>
		/// Used to raise the <see cref="RibbonTabItemOpening"/> event
		/// </summary>
		/// <param name="args">Event arguments for the event being raised</param>
		/// <seealso cref="RibbonTabItemOpening"/>
		/// <seealso cref="RibbonTabItemOpeningEvent"/>
		protected virtual void OnRibbonTabItemOpening(RibbonTabItemOpeningEventArgs args)
		{
			this.RaiseEvent(args);
		}

		private void RaiseRibbonTabItemOpening(RibbonTabItemOpeningEventArgs args)
		{
			args.RoutedEvent	= XamRibbon.RibbonTabItemOpeningEvent;
			args.Source			= this;

			this.OnRibbonTabItemOpening(args);
		}

		/// <summary>
		/// Occurs when the <see cref="IsMinimized"/> of the <see cref="XamRibbon"/> is true and the popup for the <see cref="SelectedTab"/> is being opened.
		/// </summary>
		/// <seealso cref="RibbonTabItemOpened"/>
		/// <seealso cref="RibbonTabItemCloseUp"/>
		/// <seealso cref="OnRibbonTabItemOpening"/>
		/// <seealso cref="RibbonTabItemOpeningEvent"/>
		//[Description("Occurs when the 'IsMinimized' of the XamRibbon is true and the popup for the 'SelectedTab' is being opened.")]
		//[Category("Ribbon Events")]
		public event EventHandler<RibbonTabItemOpeningEventArgs> RibbonTabItemOpening
		{
			add
			{
				base.AddHandler(XamRibbon.RibbonTabItemOpeningEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamRibbon.RibbonTabItemOpeningEvent, value);
			}
		}

			#endregion //RibbonTabItemOpening

			#region RibbonTabItemSelected

		/// <summary>
		/// Event ID for the <see cref="RibbonTabItemSelected"/> routed event
		/// </summary>
		/// <seealso cref="RibbonTabItemSelected"/>
		/// <seealso cref="OnRibbonTabItemSelected"/>
		public static readonly RoutedEvent RibbonTabItemSelectedEvent =
			EventManager.RegisterRoutedEvent("RibbonTabItemSelected", RoutingStrategy.Bubble, typeof(EventHandler<RibbonTabItemSelectedEventArgs>), typeof(XamRibbon));

		/// <summary>
		/// Used to raise the <see cref="RibbonTabItemSelected"/> event
		/// </summary>
		/// <param name="args">Event arguments for the event being raised</param>
		/// <seealso cref="RibbonTabItemSelected"/>
		/// <seealso cref="RibbonTabItemSelectedEvent"/>
		protected virtual void OnRibbonTabItemSelected(RibbonTabItemSelectedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		private void RaiseRibbonTabItemSelected(RibbonTabItemSelectedEventArgs args)
		{
			args.RoutedEvent = XamRibbon.RibbonTabItemSelectedEvent;
			args.Source = this;

			this.OnRibbonTabItemSelected(args);
		}

		/// <summary>
		/// Occurs after a <see cref="RibbonTabItem"/> has been selected.
		/// </summary>
		/// <seealso cref="SelectedTab"/>
		/// <seealso cref="OnRibbonTabItemSelected"/>
		/// <seealso cref="RibbonTabItemSelectedEvent"/>
		//[Description("Occurs after a RibbonTabItem has been selected.")]
		//[Category("Ribbon Events")]
		public event EventHandler<RibbonTabItemSelectedEventArgs> RibbonTabItemSelected
		{
			add
			{
				base.AddHandler(XamRibbon.RibbonTabItemSelectedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamRibbon.RibbonTabItemSelectedEvent, value);
			}
		}

			#endregion //RibbonTabItemSelected

		#endregion Events

		#region ICommandHost Members

		// AS 2/5/08 ExecuteCommandInfo
		//bool ICommandHost.CanExecute(System.Windows.Input.RoutedCommand command, object commandParameter)
		bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;
			return command != null && command.OwnerType == typeof(RibbonCommands);
		}

		// SSP 3/18/10 TFS29783 - Optimizations
		// Changed CurrentState property to a method.
		// 
		//long ICommandHost.CurrentState
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			RibbonStates currentState = 0;

			FrameworkElement activeTool = this.ActiveItem;

			if ( activeTool != null )
			{
				currentState |= RibbonStates.ActiveItem;

				if ( RibbonToolHelper.GetIsOnQat( activeTool ) )
					currentState |= RibbonStates.ActiveItemOnQat;

				if ( XamRibbon.GetLocation( activeTool ) == ToolLocation.QuickAccessToolbar )
					currentState |= RibbonStates.ActiveItemDirectlyOnQat;
			}


			if ( this.AllowMinimize )
				currentState |= RibbonStates.RibbonCanMinimize;


			return (long)currentState & statesToQuery;
		}

		// AS 2/5/08 ExecuteCommandInfo
		//void ICommandHost.Execute(System.Windows.Input.ExecutedRoutedEventArgs args)
		bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
		{
			// AS 2/5/08 ExecuteCommandInfo
			//RoutedCommand command = args.Command as RoutedCommand;
			//if (command != null)
			//    args.Handled = this.ExecuteCommandImpl(command, args.Parameter);
			RoutedCommand command = commandInfo.RoutedCommand;
			return null != command && this.ExecuteCommandImpl(command, commandInfo.Parameter);
		}

		#endregion

		#region IKeyTipContainer

		void IKeyTipContainer.Deactivate()
		{
			// do nothing
		}

		IKeyTipProvider[] IKeyTipContainer.GetKeyTipProviders()
		{
			List<IKeyTipProvider> providers = new List<IKeyTipProvider>();

			// include all tabs so autogenerated keytips don't change
			foreach (RibbonTabItem tab in this.Tabs)
				providers.Add(tab);

			foreach(ContextualTabGroup group in this.ContextualTabGroups)
				foreach (RibbonTabItem tab in group.Tabs)
					providers.Add(tab);

			QuickAccessToolbar qat = this.QuickAccessToolbar;

			if (qat != null)
				providers.AddRange(qat.GetKeyTipProviders(false));

			ApplicationMenu menu = this.ApplicationMenu;

			if (null != menu)
			{
				RibbonToolProxy proxy = ((IRibbonTool)menu).ToolProxy;

				if (null != proxy)
					providers.AddRange(proxy.GetKeyTipProviders(menu));
			}

			return providers.ToArray();
		}

		bool IKeyTipContainer.ReuseParentKeyTips
		{
			get { return false; }
		}

		#endregion //IKeyTipContainer

		#region TabControlPopupManager
		private class TabControlPopupManager : IRibbonPopupOwner
		{
			#region Member Variables

			private XamRibbon _ribbon;
			private PopupOwnerProxy _popupProxy;

			#endregion //Member Variables

			#region Constructor
			internal TabControlPopupManager(XamRibbon ribbon)
			{
				this._ribbon = ribbon;
				this._popupProxy = new PopupOwnerProxy(this);

				this._ribbon.RibbonTabControl.DropDownOpened += new RoutedEventHandler(OnRibbonTabControlDropDownOpened);
				this._ribbon.RibbonTabControl.DropDownClosed += new RoutedEventHandler(OnRibbonTabControlDropDownClosed);

				this._popupProxy.Initialize(this._ribbon.RibbonTabControl);
			}

			#endregion //Constructor

			#region IRibbonPopupOwner Members

			XamRibbon IRibbonPopupOwner.Ribbon
			{
				get { return this._ribbon; }
			}

			bool IRibbonPopupOwner.CanOpen
			{
				get { return this._ribbon.RibbonTabControl.IsMinimized; }
			}

			bool IRibbonPopupOwner.IsOpen
			{
				get
				{
					return this._ribbon.RibbonTabControl.IsDropDownOpen;
				}
				set
				{
					this._ribbon.RibbonTabControl.IsDropDownOpen = value;
				}
			}

			bool IRibbonPopupOwner.FocusFirstItem()
			{
				RibbonTabItem tabToActivate = this._ribbon.ActiveItem as RibbonTabItem;

				if (tabToActivate == null)
					tabToActivate = this._ribbon.RibbonTabControl.SelectedItem as RibbonTabItem;

				if (null != tabToActivate)
				{
					// now try to focus an item in a ribbon group
					foreach (RibbonGroup group in tabToActivate.RibbonGroups)
					{
						if (group.IsCollapsed)
						{
							if (group.Focusable)
								return group.Focus();
						}
						else
						{
							if (PopupOwnerProxy.FocusFirstItem(group))
								return true;
						}
					}
				}

				return false;
			}

			UIElement IRibbonPopupOwner.PopupTemplatedParent
			{
				get
				{
					return this._ribbon.RibbonTabControl;
				}
			}

			bool IRibbonPopupOwner.HookKeyDown
			{
				get { return true; }
			}


			// AS 10/18/07
			FrameworkElement IRibbonPopupOwner.ParentElementToFocus
			{
				get { return this._ribbon.SelectedTab; }
			}

			#endregion

			#region Methods

			#region OnRibbonTabControlDropDownClosed
			void OnRibbonTabControlDropDownClosed(object sender, RoutedEventArgs e)
			{
				this._popupProxy.OnClose();

				// we only want the tab control to be focusable while it is dropped down so that 
				// we can get keyboard events
				this._ribbon.RibbonTabControl.Focusable = false;
			}
			#endregion //OnRibbonTabControlDropDownClosed

			#region OnRibbonTabControlDropDownOpened
			void OnRibbonTabControlDropDownOpened(object sender, RoutedEventArgs e)
			{
				// we only want the tab control to be focusable while it is dropped down so that 
				// we can get keyboard events
				this._ribbon.RibbonTabControl.Focusable = true;

				this._popupProxy.OnOpen();
			}
			#endregion //OnRibbonTabControlDropDownOpened

			#endregion //Methods
		} 
		#endregion //TabControlPopupManager
	}

	#region ToolsNotInRibbonCollection Class

	/// <summary>
	/// A modifiable collection of Tools that are not sited on a <see cref="RibbonGroup"/>, <see cref="ApplicationMenuFooterToolbar"/> or <see cref="MenuTool"/>.
	/// </summary>
	/// <seealso cref="XamRibbon.ToolsNotInRibbon"/>
	public class ToolsNotInRibbonCollection : ObservableCollectionExtended<FrameworkElement>
	{
		#region Member Variables

		private XamRibbon								_ribbon;

		#endregion //Member Variables

		#region Constructor

		internal ToolsNotInRibbonCollection(XamRibbon ribbon)
		{
			Debug.Assert(ribbon != null, "ribbon is null!");

			this._ribbon = ribbon;
		}

		#endregion //Constructor	
    
		#region Base Class Overrides

		// AS 11/16/07 BR28515
		// Updated this class to handle making the tools logical children of the ribbon
		// when they are added to the ToolsNotInRibbon collection. This will handle registering
		// them and will also allow them to get template updates and participate in command
		// eventing, raise tool events, etc. This is also more complete because the OnItemAdded
		// OnItemRemoved are called as each item is added/removed whereas the collection change
		// notification doesn't always give you the list of items affected - e.g. during a clear,
		// addrange, removerange, etc. Lastly, we'll flag the items in the collection in case
		// we need to know why they have a location of unknown.
		//
		/// <summary>
		/// Indicates to the base class that the <see cref="OnItemAdded(FrameworkElement)"/> and <see cref="OnItemRemoved(FrameworkElement)"/> methods should be invoked.
		/// </summary>
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Invoked when a tool is added to the collection.
		/// </summary>
		/// <param name="itemAdded">The item that was added</param>
		protected override void OnItemAdded(FrameworkElement itemAdded)
		{
			base.OnItemAdded(itemAdded);

			if (null != itemAdded)
			{
				itemAdded.SetValue(XamRibbon.IsInToolsNotInRibbonProperty, KnownBoxes.TrueBox);
				this._ribbon.AddLogicalChildInternal(itemAdded);
			}
		}

		/// <summary>
		/// Invoked when a tool is removed from the collection.
		/// </summary>
		/// <param name="itemRemoved">The item that was removed</param>
		protected override void OnItemRemoved(FrameworkElement itemRemoved)
		{
			base.OnItemRemoved(itemRemoved);

			if (null != itemRemoved)
			{
				itemRemoved.ClearValue(XamRibbon.IsInToolsNotInRibbonProperty);
				this._ribbon.RemoveLogicalChildInternal(itemRemoved);
			}
		}

		#endregion //Base Class Overrides
	}

	#endregion //ToolsNotInRibbonCollection Class
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