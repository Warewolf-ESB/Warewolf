
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class DisplayMessageBoxMessage
    {
        public string Heading { get; set; }
        public string Message { get; set; }
        public MessageBoxImage MessageBoxImage { get; set; }

        public DisplayMessageBoxMessage(string heading, string message, MessageBoxImage messageBoxImage)
        {
            Heading = heading;
            Message = message;
            MessageBoxImage = messageBoxImage;
        }
    }
}
