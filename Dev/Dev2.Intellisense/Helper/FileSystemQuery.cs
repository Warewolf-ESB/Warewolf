#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Intellisense.Helper
{
    public class FileSystemQuery:IFileSystemQuery
    {
        const char SlashC = '\\';

        [NonSerialized]
        List<string> _queryCollection;

        List<string> _computerNameCache = new List<string>();
        DateTime _gotComputerNamesLast;
        readonly TimeSpan _networkQueryTime  = new TimeSpan(0,0,15,0);
        readonly IDirectory _directory;
        readonly IDirectoryEntryFactory _directoryEntryFactory;
        readonly IShareCollectionFactory _shareCollectionFactory;

        public FileSystemQuery(IDirectory directory, IDirectoryEntryFactory directoryEntryFactory, IShareCollectionFactory shareCollectionFactory)
        {
            VerifyArgument.IsNotNull("Directory",directory);
            VerifyArgument.IsNotNull("ShareCollectionFactory", shareCollectionFactory);
            _directory = directory;
            _directoryEntryFactory = directoryEntryFactory;
            _shareCollectionFactory = shareCollectionFactory;
        }

        public FileSystemQuery()
        {
            _directory = new DirectoryWrapper();
            _shareCollectionFactory = new ShareCollectionFactory();
        }

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

        public IDirectory Directory => _directory;

        public void QueryList(string searchPath)
        {
            var directorySeparatorChar = Path.DirectorySeparatorChar;

            var queryCollection = new List<string>();
            queryCollection = (searchPath ?? string.Empty).Length <= 1 
                              ? new List<string>(Directory.GetLogicalDrives()) 
                              : SearchForFileAndFolders(searchPath, queryCollection, directorySeparatorChar);
            QueryCollection = queryCollection;
        }

      public  List<string> SearchForFileAndFolders(string searchPath, List<string> queryCollection, char directorySeparatorChar)
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

      public  List<string> GetComputerNamesOnNetwork(string searchPath, List<string> queryCollection)
        {
            if (searchPath != null && searchPath.Contains("\\"))
            {
                if (_computerNameCache.Count == 0 || DateTime.Now.Subtract(_gotComputerNamesLast) > _networkQueryTime)
                {
                    _computerNameCache = FindNetworkComputers();
                    _gotComputerNamesLast = DateTime.Now;
                }
                queryCollection = _computerNameCache;
            }
            return queryCollection;
        }

        public List<string> GetFilesAndFoldersFromDrive(string searchPath, List<string> queryCollection, char directorySeparatorChar)
        {
            queryCollection = searchPath != null && searchPath[1] == ':' ? new List<string>(Directory.GetFileSystemEntries(searchPath + directorySeparatorChar)) : queryCollection;
            return queryCollection;
        }

        public List<string> GetAllFilesAndFolders(string searchPath, List<string> queryCollection, char directorySeparatorChar)
        {
            var bQueryUncShares = false;
            var sFileServer = string.Empty;
            if (String.IsNullOrEmpty(searchPath))
            {
                return new List<string>();
            }

            if (searchPath.Length > 3)
            {
                bQueryUncShares = GetServerNameFromInput(searchPath, ref queryCollection, ref sFileServer);
            }
            if (bQueryUncShares)
            {
                GetSharesInformationFromSpecifiedServer(sFileServer, queryCollection);
            }
            else
            {
                if (queryCollection.Count == 0)
                {
                    queryCollection = GetFilesAndFoldersIncludingNetwork(searchPath, queryCollection, directorySeparatorChar);
                }
            }
            return queryCollection;
        }

        public bool GetServerNameFromInput(string searchPath, ref List<string> queryCollection, ref string sFileServer)
        {
            var fileServer = searchPath.Substring(2, searchPath.Length - 3);
            var c = searchPath[searchPath.Length - 1];
            var bQueryUncShares =false;
            if (searchPath[0] == SlashC && searchPath[1] == SlashC && c == SlashC && !fileServer.Contains("\\"))
            {
                bQueryUncShares = true;
                sFileServer = fileServer;
            }
            else
            {
                if (searchPath[0] == SlashC && searchPath[1] == SlashC && c != SlashC && !fileServer.Contains("\\"))
                {
                    fileServer = searchPath.Substring(2);
                    if (_computerNameCache.Count == 0)
                    {
                        GetComputerNamesOnNetwork("\\", queryCollection);
                    }
                    queryCollection = _computerNameCache.Where(s => s.ToLower().Contains(fileServer.ToLower())).ToList();
                }
            }
            return bQueryUncShares;
        }

        public void GetSharesInformationFromSpecifiedServer(string sFileServer, List<string> queryCollection)
        {
            var sc = _shareCollectionFactory.CreateShareCollection(sFileServer);

            if(sc.Count > 0)
            {
                queryCollection.AddRange(from Share s in sc
                                         where s.IsFileSystem
                                         select s.ToString());
            }
        }

        public List<string> GetFilesAndFoldersIncludingNetwork(string searchPath, List<string> queryCollection, char directorySeparatorChar)
        {
            if (GetServerFolderShare(searchPath, out string sServerFolderShare))
            {
                queryCollection.Add(sServerFolderShare);
            }

            queryCollection.AddRange( GetFoldersAndFiles(searchPath, directorySeparatorChar,Directory));
            return queryCollection;
        }

        public static List<string> GetFoldersAndFiles(string searchPath, char directorySeparatorChar,IDirectory dir)
        {
            VerifyArgument.IsNotNull("Directory", dir);
       

            var queryCollection = new List<string>();
            if (searchPath != null && dir.Exists(searchPath))
            {
                queryCollection = new List<string>(dir.GetFileSystemEntries(searchPath));
            }
            else
            {
                if(searchPath != null)
                {
                    queryCollection = GetFilesListing(searchPath, directorySeparatorChar,dir);
                }
            }
            return queryCollection;
        }

        public static List<string> GetFilesListing(string searchPath, char directorySeparatorChar, IDirectory dir)
        {
            VerifyArgument.IsNotNull("Directory",dir);

            var lastIndexOfDirSepChar = searchPath.LastIndexOf(directorySeparatorChar);
            var queryCollection = new List<string>(); 
            if(lastIndexOfDirSepChar > 0)
            {
                var parentDir = searchPath.Substring(0, lastIndexOfDirSepChar + 1);
                var searchPattern = string.Format("*{0}*", searchPath.Substring(lastIndexOfDirSepChar + 1));
                if (dir.Exists(parentDir))
                {
                    queryCollection = new List<string>(dir.GetFileSystemEntries(parentDir, searchPattern));
                }

            }
            return queryCollection;
        }

        public List<string> FindNetworkComputers()
        {
            var root =  _directoryEntryFactory.Create( "WinNT:");
            return (from IDirectoryEntry dom in root.Children
                    from IDirectoryEntry entry in dom.Children
                    where entry.SchemaClassName == "Computer"
                    select @"\\"+entry.Name).ToList();
        }

        public bool GetServerFolderShare(string sInPath, out string sServerFolderShare)
        {
            sServerFolderShare = string.Empty;
            const char cPathDel = SlashC;

            if(sInPath == null)
            {
                return false;
            }

            if(sInPath.Length <= 8)
            {
                return false;
            }

            if(sInPath[0] != cPathDel || sInPath[1] != cPathDel)
            {
                return false;
            }

            int environmentModel;
            int iShare;

            if((environmentModel = sInPath.IndexOf(cPathDel, 2)) == -1) //somewhere in hell
            {
                return false;
            }

            if((iShare = sInPath.IndexOf(cPathDel, environmentModel + 1)) == -1)
            {
                if(Directory.Exists(sInPath))
                {
                    sServerFolderShare = sInPath.ToUpper() + cPathDel;
                    return true;
                }
                return false;
            }

            sServerFolderShare = sInPath.ToUpper().Substring(0, iShare + 1);
            return true;
        }
    }
}
