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
using Hangfire.SqlServer;
using HangfireServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

#if NETFRAMEWORK
[assembly: OwinStartup(typeof(Dashboard))]
#endif
namespace HangfireServer
{
    public class Dashboard
	{
#if NETFRAMEWORK
        [ExcludeFromCodeCoverage]
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard("/" + Dev2.Common.Config.Persistence.DashboardName, new DashboardOptions()
            {
                Authorization = new[] { new HangFireAuthorizationFilter () },
                IgnoreAntiforgeryToken = true
            });
            
            if(Dev2.Common.Config.Persistence.UseAsServer)
            {
                app.UseHangfireServer(new BackgroundJobServerOptions()
                {
                    ServerName = Dev2.Common.Config.Persistence.ServerName,
                    ServerTimeout = TimeSpan.FromMinutes(10),
                    WorkerCount = Environment.ProcessorCount
                });
            }
        }
#else
		WebApplicationBuilder _builder = null;   
        public Dashboard()
        {
            _builder = WebApplication.CreateBuilder();
        }

        public IServiceCollection GetServices()
        {
            return _builder.Services;
        }

        [ExcludeFromCodeCoverage]
        public void Start(string startEndPoint)
        {
            if (Dev2.Common.Config.Persistence.UseAsServer)
            {
                _builder.Services.AddHangfireServer(provider => new BackgroundJobServerOptions
                {
                    ServerName = Dev2.Common.Config.Persistence.ServerName,
                    ServerTimeout = TimeSpan.FromMinutes(10),
                    WorkerCount = Environment.ProcessorCount
                });
            }
            else
                _builder.Services.AddHangfireServer();

            var app = _builder.Build();

            app.UseHangfireDashboard("/" + Dev2.Common.Config.Persistence.DashboardName, new DashboardOptions()
            {
                Authorization = new[] { new HangFireAuthorizationFilter() },
                IgnoreAntiforgeryToken = true
            });

            app.Urls.Add(startEndPoint);

            app.StartAsync().Wait();
        }
#endif
	}
}