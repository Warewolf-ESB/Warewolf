using System.IO;

namespace Dev2.Common.Common
{
    public class DeleteHelper
    {
        public static bool Delete(string path)
        {
            if(path == null)
            {
                return false;
            }

            var dirRoot = Path.GetDirectoryName(path);
            var pattern = Path.GetFileName(path);
            // directory
            if(IsDirectory(path))
            {
                DirectoryHelper.CleanUp(path);
                return true;
            }

            // wild-card char
            if(path.IndexOf("*", System.StringComparison.Ordinal) >= 0)
            {
                if(pattern != null && dirRoot != null)
                {
                    var fileList = Directory.GetFileSystemEntries(dirRoot, pattern, SearchOption.TopDirectoryOnly);
                    foreach(var file in fileList)
                    {
                        if(IsDirectory(file))
                        {
                            //it's a directory
                            Directory.Delete(file, true);
                        }
                        else
                        {
                            // we can before, we want to avoid deleting an already deleted file in sub-directory
                            File.Delete(file);
                        }
                    }
                }
            }
            else
            {
                // single file delete
                File.Delete(path);
            }

            return true;
        }


        private static bool IsDirectory(string path)
        {
            if(path.IndexOf("*", System.StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            FileAttributes attr = File.GetAttributes(path);
            if((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }

            return false;
        }
    }

}
