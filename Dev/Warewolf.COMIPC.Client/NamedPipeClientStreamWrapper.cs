#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
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

        public Stream GetInternalStream() => _pipeClientStream;

        public void Connect()
        {
            try
            {
                _pipeClientStream.Connect(30000);
                ReadMode = PipeTransmissionMode.Message;
            }
            catch (TimeoutException e)
            {
                Dev2Logger.Warn("Error connecting to COM Named Pipe Client Stream. " + e.Message, GlobalConstants.WarewolfWarn);
            }
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