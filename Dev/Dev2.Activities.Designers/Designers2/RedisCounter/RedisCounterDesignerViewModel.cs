/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using System.Collections.ObjectModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Data.ServiceModel;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System;

namespace Dev2.Activities.Designers2.RedisCounter
{
    public class RedisCounterDesignerViewModel : ActivityDesignerViewModel
    {
        private readonly IServer _server;
        private readonly IShellViewModel _shellViewModel;

        [ExcludeFromCodeCoverage]
        public RedisCounterDesignerViewModel(ModelItem modelItem)
            : this(modelItem, ServerRepository.Instance.ActiveServer, CustomContainer.Get<IShellViewModel>())
        {

        }

        public RedisCounterDesignerViewModel(ModelItem modelItem, IServer server, IShellViewModel shellViewModel)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("environmentModel", server);
            _server = server;
            VerifyArgument.IsNotNull("shellViewModel", shellViewModel);
            _shellViewModel = shellViewModel;

            AddTitleBarLargeToggle();

            RedisServers = new ObservableCollection<RedisSource>();
            LoadRedisServers();
            EditRedisServerCommand = new RelayCommand(o => EditRedisServerSource(), o => IsRedisServerSelected);
            NewRedisServerCommand = new RelayCommand(o => NewRedisServerSource());

            if (modelItem.Properties["Key"]?.ComputedValue != null)
            {
                Key = modelItem.Properties["Key"]?.ComputedValue.ToString();
            }
            if (modelItem.Properties["StepSize"]?.ComputedValue != null)
            {
                StepSize = modelItem.Properties["StepSize"]?.ComputedValue.ToString();
            }
            if (modelItem.Properties["CounterType"]?.ComputedValue != null)
            {
                CounterType = modelItem.Properties["CounterType"]?.ComputedValue.ToString();
            }

            if (modelItem.Properties["Increment"]?.ComputedValue != null)
            {
                Increment = (bool)modelItem.Properties["Increment"]?.ComputedValue;
            }
            if (modelItem.Properties["Decrement"]?.ComputedValue != null)
            {
                Decrement = (bool)modelItem.Properties["Decrement"]?.ComputedValue;
            }

        }

        public ObservableCollection<RedisSource> RedisServers { get; private set; }

        public RedisSource SelectedRedisServer
        {
            get => (RedisSource)GetValue(SelectedRedisServerProperty);
            set => SetValue(SelectedRedisServerProperty, value);
        }

        public static readonly DependencyProperty SelectedRedisServerProperty =
            DependencyProperty.Register("SelectedRedisServer", typeof(RedisSource), typeof(RedisCounterDesignerViewModel), new PropertyMetadata(null, OnSelectedRedisServerChanged));

        private static void OnSelectedRedisServerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisCounterDesignerViewModel)d;
            viewModel.OnSelectedRedisServerChanged();
            viewModel.EditRedisServerCommand?.RaiseCanExecuteChanged();
        }

        protected virtual void OnSelectedRedisServerChanged()
        {
            ModelItem.SetProperty("SourceId", SelectedRedisServer.ResourceID);
        }

        public RelayCommand EditRedisServerCommand { get; private set; }
        public RelayCommand NewRedisServerCommand { get; set; }

        public bool IsRedisServerSelected => SelectedRedisServer != null;
        
        public bool? Increment
        {
            get
            {
                return (bool)GetValue(IncrementProperty);
            }
            set
            {
                SetValue(IncrementProperty, value);
                SetValue(CounterTypeProperty, nameof(Increment));
            }
        }
        public static DependencyProperty IncrementProperty =
           DependencyProperty.Register("Increment", typeof(bool), typeof(RedisCounterDesignerViewModel), new PropertyMetadata(true, OnIncrementChanged));
        private static void OnIncrementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisCounterDesignerViewModel)d;
            viewModel.OnIncrementChanged();
        }
        protected virtual void OnIncrementChanged()
        {
            ModelItem.SetProperty("Increment", Increment);
            SetValue(CounterTypeProperty, "Decrement");
        }

        public bool? Decrement
        {
            get
            {
                return (bool)GetValue(DecrementProperty);
            }
            set
            {
                SetValue(DecrementProperty, value);               
            }
        }
        public static DependencyProperty DecrementProperty =
          DependencyProperty.Register("Decrement", typeof(bool), typeof(RedisCounterDesignerViewModel), new PropertyMetadata(false, OnDecrementChanged));
        private static void OnDecrementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisCounterDesignerViewModel)d;
            viewModel.OnDecrementChanged();
        }
        protected virtual void OnDecrementChanged()
        {
            ModelItem.SetProperty("Decrement", Decrement);
            SetValue(CounterTypeProperty, "Increment");
        }

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public string CounterType
        {
            get => (string)GetValue(CounterTypeProperty);
            set => SetValue(CounterTypeProperty, value);
        }
        public static readonly DependencyProperty CounterTypeProperty =
          DependencyProperty.Register("CounterType", typeof(string), typeof(RedisCounterDesignerViewModel), new PropertyMetadata("Increment", OnCounterTypeChanged));

        private static void OnCounterTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisCounterDesignerViewModel)d;
            viewModel.OnCounterTypeChanged();
        }
        protected virtual void OnCounterTypeChanged()
        {
            ModelItem.SetProperty("CounterType", CounterType);
        }
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(RedisCounterDesignerViewModel), new PropertyMetadata(null, OnKeyChanged));

        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisCounterDesignerViewModel)d;
            viewModel.OnKeyChanged();
        }

        protected virtual void OnKeyChanged()
        {
            ModelItem.SetProperty("Key", Key);
        }
        public bool Reset
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
        public string StepSize
        {
            get => (string)GetValue(StepSizeProperty);
            set => SetValue(StepSizeProperty, value);
        }
        public static readonly DependencyProperty StepSizeProperty =
          DependencyProperty.Register("StepSize", typeof(string), typeof(RedisCounterDesignerViewModel), new PropertyMetadata(null, OnStepSizeChanged));

        private static void OnStepSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisCounterDesignerViewModel)d;
            viewModel.OnStepSizeChanged();
        }
        protected virtual void OnStepSizeChanged()
        {
            ModelItem.SetProperty("StepSize", StepSize);
        }
        private void EditRedisServerSource()
        {
            _shellViewModel.OpenResource(SelectedRedisServer.ResourceID, _server.EnvironmentID, _shellViewModel.ActiveServer);
            LoadRedisServers();
        }

        private void NewRedisServerSource()
        {
            _shellViewModel.NewRedisSource("");
            LoadRedisServers();
        }

        private void LoadRedisServers()
        {
            var redisServers = _server.ResourceRepository.FindSourcesByType<RedisSource>(_server, enSourceType.RedisSource);
            RedisServers = redisServers.ToObservableCollection();
            SetSelectedRedisServer();
        }

        private void SetSelectedRedisServer()
        {
            var sourceId = Guid.Parse(ModelItem.Properties["SourceId"].ComputedValue.ToString());
            var selectedRedisServer = RedisServers.FirstOrDefault(redisServer => redisServer.ResourceID == sourceId);
            SelectedRedisServer = selectedRedisServer;
        }

        [ExcludeFromCodeCoverage]
        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
