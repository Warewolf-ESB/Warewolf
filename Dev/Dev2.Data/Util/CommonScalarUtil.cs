#pragma warning disable
using Dev2.Data.Interfaces;

namespace Dev2.Data.Util
{
    class CommonScalarUtil : ICommonScalarUtil
    {
        #region Implementation of ICommonScalarUtil

        public bool IsValueScalar(string value)
        {
            var result = false;

            if (!string.IsNullOrEmpty(value) && value.StartsWith(DataListUtil.OpeningSquareBrackets) && value.EndsWith(DataListUtil.ClosingSquareBrackets) && !DataListUtil.IsValueRecordset(value) && !value.Contains("."))
            {
                result = true;
            }


            return result;
        }

        #endregion
    }
}