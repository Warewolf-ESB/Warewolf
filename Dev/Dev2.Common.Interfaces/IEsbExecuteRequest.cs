using System.Collections.Generic;
using System.Text;

namespace Dev2.Common.Interfaces
{
    public interface IEsbExecuteRequest
    {
        string ServiceName { get; set; }
        Dictionary<string, StringBuilder> Args { get; set; }
        StringBuilder ExecuteResult { get; set; }
        bool WasInternalService { get; set; }
        void AddArgument(string key, StringBuilder value);
    }
}