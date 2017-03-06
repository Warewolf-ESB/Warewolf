using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    public class Parent
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Id { get; set; }
#pragma warning disable 169
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private string _name = "name";
#pragma warning restore 169

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _name;
        }

        #endregion
    }

    public class Child
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }
#pragma warning disable 169
        private string _name;
#pragma warning restore 169
    }

    [TestClass]
    public class MapperTests
    {

        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Map_GivenObjects_ShouldMapCorrectly()
        {
            //---------------Set up test pack-------------------
            Mapper.AddMap<Parent, Child>((parent1, child1) =>
            {
                child1.ParentId = parent1.Id;
            });
            var parent = new Parent()
            {
                Id = 100,
                Name = "name",
                Surname = "surname"
            };
            var child = new Child()
            {
                Name = "child",
                Id = 1
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Mapper.Map(parent, child, true, "Id", "Name");
            //---------------Test Result -----------------------
            Assert.AreEqual(100, parent.Id);
            Assert.AreEqual(100, child.ParentId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Map_GivenObjectsNoActions_ShouldMapCorrectly()
        {
            //---------------Set up test pack-------------------
            Mapper.AddMap<Parent, Child>();
            var parent = new Parent()
            {
                Id = 100,
                Name = "name",
                Surname = "surname"
            };
            var child = new Child()
            {
                Name = "child",
                Id = 1
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Mapper.Map(parent, child);
            //---------------Test Result -----------------------
            Assert.AreEqual(100, parent.Id);
            Assert.AreEqual(parent.Id, child.Id);
            Assert.AreEqual(parent.Name, child.Name);
            Assert.AreEqual(parent.Surname, child.Surname);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Map_GivenNullFrom_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            Mapper.AddMap<Parent, Child>();
            var parent = new Parent()
            {
                Id = 100,
                Name = "name",
                Surname = "surname"
            };
            var child = new Child()
            {
                Name = "child",
                Id = 1
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Mapper.Map(default(Parent), child);
            //---------------Test Result -----------------------
            Assert.AreEqual(100, parent.Id);
            Assert.AreEqual(parent.Id, child.Id);
            Assert.AreEqual(parent.Name, child.Name);
            Assert.AreEqual(parent.Surname, child.Surname);
        }

       

        
    }
}
