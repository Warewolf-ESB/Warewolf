
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
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

namespace Dev2.Data.ServiceModel.Messages
{
    /// <summary>
    ///     Used to return a list of messages
    /// </summary>
    public class CompileMessageList : ICompileMessageList
    {
        public IList<ICompileMessageTO> MessageList { get; set; }

        public Guid ServiceID { get; set; }

        public int Count
        {
            get
            {
                return MessageList == null ? 0 : MessageList.Count;
            }
        }

        public IList<string> Dependants { get; set; }
    }
}
