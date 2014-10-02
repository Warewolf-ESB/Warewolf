
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.PathOperations;

namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public interface IDateTimeOperationTO : IResult
    {
        string InputFormat { get; set; }
        string OutputFormat { get; set; }
        string DateTime { get; set; }
        //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
        string TimeModifierType { get; set; }
        int TimeModifierAmount { get; set; }
    }
}
