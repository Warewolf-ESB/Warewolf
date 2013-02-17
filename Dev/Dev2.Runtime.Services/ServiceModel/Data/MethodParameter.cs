
namespace Dev2.Runtime.ServiceModel.Data
{
    public class MethodParameter
    {
        public string Name { get; set; }
        public bool EmptyToNull { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
    }
}
