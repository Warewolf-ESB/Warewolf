using System;

namespace Dev2.Common.Interfaces
{
    public interface IManageOAuthSourceModel
    {
        void Save(IOAuthSource toDbSource);
        string ServerName { get; }

        IOAuthSource FetchSource(Guid resourceID);
    }
}