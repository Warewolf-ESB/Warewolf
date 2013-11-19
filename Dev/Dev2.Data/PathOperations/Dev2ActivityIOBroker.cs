using System.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Binary_Objects;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dev2.PathOperations
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a concrete impl of the IActivityOperationBroker to facilitate IO operations
    /// </summary>
    internal class Dev2ActivityIOBroker : IActivityOperationsBroker
    {
        private const string resultOk = "Success";
        private const string resultBad = "Failure";

        // See interfaces summary's for more detail

        public string Get(IActivityIOOperationsEndPoint path, bool deferredRead = false)
        {

            if(!deferredRead)
            {
                byte[] bytes;
                using(Stream s = path.Get(path.IOPath))
                {
                    bytes = new byte[s.Length];
                    s.Position = 0;
                    s.Read(bytes, 0, (int)s.Length);
                    s.Close();
                    s.Dispose();
                }

                return Encoding.UTF8.GetString(bytes);
            }
            else
            {
                // Travis.Frisinger - 01.02.2013 : Bug 8579
                // If we want to deferr the read of data, just return the file name ;)

                // Serialize to binary and return 
                BinaryDataListUtil bdlUtil = new BinaryDataListUtil();
                return bdlUtil.SerializeDeferredItem(path);

            }
        }

        public Stream GetRaw(IActivityIOOperationsEndPoint path)
        {
            return path.Get(path.IOPath);
        }

        public string PutRaw(IActivityIOOperationsEndPoint dst, Dev2PutRawOperationTO args)
        {

            Stream s;
            string tmp = CreateTmpFile();

            // directory put?
            // wild char put?

            if(dst.RequiresLocalTmpStorage())
            {
                switch(args.WriteType)
                {
                    case WriteType.AppendBottom:
                        using(s = dst.Get(dst.IOPath))
                        {
                            File.WriteAllBytes(tmp, s.ToByteArray());
                            File.AppendAllText(tmp, args.FileContents);
                            s.Close();
                            s.Dispose();
                        }
                        break;
                    case WriteType.AppendTop:
                        File.WriteAllText(tmp, args.FileContents);
                        AppendToTemp(dst.Get(dst.IOPath), tmp);
                        break;
                    default:
                        if(IsBase64(args.FileContents))
                        {
                            byte[] data = Convert.FromBase64String(args.FileContents.Replace("Content-Type:BASE64", ""));
                            File.WriteAllBytes(tmp, data);
                        }
                        else
                        {
                            File.WriteAllText(tmp, args.FileContents);
                        }
                        break;
                }
            }
            else
            {
                // we can write directly to the file
                switch(args.WriteType)
                {
                    case WriteType.AppendBottom:
                        File.WriteAllText(tmp, File.ReadAllText(dst.IOPath.Path));
                        File.WriteAllText(tmp, args.FileContents);
                        break;
                    case WriteType.AppendTop:
                        File.WriteAllText(tmp, args.FileContents);
                        AppendToTemp(dst.Get(dst.IOPath), tmp);
                        break;
                    default:
                        if(IsBase64(args.FileContents))
                        {
                            byte[] data = Convert.FromBase64String(args.FileContents.Replace("Content-Type:BASE64", ""));
                            File.WriteAllBytes(tmp, data);
                        }
                        else
                        {
                            File.WriteAllText(tmp, args.FileContents);
                        }
                        break;
                }
            }

            string result = resultOk;

            using(s = new MemoryStream(File.ReadAllBytes(tmp)))
            {
                Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(true);

                //MO : 22-05-2012 : If the file doesnt exisit then create the file
                if(!dst.PathExist(dst.IOPath))
                {
                    CreateEndPoint(dst, newArgs, true);
                }
                if(dst.Put(s, dst.IOPath, newArgs, null) < 0)
                {
                    result = resultBad;
                }
                s.Close();
                s.Dispose();
            }

            RemoveTmpFile(tmp);

            return result;

        }

        static void AppendToTemp(Stream originalFileStream, string temp)
        {
            const int BufferSize = 1024 * 1024;
            var buffer = new char[BufferSize];

            using(var writer = new StreamWriter(temp, true))
            {
                using(var reader = new StreamReader(originalFileStream))
                {
                    int bytesRead;
                    while((bytesRead = reader.ReadBlock(buffer, 0, BufferSize)) != 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public string Delete(IActivityIOOperationsEndPoint src)
        {
            string result = resultOk;

            if(!src.Delete(src.IOPath))
            {
                result = resultBad;
            }

            return result;
        }

        public IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src, ReadTypes readTypes)
        {
            if(readTypes == ReadTypes.FilesAndFolders)
            {
                return src.ListDirectory(src.IOPath);
            }
            if(readTypes == ReadTypes.Files)
            {
                return src.ListFilesInDirectory(src.IOPath);
            }
            return src.ListFoldersInDirectory(src.IOPath);
        }

        public string CreateChildrenDirectoriesAtDestination(IActivityIOOperationsEndPoint dst, IActivityIOOperationsEndPoint src, Dev2CRUDOperationTO args)
        {
            IEnumerable<string> activityIOPaths = src.ListDirectory(src.IOPath).Where(path => src.PathIs(path) == enPathType.Directory).Select(path => path.Path.Replace(src.IOPath.Path, ""));
            string origDstPath = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(dst.IOPath.Path);
            foreach(var activityIOPath in activityIOPaths)
            {
                IActivityIOPath pathFromString = ActivityIOFactory.CreatePathFromString(origDstPath + activityIOPath, dst.IOPath.Username, dst.IOPath.Password);
                dst.CreateDirectory(pathFromString, args);
            }

            return "";
        }

        public string CreateEndPoint(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
        {

            string result = resultOk;

            // check the the dir strucutre exist
            var activityIOPath = dst.IOPath;
            IList<string> dirParts = MakeDirectoryParts(activityIOPath, dst.PathSeperator());

            // check from lowest path part up
            int deepestIndex = -1;
            int startDepth = (dirParts.Count - 1);

            int pos = startDepth;

            while(pos >= 0 && deepestIndex == -1)
            {
                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(dirParts[pos], activityIOPath.Username, activityIOPath.Password);
                try
                {
                    if(dst.ListDirectory(tmpPath) != null)
                    {
                        deepestIndex = pos;
                    }
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    // nothing to do, we swallow to keep going and find the deepest index a directory does exist at
                }
                pos--;
            }

            // now create all the directories we need ;)
            pos = (deepestIndex + 1);
            bool ok = true;

            IActivityIOPath origPath = dst.IOPath;

            while(pos <= startDepth && ok)
            {
                IActivityIOPath toCreate = ActivityIOFactory.CreatePathFromString(dirParts[pos], dst.IOPath.Username, dst.IOPath.Password);
                dst.IOPath = toCreate;
                ok = CreateDirectory(dst, args);
                pos++;
            }

            dst.IOPath = origPath;

            // dir create failed
            if(!ok)
            {
                result = resultBad;
            }
            else if(dst.PathIs(dst.IOPath) == enPathType.File && createToFile && ok)
            {
                if(!CreateFile(dst, args))
                {
                    result = resultBad;
                }
            }

            return result;
        }

        public string Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                           Dev2CRUDOperationTO args)
        {
            return ValidateRenameSourceAndDesinationTypes(src, dst, args);
        }


        public string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                           Dev2CRUDOperationTO args)
        {
            return ValidateCopySourceDestinationFileOperation(src, dst, args, () =>
                {
                    if(src.RequiresLocalTmpStorage())
                    {
                        if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                        {
                            dst.IOPath.Path = Path.Combine(dst.IOPath.Path, GetFileNameFromEndPoint(src));
                        }

                        using(Stream s = src.Get(src.IOPath))
                        {
                            dst.Put(s, dst.IOPath, args, Path.IsPathRooted(src.IOPath.Path) ? new FileInfo(src.IOPath.Path).Directory : null);
                            s.Close();
                            s.Dispose();
                        }
                    }
                    else
                    {
                        var sourceFile = new FileInfo(src.IOPath.Path);
                        if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                        {
                            dst.IOPath.Path = Path.Combine(dst.IOPath.Path, sourceFile.Name);
                        }

                        using(Stream s = src.Get(src.IOPath))
                        {
                            dst.Put(s, dst.IOPath, args, sourceFile.Directory);
                            s.Close();
                            s.Dispose();
                        }
                    }
                    return resultOk;
                });

        }

        public string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                           Dev2CRUDOperationTO args)
        {
            string result = Copy(src, dst, args);

            if (result.Equals("Success"))
            {
                src.Delete(src.IOPath);
            }

            return result;
        }

        private string ValidateRenameSourceAndDesinationTypes(IActivityIOOperationsEndPoint src,
                                                              IActivityIOOperationsEndPoint dst,
                                                              Dev2CRUDOperationTO args)
        {
            //ensures that the source and destination locations are of the same type
            if (src.PathIs(src.IOPath) != dst.PathIs(dst.IOPath))
            {
                throw new Exception("Source and destination need to be both files or directories");
            }

            //Rename Tool if the file/folder exists then delete it and put the source there
            if (dst.PathExist(dst.IOPath))
            {
                if (!args.Overwrite)
                {
                    throw new Exception("Destination directory already exists and overwrite is set to false");
                }

                //Clear the existing folder
                dst.Delete(dst.IOPath);
            }

            //ensures destination folder structure exists
            string opStatus = CreateEndPoint(dst, args, dst.PathIs(dst.IOPath) == enPathType.Directory);
            if (!opStatus.Equals("Success"))
            {
                throw new Exception("Recursive Directory Create Failed For [ " + dst.IOPath.Path + " ]");
            }

            return Move(src, dst, args);
        }

        private string ValidateCopySourceDestinationFileOperation(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                                        Dev2CRUDOperationTO args, Func<string> performAfterValidation)
        {
            string result = resultOk;

            if (src.IOPath.Path.Equals(dst.IOPath.Path))
            {
                throw new Exception("Source and destination cannot be same");
            }

            if (dst.IOPath.Path.ToLower().Trim().Contains(src.IOPath.Path.ToLower().Trim()) && dst.PathIs(dst.IOPath) == enPathType.Directory)
            {
                throw new Exception("Destination directory cannot be a child of the source directory");
            }

            //ensures destination folder structure exists
            string opStatus = CreateEndPoint(dst, args, dst.PathIs(dst.IOPath) == enPathType.Directory);
            if(!opStatus.Equals("Success"))
            {
                throw new Exception("Recursive Directory Create Failed For [ " + dst.IOPath.Path + " ]");
            }

            //transfer contents to destination when the source is a directory
            if(src.PathIs(src.IOPath) == enPathType.Directory)
            {
                if(!TransferDirectoryContents(src, dst, args))
                {
                    result = resultBad;
                }
            }
            else
            {
                if(!args.Overwrite)
                {
                    EnsureFilesDontExists(src, dst);
                }

                if(!Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                {
                    return performAfterValidation();
                }

                // we have star wild cards to deal with
                if(!TransferDirectoryContents(src, dst, args))
                {
                    result = resultBad;
                }
            }

            return result;
        }

        private static void EnsureFilesDontExists(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
        {
            if(dst.PathExist(dst.IOPath))
            {
                // destination is a file
                if(dst.PathIs(dst.IOPath) == enPathType.File)
                {
                    throw new Exception(
                        "A file with the same name exists on the destination and overwrite is set to false");
                }

                //destination is a folder
                IList<IActivityIOPath> dstContents = dst.ListDirectory(dst.IOPath);
                var destinationFileNames = dstContents.Select(dstFile => GetFileNameFromEndPoint(dst, dstFile));
                var sourceFile = GetFileNameFromEndPoint(src);

                if(destinationFileNames.Contains(sourceFile))
                {
                    throw new Exception("The following file(s) exist in the destination folder and overwrite is set to false :- " + sourceFile);
                }
            }
        }

        private static string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint)
        {
            string pathSeperator = endPoint.PathSeperator();
            return endPoint.IOPath.Path.Split(pathSeperator.ToCharArray()).Last();
        }

        private static string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint, IActivityIOPath path)
        {
            string pathSeperator = endPoint.PathSeperator();
            return path.Path.Split(pathSeperator.ToCharArray()).Last();
        }

        public string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args)
        {
            // tmp zip file location
            string tmpZip = CreateTmpFile();

            if(src.PathIs(src.IOPath) == enPathType.Directory || Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
            {

                // tmp dir for files required
                string tmpDir = CreateTmpDirectory();
                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmpDir, src.IOPath.Username, src.IOPath.Password);
                IActivityIOOperationsEndPoint tmpEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                Dev2CRUDOperationTO myArgs = new Dev2CRUDOperationTO(true);
                // stage contents to local folder
                TransferDirectoryContents(src, tmpEndPoint, myArgs);

                // get directory contents
                IList<IActivityIOPath> toAdd = ListDirectory(tmpEndPoint, ReadTypes.FilesAndFolders);

                using(ZipFile zip = new ZipFile())
                {
                    // set password if exist
                    if(args.ArchivePassword != string.Empty)
                    {
                        zip.Password = args.ArchivePassword;
                    }

                    // compression ratio                    
                    zip.CompressionLevel = ExtractZipCompressionLevel(args.CompressionRatio);

                    // add all files to archive
                    foreach(IActivityIOPath p in toAdd)
                    {
                        zip.AddFile(p.Path, ".");
                    }
                    zip.Save(tmpZip);
                }

                // remove locally staged files
                DirectoryHelper.CleanUp(tmpDir);
            }
            else
            {
                // normal not wild char file && not directory
                string packFile = src.IOPath.Path;

                if(src.RequiresLocalTmpStorage())
                {
                    string tmpFile = CreateTmpFile();
                    packFile = tmpFile;
                    Stream s = src.Get(src.IOPath);
                    File.WriteAllBytes(tmpFile, s.ToByteArray());
                    s.Close();
                }

                using(ZipFile zip = new ZipFile())
                {
                    // set password if exist
                    if(args.ArchivePassword != string.Empty)
                    {
                        zip.Password = args.ArchivePassword;
                    }

                    // compression ratio
                    zip.CompressionLevel = ExtractZipCompressionLevel(args.CompressionRatio);

                    // add all files to archive
                    zip.AddFile(packFile, ".");

                    zip.Save(tmpZip);
                }

            }

            // now transfer the zip file to the correct location
            string result;
            using(Stream s2 = new MemoryStream(File.ReadAllBytes(tmpZip)))
            {

                // adjust so it has .zip on the end
                string zipLoc = dst.IOPath.Path;

                //MO : 22-08-2012 : If the user enters a full file path and no archive name 
                if(!string.IsNullOrEmpty(args.ArchiveName))
                {
                    if(!args.ArchiveName.EndsWith(".zip"))
                    {
                        zipLoc += dst.PathSeperator() + args.ArchiveName + ".zip";
                    }
                    else
                    {
                        zipLoc += dst.PathSeperator() + args.ArchiveName;
                    }
                }
                else
                {
                    if(!dst.IOPath.Path.EndsWith(".zip"))
                    {
                        zipLoc += ".zip";
                    }
                }

                // add archive name to path
                dst = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipLoc, dst.IOPath.Username, dst.IOPath.Password));

                Dev2CRUDOperationTO zipTransferArgs = new Dev2CRUDOperationTO(args.Overwrite);

                result = resultOk;

                if(IsNotFTPTypePath(src))
                {
                    var fileInfo = new FileInfo(src.IOPath.Path);
                    if(fileInfo.Directory != null && Path.IsPathRooted(fileInfo.Directory.ToString()))
                    {
                        if(dst.Put(s2, dst.IOPath, zipTransferArgs, fileInfo.Directory) < 0)
                        {
                            result = resultBad;
                        }
                    }
                    else
                    {
                        if(dst.Put(s2, dst.IOPath, zipTransferArgs, null) < 0)
                        {
                            result = resultBad;
                        }
                    }
                }
                else
                {
                    if(dst.Put(s2, dst.IOPath, zipTransferArgs, null) < 0)
                    {
                        result = resultBad;
                    }
                }

                // remove tmp directory and tmp zip
                RemoveTmpFile(tmpZip);
            }

            return result;
        }

        static bool IsNotFTPTypePath(IActivityIOOperationsEndPoint src)
        {
            return IsNotFTPTypePath(src.IOPath);
        }

        static bool IsNotFTPTypePath(IActivityIOPath src)
        {
            return !src.Path.StartsWith("ftp://") && !src.Path.StartsWith("ftps://") && !src.Path.StartsWith("sftp://");
        }

        static bool IsUncFileTypePath(IActivityIOPath src)
        {
            return src.Path.StartsWith(@"\\");
        }

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2UnZipOperationTO args)
        {
            string zipFile = src.IOPath.Path;
            ZipFile zip = null;
            if(src.RequiresLocalTmpStorage())
            {

                string tmpZip = CreateTmpFile();
                using(Stream s = src.Get(src.IOPath))
                {

                    File.WriteAllBytes(tmpZip, s.ToByteArray());
                    s.Close();
                    s.Dispose();
                }

                zipFile = tmpZip;
                zip = ZipFile.Read(zipFile);
            }
            else
            {
                zip = ZipFile.Read(src.Get(src.IOPath));
            }

            string unzipDirectory = dst.IOPath.Path;

            if(dst.RequiresLocalTmpStorage())
            {
                // unzip locally then Put the contents of the archive to the dst end-point
                unzipDirectory = CreateTmpDirectory();
            }

            // var stream = src.Get(src.IOPath);
            if(zip != null)
            {
                using(zip)
                {

                    if(!string.IsNullOrEmpty(args.ArchivePassword))
                    {
                        zip.Password = args.ArchivePassword;
                    }

                    foreach(ZipEntry ze in zip)
                    {
                        ze.Extract(unzipDirectory,
                            args.Overwrite ?
                            ExtractExistingFileAction.OverwriteSilently :
                            ExtractExistingFileAction.DoNotOverwrite);
                    }
                }

            }
            if(dst.RequiresLocalTmpStorage())
            {
                IActivityIOPath newSrc = ActivityIOFactory.CreatePathFromString(unzipDirectory, dst.IOPath.Username, dst.IOPath.Password);
                IActivityIOOperationsEndPoint newSrcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(newSrc);
                Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(false);
                Copy(newSrcEP, dst, newArgs);
                // local delete taken care of for us ;)
            }

            // clean up local tmp
            if(src.RequiresLocalTmpStorage())
            {
                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(zipFile, src.IOPath.Username, src.IOPath.Password);
                IActivityIOOperationsEndPoint tmpEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                Delete(tmpEP);
            }

            return resultOk;
        }

        #region Private Methods
        private bool IsBase64(string payload)
        {
            return payload.StartsWith("Content-Type:BASE64");
        }


        private IList<string> MakeDirectoryParts(IActivityIOPath path, string splitter)
        {
            string[] tmp;

            IList<string> result = new List<string>();

            char[] splitOn = splitter.ToCharArray();

            if(IsNotFTPTypePath(path))
            {
                tmp = path.Path.Split(splitOn);

            }
            else
            {
                var splitValues = path.Path.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                splitValues.RemoveAt(0);
                var newPath = string.Join("/", splitValues);
                tmp = newPath.Split(splitOn);
            }

            // remove trailing file entry if exist
            string candiate = tmp[tmp.Length - 1];
            int len = tmp.Length;
            if(candiate.Contains("*.") || candiate.Contains("."))
            {
                len = (tmp.Length - 1);
            }

            string builderPath = "";
            // build up URI parts from root down
            for(int i = 0; i < len; i++)
            {
                if(!string.IsNullOrWhiteSpace(tmp[i]))
                {
                    builderPath += tmp[i] + splitter;
                    if(!IsNotFTPTypePath(path) && !builderPath.Contains("://"))
                    {
                        var splitValues = path.Path.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        builderPath = splitValues[0] + "://" + builderPath;
                    }
                    result.Add(IsUncFileTypePath(path) ? @"\\" + builderPath : builderPath);
                }
            }
            return result;
        }

        private bool CreateDirectory(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            bool result = true;

            if(!dst.CreateDirectory(dst.IOPath, args))
            {
                result = false;
            }

            return result;
        }

        private bool CreateFile(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {

            bool result = true;

            String tmp = CreateTmpFile();
            using(Stream s = new MemoryStream(File.ReadAllBytes(tmp)))
            {

                if(dst.Put(s, dst.IOPath, args, null) < 0)
                {
                    result = false;
                }

                s.Close();
                RemoveTmpFile(tmp);
            }

            return result;
        }

        private Ionic.Zlib.CompressionLevel ExtractZipCompressionLevel(string lvl)
        {
            Array lvls = Enum.GetValues(typeof(Ionic.Zlib.CompressionLevel));
            int pos = 0;
            //19.09.2012: massimo.guerrera - Changed to default instead of none
            Ionic.Zlib.CompressionLevel clvl = Ionic.Zlib.CompressionLevel.Default;

            while(pos < lvls.Length && lvls.GetValue(pos).ToString() != lvl)
            {
                pos++;
            }

            if(pos < lvls.Length)
            {
                clvl = (Ionic.Zlib.CompressionLevel)lvls.GetValue(pos);
            }

            return clvl;
        }

        /// <summary>
        /// Transfer the contents of the directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        private bool TransferDirectoryContents(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            // List directory contents
            
            if (args.DoRecursiveCopy)
            {
                IList<IActivityIOPath> srcContentsFolders = src.ListFoldersInDirectory(src.IOPath);
                foreach (var sourcePath in srcContentsFolders)
                {
                    IActivityIOOperationsEndPoint sourceEndPoint =
                        ActivityIOFactory.CreateOperationEndPointFromIOPath(sourcePath);
                    IList<string> dirParts =
                        sourceEndPoint.IOPath.Path.Split(sourceEndPoint.PathSeperator().ToCharArray());
                    var directory = dirParts.Last();
                    IActivityIOPath destinationPath =
                        ActivityIOFactory.CreatePathFromString(Path.Combine(dst.IOPath.Path, directory),
                                                               dst.IOPath.Username,
                                                               dst.IOPath.Password);
                    IActivityIOOperationsEndPoint destinationEndPoint =
                        ActivityIOFactory.CreateOperationEndPointFromIOPath(destinationPath);
                    dst.CreateDirectory(destinationPath, args);
                    //}
                    Copy(sourceEndPoint, destinationEndPoint, args);
                }
            }
            
            IList<IActivityIOPath> srcContents = src.ListFilesInDirectory(src.IOPath);
            IList<IActivityIOPath> dstContents = dst.ListFilesInDirectory(dst.IOPath);

            if(!args.Overwrite)
            {
                var sourceFileNames = srcContents.Select(srcFile => GetFileNameFromEndPoint(src, srcFile)).ToList();
                var destinationFileNames = dstContents.Select(dstFile => GetFileNameFromEndPoint(dst, dstFile)).ToList();

                if(destinationFileNames.Count > 0)
                {
                    var commonFiles = sourceFileNames.Where(destinationFileNames.Contains).ToList();

                    if(commonFiles.Count > 0)
                    {
                        string fileNames = commonFiles.Aggregate("",
                                                                 (current, commonFile) =>
                                                                 current + ("\r\n" + commonFile));
                        throw new Exception("The following file(s) exist in the destination folder and overwrite is set to false:- " + fileNames);
                    }
                }
            }

            bool result = true;

            string origDstPath = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(dst.IOPath.Path);

            // Sashen: 22-08-2012 - Not entirely sure what the behaviour should be if a folder already exists
            // as it stands, we will create a new folder 
            // check if the directory we want to copy into exists

            if(!dst.PathExist(dst.IOPath))
            {
                CreateDirectory(dst, args);
            }

            // get each file, then put it to the correct location
            foreach(IActivityIOPath p in srcContents)
            {
                Stream s = null;
                try
                {
                    //  ensure file is not present already 
                    // Sashen : 22-08-2012 - This used to check if the directory was present, but instead should check
                    // if the files exists in a directory, the name of the directory does imply that only the contents
                    // should be moved
                    if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                    {

                        IActivityIOPath cpPath = ActivityIOFactory.CreatePathFromString(string.Format("{0}{1}{2}", origDstPath, dst.PathSeperator(), (Dev2ActivityIOPathUtils.ExtractFileName(p.Path))), dst.IOPath.Username, dst.IOPath.Password);
                        if(args.Overwrite || !dst.PathExist(cpPath))
                        {
                            using(s = src.Get(p))
                            {

                                // Need to ensure we have a file name on dst
                                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(cpPath.Path, dst.IOPath.Username, dst.IOPath.Password);
                                IActivityIOOperationsEndPoint tmpEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                                var whereToPut = GetWhereToPut(src, dst);

                                if(tmpEP.Put(s, tmpEP.IOPath, args, whereToPut) < 0)
                                {
                                    result = false;
                                }
                                s.Close();
                                s.Dispose();
                            }
                        }
                    }
                    else if(args.Overwrite || !dst.PathExist(dst.IOPath))
                    {
                        using(s = src.Get(p))
                        {

                            // Need to ensure we have a file name on dst
                            string tmp = origDstPath + "\\" + Dev2ActivityIOPathUtils.ExtractFileName(p.Path);

                            IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmp, dst.IOPath.Username, dst.IOPath.Password);
                            IActivityIOOperationsEndPoint tmpEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);

                            var whereToPut = GetWhereToPut(src, dst);

                            if(tmpEP.Put(s, tmpEP.IOPath, args, whereToPut) < 0)
                            {
                                result = false;
                            }
                            s.Close();
                            s.Dispose();
                        }
                    }
                }
                catch(Exception ex)
                {
                    if(s != null)
                    {
                        s.Close();
                    }
                    ServerLogger.LogError(ex);
                }
            }
            return result;
        }

        static DirectoryInfo GetWhereToPut(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
        {
            if(src.IOPath.PathType == enActivityIOPathType.FileSystem)
            {
                return new FileInfo(src.IOPath.Path).Directory;
            }
            if(dst.IOPath.PathType == enActivityIOPathType.FileSystem)
            {

                return new FileInfo(dst.IOPath.Path).Directory;
            }
            return null; // this means that neither the src or destination where local files
        }

        /// <summary>
        /// Creates a tmp file
        /// </summary>
        /// <returns></returns>
        private string CreateTmpFile()
        {
            string tmpFile = Path.GetTempFileName();
            return tmpFile;
        }

        /// <summary>
        /// Remove a tmp file
        /// </summary>
        /// <param name="path"></param>
        private void RemoveTmpFile(string path)
        {
            File.Delete(path);
        }

        private string CreateTmpDirectory()
        {
            string tmpDir = Path.GetTempPath();
            DirectoryInfo di = Directory.CreateDirectory(tmpDir + "\\" + Guid.NewGuid());

            return (di.FullName);
        }

        #endregion
    }
}
