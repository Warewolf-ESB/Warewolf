using System.IO.Pipes;

namespace WarewolfCOMIPC.Client
{
    public interface INamedPipeClientStreamWrapper
    {
        void Dispose();
        PipeTransmissionMode ReadMode { get; set; }
        void Connect();
        void Close();
        NamedPipeClientStream GetInternalStream();
    }
}