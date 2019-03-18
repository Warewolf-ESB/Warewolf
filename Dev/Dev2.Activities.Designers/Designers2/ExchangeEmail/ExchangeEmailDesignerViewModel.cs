#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Exchange;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Dev2.Validation;
using Warewolf.Resource.Errors;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.ExchangeEmail
{
    public class ExchangeEmailDesignerViewModel : CustomToolWithRegionBase, IExchangeServiceViewModel
    {
        readonly IEventAggregator _eventPublisher;
        readonly IServer _server;
        readonly IAsyncWorker _asyncWorker;
        ISourceToolRegion<IExchangeSource> _sourceRegion;

        public RelayCommand TestEmailAccountCommand { get; private set; }
        public ICommand ChooseAttachmentsCommand { get; private set; }

        public ExchangeEmailDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), ServerRepository.Instance.ActiveServer, EventPublishers.Aggregator)
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

        public ExchangeEmailDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IServer server, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _asyncWorker = asyncWorker;
            _server = server;
            _eventPublisher = eventPublisher;
            _eventPublisher.Subscribe(this);

            AddTitleBarLargeToggle();

            TestEmailAccountCommand = new RelayCommand(o => TestEmailAccount(), o => CanTestEmailAccount);
            ChooseAttachmentsCommand = new DelegateCommand(o => ChooseAttachments());

            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var model = CustomContainer.CreateInstance<IExchangeServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;
            SetupCommonProperties();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Email_Exchange_Send;
        }

        void SetupCommonProperties()
        {
            Testing = false;
            AddTitleBarMappingToggle();
            InitialiseViewModel();
        }

        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }

        void InitialiseViewModel()
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
            get => _sourceRegion;
            set
            {
                _sourceRegion = value;
                OnPropertyChanged();
            }
        }

        string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged("StatusMessage");
            }
        }

        bool _testing;

        public bool Testing
        {
            get => _testing;
            set
            {
                _testing = value;
                OnPropertyChanged("Testing");
            }
        }

        IExchangeServiceModel Model { get; set; }
        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();

            SourceRegion = new ExchangeSourceRegion(Model,ModelItem, "ExchangeSource");
            regions.Add(SourceRegion);

            return regions;
        }

        public static readonly DependencyProperty CanTestEmailAccountProperty = DependencyProperty.Register("CanTestEmailAccount", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(true));
        public bool IsEmailSourceFocused { get => (bool)GetValue(IsEmailSourceFocusedProperty); set => SetValue(IsEmailSourceFocusedProperty, value); }
        public static readonly DependencyProperty IsEmailSourceFocusedProperty = DependencyProperty.Register("IsEmailSourceFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsToFocused { get => (bool)GetValue(IsToFocusedProperty); set => SetValue(IsToFocusedProperty, value); }
        public static readonly DependencyProperty IsToFocusedProperty = DependencyProperty.Register("IsToFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsCcFocused { get => (bool)GetValue(IsCcFocusedProperty); set => SetValue(IsCcFocusedProperty, value); }
        public static readonly DependencyProperty IsCcFocusedProperty = DependencyProperty.Register("IsCcFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsBccFocused { get => (bool)GetValue(IsBccFocusedProperty); set => SetValue(IsBccFocusedProperty, value); }
        public static readonly DependencyProperty IsBccFocusedProperty = DependencyProperty.Register("IsBccFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsSubjectFocused { get => (bool)GetValue(IsSubjectFocusedProperty); set => SetValue(IsSubjectFocusedProperty, value); }
        public static readonly DependencyProperty IsSubjectFocusedProperty = DependencyProperty.Register("IsSubjectFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsAttachmentsFocused { get => (bool)GetValue(IsAttachmentsFocusedProperty); set => SetValue(IsAttachmentsFocusedProperty, value); }
        public static readonly DependencyProperty IsAttachmentsFocusedProperty = DependencyProperty.Register("IsAttachmentsFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));
        string To => GetProperty<string>();

        string Cc => GetProperty<string>();

        string Bcc => GetProperty<string>();

        string Attachments { get => GetProperty<string>(); set => SetProperty(value); }
        string Subject => GetProperty<string>();

        string Body => GetProperty<string>();

        public bool CanTestEmailAccount
        {
            get => (bool)GetValue(CanTestEmailAccountProperty);
            set
            {
                SetValue(CanTestEmailAccountProperty, value);
                TestEmailAccountCommand.RaiseCanExecuteChanged();
            }
        }

        public void TestEmailAccount()
        {
            if (Errors != null && Errors.Count > 0)
            {
                return;
            }

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

            var testSource = new ExchangeSource
            {
                AutoDiscoverUrl = SourceRegion.SelectedSource.AutoDiscoverUrl,
                Password = SourceRegion.SelectedSource.Password,
                UserName = SourceRegion.SelectedSource.UserName,
            };

            var testMessage = new ExchangeTestMessage
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

        void SendEmail(ExchangeSource testSource, ExchangeTestMessage testMessage)
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
            const string Separator = ";";
            var message = new FileChooserMessage
            {
                SelectedFiles = Attachments.Split(Separator.ToCharArray())
            };
            message.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SelectedFiles" && message.SelectedFiles != null)
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

            };
            _eventPublisher.Publish(message);
        }

        public override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());
            Errors = result.Count == 0 ? null : result;
        }

        IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            var datalist = DataListSingleton.ActiveDataList.Resource.DataList;
            foreach (var error in GetRuleSet("EmailSource", datalist).ValidateRules("'Email Source'", () => IsEmailSourceFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Recipients", datalist).ValidateRules("'To', 'Cc' or 'Bcc'", () => IsToFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("To", datalist).ValidateRules("'To'", () => IsToFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Cc", datalist).ValidateRules("'Cc'", () => IsCcFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Bcc", datalist).ValidateRules("'Bcc'", () => IsBccFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("SubjectAndBody", datalist).ValidateRules("'Subject' or 'Body'", () => IsSubjectFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Attachments", datalist).ValidateRules("'Attachments'", () => IsAttachmentsFocused = true))
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
                    var toExprRule = new IsValidExpressionRule(() => To, datalist, "user@test.com", new VariableUtils());
                    ruleSet.Add(toExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => toExprRule.ExpressionValue));
                    break;
                case "Cc":
                    var ccExprRule = new IsValidExpressionRule(() => Cc, datalist, "user@test.com", new VariableUtils());
                    ruleSet.Add(ccExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => ccExprRule.ExpressionValue));
                    break;
                case "Bcc":
                    var bccExprRule = new IsValidExpressionRule(() => Bcc, datalist, "user@test.com", new VariableUtils());
                    ruleSet.Add(bccExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => bccExprRule.ExpressionValue));
                    break;
                case "Attachments":
                    var attachmentsExprRule = new IsValidExpressionRule(() => Attachments, datalist, @"c:\test.txt", new VariableUtils());
                    ruleSet.Add(attachmentsExprRule);
                    ruleSet.Add(new IsValidFileNameRule(() => attachmentsExprRule.ExpressionValue));
                    break;
                case "Recipients":
                    ruleSet.Add(new HasAtLeastOneRule(() => To, () => Cc, () => Bcc));
                    break;
                case "SubjectAndBody":
                    ruleSet.Add(new HasAtLeastOneRule(() => Subject, () => Body));
                    break;
                default:
                    break;
            }
            return ruleSet;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
}
