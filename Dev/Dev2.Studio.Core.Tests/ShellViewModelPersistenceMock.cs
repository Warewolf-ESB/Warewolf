/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.WorkSurface;
using Moq;

namespace Dev2.Core.Tests
{
    public sealed class ShellViewModelPersistenceMock : ShellViewModel
    {
        public ShellViewModelPersistenceMock(IServerRepository serverRepository, bool createDesigners = true)
            : base(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, serverRepository, new VersionChecker(), new Mock<IViewFactory>().Object, createDesigners)
        {
        }
        
        public ShellViewModelPersistenceMock(IServerRepository serverRepository, IPopupController popupController, IAsyncWorker asyncWorker, bool createDesigners = true)
            : base(new Mock<IEventAggregator>().Object, asyncWorker, serverRepository, new VersionChecker(), new Mock<IViewFactory>().Object, createDesigners, null, popupController, new Mock<IExplorerViewModel>().Object, null)
        {          
        }

        public void TestClose() => OnDeactivate(true);

        public void CallDeactivate(IWorkSurfaceContextViewModel item) => DeactivateItem(item, true);
    }
}
