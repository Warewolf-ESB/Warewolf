
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Web.Script.Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor)]
public class Depends : System.Attribute, IDisposable
{
    readonly string RigOpsHost = "RSAKLFSVRHST1";
    readonly string RigOpsDomain = "dev2.local";

    public enum ContainerType
    {
        MySQL = 0,
        MSSQL = 1,
        PostGreSQL = 2,
        Warewolf = 3,
        RabbitMQ = 4,
        CIRemote = 5
    }

    string ConvertToString(ContainerType containerType)
    {
        switch (containerType)
        {
            case ContainerType.MySQL:
                return "MySQL";
            case ContainerType.MSSQL:
                return "MSSQL";
            case ContainerType.PostGreSQL:
                return "PostGreSQL";
            case ContainerType.Warewolf:
                return "Warewolf";
            case ContainerType.RabbitMQ:
                return "RabbitMQ";
            case ContainerType.CIRemote:
                return "CIRemote";
        }
        throw new ArgumentOutOfRangeException();
    }

    ContainerType _containerType;

    public string Hostname;

    public Depends() { throw new ArgumentNullException("Missing type of the container."); }

    public Depends(ContainerType type) : this(type, "") { }

    public Depends(ContainerType type, string hostname)
    {
        _containerType = type;
        Hostname = hostname;
        using (var client = new WebClientWithExtendedTimeout { Credentials = CredentialCache.DefaultNetworkCredentials })
        {
            var result = client.DownloadString($"http://{RigOpsHost}.{RigOpsDomain}:3142/public/Container/Async/Start/{ConvertToString(_containerType)}.json?Hostname={hostname}");
            Hostname = ParseForHostname(result);
        }
        switch (_containerType)
        {
            case ContainerType.MySQL:
                StartRemoteMySQLContainer(true);
                break;
            case ContainerType.MSSQL:
                StartRemoteMSSQLContainer(true);
                break;
            case ContainerType.RabbitMQ:
                StartRemoteRabbitMQContainer(true);
                break;
            case ContainerType.CIRemote:
                StartRemoteCIRemoteContainer(true);
                break;
        }
    }

    string ParseForHostname(string result)
    {
        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        var JSONObj = javaScriptSerializer.Deserialize<NewContainer>(result);
        return JSONObj.ID.Substring(0, 12) + '.' + RigOpsDomain;
    }


    public void Dispose()
    {
        using (var client = new WebClient { Credentials = CredentialCache.DefaultNetworkCredentials })
        {
            var result = client.DownloadString($"http://{RigOpsHost}.{RigOpsDomain}:3142/public/Container/Async/Stop/{ConvertToString(_containerType)}.json");
            if (result != @"{
  ""Result"": ""Success""
}")
            {
                throw new Exception($"Cannot stop container{(result == string.Empty ? "." : ": " + result)}");
            }
        }
    }

    void StartRemoteCIRemoteContainer(bool EnableDocker)
    {
        var knownServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\ChangingServerAuthUITest.bite",
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\ChangingServerAuthUITest.xml",
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\Remote Connection Integration.bite",
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\Remote Connection Integration.xml",
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\Restricted Remote Connection.bite",
                @"%programdata%\Warewolf\Resources\Acceptance Testing Resources\Restricted Remote Connection.xml",
                @"%programdata%\Warewolf\Resources\ExistingCodedUITestServerSource.bite",
                @"%programdata%\Warewolf\Resources\ExistingCodedUITestServerSource.xml",
                @"%programdata%\Warewolf\Resources\Remote Container.bite",
                @"%programdata%\Warewolf\Resources\Remote Container.xml",
                @"%programdata%\Warewolf\Resources\RemoteServerToDelete.bite",
                @"%programdata%\Warewolf\Resources\RemoteServerToDelete.xml",
                @"%programdata%\Warewolf\Resources\Remote Connection Integration.bite",
                @"%programdata%\Warewolf\Resources\Remote Connection Integration.xml"
            };
        if (EnableDocker)
        {
            UpdateSourcesConnectionStrings($"AppServerUri=http://{Hostname}:3142/dsf;WebServerPort=3142;AuthenticationType=User;UserName=WarewolfAdmin;Password=W@rEw0lf@dm1n;", knownServerSources);
        }
        else
        {
            var defaultServer = GetIPAddress("tst-ci-remote");
            if (defaultServer != null)
            {
                UpdateSourcesConnectionStrings($"AppServerUri=http://{defaultServer}:3142/dsf;WebServerPort=3142;AuthenticationType=Windows", knownServerSources);
                Thread.Sleep(30000);
            }
        }
    }

    void StartRemoteMSSQLContainer(bool EnableDocker)
    {
        var knownMssqlServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Sources\Database\NewSqlServerSource.bite",
                @"%programdata%\Warewolf\Resources\Sources\Database\NewSqlServerSource.xml",
                @"%programdata%\Warewolf\Resources\Sources\Database\TestDb.bite",
                @"%programdata%\Warewolf\Resources\Sources\Database\TestDb.xml",
                @"%programdata%\Warewolf\Resources\Sources\Database\NewSqlBulkInsertSource.bite",
                @"%programdata%\Warewolf\Resources\Sources\Database\NewSqlBulkInsertSource.xml"
            };
        if (EnableDocker)
        {
            UpdateSourcesConnectionStrings($"Data Source={Hostname},1433;Initial Catalog=Dev2TestingDB;User ID=testuser;Password=test123;", knownMssqlServerSources);
            Thread.Sleep(30000);
        }
        else
        {
            var defaultServer = GetIPAddress("RSAKLFSVRDEV");
            if (defaultServer != null)
            {
                UpdateSourcesConnectionStrings($"Data Source={defaultServer},1433;Initial Catalog=Dev2TestingDB;User ID=testuser;Password=test123;", knownMssqlServerSources);
                Thread.Sleep(30000);
            }
        }
    }

    void StartRemoteRabbitMQContainer(bool EnableDocker)
    {
        var knownServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Sources\RabbitMq\testRabbitMQSource.bite",
                @"%programdata%\Warewolf\Resources\Sources\RabbitMq\testRabbitMQSource.xml"
            };
        if (EnableDocker)
        {
            UpdateSourcesConnectionStrings($"HostName={Hostname};Port=5672;UserName=guest;Password=guest;VirtualHost=/", knownServerSources);
            Thread.Sleep(120000);
        }
        else
        {
            var defaultServer = GetIPAddress("rsaklfsvrdev.dev2.local");
            if (defaultServer != null)
            {
                UpdateSourcesConnectionStrings($"HostName={defaultServer};Port=5672;UserName=test;Password=test;VirtualHost=/", knownServerSources);
                Thread.Sleep(30000);
            }
        }
    }

    void StartRemoteMySQLContainer(bool EnableDocker)
    {
        if (EnableDocker)
        {
            UpdateSourcesConnectionString(Hostname, @"%programdata%\Warewolf\Resources\Sources\Database\NewMySqlSource.bite");
            Thread.Sleep(30000);
        }
        else
        {
            var defaultServer = GetIPAddress("RSAKLFSVRDEV.dev2.local");
            if (defaultServer != null)
            {
                UpdateSourcesConnectionString(defaultServer, @"%programdata%\Warewolf\Resources\Sources\Database\NewMySqlSource.bite");
                Thread.Sleep(30000);
            }
        }
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

class NewContainer
{
    public string ID { get; set; }
    public string IP { get; set; }
}

class WebClientWithExtendedTimeout : WebClient
{
    protected override WebRequest GetWebRequest(Uri uri)
    {
        WebRequest w = base.GetWebRequest(uri);
        w.Timeout = 20 * 60 * 1000;
        return w;
    }
}