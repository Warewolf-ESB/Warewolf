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
	/// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="TextColumn"/>
	/// </summary>
	public class TextColumnContentProvider : ColumnContentProviderBase
	{
		#region Members

		TextBlock _tb;
		TextBox _editor;
        bool _bindingSet = false;
        bool _resetBindingSet;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Instantiates a new instance of the <see cref="TextColumnContentProvider"/>.
		/// </summary>
		public TextColumnContentProvider()
		{
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
			TextColumn column = (TextColumn)cell.Column;

			this._tb.TextWrapping = column.TextWrapping;

            this._tb.Style = column.TextBlockStyle;

            this.ApplyBindingToDisplayElement(cell, cellBinding);

			return this._tb;
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
            if (cellBinding != null)
            {
                this._tb.SetBinding(TextBlock.TextProperty, cellBinding);
                this._bindingSet = true;
                this._resetBindingSet = cell.Row.RowType == RowType.FilterRow;
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
            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            if (this._editor != null && cell.Control != null)
            {
                this._editor.SetBinding(Control.FlowDirectionProperty, new Binding("FlowDirection") { Source = cell.Control });
            }

            if (this._editor == null)
            {
                this._editor = new TextBox();                           
            }

            this._editor.TextChanged -= Editor_TextChanged;
            this._editor.TextChanged += Editor_TextChanged;

            TextColumn column = (TextColumn)cell.Column;

            TextColumnContentProvider.SetControlStyle(this._editor, cell.EditorStyleResolved);

            this._editor.TextWrapping = column.TextWrapping;

            this._editor.SetBinding(TextBox.TextProperty, editorBinding);

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
			object retValue = null;

			if (this._editor != null)
			{
				retValue = this._editor.Text;
			}

			return retValue;
		}

		#endregion // ResolveValueFromEditor

		#region EditorLoaded
		/// <summary>
		/// Invoked when the editor's loaded event is raised. 
		/// </summary>
		/// <remarks>This method should be used to do such things as selecting the text of a TextBox, or causing a specific element in the editor to focus.</remarks>
        public override void EditorLoaded()
        {
            if (this._editor != null)
                this._editor.SelectAll();
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
		public override object ApplyFormatting(object value, Column column, System.Globalization.CultureInfo culture)
		{
			TextColumn col = (TextColumn)column;
			
            if (!string.IsNullOrEmpty(col.FormatString))
				return String.Format(culture, col.FormatString, value);

            if (!string.IsNullOrEmpty(col.DataField.FormatString))
                return String.Format(culture, col.DataField.FormatString, value);

			return base.ApplyFormatting(value, column, culture);
		}

		#endregion // ApplyFormatting

        #region AdjustDisplayElement

        /// <summary>
        /// Called during EnsureContent to allow the provider a chance to modify it's display based on the current conditions.
        /// </summary>
        /// <param name="cell"></param>
        public override void AdjustDisplayElement(Cell cell)
        {
            base.AdjustDisplayElement(cell);

            // Its possible with the AddNewRow that there could be a timing issue, and no data will be available during ResolveDisplayElement
            // If that happens, then the binding will not be set. So in that case, when we do have a binding, apply it during AdjustDisplayElement.
            if (!this._bindingSet)
            {
                Binding b = this.ResolveBinding(cell);
                if (b != null)
                {
                    this._tb.SetBinding(TextBlock.TextProperty, b);
                    this._bindingSet = true;
                }
            }
        }

        #endregion // AdjustDisplayElement

        #region ResetContent
        /// <summary>
        /// Raised when the cell is recycling to allow the provider a chance to clear any internal members.
        /// </summary>
        public override void ResetContent()
        {
            base.ResetContent();
            if (this._resetBindingSet)
                this._bindingSet = false;
        }

        #endregion // ResetContent

        #region FocusEditor

        /// <summary>
        /// Calls the ContentProvider to Attempt to Focus the underlying editor control
        /// </summary>
        protected internal override void FocusEditor()
        {
            if (this._editor != null)
                this._editor.Focus();
        }

        #endregion

        #region EditorRemoved
        /// <summary>
        /// Raised when the editor is removed from the cell.
        /// </summary>
        public override void EditorRemoved()
        {
            if (this._editor != null)
            {






            }
            base.EditorRemoved();
        }
        #endregion // EditorRemoved

        #endregion // Methods

        #region Properties

        #region RemovePaddingDuringEditing

        /// <summary>
		/// Gets/Sets whether the padding of a <see cref="Cell"/> should be removed before putting an editor into edit mode. 
		/// </summary>
		/// <remarks>
		/// This property will determine the availableHeight and availableWidth parameters of the ResolveEditorControl method.
		/// </remarks>
		public override bool RemovePaddingDuringEditing
		{
			get { return true; }
		}

		#endregion // RemovePaddingDuringEditing

		#endregion // Properties

		#region EventHandlers 

		void Editor_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.NotifyEditorValueChanged(this._editor.Text);
		}
		#endregion // EventHandlers
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