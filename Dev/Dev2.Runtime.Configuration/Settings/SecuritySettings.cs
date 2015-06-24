
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class SecuritySettings : SettingsBase
    {
        public new const string SettingName = "Security";
        
        #region CTOR

        public SecuritySettings(string webServerUri)
            : base(SettingName, "Security",webServerUri)
        {
        }

        public SecuritySettings(XElement xml, string webServerUri)
            : base(xml,webServerUri)
        {
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();

            return result;
        }

        #endregion
    }
}
