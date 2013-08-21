namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class ActionManager
    {
        private IActionHistory mHistory;
        private Stack<ITransaction> mTransactionStack = new Stack<ITransaction>();
        private object recordActionLock = new object();

        public event EventHandler CollectionChanged;

        public ActionManager()
        {
            this.History = new SimpleHistory();
        }

        private void CheckNotRunningBeforeRecording(IAction existingAction)
        {
            string str = (existingAction != null) ? existingAction.ToString() : "";
            if (this.CurrentAction != null)
            {
                throw new InvalidOperationException(string.Format("ActionManager.RecordActionDirectly: the ActionManager is currently running or undoing an action ({0}), and this action (while being executed) attempted to recursively record another action ({1}), which is not allowed. You can examine the stack trace of this exception to see what the executing action did wrong and change this action not to influence the Undo stack during its execution. Checking if ActionManager.ActionIsExecuting == true before launching another transaction might help to avoid the problem. Thanks and sorry for the inconvenience.", this.CurrentAction.ToString(), str));
            }
        }

        internal void Clear()
        {
            this.History.Clear();
            this.CurrentAction = null;
        }

        internal void CommitTransaction()
        {
            if (this.TransactionStack.Count == 0)
            {
                throw new InvalidOperationException("ActionManager.CommitTransaction was called when there is no open transaction (TransactionStack is empty). Please examine the stack trace of this exception to find code which called CommitTransaction one time too many. Normally you don't call OpenTransaction and CommitTransaction directly, but use using(var t = Transaction.Create(Root)) instead.");
            }
            ITransaction transaction = this.TransactionStack.Pop();
            if (transaction.AccumulatingAction.Count > 0)
            {
                this.RecordAction(transaction.AccumulatingAction);
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
            return this.History.EnumUndoableActions();
        }

        internal void OpenTransaction(ITransaction t)
        {
            this.TransactionStack.Push(t);
        }

        protected void RaiseUndoBufferChanged(object sender, EventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, e);
            }
        }

        public void RecordAction(IAction existingAction)
        {
            if (existingAction != null)
            {
                this.CheckNotRunningBeforeRecording(existingAction);
                if (this.ExecuteImmediatelyWithoutRecording && existingAction.CanExecute())
                {
                    existingAction.Execute();
                }
                else
                {
                    ITransaction recordingTransaction = this.RecordingTransaction;
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
                        this.RunActionDirectly(existingAction);
                    }
                }
            }
        }

        public void Redo()
        {
            if (this.CanRedo)
            {
                //20.09.2012: massimo.guerrera - Changed not to throw an exception

                if (!this.ActionIsExecuting)
                {
                    this.CurrentAction = this.History.CurrentState.NextAction;
                    if (this.CurrentAction != null)
                    {
                        this.History.MoveForward();
                        this.CurrentAction = null;
                    }
                }
            }
        }

        internal void RollBackTransaction()
        {
            if (this.TransactionStack.Count != 0)
            {
                ITransaction transaction = this.TransactionStack.Peek();
                if ((transaction != null) && (transaction.AccumulatingAction != null))
                {
                    transaction.AccumulatingAction.UnExecute();
                }
                this.TransactionStack.Clear();
            }
        }

        private void RunActionDirectly(IAction actionToRun)
        {
            this.CheckNotRunningBeforeRecording(actionToRun);
            lock (this.recordActionLock)
            {
                this.CurrentAction = actionToRun;
                if (this.History.AppendAction(actionToRun))
                {
                    this.History.MoveForward();
                }
                this.CurrentAction = null;
            }
        }

        public void Undo()
        {
            if (this.CanUndo)
            {
                //20.09.2012: massimo.guerrera - Changed not to throw an exception

                if (!this.ActionIsExecuting)
                {
                    this.CurrentAction = this.History.CurrentState.PreviousAction;
                    if (this.CurrentAction != null)
                    {
                        this.History.MoveBack();
                        this.CurrentAction = null;
                    }
                }
            }
        }

        public bool ActionIsExecuting
        {
            get
            {
                return (this.CurrentAction != null);
            }
        }

        public bool CanRedo
        {
            get
            {
                return this.History.CanMoveForward;
            }
        }

        public bool CanUndo
        {
            get
            {
                return this.History.CanMoveBack;
            }
        }

        public IAction CurrentAction { get; internal set; }

        public bool ExecuteImmediatelyWithoutRecording { get; set; }

        internal IActionHistory History
        {
            get
            {
                return this.mHistory;
            }
            set
            {
                if (this.mHistory != null)
                {
                    this.mHistory.CollectionChanged -= new EventHandler(this.RaiseUndoBufferChanged);
                }
                this.mHistory = value;
                if (this.mHistory != null)
                {
                    this.mHistory.CollectionChanged += new EventHandler(this.RaiseUndoBufferChanged);
                }
            }
        }

        internal ITransaction RecordingTransaction
        {
            get
            {
                if (this.TransactionStack.Count > 0)
                {
                    return this.TransactionStack.Peek();
                }
                return null;
            }
        }

        internal Stack<ITransaction> TransactionStack
        {
            get
            {
                return this.mTransactionStack;
            }
            set
            {
                this.mTransactionStack = value;
            }
        }
    }
}

