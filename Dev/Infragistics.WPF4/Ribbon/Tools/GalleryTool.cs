using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Collections.Specialized;
using System.Windows.Media;
using Infragistics.Windows.Ribbon.Events;
using Infragistics.Windows.Ribbon.Internal;
using System.Windows.Threading;
using System.Windows.Input;
using System.Diagnostics;
using Infragistics.Shared;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using Infragistics.Windows.Internal;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// The GalleryTool is designed to display a list of options which each contain a graphic that visually describes the option as well as optional textual description.
	/// </summary>
	/// <remarks>
	/// <p class="body">The GalleryTool exposes an Items property that holds a collection of <see cref="GalleryItem"/> objects.  Each <see cref="GalleryItem"/>
	/// exposes a <see cref="GalleryItem.Text"/>and a <see cref="GalleryItem.Image"/> property which are displayed for each item.  The GalleryTool also exposes
	/// a <see cref="GalleryTool.Groups"/> property that holds a collection of <see cref="GalleryItemGroup"/> objects which define groupings for the 
	/// <see cref="GalleryItem"/>s when they are displayed in a GalleryTool dropdown.  <see cref="GalleryItem"/>s, which can belong to 1 or more 
	/// <see cref="GalleryItemGroup"/>s, are assigned to a <see cref="GalleryItemGroup"/> by adding the Key associated with the <see cref="GalleryItem"/> to the 
	/// <see cref="GalleryItemGroup"/>'s <see cref="GalleryItemGroup.ItemKeys"/> collection.</p>
	/// <p class="body">When used within the <see cref="XamRibbon"/> control, the GalleryTool is designed to be placed on a <see cref="MenuTool"/>.  An exception 
	/// will be thrown by the <see cref="XamRibbon"/> if a GalleryTool is placed anywhere other than a <see cref="MenuTool"/>.</p>
	/// <p class="note"><b>Note: </b>There are no such restrictions when using the GalleryTool outside the <see cref="XamRibbon"/>.</p>
	/// <p class="body">To display a GalleryTool in a <see cref="RibbonGroup"/> with preview you should:
	/// <ul>
	/// <li>create a <see cref="MenuTool"/> and add it to a <see cref="RibbonGroup"/></li>
	/// <li>create a GalleryTool and add one or more <see cref="GalleryItemGroup"/>s, each with one or more <see cref="GalleryItem"/>s</li>
	/// <li>add the GalleryTool and to the <see cref="MenuTool"/>'s Items collection of the <see cref="MenuTool"/>.  You can also add other tools to the menu if necessary.</li>
	/// <li>set the <see cref="MenuTool"/>'s <see cref="MenuTool.ShouldDisplayGalleryPreview"/> property to true.  This will display a scrollable preview of 
	/// the <see cref="GalleryItem"/>s in the area which normally displays the <see cref="MenuTool"/>'s dropdown button.</li>
	/// </ul>
	/// When the GalleryTool preview is being shown in a <see cref="RibbonGroup"/> as described above, the <see cref="GalleryItemGroup"/> are not displayed - only the <see cref="GalleryItem"/>s
	/// are displayed.  In addition, up/down scroll buttons are automatically provided to allow the end-user to scroll through the <see cref="GalleryItem"/>s 
	/// if necessary.  Also, a dropdown button is provided below the scroll buttons that, when clicked, drops down the contents of the <see cref="MenuTool"/> including the 
	/// <see cref="GalleryItem"/>s, <see cref="GalleryItemGroup"/>s and any other tools that were added to the <see cref="MenuTool"/>.</p>
	/// <p class="body">Examples of how a GalleryTool might be used in an application include displaying a list of graphics showing the different text styles that 
	/// would result if particular document styles are selected, or graphics that show thumbnails of different chart renderings that would result if a 
	/// particular chart type is selected from the GalleryTool.</p>
	/// </remarks>
	/// <exception cref="NotSupportedException">If the GalleryTool is placed anywhere other than a <see cref="MenuTool"/> (e.g., on the <see cref="QuickAccessToolbar"/> 
	/// or in a <see cref="RibbonGroup"/>).</exception>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="MenuTool"/>
	/// <seealso cref="MenuTool.ShouldDisplayGalleryPreview"/>
	/// <seealso cref="RibbonGroup"/>
	/// <seealso cref="GalleryItem"/>
	/// <seealso cref="GalleryItemGroup"/>
	/// <seealso cref="QuickAccessToolbar"/>

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

	[ContentProperty("Items")]
	// AS 11/14/07 BR28450
	// Changed the part from a contentpresenter to a contentcontrol so its contents are in the logical tree.
	//
	[TemplatePart(Name = "PART_DropDownPresenterSite", Type = typeof(ContentControl))]
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class GalleryTool : Control, 
							   IRibbonTool
	{
		#region Member Variables

		private int												_activationActionDelay;
		private int												_activationInitialActionDelay = 1000;
		private GalleryItemCollection							_items;
		private ObservableCollection<GalleryItemGroup>			_groups;
		private ObservableCollection<GalleryItem>				_previewItemsInternal;

		private ContentControl									_dropDownPresenterSite;
		private GalleryToolDropDownPresenter					_dropDownPresenter;

		private bool											_ignoreSelectedItemChanged;

		private DispatcherTimer									_itemActivationDelayTimer;
		private GalleryItem										_currentActiveItem;
		private ItemsControl									_currentActiveItemArea;
		private bool											_isInInitialActivationState = true;
        
        // JJD 11/07/07 - added properties to support triggering of separators
        private bool                                            _isFirstInMenu;
		private bool											_isLastInMenu;

		// JM 11-12-07
		private bool											_ignoreNextMouseLeaveItemArea;
		private bool											_itemActivatedEventFired;

        // JJD 11/21/07 - BR27066
        // Keep track of whether at least one activation event was raised during a dropdown
		private bool											_itemActivatedEventFiredOnceDuringDropdown;
        
        // JJD 11/20/07 - BR27066
        // Added flag to bypass mouse enter processing when navigating via the keyboard
		private bool											_ignoreMouseEnter;

        // JJD 11/20/07 - BR27066
        // Keep track of GalleryItemPresenter of active item so we can clear the IsHighlighted property
        private GalleryItemPresenter                            _currentActiveItemPresenter;

        // JJD 11/27/07 - BR27268
        // Added member to cache the mesured size of an item.
        private Size                                            _measuredItemSize;


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


        #endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryTool"/> class.
		/// </summary>
		public GalleryTool()
		{
			this._previewItemsInternal = new ObservableCollection<GalleryItem>();
		}

		static GalleryTool()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(typeof(GalleryTool)));

			RibbonGroup.MaximumSizeProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox));
			RibbonGroup.MinimumSizeProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox));

			KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

            // JJD 11/20/07 - BR27066
            // Leave DirectionalNavigation default alone
            //KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));

            // JJD 11/20/07 - BR27066
            // Default focusable to false because we put focus in the galleryToolitems instead.
            FrameworkElement.FocusableProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            XamRibbon.IsActiveProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), XamRibbon.IsActivePropertyKey);
            XamRibbon.LocationProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), XamRibbon.LocationPropertyKey);
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(GalleryTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		#endregion //Constructor

		#region Base Class Overrides

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

			this.VerifyParts();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

			#endregion //OnApplyTemplate	
    
            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="GalleryTool"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.GalleryToolAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GalleryToolAutomationPeer(this);
        }
            #endregion

            #region OnInitialized

        /// <summary>
		/// Called when the element is initialized.
		/// </summary>
		/// <param name="e">The event args.</param>
    	protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.SetValue(GalleryTool.PreviewItemsPropertyKey, new ReadOnlyObservableCollection<GalleryItem>(this._previewItemsInternal));

			this.VerifyMaxColumnSpan();
		}

   			#endregion //OnInitialized	
    
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
    
			#region OnVisualParentChanged

		/// <summary>
		/// Invoked when the parent element of this element reports a change to its underlying visual parent.
		/// </summary>
		/// <param name="oldParent">The previous parent. This may be provided as null if the System.Windows.DependencyObject did not have a parent element previously.</param>
		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			base.OnVisualParentChanged(oldParent);

			DependencyObject	iv = this;
			DependencyObject	visual;
			bool				isSitedOnMenuTool = false;

			while (iv != null)
			{
				visual = VisualTreeHelper.GetParent(iv) as DependencyObject;
				if (visual != null)
				{
					if (visual is ToolMenuItem)
					{
						isSitedOnMenuTool = true;
						break;
					}
				}

				iv = visual;
			}
			
			// AS 9/25/07
			// Allow the tool to be removed without considering it an invalid parent.
			//
			//if (isSitedOnMenuTool == false)
			if (isSitedOnMenuTool == false && VisualTreeHelper.GetParent(this) != null)
				throw new NotSupportedException(XamRibbon.GetString("LE_NotSupportedException_1")); 

			// JM BR27059 10-3-07
			if (VisualTreeHelper.GetParent(this) != null)
			{
				// JJD 10/22/07
				// 
				//if (!(this.Parent is MenuTool)) // JM BR27259 10-10-07 - Check logical parent before doing GetAncestorFromType
				DependencyObject logicalParent = this.Parent;
				if (!( logicalParent is MenuTool) && // JM BR27259 10-10-07 - Check logical parent before doing GetAncestorFromType
					!(logicalParent is ToolMenuItem.LogicalContainer) && // AS 6/8/09 TFS17066
					!( logicalParent is ToolMenuItem) ) // JJD 10/22/07 - Check for the parent being a ToolMenuItem also
				{
					DependencyObject containingMenuTool = Utilities.GetAncestorFromType(this, typeof(MenuTool), true);
					if (containingMenuTool == null)
						throw new NotSupportedException(XamRibbon.GetString("LE_NotSupportedException_1"));
				}
			}
		}

			#endregion //OnVisualParentChanged	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ActivationActionDelay

		/// <summary>
		/// Returns/sets the number of milliseconds after a <see cref="GalleryItem"/> is activated before the ItemActivated event is fired.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="ActivationActionDelay"/> and <see cref="ActivationInitialActionDelay"/> properties and <see cref="ItemActivated"/> event can be used to simulate the 
		/// Live Preview functionality found in Office 2007 applications (e.g., Word).  Specify a delay that makes sense for the preview you are doing (e.g., longer delay for more expensive
		/// previews so that your application is not constructing previews everytime a <see cref="GalleryItem"/> is moused over), and handle the <see cref="ItemActivated"/> event to display the preview.</p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">If the property is set to a value that is less than zero.</exception>
		/// <seealso cref="ActivationInitialActionDelay"/>
		/// <seealso cref="ItemActivated"/>
		/// <seealso cref="GalleryItem"/>
		//[Description("Returns/sets the number of milliseconds after a GalleryItem is activated before the ItemActivated event is fired.")]
		//[Category("Ribbon Properties")]
		[DefaultValue(0)]
		public int ActivationActionDelay
		{
			get { return this._activationActionDelay; }
			set
			{
				if (value != this._activationActionDelay)
				{
					if (value < 0)
						throw new ArgumentOutOfRangeException("ActivationActionDelay", XamRibbon.GetString("LE_ArgumentLessThanZero"));

					this._activationActionDelay = value;
				}
			}
		}

				#endregion //ActivationActionDelay	

				#region ActivationInitialActionDelay

		/// <summary>
		/// The number of milliseconds after the first <see cref="GalleryItem"/> is activated before the <see cref="ItemActivated"/> event is fired.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="ActivationActionDelay"/> and <see cref="ActivationInitialActionDelay"/> properties and <see cref="ItemActivated"/> event can be used to simulate the 
		/// Live Preview functionality found in Office 2007 applications (e.g., Word).  Specify a delay that makes sense for the preview you are doing (e.g., longer delay for more expensive
		/// previews so that your application is not constructing previews everytime a <see cref="GalleryItem"/> is moused over), and handle the <see cref="ItemActivated"/> event to display the preview.</p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">If the property is set to a value that is less than zero.</exception>
		/// <seealso cref="ActivationActionDelay"/>
		/// <seealso cref="ItemActivated"/>
		/// <seealso cref="GalleryItem"/>
		//[Description("Returns/sets the number of milliseconds after a GalleryItem is activated before the ItemActivated event is fired.")]
		//[Category("Ribbon Properties")]
		[DefaultValue(-1)]
		public int ActivationInitialActionDelay
		{
			get { return this._activationInitialActionDelay; }
			set
			{
				if (value != this._activationInitialActionDelay)
				{
					if (value < -1)
						throw new ArgumentOutOfRangeException("ActivationInitialActionDelay", XamRibbon.GetString("LE_ArgumentLessThanZero"));

					this._activationInitialActionDelay = value;
				}
			}
		}

				#endregion //ActivationInitialActionDelay	

				#region AllowResizeDropDown

		/// <summary>
		/// Identifies the <see cref="AllowResizeDropDown"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowResizeDropDownProperty = DependencyProperty.Register("AllowResizeDropDown",
			typeof(bool), typeof(GalleryTool), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets a value that indicates whether the <see cref="GalleryTool"/> dropdown can be resized.  If true, a resizer bar will be displayed within the
		/// dropdown to allow resizing in the vertical and/or horizontal dimension (see note below).
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>If the <see cref="MinDropDownColumns"/> and <see cref="MaxDropDownColumns"/> have the same value, the dropdown
		/// will only be resizable vertically.</p>
		/// </remarks>
		/// <seealso cref="AllowResizeDropDownProperty"/>
		/// <seealso cref="MinDropDownColumns"/>
		/// <seealso cref="MaxDropDownColumns"/>
		//[Description("Resturns/sets a value that indicates whether the GalleryTool dropdown can be resized.  ")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool AllowResizeDropDown
		{
			get
			{
				return (bool)this.GetValue(GalleryTool.AllowResizeDropDownProperty);
			}
			set
			{
				this.SetValue(GalleryTool.AllowResizeDropDownProperty, value);
			}
		}

				#endregion //AllowResizeDropDown

				#region Groups

		/// <summary>
		/// Returns a collection of the <see cref="GalleryItemGroup"/>s displayed in the <see cref="GalleryTool"/> dropdown.
		/// </summary>
		/// <remarks>
		/// <p class="body">Adding a <see cref="GalleryItemGroup"/> to this collection will prevent the display of <see cref="GalleryItem"/>s in the 
		/// <see cref="GalleryTool"/> dropdown, unless you add the key of at least 1 <see cref="GalleryItem"/> to the <see cref="GalleryItemGroup.ItemKeys"/> 
		/// collection of the <see cref="GalleryItemGroup"/>.</p>
		/// <p class="note"><b>Note: </b>Once a <see cref="GalleryItemGroup"/> has been added to this collection, the <see cref="GalleryTool"/> will not display 
		/// <see cref="GalleryItem"/>s in the <see cref="GalleryTool"/> dropdown unless they are assigned to <see cref="GalleryItemGroup"/>s by adding the 
		/// <see cref="GalleryItem.Key"/> to the <see cref="GalleryItemGroup.ItemKeys"/> collection of the Group in which the <see cref="GalleryItem "/> should 
		/// be displayed.</p>
		/// <p class="note"><b>Note: </b>Adding <see cref="GalleryItemGroup"/>s to this collection has no effect on the display of <see cref="GalleryItem"/>s in 
		/// the <see cref="GalleryTool"/> preview area, since <see cref="GalleryItemGroup"/>s are never displayed in the preview area</p>
		/// </remarks>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="GalleryItemGroup"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItemGroup.ItemKeys"/>
		/// <seealso cref="MenuTool.ShouldDisplayGalleryPreview"/>
		//[Description("Returns a collection of the GalleryItemGroups displayed in the GalleryTool dropdown.")]
		//[Category("Ribbon Properties")]
		[ReadOnly(true)]
		public ObservableCollection<GalleryItemGroup> Groups
		{
			get
			{
				if (this._groups == null)
				{
					this._groups					= new ObservableCollection<GalleryItemGroup>();
					this._groups.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnGroupsCollectionChanged);
				}

				return this._groups;
			}
		}

				#endregion //Groups	

				#region ItemBehavior

		/// <summary>
		/// Identifies the <see cref="ItemBehavior"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemBehaviorProperty = DependencyProperty.Register("ItemBehavior",
			typeof(GalleryToolItemBehavior), typeof(GalleryTool), new FrameworkPropertyMetadata(GalleryToolItemBehavior.Button));

		/// <summary>
		/// Returns/sets the behavior of <see cref="GalleryItem"/>s when they are clicked.
		/// </summary>
		/// <remarks>
		/// <p class="body">If set to 'Button' the <see cref="GalleryItem"/> is not selected - instead a <see cref="ItemClicked"/> event is raised.
		/// If set to 'StateButton' The <see cref="GalleryItem"/> is selected and <see cref="SelectedItem"/> is set to reflect the selection.  The 
		/// <see cref="ItemSelectedEvent"/> event is raised.</p>
		/// </remarks>
		/// <seealso cref="ItemBehaviorProperty"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="ItemClickedEvent"/>
		/// <seealso cref="ItemSelectedEvent"/>
		//[Description("Returns/sets the behavior of GalleryItems when they are clicked.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public GalleryToolItemBehavior ItemBehavior
		{
			get
			{
				return (GalleryToolItemBehavior)this.GetValue(GalleryTool.ItemBehaviorProperty);
			}
			set
			{
				this.SetValue(GalleryTool.ItemBehaviorProperty, value);
			}
		}

				#endregion //ItemBehavior

				#region Items

		/// <summary>
		/// Returns a collection of the <see cref="GalleryItem"/>s defined in the <see cref="GalleryTool"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">Adding <see cref="GalleryItem"/>(s) to this collection will not automatically display <see cref="GalleryItem"/>(s) in the 
		/// <see cref="GalleryTool"/> dropdown if one or more <see cref="GalleryItemGroup"/>s have been aded to the <see cref="Groups"/> collection.  
		/// You must add the key of at least 1 <see cref="GalleryItem"/> to the <see cref="GalleryItemGroup.ItemKeys"/> 
		/// collection of at least one of the <see cref="GalleryItemGroup"/>(s) in order for the <see cref="GalleryItem"/>(s) added here to be displayed.</p>
		/// <p class="note"><b>Note: </b><see cref="GalleryItem"/>s added here will always be displayed in the <see cref="GalleryTool"/> preview area regardless of whether
		/// <see cref="GalleryItemGroup"/>s have been added to the <see cref="Groups"/> collection.</p>
		/// </remarks>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItemCollection"/>
		/// <seealso cref="Groups"/>
		/// <seealso cref="GalleryItemGroup"/>
		/// <seealso cref="GalleryItemGroup.ItemKeys"/>
		/// <seealso cref="MenuTool.ShouldDisplayGalleryPreview"/>
		//[Description("Returns a collection of the GalleryItems defined in the GalleryTool.")]
		//[Category("Ribbon Properties")]
		[ReadOnly(true)]
		public GalleryItemCollection Items
		{
			get
			{
				if (this._items == null)
				{
					this._items = new GalleryItemCollection();
					this._items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnItemsCollectionChanged);
					this._items.ItemPropertyChanged += new EventHandler<ItemPropertyChangedEventArgs>(OnItemPropertyChanged);
				}

				return this._items;
			}
		}

				#endregion //Items

				#region ItemSettings

		/// <summary>
		/// Identifies the <see cref="ItemSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemSettingsProperty = DependencyProperty.Register("ItemSettings",
			typeof(GalleryItemSettings), typeof(GalleryTool), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemSettingsChanged)));

		private static void OnItemSettingsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			GalleryTool galleryTool = target as GalleryTool;
			if (galleryTool != null)
			{
				if (e.OldValue != null)
					((GalleryItemSettings)e.OldValue).PropertyChanged -= new PropertyChangedEventHandler(galleryTool.OnItemSettingsPropertyChanged);

				if (e.NewValue != null)
					((GalleryItemSettings)e.NewValue).PropertyChanged += new PropertyChangedEventHandler(galleryTool.OnItemSettingsPropertyChanged);
			}
		}

		/// <summary>
		/// Returns/sets the settings that will be used as the default for all <see cref="GalleryItem"/>s.  These settings can be overridden at lower levels.
		/// </summary>
		/// <remarks>
		/// <p class="body">The various property values in the <see cref="GalleryItemSettings"/> specified at the <see cref="GalleryTool"/> level 
		/// (via the <see cref="GalleryTool.ItemSettings"/> property) serve as the ultimate defaults for all <see cref="GalleryItem"/>s.  These values 
		/// can be overridden at two lower levels:
		/// <ul>
		/// <li>at the <see cref="GalleryItemGroup"/> level via the <see cref="GalleryItemGroup.ItemSettings"/> property.  The values specified there
		/// will override corresponding values set at the <see cref="GalleryTool"/> level, but could be further overridden at the <see cref="GalleryItem"/>
		/// level (see next bullet)</li>
		/// <li>at the <see cref="GalleryItem"/> level via the <see cref="GalleryItem.Settings"/> property.  The values specified here will override corresponding 
		/// values set at the <see cref="GalleryTool"/> and <see cref="GalleryItemGroup"/> levels</li>
		/// </ul>
		/// </p>
		/// </remarks>
		/// <seealso cref="ItemSettingsProperty"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItem.Settings"/>
		/// <seealso cref="GalleryItemGroup"/>
		/// <seealso cref="GalleryItemGroup.ItemSettings"/>
		//[Description("Returns/sets the settings that will be used as the default for all GalleryItems in each GalleryItemGroup.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public GalleryItemSettings ItemSettings
		{
			get
			{
				return (GalleryItemSettings)this.GetValue(GalleryTool.ItemSettingsProperty);
			}
			set
			{
				this.SetValue(GalleryTool.ItemSettingsProperty, value);
			}
		}

				#endregion //ItemSettings

				#region MaxDropDownColumns

		/// <summary>
		/// Identifies the <see cref="MaxDropDownColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxDropDownColumnsProperty = DependencyProperty.Register("MaxDropDownColumns",
			typeof(int), typeof(GalleryTool), new FrameworkPropertyMetadata(0));

		/// <summary>
		/// Returns/sets the maximum number of columns that can be displayed in the gallery drop down.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>If the <see cref="MinDropDownColumns"/> and <see cref="MaxDropDownColumns"/> have the same value, and 
		/// <see cref="AllowResizeDropDown"/> is set to true, the dropdown will only be resizable vertically.</p>
		/// </remarks>
		/// <seealso cref="MaxDropDownColumnsProperty"/>
		/// <seealso cref="MinDropDownColumns"/>
		/// <seealso cref="MaxPreviewColumns"/>
		/// <seealso cref="MinPreviewColumns"/>
		/// <seealso cref="AllowResizeDropDown"/>
		//[Description("Returns/sets the maximum number of columns that can be displayed in the gallery drop down.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int MaxDropDownColumns
		{
			get
			{
				return (int)this.GetValue(GalleryTool.MaxDropDownColumnsProperty);
			}
			set
			{
				this.SetValue(GalleryTool.MaxDropDownColumnsProperty, value);
			}
		}

				#endregion //MaxDropDownColumns

				#region MaxPreviewColumns

		/// <summary>
		/// Identifies the <see cref="MaxPreviewColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxPreviewColumnsProperty = DependencyProperty.Register("MaxPreviewColumns",
			typeof(int), typeof(GalleryTool), new FrameworkPropertyMetadata(0));

		/// <summary>
		/// Returns/sets the maximum number of columns that can be displayed in the gallery preview.  
		/// </summary>
		/// <remarks>
		/// <p class="body">The gallery preview (a scrollable list of the <see cref="GalleryItem"/>s contained in the <see cref="GalleryTool"/>) is displayed in a 
		/// <see cref="RibbonGroup"/> when the <see cref="MenuTool"/> containing the <see cref="GalleryTool"/> has its <see cref="MenuTool.ShouldDisplayGalleryPreview"/>
		/// property set to true.</p>
		/// </remarks>
		/// <seealso cref="MaxPreviewColumnsProperty"/>
		/// <seealso cref="MinPreviewColumns"/>
		/// <seealso cref="MaxDropDownColumns"/>
		/// <seealso cref="MinDropDownColumns"/>
		/// <seealso cref="RibbonGroup"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="MenuTool.ShouldDisplayGalleryPreview"/>
		//[Description("Returns/sets the maximum number of columns that can be displayed in the gallery preview.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int MaxPreviewColumns
		{
			get
			{
				return (int)this.GetValue(GalleryTool.MaxPreviewColumnsProperty);
			}
			set
			{
				this.SetValue(GalleryTool.MaxPreviewColumnsProperty, value);
			}
		}

				#endregion //MaxPreviewColumns

				#region MinDropDownColumns

		/// <summary>
		/// Identifies the <see cref="MinDropDownColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinDropDownColumnsProperty = DependencyProperty.Register("MinDropDownColumns",
			typeof(int), typeof(GalleryTool), new FrameworkPropertyMetadata(1));

		/// <summary>
		/// Returns/sets the minimum number of columns that can be displayed in the gallery drop down. 
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>If the <see cref="MinDropDownColumns"/> and <see cref="MaxDropDownColumns"/> have the same value, and 
		/// <see cref="AllowResizeDropDown"/> is set to true, the dropdown will only be resizable vertically.</p>
		/// </remarks>
		/// <seealso cref="MinDropDownColumnsProperty"/>
		/// <seealso cref="MaxDropDownColumns"/>
		/// <seealso cref="MinPreviewColumns"/>
		/// <seealso cref="MaxPreviewColumns"/>
		/// <seealso cref="AllowResizeDropDown"/>
		//[Description("Returns/sets the minimum number of columns that can be displayed in the gallery drop down. ")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int MinDropDownColumns
		{
			get
			{
				return (int)this.GetValue(GalleryTool.MinDropDownColumnsProperty);
			}
			set
			{
				this.SetValue(GalleryTool.MinDropDownColumnsProperty, value);
			}
		}

				#endregion //MinDropDownColumns

				#region MinPreviewColumns

		/// <summary>
		/// Identifies the <see cref="MinPreviewColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinPreviewColumnsProperty = DependencyProperty.Register("MinPreviewColumns",
			typeof(int), typeof(GalleryTool), new FrameworkPropertyMetadata(1));

		/// <summary>
		/// Returns/sets the minimum number of columns that can be displayed in the gallery preview.
		/// </summary>
		/// <remarks>
		/// <p class="body">The gallery preview (a scrollable list of the <see cref="GalleryItem"/>s contained in the <see cref="GalleryTool"/>) is displayed in a 
		/// <see cref="RibbonGroup"/> when the <see cref="MenuTool"/> containing the <see cref="GalleryTool"/> has its <see cref="MenuTool.ShouldDisplayGalleryPreview"/>
		/// property set to true.</p>
		/// </remarks>
		/// <seealso cref="MinPreviewColumnsProperty"/>
		/// <seealso cref="MaxPreviewColumns"/>
		/// <seealso cref="MaxDropDownColumns"/>
		/// <seealso cref="MinDropDownColumns"/>
		/// <seealso cref="RibbonGroup"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="MenuTool.ShouldDisplayGalleryPreview"/>
		//[Description("Returns/sets the minimum number of columns that can be displayed in the gallery preview.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int MinPreviewColumns
		{
			get
			{
				return (int)this.GetValue(GalleryTool.MinPreviewColumnsProperty);
			}
			set
			{
				this.SetValue(GalleryTool.MinPreviewColumnsProperty, value);
			}
		}

				#endregion //MinPreviewColumns

				#region PreferredDropDownColumns

		/// <summary>
		/// Identifies the <see cref="PreferredDropDownColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredDropDownColumnsProperty = DependencyProperty.Register("PreferredDropDownColumns",
			typeof(int), typeof(GalleryTool), new FrameworkPropertyMetadata(1));

		/// <summary>
		/// Returns/sets the preferred number of columns that should be displayed in the gallery dropdown.
		/// </summary>
		/// <remarks>
		/// <p class="body">The value specified for <b>PreferredDropDownColumns</b> is used when a gallery preview is not being displayed (as determined
		/// by the setting of <see cref="MenuTool.ShouldDisplayGalleryPreview"/> property on the <see cref="MenuTool"/> containing the <see cref="GalleryTool"/>)
		/// and therefore the the width of gallery dropdown is not being constrained.</p>
		/// </remarks>
		/// <seealso cref="PreferredDropDownColumnsProperty"/>
		/// <seealso cref="MaxPreviewColumns"/>
		/// <seealso cref="MinPreviewColumns"/>
		/// <seealso cref="MaxDropDownColumns"/>
		/// <seealso cref="MinDropDownColumns"/>
		/// <seealso cref="MenuTool.ShouldDisplayGalleryPreview"/>
		//[Description("Returns/sets the preferred number of columns that should be displayed in the gallery drop down.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int PreferredDropDownColumns
		{
			get
			{
				return (int)this.GetValue(GalleryTool.PreferredDropDownColumnsProperty);
			}
			set
			{
				this.SetValue(GalleryTool.PreferredDropDownColumnsProperty, value);
			}
		}

				#endregion //PreferredDropDownColumns

				#region PreviewItems

		private static readonly DependencyPropertyKey PreviewItemsPropertyKey =
			DependencyProperty.RegisterReadOnly("PreviewItems",
			typeof(ReadOnlyObservableCollection<GalleryItem>), typeof(GalleryTool), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="PreviewItems"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreviewItemsProperty =
			PreviewItemsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a read-only list off all the <see cref="GalleryItem"/>s defined in the <see cref="GalleryTool"/>. It is used to show the 
		/// <see cref="GalleryItem"/>s in the preview area.
		/// </summary>
		//[Description("Returns a read-only list off all the GalleryItems defined in the GalleryTool. It is used to show the items in the preview area.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public ReadOnlyObservableCollection<GalleryItem> PreviewItems
		{
			get
			{
				return (ReadOnlyObservableCollection<GalleryItem>)this.GetValue(GalleryTool.PreviewItemsProperty);
			}
		}

				#endregion //PreviewItems

				#region SelectedItem

		/// <summary>
		/// Identifies the <see cref="SelectedItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
			typeof(GalleryItem), typeof(GalleryTool), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemChanged), new CoerceValueCallback(OnCoerceSelectedItem)));

		private static object OnCoerceSelectedItem(DependencyObject target, object value)
		{
            
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


			return value;
		}

		private static void OnSelectedItemChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			GalleryTool galleryTool		= target as GalleryTool;
			GalleryItem newSelectedItem	= e.NewValue as GalleryItem;

            // AS 3/10/09 TFS15169
            GalleryItem oldSelectedItem = e.OldValue as GalleryItem;
            if (null != oldSelectedItem)
            {
                oldSelectedItem.IsSelected = false;
            }

            if (galleryTool != null && newSelectedItem != null  &&  galleryTool._ignoreSelectedItemChanged == false)
            {
                // AS 3/10/09 TFS15169
                ////galleryTool.SetSelectedItem(newSelectedItem, null);
                //galleryTool.SetSelectedItem(newSelectedItem, null, null); // JM 11-12-07
                galleryTool.SetSelectedItem(newSelectedItem, null, null, true);
            }
        }

		/// <summary>
		/// Returns/sets the currently selected <see cref="GalleryItem"/> or null if no <see cref="GalleryItem"/> is currently selected.
		/// </summary>
		/// <remarks>
		/// <p class="body">While this read-write property can be set directly in code or XAML, it will be automatically set if the the <see cref="ItemBehavior"/>
		/// property is set to 'StateButton' and the user clicks on a <see cref="GalleryItem"/>.  </p>
		/// <p class="note"><b>Note: </b>The <see cref="GalleryItemSettings.SelectionDisplayMode"/> property determines what part (if any) of the <see cref="GalleryItem"/>
		/// is visually highlighted when it is selected.</p>
		/// </remarks>
		/// <seealso cref="SelectedItemProperty"/>
		/// <seealso cref="ItemBehavior"/>
		/// <seealso cref="ItemSelectedEvent"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItemSettings.SelectionDisplayMode"/>
		/// <seealso cref="GalleryItemSelectionDisplayMode"/>
		//[Description("Returns/sets the currently selected GalleryItem or null if no GalleryItem is currently selected.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public GalleryItem SelectedItem
		{
			get
			{
				return (GalleryItem)this.GetValue(GalleryTool.SelectedItemProperty);
			}
			set
			{
				this.SetValue(GalleryTool.SelectedItemProperty, value);
			}
		}

				#endregion //SelectedItem

			#endregion //Public Properties

			#region Common Tool Properties

				#region Caption

		/// <summary>
		/// Identifies the Caption dependency property.
		/// </summary>
		/// <seealso cref="Caption"/>
		/// <seealso cref="HasCaptionProperty"/>
		/// <seealso cref="HasCaption"/>
		public static readonly DependencyProperty CaptionProperty = RibbonToolHelper.CaptionProperty.AddOwner(typeof(GalleryTool));

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
				return (string)this.GetValue(GalleryTool.CaptionProperty);
			}
			set
			{
				this.SetValue(GalleryTool.CaptionProperty, value);
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
		public static readonly DependencyProperty HasCaptionProperty = RibbonToolHelper.HasCaptionProperty.AddOwner(typeof(GalleryTool));

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
				return (bool)this.GetValue(GalleryTool.HasCaptionProperty);
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
		public static readonly DependencyProperty HasImageProperty = RibbonToolHelper.HasImageProperty.AddOwner(typeof(GalleryTool));

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
				return (bool)this.GetValue(GalleryTool.HasImageProperty);
			}
		}

				#endregion //HasImage

				#region Id

		/// <summary>
		/// Identifies the Id dependency property.
		/// </summary>
		/// <seealso cref="Id"/>
		public static readonly DependencyProperty IdProperty = RibbonToolHelper.IdProperty.AddOwner(typeof(GalleryTool));

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
				return (string)this.GetValue(GalleryTool.IdProperty);
			}
			set
			{
				this.SetValue(GalleryTool.IdProperty, value);
			}
		}

				#endregion //Id

				#region ImageResolved

		/// <summary>
		/// Identifies the <see cref="ImageResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageResolvedProperty = RibbonToolHelper.ImageResolvedProperty.AddOwner(typeof(GalleryTool));

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
				return (ImageSource)this.GetValue(GalleryTool.ImageResolvedProperty);
			}
		}
	
				#endregion //ImageResolved

				#region IsActive

		/// <summary>
		/// Identifies the IsActive dependency property.
		/// </summary>
		/// <seealso cref="IsActive"/>
		public static readonly DependencyProperty IsActiveProperty = XamRibbon.IsActiveProperty.AddOwner(typeof(GalleryTool));

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
				return (bool)this.GetValue(GalleryTool.IsActiveProperty);
			}
		}

				#endregion //IsActive

				#region IsQatCommonTool

		/// <summary>
		/// Identifies the IsQatCommonTool dependency property.
		/// </summary>
		/// <seealso cref="IsQatCommonTool"/>
		public static readonly DependencyProperty IsQatCommonToolProperty = RibbonToolHelper.IsQatCommonToolProperty.AddOwner(typeof(GalleryTool));

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
				return (bool)this.GetValue(GalleryTool.IsQatCommonToolProperty);
			}
			set
			{
				this.SetValue(GalleryTool.IsQatCommonToolProperty, value);
			}
		}

				#endregion //IsQatCommonTool

				#region IsOnQat

		/// <summary>
		/// Identifies the IsOnQat dependency property.
		/// </summary>
		/// <seealso cref="IsOnQat"/>
		public static readonly DependencyProperty IsOnQatProperty = RibbonToolHelper.IsOnQatProperty.AddOwner(typeof(GalleryTool));

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
				return (bool)this.GetValue(GalleryTool.IsOnQatProperty);
			}
		}

				#endregion //IsOnQat

				#region KeyTip

		/// <summary>
		/// Identifies the KeyTip dependency property.
		/// </summary>
		/// <seealso cref="KeyTip"/>
		public static readonly DependencyProperty KeyTipProperty = RibbonToolHelper.KeyTipProperty.AddOwner(typeof(GalleryTool));

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
				return (string)this.GetValue(GalleryTool.KeyTipProperty);
			}
			set
			{
				this.SetValue(GalleryTool.KeyTipProperty, value);
			}
		}

				#endregion //KeyTip

				#region LargeImage

		/// <summary>
		/// Identifies the LargeImage dependency property.
		/// </summary>
		/// <seealso cref="LargeImage"/>
		public static readonly DependencyProperty LargeImageProperty = RibbonToolHelper.LargeImageProperty.AddOwner(typeof(GalleryTool));

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
				return (ImageSource)this.GetValue(GalleryTool.LargeImageProperty);
			}
			set
			{
				this.SetValue(GalleryTool.LargeImageProperty, value);
			}
		}

				#endregion //LargeImage

				#region Location

		/// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		/// <seealso cref="Location"/>
		public static readonly DependencyProperty LocationProperty = XamRibbon.LocationProperty.AddOwner(typeof(GalleryTool));

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
				return (ToolLocation)this.GetValue(GalleryTool.LocationProperty);
			}
		}

				#endregion //Location

				#region SizingMode

		/// <summary>
		/// Identifies the SizingMode dependency property.
		/// </summary>
		/// <seealso cref="SizingMode"/>
		public static readonly DependencyProperty SizingModeProperty = RibbonToolHelper.SizingModeProperty.AddOwner(typeof(GalleryTool));

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
				return (RibbonToolSizingMode)this.GetValue(GalleryTool.SizingModeProperty);
			}
		}

				#endregion //SizingMode

				#region SmallImage

		/// <summary>
		/// Identifies the SmallImage dependency property.
		/// </summary>
		/// <seealso cref="SmallImage"/>
		public static readonly DependencyProperty SmallImageProperty = RibbonToolHelper.SmallImageProperty.AddOwner(typeof(GalleryTool));

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
				return (ImageSource)this.GetValue(GalleryTool.SmallImageProperty);
			}
			set
			{
				this.SetValue(GalleryTool.SmallImageProperty, value);
			}
		}

				#endregion //SmallImage

			#endregion //Common Tool Properties

			#region Internal Properties

                // JJD 11/20/07 - BR27066
                #region IgnoreMouseEnterLeave

        // JJD 11/20/07 - BR27066
        // Added flag to bypass mouse enter processing when navigating via the keyboard
        internal bool IgnoreMouseEnter
        {
            get { return this._ignoreMouseEnter; }
            set { this._ignoreMouseEnter = value; }
         }

        internal void ResetIgnoreMouseEnter()
        {
            this._ignoreMouseEnter = false;
        }

                #endregion //IgnoreMouseEnterLeave	

                // JJD 11/21/07 - BR27066
                // Keep track of whether at least one activation event was raised during a dropdown
                #region ItemActivatedEventFiredOnceDuringDropdown

        internal bool ItemActivatedEventFiredOnceDuringDropdown { get { return this._itemActivatedEventFiredOnceDuringDropdown; } }

                #endregion //ItemActivatedEventFiredOnceDuringDropdown	
            
				#region ItemSettingsVersion

		private static readonly DependencyPropertyKey ItemSettingsVersionPropertyKey =
			DependencyProperty.RegisterReadOnly("ItemSettingsVersion",
			typeof(int), typeof(GalleryTool), new FrameworkPropertyMetadata(0));

		internal static readonly DependencyProperty ItemSettingsVersionProperty =
			ItemSettingsVersionPropertyKey.DependencyProperty;

				#endregion //ItemSettingsVersion

				#region MaxColumnSpan

		private static readonly DependencyPropertyKey MaxColumnSpanPropertyKey =
			DependencyProperty.RegisterReadOnly("MaxColumnSpan",
			typeof(int), typeof(GalleryTool), new FrameworkPropertyMetadata(1));

		internal static readonly DependencyProperty MaxColumnSpanProperty =
			MaxColumnSpanPropertyKey.DependencyProperty;

				#endregion //MaxColumnSpan

				#region MaxPossiblePreviewColumns

		internal static readonly DependencyPropertyKey MaxPossiblePreviewColumnsPropertyKey =
			DependencyProperty.RegisterReadOnly("MaxPossiblePreviewColumns",
			typeof(int), typeof(GalleryTool), new FrameworkPropertyMetadata(0));

		internal static readonly DependencyProperty MaxPossiblePreviewColumnsProperty =
			MaxPossiblePreviewColumnsPropertyKey.DependencyProperty;

				#endregion //MaxPossiblePreviewColumns

                // JJD 11/28/07 - BR27268
                // Added member to cache the mesured size of an item.
                #region MeasuredItemSize

         internal Size MeasuredItemSize
        {
            get { return this._measuredItemSize; }
            set { this._measuredItemSize = value; }
        }

                #endregion //MeasuredItemSize	
 
			#endregion //Internal Properties

			#region Private Properties

				#region ActivationInitialActionDelayResolved

		private int ActivationInitialActionDelayResolved
		{
			get
			{
				if (this.ActivationInitialActionDelay > -1)
					return this.ActivationInitialActionDelay;
				else
					return this.ActivationActionDelay;
			}
		}

				#endregion //ActivationInitialActionDelayResolved	

				#region CurrentActiveItem

		internal GalleryItem CurrentActiveItem
		{
			get { return this._currentActiveItem; }
		}

				#endregion //CurrentActiveItem	
    
				#region CurrentActiveItemArea

		private ItemsControl CurrentActiveItemArea
		{
			get { return this._currentActiveItemArea; }
		}

				#endregion //CurrentActiveItemArea	
    
				#region DropDownPresenter

		private GalleryToolDropDownPresenter DropDownPresenter
		{
			get
			{
                if (this._dropDownPresenter == null)
                {
                    this._dropDownPresenter = new GalleryToolDropDownPresenter(this);

                    // JJD 11/07/07 - added properties to support triggering of separators
                    // Initialize IsFirstInMenu and IsLastInMenu properties
                   if ( this._isFirstInMenu )
                        this._dropDownPresenter.SetValue(GalleryToolDropDownPresenter.IsFirstInMenuPropertyKey, KnownBoxes.TrueBox);
                    
                    if ( this._isLastInMenu )
                        this._dropDownPresenter.SetValue(GalleryToolDropDownPresenter.IsLastInMenuPropertyKey, KnownBoxes.TrueBox);
                }

				return this._dropDownPresenter;
			}
		}

				#endregion //DropDownPresenter	

				#region IsInInitialActivationState

		private bool IsInInitialActivationState
		{
			get { return this._isInInitialActivationState; }
			set { this._isInInitialActivationState = value; }
		}

				#endregion //IsInInitialActivationState

				#region IsItemActivationPending

		private bool IsItemActivationPending
		{
			get { return this._itemActivationDelayTimer != null && this._itemActivationDelayTimer.IsEnabled; }
		}

				#endregion //IsItemActivationPending	

				#region ItemActivationDelayTimer

		private DispatcherTimer ItemActivationDelayTimer
		{
			get
			{
				if (this._itemActivationDelayTimer == null)
				{
					this._itemActivationDelayTimer = new DispatcherTimer(DispatcherPriority.Render);

					this._itemActivationDelayTimer.Tick += new EventHandler(OnItemActivationDelayTimerTick);
				}

				return this._itemActivationDelayTimer;
			}
		}

				#endregion //ItemActivationDelayTimer	
    
				#region ItemPresenterAwaitingActivation

		private GalleryItemPresenter ItemPresenterAwaitingActivation
		{
			get
			{
				if (this._itemActivationDelayTimer == null)
					return null;

				return this._itemActivationDelayTimer.Tag as GalleryItemPresenter;
			}
		}

				#endregion //ItemPresenterAwaitingActivation	
    
			#endregion //Private Properties

		#endregion //Properties

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


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            RibbonToolHelper.SetToolVisualState(this, useTransitions, this.IsActive);

        }

        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            GalleryTool tool = target as GalleryTool;

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

			#region Private Methods

                // JJD 11/20/07 - BR27066
                #region ClearIsHighlighted

        private void ClearIsHighlighted()
        {
            // JJD 11/20/07 - BR27066
            // Added IsHighlighted property to support keyboard navigation
            // clear the IsHighlighted property from the last active iitem presenter
            if (this._currentActiveItemPresenter != null)
            {
                this._currentActiveItemPresenter.SetIsHighlighted(false);
                this._currentActiveItemPresenter = null;
            }
        }

                #endregion //ClearIsHighlighted	
    
				#region OnGroupsCollectionChanged

		private void OnGroupsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						((GalleryItemGroup)e.NewItems[i]).GalleryToolInternal = this;
					}

					break;
			}


			this.SetDropDownPresenterItemsSource();
		}

				#endregion //OnGroupsCollectionChanged	

				#region OnItemActivationDelayTimerTick

		private void OnItemActivationDelayTimerTick(object sender, EventArgs e)
		{
			this.IsInInitialActivationState = false;
			this.ItemActivationDelayTimer.Stop();

			// JJD 12/9/11 - added internal ctor that takes GalleryItemPresenter so we can expose it for TestAdvantage use
			//GalleryItemEventArgs args = new GalleryItemEventArgs(this, this.ItemPresenterAwaitingActivation.GalleryItemGroup, this.ItemPresenterAwaitingActivation.Item);
			GalleryItemEventArgs args = new GalleryItemEventArgs(this, this.ItemPresenterAwaitingActivation.GalleryItemGroup, this.ItemPresenterAwaitingActivation.Item, this.ItemActivationDelayTimer.Tag as GalleryItemPresenter);
			this.RaiseItemActivated(args);
		}

				#endregion //OnItemActivationDelayTimerTick	
    
				#region OnItemsCollectionChanged

		private void OnItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			bool recalcMaxColSpan = true;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					// update inline
					recalcMaxColSpan = false;

					int maxSpan = 1;

					for (int i = 0, count = e.NewItems.Count; i < count; i++)
					{
						GalleryItem item = (GalleryItem)e.NewItems[i];
						this._previewItemsInternal.Insert(e.NewStartingIndex + i, item);

						if (item.ColumnSpan > maxSpan)
							maxSpan = item.ColumnSpan;

						// JM 01-02-08 BR29325
						if (item.IsSelected)
							this.SetSelectedItem(item, null, null);

						// JM 10-23-08 TFS7560
						item.GalleryToolInternal = this;
					}

					if (maxSpan > (int)this.GetValue(MaxColumnSpanProperty))
						this.SetValue(MaxColumnSpanPropertyKey, maxSpan);

					break;

				case NotifyCollectionChangedAction.Move:
					// only moving items within the collection
					recalcMaxColSpan = false;

					for (int i = 0, count=e.NewItems.Count; i < count; i++)
					{
						this._previewItemsInternal.RemoveAt(e.OldStartingIndex);
						this._previewItemsInternal.Insert(e.NewStartingIndex + i, (GalleryItem)e.NewItems[i]);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					for (int i = 0, count = e.OldItems.Count; i < count; i++)
					{
						this._previewItemsInternal.RemoveAt(e.OldStartingIndex);
					}
					break;

				case NotifyCollectionChangedAction.Replace:
					for (int i = 0, count = e.NewItems.Count; i < count; i++)
					{
						this._previewItemsInternal[e.NewStartingIndex + i] = (GalleryItem)e.NewItems[i];
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					this._previewItemsInternal.Clear();
					for (int i = 0, count = this._items.Count; i < count; i++)
						this._previewItemsInternal.Add(this._items[i]);
					break;
			}

			if (recalcMaxColSpan)
				this.VerifyMaxColumnSpan();
		}

				#endregion //OnItemsCollectionChanged

				#region OnItemPropertyChanged
		private void OnItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ColumnSpan")
			{
				this.VerifyMaxColumnSpan();
			}
		} 
				#endregion //OnItemPropertyChanged

				#region OnItemSettingsPropertyChanged

		private void OnItemSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			int currentVersion	= (int)this.GetValue(GalleryTool.ItemSettingsVersionProperty);
			int newVersion		= ++currentVersion;
			this.SetValue(GalleryTool.ItemSettingsVersionPropertyKey, newVersion);
		}

				#endregion //OnItemSettingsPropertyChanged	
    
				#region SetDropDownPresenterItemsSource

		private void SetDropDownPresenterItemsSource()
		{
			if (this._dropDownPresenter != null)
			{
				if (this.Groups.Count > 0)
				{
					this._dropDownPresenter.DropDownItemsType	= GalleryToolDropDownPresenter.ItemsType.Groups;
					this._dropDownPresenter.ItemsSource			= this.Groups;
				}
				else
				{
					this._dropDownPresenter.DropDownItemsType	= GalleryToolDropDownPresenter.ItemsType.Items;
					this._dropDownPresenter.ItemsSource			= this.Items;
				}
			}
		}

				#endregion //SetDropDownPresenterItemsSource	

				#region StartItemActivationDelayTimer

		private void StartItemActivationDelayTimer(GalleryItemPresenter galleryItemPresenter)
		{
			if (this.ItemActivationDelayTimer.IsEnabled)
				this.ItemActivationDelayTimer.Stop();

			if (this.IsInInitialActivationState)
				this.ItemActivationDelayTimer.Interval = TimeSpan.FromMilliseconds(this.ActivationInitialActionDelayResolved);
			else
				this.ItemActivationDelayTimer.Interval = TimeSpan.FromMilliseconds(this.ActivationActionDelay);

			this.ItemActivationDelayTimer.Tag = galleryItemPresenter;

			this.ItemActivationDelayTimer.Start();
		}

				#endregion //StartItemActivationDelayTimer	
    
				#region VerifyMaxColumnSpan
		private void VerifyMaxColumnSpan()
		{
			if (this.IsInitialized == false)
				return;

			int maxSpan = 1;

			if (null != this._items)
			{
				for (int i = 0, count = this._items.Count; i < count; i++)
				{
					int span = this._items[i].ColumnSpan;

					if (span > maxSpan)
						maxSpan = span;
				}
			}

			this.SetValue(MaxColumnSpanPropertyKey, maxSpan);
		} 
				#endregion //VerifyMaxColumnSpan

				#region VerifyParts

		private void VerifyParts()
		{
			// AS 11/14/07 BR28450
			// A couple of changes were required here. First, we should clear the content of the old content
			// presenter since the template could be changed to have a different content presenter. Second,
			// If the template did change, we were not setting the content of the new content presenter
			// to the dropdown presenter. Lastly, we have to change the part to be a content control since 
			// otherwise the namescope will be broken.
			//

			// DropDownPresenterSite
			if (this._dropDownPresenterSite != null && this._dropDownPresenterSite.Content == this._dropDownPresenter)
				this._dropDownPresenterSite.Content = null;

			this._dropDownPresenterSite = base.GetTemplateChild("PART_DropDownPresenterSite") as ContentControl;

			if (this._dropDownPresenterSite != null && this._dropDownPresenterSite.Content == null)
			{
                // JJD 11/20/07 - BR27066
                // Set the site's focusable property to false so it doesn't interfere with the normal focus flow
                this._dropDownPresenterSite.Focusable = false;

				this._dropDownPresenterSite.Content = this.DropDownPresenter;
				this.SetDropDownPresenterItemsSource();
			}
		}

				#endregion //VerifyParts	
    
			#endregion //Private Methods

			#region Internal Methods

                // JJD 11/20/07 - BR27066
                #region ClearActiveItem

		// JJD 12/9/11 - added GalleryItemPresenter so we can expose it for TestAdvantage use
		//internal void ClearActiveItem()
        internal void ClearActiveItem(GalleryItemPresenter presenter)
        {
            // JJD 11/20/07 - BR27066
            // clear the IsHighlighted property from the last active iitem presenter
            this.ClearIsHighlighted();

            // JM 10-26-07 BR27763
            //if (this._currentActiveItem != null)
			if (this._currentActiveItem != null &&
				this._ignoreNextMouseLeaveItemArea == false &&
				this._itemActivatedEventFired == true)	// JM 11-12-07
			{
				// JJD 12/9/11 - added internal ctor that takes GalleryItemPresenter so we can expose it for TestAdvantage use
				//this.RaiseItemActivated(new GalleryItemEventArgs(this, null, null));
				this.RaiseItemActivated(new GalleryItemEventArgs(this, null, null, presenter));
			}

            this._isInInitialActivationState = false;
            this._currentActiveItemArea = null;
            this._currentActiveItem = null;
            this._ignoreNextMouseLeaveItemArea = false;	// JM 11-12-07
            this._itemActivatedEventFired = false;	// JM 11-12-07
        }

                #endregion //ClearActiveItem	

				#region OnItemClicked

		// JJD 12/9/11 - added GalleryItemPresenter so we can expose it for TestAdvantage use
		//internal void OnItemClicked(GalleryItem galleryItem, GalleryItemGroup galleryItemGroup)
		internal void OnItemClicked(GalleryItem galleryItem, GalleryItemGroup galleryItemGroup, GalleryItemPresenter presenter)
		{
			// AS 10/25/07 BR27690
			XamRibbon ribbon = XamRibbon.GetRibbon(this);

			if (null != ribbon)
				ribbon.TransferFocusOutOfRibbon();

			// JJD 12/9/11 - added GalleryItemPresenter so we can expose it for TestAdvantage use
			//GalleryItemEventArgs args = new GalleryItemEventArgs(this, galleryItemGroup, galleryItem);
			GalleryItemEventArgs args = new GalleryItemEventArgs(this, galleryItemGroup, galleryItem, presenter);
			this.RaiseItemClicked(args);
		}

				#endregion //OnItemClicked

                // JJD 11/20/07 - BR27066
				#region OnIsKeyboardFocusWithinItemChanged

        internal void OnIsKeyboardFocusWithinItemChanged(GalleryItemPresenter galleryItemPresenter)
		{
            // if the item is losing focus then we don't have to do anything
            if (galleryItemPresenter.IsKeyboardFocusWithin == false)
                return;

            // if the item is already the active item then return
            if (this._currentActiveItem == galleryItemPresenter.Item)
                return;

            // JJD 11/20/07 - BR27066
            // clear the IsHighlighted property from the last active iitem presenter
            this.ClearIsHighlighted();

			this._currentActiveItem = galleryItemPresenter.Item;

            // JJD 11/20/07 - BR27066
            // Added IsHighlighted property to support keyboard navigation
            this._currentActiveItemPresenter = galleryItemPresenter;
            this._currentActiveItemPresenter.SetIsHighlighted(true);

			// JJD 12/9/11 - added GalleryItemPresenter so we can expose it for TestAdvantage use
			//GalleryItemEventArgs args = new GalleryItemEventArgs(this, this._currentActiveItemPresenter.GalleryItemGroup, this._currentActiveItem);
			GalleryItemEventArgs args = new GalleryItemEventArgs(this, this._currentActiveItemPresenter.GalleryItemGroup, this._currentActiveItem, galleryItemPresenter);
            this.RaiseItemActivated(args);
        }

				#endregion //OnIsKeyboardFocusWithinItemChanged	

                // JJD 11/20/07 - BR27066
                #region OnMenuClosed

        internal void OnMenuClosed()
        {
            // JJD 11/20/07 - BR27066
            // clear the active item
			// JJD 12/9/11 - added GalleryItemPresenter so we can expose it for TestAdvantage use
			//this.ClearActiveItem();
            this.ClearActiveItem(_currentActiveItemPresenter);
            this.ResetIgnoreMouseEnter();

            // JJD 11/21/07 - BR27066
            // Keep track of whether at least one activation event was raised during a dropdown
		    this._itemActivatedEventFiredOnceDuringDropdown = false;
         
        }

                #endregion //OnMenuClosed	

				#region OnMouseEnterItem

		internal void OnMouseEnterItem(GalleryItemPresenter galleryItemPresenter)
		{
            // JJD 11/20/07 - BR27066
            // Check the ignore flag
            if (this._ignoreMouseEnter)
                return;

			// JM 11-07-07 Bug/WorkItem 682 - Turns out that we do not necessarily get the OnMouseEnterItemArea before we get the OnMouseEnterItem.
			// This was causing the timer to be started with this._isInInitialActivateState = false.
			// To ensure that this._isInInitialActivateState gets set to true before we start the delay timer on the first item entered, set it here if 
			// this._currentActiveItem is null (which essentially means that this is the first item that the mouse has entered since entering the ItemArea)
			if (this._currentActiveItem == null)
				this._isInInitialActivationState = true;
            
            // JJD 11/20/07 - BR27066
            // clear the IsHighlighted property from the last active iitem presenter
            this.ClearIsHighlighted();

			// JM 10-26-07 BR27763
			this._currentActiveItem = galleryItemPresenter.Item;

            // JJD 11/20/07 - BR27066
            // Added IsHighlighted property to support keyboard navigation
            this._currentActiveItemPresenter = galleryItemPresenter;
            this._currentActiveItemPresenter.SetIsHighlighted(true);

            // JJD 11/20/07 - BR27066
            // If the presenter is in the dropdown (i.e.. not in the prview area) then give it focus
            if (this._currentActiveItemPresenter.IsInPreviewArea == false)
                Keyboard.Focus(this._currentActiveItemPresenter);

			this.StartItemActivationDelayTimer(galleryItemPresenter);
		}

				#endregion //OnMouseEnterItem	

				#region OnMouseEnterItemArea

		internal void OnMouseEnterItemArea(ItemsControl galleryItemArea)
		{
			this._isInInitialActivationState	= true;
			this._currentActiveItemArea			= galleryItemArea;
			// JM 10-26-07 BR27763
			//this._currentActiveItem			= null;
		}

				#endregion //OnMouseEnterItemArea	
    
				#region OnMouseLeaveItem

		internal void OnMouseLeaveItem(GalleryItemPresenter galleryItemPresenter)
		{
			this.ItemActivationDelayTimer.Stop();
		}

				#endregion //OnMouseLeaveItem	

				#region OnMouseLeaveItemArea

		internal void OnMouseLeaveItemArea(ItemsControl galleryItemArea)
		{

            // JJD 11/20/07 - BR27066
            // When the mouse leaves the item area set focus to the
            // containing tool menu item
            if (this._currentActiveItemPresenter != null &&
                 this._currentActiveItemPresenter.IsKeyboardFocusWithin &&
                 !this._currentActiveItemPresenter.IsInPreviewArea)
            {
                ToolMenuItem tmi = Utilities.GetAncestorFromType(this._currentActiveItemPresenter, typeof(ToolMenuItem), true) as ToolMenuItem;

                if (tmi != null)
                    tmi.Focus();

            }

            // JJD 11/20/07 - BR27066
            // Moved to internal helper method
            #region Old code - moved to ClearActiveItem method

            // JM 10-26-07 BR27763
            //if (this._currentActiveItem != null)
            //if (this._currentActiveItem				!= null		&& 
            //    this._ignoreNextMouseLeaveItemArea	== false	&&
            //    this._itemActivatedEventFired		== true)	// JM 11-12-07
            //    this.RaiseItemActivated(new GalleryItemEventArgs(this, null, null));

            //this._isInInitialActivationState	= false;
            //this._currentActiveItemArea			= null;
            //this._currentActiveItem				= null;
            //this._ignoreNextMouseLeaveItemArea	= false;	// JM 11-12-07
            //this._itemActivatedEventFired		= false;	// JM 11-12-07            

            #endregion //Old code - moved to ClearActiveItem method

			// JJD 12/9/11 - added GalleryItemPresenter so we can expose it for TestAdvantage use
			//this.ClearActiveItem();
            this.ClearActiveItem(_currentActiveItemPresenter);

		}

				#endregion //OnMouseEnterLeaveArea	
 
                // JJD 11/07/07 - added properties to support triggering of separators
                #region SetFirstLastProps

        // Set IsFirstInMenu and IsLastInMenu properties
        internal void SetFirstLastProps(bool isFirstInMenu, bool isLastInMenu)
        {
            // JJD 11/07/07 - added properties to support triggering of separators
            // see if value has changed
            if (isFirstInMenu != this._isFirstInMenu)
            {
                // cache the value
                this._isFirstInMenu = isFirstInMenu;

                // update the dropdownpresenter's properties 
                if (this._dropDownPresenter != null)
                {
                    if (this._isFirstInMenu)
                        this._dropDownPresenter.SetValue(GalleryToolDropDownPresenter.IsFirstInMenuPropertyKey, KnownBoxes.TrueBox);
                    else
                        this._dropDownPresenter.ClearValue(GalleryToolDropDownPresenter.IsFirstInMenuPropertyKey);
                }
            }

            // JJD 11/07/07 - added properties to support triggering of separators
            // see if value has changed
            if (isLastInMenu != this._isLastInMenu)
            {
                // cache the value
                this._isLastInMenu = isLastInMenu;

                // update the dropdownpresenter's properties 
                if (this._dropDownPresenter != null)
                {
                    if (this._isLastInMenu)
                        this._dropDownPresenter.SetValue(GalleryToolDropDownPresenter.IsLastInMenuPropertyKey, KnownBoxes.TrueBox);
                    else
                        this._dropDownPresenter.ClearValue(GalleryToolDropDownPresenter.IsLastInMenuPropertyKey);
                }
            }
        }

                #endregion //SetFirstLastProps	
    
				#region SetSelectedItem

//		internal void SetSelectedItem(GalleryItem galleryItem, GalleryItemGroup galleryItemGroup)
        internal void SetSelectedItem(GalleryItem galleryItem, GalleryItemGroup galleryItemGroup, GalleryItemPresenter galleryItemPresenter) // JM 11-12-07
        {
            // AS 3/10/09 TFS15169
            this.SetSelectedItem(galleryItem, galleryItemGroup, galleryItemPresenter, false);
        }

        // AS 3/10/09 TFS15169
        // Added overload so we can ignore that the SelectedItem is set if being called from the 
        // SelectedItem setter when we are not setting it.
        //
        private void SetSelectedItem(GalleryItem galleryItem, GalleryItemGroup galleryItemGroup, GalleryItemPresenter galleryItemPresenter, bool force)
		{
            // AS 3/10/09 TFS15169
            //// JM 10-23-08 TFS7560
			//if (galleryItem == this.SelectedItem)
			if (galleryItem == this.SelectedItem && force == false)
				return;


			this._ignoreSelectedItemChanged	= true;
			this.SelectedItem				= galleryItem;
			this._ignoreSelectedItemChanged	= false;

			if (galleryItem != null)
				galleryItem.IsSelected = true;

			// AS 10/25/07 BR27690
			XamRibbon ribbon = XamRibbon.GetRibbon(this);

			if (null != ribbon)
			{
				// JM 11-12-07 - avoid firing the ItemActivated event (with a null item) when a GalleryItem is selected in a DropDownPreviewPresenter.  
				// The ItemActivated event (with null item) is fired in the OnMouseLeaveItemArea which ends up getting called indirectly as a result of
				// transferring focus out of the ribbon.
				//ribbon.TransferFocusOutOfRibbon();
				if (galleryItemPresenter != null)
				{
					GalleryToolDropDownPresenter gtdp = Utilities.GetAncestorFromType(galleryItemPresenter, typeof(GalleryToolDropDownPresenter),  true, XamRibbon.GetRibbon(this)) as GalleryToolDropDownPresenter;

					if (gtdp != null)
					{
						this._ignoreNextMouseLeaveItemArea = true;
						ribbon.TransferFocusOutOfRibbon();
						this._ignoreNextMouseLeaveItemArea = false;
					}
					else
						ribbon.TransferFocusOutOfRibbon();

				}
				else
					ribbon.TransferFocusOutOfRibbon();
			}

			// JJD 12/9/11 - added GalleryItemPresenter so we can expose it for TestAdvantage use
			//GalleryItemEventArgs args = new GalleryItemEventArgs(this, galleryItemGroup, galleryItem);
			GalleryItemEventArgs args = new GalleryItemEventArgs(this, galleryItemGroup, galleryItem, galleryItemPresenter);
			this.RaiseItemSelected(args);
		}

				#endregion //SetSelectedItem	
    
			#endregion //Internal Methods

		#endregion //Methods

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
		internal static readonly RoutedEvent ActivatedEvent = MenuTool.ActivatedEvent.AddOwner(typeof(GalleryTool));

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
		public static readonly RoutedEvent ClonedEvent = MenuTool.ClonedEvent.AddOwner(typeof(GalleryTool));

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
		public static readonly RoutedEvent CloneDiscardedEvent = MenuTool.CloneDiscardedEvent.AddOwner(typeof(GalleryTool));

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
		internal static readonly RoutedEvent DeactivatedEvent = MenuTool.DeactivatedEvent.AddOwner(typeof(GalleryTool));

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

			#region ItemActivated

		/// <summary>
		/// Event ID for the <see cref="ItemActivated"/> routed event
		/// </summary>
		/// <seealso cref="ItemActivated"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent ItemActivatedEvent =
			EventManager.RegisterRoutedEvent("ItemActivated", RoutingStrategy.Bubble, typeof(EventHandler<GalleryItemEventArgs>), typeof(GalleryTool));

		/// <summary>
		/// Occurs after a <see cref="GalleryItem"/> has been activated.  A <see cref="GalleryItem"/> is activated after the mouse pauses over the item for an amount of time that
		/// exceeds the values specified in <see cref="ActivationActionDelay"/> and <see cref="ActivationInitialActionDelay"/> properties. 
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="ActivationActionDelay"/> and <see cref="ActivationInitialActionDelay"/> properties and <see cref="ItemActivated"/> event can be used to simulate the 
		/// Live Preview functionality found in Office 2007 applications (e.g., Word).  Specify a delay that makes sense for the preview you are doing (e.g., longer delay for more expensive
		/// previews so that your application is not constructing previews everytime a <see cref="GalleryItem"/> is moused over), and handle the <see cref="ItemActivated"/> event to display the preview.</p>
		/// </remarks>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="ItemBehavior"/>
		/// <seealso cref="ItemActivatedEvent"/>
		/// <seealso cref="ItemClickedEvent"/>
		/// <seealso cref="ItemSelectedEvent"/>
		/// <seealso cref="GalleryItemEventArgs"/>
		/// <seealso cref="ActivationInitialActionDelay"/>
		/// <seealso cref="ActivationActionDelay"/>
		protected virtual void OnItemActivated(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseItemActivated(RoutedEventArgs args)
		{
			args.RoutedEvent	= GalleryTool.ItemActivatedEvent;
			args.Source			= this;
			this.OnItemActivated(args);

			// JM 11-12-07
			this._itemActivatedEventFired	= true;

            // JJD 11/21/07 - BR27066
            // Keep track of whether at least one activation event was raised during a dropdown
		    this._itemActivatedEventFiredOnceDuringDropdown = true;
		}

		/// <summary>
		/// Occurs after a <see cref="GalleryItem"/> has been activated.  A <see cref="GalleryItem"/> is activated after the mouse pauses over the item for an amount of time that
		/// exceeds the values specified in <see cref="ActivationActionDelay"/> and <see cref="ActivationInitialActionDelay"/> properties. 
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="ActivationActionDelay"/> and <see cref="ActivationInitialActionDelay"/> properties and <see cref="ItemActivated"/> event can be used to simulate the 
		/// Live Preview functionality found in Office 2007 applications (e.g., Word).  Specify a delay that makes sense for the preview you are doing (e.g., longer delay for more expensive
		/// previews so that your application is not constructing previews everytime a <see cref="GalleryItem"/> is moused over), and handle the <see cref="ItemActivated"/> event to display the preview.</p>
		/// </remarks>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="ItemBehavior"/>
		/// <seealso cref="ItemActivatedEvent"/>
		/// <seealso cref="ItemSelectedEvent"/>
		/// <seealso cref="ItemClickedEvent"/>
		/// <seealso cref="GalleryItemEventArgs"/>
		/// <seealso cref="ActivationInitialActionDelay"/>
		/// <seealso cref="ActivationActionDelay"/>
		//[Description("Occurs after a GalleryItem has been activated")]
		//[Category("Ribbon Events")]
		public event EventHandler<GalleryItemEventArgs> ItemActivated
		{
			add
			{
				base.AddHandler(GalleryTool.ItemActivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(GalleryTool.ItemActivatedEvent, value);
			}
		}

			#endregion //ItemActivated

			#region ItemClicked

		/// <summary>
		/// Event ID for the <see cref="ItemClicked"/> routed event
		/// </summary>
		/// <seealso cref="ItemClicked"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent ItemClickedEvent =
			EventManager.RegisterRoutedEvent("ItemClicked", RoutingStrategy.Bubble, typeof(EventHandler<GalleryItemEventArgs>), typeof(GalleryTool));

		/// <summary>
		/// Occurs after a <see cref="GalleryItem"/> has been clicked.
		/// </summary>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="ItemBehavior"/>
		/// <seealso cref="ItemActivatedEvent"/>
		/// <seealso cref="ItemClickedEvent"/>
		/// <seealso cref="ItemSelectedEvent"/>
		/// <seealso cref="GalleryItemEventArgs"/>
		protected virtual void OnItemClicked(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseItemClicked(RoutedEventArgs args)
		{
			args.RoutedEvent	= GalleryTool.ItemClickedEvent;
			args.Source			= this;
			this.OnItemClicked(args);
		}

		/// <summary>
		/// Occurs after a <see cref="GalleryItem"/> has been clicked.
		/// </summary>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="ItemBehavior"/>
		/// <seealso cref="ItemActivatedEvent"/>
		/// <seealso cref="ItemSelectedEvent"/>
		/// <seealso cref="ItemClickedEvent"/>
		/// <seealso cref="GalleryItemEventArgs"/>
		//[Description("Occurs after a GalleryItem has been clicked.")]
		//[Category("Ribbon Events")]
		public event EventHandler<GalleryItemEventArgs> ItemClicked
		{
			add
			{
				base.AddHandler(GalleryTool.ItemClickedEvent, value);
			}
			remove
			{
				base.RemoveHandler(GalleryTool.ItemClickedEvent, value);
			}
		}

			#endregion //ItemClicked

			#region ItemSelected

		/// <summary>
		/// Event ID for the <see cref="ItemSelected"/> routed event
		/// </summary>
		/// <seealso cref="ItemSelected"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent ItemSelectedEvent =
			EventManager.RegisterRoutedEvent("ItemSelected", RoutingStrategy.Bubble, typeof(EventHandler<GalleryItemEventArgs>), typeof(GalleryTool));

		/// <summary>
		/// Occurs after a <see cref="GalleryItem"/> has been selected.
		/// </summary>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="ItemBehavior"/>
		/// <seealso cref="ItemActivatedEvent"/>
		/// <seealso cref="ItemClickedEvent"/>
		/// <seealso cref="ItemSelectedEvent"/>
		/// <seealso cref="GalleryItemEventArgs"/>
		protected virtual void OnItemSelected(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseItemSelected(RoutedEventArgs args)
		{
			args.RoutedEvent	= GalleryTool.ItemSelectedEvent;
			args.Source			= this;
			this.OnItemSelected(args);
		}

		/// <summary>
		/// Occurs after a <see cref="GalleryItem"/> has been selected.
		/// </summary>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="ItemBehavior"/>
		/// <seealso cref="ItemActivatedEvent"/>
		/// <seealso cref="ItemClickedEvent"/>
		/// <seealso cref="ItemSelectedEvent"/>
		/// <seealso cref="GalleryItemEventArgs"/>
		//[Description("Occurs after a GalleryItem has been clicked.")]
		//[Category("Ribbon Events")]
		public event EventHandler<GalleryItemEventArgs> ItemSelected
		{
			add
			{
				base.AddHandler(GalleryTool.ItemSelectedEvent, value);
			}
			remove
			{
				base.RemoveHandler(GalleryTool.ItemSelectedEvent, value);
			}
		}

			#endregion //ItemSelected

		#endregion //Events

		#region IRibbonTool Members

		RibbonToolProxy IRibbonTool.ToolProxy
		{
			get { return GalleryToolProxy.Instance; }
		}

		#endregion

		#region GalleryToolProxy

		/// <summary>
		/// Derived <see cref="RibbonToolProxy"/> for <see cref="GalleryTool"/> instances
		/// </summary>
		protected class GalleryToolProxy : RibbonToolProxy<GalleryTool>
		{
			// AS 5/16/08 BR32980 - See the ToolProxyTests.NoInstanceVariablesOnProxies proxy for details.
			//[ThreadStatic()]
			internal static readonly GalleryToolProxy Instance = new GalleryToolProxy();
    
			#region GetKeyTipProviders
			internal override IKeyTipProvider[] GetKeyTipProviders(GalleryTool tool, ToolMenuItem menuItem)
			{
				Debug.Assert(menuItem != null, "GalleryTool was expected to be used on a menu. Should this actually return a keytip when its somewhere else?");

				// the gallery doesn't show a keytip
				return new IKeyTipProvider[0];
			}
			#endregion //GetKeyTipProviders

			#region GetMenuItemDisplayMode

			/// <summary>
			/// Returns display mode for this tool when it is inside a menu.
			/// </summary>
			/// <returns>Returns 'UseToolForEntireArea'</returns>
			/// <param name="tool">The tool instance whose display mode is being queried.</param>
			protected override ToolMenuItemDisplayMode GetMenuItemDisplayMode(GalleryTool tool)
			{
				return ToolMenuItemDisplayMode.UseToolForEntireArea;
			}

			#endregion //GetMenuItemDisplayMode	

			#region RaiseToolEvent

			/// <summary>
			/// Called by the <b>Ribbon</b> to raise one of the common tool events. 
			/// </summary>
			/// <remarks>
			/// <para class="body">This method will be called to raise a commmon tool event, e.g. <see cref="GalleryTool.Cloned"/>, <see cref="GalleryTool.CloneDiscarded"/>.</para>
			/// <para class="note"><b>Note:</b> the implementation of this method calls a protected virtual method named <see cref="GalleryTool.OnRaiseToolEvent"/> that simply calls the RaiseEvent method. This allows derived classes the opportunity of adding custom logic.</para>
			/// </remarks>
			/// <param name="sourceTool">The tool for which the event should be raised.</param>
			/// <param name="args">The event arguments</param>
			/// <seealso cref="XamRibbon"/>
			/// <seealso cref="ToolClonedEventArgs"/>
			/// <seealso cref="ToolCloneDiscardedEventArgs"/>
			protected override void RaiseToolEvent(GalleryTool sourceTool, RoutedEventArgs args)
			{
				sourceTool.OnRaiseToolEvent(args);
			}

			#endregion //RaiseToolEvent
		}

		#endregion //GalleryToolProxy
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