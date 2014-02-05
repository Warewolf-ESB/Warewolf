using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Data.Enums;
using Dev2.DynamicServices;
using Dev2.Providers.Errors;
using Dev2.Providers.Logs;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Validation;

namespace Dev2.Activities.Designers2.Email
{
    public class EmailDesignerViewModel : ActivityDesignerViewModel
    {
        readonly ObservableCollection<EmailSource> _emailSourceList = new ObservableCollection<EmailSource>();
        readonly ObservableCollection<enMailPriorityEnum> _prioritiesList = new ObservableCollection<enMailPriorityEnum>();
        readonly EmailSource _baseEmailSource = new EmailSource();
        readonly IEventAggregator _eventPublisher;

        ICommand _editEmailSourceCommand;
        ICommand _testPasswordCommand;
        ICommand _chooseAttachmentsCommand;

        public EmailDesignerViewModel(ModelItem modelItem)
            : this(modelItem, EventPublishers.Aggregator)
        {
        }

        public EmailDesignerViewModel(ModelItem modelItem, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            _eventPublisher = eventPublisher;
            _baseEmailSource.ResourceName = "New Email Source...";
            EmailSourceList.Add(_baseEmailSource);
            UpdateEnvironmentResources();

            if(SelectedEmailSource != null)
            {
                TheSelectedEmailSource = SelectedEmailSource;
            }

            _prioritiesList.Add(enMailPriorityEnum.High);
            _prioritiesList.Add(enMailPriorityEnum.Normal);
            _prioritiesList.Add(enMailPriorityEnum.Low);
        }

        public EmailSource TheSelectedEmailSource { get { return (EmailSource)GetValue(TheSelectedEmailSourceProperty); } set { SetValue(TheSelectedEmailSourceProperty, value); } }
        public static readonly DependencyProperty TheSelectedEmailSourceProperty = DependencyProperty.Register("TheSelectedEmailSource", typeof(EmailSource), typeof(EmailDesignerViewModel), new PropertyMetadata(null, OnSelectedEmailSourceChanged));

        static void OnSelectedEmailSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (EmailDesignerViewModel)d;
            var value = e.NewValue as EmailSource;

            if(value != null)
            {
                if(value == viewModel._baseEmailSource)
                {
                    viewModel.CreateNewEmailSource();
                }
                else
                {
                    viewModel.SelectedEmailSource = value;
                    if(string.IsNullOrEmpty(viewModel.FromAccount))
                    {
                        viewModel.FromAccount = value.UserName;
                    }
                }
            }
        }

        public ICommand EditEmailSourceCommand { get { return _editEmailSourceCommand ?? (_editEmailSourceCommand = new RelayCommand(param => EditEmailSource())); } }
        public ICommand TestPasswordCommand { get { return _testPasswordCommand ?? (_testPasswordCommand = new RelayCommand(param => TestPassword())); } }
        public ICommand ChooseAttachmentsCommand { get { return _chooseAttachmentsCommand ?? (_chooseAttachmentsCommand = new RelayCommand(param => ChooseAttachments())); } }

        public bool CanEditSource { get; set; }

        public ObservableCollection<EmailSource> EmailSourceList { get { return _emailSourceList; } }
        public ObservableCollection<enMailPriorityEnum> PrioritiesList { get { return _prioritiesList; } }

        public bool IsEmailSourceFocused { get { return (bool)GetValue(IsEmailSourceFocusedProperty); } set { SetValue(IsEmailSourceFocusedProperty, value); } }
        public static readonly DependencyProperty IsEmailSourceFocusedProperty = DependencyProperty.Register("IsEmailSourceFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsFromAccountFocused { get { return (bool)GetValue(IsFromAccountFocusedProperty); } set { SetValue(IsFromAccountFocusedProperty, value); } }
        public static readonly DependencyProperty IsFromAccountFocusedProperty = DependencyProperty.Register("IsFromAccountFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));

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
        EmailSource SelectedEmailSource { get { return GetProperty<EmailSource>(); } set { SetProperty(value); } }
        string FromAccount { get { return GetProperty<string>(); } set { SetProperty(value); } }
        string To { get { return GetProperty<string>(); } }
        string Cc { get { return GetProperty<string>(); } }
        string Bcc { get { return GetProperty<string>(); } }
        string Subject {  get { return GetProperty<string>(); } }
        string Attachments { get { return GetProperty<string>(); } }

        public void UpdateEnvironmentResourcesCallback(IEnvironmentModel environmentModel)
        {
            if(environmentModel != null)
            {
                List<EmailSource> tmpSourceList = EmailSourceList.ToList();
                var sourceList = GetSources(environmentModel);
                EmailSourceList.Clear();
                _baseEmailSource.ResourceName = "New Email Source...";
                EmailSourceList.Add(_baseEmailSource);

                foreach(var emailSrc in sourceList)
                {
                    EmailSourceList.Add(emailSrc);
                }

                // TODO : Refactor to avoid the use of Foreach with break
                if(EmailSourceList.Count > tmpSourceList.Count)
                {
                    foreach(EmailSource source in EmailSourceList)
                    {
                        if(tmpSourceList.FirstOrDefault(c => c.ResourceName == source.ResourceName) == null)
                        {
                            TheSelectedEmailSource = source;
                            break;
                        }
                    }
                }
                else
                {
                    TheSelectedEmailSource = null;
                }
            }
        }

        public virtual List<EmailSource> GetSources(IEnvironmentModel environmentModel)
        {
            return environmentModel.ResourceRepository.FindSourcesByType<EmailSource>(environmentModel, enSourceType.EmailSource);
        }

        public void CreateNewEmailSource()
        {
            _eventPublisher.Publish(new ShowNewResourceWizard("EmailSource"));
            UpdateEnvironmentResources();
        }

        public void EditEmailSource()
        {
            Action<IEnvironmentModel> callback = EditEmailSource;
            _eventPublisher.Publish(new GetActiveEnvironmentCallbackMessage(callback));
        }

        void EditEmailSource(IEnvironmentModel env)
        {
            if(SelectedEmailSource != null)
            {
                IResourceModel resourceModel = env.ResourceRepository.FindSingle(c => c.ResourceName == SelectedEmailSource.ResourceName);
                if(resourceModel != null)
                {
                    _eventPublisher.Publish(new ShowEditResourceWizardMessage(resourceModel));
                }
            }
        }

        void UpdateEnvironmentResources()
        {
            Action<IEnvironmentModel> callback = UpdateEnvironmentResourcesCallback;
            this.TraceInfo("Publish message of type - " + typeof(GetActiveEnvironmentCallbackMessage));
            _eventPublisher.Publish(new GetActiveEnvironmentCallbackMessage(callback));
        }

        void TestPassword()
        {
        }

        void ChooseAttachments()
        {
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
            foreach(var error in GetRuleSet("Subject").ValidateRules("'Subject'", () => IsSubjectFocused = true))
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
                    ruleSet.Add(new IsNullRule(() => SelectedEmailSource));
                    break;
                case "FromAccount":
                    var fromExprRule = new IsValidExpressionRule(() => FromAccount);
                    ruleSet.Add(fromExprRule);
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => fromExprRule.ExpressionValue));
                    ruleSet.Add(new IsValidEmailRule(() => fromExprRule.ExpressionValue));
                    break;
                case "To":
                    var toExprRule = new IsValidExpressionRule(() => To);
                    ruleSet.Add(toExprRule);
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => toExprRule.ExpressionValue));
                    ruleSet.Add(new IsValidEmailRule(() => toExprRule.ExpressionValue));
                    break;
                case "Cc":
                    var ccExprRule = new IsValidExpressionRule(() => Cc);
                    ruleSet.Add(ccExprRule);
                    ruleSet.Add(new IsValidEmailRule(() => ccExprRule.ExpressionValue));
                    break;
                case "Bcc":
                    var bccExprRule = new IsValidExpressionRule(() => Bcc);
                    ruleSet.Add(bccExprRule);
                    ruleSet.Add(new IsValidEmailRule(() => bccExprRule.ExpressionValue));
                    break;
                case "Subject":
                    var subjectExprRule = new IsValidExpressionRule(() => Subject);
                    ruleSet.Add(subjectExprRule);
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => subjectExprRule.ExpressionValue));
                    break;
                case "Attachments":
                    var attachmentsExprRule = new IsValidExpressionRule(() => Attachments);
                    ruleSet.Add(attachmentsExprRule);
                    ruleSet.Add(new IsValidFileNameRule(() => attachmentsExprRule.ExpressionValue));
                    break;
            }
            return ruleSet;
        }
    }
}