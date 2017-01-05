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
using System.Configuration;

namespace Dev2.Util
{
    public class AppSettings
    {
        public static string LocalHost { get; set; }

        static string _serviceName;
        public static string ServiceName => _serviceName ?? (_serviceName = ConfigurationManager.AppSettings["ServiceName"] ?? "Warewolf Server");

        public static bool CollectUsageStats
        {
            get
            {
                bool collectUsageStats;
                Boolean.TryParse(ConfigurationManager.AppSettings["CollectUsageStats"], out collectUsageStats);
                return collectUsageStats;
            }
        }
        public static string ServicesAddress => LocalHost + "/wwwroot/services/Service/Resources/{0}";
    }
}
