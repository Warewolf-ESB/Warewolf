using Caliburn.Micro;
using Dev2.Messages;
using Dev2.Security;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Diagnostics;
using System;
using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.WorkSurface
// ReSharper restore CheckNamespace
{
    public interface IWorkSurfaceContextViewModel
    {
        WorkSurfaceKey WorkSurfaceKey { get; }
        IEnvironmentModel Environment { get; }
        DebugOutputViewModel DebugOutputViewModel { get; set; }
        bool DeleteRequested { get; set; }
        IDataListViewModel DataListViewModel { get; set; }
        IWorkSurfaceViewModel WorkSurfaceViewModel { get; set; }
        IContextualResourceModel ContextualResourceModel { get; }
        AuthorizeCommand EditCommand { get; }
        AuthorizeCommand SaveCommand { get; }
        AuthorizeCommand RunCommand { get; }
        AuthorizeCommand ViewInBrowserCommand { get; }
        AuthorizeCommand DebugCommand { get; }
        AuthorizeCommand QuickViewInBrowserCommand { get; }
        AuthorizeCommand QuickDebugCommand { get; }
        IEventAggregator EventPublisher { get; }
        ValidationController ValidationController { get; set; }
        /// <summary>
        /// Indicates if a close has been requested
        /// </summary>
        bool CloseRequested { get; }
        ViewModelDialogResults DialogResult { get; set; }
        object Parent { get; set; }
        string DisplayName { get; set; }
        bool IsNotifying { get; set; }

        void Handle(DebugResourceMessage message);

        void Handle(ExecuteResourceMessage message);

        void Handle(SaveResourceMessage message);

        void Handle(UpdateWorksurfaceDisplayName message);

        void Handle(UpdateWorksurfaceFlowNodeDisplayName message);

        void SetDebugStatus(DebugStatus debugStatus);

        void Debug(IContextualResourceModel resourceModel, bool isDebug);

        void StopExecution();

        void ViewInBrowser();

        void QuickViewInBrowser();

        void QuickDebug();

        void BindToModel();

        void ShowSaveDialog(IContextualResourceModel resourceModel, bool addToTabManager);

        void Save(bool isLocalSave = false, bool isStudioShutdown = false);

        bool IsEnvironmentConnected();

        void FindMissing();

        void Debug();

        void Dispose();

        /// <summary>
        /// Requests tha the view bound to this view model closes
        /// </summary>
        void RequestClose();

        /// <summary>
        /// Requests tha the view bound to this view model closes
        /// </summary>
        void RequestClose(ViewModelDialogResults dialogResult);

        void CanClose(Action<bool> callback);

        void TryClose();

        void TryClose(bool? dialogResult);

        event EventHandler<ActivationEventArgs> Activated;
        event EventHandler<DeactivationEventArgs> AttemptingDeactivation;
        event EventHandler<DeactivationEventArgs> Deactivated;

        object GetView(object context);

        event EventHandler<ViewAttachedEventArgs> ViewAttached;

        void Refresh();

        void RaisePropertyChangedEventImmediately(string propertyName);

        void OnDeserialized(StreamingContext c);

        bool ShouldSerializeIsNotifying();
    }
}