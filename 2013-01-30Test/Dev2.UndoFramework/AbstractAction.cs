namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class AbstractAction : IAction
    {
        private bool mAllowToMergeWithPrevious = true;

        protected AbstractAction()
        {
        }

        public virtual bool CanExecute()
        {
            return (this.ExecuteCount == 0);
        }

        public virtual bool CanUnExecute()
        {
            return !this.CanExecute();
        }

        public virtual void Execute()
        {
            if (this.CanExecute())
            {
                this.ExecuteCore();
                this.ExecuteCount++;
            }
        }

        protected abstract void ExecuteCore();
        public virtual bool TryToMerge(IAction followingAction)
        {
            return false;
        }

        public virtual void UnExecute()
        {
            if (this.CanUnExecute())
            {
                this.UnExecuteCore();
                this.ExecuteCount--;
            }
        }

        protected abstract void UnExecuteCore();

        public bool AllowToMergeWithPrevious
        {
            get
            {
                return this.mAllowToMergeWithPrevious;
            }
            set
            {
                this.mAllowToMergeWithPrevious = value;
            }
        }

        protected int ExecuteCount { get; set; }
    }
}

