using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Builders;
using Dev2.Data.DataListCache;
using Dev2.Data.Interfaces;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Interfaces;
using Dev2.Server.DataList;
using Dev2.Server.DataList.Translators;
using Dev2.Server.Datalist;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.DataList.Contract
{
    public static class DataListFactory
    {
        #region Class Members

        private static readonly object CacheGuard = new object();
        private static volatile IEnvironmentModelDataListCompiler _serverCompilerCache;
        private static volatile IDataListServer _serverCache;

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

        public static IRecordSetCollection CreateRecordSetCollectionForDbService(IList<IDev2Definition> parsedOutput, bool isOutput)
        {
            return RecordSetCollection(parsedOutput, isOutput, true);
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

            foreach(IDev2Definition def in parsedOutput)
            {
                if(isOutput)
                {
                    var rsName = DataListUtil.ExtractRecordsetNameFromValue(def.Value);

                    if(!def.IsRecordSet && string.IsNullOrEmpty(rsName))
                    {
                        result.Add(def);
                    }
                }
                else
                {
                    var rsName = DataListUtil.ExtractRecordsetNameFromValue(def.Name);

                    if(!def.IsRecordSet && string.IsNullOrEmpty(rsName))
                    {
                        result.Add(def);
                    }
                }

            }

            return result;
        }

        public static IDataValue CreateNewDataValue(string val, string tagName, bool isSystemRegion)
        {
            return new DataValue(val, tagName, isSystemRegion);
        }

        public static IDev2LanguageParser CreateOutputParser()
        {
            return new OutputLanguageParser();
        }

        public static IDev2LanguageParser CreateInputParser()
        {
            return new InputLanguageParser();
        }

        public static IDataListCompiler CreateDataListCompiler()
        {
            return CreateDataListCompiler(CreateServerDataListCompiler());
        }

        public static IDataListCompiler CreateDataListCompiler(IEnvironmentModelDataListCompiler serverDataListCompiler)
        {
            return new DataListCompiler(CreateServerDataListCompiler());
        }

        public static IEnvironmentModelDataListCompiler CreateServerDataListCompiler()
        {
            if(_serverCompilerCache == null)
            {
                lock(CacheGuard)
                {
                    if(_serverCompilerCache == null)
                    {
                        _serverCompilerCache = CreateServerDataListCompiler(CreateDataListServer());
                    }
                }
            }
            return _serverCompilerCache;
        }

        public static IEnvironmentModelDataListCompiler CreateServerDataListCompiler(IDataListServer dataListServer)
        {
            return new ServerDataListCompiler(dataListServer);
        }

        public static IDataListServer CreateDataListServer()
        {
            if(_serverCache == null)
            {
                lock(CacheGuard)
                {
                    if(_serverCache == null)
                    {
                        _serverCache = CreateDataListServer(DataListPersistenceProviderFactory.CreateMemoryProvider());
                    }
                }
            }

            return _serverCache;
        }

        public static IDataListServer CreateDataListServer(IDataListPersistenceProvider persistenceProvider)
        {

            // This needs to remain reflection based, it is payed only once!!!!!!

            DataListTranslatorFactory dltf = new DataListTranslatorFactory();
            IDataListServer svr = new DataListServer(persistenceProvider);

            foreach(var translator in dltf.FetchAll())
            {
                svr.AddTranslator(translator);
            }

            return svr;
        }

        public static ISystemTag CreateSystemTag(enSystemTag tag)
        {
            return new SystemTag(tag.ToString());
        }

        public static IInputLanguageDefinition CreateInputDefinition(string name, string mapsTo, bool isEvaluated)
        {
            return new InputDefinition(name, mapsTo, isEvaluated);
        }

        public static string GenerateMapping(IList<IDev2Definition> defs, enDev2ArgumentType typeOf)
        {
            DefinitionBuilder b = new DefinitionBuilder { ArgumentType = typeOf, Definitions = defs };

            return b.Generate();
        }

        public static IList<IDev2DataLanguageIntellisensePart> GenerateIntellisensePartsFromDataList(string dataList)
        {
            DataListIntellisenseBuilder dlib = new DataListIntellisenseBuilder();

            IntellisenseFilterOpsTO ifot = new IntellisenseFilterOpsTO { FilterType = enIntellisensePartType.All };

            dlib.FilterTO = ifot;

            IList<IDev2DataLanguageIntellisensePart> result = dlib.Generate();

            return result;
        }

        public static IList<IDev2DataLanguageIntellisensePart> GenerateIntellisensePartsFromDataList(string dataList, IntellisenseFilterOpsTO fiterTo)
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
        /// Creating a new SearchTO object
        /// </summary>
        public static SearchTO CreateSearchTO(string fieldsToSearch, string searchType, string searchCriteria, string result)
        {
            return new SearchTO(fieldsToSearch, searchType, searchCriteria, result);
        }


        /// <summary>
        /// Creating a new SearchTO object
        /// </summary>
        public static SearchTO CreateSearchTO(string fieldsToSearch, string searchType, string searchCriteria, string startIndex, string result, string from, string to)
        {
            return new SearchTO(fieldsToSearch, searchType, searchCriteria, startIndex, result, false, from, to);
        }

        /// <summary>
        /// Creating a new SearchTO object
        /// </summary>
        public static SearchTO CreateSearchTO(string fieldsToSearch, string searchType, string searchCriteria, string startIndex, string result, bool matchCase, bool requireAllFieldsToMatch = false, string from = "", string to = "")
        {
            return new SearchTO(fieldsToSearch, searchType, searchCriteria, startIndex, result, matchCase, from, to, requireAllFieldsToMatch);
        }

        /// <summary>
        /// Creates a new Dev2Column object for a recordset
        /// </summary>
        public static Dev2Column CreateDev2Column(string columnName, string columnDescription)
        {
            return new Dev2Column(columnName, columnDescription);
        }

        /// <summary>
        /// Creates a new Dev2Column object for a recordset
        /// </summary>
        public static Dev2Column CreateDev2Column(string columnName, string columnDescription, bool isEditable)
        {
            return new Dev2Column(columnName, columnDescription, isEditable);
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
