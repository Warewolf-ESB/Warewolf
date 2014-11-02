
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections;
using System.Collections.Generic;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2ActionCollection : IActionCollection
    {
        private readonly ActionCollection _nativeInstance;
        private readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2ActionCollection(ITaskServiceConvertorFactory taskServiceConvertorFactory,
                                    ActionCollection nativeInstance)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _nativeInstance = nativeInstance;
        }

        public IAction Add(IAction action)
        {
            return _taskServiceConvertorFactory.CreateAction(Instance.Add(action.Instance));
        }

        public bool ContainsType(Type actionType)
        {
            return Instance.ContainsType(actionType);
        }


        public int Count
        {
            get { return Instance.Count; }
        }


        public IEnumerator<IAction> GetEnumerator()
        {
            IEnumerator<Microsoft.Win32.TaskScheduler.Action> en = Instance.GetEnumerator();
            while (en.MoveNext())
            {
                yield return _taskServiceConvertorFactory.CreateAction(en.Current);
            }
        }


        public void Dispose()
        {
            Instance.Dispose();
        }


        public ActionCollection Instance
        {
            get { return _nativeInstance; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
