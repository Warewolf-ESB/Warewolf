using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Newtonsoft.Json;

namespace Warewolf.UnitTestAttributes
{
    public class Depends : IDisposable
    {
        public static readonly List<string> RigOpsHosts =  new List<string>
        {
            "RSAKLFSVRHST1.premier.local",
            "t004124.premier.local",
            "rsaklfwynand.premier.local",
            "PIETER.premier.local",
            "localhost"
        };
        private string SelectedHost = "";
        
        static readonly string BackupServer = "RSAKLFSVRHST1.premier.local";
        public static readonly string TFSBLDIP = "TFSBLD.premier.local";
        public static readonly string SharepointBackupServer = BackupServer;
        static readonly string BackupCIRemoteServer = "tst-ci-remote.premier.local";
        static readonly bool EnableRigOpsWorkflows = false;

        public enum ContainerType
        {
            MySQL = 0,
            MSSQL = 1,
            PostGreSQL = 2,
            Warewolf = 3,
            RabbitMQ = 4,
            CIRemote = 5,
            Redis = 6,
            AnonymousRedis = 7,
            AnonymousWarewolf = 8,
            Elasticsearch = 9,
            AnonymousElasticsearch = 10,
            WebApi = 11
        }

        ContainerType _containerType;

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
                case ContainerType.Redis:
                    return "Redis";
                case ContainerType.AnonymousRedis:
                    return "AnonymousRedis";
                case ContainerType.AnonymousWarewolf:
                    return "AnonymousWarewolf";
                case ContainerType.Elasticsearch:
                    return "Elasticsearch";
                case ContainerType.AnonymousElasticsearch:
                    return "AnonymousElasticsearch";
                case ContainerType.WebApi:
                    return "WebApi";
            }

            throw new ArgumentOutOfRangeException();
        }

        string GitURL(ContainerType containerType)
        {
            string getUrl;
            switch (containerType)
            {
                case ContainerType.MySQL:
                    getUrl = "https://gitlab.com/warewolf/mysql-connector-testing";
                    break;
                case ContainerType.MSSQL:
                    getUrl = "https://gitlab.com/warewolf/mssql-connector-testing";
                    break;
                case ContainerType.PostGreSQL:
                    getUrl = "https://gitlab.com/warewolf/postgres-connector-testing";
                    break;
                case ContainerType.Warewolf:
                    getUrl = "https://gitlab.com/warewolf/warewolf";
                    break;
                case ContainerType.RabbitMQ:
                    getUrl = "https://gitlab.com/warewolf/rabbitmq-connector-testing";
                    break;
                case ContainerType.CIRemote:
                    getUrl = "https://gitlab.com/warewolf/remote-warewolf-connector-testing";
                    break;
                case ContainerType.Redis:
                    getUrl = "https://gitlab.com/warewolf/redis-connector-testing";
                    break;
                case ContainerType.AnonymousRedis:
                    getUrl = "https://gitlab.com/warewolf/anonymous-redis-connector-testing";
                    break;
                case ContainerType.AnonymousWarewolf:
                    getUrl = "https://gitlab.com/warewolf/anonymous-remote-warewolf-connector-testing";
                    break;
                case ContainerType.Elasticsearch:
                    getUrl = "https://gitlab.com/warewolf/elasticsearch-connector-testing";
                    break;
                case ContainerType.AnonymousElasticsearch:
                    getUrl = "https://gitlab.com/warewolf/anonymous-elasticsearch-connector-testing";
                    break;
                case ContainerType.WebApi:
                    getUrl = "https://gitlab.com/warewolf/Web-API-connector-testing";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Uri.EscapeDataString(getUrl);
        }

        public Container Container;

        public Depends() => throw new ArgumentNullException("Missing type of the container.");

        public Depends(ContainerType type, bool performSourceInjection = true)
        {
            _containerType = type;
            if (EnableRigOpsWorkflows)
            {
                using (var client = new WebClientWithExtendedTimeout
                    {Credentials = CredentialCache.DefaultNetworkCredentials})
                {
                    var result = "";
                    var retryCount = 0;
                    var containerUrl = GitURL(_containerType);
                    do
                    {
                        SelectedHost = RigOpsHosts.ElementAt(retryCount);
                        var address = $"http://{SelectedHost}:3142/public/Container/Run%20From%20Git.json?RepoURL={containerUrl}&ScriptPath={(_containerType==ContainerType.Warewolf?"Dev%5CDev2.Server%5CDockerfile.deploy.bat":"")}";
                        try
                        {
                            result = client.DownloadString(address);
                        }
                        catch (WebException)
                        {
                            //Retry another Rig Ops host
                        }
                        if (result == "" || result.Contains("\"ID\": \"\""))
                        {
                            retryCount++;
                        }
                        else
                        {
                            retryCount = RigOpsHosts.Count;
                        }
                    } while (retryCount < RigOpsHosts.Count);

                    Container = JsonConvert.DeserializeObject<Container>(result) ?? new Container();

                    if (!int.TryParse(Container.Port, out _))
                    {
                        Container.Port = GetBackupPort(_containerType);
                    }

                    Container.IP = SelectedHost;
                }
            }
            else
            {
                Container = new Container()
                {
                    IP = BackupServer,
                    Port=GetBackupPort(_containerType)
                };
            }

            if (!performSourceInjection) return;
            switch (_containerType)
            {
                case ContainerType.MySQL:
                    InjectMySQLContainer();
                    break;
                case ContainerType.MSSQL:
                    InjectMSSQLContainer();
                    break;
                case ContainerType.RabbitMQ:
                    InjectRabbitMQContainer();
                    break;
                case ContainerType.CIRemote:
                    InjectCIRemoteContainer();
                    break;
                case ContainerType.PostGreSQL:
                    InjectPostGreSQLContainer();
                    break;
                case ContainerType.AnonymousElasticsearch:
                    InjectElasticContainer();
                    break;
                case ContainerType.WebApi:
                    InjectWebApiContainer();
                    break;
            }
        }

        static string GetBackupPort(ContainerType type)
        {
            switch (type)
            {
                case ContainerType.MSSQL:
                    return "1433";
                case ContainerType.CIRemote:
                    return "3144";
                case ContainerType.MySQL:
                    return "3306";
                case ContainerType.PostGreSQL:
                    return "5432";
                case ContainerType.RabbitMQ:
                    return "5672";
                case ContainerType.Redis:
                    return "6379";
                case ContainerType.AnonymousRedis:
                    return "6380";
                case ContainerType.AnonymousWarewolf:
                    return "3148";
                case ContainerType.Warewolf:
                    return "3146";
                case ContainerType.AnonymousElasticsearch:
                    return "9200";
                case ContainerType.Elasticsearch:
                    return "9400";
                case ContainerType.WebApi:
                    return "8080";
            }
            throw new ArgumentOutOfRangeException();
        }

        public void Dispose()
        {
            //TODO: Stop containers when they are not in use as an optimization.
        }

        void Stop()
        {
            using (var client = new WebClient {Credentials = CredentialCache.DefaultNetworkCredentials})
            {
                var result =
                    client.DownloadString(
                        $"http://{SelectedHost}:3142/public/Container/Async/Stop/{ConvertToString(_containerType)}.json");
                var JSONObj = JsonConvert.DeserializeObject<StopContainer>(result);
                if (JSONObj.Result != "Success" &&
                    JSONObj.Result != "This API does not support stopping Linux containers." && JSONObj.Result != "")
                {
                    Console.WriteLine($"Cannot stop container{(result == string.Empty ? "." : ": " + result)}");
                }
            }
        }

        void InjectCIRemoteContainer()
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
            UpdateSourcesConnectionStrings(
                $"AppServerUri=http://{Container.IP}:{Container.Port}/dsf;WebServerPort={Container.Port};AuthenticationType=User;UserName=WarewolfAdmin;Password=W@rEw0lf@dm1n;",
                knownServerSources);
        }

        void InjectMSSQLContainer()
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
            UpdateSourcesConnectionStrings(
                $"Data Source={Container.IP},{Container.Port};Initial Catalog=Dev2TestingDB;User ID=testuser;Password=test123;",
                knownMssqlServerSources);
        }
        
        void InjectRabbitMQContainer()
        {
            var knownServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Sources\RabbitMq\testRabbitMQSource.bite",
                @"%programdata%\Warewolf\Resources\Sources\RabbitMq\testRabbitMQSource.xml"
            };
            UpdateSourcesConnectionStrings(
                $"HostName={Container.IP};Port={Container.Port};UserName=test;Password=test;VirtualHost=/",
                knownServerSources);
        }
        private void InjectElasticContainer()
        {
            var knownServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Sources\Elasticsearch\testElasticsearchSource.bite",
                @"%programdata%\Warewolf\Resources\Sources\Elasticsearch\testElasticsearchSource.xml"
            };
            UpdateSourcesConnectionStrings(
                $"HostName=http://{Container.IP};Port={Container.Port};UserName=test;Password=test;VirtualHost=/",
                knownServerSources);
        }
        void InjectPostGreSQLContainer()
        {
            var knownServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Sources\Database\NewPostgresSource.bite",
                @"%programdata%\Warewolf\Resources\Sources\Database\NewPostgresSource.xml",
                @"%programdata%\Warewolf\Resources\Sources\Database\PostgreSourceTest.bite",
                @"%programdata%\Warewolf\Resources\Sources\Database\PostgreSourceTest.xml"
            };
            UpdateSourcesConnectionStrings($"Host={Container.IP};Username=postgres;Password=test123;Database=TestDB",
                knownServerSources);
        }

        void InjectWebApiContainer()
        {
            UpdateSourcesConnectionStrings($"Address=http://{Container.IP}:{Container.Port}/api/products/Delete;DefaultQuery=;AuthenticationType=Anonymous",
                new List<string>
                {
                    @"%programdata%\Warewolf\Resources\Sources\Web\WebDeleteServiceSource.xml",
                    @"%programdata%\Warewolf\Resources\Sources\Web\WebDeleteServiceSource.bite"
                });
            UpdateSourcesConnectionStrings($"Address=http://{Container.IP}:{Container.Port}/api/products/Get;DefaultQuery=;AuthenticationType=Anonymous",
                new List<string>
                {
                    @"%programdata%\Warewolf\Resources\Sources\Web\WebGetServiceSource.xml",
                    @"%programdata%\Warewolf\Resources\Sources\Web\WebGetServiceSource.bite"
                });
            UpdateSourcesConnectionStrings($"Address=http://{Container.IP}:{Container.Port}/api/products/Put;DefaultQuery=;AuthenticationType=Anonymous",
                new List<string>
                {
                    @"%programdata%\Warewolf\Resources\Sources\Web\WebPutServiceSource.xml",
                    @"%programdata%\Warewolf\Resources\Sources\Web\WebPutServiceSource.bite"
                });
            UpdateSourcesConnectionStrings($"Address=http://{Container.IP}:{Container.Port}/api/products/Post;DefaultQuery=;AuthenticationType=Anonymous",
                new List<string>
                {
                    @"%programdata%\Warewolf\Resources\Sources\Web\WebPostServiceSource.xml",
                    @"%programdata%\Warewolf\Resources\Sources\Web\WebPostServiceSource.bite"
                });
        }

        void InjectSVRDEVIP()
        {
            UpdateSourcesConnectionStrings($"Server={BackupServer};Database=test;Uid=root;Pwd=admin;",
                new List<string>
                {
                    @"%programdata%\Warewolf\Resources\Sources\Database\NewMySqlSource.xml",
                    @"%programdata%\Warewolf\Resources\Sources\Database\NewMySqlSource.bite"
                });
            UpdateSourcesConnectionStrings($"Server=http://{BackupServer}/;AuthenticationType=User;UserName=integrationtester@dev2.local;Password=I73573r0",
                new List<string>
                {
                    @"programdata%\Warewolf\Resources\Sources\Sharepoint\SharePoint Test Server.xml",
                    @"programdata%\Warewolf\Resources\Sources\Sharepoint\SharePoint Test Server.bite"
                });
        }

        public static void InjectOracleSources()
        {
            var knownServerSources = new List<string>()
            {
                @"%programdata%\Warewolf\Resources\Sources\Database\NewOracleSource.bite",
                @"%programdata%\Warewolf\Resources\Sources\Database\NewOracleSource.xml"
            };
            UpdateSourcesConnectionStrings($"User Id=Testuser;Password=test123;Data Source={BackupServer};Database=HR;",
                knownServerSources);
        }

        void InjectMySQLContainer()
        {
            UpdateMySQLSourceConnectionString($"{Container.IP};Port={Container.Port}",
                @"%programdata%\Warewolf\Resources\Sources\Database\NewMySqlSource.bite");
        }

        static void UpdateMySQLSourceConnectionString(string defaultServer, string knownServerSource)
        {
            var sourcePath = Environment.ExpandEnvironmentVariables(knownServerSource);
            if (!File.Exists(sourcePath)) return;
            File.WriteAllText(sourcePath,
                InsertServerSourceAddress(File.ReadAllText(sourcePath),
                    $"Server={defaultServer};Database=test;Uid=root;Pwd=admin;"));
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
            var InjectedSource = false;
            foreach (var source in knownServerSources)
            {
                string sourcePath = Environment.ExpandEnvironmentVariables(source);
                if (File.Exists(sourcePath))
                {
                    File.WriteAllText(sourcePath,
                        InsertServerSourceAddress(File.ReadAllText(sourcePath), NewConnectionString));
                    InjectedSource = true;
                }
            }

            if (InjectedSource)
            {
                RefreshServer();
            }
        }

        public static void RefreshServer()
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
                try
                {
                    Console.WriteLine(client.DownloadString(
                        "http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true"));
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

            return serverSourceXML.Substring(0, startIndex) + newConnectionString +
                   serverSourceXML.Substring(startIndex, serverSourceXML.Length - startIndex);
        }
    }

    public class Container
    {
        public string ID { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
    }

    class StopContainer
    {
        public string Result { get; set; }
    }

    class WebClientWithExtendedTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 10 * 60 * 1000;
            return w;
        }
    }
}