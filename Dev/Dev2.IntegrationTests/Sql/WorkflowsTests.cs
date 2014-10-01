
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Sql
{
    [TestClass]
    public class WorkflowsTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {

            /*
             *    It was once upon a time, then someone placed a post build event to delete the dll, I removed this and all is good ;)
             */

            // Warewolf.Sql.dll is NOT in bin directory so must be loaded manually!!
            //AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    if(args.Name.IndexOf("PublicKeyToken=31e03a70959ac5c4") >= 0)
            //    {
            //        var assemblyPath = Path.Combine(context.DeploymentDirectory, @"SqlResources\Warewolf\Warewolf.Sql.dll");
            //        return Assembly.LoadFile(assemblyPath);
            //    }
            //    return null;
            //};
        }

        [TestMethod]
        public void RunWorkflowIntegration()
        {
            string reponseData = TestHelper.PostDataToWebserver(string.Format("{0}{1}", ServerSettings.WebserverURI, "SampleEmployeesWorkflow?ResultType=Managers"));
            Assert.IsNotNull(reponseData);
        }

    }
}
