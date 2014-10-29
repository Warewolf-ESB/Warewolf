/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;

namespace Dev2
{
    public static class Exceptions
    {
        public static void ThrowArgumentNullExceptionIfObjectIsNull(string objectName, object objectValue)
        {
            if (objectValue == null)
            {
                throw new ArgumentNullException(objectName, FrameworkResources.Exception_ArgumentCannotBeNull);
            }
        }

        public static void ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString(string objectName, object objectValue)
        {
            ThrowArgumentNullExceptionIfObjectIsNull(objectName, objectValue);

            if (objectValue is string)
            {
                if (string.IsNullOrEmpty(objectValue.ToString()))
                {
                    throw new ArgumentException(FrameworkResources.Exception_ArgumentCannotBeNullOrEmpty, objectName);
                }
            }
        }
    }
}