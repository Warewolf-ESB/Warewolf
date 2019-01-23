using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Activities.Designers2.ExchangeNewEmail;
using Dev2.Activities.Exchange;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Providers.Errors;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels;

namespace Dev2.Activities.Designers.Tests.Exchange
{
    [TestClass]
    public class ExchangeNewEmailDesignerViewModelTests
    {
        const string AppLocalhost = "http://localhost:3142";

        [TestInitialize]
        public void Initialize()
        {
            AppUsageStats.LocalHost = AppLocalhost;
        }

        static ModelItem CreateModelItem()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "test@gmail.com",
                Subject = "Test Exchange",
                Body = "Test email from exchange"
            };

            return ModelItemUtils.CreateModelItem(activity);
        }

        static ModelItem CreateNullToModelItem()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = null,
            };

            return ModelItemUtils.CreateModelItem(activity);
        }

        static ModelItem CreateFullToModelItem()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "test",
                Cc = "test",
                Body = "test",
                Bcc = "test",
                Attachments = "test"
            };

            return ModelItemUtils.CreateModelItem(activity);
        }

        static List<ExchangeSourceDefinition> CreateEmailSources(int count)
        {
            var result = new List<ExchangeSourceDefinition>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new ExchangeSourceDefinition
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = nameof(Email) + i,
                    UserName = "user" + i + "@test.com",
                    Password = "pasword" + i,
                    AutoDiscoverUrl = "http" + i
                });
            }

            return result;
        }

        static ExchangeNewEmailDesignerViewModel CreateViewModel(IShellViewModel shellViewModel, ModelItem modelItem, IEventAggregator eve, bool isemptySource = false)
        {
            var mockModel = new TestExchangeServiceModel(isemptySource);
            return CreateViewModel(shellViewModel, modelItem, mockModel, eve);
        }

        static ExchangeNewEmailDesignerViewModel CreateViewModel(IShellViewModel shellViewModel, ModelItem modelItem, IExchangeServiceModel model, IEventAggregator eve)
        {
            var mockActiveDataList = SetupActiveDataList();
            var testEmailDesignerViewModel = new ExchangeNewEmailDesignerViewModel(modelItem, new AsyncWorker(), model, eve, shellViewModel, mockActiveDataList.Object);
            testEmailDesignerViewModel.SourceRegion.SelectedSource = model.RetrieveSources().First();

            return testEmailDesignerViewModel;
        }

        static ExchangeNewEmailDesignerViewModel CreateNewViewModel(IShellViewModel shellViewModel, ModelItem modelItem, IEventAggregator eve, bool isemptySource = false)
        {
            var mockModel = new TestExchangeServiceModel(isemptySource);
            mockModel.Sources = mockModel.RetrieveSources();
            return CreateNewViewModel(shellViewModel, modelItem, mockModel, eve);
        }

        static ExchangeNewEmailDesignerViewModel CreateNewViewModel(IShellViewModel shellViewModel, ModelItem modelItem, IExchangeServiceModel model, IEventAggregator eve)
        {
            var mockActiveDataList = SetupActiveDataList();
            var mockServer = new Mock<IServer>();
            var mockStudioUpdateManager = new Mock<IStudioUpdateManager>();
            mockServer.SetupGet(it => it.UpdateRepository).Returns(mockStudioUpdateManager.Object);
            var mockQueryManager = new Mock<IQueryManager>();

            var mockExchangeServiceModel = new Mock<IExchangeServiceModel>();
            mockExchangeServiceModel.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IExchangeSource> { new ExchangeSourceDefinition { Id = Guid.NewGuid() } });

            mockQueryManager.Setup(query => query.FetchExchangeSources()).Returns(new ObservableCollection<IExchangeSource> { new ExchangeSourceDefinition { Id = Guid.NewGuid() } });
            mockServer.SetupGet(it => it.QueryProxy).Returns(mockQueryManager.Object);
            ServerRepository.Instance.ActiveServer = mockServer.Object;

            var testEmailDesignerViewModel = new ExchangeNewEmailDesignerViewModel(modelItem, new AsyncWorker(), mockServer.Object, eve, shellViewModel, mockActiveDataList.Object);
            testEmailDesignerViewModel.SourceRegion.SelectedSource = model.RetrieveSources().First();

            return testEmailDesignerViewModel;
        }

        private static Mock<IActiveDataList> SetupActiveDataList()
        {
            const string trueString = "True";
            const string noneString = "None";
            var datalist = $"<DataList><var Description=\"\" IsEditable=\"{trueString}\" ColumnIODirection=\"{noneString}\" /><a Description=\"\" IsEditable=\"{trueString}\" ColumnIODirection=\"{noneString}\" /><b Description=\"\" IsEditable=\"{trueString}\" ColumnIODirection=\"{noneString}\" /><h Description=\"\" IsEditable=\"{trueString}\" ColumnIODirection=\"{noneString}\" /><r Description=\"\" IsEditable=\"{trueString}\" ColumnIODirection=\"{noneString}\" /><rec Description=\"\" IsEditable=\"{trueString}\" ColumnIODirection=\"{noneString}\" ><set Description=\"\" IsEditable=\"{trueString}\" ColumnIODirection=\"{noneString}\" /></rec></DataList>";

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.Setup(res => res.DataList).Returns(datalist);

            IDataListViewModel setupDatalist = new DataListViewModel();
            setupDatalist.InitializeDataListViewModel(resourceModel.Object);
            var mockActiveDataList = new Mock<IActiveDataList>();
            mockActiveDataList.Setup(a => a.ActiveDataList).Returns(setupDatalist);
            return mockActiveDataList;
        }

        void Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount(string to)
        {
            //------------Setup for test--------------------------
            var activity = new DsfExchangeEmailNewActivity { To = to };

            var emailSource = new ExchangeSourceDefinition
            {
                UserName = "bob@mydomain.com",
                Password = "MyPassword",
                ResourceID = Guid.NewGuid()
            };
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);

            //------------Execute Test---------------------------
            viewModel.SourceRegion.SelectedSource = emailSource;

            //------------Assert Results-------------------------
            var toReceipient = modelItem.GetProperty<string>("To");
            Assert.AreEqual(to, toReceipient);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExchangeNewEmailDesignerViewModel_Constructor_AsyncWorkerIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockActiveDataList = SetupActiveDataList();

            //------------Execute Test---------------------------
            using (new ExchangeNewEmailDesignerViewModel(CreateModelItem(), null, new Mock<IServer>().Object, null, mockShellViewModel.Object, mockActiveDataList.Object))
            {
                //
            }
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExchangeNewEmailDesignerViewModel_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockActiveDataList = SetupActiveDataList();
            //------------Execute Test---------------------------
            using (new ExchangeNewEmailDesignerViewModel(CreateModelItem(), null, (IServer)null, null, mockShellViewModel.Object, mockActiveDataList.Object))
            {
                //
            }
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExchangeNewEmailDesignerViewModel_Constructor_EventAggregatorIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockActiveDataList = SetupActiveDataList();
            //------------Execute Test---------------------------
            using (new ExchangeNewEmailDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, new Mock<IServer>().Object, null, mockShellViewModel.Object, mockActiveDataList.Object))
            {
                //
            }
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_Constructor_ModelItemIsNew_InitializesProperties()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            var propertyChanged = false;
            modelItem.PropertyChanged += (sender, args) =>
            {
                propertyChanged = true;
            };
            const int emailSourceCount = 1;

            var eventPublisher = new Mock<IEventAggregator>();
            //------------Execute Test---------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.SourceRegion.EditSourceCommand);
            Assert.IsNotNull(viewModel.TestEmailAccountCommand);
            Assert.IsNotNull(viewModel.ChooseAttachmentsCommand);
            Assert.IsNotNull(viewModel.SourceRegion.Sources);
            Assert.IsTrue(viewModel.CanTestEmailAccount);
            Assert.AreEqual(emailSourceCount, viewModel.SourceRegion.Sources.Count);
            Assert.AreEqual("TestExchange", viewModel.SourceRegion.SelectedSource.ResourceName);
            Assert.IsTrue(propertyChanged);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            var eventPublisher = new Mock<IEventAggregator>();

            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(help => help.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockShellViewModel.Setup(help => help.HelpViewModel).Returns(mockHelpViewModel.Object);

            var viewModel = CreateNewViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_Constructor_ModelItemIsNotNew_InitializesProperties()
        {
            //------------Setup for test--------------------------
            const int emailSourceCount = 1;
            var sources = CreateEmailSources(emailSourceCount);
            var selectedEmailSource = sources.First();

            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedEmailSource", selectedEmailSource);

            var propertyChanged = false;
            modelItem.PropertyChanged += (sender, args) =>
            {
                propertyChanged = true;
            };
            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.TestEmailAccountCommand);
            Assert.IsNotNull(viewModel.ChooseAttachmentsCommand);
            Assert.IsNotNull(viewModel.SourceRegion.Sources);
            Assert.IsTrue(viewModel.CanTestEmailAccount);

            Assert.AreEqual(emailSourceCount, viewModel.SourceRegion.Sources.Count);
            Assert.AreEqual("TestExchange", viewModel.SourceRegion.SelectedSource.ResourceName);

            Assert.IsTrue(propertyChanged);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount()
        {
            Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount("test@mydomain.com");
            Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount("");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ChooseAttachments_PublishesFileChooserMessage()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();

            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<FileChooserMessage>())).Verifiable();
            var mockShellViewModel = new Mock<IShellViewModel>();

            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);

            //------------Execute Test---------------------------
            viewModel.ChooseAttachmentsCommand.Execute(null);

            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<FileChooserMessage>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ChooseAttachments_SelectedFilesIsNotNull_AddsFilesToAttachments()
        {
            Verify_ChooseAttachments(new List<string> { @"c:\tmp2.txt", @"c:\logs\errors2.log" });
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ChooseAttachments_SelectedFilesIsNull_DoesAddNotFilesToAttachments()
        {
            Verify_ChooseAttachments(null);
        }

        void Verify_ChooseAttachments(List<string> selectedFiles)
        {
            //------------Setup for test--------------------------
            var existingFiles = new List<string> { @"c:\tmp1.txt", @"c:\logs\errors1.log" };

            var expectedFiles = new List<string>();
            expectedFiles.AddRange(existingFiles);
            if (selectedFiles != null)
            {
                expectedFiles.AddRange(selectedFiles);
            }

            var modelItem = CreateModelItem();
            modelItem.SetProperty("Attachments", string.Join(";", existingFiles));

            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<FileChooserMessage>())).Callback((object m) =>
            {
                ((FileChooserMessage)m).SelectedFiles = selectedFiles;
            });

            var mockShellViewModel = new Mock<IShellViewModel>();
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);

            //------------Execute Test---------------------------
            viewModel.ChooseAttachmentsCommand.Execute(null);

            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<FileChooserMessage>()));
            var attachments = modelItem.GetProperty<string>("Attachments");
            Assert.AreEqual(string.Join(";", expectedFiles), attachments);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_TestEmail_Valid_ReturnsSucccess()
        {
            var modelItem = CreateModelItem();

            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<FileChooserMessage>())).Verifiable();
            var mockShellViewModel = new Mock<IShellViewModel>();

            var viewModel = CreateNewViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);
            var mockModel = new TestExchangeServiceModel(true);
            viewModel.SourceRegion = new ExchangeSourceRegion(mockModel, modelItem, "ExchangeSource");

            viewModel.TestEmailAccount();

            Assert.IsFalse(viewModel.Testing);
            Assert.AreEqual("", viewModel.StatusMessage);
            Assert.AreEqual(1, viewModel.Errors.Count);
            Assert.AreEqual("Please select valid source", viewModel.Errors[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_AddTitleBarLargeToggle()
        {
            var modelItem = CreateModelItem();

            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<FileChooserMessage>())).Verifiable();
            var mockShellViewModel = new Mock<IShellViewModel>();

            var viewModel = CreateNewViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);

            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateViewModelProperties_ReturnsNoError()
        {
            var modelItem = CreateModelItem();

            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();

            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);

            viewModel.CanTestEmailAccount = true;
            viewModel.IsEmailSourceFocused = true;
            viewModel.IsToFocused = true;
            viewModel.IsCcFocused = true;
            viewModel.IsBccFocused = true;
            viewModel.IsAttachmentsFocused = true;
            viewModel.IsSubjectFocused = true;
            viewModel.StatusMessage = "Passed";
            viewModel.Testing = true;
            viewModel.Properties = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("test", "test")
            };

            viewModel.AddProperty("Test", "test");

            Assert.IsTrue(viewModel.HasLargeView);
            Assert.IsTrue(viewModel.CanTestEmailAccount);
            Assert.IsTrue(viewModel.IsEmailSourceFocused);
            Assert.IsTrue(viewModel.IsToFocused);
            Assert.IsTrue(viewModel.IsCcFocused);
            Assert.IsTrue(viewModel.IsBccFocused);
            Assert.IsTrue(viewModel.IsAttachmentsFocused);
            Assert.IsTrue(viewModel.IsSubjectFocused);
            Assert.IsTrue(viewModel.Testing);
            Assert.AreEqual("Passed", viewModel.StatusMessage);
            Assert.AreEqual(2, viewModel.Properties.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_RunCommands_RetrunsSuccess()
        {
            var modelItem = CreateModelItem();

            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();

            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);
            viewModel.TestEmailAccountCommand.Execute(null);
            viewModel.CanTestEmailAccount = true;
            viewModel.Errors = null;
            viewModel.ChooseAttachmentsCommand.Execute(null);
            viewModel.TestEmailAccount();
            viewModel.Testing = true;

            Assert.IsNull(viewModel.Errors);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_TestEmail_NullTo_RetrunsErrors()
        {
            var modelItem = CreateNullToModelItem();
            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);
            viewModel.TestEmailAccount();

            Assert.IsNotNull(viewModel.Errors);
            Assert.IsNotNull(viewModel.SourceRegion.SelectedSource);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_TestEmail_ErrorIsNotNull_RetrunsErrors()
        {
            var modelItem = CreateFullToModelItem();
            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object, true);
            viewModel.Errors = new List<IActionableErrorInfo>
            {
                new ActionableErrorInfo(() => viewModel.IsToFocused = true)
                {
                    Message = "Please select a Source to Test."
                }
            };
            viewModel.TestEmailAccount();

            Assert.IsNotNull(viewModel.Errors);
            Assert.IsNotNull(viewModel.SourceRegion.SelectedSource);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_TestEmail_ErrorIsNull_RetrunsErrors()
        {
            var modelItem = CreateFullToModelItem();
            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object, true);

            viewModel.TestEmailAccount();
            viewModel.SetStatusMessage("Passed");
            viewModel.UpdateHelpDescriptor("Test");

            Assert.AreEqual("Passed", viewModel.StatusMessage);
            Assert.IsNull(viewModel.Errors);
            Assert.IsNotNull(viewModel.SourceRegion.SelectedSource);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_TestEmail_RetrunsErrors()
        {
            var modelItem = CreateFullToModelItem();
            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);
            viewModel.TestEmailAccount();

            Assert.IsNull(viewModel.Errors);
            Assert.IsNotNull(viewModel.SourceRegion.SelectedSource);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_UpdateHelperText_RetrunsSuccess()
        {
            var modelItem = CreateModelItem();
            var eventPublisher = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);
            viewModel.UpdateHelpDescriptor("Testing");

            Assert.IsNull(viewModel.HelpText);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_RecipientsIsEmpty_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity, "Please supply at least one of the following: 'To', 'Cc' or 'Bcc'", ExchangeNewEmailDesignerViewModel.IsToFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_RecipientsToIsNotEmpty_DoesHaveNotErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "user2@mydomain.com",
                Cc = "",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_RecipientsCcIsNotEmpty_DoesHaveNotErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "user2@mydomain.com",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_RecipientsBccIsNotEmpty_DoesHaveNotErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "",
                Bcc = "user2@mydomain.com",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_SubjectAndBodyIsEmpty_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "user2@mydomain.com",
                Cc = "",
                Bcc = "",
                Subject = "",
                Attachments = "",
                Body = "",
            };
            ValidateThis(activity, "Please supply at least one of the following: 'Subject' or 'Body'", ExchangeNewEmailDesignerViewModel.IsSubjectFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_SubjectIsNotEmpyAndBodyIsEmpty_DoesNotHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "user2@mydomain.com",
                Cc = "",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_SubjectIsEmpyAndBodyIsNotEmpty_DoesNotHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "user2@mydomain.com",
                Cc = "",
                Bcc = "",
                Subject = "",
                Attachments = "",
                Body = "The Body",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_ToIsNotValidExpression_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "h]]",
                Cc = "",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity, Warewolf.Resource.Errors.ErrorResource.EmailToInvalidExpressionErrorTest, ExchangeNewEmailDesignerViewModel.IsToFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_ToIsValidExpression_DoesNotHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "[[h]]",
                Cc = "",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_ToIsNotValidEmailAddress_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "user2#mydomain.com",
                Cc = "",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity, "'To' contains an invalid email address", ExchangeNewEmailDesignerViewModel.IsToFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_CcIsNotValidExpression_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "h]]",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity, Warewolf.Resource.Errors.ErrorResource.EmailCcInvalidExpressionErrorTest, ExchangeNewEmailDesignerViewModel.IsCcFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_CcIsValidExpression_DoesNotHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "[[h]]",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_CcIsNotValidEmailAddress_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "user2#mydomain.com",
                Bcc = "",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity, "'Cc' contains an invalid email address", ExchangeNewEmailDesignerViewModel.IsCcFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_BccIsNotValidExpression_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "",
                Bcc = "h]]",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity, Warewolf.Resource.Errors.ErrorResource.EmailBccInvalidExpressionErrorTest, ExchangeNewEmailDesignerViewModel.IsBccFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_BccIsValidExpression_DoesNotHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "",
                Bcc = "[[h]]",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_BccIsNotValidEmailAddress_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "",
                Bcc = "user2#mydomain.com",
                Subject = "The Subject",
                Attachments = "",
                Body = "The body",
            };
            ValidateThis(activity, "'Bcc' contains an invalid email address", ExchangeNewEmailDesignerViewModel.IsBccFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_AttachmentsIsNotValidExpression_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "",
                Bcc = "user2@mydomain.com",
                Subject = "The Subject",
                Attachments = "h]]",
                Body = "The body",
            };
            ValidateThis(activity, Warewolf.Resource.Errors.ErrorResource.EmailAttachmentsInvalidExpressionErrorTest, ExchangeNewEmailDesignerViewModel.IsAttachmentsFocusedProperty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_AttachmentsIsValidExpression_DoesNotHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "",
                Bcc = "user2@mydomain.com",
                Subject = "The Subject",
                Attachments = "[[h]]",
                Body = "The body",
            };
            ValidateThis(activity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeNewEmailDesignerViewModel))]
        public void ExchangeNewEmailDesignerViewModel_ValidateThis_AttachmentsIsNotValidFileName_DoesHaveErrors()
        {
            var activity = new DsfExchangeEmailNewActivity
            {
                To = "",
                Cc = "",
                Bcc = "user2@mydomain.com",
                Subject = "The Subject",
                Attachments = "c:\\logs",
                Body = "The body",
            };
            ValidateThis(activity, "'Attachments' contains an invalid file name", ExchangeNewEmailDesignerViewModel.IsAttachmentsFocusedProperty);
        }

        void ValidateThis(DsfExchangeEmailNewActivity activity, string expectedErrorMessage = null, DependencyProperty isFocusedProperty = null, bool setSelectedEmailSource = true)
        {
            var sources = CreateEmailSources(1);
            if (setSelectedEmailSource)
            {
                activity.SavedSource = sources[0];
            }

            var modelItem = ModelItemUtils.CreateModelItem(activity);

            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<FileChooserMessage>())).Verifiable();
            var mockShellViewModel = new Mock<IShellViewModel>();

            var viewModel = CreateViewModel(mockShellViewModel.Object, modelItem, eventPublisher.Object);
            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            if (string.IsNullOrEmpty(expectedErrorMessage))
            {
                Assert.IsNull(viewModel.Errors);
            }
            else
            {
                Assert.IsNotNull(viewModel.Errors);
                Assert.AreEqual(1, viewModel.Errors.Count);
                StringAssert.Contains(viewModel.Errors[0].Message, expectedErrorMessage);

                viewModel.Errors[0].Do();
                var isFocused = isFocusedProperty != null && (bool)viewModel.GetValue(isFocusedProperty);
                Assert.IsTrue(isFocused);
            }
        }
    }

    public class TestExchangeServiceModel : IExchangeServiceModel
    {
        public ObservableCollection<IExchangeSource> Sources { get; set; }
        readonly ObservableCollection<IExchangeSource> _sources = new ObservableCollection<IExchangeSource>
        {
            new ExchangeSourceDefinition
            {
                ResourceName = "TestExchange",
                Type = enSourceType.ExchangeSource,
                ResourceType = "ExchangeSource",
                AutoDiscoverUrl = "Localhost",
                UserName = "test",
                Password = "test",
            }
        };
        public TestExchangeServiceModel(bool emptySource = false)
        {
            if (emptySource)
            {
                _sources = new ObservableCollection<IExchangeSource>()
                {
                    new ExchangeSourceDefinition()
                };
            }
        }

        public ObservableCollection<IExchangeSource> RetrieveSources()
        {
            return _sources;
        }

        public void CreateNewSource() { }

        public void EditSource(IExchangeSource selectedSource) { }

        public IStudioUpdateManager UpdateRepository { get; set; }
    }
}
