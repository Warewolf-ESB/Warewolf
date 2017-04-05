
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.Studio.Core.Messages
{
    public class DebugStatusMessage : IMessage
    {
        public DebugStatusMessage(bool debugStatus)
        {
            DebugStatus = debugStatus;
        }

        public bool DebugStatus { get; set; }
    }
}
