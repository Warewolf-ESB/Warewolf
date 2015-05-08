
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
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Binary_Objects;

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
      
       
        #endregion




        #endregion External Methods

    }
}
