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
using System.ComponentModel;
using Infragistics.Controls.Grids.Primitives;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{

	/// <summary>
	/// A class that controls the filter row editor settings for an object on the <see cref="XamGrid"/>.
	/// </summary>
    public class FilteringSettings : EditingSettingsBase, IProvidePropertyPersistenceSettings
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="FilteringSettings"/> class.
		/// </summary>
		public FilteringSettings()
		{
			this.IsOnCellActiveEditingEnabled = true;
		}
		#endregion // Constructor

		#region Properties

		#region Public

		#region AllowFiltering

		/// <summary>
		/// Identifies the <see cref="AllowFiltering"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowFilteringProperty = DependencyProperty.Register("AllowFiltering", typeof(FilterUIType), typeof(FilteringSettings), new PropertyMetadata(new PropertyChangedCallback(AllowFilteringChanged)));

		/// <summary>
		/// Gets / sets the <see cref="FilterUIType"/> which will be provided for filtering.
		/// </summary>
		public FilterUIType AllowFiltering
		{
			get { return (FilterUIType)this.GetValue(AllowFilteringProperty); }
			set { this.SetValue(AllowFilteringProperty, value); }
		}

		private static void AllowFilteringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			settings.OnPropertyChanged("AllowFiltering");
		}

		#endregion // AllowFiltering

		#region AllowFilterRow

		/// <summary>
		/// Identifies the <see cref="AllowFilterRow"/> dependency property. 
		/// </summary>		
		public static readonly DependencyProperty AllowFilterRowProperty = DependencyProperty.Register("AllowFilterRow", typeof(FilterRowLocation), typeof(FilteringSettings), new PropertyMetadata(FilterRowLocation.None, new PropertyChangedCallback(AllowFilterRowChanged)));
		
		/// <summary>
		/// Gets / sets where the <see cref="FilterRow"/> will be visible for the <see cref="XamGrid"/>.
		/// </summary>
		[Obsolete("This property is obsolete. The functionality has been superceded by the AllowFiltering property.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public FilterRowLocation AllowFilterRow
		{
			get { return (FilterRowLocation)this.GetValue(AllowFilterRowProperty); }
			set { this.SetValue(AllowFilterRowProperty, value); }
		}

		private static void AllowFilterRowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			settings.OnPropertyChanged("AllowFilterRow");
		}

		#endregion // AllowFilterRow

		#region ExpansionIndicatorStyle

		/// <summary>
		/// Identifies the <see cref="ExpansionIndicatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorStyleProperty = DependencyProperty.Register("ExpansionIndicatorStyle", typeof(Style), typeof(FilteringSettings), new PropertyMetadata(new PropertyChangedCallback(ExpansionIndicatorStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used on the <see cref="FilterRowExpansionIndicatorCellControl"/> objects of the <see cref="ColumnLayout"/>.
		/// </summary>
		public Style ExpansionIndicatorStyle
		{
			get { return (Style)this.GetValue(ExpansionIndicatorStyleProperty); }
			set { this.SetValue(ExpansionIndicatorStyleProperty, value); }
		}

		private static void ExpansionIndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			if (settings.Grid != null)
				settings.Grid.ResetPanelRows();
			settings.OnPropertyChanged("ExpansionIndicatorStyle");
		}

		#endregion // ExpansionIndicatorStyle

		#region RowFiltersCollection        
		/// <summary>
		/// Gets the <see cref="RowFiltersCollection"/> which is applied to the top level <see cref="ColumnLayout"/>.
		/// </summary>
        [Browsable(false)]
		public RowFiltersCollection RowFiltersCollection
		{
			get
			{
				RowFiltersCollection collection = null;

				if (this.Grid != null)
					collection = this.Grid.RowsManager.RowFiltersCollectionResolved;

				return collection;
			}
		}

		#endregion // RowFiltersCollection

		#region Style

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(FilteringSettings), new PropertyMetadata(null, new PropertyChangedCallback(StyleChanged)));

		/// <summary>
		/// Gets/Sets the Style for a FilterRowCell for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		/// <remarks>This is only used when the FilterRow is used as the UI filtering mechanism.</remarks>
		public Style Style
		{
			get { return (Style)this.GetValue(StyleProperty); }
			set { this.SetValue(StyleProperty, value); }
		}

		private static void StyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			settings.OnPropertyChanged("Style");
		}

		#endregion // Style

		#region RowSelectorStyle

		/// <summary>
		/// Identifies the <see cref="RowSelectorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowSelectorStyleProperty = DependencyProperty.Register("RowSelectorStyle", typeof(Style), typeof(FilteringSettings), new PropertyMetadata(null, new PropertyChangedCallback(RowSelectorStyleChanged)));

		/// <summary>
		/// Gets/Sets the style that will be applied to the RowSelector of an <see cref="AddNewRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style RowSelectorStyle
		{
			get { return (Style)this.GetValue(RowSelectorStyleProperty); }
			set { this.SetValue(RowSelectorStyleProperty, value); }
		}

		private static void RowSelectorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			settings.OnPropertyChanged("RowSelectorStyle");
		}

		#endregion // RowSelectorStyle

		#region FilteringScope

		/// <summary>
		/// Identifies the <see cref="FilteringScope"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilteringScopeProperty = DependencyProperty.Register("FilteringScope", typeof(FilteringScope), typeof(FilteringSettings), new PropertyMetadata(FilteringScope.ChildBand, new PropertyChangedCallback(FilteringScopeChanged)));

		/// <summary>
		/// Gets/Sets RowFilteringScope for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public FilteringScope FilteringScope
		{
			get { return (FilteringScope)this.GetValue(FilteringScopeProperty); }
			set { this.SetValue(FilteringScopeProperty, value); }
		}

		private static void FilteringScopeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			settings.OnPropertyChanged("FilteringScope");
		}

		#endregion // FilteringScope

		#region FilterRowHeight

		/// <summary>
		/// Identifies the <see cref="FilterRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterRowHeightProperty = DependencyProperty.Register("FilterRowHeight", typeof(RowHeight), typeof(FilteringSettings), new PropertyMetadata(RowHeight.SizeToLargestCell, new PropertyChangedCallback(FilterRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="RowHeight"/> for the <see cref="FilterRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight FilterRowHeight
		{
			get { return (RowHeight)this.GetValue(FilterRowHeightProperty); }
			set { this.SetValue(FilterRowHeightProperty, value); }
		}

		private static void FilterRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			settings.OnPropertyChanged("FilterRowHeight");
		}

		#endregion // FilterRowHeight

		#region FilterMenuClearFiltersString

		/// <summary>
		/// Identifies the <see cref="FilterMenuClearFiltersString"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterMenuClearFiltersStringProperty = DependencyProperty.Register("FilterMenuClearFiltersString", typeof(string), typeof(FilteringSettings), new PropertyMetadata(new PropertyChangedCallback(FilterMenuClearFiltersStringChanged)));

		/// <summary>
		/// Gets / sets the string that will be used for the clear text on the FilterMenu.
		/// </summary>
		public string FilterMenuClearFiltersString
		{
			get { return (string)this.GetValue(FilterMenuClearFiltersStringProperty); }
			set { this.SetValue(FilterMenuClearFiltersStringProperty, value); }
		}

		private static void FilterMenuClearFiltersStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			settings.OnPropertyChanged("FilterMenuClearFiltersString");
		}

		#endregion // FilterMenuClearFiltersString 		

		#region FilterMenuCustomFilteringButtonVisibility

		/// <summary>
		/// Identifies the <see cref="FilterMenuCustomFilteringButtonVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterMenuCustomFilteringButtonVisibilityProperty = DependencyProperty.Register("FilterMenuCustomFilteringButtonVisibility", typeof(Visibility), typeof(FilteringSettings), new PropertyMetadata(new PropertyChangedCallback(FilterMenuCustomFilteringButtonVisibilityChanged)));

		/// <summary>
		/// Gets/Sets FilterMenuCustomFilteringButtonVisibility for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Visibility FilterMenuCustomFilteringButtonVisibility
		{
			get { return (Visibility)this.GetValue(FilterMenuCustomFilteringButtonVisibilityProperty); }
			set { this.SetValue(FilterMenuCustomFilteringButtonVisibilityProperty, value); }
		}

		private static void FilterMenuCustomFilteringButtonVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettings settings = (FilteringSettings)obj;
			settings.OnPropertyChanged("FilterMenuCustomFilteringButtonVisibility");
		}

		#endregion // FilterMenuCustomFilteringButtonVisibility 				

        #region FilterSelectionControlStyle

        /// <summary>
        /// Identifies the <see cref="FilterSelectionControlStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterSelectionControlStyleProperty = DependencyProperty.Register("FilterSelectionControlStyle", typeof(Style), typeof(FilteringSettings), new PropertyMetadata(new PropertyChangedCallback(FilterSelectionControlStyleChanged)));

        /// <summary>
        /// Gets/Sets the style that will be used for the <see cref="FilterSelectionControl"/> for all <see cref="ColumnLayout"/> objects of the <see cref="XamGrid"/>
        /// </summary>
        public Style FilterSelectionControlStyle
        {
            get { return (Style)this.GetValue(FilterSelectionControlStyleProperty); }
            set { this.SetValue(FilterSelectionControlStyleProperty, value); }
        }

        private static void FilterSelectionControlStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilteringSettings settings = (FilteringSettings)obj;
            settings.OnPropertyChanged("FilterSelectionControlStyle");
        }

        #endregion // FilterSelectionControlStyle

        #region CustomFilterDialogStyle

        /// <summary>
        /// Identifies the <see cref="CustomFilterDialogStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CustomFilterDialogStyleProperty = DependencyProperty.Register("CustomFilterDialogStyle", typeof(Style), typeof(FilteringSettings), new PropertyMetadata(new PropertyChangedCallback(CustomFilterDialogStyleChanged)));

        /// <summary>
        /// Gets/Sets the style that will be used for the <see cref="ColumnFilterDialogControl"/> for all <see cref="ColumnLayout"/> objects of the <see cref="XamGrid"/>
        /// </summary>
        public Style CustomFilterDialogStyle
        {
            get { return (Style)this.GetValue(CustomFilterDialogStyleProperty); }
            set { this.SetValue(CustomFilterDialogStyleProperty, value); }
        }

        private static void CustomFilterDialogStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilteringSettings settings = (FilteringSettings)obj;
            settings.OnPropertyChanged("CustomFilterDialogStyle");
        }

        #endregion // CustomFilterDialogStyle 

        #region SelectAllText

        /// <summary>
        /// Identifies the <see cref="SelectAllText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectAllTextProperty = DependencyProperty.Register("SelectAllText", typeof(string), typeof(FilteringSettings), new PropertyMetadata(SRGrid.GetString("SelectAllCheckBox"), new PropertyChangedCallback(SelectAllTextChanged)));

        /// <summary>
        /// Gets/Sets the text for the Select All Checkbox, in the FilterMenu.
        /// </summary>
        public string SelectAllText
        {
            get { return (string)this.GetValue(SelectAllTextProperty); }
            set { this.SetValue(SelectAllTextProperty, value); }
        }

        private static void SelectAllTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // SelectAllText

        #region EmptyValueString

        /// <summary>
        /// Identifies the <see cref="EmptyValueString"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EmptyValueStringProperty = DependencyProperty.Register("EmptyValueString", typeof(string), typeof(FilteringSettings), new PropertyMetadata(SRGrid.GetString("EmptyContentString"), new PropertyChangedCallback(EmptyValueStringChanged)));

        /// <summary>
        /// Gets/Sets the text that is displayed for values in the FilterMenu that are equal to String.Empty
        /// </summary>
        public string EmptyValueString
        {
            get { return (string)this.GetValue(EmptyValueStringProperty); }
            set { this.SetValue(EmptyValueStringProperty, value); }
        }

        private static void EmptyValueStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // EmptyValueString

        #region NullValueString

        /// <summary>
        /// Identifies the <see cref="NullValueString"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NullValueStringProperty = DependencyProperty.Register("NullValueString", typeof(string), typeof(FilteringSettings), new PropertyMetadata(SRGrid.GetString("NullContentString"), new PropertyChangedCallback(NullValueStringChanged)));

        /// <summary>
        /// Gets/Sets the text that is displayed for values in the Filtermenu that are equal to Null
        /// </summary>
        public string NullValueString
        {
            get { return (string)this.GetValue(NullValueStringProperty); }
            set { this.SetValue(NullValueStringProperty, value); }
        }

        private static void NullValueStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // NullValueString 
			
        #region FilterMenuSelectionListGeneration

        /// <summary>
        /// Identifies the <see cref="FilterMenuSelectionListGeneration"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterMenuSelectionListGenerationProperty = DependencyProperty.Register("FilterMenuSelectionListGeneration", typeof(FilterMenuCumulativeSelectionList), typeof(FilteringSettings), new PropertyMetadata(FilterMenuCumulativeSelectionList.ExcelStyle, new PropertyChangedCallback(FilterMenuSelectionListGenerationChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="FilterMenuCumulativeSelectionList"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
        /// </summary>
        public FilterMenuCumulativeSelectionList FilterMenuSelectionListGeneration
        {
            get { return (FilterMenuCumulativeSelectionList)this.GetValue(FilterMenuSelectionListGenerationProperty); }
            set { this.SetValue(FilterMenuSelectionListGenerationProperty, value); }
        }

        private static void FilterMenuSelectionListGenerationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilteringSettings ctrl = (FilteringSettings)obj;
            ctrl.OnPropertyChanged("FilterMenuSelectionListGeneration");
        }

        #endregion // FilterMenuSelectionListGeneration 

        #endregion // Public

        #endregion // Properties

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        List<string> _propertiesThatShouldntBePersisted;

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                if (this._propertiesThatShouldntBePersisted == null)
                {
                    this._propertiesThatShouldntBePersisted = new List<string>()
					{
                        "RowFiltersCollection"
					};
                }

                return this._propertiesThatShouldntBePersisted;
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