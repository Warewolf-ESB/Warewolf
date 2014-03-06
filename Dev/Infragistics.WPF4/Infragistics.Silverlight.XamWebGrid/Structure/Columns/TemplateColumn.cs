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
using System.Windows.Data;
using Infragistics.Controls.Grids.Primitives;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A column that allows the user to specify what content will get displayed via it's <see cref="ItemTemplate"/> property.
	/// </summary>
	/// <remarks>Note: the <see cref="TemplateColumn"/> does not recycle it's content.</remarks>
	public class TemplateColumn : EditableColumn
    {
        #region Members

        string _filterMenuCustomFilterString;

        #endregion // Members

        #region Constructor
        /// <summary>
		/// Initializes a new instance of the <see cref="TemplateColumn"/> class.
		/// </summary>
		public TemplateColumn()
			:base()
		{
			
		}
		#endregion 

		#region Properties

		#region Public

		#region ItemTemplate

		/// <summary>
		/// Identifies the <see cref="ItemTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TemplateColumn), new PropertyMetadata(new PropertyChangedCallback(ItemTemplateChanged)));

		/// <summary>
		/// Gets / sets the <see cref="DataTemplate"/> that will be used to generate content for each <see cref="CellBase"/> of the <see cref="TemplateColumn"/>.
		/// </summary>
		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
			set { this.SetValue(ItemTemplateProperty, value); }
		}

		private static void ItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
            TemplateColumn column = (TemplateColumn)obj;

            if (column.ColumnLayout != null && column.ColumnLayout.Grid != null && column.ColumnLayout.Grid.IsLoaded)
            {
                column.ColumnLayout.Grid.ResetPanelRows(true);
            }
		}

		#endregion // ItemTemplate 

		#region EditorTemplate

		/// <summary>
		/// Identifies the <see cref="EditorTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty EditorTemplateProperty = DependencyProperty.Register("EditorTemplate", typeof(DataTemplate), typeof(TemplateColumn), new PropertyMetadata(new PropertyChangedCallback(EditorTemplateChanged)));

		/// <summary>
		/// Gets/sets the Editor that will be displayed when a <see cref="Cell"/> in this <see cref="TemplateColumn"/> is in edit mode.
		/// </summary>
		public DataTemplate EditorTemplate
		{
			get { return (DataTemplate)this.GetValue(EditorTemplateProperty); }
			set { this.SetValue(EditorTemplateProperty, value); }
		}

		private static void EditorTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TemplateColumn col = (TemplateColumn)obj;
			col.EditorTemplateDirtyFlag++;
		}

		#endregion // EditorTemplate 

		#region FilterItemTemplate

		/// <summary>
		/// Identifies the <see cref="FilterItemTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterItemTemplateProperty = DependencyProperty.Register("FilterItemTemplate", typeof(DataTemplate), typeof(TemplateColumn), new PropertyMetadata(new PropertyChangedCallback(FilterItemTemplateChanged)));

		/// <summary>
		/// Gets / sets the <see cref="DataTemplate"/> that will be used to generate content for the <see cref="CellBase"/> of the <see cref="TemplateColumn"/> in the <see cref="FilterRowCell"/>.
		/// </summary>
		/// <remarks>
		/// If this property is null then the <see cref="ItemTemplate"/> is used.
		/// </remarks>
		public DataTemplate FilterItemTemplate
		{
			get { return (DataTemplate)this.GetValue(FilterItemTemplateProperty); }
			set { this.SetValue(FilterItemTemplateProperty, value); }
		}

		private static void FilterItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TemplateColumn col = (TemplateColumn)obj;
			col.OnPropertyChanged("FilterItemTemplate");
		}

		#endregion // FilterItemTemplate 
				
		#region FilterEditorTemplate

		/// <summary>
		/// Identifies the <see cref="FilterEditorTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterEditorTemplateProperty = DependencyProperty.Register("FilterEditorTemplate", typeof(DataTemplate), typeof(TemplateColumn ), new PropertyMetadata(new PropertyChangedCallback(FilterEditorTemplateChanged)));

		/// <summary>
		/// Gets / sets the Editor that will be displayed when this <see cref="FilterRowCell"/> in this <see cref="TemplateColumn"/> is in edit mode.
		/// </summary>
		/// <remarks>
		/// If this property is null then the <see cref="EditorTemplate"/> is used.
		/// </remarks>
		public DataTemplate FilterEditorTemplate
		{
			get { return (DataTemplate)this.GetValue(FilterEditorTemplateProperty); }
			set { this.SetValue(FilterEditorTemplateProperty, value); }
		}

		private static void FilterEditorTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TemplateColumn col = (TemplateColumn)obj;
			col.OnPropertyChanged("FilterEditorTemplate");
		}

		#endregion // FilterEditorTemplate 

        #region FilterMenuCustomFilterString
        /// <summary>
        /// Gets the default string for the FilterMenu for the CustomFilter text
        /// </summary>
        protected override string FilterMenuCustomFilterString
        {
            get
            {
                if (string.IsNullOrEmpty(this._filterMenuCustomFilterString))
                    return base.FilterMenuCustomFilterString;

                return SRGrid.GetString(this._filterMenuCustomFilterString);
            }
        }
        #endregion // FilterMenuCustomFilterString
				
		#endregion // Public

		#region Internal

		internal int EditorTemplateDirtyFlag
		{
			get;
			set;
		}

		#endregion // Internal

		#endregion // Properties

		#region Overrides

		#region IsSortable

		/// <summary>
		/// Gets/Sets whether the <see cref="TemplateColumn"/> is sortable. 		 
		/// </summary>
		/// <remarks>
		/// In order for a TemplateColumn to be sortable, it must have a Key that maps to a property on the underlying datasource.
		/// </remarks>
		public override bool IsSortable
		{
			get
			{
				return base.IsSortable && (this.DataType != null || this.SortComparer != null);
			}
			set
			{
				base.IsSortable = value;
			}
		}

		#endregion // IsSortable

		#region GenerateContentProvider
		/// <summary>
		/// Generates a new <see cref="TemplateColumnContentProvider"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override ColumnContentProviderBase GenerateContentProvider()
		{
			return new TemplateColumnContentProvider();
		}

		#endregion // GenerateContentProvider

		#region IsEditable

		/// <summary>
		/// Resolves whether this <see cref="Column"/> supports editing.
		/// </summary>
		protected internal override bool IsEditable
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                return !this.IsReadOnly;
			}
		}

		#endregion // IsEditable

		#region UniqueColumnContent

		/// <summary>
		/// Resolves whether <see cref="Cell"/> objects can be recycled and shared amongst columns of the same type.
		/// </summary>
		/// <remarks>
		/// Note: If false, then the Cells generated by this Column's ContentProvider will be 
		/// shared between other column's of this specific type.
		/// </remarks>
		protected internal override bool UniqueColumnContent
		{
			get { return true; }
		}

		#endregion // UniqueColumnContent

		#region EditorValueConverter

		/// <summary>
		/// This property isn't supported on this <see cref="Column"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new IValueConverter EditorValueConverter
		{
			get { return (IValueConverter)this.GetValue(EditorValueConverterProperty); }
			set { this.SetValue(EditorValueConverterProperty, value); }
		}

	
		#endregion // EditorValueConverter

		#region EditorValueConverterParameter
		/// <summary>
		/// This property isn't supported on this <see cref="Column"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new object EditorValueConverterParameter
		{
			get { return (object)this.GetValue(EditorValueConverterParameterProperty); }
			set { this.SetValue(EditorValueConverterParameterProperty, value); }
		}

		#endregion // EditorValueConverterParameter

		#region ValueConverter

		/// <summary>
		/// This property isn't supported on this <see cref="Column"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new IValueConverter ValueConverter
		{
			get { return (IValueConverter)this.GetValue(ValueConverterProperty); }
			set { this.SetValue(ValueConverterProperty, value); }
		}

		#endregion // ValueConverter

		#region ValueConverterParameter

		/// <summary>
		/// This property isn't supported on this <see cref="Column"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new object ValueConverterParameter
		{
			get { return (object)this.GetValue(ValueConverterParameterProperty); }
			set { this.SetValue(ValueConverterParameterProperty, value); }
		}

		#endregion // ValueConverterParameter

        #region UseReadOnlyFlag

        /// <summary>
        /// Determines if the <see cref="Column"/> should use the ReadOnly flag on a property, to determine if it can enter edit mode.
        /// </summary>
        protected internal override bool UseReadOnlyFlag
        {
            get { return false; }
        }

        #endregion // UseReadOnlyFlag

        #region FillAvailableFilters

        /// <summary>
        /// Fills the <see cref="FilterOperandCollection"/> with the operands that the column expects as filter values.
        /// </summary>
        /// <param name="availableFilters"></param>
        protected internal override void FillAvailableFilters(FilterOperandCollection availableFilters)
        {
            base.FillAvailableFilters(availableFilters);

            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
            {
                Dictionary<ComparisonOperator, DataTemplate> icons = this.ColumnLayout.Grid.FilterIcons;
                if (icons != null && this.DataType != null)
                {
                    if (this.DataType == typeof(string))
                    {
                        //this.DefaultFilterOperand = new StartsWithOperand();

                        availableFilters.Add(new StartsWithOperand() { XamWebGrid = this.ColumnLayout.Grid });
                        availableFilters.Add(new ContainsOperand() { XamWebGrid = this.ColumnLayout.Grid });
                        availableFilters.Add(new EndsWithOperand() { XamWebGrid = this.ColumnLayout.Grid });
                        availableFilters.Add(new DoesNotContainOperand() { XamWebGrid = this.ColumnLayout.Grid });
                        availableFilters.Add(new DoesNotStartWithOperand() { XamWebGrid = this.ColumnLayout.Grid });
                        availableFilters.Add(new DoesNotEndWithOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    }
                    else if (this.DataType.IsValueType)
                    {
                        
                        availableFilters.Add(new GreaterThanOperand() { XamWebGrid = this.ColumnLayout.Grid });
                        availableFilters.Add(new GreaterThanOrEqualOperand() { XamWebGrid = this.ColumnLayout.Grid });
                        availableFilters.Add(new LessThanOperand() { XamWebGrid = this.ColumnLayout.Grid });
                        availableFilters.Add(new LessThanOrEqualOperand() { XamWebGrid = this.ColumnLayout.Grid });
                    }
                }
            }
        }
        #endregion // FillAvailableFilters

        #region FillFilterMenuOptions
        /// <summary>
        /// Fills the inputted list with options for the FilterMenu control.
        /// </summary>
        /// <param name="list"></param>
        internal protected override void FillFilterMenuOptions(List<FilterMenuTrackingObject> list)
        {
            base.FillFilterMenuOptions(list);

            if (this.DataType != null && this.ColumnLayout != null && this.ColumnLayout.Grid != null)
            {
                if (list != null && list.Count > 0)
                {
                    FilterMenuTrackingObject fmto = list[0];
                    list = fmto.Children;
                    if (this.DataType == typeof(string))
                    {
                        list.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                        list.Add(new FilterMenuTrackingObject(new StartsWithOperand() { DisplayName = SRGrid.GetString("StartsWithEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new EndsWithOperand() { DisplayName = SRGrid.GetString("EndsWithEllipsis") }));
                        list.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                        list.Add(new FilterMenuTrackingObject(new ContainsOperand() { DisplayName = SRGrid.GetString("ContainsEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new DoesNotContainOperand() { DisplayName = SRGrid.GetString("DoesNotContainEllipsis") }));
                        list.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                        list.Add(new FilterMenuTrackingObject(new DoesNotStartWithOperand() { DisplayName = SRGrid.GetString("DoesNotStartWithEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new DoesNotEndWithOperand() { DisplayName = SRGrid.GetString("DoesNotEndWithEllipsis") }));
                    }
                    else if ((this.DataType.IsPrimitive || FilterRow.IsNullableValueType(this.DataType))
                        && this.DataType != typeof(bool) && this.DataType != typeof(bool?)
                        && this.DataType != typeof(DateTime) && this.DataType != typeof(DateTime?)
                        )
                    {
                        
                        list.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                        list.Add(new FilterMenuTrackingObject(new GreaterThanOperand() { DisplayName = SRGrid.GetString("GreaterThanEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new GreaterThanOrEqualOperand() { DisplayName = SRGrid.GetString("GreaterThanOrEqualEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new LessThanOperand() { DisplayName = SRGrid.GetString("LessThanEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new LessThanOrEqualOperand() { DisplayName = SRGrid.GetString("LessThanOrEqualEllipsis") }));

                        FilterMenuTrackingObject obj = new FilterMenuTrackingObject();
                        obj.Label = SRGrid.GetString("BetweenEllipsis");
                        obj.FilterOperands.Add(new GreaterThanOrEqualOperand());
                        obj.FilterOperands.Add(new LessThanOrEqualOperand());
                        list.Add(obj);
                    }
                }
            }
        }
        #endregion // FillFilterMenuOptions

        #region OnDataTypeChanged
        /// <summary>
        /// Raised when the DataType of the <see cref="ColumnBase" /> is changed.
        /// </summary>
        protected override void OnDataTypeChanged()
        {
            if (this.DataType == typeof(string))
            {
                this._filterMenuCustomFilterString = "StringFilters";
            }
            else if ((this.DataType.IsPrimitive || FilterRow.IsNullableValueType(this.DataType)) &&
                this.DataType != typeof(bool) && this.DataType != typeof(bool?) &&
                this.DataType != typeof(DateTime) && this.DataType != typeof(DateTime?)
                )
            {
                this._filterMenuCustomFilterString = "NumberFilters";
            }
            else if (this.DataType == typeof(bool) || this.DataType == typeof(bool?))
            {
                this._filterMenuCustomFilterString = "BoolFilters";
            }
            base.OnDataTypeChanged();
        }
        #endregion // OnDataTypeChanged

		#endregion // Overrides
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