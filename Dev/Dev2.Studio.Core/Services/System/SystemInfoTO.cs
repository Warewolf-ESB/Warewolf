
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Services.System
{

    /// <summary>
    /// Transfer object returned from the System info Service
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <datetime>2013/01/14-09:06 AM</datetime>
    public class SystemInfoTO
    {
        public string Name { get; set; }
        public string Edition { get; set; }
        public string ServicePack { get; set; }
        public string Version { get; set; }
        public string OsBits { get; set; }
        public int ApplicationExecutionBits { get; set; }
    }
}
