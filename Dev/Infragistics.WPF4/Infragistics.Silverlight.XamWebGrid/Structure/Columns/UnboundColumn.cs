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
using Infragistics.Controls.Grids.Primitives;
using System.Windows.Data;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A <see cref="Column"/> which can be used to show calculated data.
	/// </summary>
	public class UnboundColumn : EditableColumn
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="UnboundColumn"/> class.
		/// </summary>
		public UnboundColumn()
			: base()
		{
			this.IsFilterable = false;
			this.IsSummable = false;
			this.DataType = typeof(object);
		}

		#endregion // Constructor

		#region Overrides

		#region Properties

		#region DefaultFilterOperand

		/// <summary>
		/// The default <see cref="FilterOperand"/> for this column type;
		/// </summary>
		protected internal override FilterOperand DefaultFilterOperand
		{
			get
			{
				return null;
			}
		}
		#endregion // DefaultFilterOperand		

        #region UseReadOnlyFlag

        /// <summary>
        /// Determines if the <see cref="Column"/> should use the ReadOnly flag on a property, to determine if it can enter edit mode.
        /// </summary>
        protected internal override bool UseReadOnlyFlag
        {
            get { return false; }
        }

        #endregion // UseReadOnlyFlag


        #region IsEditable

        /// <summary>
        /// Resolves whether this <see cref="Column"/> supports editing.
        /// </summary>
        protected internal override bool IsEditable
        {
            get { return base.IsEditable && (this.EditorTemplate != null || this.AddNewRowEditorTemplate != null); }
        }

        #endregion // IsEditable


		#endregion // Properties

		#endregion // Overrides

		#region Internal

		internal int EditorTemplateDirtyFlag
		{
			get;
			set;
		}

		#endregion // Internal

		#region Properties

		#region Public

		#region ItemTemplate

		/// <summary>
		/// Identifies the <see cref="ItemTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(UnboundColumn), new PropertyMetadata(new PropertyChangedCallback(ItemTemplateChanged)));

		/// <summary>
		/// Gets the <see cref="DataTemplate"/> that will be used to generate content for each <see cref="CellBase"/> of the <see cref="UnboundColumn"/>.
		/// </summary>
		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
			set { this.SetValue(ItemTemplateProperty, value); }
		}

		private static void ItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion // ItemTemplate

		#region EditorTemplate

		/// <summary>
		/// Identifies the <see cref="EditorTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty EditorTemplateProperty = DependencyProperty.Register("EditorTemplate", typeof(DataTemplate), typeof(UnboundColumn), new PropertyMetadata(new PropertyChangedCallback(EditorTemplateChanged)));

		/// <summary>
		/// Gets/sets the Editor that will be displayed when a <see cref="Cell"/> in this <see cref="UnboundColumn"/> is in edit mode.
		/// </summary>
		public DataTemplate EditorTemplate
		{
			get { return (DataTemplate)this.GetValue(EditorTemplateProperty); }
			set { this.SetValue(EditorTemplateProperty, value); }
		}

		private static void EditorTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			UnboundColumn col = (UnboundColumn)obj;
			col.EditorTemplateDirtyFlag++;
		}

		#endregion // EditorTemplate

		#region FilterItemTemplate

		/// <summary>
		/// Identifies the <see cref="FilterItemTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterItemTemplateProperty = DependencyProperty.Register("FilterItemTemplate", typeof(DataTemplate), typeof(UnboundColumn), new PropertyMetadata(new PropertyChangedCallback(FilterItemTemplateChanged)));

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
			UnboundColumn col = (UnboundColumn)obj;
			col.OnPropertyChanged("FilterItemTemplate");
		}

		#endregion // FilterItemTemplate

		#region FilterEditorTemplate

		/// <summary>
		/// Identifies the <see cref="FilterEditorTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FilterEditorTemplateProperty = DependencyProperty.Register("FilterEditorTemplate", typeof(DataTemplate), typeof(UnboundColumn ), new PropertyMetadata(new PropertyChangedCallback(FilterEditorTemplateChanged)));

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
			UnboundColumn col = (UnboundColumn)obj;
			col.OnPropertyChanged("FilterEditorTemplate");
		}

		#endregion // FilterEditorTemplate 

		#endregion // Public

        #region Protected

        #region ResetCellValueObjectAfterEditing

        /// <summary>
        /// Gets if the <see cref="Column"/> should reset the CellValueObject when exiting edit mode.
        /// </summary>
        protected internal override bool ResetCellValueObjectAfterEditing
        {
            get
            {
                return false;
            }
        }

        #endregion // ResetCellValueObjectAfterEditing
        
        #endregion // Protected

        #endregion // Properties

        #region Overrides

        #region RequiresBoundDataKey
        /// <summary>
		/// Gets whether an exception should be thrown if the key associated with the <see cref="ColumnBase"/> doesn't 
		/// correspond with a property in the data that this object represents.
		/// </summary>
		protected internal override bool RequiresBoundDataKey
		{
			get
			{
				return false;
			}
		}
		#endregion // RequiresBoundDataKey

		#region IsSortable

		/// <summary>
		/// Gets/Sets whether the <see cref="UnboundColumn"/> is sortable. 		 
		/// </summary>
		/// <remarks>
        /// In order for a Unbound to be sortable, it must have a <see cref="Infragistics.Controls.Grids.Column.SortComparer" /> or <see cref="Infragistics.Controls.Grids.Column.ValueConverter"/>.
		/// </remarks>
		public override bool IsSortable
		{
			get
			{			
				return base.IsSortable && (this.ValueConverter != null || this.SortComparer != null);
			}
			set
			{
				base.IsSortable = value;
			}
		}

		#endregion // IsSortable

		#region GenerateDataCell

		/// <summary>
		/// Returns a new instance of a <see cref="Cell"/>
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected override CellBase GenerateDataCell(RowBase row)
		{
			if (row.RowType == RowType.AddNewRow)
				return new AddNewRowCell(row, this);

			if (row.RowType == RowType.FilterRow)
				return new FilterRowCell(row, this);

			if (row.RowType == RowType.SummaryRow)
				return new SummaryRowCell(row, this);

            if (this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
				return new ConditionalFormattingUnboundCell(row, this);
					
			return new UnboundCell(row, this);
		}

		#endregion // GenerateDataCell

		#region GenerateContentProvider

		/// <summary>
		/// Generates a new <see cref="UnboundColumnContentProvider"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override ColumnContentProviderBase GenerateContentProvider()
		{
			return new UnboundColumnContentProvider();
		}

		#endregion // GenerateContentProvider

		#region FillAvailableFilters
		/// <summary>
		/// Fills the <see cref="FilterOperandCollection"/> with the operands that the column expects as filter values.
		/// </summary>
		/// <param name="availableFilters"></param>
		protected internal override void FillAvailableFilters(FilterOperandCollection availableFilters)
		{

		}
		#endregion // FillAvailableFilters

		#region FillAvailableSummaries

		/// <summary>
		/// Fills the <see cref="SummaryOperandCollection"/> with the operands that the column expects as summary values.
		/// </summary>
		/// <param name="availableSummaries"></param>
		protected internal override void FillAvailableSummaries(SummaryOperandCollection availableSummaries)
		{

		}

		#endregion // FillAvailableSummaries		

		#region ValidateSortComparer
		
		/// <summary>
        /// Used to validate the <see cref="Infragistics.Controls.Grids.Column.SortComparer"/> can be used on this <see cref="Column"/>.
		/// </summary>
		protected override void ValidateSortComparer()
		{
			if (this.SortComparer != null && this.ColumnLayout != null && this.ColumnLayout.ObjectDataType != null)
			{
				System.Type typeOfIComp = typeof(IComparer<>).MakeGenericType(new System.Type[] { this.ColumnLayout.ObjectDataType });

				if (!typeOfIComp.IsAssignableFrom(this.SortComparer.GetType()))
				{
					throw new TypeLoadException(string.Format(System.Threading.Thread.CurrentThread.CurrentCulture, SRGrid.GetString("TypeMismatchException"), this.SortComparer.GetType().Name, typeOfIComp.Name));
				}
			}
		}

		#endregion // ValidateSortComparer

		#region ValidateGroupByComparer

		/// <summary>
        /// Used to validate the <see cref="Infragistics.Controls.Grids.Column.GroupByComparer"/> can be used on this <see cref="Column"/>.
		/// </summary>
		protected override void ValidateGroupByComparer()
		{
			if (this.GroupByComparer != null && this.ColumnLayout != null && this.ColumnLayout.ObjectDataType != null)
			{
				System.Type typeOfIComp = typeof(IEqualityComparer<>).MakeGenericType(new System.Type[] { this.ColumnLayout.ObjectDataType });

				if (!typeOfIComp.IsAssignableFrom(this.GroupByComparer.GetType()))
				{
					throw new TypeLoadException(string.Format(System.Threading.Thread.CurrentThread.CurrentCulture, SRGrid.GetString("TypeMismatchException"), this.GroupByComparer.GetType().Name, typeOfIComp.Name));
				}
			}
		}

		#endregion // ValidateGroupByComparer

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
			get
			{
				return true;
			}
		}
		#endregion // UniqueColumnContent

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