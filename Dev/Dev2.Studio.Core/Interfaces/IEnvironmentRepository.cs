
using System;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentRepository : IFrameworkRepository<IEnvironmentModel>
    {
        IEnvironmentModel Source { get; }

        bool IsLoaded { get; set; }

        void Clear();

        IEnvironmentModel Fetch(IServer server);

        IList<Guid> ReadSession();

        void WriteSession(IEnumerable<Guid> environmentGuids);

    }
}
