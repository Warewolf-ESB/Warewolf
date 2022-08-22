using System;

public static class HttpRequestExtensions
{
    public static Uri ToUri(this Microsoft.AspNetCore.Http.HttpRequest request)
    {
        var hostComponents = request.Host.ToUriComponent().Split(':');

        var builder = new UriBuilder
        {
            Scheme = request.Scheme,
            Host = hostComponents[0],
            Path = request.Path,
            Query = request.QueryString.ToUriComponent()
        };

        if (hostComponents.Length == 2)
        {
            builder.Port = Convert.ToInt32(hostComponents[1]);
        }

        return builder.Uri;
    }
}
