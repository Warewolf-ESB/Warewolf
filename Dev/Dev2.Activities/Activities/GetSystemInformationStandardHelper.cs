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
using Dev2.Common;


namespace Dev2.Activities
{
    public class GetSystemInformationStandardHelper: GetSystemInformationHelper
    {
        public override string GetFullDateTimeInformation()
        {
            return DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat);
        }

        public override string GetDateTimeFormatInformation()
        {
            return GlobalConstants.Dev2DotNetDefaultDateTimeFormat;
        }
    }
}
