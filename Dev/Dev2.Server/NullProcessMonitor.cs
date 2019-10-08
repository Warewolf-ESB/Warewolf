/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Warewolf.OS;

namespace Dev2
{
    internal class NullProcessMonitor : IProcessMonitor
    {
#pragma warning disable CS0067
        public event ProcessDiedEvent OnProcessDied;
#pragma warning restore CS0067

        public void Shutdown()
        {
        }

        public void Start()
        {
        }
    }
}