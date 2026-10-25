/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text.Json.Nodes;

namespace Warewolf.Bite
{
    /// <summary>
    /// A secret property (DsfSendEmailActivity.Password, the file activities' passwords, ...) encrypts in its
    /// setter whenever this machine cannot decrypt the value it is given (DataListUtil.ShouldEncrypt). A secret
    /// stored by another machine, or stored as plain text, is therefore encrypted again on every load: by DPAPI,
    /// or by the AES hook (<c>WFAES::</c>) where one is registered, and both are randomised per call. Such
    /// values legitimately differ between readers, so a comparison ignores them, but only when both sides are
    /// ciphertext and neither was stored in the resource.
    /// </summary>
    public static class ReencryptedSecrets
    {
        /// <summary>Base64 of the DPAPI blob header (version 1 and the DPAPI provider GUID).</summary>
        const string DpapiPrefix = "AQAAANCMnd8BFdERjHoAwE/Cl+s";
        /// <summary>What <c>DpapiWrapper.AesEncryptHook</c> output starts with (FileDecryptionHelper.WfAesPrefix).</summary>
        const string WfAesPrefix = "WFAES::";

        public static bool IsReencryptionDifference(GraphDifference difference, string source)
        {
            var expected = Unquote(difference.Expected);
            var actual = Unquote(difference.Actual);
            return IsCiphertext(expected) && IsCiphertext(actual)
                && !source.Contains(expected!, StringComparison.Ordinal) && !source.Contains(actual!, StringComparison.Ordinal);
        }

        static bool IsCiphertext(string? value) =>
            value != null && (value.StartsWith(DpapiPrefix, StringComparison.Ordinal) || value.StartsWith(WfAesPrefix, StringComparison.Ordinal));

        /// <summary>A difference carries JSON text (quoted, with <c>+</c> escaped); anything else is not a string value.</summary>
        static string? Unquote(string? json)
        {
            if (json == null || !json.StartsWith('"'))
            {
                return null;
            }
            try
            {
                return JsonNode.Parse(json)?.GetValue<string>();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
