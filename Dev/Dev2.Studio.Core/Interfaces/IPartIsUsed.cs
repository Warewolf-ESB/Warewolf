using Dev2.Data.Interfaces;

namespace Dev2.Studio.Core.Interfaces
{
    interface IPartIsUsed
    {
        void SetScalarPartIsUsed(IDataListVerifyPart part, bool isUsed);

        void SetRecordSetPartIsUsed(IDataListVerifyPart part, bool isUsed);

        void SetComplexObjectSetPartIsUsed(IDataListVerifyPart part, bool isUsed);
    }
}