
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;

namespace Dev2.InterfaceImplementors
{
    public class FileSystemQuery : IFileSystemQuery
    {
        [NonSerialized]
        List<string> _queryCollection;

        List<string> _computerNameCache = new List<string>();
        DateTime _gotComputerNamesLast;
        readonly TimeSpan _networkQueryTime = new TimeSpan(0, 0, 15, 0);

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
            if(String.IsNullOrEmpty(searchPath)) return new List<string>();
            if(searchPath.Length > 3)
            {
                bQueryUncShares = GetServerNameFromInput(searchPath, ref queryCollection, ref sFileServer);
            }
            if(bQueryUncShares)
            {
                GetSharesInformationFromSpecifiedServer(sFileServer, queryCollection);
            }
            else if(queryCollection.Count == 0)
            {
                queryCollection = GetFilesAndFoldersIncludingNetwork(searchPath, queryCollection, directorySeparatorChar);
            }
            return queryCollection;
        }

        bool GetServerNameFromInput(string searchPath, ref List<string> queryCollection, ref string sFileServer)
        {
            var fileServer = searchPath.Substring(2, searchPath.Length - 3);
            var c = searchPath[searchPath.Length - 1];
            bool bQueryUncShares = false;
            if(searchPath[0] == '\\' && searchPath[1] == '\\' && c == '\\' && !fileServer.Contains("\\"))
            {
                bQueryUncShares = true;
                sFileServer = fileServer;
            }
            else if(searchPath[0] == '\\' && searchPath[1] == '\\' && c != '\\' && !fileServer.Contains("\\"))
            {
                fileServer = searchPath.Substring(2);
                if(_computerNameCache.Count == 0)
                {
                    GetComputerNamesOnNetwork("\\", queryCollection);
                }
                queryCollection = _computerNameCache.Where(s => s.ToLower().Contains(fileServer.ToLower())).ToList();
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
                queryCollection = Directory.Exists(parentDir) ? new List<string>(Directory.GetFileSystemEntries(parentDir, searchPattern)) : new List<string>();
            }
            return queryCollection;
        }

        List<string> FindNetworkComputers()
        {
            var root = new DirectoryEntry("WinNT:");
            return (from DirectoryEntry dom in root.Children
                    from DirectoryEntry entry in dom.Children
                    where entry.SchemaClassName == "Computer"
                    select @"\\" + entry.Name).ToList();
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

            int environmentModel;
            int iShare;

            if((environmentModel = sInPath.IndexOf(CPathDel, 2)) == -1)
            {
                return false;
            }

            if((iShare = sInPath.IndexOf(CPathDel, environmentModel + 1)) == -1)
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
