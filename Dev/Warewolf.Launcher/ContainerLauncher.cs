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
        const string remoteSwarmDockerApi = "ASH";
        string serverContainerID = null;
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
                InspectContainer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
            var url = $"http://{remoteSwarmDockerApi}:2375/version";
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
            while (string.IsNullOrEmpty(IP) && count++<5)
            {
                Console.WriteLine($"Inspecting {serverContainerID.Substring(0, 8)} on {remoteSwarmDockerApi}.");
                var url = $"http://{remoteSwarmDockerApi}:2375/containers/{serverContainerID}/json";
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
                            ParseForNetworkID(reader.ReadToEnd());
                        }
                    }
                }
            }
        }

        void StartContainer()
        {
            Console.WriteLine($"Starting {serverContainerID.Substring(0, 8)} on {remoteSwarmDockerApi}.");
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
            Console.WriteLine($"Stopping {serverContainerID.Substring(0, 8)} on {remoteSwarmDockerApi}");
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
}
