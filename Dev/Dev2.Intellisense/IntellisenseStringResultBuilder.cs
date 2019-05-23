#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Dev2.Intellisense
{
    public class IntellisenseStringResultBuilder : IIntellisenseStringResultBuilder
    {
        readonly char[] _tokenisers = "!#$%^&*()_+_+{}|:\"?><`~<>?:'{}| ".ToCharArray();
        #region Implementation of IIntellisenseStringResultBuilder

        public IIntellisenseStringResult Build(string selectedOption, int originalCaret, string originalText, string editorText)
        {
            if (originalText == String.Empty || selectedOption.StartsWith(originalText))
            {
                return new IntellisenseStringResult(selectedOption, selectedOption.Length);
            }
            try
            {
#pragma warning disable 618
                var rep = IntellisenseStringProvider.doReplace(originalText, originalCaret, selectedOption);
#pragma warning restore 618
                return new IntellisenseStringResult(rep.Item1, rep.Item2);
            }
            catch (Exception)
            {
                var diff = originalCaret >= editorText.Length - 1 ? editorText : editorText.Substring(0, originalCaret + 1);
                var ignore = originalCaret - Math.Max(diff.LastIndexOf("[[", StringComparison.Ordinal), diff.LastIndexOf("]]", StringComparison.Ordinal) + 1);
                var len = originalCaret - ignore;
                var delimchar = "";
                var lastIndexOfAny = diff.LastIndexOfAny(_tokenisers);
                if (len < 0)
                {

                    if (lastIndexOfAny > 0 && !"[]".Contains(diff[lastIndexOfAny].ToString()))
                    {
                        delimchar = diff[lastIndexOfAny].ToString();
                    }
                    ignore = originalCaret - lastIndexOfAny;

                }
                len = lastIndexOfAny < 0 && diff.LastIndexOfAny("[]".ToCharArray()) < 0 ? 0 : originalCaret - ignore;
                var suffix = originalText.Substring(originalCaret);
                if (suffix.StartsWith("]]"))
                {
                    suffix = suffix.Substring(2);
                }

                var text = originalText.Substring(0, len) + delimchar + selectedOption + suffix;
                var car = (originalText.Substring(0, len) + selectedOption).Length + delimchar.Length;

                return new IntellisenseStringResult(text, car);
            }
        }

        #endregion
    }
}
