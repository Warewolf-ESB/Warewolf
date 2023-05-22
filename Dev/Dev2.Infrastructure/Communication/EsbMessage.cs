using Dev2.Common.Interfaces.Infrastructure.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Communication
{
    public  class EsbMessage : IEsbMessage
    {
        public bool MessagePublished { get; set; }
    }
}
