using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Studio.ViewModels
{
    public class DatabaseService : IDatabaseService
    {
        #region Implementation of IDatabaseService

        public IDbSource Source { get; set; }
        public string Action { get; set; }
        public IList<IDbInput> Inputs { get; set; }
        public IList<IDbOutputMapping> OutputMappings { get; set; }

        #endregion
    }
}