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
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Utils
{
    /// <summary>
    ///     Make Service Mapping Operations Testable ;)
    /// </summary>
    public class ServiceMappingHelper
    {
        /// <summary>
        ///     Maps the database outputs.
        /// </summary>
        /// <param name="outputDescription">The output description.</param>
        /// <param name="theService">The service.</param>
        /// <param name="addFields">if set to <c>true</c> [add fields].</param>
        public void MapDbOutputs(IOutputDescription outputDescription, ref DbService theService, bool addFields)
        {
            // only fetch paths with valid data to map ;)
            IEnumerable<IPath> outputsToMap =
                outputDescription.DataSourceShapes[0].Paths.Where(
                    p => (!string.IsNullOrEmpty(p.DisplayPath) && p.DisplayPath != "DocumentElement"));

            var rsFields = new List<RecordsetField>(theService.Recordset.Fields);
#pragma warning disable 219
            int recordsetIndex = 0;
#pragma warning restore 219

            foreach (IPath path in outputsToMap)
            {
                // Remove bogus names and dots
                string name =
                    path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "").Replace("DocumentElement", "");
                string alias =
                    path.DisplayPath.Replace("NewDataSet", "")
                        .Replace(".Table.", "")
                        .Replace(".", "")
                        .Replace("DocumentElement", "");

                int idx = name.IndexOf("()", StringComparison.InvariantCultureIgnoreCase);
                if (idx >= 0)
                {
                    name = name.Remove(0, idx + 2);
                }

                var field = new RecordsetField
                {
                    Name = name,
                    Alias = string.IsNullOrEmpty(path.OutputExpression) ? alias : path.OutputExpression,
                    Path = path
                };

                RecordsetField rsField;
                if (!addFields &&
                    (rsField =
                        rsFields.FirstOrDefault(
                            f => f.Path != null ? f.Path.ActualPath == path.ActualPath : f.Name == field.Name)) != null)
                {
                    field.Alias = rsField.Alias;
                }

                theService.Recordset.Fields.Add(field);

                // 2013.12.11 - COMMUNITY BUG - 341463 - data with empty cells displays incorrectly
                string[] data = path.SampleData.Split(new[] {GlobalConstants.AnytingToXmlCommaToken},
                    StringSplitOptions.None);
                int recordIndex = 0;

                foreach (string item in data)
                {
                    theService.Recordset.SetValue(recordIndex, recordsetIndex, item);
                    recordIndex++;
                }

                recordsetIndex++;
            }
        }
    }
}