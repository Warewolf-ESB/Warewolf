using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.EqualityComparers;
using Dev2.DataList.Contract.TO;
using Dev2.Enums;
using Dev2.Server.Datalist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Dev2.DataList.Contract
{
    internal class DataListCompiler : IDataListCompiler, IDisposable
    {
        #region Attributes

        private object _disposeGuard = new object();
        private bool _isDisposed = false;

        private static readonly Regex removeSpaceNodeTerm = new Regex("\\s+/>");

        private static readonly string[] stripTags = { "<XmlData>", "</XmlData>", "<Dev2ServiceInput>", "</Dev2ServiceInput>", "<sr>", "</sr>", "<DataList>", "</DataList>", "<ADL />" };

        // New Stuff
        private static readonly IDev2LanguageParser _outputParser = DataListFactory.CreateOutputParser();
        private static readonly IDev2LanguageParser _inputParser = DataListFactory.CreateInputParser();
        private static readonly IDev2DataLanguageParser _parser = DataListFactory.CreateLanguageParser();

        // These are tags to strip from the ADL for ExtractShapeFromADLAndCleanWithDefs used with ShapeInput ;)
        private static readonly string[] naughtyTags = { "<Dev2ResumeData>", "</Dev2ResumeData>", 
                                                         "<Dev2XMLResult>", "</Dev2XMLResult>", 
                                                         "<Dev2WebServer>", "</Dev2WebServer>", 
                                                         "<WebXMLConfiguration>", "</WebXMLConfiguration>", 
                                                         "<Dev2WebpartBindingData>", "</Dev2WebpartBindingData>", 
                                                         "<ActivityInput>", "</ActivityInput>", 
                                                         "<WebPart>", "</WebPart>",
                                                         "<ADL>","</ADL>",
                                                         "<DL>","</DL>"
                                                       };

        //private static readonly string _emptyADL = "<ADL></ADL>";

        private Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>();
        private IServerDataListCompiler _svrCompiler;
        #endregion

        internal DataListCompiler(IServerDataListCompiler svrC)
        {
            // TODO : Allow IP to be sent when using the DataList compiler...
            _svrCompiler = svrC;
        }

        // Travis.Frisinger : 29.10.2012 - New DataListCompiler Methods
        #region New Methods

        public Guid CloneDataList(Guid curDLID, out ErrorResultTO errors)
        {
            return _svrCompiler.CloneDataList(curDLID, out errors);
        }

        public IBinaryDataListEntry Evaluate(Guid curDLID, enActionType typeOf, string expression, bool toRoot, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            return _svrCompiler.Evaluate(null, curDLID, typeOf, expression, out errors, toRoot);

        }

        /// <summary>
        /// Who ever created this method please correct the name and comment it, currently it is very misleading
        /// </summary>
        /// <param name="webpageXml"></param>
        /// <returns></returns>
        public IList<IDev2Definition> GenerateDefsFromWebpageXMl(string webpageXml)
        {
            IList<String> resultData = FormatDsfActivityField(webpageXml);

            IList<IDev2Definition> result = new List<IDev2Definition>();
            XElement root = XElement.Parse(webpageXml);
            //XElement root = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(webpageXml).xmlData as XElement;
            string elementName = "dev2elementname";
            IEnumerable<XElement> elements = root.DescendantsAndSelf().Where(c => c.Name.ToString().ToUpper().Contains(elementName.ToUpper()));

            var results = from e in elements
                          select e.Value;

            List<String> resultSet = resultData.ToList();
            resultSet.AddRange(results.ToList());
            resultData = resultSet;

            foreach (var element in resultData)
            {
                BuildDataPart(element);

            }
            foreach (var tmp in _uniqueWorkflowParts)
            {
                result.Add(DataListFactory.CreateDefinition(tmp.Key.Field, "", "", tmp.Key.Recordset, false, "", false, "", false));
            }

            return result;
        }

        public string GenerateWizardDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors, bool withData = false)
        {
            IList<IDev2Definition> defs = new List<IDev2Definition>();
            IList<IDev2Definition> wizdefs = new List<IDev2Definition>();

            if (defType == enDev2ArgumentType.Output)
            {
                defs = _outputParser.ParseAndAllowBlanks(definitions);

                foreach (IDev2Definition def in defs)
                {
                    if (def.IsRecordSet)
                    {
                        wizdefs.Add(DataListFactory.CreateDefinition(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                    }
                    else
                    {
                        wizdefs.Add(DataListFactory.CreateDefinition(def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                    }
                }
            }
            else if (defType == enDev2ArgumentType.Input)
            {
                defs = _inputParser.Parse(definitions);
                foreach (IDev2Definition def in defs)
                {
                    if (def.IsRecordSet)
                    {
                        wizdefs.Add(DataListFactory.CreateDefinition(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                    }
                    else
                    {
                        wizdefs.Add(DataListFactory.CreateDefinition(def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                    }
                }
            }

            return GenerateDataListFromDefs(wizdefs, pushToServer, out errors, withData);
        }

        public string GenerateDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors)
        {
            IList<IDev2Definition> defs = new List<IDev2Definition>();

            if (defType == enDev2ArgumentType.Output)
            {
                defs = _outputParser.ParseAndAllowBlanks(definitions);
            }
            else if (defType == enDev2ArgumentType.Input)
            {
                defs = _inputParser.Parse(definitions);
            }

            return GenerateDataListFromDefs(defs, pushToServer, out errors);
        }

        public string GenerateDataListFromDefs(IList<IDev2Definition> definitions, bool pushToServer, out ErrorResultTO errors, bool withData = false)
        {
            errors = new ErrorResultTO();
            string dataList = GenerateDataListFromDefs(definitions, withData);
            string result = Guid.Empty.ToString();

            if (pushToServer)
            {
                byte[] data = new byte[0];
                result = _svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString();
            }
            else
            {
                result = dataList;
            }

            return result;
        }

        public string ShapeDev2DefinitionsToDataList(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors)
        {
            string dataList = ShapeDefinitionsToDataList(definitions, defType, out errors);
            string result = Guid.Empty.ToString();

            if (pushToServer)
            {
                //  Push to server and return GUID
                byte[] data = new byte[0];
                result = _svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString();
            }
            else
            {
                result = dataList; // else return the datalist as requested
            }

            return result;
        }

        // Travis.Frisinger - 29.01.2013 : Bug 8412
        public string ShapeDev2DefinitionsToDataList(IList<IDev2Definition> definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors)
        {

            ErrorResultTO allErrors = new ErrorResultTO(); 
            string dataList = DataListUtil.ShapeDefinitionsToDataList(definitions, defType, out errors);
            allErrors.MergeErrors(errors);
            errors.ClearErrors();

            // ReSharper disable RedundantAssignment
            string result = GlobalConstants.NullDataListID.ToString();
            // ReSharper restore RedundantAssignment

            if (pushToServer)
            {
                //  Push to server and return GUID
                byte[] data = new byte[0];
                result = _svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString();
            }
            else
            {
                result = dataList; // else return the datalist as requested
            }

            return result;
        }

        public IBinaryDataList FetchBinaryDataList(Guid curDLID, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            return _svrCompiler.FetchBinaryDataList(null, curDLID, out errors);
        }

        public Guid Upsert(Guid curDLID, string expression, IBinaryDataListEntry value, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            return _svrCompiler.Upsert(null, curDLID, expression, value, out errors);
        }

        public Guid Upsert(Guid curDLID, IList<string> expressions, IList<IBinaryDataListEntry> values, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, expressions, values, out errors);
        }

        public Guid Upsert(Guid curDLID, IList<string> expressions, IList<string> values, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, expressions, values, out errors);
        }

        public Guid Upsert(Guid curDLID, string expression, string value, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, new List<string> { expression }, new List<string> { value }, out errors);
        }

        public Guid Upsert(Guid curDLID, IDev2DataListUpsertPayloadBuilder<string> payload, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, payload, out errors);
        }

        public Guid Upsert(Guid curDLID, IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Upsert(null, curDLID, payload, out errors);
        }

        public Guid Shape(Guid curDLID, enDev2ArgumentType typeOf, string definitions, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Shape(null, curDLID, typeOf, definitions, out errors);
        }

        public Guid Shape(Guid curDLID, enDev2ArgumentType typeOf, IList<IDev2Definition> definitions, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Shape(null, curDLID, typeOf, definitions, out errors);
        }

        public Guid Merge(Guid leftID, Guid rightID, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Merge(null, leftID, rightID, mergeType, depth, createNewList, out errors);
        }

        public IBinaryDataList Merge(IBinaryDataList left, IBinaryDataList right, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return (left.Merge(right, mergeType, depth, createNewList, out errors));
        }

        public void ConditionalMerge(DataListMergeFrequency conditions,
            Guid destinationDatalistID, Guid sourceDatalistID, DataListMergeFrequency datalistMergeFrequency,
            enDataListMergeTypes datalistMergeType, enTranslationDepth datalistMergeDepth)
        {
            _svrCompiler.ConditionalMerge(null, conditions, destinationDatalistID, sourceDatalistID, datalistMergeFrequency, datalistMergeType, datalistMergeDepth);
        }

        public Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, string val, out ErrorResultTO errors)
        {
            return _svrCompiler.UpsertSystemTag(curDLID, tag, val, out errors);
        }

        public IList<DataListFormat> TranslationTypes()
        {
            return (_svrCompiler.FetchTranslatorTypes());
        }

        public Guid ConvertTo(DataListFormat typeOf, string payload, string shape, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            byte[] data = Encoding.UTF8.GetBytes(payload);
            return _svrCompiler.ConvertTo(null, typeOf, data, shape, out errors);
        }

        public Guid ConvertTo(DataListFormat typeOf, byte[] payload, string shape, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.ConvertTo(null, typeOf, payload, shape, out errors);
        }

        public Guid PushBinaryDataList(Guid dlID, byte[] payload, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._BINARY), payload, string.Empty, out errors);
        }

        public Guid PushBinaryDataList(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            byte[] payload = new byte[0];
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, bdl);
            payload = ms.ToArray();

            return _svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._BINARY), payload, string.Empty, out errors);
        }

        public string ConvertFrom(Guid curDLID, DataListFormat typeOf, enTranslationDepth depth, out ErrorResultTO errors)
        {

            DataListTranslatedPayloadTO tmp = _svrCompiler.ConvertFrom(null, curDLID, depth, typeOf, out errors);

            return tmp.FetchAsString();
        }

        public T ConvertFromJsonToModel<T>(string payload)
        {

            T obj = JsonConvert.DeserializeObject<T>(payload);

            return obj;
        }

        public string ConvertModelToJson<T>(T payload)
        {
            string result = JsonConvert.SerializeObject(payload);

            return result;
        }

        public Guid PushSystemModelToDataList<T>(T model, out ErrorResultTO errors)
        {
            return PushSystemModelToDataList(GlobalConstants.NullDataListID, model, out errors);
        }

        public Guid PushSystemModelToDataList<T>(Guid dlID, T model, out ErrorResultTO errors)
        {
            // Serialize the model first ;)
            string jsonModel = ConvertModelToJson(model);
            ErrorResultTO allErrors = new ErrorResultTO();
            
            // Create a new DL if need be
            Guid pushID = dlID;
            if (pushID == GlobalConstants.NullDataListID)
            {
                IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();
                pushID = PushBinaryDataList(bdl.UID, bdl, out errors);
                allErrors.MergeErrors(errors);
                errors.ClearErrors();
            }

            UpsertSystemTag(pushID, enSystemTag.SystemModel, jsonModel, out errors);
            allErrors.MergeErrors(errors);

            errors = allErrors;

            return pushID;
        }

        public T FetchSystemModelFromDataList<T>(Guid dlID, out ErrorResultTO errors)
        {
            string modelData = EvaluateSystemEntry(dlID, enSystemTag.SystemModel, out errors);

            T obj = Activator.CreateInstance<T>();

            if (!string.IsNullOrEmpty(modelData))
            {

                if (!String.IsNullOrEmpty(modelData))
                {
                    obj = ConvertFromJsonToModel<T>(modelData);
                }
            }
            else
            {
                
                errors.AddError("Failed to locate model!");
            }

            return obj;
        }

        public string FetchSystemModelAsWebModel<T>(Guid dlID, out ErrorResultTO errors )
        {
            T model = FetchSystemModelFromDataList<T>(dlID, out errors);
            string result = "{}"; // empty data set for injection ;)

            if (!errors.HasErrors())
            {
                var dev2DataModel = model as IDev2DataModel;

                if (dev2DataModel != null) result = dev2DataModel.ToWebModel();

            }

            return result;
        }

        public string EvaluateSystemEntry(Guid curDLID, enSystemTag sysTag, out ErrorResultTO errors)
        {
            return _svrCompiler.Evaluate(null, curDLID, enActionType.System, sysTag.ToString(), out errors).FetchScalar().TheValue;
        }

        public Guid ShapeInput(Guid curDLID, string definitions, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Shape(null, curDLID, enDev2ArgumentType.Input, definitions, out errors);
        }

        public Guid ShapeInput(Guid curDLID, IList<IDev2Definition> definitions, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Shape(null, curDLID, enDev2ArgumentType.Input, definitions, out errors);
        }

        public Guid ShapeOutput(Guid curDLID, string definitions, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Shape(null, curDLID, enDev2ArgumentType.Output, definitions, out errors);
        }

        public Guid ShapeOutput(Guid curDLID, IList<IDev2Definition> definitions, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return _svrCompiler.Shape(null, curDLID, enDev2ArgumentType.Output, definitions, out errors);
        }

        public IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(Guid id, StateType direction)
        {
            return _svrCompiler.FetchChanges(null, id, direction);
        }

        public bool DeleteDataListByID(Guid curDLID)
        {

            return _svrCompiler.DeleteDataListByID(curDLID, false);
        }

        public bool ForceDeleteDataListByID(Guid curDLID)
        {
            return _svrCompiler.DeleteDataListByID(curDLID, true);
        }

        public int GetMaxNumberOfExecutions(Guid curDLID, IList<string> expressions)
        {

            int result = 1;
            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList bdl = FetchBinaryDataList(curDLID, out errors);
            // Loop each expression to find the total number of executions ;)
            foreach (string exp in expressions)
            {
                IList<IIntellisenseResult> parts = _parser.ParseExpressionIntoParts(exp, bdl.FetchIntellisenseParts());
                foreach (IIntellisenseResult p in parts)
                {
                    result = Math.Max(result, FetchNumberOfExecutions(p, bdl));
                }
            }

            return result;
        }

        public Guid FetchParentID(Guid curDLID)
        {

            ErrorResultTO errors = new ErrorResultTO();
            return (_svrCompiler.FetchBinaryDataList(null, curDLID, out errors).ParentUID);
        }

        public bool HasErrors(Guid curDLID)
        {
            ErrorResultTO errors = new ErrorResultTO();
            return (_svrCompiler.FetchBinaryDataList(null, curDLID, out errors).HasErrors());
        }

        public string FetchErrors(Guid curDLID)
        {
            ErrorResultTO errors = new ErrorResultTO();
            return (_svrCompiler.FetchBinaryDataList(null, curDLID, out errors).FetchErrors());
        }

        public bool SetParentID(Guid curDLID, Guid newParent)
        {
            bool result = true;
            ErrorResultTO errors = new ErrorResultTO();

            _svrCompiler.SetParentUID(curDLID, newParent, out errors);
            if (errors.HasErrors())
            {
                result = false;
            }

            return result;
        }

        public bool PersistResumableDataListChain(Guid baseChildID)
        {
            bool result = false;

            result = _svrCompiler.PersistResumableDataListChain(baseChildID);

            return result;
        }

        public WizardDataListMergeTO MergeFixedWizardDataList(string wizardDL, string serviceDL)
        {
            WizardDataListMergeTO result = new WizardDataListMergeTO();
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            Guid wizardID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, wizardDL, out errors);
            allErrors.MergeErrors(errors);
            Guid serviceID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, serviceDL, out errors);
            allErrors.MergeErrors(errors);

            IBinaryDataList wizardBDL = FetchBinaryDataList(wizardID, out errors);
            allErrors.MergeErrors(errors);
            IBinaryDataList serviceBDL = FetchBinaryDataList(serviceID, out errors);
            allErrors.MergeErrors(errors);


            // Added Removed refenced ParentDL
            // First find difference between parent and wizard
            IList<IBinaryDataListEntry> serviceEntries = serviceBDL.FetchAllEntries();
            IList<IBinaryDataListEntry> wizardEntries = wizardBDL.FetchAllEntries();

            // iterate each service entry
            foreach (IBinaryDataListEntry serviceEntry in serviceEntries)
            {

                // Find all new entries
                bool found = false;
                int pos = 0;
                while (pos < wizardEntries.Count && !found)
                {

                    IBinaryDataListEntry tmp = wizardEntries[pos];
                    if (tmp.Namespace == serviceEntry.Namespace && ((tmp.Columns == null && serviceEntry.Columns == null) || (tmp.Columns.SequenceEqual(serviceEntry.Columns, Dev2ColumnComparer.Instance))))
                    {
                        found = true;
                    }

                    pos++;
                }
                if (!found)
                {
                    result.AddNewRegion(serviceEntry);
                }
            }

            // iterate each service entry
            foreach (IBinaryDataListEntry wizardEntry in wizardEntries)
            {

                // Find all new entries
                bool found = false;
                int pos = 0;
                while (pos < serviceEntries.Count && !found)
                {

                    IBinaryDataListEntry tmp = serviceEntries[pos];
                    if (tmp.Namespace == wizardEntry.Namespace && ((tmp.Columns == null && wizardEntry.Columns == null) || (tmp.Columns.SequenceEqual(wizardEntry.Columns, Dev2ColumnComparer.Instance))))
                    {
                        found = true;
                    }

                    pos++;
                }
                if (!found)
                {
                    result.AddRemovedRegion(wizardEntry);
                }

            }

            // Now build the new Binary Data List
            string tmpDL = ConvertFrom(serviceID, DataListFormat.CreateFormat(GlobalConstants._FIXED_WIZARD), enTranslationDepth.Shape, out errors);
            allErrors.MergeErrors(errors);
            result.SetIntersectedDataList(tmpDL);

            // now clean up
            DeleteDataListByID(serviceID);
            DeleteDataListByID(wizardID);

            errors = allErrors;

            return result;
        }

        public string GetWizardDataListForService(string serviceDefinition)
        {
            string result = string.Empty;

            ErrorResultTO errors = new ErrorResultTO();

            string inputs = string.Empty;
            string outputs = string.Empty;
            try
            {
                inputs = DataListUtil.ExtractInputDefinitionsFromServiceDefinition(serviceDefinition);
                outputs = DataListUtil.ExtractOutputDefinitionsFromServiceDefinition(serviceDefinition);
            }
            catch
            {
                throw new XmlException("Inputs/Outputs tags were not found in the service definition");
            }

            string inputDl = string.Empty;
            string outputDl = string.Empty;

            inputDl = GenerateWizardDataListFromDefs(inputs, enDev2ArgumentType.Input, false, out errors);

            outputDl = GenerateWizardDataListFromDefs(outputs, enDev2ArgumentType.Output, false, out errors);

            Guid inputDlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, inputDl, out errors);
            Guid outputDlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, outputDl, out errors);
            Guid mergedDlID = Merge(inputDlID, outputDlID, enDataListMergeTypes.Union, enTranslationDepth.Shape, true, out errors);
            result = ConvertFrom(mergedDlID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errors);
            return result;
        }

        public string GetWizardDataListForWorkflow(string dataList)
        {
            IBinaryDataList newDl = Dev2BinaryDataListFactory.CreateDataList();
            ErrorResultTO errors;
            Guid dlID = ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), dataList, dataList, out errors);
            if (!errors.HasErrors())
            {
                IBinaryDataList dl = FetchBinaryDataList(dlID, out errors);
                if (!errors.HasErrors())
                {
                    IList<IBinaryDataListEntry> entries = dl.FetchAllEntries();
                    foreach (IBinaryDataListEntry entry in entries)
                    {
                        string errorString;
                        if (entry.IsRecordset)
                        {
                            foreach (Dev2Column col in entry.Columns)
                            {
                                newDl.TryCreateScalarTemplate(string.Empty, entry.Namespace + GlobalConstants.RecordsetJoinChar + col.ColumnName, entry.Description, true, out errorString);
                            }
                        }
                        else
                        {
                            IBinaryDataListItem scalar = entry.FetchScalar();
                            newDl.TryCreateScalarTemplate(string.Empty, scalar.FieldName, entry.Description, true, out errorString);
                            entry.FetchScalar();
                        }
                    }
                    Guid newDlId = PushBinaryDataList(newDl.UID, newDl, out errors);
                    dataList = ConvertFrom(newDlId, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Shape, out errors);
                }
                else
                {
                    throw new Exception(errors.MakeUserReady());
                }
            }
            else
            {
                throw new Exception(errors.MakeUserReady());
            }
            return dataList;
        }

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList)
        {
            return GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.Both);
        }

        public IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if (!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for (int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    XmlAttribute ioDirectionAttribute = tmpNode.Attributes[GlobalConstants.DataListIoColDirection];

                    enDev2ColumnArgumentDirection ioDirection;
                    if (ioDirectionAttribute != null)
                    {
                        ioDirection = Dev2EnumConverter.GetEnumFromStringValue<enDev2ColumnArgumentDirection>(ioDirectionAttribute.Value);
                    }
                    else
                    {
                        ioDirection = enDev2ColumnArgumentDirection.Both;
                    }

                    if (ioDirection == dev2ColumnArgumentDirection ||
                        (ioDirection == enDev2ColumnArgumentDirection.Both &&
                        (dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Input || dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Output)))
                    {
                        if (tmpNode.HasChildNodes)
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            // now extract child node defs
                            XmlNodeList childNL = tmpNode.ChildNodes;
                            for (int q = 0; q < childNL.Count; q++)
                            {
                                result.Add(DataListFactory.CreateDefinition(childNL[q].Name, "", "", recordsetName, false, "",
                                                                            false, "", false));
                            }
                        }
                        else
                        {
                            // scalar value, make it as such
                            result.Add(DataListFactory.CreateDefinition(tmpNode.Name, "", "", false, "", false, ""));
                        }
                    }
                }
            }

            return result;
        }

        #region New Private Methods

        private int FetchNumberOfExecutions(IIntellisenseResult part, IBinaryDataList bdl)
        {
            int result = 1;
            IBinaryDataListEntry entry;
            string error = string.Empty;

            if (!part.Option.IsScalar)
            {
                // process the recordset...
                enRecordsetIndexType type = DataListUtil.GetRecordsetIndexType(part.Option.DisplayValue);
                if (type == enRecordsetIndexType.Star)
                {
                    // Fetch entry and find the last index
                    if (bdl.TryGetEntry(part.Option.Recordset, out entry, out error))
                    {
                        result = entry.FetchLastRecordsetIndex();
                    }
                }
                else if (type == enRecordsetIndexType.Numeric)
                {
                    // Fetch index out
                    Int32.TryParse(part.Option.RecordsetIndex, out result);
                }
            }

            return result;
        }

        /// <summary>
        /// Generate DL shape from IO defs
        /// </summary>
        /// <param name="defs"></param>
        /// <returns></returns>
        private string GenerateDataListFromDefs(IList<IDev2Definition> defs, bool withData = false)
        {
            return DataListUtil.GenerateDataListFromDefs(defs, withData);
        }

        /// <summary>
        /// Create a DL shape as per IO mapping
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="typeOf"></param>
        /// <returns></returns>
        private string ShapeDefinitionsToDataList(string arguments, enDev2ArgumentType typeOf, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return DataListUtil.ShapeDefinitionsToDataList(arguments, typeOf, out errors);
        }

        #region Old Webpage mapping methods

        private void BuildDataPart(string DataPartFieldData)
        {

            IDataListVerifyPart verifyPart;
            string fullyFormattedStringValue;
            string[] fieldList = DataPartFieldData.Split('.');
            if (fieldList.Count() > 1 && !String.IsNullOrEmpty(fieldList[0]))
            {  // If it's a RecordSet Containing a field
                foreach (string item in fieldList)
                {
                    if (item.EndsWith(")") && item == fieldList[0])
                    {
                        if (item.Contains("("))
                        {
                            fullyFormattedStringValue = DataListUtil.RemoveRecordsetBracketsFromValue(item);
                            verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue, String.Empty);
                            AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                        }
                        else
                        { // If it's a field containing a single brace
                            continue;
                        }
                    }
                    else if (item == fieldList[1] && !(item.EndsWith(")") && item.Contains(")")))
                    { // If it's a field to a record set
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(DataListUtil.RemoveRecordsetBracketsFromValue(fieldList.ElementAt(0)), item);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (fieldList.Count() == 1 && !String.IsNullOrEmpty(fieldList[0]))
            { // If the workflow field is simply a scalar or a record set without a child
                if (DataPartFieldData.EndsWith(")") && DataPartFieldData == fieldList[0])
                {
                    if (DataPartFieldData.Contains("("))
                    {
                        fullyFormattedStringValue = DataListUtil.RemoveRecordsetBracketsFromValue(fieldList[0]);
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue, String.Empty);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                }
                else
                {
                    verifyPart = IntellisenseFactory.CreateDataListValidationScalarPart(DataListUtil.RemoveRecordsetBracketsFromValue(DataPartFieldData));
                    AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                }
            }
        }

        private void AddDataVerifyPart(IDataListVerifyPart part, string nameOfPart)
        {
            _uniqueWorkflowParts.Add(part, nameOfPart);
        }

        private IList<String> FormatDsfActivityField(string webpage)
        {
            Dev2DataLanguageParser languageParser = new Dev2DataLanguageParser();
            try
            {
                IList<String> resultData = languageParser.ParseForActivityDataItems(webpage);
                return resultData.Where(result => (!String.IsNullOrEmpty(result.ToString()))).ToList();
            }
            catch (Dev2DataLanguageParseError)
            {
                return new List<String>();
            }
            catch (NullReferenceException)
            {
                return new List<String>();
            }
        }

        #endregion Old Webpage mapping methods

        #endregion

        #endregion

        #region Tear Down

        public void Dispose()
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }

                _uniqueWorkflowParts.Clear();
                _uniqueWorkflowParts = null;

                _svrCompiler = null;

                _isDisposed = true;
            }
        }

        #endregion Tear Down
    }
}
