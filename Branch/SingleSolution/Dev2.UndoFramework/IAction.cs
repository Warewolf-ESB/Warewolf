namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;

    public interface IAction
    {
        bool CanExecute();
        bool CanUnExecute();
        void Execute();
        bool TryToMerge(IAction followingAction);
        void UnExecute();

        bool AllowToMergeWithPrevious { get; set; }
    }
}

