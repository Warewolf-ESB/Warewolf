using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Interfaces;
using Dev2.DataList.Contract.Network;
using Dev2.Server.Datalist;
using Dev2.Server.DataList;
using Dev2.Server.DataList.Translators;
using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public static class DataListFactory
    {
        #region Class Members

        private static object _cacheGuard = new object();
        private static volatile IServerDataListCompiler _serverCompilerCache;
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

        public static IRecordSetCollection CreateRecordSetCollection(IList<IDev2Definition> parsedOutput)
        {
            IRecordSetCollection result = null;
            RecordSetCollectionBuilder b = new RecordSetCollectionBuilder();

            b.setParsedOutput(parsedOutput);

            result = b.Generate();

            return result;
        }

        public static IRecordSetDefinition CreateRecordSetDefinitionFromValueField(IRecordSetDefinition def)
        {
            IRecordSetDefinition result = null;
            RecordSetDefintionMutator m = new RecordSetDefintionMutator();

            m.setDefinition(def);

            result = m.Generate();

            return result;
        }

        public static IList<IDev2Definition> CreateScalarList(IList<IDev2Definition> parsedOutput)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            foreach (IDev2Definition def in parsedOutput)
            {
                if (!def.IsRecordSet)
                {
                    result.Add(def);
                }
            }

            return result;
        }

        public static IDataValue CreateNewDataValue(string val, string tagName, bool isSystemRegion)
        {
            return new DataValue(val, tagName, isSystemRegion);
        }

        public static IRecordSetInstance CreateNewRecordSetInstance(IRecordSetDefinition def)
        {
            return new RecordSetInstance(def);
        }

        public static IDev2LanguageParser CreateOutputParser()
        {
            return new OutputLanguageParser();
        }

        public static IDev2LanguageParser CreateInputParser()
        {
            return new InputLanguageParser();
        }

        public static IActivityDataParser CreateActivityDataParser()
        {
            return new ActivityDataParser();
        }

        public static IDataListCompiler CreateDataListCompiler()
        {
            return CreateDataListCompiler(CreateServerDataListCompiler());
        }

        public static IDataListCompiler CreateDataListCompiler(IServerDataListCompiler serverDataListCompiler)
        {
            return new DataListCompiler(CreateServerDataListCompiler());
        }

        public static IDataListCompiler CreateDataListCompiler(INetworkDataListChannel channel)
        {
            IDataListPersistenceProvider persistenceProvider = DataListPersistenceProviderFactory.CreateServerProvider(channel);
            IDataListServer datalistServer = CreateDataListServer(persistenceProvider);
            IServerDataListCompiler serverDataListCompiler = CreateServerDataListCompiler(datalistServer);
            return new DataListCompiler(serverDataListCompiler);
        }

        public static IServerDataListCompiler CreateServerDataListCompiler()
        {
            if (_serverCompilerCache == null)
            {
                lock (_cacheGuard)
                {
                    if (_serverCompilerCache == null)
                    {
                        _serverCompilerCache = CreateServerDataListCompiler(CreateDataListServer());
                    }
                }
            }
            return _serverCompilerCache;
        }

        public static IServerDataListCompiler CreateServerDataListCompiler(IDataListServer dataListServer)
        {
            return new ServerDataListCompiler(dataListServer);
        }

        public static IDataListServer CreateDataListServer()
        {
            if (_serverCache == null)
            {
                lock (_cacheGuard)
                {
                    if (_serverCache == null)
                    {
                        _serverCache = CreateDataListServer(DataListPersistenceProviderFactory.CreateMemoryProvider());
                    }
                }
            }

            return _serverCache;
        }

        public static IDataListServer CreateDataListServer(IDataListPersistenceProvider persistenceProvider)
        {
            IDataListServer svr = new DataListServer(persistenceProvider);
            svr.AddTranslator(DataListTranslatorFactory.FetchBinaryTranslator());
            svr.AddTranslator(DataListTranslatorFactory.FetchXmlTranslator());
            svr.AddTranslator(DataListTranslatorFactory.FetchXmlTranslatorWithoutSystemTags());
            svr.AddTranslator(DataListTranslatorFactory.FetchStudioDataListXMLTranslator());
            svr.AddTranslator(DataListTranslatorFactory.FixedWizardDataListXMLTranslator());
            return svr;
        }

        //public static IDataListSentinel CreateSentinel() {
        //    return new DataListSentinel();
        //}

        public static ISystemTag CreateSystemTag(enSystemTag tag)
        {
            return new SystemTag(tag.ToString());
        }

        //public static string ReshapeDataList(string currentDataList, string dataListShape) {
        //    return (new DataListCompiler().ReshapeDataList(currentDataList, dataListShape));
        //}

        public static IInputLanguageDefinition CreateInputDefinition(string name, string mapsTo, bool isEvaluated)
        {
            return new InputDefinition(name, mapsTo, isEvaluated);
        }

        public static string GenerateMapping(IList<IDev2Definition> defs, enDev2ArgumentType typeOf)
        {
            DefinitionBuilder b = new DefinitionBuilder();
            b.ArgumentType = typeOf;
            b.Definitions = defs;

            return b.Generate();
        }

        public static string GenerateMappingFromDataList(string dataList, enDev2ArgumentType outputType, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IDataListCompiler compiler = CreateDataListCompiler();
            DefinitionBuilder db = new DefinitionBuilder();
            db.ArgumentType = outputType;
            db.Definitions = compiler.GenerateDefsFromDataList(dataList, dev2ColumnArgumentDirection);

            return db.Generate();
        }

        public static string GenerateMappingFromWebpage(string webpage, string dataList, enDev2ArgumentType outputType)
        {
            DefinitionBuilder db = new DefinitionBuilder();
            IDataListCompiler compiler = CreateDataListCompiler();
            db.ArgumentType = outputType;
            db.Definitions = DataListFactory.CreateDataListCompiler().GenerateDefsFromWebpageXMl(webpage);
            if (!string.IsNullOrEmpty(dataList))
            {
                foreach (IDev2Definition definition in compiler.GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.None))
                {
                    db.Definitions.Add(definition);
                }
            }
            //.GenerateDefsFromWebpageXML(webpage);

            return db.Generate();
        }

        public static IList<IDev2DataLanguageIntellisensePart> GenerateIntellisensePartsFromDataList(string dataList)
        {
            IList<IDev2DataLanguageIntellisensePart> result = new List<IDev2DataLanguageIntellisensePart>();

            DataListIntellisenseBuilder dlib = new DataListIntellisenseBuilder();

            IntellisenseFilterOpsTO ifot = new IntellisenseFilterOpsTO();
            ifot.FilterType = enIntellisensePartType.All;

            dlib.FilterTO = ifot;
            //dlib.DataList = DataListFactory.CreateDataListCompiler().StripCrap(dataList);

            result = dlib.Generate();

            return result;
        }

        public static IList<IDev2DataLanguageIntellisensePart> GenerateIntellisensePartsFromDataList(string dataList, IntellisenseFilterOpsTO fiterTO)
        {
            IList<IDev2DataLanguageIntellisensePart> result = new List<IDev2DataLanguageIntellisensePart>();

            DataListIntellisenseBuilder dlib = new DataListIntellisenseBuilder();

            dlib.FilterTO = fiterTO;
            dlib.DataList = dataList;

            result = dlib.Generate();

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

        //public static IRecordsetScopingObject CreateRecordsetScopingObject(string DataListShape, string CurrentDataList) {
        //    //return new RecordsetScopingObject(DataListShape, CurrentDataList);
        //}

        public static IRecordsetTO CreateRecordsetTO(string recordsetString, IEnumerable<IRecordSetDefinition> cols, int currentIndex = 0)
        {
            return new RecordsetTO(recordsetString, cols, currentIndex);
        }

        public static OutputTO CreateOutputTO(string OutputDescription)
        {
            return new OutputTO(OutputDescription);
        }

        public static OutputTO CreateOutputTO(string OutputDescription, IList<string> OutputStrings)
        {
            return new OutputTO(OutputDescription, OutputStrings);
        }

        public static OutputTO CreateOutputTO(string OutputDescription, string OutputString)
        {
            return new OutputTO(OutputDescription, new List<string> { OutputString });
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
        public static SearchTO CreateSearchTO(string fieldsToSearch, string searchType, string searchCriteria, string startIndex, string result, bool matchCase)
        {
            return new SearchTO(fieldsToSearch, searchType, searchCriteria, startIndex, result, matchCase);
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
