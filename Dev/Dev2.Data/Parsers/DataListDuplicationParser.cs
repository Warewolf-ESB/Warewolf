using System.Collections.Generic;
using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract {
    public class DataListVerifyPartDuplicationParser : EqualityComparer<IDataListVerifyPart>, IDataListVerifyPartDuplicationParser {
            public override bool Equals(IDataListVerifyPart ComparePart, IDataListVerifyPart Comparator) {
                if (ComparePart.DisplayValue == Comparator.DisplayValue) {
                    return true;
                }
                else
                    return false;
            }

            public override int GetHashCode(IDataListVerifyPart PartToVerify) {
                int hashCode = PartToVerify.DisplayValue.GetHashCode();
                return hashCode;
            }
    }
}
