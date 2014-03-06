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
using System.Windows.Data;
using System.Collections.Generic;
using System.Reflection;


namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="TemplateColumn"/>
    /// </summary>
    public class TemplateColumnContentProvider : ColumnContentProviderBase
    {
        #region Members
        FrameworkElement _content;
        FrameworkElement _editor;
        Infragistics.Controls.Grids.Cell.CellValueObject _cellObj;
        int _templateFlag;
        bool _isFilterRow;
        bool _justReleased = false;
        Binding _cellBinding;
        #endregion // Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of the <see cref="TemplateColumnContentProvider"/>.
        /// </summary>
        public TemplateColumnContentProvider()
        {
            this._cellObj = new Cell.CellValueObject();
        }

        #endregion // Constructor

        #region Methods

        #region ResolveDisplayElement

        /// <summary>
        /// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
        /// </summary>
        /// <param propertyName="cell">The cell that the display element will be displayed in.</param>
        /// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
        /// <returns>The element that should be displayed.</returns>
        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            TemplateColumn column = (TemplateColumn)cell.Column;
            if (cell.Row.RowType == RowType.FilterRow && column.FilterItemTemplate != null && this._content == null)
            {
                this._content = column.FilterItemTemplate.LoadContent() as FrameworkElement;
            }
            else if (cell.Row.RowType == RowType.AddNewRow && column.AddNewRowItemTemplate != null && this._content == null)
            {
                this._content = column.AddNewRowItemTemplate.LoadContent() as FrameworkElement;
            }
            else if (column.ItemTemplate != null && this._content == null)
            {
                this._content = column.ItemTemplate.LoadContent() as FrameworkElement;
            }
            else if (column.ItemTemplate == null)
            {
                this._content = new ContentPresenter();
            }

            this._cellBinding = cellBinding;

            if (!this.IsToolTip)
            {
                // Lets make sure that this is set, so that when raise the CellControlAttached event, the user
                // has access to the control. 
                if (cell.Control != null && cell.Control.Content != this._content)
                    cell.Control.Content = this._content;


                if (cell.Row.RowType != RowType.AddNewRow)
                {
                    if (cell.Row.RowType == RowType.FilterRow)
                    {
                        this._isFilterRow = true;
                    }
                    else if (cellBinding == null && cell.GetType() == typeof(Cell))
                    {
                        // If we're a Cell, (Not an AddNewRowCell, FilterRowCell, etc...), and we don't have a binding, 
                        // lets raise the event to notify people that the Cell is being attached to the control. 
                        cell.RaiseCellControlAttachedEvent();
                    }
                }
            }

            this.ApplyBindingToDisplayElement(cell, cellBinding);

            return this._content;
        }

        #endregion // ResolveDisplayElement

        #region ApplyBindingToDisplayElement

        /// <summary>
        /// This is where a ColumnContentProvider should apply the Binding to their Display element.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="cellBinding"></param>
        private void ApplyBindingToDisplayElement(Cell cell, Binding cellBinding)
        {
            if (cellBinding != null && !this.IsToolTip && cell.Row.RowType != RowType.AddNewRow && cell.Row.RowType != RowType.FilterRow)
            {
                if (((TemplateColumn)cell.Column).ItemTemplate == null && this._content is ContentPresenter)
                {
                    this._content.SetBinding(ContentPresenter.ContentProperty, cellBinding);
                }
                else
                {
                    this._cellObj.SetBinding(Infragistics.Controls.Grids.Cell.CellValueObject.ValueProperty, cellBinding);
                }
            }
        }

        #endregion // ApplyBindingToDisplayElement

        #region AdjustDisplayElement

        /// <summary>
        /// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
        /// </summary>
        /// <param name="cell"></param>
        public override void AdjustDisplayElement(Cell cell)
        {
            if (this._isFilterRow)
            {
                if (((TemplateColumn)cell.Column).FilterItemTemplate != null)
                {
                    FilterRowCell frc = (FilterRowCell)cell;
                    FilteringDataContext fdc = new FilteringDataContext(frc);
                    this._content.DataContext = fdc;
                }
            }
            


            else if (cell.Control != null)
            {
                if (this._justReleased && this._cellObj.DataContext == cell.Control.DataContext)
                    this._cellObj.DataContext = null;
                this._cellObj.DataContext = cell.Control.DataContext;
            }

            // So when an element is release, we reset this flag, this tells us to check to see if we need 
            // to fire the CellControlAttached Event, as the event won't be raised if the element isn't bound. 
            if (this._justReleased && this._cellBinding == null && cell.GetType() == typeof(Cell))
            {
                // If we're a Cell, (Not an AddNewRowCell, FilterRowCell, etc...), and we don't have a binding, 
                // lets raise the event to notify people that the Cell is being attached to the control. 
                cell.RaiseCellControlAttachedEvent();
            }

            // Since we no longer have been "just released", we should set the flag. 
            this._justReleased = false;

            base.AdjustDisplayElement(cell);
        }

        #endregion // AdjustDisplayElement

        #region ResolveEditorControl
        /// <summary>
        /// Sets up the edtior control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
        /// </summary>
        /// <param propertyName="cell">The <see cref="Cell"/> entering edit mode.</param>
        /// <param propertyName="editorValue">The value that should be put in the editor.</param>
        /// <param propertyName="availableWidth">The amount of horizontal space available.</param>
        /// <param propertyName="availableHeight">The amound of vertical space available.</param>
        /// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
        /// <returns></returns>
        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            TemplateColumn column = (TemplateColumn)cell.Column;

            bool isFilterRow = cell.Row.RowType == RowType.FilterRow;
            bool isAddNewRow = cell.Row.RowType == RowType.AddNewRow;
            bool isImplicit = false;

            if (column.EditorTemplateDirtyFlag != this._templateFlag || this._editor == null)
            {
                if (isFilterRow && column.FilterEditorTemplate != null && this._editor == null)
                {
                    this._editor = column.FilterEditorTemplate.LoadContent() as FrameworkElement;
                }
                else if (isAddNewRow && column.AddNewRowEditorTemplate != null && this._editor == null)
                {
                    this._editor = column.AddNewRowEditorTemplate.LoadContent() as FrameworkElement;
                }
                else if (this._editor == null && column.EditorTemplate != null)
                {
                    this._editor = column.EditorTemplate.LoadContent() as FrameworkElement;
                }
                else if (this._editor == null && column.EditorTemplate == null)
                {
                    this._editor = new TemplateColumnEditorContentPresenter();
                    isImplicit = true;
                }
                else
                {
                    this._editor = null;
                }

                this._templateFlag = column.EditorTemplateDirtyFlag;
            }

            if (this._editor != null)
            {
                if (isFilterRow && column.FilterEditorTemplate != null)
                {
                    FilterRowCell frc = (FilterRowCell)cell;
                    FilteringDataContext fdc = new FilteringDataContext(frc);
                    this._editor.DataContext = fdc;
                }
                else if (isImplicit && editorBinding != null)
                {
                    this._editor.SetBinding(ContentPresenter.ContentProperty, editorBinding);
                }
                else
                {
                    cell.Control.DataContext = cell.Row.Data;
                    ColumnContentProviderBase.SetControlStyle(this._editor, cell.EditorStyleResolved);
                }
            }

            return this._editor;
        }
        #endregion // ResolveEditorControl

        #region ResolveValueFromEditor

        /// <summary>
        /// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
        /// </summary>
        /// <param propertyName="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
        /// <returns>The value that should be displayed in the cell.</returns>
        public override object ResolveValueFromEditor(Cell cell)
        {
            return null;
        }
        #endregion // ResolveValueFromEditor

        #region EditorRemoved

        /// <summary>
        /// Raised when the editor is removed from the cell.
        /// </summary>
        public override void EditorRemoved()
        {
            if (this._editor != null)
                this._editor.DataContext = null;
            this._editor = null;
            this._content = null;
        }

        #endregion // EditorRemoved

        #region ResetContent
        /// <summary>
        /// Raised when the cell is recycling to allow the provider a chance to clear any internal members.
        /// </summary>
        public override void ResetContent()
        {
            this._justReleased = true;
            if (this._cellObj != null)
                this._cellObj.DataContext = null;
            base.ResetContent();
        }
        #endregion // ResetContent

        #region FocusEditor

        /// <summary>
        /// Calls the ContentProvider to Attempt to Focus the underlying editor control
        /// </summary>
        protected internal override void FocusEditor()
        {
            var control = this._editor as Control;
            if (control != null)
                control.Focus();
        }

        #endregion

        #endregion // Methods

        #region Properites

        #region RemovePaddingDuringEditing

        /// <summary>
        /// Gets/Sets whether the padding of a <see cref="Cell"/> should be removed before putting an editor into edit mode. 
        /// </summary>
        /// <remarks>
        /// This property will determine the availableHeight and availableWidth parameters of the ResolveEditorControl method.
        /// </remarks>
        public override bool RemovePaddingDuringEditing
        {
            get
            {
                return true;
            }
        }

        #endregion // RemovePaddingDuringEditing

        #region CanResolveValueFromEditor

        /// <summary>
        /// Gets a value indicating whether the <see cref="TemplateColumnContentProvider"/> can resolve value from editor using the <see cref="ResolveValueFromEditor"/> method.
        /// </summary>
        protected internal override bool CanResolveValueFromEditor
	    {
	        get
	        {
	            return false;
	        }
        }

        #endregion // CanResolveValueFromEditor

        #endregion // Properties
    }

    /// <summary>
    /// For Internal use only. This content presenter is used for handling of implicit editor data templates.
    /// </summary>
    public sealed class TemplateColumnEditorContentPresenter : ContentPresenter
    {
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>. In simplest terms, this means the method is called just before a UI element displays in an application. For more information, see Remarks.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            CellControl control = this.Control;

            if (control != null)
            {
                control.RefreshEditorBindings();
            }
        }

        /// <summary>
        /// Gets or sets the control that owns the <see cref="TemplateColumnEditorContentPresenter"/>.
        /// </summary>
        /// <value>
        /// The control.
        /// </value>
        internal CellControl Control { get; set; }
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