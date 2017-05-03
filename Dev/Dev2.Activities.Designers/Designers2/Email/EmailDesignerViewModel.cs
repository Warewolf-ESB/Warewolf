/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Dev2.Util;
using Dev2.Validation;
using Warewolf.Resource.Errors;

namespace Dev2.Activities.Designers2.Email
{
    public class EmailDesignerViewModel : ActivityDesignerViewModel, IHandle<UpdateResourceMessage>
    {
        static readonly EmailSource NewEmailSource = new EmailSource { ResourceID = Guid.NewGuid(), ResourceName = "New Email Source..." };
        static readonly EmailSource SelectEmailSource = new EmailSource { ResourceID = Guid.NewGuid(), ResourceName = "Select an Email Source..." };

        readonly IEventAggregator _eventPublisher;
        readonly IServer _server;
        readonly IAsyncWorker _asyncWorker;

        bool _isInitializing;
        public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;

        public EmailDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), ServerRepository.Instance.ActiveServer, EventPublishers.Aggregator)
        {
            this.RunViewSetup();
        }

        public EmailDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IServer server, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("environmentModel", server);
            _asyncWorker = asyncWorker;
            _server = server;
            _eventPublisher = eventPublisher;
            _eventPublisher.Subscribe(this);

            AddTitleBarLargeToggle();

            EmailSources = new ObservableCollection<EmailSource>();
            Priorities = new ObservableCollection<enMailPriorityEnum> { enMailPriorityEnum.High, enMailPriorityEnum.Normal, enMailPriorityEnum.Low };

            EditEmailSourceCommand = new RelayCommand(o => EditEmailSource(), o => IsEmailSourceSelected);
            TestEmailAccountCommand = new RelayCommand(o => TestEmailAccount(), o => CanTestEmailAccount);
            ChooseAttachmentsCommand = new DelegateCommand(o => ChooseAttachments());

            RefreshSources(true);
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Email_SMTP_Send;
        }

        public EmailSource SelectedEmailSource
        {
            get
            {
                return (EmailSource)GetValue(SelectedEmailSourceProperty);
            }
            set
            {
                SetValue(SelectedEmailSourceProperty, value);
                EditEmailSourceCommand.RaiseCanExecuteChanged();
            }
        }
        public static readonly DependencyProperty SelectedEmailSourceProperty = DependencyProperty.Register("SelectedEmailSource", typeof(EmailSource), typeof(EmailDesignerViewModel), new PropertyMetadata(null, OnSelectedEmailSourceChanged));

        static void OnSelectedEmailSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (EmailDesignerViewModel)d;

            viewModel.OnSelectedEmailSourceChanged();
        }

        public RelayCommand EditEmailSourceCommand { get; }
        public RelayCommand TestEmailAccountCommand { get; }
        public ICommand ChooseAttachmentsCommand { get; private set; }

        public bool IsEmailSourceSelected => SelectedEmailSource != SelectEmailSource;

        public bool CanEditSource { get; set; }

        public ObservableCollection<EmailSource> EmailSources { get; }
        public ObservableCollection<enMailPriorityEnum> Priorities { get; private set; }

        public bool IsRefreshing { get { return (bool)GetValue(IsRefreshingProperty); } set { SetValue(IsRefreshingProperty, value); } }
        public static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.Register("IsRefreshing", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

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
        public static readonly DependencyProperty CanTestEmailAccountProperty = DependencyProperty.Register("CanTestEmailAccount", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(true));

        public bool IsEmailSourceFocused { get { return (bool)GetValue(IsEmailSourceFocusedProperty); } set { SetValue(IsEmailSourceFocusedProperty, value); } }
        public static readonly DependencyProperty IsEmailSourceFocusedProperty = DependencyProperty.Register("IsEmailSourceFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsFromAccountFocused { get { return (bool)GetValue(IsFromAccountFocusedProperty); } set { SetValue(IsFromAccountFocusedProperty, value); } }
        public static readonly DependencyProperty IsFromAccountFocusedProperty = DependencyProperty.Register("IsFromAccountFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsPasswordFocused { get { return (bool)GetValue(IsPasswordFocusedProperty); } set { SetValue(IsPasswordFocusedProperty, value); } }
        public static readonly DependencyProperty IsPasswordFocusedProperty = DependencyProperty.Register("IsPasswordFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsToFocused { get { return (bool)GetValue(IsToFocusedProperty); } set { SetValue(IsToFocusedProperty, value); } }
        public static readonly DependencyProperty IsToFocusedProperty = DependencyProperty.Register("IsToFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsCcFocused { get { return (bool)GetValue(IsCcFocusedProperty); } set { SetValue(IsCcFocusedProperty, value); } }
        public static readonly DependencyProperty IsCcFocusedProperty = DependencyProperty.Register("IsCcFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsBccFocused { get { return (bool)GetValue(IsBccFocusedProperty); } set { SetValue(IsBccFocusedProperty, value); } }
        public static readonly DependencyProperty IsBccFocusedProperty = DependencyProperty.Register("IsBccFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsSubjectFocused { get { return (bool)GetValue(IsSubjectFocusedProperty); } set { SetValue(IsSubjectFocusedProperty, value); } }
        public static readonly DependencyProperty IsSubjectFocusedProperty = DependencyProperty.Register("IsSubjectFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsAttachmentsFocused { get { return (bool)GetValue(IsAttachmentsFocusedProperty); } set { SetValue(IsAttachmentsFocusedProperty, value); } }
        public static readonly DependencyProperty IsAttachmentsFocusedProperty = DependencyProperty.Register("IsAttachmentsFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        // DO NOT bind to these properties - these are here for convenience only!!!
        public string Password { get { return GetProperty<string>(); } set { SetProperty(value); } }
        string FromAccount => GetProperty<string>();
        string To => GetProperty<string>();
        string Cc => GetProperty<string>();
        string Bcc => GetProperty<string>();
        string Attachments { get { return GetProperty<string>(); } set { SetProperty(value); } }
        string Subject => GetProperty<string>();
        string Body => GetProperty<string>();

        EmailSource EmailSource
        {
            // ReSharper disable ExplicitCallerInfoArgument
            get { return GetProperty<EmailSource>("SelectedEmailSource"); }
            // ReSharper restore ExplicitCallerInfoArgument
            set
            {
                if(!_isInitializing)
                {
                    // ReSharper disable ExplicitCallerInfoArgument
                    SetProperty(value, "SelectedEmailSource");
                    // ReSharper restore ExplicitCallerInfoArgument
                }
            }
        }

        public void CreateEmailSource()
        {
            CustomContainer.Get<IShellViewModel>().NewEmailSource(string.Empty);
            RefreshSources();
        }

        public void EditEmailSource()
        {
            var def = new EmailServiceSourceDefinition
            {
                Id = SelectedEmailSource.ResourceID,
                HostName = SelectedEmailSource.Host,
                Password = SelectedEmailSource.Password,
                UserName = SelectedEmailSource.UserName,
                Port = SelectedEmailSource.Port,
                Timeout = SelectedEmailSource.Timeout,
                ResourceName = SelectedEmailSource.ResourceName,
                EnableSsl = SelectedEmailSource.EnableSsl
            };

            CustomContainer.Get<IShellViewModel>().EditResource(def);

        }

        string GetTestEmailAccount()
        {
            var addresses = new List<string>();
            addresses.AddRange(To.Split(';'));
            addresses.AddRange(Cc.Split(';'));
            addresses.AddRange(Bcc.Split(';'));
            return addresses.FirstOrDefault();
        }

        void TestEmailAccount()
        {
            var testEmailAccount = GetTestEmailAccount();
            if(string.IsNullOrEmpty(testEmailAccount))
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(() => IsToFocused = true) { Message = ErrorResource.ToAddressRequired } };
                return;
            }
            CanTestEmailAccount = false;

            var testSource = new EmailSource(SelectedEmailSource.ToXml());
            if(!string.IsNullOrEmpty(FromAccount))
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

        bool EmailAddresssIsAVariable(string testEmailAccount)
        {
            var postResult = "";
            var hasVariable = false;
            if(DataListUtil.IsFullyEvaluated(testEmailAccount))
            {
                postResult += "Variable " + testEmailAccount + " cannot be used while testing.";
                hasVariable = true;
            }
            if(DataListUtil.IsFullyEvaluated(FromAccount))
            {
                var errorMessage = "Variable " + FromAccount + " cannot be used while testing.";
                if(string.IsNullOrEmpty(postResult))
                {
                    postResult += errorMessage;
                }
                else
                {
                    postResult += Environment.NewLine + errorMessage;
                }
                hasVariable = true;
            }
            if(hasVariable)
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

        protected virtual void OnSelectedEmailSourceChanged()
        {
            if(SelectedEmailSource == NewEmailSource)
            {
                CreateEmailSource();
                return;
            }

            IsRefreshing = true;

            if(SelectedEmailSource != SelectEmailSource)
            {
                EmailSources.Remove(SelectEmailSource);
            }
            EmailSource = SelectedEmailSource;
            EditEmailSourceCommand.RaiseCanExecuteChanged();
        }

        void RefreshSources(bool isInitializing = false)
        {
            IsRefreshing = true;
            var selectedEmailSource = EmailSource;
            if(isInitializing)
            {
                _isInitializing = true;
            }
            LoadSources(() =>
            {
                SetSelectedEmailSource(selectedEmailSource);
                IsRefreshing = false;
                if(isInitializing)
                {
                    _isInitializing = false;
                }
            });
        }

        void SetSelectedEmailSource(Resource source)
        {
            var selectedSource = source == null ? null : EmailSources.FirstOrDefault(d => d.ResourceID == source.ResourceID);
            if(selectedSource == null)
            {
                if(EmailSources.FirstOrDefault(d => d.Equals(SelectEmailSource)) == null)
                {
                    EmailSources.Insert(0, SelectEmailSource);
                }
                selectedSource = SelectEmailSource;
            }
            SelectedEmailSource = selectedSource;
        }

        void LoadSources(System.Action continueWith = null)
        {
            EmailSources.Clear();
            EmailSources.Add(NewEmailSource);

            _asyncWorker.Start(() => GetEmailSources().OrderBy(r => r.ResourceName), sources =>
            {
                foreach(var source in sources)
                {
                    EmailSources.Add(source);
                }
                continueWith?.Invoke();
            });
        }

        IEnumerable<EmailSource> GetEmailSources()
        {
            return _server.ResourceRepository.FindSourcesByType<EmailSource>(_server, enSourceType.EmailSource);
        }

        void ChooseAttachments()
        {
            const string Separator = @";";
            var message = new FileChooserMessage { SelectedFiles = Attachments.Split(Separator.ToCharArray()) };
            message.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == @"SelectedFiles")
                {
                    if (message.SelectedFiles == null || !message.SelectedFiles.Any())
                    {
                        Attachments = "";
                    }
                    else
                    {
                        if (message.SelectedFiles != null)
                        {
                            Attachments = string.Join(Separator, message.SelectedFiles);
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

        IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            foreach(var error in GetRuleSet("EmailSource", GetDatalistString()).ValidateRules("'Email Source'", () => IsEmailSourceFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("FromAccount", GetDatalistString()).ValidateRules("'From Account'", () => IsFromAccountFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Password", GetDatalistString()).ValidateRules("'Password'", () => IsPasswordFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Recipients", GetDatalistString()).ValidateRules("'To', 'Cc' or 'Bcc'", () => IsToFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("To", GetDatalistString()).ValidateRules("'To'", () => IsToFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Cc", GetDatalistString()).ValidateRules("'Cc'", () => IsCcFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Bcc", GetDatalistString()).ValidateRules("'Bcc'", () => IsBccFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("SubjectAndBody", GetDatalistString()).ValidateRules("'Subject' or 'Body'", () => IsSubjectFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Attachments", GetDatalistString()).ValidateRules("'Attachments'", () => IsAttachmentsFocused = true))
            {
                yield return error;
            }
        }

        IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();

            switch(propertyName)
            {
                case "EmailSource":
                    ruleSet.Add(new IsNullRule(() => EmailSource));
                    break;
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

        public void Handle(UpdateResourceMessage message)
        {
            var selectedSource = new EmailSource(message.ResourceModel.WorkflowXaml.ToXElement());
            if(EmailSource == null)
            {
                EmailSource = selectedSource;
            }
            else
            {
                if(selectedSource.ResourceID == EmailSource.ResourceID)
                {
                    EmailSource = null;
                    EmailSource = selectedSource;
                }
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
