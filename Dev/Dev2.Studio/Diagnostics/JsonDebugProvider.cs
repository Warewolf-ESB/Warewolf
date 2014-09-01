using System;
using System.Collections.Generic;
using System.Net;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Newtonsoft.Json;

namespace Dev2.Diagnostics
{
    public class JsonDebugProvider : IDebugProvider
    {
        public IEnumerable<IDebugState> GetDebugStates(string serverWebUri, DirectoryPath directory, FilePath filePath)
        {
            var webclient = new WebClient { Credentials = CredentialCache.DefaultCredentials };
            var address = String.Format(serverWebUri + "{0}/{1}?DirectoryPath={2}&FilePath={3}",
                "Services", "DebugStateService", directory.PathToSerialize, filePath.Title);
            var datalistJson = webclient.UploadString(address, string.Empty);
            return JsonConvert.DeserializeObject<IList<DebugState>>(datalistJson);
        }

        #region Implementation of IDebugProvider

        public IEnumerable<IDebugState> GetDebugStates(string serverWebUri, IDirectoryPath directory, IFilePath path)
        {
            yield break;
        }

        #endregion
    }
}
