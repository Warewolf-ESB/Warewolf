
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.UndoFramework;

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

