using System.ComponentModel;
using System.Linq.Expressions;
using System;
using System.Globalization;
using Infragistics.Controls.Grids.Primitives;
using System.Windows;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// An object that controls filtering settings for a particular <see cref="Column"/> object.
    /// </summary>
    public class FilterColumnSettings : ColumnSettings, IProvidePropertyPersistenceSettings
    {
        #region Members
        private FilterOperand _filterRowDefaultOperator;
        private bool filterCaseSensitive;
        private object _filterCellValue;
        FilterOperandCollection _rowFilterOperands;
        FilterOperand _filteringOperand;
        bool _autopopulatedFilterOperands;
        bool _autopopulatedFilterMenuOperands;
        private string _filterMenuClearFiltersString;
        string _filterMenuTypeSpecificFiltersString;
        List<FilterMenuTrackingObject> _filterMenuOps;
        #endregion // Members

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterColumnSettings"/> class.
        /// </summary>
        /// <param name="column">The <see cref="Column"/> object which this settings object will be associated.</param>
        public FilterColumnSettings(Column column)
        {
            base.Column = column;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterColumnSettings"/> class.
        /// </summary>
        public FilterColumnSettings()
        {
        }
        #endregion // Constructor

        #region Properties

        #region Public

        #region FilteringOperand

        /// <summary>
        /// Gets / sets the <see cref="FilterOperand"/> which will be used during ColumnLayout filtering.
        /// </summary>
        [Browsable(false)]
        public FilterOperand FilteringOperand
        {
            get
            {
                return this._filteringOperand;
            }
            set
            {
                if (this._filteringOperand != value)
                {
                    this._filteringOperand = value;

                    this.OnPropertyChanged("FilteringOperand");

                    if (this._filteringOperand != null && this.Column != null)
                    {
                        ColumnLayout colLayout = this.Column.ColumnLayout;
                        if (!this._filteringOperand.RequiresFilteringInput)
                        {
                            if (colLayout != null)
                                this.Column.ColumnLayout.Grid.ExitEditModeInternal(true);
                            this._filterCellValue = null;
                        }
                    }

					this.BuildFilters(true);
				}
			}
		}

        #endregion // FilteringOperand

        #region FilteringOperandResolved

        /// <summary>
        /// Gets the resolved <see cref="FilterOperand"/>.  This value takes into account default values.
        /// </summary>
        public FilterOperand FilteringOperandResolved
        {
            get
            {
                if (this._filteringOperand != null)
                {
                    return this._filteringOperand;
                }

                if (this._filterRowDefaultOperator != null)
                {
                    Type filterOperandType = this._filterRowDefaultOperator.GetType();
                    foreach (FilterOperand op in this.RowFilterOperands)
                    {
                        if (op.GetType() == filterOperandType)
                            return this._filterRowDefaultOperator;
                    }
                }
                if (this.RowFilterOperands.Count == 0)
                    return this._filterRowDefaultOperator;
                else
                    return this.RowFilterOperands[0];
            }
        }

        #endregion // FilteringOperandResolved

        #region FilterCaseSensitive

		/// <summary>
		/// Gets / sets if the filters applied are case sensitive.
		/// </summary>
		/// <remarks>This setting is only applied when the <see cref="ColumnBase.DataType"/> evaluates to <see cref="string"/> </remarks>
		public bool FilterCaseSensitive
		{
			get
			{
				return this.filterCaseSensitive;
			}
			set
			{
				if (this.filterCaseSensitive != value)
				{
					this.filterCaseSensitive = value;
					this.OnPropertyChanged("FilterCaseSensitive");
                    this.BuildFilters(true);
				}
			}
		}
		#endregion // FilterCaseSensitive

        #region FilterCellValue

        /// <summary>
        /// Gets / sets the value that is currently being used to filter the rows when using a <see cref="FilterRow"/>.
        /// </summary>
        /// <remarks>This value is only used when the <see cref="FilteringScope"/> resolves to ColumnLayout.</remarks>
        [Browsable(false)]
		public object FilterCellValue
		{
			get
			{
				return this._filterCellValue;
			}
			set
			{
				if (this.Column == null || this.Column.DataType == null)
				{
					this._filterCellValue = value;
					this.OnPropertyChanged("FilterCellValue");
                    this.BuildFilters(true);
					return;
				}

                if (this._filterCellValue != value)
                {
                    string convertedValue = value as string;

                    if (!string.IsNullOrEmpty(convertedValue))
                    {
                        try
                        {
                            object objectValue = FilterRow.ChangeType(value, this.Column.DataType);

							if (this._filterCellValue != objectValue)
							{
								this._filterCellValue = FilterRow.ChangeType(value, this.Column.DataType);
								this.OnPropertyChanged("FilterCellValue");
                                this.BuildFilters(true);
							}
						}
						catch (FormatException)
						{
							EditableColumn col = this.Column as EditableColumn;
							if (col != null && col.AllowEditingValidation)
							{
								throw;
							}
						}
					}
					else
					{
						if (this._filterCellValue != null)
						{
							if (this.Column.DataType == typeof(string))
							{
								if (!string.IsNullOrEmpty(this._filterCellValue as string))
								{
									this._filterCellValue = value;
									this.OnPropertyChanged("FilterCellValue");
                                    this.BuildFilters(true);
								}
							}
							else
							{
								this._filterCellValue = value;
								this.OnPropertyChanged("FilterCellValue");
                                this.BuildFilters(true);
							}
						}
						else
						{
							if (value is string && string.IsNullOrEmpty((string)value) && this._filterCellValue == null)
							{
							}
							else
							{
								this._filterCellValue = value;
								this.OnPropertyChanged("FilterCellValue");
                                this.BuildFilters(true);
							}							
						}
					}
				}
			}
		}

        #endregion // FilterCellValue

        #region RowFilterOperands
        /// <summary>
        /// Gets the filter operands which are available for this <see cref="Column"/>.
        /// </summary>
        [Browsable(false)]
        public FilterOperandCollection RowFilterOperands
        {
            get
            {
                if (this._rowFilterOperands == null)
                {
                    this._rowFilterOperands = new FilterOperandCollection();
                    if (this.Column != null)
                    {
                        this.Column.FillAvailableFilters(this._rowFilterOperands);
                        this._autopopulatedFilterOperands = true;
                    }

                    this.OnPropertyChanged("RowFilterOperands");
                }

                if (this.Column != null && this.Column.ColumnLayout != null)
                {
                    foreach (FilterOperand op in this._rowFilterOperands)
                        op.XamWebGrid = this.Column.ColumnLayout.Grid;
                }

                return this._rowFilterOperands;
            }
        }

        #endregion // RowFilterOperands

        #region FilterMenuClearFiltersString

        /// <summary>
        /// Gets / sets the string value for the FilterMenu's Clear Filter text.
        /// </summary>
        public string FilterMenuClearFiltersString
        {
            get
            {
                return this._filterMenuClearFiltersString;
            }
            set
            {
                if (this._filterMenuClearFiltersString != value)
                {
                    this._filterMenuClearFiltersString = value;
                    this.OnPropertyChanged("FilterMenuClearFiltersString");
                }
            }
        }
        #endregion // FilterMenuClearFiltersString

        #region FilterMenuClearFiltersStringResolved

        /// <summary>
        /// Gets the string that will be used for the <see cref="FilterMenuClearFiltersString"/>.
        /// </summary>
        public string FilterMenuClearFiltersStringResolved
        {
            get
            {
                string s = this.FilterMenuClearFiltersString;

                if (string.IsNullOrEmpty(s) && this.Column != null && this.Column.ColumnLayout != null)
                {
                    return this.Column.ColumnLayout.FilteringSettings.FilterMenuClearFiltersStringResolved;
                }

                return s;
            }
        }

        #endregion // FilterMenuClearFiltersString

        #region FilterMenuTypeSpecificFiltersString

        /// <summary>
        /// Gets / sets the string that should be used for the type specific 
        /// </summary>
        public string FilterMenuTypeSpecificFiltersString
        {
            get
            {
                return this._filterMenuTypeSpecificFiltersString;
            }
            set
            {
                if (this._filterMenuTypeSpecificFiltersString != value)
                {
                    this._filterMenuTypeSpecificFiltersString = value;
                    this.OnPropertyChanged("FilterMenuTypeSpecificFiltersString");
                }
            }
        }

        #endregion // FilterMenuTypeSpecificFiltersString

        #region FilterMenuCustomFilteringButtonVisibility

        /// <summary>
        /// Identifies the <see cref="FilterMenuCustomFilteringButtonVisibility"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterMenuCustomFilteringButtonVisibilityProperty = DependencyProperty.Register("FilterMenuCustomFilteringButtonVisibility", typeof(Visibility?), typeof(FilterColumnSettings), new PropertyMetadata(null, new PropertyChangedCallback(FilterMenuCustomFilteringButtonVisibilityChanged)));

        /// <summary>
        /// Gets/Sets FilterMenuCustomFilteringButtonVisibility for a particular <see cref="ColumnLayout"/>
        /// </summary>
        [TypeConverter(typeof(NullableEnumTypeConverter<Visibility>))]
        public Visibility? FilterMenuCustomFilteringButtonVisibility
        {
            get { return (Visibility?)this.GetValue(FilterMenuCustomFilteringButtonVisibilityProperty); }
            set { this.SetValue(FilterMenuCustomFilteringButtonVisibilityProperty, value); }
        }

        private static void FilterMenuCustomFilteringButtonVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterColumnSettings fcs = (FilterColumnSettings)obj;
            fcs.OnPropertyChanged("FilterMenuCustomFilteringButtonVisibility");
            fcs.OnPropertyChanged("FilterMenuCustomFilteringButtonVisibilityResolved");
        }

        #endregion // FilterMenuCustomFilteringButtonVisibility

        #region FilterMenuCustomFilteringButtonVisibilityResolved

        /// <summary>
        /// Resolves the <see cref="FilterMenuCustomFilteringButtonVisibility"/> property for a particular <see cref="FilterColumnSettings"/>
        /// </summary>
        public Visibility FilterMenuCustomFilteringButtonVisibilityResolved
        {
            get
            {
                Visibility currentVisibility = Visibility.Visible;

                if (this.FilterMenuCustomFilteringButtonVisibility == null)
                {
                    if (this.Column != null && this.Column.ColumnLayout != null)
                        currentVisibility = this.Column.ColumnLayout.FilteringSettings.FilterMenuCustomFilteringButtonVisibilityResolved;
                }
                else
                    currentVisibility = (Visibility)this.FilterMenuCustomFilteringButtonVisibility;

                return currentVisibility;
            }
        }

        #endregion //FilterMenuCustomFilteringButtonVisibilityResolved

        #region FilterMenuFormatString

        /// <summary>
        /// Identifies the <see cref="FilterMenuFormatString"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterMenuFormatStringProperty = DependencyProperty.Register("FilterMenuFormatString", typeof(string), typeof(FilterColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FilterMenuFormatStringChanged)));

        /// <summary>
        /// Gets / set the string that will be used to format items in the FilterMenu.
        /// </summary>
        public string FilterMenuFormatString
        {
            get { return (string)this.GetValue(FilterMenuFormatStringProperty); }
            set { this.SetValue(FilterMenuFormatStringProperty, value); }
        }

        private static void FilterMenuFormatStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterColumnSettings fcs = (FilterColumnSettings)obj;
            fcs.OnPropertyChanged("FilterMenuFormatString");
        }

        #endregion // FilterMenuFormatString

        #region FilterMenuFormatStringResolved

        /// <summary>
        /// Gets the string that will be used as to format the entries in the FilterMenu.
        /// </summary>
        public string FilterMenuFormatStringResolved
        {
            get
            {
                if (!string.IsNullOrEmpty(this.FilterMenuFormatString))
                    return this.FilterMenuFormatString;

                return this.Column.FormatStringResolved;
            }
        }

        #endregion // FilterMenuFormatStringResolved

        #region FilterRowCellStyle

        /// <summary>
        /// Identifies the <see cref="FilterRowCellStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterRowCellStyleProperty = DependencyProperty.Register("FilterRowCellStyle", typeof(Style), typeof(FilterColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FilterRowCellStyleChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be used for the <see cref="CellControl"/> objects on the <see cref="FilterRow"/> for this <see cref="ColumnBase"/>.
        /// </summary>
        public Style FilterRowCellStyle
        {
            get { return (Style)this.GetValue(FilterRowCellStyleProperty); }
            set { this.SetValue(FilterRowCellStyleProperty, value); }
        }

        private static void FilterRowCellStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterColumnSettings ctrl = (FilterColumnSettings)obj;
            if (ctrl.Column != null)
                ctrl.Column.OnStyleChanged();
            ctrl.OnPropertyChanged("FilterRowCellStyle");
        }

        #endregion // FilterRowCellStyle 

        #region FilterMenuOperands

        /// <summary>
        /// Gets a List of <see cref="FilterMenuTrackingObject"/> which will be displayed by the FilterMenu.
        /// </summary>
        public List<FilterMenuTrackingObject> FilterMenuOperands
        {
            get
            {

                if (this._filterMenuOps == null)
                {
                    this._filterMenuOps = new List<FilterMenuTrackingObject>();
                    if (this.Column != null)
                    {
                        this.Column.FillFilterMenuOptions(this._filterMenuOps);
                        this._autopopulatedFilterMenuOperands = true;
                    }
                    this.OnPropertyChanged("FilterMenuOperands");
                }

                this.SetFilterColumnSettings(this._filterMenuOps);

                return this._filterMenuOps;
            }
        }

        #endregion // FilterMenuOperands

        #endregion // Public

        #region Protected

        #region SilentUpdate
        /// <summary>
        /// Gets / sets if the filters being built by modifing this object should call for a rebind of data.
        /// </summary>
        protected internal bool SilentUpdate { get; set; }
        #endregion // SilentUpdate

        #region Column
        /// <summary>
        /// Gets the <see cref="Column"/> object that this settings object is applying to.
        /// </summary>
        public override Column Column
        {
            get
            {
                return base.Column;
            }
            protected internal set
            {
                if (this.Column != value)
                {
                    Column c = value;

                    base.Column = c;

                    if (c != null)
                    {
                        this._filterRowDefaultOperator = this.Column.DefaultFilterOperand;
                    }
                }
            }
        }
        #endregion // Column

        #endregion // Protected

        #endregion // Properties

        #region Methods

        #region Private

		internal void BuildFilters(bool raiseEvents)
		{
            this.BuildFilters(raiseEvents, true);
        }

        internal void BuildFilters(bool raiseEvents, bool clearExistingFilters)
        {
            if (this.Column != null)
            {
                ColumnLayout columnLayout = this.Column.ColumnLayout;

                if (columnLayout != null)
                {
                    FilteringSettingsOverride settings = columnLayout.FilteringSettings;
                    if (settings.FilteringScopeResolved == FilteringScope.ColumnLayout)
                    {
                        RowFiltersCollection rowsFiltersCollection = columnLayout.FilteringSettings.RowFiltersCollection;

                        if (columnLayout.BuildFilters(rowsFiltersCollection, this.FilterCellValue, this.Column, this.FilteringOperandResolved, this.SilentUpdate, clearExistingFilters, raiseEvents))
                        {
                            columnLayout.Grid.OnFiltered(rowsFiltersCollection);
                        }
                    }

                    columnLayout.Grid.InvalidateScrollPanel(false);
                }
            }
        }

        private void SetFilterColumnSettings(List<FilterMenuTrackingObject> list)
        {
            if (list != null)
            {
                foreach (FilterMenuTrackingObject fmto in list)
                {
                    fmto.FilterColumnSettings = this;
                    SetFilterColumnSettings(fmto.Children);
                }
            }
        }

        #endregion // Private

        #region Protected

        #region ValidateFilterOperands

        /// <summary>
        /// Called to ensure that the operand list that is set, if it was autogenerated, is correct with respect to the <see cref="Infragistics.Controls.Grids.ColumnBase.DataType"/>.
        /// </summary>
        protected internal void ValidateFilterOperands()
        {
            if (this._rowFilterOperands == null || (this._autopopulatedFilterOperands && this.Column != null))
            {
                this._rowFilterOperands = null;

                // this forces a rebuilding of the list 
                FilterOperandCollection fro = this.RowFilterOperands;

                this._filterRowDefaultOperator = this.Column.DefaultFilterOperand;

                this.OnLoadedCatchUp();                
            }
            else if (this._rowFilterOperands != null && !this._autopopulatedFilterOperands)
            {
                this.OnLoadedCatchUp();
            }

            this.OnPropertyChanged("RowFilterOperands");
        }

        #endregion // ValidateFilterOperands

        #region ValidateFilterMenuOperands

        /// <summary>
        /// Called to ensure that the filter menu list that is set, if it was autogenerated, is correct with respect to the <see cref="Infragistics.Controls.Grids.ColumnBase.DataType"/>.
        /// </summary>
        protected internal void ValidateFilterMenuOperands()
        {
            if (this._filterMenuOps == null || (this._autopopulatedFilterMenuOperands && this.Column != null))
            {
                this._filterMenuOps = null;

                List<FilterMenuTrackingObject> list = this.FilterMenuOperands;
            }
        }

        #endregion // ValidateFilterMenuOperands

        #region ValidateResolvedProperties

        /// <summary>
        /// Resolved properties may have changed values from parent levels which won't be seen at things bound at this level.  So force the Resolved properties to 
        /// reevaluate and send values through their bindings.
        /// </summary>
        protected internal void ValidateResolvedProperties()
        {
            this.OnPropertyChanged("FilterMenuFormatStringResolved");
            this.OnPropertyChanged("FilterMenuCustomFilteringButtonVisibilityResolved");
            this.OnPropertyChanged("FilterMenuClearFiltersStringResolved");
        }

        #endregion // ValidateResolvedProperties

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region OnLoadedCatchUp
        /// <summary>
        /// Designed to be called during the <see cref="XamGrid"/> OnLoaded to allow any processing         
        /// due to objects added in the XAML but not able to be processed until other objects populated.
        /// </summary>
        protected internal override void OnLoadedCatchUp()
        {
            Column column = this.Column;
            if (column != null && column.ColumnLayout != null)
            {
                ColumnLayout columnLayout = column.ColumnLayout;
                XamGrid grid = columnLayout.Grid;
                if (grid != null)
                {
                    if (columnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ColumnLayout || columnLayout.Level == 0)
                    {
                        FilterOperand fo = this.FilteringOperandResolved;

                        if (fo != null && (!fo.RequiresFilteringInput ||
                                (fo.RequiresFilteringInput && this.FilterCellValue != null)))
                        {
                           if (columnLayout.BuildFilters(grid.RowsManager.RowFiltersCollectionResolved, this.FilterCellValue, column, fo, true, true))
                            columnLayout.Grid.OnFiltered(grid.RowsManager.RowFiltersCollectionResolved);
                        }
                    }
                }
            }
        }
        #endregion // OnLoadedCatchUp

        #region Reset
        /// <summary>
        /// When overridden in derived classes allows the <see cref="ColumnSettings"/> object a chance to clean itself.
        /// </summary>
        protected internal override void Reset()
        {
            base.Reset();

            if (this._autopopulatedFilterMenuOperands && this._filterMenuOps != null)
            {
                this._filterMenuOps = null;                
            }
            
            if (_autopopulatedFilterOperands && this._rowFilterOperands != null)
            {
                foreach (FilterOperand op in this._rowFilterOperands)
                    op.XamWebGrid = null;

                this._rowFilterOperands.Clear();

                this._rowFilterOperands = null;
            }
        }
        #endregion // Reset

        #endregion // Overrides

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        List<string> _propertiesThatShouldntBePersisted;

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected override List<string> PropertiesToIgnore
        {
            get
            {
                if (this._propertiesThatShouldntBePersisted == null)
                {
                    this._propertiesThatShouldntBePersisted = new List<string>()
					{
                        "FilteringOperandResolved", 
                        "FilterMenuCustomFilteringButtonVisibilityResolved",
                        "FilterMenuFormatStringResolved",
                        "FilterMenuClearFiltersStringResolved",
                        "Column"
					};
                }

                return this._propertiesThatShouldntBePersisted;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected override List<string> PriorityProperties
        {
            get
            {
                return null;
            }
        }

        #endregion // PriorityProperties

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