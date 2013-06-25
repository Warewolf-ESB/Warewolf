using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class EnvironmentDeletedMessage : AbstractEnvironmentMessage
    {
        public EnvironmentDeletedMessage(IEnvironmentModel environmentModel) : base(environmentModel)
        {
        }
    }
}
