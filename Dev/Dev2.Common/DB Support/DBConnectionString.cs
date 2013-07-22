namespace Dev2.Common.DB
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
