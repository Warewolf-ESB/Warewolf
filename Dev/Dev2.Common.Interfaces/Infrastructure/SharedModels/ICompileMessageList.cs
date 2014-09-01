using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Infrastructure.SharedModels
{
    public interface ICompileMessageList
    {
        IList<ICompileMessageTO> MessageList { get; set; }
        Guid ServiceID { get; set; }
        int Count { get; }
        IList<string> Dependants { get; set; }
    }
}