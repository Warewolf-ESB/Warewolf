
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.CustomControls.Utils
{
    public static class WatermarkSential
    {
        /// <summary>
        /// This value is only set when scrolling a designer
        /// The reason this exist is due to the fact that when a designer is loaded not
        /// all water marks are updated. Only on scroll, hence why this is static. 
        /// You can only scroll 1 designer at a time ;)
        /// </summary>
        public static bool IsWatermarkBeingApplied { get; set; }
    }
}
