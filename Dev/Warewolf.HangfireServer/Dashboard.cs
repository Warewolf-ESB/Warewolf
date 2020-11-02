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
using System.Diagnostics.CodeAnalysis;
using Hangfire;
using Microsoft.Owin;
using Owin;
using HangfireServer;

[assembly: OwinStartup(typeof(Dashboard))]
namespace HangfireServer
{
    public class Dashboard
    {
        [ExcludeFromCodeCoverage]
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard("/" + Dev2.Common.Config.Persistence.DashboardName);
            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                ServerName = Dev2.Common.Config.Persistence.ServerName,
                ServerTimeout = TimeSpan.FromMinutes(10)
            });

        }
    }
}