namespace DirectoryUtils
{
    public class DirectoryCommands
    {
        public static int Main(string[] args)
        {
            if (args.Length == 2)
            {
                return DirectoryUtil.CopyFromTo(args[0], args[1]);
            }

            return 1;
        }
    }
}
