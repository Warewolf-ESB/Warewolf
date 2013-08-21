using System;
using System.Collections.Generic;

namespace Dev2.Data.ServiceModel.Messages
{
    /// <summary>
    ///     Used to return a list of messages
    /// </summary>
    public class CompileMessageList
    {
        public IList<CompileMessageTO> MessageList { get; set; }

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