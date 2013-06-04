using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

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
        public IList<IDev2Definition> Parse(string InputDefinition) {
            return base.Parse(InputDefinition);
        }

        public IList<IDev2Definition> ParseAndAllowBlanks(string InputDefinition) {
            return base.Parse(InputDefinition, false);
        }
        #endregion
    }
}
