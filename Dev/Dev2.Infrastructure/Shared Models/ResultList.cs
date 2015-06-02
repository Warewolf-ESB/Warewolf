
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
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class ResultList<T>
    {
        public bool HasErrors { get; set; }
        public string Errors { get; set; }

        public List<T> Items { get; private set; }

        public ResultList()
        {
            Items = new List<T>();
        }

        public ResultList(string errorFormat, params object[] args)
            : this()
        {
            HasErrors = true;
            Errors = string.Format(errorFormat, args);
        }

        public ResultList(Exception ex)
            : this()
        {
            var errors = new StringBuilder();
            var tmp = ex;
            while(tmp != null)
            {
                errors.AppendLine(tmp.Message);
                tmp = tmp.InnerException;
            }

            HasErrors = true;
            Errors = errors.ToString();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
