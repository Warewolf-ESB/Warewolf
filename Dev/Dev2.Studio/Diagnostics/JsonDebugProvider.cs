using Dev2.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public class JsonDebugProvider : IDebugProvider
    {
        public IEnumerable<DebugState> GetDebugStates(string serverWebUri, DirectoryPath directory, FilePath filePath)
        {
            var webclient = new WebClient { Credentials = CredentialCache.DefaultCredentials };
            var address = String.Format(serverWebUri + "{0}/{1}?DirectoryPath={2}&FilePath={3}",
                "Services", "DebugStateService", directory.PathToSerialize, filePath.Title);
            var datalistJson = webclient.UploadString(address, string.Empty);
            return JsonConvert.DeserializeObject<IList<DebugState>>(datalistJson);
        }
    }
}
