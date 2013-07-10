namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class SimpleHistory : IActionHistory, IEnumerable<IAction>, IEnumerable
    {
        private SimpleHistoryNode mCurrentState = new SimpleHistoryNode();

        public event EventHandler CollectionChanged;

        public SimpleHistory()
        {
            this.Init();
        }

        public bool AppendAction(IAction newAction)
        {
            if (this.CurrentState.PreviousAction == null)
            {
                this.CurrentState.NextAction = newAction;
                this.CurrentState.NextNode = new SimpleHistoryNode(newAction, this.CurrentState);
            }
            else
            {
                if (this.CurrentState.PreviousAction.TryToMerge(newAction))
                {
                    this.RaiseUndoBufferChanged();
                    return false;
                }
                this.CurrentState.NextAction = newAction;
                this.CurrentState.NextNode = new SimpleHistoryNode(newAction, this.CurrentState);
            }
            return true;
        }

        public void Clear()
        {
            this.Init();
            this.RaiseUndoBufferChanged();
        }

        public IEnumerable<IAction> EnumUndoableActions()
        {
            SimpleHistoryNode head = this.Head;
            while (true)
            {
                if (((head == null) || (head == this.CurrentState)) || (head.NextAction == null))
                {
                    yield break;
                }
                yield return head.NextAction;
                head = head.NextNode;
            }
        }

        public IEnumerator<IAction> GetEnumerator()
        {
            return this.EnumUndoableActions().GetEnumerator();
        }

        private void Init()
        {
            this.CurrentState = new SimpleHistoryNode();
            this.Head = this.CurrentState;
        }

        public void MoveBack()
        {
            if (!this.CanMoveBack)
            {
                throw new InvalidOperationException("History.MoveBack() cannot execute because CanMoveBack returned false (the current state is the last state in the undo buffer.");
            }
            this.CurrentState.PreviousAction.UnExecute();
            this.CurrentState = this.CurrentState.PreviousNode;
            this.Length--;
            this.RaiseUndoBufferChanged();
        }

        public void MoveForward()
        {
            if (!this.CanMoveForward)
            {
                throw new InvalidOperationException("History.MoveForward() cannot execute because CanMoveForward returned false (the current state is the last state in the undo buffer.");
            }
            this.CurrentState.NextAction.Execute();
            this.CurrentState = this.CurrentState.NextNode;
            this.Length++;
            this.RaiseUndoBufferChanged();
        }

        protected void RaiseUndoBufferChanged()
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new EventArgs());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool CanMoveBack
        {
            get
            {
                return ((this.CurrentState.PreviousAction != null) && (this.CurrentState.PreviousNode != null));
            }
        }

        public bool CanMoveForward
        {
            get
            {
                return ((this.CurrentState.NextAction != null) && (this.CurrentState.NextNode != null));
            }
        }

        public SimpleHistoryNode CurrentState
        {
            get
            {
                return this.mCurrentState;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("CurrentState");
                }
                this.mCurrentState = value;
            }
        }

        public SimpleHistoryNode Head { get; set; }

        public IAction LastAction { get; set; }

        public int Length { get; set; }

    }
}

