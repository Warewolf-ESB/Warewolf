using System;
using System.IO;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Session;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.DataList
{
    [TestClass]
    public class DebugToTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DebugTO_Cleanup")]
        public void DebugTO_Cleanup_DisposeExpectsClear()
        {
            // bootstrap
            var dto = new Mock<DebugTO>();
            var dl = new Mock<IBinaryDataList>();
            bool disposed = false;
            dto.CallBase = true;
            DebugTO to = dto.Object;
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><persistantscalar>SomeValue</persistantscalar><rs><f1>f1Value</f1><f2>f2Value</f2></rs><recset><field1>somedata</field1><field2>moredata</field2></recset><recset><field1></field1><field2></field2></recset></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to.BinaryDataList = dl.Object;
            dl.Setup(a => a.Dispose()).Callback(() => disposed = true);
            to.CleanUp();

            Assert.IsTrue(disposed);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DebugTO_Cleanup")]
        public void DebugTO_ValuesCorrect()
        {
            // bootstrap
            var dto = new Mock<DebugTO>();
            var dl = new Mock<IBinaryDataList>();
  
            dto.CallBase = true;
            DebugTO to = dto.Object;
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            to.RememberInputs = true;
            Assert.IsTrue(to.RememberInputs);
            to.BaseSaveDirectory = rootFolder;
            Assert.AreEqual(to.BaseSaveDirectory,rootFolder);
            to.DataList = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            Assert.AreEqual(to.DataList, "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>");
            to.XmlData = "<DataList><scalar1>s1</scalar1><persistantscalar>SomeValue</persistantscalar><rs><f1>f1Value</f1><f2>f2Value</f2></rs><recset><field1>somedata</field1><field2>moredata</field2></recset><recset><field1></field1><field2></field2></recset></DataList>";
            Assert.AreEqual(to.XmlData, "<DataList><scalar1>s1</scalar1><persistantscalar>SomeValue</persistantscalar><rs><f1>f1Value</f1><f2>f2Value</f2></rs><recset><field1>somedata</field1><field2>moredata</field2></recset><recset><field1></field1><field2></field2></recset></DataList>");
            to.ServiceName = "DummyService";
            Assert.AreEqual(to.ServiceName, "DummyService");
            to.WorkflowID = "DummyService";
            to.BinaryDataList = dl.Object;


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DebugTO_Cleanup")]
        public void DebugTO_Copy_ValuesCorrect()
        {
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><persistantscalar>SomeValue</persistantscalar><rs><f1>f1Value</f1><f2>f2Value</f2></rs><recset><field1>somedata</field1><field2>moredata</field2></recset><recset><field1></field1><field2></field2></recset></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            var save = to.CopyToSaveDebugTO();
            Assert.AreEqual(save.DataList,to.DataList);
            Assert.AreEqual(save.ServiceName,to.ServiceName);
            Assert.AreEqual(to.IsDebugMode,to.IsDebugMode);
            Assert.AreEqual(to.WorkflowID,to.WorkflowID);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DebugTO_Cleanup")]
        public void DebugTO_CopyFrom_ValuesCorrect()
        {
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><persistantscalar>SomeValue</persistantscalar><rs><f1>f1Value</f1><f2>f2Value</f2></rs><recset><field1>somedata</field1><field2>moredata</field2></recset><recset><field1></field1><field2></field2></recset></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            var save = to.CopyToSaveDebugTO();
            save.DataList = "bob";
            save.WorkflowID = "dave";
            save.IsDebugMode = false;
            to.CopyFromSaveDebugTO(save);
            Assert.AreEqual(to.WorkflowID,"dave");
            Assert.AreEqual(to.IsDebugMode,false);
            Assert.AreEqual(to.DataList,"bob");

        }


    }
}
