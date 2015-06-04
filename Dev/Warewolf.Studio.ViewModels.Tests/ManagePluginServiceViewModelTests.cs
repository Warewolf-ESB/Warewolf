using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.PluginService;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManagePluginServiceViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagePluginServiceViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ManagePluginServiceViewModel_Ctor_AssertRightThingsVisible_ExpectValidObject()

        {
            //------------Setup for test--------------------------
            var managePluginServiceViewModel = new ManagePluginServiceViewModel(new Mock<IPluginServiceModel>().Object,new Mock<IRequestServiceNameViewModel>().Object);
            

            //------------Execute Test---------------------------
            managePluginServiceViewModel.CanEditMappings.Should().BeFalse();
            managePluginServiceViewModel.CanEditSource.Should().BeFalse();
            managePluginServiceViewModel.AvalaibleActions.Should().BeEmpty();
            managePluginServiceViewModel.CanTest.Should().BeFalse();
            managePluginServiceViewModel.ErrorText.Should().BeEmpty();
            managePluginServiceViewModel.OutputMapping.Should().BeEmpty();
            managePluginServiceViewModel.Inputs.Should().BeEmpty();

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagePluginServiceViewModel_Ctor")]
        public void ManagePluginServiceViewModel_Ctor_AssertRightThingsVisible_ExpectException
            ()
        {
            //------------Setup for test--------------------------
          //  var managePluginServiceViewModel = new ManagePluginServiceViewModel(new Mock<IPluginServiceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object);
            UnittestingUtils.NullArgumentConstructorHelper.AssertNullConstructor(new object[]{ new Mock<IPluginServiceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object }, typeof( ManagePluginServiceViewModel));

            //------------Execute Test---------------------------


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagePluginServiceViewModel_SelectSource")]
        public void ManagePluginServiceViewModel_SelectSource_ExpectThatOtherOptionsAreAvailable()
        {
            //------------Setup for test--------------------------
            var managePluginServiceViewModel = new ManagePluginServiceViewModel(CreateMockModel().Object, new Mock<IRequestServiceNameViewModel>().Object);
          
            //------------Execute Test---------------------------
            // ReSharper disable MaximumChainedReferences
            var mother = managePluginServiceViewModel.CreateMother();
                mother.WhenISet(a => a.SelectedSource = a.Sources.First())
                .ThenTheValueOf(a => a.AvalaibleActions)
                .Value
                .Should().HaveCount(2)
                .And.Contain(a => a.FullName == "bob");
                mother.Value.SelectedAction.Should().Be(null);

        
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagePluginServiceViewModel_SelectSource")]
        public void ManagePluginServiceViewModel_SelectActionExpectInputValuesToBePopulated()
        {
            //------------Setup for test--------------------------
            var managePluginServiceViewModel = new ManagePluginServiceViewModel(CreateMockModel().Object, new Mock<IRequestServiceNameViewModel>().Object);

            //------------Execute Test---------------------------
            // ReSharper disable MaximumChainedReferences
            var mother = managePluginServiceViewModel.CreateMother();
            mother.WhenISet(a => a.SelectedSource = a.Sources.First())
            .And()
            .WhenISet(a => a.SelectedAction = a.AvalaibleActions.ToList()[0])
            .Value.CanTest.Should().BeTrue();
            

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagePluginServiceViewModel_SelectSource")]
        public void ManagePluginServiceViewModel_TestExpectFailure_AssertCannotDoAnythingElse()
        {
            //------------Setup for test--------------------------
            var managePluginServiceViewModel = new ManagePluginServiceViewModel(CreateMockModel(failTest:true).Object, new Mock<IRequestServiceNameViewModel>().Object);

            //------------Execute Test---------------------------
            // ReSharper disable MaximumChainedReferences
            var mother = managePluginServiceViewModel.CreateMother();
            var vm = mother.WhenISet(a => a.SelectedSource = a.Sources.First())
            .And()
            .WhenISet(a => a.TestPluginCommand.Execute(null)).Value;
            vm.ErrorText.Should().Be("message");
            vm.CanEditMappings.Should().BeFalse();
            vm.CanSave.Should().BeFalse();
            vm.IsTesting.Should().BeFalse();
    
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagePluginServiceViewModel_SelectSource")]
        public void ManagePluginServiceViewModel_TestExpectFailure_AssertCanneditMappings()
        {
            //------------Setup for test--------------------------
            var managePluginServiceViewModel = new ManagePluginServiceViewModel(CreateMockModel(failTest: false).Object, new Mock<IRequestServiceNameViewModel>().Object);

            //------------Execute Test---------------------------
            // ReSharper disable MaximumChainedReferences
            var mother = managePluginServiceViewModel.CreateMother();
            var vm = mother.WhenISet(a => a.SelectedSource = a.Sources.First())
            .And()
            .WhenISet(a => a.TestPluginCommand.Execute(null)).Value;
            vm.ErrorText.Should().Be("");
            vm.CanEditMappings.Should().BeTrue();
            vm.CanSave.Should().BeTrue();
            vm.IsTesting.Should().BeFalse();
            vm.OutputMapping.Count.Should().Be(1);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagePluginServiceViewModel_Save")]
        public void ManagePluginServiceViewModel_Save()
        {
            //------------Setup for test--------------------------

            var model = CreateMockModel();
            var name = new Mock<IRequestServiceNameViewModel>();
            name.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            name.Setup(a=>a.ResourceName).Returns(new ResourceName("bob","builder"));
            var managePluginServiceViewModel = new ManagePluginServiceViewModel(model.Object, name.Object);

            //------------Execute Test---------------------------
            // ReSharper disable MaximumChainedReferences
            var mother = managePluginServiceViewModel.CreateMother();
            var vm = mother.WhenISet(a => a.SelectedSource = a.Sources.First())
            .And()
            .WhenISet(a => a.TestPluginCommand.Execute(null))
            .And()
            .WhenISet(a => a.SaveCommand.Execute(null));
            model.Verify(a=>a.SaveService(It.IsAny<IPluginService>()));
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagePluginServiceViewModel_Save")]
        public void ManagePluginServiceViewModel_Save_UserCancel()
        {
            //------------Setup for test--------------------------

            var model = CreateMockModel();
            var name = new Mock<IRequestServiceNameViewModel>();
            name.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.No);
            name.Setup(a => a.ResourceName).Returns(new ResourceName("bob", "builder"));
            var managePluginServiceViewModel = new ManagePluginServiceViewModel(model.Object, name.Object);

            //------------Execute Test---------------------------
            // ReSharper disable MaximumChainedReferences
            var mother = managePluginServiceViewModel.CreateMother();
            var vm = mother.WhenISet(a => a.SelectedSource = a.Sources.First())
            .And()
            .WhenISet(a => a.TestPluginCommand.Execute(null))
            .And()
            .WhenISet(a => a.SaveCommand.Execute(null));
            model.Verify(a => a.SaveService(It.IsAny<IPluginService>()),Times.Never());
        }


        public Mock<IPluginServiceModel> CreateMockModel(bool failTest=false)
        {
            var model = new Mock<IPluginServiceModel>();
            var lst = new List<IPluginSource> { new PluginSourceDefinition(), new PluginSourceDefinition() };
            model.Setup(a => a.RetrieveSources()).Returns(lst);
            model.Setup(a => a.GetActions(lst[0],new Mock<INamespaceItem>().Object)).Returns(new List<IPluginAction> { new PluginAction { FullName = "bob" }, new PluginAction() { FullName = "Dave" } });
            model.Setup(a => a.GetActions(lst[1], new Mock<INamespaceItem>().Object)).Returns(new List<IPluginAction> { new PluginAction { FullName = "dora" }, new PluginAction() { FullName = "carmen" } });
            if(failTest)
            {
                model.Setup(a => a.TestService(It.IsAny<IPluginService>())).Throws(new Exception("message"));
            }
            else
            {
                var p = new PluginService();
                var l = new RecordsetList();
                var r = new Recordset();
                r.Fields.Add(new RecordsetField(){Name = "bob",Alias = "boo"});
                l.Add(r);
                p.Recordsets = l;
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                
                model.Setup(a => a.TestService(It.IsAny<IPluginService>())).Returns(serializer.SerializeToBuilder(p).ToString());
            }
            return model;
        }

        // ReSharper restore MaximumChainedReferences
    }

    // ReSharper restore InconsistentNaming
    public static class TestExt
    {

        public static TestBinder<T> CreateMother<T>(this T obj)
        {

            return new TestBinder<T>(obj);
        }
        public static T Then<T> (this T obj)
        {

            return obj;
        }
    }
    public class TestBinder<T>
    {
        T _obj;
    
        object parent;
        Type parentType;
        public TestBinder(T obj)
        {
            _obj = obj;
        }


        public TestBinder<T> WhenISet( Action<T> act )
        {
            act(_obj);
            return  this;
        }

        public TestBinder<T> And()
        {
        
            return this;
        }

        public TestBinder<U> WhenIExecute<U>(Func<T,U>  func)
        {
            return  new TestBinder<U>(func(_obj));
        }
        public TestBinder<U> ThenTheValueOf<U>(Func<T, U> func)
        {
            return new TestBinder<U>(func(_obj)){parent = this,ParentType = GetType()};
        }
        public T Value
        {
          get{  return _obj;}
        }
        private object ParentBinder
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }
        public Type ParentType
        {
            get
            {
                return parentType;
            }
            set
            {
                parentType = value;
            }
        }
        public TestBinder<TU> Parent<TU>()
        {
            return (TestBinder<TU>)parent; 
        }

    }
}