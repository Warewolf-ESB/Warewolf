using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Sql;

namespace Dev2.Integration.Tests.Sql
{
    [TestClass]
    public class WorkflowsTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Warewolf.Sql.dll is NOT in bin directory so must be loaded manually!!
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if(args.Name == "Warewolf.Sql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31e03a70959ac5c4")
                {
                    var assemblyPath = Path.Combine(context.DeploymentDirectory, @"SqlResources\Warewolf\Warewolf.Sql.dll");
                    return Assembly.LoadFile(assemblyPath);
                }
                return null;
            };
        }

        [TestMethod]
        public void RunWorkflowIntegration()
        {
            var workflows = new Workflows();
            var result = workflows.RunWorkflow("http://localhost:1234/services/SampleEmployeesWorkflow?ResultType=Managers");
            Assert.IsNotNull(result);
        }

    }
}
