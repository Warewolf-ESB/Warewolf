using System;
using System.Collections.Generic;
using System.Net;
using Dev2.Diagnostics;
using Newtonsoft.Json; 

namespace Dev2.Studio.Diagnostics
{ 
    public class JsonDebugProvider : IDebugProvider
    {
        public IEnumerable<DebugState> GetDebugStates(string serverWebUri, DirectoryPath directory, FilePath filePath)
        {
            var webclient = new WebClient();
            var address = String.Format(serverWebUri + "{0}/{1}?DirectoryPath={2}&FilePath={3}",
                "Services", "DebugStateService", directory.PathToSerialize, filePath.Title);
            var datalistJSON = webclient.UploadString(address, string.Empty);
            return JsonConvert.DeserializeObject<IList<DebugState>>(datalistJSON);
        }
    }
}
