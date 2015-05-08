
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
using System.Data;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects;

// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
{
    public interface IDataListCompiler : IDisposable
    {


        // Travis.Frisinger : 29.10.2012 - Amend Compiler Interface for refactoring
        #region New External Methods

        #region Evaluation Operations

        #endregion

        #region Internal Binary Operations

        /// <summary>
        /// Generates the wizard data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="defType">Type of the def.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server].</param>
        /// <param name="errors">The errors.</param>
        /// <param name="withData"></param>
        /// <returns></returns>
        StringBuilder GenerateWizardDataListFromDefs(string definitions, enDev2ArgumentType defType, bool pushToServer, out ErrorResultTO errors, bool withData = false);

        /// <summary>
        /// Generates the data list from defs
        /// </summary>
        /// <param name="definitions">The definitions as strings</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server]. the GUID is returned</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        StringBuilder GenerateDataListFromDefs(string definitions, enDev2ArgumentType typeOf, bool pushToServer, out ErrorResultTO errors);

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="definitions">The definitions as binary objects</param>
        /// <param name="pushToServer">if set to <c>true</c> [push to server]. the GUID is returned</param>
        /// <param name="errors">The errors.</param>
        /// <param name="withData">if set to <c>true</c> [with data].</param>
        /// <returns></returns>
        StringBuilder GenerateDataListFromDefs(IList<IDev2Definition> definitions, bool pushToServer, out ErrorResultTO errors, bool withData = false);

        /// <summary>
        /// Generates the serializable defs from data list.
        /// </summary>
        /// <param name="datalist">The datalist.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        string GenerateSerializableDefsFromDataList(string datalist, enDev2ColumnArgumentDirection direction);

        /// <summary>
        /// Generate IO definitions from the DL
        /// </summary>
        /// <param name="dataList">The data list.</param>
        /// <returns></returns>
        IList<IDev2Definition> GenerateDefsFromDataList(string dataList);

        /// <summary>
        /// Generate IO definitions from the DL
        /// </summary>
        /// <param name="dataList">The data list.</param>
        /// <param name="dev2ColumnArgumentDirection">The dev2 column argument direction.</param>
        /// <returns></returns>
        IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection);

                /// <summary>
        /// Generate IO definitions from the DL, specifically for debug
        /// </summary>
        /// <param name="dataList">The data list.</param>
        /// <param name="dev2ColumnArgumentDirection">The dev2 column argument direction.</param>
        /// <returns></returns>
        IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection);
      
        /// <summary>
        /// Fetches the binary data list.
        /// </summary>
        /// <param name="curDlid">The cur DL ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        IBinaryDataList FetchBinaryDataList(Guid curDlid, out ErrorResultTO errors);

        /// <summary>
        /// Clones the data list.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid CloneDataList(Guid curDlid, out ErrorResultTO errors);

        /// <summary>
        /// Gets the wizard data list for a service.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <returns>
        /// The string for the data list
        /// </returns>
        /// <exception cref="System.Xml.XmlException">Inputs tag not found in the service definition</exception>
        /// <exception cref="System.Xml.XmlException">Outputs tag not found in the service definition</exception>
        string GetWizardDataListForService(string serviceDefinition);

        /// <summary>
        /// Gets the wizard data list for a workflow.
        /// </summary>
        /// <param name="dataList">The data list.</param>
        /// <returns>
        /// The string for the data list
        /// </returns>
        /// <exception cref="System.Xml.XmlException">Inputs tag not found in the service definition</exception>
        /// <exception cref="System.Xml.XmlException">Inputs tag not found in the service definition</exception>
        string GetWizardDataListForWorkflow(string dataList);

        #endregion

        #region Manipulation Operations
  
        #endregion

        #region External Translation

        /// <summary>
        /// Translation types for conversion to and from binary
        /// </summary>
        /// <returns></returns>
        IList<DataListFormat> TranslationTypes();

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertTo(DataListFormat typeOf, StringBuilder payload, StringBuilder shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts from selected Type to binary
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertTo(DataListFormat typeOf, byte[] payload, StringBuilder shape, out ErrorResultTO errors);


        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertTo(DataListFormat typeOf, object payload, StringBuilder shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts the and only map inputs.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ConvertAndOnlyMapInputs(DataListFormat typeOf, StringBuilder payload, StringBuilder shape, out ErrorResultTO errors);

        /// <summary>
        /// Populates the data list.
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <param name="input">The input.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="targetDlid">The target dlid.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PopulateDataList(DataListFormat typeOf, object input, string outputDefs, Guid targetDlid, out ErrorResultTO errors);

        /// <summary>
        /// Converts to selected Type from binary
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        StringBuilder ConvertFrom(Guid curDlid, DataListFormat typeOf, enTranslationDepth depth, out ErrorResultTO errors);

        /// <summary>
        /// Converts the and filter.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="filterShape">The filter shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        StringBuilder ConvertAndFilter(Guid curDlid, DataListFormat typeOf, StringBuilder filterShape, out ErrorResultTO errors);

        DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions = PopulateOptions.IgnoreBlankRows);

        /// <summary>
        /// Converts from to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        T ConvertFromJsonToModel<T>(StringBuilder payload);

        /// <summary>
        /// Converts the model to json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        StringBuilder ConvertModelToJson<T>(T payload);

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PushSystemModelToDataList<T>(T model, out ErrorResultTO errors);

        /// <summary>
        /// Pushes the system model to data list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PushSystemModelToDataList<T>(Guid dlID, T model, out ErrorResultTO errors);

        /// <summary>
        /// Pushes the binary data list.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PushBinaryDataList(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors);

        #endregion

        #region Admin Operations
        /// <summary>
        /// Fetches the change log for pre ( inputs ) or post execute ( outputs )
        /// </summary>
        /// <returns></returns>
        IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(Guid id, StateType direction);

        /// <summary>
        /// Deletes the data list by ID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <returns></returns>
        bool DeleteDataListByID(Guid curDlid);

        /// <summary>
        /// Forces the delete data list by ID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <returns></returns>
        bool ForceDeleteDataListByID(Guid curDlid);

        
        /// <summary>
        /// Fetches the parent ID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <returns></returns>
        Guid FetchParentID(Guid curDlid);

        /// <summary>
        /// Determines whether the specified cur DLID has errors.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <returns>
        ///   <c>true</c> if the specified cur DLID has errors; otherwise, <c>false</c>.
        /// </returns>
        bool HasErrors(Guid curDlid);


        /// <summary>
        /// Sets the parent ID.
        /// </summary>
        /// <param name="curDlid">The cur DLID.</param>
        /// <param name="newParent">The new parent.</param>
        /// <returns></returns>
        bool SetParentID(Guid curDlid, Guid newParent);


        /// <summary>
        /// Pushes the binary data list in server scope.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid PushBinaryDataListInServerScope(Guid dlID, IBinaryDataList bdl, out ErrorResultTO errors);

        #endregion

        #region Studio Method


        #endregion

        #endregion External Methods

    }
}
