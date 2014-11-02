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
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Communication
{
    public class DesignValidationMemo : Memo
    {
        public DesignValidationMemo()
        {
            Errors = new List<IErrorInfo>();
            IsValid = true;
        }

        public string Source { get; set; }
        public string Type { get; set; }
        public Guid ServiceID { get; set; }
        public bool IsValid { get; set; }

        public List<IErrorInfo> Errors { get; set; }
    }
}