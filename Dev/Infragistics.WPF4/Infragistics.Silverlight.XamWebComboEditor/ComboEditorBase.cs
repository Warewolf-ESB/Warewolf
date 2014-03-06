using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Controls.Editors.Primitives;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Interop;
using Infragistics.AutomationPeers;
using System.Text.RegularExpressions;
using Infragistics.Collections;
using System.Windows.Documents;
using System.Reflection;


using System.Data;


namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// Represents a selection control with a drop-down list that can be shown or hidden by clicking the arrow on the control.
    /// </summary>
    [TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]

    [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]

    [TemplateVisualState(Name = "Selectable", GroupName = "ModeStates")]
    [TemplateVisualState(Name = "Editable", GroupName = "ModeStates")]

    [TemplateVisualState(Name = "WaterMarkHidden", GroupName = "WaterMarkStates")]
    [TemplateVisualState(Name = "WaterMarkVisible", GroupName = "WaterMarkStates")]

    [TemplateVisualState(Name = "Closed", GroupName = "DropDownStates")]
    [TemplateVisualState(Name = "Open", GroupName = "DropDownStates")]

    [TemplatePart(Name = "VerticalScrollBar", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "HorizontalScrollBar", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "TextBoxPresenter", Type = typeof(TextBox))]
    [TemplatePart(Name = "ItemsPanel", Type = typeof(ItemsPanel))]
    [TemplatePart(Name = "RootPopupElement", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "MultiSelectConentPanel", Type = typeof(Panel))]
	[TemplatePart(Name = "ToggleButton", Type = typeof(ToggleButton))]	// JM 05-26-11 Port to WPF

    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ComboEditorItemControl))]
    [ComplexBindingProperties("ItemsSource")]
    public abstract class ComboEditorBase<T, TControl> : Control, IProvideDataItems<T>, INotifyPropertyChanged, IProvidePropertyPersistenceSettings, ISupportScrollHelper where T : ComboEditorItemBase<TControl> where TControl : FrameworkElement
    {
        #region Members

        private bool _isLoaded, _ignoreSelectionChanging, _mouseIsOver, _ignoreIsDropDownChanging, _supressSelectionChangedEvent;
        private string _searchedText, _previousText;
        private ItemsPanelBase<T, TControl> _panel;
        private TimeSpan _timeoutTimeSpan = TimeSpan.FromMilliseconds(1000);
        private DispatcherTimer _timeoutTracker, _delayTracker;
        private Dictionary<object, T> _cachedRows;
        private ObservableCollection<object> _selectedItems, _backupSelectedItems;
        private DataManagerBase _dataManager, _equalsDataManager;
        private BindableItemCollection<T> _items;
        private Dictionary<object, int> _duplicateObjectValidator;
        private FrameworkElement _rootPopupElement;
        private ComboEditorItemValueProxy _valueProxy;
        private List<T> _startsWithList;
        private T _focusItem;
        private Panel _multiSelectPanel;
        private Popup _dropDownPopup;
        private LastInvokeAction _lastAction;

		private bool _isEditableResolved = true;			// value matches the default value of the related DependencyProperty in derived classes.
		private bool _allowFilteringResolved = true;		// value matches the default value of the related DependencyProperty in derived classes.
		private bool _openDropDownOnTypingResolved = true;	// value matches the default value of the related DependencyProperty in derived classes.
		private FilterMode _filterModeResolved = FilterMode.FilterOnAllColumns;	// value matches the default value of the related DependencyProperty in derived classes.
		private bool _allowDropDownResizingResolved = true; // value matches the default value of the related DependencyProperty in derived classes.

		// JM 8-2-11 - XamMultiColumnComboEditor
		private string _currentSearchText;
		private double _lastDropdownWidth = double.NaN;
        private double _lastSetHeight = double.NaN;
        private bool _setDropDownPostion = true, _updateDropDownPosition;

		// JM 05-26-11 Port to WPF

		private LostFocusTracker	_comboLostFocusTracker;
		private bool				_windowEventsHooked;


		private Window _window;		// JM 10-5-11 TFS90426

		private bool _skipDropDownClosingOnMouseClick;	// JM 10-14-11 TFS91993





		// JM 11-29-11 TFS96420

		private bool _applySelectedIndexWhenItemsSourceChanges;


		// JM 01-11-12 TFS98440
		private string _firstFieldName;

		// JM 02-13-12 TFS100051
		private bool _isTextCommitted = true;

		// JM 02-17-12 TFS98565
		private IEnumerable<DataField> _dataFieldsCache;

		// JM 02-22-12 TFS100340
		private ToggleButton _toggleButton;

		// JM 02-23-12 TFS100053, TFS100158
		private TouchScrollHelper _touchScrollHelper;




        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the ComboEditorBase class.
        /// </summary>
        protected ComboEditorBase()
        {
            this._cachedRows = new Dictionary<object, T>();

            this.Loaded += this.XamWebComboEditor_Loaded;
            this.Unloaded += new RoutedEventHandler(XamComboEditor_Unloaded);
            this.SizeChanged += this.XamWebComboEditor_SizeChanged;
            this.IsEnabledChanged += this.XamWebComboEditor_IsEnabledChanged;

            this._timeoutTracker = new DispatcherTimer();
            this._timeoutTracker.Interval = this._timeoutTimeSpan;
            this._timeoutTracker.Tick += new EventHandler(TimeoutTracker_Tick);

            this._delayTracker = new DispatcherTimer();
            this._delayTracker.Interval = new TimeSpan(0, 0, this.AutoCompleteDelay / 1000);
            this._delayTracker.Tick += this.DelayTracker_Tick;

            this._duplicateObjectValidator = new Dictionary<object, int>();
            this._valueProxy = new ComboEditorItemValueProxy();

            this.SizeChanged += new SizeChangedEventHandler(ComboEditorBase_SizeChanged);

            this.AddHandler(ComboEditorBase<T, TControl>.KeyDownEvent, new KeyEventHandler(this.XamWebComboEditor_KeyDown), true);
            this.AddHandler(ComboEditorBase<T, TControl>.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.XamWebComboEditor_MouseLeftButtonDown), true);


			// JM 01-23-12 TFS99917 
			this.AddHandler(ComboEditorBase<T, TControl>.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(this.XamWebComboEditor_PreviewMouseLeftButtonDown), true);


			// JM 05-24-11 Port to WPF.  Moved this here from the default style in generic.xaml.



			KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Once);




#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion //Constructor

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Builds the visual tree for the ComboEditorBase when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.Editor != null)
            {
                this.Editor.TextChanged -= this.Editor_TextChanged;
                this.Editor.LostFocus -= Editor_LostFocus;
                this.Editor.GotFocus -= Editor_GotFocus;
                this.Editor.RemoveHandler(TextBox.KeyDownEvent, new KeyEventHandler(Editor_KeyDown));
            }

            this.Editor = this.GetTemplateChild("TextBoxPresenter") as TextBox;

            if (this.Editor != null)
            {
                this.Editor.TextChanged += this.Editor_TextChanged;
                this.Editor.LostFocus += new RoutedEventHandler(Editor_LostFocus);
                this.Editor.GotFocus += new RoutedEventHandler(Editor_GotFocus);
                this.Editor.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(Editor_KeyDown), true);
            }

            if (this.VerticalScrollBar != null)
            {
                this.VerticalScrollBar.ValueChanged -= this.VerticalScrollBar_ValueChanged;
            }

            this.VerticalScrollBar = this.GetTemplateChild("VerticalScrollBar") as ScrollBar;
            if (this.VerticalScrollBar != null)
            {
                this.VerticalScrollBar.ValueChanged += this.VerticalScrollBar_ValueChanged;
                this.VerticalScrollBar.Orientation = Orientation.Vertical;
                this.VerticalScrollBar.Visibility = Visibility.Collapsed;
            }

            if (this.HorizontalScrollBar != null)
            {
                this.HorizontalScrollBar.ValueChanged -= this.HorizontalScrollBar_ValueChanged;
            }

            this.HorizontalScrollBar = this.GetTemplateChild("HorizontalScrollBar") as ScrollBar;
            if (this.HorizontalScrollBar != null)
            {
                this.HorizontalScrollBar.ValueChanged += this.HorizontalScrollBar_ValueChanged;
                this.HorizontalScrollBar.Orientation = Orientation.Horizontal;
                this.HorizontalScrollBar.Visibility = Visibility.Collapsed;
            }

            if (this._panel != null)
            {
                RecyclingManager.Manager.ReleaseAll(this._panel);
                this._panel.Children.Clear();
                this._panel.ComboEditor = null;

				// JM 02-23-12 TFS100053, TFS100158
                this._touchScrollHelper = null;
			}

            this._panel = this.GetTemplateChild("ItemsPanel") as ItemsPanelBase<T, TControl>;
            if (this._panel != null)
            {
                this._panel.ComboEditor = this;

				// JM 02-23-12 TFS100053, TFS100158
				this._panel.Background = new SolidColorBrush(Colors.Transparent);
				this._touchScrollHelper = new TouchScrollHelper(this._panel, this);

				// JM 03-22-12 TFS105811
				this._touchScrollHelper.IsEnabled = this.IsTouchSupportEnabled;


				this._panel.SetCurrentValue(FrameworkElement.IsManipulationEnabledProperty, this.IsTouchSupportEnabled);

			}

            if (this._rootPopupElement != null)
            {
                this._rootPopupElement.MouseEnter -= RootPoupElement_MouseEnter;
                this._rootPopupElement.MouseLeave -= RootPoupElement_MouseLeave;
                this._rootPopupElement.MouseWheel -= RootPoupElement_MouseWheel;
            }

            this._rootPopupElement = this.GetTemplateChild("RootPopupElement") as FrameworkElement;

            if (this._rootPopupElement != null)
            {
                this._rootPopupElement.MouseEnter += new MouseEventHandler(RootPoupElement_MouseEnter);
                this._rootPopupElement.MouseLeave += new MouseEventHandler(RootPoupElement_MouseLeave);
                this._rootPopupElement.MouseWheel += new MouseWheelEventHandler(RootPoupElement_MouseWheel);
            }

            this._multiSelectPanel = this.GetTemplateChild("MultiSelectConentPanel") as Panel;

            this._dropDownPopup = this.GetTemplateChild("Popup") as Popup;

            if (this.SelectedItem != null)
            {
                this.InvalidateSelectedContent();
            }

			// JM 02-22-12 TFS100340 - Use Member variable for toggle button instead of stack variable.

			// JM 05-26-11 Port to WPF
			this._toggleButton = this.GetTemplateChild("ToggleButton") as ToggleButton;

			//ToggleButton toggleButton = this.GetTemplateChild("ToggleButton") as ToggleButton;
			//if (null != toggleButton)
			//	toggleButton.ClickMode = ClickMode.Press;
			if (null != this._toggleButton)
				this._toggleButton.ClickMode = ClickMode.Press;


			this._lastSetHeight = double.NaN;	// JM 10-12-11 TFS91670
			this._rootPopupElement.MinWidth = this.ActualWidth;	// JM 10-17-11 TFS92259

			this.EnsureVisualStates();

			// JM 11-16-11 TFS96080 - In Silverlight we need to hide the mouseover border of the TextBox.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			// JM 10-24-11 TFS93718
			CommandSourceManager.NotifyCanExecuteChanged(typeof(MultiColumnComboEditorClearSelectionCommand));
        }

        #endregion //OnApplyTemplate

        #region OnLostFocus

        /// <summary>
        /// Called before the LostFocus event is raised.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Unfocused", true);






            base.OnLostFocus(e);
        }

		#endregion // OnLostFocus

		#region OnGotFocus

		/// <summary>
        /// Called before the GotFocus event is raised.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Focused", true);

            base.OnGotFocus(e);
        }
        #endregion // OnGotFocus

        #region OnCreateAutomationPeer
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamComboEditorAutomationPeer<T, TControl>(this);
        }
        #endregion //OnCreateAutomationPeer

        #endregion // Overrides

        #region Properties

        #region Public

        #region IsDropDownOpen

        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(new PropertyChangedCallback(ComboEditorBase<T, TControl>.OnIsDropDownOpenChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the selectable drop-down is open.
        /// </summary>                       
        public bool IsDropDownOpen
        {
            get { return (bool)this.GetValue(IsDropDownOpenProperty); }
            set { this.SetValue(IsDropDownOpenProperty, value); }
        }

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)d;

            // Ignore the IsDropDown property when set in the designer, otherwise it can cause VS to crash.
			// JM 05-24-11 Port to WPF.



			if (combo.IsInDesignMode)

            {
                combo.IsDropDownOpen = false;
                return;
            }

            // We only want to do some calculations when the dropdown is first dropped down. 
            combo._setDropDownPostion = true;
            combo._updateDropDownPosition = true;

            if (!combo._isLoaded && ((bool)e.NewValue))
            {
                combo._ignoreIsDropDownChanging = true;
                combo.IsDropDownOpen = false;
                combo._ignoreIsDropDownChanging = false;
                return;
            }

            if (!combo._ignoreIsDropDownChanging)
            {
                combo._mouseIsOver = false;

                if ((bool)e.NewValue) // Opening
                {
					// JM 02-21-12 TFS102346
					combo.SetFocusToControlOrEditor();

                    combo._ignoreIsDropDownChanging = true;
                    combo.IsDropDownOpen = false;
                    combo._ignoreIsDropDownChanging = false;

                    if (!combo.OnDropDownOpening())
                    {
                        combo._ignoreIsDropDownChanging = true;
                        combo.IsDropDownOpen = true;
                        combo._ignoreIsDropDownChanging = false;

                        combo.EnsureVisualStates();

                        // If there isn't any text, be sure to clear the filters, as one shouldn't be applied then.
                        if (combo.TextEditor != null && string.IsNullOrEmpty(combo.TextEditor.Text))
                            combo.ClearFilters();

						// JM 05-24-11 Port to WPF.


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

						combo.EnableCloseUpTriggerTrackers();


						if (combo._rootPopupElement != null)
						{
							if (combo.AllowDropDownResizingResolved == true && double.IsNaN(combo._lastDropdownWidth) == false)
								combo._rootPopupElement.Width = combo._lastDropdownWidth;
							else
								combo._rootPopupElement.Width = combo.ActualWidth;
						}

                        if (combo._focusItem == null && combo.Items.Count > 0)
                        {
							// JM 08-07-12 TFS118024
							//combo.SetFocusedItem(combo.Items[0], true);
							combo.SetFocusToNextEnabledItem(combo.Items[0], true, true);
						}
                        else
                        {
                            DataManagerBase manager = combo.DataManager;

                            if (manager != null && combo.Items.Count > 0)
                            {
                                int index = -1;
                                
                                if (combo.SelectedItem != null)
                                {
                                    index = combo.DataManager.ResolveIndexForRecord(combo.SelectedItem);
                                }

                                if (index == -1 && combo._focusItem != null)
                                {
                                    index = combo.DataManager.ResolveIndexForRecord(combo._focusItem.Data);
                                }

                                if (index == -1)
                                {
									// JM 08-07-12 TFS118024
									//combo.SetFocusedItem(combo.Items[0], true);
									combo.SetFocusToNextEnabledItem(combo.Items[0], true, true);
								}
                                else
                                {
                                    T item = combo.Items[index];
									if (!item.IsFocused)
									{
										// JM 08-07-12 TFS118024
										//combo.SetFocusedItem(item, true);
										combo.SetFocusToNextEnabledItem(item, true, true);
									}

                                    combo.ScrollItemIntoView(item, true);
                                }
                            }

                        }

                        combo.InvalidateDropDownPosition(false);

                        combo.OnDropDownOpened();

						if (combo.IsEditableResolved == false)
							combo.Focus();
                    }


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				}
                else // Closing
                {
                    if (combo._isLoaded)
                    {
                        combo._ignoreIsDropDownChanging = true;
                        combo.IsDropDownOpen = true;
                        combo._ignoreIsDropDownChanging = false;
                    }

                    if (!combo._isLoaded || !combo.OnDropDownClosing())
                    {
                        combo._ignoreIsDropDownChanging = true;
                        combo.IsDropDownOpen = false;
                        combo._ignoreIsDropDownChanging = false;

                        combo.EnsureVisualStates();

						// JM 05-24-11 Port to WPF.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

						combo.DisableCloseUpTriggerTrackers();


                        // Be sure to clear the filters:
                        combo.ClearFilters();

                        combo.OnDropDownClosed();

                        if (combo._panel != null)
                            combo._panel.DropDownClosed();

						combo.CurrentSearchText = string.Empty;

						if (combo._rootPopupElement != null)
							combo._lastDropdownWidth = combo._rootPopupElement.ActualWidth;
					}
                }
            }
        }

        #endregion // IsDropDownOpen

		// JJD 03/14/12 - TFS100150 - Added touch support
		#region IsTouchSupportEnabled

		/// <summary>
		/// Identifies the <see cref="IsTouchSupportEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTouchSupportEnabledProperty = DependencyPropertyUtilities.Register("IsTouchSupportEnabled",
			typeof(bool), typeof(ComboEditorBase<T, TControl>),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsTouchSupportEnabledChanged))
			);

		private static void OnIsTouchSupportEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ComboEditorBase<T, TControl> instance = (ComboEditorBase<T, TControl>)d;

			// JM 03-22-12 TFS105811 - Add null check
			if (instance._touchScrollHelper != null)
				instance._touchScrollHelper.IsEnabled = (bool)e.NewValue;


			if ( instance._panel != null )
				instance._panel.SetCurrentValue(FrameworkElement.IsManipulationEnabledProperty, KnownBoxes.FromValue((bool)e.NewValue));

		}

		/// <summary>
		/// Returns or sets whether touch support is enabled for this control
		/// </summary>
		/// <seealso cref="IsTouchSupportEnabledProperty"/>
		public bool IsTouchSupportEnabled
		{
			get
			{
				return (bool)this.GetValue(ComboEditorBase<T, TControl>.IsTouchSupportEnabledProperty);
			}
			set
			{
				this.SetValue(ComboEditorBase<T, TControl>.IsTouchSupportEnabledProperty, value);
			}
		}

		#endregion //IsTouchSupportEnabled

        #region ItemContainerStyle

        /// <summary>
        /// Identifies the <see cref="ItemContainerStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(new PropertyChangedCallback(ItemContainerStyleChanged)));

        /// <summary>
        /// Gets/sets the style that will be applied to all <see cref="ComboEditorItemControl"/>'s in the dropdown of this combo.
        /// </summary>
        public Style ItemContainerStyle
        {
            get { return (Style)this.GetValue(ItemContainerStyleProperty); }
            set { this.SetValue(ItemContainerStyleProperty, value); }
        }

        private static void ItemContainerStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;
            combo.OnPropertyChanged("ItemContainerStyle");
        }

        #endregion // ItemContainerStyle

        #region MaxDropDownHeight

        /// <summary>
        /// Identifies the <see cref="MaxDropDownHeight"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(double.PositiveInfinity, new PropertyChangedCallback(MaxDropDownHeightChanged)));

        /// <summary>
        /// Gets/Sets the maximum height the dropdown of this ComboEditorBase can be. 
        /// </summary>
        public double MaxDropDownHeight
        {
            get { return (double)this.GetValue(MaxDropDownHeightProperty); }
            set { this.SetValue(MaxDropDownHeightProperty, value); }
        }

        private static void MaxDropDownHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> editor = (ComboEditorBase<T, TControl>)obj;
            editor.OnPropertyChanged("MaxDropDownHeight");
        }

        #endregion // MaxDropDownHeight

        #region SelectedIndex

        /// <summary>
        /// Identifies the <see cref="SelectedIndex"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(-1, new PropertyChangedCallback(SelectedIndexChanged)));

        /// <summary>
        /// Gets/Sets the index of the selected item in this ComboEditorBase.
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)this.GetValue(SelectedIndexProperty); }
            set { this.SetValue(SelectedIndexProperty, value); }
        }

        private static void SelectedIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;

            if (!combo._ignoreSelectionChanging)
                combo.SelectItem((int)e.NewValue, true);

			// JM 11-29-11 TFS96420

			if (combo.ItemsSource == null)
				combo._applySelectedIndexWhenItemsSourceChanges = true;

        }

        #endregion // SelectedIndex

        #region SelectedItem

        /// <summary>
        /// Identifies the <see cref="SelectedItem"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(null, new PropertyChangedCallback(SelectedItemChanged)));

        /// <summary>
        /// Gets/Sets the selected data item of this ComboEditorBase
        /// <para>Note: the item will be of your data model, not of type <see cref="ComboEditorItem"/></para>
        /// </summary>
        public object SelectedItem
        {
            get { return (object)this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

        private static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;

            if (!combo._ignoreSelectionChanging)
                combo.SelectItem((object)e.NewValue, true);
        }

        #endregion // SelectedItem

        #region SelectedItems

        /// <summary>
        /// Gets the currently selected items.
        /// </summary>               
        //[EditorBrowsable(EditorBrowsableState.Never)]  // JM 06-02-11 No Need to hide this in the Editor
        [Browsable(true)]
        public ObservableCollection<object> SelectedItems
        {
            get
            {
                if (this._selectedItems == null)
                {
                    this._selectedItems = new ObservableCollection<object>();
                    this._backupSelectedItems = new ObservableCollection<object>();
                    this._selectedItems.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SelectedItems_CollectionChanged);
                }

                return this._selectedItems;
            }
        }

        #endregion // SelectedItems

        #region CustomItemsFilter

        /// <summary>
        /// Identifies the <see cref="CustomItemsFilter"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CustomItemsFilterProperty = DependencyProperty.Register("CustomItemsFilter", typeof(ItemsFilter), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(new PropertyChangedCallback(CustomItemsFilterChanged)));

        /// <summary>
        ///  Gets/Sets the <see cref="ItemsFilter"/> that should be applied when filtering is turned on. 
        ///  <para>Note: when not set, this will default to a StartsWith filter.</para>
        /// </summary>
        public ItemsFilter CustomItemsFilter
        {
            get { return (ItemsFilter)this.GetValue(CustomItemsFilterProperty); }
            set { this.SetValue(CustomItemsFilterProperty, value); }
        }

        private static void CustomItemsFilterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;
            combo.ClearFilters();
            if (combo.Editor != null)
            {
                combo.ProcessEditorText(false);
            }
        }

        #endregion // CustomItemsFilter

        #region ItemTemplate

        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(new PropertyChangedCallback(ItemTemplateChanged)));

        /// <summary>
        /// Gets/Sets the DataTemplate that should be applied to each <see cref="ComboEditorItem"/> in the dropdown.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }

        private static void ItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;
            combo.OnPropertyChanged("ItemTemplate");

            combo.InvalidateSelectedContent();
            combo.InvalidateScrollPanel(false, true);
        }

        #endregion // ItemTemplate

        #region ItemsSource

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(new PropertyChangedCallback(ComboEditorBase<T, TControl>.OnItemsSourceChanged)));

        /// <summary>
        /// Gets or sets a collection used to generate the content of each item.
        /// </summary>               
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;

            if (combo._isLoaded)
            {
                combo.ApplyItemSource((IEnumerable)e.NewValue);

                // The collection changed, which means we might have more or less items, and that means we need to recalculate our height
                // in the dropdown. 
                combo._lastSetHeight = double.NaN;
                combo._updateDropDownPosition = true;

				// JM 02-24-12 TFS99858
				combo.InvalidateDropDownPosition(true);

				// JM 11-29-11 TFS96420 - In WPF when the ItemsSource of the control is bound to the DataContext in XAML,
				// and the DataContext is set in the Loaded event of the control, the ItemsSource does not reflect the 
				// settting until after the Loaded event completes.  As a result, if the user sets the SelectedIndex 
				// in the Loaded event it does not take effect.  Work around that by re-selecting the index here.

				if (e.OldValue == null && combo.SelectedIndex >= 0 && combo._applySelectedIndexWhenItemsSourceChanges)
				{
					combo._applySelectedIndexWhenItemsSourceChanges = false;
					combo.SelectItem(combo.SelectedIndex, false);
				}

            }
        }

        #endregion // ItemsSource

        #region DisplayMemberPath

        /// <summary>
        /// Identifies the <see cref="DisplayMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(new PropertyChangedCallback(ComboEditorBase<T, TControl>.OnDisplayMemberPathChanged)));

        /// <summary>
        /// Gets or sets a path to a value on the source object to serve as the visual representation of the object.
        /// </summary>               
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        private static void OnDisplayMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;

            if (combo._isLoaded)
            {
                combo.ClearFilters();
                combo.InvalidateSelectedContent();
                combo.InvalidateScrollPanel(false, true);

                combo.CheckDisplayMemberPath();
            }
        }

        #endregion // DisplayMemberPath

        #region Items

        /// <summary>
        /// Gets the collection used to generate the content of the control.
        /// </summary>
        public BindableItemCollection<T> Items
        {
            get
            {
                if (this._items == null)
                {
                    this._items = new BindableItemCollection<T>(this);
                }

                return this._items;
            }
        }

        #endregion // Items

        #region AllowMultipleSelection

        /// <summary>
        /// Identifies the <see cref="AllowMultipleSelection"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowMultipleSelectionProperty = DependencyProperty.Register("AllowMultipleSelection", typeof(bool), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(false, new PropertyChangedCallback(AllowMultipleSelectionChanged)));

        /// <summary>
        /// Gets/Sets whether the end user should be allowed to select multiple items at the same time.
        /// </summary>
        public bool AllowMultipleSelection
        {
            get { return (bool)this.GetValue(AllowMultipleSelectionProperty); }
            set { this.SetValue(AllowMultipleSelectionProperty, value); }
        }

        private static void AllowMultipleSelectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> editor = (ComboEditorBase<T, TControl>)obj;

            if (editor.SelectedItems.Count > 0)
            {
                // Reset Selection if this property is toggled. 
                editor.SelectedItems.Clear();
            }
            editor.EnsureVisualStates();
        }

        #endregion // AllowMultipleSelection

        #region CheckBoxVisibility

        /// <summary>
        /// Identifies the <see cref="CheckBoxVisibility"/> dependency property. 
        /// </summary>
		public static readonly DependencyProperty CheckBoxVisibilityProperty = DependencyProperty.Register("CheckBoxVisibility", typeof(Visibility), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(CheckBoxVisibilityChanged)));

		private static void CheckBoxVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamMultiColumnComboEditor editor = obj as XamMultiColumnComboEditor;

			if (null != editor)
			{
				if ((Visibility)e.NewValue == Visibility.Visible)
					editor.Columns.InternalRegisterFixedAdornerColumn(editor.Columns.RowSelectionCheckBoxColumn, true);
				else
					editor.Columns.InternalUnregisterFixedAdornerColumn(editor.Columns.RowSelectionCheckBoxColumn);

				editor.EnsureVisualStates();
			}
		}

        /// <summary>
        /// Gets/Sets whether checkboxes should be displayed in the <see cref="ComboEditorItemControl"/>'s dropdown.
        /// </summary>               
        public Visibility CheckBoxVisibility
        {
            get { return (Visibility)this.GetValue(CheckBoxVisibilityProperty); }
            set { this.SetValue(CheckBoxVisibilityProperty, value); }
        }

        #endregion // CheckBoxVisibility

        #region MultiSelectValueDelimiter

        /// <summary>
        /// Identifies the <see cref="MultiSelectValueDelimiter"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MultiSelectValueDelimiterProperty = DependencyProperty.Register("MultiSelectValueDelimiter", typeof(char), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(',', new PropertyChangedCallback(MultiSelectValueDelimiterChanged)));

        /// <summary>
        /// Gets/Sets the char that should be used break up the SelectedItems in the ComboEditorBase
        /// <para>This is only appleid when AllowMultipleSelection is true.</para>
        /// </summary>
        [TypeConverter(typeof(StringToCharTypeConverter))]
        public char MultiSelectValueDelimiter
        {
            get { return (char)this.GetValue(MultiSelectValueDelimiterProperty); }
            set { this.SetValue(MultiSelectValueDelimiterProperty, value); }
        }

        private static void MultiSelectValueDelimiterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;

            if (combo.Editor != null)
            {
                if (combo.SelectedItems.Count > 0)
                {
                    combo.SelectedItems.Clear();
                }
                combo.Editor.Text = "";
            }
        }

        #endregion // MultiSelectValueDelimiter

        #region AutoCompleteDelay

        /// <summary>
        /// Identifies the <see cref="AutoCompleteDelay"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AutoCompleteDelayProperty = DependencyProperty.Register("AutoCompleteDelay", typeof(int), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(0, ComboEditorBase<T, TControl>.OnAutoCompleteDelayChanged));

        /// <summary>
        /// Gets or sets the minimum delay, in milliseconds, after text is typed
        /// in the text box before the
        /// ComboEditorBase control
        /// populates the list of possible matches in the drop-down.
        /// </summary>
        public int AutoCompleteDelay
        {
            get { return (int)this.GetValue(AutoCompleteDelayProperty); }
            set { this.SetValue(AutoCompleteDelayProperty, value); }
        }

        private static void OnAutoCompleteDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> xamWebComboEditor = d as ComboEditorBase<T, TControl>;
            xamWebComboEditor._delayTracker.Interval = TimeSpan.FromMilliseconds(xamWebComboEditor.AutoCompleteDelay);
        }

        #endregion // AutoCompleteDelay

        #region CustomValueEnteredAction

        /// <summary>
        /// Identifies the <see cref="CustomValueEnteredAction"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CustomValueEnteredActionProperty = DependencyProperty.Register("CustomValueEnteredAction", typeof(CustomValueEnteredActions), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(CustomValueEnteredActions.Ignore, new PropertyChangedCallback(CustomValueEnteredActionChanged)));

        /// <summary>
        /// Gets/Sets what action should occur when entering data into the combo editor
        /// that doesn't exist in the underlying ItemsSource.
        /// </summary>
        public CustomValueEnteredActions CustomValueEnteredAction
        {
            get { return (CustomValueEnteredActions)this.GetValue(CustomValueEnteredActionProperty); }
            set { this.SetValue(CustomValueEnteredActionProperty, value); }
        }

        private static void CustomValueEnteredActionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // CustomValueEnteredAction

        #region EmptyText

        /// <summary>
        /// Identifies the <see cref="EmptyText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EmptyTextProperty = DependencyProperty.Register("EmptyText", typeof(string), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(new PropertyChangedCallback(EmptyTextChanged)));

        /// <summary>
        /// Gets/Sets the Text that should be displayed when the editor doesn't have anything selected. 
        /// </summary>
        public string EmptyText
        {
            get { return (string)this.GetValue(EmptyTextProperty); }
            set { this.SetValue(EmptyTextProperty, value); }
        }

        private static void EmptyTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> combo = (ComboEditorBase<T, TControl>)obj;
            combo.OnPropertyChanged("EmptyText");
        }

        #endregion // EmptyText

        #region EditAreaBackground

        /// <summary>
        /// Identifies the <see cref="EditAreaBackground"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EditAreaBackgroundProperty = DependencyProperty.Register("EditAreaBackground", typeof(Brush), typeof(ComboEditorBase<T, TControl>), new PropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(EditAreaBackgroundChanged)));

        /// <summary>
        /// Controls the color that will be applied to the text area when the control is editable.
        /// </summary>
        public Brush EditAreaBackground
        {
            get { return (Brush)this.GetValue(EditAreaBackgroundProperty); }
            set { this.SetValue(EditAreaBackgroundProperty, value); }
        }

        private static void EditAreaBackgroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboEditorBase<T, TControl> ctrl = (ComboEditorBase<T, TControl>)obj;
            ctrl.OnPropertyChanged("EditAreaBackground");
        }

        #endregion // EditAreaBackground 

        #endregion //Public

        #region Internal

        #region DataManager

        /// <summary>
        /// Gets a reference to the <see cref="DataManagerBase"/> of the ComboEditorBase.
        /// </summary>
        protected internal virtual DataManagerBase DataManager
        {
            get
            {
                this.EnsureDataManager();
                return this._dataManager;
            }
        }
        #endregion // DataManager

        #region VerticalScrollBar

        internal ScrollBar VerticalScrollBar
        {
            get;
            set;
        }

        #endregion //VerticalScrollBar

        #region HorizontalScrollBar

        internal ScrollBar HorizontalScrollBar
        {
            get;
            set;
        }

        #endregion //VerticalScrollBar

        #region IsInDesignMode

        internal bool IsInDesignMode
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode(this);
            }
        }

        #endregion //IsInDesignMode

        #region InvalidateDropDownPosition
        internal void InvalidateDropDownPosition(bool adjustHeight)
        {
            if ((this._dropDownPopup != null) && this.IsDropDownOpen)
            {
                double aboveOffsetAdjustment = 0;
                double belowOffsetAdjustment = 0; 

                belowOffsetAdjustment = this.ActualHeight;

                this._dropDownPopup.PlacementTarget = this;
                this._dropDownPopup.Placement		= PlacementMode.Relative;

				// JM 01-03-12 TFS98438 - Wrap in a try/catch since we may not be connected to a PresentationSource if the containing
				// window is in the process of closing.
				Point thisOffset;
				try
				{
					thisOffset = this.PointFromScreen(new Point(0, 0));
				}
				catch (Exception e)
				{
					if (e is InvalidOperationException)
						thisOffset = new Point(0, 0);
					else
						throw (e);
				}

				thisOffset.Y = Math.Abs(thisOffset.Y);
                double rootHeight = System.Windows.SystemParameters.PrimaryScreenHeight;


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                bool calculateHeight = false;

                if (this._rootPopupElement != null)
                {
                    if (double.IsNaN(this._lastSetHeight) || this._rootPopupElement.Height == this._lastSetHeight)
                    {
                        calculateHeight = true;
                    }
                }

                double belowHeight = (rootHeight - (thisOffset.Y + this.ActualHeight));
				belowHeight += 10; // JM 01-06-12 Fudge the belowHeight to give preference to dropping down below.
                double aboveHeight = thisOffset.Y;
                bool isAbove = (aboveHeight > belowHeight);
                double height = 0;
				double heightWhenNoItems = 4;	// JM 01-06-12 TFS96922
                if (calculateHeight)
                {
                    height = (isAbove) ? aboveHeight : belowHeight;

                    height = Math.Min(this.MaxDropDownHeight, height);

                    // Apply a limitation as defined in the MS ComboBox.
                    if (double.IsInfinity(this.MaxDropDownHeight) || double.IsNaN(this.MaxDropDownHeight))
                    {
						// JM 01-06-12
						//height = (height * 3.0) / 5.0;
						height = (height * 4.0) / 5.0;
						height = Math.Round(height);
                    }

					// JM 02-21-12 TFS101025 - Use the 'heightWhenNoItems' height when there is no DataManager
					// or when there are no records
					// JM 01-18-12 TFS99569 - Add null check for DataManager
					// JM 01-06-12 TFS96922
					//if (this.DataManager								!= null &&
					//    this.DataManager.SortedFilteredDataSource		!= null &&
					//    this.DataManager.SortedFilteredDataSource.Count < 1)
					if (this.DataManager == null || this.DataManager.RecordCount	< 1)
						height = heightWhenNoItems;

					// JM 02-24-12 TFS99858 - Change the way we are setting heights.
					//this.DropDownHeight = height;

					//this._rootPopupElement.Height = height;
					//this._lastSetHeight = height;
					bool setHeight = true;

					if (adjustHeight)
					{
						this._panel.Measure(new Size(1, 1));
						this._panel.Measure(new Size(this._panel.DesiredSize.Width, height));
						if (this._panel.DesiredSize.Height < height)
							setHeight = false;
					}

					if (setHeight)
					{
						this.DropDownHeight = height;
						this._rootPopupElement.Height = height;
						this._lastSetHeight = height;
					}
				}
                else
                {
                    double newHeight = this._rootPopupElement.Height;

                    if (this._setDropDownPostion)
                    {
                        if (isAbove && newHeight > aboveHeight)
                            newHeight = aboveHeight;
                        else if (!isAbove && newHeight > belowHeight)
                            newHeight = belowHeight;
                    }

					// JM 02-21-12 TFS101025 - Use the 'heightWhenNoItems' height when there is no DataManager
					// or when there are no records
					// JM 01-18-12 TFS99569 - Add null check for DataManager
					// JM 01-06-12 TFS96922
					//if (this.DataManager								!= null &&
					//    this.DataManager.SortedFilteredDataSource		!= null &&
					//    this.DataManager.SortedFilteredDataSource.Count < 1)
					if (this.DataManager == null || this.DataManager.RecordCount < 1)
						newHeight = heightWhenNoItems;

                    this._rootPopupElement.Height = this.DropDownHeight = height = newHeight;
                }

                if (isAbove)
                {
                    VisualStateManager.GoToState(this, "Above", false);

                    bool shouldSetDropDownPositionFlag = false;

                    // Since we had to restructure the xaml so that the Resizer element doesn't get clipped
                    // the _rootPopupElement isn't really the root element anymore. 
                    // So lets find it, and adjust the dropDownposition based on it's height. 
                    FrameworkElement actualRootElem = this._dropDownPopup.Child as FrameworkElement;
                    if (actualRootElem == this._rootPopupElement)
                    {
                        shouldSetDropDownPositionFlag = true;
                    }
                    else if (actualRootElem != null && actualRootElem.DesiredSize.Height != 0)
                    {
                        if (actualRootElem.DesiredSize.Height >= height || adjustHeight)
                        {
                            height = actualRootElem.DesiredSize.Height;
                            shouldSetDropDownPositionFlag = true;
                        }
                    }
                    
                    if (this._setDropDownPostion)
                    {
						this._dropDownPopup.VerticalOffset = -(height + aboveOffsetAdjustment);

                        if(shouldSetDropDownPositionFlag)
                            this._setDropDownPostion = false;
                    }
                }
                else
                {
                    VisualStateManager.GoToState(this, "Below", false);
                    this._dropDownPopup.VerticalOffset = belowOffsetAdjustment;
                    this._setDropDownPostion = false;
                }
            }
        }
        #endregion // InvalidateDropDownPosition

        #region UpdateDropDownPosition
        internal void UpdateDropDownPosition(double availWidth, double availHeight, double actualHeight)
        {
            if (this._dropDownPopup != null && (this._updateDropDownPosition || this._panel.HorizontalScrollBarJustMadeVisible))
            {
                bool useScrollbarHeight = false; 

                double originalHeight = actualHeight;
                // In the case, where HorizontalScrollBarJustMadeVisible is true, and _updateDropDownPosition is false, the rootPopupElementHeight
                // should already be correct, with th exception of the addition of the scrollbar height, so we can skip this part. 
                if (actualHeight == 0)
                {
                    this._updateDropDownPosition = false;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                      {
                          this._setDropDownPostion = true;
                          this.InvalidateDropDownPosition(true);
                      }));
                    return;
                }
                else if (this._updateDropDownPosition)
                {
                    // Below we're trying to determine the size of the other elements in the rootpoup, besides the panel. 
                    // Basically we know the size of the panel, but we don't know the size of the other elements such as the scrollbar. 
                    // Or ResizeElement.
                    double diff = (this._rootPopupElement.Height - availHeight);

                    // Assume that if the difference is greater than the actualHeight, then we rootPopup's height is stale. 
                    // otherwise use it. 
					if (diff >= 0)	// JM 02-22-12 TFS99858 - change from > to >=
                    {
                        if (this._rootPopupElement.Height != this._rootPopupElement.DesiredSize.Height)
                        {
                            if (this._rootPopupElement == this._dropDownPopup.Child)
                            {
                                diff = (this._rootPopupElement.DesiredSize.Height - availHeight);
                            }
                            useScrollbarHeight = true;                                
                        }

						// JM 02-22-12 TFS99858 - Edit 'diff' before setting the actualHeight.
						if (diff < actualHeight) 
							actualHeight += diff;
                    }
                    else // No need to walk up our parent if our height is zero, as it will be zero.
                    {
                        // If the RootPopup's height is stale, then lets look at our parent's height if it's available
                        FrameworkElement parent = this._panel.Parent as FrameworkElement;
                        if (parent != null)
                        {
                            diff = parent.DesiredSize.Height - availHeight;
                            if (diff < actualHeight)
                                actualHeight += diff;
                        }
                    }
                }

                // We should never wind up being smaller than the size we were when we came in. 
                if (actualHeight < originalHeight)
                {
                    actualHeight = originalHeight;
                }

                // If the scrollbar was just made visible, then it was accounted for in the roopPopupElemen't height
                // So account for it now. 
                if (this._panel != null && this._panel.HorizontalScrollBarJustMadeVisible || (this.HorizontalScrollBar.Visibility == System.Windows.Visibility.Visible && useScrollbarHeight))
                {
                    this.HorizontalScrollBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    actualHeight += this.HorizontalScrollBar.DesiredSize.Height;
                }

                // If the actualHeight is zero, then don't bother doing anything. 
                if (actualHeight > 0)
                {
                    // We've been invalidated, where the height we originally predicted is too big, b/c we don't have enough items to fill the drop down
                    // So we need to re-adjust our sizes. 
                    this._rootPopupElement.Height = actualHeight;

                    this._setDropDownPostion = true;

                    FrameworkElement actualRootElem = this._dropDownPopup.Child as FrameworkElement;
                    if (actualRootElem == null)
                        actualRootElem = this._rootPopupElement;

                    if (actualRootElem == this._rootPopupElement)
                    {
						// JM 02-22-12 TFS99858 - change the element we are hiding to the popup and only hide it in SILVERLIGHT.
						// If we invoke this Measure, while we're already in a measure, the browser will crash. 
                        // So hide the element, so that we doon't see a flicker, and then make it visible and measure it after the original measure call. 



                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {



							this._setDropDownPostion = true;
							this.InvalidateDropDownPosition(true);

							this._rootPopupElement.Measure(new Size(availWidth, actualHeight));
                        }));
                    }
                    else// Use the actual root element if the rootPopup element isn't the real root. That way we get the proper size.
                    {
						// JM 02-22-12 TFS99858 - change the element we are hiding to the popup and only hide it in SILVERLIGHT.
                        // If we invoke this Measure, while we're already in a measure, the browser will crash. 
                        // So hide the element, so that we doon't see a flicker, and then make it visible and measure it after the original measure call. 



						this.Dispatcher.BeginInvoke(new Action(() =>
                        {



							actualRootElem.Measure(new Size(1, 1));
                            actualRootElem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                            this._setDropDownPostion = true;
                            this.InvalidateDropDownPosition(true);
                            this._rootPopupElement.Measure(new Size(availWidth, actualHeight));
                        }));
                    }
                }

            }

            // This update should only occur when we're first dropped down, not on any size change while its open. 
            this._updateDropDownPosition = false;
        }
        #endregion // UpdateDropDownPosition

        #region MultiSelectPanel

        internal Panel MultiSelectPanel
        {
            get
            {
                return this._multiSelectPanel;
            }
        }

        #endregion //MultiSelectPanel

        #region TextEditor

        internal TextBox TextEditor
        {
            get
            {
                return this.Editor;
            }
        }

        #endregion //TextEditor

        #region CachedRows

        internal Dictionary<object, T> CachedRows
        {
            get
            {
                return _cachedRows;
            }
        }

        #endregion //CachedRows

        #region Panel

        internal ItemsPanelBase<T, TControl> Panel
        {
            get
            {
                return _panel;
            }
        }

        #endregion //Panel

		// JM 07-18-11 - XamMultiColumnComboEditor
		#region IsEditableResolved





		internal bool IsEditableResolved
		{
			get { return this._isEditableResolved; }
			set
			{
				this._isEditableResolved = value;

				this.ClearFilters();
				this.EnsureVisualStates();
				this.InvalidateSelectedContent();
				this.IsTabStop = !value;
			}
		}
		#endregion //IsEditableResolved

		// JM 07-18-11 - XamMultiColumnComboEditor
		#region AllowFilteringResolved





		internal bool AllowFilteringResolved
		{
			get { return this._allowFilteringResolved; }
			set { this._allowFilteringResolved = value; }
		}
		#endregion //AllowFilteringResolved

		// JM 07-18-11 - XamMultiColumnComboEditor
		#region OpenDropDownOnTypingResolved





		internal bool OpenDropDownOnTypingResolved
		{
			get { return this._openDropDownOnTypingResolved; }
			set { this._openDropDownOnTypingResolved = value; }
		}
		#endregion //OpenDropDownOnTypingResolved

		// JM 07-18-11 - XamMultiColumnComboEditor
		#region AutoCompleteResolved





		internal bool AutoCompleteResolved
		{
			get;
			set;
		}
		#endregion //AutoCompleteResolved

		// JM 07-18-11 - XamMultiColumnComboEditor
		#region DropDownOpenedViaTyping
		internal bool DropDownOpenedViaTyping
		{
			get;
			set;
		}
		#endregion //DropDownOpenedViaTyping

		// JM 07-18-11 - XamMultiColumnComboEditor
		#region FilterModeResolved





		internal FilterMode FilterModeResolved
		{
			get { return this._filterModeResolved; }
			set
			{
				this._filterModeResolved = value;

				if (this._isLoaded)
				{
					this.ClearFilters();
					this.EnsureVisualStates();
				}
			}
		}
		#endregion //FilterModeResolved

		// JM 07-18-11 - XamMultiColumnComboEditor.
		#region SupportsAlternateFilters







		internal bool SupportsAlternateFilters
		{
			get;
			set;
		}

		#endregion //SupportsAlternateFilters

		// JM 07-18-11 - XamMultiColumnComboEditor.
		#region CurrentSearchText

		internal string CurrentSearchText
		{
			get { return this._currentSearchText; }
			set
			{
				this._currentSearchText = value;

				this.OnCurrentSearchTextChanged(value);
			}
		}

		#endregion //CurrentSearchText

		// JM 08-05-11 - XamMultiColumnComboEditor
		#region AllowDropDownResizingResolved





		internal bool AllowDropDownResizingResolved
		{
			get { return this._allowDropDownResizingResolved; }
			set
			{
				this._allowDropDownResizingResolved = value;
			}
		}
		#endregion //AllowDropDownResizingResolved

		// JM 01-11-12 TFS98440 Added.
		#region DisplayMemberPathResolved





		internal string DisplayMemberPathResolved
		{
			get
			{
				if (string.IsNullOrEmpty(this.DisplayMemberPath))
					return this._firstFieldName;
				else
					return this.DisplayMemberPath;
			}
		}
		#endregion //DisplayMemberPathResolved

		#endregion //Internal

		#region Protected

		#region DataCount

		/// <summary>
        /// Gets the amount of objects in the collection.
        /// </summary>
        protected int DataCount
        {
            get
            {
                DataManagerBase manager = this.DataManager;
                if (manager != null)
                {
                    return manager.RecordCount;
                }

                return 0;
            }
        }

        #endregion //DataCount

		#endregion //Protected

		#region Private

		#region StartsWithItems
		private List<T> StartsWithItems
        {
            get
            {
                if (this._startsWithList == null)
                    this._startsWithList = new List<T>();

                return this._startsWithList;
            }
        }
        #endregion // StartsWithItems

        #region IsEditorTextValidInItemSource
        private bool IsEditorTextValidInItemSource
        {
            get
            {
                if (this.CustomValueEnteredAction == CustomValueEnteredActions.Allow)
                {
                    return this._lastAction == LastInvokeAction.TextBox;
                }

                DataManagerBase manager = this.DataManager;
                if (this.CustomItemsFilter != null && manager != null && manager.Filters != null && manager.Filters.Count > 0)
                {
                    return this.Items.Count > 0;
                }
                else
				if (this.SupportsAlternateFilters)
				{
					return this.Items.Count > 0;
				}
				else
                {
                    return this.StartsWithItems.Count > 0;
                }
            }
        }
        #endregion // IsEditorTextValidInItemSource

        #region Editor
        private TextBox Editor
        {
            get;
            set;
        }
        #endregion // Editor

        #region EqualsDataManager

        private DataManagerBase EqualsDataManager
        {
            get
            {
                return this._equalsDataManager;
            }
        }
        #endregion // DataManager

		#endregion // Private

		#endregion //Properties

		#region Methods

		#region Private

		#region EnsureVisualStates

		private void EnsureVisualStates()
        {
            if (this.IsEditableResolved)
            {
                VisualStateManager.GoToState(this, "Editable", false);

				// JM 05-24-11 Port to WPF.



				// JM 9-15-11 TFS88099
				//object focusElement = FocusManager.GetFocusedElement(this);
				object focusElement = this.Editor != null && this.Editor.IsFocused ? this.Editor : FocusManager.GetFocusedElement(this);

                if (this.Editor != null && this.Editor.Text.Length == 0 && focusElement != this.Editor)
                {
                    VisualStateManager.GoToState(this, "WaterMarkVisible", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "WaterMarkHidden", false);
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "Selectable", false);

                if (this.SelectedItems.Count == 0)
                {
                    VisualStateManager.GoToState(this, "WaterMarkVisible", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "WaterMarkHidden", false);
                }
            }

            if (this.IsDropDownOpen)
            {
                VisualStateManager.GoToState(this, "Open", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Closed", false);
            }

            if (this.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Normal", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }
        }

        #endregion //EnsureVisualStates

        #region ProcessEditorText

		private void ProcessEditorText(bool allowDropDown)
		{
			this.ProcessEditorText(allowDropDown, true);
		}

        private void ProcessEditorText(bool allowDropDown, bool attemptAutoComplete)
        {
			// JM 02-13-12 TFS100397
			if (this._isTextCommitted)
				return;
			// JM 02-16-12 TFS102045 - Selectively do this below.
			//else
			//    this._isTextCommitted = true;

			if (this.IsEditableResolved == false)
				return;

            string text = this.Editor.Text;

            // If the Text is empty, then don't do anything. 
            if (text.Length == 0)
            {
                this.VerticalScrollBar.Value = 0;
                this.ClearFilters();
                this.CommitTextAsSelected(false, true, false, true);

                if (this.SelectedItems.Count > 0)
                {
                    this.SelectedItems.Clear();
                }

				// JM 07-18-11 - XamMultiColumnComboEditor.
				this.CurrentSearchText = text;
			}
            else // Time to parse the current text. 
            {
                // If the item is already selected, and matches the text, then we don't need to do any validation.
                if (this.SelectedItem != null)
                {
                    string currentSelectedItemText = this.ResolveText(this.SelectedItem);
					if (currentSelectedItemText == text)
					{
						// JM 02-16-12 TFS102045
						this._isTextCommitted = true;

						return;
					}
                }

                bool multiSelectCharJustPressed = false;

                // We'll use this array to walk through the text to stop invalid text from being entered. 
                string[] splitText = new string[] { text };

                if (this.AllowMultipleSelection)
                {
                    // When MultiSelect, the strings should be split up. 
                    splitText = text.Split(this.MultiSelectValueDelimiter);

                    // Lets look at the last portion of the text entered. 
                    if (splitText.Length > 0)
                        text = splitText[splitText.Length - 1];

                    // Ok, so the user just pressed the MultiSelectChar, we need to evaluate if the text entered is valid. 
                    // So lets evaluate the next before the char, and remove the char from the textbox. 
                    if (string.IsNullOrEmpty(text) && this.SelectedItems.Count < splitText.Length - 1)
                    {
                        text = splitText[splitText.Length - 2];
                        multiSelectCharJustPressed = true;
                        this.SetEditorText(this.Editor.Text.Remove(this.Editor.Text.Length - 1, 1), true, false);
                    }
                }

                // Now, lets invalidate the selected items. 
                // First store off some information about them, such as the amount of items selected before and the textbox text
                // Then Validate, and reset the textbox's text. So we can continue working.                 
                string preValidateText = this.Editor.Text;
                int prevCount = this.SelectedItems.Count;
                int previousCaretPosition = this.Editor.SelectionStart;
                this.ValidateSelection(this.Editor.Text);
                string postValdiateText = this.Editor.Text;
                this.SetEditorText(preValidateText, true, false);
                this.Editor.SelectionStart = previousCaretPosition;

				// JM 07-18-11 - XamMultiColumnComboEditor.
				this.CurrentSearchText = text;

                // Apply any filter/auto complete that might neccessary
                this.SearchAndFilterItemsByText(text, true);

                if (this.IsEditorTextValidInItemSource)
                {
                    if (this.AutoCompleteResolved && attemptAutoComplete)
                    {
                        if (this.StartsWithItems.Count > 0)
                        {
                            if (this.CustomValueEnteredAction == CustomValueEnteredActions.Ignore || !multiSelectCharJustPressed)
                            {
                                // Lets look at the text and see if we can apply the auto complete.
                                this.NavigateAutoCompleteOptions(true, false, multiSelectCharJustPressed, true);

                                // If we determined above that the multiSelectChart was just pressed, and the text was valid, then lets
                                // put the multi select char back on.
                                if (multiSelectCharJustPressed && this.Editor.SelectionLength == 0)
                                {
                                    this.SetEditorText(this.Editor.Text + this.MultiSelectValueDelimiter, true, true);
                                }
                            }
                            else
                            {
                                if (multiSelectCharJustPressed)
                                {
                                    this.CommitTextAsSelected(false, false, true, false);
                                    this.SetEditorText(this.Editor.Text + this.MultiSelectValueDelimiter, true, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        // So, we know the value that the value in text, is valid, although not complete. 
                        if (this.AllowMultipleSelection && splitText.Length > 1)
                        {
                            string newText = postValdiateText;

                            // If the text, isn't in there, that means, we need to append it back on. 
                            if (!postValdiateText.Contains(text))
                            {
                                // If the text already has items in it, then append to it. 
                                if (postValdiateText.Length > 0)
                                {
                                    newText = postValdiateText + this.MultiSelectValueDelimiter + text;
                                }
                                else// Otherwise, just use the text. 
                                {
                                    newText = text;
                                }
                            }

                            this.SetEditorText(newText, true, true);

                            // If we determined above that the multiSelectChart was just pressed, and the text was valid, then lets
                            // put the multi select char back on.
                            if (multiSelectCharJustPressed)
                            {
                                this.CommitTextAsSelected(false, false, true, true);
                                this.SetEditorText(this.Editor.Text + this.MultiSelectValueDelimiter, true, true);
                            }
                        }
                        else
                        {
							// JM 9-1-11 TFS84368 - Only move the cursor to the end if the CustomValueEnteredAction is Ignore.
                            // When not multi select, we know that just setting it to text, is cool
							this.SetEditorText(text, true, this.CustomValueEnteredAction == CustomValueEnteredActions.Ignore);
                        }
                    }
                }
                else
                {
                    // So, data that was invalid was typed into the combo, lets remove that invalid text.
                    if (this.CustomValueEnteredAction == CustomValueEnteredActions.Ignore)
                    {
                        if (text.Length > 0)
                        {
                            // First, loop through the text, removing one char at a time, 
                            // and performing the StartsWith filter, to see if the text exists in the underlying itemssource.
                            string currenText = text.Remove(text.Length - 1, 1);
                            while (currenText.Length > 0)
                            {
                                this.SearchAndFilterItemsByText(currenText, true);

                                // If we've fixed the text, then update the editor's text. 
                                if (this.IsEditorTextValidInItemSource)
                                {
                                    // Rebuild the text string, with new text. 
                                    string nexText = "";
                                    for (int i = 0; i < splitText.Length - 1; i++)
                                    {
                                        nexText += splitText[i];

                                        if (this.AllowMultipleSelection)
                                            nexText += this.MultiSelectValueDelimiter;
                                    }
                                    nexText += currenText;
                                    this.SetEditorText(nexText, true, true);

                                    // If we're doing AutoComplete, then update the textbox again. so we can have hilighted chars.
                                    if (this.AutoCompleteResolved)
                                    {
                                        if (this.StartsWithItems.Count > 0)
                                        {
                                            this.NavigateAutoCompleteOptions(true, false, multiSelectCharJustPressed, true);
                                        }
                                    }

                                    break;
                                }
                                else // otherwise, move on to the next char.
                                {
                                    currenText = currenText.Remove(currenText.Length - 1, 1);
                                }
                            }

                            // Well, we couldn't resolve the curentText, so lets rebuild the text, without it. 
                            if (currenText.Length == 0)
                            {
                                string nexText = "";
                                for (int i = 0; i < splitText.Length - 1; i++)
                                {
                                    nexText += splitText[i];
                                    if (this.AllowMultipleSelection)
                                        nexText += this.MultiSelectValueDelimiter;
                                }
                                this.SetEditorText(nexText, true, true);

								// JM 07-18-11 - XamMultiColumnComboEditor.
								this.CurrentSearchText = currenText;
								this.ClearFilters();
                            }
                        }
                    }
                    else
                    {
                        if (this.CustomValueEnteredAction == CustomValueEnteredActions.Add && this._focusItem != null)
                        {
                            this.SetFocusedItem(null, false);
                        }

                        if (multiSelectCharJustPressed)
                        {
                            this.CommitTextAsSelected(false, false, true, false);
                            this.SetEditorText(this.Editor.Text + this.MultiSelectValueDelimiter, true, true);
                        }
                    }
                }
            }

            this._previousText = this.Editor.Text.Substring(0, this.Editor.SelectionStart);

            // Opens the DropDown while typing.
            if (this.OpenDropDownOnTypingResolved && allowDropDown)
            {
                if (!this.IsDropDownOpen && !string.IsNullOrEmpty(this.Editor.Text))
                {
					// JM 07-18-11 - XamMultiColumnComboEditor
					this.DropDownOpenedViaTyping = true;

					this.IsDropDownOpen = true;

                    this.Editor.Focus();
                }
            }

			// JM 02-16-12 TFS102045
			this._isTextCommitted = true;
        }

        #endregion // ProcessEditorText

        #region SearchAndFilterItemsByText

        private void SearchAndFilterItemsByText(string text, bool performAutoComplete)
        {
            DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                if (!string.IsNullOrEmpty(this.DisplayMemberPathResolved))	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                {
                    // No text to filter by, so clear any existing filters. 
                    if (string.IsNullOrEmpty(text))
                    {
                        this.ClearFilters();
                        this.StartsWithItems.Clear();
                    }
                    else
                    {
                        ItemsFilter startsWithFilter = null;
						bool managerInitializedWithStartsWithFilters = false;

						// JM 02-17-12 TFS98565 - Call GetDataManagerDataFields which strips out indexer fields.
						//IEnumerable<DataField> fields = manager.GetDataProperties();
                        IEnumerable<DataField> fields = this.GetDataManagerDataFields();

                        DataField field = null;
                        foreach (DataField f in fields)
                        {
							if (f.Name == this.DisplayMemberPathResolved)	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                            {
                                field = f;
                                break;
                            }
                        }

						// JM 06-22-12 TFS115194 No longer need these since we are calling the new 'GetStartsWithCondition' method 
						// to create a CustomComparisonCondition so we can support StartsWith filtering on value types.
						//ComparisonOperator comparisonOperator = ComparisonOperator.StartsWith;
						//ComparisonCondition comparisonCondition = new ComparisonCondition() { Operator = comparisonOperator, FilterValue = text };
						startsWithFilter = new ItemsFilter() { ObjectType = manager.CachedType, FieldName = this.DisplayMemberPathResolved, Field = field };	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
						startsWithFilter.ObjectTypedInfo = manager.CachedTypedInfo;
						startsWithFilter.Conditions.LogicalOperator = LogicalOperator.Or;

						// JM 06-22-12 TFS115194 Create a CustomComparisonCondition so we can support StartsWith filtering on value types.
						//startsWithFilter.Conditions.Add(comparisonCondition);
						startsWithFilter.Conditions.Add(this.GetStartsWithCondition(manager.CachedType, this.DisplayMemberPathResolved, text));

						if (performAutoComplete)
						{
							RecordFilterCollection recordFilterCollection = new RecordFilterCollection();
							recordFilterCollection.LogicalOperator = LogicalOperator.Or;
							recordFilterCollection.Add(startsWithFilter);
							manager.Filters = recordFilterCollection;
							managerInitializedWithStartsWithFilters = true;
							this.StartsWithItems.Clear();
							this.StartsWithItems.AddRange(this.Items);

							if (!this.AllowFilteringResolved && !this.AutoCompleteResolved && this.StartsWithItems.Count > 0)
								this.SetFocusedItem(this.StartsWithItems[0], true);
						}

                        // Filtering is not on, which means we should clear out the filter. 
                        if (!this.AllowFilteringResolved)
                        {
                            this.ClearFilters();
                        }
                        else
                        {
                            // Determine which filter to use (i.e., custom ItemsFilters, slternate ItemsFilters or StartsWith
							if (this.CustomItemsFilter != null)
                            {
								ItemsFilter customItemsFilter		= this.CustomItemsFilter;
								customItemsFilter.ObjectType		= manager.CachedType;
								customItemsFilter.ObjectTypedInfo	= manager.CachedTypedInfo;
								customItemsFilter.FieldName			= this.DisplayMemberPathResolved;	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                                customItemsFilter.Field = field;

								for (int i = 0; i < customItemsFilter.Conditions.Count; i++)
                                {
                                    object filterValue = text;
                                    if (this.DataManager.RecordCount > 0 && !this.ResolveType(this.DataManager.GetRecord(0)).Equals(typeof(String)))
                                    {
                                        filterValue = ComboEditorBase<T, TControl>.ChangeType(text, this.ResolveType(this.DataManager.GetRecord(0)));
                                    }

									((ComparisonCondition)customItemsFilter.Conditions[i]).FilterValue = filterValue;
                                }

								RecordFilterCollection recordFilterCollection = new RecordFilterCollection();
								recordFilterCollection.Add(customItemsFilter);
								manager.Filters = recordFilterCollection;
							}
							else
							if (this.SupportsAlternateFilters)
							{
								manager.Filters = this.GetAlternateFilters(text);
							}
							else  // Using StartsWith filters (only apply the StartsWith filter to the manager if we didn't already do so above)
							if (false == managerInitializedWithStartsWithFilters)
							{
								RecordFilterCollection recordFilterCollection = new RecordFilterCollection();
								recordFilterCollection.Add(startsWithFilter);
								manager.Filters = recordFilterCollection;
							}

                            if (this._focusItem != null)
                            {
                                int index = this.DataManager.ResolveIndexForRecord(this._focusItem.Data);
                                if (index == -1 && this.DataManager.RecordCount > 0)
                                {
                                    this.SetFocusedItem(this.Items[0], true);
                                }
                                else
                                {
                                    this.SetFocusedItem(this.Items[index], true);
                                }
                            }
                            else if (this.DataManager.RecordCount > 0)
                            {
                                this.SetFocusedItem(this.Items[0], true);
                            }
                            else
                            {
                                this.SetFocusedItem(null, true);
                            }

                            if (this.IsDropDownOpen)
                            {
                                this.InvalidateScrollPanel(true);
                            }
                        }
                    }
                }
            }
        }

        #endregion //SearchAndFilterItemsByText

		// JM 06-22-12 TFS115194 Added.
		#region GetStartsWithCondition
		private CustomComparisonCondition GetStartsWithCondition(Type dataType, string fieldName, string filterText)
		{
			System.Linq.Expressions.ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(dataType);

			// JM 06-26-12 TFS115507 - If the data type is DataRowView or DataRow, call a custom static GetValueFromDataRow method which
			// will use the indexer on the DataRow/DataRowView to get the field value instead of looking for a 'fieldName' property
			// directly on the DataRow/DataRowView
			System.Linq.Expressions.Expression prop;

			if (typeof(DataRowView).IsAssignableFrom(dataType) || typeof(DataRow).IsAssignableFrom(dataType))
				prop = System.Linq.Expressions.Expression.Call(typeof(ComboEditorBase<T, TControl>).GetMethod("GetValueFromDataRow", BindingFlags.NonPublic | BindingFlags.Static), System.Linq.Expressions.Expression.Constant(fieldName), parameterExpression);
			else

				prop = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression);

			var toStringMi = prop.Type.GetMethod("ToString", new Type[] { });

			System.Linq.Expressions.Expression						body		= System.Linq.Expressions.Expression.Call(prop, toStringMi);

			// JM 06-26-12 TFS115508 - Make StartsWith case insensitive.
			System.Linq.Expressions.Expression<Func<string, bool>>	startsWith	= (s) => s != null && s.StartsWith(filterText, StringComparison.CurrentCultureIgnoreCase);

			body = System.Linq.Expressions.Expression.Invoke(startsWith, body);

			if (!dataType.IsValueType)
			{
				System.Linq.Expressions.Expression equalExpression = System.Linq.Expressions.Expression.Equal(parameterExpression, System.Linq.Expressions.Expression.Constant(null, dataType));
				body = System.Linq.Expressions.Expression.Condition(equalExpression, System.Linq.Expressions.Expression.Constant(false), body);
			}

			System.Linq.Expressions.Expression resultExpression = System.Linq.Expressions.Expression.Lambda(body, parameterExpression);

			return new CustomComparisonCondition { Expression = resultExpression };
		}
		#endregion //GetStartsWithCondition

		#region ApplyItemSource

		// JM 9-1-11 TFS85162 - Change scope to internal.
        internal void ApplyItemSource(IEnumerable itemSource)
        {
            if (this._dataManager != null)
            {
                this.SelectedItem = null;
                this.UnhookDataManager();
            }
            else
            {
                this.EnsureDataManager();
            }

            if (this.HorizontalScrollBar != null)
            {
                this.HorizontalScrollBar.Value = 0;
            }

            if (this.VerticalScrollBar != null)
            {
                this.VerticalScrollBar.Value = 0;
                this.VerticalScrollBar.Visibility = Visibility.Collapsed;
            }

            if (this.SelectedItem != null)
                this.SelectItem(this.SelectedItem, true);

            this.InvalidateScrollPanel(true);
        }
        #endregion // ApplyItemSource

        #region EnsureDataManager
        /// <summary>
        /// This method checks to ensure that a DataManagerBase is created for a given level and if not creates it for that level.
        /// </summary>
        protected void EnsureDataManager()
        {
            if (this.ItemsSource != null)
            {
                if (this._dataManager == null)
                {
                    this.SetupDataManager();
                }
            }
        }

        #endregion // EnsureDataManager

        #region SetupDataManager

        private void SetupDataManager()
        {
            this._dataManager = DataManagerBase.CreateDataManager(this.ItemsSource);
            if (this._dataManager != null)
            {
                this._equalsDataManager = DataManagerBase.CreateDataManager(this.ItemsSource);
                
                this._equalsDataManager.AllowCollectionViewOverrides = false;
                this._dataManager.AllowCollectionViewOverrides = false;

                this._dataManager.CollectionChanged += new NotifyCollectionChangedEventHandler(DataManager_CollectionChanged);
                this._dataManager.NewObjectGeneration += new EventHandler<HandleableObjectGenerationEventArgs>(DataManager_NewObjectGeneration);
                this._dataManager.DataUpdated += new EventHandler<EventArgs>(DataManager_DataUpdated);


				// JM 01-11-12 TFS98440 Establish the field that we will use as the DisplayMemberath if
				// DisplayMemberPath is not specified.  We prefer a string field so that sorting/filtering will work.
				// JM 02-17-12 TFS98565 - Call GetDataManagerDataFields which strips out indexer fields.
				//IEnumerable<DataField> fields	= this._dataManager.GetDataProperties();
				IEnumerable<DataField> fields = this.GetDataManagerDataFields();

				string firstFieldName = string.Empty;
				string firstStringFieldName		= string.Empty;
				foreach (DataField field in fields)
				{
					if (string.IsNullOrEmpty(firstFieldName))
						firstFieldName = field.Name;

					if (field.FieldType == typeof(string) && string.IsNullOrEmpty(firstStringFieldName))
						firstStringFieldName = field.Name;
				}

				if (string.IsNullOrEmpty(firstStringFieldName))
					this._firstFieldName = firstFieldName;
				else
					this._firstFieldName = firstStringFieldName;


                this.InitializeData();
            }
            else
            {
                // If a data source was assigned
                // but there was no data in the collection
                // but the collection implements INotifyCollectionChanged
                // then we can figure out when data is added and create a datamanager at that point. 
                INotifyCollectionChanged incc = this.ItemsSource as INotifyCollectionChanged;
                if (incc != null)
                {
                    incc.CollectionChanged -= ItemsSource_CollectionChanged;
                    incc.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
                }

				// JM 01-11-12 TFS98440.
				this._firstFieldName = null;
			}
        }

        #endregion // SetupDataManager

        #region UnhookDatamanager

		// JM 9-1-11 TFS85162 - Change scope to internal.
		internal void UnhookDataManager()
        {
            this.InvalidateItems();
            this._cachedRows.Clear();

            if (this._dataManager != null)
            {
                this._dataManager.CollectionChanged -= DataManager_CollectionChanged;
                this._dataManager.NewObjectGeneration -= DataManager_NewObjectGeneration;
                this._dataManager.DataUpdated -= DataManager_DataUpdated;
                this._dataManager.Filters = null;
                this._dataManager = null;

				// JM 02-17-12 TFS98565 Added.
				this._dataFieldsCache = null;
            }
        }

        #endregion // UnhookDataManager

        #region InvalidateScrollPanel

        /// <summary>
        /// Forces the underlying dropwdown panel to invalidate.
        /// </summary>
        /// <param name="resetScrollPosition"></param>
        protected internal void InvalidateScrollPanel(bool resetScrollPosition)
        {
            this.InvalidateScrollPanel(resetScrollPosition, false);
        }

		// JM 9-27-11 TFS88306 - Make this virtual.
        internal virtual void InvalidateScrollPanel(bool resetScrollPosition, bool resetItems)
        {
            if (this._panel != null)
            {
                if (resetItems)
                {
                    this._panel.ResetItems();
                    this._panel.InvalidateMeasure();
                }

                if (this.IsDropDownOpen)
                {
                    this._panel.InvalidateMeasure();

                    if (resetScrollPosition)
                    {
                        if (this.HorizontalScrollBar != null)
                        {
                            this.HorizontalScrollBar.Value = 0;
                        }
                        if (this.VerticalScrollBar != null)
                        {
                            this.VerticalScrollBar.Value = 0;
                        }
                    }
                }
            }
        }

        #endregion // InvalidateScrollPanel

        #region ResolveText

        private string ResolveText(object dataItem)
        {
            string value = "";

			if (!string.IsNullOrEmpty(this.DisplayMemberPathResolved))	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
            {
				Binding b = new Binding(this.DisplayMemberPathResolved);	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                b.Mode = BindingMode.OneWay;
                b.Source = dataItem;
                this._valueProxy.SetBinding(ComboEditorItemValueProxy.ValueProperty, b);

                if (this._valueProxy.Value != null)
                    value = this._valueProxy.Value.ToString();
            }

            return value;
        }

        #endregion // ResolveText

        #region ResolveType

        private Type ResolveType(object dataItem)
        {
            Type type = typeof(string);

			if (!string.IsNullOrEmpty(this.DisplayMemberPathResolved))	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
            {
				Binding b = new Binding(this.DisplayMemberPathResolved);	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                b.Mode = BindingMode.OneWay;
                b.Source = dataItem;
                this._valueProxy.SetBinding(ComboEditorItemValueProxy.ValueProperty, b);

                if (this._valueProxy.Value != null)
                {
                    type = this._valueProxy.Value.GetType();
                }
            }

            return type;
        }

        #endregion //ResolveType

        #region SetItemText

        private void SetItemText(object dataItem, string text)
        {
			if (!string.IsNullOrEmpty(this.DisplayMemberPathResolved))	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
            {
				Binding b = new Binding(this.DisplayMemberPathResolved);	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                b.Mode = BindingMode.TwoWay;
                b.Source = dataItem;
                this._valueProxy.SetBinding(ComboEditorItemValueProxy.ValueProperty, b);

                this._valueProxy.Value = text;
            }
        }

        #endregion // SetItemText

        #region ClearFilters
        private void ClearFilters()
        {
            DataManagerBase manager = this.DataManager;
            if (manager != null && manager.Filters != null && manager.Filters.Count > 0)
            {
                manager.Filters.Clear();

                if (this.IsDropDownOpen)
                {
                    this._panel.InvalidateMeasure();
                }
            }
        }
        #endregion // ClearFilters

        #region CommitTextAsSelected

        private void CommitTextAsSelected(bool closeDropDown, bool cancelSelection, bool moveSelectionToTheEnd, bool allowUseOfFocusedItem)
        {
			// JM 02-16-12 TFS102045 - query this flag selectively below.
			//// JM 02-13-12 TFS100051
			//if (this._isTextCommitted)
			//    return;

			if (cancelSelection)
            {
                this.SetEditorText(this.Editor.Text.Remove(this.Editor.SelectionStart, this.Editor.SelectionLength), true, false);
            }
			// JM 02-16-12 TFS102045 - also check _isTextCommitted
            else if (IsEditorTextValidInItemSource && allowUseOfFocusedItem && this.CustomValueEnteredAction != CustomValueEnteredActions.Allow && this._isTextCommitted == false)
            {
                T item = this._focusItem;
                if (item != null)
                {
                    if (item.IsSelected)
                    {
                        this._supressSelectionChangedEvent = true;
                        this.UnselectItem(item);
                        this.SelectItem(item, false);
                        this._supressSelectionChangedEvent = false;
                    }
                    else
                    {
                        this.SelectItem(item, false);
                    }
                }
            }
            else
            {
                if (this.CustomValueEnteredAction != CustomValueEnteredActions.Ignore)
                {
                    string itemText = "";

                    if (this.AllowMultipleSelection)
                    {
                        string[] splitText = this.Editor.Text.Split(this.MultiSelectValueDelimiter);
                        itemText = splitText[splitText.Length - 1];
                    }
                    else
                    {
                        itemText = this.Editor.Text;
                    }

                    if (!string.IsNullOrEmpty(itemText))
                    {
						// JM 02-16-12 TFS102045 - also check _isTextCommitted
						if (!this.AddOrAllowItem(itemText) && this._isTextCommitted == false)
                        {
							// JM 11-29-11 TFS96693 - Add checkfor record count > 0;
							if (this.EqualsDataManager != null && this.EqualsDataManager.RecordCount > 0)
                                this.SelectItem(this.EqualsDataManager.GetRecord(0), false);
                        }
                    }
                }
            }

            if (closeDropDown)
                this.IsDropDownOpen = false;

            this.StartsWithItems.Clear();

            if (moveSelectionToTheEnd)
                this.Editor.SelectionStart = this.Editor.Text.Length;

            this.ClearFilters();

			// JM 02-13-12 TFS100051
			this._isTextCommitted = true;
		}

        #endregion // CommitTextAsSelected

        #region NavigateAutoCompleteOptions
        private void NavigateAutoCompleteOptions(bool increment, bool usePrevText, bool lookToCommit, bool ignoreFocusedItem)
        {
            if (this.IsEditorTextValidInItemSource)
            {
                DataManagerBase manager = this.DataManager;

                if (manager != null && manager.RecordCount > 0)
                {
                    int index = -1;

                    if (!ignoreFocusedItem && this._focusItem != null)
                        index = this.StartsWithItems.IndexOf(this._focusItem);

                    // Determine the index of the item that should be navigated.
                    if (increment)
                        index++;
                    else
                        index--;

                    // If we reach the max or min limit, then loop around
                    if (index >= this.StartsWithItems.Count)
                        index = 0;
                    else if (index < 0)
                        index = this.StartsWithItems.Count - 1;

                    T item = null;
                    string text = "";

                    string initalText = (usePrevText) ? this._previousText : this.Editor.Text;
                    string[] splitText = initalText.Split(this.MultiSelectValueDelimiter);

                    // So the string we're looking at, might be an item's text
                    // And it wants to try and comit it. 
                    // If thats the case, then we need to look through all possible values to see if one of them matches.
                    if (lookToCommit)
                    {
                        string currentVal = (splitText[splitText.Length - 1]).ToLower();
                        foreach (T cei in this.StartsWithItems)
                        {
                            string val = this.ResolveText(cei.Data);
                            if (val.ToLower() == currentVal)
                            {
                                text = val;
                                item = cei;
                                break;
                            }
                        }
                    }

                    if (item == null && this.StartsWithItems.Count > index && index != -1)
                    {
                        item = this.StartsWithItems[index];
                        text = this.ResolveText(item.Data);
                    }

                    this.SetFocusedItem(item, true);

                    if (!string.IsNullOrEmpty(text) && item != null)
                    {
                        // Lets rebuild the text for the textbox, including the suggested text. 
                        splitText[splitText.Length - 1] = text;

                        string newText = "";

                        foreach (string str in splitText)
                            newText += str + this.MultiSelectValueDelimiter;

                        newText = newText.Remove(newText.Length - 1, 1);

                        this.SetEditorText(newText, true, false);

                        // hilight the suggested part of the text. 
                        this.Editor.SelectionStart = initalText.Length;

						// JM 06-20-12 TFS113146
                        //this.Editor.SelectionLength = this.Editor.Text.Length - initalText.Length;
						this.Editor.SelectionLength = Math.Max(0, this.Editor.Text.Length - initalText.Length);

                        // if there is no text to hilight, then it means we must have a valid item. 
                        // So lets select it, and move the selection to the end. 
                        if (lookToCommit && this.Editor.SelectionLength == 0)
                        {
                            this.SelectItem(item, false);
                            this.Editor.SelectionStart = this.Editor.Text.Length;
                        }
                    }
                }
            }
        }
        #endregion // NavigateAutoCompleteOptions

        #region InvalidateItems

        private void InvalidateItems()
        {
            this.Items.Clear();
            this._duplicateObjectValidator.Clear();

            this.InvalidateScrollPanel(false);           
        }

        #endregion // InvalidateItems

        #region SetEditorText

        private void SetEditorText(string text, bool silently, bool moveSelectionToEnd)
        {
              if (this.Editor != null)
              {
                  if (silently)
                      this.Editor.TextChanged -= this.Editor_TextChanged;

                  var stb = this.Editor as SpecializedTextBox;

                  // JM 12-1-11 TFS97000
                  // There is a case where we don't care if the TB says not to allow changes.
                  if (stb == null || stb.AllowTextChanges)
                  {
                      this.Editor.Text = text;
                  }

                  if (silently)
                      this.Editor.TextChanged += this.Editor_TextChanged;

                  if (moveSelectionToEnd)
                      this.Editor.SelectionStart = this.Editor.Text.Length;
              }
        }
        #endregion // SetEditorText

        #region ValidateSelection
        private void ValidateSelection(string text)
        {
            string[] splitText = text.Split(this.MultiSelectValueDelimiter);

            List<string> itemStrings = new List<string>(splitText);
            List<object> itemsToRemove = new List<object>();
            List<string> selectedItemStrings = new List<string>();

            foreach (object dataItem in this.SelectedItems)
            {
                string itemText = this.ResolveText(dataItem);
                if (!itemStrings.Contains(itemText))
                {
                    itemsToRemove.Add(dataItem);
                }
                else
                {
                    selectedItemStrings.Add(itemText);
                }
            }

            if (itemsToRemove.Count > 0)
            {
                foreach (object dataItem in itemsToRemove)
                {
                    this.SelectedItems.Remove(dataItem);
                }
            }

            splitText = this.Editor.Text.Split(this.MultiSelectValueDelimiter);
            itemStrings = new List<string>(splitText);

            string newText = "";
            int count = itemStrings.Count;
            for (int i = 0; i < count - 1; i++)
            {
                string itemText = itemStrings[i];
                if (!selectedItemStrings.Contains(itemText))
                {
                    if (this.AddOrAllowItem(itemText))
                    {
                        newText += itemText + this.MultiSelectValueDelimiter;
                    }
                    else
                    {
						// JM 03-02-12 TFS103659 - Check for null EqualsDataManager
						// JM 11-9-11 TFS95726, TFS95725 - Add a check for record count before accessing record 0.
						if (this.EqualsDataManager != null && this.EqualsDataManager.RecordCount > 0)
							this.SelectItem(this.EqualsDataManager.GetRecord(0), false);
                        newText += itemText + this.MultiSelectValueDelimiter;
                    }
                }
                else
                    newText += itemText + this.MultiSelectValueDelimiter;

            }

            newText += itemStrings[itemStrings.Count - 1];
            this.SetEditorText(newText, true, false);
        }

        #endregion // ValidateSelection

        #region SetFocusedItem

		// JM 08-01-12 TFS117221 Add an overload that includes a parameter which specifies whether
		// or not to scroll
		private void SetFocusedItem(T item, bool scrollToTop)
		{
			this.SetFocusedItem(item, scrollToTop, true);
		}

		private void SetFocusedItem(T item, bool scrollToTop, bool scroll)
        {
            if (this._focusItem != null)
                this._focusItem.IsFocused = false;

            if (item != null)
                item.IsFocused = true;

            this._focusItem = item;

            if (scroll && this.IsDropDownOpen)
                this.ScrollItemIntoView(this._focusItem, scrollToTop);
        }
        #endregion // SetFocusedItem

        #region InvalidateSelectedContent
        private void InvalidateSelectedContent()
        {
            if (this.IsEditableResolved)
            {
                string text = "";
                foreach (object data in this.SelectedItems)
                {
                    text += this.ResolveText(data) + this.MultiSelectValueDelimiter;
                }

                if (text.Length > 0)
                    text = text.Remove(text.Length - 1, 1);

                if (this.Editor != null)
                {
					// JM 9-1-11 TFS84368 - Only move the cursor to the end if we still have a selected item.
                    this.SetEditorText(text, true, this.SelectedItems.Count > 0);
                }
            }
            else
            {

                if (this._multiSelectPanel != null)
                {
                    this._multiSelectPanel.Children.Clear();

                    foreach (object data in this.SelectedItems)
                    {

                        if (this.ItemTemplate != null)
                        {
                            ContentControl ctrl = new ContentControl();
                            ctrl.IsHitTestVisible = false;
                            ctrl.IsTabStop = false;
                            ctrl.Content = data;
                            ctrl.ContentTemplate = this.ItemTemplate;
                            this._multiSelectPanel.Children.Add(ctrl);
                        }
                        else
                        {
							string path = this.DisplayMemberPathResolved;	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED

                            if (path == null)
                                path = "";

                            Binding b = new Binding(path);
                            b.Mode = BindingMode.OneWay;
                            b.Source = data;
                            TextBlock tb = new TextBlock();
                            tb.IsHitTestVisible = false;
                            tb.SetBinding(TextBlock.TextProperty, b);
                            this._multiSelectPanel.Children.Add(tb);
                        }


                        this._multiSelectPanel.Children.Add(new TextBlock() { Text = this.MultiSelectValueDelimiter.ToString() });
                    }

                    if (this._multiSelectPanel.Children.Count > 0)
                    {
                        this._multiSelectPanel.Children.RemoveAt(this._multiSelectPanel.Children.Count - 1);
                    }
                }

            }
        }
        #endregion // InvalidateSelectedContent

        #region CheckDisplayMemberPath

        private void CheckDisplayMemberPath()
        {
			if (!string.IsNullOrEmpty(this.DisplayMemberPathResolved) && this.DataManager != null)	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
            {
                bool isDisplayMemberPathValid = false;

                try
                {
					// JM 01-18-12 TFS99653 - Use DataManagerBase.ResolvePropertyTypeFromPropertyName instead of 
					// DataManagerBase.BuildPropertyExpressionFromPropertyName
					//System.Linq.Expressions.Expression expression =
					//    DataManagerBase.BuildPropertyExpressionFromPropertyName(
					//        this.DisplayMemberPathResolved, /* JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED */
					//        System.Linq.Expressions.Expression.Parameter(this.DataManager.CachedType, "param"));

					//if (expression != null)
					//{
					//    isDisplayMemberPathValid = true;
					//}
					Type t = DataManagerBase.ResolvePropertyTypeFromPropertyName(this.DisplayMemberPathResolved, this.DataManager.CachedTypedInfo);
					if (t != null)
						isDisplayMemberPathValid = true;
                }
                catch
                {
                    // probably an illegal type, or the indexer is of type object and they want to access a property off of it. 
                }

                if (!isDisplayMemberPathValid)
                {
                    throw new ArgumentException(
                        string.Format(
                            SRCombo.GetString("InvalidDisplayMember"),
							this.DisplayMemberPathResolved,	 
                            this.DataManager.CachedType.Name));
                }
            }
        }

        #endregion //CheckDisplayMemberPath

        #region AddOrAllowItem

        private bool AddOrAllowItem(string itemText)
        {
            if (this.EqualsDataManager != null)
            {
                ComparisonOperator comparisonOperator = ComparisonOperator.Equals;

				// JM 02-17-12 TFS98565 - Call GetDataManagerDataFields which strips out indexer fields.
				//IEnumerable<DataField> fields = this.EqualsDataManager.GetDataProperties();
				IEnumerable<DataField> fields = this.GetDataManagerDataFields();
                DataField field = null;
                foreach (DataField f in fields)
                {
					if (f.Name == this.DisplayMemberPathResolved)	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                    {
                        field = f;
                        break;
                    }
                }

				ItemsFilter filter = new ItemsFilter() { ObjectType = this.EqualsDataManager.CachedType, FieldName = this.DisplayMemberPathResolved, Field = field };	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                filter.ObjectTypedInfo = this.EqualsDataManager.CachedTypedInfo;
                object value = null;

                try
                {
                    value = System.Convert.ChangeType(itemText, filter.FieldType, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    value = null;
                }

                if (value != null)
                {
					// JM 8-7-12 TFS118008 - Use a case sensitive comparison condition.
					//ComparisonCondition comparisonCondition = new ComparisonCondition() { Operator = comparisonOperator, FilterValue = value };
					ComparisonCondition comparisonCondition = new ComparisonCondition() { Operator = comparisonOperator, FilterValue = value, CaseSensitive = true };

                    filter.Conditions.Add(comparisonCondition);

                    RecordFilterCollection recordFilterCollection = new RecordFilterCollection();
                    recordFilterCollection.Add(filter);
                    this.EqualsDataManager.Filters = recordFilterCollection;

                    if (this.EqualsDataManager.RecordCount == 0)
                    {
                        // If the CustomValueEnteredAction is Ignore, do nothing, as the text should be removed. 
                        if (this.CustomValueEnteredAction == CustomValueEnteredActions.Add)
                        {
                            T item = this.CreateItem();
                            if (item != null)
                            {
                                this.SetItemText(item.Data, itemText);
                                if (!this.OnItemAdding(item))
                                {
                                    this.AddItem(item);
                                    this.SelectItem(item.Data, false);
                                    this.OnItemAdded(item);

                                    return true;
                                }
                            }

                        }
                        else if (this.CustomValueEnteredAction == CustomValueEnteredActions.Allow)
                        {
                            T item = this.CreateItem();
                            if (item != null)
                            {
                                this.SetItemText(item.Data, itemText);
                                this.SelectItem(item, false);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion // AddOrAllowItem

		// JM 05-26-11 TFS76765 Port to WPF.  Added.

		#region OnComboLostFocus

		private void OnComboLostFocus()
		{
			if (false == this.IsKeyboardFocusWithin && this.IsDropDownOpen)
				this.IsDropDownOpen = false;
		}

		#endregion //OnComboLostFocus

		#region OnWindowSizeChanged

		void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.OnWindowStateChanged(sender, EventArgs.Empty);
		}

		#endregion //OnWindowSizeChanged

		// JM 05-26-11 TFS76765 Port to WPF.  Added.
		#region OnWindowStateChanged

		void OnWindowStateChanged(object sender, EventArgs e)
		{
			// JM 10-14-11 TFS91993 - Uncomment this code which was mistakenly commented out
			// as part of the testing for the fix for TFS87910
			if (this.IsDropDownOpen)
				this.IsDropDownOpen = false;
		}

		#endregion //OnWindowStateChanged

		// JM 05-26-11 TFS76765 Port to WPF.  Added.
		#region EnableCloseUpTriggerTrackers

		private void EnableCloseUpTriggerTrackers()
		{
			if (null == this._comboLostFocusTracker)
				this._comboLostFocusTracker = new LostFocusTracker(this, this.OnComboLostFocus);

			if (false == this._windowEventsHooked)
			{
				// JM 10-5-11 TFS90426 - Use _window member variable instead of stack variable
				this._window = Window.GetWindow(this);
				if (null != this._window)
				{
					this._window.LocationChanged	+= new EventHandler(OnWindowStateChanged);
					this._window.Deactivated		+= new EventHandler(OnWindowStateChanged);
					this._window.SizeChanged		+= new SizeChangedEventHandler(OnWindowSizeChanged);

					this._window.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVisual_MouseLeftButtonDown), true);
					this._windowEventsHooked		= true;
				}
			}
		}

		#endregion //EnableCloseUpTriggerTrackers

		// JM 05-26-11 TFS76765 Port to WPF. Added
		#region DisableCloseUpTriggerTrackers

		private void DisableCloseUpTriggerTrackers()
		{
			if (null != this._comboLostFocusTracker)
			{
				this._comboLostFocusTracker.Deactivate(true);
				this._comboLostFocusTracker = null;
			}

			// JM 10-5-11 TFS90426 - Use _window member variable instead of stack variable
			if (true == this._windowEventsHooked && this._window != null)
			{
				if (null != this._window)
				{
					this._window.LocationChanged	-= new EventHandler(OnWindowStateChanged);
					this._window.Deactivated		-= new EventHandler(OnWindowStateChanged);
					this._window.SizeChanged		-= new SizeChangedEventHandler(OnWindowSizeChanged);

					this._window.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVisual_MouseLeftButtonDown));
					this._windowEventsHooked = false;
				}
			}
		}

		#endregion //DisableCloseUpTriggerTrackers


		// JM 09-15-11 TFS88083 Added.


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		// JM 10-27-11 TFS94258
		#region GetUltimateVisualParent
		private FrameworkElement GetUltimateVisualParent()
		{
			DependencyObject descendant = this;
			DependencyObject parent		= null;
			while (descendant != null)
			{
				descendant = VisualTreeHelper.GetParent(descendant) as FrameworkElement;
				if (descendant != null)
					parent = descendant;	
			}

			return parent as FrameworkElement;
		}
		#endregion //GetUltimateVisualParent

		// JM 02-21-12 TFS102346 Added.
		#region SetFocusToControlOrEditor
		private void SetFocusToControlOrEditor()
		{
			if (this.Editor == null || this.IsEditableResolved == false)
				this.Focus();
			else
				this.Editor.Focus();
		}
		#endregion //SetFocusToControlOrEditor

		#endregion //Private

		#region Protected

		#region GenerateNewObject

		/// <summary>
        /// Override this on a base class to create a new instance of the object that should wrap the data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract T GenerateNewObject(object data);

        #endregion // GenerateNewObject

        #region CreateItem

        /// <summary>
        /// Creates a new ComboEditorItem object 
        /// </summary>
        /// <param propertyName="data"></param>
        /// <param propertyName="dataManager"></param>
        /// <returns></returns>
        protected internal virtual T CreateItem(object data, DataManagerBase dataManager)
        {
            T item = default(T);
            if (dataManager != null)
            {
                if (!_dataManager.CachedType.IsAssignableFrom(data.GetType()))
                {
                    throw new Exception(string.Format(CultureInfo.InvariantCulture, SRCombo.GetString("InvalidDataTypeException"), data.GetType(), dataManager.CachedType));
                }
                item = this.GenerateNewObject(data);
            }
            else
                throw new Exception(SRCombo.GetString("AddWithOutItemSourceException"));

            return item;
        }

        /// <summary>
        /// Creates a new object with a default underlying data object.
        /// </summary>
        /// <returns>The item that was created.</returns>
        protected virtual T CreateItem()
        {
            T item = default(T);
            DataManagerBase dm = this.DataManager;
            if (dm != null)
                item = this.CreateItem(dm.GenerateNewObject(), dm);

            return item;
        }

        /// <summary>
        /// Creates a new object using the inputted data object.
        /// </summary>
        /// <param name="dataItem">The business object.</param>
        /// <returns>The item that was created.</returns>
        protected virtual T CreateItem(object dataItem)
        {
            return this.CreateItem(dataItem, this.DataManager);
        }

        #endregion //CreateItem

        #region GetDataItem

        /// <summary>
        /// Resolves the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item that is requested.</returns>
        protected virtual T GetDataItem(int index)
        {
            DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                T item = default(T);
                object data = manager.GetRecord(index);

                if (data != null)
                {
                    if (this._cachedRows.ContainsKey(data))
                    {
                        



                        if (!this._duplicateObjectValidator.ContainsKey(data))
                        {
                            this._duplicateObjectValidator.Add(data, index);
                            return this._cachedRows[data];
                        }
                        else
                        {
                            return this.GenerateNewObject(data);
                        }
                    }

                    item = this.GenerateNewObject(data);
                    this._cachedRows.Add(data, item);
                    this._duplicateObjectValidator.Add(data, index);
                }
                else
                {
                    item = this.GenerateNewObject(data);
                }

                return item;
            }

            return default(T);
        }

        #endregion //GetDataItem

        #region AddItem

        /// <summary>
        /// Adds a new object to the collection
        /// </summary>
        /// <param name="addedObject">An item that will be added.</param>
        protected virtual void AddItem(T addedObject)
        {
            DataManagerBase dm = this.DataManager;
            if (dm != null)
            {
                dm.AddRecord(addedObject.Data);

                if (!this._cachedRows.ContainsKey(addedObject.Data))
                    this._cachedRows.Add(addedObject.Data, addedObject);
            }

        }

        #endregion //AddItem

        #region RemoveItem
        /// <summary>
        /// Removes a ComboEditorItem from the underlying ItemSource
        /// </summary>
        /// <param propertyName="removedObject"></param>
        /// <returns>true if the ComboEditorItem is removed.</returns>
        protected bool RemoveItem(T removedObject)
        {
            return this.RemoveItem(removedObject, this.DataManager);
        }

        /// <summary>
        /// Removes a ComboEditorItem from the underlying ItemSource
        /// </summary>
        /// <param name="removedObject">The ComboEditorItem to remove</param>
        /// <param name="manager">The Manager that should be performing the removal.</param>
        /// <returns></returns>
        protected virtual bool RemoveItem(T removedObject, DataManagerBase manager)
        {
            if (removedObject != null && manager != null)
            {
                manager.RemoveRecord(removedObject.Data);
                this._cachedRows.Remove(removedObject.Data);

                return true;
            }
            return false;
        }

        #endregion // RemoveItem

        #region RemoveRange

        /// <summary>
        /// Removes the specified range of items from the collection.
        /// </summary>
        /// <param name="itemsToRemove">The range that will be removed.</param>
        protected virtual void RemoveRange(IList<T> itemsToRemove)
        {
            DataManagerBase manager = this.DataManager;

            if (itemsToRemove == null || itemsToRemove.Count == 0 || manager == null)
                return;

            foreach (T item in itemsToRemove)
                this.RemoveItem(item, manager);
        }

        #endregion //RemoveRange

        #region InsertItem

        /// <summary>
        /// Adds an item to the collection at a given index.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <param name="insertedObject">An item that will be inserted.</param>
        protected virtual void InsertItem(int index, T insertedObject)
        {
            DataManagerBase dm = this.DataManager;

            if (dm != null)
            {
                dm.InsertRecord(index, insertedObject.Data);

                if (!this._cachedRows.ContainsKey(insertedObject.Data))
                    this._cachedRows.Add(insertedObject.Data, insertedObject);

            }
        }

        #endregion //InsertItem

        #region ChangeType

        /// <summary>
        /// Changes the value to given data type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        protected internal static object ChangeType(object value, Type dataType)
        {
            if (dataType.IsGenericType && dataType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;

                dataType = Nullable.GetUnderlyingType(dataType);
            }

            if (dataType.IsEnum && value != null)
            {
                return Enum.Parse(dataType, value.ToString(), true);
            }

            return Convert.ChangeType(value, dataType, CultureInfo.CurrentCulture);
        }

        #endregion //ChangeType

        #region InitializeData

        /// <summary>
        /// A DataManager was just created, so this is our chance to look at the data and do any initializing.
        /// </summary>
        protected virtual void InitializeData()
        {
        }

        #endregion // InitializeData

		// JM 07-18-11 - XamMultiColumnComboEditor.
		#region OnEditorKeyDown

		/// <summary>
		/// Called when a key is pressed while the ComboEditorBase's textbox has focus.
		/// </summary>
		/// <param name="e">Event args that describe the key that was pressed.</param>
		protected virtual void OnEditorKeyDown(KeyEventArgs e)
		{
		}

		#endregion //OnEditorKeyDOwn

		// JM 02-17-12 TFS98565 Added.
		#region GetDataManagerDataFields
		/// <summary>
		/// Returns an IEnumerable<DataField> from the DataManager stripping out fields that represent indexer properties.</DataField>
		/// </summary>
		/// <returns></returns>
		protected internal IEnumerable<DataField> GetDataManagerDataFields()
		{
			// If we don't have a DataManager, return an empty list.
			System.Diagnostics.Debug.Assert(this._dataManager != null, "DataManager is null in GetDataManagerDataFields!!");
			if (this._dataManager == null)
				return new List<DataField>();

			// If we have already created the list of fields just return it.
			if (this._dataFieldsCache != null)
				return this._dataFieldsCache;

			// Create a new list of DataFields.
			IEnumerable<DataField> fields = this._dataManager.GetDataProperties();

			// JM 02-21-12 TFS102268 - Just return the fields 'as-is' for DataRowView since there are no indexer fields.

			if (this._dataManager.CachedTypedInfo.CachedType == typeof(System.Data.DataRowView))



				return fields;

			// Before returning remove all fields that represent indexer properties.
			PropertyInfo[]	propinfo	= this._dataManager.CachedTypedInfo.CachedType.GetProperties();
			List<DataField> temp		= new List<DataField>();

			foreach (DataField field in fields)
			{
				foreach (PropertyInfo info in propinfo)
				{
					// If the property does not have any index parameters then it is not an indexer property.
					if (info.Name == field.Name && info.GetIndexParameters().Length == 0)
					{
						temp.Add(field);
						break;
					}
				}
			}

			// Cache the list
			this._dataFieldsCache = temp;

			return temp;
		}
		#endregion //GetDataManagerDataFields

		#endregion //Protected

		#region Internal

		#region SelectItem

		internal void SelectItem(int index, bool clearRestOfSelection)
        {
            DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                if (index == -1)
                {
                    this.SelectItem(null, index, clearRestOfSelection);
                }
                else
                {
                    this.SelectItem(manager.GetRecord(index), index, clearRestOfSelection);
                }
            }
        }

        internal void SelectItem(object data, bool clearRestOfSelection)
        {
            DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                this.SelectItem(data, manager.ResolveIndexForRecord(data), clearRestOfSelection);
            }
        }

        internal void SelectItem(T item, bool clearRestOfSelection)
        {
            if (item != null)
            {
                DataManagerBase manager = this.DataManager;
                if (manager != null)
                {

                    if (manager != null && manager.Filters != null && manager.Filters.Count > 0)
                    {
                        manager.Filters.Clear();
                    }

					this.CurrentSearchText = string.Empty;	// JM 9-29-11 TFS89774

                    this.SelectItem(item.Data, manager.ResolveIndexForRecord(item.Data), clearRestOfSelection);
                }
            }
            else
                this.SelectItem(null, -1, clearRestOfSelection);
        }

        internal void SelectItem(object data, int index, bool clearRestOfSelection)
        {
            DataManagerBase manager = this.DataManager;

            if (manager != null)
            {
                if (clearRestOfSelection || (!this.AllowMultipleSelection && this.SelectedItem != data))
                {
                    if (this.SelectedItems.Count > 0)
                    {
                        if (data != null)
                            this._supressSelectionChangedEvent = true;

                        this.SelectedItems.Clear();

                        this._supressSelectionChangedEvent = false;
                    }
                }

                T cei = this.Items[manager.ResolveIndexForRecord(data)];

                // We should always go into this code if CustomValueEnteredAction == Allow, as there will never be a cei created for it.
                //GT 8/20/2010 Add the condition data!=null for CustomeValueEnteredAction, because the collection throws NRE
                if ((cei != null && cei.IsEnabled && data != null) || (data != null && this.CustomValueEnteredAction == CustomValueEnteredActions.Allow))
                {
                    if (this.SelectedItems.Contains(data))
                    {
                        this._supressSelectionChangedEvent = true;
                        this.SelectedItems.Remove(data);
                    }

                    this.SelectedItems.Add(data);

                    this._supressSelectionChangedEvent = false;
                }
            }
        }


        #endregion // SelectItem

        #region UnselectItem

        internal void UnselectItem(int index)
        {
            this.UnselectItem(this.DataManager.GetRecord(index));
        }

        internal void UnselectItem(T item)
        {
            if (item != null)
                this.UnselectItem(item.Data);
            else
                this.UnselectItem(null);
        }

        internal void UnselectItem(object data)
        {
            this.SelectedItems.Remove(data);
        }

        #endregion // UnselectItem

        #region OnComboEditorItemClicked

        internal virtual void OnComboEditorItemClicked(T comboEditorItem)
        {
            // JM 02-23-12 TFS100053, TFS100158


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


            bool ctrlKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) || this.CheckBoxVisibility == Visibility.Visible;

            if (comboEditorItem.IsSelected)
            {
                if (ctrlKeyPressed)
                    this.UnselectItem(comboEditorItem);
                else
                {
                    this._supressSelectionChangedEvent = true;

                    this.UnselectItem(comboEditorItem);
                    this.SelectItem(comboEditorItem, true);

                    this._supressSelectionChangedEvent = false;
                }
            }
            else
                this.SelectItem(comboEditorItem, !(ctrlKeyPressed && this.AllowMultipleSelection));

            if (!ctrlKeyPressed || !this.AllowMultipleSelection)
                this.IsDropDownOpen = false;
			else  // JM 10-14-11 TFS91993
				this._skipDropDownClosingOnMouseClick = true;	
        }

        #endregion //OnComboEditorItemClicked

        #region DropDownHeight
        internal double DropDownHeight
        {
            get;
            set;
        }
        #endregion // DropDownHeight

		// JM 07-18-11 - XamMultiColumnComboEditor.
		#region GetAlternateFilters

		internal virtual RecordFilterCollection GetAlternateFilters(string text)
		{
			return null;
		}

		#endregion //GetAlternateFilters

		// JM 07-18-11 - XamMultiColumnComboEditor.
		#region OnCurrentSearchTextChanged

		internal virtual void OnCurrentSearchTextChanged(string currentSearchText)
		{
		}

		#endregion //OnCurrentSearchTextChanged

		// JM 08-17-11 - XamMultiColumnComboEditor.
		#region ClearCurrentSelection
		internal void ClearCurrentSelection()
		{
			this.SelectedItems.Clear();

			// JM 9-7-11 TFS84984
			if (this.IsDropDownOpen == true)
				this.InvalidateScrollPanel(false, false);
		}
		#endregion //ClearCurrentSelection

		#endregion //Internal

		#region Public

		#region ScrollItemIntoView

		/// <summary>
        /// Scrolls the specified <see cref="ComboEditorItem"/> into view in the drop down.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toTop"></param>
        public void ScrollItemIntoView(T item, bool toTop)
        {
            if (item != null && this._panel != null)
            {
                this._panel.ScrollItemIntoView(item, toTop);
            }
        }

        /// <summary>
        /// Trys to resolve the specified data item as a <see cref="ComboEditorItem"/> and scrolls it into view, in the drop down.
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="toTop"></param>
        public void ScrollItemIntoView(object dataItem, bool toTop)
        {
            DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                int index = manager.ResolveIndexForRecord(dataItem);
                if (index != -1)
                    this.ScrollItemIntoView(this.Items[index], toTop);
            }
        }

        #endregion // ScrollItemIntoView

        #endregion // Public

        #region Static

		// JM 06-26-12 TFS115507 Added.

		#region GetValueFromDataRow
		private static object GetValueFromDataRow(string fieldName, object dataItem)
		{
			if (dataItem is DataRowView)
				return ((DataRowView)dataItem)[fieldName];

			if (dataItem is DataRow)
				return ((DataRow)dataItem)[fieldName];

			return null;
		}
		#endregion //GetValueFromDataRow


		// JM 05-24-11 Port to WPF.
        #region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

        #endregion // RegisterResources

        #region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

        #endregion // UnregisterResources

        #endregion // Static

        #endregion //Methods

        #region Events

        #region DropDownClosing

        /// <summary>
        /// Occurs when the IsDropDownOpen property is changing from true to false. 
        /// </summary>
        public event EventHandler<CancelEventArgs> DropDownClosing;

        /// <summary>
        /// Called before the DropDownClosing event occurs.
        /// </summary>
        protected virtual bool OnDropDownClosing()
        {
            if (this.DropDownClosing != null)
            {
                CancelEventArgs e = new CancelEventArgs();
                this.DropDownClosing(this, e);
                return e.Cancel;
            }

            return false;
        }

        #endregion //DropDownClosing

        #region DropDownClosed

        /// <summary>
        /// Occurs when the IsDropDownOpen property was changed from true to false and the drop-down is closed.
        /// </summary>
        public event EventHandler DropDownClosed;

        /// <summary>
        /// Called before the DropDownClosed event occurs.
        /// </summary>
        protected virtual void OnDropDownClosed()
        {
			// JM 07-18-11 - XamMultiColumnComboEditor
			this.DropDownOpenedViaTyping = false;

			if (this.DropDownClosed != null)
            {
                this.DropDownClosed(this, EventArgs.Empty);
			}
        }

        #endregion //DropDownClosed

        #region ItemAdding

        /// <summary>
        /// Occurs when an item is going to be added to the underlying ComboEditorItemCollection of the ComboEditorBase
        /// </summary>
        public event EventHandler<ComboItemAddingEventArgs<T>> ItemAdding;

        /// <summary>
        /// Called before the ItemAdding event occurs.
        /// </summary>
        protected virtual bool OnItemAdding(T item)
        {
            if (this.ItemAdding != null)
            {
                ComboItemAddingEventArgs<T> e = new ComboItemAddingEventArgs<T>(item);
                this.ItemAdding(this, e);
                return e.Cancel;
            }

            return false;
        }

        #endregion //ItemAdding

        #region ItemAdded

        /// <summary>
        /// Occurs when an item is added to the underlying ComboEditorItemCollection of the ComboEditorBase
        /// </summary>
        public event EventHandler<ComboItemAddedEventArgs<T>> ItemAdded;

        /// <summary>
        /// Called before the ItemAdded event occurs.
        /// </summary>
        protected virtual void OnItemAdded(T item)
        {
            if (this.ItemAdded != null)
            {
                this.ItemAdded(this, new ComboItemAddedEventArgs<T>(item));
            }
        }

        #endregion //ItemAdded

        #region DropDownOpening

        /// <summary>
        /// Occurs when the value of the IsDropDownOpen property is changing from false to true. 
        /// </summary>
        public event EventHandler<CancelEventArgs> DropDownOpening;

        /// <summary>
        /// Called before the DropDownOpening event occurs.
        /// </summary>
        protected virtual bool OnDropDownOpening()
        {
            if (this.DropDownOpening != null)
            {
                CancelEventArgs e = new CancelEventArgs();
                this.DropDownOpening(this, e);
                return e.Cancel;
            }
            return false;
        }

        #endregion //DropDownOpening

        #region DropDownOpened

        /// <summary>
        /// Occurs when the value of the IsDropDownOpen property has changed from false to true and the drop-down is open.
        /// </summary>
        public event EventHandler DropDownOpened;

        /// <summary>
        /// Called before the DropDownOpened event occurs.
        /// </summary>
        protected virtual void OnDropDownOpened()
        {
            if (this.DropDownOpened != null)
            {
                this.DropDownOpened(this, EventArgs.Empty);
            }
        }

        #endregion //DropDownOpened

        #region SelectionChanged

        /// <summary>
        /// Occurs when the selection of the ComboEditorBase changes.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Called before the SelectionChanged event occurs.
        /// </summary>
        protected virtual void OnSelectionChanged()
        {

			// JM 11-11-11 TFS94292
			if (this.Editor != null)
				// JM 04-09-12 TFS107171 - Do the scroll asynchronously.
				//this.Editor.ScrollToHorizontalOffset(999999999);
				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					this.Editor.ScrollToHorizontalOffset(999999999);
				}), DispatcherPriority.Render);


			if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
			}

			// JM 11-18-11 TFS96115
			if (null != this.Panel)
				this.Panel.InvalidateMeasure();
        }

        #endregion //SelectionChanged

        #region DataObjectRequested

        /// <summary>
        /// This event is raised, when the ComboEditorBase needs to create a new data object.
        /// </summary>
        public event EventHandler<HandleableObjectGenerationEventArgs> DataObjectRequested;

        /// <summary>
        /// Called before the DataObjectRequested event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected virtual void OnDataObjectRequested(HandleableObjectGenerationEventArgs e)
        {
            if (this.DataObjectRequested != null)
            {
                this.DataObjectRequested(this, e);
            }
        }

        #endregion //DataObjectRequested

        #endregion //Events

        #region Event Handlers

        #region DataManager_NewObjectGeneration

        private void DataManager_NewObjectGeneration(object sender, HandleableObjectGenerationEventArgs e)
        {
            this.OnDataObjectRequested(e);
        }

        #endregion //DataManager_NewObjectGeneration

        #region DataManager_DataUpdated

        private void DataManager_DataUpdated(object sender, EventArgs e)
        {
            this.InvalidateItems();

            // The collection changed, which means we might have more or less items, and that means we need to recalculate our height
            // in the dropdown. 
            this._lastSetHeight = double.NaN;
            this._updateDropDownPosition = true;
            this._setDropDownPostion = true;

			// JM 02-24-12 TFS99858
			this.Dispatcher.BeginInvoke(new Action(() =>
			{
				this.InvalidateDropDownPosition(true);
			}));
		}

        #endregion //DataManager_DataUpdated

        #region DataManager_CollectionChanged

        void DataManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CustomValueEnteredAction != CustomValueEnteredActions.Allow)
            {
                List<object> itemsToRemove = new List<object>();

                foreach (object item in this.SelectedItems)
                {
                    if (this.DataManager.ResolveIndexForRecord(item) == -1)
                        itemsToRemove.Add(item);
                }

                foreach (object item in itemsToRemove)
                    this.SelectedItems.Remove(item);
            }

            // The collection changed, which means we might have more or less items, and that means we need to recalculate our height
            // in the dropdown. 
            this._lastSetHeight = double.NaN;

			// JM 02-24-12 TFS99858
			this.InvalidateDropDownPosition(true);
		}
        #endregion // DataManager_CollectionChanged

		#region Parent_MouseLeftButtonDown

		private void RootVisual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Validate that the item clicked, was ourself
            bool found = false;
            DependencyObject focused = e.OriginalSource as DependencyObject;
            while (focused != null)
            {
                if (object.ReferenceEquals(focused, this))
                {
                    found = true;
                    break;
                }


				if (focused is Run)
				    focused = ((Run)focused).Parent;
				if (false == focused is Visual)
					break;





				// This helps deal with popups that may not be in the same visual tree
				DependencyObject parent = VisualTreeHelper.GetParent(focused);
                if (parent == null)
                {
                    // Try the logical parent.
                    FrameworkElement element = focused as FrameworkElement;
                    if (element != null)
                    {
                        parent = element.Parent;
                    }
                }

                focused = parent;
            }

			// JM 10-14-11 TFS91993
            //if (!found && this.IsDropDownOpen)
			if (!found && this.IsDropDownOpen && this._skipDropDownClosingOnMouseClick == false)
			{
                this.IsDropDownOpen = false;
            }

			this._skipDropDownClosingOnMouseClick = false;	// JM 10-14-11 TFS91993
        }

        #endregion //Parent_MouseLeftButtonDown

        #region XamWebComboEditor_SizeChanged

        private void XamWebComboEditor_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsDropDownOpen && this._rootPopupElement != null)
            {
				if (this.AllowDropDownResizingResolved == false)
					this._rootPopupElement.Width = e.NewSize.Width;
            }
        }

        #endregion //XamWebComboEditor_SizeChanged

        #region XamWebComboEditor_IsEnabledChanged

        private void XamWebComboEditor_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.EnsureVisualStates();
        }

        #endregion //XamWebComboEditor_IsEnabledChanged

        #region XamWebComboEditor_Loaded

        private void XamWebComboEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this._isLoaded = true;

            IEnumerable itemSource = (IEnumerable)this.GetValue(ItemsSourceProperty);

            if (itemSource != null && this._dataManager == null || (this._dataManager != null && this._dataManager.OriginalDataSource != itemSource ))
            {
                this.ApplyItemSource(itemSource);
            }

            this.IsTabStop = !this.IsEditableResolved;

            this.CheckDisplayMemberPath();

			// JM 05-24-11 Port to WPF.




			this.EnableCloseUpTriggerTrackers();

        }

        #endregion //XamWebComboEditor_Loaded

        #region XamComboEditor_Unloaded

        void XamComboEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            this._isLoaded = false;
            this.IsDropDownOpen = false;

			// JM 05-24-11 Port to WPF.



			this.DisableCloseUpTriggerTrackers();

		}

        #endregion // XamComboEditor_Unloaded

        #region VerticalScrollBar_ValueChanged

        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._panel.InvalidateMeasure();
        }

        #endregion //VerticalScrollBar_ValueChanged

        #region HorizontalScrollBar_ValueChanged

        private void HorizontalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._panel.InvalidateMeasure();
        }

        #endregion //HorizontalScrollBar_ValueChanged

        #region DelayTracker_Tick

        private void DelayTracker_Tick(object sender, EventArgs e)
        {
            this.ProcessEditorText(true);
            this._delayTracker.Stop();
        }

        #endregion //DelayTracker_Tick

        #region TimeoutTracker_Tick
        void TimeoutTracker_Tick(object sender, EventArgs e)
        {
            this._searchedText = string.Empty;
            this._timeoutTracker.Stop();
        }
        #endregion // TimeoutTracker_Tick

        #region ItemsSource_CollectionChanged

        void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this._dataManager = DataManagerBase.CreateDataManager(this.ItemsSource);
            if (this._dataManager != null)
            {
                this._equalsDataManager = DataManagerBase.CreateDataManager(this.ItemsSource);
                this._dataManager.AllowCollectionViewOverrides = false;
                this._equalsDataManager.AllowCollectionViewOverrides = false;

                INotifyCollectionChanged incc = this.ItemsSource as INotifyCollectionChanged;
                if (incc != null)
                    incc.CollectionChanged -= ItemsSource_CollectionChanged;

                this._dataManager.NewObjectGeneration += new EventHandler<HandleableObjectGenerationEventArgs>(DataManager_NewObjectGeneration);
                this._dataManager.DataUpdated += new EventHandler<EventArgs>(DataManager_DataUpdated);

                this.InitializeData();
            }
        }

        #endregion // ItemsSource_CollectionChanged

        #region RootPoupElement_MouseLeave

        void RootPoupElement_MouseLeave(object sender, MouseEventArgs e)
        {
            this._mouseIsOver = false;
        }
        #endregion // RootPoupElement_MouseLeave

        #region RootPoupElement_MouseEnter

        void RootPoupElement_MouseEnter(object sender, MouseEventArgs e)
        {
            this._mouseIsOver = true;
        }
        #endregion // RootPoupElement_MouseEnter

        #region RootPoupElement_MouseWheel
        void RootPoupElement_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.IsDropDownOpen)
            {
                if (this._mouseIsOver)
                {
                    bool handled = false;
                    int multiplier = (e.Delta < 0) ? 1 : -1;

                    if (this.VerticalScrollBar != null && this.VerticalScrollBar.Visibility == Visibility.Visible)
                    {
                        handled = true;
                        this.VerticalScrollBar.Value += this.VerticalScrollBar.SmallChange * multiplier;
                    }
                    else if (this.HorizontalScrollBar != null &&
                             this.HorizontalScrollBar.Visibility == Visibility.Visible)
                    {
                        handled = true;
                        this.HorizontalScrollBar.Value += this.HorizontalScrollBar.SmallChange * multiplier;
                    }

                    if (handled)
                    {
                        this.InvalidateScrollPanel(false);
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion // RootPoupElement_MouseWheel

        #region Editor_TextChanged

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
			// JM 02-13-12 TFS100051
			this._isTextCommitted = false;

			if (this.IsEditableResolved && !this._delayTracker.IsEnabled)
            {
                this._delayTracker.Start();
                this._lastAction = LastInvokeAction.TextBox;
            }
        }

        #endregion //Editor_TextChanged

        #region Editor_KeyDown
        void Editor_KeyDown(object sender, KeyEventArgs e)
        {
			// JM 07-18-11 - XamMultiColumnComboEditor
			this.OnEditorKeyDown(e);

            if (this.IsEditableResolved)
            {
                switch (e.Key)
                {
                    case Key.Tab:
						// JM 02-13-12 TFS100051
						this._isTextCommitted = false;

                        // Complete the selection.
                        if ((this.IsEditorTextValidInItemSource) && this.Editor.SelectedText.Length > 0)
                        {
                            this.CommitTextAsSelected(true, false, true, true);
                            e.Handled = true;
                        }

                        break;

                    case Key.Space:
						// JM 02-13-12 TFS100051
						this._isTextCommitted = false;

						// Complete the selection.
                        if ((this.IsEditorTextValidInItemSource && this.Editor.SelectedText.Length > 0 && !this.Editor.SelectedText.Contains(" ")))
                        {
                            this.CommitTextAsSelected(true, false, true, true);
                            e.Handled = true;
                        }

                        break;

                    case Key.Back:
						// JM 02-13-12 TFS100051
						this._isTextCommitted = false;

						if (this.IsEditorTextValidInItemSource)
                        {
                            int index = this.Editor.SelectionStart;

                            if (this.Editor.Text == this._previousText && this._previousText.Length > 0 && index != 0)
								// JM 08-07-12 TFS117770 - Pass 'true' to move the selection to the end.
                                //this.SetEditorText(this.Editor.Text.Remove(this.Editor.SelectionStart - 1, this.Editor.SelectionLength + 1), true, false);
								this.SetEditorText(this.Editor.Text.Remove(this.Editor.SelectionStart - 1, this.Editor.SelectionLength + 1), true, true);

                            // Depending on the CustomValueEnteredAction, we should perform setting the selectionStart at different times.
                            // Bug: 61333
                            if (this.CustomValueEnteredAction == CustomValueEnteredActions.Ignore)
                            {
                                if (this.Editor.Text.Length > index)
                                    this.Editor.SelectionStart = index;
                                else
                                    this.Editor.SelectionStart = this.Editor.Text.Length;
                            }

                            this.StartsWithItems.Clear();

                            this.ProcessEditorText(true, false);

                            // Depending on the CustomValueEnteredAction, we should perform setting the selectionStart at different times.
                            // Bug: 28248
                            if (this.CustomValueEnteredAction != CustomValueEnteredActions.Ignore && !this.AutoCompleteResolved)
                            {
                                if (this.Editor.Text.Length > index)
                                    this.Editor.SelectionStart = index;
                                else
                                    this.Editor.SelectionStart = this.Editor.Text.Length;
                            }
                        }
                        break;

                    case Key.Delete:
						// JM 02-13-12 TFS100051
						this._isTextCommitted = false;

						if (this.IsEditorTextValidInItemSource)
                        {
                            int index = this.Editor.SelectionStart;

                            this.ProcessEditorText(true);

                            if (this.Editor.Text.Length > index)
                                this.Editor.SelectionStart = index;
                            else
                                this.Editor.SelectionStart = this.Editor.Text.Length;
                        }

                        break;
                }
            }
        }
        #endregion // Editor_KeyDown
                
        #region Editor_LostFocus
        void Editor_LostFocus(object sender, RoutedEventArgs e)
        {
			// JM 01-05-12 TFS98823 - Uncomment the following code and enclose in an If block that checks for a 
			// CustomValueEnteredAction != Add.  
			if (this.CustomValueEnteredAction != CustomValueEnteredActions.Add)
			{
				// JM 11-30-11 TFS96801 - Comment this out and add an 'else' case below.
				//if (!this.AutoCompleteResolved)
				//{
				//    this.EnsureVisualStates();	// JM 9-15-11 TFS88099
				//    return;
				//}
				if (!this.AutoCompleteResolved)
				{
					this.EnsureVisualStates();	// JM 9-15-11 TFS88099
					return;
				}
			}

            bool isDropDown = this.IsDropDownOpen;

            this.ProcessEditorText(false);

			if (this.AutoCompleteResolved)
			{
				// JM 04-05-12 TFS108039
				this._isTextCommitted = false;

				this.CommitTextAsSelected(false, false, false, true);
			}
			else
				this.CommitTextAsSelected(!isDropDown, false, false, true);

            this.ClearFilters();

            this.EnsureVisualStates();
		}
        #endregion // Editor_LostFocus

        #region Editor_GotFocus
        void Editor_GotFocus(object sender, RoutedEventArgs e)
        {
            this.EnsureVisualStates();
        }
        #endregion // Editor_GotFocus

        #region XamWebComboEditor_MouseLeftButtonDown

        private void XamWebComboEditor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
			if (false == this.IsDropDownOpen)	// JM 07-2011 XamMultiColumnComboEditor
			{
				// JM 02-21-12 Use new common routine created for TFS102346
				//if (this.IsEditableResolved && this.Editor != null)
				//{
				//    this.Editor.Focus();
				//}
				//else
				//{
				//    this.Focus();
				//}
				this.SetFocusToControlOrEditor();

				e.Handled = true;
			}
        }
		#endregion // XamWebComboEditor_MouseLeftButtonDown

		// JM 01-23-12 TFS99917 Added.
		#region XamWebComboEditor_PreviewMouseLeftButtonDown
		private void XamWebComboEditor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (false == this.IsDropDownOpen)
				// JM 02-21-12 Noticed this while fixing 102346
				//this.Focus();
				this.SetFocusToControlOrEditor();
		}
		#endregion //XamWebComboEditor_PreviewMouseLeftButtonDown

		#region XamWebComboEditor_KeyDown

		private void XamWebComboEditor_KeyDown(object sender, KeyEventArgs e)
        {
			int increment = 1;

			if (this.IsEnabled)
			{
				switch (e.Key)
				{

					case Key.F4:
						if (!this.IsDropDownOpen)
							this.IsDropDownOpen = true;

						break;

					case Key.Right:
					case Key.Down:
						if (!this.IsDropDownOpen)
						{
							T holdFocusItem = this._focusItem;
							this.IsDropDownOpen = true;
							
							if (holdFocusItem != null)
								this.SetFocusedItem(holdFocusItem, false);

							increment = 0;
						}

						this._lastAction = LastInvokeAction.ArrowKeys;

						if (this.IsEditableResolved && e.Key == Key.Right)
						{
							break;
						}

						if (this.StartsWithItems.Count > 0 && this.AutoCompleteResolved)
						{
							this.NavigateAutoCompleteOptions(true, true, false, false);
						}
						else
						{
							if (this._focusItem != null)
							{
								int index = this.Items.IndexOf(this._focusItem);

								if (index != -1 && (index + 1) != this.Items.Count)
								{
									// JM 08-07-12 TFS118024
									//this.SetFocusedItem(this.Items[index + increment], false);
									this.SetFocusToNextEnabledItem(this.Items[index + increment], true, false);
								}
								else if (index == -1 && this.Items.Count > 0)
								{
									// JM 08-07-12 TFS118024
									//this.SetFocusedItem(this.Items[0], false);
									this.SetFocusToNextEnabledItem(this.Items[0], true, false);
								}
							}
							else if (this.Items.Count > 0)
							{
								// JM 08-07-12 TFS118024
								//this.SetFocusedItem(this.Items[0], false);
								this.SetFocusToNextEnabledItem(this.Items[0], true, false);
							}

							if (this._focusItem != null)
								this.SetFocusedItem(this._focusItem, false);
						}
						e.Handled = true;
						break;

					case Key.Left:
					case Key.Up:
						if (!this.IsDropDownOpen)
						{
							T holdFocusItem = this._focusItem;
							this.IsDropDownOpen = true;

							if (holdFocusItem != null)
								this.SetFocusedItem(holdFocusItem, false);

							increment = 0;
						}

						this._lastAction = LastInvokeAction.ArrowKeys;

						if (this.IsEditableResolved && e.Key == Key.Left)
						{
							break;
						}

						if (this.StartsWithItems.Count > 0 && this.AutoCompleteResolved)
						{
							this.NavigateAutoCompleteOptions(false, true, false, false);
						}
						else if (this._focusItem != null)
						{
							int index = this.Items.IndexOf(this._focusItem);
							if (index != -1 && (index - 1) != -1)
							{
								// JM 08-07-12 TFS118024
								//this.SetFocusedItem(this.Items[index - increment], false);
								this.SetFocusToNextEnabledItem(this.Items[index - increment], false, false);
							}
						}

						if (this._focusItem != null)
							this.SetFocusedItem(this._focusItem, false);

						e.Handled = true;
						break;
				}
			}

            if (!e.Handled && this.IsEnabled)
            {
                switch (e.Key)
                {
					case Key.Space:

						if (this.IsDropDownOpen && !this.IsEditableResolved && !this.AllowFilteringResolved && this._focusItem != null)
						{
							if (!this._focusItem.IsSelected)
							{
								if (this._focusItem.IsEnabled)
								{
									this.SelectItem(this._focusItem, true);
									this.IsDropDownOpen = false;
								}
							}
							else
							{
								this.IsDropDownOpen = false;
							}

							e.Handled = true;
						}

						break;




					case Key.Return:

						if (this.IsDropDownOpen)
						{
							if (this._focusItem != null)
							{
								if (this.AllowMultipleSelection)
								{
									bool ctrlKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
									if (this._focusItem.IsSelected && ctrlKeyPressed)
										this.UnselectItem(this._focusItem);
									else if (this._focusItem.IsEnabled)
										// JM 08-07-12 TFS117433
										//this.SelectItem(this._focusItem, !ctrlKeyPressed);
										this.SelectItem(this._focusItem, false);

									if (!ctrlKeyPressed)
										this.IsDropDownOpen = false;
								}
								else if (!this._focusItem.IsSelected)
								{
									if (this._focusItem.IsEnabled)
									{
										this.SelectItem(this._focusItem, true);
										this.IsDropDownOpen = false;
									}
								}
								else
								{
									this.IsDropDownOpen = false;
								}
							}
							else
							{
								this.CommitTextAsSelected(true, false, true, false);
							}

						}
						else if (this.CustomValueEnteredAction == CustomValueEnteredActions.Add)
						{
							string text = this.Editor.Text;
							text = text.Remove(this.Editor.SelectionStart, this.Editor.SelectionLength);
							this.SetEditorText(text, true, true);
							this.CommitTextAsSelected(true, false, true, false);
						}
						else if (this.IsEditorTextValidInItemSource)
						{
							this.CommitTextAsSelected(true, false, true, true);

							// JM 02-22-12 TFS101666
							this.IsDropDownOpen = true;
						}
						else
						{
							this.IsDropDownOpen = true;
						}
						e.Handled = true;
						break;
					case Key.Escape:

                        bool isDropDownOpen = this.IsDropDownOpen;

                        if (this.IsEditorTextValidInItemSource)
                        {
                            this.CommitTextAsSelected(true, true, true, true);
                        }

                        // NZ 18 June 2012 TFS105545, TFS105981
                        if (isDropDownOpen)
                        {
                            e.Handled = true;
                            this.IsDropDownOpen = false;    
                        }

                        break;
                }

                if (!this.IsEditableResolved && !e.Handled)
                {
                    if ((this.IsDropDownOpen || this.OpenDropDownOnTypingResolved) && this.AllowFilteringResolved)
                    {
                        string symbol = string.Empty;

                        // Searching in Selectable mode.
                        if (e.Key == Key.Back) // catch the backspace key.
                        {
                            if (!string.IsNullOrEmpty(this._searchedText))
                            {
                                this._searchedText = this._searchedText.Substring(0, this._searchedText.Length - 1);
                            }
                            else
                            {
                                this.ClearFilters();
                            }
                        }
                        else
                        {
                            string keyString = e.Key.ToString();

                            if (e.Key == Key.Space)
                            {
                                symbol = " ";
                            }
                            else if (keyString.Length == 1 && Char.IsLetter(keyString[0]))
                            {
                                symbol = keyString[0].ToString();
                            }
                            else if (keyString.Length == 2 && keyString[0] == 'D' && Char.IsNumber(keyString[1]))
                            {
                                symbol = keyString[1].ToString();
                            }


                            this._searchedText += symbol;
                        }

                        if (!string.IsNullOrEmpty(this._searchedText))
                        {
                            this.SearchAndFilterItemsByText(this._searchedText, false);

                            if (this.Items.Count > 0)
                            {
                                if (_timeoutTracker.IsEnabled)
                                {
                                    _timeoutTracker.Stop(); // reset the remaining time.
                                }

                                _timeoutTracker.Start();
                            }
                            else
                            {
                                this._searchedText = string.Empty;
                                this.ClearFilters();
                            }
                        }
                        else
                        {
                            this.ClearFilters();
                        }

                        if (this.OpenDropDownOnTypingResolved && this.Items.Count > 0 && !this.IsDropDownOpen && !string.IsNullOrEmpty(symbol))
                        {
                            this.IsDropDownOpen = true;
                        }
                    }
                }
            }

            base.OnKeyDown(e);
        }

		#endregion // XamWebComboEditor_KeyDown

		// JM 08-07-12 TFS118024 Added
		#region SetFocusToNextEnabledItem
		private void SetFocusToNextEnabledItem(T startAtItem, bool forward, bool scrollToTop)
		{
			if (startAtItem.IsEnabled)
			{
				this.SetFocusedItem(startAtItem, scrollToTop);
				return;
			}

			T lastItemProcessed = startAtItem;

			int index		= this.Items.IndexOf(startAtItem);
			int increment	= forward ? 1 : -1;
			index			+= increment;
			int limit		= forward ? this.Items.Count - 1 : 0;

			while (forward && index <= limit  || false == forward && index >= limit) 
			{
				T t = this.Items[index];

				if (t.IsEnabled)
				{
					this.SetFocusedItem(t, scrollToTop);
					return;
				}
				else
					lastItemProcessed = t;

				index += increment;
			}

			// If we are here we didn't explicitly set focus to an item, so scroll the last item
			// we processed into view.  
			if (null != lastItemProcessed && this.IsDropDownOpen)
				this.ScrollItemIntoView(lastItemProcessed, scrollToTop);
		}
		#endregion //SetFocusToNextEnabledItem

		#region SelectedItems_CollectionChanged
		void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (object data in this._backupSelectedItems)
                {
                    if (this._cachedRows.ContainsKey(data))
                        this._cachedRows[data].SetSelected(false);

                }
                this._backupSelectedItems.Clear();
            }

            if (!this.AllowMultipleSelection && this.SelectedItems.Count > 1)
            {
                throw new Exception(SRCombo.GetString("InvalidMultipleSelectionException"));
            }

            if (e.NewItems != null)
            {
                int index = e.NewStartingIndex;
                foreach (object data in e.NewItems)
                {
                    if (this._cachedRows.ContainsKey(data))
                        this._cachedRows[data].SetSelected(true);
                    else
                    {
                        int itemIndex = this.DataManager.ResolveIndexForRecord(data);
                        if (itemIndex != -1)
                            this.Items[itemIndex].SetSelected(true);
                    }

                    if (this._backupSelectedItems.Contains(data))
                    {
                        throw new Exception(SRCombo.GetString("InvalidSelection"));
                    }

                    this._backupSelectedItems.Insert(index, data);
                    index++;
                }
            }

            if (e.OldItems != null)
            {
                int index = e.OldStartingIndex;
                foreach (object data in e.OldItems)
                {
                    if (this._cachedRows.ContainsKey(data))
                        this._cachedRows[data].SetSelected(false);

                    this._backupSelectedItems.RemoveAt(index);
                    index++;
                }
            }

            // Since we're supressing, don't change the SelectedItem, otherwise, what we wind up doing is causing the setter
            // of any bound properties to the SelectedItem to fire unnecessarily.
            if (!this._supressSelectionChangedEvent)
            {
                this._ignoreSelectionChanging = true;

                if (this.SelectedItems.Count > 0)
                {
                    object newData = null;

                    if (e.NewItems != null && e.NewItems.Count > 0)
                    {
                        newData = e.NewItems[e.NewItems.Count - 1];
                    }
                    else
                    {
                        newData = this.SelectedItems[this.SelectedItems.Count - 1];
                    }

                    this.SelectedIndex = this.DataManager.ResolveIndexForRecord(newData);
                    this.SelectedItem = newData;
                }
                else
                {
                    this.SelectedIndex = -1;
                    this.SelectedItem = null;
                }


                this._ignoreSelectionChanging = false;

                if (this.SelectedItem != null)
                {
					// JM 08-01-12 TFS117221 - Comment out the dropdown closed check, and conditionally scroll in the
					// SetFocusedItem call below depending on whether the dropdown is open
					// JM 11-11-11 TFS94280 - Only set the focused item in this scenario if the dropdown is closed.
					//if (this.IsDropDownOpen == false)
					{
						if (this._focusItem != null)
						{
							if (this._focusItem.Data != this.SelectedItem)
							{
								int index = this.DataManager.ResolveIndexForRecord(this.SelectedItem);
								// JM 08-01-12 TFS117221 - Pass new 3rd parameter and only scroll if the dropdown is closed
								//this.SetFocusedItem(this.Items[index], false);
								this.SetFocusedItem(this.Items[index], false, this.IsDropDownOpen == false);
							}
						}
						else
						{
							int index = this.DataManager.ResolveIndexForRecord(this.SelectedItem);
							this.SetFocusedItem(this.Items[index], false);
						}
					}
                }
                else
                {
                    this.SetFocusedItem(null, false);
                }
            
                this.OnSelectionChanged();
            }

            this.InvalidateSelectedContent();

            this.EnsureVisualStates();

			CommandSourceManager.NotifyCanExecuteChanged(typeof(MultiColumnComboEditorClearSelectionCommand));
        }
        #endregion // SelectedItems_CollectionChanged

        #region ComboEditorBase_SizeChanged

        void ComboEditorBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this._rootPopupElement != null)
                this._rootPopupElement.MinWidth = this.ActualWidth;
        }

        #endregion // ComboEditorBase_SizeChanged

        #endregion //Event Handlers

        #region IProvideDataItems<T> Members

        int IProvideDataItems<T>.DataCount
        {
            get
            {
                return this.DataCount;
            }
        }

        T IProvideDataItems<T>.GetDataItem(int index)
        {
            return this.GetDataItem(index);
        }

        T IProvideDataItems<T>.CreateItem()
        {
            return this.CreateItem();
        }

        T IProvideDataItems<T>.CreateItem(object dataItem)
        {
            return this.CreateItem(dataItem);
        }

        void IProvideDataItems<T>.AddItem(T addedObject)
        {            
            if (!this.OnItemAdding(addedObject))
            {
                this.AddItem(addedObject);
                this.OnItemAdded(addedObject);
            }
        }

        bool IProvideDataItems<T>.RemoveItem(T removedObject)
        {
            return this.RemoveItem(removedObject);
        }

        void IProvideDataItems<T>.RemoveRange(IList<T> itemsToRemove)
        {
            this.RemoveRange(itemsToRemove);
        }

        void IProvideDataItems<T>.InsertItem(int index, T insertedObject)
        {
            if (!this.OnItemAdding(insertedObject))
            {
                this.InsertItem(index, insertedObject);
                this.OnItemAdded(insertedObject);
            }
        }

        #endregion IProvideDataItems<T> Members

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the ComboEditorBase.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the ComboEditorBase object.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                List<string> list = new List<string>()
                {
                    "ItemsSource",
                    "Items",
                    "IsDropDownOpen"
                };

                return list;
            }
        }

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
        {
            get
            {
                return this.PropertiesToIgnore;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected virtual List<string> PriorityProperties
        {
            get
            {
                return null;
            }
        }
        List<string> IProvidePropertyPersistenceSettings.PriorityProperties
        {
            get { return this.PriorityProperties; }
        }


        #endregion // PriorityProperties

        #region FinishedLoadingPersistence
        /// <summary>
        /// Allows an object to perform an operation, after it's been loaded.
        /// </summary>
        protected virtual void FinishedLoadingPersistence()
        {

        }

        void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
        {
            this.FinishedLoadingPersistence();
        }
        #endregion // FinishedLoadingPersistence

        #endregion

		// JM 02-23-12 TFS100053, TFS100158 Added.
		#region ISupportScrollHelper Members

		double ISupportScrollHelper.GetFirstItemHeight()
		{
			if (this.Panel != null && this.Panel.VisibleItems.Count > 0)
				return this.Panel.VisibleItems[0].Control.DesiredSize.Height;
			else
				return 0;
		}

		double ISupportScrollHelper.GetFirstItemWidth()
		{
			return 100;
		}

		double ISupportScrollHelper.HorizontalMax
		{
			get
			{
				if (this.HorizontalScrollBar != null)
					return this.HorizontalScrollBar.Maximum;

				return 0;
			}
		}

		ScrollType ISupportScrollHelper.HorizontalScrollType
		{
			get 
            {
                if (this is XamComboEditor)
                    return ScrollType.Pixel;
                else
                    return ScrollType.Item;
            }
		}

		double ISupportScrollHelper.HorizontalValue
		{
			get
			{
				if (this.HorizontalScrollBar != null)
					return this.HorizontalScrollBar.Value;

				return 0;
			}
			set
			{
				if (this.HorizontalScrollBar != null)
					this.HorizontalScrollBar.Value = value;
			}
		}

		void ISupportScrollHelper.InvalidateScrollLayout()
		{
			this.InvalidateScrollPanel(false);
		}

		double ISupportScrollHelper.VerticalMax
		{
			get
			{
				if (this.VerticalScrollBar != null)
					return this.VerticalScrollBar.Maximum;

				return 0;
			}
		}

		ScrollType ISupportScrollHelper.VerticalScrollType
		{
			get { return ScrollType.Item; }
		}

		double ISupportScrollHelper.VerticalValue
		{
			get
			{
				if (this.VerticalScrollBar != null)
					return this.VerticalScrollBar.Value;

				return 0;
			}
			set
			{
				if (this.VerticalScrollBar != null)
					this.VerticalScrollBar.Value = value;
			}
		}


		TouchScrollMode ISupportScrollHelper.GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver)
		{
			return TouchScrollMode.Both;
		}

		void ISupportScrollHelper.OnStateChanged(TouchState newState, TouchState oldState)
		{


#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

		}

		void ISupportScrollHelper.OnPanComplete()
		{
			
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