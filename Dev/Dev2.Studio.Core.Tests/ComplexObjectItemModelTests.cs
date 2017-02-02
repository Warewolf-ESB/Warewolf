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
            Assert.AreEqual("@TestItem",complexObjectItemModel.Name);
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
            Assert.AreEqual("@ParentItem.TestItem",complexObjectItemModel.Name);
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

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ComplexObjectItemModel_ValidateNames")]
        public void ComplexObjectItemModel_ValidateNames_WithInvalidComplexObjectParentNameWithDot_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var complexObject = new ComplexObjectItemModel("Parent.");

            //------------Execute Test---------------------------
            
            //------------Assert Results-------------------------
            Assert.IsTrue(complexObject.HasError);
            Assert.AreEqual("Complex Object name [[Parent.]] contains invalid character(s). Only use alphanumeric _ and - ", complexObject.ErrorMessage);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ComplexObjectItemModel_ValidateNames")]
        public void ComplexObjectItemModel_ValidateNames_WithInvalidComplexObjectChildNameWithDot_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var complexObject = new ComplexObjectItemModel("Parent");
            complexObject.Children.Add(new ComplexObjectItemModel("Name.", complexObject));
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            //Parent
            Assert.IsFalse(complexObject.HasError);
            Assert.IsNull(complexObject.ErrorMessage);
            //Child
            Assert.IsTrue(complexObject.Children[0].HasError);
            Assert.AreEqual("Complex Object name [[Name.]] contains invalid character(s). Only use alphanumeric _ and - ", complexObject.Children[0].ErrorMessage);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ComplexObjectItemModel_ValidateNames")]
        public void ComplexObjectItemModel_ValidateNames_WithValidComplexObject_IsUsed()
        {
            //------------Setup for test--------------------------
            var complexObject = new ComplexObjectItemModel("Parent");
            complexObject.Children.Add(new ComplexObjectItemModel("Name", complexObject));
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            //Parent
            Assert.IsTrue(complexObject.IsUsed);
            
            //Child
            Assert.IsTrue(complexObject.Children[0].IsUsed);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ComplexObjectItemModel_ValidateNames")]
        public void ComplexObjectItemModel_ValidateNames_WithValidLongComplexObject_IsUsed()
        {
            //------------Setup for test--------------------------
            var complexObject = new ComplexObjectItemModel("Pet");
            complexObject.Children.Add(new ComplexObjectItemModel("O()", complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("n()", complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("l()", complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("m(*)", complexObject));
            complexObject.Children.Add(new ComplexObjectItemModel("X", complexObject));
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            //Parent
            Assert.IsTrue(complexObject.IsUsed);

            //Child
            Assert.IsTrue(complexObject.Children[0].IsUsed);
            Assert.IsTrue(complexObject.Children[1].IsUsed);
            Assert.IsTrue(complexObject.Children[2].IsUsed);
            Assert.IsTrue(complexObject.Children[3].IsUsed);
            Assert.IsTrue(complexObject.Children[4].IsUsed);
        }
    }
}
