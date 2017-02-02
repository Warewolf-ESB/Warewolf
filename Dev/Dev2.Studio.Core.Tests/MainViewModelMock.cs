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
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;

namespace Dev2.Core.Tests
{
    public class MainViewModelMock : MainViewModel
    {
        public MainViewModelMock(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, IVersionChecker versionChecker, bool createDesigners = true, IBrowserPopupController browserPopupController = null)
            : base(eventPublisher, asyncWorker, environmentRepository, versionChecker, createDesigners, browserPopupController)
        {
        }

        public int AddWorkspaceItemsHitCount { get; private set; }

        protected override void AddWorkspaceItems()
        {
            AddWorkspaceItemsHitCount++;
        }
    }
}
