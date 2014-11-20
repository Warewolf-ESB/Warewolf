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

namespace Dev2.TaskScheduler.Wrappers.Interfaces
{
    public class Dev2ExecAction : Dev2Action, IExecAction
    {
        public Dev2ExecAction(ITaskServiceConvertorFactory taskServiceConvertorFactory, ExecAction nativeTnstance)
            : base(nativeTnstance)
        {
        }

        public new ExecAction Instance
        {
            get { return (ExecAction) base.Instance; }
        }

        public string Path
        {
            get { return Instance.Path; }
            set { Instance.Path = value; }
        }

        public string Arguments
        {
            get { return Instance.Arguments; }
            set { Instance.Arguments = value; }
        }

        public string WorkingDirectory
        {
            get { return Instance.WorkingDirectory; }
            set { Instance.WorkingDirectory = value; }
        }
    }
}