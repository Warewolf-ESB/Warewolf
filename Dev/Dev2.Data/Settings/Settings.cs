/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Services.Security;

namespace Dev2.Data.Settings
{
    public class Settings
    {
        private SecuritySettingsTO _security;
        public SecuritySettingsTO Security
        {
            get
            {
                return _security;
            }
            set
            {
                _security = value;
            }
        }
        public LoggingSettingsTo Logging { get; set; }

        public bool HasError { get; set; }
        public string Error { get; set; }
        public IPerformanceCounterTo PerfCounters { get; set; }

        public override string ToString()
        {
            var serializer = new Dev2JsonSerializer();
            return serializer.Serialize(this);
        }
    }
}
