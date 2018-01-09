using System;
using System.IO;
using System.IO.Pipes;

namespace WarewolfCOMIPC.Client
{
    public interface INamedPipeClientStreamWrapper : IDisposable
    {
        PipeTransmissionMode ReadMode { get; set; }
        void Connect();
        void Close();
        Stream GetInternalStream();
    }
}