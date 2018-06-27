using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Warewolf.Launcher
{
    public class ContainerLauncher : IDisposable
    {
        const string remoteSwarmDockerApi = "TEST-LOAD";
        string serverContainerID = null;
        string serviceID = null;
        string FullImageID = null;
        public string Hostname;
        public string IP;
        public string Version;
        public string ImageName;
        public const string Username = "WarewolfAdmin";
        public const string Password = "W@rEw0lf@dm1n";

        public ContainerLauncher(string hostname = "", string version = "latest", bool CIRemoteResources = false)
        {
            Hostname = hostname;
            Version = version;
            if (!CIRemoteResources)
            {
                ImageName = "warewolfserver";
            }
            else
            {
                ImageName = "ciremote";
            }
            if (CheckForSwarm())
            {
                StartWarewolfServerServiceOnSwarm();
            }
            else
            {
                StartWarewolfServerContainer();
            }
        }

        void StartWarewolfServerServiceOnSwarm()
        {
            try
            {
                RunAsServiceOnSwarm();
                InspectSwarmService();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Dispose();
            }
        }

        void StartWarewolfServerContainer()
        {
            try
            {
                RunAsContainer();
                InspectContainer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Dispose();
            }
        }

        void RunAsContainer()
        {
            Pull();
            CreateContainer();
            StartContainer();
        }

        public void Dispose()
        {
            if (serviceID != null)
            {
                DeleteService();
            }
            if (serverContainerID != null)
            {
                StopContainer();
                RecoverServerLogFile();
                DeleteContainer();
            }
        }

        void RunAsServiceOnSwarm()
        {
            var url = $"http://{remoteSwarmDockerApi}:2375/services/create";
            HttpContent containerContent = new StringContent(@"
{
    ""TaskTemplate"":
    {
        ""ContainerSpec"":
        {
             ""Image"":""warewolfserver/" + $"{ImageName}:{Version}" + @"""
        },
        ""Resources"": 
        {
            ""Limits"": 
            {
                ""MemoryBytes"": 1500000000
            }
        },
        ""RestartPolicy"": 
        {
            ""Condition"": ""none""
        }
    },
    ""Mode"": 
    {
        ""Replicated"": 
        {
            ""Replicas"": 1
        }
    },
    ""EndpointSpec"": 
    {
        ""Ports"": 
        [{
            ""Protocol"": ""tcp"", 
            ""PublishedPort"": 3142, 
            ""TargetPort"": 3142
        }]
    }
}
");
            containerContent.Headers.Remove("Content-Type");
            containerContent.Headers.Add("Content-Type", "application/json");
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.PostAsync(url, containerContent).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error creating service on swarm. " + reader.ReadToEnd());
                    }
                    else
                    {
                        serviceID = ParseForServiceID(reader.ReadToEnd());
                        Console.WriteLine($"Created warewolfserver/{ImageName}:{Version} service on swarm managed by {remoteSwarmDockerApi} as {serviceID}");
                    }
                }
            }
        }

        string ParseForServiceID(string responseText)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var JSONObj = javaScriptSerializer.Deserialize<CreateService>(responseText);
            return JSONObj.ID;
        }

        void DeleteService()
        {
            var url = $"http://{remoteSwarmDockerApi}:2375/services/{serviceID}";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.DeleteAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Error deleting service on swarm: " + reader.ReadToEnd());
                    }
                }
            }
        }

        bool CheckForSwarm()
        {
            var url = $"http://{remoteSwarmDockerApi}:2375/nodes";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    return response.IsSuccessStatusCode;
                }
            }
        }

        void Pull()
        {
            Console.WriteLine($"Pulling warewolfserver/{ImageName}:{Version} to {remoteSwarmDockerApi}");
            var url = $"http://{remoteSwarmDockerApi}:2375/images/create?fromImage=warewolfserver%2F{ImageName}&tag={Version}";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(1, 0, 0);
                var response = client.PostAsync(url, new StringContent("")).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error pulling image. " + reader.ReadToEnd());
                    }
                    else
                    {
                        FullImageID = ParseForImageID(reader.ReadToEnd());
                    }
                }
            }
        }

        void InspectContainer()
        {
            int count = 0;
            while (string.IsNullOrEmpty(IP) && count++ < 7)
            {
                Console.WriteLine($"Inspecting {serverContainerID.Substring(0, 12)} on {remoteSwarmDockerApi}.");
                var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}/json";
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 20, 0);
                    var response = client.GetAsync(url).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            ParseForNetworkID(reader.ReadToEnd());
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Still inspecting {serverContainerID.Substring(0, 12)}.");
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
        }

        void InspectSwarmService()
        {
            var state = "pending";
            int count = 0;
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            Thread.Sleep(1000);
            while (state != "running" && count++ < 100)
            {
                var url = $"http://{remoteSwarmDockerApi}:2375/tasks?filters=" + "{\"service\":{\"" + serviceID + "\":true}}";
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 20, 0);
                    var response = client.GetAsync(url).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var readInspectResponse = reader.ReadToEnd();
                            state = GetServiceStatus(readInspectResponse);
                            if (state == "running")
                            {
                                GetServiceMachineName(readInspectResponse);
                                break;
                            }
                            else
                            {
                                Console.WriteLine($"Still waiting for {serviceID}. It is currently {state}.");
                                Thread.Sleep(3000);
                            }
                        }
                        else 
                        {
                            throw new HttpRequestException(reader.ReadToEnd());
                        }
                    }
                }
            }
            if (count >= 100)
            {
                throw new TimeoutException("Timed out waiting for CI Remote server container to start.");
            }
        }

        string GetServiceStatus(string responseText)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var JSONObj = javaScriptSerializer.Deserialize<List<SwarmService>>(responseText);
            return JSONObj[0].Status.State;
        }

        void GetServiceMachineName(string responseText)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var JSONObj = javaScriptSerializer.Deserialize<List<SwarmService>>(responseText);
            var nodeID = JSONObj[0].NodeID;
            string nodeHostname;
            serverContainerID = JSONObj[0].Status.ContainerStatus.ContainerID;
            Console.WriteLine($"Inspecting Node {nodeID} on {remoteSwarmDockerApi}.");
            var url = $"http://{remoteSwarmDockerApi}:2375/nodes/{nodeID}";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        nodeHostname = ParseForNodeHostname(reader.ReadToEnd());
                    }
                    else
                    {
                        throw new HttpRequestException(reader.ReadToEnd());
                    }
                }
            }
            Console.WriteLine($"Inspecting {serverContainerID.Substring(0, 12)} on {nodeHostname}.");
            url = $"http://{nodeHostname}:2375/containers/{serverContainerID}/json";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        ParseForNetworkID(reader.ReadToEnd());
                    }
                    else
                    {
                        Console.WriteLine($"Still inspecting {serverContainerID.Substring(0, 12)}.");
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        void StartContainer()
        {
            Console.WriteLine($"Starting {serverContainerID.Substring(0, 12)} on {remoteSwarmDockerApi}.");
            var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}/start";
            HttpContent containerStartContent = new StringContent("");
            containerStartContent.Headers.Remove("Content-Type");
            containerStartContent.Headers.Add("Content-Type", "application/json");
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.PostAsync(url, containerStartContent).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error starting server container. " + reader.ReadToEnd());
                    }
                }
            }
        }

        void CreateContainer()
        {
            var url = $"http://{remoteSwarmDockerApi}:2375/containers/create";
            HttpContent containerContent;
            if (Hostname == "")
            {
                Console.WriteLine($"Creating {FullImageID} on {remoteSwarmDockerApi}");
                containerContent = new StringContent(@"
{
     ""Image"":""" + FullImageID + @""",
     ""HostConfig"":
     {
          ""NetworkMode"":""Default Switch""
     }
}
");
            }
            else
            {
                Console.WriteLine($"Creating {FullImageID} with hostname {Hostname} on {remoteSwarmDockerApi}");
                containerContent = new StringContent(@"
{
    ""Hostname"": """ + Hostname + @""",
     ""Image"":""" + FullImageID + @""",
     ""HostConfig"":
     {
          ""NetworkMode"":""Default Switch""
     }
}
");
            }
            containerContent.Headers.Remove("Content-Type");
            containerContent.Headers.Add("Content-Type", "application/json");
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.PostAsync(url, containerContent).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error creating remote server container. " + reader.ReadToEnd());
                    }
                    else
                    {
                        Console.WriteLine($"Created service on swarm {reader.ReadToEnd()}");
                    }
                }
            }
        }

        string ParseForImageID(string responseText)
        {
            var parseAround = "Successfully built ";
            if (responseText.Contains(parseAround))
            {
                Console.Write("Build Image: " + responseText);
                return responseText.Substring(responseText.IndexOf(parseAround) + parseAround.Length, 12);
            }
            else
            {
                if (responseText.Contains($"Status: Image is up to date for warewolfserver/{ImageName}:{Version}"))
                {
                    return $"warewolfserver/{ImageName}:{Version}";
                }
                else
                {
                    throw new HttpRequestException("Error parsing for image ID. " + responseText);
                }
            }
        }

        string ParseForContainerID(string responseText)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var JSONObj = javaScriptSerializer.Deserialize<CreateContainer>(responseText);
            return JSONObj.ID;
        }

        void ParseForNetworkID(string responseText)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var JSONObj = javaScriptSerializer.Deserialize<ServerContainer>(responseText);
            if (JSONObj.NetworkSettings.Networks.ContainsKey("nat"))
            {
                IP = JSONObj.NetworkSettings.Networks["nat"].IPAddress;
            }
            Hostname = JSONObj.Config.Hostname;
        }

        string ParseForNodeHostname(string responseText)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var JSONObj = javaScriptSerializer.Deserialize<Node>(responseText);
            return JSONObj.Description.Hostname;
        }

        void DeleteImage()
        {
            var url = $"http://{remoteSwarmDockerApi}:2375/images/{FullImageID}?force=true";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.DeleteAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Error deleting remote server image. " + reader.ReadToEnd());
                    }
                }
            }
        }

        void DeleteContainer()
        {
            var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}?v=1";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.DeleteAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Deleting remote server container: " + reader.ReadToEnd());
                    }
                }
            }
        }

        void StopContainer()
        {
            Console.WriteLine($"Stopping {serverContainerID.Substring(0, 12)} on {remoteSwarmDockerApi}");
            var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}/stop";
            HttpContent containerStopContent = new StringContent("");
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.PostAsync(url, containerStopContent).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error stopping server container on {remoteSwarmDockerApi}: " + reader.ReadToEnd());
                    }
                    else
                    {
                        Console.WriteLine($"Server container {serverContainerID} at {remoteSwarmDockerApi} has been stopped.");
                    }
                }
            }
        }

        void RecoverServerLogFile()
        {
            var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}/archive?path=C%3A%5CProgramData%5CWarewolf%5CServer+Log%5Cwarewolf-server.log";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Error recoving server log file: " + reader.ReadToEnd());
                    }
                    else
                    {
                        Console.WriteLine("Recovered server container log file: " + ExtractTar(reader.BaseStream));
                    }
                }
            }
        }

        string ExtractTar(Stream tarSteam)
        {
            using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(tarSteam, TarBuffer.DefaultBlockFactor))
            {
                tarArchive.ExtractContents(Environment.ExpandEnvironmentVariables("%TEMP%"));
            }
            return File.ReadAllText(Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "warewolf-server.log"));
        }
    }

    class ServerContainer
    {
        public ServerContainerNetworkSettings NetworkSettings { get; set; }
        public ServerContainerConfig Config { get; set; }
    }

    class ServerContainerConfig
    {
        public string Hostname { get; set; }
    }

    class ServerContainerNetwork
    {
        public string IPAddress { get; set; }
    }

    class ServerContainerNetworkSettings
    {
        public Dictionary<string, ServerContainerNetwork> Networks { get; set; }
    }

    class CreateContainer
    {
        public string ID { get; set; }
        public List<string> Warnings { get; set; }
    }

    class CreateService
    {
        public string ID { get; set; }
    }

    class SwarmService
    {
        public string NodeID { get; set; }
        public ServiceStatus Status;
    }

    class ServiceStatus
    {
        public ServiceContainer ContainerStatus;
        public string State { get; set; }
    }

    class ServiceContainer
    {
        public string ContainerID;
    }

    class Node
    {
        public NodeDescription Description;
    }

    public class NodeDescription
    {
        public string Hostname { get; set; }
    }
}
