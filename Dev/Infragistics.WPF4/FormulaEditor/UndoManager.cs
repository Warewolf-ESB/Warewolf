using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Diagnostics;

namespace Infragistics.Controls.Interactions
{
	internal class UndoManager<T> where T : UndoState
	{
		#region Member Variables

		private static readonly TimeSpan CommitChangesDelay = TimeSpan.FromSeconds(1);

		private DispatcherTimer _delayedCommitChangesTimer;
		private IUndoManagerOwner<T> _owner;
		private bool _hasPendingDelayedChanges;
		private T _previousState;
		private Stack<T> _redoStack = new Stack<T>();
		private int _suspendCount;
		private Stack<T> _undoStack = new Stack<T>();

		#endregion  // Member Variables

		#region Constructor

		public UndoManager(IUndoManagerOwner<T> owner)
		{
			_owner = owner;
			_previousState = _owner.GetCurrentState();
		}

		#endregion  // Constructor

		#region Methods

		#region Public Methods

		#region ClearHistory

		public void ClearHistory()
		{
			this.ClearUndoStack();
			this.ClearRedoStack();
		}

		#endregion  // ClearHistory

		#region OnFormulaChanged

		public void OnStateChanged(bool shouldCommitChange = true)
		{
			if (this.IsSuspended)
				return;

			// If we shouldn't commit the change, just update the _previousState member to the current state of the owner.
			// But if a change is pending, commit that first, then update the _previousState member.
			if (shouldCommitChange == false)
			{
				T state = _owner.GetCurrentState();

				if (_hasPendingDelayedChanges)
					this.CommitChanges(state);

				_previousState = state;
				return;
			}

			// When a change occurs, immediately clear the redo stack.
			this.ClearRedoStack();

			T currentState = _owner.GetCurrentState();
			if (currentState.ShouldBeCommittedSynchronously(_previousState))
			{
				this.CommitChanges(currentState);
				return;
			}

			// Otherwise, we can do a delayed commit to the undo stack.
			if (_hasPendingDelayedChanges == false)
			{
				_hasPendingDelayedChanges = true;

				// Even though there may be nothing currently on the undo stack, there will be shortly.
				_owner.SetCanUndo(true);
				this.DelayedCommitChangesTimer.Start();
			}
		}

		#endregion  // OnFormulaChanged

		#region Redo

		public bool Redo()
		{
			if (_redoStack.Count == 0)
				return false;

			Debug.Assert(_hasPendingDelayedChanges == false, "There should be no pending changes when a Redo occurs.");

			_undoStack.Push(_previousState);
			_previousState = _redoStack.Pop();
			this.ApplyState(_previousState);

			return true;
		}

		#endregion  // Redo

		// MD 11/3/11 - TFS95198
		#region Reinitialize

		public void Reinitialize()
		{
			this.ClearHistory();
			_previousState = _owner.GetCurrentState();
		}

		#endregion  // Reinitialize

		#region Resume

		public void Resume()
		{
			this.Resume(true);
		}

		private void Resume(bool commitChangesOnResume)
		{
			_suspendCount--;
			Debug.Assert(_suspendCount >= 0, "We resumed too many times.");

			if (_suspendCount == 0 && commitChangesOnResume)
				this.CommitChanges();
		}

		#endregion  // Resume

		#region Suspend

		public void Suspend()
		{
			_suspendCount++;
		}

		#endregion  // Suspend

		#region Undo

		public bool Undo()
		{
			if (_hasPendingDelayedChanges)
				this.CommitChanges();

			if (_undoStack.Count == 0)
				return false;

			_redoStack.Push(_previousState);
			_previousState = _undoStack.Pop();
			this.ApplyState(_previousState);

			return true;
		}

		#endregion  // Undo

		#endregion  // Public Methods

		#region Private Methods

		#region ApplyState

		private void ApplyState(T state)
		{
			this.Suspend();

			try
			{
				_owner.SetCurrentState(state);
				this.UpdateDialogState();
			}
			finally
			{
				this.Resume(false);
			}
		}

		#endregion  // ApplyState

		#region ClearRedoStack

		private void ClearRedoStack()
		{
			_redoStack.Clear();
			_owner.SetCanRedo(false);
		}

		#endregion  // ClearRedoStack

		#region ClearUndoStack

		private void ClearUndoStack()
		{
			_undoStack.Clear();
			_owner.SetCanUndo(false);
		}

		#endregion  // ClearUndoStack

		#region CommitChanges

		private void CommitChanges(T currentState = null)
		{
			if (currentState == null)
				currentState = _owner.GetCurrentState();

			_hasPendingDelayedChanges = false;

			if (_delayedCommitChangesTimer != null)
				_delayedCommitChangesTimer.Stop();

			// Push previous state on the undo stack if the editor has changed.
			if (currentState.Equals(_previousState) == false)
			{
				_undoStack.Push(_previousState);
				_owner.SetCanUndo(true);

				this.ClearRedoStack();

				_previousState = currentState;
			}
		}

		#endregion  // CommitChanges

		#region OnDelayedCommitChangesTimerTick

		private void OnDelayedCommitChangesTimerTick(object sender, EventArgs e)
		{
			this.CommitChanges();
		}

		#endregion  // OnDelayedCommitChangesTimerTick

		#region UpdateDialogState

		private void UpdateDialogState()
		{
			_owner.SetCanUndo(_undoStack.Count != 0);
			_owner.SetCanRedo(_redoStack.Count != 0);
		}

		#endregion  // UpdateDialogState

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region DelayedCommitChangesTimer

		private DispatcherTimer DelayedCommitChangesTimer
		{
			get
			{
				if (_delayedCommitChangesTimer == null)
				{
					_delayedCommitChangesTimer = new DispatcherTimer();
					_delayedCommitChangesTimer.Interval = CommitChangesDelay;
					_delayedCommitChangesTimer.Tick += this.OnDelayedCommitChangesTimerTick;
				}

				return _delayedCommitChangesTimer;
			}
		}

		#endregion  // DelayedCommitChangesTimer

		#region IsSuspended

		private bool IsSuspended
		{
			get { return _suspendCount > 0; }
		}

		#endregion  // IsSuspended

		#endregion  // Properties
	}

	internal interface IUndoManagerOwner<T> where T : UndoState
	{
		T GetCurrentState();
		void SetCanRedo(bool canRedo);
		void SetCanUndo(bool canUndo);
		void SetCurrentState(T currentState);
	}

	internal abstract class UndoState
	{
		public abstract override int GetHashCode();
		public abstract override bool Equals(object obj);
		public abstract bool ShouldBeCommittedSynchronously(UndoState previousState);
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