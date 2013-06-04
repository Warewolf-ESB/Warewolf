using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class ShowDependenciesMessage : AbstractResourceMessage
    {
        public ShowDependenciesMessage(IResourceModel resourceModel) : base(resourceModel)
        {
        }
    }
}
