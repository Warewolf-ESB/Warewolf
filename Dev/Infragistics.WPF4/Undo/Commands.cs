using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Infragistics.Controls;
using System.Windows.Data;
using System.Windows;

namespace Infragistics.Undo
{
	#region UndoManagerCommandType enum
	/// <summary>
	/// Enumeration used by an <see cref="UndoManagerCommandSource"/> to identify a command for a <see cref="UndoManager"/> instance.
	/// </summary>
	public enum UndoManagerCommandType
	{
		/// <summary>
		/// Performs an undo operation. The parameter should be an object that was registered as a reference (see <see cref="UndoManager.RegisterReference(object)"/> or an <see cref="UndoManager"/> instance or null to operate on the shared <see cref="UndoManager.Current"/>.
		/// </summary>
		Undo,

		/// <summary>
		/// Performs a redo operation. The parameter should be an object that was registered as a reference (see <see cref="UndoManager.RegisterReference(object)"/> or an <see cref="UndoManager"/> instance or null to operate on the shared <see cref="UndoManager.Current"/>.
		/// </summary>
		Redo,

		/// <summary>
		/// Invokes an undo or redo for a given <see cref="UndoHistoryItem"/> up to and including the HistoryItem. The parameter must be a <see cref="UndoHistoryItem"/> instance.
		/// </summary>
		UndoRedoHistoryItem,

		/// <summary>
		/// Temporarily prevents merging of UndoUnits until the next UndoUnit that is not part of a Undo/Redo operation is performed.
		/// </summary>
		PreventMerge,
	} 
	#endregion //UndoManagerCommandType enum

	#region UndoManagerCommandSource
	/// <summary>
	/// The command source for the <see cref="UndoManager"/>
	/// </summary>
	public class UndoManagerCommandSource : CommandSource
	{
		#region Member Variables

		private UndoManagerCommandType _commandType;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManagerCommandSource"/>
		/// </summary>
		public UndoManagerCommandSource()
		{
			// we need a target in order to be executed and the undo manager won't exist
			// within the tree. someone can still overwrite this but at least by default 
			// it will handle being executable
			this.Target = this;
		}
		#endregion //Constructor

		#region Base class overrides

		#region ResolveCommand
		/// <summary>
		/// Creates the <see cref="ICommand"/> that will perform the associated action based upon the <see cref="CommandType"/> property value.
		/// </summary>
		/// <seealso cref="CommandType"/>
		protected override ICommand ResolveCommand()
		{
			switch (_commandType)
			{
				case UndoManagerCommandType.Undo:
					return new UndoManagerUndoCommand();
				case UndoManagerCommandType.Redo:
					return new UndoManagerRedoCommand();
				case UndoManagerCommandType.UndoRedoHistoryItem:
					return new UndoManagerHistoryItemCommand();
				case UndoManagerCommandType.PreventMerge:
					return new UndoManagerPreventMergeCommand();
			}

			return null;
		}
		#endregion //ResolveCommand

		#endregion //Base class overrides

		#region Properties

		#region CommandType
		/// <summary>
		/// Returns or sets an enumeration used to identify the command that should be executed when the command source is triggered.
		/// </summary>
		public UndoManagerCommandType CommandType
		{
			get { return _commandType; }
			set { _commandType = value; }
		}
		#endregion //CommandType

		#endregion //Properties
	} 
	#endregion //UndoManagerCommandSource

	#region UndoManagerCommandBase
	/// <summary>
	/// Base class for a command for an <see cref="UndoManager"/> where the parameter may be a reference or an <see cref="UndoManager"/>
	/// </summary>
	public abstract class UndoManagerCommandBase : CommandBase
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManagerCommandBase"/>
		/// </summary>
		protected UndoManagerCommandBase()
		{
		}
		#endregion //Constructor

		#region Base class overrides
		/// <summary>
		/// Returns a boolean indicating if the command can be executed based on the specified parameter
		/// </summary>
		/// <param name="parameter">The parameter to evaluate. May be an integer to indicate the number of items in the history to undo.</param>
		/// <returns>True if the parameter </returns>
		public override bool CanExecute(object parameter)
		{
			return GetUndoManager(parameter) != null;
		}

		/// <summary>
		/// Invoked when the command should be executed.
		/// </summary>
		/// <param name="parameter">The parameter to evaluate. Should be a <see cref="UndoHistoryItem"/> instance.</param>
		public override void Execute(object parameter)
		{
			UndoManager mgr = GetUndoManager(parameter);

			if (null != mgr && this.Execute(mgr))
			{
				if (null != this.CommandSource)
					this.CommandSource.Handled = true;
			}
		}
		#endregion //Base class overrides

		#region Methods

		#region Protected Methods

		#region Execute

		/// <summary>
		/// Used to perform the action associated with the command.
		/// </summary>
		/// <param name="manager">The undo manager for which the operation is to be performed</param>
		/// <returns>Returns true if the operation was performed. Otherwise false is returned.</returns>
		protected abstract bool Execute(UndoManager manager);

		#endregion //Execute 

		#endregion //Protected Methods

		#region Internal Methods

		#region GetUndoManager
		internal static UndoManager GetUndoManager(object parameter)
		{
			UndoManager manager = parameter as UndoManager;

			if (manager == null)
				manager = UndoManager.FromReference(parameter);

			return manager;
		}
		#endregion //GetUndoManager 

		#endregion //Internal Methods

		#endregion //Methods
	}
	#endregion //UndoManagerUndoRedoCommand

	#region UndoManagerUndoCommand
	/// <summary>
	/// Custom command for invoking the <see cref="UndoManager.Undo(int)"/> method on an <see cref="UndoManager"/>
	/// </summary>
	public class UndoManagerUndoCommand : UndoManagerCommandBase
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManagerUndoCommand"/>
		/// </summary>
		public UndoManagerUndoCommand()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region Execute
		/// <summary>
		/// Used to perform the action associated with the command.
		/// </summary>
		/// <param name="manager">The undo manager for which the operation is to be performed</param>
		/// <returns>Returns true if the operation was performed. Otherwise false is returned.</returns>
		protected override bool Execute(UndoManager manager)
		{
			manager.Undo(1);
			return true;
		}

		#endregion //Execute 

		#endregion //Base class overrides
	} 
	#endregion //UndoManagerUndoCommand

	#region UndoManagerRedoCommand
	/// <summary>
	/// Custom command for invoking the <see cref="UndoManager.Redo(int)"/> method on an <see cref="UndoManager"/>
	/// </summary>
	public class UndoManagerRedoCommand : UndoManagerCommandBase
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManagerRedoCommand"/>
		/// </summary>
		public UndoManagerRedoCommand()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region Execute
		/// <summary>
		/// Used to perform the action associated with the command.
		/// </summary>
		/// <param name="manager">The undo manager for which the operation is to be performed</param>
		/// <returns>Returns true if the operation was performed. Otherwise false is returned.</returns>
		protected override bool Execute(UndoManager manager)
		{
			manager.Redo(1);
			return true;
		}

		#endregion //Execute 

		#endregion //Base class overrides
	} 
	#endregion //UndoManagerRedoCommand

	#region UndoManagerHistoryItemCommand
	/// <summary>
	/// Custom command used to execute an undo/redo for an <see cref="UndoManager"/> given a specific <see cref="UndoHistoryItem"/>
	/// </summary>
	public class UndoManagerHistoryItemCommand : CommandBase
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManagerHistoryItemCommand"/>
		/// </summary>
		public UndoManagerHistoryItemCommand()
		{
		}
		#endregion //Constructor

		#region Base class overrides
		/// <summary>
		/// Returns a boolean indicating if the command can be executed based on the specified parameter
		/// </summary>
		/// <param name="parameter">The parameter to evaluate. Should be a <see cref="UndoHistoryItem"/> instance</param>
		/// <returns>True if the parameter </returns>
		public override bool CanExecute(object parameter)
		{
			var item = parameter as UndoHistoryItem;

			if (item == null)
				return false;

			return item.CanUndoRedo();
		}

		/// <summary>
		/// Invoked when the command should be executed.
		/// </summary>
		/// <param name="parameter">The parameter to evaluate. Should be a <see cref="UndoHistoryItem"/> instance.</param>
		public override void Execute(object parameter)
		{
			var item = parameter as UndoHistoryItem;

			if (null != item)
			{
				item.PerformUndoRedo(false);

				if (null != this.CommandSource)
					this.CommandSource.Handled = true;
			}
		}
		#endregion //Base class overrides
	} 
	#endregion //UndoManagerHistoryItemCommand

	#region UndoManagerPreventMergeCommand
	/// <summary>
	/// Custom command for invoking the <see cref="UndoManager.ShouldPreventMerge()"/> method on an <see cref="UndoManager"/>
	/// </summary>
	public class UndoManagerPreventMergeCommand : UndoManagerCommandBase
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManagerPreventMergeCommand"/>
		/// </summary>
		public UndoManagerPreventMergeCommand()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region Execute
		/// <summary>
		/// Used to perform the action associated with the command.
		/// </summary>
		/// <param name="manager">The undo manager for which the operation is to be performed</param>
		/// <returns>Returns true if the operation was performed. Otherwise false is returned.</returns>
		protected override bool Execute(UndoManager manager)
		{
			manager.PreventMerge();
			return true;
		}

		#endregion //Execute

		#endregion //Base class overrides
	}
	#endregion //UndoManagerUndoCommand
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