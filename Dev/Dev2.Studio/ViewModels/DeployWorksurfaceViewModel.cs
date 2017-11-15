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
        readonly IPopupController _popupController;

        public DeployWorksurfaceViewModel() : base(new EventAggregator())
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

        public DeployWorksurfaceViewModel(IEventAggregator eventPublisher, SingleExplorerDeployViewModel vm, IPopupController popupController,IView view)
            : base(eventPublisher)
        {
            ViewModel = vm;
            View = view;
            _popupController = popupController;
            ViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "Header")
                {
                    OnPropertyChanged("DisplayName");
                }
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                ViewModelUtils.RaiseCanExecuteChanged(mainViewModel?.SaveCommand);
            };
        }

        public override object GetView(object context = null)
        {
            return View;
        }

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

        #region Implementation of IHelpSource

        public string HelpText { get; set; }
        public SingleExplorerDeployViewModel ViewModel { get; set; }
        public IView View { get; set; }

        #endregion

        #region Implementation of IStudioTab

        public bool IsDirty => false;

        public void CloseView()
        {
        }

        public bool DoDeactivate(bool showMessage)
        {

            return true;
        }

        #endregion

        public override bool HasVariables => false;
        public override bool HasDebugOutput => false;
    }
}