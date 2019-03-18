#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;



namespace Dev2.ViewModels
{
    public class SourceViewModel<T> : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab
        where T : IEquatable<T>
    {
        readonly IPopupController _popupController;
        readonly IServer _server;

        public SourceViewModel(IEventAggregator eventPublisher, SourceBaseImpl<T> vm, IPopupController popupController,IView view,IServer server)
            : base(eventPublisher)
        {
            ViewModel = vm;
            View = view;
            _popupController = popupController;
            _server = server;
            ViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "Header")
                {
                    OnPropertyChanged("DisplayName");
                }
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
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

        public override object GetView(object context = null) => View;

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, ViewModel);
        }

        public override string DisplayName
        {
            get
            {
                var header = ViewModel.Header;
                if (!_server.IsLocalHost)
                {
                    var name = _server.Name;
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
            if (view is IView loadedView)
            {
                loadedView.DataContext = ViewModel;
                base.OnViewLoaded(loadedView);
            }
        }



    
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
            ViewModel.UpdateHelpDescriptor(string.Empty);
            if (showMessage)
            {
                if (IsDirty)
                {
                    var result = IsDirtyPopup();
                    switch (result)
                    {
                        case MessageBoxResult.No:
                            return true;
                        case MessageBoxResult.Yes:
                            ViewModel.Save();
                            break;
                        case MessageBoxResult.None:
                            break;
                        case MessageBoxResult.OK:
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                        default:
                            return false;
                    }
                }
                else if (ViewModel.HasChanged)
                {
                    var result = HasChangedPopup();
                    switch (result)
                    {
                        case MessageBoxResult.No:
                            return true;
                        case MessageBoxResult.None:
                            break;
                        case MessageBoxResult.OK:
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                        case MessageBoxResult.Yes:
                            break;
                        default:
                            return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (ViewModel.CanSave())
                {
                    ViewModel.Save();
                }
            }
            return true;
        }

        MessageBoxResult IsDirtyPopup() => _popupController.Show(string.Format(StringResources.ItemSource_NotSaved),
                                    $"Save {ViewModel.Header.Replace("*", "")}?",
                                                      MessageBoxButton.YesNoCancel,
                                                      MessageBoxImage.Information, "", false, false, true, false, false, false);

        MessageBoxResult HasChangedPopup() => _popupController.Show(string.Format(StringResources.ItemSource_HasChanged_NotTested),
                                    $"Test {ViewModel.Header.Replace("*", "")}?",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Information, "", false, false, true, false, false, false);

        #endregion
    }
}