
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
namespace Dev2.Studio.Core.Interfaces
{
    public interface IOperatorType
    {
        string OperatorName { get; set; }
        string FriendlyName { get; set; }
        string OperatorSymbol { get; set; }
        dynamic Parent { get; set; }
        string TagName { get; set; }
        object Value { get; set; }
        object EndValue { get; set; }
        bool Selected { get; set; }
        bool ShowEndValue { get; set; }
        string Expression { get; }
        bool IsValid { get; }
    }
}
