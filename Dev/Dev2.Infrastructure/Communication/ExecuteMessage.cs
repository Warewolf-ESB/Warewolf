
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using Dev2.Common.Interfaces.Infrastructure.Communication;

namespace Dev2.Communication
{
    public class ExecuteMessage : IExecuteMessage
    {
        public bool HasError { get; set; }

        public StringBuilder Message { get; set; }

        public ExecuteMessage()
        {
            Message = new StringBuilder();
        }

        public void SetMessage(string message)
        {
            Message.Append(message);
        }
    }
}
