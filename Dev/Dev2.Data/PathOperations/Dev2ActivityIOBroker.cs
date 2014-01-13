using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.PathOperations.Enums;
using Dev2.Data.PathOperations.Extension;
using Ionic.Zip;

// ReSharper disable CheckNamespace
namespace Dev2.PathOperations
// ReSharper restore CheckNamespace
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
                }

                return Encoding.UTF8.GetString(bytes);
            }

                // Travis.Frisinger - 01.02.2013 : Bug 8579
            // If we want to defer the read of data, just return the file name ;)

                // Serialize to binary and return 
                BinaryDataListUtil bdlUtil = new BinaryDataListUtil();
                return bdlUtil.SerializeDeferredItem(path);
            }

        public Stream GetRaw(IActivityIOOperationsEndPoint path)
        {
            return path.Get(path.IOPath);
        }

        public string PutRaw(IActivityIOOperationsEndPoint dst, Dev2PutRawOperationTO args)
        {
            string result = resultOk; 

            // directory put?
            // wild char put?

            if(dst.RequiresLocalTmpStorage())
            {
                string tmp = CreateTmpFile();
                switch(args.WriteType)
                {
                    case WriteType.AppendBottom:
                        using(Stream s = dst.Get(dst.IOPath))
                        {
                            File.WriteAllBytes(tmp, s.ToByteArray());
                            File.AppendAllText(tmp, args.FileContents);
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
                result = MoveTmpFileToDestination(dst, tmp, result);
            }
            else
            {
                if(File.Exists(dst.IOPath.Path))
                {

                    string tmp = CreateTmpFile();                    
                    switch(args.WriteType)
                    {
                        case WriteType.AppendBottom:
                            File.AppendAllText(dst.IOPath.Path, args.FileContents);
                            result = resultOk;
                            break;
                        case WriteType.AppendTop:
                            File.WriteAllText(tmp, args.FileContents);
                            AppendToTemp(dst.Get(dst.IOPath), tmp);
                            result = MoveTmpFileToDestination(dst, tmp, result);
                            RemoveTmpFile(tmp);
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
                    result = MoveTmpFileToDestination(dst, tmp, result);
                    RemoveTmpFile(tmp);
                            break;
                }
                }
                else
                {
                    // we can write directly to the file
                    Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(true);

                    CreateEndPoint(dst, newArgs, true);

                    if(IsBase64(args.FileContents))
                    {
                        byte[] data = Convert.FromBase64String(args.FileContents.Replace("Content-Type:BASE64", ""));
                        File.WriteAllBytes(dst.IOPath.Path, data);
                    }
                    else
                    {
                        File.WriteAllText(dst.IOPath.Path, args.FileContents);
                    }
                }
            }                       

            return result;
        }

        string MoveTmpFileToDestination(IActivityIOOperationsEndPoint dst, string tmp, string result)
        {
            Stream s;
            using(s = new MemoryStream(File.ReadAllBytes(tmp)))
            {
                Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(true);

                //MO : 22-05-2012 : If the file doesnt exist then create the file
                if(!dst.PathExist(dst.IOPath))
                {
                    CreateEndPoint(dst, newArgs, true);
                }
                if(dst.Put(s, dst.IOPath, newArgs, null) < 0)
                {
                    result = resultBad;
                }
            }
            return result;
        }

        private static void AppendToTemp(Stream originalFileStream, string temp)
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

        public string Create(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
        {
            ValidateEndPoint(dst, args);
            return CreateEndPoint(dst, args, createToFile);
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
                            dst.IOPath.Path = dst.Combine(GetFileNameFromEndPoint(src));
                        }

                        using(Stream s = src.Get(src.IOPath))
                        {
                            dst.Put(s, dst.IOPath, args,
                                    Path.IsPathRooted(src.IOPath.Path) ? new FileInfo(src.IOPath.Path).Directory : null);
                            s.Close();
                            s.Dispose();
                        }
                    }
                    else
                    {
                        var sourceFile = new FileInfo(src.IOPath.Path);
                        if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                        {
                            dst.IOPath.Path = dst.Combine(sourceFile.Name);
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

            if(result.Equals("Success"))
            {
                src.Delete(src.IOPath);
            }

            return result;
        }

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                            Dev2UnZipOperationTO args)
        {
            return ValidateUnzipSourceDestinationFileOperation(src, dst, args, () =>
                {
                    ZipFile zip;
                    string tempFile = "";

                    if(src.RequiresLocalTmpStorage())
                    {

                        var tmpZip = CreateTmpFile();
                        using(Stream s = src.Get(src.IOPath))
                        {

                            File.WriteAllBytes(tmpZip, s.ToByteArray());
                            s.Close();
                            s.Dispose();
                        }

                        tempFile = tmpZip;
                        zip = ZipFile.Read(tempFile);
                    }
                    else
                    {
                        zip = ZipFile.Read(src.Get(src.IOPath));
                    }

                    if(dst.RequiresLocalTmpStorage())
                    {
                        // unzip locally then Put the contents of the archive to the dst end-point
                        var tempPath = CreateTmpDirectory();
                        ExtractFile(args, zip, tempPath);
                        IActivityIOPath endPointPath = ActivityIOFactory.CreatePathFromString(tempPath, "", "");
                        IActivityIOOperationsEndPoint endPoint =
                            ActivityIOFactory.CreateOperationEndPointFromIOPath(endPointPath);
                        Move(endPoint, dst, new Dev2CRUDOperationTO(args.Overwrite));
                    }
                    else
                    {
                        ExtractFile(args, zip, dst.IOPath.Path);
                    }

                    if(src.RequiresLocalTmpStorage())
                    {
                        File.Delete(tempFile);
                    }

                    return resultOk;
                });
        }

        private static void ExtractFile(Dev2UnZipOperationTO args, ZipFile zip, string extractFromPath)
        {
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
                        try
                        {
                            ze.Extract(extractFromPath,
                                       args.Overwrite
                                           ? ExtractExistingFileAction.OverwriteSilently
                                           : ExtractExistingFileAction.DoNotOverwrite);
                        }
                        catch(BadPasswordException bpe)
                        {
                            throw new Exception("Invalid archive password", bpe);
                        }
                    }
                }
            }
        }

        #region Private Methods

        private string CreateEndPoint(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
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
                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(dirParts[pos], activityIOPath.Username,
                                                                                 activityIOPath.Password, true);
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
                IActivityIOPath toCreate = ActivityIOFactory.CreatePathFromString(dirParts[pos], dst.IOPath.Username,
                                                                                  dst.IOPath.Password, true);
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
            else if(dst.PathIs(dst.IOPath) == enPathType.File && createToFile)
            {
                if(!CreateFile(dst, args))
                {
                    result = resultBad;
                }
            }

            return result;
        }

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
        private bool TransferDirectoryContents(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst,
                                               Dev2CRUDOperationTO args)
        {
            ValidateSourceAndDestinationContents(src, dst, args);
           
            if(args.DoRecursiveCopy)
            {
                try
                {
                    // List directory contents
                    IList<IActivityIOPath> srcContentsFolders = src.ListFoldersInDirectory(src.IOPath);
                    Task.WaitAll(srcContentsFolders.Select(sourcePath => Task.Run(() =>
                        {
                            IActivityIOOperationsEndPoint sourceEndPoint =
                                ActivityIOFactory.CreateOperationEndPointFromIOPath(sourcePath);
                            IList<string> dirParts =
                                sourceEndPoint.IOPath.Path.Split(sourceEndPoint.PathSeperator().ToCharArray(),
                                                                 StringSplitOptions.RemoveEmptyEntries);
                            IActivityIOPath destinationPath =
                                ActivityIOFactory.CreatePathFromString(dst.Combine(dirParts.Last()), dst.IOPath.Username,
                                                                       dst.IOPath.Password, true);
                            IActivityIOOperationsEndPoint destinationEndPoint =
                                ActivityIOFactory.CreateOperationEndPointFromIOPath(destinationPath);
                            dst.CreateDirectory(destinationPath, args);
                            TransferDirectoryContents(sourceEndPoint, destinationEndPoint, args);
                        })).ToArray());
                }
                catch(AggregateException e)
                {
                    string message = "";
                    foreach(var exception in e.InnerExceptions)
                    {
                        if(exception != null && !string.IsNullOrEmpty(exception.Message))
                        {
                            message += exception.Message + "\r\n";
                        }
                    }
                    throw new Exception(message, e);
                }
            }

            IList<IActivityIOPath> srcContents = src.ListFilesInDirectory(src.IOPath);

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

                        IActivityIOPath cpPath =
                            ActivityIOFactory.CreatePathFromString(
                                string.Format("{0}{1}{2}", origDstPath, dst.PathSeperator(),
                                              (Dev2ActivityIOPathUtils.ExtractFileName(p.Path))),
                                dst.IOPath.Username,
                                dst.IOPath.Password, true);
                        if(args.Overwrite || !dst.PathExist(cpPath))
                        {
                            using(s = src.Get(p))
                            {

                                // Need to ensure we have a file name on dst
                                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(cpPath.Path,
                                                                                                 dst.IOPath.Username,
                                                                                                 dst.IOPath.Password,
                                                                                                 true);
                                IActivityIOOperationsEndPoint tmpEP =
                                    ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
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

                            IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmp,
                                                                                             dst.IOPath.Username,
                                                                                             dst.IOPath.Password);
                            IActivityIOOperationsEndPoint tmpEP =
                                ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);

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




        /// <summary>
        /// Transfer the contents of the directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        private void ValidateSourceAndDestinationContents(IActivityIOOperationsEndPoint src,
                                                          IActivityIOOperationsEndPoint dst,
                                                          Dev2CRUDOperationTO args)
        {
            if(!args.Overwrite)
            {
                IList<IActivityIOPath> srcContentsFolders = src.ListFoldersInDirectory(src.IOPath);
                foreach(var sourcePath in srcContentsFolders)
                {
                    IActivityIOOperationsEndPoint sourceEndPoint =
                        ActivityIOFactory.CreateOperationEndPointFromIOPath(sourcePath);

                    IList<string> dirParts =
                        sourceEndPoint.IOPath.Path.Split(sourceEndPoint.PathSeperator().ToCharArray(),
                                                         StringSplitOptions.RemoveEmptyEntries);
                    var directory = dirParts.Last();
                    IActivityIOPath destinationPath =
                        ActivityIOFactory.CreatePathFromString(dst.Combine(directory),
                                                               dst.IOPath.Username,
                                                               dst.IOPath.Password, true);

                    IActivityIOOperationsEndPoint destinationEndPoint =
                        ActivityIOFactory.CreateOperationEndPointFromIOPath(destinationPath);

                    if(destinationEndPoint.PathExist(destinationEndPoint.IOPath))
                    {
                        ValidateSourceAndDestinationContents(sourceEndPoint, destinationEndPoint, args);
                    }
                }


                IList<IActivityIOPath> srcContents = src.ListFilesInDirectory(src.IOPath);
                IList<IActivityIOPath> dstContents = dst.ListFilesInDirectory(dst.IOPath);

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
                        throw new Exception(
                            "The following file(s) exist in the destination folder and overwrite is set to false:- " +
                            fileNames);
                    }
                }
            }
        }

        private static DirectoryInfo GetWhereToPut(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst)
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
                    throw new Exception(
                        "The following file(s) exist in the destination folder and overwrite is set to false :- " +
                        sourceFile);
                }
            }
        }

        private static string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint)
        {
            string pathSeperator = endPoint.PathSeperator();
            return endPoint.IOPath.Path.Split(pathSeperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
        }

        private static string GetFileNameFromEndPoint(IActivityIOOperationsEndPoint endPoint, IActivityIOPath path)
        {
            string pathSeperator = endPoint.PathSeperator();
            return path.Path.Split(pathSeperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
        }

        public string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args)
        {
            return ValidateZipSourceDestinationFileOperation(src, dst, args, () =>
            {
                string tempFileName;

                if(src.PathIs(src.IOPath) == enPathType.Directory || Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                {
                    tempFileName = ZipDirectoryToALocalTempFile(src, args);
                }
                else
                {
                    tempFileName = ZipFileToALocalTempFile(src, args);
                }

                return TransferTempZipFileToDestination(src, dst, args, tempFileName);
            });
        }

        private string ZipFileToALocalTempFile(IActivityIOOperationsEndPoint src, Dev2ZipOperationTO args)
        {
            // normal not wild char file && not directory
            string packFile = src.IOPath.Path;
            string tempFileName = CreateTmpFile();

            if(src.RequiresLocalTmpStorage())
            {
                string tempDir = CreateTmpDirectory();
                var tmpFile = Path.Combine(tempDir,
                                           src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                                 StringSplitOptions.RemoveEmptyEntries)
                                              .Last());
                packFile = tmpFile;
                Stream s = src.Get(src.IOPath);
                File.WriteAllBytes(tmpFile, s.ToByteArray());
                s.Close();
            }

            using(var zip = new ZipFile())
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

                zip.Save(tempFileName);
            }

            return tempFileName;
        }

        private string ZipDirectoryToALocalTempFile(IActivityIOOperationsEndPoint src, Dev2ZipOperationTO args)
        {
            // tmp dir for files required
            string tmpDir = CreateTmpDirectory();
            string tempFilename = CreateTmpFile();
            IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmpDir, "", "");
            IActivityIOOperationsEndPoint tmpEndPoint =
                ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);

            // stage contents to local folder
            TransferDirectoryContents(src, tmpEndPoint, new Dev2CRUDOperationTO(true));
            // get directory contents

            using(var zip = new ZipFile())
            {
                // set password if exist
                if(args.ArchivePassword != string.Empty)
                {
                    zip.Password = args.ArchivePassword;
                }

                // compression ratio                    
                zip.CompressionLevel = ExtractZipCompressionLevel(args.CompressionRatio);

                IList<IActivityIOPath> toAdd = ListDirectory(tmpEndPoint, ReadTypes.FilesAndFolders);
                // add all files to archive
                foreach(IActivityIOPath p in toAdd)
                {
                    if(tmpEndPoint.PathIs(p) == enPathType.Directory)
                    {
                        string directoryPathInArchive = p.Path.Replace(tmpPath.Path, "");
                        zip.AddDirectory(p.Path, directoryPathInArchive);
                    }
                    else
                    {
                        zip.AddFile(p.Path, ".");
                    }
                }
                zip.Save(tempFilename);
            }

            // remove locally staged files
            DirectoryHelper.CleanUp(tmpDir);

            return tempFilename;
        }

        private string TransferTempZipFileToDestination(IActivityIOOperationsEndPoint src,
                                                        IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args,
                                                        string tmpZip)
        {
            // now transfer the zip file to the correct location
            string result;
            using(Stream s2 = new MemoryStream(File.ReadAllBytes(tmpZip)))
            {
                // add archive name to path
                dst =
                    ActivityIOFactory.CreateOperationEndPointFromIOPath(
                        ActivityIOFactory.CreatePathFromString(dst.IOPath.Path, dst.IOPath.Username,
                                                               dst.IOPath.Password, true));

                var zipTransferArgs = new Dev2CRUDOperationTO(args.Overwrite);

                result = resultOk;

                if(src.RequiresLocalTmpStorage())
                {
                    if(dst.Put(s2, dst.IOPath, zipTransferArgs, null) < 0)
                    {
                        result = resultBad;
                    }
                }
                else
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

                // remove tmp directory and tmp zip
                RemoveTmpFile(tmpZip);
            }
            return result;
        }

        private static bool IsNotFTPTypePath(IActivityIOPath src)
        {
            return !src.Path.StartsWith("ftp://") && !src.Path.StartsWith("ftps://") && !src.Path.StartsWith("sftp://");
        }

        private static bool IsUncFileTypePath(IActivityIOPath src)
        {
            return src.Path.StartsWith(@"\\");
        }

        #endregion

        #region Validations

        private string ValidateRenameSourceAndDesinationTypes(IActivityIOOperationsEndPoint src,
                                                              IActivityIOOperationsEndPoint dst,
                                                              Dev2CRUDOperationTO args)
        {
            //ensures that the source and destination locations are of the same type
            if(src.PathIs(src.IOPath) != dst.PathIs(dst.IOPath))
            {
                throw new Exception("Source and destination need to be both files or directories");
            }

            //Rename Tool if the file/folder exists then delete it and put the source there
            if(dst.PathExist(dst.IOPath))
            {
                if(!args.Overwrite)
                {
                    throw new Exception("Destination directory already exists and overwrite is set to false");
                }

                //Clear the existing folder
                dst.Delete(dst.IOPath);
            }

            return Move(src, dst, args);
        }

        private string ValidateCopySourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                  IActivityIOOperationsEndPoint dst,
                                                                  Dev2CRUDOperationTO args,
                                                                  Func<string> performAfterValidation)
        {
            string result = resultOk;
            ValidateSourceAndDestinationPaths(src, dst);

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

        private string ValidateUnzipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                   IActivityIOOperationsEndPoint dst,
                                                                   Dev2UnZipOperationTO args,
                                                                   Func<string> performAfterValidation)
        {
            ValidateSourceAndDestinationPaths(src, dst);

            if(dst.PathIs(dst.IOPath) != enPathType.Directory)
            {
                throw new Exception("Destination must be a directory");
            }

            if(src.PathIs(src.IOPath) != enPathType.File)
            {
                throw new Exception("Source must be a file");
            }

            if(!args.Overwrite && dst.PathExist(dst.IOPath))
            {
                throw new Exception("Destination directory already exists and overwrite is set to false");
            }

            return performAfterValidation();
        }

        private string ValidateZipSourceDestinationFileOperation(IActivityIOOperationsEndPoint src,
                                                                 IActivityIOOperationsEndPoint dst,
                                                                 Dev2ZipOperationTO args,
                                                                 Func<string> performAfterValidation)
        {
            AddMissingFileDirectoryParts(src, dst);


            if(dst.PathIs(dst.IOPath) == enPathType.Directory)
            {
                string sourcePart =
                    src.IOPath.Path.Split(src.PathSeperator().ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                       .Last();
                if(src.PathIs(src.IOPath) == enPathType.File)
                {
                    var fileInfo = new FileInfo(sourcePart);
                    dst.IOPath.Path = dst.Combine(sourcePart.Replace(fileInfo.Extension, ".zip"));
                }
                else
                {
                    dst.IOPath.Path = dst.IOPath.Path + ".zip";
                }
            }
            else
            {
                string sourcePart =
                    dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                       .Last();
                var fileInfo = new FileInfo(sourcePart);
                dst.IOPath.Path = dst.IOPath.Path.Replace(fileInfo.Extension, ".zip");
            }

            if(!args.Overwrite && dst.PathExist(dst.IOPath))
            {
                throw new Exception("Destination file already exists and overwrite is set to false");
            }

            //ensures destination folder structure exists
            string opStatus = CreateEndPoint(dst, new Dev2CRUDOperationTO(args.Overwrite),
                                             dst.PathIs(dst.IOPath) == enPathType.Directory);
            if(!opStatus.Equals("Success"))
            {
                throw new Exception("Recursive Directory Create Failed For [ " + dst.IOPath.Path + " ]");
            }

            return performAfterValidation();
        }

        private void AddMissingFileDirectoryParts(IActivityIOOperationsEndPoint src,
                                                  IActivityIOOperationsEndPoint dst)
        {
            if(src.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception("Source can not be an empty string");
            }


            var sourceParts = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                    StringSplitOptions.RemoveEmptyEntries).ToList();

            if(dst.IOPath.Path.Trim().Length == 0)
            {
                dst.IOPath.Path = src.IOPath.Path;
            }
            else
            {
                if(!Path.IsPathRooted(dst.IOPath.Path) && IsNotFTPTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath))
                {
                    string lastPart = sourceParts.Last();
                    dst.IOPath.Path =
                        Path.Combine(src.PathIs(dst.IOPath) == enPathType.Directory
                                         ? src.IOPath.Path
                                         : src.IOPath.Path.Replace(lastPart, ""), dst.IOPath.Path);
                }
            }


            var destinationParts = dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(),
                                                         StringSplitOptions.RemoveEmptyEntries).ToList();

            while(destinationParts.Count > sourceParts.Count)
            {
                destinationParts.Remove(destinationParts.Last());
            }

            if(destinationParts.OrderBy(i => i).SequenceEqual(
                sourceParts.OrderBy(i => i)))
            {
                if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                {
                    string[] strings = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                             StringSplitOptions.RemoveEmptyEntries);
                    string lastPart = strings.Last();
                    dst.IOPath.Path = src.PathIs(src.IOPath) == enPathType.Directory
                                          ? Path.Combine(dst.IOPath.Path, lastPart)
                                          : dst.IOPath.Path.Replace(lastPart, "");
                }
            }
            else
            {
                if(dst.PathIs(dst.IOPath) == enPathType.Directory && src.PathIs(src.IOPath) == enPathType.Directory)
                {
                    string[] strings = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                             StringSplitOptions.RemoveEmptyEntries);
                    string lastPart = strings.Last();
                    dst.IOPath.Path = dst.Combine(lastPart);
                }
            }
        }

        private void ValidateSourceAndDestinationPaths(IActivityIOOperationsEndPoint src,
                                                       IActivityIOOperationsEndPoint dst)
        {
            if(src.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception("Source can not be an empty string");
            }

            var sourceParts = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries).ToList();

            if(dst.IOPath.Path.Trim().Length == 0)
            {
                dst.IOPath.Path = src.IOPath.Path;
            }
            else
            {
                if(!Path.IsPathRooted(dst.IOPath.Path) && IsNotFTPTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath))
                {
                   
                    string lastPart = sourceParts.Last();

                    dst.IOPath.Path =
                        Path.Combine(src.PathIs(dst.IOPath) == enPathType.Directory
                                         ? src.IOPath.Path
                                         : src.IOPath.Path.Replace(lastPart, ""), dst.IOPath.Path);
                }
            }

            var destinationParts = dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries).ToList();

            while(destinationParts.Count > sourceParts.Count)
            {
                destinationParts.Remove(destinationParts.Last());
            }

            if(destinationParts.OrderBy(i => i).SequenceEqual(
                 sourceParts.OrderBy(i => i)))
            {
                throw new Exception("Destination directory can not be a child of the source directory");
            }
        }

        private void ValidateEndPoint(IActivityIOOperationsEndPoint endPoint, Dev2CRUDOperationTO args)
        {
            if(endPoint.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception("Source can not be an empty string");
            }

            if(endPoint.PathExist(endPoint.IOPath) && !args.Overwrite)
            {
                var type = endPoint.PathIs(endPoint.IOPath) == enPathType.Directory ? "Directory" : "File";
                throw new Exception(string.Format("Destination {0} already exists and overwrite is set to false",
                                                  type));
            }
        }

        #endregion
    }
}

