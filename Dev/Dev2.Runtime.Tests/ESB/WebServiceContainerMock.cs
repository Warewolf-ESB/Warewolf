
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.ESB
{
    public class WebServiceContainerMock : WebServiceContainer
    {
        // ReSharper disable once TooManyDependencies
        public WebServiceContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
        }

        public string WebRequestRespsonse { get; set; }

        public override Guid Execute(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return DataObject.DataListID;
        }
    }
}
