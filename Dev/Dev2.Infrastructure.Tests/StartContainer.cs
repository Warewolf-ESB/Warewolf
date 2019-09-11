using System;
using System.Net;
using System.Reflection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor)]
public class StartContainer: System.Attribute, IDisposable
{
    public enum ContainerType
    {
        MySQL = 0,
        MSSQL = 1,
        Oracle = 2,
        PostGreSQL = 3
    }

    string ConvertToString(ContainerType containerType)
    {
        switch (_containerType)
        {
            case ContainerType.MySQL:
                return "MySQL";
            case ContainerType.MSSQL:
                return "MSSQL";
            case ContainerType.Oracle:
                return "Oracle";
            case ContainerType.PostGreSQL:
                return "PostGreSQL";
        }
        throw new ArgumentOutOfRangeException();
    }

    ContainerType _containerType;

    public string Hostname;

    public StartContainer() { throw new ArgumentNullException("Missing type and hostname of the container."); }

    public StartContainer(ContainerType type, string hostname)
    {
        _containerType = type;
        Hostname = hostname;
        var client = new WebClient { Credentials = CredentialCache.DefaultNetworkCredentials };
        var result = client.DownloadString($"http://T004124.premier.local:3142/public/Start/{ConvertToString(_containerType)}.json?Hostname={hostname}");
        if (result != "Success")
        {
            throw new Exception($"Cannot start container{(result==string.Empty?".":": "+result)}");
        }
    }

    public void Dispose()
    {
        var client = new WebClient { Credentials = CredentialCache.DefaultNetworkCredentials };
        var result = client.DownloadString($"http://T004124.premier.local:3142/public/Stop/{ConvertToString(_containerType)}.json?Hostname={Hostname}");
        if (result != "Success")
        {
            throw new Exception($"Cannot stop container{(result==string.Empty?".":": "+result)}");
        }
    }
} 