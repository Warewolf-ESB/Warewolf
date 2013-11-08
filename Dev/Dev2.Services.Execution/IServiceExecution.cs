using System;
using Dev2.DataList.Contract;

namespace Dev2.Services.Execution
{
    public interface IServiceExecution
    {
        IDSFDataObject DataObj { get; set; }
        string InstanceOutputDefintions { get; set; }
        void BeforeExecution(ErrorResultTO errors);
        Guid Execute(out ErrorResultTO errors);
        void AfterExecution(ErrorResultTO errors);
    }
}
