using System;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;

namespace Dev2.Tests.Activities.ActivityComparerTests.DotNet_Dll
{
    [TestClass]
    public class DsfEnhancedDotNetDllActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId };
            var activity1 = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId };
            var activity1 = new DsfEnhancedDotNetDllActivity { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId, DisplayName = "a" };
            var activity1 = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId, DisplayName = "A" };
            var activity1 = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var activity1 = new DsfEnhancedDotNetDllActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Method_DifferentName_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a"
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "b"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction }
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "aaa",
                MethodsToRun = new List<IPluginAction> { pluginActiona }
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Method_SameName_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a"
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction }
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona }
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Method_DifferentIsObject_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = false
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction }
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "aaa",
                MethodsToRun = new List<IPluginAction> { pluginActiona }
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Method_SameIsObject_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction }
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginActiona }
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Method_DifferentOutputVariable_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]"
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[b]]"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction }
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginActiona }
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Method_SameOutputVariable_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]"
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction }
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona }
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Method_DifferentInputs_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput()
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput(), new ServiceInput()
                }
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction }
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "aaa"
                ,
                MethodsToRun = new List<IPluginAction> { pluginActiona }
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Method_SameInputs_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction }
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona }
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameAssemblyLocation_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals); ;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameAssemblyName_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameFullName_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_SameConstructorName_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            IPluginConstructor constructor = new PluginConstructor()
            {
                ConstructorName = "a"
            };
            IPluginConstructor constructor1 = new PluginConstructor()
            {
                ConstructorName = "a"
            };
            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem,
                Constructor = constructor
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1,
                Constructor = constructor1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_SameIsExistingObject_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            IPluginConstructor constructor = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false
            };
            IPluginConstructor constructor1 = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false
            };
            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem,
                Constructor = constructor
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1,
                Constructor = constructor1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_SameReturnObject_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            IPluginConstructor constructor = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false,
                ReturnObject = "{0}"
            };
            IPluginConstructor constructor1 = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false,
                ReturnObject = "{0}"
            };
            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem,
                Constructor = constructor
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1,
                Constructor = constructor1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_SameInputs_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            IPluginConstructor constructor = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false,
                ReturnObject = "{0}",
                Inputs = new List<IConstructorParameter>()
                {
                    new ConstructorParameter(){Name = "a", Value = "a", IsObject = true, EmptyToNull = true}
                }
            };
            IPluginConstructor constructor1 = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false,
                ReturnObject = "{0}",
                Inputs = new List<IConstructorParameter>()
                {
                    new ConstructorParameter(){Name = "a", Value = "a", IsObject = true, EmptyToNull = true}
                }
            };
            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem,
                Constructor = constructor
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1,
                Constructor = constructor1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_DifferentConstructorName_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            IPluginConstructor constructor = new PluginConstructor()
            {
                ConstructorName = "aa"
            };
            IPluginConstructor constructor1 = new PluginConstructor()
            {
                ConstructorName = "a"
            };
            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem,
                Constructor = constructor
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1,
                Constructor = constructor1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_DifferentIsExistingObject_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            IPluginConstructor constructor = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = true
            };
            IPluginConstructor constructor1 = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false
            };
            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem,
                Constructor = constructor
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1,
                Constructor = constructor1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_DifferentReturnObject_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            IPluginConstructor constructor = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false,
                ReturnObject = ""
            };
            IPluginConstructor constructor1 = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false,
                ReturnObject = "{0}"
            };
            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem,
                Constructor = constructor
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1,
                Constructor = constructor1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_DifferentInputs_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };
            IPluginConstructor constructor = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false,
                ReturnObject = "{0}",
                Inputs = new List<IConstructorParameter>()
                {
                    new ConstructorParameter(){Name = "B", Value = "B", IsObject = true, EmptyToNull = true}
                }
            };
            IPluginConstructor constructor1 = new PluginConstructor()
            {
                ConstructorName = "a",
                IsExistingObject = false,
                ReturnObject = "{0}",
                Inputs = new List<IConstructorParameter>()
                {
                    new ConstructorParameter(){Name = "a", Value = "a", IsObject = true, EmptyToNull = true}
                }
            };
            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem,
                Constructor = constructor
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1,
                Constructor = constructor1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameMethodName_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_SameJsonObject_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod",
                JsonObject = "{}"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod",
                JsonObject = "{}"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_DifferentAssemblyName_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "b"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_DifferentFullName_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "b"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_DifferentMethodName_Object_Is_not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "a"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NameSpace_DifferentJsonObject_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            IPluginAction pluginAction = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                   new ServiceInput {Name = "a"},
                   new ServiceInput {Name = "b"},
               }
            };
            IPluginAction pluginActiona = new PluginAction
            {
                Method = "a",
                IsObject = true,
                OutputVariable = "[[a]]",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput {Name = "a"},
                    new ServiceInput {Name = "b"},
                }
            };

            INamespaceItem namespaceItem = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod",
                JsonObject = "{\"name\":\"name\"}"
            };

            INamespaceItem namespaceItem1 = new NamespaceItem
            {
                AssemblyLocation = "a",
                AssemblyName = "a",
                FullName = "a",
                MethodName = "MyMethod",
                JsonObject = "{}"
            };
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA"
                ,
                MethodsToRun = new List<IPluginAction> { pluginAction },
                Namespace = namespaceItem
            };
            var activity1 = new DsfEnhancedDotNetDllActivity
            {
                UniqueID = uniqueId
                ,
                DisplayName = "AAA",
                MethodsToRun = new List<IPluginAction> { pluginActiona },
                Namespace = namespaceItem1
            };
            //---------------Assert DsfEnhancedDotNetDllActivity----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
    }
}
