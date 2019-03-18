#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
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
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;





namespace Dev2.Activities.Designers2.RabbitMQ.Consume
{
    public class RabbitMQConsumeDesignerViewModel : ActivityDesignerViewModel, INotifyPropertyChanged
    {
        readonly IRabbitMQSourceModel _model;
        readonly IShellViewModel _shellViewModel;
        public RabbitMQConsumeDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);

            _shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = _shellViewModel.ActiveServer;
            _model = CustomContainer.CreateInstance<IRabbitMQSourceModel>(server.UpdateRepository, server.QueryProxy, _shellViewModel);
            SetupCommonViewModelProperties();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Utility_Rabbit_MQ_Consume;
        }

        public RabbitMQConsumeDesignerViewModel(ModelItem modelItem, IRabbitMQSourceModel model)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);
            VerifyArgument.IsNotNull("model", model);

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
        }

        public ObservableCollection<IRabbitMQServiceSourceDefinition> RabbitMQSources { get; private set; }

        public RelayCommand NewRabbitMQSourceCommand { get; private set; }

        public RelayCommand EditRabbitMQSourceCommand { get; private set; }

        public bool IsRabbitMQSourceFocused { get => (bool)GetValue(IsRabbitMQSourceFocusedProperty); set => SetValue(IsRabbitMQSourceFocusedProperty, value); }
        public static readonly DependencyProperty IsRabbitMQSourceFocusedProperty = DependencyProperty.Register("IsRabbitMQSourceFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsQueueNameFocused { get => (bool)GetValue(IsQueueNameFocusedProperty); set => SetValue(IsQueueNameFocusedProperty, value); }
        public static readonly DependencyProperty IsQueueNameFocusedProperty = DependencyProperty.Register("IsQueueNameFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsPrefetchFocused { get => (bool)GetValue(IsPrefetchFocusedProperty); set => SetValue(IsPrefetchFocusedProperty, value); }
        public static readonly DependencyProperty IsPrefetchFocusedProperty = DependencyProperty.Register("IsPrefetchFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsResponseFocused { get => (bool)GetValue(IsResponseFocusedProperty); set => SetValue(IsResponseFocusedProperty, value); }
        public static readonly DependencyProperty IsResponseFocusedProperty = DependencyProperty.Register("IsResponseFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsTimeOutFocused { get => (bool)GetValue(IsTimeOutFocusedProperty); set => SetValue(IsTimeOutFocusedProperty, value); }
        public static readonly DependencyProperty IsTimeOutFocusedProperty = DependencyProperty.Register("IsTimeOutFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

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

        public string Response
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Prefetch
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool Acknowledge
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
        public string TimeOut
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool ReQueue
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsObject
        {
            get
            {
                var isObject = ModelItem.GetProperty<bool>("IsObject");
                return isObject;
            }
            set
            {
                ModelItem.SetProperty("IsObject", value);
                OnPropertyChanged();
                OnPropertyChanged("ObjectName");
            }
        }

        public string ObjectName
        {
            get
            {
                var objectName = ModelItem.GetProperty<string>("ObjectName");
                return objectName;
            }
            set
            {
                ModelItem.SetProperty("ObjectName", value);
                OnPropertyChanged();
            }
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
            foreach (var error in GetRuleSet("Prefetch").ValidateRules("'Prefetch'", () => IsPrefetchFocused = true))
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

                case "Prefetch":
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => Prefetch));
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