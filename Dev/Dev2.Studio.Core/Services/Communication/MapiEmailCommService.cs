
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using SendFileTo;
using ServiceStack.Common.Extensions;

//using Dev2.Studio.Core.Services.Communication.Mapi;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Services.Communication
{
    /// <summary>
    /// Used to invoke the default email client
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Jurie.smit</author>
    /// <datetime>2013/01/14-09:09 AM</datetime>
    public class MapiEmailCommService<T> : ICommService<T>
        where T : EmailCommMessage
    {
        /// <summary>
        /// Invokes the default email client and prepoluates the fields from message.
        /// </summary>
        /// <param name="message">The message to prepopulate with.</param>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:10 AM</datetime>
        public void SendCommunication(T message)
        {
            //Dev2Logger.Log.Debug();
            MAPI mapi = new MAPI();
            mapi.AddRecipientTo(message.To);

            if(!String.IsNullOrEmpty(message.AttachmentLocation))
            {
                if(message.AttachmentLocation.Contains(";"))
                {
                    var attachmentList = message.AttachmentLocation.Split(';');
                    attachmentList.ForEach(mapi.AddAttachment);
                }
                else
                {
                    mapi.AddAttachment(@message.AttachmentLocation);
                }
            }
            mapi.SendMailPopup(message.Subject, message.Content);
        }
    }
}
