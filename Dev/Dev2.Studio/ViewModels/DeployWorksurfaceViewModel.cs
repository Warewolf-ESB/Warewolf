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

using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Dev2.ViewModels
{
    public class DeployWorksurfaceViewModel : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab
    {
        public DeployWorksurfaceViewModel():base(new EventAggregator())
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            var dest = new DeployDestinationViewModel(mainViewModel, CustomContainer.Get<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>());
            var stats = new DeployStatsViewerViewModel(dest);
            var source = new DeploySourceExplorerViewModel(mainViewModel, CustomContainer.Get<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>(), stats);
            dest.StatsArea = stats;
            var vm = new SingleExplorerDeployViewModel(dest, source, new List<IExplorerTreeItem>(), stats, mainViewModel, CustomContainer.Get<IPopupController>());
            ViewModel = vm;
            View = new DeployView();
            ViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Header")
                {
                    OnPropertyChanged("DisplayName");
                }
                ViewModelUtils.RaiseCanExecuteChanged(mainViewModel?.SaveCommand);
            };
        }

        public override object GetView(object context = null) => View;

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, ViewModel);
        }

        public override string DisplayName => "Deploy";

        protected override void OnViewLoaded(object view)
        {
            if (view is IView loadedView)
            {
                loadedView.DataContext = ViewModel;
                base.OnViewLoaded(loadedView);
            }
        }

        public string ResourceType => "DeployViewer";

        public string HelpText { get; set; }
        public SingleExplorerDeployViewModel ViewModel { get; set; }
        public IView View { get; set; }

        public bool IsDirty => false;

        public void CloseView() { }

        public bool DoDeactivate(bool showMessage) => true;

        public override bool HasVariables => false;
        public override bool HasDebugOutput => false;
    }
}