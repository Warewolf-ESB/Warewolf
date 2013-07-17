using Unlimited.Framework;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// Defines the requirements for an <see cref="IServer"/> provider.
    /// </summary>
    public interface IServerProvider
    {
        List<IServer> Load();
        List<IServer> Load(IEnvironmentRepository environmentRepository);
    }
}
