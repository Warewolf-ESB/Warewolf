using System;
using Dev2.Network;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using TechTalk.SpecFlow;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Net.Http;
using System.Text;

namespace Dev2.Activities.Specs.Deploy
{
    class ContainerOps
    {
        readonly string _remoteDockerApi = "test-load";
        string _remoteContainerID = null;
        string _remoteImageID = null;

        public void ConnectToRemoteServerContainer(string destinationServer)
        {
            var arg = @"C:\Program Files (x86)\Warewolf\Server";
            StartRemoteContainer(arg);

            var formattableString = $"http://{destinationServer}:3142";
            IServer remoteServer = new Server(new Guid(), new ServerProxy(formattableString, "WarewolfUser", "Dev2@dmin123"))
            {
                Name = destinationServer
            };
            ScenarioContext.Current.Add("destinationServer", remoteServer);
            remoteServer.Connect();
        }

        void StartRemoteContainer(string serverPath)
        {
            var tempTarFilePath = @"c:\temp\gzip-server.tar.gz";
            if (!File.Exists(tempTarFilePath))
            {
                CreateTarGZ(tempTarFilePath, serverPath);
            }
            var url = "http://" + _remoteDockerApi + ":2375/build";
            byte[] paramFileBytes = File.ReadAllBytes(tempTarFilePath);
            HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
            bytesContent.Headers.Remove("Content-Type");
            bytesContent.Headers.Add("Content-Type", "application/x-tar");
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(1, 0, 0);
                var response = client.PostAsync(url, bytesContent).Result;
                var streamingResult = response.Content.ReadAsStreamAsync().Result;
                using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error creating remote server image. " + reader.ReadToEnd());
                    }
                    else
                    {
                        var responseText = reader.ReadToEnd();
                        Console.Write("Build Image: " + responseText);
                        var parseAround = "Successfully built ";
                        if (responseText.Contains(responseText))
                        {
                            _remoteImageID = responseText.Substring(responseText.IndexOf(parseAround) + parseAround.Length, 12);
                        }
                        else
                        {
                            throw new HttpRequestException("Error creating remote server image. " + responseText);
                        }
                    }
                }
            }
            url = "http://" + _remoteDockerApi + ":2375/containers/create";
            HttpContent containerContent = new StringContent(@"
{
     ""Hostname"":"""",
     ""User"":"""",
     ""Memory"":0,
     ""MemorySwap"":0,
     ""AttachStdin"":false,
     ""AttachStdout"":true,
     ""AttachStderr"":true,
     ""PortSpecs"":null,
     ""Privileged"": false,
     ""Tty"":false,
     ""OpenStdin"":false,
     ""StdinOnce"":false,
     ""Env"":null,
     ""Dns"":null,
     ""Image"":""" + _remoteImageID + @""",
     ""Volumes"":{
            },
     ""VolumesFrom"":"""",
     ""WorkingDir"":""""
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
                        var createContainer = reader.ReadToEnd();
                        Console.Write("Create Container: " + createContainer);
                        _remoteContainerID = createContainer.Substring(7, 64);
                    }
                }
            }
            url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "/start";
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
                }
            }
        }

        public void DeleteRemoteContainer()
        {
            if (_remoteContainerID != null)
            {
                var url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "/exec";
                HttpContent containerExecContent = new StringContent(@"
{
  ""AttachStdin"": false,
  ""AttachStdout"": false,
  ""AttachStderr"": true,
  ""Cmd"": [""sc stop \""Warewolf Server\""""],
  ""DetachKeys"": ""ctrl-p,ctrl-q"",
  ""Privileged"": true,
  ""Tty"": true,
  ""User"": ""WarewolfUser:Dev2@dmin123""
}
");
                containerExecContent.Headers.Remove("Content-Type");
                containerExecContent.Headers.Add("Content-Type", "application/json");
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 20, 0);
                    var response = client.PostAsync(url, containerExecContent).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("Error stopping Warewolf Server in remote container. " + reader.ReadToEnd());
                        }
                        else
                        {
                            Console.Write("Stopped Warewolf Server: " + reader.ReadToEnd());
                        }
                    }
                }
                containerExecContent = new StringContent(@"
{
  ""AttachStdin"": true,
  ""AttachStdout"": true,
  ""AttachStderr"": true,
  ""Cmd"": [""cmd /c type \""C:\\ProgramData\\Warewolf\\Server Log\\warewolf - server.log\""""],
  ""DetachKeys"": ""ctrl-p,ctrl-q"",
  ""Privileged"": true,
  ""Tty"": true,
  ""User"": ""WarewolfUser:Dev2@dmin123""
}
");
                containerExecContent.Headers.Remove("Content-Type");
                containerExecContent.Headers.Add("Content-Type", "application/json");
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 20, 0);
                    var response = client.PostAsync(url, containerExecContent).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("Error starting remote server container. " + reader.ReadToEnd());
                        }
                        else
                        {
                            Console.Write("Got Warewolf Server Log: " + reader.ReadToEnd());
                        }
                    }
                }
                url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "/stop";
                HttpContent containerStopContent = new StringContent("");
                containerStopContent.Headers.Remove("Content-Type");
                containerStopContent.Headers.Add("Content-Type", "application/json");
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 20, 0);
                    var response = client.PostAsync(url, containerStopContent).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("Error starting remote server container. " + reader.ReadToEnd());
                        }
                    }
                }
                url = "http://" + _remoteDockerApi + ":2375/containers/" + _remoteContainerID + "?v=1";
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 20, 0);
                    var response = client.DeleteAsync(url).Result;
                    var streamingResult = response.Content.ReadAsStreamAsync().Result;
                    using (StreamReader reader = new StreamReader(streamingResult, Encoding.UTF8))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("Error deleting remote server container. " + reader.ReadToEnd());
                        }
                    }
                }
            }
            if (_remoteImageID != null)
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
                            throw new HttpRequestException("Error deleting remote server image. " + reader.ReadToEnd());
                        }
                    }
                }
            }
        }

        void CreateTarGZ(string tgzFilename, string sourceDirectory)
        {
            Stream outStream = File.Create(tgzFilename);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);

            tarArchive.Close();
        }

        void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            //
            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            //
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);
            //
            // Write each file to the tar.
            //
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }
    }
}
