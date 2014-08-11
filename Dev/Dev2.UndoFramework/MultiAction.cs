using System.Linq;
using System.Collections.Generic;

namespace Dev2.UndoFramework
{
    internal class MultiAction : List<IAction>, IMultiAction
    {
        public MultiAction()
        {
            IsDelayed = true;
        }

        public bool CanExecute()
        {
            return this.All(action => action.CanExecute());
        }

        public bool CanUnExecute()
        {
            return this.All(action => action.CanUnExecute());
        }

        public void Execute()
        {
            if(!IsDelayed)
            {
                IsDelayed = true;
            }
            else
            {
                foreach(IAction action in this)
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
            foreach(IAction action in list)
            {
                action.UnExecute();
            }
        }

        public bool AllowToMergeWithPrevious { get; set; }

        public bool IsDelayed { get; set; }
    }
}

