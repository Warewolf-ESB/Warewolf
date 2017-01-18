using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestingDotnetDllCascading;
using Warewolf.Core;
using Warewolf.Storage;
using WarewolfParserInterop;

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
                Inputs = new List<IConstructorParameter>(),
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
        public void Execute_GivenIsExistingObject_ShouldEvalFromEnvironment()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            var env = new Mock<IExecutionEnvironment>();
            var human = new Human("Micky", "Mouse", new Food());
            var humanString = DataListUtil.ConvertModelToJson(human).ToString();
            var newWarewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(humanString));
            env.Setup(environment => environment.EvalForJson(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(newWarewolfAtomResult);
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);

            mock.SetupGet(o => o.Environment).Returns(env.Object);
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "ToString"
            };
            activity.MethodsToRun = new List<Dev2MethodInfo>();
            activity.Constructor = new PluginConstructor()
            {
                IsExistingObject = true,
                ConstructorName = "@name"
            };
            //---------------Assert Precondition----------------
            Assert.AreEqual("DotNet DLL Connector", activity.Type.Expression.ToString());
            Assert.AreEqual("DotNet DLL", activity.DisplayName);
            Assert.IsNotNull(activity.ConstructorInputs);
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            env.Verify(environment => environment.EvalForJson("[[@name]]", It.IsAny<bool>()));
            Assert.AreEqual(0,activity.MethodsToRun.Count);
        }

       

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNoConstructor_ShouldDefaultEmptyConstructor()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            var env = new Mock<IExecutionEnvironment>();
            var human = new Human("Micky", "Mouse", new Food());
            var humanString = DataListUtil.ConvertModelToJson(human).ToString();
            var newWarewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(humanString));
            env.Setup(environment => environment.EvalForJson(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(newWarewolfAtomResult);
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.SetupGet(o => o.Environment).Returns(env.Object);
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
            Assert.IsNotNull(activity.ConstructorInputs);
            Assert.IsNull(activity.Constructor);
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
                Inputs = new List<IConstructorParameter>(),
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
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, err.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenExistingObject_ShouldUseExistingObject()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var human = new Human("Micky", "Mouse", new Food { FoodName = "Lettuce" });
            var knownBinder = new KnownTypesBinder();
            knownBinder.KnownTypes.Add(type);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            var executionEnv = new Mock<IExecutionEnvironment>();
            var humanString = DataListUtil.ConvertModelToJson(human).ToString();
            var newWarewolfAtomResult = CommonFunctions.WarewolfEvalResult
                .NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(humanString));
            executionEnv.Setup(environment => environment.EvalForJson(It.IsAny<string>(), It.IsAny<bool>()))
               .Returns(newWarewolfAtomResult);
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.Setup(o => o.Environment).Returns(executionEnv.Object);
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
                IsExistingObject = true,
                ConstructorName = "@Human"
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
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, err.FetchErrors().Count);
            var methodResult = activity.MethodsToRun.Single().MethodResult;
            Assert.AreEqual("\"Name:Micky, Surname:Mouse, FoodName:Lettuce\"", methodResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenParameters_ShouldUseExistingObject()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var human = new Human("Micky", "Mouse", new Food { FoodName = "Lettuce" });
            var knownBinder = new KnownTypesBinder();
            knownBinder.KnownTypes.Add(type);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            var executionEnv = new Mock<IExecutionEnvironment>();
            var humanString = DataListUtil.ConvertModelToJson(human).ToString();
            var newWarewolfAtomResult = CommonFunctions.WarewolfEvalResult
                .NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(humanString));
            executionEnv.Setup(environment => environment.EvalForJson(It.IsAny<string>(), It.IsAny<bool>()))
               .Returns(newWarewolfAtomResult);
            var johnResult = CommonFunctions.WarewolfEvalResult
                .NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("John"));
            executionEnv.Setup(environment => environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(johnResult);
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.Setup(o => o.Environment).Returns(executionEnv.Object);
            activity.ConstructorInputs = new List<IServiceInput>()
            {
                new ServiceInput("name","John")
                {
                    TypeName = typeof(string).FullName,
                    RequiredField = true
                }
            };
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
                IsExistingObject = true,
                ConstructorName = "@Human"
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
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, err.FetchErrors().Count);
            var methodResult = activity.MethodsToRun.Single().MethodResult;
            Assert.AreEqual("\"Name:Micky, Surname:Mouse, FoodName:Lettuce\"", methodResult);
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
