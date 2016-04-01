﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Dev2.Util;
using Dev2.Validation;
using Action = System.Action;
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UseNullPropagation
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable ArrangeTypeMemberModifiers

namespace Dev2.Activities.Designers2.ExchangeEmail
{
    public class ExchangeEmailDesignerViewModel : CustomToolWithRegionBase
    {
        // ReSharper disable UnusedMember.Local
        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;

        readonly string _sourceNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotSelected;
        readonly string _methodNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.PluginServiceMethodNotSelected;
        readonly string _serviceExecuteOnline = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteOnline;
        readonly string _serviceExecuteLoginPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteLoginPermission;
        readonly string _serviceExecuteViewPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteViewPermission;
        // ReSharper restore UnusedMember.Local

        readonly IEventAggregator _eventPublisher;
        readonly IEnvironmentModel _environmentModel;
        readonly IAsyncWorker _asyncWorker;
        private ISourceToolRegion<IExchangeSource> _sourceRegion;
        public ObservableCollection<ExchangeSource> EmailSources { get; private set; }
        public ObservableCollection<enMailPriorityEnum> Priorities { get; private set; }

        public RelayCommand EditEmailSourceCommand { get; private set; }
        public RelayCommand TestEmailAccountCommand { get; private set; }
        public ICommand ChooseAttachmentsCommand { get; private set; }

        public ExchangeEmailDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
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

            EmailSources = new ObservableCollection<ExchangeSource>();
            Priorities = new ObservableCollection<enMailPriorityEnum> { enMailPriorityEnum.High, enMailPriorityEnum.Normal, enMailPriorityEnum.Low };
            TestEmailAccountCommand = new RelayCommand(o => TestEmailAccount(), o => CanTestEmailAccount);
            ChooseAttachmentsCommand = new DelegateCommand(o => ChooseAttachments());

            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<IExchangeServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;
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

        private IExchangeServiceModel Model { get; set; }
        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();

            SourceRegion = new ExchangeSourceRegion(Model,ModelItem, enSourceType.EmailSource);
            regions.Add(SourceRegion);

            return regions;
        }

        private Action _sourceChangedAction;
        public Action SourceChangedAction
        {
            get
            {
                return _sourceChangedAction ?? (() => { });
            }
            set
            {
                _sourceChangedAction = value;
            }
        }

        public static readonly DependencyProperty CanTestEmailAccountProperty = DependencyProperty.Register("CanTestEmailAccount", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(true));

        public bool IsEmailSourceFocused { get { return (bool)GetValue(IsEmailSourceFocusedProperty); } set { SetValue(IsEmailSourceFocusedProperty, value); } }
        public static readonly DependencyProperty IsEmailSourceFocusedProperty = DependencyProperty.Register("IsEmailSourceFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsFromAccountFocused { get { return (bool)GetValue(IsFromAccountFocusedProperty); } set { SetValue(IsFromAccountFocusedProperty, value); } }
        public static readonly DependencyProperty IsFromAccountFocusedProperty = DependencyProperty.Register("IsFromAccountFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsPasswordFocused { get { return (bool)GetValue(IsPasswordFocusedProperty); } set { SetValue(IsPasswordFocusedProperty, value); } }
        public static readonly DependencyProperty IsPasswordFocusedProperty = DependencyProperty.Register("IsPasswordFocused", typeof(bool), typeof(ExchangeEmailDesignerViewModel), new PropertyMetadata(default(bool)));

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

        public string Password { get { return GetProperty<string>(); } set { SetProperty(value); } }
        string FromAccount { get { return GetProperty<string>(); } }
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

        void TestEmailAccount()
        {
            var testEmailAccount = GetTestEmailAccount();
            if (string.IsNullOrEmpty(testEmailAccount))
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(() => IsToFocused = true) { Message = "Please supply a To address in order to Test." } };
                return;
            }
            CanTestEmailAccount = false;

            //Todo Implement Exchange Logic
            var testSource = new ExchangeSource();
            if (!string.IsNullOrEmpty(FromAccount))
            {
                testSource.UserName = FromAccount;
                testSource.Password = Password;
            }
            testSource.TestFromAddress = testSource.UserName;
            testSource.TestToAddress = testEmailAccount;
            if (EmailAddresssIsAVariable(testEmailAccount))
            {
                return;
            }
            Uri uri = new Uri(new Uri(AppSettings.LocalHost), "wwwroot/sources/Service/EmailSources/Test");
            var jsonData = testSource.ToString();

            var requestInvoker = CreateWebRequestInvoker();
            requestInvoker.ExecuteRequest("POST", uri.ToString(), jsonData, null, OnTestCompleted);
        }

        string GetTestEmailAccount()
        {
            var addresses = new List<string>();
            addresses.AddRange(To.Split(';'));
            addresses.AddRange(Cc.Split(';'));
            addresses.AddRange(Bcc.Split(';'));
            return addresses.FirstOrDefault();
        }

        bool EmailAddresssIsAVariable(string testEmailAccount)
        {
            var postResult = "";
            var hasVariable = false;
            if (DataListUtil.IsFullyEvaluated(testEmailAccount))
            {
                postResult += "Variable " + testEmailAccount + " cannot be used while testing.";
                hasVariable = true;
            }
            if (DataListUtil.IsFullyEvaluated(FromAccount))
            {
                var errorMessage = "Variable " + FromAccount + " cannot be used while testing.";
                if (string.IsNullOrEmpty(postResult))
                {
                    postResult += errorMessage;
                }
                else
                {
                    postResult += Environment.NewLine + errorMessage;
                }
                hasVariable = true;
            }
            if (hasVariable)
            {
                var validationResult = new ValidationResult
                {
                    ErrorMessage = postResult,
                    IsValid = false
                };
                OnTestCompleted(new Dev2JsonSerializer().Serialize(validationResult));
                return true;
            }
            return false;
        }

        void OnTestCompleted(string postResult)
        {
            try
            {
                var result = new Dev2JsonSerializer().Deserialize<ValidationResult>(postResult);
                Errors = result.IsValid
                    ? null
                    : new List<IActionableErrorInfo> { new ActionableErrorInfo(() => IsFromAccountFocused = true) { Message = result.ErrorMessage } };
            }
            finally
            {
                CanTestEmailAccount = true;
            }
        }

        protected virtual IWebRequestInvoker CreateWebRequestInvoker()
        {
            return new WebRequestInvoker();
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
            foreach (var error in GetRuleSet("FromAccount", GetDatalistString()).ValidateRules("'From Account'", () => IsFromAccountFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Password", GetDatalistString()).ValidateRules("'Password'", () => IsPasswordFocused = true))
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
                case "FromAccount":
                    var fromExprRule = new IsValidExpressionRule(() => FromAccount, datalist, "user@test.com");
                    ruleSet.Add(fromExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => fromExprRule.ExpressionValue));
                    break;
                case "Password":
                    ruleSet.Add(new IsRequiredWhenOtherIsNotEmptyRule(() => Password, () => FromAccount));
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
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }
    }
}
