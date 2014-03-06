using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Grids.Primitives;
using System.Linq;
using System.Windows.Data;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A <see cref="RowsManagerBase"/> that manages <see cref="Row"/> objects.
	/// </summary>
	public class RowsManager : RowsManagerBase, IProvideDataItems<Row>
	{
		#region Members

		DataManagerBase _dataManager;
		RowBaseCollection _rowBaseCollection;
		RowCollection _rows;
		List<RowBase> _registeredTopRows, _registeredBottomRows;
		HeaderRow _headerRow;
		FooterRow _footerRow;
		PagerRow _topPagerRow;
		PagerRow _bottomPagerRow;
		GroupByColumn _groupByColumn;

		AddNewRow _addNewRowTop;
		AddNewRow _addNewRowBottom;

		FilterRow _filterRowTop;
		FilterRow _filterRowBottom;

		SummaryRow _summaryRowTop;
		SummaryRow _summaryRowBottom;

		Dictionary<object, Row> _cachedRows;
		Dictionary<object, int> _duplicateObjectValidator;
		int _currentPageIndex = 1;
		int _groupByLevel = -1;
		int _displayableRowCount;
		FixedRowsOrderComparer _fixedRowsOrderComparer = new FixedRowsOrderComparer();
		Dictionary<object, Row> _cachedGroupByRows;
		ColumnLayoutTemplateColumn _columnLayoutTemplateColumn;
		ColumnLayoutTemplateRow _columnLayoutTemplateRow;
		bool _deferInvalidatingSort, _deferItemSourceInvalidation, _supressInvalidateRows, _deferClearGroupByCache, _supressCFUpdating;

		IEnumerable _itemSource;

		RowFiltersCollection _rowFiltersCollection;
		SummaryDefinitionCollection _summaryDefinitionCollection;
		SummaryResultCollection _summaryResultCollection;

		RowType? _newDataObjectRowType;

		ConditionalFormatProxyCollection _conditionalFormatProxyCollection;
        List<WeakReference> _attachedPropertyChangedTargets = new List<WeakReference>();
	    private WeakCollectionChangedHandler<RowsManager> _weakItemsSourceCollectionChanged;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="RowsManager"/> class.
		/// </summary>
		/// <param propertyName="level">The level that <see cref="RowsManager"/> is at.</param>
		/// <param propertyName="columnLayout">A reference to <see cref="ColumnLayout"/> object that corresponds with the <see cref="RowsManager"/></param>
		/// <param propertyName="parentLayoutRow">The owning <see cref="ChildBand"/> of the <see cref="RowsManager"/>. </param>
		internal RowsManager(int level, ColumnLayout columnLayout, RowBase parentLayoutRow)
			: base(columnLayout)
		{
			this.Level = level;
			this.ParentRow = parentLayoutRow;
			this._rows = new RowCollection(this, columnLayout);

			this._cachedRows = new Dictionary<object, Row>(new DataKeyComparer());
			this._cachedGroupByRows = new Dictionary<object, Row>();
            this._duplicateObjectValidator = new Dictionary<object, int>(new DataKeyComparer());

			this.HeaderSupported = this.FooterSupported = true;

			this.InvalidateAddNewRowVisibility(true);

			this.EnsureDataManager();            
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region ItemsSource

		/// <summary>
		/// Gets the data source for the <see cref="RowsManagerBase"/>.
		/// </summary>
		public IEnumerable ItemsSource
		{
			get { return this._itemSource; }
            protected internal set
            {
                if (this._itemSource != value)
                {
                    



                    if (this._itemSource != null)
                    {
                        if (this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
                            AttachDetachPropertyChanged(false);
                    }
                    this._itemSource = value;
                    this.OnItemsSourceChanged(this.ParentRow == null || this.ParentRow.RowType != RowType.GroupByRow);

                    if (this._itemSource != null)
                    {
                        if (this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
                            AttachDetachPropertyChanged(true);
                    }
                }
            }
		}

		#endregion // ItemsSource

		#region HeaderRow

		/// <summary>
		/// Gets a reference to the <see cref="HeaderRow"/> for this <see cref="RowsManager"/>
		/// </summary>
		public HeaderRow HeaderRow
		{
			get
			{
				if (this._headerRow == null)
					this._headerRow = new HeaderRow(this);
				return this._headerRow;
			}
		}

		#endregion // HeaderRow

		#region FooterRow

		/// <summary>
		/// Gets a reference to the <see cref="FooterRow"/> for this <see cref="RowsManager"/>
		/// </summary>
		public FooterRow FooterRow
		{
			get
			{
				if (this._footerRow == null)
					this._footerRow = new FooterRow(this);
				return this._footerRow;
			}
		}

		#endregion // FooterRow

		#region AddNewRowTop
		/// <summary>
		/// Gets a specialized row representing the AddNewRow of the top of the row island.
		/// </summary>
		public AddNewRow AddNewRowTop
		{
			get
			{
				if (this._addNewRowTop == null)
					this._addNewRowTop = new AddNewRow(this);
				return this._addNewRowTop;
			}
		}
		#endregion // AddNewRowTop

		#region AddNewRowBottom
		/// <summary>
		/// Gets a specialized row representing the AddNewRow of the bottom of the row island.
		/// </summary>
		public AddNewRow AddNewRowBottom
		{
			get
			{
				if (this._addNewRowBottom == null)
					this._addNewRowBottom = new AddNewRow(this);
				return this._addNewRowBottom;
			}
		}
		#endregion // AddNewRowBottom

		#region PagerRowTop
		/// <summary>
		/// Gets a specialized row representing the pager of the top of the row island.
		/// </summary>
		public PagerRow PagerRowTop
		{
			get
			{
				if (this._topPagerRow == null)
					this._topPagerRow = new PagerRow(this);
				return this._topPagerRow;
			}
		}
		#endregion // PagerRowTop

		#region PagerRowBottom
		/// <summary>
		/// Gets a specialized row representing the pager of the bottom of the row island.
		/// </summary>
		public PagerRow PagerRowBottom
		{
			get
			{
				if (this._bottomPagerRow == null)
					this._bottomPagerRow = new PagerRow(this);
				return this._bottomPagerRow;
			}
		}
		#endregion  // PagerRowBottom

		#region FilterRowTop
		/// <summary>
		/// Gets a specialized row representing the FilterRow UI at the top of the row island.
		/// </summary>
		public FilterRow FilterRowTop
		{
			get
			{
				if (this._filterRowTop == null)
					this._filterRowTop = new FilterRow(this);
				return this._filterRowTop;
			}
		}

		#endregion

		#region FilterRowBottom
		/// <summary>
		/// Gets a specialized row representing the FilterRow UI at the bottom of the row island.
		/// </summary>
		public FilterRow FilterRowBottom
		{
			get
			{
				if (this._filterRowBottom == null)
					this._filterRowBottom = new FilterRow(this);
				return this._filterRowBottom;
			}
		}

		#endregion

		#region SummaryRowTop
		/// <summary>
		/// Gets a specialized row representing the Summary UI at the top of the row island.
		/// </summary>
		public SummaryRow SummaryRowTop
		{
			get
			{
				if (this._summaryRowTop == null)
					this._summaryRowTop = new SummaryRow(this);
				return this._summaryRowTop;
			}
		}
		#endregion // SummaryRowTop

		#region SummaryRowBottom
		/// <summary>
		/// Gets a specialized row representing the Summary UI at the bottom of the row island.
		/// </summary>
		public SummaryRow SummaryRowBottom
		{
			get
			{
				if (this._summaryRowBottom == null)
					this._summaryRowBottom = new SummaryRow(this);
				return this._summaryRowBottom;
			}
		}
		#endregion // SummaryRowBottom

		#region CurrentPageIndex
		/// <summary>
		/// Gets / sets the page of data to be shown when the <see cref="ColumnLayout.PagerSettings"/> allows for paging.
		/// </summary>
		public int CurrentPageIndex
		{
			get
			{
				DataManagerBase manager = this.DataManager;
				if (manager != null)
					return manager.CurrentPage;
				return 0;
			}
			set
			{
                if (value < 0)
                {
                    throw new InvalidPageIndexException(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("InvalidPageIndexException"), value));
                }

				if (value != this._currentPageIndex)
				{
                    // If we're paging, and we're in edit mode, then exit, and cancel it. Otherwise if there is a 
                    // validation error, we won't be able to put another cell in edit mode. 
                    if (this.ColumnLayout.Grid.CurrentEditCell != null || this.ColumnLayout.Grid.CurrentEditRow != null)
                        this.ColumnLayout.Grid.ExitEditMode(true);

					bool cancel = false;

					if (this.ColumnLayout != null)
					{
						cancel = this.ColumnLayout.Grid.OnPageIndexChanging(value, this.Level, this._rows, this.ColumnLayout);
					}
					if (!cancel)
					{
						int oldPageIndex = this._currentPageIndex;

						this._currentPageIndex = value;

                        this.AttachDetachPropertyChanged(false);

						DataManagerBase manager = this.DataManager;

						if (manager != null)
						{
							manager.CurrentPage = value;
						}

						this.ColumnLayout.Grid.OnPageIndexChanged(oldPageIndex);

						// Only scroll if we're paging the root level. 
						if (this.ParentRow == null && this.Rows.Count > 0 && this.Rows[0].Cells.Count > 0)
							this.ColumnLayout.Grid.ScrollCellIntoView(this.Rows[0].Cells[0]);
					}
					else
					{
						InvalidatePager();
					}
				}
				else
				{
					DataManagerBase manager = this.DataManager;
					if (manager != null)
					{
						manager.CurrentPage = value;
					}
				}

			}
		}
		#endregion

		#region PageCount

		/// <summary>
		/// Gets the total number of pages available from the DataManager.
		/// </summary>
		protected internal int PageCount
		{
			get
			{
				int pageCount = 0;
				DataManagerBase manager = this.DataManager;
				if (manager != null)
				{
					if (manager.PageCount < 1)
						pageCount = 1;
					else
						pageCount = manager.PageCount;
				}

				return pageCount;
			}
		}

		#endregion // PageCount

		#region GroupByLevel

		/// <summary>
		/// Gets the level that a <see cref="RowsManager"/> is grouped at.
		/// </summary>
		public int GroupByLevel
		{
			get
			{
				return this._groupByLevel;
			}
			protected set
			{
				this._groupByLevel = value;
			}
		}

		#endregion // GroupByLevel

		#region GroupedColumn

		/// <summary>
		/// Gets the <see cref="Column"/> that a <see cref="RowsManager"/> is grouped by.
		/// </summary>
		public Column GroupedColumn
		{
			get;
			protected internal set;
		}
		#endregion // GroupedColumn

		#region GroupByColumn

		/// <summary>
		/// Gets the <see cref="GroupByColumn"/> that represents all <see cref="GroupByCell"/> objects of this GroupByLevel.
		/// </summary>
		internal GroupByColumn GroupByColumn
		{
			get
			{
				if (this._groupByColumn == null)
				{
					this._groupByColumn = new GroupByColumn();
					this._groupByColumn.ColumnLayout = this.ColumnLayout;
				}
				return this._groupByColumn;
			}
		}
		#endregion // GroupByColumn

		#region ColumnLayoutTemplateColumn

		/// <summary>
		/// Gets the <see cref="ColumnLayoutTemplateColumn"/> that represents all <see cref="ColumnLayoutTemplateCell"/> objects that are displayed when a <see cref="DataTemplate"/> is specified on a <see cref="TemplateColumnLayout"/>
		/// </summary>
		internal ColumnLayoutTemplateColumn ColumnLayoutTemplateColumn
		{
			get
			{
				if (this._columnLayoutTemplateColumn == null)
				{
					this._columnLayoutTemplateColumn = new ColumnLayoutTemplateColumn();
					this._columnLayoutTemplateColumn.ColumnLayout = this.ColumnLayout;
				}
				return this._columnLayoutTemplateColumn;
			}
		}
		#endregion // ColumnLayoutTemplateColumn

		#region RowFiltersCollection
		/// <summary>
		/// Gets a <see cref="RowFiltersCollection"/> object that contains the filters being applied to this <see cref="ColumnLayout"/>.
		/// </summary>
		public RowFiltersCollection RowFiltersCollection
		{
			get
			{
				if (this._rowFiltersCollection == null)
				{
					this._rowFiltersCollection = new RowFiltersCollection();
					this._rowFiltersCollection.PropertyChanged += RowFiltersCollection_PropertyChanged;
					this._rowFiltersCollection.CollectionChanged += RowFiltersCollection_CollectionChanged;
					this._rowFiltersCollection.CollectionItemChanged += RowFiltersCollection_CollectionItemChanged;
				}

				return this._rowFiltersCollection;
			}
		}

		#endregion // RowFiltersCollection

		#region RowFiltersCollectionResolved

		/// <summary>
		/// Gets the <see cref="RowFiltersCollection"/> that will be used by the <see cref="RowsManager"/> based on the <see cref="FilteringScope"/> of the <see cref="ColumnLayout"/>.
		/// </summary>
		public RowFiltersCollection RowFiltersCollectionResolved
		{
			get
			{
				ColumnLayout cl = this.ColumnLayout;
				if (cl != null)
				{
					FilteringSettingsOverride settings = cl.FilteringSettings;
					if (settings.FilteringScopeResolved == FilteringScope.ColumnLayout)
					{
						return settings.RowFiltersCollection;
					}
				}
				return this.RowFiltersCollection;
			}
		}

		#endregion

		#region SummaryDefinitionCollection
		/// <summary>
		/// Gets a <see cref="SummaryDefinitionCollection"/> object that contains the filters being applied to this <see cref="ColumnLayout"/>.
		/// </summary>
		public SummaryDefinitionCollection SummaryDefinitionCollection
		{
			get
			{
				if (this._summaryDefinitionCollection == null)
				{
					this._summaryDefinitionCollection = new SummaryDefinitionCollection();
					this._summaryDefinitionCollection.CollectionChanged += SummaryDefinitionCollection_CollectionChanged;
				}
				return this._summaryDefinitionCollection;
			}
		}
		#endregion // SummaryDefinitionCollection

		#region SummaryDefinitionCollectionResolved
		/// <summary>
		/// Gets the <see cref="SummaryDefinitionCollection"/> that will be used by the <see cref="RowsManager"/> based on the <see cref="SummaryScope"/> of the <see cref="ColumnLayout"/>.
		/// </summary>
		public SummaryDefinitionCollection SummaryDefinitionCollectionResolved
		{
			get
			{
				SummaryRowSettingsOverride settings = this.ColumnLayout.SummaryRowSettings;
				if (settings.SummaryScopeResolved == SummaryScope.ColumnLayout)
				{
					return settings.SummaryDefinitionCollection;
				}
				return this.SummaryDefinitionCollection;
			}
		}

		#endregion // SummaryDefinitionCollectionResolved

		#region SummaryResultCollection

		/// <summary>
		/// Gets the collection of <see cref="SummaryResult"/> objects that will be displayed.
		/// </summary>
		public ReadOnlyCollection<SummaryResult> SummaryResultCollection
		{
			get
			{
				return new ReadOnlyCollection<SummaryResult>(this.SummaryResultCollectionInternal);
			}
		}
		#endregion // SummaryResultCollection

		#endregion // Public

		#region Protected

		#region DataManagerBase

		/// <summary>
		/// Gets a reference to the <see cref="DataManagerBase"/> of the <see cref="RowsManager"/>.
		/// </summary>
		protected internal virtual DataManagerBase DataManager
		{
			get
			{
				this.EnsureDataManager();
				return this._dataManager;
			}
		}
		#endregion // DataManagerBase

		#region DataCount

		/// <summary>
		/// Gets the amount of <see cref="Row"/>s in the <see cref="RowsManager"/>.
		/// </summary>
		protected virtual int DataCount
		{
			get
			{
				DataManagerBase manager = this.DataManager;
				if (manager != null)
				{
                    manager.EnablePaging = this.ColumnLayout.PagerSettings.AllowPagingResolved != PagingLocation.None;

                    if (manager.EnablePaging)
					    manager.PageSize = this.ColumnLayout.PagerSettings.PageSizeResolved;
					
                    return manager.RecordCount;
				}
				return 0;
			}
		}
		#endregion // DataCount

		#region CachedRows

		/// <summary>
		/// Gets a dictionary of rows that have already been created.
		/// </summary>
		/// <remarks>
		/// This property is used so that when operations such as sorting occur,
		/// the rows that were previously created aren'type lost. 
		/// </remarks>
		protected internal Dictionary<object, Row> CachedRows
		{
			get { return this._cachedRows; }
			set { this._cachedRows = value; }
		}

		#endregion // CachedRows

		#region FooterSupported
		/// <summary>
		/// Gets if the <see cref="RowsManager"/> supports showing the footer in the general rows body.
		/// </summary>
		protected virtual bool FooterSupported
		{
			get;
			set;
		}
		#endregion // FooterSupported

		#region HeaderSupported
		/// <summary>
		/// Gets if the <see cref="RowsManager"/> supports showing the header in the general rows body.
		/// </summary>
		protected virtual bool HeaderSupported
		{
			get;
			set;
		}
		#endregion // HeaderSupported

		#region RegisteredTopRows
		/// <summary>
		/// Gets the <see cref="RowBase"/> object collection of rows that will be considered header rows.
		/// </summary>
		protected internal List<RowBase> RegisteredTopRows
		{
			get
			{
				if (this._registeredTopRows == null)
					this._registeredTopRows = new List<RowBase>();

				return this._registeredTopRows;
			}
		}
		#endregion // RegisteredTopRows

		#region RegisteredBottomRows
		/// <summary>
		/// Gets the <see cref="RowBase"/> object collection of rows that will be considered footer rows.
		/// </summary>
		protected internal List<RowBase> RegisteredBottomRows
		{
			get
			{
				if (this._registeredBottomRows == null)
					this._registeredBottomRows = new List<RowBase>();

				return this._registeredBottomRows;
			}
		}

		#endregion // RegisteredBottomRows

		#region ChildrenShouldBeDisplayedResolved

		/// <summary>
		/// Resolves whether Child rows should be displayed.
		/// </summary>
		protected internal virtual  bool ChildrenShouldBeDisplayedResolved
		{
			get
			{
                if (this.ColumnLayout.Visibility != Visibility.Visible)
                    return false;

				if (this.ParentRow != null && (this.ColumnLayout.InternalTemplate != null))
					return true;

				if (this.ItemsSource == null)
					return false;

				if (this._displayableRowCount > 0)
				{
					foreach (RowBase r in this.RegisteredBottomRows)
					{
						if (r.IsStandAloneRowResolved)
							return true;
					}

					foreach (RowBase r in this.RegisteredTopRows)
					{
						if (r.IsStandAloneRowResolved)
							return true;
					}
				}

                RowFiltersCollection rfc = this.RowFiltersCollectionResolved;

                for (int i = 0; i < rfc.Count; i++)
                {
                    ConditionCollection cc = rfc[i].Conditions;

                    if (cc.Count > 0 && this.DataManager != null && this.DataManager.TotalRecordCount > 0)
                        return true;
                }

                return (this.DataCount > 0);                                                  
			}
		}
		#endregion // ChildrenShouldBeDisplayedResolved

		#region SummaryResultCollectionInternal
		/// <summary>
		/// Gets the collection of <see cref="SummaryResult"/> objects that will be displayed.
		/// </summary>
		internal protected SummaryResultCollection SummaryResultCollectionInternal
		{
			get
			{
				if (this._summaryResultCollection == null)
				{
					this._summaryResultCollection = new SummaryResultCollection();
				}
				return this._summaryResultCollection;
			}
		}
		#endregion // SummaryResultCollectionInternal

		#region ConditionalFormattingRulesResolved

		/// <summary>
		/// Gets the <see cref="ConditionalFormatCollection"/> which will be applied.
		/// </summary>
		internal protected ConditionalFormatCollection ConditionalFormattingRulesResolved
		{
			get
			{
				return this.ColumnLayout.ConditionalFormatCollection;
			}
		}

		#endregion // ConditionalFormattingRulesResolved

		#region ConditionalFormattingProxyRulesResolved

		/// <summary>
		/// Gets the <see cref="ConditionalFormatProxyCollection"/> which will be applied.
		/// </summary>
		internal protected ConditionalFormatProxyCollection ConditionalFormattingProxyRulesResolved
		{
			get
			{
				return this.ConditionalFormatProxyCollection;
			}
		}

		#endregion // ConditionalFormattingProxyRulesResolved

		#region ConditionalFormatProxyCollection

		/// <summary>
		/// Gets the <see cref="ConditionalFormatProxyCollection"/> which contains the formats which will applied to the rows/
		/// </summary>
		protected ConditionalFormatProxyCollection ConditionalFormatProxyCollection
		{
			get
			{
				if (this._conditionalFormatProxyCollection == null)
				{
					this._conditionalFormatProxyCollection = new ConditionalFormatProxyCollection();
				}
				return this._conditionalFormatProxyCollection;
			}
		}

		#endregion // ConditionalFormatProxyCollection

		#endregion // Protected

		#endregion // Properties

		#region Methods

		#region Private

		#region BuildConditionalFormats
		private void BuildConditionalFormats()
		{
			this.ConditionalFormatProxyCollection.Clear();
			foreach (IConditionalFormattingRule rule in this.ColumnLayout.ConditionalFormatCollection)
			{
				IConditionalFormattingRuleProxy proxy = rule.CreateProxy();
				proxy.Parent = rule;
				this.ConditionalFormatProxyCollection.Add(proxy);
			}
			this.InvalidateConditionalFormatting();
		}
		#endregion // BuildConditionalFormats

		#region SetupDataManager

		private void SetupDataManager()
		{
            this._dataManager = DataManagerBase.CreateDataManager(this.ItemsSource, this.ColumnLayout.Grid.DataManagerProvider);
			if (this._dataManager != null)
			{
                if (this.Level == 0 && this.ParentRow != null && this.ParentRow.RowType == RowType.GroupByRow)
                    this._dataManager.CachedTypedInfo = ((RowsManager)this.ParentRow.Manager).DataManager.CachedTypedInfo;

				this._dataManager.CollectionChanged += new NotifyCollectionChangedEventHandler(DataManager_CollectionChanged);
				this._dataManager.NewObjectGeneration += new EventHandler<HandleableObjectGenerationEventArgs>(DataManager_NewObjectGeneration);
				this._dataManager.ResolvingData += new EventHandler<DataAcquisitionEventArgs>(DataManager_ResolvingData);
				this._dataManager.DataUpdated += new EventHandler<EventArgs>(DataManager_DataUpdated);
                this._dataManager.CurrentItemChanged += new EventHandler<CurrentItemEventArgs>(DataManager_CurrentItemChanged);

				this._dataManager.SuspendInvalidateDataSource = true;

				this.InitializeData();

				
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				if (this.ColumnLayout.SortingSettings.SortedColumns.Count > 0) 
					this.InvalidateSort();

                if (this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
                    this._dataManager.ConditionalFormattingRules = this.ConditionalFormatProxyCollection.GenerateIRuleCollection();
                else
                    this._dataManager.ConditionalFormattingRules = null;

				this._dataManager.SummaryResultCollection = this.SummaryResultCollectionInternal;

				this._dataManager.Filters = this.RowFiltersCollectionResolved;

				this._dataManager.SummaryExecution = this.ColumnLayout.SummaryRowSettings.SummaryExecutionResolved;

				this._dataManager.Summaries = this.SummaryDefinitionCollectionResolved;

                this._dataManager.CurrentPage = this._currentPageIndex;

				this._dataManager.SuspendInvalidateDataSource = false;
			}
			else
			{
				// If a data source was assigned
				// but there was no data in the collection
				// but the collection implements INotifyCollectionChanged
				// then we can figure out when data is added and create a datamanager at that point. 
                INotifyCollectionChanged notifyCollectionChanged = this.ItemsSource as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
				{
                    if (this._weakItemsSourceCollectionChanged != null)
				    {
				        this._weakItemsSourceCollectionChanged.Detach();
				        this._weakItemsSourceCollectionChanged = null;
				    }

                    this._weakItemsSourceCollectionChanged =
                        new WeakCollectionChangedHandler<RowsManager>
                        (
                            this,
                            notifyCollectionChanged,
                            (instance, s, e) => instance.ItemsSource_CollectionChanged(s, e)
                        );
                    notifyCollectionChanged.CollectionChanged += this._weakItemsSourceCollectionChanged.OnEvent;
				}
			}
		} 

		#endregion // SetupDataManager

		#region EnsureDataManager
		/// <summary>
		/// This method checks to ensure that a DataManagerBase is created for a given level and if not creates it for that level.
		/// </summary>
		protected void EnsureDataManager()
		{
			if (this.ItemsSource == null)
			{
				if (this.ParentRow != null)
				{
					if (this.ParentRow.ItemSource != null)
					{
						this.ItemsSource = this.ParentRow.ItemSource;
					}
					else
					{
						object data = this.ParentRow.Data;
						if (data != null)
						{
							PropertyInfo info = this.ColumnLayout.ResovlePropertyInfo(data);
							if (info != null)
							{
								this.ItemsSource = info.GetValue(this.ParentRow.Data, null) as IEnumerable;
							}
							else
							{
                                object obj = DataManagerBase.ResolveValueFromPropertyPath(this.ColumnLayout.Key, data);
								this.ItemsSource = obj as IEnumerable;
							}

							// An ItemSource property, could potentially have a setter, and if the property changes
							// for that IEnumerable, the Grid should reflect the new data. 
							INotifyPropertyChanged inpc = data as INotifyPropertyChanged;
							if (inpc != null)
							{
								inpc.PropertyChanged -= ItemSource_PropertyChanged;
								inpc.PropertyChanged += ItemSource_PropertyChanged;
							}
						}
					}

					if (this.ItemsSource != null && this._dataManager == null)
						this.SetupDataManager();
				}
			}
			else if (this._dataManager == null)
			{
				this.SetupDataManager();

                // When a DataManager is unhooked, we remove the PropertyChange hook
                // However, its possible it was unhooked b/c we got a new ItemSource attached, in which case it won't be null
                // So we should stil listen to inpc, so lets hook it back up if we can. 
                if (this.ParentRow != null)
                {
                    object data = this.ParentRow.Data;
                    if (data != null)
                    {
                        // An ItemSource property, could potentially have a setter, and if the property changes
                        // for that IEnumerable, the Grid should reflect the new data. 
                        INotifyPropertyChanged inpc = data as INotifyPropertyChanged;
                        if (inpc != null)
                        {
                            inpc.PropertyChanged -= ItemSource_PropertyChanged;
                            inpc.PropertyChanged += ItemSource_PropertyChanged;
                        }
                    }
                }
			}
		}

		#endregion // EnsureDataManager

		#region InvalidateItemSource

        private void InvalidateItemSource(bool validateInList, bool invalidateSelection, bool invalidateSort)
		{
			if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
			{
				ICollectionView icv = this.ItemsSource as ICollectionView;
				IPagedCollectionView ipcv = this.ItemsSource as IPagedCollectionView;

                // No need to invalidate if we're a GroupyBy row's ItemSource.
                if(this.ParentRow == null || this.ParentRow.RowType != RowType.GroupByRow)
                    this.InvalidateSelectionAndActivation(validateInList, invalidateSelection);

				if (icv != null && icv.CanSort)
				{
					bool reset = false;

                    ReadOnlyCollection<Column> groupedCols = this.ColumnLayout.Grid.GroupBySettings.GroupByColumns[this.ColumnLayout];
                    int groupedColCount = (icv.GroupDescriptions == null) ? 0 : icv.GroupDescriptions.Count;

                    if (groupedColCount > 0 && groupedCols.Count > 0)
                    {
                        PropertyGroupDescription pgd = icv.GroupDescriptions[0] as PropertyGroupDescription;
                        if (pgd.PropertyName != groupedCols[0].Key)
                            reset = true;
                    }
                    else if (groupedCols.Count == 0 && groupedColCount > 0)
                    {
                        reset = true;
                    }

                    if (reset)
                    {
                        this._deferInvalidatingSort = true;

                        List<Column> groups = new List<Column>(groupedCols);

                        GroupByColumnsCollection gbcc = this.ColumnLayout.Grid.GroupBySettings.GroupByColumns;
                        foreach (Column col in groups)
                            gbcc.Remove(col);

                        foreach (GroupDescription gd in icv.GroupDescriptions)
                        {
                            PropertyGroupDescription pgd = gd as PropertyGroupDescription;
                            if (pgd != null)
                            {
                                Column col = this.ColumnLayout.Columns.DataColumns[pgd.PropertyName];
                                if (col != null)
                                    col.IsGroupBy = true;
                            }
                        }
                    }

					SortedColumnsCollection cols = this.ColumnLayout.SortingSettings.SortedColumns;
					int count = icv.SortDescriptions.Count;

					// If the count is wrong... reset
					if (count != cols.Count)
					{
						reset = true;
					}
					else if (count != 0)
					{
						List<Column> sortedCols = new List<Column>(cols);

						int sdOffset = 0;
						if (this.GroupedColumn != null)
						{
							SortDescription sd = icv.SortDescriptions[0];

							bool colAscending = (this.GroupedColumn.IsSorted == SortDirection.Ascending);
							bool sdAscending = (sd.Direction == ListSortDirection.Ascending);

							if (this.GroupedColumn.Key != sd.PropertyName || colAscending != sdAscending)
							{
								reset = true;
							}
							sortedCols.Remove(this.GroupedColumn);
							sdOffset = 1;
						}

						if (!reset)
						{
							// Otherwise, validate the order of the sorted columns, and the direction of them.
							for (int i = sdOffset; i < count; i++)
							{
								SortDescription sd = icv.SortDescriptions[i];
								Column col = sortedCols[i - sdOffset];

								bool colAscending = (col.IsSorted == SortDirection.Ascending);
								bool sdAscending = (sd.Direction == ListSortDirection.Ascending);

								if (col.Key != sd.PropertyName || colAscending != sdAscending)
								{
									reset = true;
									break;
								}
							}
						}
					}

					if (reset)
					{
						this._deferInvalidatingSort = true;

						cols.Clear();
						foreach (SortDescription sd in icv.SortDescriptions)
						{
							Column col = this.ColumnLayout.Columns.DataColumns[sd.PropertyName];
							if (col != null)
								col.IsSorted = (sd.Direction == ListSortDirection.Ascending) ? SortDirection.Ascending : SortDirection.Descending;
						}

						this._deferInvalidatingSort = false;
						this.InvalidateSort();
					}

                    PagerSettingsOverride pagerSettings = this.ColumnLayout.PagerSettings;
                    if (ipcv != null)
                    {
                        if (ipcv.PageIndex != -1)
                        {
                            if (pagerSettings.PageSizeResolved != ipcv.PageSize)
                                pagerSettings.PageSize = ipcv.PageSize;

                            int pageIndex = ipcv.PageIndex + 1;
                            if (this.CurrentPageIndex != pageIndex)
                                this.CurrentPageIndex = pageIndex;
                        }
                        else
                        {
                            if (pagerSettings.AllowPagingResolved != PagingLocation.None)
                            {
                                if (ipcv.PageSize != 0)
                                    ipcv.PageSize = pagerSettings.PageSizeResolved;
                            }
                        }
                    }
                    else
                    {
                        this.ValidatePagerInformation();
                    }
				}
				else if(invalidateSort)
                {
                    this.InvalidateSort();
                }
            }
		}

		#endregion // InvalidateItemSource

        #region InvalidateSelectionAndActivation

		private void InvalidateSelectionAndActivation(bool validateInList, bool invalidateSelection)
		{
			ICollectionView icv = this.ItemsSource as ICollectionView;
			IList list = this.ItemsSource as IList;

            if (invalidateSelection)
            {
                Collection<Row> selectedRowsToRemove = new Collection<Row>();
                SelectedRowsCollection rows = this.ColumnLayout.Grid.SelectionSettings.SelectedRows;

                foreach (Row row in rows)
                {
                    if (row.Manager == this)
                    {
                        if (!validateInList)
                        {
                            if (list != null && list.Contains(row.Data) || (icv != null && icv.Contains(row.Data)))
                                selectedRowsToRemove.Add(row);
                        }
                        else
                        {
                            if (list != null && !list.Contains(row.Data) || (icv != null && !icv.Contains(row.Data)))
                                selectedRowsToRemove.Add(row);
                        }
                    }
                }

                foreach (Row deletedRow in selectedRowsToRemove)
                    rows.Remove(deletedRow);

                Collection<Cell> selectedCellsToRemove = new Collection<Cell>();
                SelectedCellsCollection cells = this.ColumnLayout.Grid.SelectionSettings.SelectedCells;

                foreach (Cell cell in cells)
                {
                    RowBase row = cell.Row;
                    if (row.Manager == this)
                    {
                        if (!validateInList)
                        {
                            if (list != null && list.Contains(row.Data) || (icv != null && icv.Contains(row.Data)))
                                selectedCellsToRemove.Add(cell);
                        }
                        else
                        {
                            if (list != null && !list.Contains(row.Data) || (icv != null && !icv.Contains(row.Data)))
                                selectedCellsToRemove.Add(cell);
                        }
                    }
                }

                foreach (Cell deletedCell in selectedCellsToRemove)
                    cells.Remove(deletedCell);


                CellBase activeCell = this.ColumnLayout.Grid.ActiveCell;

                if (activeCell != null)
                {
                    RowBase activeRow = activeCell.Row;
                    if (activeRow.Manager == this)
                    {
                        if (!this.RegisteredTopRows.Contains(activeRow) && !this.RegisteredBottomRows.Contains(activeRow))
                        {
                            if (!validateInList)
                            {
                                if (list != null && list.Contains(activeRow.Data) || (icv != null && icv.Contains(activeRow.Data)))
                                    this.ColumnLayout.Grid.ActiveCell = null;
                            }
                            else
                            {
                               
                                if (list != null && !list.Contains(activeRow.Data) || (icv != null && !icv.Contains(activeRow.Data)))
                                {
                                    // For ICollectionView, do a check on the SourceCollection as well. 
                                    IList list2 = null;
                                    if (icv != null)
                                        list2 = icv.SourceCollection as IList;

                                    if (list2 == null || list2 != null && !list2.Contains(activeRow.Data))
                                    {
                                        // If the data is null it could be anything, so just assume its ok, b/c we might be dealing with a VC
                                        if (activeRow.Data != null)
                                            this.ColumnLayout.Grid.ActiveCell = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
		}

        #endregion // InvalidateSelectionAndActivation

		#region UnhookDatamanager

		private void UnhookDataManager(bool clearChildRows, bool clearSelection, bool clearGroupBy)
		{
            if (this.ParentRow != null)
            {
                object data = this.ParentRow.Data;
                if (data != null)
                {
                    // An ItemSource property, could potentially have a setter, and if the property changes
                    // for that IEnumerable, the Grid should reflect the new data. 
                    INotifyPropertyChanged inpc = data as INotifyPropertyChanged;
                    if (inpc != null)
                    {
                        inpc.PropertyChanged -= ItemSource_PropertyChanged;
                    }
                }
            }

			if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
			{
				this.InvalidateSelectionAndActivation(false, clearSelection);
			}

			this.InvalidateRows();

            // Invalidate the Cells, otherwise, the columns and header cells won't be released.
            this.HeaderRow.Cells.Reset();

			if (clearChildRows)
			{
                bool skipunhook = false;
                if (this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.GridIsGoingToBeDestroyed)
                {
                    skipunhook = true;
                }

                if (!skipunhook)
                {
                    foreach (KeyValuePair<object, Row> obj in this._cachedRows)
                    {
                        if (obj.Value.Manager == this) // Don't clear it if it doesn't belong to you. 
                        {
                            if (obj.Value.ChildRowsManager != null)
                            {
                                obj.Value.ChildRowsManager.UnregisterRowsManager(true, true, true);
                            }
                        }
                    }
                }

				this._cachedRows.Clear();

                if (clearGroupBy)
                {
                    foreach (KeyValuePair<object, Row> obj in this._cachedGroupByRows)
                    {
                        if (obj.Value.Manager == this) // Don't clear it if it doesn't belong to you. 
                        {
                            if (obj.Value.ChildRowsManager != null)
                                ((RowsManager)obj.Value.ChildRowsManager).UnregisterRowsManager(true, true, true);
                        }
                    }

                    
                    if (this.GroupedColumn != null && !this.GroupedColumn.IsGroupBy)
                    {
                        this.GroupedColumn.SetSortedColumnStateInternally(SortDirection.None, true);
                        this.GroupedColumn = null;
                    }

                    this._cachedGroupByRows.Clear();
                }
			}

			if (this._dataManager != null)
			{
                this._dataManager.DataSource = null;
				this._dataManager.CollectionChanged -= DataManager_CollectionChanged;
				this._dataManager.NewObjectGeneration -= DataManager_NewObjectGeneration;
				this._dataManager.ResolvingData -= DataManager_ResolvingData;
				this._dataManager.DataUpdated -= DataManager_DataUpdated;
                this._dataManager.CurrentItemChanged -= DataManager_CurrentItemChanged;
				this._dataManager.Filters = null;
				this._dataManager = null;
			}
		}

		#endregion // UnhookDataManager

		#region ClearSpecializedRows
		private void ClearSpecializedRows()
		{
			if (this._addNewRowBottom != null)
            {
                this.UnregisterBottomRow(this.AddNewRowBottom);
                this._addNewRowBottom.Cells.Reset();
                this._addNewRowBottom.Manager = null;
            }

			if (this._addNewRowTop != null)
            {
                this.UnregisterTopRow(this.AddNewRowTop);
                this._addNewRowTop.Cells.Reset();
                this._addNewRowTop.Manager = null;
            }

            if (this._filterRowTop != null)
            {
                this.UnregisterTopRow(this.FilterRowTop);
                foreach (CellBase cell in this._filterRowTop.Cells)
                {
                    FilterRowCell frc = cell as FilterRowCell;
                    if (frc != null)
                    {
                        frc.FilterCellValue = null;
                    }
                }

                this._filterRowTop.Cells.Reset();
                this._filterRowTop.Manager = null;
            }

            if (this._filterRowBottom != null)
            {
                this.UnregisterBottomRow(this.FilterRowBottom);
                foreach (CellBase cell in this._filterRowBottom.Cells)
                {
                    FilterRowCell frc = cell as FilterRowCell;
                    if (frc != null)
                    {
                        frc.FilterCellValue = null;
                    }
                }

                this._filterRowBottom.Cells.Clear();
                this._filterRowBottom.Manager = null;
            }

			if (this._summaryRowTop != null)
            {
                this.UnregisterTopRow(this.SummaryRowTop);
                this._summaryRowTop.Cells.Clear();
                this._summaryRowTop.Manager = null;
            }

			if (this._summaryRowBottom != null)
            {
                this.UnregisterBottomRow(this.SummaryRowBottom);
                this._summaryRowBottom.Cells.Clear();
                this._summaryRowBottom.Manager = null;
            }

            if (this._topPagerRow != null)
            {
                this.UnregisterTopRow(this.PagerRowTop);
                this._topPagerRow.Cells.Reset();
                this._topPagerRow.Manager = null;
            }

            if (this._bottomPagerRow != null)
            {
                this.UnregisterBottomRow(this.PagerRowBottom);
                this._bottomPagerRow.Cells.Reset();
                this._bottomPagerRow.Manager = null;
            }

			this._filterRowBottom = null;
			this._filterRowTop = null;
			this._addNewRowBottom = null;
			this._addNewRowTop = null;
			this._summaryRowBottom = null;
			this._summaryRowTop = null;
            this._topPagerRow = null;
            this._bottomPagerRow = null;
		}
		#endregion // ClearSpecializedRows

		#endregion // Private

		#region Protected

		#region GetDataItem

		/// <summary>
		/// Returns the <see cref="Row"/> for the given index.
		/// </summary>
		/// <param propertyName="index">The index of the row to retrieve.</param>
		/// <returns></returns>
		protected virtual Row GetDataItem(int index)
		{
			DataManagerBase manager = this.DataManager;
			if (manager != null)
			{
				PagerSettingsOverride pagerSettings = this.ColumnLayout.PagerSettings;
				manager.EnablePaging = pagerSettings.AllowPagingResolved != PagingLocation.None;

                if (manager.EnablePaging)
                    manager.PageSize = pagerSettings.PageSizeResolved;

				object data = manager.GetRecord(index);

				GroupByDataContext groupData = data as GroupByDataContext;

				if (groupData == null)
				{
                    // Check to see if the data we're getting back is merged.
                    MergedRowInfo mri = data as MergedRowInfo;
                    if (mri != null)
                        data = mri.Data;

					Row row = null;
					if (data != null)
					{
						if (this._cachedRows.ContainsKey(data))
						{
							



							if (!this._duplicateObjectValidator.ContainsKey(data))
							{
								row = (Row)this._cachedRows[data];
								row.Manager = this;
								this._duplicateObjectValidator.Add(data, index);
								row.Index = index;

                                // If someone hides a ColumnLayout, that was visible and there are rows off screen that were expanded
                                // that are now on screen, rows that shouldn't be visible could be seen.
                                // SO, to fix that, I added this InvalidateManager method to the ChildBandRowsManager, which allows it
                                // to do the validation, that other ChildRowsManagers got to do, when they were invalidated
                                // when the COlumnLayout's visibility changed.
                                if (row.ContainsChildBandRowsManager)
                                {
                                    ChildBandRowsManager cbrm = (ChildBandRowsManager)row.ChildRowsManager;
                                    if (cbrm != null)
                                        cbrm.InvalidateManager();
                                }
                                if (row.IsExpanded)
                                {
                                    this.RegisterChildRowsManager(row.ChildRowsManager);
                                }

                                row.MergeData = mri;

								return row;
							}
							else
							{
                                return new Row(index, this, data) { MergeData = mri };
							}
						}

                        // Ok, so in some cases, specvifically with a VC
                        // We may have an activeCell/row and it might not be out of sync, meaning its data is invalid
                        // So do a quick check and see if its the data we're dealing with here. 
                        // If so, thats our new row.
                        CellBase cell = this.ColumnLayout.Grid.ActiveCell;
                        if (cell != null && cell.Row.Manager == this && cell.Row.ColumnLayout == this.ColumnLayout)
                        {
                            if (cell.Row.Index == index && cell.Row.Data == null)
                            {
                                row = (Row)cell.Row;
                                row.Data = data;
                                if (row.Control != null)
                                    row.Control.DataContext = data;
                            }
                        }

                        if(row == null)
                        {
                            // It's possible we're in a SelectAll situation, in a VirtualCollection
                            // So if we have rows selected, and their data is null, and they're index matches our index
                            // Then assume that they should be selected. 
                            SelectedRowsCollection src = this.ColumnLayout.Grid.SelectionSettings.SelectedRows;
                            if (src.Count > index)
                            {
                                Row r = src[index];
                                if (r.RowType != RowType.GroupByRow && r.Data == null && r.Index == index)
                                {
                                    r.Data = data;
                                    row = r;
                                    if (r.Control != null)
                                        r.Control.DataContext = data;
                                }
                            }

                            if(row == null)
                                row = new Row(index, this, data);
                        }

						this._cachedRows.Add(data, row);
						this._duplicateObjectValidator.Add(data, index);
					}
					else
						row = new Row(index, this, data);

					if (row != null)
						this.ColumnLayout.Grid.OnInitializeRow(row);

                    row.MergeData = mri;

					return row;
				}
				else
				{
					GroupByRow row = null;

					object key = groupData.Value;
					if (key == null)
						key = "ig:XNull";

                    groupData.GroupBySummaries = this.GroupedColumn.SummaryColumnSettings.GroupBySummaryDefinitions;

					if (this._cachedGroupByRows.ContainsKey(key))
					{
						row = (GroupByRow)this._cachedGroupByRows[key];
						row.Manager = this;
						row.Index = index;
						row.Data = groupData.Value;
						row.GroupByData = groupData;

                        RowsManager childRowsManager = row.ChildRowsManager as RowsManager;

                        childRowsManager._deferClearGroupByCache = true;
						
                        // Make sure the itemSource is updated, so that it reflects any added or removed rows. 
						row.ItemSource = groupData.Records;

                        childRowsManager._deferClearGroupByCache = false;

						childRowsManager.InvalidateItemSource(false, true, true);

						if (row.IsExpanded)
							this.RegisterChildRowsManager(row.ChildRowsManager);
						return row;
					}

					row = new GroupByRow(index, this, groupData);
					this._cachedGroupByRows.Add(key, row);

					return row;
				}
			}
			return null;
		}

		#endregion // GetDataItem

		#region GenerateColumnForField

		/// <summary>
		/// Returns a column for the specified <see cref="DataField"/>.
		/// </summary>
		/// <param propertyName="field">A <see cref="DataField"/>.</param>
		/// <returns>A <see cref="ColumnBase"/> based off of the type of the <see cref="DataField"/></returns>
		protected virtual ColumnBase GenerateColumnForField(DataField field)
		{
			ColumnTypeMappingsCollection mapping = this.ColumnLayout.Grid.ColumnTypeMappings;

			ColumnBase column = null;

			if ((typeof(IEnumerable).IsAssignableFrom(field.FieldType) && !(typeof(String).IsAssignableFrom(field.FieldType))))
				column = this.ColumnLayout.Grid.ColumnLayouts.InternalFromKey(field.Name);

			if (column == null)
			{
				foreach (ColumnTypeMapping map in mapping)
				{
					if (map.DataType != null && map.ColumnType != null)
					{
						if (map.DataType == field.FieldType || (map.DataType.IsAssignableFrom(field.FieldType) && !(typeof(String).IsAssignableFrom(field.FieldType)) && field.FieldType.GetInterface("IDictionary", false) == null))
						{
							bool isNullable1 = map.DataType.Name.Contains("Nullable`1");
							bool isNullable2 = field.FieldType.Name.Contains("Nullable`1");

							if (isNullable1 == isNullable2)
							{
								column = map.ColumnType.GetConstructor(new Type[] { }).Invoke(null) as ColumnBase;
								break;
							}
						}
					}
				}
			}

			if (column == null)
				column = new TextColumn();

			return column;
		}

		#endregion // GenerateColumnForField

		#region ResetAddNewRows
		/// <summary>
		/// Assigns a new data object to the <see cref="AddNewRow"/> object.
		/// </summary>
		protected internal virtual void ResetAddNewRows(bool generateNewData)
		{
			if (generateNewData && this.ColumnLayout.AddNewRowSettings.AllowAddNewRowResolved != AddNewRowLocation.None)
			{
				object dataObject = this.GenerateNewObject(RowType.AddNewRow);

				if (this.AddNewRowBottom.Control != null)
				{
					RecyclingManager.Manager.ReleaseElement(this.AddNewRowBottom, this.ColumnLayout.Grid.Panel);
				}

				if (this.AddNewRowTop.Control != null)
				{
					RecyclingManager.Manager.ReleaseElement(this.AddNewRowTop, this.ColumnLayout.Grid.Panel);
				}

				this.AddNewRowBottom.SetData(dataObject);
				this.AddNewRowTop.SetData(dataObject);
			}
		}
		#endregion // ResetAddNewRows

		#region ResetFilterRows

		/// <summary>
		/// Clears the underlying dataobject for the FilterRow UI.
		/// </summary>
        protected internal virtual void ResetFilterRows(bool generateNewData)
		{
			FilterUIType filterUIType = this.ColumnLayout.FilteringSettings.AllowFilteringResolved;

            if (generateNewData && filterUIType != FilterUIType.None && filterUIType != FilterUIType.None)
			{
				object dataObject;

				if (this.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ChildBand)
				{
					dataObject = this.GenerateNewObject(RowType.FilterRow);
				}
				else
				{
					dataObject = this.ColumnLayout.ColumnLayoutFilteringObject;

					if (dataObject == null)
					{
						dataObject = this.ColumnLayout.ColumnLayoutFilteringObject = this.GenerateNewObject(RowType.FilterRow);
					}
				}

				if (this.FilterRowBottom.Control != null)
				{
					RecyclingManager.Manager.ReleaseElement(this.FilterRowBottom, this.ColumnLayout.Grid.Panel);
				}

				if (this.FilterRowTop.Control != null)
				{
					RecyclingManager.Manager.ReleaseElement(this.FilterRowTop, this.ColumnLayout.Grid.Panel);
				}

				this.FilterRowBottom.SetData(dataObject);
				this.FilterRowTop.SetData(dataObject);
			}
		}

		#endregion // ResetFilterRows

		#region GenerateNewObject
		/// <summary>		
		/// Creates a new object from the <see cref="DataManagerBase"/>
		/// </summary>
		/// <returns></returns>
		protected internal virtual object GenerateNewObject(RowType rowType)
		{
			object obj = null;
			if (this._dataManager != null)
			{
				this._newDataObjectRowType = rowType;
				obj = this._dataManager.GenerateNewObject();
			}
			return obj;
		}
		#endregion // GenerateNewObject

		#region InitializeData

		/// <summary>
		/// Looks at the data provided for the <see cref="RowsManager"/> and generates <see cref="ColumnBase"/> objects 
		/// if AutoGenerateColumns is true.
		/// </summary>
		protected virtual void InitializeData()
		{
			if (this._dataManager != null)
			{
				ColumnLayout colLayout = null;

				Type dataType = this._dataManager.CachedType;
				if (dataType != null)
					colLayout = this.ColumnLayout.Grid.ColumnLayouts.InternalFromType(dataType);

				if (colLayout == null)
					colLayout = this.ColumnLayout.Grid.ColumnLayouts.InternalFromKey(this.ColumnLayout.Key);

				if (colLayout != null && this.ColumnLayout != colLayout)
				{
					colLayout.Grid = this.ColumnLayout.Grid;
					this.ColumnLayout = colLayout;
				}

				ColumnLayoutAssignedEventArgs e = new ColumnLayoutAssignedEventArgs() { ColumnLayout = this.ColumnLayout, Level = this.Level, DataType = dataType, Key = this.ColumnLayout.Key, Rows = this._rows };
				this.ColumnLayout.Grid.OnColumnLayoutAssigned(e);

				if (e.ColumnLayout != null && e.ColumnLayout != this.ColumnLayout)
				{
					e.ColumnLayout.Grid = this.ColumnLayout.Grid;
					this.ColumnLayout = e.ColumnLayout;

                    if(string.IsNullOrEmpty(e.ColumnLayout.Key))
                        throw new EmptyColumnKeyException();
				}

				if (!this.ColumnLayout.Grid.ColumnLayouts.Contains(this.ColumnLayout))
					this.ColumnLayout.Grid.ColumnLayouts.AddItemLocally(this.ColumnLayout);

				if (!this.ColumnLayout.IsInitialized)
				{
					if (this.ColumnLayout.DataType == null)
						this.ColumnLayout.DataType = this.ItemsSource.GetType();

					this.ColumnLayout.ObjectDataType = this._dataManager.CachedType;
                    this.ColumnLayout.ObjectDataTypeInfo = this._dataManager.CachedTypedInfo;

					this.ColumnLayout.IsInitialized = true;

					IEnumerable<DataField> fields = this._dataManager.GetDataProperties();
					this.ColumnLayout.DataFields = fields;

					if (this.ColumnLayout.AutoGenerateColumnsResolved)
					{
						
						Collection<string> keysBeingUsed = new Collection<string>();
                        ReadOnlyKeyedColumnBaseCollection<ColumnBase> cols = this.ColumnLayout.Columns.AllColumns;
                        foreach (ColumnBase column in cols)
						{
							if (!string.IsNullOrEmpty(column.Key))
								keysBeingUsed.Add(column.Key);
						}

						
						foreach (DataField field in fields)
						{
							
                            if (field.AutoGenerate && !keysBeingUsed.Contains(field.Name))
							{
								ColumnBase column = this.GenerateColumnForField(field);

								if (column != null)
								{
                                    column.DataField = field;
									column.IsAutoGenerated = true;
									column.Key = field.Name;
                                    column.DataType = field.FieldType;

                                    //Fire off the Column Auto-Generated Event
                                    var eventArgs = new ColumnAutoGeneratedEventArgs() { Column = column };
                                    this.ColumnLayout.Grid.OnColumnAutoGenerated(eventArgs);
                                    
                                    //Copy the new column incase it changed.
                                    column = eventArgs.Column;
                                    
                                    //Check to make sure the user didn't null it out.
                                    if (column != null)
                                        this.ColumnLayout.Columns.Add(column);
                                        
								}
							}
						}
					}
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					else if (this.ColumnLayout.Grid.ShouldAddColumnLayouts(this.ColumnLayout))
					{
						ColumnLayoutCollection layouts = this.ColumnLayout.Grid.ColumnLayouts;
						foreach (DataField field in fields)
						{
							Type itemType = null;
							if (field.FieldType.IsGenericType)
							{
								Type[] types = field.FieldType.GetGenericArguments();
								if (types.Length > 0)
									itemType = types[0];
							}

							ColumnLayout columnLayout = null;

							if (itemType != null)
								columnLayout = layouts.InternalFromType(itemType);

							if (columnLayout == null)
								columnLayout = layouts.InternalFromKey(field.Name);

							if (columnLayout != null && this.ColumnLayout.Columns[columnLayout.Key] == null)
								this.ColumnLayout.Columns.Add(columnLayout);
						}

					}

					Collection<string> dataKeys = new Collection<string>();
					foreach (DataField field in fields)
					{
                        ColumnBase col = this.ColumnLayout.Columns[field.Name];
                        if (col != null)
                        {
                            col.DataType = field.FieldType;
                            col.DataField = field;
                            if (string.IsNullOrEmpty(col.HeaderText))
                                col.HeaderText = field.DisplayName;
                        }

						dataKeys.Add(field.Name);
					}

					this.ColumnLayout.Columns.DataType = dataType;
                         
                    




                    foreach (Column column in this.ColumnLayout.Columns.DataColumns)
                    {
                        if (ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ColumnLayout)
                        {
                            FilterOperand fo = column.FilterColumnSettings.FilteringOperandResolved;
                            if (fo != null)
                            {
                                if (fo.RequiresFilteringInput && column.FilterColumnSettings.FilterCellValue != null)
                                {
                                    object setValue = column.ResolveValue(column.FilterColumnSettings.FilterCellValue);
                                    if (setValue == column.FilterColumnSettings.FilterCellValue)
                                    {
                                        RowsFilter rf = ColumnLayout.FilteringSettings.RowFiltersCollection[column.Key];

                                        if (rf == null || rf.Conditions.Count == 0)
                                        {
                                            column.FilterColumnSettings.BuildFilters(true);
                                        }
                                    }
                                    else
                                        column.FilterColumnSettings.FilterCellValue = setValue;
                                }
                                else
                                {
                                    column.FilterColumnSettings.BuildFilters(false, false);
                                }
                            }
                        }
                        else
                        {
                            FilterRow filterRow = null;
                            if (this.ColumnLayout.FilteringSettings.AllowFilteringResolved == FilterUIType.FilterRowTop)
                            {
                                filterRow = this.FilterRowTop;
                            }
                            else if(this.ColumnLayout.FilteringSettings.AllowFilteringResolved == FilterUIType.FilterRowTop)
                            {
                                filterRow = this.FilterRowBottom;
                            }

                            if (filterRow != null)
                            {
                                FilterRowCell filterRowCell = filterRow.Cells[column.Key] as FilterRowCell;
                                if (filterRowCell != null)
                                {
                                    FilterOperand fo = filterRowCell.FilteringOperandResolved;
                                    if (fo != null)
                                    {
                                        if (fo.RequiresFilteringInput && filterRowCell.FilterCellValueResolved != null)
                                        {
                                            object setValue = column.ResolveValue(filterRowCell.FilterCellValueResolved);
                                            if (setValue == filterRowCell.FilterCellValueResolved)
                                                filterRow.BuildFilters(filterRowCell, filterRowCell.FilterCellValueResolved, false);
                                            else
                                                filterRowCell.FilterCellValue = setValue;
                                        }
                                        else
                                        {
                                            column.FilterColumnSettings.BuildFilters(false, false);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ReadOnlyKeyedColumnBaseCollection<ColumnBase> columns = this.ColumnLayout.Columns.AllColumns;

                    PropertyDescriptorCollection pdcs = this.DataManager.CachedTypedInfo.PropertyDescriptors;                    

                    foreach (ColumnBase column in columns)
					{
                        if (column.Key.Contains("[") && column.Key.Contains("]"))
                        {
                            try
                            {
                                System.Linq.Expressions.Expression expression = DataManagerBase.BuildPropertyExpressionFromPropertyName(column.Key, System.Linq.Expressions.Expression.Parameter(dataType, "param"));
								// don't set the datatype if it was already set
								if (expression != null && column.DataType == null)
                                    column.DataType = expression.Type;
                            }
                            catch (Exception)
                            {
                                // probably an illegal type, or the indexer is of type object and they want to access a property off of it. 
                                // If thats the case, then we can't support data interactions such as sorting, filtering or groupby
                            }
                            continue;
                        }


                        if (pdcs != null)
                        {
                            PropertyDescriptor pd = pdcs[column.Key];
                            if (pd != null)
                            {
                                column.DataType = pd.PropertyType;
                                continue;
                            }
                        }


						string[] keys = column.Key.Split('.');
						if (keys.Length > 0)
						{
							if (!dataKeys.Contains(keys[0]))
							{
								ColumnLayout layout = column as ColumnLayout;
								if (column.RequiresBoundDataKey)
								{
                                    if (layout == null || (!layout.IsDefinedGlobally && string.IsNullOrEmpty(layout.TargetTypeName)))
                                    {
                                        this.ColumnLayout.Grid.ThrowInvalidColumnKeyException(column.Key);
                                    }
								}
							}
							else
							{
								Type t = dataType;
								bool found = true;
								foreach (string key in keys)
								{
									PropertyInfo pi = t.GetProperty(key);
									if (pi == null)
									{
										ColumnLayout layout = column as ColumnLayout;
										if (column.RequiresBoundDataKey)
										{
											if (layout == null || (!layout.IsDefinedGlobally && string.IsNullOrEmpty(layout.TargetTypeName)))
                                                this.ColumnLayout.Grid.ThrowInvalidColumnKeyException(column.Key);
										}
										else
										{
											found = false;
											break;
										}
									}
                                    else
									{
									   t = pi.PropertyType;
								    }
								}
								if (found)
								{
									column.DataType = t;
								}
							}
						}
					}

                    this.ColumnLayout.IsXamlComplete = true;
				}

				this.InvalidateItemSource(false, true, true);

				ResetAddNewRows(true);

                ResetFilterRows(true);

                if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                    this.ColumnLayout.Grid.InvalidateScrollPanel(true);
			}
		}
		#endregion // InitializeData

		#region OnItemsSourceChanged

		/// <summary>
		/// Invoked when the the underlying ItemsSource property changes.
		/// </summary>
        protected virtual void OnItemsSourceChanged(bool invalidateSelectionAndActivation)
		{
			if (this._dataManager != null)
			{
				XamGrid grid = this.ColumnLayout.Grid;
				if (grid != null)
				{
                    if (invalidateSelectionAndActivation)
                    {
                        if (grid.ActiveCell != null)
                        {
                            if (grid.ActiveCell.Row.Level > this.Level)
                            {
                                grid.ActiveCell = null;
                            }
                            else if (grid.ActiveCell.Row.ColumnLayout == this.ColumnLayout)
                            {
                                if (grid.ActiveCell.Row.RowType == RowType.DataRow)
                                    grid.ActiveCell = null;
                            }
                        }

                        SelectionSettings selectionSettings = grid.SelectionSettings;
                        selectionSettings.SelectedCells.Clear();
                        selectionSettings.SelectedColumns.Clear();
                        selectionSettings.SelectedRows.Clear();
                    }

					if (grid.CurrentEditCell != null)
					{
						if (grid.CurrentEditCell.Row.ColumnLayout == this.ColumnLayout && grid.CurrentEditCell.Row.RowType == RowType.DataRow)
						{
							grid.ExitEditModeInternal(true);
						}
					}
					else if (grid.CurrentEditRow != null)
					{
						if (grid.CurrentEditRow.ColumnLayout == this.ColumnLayout && grid.CurrentEditRow.RowType == RowType.DataRow)
						{
							grid.ExitEditModeInternal(true);
						}
					}

                    if (this.ItemsSource != null)
                    {
                        Type t = DataManagerBase.ResolveItemType(this.ItemsSource);
                        if (t == this._dataManager.CachedType)
                        {
                            // Be sure to reset the DataManager, otherwise we could have a memory leak,
                            // as the DataManager tends to hold on to the data somehow.
                            this._dataManager.DataSource = null;
                            this.UnhookDataManager(true, true, false);
                            this.EnsureDataManager();

                            this.InvalidateRows();
                            this.InvalidateData();
                            return;
                        }
                    }
                    else
                    {
                        // Clear out the DataSource, otherwise, we could have a memory leak
                        this._dataManager.DataSource = this.ItemsSource;
                    }

					this._deferInvalidatingSort = true;
					grid.SummaryRowSettings.SummaryDefinitionCollection.Clear();
					

					this.ClearSpecializedRows();

					this.ColumnLayout.DefaultDataObject = null;

                    grid.ResetPanelRows();

                    grid.GroupBySettings.GroupByColumns.Clear();

					Collection<ColumnBase> columnsToRemove = new Collection<ColumnBase>();
					foreach (ColumnBase column in this.ColumnLayout.Columns.AllColumns)
					{
                        if (column.IsAutoGenerated)
                            columnsToRemove.Add(column);

                        else if(this.ItemsSource == null)
                        {
                            ColumnLayout layout = column as ColumnLayout;

                            // The ColumnLayout won't be released, b/c its not autogenerated...
                            // So instead, we need to make sure all of it's RowsManagers/DataManagers get their itemsSource reset
                            // specifically if they're INotifyCollectionChanged, so that there isn't a memory leak. 
                            if (layout != null)
                                layout.OnColumnLayoutReset();
                        }

						Column col = column as Column;

                        if (col != null)
                        {
                            col.IsInitialAutoSet = false;
                            col.CachedPropertyReadOnly = null;
                            col.FilterColumnSettings.FilterCellValue = null;

                            if (!col.IsAutoGenerated)
                            {
                                col.SummaryColumnSettings.RepopulateSummaryDefinitionCollection();
                            }
                        }
					}

                    



                    grid.FilteringSettings.RowFiltersCollection.Clear();

					foreach (ColumnBase column in columnsToRemove)
					{
						ColumnLayout cl = column as ColumnLayout;
						if (cl != null)
						{
                            if (cl.IsDefinedGlobally)
                            {
                                continue;
                            }
                            else
                            {
                                if (grid.ColumnLayouts.Contains(cl))
                                    grid.ColumnLayouts.Remove(cl);
                            }
						}

						this.ColumnLayout.Columns.Remove(column);
					}
                    
                    



                    this.UnhookDataManager(true, true, true);
					this.ColumnLayout.IsInitialized = false;
					this.ColumnLayout.DataFields = null;

					this._deferInvalidatingSort = false;

					grid.InvalidateScrollPanel(true, true);
				}
			}
            else if (this.ItemsSource != null)
			{
				this.EnsureDataManager();
                bool clearSummaryCollection = this.SummaryDefinitionCollectionResolved.Count == 0;
				foreach (ColumnBase column in this.ColumnLayout.Columns.AllColumns)
				{
					Column col = column as Column;
                    if (col != null)
                    {
                        col.IsInitialAutoSet = false;
                        if (clearSummaryCollection)
                        {
                            col.SummaryColumnSettings.RepopulateSummaryDefinitionCollection();
                        }
                    }
				}
			}

			if (this.ColumnLayout.Grid != null)
				this.InvalidateGroupBy(false);
		}

		#endregion // OnItemsSourceChanged

        #region AttachDetachPropertyChanged

        internal void AttachDetachPropertyChanged(bool attach)
		{
            int count = _attachedPropertyChangedTargets.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                this.AttachDetachPropertyChanged(_attachedPropertyChangedTargets[i].Target as INotifyPropertyChanged, false);
            }

            if (!attach)
                _attachedPropertyChangedTargets.Clear();

            if (attach)
            {
                if (ItemsSource != null)
                {
                    foreach (object o in this.ItemsSource)
                    {
                        INotifyPropertyChanged propChanged = o as INotifyPropertyChanged;
                        if (propChanged != null)
                        {
                            this.AttachDetachPropertyChanged(propChanged, true);
                        }
                    }
                }
            }            
		}

        internal void AttachDetachPropertyChanged(INotifyPropertyChanged propertyChangedDataObject, bool attach)
        {
            if (propertyChangedDataObject != null)
            {
                propertyChangedDataObject.PropertyChanged -= PropertyChangedDataObject_PropertyChanged;

                if (attach && this.GroupedColumn == null)
                {
                    propertyChangedDataObject.PropertyChanged += PropertyChangedDataObject_PropertyChanged;
                    _attachedPropertyChangedTargets.Add(new WeakReference(propertyChangedDataObject));
                }
            }
        }

        internal void AttachDetachPropertyChanged()
        {
            int count = _attachedPropertyChangedTargets.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                this.AttachDetachPropertyChanged(_attachedPropertyChangedTargets[i].Target as INotifyPropertyChanged , false);
            }
            _attachedPropertyChangedTargets.Clear();
        }

        #endregion // AttachDetachPropertyChanged

        #region RegisterTopRow

        /// <summary>
		/// Registers a <see cref="RowBase"/> as a row that should be displayed above all other rows, such as the <see cref="HeaderRow"/>
		/// </summary>
		/// <param propertyName="row"></param>
		protected virtual void RegisterTopRow(RowBase row)
		{
			if (!this.RegisteredTopRows.Contains(row))
			{
				if (row.IsStandAloneRow)
					this._displayableRowCount++;
				this.RegisteredTopRows.Add(row);
				this.RegisteredTopRows.Sort(this._fixedRowsOrderComparer);
			}
		}

		#endregion // RegisterTopRow

		#region RegisterBottomRow

		/// <summary>
		/// Registers a <see cref="RowBase"/> as a row that should be displayed below all other rows, such as the <see cref="FooterRow"/>
		/// </summary>
		/// <param propertyName="row"></param>
		protected virtual void RegisterBottomRow(RowBase row)
		{
			if (!this.RegisteredBottomRows.Contains(row))
			{
				if (row.IsStandAloneRow)
					this._displayableRowCount++;

				this.RegisteredBottomRows.Add(row);
				this.RegisteredBottomRows.Sort(this._fixedRowsOrderComparer);
			}
		}

		#endregion // RegisterBottomRow

		#region UnregisterTopRow

		/// <summary>
		/// Unregisters a <see cref="RowBase"/> that was registered to be displayed above of all other rows..
		/// </summary>
		/// <param propertyName="row"></param>
		protected virtual void UnregisterTopRow(RowBase row)
		{
			if (row.IsStandAloneRow && this.RegisteredTopRows.Contains(row))
				this._displayableRowCount--;

			this.RegisteredTopRows.Remove(row);
		}

		#endregion // UnregisterTopRow

		#region UnregisterBottomRow

		/// <summary>
		/// Unregisters a <see cref="RowBase"/> that was registered to be displayed below all other rows.
		/// </summary>
		/// <param propertyName="row"></param>
		protected virtual void UnregisterBottomRow(RowBase row)
		{
			if (row.IsStandAloneRow && this.RegisteredBottomRows.Contains(row))
				this._displayableRowCount--;

			this.RegisteredBottomRows.Remove(row);
		}

		#endregion // UnregisterBottomRow

		#region InvalidateHeaderRowVisibility

		/// <summary>
		/// Determines if the <see cref="HeaderRow"/> should be visible, and registers/unregisters it accordingly.
		/// </summary>
		protected virtual void InvalidateHeaderRowVisibility()
		{
            if (this.Level == 0 && this.ColumnLayout.HeaderVisibilityResolved == Visibility.Visible)
				this.RegisterTopRow(this.HeaderRow);
			else
				this.UnregisterTopRow(this.HeaderRow);
		}

		#endregion // InvalidateHeaderRowVisibility

		#region InvalidateFooterRowVisibility

		/// <summary>
		/// Determines if the <see cref="FooterRow"/> should be visible, and registers/unregisters it accordingly.
		/// </summary>
		protected virtual void InvalidateFooterRowVisibility()
		{
			if (this.ColumnLayout.FooterVisibilityResolved == Visibility.Visible)
				this.RegisterBottomRow(this.FooterRow);
			else
				this.UnregisterBottomRow(this.FooterRow);
		}

		#endregion // InvalidateFooterRowVisibility

		#region InvalidateAddNewRowVisibility
		/// <summary>
		/// Determines if the <see cref="AddNewRow"/> should be visible, and registers/unregisters it accordingly.
		/// </summary>
		protected virtual void InvalidateAddNewRowVisibility(bool generateNewData)
		{
			if (this.GroupByLevel != -1 || (this.ParentRow != null && this.ParentRow is GroupByRow))
			{
				this.UnregisterTopRow(this.AddNewRowTop);
				this.UnregisterBottomRow(this.AddNewRowBottom);
			}
			else
			{
				AddNewRowSettingsOverride ars = this.ColumnLayout.AddNewRowSettings;

				switch (ars.AllowAddNewRowResolved)
				{
					case (AddNewRowLocation.None):
						{
							this.UnregisterTopRow(this.AddNewRowTop);
							this.UnregisterBottomRow(this.AddNewRowBottom);
							break;
						}
					case (AddNewRowLocation.Both):
						{
                            this.ResetAddNewRows(generateNewData);
							this.RegisterBottomRow(this.AddNewRowBottom);
							this.RegisterTopRow(this.AddNewRowTop);
							break;
						}
					case (AddNewRowLocation.Top):
						{
                            this.ResetAddNewRows(generateNewData);
							this.RegisterTopRow(this.AddNewRowTop);
							this.UnregisterBottomRow(this.AddNewRowBottom);
							break;
						}
					case (AddNewRowLocation.Bottom):
						{
                            this.ResetAddNewRows(generateNewData);
							this.RegisterBottomRow(this.AddNewRowBottom);
							this.UnregisterTopRow(this.AddNewRowTop);
							break;
						}
				}
			}
		}
		#endregion // InvalidateAddNewRowVisibility

		#region InvalidateSummaryRowVisibility
		/// <summary>
		/// Determines if the <see cref="SummaryRow"/> should be visible, and registers/unregisters it accordingly.
		/// </summary>
		protected virtual void InvalidateSummaryRowVisibility()
		{
			if (this.GroupByLevel != -1)
			{
				this.UnregisterTopRow(this.SummaryRowTop);
				this.UnregisterBottomRow(this.SummaryRowBottom);
			}
			else
			{
				SummaryRowSettingsOverride ars = this.ColumnLayout.SummaryRowSettings;

				switch (ars.AllowSummaryRowResolved)
				{
					case (SummaryRowLocation.None):
						{
							this.UnregisterTopRow(this.SummaryRowTop);
							this.UnregisterBottomRow(this.SummaryRowBottom);
							break;
						}
					case (SummaryRowLocation.Top):
						{
							this.RegisterTopRow(this.SummaryRowTop);
							this.UnregisterBottomRow(this.SummaryRowBottom); break;
						}
					case (SummaryRowLocation.Bottom):
						{
							this.UnregisterTopRow(this.SummaryRowTop);
							this.RegisterBottomRow(this.SummaryRowBottom); break;
						}
					case (SummaryRowLocation.Both):
						{
							this.RegisterTopRow(this.SummaryRowTop);
							this.RegisterBottomRow(this.SummaryRowBottom); break;
						}
				}
			}
		}
		#endregion // InvalidateSummaryRowVisibility

		#region InvalidateFilterRowVisibility
		/// <summary>
		/// Determines if the <see cref="FilterRow"/> should be visible, and registers/unregisters it accordingly.
		/// </summary>
		protected virtual void InvalidateFilterRowVisibility(bool generateNewData)
		{
			if (this.GroupByLevel != -1)
			{
				this.UnregisterTopRow(this.FilterRowTop);
				this.UnregisterBottomRow(this.FilterRowBottom);
			}
			else
			{
				FilteringSettingsOverride ars = this.ColumnLayout.FilteringSettings;

				switch (ars.AllowFilteringResolved)
				{
					case (FilterUIType.FilterRowTop):
						{
                            this.ResetFilterRows(generateNewData);
							this.RegisterTopRow(this.FilterRowTop);
							this.UnregisterBottomRow(this.FilterRowBottom);
							break;
						}
					case (FilterUIType.FilterRowBottom):
						{
                            this.ResetFilterRows(generateNewData);
							this.RegisterBottomRow(this.FilterRowBottom);
							this.UnregisterTopRow(this.FilterRowTop);
							break;
						}
					default:
						{
							this.UnregisterTopRow(this.FilterRowTop);
							this.UnregisterBottomRow(this.FilterRowBottom);
							break;
						}
				}
			}
		}
		#endregion // InvalidateFilterRowVisibility

		#region InvalidatePagerRowVisibility
		/// <summary>
		/// Determines if the <see cref="PagerRow"/> should be visible, and registers/unregisters it accordingly.
		/// </summary>
		protected virtual void InvalidatePagerRowVisibility()
		{
			PagerSettingsOverride pso = this.ColumnLayout.PagerSettings;

			if (this.ColumnLayout.Grid != null && (!this.ColumnLayout.Grid.PagerSettings.DisplayPagerWhenOnlyOnePage && this.PageCount <= 1))
			{
				this.UnregisterTopRow(this.PagerRowTop);
				this.UnregisterBottomRow(this.PagerRowBottom);
			}
			else
			{
				switch (pso.AllowPagingResolved)
				{
					case (PagingLocation.None):
					case (PagingLocation.Hidden):
						{
							this.UnregisterTopRow(this.PagerRowTop);
							this.UnregisterBottomRow(this.PagerRowBottom);
							break;
						}
					case (PagingLocation.Both):
						{
							this.RegisterBottomRow(this.PagerRowBottom);
							this.RegisterTopRow(this.PagerRowTop);
							break;
						}
					case (PagingLocation.Top):
						{
							this.RegisterTopRow(this.PagerRowTop);
							this.UnregisterBottomRow(this.PagerRowBottom);
							break;
						}
					case (PagingLocation.Bottom):
						{
							this.RegisterBottomRow(this.PagerRowBottom);
							this.UnregisterTopRow(this.PagerRowTop);
							break;
						}
				}
			}
		}
		#endregion

		#region ValidatePagerInformation
		/// <summary>
		/// Ensures the Pager control's current page index is set correctly after changing the data source.
		/// </summary>
		protected virtual void ValidatePagerInformation()
		{
			if (this.Rows.Count == 0)
			{
				this.CurrentPageIndex = 1;
			}
			else if (this.CurrentPageIndex > this.PageCount)
			{
				this.CurrentPageIndex = this.PageCount;
			}
		}
		#endregion // ValidatePagerInformation

		#region InvalidateSort
		/// <summary>
		/// Sorts the rows based on the values provided by the ColumnLayout object.
		/// </summary>
		protected virtual void InvalidateSort()
		{
			if (!this._deferInvalidatingSort)
			{
				DataManagerBase manager = this.DataManager;
				if (manager != null)
				{
					bool update = false;

					SortedColumnsCollection sortedCols = this.ColumnLayout.SortingSettings.SortedColumns;
					int sortedColsCount = sortedCols.Count;
					int managerSortCount = manager.Sort.Count;
					manager.Sort.Clear();

                    List<Column> sortedOtherCols = new List<Column>();

					if (manager.GroupByObject != null)
					{
						if (this.GroupedColumn.IsSorted != SortDirection.None)
							manager.GroupBySortAscending = (this.GroupedColumn.IsSorted == SortDirection.Ascending);
						else
						{
							this.GroupedColumn.SetSortedColumnStateInternally(manager.GroupBySortAscending ? SortDirection.Ascending : SortDirection.Descending, true);
						}

						update = true;

						UnboundColumn unboundColumn = this.GroupedColumn as UnboundColumn;
						if (unboundColumn != null)
						{
							manager.GroupBySortContext = SortContext.CreateGenericSort(manager.CachedTypedInfo, manager.GroupBySortAscending, this.GroupedColumn.SortComparer, this.GroupedColumn.ValueConverter, this.GroupedColumn.ValueConverterParameter);
						}
						else
						{
                            manager.GroupBySortContext = SortContext.CreateGenericSort(manager.CachedTypedInfo, this.GroupedColumn.Key, manager.GroupBySortAscending, this.GroupedColumn.AllowCaseSensitiveSort, this.GroupedColumn.SortComparer);
						}

                        sortedOtherCols.Add(this.GroupedColumn);
					}
                    else if (manager.MergeDataContexts.Count > 0)
                    {
                        ReadOnlyCollection<Column> mergedCols = this.ColumnLayout.Grid.GroupBySettings.GroupByColumns[this.ColumnLayout];
                        int index = 0;
                        foreach (Column col in mergedCols)
                        {
                            MergedDataContext mdc = manager.MergeDataContexts[index];
                            
                            if (col.IsSorted != SortDirection.None)
                                mdc.SortAscending = (col.IsSorted == SortDirection.Ascending);
                            else
                            {
                                col.SetSortedColumnStateInternally(mdc.SortAscending ? SortDirection.Ascending : SortDirection.Descending, true);
                            }

                            update = true;

                            UnboundColumn unboundColumn = col as UnboundColumn;
                            if (unboundColumn != null)
                            {
                                mdc.SortContext = SortContext.CreateGenericSort(manager.CachedTypedInfo, mdc.SortAscending, col.SortComparer, col.ValueConverter, col.ValueConverterParameter);
                            }
                            else
                            {
                                mdc.SortContext = SortContext.CreateGenericSort(manager.CachedTypedInfo, col.Key, mdc.SortAscending, col.AllowCaseSensitiveSort, col.SortComparer);
                            }

                            sortedOtherCols.Add(col);

                            index++;
                        }
                    }

					if (sortedColsCount > 0)
					{
						for (int i = 0; i < sortedColsCount; i++)
						{
							Column c = sortedCols[i];
							if (!sortedOtherCols.Contains(c))
							{
								UnboundColumn unboundColumn = c as UnboundColumn;
								if (unboundColumn != null)
                                    manager.Sort.Add(SortContext.CreateGenericSort(manager.CachedTypedInfo, c.IsSorted == SortDirection.Ascending, c.SortComparer, unboundColumn.ValueConverter, unboundColumn.ValueConverterParameter));
								else
                                    manager.Sort.Add(SortContext.CreateGenericSort(manager.CachedTypedInfo, c.Key, c.IsSorted == SortDirection.Ascending, c.AllowCaseSensitiveSort, c.SortComparer));
							}

							update = true;
						}
					}

					if (managerSortCount != sortedColsCount)
						update = true;

					if (update)
						manager.UpdateData();
				}
			}
		}
		#endregion // InvalidateSort

		#region InvalidateFiltering
		/// <summary>
		/// Filters the data based on it's current settings.
		/// </summary>
		protected virtual void InvalidateFiltering()
		{
			DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                manager.Filters = this.RowFiltersCollectionResolved;

                // if we're filtering, then exit edit mode, otherwise we could have validation errors
                // and be stuck in edit mode. 
                FilterRowCell filterRowCell = this.ColumnLayout.Grid.CurrentEditCell as FilterRowCell;
                FilterRow filterRow = this.ColumnLayout.Grid.CurrentEditRow as FilterRow;
                if (filterRow == null && filterRowCell == null)
                    this.ColumnLayout.Grid.ExitEditMode(true);

                this.InvalidatePager();

                XamGrid grid = this.ColumnLayout.Grid;
                if (grid.ConditionalFormattingSettings.AllowConditionalFormatting)
                {
                    if (grid.Panel != null)
                        grid.Panel.ResetDataRows();
                }
            }
		}
		#endregion // InvalidateFiltering

		#region InvalidateSummaries

		/// <summary>
		/// Sums the data based on it's current settings.
		/// </summary>
		protected virtual void InvalidateSummaries()
		{
			DataManagerBase manager = this.DataManager;

			if (manager != null)
			{
				manager.SummaryExecution = this.ColumnLayout.SummaryRowSettings.SummaryExecutionResolved;

				manager.Summaries = this.SummaryDefinitionCollectionResolved;

				manager.SummaryResultCollection = this.SummaryResultCollectionInternal;

				this.ColumnLayout.Grid.ResetPanelRows();

				this.InvalidateRows();
			}
		}

		#endregion // InvalidateSummaries

		#region InvalidateConditionalFormatting

		/// <summary>
		/// Used to invalidate due to conditional formatting.
		/// </summary>
		protected virtual void InvalidateConditionalFormatting()
		{
			DataManagerBase manager = this.DataManager;

            if (manager != null)
            {
                if (this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
                    manager.ConditionalFormattingRules = this.ConditionalFormatProxyCollection.GenerateIRuleCollection();
                else
                    manager.ConditionalFormattingRules = null;

                manager.UpdateData();
            }
		}

		#endregion // InvalidateConditionalFormatting

		#region ResolveGroupByLevel

		/// <summary>
		/// Walks up the <see cref="RowsManager"/> chain and checks to see if there are any other RowsManager above this level
		/// that have a GroupByLevel.
		/// </summary>
		/// <param propertyName="layout"></param>
		/// <returns></returns>
		protected internal virtual int ResolveGroupByLevel(ColumnLayout layout)
		{
			int level = 0;

			if (this.ParentRow != null)
			{
				RowsManager parentManager = this.ParentRow.Manager as RowsManager;
				if (parentManager != null && parentManager.ColumnLayout == layout)
				{
					level += parentManager.ResolveGroupByLevel(layout) + 1;
				}
			}

			return level;
		}

		#endregion // ResolveGroupByLevel

		#region InvalidateGroupBy

		/// <summary>
		/// Validates whether a <see cref="RowsManager"/> needs to group its rows.
		/// </summary>
		protected virtual void InvalidateGroupBy(bool reset)
		{
			if (this.ColumnLayout != null && this._dataManager != null)
			{
                this._supressCFUpdating = true; 

				ReadOnlyCollection<Column> groupByColumns = this.ColumnLayout.Grid.GroupBySettings.GroupByColumns[this.ColumnLayout];

                // Is the operation GroupByRows or MergedCells?
                if (this.ColumnLayout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.GroupByRows)
                {
                    int gbLevel = (this.GroupByLevel == -1) ? this.ResolveGroupByLevel(this.ColumnLayout) : this.GroupByLevel;
                    if (groupByColumns.Count > gbLevel)
                    {
                        Column gbc = groupByColumns[gbLevel];
                        DataManagerBase manager = this.DataManager;
                        if (this.GroupedColumn != gbc || (reset && manager != null))
                        {
                            if (this.GroupedColumn != null)
                            {
                                this.GroupByLevel = -1;
                                if (!this.GroupedColumn.IsGroupBy)
                                    this.GroupedColumn.SetSortedColumnStateInternally(SortDirection.None, true);
                                this.GroupedColumn = null;


                                if (manager != null)
                                    manager.GroupByObject = null;                                

                            if (!(this.ColumnLayout.Grid.ActiveCell is FilterRowCell ))
							this.ColumnLayout.Grid.ActiveCell = null;

                                if (!this._deferClearGroupByCache)
                                {
                                    foreach (KeyValuePair<object, Row> kvp in this._cachedGroupByRows)
                                    {
                                        // NZ 24 Feb 2012 TFS101699
                                        kvp.Value.ChildRowsManager.UnregisterRowsManager(true, true, false);
                                    }
                                    this._cachedGroupByRows.Clear();
                                }
                            }

                            this.GroupedColumn = gbc;
                            this.GroupByLevel = gbLevel;

                            if (manager != null)
                            {
                                // Make sure we don't have any MDCs
                                manager.MergeDataContexts.Clear();

                                if (this.GroupedColumn is UnboundColumn)
                                {
                                    manager.GroupByObject = (GroupByContext.CreateGenericCustomGroup(manager.CachedTypedInfo, this.GroupedColumn.GroupByComparer, this.GroupedColumn.ValueConverter, this.GroupedColumn.ValueConverterParameter));
                                }
                                else
                                {
                                    manager.GroupByObject = (GroupByContext.CreateGenericCustomGroup(manager.CachedTypedInfo, this.GroupedColumn.Key, this.GroupedColumn.GroupByComparer));
                                }

                                manager.GroupBySortAscending = true;
                            }
                            else
                                this.GroupedColumn = null;

                            this.FooterSupported = this.HeaderSupported = false;

                            

                            this.OverrideHorizontalMax = -1;

                            this.InvalidateSort();

                            this.InvalidatePagerRowVisibility();
                        }
                    }
                    else if (this.GroupByLevel != -1)
                    {
                        this.GroupByLevel = -1;
                        if (this.GroupedColumn != null)
                        {
                            // Only unsort, if the column is definitely not grouped.
                            // This could simply be a case where another column is being ungrouped, and this column is 
                            // moving to another rows manager. 
                            if (!this.GroupedColumn.IsGroupBy)
                                this.GroupedColumn.SetSortedColumnStateInternally(SortDirection.None, true);

                            this.GroupedColumn = null;
                        }

                        DataManagerBase manager = this.DataManager;
                        if (manager != null)
                        {
                            manager.GroupBySortContext = null;
                            manager.GroupByObject = null;
                            manager.UpdateData();
                        }

                        this.ColumnLayout.Grid.ActiveCell = null;

                        if (!this._deferClearGroupByCache)
                        {
                            foreach (KeyValuePair<object, Row> kvp in this._cachedGroupByRows)
                            {
                                kvp.Value.ChildRowsManager.UnregisterRowsManager(true, true, false);
                            }

                            this._cachedGroupByRows.Clear();
                        }

                        this.FooterSupported = this.HeaderSupported = true;

                        this.InvalidatePagerRowVisibility();
                    }
                }
                else // MergedCells GroupBy logic
                {
                    bool updateNeeded = false;
                    bool forceUpdate = false;

                    DataManagerBase dmb = this.DataManager;

                    if (dmb != null)
                    {
                        if (dmb.GroupByObject != null)
                        {
                            this.GroupByLevel = -1;
                            this.GroupedColumn = null;

                            dmb.GroupBySortContext = null;
                            dmb.GroupByObject = null;
                            forceUpdate = true;
                        }

                        // First check if we need to remove any sorted columns.
                        foreach (MergedDataContext mdc in dmb.MergeDataContexts)
                        {
                            Column col = (Column)mdc.MergedObject;

                            if (!col.IsGroupBy)
                                col.SetSortedColumnStateInternally(SortDirection.None, true);

                            updateNeeded = true;
                        }


                        dmb.MergeDataContexts.Clear();

                        if (dmb.CachedType != null)
                        {
                            // Rebuild the MDCs
                            foreach (Column col in groupByColumns)
                            {
                                MergedDataContext mdc = null;
                                if (col is UnboundColumn)
                                {
                                    mdc = MergedDataContext.CreateGenericCustomMerge(dmb.CachedTypedInfo, col.GroupByComparer, col.ValueConverter, col.ValueConverterParameter);
                                }
                                else
                                {
                                    mdc = MergedDataContext.CreateGenericCustomMerge(dmb.CachedTypedInfo, col.Key, col.GroupByComparer);
                                }

                                mdc.MergedObject = col;
                                mdc.SortAscending = true;

                                dmb.MergeDataContexts.Add(mdc);

                                updateNeeded = false;
                            }
                        }

                        this.InvalidateSort();

                        if (updateNeeded || forceUpdate)
                            dmb.UpdateData();
                    }
                }

                this._supressCFUpdating = false;
			}
		}

		#endregion // InvalidateGroupBy

		#region CreateItem
		/// <summary>
		/// Creates a new row object 
		/// </summary>
		/// <param propertyName="data"></param>
		/// <param propertyName="dataManager"></param>
		/// <returns></returns>
		protected internal virtual Row CreateItem(object data, DataManagerBase dataManager)
		{
			Row r = null;
			if (dataManager != null)
			{
				if (data == null)
				{
					throw new NullDataException();
				}

				if (!_dataManager.CachedType.IsAssignableFrom(data.GetType()))
				{
					throw new DataTypeMismatchException(string.Format(CultureInfo.InvariantCulture, SRGrid.GetString("DataTypeMismatchExceptionVerbose"), data.GetType(), dataManager.CachedType));
				}
				r = new Row(-1, this, data);
			}
			return r;
		}

		/// <summary>
		/// Creates a new row object 
		/// </summary>
		/// <returns></returns>
		protected internal virtual Row CreateItem()
		{
			Row r = null;
			DataManagerBase dm = this.DataManager;
			if (dm != null)
				r = this.CreateItem(dm.GenerateNewObject(), dm);
			return r;
		}

		/// <summary>
		/// Creates a new row object 
		/// </summary>
		/// <param propertyName="data"></param>
		/// <returns></returns>
		protected internal virtual Row CreateItem(object data)
		{
			return this.CreateItem(data, this.DataManager);
		}
		#endregion // CreateItem

		#region InsertItem
		/// <summary>
		/// Inserts a row at a given index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="insertedObject"></param>
		protected internal virtual void InsertItem(int index, Row insertedObject)
		{
			DataManagerBase dm = this.DataManager;

			if (insertedObject.ColumnLayout != this.ColumnLayout)
			{
				throw new ColumnLayoutException();
			}

			if (insertedObject.Manager != this)
			{
				insertedObject.Manager = this;
			}
			XamGrid grid = this.ColumnLayout.Grid;

			if (!grid.OnRowAdding(insertedObject, index))
			{
				this._supressInvalidateRows = true;

				dm.InsertRecord(index, insertedObject.Data);

				if (insertedObject.ChildRowsManager != null)
					insertedObject.ChildRowsManager.ColumnLayout = this.ColumnLayout;

				if (!this._cachedRows.ContainsKey(insertedObject.Data))
					this._cachedRows.Add(insertedObject.Data, insertedObject);

				this._supressInvalidateRows = false;

				this.InvalidateRows();

				// Ensures the object is added correctly to the underlying rows collection
				insertedObject = (Row)this.Rows[index];

				grid.OnRowAdded(insertedObject);

				if (this.ColumnLayout.Grid != null)
					this.ColumnLayout.Grid.InvalidateScrollPanel(true, false);


			}
		}
		#endregion // InsertItem

		#region AddItem
		/// <summary>
		/// Adds a row to the collection.
		/// </summary>
		/// <param propertyName="addedObject"></param>
		protected internal virtual void AddItem(Row addedObject)
		{
			DataManagerBase dm = this.DataManager;

			if (addedObject.ColumnLayout != this.ColumnLayout)
			{
				throw new ColumnLayoutException();
			}

			if (addedObject.Manager != this)
			{
				addedObject.Manager = this;
			}

			XamGrid grid = this.ColumnLayout.Grid;

			if (!grid.OnRowAdding(addedObject, dm.RecordCount))
			{
				this._supressInvalidateRows = true;

				dm.AddRecord(addedObject.Data);

				if (addedObject.ChildRowsManager != null)
					addedObject.ChildRowsManager.ColumnLayout = this.ColumnLayout;

				if (!this._cachedRows.ContainsKey(addedObject.Data))
					this._cachedRows.Add(addedObject.Data, addedObject);

				this._supressInvalidateRows = false;

				this.InvalidateRows();

				int index = dm.ResolveIndexForRecord(addedObject.Data);
				if (index != -1)
				{
					// Ensures the object is added correctly to the underlying rows collection
					addedObject = (Row)this.Rows[index];
				}

				grid.OnRowAdded(addedObject);

				if (this.ColumnLayout.Grid != null)
					this.ColumnLayout.Grid.InvalidateScrollPanel(true, false);

			}
		}
		#endregion // AddItem

		#region RemoveItem
		/// <summary>
		/// Removes a row from the underlying ItemSource
		/// </summary>
		/// <param propertyName="removedObject"></param>
		/// <returns>true if the row is removed.</returns>
		protected bool RemoveItem(Row removedObject)
		{
			return this.RemoveItem(removedObject, true, this.DataManager);
		}

		/// <summary>
		/// Removes a row from the underlying ItemSource
		/// </summary>
		/// <param name="removedObject">The row to remove</param>
		/// <param name="invalidate">Whether the rows of the manager should be refreshed</param>
		/// <param name="manager">The Manager that should be performing the removal.</param>
		/// <returns></returns>
		protected virtual bool RemoveItem(Row removedObject, bool invalidate, DataManagerBase manager)
		{
			if (removedObject == null)
				return false;

            if (removedObject.ParentRow is GroupByRow)
            {
                RowBase parent = removedObject.ParentRow;
                RowsManagerBase parentManger = null;
                while (parent is GroupByRow)
                {
                    parentManger = parent.Manager;
                    parent = parentManger.ParentRow;
                }

                RowsManager rm = parentManger as RowsManager;
                if (rm != null)
                    manager = rm.DataManager;
            }

			XamGrid grid = this.ColumnLayout.Grid;

			if (grid.OnRowDeleting(removedObject))
			{
				return false;
			}

            // If the row or a cell belonging to this row is in edit-mode - force ExitEditMode,
            // because if there is a validation error we might be left in an incorrect state.
            if ((grid.CurrentEditCell != null && grid.CurrentEditCell.Row == removedObject) || grid.CurrentEditRow == removedObject)
		    {
		        grid.ExitEditModeInternal(true, true);
		    }
		    
			this._deferItemSourceInvalidation = true;
			manager.RemoveRecord(removedObject.Data);
			this._deferItemSourceInvalidation = false;

			grid.OnRowDeleted(removedObject);

			if (removedObject.ChildRowsManager != null)
				removedObject.ChildRowsManager.ColumnLayout = null;

			this._cachedRows.Remove(removedObject.Data);

            if (invalidate)
            {
                this.InvalidateItemSource(true, true, true);
            }

			return true;
		}

		#endregion // RemoveItem

		#region RemoveRange
		/// <summary>
		/// Removes the specified rows from the collection.
		/// </summary>
		/// <param propertyName="itemsToRemove"></param>
		protected virtual void RemoveRange(IList<Row> itemsToRemove)
		{
			if (itemsToRemove == null || itemsToRemove.Count == 0)
				return;

			DataManagerBase manager = this.DataManager;

            foreach (Row r in itemsToRemove)
            {
                if(r.CanBeDeleted)
                    this.RemoveItem(r, false, manager);
            }

			this.InvalidateItemSource(true, true, true);

		}
		#endregion // RemoveRange

		#region InvalidateData

		/// <summary>
		/// Triggers all Data operations such as sorting and GroupBy to be invalidated. 
		/// </summary>
		protected override void InvalidateData()
		{
            bool setDeferFlagToFalse = true;
			DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                if (manager.Defer == false)
                    manager.Defer = true;
                else
                    setDeferFlagToFalse = false;
                manager.Reset();

                if (this.Level == 0 && this.ParentRow != null && this.ParentRow.RowType == RowType.GroupByRow)
                    manager.CachedTypedInfo = ((RowsManager)this.ParentRow.Manager).DataManager.CachedTypedInfo;
            }

			this.InvalidateGroupBy(true);
			this.InvalidateSort();
			this.InvalidateFiltering();
			this.InvalidateConditionalFormatting();
			this.InvalidatePager();

            if (manager != null)
            {
                if (setDeferFlagToFalse)
                    manager.Defer = false;
                manager.UpdateData();

                



                if (!manager.SupportsChangeNotification)
                {
                    this.InvalidateRows();
                    this.InvalidatePagerRowVisibility();
                }
            }
		}

		#endregion // InvalidateData

		#region GetDistinctItemsForColumn

        /// <summary>
        /// Generates an <see cref="IList"/> of unique elements from the values available in the column.
        /// </summary>
        /// <param name="cellBase"></param>
        /// <param name="respectPreviousFilters"></param>
        /// <returns></returns>
        protected internal IList GetDistinctItemsForColumn(CellBase cellBase, bool respectPreviousFilters)
        {
            DataManagerBase manager = this.DataManager;

            bool caseSensitive = cellBase.Column.FilterColumnSettings.FilterCaseSensitive;

            InformationContext ic = InformationContext.CreateGenericInformationContext(manager.CachedTypedInfo, cellBase.Column.Key, true, caseSensitive, null, cellBase.Column is DateColumn);

            IList q;

            if (respectPreviousFilters && manager.SortedFilteredDataSource != null)
            {
                q = ic.GetDistinctValues(manager.SortedFilteredDataSource);
            }
            else
            {
                q = ic.GetDistinctValues(manager.OriginalDataSource);
            }

            return q;
        }

        internal Task<IList> GetDistinctItemsForColumn(CellBase cellBase, bool respectPreviousFilters, CancellationToken token)
        {
            DataManagerBase manager = this.DataManager;

            bool caseSensitive = cellBase.Column.FilterColumnSettings.FilterCaseSensitive;

            CachedTypedInfo cachedTypedInfo = manager.CachedTypedInfo;
            string propertyName = cellBase.Column.Key;
            bool sortAscending = true;
            bool fromDateColumn = cellBase.Column is DateColumn;

            InformationContext ic = InformationContext.CreateGenericInformationContext(cachedTypedInfo, propertyName, sortAscending, caseSensitive, null, fromDateColumn);

            Task<IList> task;

            if (respectPreviousFilters && manager.SortedFilteredDataSource != null)
            {
                task = ic.GetDistinctValuesAsync(manager.SortedFilteredDataSource, token, true);
            }
            else
            {
                task = ic.GetDistinctValuesAsync(manager.OriginalDataSource, token, true);
            }

            return task;
        }

		#endregion // GetDistinctItemsForColumn

		#region GetRowScopedConditions

		/// <summary>
		/// Creates a collection of <see cref="IConditionalFormattingRuleProxy"/> objects scoped to <see cref="StyleScope"/>.Row.
		/// </summary>
		/// <returns></returns>
		public override ReadOnlyCollection<IConditionalFormattingRuleProxy> GetRowScopedConditions()
		{
			return this.ConditionalFormatProxyCollection.GetRowScopedConditions();
		}

		#endregion // GetRowScopedConditions

		#region GetCellScopedConditions

		/// <summary>
		/// Creates a collection of <see cref="IConditionalFormattingRuleProxy"/> objects scoped to <see cref="StyleScope"/>.Cell for a given <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		public override ReadOnlyCollection<IConditionalFormattingRuleProxy> GetCellScopedConditions(string columnKey)
		{
			return this.ConditionalFormatProxyCollection.GetCellScopedConditionsForKey(columnKey);
		}

		#endregion // GetCellScopedConditions

		#region ResetRows

		/// <summary>
		/// Clears the cached rows and invalidates the rows.
		/// </summary>
		protected virtual void ResetRows()
		{
			this.CachedRows.Clear();
			this.InvalidateRows();
		}

		#endregion // ResetRows

        #region OnCurrentItemChanged

        /// <summary>
        /// Raised when the underlying data sources current item changes.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnCurrentItemChanged(object data)
        {

        }

        #endregion // OnCurrentItemChanged

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region Properties

        #region Rows

        /// <summary>
		/// Gets the collection of child rows that belongs to the <see cref="RowsManager"/>.
		/// </summary>
		public override RowBaseCollection Rows
		{
			get
			{
				if (this._rowBaseCollection == null)
				{
					this._rowBaseCollection = new RowBaseCollection(this._rows);
				}
				return this._rowBaseCollection;
			}
		}

		#endregion // Rows

		#region FullRowCount

		/// <summary>
		/// Gets the total amount of rows that can be displayed for the <see cref="RowsManager"/>.
		/// </summary>
		protected internal override int FullRowCount
		{
			get
			{
				int rowCount = 0;
				if (this.ColumnLayout != null)
				{

					if (this.ParentRow != null && this.ColumnLayout.InternalTemplate != null)
						rowCount = 1;
					else
					{
						rowCount = this.DataCount;
						if ((rowCount > 0 || this.ChildrenShouldBeDisplayedResolved) && this.ColumnLayout.Visibility == Visibility.Visible)
						{
							if (this.HeaderSupported)
								rowCount += this.RegisteredTopRows.Count;

							if (this.FooterSupported)
								rowCount += this.RegisteredBottomRows.Count;
						}
						else
							rowCount = 0;
					}
				}

				return rowCount;
			}
		}

		#endregion // FullRowCount

		#endregion // Properties

		#region Methods

		#region ResolveRowForIndex

		/// <summary>
		/// Returns the <see cref="RowBase"/> for the given index. 
		/// </summary>
		/// <param propertyName="index">The index of the row to retrieve.</param>
		/// <returns></returns>
		protected internal override RowBase ResolveRowForIndex(int index)
		{
			if (this.ParentRow != null && this.ColumnLayout.InternalTemplate != null)
			{
				if (this._columnLayoutTemplateRow == null)
					this._columnLayoutTemplateRow = new ColumnLayoutTemplateRow(this, this.ParentRow.Data);
				return this._columnLayoutTemplateRow;
			}

			if (index < 0 || index > this.FullRowCount - 1)
				return null;

			if (this.HeaderSupported && index < this.RegisteredTopRows.Count)
				return this.RegisteredTopRows[index];
			else if (this.FooterSupported)
			{
				int fullRowCount = this.FullRowCount;
				int range = fullRowCount - this.RegisteredBottomRows.Count;
				if (index >= range)
					return this.RegisteredBottomRows[fullRowCount - index - 1];
				else
					index -= this.RegisteredTopRows.Count;
			}

			return this.Rows[index];
		}

		#endregion // ResolveRowForIndex

		#region ResolveIndexForRow

		/// <summary>
		/// Returns the index for a given row.
		/// </summary>
		/// <param propertyName="row">The row whose index should be returned.</param>
		/// <returns></returns>
		protected internal override int ResolveIndexForRow(RowBase row)
		{
			int index = 0;

			if (this.HeaderSupported)
			{
				index = this.RegisteredTopRows.IndexOf(row);
				if (index != -1)
					return index;
			}

			if (this.FooterSupported)
			{
				index = this.RegisteredBottomRows.IndexOf(row);
				if (index != -1)
				{
					int modifiedBottomCount = this.RegisteredBottomRows.Count - 1;
					int count = this.Rows.Count;
					if (this.HeaderSupported)
						count += this.RegisteredTopRows.Count;

					return (modifiedBottomCount - index) + count;
				}
			}

			int topRowCount = (this.HeaderSupported) ? this.RegisteredTopRows.Count : 0;

			index = this.Rows.IndexOf((Row)row) + topRowCount;

            // Instead of just failing, a row does have an index property
            // So lets try that.
            if (index < 0)
                index = row.Index;

			if (index < 0)
				index = 0;

			return index;
		}

		#endregion // ResolveIndexForRow

		#region CompareTo

		/// <summary>
		/// Compares the index of the parent row's <see cref="ColumnLayout"/> , to the parent row's <see cref="ColumnLayout"/> index of the other manager. 
		/// </summary>
		/// <param propertyName="other"></param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and value. 
		/// Return FilterActionValue Description: 
		/// Less than zero This instance is less than value. 
		/// Zero This instance is equal to value. 
		/// Greater than zero This instance is greater than value. -or- value is null. 
		/// </returns>
		protected override int CompareTo(RowsManagerBase other)
		{
			int layoutIndex = this.ParentRow.Manager.ColumnLayout.Columns.IndexOf(this.ParentRow.ColumnLayout);
			int otherLayoutIndex = other.ParentRow.Manager.ColumnLayout.Columns.IndexOf(other.ParentRow.ColumnLayout);

			if (layoutIndex == otherLayoutIndex)
			{
				int index = this.ParentRow.Index;
				int otherIndex = other.ParentRow.Index;

				return index.CompareTo(otherIndex);
			}
			else
				return layoutIndex.CompareTo(otherLayoutIndex);
		}

		#endregion // CompareTo

		#region OnColumnLayoutPropertyChanged

		/// <summary>
		/// Raised when a property has changed on the ColumnLayout that this <see cref="RowsManager"/> represents.
		/// </summary>
		/// <param propertyName="layout"></param>
		/// <param propertyName="propertyName"></param>
		protected override void OnColumnLayoutPropertyChanged(ColumnLayout layout, string propertyName)
		{
			base.OnColumnLayoutPropertyChanged(layout, propertyName);

			switch (propertyName)
			{
                case ("SummaryScope"):
                    {
                        if (this.ColumnLayout != null)
                            this.ColumnLayout.SummaryRowSettings.SummaryDefinitionCollection.Clear();

                        this.SummaryDefinitionCollection.Clear();
                        break;
                    }
				case ("HeaderVisibility"):
					{
						this.InvalidateHeaderRowVisibility();
						break;
					}
				case ("FooterVisibility"):
					{
						this.InvalidateFooterRowVisibility();
						break;
					}
				case ("SortingInvalidated"):
					{
						this.InvalidateSort();
						break;
					}
				case ("PagerLocation"):
					{
						this.InvalidatePagerRowVisibility();
						break;
					}
				case ("AllowPaging"):
					{
						this.InvalidatePagerRowVisibility();
						break;
					}
				case ("PageSize"):
					{
                        // When PageSize is changed manually, be sure to update it via the DataManager as well. 
                        // if we don't do this, then if the ItemSource is a IPagedCollectionView, it the pageSize will be overwritten.
                        if (this.ColumnLayout != null)
                        {
                            DataManagerBase manager = this.DataManager;
                            if (manager != null)
                            {
                                manager.PageSize = this.ColumnLayout.PagerSettings.PageSizeResolved;
                            }
                        }

                        this.CurrentPageIndex = 1;
						break;
					}
				case ("GroupByInvalidated"):
					{
						this.InvalidateGroupBy(false);
						break;
					}
				case ("GroupByInvalidatedReset"):
					{
						this.InvalidateGroupBy(true);
						break;
					}
				case ("AddNewRowSettings"):
				case ("AllowAddNewRow"):
					{
						this.InvalidateAddNewRowVisibility(true);
						break;
					}
				case ("FilteringSettings"):
				case ("AllowFiltering"):
					{
						this.InvalidateFilterRowVisibility(true);
						break;
					}
				case ("AllowSummaryRow"):
					{
						this.InvalidateSummaryRowVisibility();
						break;
					}
				case ("FilteringInvalidated"):
					{
						this.InvalidateFiltering();
						this.InvalidatePager();
						break;
					}
				case ("ColumnLayoutFilteringInvalidated"):
					{
						this.InvalidateFiltering();
						this.InvalidatePager();
						break;
					}
				case ("InvalidateSummaries"):
					{
						this.InvalidateSummaries();
						break;
					}
				case ("SummaryExecution"):
					{
						if (this._dataManager != null && this.ColumnLayout != null)
						{
							this._dataManager.SummaryExecution = this.ColumnLayout.SummaryRowSettings.SummaryExecutionResolved;
							this.InvalidateRows();
						}
						break;
					}
				case ("ClearFilterProxies"):
					{
						this.ConditionalFormatProxyCollection.Clear();
						break;
					}
                case ("InvalidateConditionalFormatting"):
                    {
                        if (this.ColumnLayout != null && this.ColumnLayout.Grid != null && !this.ColumnLayout.Grid.SuspendConditionalFormatUpdates)
                        {
                            if (this.DataManager != null)
                            {
                                this.DataManager.SuspendInvalidateDataSource = true;
                            }
                            this.BuildConditionalFormats();
                            this.InvalidateData();
                            if (this.DataManager != null)
                            {
                                this.DataManager.SuspendInvalidateDataSource = false;
                            }
                            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                            {
                                RowsPanel rp = this.ColumnLayout.Grid.Panel as RowsPanel;
                                if (rp != null && !rp.InLayoutPhase)
                                {
                                    this.ColumnLayout.Grid.ResetPanelRows(true);
                                }
                            }
                        }
                        break;
                    }
				case ("ConditionalFormatRule"):
					{
						if (this.DataManager != null)
						{
							this.DataManager.SuspendInvalidateDataSource = true;
						}
						this.BuildConditionalFormats();
						this.InvalidateData();
						if (this.DataManager != null)
						{
							this.DataManager.SuspendInvalidateDataSource = false;
						}
						if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
							this.ColumnLayout.Grid.ResetPanelRows();
						break;
					}
				case ("ClearCachedRows"):
					{
						this.ResetRows();
						break;
					}
				case ("RefreshSummaryRowVisual"):
					{
						this.InvalidateSummaryRowsVisual();
						break;
					}
				case ("AllowConditionalFormatting"):
					{
						this.AttachDetachPropertyChanged(this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting);
                        
                        if (this.ColumnLayout != null && this.ColumnLayout.Grid != null && !this.ColumnLayout.Grid.SuspendConditionalFormatUpdates)
                        {
                            bool resetDeferFlag = true;
                            if (this.DataManager != null)
                            {
                                if (!this.DataManager.Defer)                                
                                    this.DataManager.Defer = true;                                
                                else
                                    resetDeferFlag = false;
                            }
                            this.BuildConditionalFormats();

                            if (resetDeferFlag && this.DataManager!=null)
                                this.DataManager.Defer = false;                                

                            this.InvalidateData();
                            
                            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                                this.ColumnLayout.Grid.ResetPanelRows(true);
                        }
						break;
					}
			}
			this.ColumnLayout.Grid.InvalidateScrollPanel(true);
		}

		#endregion // OnColumnLayoutPropertyChanged

		#region OnColumnLayoutAssigned

		/// <summary>
		/// Called when a ColumnLayout is assigned to this <see cref="RowsManager"/>.
		/// </summary>
		/// <param propertyName="layout"></param>
		protected override void OnColumnLayoutAssigned(ColumnLayout layout)
		{
			base.OnColumnLayoutAssigned(layout);
			this.InvalidateTopAndBottomRows();
			this.BuildConditionalFormats();
		}

		#endregion // OnColumnLayoutAssigned

		#region OnChildColumnLayoutRemoved

		/// <summary>
		/// Raised when a <see cref="ColumnLayout"/> is removed from the owning ColumnLayout's Columns collection.
		/// </summary>
		/// <param propertyName="layout">The <see cref="ColumnLayout"/> being removed.</param>
		protected override void OnChildColumnLayoutRemoved(ColumnLayout layout)
		{
            bool isRowVisible = false;

			for (int i = this.VisibleChildManagers.Count - 1; i >= 0; i--)
			{
				ChildBandRowsManager manager = this.VisibleChildManagers[i] as ChildBandRowsManager;

                if (manager != null)
                {
                    int count = 0; 
                    // Child bands have Rows, that are visible, when there are Siblings, or the ColumnLayoutHeaderVisbility is set to always
                    // So we need to take them into account. 				
                    foreach (ChildBand row in manager.Rows)
                    {
                        if (row.ResolveIsVisible)
                        {
                            count++;
                            isRowVisible = true;
                            break;
                        }
                    }

                    for (int j = manager.VisibleChildManagers.Count - 1; j >= 0; j--)
                    {
                        RowsManagerBase childManager = manager.VisibleChildManagers[j] as RowsManagerBase;

                        if (childManager.ColumnLayout == layout)
                        {
                            manager.UnregisterChildRowsManager(childManager);
                        }
                    }

                    if (manager.VisibleChildManagers.Count == 0)
                    {
                        // If there is a row visible, then we shouldn't unregister the manager, b/c there is still a sibling that is viisble. 
                        if (!isRowVisible)
                            this.UnregisterChildRowsManager(manager);
                    }

                    if(count == 1 && this.ColumnLayout.ColumnLayoutHeaderVisibilityResolved != ColumnLayoutHeaderVisibility.Always)
                        isRowVisible = false;
                }
			}

            // If there are still childbands visible, don't unregister all VisibleChildRowsManagers
            // if they aren't in the viewport, then this won't be registered, and child rows can still show up.
            this.InvalidateRows(!isRowVisible); 
		}

		#endregion // OnChildColumnLayoutRemoved

		#region OnChildColumnLayoutVisibilityChanged

		/// <summary>
		/// Raised when a child <see cref="ColumnLayout"/> of the owning ColumnLayout, visibility changes.
		/// </summary>
		/// <param propertyName="layout">The <see cref="ColumnLayout"/> that had it's Visibility changed.</param>
		protected override void OnChildColumnLayoutVisibilityChanged(ColumnLayout layout)
		{
			if (layout.Visibility == Visibility.Collapsed)
			{
				this.OnChildColumnLayoutRemoved(layout);
			}
			else
			{
				for (int i = this.VisibleChildManagers.Count - 1; i >= 0; i--)
				{
					ChildBandRowsManager manager = this.VisibleChildManagers[i] as ChildBandRowsManager;
                    if (manager != null)
                    {
                        foreach (ChildBand row in manager.Rows)
                        {
                            if (row.ColumnLayout == layout)
                            {
                                if (row.IsExpanded)
                                {
                                    manager.RegisterChildRowsManager(row.ChildRowsManager);
                                }
                                break;
                            }
                        }
                    }
				}
				this.InvalidateRows();
			}
		}

		#endregion // OnChildColumnLayoutVisibilityChanged

		#region InvalidateRows
		/// <summary>
		/// Clears rows and child rows and lets the grid rerender.
		/// </summary>
		public override void InvalidateRows()
		{
            this.InvalidateRows(true);
		}

        private void InvalidateRows(bool unregister)
        {
            if (!this._supressInvalidateRows)
            {
                if (unregister)
                {
                    // Only clear the rows, when we're unregistering.
                    this.ClearRows();

                    this.UnregisterAllChildRowsManager();
                }

                if (this.ColumnLayout.Grid != null)
                    this.ColumnLayout.Grid.InvalidateScrollPanel(true, false);
            }
        }
		#endregion

		#region InvalidateTopAndBottomRows

		/// <summary>
		/// Evaluates all header and footer rows, and determines if they should be hidden or visible.
		/// </summary>
		public virtual void InvalidateTopAndBottomRows()
		{
			this.InvalidateHeaderRowVisibility();
			this.InvalidateFooterRowVisibility();
			this.InvalidatePagerRowVisibility();
			this.InvalidateAddNewRowVisibility(false);
			this.InvalidateFilterRowVisibility(false);
			this.InvalidateSummaryRowVisibility();
		}

		#endregion // InvalidateTopAndBottomRows

		#region InvalidatePager

		private void InvalidatePager()
		{
            DataManagerBase dataManager = this.DataManager;
            if (dataManager != null)
            {
                PagerCell pagerCell = this.PagerRowBottom.Cells[0] as PagerCell;
                if (pagerCell != null && pagerCell.Control != null)
                {
                    PagerControlBase pc = ((PagerCellControl)pagerCell.Control).PagerControl;

                    int pageCount = dataManager.PageCount;
                    if (pc.TotalPages > pageCount)
                    {
                        pc.TotalPages = pageCount;
                        this.CurrentPageIndex = 1;
                    }

                    pc.InvalidateItems();
                }

                pagerCell = this.PagerRowTop.Cells[0] as PagerCell;
                if (pagerCell != null && pagerCell.Control != null)
                {
                    PagerControlBase pc = ((PagerCellControl)pagerCell.Control).PagerControl;

                    int pageCount = dataManager.PageCount;
                    if (pc.TotalPages != pageCount)
                        pc.TotalPages = pageCount;

                    pc.InvalidateItems();
                }
            }
		}

		#endregion // InvalidatePager

		#region UnregisterRowsManager
		/// <summary>
		/// When a RowsManager is no longer needed, this method should be called, to detach all events that are hooked up. 
		/// To avoid Memory leaks.
		/// </summary>
		/// <param name="removeColumnLayout">Whether the ColumnLayout should be removed, or just its events.</param>
		/// <param name="clearChildRowsManager">Whether the ChildRowsManager should be disposed of on each row.</param>
		/// <param name="clearSelection">Whether the selected items should be unselected</param>
		protected internal override void UnregisterRowsManager(bool removeColumnLayout, bool clearChildRowsManager, bool clearSelection)
		{
			this.AttachDetachPropertyChanged(false);

            //if (this._itemSource != null)
            //{
            //    this.ItemsSource = null;
            //}

			this.UnhookDataManager(clearChildRowsManager, clearSelection, true);

			base.UnregisterRowsManager(removeColumnLayout, clearChildRowsManager, clearSelection);
		}
		#endregion // UnregisterRowsManager

		#region InvalidateSummaryRowsVisual
		private void InvalidateSummaryRowsVisual()
		{
			InvalidateSummaryRowVisualHelper(this._summaryRowBottom);
			InvalidateSummaryRowVisualHelper(this._summaryRowTop);
		}
		#endregion // InvalidateSummaryRowsVisual

		#region InvalidateSummaryRowVisualHelper
		private void InvalidateSummaryRowVisualHelper(SummaryRow sr)
		{
			if (sr != null)
			{
				if (this.ColumnLayout != null)
				{
					ReadOnlyKeyedColumnBaseCollection<Column> columns = this.ColumnLayout.Columns.DataColumns;

					foreach (Column c in columns)
					{
						sr.Cells[c].Refresh();
					}
				}
			}
		}
		#endregion // InvalidateSummaryRowVisualHelper

        #region RefreshSummaries

        /// <summary>
        /// Reevaluates the data for the summaries on the rows.
        /// </summary>
        public override void RefreshSummaries()
        {
            base.RefreshSummaries();
            this.DataManager.RefreshSummaries();
            if (this._summaryRowBottom != null)
                this._summaryRowBottom.InvalidateRow();
            if (this._summaryRowTop != null)
                this._summaryRowTop.InvalidateRow();
            
            // If we are on a row, who is a child of a GroupByRow then we should refresh
            // its content if it has a GroupByItemTemplate set, b/c it may have a Summary bound to it.
            if (this.ParentRow != null && this.ParentRow.RowType == RowType.GroupByRow)
            {
                RowsManager rm = (RowsManager)this.ParentRow.Manager;

                if (rm.GroupedColumn.GroupByItemTemplate != null)
                {
                    ((GroupByRow)this.ParentRow).GroupByData.Reload();
                    this.ColumnLayout.Grid.ResetPanelRows();
                }
            }
        }

        #endregion // RefreshSummaries
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         		
        #region ClearRows
        internal void ClearRows()
        {
            this.Rows.Clear();

            this._duplicateObjectValidator.Clear();
        }
        #endregion // ClearRows

        #region OnColumnLayoutReset

        /// <summary>
        /// Raised when the ColumnLayout wasn't removed, but it's data has been reset.
        /// </summary>
        protected override void OnColumnLayoutReset()
        {
            DataManagerBase dmb = this.DataManager;
            if (dmb != null)
                dmb.DataSource = null;
        }

        #endregion // OnColumnLayoutReset

		#endregion // Methods

        #endregion // Overrides

        #region IDisposable Members

        /// <summary>
		/// Releases the unmanaged resources used by the <see cref="RowsManager"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (this._rows != null)
				this._rows.Dispose();

			if (this._headerRow != null)
				this._headerRow.Dispose();

			if (this._footerRow != null)
				this._footerRow.Dispose();

			if (this._cachedRows != null)
				this._cachedRows.Clear();

			if (this._topPagerRow != null)
			{
				this._topPagerRow.Dispose();
				this._topPagerRow = null;
			}
			if (this._bottomPagerRow != null)
			{
				this._bottomPagerRow.Dispose();
				this._bottomPagerRow = null;
			}

			if (this._addNewRowBottom != null)
			{
				this._addNewRowBottom.Dispose();
				this._addNewRowBottom = null;
			}

			if (this._addNewRowTop != null)
			{
				this._addNewRowTop.Dispose();
				this._addNewRowTop = null;
			}

			if (this._filterRowTop != null)
			{
				this._filterRowTop.Dispose();
				this._filterRowTop = null;
			}
			if (this._filterRowBottom != null)
			{
				this._filterRowBottom.Dispose();
				this._filterRowBottom = null;
			}

			if (this._summaryRowTop != null)
			{
				this._summaryRowTop.Dispose();
				this._summaryRowTop = null;
			}
			if (this._summaryRowBottom != null)
			{
				this._summaryRowBottom.Dispose();
				this._summaryRowBottom = null;
			}

			if (this._rowFiltersCollection != null)
			{
				this._rowFiltersCollection.PropertyChanged -= RowFiltersCollection_PropertyChanged;
				this._rowFiltersCollection.CollectionChanged -= RowFiltersCollection_CollectionChanged;
				this._rowFiltersCollection.CollectionItemChanged -= RowFiltersCollection_CollectionItemChanged;
				this._rowFiltersCollection.Dispose();
				this._rowFiltersCollection = null;
			}

			if (this._columnLayoutTemplateRow != null)
				this._columnLayoutTemplateRow.Dispose();

			if (this._summaryDefinitionCollection != null)
				this._summaryDefinitionCollection.Dispose();

			if (this._summaryResultCollection != null)
				this._summaryResultCollection.Dispose();

			if (this._conditionalFormatProxyCollection != null)
				this._conditionalFormatProxyCollection.Dispose();

			base.Dispose(disposing);
		}

		#endregion

		#region IProvideDataItems<Row> Members

		int IProvideDataItems<Row>.DataCount
		{
			get { return this.DataCount; }
		}

		Row IProvideDataItems<Row>.GetDataItem(int index)
		{
			return this.GetDataItem(index);
		}

		Row IProvideDataItems<Row>.CreateItem()
		{
			return this.CreateItem();
		}

		Row IProvideDataItems<Row>.CreateItem(object dataObject)
		{
			return this.CreateItem(dataObject);
		}

		bool IProvideDataItems<Row>.RemoveItem(Row removedObject)
		{
			return this.RemoveItem(removedObject);
		}

		void IProvideDataItems<Row>.AddItem(Row addedObject)
		{
			this.AddItem(addedObject);
		}

		void IProvideDataItems<Row>.InsertItem(int index, Row addedObject)
		{
			this.InsertItem(index, addedObject);
		}

		void IProvideDataItems<Row>.RemoveRange(IList<Row> itemsToRemove)
		{
			this.RemoveRange(itemsToRemove);
		}

		#endregion

		#region EventHandlers

		void DataManager_NewObjectGeneration(object sender, HandleableObjectGenerationEventArgs e)
		{
			RowBase r = this.ParentRow;

			while (r != null)
			{
				if (r is Row)
					break;

				ChildBand cb = r as ChildBand;

				if (cb != null)
					r = cb.ParentRow;
			}

			this.ColumnLayout.Grid.OnDataObjectRequested(e, this.ColumnLayout, r as Row, this._newDataObjectRowType as RowType? );

			this._newDataObjectRowType = null;
		}

        private void DataManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool allowConditionalFormatting = this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting;

            if (!this._deferItemSourceInvalidation)
            {
                // These checks are to support the VirtualCollection.
                bool invalidateSelection = true;

                // support for Fast adding in sorted grid
                bool invalidateSorting = true;

                // No need to invalidate selection if all we're doing is adding.
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    invalidateSelection = false;
                    invalidateSorting = false;
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    // VC doesn't fire the replace action, just remove. 
                    // B/c of that, if every item being replaced is null...then we no it's a replace
                    // and we can just assume it's safe to leave the selection as is. 
                    invalidateSelection = false;
                    foreach (object item in e.OldItems)
                    {
                        if (item != null)
                        {
                            invalidateSelection = true;
                            break;
                        }
                    }

                    invalidateSorting = false;
                }

                if (this.GroupByLevel != -1)
                    this.ColumnLayout.Grid.ResetPanelRows(false);

                this.InvalidateItemSource(true, invalidateSelection, invalidateSorting);
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object o in e.OldItems)
                {
                    INotifyPropertyChanged propChange = o as INotifyPropertyChanged;
                    this.AttachDetachPropertyChanged(propChange, false);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object o in e.NewItems)
                {
                    INotifyPropertyChanged propChange = o as INotifyPropertyChanged;
                    this.AttachDetachPropertyChanged(propChange, true);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.AttachDetachPropertyChanged();
                if (allowConditionalFormatting)
                {                    
                    this.AttachDetachPropertyChanged(true);
                }
            }

            if (allowConditionalFormatting && !this._supressCFUpdating)
            {
                bool needUpdate = false;
                foreach (IConditionalFormattingRule rule in this.ConditionalFormattingRulesResolved)
                {
                    if (rule.ShouldRefreshOnDataChange)
                    {
                        needUpdate = true;
                        break;
                    }
                }
                if (needUpdate)
                {
                    this.ColumnLayout.Grid.NeedConditionalFormatUpdate = true;

                    if (!this.ColumnLayout.Grid.SuspendConditionalFormatUpdates)
                    {
                        DependencyObject cc = PlatformProxy.GetFocusedElement(this.ColumnLayout.Grid) as DependencyObject;
                        bool hasFocus = this.ColumnLayout.Grid.IsDependancyObjectInsideGrid(cc);
                        this.ColumnLayout.InvalidateConditionalFormatting();
                        if (hasFocus)
                            this.ColumnLayout.Grid.Focus();
                    }
                }
            }
        }

        void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this._dataManager = DataManagerBase.CreateDataManager(this.ItemsSource, this.ColumnLayout.Grid.DataManagerProvider);
            if (this._dataManager != null)
            {
                if (this._weakItemsSourceCollectionChanged != null)
                {
                    this._weakItemsSourceCollectionChanged.Detach();
                    this._weakItemsSourceCollectionChanged = null;
                }

                this._dataManager.CollectionChanged += new NotifyCollectionChangedEventHandler(DataManager_CollectionChanged);
                this._dataManager.NewObjectGeneration += new EventHandler<HandleableObjectGenerationEventArgs>(DataManager_NewObjectGeneration);
                this._dataManager.ResolvingData += new EventHandler<DataAcquisitionEventArgs>(DataManager_ResolvingData);
                this._dataManager.DataUpdated += new EventHandler<EventArgs>(DataManager_DataUpdated);
                this._dataManager.CurrentItemChanged += new EventHandler<CurrentItemEventArgs>(DataManager_CurrentItemChanged);
                this.InitializeData();
                this.InvalidateGroupBy(false);
                this.InvalidateSort();
                this.InvalidateFiltering();
                this.InvalidateConditionalFormatting();
                this.InvalidateSummaries();
            }
        }

		void DataManager_ResolvingData(object sender, DataAcquisitionEventArgs e)
		{
			DataLimitingEventArgs args = new DataLimitingEventArgs(e);

			RowBase r = this.ParentRow;

			while (r != null)
			{
				if (r is Row)
					break;

				ChildBand cb = r as ChildBand;

				if (cb != null)
					r = cb.ParentRow;
			}

			args.ParentRow = r as Row;

			args.ColumnLayout = this.ColumnLayout;

			this.ColumnLayout.Grid.OnDataResolution(args);
		}

		void ItemSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			string[] split = this.ColumnLayout.Key.Split('.');
			if (e.PropertyName == split[split.Length - 1] || e.PropertyName == split[0])
			{
				object data = this.ParentRow.Data;
				PropertyInfo info = this.ColumnLayout.ResovlePropertyInfo(data);
				if (info != null)
				{
					this.ItemsSource = info.GetValue(data, null) as IEnumerable;
				}
				else
				{
                    object obj = DataManagerBase.ResolveValueFromPropertyPath(this.ColumnLayout.Key, data);
					this.ItemsSource = obj as IEnumerable;
				}
				if (this.ItemsSource != null)
					this.SetupDataManager();

				this.ColumnLayout.Grid.InvalidateScrollPanel(true);
			}
		}

		void RowFiltersCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.InvalidateFiltering();
			this.InvalidatePager();
		}

		void RowFiltersCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.InvalidateFiltering();
			this.InvalidatePager();
		}

		void RowFiltersCollection_CollectionItemChanged(object sender, EventArgs e)
		{
			this.InvalidateFiltering();
			this.InvalidatePager();
		}

		void SummaryDefinitionCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.ColumnLayout != null)
			{
				switch (e.Action)
				{
					case (NotifyCollectionChangedAction.Add):
						{
							if (e.NewItems != null)
							{
								foreach (object o in e.NewItems)
								{
									SummaryDefinition sd = (SummaryDefinition)o;

									Column col = this.ColumnLayout.Columns.DataColumns[sd.ColumnKey];

									if (col != null && col.SummaryColumnSettings.SummaryOperands.Contains(sd.SummaryOperand))
									{
										sd.SummaryOperand.IsApplied = true;
									}
								}
							}
							break;
						}
					case (NotifyCollectionChangedAction.Remove):
					case (NotifyCollectionChangedAction.Reset):
						{
							if (e.OldItems != null)
							{
								foreach (object o in e.OldItems)
								{
									SummaryDefinition sd = (SummaryDefinition)o;

									Column col = this.ColumnLayout.Columns.DataColumns[sd.ColumnKey];

									if (col != null && col.SummaryColumnSettings.SummaryOperands.Contains(sd.SummaryOperand))
									{
										sd.SummaryOperand.IsApplied = false;
									}
								}
							}
							break;
						}
					case (NotifyCollectionChangedAction.Replace):
						{ break; }
				}
			}
			this.InvalidateSummaries();
		}

		void DataManager_DataUpdated(object sender, EventArgs e)
		{
			this.InvalidateRows();
			this.InvalidatePagerRowVisibility();
		}

		void PropertyChangedDataObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Column c = this.ColumnLayout.Columns.AllColumns[e.PropertyName] as Column;

			if (c != null)
			{
				if (c.ConditionalFormatCollection.Count > 0)
				{
					foreach (IConditionalFormattingRule rule in c.ConditionalFormatCollection)
					{
						if (rule.ShouldRefreshOnDataChange)
						{
							this.ColumnLayout.Grid.NeedConditionalFormatUpdate = true;

							if (!this.ColumnLayout.Grid.SuspendConditionalFormatUpdates)
								this.ColumnLayout.InvalidateConditionalFormatting();

							break;
						}
					}
				}
			}
		}

        void DataManager_CurrentItemChanged(object sender, CurrentItemEventArgs e)
        {
            this.OnCurrentItemChanged(e.Item);
        }


		#endregion // EventHandlers
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