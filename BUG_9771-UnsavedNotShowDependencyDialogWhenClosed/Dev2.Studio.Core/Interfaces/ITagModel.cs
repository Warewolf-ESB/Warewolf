namespace Dev2.Studio.Core.Interfaces {
    public interface ITagModel {
        string Tag { get; set; }
        object Resource { get; set; }
        bool IsSelected { get; set; }
    }
}