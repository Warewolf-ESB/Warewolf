using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Providers.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.WorkSurface;
using Infragistics.Windows.DockManager;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.ViewModels
{
    public class StudioTestViewModel : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab, IHandle<DebugOutputMessage>
    {
        readonly IPopupController _popupController;
        private DebugOutputViewModel _debugOutputViewModel;

        public StudioTestViewModel(IEventAggregator eventPublisher, IServiceTestViewModel vm, IPopupController popupController, IView view)
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

                if (args.PropertyName == "DisplayName")
                {
                    NotifyOfPropertyChange(() => DisplayName);
                }
            };
            DebugOutputViewModel = new DebugOutputViewModel(new EventPublisher(), EnvironmentRepository.Instance, new DebugOutputFilterStrategy(), ViewModel.WorkflowDesignerViewModel.ResourceModel);
            DebugOutputViewModel.IsTestView = true;
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => true;

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
        public string ResourceType => "ServiceTestsViewer";

        #region Implementation of IHelpSource

        public string HelpText { get; set; }
        public IServiceTestViewModel ViewModel { get; set; }
        public IView View { get; set; }
        public DebugOutputViewModel DebugOutputViewModel
        {
            get
            {
                return _debugOutputViewModel;
            }
            set
            {
                _debugOutputViewModel = value;
                NotifyOfPropertyChange(() => DebugOutputViewModel);
            }
        }

        public void Handle(DebugOutputMessage message)
        {
            Dev2Logger.Info(message.GetType().Name);
            DebugOutputViewModel.Clear();
            DebugOutputViewModel.DebugStatus = DebugStatus.Ready;
            foreach (var debugState in message.DebugStates)
            {
                if (debugState != null)
                {
                    debugState.SessionID = DebugOutputViewModel.SessionID;
                    DebugOutputViewModel.Append(debugState);
                }
            }
        }

        #endregion

        #region Implementation of IStudioTab

        public bool IsDirty => ViewModel.CanSave;

        public PaneToolWindow FindParentWindow(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent == null) return null;

            PaneToolWindow parentWindow = parent as PaneToolWindow;
            if (parentWindow != null)
            {
                return parentWindow;
            }
            return FindParentWindow(parent);
        }

        public void CloseView()
        {
            var parent = FindParentWindow((FrameworkElement)View);
            parent?.Close();
//
//            var dockManager = (XamDockManager)((FrameworkElement)((FrameworkElement)View)?.Parent)?.Parent;
//            var panes = dockManager?.Panes;
//            if (panes != null)
//            {
//                if (panes.Count == 1)
//                {
//                    var parent = FindParentWindow((FrameworkElement)View);
//                    parent?.Close();
//                }
//                else
//                {
//                    if (dockManager.ActivePane != null)
//                    {
//                        dockManager.ActivePane.CloseAction = PaneCloseAction.RemovePane;
//                        dockManager.ActivePane.ExecuteCommand(ContentPaneCommands.Close);
//                    }
//                }
//            }
            
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
                        MessageBoxImage.Information, "", false, false, true, false);

                    switch (result)
                    {
                        case MessageBoxResult.Cancel:
                        case MessageBoxResult.None:
                            return false;
                        case MessageBoxResult.No:
                            return true;
                        case MessageBoxResult.Yes:
                            if (ViewModel.HasDuplicates())
                            {
                                ViewModel.ShowDuplicatePopup();
                                return false;//dont close the tab
                            }
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