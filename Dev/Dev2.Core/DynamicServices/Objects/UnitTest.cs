/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices.Objects.Base;

namespace Dev2.DynamicServices
{
    public class UnitTest : DynamicServiceObjectBase
    {
        public UnitTest() : base(enDynamicServiceObjectType.UnitTest)
        {
        }

        public string ServiceName { get; set; }
        public string InputXml { get; set; }
        public string RequiredTagName { get; set; }
        public string ValidationExpression { get; set; }
    }
}