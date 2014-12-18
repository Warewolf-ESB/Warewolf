
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbTable : IDbTable
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string FullName
        {
            get
            {
                if(string.IsNullOrEmpty(Schema))
                {
                    return TableName;
                }
                if(string.IsNullOrEmpty(TableName))
                {
                    return "";
                }
                return string.Format("{0}.{1}", Schema, TableName);
            }
        }
        public List<IDbColumn> Columns { get; set; }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return FullName;
        }

        #endregion
    }
}


