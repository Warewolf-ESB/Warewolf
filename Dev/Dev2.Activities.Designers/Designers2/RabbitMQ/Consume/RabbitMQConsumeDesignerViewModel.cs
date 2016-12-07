﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
// ReSharper disable ConvertPropertyToExpressionBody

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers2.RabbitMQ.Consume
{
    public class RabbitMQConsumeDesignerViewModel : ActivityDesignerViewModel, INotifyPropertyChanged
    {
        private readonly IRabbitMQSourceModel _model;

        public RabbitMQConsumeDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);

            IShellViewModel shellViewModel = CustomContainer.Get<IShellViewModel>();
            IServer server = shellViewModel.ActiveServer;
            _model = CustomContainer.CreateInstance<IRabbitMQSourceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel);
            SetupCommonViewModelProperties();
        }

        public RabbitMQConsumeDesignerViewModel(ModelItem modelItem, IRabbitMQSourceModel model)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("modelItem", modelItem);
            VerifyArgument.IsNotNull("model", model);

            _model = model;
            SetupCommonViewModelProperties();
        }

        private void SetupCommonViewModelProperties()
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

        public bool IsRabbitMQSourceFocused { get { return (bool)GetValue(IsRabbitMQSourceFocusedProperty); } set { SetValue(IsRabbitMQSourceFocusedProperty, value); } }
        public static readonly DependencyProperty IsRabbitMQSourceFocusedProperty = DependencyProperty.Register("IsRabbitMQSourceFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsQueueNameFocused { get { return (bool)GetValue(IsQueueNameFocusedProperty); } set { SetValue(IsQueueNameFocusedProperty, value); } }
        public static readonly DependencyProperty IsQueueNameFocusedProperty = DependencyProperty.Register("IsQueueNameFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsPrefetchFocused { get { return (bool)GetValue(IsPrefetchFocusedProperty); } set { SetValue(IsPrefetchFocusedProperty, value); } }
        public static readonly DependencyProperty IsPrefetchFocusedProperty = DependencyProperty.Register("IsPrefetchFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsResponseFocused { get { return (bool)GetValue(IsResponseFocusedProperty); } set { SetValue(IsResponseFocusedProperty, value); } }
        public static readonly DependencyProperty IsResponseFocusedProperty = DependencyProperty.Register("IsResponseFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        public bool IsTimeOutFocused { get { return (bool)GetValue(IsTimeOutFocusedProperty); } set { SetValue(IsTimeOutFocusedProperty, value); } }
        public static readonly DependencyProperty IsTimeOutFocusedProperty = DependencyProperty.Register("IsTimeOutFocused", typeof(bool), typeof(RabbitMQConsumeDesignerViewModel), new PropertyMetadata(default(bool)));

        private IRabbitMQServiceSourceDefinition _selectedRabbitMQSource;

        public IRabbitMQServiceSourceDefinition SelectedRabbitMQSource
        {
            get
            {
                return _selectedRabbitMQSource;
            }
            set
            {
                _selectedRabbitMQSource = value;
                RabbitMQSourceResourceId = _selectedRabbitMQSource == null ? Guid.Empty : _selectedRabbitMQSource.ResourceID;
                OnPropertyChanged("IsRabbitMQSourceSelected");
                EditRabbitMQSourceCommand.RaiseCanExecuteChanged();
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

        public string Result
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        private ObservableCollection<IRabbitMQServiceSourceDefinition> LoadRabbitMQSources()
        {
            ICollection<IRabbitMQServiceSourceDefinition> rabbitMQSources = _model.RetrieveSources();
            return rabbitMQSources.ToObservableCollection();
        }

        private void SetSelectedRabbitMQSource(IRabbitMQServiceSourceDefinition rabbitMQSource)
        {
            IRabbitMQServiceSourceDefinition selectRabbitMQSource = rabbitMQSource ?? RabbitMQSources.FirstOrDefault(d => d.ResourceID == RabbitMQSourceResourceId);
            SelectedRabbitMQSource = selectRabbitMQSource;
        }

        private void EditRabbitMQSource()
        {
            _model.EditSource(SelectedRabbitMQSource);
            RabbitMQSources = LoadRabbitMQSources();
            IRabbitMQServiceSourceDefinition editedRabbitMQSources = RabbitMQSources.FirstOrDefault(source => source.ResourceID == RabbitMQSourceResourceId);
            SetSelectedRabbitMQSource(editedRabbitMQSources);
        }

        private void NewRabbitMQSource()
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
            }
            return ruleSet;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
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