namespace Dev2.Composition
{
    public class ImportServiceContext
    {
        private object _value;
        private int _hash;

        public ImportServiceContext()
        {
            _value = new object();
            _hash = _value.GetHashCode();
        }

        public override int  GetHashCode()
        {
 	        return _hash;
        }
    }
}
