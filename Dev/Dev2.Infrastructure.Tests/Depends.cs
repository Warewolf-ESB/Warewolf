using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor)]
public class Depends: System.Attribute, IDisposable
{
    public enum ContainerType
    {
        MySQL = 0,
        MSSQL = 1,
        Oracle = 2,
        PostGreSQL = 3,
        Warewolf = 4,
        RabbitMQ = 5
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
            case ContainerType.Warewolf:
                return "Warewolf";
            case ContainerType.RabbitMQ:
                return "RabbitMQ";
        }
        throw new ArgumentOutOfRangeException();
    }

    ContainerType _containerType;

    public string Hostname;

    public Depends() { throw new ArgumentNullException("Missing type and hostname of the container."); }

    public Depends(ContainerType type, string hostname)
    {
        _containerType = type;
        Hostname = hostname;
        using (var client = new WebClient { Credentials = CredentialCache.DefaultNetworkCredentials })
        {
            var result = client.DownloadString($"http://T004124.premier.local:3142/public/Start/{ConvertToString(_containerType)}.json?Hostname={hostname}");
            if (result != "Success")
            {
                throw new Exception($"Cannot start container{(result == string.Empty ? "." : ": " + result)}");
            }
        }
    }

    public void Dispose()
    {
        using (var client = new WebClient { Credentials = CredentialCache.DefaultNetworkCredentials })
        {
            var result = client.DownloadString($"http://T004124.premier.local:3142/public/Stop/{ConvertToString(_containerType)}.json?Hostname={Hostname}");
            if (result != "Success")
            {
                throw new Exception($"Cannot stop container{(result == string.Empty ? "." : ": " + result)}");
            }
        }
    }

    public static Depends StartRemoteCIRemoteContainer(bool EnableDocker)
    {
        var knownServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\ChangingServerAuthUITest.xml",
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\Remote Connection Integration.xml",
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\Restricted Remote Connection.xml",
                @"%programdata%\Warewolf\Resources\ExistingCodedUITestServerSource.xml",
                @"%programdata%\Warewolf\Resources\Remote Container.bite",
                @"%programdata%\Warewolf\Resources\RemoteServerToDelete.xml",
                @"%programdata%\Warewolf\Resources\Remote Connection Integration.xml"
            };
        if (EnableDocker)
        {
            var Depends = new Depends(ContainerType.Warewolf, "test-remotewarewolf");
            UpdateSourcesConnectionStrings($"AppServerUri=http://{Depends.Hostname}:3142/dsf;WebServerPort=3142;AuthenticationType=Windows", knownServerSources);
            return Depends;
        }
        else
        {
            var defaultServer = GetIPAddress("tst-ci-remote.premier.local");
            if (defaultServer != null)
            {
                UpdateSourcesConnectionStrings($"AppServerUri=http://{defaultServer}:3142/dsf;WebServerPort=3142;AuthenticationType=Windows", knownServerSources);
                Thread.Sleep(30000);
            }
        }
        return null;
    }

    public static Depends StartRemoteMSSQLContainer(bool EnableDocker)
    {
        var knownMssqlServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Sources\Database\NewSqlServerSource.xml",
                @"%programdata%\Warewolf\Resources\Sources\Database\TestDb.bite",
                @"%programdata%\Warewolf\Resources\Sources\Database\NewSqlBulkInsertSource.xml"
            };
        if (EnableDocker)
        {
            var Depends = new Depends(ContainerType.MSSQL, "test-mssql");
            UpdateSourcesConnectionStrings($"Data Source={Depends.Hostname},1433;Initial Catalog=Dev2TestingDB;User ID=testuser;Password=test123;", knownMssqlServerSources);
            Thread.Sleep(30000);
            return Depends;
        }
        else
        {
            var defaultServer = GetIPAddress("SVRDEV.premier.local");
            if (defaultServer != null)
            {
                UpdateSourcesConnectionStrings($"Data Source={defaultServer},1433;Initial Catalog=Dev2TestingDB;User ID=testuser;Password=test123;", knownMssqlServerSources);
                Thread.Sleep(30000);
            }
        }
        return null;
    }

    public static Depends StartRemoteRabbitMQContainer(bool EnableDocker)
    {
        var knownServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Sources\Database\NewSqlServerSource.xml",
                @"%programdata%\Warewolf\Resources\Sources\Database\TestDb.bite"
            };
        if (EnableDocker)
        {
            var Depends = new Depends(ContainerType.RabbitMQ, "test-rabbitmq");
            UpdateSourcesConnectionStrings($"HostName={Depends.Hostname};Port=5672;UserName=guest;Password=guest;VirtualHost=/", knownServerSources);
            Thread.Sleep(120000);
            return Depends;
        }
        else
        {
            var defaultServer = GetIPAddress("SVRDEV.premier.local");
            if (defaultServer != null)
            {
                UpdateSourcesConnectionStrings($"HostName={defaultServer};Port=5672;UserName=test;Password=test;VirtualHost=/", knownServerSources);
                Thread.Sleep(30000);
            }
        }
        return null;
    }

    public static Depends StartRemoteMySQLContainer(bool EnableDocker)
    {
        if (EnableDocker)
        {
            var Depends = new Depends(ContainerType.MySQL, "test-mysql");
            UpdateSourcesConnectionString(Depends.Hostname, @"%programdata%\Warewolf\Resources\Sources\Database\NewMySqlSource.bite");
            Thread.Sleep(30000);
            return Depends;
        }
        else
        {
            var defaultServer = GetIPAddress("SVRDEV.premier.local");
            if (defaultServer != null)
            {
                UpdateSourcesConnectionString(defaultServer, @"%programdata%\Warewolf\Resources\Sources\Database\NewMySqlSource.bite");
                Thread.Sleep(30000);
            }
        }
        return null;
    }

    static void UpdateSourcesConnectionString(string defaultServer, string knownServerSource)
    {
        string sourcePath = Environment.ExpandEnvironmentVariables(knownServerSource);
        File.WriteAllText(sourcePath, InsertServerSourceAddress(File.ReadAllText(sourcePath), $"Server={defaultServer};Database=test;Uid=root;Pwd=admin;"));
        RefreshServer();
    }

    public static string GetIPAddress(string nameOrAddress)
    {
        string ipAddress = null;
        Ping pinger = null;

        try
        {
            pinger = new Ping();
            PingReply reply = pinger.Send(nameOrAddress);
            ipAddress = reply.Address.ToString();
        }
        catch (PingException)
        {
            // Discard PingExceptions and return false;
        }
        finally
        {
            if (pinger != null)
            {
                pinger.Dispose();
            }
        }

        return ipAddress;
    }

    static void UpdateSourcesConnectionStrings(string NewConnectionString, List<string> knownServerSources)
    {
        foreach (var source in knownServerSources)
        {
            string sourcePath = Environment.ExpandEnvironmentVariables(source);
            if (File.Exists(sourcePath))
            {
                File.WriteAllText(sourcePath, InsertServerSourceAddress(File.ReadAllText(sourcePath), NewConnectionString));
            }
        }
        RefreshServer();
    }

    public static void RefreshServer()
    {
        using (WebClient client = new WebClient())
        {
            client.Credentials = CredentialCache.DefaultNetworkCredentials;
            try
            {
                client.DownloadString("http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true");
            }
            catch (WebException e)
            {
                Console.WriteLine($"Cannot refresh server to redirect server sources. {e.Message}");
            }
        }
    }

    static string InsertServerSourceAddress(string serverSourceXML, string newConnectionString)
    {
        var startFrom = "ConnectionString=\"";
        string subStringTo;
        const string serverSourceSubStringEnd = "\" Type=\"Connection\" ";
        const string altServerSourceSubStringEnd = "\" Type=\"Dev2Server\" ";
        if (serverSourceXML.Contains(serverSourceSubStringEnd))
        {
            subStringTo = serverSourceSubStringEnd;
        }
        else if (serverSourceXML.Contains(altServerSourceSubStringEnd))
        {
            subStringTo = altServerSourceSubStringEnd;
        }
        else
        {
            subStringTo = "\" ServerVersion=\"";
        }
        int startIndex = serverSourceXML.IndexOf(startFrom) + startFrom.Length;
        int length = serverSourceXML.IndexOf(subStringTo) - startIndex;
        string oldAddress = serverSourceXML.Substring(startIndex, length);
        if (!string.IsNullOrEmpty(oldAddress))
        {
            serverSourceXML = serverSourceXML.Replace(oldAddress, "");
        }
        return serverSourceXML.Substring(0, startIndex) + newConnectionString + serverSourceXML.Substring(startIndex, serverSourceXML.Length - startIndex);
    }
} 