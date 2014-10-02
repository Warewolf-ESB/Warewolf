
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Interfaces;
using Dev2.Webs.Callbacks;
using Newtonsoft.Json.Linq;

namespace Dev2.Core.Tests.Webs
{
    public class WebServiceCallbackHandlerMock : ServiceCallbackHandler
    {
        public WebServiceCallbackHandlerMock(IEnvironmentRepository environmentRepository)
            : base(environmentRepository)
        {
        }

        public void TestSave(IEnvironmentModel environmentModel, JObject jsonObj)
        {
            base.Save(environmentModel, jsonObj);
        }
    }
}
