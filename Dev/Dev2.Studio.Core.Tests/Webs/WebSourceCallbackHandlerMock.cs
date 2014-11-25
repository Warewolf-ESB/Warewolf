
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

namespace Dev2.Core.Tests.Webs
{
    public class WebSourceCallbackHandlerMock : WebSourceCallbackHandler
    {
        public int StartUriProcessHitCount { get; set; }

        public WebSourceCallbackHandlerMock(IEnvironmentRepository environmentRepository)
            : base(environmentRepository)
        {
        }

        protected override void StartUriProcess(string uri)
        {
            StartUriProcessHitCount++;
        }
    }
}
