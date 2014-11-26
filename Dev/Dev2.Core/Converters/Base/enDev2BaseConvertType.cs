/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Dev2.Converters
{
    /// <summary>
    ///     The base convert types available in the system
    /// </summary>
    public enum enDev2BaseConvertType
    {
        [Description("Text")] Text,

        [Description("Binary")] Binary,

        [Description("Hex")] Hex,

        [Description("Base 64")] Base64
    }
}