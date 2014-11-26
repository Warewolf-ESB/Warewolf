
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
