using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.ExchangeEmail;
using Dev2.Activities.Exchange;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Communication;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Exchange.Email
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExchangeEmailDesignerViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "Exchange Email";

        const string AppLocalhost = "http://localhost:3142";

        [TestInitialize]
        public void Initialize()
        {
            AppSettings.LocalHost = AppLocalhost;
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfExchangeEmailActivity());
        }

        static List<ExchangeSource> CreateEmailSources(int count)
        {
            var result = new List<ExchangeSource>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new ExchangeSource()
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "Email" + i,
                    UserName = "user" + i + "@test.com",
                    Password = "pasword" + i,
                    AutoDiscoverUrl = "http" + i
                });
            }

            return result;
        }

        static TestExchangeEmailDesignerViewModel CreateViewModel(ModelItem modelItem, IEventAggregator eve)
        {
            var mockModel = new TestExchangeServiceModel();
      
            return CreateViewModel(modelItem, mockModel,eve);
        }

        static TestExchangeEmailDesignerViewModel CreateViewModel(ModelItem modelItem, IExchangeServiceModel model, IEventAggregator eve)
        {
           
            var testEmailDesignerViewModel = new TestExchangeEmailDesignerViewModel(modelItem, model, eve)
            {
              
                GetDatalistString = () =>
                {
                    const string trueString = "True";
                    const string noneString = "None";
                    var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><b Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><h Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><r Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);
                    return datalist;
                }
            };

            testEmailDesignerViewModel.SourceRegion.SelectedSource = model.RetrieveSources().First();

            return testEmailDesignerViewModel;
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailDesignerViewModel_Constructor_AsyncWorkerIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new ExchangeEmailDesignerViewModel(CreateModelItem(), null, null, null);
            // ReSharper restore ObjectCreationAsStatement

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailDesignerViewModel_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new ExchangeEmailDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, null, null);
            // ReSharper restore ObjectCreationAsStatement

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailDesignerViewModel_Constructor_EventAggregatorIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new ExchangeEmailDesignerViewModel(CreateModelItem(), new Mock<IAsyncWorker>().Object, new Mock<IEnvironmentModel>().Object, null);
            // ReSharper restore ObjectCreationAsStatement

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void EmailDesignerViewModel_Constructor_ModelItemIsNew_InitializesProperties()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            var propertyChanged = false;
            modelItem.PropertyChanged += (sender, args) =>
            {
                propertyChanged = true;
            };
            const int EmailSourceCount = 1;

            var eventPublisher = new Mock<IEventAggregator>();
            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(modelItem,eventPublisher.Object);


            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.SourceRegion.EditSourceCommand);
            Assert.IsNotNull(viewModel.TestEmailAccountCommand);
            Assert.IsNotNull(viewModel.ChooseAttachmentsCommand);
            Assert.IsNotNull(viewModel.SourceRegion.Sources);
            Assert.IsTrue(viewModel.CanTestEmailAccount);
            Assert.AreEqual(EmailSourceCount, viewModel.SourceRegion.Sources.Count);
            Assert.AreEqual("TestExchange", viewModel.SourceRegion.SelectedSource.Name);
            Assert.IsTrue(propertyChanged);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void EmailDesignerViewModel_Constructor_ModelItemIsNotNew_InitializesProperties()
        {
            //------------Setup for test--------------------------
            const int EmailSourceCount = 1;
            var sources = CreateEmailSources(EmailSourceCount);
            var selectedEmailSource = sources.First();

            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedEmailSource", selectedEmailSource);

            var propertyChanged = false;
            modelItem.PropertyChanged += (sender, args) =>
            {
                propertyChanged = true;
            };
            var eventPublisher = new Mock<IEventAggregator>();
            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(modelItem,eventPublisher.Object);


            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.TestEmailAccountCommand);
            Assert.IsNotNull(viewModel.ChooseAttachmentsCommand);
            Assert.IsNotNull(viewModel.SourceRegion.Sources);
            Assert.IsTrue(viewModel.CanTestEmailAccount);

            Assert.AreEqual(EmailSourceCount, viewModel.SourceRegion.Sources.Count);
            Assert.AreEqual("TestExchange", viewModel.SourceRegion.SelectedSource.Name);

            Assert.IsTrue(propertyChanged);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void EmailDesignerViewModel_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount()
        {
            Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount("test@mydomain.com");
            Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount("");
        }

        void Verify_Constructor_DoesNotAutoCopyEmailSourceUserNameIntoFromAccount(string to)
        {
            //------------Setup for test--------------------------
            var activity = new DsfExchangeEmailActivity() { To = to };

            var emailSource = new ExchangeSource()
            {
                UserName = "bob@mydomain.com",
                Password = "MyPassword",
                ResourceID = Guid.NewGuid()
            };
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var eventPublisher = new Mock<IEventAggregator>();
            var viewModel = CreateViewModel(modelItem,eventPublisher.Object);

            //------------Execute Test---------------------------
            viewModel.SourceRegion.SelectedSource = emailSource;

            //------------Assert Results-------------------------
            var toReceipient = modelItem.GetProperty<string>("To");
            Assert.AreEqual(to, toReceipient);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void EmailDesignerViewModel_ChooseAttachments_PublishesFileChooserMessage()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();

            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<FileChooserMessage>())).Verifiable();

            var viewModel = CreateViewModel(modelItem,eventPublisher.Object);

            //------------Execute Test---------------------------
            viewModel.ChooseAttachmentsCommand.Execute(null);

            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<FileChooserMessage>()));
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void EmailDesignerViewModel_ChooseAttachments_SelectedFilesIsNotNull_AddsFilesToAttachments()
        {
            Verify_ChooseAttachments(new List<string> { @"c:\tmp2.txt", @"c:\logs\errors2.log" });
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

            var emailSources = CreateEmailSources(2);
            var modelItem = CreateModelItem();
            modelItem.SetProperty("Attachments", string.Join(";", existingFiles));

            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<FileChooserMessage>())).Callback((object m) =>
            {
                ((FileChooserMessage)m).SelectedFiles = selectedFiles;
            });

            var viewModel = CreateViewModel(modelItem,eventPublisher.Object);

            //------------Execute Test---------------------------
            viewModel.ChooseAttachmentsCommand.Execute(null);

            //------------Assert Results-------------------------
            eventPublisher.Verify(p => p.Publish(It.IsAny<FileChooserMessage>()));
            var attachments = modelItem.GetProperty<string>("Attachments");
            Assert.AreEqual(string.Join(";", expectedFiles), attachments);
        }

        
    }


    public class TestExchangeEmailDesignerViewModel : ExchangeEmailDesignerViewModel
    {
        public TestExchangeEmailDesignerViewModel(ModelItem modelItem, IExchangeServiceModel model,IEventAggregator eventPublisher)
            : base(modelItem, model,eventPublisher)
        {
        }

        public ExchangeSource SelectedEmailSourceModelItemValue
        {
            // ReSharper disable ExplicitCallerInfoArgument
            get { return GetProperty<ExchangeSource>("SelectedEmailSource"); }
            // ReSharper restore ExplicitCallerInfoArgument
            set
            {
                // ReSharper disable ExplicitCallerInfoArgument
                SetProperty(value, "SelectedEmailSource");
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        public IWebRequestInvoker WebRequestInvoker { get; set; }
        protected override IWebRequestInvoker CreateWebRequestInvoker()
        {
            return WebRequestInvoker;
        }
    }

    public class TestExchangeServiceModel : IExchangeServiceModel
    {
        
        private readonly ObservableCollection<IExchangeSource> _sources = new ObservableCollection<IExchangeSource>
        {
            new ExchangeSource()
            {
                Name = "TestExchange",
                Type = enSourceType.ExchangeSource,
                AutoDiscoverUrl = "Localhost",
                UserName = "test",
                Password = "test",
            }
        };

        public ObservableCollection<IExchangeSource> RetrieveSources()
        {
            return _sources;
        }

        public void CreateNewSource()
        {
            
        }

        public void EditSource(IExchangeSource selectedSource)
        {
            
        }

        public void SaveService(IExchangeSource model)
        {
            
        }

        public IStudioUpdateManager UpdateRepository { get; }
    }
}
