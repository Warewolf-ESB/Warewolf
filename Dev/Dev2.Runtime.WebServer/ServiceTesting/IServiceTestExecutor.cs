// 
// /*
// *  Warewolf - Once bitten, there's no going back
// *  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
// *  Licensed under GNU Affero General Public License 3.0 or later.
// *  Some rights reserved.
// *  Visit our website for more information <http://warewolf.io/>
// *  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
// *  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
// */

using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;

namespace Warewolf.Execution
{
    public interface IServiceTestExecutor
    {
#pragma warning disable CC0044 // You should use a class
        Task<IServiceTestModelTO> ExecuteTestAsyncTask(string serviceName, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, IDSFDataObject dataObjectClone);
#pragma warning restore CC0044 // You should use a class
        DataListFormat ExecuteTests(IDSFDataObject dataObject, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCatalog testCatalog, IResourceCatalog resourceCatalog, out string executePayload, ITestCoverageCatalog testCoverageCatalog);
    }
}