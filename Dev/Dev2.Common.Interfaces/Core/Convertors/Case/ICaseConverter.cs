using Dev2.Common.Interfaces.DataList.Contract;

namespace Dev2.Common.Interfaces.Core.Convertors.Case
{
    public interface ICaseConverter
    {
        IBinaryDataListItem TryConvert(string conversionType, IBinaryDataListItem item);
    }
}
