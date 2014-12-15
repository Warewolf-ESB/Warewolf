
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
using System.Collections;
using System.Collections.Generic;
using Dev2.Common.Interfaces.UndoFramework;

namespace Dev2.UndoFramework

{
    internal class SimpleHistory : IActionHistory
    {
        private SimpleHistoryNode _mCurrentState = new SimpleHistoryNode();

        public event EventHandler CollectionChanged;

        public SimpleHistory()
        {
            Init();
        }

        public bool AppendAction(IAction newAction)
        {
            if(CurrentState.PreviousAction == null)
            {
                CurrentState.NextAction = newAction;
                CurrentState.NextNode = new SimpleHistoryNode(newAction, CurrentState);
            }
            else
            {
                if(CurrentState.PreviousAction.TryToMerge(newAction))
                {
                    RaiseUndoBufferChanged();
                    return false;
                }
                CurrentState.NextAction = newAction;
                CurrentState.NextNode = new SimpleHistoryNode(newAction, CurrentState);
            }
            return true;
        }

        public void Clear()
        {
            Init();
            RaiseUndoBufferChanged();
        }

        public IEnumerable<IAction> EnumUndoableActions()
        {
            SimpleHistoryNode head = Head;
            while(true)
            {
                if(((head == null) || (head == CurrentState)) || (head.NextAction == null))
                {
                    yield break;
                }
                yield return head.NextAction;
                head = head.NextNode;
            }
        }

        public IEnumerator<IAction> GetEnumerator()
        {
            return EnumUndoableActions().GetEnumerator();
        }

        private void Init()
        {
            CurrentState = new SimpleHistoryNode();
            Head = CurrentState;
        }

        public void MoveBack()
        {
            if(!CanMoveBack)
            {
                throw new InvalidOperationException("History.MoveBack() cannot execute because CanMoveBack returned false (the current state is the last state in the undo buffer.");
            }
            CurrentState.PreviousAction.UnExecute();
            CurrentState = CurrentState.PreviousNode;
            Length--;
            RaiseUndoBufferChanged();
        }

        public void MoveForward()
        {
            if(!CanMoveForward)
            {
                throw new InvalidOperationException("History.MoveForward() cannot execute because CanMoveForward returned false (the current state is the last state in the undo buffer.");
            }
            CurrentState.NextAction.Execute();
            CurrentState = CurrentState.NextNode;
            Length++;
            RaiseUndoBufferChanged();
        }

        protected void RaiseUndoBufferChanged()
        {
            if(CollectionChanged != null)
            {
                CollectionChanged(this, new EventArgs());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool CanMoveBack
        {
            get
            {
                return ((CurrentState.PreviousAction != null) && (CurrentState.PreviousNode != null));
            }
        }

        public bool CanMoveForward
        {
            get
            {
                return ((CurrentState.NextAction != null) && (CurrentState.NextNode != null));
            }
        }

        public SimpleHistoryNode CurrentState
        {
            get
            {
                return _mCurrentState;
            }
            set
            {
                if(value == null)
                {
                    // ReSharper disable NotResolvedInText
                    throw new ArgumentNullException("CurrentState");
                    // ReSharper restore NotResolvedInText
                }
                _mCurrentState = value;
            }
        }

        public SimpleHistoryNode Head { get; set; }

        public IAction LastAction { get; set; }

        public int Length { get; set; }

    }
}

