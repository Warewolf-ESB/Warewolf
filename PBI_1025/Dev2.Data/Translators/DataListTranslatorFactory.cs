using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Data.Translators;
using Dev2.DataList.Contract;
using Dev2.Server.DataList.Translators;

namespace Dev2.Server.DataList.Translators {

    /// <summary>
    /// Translator factory for unit testing ;)
    /// </summary>
    public static class DataListTranslatorFactory {

        /// <summary>
        /// Fetches the XML translator.
        /// </summary>
        /// <returns></returns>
        public static IDataListTranslator FetchXmlTranslator() {
            return new DataListXMLTranslator();
        }

        /// <summary>
        /// Fetches the XML translator without system tags.
        /// </summary>
        /// <returns></returns>
        public static IDataListTranslator FetchXmlTranslatorWithoutSystemTags()
        {
            return new DataListXMLTranslatorWithOutSystemTags();
        }

        /// <summary>
        /// Fetches the XML translator without system tags.
        /// </summary>
        /// <returns></returns>
        public static IDataListTranslator FetchStudioDataListXMLTranslator()
        {
            return new StudioDataListXMLTranslator();
        }

        /// <summary>
        /// Fetches the binary translator.
        /// </summary>
        /// <returns></returns>
        public static IDataListTranslator FetchBinaryTranslator() {
            return new DataListBinaryTranslator();
        }

        /// <summary>
        /// Fetches the JSON translator.
        /// </summary>
        /// <returns></returns>
        public static IDataListTranslator FetchJSONTranslator() {
            return new DataListJSONTranslator();
        }

        /// <summary>
        /// Fixeds the wizard data list XML translator.
        /// </summary>
        /// <returns></returns>
        public static IDataListTranslator FixedWizardDataListXMLTranslator() {
            return new FixedWizardDataListXMLTranslator();
        }

        /// <summary>
        /// Fetches the knockout model translator
        /// </summary>
        /// <returns></returns>
        public static IDataListTranslator FetchKnockoutModelTranslator()
        {
            return new DataListKnockoutModelTranslator();
        }

        /// <summary>
        /// Fetches the decision stack translator.
        /// </summary>
        /// <returns></returns>
        public static IDataListTranslator FetchDecisionStackTranslator()
        {
            return new Dev2DecisionStackTranslator();
        }
    }
}
