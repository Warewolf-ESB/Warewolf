
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



// ReSharper disable once CheckNamespace

using Dev2.Common.Interfaces.Activity;
using Dev2.Common.Interfaces.Core;

namespace Dev2.Studio.Core
{
    public class ActivityMessage : IApplicationMessage
    {
        public event MessageEventHandler MessageReceived;

        public void SendMessage(string message)
        {
            if(MessageReceived != null)
            {
                MessageReceived(message);
            }
        }
    }
}
