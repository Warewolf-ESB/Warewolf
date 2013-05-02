using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Messages
{
    public class ShowNewResourceWizard : IMessage
    {
        public string ResourceType { get; set; }

        public ShowNewResourceWizard(string resourceType)
        {
            ResourceType = resourceType;
        }
    }
}
