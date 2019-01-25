/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageSqliteSourceViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManageSqliteSourceViewModel))]
        public void ManageSqliteSourceViewModel_Validate_Header()
        {
            var mockManageDatabaseSourceModel = new Mock<IManageDatabaseSourceModel>();
            var mockRequestServiceNameViewModel = new Mock<Task<IRequestServiceNameViewModel>>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockAsyncWorker = new Mock<IAsyncWorker>();

            using (var viewModel = new ManageSqliteSourceViewModel(mockManageDatabaseSourceModel.Object, mockRequestServiceNameViewModel.Object, mockEventAggregator.Object, mockAsyncWorker.Object))
            {
                Assert.AreEqual(Resources.Languages.Core.SqlServerSourceServerNewHeaderLabel, viewModel.HeaderText);
                Assert.AreEqual(Resources.Languages.Core.SqlServerSourceServerNewHeaderLabel, viewModel.Header);
            }
        }
    }
}
