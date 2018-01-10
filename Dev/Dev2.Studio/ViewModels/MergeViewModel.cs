using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.Practices.Prism.Mvvm;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Dev2.ViewModels
{
    public class MergeViewModel : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab
    {
        readonly IPopupController _popupController;

        public MergeViewModel(IEventAggregator eventPublisher, IMergeWorkflowViewModel vm, IPopupController popupController, IView view)
            : base(eventPublisher)
        {
            ViewModel = vm;
            View = view;
            _popupController = popupController;
            ViewModel.PropertyChanged += (sender, args) =>
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                if (mainViewModel != null)
                {
                    ViewModelUtils.RaiseCanExecuteChanged(mainViewModel.SaveCommand);
                }

                if (args.PropertyName == "DisplayName")
                {
                    NotifyOfPropertyChange(() => DisplayName);
                }

                if (args.PropertyName == "DataListViewModel")
                {
                    NotifyOfPropertyChange(() => DataListViewModel);
                }
            };
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => false;

        protected override void OnDispose()
        {
            _eventPublisher.Unsubscribe(this);
            base.OnDispose();
            ViewModel?.Dispose();
        }

        public override object GetView(object context = null)
        {
            return View;
        }
        [ExcludeFromCodeCoverage]
        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, ViewModel);
        }

        public override string DisplayName => ViewModel.DisplayName;
        [ExcludeFromCodeCoverage]
        protected override void OnViewLoaded(object view)
        {
            if (view is IView loadedView)
            {
                loadedView.DataContext = ViewModel;
                base.OnViewLoaded(loadedView);
            }
        }
        public IDataListViewModel DataListViewModel => ViewModel.DataListViewModel;

        public string ResourceType => "MergeConflicts";

        #region Implementation of IHelpSource

        public string HelpText { get; set; }
        public IMergeWorkflowViewModel ViewModel { get; set; }
        public IView View { get; set; }

        #endregion

        #region Implementation of IStudioTab

        public bool IsDirty => ViewModel.CanSave;

        [ExcludeFromCodeCoverage]
        public void CloseView()
        {
        }

        public bool DoDeactivate(bool showMessage)
        {
            if (showMessage)
            {
                ViewModel.UpdateHelpDescriptor(string.Empty);
                if (ViewModel.IsDirty)
                {
                    var result = _popupController.Show(string.Format(StringResources.ItemSource_NotSaved),
                        $"Save {ViewModel.DisplayName.Replace("*", "")}?",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Information, "", false, false, true, false, false, false);

                    switch (result)
                    {
                        case MessageBoxResult.Cancel:
                        case MessageBoxResult.None:
                            return false;
                        case MessageBoxResult.No:
                            return true;
                        case MessageBoxResult.Yes:
                            if (ViewModel.CanSave)
                            {
                                ViewModel.Save();
                            }
                            break;
                    }
                }
            }
            else
            {
                ViewModel.UpdateHelpDescriptor(string.Empty);
                if (ViewModel.CanSave)
                {
                    ViewModel.Save();
                }
            }
            return true;
        }

        #endregion

    }
}
