using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// Defines the requirements for an <see cref="IEnvironmentModel"/> provider.
    /// </summary>
    public interface IEnvironmentModelProvider
    {
        List<IEnvironmentModel> Load();
        List<IEnvironmentModel> Load(IEnvironmentRepository environmentRepository);
    }
}
