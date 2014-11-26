
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.UndoFramework;

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

