using System;

namespace Dev2.Communication
{
    public interface IMemo : IEquatable<IMemo>
    {
        Guid InstanceID { get; set; }

        Guid ServerID { get; set; }

        Guid WorkspaceID { get; set; }

        DateTime Date { get; set; }

        string DateString { get; }

        string ToString(ISerializer serializer);
    }
}