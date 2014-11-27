
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Specs
{
    [TestClass]
    public class ContextForTests
    {
        public static string DeploymentDirectory;
        public static bool IsLocal = false;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext testCtx)
        {
            DeploymentDirectory = testCtx.DeploymentDirectory;
            IsLocal = testCtx.Properties["ControllerName"] == null || testCtx.Properties["ControllerName"].ToString() == "localhost:6901";
        }
    }
}
