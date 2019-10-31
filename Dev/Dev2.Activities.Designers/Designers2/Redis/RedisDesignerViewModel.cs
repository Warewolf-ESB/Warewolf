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
using System.Activities;
using System.Windows.Media;
using Dev2.Studio.Core.Activities.Utils;
using System.Collections.ObjectModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Data.ServiceModel;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Common;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Activities.Designers2.Redis
{
    public class RedisDesignerViewModel : ActivityDesignerViewModel
    {
        readonly IServer _server;

        [ExcludeFromCodeCoverage]
        public RedisDesignerViewModel(ModelItem modelItem)
            : this(modelItem, ServerRepository.Instance.ActiveServer)
        {

        }

        public RedisDesignerViewModel(ModelItem modelItem, IServer server)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("environmentModel", server);
            _server = server;

            AddTitleBarLargeToggle();
            var dataFunc = modelItem.Properties["ActivityFunc"]?.ComputedValue as ActivityFunc<string, bool>;
            ActivityFuncDisplayName = dataFunc?.Handler == null ? "" : dataFunc?.Handler?.DisplayName;
            var type = dataFunc?.Handler?.GetType();
            if (type != null)
            {
                ActivityFuncIcon = ModelItemUtils.GetImageSourceForToolFromType(type);
            }
            RedisServers = new ObservableCollection<RedisSource>();
            LoadRedisServers();
            EditRedisServerCommand = new RelayCommand(o => EditRedisServerSource(), o => IsRedisServerSelected);
        }

        public ObservableCollection<RedisSource> RedisServers { get; private set; }

        public RedisSource SelectedRedisServer
        {
            get => (RedisSource)GetValue(SelectedRedisServerProperty);
            set => SetValue(SelectedRedisServerProperty, value);
        }

        public static readonly DependencyProperty SelectedRedisServerProperty =
            DependencyProperty.Register("SelectedRedisServer", typeof(RedisSource), typeof(RedisDesignerViewModel), new PropertyMetadata(null, OnSelectedRedisServerChanged));

        private static void OnSelectedRedisServerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RedisDesignerViewModel)d;
            viewModel.EditRedisServerCommand.RaiseCanExecuteChanged();
        }

        public RelayCommand EditRedisServerCommand { get; private set; }

        public bool IsRedisServerSelected => SelectedRedisServer != null;

        private void EditRedisServerSource()
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            if (shellViewModel != null)
            {
                shellViewModel.OpenResource(SelectedRedisServer.ResourceID, _server.EnvironmentID, shellViewModel.ActiveServer);
                LoadRedisServers();
            }
        }

        private void LoadRedisServers()
        {
            var redisServers = _server.ResourceRepository.FindSourcesByType<RedisSource>(_server, enSourceType.RedisSource);
            RedisServers = redisServers.ToObservableCollection();
        }

        public string ActivityFuncDisplayName
        {
            get => (string)GetValue(ActivityFuncDisplayNameProperty);
            set => SetValue(ActivityFuncDisplayNameProperty, value);
        }

        public static readonly DependencyProperty ActivityFuncDisplayNameProperty =
            DependencyProperty.Register("ActivityFuncDisplayName", typeof(string), typeof(RedisDesignerViewModel), new PropertyMetadata(null));

        public ImageSource ActivityFuncIcon
        {
            get => (ImageSource)GetValue(ActivityFuncIconProperty);
            set => SetValue(ActivityFuncIconProperty, value);
        }

        public static readonly DependencyProperty ActivityFuncIconProperty =
            DependencyProperty.Register("ActivityFuncIcon", typeof(ImageSource), typeof(RedisDesignerViewModel), new PropertyMetadata(null));

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
