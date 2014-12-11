
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
using System.Linq;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Intellisense.Helper
{
    public class FileSystemQuery:IFileSystemQuery
    {
        private const char SlashC = '\\';

        [NonSerialized]
        List<string> _queryCollection;

        List<string> _computerNameCache = new List<string>();
        DateTime _gotComputerNamesLast;
        readonly TimeSpan _networkQueryTime  = new TimeSpan(0,0,15,0);
        private readonly IDirectory _directory;
        private readonly IDirectoryEntryFactory _directoryEntryFactory;
        private readonly IShareCollectionFactory _shareCollectionFactory;

        public FileSystemQuery(IDirectory directory,IDirectoryEntryFactory directoryEntryFactory,IShareCollectionFactory shareCollectionFactory)
        {
            VerifyArgument.IsNotNull("Directory",directory);
            VerifyArgument.IsNotNull("DirectoryEntryFactory", directoryEntryFactory);
            VerifyArgument.IsNotNull("ShareCollectionFactory", shareCollectionFactory);
            _directory = directory;
            _directoryEntryFactory = directoryEntryFactory;
            _shareCollectionFactory = shareCollectionFactory;
        }
        public FileSystemQuery()
        {
            _directory = new DirectoryWrapper();
            _shareCollectionFactory = new ShareCollectionFactory();
            _directoryEntryFactory = new DirectoryEntryFactory();
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

        public IDirectory Directory
        {
            get { return _directory; }            
        }

        public void QueryList(string searchPath)
        {
            var directorySeparatorChar = System.IO.Path.DirectorySeparatorChar;

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
            else if(queryCollection.Count==0)
            {
                queryCollection = GetFilesAndFoldersIncludingNetwork(searchPath, queryCollection, directorySeparatorChar);
            }
            return queryCollection;
        }

        public bool GetServerNameFromInput(string searchPath, ref List<string> queryCollection, ref string sFileServer)
        {
            var fileServer = searchPath.Substring(2, searchPath.Length - 3);
            var c = searchPath[searchPath.Length - 1];
            bool bQueryUncShares =false;
            if(searchPath[0] == SlashC && searchPath[1] == SlashC && c == SlashC && !fileServer.Contains("\\"))
            {
                bQueryUncShares = true;
                sFileServer = fileServer;
            }
            else if (searchPath[0] == SlashC && searchPath[1] == SlashC && c != SlashC && !fileServer.Contains("\\"))
            {
                fileServer = searchPath.Substring(2);
                if(_computerNameCache.Count == 0)
                {
                    GetComputerNamesOnNetwork("\\",queryCollection);
                }
                queryCollection = _computerNameCache.Where(s => s.ToLower().Contains(fileServer.ToLower())).ToList();
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
            string sServerFolderShare;
            if(GetServerFolderShare(searchPath, out sServerFolderShare))
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

            int lastIndexOfDirSepChar = searchPath.LastIndexOf(directorySeparatorChar);
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
