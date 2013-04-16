using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Dev2.DataList.Contract
{
    public class OutputLanguageParser : LanguageParser, IDev2LanguageParser {

        #region Attributes
        private static readonly string _elementTag = "Output";
        private static readonly string _mapsToAttribute = "MapsTo";
        #endregion

        #region Ctor
        internal OutputLanguageParser() : base(_elementTag, _mapsToAttribute, false) {}

        #endregion

        #region Methods
        public IList<IDev2Definition> Parse(string OutputDefinition) {
            return base.Parse(OutputDefinition);
        }

        public IList<IDev2Definition> ParseAndAllowBlanks(string OutputDefinition) {
            return base.Parse(OutputDefinition, false);
        }
        #endregion
    }
}
