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

// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.ViewModels
{
    public class SourceViewModel<T> : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab
        where T : IEquatable<T>
    {
        readonly IPopupController _popupController;
        private readonly IEnvironmentModel _environmentModel;

        public SourceViewModel(IEventAggregator eventPublisher, SourceBaseImpl<T> vm, IPopupController popupController,IView view,IEnvironmentModel environmentModel)
            : base(eventPublisher)
        {
            ViewModel = vm;
            View = view;
            _popupController = popupController;
            _environmentModel = environmentModel;
            ViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "Header")
                {
                    OnPropertyChanged("DisplayName");
                }
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
            ViewModel?.Dispose();
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => false;

        public override object GetView(object context = null)
        {
            return View;
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, ViewModel);
        }

        public override string DisplayName
        {
            get
            {
                var header = ViewModel.Header;
                if (!_environmentModel.IsLocalHost)
                {
                    var name = _environmentModel.Name;
                    if (header.EndsWith(" *"))
                    {
                        return header.Replace(" *", "") + " - " + name + " *";
                    }
                    return header + " - " + name;
                }
                return header;
            }
        }

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
                if(ViewModel?.Image != null)
                {
                    return ViewModel.Image;
                }
                return "Unknown";
            }
        }


        #region Implementation of IHelpSource

        public string HelpText { get; set; }
        public SourceBaseImpl<T> ViewModel { get; set; }
        public IView View { get; set; }

        #endregion

        #region Implementation of IStudioTab

        public bool IsDirty => ViewModel.HasChanged && ViewModel.CanSave();

        public void CloseView()
        {
        }

        public bool DoDeactivate(bool showMessage)
        {
            if (showMessage)
            {
                ViewModel.UpdateHelpDescriptor(string.Empty);
                if (ViewModel.HasChanged)
                {
                    var result = _popupController.Show(string.Format(StringResources.ItemSource_NotSaved),
                        $"Save {ViewModel.Header.Replace("*", "")}?",
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
                            if (ViewModel.CanSave())
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
                if (ViewModel.CanSave())
                {
                    ViewModel.Save();
                }
            }
            return true;
        }

        #endregion
    }
}