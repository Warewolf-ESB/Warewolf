using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Infragistics.Undo
{
	/// <summary>
	/// Custom <see cref="UndoUnit"/> that contains one or more <see cref="UndoUnit"/>
	/// </summary>
	/// <seealso cref="Infragistics.Undo.UndoManager.StartTransaction(string, string)"/>
	/// <seealso cref="Infragistics.Undo.UndoManager.CurrentTransaction"/>
	/// <seealso cref="Infragistics.Undo.UndoManager.RootTransaction"/>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class UndoTransaction : UndoUnit,
		IUndoTransactionOwner
	{
		#region Member Variables

		private StackList<UndoUnit> _unitsSource;
		private ReadOnlyCollection<UndoUnit> _units;
		private IUndoTransactionOwner _parent;
		private bool? _isOpened;
		private UndoTransaction _openTransaction;
		private string _description;
		private string _descriptionDetailed;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoTransaction"/>
		/// </summary>
		/// <param name="description">The description for the transaction or null to provide a default one.</param>
		/// <param name="detailedDescription">The detailed description for the transaction or null to provide a default one.</param>
		/// <remarks>
		/// <p class="note">A transaction is typically created by using the <see cref="UndoManager.StartTransaction(string, string)"/>. The control 
		/// is exposed in case a derived transaction class is needed in which case one would need a derived <see cref="UndoUnitFactory"/> 
		/// where the <see cref="UndoUnitFactory.CreateTransaction(string, string)"/> method is overriden and then an instance of that factory 
		/// class is set on either the <see cref="UndoUnitFactory.Current"/> or the <see cref="UndoManager.UndoUnitFactory"/> properties.</p>
		/// </remarks>
		internal protected UndoTransaction(string description, string detailedDescription)
		{
			_description = description;
			_descriptionDetailed = detailedDescription;
			_unitsSource = new StackList<UndoUnit>();
			_units = new ReadOnlyCollection<UndoUnit>(_unitsSource);
		} 
		#endregion //Constructor

		#region Base class overrides

		#region Execute
		/// <summary>
		/// Used to perform the associated action for all items in the <see cref="Units"/>
		/// </summary>
		/// <param name="executeInfo">Provides information about the undo/redo operation being executed.</param>
		/// <returns>Returns true if one or more of the <see cref="Units"/> executed successfully. Otherwise false is returned.</returns>
		protected internal override bool Execute(UndoExecuteContext executeInfo)
		{
			if (_openTransaction != null)
				throw new InvalidOperationException(Utils.GetString("LE_CannotExecuteOpenTransaction", _openTransaction)); 

			bool executed = false;
			var transaction = this.StartTransaction(executeInfo);

			try
			{
				foreach (UndoUnit unit in _unitsSource)
				{
					if (executeInfo.Execute(unit))
						executed = true;
				}
			}
			finally
			{
				// if we create a transaction around this operation and it 
				// wasn't implicitly closed by closing an ancestor then close it now
				if (transaction != null && !transaction.IsClosed)
					transaction.Commit();
			}

			return executed;
		}
		#endregion //Execute

		#region ForEach
		/// <summary>
		/// Used to invoke an action on the descendants.
		/// </summary>
		/// <param name="action">The action to invoke on descendants</param>
		internal override void ForEach(Action<UndoUnit> action)
		{
			base.ForEach(action);

			foreach (var child in _units)
			{
				// call it on each child
				action(child);

				// call recursively down the chain
				child.ForEach(action);
			}
		} 
		#endregion //ForEach

		#region GetDescription
		/// <summary>
		/// Returns a string representation of the action based on whether this is for an undo or redo operation.
		/// </summary>
		/// <param name="itemType">The type of history for which the description is being requested.</param>
		/// <param name="detailed">A boolean indicating if a detailed description should be returned. For example, when false one may return "Typing" but for verbose one may return "Typing 'qwerty'".</param>
		public override string GetDescription(UndoHistoryItemType itemType, bool detailed)
		{
			string description = detailed ? _descriptionDetailed : _description;

			if (description == null)
			{
				if (_unitsSource.Count > 0)
				{
					foreach (var undoUnit in _unitsSource)
					{
						description = _unitsSource.Peek().GetDescription(itemType, detailed);
						if (description != null)
							break;
					}
				}

				if (description == null)
					description = Utils.GetString("FallbackTransactionDescription");
			}

			return description;
		}
		#endregion //GetDescription

		#region Merge
		/// <summary>
		/// Used to allow multiple consecutive undo units to be merged into a single operation.
		/// </summary>
		/// <param name="mergeInfo">Provides information about the unit to evaluate for a merge operation</param>
		/// <returns>Returns an enumeration used to provide identify how the unit was merged.</returns>
		protected internal override UndoMergeAction Merge(UndoMergeContext mergeInfo)
		{
			var result = UndoMergeAction.NotMerged;

			if (_isOpened == true && _unitsSource.Count > 0)
			{
				var last = _unitsSource.Peek();
				result = last.Merge(mergeInfo);

				// if the unit wants to be removed then remove it from our collection
				if (result == UndoMergeAction.MergedRemoveUnit)
				{
					_unitsSource.Pop();

					// I originally thought of letting this transaction be popped but that 
					// could be an issue if it was opened. also it could be a coincidence 
					// that the 1st change after a transaction was unmerged but the person 
					// who opened the transaction is expecting it to be kept open
					//// if we still have units then we can consider it a regular merge
					//if (_units.Count > 0)
					result = UndoMergeAction.Merged;
				}
			}

			return result;
		}
		#endregion //Merge

		#region RemoveAll
		/// <summary>
		/// Used to remove any matching descendants.
		/// </summary>
		/// <param name="match">The routine to evaluate any descendant units. This should not be executed against the UndoUnit whose RemoveAll is being called.</param>
		/// <returns>Returns true if the item should be removed</returns>
		internal override bool RemoveAll(Func<UndoUnit, bool> match)
		{
			// act on the child items if this item is not to be removed
			this._unitsSource.RemoveAll(match);

			// remove it if all the child items were removed
			return _unitsSource.Count == 0;
		}
		#endregion //RemoveAll

		#region Target
		/// <summary>
		/// Returns null since the <see cref="UndoTransaction"/> is a group of <see cref="UndoUnit"/> instances and may affect multiple targets
		/// </summary>
		public override object Target
		{
			get { return null; }
		} 
		#endregion //Target

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region IsClosed
		/// <summary>
		/// Returns a boolean indicating if the transaction has been closed/ended.
		/// </summary>
		public bool IsClosed
		{
			get { return _isOpened == false; }
		} 
		#endregion //IsClosed

		#region OpenTransaction
		/// <summary>
		/// Returns the child transaction that is currently opened.
		/// </summary>
		public UndoTransaction OpenTransaction
		{
			get { return _openTransaction; }
		}
		#endregion //OpenTransaction

		#region Parent
		/// <summary>
		/// Returns the parent/containing <see cref="UndoTransaction"/>
		/// </summary>
		public UndoTransaction Parent
		{
			get { return _parent as UndoTransaction; }
		} 
		#endregion //Parent

		#region Units
		/// <summary>
		/// Returns a read-only collection of the child units.
		/// </summary>
		/// <remarks>
		/// <p class="body">Units are added to a group by using the <see cref="UndoManager.AddChange(UndoUnit)"/> method.</p>
		/// </remarks>
		public IList<UndoUnit> Units
		{
			get { return _units; }
		}
		#endregion //Units

		#endregion //Public Properties

		#region Internal Properties

		#region OpenState
		internal bool? OpenState
		{
			get { return _isOpened; }
		}
		#endregion //OpenState

		#region Owner
		internal IUndoTransactionOwner Owner
		{
			get { return _parent; }
		}
		#endregion //Owner

		#endregion //Internal Properties

		#region Private Properties

		#region DebuggerDisplay
		private string DebuggerDisplay
		{
			get { return string.Format("UndoTransaction: {0} items", _units.Count); }
		}
		#endregion //DebuggerDisplay

		#endregion //Private Properties
		#endregion //Properties

		#region Methods

		#region Private Methods

		#region Close
		/// <summary>
		/// Used to close the group either committing the units or cancelling them.
		/// </summary>
		/// <param name="closeAction">Indicates the action to take</param>
		private void Close(UndoTransactionCloseAction closeAction)
		{
			this.VerifyCanModify();

			var mgr = _parent.UndoManager;
			Debug.Assert(null != mgr);

			mgr.VerifyCanClose(this);

			if (_openTransaction != null)
				_openTransaction.Close(closeAction);

			// consider ourselves closed...
			_isOpened = false;

			// undo any pending transactions
			if (closeAction == UndoTransactionCloseAction.Rollback)
			{
				mgr.Rollback(this);
			}

			_parent.OnChildClosed(this, closeAction);

			mgr.OnTransactionOpenedOrClosed(this);
		}
		#endregion //Close

		#region StartTransaction
		private UndoTransaction StartTransaction(UndoExecuteContext executeInfo)
		{
			switch (executeInfo.Reason)
			{
				// if we're rolling back a transaction then we don't need 
				// one to wrap it
				case UndoExecuteReason.Rollback:
					return null;
				case UndoExecuteReason.Undo:
				case UndoExecuteReason.Redo:
					return executeInfo.UndoManager.StartTransaction(_description, _descriptionDetailed);
				default:
					Debug.Assert(false, "Unrecognized reason:" + executeInfo.Reason.ToString());
					return null;
			}
		} 
		#endregion //StartTransaction

		#region VerifyCanModify
		private void VerifyCanModify()
		{
			if (_isOpened == null)
				throw new InvalidOperationException(Utils.GetString("LE_TransactionNotStarted"));

			if (_isOpened == false)
				throw new InvalidOperationException(Utils.GetString("LE_TransactionClosed"));
		}
		#endregion //VerifyCanModify

		#endregion //Private Methods

		#region Internal Methods

		#region Add
		internal void Add(UndoUnit unit)
		{
			CoreUtilities.ValidateNotNull(unit, "unit");

			// can only add items to an open group
			this.VerifyCanModify();

			// make sure we don't have an open child
			if (_openTransaction != null)
				throw new InvalidOperationException(Utils.GetString("LE_AddUnitWhileOpenTransaction", _openTransaction));

			_unitsSource.Push(unit);
		}
		#endregion //Add

		#region Cancel
		internal void Cancel()
		{
			this.Close(UndoTransactionCloseAction.Cancel);
		} 
		#endregion //Cancel

		#region ContainsNonTransactionUnits
		internal bool ContainsNonTransactionUnits()
		{
			foreach (var unit in _unitsSource)
			{
				UndoTransaction transaction = unit as UndoTransaction;

				if (null != transaction)
				{
					if (transaction.ContainsNonTransactionUnits())
						return true;
				}
				else
				{
					return true;
				}
			}

			return false;
		}
		#endregion //ContainsNonTransactionUnits

		#region GetLeafOpenGroup
		internal static UndoTransaction GetLeafOpenGroup(UndoTransaction group)
		{
			CoreUtilities.ValidateNotNull(group, "group");

			if (group.OpenState != true)
				throw new ArgumentException(Utils.GetString("LE_TransactionNotOpened", group));

			while (group.OpenTransaction != null)
				group = group.OpenTransaction;

			return group;
		}
		#endregion //GetLeafOpenGroup

		#region Open
		/// <summary>
		/// Invoked when a group is about to be opened.
		/// </summary>
		/// <param name="parent">The parent for the group.</param>
		internal void Open(IUndoTransactionOwner parent)
		{
			CoreUtilities.ValidateNotNull(parent, "parent");

			if (_isOpened != null)
				throw new InvalidOperationException(Utils.GetString("LE_TransactionAlreadyOpened"));

			Debug.Assert(_parent == null, "Already have a parent?");

			// store the parent (at least temporarily
			_parent = parent;
			_isOpened = true;

			// assuming the parent is ok with us being opened we can initialize our state as such
			if (!parent.OnChildOpened(this))
			{
				// otherwise we should clear our reference to the parent
				_isOpened = null;
				_parent = null;
			}
			else
			{
				parent.UndoManager.OnTransactionOpenedOrClosed(this);
			}
		}
		#endregion //Open

		#endregion //Internal Methods

		#region Public Methods

		#region Commit
		/// <summary>
		/// Commits and closes the transaction
		/// </summary>
		public void Commit()
		{
			this.Close(UndoTransactionCloseAction.Commit);
		} 
		#endregion //Commit

		#region Rollback
		/// <summary>
		/// Closes the transaction, executes the <see cref="Units"/> and removes it from the parent.
		/// </summary>
		public void Rollback()
		{
			this.Close(UndoTransactionCloseAction.Rollback);
		} 
		#endregion //Rollback

		#endregion //Public Methods

		#endregion //Methods

		#region IUndoTransactionOwner Members

		UndoManager IUndoTransactionOwner.UndoManager
		{
			get { return _parent.UndoManager; }
		}

		bool IUndoTransactionOwner.OnChildOpened(UndoTransaction child)
		{
			CoreUtilities.ValidateNotNull(child, "child");

			// throw if this group is not open
			this.VerifyCanModify();

			if (_openTransaction != null)
				throw new InvalidOperationException(Utils.GetString("LE_HasOpenTransaction"));

			// must be one of our units
			if (!_unitsSource.Contains(child))
				throw new ArgumentException(Utils.GetString("LE_ChildTransactionNotInUnits", child));

			if (child.OpenState != true)
				throw new ArgumentException(Utils.GetString("LE_TransactionNotOpened", child));

			if (child.Owner != this)
				throw new ArgumentException(Utils.GetString("LE_InvalidTransactionOwner"));

			_openTransaction = child;
			return true;
		}

		void IUndoTransactionOwner.OnChildClosed(UndoTransaction child, UndoTransactionCloseAction closeAction)
		{
			CoreUtilities.ValidateNotNull(child, "child");

			// throw if this group is not open
			this.VerifyCanModify();

			if (child != _openTransaction)
				throw new ArgumentException(Utils.GetString("LE_ClosingOtherTransaction", child, _openTransaction));

			_openTransaction = null;

			// if it is being cancelled or was rolled back ... or if it was closed without
			// any child items then we can just remove the unit
			if (closeAction != UndoTransactionCloseAction.Commit || !child.ContainsNonTransactionUnits())
			{
				Debug.Assert(child == _unitsSource.Peek(), "Not the last item in the group?");
				_unitsSource.Pop();
			}
		}

		#endregion //IUndoTransactionOwner Members
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