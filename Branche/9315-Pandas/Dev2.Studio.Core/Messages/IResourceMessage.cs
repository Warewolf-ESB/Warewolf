using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public interface IResourceMessage : IMessage
    {
        IResourceModel ResourceModel { get; set; }
    }
}
