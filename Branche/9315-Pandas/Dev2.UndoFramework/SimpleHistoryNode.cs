namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;
    using System.Runtime.CompilerServices;

    internal class SimpleHistoryNode
    {
        public SimpleHistoryNode()
        {
        }

        public SimpleHistoryNode(IAction lastExistingAction, SimpleHistoryNode lastExistingState)
        {
            this.PreviousAction = lastExistingAction;
            this.PreviousNode = lastExistingState;
        }

        public IAction NextAction { get; set; }

        public SimpleHistoryNode NextNode { get; set; }

        public IAction PreviousAction { get; set; }

        public SimpleHistoryNode PreviousNode { get; set; }
    }
}

