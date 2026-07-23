/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Common.Serializers;
using Dev2.Runtime.ServiceModel.Data;
using Hangfire;
using Hangfire.SqlServer;
using Warewolf.Security.Encryption;

namespace Warewolf.Driver.Persistence
{
    /// <summary>
    /// Builds the Hangfire SQL <see cref="JobStorage"/> from <c>Config.Persistence</c> —
    /// the single connection-resolution used by every host that touches the job store
    /// (the Execution Engine's resume path and the JobProcessor). Mirrors
    /// <c>HangfireScheduler.ConnectionString</c>: the <c>PersistenceDataSource</c> payload
    /// is a <c>Dev2JsonSerializer</c>-serialized <see cref="DbSource"/>, optionally
    /// encrypted (decrypted through <see cref="DpapiWrapper"/>, which routes to the AES
    /// hooks on Azure hosts).
    /// </summary>
    public static class HangfireStorageFactory
    {
        public static JobStorage BuildFromPersistenceConfig()
        {
            var payload = Config.Persistence.PersistenceDataSource?.Payload;
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new InvalidOperationException(
                    "Persistence data source payload is empty — persistence settings are not configured for this host.");
            }

            if (Config.Persistence.EncryptDataSource)
            {
                payload = payload.CanBeDecrypted() ? DpapiWrapper.Decrypt(payload) : payload;
            }

            var source = new Dev2JsonSerializer().Deserialize<DbSource>(payload)
                         ?? throw new InvalidOperationException(
                             "Persistence data source payload could not be deserialised to a DbSource.");

            return new SqlServerStorage(source.ConnectionString, new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = Config.Persistence.PrepareSchemaIfNecessary,
            });
        }
    }
}
