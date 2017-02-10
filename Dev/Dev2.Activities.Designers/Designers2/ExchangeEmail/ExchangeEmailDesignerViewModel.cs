using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Exchange;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Dev2.Validation;
using Warewolf.Resource.Errors;
// ReSharper disable UnusedMember.Global

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UseNullPropagation
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MergeSequentialChecks

namespace Dev2.Activities.Designers2.ExchangeEmail
{
    public class ExchangeEmailDesignerViewModel : CustomToolWithRegionBase, IExchangeServiceViewModel
    {

        readonly IEventAggregator _eventPublisher;
        readonly IEnvironmentModel _environmentModel;
        readonly IAsyncWorker _asyncWorker;
        private ISourceToolRegion<IExchangeSource> _sourceRegion;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public RelayCommand TestEmailAccountCommand { get; private set; }
        public ICommand ChooseAttachmentsCommand { get; private set; }

        public ExchangeEmailDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public ExchangeEmailDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IExchangeServiceModel model,IEventAggregator eventPublisher) : base(modelItem)
        {
            TestEmailAccountCommand = new RelayCommand(o => TestEmailAccount(), o => CanTestEmailAccount);
            ChooseAttachmentsCommand = new DelegateCommand(o => ChooseAttachments());
            _eventPublisher = eventPublisher;
            _asyncWorker = asyncWorker;
            Model = model;
            SetupCommonProperties();
        }

        public ExchangeEmailDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("environmentModel", environmentModel);
            _asyncWorker = asyncWorker;
            _environmentModel = environmentModel;
            _eventPublisher = eventPublisher;
            _eventPublisher.Subscribe(this);

            AddTitleBarLargeToggle();

            TestEmailAccountCommand = new RelayCommand(o => TestEmailAccount(), o => CanTestEmailAccount);
            ChooseAttachmentsCommand = new DelegateCommand(o => ChooseAttachments());

            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<IExchangeServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;
            SetupCommonProperties();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Email_Exchange_Send;
        }

        private void SetupCommonProperties()
        {
            Testing = false;
            AddTitleBarMappingToggle();
            InitialiseViewModel();
        }
        
        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }

        private void InitialiseViewModel()
        {
            BuildRegions();
            InitializeProperties();
        }

        public List<KeyValuePair<string, string>> Properties { get; set; }
        void InitializeProperties()
        {
            Properties = new List<KeyValuePair<string, string>>();
            AddProperty("Source :", SourceRegion.SelectedSource == null ? "" : SourceRegion.SelectedSource.ResourceName);
        }

        public void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public ISourceToolRegion<IExchangeSource> SourceRegion
        {
            get
            {
                return _sourceRegion;
            }
            set
            {
                _sourceRegion = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;

                OnPropertyChanged("StatusMessage");
            }
        }

        private bool _testing;

        public bool Testing
        {
            get
            {
                return _testing;
            }
             set
            {
                _testing = value;

                OnPropertyChanged("Testing");
            }
        }

        private IExchangeServiceModel Model { get; set; }
        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();

            SourceRegion = new ExchangeSourceRegion(Model,ModelItem, "ExchangeSource");
            regions.Add(SourceRegion);

            return regions;
        }

        public static readonly DependencyProperty CanTestEmailAccountProperty = DependencyProperty.Register("CanTestEmailAccount", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(true));
        public bool IsEmailSourceFocused { get { return (bool)GetValue(IsEmailSourceFocusedProperty); } set { SetValue(IsEmailSourceFocusedProperty, value); } }
        public static readonly DependencyProperty IsEmailSourceFocusedProperty = DependencyProperty.Register("IsEmailSourceFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));
        
        public bool IsToFocused { get { return (bool)GetValue(IsToFocusedProperty); } set { SetValue(IsToFocusedProperty, value); } }
        public static readonly DependencyProperty IsToFocusedProperty = DependencyProperty.Register("IsToFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsCcFocused { get { return (bool)GetValue(IsCcFocusedProperty); } set { SetValue(IsCcFocusedProperty, value); } }
        public static readonly DependencyProperty IsCcFocusedProperty = DependencyProperty.Register("IsCcFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsBccFocused { get { return (bool)GetValue(IsBccFocusedProperty); } set { SetValue(IsBccFocusedProperty, value); } }
        public static readonly DependencyProperty IsBccFocusedProperty = DependencyProperty.Register("IsBccFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsSubjectFocused { get { return (bool)GetValue(IsSubjectFocusedProperty); } set { SetValue(IsSubjectFocusedProperty, value); } }
        public static readonly DependencyProperty IsSubjectFocusedProperty = DependencyProperty.Register("IsSubjectFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsAttachmentsFocused { get { return (bool)GetValue(IsAttachmentsFocusedProperty); } set { SetValue(IsAttachmentsFocusedProperty, value); } }
        public static readonly DependencyProperty IsAttachmentsFocusedProperty = DependencyProperty.Register("IsAttachmentsFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));
        string To { get { return GetProperty<string>(); } }
        string Cc { get { return GetProperty<string>(); } }
        string Bcc { get { return GetProperty<string>(); } }
        string Attachments { get { return GetProperty<string>(); } set { SetProperty(value); } }
        string Subject { get { return GetProperty<string>(); } }
        string Body { get { return GetProperty<string>(); } }

        public bool CanTestEmailAccount
        {
            get
            {
                return (bool)GetValue(CanTestEmailAccountProperty);
            }
            set
            {
                SetValue(CanTestEmailAccountProperty, value);
                TestEmailAccountCommand.RaiseCanExecuteChanged();
            }
        }

        public void TestEmailAccount()
        {

            if (Errors != null && Errors.Count > 0)return;
           
            Testing = true;
            StatusMessage = string.Empty;

            if (string.IsNullOrEmpty(To))
            {
                Testing = false;
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(() => IsToFocused = true) { Message = ErrorResource.ToAddressRequired} };
                return;
            }
            if (SourceRegion.SelectedSource == null)
            {
                Testing = false;
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(() => IsToFocused = true) { Message = ErrorResource.InvalidSource } };
                return;
            }
        
            var testSource = new ExchangeSource()
            {
                AutoDiscoverUrl = SourceRegion.SelectedSource.AutoDiscoverUrl,
                Password = SourceRegion.SelectedSource.Password,
                UserName = SourceRegion.SelectedSource.UserName,
            };

            var testMessage = new ExchangeTestMessage()
            {
                Body = Body,
                Subject = Subject,
            };

            if (!string.IsNullOrEmpty(Attachments))
            {
                var attachments = Attachments.Split(';');
                testMessage.Attachments.AddRange(attachments);
            }

            if (!string.IsNullOrEmpty(To))
            {
                var tos = To.Split(';');
                testMessage.Tos.AddRange(tos);
            }

            if (!string.IsNullOrEmpty(Cc))
            {
                var ccs = Cc.Split(';');
                testMessage.CCs.AddRange(ccs);
            }

            if (!string.IsNullOrEmpty(Bcc))
            {
                var bccs = Bcc.Split(';');
                testMessage.BcCs.AddRange(bccs);
            }

            SendEmail(testSource, testMessage);
        }

        private void SendEmail(ExchangeSource testSource, ExchangeTestMessage testMessage)
        {
            _asyncWorker.Start(() =>
            {
                try
                {
                    testSource.Send(new ExchangeEmailSender(testSource), testMessage);
                }
                catch (Exception)
                {
                    SetStatusMessage("One or more errors occured");
                }
                finally
                {
                    Testing = false;
                }
            });
        }

        public void SetStatusMessage(string message)
        {
            StatusMessage = message;
        }


        void ChooseAttachments()
        {

            // ReSharper disable once InconsistentNaming
            const string Separator = ";";
            var message = new FileChooserMessage();
            message.SelectedFiles = Attachments.Split(Separator.ToCharArray());
            message.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SelectedFiles")
                {
                    if (message.SelectedFiles != null)
                    {
                        if (string.IsNullOrEmpty(Attachments))
                        {
                            Attachments = string.Join(Separator, message.SelectedFiles);
                        }
                        else
                        {
                            Attachments += Separator + string.Join(Separator, message.SelectedFiles);
                        }
                    }
                }
            };
            _eventPublisher.Publish(message);
        }

        public override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());
            Errors = result.Count == 0 ? null : result;
        }

        public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;

        IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            foreach (var error in GetRuleSet("EmailSource", GetDatalistString()).ValidateRules("'Email Source'", () => IsEmailSourceFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Recipients", GetDatalistString()).ValidateRules("'To', 'Cc' or 'Bcc'", () => IsToFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("To", GetDatalistString()).ValidateRules("'To'", () => IsToFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Cc", GetDatalistString()).ValidateRules("'Cc'", () => IsCcFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Bcc", GetDatalistString()).ValidateRules("'Bcc'", () => IsBccFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("SubjectAndBody", GetDatalistString()).ValidateRules("'Subject' or 'Body'", () => IsSubjectFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Attachments", GetDatalistString()).ValidateRules("'Attachments'", () => IsAttachmentsFocused = true))
            {
                yield return error;
            }
        }

        IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();

            switch (propertyName)
            {
                case "EmailSource":
                    ruleSet.Add(new IsNullRule(() => SourceRegion.SelectedSource));
                    break;
                case "To":
                    var toExprRule = new IsValidExpressionRule(() => To, datalist, "user@test.com");
                    ruleSet.Add(toExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => toExprRule.ExpressionValue));
                    break;
                case "Cc":
                    var ccExprRule = new IsValidExpressionRule(() => Cc, datalist, "user@test.com");
                    ruleSet.Add(ccExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => ccExprRule.ExpressionValue));
                    break;
                case "Bcc":
                    var bccExprRule = new IsValidExpressionRule(() => Bcc, datalist, "user@test.com");
                    ruleSet.Add(bccExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => bccExprRule.ExpressionValue));
                    break;
                case "Attachments":
                    var attachmentsExprRule = new IsValidExpressionRule(() => Attachments, datalist, @"c:\test.txt");
                    ruleSet.Add(attachmentsExprRule);
                    ruleSet.Add(new IsValidFileNameRule(() => attachmentsExprRule.ExpressionValue));
                    break;
                case "Recipients":
                    ruleSet.Add(new HasAtLeastOneRule(() => To, () => Cc, () => Bcc));
                    break;
                case "SubjectAndBody":
                    ruleSet.Add(new HasAtLeastOneRule(() => Subject, () => Body));
                    break;
            }
            return ruleSet;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();

            if (mainViewModel != null)
            {
                SetHelpModelHelpText(helpText, mainViewModel);
            }
        }

        private void SetHelpModelHelpText(string helpText, IMainViewModel mainViewModel)
        {
            mainViewModel.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}







