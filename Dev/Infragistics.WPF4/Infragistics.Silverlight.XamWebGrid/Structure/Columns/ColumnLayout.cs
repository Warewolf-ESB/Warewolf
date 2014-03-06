using System;
using System.Net;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Grids.Primitives;
using System.Collections;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that organizes a group of columns and contains settings that dictates how the data for those columns will be displayed.
	/// </summary>
	public class ColumnLayout : ColumnBase, IDisposable, INeedInitializationForPersistence
	{
		#region Members

		ColumnBaseCollection _columns;
		XamGrid _grid;
		ExpansionIndicatorSettingsOverride _expansionIndicatorSettings;
		RowSelectorSettingsOverride _rowSelectorSettings;
		ColumnMovingSettingsOverride _columnMovingSettings;
		ColumnResizingSettingsOverride _columnResizingSettings;
		SortingSettingsOverride _sortingSettings;
		PagerSettingsOverride _pagerSettings;
		FixedColumnSettingsOverride _fixedColumnSettings;
		GroupBySettingsOverride _groupBySettings;
		DeferredScrollingSettingsOverride _deferredScrollingSettings;
		AddNewRowSettingsOverride _addNewRowSettings;
		FilteringSettingsOverride _filteringSettings;
		SummaryRowSettingsOverride _summaryRowSettings;
		FillerColumnSettingsOverride _fillerColumnSettings;
		PropertyInfo _propertyInfo;
		ConditionalFormattingSettingsOverride _conditionalFormattingSettings;
		ConditionalFormatCollection _conditionalFormats;
        Style _childBandHeaderStyle;
        ColumnChooserSettingsOverride _columnChooserSettings;

        Collection<string> _keys;
        private bool? _cachedIsAlternateRowsEnabled = null;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnLayout"/> class.
		/// </summary>
		public ColumnLayout()
			: base()
		{
            this._keys = new Collection<string>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnLayout"/> class.
		/// </summary>
		/// <param propertyName="grid">The <see cref="XamGrid"/> that owns the <see cref="ColumnLayout"/>.</param>
		public ColumnLayout(XamGrid grid)
			: this()
		{
			this.Grid = grid;
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region TargetType

		/// <summary>
		/// Identifies the <see cref="TargetType"/> dependency property. 
		/// </summary>
		private static readonly DependencyProperty TargetTypeProperty = DependencyProperty.Register("TargetType", typeof(Type), typeof(ColumnLayout), new PropertyMetadata(new PropertyChangedCallback(TypeChanged)));

		/// <summary>
		/// Get/sets the Type of data that the <see cref="ColumnLayout"/> should represent.
		/// </summary>
		[TypeConverter(typeof(TypeTypeConverter))]
		private Type TargetType
		{
			get { return (Type)this.GetValue(TargetTypeProperty); }
			set { this.SetValue(TargetTypeProperty, value); }
		}

		private static void TypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion // TargetType

		#region TargetTypeName

		/// <summary>
		/// Identifies the <see cref="TargetTypeName"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty TargetTypeNameProperty = DependencyProperty.Register("TargetTypeName", typeof(string), typeof(ColumnLayout), new PropertyMetadata(new PropertyChangedCallback(TargetTypeNameChanged)));

		/// <summary>
		/// Get/Sets the System.Type.Name or <see cref="Type.FullName"/> that this <see cref="ColumnLayout"/> object should represent.
		/// </summary>
		/// <remarks>
		/// The property is only used if the <see cref="ColumnLayout"/> is defined in the <see cref="XamGrid.ColumnLayouts"/>
		/// </remarks>
		public string TargetTypeName
		{
			get { return (string)this.GetValue(TargetTypeNameProperty); }
			set { this.SetValue(TargetTypeNameProperty, value); }
		}

		private static void TargetTypeNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion // TargetTypeName

		#region Columns
		/// <summary>
		/// Gets the <see cref="ColumnBaseCollection"/> that the <see cref="ColumnLayout"/> owns.
		/// </summary>
		public ColumnBaseCollection Columns
		{
			get
			{
				if (this._columns == null)
				{
					this._columns = new ColumnBaseCollection(this);
					this._columns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
				}
				return this._columns;
			}
		}





		#endregion // Columns

		#region Grid

		/// <summary>
		/// Gets the <see cref="XamGrid"/> that the <see cref="ColumnLayout"/> belongs to.
		/// </summary>
		public XamGrid Grid
		{
			get
			{
				return this._grid;
			}
			internal set
			{
				if (this._grid != null)
				{
					if (this._grid == value)
						return;
					else
						this._grid.PropertyChanged -= new PropertyChangedEventHandler(Grid_PropertyChanged);
				}

				this._grid = value;
				if (this._grid != null)
				{
					this._grid.PropertyChanged += new PropertyChangedEventHandler(Grid_PropertyChanged);

					this.RegisterExpansionIndicatorSettings();
					this.RegisterRowSelectorSettings();
					this.RegisterSortingSettings();
					this.RegisterPagerSettings();
					this.RegisterFixedColumnSettings();
					this.RegisterGroupBySettings();
					this.RegisterDeferredScrollingSettings();
					this.RegisterAddNewRowSettings();
					this.RegisterFilterRowSettings();
					this.RegisterFillerColumnSettings();
					this.RegisterSummaryRowSettings();
					this.RegisterConditionalFormattingSettings();
                    this.RegisterColumnChooserSettings();

					this.ExecuteColumnCatchup();
				}
			}
		}

		#endregion // Grid

		#region HeaderVisibility

		/// <summary>
		/// Identifies the <see cref="HeaderVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.Register("HeaderVisibility", typeof(Visibility?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(HeaderVisibilityChanged)));

		/// <summary>
		/// Gets/sets the visibility of the Header of the <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<Visibility>))]
		public Visibility? HeaderVisibility
		{
			get { return (Visibility?)this.GetValue(HeaderVisibilityProperty); }
			set { this.SetValue(HeaderVisibilityProperty, value); }
		}

		private static void HeaderVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout columnLayout = (ColumnLayout)obj;
			columnLayout.OnPropertyChanged("HeaderVisibility");
		}

		#endregion // HeaderVisibility

		#region HeaderVisibilityResolved

		/// <summary>
		/// Resolves if the <see cref="HeaderRow"/> of this <see cref="ColumnLayout"/> is actually Visible.
		/// </summary>
		public Visibility HeaderVisibilityResolved
		{
			get
			{
				if (this.HeaderVisibility == null && this.Grid != null)
					return this.Grid.HeaderVisibility;
				else
					return (this.HeaderVisibility == System.Windows.Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		#endregion // HeaderVisibilityResolved

		#region FooterVisibility

		/// <summary>
		/// Identifies the <see cref="FooterVisibility"/> dependency property. 
		/// </summary>		
		public static readonly DependencyProperty FooterVisibilityProperty = DependencyProperty.Register("FooterVisibility", typeof(Visibility?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(FooterVisibilityChanged)));

		/// <summary>
		/// Gets/sets the visibility of the Footer of the <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<Visibility>))]
		public Visibility? FooterVisibility
		{
			get { return (Visibility?)this.GetValue(FooterVisibilityProperty); }
			set { this.SetValue(FooterVisibilityProperty, value); }
		}

		private static void FooterVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{

			ColumnLayout columnLayout = (ColumnLayout)obj;
			columnLayout.OnPropertyChanged("FooterVisibility");
		}

		#endregion // FooterVisibility

		#region FooterVisibilityResolved

		/// <summary>
		/// Resolves if the <see cref="FooterRow"/> of this <see cref="ColumnLayout"/> is actually Visible.
		/// </summary>
		public Visibility FooterVisibilityResolved
		{
			get
			{
				if (this.FooterVisibility == null && this.Grid != null)
					return this.Grid.FooterVisibility;
				else
					return (this.FooterVisibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		#endregion // FooterVisibilityResolved

		#region AutoGenerateColumns

		/// <summary>
		/// Identifies the <see cref="AutoGenerateColumns"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AutoGenerateColumnsProperty = DependencyProperty.Register("AutoGenerateColumns", typeof(bool?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(AutoGenerateColumnsChanged)));

		/// <summary>
		/// Gets/sets whether the columns of the <see cref="ColumnLayout"/> should be generated if otherwise not specified.
		/// </summary>
		/// <remarks>
		/// This property must be set before the ItemSource is set. 
		/// As setting the ItemSource will trigger the object model of the grid to be setup.
		/// That includes the property being set in XAML before the ItemSource is set. 
		/// </remarks>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? AutoGenerateColumns
		{
			get { return (bool?)this.GetValue(AutoGenerateColumnsProperty); }
			set { this.SetValue(AutoGenerateColumnsProperty, value); }
		}


		private static void AutoGenerateColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout columnLayout = (ColumnLayout)obj;
			columnLayout.OnPropertyChanged("AutoGenerateColumns");
		}

		#endregion // AutoGenerateColumns

		#region AutoGenerateColumnsResolved

		/// <summary>
		/// Resolves if <see cref="ColumnBase"/> objects should be generated automatically for this particular <see cref="ColumnLayout"/>
		/// </summary>
		public bool AutoGenerateColumnsResolved
		{
			get
			{
				if (this.AutoGenerateColumns == null && this.Grid != null)
					return this.Grid.AutoGenerateColumns;
				else
					return (this.AutoGenerateColumns == true);
			}
		}

		#endregion // AutoGenerateColumnsResolved

		#region Indentation

		/// <summary>
		/// Identifies the <see cref="Indentation"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation", typeof(double), typeof(ColumnLayout), new PropertyMetadata(double.NaN, new PropertyChangedCallback(IndentationChanged)));

		/// <summary>
		/// Gets/Sets the Indentation of the <see cref="ColumnLayout"/>.
		/// </summary>
		public double Indentation
		{
			get { return (double)this.GetValue(IndentationProperty); }
			set { this.SetValue(IndentationProperty, value); }
		}

		private static void IndentationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout columnLayout = (ColumnLayout)obj;
			columnLayout.OnPropertyChanged("Indentation");
		}

		#endregion // Indentation

		#region IndentationResolved

		/// <summary>
		/// Resolves what the actual Indentation of this <see cref="ColumnLayout"/> should be. 
		/// </summary>
		public double IndentationResolved
		{
			get
			{
				if (double.IsNaN(this.Indentation) && this.Grid != null)
					return this.Grid.Indentation;
				else
					return this.Indentation;
			}
		}

		#endregion // IndentationResolved

		#region ColumnLayoutHeaderVisibility

		/// <summary>
		/// Identifies the <see cref="ColumnLayoutHeaderVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnLayoutHeaderVisibilityProperty = DependencyProperty.Register("ColumnLayoutHeaderVisibility", typeof(ColumnLayoutHeaderVisibility?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(ColumnLayoutHeaderVisibilityChanged)));

		/// <summary>
		/// Gets/sets whether a header should be displayed for the <see cref="ColumnLayout"/>. 
		/// The header generally contains the propertyName of the data field that owns the collection that the <see cref="ColumnLayout"/> is displaying.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<ColumnLayoutHeaderVisibility>))]
		public ColumnLayoutHeaderVisibility? ColumnLayoutHeaderVisibility
		{
			get { return (ColumnLayoutHeaderVisibility?)this.GetValue(ColumnLayoutHeaderVisibilityProperty); }
			set { this.SetValue(ColumnLayoutHeaderVisibilityProperty, value); }
		}

		private static void ColumnLayoutHeaderVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout columnLayout = (ColumnLayout)obj;
			columnLayout.OnPropertyChanged("ColumnLayoutHeaderVisibility");
		}

		#endregion // ColumnLayoutHeaderVisibility

		#region ColumnLayoutHeaderVisibilityResolved

		/// <summary>
		/// Gets the actual visibility of the <see cref="ChildBand"/> for this particular <see cref="ColumnLayout"/>
		/// </summary>
		public ColumnLayoutHeaderVisibility ColumnLayoutHeaderVisibilityResolved
		{
			get
			{
                if (this.ColumnLayoutHeaderVisibility == null)
                {
                    if (this.Grid != null)
                        return this.Grid.ColumnLayoutHeaderVisibility;
                }
                else
                    return (ColumnLayoutHeaderVisibility)this.ColumnLayoutHeaderVisibility;

                return (ColumnLayoutHeaderVisibility)XamGrid.ColumnLayoutHeaderVisibilityProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
			}
		}

		#endregion // ColumnLayoutHeaderVisibilityResolved

		#region IsAlternateRowsEnabled

		/// <summary>
		/// Identifies the <see cref="IsAlternateRowsEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsAlternateRowsEnabledProperty = DependencyProperty.Register("IsAlternateRowsEnabled", typeof(bool?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(IsAlternateRowsEnabledChanged)));

		/// <summary>
		/// Gets/sets whether Alternate Row styling is enabled.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? IsAlternateRowsEnabled
		{
			get { return (bool?)this.GetValue(IsAlternateRowsEnabledProperty); }
			set { this.SetValue(IsAlternateRowsEnabledProperty, value); }
		}


		private static void IsAlternateRowsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout columnLayout = (ColumnLayout)obj;
            columnLayout.CachedIsAlternateRowsEnabled = (bool?)e.NewValue;
			columnLayout.OnPropertyChanged("IsAlternateRowsEnabled");
		}

		#endregion // IsAlternateRowsEnabled

		#region IsAlternateRowsEnabledResolved

		/// <summary>
		/// Resolves if this <see cref="ColumnLayout"/> had Alternate row styling.
		/// </summary>
		public bool IsAlternateRowsEnabledResolved
		{
			get
			{
                if (this.CachedIsAlternateRowsEnabled == null && this.Grid != null)
					return this.Grid.CachedIsAlternateRowsEnabled;
				else
                    return (this.CachedIsAlternateRowsEnabled == true);
			}
		}

		#endregion // IsAlternateRowsEnabledResolved

		#region ChildBandHeaderStyle

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="ChildBandCellControl"/> objects on this <see cref="ColumnLayout"/>.
        /// </summary>
        public Style ChildBandHeaderStyle
        {
            get
            {
                return this._childBandHeaderStyle;
            }
            set
            {
                if (this._childBandHeaderStyle != value)
                {
                    this._childBandHeaderStyle = value;
                    if (this.Grid != null)
                        this.Grid.ResetPanelRows();
                    this.OnPropertyChanged("ChildBandHeaderStyle");
                }
            }
        }

		#endregion // ChildBandHeaderStyle

		#region ChildBandHeaderStyleResolved

		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="ChildBandCellControl"/>
		/// </summary>
		public Style ChildBandHeaderStyleResolved
		{
			get
			{
				if (this.ChildBandHeaderStyle == null && this.Grid != null)
					return this.Grid.ChildBandHeaderStyle;
				else
					return this.ChildBandHeaderStyle;
			}
		}

		#endregion //ChildBandHeaderStyleResolved

		#region ColumnsHeaderTemplate

		/// <summary>
		/// Identifies the <see cref="ColumnsHeaderTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnsHeaderTemplateProperty = DependencyProperty.Register("ColumnsHeaderTemplate", typeof(DataTemplate), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(ColumnsHeaderTemplateChanged)));

		/// <summary>
		/// Gets the <see cref="DataTemplate"/> that will be applied to the header of every <see cref="Column"/> in this <see cref="ColumnLayout"/>.
		/// </summary>
		public DataTemplate ColumnsHeaderTemplate
		{
			get { return (DataTemplate)this.GetValue(ColumnsHeaderTemplateProperty); }
			set { this.SetValue(ColumnsHeaderTemplateProperty, value); }
		}

		private static void ColumnsHeaderTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout columnLayout = (ColumnLayout)obj;
			columnLayout.OnPropertyChanged("ColumnsHeaderTemplate");
		}

		#endregion // ColumnsHeaderTemplate

		#region ColumnsHeaderTemplateResolved

		/// <summary>
		/// Gets the actual ColumnsHeaderTemplate value of this ColumnLayout.
		/// </summary>
		public DataTemplate ColumnsHeaderTemplateResolved
		{
			get
			{
				if (this.ColumnsHeaderTemplate == null && this.Grid != null)
					return this.Grid.ColumnsHeaderTemplate;
				else
					return this.ColumnsHeaderTemplate;
			}
		}

		#endregion //ColumnsHeaderTemplateResolved

		#region ColumnsFooterTemplate

		/// <summary>
		/// Identifies the <see cref="ColumnsFooterTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnsFooterTemplateProperty = DependencyProperty.Register("ColumnsFooterTemplate", typeof(DataTemplate), typeof(ColumnLayout), new PropertyMetadata(new PropertyChangedCallback(ColumnsFooterTemplateChanged)));

		/// <summary>
		/// Gets the <see cref="DataTemplate"/> that will be applied to the footer of every <see cref="Column"/> in this <see cref="ColumnLayout"/>.
		/// </summary>
		public DataTemplate ColumnsFooterTemplate
		{
			get { return (DataTemplate)this.GetValue(ColumnsFooterTemplateProperty); }
			set { this.SetValue(ColumnsFooterTemplateProperty, value); }
		}

		private static void ColumnsFooterTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout layout = (ColumnLayout)obj;
			layout.OnPropertyChanged("ColumnsFooterTemplate");
		}

		#endregion // ColumnsFooterTemplate

		#region ColumnsFooterTemplateResolved

		/// <summary>
		/// Gets the actual <see cref="ColumnsFooterTemplate"/> value of this ColumnLayout.
		/// </summary>
		public DataTemplate ColumnsFooterTemplateResolved
		{
			get
			{
				if (this.ColumnsFooterTemplate == null && this.Grid != null)
					return this.Grid.ColumnsFooterTemplate;
				else
					return this.ColumnsFooterTemplate;
			}
		}

		#endregion //FooterTemplateResolved

		#region RowSelectorSettings

		/// <summary>
		/// /// Gets a reference to the <see cref="RowSelectorSettings"/> object that controls all the properties of the <see cref="RowSelectorColumn"/> of this <see cref="ColumnLayout"/>.
		/// </summary>
		public RowSelectorSettingsOverride RowSelectorSettings
		{
			get
			{
				if (this._rowSelectorSettings == null)
				{
					this._rowSelectorSettings = new RowSelectorSettingsOverride();
					this._rowSelectorSettings.PropertyChanged += new PropertyChangedEventHandler(RowSelectorSettings_PropertyChanged);
				}
				this._rowSelectorSettings.ColumnLayout = this;

				return this._rowSelectorSettings;
			}
			set
			{
				if (value != this._rowSelectorSettings)
				{
					if (this._rowSelectorSettings != null)
						this._rowSelectorSettings.PropertyChanged -= new PropertyChangedEventHandler(RowSelectorSettings_PropertyChanged);

					this._rowSelectorSettings = value;

					if (this._rowSelectorSettings != null)
						this._rowSelectorSettings.PropertyChanged += new PropertyChangedEventHandler(RowSelectorSettings_PropertyChanged);

					if (this.Columns != null)
						this.RegisterUnregisterFixedAdornerColumn(this.Columns.RowSelectorColumn, this.RowSelectorSettings.VisibilityResolved, false);

					if (this.Grid != null)
						this.Grid.InvalidateScrollPanel(true);
				}
			}
		}

		#endregion // RowSelectorSettings

		#region ExpansionIndicatorSettings

		/// <summary>
		/// Gets a reference to the <see cref="ExpansionIndicatorSettings"/> object that controls all the properties of the <see cref="ExpansionIndicatorColumn"/> of this <see cref="ColumnLayout"/>.
		/// </summary>
		public ExpansionIndicatorSettingsOverride ExpansionIndicatorSettings
		{
			get
			{
				if (this._expansionIndicatorSettings == null)
				{
					this._expansionIndicatorSettings = new ExpansionIndicatorSettingsOverride();
					this._expansionIndicatorSettings.PropertyChanged += new PropertyChangedEventHandler(ExpansionIndicatorSettings_PropertyChanged);
				}
				this._expansionIndicatorSettings.ColumnLayout = this;

				return this._expansionIndicatorSettings;
			}
			set
			{
				if (value != this._expansionIndicatorSettings)
				{
					if (this._expansionIndicatorSettings != null)
						this._expansionIndicatorSettings.PropertyChanged -= new PropertyChangedEventHandler(ExpansionIndicatorSettings_PropertyChanged);

					this._expansionIndicatorSettings = value;

					if (this._expansionIndicatorSettings != null)
						this._expansionIndicatorSettings.PropertyChanged += new PropertyChangedEventHandler(ExpansionIndicatorSettings_PropertyChanged);

					if (this.Columns != null && this.Columns.ColumnLayouts.Count > 0)
						this.RegisterUnregisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn, this.ExpansionIndicatorSettings.VisibilityResolved, true);

					if (this.Grid != null)
						this.Grid.InvalidateScrollPanel(true);
				}
			}
		}

		#endregion // ExpansionIndicatorSettings

		#region ColumnMovingSettings

		/// <summary>
		/// Gets a reference to the <see cref="ColumnMovingSettings"/> object that controls all the properties for moving columns on this <see cref="ColumnLayout"/>.
		/// </summary>
		public ColumnMovingSettingsOverride ColumnMovingSettings
		{
			get
			{
				if (this._columnMovingSettings == null)
				{
					this._columnMovingSettings = new ColumnMovingSettingsOverride();
					this._columnMovingSettings.PropertyChanged += new PropertyChangedEventHandler(ColumnMovingSettings_PropertyChanged);
				}

				this._columnMovingSettings.ColumnLayout = this;

				return this._columnMovingSettings;
			}
			set
			{
				if (value != this._columnMovingSettings)
				{
					if (this._columnMovingSettings != null)
						this._columnMovingSettings.PropertyChanged -= new PropertyChangedEventHandler(ColumnMovingSettings_PropertyChanged);

					this._columnMovingSettings = value;

					if (this._columnMovingSettings != null)
						this._columnMovingSettings.PropertyChanged += new PropertyChangedEventHandler(ColumnMovingSettings_PropertyChanged);

					this.OnPropertyChanged("");
				}
			}
		}

		#endregion // ColumnMovingSettings

		#region EditingSettings
		EditingSettingsOverride _editingSettings;
		/// <summary>
		/// Gets a reference to the <see cref="EditingSettings"/> object that controls all the properties for moving columns on this <see cref="ColumnLayout"/>.
		/// </summary>
		public EditingSettingsOverride EditingSettings
		{
			get
			{
				if (this._editingSettings == null)
				{
					this._editingSettings = new EditingSettingsOverride();
					this._editingSettings.PropertyChanged += new PropertyChangedEventHandler(EditingSettings_PropertyChanged);
				}

				this._editingSettings.ColumnLayout = this;

				return this._editingSettings;
			}
			set
			{
				if (this._editingSettings != null)
					this._editingSettings.PropertyChanged -= new PropertyChangedEventHandler(EditingSettings_PropertyChanged);

				this._editingSettings = value;

				if (this._editingSettings != null)
					this._editingSettings.PropertyChanged += new PropertyChangedEventHandler(EditingSettings_PropertyChanged);
			}
		}
		#endregion // EditingSettings

		#region SortingSettings
		/// <summary>
		/// Gets a reference to the <see cref="SortingSettings"/> object that controls all the properties for sorting columns on this <see cref="ColumnLayout"/>.
		/// </summary>
		public SortingSettingsOverride SortingSettings
		{
			get
			{
				if (_sortingSettings == null)
				{
					this._sortingSettings = new SortingSettingsOverride();
					this._sortingSettings.PropertyChanged += new PropertyChangedEventHandler(SortingSettings_PropertyChanged);
				}

				this._sortingSettings.ColumnLayout = this;

				return this._sortingSettings;
			}
			set
			{
				if (value != this._sortingSettings)
				{
					if (this._sortingSettings != null)
						this._sortingSettings.PropertyChanged -= SortingSettings_PropertyChanged;


					this._sortingSettings = value;

					if (this._sortingSettings != null)
						this._sortingSettings.PropertyChanged += new PropertyChangedEventHandler(SortingSettings_PropertyChanged);

					this.OnPropertyChanged("");
				}
			}
		}
		#endregion // SortingSettings

		#region GroupBySettings
		/// <summary>
		/// Gets a reference to the <see cref="GroupBySettingsOverride"/> object that controls all the properties for grouping data by columns on this <see cref="ColumnLayout"/>.
		/// </summary>
		public GroupBySettingsOverride GroupBySettings
		{
			get
			{
				if (this._groupBySettings == null)
				{
					this._groupBySettings = new GroupBySettingsOverride();
					this._groupBySettings.PropertyChanged += new PropertyChangedEventHandler(GroupBySettings_PropertyChanged);
				}

				this._groupBySettings.ColumnLayout = this;

				return this._groupBySettings;
			}
			set
			{
				if (this._groupBySettings != null)
				{
					this._groupBySettings.PropertyChanged -= GroupBySettings_PropertyChanged;
				}

				this._groupBySettings = value;

				if (this._groupBySettings != null)
					this._groupBySettings.PropertyChanged += new PropertyChangedEventHandler(GroupBySettings_PropertyChanged);
			}
		}
		#endregion // GroupBySettings

		#region DeferredScrollingSettings
		/// <summary>
		/// Gets a reference to the <see cref="DeferredScrollingSettingsOverride"/> object that controls all the properties for DeferredScrolling on this <see cref="ColumnLayout"/>.
		/// </summary>
		public DeferredScrollingSettingsOverride DeferredScrollingSettings
		{
			get
			{
				if (this._deferredScrollingSettings == null)
				{
					this._deferredScrollingSettings = new DeferredScrollingSettingsOverride();
					this._deferredScrollingSettings.PropertyChanged += new PropertyChangedEventHandler(DeferredScrollingSettings_PropertyChanged);
				}

				this._deferredScrollingSettings.ColumnLayout = this;

				return this._deferredScrollingSettings;
			}
			set
			{
				if (this._deferredScrollingSettings != null)
				{
					this._deferredScrollingSettings.PropertyChanged -= DeferredScrollingSettings_PropertyChanged;
				}

				this._deferredScrollingSettings = value;

				if (this._deferredScrollingSettings != null)
					this._deferredScrollingSettings.PropertyChanged += new PropertyChangedEventHandler(DeferredScrollingSettings_PropertyChanged);
			}
		}
		#endregion // DeferredScrollingSettings

		#region PagerSettings
		/// <summary>
		/// Gets a reference to the <see cref="PagerSettings"/> object that controls all the properties for paging rows on this <see cref="ColumnLayout"/>.
		/// </summary>
		public PagerSettingsOverride PagerSettings
		{
			get
			{
				if (_pagerSettings == null)
				{
					this._pagerSettings = new PagerSettingsOverride();
					this._pagerSettings.PropertyChanged += new PropertyChangedEventHandler(PagerSettings_PropertyChanged);
				}

				this._pagerSettings.ColumnLayout = this;

				return this._pagerSettings;
			}
			set
			{
				if (this._pagerSettings != null)
				{
					this._pagerSettings.PropertyChanged -= PagerSettings_PropertyChanged;
				}

				this._pagerSettings = value;

				if (this._pagerSettings != null)
					this._pagerSettings.PropertyChanged += new PropertyChangedEventHandler(PagerSettings_PropertyChanged);
			}
		}
		#endregion // PagerSettings

		#region CellStyleResolved

		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="CellControl"/>
		/// </summary>
		public override Style CellStyleResolved
		{
			get
			{
				if (this.CellStyle == null && this.Grid != null)
					return this.Grid.CellStyle;
				else
					return this.CellStyle;
			}
		}

		#endregion // CellStyleResolved

		#region HeaderStyleResolved

		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="HeaderCellControl"/>
		/// </summary>
		public override Style HeaderStyleResolved
		{
			get
			{
				if (this.HeaderStyle == null && this.Grid != null)
					return this.Grid.HeaderStyle;
				else
					return this.HeaderStyle;
			}
		}

		#endregion //HeaderStyleResolved

		#region FooterStyleResolved

		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="FooterCellControl"/>
		/// </summary>
		public override Style FooterStyleResolved
		{
			get
			{
				if (this.FooterStyle == null && this.Grid != null)
					return this.Grid.FooterStyle;
				else
					return this.FooterStyle;
			}
		}

		#endregion //FooterStyleResolved

		#region FixedColumnSettings

		/// <summary>
		/// Gets a reference to the <see cref="FixedColumnSettings"/> object that controls all the properties concerning locking <see cref="Column"/> objects to the left or right side of the <see cref="XamGrid"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public FixedColumnSettingsOverride FixedColumnSettings
		{
			get
			{
				if (this._fixedColumnSettings == null)
				{
					this._fixedColumnSettings = new FixedColumnSettingsOverride();
					this._fixedColumnSettings.PropertyChanged += new PropertyChangedEventHandler(FixedColumnSettings_PropertyChanged);
				}

				this._fixedColumnSettings.ColumnLayout = this;

				return this._fixedColumnSettings;
			}
			set
			{
				if (value != this._fixedColumnSettings)
				{
					if (this._fixedColumnSettings != null)
						this._fixedColumnSettings.PropertyChanged -= new PropertyChangedEventHandler(FixedColumnSettings_PropertyChanged);

					this._fixedColumnSettings = value;

					if (this._fixedColumnSettings != null)
						this._fixedColumnSettings.PropertyChanged += new PropertyChangedEventHandler(FixedColumnSettings_PropertyChanged);

					this.OnPropertyChanged("FixedColumnSettings");
				}
			}
		}

		#endregion // FixedColumnSettings

		#region ColumnResizingSettings

		/// <summary>
		/// Gets a reference to the <see cref="ColumnResizingSettingsOverride"/> object that controls all the properties for resizing columns on this <see cref="ColumnLayout"/>.
		/// </summary>
		public ColumnResizingSettingsOverride ColumnResizingSettings
		{
			get
			{
				if (this._columnResizingSettings == null)
				{
					this._columnResizingSettings = new ColumnResizingSettingsOverride();
					this._columnResizingSettings.PropertyChanged += new PropertyChangedEventHandler(ColumnResizingSettings_PropertyChanged);
				}

				this._columnResizingSettings.ColumnLayout = this;

				return this._columnResizingSettings;
			}
			set
			{
				if (this._columnResizingSettings != null)
					this._columnResizingSettings.PropertyChanged -= new PropertyChangedEventHandler(ColumnResizingSettings_PropertyChanged);

				this._columnResizingSettings = value;

				if (this._columnResizingSettings != null)
					this._columnResizingSettings.PropertyChanged += new PropertyChangedEventHandler(ColumnResizingSettings_PropertyChanged);
			}
		}

		#endregion // ColumnResizingSettings

		#region AddNewRowSettings
		/// <summary>
		/// Gets a reference to the <see cref="AddNewRowSettingsOverride"/> object that controls all the properties for the <see cref="AddNewRow"/> on this <see cref="ColumnLayout"/>.
		/// </summary>
		public AddNewRowSettingsOverride AddNewRowSettings
		{
			get
			{
				if (this._addNewRowSettings == null)
				{
					this._addNewRowSettings = new AddNewRowSettingsOverride();
					this._addNewRowSettings.PropertyChanged += new PropertyChangedEventHandler(AddNewRowSettings_PropertyChanged);
				}
				this._addNewRowSettings.ColumnLayout = this;

				return this._addNewRowSettings;
			}
			set
			{
				if (value != this._addNewRowSettings)
				{
					if (this._addNewRowSettings != null)
						this._addNewRowSettings.PropertyChanged -= AddNewRowSettings_PropertyChanged;

					this._addNewRowSettings = value;

					if (this._addNewRowSettings != null)
					{
						this._addNewRowSettings.ColumnLayout = this;
						this._addNewRowSettings.PropertyChanged += new PropertyChangedEventHandler(AddNewRowSettings_PropertyChanged);
					}

					this.OnPropertyChanged("AddNewRowSettings");
				}
			}
		}
		#endregion // AddNewRowSettings

		#region SummaryRowSettings
		/// <summary>
		/// Gets a reference to the <see cref="SummaryRowSettingsOverride"/> object that controls all the properties for the <see cref="SummaryRow"/> on this <see cref="ColumnLayout"/>.
		/// </summary>
		public SummaryRowSettingsOverride SummaryRowSettings
		{
			get
			{
				if (this._summaryRowSettings == null)
				{
					this._summaryRowSettings = new SummaryRowSettingsOverride();
					this._summaryRowSettings.PropertyChanged += SummaryRowSettings_PropertyChanged;
				}
				this._summaryRowSettings.ColumnLayout = this;

				return this._summaryRowSettings;
			}
			set
			{
				if (this._summaryRowSettings != null)
					this._summaryRowSettings.PropertyChanged -= SummaryRowSettings_PropertyChanged;

				this._summaryRowSettings = value;

				if (this._summaryRowSettings != null)
					this._summaryRowSettings.PropertyChanged += SummaryRowSettings_PropertyChanged;
			}
		}

		#endregion //

		#region FilteringSettings
		/// <summary>
		/// Gets a reference to the <see cref="FilteringSettingsOverride"/> object that controls all the properties for the <see cref="FilterRow"/> on this <see cref="ColumnLayout"/>.
		/// </summary>
		public FilteringSettingsOverride FilteringSettings
		{
			get
			{
				if (this._filteringSettings == null)
				{
					this._filteringSettings = new FilteringSettingsOverride();
					this._filteringSettings.PropertyChanged += new PropertyChangedEventHandler(FilteringSettings_PropertyChanged);
				}
				this._filteringSettings.ColumnLayout = this;

				return this._filteringSettings;
			}
			set
			{
				if (value != this._filteringSettings)
				{
					if (this._filteringSettings != null)
						this._filteringSettings.PropertyChanged -= FilteringSettings_PropertyChanged;

					this._filteringSettings = value;

					if (this._filteringSettings != null)
					{
						this._filteringSettings.ColumnLayout = this;
						this._filteringSettings.PropertyChanged += FilteringSettings_PropertyChanged;
					}

					this.OnPropertyChanged("FilteringSettings");
				}
			}
		}

		#endregion // FilteringSettings

		#region RowHeight

		/// <summary>
		/// Identifies the <see cref="RowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register("RowHeight", typeof(RowHeight?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(RowHeightChanged)));

		/// <summary>
		/// Gets/Sets the Height that will be applied to every row in this particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? RowHeight
		{
			get { return (RowHeight?)this.GetValue(RowHeightProperty); }
			set { this.SetValue(RowHeightProperty, value); }
		}

		private static void RowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout layout = (ColumnLayout)obj;
			layout.OnPropertyChanged("RowHeight");
		}

		#endregion // RowHeight

		#region RowHeightResolved

		/// <summary>
		/// Resolves the <see cref="ColumnLayout.RowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight RowHeightResolved
		{
			get
			{
                if (this.RowHeight == null)
                {
                    if (this.Grid != null)
                        return this.Grid.RowHeight;
                }
                else
                    return (RowHeight)this.RowHeight;

                return (RowHeight)XamGrid.RowHeightProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
			}
		}

		#endregion // RowHeightResolved

		#region MinimumRowHeight

		/// <summary>
		/// Identifies the <see cref="MinimumRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MinimumRowHeightProperty = DependencyProperty.Register("MinimumRowHeight", typeof(double?), typeof(ColumnLayout), new PropertyMetadata(new PropertyChangedCallback(MinimumRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the Minimum Height for every <see cref="RowBase"/> on this particular <see cref="ColumnLayout"/> 
		/// </summary>
		/// <remarks>
		/// This value is ignored if RowHeight is of Type Numeric.
		/// </remarks>
		[TypeConverter(typeof(NullableDoubleConverter))]
		public double? MinimumRowHeight
		{
			get { return (double?)this.GetValue(MinimumRowHeightProperty); }
			set { this.SetValue(MinimumRowHeightProperty, value); }
		}

		private static void MinimumRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout layout = (ColumnLayout)obj;
			layout.OnPropertyChanged("MinimumRowHeight");
		}

		#endregion // MinimumRowHeight

		#region MinimumRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="ColumnLayout.MinimumRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public double MinimumRowHeightResolved
		{
			get
			{
                if (this.MinimumRowHeight == null)
                {
                    if (this.Grid != null)
                        return this.Grid.MinimumRowHeight;
                }
                else
                    return (double)this.MinimumRowHeight;

                return (double)XamGrid.MinimumRowHeightProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
			}
		}

		#endregion // MinimumRowHeightResolved

		#region DeleteKeyAction

		/// <summary>
		/// Identifies the <see cref="DeleteKeyAction"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty DeleteKeyActionProperty = DependencyProperty.Register("DeleteKeyAction", typeof(DeleteKeyAction?), typeof(ColumnLayout), new PropertyMetadata(null));


		/// <summary>
		/// Gets/Sets the action that should be taken when the Delete Key is pressed for a specific <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<DeleteKeyAction>))]
		public DeleteKeyAction? DeleteKeyAction
		{
			get { return (DeleteKeyAction?)this.GetValue(DeleteKeyActionProperty); }
			set { this.SetValue(DeleteKeyActionProperty, value); }
		}

		#endregion // DeleteKeyAction

		#region DeleteKeyActionResolved

		/// <summary>
		/// Resolves the <see cref="ColumnLayout.DeleteKeyAction"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public DeleteKeyAction DeleteKeyActionResolved
		{
            get
            {
                if (this.DeleteKeyAction == null)
                {
                    if (this.Grid != null)
                        return this.Grid.DeleteKeyAction;
                }
                else
                    return (DeleteKeyAction)this.DeleteKeyAction;

                return (DeleteKeyAction)XamGrid.DeleteKeyActionProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
            }
		}

		#endregion // DeleteKeyActionResolved

		#region HeaderRowHeight

		/// <summary>
		/// Identifies the <see cref="HeaderRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty HeaderRowHeightProperty = DependencyProperty.Register("HeaderRowHeight", typeof(RowHeight?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(HeaderRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="RowHeight"/> for the <see cref="HeaderRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? HeaderRowHeight
		{
			get { return (RowHeight?)this.GetValue(HeaderRowHeightProperty); }
			set { this.SetValue(HeaderRowHeightProperty, value); }
		}

		private static void HeaderRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout layout = (ColumnLayout)obj;
			layout.OnPropertyChanged("HeaderRowHeight");
		}

		#endregion // HeaderRowHeight

		#region HeaderRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="ColumnLayout.HeaderRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight HeaderRowHeightResolved
		{
            get
            {
                if (this.HeaderRowHeight == null)
                {
                    if (this.Grid != null)
                        return this.Grid.HeaderRowHeight;
                }
                else
                    return (RowHeight)this.HeaderRowHeight;

                return (RowHeight)XamGrid.HeaderRowHeightProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
            }
		}

		#endregion //HeaderRowHeightResolved

		#region FooterRowHeight

		/// <summary>
		/// Identifies the <see cref="FooterRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FooterRowHeightProperty = DependencyProperty.Register("FooterRowHeight", typeof(RowHeight?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(FooterRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="RowHeight"/> for the <see cref="FooterRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? FooterRowHeight
		{
			get { return (RowHeight?)this.GetValue(FooterRowHeightProperty); }
			set { this.SetValue(FooterRowHeightProperty, value); }
		}

		private static void FooterRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout layout = (ColumnLayout)obj;
			layout.OnPropertyChanged("FooterRowHeight");
		}

		#endregion // FooterRowHeight

		#region FooterRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="ColumnLayout.FooterRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight FooterRowHeightResolved
		{
			get
			{
                if (this.FooterRowHeight == null)
                {
                    if (this.Grid != null)
                        return this.Grid.FooterRowHeight;
                }
                else
                    return (RowHeight)this.FooterRowHeight;

                return (RowHeight)XamGrid.FooterRowHeightProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
			}
		}

		#endregion //FooterRowHeightResolved

		#region ChildBandHeaderHeight

		/// <summary>
		/// Identifies the <see cref="ChildBandHeaderHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ChildBandHeaderHeightProperty = DependencyProperty.Register("ChildBandHeaderHeight", typeof(RowHeight?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(ChildBandHeaderHeightChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="RowHeight"/> for the <see cref="ChildBand"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? ChildBandHeaderHeight
		{
			get { return (RowHeight?)this.GetValue(ChildBandHeaderHeightProperty); }
			set { this.SetValue(ChildBandHeaderHeightProperty, value); }
		}

		private static void ChildBandHeaderHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout layout = (ColumnLayout)obj;
			layout.OnPropertyChanged("ChildBandHeaderHeight");
		}

		#endregion // ChildBandHeaderHeight

		#region ChildBandHeaderHeightResolved

		/// <summary>
		/// Resolves the <see cref="ColumnLayout.ChildBandHeaderHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight ChildBandHeaderHeightResolved
		{
			get
			{
                if (this.ChildBandHeaderHeight == null)
                {
                    if (this.Grid != null)
                        return this.Grid.ChildBandHeaderHeight;
                }
                else
                    return (RowHeight)this.ChildBandHeaderHeight;

                return (RowHeight)XamGrid.ChildBandHeaderHeightProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
			}
		}

		#endregion //ChildBandHeaderHeightResolved

		#region ColumnWidth

		/// <summary>
		/// Identifies the <see cref="ColumnWidth"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register("ColumnWidth", typeof(ColumnWidth?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(ColumnWidthChanged)));

		/// <summary>
		/// Gets/Sets the Width that will be applied to every <see cref="Column"/> in this particular <see cref="ColumnLayout"/>
		/// </summary>
        [TypeConverter(typeof(NullableColumnWidthTypeConverter))]
		public ColumnWidth? ColumnWidth
		{
			get { return (ColumnWidth?)this.GetValue(ColumnWidthProperty); }
			set { this.SetValue(ColumnWidthProperty, value); }
		}

		private static void ColumnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnLayout layout = (ColumnLayout)obj;
			layout.OnPropertyChanged("ColumnWidth");

			foreach (Column col in layout.Columns.DataColumns)
				col.ActualWidth = 0;
		}

		#endregion // ColumnWidth

		#region ColumnWidthResolved

		/// <summary>
		/// Resolves the <see cref="ColumnLayout.ColumnWidth"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public ColumnWidth ColumnWidthResolved
		{
			get
			{
				if (this.ColumnWidth == null)
				{
					if (this.Grid != null)
						return this.Grid.ColumnWidth;
					else
                        return Infragistics.Controls.Grids.ColumnWidth.Auto;
				}
				else
					return (ColumnWidth)this.ColumnWidth;
			}
		}

		#endregion // ColumnWidthResolved

		#region FillerColumnSettings

		/// <summary>
		/// Gets an object that contains settings that pertain to the <see cref="FillerColumn"/> of a particular <see cref="ColumnLayout"/>.
		/// </summary>
		public FillerColumnSettingsOverride FillerColumnSettings
		{
			get
			{
				if (this._fillerColumnSettings == null)
				{
					this._fillerColumnSettings = new FillerColumnSettingsOverride();
					this._fillerColumnSettings.PropertyChanged += new PropertyChangedEventHandler(FillerColumnSettings_PropertyChanged);
				}
				this._fillerColumnSettings.ColumnLayout = this;

				return this._fillerColumnSettings;
			}
			set
			{
				if (this._fillerColumnSettings != null)
					this._fillerColumnSettings.PropertyChanged -= new PropertyChangedEventHandler(FillerColumnSettings_PropertyChanged);

				this._fillerColumnSettings = value;

				if (this._fillerColumnSettings != null)
					this._fillerColumnSettings.PropertyChanged += new PropertyChangedEventHandler(FillerColumnSettings_PropertyChanged);
			}
		}

		#endregion // FillerColumnSettings

        #region HeaderTextHorizontalAlignment

        /// <summary>
        /// Identifies the <see cref="HeaderTextHorizontalAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTextHorizontalAlignmentProperty = DependencyProperty.Register("HeaderTextHorizontalAlignment", typeof(HorizontalAlignment?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(HeaderTextHorizontalAlignmentChanged)));

        /// <summary>
        /// Gets/sets  the <see cref="HorizontalAlignment"/> of the content for each <see cref="Column"/> in this particular <see cref="ColumnLayout"/>
        /// </summary>
        [TypeConverter(typeof(NullableEnumTypeConverter<HorizontalAlignment>))]
        public HorizontalAlignment? HeaderTextHorizontalAlignment
        {
            get { return (HorizontalAlignment?)this.GetValue(HeaderTextHorizontalAlignmentProperty); }
            set { this.SetValue(HeaderTextHorizontalAlignmentProperty, value); }
        }

        private static void HeaderTextHorizontalAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnLayout layout = (ColumnLayout)obj;
            layout.OnPropertyChanged("HeaderTextHorizontalAlignment");
        }

        #endregion // HeaderTextHorizontalAlignment 

        #region HeaderTextHorizontalAlignmentResolved

        /// <summary>
        /// Gets the actual <see cref="HorizontalAlignment"/> of the content for each <see cref="Column"/> in this particular <see cref="ColumnLayout"/>
        /// </summary>
        public HorizontalAlignment HeaderTextHorizontalAlignmentResolved
        {
            get
            {
                if (this.HeaderTextHorizontalAlignment == null)
                {
                    if (this.Grid != null)
                        return this.Grid.HeaderTextHorizontalAlignment;
                }
                else
                    return (HorizontalAlignment)this.HeaderTextHorizontalAlignment;

                return (HorizontalAlignment)XamGrid.HeaderTextHorizontalAlignmentProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
            }
        }

        #endregion // HeaderTextHorizontalAlignmentResolved

        #region HeaderTextVerticalAlignment

        /// <summary>
        /// Identifies the <see cref="HeaderTextVerticalAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTextVerticalAlignmentProperty = DependencyProperty.Register("HeaderTextVerticalAlignment", typeof(VerticalAlignment?), typeof(ColumnLayout), new PropertyMetadata(null, new PropertyChangedCallback(HeaderTextVerticalAlignmentChanged)));

        /// <summary>
        /// Gets/sets  the <see cref="VerticalAlignment"/> of the content for each <see cref="Column"/> in this particular <see cref="ColumnLayout"/>
        /// </summary>
        [TypeConverter(typeof(NullableEnumTypeConverter<VerticalAlignment>))]
        public VerticalAlignment? HeaderTextVerticalAlignment
        {
            get { return (VerticalAlignment?)this.GetValue(HeaderTextVerticalAlignmentProperty); }
            set { this.SetValue(HeaderTextVerticalAlignmentProperty, value); }
        }

        private static void HeaderTextVerticalAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnLayout layout = (ColumnLayout)obj;
            layout.OnPropertyChanged("HeaderTextVerticalAlignment");
        }

        #endregion // HeaderTextVerticalAlignment

        #region HeaderTextVerticalAlignmentResolved

        /// <summary>
        /// Gets the actual <see cref="VerticalAlignment"/> of the content for each <see cref="Column"/> in this particular <see cref="ColumnLayout"/>
        /// </summary>
        public VerticalAlignment HeaderTextVerticalAlignmentResolved
        {
            get
            {
                if (this.HeaderTextVerticalAlignment == null)
                {
                    if (this.Grid != null)
                        return this.Grid.HeaderTextVerticalAlignment;
                }
                else
                    return (VerticalAlignment)this.HeaderTextVerticalAlignment;

                return (VerticalAlignment)XamGrid.HeaderTextVerticalAlignmentProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
            }
        }

        #endregion // HeaderTextVerticalAlignmentResolved
                
		#region ConditionalFormattingSettings

		/// <summary>
		/// Gets / sets an object the contains settings that pertain to the Conditional Formatting for a particular <see cref="ColumnLayout"/>.
		/// </summary>
		internal ConditionalFormattingSettingsOverride ConditionalFormattingSettings
		{
			get
			{
				if (this._conditionalFormattingSettings == null)
				{
					this._conditionalFormattingSettings = new ConditionalFormattingSettingsOverride();
					this._conditionalFormattingSettings.PropertyChanged += this.ConditionalFormattingSettings_PropertyChanged;
				}

				this._conditionalFormattingSettings.ColumnLayout = this;

				return this._conditionalFormattingSettings;
			}
			set
			{
				if (this._conditionalFormattingSettings != null)
					this._conditionalFormattingSettings.PropertyChanged -= this.ConditionalFormattingSettings_PropertyChanged;

				this._conditionalFormattingSettings = value;

				if (this._conditionalFormattingSettings != null)
					this._conditionalFormattingSettings.PropertyChanged += this.ConditionalFormattingSettings_PropertyChanged;
			}
		}

		#endregion // ConditionalFormattingSettings

        #region ColumnChooserSettings
        /// <summary>
        /// Gets a reference to the <see cref="ColumnChooserSettings"/> object that controls all the properties for the Column Chooser on this <see cref="ColumnLayout"/>.
        /// </summary>
        public ColumnChooserSettingsOverride ColumnChooserSettings
        {
            get
            {
                if (this._columnChooserSettings == null)
                {
                    this._columnChooserSettings = new ColumnChooserSettingsOverride();
                    this._columnChooserSettings.PropertyChanged += new PropertyChangedEventHandler(ColumnChooserSettings_PropertyChanged);
                }

                this._columnChooserSettings.ColumnLayout = this;

                return this._columnChooserSettings;
            }
            set
            {
                if (this._columnChooserSettings != null)
                {
                    this._columnChooserSettings.PropertyChanged -= ColumnChooserSettings_PropertyChanged;
                }

                this._columnChooserSettings = value;

                if (this._columnChooserSettings != null)
                    this._columnChooserSettings.PropertyChanged += new PropertyChangedEventHandler(ColumnChooserSettings_PropertyChanged);
            }
        }
        #endregion // ColumnChooserSettingsOverride

        #endregion // Public

        #region Protected

        #region InternalTemplate

        /// <summary>
		/// Gets/Sets the <see cref="DataTemplate"/> that, if set, will be displayed instead of the rows that this <see cref="ColumnLayout"/> represents.
		/// </summary>
		protected internal DataTemplate InternalTemplate
		{
			get;
			set;
		}

		#endregion // InternalTemplate

		#region DefaultDataObject
		/// <summary>
		/// Gets / sets the object that will be used to compare against an object in the AddNewRow to detect if the AddNewRow is dirty.
		/// </summary>
		protected internal object DefaultDataObject
		{
			get;
			set;
		}
		#endregion // DefaultDataObject

		#region ObjectDataType
		/// <summary>
		/// The Type that the column's data is derived from.  
		/// </summary>
		/// <remarks>
		/// The DataType property gives you the Type of the collection, this give you the type of the object in the collection.
		/// </remarks>
		protected internal Type ObjectDataType
		{
			get;
			set;
		}
		#endregion // ObjectDataType

        #region ObjectDataTypeInfo
        /// <summary>
        /// The Type that the column's data is derived from.  
        /// </summary>
        /// <remarks>
        /// The DataType property gives you the Type of the collection, this give you the type of the object in the collection.
        /// </remarks>
        protected internal CachedTypedInfo ObjectDataTypeInfo
        {
            get;
            set;
        }
        #endregion // ObjectDataTypeInfo

        #region ColumnLayoutFilteringObject

        /// <summary>
		/// Used by Filtering, this object is used when the FilteringScope evaluates out to ColumnLayout.  It allows for objects
		/// in template columns to be bound to a common object.
		/// </summary>
		protected internal Object ColumnLayoutFilteringObject
		{
			get;
			set;
		}

		#endregion // ColumnLayoutFilteringObject

		#region ConditionalFormats
		/// <summary>
		/// Gets the Conditional Formats that have been set to sub columns on this ColumnLayout.
		/// </summary>
		protected internal ConditionalFormatCollection ConditionalFormatCollection
		{
			get
			{
				if (this._conditionalFormats == null)
				{
					this._conditionalFormats = new ConditionalFormatCollection();
					this._conditionalFormats.CollectionChanged += ConditionalFormats_CollectionChanged;
					this._conditionalFormats.CollectionItemChanged += ConditionalFormats_CollectionItemChanged;
				}
				return this._conditionalFormats;
			}
		}

		#endregion // ConditionalFormats

		#endregion // Protected

		#region Internal

		internal bool IsInitialized
		{
			get;
			set;
		}

		internal IEnumerable<DataField> DataFields
		{
			get;
			set;
		}

		internal bool IsDefinedGlobally
		{
			get;
			set;
		}

		internal int Level
		{
			get
			{
				int level = 0;
				ColumnLayout layout = this;
                ColumnLayout prevColLayout = null;
                while (layout.ColumnLayout != null && layout.ColumnLayout != this && layout != prevColLayout)
                {
                    level++;
                    prevColLayout = layout;
                    layout = layout.ColumnLayout;
                }
				return level;
			}
		}

        internal bool IsXamlComplete
        {
            get;
            set;
        }

        internal bool? CachedIsAlternateRowsEnabled
	    {
	        get
	        {
                return this._cachedIsAlternateRowsEnabled;
	        }
            private set
            {
                this._cachedIsAlternateRowsEnabled = value;
            }
	    }

        #region StrippedCellStyleForConditionalFormattingResolved

        internal override Style StrippedCellStyleForConditionalFormattingResolved
        {
            get
            {
                if (this.StrippedCellStyleForConditionalFormatting == null && this.Grid != null)
                    return this.Grid.StrippedCellStyleForConditionalFormatting;
                else
                    return this.StrippedCellStyleForConditionalFormatting;
            }
        }

        #endregion // StrippedCellStyleForConditionalFormattingResolved

		#endregion // Internal

		#endregion // Properties

		#region Methods

		#region Protected

		#region OnChildColumnLayoutAdded

		/// <summary>
		/// Invoked when a <see cref="ColumnLayout"/> is added to the <see cref="ColumnLayout.Columns"/> collection of this <see cref="ColumnLayout"/>.
		/// </summary>
		/// <param propertyName="childColumnLayout"></param>
		protected internal virtual void OnChildColumnLayoutAdded(ColumnLayout childColumnLayout)
		{
            if (childColumnLayout.Visibility != System.Windows.Visibility.Collapsed && this.ExpansionIndicatorSettings.VisibilityResolved == Visibility.Visible && this.Columns.ColumnLayouts.Count > 0)
				this.Columns.InternalRegisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn, true);

			if (this.ChildColumnLayoutAdded != null)
			{
				this.ChildColumnLayoutAdded(this, new ColumnLayoutEventArgs() { ColumnLayout = childColumnLayout });
			}
		}

		#endregion // OnChildColumnLayoutAdded

		#region OnChildColumnLayoutRemoved

		/// <summary>
		/// Invoked when a <see cref="ColumnLayout"/> is removed from the <see cref="ColumnLayout.Columns"/> collection of this <see cref="ColumnLayout"/>.
		/// </summary>
		/// <param propertyName="childColumnLayout"></param>
		protected internal virtual void OnChildColumnLayoutRemoved(ColumnLayout childColumnLayout)
		{
			if (this.ExpansionIndicatorSettings.VisibilityResolved == Visibility.Collapsed || this.Columns.ColumnLayouts.Count == 0)
				this.Columns.InternalUnregisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn);

			if (this.ChildColumnLayoutRemoved != null)
			{
				this.ChildColumnLayoutRemoved(this, new ColumnLayoutEventArgs() { ColumnLayout = childColumnLayout });
			}
		}

		#endregion // OnChildColumnLayoutRemoved

		#region InvalidateSorting
		/// <summary>
		/// This method is to be called when you need the grid to update its visual tree 
		/// due to a change in the sorting order.
		/// </summary>
		protected internal virtual void InvalidateSorting()
		{
			this.OnPropertyChanged("SortingInvalidated");
		}
		#endregion // InvalidateSorting

		#region InvalidateGroupBy
		/// <summary>
		/// This method is to be called when you need the grid to update its visual tree 
		/// due to a change in the GroupBy.
		/// </summary>
		protected internal virtual void InvalidateGroupBy()
		{
			this.OnPropertyChanged("GroupByInvalidated");
		}
		#endregion // InvalidateGroupBy

		#region InvalidateGroupBy
		/// <summary>
		/// This method is to be called when you need the grid to update its visual tree 
		/// due to a change in the GroupBy.
		/// </summary>
		protected internal virtual void InvalidateGroupByReset()
		{
			this.OnPropertyChanged("GroupByInvalidatedReset");
		}
		#endregion // InvalidateGroupBy

		#region InvalidateFiltering

		/// <summary>
		/// This method is to be called when you need the grid to update its visual tree 
		/// due to a change in the Filtering.
		/// </summary>
		protected internal virtual void InvalidateFiltering()
		{
			this.OnPropertyChanged("ColumnLayoutFilteringInvalidated");
		}

		#endregion // InvalidateFiltering

		#region InvalidateConditionalFormatting

		/// <summary>
		/// This method is to be called when you need the grid to update it's visual tree due to a change in ConditionalFormatting
		/// </summary>
		protected internal virtual void InvalidateConditionalFormatting()
		{
			this.OnPropertyChanged("InvalidateConditionalFormatting");
		}

		#endregion // InvalidateConditionalFormatting

		#region InvalidateSummaries

		/// <summary>
		/// This method is to be called when you need to grid to update it's visual tree due to a change in Summaries.
		/// </summary>
		protected internal virtual void InvalidateSummaries()
		{
			this.OnPropertyChanged("InvalidateSummaries");
		}

		#endregion // InvalidateSummaries

		#region InvalidateData

		/// <summary>
		/// Triggers all Data operations such as sorting and GroupBy to be invalidated. 
		/// </summary>
		protected internal virtual void InvalidateData()
		{
			this.OnPropertyChanged("InvalidateData");

		}
		#endregion // InvalidateData

		#region BuildFilters
		/// <summary>
		/// This is used by the FilterMenu to build filters for (Null) cases.
		/// </summary>
		/// <param name="rowFilters"></param>
		/// <param name="column"></param>
		/// <param name="filterOperand"></param>
		/// <param name="suppressInvalidation"></param>
		/// <param name="clearExistingFilters"></param>
        /// <param name="raiseEvents"></param>
        protected internal void BuildNullableFilter(RowFiltersCollection rowFilters, Column column, FilterOperand filterOperand, bool suppressInvalidation, bool clearExistingFilters, bool raiseEvents)
        {
            if (column.DataType == null)
                return;

            if (this.Grid.OnFiltering(column, filterOperand, rowFilters, null))
            {
                this.InvalidateFiltering();
                return;
            }

            RowsFilter r = rowFilters[column.Key];
            if (r != null)
            {
                if (clearExistingFilters)
                {
                    r.Conditions.ClearSilently();
                }
            }
            else
            {
                r = new RowsFilter(this.ObjectDataType, column);
                rowFilters.AddItemSilently(r);
            }

            if (filterOperand == null)
            {
                return;
            }

            r.Conditions.AddItemSilently(new ComparisonCondition() { Operator = (ComparisonOperator)filterOperand.ComparisonOperatorValue, FilterValue = null });

            if (!suppressInvalidation)
                this.InvalidateFiltering();

            if (raiseEvents)
                this.Grid.OnFiltered(rowFilters);
        }

		/// <summary>
		/// For a single column, based of the cell, clears any existing filtering conditions and creates a new condition based on current information.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="filterOperand"></param>
		/// <param name="newValue"></param>
		/// <param name="rowFilters"></param>
		/// <param name="suppressInvalidation"></param>
        /// <param name="fireEvents"></param>
        /// <returns>True if a filter was added</returns>
        protected internal bool BuildFilters(RowFiltersCollection rowFilters, object newValue, Column column, FilterOperand filterOperand, bool suppressInvalidation, bool fireEvents)
        {
            return this.BuildFilters(rowFilters, newValue, column, filterOperand, suppressInvalidation, true, fireEvents);
        }

		/// <summary>
		/// For a single column, based of the cell, creates a new condition based on current information.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="filterOperand"></param>
		/// <param name="newValue"></param>
		/// <param name="rowFilters"></param>
		/// <param name="suppressInvalidation"></param>
		/// <param name="clearExistingFilters"></param>
        /// <param name="fireEvents"></param>
        /// <returns>True if a filter was added</returns>
		protected internal bool BuildFilters(RowFiltersCollection rowFilters, object newValue, Column column, FilterOperand filterOperand, bool suppressInvalidation, bool clearExistingFilters, bool fireEvents)
		{
			if (column.DataType == null || !column.CanBeFiltered)
				return false;

            if (fireEvents && this.Grid.OnFiltering(column, filterOperand, rowFilters, newValue))
            {
                this.InvalidateFiltering();
                return false;
            }

            RowsFilter r = rowFilters[column.Key];
            bool setAFilter = false;
            string newValString = newValue as string;
            if (r != null)
            {
                if (clearExistingFilters)
                {
                    setAFilter = (r.Conditions.Count > 0);
                    r.Conditions.ClearSilently();
                }
            }
            else
            {
                r = new RowsFilter(this.ObjectDataType, column);
                rowFilters.AddItemSilently(r);
            }

			if (filterOperand == null)
			{
			}
            else if (filterOperand.ComparisonOperatorValue == null)
            {
                if ((filterOperand.RequiresFilteringInput && newValue != null) || !filterOperand.RequiresFilteringInput)
                {
                    System.Linq.Expressions.Expression expression = filterOperand.FilteringExpression(newValue);
                    if (expression != null)
                    {
                        r.Conditions.AddItemSilently(new CustomComparisonCondition() { FilterOperand = filterOperand.GetType(), FilterValue = newValue, Expression = expression });
                        setAFilter = true;
                    }
                }
            }
            else if (column.DataType == typeof(string) && string.IsNullOrEmpty(newValString))
            {
            }
            else if ((FilterRow.IsNullableValueType(column.DataType) || column.DataType.IsValueType) && newValString != null)
            {
            }
            else if (!column.DataType.IsValueType && newValue is string && string.IsNullOrEmpty((string)newValue))
            {
            }
            else
            {
                if (newValue != null)
                {
                    r.Conditions.AddItemSilently(new ComparisonCondition() { Operator = (ComparisonOperator)filterOperand.ComparisonOperatorValue, FilterValue = newValue });
                    setAFilter = true;
                }
                else if (!filterOperand.RequiresFilteringInput)
                {
                    r.Conditions.AddItemSilently(new ComparisonCondition() { Operator = (ComparisonOperator)filterOperand.ComparisonOperatorValue });
                    setAFilter = true;
                }
            }

            if (!suppressInvalidation)
                this.InvalidateFiltering();

            return setAFilter;
        }
		#endregion // BuildFilters

		#region RefreshSummaryRowVisual

		/// <summary>
		/// This method communicates to the RowsManager that the SummaryRow should be redrawn.
		/// </summary>
		protected internal void RefreshSummaryRowVisual()
		{
			this.OnPropertyChanged("RefreshSummaryRowVisual");
		}

		#endregion // RefreshSummaryRowVisual

		#region ClearCachedRows

		/// <summary>
		/// Used to notify attached <see cref="RowsManager"/>s to clear their cached rows.
		/// </summary>
		protected internal virtual void ClearCachedRows()
		{
			this.OnPropertyChanged("ClearCachedRows");
		}

		#endregion // ClearCachedRows

		#region InitObject

		/// <summary>
		/// This method is used to setup the object that was just created. 
		/// </summary>
		/// <param name="owner">The object that owns the object being initialized. </param>
		protected virtual void InitObject(object owner)
		{
			XamGrid grid = owner as XamGrid;
			if (grid != null)
			{
				this.Grid = grid;
			}
		}

		#endregion // InitObject

        #region OnColumnAdded

        /// <summary>
        /// Invoked when a <see cref="ColumnBase"/> has been added to this <see cref="ColumnLayout"/>
        /// </summary>
        /// <param name="columnBase"></param>
        protected internal virtual void OnColumnAdded(ColumnBase columnBase)
        {

            bool isInDesigner = false;
            if (this.Grid != null)
                isInDesigner = DesignerProperties.GetIsInDesignMode(this.Grid);






            if (!isInDesigner)
            {
                if (this._keys.Contains(columnBase.Key))
                {
                    throw new DuplicateColumnKeyException(columnBase.Key);
                }

                if (string.IsNullOrEmpty(columnBase.Key))
                {
                    throw new EmptyColumnKeyException();
                }
            }

            this._keys.Add(columnBase.Key);

            ColumnLayout colLayout = columnBase as ColumnLayout;

            if (colLayout == null || colLayout.ColumnLayout == null || colLayout != this)
                columnBase.ColumnLayout = this;

            if (this.DataFields != null)
            {
                if (colLayout == null || (!colLayout.IsDefinedGlobally && string.IsNullOrEmpty(colLayout.TargetTypeName)))
                {
                    bool found = false;
                    foreach (DataField field in this.DataFields)
                    {
                        if (field.Name == columnBase.Key)
                        {
                            found = true;
                            columnBase.DataType = field.FieldType;
                            columnBase.DataField = field;

                            if (string.IsNullOrEmpty(columnBase.HeaderText))
                                columnBase.HeaderText = field.DisplayName;
                            break;
                        }
                    }

                    if (!found)
                    {
                        if (columnBase.Key.Contains("[") && columnBase.Key.Contains("]"))
                        {
                            try
                            {
                                System.Linq.Expressions.Expression expression = DataManagerBase.BuildPropertyExpressionFromPropertyName(columnBase.Key, System.Linq.Expressions.Expression.Parameter(this.Columns.DataType, "param"));
								// don't set the datatype if it was already set.
                                if (expression != null && columnBase.DataType == null)
                                    columnBase.DataType = expression.Type;
                            }
                            catch (Exception)
                            {
                                // probably an illegal type, or the indexer is of type object and they want to access a property off of it. 
                                // If thats the case, then we can't support data interactions such as sorting, filtering or groupby
                            }
                        }
                        else
                        {
                            string[] keys = columnBase.Key.Split('.');
                            Type t = this.Columns.DataType;
                            found = true;
                            foreach (string key in keys)
                            {
                                PropertyInfo pi = t.GetProperty(key);
                                if (pi == null)
                                {
                                    if (columnBase.RequiresBoundDataKey)
                                    {
                                        if (this.Grid != null)
                                            this.Grid.ThrowInvalidColumnKeyException(columnBase.Key);
                                        else
                                            throw new InvalidColumnKeyException(columnBase.Key);
                                    }
                                    else
                                    {
                                        found = false;
                                        break;
                                    }
                                }
                                else
                                    t = pi.PropertyType;
                            }
                            if (found)
                                columnBase.DataType = t;
                        }
                    }
                }
            }            

            if (colLayout != null)
            {
                this.Columns.InternalColumnLayouts.Add(colLayout);
                this.OnChildColumnLayoutAdded(colLayout);
            }
            else
            {
                Column col = (Column)columnBase;

                ICollectionBase childCols = col.ResolveChildColumns();
                if (childCols != null)
                {
                    childCols.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
                    this.AddRemoveConditionalFormatsFromMergeCollection(childCols, true);
                }

                if (col.IsFixed == FixedState.Left)
                    this.Columns.FixedColumnsLeft.Add(col);
                else if (col.IsFixed == FixedState.Right)
                    this.Columns.FixedColumnsRight.Add(col);

                if (col.IsGroupBy)
                {
                    if (this.Grid != null)
                    {
                        this.Columns.GroupByColumns.Add(col);
                    }
                    else
                        throw new ChildColumnIsGroupByAccessDeniedException();
                }

                if (col.IsSorted != SortDirection.None)
                {
                    this.Columns.SortedColumns.AddItemSilently(col);
                }

                if (col.IsSelected)
                {
                    if (this.Grid != null)
                    {
                        SelectedColumnsCollection columns = this.Grid.SelectionSettings.SelectedColumns;
                        if (!columns.Contains(col))
                            columns.InternalAddItemSilently(columns.Count, col);
                    }
                    else
                        throw new ChildColumnIsSelectedAccessDeniedException();
                }

                col.SummaryColumnSettings.OnLoadedCatchUp();

                col.FilterColumnSettings.OnLoadedCatchUp();
            }

            if (this.IsXamlComplete)
                this.InvalidateData();

            if (this.Grid != null)
                this.Grid.InvalidateScrollPanel(true);

            columnBase.PropertyChanged += new PropertyChangedEventHandler(Column_PropertyChanged);
        }

        #endregion // OnColumnAdded

        #region OnColumnRemoved

        /// <summary>
        /// Invoked when a <see cref="ColumnBase"/> has been removed from this <see cref="ColumnLayout"/>.
        /// </summary>
        /// <param name="columnBase"></param>
        protected internal virtual void OnColumnRemoved(ColumnBase columnBase)
        {
            ColumnLayout item = columnBase as ColumnLayout;
            this._keys.Remove(columnBase.Key);

            if (item != null)
            {
                this.Columns.InternalColumnLayouts.Remove(item);
                this.OnChildColumnLayoutRemoved(item);                
            }

            Column col = columnBase as Column;
            if (col != null)
            {
                foreach (IConditionalFormattingRule rule in col.ConditionalFormatCollection)
                {
                    if (this.ConditionalFormatCollection.Contains(rule))
                    {
                        this.ConditionalFormatCollection.Remove(rule);
                    }
                }

                ICollectionBase childCols = col.ResolveChildColumns();
                if (childCols != null)
                {
                    childCols.CollectionChanged -= Columns_CollectionChanged;
                    this.AddRemoveConditionalFormatsFromMergeCollection(childCols, false);
                }
                
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

                TemplateColumn tc = col as TemplateColumn;
                if (this.Grid != null && this.Grid.IsLoaded && tc != null)
                {
                    this.Grid.ResetPanelRows(true);
                }
            }


            this.Columns.InvalidateActiveCell(columnBase);
            this.Columns.InvalidateSelection(columnBase);

            columnBase.PropertyChanged -= Column_PropertyChanged;
        }

        #endregion // OnColumnRemoved

        #endregion // Protected

        #region Internal

        internal PropertyInfo ResovlePropertyInfo(object data)
		{
			if (this._propertyInfo == null && data != null && !string.IsNullOrEmpty(this.Key))
			{
				if (this.IsDefinedGlobally && !string.IsNullOrEmpty(this.TargetTypeName))
				{
					PropertyInfo[] props = data.GetType().GetProperties(BindingFlags.Public);
					foreach (PropertyInfo prop in props)
					{
						object obj = prop.GetValue(data, null);
						if (obj != null)
						{
							IEnumerable list = obj as IEnumerable;
							if (list != null)
							{
								Type t = DataManagerBase.ResolveItemType(list);
								if (t != null)
								{
									if (this == this.Grid.ColumnLayouts.FromType(t))
									{
										this._propertyInfo = prop;
										break;
									}
								}
							}
						}
					}
				}
				else
					this._propertyInfo = data.GetType().GetProperty(this.Key, BindingFlags.Public);
			}

			return this._propertyInfo;
		}

		#endregion // Internal

		#region Private

		#region RegisterExpansionIndicatorSettings

		private void RegisterExpansionIndicatorSettings()
		{
			this._grid.ExpansionIndicatorSettings.PropertyChanged += new PropertyChangedEventHandler(this.ExpansionIndicatorSettings_PropertyChanged);

			if (this.Columns.ColumnLayouts.Count > 0)
			{
				if (this.ExpansionIndicatorSettings.VisibilityResolved == Visibility.Visible)
					this.Columns.InternalRegisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn, true);
				else
					this.Columns.InternalUnregisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn);
			}
		}

		#endregion // RegisterExpansionIndicatorSettings

		#region RegisterRowSelectorSettings

		private void RegisterRowSelectorSettings()
		{
			this._grid.RowSelectorSettings.PropertyChanged += new PropertyChangedEventHandler(this.RowSelectorSettings_PropertyChanged);

			if (this.RowSelectorSettings.VisibilityResolved == Visibility.Visible)
				this.Columns.InternalRegisterFixedAdornerColumn(this.Columns.RowSelectorColumn, false);
			else
				this.Columns.InternalUnregisterFixedAdornerColumn(this.Columns.RowSelectorColumn);
		}

		#endregion // RegisterRowSelectorSettings

		#region RegisterSortingSettings

		private void RegisterSortingSettings()
		{
			this._grid.SortingSettings.PropertyChanged += new PropertyChangedEventHandler(this.SortingSettings_PropertyChanged);
		}

		#endregion // RegisterSortingSettings

		#region RegisterGroupBySettings

		private void RegisterGroupBySettings()
		{
			this._grid.GroupBySettings.PropertyChanged += new PropertyChangedEventHandler(this.GroupBySettings_PropertyChanged);
		}

		#endregion // RegisterGroupBySettings

		#region RegisterDeferredScrollingSettings

		private void RegisterDeferredScrollingSettings()
		{
			this._grid.DeferredScrollingSettings.PropertyChanged += new PropertyChangedEventHandler(this.DeferredScrollingSettings_PropertyChanged);
		}

		#endregion // RegisterDeferredScrollingSettings

		#region RegisterPagerSettings
		private void RegisterPagerSettings()
		{
			this._grid.PagerSettings.PropertyChanged += new PropertyChangedEventHandler(this.PagerSettings_PropertyChanged);
		}
		#endregion // RegisterPagerSettings

		#region RegisterFixedColumnSettings

		private void RegisterFixedColumnSettings()
		{
			this._grid.FixedColumnSettings.PropertyChanged += new PropertyChangedEventHandler(FixedColumnSettings_PropertyChanged);
		}

		#endregion // RegisterFixedColumnSettings

		#region RegisterAddNewRowSettings
		private void RegisterAddNewRowSettings()
		{
			this._grid.AddNewRowSettings.PropertyChanged += new PropertyChangedEventHandler(this.AddNewRowSettings_PropertyChanged);
		}
		#endregion // RegisterAddNewRowSettings

		#region RegisterFilterRowSettings
		private void RegisterFilterRowSettings()
		{
			this._grid.FilteringSettings.PropertyChanged += new PropertyChangedEventHandler(this.FilteringSettings_PropertyChanged);
		}
		#endregion // RegisterFilterRowSettings

		#region RegisterSummaryRowSettings

		private void RegisterSummaryRowSettings()
		{
			this._grid.SummaryRowSettings.PropertyChanged += this.SummaryRowSettings_PropertyChanged;
		}

		#endregion // RegisterSummaryRowSettings

		#region RegisterFillerColumnSettings

		private void RegisterFillerColumnSettings()
		{
			this._grid.FillerColumnSettings.PropertyChanged += new PropertyChangedEventHandler(this.FillerColumnSettings_PropertyChanged);
		}

		#endregion // RegisterFillerColumnSettings

		#region RegisterUnregisterFixedAdornerColumn

		private void RegisterUnregisterFixedAdornerColumn(Column col, Visibility visibility, bool first)
		{
			if (visibility == System.Windows.Visibility.Visible)
				this.Columns.InternalRegisterFixedAdornerColumn(col, first);
			else
				this.Columns.InternalUnregisterFixedAdornerColumn(col);

			if (this.Grid != null)
				this.Grid.InvalidateScrollPanel(true);
		}

		#endregion // RegisterUnregisterFixedAdornerColumn

		#region RegisterConditionalFormattingSettings

		private void RegisterConditionalFormattingSettings()
		{
			this._grid.ConditionalFormattingSettings.PropertyChanged += this.ConditionalFormattingSettings_PropertyChanged;
		}


		#endregion // RegisterConditionalFormattingSettings

        #region RegisterColumnChooserSettings
        private void RegisterColumnChooserSettings()
        {
            this._grid.ColumnChooserSettings.PropertyChanged += new PropertyChangedEventHandler(this.ColumnChooserSettings_PropertyChanged);
        }
        #endregion // RegisterColumnChooserSettings

        #region ExecuteColumnCatchup
        private void ExecuteColumnCatchup()
		{
			foreach (ColumnBase cb in this.Columns.AllColumns)
			{
				Column col = cb as Column;
				if (col != null)
				{
					col.FilterColumnSettings.OnLoadedCatchUp();
					col.SummaryColumnSettings.OnLoadedCatchUp();
				}
			}
		}
		#endregion // ExecuteColumnCatchup

		#endregion // Private

		#endregion // Methods

		#region Overrides

		#region OnStyleChanged

		/// <summary>
		/// Raised when any of the Style properties of the <see cref="ColumnLayout"/> have changed;
		/// </summary>
		protected internal override void OnStyleChanged()
		{
			if (this.Grid != null)
				this.Grid.ResetPanelRows();
		}

		#endregion // OnStyleChanged

		#region OnVisibilityChanged
		/// <summary>
		/// Raised when the Visiblity of a <see cref="ColumnLayout"/> has changed.
		/// </summary>
		protected override void OnVisibilityChanged()
		{
			base.OnVisibilityChanged();

			



			if (this.Visibility == Visibility.Collapsed)
			{
				XamGrid grid = this.Grid;
				if (grid != null && grid.ActiveCell != null)
				{
					ColumnLayout parent = grid.ActiveCell.Row.ColumnLayout;
					while (parent != null)
					{
						if (parent.Visibility == Visibility.Collapsed)
						{
							grid.ActiveCell = null;
							break;
						}
						parent = parent.ColumnLayout;
					}
				}

			}

			if (this.ColumnLayout != null && this.ColumnLayout.ChildColumnLayoutVisibilityChanged != null)
				this.ColumnLayout.ChildColumnLayoutVisibilityChanged(this, new ColumnLayoutEventArgs() { ColumnLayout = this });
		}
		#endregion // OnVisibilityChanged

		#region ToString


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		#endregion // ToString

        internal override ControlTemplate ControlTemplateForConditionalFormattingResolved
        {
            get
            {
                if (this.ControlTemplateForConditionalFormatting == null && this.Grid != null)
                    return this.Grid.ControlTemplateForConditionalFormatting;

                return this.ControlTemplateForConditionalFormatting;
            }
        }

		#endregion // Overrides

		#region EventHandlers

		#region Grid_PropertyChanged
		void Grid_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "RowSelectorSettings")
			{
				this.RegisterRowSelectorSettings();
			}
			else if (e.PropertyName == "ExpansionIndicatorSettings")
			{
				this.RegisterExpansionIndicatorSettings();
			}
			else if (e.PropertyName == "SortingSettings")
			{
				this.RegisterSortingSettings();
			}
			else if (e.PropertyName == "FixedColumnSettings")
			{
				this.RegisterFixedColumnSettings();
			}
			else if (e.PropertyName == "PagingSettings")
			{
				this.RegisterPagerSettings();
			}
			else if (e.PropertyName == "GroupBySettings")
			{
				this.RegisterGroupBySettings();
			}
			else if (e.PropertyName == "DeferredScrollingSettings")
			{
				this.RegisterDeferredScrollingSettings();
			}
			else if (e.PropertyName == "AddNewRowSettings")
			{
				this.RegisterAddNewRowSettings();
			}
			else if (e.PropertyName == "FilteringSettings")
			{
				this.RegisterFilterRowSettings();
			}
			else if (e.PropertyName == "SummaryRowSettings")
			{
				this.RegisterSummaryRowSettings();
			}
			else if (e.PropertyName == "FillerColumnSettings")
			{
				this.RegisterFillerColumnSettings();
			}
			else if (e.PropertyName == "ConditionalFormattingSettings")
			{
				this.RegisterConditionalFormattingSettings();
			}
            else if (e.PropertyName == "ColumnChooserSettings")
            {
                this.RegisterColumnChooserSettings();
            }
            else if (e.PropertyName == "ColumnWidth")
            {
                foreach (Column col in this.Columns.DataColumns)
                    col.ActualWidth = 0;
            }
            else
                this.OnPropertyChanged(e.PropertyName);

			this.Grid.InvalidateScrollPanel(true);
		}

		#endregion // Grid_PropertyChanged

		#region RowSelectorSettings_PropertyChanged
		internal void RowSelectorSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Visibility")
			{
				this.RegisterUnregisterFixedAdornerColumn(this.Columns.RowSelectorColumn, this.RowSelectorSettings.VisibilityResolved, false);
			}

			if (this.Grid != null)
				this.Grid.InvalidateScrollPanel(true);
		}

		#endregion // RowSelectorSettings_PropertyChanged

		#region FillerColumnSettings_PropertyChanged
		internal void FillerColumnSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.Grid != null)
				this.Grid.InvalidateScrollPanel(true);
		}

		#endregion // FillerColumnSettings_PropertyChanged

		#region ExpansionIndicatorSettings_PropertyChanged
		internal void ExpansionIndicatorSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Visibility" && this.Columns.ColumnLayouts.Count > 0)
				this.RegisterUnregisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn, this.ExpansionIndicatorSettings.VisibilityResolved, true);

			if (this.Grid != null)
				this.Grid.InvalidateScrollPanel(true);
		}
		#endregion // ExpansionIndicatorSettings_PropertyChanged

		#region ColumnMovingSettings_PropertyChanged
		private void ColumnMovingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{

		}
		#endregion // ColumnMovingSettings_PropertyChanged

		#region ColumnResizingSettings_PropertyChanged
		private void ColumnResizingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{

		}
		#endregion // ColumnMovingSettings_PropertyChanged

		#region FixedColumnSettings_PropertyChanged
		private void FixedColumnSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.Grid != null)
				this.Grid.InvalidateScrollPanel(true);
		}
		#endregion // FixedColumnSettings_PropertyChanged

		#region SortingSettings_PropertyChanged
		private void SortingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.Grid != null)
				this.Grid.InvalidateScrollPanel(true);
		}
		#endregion //SortingSettings_PropertyChanged

		#region GroupBySettings_PropertyChanged
		private void GroupBySettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.Grid != null)
				this.Grid.InvalidateScrollPanel(true);

			this.OnPropertyChanged(e.PropertyName);
		}
		#endregion //GroupBySettings_PropertyChanged

		#region DeferredScrollingSettings_PropertyChanged
		private void DeferredScrollingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnPropertyChanged(e.PropertyName);
		}
		#endregion //DeferredScrollingSettings_PropertyChanged

		#region PagerSettings_PropertyChanged
		private void PagerSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{		
			if (this.Grid != null)
			{
				if (e.PropertyName == "AllowPaging" && this.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
				{
					this.InvalidateConditionalFormatting();
				}

				this.Grid.InvalidateScrollPanel(true);
			}

			this.OnPropertyChanged(e.PropertyName);
		}
		#endregion //PagerSettings_PropertyChanged

		#region EditingSettings_PropertyChanged
		private void EditingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{

		}
		#endregion //EditingSettings_PropertyChanged

        #region AddRemoveConditionalFormatsFromMergeCollection

        void AddRemoveConditionalFormatsFromMergeCollection(IList items, bool add)
		{
			if (items != null && items.Count > 0)
			{
				int count = items.Count;
				for (int i = 0; i < count; i++)
				{
					Column col = items[i] as Column;

					if (col != null)
					{
						if (add)
						{
							col.ConditionalFormatCollection.CollectionChanged += ConditionalFormatCollection_CollectionChanged;
						}
						else
						{
							col.ConditionalFormatCollection.CollectionChanged -= ConditionalFormatCollection_CollectionChanged;
						}

						foreach (IConditionalFormattingRule rule in col.ConditionalFormatCollection)
						{
							if (add)
							{
								this.ConditionalFormatCollection.Add(rule);
							}
							else
							{
								this.ConditionalFormatCollection.Remove(rule);
							}
						}
					}
				}
			}
		}

        #endregion // AddRemoveConditionalFormatsFromMergeCollection

        #region ConditionalFormatCollection_CollectionChanged

        void ConditionalFormatCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				foreach (IConditionalFormattingRule rule in e.NewItems)
				{
					this.ConditionalFormatCollection.Add(rule);
				}

			}
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
			{
				foreach (IConditionalFormattingRule rule in e.OldItems)
				{
					this.ConditionalFormatCollection.Remove(rule);
				}
			}
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
			{
				foreach (IConditionalFormattingRule rule in e.NewItems)
				{
					this.ConditionalFormatCollection.Add(rule);
				}
				foreach (IConditionalFormattingRule rule in e.OldItems)
				{
					this.ConditionalFormatCollection.Remove(rule);
				}
			}
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
			{
				int count = this.ConditionalFormatCollection.Count;

				this.OnPropertyChanged("ClearFilterProxies");

				this.ConditionalFormatCollection.CollectionChanged -= ConditionalFormats_CollectionChanged;
				for (int i = count - 1; i >= 0; i--)
				{
					this.ConditionalFormatCollection.RemoveAt(i);
				}
				this.ConditionalFormatCollection.CollectionChanged += ConditionalFormats_CollectionChanged;
				this.InvalidateConditionalFormatting();
			}
		}

        #endregion // ConditionalFormatCollection_CollectionChanged

        #region Columns_CollectionChanged

        void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddRemoveConditionalFormatsFromMergeCollection(e.NewItems, true);
            this.AddRemoveConditionalFormatsFromMergeCollection(e.OldItems, false);
        }

		#endregion // Columns_CollectionChanged

		#region AddNewRowSettings_PropertyChanged
		private void AddNewRowSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.Grid != null)
			{
				this.Grid.InvalidateScrollPanel(true);
			}

			this.OnPropertyChanged(e.PropertyName);
		}
		#endregion AddNewRowSettings_PropertyChanged

		#region SummaryRowSettings_PropertyChanged

		void SummaryRowSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.Grid != null)
			{
				if (e.PropertyName == "SummaryExecution")
				{
					this.Grid.ResetPanelRows();
				}
				this.Grid.InvalidateScrollPanel(true);
			}

			this.OnPropertyChanged(e.PropertyName);
		}

		#endregion // SummaryRowSettings_PropertyChanged

		#region FilteringSettings_PropertyChanged
		private void FilteringSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.Grid != null)
			{
				this.Grid.InvalidateScrollPanel(true);
			}

			this.OnPropertyChanged(e.PropertyName);
		}
		#endregion // FilteringSettings_PropertyChanged

		#region ConditionalFormattingSettings_PropertyChanged

		private void ConditionalFormattingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnPropertyChanged(e.PropertyName);
		}

		#endregion // ConditionalFormattingSettings_PropertyChanged

		#region ConditionalFormats_CollectionItemChanged

		void ConditionalFormats_CollectionItemChanged(object sender, EventArgs e)
		{
			this.OnPropertyChanged("ConditionalFormatRule");
		}

		#endregion // ConditionalFormats_CollectionItemChanged

		#region ConditionalFormats_CollectionChanged

		void ConditionalFormats_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.InvalidateConditionalFormatting();
		}

		#endregion // ConditionalFormats_CollectionChanged

        #region ColumnChooserSettings_PropertyChanged
        private void ColumnChooserSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Grid != null)
            {
                this.Grid.InvalidateScrollPanel(true);
            }

            this.OnPropertyChanged(e.PropertyName);
        }
        #endregion //ColumnChooserSettings_PropertyChanged

        #region Column_PropertyChanged

        void Column_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visibility")
            {
                this.Columns.InvalidateColumnsCollections(true);

                ColumnLayout layout = sender as ColumnLayout;
                if (layout != null)
                {
                    if (layout.Grid == null)
                        layout.Grid = this.Grid;

                    bool hide = true;
                    foreach (ColumnLayout cl in this.Columns.ColumnLayouts)
                    {
                        if (cl.Visibility == Visibility.Visible)
                        {
                            hide = false;
                            break;
                        }
                    }

                    if (hide)
                        this.Columns.InternalUnregisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn);
                    else
                    {
                        if (layout.ExpansionIndicatorSettings.VisibilityResolved == System.Windows.Visibility.Visible)
                            this.Columns.InternalRegisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn, true);
                        else
                            this.Columns.InternalUnregisterFixedAdornerColumn(this.Columns.ExpansionIndicatorColumn);
                    }
                }
                else
                {
                    ColumnBase cb = sender as ColumnBase;
                    if (cb != null && cb.Visibility == Visibility.Collapsed)
                    {
                        XamGrid grid = this.Grid;
                        if (grid != null)
                        {
                            if (grid.ActiveCell != null && cb.Equals(grid.ActiveCell.Column))
                            {
                                grid.ActiveCell = null;
                            }
                        }
                    }
                }

                // Raise ColumnVisibilityChanged event
                if (this.Grid != null)
                {
                    ColumnVisibilityChangedEventArgs eventArgs = new ColumnVisibilityChangedEventArgs
                    {
                        Column = (ColumnBase)sender
                    };

                    this.Grid.OnColumnVisibilityChanged(eventArgs);
                }
            }
            else if (e.PropertyName == "IsFixed" || e.PropertyName == "Width")
            {
                this.Columns.InvalidateColumnsCollections(true);
            }
            else if (e.PropertyName == "AllowCaseSensitiveSort" ||
                        e.PropertyName == "SortComparer" ||
                        e.PropertyName == "IsSorted"
                )
            {
                this.InvalidateSorting();
            }
            else if (e.PropertyName == "IsGroupBy")
            {
                this.InvalidateGroupBy();

                if (this.Grid != null && this.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                    this.Columns.InvalidateColumnsCollections(true);
            }
            else if (e.PropertyName == "FilterColumnSettings" ||
                        e.PropertyName == "FilteringInvalidated"
                )
            {
                this.InvalidateFiltering();
            }
            else if (e.PropertyName == "SummaryColumnSettings")
            {
                this.InvalidateSummaries();
            }
            else if (e.PropertyName == "ValueConverter" || e.PropertyName == "EditorStyle" || e.PropertyName == "ConditionalFormatCollectionCollectionChanged")
            {
                if (this.Grid != null)
                {
                    this.Grid.ResetPanelRows();
                    this.Grid.InvalidateScrollPanel(false);
                }
            }
            else if (e.PropertyName == "SelectedDateFormat")
            {
                if (this.Grid != null && this.Grid.IsLoaded)
                {
                    this.Grid.ResetPanelRows(true);
                }
            }
            else if (e.PropertyName == "ItemsSource")
            {
                if (this.Grid != null)
                {
                    this.Grid.ResetPanelRows(true);
                }
            }
            else if (e.PropertyName != "ActualWidth")
            {
                if (this.Grid != null)
                    this.Grid.InvalidateScrollPanel(true);
            }
            else if (this.Grid != null)
            {
                this.Grid.InvalidateScrollPanel(false);
            }
        }

        #endregion // Column_PropertyChanged

        #endregion // EventHandlers

        #region Events

        internal event EventHandler<ColumnLayoutEventArgs> ChildColumnLayoutRemoved;
		internal event EventHandler<ColumnLayoutEventArgs> ChildColumnLayoutAdded;
		internal event EventHandler<ColumnLayoutEventArgs> ChildColumnLayoutVisibilityChanged;
        internal event EventHandler<EventArgs> ColumnLayoutDisposing;
        internal event EventHandler<EventArgs> ColumnLayoutReset;

        internal void OnColumnLayoutDisposing()
        {
            if (this.ColumnLayoutDisposing != null)
                this.ColumnLayoutDisposing(this, EventArgs.Empty);
        }

        internal void OnColumnLayoutReset()
        {
            if (this.ColumnLayoutReset != null)
                this.ColumnLayoutReset(this, EventArgs.Empty);
        }



		#endregion // Events

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="ColumnLayout"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._columns != null)
			{
				this._columns.CollectionChanged -= Columns_CollectionChanged;
				this._columns.Dispose();
			}

			if (this._fixedColumnSettings != null)
				this._fixedColumnSettings.Dispose();

			if (this._sortingSettings != null)
				this._sortingSettings.Dispose();

			//if (this._groupBySettings != null)
			//    this._groupBySettings.Dispose();

			if (this._filteringSettings != null)
				this._filteringSettings.Dispose();

			if (this._summaryRowSettings != null)
				this._summaryRowSettings.Dispose();

			if (this._conditionalFormats != null)
			{
				this._conditionalFormats.CollectionChanged -= ConditionalFormats_CollectionChanged;
				this._conditionalFormats.CollectionItemChanged -= ConditionalFormats_CollectionItemChanged;
				this._conditionalFormats.Dispose();
			}
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="ColumnLayout"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

        #region INeedInitializationForPersistence Members

        void INeedInitializationForPersistence.InitObject(object owner)
		{
			this.InitObject(owner);
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