using System.Windows;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A custom <see cref="TextColumnContentProvider"/> which is used by the <see cref="ProxyColumn"/> on the <see cref="CompoundFilterDialogControl"/>.
    /// </summary>
    /// <remarks>Not for general use.</remarks>
    public class ProxyColumnContentProvider : TextColumnContentProvider
    {
        #region Members        
        ColumnContentProviderBase _proxiedColumnContentProvider;
        
        #endregion // Members

        public override bool RemovePaddingDuringEditing
        {
            get {

                if (this._proxiedColumnContentProvider != null)
                {
                    return this._proxiedColumnContentProvider.RemovePaddingDuringEditing;
                }
                return base.RemovePaddingDuringEditing;
            }
        }

        #region Overrides
        #endregion // Overrides

        #region ResolveDisplayElementInternal
        /// <summary>
        /// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
        /// </summary>
        /// <param propertyName="cell">The cell that the display element will be displayed in.</param>
        /// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
        /// <returns>The element that should be displayed.</returns>
        protected internal override FrameworkElement ResolveDisplayElementInternal(Cell cell, System.Windows.Data.Binding cellBinding)
        {
            ProxyColumn pc = (ProxyColumn)cell.Column;
            if (pc.ProxiedSource != null)
            {
                EditableColumn ec = pc.ProxiedSource.Column as EditableColumn;

                if (ec != null)
                {
                    pc.EditorHorizontalContentAlignment = ec.EditorHorizontalContentAlignment;
                    pc.EditorVerticalContentAlignment = ec.EditorVerticalContentAlignment;
                    pc.HorizontalContentAlignment = ec.HorizontalContentAlignment;
                    pc.VerticalContentAlignment = ec.VerticalContentAlignment;
                }

                if (this._proxiedColumnContentProvider == null)
                    this._proxiedColumnContentProvider = pc.ProxiedSource.Column.GenerateContentProvider();

                FilterRow fr = new FilterRow((RowsManager)cell.Row.Manager);
                fr.SetData(cell.Row.Data);
                cell = new ProxyFilterRowCell(fr, pc.ProxiedSource.Column);                
                return this._proxiedColumnContentProvider.ResolveDisplayElementInternal(cell, cellBinding);
            }
            return base.ResolveDisplayElementInternal(cell, cellBinding);
        }
        #endregion // ResolveDisplayElementInternal

        #region ResolveEditorControlInternal
        /// <summary>
        /// Sets up the edtior control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
        /// </summary>
        /// <param propertyName="cell">The <see cref="Cell"/> entering edit mode.</param>
        /// <param propertyName="editorValue">The value that should be put in the editor.</param>
        /// <param propertyName="availableWidth">The amount of horizontal space available.</param>
        /// <param propertyName="availableHeight">The amound of vertical space available.</param>
        /// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
        /// <returns></returns>
        protected internal override FrameworkElement ResolveEditorControlInternal(Cell cell, object editorValue, double availableWidth, double availableHeight, System.Windows.Data.Binding editorBinding)
        {
            ProxyColumn pc = (ProxyColumn)cell.Column;

            EditableColumn ec = pc.ProxiedSource.Column as EditableColumn;
            if (ec != null)
            {
                pc.EditorHorizontalContentAlignment = ec.EditorHorizontalContentAlignment;
                pc.EditorVerticalContentAlignment = ec.EditorVerticalContentAlignment;
                pc.HorizontalContentAlignment = ec.HorizontalContentAlignment;
                pc.VerticalContentAlignment = ec.VerticalContentAlignment;
               
            }
            if (this._proxiedColumnContentProvider == null)
                this._proxiedColumnContentProvider = pc.ProxiedSource.Column.GenerateContentProvider();

            editorBinding = this._proxiedColumnContentProvider.ResolveEditorBinding(cell);

            FilterRow fr = new FilterRow((RowsManager)cell.Row.Manager);

            fr.SetData(cell.Row.Data);

            cell = new ProxyFilterRowCell(fr, pc.ProxiedSource.Column);

            return this._proxiedColumnContentProvider.ResolveEditorControlInternal(cell, editorValue, availableWidth, availableHeight, editorBinding);
        }
        #endregion // ResolveEditorControlInternal

        #region ResolveValueFromEditor
        /// <summary>
        /// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
        /// </summary>
        /// <param propertyName="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
        /// <returns>The value that should be displayed in the cell.</returns>
        public override object ResolveValueFromEditor(Cell cell)
        {
            ProxyColumn pc = (ProxyColumn)cell.Column;

            if (this._proxiedColumnContentProvider == null)
                this._proxiedColumnContentProvider = pc.ProxiedSource.Column.GenerateContentProvider();

            object value = this._proxiedColumnContentProvider.ResolveValueFromEditor(cell);

            return value;
        }
        #endregion // ResolveValueFromEditor
    } 
}

namespace Infragistics.Controls.Grids.Primitives
{
    internal class ProxyFilterRowCell : FilterRowCell
    {
        #region Members
        object _filterCellValue;
        #endregion // Members

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFilterRowCell"/> class.
        /// </summary>
        /// <param propertyName="row">The <see cref="FilterRow"/> object that owns the <see cref="FilterRowCell"/></param>
        /// <param propertyName="column">The <see cref="Column"/> object that the <see cref="FilterRowCell"/> represents.</param>
        protected internal ProxyFilterRowCell(RowBase row, Column column)
            : base(row, column)
        {
        }

        #endregion // Constructor

        #region Value
        public override object FilterCellValue
        {
            get
            {
                if (this.Row != null && this.Row.Data != null)
                {
                    XamGridConditionInfo data = (XamGridConditionInfo)this.Row.Data;
                    return data.FilterValue;
                }
                return this._filterCellValue;
            }
            set
            {
                if (this.Row != null && this.Row.Data != null)
                {
                    XamGridConditionInfo data = (XamGridConditionInfo)this.Row.Data;
                    data.FilterValue = value; ;
                }
                _filterCellValue = value;
            }
        }
        #endregion // Value
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