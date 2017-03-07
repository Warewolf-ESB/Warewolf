using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    class Contact
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    [TestClass]
    public class DistinctByTests
    {




        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenPropertyName_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            List<Contact> contacts1 = new List<Contact>() {
                                                                        new Contact(),
                                                                        new Contact()
                                                                        {
                                                                            Id = Guid.NewGuid(),
                                                                            Name = "a"
                                                                        },
                                                                        new Contact() {Name = "b"}
                                                            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var contacts = contacts1.DistinctBy(contact => contact.Name);
            Assert.AreEqual(3, contacts.Count());
            contacts1.Add(new Contact());
            contacts = contacts1.DistinctBy(contact => contact.Name);
            //---------------Test Result -----------------------
            Assert.AreEqual(3, contacts.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenPropertyNullGuidValues_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            List<Contact> contacts1 = new List<Contact>() {
                                                                        new Contact(),
                                                                        new Contact()
                                                                        {
                                                                            Id = Guid.NewGuid(),
                                                                            Name = "a"
                                                                        },
                                                                        new Contact() {Name = "b"}
                                                            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var contacts = contacts1.DistinctBy(contact => contact.Id);
            Assert.AreEqual(2, contacts.Count());
            contacts1.Add(new Contact());
            contacts = contacts1.DistinctBy(contact => contact.Id);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, contacts.Count());
        }
       
    }
}
