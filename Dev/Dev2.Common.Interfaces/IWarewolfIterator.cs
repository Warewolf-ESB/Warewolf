namespace Dev2.Common.Interfaces
{
    public interface IWarewolfIterator
    {
        string GetNextValue();

        int GetLength();

        bool HasMoreData();
    }
}