using System;
using Dev2.DataList.Contract;

namespace Dev2.Server.DataList
{
    public sealed class DataListIDProvider : IDataListIDProvider
    {
        public Guid AllocateID()
        {
            return Guid.NewGuid();
        }

        public bool ValidateID(Guid id)
        {
            return id != Guid.Empty;
        }
    }
}
