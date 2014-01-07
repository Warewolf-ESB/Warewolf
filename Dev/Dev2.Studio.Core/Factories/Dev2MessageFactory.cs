using System.Collections.Generic;

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
