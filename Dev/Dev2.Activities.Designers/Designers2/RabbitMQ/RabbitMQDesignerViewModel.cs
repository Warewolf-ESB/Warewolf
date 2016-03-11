
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Threading;
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

namespace Dev2.Activities.Designers2.RabbitMQ
{
    public class RabbitMQDesignerViewModel : ActivityDesignerViewModel, IHandle<UpdateResourceMessage>
    {
        static readonly RabbitMQSource NewRabbitMQSource = new RabbitMQSource { ResourceID = Guid.NewGuid(), ResourceName = "New RabbitMQ Source..." };
        static readonly RabbitMQSource SelectRabbitMQSource = new RabbitMQSource { ResourceID = Guid.NewGuid(), ResourceName = "Select an RabbitMQ Source..." };

        readonly IEventAggregator _eventPublisher;
        readonly IEnvironmentModel _environmentModel;
        readonly IAsyncWorker _asyncWorker;

        bool _isInitializing;
        public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;

        public RabbitMQDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public RabbitMQDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
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

            RabbitMQSources = new ObservableCollection<RabbitMQSource>();
            Priorities = new ObservableCollection<enMailPriorityEnum> { enMailPriorityEnum.High, enMailPriorityEnum.Normal, enMailPriorityEnum.Low };

            EditRabbitMQSourceCommand = new RelayCommand(o => EditRabbitMQSource(), o => IsRabbitMQSourceSelected);
            TestRabbitMQAccountCommand = new RelayCommand(o => TestRabbitMQAccount(), o => CanTestRabbitMQAccount);
            //ChooseAttachmentsCommand = new DelegateCommand(o => ChooseAttachments());

            RefreshSources(true);
        }

        public RabbitMQSource SelectedRabbitMQSource
        {
            get
            {
                return (RabbitMQSource)GetValue(SelectedRabbitMQSourceProperty);
            }
            set
            {
                SetValue(SelectedRabbitMQSourceProperty, value);
                EditRabbitMQSourceCommand.RaiseCanExecuteChanged();
            }
        }
        public static readonly DependencyProperty SelectedRabbitMQSourceProperty = DependencyProperty.Register("SelectedRabbitMQSource", typeof(RabbitMQSource), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(null, OnSelectedRabbitMQSourceChanged));

        static void OnSelectedRabbitMQSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RabbitMQDesignerViewModel)d;

            viewModel.OnSelectedRabbitMQSourceChanged();
        }

        public RelayCommand EditRabbitMQSourceCommand { get; private set; }
        public RelayCommand TestRabbitMQAccountCommand { get; private set; }
        public ICommand ChooseAttachmentsCommand { get; private set; }

        public bool IsRabbitMQSourceSelected
        {
            get
            {
                return SelectedRabbitMQSource != SelectRabbitMQSource;
            }
        }

        public bool CanEditSource { get; set; }

        public ObservableCollection<RabbitMQSource> RabbitMQSources { get; private set; }
        public ObservableCollection<enMailPriorityEnum> Priorities { get; private set; }

        public bool IsRefreshing { get { return (bool)GetValue(IsRefreshingProperty); } set { SetValue(IsRefreshingProperty, value); } }
        public static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.Register("IsRefreshing", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool CanTestRabbitMQAccount
        {
            get
            {
                return (bool)GetValue(CanTestRabbitMQAccountProperty);
            }
            set
            {
                SetValue(CanTestRabbitMQAccountProperty, value);
                TestRabbitMQAccountCommand.RaiseCanExecuteChanged();
            }
        }
        public static readonly DependencyProperty CanTestRabbitMQAccountProperty = DependencyProperty.Register("CanTestRabbitMQAccount", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(true));

        public bool IsRabbitMQSourceFocused { get { return (bool)GetValue(IsRabbitMQSourceFocusedProperty); } set { SetValue(IsRabbitMQSourceFocusedProperty, value); } }
        public static readonly DependencyProperty IsRabbitMQSourceFocusedProperty = DependencyProperty.Register("IsRabbitMQSourceFocused", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsFromAccountFocused { get { return (bool)GetValue(IsFromAccountFocusedProperty); } set { SetValue(IsFromAccountFocusedProperty, value); } }
        public static readonly DependencyProperty IsFromAccountFocusedProperty = DependencyProperty.Register("IsFromAccountFocused", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsPasswordFocused { get { return (bool)GetValue(IsPasswordFocusedProperty); } set { SetValue(IsPasswordFocusedProperty, value); } }
        public static readonly DependencyProperty IsPasswordFocusedProperty = DependencyProperty.Register("IsPasswordFocused", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsToFocused { get { return (bool)GetValue(IsToFocusedProperty); } set { SetValue(IsToFocusedProperty, value); } }
        public static readonly DependencyProperty IsToFocusedProperty = DependencyProperty.Register("IsToFocused", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsCcFocused { get { return (bool)GetValue(IsCcFocusedProperty); } set { SetValue(IsCcFocusedProperty, value); } }
        public static readonly DependencyProperty IsCcFocusedProperty = DependencyProperty.Register("IsCcFocused", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsBccFocused { get { return (bool)GetValue(IsBccFocusedProperty); } set { SetValue(IsBccFocusedProperty, value); } }
        public static readonly DependencyProperty IsBccFocusedProperty = DependencyProperty.Register("IsBccFocused", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsSubjectFocused { get { return (bool)GetValue(IsSubjectFocusedProperty); } set { SetValue(IsSubjectFocusedProperty, value); } }
        public static readonly DependencyProperty IsSubjectFocusedProperty = DependencyProperty.Register("IsSubjectFocused", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsAttachmentsFocused { get { return (bool)GetValue(IsAttachmentsFocusedProperty); } set { SetValue(IsAttachmentsFocusedProperty, value); } }
        public static readonly DependencyProperty IsAttachmentsFocusedProperty = DependencyProperty.Register("IsAttachmentsFocused", typeof(bool), typeof(RabbitMQDesignerViewModel), new PropertyMetadata(default(bool)));

        // DO NOT bind to these properties - these are here for convenience only!!!
        public string Password { get { return GetProperty<string>(); } set { SetProperty(value); } }
        //string FromAccount { get { return GetProperty<string>(); } }
        //string To { get { return GetProperty<string>(); } }
        //string Cc { get { return GetProperty<string>(); } }
        //string Bcc { get { return GetProperty<string>(); } }
        //string Attachments { get { return GetProperty<string>(); } set { SetProperty(value); } }
        //string Subject { get { return GetProperty<string>(); } }
        string Body { get { return GetProperty<string>(); } }
        RabbitMQSource RabbitMQSource
        {
            // ReSharper disable ExplicitCallerInfoArgument
            get { return GetProperty<RabbitMQSource>("SelectedRabbitMQSource"); }
            // ReSharper restore ExplicitCallerInfoArgument
            set
            {
                if (!_isInitializing)
                {
                    // ReSharper disable ExplicitCallerInfoArgument
                    SetProperty(value, "SelectedRabbitMQSource");
                    // ReSharper restore ExplicitCallerInfoArgument
                }
            }
        }

        public void CreateRabbitMQSource()
        {
            _eventPublisher.Publish(new ShowNewResourceWizard("RabbitMQSource"));
            RefreshSources();
        }

        public void EditRabbitMQSource()
        {
            CustomContainer.Get<IShellViewModel>().OpenResource(SelectedRabbitMQSource.ResourceID, CustomContainer.Get<IShellViewModel>().ActiveServer);
        }

        //string GetTestRabbitMQAccount()
        //{
        //    var addresses = new List<string>();
        //    addresses.AddRange(To.Split(';'));
        //    addresses.AddRange(Cc.Split(';'));
        //    addresses.AddRange(Bcc.Split(';'));
        //    return addresses.FirstOrDefault();
        //}

        void TestRabbitMQAccount()
        {
            //var testRabbitMQAccount = GetTestRabbitMQAccount();
            //if(string.IsNullOrEmpty(testRabbitMQAccount))
            //{
            //    Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(() => IsToFocused = true) { Message = "Please supply a To address in order to Test." } };
            //    return;
            //}
            //CanTestRabbitMQAccount = false;

            ////var testSource = new RabbitMQSource(SelectedRabbitMQSource.ToXml());
            //if(!string.IsNullOrEmpty(FromAccount))
            //{
            //    testSource.UserName = FromAccount;
            //    testSource.Password = Password;
            //}
            //testSource.TestFromAddress = testSource.UserName;
            //testSource.TestToAddress = testRabbitMQAccount;
            //if (RabbitMQAddresssIsAVariable(testRabbitMQAccount))
            //{
            //    return;
            //}
            //Uri uri = new Uri(new Uri(AppSettings.LocalHost), "wwwroot/sources/Service/RabbitMQSources/Test");
            //var jsonData = testSource.ToString();

            //var requestInvoker = CreateWebRequestInvoker();
            //requestInvoker.ExecuteRequest("POST", uri.ToString(), jsonData, null, OnTestCompleted);
        }

        //bool RabbitMQAddresssIsAVariable(string testRabbitMQAccount)
        //{
        //    var postResult = "";
        //    var hasVariable = false;
        //    if(DataListUtil.IsFullyEvaluated(testRabbitMQAccount))
        //    {
        //        postResult += "Variable " + testRabbitMQAccount + " cannot be used while testing.";
        //        hasVariable = true;
        //    }
        //    if(DataListUtil.IsFullyEvaluated(FromAccount))
        //    {
        //        var errorMessage = "Variable " + FromAccount + " cannot be used while testing.";
        //        if(string.IsNullOrEmpty(postResult))
        //        {
        //            postResult += errorMessage;
        //        }
        //        else
        //        {
        //            postResult += Environment.NewLine + errorMessage;
        //        }
        //        hasVariable = true;
        //    }
        //    if(hasVariable)
        //    {
        //        var validationResult = new ValidationResult
        //        {
        //            ErrorMessage = postResult,
        //            IsValid = false
        //        };
        //        OnTestCompleted(new Dev2JsonSerializer().Serialize(validationResult));
        //        return true;
        //    }
        //    return false;
        //}

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
                CanTestRabbitMQAccount = true;
            }
        }

        protected virtual IWebRequestInvoker CreateWebRequestInvoker()
        {
            return new WebRequestInvoker();
        }

        protected virtual void OnSelectedRabbitMQSourceChanged()
        {
            if (SelectedRabbitMQSource == NewRabbitMQSource)
            {
                CreateRabbitMQSource();
                return;
            }

            IsRefreshing = true;

            if (SelectedRabbitMQSource != SelectRabbitMQSource)
            {
                RabbitMQSources.Remove(SelectRabbitMQSource);
            }
            RabbitMQSource = SelectedRabbitMQSource;
            EditRabbitMQSourceCommand.RaiseCanExecuteChanged();
        }

        void RefreshSources(bool isInitializing = false)
        {
            IsRefreshing = true;
            var selectedRabbitMQSource = RabbitMQSource;
            if (isInitializing)
            {
                _isInitializing = true;
            }
            LoadSources(() =>
            {
                SetSelectedRabbitMQSource(selectedRabbitMQSource);
                IsRefreshing = false;
                if (isInitializing)
                {
                    _isInitializing = false;
                }
            });
        }

        void SetSelectedRabbitMQSource(Resource source)
        {
            var selectedSource = source == null ? null : RabbitMQSources.FirstOrDefault(d => d.ResourceID == source.ResourceID);
            if (selectedSource == null)
            {
                if (RabbitMQSources.FirstOrDefault(d => d.Equals(SelectRabbitMQSource)) == null)
                {
                    RabbitMQSources.Insert(0, SelectRabbitMQSource);
                }
                selectedSource = SelectRabbitMQSource;
            }
            SelectedRabbitMQSource = selectedSource;
        }

        void LoadSources(System.Action continueWith = null)
        {
            RabbitMQSources.Clear();
            RabbitMQSources.Add(NewRabbitMQSource);

            _asyncWorker.Start(() => GetRabbitMQSources().OrderBy(r => r.ResourceName), sources =>
            {
                foreach (var source in sources)
                {
                    RabbitMQSources.Add(source);
                }
                if (continueWith != null)
                {
                    continueWith();
                }
            });
        }

        IEnumerable<RabbitMQSource> GetRabbitMQSources()
        {
            return _environmentModel.ResourceRepository.FindSourcesByType<RabbitMQSource>(_environmentModel, enSourceType.RabbitMQSource);
        }

        //void ChooseAttachments()
        //{

        //    const string Separator = ";";
        //    var message = new FileChooserMessage();
        //    message.SelectedFiles = Attachments.Split(Separator.ToCharArray());
        //    message.PropertyChanged += (sender, args) =>
        //    {
        //        if (args.PropertyName == "SelectedFiles")
        //        {
        //            if (message.SelectedFiles != null)
        //            {
        //                if(string.IsNullOrEmpty(Attachments))
        //                {
        //                    Attachments = string.Join(Separator, message.SelectedFiles);
        //                }
        //                else
        //                {
        //                    Attachments +=Separator+string.Join(Separator, message.SelectedFiles);
        //                }
        //            }
        //        }
        //    };
        //    _eventPublisher.Publish(message);
        //}

        public override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());
            Errors = result.Count == 0 ? null : result;
        }

        IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            foreach(var error in GetRuleSet("RabbitMQSource", GetDatalistString()).ValidateRules("'RabbitMQ Source'", () => IsRabbitMQSourceFocused = true))
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
                case "RabbitMQSource":
                    ruleSet.Add(new IsNullRule(() => RabbitMQSource));
                    break;
            //    case "FromAccount":
            //        var fromExprRule = new IsValidExpressionRule(() => FromAccount, datalist, "user@test.com");
            //        ruleSet.Add(fromExprRule);
            //        ruleSet.Add(new IsValidRabbitMQAddressRule(() => fromExprRule.ExpressionValue));
            //        break;
            //    case "Password":
            //        ruleSet.Add(new IsRequiredWhenOtherIsNotEmptyRule(() => Password, () => FromAccount));
            //        break;
            //    case "To":
            //        var toExprRule = new IsValidExpressionRule(() => To, datalist, "user@test.com");
            //        ruleSet.Add(toExprRule);
            //        ruleSet.Add(new IsValidRabbitMQAddressRule(() => toExprRule.ExpressionValue));
            //        break;
            //    case "Cc":
            //        var ccExprRule = new IsValidExpressionRule(() => Cc, datalist, "user@test.com");
            //        ruleSet.Add(ccExprRule);
            //        ruleSet.Add(new IsValidRabbitMQAddressRule(() => ccExprRule.ExpressionValue));
            //        break;
            //    case "Bcc":
            //        var bccExprRule = new IsValidExpressionRule(() => Bcc, datalist, "user@test.com");
            //        ruleSet.Add(bccExprRule);
            //        ruleSet.Add(new IsValidRabbitMQAddressRule(() => bccExprRule.ExpressionValue));
            //        break;
            //    case "Attachments":
            //        var attachmentsExprRule = new IsValidExpressionRule(() => Attachments, datalist, @"c:\test.txt");
            //        ruleSet.Add(attachmentsExprRule);
            //        ruleSet.Add(new IsValidFileNameRule(() => attachmentsExprRule.ExpressionValue));
            //        break;
            //    case "Recipients":
            //        ruleSet.Add(new HasAtLeastOneRule(() => To, () => Cc, () => Bcc));
            //        break;
            //    case "SubjectAndBody":
            //        ruleSet.Add(new HasAtLeastOneRule(() => Subject, () => Body));
            //        break;
            }
            return ruleSet;
        }

        public void Handle(UpdateResourceMessage message)
        {
            var selectedSource = new RabbitMQSource(message.ResourceModel.WorkflowXaml.ToXElement());
            if (RabbitMQSource == null)
            {
                RabbitMQSource = selectedSource;
            }
            else
            {
                if (selectedSource.ResourceID == RabbitMQSource.ResourceID)
                {
                    RabbitMQSource = null;
                    RabbitMQSource = selectedSource;
                }
            }
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
