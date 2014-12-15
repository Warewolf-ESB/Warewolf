
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
using System.Globalization;
using System.Linq;
using Dev2.Common.ExtMethods;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.BussinessLogic
{
    public class RsOpIsBase64 : AbstractRecsetSearchValidation
    {
        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            Func<IList<string>> result = () =>
                {
                    ErrorResultTO err;
                    IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();
                    IList<string> fnResult = new List<string>();

                    foreach(RecordSetSearchPayload p in operationRange)
                    {

                        if(p.Payload.IsBase64())
                        {
                            fnResult.Add(p.Index.ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            if(to.RequireAllFieldsToMatch)
                            {
                                return new List<string>();
                            }
                        }
                    }

                    return fnResult.Distinct().ToList();
                };

            return result;
        }

        public override string HandlesType()
        {
            return "Is Base64";
        }
    }
}

