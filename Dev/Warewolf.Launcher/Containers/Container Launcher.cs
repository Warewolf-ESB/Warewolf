using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Warewolf.Launcher
{
    public class ContainerLauncher
    {
        public readonly string _remoteDockerApi;
        public string _remoteContainerID = null;
        public string _remoteImageID = null;
        public string _hostname;

        public ContainerLauncher(string hostname = "", string remoteDockerApi = "localhost")
        {
            _remoteDockerApi = remoteDockerApi;
            GetDockerRemoteApiVersion();
        }

        public string GetDockerRemoteApiVersion()
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

        public string GetContainerHostname()
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

        public void StartContainer()
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
                        Console.Write("Started Remote Warewolf Server. " + reader.ReadToEnd());
                    }
                }
            }
        }

        public void CreateContainer(string hostname = "")
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

        public string ParseForImageID(string responseText)
        {
            var parseAround = "Successfully built ";
            if (responseText.Contains(parseAround))
            {
                Console.Write("Build Image: " + responseText);
                return responseText.Substring(responseText.IndexOf(parseAround) + parseAround.Length, 12);
            }
            else
            {
                throw new HttpRequestException("Error parsing for image ID. " + responseText);
            }
        }

        public string ParseForContainerID(string responseText)
        {
            if (responseText.Length > 7 + 64)
            {
                Console.Write("Create Container: " + responseText);
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
                Console.Write("Get Hostname: " + responseText);
                return responseText.Substring(responseText.IndexOf(parseAround) + parseAround.Length, 12);
            }
            else
            {
                throw new HttpRequestException("Error getting container hostname. " + responseText);
            }
        }

        public void Delete()
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

        public void DeleteContainer()
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

        public void StopContainer()
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
    }
}
