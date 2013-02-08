using System;
using System.IO;
using System.Xml;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Resources.Save(workspacePath, directoryName, "testResource",
                           "<?xml version=\"1.0\"?><Root>This is the text of the root element</Root>");
            Assert.IsTrue(File.ReadAllText(Directory.GetCurrentDirectory() + @"\" + threadSave + @"\testResource.xml").Contains("This is the text of the root element"));
            //Cleanup
            if(Directory.Exists(Directory.GetCurrentDirectory() + @"\" + threadSave)) DeleteDirectory(Directory.GetCurrentDirectory() + @"\" + threadSave);
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
            Resources.Save(workspacePath, directoryName, "testResource",
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
            Resources.Save(workspacePath, directoryName, "testResource",
                   "<?xml version=\"1.0\"?>This is the text of the root element");
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void SaveSlashesInDirectory_Expected_DirectoryNotFoundException()
        {
            var threadSave = Guid.NewGuid();
            string directoryName = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\" + threadSave).Name;
            string workspacePath = Directory.GetParent(Directory.GetCurrentDirectory() + @"\" + threadSave).ToString();
            Resources.Save(workspacePath, directoryName, "test/Resource",
                   "<?xml version=\"1.0\"?><Root>This is the text of the root element</Root>");
        }

        [TestMethod]
        public void Paths_Expected_JSONSources()
        {
            var threadSave = Guid.NewGuid();
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory() + @"/Workspaces/" + threadSave, "Plugins"));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory() + @"/Workspaces/" + threadSave, "Services"));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory() + @"/Workspaces/" + threadSave, "Sources"));
            var xml = XmlResource.Fetch("Calculate_RecordSet_Subtract");
            xml.Save(Path.Combine(Path.Combine(Directory.GetCurrentDirectory() + @"/Workspaces/" + threadSave, "Services"), "Calculate_RecordSet_Subtract.xml"));
            xml = XmlResource.Fetch("HostSecurityProviderServerSigned");
            xml.Save(Path.Combine(Path.Combine(Directory.GetCurrentDirectory() + @"/Workspaces/" + threadSave, "Sources"), "HostSecurityProviderServerSigned.xml"));

            var testResources = new Resources();
            string actual = testResources.Paths("", threadSave, generateADLGuid());
            Assert.AreEqual("[\"Integration Test Services\"]", actual);
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
            var services = new Resources();
            var result = services.Sources(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SourcesWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new Resources();
            var result = services.Sources("xxxx", Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

    }
}
