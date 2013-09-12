using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public class InputLanguageParser :  LanguageParser, IDev2LanguageParser {

        #region Attributes
        private static readonly string _elementTag = "Input";
        private static readonly string _mapsTo = "Source";
        #endregion

        #region Ctor
        internal InputLanguageParser() : base(_elementTag, _mapsTo, true) { }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the specified input definition.
        /// </summary>
        /// <param name="InputDefinition">The input definition.</param>
        /// <returns></returns>
        public IList<IDev2Definition> Parse(string InputDefinition) {
            return base.Parse(InputDefinition);
        }

        /// <summary>
        /// Parses the and allow blanks.
        /// </summary>
        /// <param name="InputDefinition">The input definition.</param>
        /// <returns></returns>
        public IList<IDev2Definition> ParseAndAllowBlanks(string InputDefinition) {
            return base.Parse(InputDefinition, false);
        }
        #endregion
    }
}
