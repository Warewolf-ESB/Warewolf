using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Warewolf.Launcher
{
    class RegistryContainerLauncher
    {
        ContainerLauncher launcher;

        public RegistryContainerLauncher(string hostname, string remoteDockerApi = "localhost")
        {
            launcher = new ContainerLauncher(hostname, remoteDockerApi);
            StartRegistryContainer(hostname);
        }

        public string StartRegistryContainer(string hostname)
        {
            Pull();
            launcher.CreateContainer();
            launcher.StartContainer();
            if (hostname == "")
            {
                return launcher.GetContainerHostname();
            }
            else
            {
                return hostname;
            }
        }

        void Pull()
        {
            var url = "http://" + launcher._remoteDockerApi + ":2375/images/create?fromImage=warewolfserver&tag=latest";
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
                        launcher._remoteImageID = launcher.ParseForImageID(reader.ReadToEnd());
                    }
                }
            }
        }
    }
}
