
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpFramework.Exceptions;
using HttpFramework.Helpers;
using HttpFramework.Sessions;

namespace HttpFramework.HttpModules
{
    /// <summary>
    /// Implementation of the SCGI protocol.
    /// </summary>
    /// <remarks>
    /// <para>The implementation reads configuration from an INI file called "scgi.ini", it should
    /// be placed in the same folder as the server. The file should contain file mappings to
    /// socket severs: 
    /// <example>
    /// [main]
    /// php: localhost:4082
    /// py: 192.168.1.20:8037
    /// </example>
    /// </para>
    /// </remarks>
    public class SimpleCommonGatewayInterfaceModule : HttpModule
    {
        private readonly Dictionary<string, IPEndPoint> _servers = new Dictionary<string, IPEndPoint>();

        /// <summary>
        /// Method that process the url
        /// </summary>
        /// <param name="request">Information sent by the browser about the request</param>
        /// <param name="response">Information that is being sent back to the client.</param>
        /// <param name="session">Session used to</param>
        /// <returns>true if this module handled the request.</returns>
        public override bool Process(IHttpRequest request, IHttpResponse response, IHttpSession session)
        {
            int pos = request.Uri.AbsolutePath.LastIndexOf('.');
            if (pos == -1)
                return false;

            string extension = request.Uri.AbsolutePath.Substring(pos + 1);

            IPEndPoint endPoint;
            lock (_servers)
                if (!_servers.TryGetValue(extension, out endPoint))
                    return false;


            // Build header.
            var sb = new StringBuilder();
            sb.AppendFormat("CONTENT_LENGTH: {0}\r\n", request.Body.Length);
            sb.AppendLine("SCGI: 1");
            sb.AppendLine("REQUEST_METHOD: " + request.Method.ToUpper());
            sb.AppendLine("REQUEST_URI: " + request.Uri.OriginalString);
            foreach (string header in request.Headers)
            {
                if (header.ToLower() == "content_length")
                    continue;
                sb.AppendLine(header.ToUpper() + ": " + request.Headers[header]);
            }

            // separator between header and body
            sb.AppendLine();

            // Add body
            var reader = new StreamReader(request.Body);
            sb.Append(reader.ReadToEnd());

            // and get bytes that we can send.
            byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());


            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(endPoint);
            }
            catch (SocketException err)
            {
                throw new InternalServerException("Failed to connect to SCGI server, try again later", err);
            }

            try
            {
                socket.Send(buffer);
            }
            catch (SocketException err)
            {
                throw new InternalServerException("Failed to send request to SCGI server.", err);
            }

            // Loop until we got complete header.
            // Send a correct HTTP header
            sb.Length = 0;
            try
            {
                var readbuffer = new byte[8192];
                while (true)
                {
                    int bytes = socket.Receive(readbuffer);
                    if (bytes == 0)
                        break;
                }
            }
            catch (SocketException err)
            {
                throw new InternalServerException("Failed to read response from SCGI server.", err);
            }

            // And loop here until complete body is sent.
            // Need to do it in two steps so we doesnt store the entire
            // body in memory.


            return true;
        }

        private void ReadConfig()
        {
            if (!File.Exists("scgi.ini"))
                return;

            IniFile ini = IniFile.Load("scgi.ini");
            foreach (var parameter in ini["main"])
            {
                // Get port number.
                int index = parameter.Value.IndexOf(":");
                if (index == -1)
                    throw new FormatException("Hostname is missing port: " + parameter.Value);
                int port;
                if (!int.TryParse(parameter.Value.Substring(index + 1), out port))
                    throw new FormatException("Port number is not numeric: " + parameter.Value);


                string host = parameter.Value.Substring(0, index);
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                if (addresses.Length == 0)
                    throw new InvalidOperationException("Failed to find IP address of: " + host);

                _servers.Add(parameter.Key.ToLower(), new IPEndPoint(addresses[0], port));
            }

            return;
        }

        /// <summary>
        /// Start module, will try to read the config file.
        /// </summary>
        public void Start()
        {
            ReadConfig();
        }
    }
}
