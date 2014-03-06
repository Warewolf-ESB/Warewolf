using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Collections.ObjectModel;
using Infragistics.Collections;
using Infragistics.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows;
using System.ComponentModel;

// AS - NA 11.2 Excel Style Filtering
namespace Infragistics.Windows.DataPresenter
{
	#region RecordFilterTreeItemType enum
	// note currently the order is specific to how we will sort the types as well
	internal enum RecordFilterTreeItemType
	{
		/// <summary>
		/// Represents the item that when checked will select/unselect all the tree items.
		/// </summary>
		SelectAll,

		/// <summary>
		/// Represents the item that will select/unselect all the items when a filter is applied to the tree.
		/// </summary>
		SelectAllSearchResults,

		/// <summary>
		/// Used to indicate how the filtered changes will affect the existing filter. When unchecked the changes will replace the current filter. When checked, any checked items will be added to the filter and any unchecked item will be removed from the filter.
		/// </summary>
		AddCurrentSelectionToFilter,

		// note: the datetime ones are assumed to be in descending order altogether
		/// <summary>
		/// Represents the item that contains all the date based values.
		/// </summary>
		Dates,

		/// <summary>
		/// Represents the year portion of a DateTime
		/// </summary>
		Year,

		/// <summary>
		/// Represents the month portion of a DateTime
		/// </summary>
		Month,

		/// <summary>
		/// Represents the day portion of a DateTime
		/// </summary>
		Day,

		/// <summary>
		/// Represents the hour portion of a DateTime
		/// </summary>
		Hour,

		/// <summary>
		/// Represents the minute portion of a DateTime
		/// </summary>
		Minute,

		/// <summary>
		/// Represents the second portion of a DateTime
		/// </summary>
		Second,

		/// <summary>
		/// Represents the item that contains all the non-date based values.
		/// </summary>
		Values,

		/// <summary>
		/// Represents a single value.
		/// </summary>
		Value,

		/// <summary>
		/// Represents the Blanks special operand.
		/// </summary>
		Blanks,

	}
	#endregion //RecordFilterTreeItemType enum

	#region RecordFilterTreeItem
	/// <summary>
	/// Object that represents an item in the tree containing the list of unique values.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public class RecordFilterTreeItem : PropertyChangeNotifier
	{
		#region Member Variables

		private RecordFilterTreeItemType _itemType;
		private bool? _isChecked;
		private RecordFilterTreeItem _parent;
		private FilterDropDownItem _dropDownItem;

		#endregion //Member Variables

		#region Constructor
		internal RecordFilterTreeItem(RecordFilterTreeItemType itemType, FilterDropDownItem dropDownItem)
		{
			_itemType = itemType;
			_dropDownItem = dropDownItem;
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region DisplayText
		/// <summary>
		/// Returns the text used to describe the item.
		/// </summary>
		public string DisplayText
		{
			get { return this.DisplayTextInternal; }
		}
		#endregion //DisplayText

		#region IsChecked

		/// <summary>
		/// Returns or sets a boolean indicating if the item should be displayed as checked.
		/// </summary>
		public bool? IsChecked
		{
			get { return _isChecked; }
			set
			{
				this.InitializeIsChecked(value, true, true);
			}
		}
		#endregion //IsChecked

		#region IsExpanded

		/// <summary>
		/// Returns or sets a boolean indicating if the item should be expanded.
		/// </summary>
		public bool IsExpanded
		{
			get { return this.IsExpandedInternal; }
			set { this.IsExpandedInternal = value; }
		}
		#endregion //IsExpanded

		#region Items
		/// <summary>
		/// Returns a read-only collection of the child items.
		/// </summary>
		public ReadOnlyObservableCollection<RecordFilterTreeItem> Items
		{
			get { return this.ItemsInternal; }
		}
		#endregion //Items

		#endregion //Public Properties

		#region Internal Properties

		#region DisplayTextInternal
		internal virtual string DisplayTextInternal
		{
			get { return _dropDownItem.DisplayText; }
		}
		#endregion //DisplayTextInternal

		#region DropDownItem
		internal FilterDropDownItem DropDownItem
		{
			get { return _dropDownItem; }
		}
		#endregion //DropDownItem

		#region IsExpandedInternal
		internal virtual bool IsExpandedInternal
		{
			get { return false; }
			set { }
		}
		#endregion //IsExpandedInternal

		#region ItemsCount
		internal virtual int ItemsCount
		{
			get { return 0; }
		}
		#endregion //ItemsCount

		#region ItemsInternal
		internal virtual ReadOnlyObservableCollection<RecordFilterTreeItem> ItemsInternal
		{
			get { return null; }
		}
		#endregion //ItemsInternal

		#region ItemsSource
		internal virtual RecordFilterTreeItemCollection ItemsSource
		{
			get { return null; }
		}
		#endregion //ItemsSource

		#region ItemType
		internal RecordFilterTreeItemType ItemType
		{
			get { return _itemType; }
		}
		#endregion //ItemType

		#region Parent
		internal RecordFilterTreeItem Parent
		{
			get { return _parent; }
		}
		#endregion //Parent

		#region Value
		internal virtual object Value
		{
			get { return _dropDownItem.Value; }
		}
		#endregion //Value

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region CreateNew
		internal virtual RecordFilterTreeItem CreateNew()
		{
			return new RecordFilterTreeItem(_itemType, _dropDownItem);
		}
		#endregion //CreateNew

		#region InitializeIsChecked
		private void InitializeIsChecked(bool? newValue, bool notifyParents, bool notifyChildren)
		{
			if (newValue == _isChecked)
				return;

			bool? oldValue = _isChecked;
			_isChecked = newValue;

			var itemsSource = this.ItemsSource;

			if (notifyChildren && itemsSource != null)
			{
				Debug.Assert(null != newValue, "Should we be initializing the children to null?");

				foreach (RecordFilterTreeItem item in itemsSource)
					item.InitializeIsChecked(newValue, false, true);
			}

			if (null != _parent && notifyParents)
			{
				_parent.ItemsSource.OnChildIsCheckedChanged(this, oldValue, newValue);
			}

			// if we're changing the state to an explicit value
			if (null != newValue && itemsSource != null)
			{
				itemsSource.UpdateCheckedCount();
			}

			this.OnPropertyChanged("IsChecked");
		}
		#endregion //InitializeIsChecked

		#region IsAncestorOf
		internal bool IsAncestorOf(RecordFilterTreeItem item)
		{
			while (item != null)
			{
				if (item == this)
					return true;

				item = item._parent;
			}

			return false;
		}
		#endregion //IsAncestorOf

		#endregion //Methods

		#region RecordFilterTreeItemCollection class
		internal class RecordFilterTreeItemCollection : ObservableCollectionExtended<RecordFilterTreeItem>
		{
			#region Member Variables

			private RecordFilterTreeItem _parent;
			private int _countChecked;
			private int _countUnchecked;

			#endregion //Member Variables

			#region Constructor
			internal RecordFilterTreeItemCollection(RecordFilterTreeItem parent)
				: base(false, false)
			{
				Utilities.ValidateNotNull(parent);
				_parent = parent;
			}
			#endregion //Constructor

			#region Base class overrides

			#region ClearItems
			protected override void ClearItems()
			{
				if (this.Items.Count == 0)
					return;

				this.BeginUpdate();

				try
				{
					base.ClearItems();
				}
				finally
				{
					this.EndUpdate();
				}
			}
			#endregion //ClearItems

			#region OnEndUpdate
			protected override void OnEndUpdate()
			{
				base.OnEndUpdate();

				this.OnItemsChanged();
			}
			#endregion //OnEndUpdate

			#region NotifyItemsChanged
			protected override bool NotifyItemsChanged
			{
				get
				{
					return true;
				}
			}
			#endregion //NotifyItemsChanged

			#region OnItemAdding
			protected override void OnItemAdding(RecordFilterTreeItem itemAdded)
			{
				Debug.Assert(itemAdded != null, "Cannot provide a null item");
				Debug.Assert(itemAdded.Parent == null, "Item is already parented elsewhere");
				Debug.Assert(!itemAdded.IsAncestorOf(_parent), "This item is a descendant of the specified item.");
			}
			#endregion //OnItemAdding

			#region OnItemAdded
			protected override void OnItemAdded(RecordFilterTreeItem itemAdded)
			{
				if (itemAdded._isChecked == true)
					_countChecked++;
				else if (itemAdded._isChecked == false)
					_countUnchecked++;

				itemAdded._parent = _parent;

				if (!this.IsUpdating)
					this.OnItemsChanged();
			}
			#endregion //OnItemAdded

			#region OnItemRemoved
			protected override void OnItemRemoved(RecordFilterTreeItem itemRemoved)
			{
				if (itemRemoved._isChecked == true)
					_countChecked--;
				else if (itemRemoved._isChecked == false)
					_countUnchecked--;

				itemRemoved._parent = null;

				if (!this.IsUpdating)
					this.OnItemsChanged();
			}
			#endregion //OnItemRemoved

			#endregion //Base class overrides

			#region Methods

			#region Private Methods

			#region CalculateIsChecked
			private bool? CalculateIsChecked()
			{
				if (_countUnchecked == this.Count)
					return false;

				if (_countChecked == this.Count)
					return true;

				Debug.Assert(_countChecked + _countUnchecked <= this.Count, "Out of sync?");
				return null;
			}
			#endregion //CalculateIsChecked

			#region OnItemsChanged
			private void OnItemsChanged()
			{
				_parent.InitializeIsChecked(this.CalculateIsChecked(), true, false);
			}
			#endregion //OnItemsChanged

			#endregion //Private Methods

			#region Internal Methods

			#region InsertSorted
			internal void InsertSorted(RecordFilterTreeItem newItem, IComparer<RecordFilterTreeItem> comparer)
			{
				int index = this.BinarySearch(newItem, comparer);

				if (index < 0)
					index = ~index;

				this.Insert(index, newItem);
			} 
			#endregion //InsertSorted

			#region OnChildIsCheckedChanged
			internal void OnChildIsCheckedChanged(RecordFilterTreeItem child, bool? oldValue, bool? newValue)
			{
				if (oldValue == false)
					_countUnchecked--;
				else if (oldValue == true)
					_countChecked--;

				if (newValue == false)
					_countUnchecked++;
				else if (newValue == true)
					_countChecked++;

				Debug.Assert(_countUnchecked + _countChecked <= this.Count, "Out of sync?");

				bool? isChecked = this.CalculateIsChecked();

				_parent.InitializeIsChecked(isChecked, true, false);
			}
			#endregion //OnChildIsCheckedChanged

			#region UpdateCheckedCount
			internal void UpdateCheckedCount()
			{
				bool? isChecked = _parent._isChecked;

				if (isChecked == true)
				{
					_countChecked = this.Count;
					_countUnchecked = 0;
				}
				else if (isChecked == false)
				{
					_countUnchecked = this.Count;
					_countChecked = 0;
				}
			}
			#endregion //UpdateCheckedCount

			#endregion //Internal Methods

			#endregion //Methods
		}
		#endregion //RecordFilterTreeItemCollection class
	}
	#endregion //RecordFilterTreeItem

	#region RecordFilterTreeItemEx
	internal class RecordFilterTreeItemEx : RecordFilterTreeItem
	{
		#region Member Variables

		private string _displayText;
		private object _value;

		#endregion //Member Variables

		#region Constructor
		internal RecordFilterTreeItemEx(RecordFilterTreeItemType itemType, string displayText, object value, FilterDropDownItem dropDownItem)
			: base(itemType, dropDownItem)
		{
			_displayText = displayText;
			_value = value;
		}
		#endregion //Constructor

		#region Base class overrides

		#region CreateNew
		internal override RecordFilterTreeItem CreateNew()
		{
			return new RecordFilterTreeItemEx(this.ItemType, _displayText, _value, this.DropDownItem);
		}
		#endregion //CreateNew

		#region DisplayTextInternal
		internal override string DisplayTextInternal
		{
			get { return _displayText; }
		}
		#endregion //DisplayTextInternal

		#region Value
		internal override object Value
		{
			get { return _value; }
		}
		#endregion //Value

		#endregion //Base class overrides
	} 
	#endregion //RecordFilterTreeItemEx

	#region RecordFilterTreeItemParent
	internal class RecordFilterTreeItemParent : RecordFilterTreeItemEx
	{
		#region Member Variables

		private bool _isExpanded;
		private RecordFilterTreeItemCollection _itemsSource;
		private ReadOnlyObservableCollection<RecordFilterTreeItem> _items;

		#endregion //Member Variables

		#region Constructor
		internal RecordFilterTreeItemParent(RecordFilterTreeItemType itemType, string displayText, object value, FilterDropDownItem dropDownItem)
			: base(itemType, displayText, value, dropDownItem)
		{
			_itemsSource = new RecordFilterTreeItemCollection(this);

			switch (itemType)
			{
				case RecordFilterTreeItemType.Value:
				case RecordFilterTreeItemType.Second:
				case RecordFilterTreeItemType.AddCurrentSelectionToFilter:
				case RecordFilterTreeItemType.Blanks:
					Debug.Fail("Should not be creating this type for leaf item types!");
					break;

				case RecordFilterTreeItemType.SelectAll:
				case RecordFilterTreeItemType.SelectAllSearchResults:
					// avoid allocating a publicly exposed collection for the types that cannot use it
					break;
				default:
					_items = new ReadOnlyObservableCollection<RecordFilterTreeItem>(this.ItemsSource);
					break;
			}
		}
		#endregion //Constructor

		#region Base class overrides

		#region CreateNew
		internal override RecordFilterTreeItem CreateNew()
		{
			return new RecordFilterTreeItemParent(this.ItemType, this.DisplayTextInternal, this.Value, this.DropDownItem);
		}
		#endregion //CreateNew

		#region IsExpandedInternal
		internal override bool IsExpandedInternal
		{
			get
			{
				return _isExpanded;
			}
			set
			{
				if (_isExpanded != value)
				{
					_isExpanded = value;
					this.OnPropertyChanged("IsExpanded");
				}
			}
		}
		#endregion //IsExpandedInternal

		#region ItemsCount
		internal override int ItemsCount
		{
			get { return _itemsSource.Count; }
		}
		#endregion //ItemsCount

		#region ItemsInternal
		internal override ReadOnlyObservableCollection<RecordFilterTreeItem> ItemsInternal
		{
			get { return _items; }
		}
		#endregion //ItemsInternal

		#region ItemsSource
		internal override RecordFilterTreeItemCollection ItemsSource
		{
			get { return _itemsSource; }
		}
		#endregion //ItemsSource

		#endregion //Base class overrides
	} 
	#endregion //RecordFilterTreeItemParent

	#region RecordFilterTreeItemProvider
	internal class RecordFilterTreeItemProvider : PropertyChangeNotifier
		, IWeakEventListener
	{
		#region Member Variables

		private AggregateCollection<RecordFilterTreeItem> _allItems;

		private RecordFilterTreeItem _selectAllItem;
		private RecordFilterTreeItem _blanksItem;
		private RecordFilterTreeItem _datesItem;
		private RecordFilterTreeItem _valuesItem;
		private ObservableCollection<RecordFilterTreeItem> _suffixItems;

		private bool? _includeBlanks;
		private bool? _addedBlanks;
		private SpecialFilterOperandBase _blanksOperand;
		private bool _isGroupingDates = true;

		private string _hourFormat;
		private CultureInfo _culture;
		private RecordFilter _recordFilter;
		private ResolvedRecordFilterCollection.FieldFilterInfo _filterInfo;
		private FilterDropDownConditionEvaluationContext _evaluationContext;
		private bool _isUpdatingFilter;

		// search related items
		private RecordFilterTreeItem _selectAllSearchItem;
		private RecordFilterTreeItem _addToFilterItem;
		private RecordFilterTreeItem _searchBlanks;
		private Dictionary<RecordFilterTreeItem, RecordFilterTreeItem> _searchToRootTable;
		private string _searchText;
		private RecordFilterTreeSearchScope _searchScope;
		private RecordFilterTreeItemType? _mostSpecificCreatedDateType;

		#endregion //Member Variables

		#region Constructor
		internal RecordFilterTreeItemProvider(bool? includeBlanks)
		{
			_culture = CultureInfo.CurrentCulture;
			_includeBlanks = includeBlanks;

			_selectAllItem = new RecordFilterTreeItemParent(RecordFilterTreeItemType.SelectAll, DataPresenterBase.GetString("RecordFilterTreeItem_SelectAll"), null, null);
			_valuesItem = new RecordFilterTreeItemParent(RecordFilterTreeItemType.Values, "Values", null, null);
			_datesItem = new RecordFilterTreeItemParent(RecordFilterTreeItemType.Dates, "Dates", null, null);
			_suffixItems = new ObservableCollection<RecordFilterTreeItem>();

			// get the blanks operand
			_blanksOperand = SpecialFilterOperands.Blanks;

			this.Initialize();
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region AllItems
		/// <summary>
		/// Returns a collection of the tree items that should be displayed to the end user.
		/// </summary>
		public IEnumerable<RecordFilterTreeItem> AllItems
		{
			get
			{
				if (_allItems == null)
				{
					_allItems = new AggregateCollection<RecordFilterTreeItem>();

					this.InitializeAllItems();
				}

				return _allItems;
			}
		}
		#endregion //AllItems

		#region HasSearchText
		/// <summary>
		/// Returns a boolean indicating if there is search criteria applied.
		/// </summary>
		public bool HasSearchText
		{
			get
			{
				return !string.IsNullOrEmpty(_searchText);
			}
		}
		#endregion //HasSearchText

		#region IsEmptySearch
		/// <summary>
		/// Returns a boolean indicating if there is a search that has results in no matching items.
		/// </summary>
		public bool IsEmptySearch
		{
			get
			{
				return this.HasSearchText && (_selectAllSearchItem == null || _selectAllSearchItem.ItemsSource.Count == 0);
			}
		} 
		#endregion //IsEmptySearch

		#region IsGroupingDates
		/// <summary>
		/// Determines if the DateTime values are represented as dates.
		/// </summary>
		public bool IsGroupingDates
		{
			get { return _isGroupingDates; }
			set
			{
				if (value != _isGroupingDates)
				{
					_isGroupingDates = value;

					this.Clear();

					this.OnPropertyChanged("IsGroupingDates");
				}
			}
		} 
		#endregion //IsGroupingDates

		#region MostSpecificCreatedDateType
		/// <summary>
		/// Returns the lowest level type of date node that has been created.
		/// </summary>
		public RecordFilterTreeItemType? MostSpecificCreatedDateType
		{
			get { return _mostSpecificCreatedDateType; }
		}
		#endregion //MostSpecificCreatedDateType

		#region SearchScope
		/// <summary>
		/// Returns the current scope used when performing a search via the SearchText.
		/// </summary>
		public RecordFilterTreeSearchScope SearchScope
		{
			get { return _searchScope; }
		} 
		#endregion //SearchScope

		#region SearchText
		/// <summary>
		/// Returns or sets the string used to filter the list of available values.
		/// </summary>
		public string SearchText
		{
			get { return _searchText; }
		}
		#endregion //SearchText

		#endregion //Public Properties

		#region Internal Properties

		#region CanUpdateRecordFilter
		internal bool CanUpdateRecordFilter
		{
			get
			{
				// if there is no search criteria then we're using the default 
				// items in which case we need at least 1 checked item
				if (!this.HasSearchText)
					return _selectAllItem.IsChecked != false;

				// if we have search criteria but no matching items then we cannot commit
				if (this.IsEmptySearch)
					return false;

				// if we have some search text and there is at least 
				// one item checked then we can update
				if (_selectAllSearchItem.IsChecked != false)
					return true;

				// when updating the existing filter excel always allows
				return _addToFilterItem.IsChecked != false;
			}
		} 
		#endregion //CanUpdateRecordFilter

		#region FilterInfo
		internal ResolvedRecordFilterCollection.FieldFilterInfo FilterInfo
		{
			get { return _filterInfo; }
			set
			{
				if (value != _filterInfo)
				{
					if (null != _filterInfo)
						PropertyChangedEventManager.RemoveListener(_filterInfo, this, string.Empty);

					_filterInfo = value;

					if (null != _filterInfo)
						PropertyChangedEventManager.AddListener(_filterInfo, this, string.Empty);

					this.OnFilterInfoChanged();
				}
			}
		} 
		#endregion //FilterInfo

		#region IsUpdatingFilter
		internal bool IsUpdatingFilter
		{
			get { return _isUpdatingFilter; }
		} 
		#endregion //IsUpdatingFilter

		#endregion //Internal Properties

		#region Private Properties

		#region Culture
		private CultureInfo Culture
		{
			get { return _culture; }
			set
			{
				if (value != _culture)
				{
					_culture = value;
					_hourFormat = null;
				}
			}
		}
		#endregion //Culture 

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region SetSearchCriteria
		/// <summary>
		/// Changes the search text and scope in a single operation.
		/// </summary>
		/// <param name="searchText">The new text to use when evaluating the nodes</param>
		/// <param name="scope">The type of nodes to be evaluated</param>
		/// <param name="force">True to re-apply the search criteria even if the values match the current criteria</param>
		public void SetSearchCriteria(string searchText, RecordFilterTreeSearchScope scope, bool force)
		{
			bool hasTextChanged = _searchText != searchText;
			bool hasScopeChanged = _searchScope != scope;

			if (!force && !hasTextChanged && !hasScopeChanged)
				return;

			bool hadSearch = this.HasSearchText;
			bool wasEmpty = this.IsEmptySearch;

			_searchText = searchText;
			_searchScope = scope;

			// if we had search text or now have it refresh the search (i.e.
			// don't if the scope changed but we don't have search text)
			if (hasTextChanged || HasSearchText)
				this.OnSearchTextChanged();

			if (hasTextChanged)
				this.OnPropertyChanged("SearchText");

			if (hasScopeChanged)
				this.OnPropertyChanged("SearchScope");

			if (hadSearch != this.HasSearchText)
				this.OnPropertyChanged("HasSearchText");

			if (wasEmpty != this.IsEmptySearch)
				this.OnPropertyChanged("IsEmptySearch");
		}
		#endregion //SetSearchCriteria

		#endregion //Public Methods

		#region Internal Methods

		#region AddBlanks
		internal void AddBlanks()
		{
			if (_includeBlanks == null && _addedBlanks == null)
				this.AddBlanksItem();
		} 
		#endregion //AddBlanks

		#region AddRange
		internal void AddRange(IList<FilterDropDownItem> items, bool clearValues)
		{
			var oldMostSpecificType = _mostSpecificCreatedDateType;

			// begin update on the overall collection
			_valuesItem.ItemsSource.BeginUpdate();
			_datesItem.ItemsSource.BeginUpdate();

			try
			{
				Dictionary<FilterDropDownItem, RecordFilterTreeItem> oldItems = null;

				// for dates the order is determined by us but for values the order should match 
				// what was provided by the sorting of the filterdropdown items so we need to 
				// clear the old values out. since we've already created the tree items we'll 
				// cache them and reuse them
				if (clearValues && _valuesItem.ItemsCount > 0)
				{
					oldItems = new Dictionary<FilterDropDownItem, RecordFilterTreeItem>();

					foreach (var item in _valuesItem.ItemsSource)
						oldItems[item.DropDownItem] = item;

					_valuesItem.ItemsSource.Clear();
				}

				if (items != null)
				{
					foreach (var item in items)
					{
						RecordFilterTreeItem treeItem;

						if (null == oldItems || !oldItems.TryGetValue(item, out treeItem))
						{
							this.AddItem(item);
						}
						else
						{
							treeItem.IsChecked = IsOriginallyChecked(item);
							_valuesItem.ItemsSource.Add(treeItem);
						}
					}
				}
				else if (clearValues)
				{
					_mostSpecificCreatedDateType = null;
				}
			}
			finally
			{
				_valuesItem.ItemsSource.EndUpdate();
				_datesItem.ItemsSource.EndUpdate();
			}

			if (oldMostSpecificType != _mostSpecificCreatedDateType)
			{
				// if we now have seconds then make a pass over any day date items
				// expanding them to have hours/minutes/seconds. this is what excel 
				// does and presumably it is so that searching may now be scoped for 
				// hour/minute/second
				if (_mostSpecificCreatedDateType == RecordFilterTreeItemType.Second)
				{
					this.EnsureDayNodesHaveSeconds();
				}

				this.OnPropertyChanged("MostSpecificCreatedDateType");
			}

			if (this.HasSearchText)
			{
				this.SetSearchCriteria(_searchText, _searchScope, true);
			}
		}
		#endregion //AddRange

		#region Clear
		internal void Clear()
		{
			bool hasSpecificCreatedType = _mostSpecificCreatedDateType != null;
			bool wasEmptySearch = this.IsEmptySearch;

			_selectAllItem.ItemsSource.Clear();
			_datesItem.ItemsSource.Clear();
			_valuesItem.ItemsSource.Clear();
			_suffixItems.Clear();
			_addedBlanks = null;
			_mostSpecificCreatedDateType = null;

			this.Initialize();

			if (wasEmptySearch != this.IsEmptySearch)
				this.OnPropertyChanged("IsEmptySearch");

			if (hasSpecificCreatedType)
				this.OnPropertyChanged("MostSpecificCreatedDateType");
		} 
		#endregion //Clear

		// AS 8/15/11 TFS84221
		#region ResetIsExpanded
		internal void ResetIsExpanded()
		{
			if (this.HasSearchText)
				return;

			if (_datesItem == null || _datesItem.ItemsCount == 0)
				return;

			int yearCount = _datesItem.ItemsCount;

			// expand year if only 1
			bool expandYears = yearCount == 1;

			for (int i = 0; i < yearCount; i++)
			{
				var yearItem = _datesItem.Items[i];
				
				yearItem.IsExpanded = expandYears;

				int monthCount = yearItem.ItemsCount;
				// expand month if only 1
				bool expandMonth = expandYears && monthCount == 1;
				for (int j = 0; j < monthCount; j++)
				{
					yearItem.Items[j].IsExpanded = expandMonth;
				}
			}
		}
		#endregion //ResetIsExpanded

		#region ResetIsChecked
		internal void ResetIsChecked()
		{
			this.InitializeIsChecked();
		}
		#endregion //ResetIsChecked

		#region UpdateRecordFilter
		internal void UpdateRecordFilter()
		{
			RecordFilter recordFilter = _recordFilter;

			Debug.Assert(recordFilter != null, "RecordFilter has not been set");

			if (recordFilter == null)
				return;

			// propogate the changes from the search to the 
			if (this.HasSearchText && this.CanUpdateRecordFilter)
			{
				// if we're not adding to the existing filter then clear the ischecked
				// of the root node before we copy over the state of the filtered nodes
				if (_addToFilterItem != null && _addToFilterItem.IsChecked == false)
				{
					_selectAllItem.IsChecked = false;
				}
				else
				{
					// otherwise if we will be "adding" to the existing filter then ensure 
					// that the unfiltered nodes have their ischecked state set to the 
					// original values
					this.InitializeIsChecked();
				}

				// then copy over the state of the search nodes to the original nodes
				this.UpdateRootNodeFromSearchNodes();
			}

			if (!this.CanUpdateRecordFilter)
			{
				Debug.Fail("We cannot perform an item at this point.");
				return;
			}

			ConditionGroup group = new ConditionGroup();
			UpdateRecordFilter(group);

			if (group.Count == 1 && group[0] is ComparisonCondition)
			{
				ComparisonCondition cc = group[0] as ComparisonCondition;

				recordFilter.CurrentUIOperator = cc.Operator;
				recordFilter.CurrentUIOperand = cc.Value;
			}
			else
			{
				recordFilter.CurrentUIOperator = ComparisonOperator.Equals;
				recordFilter.CurrentUIOperand = group;
			}

			bool wasUpdatingFilter = _isUpdatingFilter;
			_isUpdatingFilter = true;

			try
			{
				recordFilter.ApplyPendingFilter(true, true);
			}
			finally
			{
				_isUpdatingFilter = wasUpdatingFilter;
			}
		}
		#endregion //UpdateRecordFilter

		#endregion //Internal Methods

		#region Private Methods

		#region AddBlanksItem
		private void AddBlanksItem()
		{
			Debug.Assert(_addedBlanks == null);

			_addedBlanks = true;

			if (_blanksItem == null)
			{
				string displayText = _blanksOperand == null ? "(Blanks)" : _blanksOperand.ToString();
				RecordFilterTreeItem newItem = new RecordFilterTreeItemEx(RecordFilterTreeItemType.Blanks, displayText, null, null);
				newItem.IsChecked = this.IsOriginallyChecked(null);
				_blanksItem = newItem;
			}

			_selectAllItem.ItemsSource.Add(_blanksItem);
			_suffixItems.Add(_blanksItem);
		}
		#endregion //AddBlanksItem

		#region AddDate
		private void AddDate(DateTime date, RecordFilterTreeItem parent, FilterDropDownItem filterItem)
		{
			Debug.Assert(parent is RecordFilterTreeItemParent, "Expected parent");

			RecordFilterTreeItemType childType = parent.ItemType;
			childType++;

			int nextValue;

			switch (childType)
			{
				case RecordFilterTreeItemType.Year:
					nextValue = date.Year;
					break;
				case RecordFilterTreeItemType.Month:
					nextValue = date.Month;
					break;
				case RecordFilterTreeItemType.Day:
					nextValue = date.Day;
					break;
				case RecordFilterTreeItemType.Hour:
					nextValue = date.Hour;
					break;
				case RecordFilterTreeItemType.Minute:
					nextValue = date.Minute;
					break;
				case RecordFilterTreeItemType.Second:
					nextValue = date.Second;
					break;
				default:
					Debug.Fail(string.Format("Unexpected type - parent={0}, child={1}", parent.ItemType, childType));
					return;
			}

			RecordFilterTreeItem childItem = null;

			int childIndex = BinarySearchDateItem(parent, nextValue);

			if (childIndex >= 0)
				childItem = parent.ItemsSource[childIndex];
			else
				childIndex = ~childIndex;

			bool addItem = childItem == null;

			// if we haven't encountered this item yet then create it now
			if (addItem)
			{
				string format = this.GetDateValueFormat(childType);
				bool isLeafItem;

				if (childType == RecordFilterTreeItemType.Second)
					isLeafItem = true;
				// if its a date without time and we don't have seconds for any node yet then use a leaf item
				else if (childType == RecordFilterTreeItemType.Day && date.TimeOfDay.Ticks == 0 && _mostSpecificCreatedDateType != RecordFilterTreeItemType.Second)
					isLeafItem = true;
				else
					isLeafItem = false;

				if (isLeafItem)
					childItem = new RecordFilterTreeItemEx(childType, date.ToString(format, _culture), nextValue, filterItem);
				else
					childItem = new RecordFilterTreeItemParent(childType, date.ToString(format, _culture), nextValue, filterItem);

				if (_mostSpecificCreatedDateType == null || childType > _mostSpecificCreatedDateType)
					_mostSpecificCreatedDateType = childType;
			}

			try
			{
				// we may or may not need to create children for a day (i.e. include the time portion)
				if (childType == RecordFilterTreeItemType.Day)
				{
					// if the item has no children then we have some extra considerations
					if (childItem.ItemsSource == null)
					{
						// if this item doesn't have children and doesn't have time then 
						// it doesn't need children yet so we can just exit
						if (date.TimeOfDay.Ticks == 0 && _mostSpecificCreatedDateType == RecordFilterTreeItemType.Day)
						{
							if (addItem)
								childItem.IsChecked = IsOriginallyChecked(filterItem);

							return;
						}
						else if (!addItem)
						{
							// we only had a day as the leaf but since we know have time we need 
							// to convert the item to have children representing 12 midnight
							childItem = ConvertLeafDayToSecond(date, parent, childItem, childIndex);
						}
					}
				}
				else if (childType == RecordFilterTreeItemType.Second)
				{
					// if we just created the seconds then we're done
					if (addItem)
						childItem.IsChecked = IsOriginallyChecked(filterItem);

					return;
				}

				// otherwise add the descendant items for this date
				AddDate(date, childItem, filterItem);
			}
			finally
			{
				// to avoid unchecking the ancestors as each node is added we'll add the children
				// first to an item before we add the item to the parent
				if (addItem)
				{
					if (childIndex < 0)
						childIndex = parent.ItemsSource.Count;

					parent.ItemsSource.Insert(childIndex, childItem);
				}
			}
		}
		#endregion //AddDate

		#region AddItem
		private void AddItem(FilterDropDownItem item)
		{
			Debug.Assert(item.IsCellValue, "Only expecting to deal with cell values!");

			object cellValue = item.Value;

			// if adding of blanks is dependant on whether there is a blank value then 
			// check the value as it comes in. for simplicity we're keeping it at the 
			// top of the list
			if (_includeBlanks == null && _blanksOperand.IsMatch(ComparisonOperator.Equals, cellValue, null))
			{
				if (_addedBlanks == null)
					this.AddBlanksItem();

				return;
			}

			if (cellValue is DateTime == false || _isGroupingDates == false)
			{
				if (_valuesItem.Parent == null)
					_selectAllItem.ItemsSource.InsertSorted(_valuesItem, Utilities.CreateComparer<RecordFilterTreeItem>(CompareByItems));

				RecordFilterTreeItem newItem = new RecordFilterTreeItem(RecordFilterTreeItemType.Value, item);
				newItem.IsChecked = IsOriginallyChecked(item);

				_valuesItem.ItemsSource.Add(newItem);
			}
			else
			{
				if (_datesItem.Parent == null)
					_selectAllItem.ItemsSource.InsertSorted(_datesItem, Utilities.CreateComparer<RecordFilterTreeItem>(CompareByItems));

				// deal with date parts
				DateTime date = (DateTime)cellValue;
				this.AddDate(date, _datesItem, item);
			}
		}
		#endregion //AddItem

		#region BinarySearchDateItem
		private int BinarySearchDateItem(RecordFilterTreeItem parent, int nextValue)
		{
			if (parent.ItemType == RecordFilterTreeItemType.Dates)
				return BinarySearchDateItem(parent.ItemsSource, nextValue, CompareDescendingDates);
			else
				return BinarySearchDateItem(parent.ItemsSource, nextValue, CompareAscendingDates);
		}

		private int BinarySearchDateItem(IList<RecordFilterTreeItem> items, int value, Comparison<int> comparer)
		{
			// years are in descending order
			int si = 0, ei = items.Count - 1;
			int mi = 0;

			while (si <= ei)
			{
				mi = (si + ei) / 2;

				RecordFilterTreeItem item = items[mi];
				int itemValue = (int)item.Value;

				int result = comparer(itemValue, value);

				if (result > 0)
					ei = mi - 1;
				else if (result < 0)
					si = mi + 1;
				else
					return mi;
			}

			return ~si;
		}
		#endregion //BinarySearchDateItem

		#region CloneMatchingDateItem
		internal void CloneMatchingDateItem(Predicate<string> isMatchCallback, RecordFilterTreeItemType? typeToMatch, IList<RecordFilterTreeItem> list)
		{
			foreach (var item in _datesItem.ItemsSource)
			{
				bool isMatch;
				var clone = CloneMatchingDateItem(item, isMatchCallback, typeToMatch, false, out isMatch);

				if (null != clone)
				{
					// check the item and its descendants
					clone.IsChecked = true;

					// add it to the subsearch
					list.Add(clone);

					_searchToRootTable[clone] = item;
				}
			}
		}

		internal RecordFilterTreeItem CloneMatchingDateItem(RecordFilterTreeItem item, Predicate<String> isMatchCallback, RecordFilterTreeItemType? typeToMatch, bool forceClone, out bool isMatch)
		{
			RecordFilterTreeItem clone = null;

			// assume this is not a match
			isMatch = false;

			// a condition won't be specified when we don't need to compare
			// such as when we are matching on month (found a match) and just 
			// want to clone the descendants
			if (typeToMatch == null || typeToMatch.Value == item.ItemType)
			{
				isMatch = isMatchCallback(item.DisplayTextInternal);

				if (typeToMatch != null)
				{
					// if we're matching a specific scope then exit if this doesn't match
					if (isMatch == false)
					{
						Debug.Assert(!forceClone, "Why are we forcing here?");
						return null;
					}
				}

				// if this matches then we want to return it whether it has children or not and if it has children
				// then we want to include them whether they match or not
				if (isMatch)
					forceClone = true;
			}

			if (forceClone)
			{
				clone = item.CreateNew();
			}

			if (item.Items != null)
			{
				bool hasChildMatch = false;

				foreach (var child in item.ItemsSource)
				{
					bool isChildMatch = false;
					var childClone = CloneMatchingDateItem(child, isMatchCallback, typeToMatch, isMatch || forceClone, out isChildMatch);

					if (null == childClone)
						continue;

					// if we haven't created the clone yet do so now
					if (clone == null)
						clone = item.CreateNew();

					// if any child matched then let the caller know. it will want to expand itself to show the descendant
					if (isChildMatch)
					{
						hasChildMatch = true;
						clone.IsExpandedInternal = true;
					}

					_searchToRootTable[childClone] = child;

					clone.ItemsSource.Add(childClone);
				}

				if (hasChildMatch)
					isMatch = true;
			}

			return clone;
		}
		#endregion //CloneMatchingDateItem

		#region CompareDates
		private int CompareDescendingDates(int itemYear, int newYearValue)
		{
			return newYearValue.CompareTo(itemYear);
		}

		private int CompareAscendingDates(int itemYear, int newYearValue)
		{
			return itemYear.CompareTo(newYearValue);
		}
		#endregion //CompareDates

		#region CompareByItems
		private static int CompareByItems(RecordFilterTreeItem x, RecordFilterTreeItem y)
		{
			if (x == y)
				return 0;
			else if (x == null)
				return -1;
			else if (y == null)
				return 1;

			return x.ItemType.CompareTo(y.ItemType);
		}
		#endregion //CompareByItems

		#region ConvertLeafDayToSecond
		private RecordFilterTreeItem ConvertLeafDayToSecond(DateTime date, RecordFilterTreeItem parent, RecordFilterTreeItem childItem, int childIndex)
		{
			// we now have one with time so we need a parent item instead. replace 
			// the child leaf item with a parent item
			parent.ItemsSource[childIndex] = childItem = new RecordFilterTreeItemParent(RecordFilterTreeItemType.Day, childItem.DisplayText, childItem.Value, childItem.DropDownItem);

			// if this has time and is an existing item without 
			// children (i.e. its just a date only) then add the 
			// descendants for 12:00 am since we must have had 
			// that already to have created the day
			AddDate(date.Date, childItem, childItem.DropDownItem);
			return childItem;
		}
		#endregion //ConvertLeafDayToSecond

		#region EnsureDayNodesHaveSeconds
		private void EnsureDayNodesHaveSeconds()
		{
			for (int y = 0, yCount = _datesItem.ItemsCount; y < yCount; y++)
			{
				var year = _datesItem.ItemsSource[y];

				for (int m = 0, mCount = year.ItemsCount; m < mCount; m++)
				{
					var month = year.ItemsSource[m];

					for (int d = 0, dCount = month.ItemsCount; d < dCount; d++)
					{
						var day = month.ItemsSource[d];

						// if this is a leaf item (i.e. it doesn't have hours/minutes/seconds)...
						if (day.ItemsSource == null)
						{
							DateTime date = (DateTime)day.DropDownItem.Value;
							this.ConvertLeafDayToSecond(date, month, day, d);
						}
					}
				}
			}
		}
		#endregion //EnsureDayNodesHaveSeconds

		#region GetDateRangeScope
		private static DateRangeScope GetDateRangeScope(RecordFilterTreeItemType itemType)
		{
			switch (itemType)
			{
				case RecordFilterTreeItemType.Year:
					return DateRangeScope.Year;
				case RecordFilterTreeItemType.Month:
					return DateRangeScope.Month;
				case RecordFilterTreeItemType.Day:
					return DateRangeScope.Day;
				case RecordFilterTreeItemType.Hour:
					return DateRangeScope.Hour;
				case RecordFilterTreeItemType.Minute:
					return DateRangeScope.Minute;
				case RecordFilterTreeItemType.Second:
					return DateRangeScope.Second;
				default:
					Debug.Fail("Unexpected item type:" + itemType);
					return DateRangeScope.Year;
			}
		}
		#endregion //GetDateRangeScope

		#region GetDateScope
		private static RecordFilterTreeItemType? GetDateScope(RecordFilterTreeSearchScope scope)
		{
			switch (scope)
			{
				default:
				case RecordFilterTreeSearchScope.All:
					Debug.Assert(scope == RecordFilterTreeSearchScope.All, "Unexpected scope value:" + scope.ToString());
					return null;
				case RecordFilterTreeSearchScope.Year:
					return RecordFilterTreeItemType.Year;
				case RecordFilterTreeSearchScope.Month:
					return RecordFilterTreeItemType.Month;
				case RecordFilterTreeSearchScope.Day:
					return RecordFilterTreeItemType.Day;
				case RecordFilterTreeSearchScope.Hour:
					return RecordFilterTreeItemType.Hour;
				case RecordFilterTreeSearchScope.Minute:
					return RecordFilterTreeItemType.Minute;
				case RecordFilterTreeSearchScope.Second:
					return RecordFilterTreeItemType.Second;
			}
		}
		#endregion //GetDateScope 

		#region GetDateValueFormat
		private string GetDateValueFormat(RecordFilterTreeItemType childType)
		{
			switch (childType)
			{
				case RecordFilterTreeItemType.Day:
					return "dd";
				case RecordFilterTreeItemType.Second:
					return ":ss";
				case RecordFilterTreeItemType.Minute:
					return ":mm";
				case RecordFilterTreeItemType.Hour:
					{
						if (_hourFormat == null)
						{
							if (_culture.DateTimeFormat.LongTimePattern.Contains("H"))
								_hourFormat = "HH";
							else
								_hourFormat = "hh tt";
						}

						return _hourFormat;
					}
				case RecordFilterTreeItemType.Month:
					return "MMMM";
				case RecordFilterTreeItemType.Year:
					return "yyyy";
				default:
					Debug.Fail("Unexpected item type:" + childType.ToString());
					return "G";
			}
		}
		#endregion //GetDateValueFormat

		#region Initialize
		private void Initialize()
		{
			_selectAllItem.IsChecked = false;

			if (_blanksOperand == null)
				_addedBlanks = false;
			else if (_includeBlanks == true)
				this.AddBlanksItem();
			else
				_addedBlanks = _includeBlanks;
		}
		#endregion //Initialize

		#region InitializeAllItems
		private void InitializeAllItems()
		{
			if (_allItems == null)
				return;

			_allItems.Collections = new IList<RecordFilterTreeItem>[]
			{
				new RecordFilterTreeItem[] { _selectAllItem }
				, _datesItem.ItemsSource
				, _valuesItem.ItemsSource
				, _suffixItems
			};
		}
		#endregion //InitializeAllItems

		#region InitializeIsChecked
		private void InitializeIsChecked()
		{
			if (_recordFilter == null || _recordFilter.Conditions.Count == 0)
				_selectAllItem.IsChecked = true;
			else
			{
				if (null != _blanksItem)
					_blanksItem.IsChecked = this.IsOriginallyChecked(null);

				foreach (RecordFilterTreeItem item in _valuesItem.ItemsSource)
					item.IsChecked = this.IsOriginallyChecked(item.DropDownItem);

				foreach (RecordFilterTreeItem item in _datesItem.ItemsSource)
					InitializeIsChecked(item);
			}
		}

		private void InitializeIsChecked(RecordFilterTreeItem item)
		{
			if (item.ItemsCount == 0)
			{
				Debug.Assert(null != item.DropDownItem, "No drop down item associated with value?");
				item.IsChecked = IsOriginallyChecked(item.DropDownItem);
			}
			else
			{
				foreach (RecordFilterTreeItem child in item.Items)
				{
					InitializeIsChecked(child);
				}
			}
		}
		#endregion //InitializeIsChecked

		#region IsOriginallyChecked
		private bool IsOriginallyChecked(FilterDropDownItem dropDownItem)
		{
			if (_recordFilter == null || _recordFilter.Conditions.Count == 0)
				return true;

			if (_evaluationContext == null)
			{
				IEnumerable<DataRecord> allValuesRecords = GridUtilities.Filter<DataRecord>(_filterInfo.RecordManager.Unsorted, new GridUtilities.MeetsCriteria_RecordVisible(true));
				_evaluationContext = new FilterDropDownConditionEvaluationContext(_filterInfo.Field, allValuesRecords);
			}

			// when matching a null/blank entry create an item
			if (dropDownItem == null)
				dropDownItem = new FilterDropDownItem(null, string.Empty, true);

			_evaluationContext.Initialize(dropDownItem);

			return _recordFilter.Conditions.IsMatch(dropDownItem.Value, _evaluationContext);
		}
		#endregion //IsOriginallyChecked

		#region OnFilterInfoChanged
		private void OnFilterInfoChanged()
		{
			bool wasEmpty = this.IsEmptySearch;

			// the record filter may have changed
			_recordFilter = _filterInfo == null ? null : _filterInfo.RecordFilter;

			// clear the context
			_evaluationContext = null;

			// use the culture for the field
			Field field = _filterInfo != null ? _filterInfo.Field : null;
			this.Culture = field == null ? CultureInfo.CurrentCulture : GridUtilities.GetDefaultCulture(field);

			this.InitializeIsChecked();

			if (this.HasSearchText)
			{
				this.OnSearchTextChanged();

				if (wasEmpty != this.IsEmptySearch)
					this.OnPropertyChanged("IsEmptySearch");
			}
		}
		#endregion //OnFilterInfoChanged

		#region OnSearchTextChanged
		private void OnSearchTextChanged()
		{
			Debug.Assert(_filterInfo != null, "Need the filter into to get the evaluation context");

			if (_filterInfo == null)
				return;

			// make sure the collection is allocated
			var allItems = this.AllItems;

			if (!this.HasSearchText)
			{
				#region No Search Text

				if (_selectAllSearchItem != null)
				{
					_selectAllSearchItem.ItemsSource.Clear();
				}

				// update the AllItems collection
				this.InitializeAllItems();

				// when the filter is cleared then restore the state of this check box. we don't want to 
				// do it as the user is typing into the filter text box
				if (null != _addToFilterItem)
					_addToFilterItem.IsChecked = false;

				// when the search is cleared excel sets the ischecked state back to the original
				this.InitializeIsChecked(); 

				#endregion //No Search Text
			}
			else
			{
				if (_searchToRootTable == null)
					_searchToRootTable = new Dictionary<RecordFilterTreeItem, RecordFilterTreeItem>();
				else
					_searchToRootTable.Clear();

				if (null == _selectAllSearchItem)
					_selectAllSearchItem = new RecordFilterTreeItemParent(RecordFilterTreeItemType.SelectAllSearchResults, DataPresenterBase.GetString("RecordFilterTreeItem_SelectAllSearch"), null, null);

				try
				{
					_selectAllSearchItem.ItemsSource.BeginUpdate();

					if (null == _addToFilterItem)
					{
						_addToFilterItem = new RecordFilterTreeItemEx(RecordFilterTreeItemType.AddCurrentSelectionToFilter, DataPresenterBase.GetString("RecordFilterTreeItem_AddToFilter"), null, null);
						_addToFilterItem.IsChecked = false;
					}

					// we'll use a differnt blanks item since the _blanksItem is parented into the main select all
					if (_blanksItem != null && _addedBlanks == true)
					{
						_searchBlanks = _blanksItem.CreateNew();
					}

					Predicate<string> isMatchCallback = SearchHelper.CreateMatchCallback(_searchText);

					List<RecordFilterTreeItem> items = new List<RecordFilterTreeItem>();

					RecordFilterTreeItemType? dateScope = GetDateScope(_searchScope);
					CloneMatchingDateItem(isMatchCallback, dateScope, items);

					// all is obvious but we're taking year as well since that is what excel does. i can only 
					// assume that that allows someone who has a mixed set of dates and flat values to search 
					// for text without worry about if that text is in a month name, etc.
					if (_searchScope == RecordFilterTreeSearchScope.All || _searchScope == RecordFilterTreeSearchScope.Year)
					{
						foreach (var valueItem in _valuesItem.ItemsSource)
						{
							if (isMatchCallback(valueItem.DisplayTextInternal))
							{
								var item = new RecordFilterTreeItem(RecordFilterTreeItemType.Value, valueItem.DropDownItem);
								item.IsChecked = true;
								items.Add(item);

								_searchToRootTable[item] = valueItem;
							}
						}

						// lastly add the blanks if needed
						if (null != _blanksItem && _addedBlanks == true && isMatchCallback(_searchBlanks.DisplayTextInternal))
						{
							_searchBlanks.IsChecked = true;
							items.Add(_searchBlanks);

							_searchToRootTable[_blanksItem] = _searchBlanks;
						}
					}

					// in theory we could compare the items and do nothing if nothing has changed but 
					// excel repopulates or at least reinitializes the ischecked state of the items to true
					_selectAllSearchItem.ItemsSource.ReInitialize(items);

					// update the AllItems collection
					_allItems.Collections = new IList<RecordFilterTreeItem>[] {
						new RecordFilterTreeItem[] { _selectAllSearchItem, _addToFilterItem }
						, _selectAllSearchItem.ItemsSource
					};
				}
				finally
				{
					_selectAllSearchItem.ItemsSource.EndUpdate();
				}
			}
		}
		#endregion //OnSearchTextChanged

		#region UpdateDateRecordFilter
		private void UpdateDateRecordFilter(ConditionGroup conditionGroup, RecordFilterTreeItem item)
		{
			if (item.IsChecked == false)
				return;

			if (item.IsChecked == true)
			{
				DateRangeScope scope = GetDateRangeScope(item.ItemType);
				DateTime relativeDate = (DateTime)item.DropDownItem.Value;
				conditionGroup.Add(new ComparisonCondition(ComparisonOperator.Equals, SpecialFilterOperands.CreateDateRangeOperand(relativeDate, scope)));
			}
			else
			{
				foreach (RecordFilterTreeItem child in item.ItemsSource)
				{
					UpdateDateRecordFilter(conditionGroup, child);
				}
			}
		}

		#endregion //UpdateDateRecordFilter

		#region UpdateRecordFilter(ConditionGroup)
		private void UpdateRecordFilter(ConditionGroup conditionGroup)
		{
			conditionGroup.Clear();

			// if everything is checked we can leave the filter empty
			if (_selectAllItem.IsChecked == true)
				return;

			conditionGroup.LogicalOperator = LogicalOperator.Or;

			#region Blanks
			if (_addedBlanks == true)
			{
				if (_blanksItem.IsChecked == false)
				{
					bool isAllChecked = (_valuesItem.IsChecked == true || _valuesItem.ItemsSource.Count == 0) &&
						(_datesItem.IsChecked == true || _datesItem.ItemsSource.Count == 0);

					if (isAllChecked)
					{
						// everything except blanks then just include a != Blanks
						conditionGroup.Add(new ComparisonCondition(ComparisonOperator.NotEquals, _blanksOperand));
						return;
					}
				}
			}
			#endregion //Blanks

			#region Dates
			if (_datesItem.IsChecked != false)
			{
				foreach (RecordFilterTreeItem item in _datesItem.ItemsSource)
				{
					if (item.IsChecked != false)
						UpdateDateRecordFilter(conditionGroup, item);
				}
			} 
			#endregion //Dates

			#region Values
			// if any values are checked then include those values
			if (_valuesItem.IsChecked != false)
			{
				foreach (RecordFilterTreeItem item in _valuesItem.ItemsSource)
				{
					if (item.IsChecked == true)
						conditionGroup.Add(new ComparisonCondition(ComparisonOperator.Equals, item.Value));
				}
			}
			#endregion //Values

			#region Blanks
			if (_addedBlanks == true)
			{
				if (_blanksItem.IsChecked == true)
				{
					conditionGroup.Add(new ComparisonCondition(ComparisonOperator.Equals, _blanksOperand));
				}
			}
			#endregion //Blanks
		}

		#endregion //UpdateRecordFilter(ConditionGroup)

		#region UpdateRootNodeFromSearchNodes
		private void UpdateRootNodeFromSearchNodes()
		{
			this.UpdateRootNodeFromSearchNodes(_selectAllSearchItem);
		}

		private void UpdateRootNodeFromSearchNodes(RecordFilterTreeItem searchItem)
		{
			foreach (var item in searchItem.ItemsSource)
			{
				if (item.IsChecked == null)
				{
					UpdateRootNodeFromSearchNodes(item);
				}
				else
				{
					RecordFilterTreeItem rootItem;

					if (_searchToRootTable.TryGetValue(item, out rootItem))
					{
						// if both items are checked/unchecked then we don't need to continue
						if (item.IsChecked == rootItem.IsChecked)
							continue;

						// if the item has children and they have different checked states 
						// then we need to delve into it and deal with the leaf items
						if (item.ItemsCount > 0)
							UpdateRootNodeFromSearchNodes(item);
						else
							rootItem.IsChecked = item.IsChecked;
					}
				}
			}
		}
		#endregion //UpdateRootNodeFromSearchNodes

		#endregion //Private Methods

		#endregion //Methods

		#region IWeakEventListener Members
		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				switch (args.PropertyName)
				{
					case "":
					case "RecordFilter":
						this.OnFilterInfoChanged();
						break;
					case "RecordFilterVersion":
						if (!this.IsUpdatingFilter)
							this.ResetIsChecked();
						break;
				}
				return true;
			}

			return false;
		} 
		#endregion //IWeakEventListener Members

		#region SearchHelper class
		private class SearchHelper
		{
			private string _searchText;

			private SearchHelper(string searchText)
			{
				_searchText = searchText;
			}

			private bool IsMatch(string displayText)
			{
				return displayText != null && displayText.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;
			}

			internal static Predicate<string> CreateMatchCallback(string searchText)
			{
				// note our wildcard characters are different than excel. this is necessary 
				// since we want to be consistent within the product and the wildcard 
				// characters used for like matching support the same characters that 
				// vb's like matching supports. note as documented for the like docs 
				// for vb the right bracket does not need to be escaped (i.e. enclosed in 
				// brackets) and ! outside a bracket matches itself so we don't need to 
				// check for those characters
				if (searchText.IndexOfAny(new char[] { '?', '*', '[', '#' }) < 0)
				{
					// AS 6/3/11
					// While this would be consistent with excel it wouldn't be consistent 
					// with our contains/wildcard matching within the record filter itself.
					// It seems better to be consistent within the product so we'll just 
					// do a simple case insensitive contains implementation and not 
					// consider escaped characters.
					//
					//// follow excel and strip out any leading ~. the docs indicate 
					//// this really only applies when a following ~, ? or * but in 
					//// actuality they always treat it as an escape character and 
					//// strip it
					//if (searchText.IndexOf('~') >= 0)
					//    searchText = Regex.Replace(searchText, "~(.?)", "$1");

					return new SearchHelper(searchText).IsMatch;
				}

				Regex regex = null;

				try
				{
					regex = ComparisonCondition.WildcardToRegexConverter.Convert(searchText, true);
				}
				catch (ArgumentException)
				{
				}

				if (regex == null)
					return delegate(string text) { return false; };

				return regex.IsMatch;
			}
		}
		#endregion //SearchHelper class
	}
	#endregion //RecordFilterTreeItemProvider
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