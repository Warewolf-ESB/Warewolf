using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;

namespace Dev2.Studio.InterfaceImplementors
{
    public class FileSystemQuery:IFileSystemQuery
    {
        [NonSerialized]
        List<string> _queryCollection;

        List<string> _computerNameCache = new List<string>();
        DateTime _gotComputerNamesLast;
        readonly TimeSpan _networkQueryTime  = new TimeSpan(0,0,15,0);

        public List<string> QueryCollection
        {
            get
            {
                return _queryCollection;
            }
            private set
            {
                _queryCollection = value;
            }
        }

        public void QueryList(string searchPath)
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;

            var queryCollection = new List<string>();
            queryCollection = (searchPath ?? string.Empty).Length <= 1 
                              ? new List<string>(Directory.GetLogicalDrives()) 
                              : SearchForFileAndFolders(searchPath, queryCollection, directorySeparatorChar);
            QueryCollection = queryCollection;
        }

        List<string> SearchForFileAndFolders(string searchPath, List<string> queryCollection, char directorySeparatorChar)
        {
            if((searchPath ?? string.Empty).Length == 2)
            {
                queryCollection = GetFilesAndFoldersFromDrive(searchPath, queryCollection, directorySeparatorChar);
                queryCollection = GetComputerNamesOnNetwork(searchPath, queryCollection);
            }
            else
            {
                queryCollection = GetAllFilesAndFolders(searchPath, queryCollection, directorySeparatorChar);
            }
            return queryCollection;
        }

        List<string> GetComputerNamesOnNetwork(string searchPath, List<string> queryCollection)
        {
            if(searchPath != null && searchPath.Contains("\\"))
            {
                if(_computerNameCache.Count == 0 || DateTime.Now.Subtract(_gotComputerNamesLast) > _networkQueryTime)
                {
                    _computerNameCache = FindNetworkComputers();
                    _gotComputerNamesLast = DateTime.Now;
                }
                queryCollection = _computerNameCache;
            }
            return queryCollection;
        }

        static List<string> GetFilesAndFoldersFromDrive(string searchPath, List<string> queryCollection, char directorySeparatorChar)
        {
            queryCollection = searchPath != null && searchPath[1] == ':' ? new List<string>(Directory.GetFileSystemEntries(searchPath + directorySeparatorChar)) : queryCollection;
            return queryCollection;
        }

        List<string> GetAllFilesAndFolders(string searchPath, List<string> queryCollection, char directorySeparatorChar)
        {
            bool bQueryUncShares = false;
            string sFileServer = string.Empty;

            if(searchPath != null && searchPath.Length > 3)
            {
                bQueryUncShares = GetServerNameFromInput(searchPath, false, ref sFileServer);
            }
            if(bQueryUncShares)
            {
                GetSharesInformationFromSpecifiedServer(sFileServer, queryCollection);
            }
            else
            {
                queryCollection = GetFilesAndFoldersIncludingNetwork(searchPath, queryCollection, directorySeparatorChar);
            }
            return queryCollection;
        }

        static bool GetServerNameFromInput(string searchPath, bool bQueryUncShares, ref string sFileServer)
        {
            if(searchPath[0] == '\\' && searchPath[1] == '\\' && searchPath[searchPath.Length - 1] == '\\' &&
               searchPath.Substring(2, searchPath.Length - 3).Contains("\\") == false)
            {
                bQueryUncShares = true;
                sFileServer = searchPath.Substring(2, searchPath.Length - 3);
            }
            return bQueryUncShares;
        }

        static void GetSharesInformationFromSpecifiedServer(string sFileServer, List<string> queryCollection)
        {
            var sc = new ShareCollection(sFileServer);

            if(sc.Count > 0)
            {
                queryCollection.AddRange(from Share s in sc
                                         where s.IsFileSystem
                                         select s.ToString());
            }
        }

        List<string> GetFilesAndFoldersIncludingNetwork(string searchPath, List<string> queryCollection, char directorySeparatorChar)
        {
            string sServerFolderShare;
            if(GetServerFolderShare(searchPath, out sServerFolderShare))
            {
                queryCollection.Add(sServerFolderShare);
            }

            queryCollection = GetFoldersAndFiles(searchPath, queryCollection, directorySeparatorChar);
            return queryCollection;
        }

        static List<string> GetFoldersAndFiles(string searchPath, List<string> queryCollection, char directorySeparatorChar)
        {
            if(searchPath != null && Directory.Exists(searchPath))
            {
                queryCollection = new List<string>(Directory.GetFileSystemEntries(searchPath));
            }
            else
            {
                if(searchPath != null)
                {
                    queryCollection = GetFilesListing(searchPath, directorySeparatorChar, queryCollection);
                }
            }
            return queryCollection;
        }

        static List<string> GetFilesListing(string searchPath, char directorySeparatorChar, List<string> queryCollection)
        {
            int lastIndexOfDirSepChar = searchPath.LastIndexOf(directorySeparatorChar);

            if(lastIndexOfDirSepChar > 0)
            {
                string parentDir = searchPath.Substring(0, lastIndexOfDirSepChar + 1);
                string searchPattern = "*" + searchPath.Substring(lastIndexOfDirSepChar + 1) + "*";
                queryCollection = new List<string>(Directory.GetFileSystemEntries(parentDir, searchPattern));
            }
            return queryCollection;
        }

        List<string> FindNetworkComputers()
        {
            var root = new DirectoryEntry("WinNT:");
            return (from DirectoryEntry dom in root.Children
                    from DirectoryEntry entry in dom.Children
                    where entry.SchemaClassName == "Computer"
                    select @"\\"+entry.Name).ToList();
        }

        bool GetServerFolderShare(string sInPath, out string sServerFolderShare)
        {
            sServerFolderShare = string.Empty;
            const char CPathDel = '\\';

            if(sInPath == null)
            {
                return false;
            }

            if(sInPath.Length <= 8)
            {
                return false;
            }

            if(sInPath[0] != CPathDel || sInPath[1] != CPathDel)
            {
                return false;
            }

            int iServer;
            int iShare;

            if((iServer = sInPath.IndexOf(CPathDel, 2)) == -1)
            {
                return false;
            }

            if((iShare = sInPath.IndexOf(CPathDel, iServer + 1)) == -1)
            {
                if(Directory.Exists(sInPath))
                {
                    sServerFolderShare = sInPath.ToUpper() + CPathDel;
                    return true;
                }
                return false;
            }

            sServerFolderShare = sInPath.ToUpper().Substring(0, iShare + 1);
            return true;
        }
    }
}