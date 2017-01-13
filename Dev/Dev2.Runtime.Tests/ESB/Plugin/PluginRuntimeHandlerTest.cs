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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Runtime;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using TestingDotnetDllCascading;
using Warewolf.Core;

namespace Dev2.Tests.Runtime.ESB.Plugin
{
    /// <summary>
    /// Summary description for PluginRuntimeHandlerTest
    /// </summary>
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
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

        #endregion

        //#region Run

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_Run")]
        public void PluginRuntimeHandler_Run_WhenValidLocation_ExpectResult()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            var source = CreatePluginSource(typeof(DummyClassForPluginTest));
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = source.AssemblyName, Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = svc.Method.Parameters };
                var result = isolated.Value.Run(args);
                var castResult = JsonConvert.DeserializeObject<DummyClassForPluginTest>(result.ToString());
                //------------Assert Results-------------------------
                if (castResult != null)
                {
                    StringAssert.Contains(castResult.Name, "test data");
                }
                else
                {
                    Assert.Fail("Failed Conversion for Assert");
                }
            }


        }
        //uses Human as the class under test
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_Run")]
        public void PluginRuntimeHandler_Run_WhenDefaultConstructor_ExpectResult()
        {
            /*   public Human()
             {
                 Name = "Default";
                 PersonFood = new Food()
                 {
                     FoodName = "DefaultFood"
                 };
             }*/
            //------------Setup for test--------------------------
            var type = typeof(Human);
            var svc = CreatePluginService(new ServiceMethod() { Name = "ToString" }, type, new ServiceConstructor
            {
                Name = ".ctor",
            });
            var source = CreatePluginSource(type);
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs
                {
                    AssemblyLocation = source.AssemblyLocation
                    ,
                    AssemblyName = source.AssemblyName
                    ,
                    Fullname = svc.Namespace
                    ,
                    Method = svc.Method.Name
                    ,
                    Parameters = svc.Method.Parameters,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IServiceInput>()
                    },
                };
                foreach (var constructorParameter in svc.Constructor.Parameters)
                {
                    args.PluginConstructor.Inputs.Add(new ServiceInput(constructorParameter.Name, constructorParameter.Value));
                }


                var result = isolated.Value.Run(args);
                //------------Assert Results-------------------------
                if (result != null)
                {
                    StringAssert.Contains(result.ToString(), "Default");
                    StringAssert.Contains(result.ToString(), "DefaultFood");
                }
                else
                {
                    Assert.Fail("Failed Conversion for Assert");
                }
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginRuntimeHandler_Run")]
        public void PluginRuntimeHandler_Run_WhenOptionalConstructor_ExpectResult()
        {
            /*  
                public Human(string name)
            {
                Name = name;
                PersonFood = new Food()
                {
                    FoodName = "DefaultFood"
                };
            }
             */
            //------------Setup for test--------------------------
            var type = typeof(Human);
            var svc = CreatePluginService(new ServiceMethod() { Name = "ToString" }, type, new ServiceConstructor
            {
                Name = ".ctor",
                Parameters = new List<ConstructorParameter>()
                    {
                        new ConstructorParameter()
                        {
                            Name = "name",
                            TypeName = typeof(string).AssemblyQualifiedName,
                            Value = "Micky",

                        }
                    }
            });
            var source = CreatePluginSource(type);
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs
                {
                    AssemblyLocation = source.AssemblyLocation
                    ,
                    AssemblyName = source.AssemblyName
                    ,
                    Fullname = svc.Namespace
                    ,
                    Method = svc.Method.Name
                    ,
                    Parameters = svc.Method.Parameters,
                    PluginConstructor = new PluginConstructor
                    {
                        ConstructorName = svc.Constructor.Name,
                        Inputs = new List<IServiceInput>()
                    },
                };
                foreach (var constructorParameter in svc.Constructor.Parameters)
                {
                    args.PluginConstructor.Inputs.Add(new ServiceInput(constructorParameter.Name, constructorParameter.Value));
                }


                var result = isolated.Value.Run(args);
                //------------Assert Results-------------------------
                if (result != null)
                {
                    StringAssert.Contains(result.ToString(), "Default");
                    StringAssert.Contains(result.ToString(), "Micky");
                }
                else
                {
                    Assert.Fail("Failed Conversion for Assert");
                }
            }
        }

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_Run")]
        //[ExpectedException(typeof(NullReferenceException))]
        //public void PluginRuntimeHandler_Run_WhenNullLocation_ExpectException()
        //{
        //    //------------Setup for test--------------------------
        //    var svc = CreatePluginService();
        //    using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
        //    {
        //        PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = null, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = svc.Method.Parameters };
        //        isolated.Value.Run(args);
        //    }
        //}

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_Run")]
        //[ExpectedException(typeof(NullReferenceException))]
        //public void PluginRuntimeHandler_Run_WhenInvalidNamespace_ExpectException()
        //{
        //    //------------Setup for test--------------------------
        //    var svc = CreatePluginService();
        //    var source = CreatePluginSource(typeof(DummyClassForPluginTest));
        //    using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
        //    {
        //        PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = "Foo", Fullname = "foo.bar", Method = svc.Method.Name, Parameters = svc.Method.Parameters };
        //        isolated.Value.Run(args);
        //    }
        //}

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_Run")]
        //public void PluginRuntimeHandler_Run_WhenInvalidMethod_ExpectException()
        //{
        //    //------------Setup for test--------------------------
        //    var svc = CreatePluginService();
        //    var source = CreatePluginSource(typeof(DummyClassForPluginTest));
        //    //------------Execute Test---------------------------
        //    using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
        //    {
        //        PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = "Foo", Fullname = svc.Namespace, Method = "InvalidName", Parameters = svc.Method.Parameters };
        //        var run = isolated.Value.Run(args);
        //        Assert.IsNotNull(run);
        //    }
        //}

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_Run")]
        //[ExpectedException(typeof(NullReferenceException))]
        //public void PluginRuntimeHandler_Run_WhenNullParameters_ExpectException()
        //{
        //    //------------Setup for test--------------------------
        //    var svc = CreatePluginService();
        //    var source = CreatePluginSource(typeof(DummyClassForPluginTest));
        //    //------------Execute Test---------------------------
        //    using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
        //    {
        //        PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = null };
        //        isolated.Value.Run(args);
        //    }
        //}

        //#endregion



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

        //[TestMethod]
        //[Owner("Nkosinathi Sangweni")]
        //[TestCategory("PluginRuntimeHandler_CreateInstance")]
        //public void PluginRuntimeHandler_CreateInstance_WhenHuman_ExpectHumanStringObject()
        //{
        //    ------------Setup for test--------------------------

        //    var type = typeof(Human);
        //    var svc = CreatePluginService(new List<Dev2MethodInfo> { new Dev2MethodInfo { Method = "ToString" } }, type, new ServiceConstructor());
        //    ------------Execute Test-------------------------- -
        //    using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
        //    {
        //        var instance = isolated.Value.CreateInstance(new PluginInvokeArgs
        //        {
        //            MethodsToRun = svc.MethodsToRun,
        //            PluginConstructor = new PluginConstructor
        //            {
        //                ConstructorName = svc.Constructor.Name,
        //                Inputs = new List<IServiceInput>(),

        //            },
        //            AssemblyLocation = type.Assembly.Location,
        //            AssemblyName = type.Assembly.FullName,
        //            Fullname = type.FullName,

                });

        //        var deserializeToObject = instance.DeserializeToObject<Human>();
        //        Assert.IsNotNull(deserializeToObject);
        //    }
        //}

        //[TestMethod]
        //[Owner("Nkosinathi Sangweni")]
        //[TestCategory("PluginRuntimeHandler_CreateInstance")]
        //public void PluginRuntimeHandler_Run_WhenObjectStringIsNotNull_ExpectRunsCorrectly()
        //{
        //    //------------Setup for test--------------------------

        //    var type = typeof(Human);
        //    var svc = CreatePluginService(new List<Dev2MethodInfo> { new Dev2MethodInfo { Method = "ToString" , Parameters = new List<MethodParameter>()} }, type, new ServiceConstructor());
        //    //------------Execute Test---------------------------
        //    using (var isolated = new Isolated<PluginRuntimeHandler>())
        //    {
        //        var pluginInvokeArgs = new PluginInvokeArgs
        //        {
        //            MethodsToRun = svc.MethodsToRun,
        //            PluginConstructor = new PluginConstructor
        //            {
        //                ConstructorName = svc.Constructor.Name,
        //                Inputs = new List<IServiceInput>(),
        //            },
        //            AssemblyLocation = type.Assembly.Location,
        //            AssemblyName = type.Assembly.FullName,
        //            Fullname = type.FullName,

                };
                var instance = isolated.Value.CreateInstance(pluginInvokeArgs, type.Assembly);

        //        var deserializeToObject = instance.DeserializeToObject<Human>();
        //        Assert.IsNotNull(deserializeToObject);

        //        var run = isolated.Value.Run(instance, pluginInvokeArgs);
        //        Assert.IsNotNull(run);
        //        StringAssert.Contains(run.ToString(),"Default");
        //    }
        //}

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
            return CreatePluginService(new List<Dev2MethodInfo>
            {
                new Dev2MethodInfo
                {
                    Method = "DummyMethod"
                }
            }, typeof(DummyClassForPluginTest));
        }

        private static PluginService CreatePluginService(List<Dev2MethodInfo> method, Type type, ServiceConstructor constructor = null)
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
    }
}
