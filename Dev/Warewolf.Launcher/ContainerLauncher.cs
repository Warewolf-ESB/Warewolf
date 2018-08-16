using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Threading.Tasks;

namespace Warewolf.Launcher
{
    public class ContainerLauncher : IDisposable
    {
        string remoteSwarmDockerApi;
        string serverContainerID = null;
        string serviceID = null;
        string FullImageID = null;
        DateTime startTime;
        public string Hostname;
        public string IP;
        public string Status;
        public string Version;
        public string ImageName;
        public string LogOutputDirectory = Environment.ExpandEnvironmentVariables("%TEMP%");
        public const string Username = "WarewolfAdmin";
        public const string Password = "W@rEw0lf@dm1n";

        public ContainerLauncher(string imageName, string hostname = "", string remoteDockerApi = "localhost", string version = "latest")
        {
            remoteSwarmDockerApi = remoteDockerApi;
            Hostname = hostname;
            Version = version;
            ImageName = imageName;
            if (CheckForSwarm())
            {
                StartWarewolfServerServiceOnSwarm();
            }
            else
            {
                TryRunContainer();
            }
            if (ImageName.ToLower() == "warewolfserver" ||
                ImageName.ToLower() == "ciremote")
            {
                WaitForServerInContainer();
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
                Dispose();
                throw e;
            }
        }

        void TryRunContainer()
        {
            try
            {
                RunAsContainer();
                InspectContainer();
            }
            catch (Exception e)
            {
                Dispose();
                throw e;
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
                serviceID = null;
            }
            if (serverContainerID != null)
            {
                StopContainer();
                if (ImageName.ToLower() == "warewolfserver" || ImageName.ToLower() == "ciremote")
                {
                    RecoverServerLogFile();
                }
                else
                {
                    RecoverLogFile();
                }
                DeleteContainer();
                serverContainerID = null;
            }
        }

        void RecoverLogFile()
        {
            var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}/logs?stderr=1&stdout=1";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Error recoving container log file: " + reader.ReadToEnd());
                    }
                    else
                    {
                        reader.BaseStream.CopyToAsync(Console.OpenStandardOutput());
                    }
                }
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
                ""MemoryBytes"": 700000000
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
                try
                {
                    var response = client.GetAsync(url).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
                catch(AggregateException e) { return false; }
                catch (SocketException e) { return false; }
                catch (HttpRequestException e) { return false; }
            }
        }

        void Pull()
        {
            Console.WriteLine($"Pulling warewolfserver/{ImageName}:{Version} to {remoteSwarmDockerApi}");
            var url = $"http://{remoteSwarmDockerApi}:2375/images/create?fromImage=warewolfserver%2F{ImageName}&tag={Version}";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(1, 0, 0);
                try
                {
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
                catch (SocketException e)
                {
                    if (e.Message == $"No connection could be made because the target machine actively refused it {remoteSwarmDockerApi}:2375" ||
                        e.Message == "No connection could be made because the target machine actively refused it 127.0.0.1:2375" ||
                        e.Message == "No connection could be made because the target machine actively refused it localhost:2375")
                    {
                        throw new Exception($"Cannot connect to docker remote api at {remoteSwarmDockerApi}. Check this job has the \"Docker\" requirement and that the agent runinng this job is fully Docker capable.");
                    }
                }
            }
        }

        void InspectContainer()
        {
            int count = 0;
            Console.WriteLine($"Inspecting {serverContainerID.Substring(0, 12)} on {remoteSwarmDockerApi}.");
            while ((Status != "healthy" || string.IsNullOrEmpty(IP)) && ++count < 100)
            {
                var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}/json";
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 3, 0);
                    var response = client.GetAsync(url).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            ParseForNetworkID(reader.ReadToEnd());
                        }
                        if ((Status != "healthy" || string.IsNullOrEmpty(IP)) && count < 100)
                        {
                            Console.WriteLine($"Still inspecting {serverContainerID.Substring(0, 12)}.");
                            Thread.Sleep(3000);
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
                client.Timeout = new TimeSpan(0, 3, 0);
                int retryCount = 0;
                while (++retryCount < 10)
                {
                    try
                    {
                        var response = client.PostAsync(url, containerStartContent).Result;
                        var streamingResult = response.Content.ReadAsStreamAsync().Result;
                        using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                throw new HttpRequestException($"Error {(int)response.StatusCode} starting container. " + reader.ReadToEnd());
                            }
                            else
                            {
                                startTime = DateTime.Now;
                                return;
                            }
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        if (retryCount == 9)
                        {
                            Console.WriteLine("Timed out waiting for start container.");
                            throw e;
                        }
                        Console.WriteLine($"Still waiting for container {serverContainerID.Substring(0, 12)} to start.");
                        Thread.Sleep(3000);
                    }
                }
            }
        }

        void WaitForServerInContainer()
        {
            Console.WriteLine($"Waiting for Warewolf server to start in {serverContainerID.Substring(0, 12)}.");
            
            var url = $"http://{Hostname}:3142/apis.json";
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                int retryCount = 0;
                while (++retryCount < 10)
                {
                    try
                    {
                        var result = client.GetAsync(url).Result;
                        if (result != null)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"Still waiting for Warewolf server in {serverContainerID.Substring(0, 12)} to start.");
                        Thread.Sleep(1000);
                    }
                }
                if (retryCount >= 10)
                {
                    client.Dispose();
                    throw new TimeoutException($"Timed out waiting for Warewolf server in {serverContainerID.Substring(0, 12)} to start.");
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
     ""AutoRemove"": true,
     ""HostConfig"":
     {
          ""Memory"": 700000000
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
     ""AutoRemove"": true,
     ""HostConfig"":
     {
          ""Memory"": 700000000
     }
}
");
            }
            containerContent.Headers.Remove("Content-Type");
            containerContent.Headers.Add("Content-Type", "application/json");
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 30);
                try
                {
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
                            serverContainerID = ParseForContainerID(reader.ReadToEnd());
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to get container ID.");
                }
            }
            if (string.IsNullOrEmpty(serverContainerID))
            {
                serverContainerID = GetNewContainerID();
            }
            if (string.IsNullOrEmpty(Hostname))
            {
                Hostname = serverContainerID.Substring(0, 12);
            }
        }

        string GetNewContainerID()
        {
            Console.WriteLine($"Getting Container ID from {remoteSwarmDockerApi}.");
            var url = $"http://{remoteSwarmDockerApi}:2375/containers/json?all=true";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 3, 0);
                int retryCount = 0;
                while (++retryCount < 10) 
                {
                    var response = client.GetAsync(url).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    var result = string.Empty;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            result = ParseForNewContainerID(reader.ReadToEnd());
                        }
                    }
                    if (!string.IsNullOrEmpty(result))
                    {
                        client.Dispose();
                        return result;
                    }
                    Console.WriteLine($"Still Getting Container ID from {remoteSwarmDockerApi}.");
                    Thread.Sleep(1000);
                }
            }
            throw new TimeoutException($"Timed out waiting for container ID after creating a new container on {remoteSwarmDockerApi}.");
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
                if (responseText.Contains($"Status: Image is up to date for warewolfserver/{ImageName}:{Version}") || responseText.Contains($"Status: Downloaded newer image for warewolfserver/{ImageName}:{Version}"))
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

        string ParseForNewContainerID(string responseText)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var JSONObj = javaScriptSerializer.Deserialize<List<CreateContainer>>(responseText);
            if (JSONObj.Count > 0)
            {
                var UnixEpochTimeNow = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                Console.WriteLine($"Found container {JSONObj[0].ID}. Created {(UnixEpochTimeNow - int.Parse(JSONObj[0].Created))/10}ms ago.");
                return JSONObj[0].ID;
            }
            return null;
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
            Status = JSONObj.State.Health?.Status??"healthy";
            if (Status == "healthy")
            {
                Console.WriteLine($"Got IP Address for {serverContainerID.Substring(0, 12)} as {IP}.");
                Console.WriteLine($"Got Hostname for {serverContainerID.Substring(0, 12)} as {Hostname}.");
            }
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
            TimeSpan uptime = DateTime.Now.Subtract(startTime);
            string formatTimeSpan = uptime.Hours > 0 ? $"{uptime.Hours.ToString()} Hours." : uptime.Minutes > 0 ? $"{uptime.Minutes.ToString()} Minutes." : uptime.Seconds > 0 ? $"{uptime.Seconds.ToString()} Seconds." : $"{uptime.Milliseconds.ToString()} Milliseconds.";
            Console.WriteLine($"Stopping {serverContainerID.Substring(0, 12)} on {remoteSwarmDockerApi} after {formatTimeSpan}");
            var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}/stop";
            HttpContent containerStopContent = new StringContent("");
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 3, 0);
                var response = client.PostAsync(url, containerStopContent).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error {(int)response.StatusCode} stopping server container on {remoteSwarmDockerApi}: " + reader.ReadToEnd());
                    }
                    else
                    {
                        Console.WriteLine($"Server container {serverContainerID.Substring(0, 12)} at {remoteSwarmDockerApi} has been stopped.");
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
                        ExtractTar(reader.BaseStream);
                        string destFileName = Path.Combine(LogOutputDirectory, $"Container {serverContainerID.Substring(0, 12)} Warewolf Server.log");
                        File.Move(Path.Combine(LogOutputDirectory, "warewolf-server.log"), destFileName);
                        Console.WriteLine($"Recovered server container log file to \"{destFileName}\"");
                    }
                }
            }
        }

        void ExtractTar(Stream tarSteam)
        {
            using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(tarSteam, TarBuffer.DefaultBlockFactor))
            {
                tarArchive.ExtractContents(LogOutputDirectory);
            }
        }
    }

    class ServerContainer
    {
        public ServerContainerNetworkSettings NetworkSettings { get; set; }
        public ServerContainerConfig Config { get; set; }
        public ServerContainerState State { get; set; }
    }

    public class ServerContainerState
    {
        public ServerContainerHealth Health;
    }

    public class ServerContainerHealth
    {
        public string Status;
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
        public string Created { get; set; }
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
