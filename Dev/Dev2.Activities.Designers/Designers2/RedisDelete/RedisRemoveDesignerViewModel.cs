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
using System.ComponentModel;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Activities.Designers2.RedisValidator;

namespace Dev2.Activities.Designers2.RedisRemove
{
    public class RedisRemoveDesignerViewModel : ActivityDesignerViewModel, INotifyPropertyChanged
    {
        readonly IServer _server;
        IShellViewModel _shellViewModel;

        [ExcludeFromCodeCoverage]
        public RedisRemoveDesignerViewModel(ModelItem modelItem)
            : this(modelItem, ServerRepository.Instance.ActiveServer, CustomContainer.Get<IShellViewModel>())
        {

        }

        public RedisRemoveDesignerViewModel(ModelItem modelItem, IServer server, IShellViewModel shellViewModel)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("environmentModel", server);
            _server = server;
            VerifyArgument.IsNotNull("shellViewModel", shellViewModel);
            _shellViewModel = shellViewModel;

            AddTitleBarLargeToggle();
            ShowLarge = true;
            RedisSources = new ObservableCollection<RedisSource>();
            LoadRedisSources();
            EditRedisSourceCommand = new RelayCommand(o => EditRedisSource(), o => IsRedisSourceSelected);
            NewRedisSourceCommand = new RelayCommand(o => NewRedisSource());
            if (modelItem.Properties["Key"]?.ComputedValue != null)
            {
                Key = modelItem.Properties["Key"]?.ComputedValue.ToString();
            }
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Database_RedisRemove;
        }

        public ObservableCollection<RedisSource> RedisSources { get; private set; }

        public RedisSource SelectedRedisSource
        {
            get => (RedisSource)GetValue(SelectedRedisSourceProperty);
            set => SetValue(SelectedRedisSourceProperty, value);
        }

        public static readonly DependencyProperty SelectedRedisSourceProperty =
            DependencyProperty.Register("SelectedRedisSource", typeof(RedisSource), typeof(RedisRemoveDesignerViewModel), new PropertyMetadata(null, OnSelectedRedisSourceChanged));

        private static void OnSelectedRedisSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisRemoveDesignerViewModel)d;
            viewModel.OnSelectedRedisSourceChanged();
            viewModel.EditRedisSourceCommand?.RaiseCanExecuteChanged();
        }

        protected virtual void OnSelectedRedisSourceChanged()
        {
            ModelItem.SetProperty("SourceId", SelectedRedisSource.ResourceID);

            OnPropertyChanged(nameof(IsRedisSourceSelected));
        }

        public RelayCommand EditRedisSourceCommand { get; private set; }
        public RelayCommand NewRedisSourceCommand { get; set; }

        public bool IsRedisSourceSelected => SelectedRedisSource != null;

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(RedisRemoveDesignerViewModel), new PropertyMetadata(null, OnKeyChanged));

        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisRemoveDesignerViewModel)d;
            viewModel.OnKeyChanged();
        }

        protected virtual void OnKeyChanged()
        {
            ModelItem.SetProperty("Key", Key);
        }

        public string Result
        {
            get => (string)GetValue(ResultProperty);
            set => SetValue(ResultProperty, value);
        }

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(string), typeof(RedisRemoveDesignerViewModel), new PropertyMetadata(null, OnResultChanged));

        private static void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisRemoveDesignerViewModel)d;
            viewModel.OnResultChanged();
        }

        protected virtual void OnResultChanged()
        {
            ModelItem.SetProperty("Result", Result);
        }

        private void EditRedisSource()
        {
            _shellViewModel.OpenResource(SelectedRedisSource.ResourceID, _server.EnvironmentID, _shellViewModel.ActiveServer);
            LoadRedisSources();
        }

        private void NewRedisSource()
        {
            _shellViewModel.NewRedisSource("");
            LoadRedisSources();
        }

        private void LoadRedisSources()
        {
            var redisSources = _server.ResourceRepository.FindSourcesByType<RedisSource>(_server, enSourceType.RedisSource);
            RedisSources = redisSources.ToObservableCollection();
            SetSelectedRedisSource();
        }

        void SetSelectedRedisSource()
        {
            var sourceId = Guid.Parse(ModelItem.Properties["SourceId"].ComputedValue.ToString());
            var selectedRedisSource = RedisSources.FirstOrDefault(redisSource => redisSource.ResourceID == sourceId);
            SelectedRedisSource = selectedRedisSource;
        }

        public bool IsKeyFocused { get => (bool)GetValue(IsKeyFocusedProperty); set { SetValue(IsKeyFocusedProperty, value); } }

        public static readonly DependencyProperty IsKeyFocusedProperty =
            DependencyProperty.Register("IsKeyFocused", typeof(bool), typeof(RedisRemoveDesignerViewModel), new PropertyMetadata(false));

        public bool IsRedisSourceFocused { get => (bool)GetValue(IsRedisSourceFocusedProperty); set { SetValue(IsRedisSourceFocusedProperty, value); } }

        public static readonly DependencyProperty IsRedisSourceFocusedProperty = 
            DependencyProperty.Register("IsRedisSourceFocused", typeof(bool), typeof(RedisRemoveDesignerViewModel), new PropertyMetadata(false));

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [ExcludeFromCodeCoverage]
        public override void Validate()
        {
            var result = new List<IActionableErrorInfo>();
            var redisDesignerDTO = new RedisDesignerDTO(SelectedRedisSource, Key);
            result.AddRange(RedisValidatorDesignerViewModel.Validate(redisDesignerDTO, _isRedisSourceFocused => IsRedisSourceFocused = _isRedisSourceFocused, _isKeyFocused => IsKeyFocused = _isKeyFocused));
            Errors = result.Count == 0 ? null : result;
        }
       
        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
