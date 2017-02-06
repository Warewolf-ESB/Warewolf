using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Net_Dll_Enhanced;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestingDotnetDllCascading;
using Warewolf.Core;
using Warewolf.Testing;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.DotNetDll
{
    [TestClass]
    public class DotNetDllEnhancedViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DotNetDllEnhancedViewModel_Constructor_NullModelItem_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new DotNetDllEnhancedViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_Constructor")]
        public void DotNetDllEnhancedViewModel_Constructor_Valid_ShouldSetupViewModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------
            var vm = new DotNetDllEnhancedViewModel(CreateModelItem(), ps.Object);
            vm.MethodRegion = new DotNetMethodRegion();
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.ModelItem);
            Assert.IsTrue(vm.HasLargeView);
            Assert.AreEqual(46, vm.LabelWidth);
            Assert.AreEqual("Done", vm.ButtonDisplayValue);
            Assert.IsTrue(vm.ShowLarge);
            Assert.AreEqual(Visibility.Visible, vm.ThumbVisibility);
            Assert.AreEqual(Visibility.Collapsed, vm.ShowExampleWorkflowLink);
            Assert.IsNotNull(vm.DesignValidationErrors);
            Assert.IsNotNull(vm.FixErrorsCommand);
            Assert.IsNotNull(vm.SourceRegion);
            Assert.IsNotNull(vm.NamespaceRegion);
            Assert.IsNotNull(vm.MethodRegion);
            Assert.IsNotNull(vm.InputArea);
            Assert.IsNotNull(vm.OutputsRegion);
            Assert.IsNotNull(vm.ErrorRegion);
            Assert.IsNotNull(vm.Regions);
            Assert.IsTrue(vm.OutputsRegion.OutputMappingEnabled);
            Assert.IsNotNull(vm.Properties);
            Assert.AreEqual(1, vm.Properties.Count);
        }
        
       

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_ToModel")]
        public void DotNetDllEnhancedViewModel_ToModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            var modelx = vm.ToModel();
            Assert.IsNotNull(modelx);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_ToModel")]
        public void DotNetDllEnhancedViewModel_ClearValidationMessage()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.ClearValidationMemoWithNoFoundError();
            Assert.AreEqual(vm.DesignValidationErrors[0].Message, string.Empty);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_SetDisplayName")]
        public void DotNetDllEnhancedViewModel_SetDisplayName()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.SetDisplayName("dsfbob_builer");
            PrivateObject p = new PrivateObject(vm);
            Assert.AreEqual(p.GetProperty("DisplayName"), "DotNet DLLdsfbob_builer");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetDllEnhancedViewModel_Handle")]
        public void DotNetDllEnhancedViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var ps = SetupEmptyMockSource();
            var viewModel = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_SetDisplayName")]
        public void DotNetDllEnhancedViewModel_ErrorMessage()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.ErrorMessage(new AccessViolationException("bob"), true);
            Assert.IsTrue(vm.Errors.Count > 0);
            Assert.AreEqual("bob", vm.Errors[0].Message);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_SetDisplayName")]
        public void DotNetDllEnhancedViewModel_FixErrors()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.FixErrorsCommand.Execute(null);
            Assert.IsTrue(vm.IsWorstErrorReadOnly);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_SetDisplayName")]
        public void DotNetDllEnhancedViewModel_UpdateWorstDesignError()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);
            vm.DesignValidationErrors.Add(new ErrorInfo { Message = "bob error", ErrorType = ErrorType.Critical });
            PrivateObject p = new PrivateObject(vm);
            p.Invoke("UpdateWorstError");
            var inf = p.GetProperty("WorstDesignError") as ErrorInfo;
            //------------Assert Results-------------------------

            Assert.IsNotNull(inf);
            Assert.AreEqual("bob error", inf.Message);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildRegions_GivenNamespacesRegionHasErrors_ShouldhaveErrors()
        {
            //---------------Set up test pack-------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);

            var ps = new Mock<IPluginServiceModel>();
            ps.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource> { new PluginSourceDefinition { Id = id } });
            ps.Setup(a => a.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Throws(new BadImageFormatException());
            ps.Setup(a => a.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new ObservableCollection<IPluginAction> { new PluginAction { FullName = "bob", Inputs = new List<IServiceInput>() } });

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object) { SourceRegion = null };
            var buildRegions = vm.BuildRegions();
            //---------------Test Result -----------------------
            Assert.AreEqual(buildRegions.Single(region => region is INamespaceToolRegion<INamespaceItem>).Errors.Count, 1);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LoadDotNetTool_GivenModelItemWIthValues_ShouldPopulateViewModelSourceRegionProperties()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPluginServiceModel>();
            var guid = Guid.NewGuid();
            mock.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>
            {
                new PluginSourceDefinition {Name = "Source1", Id = guid , GACAssemblyName = "GACAssemblyName", }
            });
            mock.Setup(model => model.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem>());
            mock.Setup(model => model.GetConstructors(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginConstructor>());
            mock.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>());
            var activity = new DsfEnhancedDotNetDllActivity
            {
                Constructor = new PluginConstructor
                {
                    ConstructorName = ".ctor ",
                    Inputs = new List<IConstructorParameter>
                    {
                        new ConstructorParameter { Name = "name", Value = "Jimmy", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "surname", Value = "Mouse", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "food", Value = "Jimmy", TypeName = typeof(Food).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    },
                },
                Namespace = new NamespaceItem
                {
                    FullName = typeof(Human).FullName,
                    AssemblyLocation = typeof(Human).Assembly.Location,
                }
            };

            var food = new Food
            {
                FoodName = "Cake"
            };
            activity.ConstructorInputs = new List<IServiceInput>
            {
                new ServiceInput("name","John") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("surname","Doe") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("food",food.SerializeToJsonString(new KnownTypesBinder
                {
                                KnownTypes = typeof(Food).Assembly.ExportedTypes.ToList()
                            }))
                {
                        TypeName = typeof(string).AssemblyQualifiedName
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "SetNameInternal",
                }
            };
            activity.SourceId = guid;
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity));
            //---------------Execute Test ----------------------
            var dotNetDllEnhancedViewModel = new DotNetDllEnhancedViewModel(modelItem, mock.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(dotNetDllEnhancedViewModel.SourceRegion);

            var sourceRegion = dotNetDllEnhancedViewModel.SourceRegion;
            Assert.IsNotNull(sourceRegion);
            var selectedSource = sourceRegion.SelectedSource;
            Assert.AreEqual(activity.SourceId, selectedSource.Id);
            Assert.AreEqual("Source1", selectedSource.Name);
            Assert.AreEqual("GACAssemblyName", selectedSource.GACAssemblyName);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LoadDotNetTool_GivenModelItemWIthValues_ShouldPopulateViewModelNameSpaceRegionProperties()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPluginServiceModel>();
            var type = typeof(Human);
            var guid = Guid.NewGuid();
            mock.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>
            {
                new PluginSourceDefinition {Name = "Source1", Id = guid , GACAssemblyName = "GACAssemblyName", }
            });

            mock.Setup(model => model.GetConstructors(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginConstructor>());
            mock.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>());
            var jsonString = new Human("Jimmy", "Jambo", new Food()).SerializeToJsonString(new KnownTypesBinder()
            {
                KnownTypes = new List<Type>(type.Assembly.ExportedTypes)
            });

            var namespaceItem = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                JsonObject = jsonString,

            };
            mock.Setup(model => model.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem> { namespaceItem });
            var activity = new DsfEnhancedDotNetDllActivity
            {
                Constructor = new PluginConstructor
                {
                    ConstructorName = ".ctor ",
                    Inputs = new List<IConstructorParameter>
                    {
                        new ConstructorParameter { Name = "name", Value = "Jimmy", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "surname", Value = "Mouse", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "food", Value = "Jimmy", TypeName = typeof(Food).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    },
                },
                Namespace = namespaceItem
            };

            var food = new Food
            {
                FoodName = "Cake"
            };
            activity.ConstructorInputs = new List<IServiceInput>
            {
                new ServiceInput("name","John") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("surname","Doe") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("food",food.SerializeToJsonString(new KnownTypesBinder
                {
                                KnownTypes = typeof(Food).Assembly.ExportedTypes.ToList()
                            }))
                {
                        TypeName = typeof(string).AssemblyQualifiedName
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "SetNameInternal",
                }
            };
            activity.SourceId = guid;
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity));
            //---------------Execute Test ----------------------
            var dotNetDllEnhancedViewModel = new DotNetDllEnhancedViewModel(modelItem, mock.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(dotNetDllEnhancedViewModel.NamespaceRegion);

            var namespaceRegion = dotNetDllEnhancedViewModel.NamespaceRegion;
            Assert.IsNotNull(namespaceRegion);
            var selectedNamespace = namespaceRegion.SelectedNamespace;
            Assert.IsNotNull(selectedNamespace);
            Assert.AreEqual(namespaceItem.FullName, selectedNamespace.FullName);
            Assert.AreEqual(namespaceItem.AssemblyLocation, selectedNamespace.AssemblyLocation);
            Assert.AreEqual(namespaceItem.AssemblyName, selectedNamespace.AssemblyName);
            Assert.AreEqual(namespaceItem.JsonObject, selectedNamespace.JsonObject);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LoadDotNetTool_GivenNamespaceChanges_ShouldPopulateOutPutRegionWithObjectResult()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPluginServiceModel>();
            var type = typeof(Human);
            var guid = Guid.NewGuid();
            mock.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>
            {
                new PluginSourceDefinition {Name = "Source1", Id = guid , GACAssemblyName = "GACAssemblyName", }
            });

            mock.Setup(model => model.GetConstructors(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginConstructor>());
            mock.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>());
            var jsonString = new Human("Jimmy", "Jambo", new Food()).SerializeToJsonString(new KnownTypesBinder()
            {
                KnownTypes = new List<Type>(type.Assembly.ExportedTypes)
            });

            var namespaceItem = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                JsonObject = jsonString,

            };
            mock.Setup(model => model.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem> { namespaceItem });
            var activity = new DsfEnhancedDotNetDllActivity
            {
                Constructor = new PluginConstructor
                {
                    ConstructorName = ".ctor ",
                    Inputs = new List<IConstructorParameter>
                    {
                        new ConstructorParameter { Name = "name", Value = "Jimmy", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "surname", Value = "Mouse", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "food", Value = "Jimmy", TypeName = typeof(Food).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    },
                },
                Namespace = namespaceItem
            };

            var food = new Food
            {
                FoodName = "Cake"
            };
            activity.ConstructorInputs = new List<IServiceInput>
            {
                new ServiceInput("name","John") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("surname","Doe") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("food",food.SerializeToJsonString(new KnownTypesBinder
                {
                                KnownTypes = typeof(Food).Assembly.ExportedTypes.ToList()
                            }))
                {
                        TypeName = typeof(string).AssemblyQualifiedName
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "SetNameInternal",
                }
            };
            activity.SourceId = guid;
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity));
            //---------------Execute Test ----------------------
            var dotNetDllEnhancedViewModel = new DotNetDllEnhancedViewModel(modelItem, mock.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(dotNetDllEnhancedViewModel.NamespaceRegion);

            var namespaceRegion = dotNetDllEnhancedViewModel.NamespaceRegion;
            Assert.IsNotNull(namespaceRegion);
            var selectedNamespace = namespaceRegion.SelectedNamespace;
            Assert.IsNotNull(selectedNamespace);
            Assert.AreEqual(namespaceItem.FullName, selectedNamespace.FullName);
            Assert.AreEqual(namespaceItem.AssemblyLocation, selectedNamespace.AssemblyLocation);
            Assert.AreEqual(namespaceItem.AssemblyName, selectedNamespace.AssemblyName);
            Assert.AreEqual(namespaceItem.JsonObject, selectedNamespace.JsonObject);
            namespaceRegion.SelectedNamespace = new NamespaceItem()
            {
                JsonObject = "JsonObject"
                ,
                FullName = typeof(Food).FullName
                ,
                AssemblyLocation = typeof(Food).Assembly.Location

            };
            var objectResult = dotNetDllEnhancedViewModel.OutputsRegion.ObjectResult;
            Assert.AreEqual("JsonObject", objectResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToModel_GivenNamespaceChanges_ShouldModelCorreclty()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPluginServiceModel>();
            var type = typeof(Human);
            var guid = Guid.NewGuid();
            mock.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>
            {
                new PluginSourceDefinition {Name = "Source1", Id = guid , GACAssemblyName = "GACAssemblyName", }
            });

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);

            mock.Setup(model => model.GetConstructors(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginConstructor>());
            mock.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>());
            var jsonString = new Human("Jimmy", "Jambo", new Food()).SerializeToJsonString(new KnownTypesBinder()
            {
                KnownTypes = new List<Type>(type.Assembly.ExportedTypes)
            });

            var namespaceItem = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                JsonObject = jsonString,

            };
            mock.Setup(model => model.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem> { namespaceItem });
            var pluginConstructor = new PluginConstructor
            {
                ConstructorName = ".ctor ",
                ReturnObject = string.Empty,
                Inputs = new List<IConstructorParameter>
                {
                    new ConstructorParameter { Name = "name", Value = "Jimmy", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    new ConstructorParameter { Name = "surname", Value = "Mouse", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    new ConstructorParameter { Name = "food", Value = "Jimmy", TypeName = typeof(Food).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                }
            };
            var activity = new DsfEnhancedDotNetDllActivity
            {
                Constructor = pluginConstructor,
                Namespace = namespaceItem
            };

            var food = new Food
            {
                FoodName = "Cake"
            };
            activity.ConstructorInputs = new List<IServiceInput>
            {
                new ServiceInput("name","John") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("surname","Doe") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("food",food.SerializeToJsonString(new KnownTypesBinder
                {
                                KnownTypes = typeof(Food).Assembly.ExportedTypes.ToList()
                            }))
                {
                        TypeName = typeof(string).AssemblyQualifiedName
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "SetNameInternal",
                }
            };
            activity.SourceId = guid;
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity));
            //---------------Execute Test ----------------------
            var dotNetDllEnhancedViewModel = new DotNetDllEnhancedViewModel(modelItem, mock.Object);
            //---------------Test Result -----------------------
            var pluginService = dotNetDllEnhancedViewModel.ToModel();
            Assert.AreEqual(activity.SourceId, pluginService.Source.Id);
            Assert.AreEqual(activity.Namespace.FullName, pluginService.Namespace.FullName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public async Task DeleteActionCommand_GivenMethodRegion_ShouldFirePropertyChangeOnTheRegionList()
        {
            //---------------Set up test pack-------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mock = new Mock<IPluginServiceModel>();
            CustomContainer.Register(mockShellViewModel.Object);
            var type = typeof(Human);
            var guid = Guid.NewGuid();
            mock.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>
            {
                new PluginSourceDefinition {Name = "Source1", Id = guid , GACAssemblyName = "GACAssemblyName", }
            });

            mock.Setup(model => model.GetConstructors(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginConstructor>());
            mock.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>());
            var jsonString = new Human("Jimmy", "Jambo", new Food()).SerializeToJsonString(new KnownTypesBinder()
            {
                KnownTypes = new List<Type>(type.Assembly.ExportedTypes)
            });

            var namespaceItem = new NamespaceItem
            {
                FullName = type.FullName,
                AssemblyLocation = type.Assembly.Location,
                JsonObject = jsonString,

            };
            mock.Setup(model => model.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem> { namespaceItem });
            var activity = new DsfEnhancedDotNetDllActivity
            {
                Constructor = new PluginConstructor
                {
                    ConstructorName = ".ctor ",
                    Inputs = new List<IConstructorParameter>
                    {
                        new ConstructorParameter { Name = "name", Value = "Jimmy", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "surname", Value = "Mouse", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "food", Value = "Jimmy", TypeName = typeof(Food).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    },
                },
                Namespace = namespaceItem
            };

            var food = new Food
            {
                FoodName = "Cake"
            };
            activity.ConstructorInputs = new List<IServiceInput>
            {
                new ServiceInput("name","John") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("surname","Doe") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("food",food.SerializeToJsonString(new KnownTypesBinder
                {
                                KnownTypes = typeof(Food).Assembly.ExportedTypes.ToList()
                            }))
                {
                        TypeName = typeof(string).AssemblyQualifiedName
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "Method1",
                },new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "Method2",
                }
            };
            activity.SourceId = guid;
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var dotNetDllEnhancedViewModel = new DotNetDllEnhancedViewModel(modelItem, mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsTrue(modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity));
            Assert.IsNotNull(dotNetDllEnhancedViewModel.DeleteActionCommand);
            Assert.IsTrue(dotNetDllEnhancedViewModel.DeleteActionCommand.CanExecute(null));
            bool wasCalled = false;
            dotNetDllEnhancedViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "MethodsToRunList")
                {
                    wasCalled = true;
                }
            };
            //---------------Execute Test ----------------------
            var methodToolRegion = dotNetDllEnhancedViewModel.MethodsToRunList.First() as DotNetMethodRegion;
            Assert.IsNotNull(methodToolRegion);
            Assert.AreEqual(3, dotNetDllEnhancedViewModel.MethodsToRunList.Count);
            await dotNetDllEnhancedViewModel.DeleteActionCommand.Execute(methodToolRegion);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, dotNetDllEnhancedViewModel.MethodsToRunList.Count);
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LoadDotNetTool_GivenModelItemWIthValues_ShouldPopulateViewModelConstructorRegionProperties()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPluginServiceModel>();
            mock.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>());
            mock.Setup(model => model.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem>());
            mock.Setup(model => model.GetConstructors(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginConstructor>());
            mock.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>());
            var activity = new DsfEnhancedDotNetDllActivity
            {
                Constructor = new PluginConstructor
                {
                    ConstructorName = ".ctor ",
                    Inputs = new List<IConstructorParameter>
                    {
                        new ConstructorParameter { Name = "name", Value = "Jimmy", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "surname", Value = "Mouse", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "food", Value = "Jimmy", TypeName = typeof(Food).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    },
                },
                Namespace = new NamespaceItem
                {
                    FullName = typeof(Human).FullName,
                    AssemblyLocation = typeof(Human).Assembly.Location,
                }
            };

            var food = new Food
            {
                FoodName = "Cake"
            };
            activity.ConstructorInputs = new List<IServiceInput>
            {
                new ServiceInput("name","John") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("surname","Doe") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("food",food.SerializeToJsonString(new KnownTypesBinder
                {
                                KnownTypes = typeof(Food).Assembly.ExportedTypes.ToList()
                            }))
                {
                        TypeName = typeof(string).AssemblyQualifiedName
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "SetNameInternal",
                }
            };
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity));
            //---------------Execute Test ----------------------
            var dotNetDllEnhancedViewModel = new DotNetDllEnhancedViewModel(modelItem, mock.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(dotNetDllEnhancedViewModel.ConstructorRegion);

            var pluginConstructor = dotNetDllEnhancedViewModel.ConstructorRegion.SelectedConstructor;
            Assert.IsNotNull(pluginConstructor);
            var constructorName = pluginConstructor.ConstructorName;
            Assert.AreEqual(activity.Constructor.ConstructorName, constructorName);
            Assert.AreEqual(activity.Constructor.Inputs.Count, pluginConstructor.Inputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LoadDotNetTool_GivenModelItemWIthValues_ShouldPopulateViewModelOutPutRegionProperties()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPluginServiceModel>();
            mock.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>());
            mock.Setup(model => model.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem>());
            mock.Setup(model => model.GetConstructors(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginConstructor>());
            mock.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>());
            var activity = new DsfEnhancedDotNetDllActivity
            {
                Constructor = new PluginConstructor
                {
                    ConstructorName = ".ctor ",
                    Inputs = new List<IConstructorParameter>
                    {
                        new ConstructorParameter { Name = "name", Value = "Jimmy", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "surname", Value = "Mouse", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "food", Value = "Jimmy", TypeName = typeof(Food).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    },
                },
                Namespace = new NamespaceItem
                {
                    FullName = typeof(Human).FullName,
                    AssemblyLocation = typeof(Human).Assembly.Location,
                }
            };

            var food = new Food
            {
                FoodName = "Cake"
            };
            var humanString = food.SerializeToJsonString(new KnownTypesBinder
            {
                KnownTypes = typeof(Food).Assembly.ExportedTypes.ToList()
            });
            activity.ConstructorInputs = new List<IServiceInput>
            {
                new ServiceInput("name","John") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("surname","Doe") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("food",humanString)
                {
                        TypeName = typeof(string).AssemblyQualifiedName
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "SetNameInternal",
                }
            };
            activity.IsObject = true;
            activity.ObjectName = "@Human";
            activity.ObjectResult = humanString;
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity));
            //---------------Execute Test ----------------------
            var dotNetDllEnhancedViewModel = new DotNetDllEnhancedViewModel(modelItem, mock.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(dotNetDllEnhancedViewModel.OutputsRegion);

            var outputsRegion = dotNetDllEnhancedViewModel.OutputsRegion;
            Assert.IsNotNull(outputsRegion);
            Assert.AreEqual(true, outputsRegion.IsObject);
            Assert.AreEqual("@Human", outputsRegion.ObjectName);
            //Assert.AreEqual(humanString.RemoveWhiteSpace().Replace(Environment.NewLine,"").TrimStart().TrimEnd()
            //    , outputsRegion.ObjectResult.RemoveWhiteSpace().Replace(Environment.NewLine, "").TrimStart().TrimEnd());
            Assert.AreEqual(0, outputsRegion.Outputs.Count);
            Assert.AreEqual(1, outputsRegion.Dependants.Count);
            Assert.AreEqual("OutputsRegion", outputsRegion.ToolRegionName);
            Assert.AreEqual(true, outputsRegion.IsObjectOutputUsed);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LoadDotNetTool_GivenModelItemWIthValues_ShouldPopulateViewModelMethodRegionProperties()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPluginServiceModel>();
            mock.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>());
            mock.Setup(model => model.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem>());
            mock.Setup(model => model.GetConstructors(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginConstructor>());
            mock.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>());
            var activity = new DsfEnhancedDotNetDllActivity
            {
                Constructor = new PluginConstructor
                {
                    ConstructorName = ".ctor ",
                    Inputs = new List<IConstructorParameter>
                    {
                        new ConstructorParameter { Name = "name", Value = "Jimmy", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "surname", Value = "Mouse", TypeName = typeof(string).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                        new ConstructorParameter { Name = "food", Value = "Jimmy", TypeName = typeof(Food).AssemblyQualifiedName, IsRequired = true, EmptyToNull = true },
                    },
                },
                Namespace = new NamespaceItem
                {
                    FullName = typeof(Human).FullName,
                    AssemblyLocation = typeof(Human).Assembly.Location,
                }
            };

            var food = new Food
            {
                FoodName = "Cake"
            };
            activity.ConstructorInputs = new List<IServiceInput>
            {
                new ServiceInput("name","John") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("surname","Doe") {TypeName = typeof(string).AssemblyQualifiedName},
                new ServiceInput("food",food.SerializeToJsonString(new KnownTypesBinder
                {
                                KnownTypes = typeof(Food).Assembly.ExportedTypes.ToList()
                            }))
                {
                        TypeName = typeof(string).AssemblyQualifiedName
                },
            };
            activity.MethodsToRun = new List<IPluginAction>
            {
                new PluginAction
                {
                    Inputs = new List<IServiceInput>(),
                    IsObject = false,
                    IsVoid = true,
                    Method = "SetNameInternal",
                },
                new PluginAction()
                {
                      Inputs = new List<IServiceInput>()
                      {
                          new ServiceInput("name","name"),
                          new ServiceInput("name","name"),
                      },
                    IsObject = true,
                    IsVoid = false,
                    Method = "ToString",
                }
            };
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity));
            //---------------Execute Test ----------------------
            var dotNetDllEnhancedViewModel = new DotNetDllEnhancedViewModel(modelItem, mock.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(dotNetDllEnhancedViewModel.MethodsToRunList);

            var methodToolRegions = dotNetDllEnhancedViewModel.MethodsToRunList;
            Assert.AreEqual(3, methodToolRegions.Count);
            IMethodToolRegion<IPluginAction> methodToolRegion1 = methodToolRegions[0];
            Assert.IsNotNull(methodToolRegion1.SelectedMethod);
            Assert.AreEqual("SetNameInternal", methodToolRegion1.SelectedMethod.Method);
            Assert.AreEqual(true, methodToolRegion1.SelectedMethod.IsVoid);
            Assert.AreEqual(false, methodToolRegion1.SelectedMethod.IsObject);
            Assert.AreEqual(0, methodToolRegion1.SelectedMethod.Inputs.Count);

            IMethodToolRegion<IPluginAction> methodToolRegion2 = methodToolRegions[1];
            Assert.IsNotNull(methodToolRegion2.SelectedMethod);
            Assert.AreEqual("ToString", methodToolRegion2.SelectedMethod.Method);
            Assert.AreEqual(false, methodToolRegion2.SelectedMethod.IsVoid);
            Assert.AreEqual(true, methodToolRegion2.SelectedMethod.IsObject);
            Assert.AreEqual(2, methodToolRegion2.SelectedMethod.Inputs.Count);

            IMethodToolRegion<IPluginAction> emptyMethod = methodToolRegions[2];
            Assert.IsNull(emptyMethod.SelectedMethod);

        }

        private static readonly Guid id = Guid.NewGuid();
        private static Mock<IPluginServiceModel> SetupEmptyMockSource()
        {
            var ps = new Mock<IPluginServiceModel>();
            ps.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource> { new PluginSourceDefinition { Id = id } });
            ps.Setup(a => a.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new ObservableCollection<INamespaceItem> { new NamespaceItem { FullName = "f" } });
            ps.Setup(a => a.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new ObservableCollection<IPluginAction> { new PluginAction { FullName = "bob", Inputs = new List<IServiceInput>() } });
            return ps;
        }

        static ModelItem CreateModelItem()
        {
            var activity = new DsfEnhancedDotNetDllActivity();
            return ModelItemUtils.CreateModelItem(activity);
        }

        static ModelItem CreateModelItemWithValues()
        {
            var activity = new DsfEnhancedDotNetDllActivity
            {
                MethodsToRun = new List<IPluginAction>(new[]
                {
                    new PluginAction { FullName = "bob", Inputs = new List<IServiceInput> { new ServiceInput { Name = "a", Value = "b" } } }
                }),
                Namespace = new NamespaceItem { AssemblyLocation = "d", AssemblyName = "e", FullName = "f", MethodName = "g" },
                SourceId = id
            };
            return ModelItemUtils.CreateModelItem(activity);
        }
    }
}