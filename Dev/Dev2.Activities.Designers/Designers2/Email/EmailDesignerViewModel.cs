using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.DynamicServices;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Threading;
using Dev2.Util;
using Dev2.Validation;

namespace Dev2.Activities.Designers2.Email
{
    public class EmailDesignerViewModel : ActivityDesignerViewModel
    {
        static readonly EmailSource NewEmailSource = new EmailSource { ResourceID = Guid.NewGuid(), ResourceName = "New Email Source..." };
        static readonly EmailSource SelectEmailSource = new EmailSource { ResourceID = Guid.NewGuid(), ResourceName = "Select an Email Source..." };

        readonly IEventAggregator _eventPublisher;
        readonly IEnvironmentModel _environmentModel;
        readonly IAsyncWorker _asyncWorker;

        bool _isInitializing;

        public EmailDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public EmailDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("environmentModel", environmentModel);
            _asyncWorker = asyncWorker;
            _environmentModel = environmentModel;
            _eventPublisher = eventPublisher;

            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            EmailSources = new ObservableCollection<EmailSource>();
            Priorities = new ObservableCollection<enMailPriorityEnum> { enMailPriorityEnum.High, enMailPriorityEnum.Normal, enMailPriorityEnum.Low };

            EditEmailSourceCommand = new RelayCommand(o => EditEmailSource(), o => IsEmailSourceSelected);
            TestEmailAccountCommand = new RelayCommand(o => TestEmailAccount(), o => CanTestEmailAccount);
            ChooseAttachmentsCommand = new RelayCommand(o => ChooseAttachments(), o => true);

            RefreshSources(true);
        }

        public EmailSource SelectedEmailSource { get { return (EmailSource)GetValue(SelectedEmailSourceProperty); } set { SetValue(SelectedEmailSourceProperty, value); } }
        public static readonly DependencyProperty SelectedEmailSourceProperty = DependencyProperty.Register("SelectedEmailSource", typeof(EmailSource), typeof(EmailDesignerViewModel), new PropertyMetadata(null, OnSelectedEmailSourceChanged));

        static void OnSelectedEmailSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (EmailDesignerViewModel)d;
            if(viewModel.IsRefreshing)
            {
                return;
            }
            viewModel.OnSelectedEmailSourceChanged();
        }

        public ICommand EditEmailSourceCommand { get; private set; }
        public ICommand TestEmailAccountCommand { get; private set; }
        public ICommand ChooseAttachmentsCommand { get; private set; }

        public bool IsEmailSourceSelected { get { return SelectedEmailSource != SelectEmailSource; } }

        public bool CanEditSource { get; set; }

        public ObservableCollection<EmailSource> EmailSources { get; private set; }
        public ObservableCollection<enMailPriorityEnum> Priorities { get; private set; }

        public bool IsRefreshing { get { return (bool)GetValue(IsRefreshingProperty); } set { SetValue(IsRefreshingProperty, value); } }
        public static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.Register("IsRefreshing", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool CanTestEmailAccount { get { return (bool)GetValue(CanTestEmailAccountProperty); } set { SetValue(CanTestEmailAccountProperty, value); } }
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
        string FromAccount { get { return GetProperty<string>(); } }
        string To { get { return GetProperty<string>(); } }
        string Cc { get { return GetProperty<string>(); } }
        string Bcc { get { return GetProperty<string>(); } }
        string Attachments { get { return GetProperty<string>(); } set { SetProperty(value); } }
        string Subject { get { return GetProperty<string>(); } }
        string Body { get { return GetProperty<string>(); } }
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
            _eventPublisher.Publish(new ShowNewResourceWizard("EmailSource"));
            RefreshSources();
        }

        public void EditEmailSource()
        {
            var resourceModel = _environmentModel.ResourceRepository.FindSingle(c => c.ResourceName == SelectedEmailSource.ResourceName);
            if(resourceModel != null)
            {
                _eventPublisher.Publish(new ShowEditResourceWizardMessage(resourceModel));
                RefreshSources();
            }
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

            Uri uri = new Uri(new Uri(AppSettings.LocalHost), "wwwroot/sources/Service/EmailSources/Test");
            var jsonData = testSource.ToString();

            var requestInvoker = CreateWebRequestInvoker();
            requestInvoker.ExecuteRequest("POST", uri.ToString(), jsonData, null, OnTestCompleted);
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

            EmailSources.Remove(SelectEmailSource);
            EmailSource = SelectedEmailSource;
        }

        void RefreshSources(bool isInitializing = false)
        {
            IsRefreshing = true;
            if(isInitializing)
            {
                _isInitializing = true;
            }
            LoadSources(() =>
            {
                SetSelectedEmailSource(EmailSource);
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
                if(continueWith != null)
                {
                    continueWith();
                }
            });
        }

        IEnumerable<EmailSource> GetEmailSources()
        {
            return _environmentModel.ResourceRepository.FindSourcesByType<EmailSource>(_environmentModel, enSourceType.EmailSource);
        }

        void ChooseAttachments()
        {
            const string Separator = ";";
            var message = new FileChooserMessage();
            message.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "SelectedFiles")
                {
                    if(message.SelectedFiles != null)
                    {
                        Attachments = string.Join(Separator, Attachments, string.Join(Separator, message.SelectedFiles));
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
            foreach(var error in GetRuleSet("EmailSource").ValidateRules("'Email Source'", () => IsEmailSourceFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("FromAccount").ValidateRules("'From Account'", () => IsFromAccountFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Password").ValidateRules("'Password'", () => IsPasswordFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Recipients").ValidateRules("'To', 'Cc' or 'Bcc'", () => IsToFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("To").ValidateRules("'To'", () => IsToFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Cc").ValidateRules("'Cc'", () => IsCcFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Bcc").ValidateRules("'Bcc'", () => IsBccFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("SubjectAndBody").ValidateRules("'Subject' or 'Body'", () => IsSubjectFocused = true))
            {
                yield return error;
            }
            foreach(var error in GetRuleSet("Attachments").ValidateRules("'Attachments'", () => IsAttachmentsFocused = true))
            {
                yield return error;
            }
        }

        IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();

            switch(propertyName)
            {
                case "EmailSource":
                    ruleSet.Add(new IsNullRule(() => EmailSource));
                    break;
                case "FromAccount":
                    var fromExprRule = new IsValidExpressionRule(() => FromAccount, "user@test.com");
                    ruleSet.Add(fromExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => fromExprRule.ExpressionValue));
                    break;
                case "Password":
                    ruleSet.Add(new IsRequiredWhenOtherIsNotEmptyRule(() => Password, () => FromAccount));
                    break;
                case "To":
                    var toExprRule = new IsValidExpressionRule(() => To, "user@test.com");
                    ruleSet.Add(toExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => toExprRule.ExpressionValue));
                    break;
                case "Cc":
                    var ccExprRule = new IsValidExpressionRule(() => Cc, "user@test.com");
                    ruleSet.Add(ccExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => ccExprRule.ExpressionValue));
                    break;
                case "Bcc":
                    var bccExprRule = new IsValidExpressionRule(() => Bcc, "user@test.com");
                    ruleSet.Add(bccExprRule);
                    ruleSet.Add(new IsValidEmailAddressRule(() => bccExprRule.ExpressionValue));
                    break;
                case "Attachments":
                    var attachmentsExprRule = new IsValidExpressionRule(() => Attachments, @"c:\test.txt");
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
    }
}