/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.WorkSurface;
using Moq;

namespace Dev2.Core.Tests
{
    public sealed class MainViewModelPersistenceMock : MainViewModel
    {
        public MainViewModelPersistenceMock(IEnvironmentRepository environmentRepository, bool createDesigners = true)
            : base(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, environmentRepository, new VersionChecker(), createDesigners)
        {
        }  
        
        public MainViewModelPersistenceMock(IEnvironmentRepository environmentRepository,IAsyncWorker asyncWorker, bool createDesigners = true)
            : base(new Mock<IEventAggregator>().Object, asyncWorker, environmentRepository, new VersionChecker(), createDesigners)
        {
          
        }

        public void TestClose()
        {
            OnDeactivate(true);
        }

        public void CallDeactivate(WorkSurfaceContextViewModel item)
        {
            DeactivateItem(item, true);
        }
    }
}
