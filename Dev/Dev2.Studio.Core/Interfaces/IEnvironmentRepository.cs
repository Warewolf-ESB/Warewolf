
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentRepository : IFrameworkRepository<IEnvironmentModel>
    {
        IEnvironmentModel Source { get; }
        IEnvironmentModel ActiveEnvironment { get; set; }

        bool IsLoaded { get; set; }

        void Clear();

        IEnvironmentModel Fetch(IEnvironmentModel server);

        IList<Guid> ReadSession();

        void WriteSession(IEnumerable<Guid> environmentGuids);

        void ForceLoad();

        void Remove(Guid id);

        IEnvironmentModel Get(Guid id);
       
    }
}
