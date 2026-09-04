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
    /// The server listens on a random OS-assigned port by default (see <see cref="Port"/>); pass a
    /// port to the constructor to bind a fixed one, which <see cref="InProcess.SmtpEmulator"/> needs
    /// because that port has to be baked into a committed .bite EmailSource fixture.
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

        /// <summary>
        /// The raw argument of the last accepted AUTH command (mechanism plus base64 blob), or null
        /// when the client never authenticated. Lets a test assert the AUTH leg actually happened
        /// instead of inferring it from a successful send.
        /// </summary>
        public string? AuthenticatedAs { get; private set; }

        /// <param name="port">
        /// TCP port to bind on loopback, or 0 (the default) to let the OS assign one.
        /// </param>
        public FakeSmtpServer(int port = 0)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
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
                                // AUTH PLAIN is advertised - and PLAIN only - because MailKit refuses
                                // to authenticate against a server advertising no mechanism it
                                // supports, and DsfSendEmailActivity ALWAYS authenticates: a
                                // non-empty FromAccount overwrites runtimeSource.UserName (see
                                // DsfSendEmailActivity.SendEmail) and EmailSource.Send then calls
                                // client.Authenticate for any non-empty UserName, while an empty
                                // From address throws before the send. PLAIN is a single AUTH command
                                // carrying one base64 blob, so it needs no 334-challenge state
                                // machine here, unlike LOGIN.
                                await WriteLineAsync("250-localhost Hello\r\n250-SIZE 10240000\r\n250-AUTH PLAIN\r\n250 OK");
                            }
                            else if (upper.StartsWith("AUTH"))
                            {
                                // Any credential is accepted: this stub proves the transaction
                                // completes, never that a password is correct.
                                AuthenticatedAs = line.Length > 5 ? line[5..] : string.Empty;
                                await WriteLineAsync("235 2.7.0 Authentication successful");
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

        /// <summary>
        /// Covers the AUTH leg of <see cref="FakeSmtpServer"/>, which the round-trip fidelity
        /// sweep's generated Send Email fixture depends on and neither test above reaches: both
        /// write <c>UserName=</c>, and <see cref="EmailSource.Send"/> only authenticates for a
        /// non-empty UserName.
        ///
        /// It is not an optional path there. <c>DsfSendEmailActivity.SendEmail</c> copies a
        /// non-empty FromAccount onto <c>runtimeSource.UserName</c> before sending, and the
        /// activity needs a non-empty FromAccount to have a legal FROM address at all - so the
        /// fixture always authenticates, and against a server advertising no mechanism MailKit
        /// throws <c>NotSupportedException</c> instead of sending. Asserting the AUTH command was
        /// actually received (not just that the message arrived) is what distinguishes this from
        /// the unauthenticated case.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_Email")]
        public async Task TC_EmailSource_AuthenticatedSend_CompletesAuthLeg()
        {
            using var smtp = new FakeSmtpServer();

            var dir = Path.Combine(Path.GetTempPath(), $"live-email-auth-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);

            var id = Guid.NewGuid();
            var connStr = $"Host=127.0.0.1;Port={smtp.Port};EnableSsl=false;Timeout=10000;UserName=sender@test.local;Password=any-secret";
            File.WriteAllText(Path.Combine(dir, $"{id:N}.bite"),
                $"""<Source Type="EmailSource" ResourceID="{id}" ID="{id}" Name="FakeEmailAuth" ResourceType="EmailSource" IsValid="false" ConnectionString="{connStr}" />""");

            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = loader;
            Assert.IsTrue(iLoader.EnsureSourceLoaded(id), "Source should load from .bite");

            var source = GetFromCatalog<Dev2.Runtime.ServiceModel.Data.EmailSource>(id);
            Assert.IsNotNull(source, "EmailSource must be in ResourceCatalog after load");
            Assert.AreEqual("sender@test.local", source.UserName,
                "a non-empty UserName is what makes EmailSource.Send authenticate");

            var msg = new MailMessage("sender@test.local", "to@test.local", "Auth Subject", "Auth Body");
            source.Send(msg);

            // Allow the fake server a brief moment to process the received data.
            await Task.Delay(200);

            Assert.IsNotNull(smtp.AuthenticatedAs,
                "the client must have authenticated - if FakeSmtpServer stops advertising a " +
                "mechanism MailKit supports, Send throws NotSupportedException instead and the " +
                "fidelity sweep's Send Email row regresses to PassBothFailedIdentically");
            StringAssert.StartsWith(smtp.AuthenticatedAs, "PLAIN",
                "PLAIN is the only mechanism the stub advertises, and the only one whose exchange " +
                "it can complete without a 334-challenge state machine");
            Assert.IsTrue(smtp.ReceivedMessage,
                "the message must still be accepted after the AUTH leg");
        }
    }
}
