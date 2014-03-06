using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

namespace Infragistics.Controls.Interactions.Primitives
{
	#region FormulaEditorCommand

	/// <summary>
	/// An enumeration of available commands for the formula editor and dialog
	/// </summary>
	public enum FormulaEditorCommand
	{
		/// <summary>
		/// Inserts the item from the auto-complete list in the formula at the current edit position.
		/// </summary>
		AutoCompleteItem,

		/// <summary>
		/// Cancels out of the <see cref="FormulaEditorDialog"/>.
		/// </summary>
		CancelDialog,

		/// <summary>
		/// Clears the formula in the <see cref="FormulaEditorDialog"/>.
		/// </summary>
		ClearFormula,

		/// <summary>
		/// Commits the formula from the <see cref="FormulaEditorDialog"/> to the target and closes the dialog.
		/// </summary>
		CommitDialog,

		/// <summary>
		/// Displays the <see cref="FormulaEditorDialog"/>.
		/// </summary>
		DisplayDialog,

		/// <summary>
		/// Inserts the function in the formula.
		/// </summary>
		InsertFunction,

		/// <summary>
		/// Inserts the operand in the formula.
		/// </summary>
		InsertOperand,

		/// <summary>
		/// Inserts the operator in the formula.
		/// </summary>
		InsertOperator,

		/// <summary>
		/// Shows the next syntax error of the formula.
		/// </summary>
		NextSyntaxError,

		/// <summary>
		/// Shows the previous syntax error of the formula.
		/// </summary>
		PreviousSyntaxError,

		/// <summary>
		/// Performs a redo operation in the <see cref="FormulaEditorDialog"/>.
		/// </summary>
		RedoFormulaEdit,

		/// <summary>
		/// Performs a undo operation in the <see cref="FormulaEditorDialog"/>.
		/// </summary>
		UndoFormulaEdit,
	}

	#endregion // FormulaEditorCommand


	#region FormulaEditorCommandSource Class

	/// <summary>
	/// The command source object for the <see cref="XamFormulaEditor"/> or <see cref="FormulaEditorDialog"/> object.
	/// </summary>
	public class FormulaEditorCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the FormulaEditorCommand which is to be executed by the command.
		/// </summary>
		public FormulaEditorCommand CommandType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the key the command should be executed for when the event arguments are KeyEventArgs.
		/// </summary>
		public Key? Key 
		{
			get; 
			set; 
		}

		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand()
		{
			switch (this.CommandType)
			{
				case FormulaEditorCommand.AutoCompleteItem:
					return new AutoCompleteItemCommand();

				case FormulaEditorCommand.CancelDialog:
					return new CancelDialogCommand();

				case FormulaEditorCommand.ClearFormula:
					return new ClearFormulaCommand();

				case FormulaEditorCommand.CommitDialog:
					return new CommitDialogCommand();

				case FormulaEditorCommand.DisplayDialog:
					return new DisplayDialogCommand();

				case FormulaEditorCommand.InsertFunction:
					return new InsertFunctionCommand();

				case FormulaEditorCommand.InsertOperand:
					return new InsertOperandCommand();

				case FormulaEditorCommand.InsertOperator:
					return new InsertOperatorCommand();

				case FormulaEditorCommand.NextSyntaxError:
					return new NextSyntaxErrorCommand();

				case FormulaEditorCommand.PreviousSyntaxError:
					return new PreviousSyntaxErrorCommand();

				case FormulaEditorCommand.RedoFormulaEdit:
					return new RedoFormulaEditCommand();

				case FormulaEditorCommand.UndoFormulaEdit:
					return new UndoFormulaEditCommand();
			}

			Debug.Assert(false, "Unknown FormulaEditorCommand: " + this.CommandType);
			return null;
		}
	}

	#endregion // FormulaEditorCommandSource Class

	#region FormulaEditorCommandBase Class

	/// <summary>
	/// Base class for commands that deal with the <see cref="XamFormulaEditor"/> or <see cref="FormulaEditorDialog"/> object.
	/// </summary>
	public abstract class FormulaEditorCommandBase : CommandBase
	{
		#region Member Variables

		private FormulaEditorBase _editor;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="FormulaEditorCommandBase"/> instance.
		/// </summary>
		protected FormulaEditorCommandBase() { }

		internal FormulaEditorCommandBase(FormulaEditorBase editor)
		{
			_editor = editor;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			FormulaEditorCommandSource commandSource = this.CommandSource as FormulaEditorCommandSource;

			if (commandSource != null)
			{
				KeyEventArgs keyArgs = commandSource.OriginEventArgs as KeyEventArgs;

				if (keyArgs != null && commandSource.Key.HasValue && keyArgs.Key != commandSource.Key)
					return false;
			}

			FormulaEditorBase editor = this.GetEditor(parameter);

			if (editor != null)
				return this.CanExecuteCommand(editor, parameter);

			return true;
		}
		#endregion

		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter">The <see cref="XamFormulaEditor"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			FormulaEditorBase editor = this.GetEditor(parameter);
			if (editor != null)
			{
				this.ExecuteCommand(editor, parameter);

				if (this.CommandSource != null)
					this.CommandSource.Handled = true;
			}

			base.Execute(parameter);
		}
		#endregion // Execute

		#endregion  // Base Class Overrides

		#region Methods

		#region Internal Methods

		#region CanExecuteCommand



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool CanExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			return true;
		}

		#endregion  // CanExecuteCommand

		#region ExecuteCommand



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal abstract void ExecuteCommand(FormulaEditorBase editor, object parameter);

		#endregion //ExecuteCommand

		#region RaiseCanExecuteChanged

		internal void RaiseCanExecuteChanged()
		{
			this.OnCanExecuteChanged();
		}

		#endregion  // RaiseCanExecuteChanged

		#endregion // Internal Methods

		#region Private Methods

		#region GetEditor

		private FormulaEditorBase GetEditor(object parameter)
		{
			FormulaEditorBase editor = parameter as FormulaEditorBase;

			if (editor == null)
			{
				IFormulaElement formulaElement = parameter as IFormulaElement;
				if (formulaElement != null)
					editor = formulaElement.Editor;
			}

			return editor ?? _editor;
		}

		#endregion  // GetEditor

		#endregion  // Private Methods

		#endregion // Methods
	}

	#endregion // FormulaEditorCommandBase Class


	#region AutoCompleteItemCommand

	/// <summary>
	/// A command that inserts the item from the auto-complete list in the formula at the current edit position.
	/// </summary>
	public class AutoCompleteItemCommand : FormulaEditorCommandBase
	{
		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor.TextBox != null)
			{
				ContextualHelpHost contextualHelpHost = editor.TextBox.GetContextualHelpHost();
				if (contextualHelpHost != null)
					contextualHelpHost.CommitCurrentAutoCompleteItem();
			}
		}
	}

	#endregion  // AutoCompleteItemCommand

	#region CancelDialogCommand

	/// <summary>
	/// A command that cancels out of the <see cref="FormulaEditorDialog"/>.
	/// </summary>
	public class CancelDialogCommand : FormulaEditorCommandBase
	{
		/// <summary>
		/// Creates a new <see cref="CancelDialogCommand"/> instance.
		/// </summary>
		public CancelDialogCommand() { }

		internal CancelDialogCommand(FormulaEditorBase editor)
			: base(editor) { }

		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor != null)
				editor.CancelEdit();
		}
	}

	#endregion  // CancelDialogCommand

	#region ClearFormulaCommand

	/// <summary>
	/// A command that clears the formula in the <see cref="FormulaEditorDialog"/>.
	/// </summary>
	public class ClearFormulaCommand : FormulaEditorCommandBase
	{
		/// <summary>
		/// Creates a new <see cref="ClearFormulaCommand"/> instance.
		/// </summary>
		public ClearFormulaCommand() { }

		internal ClearFormulaCommand(FormulaEditorBase editor)
			: base(editor) { }

		internal override bool CanExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor != null)
				return String.IsNullOrEmpty(editor.Formula) == false;

			return base.CanExecuteCommand(editor, parameter);
		}

		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor != null)
				editor.Formula = string.Empty;
		}
	}

	#endregion  // ClearFormulaCommand

	#region CommitDialogCommand

	/// <summary>
	/// A command that commits the formula from the <see cref="FormulaEditorDialog"/> to the target and closes the dialog.
	/// </summary>
	public class CommitDialogCommand : FormulaEditorCommandBase
	{
		/// <summary>
		/// Creates a new <see cref="CommitDialogCommand"/> instance.
		/// </summary>
		public CommitDialogCommand() { }

		internal CommitDialogCommand(FormulaEditorBase editor)
			: base(editor) { }

		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor != null)
				editor.CommitEdit();
		}
	}

	#endregion  // CommitDialogCommand

	#region DisplayDialogCommand

	/// <summary>
	/// A command that displays the <see cref="FormulaEditorDialog"/>.
	/// </summary>
	public class DisplayDialogCommand : FormulaEditorCommandBase
	{
		internal override bool CanExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			return editor is XamFormulaEditor;
		}

		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			XamFormulaEditor formulaEditor = editor as XamFormulaEditor;

			if (formulaEditor != null)
				formulaEditor.DisplayFormulaEditorDialog();
		}
	}

	#endregion // DisplayDialogCommand

	#region InsertFunctionCommand

	/// <summary>
	/// A command that inserts the function in the formula.
	/// </summary>
	public class InsertFunctionCommand : FormulaEditorCommandBase
	{
		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			FunctionInfo functionInfo = parameter as FunctionInfo;
			if (functionInfo != null)
				FormulaEditorUtilities.InsertFunction(editor, functionInfo.Function);
		}
	}

	#endregion  // InsertFunctionCommand

	#region InsertOperandCommand

	/// <summary>
	/// A command that inserts the operand in the formula.
	/// </summary>
	public class InsertOperandCommand : FormulaEditorCommandBase
	{
		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			OperandInfo operandInfo = parameter as OperandInfo;
			if (operandInfo != null && operandInfo.IsDataReference)
				FormulaEditorUtilities.InsertOperand(editor, operandInfo);
		}
	}

	#endregion  // InsertOperandCommand

	#region InsertOperatorCommand

	/// <summary>
	/// A command that inserts the operator in the formula.
	/// </summary>
	public class InsertOperatorCommand : FormulaEditorCommandBase
	{
		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			OperatorInfo operatorInfo = parameter as OperatorInfo;
			if (operatorInfo != null)
				FormulaEditorUtilities.InsertOperator(editor, operatorInfo.Value);
		}
	}

	#endregion  // InsertOperatorCommand

	#region NextSyntaxErrorCommand

	/// <summary>
	/// A command that shows the next syntax error of the formula.
	/// </summary>
	public class NextSyntaxErrorCommand : FormulaEditorCommandBase
	{
		/// <summary>
		/// Creates a new <see cref="NextSyntaxErrorCommand"/> instance.
		/// </summary>
		public NextSyntaxErrorCommand() { }

		internal NextSyntaxErrorCommand(FormulaEditorBase editor)
			: base(editor) { }

		internal override bool CanExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			FormulaEditorDialog dialog = editor as FormulaEditorDialog;
			if (dialog != null)
			{
				if (dialog.SyntaxErrorInfos == null)
					return false;

				if (dialog.CurrentSyntaxErrorInfo == null)
					return false;

				int index = dialog.SyntaxErrorInfos.IndexOf(dialog.CurrentSyntaxErrorInfo);

				if (index < 0 || dialog.SyntaxErrorInfos.Count - 1 <= index)
					return false;

				return true;
			}

			return base.CanExecuteCommand(dialog, parameter);
		}

		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			FormulaEditorDialog dialog = editor as FormulaEditorDialog;
			if (dialog != null)
			{
				if (dialog.SyntaxErrorInfos == null)
					return;

				if (dialog.CurrentSyntaxErrorInfo == null)
					return;

				int index = dialog.SyntaxErrorInfos.IndexOf(dialog.CurrentSyntaxErrorInfo);

				if (index < 0 || dialog.SyntaxErrorInfos.Count - 1 <= index)
					return;

				dialog.CurrentSyntaxErrorInfo = dialog.SyntaxErrorInfos[index + 1];
			}
		}
	}

	#endregion  // UndoFormulaEditCommand

	#region PreviousSyntaxErrorCommand

	/// <summary>
	/// A command that shows the previous syntax error of the formula.
	/// </summary>
	public class PreviousSyntaxErrorCommand : FormulaEditorCommandBase
	{
		/// <summary>
		/// Creates a new <see cref="PreviousSyntaxErrorCommand"/> instance.
		/// </summary>
		public PreviousSyntaxErrorCommand() { }

		internal PreviousSyntaxErrorCommand(FormulaEditorBase editor)
			: base(editor) { }

		internal override bool CanExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			FormulaEditorDialog dialog = editor as FormulaEditorDialog;
			if (dialog != null)
			{
				if (dialog.SyntaxErrorInfos == null)
					return false;

				if (dialog.CurrentSyntaxErrorInfo == null)
					return false;

				int index = dialog.SyntaxErrorInfos.IndexOf(dialog.CurrentSyntaxErrorInfo);
				return index > 0;
			}

			return base.CanExecuteCommand(dialog, parameter);
		}

		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			FormulaEditorDialog dialog = editor as FormulaEditorDialog;
			if (dialog != null)
			{
				if (dialog.SyntaxErrorInfos == null)
					return;

				if (dialog.CurrentSyntaxErrorInfo == null)
					return;

				int index = dialog.SyntaxErrorInfos.IndexOf(dialog.CurrentSyntaxErrorInfo);
				if (index > 0)
					dialog.CurrentSyntaxErrorInfo = dialog.SyntaxErrorInfos[index - 1];
			}
		}
	}

	#endregion  // PreviousSyntaxErrorCommand

	#region RedoFormulaEditCommand

	/// <summary>
	/// A command that performs a redo operation in the <see cref="FormulaEditorDialog"/>.
	/// </summary>
	public class RedoFormulaEditCommand : FormulaEditorCommandBase
	{
		/// <summary>
		/// Creates a new <see cref="RedoFormulaEditCommand"/> instance.
		/// </summary>
		public RedoFormulaEditCommand() { }

		internal RedoFormulaEditCommand(FormulaEditorBase editor)
			: base(editor) { }

		internal override bool CanExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor != null)
				return editor.CanRedo;

			return base.CanExecuteCommand(editor, parameter);
		}

		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor != null)
				editor.Redo();
		}
	}

	#endregion  // RedoFormulaEditCommand

	#region UndoFormulaEditCommand

	/// <summary>
	/// A command that performs a undo operation in the <see cref="FormulaEditorDialog"/>.
	/// </summary>
	public class UndoFormulaEditCommand : FormulaEditorCommandBase
	{
		/// <summary>
		/// Creates a new <see cref="UndoFormulaEditCommand"/> instance.
		/// </summary>
		public UndoFormulaEditCommand() { }

		internal UndoFormulaEditCommand(FormulaEditorBase editor)
			: base(editor) { }

		internal override bool CanExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor != null)
				return editor.CanUndo;

			return base.CanExecuteCommand(editor, parameter);
		}

		internal override void ExecuteCommand(FormulaEditorBase editor, object parameter)
		{
			if (editor != null)
				editor.Undo();
		}
	}

	#endregion  // UndoFormulaEditCommand
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