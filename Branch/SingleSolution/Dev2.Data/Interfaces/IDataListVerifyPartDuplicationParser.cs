namespace Dev2.Data.Interfaces
{
    public interface IDataListVerifyPartDuplicationParser {
        bool Equals(IDataListVerifyPart Comparable, IDataListVerifyPart Comparor);
        int GetHashCode(IDataListVerifyPart DataListVerifyPart);
    }
}
