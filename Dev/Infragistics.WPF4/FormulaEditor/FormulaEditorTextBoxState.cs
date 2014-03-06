using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Infragistics.Controls.Interactions.Primitives;

namespace Infragistics.Controls.Interactions
{
	internal sealed class FormulaEditorTextBoxState : UndoState
	{
		#region Member Variables

		private string _selectedText;
		private int _selectionStart;
		private string _text;

		#endregion  // Member Variables

		#region Constructor

		public FormulaEditorTextBoxState(FormulaEditorTextBox textBox)
		{
			// MD 5/8/12 - TFS97379
			// Use the actual text. Don't resolve it to an empty string.
			//_text = textBox.Text ?? string.Empty;
			_text = textBox.Text;

			_selectedText = textBox.Selection.Text;
			_selectionStart = textBox.Selection.Start.GetCharacterIndex(_text);

			// MD 5/8/12 - TFS97379
			// Use the actual text. Don't resolve it to an empty string.
			//if (_selectedText == null)
			//    _selectedText = string.Empty;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		public override bool Equals(object obj)
		{
			FormulaEditorTextBoxState otherState = obj as FormulaEditorTextBoxState;
			if (otherState == null)
				return false;

			return
				_text == otherState._text &&
				_selectionStart == otherState._selectionStart &&
				_selectedText == otherState._selectedText;
		}

		public override int GetHashCode()
		{
			return _text.GetHashCode();
		}

		#endregion  // Base Class Overrides

		#region Methods

		public override bool ShouldBeCommittedSynchronously(UndoState previousState)
		{
			// If a delete or backspace has occurred, we need an undo committed for each operation.

			FormulaEditorTextBoxState previousEditorState = (FormulaEditorTextBoxState)previousState;

			if (_selectedText == null || previousEditorState._selectedText == null)
				return false;

			int selectionLength = _selectedText.Length;
			int previousSelectionLength = _selectedText.Length;

			// We will mainly compare the unselected text because that will text us if a block of text was selected and then the
			// user typed a single character. In that case, even though the text is shorter, we don't want to consider it a delete
			// or backspace and commit it synchronously. Commit it asynchronously
			// MD 5/8/12 - TFS97379
			// The Text could be null now, so use the TextLength property, which will return 0 for null text.
			//int unselectedTextLength = this.Text.Length - selectionLength;
			//int previousUnselectedTextLength = previousEditorState.Text.Length - previousSelectionLength;
			int unselectedTextLength = this.TextLength - selectionLength;
			int previousUnselectedTextLength = previousEditorState.TextLength - previousSelectionLength;

			// If a block of text was selected and the user hit backspace or delete, the unselected text lengths will be the same,
			// but the selection length will decrease.
			if (unselectedTextLength == previousUnselectedTextLength)
				return selectionLength < previousSelectionLength;

			return unselectedTextLength < previousUnselectedTextLength;
		}

		#endregion  // Methods

		#region Properties

		public string Text
		{
			get { return _text; }
		}

		// MD 5/8/12 - TFS97379
		public int TextLength
		{
			get 
			{
				if (_text == null)
					return 0;

				return _text.Length;
			}
		}

		public int SelectionLength
		{
			get { return _selectedText.Length; }
		}

		public int SelectionStart
		{
			get { return _selectionStart; }
		}

		#endregion  // Properties
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