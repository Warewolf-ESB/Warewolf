using Caliburn.Micro;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DataListViewModelEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NewDataListViewModel_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            var dataListViewModel1 = new DataListViewModel(new Mock<IEventAggregator>().Object);

            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            dataListViewModel1.InitializeDataListViewModel(mockResourceModel.Object);
            dataListViewModel.RecsetCollection.Clear();
            dataListViewModel.ScalarCollection.Clear();
            dataListViewModel1.RecsetCollection.Clear();
            dataListViewModel1.ScalarCollection.Clear();
            dataListViewModel1.ComplexObjectCollection.Clear();
            dataListViewModel.ComplexObjectCollection.Clear();

           
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dataListViewModel);
            Assert.IsNotNull(dataListViewModel1);
            //---------------Execute Test ----------------------
            var equals = dataListViewModel.Equals(dataListViewModel1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DataListViewModel_SameVaribalesCollection_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            var dataListViewModel1 = new DataListViewModel(new Mock<IEventAggregator>().Object);

            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            dataListViewModel1.InitializeDataListViewModel(mockResourceModel.Object);
            dataListViewModel.RecsetCollection.Clear();
            dataListViewModel.ScalarCollection.Clear();
            dataListViewModel1.RecsetCollection.Clear();
            dataListViewModel1.ScalarCollection.Clear();
            dataListViewModel1.ComplexObjectCollection.Clear();
            dataListViewModel.ComplexObjectCollection.Clear();

            IRecordSetItemModel carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("Car", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));
            carRecordset.Input = true;
            carRecordset.Output = true;
            dataListViewModel.RecsetCollection.Add(carRecordset);
            dataListViewModel1.RecsetCollection.Add(carRecordset);
            dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both));
            dataListViewModel1.ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dataListViewModel);
            Assert.IsNotNull(dataListViewModel1);
            //---------------Execute Test ----------------------
            var equals = dataListViewModel.Equals(dataListViewModel1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DataListViewModel_DifferentVariablesCollection_Object_Is_NOT_Equal()
        {
            //---------------Set up test pack-------------------
            var mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            var dataListViewModel1 = new DataListViewModel(new Mock<IEventAggregator>().Object);

            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            dataListViewModel1.InitializeDataListViewModel(mockResourceModel.Object);
            dataListViewModel.RecsetCollection.Clear();
            dataListViewModel.ScalarCollection.Clear();
            dataListViewModel1.RecsetCollection.Clear();
            dataListViewModel1.ScalarCollection.Clear();
            dataListViewModel1.ComplexObjectCollection.Clear();
            dataListViewModel.ComplexObjectCollection.Clear();

            IRecordSetItemModel carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("Car", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));
            carRecordset.Input = true;
            carRecordset.Output = true;
            dataListViewModel.RecsetCollection.Add(carRecordset);
            dataListViewModel1.RecsetCollection.Add(carRecordset);
            dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dataListViewModel);
            Assert.IsNotNull(dataListViewModel1);
            //---------------Execute Test ----------------------
            var equals = dataListViewModel.Equals(dataListViewModel1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
    }
}
