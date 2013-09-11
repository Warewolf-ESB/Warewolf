// Type: Microsoft.TeamFoundation.PowerTools.Client.VersionControl.AnnotatedVersionAvailableEventArgs
// Assembly: Microsoft.TeamFoundation.PowerTools.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Program Files (x86)\Microsoft Team Foundation Server 2012 Power Tools\Microsoft.TeamFoundation.PowerTools.Client.dll

using System;

namespace Tfs.Squish
{
    internal class AnnotatedVersionAvailableEventArgs : EventArgs
    {
        private int m_index;
        private AnnotatedFile.FileVersion m_file;
        private bool m_canceled;

        public int Index
        {
            get
            {
                return this.m_index;
            }
        }

        public AnnotatedFile.FileVersion File
        {
            get
            {
                return this.m_file;
            }
        }

        public bool Canceled
        {
            get
            {
                return this.m_canceled;
            }
            set
            {
                this.m_canceled = value;
            }
        }

        public AnnotatedVersionAvailableEventArgs(int index, AnnotatedFile.FileVersion file)
        {
            this.m_index = index;
            this.m_file = file;
        }
    }
}
