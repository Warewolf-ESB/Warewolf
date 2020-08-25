/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TestingDotnetDllCascading;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DsfEnhancedDotNetDllActivityNewTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_Namespace_IsNull_ExpectNoNamespaceSelectedError()
        {
            var sut = new DsfEnhancedDotNetDllActivityStub();

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, new Mock<IDSFDataObject>().Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual(ErrorResource.NoNamespaceSelected, errors.FetchErrors().First()); 
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AssemblyLocation_IsNotFound_ExpectNotFoundError()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = "bad AssemblyLocation",
                });

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new Mock<INamespaceItem>().Object,
                ResourceCatalog = resourceCatalog.Object,
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, new Mock<IDSFDataObject>().Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual("Assembly Name Not found", errors.FetchErrors().First());

        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AssemblyLocation_IsFound_And_CTOR_NotSetup_ExpectInvalidError()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new Mock<INamespaceItem>().Object,
                ResourceCatalog = resourceCatalog.Object,
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, new Mock<IDSFDataObject>().Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual("Namespace is invalid", errors.FetchErrors().First());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AssemblyLocation_IsFound_And_BadNamespace_ExpectError()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = "bad name" 
                },
                ResourceCatalog = resourceCatalog.Object,
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, new Mock<IDSFDataObject>().Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual("bad name is invalid type name", errors.FetchErrors().First());

        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AssemblyLocation_IsFound_And_GoodNamespace_ExpectNoError()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName
                },
                ResourceCatalog = resourceCatalog.Object,
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, new Mock<IDSFDataObject>().Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            Assert.IsFalse(errors.HasErrors());

        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AssemblyLocation_IsFound_And_ObjectName_IsNotSet_ExpectNoErrors()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(new ExecutionEnvironment());

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = string.Empty
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AssemblyLocation_IsFound_And_ObjectName_IsSet_ExpectJsonStringObject()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(new ExecutionEnvironment());

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "outputObjectName"
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AssemblyLocation_IsFound_And_MethodToRun_IsInvalid_ExpectNoErrors()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(new ExecutionEnvironment());

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "outputObjectName",
                MethodsToRun = new List<IPluginAction>
                {
                    { new PluginAction { Method = "bad method name" } }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            Assert.IsFalse(errors.HasErrors());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_NoParams_CTOR_And_MethodToRun_IsValid_ExpectNoErrorsDefaultName()
        {
            var humanType = typeof(Human);
            var env = new ExecutionEnvironment();
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                MethodsToRun = new List<IPluginAction>
                {
                    { new PluginAction { Method = "FavouriteFoods" } }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(result.Item.IsJsonObject);

            var jsonStringObject = (result.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisHuman = jsonStringObject.ToObject(humanType) as Human;

            Assert.IsFalse(errors.HasErrors());

            Assert.IsTrue(thisHuman.Name is "Default");
            Assert.IsTrue(thisHuman.PersonFood.FoodName is "DefaultFood");
            Assert.IsTrue(thisHuman.FavouriteFoodsProperty.Count > 3);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_WithInvalidParam_CTOR_And_MethodToRun_IsValid_ExpectExceptionAsError()
        {
            var humanType = typeof(Human);
            var env = new ExecutionEnvironment();
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                ConstructorInputs = new List<IServiceInput> 
                {
                    { 
                        new ServiceInput 
                        { 
                            Name = "bad param name", 
                            Value = "asdValue" 
                        } 
                    }
                },
                MethodsToRun = new List<IPluginAction>
                {
                    { new PluginAction { Method = "FavouriteFoods" } }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsFalse(result.Item.IsJsonObject);

            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual("Constructor on type 'TestingDotnetDllCascading.Human' not found.", errors.FetchErrors().First());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_WithValid_StringType_Param_CTOR_And_MethodToRun_IsValid_ExpectNoErrors()
        {
            var humanType = typeof(Human);
            var env = new ExecutionEnvironment();
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                ConstructorInputs = new List<IServiceInput>
                {
                    {
                        new ServiceInput
                        {
                            Name = "name",
                            Value = "Lucky",
                            TypeName = typeof(string).AssemblyQualifiedName,
                            RequiredField = true
                        }
                    }

                },
                MethodsToRun = new List<IPluginAction>
                {
                    { new PluginAction { Method = "FavouriteFoods" } }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(result.Item.IsJsonObject);

            var jsonStringObject = (result.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisHuman = jsonStringObject.ToObject(humanType) as Human;

            Assert.IsFalse(errors.HasErrors());

            Assert.IsTrue(thisHuman.Name is "Lucky");
            Assert.IsTrue(thisHuman.PersonFood.FoodName is "DefaultFood");
            Assert.IsTrue(thisHuman.FavouriteFoodsProperty.Count > 3);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_Existing_CTOR_And_MethodToRun_IsValid_ExpectMethodResults()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();

            var human = new Human("Bob", "Marley", new Food { FoodName = "Lettuce" });
            var humanString = DataListUtil.ConvertModelToJson(human).ToString();

            var env = new ExecutionEnvironment();
            env.Assign("[[@existingInstance]]", humanString, 0);

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                Constructor = new PluginConstructor
                {
                    ConstructorName = "[[@existingInstance]]",
                    ID = Guid.Empty,
                    IsExistingObject = true,
                },
                MethodsToRun = new List<IPluginAction>
                {
                    { 
                        new PluginAction
                        {
                            Method = "EmptyIsNullTest",
                            Inputs = new List<IServiceInput>
                            {
                                { 
                                    new ServiceInput 
                                    { 
                                        Name = "value",
                                        Value = "method paramater value",
                                        TypeName = typeof(string).AssemblyQualifiedName,
                                    } 
                                }
                            },
                        }
                    }
                },
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(result.Item.IsJsonObject);

            var jsonStringObject = (result.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisHuman = jsonStringObject.ToObject(humanType) as Human;

            Assert.IsFalse(errors.HasErrors());

            Assert.IsTrue(thisHuman.Name is "Bob");
            Assert.IsTrue(thisHuman.SurName is "Marley");
            Assert.IsTrue(thisHuman.PersonFood.FoodName is "Lettuce");

            var methodResult = sut.MethodsToRun.First().MethodResult;
            Assert.AreEqual("method paramater value", methodResult);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_WithValid_CustomType_Param_CTOR_And_MethodToRun_IsValid_ExpectNoErrors()
        {
            var humanType = typeof(Human);
            var env = new ExecutionEnvironment();
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                ConstructorInputs = new List<IServiceInput>
                {
                    { 
                        new ServiceInput 
                        { 
                            Name = "name", 
                            Value = "Lucky", 
                            TypeName = typeof(string).AssemblyQualifiedName,
                            RequiredField = true
                        }
                    },
                    {
                        new ServiceInput
                        {
                            Name = "surname",
                            Value = "Dube",
                            TypeName = typeof(string).AssemblyQualifiedName,
                            RequiredField = true
                        }
                    },
                    {
                        new ServiceInput
                        {
                            Name = "food",
                            Value = JsonConvert.SerializeObject(new Food
                            {
                                FoodName = "Lucky Beans"
                            }),
                            TypeName = typeof(Food).AssemblyQualifiedName,
                            RequiredField = true
                        }
                    }

                },
                MethodsToRun = new List<IPluginAction>
                {
                    { new PluginAction { Method = "FavouriteFoods" } }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(result.Item.IsJsonObject);

            var jsonStringObject = (result.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisHuman = jsonStringObject.ToObject(humanType) as Human;

            Assert.IsFalse(errors.HasErrors());

            Assert.IsTrue(thisHuman.Name is "Lucky");
            Assert.IsTrue(thisHuman.SurName is "Dube");
            Assert.IsTrue(thisHuman.PersonFood.FoodName is "Lucky Beans");
            Assert.IsTrue(thisHuman.FavouriteFoodsProperty.Count is 9);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_WithValid_CustomType_Param_CTOR_And_MethodToRun_IsValidAndStatic_ExpectNoErrors()
        {
            var humanType = typeof(Human);
            var env = new ExecutionEnvironment();
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                ConstructorInputs = new List<IServiceInput>
                {
                    {
                        new ServiceInput
                        {
                            Name = "name",
                            Value = "Lucky",
                            TypeName = typeof(string).AssemblyQualifiedName,
                            RequiredField = true
                        }
                    },
                    {
                        new ServiceInput
                        {
                            Name = "surname",
                            Value = "Dube",
                            TypeName = typeof(string).AssemblyQualifiedName,
                            RequiredField = true
                        }
                    },
                    {
                        new ServiceInput
                        {
                            Name = "food",
                            Value = JsonConvert.SerializeObject(new Food
                            {
                                FoodName = "Lucky Beans"
                            }),
                            TypeName = typeof(Food).AssemblyQualifiedName,
                            RequiredField = true
                        }
                    }

                },
                MethodsToRun = new List<IPluginAction>
                {
                    { 
                        new PluginAction 
                        { 
                            Method = "Sum", 
                            Inputs = new List<IServiceInput>
                            {
                                {
                                    new ServiceInput
                                    {
                                        Name = "num1",
                                        Value = "2",
                                        TypeName = typeof(int).AssemblyQualifiedName,
                                        RequiredField = true
                                        
                                    }
                                },
                                {
                                    new ServiceInput
                                    {
                                        Name = "num2",
                                        Value = "3",
                                        TypeName = typeof(int).AssemblyQualifiedName,
                                        RequiredField = true
                                    }
                                }
                            }
                        } 
                    }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(result.Item.IsJsonObject);

            var jsonStringObject = (result.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisHuman = jsonStringObject.ToObject(humanType) as Human;

            Assert.IsFalse(errors.HasErrors());

            Assert.IsTrue(thisHuman.Name is "Lucky");
            Assert.IsTrue(thisHuman.SurName is "Dube");
            Assert.IsTrue(thisHuman.PersonFood.FoodName is "Lucky Beans");
            Assert.IsTrue(thisHuman.FavouriteFoodsProperty.Count is 9);

            Assert.AreEqual("5", sut.MethodsToRun.First().MethodResult);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_StaticClass_ExpectNoErrors()
        {
            var staticClassType = typeof(StaticClass);
            var env = new ExecutionEnvironment();
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = staticClassType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = staticClassType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            Method = "ToStringOnStatic",
                        }
                    }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsFalse(result.Item.IsJsonObject);

            Assert.IsFalse(errors.HasErrors());

            Assert.AreEqual("ToStringOnStatic", sut.MethodsToRun.First().MethodResult);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_SealedClass_MethodsToRun_HasError_ExpectError()
        {
            var sealesClassType = typeof(SealedClass);
            var env = new ExecutionEnvironment();
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = sealesClassType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = sealesClassType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            Method = "UnkownMethodName",
                        }
                    }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(result.Item.IsJsonObject);

            var jsonStringObject = (result.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisSealedClass = jsonStringObject.ToObject(sealesClassType) as SealedClass;
            Assert.IsNotNull(thisSealedClass);

            Assert.IsFalse(errors.HasErrors());

            var methodResult = sut.MethodsToRun.First();

            Assert.IsTrue(methodResult.HasError);
            Assert.AreEqual("UnkownMethodName not found", methodResult.ErrorMessage);
            Assert.IsNull(methodResult.MethodResult);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_SealedClass_MethodsToRun_NotHasError_ExpectNoError()
        {
            var sealesClassType = typeof(SealedClass);
            var env = new ExecutionEnvironment();
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = sealesClassType.Assembly.Location,
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = sealesClassType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            Method = "ToString",
                        }
                    }
                }
            };

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(result.Item.IsJsonObject);

            var jsonStringObject = (result.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisSealedClass = jsonStringObject.ToObject(sealesClassType) as SealedClass;
            Assert.IsNotNull(thisSealedClass);
            
            Assert.IsFalse(errors.HasErrors());

            Assert.AreEqual("ToStringOnsealed", sut.MethodsToRun.First().MethodResult);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AddToJsonObjects_ThrowsCannotConvertJson_ExpectExceptionAsError()
        {
            var sealesClassType = typeof(SealedClass);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = sealesClassType.Assembly.Location,
                });

           
            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = sealesClassType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            Method = "ToString",
                        }
                    }
                }
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(o => o.AddToJsonObjects(sut.ObjectName, It.IsAny<JContainer>()))
                .Throws(new Exception("Cannot convert given JSON to target type"));

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(mockExecutionEnvironment.Object);

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var env = mockExecutionEnvironment.Object;
            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNull(result);

            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual("Cannot convert given JSON to target type\r\nCannot convert given JSON to target type", errors.FetchErrors().First());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_AddToJsonObjects_ThrowsForOtherReason_ExpectExceptionAsError()
        {
            var sealesClassType = typeof(SealedClass);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = sealesClassType.Assembly.Location,
                });


            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = sealesClassType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            Method = "ToString",
                        }
                    }
                }
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(o => o.AddToJsonObjects(sut.ObjectName, It.IsAny<JContainer>()))
                .Throws(new Exception("false exception: test if other exception messages"));

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(mockExecutionEnvironment.Object);

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var env = mockExecutionEnvironment.Object;
            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNull(result);

            Assert.IsTrue(errors.HasErrors());
            Assert.AreEqual("false exception: test if other exception messages", errors.FetchErrors().First());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_IsServiceTestExecution_ServiceTestStep_IsNull_ExpectNoError()
        {
            var sealesClassType = typeof(SealedClass);
            var sourceId = Guid.NewGuid();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = sealesClassType.Assembly.Location,
                });


            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = sealesClassType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            Method = "ToString",
                        }
                    }
                }
            };

            var env = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);
            mockDataObject.Setup(o => o.IsServiceTestExecution)
                .Returns(true);

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(result);

            var jsonStringObject = (result.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisSealedClass = jsonStringObject.ToObject(sealesClassType) as SealedClass;
            Assert.IsNotNull(thisSealedClass);

            Assert.IsFalse(errors.HasErrors());

            var methodResult = sut.MethodsToRun.First();
            Assert.AreEqual("ToStringOnsealed", methodResult.MethodResult);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_IsServiceTestExecution_ServiceTestStep_IsNotNull_And_MethodsToRun_IsNotObject_ExpectNoError()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();
            var activityID = Guid.NewGuid();

            var testValue = JsonConvert.SerializeObject(new Human());
            var testVariable = "[[@testVariable]]";

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });


            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                Constructor = new PluginConstructor
                {
                    ID = activityID,
                    ConstructorName = "[[@testConstructor]]"
                },
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            ID = activityID,
                            Method = "SetNameInternal",
                            IsObject = false
                        }
                    }
                }
            };

            var env = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);
            mockDataObject.Setup(o => o.IsServiceTestExecution)
                .Returns(true);
            mockDataObject.Setup(o => o.ServiceTest)
                .Returns(new ServiceTestModelTO 
                {
                    TestSteps = new List<IServiceTestStep>
                    {
                        { 
                            new ServiceTestStepTO
                            {
                                ActivityID = activityID,
                                UniqueID = activityID,
                                Children = new ObservableCollection<IServiceTestStep>
                                {
                                    {
                                       new ServiceTestStepTO
                                       {
                                           ActivityID = activityID,
                                           UniqueID = activityID,
                                           StepOutputs = new ObservableCollection<IServiceTestOutput>
                                           {
                                               {
                                                   new ServiceTestOutputTO
                                                   {
                                                       Variable = testVariable,
                                                       Value = testValue,
                                                   }
                                               }
                                           }
                                       }
                                    }
                                }
                            }
                        }
                    }
                });

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(result);

            Assert.IsFalse(errors.HasErrors());

            var methodResult = sut.MethodsToRun.First();
            Assert.AreEqual(testValue, methodResult.MethodResult);
            
            var testOutput = env.EvalForJson(testVariable) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(testVariable);

            var jsonStringObject = (testOutput.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisHumanClass = jsonStringObject.ToObject(humanType) as Human;

            Assert.AreEqual("Default", thisHumanClass.Name);
            Assert.AreEqual("DefaultFood", thisHumanClass.PersonFood.FoodName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_IsServiceTestExecution_ServiceTestStep_HasInvalid_ServiceTestOutputTO_Variable_Expect()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();
            var activityID = Guid.NewGuid();

            var testValue = "some bad string value";
            var testVariable = "[[@testVariable]]";

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });


            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                Constructor = new PluginConstructor
                {
                    ID = activityID,
                    ConstructorName = "[[@testConstructor]]"
                },
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            ID = activityID,
                            Method = "SetNameInternal",
                            IsObject = false
                        }
                    }
                }
            };

            var env = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);
            mockDataObject.Setup(o => o.IsServiceTestExecution)
                .Returns(true);
            mockDataObject.Setup(o => o.ServiceTest)
                .Returns(new ServiceTestModelTO
                {
                    TestSteps = new List<IServiceTestStep>
                    {
                        {
                            new ServiceTestStepTO
                            {
                                ActivityID = activityID,
                                UniqueID = activityID,
                                Children = new ObservableCollection<IServiceTestStep>
                                {
                                    {
                                       new ServiceTestStepTO
                                       {
                                           ActivityID = activityID,
                                           UniqueID = activityID,
                                           StepOutputs = new ObservableCollection<IServiceTestOutput>
                                           {
                                               {
                                                   new ServiceTestOutputTO
                                                   {
                                                       Variable = testVariable,
                                                       Value = testValue,
                                                   }
                                               }
                                           }
                                       }
                                    }
                                }
                            }
                        }
                    }
                });

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(result);

            Assert.IsFalse(errors.HasErrors());

            var methodResult = sut.MethodsToRun.First();
            Assert.AreEqual(testValue, methodResult.MethodResult);

            var testOutput = env.EvalForJson(testVariable) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsTrue(testOutput.Item.IsNothing);

            Assert.IsTrue(env.HasErrors());
            Assert.AreEqual("Unexpected character encountered while parsing value: s. Path '', line 0, position 0.", env.Errors.First());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfEnhancedDotNetDllActivityNew))]
        public void DsfEnhancedDotNetDllActivityNew_ExecutionImpl_Given_IsServiceTestExecution_ServiceTestStep_IsNotNull_And_MethodsToRun_IsObject_ExpectNoError()
        {
            var humanType = typeof(Human);
            var sourceId = Guid.NewGuid();
            var activityID = Guid.NewGuid();

            var testValue = JsonConvert.SerializeObject(new Human());
            var testVariable = "[[@testVariable]]";

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(new PluginSource
                {
                    AssemblyLocation = humanType.Assembly.Location,
                });


            var sut = new DsfEnhancedDotNetDllActivityStub
            {
                SourceId = sourceId,
                Namespace = new NamespaceItem
                {
                    FullName = humanType.FullName,
                },
                ResourceCatalog = resourceCatalog.Object,
                ObjectName = "[[@outputObjectName]]",
                Constructor = new PluginConstructor
                {
                    ID = activityID,
                    ConstructorName = "[[@testConstructor]]"
                },
                MethodsToRun = new List<IPluginAction>
                {
                    {
                        new PluginAction
                        {
                            ID = activityID,
                            Method = "SetNameInternal",
                            IsObject = true
                        }
                    }
                }
            };

            var env = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);
            mockDataObject.Setup(o => o.IsServiceTestExecution)
                .Returns(true);
            mockDataObject.Setup(o => o.ServiceTest)
                .Returns(new ServiceTestModelTO
                {
                    TestSteps = new List<IServiceTestStep>
                    {
                        {
                            new ServiceTestStepTO
                            {
                                ActivityID = activityID,
                                UniqueID = activityID,
                                Children = new ObservableCollection<IServiceTestStep>
                                {
                                    {
                                       new ServiceTestStepTO
                                       {
                                           ActivityID = activityID,
                                           UniqueID = activityID,
                                           StepOutputs = new ObservableCollection<IServiceTestOutput>
                                           {
                                               {
                                                   new ServiceTestOutputTO
                                                   {
                                                       Variable = testVariable,
                                                       Value = testValue,
                                                   }
                                               }
                                           }
                                       }
                                    }
                                }
                            }
                        }
                    }
                });

            sut.ExecutionImplStub(new Mock<IEsbChannel>().Object, mockDataObject.Object, string.Empty, string.Empty, out ErrorResultTO errors, 0);

            var result = env.EvalForJson(sut.ObjectName) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(result);

            Assert.IsFalse(errors.HasErrors());

            var methodResult = sut.MethodsToRun.First();
            Assert.AreEqual(testValue, methodResult.MethodResult);

            var testOutput = env.EvalForJson(testVariable) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(testVariable);

            var jsonStringObject = (testOutput.Item as DataStorage.WarewolfAtom.JsonObject).Item;
            var thisHumanClass = jsonStringObject.ToObject(humanType) as Human;

            Assert.AreEqual("Default", thisHumanClass.Name);
            Assert.AreEqual("DefaultFood", thisHumanClass.PersonFood.FoodName);
        }

    }

    internal class DsfEnhancedDotNetDllActivityStub : DsfEnhancedDotNetDllActivityNew
    {
        public DsfEnhancedDotNetDllActivityStub()
        {
        }

        public void ExecutionImplStub(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
        }

    }
}
