using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Specialized;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Ribbon.Internal;
using Infragistics.Windows.Ribbon.Events;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Infragistics.Shared;
using System.Windows.Input;
using System.Collections;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Internal;
using Infragistics.Collections;
using System.Windows.Threading;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents a logical and visual grouping of tools within a <see cref="RibbonTabItem"/> on a <see cref="XamRibbon"/> control.
	/// </summary>
	/// <remarks>
	/// <p class="body">The <see cref="ItemsControl.Items"/> property of the RibbonGroup is designed to contain 
	/// FrameworkElement-derived tools or any of several RibbonGroup layout panels including <see cref="ToolHorizontalWrapPanel"/> 
	/// and <see cref="ToolVerticalWrapPanel"/>.</p>
	/// <p class="body">The RibbonGroup exposes several properties that allow you to control how the contents 
	/// are resized. The <see cref="Variants"/> property is used to provide a list of <see cref="GroupVariant"/> 
	/// instances that determines what type of resize operations are allowed for the group - as well as when it 
	/// should be resized with respect to other groups in the owning <see cref="RibbonTabItem"/>. The group 
	/// also exposes 2 attached properties (<see cref="MaximumSizeProperty"/> and <see cref="MinimumSizeProperty"/>) 
	/// that can be used to control the maximum and minimum sizes respectively of the contained tools.</p>
	/// <p class="body">The <see cref="DialogBoxLauncherTool"/> property is used to provide a <see cref="ButtonTool"/> 
	/// instance that will be displayed in the caption area of the group adjacent to the <see cref="Caption"/>.</p>
	/// </remarks>
	/// <seealso cref="RibbonTabItem"/>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="ToolHorizontalWrapPanel"/>
	/// <seealso cref="ToolVerticalWrapPanel"/>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,           GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,            GroupName = VisualStateUtilities.GroupCommon)]
    // JJD 5/20/10 - TFS32570 - added Highlight state
    [TemplateVisualState(Name = VisualStateUtilities.StateHighlight,           GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateActive,              GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,            GroupName = VisualStateUtilities.GroupActive)]

    [TemplateVisualState(Name = VisualStateUtilities.StateContextual,          GroupName = VisualStateUtilities.GroupContextual)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNonContextual,       GroupName = VisualStateUtilities.GroupContextual)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateExpanded,            GroupName = VisualStateUtilities.GroupExpansion)]
    [TemplateVisualState(Name = VisualStateUtilities.StateCollapsed,           GroupName = VisualStateUtilities.GroupExpansion)]
    
    // JJD 5/20/10 - TFS32570 - added Focus group
    [TemplateVisualState(Name = VisualStateUtilities.StateFocused,             GroupName = VisualStateUtilities.GroupFocus)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFocusedDropDown,     GroupName = VisualStateUtilities.GroupFocus)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfocused,           GroupName = VisualStateUtilities.GroupFocus)]

    [TemplateVisualState(Name = VisualStateUtilities.StateRibbon,              GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateQAT,                 GroupName = VisualStateUtilities.GroupLocation)]

	[TemplatePart(Name = "PART_Caption", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_Header", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_RibbonGroupSite", Type = typeof(ContentControl))]
	[TemplatePart(Name = "PART_RibbonGroupButton", Type = typeof(DropDownToggle))]
	[TemplatePart(Name = "PART_PopupRibbonGroupSite", Type = typeof(ContentControl))]
	[TemplatePart(Name = "PART_Items", Type = typeof(ItemsPresenter))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RibbonGroup : ItemsControl, IRibbonToolLocation,
		IKeyTipProvider,
		IKeyTipContainer,
		IRibbonPopupOwner
	{
		#region Member Variables

		private XamRibbon									_ribbon = null;
		private ObservableCollectionExtended<GroupVariant>	_variants = null;
		private FrameworkElement							_captionElement;
		private FrameworkElement							_headerElement;
		private List<FrameworkElement>						_clonedTools;
		private ItemsPresenter								_itemsPresenter;
		private ContentControl								_groupPresenterCollapsed;
		private ContentControl								_groupPresenterNormal;
		private DropDownToggle								_groupButton;

		private List<IRibbonToolPanel>						_registeredPanels;
		private List<MenuTool>								_registeredMenus;
		private List<IRibbonTool>							_registeredTools;

		// AS 10/8/07 PopupOwnerProxy
		//private bool										_restoreDocumentFocusOnClose;
		private PopupOwnerProxy								_popupOwnerProxy;

		// AS 11/1/07 BR27469
		private int											_itemsVersion;


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="RibbonGroup"/> class.
		/// </summary>
		public RibbonGroup()
		{
			this._registeredPanels = new List<IRibbonToolPanel>();
			this._registeredMenus = new List<MenuTool>();
			this._registeredTools = new List<IRibbonTool>();
		}

		static RibbonGroup()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGroup), new FrameworkPropertyMetadata(typeof(RibbonGroup)));

			ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(ToolVerticalWrapPanel)));
			template.Seal();
			ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(RibbonGroup), new FrameworkPropertyMetadata(template));

			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(RibbonGroup), new FrameworkPropertyMetadata(new Style()));
			XamRibbon.RibbonProperty.OverrideMetadata(typeof(RibbonGroup), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRibbonChanged)));
			XamRibbon.LocationPropertyKey.OverrideMetadata(typeof(RibbonGroup), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLocationChanged)));

			XamRibbon.IsActivePropertyKey.OverrideMetadata(typeof(RibbonGroup), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsActiveChanged)));
			EventManager.RegisterClassHandler(typeof(RibbonGroup), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(RibbonGroup.OnRequestBringIntoView));

            // JJD 5/20/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(RibbonGroup), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		#endregion //Constructor

		#region Base Class Overrides

			// AS 8/16/11 TFS82301
			#region GetContainerForItemOverride
		/// <summary>
		/// Used to obtain the element that is used to display items in the control.
		/// </summary>
		/// <returns>A ContentPresenter</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ItemContentPresenter();
		} 
			#endregion //GetContainerForItemOverride

			#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				// AS 10/11/07
				// This tool is in the logical children but we were not including it so its Ribbon 
				// property was null.
				//
				if (this.DialogBoxLauncherTool != null)
					return new MultiSourceEnumerator(new IEnumerator[] { base.LogicalChildren, new SingleItemEnumerator(this.DialogBoxLauncherTool) });

				return base.LogicalChildren;
			}
		}

			#endregion //LogicalChildren
		
			#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the element has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			this._captionElement = this.GetTemplateChild("PART_Caption") as FrameworkElement;
			this._headerElement = this.GetTemplateChild("PART_Header") as FrameworkElement;
			this._itemsPresenter = this.GetTemplateChild("PART_Items") as ItemsPresenter;
			this._groupPresenterCollapsed = this.GetTemplateChild("PART_PopupRibbonGroupSite") as ContentControl;
			this._groupPresenterNormal = this.GetTemplateChild("PART_RibbonGroupSite") as ContentControl;
			this._groupButton = this.GetTemplateChild("PART_RibbonGroupButton") as DropDownToggle;
			
			// synchronize the IsActive property
			if ( this._groupButton != null )
			{
				this._groupButton.SetValue(XamRibbon.IsActivePropertyKey, this.GetValue(XamRibbon.IsActiveProperty));
				this._groupButton.SetValue(RibbonGroup.IsInContextualTabGroupPropertyKey, this.GetValue(IsInContextualTabGroupProperty));
			}

			// make sure the height of the group is synchronized with the other groups
			this.SynchronizeItemsHeight();

			// AS 6/18/08 BR33990
			this.UpdateHighlightContentBindings(this.IsDropDown ? this._groupPresenterCollapsed : this._groupPresenterNormal);

			base.OnApplyTemplate();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        } 
			#endregion //OnApplyTemplate

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="RibbonGroup"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.RibbonGroupAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new RibbonGroupAutomationPeer(this);
        }
            #endregion

            #region OnKeyDown

		/// <summary>
		/// Called when the element has input focus and a key is pressed.
		/// </summary>
		/// <param name="e">An instance of KeyEventArgs that contains information about the key that was pressed.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			#region Commented out
			
#region Infragistics Source Cleanup (Region)






























































































































#endregion // Infragistics Source Cleanup (Region)

			#endregion //Commented out
			switch (e.Key)
			{
				case Key.Up:
					{
						#region Up
						XamRibbon ribbon = this.Ribbon;

						// if a tool within the ribbon group is active and the up arrow is pressed, make
						// sure that our active tab gets focus if another tab would have gotten focus
						FrameworkElement activeItem = null != ribbon ? ribbon.ActiveItem : null;

						if (this.IsOpen == false && null != activeItem && this.IsAncestorOf(activeItem))
						{
							UIElement focusedElement = Keyboard.FocusedElement as UIElement;
							RibbonTabItem parentTab = this.Parent as RibbonTabItem;

							if (null != focusedElement && parentTab != null)
							{
								DependencyObject elementAbove = focusedElement.PredictFocus(FocusNavigationDirection.Up);

								// if the up arrow would take focus out of the tab to another tab or some 
								// other element then focus the parent tab instead
								if (elementAbove != parentTab && elementAbove != null && this.IsAncestorOf(elementAbove) == false)
								{
									parentTab.Focus();
									e.Handled = true;
								}
							}
						}
						break;
						#endregion //Up		
					}
			}


			this.PopupOwnerProxy.ProcessKeyDown(e);
		}

			#endregion //OnKeyDown	

			#region OnIsKeyboardFocusWithinChanged
		/// <summary>
		/// Invoked when the value of the <see cref="UIElement.IsKeyboardFocusWithin"/> property changes.
		/// </summary>
		/// <param name="e">Provides information about the property change</param>
		protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			base.OnIsKeyboardFocusWithinChanged(e);


            // JJD 5/20/10 - TFS32570
            // Update the visual states when the IsCollapsed state changes
            this.UpdateVisualStates();

        } 
			#endregion //OnIsKeyboardFocusWithinChanged

			#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="e">Contains information about the items that were changed.</param>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

            // AS 2/5/09 TFS11796
            // Changed to a helper method since we don't always want to bump it.
            //
            //// AS 11/1/07 BR27469
            //this._itemsVersion++;
            this.BumpItemsVersion();

			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		}

			#endregion //OnItemsChanged
    
			#region OnMouseEnter

		/// <summary>
		/// Raised when the mouse enters the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);


            // JJD 5/20/10 - NA2010 Vol 2 - Added support for VisualStateManager
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


            // JJD 5/20/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

		}

			#endregion //OnMouseLeave	

			#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);
		}

			#endregion //PrepareContainerForItemOverride	
    
			#region ToString


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			#endregion //ToString

		#endregion //Base Class Overrides

		#region Properties

			#region Attached Properties

				#region ContainingGroup

		internal static readonly DependencyPropertyKey ContainingGroupPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ContainingGroup",
			typeof(RibbonGroup), typeof(RibbonGroup), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContainingGroupChanged)));

		private static void OnContainingGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// we need to keep track of the menus/galleries and panels in each ribbon group
			RibbonGroup oldGroup = (RibbonGroup)e.OldValue;
			RibbonGroup newGroup = (RibbonGroup)e.NewValue;

			if (oldGroup != null)
			{
				if (d is IRibbonToolPanel)
					oldGroup._registeredPanels.Remove((IRibbonToolPanel)d);
				else if (d is MenuTool)
					oldGroup._registeredMenus.Remove((MenuTool)d);

				if (d is IRibbonTool)
					oldGroup._registeredTools.Remove((IRibbonTool)d);

                // AS 2/5/09 TFS11796
                // Changed to a helper method since we don't always want to bump it.
                //
                //// AS 11/1/07 BR27469
				//oldGroup._itemsVersion++;
                oldGroup.BumpItemsVersion();
			}

			if (newGroup != null)
			{
				if (d is IRibbonToolPanel)
					newGroup._registeredPanels.Add((IRibbonToolPanel)d);
				else if (d is MenuTool)
					newGroup._registeredMenus.Add((MenuTool)d);

				if (d is IRibbonTool)
					newGroup._registeredTools.Add((IRibbonTool)d);

                // AS 2/5/09 TFS11796
                // Changed to a helper method since we don't always want to bump it.
                //
                //// AS 11/1/07 BR27469
				//newGroup._itemsVersion++;
                newGroup.BumpItemsVersion();
			}

			RibbonGroup group = newGroup ?? oldGroup;

            // AS 2/5/09 TFS11796
            // We don't need to dirty the version of the ribbon's group panel
            // unless we are on the ribbon - i.e. we don't have to for the qat.
            //
			//if (group != null)
            // AS 2/10/09 TFS11796
            // Modification of the fix above. Apparantly we don't set the location 
            // for the ribbon group's within a tab item so instead check for not 
            // being in the qat.
            //
            //if (group != null && XamRibbon.GetLocation(group) == ToolLocation.Ribbon)
			if (group != null && XamRibbon.GetLocation(group) != ToolLocation.QuickAccessToolbar)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(group);

				if (null != ribbon)
					ribbon.BumpRibbonGroupSizeVersion();
			}
		}

		/// <summary>
		/// Identifies the ContainingGroup" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetContainingGroup"/>
		internal static readonly DependencyProperty ContainingGroupProperty =
			ContainingGroupPropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'ContainingGroup' attached readonly property
		/// </summary>
		/// <seealso cref="ContainingGroupProperty"/>
		internal static RibbonGroup GetContainingGroup(DependencyObject d)
		{
			return (RibbonGroup)d.GetValue(RibbonGroup.ContainingGroupProperty);
		}

				#endregion //ContainingGroup

				#region IsDialogBoxLauncherTool

		internal static readonly DependencyPropertyKey IsDialogBoxLauncherToolPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsDialogBoxLauncherTool",
			typeof(bool), typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the IsDialogBoxLauncherTool" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsDialogBoxLauncherTool"/>
		/// <seealso cref="DialogBoxLauncherTool"/>
		/// <seealso cref="DialogBoxLauncherToolProperty"/>
		public static readonly DependencyProperty IsDialogBoxLauncherToolProperty =
			IsDialogBoxLauncherToolPropertyKey.DependencyProperty;


		/// <summary>
		/// Indicates if the specified tool instance represents the <see cref="DialogBoxLauncherTool"/> for a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This method will return true for a <see cref="ButtonTool"/> that has been 
		/// set as the <see cref="DialogBoxLauncherTool"/> for a <see cref="RibbonGroup"/>.</p>
		/// </remarks>
		/// <seealso cref="IsDialogBoxLauncherToolProperty"/>
		/// <seealso cref="DialogBoxLauncherTool"/>
		/// <seealso cref="DialogBoxLauncherToolProperty"/>
		public static bool GetIsDialogBoxLauncherTool(DependencyObject d)
		{
			return (bool)d.GetValue(RibbonGroup.IsDialogBoxLauncherToolProperty);
		}

				#endregion //IsDialogBoxLauncherTool

				#region MaximumSize

		/// <summary>
		/// Identifies the MaximumSize attached dependency property which determines the maximum size of a tool when it appears in a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <seealso cref="GetMaximumSize"/>
		/// <seealso cref="SetMaximumSize"/>
		/// <seealso cref="GetMinimumSize"/>
		/// <seealso cref="SetMinimumSize"/>
		/// <seealso cref="Variants"/>
		public static readonly DependencyProperty MaximumSizeProperty = DependencyProperty.RegisterAttached("MaximumSize",
			typeof(RibbonToolSizingMode), typeof(RibbonGroup), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox, FrameworkPropertyMetadataOptions.AffectsParentMeasure,
				// AS 5/27/08 BR31372
				new PropertyChangedCallback(OnMinMaxSizeChanged)));

		private static void OnMinMaxSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 5/27/08 BR31372
			// If the tool is within a visible ribbon group then we need to dirty the 
			// layout of the containing RibbonGroupPanel.
			//
			RibbonGroup containingGroup = GetContainingGroup(d);

			if (containingGroup != null && containingGroup.IsVisible)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(d);

				if (null != ribbon)
					ribbon.BumpRibbonGroupSizeVersion();
			}
		}

		/// <summary>
		/// Gets the value of the MaximumSize attached dependency property which determines the 
		/// maximum size of a tool when it appears in a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The MaximumSize is used to determine the preferred size for tools within the ribbon group. This represents the 
		/// largest size to which a tool within the <see cref="RibbonGroup"/> may be sized. A value of <b>ImageAndTextLarge</b> indicates that 
		/// the tool should span the full height of the containing RibbonGroup. A value of <b>ImageAndTextNormal</b> is used to indicate that 
		/// the tool should only occupy about 1/3 the height of the RibbonGroup but display its caption as well as its image by default. A value of 
		/// <b>ImageOnly</b> is used to indicate that the tool should never display its caption.</p>
		/// <p class="body">The default value for this property may be different for different tool types. For example, the default MaximumSize for a 
		/// <see cref="MenuTool"/> is <b>ImageAndTextLarge</b> but the default size for a <see cref="ButtonTool"/> is <b>ImageAndTextNormal</b>.</p>
		/// <p class="note"><b>Note:</b> The maximum size may not be honored by all panels. For example, tools within a 
		/// <see cref="ButtonGroup"/> will never use a maximum size of <b>ImageAndTextLarge</b> since that panel is 
		/// designed to arrange one or more tools in a small island within a <see cref="RibbonGroup"/> similar to that of 
		/// the tools within the Font group of Microsoft Word's Home tab.</p>
		/// </remarks>
		/// <seealso cref="MaximumSizeProperty"/>
		/// <seealso cref="SetMaximumSize"/>
		/// <seealso cref="MinimumSizeProperty"/>
		/// <seealso cref="GetMinimumSize"/>
		/// <seealso cref="Variants"/>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static RibbonToolSizingMode GetMaximumSize(DependencyObject d)
		{
			return (RibbonToolSizingMode)d.GetValue(RibbonGroup.MaximumSizeProperty);
		}

		/// <summary>
		/// Sets the value of the MaximumSize attached dependency property which determines the maximum size of a tool when it appears in a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <seealso cref="MaximumSizeProperty"/>
		/// <seealso cref="GetMaximumSize"/>
		/// <seealso cref="MinimumSizeProperty"/>
		/// <seealso cref="SetMinimumSize"/>
		/// <seealso cref="Variants"/>
		public static void SetMaximumSize(DependencyObject d, RibbonToolSizingMode value)
		{
			d.SetValue(RibbonGroup.MaximumSizeProperty, RibbonKnownBoxes.FromValue(value));
		}

				#endregion //MaximumSize

				#region MinimumSize

		/// <summary>
		/// Identifies the MinimumSize attached dependency property which determines the maximum size of a tool when it appears in a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <seealso cref="GetMinimumSize"/>
		/// <seealso cref="SetMinimumSize"/>
		/// <seealso cref="GetMaximumSize"/>
		/// <seealso cref="SetMaximumSize"/>
		/// <seealso cref="Variants"/>
		public static readonly DependencyProperty MinimumSizeProperty = DependencyProperty.RegisterAttached("MinimumSize",
			typeof(RibbonToolSizingMode), typeof(RibbonGroup), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox, FrameworkPropertyMetadataOptions.AffectsParentMeasure,
				// AS 5/27/08 BR31372
				new PropertyChangedCallback(OnMinMaxSizeChanged)));

		/// <summary>
		/// Gets the value of the MinimumSize attached dependency property which determines the maximum size of a tool when it appears in a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The MinimumSize is used in conjunction with the <see cref="MaximumSizeProperty"/> and the <see cref="Variants"/> when determining the 
		/// size of tools within the <see cref="RibbonGroup"/>. The MinimumSize indicates the minimum size to which a tool may be reduced. A value of 
		/// <b>ImageAndTextLarge</b> is used to ensure that the tool is always displayed as a large tool - that is that the tool always spans the full height 
		/// of the <see cref="RibbonGroup"/>. A value of <b>ImageAndTextNormal</b> is used to ensure that the caption of the tool is always displayed. A value of 
		/// <b>ImageOnly</b> is used to indicate that the tool can be resized such that the caption is hidden; this is often used when the Image of the tool 
		/// provides enough context for the end user to identify the functionality of the tool.</p>
		/// <p class="body">This value is primarily used when a <see cref="GroupVariant"/> from the group's <see cref="Variants"/> collection is processed and 
		/// that GroupVariant has a <see cref="GroupVariant.ResizeAction"/> of <b>ReduceImageAndTextLargeTools</b> or <b>ReduceImageAndTextNormalTools</b>.</p>
		/// </remarks>
		/// <seealso cref="MinimumSizeProperty"/>
		/// <seealso cref="SetMinimumSize"/>
		/// <seealso cref="MaximumSizeProperty"/>
		/// <seealso cref="GetMaximumSize"/>
		/// <seealso cref="Variants"/>
		[AttachedPropertyBrowsableWhenAttributePresent(typeof(RibbonToolAttribute))]
		public static RibbonToolSizingMode GetMinimumSize(DependencyObject d)
		{
			return (RibbonToolSizingMode)d.GetValue(RibbonGroup.MinimumSizeProperty);
		}

		/// <summary>
		/// Sets the value of the MinimumSize attached dependency property which determines the maximum size of a tool when it appears in a <see cref="RibbonGroup"/>.
		/// </summary>
		/// <seealso cref="MinimumSizeProperty"/>
		/// <seealso cref="GetMinimumSize"/>
		/// <seealso cref="MaximumSizeProperty"/>
		/// <seealso cref="SetMaximumSize"/>
		/// <seealso cref="Variants"/>
		public static void SetMinimumSize(DependencyObject d, RibbonToolSizingMode value)
		{
			d.SetValue(RibbonGroup.MinimumSizeProperty, RibbonKnownBoxes.FromValue(value));
		}

				#endregion //MinimumSize

				// JM 06-01-10 TFS32650
				#region ReplacedByGroup

		internal static readonly DependencyPropertyKey ReplacedByGroupPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ReplacedByGroup",
			typeof(RibbonGroup), typeof(RibbonGroup), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the ReplacedByGroup" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetReplacedByGroup"/>
		internal static readonly DependencyProperty ReplacedByGroupProperty =
			ReplacedByGroupPropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'ReplacedByGroup' attached readonly property
		/// </summary>
		/// <seealso cref="ReplacedByGroupProperty"/>
		internal static RibbonGroup GetReplacedByGroup(DependencyObject d)
		{
			return (RibbonGroup)d.GetValue(RibbonGroup.ReplacedByGroupProperty);
		}

				#endregion //ReplacedByGroup

			#endregion //Attached Properties

			#region Public Properties

				#region Caption

		/// <summary>
		/// Identifies the Caption dependency property.
		/// </summary>
		/// <seealso cref="Caption"/>
		public static readonly DependencyProperty CaptionProperty = RibbonToolHelper.CaptionProperty.AddOwner(typeof(RibbonGroup));

		/// <summary>
		/// Returns/sets the caption associated with the RibbonGroup.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Caption is the text that is displayed within the header area of the RibbonGroup and also in the button 
		/// that represents the group when <see cref="IsCollapsed"/> is true. If the <see cref="DialogBoxLauncherTool"/> has been 
		/// specified, it will be displayed adjacent to the caption within the header area.</p>
		/// </remarks>
		/// <seealso cref="CaptionProperty"/>
		/// <seealso cref="DialogBoxLauncherTool"/>
		//[Description("Returns/sets the caption associated with the RibbonGroup.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Caption
		{
			get
			{
				return (string)this.GetValue(RibbonGroup.CaptionProperty);
			}
			set
			{
				this.SetValue(RibbonGroup.CaptionProperty, value);
			}
		}

				#endregion //Caption

				#region CollapsedGroupButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the <see cref="System.Windows.Controls.Primitives.ToggleButton"/> that represents the <see cref="RibbonGroup"/> within the <see cref="RibbonTabItem"/> when it is collapsed.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="ControlTemplate"/> targeting a <see cref="DropDownToggle"/>. The template is used when the <see cref="IsCollapsed"/> property 
		/// of a <see cref="RibbonGroup"/> within the <see cref="RibbonTabItem"/> is true.</p>
		/// </remarks>
		/// <seealso cref="IsCollapsed"/>
		public static readonly ResourceKey CollapsedGroupButtonStyleKey = new StaticPropertyResourceKey(typeof(RibbonGroup), "CollapsedGroupButtonStyleKey");

				#endregion //CollapsedGroupButtonStyleKey

				// AS 10/30/07
				// For styling purposes, the button that represents a contextual tab group needed to be able to 
				// get to the contextual tab group containing the group so that it can get the basebackcolor.
				//
				#region ContextualTabGroup

		internal static readonly DependencyPropertyKey ContextualTabGroupPropertyKey =
			DependencyProperty.RegisterReadOnly("ContextualTabGroup",
			typeof(ContextualTabGroup), typeof(RibbonGroup), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContextualTabGroupChanged)));

		private static void OnContextualTabGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = d as RibbonGroup;


			// JM BR28878 12-04-07 - Solved this problem a different way - i.e. by checking for IsInDesignMode in the conditions of the RibbonGroup template's
			//                        MultiTrigger that was causing the problem
			//// JM BR28655 11-29-07 - Setting the IsInContextualTabGroup property right here at design time gives VS and Blend indigestion.  Doing
			//// it asynchronously makes them happy.
			//group.SetValue(IsInContextualTabGroupPropertyKey, KnownBoxes.FromValue(e.NewValue != null));
			//if (DesignerProperties.GetIsInDesignMode(group))
			//    group.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new XamRibbon.MethodInvoker(group.OnDeferSetIsInContextualTabGroup));
			//else
			// AS 1/7/08
			// There is a bug in the WPF framework that when evaluating the triggers of an element, they
			// don't always do null checks. In this case, they are looking at the multi triggers and one of 
			// the triggers is based on an element in the template and their condition node is null. If we
			// force the template to be applied/hydrated then this node is available and the null ref 
			// exception they would have generated can be avoided.
			//
			//	group.SetValue(IsInContextualTabGroupPropertyKey, KnownBoxes.FromValue(e.NewValue != null));
			bool isInGroup = e.NewValue != null;
			if (group.IsInContextualTabGroup != isInGroup)
			{
				// don't bother doing this while we are initializing since the issue only seems to happen
				// when initializing them in code
				if (group.IsInitialized)
					group.ApplyTemplate();
				group.SetValue(IsInContextualTabGroupPropertyKey, KnownBoxes.FromValue(isInGroup));
			}
		}

		// JM BR28878 12-04-07 - Solved this problem a different way - i.e. by checking for IsInDesignMode in the conditions of the RibbonGroup template's
		//                        MultiTrigger that was causing the problem
		//// JM BR28655 11-29-07 - Called asynchronously from OnCOntextualTabGroupChanged.
		//private void OnDeferSetIsInContextualTabGroup()
		//{
		//    this.SetValue(RibbonGroup.IsInContextualTabGroupPropertyKey, KnownBoxes.FromValue(this.ContextualTabGroup != null));
		//}

		/// <summary>
		/// Identifies the <see cref="ContextualTabGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContextualTabGroupProperty =
			ContextualTabGroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the owning ContextualTabGroup
		/// </summary>
		/// <p class="body">The ContextualTabGroup property returns the <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/> instance that contains the <see cref="RibbonTabItem"/> 
		/// that contains this <see cref="RibbonGroup"/> instance or null (Nothing in VB) if the containing RibbonTabItem does not below to a 
		/// contextual tab group. The <see cref="IsInContextualTabGroup"/> property will return true, if the ContextualTabGroup property is set.</p>
		/// <seealso cref="ContextualTabGroupProperty"/>
		/// <seealso cref="IsInContextualTabGroup"/>
		public ContextualTabGroup ContextualTabGroup
		{
			get
			{
				return (ContextualTabGroup)this.GetValue(RibbonGroup.ContextualTabGroupProperty);
			}
		}

				#endregion //ContextualTabGroup

				#region DialogBoxLauncherTool

		/// <summary>
		/// Identifies the <see cref="DialogBoxLauncherTool"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DialogBoxLauncherToolProperty = DependencyProperty.Register("DialogBoxLauncherTool",
			typeof(ButtonTool), typeof(RibbonGroup), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDialogBoxLauncherToolChanged), new CoerceValueCallback(OnCoerceDialogBoxLauncherTool)));

		private static object OnCoerceDialogBoxLauncherTool(DependencyObject d, object newValue)
		{
			RibbonGroup group = d as RibbonGroup;
			DependencyObject newTool = newValue as DependencyObject;

			if (group != null && newTool != null)
			{
				// if this is a dialog box launcher for another ribbon group...
				if (RibbonGroup.GetIsDialogBoxLauncherTool(newTool))
					throw new InvalidOperationException(XamRibbon.GetString("LE_ToolIsAlreadyDialogBoxLauncherTool"));
			}

			return newValue;
		}

		private static void OnDialogBoxLauncherToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = d as RibbonGroup;

			if (group != null)
			{
				ButtonTool oldTool = (ButtonTool)e.OldValue;
				ButtonTool newTool = (ButtonTool)e.NewValue;

				if (null != oldTool)
				{
					group.RemoveLogicalChild(oldTool);
					oldTool.SetValue(IsDialogBoxLauncherToolPropertyKey, KnownBoxes.FalseBox);
				}

				group.SetValue(HasDialogBoxLauncherToolPropertyKey, KnownBoxes.FromValue(newTool != null));

				if (null != newTool)
				{
					group.AddLogicalChild(newTool);
					newTool.SetValue(IsDialogBoxLauncherToolPropertyKey, KnownBoxes.TrueBox);
				}
			}
		}

		/// <summary>
		/// Returns/sets the tool that will be displayed within the caption area of the RibbonGroup.
		/// </summary>
		/// <remarks>
		/// <p class="body">The DialogBoxLauncherTool property is used to provide the <see cref="ButtonTool"/> 
		/// instance that will be displayed within the caption area. By default, the ButtonTool will be 
		/// use a custom <see cref="ControlTemplate"/> whose resource key is <see cref="DialogBoxLauncherToolTemplateKey"/>. The 
		/// ButtonTool instance set for this property will have the attached <see cref="IsDialogBoxLauncherToolProperty"/> 
		/// property set to true and the RibbonGroup's <see cref="HasDialogBoxLauncherTool"/> will be set to true.</p>
		/// </remarks>
		/// <seealso cref="DialogBoxLauncherToolProperty"/>
		/// <seealso cref="IsDialogBoxLauncherToolProperty"/>
		/// <seealso cref="HasDialogBoxLauncherTool"/>
		//[Description("Returns/sets the tool that will be displayed within the caption area of the RibbonGroup.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ButtonTool DialogBoxLauncherTool
		{
			get
			{
				return (ButtonTool)this.GetValue(RibbonGroup.DialogBoxLauncherToolProperty);
			}
			set
			{
				this.SetValue(RibbonGroup.DialogBoxLauncherToolProperty, value);
			}
		}

				#endregion //DialogBoxLauncherTool

				#region DialogBoxLauncherToolTemplateKey

		/// <summary>
		/// The key used to identify the <see cref="ControlTemplate"/> applied to the <see cref="DialogBoxLauncherTool"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="ControlTemplate"/> targeting a <see cref="ButtonTool"/>. The template is used for the <see cref="DialogBoxLauncherTool"/> 
		/// of a <see cref="RibbonGroup"/>.</p>
		/// </remarks>
		/// <seealso cref="DialogBoxLauncherTool"/>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateKey = new StaticPropertyResourceKey(typeof(RibbonGroup), "DialogBoxLauncherToolTemplateKey");

				#endregion //DialogBoxLauncherToolTemplateKey

				#region HasDialogBoxLauncherTool

		private static readonly DependencyPropertyKey HasDialogBoxLauncherToolPropertyKey =
			DependencyProperty.RegisterReadOnly("HasDialogBoxLauncherTool",
			typeof(bool), typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasDialogBoxLauncherTool"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasDialogBoxLauncherToolProperty =
			HasDialogBoxLauncherToolPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the <see cref="DialogBoxLauncherTool"/> has been set to a <see cref="ButtonTool"/> instance.
		/// </summary>
		/// <remarks>
		/// <p class="body">The HasDialogBoxLauncherTool property is used to indicate if the <see cref="DialogBoxLauncherTool"/> property has 
		/// been set to a <see cref="ButtonTool"/> instance. By default, this property returns false.</p>
		/// </remarks>
		/// <seealso cref="HasDialogBoxLauncherToolProperty"/>
		/// <seealso cref="DialogBoxLauncherTool"/>
		//[Description("Returns a boolean indicating whether the 'DialogBoxLauncherTool' has been set to a 'ButtonTool'.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HasDialogBoxLauncherTool
		{
			get
			{
				return (bool)this.GetValue(RibbonGroup.HasDialogBoxLauncherToolProperty);
			}
		}

				#endregion //HasDialogBoxLauncherTool

				// AS 6/18/08 BR33990
				#region HighlightContent

		private static readonly DependencyPropertyKey HighlightContentPropertyKey =
			DependencyProperty.RegisterReadOnly("HighlightContent",
			typeof(bool), typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

                // JJD 5/20/10 - NA2010 Vol 2 - Added support for VisualStateManager
                , new PropertyChangedCallback(OnVisualStatePropertyChanged)

                ));

		/// <summary>
		/// Identifies the <see cref="HighlightContent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HighlightContentProperty =
			HighlightContentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the content area of the group should be highlighted.
		/// </summary>
		/// <seealso cref="HighlightContentProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HighlightContent
		{
			get
			{
				return (bool)this.GetValue(RibbonGroup.HighlightContentProperty);
			}
		}

		private void InitializeHighlightContent()
		{
			bool highlight = true.Equals(this.GetValue(IsMouseOverContentProperty)) &&
				false.Equals(this.GetValue(IsMouseCaptureWithinContentProperty));

			this.SetValue(HighlightContentPropertyKey, KnownBoxes.FromValue(highlight));
		}
				#endregion //HighlightContent

				#region Id

		/// <summary>
		/// Identifies the <see cref="Id"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IdProperty = RibbonToolHelper.IdProperty.AddOwner(typeof(RibbonGroup), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIdChanged)));

		private static void OnIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = d as RibbonGroup;

			if (null != group && group.Ribbon != null)
			{
				group.Ribbon.OnRibbonGroupIdChanged(group, e.OldValue as string, e.NewValue as string);
			}
		}

		/// <summary>
		/// Returns/sets a string used to uniquely identify the <see cref="RibbonGroup"/> with the <see cref="XamRibbon"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Id property is used to uniquely identify a <see cref="RibbonGroup"/> within the <see cref="XamRibbon"/>. This is 
		/// primarily needed when the RibbonGroup is added to the <see cref="QuickAccessToolbar"/> and is used as the <see cref="QatPlaceholderTool.TargetId"/> 
		/// of the <see cref="QatPlaceholderTool"/> that represents the group. If you want to programatically add the RibbonGroup to the 
		/// QAT, then you must set this property to a string that is unique to all RibbonGroups across all RibbonTabItems within the Ribbon 
		/// and use that as the <see cref="QatPlaceholderTool.TargetId"/> of the <see cref="QatPlaceholderTool"/> that you add to the 
		/// <see cref="QuickAccessToolbar"/>.</p>
		/// </remarks>
		/// <seealso cref="IdProperty"/>
		//[Description("Returns/sets a string used to uniquely identify the 'RibbonGroup' with the 'XamRibbon'.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Id
		{
			get
			{
				return (string)this.GetValue(RibbonGroup.IdProperty);
			}
			set
			{
				this.SetValue(RibbonGroup.IdProperty, value);
			}
		}

				#endregion //Id

				#region IsActive

		/// <summary>
		/// Identifies the IsActive dependency property.
		/// </summary>
		/// <seealso cref="IsActive"/>
		public static readonly DependencyProperty IsActiveProperty = XamRibbon.IsActiveProperty.AddOwner(typeof(RibbonGroup));

		/// <summary>
		/// Returns true if the RibbonGroup is the current active item, otherwise returns false. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">A <see cref="RibbonGroup"/> can become the active item when using the keyboard to navigate the 
		/// ribbon and the RibbonGroup is in a state that it may be opened to display the contents of its <see cref="ItemsControl.Items"/> 
		/// property within a popup - that is when it is in the <see cref="QuickAccessToolbar"/> or when it is collapsed.</p>
		/// </remarks>
		/// <seealso cref="IsActiveProperty"/>
		//[Description("Returns true if the RibbonGroup is the current active item, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(RibbonGroup.IsActiveProperty);
			}
		}

				#endregion //IsActive

				#region IsCollapsed

		internal static readonly DependencyPropertyKey IsCollapsedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsCollapsed",
			typeof(bool), typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnIsCollapsedChanged)));

		private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = (RibbonGroup)d;

            if (group != null)
            {
                group.VerifyRibbonGroupSite();


                // JJD 5/20/10 - TFS32570
                // Update the visual states when the IsCollapsed state changes
                group.UpdateVisualStates();

            }
		}

		/// <summary>
		/// Identifies the <see cref="IsCollapsed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCollapsedProperty =
			IsCollapsedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the ribbon group is currently collapsed.
		/// </summary>
		/// <remarks>
		/// <p class="body">A RibbonGroup can become collapsed when there is not enough room to display the contents 
		/// of the RibbonGroup within the containing <see cref="RibbonTabItem"/> and the RibbonGroup is allowed to be 
		/// collapsed. A RibbonGroup indicates that it is allowed to be collapsed if it contains a <see cref="GroupVariant"/> 
		/// in its <see cref="Variants"/> collection that has a resize action of <b>CollapseRibbonGroup</b> or if all 
		/// RibbonGroups within the owning RibbonTabItem have no GroupVariants defined, in which case the default resizing 
		/// logic will be used which includes allowing the RibbonGroup to be collapsed.</p>
		/// <p class="note"><b>Note:</b> A RibbonGroup may only be collapsed when it is within a <see cref="RibbonTabItem"/>. A 
		/// RibbonGroup displayed within the <see cref="QuickAccessToolbar"/> is not considered to be collapsed and therefore this 
		/// property will return false for the RibbonGroup.</p>
		/// </remarks>
		/// <seealso cref="IsCollapsedProperty"/>
		//[Description("Returns a boolean indicating if the ribbon group is currently collapsed.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsCollapsed
		{
			get
			{
				return (bool)this.GetValue(RibbonGroup.IsCollapsedProperty);
			}
		}

				#endregion //IsCollapsed

				#region IsInContextualTabGroup

		internal static readonly DependencyPropertyKey IsInContextualTabGroupPropertyKey =
			DependencyProperty.RegisterReadOnly("IsInContextualTabGroup",
			typeof(bool), typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsInContextualTabGroupChanged)));

		private static void OnIsInContextualTabGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = (RibbonGroup)d;

            if (group._groupButton != null)
            {
                group._groupButton.SetValue(IsInContextualTabGroupPropertyKey, e.NewValue);

                // JJD 5/20/10 - NA2010 Vol 2 - Added support for VisualStateManager
                group.UpdateVisualStates();

            }
		}

		/// <summary>
		/// Identifies the <see cref="IsInContextualTabGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInContextualTabGroupProperty =
			IsInContextualTabGroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the <see cref="RibbonTabItem"/> to which the <see cref="RibbonGroup"/> belongs is part of a <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property returns true if the <see cref="RibbonTabItem"/> that contains the RibbonGroup belongs to a 
		/// <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/>. When true, the <see cref="ContextualTabGroup"/> property may be 
		/// used to access the containing ContextualTabGroup instance.</p>
		/// </remarks>
		/// <seealso cref="IsInContextualTabGroupProperty"/>
		/// <seealso cref="ContextualTabGroup"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/>
		//[Description("Returns a boolean indicating whether the 'RibbonTabItem' to which the 'RibbonGroup' belongs is part of a 'ContextualTabGroup'.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsInContextualTabGroup
		{
			get
			{
				return (bool)this.GetValue(RibbonGroup.IsInContextualTabGroupProperty);
			}
		}

				#endregion //IsInContextualTabGroup

				// AS 6/18/08 BR33990
				#region IsMouseCaptureWithinContent

		private static readonly DependencyProperty IsMouseCaptureWithinContentProperty = DependencyProperty.Register("IsMouseCaptureWithinContent",
			typeof(bool), typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnHighlightContentCriteriaChanged)));

				#endregion //IsMouseCaptureWithinContent

				// AS 6/18/08 BR33990
				#region IsMouseOverContent

		private static readonly DependencyProperty IsMouseOverContentProperty = DependencyProperty.Register("IsMouseOverContent",
			typeof(bool), typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnHighlightContentCriteriaChanged)));

		private static void OnHighlightContentCriteriaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((RibbonGroup)d).InitializeHighlightContent();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            ((RibbonGroup)d).UpdateVisualStates();

		}

				#endregion //IsMouseOverContent

				#region IsOpen

		/// <summary>
		/// Identifies the <see cref="IsOpen"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
			typeof(bool), typeof(RibbonGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsOpenChanged), new CoerceValueCallback(CoerceIsOpened)));

		private static object CoerceIsOpened(DependencyObject target, object value)
		{
			if (value is bool)
			{
				bool newValue = (bool)value;

				// we only need to raise the Opening event here if the IsOpened property is being set to true.
				// The after events will be raised in the OnIsOpenChanged callback
				if (newValue == true)
				{
					RibbonGroup group = target as RibbonGroup;

					// do not let it be set to true unless the group can be dropped down
					if (group != null)
					{
						if (group.IsDropDown == false)
							return KnownBoxes.FalseBox;

						// AS 11/1/07 BR27469
						// I moved this down since we should synchronize before raising the opening event.
						//
						//if ( group.RaiseOpeningEvent() == false )
						//	return KnownBoxes.FalseBox;

						// AS 11/1/07 BR27469
						// Changed multiple references in the next if block to use a local for the source group.
						//
						RibbonGroup sourceGroup = group.CloneFromGroup;

						if (sourceGroup != null)
						{
							// AS 11/1/07 BR27469
							// Previously we were clearing the items collection of the cloned group when 
							// it was closed but this causes a problem since the tools will be detached
							// from the ribbon and the tool may not have performed (or finished performing)
							// its action. So now we will just clear it on the opened if the items version
							// has changed.
							//
							if (group._itemsVersion != sourceGroup._itemsVersion)
							{
								// AS 11/1/07 BR27469
								// The cloned tools may not be null since we are only clearing in the open now.
								// Instead we need to clear the cloned tools here.
								//
								//Debug.Assert(group._clonedTools == null || group._clonedTools.Count == 0, "We still have tools in the collection that have not been removed!");
								if (group._clonedTools != null)
									group._clonedTools.Clear();

								group.Items.Clear();

								// clone the items collection
								group._clonedTools = group._clonedTools ?? new List<FrameworkElement>();
								IList<object> items = RibbonToolProxy.CloneItems(sourceGroup.Items, true, group._clonedTools);

								foreach (object item in items)
									group.Items.Add(item);

								// AS 11/1/07 BR27469
								// Synchronize the versions.
								//
								group._itemsVersion = sourceGroup._itemsVersion;
							}

							// AS 10/11/07
							// Verify that the DialogBoxLauncher tool is still the same.
							//
							FrameworkElement sourceRootDialogLauncher = RibbonToolProxy.GetRootSourceTool(sourceGroup.DialogBoxLauncherTool);
							FrameworkElement clonedRootDialogLauncher = RibbonToolProxy.GetRootSourceTool(group.DialogBoxLauncherTool);

							if (sourceRootDialogLauncher != clonedRootDialogLauncher)
							{
								// AS 10/12/07
								// This will be done when the Ribbon property changes and the tool is unregistered.
								//
								//// if it hasn't and we have one on the clone then release it now
								//RibbonToolProxy.ReleaseToolClone(group.DialogBoxLauncherTool);

								// if we have a new one then clone that...
								if (sourceGroup.DialogBoxLauncherTool != null)
								{
									IList<object> items = RibbonToolProxy.CloneItems(new ButtonTool[] { sourceGroup.DialogBoxLauncherTool }, true, null);
									group.DialogBoxLauncherTool = items[0] as ButtonTool;
								}
								else
									group.DialogBoxLauncherTool = null;
							}
						}

						// AS 11/1/07 BR27469
						// Moved this down from above since we should synchronize before raising the opening event.
						//
						if ( group.RaiseOpeningEvent() == false )
							return KnownBoxes.FalseBox;
					}
				}
			}

			return value;
		}

		private static void OnIsOpenChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = target as RibbonGroup;

			if (group != null && e.NewValue is bool)
			{
				group.VerifyRibbonGroupSite();

				bool val = (bool)(e.NewValue);
				XamRibbon ribbon = XamRibbon.GetRibbon(group);

				// if the ribbon group is being shown on the qat then we 
				// want to reset the size of the tools in the group
				if (val && XamRibbon.GetLocation(group) == ToolLocation.QuickAccessToolbar && group._groupPresenterCollapsed != null)
				{
                    // AS 2/5/09 TFS11796
                    // With the fix for TFS9276, when a tool is unregistered we always 
                    // suspend the tool registration operations so if the tools collection 
                    // was changed in the coerce then the panels/tools may not have been
                    // stored in the registeredPanels|Menus yet.
                    //
                    if (group.CloneFromGroup != null && null != ribbon)
                        ribbon.ForcePendingToolRegistrations();

					// we have to force a measure of the collapse content presenter since it is collapsed
					// and the tools have not been setup/initialized yet
					group._groupPresenterCollapsed.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					VariantManager.ResetVariantInfo(group, true, group);
				}

				// if its being opened
				if (val)
				{
					if (null != ribbon && ribbon.HasKeyTipManager)
					{
						KeyTipManager keyTipManager = ribbon.KeyTipManager;

						// if we're collapsed and we're opening then make sure that we're the active 
						// key tip provider
						if (keyTipManager.IsActive && keyTipManager.ProviderPendingActivation != group)
						{
							keyTipManager.ActivateKeyTipProvider(group);
						}
					}

					
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

					group.PopupOwnerProxy.OnOpen();
				}
				




				else
					group.PopupOwnerProxy.OnClose();

				RoutedEventArgs args = new RoutedEventArgs();

				if (val == true)
					group.RaiseOpened(args);
				else
					group.RaiseClosed(args);

				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


                // TK Raise property change event
                bool newValue = (bool)e.NewValue;
                bool oldValue = !newValue;
                group.RaiseAutomationExpandCollapseStateChanged(oldValue, newValue);


                // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
                group.UpdateVisualStates();

			}
		}

		/// <summary>
		/// Gets/sets whether the RibbonGroup is open.
		/// </summary>
		/// <remarks>
		/// <p class="body">The IsOpen property is used to open or close the RibbonGroup when it has been collapsed or when it 
		/// is displayed within the <see cref="QuickAccessToolbar"/>. When the RibbonGroup is opened, a popup displaying contents 
		/// (its <see cref="ItemsControl.Items"/> and the caption area containing the <see cref="Caption"/> and <see cref="DialogBoxLauncherTool"/>) 
		/// will be displayed. Changing its value will cause either the <see cref="Closed"/> event or the <see cref="Opening"/>/<see cref="Opened"/> 
		/// events to be raised.</p>
		/// <p class="note"><b>Note:</b> The <b>IsOpen</b> property is only used for a RibbonGroup whose <see cref="IsCollapsed"/> is true or when the group is displayed within the <see cref="QuickAccessToolbar"/>.</p>
		/// </remarks>
		/// <seealso cref="IsOpenProperty"/>
		/// <seealso cref="Opening"/>
		/// <seealso cref="Closed"/>
		/// <seealso cref="Opened"/>
		//[Description("Gets/sets whether the RibbonGroup is open.")]
		//[Category("Ribbon Properties")]
		public bool IsOpen
		{
			get
			{
				return (bool)this.GetValue(RibbonGroup.IsOpenProperty);
			}
			set
			{
				this.SetValue(RibbonGroup.IsOpenProperty, value);
			}
		}

				#endregion //IsOpen

				#region KeyTip

		/// <summary>
		/// Identifies the KeyTip dependency property.
		/// </summary>
		/// <seealso cref="KeyTip"/>
		public static readonly DependencyProperty KeyTipProperty = RibbonToolHelper.KeyTipProperty.AddOwner(typeof(RibbonGroup));

		/// <summary>
		/// A string with a maximum length of 3 characters that is used to navigate to the item when keytips.
		/// </summary>
		/// <remarks>
		/// <p class="body">Key tips are displayed when the ribbon is showing and the Alt key is pressed. The KeyTip for a RibbonGroup is 
		/// only used when the <see cref="IsCollapsed"/> property is true and is used to change the state of the <see cref="IsOpen"/> property to 
		/// display/hide the contents of its popup.</p>
		/// <p class="note"><br>Note: </br>If the key tip for the item conflicts with another item in the same container, this key tip may be changed.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">The value assigned has more than 3 characters.</exception>
		/// <seealso cref="KeyTipProperty"/>
		/// <seealso cref="IsOpen"/>
		//[Description("A string with a maximum length of 3 characters that is used to navigate to the item when keytips.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string KeyTip
		{
			get
			{
				return (string)this.GetValue(RibbonGroup.KeyTipProperty);
			}
			set
			{
				this.SetValue(RibbonGroup.KeyTipProperty, value);
			}
		}

				#endregion //KeyTip

				#region Location

		/// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		/// <seealso cref="Location"/>
		public static readonly DependencyProperty LocationProperty = XamRibbon.LocationProperty.AddOwner(typeof(RibbonGroup));

		/// <summary>
		/// Returns an enumeration that indicates the location of the RibbonGroup. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">The Location property is primarily intended to determine if the RibbonGroup is displayed within the <see cref="QuickAccessToolbar"/>. 
		/// When it is displayed within the QAT, the RibbonGroup is represented by a button that when clicked will open the group and display 
		/// its contents within a popup. The template of that button is based upon the template associated with the <see cref="QuickAccessToolbarGroupButtonStyleKey"/> 
		/// resource key.</p>
		/// </remarks>
		/// <seealso cref="LocationProperty"/>
		/// <seealso cref="ToolLocation"/>
		/// <seealso cref="IsOpen"/>
		//[Description("Returns an enumeration that indicates the location of the tool. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ToolLocation Location
		{
			get
			{
				return (ToolLocation)this.GetValue(RibbonGroup.LocationProperty);
			}
		}

				#endregion //Location

				#region QuickAccessToolbarGroupButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the <see cref="System.Windows.Controls.Primitives.ToggleButton"/> that represents the <see cref="RibbonGroup"/> within the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="ControlTemplate"/> targeting a <see cref="DropDownToggle"/>. The template is used when the RibbonGroup has been placed 
		/// in the <see cref="QuickAccessToolbar"/>. For information on how to add a RibbonGroup to the QAT, please refer to the <see cref="Id"/> property.</p>
		/// </remarks>
		/// <seealso cref="Id"/>
		/// <seealso cref="Location"/>
		public static readonly ResourceKey QuickAccessToolbarGroupButtonStyleKey = new StaticPropertyResourceKey(typeof(RibbonGroup), "QuickAccessToolbarGroupButtonStyleKey");

				#endregion //QuickAccessToolbarGroupButtonStyleKey

				#region SmallImage

		/// <summary>
		/// Identifies the SmallImage dependency property.
		/// </summary>
		/// <seealso cref="SmallImage"/>
		public static readonly DependencyProperty SmallImageProperty = RibbonToolHelper.SmallImageProperty.AddOwner(typeof(RibbonGroup));

		/// <summary>
		/// Returns/sets the 16x16 image to be used when the RibbonGroup is collapsed or displayed in the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The SmallImage is used by default in the <see cref="CollapsedGroupButtonStyleKey"/> and <see cref="QuickAccessToolbarGroupButtonStyleKey"/> keyed templates.</p>
		/// <p class="note"><b>Note:</b> The SmallImage is not displayed within the caption area of the RibbonGroup.</p>
		/// </remarks>
		/// <seealso cref="SmallImageProperty"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="IsCollapsed"/>
		/// <seealso cref="Location"/>
		//[Description("Returns/sets the 16x16 image to be used when the RibbonGroup is collapsed or displayed in the QuickAccessToolbar.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ImageSource SmallImage
		{
			get
			{
				return (ImageSource)this.GetValue(RibbonGroup.SmallImageProperty);
			}
			set
			{
				this.SetValue(RibbonGroup.SmallImageProperty, value);
			}
		}

				#endregion //SmallImage

				#region Variants
		/// <summary>
		/// Returns a collection of <see cref="GroupVariant"/> instances that determines the order/priority in which the group will be resized.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, the Variants collection is empty for all RibbonGroups. When all RibbonGroups within a 
		/// RibbonTabItem have no items in their Variants collection, the default resizing logic will be used. When a <see cref="GroupVariant"/> 
		/// is added to any of the RibbonGroups within a RibbonTabItem, only those resize actions will be used to resize the groups.</p>
		/// <p class="body">The <see cref="MaximumSizeProperty"/> and <see cref="MinimumSizeProperty"/> attached properties are also used in 
		/// determining how the contents of a RibbonGroup may be resized.</p>
		/// <p class="body">For a discussion of how the Variants are used, please refer to the <see cref="GroupVariant"/> class.</p>
		/// </remarks>
		/// <seealso cref="MaximumSizeProperty"/>
		/// <seealso cref="MinimumSizeProperty"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		//[Description("A collection of 'GroupVariant' instances that determine the order/priority in which the group will be resized within the containing 'RibbonTabItem'.")]
		//[Category("Ribbon Properties")]
		public ObservableCollectionExtended<GroupVariant> Variants
		{
			get
			{
				if (null == _variants)
				{
					this._variants = new ObservableCollectionExtended<GroupVariant>();
				}

				return this._variants;
			}
		}

				#endregion //Variants
		
			#endregion //Public Properties

			#region Internal Properties

				#region CaptionElement

		internal FrameworkElement CaptionElement
		{
			get
			{
				return this._captionElement;
			}
		}

				#endregion // CaptionElement

				#region CloneFromGroup

		private static readonly DependencyPropertyKey CloneFromGroupPropertyKey =
			DependencyProperty.RegisterReadOnly("CloneFromGroup",
			typeof(RibbonGroup), typeof(RibbonGroup), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="CloneFromGroup"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty CloneFromGroupProperty =
			CloneFromGroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the 'RibbonGroup' from which this was cloned.
		/// </summary>
		/// <seealso cref="CloneFromGroupProperty"/>
		//[Description("Returns the 'RibbonGroup' from which this was cloned.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		internal RibbonGroup CloneFromGroup
		{
			get
			{
				return (RibbonGroup)this.GetValue(RibbonGroup.CloneFromGroupProperty);
			}
		}

				#endregion //CloneFromGroup

				#region CurrentContentSite
		internal ContentControl CurrentContentSite
		{
			get
			{
				ContentControl cp = this._groupPresenterNormal;

				if (null == this._groupPresenterNormal || null == this._groupPresenterNormal.Content)
					cp = this._groupPresenterCollapsed;

				return cp;
			}
		} 
				#endregion //CurrentContentSite

				#region HeaderElement

		internal FrameworkElement HeaderElement
		{
			get
			{
				return this._headerElement;
			}
		}

				#endregion // HeaderElement

				#region IsDropDown
		internal bool IsDropDown
		{
			get
			{
				return ToolLocation.QuickAccessToolbar.Equals(this.GetValue(XamRibbon.LocationProperty)) ||
					this.IsCollapsed;
			}
		} 
				#endregion //IsDropDown

				#region RegisteredMenus
		internal IList<MenuTool> RegisteredMenus
		{
			get { return this._registeredMenus; }
		} 
				#endregion //RegisteredMenus

				#region RegisteredPanels
		internal IList<IRibbonToolPanel> RegisteredPanels
		{
			get { return this._registeredPanels; }
		} 
				#endregion //RegisteredPanels

			#endregion //Internal Properties

			#region Private Properties

				// AS 10/8/07 PopupOwnerProxy
				#region PopupOwnerProxy
		private PopupOwnerProxy PopupOwnerProxy
		{
			get
			{
				if (this._popupOwnerProxy == null)
				{
					this._popupOwnerProxy = new PopupOwnerProxy(this);
					this._popupOwnerProxy.Initialize(this);
				}

				return this._popupOwnerProxy;
			}
		} 
				#endregion //PopupOwnerProxy

				#region Ribbon

		private XamRibbon Ribbon
		{
			get
			{
				if (this._ribbon == null)
					this._ribbon = Infragistics.Windows.Utilities.GetAncestorFromType(this, typeof(XamRibbon),  true) as XamRibbon;

				return this._ribbon;
			}
		}

				#endregion //Ribbon	

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region Clone
		private RibbonGroup Clone()
		{
            // AS 9/22/08
            // Refactored CloneHelper into new (Tool)CloneManager
            //
            //return (RibbonGroup)RibbonToolProxy.CloneHelper(this);
            IList<DependencyProperty> propsToSkip = null;

            // AS 11/5/08 TFS9191
            // Currently we're cloning this initially but we should wait until
            // the dropdown like we do with cloning the items. We should also skip
            // the properties we will be binding
            //
            propsToSkip = new DependencyProperty[] { 
                RibbonGroup.DialogBoxLauncherToolProperty, 
                RibbonGroup.IsOpenProperty

				// AS 7/10/09 TFS18328
				, FrameworkElement.DataContextProperty
				, FrameworkElement.ContextMenuProperty
				, FrameworkElement.ToolTipProperty
			};

            return (RibbonGroup)new ToolCloneManager(propsToSkip, false, true, true, null).Clone(this);
		} 
				#endregion //Clone

				#region CloneForQat
		internal RibbonGroup CloneForQat()
		{
			RibbonGroup clone = this.Clone();

			// keep track of the tool that it was cloned from
			clone.SetValue(CloneFromGroupPropertyKey, this);

			clone.SetBinding(IdProperty, Utilities.CreateBindingObject(IdProperty, System.Windows.Data.BindingMode.TwoWay, this));
			clone.SetBinding(RibbonToolHelper.SmallImageProperty, Utilities.CreateBindingObject(RibbonToolHelper.SmallImageProperty, System.Windows.Data.BindingMode.TwoWay, this));
			clone.SetBinding(RibbonToolHelper.CaptionProperty, Utilities.CreateBindingObject(RibbonToolHelper.CaptionProperty, System.Windows.Data.BindingMode.TwoWay, this));

            // AS 11/5/08 TFS9191
            clone.SetBinding(FrameworkElement.IsEnabledProperty, Utilities.CreateBindingObject(FrameworkElement.IsEnabledProperty, System.Windows.Data.BindingMode.TwoWay, this));
            clone.SetBinding(FrameworkElement.VisibilityProperty, Utilities.CreateBindingObject(FrameworkElement.VisibilityProperty, System.Windows.Data.BindingMode.TwoWay, this));

			// AS 7/10/09 TFS18328
            clone.SetBinding(FrameworkElement.DataContextProperty, Utilities.CreateBindingObject(FrameworkElement.DataContextProperty, System.Windows.Data.BindingMode.TwoWay, this));
            clone.SetBinding(FrameworkElement.ContextMenuProperty, Utilities.CreateBindingObject(FrameworkElement.ContextMenuProperty, System.Windows.Data.BindingMode.TwoWay, this));
            clone.SetBinding(FrameworkElement.ToolTipProperty, Utilities.CreateBindingObject(FrameworkElement.ToolTipProperty, System.Windows.Data.BindingMode.TwoWay, this));

			RoutedEventManager.CopyAllEventHandlers(this, clone);

			// the group should not be consider collapsed
			clone.ClearValue(RibbonGroup.IsCollapsedPropertyKey);

			// make sure it knows its on the qat so it shows the proper template
			clone.SetValue(XamRibbon.LocationPropertyKey, RibbonKnownBoxes.ToolLocationQuickAccessToolbarBox);

			// keep track of the fact that the group has an instance on the qat
			this.SetValue(RibbonToolHelper.IsOnQatPropertyKey, KnownBoxes.TrueBox);

			// AS 10/11/07
			// We have code in other places that determines the enable state of commands like add to qat
			// based on the is on qat so make sure this clone is set as added to the qat.
			//
			clone.SetValue(RibbonToolHelper.IsOnQatPropertyKey, KnownBoxes.TrueBox);

			return clone;
		} 
				#endregion //CloneForQat

				#region GetToolRowVerticalOffset
		/// <summary>
		/// Returns the vertical offset for a key tip row number in the group relative to the 
		/// current content site - i.e. the area that displays the tools and caption
		/// </summary>
		/// <param name="rowNumber">A number between 0 and 3 (inclusive)</param>
		/// <returns>An offset relative to the current content site</returns>
		internal double GetToolRowVerticalOffset(int rowNumber)
		{
			Debug.Assert(this.Tag == XamRibbon.RibbonGroupForResizingTag, "This should only be called with the ribbon group for sizing.");

			Debug.Assert(rowNumber >= 0 && rowNumber <= 3, "There can only be 4 rows - 0 to 3");
			rowNumber = Math.Min(3, Math.Max(0, rowNumber));

			ContentControl currentSite = this.CurrentContentSite;

			if (null != currentSite)
			{
				// use the height of the group itself since the keytip
				// seems to be positioned with its top along the bottom
				if (rowNumber == 3)
				{
					Debug.Assert(currentSite.ActualHeight == this.ActualHeight, "These values are different. Perhaps we should use the ActualHeight?");

					return currentSite.ActualHeight;
				}

				// for the top 3 rows we need to base it on the items presenter - i.e. where
				// the tools are.
				if (this._itemsPresenter != null)
				{
					// get a value equaling either the top, middle or bottom of the items presenter
					double itemsPresenterOffset = this._itemsPresenter.ActualHeight / 2 * rowNumber;

					GeneralTransform gt = null;

					try
					{
						gt = this._itemsPresenter.TransformToAncestor(currentSite);
					}
					catch (ArgumentException)
					{
					}
					catch (InvalidOperationException)
					{
					}

					if (null != gt)
					{
						// then map that relative to the content site
						return gt.Transform(new Point(0, itemsPresenterOffset)).Y;
					}
				}
			}

			// fallback to a default since we don't have access to the elements needed to make 
			// the calculation. we'll just split the height evenly
			return this.ActualHeight / 3 * rowNumber;
		} 
				#endregion //GetToolRowVerticalOffset

				#region ProcessPendingRegistrationChange
		internal void ProcessPendingRegistrationChange(XamRibbon ribbon, bool isRegistering)
		{
            
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            // AS 2/10/09 TFS11796
            // As with the tool pending registration handling fix for TFS11796, this too 
            // should prefer the logical tree. We should also only have bailed out if we
            // are trying to unregister the ribbon.
            //
            //if (null != ribbon && Utilities.IsDescendantOf(ribbon, this))
            if (null != ribbon && !isRegistering && Utilities.IsDescendantOf(ribbon, this, true))
                return;

			XamRibbon oldRibbon = isRegistering ? null : ribbon;
			XamRibbon newRibbon = isRegistering ? ribbon : null;

			this.OnRibbonChanged(oldRibbon, newRibbon);
		} 
				#endregion //ProcessPendingRegistrationChange

			#endregion //Internal Methods

			#region Protected methods

                #region VisualState... Methods


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            bool mouseOver = this.IsMouseOver;

            // set common states
            if (this.IsEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else
            {
                // JJD 5/20/10 - TFS32570
                // Added Highlight state
                if (this.HighlightContent)
                {
                    if (mouseOver)
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateHighlight, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                    else
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateHighlight, VisualStateUtilities.StateNormal);
                }
                else
                {
                    if (mouseOver)
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                    else
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
                }
            }

            // set Active states
            if (this.IsActive == true)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);

            // set Contextual states
            if (this.IsInContextualTabGroup)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateContextual, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNonContextual, useTransitions);

            // set expand states
            if ( this.IsCollapsed )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateCollapsed, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateExpanded, useTransitions);

            // JJD 5/20/10 - TFS32570
            // Added focus states
            bool isFocused = this.IsKeyboardFocusWithin;

            // Set Focus states
            if (isFocused)
            {
                if (this.IsOpen)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateFocusedDropDown, VisualStateUtilities.StateFocused);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateFocused, useTransitions);
            }
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnfocused, useTransitions);

            // set location states
            if (this.Location == ToolLocation.QuickAccessToolbar)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateQAT, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateRibbon, useTransitions);
   
        }

        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            RibbonGroup group = target as RibbonGroup;

            if ( group    != null )
                group.UpdateVisualStates();
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

			#region Private Methods

                // AS 2/5/09 TFS11796
                // Do not bump the version of the clone group. We're using this
                // to determine when the source group is different than the 
                // clone group.
                //
                #region BumpItemsVersion
        private void BumpItemsVersion()
        {
            if (this.CloneFromGroup == null)
                this._itemsVersion++;
        } 
                #endregion //BumpItemsVersion

				#region OnIsActiveChanged
		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = d as RibbonGroup;

            if (null != group && group._groupButton != null)
            {
                group._groupButton.SetValue(XamRibbon.IsActivePropertyKey, e.NewValue);

                // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
                group.UpdateVisualStates();

            }
		} 
				#endregion //OnIsActiveChanged

				#region OnLocationChanged
		private static void OnLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = d as RibbonGroup;

            if (null != group)
            {
                group.VerifyRibbonGroupSite();

                // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
                group.UpdateVisualStates();

            }
		} 
				#endregion //OnLocationChanged

				#region OnRequestBringIntoView
		private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			RibbonGroup group = sender as RibbonGroup;
			Visual child = e.TargetObject as Visual;

			// if this is a group that can dropdown but isn't open...
			if (child != null && 
				group.IsDropDown &&
				group.IsOpen == false)
			{
				bool isInDropDown = (group._groupPresenterCollapsed != null && group._groupPresenterCollapsed.IsAncestorOf(child))
					|| (group._groupPresenterNormal != null && group._groupPresenterNormal.IsAncestorOf(child));

				// if we didn't find the child but its not a child of
				// the group then assume its in the dropdown since
				// the visual tree of the dropdown may be detached.
				if (isInDropDown == false && group.IsAncestorOf(child) == false)
					isInDropDown = true;

				if (isInDropDown && !RequestBringIntoViewCancelHelper.ShouldIgnoreBringIntoView(group) )
				{
					// ask the group to be brought into view
					group.BringIntoView();

					// then open the group
					group.IsOpen = true;
					e.Handled = true;
				}
			}
		}
				#endregion //OnRequestBringIntoView

				#region OnRibbonChanged
		private static void OnRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonGroup group = d as RibbonGroup;

			if (null != group)
			{
                // AS 8/21/08 BR33787
                // See the notes for the fix for BR28406. This is basically the same thing.
                // We're getting a Ribbon change but it hasn't really changed because we're
                // still in the logical tree of the ribbon so our property IS still set.
                //
                if (XamRibbon.GetRibbon(group) != e.NewValue)
                    return;

				XamRibbon oldRibbon = e.OldValue as XamRibbon;
				XamRibbon newRibbon = e.NewValue as XamRibbon;

				if (oldRibbon != null && oldRibbon.IsToolRegistrationSuspended)
				{
					Debug.Assert(newRibbon == null);
					oldRibbon.AddToPendingToolRegistrations(group, false);
					return;
				}

				if (newRibbon != null && newRibbon.IsToolRegistrationSuspended)
				{
					Debug.Assert(oldRibbon == null);
					newRibbon.AddToPendingToolRegistrations(group, true);
					return;
				}

				group.OnRibbonChanged(oldRibbon, newRibbon);
			}
		}

		private void OnRibbonChanged(XamRibbon oldRibbon, XamRibbon newRibbon)
		{
            // AS 2/10/09 TFS12207
            // If the group is owned by a different ribbon then be sure to
            // process the "unregister" of it before registering it with a
            // different one.
            //
            XamRibbon currentOwner = XamRibbon.GetRegisteredOwner(this);

            // if we have a current owner and we're not about to unregister
            // from that one then see if we need to unregister from the 
            // old owner first.
            //
            if (null != currentOwner && oldRibbon != currentOwner && currentOwner != newRibbon)
            {
                // if its owned by a different ribbon then make sure that ribbon
                // processes any pending registrations so it will clean up the 
                // group before we register it with the new ribbon
                currentOwner.ForcePendingToolRegistrations();
            }

			if (oldRibbon != null)
			{
				oldRibbon.RemovePendingToolRegistration(this, false);
				oldRibbon.UnregisterGroup(this);

				// if the this was on the qat then clear the isonqat of the source this
				// so it knows it can still be readded to the qat
				if (this.CloneFromGroup != null)
				{
					// remove all copied event handlers
					RoutedEventManager.ClearAllEventHandlers(this);


					// JM 10-11-07 - Commented out this assert since we are getting in here twice for some reason when a RibbonGroup is
					// removed from the QAT.  Also changed the SetValue to a ClearValue.
					//Debug.Assert(RibbonToolHelper.GetIsOnQat(this.CloneFromGroup));
					//this.CloneFromGroup.SetValue(RibbonToolHelper.IsOnQatPropertyKey, KnownBoxes.FalseBox);
					this.CloneFromGroup.ClearValue(RibbonToolHelper.IsOnQatPropertyKey);


					// AS 10/11/07
					// We're now setting the IsOnQat property of the ribbon group clone so 
					// we need to clear this out or we will try to remove the ribbon group
					// below.
					//
					this.ClearValue(RibbonToolHelper.IsOnQatPropertyKey);
				}

				if (RibbonToolHelper.GetIsOnQat(this))
				{
					// if the ribbon this was cloned and put on the qat and the source
					// this is being removed then remove the qat instance as well
					oldRibbon.QuickAccessToolbar.RemoveRibbonGroup(this);
				}
			}

			this._ribbon = newRibbon;

			if (newRibbon != null)
			{
				newRibbon.RemovePendingToolRegistration(this, true);
				newRibbon.VerifyGroupIsRegistered(this);
			}

            // AS 2/10/09 TFS12207
            // Keep a reference to the owning ribbon in case we need to clean up the 
            // old one.
            //
            XamRibbon.SetRegisteredOwner(this, newRibbon);

			this.VerifyRibbonGroupSite();
		} 
				#endregion //OnRibbonChanged

                #region RaiseAutomationExpandCollapseStateChanged

        private void RaiseAutomationExpandCollapseStateChanged(bool oldValue, bool newValue)
        {
            RibbonGroupAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as RibbonGroupAutomationPeer;

            if (null != peer)
                peer.RaiseExpandCollapseStateChanged(oldValue, newValue);
        }

                #endregion //RaiseAutomationExpandCollapseStateChanged	

				#region SynchronizeItemsHeight
		internal void SynchronizeItemsHeight()
		{
			// we don't want to do anything for the ribbon group created to provide the sizing
			if (this._itemsPresenter != null)
			{
				XamRibbon ribbon = this.Ribbon;

				if (null != ribbon)
				{
					if (this.Parent != null)
					{
						// all ribbon groups will synchronize the height of their items presenter to that
						// of the ribbon group we use for resizing so they are all the same
						this._itemsPresenter.SetBinding(FrameworkElement.HeightProperty, Utilities.CreateBindingObject(XamRibbon.RibbonGroupItemsHeightProperty, BindingMode.OneWay, ribbon));
					}
					else if (this.Tag == XamRibbon.RibbonGroupForResizingTag)
					{
						// this is the resizing ribbon group so it will be the source for the height
						// of the items presenters of the other groups in the ribbon
						ribbon.SetBinding(XamRibbon.RibbonGroupItemsHeightProperty, Utilities.CreateBindingObject(FrameworkElement.ActualHeightProperty, BindingMode.OneWay, this._itemsPresenter));
					}
				}
			}
		} 
				#endregion //SynchronizeItemsHeight

				// AS 6/18/08 BR33990
				#region UpdateHighlightContentBindings
		private void UpdateHighlightContentBindings(ContentControl target)
		{
			if (null == target)
			{
				BindingOperations.ClearBinding(this, IsMouseOverContentProperty);
				BindingOperations.ClearBinding(this, IsMouseCaptureWithinContentProperty);
			}
			else
			{
				this.SetBinding(IsMouseOverContentProperty, Utilities.CreateBindingObject(UIElement.IsMouseOverProperty, BindingMode.OneWay, target));
				this.SetBinding(IsMouseCaptureWithinContentProperty, Utilities.CreateBindingObject(UIElement.IsMouseCaptureWithinProperty, BindingMode.OneWay, target));
			}
		} 
				#endregion //UpdateHighlightContentBindings

				#region VerifyRibbonGroupSite
		private void VerifyRibbonGroupSite()
		{
			this.VerifyRibbonGroupSite(false);
		}

		private void VerifyRibbonGroupSite(bool forceUpdate)
		{
			// we need both content presenters in order to switch the content between them...
			if (this._groupPresenterCollapsed != null && this._groupPresenterNormal != null)
			{
				bool canOpen = this.IsDropDown;

				if (canOpen == false || this.IsOpen || forceUpdate)
				{
					ContentControl source = canOpen ? this._groupPresenterNormal : this._groupPresenterCollapsed;
					ContentControl target = canOpen ? this._groupPresenterCollapsed : this._groupPresenterNormal;

					if (null != source.Content)
					{
						XamRibbon ribbon = this.Ribbon;

						if (null != ribbon)
							ribbon.SuspendToolRegistration();

						try
						{
							object oldContent = source.Content;
							source.Content = null;
							target.Content = oldContent;

							RibbonGroupPanel groupPanel = Utilities.GetAncestorFromType(this, typeof(RibbonGroupPanel), true) as RibbonGroupPanel;

							if (null != groupPanel)
								groupPanel.PrepareCollapsedRibbonGroup(this);

							// AS 6/18/08 BR33990
							this.UpdateHighlightContentBindings(target);
						}
						finally
						{
							if (null != ribbon)
								ribbon.ResumeToolRegistration();
						}
					}
				}
			}
		}

				#endregion //VerifyRibbonGroupSite

			#endregion //Private Methods

		#endregion //Methods

		#region Events

			#region Activated

		/// <summary>
		/// Event ID for the <see cref="Activated"/> routed event
		/// </summary>
		/// <seealso cref="Activated"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		internal static readonly RoutedEvent ActivatedEvent = MenuTool.ActivatedEvent.AddOwner(typeof(RibbonGroup));

		/// <summary>
		/// Occurs after a tool has been activated
		/// </summary>
		/// <seealso cref="XamRibbon.ActiveItem"/>
		/// <seealso cref="XamRibbon.IsActiveProperty"/>
		/// <seealso cref="XamRibbon.GetIsActive(DependencyObject)"/>
		/// <seealso cref="ActivatedEvent"/>
		/// <seealso cref="ItemActivatedEventArgs"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		//[Description("Occurs after a 'RibbonGroup' has been activated")]
		//[Category("Ribbon Properties")]
		internal event EventHandler<ItemActivatedEventArgs> Activated
		{
			add
			{
				base.AddHandler(RibbonGroup.ActivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(RibbonGroup.ActivatedEvent, value);
			}
		}

			#endregion //Activated

			#region Deactivated

		/// <summary>
		/// Event ID for the <see cref="Deactivated"/> routed event
		/// </summary>
		/// <seealso cref="Deactivated"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		internal static readonly RoutedEvent DeactivatedEvent = MenuTool.DeactivatedEvent.AddOwner(typeof(RibbonGroup));

		/// <summary>
		/// Occurs after a tool has been de-activated
		/// </summary>
		/// <seealso cref="XamRibbon.ActiveItem"/>
		/// <seealso cref="XamRibbon.IsActiveProperty"/>
		/// <seealso cref="XamRibbon.GetIsActive(DependencyObject)"/>
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
				base.AddHandler(RibbonGroup.DeactivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(RibbonGroup.DeactivatedEvent, value);
			}
		}

			#endregion //Deactivated

			#region Closed

		/// <summary>
		/// Event ID for the <see cref="Closed"/> routed event
		/// </summary>
		/// <seealso cref="Closed"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OnClosed"/>
		public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RibbonGroup));

		/// <summary>
		/// Occurs after the RibbonGroup has been closed
		/// </summary>
		/// <remarks>
		/// <p class="body">The OnClosed method is invoked by the class to raise the <see cref="Closed"/> event.</p>
		/// </remarks>
		/// <seealso cref="Closed"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="ClosedEvent"/>
		protected virtual void OnClosed(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseClosed(RoutedEventArgs args)
		{
			args.RoutedEvent = RibbonGroup.ClosedEvent;
			args.Source = this;
			this.OnClosed(args);
		}

		/// <summary>
		/// Occurs after the RibbonGroup has been closed
		/// </summary>
		/// <remarks>
		/// <p class="body">The Closed event is invoked when the <see cref="IsOpen"/> property of a RibbonGroup has been changed 
		/// from true to false and the popup containing the contents of the RibbonGroup has been hidden.</p>
		/// </remarks>
		/// <seealso cref="OnClosed"/>
		/// <seealso cref="ClosedEvent"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Opened"/>
		/// <seealso cref="Opening"/>
		//[Description("Occurs after the RibbonGroup has been closed")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler Closed
		{
			add
			{
				base.AddHandler(RibbonGroup.ClosedEvent, value);
			}
			remove
			{
				base.RemoveHandler(RibbonGroup.ClosedEvent, value);
			}
		}

			#endregion //Closed

			#region Opened

		/// <summary>
		/// Event ID for the <see cref="Opened"/> routed event
		/// </summary>
		/// <seealso cref="Opened"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OnOpened"/>
		public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RibbonGroup));

		/// <summary>
		/// Occurs after the RibbonGroup has been opened
		/// </summary>
		/// <remarks>
		/// <p class="body">The OnOpened method is invoked by the class to raise the <see cref="Opened"/> event.</p>
		/// </remarks>
		/// <seealso cref="Opened"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OpenedEvent"/>
		protected virtual void OnOpened(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseOpened(RoutedEventArgs args)
		{
			args.RoutedEvent = RibbonGroup.OpenedEvent;
			args.Source = this;
			this.OnOpened(args);
		}

		/// <summary>
		/// Occurs after the RibbonGroup has been opened
		/// </summary>
		/// <remarks>
		/// <p class="body">The Opened event is invoked when the <see cref="IsOpen"/> property of a RibbonGroup has been changed 
		/// from false to true and the popup containing the contents of the RibbonGroup is being displayed.</p>
		/// </remarks>
		/// <seealso cref="OnOpened"/>
		/// <seealso cref="OpenedEvent"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Closed"/>
		/// <seealso cref="Opening"/>
		//[Description("Occurs after the RibbonGroup has been opened")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler Opened
		{
			add
			{
				base.AddHandler(RibbonGroup.OpenedEvent, value);
			}
			remove
			{
				base.RemoveHandler(RibbonGroup.OpenedEvent, value);
			}
		}

			#endregion //Opened

			#region Opening

		/// <summary>
		/// Event ID for the <see cref="Opening"/> routed event
		/// </summary>
		/// <seealso cref="Opening"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OnOpening"/>
		public static readonly RoutedEvent OpeningEvent =
			EventManager.RegisterRoutedEvent("Opening", RoutingStrategy.Bubble, typeof(EventHandler<RibbonGroupOpeningEventArgs>), typeof(RibbonGroup));

		/// <summary>
		/// Occurs before the RibbonGroup has been opened
		/// </summary>
		/// <remarks>
		/// <p class="body">The OnOpening method is invoked by the class to raise the <see cref="Opening"/> event.</p>
		/// </remarks>
		/// <seealso cref="Opening"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OpeningEvent"/>
		protected virtual void OnOpening(RibbonGroupOpeningEventArgs args)
		{
			this.RaiseEvent(args);
		}

		private void RaiseOpening(RibbonGroupOpeningEventArgs args)
		{
			args.RoutedEvent = RibbonGroup.OpeningEvent;
			args.Source = this;
			this.OnOpening(args);
		}

		internal bool RaiseOpeningEvent()
		{
			RibbonGroupOpeningEventArgs args = new RibbonGroupOpeningEventArgs(this);

			// Raise the opening event
			this.RaiseOpening(args);

			// if not cancelled then return true;
			// AS 10/15/07
			// This event is no longer cancelable. See the RibbonGroupOpeningEventArgs class for an explanation.
			//
			//return  args.Cancel == false;
			return true;
		}

		/// <summary>
		/// Occurs before the RibbonGroup has been opened
		/// </summary>
		/// <remarks>
		/// <p class="body">The Opening event is invoked when the <see cref="IsOpen"/> property of a RibbonGroup is being changed 
		/// from false to true and the popup containing the contents of the RibbonGroup is about to be displayed.</p>
		/// </remarks>
		/// <seealso cref="OnOpening"/>
		/// <seealso cref="OpeningEvent"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Opened"/>
		/// <seealso cref="Closed"/>
		//[Description("Occurs before the RibbonGroup has been opened")]
		//[Category("Ribbon Events")]
		public event EventHandler<RibbonGroupOpeningEventArgs> Opening
		{
			add
			{
				base.AddHandler(RibbonGroup.OpeningEvent, value);
			}
			remove
			{
				base.RemoveHandler(RibbonGroup.OpeningEvent, value);
			}
		}

			#endregion //Opening

		#endregion //Events

		#region IRibbonToolLocation Members

		ToolLocation IRibbonToolLocation.Location
		{
			get { return ToolLocation.Ribbon; }
		}

		#endregion

		#region IKeyTipProvider Members

		bool IKeyTipProvider.Activate()
		{
			if (this.IsDropDown)
			{
				// AS 10/3/07 BR27037
				// Make the group be the focused element which should make it the active item.
				//
				this.Focus();

				this.IsOpen = true;
				return this.IsOpen;
			}
			else
			{
				return false;
			}
		}

		bool IKeyTipProvider.Equals(IKeyTipProvider provider)
		{
			return this == provider;
		}

		IKeyTipContainer IKeyTipProvider.GetContainer()
		{
			return this;
		}

		KeyTipAlignment IKeyTipProvider.Alignment
		{
			get 
			{ 
				// top center whether its on the qat (as a popup ribbon group) or
				// if its collapsed - in which case its at the bottom of the group
				return KeyTipAlignment.TopCenter; 
			}
		}

		string IKeyTipProvider.AutoGeneratePrefix
		{
			get { return XamRibbon.GetString("RibbonGroupAutoGeneratePrefix"); }
		}

		string IKeyTipProvider.Caption
		{
			get { return RibbonToolHelper.GetCaption(this); }
		}

		bool IKeyTipProvider.DeactivateParentContainersOnActivate
		{
			get { return true; }
		}

		bool IKeyTipProvider.IsEnabled
		{
			get { return this.IsEnabled; }
		}

		bool IKeyTipProvider.IsVisible
		{
			get { return this.IsVisible; }
		}

		string IKeyTipProvider.KeyTipValue
		{
			get 
			{
				if (XamRibbon.GetLocation(this) == ToolLocation.QuickAccessToolbar)
				{
					XamRibbon ribbon = XamRibbon.GetRibbon(this);
					QuickAccessToolbar qat = ribbon != null ? ribbon.QuickAccessToolbar : null;

					if (null != qat)
					{
						int index = qat.VisibleIndexOf(this);
						// AS 12/20/07
						//return QuickAccessToolbar.GetKeyTipAtIndex(index);
						return qat.GetKeyTipAtIndex(index);
					}
				}

				return RibbonToolHelper.GetKeyTip(this); 
			}
		}

		Point IKeyTipProvider.Location
		{
			get 
			{
				// if its on the qat, treat it like any tool
				if (XamRibbon.GetLocation(this) == ToolLocation.QuickAccessToolbar)
				{
					return ToolKeyTipProvider.GetToolKeyTipLocation(this, ToolLocation.QuickAccessToolbar, RibbonToolHelper.GetSizingMode(this));
				}

				// use the horizontal center of the item and the bottom of the group
				// since the keytip should be on the bottom row
				Rect rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);

				return new Point(rect.Left + rect.Width / 2, rect.Bottom);
			}
		}

		char IKeyTipProvider.Mnemonic
		{
			// AS 6/14/11 TFS73058
			//get { return '\0'; }
			get { return Utilities.GetFirstMnemonicChar(this.Caption); }
		}

		UIElement IKeyTipProvider.AdornedElement
		{
			get { return this; }
		}

		// AS 10/3/07 BR27022
		char IKeyTipProvider.DefaultPrefix
		{
			get { return '\0'; }
		}

		// AS 1/5/10 TFS25626
		bool IKeyTipProvider.CanAutoGenerateKeyTip
		{
			get { return this.Location != ToolLocation.QuickAccessToolbar; }
		}

		#endregion //IKeyTipProvider

		#region IKeyTipContainer Members

		void IKeyTipContainer.Deactivate()
		{
			this.IsOpen = false;

			// AS 5/27/08 BR31648
			if (this.IsKeyboardFocusWithin && this.Focusable)
				this.Focus();

			// AS 1/11/12 TFS30896
			RequestBringIntoViewCancelHelper.CancelBringIntoView(this);
		}

		IKeyTipProvider[] IKeyTipContainer.GetKeyTipProviders()
		{
			List<IKeyTipProvider> providers = new List<IKeyTipProvider>();

			this.VerifyRibbonGroupSite(true);

			// add tools and dialog box launcher
			foreach (IRibbonTool tool in this._registeredTools)
			{
				RibbonToolProxy proxy = tool.ToolProxy;

				if (null != proxy)
					providers.AddRange(proxy.GetKeyTipProviders((FrameworkElement)tool));
			}

			if (this.IsCollapsed)
				providers.Add(this);

			return providers.ToArray();
		}

		bool IKeyTipContainer.ReuseParentKeyTips
		{
			get { return true; }
		}

		#endregion

		// AS 10/8/07 PopupOwnerProxy
		#region IRibbonPopupOwner Members

		XamRibbon IRibbonPopupOwner.Ribbon
		{
			get { return XamRibbon.GetRibbon(this); }
		}

		bool IRibbonPopupOwner.CanOpen
		{
			get { return this.IsDropDown; }
		}

		bool IRibbonPopupOwner.IsOpen
		{
			get
			{
				return this.IsOpen;
			}
			set
			{
				this.IsOpen = value;
			}
		}

		bool IRibbonPopupOwner.FocusFirstItem()
		{
			return PopupOwnerProxy.FocusFirstItem(this);
		}

		UIElement IRibbonPopupOwner.PopupTemplatedParent
		{
			get { return this; }
		}

		bool IRibbonPopupOwner.HookKeyDown
		{
			get { return false; }
		}

		// AS 10/18/07
		FrameworkElement IRibbonPopupOwner.ParentElementToFocus
		{
			get { return this; }
		}

		#endregion

		// AS 8/16/11 TFS82301
		#region ItemContentPresenter class
		private class ItemContentPresenter : ContentPresenter
		{
			#region Constructor
			static ItemContentPresenter()
			{
				RibbonToolHelper.SizingModePropertyKey.OverrideMetadata(typeof(ItemContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSizingModeChanged)));
			}
			#endregion //Constructor

			#region Base class overrides

			#region OnVisualChildrenChanged
			/// <summary>
			/// Invoked when a visual child is added or removed.
			/// </summary>
			/// <param name="visualAdded">The child that was added</param>
			/// <param name="visualRemoved">The child that was removed</param>
			protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
			{
				if (visualRemoved != null)
				{
					this.ClearValue(RibbonGroup.MinimumSizeProperty);
					this.ClearValue(RibbonGroup.MaximumSizeProperty);
					visualRemoved.ClearValue(RibbonToolHelper.SizingModePropertyKey);
				}

				if (visualAdded != null)
				{
					// we want the tool panel to be able to know the minimum/maximum size of the item within
					this.SetBinding(RibbonGroup.MinimumSizeProperty, Utilities.CreateBindingObject(RibbonGroup.MinimumSizeProperty, BindingMode.OneWay, visualAdded));
					this.SetBinding(RibbonGroup.MaximumSizeProperty, Utilities.CreateBindingObject(RibbonGroup.MaximumSizeProperty, BindingMode.OneWay, visualAdded));

					// we want to keep the sizing mode that the tool panel gives us on the child
					visualAdded.SetValue(RibbonToolHelper.SizingModePropertyKey, this.GetValue(RibbonToolHelper.SizingModeProperty));
				}

				base.OnVisualChildrenChanged(visualAdded, visualRemoved);
			}
			#endregion //OnVisualChildrenChanged

			#endregion //Base class overrides

			#region Methods

			#region OnSizingModeChanged
			private static void OnSizingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				ItemContentPresenter cp = d as ItemContentPresenter;

				if (cp.VisualChildrenCount == 1)
				{
					Visual child = cp.GetVisualChild(0);
					child.SetValue(RibbonToolHelper.SizingModePropertyKey, cp.GetValue(RibbonToolHelper.SizingModeProperty));
				}
			}
			#endregion //OnSizingModeChanged

			#endregion //Methods
		} 
		#endregion //ItemContentPresenter class
	}

	// AS 1/11/12 TFS30896
	internal class RequestBringIntoViewCancelHelper
	{
		#region Constructor
		private RequestBringIntoViewCancelHelper()
		{
		} 
		#endregion //Constructor

		#region Properties

		#region Helper

		private static readonly DependencyProperty HelperProperty =
			DependencyProperty.RegisterAttached("Helper", typeof(DispatcherOperation), typeof(RequestBringIntoViewCancelHelper),
				new FrameworkPropertyMetadata(null));

		private static DispatcherOperation GetHelper(DependencyObject d)
		{
			return (DispatcherOperation)d.GetValue(HelperProperty);
		}

		private static void SetHelper(DependencyObject d, DispatcherOperation value)
		{
			d.SetValue(HelperProperty, value);
		}

		#endregion // Helper

		#endregion //Properties

		#region Internal Methods

		#region ShouldIgnoreBringIntoView
		internal static bool ShouldIgnoreBringIntoView(FrameworkElement fe)
		{
			return null != fe && GetHelper(fe) != null;
		}
		#endregion //ShouldIgnoreBringIntoView

		#region CancelBringIntoView
		internal static void CancelBringIntoView(FrameworkElement fe)
		{
			var operation = GetHelper(fe);

			if (null != operation)
				operation.Abort();

			operation = fe.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Threading.SendOrPostCallback(OnCallback), fe);
			SetHelper(fe, operation);
		}
		#endregion //CancelBringIntoView

		#endregion //Internal Methods

		#region Private Methods

		#region OnCallback
		private static void OnCallback(object param)
		{
			var fe = param as FrameworkElement;
			fe.ClearValue(HelperProperty);
		}
		#endregion //OnCallback

		#endregion //Private Methods
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