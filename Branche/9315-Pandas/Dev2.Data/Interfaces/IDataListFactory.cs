using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IDataListFactory {

        #region Methods
        
        IDev2Definition CreateDefinition(string name, string mapsTo, string value, bool isEval, string defaultValue, bool isRequired, string rawValue);

        IDev2Definition CreateDefinition(string name, string mapsTo, string value, string recordSet, bool isEval, string defaultValue, bool isRequired, string rawValue);

        IRecordSetCollection CreateRecordSetCollection(IList<IDev2Definition> parsedOutput);

        IRecordSetDefinition CreateRecordSetDefinitionFromValueField(IRecordSetDefinition def);

        IList<IDev2Definition> CreateScalarList(IList<IDev2Definition> parsedOutput);

        IDataValue CreateNewDataValue(string val, string tagName, bool isSystemRegion);

        IRecordSetInstance CreateNewRecordSetInstance(IRecordSetDefinition def);

        IDev2LanguageParser CreateOutputParser();

        IDev2LanguageParser CreateInputParser();

        IActivityDataParser CreateActivityDataParser();

        IDataListCompiler CreateDataListCompiler();

        ISystemTag CreateSystemTag(enSystemTag tag);

        string ReshapeDataList(string currentDataList, string dataListShape);

        IInputLanguageDefinition CreateInputDefinition(string name, string mapsTo, bool isEvaluated);

        string GenerateMapping(IList<IDev2Definition> defs, enDev2ArgumentType typeOf);

        string GenerateMappingFromDataList(string dataList, enDev2ArgumentType outputType);

        string GenerateMappingFromWebpage(string webpage, enDev2ArgumentType outputType);

        IList<IDev2DataLanguageIntellisensePart> GenerateIntellisensePartsFromDataList(string dataList);

        IList<IDev2DataLanguageIntellisensePart> GenerateIntellisensePartsFromDataList(string dataList, IntellisenseFilterOpsTO fiterTO);

        IDev2DataLanguageIntellisensePart CreateIntellisensePart(string name, string desc, IList<IDev2DataLanguageIntellisensePart> children);

        IDev2DataLanguageIntellisensePart CreateIntellisensePart(string name, string desc);

        IRecordsetTO CreateRecordsetTO(string recordsetString, IEnumerable<IRecordSetDefinition> cols, int currentIndex = 0);

        IRecordsetScopingObject CreateRecordsetScopingObject(string DataListShape, string CurrentDataList);

        OutputTO CreateOutputTO(string OutputDescription);

        OutputTO CreateOutputTO(string OutputDescription,IList<string> OutputStrings);

        #endregion


    }
}
