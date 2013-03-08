using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DB_Support
{
    public class DBConnectionString
    {
        private readonly string _val;

        public DBConnectionString(string val)
        {
            _val = val;
        }

        public string Value
        {
            get
            {
                return _val;
            }
        }
    }
}
