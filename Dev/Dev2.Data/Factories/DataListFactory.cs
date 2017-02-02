/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Builders;
using Dev2.Data.Interfaces;
using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Interfaces;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.DataList.Contract
{
    public static class DataListFactory
    {
        #region Class Members


        #endregion Class Members

        #region Methods

        /// <summary>
        /// Creates the language parser. 
        /// </summary>
        /// <returns></returns>
        public static IDev2DataLanguageParser CreateLanguageParser()
        {
            return new Dev2DataLanguageParser();
        }

        /// <summary>
        /// Creates the studio language parser.
        /// </summary>
        /// <returns></returns>
        public static IDev2StudioDataLanguageParser CreateStudioLanguageParser()
        {
            return new Dev2DataLanguageParser();
        }

        public static IDev2Definition CreateDefinition(string name, string mapsTo, string value, bool isEval, string defaultValue, bool isRequired, string rawValue)
        {
            return new Dev2Definition(name, mapsTo, value, isEval, defaultValue, isRequired, rawValue);
        }

        public static IDev2Definition CreateDefinition(string name, string mapsTo, string value, bool isEval, string defaultValue, bool isRequired, string rawValue, bool emptyToNull)
        {
            return new Dev2Definition(name, mapsTo, value, string.Empty, isEval, defaultValue, isRequired, rawValue, emptyToNull);
        }

        public static IDev2Definition CreateDefinition(string name, string mapsTo, string value, string recordSet, bool isEval, string defaultValue, bool isRequired, string rawValue, bool emptyToNull)
        {
            return new Dev2Definition(name, mapsTo, value, recordSet, isEval, defaultValue, isRequired, rawValue, emptyToNull);
        }

        public static IRecordSetCollection CreateRecordSetCollection(IList<IDev2Definition> parsedOutput, bool isOutput)
        {
            return RecordSetCollection(parsedOutput, isOutput, false);
        }

        static IRecordSetCollection RecordSetCollection(IList<IDev2Definition> parsedOutput, bool isOutput, bool isDbService)
        {
            RecordSetCollectionBuilder b = new RecordSetCollectionBuilder();

            b.SetParsedOutput(parsedOutput);
            b.IsOutput = isOutput;
            b.IsDbService = isDbService;
            IRecordSetCollection result = b.Generate();

            return result;
        }

        public static IList<IDev2Definition> CreateScalarList(IList<IDev2Definition> parsedOutput, bool isOutput)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            foreach (IDev2Definition def in parsedOutput)
            {
                if (isOutput)
                {

                    if (!def.IsRecordSet && !def.IsObject)
                    {
                        result.Add(def);
                    }
                }
                else
                {

                    if (!def.IsRecordSet && !def.IsObject)
                    {
                        result.Add(def);
                    }
                }

            }

            return result;
        }


        public static IList<IDev2Definition> CreateObjectList(IList<IDev2Definition> parsedOutput)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            foreach (IDev2Definition def in parsedOutput)
            {
                if (def.IsObject)
                {
                    result.Add(def);
                }
            }
            return result;
        }
        public static IDev2LanguageParser CreateOutputParser()
        {
            return new OutputLanguageParser();
        }

        public static IDev2LanguageParser CreateInputParser()
        {
            return new InputLanguageParser();
        }


        public static string GenerateMapping(IList<IDev2Definition> defs, enDev2ArgumentType typeOf)
        {
            DefinitionBuilder b = new DefinitionBuilder { ArgumentType = typeOf, Definitions = defs };

            return b.Generate();
        }

        public static IList<IDev2DataLanguageIntellisensePart> GenerateIntellisensePartsFromDataList(string dataList, IIntellisenseFilterOpsTO fiterTo)
        {
            DataListIntellisenseBuilder dlib = new DataListIntellisenseBuilder { FilterTO = fiterTo, DataList = dataList };

            IList<IDev2DataLanguageIntellisensePart> result = dlib.Generate();

            return result;
        }

        public static IDev2DataLanguageIntellisensePart CreateIntellisensePart(string name, string desc, IList<IDev2DataLanguageIntellisensePart> children)
        {
            return new Dev2DataLanguageIntellisensePart(name, desc, children);
        }

        public static IDev2DataLanguageIntellisensePart CreateIntellisensePart(string name, string desc)
        {
            return new Dev2DataLanguageIntellisensePart(name, desc, null);
        }

        public static OutputTO CreateOutputTO(string outputDescription)
        {
            return new OutputTO(outputDescription);
        }

        public static OutputTO CreateOutputTO(string outputDescription, IList<string> outputStrings)
        {
            return new OutputTO(outputDescription, outputStrings);
        }

        public static OutputTO CreateOutputTO(string outputDescription, string outputString)
        {
            return new OutputTO(outputDescription, new List<string> { outputString });
        }

        /// <summary>
        /// Creates a new Dev2Column object for a recordset
        /// </summary>
        public static Dev2Column CreateDev2Column(string columnName, string columnDescription, bool isEditable, enDev2ColumnArgumentDirection colIODir)
        {
            return new Dev2Column(columnName, columnDescription, isEditable, colIODir);
        }

        #endregion Methods
    }
}
