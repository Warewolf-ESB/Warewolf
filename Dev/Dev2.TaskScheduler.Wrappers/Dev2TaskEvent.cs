/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TaskEvent : ITaskEvent
    {
        private readonly TaskEvent _nativeObject;

        public Dev2TaskEvent(TaskEvent nativeObject)
        {
            _nativeObject = nativeObject;
        }

        Guid? ITaskEvent.ActivityId => _nativeObject.ActivityId;

        int ITaskEvent.EventId => _nativeObject.EventId;

        string ITaskEvent.TaskCategory => _nativeObject.TaskCategory;

        DateTime? ITaskEvent.TimeCreated => _nativeObject.TimeCreated;

        string ITaskEvent.Correlation
        {
            get
            {
                if(_nativeObject.ActivityId != null)
                    return _nativeObject.ActivityId.Value.ToString();
                return "";
            }
        }

        public string UserId => _nativeObject.UserId.Value;
    }
}
