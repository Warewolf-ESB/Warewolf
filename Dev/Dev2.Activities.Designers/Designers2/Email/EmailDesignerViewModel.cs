using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
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
        readonly EmailSource _baseEmailSource = new EmailSource();
        readonly IEventAggregator _eventPublisher;

        ICommand _editEmailSourceCommand;
        ObservableCollection<EmailSource> _emailSourceList;

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
                SelectedSelectedEmailSource = SelectedEmailSource;
            }
        }

        public EmailSource SelectedSelectedEmailSource { get { return (EmailSource)GetValue(SelectedSelectedEmailSourceProperty); } set { SetValue(SelectedSelectedEmailSourceProperty, value); } }
        public static readonly DependencyProperty SelectedSelectedEmailSourceProperty = DependencyProperty.Register("SelectedSelectedEmailSource", typeof(EmailSource), typeof(EmailDesignerViewModel), new PropertyMetadata(null, OnSelectedEmailSourceChanged));

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

        public bool CanEditSource { get; set; }

        public ObservableCollection<EmailSource> EmailSourceList { get { return _emailSourceList ?? (_emailSourceList = new ObservableCollection<EmailSource>()); } }

        public bool IsFromAccountFocused { get { return (bool)GetValue(IsFromAccountFocusedProperty); } set { SetValue(IsFromAccountFocusedProperty, value); } }
        public static readonly DependencyProperty IsFromAccountFocusedProperty = DependencyProperty.Register("IsFromAccountFocused", typeof(bool), typeof(EmailDesignerViewModel), new PropertyMetadata(default(bool)));


        // DO NOT bind to these properties - these are here for convenience only!!!
        EmailSource SelectedEmailSource { set { SetProperty(value); } get { return GetProperty<EmailSource>(); } }
        string FromAccount { set { SetProperty(value); } get { return GetProperty<string>(); } }

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
                            SelectedSelectedEmailSource = source;
                            break;
                        }
                    }
                }
                else
                {
                    SelectedSelectedEmailSource = null;
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

        void UpdateEnvironmentResources()
        {
            Action<IEnvironmentModel> callback = UpdateEnvironmentResourcesCallback;
            this.TraceInfo("Publish message of type - " + typeof(GetActiveEnvironmentCallbackMessage));
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

        public override void Validate()
        {
            //var result = new List<IActionableErrorInfo>();
            //result.AddRange(ValidateThis());
            //Errors = result.Count == 0 ? null : result;
        }

        IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var error in GetRuleSet("FromAccount").ValidateRules("'From Account'", () => IsFromAccountFocused = true))
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                yield return error;
            }
        }

        IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();

            switch(propertyName)
            {
                case "FromAccount":
                    var sourceExprRule = new IsValidExpressionRule(() => FromAccount);
                    ruleSet.Add(sourceExprRule);
                    ruleSet.Add(new IsStringNullOrWhiteSpaceRule(() => sourceExprRule.ExpressionValue));
                    break;
            }
            return ruleSet;
        }
    }
}