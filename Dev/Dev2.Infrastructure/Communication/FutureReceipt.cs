
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Communication
{
    /// <summary>
    /// Used to fetch execution request payloads
    /// NEVER MAKE THE PROPERTIES GETTER PRIVATE AS PER SONAR
    /// THIS WILL CAUSE SIGNALR'S SERIALIZATION TO FREAK OUT AND PASS EMPTY REQUESTID VALUES THROUGH
    /// </summary>
    public class FutureReceipt
    {
        public Guid RequestID { get; set; }

        public int PartID { get; set; }

        public string User { get; set; }

        public string ToKey()
        {
            if(PartID < 0)
            {
                throw new Exception("Invalid PartID");
            }

            if(string.IsNullOrEmpty(User))
            {
                throw new Exception("Invalid User");
            }

            if(RequestID == Guid.Empty)
            {
                throw new Exception("Invalid RequestID");
            }

            return RequestID + "-" + PartID + "-" + User + "!";
        }
    }
}
