using System.Collections.Generic;
using Dev2.Common.Interfaces;
// ReSharper disable InconsistentNaming

namespace Dev2.Data
{
    public class ServiceTestOutputTO : IServiceTestOutput
    {
        public string Variable { get; set; }
        public string Value { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string AssertOp { get; set; }
        public bool HasOptionsForValue { get; set; }
        public List<string> OptionsForValue { get; set; }
    }
}