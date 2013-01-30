using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DB_Sanity
{
    public class AbstractSanitizer
    {

        internal string RemoveDelimiting(string payload)
        {
            return (payload.Replace("&lt;", "<").Replace("&gt;", ">"));
        }
    }
}
