using Dev2.Data.Enums;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Translators
{

    public interface ITranslate
    {
        /// <summary>
        /// Convert Binary object to string
        /// </summary>
        /// <param name="datalist"></param>
        /// <param name="typeOf"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        string Serialize(IBinaryDataList datalist, enTranslationTypes typeOf, out string error);

        /// <summary>
        /// Convert string to binary object
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="targetShape">The target shape.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataList DeSerialize(string data, string targetShape, enTranslationTypes typeOf, out string error);
    }
}
