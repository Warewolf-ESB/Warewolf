namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class MultiAction : List<IAction>, IMultiAction, IAction, IList<IAction>, ICollection<IAction>, IEnumerable<IAction>, IEnumerable
    {
        public MultiAction()
        {
            this.IsDelayed = true;
        }

        public bool CanExecute()
        {
            foreach (IAction action in this)
            {
                if (!action.CanExecute())
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanUnExecute()
        {
            foreach (IAction action in this)
            {
                if (!action.CanUnExecute())
                {
                    return false;
                }
            }
            return true;
        }

        public void Execute()
        {
            if (!this.IsDelayed)
            {
                this.IsDelayed = true;
            }
            else
            {
                foreach (IAction action in this)
                {
                    action.Execute();
                }
            }
        }

        public bool TryToMerge(IAction FollowingAction)
        {
            return false;
        }

        public void UnExecute()
        {
            List<IAction> list = new List<IAction>(this);
            list.Reverse();
            foreach (IAction action in list)
            {
                action.UnExecute();
            }
        }

        public bool AllowToMergeWithPrevious { get; set; }

        public bool IsDelayed { get; set; }
    }
}

