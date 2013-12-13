using System.Collections.Generic;
using System.Text;

namespace Dev2.Communication
{

    /// <summary>
    /// Internal Service Request Object - Used mainly by the studio, but server can send request if service is internal
    /// </summary>
    public class EsbExecuteRequest
    {
        public string ServiceName { get; set; }

        public Dictionary<string, StringBuilder> Args { get; set; }

        public StringBuilder ExecuteResult { get; set; }
 
        public bool WasInternalService { get; set; }
 
        public EsbExecuteRequest()
        {
            ExecuteResult = new StringBuilder();
        }

        public void AddArgument(string key, StringBuilder value)
        {
            if (Args == null)
            {
                Args = new Dictionary<string, StringBuilder>();
            }

            Args.Add(key, value);
        }
    }
}
