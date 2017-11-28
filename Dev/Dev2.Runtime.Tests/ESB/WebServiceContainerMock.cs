/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data.TO;
using Dev2.Runtime.ESB.Execution;
using Dev2.Services.Execution;

namespace Dev2.Tests.Runtime.ESB
{
    public class WebServiceContainerMock : WebServiceContainer
    {
        public WebServiceContainerMock(IServiceExecution webServiceExecution) 
            : base(webServiceExecution)
        {
        }

        public string WebRequestRespsonse { get; set; }

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            return DataObject.DataListID;
        }
    }
}
