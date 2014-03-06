using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Licensing;
using System.Diagnostics;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Virtualization;
using System.Windows.Input;
using System.Xml;
using Infragistics.Windows.Automation.Peers;
using System.Windows.Automation.Peers;
using System.Windows.Automation;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// A <see cref="System.Windows.Controls.Primitives.Selector"/> derived control that uses the <see cref="XamCarouselPanel"/> as its ItemsPanel to arrange items along a user-defined path.
	/// </summary>
	/// <remarks>
	/// <p class="body">The XamCarouselListBox takes the <see cref="XamCarouselPanel"/> to the next level.  By using the <see cref="XamCarouselPanel"/> as its ItemsPanel the XamCarouselListBox is able to combine the power and layout flexibility 
    /// of the <see cref="XamCarouselPanel"/> with the list management, selection and data binding capabilities of the WPF Selector control.  Use this control the same way you would use a <see cref="System.Windows.Controls.ListBox"/> with the added benefit of built-in support for
	/// the arrangement of list items along a user-defined path.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="XamCarouselPanel"/>
	/// <seealso cref="System.Windows.Controls.Primitives.Selector"/>
    /// <seealso cref="System.Windows.Controls.ListBox"/>
    
	
	//[ToolboxItem(true)]
	// AS 5/13/10
	//[System.Drawing.ToolboxBitmap(typeof(XamCarouselListBox), AssemblyVersion.ToolBoxBitmapFolder + "CarouselListBox.bmp")]
	//[Description("A Selector derived control that uses the XamCarouselPanel as its ItemsPanel to arrange items along a user-defined path.")]
	public class XamCarouselListBox : RecyclingItemsControl, 
									  ICarouselPanelSelectionHost
	{
		#region Member Variables

		private Stack<DependencyObject>						_containerCache = new Stack<DependencyObject>(CONTAINER_CACHE_SIZE);
		private UltraLicense								_license;

		private CarouselViewSettings						_viewSettings;
		private XamCarouselPanel							_carouselPanel;

		private bool										_updateSelectedItem = true;
		private bool										_updateSelectedIndex = true;

		private bool										_isSynchronizedWithCurrentItemPrivate = false;

		#endregion Member Variables

		#region Constructor

		static XamCarouselListBox()
		{
			ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(XamCarouselPanel)));
			template.VisualTree.SetValue(XamCarouselPanel.IsInCarouselListBoxProperty, KnownBoxes.TrueBox);
			template.Seal();

			ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(XamCarouselListBox), new FrameworkPropertyMetadata(template));

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamCarouselListBox), new FrameworkPropertyMetadata(typeof(XamCarouselListBox)));

			// [JM 05-22-07]
			// AS 6/8/07
			//XamCarouselListBox.IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(XamCarouselListBox), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
			XamCarouselListBox.IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(XamCarouselListBox), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsSelectedChanged)));
		}

		/// <summary>
		/// Creates an instance of a XamCarouselListBox control.
		/// </summary>
		public XamCarouselListBox()
		{

			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			// AS 11/7/07 BR21903
			// Always do the license checks.
			//
			//if (DesignerProperties.GetIsInDesignMode(this))
			{
				try
				{
					// We need to pass our type into the method since we do not want to pass in 
					// the derived type.
					this._license = LicenseManager.Validate(typeof(XamCarouselListBox), this) as UltraLicense;
				}
				catch (System.IO.FileNotFoundException) { }
			}

			// [JM 06-14-07]
			base.Items.CurrentChanged += new EventHandler(OnItemsCurrentChanged);
		}

		#endregion //Constructor

		#region Constants

		private const int											CONTAINER_CACHE_SIZE = 15;

		#endregion //Constants

		#region Base Class Overrides

			#region Properties

				#region HandlesScrolling

		/// <summary>
		/// Returns a value that indicates whether the control handles scrolling.
		/// </summary>
		/// <returns>True if the control has support for scrolling, otherwise false.</returns>
		protected override bool HandlesScrolling
		{
			get { return true; }
		}

				#endregion //HandlesScrolling

			#endregion //Properties

			#region Methods

				#region ClearContainerForItemOverride

		/// <summary>
		/// Called to clear the effects of the PrepareContainerForItemOverride method. 
		/// </summary>
		/// <param name="element">The container being cleared.</param>
		/// <param name="item">The item contained by the container being cleared.</param>
		protected override void ClearContainerForItemOverride(System.Windows.DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride(element, item);

			CarouselListBoxItem carouselListBoxItem = element as CarouselListBoxItem;
			if (carouselListBoxItem != null)
				carouselListBoxItem.ClearContainerForItem(item);

			//if (this._containerCache.Count < XamCarouselListBox.CONTAINER_CACHE_SIZE)
			//    this._containerCache.Push(element);
		}

				#endregion //ClearContainerForItemOverride

				#region DeactivateContainer

		/// <summary>
		/// Called when a container is being deactivated.
		/// </summary>
		/// <param name="container">The container being deactivated.</param>
		/// <param name="item">Its associated item.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the Visibility property of the container will be set to 'Collapsed' before this method is called. 
		/// The original setting for the Visibility property will be restored before a subsequent call to <see cref="ReactivateContainer"/> or <see cref="ReuseContainerForNewItem"/>.</para>
		/// </remarks>
		/// <seealso cref="ReactivateContainer"/>
		/// <seealso cref="ReuseContainerForNewItem"/>
		protected internal override void DeactivateContainer(DependencyObject container, object item)
		{
			base.DeactivateContainer(container, item);

			container.SetValue(XamCarouselListBox.IsSelectedProperty, KnownBoxes.FalseBox);
		}

				#endregion //DeactivateContainer	
    
				#region GetContainerForItemOverride

		/// <summary>
		/// Provides a container object for a items in the list.
		/// </summary>
		/// <returns>An object that that can be used as a container for items in the list.</returns>
		protected override System.Windows.DependencyObject GetContainerForItemOverride()
		{
			//if (this._containerCache.Count > 0)
			//    return this._containerCache.Pop();
			//else
				return new CarouselListBoxItem(this._carouselPanel);
		}

				#endregion //GetContainerForItemOverride

				#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the specified item is (or is eligible to be) its own container. 
		/// </summary>
		/// <param name="item">The item to evaluate</param>
		/// <returns>True if the specified item is its own container.</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return (item is CarouselListBoxItem);
		}

				#endregion //IsItemItsOwnContainerOverride

				#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size baseMeasureSize = base.MeasureOverride(availableSize);

			// Look for our XamCarouselPanel and save a referenceto it when we find it.
			if (this._carouselPanel == null)
			{
				this._carouselPanel = Utilities.GetDescendantFromType(this, typeof(XamCarouselPanel), true) as XamCarouselPanel;
				if (this._carouselPanel != null)
				{
					this._carouselPanel.SelectionHost	= this;
					this.BindCarouselPanelViewSettings(this._carouselPanel);

					// [BR20070]
					this._carouselPanel.CarouselPanelAdorner.SetBinding(CarouselPanelAdorner.OpacityProperty, Utilities.CreateBindingObject(XamCarouselListBox.OpacityProperty, BindingMode.OneWay, this));
				}
			}

			return baseMeasureSize;
		}

				#endregion //MeasureOverride	

				// AS 6/8/07 UI Automation
				#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamCarouselListBox"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.XamCarouselListBoxAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.XamCarouselListBoxAutomationPeer(this);
		} 
				#endregion //OnCreateAutomationPeer

				#region OnInitialized

		/// <summary>
		/// Raises the System.Windows.FrameworkElement.Initialized event.
		/// </summary>
		/// <param name="e">An instance of EventArgs that contains information about the event.</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			// AS 6/29/07
			// I don't think we should really be converting what could have been 
			// a templated value to a local value unless we need to create the view
			// settings.
			//
            // JJD 1/2/07 
            // Check for design mode first
            // If we are in design mode don't sync ViewSettings
            //if (this._viewSettings == null)
			if (this._viewSettings == null &&
                DesignerProperties.GetIsInDesignMode(this) == false)
			{
				// Set the ViewSettings dependency property using the CLR property which will do a lazy create of the object.
				this.SetValue(XamCarouselPanel.ViewSettingsProperty, this.ViewSettings);
			}
		}

				#endregion //OnInitialized	

				#region OnIsKeyboardFocusWithinChanged

		/// <summary>
		/// Invoked just before the System.Windows.UIElement.IsKeyboardFocusWithinChanged event is raised by this element. 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsKeyboardFocusWithinChanged(e);

			bool selectionActive = false;
			bool isKeyboardFocusWithin = base.IsKeyboardFocusWithin;

			if (isKeyboardFocusWithin)
				selectionActive = true;

			if (((bool)base.GetValue(XamCarouselListBox.IsSelectionActiveProperty)) != selectionActive)
				base.SetValue(XamCarouselListBox.IsSelectionActivePropertyKey, KnownBoxes.FromValue(selectionActive));
		}

				#endregion //OnIsKeyboardFocusWithinChanged	

				// JM 12-21-07 BR29263, BR29264
				#region OnItemsChanged

		/// <summary>
		/// Overridden. Invoked when the contents of the items collection has changed.
		/// </summary>
		/// <param name="e">Event arguments indicating the change that occurred.</param>
		protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);
			// JM 01-15-08 BR29706
			//if (base.Items != null && base.Items.CurrentItem != null)
			if (base.Items != null && base.Items.CurrentItem != null && this._isSynchronizedWithCurrentItemPrivate)
				this.SetSelectedToCurrent();
		}

				#endregion //OnItemsChanged	
    
				#region OnItemsPanelChanged

		/// <summary>
		/// Called when the ItemsPanel has changed.
		/// </summary>
		/// <param name="oldItemsPanel">The old value</param>
		/// <param name="newItemsPanel">The new value</param>
		protected override void OnItemsPanelChanged(ItemsPanelTemplate oldItemsPanel, ItemsPanelTemplate newItemsPanel)
		{
			// AS 6/29/07
			// Calling ClearAllBindings with a null value will generate an exception.
			//
			if (null != this._carouselPanel)
			{
				// Null out our reference to the path panel - it will get set to the new panel in the next MeasureOverride.
				BindingOperations.ClearAllBindings(this._carouselPanel);
				this._carouselPanel = null;
			}

			base.OnItemsPanelChanged(oldItemsPanel, newItemsPanel);
		}

				#endregion //OnItemsPanelChanged	

				// [JM 06-14-07]
				#region OnItemsSourceChanged

		/// <summary>
		/// Called when the ItemsSource property changes.
		/// </summary>
		/// <param name="oldValue">Old value of the ItemsSource property.</param>
		/// <param name="newValue">New value of the ItemsSource property.</param>
		protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
		{
			// JM 01-11-11 TFS58736
			if (oldValue != null && (this.ItemTemplate != null || this.ItemTemplateSelector != null))
				this.RecyclingItemContainerGenerator.RemoveAll();

			this.SetSynchronizationWithCurrentItem();
		}

				#endregion //OnItemsSourceChanged	
    
				#region OnPropertyChanged

		/// <summary>
		/// Called when a property changes on this object.
		/// </summary>
		/// <param name="e">An instance of DependencyPropertyChangedEventArgs that contains information about the property that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			// AS 1/24/08
			// Do a reference comparison instead of a string comparison
			//
			//switch (e.Property.Name)
			if (e.Property == ViewSettingsProperty)
			{
				//case "ViewSettings":
					if (this._viewSettings != null)
					{
						// Remove the view settings object as our logical child.
						DependencyObject logicalParent = LogicalTreeHelper.GetParent(this._viewSettings);
						if (logicalParent is XamCarouselPanel)
							((XamCarouselPanel)logicalParent).RemoveLogicalChildProxy(this._viewSettings);
						else
						if (logicalParent == this)
							this.RemoveLogicalChild(this._viewSettings);
					}


					// Set our ViewSettings member variable.
					this._viewSettings = e.NewValue as CarouselViewSettings;


					// Bind the XamCarouselPanel's ViewSettings property to our ViewSettings property.
					if (this._carouselPanel != null)
						this.BindCarouselPanelViewSettings(this._carouselPanel);


					// Add the view settings object as our logical child.
					if (this._viewSettings != null)
					{
						// Make sure the new view settings object does not already have a logical parent.
						DependencyObject logicalParent = LogicalTreeHelper.GetParent(this._viewSettings);
						if (logicalParent != null && logicalParent is XamCarouselPanel)
						{
							((XamCarouselPanel)logicalParent).RemoveLogicalChildProxy(this._viewSettings);
							logicalParent = LogicalTreeHelper.GetParent(this._viewSettings);
						}

						if (logicalParent == null)
							this.AddLogicalChild(this._viewSettings);
					}

					//break;
			}

			base.OnPropertyChanged(e);
		}
				#endregion //OnPropertyChanged	
    
				#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the specified container element to display the specified item. 
		/// </summary>
		/// <param name="element">The container element to prepare.</param>
		/// <param name="item">The item contained by the specified container element.</param>
		protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);

			CarouselListBoxItem carouselListBoxItem = element as CarouselListBoxItem;
			if (carouselListBoxItem != null)
				carouselListBoxItem.PrepareContainerForItem(item);

			if (item == this.SelectedItem)
				element.SetValue(XamCarouselListBox.IsSelectedProperty, KnownBoxes.TrueBox);
		}

				#endregion //PrepareContainerForItemOverride

				#region ReactivateContainer

		/// <summary>
		/// Called when a container is being reactivated for the same item.
		/// </summary>
		/// <param name="container">The container being reactivated.</param>
		/// <param name="item">Its associated item.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the original setting for the Visibility property, prior to its deactivation (refer to <see cref="DeactivateContainer"/>), will be restored before a this method is called.</para>
		/// </remarks>
		/// <seealso cref="DeactivateContainer"/>
		/// <seealso cref="ReuseContainerForNewItem"/>
		protected internal override void ReactivateContainer(DependencyObject container, object item)
		{
			base.ReactivateContainer(container, item);

			if (item == this.SelectedItem)
				container.SetValue(XamCarouselListBox.IsSelectedProperty, KnownBoxes.TrueBox);
		}

				#endregion //ReactivateContainer	
    
				#region ReuseContainerForNewItem

		/// <summary>
		/// Called when a container is being reused, i.e. recycled, or a different item.
		/// </summary>
		/// <param name="container">The container being reused/recycled.</param>
		/// <param name="item">The new item.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the container had previously been deactivated then the original setting for the Visibility property, prior to its deactivation (refer to <see cref="DeactivateContainer"/>), will be restored before a this method is called.</para>
		/// </remarks>
		/// <seealso cref="DeactivateContainer"/>
		/// <seealso cref="ReactivateContainer"/>
		protected internal override void ReuseContainerForNewItem(DependencyObject container, object item)
		{
			base.ReuseContainerForNewItem(container, item);

			if (item == this.SelectedItem)
				container.SetValue(XamCarouselListBox.IsSelectedProperty, KnownBoxes.TrueBox);
			else
				container.SetValue(XamCarouselListBox.IsSelectedProperty, KnownBoxes.FalseBox);
		}

				#endregion //ReuseContainerForNewItem	
    
			#endregion //Methods

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				// [JM 05-22-07]
				#region IsSelected

		/// <summary>
		/// Identifies the <see cref="IsSelectedProperty"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty;

		/// <summary>
		/// Gets the value of the <see cref="XamCarouselListBox.IsSelectedProperty"/> property on the specifed element.
		/// </summary>
		/// <param name="element">The element to check for the property value.</param>
		public static bool GetIsSelected(DependencyObject element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			return (bool)element.GetValue(XamCarouselListBox.IsSelectedProperty);
		}

		/// <summary>
		/// Sets the <see cref="XamCarouselListBox.IsSelectedProperty"/> property on the specifed element to the specified value.
		/// </summary>
		/// <param name="element">The element on which to set the property.</param>
		/// <param name="isSelected">The value to set.</param>
		public static void SetIsSelected(DependencyObject element, bool isSelected)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			element.SetValue(XamCarouselListBox.IsSelectedProperty, KnownBoxes.FromValue(isSelected));
		}

				#endregion //IsSelected	

				#region IsSelectionActive (readonly, attached)

		private static readonly DependencyPropertyKey IsSelectionActivePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsSelectionActive",
			typeof(bool), typeof(XamCarouselListBox), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the IsSelectionActive" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsSelectionActive"/>
		public static readonly DependencyProperty IsSelectionActiveProperty =
			IsSelectionActivePropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'IsSelectionActive' attached readonly property
		/// </summary>
		/// <seealso cref="IsSelectionActiveProperty"/>
		public static bool GetIsSelectionActive(DependencyObject d)
		{
			return (bool)d.GetValue(XamCarouselListBox.IsSelectionActiveProperty);
		}

				#endregion //IsSelectionActive (readonly, attached)

				#region IsSynchronizedWithCurrentItem

		/// <summary>
		/// Identifies the <see cref="IsSynchronizedWithCurrentItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty = DependencyProperty.Register("IsSynchronizedWithCurrentItem",
			typeof(bool?), typeof(XamCarouselListBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIsSynchronizedWithCurrentItemChanged)));

		private static void OnIsSynchronizedWithCurrentItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((XamCarouselListBox)d).SetSynchronizationWithCurrentItem();
		}

		/// <summary>
		/// Returns/sets a value that indicates whether the <see cref="XamCarouselListBox"/> should keep the <see cref="SelectedItem"/> synchronized with the current item in the Items property. 
		/// </summary>
		/// <seealso cref="IsSynchronizedWithCurrentItemProperty"/>
		/// <seealso cref="XamCarouselListBox"/>
		//[Description("Returns/sets a value that indicates whether the XamCarouselListBox should keep the SelectedItem synchronized with the current item in the Items property. ")]
		//[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? IsSynchronizedWithCurrentItem
		{
			get
			{
				return (bool?)this.GetValue(XamCarouselListBox.IsSynchronizedWithCurrentItemProperty);
			}
			set
			{
				this.SetValue(XamCarouselListBox.IsSynchronizedWithCurrentItemProperty, value);
			}
		}

				#endregion //IsSynchronizedWithCurrentItem

				#region ScrollInfo

		/// <summary>
		/// Returns a reference to an IScrollInfo interface that can be used to scroll the list and query scrolling information.
		/// </summary>
		public IScrollInfo ScrollInfo
		{
			get { return this._carouselPanel as IScrollInfo; }
		}

				#endregion //ScrollInfo	

				// [JM 05-22-07]
				#region SelectedIndex

		/// <summary>
		/// Identifies the <see cref="SelectedIndex"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex",
			typeof(int), typeof(XamCarouselListBox), new FrameworkPropertyMetadata(-1, new PropertyChangedCallback(XamCarouselListBox.OnSelectedIndexChanged), new CoerceValueCallback(XamCarouselListBox.OnCoerceSelectedIndex)), new ValidateValueCallback(XamCarouselListBox.OnValidateSelectedIndex));

		private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCarouselListBox xamCarouselListBox = d as XamCarouselListBox;
			if (xamCarouselListBox != null)
			{
				List<object> unselectedItems	= new List<object>();
				List<object> selectedItems		= new List<object>();


				xamCarouselListBox._updateSelectedIndex = false;

				try
				{
					IItemContainerGenerator generator = xamCarouselListBox.ActiveItemContainerGenerator;

					// Deselect the existing selection if any.
					// [JM 06-15-07]
					//if ((int)e.OldValue != -1)
					if ((int)e.OldValue != -1 && (int)e.OldValue < xamCarouselListBox.Items.Count)
					{
						unselectedItems.Add(xamCarouselListBox.Items[(int)e.OldValue]);

						CarouselPanelItem oldSelection = xamCarouselListBox.ContainerFromIndex((int)e.OldValue) as CarouselPanelItem;
						if (oldSelection != null)
							oldSelection.SetValue(XamCarouselListBox.IsSelectedProperty, KnownBoxes.FalseBox);

						if (xamCarouselListBox._updateSelectedItem)
							xamCarouselListBox.SetValue(XamCarouselListBox.SelectedItemProperty, null);
					}

					// [JM 06-15-07]
					//if ((int)e.NewValue != -1)
					if ((int)e.NewValue != -1 && (int)e.NewValue < xamCarouselListBox.Items.Count)
					{
						bool scrollNewItemIntoView = false;

						selectedItems.Add(xamCarouselListBox.Items[(int)e.NewValue]);


						// If the element associated with the item is in the list, set IsSelected = true on the element.
						// Otherwise scroll the item into view - we will set IsSelected on the element in PrepareContainerForItemOverride,
						// ReuseContainerForNewItem or ReactivateContainer
						GeneratorPosition generatorPosition = xamCarouselListBox.GetGeneratorPositionFromItemIndex((int)e.NewValue);
						if (generatorPosition.Offset == 0)
						{
							CarouselPanelItem newSelection = xamCarouselListBox.ContainerFromIndex((int)e.NewValue) as CarouselPanelItem;
							if (newSelection != null)
							{
								if (XamCarouselListBox.GetIsSelected(newSelection) != true)
									newSelection.SetValue(XamCarouselListBox.IsSelectedProperty, KnownBoxes.TrueBox);
							}
						}
						else
						{
							// Force item to be scrolled into view.
							scrollNewItemIntoView = true;
						}
						

						if (xamCarouselListBox._updateSelectedItem)
							xamCarouselListBox.SetValue(XamCarouselListBox.SelectedItemProperty, xamCarouselListBox.Items[(int)e.NewValue]);


						xamCarouselListBox.UpdateSelectedValueBinding();


						if (scrollNewItemIntoView)
						{
							if (xamCarouselListBox._carouselPanel != null)
								((IScrollInfo)xamCarouselListBox._carouselPanel).SetVerticalOffset((double)(int)e.NewValue);
						}
					}
				}
				finally { xamCarouselListBox._updateSelectedIndex = true; }


				// [JM 06-14-07]
				xamCarouselListBox.SetCurrentToSelected();


				// Raise the SelectionChanged event.
				SelectionChangedEventArgs args = new SelectionChangedEventArgs(XamCarouselListBox.SelectionChangedEvent, unselectedItems,selectedItems);
				// AS 6/8/07
				// I noticed that we were raising the event without setting the RoutedEvent property or source.
				//
				//xamCarouselListBox.OnSelectionChanged(args);
				xamCarouselListBox.RaiseSelectionChanged(args);

				// AS 6/8/07 UI Automation
				XamCarouselListBoxAutomationPeer peer = UIElementAutomationPeer.FromElement(xamCarouselListBox) as XamCarouselListBoxAutomationPeer;

				if (null != peer)
					peer.RaiseSelectionChanged(args);
			}
		}

		private static object OnCoerceSelectedIndex(DependencyObject d, object value)
		{
			XamCarouselListBox xamCarouselListBox = d as XamCarouselListBox;
			if (xamCarouselListBox != null  &&
				(value is int)				&& 
				(int)value >= xamCarouselListBox.Items.Count)
			{
				return DependencyProperty.UnsetValue;
			}

			return value;
		}

		private static bool OnValidateSelectedIndex(object value)
		{
			return (int)value >= -1;
		}


		/// <summary>
		/// Returns/sets the index of the currently selected item in the list.
		/// </summary>
		/// <seealso cref="SelectedIndexProperty"/>
		//[Description("Returns/sets the index of the currently selected item in the list.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public int SelectedIndex
		{
			get
			{
				return (int)this.GetValue(XamCarouselListBox.SelectedIndexProperty);
			}
			set
			{
				this.SetValue(XamCarouselListBox.SelectedIndexProperty, value);
			}
		}

				#endregion //SelectedIndex

				// [JM 05-22-07]
				#region SelectedItem

		/// <summary>
		/// Identifies the <see cref="SelectedItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
			typeof(object), typeof(XamCarouselListBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemChanged)));

		private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCarouselListBox xamCarouselListBox = d as XamCarouselListBox;
			if (xamCarouselListBox != null)
			{
				if (xamCarouselListBox._updateSelectedIndex)
				{
					xamCarouselListBox._updateSelectedItem = false;

					try
					{
						xamCarouselListBox.SetValue(XamCarouselListBox.SelectedIndexProperty, xamCarouselListBox.Items.IndexOf(e.NewValue));
					}
					finally
					{ xamCarouselListBox._updateSelectedItem = true; }
				}
			}
		}

		/// <summary>
		/// Returns/sets the currently selected item in the list.
		/// </summary>
		/// <seealso cref="SelectedItemProperty"/>
		//[Description("Returns/sets the currently selected item in the list.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public object SelectedItem
		{
			get
			{
				return (object)this.GetValue(XamCarouselListBox.SelectedItemProperty);
			}
			set
			{
				this.SetValue(XamCarouselListBox.SelectedItemProperty, value);
			}
		}

				#endregion //SelectedItem

				#region SelectedValue

		private static readonly DependencyPropertyKey SelectedValuePropertyKey =
			DependencyProperty.RegisterReadOnly("SelectedValue",
			typeof(object), typeof(XamCarouselListBox), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="SelectedValue"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedValueProperty =
			SelectedValuePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the object that us at the specified <see cref="SelectedValuePath"/> of the <see cref="SelectedItem"/>, or null if no item is selected.  If <see cref="SelectedValuePath"/> has not been set, then the <see cref="SelectedItem"/> is returned (read-only).  The default value is null.
		/// </summary>
		/// <seealso cref="SelectedValueProperty"/>
		/// <seealso cref="SelectedValuePath"/>
		/// <seealso cref="SelectedValuePathProperty"/>
		//[Description("Returns the object that us at the specified SelectedValuePath of the SelcetedItem, or null if no item is selected.  If SelectedValuePath has not been set, then the SelectedItem is returned (read-only).  The default value is null.")]
		//[Category("Data")]
		[Bindable(true)]
		[ReadOnly(true)]
		public object SelectedValue
		{
			get
			{
				return (object)this.GetValue(XamCarouselListBox.SelectedValueProperty);
			}
		}

				#endregion //SelectedValue

				#region SelectedValuePath

		/// <summary>
		/// Identifies the <see cref="SelectedValuePath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath",
			typeof(string), typeof(XamCarouselListBox), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedValuePathChanged)));

		private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCarouselListBox xamCarouselListBox = d as XamCarouselListBox;
			if (xamCarouselListBox != null)
				xamCarouselListBox.UpdateSelectedValueBinding();
		}

		/// <summary>
		/// Returns/sets a string that contains the path that is used to get the <see cref="SelectedValue"/>.  The default value is String.Empty.
		/// </summary>
		/// <seealso cref="SelectedValue"/>
		/// <seealso cref="SelectedValueProperty"/>
		/// <seealso cref="SelectedValuePathProperty"/>
		//[Description("Returns/sets a string that contains the path that is used to get the SelectedValue.  The default value is String.Empty.")]
		//[Category("Data")]
		[Bindable(true)]
		public string SelectedValuePath
		{
			get
			{
				return (string)this.GetValue(XamCarouselListBox.SelectedValuePathProperty);
			}
			set
			{
				this.SetValue(XamCarouselListBox.SelectedValuePathProperty, value);
			}
		}

				#endregion //SelectedValuePath
	
				#region ViewSettings

		/// <summary>
		/// Identifies the <see cref="ViewSettings"/> dependency property
		/// </summary>
		// AS 6/29/07
		// Changed to an owned property since there is no need to have separate
		// definitions plus there were some cases where we set one instead of 
		// the other.
		//
		//public static readonly DependencyProperty ViewSettingsProperty = DependencyProperty.Register("ViewSettings",
		//	typeof(CarouselViewSettings), typeof(XamCarouselListBox), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceViewSettings)));
		public static readonly DependencyProperty ViewSettingsProperty = XamCarouselPanel.ViewSettingsProperty.AddOwner(
			typeof(XamCarouselListBox), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceViewSettings)));

		private static object OnCoerceViewSettings(DependencyObject d, object value)
		{
            if (value == null)
            {
                // JJD 1/2/07 
                // Check for design mode first
                // Only create a ViewSettings if we are in run mode 
                if (DesignerProperties.GetIsInDesignMode(d) == false)
                    return new CarouselViewSettings();
            }

			return value;
		}

		/// <summary>
		/// Returns/set the <see cref="CarouselViewSettings"/> object for this <see cref="XamCarouselListBox"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="CarouselViewSettings"/> object exposes properties for controlling all aspects of the Carousel view, including
		/// the number of items to display per page (<see cref="CarouselViewSettings.ItemsPerPage"/>), the path geometry used to create the path along which items in the list are arranged
		/// (<see cref="CarouselViewSettings.ItemPath"/>) and the various parent effects that can be applied to items as they are displayed at different points along the path
		/// (<see cref="CarouselViewSettings.OpacityEffectStops"/>, <see cref="CarouselViewSettings.ScalingEffectStops"/>, <see cref="CarouselViewSettings.SkewAngleXEffectStops"/>,
		/// <see cref="CarouselViewSettings.SkewAngleYEffectStops"/>, <see cref="CarouselViewSettings.ZOrderEffectStops"/>).</p>
		/// </remarks>
		/// <seealso cref="ViewSettingsProperty"/>
		/// <seealso cref="CarouselViewSettings"/>
		//[Description("Returns/set the CarouselViewSettings object for this XamCarouselListBox.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public CarouselViewSettings ViewSettings
		{
			get
			{
                // JJD 1/2/07 
                // Check for design mode first
                // If we are in design mode don't create an instance of the Settings object
                //if (this._viewSettings == null)
                if (this._viewSettings == null &&
                    DesignerProperties.GetIsInDesignMode(this) == false)
                    this._viewSettings = new CarouselViewSettings();

				return this._viewSettings;
			}
			set
			{
				this.SetValue(XamCarouselListBox.ViewSettingsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ViewSettings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeViewSettings()
		{
			return this.ViewSettings != (CarouselViewSettings)XamCarouselListBox.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ViewSettings"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetViewSettings()
		{
			this.ViewSettings = (CarouselViewSettings)XamCarouselListBox.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

				#endregion //ViewSettings
    
			#endregion //Public Properties

			#region Private Properties

				#region ActiveItemContainerGenerator

		/// <summary>
		/// Returns the active item generator.
		/// </summary>
		/// <value>If the </value>
		protected IItemContainerGenerator ActiveItemContainerGenerator
		{
			get
			{
				ItemContainerGenerationMode mode				= ItemContainerGenerationMode.Virtualize;
				RecyclingItemsPanel			recyclingItemsPanel = this._carouselPanel as RecyclingItemsPanel;

				if (recyclingItemsPanel != null)
					mode = recyclingItemsPanel.ItemContainerGenerationModeResolved;

				if (mode == ItemContainerGenerationMode.Recycle)
					return this.RecyclingItemContainerGenerator;

				return this.ItemContainerGenerator;
			}
		}

				#endregion //ActiveItemContainerGenerator

				#region SelectedValueInternal

		private static readonly DependencyProperty SelectedValueInternalProperty = DependencyProperty.Register("SelectedValueInternal",
			typeof(object), typeof(XamCarouselListBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedValueInternalChanged)));

		private static void OnSelectedValueInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCarouselListBox xamCarouselListBox = d as XamCarouselListBox;
			if (xamCarouselListBox != null)
				xamCarouselListBox.SetValue(XamCarouselListBox.SelectedValuePropertyKey, e.NewValue);
		}

		private object SelectedValueInternal
		{
			get	{ return (object)this.GetValue(XamCarouselListBox.SelectedValueInternalProperty); }
			set	{ this.SetValue(XamCarouselListBox.SelectedValueInternalProperty, value); }
		}

				#endregion //SelectedValueInternal

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region BindCarouselPanelViewSettings

		private void BindCarouselPanelViewSettings(XamCarouselPanel carouselPanel)
		{
			Debug.Assert(carouselPanel != null, "carouselPanel is null in BindCarouselPanelViewSettings!");
			if (carouselPanel == null)
				return;


			if (this._viewSettings == null)
			{
				BindingOperations.ClearAllBindings(carouselPanel);
				return;
			}

			Binding binding = new Binding();
			binding.Source	= this;
			binding.Mode	= BindingMode.OneWay;
			binding.Path	= new PropertyPath("ViewSettings");
			carouselPanel.SetBinding(XamCarouselPanel.ViewSettingsProperty, binding);
		}

			#endregion //BindCarouselPanelViewSettings	

				#region ContainerFromIndex

		private DependencyObject ContainerFromIndex(int itemIndex)
		{
			if (this.ActiveItemContainerGenerator is RecyclingItemContainerGenerator)
				return this.RecyclingItemContainerGenerator.GetContainer(this.GetChildIndexFromItemIndex(itemIndex), false);

			return this.ItemContainerGenerator.ContainerFromIndex(itemIndex);
		}

				#endregion //ContainerFromIndex	

				#region GetChildIndexFromItemIndex



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private int GetChildIndexFromItemIndex(int itemIndex)
		{
			GeneratorPosition		generatorPosition	= this.GetGeneratorPositionFromItemIndex(itemIndex);
			return (generatorPosition.Offset == 0) ? generatorPosition.Index :
													 generatorPosition.Index + 1;
		}

				#endregion //GetChildIndexFromItemIndex

				#region GetGeneratorPositionFromItemIndex



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private GeneratorPosition GetGeneratorPositionFromItemIndex(int itemIndex)
		{
			return this.ActiveItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);
		}

				#endregion //GetGeneratorPositionFromItemIndex

				#region IsXmlNode

		private bool IsXmlNode(object item)
		{
			if ((item != null) && item.GetType().FullName.StartsWith("System.Xml", StringComparison.Ordinal))
				return item is XmlNode;

			return false;
		}

				#endregion //IsXmlNode	
    
				// AS 6/8/07 UI Automation
				#region OnIsSelectedChanged
		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCarouselListBox list = ItemsControl.ItemsControlFromItemContainer(d) as XamCarouselListBox;

			if (null != list)
			{
				XamCarouselListBoxAutomationPeer listPeer = UIElementAutomationPeer.FromElement(list) as XamCarouselListBoxAutomationPeer;
				listPeer.RaiseIsSelectedChanged(d, (bool)e.NewValue);
			}
		} 
				#endregion //OnIsSelectedChanged

				// [JM 06-14-07]
				#region OnItemsCurrentChanged

		private void OnItemsCurrentChanged(object sender, EventArgs e)
		{
			if (this._isSynchronizedWithCurrentItemPrivate)
				this.SetSelectedToCurrent();
		}

				#endregion //OnItemsCurrentChanged	

				// [JM 06-14-07]
				#region SetCurrentToSelected

		private void SetCurrentToSelected()
		{
			if (this.SelectedItem == null)
				base.Items.MoveCurrentToPosition(-1);
			else
				base.Items.MoveCurrentTo(this.SelectedItem);
		}

				#endregion //SetCurrentToSelected	
    
				#region SetSelectedToCurrent

		private void SetSelectedToCurrent()
		{
			object item = base.Items.CurrentItem;
			if (item != null)
				this.SelectedItem = item;
		}

				#endregion //SetSelectedToCurrent	
    
				#region SetSynchronizationWithCurrentItem

		private void SetSynchronizationWithCurrentItem()
		{
			bool	synchronize = false;
			bool?	isSynchronizedWithCurrentItem					= this.IsSynchronizedWithCurrentItem;
			bool	isSynchronizedWithCurrentItemPrivateOriginal	= this._isSynchronizedWithCurrentItemPrivate;

			if (isSynchronizedWithCurrentItem.HasValue)
				synchronize = isSynchronizedWithCurrentItem.Value;

			this._isSynchronizedWithCurrentItemPrivate = synchronize;
			if (!isSynchronizedWithCurrentItemPrivateOriginal && synchronize)
				this.SetSelectedToCurrent();
		}

				#endregion //SetSynchronizationWithCurrentItem	
   
				#region UpdateSelectedValueBinding

		private void UpdateSelectedValueBinding()
		{
			BindingOperations.ClearBinding(this, XamCarouselListBox.SelectedValueInternalProperty);

			if (this.SelectedItem == null)
				return;

			if (this.SelectedValuePath == null || this.SelectedValuePath == string.Empty)
				return;

			Binding b	= new Binding();
			b.Source	= this.SelectedItem;

			if (this.IsXmlNode(this.SelectedItem))
			{
				b.XPath = this.SelectedValuePath;
				b.Path	= new PropertyPath("/InnerText", new object[0]);
			}
			else
				b.Path	= new PropertyPath(this.SelectedValuePath, new object[0]);

			this.SetBinding(XamCarouselListBox.SelectedValueInternalProperty, b);
		}

				#endregion //UpdateSelectedValueBinding	
    
			#endregion //Private Methods

		#endregion //Methods

		#region Events

		#region SelectionChanged

		/// <summary>
		/// Event ID for the <see cref="SelectionChanged"/> routed event
		/// </summary>
		/// <seealso cref="SelectedIndex"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="OnSelectionChanged"/>
		/// <seealso cref="SelectionChangedEvent"/>
		/// <seealso cref="SelectionChangedEventArgs"/>
		public static readonly RoutedEvent SelectionChangedEvent =
			EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(XamCarouselListBox));

		/// <summary>
		/// Occurs when the <see cref="SelectedItem"/> has changed
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> Items can be selected via the <see cref="SelectedIndex"/> and <see cref="SelectedItem"/> properties.</para>
		/// </remarks>
		/// <seealso cref="SelectedIndex"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="OnSelectionChanged"/>
		/// <seealso cref="SelectionChangedEvent"/>
		/// <seealso cref="SelectionChangedEventArgs"/>
		protected virtual void OnSelectionChanged(SelectionChangedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseSelectionChanged(SelectionChangedEventArgs args)
		{
			args.RoutedEvent = XamCarouselListBox.SelectionChangedEvent;
			args.Source = this;
			this.OnSelectionChanged(args);
		}

		/// <summary>
		/// Occurs when the <see cref="SelectedItem"/> has changed
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> Items can be selected via the <see cref="SelectedIndex"/> and <see cref="SelectedItem"/> properties.</para>
		/// </remarks>
		/// <seealso cref="SelectedIndex"/>
		/// <seealso cref="SelectedItem"/>
		/// <seealso cref="OnSelectionChanged"/>
		/// <seealso cref="SelectionChangedEvent"/>
		/// <seealso cref="SelectionChangedEventArgs"/>
		//[Description("Occurs when the Selection has changed")]
		//[Category("Behavior")]
		public event SelectionChangedEventHandler SelectionChanged
		{
			add
			{
				base.AddHandler(XamCarouselListBox.SelectionChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamCarouselListBox.SelectionChangedEvent, value);
			}
		}

			#endregion //SelectionChanged

		#endregion //Events

		#region ICarouselPanelSelectionHost Members

		#region SelectedItemIndex

		/// <summary>
		/// Returns/sets the index of the currently selected item.
		/// </summary>
		/// <returns>The index of the currently selected item or if more than 1 item is selected this property returns/sets the first selected item.
		/// If no item is currently selected, -1 is returned.</returns>
		int ICarouselPanelSelectionHost.SelectedItemIndex
		{
			get { return this.SelectedIndex; }
			set	{ this.SelectedIndex = value; }
		}

			#endregion //SelectedItemIndex	
    
		#endregion //ICarouselPanelSelectionHost Members
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