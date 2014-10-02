
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Common.Interfaces.Activity;
using Dev2.Common.Interfaces.Core;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Administration
{
    public class BdsAdminChannel : IFrameworkDuplexCallbackChannel, IApplicationMessage
    {
        #region IFrameworkDuplexCallbackChannel Members

        public void CallbackNotification(string message)
        {
            SendMessage(message);
            Dev2Logger.Log.Info(message);
        }

        #endregion

        #region IApplicationMessage Members

        public event MessageEventHandler MessageReceived;

        public void SendMessage(string message)
        {
            if(MessageReceived != null)
            {
                MessageReceived(message);
            }
        }

        #endregion
    }
}
