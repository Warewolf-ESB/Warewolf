using System;
using System.Collections.Generic;

namespace Dev2.UndoFramework
{
    internal interface IActionHistory : IEnumerable<IAction>
    {
        event EventHandler CollectionChanged;

        bool AppendAction(IAction newAction);
        void Clear();
        IEnumerable<IAction> EnumUndoableActions();
        void MoveBack();
        void MoveForward();

        bool CanMoveBack { get; }

        bool CanMoveForward { get; }

        SimpleHistoryNode CurrentState { get; }

        int Length { get; }
    }
}

