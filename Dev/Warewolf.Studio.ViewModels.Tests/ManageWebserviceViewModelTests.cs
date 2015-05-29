using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;

// ReSharper disable InconsistentNaming
namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageWebserviceViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_Ctor")]
        public void ManagewebServiceViewModel_Ctor_Default_VerifyValuesSet()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource>());

            //------------Execute Test---------------------------
            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(mockSave.Object, managewebServiceViewModel.SaveDialog);
            Assert.AreEqual(mockModel.Object, managewebServiceViewModel.Model);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_ToModel")]
        public void ManagewebServiceViewModel_ToModel_HasCorrectParams()

        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";

            //------------Execute Test---------------------------
            IWebService model = managewebServiceViewModel.ToModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("@a", model.QueryString);
            Assert.AreEqual(model.PostData, "da");
            Assert.IsTrue(model.Headers.Count > 0);
            Assert.AreEqual("header", model.Headers[0].Name);
            Assert.AreEqual("HeaderValues", model.Headers[0].Value);
            Assert.AreEqual("@a", model.QueryString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceViewModel_ToModel")]
        public void ManageWebServiceViewModel_ToModel_WhenHeadersHaveVariables_ShouldStripBraces()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "[[header]]", Value = "[[HeaderValues]]" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";
            //------------Execute Test---------------------------
            IWebService model = managewebServiceViewModel.ToModel();
            //------------Assert Results-------------------------
            Assert.AreEqual("@a", model.QueryString);
            Assert.AreEqual(model.PostData, "da");
            Assert.IsTrue(model.Headers.Count > 0);
            Assert.AreEqual("header", model.Headers[0].Name);
            Assert.AreEqual("HeaderValues", model.Headers[0].Value);
            Assert.AreEqual("@a", model.QueryString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceViewModel_ToModel")]
        public void ManageWebServiceViewModel_ToModel_WhenPostDataHaveVariables_ShouldStripBraces()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.RequestBody = "[[da]]";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "[[header]]", Value = "[[HeaderValues]]" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";
            //------------Execute Test---------------------------
            IWebService model = managewebServiceViewModel.ToModel();
            //------------Assert Results-------------------------
            Assert.AreEqual("@a", model.QueryString);
            Assert.AreEqual("da", model.PostData);
            Assert.IsTrue(model.Headers.Count > 0);
            Assert.AreEqual("header", model.Headers[0].Name);
            Assert.AreEqual("HeaderValues", model.Headers[0].Value);
            Assert.AreEqual("@a", model.QueryString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceViewModel_ToModel")]
        public void ManageWebServiceViewModel_ToModel_WhenQueryStringDataHaveVariables_ShouldStripBraces()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };
            managewebServiceViewModel.RequestUrlQuery = "a=[[a]]";
            //------------Execute Test---------------------------
            IWebService model = managewebServiceViewModel.ToModel();
            //------------Assert Results-------------------------
            Assert.AreEqual("a=[[a]]", model.QueryString);
            Assert.AreEqual("da", model.PostData);
            Assert.AreEqual("a",model.Inputs[0].Name);
            Assert.IsTrue(model.Headers.Count > 0);
            Assert.AreEqual("header", model.Headers[0].Name);
            Assert.AreEqual("HeaderValues", model.Headers[0].Value);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_Save")]
        public void ManagewebServiceViewModel_SaveCommandCallsModel()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });
            managewebServiceViewModel.RequestBody = "dave";
            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";
            mockSave.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            mockSave.Setup(a => a.Name).Returns("boo");
            mockSave.Setup(a => a.ResourceName).Returns(new ResourceName("bobname", "dave"));
            //------------Execute Test---------------------------
            managewebServiceViewModel.SaveCommand.Execute(null);

            //------------Assert Results-------------------------
            mockModel.Verify(a => a.SaveService(It.IsAny<IWebService>()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_CanSave")]
        public void ManagewebServiceViewModel_CanSaveCommandCallsModel()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });
            managewebServiceViewModel.RequestBody = "dave";
            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";
            managewebServiceViewModel.SelectedSource = null;
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------

            Assert.IsFalse(managewebServiceViewModel.CanTest());
            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            Assert.IsTrue(managewebServiceViewModel.CanTest());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_QueryStringCreatesParams")]
        public void ManagewebServiceViewModel_QueryStringCreatesParams()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });
            managewebServiceViewModel.RequestBody = "dave";
            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };

            managewebServiceViewModel.SelectedSource = null;
            //------------Execute Test---------------------------
            Assert.AreEqual(0, managewebServiceViewModel.Variables.Count);
            managewebServiceViewModel.RequestUrlQuery = "[[a]]";
            //------------Assert Results-------------------------

            Assert.AreEqual(1, managewebServiceViewModel.Variables.Count);
            Assert.AreEqual("[[a]]", managewebServiceViewModel.Variables.ToList()[0].Name);
            Assert.AreEqual("", managewebServiceViewModel.Variables.ToList()[0].Value);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_QueryStringCreatesParams")]
        public void ManagewebServiceViewModel_QueryStringCreatesParams_NotAddedTwice()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });
            managewebServiceViewModel.RequestBody = "dave";
            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };

            managewebServiceViewModel.SelectedSource = null;
            //------------Execute Test---------------------------
            Assert.AreEqual(0, managewebServiceViewModel.Variables.Count);
            managewebServiceViewModel.RequestUrlQuery = "[[a]]";

            managewebServiceViewModel.RequestBody = "[[a]]";
            //------------Assert Results-------------------------

            Assert.AreEqual(1, managewebServiceViewModel.Variables.Count);
            Assert.AreEqual("[[a]]", managewebServiceViewModel.Variables.ToList()[0].Name);
            Assert.AreEqual("", managewebServiceViewModel.Variables.ToList()[0].Value);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_QueryStringCreatesParams")]
        public void ManagewebServiceViewModel_QueryStringCreatesParams_RemopvedIfNoLongerUsed()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });
            managewebServiceViewModel.RequestBody = "dave";
            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };

            managewebServiceViewModel.SelectedSource = null;
            //------------Execute Test---------------------------
            Assert.AreEqual(0, managewebServiceViewModel.Variables.Count);
            managewebServiceViewModel.RequestUrlQuery = "[[a]]";
            Assert.AreEqual(1, managewebServiceViewModel.Variables.Count);
            Assert.AreEqual("[[a]]", managewebServiceViewModel.Variables.ToList()[0].Name);
            managewebServiceViewModel.RequestUrlQuery = "[[b]]";
            //------------Assert Results-------------------------

            Assert.AreEqual(1, managewebServiceViewModel.Variables.Count);
            Assert.AreEqual("[[b]]", managewebServiceViewModel.Variables.ToList()[0].Name);
            Assert.AreEqual("", managewebServiceViewModel.Variables.ToList()[0].Value);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_RequiestDataCreatesParams")]
        public void ManagewebServiceViewModel_RequestDataCreatesParams()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });
            managewebServiceViewModel.RequestBody = "dave";
            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };

            managewebServiceViewModel.SelectedSource = null;
            //------------Execute Test---------------------------
            Assert.AreEqual(0, managewebServiceViewModel.Variables.Count);
            managewebServiceViewModel.RequestBody = "[[b]]";
            //------------Assert Results-------------------------

            Assert.AreEqual(1, managewebServiceViewModel.Variables.Count);
            Assert.AreEqual("[[b]]", managewebServiceViewModel.Variables.ToList()[0].Name);
            Assert.AreEqual("", managewebServiceViewModel.Variables.ToList()[0].Value);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_RequiestDataCreatesParams")]
        public void ManagewebServiceViewModel_HeaderDataCreatesParams()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });
            managewebServiceViewModel.RequestBody = "dave";
            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };

            managewebServiceViewModel.SelectedSource = null;
            //------------Execute Test---------------------------
            Assert.AreEqual(0, managewebServiceViewModel.Variables.Count);
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "[[header]]", Value = "[[HeaderValues]]" } };
            //------------Assert Results-------------------------

            Assert.AreEqual(2, managewebServiceViewModel.Variables.Count);
            Assert.AreEqual("[[header]]", managewebServiceViewModel.Variables.ToList()[0].Name);
            Assert.AreEqual("", managewebServiceViewModel.Variables.ToList()[0].Value);
            Assert.AreEqual("[[HeaderValues]]", managewebServiceViewModel.Variables.ToList()[1].Name);
            Assert.AreEqual("", managewebServiceViewModel.Variables.ToList()[1].Value);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_ToModel")]
        public void ManagewebServiceViewModel_TestCommandCallsModel_UpdatesResponseCanSave()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";
            var serializer = new Dev2JsonSerializer();
            var serializedWebService = serializer.Serialize(new WebService{RequestResponse = "bob"});
            mockModel.Setup(a => a.TestService(It.IsAny<IWebService>())).Returns(serializedWebService);

            //------------Execute Test---------------------------
            managewebServiceViewModel.TestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("bob",managewebServiceViewModel.Response);
            Assert.IsTrue(managewebServiceViewModel.CanSave());
            Assert.IsTrue(managewebServiceViewModel.CanEditMappings);
            Assert.IsFalse(managewebServiceViewModel.IsTesting);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_ToModel")]
        public void ManagewebServiceViewModel_TestCommandCallsModel_UpdatesErrorIFailed()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";

            mockModel.Setup(a => a.TestService(It.IsAny<IWebService>())).Throws(new Exception("bob error"));

            //------------Execute Test---------------------------
            managewebServiceViewModel.TestCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(managewebServiceViewModel.ErrorMessage, "bob error");
            Assert.IsFalse(managewebServiceViewModel.CanSave());
            Assert.IsFalse(managewebServiceViewModel.CanEditMappings);
            Assert.IsFalse(managewebServiceViewModel.IsTesting);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_ToModel")]
        public void ManagewebServiceViewModel_Save_UpdatesErrorIFailed()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";
            mockSave.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            mockSave.Setup(a => a.Name).Returns("boo");
            mockSave.Setup(a => a.ResourceName).Returns(new ResourceName("bobname", "dave"));
            mockModel.Setup(a => a.SaveService(It.IsAny<IWebService>())).Throws(new Exception("bob error"));

            //------------Execute Test---------------------------
            managewebServiceViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(managewebServiceViewModel.ErrorMessage, "bob error");
            Assert.IsFalse(managewebServiceViewModel.CanSave());
            Assert.IsFalse(managewebServiceViewModel.CanEditMappings);
            Assert.IsFalse(managewebServiceViewModel.IsTesting);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_ToModel")]
        public void ManagewebServiceViewModel_Save_UpdatesItemIfSucessCallNameDialog()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            managewebServiceViewModel.SelectedSource = new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" };
            managewebServiceViewModel.SelectedSource.DefaultQuery = "bob";
            managewebServiceViewModel.RequestBody = "da";
            managewebServiceViewModel.Headers = new Collection<NameValue> { new NameValue { Name = "header", Value = "HeaderValues" } };
            managewebServiceViewModel.RequestUrlQuery = "@a";

            mockModel.Setup(a => a.SaveService(It.IsAny<IWebService>()));
            mockSave.Setup(a=>a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            mockSave.Setup(a=>a.Name).Returns("boo");
            mockSave.Setup(a=>a.ResourceName).Returns(new ResourceName("bobname","dave"));
            //------------Execute Test---------------------------
            managewebServiceViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(managewebServiceViewModel.Item);
            Assert.AreEqual("bobname",managewebServiceViewModel.Path);
            Assert.AreEqual("dave", managewebServiceViewModel.Name);
             mockSave.Verify(a=>a.ShowSaveDialog(),Times.Once());
            //only once
             managewebServiceViewModel.SaveCommand.Execute(null);
             mockSave.Verify(a => a.ShowSaveDialog(), Times.Once());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_ToModel")]
        public void ManagewebServiceViewModel_Save_UpdatesItemIfSucessCallNotNameDialogIfExistingItem()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });
            var item = new WebServiceDefinition("name","path", new WebServiceSourceDefinition(), new List<IServiceInput>(), new List<IServiceOutputMapping>(),"a",Guid.NewGuid()  );
            managewebServiceViewModel.Item = item;
            //------------Execute Test---------------------------
            managewebServiceViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(managewebServiceViewModel.Item);
            mockSave.Verify(a => a.ShowSaveDialog(), Times.Never());
            //only once
            managewebServiceViewModel.SaveCommand.Execute(null);
            mockSave.Verify(a => a.ShowSaveDialog(), Times.Never());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManagewebServiceViewModel_ToModel")]
        public void ManagewebServiceViewModel_Save_NoDialogFromCtor()
        {
            //------------Setup for test--------------------------
            var mockModel = new Mock<IWebServiceModel>();
            var mockSave = new Mock<IRequestServiceNameViewModel>();
            mockModel.Setup(a => a.RetrieveSources()).Returns(new Collection<IWebServiceSource> { new WebServiceSourceDefinition { Name = "bob", DefaultQuery = "mook" } });
            var item = new WebServiceDefinition("name", "path", new WebServiceSourceDefinition(), new List<IServiceInput>(), new List<IServiceOutputMapping>(), "a", Guid.NewGuid());

            var managewebServiceViewModel = new ManageWebServiceViewModel(mockModel.Object, mockSave.Object,item);
            managewebServiceViewModel.OutputMapping = new Collection<IServiceOutputMapping>(new IServiceOutputMapping[] { new ServiceOutputMapping("bob", "dave") });

            //------------Execute Test---------------------------
            managewebServiceViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(managewebServiceViewModel.Item);
            mockSave.Verify(a => a.ShowSaveDialog(), Times.Never());
            //only once
            managewebServiceViewModel.SaveCommand.Execute(null);
            mockSave.Verify(a => a.ShowSaveDialog(), Times.Never());
        }
    }


}