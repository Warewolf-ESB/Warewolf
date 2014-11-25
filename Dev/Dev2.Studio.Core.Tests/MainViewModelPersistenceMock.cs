
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.CustomControls.Connections;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Moq;

namespace Dev2.Core.Tests
{
    public sealed class MainViewModelPersistenceMock : MainViewModel
    {
        public MainViewModelPersistenceMock(IEnvironmentRepository environmentRepository, bool createDesigners = true)
            : base(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, environmentRepository, new VersionChecker(), createDesigners,
            studioResourceRepository: new Mock<IStudioResourceRepository>().Object, connectControlSingleton: new Mock<IConnectControlSingleton>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object)
        {
        }  
        
        public MainViewModelPersistenceMock(IEnvironmentRepository environmentRepository,IAsyncWorker asyncWorker, bool createDesigners = true)
            : base(new Mock<IEventAggregator>().Object, asyncWorker, environmentRepository, new VersionChecker(), createDesigners,
            studioResourceRepository: new Mock<IStudioResourceRepository>().Object, connectControlSingleton: new Mock<IConnectControlSingleton>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object)
        {
        }

        public void TestClose()
        {
            base.OnDeactivate(true);
        }

        public void CallDeactivate(WorkSurfaceContextViewModel item)
        {
            base.DeactivateItem(item, true);
        }
    }
}
