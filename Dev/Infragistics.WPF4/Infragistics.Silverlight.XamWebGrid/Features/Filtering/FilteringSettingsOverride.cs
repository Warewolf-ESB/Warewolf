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

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A class that controls the editing settings for a filter row on the <see cref="ColumnLayout"/>.
	/// </summary>
	public class FilteringSettingsOverride : EditingSettingsBaseOverride, IDisposable
	{
		#region Members
		RowFiltersCollection _rowFiltersCollection;
		#endregion // Members

		#region Overrides

		#region SettingsObject

		/// <summary>
		/// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsOverrideBase"/>
		/// </summary>
		protected override SettingsBase SettingsObject
		{
			get
			{
				SettingsBase settings = null;
				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
					settings = this.ColumnLayout.Grid.FilteringSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides

		#region AllowFiltering

		/// <summary>
		/// Identifies the <see cref="AllowFiltering"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowFilteringProperty = DependencyProperty.Register("AllowFiltering", typeof(FilterUIType?), typeof(FilteringSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AllowFilteringChanged)));

		/// <summary>
		/// Gets / sets the <see cref="FilterUIType"/> which will be provided for filtering.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<FilterUIType>))]
		public FilterUIType? AllowFiltering
		{
			get { return (FilterUIType?)this.GetValue(AllowFilteringProperty); }
			set { this.SetValue(AllowFilteringProperty, value); }
		}

		private static void AllowFilteringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			settings.OnPropertyChanged("AllowFiltering");
		}

		#endregion // AllowFiltering

		#region AllowFilteringResolved

		/// <summary>
		/// Gets the <see cref="FilterUIType"/> which will be provided for filtering.
		/// </summary>
		public FilterUIType AllowFilteringResolved
		{
			get
			{
                if (this.AllowFiltering == null)
                {
                    if (this.SettingsObject != null)
                    {
                        return ((FilteringSettings)this.SettingsObject).AllowFiltering;
                    }
                }
				else
                    return (FilterUIType)this.AllowFiltering;

                return (FilterUIType)FilteringSettings.AllowFilteringProperty.GetMetadata(typeof(FilteringSettings)).DefaultValue;
			}
		}

		#endregion // AllowFilteringResolved

		#region AllowFilterRow

		/// <summary>
		/// Identifies the <see cref="AllowFilterRow"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowFilterRowProperty = DependencyProperty.Register("AllowFilterRow", typeof(FilterRowLocation?), typeof(FilteringSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AllowFilterRowChanged)));

		/// <summary>
		/// Gets / sets where the <see cref="FilterRow"/> will be located.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<FilterRowLocation>))]
		[Obsolete("This property is obsolete. The functionality has been superceded by the AllowFiltering property.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public FilterRowLocation? AllowFilterRow
		{
			get { return (FilterRowLocation?)this.GetValue(AllowFilterRowProperty); }
			set { this.SetValue(AllowFilterRowProperty, value); }
		}

		private static void AllowFilterRowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			settings.OnPropertyChanged("AllowFilterRow");
		}

		#endregion // AllowFilterRow

		#region AllowFilterRowResolved
		/// <summary>
		/// Gets where the <see cref="FilterRow"/> will be located.
		/// </summary>
		[Obsolete("This property is obsolete. The functionality has been superceded by the AllowFilteringResolved property.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public FilterRowLocation AllowFilterRowResolved
		{
            get
            {
                if (this.AllowFilterRow == null)
                {
                    if (this.SettingsObject != null)
                    {
                        return ((FilteringSettings)this.SettingsObject).AllowFilterRow;
                    }
                }
                else
                    return (FilterRowLocation)this.AllowFilterRow;

                return (FilterRowLocation)FilteringSettings.AllowFilterRowProperty.GetMetadata(typeof(FilteringSettings)).DefaultValue;
            }
		}
		#endregion // AllowFilterRowResolved

		#region ExpansionIndicatorStyle

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorStyleProperty = DependencyProperty.Register("ExpansionIndicatorStyle", typeof(Style), typeof(FilteringSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(ExpansionIndicatorStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style ExpansionIndicatorStyle
		{
			get { return (Style)this.GetValue(ExpansionIndicatorStyleProperty); }
			set { this.SetValue(ExpansionIndicatorStyleProperty, value); }
		}

		private static void ExpansionIndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			if (settings.ColumnLayout != null && settings.ColumnLayout.Grid != null)
				settings.ColumnLayout.Grid.ResetPanelRows();
			settings.OnPropertyChanged("ExpansionIndicatorStyle");
		}

		#endregion // ExpansionIndicatorStyle

		#region ExpansionIndicatorStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style ExpansionIndicatorStyleResolved
		{
			get
			{
				if (this.ExpansionIndicatorStyle == null && this.SettingsObject != null)
					return ((FilteringSettings)this.SettingsObject).ExpansionIndicatorStyle;
				else
					return this.ExpansionIndicatorStyle;
			}
		}

		#endregion // ExpansionIndicatorStyleResolved

		#region Style

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(FilteringSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(StyleChanged)));

		/// <summary>
		/// Gets/Sets the Style for a FilterRowCell for a particular <see cref="ColumnLayout"/>
		/// </summary>
		/// <remarks>This is only used when the FilterRow is used as the UI filtering mechanism.</remarks>
		public Style Style
		{
			get { return (Style)this.GetValue(StyleProperty); }
			set { this.SetValue(StyleProperty, value); }
		}

		private static void StyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			settings.OnPropertyChanged("Style");
		}

		#endregion // Style

		#region StyleResolved

		/// <summary>
		/// Resolves the <see cref="FilteringSettingsOverride.Style"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style StyleResolved
		{
			get
			{
				if (this.Style == null && this.SettingsObject != null)
					return ((FilteringSettings)this.SettingsObject).Style;
				else
					return (Style)this.Style;
			}
		}

		#endregion //StyleResolved

        #region FilterSelectionControlStyle

        /// <summary>
        /// Identifies the <see cref="FilterSelectionControlStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterSelectionControlStyleProperty = DependencyProperty.Register("FilterSelectionControlStyle", typeof(Style), typeof(FilteringSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FilterSelectionControlStyleChanged)));

        /// <summary>
        /// Gets/Sets the style that will be used for the <see cref="FilterSelectionControl"/> for a particular <see cref="ColumnLayout"/>
        /// </summary>
        public Style FilterSelectionControlStyle
        {
            get { return (Style)this.GetValue(FilterSelectionControlStyleProperty); }
            set { this.SetValue(FilterSelectionControlStyleProperty, value); }
        }

        private static void FilterSelectionControlStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
            settings.OnPropertyChanged("FilterSelectionControlStyle");
        }

        #endregion // FilterSelectionControlStyle

        #region FilterSelectionControlStyleResolved

        /// <summary>
        /// Gets the actual <see cref="Style"/> of the visual object.
        /// </summary>
        public Style FilterSelectionControlStyleResolved
        {
            get
            {
                if (this.FilterSelectionControlStyle == null && this.SettingsObject != null)
                    return ((FilteringSettings)this.SettingsObject).FilterSelectionControlStyle;
                else
                    return this.FilterSelectionControlStyle;
            }
        }

        #endregion // FilterSelectionControlStyleResolved

        #region CustomFilterDialogStyle

        /// <summary>
        /// Identifies the <see cref="CustomFilterDialogStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CustomFilterDialogStyleProperty = DependencyProperty.Register("CustomFilterDialogStyle", typeof(Style), typeof(FilteringSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(CustomFilterDialogStyleChanged)));

        /// <summary>
        /// Gets/Sets the style that will be used for the <see cref="ColumnFilterDialogControl"/> for a particual <see cref="ColumnLayout"/>
        /// </summary>
        public Style CustomFilterDialogStyle
        {
            get { return (Style)this.GetValue(CustomFilterDialogStyleProperty); }
            set { this.SetValue(CustomFilterDialogStyleProperty, value); }
        }

        private static void CustomFilterDialogStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
            settings.OnPropertyChanged("CustomFilterDialogStyle");
        }

        #endregion // CustomFilterDialogStyle 

        #region CustomFilterDialogStyleResolved

        /// <summary>
        /// Gets the actual <see cref="Style"/> of the visual object.
        /// </summary>
        public Style CustomFilterDialogStyleResolved
        {
            get
            {
                if (this.CustomFilterDialogStyle == null && this.SettingsObject != null)
                    return ((FilteringSettings)this.SettingsObject).CustomFilterDialogStyle;
                else
                    return this.CustomFilterDialogStyle;
            }
        }

        #endregion // CustomFilterDialogStyleResolved

        #region RowSelectorStyle

        /// <summary>
		/// Identifies the <see cref="RowSelectorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowSelectorStyleProperty = DependencyProperty.Register("RowSelectorStyle", typeof(Style), typeof(FilteringSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(RowSelectorStyleChanged)));

		/// <summary>
		/// Gets/Sets the style that will be applied to the RowSelector of an <see cref="FilterRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style RowSelectorStyle
		{
			get { return (Style)this.GetValue(RowSelectorStyleProperty); }
			set { this.SetValue(RowSelectorStyleProperty, value); }
		}

		private static void RowSelectorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			settings.OnPropertyChanged("RowSelectorStyle");
		}

		#endregion // RowSelectorStyle

		#region RowSelectorStyleResolved

		/// <summary>
		/// Resolves the <see cref="FilteringSettingsOverride.RowSelectorStyle"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style RowSelectorStyleResolved
		{
			get
			{
				if (this.RowSelectorStyle == null && this.SettingsObject != null)
					return ((FilteringSettings)this.SettingsObject).RowSelectorStyle;

				return (Style)this.RowSelectorStyle;
			}
		}

		#endregion //RowSelectorStyleResolved

		#region FilteringScope

		/// <summary>
		/// Identifies the <see cref="FilteringScope"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilteringScopeProperty = DependencyProperty.Register("FilteringScope", typeof(FilteringScope?), typeof(FilteringSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(FilteringScopeChanged)));

		/// <summary>
		/// Gets/Sets RowFilteringScope for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<FilteringScope>))]
		public FilteringScope? FilteringScope
		{
			get { return (FilteringScope?)this.GetValue(FilteringScopeProperty); }
			set { this.SetValue(FilteringScopeProperty, value); }
		}

		private static void FilteringScopeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			settings.OnPropertyChanged("FilteringScope");
		}

		#endregion // FilteringScope

		#region FilteringScopeResolved

		/// <summary>
		/// Resolves the <see cref="FilteringSettingsOverride.FilteringScope"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public FilteringScope FilteringScopeResolved
		{
			get
			{
                if (this.FilteringScope == null)
                {
                    if (this.SettingsObject != null)
                        return ((FilteringSettings)this.SettingsObject).FilteringScope;
                }
                else
                    return (FilteringScope)this.FilteringScope;

                return (FilteringScope)FilteringSettings.FilteringScopeProperty.GetMetadata(typeof(FilteringSettings)).DefaultValue;
			}
		}

		#endregion //FilteringScopeResolved

		#region RowFiltersCollection
        
		/// <summary>
		/// Gets a <see cref="RowFiltersCollection"/> object that contains the filters being applied to this <see cref="ColumnLayout"/>.
		/// </summary>
		/// <remarks>
		/// This collection is used when the <see cref="FilteringScope"/> is set to ColumnLayout.
		/// </remarks>
        [Browsable(false)]
		public RowFiltersCollection RowFiltersCollection
		{
			get
			{
				if (this._rowFiltersCollection == null)
				{
					this._rowFiltersCollection = new RowFiltersCollection();
					this._rowFiltersCollection.CollectionChanged += RowFiltersCollection_CollectionChanged;
					this._rowFiltersCollection.CollectionItemChanged += RowFiltersCollection_CollectionItemChanged;
					this._rowFiltersCollection.PropertyChanged += RowFiltersCollection_PropertyChanged;
				}
				return this._rowFiltersCollection;
			}
		}



		#endregion // RowFiltersCollection

		#region ResolveEditingType
		/// <summary>
		/// Determines what <see cref="EditingType"/> will be supported by the <see cref="EditingSettingsBaseOverride"/> object.
		/// </summary>
		/// <returns></returns>
		protected internal override EditingType ResolveEditingType()
		{
			return EditingType.Cell;
		}
		#endregion // ResolveEditingType

		#region FilterRowHeight

		/// <summary>
		/// Identifies the <see cref="FilterRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterRowHeightProperty = DependencyProperty.Register("FilterRowHeight", typeof(RowHeight?), typeof(FilteringSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(FilterRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="RowHeight"/> for the <see cref="FilterRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? FilterRowHeight
		{
			get { return (RowHeight?)this.GetValue(FilterRowHeightProperty); }
			set { this.SetValue(FilterRowHeightProperty, value); }
		}

		private static void FilterRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			settings.OnPropertyChanged("FilterRowHeight");
		}

		#endregion // FilterRowHeight

		#region FilterRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="FilteringSettingsOverride.FilterRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight FilterRowHeightResolved
		{
			get
			{
                if (this.FilterRowHeight == null)
                {
                    if (this.SettingsObject != null)
                        return ((FilteringSettings)this.SettingsObject).FilterRowHeight;
                }
                else
                    return (RowHeight)this.FilterRowHeight;

                return (RowHeight)FilteringSettings.FilterRowHeightProperty.GetMetadata(typeof(FilteringSettings)).DefaultValue;
			}
		}

		#endregion //FilterRowHeightResolved

		#region FilterMenuClearFiltersString

		/// <summary>
		/// Identifies the <see cref="FilterMenuClearFiltersString"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterMenuClearFiltersStringProperty = DependencyProperty.Register("FilterMenuClearFiltersString", typeof(string), typeof(FilteringSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FilterMenuClearFiltersStringChanged)));

		/// <summary>
		/// Gets / sets the string value for the FilterMenu's Clear Filter text.
		/// </summary>
		public string FilterMenuClearFiltersString
		{
			get { return (string)this.GetValue(FilterMenuClearFiltersStringProperty); }
			set { this.SetValue(FilterMenuClearFiltersStringProperty, value); }
		}

		private static void FilterMenuClearFiltersStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			settings.OnPropertyChanged("FilterMenuClearFiltersString");
		}

		#endregion // FilterMenuClearFiltersString

		#region FilterMenuClearFiltersStringResolved

		/// <summary>
		/// Gets the string will be used for the clear filter option of the FilterMenu feature.
		/// </summary>
		public string FilterMenuClearFiltersStringResolved
		{
			get
			{
				string s = this.FilterMenuClearFiltersString;
				if (string.IsNullOrEmpty(s))
				{
					FilteringSettings fs = this.SettingsObject as FilteringSettings;
					if (fs != null)
						return fs.FilterMenuClearFiltersString;
				}
				return s;
			}
		}

		#endregion // FilterMenuClearFiltersStringResolved

		#region FilterMenuCustomFilteringButtonVisibility

		/// <summary>
		/// Identifies the <see cref="FilterMenuCustomFilteringButtonVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterMenuCustomFilteringButtonVisibilityProperty = DependencyProperty.Register("FilterMenuCustomFilteringButtonVisibility", typeof(Visibility?), typeof(FilteringSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(FilterMenuCustomFilteringButtonVisibilityChanged)));

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
			FilteringSettingsOverride settings = (FilteringSettingsOverride)obj;
			settings.OnPropertyChanged("FilterMenuCustomFilteringButtonVisibility");
		}

		#endregion // FilterMenuCustomFilteringButtonVisibility

		#region FilterMenuCustomFilteringButtonVisibilityResolved

		/// <summary>
		/// Resolves the <see cref="FilteringSettingsOverride.FilterMenuCustomFilteringButtonVisibility"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
        public Visibility FilterMenuCustomFilteringButtonVisibilityResolved
        {
            get
            {
                if (this.FilterMenuCustomFilteringButtonVisibility == null)
                {
                    if (this.SettingsObject != null)
                        return ((FilteringSettings)this.SettingsObject).FilterMenuCustomFilteringButtonVisibility;
                }
                else
                    return (Visibility)this.FilterMenuCustomFilteringButtonVisibility;


                return (Visibility)FilteringSettings.FilterMenuCustomFilteringButtonVisibilityProperty.GetMetadata(typeof(FilteringSettings)).DefaultValue;
            }
        }

		#endregion //FilterMenuCustomFilteringButtonVisibilityResolved

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="FilteringSettingsOverride"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._rowFiltersCollection != null)
			{
				this._rowFiltersCollection.CollectionChanged -= RowFiltersCollection_CollectionChanged;
				this._rowFiltersCollection.CollectionItemChanged -= RowFiltersCollection_CollectionItemChanged;
				this._rowFiltersCollection.Dispose();
				this._rowFiltersCollection = null;
			}
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="RowsManagerBase"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region EventHandlers

		void RowFiltersCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.OnPropertyChanged("FilteringInvalidated");
		}

		void RowFiltersCollection_CollectionItemChanged(object sender, EventArgs e)
		{
			this.OnPropertyChanged("FilteringInvalidated");
		}

		void RowFiltersCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnPropertyChanged("FilteringInvalidated");
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