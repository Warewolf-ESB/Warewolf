using System;

namespace Dev2.Common.Interfaces
{
    public interface IWorkSurfaceKey
    {
        Guid? EnvironmentID { get; set; }
        Guid? ResourceID { get; set; }
        Guid? ServerID { get; set; }
    }
}