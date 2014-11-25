
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;
using Dev2.Studio;

namespace Dev2.Core.Tests.Diagnostics
{
    public class MockApp : IApp
    {
        public virtual void Shutdown(){}
        public virtual bool ShouldRestart { get; set; }
        public Window MainWindow { get; set; }
    }
}
