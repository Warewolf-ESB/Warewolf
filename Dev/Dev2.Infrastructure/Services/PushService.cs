/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Communication;
using Dev2.Communication;

namespace Dev2.Services
{
    public class PushService : IPushService
    {
        readonly ISerializer _serializer;

        public PushService()
            : this(new Dev2JsonSerializer())
        {
        }

        public PushService(ISerializer serializer)
        {
            VerifyArgument.IsNotNull("serializer", serializer);
            _serializer = serializer;
        }
    }
}
