/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using System.Threading.Tasks;
using Dev2.Communication;
using Warewolf.EsbClient;

namespace Warewolf.Client
{
    public static class EnvironmentConnectionBaseExtensionMethods
    {
        public static Task<T> NewResourceRequest<T>(this IEnvironmentConnectionBase environmentConnection, Guid workspaceId, Guid resourceId) where T : class
        {
            var req = new ResourceRequest<T>(workspaceId, resourceId);
            var task = environmentConnection.ExecuteCommandAsync(req, workspaceId)
                .ContinueWith((Task<StringBuilder> t) =>
                {
                    var serializer = new Dev2JsonSerializer();
                    var payload = t.Result;
                    return serializer.Deserialize<T>(payload);
                });
            return task;
        }

    }
}
