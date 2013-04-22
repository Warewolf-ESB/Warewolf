using System;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IContextualResourceModel : IResourceModel
    {
        IEnvironmentModel Environment { get; }
        Guid ServerID { get; set; }
        void UpdateIconPath(string iconPath);
    }
}
