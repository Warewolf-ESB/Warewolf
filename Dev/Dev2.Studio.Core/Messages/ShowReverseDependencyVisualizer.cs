using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class ShowReverseDependencyVisualizer : IMessage
    {
        public IContextualResourceModel Model { get; set; }

        public ShowReverseDependencyVisualizer(IContextualResourceModel model)
        {
            Model = model;
        }
    }
}
