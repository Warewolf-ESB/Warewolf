namespace Dev2.Common.Interfaces
{
    public interface IWarewolfListIterator
    {
        string FetchNextValue(IWarewolfIterator iterator);

        void AddVariableToIterateOn(IWarewolfIterator iterator);

        bool HasMoreData();
    }
}
