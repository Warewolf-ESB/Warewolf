namespace Dev2.Data.Interfaces
{
    public interface IDev2ZipOperationTO : IZip
    {
        bool Overwrite { get; set; }
    }
}