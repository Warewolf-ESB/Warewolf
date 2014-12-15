
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections;
using Newtonsoft.Json;

namespace Dev2.Runtime.Diagnostics
{
    public class ValidationResult
    {
        public ValidationResult()
        {
            IsValid = true;
            ErrorMessage = string.Empty;
            ErrorFields = new ArrayList();
            Result = string.Empty;
        }

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public ArrayList ErrorFields { get; set; }
        public string Result { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
