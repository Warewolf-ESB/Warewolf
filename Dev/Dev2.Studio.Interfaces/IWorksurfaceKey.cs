#pragma warning disable
using Dev2.Studio.Interfaces.Enums;
using System;

namespace Dev2.Common.Interfaces
{
    public interface IWorkSurfaceKey
    {
        Guid? EnvironmentID { get; set; }
        Guid? ResourceID { get; set; }
        Guid? ServerID { get; set; }
        WorkSurfaceContext WorkSurfaceContext { get; set; }
    }
}