
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.UndoFramework;

namespace Dev2.UndoFramework
{
    public class ActionManager
    {
        private IActionHistory _mHistory;
        private Stack<ITransaction> _mTransactionStack = new Stack<ITransaction>();
        private readonly object _recordActionLock = new object();

        public event EventHandler CollectionChanged;

        public ActionManager()
        {
            History = new SimpleHistory();
        }

        private void CheckNotRunningBeforeRecording(IAction existingAction)
        {
            string str = (existingAction != null) ? existingAction.ToString() : "";
            if (CurrentAction != null)
            {
                throw new InvalidOperationException(string.Format("ActionManager.RecordActionDirectly: the ActionManager is currently running or undoing an action ({0}), and this action (while being executed) attempted to recursively record another action ({1}), which is not allowed. You can examine the stack trace of this exception to see what the executing action did wrong and change this action not to influence the Undo stack during its execution. Checking if ActionManager.ActionIsExecuting == true before launching another transaction might help to avoid the problem. Thanks and sorry for the inconvenience.", CurrentAction, str));
            }
        }

        internal void Clear()
        {
            History.Clear();
            CurrentAction = null;
        }

        internal void CommitTransaction()
        {
            if (TransactionStack.Count == 0)
            {
                throw new InvalidOperationException("ActionManager.CommitTransaction was called when there is no open transaction (TransactionStack is empty). Please examine the stack trace of this exception to find code which called CommitTransaction one time too many. Normally you don't call OpenTransaction and CommitTransaction directly, but use using(var t = Transaction.Create(Root)) instead.");
            }
            ITransaction transaction = TransactionStack.Pop();
            if (transaction.AccumulatingAction.Count > 0)
            {
                RecordAction(transaction.AccumulatingAction);
            }
        }

        internal Transaction CreateTransaction()
        {
            return Transaction.Create(this);
        }

        internal Transaction CreateTransaction(bool delayed)
        {
            return Transaction.Create(this, delayed);
        }

        internal IEnumerable<IAction> EnumUndoableActions()
        {
            return History.EnumUndoableActions();
        }

        internal void OpenTransaction(ITransaction t)
        {
            TransactionStack.Push(t);
        }

        protected void RaiseUndoBufferChanged(object sender, EventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        public void RecordAction(IAction existingAction)
        {
            if (existingAction != null)
            {
                CheckNotRunningBeforeRecording(existingAction);
                if (ExecuteImmediatelyWithoutRecording && existingAction.CanExecute())
                {
                    existingAction.Execute();
                }
                else
                {
                    ITransaction recordingTransaction = RecordingTransaction;
                    if (recordingTransaction != null)
                    {
                        recordingTransaction.AccumulatingAction.Add(existingAction);
                        if (!recordingTransaction.IsDelayed)
                        {
                            existingAction.Execute();
                        }
                    }
                    else
                    {
                        RunActionDirectly(existingAction);
                    }
                }
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                //20.09.2012: massimo.guerrera - Changed not to throw an exception

                if (!ActionIsExecuting)
                {
                    CurrentAction = History.CurrentState.NextAction;
                    if (CurrentAction != null)
                    {
                        History.MoveForward();
                        CurrentAction = null;
                    }
                }
            }
        }

        internal void RollBackTransaction()
        {
            if (TransactionStack.Count != 0)
            {
                ITransaction transaction = TransactionStack.Peek();
                if ((transaction != null) && (transaction.AccumulatingAction != null))
                {
                    transaction.AccumulatingAction.UnExecute();
                }
                TransactionStack.Clear();
            }
        }

        private void RunActionDirectly(IAction actionToRun)
        {
            CheckNotRunningBeforeRecording(actionToRun);
            lock (_recordActionLock)
            {
                CurrentAction = actionToRun;
                if (History.AppendAction(actionToRun))
                {
                    History.MoveForward();
                }
                CurrentAction = null;
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                //20.09.2012: massimo.guerrera - Changed not to throw an exception

                if (!ActionIsExecuting)
                {
                    CurrentAction = History.CurrentState.PreviousAction;
                    if (CurrentAction != null)
                    {
                        History.MoveBack();
                        CurrentAction = null;
                    }
                }
            }
        }

        public bool ActionIsExecuting
        {
            get
            {
                return (CurrentAction != null);
            }
        }

        public bool CanRedo
        {
            get
            {
                return History.CanMoveForward;
            }
        }

        public bool CanUndo
        {
            get
            {
                return History.CanMoveBack;
            }
        }

        public IAction CurrentAction { get; internal set; }

        public bool ExecuteImmediatelyWithoutRecording { get; set; }

        internal IActionHistory History
        {
            get
            {
                return _mHistory;
            }
            set
            {
                if (_mHistory != null)
                {
                    _mHistory.CollectionChanged -= RaiseUndoBufferChanged;
                }
                _mHistory = value;
                if (_mHistory != null)
                {
                    _mHistory.CollectionChanged += RaiseUndoBufferChanged;
                }
            }
        }

        internal ITransaction RecordingTransaction
        {
            get
            {
                if (TransactionStack.Count > 0)
                {
                    return TransactionStack.Peek();
                }
                return null;
            }
        }

        internal Stack<ITransaction> TransactionStack
        {
            get
            {
                return _mTransactionStack;
            }
            set
            {
                _mTransactionStack = value;
            }
        }
    }
}

