using System.Collections.Generic;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels.VariableList;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests.Variable_List
{
    [TestClass]
    public class DataListViewRecordSetViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void DataListViewRecordSetViewModel_Ctor_NullValues_ExpectError()

        {

            
            //------------Execute Test---------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { "", new IVariableListViewColumn[0] , new Mock<IVariableListViewModel>().Object}, typeof(DataListViewRecodSetViewModel));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Ctor")]
        public void DataListViewRecordSetViewModel_Ctor_ValidValues_ExpectPropertiesSetAndCommandsNotNull()
        {
            //------------Setup for test--------------------------
            var dataListViewRecordSetViewModel = new DataListViewRecodSetViewModel("bob", new IVariableListViewColumn[0], new Mock<IVariableListViewModel>().Object);
            Assert.AreEqual("bob", dataListViewRecordSetViewModel.Name);
            Assert.IsFalse(dataListViewRecordSetViewModel.DeleteVisible);
            Assert.IsFalse(dataListViewRecordSetViewModel.Input);
            Assert.IsFalse(dataListViewRecordSetViewModel.Output);
            Assert.IsTrue(dataListViewRecordSetViewModel.InputVisible);
            Assert.IsTrue(dataListViewRecordSetViewModel.OutputVisible);
            Assert.IsNotNull(dataListViewRecordSetViewModel.Delete);
            Assert.IsNotNull(dataListViewRecordSetViewModel.EditNotes);
            Assert.AreEqual("",dataListViewRecordSetViewModel.Notes);
          
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Ctor")]
        public void DataListViewRecordSetViewModel_Ctor_Visibilities()
        {
            //------------Setup for test--------------------------
            var dataListViewRecordSetViewModel = new DataListViewRecodSetViewModel("bob", new IVariableListViewColumn[0], new Mock<IVariableListViewModel>().Object);

            dataListViewRecordSetViewModel.DeleteVisible = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.DeleteVisible);
            Assert.IsFalse(dataListViewRecordSetViewModel.InputVisible);
            Assert.IsFalse(dataListViewRecordSetViewModel.OutputVisible);
            dataListViewRecordSetViewModel.DeleteVisible = false;
            Assert.IsFalse(dataListViewRecordSetViewModel.DeleteVisible);
            Assert.IsTrue(dataListViewRecordSetViewModel.InputVisible);
            Assert.IsTrue(dataListViewRecordSetViewModel.OutputVisible);
            dataListViewRecordSetViewModel.InputVisible = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.InputVisible);
            Assert.IsFalse(dataListViewRecordSetViewModel.DeleteVisible);
            dataListViewRecordSetViewModel.OutputVisible = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.OutputVisible);
            Assert.IsFalse(dataListViewRecordSetViewModel.DeleteVisible);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Properties")]
        public void DataListViewRecordSetViewModel_Properties_GetsAndSets()
        {
            //------------Setup for test--------------------------
            var cols = new IVariableListViewColumn[0];
            var dataListViewRecordSetViewModel = new DataListViewRecodSetViewModel("bob", cols, new Mock<IVariableListViewModel>().Object);

            dataListViewRecordSetViewModel.Notes = "moocow";
            Assert.AreEqual("moocow",dataListViewRecordSetViewModel.Notes);
            dataListViewRecordSetViewModel.Used = true;
            Assert.IsFalse(dataListViewRecordSetViewModel.DeleteVisible);
            Assert.IsTrue(dataListViewRecordSetViewModel.Used);
            dataListViewRecordSetViewModel.Visible = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.Visible);
            Assert.AreEqual(cols,dataListViewRecordSetViewModel.Columns);
            dataListViewRecordSetViewModel.IsValid = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.IsValid);
            dataListViewRecordSetViewModel.ToolTip = "bobs";
            Assert.AreEqual("bobs",dataListViewRecordSetViewModel.ToolTip);
            dataListViewRecordSetViewModel.Output = true;
            dataListViewRecordSetViewModel.Input = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.Output);
            Assert.IsTrue(dataListViewRecordSetViewModel.Input);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_DeleteCommand")]
        public void DataListViewRecordSetViewModel_DeleteCommand_CallsParent()
        {
            //------------Setup for test--------------------------
           var mockItem =new Mock<IVariableListViewModel>();
           var dataListViewRecordSetViewModel = new DataListViewRecodSetViewModel("bob", new IVariableListViewColumn[0], mockItem.Object);
           dataListViewRecordSetViewModel.Delete.Execute();
           mockItem.Verify(a => a.Delete(dataListViewRecordSetViewModel));



        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Add")]
        public void DataListViewRecordSetViewModel_Add_Adds()
        {
            //------------Setup for test--------------------------
            var mockItem = new Mock<IVariableListViewModel>();
            var dataListViewRecordSetViewModel = new DataListViewRecodSetViewModel("bob", new IVariableListViewColumn[0], mockItem.Object);
            dataListViewRecordSetViewModel.Delete.Execute();
            mockItem.Verify(a => a.Delete(dataListViewRecordSetViewModel));



        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Add")]
        public void DataListViewRecordSetViewModel_Add_AddsColl()
        {
            //------------Setup for test--------------------------
            var mockItem = new Mock<IVariableListViewModel>();
            var dataListViewRecordSetViewModel = new DataListViewRecodSetViewModel("bob", new List<IVariableListViewColumn>(), mockItem.Object);
            var coll = new Mock<IVariableListViewColumn>().Object;
            dataListViewRecordSetViewModel.AddColumn(coll);
            Assert.AreEqual(dataListViewRecordSetViewModel.Columns[0],coll);



        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Add")]
        public void DataListViewRecordSetViewModel_Add_NoDuplicateColls()
        {
            //------------Setup for test--------------------------
            var mockItem = new Mock<IVariableListViewModel>();
            var dataListViewRecordSetViewModel = new DataListViewRecodSetViewModel("bob", new List<IVariableListViewColumn>(), mockItem.Object);
            var coll = new Mock<IVariableListViewColumn>().Object;
            dataListViewRecordSetViewModel.AddColumn(coll);
            dataListViewRecordSetViewModel.AddColumn(coll);
            Assert.AreEqual(1,dataListViewRecordSetViewModel.Columns.Count);
            Assert.AreEqual(dataListViewRecordSetViewModel.Columns[0], coll);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Add")]
        public void DataListViewRecordSetViewModel_Remove_Removed()
        {
            //------------Setup for test--------------------------
            var mockItem = new Mock<IVariableListViewModel>();
            var dataListViewRecordSetViewModel = new DataListViewRecodSetViewModel("bob", new List<IVariableListViewColumn>(), mockItem.Object);
            var coll = new Mock<IVariableListViewColumn>().Object;
            dataListViewRecordSetViewModel.AddColumn(coll);
            Assert.AreEqual(dataListViewRecordSetViewModel.Columns[0], coll);
            dataListViewRecordSetViewModel.RemoveColumn(coll);
            Assert.AreEqual(0, dataListViewRecordSetViewModel.Columns.Count);
           

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListViewRecordSetViewModel_Equals")]
        public void DataListViewRecordSetViewModel_Equals()
        {
            //------------Setup for test--------------------------
            var mockItem = new Mock<IVariableListViewModel>();
            var items =  new[]{new Mock<IVariableListViewColumn>().Object};
            var a = new DataListViewRecodSetViewModel("bob",items,mockItem.Object);
            var b = new DataListViewRecodSetViewModel("bob", items, mockItem.Object);
            var c = new DataListViewRecodSetViewModel("bob", new[]{new Mock<IVariableListViewColumn>().Object}, mockItem.Object);
            var d = new DataListViewRecodSetViewModel("dave", items, mockItem.Object);
            Assert.AreNotEqual(a,null);
            Assert.AreNotEqual(a, (IVariablelistViewRecordSet)null);
            Assert.AreEqual(a,a as object);
            Assert.AreEqual(a,a);
            Assert.AreNotEqual(a,3);
            Assert.AreEqual(a,(object) b);

            Assert.AreEqual(a,b);
            Assert.AreNotEqual(b,c);
            Assert.AreNotEqual(b, d);
            Assert.IsTrue(a==b);
            Assert.IsTrue(a!=c);
            Assert.IsTrue(c!=d);
            Assert.AreNotEqual(a.GetHashCode(),c.GetHashCode());
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
        // ReSharper restore InconsistentNaming
    }
}
