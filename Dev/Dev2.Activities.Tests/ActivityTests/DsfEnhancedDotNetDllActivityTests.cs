using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestingDotnetDllCascading;
using Warewolf.Core;
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
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation =  typeof(Human).Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
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
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
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
            activity.MethodsToRun = new List<IPluginAction>();
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
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
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
            activity.MethodsToRun = new List<IPluginAction>();
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
            Assert.AreEqual(0, activity.MethodsToRun.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNullValues_ShouldExecuteMethodUsingNulls()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            var env = new Mock<IExecutionEnvironment>();
            var human = new Human("Micky", "Mouse", new Food());
            var humanString = DataListUtil.ConvertModelToJson(human).ToString();
            var newWarewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(humanString));
            env.Setup(environment => environment.EvalForJson("[[@name]]", It.IsAny<bool>()))
                .Returns(newWarewolfAtomResult);

            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);

            mock.SetupGet(o => o.Environment).Returns(env.Object);
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "AddFavouriteFood"
            };
            activity.MethodsToRun = new List<IPluginAction>()
            {
                new PluginAction()
                {
                    Inputs = new List<IServiceInput>()
                    {
                        new ServiceInput()
                        {
                            TypeName = typeof(Food).AssemblyQualifiedName,
                            IsObject = true,
                            Name = "food",
                            Value = "[[food]]"
                        }
                    },
                    IsObject = false,
                    IsVoid = false,
                    Method = "AddFavouriteFood",
                    OutputVariable = "[[succes]]",

                }
            };
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
            var fieldInfo = typeof(DsfEnhancedDotNetDllActivity).GetField("_childStatesToDispatch", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo);
            var value = fieldInfo.GetValue(activity) as List<IDebugState>;
            Assert.IsNotNull(value);
            var any = value.Any(state => state.ErrorMessage != null && (state.ErrorMessage.Equals("Please specify favourite food") && state.Name.Equals("Method") &&
                                                                        state.DisplayName.Equals("AddFavouriteFood")));
            Assert.IsTrue(any);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenEmptyIsNull_ShouldExecuteWithNull()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            var env = new Mock<IExecutionEnvironment>();
            var human = new Human("Micky", "Mouse", new Food());
            var humanString = DataListUtil.ConvertModelToJson(human).ToString();
            var newWarewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(humanString));
            env.Setup(environment => environment.EvalForJson("[[@name]]", It.IsAny<bool>()))
                .Returns(newWarewolfAtomResult);

            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);

            mock.SetupGet(o => o.Environment).Returns(env.Object);
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "EmptyIsNullTest"
            };
            activity.MethodsToRun = new List<IPluginAction>()
            {
                new PluginAction()
                {
                    Inputs = new List<IServiceInput>()
                    {
                        new ServiceInput()
                        {
                            TypeName = typeof(string).AssemblyQualifiedName,
                            IsObject = false,
                            Name = "value",
                            EmptyIsNull = true
                        }
                    },
                    IsObject = false,
                    IsVoid = false,
                    Method = "EmptyIsNullTest",
                    OutputVariable = "[[succes]]",

                }
            };
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
            Assert.AreEqual(0, err.FetchErrors().Count);
            var methodResult = activity.MethodsToRun.Single().MethodResult;
            Assert.AreEqual("Null value passed", methodResult);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenEmptyIsNullFalse_ShouldExecuteWithEmptyString()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            var env = new Mock<IExecutionEnvironment>();
            var human = new Human("Micky", "Mouse", new Food());
            var humanString = DataListUtil.ConvertModelToJson(human).ToString();
            var newWarewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(humanString));
            env.Setup(environment => environment.EvalForJson("[[@name]]", It.IsAny<bool>()))
                .Returns(newWarewolfAtomResult);

            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);

            mock.SetupGet(o => o.Environment).Returns(env.Object);
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "EmptyIsNullTest"
            };
            activity.MethodsToRun = new List<IPluginAction>()
            {
                new PluginAction()
                {
                    Inputs = new List<IServiceInput>()
                    {
                        new ServiceInput()
                        {
                            TypeName = typeof(string).AssemblyQualifiedName,
                            IsObject = false,
                            Name = "value",
                            EmptyIsNull = false
                        }
                    },
                    IsObject = false,
                    IsVoid = false,
                    Method = "EmptyIsNullTest",
                    OutputVariable = "[[succes]]",

                }
            };
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
            var methodResult = activity.MethodsToRun.Single().MethodResult;
            Assert.AreEqual("", methodResult);
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNoConstructor_ShouldDefaultEmptyConstructor()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
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
            activity.MethodsToRun = new List<IPluginAction>();

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
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
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
            activity.MethodsToRun = new List<IPluginAction>();
            activity.Constructor = new PluginConstructor
            {
                Inputs = new List<IConstructorParameter>(),
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "ToString",
                    Inputs = new List<IServiceInput>(),
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
        [Owner("Hagashen Naidu")]
        public void Execute_GivenValidArgs_ListType_ToObject_ShouldReturnValidData()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "FavouriteFoods"
            };
            activity.Constructor = new PluginConstructor
            {
                Inputs = new List<IConstructorParameter>(),
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "FavouriteFoods",
                    IsObject = true,
                    Inputs = new List<IServiceInput>(),
                    OutputVariable = "[[@Foods()]]"
                }
            };
            //---------------Assert Precondition----------------
            ErrorResultTO err;
            var dsfDataObject = mock.Object;
            activity.ExecuteMock(esbChannel.Object, dsfDataObject, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, err.FetchErrors().Count);
            var jContainer = dsfDataObject.Environment.EvalJContainer("[[@Foods()]]");
            Assert.IsNotNull(jContainer);
            var values = jContainer.Children().Select(token => token.ToString()).ToList();
            Assert.IsNotNull(values);
            StringAssert.Contains(values[0], "Pizza");
            StringAssert.Contains(values[1], "Burger");
            StringAssert.Contains(values[2], "Chicken");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void Execute_GivenValidArgs_ListType_ToRecordset_ShouldReturnValidData()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "PhoneNumbers"
            };
            activity.Constructor = new PluginConstructor
            {
                Inputs = new List<IConstructorParameter>(),
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "PhoneNumbers",
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    OutputVariable = "[[Food().Name]]"
                }
            };
            //---------------Assert Precondition----------------
            ErrorResultTO err;
            var dsfDataObject = mock.Object;
            activity.ExecuteMock(esbChannel.Object, dsfDataObject, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, err.FetchErrors().Count);
            var jContainer = dsfDataObject.Environment.EvalAsList("[[Food(*).Name]]", 0).ToList();
            Assert.IsNotNull(jContainer);
            Assert.AreEqual("1284561478", jContainer[0].ToString());
            Assert.AreEqual("228561478", jContainer[1].ToString());
            Assert.AreEqual("215561475", jContainer[2].ToString());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void Execute_GivenValidArgs_ListType_ToScalar_ShouldReturnValidData()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            mock.SetupGet(o => o.EsbChannel).Returns(esbChannel.Object);
            mock.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            activity.Namespace = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                MethodName = "PhoneNumbers"
            };
            activity.Constructor = new PluginConstructor
            {
                Inputs = new List<IConstructorParameter>(),
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "PhoneNumbers",
                    IsObject = false,
                    Inputs = new List<IServiceInput>(),
                    OutputVariable = "[[Foods]]"
                }
            };
            //---------------Assert Precondition----------------
            ErrorResultTO err;
            var dsfDataObject = mock.Object;
            activity.ExecuteMock(esbChannel.Object, dsfDataObject, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, err.FetchErrors().Count);
            var jContainer = dsfDataObject.Environment.Eval("[[Foods]]", 0) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(jContainer);
            Assert.AreEqual("1284561478,228561478,215561475", jContainer.Item.ToString());
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
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
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
            activity.Constructor = new PluginConstructor
            {
                IsExistingObject = true,
                ConstructorName = "@Human"
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "ToString",
                    Inputs = new List<IServiceInput>(),
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
            Assert.AreEqual("Name:Micky, Surname:Mouse, FoodName:Lettuce", methodResult);
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
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
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
            activity.Constructor = new PluginConstructor
            {
                IsExistingObject = true,
                ConstructorName = "@Human"
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "ToString",
                    Inputs = new List<IServiceInput>(),
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
            Assert.AreEqual("Name:Micky, Surname:Mouse, FoodName:Lettuce", methodResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFindMissingType_GivenActivity_ShouldReturnDatgrid()
        {
            //---------------Set up test pack-------------------
            var activity = new DsfEnhancedDotNetDllActivityMock();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, activity.GetFindMissingType());
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildConstructorInputs_GivenConstructor_ShouldConstructorDebug()
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
            activity.Constructor = new PluginConstructor
            {
                IsExistingObject = true,
                ConstructorName = "@Human"
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "ToString",
                    Inputs = new List<IServiceInput>(),
                }
            };

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DsfEnhancedDotNetDllActivity).GetMethod("BuildConstructorInputs", BindingFlags.Instance | BindingFlags.NonPublic);
            var debugInputs = methodInfo.Invoke(activity, new object[] { executionEnv.Object, 0, false }) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugInputs);
            Assert.AreEqual(1, debugInputs.Count);

            var constructorInput1Value = debugInputs[0].ResultsList[0].Value;
            var constructorInput1name = debugInputs[0].ResultsList[0].Label;
            Assert.AreEqual("name", constructorInput1name);
            Assert.AreEqual("John", constructorInput1Value);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildConstructorOutput_GivenConstructorWithOutput_ShouldConstructorDebug()
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
                .NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(humanString));
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
            activity.Constructor = new PluginConstructor
            {
                IsExistingObject = true,
                ConstructorName = "@Human"
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "ToString",
                    Inputs = new List<IServiceInput>(),
                }
            };
            activity.ObjectName = "[[@Human]]";
            activity.ObjectResult = "humanString";

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DsfEnhancedDotNetDllActivity).GetMethod("BuildConstructorOutput", BindingFlags.Instance | BindingFlags.NonPublic);
            var debugInputs = methodInfo.Invoke(activity, new object[] { executionEnv.Object, 0, false }) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugInputs);
            Assert.AreEqual(1, debugInputs.Count);
            var constructorValue = debugInputs[0].ResultsList[0].Value;
            Assert.AreNotEqual("", constructorValue);
            Assert.IsNotNull(constructorValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildMethodInputs_GivenActionsWithInputs_ShouldActionsInputDebug()
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
            activity.Constructor = new PluginConstructor
            {
                IsExistingObject = true,
                ConstructorName = "@Human"
            };
            var pluginAction = new PluginAction()
            {
                Method = "ToString",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput("name","Micky")
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                pluginAction
            };

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DsfEnhancedDotNetDllActivity).GetMethod("BuildMethodInputs", BindingFlags.Instance | BindingFlags.NonPublic);
            var debugInputs = methodInfo.Invoke(activity, new object[] { executionEnv.Object, pluginAction, 0, false }) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugInputs);
            Assert.AreEqual(1, debugInputs.Count);

            var name = debugInputs[0].ResultsList[0].Label;
            var value = debugInputs[0].ResultsList[0].Value;
            Assert.AreEqual("name", name);
            Assert.AreEqual("John", value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildMethodOutputs_GivenMethodOutput_ShouldDebugOutput()
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
            activity.Constructor = new PluginConstructor
            {
                IsExistingObject = true,
                ConstructorName = "@Human"
            };
            var pluginAction = new PluginAction()
            {
                Method = "ToString",
                Inputs = new List<IServiceInput>()
                {
                    new ServiceInput("name","Micky")
                },
                MethodResult = new Human().SerializeToJsonString(new KnownTypesBinder()
                {
                    KnownTypes = new List<Type>() { type }
                })
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                pluginAction
            };
            activity.ObjectName = "@Human";

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DsfEnhancedDotNetDllActivity).GetMethod("BuildMethodOutputs", BindingFlags.Instance | BindingFlags.NonPublic);
            var debugOutputs = methodInfo.Invoke(activity, new object[] { executionEnv.Object, pluginAction, 0, false }) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugOutputs);
            Assert.AreEqual(1, debugOutputs.Count);

            var val = debugOutputs[0].ResultsList[0].Value;
            StringAssert.Contains(val, "John");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildMethodOutputs_GivenMethodIsvoid_ShouldReturnNone()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var human = new Human("Micky", "Mouse", new Food { FoodName = "Lettuce" });

            var knownBinder = new KnownTypesBinder();
            knownBinder.KnownTypes.Add(type);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
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
            activity.Constructor = new PluginConstructor
            {
                IsExistingObject = true,
                ConstructorName = "@Human"
            };
            var pluginAction = new PluginAction()
            {
                Method = "SetNameInternal",
                Inputs = new List<IServiceInput>(),
                MethodResult = new Human().SerializeToJsonString(new KnownTypesBinder()
                {
                    KnownTypes = new List<Type>() { type }
                }),
                IsVoid = true
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                pluginAction
            };
            activity.ObjectName = "@Human";

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(DsfEnhancedDotNetDllActivity).GetMethod("BuildMethodOutputs", BindingFlags.Instance | BindingFlags.NonPublic);
            var debugOutputs = methodInfo.Invoke(activity, new object[] { executionEnv.Object, pluginAction, 0, false }) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugOutputs);
            Assert.AreEqual(1, debugOutputs.Count);

            var val = debugOutputs[0].ResultsList[0].Value;
            StringAssert.Contains(val, "None");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenHasIncorrectExistingTypeObjectSelected_ShouldAddCorrectError()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var food = new Food { FoodName = "Lettuce" };
            var knownBinder = new KnownTypesBinder();
            knownBinder.KnownTypes.Add(type);
            var activity = new DsfEnhancedDotNetDllActivityMock();
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(resourceCatalog => resourceCatalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new PluginSource()
                {
                    AssemblyLocation = type.Assembly.Location
                });
            activity.ResourceCatalog = catalog.Object;
            var mock = new Mock<IDSFDataObject>();
            var esbChannel = new Mock<IEsbChannel>();
            var executionEnv = new Mock<IExecutionEnvironment>();
            var foodJson = food.SerializeToJsonString(new KnownTypesBinder() { KnownTypes = new List<Type>() { typeof(Food) } });
            var newWarewolfAtomResult = CommonFunctions.WarewolfEvalResult
                .NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(foodJson));
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
            activity.Constructor = new PluginConstructor
            {
                IsExistingObject = true,
                ConstructorName = "@food"
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction()
                {
                    Method = "ToString",
                    Inputs = new List<IServiceInput>()
                    {
                        new ServiceInput("name","Micky")
                    },
                    MethodResult = new Human().SerializeToJsonString(new KnownTypesBinder()
                    {
                        KnownTypes = new List<Type>(){type}
                    })
                }
            };
            activity.ObjectName = "@food";

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            ErrorResultTO err;
            activity.ExecuteMock(esbChannel.Object, mock.Object, string.Empty, string.Empty, out err);
            //---------------Test Result -----------------------

            Assert.AreEqual(1, err.FetchErrors().Count);
            var single = err.FetchErrors().Single();
            StringAssert.Contains(single, "is not compatible with");
        }
    }

    internal class DsfEnhancedDotNetDllActivityMock : DsfEnhancedDotNetDllActivity
    {
        // ReSharper disable once RedundantAssignment
        public void ExecuteMock(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors)
        {
            ExecutionImpl(esbChannel, dataObject, inputs, outputs, out errors, 0);
        }
    }
}
