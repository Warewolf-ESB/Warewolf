
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Factories
{
    public static class Dev2MessageFactory
    {

        public static string CreateStringFromListWithLabel(string labelName, IList<string> stringToJoin, string charsToSeperateWith = ", ")
        {
            string resultString = string.Empty;
            //Extract to message factory
            if(stringToJoin.Count > 0)
            {
                resultString = " \r\n " + labelName + ": ";
                resultString += (string.Join(charsToSeperateWith, stringToJoin));
                resultString += "\r\n-------------------------------------------------";
            }
            return resultString;
        }
    }
}
