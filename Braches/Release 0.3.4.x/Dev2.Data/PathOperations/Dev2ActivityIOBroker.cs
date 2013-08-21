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

        private static string resultOk = "Success";

        private static string resultBad = "Failure";

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

            if (dst.RequiresLocalTmpStorage())
            {
                if (args.Append)
                {
                    using(s = dst.Get(dst.IOPath))
                    {
                        File.WriteAllBytes(tmp, s.ToByteArray());
                        // Convert.FromBase64String(base64FileData);
                        File.AppendAllText(tmp, args.FileContents);
                        s.Close();
                        s.Dispose();
                    }
                }
                else
                {
                    // Handle base64 String
                    if (IsBase64(args.FileContents))
                    {
                        byte[] data = Convert.FromBase64String(args.FileContents.Replace("Content-Type:BASE64", ""));
                        File.WriteAllBytes(tmp, data);
                    }
                    else
                    {
                        File.WriteAllText(tmp, args.FileContents);
                    }
                }
            }
            else
            {
                // we can write directly to the file
                if (args.Append)
                {
                    File.WriteAllText(tmp, File.ReadAllText(dst.IOPath.Path));
                    File.WriteAllText(tmp, args.FileContents);
                }
                else
                {
                    // Handle base64 String
                    if (IsBase64(args.FileContents))
                    {
                        byte[] data = Convert.FromBase64String(args.FileContents.Replace("Content-Type:BASE64", ""));
                        File.WriteAllBytes(tmp, data);
                    }
                    else
                    {
                        File.WriteAllText(tmp, args.FileContents);
                    }
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
                if (dst.Put(s, dst.IOPath, newArgs, null) < 0)
                {
                    result = resultBad;
                }
                s.Close();
                s.Dispose();
            }

            RemoveTmpFile(tmp);

            return result;

        }

        public string Delete(IActivityIOOperationsEndPoint src)
        {
            string result = resultOk;

            if (!src.Delete(src.IOPath))
            {
                result = resultBad;
            }

            return result;
        }

        public IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src)
        {
            return src.ListDirectory(src.IOPath);
        }

       public string CreateChildrenDirectoriesAtDestination(IActivityIOOperationsEndPoint dst,IActivityIOOperationsEndPoint src, Dev2CRUDOperationTO args)
       {
           IEnumerable<string> activityIOPaths = src.ListDirectory(src.IOPath).Where(path => src.PathIs(path) == enPathType.Directory).Select(path => path.Path.Replace(src.IOPath.Path,""));
           string origDstPath = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(dst.IOPath.Path);
           foreach(var activityIOPath in activityIOPaths)
           {
               IActivityIOPath pathFromString = ActivityIOFactory.CreatePathFromString(origDstPath + activityIOPath);
               dst.CreateDirectory(pathFromString, args);
           }
//           IList<string> dirParts = MakeDirectoryParts(dst.IOPath, dst.PathSeperator());
//           // check from lowest path part up
//           int deepestIndex = -1;
//           int startDepth = (dirParts.Count - 1);
//
//           int pos = startDepth;
//
//           while (pos >= 0 && deepestIndex == -1)
//           {
//               IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(dirParts[pos]);
//               try
//               {
//                   if (dst.ListDirectory(tmpPath) != null)
//                   {
//                       deepestIndex = pos;
//                   }
//               }
//               catch (Exception ex)
//               {
//                   ServerLogger.LogError(ex);
//                   // nothing to do, we swallow to keep going and find the deepest index a directory does exist at
//               }
//               pos--;
//           }
           return "";
       }

        public string CreateEndPoint(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
        {

            string result = resultOk;

            string path = dst.IOPath.Path;

            // check the the dir strucutre exist
            IList<string> dirParts = MakeDirectoryParts(dst.IOPath, dst.PathSeperator());

            // check from lowest path part up
            int deepestIndex = -1;
            int startDepth = (dirParts.Count - 1);

            int pos = startDepth;

            while (pos >= 0 && deepestIndex == -1)
            {
                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(dirParts[pos]);
                try
                {
                    if (dst.ListDirectory(tmpPath) != null)
                    {
                        deepestIndex = pos;
                    }
                }
                catch (Exception ex)
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

            while (pos <= startDepth && ok)
            {
                IActivityIOPath toCreate = ActivityIOFactory.CreatePathFromString(dirParts[pos]);
                dst.IOPath = toCreate;
                ok = CreateDirectory(dst, args);
                pos++;
            }

            dst.IOPath = origPath;

            // dir create failed
            if (!ok)
            {
                result = resultBad;
            }
            else if (dst.PathIs(dst.IOPath) == enPathType.File && createToFile && ok)
            {
                if (!CreateFile(dst, args))
                {
                    result = resultBad;
                }
            }

            return result;
        }

        public string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {

            string result = resultOk;

            // create any missing directories on the endpoint
            string opStatus = CreateEndPoint(dst, args, false);
//            if(args.DoRecursiveCopy)
//            {
//                CreateChildrenDirectoriesAtDestination(dst, src,args);
//            }
            if (!opStatus.Equals("Success"))
            {
                throw new Exception("Recursive Directory Create Failed For [ " + dst.IOPath.Path + " ]");
            }

            if (src.RequiresLocalTmpStorage() && dst.RequiresLocalTmpStorage())
            {
                if (src.PathIs(src.IOPath) == enPathType.File)
                {
                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                    {
                        // single file fetch
                        String tmp = CreateTmpFile();
                        using(Stream s = src.Get(src.IOPath))
                        {
                            File.WriteAllBytes(tmp, s.ToByteArray());
                            if(!src.IOPath.Path.StartsWith("ftp://") && !src.IOPath.Path.StartsWith("ftps://"))
                            {
                                var fileInfo = new FileInfo(src.IOPath.Path);
                                if (fileInfo.Directory != null && Path.IsPathRooted(fileInfo.Directory.ToString()))
                                {
                                    if (dst.Put(s, dst.IOPath, args, fileInfo.Directory) < 0)
                                    {
                                        result = resultBad;
                                    }
                                }
                                else
                                {
                                    if (dst.Put(s, dst.IOPath, args, null) < 0)
                                    {
                                        result = resultBad;
                                    }
                                }
                            }
                            else
                            {
                                if (dst.Put(s, dst.IOPath, args, null) < 0)
                                {
                                    result = resultBad;
                                }
                            }

                            s.Close();
                            s.Dispose();
                        }
                        RemoveTmpFile(tmp);
                    }
                    else
                    {
                        // we have star wild cards to deal with
                        if (!TransferDirectoryContents(src, dst, args, false))
                        {
                            result = resultBad;
                        }
                    }
                }
                else
                {
                    // we have a directory to fetch
                    // we have star wild cards to deal with
                    if (!TransferDirectoryContents(src, dst, args, false))
                    {
                        result = resultBad;
                    }
                }
            }
            else
            {
                if (src.PathIs(src.IOPath) == enPathType.File)
                {
                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                    {
                        // single file
                        using(Stream s = src.Get(src.IOPath))
                        {
                            // If dst is a directory, retain the file name
                            string dstPath = dst.IOPath.Path;

                            if(dst.PathIs(dst.IOPath) == enPathType.Directory)
                            {
                                dstPath += dst.PathSeperator() + Dev2ActivityIOPathUtils.ExtractFileName(src.IOPath.Path);
                            }

                            IActivityIOPath tmpDst = ActivityIOFactory.CreatePathFromString(dstPath, dst.IOPath.Username, dst.IOPath.Password);

                            if (!src.IOPath.Path.StartsWith("ftp://") && !src.IOPath.Path.StartsWith("ftps://"))
                            {
                                var fileInfo = new FileInfo(src.IOPath.Path);
                                if (fileInfo.Directory != null && Path.IsPathRooted(fileInfo.Directory.ToString()))
                                {
                                    if (dst.Put(s, tmpDst, args, fileInfo.Directory) < 0)
                                    {
                                        result = resultBad;
                                    }
                                }
                                else
                                {
                                    if (dst.Put(s, tmpDst, args, null) < 0)
                                    {
                                        result = resultBad;
                                    }
                                }
                            }
                            else
                            {
                                if (dst.Put(s, tmpDst, args, null) < 0)
                                {
                                    result = resultBad;
                                }
                            }
                            s.Close();
                            s.Dispose();
                        }
                    }
                    else
                    {
                        // we have star wild cards
                        // we have star wild cards to deal with
                        if (!TransferDirectoryContents(src, dst, args, false))
                        {
                            result = resultBad;
                        }
                    }
                }
                else
                {
                    // we have a dir
                    // we have star wild cards to deal with
                    if (!TransferDirectoryContents(src, dst, args, false))
                    {
                        result = resultBad;
                    }
                }
            }

            return result;
        }

        public string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {

            string result = resultOk;

            if (src.RequiresLocalTmpStorage() && dst.RequiresLocalTmpStorage())
            {
                if (src.PathIs(src.IOPath) == enPathType.File)
                {
                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                    {
                        // single file fetch

                        // If file not present
                        if (args.Overwrite || !dst.PathExist(dst.IOPath))
                        {
                            using(Stream s = src.Get(src.IOPath))
                            {
                                //File.WriteAllBytes(tmp, s.ToByteArray());
                                if(Path.IsPathRooted(src.IOPath.Path))
                                {
                                    dst.Put(s, dst.IOPath, args, new FileInfo(src.IOPath.Path).Directory);
                                }
                                else
                                {
                                    dst.Put(s, dst.IOPath, args, null);
                                }
                                s.Close();
                                s.Dispose();
                            }

                            // delete original file ;)
                            src.Delete(src.IOPath);
                        }
                        else
                        {
                            result = resultBad;
                        }
                    }
                    else
                    {
                        // we have star wild cards to deal with
                        if (!TransferDirectoryContents(src, dst, args, true))
                        {
                            result = resultBad;
                        }
                    }
                }
                else
                {
                    // we have star wild cards to deal with
                    if (!TransferDirectoryContents(src, dst, args, true))
                    {
                        result = resultBad;
                    }
                }
            }
            else
            {
                if (src.PathIs(src.IOPath) == enPathType.File)
                {
                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
                    {
                        // single file
                        using(Stream s = src.Get(src.IOPath))
                        {
                            dst.Put(s, dst.IOPath, args, new FileInfo(src.IOPath.Path).Directory);
                            s.Close();
                            s.Dispose();
                        }
                        src.Delete(src.IOPath);
                    }
                    else
                    {
                        // we have star wild cards to deal with
                        if (!TransferDirectoryContents(src, dst, args, true))
                        {
                            result = resultBad;
                        }
                    }
                }
                else
                {
                    // we have star wild cards to deal with
                    if (!TransferDirectoryContents(src, dst, args, true))
                    {
                        result = resultBad;
                    }
                }
            }

            return result;
        }

        public string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args)
        {

            // tmp zip file location
            string tmpZip = CreateTmpFile();

            if (src.PathIs(src.IOPath) == enPathType.Directory || Dev2ActivityIOPathUtils.IsStarWildCard(src.IOPath.Path))
            {

                // tmp dir for files required
                string tmpDir = CreateTmpDirectory();
                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmpDir);
                IActivityIOOperationsEndPoint tmpEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                Dev2CRUDOperationTO myArgs = new Dev2CRUDOperationTO(true);
                // stage contents to local folder
                TransferDirectoryContents(src, tmpEndPoint, myArgs, false);

                // get directory contents
                IList<IActivityIOPath> toAdd = ListDirectory(tmpEndPoint);

                using (ZipFile zip = new ZipFile())
                {
                    // set password if exist
                    if (args.ArchivePassword != string.Empty)
                    {
                        zip.Password = args.ArchivePassword;
                    }

                    // compression ratio                    
                    zip.CompressionLevel = ExtractZipCompressionLevel(args.CompressionRatio);

                    // add all files to archive
                    foreach (IActivityIOPath p in toAdd)
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

                if (src.RequiresLocalTmpStorage())
                {
                    string tmpFile = CreateTmpFile();
                    packFile = tmpFile;
                    Stream s = src.Get(src.IOPath);
                    File.WriteAllBytes(tmpFile, s.ToByteArray());
                    s.Close();
                }

                using (ZipFile zip = new ZipFile())
                {
                    // set password if exist
                    if (args.ArchivePassword != string.Empty)
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
            string result = string.Empty;
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

                Dev2CRUDOperationTO zipTransferArgs = new Dev2CRUDOperationTO(false);

                result = resultOk;

                if (!src.IOPath.Path.StartsWith("ftp://") && !src.IOPath.Path.StartsWith("ftps://"))
                {
                    var fileInfo = new FileInfo(src.IOPath.Path);
                    if (fileInfo.Directory != null && Path.IsPathRooted(fileInfo.Directory.ToString()))
                    {
                        if (dst.Put(s2, dst.IOPath, zipTransferArgs, fileInfo.Directory) < 0)
                        {
                            result = resultBad;
                        }
                    }
                    else
                    {
                        if (dst.Put(s2, dst.IOPath, zipTransferArgs, null) < 0)
                        {
                            result = resultBad;
                        }
                    }
                }
                else
                {
                    if (dst.Put(s2, dst.IOPath, zipTransferArgs, null) < 0)
                    {
                        result = resultBad;
                    }
                }

                // remove tmp directory and tmp zip
                RemoveTmpFile(tmpZip);
            }

            return result;
        }

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2UnZipOperationTO args)
        {

            string zipFile = src.IOPath.Path;

            if (src.RequiresLocalTmpStorage())
            {

                string tmpZip = CreateTmpFile();
                using(Stream s = src.Get(src.IOPath))
                {
                    
                    File.WriteAllBytes(tmpZip, s.ToByteArray());
                    s.Close();
                    s.Dispose();
                }

                zipFile = tmpZip;
            }

            string unzipDirectory = dst.IOPath.Path;

            if (dst.RequiresLocalTmpStorage())
            {
                // unzip locally then Put the contents of the archive to the dst end-point
                unzipDirectory = CreateTmpDirectory();
            }


            using (ZipFile zip = ZipFile.Read(zipFile))
            {

                if (args.ArchivePassword != string.Empty)
                {
                    zip.Password = args.ArchivePassword;
                }

                foreach (ZipEntry ze in zip)
                {
                    ze.Extract(unzipDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }


            if (dst.RequiresLocalTmpStorage())
            {
                IActivityIOPath newSrc = ActivityIOFactory.CreatePathFromString(unzipDirectory);
                IActivityIOOperationsEndPoint newSrcEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(newSrc);
                Dev2CRUDOperationTO newArgs = new Dev2CRUDOperationTO(false);
                Move(newSrcEP, dst, newArgs);
                // local delete taken care of for us ;)
            }

            // clean up local tmp
            if (src.RequiresLocalTmpStorage())
            {
                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(zipFile);
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
            string[] tmp = new string[1];

            IList<string> result = new List<string>();

            char[] splitOn = splitter.ToCharArray();

            tmp = path.Path.Split(splitOn);

            // remove trailing file entry if exist
            string candiate = tmp[tmp.Length - 1];
            int len = tmp.Length;
            if (candiate.Contains("*.") || candiate.Contains("."))
            {
                len = (tmp.Length - 1);
            }

            string builderPath = "";
            // build up URI parts from root down
            for (int i = 0; i < len; i++)
            {
                builderPath += tmp[i] + splitter;
                result.Add(builderPath);
            }

            return result;
        }

        private bool CreateDirectory(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            bool result = true;

            if (!dst.CreateDirectory(dst.IOPath, args))
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

                if (dst.Put(s, dst.IOPath, args, null) < 0)
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

            while (pos < lvls.Length && lvls.GetValue(pos).ToString() != lvl)
            {
                pos++;
            }

            if (pos < lvls.Length)
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
        private bool TransferDirectoryContents(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool removeSrc)
        {
            // List directory contents
            IList<IActivityIOPath> dirContents = src.ListDirectory(src.IOPath);
            bool result = true;

            string origDstPath = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(dst.IOPath.Path);

            // Sashen: 22-08-2012 - Not entirely sure what the behaviour should be if a folder already exists
            // as it stands, we will create a new folder 
            // check if the directory we want to copy into exists

            if (!dst.PathExist(dst.IOPath))
            {
                CreateDirectory(dst, args);
            }

            // get each file, then put it to the correct location
            foreach (IActivityIOPath p in dirContents)
            {
                Stream s = null;
                try
                {
                    //  ensure file is not present already 
                    // Sashen : 22-08-2012 - This used to check if the directory was present, but instead should check
                    // if the files exists in a directory, the name of the directory does imply that only the contents
                    // should be moved
                    if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                    {
                        
                        IActivityIOPath cpPath = ActivityIOFactory.CreatePathFromString(string.Format("{0}{1}{2}", origDstPath, dst.PathSeperator(), (Dev2ActivityIOPathUtils.ExtractFileName(p.Path))));
                        if (args.Overwrite || !dst.PathExist(cpPath))
                        {
                            if(dst.PathIs(cpPath) == enPathType.Directory && Directory.Exists(p.Path) && args.DoRecursiveCopy)
                            {
                                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(cpPath.Path, dst.IOPath.Username, dst.IOPath.Password);
                                IActivityIOOperationsEndPoint tmpEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                                IActivityIOOperationsEndPoint operationEndPointFromIOPath = ActivityIOFactory.CreateOperationEndPointFromIOPath(p);
                                TransferDirectoryContents(operationEndPointFromIOPath, tmpEP, args, removeSrc);
                            }
                            else if (dst.PathIs(cpPath) == enPathType.Directory && Directory.Exists(p.Path) && !args.DoRecursiveCopy)
                            {
                                continue;
                            }
                            using(s = src.Get(p))
                            {

                                // Need to ensure we have a file name on dst
                                //if (dst.PathIs(dst.IOPath) == enPathType.Directory) {
                                //string tmp = origDstPath + "\\" + Dev2ActivityIOPathUtils.ExtractFileName(p.Path);

                                //IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmp, dst.IOPath.Username, dst.IOPath.Password);
                                IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(cpPath.Path, dst.IOPath.Username, dst.IOPath.Password);
                                IActivityIOOperationsEndPoint tmpEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                                // Sashen : 22-08-2012: The dst folder path is being incorrectly set on intial run and this cause all subsequent operations
                                // to not occur
                                // So, if there were 2 files or more, only the first file would be copied.
                                //dst = tmpEP;

                                //}
                                //else {
                                //    string tmp = dst.IOPath.Path + "\\" + Dev2ActivityIOPathUtils.ExtractFileName(p.Path);

                                //    IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmp, dst.IOPath.Username, dst.IOPath.Password);
                                //    IActivityIOOperationsEndPoint tmpEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                                //    dst = tmpEP;
                                //}

                                if (tmpEP.Put(s, tmpEP.IOPath, args, new FileInfo(src.IOPath.Path).Directory) < 0)
                                {
                                    result = false;
                                }
                                s.Close();
                                s.Dispose();
                            }
                            // if move op, delete orig
                            if (removeSrc)
                            {
                                src.Delete(p);
                            }
                        }
                    }
                    else if (args.Overwrite || !dst.PathExist(dst.IOPath))
                    {
                        using(s = src.Get(p))
                        {

                            // Need to ensure we have a file name on dst
                            //if (dst.PathIs(dst.IOPath) == enPathType.Directory) {
                            string tmp = origDstPath + "\\" + Dev2ActivityIOPathUtils.ExtractFileName(p.Path);

                            IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmp, dst.IOPath.Username, dst.IOPath.Password);
                            IActivityIOOperationsEndPoint tmpEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                            // Sashen : 22-08-2012: The dst folder path is being incorrectly set on intial
                            //dst = tmpEP;

                            //}
                            //else {
                            //    string tmp = dst.IOPath.Path + "\\" + Dev2ActivityIOPathUtils.ExtractFileName(p.Path);

                            //    IActivityIOPath tmpPath = ActivityIOFactory.CreatePathFromString(tmp, dst.IOPath.Username, dst.IOPath.Password);
                            //    IActivityIOOperationsEndPoint tmpEP = ActivityIOFactory.CreateOperationEndPointFromIOPath(tmpPath);
                            //    dst = tmpEP;
                            //}

                            if (tmpEP.Put(s, tmpEP.IOPath, args, new FileInfo(src.IOPath.Path).Directory) < 0)
                            {
                                result = false;
                            }
                            s.Close();
                            s.Dispose();
                        }

                        // if move op, delete orig
                        if (removeSrc)
                        {
                            src.Delete(p);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (s != null)
                    {
                        s.Close();
                    }
                    ServerLogger.LogError(ex);
                }
            }
            // if move op, delete orig
            if (removeSrc)
            {
                src.Delete(src.IOPath);
            }

            return result;
        }

        /// <summary>
        /// Creates a tmp file
        /// </summary>
        /// <returns></returns>
        private string CreateTmpFile()
        {
            string tmpFile = System.IO.Path.GetTempFileName();
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
            string tmpDir = System.IO.Path.GetTempPath();
            DirectoryInfo di = Directory.CreateDirectory(tmpDir + "\\" + Guid.NewGuid());

            return (di.FullName);
        }

        private string ExtractLastFolderName(string path)
        {
            string result = string.Empty;

            int idx = path.LastIndexOf("\\");

            if (idx < 0)
            {
                idx = path.LastIndexOf("/");
            }

            if (idx > 0)
            {
                result = path.Substring((idx + 1), (path.Length - (idx)));
            }

            return result;
        }

        #endregion
    }
}
