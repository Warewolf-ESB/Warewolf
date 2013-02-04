using System;
using System.IO;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Reflection;
using Dev2.Runtime;
using Dev2.Integration.Tests.Dev2.Application.Server.Tests.Workspace.XML;

namespace Dev2.Integration.Tests.Runtime.Tests
{
    /// <summary>
    /// Summary description for WorkspaceRespositoryTest
    /// </summary>
    [TestClass]
    public class WorkspaceRespositoryTest
    {
        static string _servicesPath;
        static WorkspaceRepository _testInstance;


        static object l = new object();

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.SetCurrentDirectory(testContext.TestDir);

            #region Copy server services to file system

            _servicesPath = Path.Combine(testContext.TestDir, "Services");
            
            Directory.CreateDirectory(_servicesPath);
            var xml = XmlResource.Fetch("Calculate_RecordSet_Subtract.xml");
            xml.Save(Path.Combine(_servicesPath, "Calculate_RecordSet_Subtract.xml"));

            #endregion

            _testInstance = WorkspaceRepository.Instance;
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {
            if (Directory.Exists(_testInstance.RepositoryPath))
            {
                Directory.Delete(_testInstance.RepositoryPath, true);
            }
            if (Directory.Exists(_servicesPath))
            {
                Directory.Delete(_servicesPath, true);
            }
        }



        


    }
}
