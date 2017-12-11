/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

#region Change Log

//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    The Source type represents all possible data providers within the Dynamic Service Engine Programming Model
//                  A data provider is responsible for serving xml data to all data consumers.
//                  Data consumers are applications that leverage the Dynamic Service Engine

#endregion

using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices.Objects.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.DynamicServices
{

    #region Using Directives

    #endregion

    #region Source Class - Represents a data source
    
    public class Source : DynamicServiceObjectBase
    {
        #region Constructors

        public Source()
            : base(enDynamicServiceObjectType.Source)
        {
            Type = enSourceType.Unknown;
            ID = Guid.Empty;
        }

        #endregion

        public Guid ID { get; set; }
        
        [JsonConverter(typeof (StringEnumConverter))]
        public enSourceType Type { get; set; }
        
        public string ConnectionString { get; set; }
        
        public string AssemblyName { get; set; }
        
        public string AssemblyLocation { get; set; }        
    }

    #endregion
}