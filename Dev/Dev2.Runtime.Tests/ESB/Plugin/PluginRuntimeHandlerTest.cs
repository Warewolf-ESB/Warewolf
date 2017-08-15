/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Runtime;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestingDotnetDllCascading;

namespace Dev2.Tests.Runtime.ESB.Plugin
{
    /// <summary>
    /// Summary description for PluginRuntimeHandlerTest
    /// </summary>
    [TestClass]

    public class PluginRuntimeHandlerTest
    {

        #region FetchNamespaceListObject

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        public void PluginRuntimeHandler_FetchNamespaceListObject_WhenValidDll_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest));
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.FetchNamespaceListObject(source);
                //------------Assert Results-------------------------
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        public void PluginRuntimeHandler_FetchNamespaceListObjectWithJsonObjects_WhenValidDll_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest));
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.FetchNamespaceListObjectWithJsonObjects(source);
                //------------Assert Results-------------------------
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObject_WhenNullDll_ExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObject(null);
                //------------Assert Results-------------------------
            }

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObjectWithJsonObjects_WhenNullDll_ExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObjectWithJsonObjects(null);
                //------------Assert Results-------------------------
            }

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObject_WhenNullLocationInSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest), true);
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObject(source);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObjectWithJsonObjects_WhenNullLocationInSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest), true);
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObjectWithJsonObjects(source);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObject_WhenNullLocationAndInvalidSourceID_ExpectException()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest), true);
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObject(source);
            }

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObjectWithJsonObjects_WhenNullLocationAndInvalidSourceID_ExpectException()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest), true);
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObjectWithJsonObjects(source);
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(BadImageFormatException))]
        public void FetchNamespaceListObject_GivenThrowsBadFormatExceptionError_ShouldRethrowBadFormatException()
        {
            //---------------Set up test pack-------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest));
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            Assembly assembly;
            mockAssemblyLoader.Setup(loader => loader.TryLoadAssembly(It.IsAny<string>(), It.IsAny<string>(), out assembly))
                .Throws(new BadImageFormatException());
            var pluginRuntimeHandler = new PluginRuntimeHandler(mockAssemblyLoader.Object);
            //---------------Test Result -----------------------
            pluginRuntimeHandler.FetchNamespaceListObject(source);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(BadImageFormatException))]
        public void FetchNamespaceListObjectWithJsonObjects_GivenThrowsBadFormatExceptionError_ShouldRethrowBadFormatException()
        {
            //---------------Set up test pack-------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest));
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            Assembly assembly;
            mockAssemblyLoader.Setup(loader => loader.TryLoadAssembly(It.IsAny<string>(), It.IsAny<string>(), out assembly))
                .Throws(new BadImageFormatException());
            var pluginRuntimeHandler = new PluginRuntimeHandler(mockAssemblyLoader.Object);
            //---------------Test Result -----------------------
            pluginRuntimeHandler.FetchNamespaceListObjectWithJsonObjects(source);
        }

        #endregion

        #region ValidatePlugin

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_ValidatePlugin")]
        //public void PluginRuntimeHandler_ValidatePlugin_WhenValidDll_ExpectBlankMessage()
        //{
        //    //------------Setup for test--------------------------
        //    var pluginRuntimeHandler = new PluginRuntimeHandler();
        //    var source = CreatePluginSource();

        //    //------------Execute Test---------------------------
        //    var result = pluginRuntimeHandler.ValidatePlugin(source.AssemblyLocation);

        //    //------------Assert Results-------------------------
        //    StringAssert.Contains(result, string.Empty);
        //}

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_ValidatePlugin")]
        //public void PluginRuntimeHandler_ValidatePlugin_WhenNotADll_ExpectErrorMessage()
        //{
        //    //------------Setup for test--------------------------
        //    var pluginRuntimeHandler = new PluginRuntimeHandler();
        //    var source = CreatePluginSource();

        //    //------------Execute Test---------------------------
        //    var result = pluginRuntimeHandler.ValidatePlugin(source.AssemblyLocation + ".foo");

        //    //------------Assert Results-------------------------
        //    StringAssert.Contains(result, "Not a Dll file");
        //}

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_ValidatePlugin")]
        //public void PluginRuntimeHandler_ValidatePlugin_WhenGacDll_ExpectBlankMessage()
        //{
        //    //------------Setup for test--------------------------
        //    var pluginRuntimeHandler = new PluginRuntimeHandler();

        //    //------------Execute Test---------------------------
        //    var result = pluginRuntimeHandler.ValidatePlugin("GAC:mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

        //    //------------Assert Results-------------------------
        //    StringAssert.Contains(result, string.Empty);
        //}


        #endregion

        #region ListNamespaces

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_ListNamespaces")]
        public void PluginRuntimeHandler_ListNamespaces_WhenValidLocation_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(typeof(DummyClassForPluginTest));
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ListNamespaces(source.AssemblyLocation, "Foo");
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_ListNamespaces")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_ListNamespaces_WhenNullLocation_ExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.ListNamespaces(null, "Foo");
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_ListNamespaces")]
        public void PluginRuntimeHandler_ListNamespaces_WhenInvalidLocation_ExpectNoResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ListNamespaces("z:\foo\asm.dll", "Foo");
                Assert.IsFalse(result.Any());
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ListMethods")]
        public void PluginRuntimeHandler_ListMethods_WhenInvalidLocation_ExpectNoResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ListMethods("z:\foo\asm.dll", "asm.dll", "asm.dll");
                Assert.IsFalse(result.Any());
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ListMethods")]
        public void PluginRuntimeHandler_ListConstructors_WhenInvalidLocation_ExpectNoResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ListConstructors("z:\foo\asm.dll", "asm.dll", "asm.dll");
                Assert.IsFalse(result.Any());
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ListMethods")]
        public void PluginRuntimeHandler_ListMethods_WhenValidLocation_ExpectResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var fullName = Assembly.GetExecutingAssembly().Location;
                var dllName = Path.GetFileName(fullName);
                var result = isolated.Value.ListMethods(fullName, dllName, typeof(Main).FullName);
                Assert.IsTrue(result.Any());
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ListMethods")]
        public void PluginRuntimeHandler_ListConstructors_WhenValidLocation_ExpectResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var fullName = Assembly.GetExecutingAssembly().Location;
                var dllName = Path.GetFileName(fullName);
                var result = isolated.Value.ListConstructors(fullName, dllName, typeof(Main).FullName);
                Assert.IsTrue(result.Any());
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ListMethodsWithReturns")]
        public void PluginRuntimeHandler_ListMethodsWithReturns_WhenValidLocation_ExpectResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var fullName = Assembly.GetExecutingAssembly().Location;
                var dllName = Path.GetFileName(fullName);
                var result = isolated.Value.ListMethodsWithReturns(fullName, dllName, typeof(Main).FullName);
                Assert.IsTrue(result.Any());
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ListMethodsWithReturns")]
        public void PluginRuntimeHandler_ListMethodsWithReturns_WhenValidLocationAndVoid_ExpectResultsWithVoidMethod()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var fullName = Assembly.GetExecutingAssembly().Location;
                var dllName = Path.GetFileName(fullName);
                var result = isolated.Value.ListMethodsWithReturns(fullName, dllName, typeof(Main).FullName);
                Assert.IsTrue(result.Any());
                Assert.IsTrue(result.Any(method => method.IsVoid));
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ListMethodsWithReturns")]
        public void PluginRuntimeHandler_ListMethodsWithReturns_WhenValidLocationAndIsProperty_ExpectResultsWithPropertyMethod()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var fullName = Assembly.GetExecutingAssembly().Location;
                var dllName = Path.GetFileName(fullName);
                var result = isolated.Value.ListMethodsWithReturns(fullName, dllName, typeof(Main).FullName);
                Assert.IsTrue(result.Any());
                Assert.IsTrue(result.Any(method => method.IsProperty));
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ListMethodsWithReturns")]
        public void PluginRuntimeHandler_ListMethodsWithReturns_WhenListFoods_ExpectJSonArrayReturnType()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var fullName = Assembly.GetExecutingAssembly().Location;
                var dllName = Path.GetFileName(fullName);
                var result = isolated.Value.ListMethodsWithReturns(fullName, dllName, typeof(Main).FullName);
                Assert.IsTrue(result.Any());
                var serviceMethods = result.Where(method => !method.IsVoid);
                var condition = serviceMethods.Any(method => method.Dev2ReturnType.Contains("["));
                Assert.IsTrue(condition);
            }
        }

        #endregion

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_CreateInstance")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PluginRuntimeHandler_CreateInstance_WhenNullParameters_ExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.CreateInstance(null);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_CreateInstance")]
        public void PluginRuntimeHandler_CreateInstance_WhenHuman_ExpectHumanStringObject()
        {
            //------------Setup for test--------------------------

            var type = typeof(Human);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "ToString" } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var instance = isolated.Value.CreateInstance(new PluginInvokeArgs
                {
                    MethodsToRun = svc.MethodsToRun,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IConstructorParameter>(),

                    },
                    AssemblyLocation = type.Assembly.Location,
                    AssemblyName = type.Assembly.FullName,
                    Fullname = type.FullName,

                });

                var deserializeToObject = instance.ObjectString.DeserializeToObject(type, new KnownTypesBinder() { KnownTypes = new List<Type>(type.Assembly.ExportedTypes) });
                Assert.IsNotNull(deserializeToObject);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_CreateInstance")]
        public void PluginRuntimeHandler_CreateInstance_WhenHumanWithInputs_ExpectHumanStringObjectWithInputs()
        {
            //------------Setup for test--------------------------

            var type = typeof(Human);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "ToString" } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var instance = isolated.Value.CreateInstance(new PluginInvokeArgs
                {
                    MethodsToRun = svc.MethodsToRun,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IConstructorParameter>()
                        {
                            new ConstructorParameter()
                            {
                                  Name = "name"
                                , Value = "Jimmy"
                                , TypeName = typeof(string).AssemblyQualifiedName
                                , IsRequired = true
                            }
                        },

                    },
                    AssemblyLocation = type.Assembly.Location,
                    AssemblyName = type.Assembly.FullName,
                    Fullname = type.FullName,

                });

                var deserializeToObject = instance.ObjectString.DeserializeToObject(type, new KnownTypesBinder() { KnownTypes = new List<Type>(type.Assembly.ExportedTypes) });
                var firstOrDefault = deserializeToObject as Human;
                if (firstOrDefault != null)
                {

                    Assert.AreEqual("Jimmy", firstOrDefault.Name);
                }
                Assert.IsNotNull(deserializeToObject);
            }
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_Run")]
        public void PluginRuntimeHandler_Run_WhenObjectStringIsNotNull_ExpectRunsCorrectly()
        {
            //------------Setup for test--------------------------

            var type = typeof(Human);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "set_Name" , Parameters = new List<IMethodParameter>()
            {
                new ConstructorParameter()
                {
                    Name = "value", Value = "Micky", TypeName = typeof(string).FullName, IsRequired = true
                }
            } } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<PluginRuntimeHandler>())
            {
                var pluginInvokeArgs = new PluginInvokeArgs
                {
                    MethodsToRun = svc.MethodsToRun,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IConstructorParameter>(),
                    },
                    AssemblyLocation = type.Assembly.Location,
                    AssemblyName = type.Assembly.FullName,
                    Fullname = type.FullName,

                };
                var instance = isolated.Value.CreateInstance(pluginInvokeArgs);

                var deserializeToObject = instance.ObjectString.DeserializeToObject(type, new KnownTypesBinder() { KnownTypes = new List<Type>(type.Assembly.ExportedTypes) });
                Assert.IsNotNull(deserializeToObject);
                instance.Args = pluginInvokeArgs;
                var dev2MethodInfo = instance.Args.MethodsToRun.First();
                string stringOBj;
                var run = isolated.Value.Run(dev2MethodInfo, instance, out stringOBj);
                Assert.IsNotNull(run);
                StringAssert.Contains(stringOBj, "Default");
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_Run")]
        public void PluginRuntimeHandler_Run_WhenHasInnerError_ExpectInerErrors()
        {
            //------------Setup for test--------------------------

            var type = typeof(Human);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "set_Name" , Parameters = new List<IMethodParameter>()
            {
                new ConstructorParameter()
                {
                    Name = "value", Value = "Micky", TypeName = typeof(string).FullName, IsRequired = true
                }
            } } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            var mock = new Mock<IAssemblyLoader>();
            Assembly loadedAssembly;

            var handler = new PluginRuntimeHandler(mock.Object);

            var pluginInvokeArgs = new PluginInvokeArgs
            {
                MethodsToRun = svc.MethodsToRun,
                PluginConstructor = new PluginConstructor
                {
                    ConstructorName = svc.Constructor.Name,
                    Inputs = new List<IConstructorParameter>(),
                },
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                Fullname = type.FullName,

            };
            var pluginExecutionDto = new PluginExecutionDto(String.Empty)
            {
                Args = pluginInvokeArgs
            };
            var exception = new Exception("err", new Exception());
            mock.Setup(loader => loader.TryLoadAssembly(It.IsAny<string>(), It.IsAny<string>(), out loadedAssembly))
                .Throws(exception);

            var dev2MethodInfo = pluginInvokeArgs.MethodsToRun.First();
            string stringOBj;
            var run = handler.Run(dev2MethodInfo, pluginExecutionDto, out stringOBj);
            Assert.IsNotNull(run);
            Assert.IsTrue(run.HasError);
            Assert.IsFalse(string.IsNullOrEmpty(run.ErrorMessage));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_Test")]
        public void PluginRuntimeHandler_Test_WhenHasInnerError_ExpectInerErrors()
        {
            //------------Setup for test--------------------------

            var type = typeof(Human);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "set_Name" , Parameters = new List<IMethodParameter>()
            {
                new ConstructorParameter()
                {
                    Name = "value", Value = "Micky", TypeName = typeof(string).FullName, IsRequired = true
                }
            } } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            var mock = new Mock<IAssemblyLoader>();
            Assembly loadedAssembly;

            var handler = new PluginRuntimeHandler(mock.Object);

            var pluginInvokeArgs = new PluginInvokeArgs
            {
                MethodsToRun = svc.MethodsToRun,
                PluginConstructor = new PluginConstructor
                {
                    ConstructorName = svc.Constructor.Name,
                    Inputs = new List<IConstructorParameter>(),
                },
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                Fullname = type.FullName,

            };

            var exception = new Exception("err", new Exception());
            mock.Setup(loader => loader.TryLoadAssembly(It.IsAny<string>(), It.IsAny<string>(), out loadedAssembly))
                .Throws(exception);
            string stringOBj;
            var run = handler.Test(pluginInvokeArgs, out stringOBj);
            Assert.IsNull(run);
            Assert.IsTrue(string.IsNullOrEmpty(stringOBj));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_Run")]
        [ExpectedException(typeof(AccessViolationException))]
        public void PluginRuntimeHandler_Run_WhenHasError_ExpectErrors()
        {
            //------------Setup for test--------------------------

            var type = typeof(Human);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "set_Name" , Parameters = new List<IMethodParameter>()
            {
                new ConstructorParameter()
                {
                    Name = "value", Value = "Micky", TypeName = typeof(string).FullName, IsRequired = true
                }
            } } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            var mock = new Mock<IAssemblyLoader>();
            Assembly loadedAssembly;

            var handler = new PluginRuntimeHandler(mock.Object);

            var pluginInvokeArgs = new PluginInvokeArgs
            {
                MethodsToRun = svc.MethodsToRun,
                PluginConstructor = new PluginConstructor
                {
                    ConstructorName = svc.Constructor.Name,
                    Inputs = new List<IConstructorParameter>(),
                },
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                Fullname = type.FullName,

            };
            var pluginExecutionDto = new PluginExecutionDto(string.Empty)
            {
                Args = pluginInvokeArgs
            };
            //var instance = handler.CreateInstance(pluginInvokeArgs);
            var exception = new AccessViolationException("err");
            mock.Setup(loader => loader.TryLoadAssembly(It.IsAny<string>(), It.IsAny<string>(), out loadedAssembly))
                .Throws(exception);

            var dev2MethodInfo = pluginInvokeArgs.MethodsToRun.First();
            string stringOBj;
            var run = handler.Run(dev2MethodInfo, pluginExecutionDto, out stringOBj);


        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_CreateInstance")]
        public void PluginRuntimeHandler_Run_WhenClassIsStatic_ExpectRunsCorrectly()
        {
            //------------Setup for test--------------------------

            var type = typeof(StaticClass);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "ToString", Parameters = new List<IMethodParameter>() } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<PluginRuntimeHandler>())
            {
                var pluginInvokeArgs = new PluginInvokeArgs
                {
                    MethodsToRun = svc.MethodsToRun,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IConstructorParameter>(),
                    },
                    AssemblyLocation = type.Assembly.Location,
                    AssemblyName = type.Assembly.FullName,
                    Fullname = type.FullName,

                };
                var instance = isolated.Value.CreateInstance(pluginInvokeArgs);
                Assert.IsTrue(string.IsNullOrEmpty(instance.ObjectString));
                Assert.IsTrue(instance.IsStatic);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_CreateInstance")]
        public void PluginRuntimeHandler_Run_WhenClassIsStatic_ExpectRunsMethodsCorrectly()
        {
            //------------Setup for test--------------------------

            var type = typeof(StaticClass);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "ToStringOnStatic", Parameters = new List<IMethodParameter>() } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<PluginRuntimeHandler>())
            {
                var pluginInvokeArgs = new PluginInvokeArgs
                {
                    MethodsToRun = svc.MethodsToRun,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IConstructorParameter>(),
                    },
                    AssemblyLocation = type.Assembly.Location,
                    AssemblyName = type.Assembly.FullName,
                    Fullname = type.FullName,

                };
                var instance = isolated.Value.CreateInstance(pluginInvokeArgs);
                instance.Args = pluginInvokeArgs;
                string str;
                isolated.Value.Run(svc.MethodsToRun.First(), instance, out str);
                Assert.IsTrue(string.IsNullOrEmpty(str));
                Assert.IsTrue(instance.IsStatic);
            }
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_CreateInstance")]
        public void PluginRuntimeHandler_Run_WhenClassIsSealed_ExpectRunsCorrectly()
        {
            //------------Setup for test--------------------------

            var type = typeof(SealedClass);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "ToString", Parameters = new List<IMethodParameter>() } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<PluginRuntimeHandler>())
            {
                var pluginInvokeArgs = new PluginInvokeArgs
                {
                    MethodsToRun = svc.MethodsToRun,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IConstructorParameter>(),
                    },
                    AssemblyLocation = type.Assembly.Location,
                    AssemblyName = type.Assembly.FullName,
                    Fullname = type.FullName,

                };
                var instance = isolated.Value.CreateInstance(pluginInvokeArgs);
                Assert.IsTrue(!string.IsNullOrEmpty(instance.ObjectString));
                Assert.IsFalse(instance.IsStatic);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_ExecuteConstructor")]
        public void PluginRuntimeHandler_ExecuteConstructor_WhenClassIsSealed_ExpectRunsCorrectly()
        {
            //------------Setup for test--------------------------

            var type = typeof(Human);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "ToString", Parameters = new List<IMethodParameter>() } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<PluginRuntimeHandler>())
            {
                var pluginInvokeArgs = new PluginInvokeArgs
                {
                    MethodsToRun = svc.MethodsToRun,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IConstructorParameter>(),
                    },
                    AssemblyLocation = type.Assembly.Location,
                    AssemblyName = type.Assembly.FullName,
                    Fullname = type.FullName,

                };
                var instance = isolated.Value.ExecuteConstructor(new PluginExecutionDto(string.Empty)
                {
                    Args = pluginInvokeArgs
                });
                Assert.IsTrue(!string.IsNullOrEmpty(instance.ObjectString));
                Assert.IsFalse(instance.IsStatic);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_Test")]
        public void PluginRuntimeHandler_Test_WhenValid_ExpectRunsCorrectly()
        {
            //------------Setup for test--------------------------

            var type = typeof(Human);
            var svc = CreatePluginService(new List<IDev2MethodInfo> { new Dev2MethodInfo { Method = "EmptyIsNullTest", Parameters = new List<IMethodParameter>() } }, type, new ServiceConstructor());
            //------------Execute Test---------------------------
            var isolated = new PluginRuntimeHandler();

            var pluginInvokeArgs = new PluginInvokeArgs
            {
                MethodsToRun = svc.MethodsToRun,
                PluginConstructor = new PluginConstructor
                {
                    ConstructorName = svc.Constructor.Name,
                    Inputs = new List<IConstructorParameter>(),
                },
                AssemblyLocation = type.Assembly.Location,
                AssemblyName = type.Assembly.FullName,
                Fullname = type.FullName,
                Parameters = new List<MethodParameter>()
                {
                    new MethodParameter()
                    {
                        Name = "value",
                        TypeName = typeof(string).FullName,
                        Value = "a"
                    }
                },

                Method = "EmptyIsNullTest"

            };
            string jresult;
            var instance = isolated.Test(pluginInvokeArgs, out jresult);
            Assert.IsTrue(!string.IsNullOrEmpty(jresult));
            var count = instance.DataSourceShapes.Count;
            Assert.AreEqual(1, count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPropertiesJObject_GivenOracleCommand_ShouldRetunWithTwoProperties()
        {
            //---------------Set up test pack-------------------
            var runtimeHandler = typeof(PluginRuntimeHandler);
            PrivateType type = new PrivateType(runtimeHandler);
#pragma warning disable 618
            var type1 = typeof(OracleCommand);
#pragma warning restore 618
            //---------------Assert Precondition----------------
            var invokeStatic = type.InvokeStatic("GetPropertiesJObject", type1);
            //---------------Execute Test ----------------------
            var jObject = invokeStatic as JObject;
            //---------------Test Result -----------------------
            Assert.IsNotNull(jObject);
            var hasValues = jObject.HasValues;
            Assert.IsTrue(hasValues);
            Assert.AreEqual(8, jObject.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPropertiesJObject_GivenOracleCommand_ShouldHaveCorrectShape()
        {
            //---------------Set up test pack-------------------
            var runtimeHandler = typeof(PluginRuntimeHandler);
            PrivateType type = new PrivateType(runtimeHandler);
#pragma warning disable 618
            var type1 = typeof(OracleCommand);
#pragma warning restore 618
            //---------------Assert Precondition----------------
            var invokeStatic = type.InvokeStatic("GetPropertiesJObject", type1);
            //---------------Execute Test ----------------------
            var jObject = invokeStatic as JObject;
            Assert.IsNotNull(jObject);
            var hasValues = jObject.HasValues;
            Assert.IsTrue(hasValues);
            Assert.AreEqual(8, jObject.Count);
            //---------------Test Result -----------------------
            const string str = "{\"CommandText\":\"\",\"CommandTimeout\":\"\",\"CommandType\":\"\",\"Connection\":\"\",\"DesignTimeVisible\":\"\",\"Transaction\":\"\",\"UpdatedRowSource\":\"\",\"Site\":\"\"}";
            var s = jObject.ToString(Formatting.None);
            Assert.AreEqual(str, s);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_AdjustPluginResult")]
        public void PluginRuntimeHandler_AdjustPluginResult_WhenClassIsSealed_ExpectRunsCorrectly()
        {
            //------------Setup for test--------------------------

            var type = typeof(PluginRuntimeHandler);

            var methodInfo = type.GetMethod("AdjustPluginResult", BindingFlags.NonPublic | BindingFlags.Instance);
            var human = new Human();
            var memberInfo = human.GetType().GetMethod("ToString", BindingFlags.Instance | BindingFlags.Public);
            object result = "string";
            //------------Execute Test---------------------------
            var runtimeHandler = new PluginRuntimeHandler();
            var resultAdgusted = methodInfo.Invoke(runtimeHandler, new[] { result, memberInfo });
            Assert.AreEqual("<PrimitiveReturnValue>string</PrimitiveReturnValue>", resultAdgusted);
        }


        #region Helper Methods

        static PluginSource CreatePluginSource(Type type, bool nullLocation = false, bool invalidResourceID = false)
        {
            var assembly = type.Assembly;

            string loc = null;
            if (!nullLocation)
            {
                loc = assembly.Location;
            }

            Guid resourceID = Guid.Empty;
            if (!invalidResourceID)
            {
                resourceID = Guid.NewGuid();
            }

            return new PluginSource
            {
                AssemblyLocation = loc,
                ResourceID = resourceID,
                ResourceName = "Dummy",
                ResourceType = "PluginSource",
                AssemblyName = assembly.GetName().Name
            };
        }

        private static PluginService CreatePluginService()
        {
            return CreatePluginService(new List<IDev2MethodInfo>
            {
                new Dev2MethodInfo
                {
                    Method = "DummyMethod"
                }
            }, typeof(DummyClassForPluginTest));
        }

        private static PluginService CreatePluginService(List<IDev2MethodInfo> method, Type type, ServiceConstructor constructor = null)
        {
            var source = CreatePluginSource(typeof(DummyClassForPluginTest));
            var service = new PluginService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DummyPluginService",
                ResourceType = "PluginService",
                Namespace = type.FullName,
                MethodsToRun = method,
                Source = source,
                Constructor = constructor,

            };
            return service;
        }

        #endregion
    }
    public class Main
    {
        private readonly string _a;

        public Main(string a)
        {
            _a = a;
        }

        public string A
        {
            get
            {
                return _a;
            }
        }

        public void VoidMethod()
        {

        }

        public List<Main> Mains { get; set; }
    }
}
