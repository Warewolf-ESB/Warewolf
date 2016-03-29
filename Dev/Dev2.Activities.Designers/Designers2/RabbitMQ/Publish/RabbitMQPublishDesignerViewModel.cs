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
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
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
    // ReSharper disable InconsistentNaming
    public class RabbitMQPublishDesignerViewModel : ActivityDesignerViewModel, INotifyPropertyChanged
    // ReSharper restore InconsistentNaming
    {
        private readonly IRabbitMQModel _model;
        private readonly IEnvironmentModel _environmentModel;
        private readonly IEventAggregator _eventPublisher;

        [ExcludeFromCodeCoverage]
        public RabbitMQPublishDesignerViewModel(ModelItem modelItem)
            : this(modelItem, CustomContainer.CreateInstance<IRabbitMQModel>(), EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public RabbitMQPublishDesignerViewModel(ModelItem modelItem, IRabbitMQModel model, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);
            VerifyArgument.IsNotNull("model", model);
            VerifyArgument.IsNotNull("environmentModel", environmentModel);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);

            _model = model;
            _environmentModel = environmentModel;
            _eventPublisher = eventPublisher;

            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;

            EditRabbitMQSourceCommand = new RelayCommand(o => EditRabbitMQSource(), o => IsRabbitMQSourceSelected);
            NewRabbitMQSourceCommand = new RelayCommand(o => NewRabbitMQSource());

            RabbitMQSources = LoadRabbitMQSources();
            SetSelectedRabbitMQSource(null);
            AddTitleBarLargeToggle();
        }

        // ReSharper disable InconsistentNaming
        public ObservableCollection<IRabbitMQSource> RabbitMQSources { get; private set; }

        public RelayCommand NewRabbitMQSourceCommand { get; private set; }

        public RelayCommand EditRabbitMQSourceCommand { get; private set; }

        public bool IsRabbitMQSourceFocused { get { return (bool)GetValue(IsRabbitMQSourceFocusedProperty); } set { SetValue(IsRabbitMQSourceFocusedProperty, value); } }
        public static readonly DependencyProperty IsRabbitMQSourceFocusedProperty = DependencyProperty.Register("IsRabbitMQSourceFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsQueueNameFocused { get { return (bool)GetValue(IsQueueNameFocusedProperty); } set { SetValue(IsQueueNameFocusedProperty, value); } }
        public static readonly DependencyProperty IsQueueNameFocusedProperty = DependencyProperty.Register("IsQueueNameFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsMessageFocused { get { return (bool)GetValue(IsMessageFocusedProperty); } set { SetValue(IsMessageFocusedProperty, value); } }
        public static readonly DependencyProperty IsMessageFocusedProperty = DependencyProperty.Register("IsMessageFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(default(bool)));

        private IRabbitMQSource _selectedRabbitQueue;

        public IRabbitMQSource SelectedRabbitMQSource
        {
            get
            {
                return _selectedRabbitQueue;
            }
            set
            {
                _selectedRabbitQueue = value;
                RabbitMQSourceResourceId = _selectedRabbitQueue == null ? Guid.Empty : _selectedRabbitQueue.ResourceID;
                OnPropertyChanged("IsRabbitMQSourceSelected");
            }
        }

        public bool IsRabbitMQSourceSelected
        {
            get
            {
                return SelectedRabbitMQSource != null;
            }
        }

        private Guid RabbitMQSourceResourceId
        {
            get
            {
                return GetProperty<Guid>();
            }
            set
            {
                SetProperty(value);
            }
        }

        // ReSharper restore InconsistentNaming

        public string QueueName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsDurable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsExclusive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsAutoDelete
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string Message
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Result
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        // ReSharper disable InconsistentNaming

        private ObservableCollection<IRabbitMQSource> LoadRabbitMQSources()
        {
            IRabbitMQSource rabbitMQSource = new RabbitMQSource()
                {
                    ResourceID = new Guid("00000000-0000-0000-0000-000000000001"),
                    ResourceType = ResourceType.RabbitMQSource,
                    ResourceName = "Test (localhost)",
                    Host = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };

            var rabbitMQSources = new[]
            {
                rabbitMQSource
            };

            // TODO: Remove the above stub and uncomment below when new RabbitMQSource has been implemented WOLF-1523
            //var rabbitMQSources = _environmentModel.ResourceRepository.FindSourcesByType<RabbitMQSource>(_environmentModel, enSourceType.RabbitMQSource);
            //var rabbitMQSources = _model.RetrieveMqSources();
            return rabbitMQSources.ToObservableCollection();
        }

        private void SetSelectedRabbitMQSource(IRabbitMQSource rabbitMQSource)
        {
            IRabbitMQSource selectRabbitMQSource = rabbitMQSource ?? RabbitMQSources.FirstOrDefault(d => d.ResourceID == RabbitMQSourceResourceId);
            SelectedRabbitMQSource = selectRabbitMQSource;
        }

        private void EditRabbitMQSource()
        {
            CustomContainer.Get<IShellViewModel>().OpenResource(SelectedRabbitMQSource.ResourceID, CustomContainer.Get<IShellViewModel>().ActiveServer);
        }

        private void NewRabbitMQSource()
        {
            //_eventPublisher.Publish(new ShowNewResourceWizard("RabbitMQSource"));
            _model.CreateNewSource();
            RabbitMQSources = LoadRabbitMQSources();
            IRabbitMQSource newRabbitMQSources = RabbitMQSources.FirstOrDefault(source => source.IsNewResource);
            SetSelectedRabbitMQSource(newRabbitMQSources);
        }

        // ReSharper restore InconsistentNaming

        public override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());
            Errors = result.Count == 0 ? null : result;
        }

        private IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            foreach (var error in GetRuleSet("RabbitMQSource").ValidateRules("'RabbitMQ Source'", () => IsRabbitMQSourceFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("QueueName").ValidateRules("'Queue Name'", () => IsQueueNameFocused = true))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet("Message").ValidateRules("'Message'", () => IsMessageFocused = true))
            {
                yield return error;
            }
        }

        private IRuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();

            switch (propertyName)
            {
                case "RabbitMQSource":
                    ruleSet.Add(new IsNullRule(() => SelectedRabbitMQSource));
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

        public event PropertyChangedEventHandler PropertyChanged;

        [ExcludeFromCodeCoverage]
        private void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [ExcludeFromCodeCoverage]
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