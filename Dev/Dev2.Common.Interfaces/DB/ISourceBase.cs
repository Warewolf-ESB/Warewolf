namespace Dev2.Common.Interfaces.DB
{
    public interface ISourceBase<T>
    {
        T Item { get; set; }
        bool HasChanged { get; }
        T ToModel();

    }
}