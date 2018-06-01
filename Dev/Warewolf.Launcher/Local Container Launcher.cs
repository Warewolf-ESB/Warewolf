using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Warewolf.Launcher
{
    class LocalContainerLauncher
    {
        public LocalContainerLauncher(string hostname)
        {
            var launcher = new RemoteContainerLauncher("localhost");
            launcher.Build(RemoteContainerLauncher.GetServerPath());
            CreateContainer(launcher, hostname);
            launcher.StartContainer();
        }

        void CreateContainer(RemoteContainerLauncher launcher, string hostname)
        {
            var url = "http://localhost:2375/containers/create";
            HttpContent containerContent = new StringContent(@"
{
    ""Hostname"": """ + hostname + @""",
     ""Image"":""" + launcher._remoteImageID + @"""
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
                        throw new HttpRequestException("Error creating remote server container. " + reader.ReadToEnd());
                    }
                    else
                    {
                        launcher._remoteContainerID = launcher.ParseForContainerID(reader.ReadToEnd());
                    }
                }
            }
        }
    }
}
