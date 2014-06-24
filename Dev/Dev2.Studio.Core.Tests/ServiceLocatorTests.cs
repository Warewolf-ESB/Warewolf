using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ServiceLocatorTests
    {

        #region Initialize

        [TestInitialize()]
        public void Initialize()
        {

        }

        #endregion

        #region Tests

        #region Register

        [TestMethod]
        public void Register_StaticEndpoint_Expected_StaticEndpointReturnedWhenAnEnpointIsRequestedUsingTheKeyItWasRegisteredWith()
        {
            string staticEndPoint = "http://localhost";
            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, new Uri(staticEndPoint));

            Uri expected = new Uri(staticEndPoint);
            Uri actual = serviceLocator.GetEndpoint(endpointKey);

            Assert.AreEqual(expected.AbsoluteUri, actual.AbsoluteUri);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Register_StaticEndpoint_Where_KeyIsAlreadyUsedForEndpointGenerationStrategy_Expected_ExceptionIsThrown()
        {
            Func<object, Uri> endPointGenerationStrategy = new Func<object, Uri>(o =>
            {
                return null;
            });

            string staticEndPoint = "http://localhost";
            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, endPointGenerationStrategy);
            serviceLocator.RegisterEnpoint(endpointKey, new Uri(staticEndPoint));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_StaticEndpoint_Where_KeyIsNull_Expected_ExceptionIsThrown()
        {
            string staticEndPoint = "http://localhost";
            string endpointKey = null;

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, new Uri(staticEndPoint));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_StaticEndpoint_Where_EndpointIsNull_Expected_ExceptionIsThrown()
        {
            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, null);
        }

        [TestMethod]
        public void Register_EndpointGenerationStrategy_Expected_GenerationStrategyIsRunWhenAnEnpointIsRequestedUsingTheKeyItWasRegisteredWith()
        {
            bool endpointGenerationStrategyWasRun = false;
            Func<object, Uri> endPointGenerationStrategy = new Func<object, Uri>(o =>
            {
                endpointGenerationStrategyWasRun = true;
                return null;
            });

            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, endPointGenerationStrategy);

            serviceLocator.GetEndpoint(endpointKey, new object());

            Assert.IsTrue(endpointGenerationStrategyWasRun);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Register_EndpointGenerationStrategy_Where_KeyIsAlreadyUsedForStaticEndpoint_Expected_ExceptionIsThrown()
        {
            Func<object, Uri> endPointGenerationStrategy = new Func<object, Uri>(o =>
            {
                return null;
            });

            string staticEndPoint = "http://localhost";
            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, new Uri(staticEndPoint));
            serviceLocator.RegisterEnpoint(endpointKey, endPointGenerationStrategy);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_EndpointGenerationStrategy_Where_KeyIsNull_Expected_ExceptionIsThrown()
        {
            Func<object, Uri> endPointGenerationStrategy = new Func<object, Uri>(o =>
            {
                return null;
            });

            string endpointKey = null;

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, endPointGenerationStrategy);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_EndpointGenerationStrategy_Where_EndpointGenerationStrategyIsNull_Expected_ExceptionIsThrown()
        {
            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, null);
        }

        #endregion Register

        #region Get

        [TestMethod]
        public void Get_StaticEndpoint_Expected_StaticEndpointReturned()
        {
            string staticEndPoint = "http://localhost";
            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, new Uri(staticEndPoint));

            Uri expected = new Uri(staticEndPoint);
            Uri actual = serviceLocator.GetEndpoint(endpointKey);

            Assert.AreEqual(expected.AbsoluteUri, actual.AbsoluteUri);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Get_StaticEndpoint_Where_KeyIsNull_Expected_ExceptionThrown()
        {
            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.GetEndpoint(null);
        }

        [TestMethod]
        public void Get_StaticEndpoint_Where_KeyDoesntExist_Expected_Null()
        {
            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();

            Uri actual = serviceLocator.GetEndpoint(endpointKey);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void Get_EndpointGenerationStrategy_Expected_GenerationStrategyReturnsEndpoint()
        {
            string endpointKey = "testKey";
            string endPoint = "http://localhost";

            Func<object, Uri> endPointGenerationStrategy = new Func<object, Uri>(o =>
            {
                return new Uri(endPoint);
            });

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, endPointGenerationStrategy);

            Uri expected = new Uri(endPoint);
            Uri actual = serviceLocator.GetEndpoint(endpointKey, new object());

            Assert.AreEqual(expected.AbsoluteUri, actual.AbsoluteUri);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Get_EndpointGenerationStrategy_Where_KeyIsNull_Expected_ExceptionThrown()
        {
            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.GetEndpoint(null, new object());
        }

        [TestMethod]
        public void Get_EndpointGenerationStrategy_Where_KeyDoesntExist_Expected_Null()
        {
            string endpointKey = "testKey";

            ServiceLocator serviceLocator = new ServiceLocator();

            Uri actual = serviceLocator.GetEndpoint(endpointKey, new object());

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Get_EndpointGenerationStrategy_Where_ParameterOfGenerationStrategyIsADifferentTypeToTheRegisteredOne_Expected_ExceptionThrown()
        {
            string endpointKey = "testKey";
            string endPoint = "http://localhost";

            Func<string, Uri> endPointGenerationStrategy = new Func<string, Uri>(o =>
            {
                return new Uri(endPoint);
            });

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, endPointGenerationStrategy);

            serviceLocator.GetEndpoint(endpointKey, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Get_EndpointGenerationStrategy_Where_GenerationStrategyThrowsAnException_Expected_ExceptionThrown()
        {
            string endpointKey = "testKey";

            Func<string, Uri> endPointGenerationStrategy = new Func<string, Uri>(o =>
            {
                throw new Exception();
            });

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.RegisterEnpoint(endpointKey, endPointGenerationStrategy);

            serviceLocator.GetEndpoint(endpointKey, 1);
        }

        #endregion Get

        #endregion Tests
    }
}
