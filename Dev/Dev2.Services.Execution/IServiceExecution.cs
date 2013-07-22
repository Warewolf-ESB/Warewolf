using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;

namespace Dev2.Services.Execution
{
    public interface IServiceExecution
    {
        Guid Execute(out ErrorResultTO errors);
    }
}
