using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ObservableNameValueTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(" ObservableNameValue")]
        public void ObservableNameValue_Constructor()
        {
            var name = "testName";
            var value = "testValue";
            var result = new NameValue() { Name = name , Value = value } as INameValue;
            Assert.IsNotNull(result);
            Assert.AreEqual("testName", result.Name);
            Assert.AreEqual("testValue", result.Value);
            Assert.AreEqual("testName", result.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(" ObservableNameValue")]
        public void ObservableNameValue_Constructor_Values()
        {
            var constructor = new NameValue("testName", "testValue");
            Assert.IsNotNull(constructor);
            Assert.AreEqual("testName", constructor.Name);
            Assert.AreEqual("testValue", constructor.Value);
            Assert.AreEqual("testName", constructor.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(" ObservableNameValue")]
        public void ObservableNameValue_OnPropertyChanged()
        {
            var constructor = new NameValue();
            var receivedEvents = new List<string>();

            constructor.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };
            constructor.Name = "testNameChanged";
            constructor.Value = "testNameChanged";
            Assert.AreEqual(2, receivedEvents.Count);
            Assert.AreEqual("Name", receivedEvents[0]);
            Assert.AreEqual("Value", receivedEvents[1]);
        }
    }
}
