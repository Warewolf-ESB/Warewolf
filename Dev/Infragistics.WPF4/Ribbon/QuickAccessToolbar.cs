using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Infragistics.Windows.Ribbon.Events;
using System.Collections;
using Infragistics.Shared;
using System.Globalization;
using System.Diagnostics;
using Infragistics.Windows.Ribbon.Internal;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Displays a user customizable toolbar that contains often used tools.
	/// </summary>
	/// <remarks>
	/// <p class="body">The <see cref="QuickAccessToolbar"/> (QAT) is aligned to the left edge of the window containing the <see cref="XamRibbon"/>, either directly above or
	/// below the ribbon (user selectable via an option on the QAT�s quick customize menu or programmatically selectable via the QAT�s <see cref="XamRibbon.QuickAccessToolbarLocation"/> property).  The QAT is the 
	/// only user-customizable area of the <see cref="XamRibbon"/>.</p>
	/// <p class="body">Tools can be added to <see cref="QuickAccessToolbar"/> by the end user (via the 'Add to Quick Access Toolbar' option on the right click context menu
	/// provided for each tool) or by the developer (by adding a <see cref="QatPlaceholderTool"/> that references the <see cref="ButtonTool.Id"/> of a tool as the <see cref="QatPlaceholderTool.TargetId"/>).</p>
	/// <p class="note"><b>Note: </b>Only <see cref="QatPlaceholderTool"/>s and <see cref="SeparatorTool"/>s can be added to the QAT. Attempting to add any other tool type will generate 
	/// a NotSupportedException.</p>
	/// </remarks>
	/// <exception cref="NotSupportedException">If a tool other than a <see cref="SeparatorTool"/> or <see cref="QatPlaceholderTool"/> is added to the QAT.</exception>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="QatPlaceholderTool"/>
	/// <seealso cref="SeparatorTool"/>
	[TemplatePart(Name = "PART_QuickAccessToolbarPanel", Type = typeof(QuickAccessToolbarPanel))]
	[TemplatePart(Name = "PART_QuickAccessToolbarOverflowPanel", Type = typeof(QuickAccessToolbarOverflowPanel))]
	[TemplatePart(Name = "PART_QuickCustomizeMenuSite", Type = typeof(ContentControl))]
	[TemplatePart(Name = "PART_OverflowPopup", Type = typeof(Popup))]
	[TemplatePart(Name = "PART_OverflowButton", Type = typeof(FrameworkElement))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class QuickAccessToolbar : ToolbarBase, IRibbonToolLocation, IRibbonPopupOwner
	{
		#region Member Variables

		private XamRibbon											_ribbon;
		private QuickAccessToolbarOverflowPanel						_toolbarOverflowPanel;
		private QuickAccessToolbarPanel								_toolbarPanel;
		private ContentControl										_quickCustomizeMenuSite;
		private MenuTool											_quickCustomizeMenu;
		private Popup												_overflowPopup;
		private ArrayList											_logicalChildren = new ArrayList(5);
		private FrameworkElement									_overflowButton;
		private IKeyTipProvider										_overflowKeyTipProvider;

		// AS 10/8/07 PopupOwnerProxy
		private PopupOwnerProxy										_popupOwnerProxy;

		// AS 10/22/07 BR27591
		private double?												_calculatedMinWidthOnTop = null;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="QuickAccessToolbar"/> class.
		/// </summary>
		public QuickAccessToolbar()
		{
		}

		static QuickAccessToolbar()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickAccessToolbar), new FrameworkPropertyMetadata(typeof(QuickAccessToolbar)));
			XamRibbon.RibbonProperty.OverrideMetadata(typeof(QuickAccessToolbar), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRibbonChanged)));
			EventManager.RegisterClassHandler(typeof(QuickAccessToolbar), MenuItem.CheckedEvent, new RoutedEventHandler(QuickAccessToolbar.OnMenuItemCheckedChanged));
			EventManager.RegisterClassHandler(typeof(QuickAccessToolbar), MenuItem.UncheckedEvent, new RoutedEventHandler(QuickAccessToolbar.OnMenuItemCheckedChanged));

			// AS 10/22/07 BR27591
			FrameworkElement.MinWidthProperty.OverrideMetadata(typeof(QuickAccessToolbar), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceMinWidth)));

			// AS 12/20/07
			EventManager.RegisterClassHandler(typeof(QuickAccessToolbar), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(QuickAccessToolbar.OnRequestBringIntoView));

			// AS 6/13/12 TFS113984
			ItemsControl.ItemsSourceProperty.OverrideMetadata(typeof(QuickAccessToolbar), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceItemsSource)));
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region ContainsId

		internal override bool ContainsId(string toolId)
		{
			ItemCollection	items = this.Items;
			int				count = items.Count;
			for (int i = 0; i < count; i++)
			{
				QatPlaceholderTool qpt = items[i] as QatPlaceholderTool;
				if (qpt != null && qpt.Target != null)
				{
					string toolIdCurrent = qpt.Target.GetValue(RibbonToolHelper.IdProperty) as string;
					if (toolIdCurrent == toolId)
						return true;
				}
			}

			return false;
		}

			#endregion //ContainsId

			#region ContainsToolInstance

		internal override bool ContainsToolInstance(FrameworkElement tool)
		{
			ItemCollection	items = this.Items;
			int				count = items.Count;
			for (int i = 0; i < count; i++)
			{
				if (items[i] is SeparatorTool)
					continue;

				if (tool is QatPlaceholderTool && tool == items[i])
					return true;

				QatPlaceholderTool qatPlaceholderTool = items[i] as QatPlaceholderTool;
				if (qatPlaceholderTool != null && qatPlaceholderTool.Target == tool)
					return true;
			}

			return false;
		}

			#endregion //ContainsToolInstance	

			#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				return new MultiSourceEnumerator(new IEnumerator[] { base.LogicalChildren, this._logicalChildren.GetEnumerator() });
			}
		}

			#endregion //LogicalChildren	

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.VerifyParts();
		}

			#endregion //OnApplyTemplate	

			#region OnInitialized

		/// <summary>
		/// Called after the control has been initialized.
		/// </summary>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

		}

			#endregion //OnInitialized	
    
			#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="e">Describes the items that changed.</param>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Replace:
					this.VerifyItemsAreValid(e.NewItems);
					break;
					
				case NotifyCollectionChangedAction.Reset:
					this.VerifyItemsAreValid(this.Items);
					break;
			}
		}


			#endregion //OnItemsChanged	

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="QuickAccessToolbar"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.QuickAccessToolbarAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new QuickAccessToolbarAutomationPeer(this);
        }
            #endregion

        #endregion //Base Class Overrides

        #region Properties

        #region Public Properties

        #region IsBelowRibbon

        internal static readonly DependencyPropertyKey IsBelowRibbonPropertyKey =
			DependencyProperty.RegisterReadOnly("IsBelowRibbon",
			typeof(bool), typeof(QuickAccessToolbar), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsBelowRibbonChanged)));

		private static void OnIsBelowRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 10/22/07 BR27591
			// We force the min width to be our calculated width when its on top.
			//
			QuickAccessToolbar qat = d as QuickAccessToolbar;
			qat.DelayVerifyMinWidth();
		}

		/// <summary>
		/// Identifies the <see cref="IsBelowRibbon"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsBelowRibbonProperty =
			IsBelowRibbonPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="QuickAccessToolbar"/> is positioned below the <see cref="XamRibbon"/>, otherwise returns false. (read only)
		/// </summary>
		/// <p class="body">This read-only property is used to indicate if the QuickAccessToolbar is currently displayed below the <see cref="XamRibbon"/>. To 
		/// programatically control the placement of the QAT, you may change the <see cref="XamRibbon.QuickAccessToolbarLocation"/> property. The location can 
		/// also be changed via the UI using the right click context menu.</p>
		/// <seealso cref="IsBelowRibbonProperty"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.QuickAccessToolbarLocation"/>
		//[Description("Returns true if the QuickAccessToolbar is positioned below the XamRibbon, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsBelowRibbon
		{
			get
			{
				return (bool)this.GetValue(QuickAccessToolbar.IsBelowRibbonProperty);
			}
		}

				#endregion //IsBelowRibbon

				#region IsOverflowOpen

		/// <summary>
		/// Identifies the <see cref="IsOverflowOpen"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsOverflowOpenProperty = DependencyProperty.Register("IsOverflowOpen",
			typeof(bool), typeof(QuickAccessToolbar), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsOverflowOpenChanged), new CoerceValueCallback(OnCoerceIsOverflowOpen)));

		private static void OnIsOverflowOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			QuickAccessToolbar qat = (QuickAccessToolbar)d;

			if (qat.IsOverflowOpen)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(qat);

				// AS 12/20/07
				// Make sure the overflow keytip provider is active if we are opening
				// and keytips are active.
				//
				if (ribbon.HasKeyTipManager)
				{
					KeyTipManager keyTipManager = ribbon.KeyTipManager;

					// if we're collapsed and we're opening then make sure that we're the active 
					// key tip provider
					if (keyTipManager.IsActive && keyTipManager.ProviderPendingActivation != qat._overflowKeyTipProvider)
					{
						keyTipManager.ActivateKeyTipProvider(qat._overflowKeyTipProvider);
					}
				}

				qat.PopupOwnerProxy.OnOpen();
			}
			else
				qat.PopupOwnerProxy.OnClose();
		}

		private static object OnCoerceIsOverflowOpen(DependencyObject target, object newValue)
		{
			QuickAccessToolbar qat = target as QuickAccessToolbar;

			if (qat != null && qat.OverflowButtonVisibility != Visibility.Visible)
				return KnownBoxes.FalseBox;

			return newValue;
		}

		/// <summary>
		/// Returns/sets whether the <see cref="QuickAccessToolbar"/> overflow area is open.
		/// </summary>
		/// <remarks>
		/// <p class="body">The IsOverflowOpen property is used to display/close the overflow popup that contains tools that were not able to 
		/// fit within the available area of the <see cref="QuickAccessToolbar"/> on the <see cref="XamRibbon"/>.</p>
		/// </remarks>
		/// <seealso cref="OverflowButtonVisibility"/>
		/// <seealso cref="IsOverflowOpenProperty"/>
		//[Description("Returns/sets whether the QuickAccessToolbar overflow area is open.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsOverflowOpen
		{
			get
			{
				return (bool)this.GetValue(QuickAccessToolbar.IsOverflowOpenProperty);
			}
			set
			{
				this.SetValue(QuickAccessToolbar.IsOverflowOpenProperty, value);
			}
		}

				#endregion //IsOverflowOpen

				#region OverflowButtonVisibility

		internal static readonly DependencyPropertyKey OverflowButtonVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("OverflowButtonVisibility",
			typeof(Visibility), typeof(QuickAccessToolbar), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

		/// <summary>
		/// Identifies the <see cref="OverflowButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OverflowButtonVisibilityProperty =
			OverflowButtonVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the QuickAccessToolbar overflow button.  The Overflow button is visible when there is not enough room on the 
		/// <see cref="QuickAccessToolbar"/> to show all its tools (read-only).
		/// </summary>
		/// <seealso cref="OverflowButtonVisibilityProperty"/>
		/// <seealso cref="IsOverflowOpen"/>
		//[Description("Returns the visibility of the QuickAccessToolbar overflow button.  The Overflow button is visible when there is not enough room on the QuickAccessToolbar to show all its tools (read-only).")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility OverflowButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(QuickAccessToolbar.OverflowButtonVisibilityProperty);
			}
		}

				#endregion //OverflowButtonVisibility

			#endregion //Public Properties

			#region Internal Properties

				#region QuickCustomizeMenuSite

		internal ContentControl QuickCustomizeMenuSite
		{
			get { return this._quickCustomizeMenuSite; }
		}

				#endregion //QuickCustomizeMenuSite

				#region QuickCustomizeMenu

		internal MenuTool QuickCustomizeMenu
		{
			get 
			{
				if (this._quickCustomizeMenu == null)
				{
					this._quickCustomizeMenu			= new MenuTool();
					this._quickCustomizeMenu.ButtonType	= MenuToolButtonType.DropDown;
					this._quickCustomizeMenu.PreOpening += new EventHandler<ToolOpeningEventArgs>(OnQuickCustomizeMenuPreOpening);
					// AS 12/20/07 BR29246
					//this._quickCustomizeMenu.Closed		+= new RoutedEventHandler(OnQuickCustomizeMenuClosed);
					this._quickCustomizeMenu.SetValue(RibbonToolHelper.SizingModePropertyKey, RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox);

					// Add a dummy item otherwise the menu will not dropdown and fire the PreOpening event which is where we actually populate the
					// menu. We clear the items before adding new ones in that routine so adding a dummy here won't hurt.
					this._quickCustomizeMenu.Items.Add("dummy"); 
				}

				return this._quickCustomizeMenu; 
			}
		}

				#endregion //QuickCustomizeMenu
    
				#region ToolbarPanel

		internal QuickAccessToolbarPanel ToolbarPanel
		{
			get { return this._toolbarPanel; }
		}

				#endregion //ToolbarPanel	

				#region ToolbarOverflowPanel

		internal QuickAccessToolbarOverflowPanel ToolbarOverflowPanel
		{
			get { return this._toolbarOverflowPanel; }
		}

				#endregion //ToolbarOverflowPanel	
    
			#endregion //Internal Properties

			#region Private Properties

				#region OverflowKeyTipProvider
		private IKeyTipProvider OverflowKeyTipProvider
		{
			get
			{
				if (this._overflowKeyTipProvider == null)
					this._overflowKeyTipProvider = new QuickCustomizeKeyTipProvider(this);

				return this._overflowKeyTipProvider;
			}
		}
				#endregion //OverflowKeyTipProvider

				// AS 10/8/07 PopupOwnerProxy
				#region PopupOwnerProxy
		private PopupOwnerProxy PopupOwnerProxy
		{
			get
			{
				if (this._popupOwnerProxy == null)
					this._popupOwnerProxy = new PopupOwnerProxy(this);

				return this._popupOwnerProxy;
			}
		} 
				#endregion //PopupOwnerProxy

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region GetKeyTipProviders
		internal IKeyTipProvider[] GetKeyTipProviders(bool bumpedToolsOnly)
		{
			List<IKeyTipProvider> providers = new List<IKeyTipProvider>();

			if (this.Visibility != Visibility.Collapsed)
			{
				IEnumerable items = null;

				if (bumpedToolsOnly == false)
					items = this.Items;
				else if (this._toolbarOverflowPanel != null)
					items = this._toolbarOverflowPanel.Children;

				if (null != items)
				{
					foreach (object item in items)
					{
						QatPlaceholderTool placeholder = item as QatPlaceholderTool;

						if (null != placeholder)
						{
							if (placeholder.Target == null || placeholder.Target.Visibility == Visibility.Collapsed)
								continue;

							if (placeholder.Target is RibbonGroup)
								providers.Add((RibbonGroup)placeholder.Target);
							else if (placeholder.Target is IRibbonTool)
							{
								RibbonToolProxy proxy = ((IRibbonTool)placeholder.Target).ToolProxy;

								if (null != proxy)
									providers.AddRange(proxy.GetKeyTipProviders(placeholder.Target));
							}
						}
					}
				}

				if (bumpedToolsOnly == false && 
					this.OverflowButtonVisibility == Visibility.Visible && 
					this._overflowButton != null)
				{
					providers.Add(this.OverflowKeyTipProvider);
				}
			}

			return providers.ToArray();
		} 
				#endregion //GetKeyTipProviders

				#region GetPlaceholderToolForItem

		internal QatPlaceholderTool GetPlaceholderToolForItem(FrameworkElement item)
		{
			ItemCollection	items = this.Items;
			int				count = items.Count;
			for (int i = 0; i < count; i++)
			{
				if (item is QatPlaceholderTool && item == items[i])
					return item as QatPlaceholderTool;

				QatPlaceholderTool qpt = items[i] as QatPlaceholderTool;
				if (qpt != null && qpt.Target == item)
					return qpt;
			}

			return null;
		}

				#endregion //GetPlaceholderToolForItem

				// MD 9/23/06 - Office 2007 - Keyboard Access
				#region GetKeyTipAtIndex

		// MD 11/21/06 - Reserving Menu Mnemonics
		// Re-wrote this method and made it an instance method for reserved menu mnemonics
		#region Refactored

		//internal static string GetKeyTipAtIndex( int index )
		//{
		//    if ( index < 0 )
		//    {
		//        Debug.Fail( "The index is invalid" );
		//        return null;
		//    }
		//
		//    // KeyTips 1 - 9
		//    if ( index < 9 )
		//        return ( index + 1 ).ToString( CultureInfo.CurrentCulture );
		//
		//    // KeyTips 09 - 01
		//    if ( index < 18 )
		//        return ( 18 - index ).ToString( "00", CultureInfo.CurrentCulture );
		//
		//    // KeyTips 0A - 0Z
		//    // TODO Find a way to use letters based on the language
		//    char appendLetter = Convert.ToChar( 'A' + ( index - 18 ) );
		//
		//    if ( Char.IsLetter( appendLetter ) == false )
		//        return null;
		//
		//    return 0.ToString( CultureInfo.CurrentCulture ) + appendLetter;
		//}

		#endregion Refactored
		// AS 12/20/07 
		//internal static string GetKeyTipAtIndex( int index )
		internal string GetKeyTipAtIndex( int index )
		{
			CultureInfo culture = CultureInfo.CurrentCulture;

			System.Windows.Markup.XmlLanguage lang = this.GetValue(FrameworkElement.LanguageProperty) as System.Windows.Markup.XmlLanguage;

			if (null != lang)
			{
				try
				{
					culture = lang.GetEquivalentCulture();
				}
				catch (InvalidOperationException)
				{
				}
			}


			if ( index < 0 )
			{
				Debug.Fail( "The index is invalid" );
				return null;
			}

			// The index resolved is 1-based and will be the number from 
			// which the key tip value is based.
			int indexResolved = index + 1;

			// KeyTips 1 - 9
			if ( indexResolved <= 9 )
				return indexResolved.ToString(culture);

			// KeyTips 09 - 01
			if ( indexResolved <= 18 )
				return ( 19 - indexResolved ).ToString( "00", culture );

			// KeyTips 0A - 0Z
			char appendLetter = Convert.ToChar( 'A' + ( indexResolved - 19 ) );

			if ( Char.IsLetter( appendLetter ) == false )
				return null;

			return 0.ToString( culture ) + appendLetter;
		} 

				#endregion GetKeyTipAtIndex

				#region RemoveRibbonGroup
		internal void RemoveRibbonGroup(RibbonGroup group)
		{
			ItemCollection items = this.Items;
			int count = items.Count;
			for (int i = 0; i < count; i++)
			{
				QatPlaceholderTool qpt = items[i] as QatPlaceholderTool;

				if (qpt != null && qpt.Target is RibbonGroup)
				{
					if (qpt.Target == group || qpt.Target == group.CloneFromGroup)
					{
						this.Items.RemoveAt(i);
						break;
					}
				}
			}
		} 
				#endregion //RemoveRibbonGroup

				#region RemoveTool
		internal void RemoveTool(FrameworkElement tool)
		{
			ItemCollection items = this.Items;
			int count = items.Count;
			for (int i = 0; i < count; i++)
			{
				QatPlaceholderTool qpt = items[i] as QatPlaceholderTool;

				if (tool == items[i] || (qpt != null && qpt.Target == tool))
				{
					this.Items.RemoveAt(i);
					break;
				}
			}
		} 
				#endregion //RemoveTool

				#region RemoveToolId

		internal void RemoveToolId(string toolId)
		{
			ItemCollection	items = this.Items;
			int				count = items.Count;
			for (int i = 0; i < count; i++)
			{
				QatPlaceholderTool qpt = items[i] as QatPlaceholderTool;
				if (qpt != null && qpt.Target != null)
				{
					string toolIdCurrent = qpt.Target.GetValue(RibbonToolHelper.IdProperty) as string;
					if (toolIdCurrent == toolId)
					{
						items.RemoveAt(i);
						return;
					}
				}
			}
		}
 
				#endregion //RemoveToolId

				#region VisibleIndexOf

		internal int VisibleIndexOf( FrameworkElement tool )
		{
			int index = 0;
			ItemCollection items = this.Items;

			for (int i = 0, count = items.Count; i < count; i++)
			{
				if (items[i] is SeparatorTool)
					continue;

				QatPlaceholderTool qatPlaceholderTool = items[i] as QatPlaceholderTool;

				if (null == qatPlaceholderTool || 
					qatPlaceholderTool.Target == null ||
					qatPlaceholderTool.Visibility == Visibility.Collapsed)
					continue;

				if (qatPlaceholderTool.Target == tool)
					break;

				index++;
			}

			return index;
		}

				#endregion VisibleIndexOf

			#endregion //Internal Methodfs

			#region Private Methods

				#region CalculateMinWidthOnTop
		// AS 10/22/07 BR27591
		// We need to know what the minimum width for the panel is without any tools. One way is
		// to recalculate the qat size when the tools panel is collapsed whenever the qat template
		// changes.
		//
		private void CalculateMinWidthOnTop()
		{
			if (this._calculatedMinWidthOnTop == null)
			{
				if (this.IsBelowRibbon == false)
				{
					if (null != this._toolbarPanel)
					{
						object oldToolbarPanelVisibility = this._toolbarPanel.ReadLocalValue(FrameworkElement.MinWidthProperty);
						this._toolbarPanel.Visibility = Visibility.Collapsed;

						this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

						this._calculatedMinWidthOnTop = this.DesiredSize.Width;

						if (oldToolbarPanelVisibility is Visibility)
							this._toolbarPanel.SetValue(FrameworkElement.VisibilityProperty, oldToolbarPanelVisibility);
						else if (oldToolbarPanelVisibility is BindingBase)
							this._toolbarPanel.SetBinding(FrameworkElement.VisibilityProperty, (BindingBase)oldToolbarPanelVisibility);
						else
							this._toolbarPanel.ClearValue(FrameworkElement.VisibilityProperty);
					}

					this.CoerceValue(FrameworkElement.MinWidthProperty);
				}
			}
		}
				#endregion //CalculateMinWidthOnTop

				#region CoerceItemsSource
		private static object CoerceItemsSource(DependencyObject d, object newValue)
		{
			// AS 6/13/12 TFS113984
			// At design time Blend has decided that our ItemsSource should be a string with the name of the class
			// or a list of 3 strings with "FrameworkElement" as the value. Since the QAT is for QatPlaceholderTool
			// and SeparatorTool we'll just ignore them at design time.
			//
			if (DesignerProperties.GetIsInDesignMode(d))
			{
				if (newValue is string || newValue is IList<string>)
					return null;
			}

			return newValue;
		} 
				#endregion //CoerceItemsSource

				#region CoerceMinWidth
		private static object CoerceMinWidth(DependencyObject d, object newValue)
		{
			// AS 10/22/07 BR27591
			QuickAccessToolbar qat = d as QuickAccessToolbar;

			if (qat.IsBelowRibbon == false)
			{
				if (newValue == null || (double)newValue == 0d)
				{
					Debug.Assert(qat._calculatedMinWidthOnTop != null, "We haven't calculated the min width yet and it doesn't seem like the coerce of the minwidth is a good place to do it!");

					if (qat._calculatedMinWidthOnTop != null)
						return qat._calculatedMinWidthOnTop;
				}
			}

			return newValue;
		} 
				#endregion //CoerceMinWidth

				#region OnMenuItemCheckedChanged

		private static void OnMenuItemCheckedChanged(object sender, RoutedEventArgs e)
		{
			QuickAccessToolbar qat = sender as QuickAccessToolbar;

			Debug.Assert(qat != null);

			if (qat == null)
				return;

			MenuItem menuItem = e.OriginalSource as MenuItem;
			if (menuItem == null)
				return;

			IRibbonTool irt = menuItem.Tag as IRibbonTool;
			if (irt == null)
				return;

			FrameworkElement tool = menuItem.Tag as FrameworkElement;
			if (tool == null)
				return;

			// Note: at this point the MenuItem's IsChecked property has already been changed!
			bool toolWasOnQat = !menuItem.IsChecked;


			if (toolWasOnQat)
			{
				// Since the tool instance we have may not be the one that is actually on the Qat, check to see if it is on the Qat.
				// If not, find the instance that is.
				FrameworkElement toolInstanceToRemove = null;
				if (XamRibbon.GetLocation(tool) != ToolLocation.QuickAccessToolbar)
				{
					IEnumerator enumerator = qat._ribbon.ToolInstanceManager.GetAllInstancesWithId(RibbonToolHelper.GetId(tool));
					enumerator.Reset();
					while (enumerator.MoveNext())
					{
						FrameworkElement relatedInstance = enumerator.Current as FrameworkElement;
						if (relatedInstance != null && XamRibbon.GetLocation(relatedInstance) == ToolLocation.QuickAccessToolbar)
						{
							toolInstanceToRemove = relatedInstance;
							break;
						}
					}
				}

				if (toolInstanceToRemove != null)
					qat._ribbon.RemoveToolFromQat(toolInstanceToRemove);
			}
			else
			if (RibbonToolHelper.GetIsOnQat(tool) == false)
				qat._ribbon.AddToolToQat(tool);
	}

				#endregion //OnMenuItemCheckedChanged

				#region OnQuickCustomizeMenuClosed

		
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


				#endregion //OnQuickCustomizeMenuClosed	

				#region OnQuickCustomizeMenuPreOpening

		void OnQuickCustomizeMenuPreOpening(object sender, ToolOpeningEventArgs originalEventArgs)
		{
			MenuTool qcMenu = sender as MenuTool;
			if (qcMenu == null)
				return;


			// Clear the old items if any.
			qcMenu.Items.Clear();


			// Add a label to serve as a header.
			LabelTool labelTool = new LabelTool();
			labelTool.Content	= XamRibbon.GetString("Qat_QuickCustomizeMenu_Title");
			qcMenu.Items.Add(labelTool);


			// Build the list of common tools.
			Dictionary<string, FrameworkElement>	commonTools = new Dictionary<string, FrameworkElement>(50);

			ObservableCollection<FrameworkElement>	allTools	= this._ribbon.ToolInstanceManager.AllToolInstances;

			ToolMenuItem menuItem;
			foreach (FrameworkElement tool in allTools)
			{
				// JM 09-18-09 TFS22383 - Also check to make sure the tool is visible.
				//if (RibbonToolHelper.GetIsQatCommonTool(tool))
				if (RibbonToolHelper.GetIsQatCommonTool(tool) && tool.Visibility == Visibility.Visible)
				{
					// JM BR28473 11-20-07 - Make sure the tool can be added to the QAT before including it here.
					IRibbonTool irt = tool as IRibbonTool;
					if (irt != null && irt.ToolProxy.CanAddToQat == true)
					{
						string toolId = RibbonToolHelper.GetId(tool);
						if (toolId != null && toolId != string.Empty && commonTools.ContainsKey(toolId) == false)
						{
							menuItem				= new ToolMenuItem();
							menuItem.Tag			= tool;
							menuItem.Header			= RibbonToolHelper.GetCaption(tool);
							menuItem.IsCheckable	= true;
							menuItem.IsChecked		= RibbonToolHelper.GetIsOnQat(tool);
							qcMenu.Items.Add(menuItem);

							commonTools.Add(toolId, tool);
						}
					}
				}
			}


			// If we added any common tools, add a separator.
			SeparatorTool st;

			if (commonTools.Count > 0)
			{
				st = new SeparatorTool();
				qcMenu.Items.Add(st);
			}


			// Add the Qat location option.
			menuItem			= new ToolMenuItem();
			if (this._ribbon.QuickAccessToolbarLocation == QuickAccessToolbarLocation.AboveRibbon)
				menuItem.Header		= XamRibbon.GetString("Qat_QuickCustomizeMenu_ShowBelowRibbon");
			else
				menuItem.Header		= XamRibbon.GetString("Qat_QuickCustomizeMenu_ShowAboveRibbon");

			menuItem.Command		= RibbonCommands.ToggleQatLocation;
			menuItem.CommandTarget	= this._ribbon;		// JM BR27006 10-3-07
			qcMenu.Items.Add(menuItem);


			// AS 9/23/09 TFS22386
			// Moved into AllowMinimize if block - we shouldn't create the separator if we won't add the menu item.
			//
			//// Add a separator.
			//st = new SeparatorTool();
			//qcMenu.Items.Add(st);


			if (this._ribbon.AllowMinimize == true)
			{
				// AS 9/23/09 TFS22386
				// Moved from above.
				//
				// Add a separator.
				st = new SeparatorTool();
				qcMenu.Items.Add(st);

				
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

				menuItem = _ribbon.CreateMinimizeMenuItem("Qat_QuickCustomizeMenu_MinimizeRibbon");

				qcMenu.Items.Add(menuItem);
			}


			// Raise the QuickCustomizeMenuOpening event using new event args since the args.RoutedEvent will be different.
			ToolOpeningEventArgs newEventArgs	= new ToolOpeningEventArgs(originalEventArgs.Tool);
			this.RaiseQuickCustomizeMenuOpening(newEventArgs);
			// JJD 10/10/07 - BR26870
			// The Opening event is no longer cancelable
			//originalEventArgs.Cancel = newEventArgs.Cancel;
		}

				#endregion //OnQuickCustomizeMenuPreOpening	
    
				// AS 12/20/07
				// We need to open the overflow if a tool in the overflow requests to be brought into view.
				//
				#region OnRequestBringIntoView
		private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			QuickAccessToolbar qat = sender as QuickAccessToolbar;
			Visual child = e.TargetObject as Visual;

			// if this is a group that can dropdown but isn't open...
			if (child != null && 
				qat.ToolbarOverflowPanel != null &&
				qat.ToolbarOverflowPanel.Children.Count > 0 &&
				qat.IsOverflowOpen == false)
			{
				// AS 5/27/08 BR31004
				// IsAncestorOf doesn't account for elements within a popup so the 
				// element making the request may be within the popup of a tool 
				// that is not in the overflow panel. Limit the assumption that it is
				// to having the element within the overflow or not within the 
				// main panel but using our helper method which does acct for popups.
				//
				//bool isInDropDown = qat.ToolbarOverflowPanel.IsAncestorOf(child);

                // JJD 9/4/08 - TFS7046
                // Use the popup instead since that contains the panel itself
				//bool isInDropDown = Utilities.IsDescendantOf(qat.ToolbarOverflowPanel, child);
                bool isInDropDown = qat._overflowPopup != null && Utilities.IsDescendantOf(qat._overflowPopup, child);

				// if we didn't find the child but its not a child of
				// the group then assume its in the dropdown since
				// the visual tree of the dropdown may be detached.
				// AS 5/27/08 BR31004
				//if (isInDropDown == false && qat.IsAncestorOf(child) == false)

                // JJD 9/4/08 - TFS7046
                // Make sure the child is not the toolbarpanel also
                //if (isInDropDown == false && Utilities.IsDescendantOf(qat.ToolbarPanel, child) == false)
                if (isInDropDown == false && 
                    child != qat.ToolbarPanel &&
                    Utilities.IsDescendantOf(qat.ToolbarPanel, child) == false)
                {
                    // JJD 9/4/08 - TFS7046
                    // check to make sure the element isn't a descendant of the overflow button before
                    // assuming it is in the dropdown
                    if (qat._overflowButton != child &&
                        (qat._overflowButton == null ||
                         Utilities.IsDescendantOf(qat._overflowButton, child) == false))
                        isInDropDown = true;
                }

				if (isInDropDown && !RequestBringIntoViewCancelHelper.ShouldIgnoreBringIntoView(qat._overflowPopup) )
				{
					qat.IsOverflowOpen = true;
					qat.UpdateLayout();
					qat._overflowPopup.UpdateLayout();
					e.Handled = true;
				}
			}
		}
				#endregion //OnRequestBringIntoView

				#region OnRibbonChanged

		private static void OnRibbonChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			QuickAccessToolbar qat = target as QuickAccessToolbar;

			if (null != qat)
				qat._ribbon = e.NewValue as XamRibbon;
		}

				#endregion //OnRibbonChanged

				// AS 11/16/07 BR28520
				#region VerifyItemsAreValid
		private void VerifyItemsAreValid(IList items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				object item = items[i];

				if (item is QatPlaceholderTool || item is SeparatorTool)
					continue;

				if (item == null)
					throw new ArgumentNullException();

				throw new NotSupportedException(XamRibbon.GetString("LE_InvalidQatItemType", item.GetType().FullName));
			}
		} 
				#endregion //VerifyItemsAreValid

				// AS 10/22/07 BR27591
				#region VerifyMinWidth
		private void DelayVerifyMinWidth()
		{
            this.Dispatcher.BeginInvoke(
                // AS 11/5/08 TFS9049 
                // DataBind seems to be too early. We end up processing the min width before
                // the new template is applied - i.e. while it is still on the bottom.
                //
                //System.Windows.Threading.DispatcherPriority.DataBind,
                System.Windows.Threading.DispatcherPriority.Loaded,
                new XamRibbon.MethodInvoker(this.VerifyMinWidth));
        }

		private void VerifyMinWidth()
		{
			this.CalculateMinWidthOnTop();
			this.CoerceValue(FrameworkElement.MinWidthProperty);
		} 
				#endregion //VerifyMinWidth

				#region VerifyParts

		private void VerifyParts()
		{
			// JM 05-09-08 - As part of theming changes need to make sure that our toolbar items are no longer visual
			// children of the old QuickAccessToolbarPanel (if any).  This can happen when the panel gets recreated due
			// to a new theme being set and then new templates being applied.  The framework should be taking care of this
			// but we are keeping a parallel child colleciton in the QuickAccessToolbarPanel and that may be why it is not happening.
			QuickAccessToolbarPanel			oldToolbarPanel		= this._toolbarPanel;
			QuickAccessToolbarOverflowPanel oldOverflowPanel	= this._toolbarOverflowPanel;

			// PART_QuickAccessToolbarPanel
			this._toolbarPanel = base.GetTemplateChild("PART_QuickAccessToolbarPanel") as QuickAccessToolbarPanel;
			if (this._toolbarPanel != null)
				this._toolbarPanel.QAT = this;

			// PART_QuickAccessToolbarOverflowPanel
			this._toolbarOverflowPanel = base.GetTemplateChild("PART_QuickAccessToolbarOverflowPanel") as QuickAccessToolbarOverflowPanel;
			if (this._toolbarOverflowPanel != null)
				this._toolbarOverflowPanel.QAT = this;

            
#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)


			// PART_QuickCustomizeMenuSite
			if (this._quickCustomizeMenuSite != null)
			{
				this._logicalChildren.Remove(this.QuickCustomizeMenu);
				this.RemoveLogicalChild(this.QuickCustomizeMenu);
				this._quickCustomizeMenuSite.Content = null;
			}

			this._quickCustomizeMenuSite = base.GetTemplateChild("PART_QuickCustomizeMenuSite") as ContentControl;

			if (this._quickCustomizeMenuSite != null)
			{
				// make sure the _quickCustomizeMenuSite is not Focusable
				this._quickCustomizeMenuSite.Focusable = false;

				this._logicalChildren.Add(this.QuickCustomizeMenu);
				this.AddLogicalChild(this.QuickCustomizeMenu);
				this._quickCustomizeMenuSite.Content = this.QuickCustomizeMenu;
			}

			// PART_OverflowPopup
			this._overflowPopup = base.GetTemplateChild("PART_OverflowPopup") as Popup;

			// PART_OverflowButton
			this._overflowButton = this.GetTemplateChild("PART_OverflowButton") as FrameworkElement;

			// AS 10/8/07 PopupOwnerProxy
			this.PopupOwnerProxy.Initialize(this._overflowButton);

			// AS 10/22/07 BR27591
			this._calculatedMinWidthOnTop = null;
			this.DelayVerifyMinWidth();
		}

				#endregion //VerifyParts	

			#endregion //Private Methods

		#endregion //Methods

		#region Events

			#region QuickCustomizeMenuOpening

		/// <summary>
		/// Event ID for the <see cref="QuickCustomizeMenuOpening"/> routed event
		/// </summary>
		/// <seealso cref="QuickCustomizeMenuOpening"/>
		/// <seealso cref="OnQuickCustomizeMenuOpening"/>
		public static readonly RoutedEvent QuickCustomizeMenuOpeningEvent =
			EventManager.RegisterRoutedEvent("QuickCustomizeMenuOpening", RoutingStrategy.Bubble, typeof(EventHandler<ToolOpeningEventArgs>), typeof(QuickAccessToolbar));

		/// <summary>
		/// Used to invoke the <see cref="QuickCustomizeMenuOpening"/> event for the <see cref="QuickAccessToolbar"/>
		/// </summary>
		/// <param name="args">The event arguments for the event being raised.</param>
		/// <seealso cref="QuickCustomizeMenuOpening"/>
		/// <seealso cref="QuickCustomizeMenuOpeningEvent"/>
		protected virtual void OnQuickCustomizeMenuOpening(ToolOpeningEventArgs args)
		{
			this.RaiseEvent(args);
		}

		private void RaiseQuickCustomizeMenuOpening(ToolOpeningEventArgs args)
		{
			args.RoutedEvent = QuickAccessToolbar.QuickCustomizeMenuOpeningEvent;
			args.Source = this;

			this.OnQuickCustomizeMenuOpening(args);
		}

		/// <summary>
		/// Occurs when the QuickCustomizeMenu is about to be displayed to the end user.
		/// </summary>
		/// <remarks>
		/// <p class="body">The QuickCustomizeMenu is a popup menu that is displayed to the end user with options for customizing the 
		/// <see cref="XamRibbon"/>. It includes options for changing the <see cref="XamRibbon.IsMinimized"/> state and <see cref="XamRibbon.QuickAccessToolbarLocation"/> 
		/// properties as well as adding and removing common tools from the QAT. Common tools are those whose <see cref="ButtonTool.IsQatCommonTool"/> has 
		/// been set to true. When set to true, the caption of the tool is displayed as a menu item within the QuickCustomizeMenu. If an instance of that 
		/// tool is currently on the QAT, the menu item will appear checked and clicking that menu item will remove that instance of the tool from 
		/// the QAT. If an instance of that tool is not currently on the QAT, the menu item will appear unchecked and clicking the menu item will cause an 
		/// instance of that tool to be created and added to the end of the QAT's <see cref="ItemsControl.Items"/> collection.</p>
		/// <p class="body">The QuickCustomizeMenuOpening provides an opportunity to add/remove items from the menu before it is displayed to the end user.</p>
		/// </remarks>
		/// <seealso cref="OnQuickCustomizeMenuOpening"/>
		/// <seealso cref="QuickCustomizeMenuOpeningEvent"/>
		//[Description("Occurs when the QuickCustomizeMenu is about to be displayed to the end user.")]
		//[Category("Ribbon Events")]
		public event EventHandler<ToolOpeningEventArgs> QuickCustomizeMenuOpening
		{
			add
			{
				base.AddHandler(QuickAccessToolbar.QuickCustomizeMenuOpeningEvent, value);
			}
			remove
			{
				base.RemoveHandler(QuickAccessToolbar.QuickCustomizeMenuOpeningEvent, value);
			}
		}

			#endregion //QuickCustomizeMenuOpening

		#endregion //Events

		#region IRibbonToolLocation Members

		ToolLocation IRibbonToolLocation.Location
		{
			get { return ToolLocation.QuickAccessToolbar; }
		}

		#endregion

		#region QuickCustomizeKeyTipProvider
		private class QuickCustomizeKeyTipProvider : IKeyTipProvider,
			IKeyTipContainer
		{
			#region Member Variables

			private QuickAccessToolbar _qat;

			#endregion //Member Variables

			#region Constructor
			internal QuickCustomizeKeyTipProvider(QuickAccessToolbar qat)
			{
				Debug.Assert(qat != null);
				Debug.Assert(qat._overflowButton != null);
				this._qat = qat;
			}
			#endregion //Constructor

			#region IKeyTipProvider Members

			bool IKeyTipProvider.Activate()
			{
				this._qat.IsOverflowOpen = true;
				return this._qat.IsOverflowOpen;
			}

			bool IKeyTipProvider.Equals(IKeyTipProvider provider)
			{
				return provider == this;
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
				get { return null; }
			}

			string IKeyTipProvider.Caption
			{
				get { return null; }
			}

			bool IKeyTipProvider.DeactivateParentContainersOnActivate
			{
				get { return true; }
			}

			bool IKeyTipProvider.IsEnabled
			{
				get { return this._qat._overflowButton != null && this._qat._overflowButton.IsEnabled; }
			}

			bool IKeyTipProvider.IsVisible
			{
				get { return this._qat.OverflowButtonVisibility == Visibility.Visible; }
			}

			string IKeyTipProvider.KeyTipValue
			{
				get { return XamRibbon.GetString("QuickCustomizeOverflowButtonKeyTip"); }
			}

			Point IKeyTipProvider.Location
			{
				get
				{
					Debug.Assert(this._qat._overflowButton != null);

					Point pt = new Point();

					if (this._qat._overflowButton != null)
					{
						pt.X = (this._qat._overflowButton.ActualWidth / 2);
						pt.Y = this._qat._overflowButton.ActualHeight * 2 / 3;
					}

					return pt;
				}
			}

			char IKeyTipProvider.Mnemonic
			{
				get { return '\0'; }
			}

			UIElement IKeyTipProvider.AdornedElement
			{
				get { return this._qat._overflowButton; }
			}

			// AS 10/3/07 BR27022
			char IKeyTipProvider.DefaultPrefix
			{
				get { return '\0'; }
			}

			// AS 1/5/10 TFS25626
			bool IKeyTipProvider.CanAutoGenerateKeyTip
			{
				get { return true; }
			}

			#endregion

			#region IKeyTipContainer Members

			void IKeyTipContainer.Deactivate()
			{
				this._qat.IsOverflowOpen = false;

                // AS 2/25/09 TFS14404
                // If focus is within the popup then we need to take it back or
                // when the children are hidden the focus will be shifted out of the 
                // ribbon.
                //
                if (_qat._overflowPopup != null &&
                    _qat._overflowPopup.IsKeyboardFocusWithin)
                {
                    if (_qat._overflowButton != null && _qat._overflowButton.Focusable)
                        _qat._overflowButton.Focus();
                }

				// AS 1/11/12 TFS30896
				if (_qat._overflowPopup != null)
					RequestBringIntoViewCancelHelper.CancelBringIntoView(_qat._overflowPopup);
            }

			IKeyTipProvider[] IKeyTipContainer.GetKeyTipProviders()
			{
				// get the keytips for the tools in the overflow panel only
				return this._qat.GetKeyTipProviders(true);
			}

			bool IKeyTipContainer.ReuseParentKeyTips
			{
				get { return true; }
			}
			#endregion
		} 
		#endregion //QuickCustomizeKeyTipProvider

		// AS 10/8/07 PopupOwnerProxy
		#region IRibbonPopupOwner Members

		XamRibbon IRibbonPopupOwner.Ribbon
		{
			get { return XamRibbon.GetRibbon(this); }
		}

		bool IRibbonPopupOwner.CanOpen
		{
			get { return this.OverflowButtonVisibility == Visibility.Visible && this._toolbarOverflowPanel != null; }
		}

		bool IRibbonPopupOwner.IsOpen
		{
			get
			{
				return this.IsOverflowOpen;
			}
			set
			{
				this.IsOverflowOpen = value;
			}
		}

		bool IRibbonPopupOwner.FocusFirstItem()
		{
			return PopupOwnerProxy.FocusFirstItem(this._toolbarOverflowPanel.Children);
		}

		UIElement IRibbonPopupOwner.PopupTemplatedParent
		{
			get { return this; }
		}

		bool IRibbonPopupOwner.HookKeyDown
		{
			get { return true; }
		}

		// AS 10/18/07
		FrameworkElement IRibbonPopupOwner.ParentElementToFocus
		{
			get { return this._overflowButton; }
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