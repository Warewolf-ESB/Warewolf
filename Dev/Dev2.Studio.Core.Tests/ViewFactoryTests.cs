using System;
using System.Collections.Concurrent;
using Dev2.Studio.Core;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ViewFactoryTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_HasCorrectType()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenBadKey_returnsNull()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var view = viewFactory.GetViewGivenServerResourceType("crap");
            //---------------Test Result -----------------------
            Assert.IsNull(view);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenServer_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("Server"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenDev2Server_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("Dev2Server"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenServerSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("ServerSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenRabbitMQSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("RabbitMQSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenOauthSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("OauthSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenSharepointServerSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("SharepointServerSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenDropBoxSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("DropBoxSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenExchangeSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("ExchangeSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenEmailSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("EmailSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenWcfSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("WcfSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenComPluginSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("ComPluginSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenPluginSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("PluginSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenWebSource_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("WebSource"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenMySqlDatabase_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("MySqlDatabase"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenPostgreSQL_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("PostgreSQL"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenOracle_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("Oracle"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenODBC_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("ODBC"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewFactory_GivenSqlDatabase_ShouldHaveKey()
        {
            //---------------Set up test pack-------------------
            var viewFactory = new ViewFactory();
            PrivateObject privateObject = new PrivateObject(viewFactory);
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(viewFactory, typeof(IViewFactory));
            //---------------Execute Test ----------------------
            var concurrentDictionary = privateObject.GetField("_viewMap") as ConcurrentDictionary<string, Func<IView>>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(concurrentDictionary);
            Assert.IsTrue(concurrentDictionary.ContainsKey("SqlDatabase"));
        }

    }
}
