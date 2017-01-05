/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Services.Sql
{
    [TestClass]
    public class SqlServerTestUtils
    {
        // ReSharper disable InconsistentNaming

        public static DbSource CreateDev2TestingDbSource(AuthenticationType authenticationType = AuthenticationType.User)
        {
            var dbSource = new DbSource
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dev2TestingDB",
                DatabaseName = "Dev2TestingDB",
                Server = "RSAKLFSVRGENDEV",
                AuthenticationType = authenticationType,
                ServerType = enSourceType.SqlDatabase,
                ReloadActions = true,
                UserID = authenticationType == AuthenticationType.User ? "testUser" : null,
                Password = authenticationType == AuthenticationType.User ? "test123" : null
            };
            return dbSource;
        }

        // ReSharper restore InconsistentNaming
    }
}
