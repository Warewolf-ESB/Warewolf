using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Dev2.ViewModels
{
    public class StudioTestViewModel : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab
    {
        readonly IPopupController _popupController;

        public StudioTestViewModel(IEventAggregator eventPublisher, ITestViewModel vm, IPopupController popupController,IView view)
            : base(eventPublisher)
        {
            ViewModel = vm;
            View = view;
            _popupController = popupController;
            ViewModel.PropertyChanged += (sender, args) =>
            {               
                var mainViewModel = CustomContainer.Get<IMainViewModel>();
                if (mainViewModel != null)
                {
                    ViewModelUtils.RaiseCanExecuteChanged(mainViewModel.SaveCommand);
                }
            };
        }

        protected override void OnDispose()
        {
            _eventPublisher.Unsubscribe(this);
            base.OnDispose();
            if (ViewModel != null) ViewModel.Dispose();
        }

        public override object GetView(object context = null)
        {
            return View;
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, ViewModel);
        }

        public override string DisplayName => ViewModel.DisplayName;

        protected override void OnViewLoaded(object view)
        {
            var loadedView = view as IView;
            if (loadedView != null)
            {
                loadedView.DataContext = ViewModel;
                base.OnViewLoaded(loadedView);
            }
        }



        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public string ResourceType
        {
            get
            {               
                return "Unknown";
            }
        }


        #region Implementation of IHelpSource

        public string HelpText { get; set; }
        public ITestViewModel ViewModel { get; set; }
        public IView View { get; set; }

        #endregion

        #region Implementation of IStudioTab

        public bool IsDirty => ViewModel.CanSave;

        public bool DoDeactivate(bool showMessage)
        {
            if (showMessage)
            {
                ViewModel.UpdateHelpDescriptor(string.Empty);
                if (ViewModel.HasChanged)
                {
                    var result = _popupController.Show(string.Format(StringResources.ItemSource_NotSaved),
                        $"Save {ViewModel.DisplayName.Replace("*", "")}?",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Information, "", false, false, true, false);

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
                ViewModel.UpdateHelpDescriptor(String.Empty);
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