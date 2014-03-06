using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Ribbon.Internal;
using System.Diagnostics;
using System.Windows.Input;
using Infragistics.Shared;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Windows.Automation.Peers;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	
	/// <summary>
	/// The dropdown menu that is displayed in the upper left corner of the <see cref="XamRibbon"/> when the Application Menu Button is clicked.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ApplicationMenu : MenuToolBase, 
		IRibbonTool,
		IRibbonToolLocation,
		// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
		IRibbonToolLocationEx
	{
		#region Member Variables

		// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
		private ApplicationMenuItems _menuPresenterItems;
		private ApplicationMenuRecentItems _recentItems;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="ApplicationMenu"/> class.
		/// </summary>
		public ApplicationMenu()
		{
			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			this._recentItems = new ApplicationMenuRecentItems(this);
			this._menuPresenterItems = new ApplicationMenuItems(this);
		}

		static ApplicationMenu()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationMenu), new FrameworkPropertyMetadata(typeof(ApplicationMenu)));

			// AS 10/12/07 UseLargeImages
			MenuToolBase.UseLargeImagesProperty.OverrideMetadata(typeof(ApplicationMenu), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

			// AS 12/4/07 BR28886
			XamRibbon.IsActivePropertyKey.OverrideMetadata(typeof(ApplicationMenu), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsActiveChanged)));
		}

		#endregion //Constructor

		#region Base class overrides

			#region CreateMenuToolPresenter

		/// <summary>
		/// Called to create an instance of a MenuToolPresenter to represent this tool on the ribbon.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the DataContext of this presenter will be set by the caller to this tool.</para></remarks>
		/// <returns>A newly created instance of a <see cref="MenuToolPresenter"/> with its Content initialized appropriately.</returns>
		protected override ToolMenuItem CreateMenuToolPresenter()
		{
			return new ApplicationMenuPresenter();
		}

			#endregion //CreateMenuToolPresenter	

			#region GetKeyTipProviders
		internal override IKeyTipProvider[] GetKeyTipProviders()
		{
			List<IKeyTipProvider> providers = new List<IKeyTipProvider>();

			// add in the keytips for the tools
			providers.AddRange(base.GetKeyTipProviders());

			
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)


			// get the keytips for the footer toolbar
			ApplicationMenuFooterToolbar toolbar = this.FooterToolbar;

			if (null != toolbar)
			{
				for (int i = 0, count = toolbar.RegisteredTools.Count; i < count; i++)
				{
					IRibbonTool tool = toolbar.RegisteredTools[i];
					RibbonToolProxy proxy = tool.ToolProxy;

					if (null != proxy)
					{
						providers.AddRange(proxy.GetKeyTipProviders(tool as FrameworkElement));
					}
				}
			}

			return providers.ToArray();
		}
			#endregion //GetKeyTipProviders

			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			#region ItemsToBind





		internal override IList ItemsToBind
		{
			get { return this._menuPresenterItems; }
		} 
			#endregion //ItemsToBind

			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				// we want to include any recent items that have not yet been reparented
				return new MultiSourceEnumerator(
					base.LogicalChildren,
					new MenuToolBase.FilteringEnumerator(this.RecentItems.GetEnumerator(), this.ReparentedTools)
					);
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

			ToolMenuItem presenter = this.Presenter;

			// JJD 10/10/07
			// Add the footer toolbar and recentietms as a logical child of the internal menu
			// to make sure they don't span focus scopes
			if (presenter != null)
			{
				ApplicationMenuFooterToolbar ft = this.FooterToolbar;

				if (ft != null)
				{
					presenter.AddLogicalChildInternal(ft);
					
					// JJD 10/17/07
					// Maintain the corresponding 'Has...' property on ApplicationMenuPresenter
					presenter.SetValue(ApplicationMenuPresenter.HasFooterToolbarPropertyKey, KnownBoxes.TrueBox);
				}
				else
				{
					presenter.ClearValue(ApplicationMenuPresenter.HasFooterToolbarPropertyKey);
				}

				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			}

			// AS 12/4/07 BR28886
			this.SynchronizePresenterIsActive();
		}

			#endregion //OnApplyTemplate	

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="ApplicationMenu"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.ApplicationMenuAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ApplicationMenuAutomationPeer(this);
        }
            #endregion
    
			#region OnMouseDoubleClick
		/// <summary>
		/// Invoked when the application menu has been double clicked.
		/// </summary>
		/// <param name="e">Provides event arguments for the double click event</param>
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			if (e.Handled == false && e.ChangedButton == MouseButton.Left)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(this);

                // JJD 5/21/10 - NA 2010 Volumne 2 - Scenic Ribbon support
                // For scenic themes, doubling clicking on the application menu does not
                // close the window
                //if (ribbon != null && ribbon.IsWithinRibbonWindow)
				// AS 6/21/12 TFS114953
				//if (ribbon != null && ribbon.IsWithinRibbonWindow && ribbon.IsScenicTheme == false)
				if (ribbon != null && ribbon.IsWithinRibbonWindow)
				{
					// AS 6/21/12 TFS114953
					// Instead of relying on whether this is a scenic theme we should just determine if we have 
					// a control box that is visible in the ribbon. If we do then the application menu is not 
					// acting as the control box as it does in Office 2007 apps and so we should not close the 
					// window when double clicking on the app menu.
					//
					Utilities.DependencyObjectSearchCallback<FrameworkElement> callback = delegate(FrameworkElement fe)
					{
						return fe.Name == "PART_WindowIcon" && fe.IsVisible;
					};
					if (null != Utilities.GetTemplateChild<FrameworkElement>(ribbon, callback))
						return;

					// AS 10/16/07 BR27402
					// Double clicking on the menu button only should result in closing
					// the application.
					//
					Point position = e.GetPosition(this);
					Rect rect = new Rect(this.RenderSize);

					if (rect.Contains(position))
					{
						// AS 6/3/08 BR32772
						//if (XamRibbonWindow.CloseCommand.CanExecute(null, this))
						RoutedCommand closeCommand = RibbonWindowCommands.CloseCommand;
						if (closeCommand.CanExecute(null, this))
						{
							closeCommand.Execute(null, this);
							e.Handled = true;
						}
					}
				}
			}
		} 
			#endregion //OnMouseDoubleClick

		#endregion //Base class overrides	
    
		#region Properties

			#region Public Properties

				#region FooterToolbar

		/// <summary>
		/// Identifies the <see cref="FooterToolbar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FooterToolbarProperty = DependencyProperty.Register("FooterToolbar",
			typeof(ApplicationMenuFooterToolbar), typeof(ApplicationMenu), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFooterToolbarChanged)));

		private static void OnFooterToolbarChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ApplicationMenu appMenu = target as ApplicationMenu;

			if (appMenu != null)
			{
				ToolMenuItem presenter = appMenu.Presenter;

				ApplicationMenuFooterToolbar ft = e.OldValue as ApplicationMenuFooterToolbar;

				if (ft != null)
				{
					ft.Initialize(null);

					if (presenter != null)
						presenter.RemoveLogicalChildInternal(ft);
				}

				ft = e.NewValue as ApplicationMenuFooterToolbar;

				if (ft != null)
				{
					ft.Initialize(appMenu);
					
					if (presenter != null)
						presenter.AddLogicalChildInternal(ft);
				}

				// JJD 10/17/07
				// Maintain the corresponding 'Has...' property on ApplicationMenuPresenter
				if (presenter != null)
				{
					if (ft != null)
						presenter.SetValue(ApplicationMenuPresenter.HasFooterToolbarPropertyKey, KnownBoxes.TrueBox);
					else
						presenter.ClearValue(ApplicationMenuPresenter.HasFooterToolbarPropertyKey);
				}

			}
		}

		/// <summary>
		/// Returns/sets the toolbar that will be placed at the bottom of the application menu.
		/// </summary>
		/// <seealso cref="FooterToolbarProperty"/>
        /// <seealso cref="RecentItems"/>
		//[Description("Returns/sets the toolbar that will be placed at the bottom of the application menu.")]
		//[Category("Ribbon Properties")]
		public ApplicationMenuFooterToolbar FooterToolbar
		{
			get
			{
				return (ApplicationMenuFooterToolbar)this.GetValue(ApplicationMenu.FooterToolbarProperty);
			}
			set
			{
				this.SetValue(ApplicationMenu.FooterToolbarProperty, value);
			}
		}

				#endregion //FooterToolbar

				#region HasRecentItemsHeader

		private static readonly DependencyPropertyKey HasRecentItemsHeaderPropertyKey =
			DependencyProperty.RegisterReadOnly("HasRecentItemsHeader",
			typeof(bool), typeof(ApplicationMenu), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasRecentItemsHeader"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasRecentItemsHeaderProperty =
			HasRecentItemsHeaderPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates whether the <see cref="RecentItemsHeader"/> is null
		/// </summary>
		/// <value>Returns a boolean indicating if the <see cref="RecentItemsHeader"/> has been set. If the header is not null, true is returned; otherwise false is returned.</value>
		/// <seealso cref="HasRecentItemsHeaderProperty"/>
        /// <seealso cref="RecentItemsHeader"/>
		//[Description("Indicates whether the 'RecentItemsHeader' is null")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HasRecentItemsHeader
		{
			get
			{
				return (bool)this.GetValue(ApplicationMenu.HasRecentItemsHeaderProperty);
			}
		}

				#endregion //HasRecentItemsHeader

				#region Image

		/// <summary>
		/// Identifies the <see cref="Image"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image",
			typeof(ImageSource), typeof(ApplicationMenu), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets/sets the image that will be displayed on the ApplicationMenu button in the <see cref="XamRibbon"/>.
		/// </summary>
		/// <seealso cref="ImageProperty"/>
		//[Description("Gets/sets the image that will be displayed on the ApplicationMenu button in the XamRibbon.")]
		//[Category("Ribbon Properties")]
		public ImageSource Image
		{
			get
			{
				return (ImageSource)this.GetValue(ApplicationMenu.ImageProperty);
			}
			set
			{
				this.SetValue(ApplicationMenu.ImageProperty, value);
			}
		}

				#endregion //Image

				#region RecentItems

		/// <summary>
		/// Returns the collection of items displayed in the right hand side of the dropdown of the <see cref="ApplicationMenu"/>.
		/// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> the items within the menu will be displayed to the left of the recent items.</para></remarks>
        /// <seealso cref="HasRecentItemsHeader"/>
        /// <seealso cref="RecentItemsHeader"/>
        /// <seealso cref="RecentItemsHeaderTemplate"/>
        /// <seealso cref="RecentItemsHeaderTemplateSelector"/>
		//[Description("The collection of items displayed in the right hand side of the dropdown of the ApplicationMenu.")]
		//[Category("Ribbon Properties")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ObservableCollectionExtended<object> RecentItems
		{
			get
			{
				return this._recentItems;
			}
		}

		/// <summary>
		/// Indicates if the <see cref="RecentItems"/> property needs to be serialized.
		/// </summary>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeRecentItems()
		{
			return this._recentItems.Count > 0;
		}
				#endregion //RecentItems

				#region RecentItemsHeader

		/// <summary>
		/// Identifies the <see cref="RecentItemsHeader"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecentItemsHeaderProperty = DependencyProperty.Register("RecentItemsHeader",
			typeof(object), typeof(ApplicationMenu), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnRecentItemsHeaderChanged)));

		private static void OnRecentItemsHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.SetValue(HasRecentItemsHeaderPropertyKey, e.NewValue == null ? KnownBoxes.FalseBox : KnownBoxes.TrueBox);
		}

		/// <summary>
		/// The object that is displayed in the header area over the recent items within the dropdown of the ApplicationMenu
		/// </summary>
		/// <seealso cref="RecentItemsHeaderProperty"/>
		/// <seealso cref="RecentItems"/>
		/// <seealso cref="HasRecentItemsHeader"/>
		/// <seealso cref="RecentItemsHeaderTemplate"/>
		/// <seealso cref="RecentItemsHeaderTemplateSelector"/>
		//[Description("Returns/sets the object that is displayed in the header area over the recent items within the dropdown of the ApplicationMenu")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public object RecentItemsHeader
		{
			get
			{
				return (object)this.GetValue(ApplicationMenu.RecentItemsHeaderProperty);
			}
			set
			{
				this.SetValue(ApplicationMenu.RecentItemsHeaderProperty, value);
			}
		}

				#endregion //RecentItemsHeader

				#region RecentItemsHeaderTemplate

		/// <summary>
		/// Identifies the <see cref="RecentItemsHeaderTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecentItemsHeaderTemplateProperty = DependencyProperty.Register("RecentItemsHeaderTemplate",
			typeof(DataTemplate), typeof(ApplicationMenu), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The template used to display the content of the <see cref="RecentItemsHeader"/>.
		/// </summary>
		/// <seealso cref="RecentItemsHeaderTemplateProperty"/>
		/// <seealso cref="RecentItemsHeader"/>
		/// <seealso cref="RecentItemsHeaderTemplateSelector"/>
		/// <seealso cref="HasRecentItemsHeader"/>
		//[Description("Returns/sets the template used to display the content of the 'RecentItemsHeader'.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public DataTemplate RecentItemsHeaderTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(ApplicationMenu.RecentItemsHeaderTemplateProperty);
			}
			set
			{
				this.SetValue(ApplicationMenu.RecentItemsHeaderTemplateProperty, value);
			}
		}

				#endregion //RecentItemsHeaderTemplate

				#region RecentItemsHeaderTemplateSelector

		/// <summary>
		/// Identifies the <see cref="RecentItemsHeaderTemplateSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecentItemsHeaderTemplateSelectorProperty = DependencyProperty.Register("RecentItemsHeaderTemplateSelector",
			typeof(DataTemplateSelector), typeof(ApplicationMenu), new FrameworkPropertyMetadata(null));

		/// <summary>
        /// Gets/sets a template selector that can provide selection logic for the template used to display the <see cref="RecentItemsHeader"/>.
		/// </summary>
		/// <seealso cref="RecentItemsHeaderTemplateSelectorProperty"/>
		//[Description("Gets/sets a template selector that can provide selection logic for the template used to display the 'RecentItemsHeader'.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public DataTemplateSelector RecentItemsHeaderTemplateSelector
		{
			get
			{
				return (DataTemplateSelector)this.GetValue(ApplicationMenu.RecentItemsHeaderTemplateSelectorProperty);
			}
			set
			{
				this.SetValue(ApplicationMenu.RecentItemsHeaderTemplateSelectorProperty, value);
			}
		}

				#endregion //RecentItemsHeaderTemplateSelector

			#endregion //Public Properties	
    
			#region Internal Properties

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				// AS 12/4/07 BR28886
				#region OnIsActiveChanged

		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ApplicationMenu)d).SynchronizePresenterIsActive();
		}

				#endregion //OnIsActiveChanged

				// AS 12/4/07 BR28886
				#region SynchronizePresenterIsActive
		private void SynchronizePresenterIsActive()
		{
			if (null != this.Presenter)
				this.Presenter.SetValue(XamRibbon.IsActivePropertyKey, this.GetValue(XamRibbon.IsActiveProperty));
		}
				#endregion //SynchronizePresenterIsActive

			#endregion //Private Methods

		#endregion //Methods

		#region IRibbonToolLocation Members

		ToolLocation IRibbonToolLocation.Location
		{
			get { return ToolLocation.ApplicationMenu; }
		}

		#endregion

		#region IRibbonTool Members

		RibbonToolProxy IRibbonTool.ToolProxy
		{
			get { return ApplicationMenuProxy.Instance; }
		}

		#endregion

		#region ApplicationMenuProxy

		/// <summary>
		/// Derived <see cref="RibbonToolProxy"/> for <see cref="ApplicationMenu"/> instances
		/// </summary>
		protected class ApplicationMenuProxy : MenuToolBaseProxy
		{
			// AS 5/16/08 BR32980 - See the ToolProxyTests.NoInstanceVariablesOnProxies proxy for details.
			//[ThreadStatic()]
			internal new static readonly MenuToolBaseProxy Instance = new ApplicationMenuProxy();

			internal override IKeyTipProvider[] GetKeyTipProviders(MenuToolBase tool, ToolMenuItem menuItem)
			{
				Debug.Assert(menuItem == null);

				ApplicationMenu menu = tool as ApplicationMenu;

				if (menu == null || menuItem != null)
					return base.GetKeyTipProviders(tool, menuItem);

				return new IKeyTipProvider[] { new ApplicationMenuKeyTipProvider(menu) };
			}

			#region CanAddToQat
			/// <summary>
			/// Returns a boolean indicating whether the tool type can be added to the <see cref="QuickAccessToolbar"/>.
			/// </summary>
			public override bool CanAddToQat
			{
				get { return false; }
			}
			#endregion //CanAddToQat
		}

		#endregion //ApplicationMenuProxy

		#region ApplicationMenuKeyTipProvider
		private class ApplicationMenuKeyTipProvider : ToolKeyTipProvider,
			IKeyTipProvider
		{
			#region Member Variables

			private ApplicationMenu _menu;

			#endregion //Member Variables

			#region Constructor
			internal ApplicationMenuKeyTipProvider(ApplicationMenu menu)
				: base(menu, null)
			{
				this._menu = menu;
			}
			#endregion //Constructor

			#region IKeyTipProvider Members

			public override KeyTipAlignment Alignment
			{
				get { return KeyTipAlignment.MiddleCenter; }
			}

			public override string KeyTipValue
			{
				get
				{
					string keyTip = RibbonToolHelper.GetKeyTip(this._menu);

					if (string.IsNullOrEmpty(keyTip))
						keyTip = XamRibbon.GetString("ApplicationMenuDefaultKeyTip");

					return keyTip;
				}
			}

			public override Point Location
			{
				get
				{
					Size size = this._menu.RenderSize;

					if (size.Width <= 0 || size.Height <= 0)
						return new Point();

					return new Point(size.Width / 2, size.Height / 2);
				}
			}
			#endregion //IKeyTipProvider
		} 
		#endregion //ApplicationMenuKeyTipProvider

		// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
		#region IRibbonToolLocationEx Members

		ToolLocation IRibbonToolLocationEx.GetLocation(object descendant)
		{
			if (this._recentItems.Contains(descendant))
				return ToolLocation.ApplicationMenuRecentItems;

			return ToolLocation.ApplicationMenu;
		}

		#endregion

		// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
		#region ApplicationMenuRecentItems
		private class ApplicationMenuRecentItems : ObservableCollectionExtended<object>
		{
			#region Member Variables

			private ApplicationMenu _menu;

			#endregion //Member Variables

			#region Constructor
			internal ApplicationMenuRecentItems(ApplicationMenu menu)
			{
				this._menu = menu;
			}
			#endregion //Constructor

			#region Base class overrides
			protected override bool NotifyItemsChanged
			{
				get
				{
					return true;
				}
			}

			protected override void OnItemAdded(object itemAdded)
			{
				base.OnItemAdded(itemAdded);

				this._menu.AddLogicalChild(itemAdded);
			}

			protected override void OnItemRemoved(object itemRemoved)
			{
				base.OnItemRemoved(itemRemoved);

				this._menu.RemoveLogicalChild(itemRemoved);
			}

			// AS 3/24/10 TFS29482
			// As we do in the OnItemsChanged of the MenuToolBase, we have to 
			// clean up the reparented items when something is removed from the 
			// recent items.
			//
			protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
			{
				base.OnCollectionChanged(e);

				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Remove:
					case NotifyCollectionChangedAction.Reset:
					case NotifyCollectionChangedAction.Replace:
						_menu.VerifyReparentedItems();
						break;
				}
			}
			#endregion //Base class overrides
		} 
		#endregion //ApplicationMenuRecentItems

		// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
		#region ApplicationMenuItems
		private class ApplicationMenuItems : PropertyChangeNotifier, IList, INotifyCollectionChanged
		{
			#region Member Variables

			private ItemCollection _items;
			private ObservableCollection<object> _recentItems;
			private ApplicationMenu _applicationMenu;
			private object _syncRoot;

			// AS 10/1/09 TFS21851
			private static readonly object EmptyMenuItemId = new object();
			private ToolMenuItem _emptyMenuItem;

			#endregion //Member Variables

			#region Constructor
			internal ApplicationMenuItems(ApplicationMenu owner)
			{
				this._applicationMenu = owner;
				this._recentItems = owner.RecentItems;
				this._items = owner.Items;
				this._syncRoot = new object();

				this._recentItems.CollectionChanged += new NotifyCollectionChangedEventHandler(OnRecentItemsChanged);
				((INotifyCollectionChanged)this._items).CollectionChanged += new NotifyCollectionChangedEventHandler(OnAppItemsChanged);
			}

			void OnAppItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				this.RaisePropertyChangedEvent("Item[]");
				this.RaisePropertyChangedEvent("Count");

				if (null != this._notifyHandler)
					this._notifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}

			void OnRecentItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				this.RaisePropertyChangedEvent("Item[]");
				this.RaisePropertyChangedEvent("Count");

				if (null != this._notifyHandler)
					this._notifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}

			#endregion //Constructor

			#region Properties

			// AS 10/1/09 TFS21851
			#region IsEmpty
			private bool IsEmpty
			{
				get
				{
					return _items.Count + _recentItems.Count == 0;
				}
			}
			#endregion //IsEmpty

			// AS 10/1/09 TFS21851
			#region EmptyMenuItem
			private ToolMenuItem EmptyMenuItem
			{
				get
				{
					if (_emptyMenuItem == null)
					{
						_emptyMenuItem = new ToolMenuItem();
						_emptyMenuItem.Name = "EmptyItem";
						_emptyMenuItem.Tag = EmptyMenuItemId;
						_emptyMenuItem.Visibility = Visibility.Collapsed;
					}

					return _emptyMenuItem;
				}
			}
			#endregion //EmptyMenuItem

			#endregion //Properties

			#region IList Members

			int IList.Add(object value)
			{
				throw new NotSupportedException();
			}

			void IList.Clear()
			{
				throw new NotSupportedException();
			}

			bool IList.Contains(object value)
			{
				if (this._items.Contains(value))
					return true;

				// AS 10/1/09 TFS21851
				// Could have an empty menu item.
				//
				//return this._recentItems.Contains(value);
				if (this._recentItems.Contains(value))
					return true;

				if (this.IsEmpty && value == this.EmptyMenuItem)
					return true;

				return false;
			}

			int IList.IndexOf(object value)
			{
				int index = this._items.IndexOf(value);

				if (index < 0)
					index = this._recentItems.IndexOf(value);

				// AS 10/1/09 TFS21851
				if (index < 0 && value == this.EmptyMenuItem && this.IsEmpty)
					return 0;

				return index;
			}

			void IList.Insert(int index, object value)
			{
				throw new NotSupportedException();
			}

			bool IList.IsFixedSize
			{
				get { return false; }
			}

			bool IList.IsReadOnly
			{
				get { return true; }
			}

			void IList.Remove(object value)
			{
				throw new NotSupportedException();
			}

			void IList.RemoveAt(int index)
			{
				throw new NotSupportedException();
			}

			object IList.this[int index]
			{
				get
				{
					// AS 10/1/09 TFS21851
					if (index == 0 && this.IsEmpty)
						return this.EmptyMenuItem;

					if (index < this._items.Count)
						return this._items[index];

					index -= this._items.Count;

					return this._recentItems[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			#endregion

			#region ICollection Members

			void ICollection.CopyTo(Array array, int index)
			{
				this._items.CopyTo(array, index);

				index += this._items.Count;

				((ICollection)this._recentItems).CopyTo(array, index);

				// AS 10/1/09 TFS21851
				if (this.IsEmpty)
					array.SetValue(this.EmptyMenuItem, index);
			}

			int ICollection.Count
			{
				get
				{
					int count = this._items.Count;

					count += this._recentItems.Count;

					// AS 10/1/09 TFS21851
					if (count == 0)
						return 1;

					return count;
				}
			}

			bool ICollection.IsSynchronized
			{
				get
				{
					return false;
				}
			}

			object ICollection.SyncRoot
			{
				get 
				{
					if (this._syncRoot == null)
					{
						IEnumerable srcCollection = this._items.SourceCollection;

						if (null != srcCollection)
						{
							try
							{
								this._syncRoot = ((ICollection)this._items).SyncRoot;
							}
							catch { }
						}

						// if we couldn't get it from the items collection then
						// create one
						if (this._syncRoot == null)
							System.Threading.Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
					}

					return this._syncRoot;
				}
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				// AS 10/1/09 TFS21851
				if (this.IsEmpty)
					return new SingleItemEnumerator(this.EmptyMenuItem);

				IEnumerator itemsEnumerator = ((IEnumerable)this._items).GetEnumerator();
				if (this._recentItems != null)
				{
					return new MultiSourceEnumerator(itemsEnumerator,
						((IEnumerable)this._recentItems).GetEnumerator());
				}

				return itemsEnumerator;
			}

			#endregion

			#region INotifyCollectionChanged Members

			private NotifyCollectionChangedEventHandler _notifyHandler = null;

			event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
			{
				add { this._notifyHandler = (NotifyCollectionChangedEventHandler)Delegate.Combine(this._notifyHandler, value); }
				remove { this._notifyHandler = (NotifyCollectionChangedEventHandler)Delegate.Remove(this._notifyHandler, value); }
			}

			#endregion
		} 
		#endregion //ApplicationMenuItems
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