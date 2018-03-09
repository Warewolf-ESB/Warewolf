using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Net.Http;
using System.Text;
using System.Management;
using System.Threading;

namespace Warewolf.Launcher
{
    public class ContainerOps
    {
        readonly string _remoteDockerApi;
        string _remoteContainerID = null;
        string _remoteImageID = null;
        public string hostname;

        public ContainerOps(string remoteDockerApi = "test-load")
        {
            _remoteDockerApi = remoteDockerApi;
            hostname = StartRemoteContainer();
        }

        public string StartRemoteContainer()
        {
            Build(GetServerPath());
            CreateContainer();
            StartContainer();
            return GetContainerHostname();
        }

        public void DeleteRemoteContainer()
        {
            if (_remoteContainerID != null)
            {
                RecoverServerLogFile();
                StopContainer();
                DeleteContainer();
            }
            if (_remoteImageID != null)
            {
                Delete();
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
                        Thread.Sleep(300000);
                        Console.Write("Started Remote Warewolf Server. " + reader.ReadToEnd());
                    }
                }
            }
        }

        void CreateContainer()
        {
            var url = "http://" + _remoteDockerApi + ":2375/containers/create";
            HttpContent containerContent = new StringContent(@"
{
     ""Image"":""" + _remoteImageID + @"""
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
                        _remoteContainerID = ParseForContainerID(reader.ReadToEnd());
                    }
                }
            }
        }

        void Build(string serverPath)
        {
            byte[] paramFileBytes = CreateTarGZ(serverPath);
            var url = "http://" + _remoteDockerApi + ":2375/build";
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
                        throw new HttpRequestException("Error building remote server image. " + reader.ReadToEnd());
                    }
                    else
                    {
                        _remoteImageID = ParseForImageID(reader.ReadToEnd());
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
                throw new HttpRequestException("Error parsing for image ID. " + responseText);
            }
        }

        string ParseForContainerID(string responseText)
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

        byte[] CreateTarGZ(string sourceDirectory)
        {
            string tempTarFilePath = GetTempTarFilePath();
            Stream outStream = File.Create(tempTarFilePath);
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
            return File.ReadAllBytes(tempTarFilePath);
        }

        private static string GetTempTarFilePath()
        {
            var TempDirPath = Environment.ExpandEnvironmentVariables("%TEMP%");
            string tempTarFilePath = "";
            if (TempDirPath != "")
            {
                tempTarFilePath = Path.Combine(TempDirPath, "gzip-server.tar.gz");
            }
            else
            {
                tempTarFilePath = @"c:\temp\gzip-server.tar.gz";
            }
            if (File.Exists(tempTarFilePath))
            {
                File.Delete(tempTarFilePath);
            }

            return tempTarFilePath;
        }

        void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
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

        string GetServerPath()
        {
            String[] properties = { "Name", "ExecutablePath" };
            SelectQuery s = new SelectQuery("Win32_Process",
               "Name = 'Warewolf Server.exe' ",
               properties);
            ManagementObjectSearcher searcher =
               new ManagementObjectSearcher(s);
            ManagementObjectCollection objCollection = searcher.Get();
            if (objCollection.Count <= 0)
            {
                throw new Exception("Warewolf Server is not running.");
            }
            string serverFilePath = "";
            foreach (ManagementBaseObject obj in objCollection)
            {
                serverFilePath = obj["ExecutablePath"].ToString();
                break;
            }
            return Path.GetDirectoryName(serverFilePath);
        }
    }
}
