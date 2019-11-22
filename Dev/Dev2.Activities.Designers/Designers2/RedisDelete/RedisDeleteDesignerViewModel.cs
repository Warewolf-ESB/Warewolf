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
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Validation;
using Dev2.Providers.Errors;
using Warewolf.Resource.Errors;

namespace Dev2.Activities.Designers2.RedisDelete
{
    public class RedisDeleteDesignerViewModel : ActivityDesignerViewModel
    {
        readonly IServer _server;
        IShellViewModel _shellViewModel;

        [ExcludeFromCodeCoverage]
        public RedisDeleteDesignerViewModel(ModelItem modelItem)
            : this(modelItem, ServerRepository.Instance.ActiveServer, CustomContainer.Get<IShellViewModel>())
        {

        }

        public RedisDeleteDesignerViewModel(ModelItem modelItem, IServer server, IShellViewModel shellViewModel)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("environmentModel", server);
            _server = server;
            VerifyArgument.IsNotNull("shellViewModel", shellViewModel);
            _shellViewModel = shellViewModel;

            AddTitleBarLargeToggle();
            ShowLarge = true;
            RedisServers = new ObservableCollection<RedisSource>();
            LoadRedisServers();
            EditRedisServerCommand = new RelayCommand(o => EditRedisServerSource(), o => IsRedisServerSelected);
            NewRedisServerCommand = new RelayCommand(o => NewRedisServerSource());
            if (modelItem.Properties["Key"]?.ComputedValue != null)
            {
                Key = modelItem.Properties["Key"]?.ComputedValue.ToString();
            }            
        }

        public ObservableCollection<RedisSource> RedisServers { get; private set; }

        public RedisSource SelectedRedisServer
        {
            get => (RedisSource)GetValue(SelectedRedisServerProperty);
            set => SetValue(SelectedRedisServerProperty, value);
        }

        public static readonly DependencyProperty SelectedRedisServerProperty =
            DependencyProperty.Register("SelectedRedisServer", typeof(RedisSource), typeof(RedisDeleteDesignerViewModel), new PropertyMetadata(null, OnSelectedRedisServerChanged));

        private static void OnSelectedRedisServerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisDeleteDesignerViewModel)d;
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

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(RedisDeleteDesignerViewModel), new PropertyMetadata(null, OnKeyChanged));

        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisDeleteDesignerViewModel)d;
            viewModel.OnKeyChanged();
        }

        protected virtual void OnKeyChanged()
        {
            ModelItem.SetProperty("Key", Key);
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

        void SetSelectedRedisServer()
        {
            var sourceId = Guid.Parse(ModelItem.Properties["SourceId"].ComputedValue.ToString());
            var selectedRedisServer = RedisServers.FirstOrDefault(redisServer => redisServer.ResourceID == sourceId);
            SelectedRedisServer = selectedRedisServer;
        }

        public bool IsKeyFocused { get => (bool)GetValue(IsKeyFocusedProperty); set { SetValue(IsKeyFocusedProperty, value); } }

        public static readonly DependencyProperty IsKeyFocusedProperty =
            DependencyProperty.Register("IsKeyFocused", typeof(bool), typeof(RedisDeleteDesignerViewModel), new PropertyMetadata(false));

        [ExcludeFromCodeCoverage]
        public override void Validate()
        {
            Errors = null;
            var errors = new List<IActionableErrorInfo>();

            Action onError = () => IsKeyFocused = true;
            var util = new VariableUtils();
            util.AddError(errors, util.TryParseVariables(Key, out string keyValue, onError));

            if (string.IsNullOrWhiteSpace(keyValue))
            {
                errors.Add(new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = string.Format(ErrorResource.PropertyMusHaveAValue, "Key") });
            }

            // Always assign property otherwise binding does not update!
            Errors = errors;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
