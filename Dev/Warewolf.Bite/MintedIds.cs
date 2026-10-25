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

namespace Warewolf.Bite
{
    /// <summary>
    /// An activity whose XAML carries no UniqueID gets an id minted at load: a random Guid per load on the WF path
    /// (DsfNativeActivity's constructor), a stable derived one from <see cref="BiteReader"/>. Such ids - and
    /// references to them - legitimately differ between readers, so a comparison ignores them, but only when
    /// neither value was stored in the resource.
    /// </summary>
    public static class MintedIds
    {
        public static bool IsMintedIdDifference(GraphDifference difference, string source)
        {
            if (!difference.Path.EndsWith(".UniqueID", StringComparison.Ordinal) && !difference.Path.EndsWith(".$ref", StringComparison.Ordinal))
            {
                return false;
            }
            return !Stored(difference.Expected, source) && !Stored(difference.Actual, source);
        }

        static bool Stored(string? value, string source)
        {
            var id = value?.Trim('"');
            return !string.IsNullOrEmpty(id) && source.Contains(id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
