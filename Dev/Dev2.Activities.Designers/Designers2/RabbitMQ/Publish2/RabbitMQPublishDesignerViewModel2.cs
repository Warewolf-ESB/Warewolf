#pragma warning disable
 /*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Dev2.Studio.Interfaces;
using Warewolf.Data.Options;
using Warewolf.Options;
using Warewolf.UI;


namespace Dev2.Activities.Designers2.RabbitMQ.Publish2
{
    public class RabbitMQPublishDesignerViewModel2 : ActivityDesignerViewModel, INotifyPropertyChanged
    {
        readonly IRabbitMQSourceModel _model;
        private readonly ModelItem _modelItem;
        private OptionsWithNotifier _basicProperties;
        public RabbitMQPublishDesignerViewModel2(ModelItem modelItem)
            : base(modelItem)
        {  
            _modelItem = modelItem;
            VerifyArgument.IsNotNull("modelItem", modelItem);

            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            _model = CustomContainer.CreateInstance<IRabbitMQSourceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel);
            SetupCommonViewModelProperties();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Utility_Rabbit_MQ_Publish;
        }

        public RabbitMQPublishDesignerViewModel2(ModelItem modelItem, IRabbitMQSourceModel model)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);
            VerifyArgument.IsNotNull("model", model);
            _modelItem = modelItem;
            _model = model;
            SetupCommonViewModelProperties();
        }

        void SetupCommonViewModelProperties()
        {
            ShowLarge = false;

            EditRabbitMQSourceCommand = new RelayCommand(o => EditRabbitMQSource(), o => IsRabbitMQSourceSelected);
            NewRabbitMQSourceCommand = new RelayCommand(o => NewRabbitMQSource());

            RabbitMQSources = LoadRabbitMQSources();
            SetSelectedRabbitMQSource(null);
            AddTitleBarLargeToggle();
            LoadBasicProperties();
        }

        public ObservableCollection<IRabbitMQServiceSourceDefinition> RabbitMQSources { get; private set; }

        private void LoadBasicProperties()
        { 
            var basicProperties = _modelItem.Properties["BasicProperties"].ComputedValue as RabbitMqPublishOptions;
            if (basicProperties is null)
            {
                basicProperties = new RabbitMqPublishOptions();
                _modelItem.Properties["BasicProperties"].SetValue(basicProperties);
            }
            var result = new List<IOption>();
            var failureOptions = OptionConvertor.Convert(basicProperties);
            result.AddRange(failureOptions);
            BasicProperties = new OptionsWithNotifier { Options = result };
        }
        public OptionsWithNotifier BasicProperties
        {
            get => _basicProperties;
            set
            {
                _basicProperties = value;
                OnPropertyChanged(nameof(BasicProperties));
                _basicProperties.OptionChanged += UpdateBasicPropertiesModelItem;
            }
        }
        private void UpdateBasicPropertiesModelItem()
        {
            if (BasicProperties?.Options != null)
            {
                var basicProperties = _modelItem.Properties["BasicProperties"]?.ComputedValue as RabbitMqPublishOptions;
                _modelItem.Properties["BasicProperties"]?.SetValue(OptionConvertor.Convert(typeof(RabbitMqPublishOptions), BasicProperties.Options, basicProperties));
                OnPropertyChanged(nameof(BasicProperties));
            }
        }
        public RelayCommand NewRabbitMQSourceCommand { get; private set; }

        public RelayCommand EditRabbitMQSourceCommand { get; private set; }

        public bool IsRabbitMQSourceFocused { get => (bool)GetValue(IsRabbitMQSourceFocusedProperty); set => SetValue(IsRabbitMQSourceFocusedProperty, value); }
        public static readonly DependencyProperty IsRabbitMQSourceFocusedProperty = DependencyProperty.Register("IsRabbitMQSourceFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel2), new PropertyMetadata(default(bool)));

        public bool IsQueueNameFocused { get => (bool)GetValue(IsQueueNameFocusedProperty); set => SetValue(IsQueueNameFocusedProperty, value); }
        public static readonly DependencyProperty IsQueueNameFocusedProperty = DependencyProperty.Register("IsQueueNameFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel2), new PropertyMetadata(default(bool)));
      
       
        public bool IsMessageFocused { get => (bool)GetValue(IsMessageFocusedProperty); set => SetValue(IsMessageFocusedProperty, value); }
        public static readonly DependencyProperty IsMessageFocusedProperty = DependencyProperty.Register("IsMessageFocused", typeof(bool), typeof(RabbitMQPublishDesignerViewModel2), new PropertyMetadata(default(bool)));

        IRabbitMQServiceSourceDefinition _selectedRabbitMQSource;

        public IRabbitMQServiceSourceDefinition SelectedRabbitMQSource
        {
            get
            {
                return _selectedRabbitMQSource;
            }
            set
            {
                _selectedRabbitMQSource = value;
                RabbitMQSourceResourceId = _selectedRabbitMQSource?.ResourceID ?? Guid.Empty;
                OnPropertyChanged("IsRabbitMQSourceSelected");
                EditRabbitMQSourceCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsRabbitMQSourceSelected => SelectedRabbitMQSource != null;

        Guid RabbitMQSourceResourceId
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

        ObservableCollection<IRabbitMQServiceSourceDefinition> LoadRabbitMQSources()
        {
            var rabbitMQSources = _model.RetrieveSources();
            return rabbitMQSources.ToObservableCollection();
        }

        void SetSelectedRabbitMQSource(IRabbitMQServiceSourceDefinition rabbitMQSource)
        {
            var selectRabbitMQSource = rabbitMQSource ?? RabbitMQSources.FirstOrDefault(d => d.ResourceID == RabbitMQSourceResourceId);
            SelectedRabbitMQSource = selectRabbitMQSource;
        }

        void EditRabbitMQSource()
        {
            _model.EditSource(SelectedRabbitMQSource);
            RabbitMQSources = LoadRabbitMQSources();
            var editedRabbitMQSources = RabbitMQSources.FirstOrDefault(source => source.ResourceID == RabbitMQSourceResourceId);
            SetSelectedRabbitMQSource(editedRabbitMQSources);
        }

        void NewRabbitMQSource()
        {
            _model.CreateNewSource();
            RabbitMQSources = LoadRabbitMQSources();
        }

        public override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            result.AddRange(ValidateThis());
            Errors = result.Count == 0 ? null : result;
        }

        IEnumerable<IActionableErrorInfo> ValidateThis()
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

        IRuleSet GetRuleSet(string propertyName)
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
                default:
                    break;
            }
            return ruleSet;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}