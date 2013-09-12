using System;

namespace Tfs.Squish
{
    internal class AnnotatedVersionAvailableEventArgs : EventArgs
    {
        public int Index { get; private set; }

        public AnnotatedFile.FileVersion File { get; private set; }

        public bool Canceled { get; set; }

        public AnnotatedVersionAvailableEventArgs(int index, AnnotatedFile.FileVersion file)
        {
            Index = index;
            File = file;
        }
    }
}
