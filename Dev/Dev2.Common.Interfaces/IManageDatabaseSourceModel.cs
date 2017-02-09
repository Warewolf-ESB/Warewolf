using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces
{
    public interface IManageDatabaseSourceModel
    {
        IList<string> GetComputerNames();
        IList<string> TestDbConnection(IDbSource resource);
        void Save(IDbSource toDbSource);
        string ServerName { get; }

        IDbSource FetchDbSource(Guid resourceID);
    }
}