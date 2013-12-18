using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Session
{
    public interface IDev2StudioSessionBroker : IDebugSession, ITranslate
    {
        string GetXMLForInputs(IBinaryDataList binaryDataList);
    }
}
