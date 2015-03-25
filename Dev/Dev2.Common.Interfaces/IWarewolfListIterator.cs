namespace Dev2.Common.Interfaces
{
    public interface IWarewolfListIterator
    {
        string FetchNextValue(string expression);

        void AddVariableToIterateOn(string expression);

        bool HasMoreData();
    }
}