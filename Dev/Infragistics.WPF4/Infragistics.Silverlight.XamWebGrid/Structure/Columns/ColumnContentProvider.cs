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
using System.Globalization;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A base class used to provide content to a <see cref="Cell"/> object for a particular <see cref="Column"/>.
	/// </summary>
	public abstract class ColumnContentProviderBase
	{
		#region Members

		Action<object> _valueChangedFunction;

		#endregion // Members

		#region Methods

		#region Public

		#region ResolveDisplayElement

		/// <summary>
		/// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
		/// </summary>
		/// <param propertyName="cell">The cell that the display element will be displayed in.</param>
		/// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
		/// <returns>The element that should be displayed.</returns>
		public abstract FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding);

		#endregion // ResolveDisplayElement

		#region AdjustDisplayElement

		/// <summary>
		/// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
		/// </summary>
		/// <param name="cell"></param>
		public virtual void AdjustDisplayElement(Cell cell)
		{
		}

		#endregion // AdjustDisplayElement

		#region ResolveValueFromEditor

		/// <summary>
		/// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
		/// </summary>
		/// <param propertyName="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
		/// <returns>The value that should be displayed in the cell.</returns>
		public abstract object ResolveValueFromEditor(Cell cell);
		
		#endregion // ResolveValueFromEditor

		#region GenerateGroupByCellContent

		/// <summary>
		/// Used when rendering a GroupByRow, allows for the column content provider to override default behavior and render out a representation of the data.
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="cellBinding"></param>
		/// <returns></returns>
		public virtual FrameworkElement GenerateGroupByCellContent(GroupByCell cell, Binding cellBinding)
		{
			return null;
		}

		#endregion // GenerateGroupByCellContent

		#region EditorLoaded
		/// <summary>
		/// Invoked when the editor's loaded event is raised. 
		/// </summary>
		/// <remarks>This method should be used to do such things as selecting the text of a TextBox, or causing a specific element in the editor to focus.</remarks>
		public virtual void EditorLoaded()
		{

		}

		#endregion // EditorLoaded

		#region ApplyFormatting

		/// <summary>
		/// Allows the <see cref="ColumnContentProviderBase"/> to update the value being set for the display element.
		/// </summary>
		/// <param propertyName="value">The original data value from the underlying data.</param>
		/// <param propertyName="column">The <see cref="Column"/> whose properties should be used to determine the formatting.</param>
		/// <param propertyName="culture"></param>
		/// <returns>The value that should be displayed in the <see cref="Cell"/></returns>
		public virtual object ApplyFormatting(object value, Column column, System.Globalization.CultureInfo culture)
		{
			return value;
		}
		#endregion // ApplyFormatting

		#region ResolveEditor

		/// <summary>
		/// Sets up the edtior control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
		/// </summary>
		/// <param propertyName="cell">The <see cref="Cell"/> entering edit mode.</param>
		/// <param propertyName="valueChangedFunction">The function that should be called to notify when a value in the editor has changed.</param>
		/// <param propertyName="editorValue">The value that should be put in the editor.</param>
		/// <param propertyName="availableWidth">The amount of horizontal space available.</param>
		/// <param propertyName="availableHeight">The amound of vertical space available.</param>
		/// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
		/// <returns></returns>	
		public FrameworkElement ResolveEditor(Cell cell, Action<object> valueChangedFunction, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
		{
			this._valueChangedFunction = valueChangedFunction;

			return this.ResolveEditorControlInternal(cell, editorValue, availableWidth, availableHeight, editorBinding);
		}

		#endregion // ResolveEditor

		#region EditorRemoved

		/// <summary>
		/// Raised when the editor is removed from the cell.
		/// </summary>
		public virtual void EditorRemoved()
		{
		}

		#endregion // EditorRemoved

		#region ResetContent
		/// <summary>
		/// Raised when the cell is recycling to allow the provider a chance to clear any internal members.
		/// </summary>
		public virtual void ResetContent()
		{
		}
		#endregion // ResetContent

		#endregion // Public

		#region Protected

		#region NotifyEditorValueChanged

		/// <summary>
		/// Used to notify the owning cell when an editor's value has changed.
		/// </summary>
		/// <param propertyName="value"></param>
		protected void NotifyEditorValueChanged(object value)
		{
			if (this._valueChangedFunction != null)
				this._valueChangedFunction(value);
		}

		#endregion // NotifyEditorValueChanged

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
		protected abstract FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding);

		#endregion // ResolveEditorControl

        #region ResolveBinding
        /// <summary>
        /// Builds the binding that will be used for a <see cref="Cell"/>
        /// </summary>
        /// <returns>If a binding cannot be created, null will be returned.</returns>
        protected internal virtual Binding ResolveBinding(Cell cell)
        {
            Binding binding = null;
            if (cell != null)
            {
                if (cell.Control != null)
                {
                    binding = cell.Control.ResolveBinding();
                }
                if (binding == null)
                {
                    binding = ColumnContentProviderBase.ResolveBindingInternal(cell);
                }
            }
            return binding;
        }
        #endregion // ResolveBinding

		#region ResolveEditorBinding

		/// <summary>
		/// Creates a <see cref="Binding"/> that can be applied to an editor.
		/// </summary>
		/// <returns></returns>
		protected internal virtual Binding ResolveEditorBinding(Cell cell)
		{
			Binding binding = null;

			if (cell.Control != null)
			{
				binding = cell.Control.ResolveEditorBinding();
			}
			if (binding == null)
			{
				object data = cell.Row.Data;
				if (cell.Column.Key != null && data != null)
				{
					binding = new Binding(cell.Column.Key);
					binding.Source = data;

					binding.ConverterCulture = CultureInfo.CurrentCulture;

					binding.Mode = BindingMode.TwoWay;
					binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

					binding.ConverterParameter = cell;
                    binding.Converter = new Infragistics.Controls.Grids.Cell.CellEditingBindingConverter();

					EditableColumn col = cell.Column as EditableColumn;
                    if (col != null)
                    {
                        binding.ValidatesOnDataErrors = binding.ValidatesOnExceptions = binding.NotifyOnValidationError = col.AllowEditingValidation;



                    }
				}
			}
			return binding;
		}
		#endregion // ResolveEditorBinding

        #region ResolveDisplayElementInternal
        /// <summary>
		/// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
		/// </summary>
		/// <param propertyName="cell">The cell that the display element will be displayed in.</param>
		/// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
		/// <returns>The element that should be displayed.</returns>
        protected internal virtual FrameworkElement ResolveDisplayElementInternal(Cell cell, Binding cellBinding)
        {
            if (cell.Row.RowType == RowType.AddNewRow)
            {
                FrameworkElement fe = this.ApplyAddNewRowItemTemplate(cell);
                if (fe != null)
                    return fe;
            }

            return this.ResolveDisplayElement(cell, cellBinding);
        }

        #endregion // ResolveDisplayElementInternal

        #region ResolveDisplayElementInternal
        /// <summary>
        /// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
        /// </summary>
        /// <param propertyName="cell">The cell that the display element will be displayed in.</param>
        /// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
        /// <returns>The element that should be displayed.</returns>
        protected internal virtual FrameworkElement ResolveDisplayElementInternal(Cell cell, Binding cellBinding, RowType rowType)
        {
            if (rowType == RowType.AddNewRow)
            {
                FrameworkElement fe = this.ApplyAddNewRowItemTemplate(cell);
                if (fe != null)
                    return fe;
            }

            return this.ResolveDisplayElement(cell, cellBinding);
        }

        #endregion // ResolveDisplayElementInternal

        #region ApplyAddNewRowItemTemplate

        /// <summary>
        /// If there is an AddNewItemTemplate then it will be loaded for an AddNewRow cell.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        protected virtual FrameworkElement ApplyAddNewRowItemTemplate(Cell cell)
        {
            FrameworkElement fe = null;

            Column column = cell.Column as Column;

            if (column != null && column.AddNewRowItemTemplate!=null)
            {
                 fe = column.AddNewRowItemTemplate.LoadContent() as FrameworkElement;
            }

            return fe;
        }

        #endregion // ApplyAddNewRowItemTemplate

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
        protected internal virtual FrameworkElement ResolveEditorControlInternal(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            if (cell.Row.RowType == RowType.AddNewRow)
            {
                FrameworkElement fe = this.ApplyAddNewRowEditorTemplate(cell, editorValue, availableWidth, availableHeight, editorBinding);
                if (fe != null)
                {
                    if (cell.Control != null)
                    {
                        cell.Control.VerticalContentAlignment = VerticalAlignment.Stretch;
                        cell.Control.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    }
                    return fe;
                }
            }

            return this.ResolveEditorControl(cell, editorValue, availableWidth, availableHeight, editorBinding);
        }

        #endregion // ResolveEditorControlInternal

        #region ApplyAddNewRowEditorTemplate

        /// <summary>
        /// If there is an <see cref="Infragistics.Controls.Grids.Column.AddNewRowEditorTemplate"/> then it will be loaded for an <see cref="AddNewRowCell"/>.
        /// </summary>
        /// <param name="cell"></param>        
        /// <param name="editorValue">The value that should be put in the editor.</param>
        /// <param name="availableWidth">The amount of horizontal space available.</param>
        /// <param name="availableHeight">The amound of vertical space available.</param>
        /// <param name="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>        
        /// <returns></returns>
        protected virtual FrameworkElement ApplyAddNewRowEditorTemplate(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            FrameworkElement fe = null;

            Column editableColumn = cell.Column as Column;

            if (editableColumn != null && editableColumn.AddNewRowEditorTemplate != null)
            {
                fe = editableColumn.AddNewRowEditorTemplate.LoadContent() as FrameworkElement;
            }

            return fe;
        }

        #endregion // ApplyAddNewRowEditorTemplate    

        #region FocusEditor

        /// <summary>
        /// Calls the ContentProvider to Attempt to Focus the underlying editor control
        /// </summary>
        protected internal virtual void FocusEditor()
        {

        }

        #endregion // ApplyBindingToDisplayElement

        #endregion // Protected

        #region Static

        #region ResolveBindingInternal
        /// <summary>
        /// Builds the binding that will be used for a <see cref="Cell"/>
        /// </summary>
        /// <returns>If a binding cannot be created, null will be returned.</returns>
        internal static Binding ResolveBindingInternal(Cell cell)
        {
            Binding binding = null;
            if (cell.Column.Key != null && cell.Column.DataType != null)
            {
                if (cell.Column is UnboundColumn)
                    binding = new Binding();
                else
                    binding = new Binding(cell.Column.Key);

                binding.ConverterCulture = CultureInfo.CurrentCulture;
                binding.Mode = cell.BindingMode;

                

                binding.ConverterParameter = cell.Control;
                binding.Converter = cell.CreateCellBindingConverter();
            }
            return binding;
        }
        #endregion // ResolveBindingInternal

        #region SetControlStyle
        /// <summary>
        /// Sets the inputted style to the inputted control, and if the style is null then it clears the style off the control.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="style"></param>
        internal static void SetControlStyle(FrameworkElement control, Style style)
        {
            if (control != null)
            {
                if (style == null)
                {

                    if (control.Style != null)

                    control.ClearValue(Control.StyleProperty);
                }
                else
                {
                    control.SetValue(Control.StyleProperty, style);
                }
            }
        }
        #endregion SetControlStyle

        #endregion // Static

        #endregion // Methods

        #region Properties

        #region CanResolveValueFromEditor

        /// <summary>
	    /// Gets a value indicating whether the <see cref="ColumnChooserCommandBase"/> can resolve value from editor using the <see cref="ResolveValueFromEditor"/> method.
	    /// </summary>
	    protected internal virtual bool CanResolveValueFromEditor
	    {
	        get { return true; }
	    }

        #endregion // CanResolveValueFromEditor

        #region RemovePaddingDuringEditing

        /// <summary>
		/// Gets/Sets whether the padding of a <see cref="Cell"/> should be removed before putting an editor into edit mode. 
		/// </summary>
		/// <remarks>
		/// This property will determine the availableHeight and availableWidth parameters of the ResolveEditorControl method.
		/// </remarks>
		public virtual bool RemovePaddingDuringEditing
		{
			get { return false; }
		}

		#endregion // RemovePaddingDuringEditing

        #region IsToolTip

        /// <summary>
        /// Gets/Sets whether this <see cref="ColumnContentProviderBase"/> is being used to populate the content of a ToolTip.
        /// </summary>
        public bool IsToolTip
        {
            get;
            set;
        }

        #endregion // IsToolTip

        #endregion // Properties
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