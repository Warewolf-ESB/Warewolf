using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Controls.Editors;
using System.Windows.Data;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="DateTimeColumn"/>
    /// </summary>
    public class DateTimeColumnContentProvider : ColumnContentProviderBase
    {
    	#region Members
		
        private XamDateTimeInput _dp, _dpEditor;
        private TextBlock _tb;

		#endregion // Members

		#region Constructor

		/// <summary>
        /// Instantiates a new instance of the <see cref="DateTimeColumnContentProvider"/>.
		/// </summary>
        public DateTimeColumnContentProvider()
		{
            this._dp = new XamDateTimeInput();
            this._dpEditor = new XamDateTimeInput();
            this._tb = new TextBlock();            
		}

		#endregion // Constructor

        #region Overrides

        #region ResolveDisplayElement

        /// <summary>
        /// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
        /// </summary>
        /// <param name="cell">The cell that the display element will be displayed in.</param>
        /// <param name="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
        /// <returns>The element that should be displayed.</returns>
        public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
        {
            var col = (DateTimeColumn)cell.Column;

            this.ApplyBindingToDisplayElement(cell, cellBinding);

            if (cell.EnableCustomEditorBehaviors && col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode)
            {
                return _tb;
            }

            ColumnContentProviderBase.SetControlStyle(_dp, cell.EditorStyleResolved);

            bool isEditable = false;

            if (cell.EnableCustomEditorBehaviors && cell.EnableCustomEditorBehaviors)
            {
                isEditable = col.EditorDisplayBehavior == EditorDisplayBehaviors.Always;
            }

            this._dp.IsHitTestVisible = isEditable;

            this._dp.Focusable = isEditable;


            this._dp.IsEnabled = IsEnabledResolved(cell, col);

            return this._dp;
        }

        #endregion // ResolveDisplayElement

        #region ResolveEditorControl

        /// <summary>
        /// Sets up the edtior control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
        /// </summary>
        /// <param name="cell">The <see cref="Cell"/> entering edit mode.</param>
        /// <param name="editorValue">The value that should be put in the editor.</param>
        /// <param name="availableWidth">The amount of horizontal space available.</param>
        /// <param name="availableHeight">The amound of vertical space available.</param>
        /// <param name="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
        /// <returns></returns>
        protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            DateTimeColumn column = ((DateTimeColumn)cell.Column);

            editorBinding.ValidatesOnDataErrors = editorBinding.ValidatesOnExceptions = editorBinding.NotifyOnValidationError = column.AllowEditingValidation;





            this._dpEditor.SetBinding(ValueInput.ValueProperty, editorBinding);

            ColumnContentProviderBase.SetControlStyle(_dpEditor, cell.EditorStyleResolved);

            if (cell.IsEditing)
            {
                this._dpEditor.Focus();
            }

            this._dpEditor.Mask = column.SelectedDateMask;
            this._dpEditor.IsEnabled = this.IsEnabledResolved(cell, column);

            return this._dpEditor;
        }

        #endregion // ResolveEditorControl

        #region ResolveValueFromEditor

        /// <summary>
        /// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
        /// </summary>
        /// <param name="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
        /// <returns>The value that should be displayed in the cell.</returns>
        public override object ResolveValueFromEditor(Cell cell)
        {
            return this._dpEditor.Value;
        }

        #endregion // ResolveValueFromEditor

        #region AdjustDisplayElement

        /// <summary>
        /// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
        /// </summary>
        /// <param name="cell"></param>
        public override void AdjustDisplayElement(Cell cell)
        {
            DateTimeColumn column = (DateTimeColumn)cell.Column;
            bool dataIsNull = (cell.Row.Data == null);

            if (!dataIsNull)
            {
                BindingExpression bindingExpression = this._dp.GetBindingExpression(ValueInput.ValueProperty);

                if (bindingExpression == null)
                {
                    Binding binding = this.ResolveBinding(cell);

                    if (binding != null)
                    {
                        binding.ValidatesOnDataErrors = binding.ValidatesOnExceptions = binding.NotifyOnValidationError = false;



                        this._dp.SetBinding(ValueInput.ValueProperty, binding);
                    }
                }
            }

            this._dp.Mask = column.SelectedDateMask;
            this._dp.IsEnabled = this.IsEnabledResolved(cell, column);

            ColumnContentProviderBase.SetControlStyle(this._dpEditor, cell.EditorStyleResolved);

            base.AdjustDisplayElement(cell);
        }

        #endregion // AdjustDisplayElement

        #endregion // Overrides

		#region Methods

        #region ApplyBindingToDisplayElement

        /// <summary>
        /// This is where a ColumnContentProvider should apply the Binding to their Display element.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="binding"></param>
        private void ApplyBindingToDisplayElement(Cell cell, Binding binding)
        {
            if (binding != null)
            {
                binding.ValidatesOnDataErrors = binding.ValidatesOnExceptions = binding.NotifyOnValidationError = false;





                var col = (DateTimeColumn)cell.Column;

                if (cell.EnableCustomEditorBehaviors && col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode)
                {
                    var wrappingConverter = new DisplayTextConverter(_dp, binding.Converter);
                    binding.Converter = wrappingConverter;

                    this._tb.SetBinding(TextBlock.TextProperty, binding);
                }
                else
                {
                    this._dp.SetBinding(ValueInput.ValueProperty, binding);
                }
            }
        }
  
        #endregion // ApplyBindingToDisplayElement

        #region FocusEditor

        /// <summary>
        /// Calls the ContentProvider to Attempt to Focus the underlying editor control
        /// </summary>
        protected internal override void FocusEditor()
        {
            if (this._dpEditor != null)
            {
                this._dpEditor.Focus();
            }
        }

        #endregion // FocusEditor

        #region IsEnabledResolved

        private bool IsEnabledResolved(Cell cell, DateColumnBase col)
        {
            if (col.IsReadOnly)
            {
                return false;
            }

            if (cell.EnableCustomEditorBehaviors)
            {
                return (cell.IsEditable | col.EditorDisplayBehavior == EditorDisplayBehaviors.Always);
            }
            
            return cell.IsEditable;
        }

        #endregion // IsEnabledResolved

        #endregion // Methods

        #region Classes

        #region DisplayTextConverter

        private class DisplayTextConverter : IValueConverter
        {
            #region Members

            private readonly XamMaskedInput _editor;
            private readonly IValueConverter _wrappedConverter;

            #endregion // Members

            #region Constructor

            public DisplayTextConverter(XamMaskedInput editor, IValueConverter wrappedConverter)
            {
                this._editor = editor;
                this._wrappedConverter = wrappedConverter;
            }

            #endregion // Constructor

            #region IValueConverter

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object convertedValue;

                if (this._wrappedConverter != null)
                {
                    convertedValue = this._wrappedConverter.Convert(value, targetType, parameter, culture);
                }
                else
                {
                    convertedValue = value;
                }

                Exception ex;
                string displayText;

                this._editor.ConvertValueToDisplayText(convertedValue, out displayText, out ex);

                return displayText;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DependencyProperty.UnsetValue;
            }

            #endregion // IValueConverter
        }

        #endregion // DisplayTextConverter

        #endregion // Classes
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