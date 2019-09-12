/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;

namespace Warewolf.OS
{
    public class ProcessWrapper : IProcess
    {
        private readonly Process _process;

        public int Id => _process.Id;
        public bool HasExited
        {
            get
            {
                try
                {
                    return _process.HasExited;
                } catch (InvalidOperationException)
                {
                    return true;
                }
            }
        }

        public ProcessWrapper(Process process)
        {
            _process = process;
        }

        public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

        public void Dispose()
        {
            _process.Dispose();
        }

        public void Kill()
        {
            _process.Kill();
        }

        public Process Unwrap()
        {
            return _process;
        }
    }
    public class ProcessWrapperFactory : IProcessFactory
    { 
        public IProcess Start(string fileName)
        {
            return new ProcessWrapper(Process.Start(fileName));
        }
        public IProcess Start(ProcessStartInfo startInfo)
        {
            return new ProcessWrapper(Process.Start(startInfo));
        }
    }
}
