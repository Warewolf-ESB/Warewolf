using Dev2.DataList.Contract;

namespace Dev2.Data.Interfaces
{
    public interface IInputOutputViewModel
    {
        string Name { get; set; }
        bool IsSelected { get; set; }
        string Value { get; set; }
        string MapsTo { get; set; }
        string DefaultValue { get; set; }
        bool Required { get; set; }
        string RecordSetName { get; set; }
        string DisplayName { get; set; }
        string DisplayDefaultValue { get; }
        bool IsNew { get; set; }
        bool RequiredMissing { get; set; }
        string TypeName { get; set; }
        bool IsMapsToFocused { get; set; }
        bool IsValueFocused { get; set; }

        IDev2Definition GetGenerationTO();
    }
}
