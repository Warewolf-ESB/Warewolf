/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;

namespace Dev2.Activities.Designers2.RabbitMQ.Publish
{
    public class RabbitMQPublishDesignerViewModel : ActivityDesignerViewModel, IHandle<UpdateResourceMessage>
    {
        private static readonly RabbitMQSource NewRabbitMQSource = new RabbitMQSource { ResourceID = Guid.NewGuid(), ResourceName = "New RabbitMQ Source..." };
        private static readonly RabbitMQSource SelectRabbitMQSource = new RabbitMQSource { ResourceID = Guid.NewGuid(), ResourceName = "Select an RabbitMQ Source..." };

        private readonly IEventAggregator _eventPublisher;
        private readonly IEnvironmentModel _environmentModel;
        private readonly IAsyncWorker _asyncWorker;

        private bool _isInitializing;
        public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;

        public RabbitMQPublishDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public RabbitMQPublishDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
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

            EditRabbitMQSourceCommand = new RelayCommand(o => EditRabbitMQSource(), o => IsRabbitMQSourceSelected);
            TestRabbitMQPublishCommand = new RelayCommand(o => TestRabbitMQPublish(QueueName, IsDurable, IsExclusive, IsAutoDelete, Message), o => CanTestRabbitMQPublish);

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

        public static readonly DependencyProperty SelectedRabbitMQSourceProperty = DependencyProperty.Register("SelectedRabbitMQSource", typeof(RabbitMQSource), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(null, OnSelectedRabbitMQSourceChanged));

        private static void OnSelectedRabbitMQSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RabbitMQPublishDesignerViewModel)d;

            viewModel.OnSelectedRabbitMQSourceChanged();
        }

        public RelayCommand EditRabbitMQSourceCommand { get; private set; }
        public RelayCommand TestRabbitMQPublishCommand { get; private set; }

        public bool IsRabbitMQSourceSelected
        {
            get
            {
                return SelectedRabbitMQSource != SelectRabbitMQSource;
            }
        }

        public bool CanEditSource { get; set; }

        public ObservableCollection<RabbitMQSource> RabbitMQSources { get; private set; }

        public bool CanTestRabbitMQPublish
        {
            get
            {
                return (bool)GetValue(CanTestRabbitMQPublishProperty);
            }
            set
            {
                SetValue(CanTestRabbitMQPublishProperty, value);
                TestRabbitMQPublishCommand.RaiseCanExecuteChanged();
            }
        }
        public static readonly DependencyProperty CanTestRabbitMQPublishProperty = DependencyProperty.Register("CanTestRabbitMQPublish", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(true));

        public bool IsRefreshing { get { return (bool)GetValue(IsRefreshingProperty); } set { SetValue(IsRefreshingProperty, value); } }
        public static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.Register("IsRefreshing", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsRabbitMQSourceFocused { get { return (bool)GetValue(IsRabbitMQSourceFocusedProperty); } set { SetValue(IsRabbitMQSourceFocusedProperty, value); } }
        public static readonly DependencyProperty IsRabbitMQSourceFocusedProperty = DependencyProperty.Register("IsRabbitMQSourceFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsQueueNameFocused { get { return (bool)GetValue(IsQueueNameFocusedProperty); } set { SetValue(IsQueueNameFocusedProperty, value); } }
        public static readonly DependencyProperty IsQueueNameFocusedProperty = DependencyProperty.Register("IsQueueNameFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsMessageFocused { get { return (bool)GetValue(IsMessageFocusedProperty); } set { SetValue(IsMessageFocusedProperty, value); } }
        public static readonly DependencyProperty IsMessageFocusedProperty = DependencyProperty.Register("IsMessageFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(default(bool)));

        public string QueueName
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged("QueueName");
            }
        }

        public bool IsDurable
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged("IsDurable");
            }
        }

        public bool IsExclusive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged("IsExclusive");
            }
        }

        public bool IsAutoDelete
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged("IsAutoDelete");
            }
        }

        public string Message
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged("Message");
            }
        }

        public string PublishResult
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged("PublishResult");
            }
        }

        private RabbitMQSource RabbitMQSource
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

        public event PropertyChangedEventHandler PropertyChanged;
        [ExcludeFromCodeCoverage]
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
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

        private void TestRabbitMQPublish(string queueName, bool isDurable, bool isExclusive, bool isAutoDelete, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    message = "Test message";
                SelectRabbitMQSource.Publish(queueName, isDurable, isExclusive, isAutoDelete, message);
                PublishResult = "Success";
            }
            catch
            {
                PublishResult = "Fail";    
            }
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

        private void RefreshSources(bool isInitializing = false)
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

        private void SetSelectedRabbitMQSource(Resource source)
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

        private void LoadSources(System.Action continueWith = null)
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

        private IEnumerable<RabbitMQSource> GetRabbitMQSources()
        {
            return new[]
            {
                new RabbitMQSource()
                {
                    Host = "localhost", 
                    UserName = "guest", 
                    Password = "guest"
                }
            };
            //return _environmentModel.ResourceRepository.FindSourcesByType<RabbitMQSource>(_environmentModel, enSourceType.RabbitMQSource);
        }

        public override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());
            Errors = result.Count == 0 ? null : result;
        }

        private IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            foreach (var error in GetRuleSet("RabbitMQSource", GetDatalistString()).ValidateRules("'RabbitMQ Source'", () => IsRabbitMQSourceFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("QueueName", GetDatalistString()).ValidateRules("'Queue Name'", () => IsQueueNameFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Message", GetDatalistString()).ValidateRules("'Message'", () => IsMessageFocused = true))
            {
                yield return error;
            }
        }

        private IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();

            switch (propertyName)
            {
                case "RabbitMQSource":
                    ruleSet.Add(new IsNullRule(() => RabbitMQSource));
                    break;
                case "QueueName":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => QueueName));
                    break;
                case "Message":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => Message));
                    break;
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