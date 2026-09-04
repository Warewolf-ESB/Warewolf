/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  In-process SMTP emulator for the round-trip fidelity sweep's "Send Email (SMTP)" fixture.
 *
 *  The generated fixture pair (Resources/tools/send email/Fidelity SMTP Source.bite +
 *  Fidelity_SendEmail.bite - see FidelityFixtureGenerator.WriteEmailSource) points a plaintext
 *  EmailSource at localhost:<Port>, so DsfSendEmailActivity completes a real SMTP transaction
 *  instead of failing on an unreachable host. Without it the row could only ever reach
 *  PassBothFailedIdentically ("Invalid URI: The hostname could not be parsed." - the only real
 *  corpus samples carry EmailSources with no host), which proves the round trip preserved
 *  behaviour but not that the activity's own logic survived it.
 *
 *  Same shape and lifecycle as HttpbinEmulator/ElasticsearchEmulator: a plain static helper driven
 *  once per assembly by IntegrationTestAssemblyInit, since MSTest permits only one
 *  [AssemblyInitialize]. That covers both CI jobs this assembly is partitioned between, so no
 *  TestRun.ps1 -Start* flag, container or pipeline argument is needed for the email path.
 */

namespace Warewolf.Execution.Lightweight.Integration.Tests.InProcess
{
    /// <summary>
    /// Assembly-wide fake SMTP endpoint listening on the port baked into the generated
    /// EmailSource fixture (<see cref="Port"/>).
    /// </summary>
    internal static class SmtpEmulator
    {
        /// <summary>
        /// Port the emulator binds on loopback. MUST match the Port= in the committed
        /// 'Fidelity SMTP Source.bite' - FidelityFixtureGenerator reads this constant when it
        /// writes that fixture, and
        /// FidelityFixtureGenerator.CommittedEmailSourceFixture_PointsAtTheSmtpEmulatorPort
        /// fails if the two ever drift.
        ///
        /// 2525 rather than 25: binding the privileged port needs elevation on some hosts, and
        /// 25 is exactly the port a real local mail service (or Warewolf.Tools.Specs'
        /// SimpleSmtpServer.Start(25)) would already hold.
        /// </summary>
        public const int Port = 2525;

        /// <summary>Host the fixture's EmailSource resolves. Loopback only - nothing is delivered.</summary>
        public const string Host = "localhost";

        static FakeSmtpServer? _server;

        public static void Start()
        {
            if (_server != null)
            {
                return;
            }

            _server = new FakeSmtpServer(Port);
        }

        public static void Stop()
        {
            _server?.Dispose();
            _server = null;
        }
    }
}
