using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Editors.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using Infragistics.Collections;
using System.Reflection;


using Infragistics.Windows.Licensing;


namespace Infragistics.Controls.Editors
{
	/// <summary>
	/// Represents a selection control with a drop-down list of data items that display their associated properties in a multi-column grid like arrangement.  The dropdown list can be shown or hidden by clicking the arrow on the control.
	/// </summary>

	
	
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class XamMultiColumnComboEditor : ComboEditorBase<ComboRow, ComboCellsPanel>, ICommandTarget
    {
        #region Members


		private UltraLicense _license;



        ComboColumnCollection _columns;
        ComboColumnTypeMappingsCollection _columnTypeMappings;
		ComboHeaderRow _headerRow;
		WeakList<HighlightingTextBlock> _cellTextBlocks;

		private Dictionary<string, string> _localizedStrings;

        #endregion // Members

        #region Constructor

		static XamMultiColumnComboEditor()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamMultiColumnComboEditor), new FrameworkPropertyMetadata(typeof(XamMultiColumnComboEditor)));
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="XamMultiColumnComboEditor"/> class.
        /// </summary>
        public XamMultiColumnComboEditor()
        {
			// JM 05-24-11 Port to WPF.



			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamMultiColumnComboEditor), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }


			this.SupportsAlternateFilters = true;
        }

        #endregion // Constructor

        #region Properties

        #region Public

		#region AllowDropDownResizing

		/// <summary>
		/// Identifies the <see cref="AllowDropDownResizing"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowDropDownResizingProperty = DependencyProperty.Register("AllowDropDownResizing", typeof(bool), typeof(XamMultiColumnComboEditor), new PropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(AllowDropDownResizingChanged)));

		/// <summary>
		/// Gets/sets whether the drop down panel should be resizable by the end user.
		/// </summary>
		public bool AllowDropDownResizing
		{
			get { return (bool)this.GetValue(AllowDropDownResizingProperty); }
			set { this.SetValue(AllowDropDownResizingProperty, value); }
		}


		private static void AllowDropDownResizingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamMultiColumnComboEditor editor = obj as XamMultiColumnComboEditor;
			editor.AllowDropDownResizingResolved = (bool)e.NewValue;
		}

		#endregion // AllowDropDownResizing 

        #region AutoGenerateColumns

        /// <summary>
        /// Identifies the <see cref="AutoGenerateColumns"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AutoGenerateColumnsProperty = DependencyProperty.Register("AutoGenerateColumns", typeof(bool), typeof(XamMultiColumnComboEditor), new PropertyMetadata(true, new PropertyChangedCallback(AutoGenerateColumnsChanged)));

        /// <summary>
        /// Gets/sets whether the columns this <see cref="XamMultiColumnComboEditor"/> should be generated if otherwise not specified.
        /// </summary>
        public bool AutoGenerateColumns
        {
            get { return (bool)this.GetValue(AutoGenerateColumnsProperty); }
            set { this.SetValue(AutoGenerateColumnsProperty, value); }
        }

       
        private static void AutoGenerateColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
			// JM 9-1-11 TFS85162 - Force columns to get generated/removed depending on the new setting of AutoGenerateColumns.
			XamMultiColumnComboEditor editor = obj as XamMultiColumnComboEditor;
			if (editor.ItemsSource != null)
			{
				// If we are turning off AutoGenerateColumns, remove any previously auto-generated columns.
				if ((bool)e.NewValue == false && editor.Columns != null)
				{
					ObservableCollection<ComboColumn> cols = editor.Columns;
					for (int i = cols.Count - 1; i > 0; i--)
					{
						ComboColumn column = cols[i];
						if (column.IsAutoGenerated)
							cols.RemoveAt(i);
					}
				}

				editor.UnhookDataManager();
				editor.ApplyItemSource(editor.ItemsSource);
			}
        }

        #endregion // AutoGenerateColumns 

        #region Columns
		/// <summary>
        /// Gets the <see cref="ComboColumnCollection"/> that the <see cref="XamMultiColumnComboEditor"/> owns.
		/// </summary>
        public ComboColumnCollection Columns
		{
			get
			{
				if (this._columns == null)
				{
                    this._columns = new ComboColumnCollection(this);
					this._columns.CollectionChanged +=new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
				}
				return this._columns;
			}
		}

		#endregion // Columns

        #region ColumnMapping

		/// <summary>
		/// Gets a collection of <see cref="ComboColumnTypeMapping"/> objects in the <see cref="XamMultiColumnComboEditor"/>
		/// </summary>
		/// <remarks>
		/// This collection should be used to map a specific data type to a certain Column type for the AutoGeneration of columns
		/// in the <see cref="XamMultiColumnComboEditor"/>.
		/// </remarks>
        [Browsable(false)]
		public ComboColumnTypeMappingsCollection ColumnTypeMappings
		{
			get
			{
				if (this._columnTypeMappings == null)
				{
					this._columnTypeMappings = new ComboColumnTypeMappingsCollection();
					this._columnTypeMappings.Add(new ComboColumnTypeMapping() { DataType = typeof(bool), ColumnType = typeof(CheckboxComboColumn) });
                    this._columnTypeMappings.Add(new ComboColumnTypeMapping() { DataType = typeof(bool?), ColumnType = typeof(CheckboxComboColumn) });
					this._columnTypeMappings.Add(new ComboColumnTypeMapping() { DataType = typeof(ImageSource), ColumnType = typeof(ImageComboColumn) });
					this._columnTypeMappings.Add(new ComboColumnTypeMapping() { DataType = typeof(DateTime), ColumnType = typeof(DateComboColumn) });
					this._columnTypeMappings.Add(new ComboColumnTypeMapping() { DataType = typeof(DateTime?), ColumnType = typeof(DateComboColumn) });
				}
				return this._columnTypeMappings;
			}
		}

		#endregion // ColumnMapping

		#region FilterMode

		/// <summary>
		/// Identifies the <see cref="FilterMode"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterModeProperty = 
			DependencyProperty.Register("FilterMode", typeof(FilterMode), typeof(XamMultiColumnComboEditor), new PropertyMetadata(FilterMode.FilterOnAllColumns, new PropertyChangedCallback(FilterModeChanged)));

		/// <summary>
		/// Gets/sets whether the columns this <see cref="XamMultiColumnComboEditor"/> should be generated if otherwise not specified.
		/// </summary>
		public FilterMode FilterMode
		{
			get { return (FilterMode)this.GetValue(FilterModeProperty); }
			set { this.SetValue(FilterModeProperty, value); }
		}


		private static void FilterModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamMultiColumnComboEditor editor = obj as XamMultiColumnComboEditor;
			editor.FilterModeResolved = (FilterMode)e.NewValue;

			editor.SupportsAlternateFilters		= editor.FilterModeResolved == FilterMode.FilterOnAllColumns;
			editor.AutoCompleteResolved			= editor.FilterModeResolved == FilterMode.FilterOnPrimaryColumnOnly;
		}

		#endregion // FilterMode 

		#region Footer

		/// <summary>
		/// Identifies the <see cref="Footer"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FooterProperty = DependencyPropertyUtilities.Register("Footer",
			typeof(object), typeof(XamMultiColumnComboEditor),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnFooterChanged))
			);

		private static void OnFooterChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>
		/// Gets/sets an object that will serve as content for the area at the bottom of the dropdown panel.
		/// </summary>
		/// <seealso cref="FooterProperty"/>
		/// <seealso cref="FooterTemplateProperty"/>
		[Bindable(true)]
		public object Footer
		{
			get { return (object)this.GetValue(XamMultiColumnComboEditor.FooterProperty); }
			set { this.SetValue(XamMultiColumnComboEditor.FooterProperty, value); }
		}

		#endregion //Footer

		#region FooterTemplate

		/// <summary>
		/// Identifies the <see cref="FooterTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FooterTemplateProperty = DependencyPropertyUtilities.Register("FooterTemplate",
			typeof(DataTemplate), typeof(XamMultiColumnComboEditor),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnFooterTemplateChanged))
			);

		private static void OnFooterTemplateChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>
		/// Gets/sets a DataTemplate that will be used to layout content that will appear at the bottom of the dropdown panel.
		/// </summary>
		/// <seealso cref="FooterTemplateProperty"/>
		/// <seealso cref="FooterProperty"/>
		[Bindable(true)]
		public DataTemplate FooterTemplate
		{
			get { return (DataTemplate)this.GetValue(XamMultiColumnComboEditor.FooterTemplateProperty); }
			set { this.SetValue(XamMultiColumnComboEditor.FooterTemplateProperty, value); }
		}

		#endregion //FooterTemplate

		#region LocalizedStrings
		/// <summary>
		/// Returns a dictionary of localized strings for use by the control in its template.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[ReadOnly(true)]
		public Dictionary<string, string> LocalizedStrings
		{
			get
			{
				if (this._localizedStrings == null)
				{
					this._localizedStrings = new Dictionary<string, string>(10);

#pragma warning disable 436
					this._localizedStrings.Add("SelectedItemsResetButton_ToolTip", SR.GetString("SelectedItemsResetButton_ToolTip"));
#pragma warning restore 436
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#region SelectedItemsResetButtonVisibility

		/// <summary>
		/// Identifies the <see cref="SelectedItemsResetButtonVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SelectedItemsResetButtonVisibilityProperty = 
			DependencyProperty.Register("SelectedItemsResetButtonVisibility", typeof(Visibility), typeof(XamMultiColumnComboEditor), new PropertyMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(SelectedItemsResetButtonVisibilityChanged)));

		/// <summary>
		/// Gets/sets whether a button is displayed in the control's UI that lets the end user reset the list of currently selected items.
		/// </summary>
		public Visibility SelectedItemsResetButtonVisibility
		{
			get { return (Visibility)this.GetValue(SelectedItemsResetButtonVisibilityProperty); }
			set { this.SetValue(SelectedItemsResetButtonVisibilityProperty, value); }
		}


		private static void SelectedItemsResetButtonVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion // SelectedItemsResetButtonVisibility 

        #endregion // Public

		#region Private

		#region CellTextBlocks

		private WeakList<HighlightingTextBlock> CellTextBlocks
		{
			get
			{
				if (this._cellTextBlocks == null)
					this._cellTextBlocks = new WeakList<HighlightingTextBlock>();

				return this._cellTextBlocks;
			}
		}

		#endregion //CellTextBlocks

		#region FooterContentArea
		private ContentControl FooterContentArea
		{
			get;
			set;
		}
		#endregion // FooterContentArea

		#region HeaderRow

		private ComboHeaderRow HeaderRow
		{
			get
			{
				if (this._headerRow == null)
					this._headerRow = new ComboHeaderRow(null, this);

				return this._headerRow;
			}
		}

		#endregion //HeaderRow

		#endregion //Private

        #region Internal

        #region ColumnWidth

        /// <summary>
        /// Identifies the <see cref="ComboColumnWidth"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register("ColumnWidth", typeof(ComboColumnWidth), typeof(XamMultiColumnComboEditor), new PropertyMetadata(ComboColumnWidth.Auto, new PropertyChangedCallback(ColumnWidthChanged)));

        /// <summary>
        /// Gets/Sets the Width that will be applied to every <see cref="ComboColumn"/> in the <see cref="XamMultiColumnComboEditor"/>
        /// </summary>
        internal ComboColumnWidth ColumnWidth
        {
            get { return (ComboColumnWidth)this.GetValue(ColumnWidthProperty); }
            set { this.SetValue(ColumnWidthProperty, value); }
        }

        private static void ColumnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMultiColumnComboEditor combo = (XamMultiColumnComboEditor)obj;
            combo.OnPropertyChanged("ColumnWidth");
        }

        #endregion // ColumnWidth

        internal bool IsFirstRowRenderingInThisLayoutCycle
        {
            get;
            set;
        }

        internal bool InvalidateOverrideHorizontalMax
        {
            get;
            set;
        }

        internal double OverrideHorizontalMax
        {
            get;
            set;
        }

        internal int IndexOfFirstColumnRendered
        {
            get;
            set;
        }

        internal int TotalColumnsRendered
        {
            get;
            set;
        }

        internal double RowWidth
        {
            get;
            set;
        }

        internal double OverflowAdjustment
        {
            get;
            set;
        }

        internal int VisibleCellCount
        {
            get;
            set;
        }

        internal int ScrollableCellCount
        {
            get;
            set;
        }

        internal double CurrentVisibleWidth
        {
            get;
            set;
        }

        Dictionary<ComboColumn, double> _starColumnWidths;
        internal Dictionary<ComboColumn, double> StarColumnWidths
        {
            get 
            {
                if (this._starColumnWidths == null)
                    this._starColumnWidths = new Dictionary<ComboColumn, double>();
                return this._starColumnWidths; 
            }
        }

        internal Dictionary<string, DataField> DataKeys
        {
            get;
            set;
        }


        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Protected

        #region GenerateColumnForField

        /// <summary>
		/// Returns a column for the specified <see cref="DataField"/>.
		/// </summary>
		/// <param propertyName="field">A <see cref="DataField"/>.</param>
		/// <returns>A <see cref="ComboColumn"/> based off of the type of the <see cref="DataField"/></returns>
		protected virtual ComboColumn GenerateColumnForField(DataField field)
		{
			ComboColumnTypeMappingsCollection mapping = this.ColumnTypeMappings;

			ComboColumn column = null;

			foreach (ComboColumnTypeMapping map in mapping)
			{
				if (map.DataType != null && map.ColumnType != null)
				{
					if (map.DataType == field.FieldType || (map.DataType.IsAssignableFrom(field.FieldType) && !(typeof(String).IsAssignableFrom(field.FieldType)) && field.FieldType.GetInterface("IDictionary", false) == null))
					{
						bool isNullable1 = map.DataType.Name.Contains("Nullable`1");
						bool isNullable2 = field.FieldType.Name.Contains("Nullable`1");

						if (isNullable1 == isNullable2)
						{
							column = map.ColumnType.GetConstructor(new Type[] { }).Invoke(null) as ComboColumn;
							break;
						}
					}
				}
			}

			if (column == null)
				column = new TextComboColumn();

			return column;
		}

		#endregion // GenerateColumnForField

        #endregion Protected

		#region Private

		#region InvalidateHeaderRows

		private void InvalidateHeaderRows()
		{
			MultiColumnComboItemsPanel panel = this.Panel as MultiColumnComboItemsPanel;
			if (null != panel)
			{
				if (null != this._headerRow)
					panel.UnregisterFixedRowTop(this._headerRow);

				panel.RegisterFixedRowTop(this.HeaderRow);
			}
		}

		#endregion //InvalidateHeaderRows

		#endregion //Private

		#region Internal

		#region RegisterCellTextBlock

		internal void RegisterCellTextBlock(HighlightingTextBlock highlightingTextBlock)
		{
			if (!this.CellTextBlocks.Contains(highlightingTextBlock))
			{
				this._cellTextBlocks.Add(highlightingTextBlock);
				highlightingTextBlock.ComboEditor = this;
			}
		}

		#endregion //RegisterCellTextBlock

        #region ValidateColumn

        internal static void ValidateColumn(ComboColumn column, Dictionary<string, DataField> dataKeys, DataManagerBase manager

            ,PropertyDescriptorCollection pdcs

)
        {
            if (column.Key.Contains("[") && column.Key.Contains("]"))
            {
                try
                {
                    System.Linq.Expressions.Expression expression = DataManagerBase.BuildPropertyExpressionFromPropertyName(column.Key, System.Linq.Expressions.Expression.Parameter(manager.CachedType, "param"));
                    if (expression != null)
                        column.DataType = expression.Type;
                }
                catch (Exception)
                {
                    // probably an illegal type, or the indexer is of type object and they want to access a property off of it. 
                    // If thats the case, then we can't support data interactions such as sorting, filtering or groupby
                }
                return;
            }


                        if (pdcs != null)
                        {
                            PropertyDescriptor pd = pdcs[column.Key];
                            if (pd != null)
                            {
                                column.DataType = pd.PropertyType;

                                if (dataKeys.ContainsKey(column.Key))
                                {
                                    DataField field = dataKeys[column.Key];
                                    column.DataField = field;

                                    if (string.IsNullOrEmpty(column.HeaderText))
                                        column.HeaderText = field.DisplayName;
                                }
            
                                return;
                            }
                        }


            string[] keys = column.Key.Split('.');
            if (keys.Length > 0)
            {
                if (!dataKeys.ContainsKey(keys[0]))
                {
                    throw new Exception(string.Format(SRCombo.GetString("InvalidKeyException"), column.Key));
                }
                else
                {
                    DataField field = dataKeys[keys[0]];
                    Type t = manager.CachedType;
                    bool found = true;
                    foreach (string key in keys)
                    {
                        PropertyInfo pi = t.GetProperty(key);
                        if (pi == null)
                        {
                            throw new Exception(string.Format(SRCombo.GetString("InvalidKeyException"), column.Key));
                        }

                        t = pi.PropertyType;
                    }
                    if (found)
                    {
                        column.DataType = t;
                        column.DataField = field;

                        if (string.IsNullOrEmpty(column.HeaderText))
                            column.HeaderText = field.DisplayName;
                    }
                }
            }
        }
        #endregion // ValidateColumn

        #endregion //Internal

        #endregion // Methods

        #region Overrides

        #region GenerateNewObject

        /// <summary>
        /// Creates a new instance of the <see cref="ComboRow"/> object with the specified data. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override ComboRow GenerateNewObject(object data)
        {
            return new ComboRow(data, this);
        }

        #endregion // GenerateNewObject

		#region GetAlternateFilters

		internal override RecordFilterCollection GetAlternateFilters(string text)
		{
			if (this.FilterMode == FilterMode.FilterOnPrimaryColumnOnly)
				return null;

            DataManagerBase	dmb	= this.DataManager;
			if (dmb != null)
			{
				RecordFilterCollection recordFilterCollection	= new RecordFilterCollection();
				recordFilterCollection.LogicalOperator			= LogicalOperator.Or;

				// JM 02-17-12 TFS98565 - Call GetDataManagerDataFields which strips out indexer fields.
				//IEnumerable<DataField> fields = dmb.GetDataProperties();
				IEnumerable<DataField> fields = this.GetDataManagerDataFields();

				ItemsFilter containsFilter = null;
				foreach (DataField field in fields)
				{
					if (field.FieldType == typeof(string))
					{
                        containsFilter = new ItemsFilter() { ObjectType = dmb.CachedType, FieldName = field.Name, Field= field };
						containsFilter.ObjectTypedInfo				= dmb.CachedTypedInfo;
						containsFilter.Conditions.LogicalOperator	= LogicalOperator.Or;
						containsFilter.Conditions.Add(new ComparisonCondition() { Operator = ComparisonOperator.Contains, FilterValue = text });
						recordFilterCollection.Add(containsFilter);
					}
				}

				return recordFilterCollection;
			}

			return null;
		}

		#endregion //GetAlternateFilters

		#region InitializeData

		/// <summary>
        /// A DataManager was just created, so this is our chance to look at the data and do any initializing.
        /// </summary>
        protected override void InitializeData()
        {
            DataManagerBase dmb = this.DataManager;
            if (dmb != null)
            {
                this.Columns.IsInitialized = false;

				// JM 02-17-12 TFS98565 - Call GetDataManagerDataFields which strips out indexer fields.
				//IEnumerable<DataField> fields = dmb.GetDataProperties();
				IEnumerable<DataField> fields = this.GetDataManagerDataFields();

                Dictionary<string, DataField> dataKeys = new Dictionary<string, DataField>();
                int colCount = this.Columns.Count;
                foreach (DataField field in fields)
                {
                    dataKeys.Add(field.Name, field);
                }

                this.DataKeys = dataKeys;

                if (this.AutoGenerateColumns)
                {
                    
                    Collection<string> keysBeingUsed = new Collection<string>();
                    ObservableCollection<ComboColumn> cols = this.Columns;
                    foreach (ComboColumn column in cols)
                    {
						if (!string.IsNullOrEmpty(column.Key))
						{
							keysBeingUsed.Add(column.Key);

							// JM 9-30-11 TFS89871
							column.ComboEditor = this;
						}
                    }

                    
                    foreach (DataField field in fields)
                    {
                        
                        if (field.AutoGenerate && !keysBeingUsed.Contains(field.Name))
                        {
                            ComboColumn column = this.GenerateColumnForField(field);

                            if (column != null)
                            {
                                column.DataField = field;
                                column.IsAutoGenerated = true;
                                column.Key = field.Name;
                                this.Columns.Add(column);
                                column.DataType = field.FieldType;
                            }
                        }
                    }
                }


                    PropertyDescriptorCollection pdcs = dmb.CachedTypedInfo.PropertyDescriptors;                    

                foreach (ComboColumn column in this.Columns)
                {
                    XamMultiColumnComboEditor.ValidateColumn(column, dataKeys, dmb

                        ,pdcs

                    );
                }

                this.Columns.IsInitialized = true;

                this.InvalidateHeaderRows();
            }
        }

        #endregion // InitializeData

		#region OnCurrentSearchTextChanged

		internal override void OnCurrentSearchTextChanged(string currentSearchText)
		{
			foreach (HighlightingTextBlock highlightingTextBlock in this.CellTextBlocks)
				highlightingTextBlock.HighlightText(currentSearchText);
		}

		#endregion //OnCurrentSearchTextChanged

		#region OnDropDownOpening

		/// <summary>
		/// Called before the DropDownOpening event occurs.
		/// </summary>
		protected override bool OnDropDownOpening()
		{
			this.InvalidateHeaderRows();

			return base.OnDropDownOpening();
		}

		#endregion //OnDropDownOpening

		#region OnEditorKeyDown

		/// <summary>
		/// Called when a key is pressed while the ComboEditorBase's textbox has focus.
		/// </summary>
		/// <param name="e">Event args that describe the key that was pressed.</param>
		protected override void OnEditorKeyDown(KeyEventArgs e)
		{
		}

		#endregion //OnEditorKeyDOwn

		// JM 9-27-11 TFS88306 - Added.
		#region InvalidateScrollPanel
		internal override void InvalidateScrollPanel(bool resetScrollPosition, bool resetItems)
		{
			if (resetScrollPosition)
				this.OverrideHorizontalMax = -1;

			base.InvalidateScrollPanel(resetScrollPosition, resetItems);
		}
		#endregion // InvalidateScrollPanel

		// JM 10-27-11 TFS94275 Added.
		#region OnComboEditorItemClicked
		internal override void OnComboEditorItemClicked(ComboRow comboEditorItem)
		{
			base.OnComboEditorItemClicked(comboEditorItem);

			// JM 11-11-11 TFSD94280 Check for nulls.
			if (comboEditorItem != null && comboEditorItem.Control != null)
				comboEditorItem.Control.InternalEnsureAllCellVisualStates();
		}
		#endregion //OnComboEditorItemClicked

		#endregion // Overrides

		#region Events

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DropDownClosing
		/// <summary>
		/// Occurs when the IsDropDownOpen property is changing from true to false. 
		/// </summary>
		public new event EventHandler<CancelEventArgs> DropDownClosing
		{
			add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownClosing += value; }
			remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownClosing -= value; }
		}
		#endregion //DropDownClosing

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DropDownClosed
		/// <summary>
		/// Occurs when the IsDropDownOpen property was changed from true to false and the drop-down is closed.
		/// </summary>
		public new event EventHandler DropDownClosed
		{
			add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownClosed += value; }
			remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownClosed -= value; }
		}
		#endregion //DropDownClosed

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region ItemAdding
		/// <summary>
		/// Occurs when an item is going to be added to the underlying ComboEditorItemCollection of the ComboEditorBase
		/// </summary>
		public new event EventHandler<ComboItemAddingEventArgs<ComboRow>> ItemAdding
		{
			add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).ItemAdding += value; }
			remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).ItemAdding -= value; }
		}
		#endregion //ItemAdding

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region ItemAdded
		/// <summary>
		/// Occurs when an item is added to the underlying ComboEditorItemCollection of the ComboEditorBase
		/// </summary>
		public new event EventHandler<ComboItemAddedEventArgs<ComboRow>> ItemAdded
		{
			add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).ItemAdded += value; }
			remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).ItemAdded -= value; }
		}
		#endregion //ItemAdded

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DropDownOpening
		/// <summary>
		/// Occurs when the value of the IsDropDownOpen property is changing from false to true. 
		/// </summary>
		public new event EventHandler<CancelEventArgs> DropDownOpening
		{
			add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownOpening += value; }
			remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownOpening -= value; }
		}
		#endregion //DropDownOpening

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DropDownOpened
		/// <summary>
		/// Occurs when the value of the IsDropDownOpen property has changed from false to true and the drop-down is open.
		/// </summary>
		public new event EventHandler DropDownOpened
		{
			// JM 11-30-11 TFS96911 - Fix incorrect event name
			//add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownOpening += value; }
			//remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownOpening -= value; }
			add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownOpened += value; }
			remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DropDownOpened -= value; }
		}
		#endregion //DropDownOpened

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region SelectionChanged
		/// <summary>
		/// Occurs when the selection of the ComboEditorBase changes.
		/// </summary>
		public new event EventHandler SelectionChanged
		{
			add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).SelectionChanged += value; }
			remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).SelectionChanged -= value; }
		}
		#endregion //SelectionChanged

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DataObjectRequested
		/// <summary>
		/// This event is raised, when the ComboEditorBase needs to create a new data object.
		/// </summary>
		public new event EventHandler<HandleableObjectGenerationEventArgs> DataObjectRequested
		{
			add { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DataObjectRequested += value; }
			remove { ((ComboEditorBase<ComboRow, ComboCellsPanel>)this).DataObjectRequested -= value; }
		}
		#endregion //DataObjectRequested

		#endregion //Events

		#region EventHandlers

		void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
			this.InvalidateHeaderRows(); 	
        }

		#endregion // EventHandlers

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			if (source.Command is MultiColumnComboEditorCommandBase)
				return this;

			return null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			// If a parameter has been specified in the CommandSource, make sure it is a reference to this instance 
			CommandBase commandBase = command as CommandBase;
			if (null != commandBase &&
				null != commandBase.CommandSource &&
				null != commandBase.CommandSource.Parameter)
				return commandBase.CommandSource.Parameter == this;

			return command is MultiColumnComboEditorCommandBase;
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