/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Controller
{
    public class ProcessController
    {
        private bool _isRunning;

        public Process UtilityProcess { get; private set; }

        public string CmdLine { get; set; }

        public string Arguments { get; set; }

        public bool ShowErrorDialog { get; set; }

        public bool UseShellExecute { get; set; }

        public string Verb { get; set; }

        public ProcessController()
            : this(null)
        {
        }

        public ProcessController(Process process)
        {
            UtilityProcess = process;
        }
    }
}
