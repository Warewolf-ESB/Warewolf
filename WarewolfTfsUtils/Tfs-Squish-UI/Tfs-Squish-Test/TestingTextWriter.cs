using System;
using System.IO;
using System.Text;

namespace Tfs_Squish_Test
{
    internal class TestingTextWriter : TextWriter
    {
        private StringBuilder _data = new StringBuilder();

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void Write(char value)
        {
            _data.Append(value);
        }

        public override void Write(int value)
        {
            _data.Append(value);
        }

        public override void Write(string value)
        {
            _data.Append(value);
        }

        public override void Write(object value)
        {
            _data.Append(value);
        }

        public string FetchContents()
        {
            return _data.ToString();
        }
    }
}
