using System.IO;

namespace DirectoryUtils
{
    public class DirectoryUtil
    {

        /// <summary>
        /// Copies from all files from src dir to dst dir.
        /// </summary>
        /// <param name="src">The SRC.</param>
        /// <param name="dst">The DST.</param>
        /// <returns></returns>
        public static int CopyFromTo(string src, string dst)
        {
            int result = 0;
            try
            {
                string[] files = Directory.GetFiles(src);
                
                foreach (var f in files)
                {
                    var fileName = Path.GetFileName(f);

                    if (fileName != null)
                    {
                        var loc = Path.Combine(dst, fileName);
                        File.Copy(f, loc);
                    }
                    else
                    {
                        result = 1;
                    }
                }
            }
            catch
            {
                result = 1;
            }

            return result;
        }
    }
}
