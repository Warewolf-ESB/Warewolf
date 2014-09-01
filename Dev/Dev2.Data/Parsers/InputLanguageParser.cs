using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.DataList.Contract
{
    public class InputLanguageParser : LanguageParser, IDev2LanguageParser
    {

        #region Attributes

        const string _elementTag = "Input";
        const string _mapsTo = "Source";

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
        public IList<IDev2Definition> Parse(string InputDefinition)
        {
            return base.Parse(InputDefinition);
        }

        /// <summary>
        /// Parses the and allow blanks.
        /// </summary>
        /// <param name="InputDefinition">The input definition.</param>
        /// <returns></returns>
        public IList<IDev2Definition> ParseAndAllowBlanks(string InputDefinition)
        {
            // ReSharper disable RedundantBaseQualifier
            return base.Parse(InputDefinition, false);
            // ReSharper restore RedundantBaseQualifier
        }
        #endregion
    }
}
