/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2Action : IAction
    {
        private readonly Action _nativeTnstance;

        public Dev2Action(Action nativeTnstance)
        {
            _nativeTnstance = nativeTnstance;
        }

        public TaskActionType ActionType
        {
            get { return _nativeTnstance.ActionType; }
        }

        public string Id
        {
            get { return Instance.Id; }
            set { Instance.Id = value; }
        }

        public void Dispose()
        {
            Instance.Dispose();
        }


        public Action Instance
        {
            get { return _nativeTnstance; }
        }
    }
}