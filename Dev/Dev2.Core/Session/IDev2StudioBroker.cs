using Dev2.Data.Translators;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Session
{
    public interface IDev2StudioSessionBroker : IDebugSession, ITranslate
    {
        // ReSharper disable InconsistentNaming
        string GetXMLForInputs(IBinaryDataList binaryDataList);
        // ReSharper restore InconsistentNaming
    }
}
