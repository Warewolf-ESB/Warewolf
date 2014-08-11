namespace Dev2.UndoFramework
{
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

