using System.Collections.Specialized;

namespace Dev2.Common.Interfaces.DataList
{
    public interface IEmptyExpression : IStorageExpression
    {
        
    }
    public interface IScalarExpression:IStorageExpression
    {
        string Name { get; }
    }

    public interface IRecordSetExpression : IStorageExpression
    {
        string Name { get; }

    }
}