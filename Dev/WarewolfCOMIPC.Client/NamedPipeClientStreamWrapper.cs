using System;
using System.IO;
using System.IO.Pipes;

namespace WarewolfCOMIPC.Client
{
    public class NamedPipeClientStreamWrapper : INamedPipeClientStreamWrapper, IDisposable
    {
        readonly NamedPipeClientStream _pipeClientStream;
        public NamedPipeClientStreamWrapper(string v, string token, PipeDirection inOut)
        {
            _pipeClientStream = new NamedPipeClientStream(v, token, inOut);
        }

        public Stream GetInternalStream()
        {
            return _pipeClientStream;
        }

        public void Connect()
        {
            _pipeClientStream.Connect();
        }

        public void Close()
        {
            _pipeClientStream.Close();
        }

        public void Dispose()
        {
            _pipeClientStream.Dispose();
        }

        public PipeTransmissionMode ReadMode
        {
            get
            {
                return _pipeClientStream.ReadMode;
            }
            set
            {
                _pipeClientStream.ReadMode = value;
            }
        }
    }
}