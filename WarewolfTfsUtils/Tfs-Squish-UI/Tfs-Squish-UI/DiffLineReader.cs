using System;
using System.IO;
using System.Text;

namespace Tfs.Squish
{
    internal class DiffLineReader : IDisposable
    {
        private const int _lineSeparator = 8232;
        private const int _paragraphSeparator = 8233;
        private const int _nextLine = 133;
        private readonly TextReader m_rdr;
        private readonly StringBuilder m_sb;

        internal DiffLineReader(TextReader rdr)
        {
            m_rdr = rdr;
            m_sb = new StringBuilder();
        }

        public void Dispose()
        {
            m_rdr.Dispose();
        }

        internal string ReadLine()
        {
            m_sb.Length = 0;
            while(true)
            {
                int num = m_rdr.Read();
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
                        m_sb.Append((char)num);
                        continue;
                }
            }
        label_1:
            if(m_rdr.Peek() == 10)
                m_rdr.Read();
            return ((object)m_sb).ToString();
        label_4:
            return ((object)m_sb).ToString();
        label_7:
            if(m_sb.Length > 0)
                return ((object)m_sb).ToString();
            else
                return (string)null;
        }
    }
}
