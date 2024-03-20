﻿using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elsa.Studio.Workflows.Extensions
{
    public static class HubConnectionExtensions
    {
        public static IHubConnectionBuilder WithUrl(this IHubConnectionBuilder builder, string url, IHttpMessageHandlerFactory clientFactory)
        {
            return builder.WithUrl(url, options =>
            {
                options.HttpMessageHandlerFactory = _ => clientFactory.CreateHandler();
            });
        }
    }
}
