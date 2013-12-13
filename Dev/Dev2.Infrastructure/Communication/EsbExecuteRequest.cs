using System.Collections.Generic;
using System.Text;

namespace Dev2.Communication
{

    public class EsbExecuteRequest
    {
        public string ServiceName { get; set; }

        public Dictionary<string, StringBuilder> Args { get; set; }

        public StringBuilder ExecuteResult { get; set; }
 
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
