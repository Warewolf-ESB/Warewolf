
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
