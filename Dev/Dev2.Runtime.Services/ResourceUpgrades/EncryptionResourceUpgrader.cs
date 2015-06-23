
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
using Dev2.Runtime.Services.Security;

namespace Dev2.Runtime.ResourceUpgrades
{
    public class EncryptionResourceUpgrader : IResourceUpgrade
    {
        #region Implementation of IResourceUpgrade
        public EncryptionResourceUpgrader()
        {
            UpgradeFunc = Upgrade;
        }

        XElement Upgrade(XElement arg)
        {
            string xml = arg.ToString();
            xml = EncryptConnectionStrings(xml);
            return XElement.Parse(xml);
        }

        public string EncryptConnectionStrings(string xml)
        {
            var connectionStringRegex = new Regex(@"<Source ID=""[a-e0-9\-]+"" .*ConnectionString=""([^""]+)"" .*>");
            MatchCollection matches = connectionStringRegex.Matches(xml);
            if (matches.Count == 0) return xml;
            StringBuilder result = new StringBuilder(xml);
            StringBuilder encrypted = new StringBuilder();
            foreach (Match match in matches)
            {
                result.Remove(match.Index, match.Length);
                encrypted.Clear();
                encrypted.Append(match.Value);
                Group group = match.Groups[1];
                int indexInMatch = group.Index - match.Index;
                encrypted.Remove(indexInMatch, group.Length);
                encrypted.Insert(indexInMatch, DPAPIWrapper.Encrypt(group.Value));
                result.Insert(match.Index, encrypted.ToString());
            }
            return result.ToString();
        }

        public Func<XElement, XElement> UpgradeFunc { get; private set; }

        #endregion
    }
}
