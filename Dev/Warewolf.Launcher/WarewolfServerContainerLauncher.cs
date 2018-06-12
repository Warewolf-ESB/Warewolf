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
    public class WarewolfServerContainerLauncher : IDisposable
    {
        readonly string _remoteDockerApi;
        string serverContainerID = null;
        string serverContainerImageID = null;
        public string Hostname;
        public string IP;
        public string Version;
        public string ImageName;
        public const string Username = "WarewolfAdmin";
        public const string Password = "W@rEw0lf@dm1n";

        public WarewolfServerContainerLauncher(string remoteDockerApi = "localhost", string hostname = "", string version = "latest", bool CIRemoteResources = false)
        {
            _remoteDockerApi = remoteDockerApi;
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
            CheckDockerRemoteApiVersion();
            StartWarewolfServerContainer();
        }

        void StartWarewolfServerContainer()
        {
            try
            {
                Pull();
                CreateContainer();
                StartContainer();
                GetContainerIP();
            }
            catch (Exception)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (serverContainerID != null)
            {
                StopContainer();
                RecoverServerLogFile();
                DeleteContainer();
            }
        }

        string CheckDockerRemoteApiVersion()
        {
            var url = $"http://{_remoteDockerApi}:2375/version";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error getting Docker Remote Api version. " + reader.ReadToEnd());
                    }
                    else
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        void Pull()
        {
            Console.WriteLine($"Pulling warewolfserver/{ImageName}:{Version} to {_remoteDockerApi}");
            var url = $"http://{_remoteDockerApi}:2375/images/create?fromImage=warewolfserver%2F{ImageName}&tag={Version}";
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
                        serverContainerImageID = ParseForImageID(reader.ReadToEnd());
                    }
                }
            }
        }

        void GetContainerIP()
        {
            int count = 0;
            while (string.IsNullOrEmpty(IP) && count++<5)
            {
                var url = $"http://{_remoteDockerApi}:2375/containers/{serverContainerID}/json";
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 20, 0);
                    var response = client.GetAsync(url).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("Error getting container IP. " + reader.ReadToEnd());
                        }
                        else
                        {
                            string gotIP = ParseForIP(reader.ReadToEnd());
                            if (!string.IsNullOrEmpty(gotIP))
                            {
                                IP = gotIP;
                            }
                            else
                            {
                                Console.WriteLine($"Failed to get IP for container on {_remoteDockerApi}. " + reader.ReadToEnd());
                                Thread.Sleep(500);
                            }
                        }
                    }
                }
            }
        }

        void StartContainer()
        {
            var url = $"http://{_remoteDockerApi}:2375/containers/{serverContainerID}/start";
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
                    else
                    {
                        Console.WriteLine($"Started Server Container {serverContainerID} on {_remoteDockerApi}. " + reader.ReadToEnd());
                    }
                }
            }
        }

        void CreateContainer()
        {
            var url = $"http://{_remoteDockerApi}:2375/containers/create";
            HttpContent containerContent;
            if (Hostname == "")
            {
                containerContent = new StringContent(@"
{
     ""Image"":""" + serverContainerImageID + @"""
}
");
            }
            else
            {
                containerContent = new StringContent(@"
{
    ""Hostname"": """ + Hostname + @""",
     ""Image"":""" + serverContainerImageID + @"""
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
                        serverContainerID = ParseForContainerID(reader.ReadToEnd());
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
            if (responseText.Length > 7 + 64)
            {
                return responseText.Substring(7, 64);
            }
            else
            {
                throw new HttpRequestException("Error parsing for container ID. " + responseText);
            }
        }

        string ParseForIP(string responseText)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var JSONObj = javaScriptSerializer.Deserialize<ServerContainer>(responseText);
            return JSONObj.NetworkSettings.Networks["nat"].IPAddress;
        }

        void DeleteImage()
        {
            var url = $"http://{_remoteDockerApi}:2375/images/{serverContainerImageID}?force=true";
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
            var url = $"http://{ _remoteDockerApi}:2375/containers/{serverContainerID}?v=1";
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
            Console.WriteLine($"Stopping server container {serverContainerID} on {_remoteDockerApi}");
            var url = $"http://{_remoteDockerApi}:2375/containers/{serverContainerID}/stop";
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
                        Console.WriteLine($"Error stopping server container on {_remoteDockerApi}: " + reader.ReadToEnd());
                    }
                    else
                    {
                        Console.WriteLine($"Server container {serverContainerID} at {_remoteDockerApi} has been stopped.");
                    }
                }
            }
        }

        void RecoverServerLogFile()
        {
            var url = $"http://{_remoteDockerApi}:2375/containers/{serverContainerID}/archive?path=C%3A%5CProgramData%5CWarewolf%5CServer+Log%5Cwarewolf-server.log";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Error Creating Stop Warewolf Server Command: " + reader.ReadToEnd());
                    }
                    else
                    {
                        Console.WriteLine(ExtractTar(reader.BaseStream));
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
        public string Id { get; set; }
        public string Created { get; set; }
        public string Path { get; set; }
        public List<string> Args { get; set; }
        public ServerContainerState State { get; set; }
        public ServerContainerNetworkSettings NetworkSettings { get; set; }
    }

    class ServerContainerState
    {
        public string Status { get; set; }
        public bool Running { get; set; }
        public bool Paused { get; set; }
        public bool Restarting { get; set; }
        public bool OOMKilled { get; set; }
        public bool Dead { get; set; }
        public int Pid { get; set; }
        public int ExitCode { get; set; }
        public string Error { get; set; }
        public string StartedAt { get; set; }
        public string FinishedAt { get; set; }
        public string Image { get; set; }
        public string ResolvConfPath { get; set; }
        public string HostnamePath { get; set; }
        public string HostsPath { get; set; }
        public string LogPath { get; set; }
        public string Name { get; set; }
        public int RestartCount { get; set; }
        public string Driver { get; set; }
        public string MountLabel { get; set; }
        public string ProcessLabel { get; set; }
        public string AppArmorProfile { get; set; }
        public string ExecIDs { get; set; }
        public ServerContainerHostConfig HostConfig { get; set; }
        public string NetworkMode { get; set; }
        public string PortBindings { get; set; }
        public Dictionary<string, string> RestartPolicy { get; set; }
        public bool AutoRemove { get; set; }
        public string VolumeDriver { get; set; }
        public string VolumesFrom { get; set; }
        public string CapAdd { get; set; }
        public string CapDrop { get; set; }
        public string Dns { get; set; }
        public string DnsOptions { get; set; }
        public string DnsSearch { get; set; }
        public string ExtraHosts { get; set; }
        public string GroupAdd { get; set; }
        public string IpcMode { get; set; }
        public string Cgroup { get; set; }
        public string Links { get; set; }
        public int OomScoreAdj { get; set; }
        public string PidMode { get; set; }
        public bool Privileged { get; set; }
        public bool PublishAllPorts { get; set; }
        public bool ReadonlyRootfs { get; set; }
        public bool SecurityOpt { get; set; }
        public string UTSMode { get; set; }
        public string UsernsMode { get; set; }
        public string ShmSize { get; set; }
        public string ConsoleSize { get; set; }
        public string Isolation { get; set; }
        public int CpuShares { get; set; }
        public string Memory { get; set; }
        public string NanoCpus { get; set; }
        public string CgroupParent { get; set; }
        public string BlkioWeight { get; set; }
        public string BlkioWeightDevice { get; set; }
        public string BlkioDeviceReadBps { get; set; }
        public string BlkioDeviceWriteBps { get; set; }
        public string BlkioDeviceReadIOps { get; set; }
        public string BlkioDeviceWriteIOps { get; set; }
        public string CpuPeriod { get; set; }
        public string CpuQuota { get; set; }
        public string CpuRealtimePeriod { get; set; }
        public string CpuRealtimeRuntime { get; set; }
        public string CpusetCpus { get; set; }
        public string CpusetMems { get; set; }
        public string Devices { get; set; }
        public string DeviceCgroupRules { get; set; }
        public string DiskQuota { get; set; }
        public string KernelMemory { get; set; }
        public string MemoryReservation { get; set; }
        public string MemorySwap { get; set; }
        public string MemorySwappiness { get; set; }
        public string OomKillDisable { get; set; }
        public string PidsLimit { get; set; }
        public string Ulimits { get; set; }
        public string CpuCount { get; set; }
        public string CpuPercent { get; set; }
        public string IOMaximumIOps { get; set; }
        public int IOMaximumBandwidth { get; set; }
        public ServerContainerGraphDriver GraphDriver { get; set; }
        public List<string> Mounts { get; set; }
        public ServerContainerConfig Config { get; set; }
        public string User { get; set; }
        public bool AttachStdin { get; set; }
        public bool AttachStdout { get; set; }
        public bool AttachStderr { get; set; }
        public Dictionary<string, string> ExposedPorts { get; set; }
        public bool Tty { get; set; }
        public bool OpenStdin { get; set; }
        public bool StdinOnce { get; set; }
        public List<string> Env { get; set; }
        public bool ArgsEscaped { get; set; }
        public List<string> Volumes { get; set; }
        public string WorkingDir { get; set; }
        public List<string> Entrypoint { get; set; }
        public string OnBuild { get; set; }
        public string Labels { get; set; }
    }

    class ServerContainerHostConfig
    {
        public string Binds { get; set; }
        public string ContainerIDFile { get; set; }
        public Dictionary<string, string> LogConfig { get; set; }
    }

    class ServerContainerGraphDriverData
    {
        public string dir { get; set; }
    }

    class ServerContainerGraphDriver
    {
        public ServerContainerGraphDriverData Data { get; set; }
        public string Name { get; set; }
    }

    class ServerContainerConfig
    {
        public string Hostname { get; set; }
        public string Domainname { get; set; }
    }

    class ServerContainerNetwork
    {
        public string IPAMConfig { get; set; }
        public string Links { get; set; }
        public string Aliases { get; set; }
        public string NetworkID { get; set; }
        public string EndpointID { get; set; }
        public string Gateway { get; set; }
        public string IPAddress { get; set; }
        public string IPPrefixLen { get; set; }
        public string IPv6Gateway { get; set; }
        public string GlobalIPv6Address { get; set; }
        public string GlobalIPv6PrefixLen { get; set; }
        public string MacAddress { get; set; }
        public string DriverOpts { get; set; }
    }

    class ServerContainerNetworkSettings
    {
        public string Bridge { get; set; }
        public string SandboxID { get; set; }
        public bool HairpinMode { get; set; }
        public string LinkLocalIPv6Address { get; set; }
        public string LinkLocalIPv6PrefixLen { get; set; }
        public Dictionary<string, string> Ports { get; set; }
        public string SandboxKey { get; set; }
        public string SecondaryIPAddresses { get; set; }
        public string SecondaryIPv6Addresses { get; set; }
        public string EndpointID { get; set; }
        public string Gateway { get; set; }
        public string GlobalIPv6Address { get; set; }
        public string GlobalIPv6PrefixLen { get; set; }
        public string IPAddress { get; set; }
        public string IPPrefixLen { get; set; }
        public string IPv6Gateway { get; set; }
        public string MacAddress { get; set; }
        public Dictionary<string, ServerContainerNetwork> Networks { get; set; }
    }

}
