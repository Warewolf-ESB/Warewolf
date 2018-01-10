/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.Serialization;
using Caliburn.Micro;
using Dev2.Messages;
using Dev2.Security;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.Diagnostics;


namespace Dev2.Studio.ViewModels.WorkSurface
{
    public interface IWorkSurfaceContextViewModel : IDisposable
    {
        WorkSurfaceKey WorkSurfaceKey { get; }
        IServer Environment { get; }
        DebugOutputViewModel DebugOutputViewModel { get; set; }
        bool DeleteRequested { get; set; }
        IDataListViewModel DataListViewModel { get; set; }
        IWorkSurfaceViewModel WorkSurfaceViewModel { get; set; }
        IContextualResourceModel ContextualResourceModel { get; }
        AuthorizeCommand SaveCommand { get; }
        AuthorizeCommand RunCommand { get; }
        AuthorizeCommand ViewInBrowserCommand { get; }
        AuthorizeCommand DebugCommand { get; }
        AuthorizeCommand QuickViewInBrowserCommand { get; }
        AuthorizeCommand QuickDebugCommand { get; }
        IEventAggregator EventPublisher { get; }
        ValidationController ValidationController { get; set; }
        bool CloseRequested { get; }
        ViewModelDialogResults DialogResult { get; set; }
        object Parent { get; set; }
        string DisplayName { get; set; }
        bool IsNotifying { get; set; }

        void Handle(ExecuteResourceMessage message);

        void Handle(SaveResourceMessage message);

        void Handle(UpdateWorksurfaceDisplayName message);
        
        void SetDebugStatus(DebugStatus debugStatus);

        void Debug(IContextualResourceModel resourceModel, bool isDebug);

        void StopExecution();

        void ViewInBrowser();

        void QuickViewInBrowser();

        void QuickDebug();

        void BindToModel();

        void ShowSaveDialog(IContextualResourceModel resourceModel, bool addToTabManager);

        bool Save(bool isLocalSave, bool isStudioShutdown);

        bool IsEnvironmentConnected();

        void FindMissing();

        void Debug();

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
