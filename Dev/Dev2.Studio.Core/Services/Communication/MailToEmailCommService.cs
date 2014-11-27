
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Services.Communication
{
    public class MailToEmailCommService<T> : ICommService<T>
        where T : EmailCommMessage
    {
        public void SendCommunication(T message)
        {
            var mailto = string.Format(
                "mailto:{0}?Subject={1}&Body={2}&Attach={3}",
                message.To, message.Subject, message.Content, message.AttachmentLocation);
            Process.Start(mailto);
        }
    }
}
