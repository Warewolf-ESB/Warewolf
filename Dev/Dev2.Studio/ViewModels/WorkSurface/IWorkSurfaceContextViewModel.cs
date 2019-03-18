#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.Serialization;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Messages;
using Dev2.Security;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.Diagnostics;


namespace Dev2.Studio.ViewModels.WorkSurface
{
    public interface IWorkSurfaceContextViewModel : IDisposable, IScreen
    {
        IWorkSurfaceKey WorkSurfaceKey { get; }
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

        bool Save();
        bool Save(bool isLocalSave, bool isStudioShutdown);

        bool IsEnvironmentConnected();

        void FindMissing();

        void Debug();
        
        void RequestClose();
        
        void RequestClose(ViewModelDialogResults dialogResult);

        bool CanSave();
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
