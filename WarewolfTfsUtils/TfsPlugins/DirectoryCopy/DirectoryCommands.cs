namespace DirectoryUtils
{
    public class DirectoryCommands
    {
        public static int Main(string[] args)
        {

            if (args.Length == 2)
            {
                // no pattern l;)
                return DirectoryUtil.CopyFromTo(args[0], args[1], null);
            }

            if (args.Length == 3)
            {
                // invoke with a pattern ;)
                return DirectoryUtil.CopyFromTo(args[0], args[1], args[2]);
            }


            return 1;
        }
    }
}
