/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;
using System;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2Action : IAction
    {
        private readonly Microsoft.Win32.TaskScheduler.Action _nativeTnstance;

        public Dev2Action(Microsoft.Win32.TaskScheduler.Action nativeTnstance)
        {
            _nativeTnstance = nativeTnstance;
        }

        public TaskActionType ActionType => _nativeTnstance.ActionType;

        public string Id
        {
            get { return Instance.Id; }
            set { Instance.Id = value; }
        }

        public void Dispose()
        {
            Instance.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        public Microsoft.Win32.TaskScheduler.Action Instance => _nativeTnstance;
    }
}
