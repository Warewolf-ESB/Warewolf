using Dev2.Common.Interfaces;

namespace Dev2.Data
{
    public class ServiceTestOutputTO : IServiceTestOutput
    {
        public string Variable { get; set; }
        public string Value { get; set; }
    }
}