/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Provides a minimal in-process SMTP server that accepts any valid SMTP transaction and
    /// records the raw SMTP commands it receives. Modelled after the smtp4dev/MailHog pattern
    /// (fake SMTP endpoint that captures messages without delivering them).
    ///
    /// The server listens on a random OS-assigned port (see <see cref="Port"/>).
    /// Dispose to stop it.
    /// </summary>
    internal sealed class FakeSmtpServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();

        public int Port { get; }

        /// <summary>Raw SMTP commands received across all client sessions (thread-safe add).</summary>
        public List<string> ReceivedCommands { get; } = new();

        /// <summary>Set to true after a complete DATA session has been accepted.</summary>
        public bool ReceivedMessage { get; private set; }

        public FakeSmtpServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(ct).ConfigureAwait(false);
                    _ = Task.Run(() => HandleClientAsync(client, ct), ct);
                }
                catch (OperationCanceledException) { break; }
                catch { /* ignore listener errors on shutdown */ }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            try
            {
                using var _ = client;
                var stream = client.GetStream();

                async Task WriteLineAsync(string s)
                {
                    var bytes = Encoding.ASCII.GetBytes(s + "\r\n");
                    await stream.WriteAsync(bytes, ct).ConfigureAwait(false);
                }

                // Greeting
                await WriteLineAsync("220 localhost FakeSmtpServer ready");

                var buffer = new byte[8192];
                var sb = new StringBuilder();

                bool inData = false;

                while (!ct.IsCancellationRequested)
                {
                    int read = await stream.ReadAsync(buffer, ct).ConfigureAwait(false);
                    if (read == 0) break;

                    sb.Append(Encoding.ASCII.GetString(buffer, 0, read));
                    string accumulated = sb.ToString();

                    while (true)
                    {
                        int idx = accumulated.IndexOf("\r\n", StringComparison.Ordinal);
                        if (idx < 0) break;

                        string line = accumulated[..idx];
                        accumulated = accumulated[(idx + 2)..];

                        lock (ReceivedCommands)
                            ReceivedCommands.Add(line);

                        if (inData)
                        {
                            if (line == ".")
                            {
                                ReceivedMessage = true;
                                inData = false;
                                await WriteLineAsync("250 Message queued");
                            }
                            // else: body line, keep reading
                        }
                        else
                        {
                            var upper = line.ToUpperInvariant();
                            if (upper.StartsWith("EHLO") || upper.StartsWith("HELO"))
                            {
                                await WriteLineAsync("250-localhost Hello\r\n250-SIZE 10240000\r\n250 OK");
                            }
                            else if (upper.StartsWith("MAIL FROM"))
                                await WriteLineAsync("250 OK");
                            else if (upper.StartsWith("RCPT TO"))
                                await WriteLineAsync("250 OK");
                            else if (upper == "DATA")
                            {
                                inData = true;
                                await WriteLineAsync("354 End data with <CR><LF>.<CR><LF>");
                            }
                            else if (upper.StartsWith("QUIT"))
                            {
                                await WriteLineAsync("221 Bye");
                                return;
                            }
                            else
                                await WriteLineAsync("250 OK");
                        }
                    }

                    sb.Clear();
                    sb.Append(accumulated);
                }
            }
            catch (OperationCanceledException) { }
            catch { /* ignore per-client errors */ }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
        }
    }

    /// <summary>
    /// Live integration tests for <see cref="Dev2.Runtime.ServiceModel.Data.EmailSource"/>
    /// loaded via <see cref="LightweightSourceLoader"/>.
    ///
    /// All tests use <see cref="FakeSmtpServer"/> — an in-process TCP SMTP acceptor that
    /// captures SMTP commands without delivering them.  This is the smtp4dev/MailHog model
    /// (fake local SMTP endpoint), but fully in-process so it works in any CI environment
    /// that supports .NET 8 without requiring Docker.
    /// </summary>
    [TestClass]
    public class LiveEmailSourceTests
    {
        private readonly List<string> _tempDirs = new();

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var d in _tempDirs)
                try { Directory.Delete(d, true); } catch { }
            _tempDirs.Clear();
            Dev2.Runtime.Interfaces.AmbientSourceLoader.Clear();
        }

        private string WriteBite(string dir, Guid id, int smtpPort)
        {
            var connStr = $"Host=127.0.0.1;Port={smtpPort};EnableSsl=false;Timeout=10000;UserName=;Password=";
            var xml = $"""<Source Type="EmailSource" ResourceID="{id}" ID="{id}" Name="FakeEmail" ResourceType="EmailSource" IsValid="false" ConnectionString="{connStr}" />""";
            var path = Path.Combine(dir, $"{id:N}.bite");
            File.WriteAllText(path, xml);
            return path;
        }

        private static T? GetFromCatalog<T>(Guid id) where T : class, Dev2.Common.Interfaces.Data.IResource
        {
            if (!Dev2.Runtime.Hosting.ResourceCatalog.Instance.WorkspaceResources
                    .TryGetValue(Dev2.Common.GlobalConstants.ServerWorkspaceID, out var ws))
                return null;
            lock (ws)
                return ws.OfType<T>().FirstOrDefault(r => r.ResourceID == id);
        }

        /// <summary>
        /// End-to-end: LightweightSourceLoader loads the .bite file → source registered in
        /// ResourceCatalog → EmailSource.Send() delivers a test message to FakeSmtpServer →
        /// FakeSmtpServer confirms receipt via SMTP DATA exchange.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_Email")]
        public async Task TC_EmailSource_SendsToFakeSmtpServer()
        {
            using var smtp = new FakeSmtpServer();

            var dir = Path.Combine(Path.GetTempPath(), $"live-email-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);

            var id = Guid.NewGuid();
            WriteBite(dir, id, smtp.Port);

            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = loader;
            Assert.IsTrue(iLoader.EnsureSourceLoaded(id), "Source should load from .bite");

            var source = GetFromCatalog<Dev2.Runtime.ServiceModel.Data.EmailSource>(id);
            Assert.IsNotNull(source, "EmailSource must be in ResourceCatalog after load");
            Assert.AreEqual("127.0.0.1", source.Host, "Host should be 127.0.0.1");
            Assert.AreEqual(smtp.Port, source.Port, "Port should match FakeSmtpServer port");

            var msg = new MailMessage("from@test.local", "to@test.local", "Subject Test", "Body Test");
            source.Send(msg);

            // Allow the fake server a brief moment to process the received data.
            await Task.Delay(200);

            Assert.IsTrue(smtp.ReceivedMessage, "FakeSmtpServer should have accepted a complete SMTP message");
        }

        /// <summary>
        /// Verifies that the EmailSource properties (Host, Port, EnableSsl, Timeout)
        /// survive the .bite serialisation / deserialisation round-trip through
        /// LightweightSourceLoader without any data loss or default-value override.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_Email")]
        public void TC_EmailSource_PropertiesRoundTrip()
        {
            using var smtp = new FakeSmtpServer();

            var dir = Path.Combine(Path.GetTempPath(), $"live-email-rt-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);

            var id = Guid.NewGuid();
            WriteBite(dir, id, smtp.Port);

            IOnDemandSourceLoader loader = LightweightSourceLoader.Instance;
            LightweightSourceLoader.Instance.EnsureIndexed(dir);
            loader.EnsureSourceLoaded(id);

            var source = GetFromCatalog<Dev2.Runtime.ServiceModel.Data.EmailSource>(id);

            Assert.IsNotNull(source);
            Assert.AreEqual("127.0.0.1", source.Host);
            Assert.AreEqual(smtp.Port, source.Port);
            Assert.IsFalse(source.EnableSsl, "EnableSsl should be false");
            Assert.AreEqual(10000, source.Timeout, "Timeout should be 10000ms");
        }
    }
}
