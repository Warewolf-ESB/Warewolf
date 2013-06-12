using System;
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
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        public static int CopyFromTo(string src, string dst, string pattern)
        {
            int result = 0;
            try
            {
                string[] files;
                if (!string.IsNullOrEmpty(pattern))
                {
                    files = Directory.GetFiles(src, pattern);
                }
                else
                {
                    files = Directory.GetFiles(src);
                }
                
                
                foreach (var f in files)
                {
                    var fileName = Path.GetFileName(f);

                    if (fileName != null)
                    {
                        try
                        {
                            var loc = Path.Combine(dst, fileName);
                            File.Copy(f, loc, true);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    else
                    {
                        result = 1;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                result = 1;
            }

            return result;
        }
    }
}
