/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.UnitTestAttributes;

namespace Dev2.Integration.Tests.Services.Sql
{
    public static class SqlServerTestUtils
    {
        public static DbSource CreateDev2TestingDbSource(string server, int port, AuthenticationType authenticationType = AuthenticationType.User)
        {
            var dbSource = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dev2TestingDB",
                DatabaseName = "Dev2TestingDB",
                Server = server,
                AuthenticationType = authenticationType,
                ServerType = enSourceType.SqlDatabase,
                ReloadActions = true,
                UserID = authenticationType == AuthenticationType.User ? "testuser" : null,
                Password = authenticationType == AuthenticationType.User ? "test123" : null,
                ConnectionTimeout = 30,
                Port = port
            };
            return dbSource;
        }
    }
}
