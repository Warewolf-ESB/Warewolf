using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Core.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.CollectionBaseViewModelTests
{
    [TestClass]
    public class ActivityCollectionViewModelBaseTests
    {
        [TestMethod]
        [TestCategory("ActivityCollectionViewModelBase_UnitTest")]
        [Description("Collection view model base can initialize")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ActivityCollectionViewModelBase_Constructor_NoRows_ViewModelConstructed()
        // ReSharper restore InconsistentNaming
        {
            //init
            const string ExpectedCollectionName = "FieldsCollection";
            var collectionNameProp = new Mock<ModelProperty>();
            var dtoListProp = new Mock<ModelProperty>();
            var displayNameProp = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            collectionNameProp.Setup(p => p.ComputedValue).Returns(ExpectedCollectionName);
            dtoListProp.Setup(p => p.ComputedValue).Returns(new List<ActivityDTO>());
            displayNameProp.Setup(p => p.ComputedValue).Returns("Test Display Name");
            properties.Add("CollectionName", collectionNameProp);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "CollectionName", true).Returns(collectionNameProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", ExpectedCollectionName, true).Returns(dtoListProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProp.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            //exe
            var vm = new TestActivityCollectionViewModelBase<ActivityDTO>(mockModel.Object);

            //assert
            Assert.AreEqual(2, vm.CountRows(), "Collection view model base cannot initialized 2 blank rows from a model item with no rows");
            Assert.IsInstanceOfType(vm, typeof(ActivityCollectionViewModelBase), "Collection view model base cannot initialize");
        }
    }
}
