using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestingDotnetDllCascading;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DsfEnhancedDotNetDllActivityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenIsNew_ShouldHaveCorrectValues()
        {
            //---------------Set up test pack-------------------
            var activity = new DsfEnhancedDotNetDllActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("DotNet DLL Connector", activity.Type.Expression.ToString());
            Assert.AreEqual("DotNet DLL", activity.DisplayName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNoNamespace_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            //---------------Assert Precondition----------------
            Assert.AreEqual("DotNet DLL Connector", activity.Type.Expression.ToString());
            Assert.AreEqual("DotNet DLL", activity.DisplayName);
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, err.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNoMethod_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            activity.Namespace = new NamespaceItem();
            //---------------Assert Precondition----------------
            Assert.AreEqual("DotNet DLL Connector", activity.Type.Expression.ToString());
            Assert.AreEqual("DotNet DLL", activity.DisplayName);
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, err.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNoMethodNameSpace_ShouldPassThough()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.SetupGet(o => o.Environment).Returns(new ExecutionEnvironment());
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "ToString"
            };
            activity.MethodsToRun = new List<Dev2MethodInfo>();
            activity.Constructor = new PluginConstructor
            {
                Inputs = new List<IServiceInput>(),
            };
            //---------------Assert Precondition----------------
            Assert.AreEqual("DotNet DLL Connector", activity.Type.Expression.ToString());
            Assert.AreEqual("DotNet DLL", activity.DisplayName);
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, err.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNoConstructor_ShouldCreateEmptyConstructor()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.SetupGet(o => o.Environment).Returns(new ExecutionEnvironment());
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "ToString"
            };
            activity.MethodsToRun = new List<Dev2MethodInfo>();
         
            //---------------Assert Precondition----------------
            Assert.AreEqual("DotNet DLL Connector", activity.Type.Expression.ToString());
            Assert.AreEqual("DotNet DLL", activity.DisplayName);
            Assert.IsNull(activity.Constructor);
            Assert.IsNotNull(activity.ConstructorInputs);
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.IsNotNull(activity.Constructor);
            Assert.IsNotNull(activity.ConstructorInputs);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenValidArgs_ShouldReturnValidData()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "ToString"
            };
            activity.MethodsToRun = new List<Dev2MethodInfo>();
            activity.Constructor = new PluginConstructor
            {
                Inputs = new List<IServiceInput>(),
            };
            activity.MethodsToRun = new List<Dev2MethodInfo>
            {
                new Dev2MethodInfo
                {
                    Method = "ToString",
                    Parameters = new List<MethodParameter>(),
                }
            };
            //---------------Assert Precondition----------------
            Assert.AreEqual("DotNet DLL Connector", activity.Type.Expression.ToString());
            Assert.AreEqual("DotNet DLL", activity.DisplayName);
            Assert.AreEqual(string.Empty, activity.ExistingObject);
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, err.FetchErrors().Count);
        }
    }

    internal class DsfEnhancedDotNetDllActivityMock : DsfEnhancedDotNetDllActivity
    {
        // ReSharper disable once RedundantAssignment
        public void ExecuteMock(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors)
        {
            base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out errors, 0);
        }
    }
}
