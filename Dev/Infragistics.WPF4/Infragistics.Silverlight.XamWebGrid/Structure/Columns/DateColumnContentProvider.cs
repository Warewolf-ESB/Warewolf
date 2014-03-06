using System.Windows.Controls;
using Infragistics.Controls.Grids.Primitives;
using System.Windows;
using System.Windows.Data;
using System;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Globalization;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="DateColumn"/>
	/// </summary>
	public class DateColumnContentProvider : ColumnContentProviderBase
	{
		#region Members
		DatePicker _dp, _dpEditor;
        TextBlock _tb;
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Instantiates a new instance of the <see cref="CheckBoxColumnContentProvider"/>.
		/// </summary>
        public DateColumnContentProvider()
        {
            this._dp = new DatePicker();
            this._dpEditor = new DatePicker();
            this._tb = new TextBlock();
            
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
            if (this._dp != null && cell.Control != null)
            {
                this._dp.SetBinding(DatePicker.FlowDirectionProperty, new Binding("FlowDirection") { Source = cell.Control });
            }

            var col = (DateColumn)cell.Column;

            if (cell.EnableCustomEditorBehaviors && (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode))
            {
                switch (col.SelectedDateFormat)
                {
                    case DatePickerFormat.Long:
                        cellBinding.StringFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
                        break;
                    case DatePickerFormat.Short:
                        cellBinding.StringFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                        break;
                }

                this.ApplyBindingToDisplayElement(cell, cellBinding);

                return _tb;
            }

            ColumnContentProviderBase.SetControlStyle(this._dp, cell.EditorStyleResolved);

            bool isEditable = false;

            if (cell.EnableCustomEditorBehaviors)
            {
                isEditable = col.EditorDisplayBehavior == EditorDisplayBehaviors.Always;
            }

            this._dp.IsHitTestVisible = isEditable;

            this._dp.Focusable = isEditable;


            this._dp.SelectedDateFormat = col.SelectedDateFormat;

            this._dp.IsEnabled = IsEnabledResolved(cell, col);

            this.ApplyBindingToDisplayElement(cell, cellBinding);

            return this._dp;
		}

		#endregion // ResolveDisplayElement

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





                var col = (DateColumn)cell.Column;

                if (cell.EnableCustomEditorBehaviors && (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode))
                    this._tb.SetBinding(TextBlock.TextProperty, binding);
                else
                {                    
                    this._dp.SetBinding(DatePicker.SelectedDateProperty, binding);
                }
            }
        }

        #endregion // ApplyBindingToDisplayElement

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
            if (this._dpEditor != null && cell.Control != null)
            {
                this._dpEditor.SetBinding(DatePicker.FlowDirectionProperty, new Binding("FlowDirection") { Source=cell.Control });
            }
            editorBinding.ValidatesOnDataErrors = editorBinding.ValidatesOnExceptions = editorBinding.NotifyOnValidationError = ((DateColumn)cell.Column).AllowEditingValidation;





            var col = (DateColumn)cell.Column;

            this._dpEditor.SetBinding(DatePicker.SelectedDateProperty, editorBinding);

            this._dpEditor.SelectedDateChanged -= DatePicker_SelectedDateChanged;
			this._dpEditor.SelectedDateChanged += DatePicker_SelectedDateChanged;

            ColumnContentProviderBase.SetControlStyle(this._dpEditor, cell.EditorStyleResolved);

			if (cell.IsEditing)
				this._dpEditor.Focus();

            this._dpEditor.SelectedDateFormat = ((DateColumn)cell.Column).SelectedDateFormat;

            this._dp.IsEnabled = IsEnabledResolved(cell, col);

			return this._dpEditor;
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
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			DatePickerTextBox dptb = PlatformProxy.GetFocusedElement(this._dpEditor) as DatePickerTextBox;
            if (dptb != null)
            {
                DependencyObject parent = dptb;
                while (parent != null)
                {
                    parent = PlatformProxy.GetParent(parent);

                    if (parent == this._dpEditor)
                    {
                        if (string.IsNullOrEmpty(dptb.Text))
                            this._dpEditor.SelectedDate = null;
                        else
                        {

                            AdjustDatePicker_WPF(dptb);




                        }
                        break;
                    }
                }
            }
            else // Walk it ourselves...
            {
                dptb = FindDatePickerTextBox(this._dpEditor);
                if (dptb != null)
                {
                    if (string.IsNullOrEmpty(dptb.Text))
                        this._dpEditor.SelectedDate = null;
                    else
                    {

                        AdjustDatePicker_WPF(dptb);




                    }
                }
            }

			return this._dpEditor.SelectedDate;
		}

		#endregion // ResolveValueFromEditor

        #region AdjustDisplayElement

        /// <summary>
        /// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
        /// </summary>
        /// <param name="cell"></param>
        public override void AdjustDisplayElement(Cell cell)
        {
            var col = (DateColumn)cell.Column;
            bool dataIsNull = (cell.Row.Data == null);

            if (!dataIsNull)
            {
                BindingExpression bindingExpression = this._dp.GetBindingExpression(DatePicker.SelectedDateProperty);
                if (bindingExpression == null)
                {
                    Binding binding = this.ResolveBinding(cell);
                    if (binding != null)
                    {
                        binding.ValidatesOnDataErrors = binding.ValidatesOnExceptions = binding.NotifyOnValidationError = false;



                        this._dp.SetBinding(DatePicker.SelectedDateProperty, binding);
                    }
                }
            }
            
            // So, sometimes, when the date is null, the DatePicker, decides that it's going to hold onto
            // the text it had before.  So, to get around it, we need to add this check. 
            // That control is SOOOOO buggy.
            if (this._dp.SelectedDate == null)
            {
                if (!string.IsNullOrEmpty(this._dp.Text))
                {
                    // The control is SOO buggy, that even calling this._dp.Text = null, or this._dp.SelectedDate, doesn't 
                    // actually change the underlying DatePickerTextBox.  What are they doing???
                    // I don't like this fix, but it  does ensure a refresh, as UpdateLayout, and both Invalidate methods do nothing.
                    this._dp.OnApplyTemplate();
                }
            }

            ColumnContentProviderBase.SetControlStyle(this._dp, cell.EditorStyleResolved);

            this._dp.IsEnabled = IsEnabledResolved(cell, col);            

            base.AdjustDisplayElement(cell);
        }
        #endregion // AdjustDisplayElement

        #region EditorRemoved
        /// <summary>
		/// Raised when the editor is removed from the cell.
		/// </summary>
		public override void EditorRemoved()
		{
			base.EditorRemoved();
            if (this._dpEditor != null)
            {
                this._dpEditor.SelectedDateChanged -= DatePicker_SelectedDateChanged;
                this._dpEditor.ClearValue(DatePicker.SelectedDateProperty);
            }
		}
		#endregion // EditorRemoved

        #region FindDatePickerTextBox

        private DatePickerTextBox FindDatePickerTextBox(DependencyObject obj)
        {
            FrameworkElement objElement = obj as FrameworkElement;
            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                DatePickerTextBox element = child as DatePickerTextBox;
                if (element != null)
                {
                    return element;
                }

                element = this.FindDatePickerTextBox(child);

                if (element != null)
                {
                    return element;
                }
            }

            return null;
        }
        #endregion // FindDatePickerTextBox        

        #region IsEnabledResolved

        private bool IsEnabledResolved(Cell cell, DateColumn col)
        {
            if (col.IsReadOnly)
                return false;

            if (cell.EnableCustomEditorBehaviors)
                return (cell.IsEditable | col.EditorDisplayBehavior == EditorDisplayBehaviors.Always);
            else
                return cell.IsEditable;
        }

        #endregion

        #region EditorLoaded
        /// <summary>
        /// Invoked when the editor's loaded event is raised. 
        /// </summary>
        /// <remarks>This method should be used to do such things as selecting the text of a TextBox, or causing a specific element in the editor to focus.</remarks>
        public override void EditorLoaded()
        {            

            if (this._dpEditor == FocusManager.GetFocusedElement(FocusManager.GetFocusScope(this._dpEditor)))
            {
                DatePickerTextBox tb = this.FindDatePickerTextBox(this._dpEditor);
                if (tb != null)
                    tb.Focus();
            }

        }
        #endregion // EditorLoaded

        #region FocusEditor

        /// <summary>
        /// Calls the ContentProvider to Attempt to Focus the underlying editor control
        /// </summary>
        protected internal override void FocusEditor()
        {
            if (this._dpEditor != null)
                this._dpEditor.Focus();
        }

        #endregion


        #region AdjustDatePicker_WPF
        private void AdjustDatePicker_WPF(DatePickerTextBox dptb)
        {
            string text = dptb.Text;
            
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            if (this._dpEditor.SelectedDate == null)
            {
                this._dpEditor.Text = string.Empty;
                this._dpEditor.Text = text;
            }
            else
            {
                if (string.IsNullOrEmpty(text))
                {
                    this._dpEditor.SelectedDate = null;
                    this._dpEditor.Text = text;
                }
                else
                {
                    // ok so we have a non null string, so we either have  a legal date or we don't.  
                    // but we should not have a null date.
                    DateTime currentDate = (DateTime)this._dpEditor.SelectedDate;

                    this._dpEditor.Text = string.Empty;

                    this._dpEditor.Text = text;

                    if (this._dpEditor.SelectedDate == null)
                    {
                        this._dpEditor.SelectedDate = currentDate;
                    }
                }
            }
        }
        #endregion // AdjustDatePicker_WPF


        #endregion // Methods

        #region Events

        void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			object valueChanged = null;

			if (e.AddedItems != null && e.AddedItems.Count > 0)
				valueChanged = e.AddedItems[0];

			this.NotifyEditorValueChanged(valueChanged);
		}

		#endregion // Events
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