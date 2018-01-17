using System.IO;

namespace WarewolfCOMIPC.Client
{
    public class StreamReaderWrapper
    {
        
        private StreamReader _StreamReader;

        public StreamReaderWrapper(Stream stream)
        {
            _StreamReader = new StreamReader(stream);
        }

      
    }
}