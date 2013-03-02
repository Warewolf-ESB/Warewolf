using Dev2.Common;
using Dev2.Common.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Xml;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class ResourcesTests
    {

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.SetCurrentDirectory(testContext.TestDir);
        }

        [TestMethod]
        public void SaveExists_Expected_ResourceSaved()
        {
            //Initialization
            var threadSave = Guid.NewGuid();
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + threadSave);
            StreamWriter testResource = File.CreateText(Directory.GetCurrentDirectory() + @"\" + threadSave + @"\testResource.xml");
            testResource.Write("<?xml version=\"1.0\"?><Root>Originally the root node looks like this</Root>");
            testResource.Close();
            //Test
            string directoryName = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\" + threadSave).Name;
            string workspacePath = Directory.GetParent(Directory.GetCurrentDirectory() + @"\" + threadSave).ToString();
            Dev2.Runtime.ServiceModel.Resources.Save(workspacePath, directoryName, "testResource",
                           "<?xml version=\"1.0\"?><Root>This is the text of the root element</Root>");
            Assert.IsTrue(File.ReadAllText(Directory.GetCurrentDirectory() + @"\" + threadSave + @"\testResource.xml").Contains("This is the text of the root element"));
            //Cleanup
            if(Directory.Exists(Directory.GetCurrentDirectory() + @"\" + threadSave)) DeleteDirectory(Directory.GetCurrentDirectory() + @"\" + threadSave);
        }

        [TestMethod]
        public void SaveExistsAndReadonly_Expected_ResourceSaved()
        {
            //Initialization
            var threadSave = Guid.NewGuid();
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + threadSave);
            string path = Directory.GetCurrentDirectory() + @"\" + threadSave + @"\testResource.xml";
            StreamWriter testResource = File.CreateText(path);
            testResource.Write("<?xml version=\"1.0\"?><Root>Originally the root node looks like this</Root>");
            testResource.Close();

            FileAttributes attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
            {
                File.SetAttributes(path, attributes ^ FileAttributes.ReadOnly);
            }

            //Test
            string directoryName = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\" + threadSave).Name;
            string workspacePath = Directory.GetParent(Directory.GetCurrentDirectory() + @"\" + threadSave).ToString();
            Dev2.Runtime.ServiceModel.Resources.Save(workspacePath, directoryName, "testResource",
                           "<?xml version=\"1.0\"?><Root>This is the text of the root element</Root>");
            Assert.IsTrue(File.ReadAllText(Directory.GetCurrentDirectory() + @"\" + threadSave + @"\testResource.xml").Contains("This is the text of the root element"));
            //Cleanup
            if (Directory.Exists(Directory.GetCurrentDirectory() + @"\" + threadSave)) DeleteDirectory(Directory.GetCurrentDirectory() + @"\" + threadSave);
        }

        [TestMethod]
        public void SaveNotExists_Expected_ResourceSaved()
        {
            //Initialization
            var threadSave = Guid.NewGuid();
            if(File.Exists(Directory.GetCurrentDirectory() + @"\" + threadSave + @"\testResource.xml")) File.Delete(Directory.GetCurrentDirectory() + @"\" + threadSave + @"\testResource.xml");
            //Test
            string directoryName = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\" + threadSave).Name;
            string workspacePath = Directory.GetParent(Directory.GetCurrentDirectory() + @"\" + threadSave).ToString();
            Dev2.Runtime.ServiceModel.Resources.Save(workspacePath, directoryName, "testResource",
                   "<?xml version=\"1.0\"?><Root>This is the text of the root element</Root>");
            Assert.IsTrue(File.ReadAllText(Directory.GetCurrentDirectory() + @"\" + threadSave + @"\testResource.xml").Contains("This is the text of the root element"));
            //Cleanup
            if(Directory.Exists(Directory.GetCurrentDirectory() + @"\" + threadSave)) DeleteDirectory(Directory.GetCurrentDirectory() + @"\" + threadSave);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void SaveInvalidXml_Expected_XmlException()
        {
            var threadSave = Guid.NewGuid();
            string directoryName = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\" + threadSave).Name;
            string workspacePath = Directory.GetParent(Directory.GetCurrentDirectory() + @"\" + threadSave).ToString();
            Dev2.Runtime.ServiceModel.Resources.Save(workspacePath, directoryName, "testResource",
                   "<?xml version=\"1.0\"?>This is the text of the root element");
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void SaveSlashesInDirectory_Expected_DirectoryNotFoundException()
        {
            var threadSave = Guid.NewGuid();
            string directoryName = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\" + threadSave).Name;
            string workspacePath = Directory.GetParent(Directory.GetCurrentDirectory() + @"\" + threadSave).ToString();
            Dev2.Runtime.ServiceModel.Resources.Save(workspacePath, directoryName, "test/Resource",
                   "<?xml version=\"1.0\"?><Root>This is the text of the root element</Root>");
        }

        [TestMethod]
        public void Paths_Expected_JSONSources()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            var servicesPath = Path.Combine(workspacePath, "Services");
            var sourcesPath = Path.Combine(workspacePath, "Sources");
            var pluginsPath = Path.Combine(workspacePath, "Plugins");
            try
            {
                Directory.CreateDirectory(servicesPath);
                Directory.CreateDirectory(sourcesPath);
                Directory.CreateDirectory(pluginsPath);

                var xml = XmlResource.Fetch("Calculate_RecordSet_Subtract");
                xml.Save(Path.Combine(servicesPath, "Calculate_RecordSet_Subtract.xml"));

                xml = XmlResource.Fetch("HostSecurityProviderServerSigned");
                xml.Save(Path.Combine(sourcesPath, "HostSecurityProviderServerSigned.xml"));

                var testResources = new Dev2.Runtime.ServiceModel.Resources();
                var actual = testResources.Paths("", workspaceID, Guid.Empty);
                Assert.AreEqual("[\"Integration Test Services\"]", actual);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        #region private test methods

        private void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach(string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach(string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private Guid generateADLGuid()
        {
            var _compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors = new ErrorResultTO();
            Guid exID = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), GetSimpleADL(), GetSimpleADLShape(), out errors);
            if(errors.HasErrors())
            {
                string errorString = string.Empty;
                foreach(string item in errors.FetchErrors())
                {
                    errorString += item;
                }

                throw new Exception(errorString);
            }
            return exID;
        }

        private string GetSimpleADL()
        {
            return @"<ADL>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
  <gRec>
    <opt>Value1</opt>
    <display>display1</display>
  </gRec>
  <recset></recset>
  <field></field>
</ADL>";
        }

        private string GetSimpleADLShape()
        {
            return @"<ADL>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
  <gRec>
    <opt></opt>
    <display></display>
  </gRec>
  <recset></recset>
  <field></field>
</ADL>";
        }

        #endregion


        #region Sources

        [TestMethod]
        public void SourcesWithNullArgsExpectedReturnsEmptyList()
        {
            var resources = new Dev2.Runtime.ServiceModel.Resources();
            var result = resources.Sources(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SourcesWithInvalidArgsExpectedReturnsEmptyList()
        {
            var resources = new Dev2.Runtime.ServiceModel.Resources();
            var result = resources.Sources("xxxx", Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SourcesWithValidArgsExpectedReturnsList()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            try
            {
                const int Modulo = 2;
                const int ExpectedCount = 6;
                for(var i = 0; i < ExpectedCount; i++)
                {
                    var resource = new Resource
                    {
                        ResourceID = Guid.NewGuid(),
                        ResourceName = string.Format("My Name {0}", i),
                        ResourcePath = string.Format("My Path {0}", i),
                        ResourceType = (i % Modulo == 0) ? ResourceType.DbSource : ResourceType.Unknown
                    };
                    resource.Save(workspaceID, Guid.Empty);
                }
                var resources = new Dev2.Runtime.ServiceModel.Resources();
                var result = resources.Sources("{\"resourceType\":\"" + ResourceType.DbSource + "\"}", workspaceID, Guid.Empty);

                Assert.AreEqual(ExpectedCount / Modulo, result.Count);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }
        #endregion

    }
}
