#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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
using System.Diagnostics.CodeAnalysis;
using Warewolf.Studio.Models;

namespace Dev2.Activities.Designers2.ExchangeNewEmail
{
    public class ExchangeNewEmailDesignerViewModel : CustomToolWithRegionBase, IExchangeServiceViewModel
    {
        string _statusMessage;
        bool _testing;
        readonly IEventAggregator _eventPublisher;
        readonly IServer _server;
        readonly IAsyncWorker _asyncWorker;
        ISourceToolRegion<IExchangeSource> _sourceRegion;

        public RelayCommand TestEmailAccountCommand { get; private set; }
        public ICommand ChooseAttachmentsCommand { get; private set; }

        private readonly IShellViewModel _shellViewModel;
        readonly IActiveDataList _activeDataList;

        [ExcludeFromCodeCoverage]
        public ExchangeNewEmailDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), ServerRepository.Instance.ActiveServer, EventPublishers.Aggregator, CustomContainer.Get<IShellViewModel>(), DataListSingleton.Instance)
        {
        }

        public ExchangeNewEmailDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IExchangeServiceModel model, IEventAggregator eventPublisher, IShellViewModel shellViewModel, IActiveDataList activeDataList)
            : base(modelItem)
        {
            _shellViewModel = shellViewModel;
            _activeDataList = activeDataList;
            TestEmailAccountCommand = new RelayCommand(o => TestEmailAccount(), o => CanTestEmailAccount);
            ChooseAttachmentsCommand = new DelegateCommand(o => ChooseAttachments());
            _eventPublisher = eventPublisher;
            _asyncWorker = asyncWorker;
            Model = model;
            SetupCommonProperties();
        }

        public ExchangeNewEmailDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IServer server, IEventAggregator eventPublisher, IShellViewModel shellViewModel, IActiveDataList activeDataList)
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

            _shellViewModel = shellViewModel;
            _activeDataList = activeDataList;
            var model = new ExchangeServiceModel(_server, _shellViewModel);
            Model = model;
            SetupCommonProperties();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Email_Exchange_Send;
        }

        void SetupCommonProperties()
        {
            Testing = false;
            HasLargeView = true;
            InitialiseViewModel();
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

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged("StatusMessage");
            }
        }

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

            SourceRegion = new ExchangeSourceRegion(Model, ModelItem, "ExchangeSource");
            regions.Add(SourceRegion);

            return regions;
        }

        public static readonly DependencyProperty CanTestEmailAccountProperty = DependencyProperty.Register("CanTestEmailAccount", typeof(bool), typeof(ExchangeNewEmailDesignerViewModel), new PropertyMetadata(true));
        public bool IsEmailSourceFocused { get => (bool)GetValue(IsEmailSourceFocusedProperty); set => SetValue(IsEmailSourceFocusedProperty, value); }
        public static readonly DependencyProperty IsEmailSourceFocusedProperty = DependencyProperty.Register("IsEmailSourceFocused", typeof(bool), typeof(ExchangeNewEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsToFocused { get => (bool)GetValue(IsToFocusedProperty); set => SetValue(IsToFocusedProperty, value); }
        public static readonly DependencyProperty IsToFocusedProperty = DependencyProperty.Register("IsToFocused", typeof(bool), typeof(ExchangeNewEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsCcFocused { get => (bool)GetValue(IsCcFocusedProperty); set => SetValue(IsCcFocusedProperty, value); }
        public static readonly DependencyProperty IsCcFocusedProperty = DependencyProperty.Register("IsCcFocused", typeof(bool), typeof(ExchangeNewEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsBccFocused { get => (bool)GetValue(IsBccFocusedProperty); set => SetValue(IsBccFocusedProperty, value); }
        public static readonly DependencyProperty IsBccFocusedProperty = DependencyProperty.Register("IsBccFocused", typeof(bool), typeof(ExchangeNewEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsSubjectFocused { get => (bool)GetValue(IsSubjectFocusedProperty); set => SetValue(IsSubjectFocusedProperty, value); }
        public static readonly DependencyProperty IsSubjectFocusedProperty = DependencyProperty.Register("IsSubjectFocused", typeof(bool), typeof(ExchangeNewEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsAttachmentsFocused { get => (bool)GetValue(IsAttachmentsFocusedProperty); set => SetValue(IsAttachmentsFocusedProperty, value); }
        public static readonly DependencyProperty IsAttachmentsFocusedProperty = DependencyProperty.Register("IsAttachmentsFocused", typeof(bool), typeof(ExchangeNewEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        string To => GetProperty<string>();

        string Cc => GetProperty<string>();

        string Bcc => GetProperty<string>();

        string Attachments { get => GetProperty<string>(); set => SetProperty(value); }
        string Subject => GetProperty<string>();

        string Body => GetProperty<string>();

        bool IsHtml => GetProperty<bool>();

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
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(() => IsToFocused = true)
                    {
                        Message = ErrorResource.ToAddressRequired
                    }
                };
                return;
            }
            if (SourceRegion.SelectedSource == null)
            {
                Testing = false;
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(() => IsToFocused = true)
                    {
                        Message = ErrorResource.InvalidSource
                    }
                };
                return;
            }

            var testSource = new ExchangeSource
            {
                AutoDiscoverUrl = SourceRegion.SelectedSource.AutoDiscoverUrl,
                Password = SourceRegion.SelectedSource.Password,
                UserName = SourceRegion.SelectedSource.UserName
            };

            var testMessage = new ExchangeTestMessage
            {
                Body = Body,
                Subject = Subject
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
                    testSource.Send(new ExchangeEmailSender(testSource), testMessage, IsHtml);
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
            result.AddRange(ValidateRecipients());
            Errors = result.Count == 0 ? null : result;
        }

        IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            var datalist = _activeDataList.ActiveDataList.Resource.DataList;
            foreach (var error in GetRuleSet("EmailSource", datalist).ValidateRules("'Email Source'", () => IsEmailSourceFocused = true))
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

        IEnumerable<IActionableErrorInfo> ValidateRecipients()
        {
            var datalist = _activeDataList.ActiveDataList.Resource.DataList;
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
            _shellViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
}
