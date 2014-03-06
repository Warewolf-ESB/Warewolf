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


namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="CheckBoxColumn"/>
	/// </summary>
	public class CheckBoxColumnContentProvider : ColumnContentProviderBase
	{
		#region Members

		CheckBox _cb;
        TextBlock _tb;
        bool _dataWasNull;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Instantiates a new instance of the <see cref="CheckBoxColumnContentProvider"/>.
		/// </summary>
		public CheckBoxColumnContentProvider()
		{
			this._cb = new CheckBox();

            this._cb.ClickMode = ClickMode.Press;

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
            this._cb.IsThreeState = (cell.Column.DataType == typeof(bool?));

            var col = (CheckBoxColumn)cell.Column;

            this.ApplyBindingToDisplayElement(cell, cellBinding);

            if (cell.EnableCustomEditorBehaviors && (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode))
            {
                return _tb;
            }

            this._cb.IsEnabled = cell.IsEditable;

            
            CheckBoxColumnContentProvider.SetControlStyle(this._cb, cell.EditorStyleResolved);

		    bool isEditable = false;

            if (cell.EnableCustomEditorBehaviors)
            {
                isEditable = col.EditorDisplayBehavior == EditorDisplayBehaviors.Always;
            }

		    this._cb.IsHitTestVisible = isEditable;

		    this._cb.Focusable = isEditable;


			return this._cb;
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
                binding.ValidatesOnDataErrors = binding.ValidatesOnExceptions = binding.NotifyOnValidationError = ((CheckBoxColumn)cell.Column).AllowEditingValidation;



                var col = (CheckBoxColumn)cell.Column;

                if (cell.EnableCustomEditorBehaviors && (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode))
                    _tb.SetBinding(TextBlock.TextProperty, binding);
                else
                    this._cb.SetBinding(CheckBox.IsCheckedProperty, binding);
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
            // Ok, so with the perf improvements, add new row doesn't exactly play nicely, when it creates cells before the data is loaded. 
            // So what happens is we need to track when the data changes, and if it does, then make sure its binding is applied
            // and that the IsEnabled property is correct. 
            bool dataIsNull = (cell.Row.Data == null);
            if (this._dataWasNull && !dataIsNull)
            {
                Binding binding = this.ResolveBinding(cell);

                if (binding != null)
                {
                    binding.ValidatesOnDataErrors = binding.ValidatesOnExceptions = binding.NotifyOnValidationError = ((CheckBoxColumn)cell.Column).AllowEditingValidation;



                    var col = (CheckBoxColumn)cell.Column;

                    if (cell.EnableCustomEditorBehaviors && (col.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode))
                        _tb.SetBinding(TextBlock.TextProperty, binding);
                    else
                        this._cb.SetBinding(CheckBox.IsCheckedProperty, binding);
                }

                this._cb.IsEnabled = cell.IsEditable;
            }

            this._dataWasNull = dataIsNull;

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
			this._cb.IsHitTestVisible = true;

            this._cb.Focusable = true;


            
            CheckBoxColumnContentProvider.SetControlStyle(this._cb, cell.EditorStyleResolved);

            this._cb.IsThreeState = (cell.Column.DataType == typeof(bool?));

            if (this._cb.GetBindingExpression(CheckBox.IsCheckedProperty) == null)
            {
                if (editorBinding != null)
                {
                    this._cb.SetBinding(CheckBox.IsCheckedProperty, editorBinding);
                }
            }

            if (cell.IsEditing)
                this._cb.Focus();

			return this._cb;
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
			return this._cb.IsChecked;
		}
		#endregion // ResolveValueFromEditor

        #region FocusEditor

        /// <summary>
        /// Calls the ContentProvider to Attempt to Focus the underlying editor control
        /// </summary>
        protected internal override void FocusEditor()
        {
            if (this._cb != null)
                this._cb.Focus();
        }

        #endregion

		#endregion // Methods
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