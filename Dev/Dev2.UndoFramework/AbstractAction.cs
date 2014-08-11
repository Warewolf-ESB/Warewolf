namespace Dev2.UndoFramework
{

    public abstract class AbstractAction : IAction
    {
        private bool mAllowToMergeWithPrevious = true;

        public virtual bool CanExecute()
        {
            return (ExecuteCount == 0);
        }

        public virtual bool CanUnExecute()
        {
            return !CanExecute();
        }

        public virtual void Execute()
        {
            if(CanExecute())
            {
                ExecuteCore();
                ExecuteCount++;
            }
        }

        protected abstract void ExecuteCore();
        public virtual bool TryToMerge(IAction followingAction)
        {
            return false;
        }

        public virtual void UnExecute()
        {
            if(CanUnExecute())
            {
                UnExecuteCore();
                ExecuteCount--;
            }
        }

        protected abstract void UnExecuteCore();

        public bool AllowToMergeWithPrevious
        {
            get
            {
                return mAllowToMergeWithPrevious;
            }
            set
            {
                mAllowToMergeWithPrevious = value;
            }
        }

        protected int ExecuteCount { get; set; }
    }
}

