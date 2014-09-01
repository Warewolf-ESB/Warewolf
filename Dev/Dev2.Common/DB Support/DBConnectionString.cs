using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.DB
{


// ReSharper disable InconsistentNaming
    public class DBConnectionString : IDBConnectionString
// ReSharper restore InconsistentNaming
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
