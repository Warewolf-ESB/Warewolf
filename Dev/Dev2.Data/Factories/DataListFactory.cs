#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Builders;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.DataList.Contract.Binary_Objects;



namespace Dev2.DataList.Contract
{
    public interface IDataListFactory
    {
        IDev2LanguageParser CreateOutputParser();
        IRecordSetCollection CreateRecordSetCollection(IList<IDev2Definition> parsedOutput, bool isOutput);
        IEnumerable<IDev2Definition> CreateScalarList(IEnumerable<IDev2Definition> parsedOutput, bool isOutput);
        IEnumerable<IDev2Definition> CreateObjectList(IEnumerable<IDev2Definition> parsedOutput);
    }
    public class DataListFactoryImplementation : IDataListFactory
    {
        public IDev2LanguageParser CreateOutputParser() => new OutputLanguageParser();

        public IRecordSetCollection CreateRecordSetCollection(IList<IDev2Definition> parsedOutput, bool isOutput) => RecordSetCollection(parsedOutput, isOutput, false);

        static IRecordSetCollection RecordSetCollection(IList<IDev2Definition> parsedOutput, bool isOutput, bool isDbService)
        {
            var b = new RecordSetCollectionBuilder();

            b.SetParsedOutput(parsedOutput);
            b.IsOutput = isOutput;
            b.IsDbService = isDbService;
            var result = b.Generate();

            return result;
        }

        public IEnumerable<IDev2Definition> CreateScalarList(IEnumerable<IDev2Definition> parsedOutput, bool isOutput)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            foreach (IDev2Definition def in parsedOutput)
            {
                if (!def.IsRecordSet && !def.IsObject)
                {
                    result.Add(def);
                }
            }

            return result;
        }
        public IEnumerable<IDev2Definition> CreateObjectList(IEnumerable<IDev2Definition> parsedOutput) => parsedOutput.Where(def => def.IsObject).ToList();
    }

    public static class DataListFactory
    {
        static IDataListFactory _instance;
        static readonly object _lock = new object();
        public static IDataListFactory Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        if (_instance is null)
                        {
                            _instance = new DataListFactoryImplementation();
                        }
                    }
                }
                return _instance;
            }
        }


        /// <summary>
        /// Creates the language parser. 
        /// </summary>
        /// <returns></returns>
        public static IDev2DataLanguageParser CreateLanguageParser() => new Dev2DataLanguageParser();

        public static IDev2Definition CreateDefinition_JsonArray(string name, string mapsTo, string value, bool isEval, string defaultValue, bool isRequired, string rawValue, bool emptyToNull, bool isArray)
        {
            var dev2Definition = Dev2Definition.NewObject(name, mapsTo, value, isEval, defaultValue, isRequired, rawValue, emptyToNull);
            dev2Definition.IsJsonArray = isArray;
            return dev2Definition;
        }

        public static IDev2Definition CreateDefinition_Recordset(string name, string mapsTo, string value, string recordSet, bool isEval, string defaultValue, bool isRequired, string rawValue, bool emptyToNull) => new Dev2Definition(name, mapsTo, value, recordSet, isEval, defaultValue, isRequired, rawValue, emptyToNull);

        public static IDev2LanguageParser CreateOutputParser() => new OutputLanguageParser();

        public static IDev2LanguageParser CreateInputParser() => new InputLanguageParser();

        public static IList<IDev2DataLanguageIntellisensePart> GenerateIntellisensePartsFromDataList(string dataList, IIntellisenseFilterOpsTO fiterTo)
        {
            var dlib = new DataListIntellisenseBuilder { FilterTO = fiterTo, DataList = dataList };

            var result = dlib.Generate();

            return result;
        }

        public static IDev2DataLanguageIntellisensePart CreateIntellisensePart(string name, string desc, IList<IDev2DataLanguageIntellisensePart> children) => new Dev2DataLanguageIntellisensePart(name, desc, children);

        public static IDev2DataLanguageIntellisensePart CreateIntellisensePart(string name, string desc) => new Dev2DataLanguageIntellisensePart(name, desc, null);

        public static OutputTO CreateOutputTO(string outputDescription) => new OutputTO(outputDescription);

        public static OutputTO CreateOutputTO(string outputDescription, IList<string> outputStrings) => new OutputTO(outputDescription, outputStrings);

        public static OutputTO CreateOutputTO(string outputDescription, string outputString) => new OutputTO(outputDescription, new List<string> { outputString });

        public static Dev2Column CreateDev2Column(string columnName, string columnDescription, bool isEditable, enDev2ColumnArgumentDirection colIODir) => new Dev2Column(columnName, columnDescription, isEditable, colIODir);
    }
}
