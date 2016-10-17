using System;

// ReSharper disable MemberCanBeInternal

namespace Dev2.Runtime.Interfaces
{
    public interface IAuthorizer
    {
        void RunPermissions(Guid resourceId);
    }
}
