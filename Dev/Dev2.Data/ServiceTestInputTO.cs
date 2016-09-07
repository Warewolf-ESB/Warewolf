using Dev2.Common.Interfaces;

namespace Dev2.Data
{
    public class ServiceTestInputTO : IServiceTestInput
    {
        public string Variable { get; set; }
        public string Value { get; set; }
        public bool EmptyIsNull { get; set; }
    }
}