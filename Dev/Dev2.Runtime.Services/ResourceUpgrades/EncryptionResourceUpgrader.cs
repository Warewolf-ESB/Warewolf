/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Utils;
using System.Collections.Generic;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ResourceUpgrades
{
    public class EncryptionResourceUpgrader : IResourceUpgrade
    {
        readonly Dictionary<string, StringTransform> _replacements = new Dictionary<string, StringTransform>();

        void BuildReplacements()
        {
            _replacements.Add("Source", new StringTransform
            {
                SearchRegex = new Regex(@"<Source ID=""[a-fA-F0-9\-]+"" .*ConnectionString=""([^""]+)"" .*>"),
                GroupNumbers = new[] { 1 },
                TransformFunction = DpapiWrapper.EncryptIfDecrypted
            }
                );
            _replacements.Add(
            "DsfAbstractFileActivity", new StringTransform
            {
                SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfFileWrite|DsfFileRead|DsfFolderRead|DsfPathCopy|DsfPathCreate|DsfPathDelete|DsfPathMove|DsfPathRename|DsfZip|DsfUnzip) .*?Password=""([^""]+)"" .*?&gt;"),
                GroupNumbers = new[] { 3 },
                TransformFunction = DpapiWrapper.EncryptIfDecrypted
            }
            );
            _replacements.Add(
            "DsfAbstractMultipleFilesActivity", new StringTransform
            {
                SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfPathCopy|DsfPathMove|DsfPathRename|DsfZip|DsfUnzip) .*?DestinationPassword=""([^""]+)"" .*?&gt;"),
                GroupNumbers = new[] { 3 },
                TransformFunction = DpapiWrapper.EncryptIfDecrypted
            }
            );
            _replacements.Add(
            "Zip", new StringTransform
            {
                SearchRegex = new Regex(@"&lt;([a-zA-Z0-9]+:)?(DsfZip|DsfUnzip) .*?ArchivePassword=""([^""]+)"" .*?&gt;"),
                GroupNumbers = new[] { 3 },
                TransformFunction = DpapiWrapper.EncryptIfDecrypted
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


    }
}
