// Type: Microsoft.TeamFoundation.PowerTools.Common.DiffLineReader
// Assembly: Microsoft.TeamFoundation.PowerTools.Common, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Program Files (x86)\Microsoft Team Foundation Server 2012 Power Tools\Microsoft.TeamFoundation.PowerTools.Common.dll

using System;
using System.IO;
using System.Text;

namespace Tfs.Squish
{
    internal class DiffLineReader : IDisposable
    {
        private const int LineSeparator = 8232;
        private const int ParagraphSeparator = 8233;
        private const int NextLine = 133;
        private TextReader m_rdr;
        private StringBuilder m_sb;

        internal DiffLineReader(TextReader rdr)
        {
            this.m_rdr = rdr;
            this.m_sb = new StringBuilder();
        }

        public void Dispose()
        {
            this.m_rdr.Dispose();
        }

        internal string ReadLine()
        {
            this.m_sb.Length = 0;
            while(true)
            {
                int num = this.m_rdr.Read();
                switch(num)
                {
                    case -1:
                        goto label_7;
                    case 133:
                    case 8232:
                    case 8233:
                    case 10:
                        goto label_4;
                    case 13:
                        goto label_1;
                    default:
                        this.m_sb.Append((char)num);
                        continue;
                }
            }
        label_1:
            if(this.m_rdr.Peek() == 10)
                this.m_rdr.Read();
            return ((object)this.m_sb).ToString();
        label_4:
            return ((object)this.m_sb).ToString();
        label_7:
            if(this.m_sb.Length > 0)
                return ((object)this.m_sb).ToString();
            else
                return (string)null;
        }
    }
}
