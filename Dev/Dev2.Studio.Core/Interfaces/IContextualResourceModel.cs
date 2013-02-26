using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IContextualResourceModel : IResourceModel
    {
        IEnvironmentModel Environment { get; }
        Guid ServerID { get; }
        void UpdateIconPath(string iconPath);
    }
}
