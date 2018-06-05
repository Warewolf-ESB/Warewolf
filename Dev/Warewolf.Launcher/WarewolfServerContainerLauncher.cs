using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Warewolf.Launcher
{
    public class WarewolfServerContainerLauncher : IDisposable
    {
        public readonly string _remoteDockerApi;
        public string _remoteContainerID = null;
        public string _remoteImageID = null;
        public string _hostname;
        public string _ip;

        public WarewolfServerContainerLauncher(string hostname = "", string remoteDockerApi = "localhost")
        {
            _remoteDockerApi = remoteDockerApi;
            _hostname = hostname;            
            CheckDockerRemoteApiVersion();
            _ip = StartWarewolfServerContainer(hostname);
        }

        string StartWarewolfServerContainer(string hostname)
        {
            Pull();
            CreateContainer(hostname);
            StartContainer();
            if (hostname == "")
            {
                return GetContainerHostname();
            }
            else
            {
                return GetContainerIP();
            }
        }

        public void Dispose()
        {
            if (_remoteContainerID != null)
            {
                StopContainer();
                RecoverServerLogFile();
                DeleteContainer();
            }
            if (_remoteImageID != null)
            {
                Delete();
            }
        }

        string CheckDockerRemoteApiVersion()
        {
            var url = "http://" + _remoteDockerApi + ":2375/version";
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
            Console.WriteLine("Pulling warewolfserver/warewolfserver:latest to " + _remoteDockerApi);
            var url = "http://" + _remoteDockerApi + ":2375/images/create?fromImage=warewolfserver%2Fwarewolfserver&tag=latest";
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
                        _remoteImageID = ParseForImageID(reader.ReadToEnd());
                    }
                }
            }
        }

        string GetContainerHostname()
        {
            var url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "/json";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error getting container hostname. " + reader.ReadToEnd());
                    }
                    else
                    {
                        return ParseForHostname(reader.ReadToEnd());
                    }
                }
            }
        }

        string GetContainerIP()
        {
            var url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "/json";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error getting container hostname. " + reader.ReadToEnd());
                    }
                    else
                    {
                        return ParseForIP(reader.ReadToEnd());
                    }
                }
            }
        }

        void StartContainer()
        {
            var url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "/start";
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
                        throw new HttpRequestException("Error starting remote server container. " + reader.ReadToEnd());
                    }
                    else
                    {
                        Console.Write($"Started Warewolf Server Container on {_remoteDockerApi}. " + reader.ReadToEnd());
                    }
                }
            }
        }

        void CreateContainer(string hostname = "")
        {
            var url = "http://" + _remoteDockerApi + ":2375/containers/create";
            HttpContent containerContent;
            if (hostname == "")
            {
                containerContent = new StringContent(@"
{
     ""Image"":""" + _remoteImageID + @"""
}
");
            }
            else
            {
                containerContent = new StringContent(@"
{
    ""Hostname"": """ + hostname + @""",
     ""Image"":""" + _remoteImageID + @"""
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
                        _remoteContainerID = ParseForContainerID(reader.ReadToEnd());
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
                if (responseText.Contains("Status: Image is up to date for warewolfserver/warewolfserver:latest"))
                {
                    return "warewolfserver/warewolfserver";
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

        string ParseForHostname(string responseText)
        {
            var parseAround = "\"Hostname\":\"";
            if (responseText.Contains(parseAround))
            {
                string containerHostname = responseText.Substring(responseText.IndexOf(parseAround) + parseAround.Length, 12);
                Console.Write("Got Container Hostname: " + containerHostname);
                return containerHostname;
            }
            else
            {
                throw new HttpRequestException("Error getting container hostname. " + responseText);
            }
        }

        string ParseForIP(string responseText)
        {
            var parseFrom = "\"IPAddress\":\"";
            var parseTo = "\",";
            if (responseText.Contains(parseFrom))
            {
                int startIndex = responseText.IndexOf(parseFrom) + parseFrom.Length;
                string containerIP = responseText.Substring(startIndex, responseText.IndexOf(parseTo) - startIndex);
                Console.Write("Got Container IP: " + containerIP);
                return containerIP;
            }
            else
            {
                throw new HttpRequestException("Error getting container hostname. " + responseText);
            }
        }

        void Delete()
        {
            var url = "http://" + _remoteDockerApi + ":2375/images/" + _remoteImageID;
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.DeleteAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.Write("Error deleting remote server image. " + reader.ReadToEnd());
                    }
                }
            }
        }

        void DeleteContainer()
        {
            var url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "?v=1";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.DeleteAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.Write("Deleting remote server container: " + reader.ReadToEnd());
                    }
                }
            }
        }

        void StopContainer()
        {
            var url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "/stop";
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
                        Console.Write("Starting remote server container: " + reader.ReadToEnd());
                    }
                }
            }
        }

        void RecoverServerLogFile()
        {
            var url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "/archive?path=C%3A%5CProgramData%5CWarewolf%5CServer+Log%5Cwarewolf-server.log";
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 20, 0);
                var response = client.GetAsync(url).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.Write("Error Creating Stop Warewolf Server Command: " + reader.ReadToEnd());
                    }
                    else
                    {
                        Console.Write(ExtractTar(reader.BaseStream));
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
}
