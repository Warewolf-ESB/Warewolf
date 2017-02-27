using Dev2.Data.Interfaces;

namespace Dev2.Studio.Interfaces
{
    public interface IPartIsUsed
    {
        void SetScalarPartIsUsed(IDataListVerifyPart part, bool isUsed);

        void SetRecordSetPartIsUsed(IDataListVerifyPart part, bool isUsed);

        void SetComplexObjectSetPartIsUsed(IDataListVerifyPart part, bool isUsed);
    }
}