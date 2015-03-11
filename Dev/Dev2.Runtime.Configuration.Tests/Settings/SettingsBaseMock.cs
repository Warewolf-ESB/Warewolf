
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
using Dev2.Runtime.Configuration.Settings;

namespace Dev2.Runtime.Configuration.Tests.Settings
{
    public class SettingsBaseMock : SettingsBase
    {
        public SettingsBaseMock(string settingName, string displayName, string webServerUri)
            : base(settingName, displayName,webServerUri)
        {
        }

        public SettingsBaseMock(XElement xml,string webServerUri)
            : base(xml,webServerUri)
        {
        }
    }
}
