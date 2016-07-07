using System;
using System.IO;
using System.Linq;
using Dev2.Common;
using Dev2.Data.PathOperations.Enums;
using Dev2.Data.PathOperations.Extension;
using Dev2.PathOperations;
using Ionic.Zip;
using Ionic.Zlib;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Util
{
    public interface ICommon
    {
        void ValidateEndPoint(IActivityIOOperationsEndPoint endPoint, Dev2CRUDOperationTO args);
        void ExtractFile(Dev2UnZipOperationTO args, ZipFile zip, string extractFromPath);
        void AppendToTemp(Stream originalFileStream, string temp);
        CompressionLevel ExtractZipCompressionLevel(string lvl);

        bool IsNotFtpTypePath(IActivityIOPath src);

        void ValidateSourceAndDestinationPaths(IActivityIOOperationsEndPoint src,
            IActivityIOOperationsEndPoint dst);

        bool IsUncFileTypePath(IActivityIOPath src);

        void AddMissingFileDirectoryParts(IActivityIOOperationsEndPoint src,
            IActivityIOOperationsEndPoint dst);

        string CreateTmpDirectory();
    }
    public class Common : ICommon
    {
        public void ValidateEndPoint(IActivityIOOperationsEndPoint endPoint, Dev2CRUDOperationTO args)
        {
            if (endPoint.IOPath?.Path.Trim().Length == 0)
            {
                throw new Exception(ErrorResource.SourceCannotBeAnEmptyString);
            }

            if (endPoint.PathExist(endPoint.IOPath) && !args.Overwrite)
            {
                throw new Exception(ErrorResource.DestinationDirectoryExist);
            }
        }

        public void ExtractFile(Dev2UnZipOperationTO args, ZipFile zip, string extractFromPath)
        {
            if (zip != null)
            {
                using (zip)
                {
                    if (!string.IsNullOrEmpty(args.ArchivePassword))
                    {
                        zip.Password = args.ArchivePassword;
                    }

                    foreach (var ze in zip)
                    {
                        try
                        {
                            ze.Extract(extractFromPath,
                                       args.Overwrite
                                           ? ExtractExistingFileAction.OverwriteSilently
                                           : ExtractExistingFileAction.DoNotOverwrite);
                        }
                        catch (BadPasswordException bpe)
                        {
                            throw new Exception(ErrorResource.InvalidArchivePassword, bpe);
                        }
                    }
                }
            }
        }

        public void AppendToTemp(Stream originalFileStream, string temp)
        {
            const int BufferSize = 1024 * 1024;
            var buffer = new char[BufferSize];

            using (var writer = new StreamWriter(temp, true))
            {
                using (var reader = new StreamReader(originalFileStream))
                {
                    int bytesRead;
                    while ((bytesRead = reader.ReadBlock(buffer, 0, BufferSize)) != 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public CompressionLevel ExtractZipCompressionLevel(string lvl)
        {
            var lvls = Enum.GetValues(typeof(CompressionLevel));
            var pos = 0;
            //19.09.2012: massimo.guerrera - Changed to default instead of none
            CompressionLevel clvl = CompressionLevel.Default;

            while (pos < lvls.Length && lvls.GetValue(pos).ToString() != lvl)
            {
                pos++;
            }

            if (pos < lvls.Length)
            {
                clvl = (CompressionLevel)lvls.GetValue(pos);
            }

            return clvl;
        }

        public bool IsNotFtpTypePath(IActivityIOPath src)
        {
            return
                !src.Path.ToUpper().StartsWith("ftp://".ToUpper())
                && !src.Path.ToUpper().StartsWith("ftps://".ToUpper())
                && !src.Path.ToUpper().StartsWith("sftp://".ToUpper());
        }

        public void ValidateSourceAndDestinationPaths(IActivityIOOperationsEndPoint src,
                                                        IActivityIOOperationsEndPoint dst)
        {
            if (src.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception(ErrorResource.SourceCannotBeAnEmptyString);
            }

            var sourceParts = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries).ToList();

            if (dst.IOPath.Path.Trim().Length == 0)
            {
                dst.IOPath.Path = src.IOPath.Path;
            }
            else
            {
                if (!Path.IsPathRooted(dst.IOPath.Path) && IsNotFtpTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath))
                {

                    var lastPart = sourceParts.Last();

                    dst.IOPath.Path =
                        Path.Combine(src.PathIs(dst.IOPath) == enPathType.Directory
                                         ? src.IOPath.Path
                                         : src.IOPath.Path.Replace(lastPart, ""), dst.IOPath.Path);
                }
            }

            var destinationParts = dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(),
                                                       StringSplitOptions.RemoveEmptyEntries).ToList();

            while (destinationParts.Count > sourceParts.Count)
            {
                destinationParts.Remove(destinationParts.Last());
            }

            if (destinationParts.OrderBy(i => i).SequenceEqual(
                 sourceParts.OrderBy(i => i)))
            {
                throw new Exception(ErrorResource.DestinationDirectoryCannotBeAChild);
            }
        }

        public bool IsUncFileTypePath(IActivityIOPath src)
        {
            return src.Path.StartsWith(@"\\");
        }

        public void AddMissingFileDirectoryParts(IActivityIOOperationsEndPoint src,
                                                 IActivityIOOperationsEndPoint dst)
        {
            if (src.IOPath.Path.Trim().Length == 0)
            {
                throw new Exception(ErrorResource.SourceCannotBeAnEmptyString);
            }
            var sourceParts = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                    StringSplitOptions.RemoveEmptyEntries).ToList();

            if (dst.IOPath.Path.Trim().Length == 0)
            {
                dst.IOPath.Path = src.IOPath.Path;
            }
            else
            {
                if (!Path.IsPathRooted(dst.IOPath.Path) && IsNotFtpTypePath(dst.IOPath) && IsUncFileTypePath(dst.IOPath))
                {
                    var lastPart = sourceParts.Last();
                    dst.IOPath.Path =
                        Path.Combine(src.PathIs(dst.IOPath) == enPathType.Directory
                                         ? src.IOPath.Path
                                         : src.IOPath.Path.Replace(lastPart, ""), dst.IOPath.Path);
                }
            }
            var destinationParts = dst.IOPath.Path.Split(dst.PathSeperator().ToCharArray(),
                                                         StringSplitOptions.RemoveEmptyEntries).ToList();

            while (destinationParts.Count > sourceParts.Count)
            {
                destinationParts.Remove(destinationParts.Last());
            }

            if (destinationParts.OrderBy(i => i).SequenceEqual(sourceParts.OrderBy(i => i)))
            {
                if (dst.PathIs(dst.IOPath) == enPathType.Directory)
                {
                    var strings = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                             StringSplitOptions.RemoveEmptyEntries);
                    var lastPart = strings.Last();
                    dst.IOPath.Path = src.PathIs(src.IOPath) == enPathType.Directory
                                          ? Path.Combine(dst.IOPath.Path, lastPart)
                                          : dst.IOPath.Path.Replace(lastPart, "");
                }
            }
            else
            {
                if (dst.PathIs(dst.IOPath) == enPathType.Directory && src.PathIs(src.IOPath) == enPathType.Directory)
                {
                    var strings = src.IOPath.Path.Split(src.PathSeperator().ToCharArray(),
                                                             StringSplitOptions.RemoveEmptyEntries);
                    var lastPart = strings.Last();
                    dst.IOPath.Path = dst.Combine(lastPart);
                }
            }
        }

        public string CreateTmpDirectory()
        {
            try
            {
                var tmpDir = GlobalConstants.TempLocation;
                var di = Directory.CreateDirectory(tmpDir + "\\" + Guid.NewGuid());

                return di.FullName;
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                throw;
            }

        }
    }
}
