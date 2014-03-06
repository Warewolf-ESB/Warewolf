using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Threading;
using System.Diagnostics;
using Infragistics.Windows.Editors;
using Infragistics.Collections;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

// AS - NA 11.2 Excel Style Filtering
namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Custom control used to display the available filtered cell values for a given field 
	/// </summary>
	[DesignTimeVisible(false)]
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	[TemplatePart(Name = PART_TREE, Type = typeof(TreeView))]
	[TemplatePart(Name = PART_SEARCHTEXTBOX, Type = typeof(TextBoxBase))]
	[TemplatePart(Name = PART_SEARCHSCOPECOMBO, Type = typeof(ComboBox))] // AS 8/19/11 TFS84512
	public class RecordFilterTreeControl : Control
	{
		#region Member Variables

		const string PART_TREE = "PART_Tree";
		const string PART_SEARCHTEXTBOX = "PART_SearchTextBox";
		const string PART_SEARCHSCOPECOMBO = "PART_SearchScopeCombo"; // AS 8/19/11 TFS84512

		private TextBoxBase _searchTextBox; // AS 8/19/11 TFS84468
		private ComboBox _searchScopeCombo; // AS 8/19/11 TFS84512
		private TreeView _tree;
		private ResolvedRecordFilterCollection.FieldFilterInfo _filterInfo;
		private ResolvedRecordFilterCollection.FilterDropDownItemLoader _valueLoader;
		private RecordFilterTreeItemProvider _treeItemProvider;
		private ObservableCollectionExtended<ComboBoxDataItem> _availableScopesInternal;
		private ReadOnlyObservableCollection<ComboBoxDataItem> _availableScopes;

		private ICommand _okCommand;
		private ICommand _cancelCommand;
		private ICommand _clearSearchTextCommand;

		private DispatcherTimer _searchTextTimer;
		
		#endregion //Member Variables

		#region Constructor
		static RecordFilterTreeControl()
		{
			Control.DefaultStyleKeyProperty.OverrideMetadata(typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(typeof(RecordFilterTreeControl)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		/// <summary>
		/// Initializes a new <see cref="RecordFilterTreeControl"/>
		/// </summary>
		public RecordFilterTreeControl()
		{
			// select/focus the node when checking/unchecking the checkbox in the node
			RoutedEventHandler handler = delegate(object sender, RoutedEventArgs e)
			{
				TreeViewItem item = Utilities.GetAncestorFromType(e.OriginalSource as DependencyObject, typeof(TreeViewItem), true) as TreeViewItem;

				if (null != item && item.IsLoaded && item.IsMouseOver && IsInValueTree(item))
					item.Focus();
			};
			this.AddHandler(CheckBox.CheckedEvent, handler, true);
			this.AddHandler(CheckBox.UncheckedEvent, handler, true);

			// initialize the default search text prompt
			this.VerifySearchTextPrompt();
		}
		#endregion //Constructor

		#region Base class overrides

		#region IsEnabledCore
		/// <summary>
		/// Used to determine if the element is enabled.
		/// </summary>
		protected override bool IsEnabledCore
		{
			get
			{
				// disable while loading
				return base.IsEnabledCore && !this.IsLoadingItems;
			}
		} 
		#endregion //IsEnabledCore

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size originalConstraint = constraint;

			if (double.IsPositiveInfinity(constraint.Width))
				constraint.Width = CalculateInfiniteExtent(constraint.Width, this.WidthInInfiniteContainers);

			if (double.IsPositiveInfinity(constraint.Height))
				constraint.Height = CalculateInfiniteExtent(constraint.Height, this.HeightInInfiniteContainers);

			Size desiredSize = base.MeasureOverride(constraint);

			if (double.IsPositiveInfinity(originalConstraint.Width) && originalConstraint.Width != constraint.Width)
				desiredSize.Width = constraint.Width;

			if (double.IsPositiveInfinity(originalConstraint.Height) && originalConstraint.Height != constraint.Height)
				desiredSize.Height = constraint.Height;

			return desiredSize;
		} 
		#endregion //MeasureOverride

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template has been applied to the control.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// AS 8/19/11 TFS84512
			if (_searchScopeCombo != null)
				_searchScopeCombo.PreviewKeyDown -= new KeyEventHandler(OnSearchScopePreviewKeyDown);

			_searchScopeCombo = this.GetTemplateChild(PART_SEARCHSCOPECOMBO) as ComboBox;

			if (_searchScopeCombo != null)
				_searchScopeCombo.PreviewKeyDown += new KeyEventHandler(OnSearchScopePreviewKeyDown);

			// AS 8/19/11 TFS84468
			// A textbox will handle the Up/Down keys even if it doesn't use them. Here we will assume that the 
			// search textbox is single line and shift focus similar to what MS does in their filter menu.
			//
			if (null != _searchTextBox)
				_searchTextBox.RemoveHandler(Keyboard.KeyDownEvent, new KeyEventHandler(OnSearchTextKeyDown));

			_searchTextBox = this.GetTemplateChild(PART_SEARCHTEXTBOX) as TextBoxBase;

			if (null != _searchTextBox)
				_searchTextBox.AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(OnSearchTextKeyDown), true);

			// set an inherited property on ourselves that we will catch when it hits treeview items
			this.SetValue(IsExpandedBindingProperty, new Binding { Path = new PropertyPath("IsExpanded"), Mode = BindingMode.TwoWay });

			// AS 8/19/11 TFS84468
			if (null != _tree)
				_tree.PreviewGotKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnTreePreviewGotFocus);

			_tree = this.GetTemplateChild(PART_TREE) as TreeView;

			// AS 8/19/11 TFS84468
			if (null != _tree)
				_tree.PreviewGotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnTreePreviewGotFocus);

			if (_tree != null)
			{
				var isVirtFieldInfo = typeof(VirtualizingStackPanel).GetField("IsVirtualizingProperty", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				var virtModeFieldInfo = typeof(VirtualizingStackPanel).GetField("VirtualizationModeProperty", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

				DependencyProperty dpIsVirtualizing = isVirtFieldInfo == null ? null : isVirtFieldInfo.GetValue(null) as DependencyProperty;
				DependencyProperty dpVirtMode = virtModeFieldInfo == null ? null : virtModeFieldInfo.GetValue(null) as DependencyProperty;

				if (dpIsVirtualizing != null && _tree.ReadLocalValue(dpIsVirtualizing) == DependencyProperty.UnsetValue)
					_tree.SetValue(dpIsVirtualizing, KnownBoxes.TrueBox);

				if (dpVirtMode != null && _tree.ReadLocalValue(dpVirtMode) == DependencyProperty.UnsetValue)
					_tree.SetValue(dpVirtMode, Enum.ToObject(dpVirtMode.PropertyType, 1));
			}
		}
		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="RecordFilterTreeControl"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.RecordFilterTreeControlAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.DataPresenter.RecordFilterTreeControlAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#region OnInitialized
		/// <summary>
		/// Invoked when the control has been initialized.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.InitializeValueLoader();
			this.ApplySearchText();
			this.VerifyAvailableScopes();
		}
		#endregion //OnInitialized

		#region OnKeyDown
		/// <summary>
		/// Invoked when the key is pressed down while focus is within the control.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Handled)
				return;

			Key key = e.Key;

			if (key == Key.System)
				key = e.SystemKey;

			if (key == Key.Space)
			{
				DependencyObject originalSource = e.OriginalSource as DependencyObject;
				TreeViewItem item = originalSource as TreeViewItem ?? Utilities.GetAncestorFromType(originalSource, typeof(TreeViewItem), true) as TreeViewItem;

				if (null != item && IsInValueTree(item))
				{
					RecordFilterTreeItem treeItem = item.DataContext as RecordFilterTreeItem;

					if (null != treeItem)
					{
						treeItem.IsChecked = treeItem.IsChecked == true ? false : true;
						e.Handled = true;
					}
				}
			}
			else if (key == Key.Enter)
			{
				// if the enter key is received then process this as a click of the ok
				if (this.CanUpdateRecordFilter(null))
				{
					this.UpdateRecordFilter(null);
					e.Handled = true;
				}
			}
			else if (key == Key.Escape)
			{
				// to emulate excel we want to clear the search when in the search text that 
				// has search criteria and the escape key is pressed
				if (this.HasSearchText)
				{
					var searchTextBox = this.GetTemplateChild(PART_SEARCHTEXTBOX) as TextBoxBase;

					if (searchTextBox != null && searchTextBox.IsKeyboardFocusWithin)
					{
						this.ClearSearchText(null);
						e.Handled = true;
					}
				}
			}
		}
		#endregion //OnKeyDown

		#region OnPreviewKeyDown
		/// <summary>
		/// Invoked when a key is pressed before the target element has received the notification.
		/// </summary>
		/// <param name="e">Provides information about the event.</param>
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if (e.Handled)
				return;

			var modifiers = Keyboard.Modifiers;
			Key key = e.Key;

			if (key == Key.System)
				key = e.SystemKey;

			if (key == Key.Up && modifiers == ModifierKeys.None)
			{
				// AS 8/19/11 TFS84468
				// When navigating up from the 1st node in a tree, the treeview item simply
				// eats the up arrow key because it thinks it should focus the treeview. To 
				// mimic excel we want to shift to the previous control.
				//
				TreeViewItem tvi = e.OriginalSource as TreeViewItem;

				if (tvi != null && _tree != null && ItemsControl.ItemsControlFromItemContainer(tvi) == _tree)
				{
					if (_tree.ItemContainerGenerator.IndexFromContainer(tvi) == 0)
					{
						if (_tree.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous)))
							e.Handled = true;
					}
				}
			}
		} 
		#endregion //OnPreviewKeyDown

		#region OnPreviewLostKeyboardFocus
		/// <summary>
		/// Invoked when the tree or an element within the tree is about to lose focus.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			// when the clear search text button is clicked the button calls Keyboard.Focus(null)
			// which tries to shift focus out of the focus scope (our filter menu) so we need to 
			// stop that
			if (e.NewFocus is Window && e.OldFocus is ICommandSource && ((ICommandSource)e.OldFocus).Command == this.ClearSearchTextCommand)
				e.Handled = true;

			base.OnPreviewLostKeyboardFocus(e);
		} 
		#endregion //OnPreviewLostKeyboardFocus

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region AvailableSearchScopes
		/// <summary>
		/// Returns a collection of items that represent the available scopes that may be used for the <see cref="SearchScope"/>
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IList<ComboBoxDataItem> AvailableSearchScopes
		{
			get
			{
				if (_availableScopes == null)
					_availableScopes = new ReadOnlyObservableCollection<ComboBoxDataItem>(this.AvailableSearchScopesInternal);

				return _availableScopes;
			}
		} 
		#endregion //AvailableSearchScopes

		#region CancelChangesCommand
		/// <summary>
		/// Returns a command that can be used to cancel changes to the <see cref="RecordFilter"/> for the associated <see cref="Field"/>
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ICommand CancelChangesCommand
		{
			get
			{
				if (_cancelCommand == null)
					_cancelCommand = new DelegateCommand(this.CancelRecordFilterChanges);

				return _cancelCommand;
			}
		}
		#endregion //CancelChangesCommand

		#region ClearSearchTextCommand
		/// <summary>
		/// Returns a command that can be used to clear the <see cref="SearchText"/>
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ICommand ClearSearchTextCommand
		{
			get
			{
				if (_clearSearchTextCommand == null)
					_clearSearchTextCommand = new DelegateCommand(this.ClearSearchText, this.CanClearSearchText);

				return _clearSearchTextCommand;
			}
		}
		#endregion //ClearSearchTextCommand

		#region Field

		private static readonly DependencyPropertyKey FieldPropertyKey = DependencyProperty.RegisterReadOnly("Field",
			typeof(Field), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="Field"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldProperty = FieldPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns or sets the Field whose record filters will be manipulated by the control.
		/// </summary>
		/// <seealso cref="FieldProperty"/>
		public Field Field
		{
			get
			{
				return (Field)this.GetValue(RecordFilterTreeControl.FieldProperty);
			}
			private set
			{
				this.SetValue(RecordFilterTreeControl.FieldPropertyKey, value);
			}
		}

		#endregion //Field

		#region HasEmptySearchResults

		private static readonly DependencyPropertyKey HasEmptySearchResultsPropertyKey =
			DependencyProperty.RegisterReadOnly("HasEmptySearchResults",
			typeof(bool), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnHasEmptySearchResultsChanged)));

		private static void OnHasEmptySearchResultsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandManager.InvalidateRequerySuggested();
		}

		/// <summary>
		/// Identifies the <see cref="HasEmptySearchResults"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasEmptySearchResultsProperty =
			HasEmptySearchResultsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the applied SearchText resulted in no matching values.
		/// </summary>
		/// <seealso cref="HasEmptySearchResultsProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HasEmptySearchResults
		{
			get
			{
				return (bool)this.GetValue(RecordFilterTreeControl.HasEmptySearchResultsProperty);
			}
		}

		#endregion //HasEmptySearchResults

		#region HasMultipleSearchScopes

		private static readonly DependencyPropertyKey HasMultipleSearchScopesPropertyKey =
			DependencyProperty.RegisterReadOnly("HasMultipleSearchScopes",
			typeof(bool), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasMultipleSearchScopes"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasMultipleSearchScopesProperty =
			HasMultipleSearchScopesPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the AvailableSearchScopes has multiple items and therefore a combo with the options should be displayed.
		/// </summary>
		/// <seealso cref="HasMultipleSearchScopesProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HasMultipleSearchScopes
		{
			get
			{
				return (bool)this.GetValue(RecordFilterTreeControl.HasMultipleSearchScopesProperty);
			}
		}

		#endregion //HasMultipleSearchScopes

		#region HasSearchText

		private static readonly DependencyPropertyKey HasSearchTextPropertyKey =
			DependencyProperty.RegisterReadOnly("HasSearchText",
			typeof(bool), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, null, new CoerceValueCallback(CoerceHasSearchText)));

		private static object CoerceHasSearchText(DependencyObject d, object newValue)
		{
			var rftc = d as RecordFilterTreeControl;

			if (false.Equals(newValue) && !string.IsNullOrEmpty(rftc.SearchText))
				return KnownBoxes.TrueBox;

			return newValue;
		}

		/// <summary>
		/// Identifies the <see cref="HasSearchText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasSearchTextProperty =
			HasSearchTextPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the SearchText property has a value.
		/// </summary>
		/// <seealso cref="HasSearchTextProperty"/>
		/// <seealso cref="SearchText"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HasSearchText
		{
			get
			{
				return (bool)this.GetValue(RecordFilterTreeControl.HasSearchTextProperty);
			}
		}

		#endregion //HasSearchText

		#region HeightInInfiniteContainers

		/// <summary>
		/// Identifies the <see cref="HeightInInfiniteContainers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeightInInfiniteContainersProperty = DependencyProperty.Register("HeightInInfiniteContainers",
			typeof(double), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(double.NaN));

		/// <summary>
		/// Returns or sets the height that the control should use when measured with an infinite height.
		/// </summary>
		/// <seealso cref="HeightInInfiniteContainersProperty"/>
		[Bindable(true)]
		public double HeightInInfiniteContainers
		{
			get
			{
				return (double)this.GetValue(RecordFilterTreeControl.HeightInInfiniteContainersProperty);
			}
			set
			{
				this.SetValue(RecordFilterTreeControl.HeightInInfiniteContainersProperty, value);
			}
		}

		#endregion //HeightInInfiniteContainers

		#region IsGroupingDates

		/// <summary>
		/// Identifies the <see cref="IsGroupingDates"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsGroupingDatesProperty = DependencyPropertyUtilities.Register("IsGroupingDates",
			typeof(bool), typeof(RecordFilterTreeControl),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsGroupingDatesChanged))
			);

		private static void OnIsGroupingDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RecordFilterTreeControl ctrl = d as RecordFilterTreeControl;

			if (ctrl._treeItemProvider != null)
			{
				// switch the mode and repopulate with what we have (if anything)
				ctrl._treeItemProvider.IsGroupingDates = (bool)e.NewValue;
				ctrl.RepopulateItems();
			}
		}

		/// <summary>
		/// Returns or sets a boolean indicating whether DateTime values should be grouped within the <see cref="Nodes"/>
		/// </summary>
		/// <seealso cref="IsGroupingDatesProperty"/>
		public bool IsGroupingDates
		{
			get
			{
				return (bool)this.GetValue(RecordFilterTreeControl.IsGroupingDatesProperty);
			}
			set
			{
				this.SetValue(RecordFilterTreeControl.IsGroupingDatesProperty, value);
			}
		}

		#endregion //IsGroupingDates

		#region IsLoadingItems

		private static readonly DependencyPropertyKey IsLoadingItemsPropertyKey =
			DependencyProperty.RegisterReadOnly("IsLoadingItems",
			typeof(bool), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsLoadingItemsChanged)));

		private static void OnIsLoadingItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tree = d as RecordFilterTreeControl;
			bool isLoading = (bool)e.NewValue;

			if (isLoading)
			{
				// if there happens to be search criteria then clear it from the provider while loading
				if (tree._treeItemProvider != null)
					tree._treeItemProvider.SetSearchCriteria(null, tree.SearchScope, false);
			}
			else
			{
				// when we are done loading we can re-apply any search criteria
				tree.ApplySearchText();
			}

			tree.CoerceValue(IsEnabledProperty);
		}

		/// <summary>
		/// Identifies the <see cref="IsLoadingItems"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsLoadingItemsProperty =
			IsLoadingItemsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the items are being loaded. This will be true while the items are being loaded asynchronously.
		/// </summary>
		/// <seealso cref="IsLoadingItemsProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsLoadingItems
		{
			get
			{
				return (bool)this.GetValue(RecordFilterTreeControl.IsLoadingItemsProperty);
			}
			private set
			{
				this.SetValue(IsLoadingItemsPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsLoadingItems

		#region Nodes
		/// <summary>
		/// Returns a collection of the items that represent the nodes of the tree.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public IEnumerable<RecordFilterTreeItem> Nodes 
		{
			get
			{
				return this.TreeItemProvider.AllItems;
			}
		}
		#endregion //Nodes

		#region RecordManager

		private static readonly DependencyPropertyKey RecordManagerPropertyKey = DependencyProperty.RegisterReadOnly("RecordManager",
			typeof(RecordManager), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="RecordManager"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordManagerProperty = RecordManagerPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns or sets the RecordManager whose record filters will be affected by the control.
		/// </summary>
		/// <seealso cref="RecordManagerProperty"/>
		public RecordManager RecordManager
		{
			get
			{
				return (RecordManager)this.GetValue(RecordFilterTreeControl.RecordManagerProperty);
			}
			private set
			{
				this.SetValue(RecordFilterTreeControl.RecordManagerPropertyKey, value);
			}
		}

		#endregion //RecordManager

		#region SearchScope

		/// <summary>
		/// Identifies the <see cref="SearchScope"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SearchScopeProperty = DependencyProperty.Register("SearchScope",
			typeof(RecordFilterTreeSearchScope), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(RecordFilterTreeSearchScope.All, new PropertyChangedCallback(OnSearchScopeChanged), new CoerceValueCallback(CoerceSearchScope) ));

		private static void OnSearchScopeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RecordFilterTreeControl ctrl = d as RecordFilterTreeControl;
			ctrl.VerifySearchTextPrompt();
			ctrl.ApplySearchText();
		}

		private static object CoerceSearchScope(DependencyObject d, object newValue)
		{
			RecordFilterTreeControl ctrl = d as RecordFilterTreeControl;

			var newScope = (RecordFilterTreeSearchScope)newValue;
			var mostSpecificScope = GetMostSpecificScope(ctrl.TreeItemProvider.MostSpecificCreatedDateType);

			if (mostSpecificScope < newScope)
				return mostSpecificScope;

			return newValue;
		}

		/// <summary>
		/// Returns or sets an enumeration used to determine which nodes are evaluated when the SearchText has been set.
		/// </summary>
		/// <seealso cref="SearchScopeProperty"/>
		[Bindable(true)]
		public RecordFilterTreeSearchScope SearchScope
		{
			get
			{
				return (RecordFilterTreeSearchScope)this.GetValue(RecordFilterTreeControl.SearchScopeProperty);
			}
			set
			{
				this.SetValue(RecordFilterTreeControl.SearchScopeProperty, value);
			}
		}

		#endregion //SearchScope

		#region SearchText

		/// <summary>
		/// Identifies the <see cref="SearchText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText",
			typeof(string), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSearchTextChanged)));

		private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RecordFilterTreeControl ctrl = d as RecordFilterTreeControl;

			ctrl.OnSearchTextChanged();
		}

		/// <summary>
		/// Returns or sets the text that should be used to search the values in the tree.
		/// </summary>
		/// <seealso cref="SearchTextProperty"/>
		[Bindable(true)]
		public string SearchText
		{
			get
			{
				return (string)this.GetValue(RecordFilterTreeControl.SearchTextProperty);
			}
			set
			{
				this.SetValue(RecordFilterTreeControl.SearchTextProperty, value);
			}
		}

		#endregion //SearchText

		#region SearchTextPrompt

		private static readonly DependencyPropertyKey SearchTextPromptPropertyKey =
			DependencyProperty.RegisterReadOnly("SearchTextPrompt",
			typeof(string), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="SearchTextPrompt"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SearchTextPromptProperty =
			SearchTextPromptPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the string that should be displayed in the search textbox when it doesn't have focus or search text.
		/// </summary>
		/// <seealso cref="SearchTextPromptProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public string SearchTextPrompt
		{
			get
			{
				return (string)this.GetValue(RecordFilterTreeControl.SearchTextPromptProperty);
			}
		}

		#endregion //SearchTextPrompt

		#region UpdateRecordFilterCommand
		/// <summary>
		/// Returns a command that can be used to commit changes to the <see cref="RecordFilter"/> for the associated <see cref="Field"/>
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ICommand UpdateRecordFilterCommand
		{
			get
			{
				if (_okCommand == null)
					_okCommand = new DelegateCommand(this.UpdateRecordFilter, this.CanUpdateRecordFilter);

				return _okCommand;
			}
		}
		#endregion //UpdateRecordFilterCommand

		#region WidthInInfiniteContainers

		/// <summary>
		/// Identifies the <see cref="WidthInInfiniteContainers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WidthInInfiniteContainersProperty = DependencyProperty.Register("WidthInInfiniteContainers",
			typeof(double), typeof(RecordFilterTreeControl), new FrameworkPropertyMetadata(double.NaN));

		/// <summary>
		/// Returns or sets the width that the control should use when measured with an infinite width.
		/// </summary>
		/// <seealso cref="WidthInInfiniteContainersProperty"/>
		[Bindable(true)]
		public double WidthInInfiniteContainers
		{
			get
			{
				return (double)this.GetValue(RecordFilterTreeControl.WidthInInfiniteContainersProperty);
			}
			set
			{
				this.SetValue(RecordFilterTreeControl.WidthInInfiniteContainersProperty, value);
			}
		}

		#endregion //WidthInInfiniteContainers

		#endregion //Public Properties

		#region Private Properties

		#region AvailableSearchScopesInternal
		private ObservableCollectionExtended<ComboBoxDataItem> AvailableSearchScopesInternal
		{
			get
			{
				if (_availableScopesInternal == null)
					_availableScopesInternal = new ObservableCollectionExtended<ComboBoxDataItem>();

				return _availableScopesInternal;
			}
		} 
		#endregion //AvailableSearchScopesInternal

		#region IsExpandedBinding

		private static readonly DependencyProperty IsExpandedBindingProperty =
			DependencyProperty.RegisterAttached("IsExpandedBinding", typeof(Binding), typeof(RecordFilterTreeControl),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnIsExpandedBindingChanged)));

		/// <summary>
		/// Handles changes to the IsExpandedBinding property.
		/// </summary>
		private static void OnIsExpandedBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TreeViewItem tvi = d as TreeViewItem;

			if (null != tvi)
				OnIsExpandedBindingChanged(tvi, e);
		}

		private static void OnIsExpandedBindingChanged(TreeViewItem tvi, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				if (IsInValueTree(tvi))
				{
					tvi.SetBinding(TreeViewItem.IsExpandedProperty, e.NewValue as Binding);
				}
			}
			else
			{
				// only clear the binding if we applied it to the item
				if (BindingOperations.GetBinding(tvi, TreeViewItem.IsExpandedProperty) == e.OldValue)
					BindingOperations.ClearBinding(tvi, TreeViewItem.IsExpandedProperty);
			}
		}

		#endregion //IsExpandedBinding

		#region ValueLoader
		internal ResolvedRecordFilterCollection.FilterDropDownItemLoader ValueLoader
		{
			get { return _valueLoader; }
			set
			{
				if (value != _valueLoader)
				{
					if (null != _valueLoader)
					{
						_valueLoader.Phase2Completed -= new EventHandler(OnValueLoaderPhase2Completed);
						_valueLoader.Phase2Updated -= new EventHandler(OnValueLoaderPhase2Updated);
					}

					_valueLoader = value;

					if (this.IsInitialized)
						this.InitializeValueLoader();
				}
			}
		}
		#endregion //ValueLoader

		#region TreeItemProvider
		private RecordFilterTreeItemProvider TreeItemProvider
		{
			get
			{
				if (_treeItemProvider == null)
				{
					_treeItemProvider = new RecordFilterTreeItemProvider(null);
					_treeItemProvider.IsGroupingDates = this.IsGroupingDates;
					_treeItemProvider.PropertyChanged += new PropertyChangedEventHandler(OnTreeItemProviderPropertyChanged);

					if (null != _filterInfo)
						_treeItemProvider.FilterInfo = _filterInfo;

					if (_valueLoader != null)
						this.RepopulateItems();
				}

				return _treeItemProvider;
			}
		}
		#endregion //TreeItemProvider

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region ApplySearchText
		private void ApplySearchText()
		{
			if (this.IsLoadingItems)
				return;

			// make sure the timer is not running
			this.StopSearchTextTimer();

			this.TreeItemProvider.SetSearchCriteria(this.SearchText, this.SearchScope, false);
		} 
		#endregion //ApplySearchText

		#region CalculateInfiniteExtent
		private double CalculateInfiniteExtent(double extent, double extentWhenInfinite)
		{
			if (double.IsPositiveInfinity(extent)
				&& !double.IsNaN(extentWhenInfinite)
				&& !double.IsInfinity(extentWhenInfinite)
				&& extentWhenInfinite >= 0)
			{
				extent = extentWhenInfinite;
			}

			return extent;
		}
		#endregion //CalculateInfiniteExtent

		#region CancelRecordFilterChanges
		private void CancelRecordFilterChanges(object parameter)
		{
			// clear the search
			this.SearchText = null;

			// reset the checked state to the original
			this.TreeItemProvider.ResetIsChecked();

			// apply the cleared search
			this.ApplySearchText();

			// shift focus out if needed
			if (!this.TransferFocus())
				this.CloseContainingMenu();
		} 
		#endregion //CancelRecordFilterChanges

		#region CanClearSearchText
		private bool CanClearSearchText(object parameter)
		{
			return this.HasSearchText;
		} 
		#endregion //CanClearSearchText

		#region CanUpdateRecordFilter
		private bool CanUpdateRecordFilter(object parameter)
		{
			if (_filterInfo == null)
				return false;

			return _treeItemProvider != null && _treeItemProvider.CanUpdateRecordFilter;
		} 
		#endregion //CanUpdateRecordFilter

		#region ClearSearchText
		private void ClearSearchText(object parameter)
		{
			this.SearchText = null;
		} 
		#endregion //ClearSearchText

		#region CloseContainingMenu
		private void CloseContainingMenu()
		{
			var focusScope = FocusManager.GetFocusScope(this);
			var menu = focusScope as Menu;

			// i found this during testing. basically if the grid was in a contentpane then when 
			// the button was clicked and it tried to shift focus 
			if (null != menu && menu.IsMouseCaptured && !menu.IsKeyboardFocusWithin)
			{
				MenuItem mi = Utilities.GetAncestorFromType(this, typeof(MenuItem), true) as MenuItem;

				if (null != mi)
					menu.ReleaseMouseCapture();
			}
		}
		#endregion //CloseContainingMenu 

		#region CreateScopeItem
		private static ComboBoxDataItem CreateScopeItem(RecordFilterTreeSearchScope scope)
		{
			return new ComboBoxDataItem(scope, GetScopeDisplayText(scope));
		}
		#endregion //CreateScopeItem

		#region GetMostSpecificScope
		private static RecordFilterTreeSearchScope GetMostSpecificScope(RecordFilterTreeItemType? mostSpecificType)
		{
			if (mostSpecificType == null)
				return RecordFilterTreeSearchScope.All;

			switch (mostSpecificType.Value)
			{
				case RecordFilterTreeItemType.Year:
					return RecordFilterTreeSearchScope.Year;
				case RecordFilterTreeItemType.Month:
					return RecordFilterTreeSearchScope.Month;
				case RecordFilterTreeItemType.Day:
					return RecordFilterTreeSearchScope.Day;
				case RecordFilterTreeItemType.Hour:
					return RecordFilterTreeSearchScope.Hour;
				case RecordFilterTreeItemType.Minute:
					return RecordFilterTreeSearchScope.Minute;
				case RecordFilterTreeItemType.Second:
					return RecordFilterTreeSearchScope.Second;
				default:
					Debug.Fail("Unrecognized type:" + mostSpecificType.Value.ToString());
					return RecordFilterTreeSearchScope.All;
			}
		}
		#endregion //GetMostSpecificScope

		#region GetScopeDisplayText
		private static string GetScopeDisplayText(RecordFilterTreeSearchScope scope)
		{
			string resourceName = string.Format("RecordFilterTreeSearchScope_{0}", scope);
			return DataPresenterBase.GetString(resourceName);
		}
		#endregion //GetScopeDisplayText

		#region InitializeTreeItems
		private void InitializeTreeItems()
		{
			if (null == _treeItemProvider || null == _valueLoader)
				return;

			// AS 8/24/11 TFS84897
			if (!_valueLoader.EndReached)
				return;

			_treeItemProvider.AddRange(_valueLoader.Phase2Items, true);

			if (_valueLoader.HasNullValues)
				_treeItemProvider.AddBlanks();

			// AS 8/15/11 TFS84221
			if (_valueLoader.EndReached && !this.HasSearchText)
				_treeItemProvider.ResetIsExpanded();
		}
		#endregion //InitializeTreeItems

		#region InitializeValueLoader
		private void InitializeValueLoader()
		{
			if (!this.IsInitialized)
				return;

			var valueLoader = _valueLoader;
			var filterInfo = valueLoader == null ? null : valueLoader.FilterInfo;
			var field = filterInfo == null ? null : filterInfo.Field;
			var rm = filterInfo == null ? null : filterInfo.RecordManager;

			_filterInfo = filterInfo;

			if (null != _treeItemProvider)
			{
				_treeItemProvider.Clear();
				_treeItemProvider.FilterInfo = _filterInfo;
			}

			if (null != _valueLoader)
			{
				valueLoader.Phase2Updated += new EventHandler(OnValueLoaderPhase2Updated);
				valueLoader.Phase2Completed += new EventHandler(OnValueLoaderPhase2Completed);

				if (valueLoader.Phase2Items == null)
					valueLoader.PopulatePhase2(false);
				else
					this.InitializeTreeItems();

				this.IsLoadingItems = !valueLoader.EndReached;
			}
			else
			{
				this.ClearValue(IsLoadingItemsPropertyKey);
			}

			this.Field = field;
			this.RecordManager = rm;
		} 
		#endregion //InitializeValueLoader 

		#region IsInValueTree
		private static bool IsInValueTree(TreeViewItem tvi)
		{
			ItemsControl ic = tvi;

			while (ic is TreeViewItem)
			{
				ic = ItemsControl.ItemsControlFromItemContainer(ic);
			}

			if (ic is TreeView == false)
				return false;

			RecordFilterTreeControl ctrl = ic.TemplatedParent as RecordFilterTreeControl;

			return ctrl != null && ic == ctrl._tree;
		}
		#endregion //IsInValueTree

		#region OnFilterInfoChanged
		private void OnFilterInfoChanged()
		{
			if (!this.IsInitialized)
				return;

			// release any existing objects
			_filterInfo = null;

			if (_valueLoader != null)
			{
				_valueLoader.Phase2Completed -= new EventHandler(OnValueLoaderPhase2Completed);
				_valueLoader.Phase2Updated -= new EventHandler(OnValueLoaderPhase2Updated);
				_valueLoader = null;
			}

			RecordManager rm = this.RecordManager;
			Field field = this.Field;

			if (null != _treeItemProvider)
				_treeItemProvider.Clear();

			if (null != field && null != rm)
			{
				_filterInfo = new ResolvedRecordFilterCollection.FieldFilterInfo(rm.RecordFiltersResolved, field);

				_valueLoader = new ResolvedRecordFilterCollection.FilterDropDownItemLoader(null, _filterInfo, null, false);
				_valueLoader.Phase2Updated += new EventHandler(OnValueLoaderPhase2Updated);
				_valueLoader.Phase2Completed += new EventHandler(OnValueLoaderPhase2Completed);

				if (_treeItemProvider != null)
					_treeItemProvider.FilterInfo = _filterInfo;

				_valueLoader.PopulatePhase1();

				
				_valueLoader.PopulatePhase2(false);
				this.IsLoadingItems = !_valueLoader.EndReached;
			}
			else
			{
				this.ClearValue(IsLoadingItemsPropertyKey);

				if (null != _treeItemProvider)
				{
					_treeItemProvider.Clear();
					_treeItemProvider.FilterInfo = null;
				}
			}
		}
		#endregion //OnFilterInfoChanged

		// AS 8/19/11 TFS84512
		#region OnSearchScopePreviewKeyDown
		private void OnSearchScopePreviewKeyDown(object sender, KeyEventArgs e)
		{
			Key key = e.Key;

			if (key == Key.Down && Keyboard.Modifiers == ModifierKeys.None && _searchScopeCombo.IsDropDownOpen == false)
			{
				_searchScopeCombo.IsDropDownOpen = true;
				e.Handled = true;
			}
		}
		#endregion //OnSearchScopePreviewKeyDown 

		#region OnSearchTextChanged
		/// <summary>
		/// Invoked when the SearchText property has been changed
		/// </summary>
		private void OnSearchTextChanged()
		{
			// stop the timer because if the user is typing we want to start a new delay
			this.StopSearchTextTimer();

			this.CoerceValue(HasSearchTextProperty);

			// if the text was reverted to the current text then we don't need the timer
			if (this.SearchText == this.TreeItemProvider.SearchText &&
				this.SearchScope == this.TreeItemProvider.SearchScope)
				return;

			// otherwise wait for the specified delay
			this.StartSearchTextTimer();
		} 
		#endregion //OnSearchTextChanged

		// AS 8/19/11 TFS84468
		#region OnSearchTextKeyDown
		private void OnSearchTextKeyDown(object sender, KeyEventArgs e)
		{
			var focusScope = FocusManager.GetFocusScope(this);
			var menu = focusScope as Menu;

			if (menu == null)
				return;

			TextBoxBase tb = sender as TextBoxBase;

			if (tb.AcceptsReturn)
				return;

			Key key = e.Key;

			if (key == Key.System)
				key = e.SystemKey;

			// excel seems to treat the up/down as next/previous. i'd rather do spatial 
			// navigation but someon used to navigating with the keyboard in excel would 
			// probably rather us be consistent
			if (key == Key.Up)
			{
				tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
			}
			else if (key == Key.Down)
			{
				tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			}
		} 
		#endregion //OnSearchTextKeyDown

		#region OnSearchTextTimerTick
		private void OnSearchTextTimerTick(object sender, EventArgs e)
		{
			this.ApplySearchText();
		} 
		#endregion //OnSearchTextTimerTick

		#region OnTreeItemProviderPropertyChanged
		private void OnTreeItemProviderPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "HasSearchText":
					this.SetValue(HasSearchTextPropertyKey, KnownBoxes.FromValue(this.TreeItemProvider.HasSearchText));

					// since the search text is updated asynchronously we need to dirty the canexecute state
					CommandManager.InvalidateRequerySuggested();
					break;
				case "IsEmptySearch":
					this.SetValue(HasEmptySearchResultsPropertyKey, KnownBoxes.FromValue(this.TreeItemProvider.IsEmptySearch));
					break;

				case "MostSpecificCreatedDateType":
					// verify the exposed collection of scopes
					this.VerifyAvailableScopes();

					// we may have coerced the scope to all
					this.CoerceValue(SearchScopeProperty);
					break;
			}
		}
		#endregion //OnTreeItemProviderPropertyChanged

		// AS 8/19/11 TFS84468
		#region OnTreePreviewGotFocus
		private void OnTreePreviewGotFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (e.NewFocus != sender)
				return;

			if (_tree.Items.Count == 0 || _tree.SelectedItem != null)
				return;

			if (e.OldFocus is DependencyObject == false || Utilities.IsDescendantOf(_tree, e.OldFocus as DependencyObject))
				return;

			// to mimic excel, we want focus to go to the 1st node in the tree. in wpf focus goes to the 
			// tree first and then to the 1st node on the subsequent tab press
			TreeViewItem item = _tree.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;

			if (null == item)
				return;

			if (item.Focus())
				e.Handled = true;
		}
		#endregion //OnTreePreviewGotFocus

		#region OnValueLoaderPhase2Completed
		private void OnValueLoaderPhase2Completed(object sender, EventArgs e)
		{
			this.ClearValue(IsLoadingItemsPropertyKey);

			this.InitializeTreeItems();
		} 
		#endregion //OnValueLoaderPhase2Completed

		#region OnValueLoaderPhase2Updated
		private void OnValueLoaderPhase2Updated(object sender, EventArgs e)
		{
			this.InitializeTreeItems();
		} 
		#endregion //OnValueLoaderPhase2Updated

		#region RepopulateItems
		private void RepopulateItems()
		{
			if (_treeItemProvider == null)
				return;

			// reset to the original state
			_treeItemProvider.Clear();

			// add any items
			this.InitializeTreeItems();

			// apply the search text if we have any and are not still loading items
			this.ApplySearchText();
		}
		#endregion //RepopulateItems

		#region StartSearchTextTimer
		private void StartSearchTextTimer()
		{
			// don't start the timer while waiting to be initialized. we'll apply in the initialized event
			if (this.IsInitialized == false)
				return;

			if (_searchTextTimer == null)
				_searchTextTimer = new DispatcherTimer(TimeSpan.FromSeconds(.25), DispatcherPriority.Render, new EventHandler(OnSearchTextTimerTick), this.Dispatcher);

			_searchTextTimer.Start();
		}
		#endregion //StartSearchTextTimer

		#region StopSearchTextTimer
		private void StopSearchTextTimer()
		{
			if (null != _searchTextTimer)
				_searchTextTimer.Stop();
		}
		#endregion //StopSearchTextTimer

		#region TransferFocus
		private bool TransferFocus()
		{
			// if we're in the root focus scope then we never need to transfer focus
			if (FocusManager.GetFocusScope(this) == null)
				return false;

			// if we don't have keyboard focus then we may still want to shift 
			// focus - e.g. if we're in a menu item and it has focus
			if (!this.IsKeyboardFocusWithin)
			{
				DependencyObject focusedObject = Keyboard.FocusedElement as DependencyObject;

				// if there is no keyboard focus or its in a different focus scope then leave keyboard focus alone
				if (null == focusedObject || FocusManager.GetFocusScope(focusedObject) != FocusManager.GetFocusScope(this))
					return false;

				// if its in another element in the focus scope and that isn't a direct ancestor then 
				// we should not assume that we should shift keyboard focus
				if (!Utilities.IsDescendantOf(focusedObject, this, false))
					return false;
			}

			// since this was designed primarily for use in a menu we want to shift focus
			// out of the menu when the ok/cancel are executed. this is similar to what 
			// a button does when it loses mouse capture and its in a different focus 
			// scope than the root focus scope
			Keyboard.Focus(null);
			return true;
		}
		#endregion //TransferFocus

		#region UpdateRecordFilter
		private void UpdateRecordFilter(object parameter)
		{
			if (this.CanUpdateRecordFilter(parameter))
			{
				_treeItemProvider.UpdateRecordFilter();

				if (!this.TransferFocus())
					this.CloseContainingMenu();
			}
		}
		#endregion //UpdateRecordFilter

		#region VerifyAvailableScopes
		private void VerifyAvailableScopes()
		{
			var availableScopes = this.AvailableSearchScopesInternal;

			RecordFilterTreeSearchScope scope = GetMostSpecificScope(this.TreeItemProvider.MostSpecificCreatedDateType);
			int neededCount = (int)scope + 1;

			if (neededCount == availableScopes.Count)
				return;

			availableScopes.BeginUpdate();

			// create any if we need more than we have
			for (int i = availableScopes.Count; i < neededCount; i++)
				availableScopes.Add(CreateScopeItem((RecordFilterTreeSearchScope)i));

			if (neededCount < availableScopes.Count)
				availableScopes.RemoveRange(neededCount, availableScopes.Count - neededCount);

			availableScopes.EndUpdate();

			// set the value that indicates that the combo should/should not be shown
			this.SetValue(HasMultipleSearchScopesPropertyKey, KnownBoxes.FromValue(neededCount > 1));

			// make sure its one of the available items
			this.CoerceValue(SearchScopeProperty);

			// the selected scope may not have changed but since the number has 
			// the search text may have changed
			this.VerifySearchTextPrompt();
		} 
		#endregion //VerifyAvailableScopes

		#region VerifySearchTextPrompt
		private void VerifySearchTextPrompt()
		{
			if (_availableScopesInternal == null || _availableScopesInternal.Count < 2)
				this.SetValue(SearchTextPromptPropertyKey, DataPresenterBase.GetString("RecordFilterTreeSearchTextPrompt"));
			else
				this.SetValue(SearchTextPromptPropertyKey, DataPresenterBase.GetString("RecordFilterTreeSearchTextPromptScoped", GetScopeDisplayText(this.SearchScope)));
		}
		#endregion //VerifySearchTextPrompt

		#endregion //Private Methods

		#endregion //Methods
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