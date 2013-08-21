using System.Collections.Generic;
using Dev2.Common;
using Dev2.DataList.Contract;

namespace Dev2.Server.DataList.Translators {

    /// <summary>
    /// Translator factory for unit testing ;)
    /// </summary>
    public class DataListTranslatorFactory : SpookyAction<IDataListTranslator,DataListFormat>
    {

        /// <summary>
        /// Fetches the translator.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public IDataListTranslator FetchTranslator(DataListFormat format)
        {
            return FindMatch(format);
        }

        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <returns></returns>
        public IList<IDataListTranslator> FetchAll()
        {
            return FindAll();
        }

        /// <summary>
        /// Fetches all formats.
        /// </summary>
        /// <returns></returns>
        public IList<DataListFormat> FetchAllFormats()
        {
            var data = FindAll();

            IList<DataListFormat> result = new List<DataListFormat>();

            foreach (var tmp in data)
            {
                result.Add(tmp.Format);
            }

            return result;
        } 

    }
}
