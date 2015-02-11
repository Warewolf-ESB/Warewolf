using System.Collections.Generic;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels.VariableList;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DatalistScalarViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DatalistViewScalarViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void DatalistViewScalarViewModel_Ctor_NullValues_ExpectError()
        {


            //------------Execute Test---------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { "", new Mock<IVariableListViewModel>().Object }, typeof(VariableListItemViewScalarViewModel));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DatalistViewScalarViewModel_Ctor")]
        public void DatalistViewScalarViewModel_Ctor_ValidValues_ExpectPropertiesSetAndCommandsNotNull()
        {
            //------------Setup for test--------------------------
            var dataListViewRecordSetViewModel = new VariableListItemViewScalarViewModel("bob", new Mock<IVariableListViewModel>().Object, new List<IVariableListViewScalarViewModel>());
            Assert.AreEqual("bob", dataListViewRecordSetViewModel.Name);
            Assert.IsFalse(dataListViewRecordSetViewModel.DeleteVisible);
            Assert.IsFalse(dataListViewRecordSetViewModel.Input);
            Assert.IsFalse(dataListViewRecordSetViewModel.Output);
            Assert.IsTrue(dataListViewRecordSetViewModel.InputVisible);
            Assert.IsTrue(dataListViewRecordSetViewModel.OutputVisible);
            Assert.IsNotNull(dataListViewRecordSetViewModel.Delete);
            Assert.IsNotNull(dataListViewRecordSetViewModel.EditNotes);
            Assert.AreEqual("", dataListViewRecordSetViewModel.Notes);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DatalistViewScalarViewModel_Ctor")]
        public void DatalistViewScalarViewModel_Ctor_Visibilities()
        {
            //------------Setup for test--------------------------
            var dataListViewRecordSetViewModel = new VariableListItemViewScalarViewModel("bob", new Mock<IVariableListViewModel>().Object, new List<IVariableListViewScalarViewModel>()) { DeleteVisible = true };

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
        [TestCategory("DatalistViewScalarViewModel_Properties")]
        public void DatalistViewScalarViewModel_Properties_GetsAndSets()
        {
            //------------Setup for test--------------------------

            var dataListViewRecordSetViewModel = new VariableListItemViewScalarViewModel("bob", new Mock<IVariableListViewModel>().Object, new List<IVariableListViewScalarViewModel>()) { Notes = "moocow" };

            Assert.AreEqual("moocow", dataListViewRecordSetViewModel.Notes);
            dataListViewRecordSetViewModel.Used = true;
            Assert.IsFalse(dataListViewRecordSetViewModel.DeleteVisible);
            Assert.IsTrue(dataListViewRecordSetViewModel.Used);
            dataListViewRecordSetViewModel.Visible = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.Visible);
            dataListViewRecordSetViewModel.IsValid = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.IsValid);
            dataListViewRecordSetViewModel.ToolTip = "bobs";
            Assert.AreEqual("bobs", dataListViewRecordSetViewModel.ToolTip);
            dataListViewRecordSetViewModel.Output = true;
            dataListViewRecordSetViewModel.Input = true;
            Assert.IsTrue(dataListViewRecordSetViewModel.Output);
            Assert.IsTrue(dataListViewRecordSetViewModel.Input);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DatalistViewScalarViewModel_DeleteCommand")]
        public void DatalistViewScalarViewModel_DeleteCommand_CallsParent()
        {
            //------------Setup for test--------------------------
            var mockItem = new Mock<IVariableListViewModel>();
            var val = new VariableListItemViewScalarViewModel("bob", mockItem.Object, new List<IVariableListViewScalarViewModel>());
            
            val.Delete.Execute();
            mockItem.Verify(a => a.Delete(val));



        }







        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DatalistViewScalarViewModel_Equals")]
        public void DatalistViewScalarViewModel_Equals()
        {
            //------------Setup for test--------------------------
            var mockItem = new Mock<IVariableListViewModel>();
            var a = new VariableListItemViewScalarViewModel("bob", mockItem.Object, new List<IVariableListViewScalarViewModel>());
            var b = new VariableListItemViewScalarViewModel("bob", mockItem.Object, new List<IVariableListViewScalarViewModel>());

            var d = new VariableListItemViewScalarViewModel("dave", mockItem.Object, new List<IVariableListViewScalarViewModel>());
            Assert.AreNotEqual(a, null);
            Assert.AreNotEqual(a, (IVariablelistViewRecordSetViewModel)null);
            Assert.AreEqual(a, a as object);
            Assert.AreEqual(a, a);
            Assert.AreNotEqual(a, 3);
            Assert.AreEqual(a, (object)b);
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(b, d);
            Assert.IsTrue(a == b);
            Assert.IsTrue(a != d);
            Assert.IsTrue(b != d);
            Assert.AreNotEqual(a.GetHashCode(), d.GetHashCode());
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
