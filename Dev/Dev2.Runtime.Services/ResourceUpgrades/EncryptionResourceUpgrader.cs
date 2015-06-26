
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common.Interfaces.Data;
using Dev2.Warewolf.Security.Encryption;
using System.Collections.Generic;

namespace Dev2.Runtime.ResourceUpgrades
{
    public class EncryptionResourceUpgrader : IResourceUpgrade
    {
        readonly Dictionary<string, StringTransform> _replacements = new Dictionary<string, StringTransform>();

        void BuildReplacements()
        {
            _replacements.Add("Source", new StringTransform
            {
                SearchRegex = new Regex(@"<Source ID=""[a-e0-9\-]+"" .*ConnectionString=""([^""]+)"" .*>"),
                GroupNumbers = new[] { 1 },
                TransformFunction = DpapiWrapper.Encrypt
            }
                );
            _replacements.Add(
            "DsfAbstractFileActivity", new StringTransform
            {
                SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfFileWrite|DsfFileRead|DsfFolderRead|DsfPathCopy|DsfPathCreate|DsfPathDelete|DsfPathMove|DsfPathRename|DsfZip|DsfUnzip) .*?Password=""([^""]+)"" .*?&gt;"),
                GroupNumbers = new[] { 3 },
                TransformFunction = DpapiWrapper.Encrypt
            }
            );
            _replacements.Add(
            "DsfAbstractMultipleFilesActivity", new StringTransform
            {
                SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfPathCopy|DsfPathMove|DsfPathRename|DsfZip|DsfUnzip) .*?DestinationPassword=""([^""]+)"" .*?&gt;"),
                GroupNumbers = new[] { 3 },
                TransformFunction = DpapiWrapper.Encrypt
            }
            );
            _replacements.Add(
            "Zip", new StringTransform
            {
                SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfZip|DsfUnzip) .*?ArchivePassword=""([^""]+)"" .*?&gt;"),
                GroupNumbers = new[] { 3 },
                TransformFunction = DpapiWrapper.Encrypt
            }
            );
        }

        #region Implementation of IResourceUpgrade
        public EncryptionResourceUpgrader()
        {
            UpgradeFunc = Upgrade;
            BuildReplacements();
        }

        XElement Upgrade(XElement arg)
        {
            string xml = arg.ToString();
            xml = EncryptPasswordsAndConnectionStrings(xml);
            return XElement.Parse(xml);
        }

        public Func<XElement, XElement> UpgradeFunc { get; private set; }

        #endregion

        public string EncryptSourceConnectionStrings(string xml)
        {
            return StringTransform.TransformAllMatches(xml, new List<StringTransform> { _replacements["Source"] });
        }

        public string EncryptDsfFileWritePasswords(string xml)
        {
            return StringTransform.TransformAllMatches(xml, new List<StringTransform> { _replacements["DsfAbstractFileActivity"] });
        }

        public string EncryptPasswordsAndConnectionStrings(string xml)
        {
            return EncryptSourceConnectionStrings(
                EncryptDsfFileWritePasswords(
                xml)
                );
        }

        class StringTransform
        {
            public Regex SearchRegex { private get; set; }
            public int[] GroupNumbers { private get; set; }
            public Func<string, string> TransformFunction { private get; set; }

            // ReSharper disable once ParameterTypeCanBeEnumerable.Local
            public static string TransformAllMatches(string initial, List<StringTransform> transforms)
            {
                StringBuilder result = new StringBuilder(initial);
                foreach (StringTransform transform in transforms)
                {
                    Regex regex = transform.SearchRegex;
                    int[] groupNumbers = transform.GroupNumbers;
                    MatchCollection matches = regex.Matches(initial);
                    if (matches.Count == 0) continue;
                    StringBuilder encrypted = new StringBuilder();
                    foreach (Match match in matches)
                    {
                        result.Remove(match.Index, match.Length);
                        encrypted.Clear();
                        encrypted.Append(match.Value);
                        foreach (int groupNumber in groupNumbers)
                        {
                            Group group = match.Groups[groupNumber];
                            int indexInMatch = group.Index - match.Index;
                            encrypted.Remove(indexInMatch, group.Length);
                            encrypted.Insert(indexInMatch, transform.TransformFunction(group.Value));
                        }
                        result.Insert(match.Index, encrypted.ToString());
                    }
                }
                return result.ToString();
            }
        }
    }
}
