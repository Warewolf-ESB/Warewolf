// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface ITagCloudItemModel
    {
        string Tag { get; set; }
        bool IsSelected { get; set; }
        string TagLabel { get; }
        double Size { get; set; }
        int Count { get; set; }
    }
}