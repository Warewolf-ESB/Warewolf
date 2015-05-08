
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.Server.Datalist;

// ReSharper disable once CheckNamespace
// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
// ReSharper restore CheckNamespace
{
    internal class DataListCompiler : IDataListCompiler
    {
        #region Attributes

        private readonly object _disposeGuard = new object();
        private bool _isDisposed;

        // New Stuff
        private static readonly IDev2LanguageParser OutputParser = DataListFactory.CreateOutputParser();
        private static readonly IDev2LanguageParser InputParser = DataListFactory.CreateInputParser();

        // These are tags to strip from the ADL for ExtractShapeFromADLAndCleanWithDefs used with ShapeInput ;)

        private Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>();
        private IEnvironmentModelDataListCompiler _svrCompiler;
        #endregion

        internal DataListCompiler(IEnvironmentModelDataListCompiler svrC)
        {
            // TODO : Allow IP to be sent when using the DataList compiler...
            _svrCompiler = svrC;
        }

        // Travis.Frisinger : 29.10.2012 - New DataListCompiler Methods
        #region New Methods

        /// <summary>
        /// Clones the data list.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid CloneDataList(Guid curDlid, out ErrorResultTO errors)
        {
            return _svrCompiler.CloneDataList(curDlid, out errors);
        }

        /// <summary>
        /// Generates the wizard data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <param name="withData"></param>
        /// <returns></returns>
        public StringBuilder GenerateWizardDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors, bool withData = false)
        {
            IList<IDev2Definition> defs;
            IList<IDev2Definition> wizdefs = new List<IDev2Definition>();

            if(defType == enDev2ArgumentType.Output)
            {
                defs = OutputParser.ParseAndAllowBlanks(definitions);

                foreach(IDev2Definition def in defs)
                {
                    wizdefs.Add(def.IsRecordSet ? DataListFactory.CreateDefinition(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue) : DataListFactory.CreateDefinition(def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                }
            }
            else if(defType == enDev2ArgumentType.Input)
            {
                defs = InputParser.Parse(definitions);
                foreach(IDev2Definition def in defs)
                {
                    wizdefs.Add(def.IsRecordSet ? DataListFactory.CreateDefinition(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue) : DataListFactory.CreateDefinition(def.Name, def.MapsTo, def.Value, def.IsEvaluated, def.DefaultValue, def.IsRequired, def.RawValue));
                }
            }

            return GenerateDataListFromDefs(wizdefs, pushToServer, out errors, withData);
        }

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public StringBuilder GenerateDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors)
        {
            IList<IDev2Definition> defs = new List<IDev2Definition>();

            if(defType == enDev2ArgumentType.Output)
            {
                defs = OutputParser.ParseAndAllowBlanks(definitions);
            }
            else if(defType == enDev2ArgumentType.Input)
            {
                defs = InputParser.Parse(definitions);
            }

            return GenerateDataListFromDefs(defs, pushToServer, out errors);
        }

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions as binary objects</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server]. the GUID is returned</param>
        /// <param name="errors">The errors.</param>
        /// <param name="withData"></param>
        /// <returns></returns>
        public StringBuilder GenerateDataListFromDefs(IList<IDev2Definition> definitions, bool pushToServer, out ErrorResultTO errors, bool withData = false)
        {
            errors = new ErrorResultTO();
            var dataList = GenerateDataListFromDefs(definitions, withData);
            StringBuilder result;

            if(pushToServer)
            {
                byte[] data = new byte[0];
                result = new StringBuilder(_svrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML), data, dataList, out errors).ToString());
            }
            else
            {
                result = dataList;
            }

            return result;
        }


        public string GenerateSerializableDefsFromDataList(string datalist, enDev2ColumnArgumentDirection direction)
        {
            DefinitionBuilder db = new DefinitionBuilder();

            if(direction == enDev2ColumnArgumentDirection.Input)
            {
                db.ArgumentType = enDev2ArgumentType.Input;
            }
            else if(direction == enDev2ColumnArgumentDirection.Output)
            {
                db.ArgumentType = enDev2ArgumentType.Output;
            }

            db.Definitions = GenerateDefsFromDataList(datalist, direction);

            return db.Generate();
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

                    var ioDirection = GetDev2ColumnArgumentDirection(tmpNode);

                    if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                    {
                        if (tmpNode.HasChildNodes)
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            // now extract child node defs
                            XmlNodeList childNl = tmpNode.ChildNodes;
                            for (int q = 0; q < childNl.Count; q++)
                            {
                                var xmlNode = childNl[q];
                                var fieldIODirection = GetDev2ColumnArgumentDirection(xmlNode);
                                if (CheckIODirection(dev2ColumnArgumentDirection, fieldIODirection))
                                {
                                    result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                                false, "", false));
                                }
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


        public IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if(!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for(int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    var ioDirection = GetDev2ColumnArgumentDirection(tmpNode);

                        if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection) && tmpNode.HasChildNodes)
                        {
                            result.Add(DataListFactory.CreateDefinition("", "", "", tmpNode.Name, false, "",
                                                                                false, "", false));
                        }
                        else if (tmpNode.HasChildNodes)
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            // now extract child node defs
                            XmlNodeList childNl = tmpNode.ChildNodes;
                            for(int q = 0; q < childNl.Count; q++)
                            {
                                var xmlNode = childNl[q];
                                var fieldIODirection = GetDev2ColumnArgumentDirection(xmlNode);
                                if(CheckIODirection(dev2ColumnArgumentDirection, fieldIODirection))
                                {
                                    result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                                false, "", false));
                                }
                            }
                        }
                        else if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                        {
                            // scalar value, make it as such
                            result.Add(DataListFactory.CreateDefinition(tmpNode.Name, "", "", false, "", false, ""));
                        }

                }
            }

            return result;
        }

        static bool CheckIODirection(enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, enDev2ColumnArgumentDirection ioDirection)
        {
            return ioDirection == dev2ColumnArgumentDirection ||
                   (ioDirection == enDev2ColumnArgumentDirection.Both &&
                    (dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Input || dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Output));
        }

        static enDev2ColumnArgumentDirection GetDev2ColumnArgumentDirection(XmlNode tmpNode)
        {
            XmlAttribute ioDirectionAttribute = tmpNode.Attributes[GlobalConstants.DataListIoColDirection];

            enDev2ColumnArgumentDirection ioDirection;
            if(ioDirectionAttribute != null)
            {
                ioDirection = (enDev2ColumnArgumentDirection)Dev2EnumConverter.GetEnumFromStringDiscription(ioDirectionAttribute.Value, typeof(enDev2ColumnArgumentDirection));
            }
            else
            {
                ioDirection = enDev2ColumnArgumentDirection.Both;
            }
            return ioDirection;
        }

        #region New Private Methods

        /// <summary>
        /// Generate DL shape from IO defs
        /// </summary>
        /// <param name="defs">The defs.</param>
        /// <param name="withData">if set to <c>true</c> [with data].</param>
        /// <returns></returns>
        private StringBuilder GenerateDataListFromDefs(IList<IDev2Definition> defs, bool withData = false)
        {
            return DataListUtil.GenerateDataListFromDefs(defs, withData);
        }
        #endregion

        #endregion

        #region Tear Down

        public void Dispose()
        {
            lock(_disposeGuard)
            {
                if(_isDisposed)
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
