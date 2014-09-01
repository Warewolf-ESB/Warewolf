using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;


// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentRepository : IFrameworkRepository<IEnvironmentModel>
    {
        IEnvironmentModel Source { get; }
        IEnvironmentModel ActiveEnvironment { get; set; }
        event EventHandler<EnvironmentEditedArgs> ItemEdited;
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
