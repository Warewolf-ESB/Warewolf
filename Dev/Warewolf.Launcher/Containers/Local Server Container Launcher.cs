using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Net.Http;
using System.Text;
using System.Management;

namespace Warewolf.Launcher
{
    public class LocalServerContainerLauncher
    {
        public ContainerLauncher launcher;

        public LocalServerContainerLauncher(string hostname = "", string remoteDockerApi = "localhost")
        {
            launcher = new ContainerLauncher(hostname, remoteDockerApi);
            launcher._hostname = StartServerContainer(hostname);
        }

        public string StartServerContainer(string hostname = "")
        {
            Build(GetServerPath());
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

        public void DeleteRemoteContainer()
        {
            if (launcher._remoteContainerID != null)
            {
                launcher.StopContainer();
                RecoverServerLogFile();
                launcher.DeleteContainer();
            }
            if (launcher._remoteImageID != null)
            {
                launcher.Delete();
            }
        }

        void RecoverServerLogFile()
        {
            var url = "http://" + launcher._remoteDockerApi + ":2375/containers/" + launcher._remoteContainerID + "/archive?path=C%3A%5CProgramData%5CWarewolf%5CServer+Log%5Cwarewolf-server.log";
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

        public void Build(string serverPath)
        {
            byte[] paramFileBytes = CreateTarGZ(serverPath);
            var url = "http://" + launcher._remoteDockerApi + ":2375/build";
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
                        launcher._remoteImageID = launcher.ParseForImageID(reader.ReadToEnd());
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

        public static string GetServerPath()
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
