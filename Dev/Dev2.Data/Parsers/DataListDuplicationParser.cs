
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
using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class DataListVerifyPartDuplicationParser : EqualityComparer<IDataListVerifyPart>, IDataListVerifyPartDuplicationParser
    {
        public override bool Equals(IDataListVerifyPart ComparePart, IDataListVerifyPart Comparator)
        {
            if(ComparePart.DisplayValue == Comparator.DisplayValue)
            {
                    return true;
                }

                    return false;
            }

        public override int GetHashCode(IDataListVerifyPart PartToVerify)
        {
            if(PartToVerify != null)
            {
                int hashCode = PartToVerify.DisplayValue.GetHashCode();
                return hashCode;
            }

            return 0;
        }
    }
}
