
namespace Dev2 {
    public interface IFrameworkDataFormatter {
        string FormatData(string formatDefinition, string dataToFormat, bool toDataFile = false);
    }
}
