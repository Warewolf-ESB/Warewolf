/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Warewolf.Common.Interfaces.NetStandard20;

namespace Warewolf.Common.NetStandard20
{
    public class WebClientFactory : IWebClientFactory
    {
        public IWebClientWrapper New(string userName, string password)
        {
            return new WebClientWrapper(userName, password);
        }

        public IWebClientWrapper New()
        {
            return new WebClientWrapper();
        }
    }
}
