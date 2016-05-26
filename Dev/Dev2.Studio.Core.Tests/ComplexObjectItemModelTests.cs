using System.Collections.ObjectModel;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
// ReSharper disable UseObjectOrCollectionInitializer

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ComplexObjectItemModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ComplexObjectItemModel_Constructor")]
        public void ComplexObjectItemModel_Constructor_ShouldCreateItem()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var complexObjectItemModel = new ComplexObjectItemModel("TestItem");
            //------------Assert Results-------------------------
            Assert.IsNotNull(complexObjectItemModel);
            Assert.IsInstanceOfType(complexObjectItemModel,typeof(DataListItemModel));
            Assert.IsInstanceOfType(complexObjectItemModel,typeof(IComplexObjectItemModel));
            Assert.IsNull(complexObjectItemModel.Parent);
            Assert.IsTrue(complexObjectItemModel.IsParentObject);
            Assert.AreEqual("TestItem",complexObjectItemModel.DisplayName);
            Assert.AreEqual("TestItem",complexObjectItemModel.Name);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ComplexObjectItemModel_Children")]
        public void ComplexObjectItemModel_Children_GetWhenNotSet_ShouldReturnNewCollection()
        {
            //------------Setup for test--------------------------
            var complexObjectItemModel = new ComplexObjectItemModel("TestItem");

            //------------Execute Test---------------------------
            var children = complexObjectItemModel.Children;
            //------------Assert Results-------------------------
            Assert.IsNotNull(children);
            Assert.AreEqual(0,children.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ComplexObjectItemModel_Children")]
        public void ComplexObjectItemModel_Children_GetWhenSet_ShouldReturnSetCollection()
        {
            //------------Setup for test--------------------------
            var complexObjectItemModel = new ComplexObjectItemModel("TestItem");

            //------------Execute Test---------------------------
            complexObjectItemModel.Children = new ObservableCollection<IComplexObjectItemModel> { new ComplexObjectItemModel("TestChild") };
            var children = complexObjectItemModel.Children;
            //------------Assert Results-------------------------
            Assert.IsNotNull(children);
            Assert.AreEqual(1,children.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ComplexObjectItemModel_Parent")]
        public void ComplexObjectItemModel_Parent_Set_ShouldUpdateNameIsParentObject()
        {
            //------------Setup for test--------------------------
            var complexObjectItemModel = new ComplexObjectItemModel("TestItem");
            
            //------------Execute Test---------------------------
            complexObjectItemModel.Parent = new ComplexObjectItemModel("ParentItem");
            //------------Assert Results-------------------------
            Assert.IsNotNull(complexObjectItemModel.Parent);
            Assert.AreEqual("ParentItem.TestItem",complexObjectItemModel.Name);
            Assert.AreEqual("TestItem",complexObjectItemModel.DisplayName);
            Assert.IsFalse(complexObjectItemModel.IsParentObject);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ComplexObject_GetJson")]
        public void ComplexObjectItemModel_GetJson_PrimitiveChildren_ShouldReturnCorrectJson()
        {
            //------------Setup for test--------------------------
            var complexObject = new ComplexObjectItemModel("Parent");
            complexObject.Children.Add(new ComplexObjectItemModel("Name",complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("Age", complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("Gender", complexObject));
            //------------Execute Test---------------------------
            var jsonString = complexObject.GetJson();
            //------------Assert Results-------------------------
            Assert.AreEqual("{\"Parent\":{\"Name\":\"\",\"Age\":\"\",\"Gender\":\"\"}}", jsonString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ComplexObject_GetJson")]
        public void ComplexObjectItemModel_GetJson_HasObjectChildren_ShouldReturnCorrectJson()
        {
            //------------Setup for test--------------------------
            var complexObject = new ComplexObjectItemModel("Parent");
            complexObject.Children.Add(new ComplexObjectItemModel("Name",complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("Age", complexObject));
            var schoolObject = new ComplexObjectItemModel("School", complexObject);
            complexObject.Children.Add(schoolObject);
            schoolObject.Children.Add(new ComplexObjectItemModel("Name", schoolObject));
            schoolObject.Children.Add(new ComplexObjectItemModel("Location", schoolObject));
            complexObject.Children.Add(new ComplexObjectItemModel("Gender", complexObject));
            //------------Execute Test---------------------------
            var jsonString = complexObject.GetJson();
            //------------Assert Results-------------------------
            Assert.AreEqual("{\"Parent\":{\"Name\":\"\",\"Age\":\"\",\"School\":{\"Name\":\"\",\"Location\":\"\"},\"Gender\":\"\"}}", jsonString);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ComplexObject_GetJson")]
        public void ComplexObjectItemModel_GetJson_IsArray_PrimitiveChildren_ShouldReturnCorrectJson()
        {
            //------------Setup for test--------------------------
            var complexObject = new ComplexObjectItemModel("Parent");
            complexObject.IsArray = true;
            complexObject.Children.Add(new ComplexObjectItemModel("Name", complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("Age", complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("Gender", complexObject));
            //------------Execute Test---------------------------
            var jsonString = complexObject.GetJson();
            //------------Assert Results-------------------------
            Assert.AreEqual("{\"Parent\":[{\"Name\":\"\",\"Age\":\"\",\"Gender\":\"\"}]}", jsonString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ComplexObject_GetJson")]
        public void ComplexObjectItemModel_GetJson_HasObjectChildrenIsArray_ShouldReturnCorrectJson()
        {
            //------------Setup for test--------------------------
            var complexObject = new ComplexObjectItemModel("Parent");
            complexObject.Children.Add(new ComplexObjectItemModel("Name", complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("Age", complexObject));
            var schoolObject = new ComplexObjectItemModel("School", complexObject);
            complexObject.Children.Add(schoolObject);
            schoolObject.Children.Add(new ComplexObjectItemModel("Name", schoolObject));
            schoolObject.Children.Add(new ComplexObjectItemModel("Location", schoolObject));
            schoolObject.IsArray = true;
            complexObject.Children.Add(new ComplexObjectItemModel("Gender", complexObject));
            //------------Execute Test---------------------------
            var jsonString = complexObject.GetJson();
            //------------Assert Results-------------------------
            Assert.AreEqual("{\"Parent\":{\"Name\":\"\",\"Age\":\"\",\"School\":[{\"Name\":\"\",\"Location\":\"\"}],\"Gender\":\"\"}}", jsonString);
        }
    }
}
