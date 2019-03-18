#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Diagnostics;
using Dev2.Providers.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.ViewModels
{
    public class StudioTestViewModel : BaseWorkSurfaceViewModel, IStudioTestWorkSurfaceViewModel, IHelpSource, IStudioTab, IHandle<DebugOutputMessage>
    {
        readonly IPopupController _popupController;
        DebugOutputViewModel _debugOutputViewModel;

        public StudioTestViewModel(IEventAggregator eventPublisher, IServiceTestViewModel vm, IPopupController popupController, IView view)
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
            };
            DebugOutputViewModel = new DebugOutputViewModel(new EventPublisher(), CustomContainer.Get<IServerRepository>(), new DebugOutputFilterStrategy(), ViewModel.WorkflowDesignerViewModel.ResourceModel) { IsTestView = true };
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => true;

        protected override void OnDispose()
        {
            _eventPublisher.Unsubscribe(this);
            base.OnDispose();
            ViewModel?.Dispose();
        }

        public override object GetView(object context = null) => View;

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
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
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
                            TrySave();
                            break;
                        case MessageBoxResult.OK:
                            break;
                        default:
                            return true;
                    }
                    if (result == MessageBoxResult.Yes && ViewModel.HasDuplicates())
                    {
                        return false;//dont close the tab
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

        private void TrySave()
        {
            if (ViewModel.HasDuplicates())
            {
                ViewModel.ShowDuplicatePopup();
            }
            if (ViewModel.CanSave)
            {
                ViewModel.Save();
            }
        }

        #endregion
    }
}