/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Diagnostics.CodeAnalysis;
using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace HangfireServer
{
    public class Dashboard
    {
        [ExcludeFromCodeCoverage]
        public void Start(string startEndPoint)
        {
            var builder = WebApplication.CreateBuilder();

            if (Dev2.Common.Config.Persistence.UseAsServer)
            {
                builder.Services.AddHangfireServer(provider => new BackgroundJobServerOptions
                {
                    ServerName = Dev2.Common.Config.Persistence.ServerName,
                    ServerTimeout = TimeSpan.FromMinutes(10),
                    WorkerCount = Environment.ProcessorCount
                });
            }
            else
                builder.Services.AddHangfireServer();

            var app = builder.Build();

            app.UseHangfireDashboard("/" + Dev2.Common.Config.Persistence.DashboardName, new DashboardOptions()
            {
                Authorization = new[] { new HangFireAuthorizationFilter() },
                IgnoreAntiforgeryToken = true
            });

            app.Urls.Add(startEndPoint);

            app.StartAsync().Wait();
        }
    }
}