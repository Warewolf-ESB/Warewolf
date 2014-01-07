using System;

namespace Dev2.DataList.Contract
{
    public interface IDataListIDProvider
    {
        Guid AllocateID();
        bool ValidateID(Guid id);
    }
}
