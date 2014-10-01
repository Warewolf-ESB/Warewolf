
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Studio.Core.Messages;

namespace Dev2.Messages
{
    public class DebugWriterWriteMessage : IMessage
    {
        public DebugWriterWriteMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DebugWriterWriteMessage(IDebugState debugState)
        {
            DebugState = debugState;
        }



        public IDebugState DebugState { get; set; }
    }
}
