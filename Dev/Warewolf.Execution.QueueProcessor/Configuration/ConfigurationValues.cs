/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

namespace Warewolf.Execution.QueueProcessor.Configuration
{
    /// <summary>
    /// Reads raw configuration values with "empty means unset" semantics.
    ///
    /// This exists as its own type rather than as local helpers in <c>Program.cs</c> for two
    /// reasons: top-level statements cannot be unit tested, and the rule below caused two real
    /// defects, so it needs regression cover.
    ///
    /// <para><b>The rule:</b> a whitespace-only value is <b>absent</b>, not a value. Plain
    /// <c>??</c> only treats <c>null</c> as absent, which is wrong here on both sides of the
    /// contract: <c>appsettings.json</c> ships keys as <c>""</c> placeholders so the full
    /// contract is discoverable, and ACA/Docker turn an unset environment variable into an empty
    /// string as readily as into a missing one.</para>
    ///
    /// <para>Both defects this prevents were silent, not loud:
    /// <list type="number">
    ///   <item><c>QUEUE__SETTINGSPATH=""</c> beat the intended default and resolved the settings
    ///   tree against the process working directory instead of
    ///   <see cref="AppContext.BaseDirectory"/>.</item>
    ///   <item><c>ENGINE__TENANTID=""</c> was passed to <c>TokenRequestContext</c>, making every
    ///   credential in the chain fail with "Invalid tenant id provided" — which reads like a
    ///   missing app role rather than a missing setting. See
    ///   <see cref="QueueProcessorOptions.EffectiveTenantId"/>.</item>
    /// </list></para>
    /// </summary>
    public static class ConfigurationValues
    {
        /// <summary>
        /// Returns <paramref name="value"/> when it carries content, otherwise
        /// <paramref name="default"/>. Whitespace counts as no content.
        /// </summary>
        public static string ReadString(string? value, string @default) =>
            string.IsNullOrWhiteSpace(value) ? @default : value!;

        /// <summary>
        /// Returns <paramref name="value"/> when it carries content, otherwise <c>null</c>. Use
        /// for optional settings where callers must distinguish "unset" from "set to empty" —
        /// notably tenant/client ids handed to Azure credentials, which reject <c>""</c> but
        /// accept <c>null</c> as "infer it".
        /// </summary>
        public static string? NullIfBlank(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        /// <summary>Parses a boolean, treating anything unparseable (including blank) as false.</summary>
        public static bool ReadBool(string? value) => bool.TryParse(value, out var b) && b;

        /// <summary>Parses an int, falling back to <paramref name="default"/> when unparseable.</summary>
        public static int ReadInt(string? value, int @default) =>
            int.TryParse(value, out var i) ? i : @default;
    }
}
