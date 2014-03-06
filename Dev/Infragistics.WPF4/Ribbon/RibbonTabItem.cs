using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Collections;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using Infragistics.Shared;
using System.Windows.Input;
using Infragistics.Windows.Controls;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Diagnostics;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents a tab in the <see cref="XamRibbon"/>.  Each RibbonTabItem can contain 1 or 
	/// more <see cref="RibbonGroup"/>s which are exposed via the <see cref="RibbonGroups"/> property.
	/// </summary>
	/// <remarks>
	/// <p class="body">The <see cref="RibbonTabItem"/> is a specialized <see cref="TabItem"/> designed to be used with the 
	/// <see cref="XamRibbon"/>. It can either be directly added to the <see cref="XamRibbon.Tabs"/> collection or 
	/// it can be part of a <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/> and added to its 
	/// <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Tabs"/> collection.</p>
	/// <p class="body">The <see cref="RibbonGroups"/> property is used to provide one or more <see cref="RibbonGroup"/> instances that provide the 
	/// content for the tab item when it is selected within the XamRibbon (<see cref="XamRibbon.SelectedTab"/>).</p>
	/// <p class="body">If the tab belongs to a ContextualTabGroup then its <see cref="ContextualTabGroup"/> property can 
	/// be used to access the owning group. The <see cref="IsFirstTabInContextualTabGroup"/> and <see cref="IsLastTabInContextualTabGroup"/> 
	/// read-only properties are set the by the ContextualTabGroup to indicate where the tab is visually placed within the tabs collection of the group.</p>
	/// </remarks>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="RibbonGroup"/>
	/// <seealso cref="RibbonGroups"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Tabs"/>
	[ContentProperty("RibbonGroups")]
	//[ToolboxItem(false)]	// JM BR28206 11-06-07 - added this here for documentation but commented out and added ToolboxBrowsableAttribute directly to DesignMetadata for the XamRibbon assembly.

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateContextual,          GroupName = VisualStateUtilities.GroupContextual)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNonContextual,       GroupName = VisualStateUtilities.GroupContextual)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RibbonTabItem : TabItemEx,
		IKeyTipProvider,
		IKeyTipContainer
	{
		#region Member Variables

		private XamRibbon									_ribbon = null;
		private ArrayList									_logicalChildren = new ArrayList(3);

		// AS 12/3/09 TFS24337
		private bool										_pendingRefocus;
		private bool										_hasKeyboardFocus;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="RibbonTabItem"/> class.  
		/// </summary>
		public RibbonTabItem()
		{
			RibbonGroupCollection groups = new RibbonGroupCollection();
			groups.CollectionChanged += new NotifyCollectionChangedEventHandler(OnGroupsCollectionChanged);
			this.SetValue(RibbonTabItem.RibbonGroupsPropertyKey, groups);

			
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

			this.SetValue(ContentControl.ContentProperty, groups);
		}

		static RibbonTabItem()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(typeof(RibbonTabItem)));
			Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			FrameworkElement.VisibilityProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisibilityChanged)));
			FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(new Style()));
			// AS 12/3/09 TFS24337
			//FrameworkElement.FocusableProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, null, new CoerceValueCallback(CoerceFocusable)));

			// AS 10/18/07
			// We need to be able to use the arrow keys to navigate between the tab items.
			//
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));

			XamRibbon.RibbonProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRibbonChanged)));

			// AS 12/6/07 BR28967/BR28970
			TabItem.IsSelectedProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsSelectedChanged)));

			// AS 12/3/09 TFS24337
			// We need to maintain our own IsKeyboardFocused flag (_hasKeyboardFocus) because we want 
			// to evaluate the element that we are losing focus to and the IsKeyboardFocused will go 
			// to false because we get either the PreviewLostKeyboardFocus or LostKeyboardFocus events.
			//
			EventManager.RegisterClassHandler(typeof(RibbonTabItem), Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnTabKeyboardFocusChanged), true);
			EventManager.RegisterClassHandler(typeof(RibbonTabItem), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnTabKeyboardFocusChanged), true);
		}
		#endregion //Constructor

		#region Base class overrides

			#region ArrangeOverride
		
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

			#endregion //ArrangeOverride

			#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				return this._logicalChildren.GetEnumerator();
			}
		}

			#endregion //LogicalChildren	
    
			#region OnHeaderChanged
		/// <summary>
		/// Invoked when the header property of the element changes.
		/// </summary>
		/// <param name="oldHeader">The old header</param>
		/// <param name="newHeader">The new header</param>
		protected override void OnHeaderChanged(object oldHeader, object newHeader)
		{
			if (null != oldHeader)
				this._logicalChildren.Remove(oldHeader);

			if (null != newHeader)
				this._logicalChildren.Add(newHeader);

			base.OnHeaderChanged(oldHeader, newHeader);
		}
			#endregion //OnHeaderChanged

			#region OnKeyDown

		/// <summary>
		/// Called when the element has input focus and a key is pressed.
		/// </summary>
		/// <param name="e">An instance of KeyEventArgs that contains information about the key that was pressed.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// AS 10/9/07
			// when navigating down from the ribbon tab, we should focus the first focusable tool
			if (e.Key == Key.Down && XamRibbon.GetIsActive(this))
			{
				foreach (RibbonGroup group in this.RibbonGroups)
				{
					if (group.IsVisible && ((IRibbonPopupOwner)group).FocusFirstItem())
					{
						e.Handled = true;
						return;
					}
				}
			}

			base.OnKeyDown(e);
		}

			#endregion //OnKeyDown	

            #region OnToolTipOpening
        /// <summary>
		/// Invoked before a tooltip would be displayed for the element.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnToolTipOpening(ToolTipEventArgs e)
		{
			// AS 10/4/07
			// Do not show a tooltip for the tab when the mouse is not over the tab item - i.e. when its
    			// over the content area
			if (this.IsMouseOverTab == false)
				e.Handled = true;

			base.OnToolTipOpening(e);
		} 
			#endregion //OnToolTipOpening

            #region SetVisualState


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected override void SetVisualState(bool useTransitions)
        {
            base.SetVisualState(useTransitions);

            // set Contextual states
            if (this.IsInContextualTabGroup)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateContextual, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNonContextual, useTransitions);


        }

        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            RibbonTabItem tabItem = target as RibbonTabItem;

            if (tabItem != null)
                tabItem.UpdateVisualStates();
        }


            #endregion //SetVisualState	

			// AS 12/6/07 BR28967/BR28970
			#region ShouldSerializeProperty
		/// <summary>
		/// Determines if the value of the specified property should be serialized.
		/// </summary>
		/// <param name="dp">The dependency property being evaluated.</param>
		/// <returns>Return false for the IsSelectedProperty and the value from the base implementation for all other properties.</returns>
		protected override bool ShouldSerializeProperty(DependencyProperty dp)
		{
			// the first tab must always be the selected tab when the applicaiton starts
			if (dp == TabItem.IsSelectedProperty)
				return false;

			return base.ShouldSerializeProperty(dp);
		} 
			#endregion //ShouldSerializeProperty

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region ContextualTabGroup

		internal static readonly DependencyPropertyKey ContextualTabGroupPropertyKey =
			DependencyProperty.RegisterReadOnly("ContextualTabGroup",
			typeof(ContextualTabGroup), typeof(RibbonTabItem), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContextualTabGroupChanged)));

		private static void OnContextualTabGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonTabItem tab = d as RibbonTabItem;

			if (tab != null)
			{
				tab.ClearValue(IsFirstTabInContextualTabGroupPropertyKey);
				tab.ClearValue(IsLastTabInContextualTabGroupPropertyKey);
			}

			ContextualTabGroup oldTabGroup = (ContextualTabGroup)e.OldValue;
			ContextualTabGroup newTabGroup = (ContextualTabGroup)e.NewValue;

			if (null != oldTabGroup)
				oldTabGroup.VerifyFirstLastTabItems();

			if (null != newTabGroup)
			{
				newTabGroup.VerifyFirstLastTabItems();
				tab.SetValue(IsInContextualTabGroupPropertyKey, KnownBoxes.TrueBox);
			}
			else
			{
				tab.ClearValue(IsInContextualTabGroupPropertyKey);
			}


			// AS 10/30/07
			// Instead of setting the IsInContextualTabGroup property, we'll set the ContextualTabGroup property
			// which in turn will set the IsInContextualTabGroup property.
			//
			foreach (RibbonGroup group in tab.RibbonGroups)
			{
				if (null != newTabGroup)
					group.SetValue(RibbonGroup.ContextualTabGroupPropertyKey, newTabGroup);
				else
					group.ClearValue(RibbonGroup.ContextualTabGroupPropertyKey);
			}
		}

		/// <summary>
		/// Identifies the <see cref="ContextualTabGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContextualTabGroupProperty =
			ContextualTabGroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/> that contains the <see cref="RibbonTabItem"/> or null (Nothing in VB) if the tab does not belong to a contextual tab group.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Tabs"/>
		/// <seealso cref="ContextualTabGroupProperty"/>
		/// <seealso cref="IsInContextualTabGroup"/>
		public ContextualTabGroup ContextualTabGroup
		{
			get
			{
				return (ContextualTabGroup)this.GetValue(RibbonTabItem.ContextualTabGroupProperty);
			}
		}

				#endregion //ContextualTabGroup

				#region IsFirstTabInContextualTabGroup

		internal static readonly DependencyPropertyKey IsFirstTabInContextualTabGroupPropertyKey =
			DependencyProperty.RegisterReadOnly("IsFirstTabInContextualTabGroup",
			typeof(bool), typeof(RibbonTabItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsFirstTabInContextualTabGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFirstTabInContextualTabGroupProperty =
			IsFirstTabInContextualTabGroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether this is the first visible tab item in a <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/>
		/// </summary>
		/// <seealso cref="IsFirstTabInContextualTabGroupProperty"/>
		/// <seealso cref="ContextualTabGroup"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Tabs"/>
		/// <seealso cref="IsInContextualTabGroup"/>
		/// <seealso cref="IsLastTabInContextualTabGroup"/>
		//[Description("Returns a boolean indicating whether this is the first visible tab item in a ContextualTabGroup")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsFirstTabInContextualTabGroup
		{
			get
			{
				return (bool)this.GetValue(RibbonTabItem.IsFirstTabInContextualTabGroupProperty);
			}
		}

				#endregion //IsFirstTabInContextualTabGroup

				#region IsInContextualTabGroup

		private static readonly DependencyPropertyKey IsInContextualTabGroupPropertyKey =
			DependencyProperty.RegisterReadOnly("IsInContextualTabGroup",
			typeof(bool), typeof(RibbonTabItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsInContextualTabGroupChanged)));

		private static void OnIsInContextualTabGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonTabItem tab = (RibbonTabItem)d;

			if (tab.IsSelected)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(tab);

				if (null != ribbon)
				{
					ribbon.RibbonTabControl.SetValue(XamRibbon.IsSelectedItemInContextualTabGroupPropertyKey, e.NewValue);
				}
			}
		}

		/// <summary>
		/// Identifies the <see cref="IsInContextualTabGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInContextualTabGroupProperty =
			IsInContextualTabGroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the tab is part of a <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/>
		/// </summary>
		/// <seealso cref="IsInContextualTabGroupProperty"/>
		/// <seealso cref="ContextualTabGroup"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Tabs"/>
		/// <seealso cref="IsFirstTabInContextualTabGroup"/>
		/// <seealso cref="IsLastTabInContextualTabGroup"/>
		//[Description("Returns a boolean indicating whether the tab is part of a ContextualTabGroup")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsInContextualTabGroup
		{
			get
			{
				return (bool)this.GetValue(RibbonTabItem.IsInContextualTabGroupProperty);
			}
		}

				#endregion //IsInContextualTabGroup

				#region IsLastTabInContextualTabGroup

		internal static readonly DependencyPropertyKey IsLastTabInContextualTabGroupPropertyKey =
			DependencyProperty.RegisterReadOnly("IsLastTabInContextualTabGroup",
			typeof(bool), typeof(RibbonTabItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsLastTabInContextualTabGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsLastTabInContextualTabGroupProperty =
			IsLastTabInContextualTabGroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether this is the last visible tab item in a <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup"/>
		/// </summary>
		/// <seealso cref="IsLastTabInContextualTabGroupProperty"/>
		/// <seealso cref="ContextualTabGroup"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Tabs"/>
		/// <seealso cref="IsInContextualTabGroup"/>
		/// <seealso cref="IsFirstTabInContextualTabGroup"/>
		//[Description("Returns a boolean indicating whether this is the last visible tab item in a ContextualTabGroup")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsLastTabInContextualTabGroup
		{
			get
			{
				return (bool)this.GetValue(RibbonTabItem.IsLastTabInContextualTabGroupProperty);
			}
		}

				#endregion //IsLastTabInContextualTabGroup

				#region KeyTip

		/// <summary>
		/// Identifies the KeyTip dependency property.
		/// </summary>
		/// <seealso cref="KeyTip"/>
		public static readonly DependencyProperty KeyTipProperty = RibbonToolHelper.KeyTipProperty.AddOwner(typeof(RibbonTabItem));

		/// <summary>
		/// A string with a maximum length of 3 characters that is used to navigate to the item when keytips.
		/// </summary>
		/// <remarks>
		/// <p class="body">Key tips are displayed when the ribbon is showing and the <b>Alt</b> key is pressed. Activating the key tip for a 
		/// <see cref="RibbonTabItem"/> will cause the tab to become the <see cref="XamRibbon.SelectedTab"/> and have its contents 
		/// displayed within the owning <see cref="XamRibbon"/>.</p>
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
				return (string)this.GetValue(RibbonTabItem.KeyTipProperty);
			}
			set
			{
				this.SetValue(RibbonTabItem.KeyTipProperty, value);
			}
		}

				#endregion //KeyTip

				#region RibbonGroups

		private static readonly DependencyPropertyKey RibbonGroupsPropertyKey =
			DependencyProperty.RegisterReadOnly("RibbonGroups",
			typeof(RibbonGroupCollection), typeof(RibbonTabItem), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="RibbonGroups"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RibbonGroupsProperty =
			RibbonGroupsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a modifiable observable collection of the <see cref="RibbonGroup"/> instances defined on 
		/// this <see cref="RibbonTabItem"/> (read only).
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="RibbonGroup"/> instances added to the collection are displayed when the tab 
		/// is selected within the owning <see cref="XamRibbon"/> (see <see cref="XamRibbon.SelectedTab"/>).</p>
		/// </remarks>
		/// <seealso cref="RibbonGroupsProperty"/>
		/// <seealso cref="RibbonTabItem"/>
		/// <seealso cref="RibbonGroup"/>
		//[Description("Returns the RibbonGroups defined on this RibbonTab (read only).")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public RibbonGroupCollection RibbonGroups
		{
			get
			{
				return (RibbonGroupCollection)this.GetValue(RibbonTabItem.RibbonGroupsProperty);
			}
		}

				#endregion //RibbonGroups

			#endregion //Public Properties

			#region Private Properties

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

			#region Internal Properties

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region AddLogicalChildHelper
		// AS 10/1/09 TFS20404
		// Changed to internal
		//
		internal void AddLogicalChildHelper(object newChild)
		{
			if (null != newChild)
			{
				this._logicalChildren.Add(newChild);
				this.AddLogicalChild(newChild);

				// AS 10/17/07
				if (newChild is RibbonGroup && this.IsInContextualTabGroup)
					((RibbonGroup)newChild).SetValue(RibbonGroup.ContextualTabGroupPropertyKey, this.ContextualTabGroup);
			}
		} 
				#endregion //AddLogicalChildHelper

				// AS 12/3/09 TFS24337
				#region CoerceFocusable
		private static object CoerceFocusable(DependencyObject d, object newValue)
		{
			if (false.Equals(newValue))
			{
				RibbonTabItem ti = d as RibbonTabItem;

				// when the window is deactivated, the hwndkeyboardinteropprovider 
				// caches a reference to the Keyboard.FocusedElement in its _restoreFocus
				// member. after that point the element will actually lose keyboard focus
				// (at least in terms of the wpf properties and events. when the window 
				// is activated it will attempt to focus that element. if that element is 
				// not focusable for any reason (disabled, hidden, ancestor hidden, focusable 
				// is false, etc.), focus is left whereever it was. this is really a bug in 
				// the wpf framework but we can try to workaround it. the situation here 
				// that leads to this problem being manifested is that we set the focusable
				// property to true only while in keytip or keyboard navigation mode. so 
				// when focus leaves the ribbon we clear the ribbon mode (since we're ending 
				// keytip mode as we should) but this causes the tab's focusable to go to 
				// false. well because of the behavior i described, the HKIP has a reference 
				// to the tab and tries to focus it when the window is reactivated. to try 
				// and get around this we're going to coerce the focusable to true while
				// (a) we think we have keyboard focus (i.e. between the tab getting keyboard 
				//		got focus and lost focus calls.
				// and 
				// (b) if when we did lose focus we lost it to another app or another window.
				//


				// if we think we have focus (because we didn't get the lostkeyboardfocus event 
				// yet), return true to keep it focusable
				if (ti._hasKeyboardFocus)
					return KnownBoxes.TrueBox;

				// if we lost focus to another window we need to keep
				// pretending that we are focusable until we reget focus 
				// and shift it out of us. what happens if that the HwndKeyboard
				if (ti._pendingRefocus)
					return KnownBoxes.TrueBox;
			}

			return newValue;
		} 
				#endregion //CoerceFocusable

				#region OnGroupsCollectionChanged
		private void OnGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					{
						foreach (object item in e.NewItems)
							this.AddLogicalChildHelper(item);
						break;
					}
				case NotifyCollectionChangedAction.Remove:
					{
						foreach (object item in e.OldItems)
							this.RemoveLogicalChildHelper(item);
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
							// JM 06-01-10 TFS32650
							((DependencyObject)item).SetValue(RibbonGroup.ReplacedByGroupPropertyKey, e.NewItems[e.OldItems.IndexOf(item)]);

							this.RemoveLogicalChildHelper(item);
						}
						foreach (object item in e.NewItems)
							this.AddLogicalChildHelper(item);

						// AS 1/8/08 BR29509
						if (DesignerProperties.GetIsInDesignMode(this))
							this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind, new XamRibbon.MethodInvoker(this.RibbonGroups.RaiseResetNotification));

						break;
					}
				case NotifyCollectionChangedAction.Reset:
					{
						// keep a dictionary where the key is the child and the boolean
						// indicates if it was previously a logical child
						Dictionary<object, bool> logicalChildren = new Dictionary<object, bool>();

						// assume the header was in the logical tree since that would be handled 
						// separate from the collection change
						if (null != this.Header)
							logicalChildren.Add(this.Header, true);

						foreach (object newItem in this.RibbonGroups)
							logicalChildren.Add(newItem, false);

						// reverify everything - remove any old that no longer exist
						// JM BR28389 11-12-07
						//for (int i = this._logicalChildren.Count - 1; i >= 0; i++)
						for (int i = this._logicalChildren.Count - 1; i >= 0; i--)
						{
							object oldChild = this._logicalChildren[i];

							// if the group is no longer in the collection then remove it from the logical tree
							if (oldChild is RibbonGroup && logicalChildren.ContainsKey(oldChild) == false)
							{
								this._logicalChildren.RemoveAt(i);
								this.RemoveLogicalChild(oldChild);

								// AS 10/17/07
								((RibbonGroup)oldChild).ClearValue(RibbonGroup.ContextualTabGroupPropertyKey);
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
						break;
					}
			}
		} 
				#endregion //OnGroupsCollectionChanged

				// AS 12/6/07 BR28967/BR28970
				#region OnIsSelectedChanged
		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (true.Equals(e.NewValue))
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(d);

				// do not set the selected tab item if the ribbon isn't initialized
				// the first tab must always be selected when the application starts
				if (null != ribbon && ribbon.IsInitialized)
				{
					XamTabControl tabCtrl = ribbon.RibbonTabControl;
					bool isSelectedItem = tabCtrl.SelectedItem == d;

					// if its not selected...
					if (isSelectedItem == false)
					{
						int index = tabCtrl.Items.IndexOf(d);

						// if its in the tabs collection then select it
						if (index >= 0)
							tabCtrl.SelectedIndex = index;
						else // otherwise reset the property
						{
							// AS 5/10/12 TFS111333
							//d.ClearValue(TabItem.IsSelectedProperty);
							DependencyPropertyUtilities.SetCurrentValue(d, TabItem.IsSelectedProperty, KnownBoxes.FalseBox);
						}
					}
				}
			}
		} 
				#endregion //OnIsSelectedChanged

				#region OnRibbonChanged

		private static void OnRibbonChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			RibbonTabItem tab = target as RibbonTabItem;

			if (tab != null)
			{
				XamRibbon ribbon = e.NewValue as XamRibbon;

				if (ribbon == null)
					tab.ClearValue(FocusableProperty);
				else
					tab.SetBinding(FocusableProperty, Utilities.CreateBindingObject(XamRibbon.IsModeSpecialProperty, BindingMode.OneWay, ribbon));

				// AS 2/29/08 BR31021
				tab._ribbon = ribbon;
			}
		}

				#endregion //OnRibbonChanged	

				// AS 12/3/09 TFS24337
				#region OnTabKeyboardFocusChanged

		private static void OnTabKeyboardFocusChanged(object sender, KeyboardFocusChangedEventArgs e)
		{
			RibbonTabItem ti = sender as RibbonTabItem;

			if (e.RoutedEvent == Keyboard.GotKeyboardFocusEvent)
			{
				// if we had focus when focus was shifted to another window 
				// then clear that flag so we can coerce focusable to false
				// but also shift focus out of the ribbon if its not in a 
				// special mode (i.e. keyboard nav/keytip modes)
				if (ti._pendingRefocus)
				{
					ti._pendingRefocus = false;

					if (null != ti._ribbon && false.Equals(ti._ribbon.GetValue(XamRibbon.IsModeSpecialProperty)))
						ti._ribbon.TransferFocusOutOfRibbon();
				}
			}
			else if (e.RoutedEvent == Keyboard.LostKeyboardFocusEvent)
			{
				// get the containing window
				Window tiWindow = Window.GetWindow(ti);
				bool isContainingWindowActive = tiWindow != null && tiWindow.IsActive;

				Debug.Assert(e.NewFocus == null || e.NewFocus is DependencyObject);

				// if the window is still active then something else 
				// in the window must be getting focus so we can let 
				// the focusable go to false
				if (isContainingWindowActive)
				{
					ti._pendingRefocus = false;
				}
				else if (e.NewFocus == null || Window.GetWindow(ti) != Window.GetWindow(e.NewFocus as DependencyObject))
				{
					// but if another app is getting focus or the element getting focus is in a
					// different window then we want to pretend we're focusable until we reget 
					// focus
					ti._pendingRefocus = true;
				}
			}

			// note i'm using the element's state in case while handling
			// the event up the chain something manipulated the focus
			ti._hasKeyboardFocus = ti.IsKeyboardFocused;

			// finally coerce the focusable since we may have been forcing it 
			// to true until we knew who it was losing focus to
			ti.CoerceValue(FocusableProperty);
		}
				#endregion //OnTabKeyboardFocusChanged
    
				#region OnVisibilityChanged
		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ContextualTabGroup tabGroup = (ContextualTabGroup)d.GetValue(RibbonTabItem.ContextualTabGroupProperty);

			if (null != tabGroup)
				tabGroup.VerifyFirstLastTabItems();

			// AS 3/18/11 TFS66436
			XamRibbon ribbon = XamRibbon.GetRibbon(d) as XamRibbon;

			if (null != ribbon)
				ribbon.VerifySelectedTab();
		} 
				#endregion //OnVisibilityChanged

				#region RemoveLogicalChildHelper
		// AS 10/1/09 TFS20404
		// Changed to internal
		//
		internal void RemoveLogicalChildHelper(object oldChild)
		{
			if (null != oldChild)
			{
				this._logicalChildren.Remove(oldChild);
				this.RemoveLogicalChild(oldChild);

				// AS 10/17/07
				((RibbonGroup)oldChild).ClearValue(RibbonGroup.ContextualTabGroupPropertyKey);
			}
		} 
				#endregion //RemoveLogicalChildHelper

			#endregion //Private Methods

		#endregion //Methods

		#region IKeyTipProvider

		bool IKeyTipProvider.Activate()
		{
			// make sure the item is in view
			this.BringIntoView();

			// AS 10/3/07 BR27037
			// Make the group be the focused element which should make it the active item.
			//
			this.Focus();

			// AS 5/10/12 TFS111333
			//this.IsSelected = true;
			DependencyPropertyUtilities.SetCurrentValue(this, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);

			// we want to force a layout since we want the groups within the tab
			// to perform their resize actions
			this.UpdateLayout();

			return this.IsSelected;
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
			get { return KeyTipAlignment.TopCenter; }
		}

		string IKeyTipProvider.AutoGeneratePrefix
		{
			get 
			{ 
				return this.IsInContextualTabGroup 
					? XamRibbon.GetString("ContextualTabGroupTabAutoGeneratePrefix") 
					: null; 
			}
		}

		string IKeyTipProvider.Caption
		{
			get 
			{ 
				string caption = this.Header != null ? this.Header.ToString() : RibbonToolHelper.GetCaption(this);

				if (this.IsInContextualTabGroup)
					caption = this.ContextualTabGroup.Caption + caption;

				return caption;
			}
		}

		bool IKeyTipProvider.DeactivateParentContainersOnActivate
		{
			get { return true; }
		}

		bool IKeyTipProvider.IsEnabled
		{
			get 
			{
				// AS 6/11/12 TFS112682
				// We should also consider a tab item where the group is not shown to be disabled/unavailable.
				//
				//return this.IsEnabled; 
				var isEnabled = this.IsEnabled;

				if (isEnabled)
				{
					ContextualTabGroup tabGroup = this.ContextualTabGroup;

					if (tabGroup != null && !tabGroup.IsVisible)
						isEnabled = false;
				}

				return isEnabled;
			}
		}

		bool IKeyTipProvider.IsVisible
		{
			get 
			{
				ContextualTabGroup tabGroup = this.ContextualTabGroup;

				if (null != tabGroup && tabGroup.IsVisible == false)
					return false;

				return this.Visibility == Visibility.Visible; 
			}
		}

		string IKeyTipProvider.KeyTipValue
		{
			get { return RibbonToolHelper.GetKeyTip(this); }
		}

		Point IKeyTipProvider.Location
		{
			get
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(this);

				// use the horizontal center of the tab item
				Rect tabRect = new Rect(0,0,this.ActualWidth, this.ActualHeight);

				// use the vertical placement of the bottom of the caption
				Rect textRect = XamRibbon.GetKeyTipPlacementElementRect(KeyTipPlacementType.Caption, this, this);

				double bottom = textRect.IsEmpty ? tabRect.Bottom : textRect.Bottom;

				return new Point(tabRect.Left + tabRect.Width / 2, bottom);
			}
		}

		char IKeyTipProvider.Mnemonic
		{
			// AS 6/14/11 TFS73058
			//get { return '\0'; }
			get 
			{
				string caption = this.Header != null ? this.Header.ToString() : RibbonToolHelper.GetCaption(this);
				return Utilities.GetFirstMnemonicChar(caption); 
			}
		}

		UIElement IKeyTipProvider.AdornedElement
		{
			get { return this; }
		}

		// AS 10/3/07 BR27022
		char IKeyTipProvider.DefaultPrefix
		{
			get { return 'T'; }
		}

		// AS 1/5/10 TFS25626
		bool IKeyTipProvider.CanAutoGenerateKeyTip
		{
			get { return true; }
		}
		#endregion //IKeyTipProvider

		#region IKeyTipContainer Members

		void IKeyTipContainer.Deactivate()
		{
			XamRibbon ribbon = XamRibbon.GetRibbon(this);

			if (null != ribbon && ribbon.RibbonTabControl.IsMinimized)
				ribbon.RibbonTabControl.IsDropDownOpen = false;
			else if (null != ribbon)
			{
				// AS 10/15/07
				// If a child has focus and we're deactivating then focus ourselves.
				//
				if (ribbon.RibbonTabControl.IsKeyboardFocusWithin && this.Focusable)
				{
					this.Focus();
				}
			}
		}

		IKeyTipProvider[] IKeyTipContainer.GetKeyTipProviders()
		{
			List<IKeyTipProvider> providers = new List<IKeyTipProvider>();

			foreach (RibbonGroup group in this.RibbonGroups)
			{
				providers.AddRange(((IKeyTipContainer)group).GetKeyTipProviders());
			}

			return providers.ToArray();
		}

		bool IKeyTipContainer.ReuseParentKeyTips
		{
			get { return false; }
		}

		#endregion //IKeyTipContainer
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