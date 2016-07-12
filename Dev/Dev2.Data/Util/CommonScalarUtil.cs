using Dev2.Data.Interfaces;

namespace Dev2.Data.Util
{
    internal class CommonScalarUtil : ICommonScalarUtil
    {
        #region Implementation of ICommonScalarUtil

        public bool IsValueScalar(string value)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(value))
            {
                if (value.StartsWith(DataListUtil.OpeningSquareBrackets) && value.EndsWith(DataListUtil.ClosingSquareBrackets) && !DataListUtil.IsValueRecordset(value) && !value.Contains("."))
                {
                    result = true;
                }
            }

            return result;
        }

        #endregion
    }
}