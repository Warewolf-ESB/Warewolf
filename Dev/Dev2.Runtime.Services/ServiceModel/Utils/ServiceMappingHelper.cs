#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
    /// Make Service Mapping Operations Testable ;)
    /// </summary>
    public class ServiceMappingHelper
    {

        /// <summary>
        /// Maps the database outputs.
        /// </summary>
        /// <param name="outputDescription">The output description.</param>
        /// <param name="theService">The service.</param>
        /// <param name="addFields">if set to <c>true</c> [add fields].</param>
        public void MapDbOutputs(IOutputDescription outputDescription, ref DbService theService, bool addFields)
        {

            // only fetch paths with valid data to map ;)
            var outputsToMap = outputDescription.DataSourceShapes[0].Paths.Where(p => !string.IsNullOrEmpty(p.DisplayPath) && p.DisplayPath != "DocumentElement");
            
            var rsFields = new List<RecordsetField>(theService.Recordset.Fields);
            var recordsetIndex = 0;

            foreach (var path in outputsToMap)
            {
                // Remove bogus names and dots
                var name = path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "").Replace("DocumentElement","");
                var alias = path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "").Replace(".","").Replace("DocumentElement", "");

                var idx = name.IndexOf("()", StringComparison.InvariantCultureIgnoreCase);
                if(idx >= 0)
                {
                    name = name.Remove(0, idx + 2);
                    if (name.StartsWith("."))
                    {
                        name = name.Substring(1);
                    }
                }

                var field = new RecordsetField { Name = name, Alias = string.IsNullOrEmpty(path.OutputExpression) ? alias : path.OutputExpression, Path = path };

                RecordsetField rsField;
                if(!addFields && (rsField = rsFields.FirstOrDefault(f => f.Path != null ? f.Path.ActualPath == path.ActualPath : f.Name == field.Name)) != null)
                {
                    field.Alias = rsField.Alias;
                }
           
                theService.Recordset.Fields.Add(field);

                // 2013.12.11 - COMMUNITY BUG - 341463 - data with empty cells displays incorrectly
                var data = path.SampleData.Split(new[] { GlobalConstants.AnytingToXmlCommaToken }, StringSplitOptions.None);
                var recordIndex = 0;

                foreach (var item in data)
                {
                    theService.Recordset.SetValue(recordIndex, recordsetIndex, item);
                    recordIndex++;
                }

                recordsetIndex++;
            }

        }



        public void MySqlMapDbOutputs(IOutputDescription outputDescription, ref DbService theService, bool addFields)
        {

            // only fetch paths with valid data to map ;)
            var outputsToMap = outputDescription.DataSourceShapes[0].Paths.Where(p => !string.IsNullOrEmpty(p.DisplayPath) && p.DisplayPath != "DocumentElement");

            var rsFields = new List<RecordsetField>(theService.Recordset.Fields);
            var recordsetIndex = 0;

            foreach (var path in outputsToMap)
            {
                // Remove bogus names and dots
                var name = path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "").Replace("DocumentElement", "");
                var alias = path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "").Replace(".", "").Replace("DocumentElement", "");

                var idx = name.IndexOf("()", StringComparison.InvariantCultureIgnoreCase);
                if (idx >= 0)
                {
                    name = name.Remove(0, idx + 3);
                }
                idx = alias.IndexOf("()", StringComparison.InvariantCultureIgnoreCase);
                if (idx >= 0)
                {
                    alias = alias.Remove(0, idx + 2);
                }
                var field = new RecordsetField { Name = name, Alias = string.IsNullOrEmpty(path.OutputExpression) ? alias : path.OutputExpression, Path = path };

                RecordsetField rsField;
                if (!addFields && (rsField = rsFields.FirstOrDefault(f => f.Path != null ? f.Path.ActualPath == path.ActualPath : f.Name == field.Name)) != null)
                {
                    field.Alias = rsField.Alias;
                }

                theService.Recordset.Fields.Add(field);

                // 2013.12.11 - COMMUNITY BUG - 341463 - data with empty cells displays incorrectly
                var data = path.SampleData.Split(new[] { GlobalConstants.AnytingToXmlCommaToken }, StringSplitOptions.None);
                var recordIndex = 0;

                foreach (var item in data)
                {
                    theService.Recordset.SetValue(recordIndex, recordsetIndex, item);
                    recordIndex++;
                }

                recordsetIndex++;
            }

        }

        public void SqliteMapDbOutputs(IOutputDescription outputDescription, ref SqliteDBService theService, bool addFields)
        {

            // only fetch paths with valid data to map ;)
            var outputsToMap = outputDescription.DataSourceShapes[0].Paths.Where(p => !string.IsNullOrEmpty(p.DisplayPath) && p.DisplayPath != "DocumentElement");

            var rsFields = new List<RecordsetField>(theService.Recordset.Fields);
            var recordsetIndex = 0;

            foreach (var path in outputsToMap)
            {
                // Remove bogus names and dots
                var name = path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "").Replace("DocumentElement", "");
                var alias = path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "").Replace(".", "").Replace("DocumentElement", "");

                var idx = name.IndexOf("()", StringComparison.InvariantCultureIgnoreCase);
                if (idx >= 0)
                {
                    name = name.Remove(0, idx + 3);
                }
                idx = alias.IndexOf("()", StringComparison.InvariantCultureIgnoreCase);
                if (idx >= 0)
                {
                    alias = alias.Remove(0, idx + 2);
                }
                var field = new RecordsetField { Name = name, Alias = string.IsNullOrEmpty(path.OutputExpression) ? alias : path.OutputExpression, Path = path };

                RecordsetField rsField;
                if (!addFields && (rsField = rsFields.FirstOrDefault(f => f.Path != null ? f.Path.ActualPath == path.ActualPath : f.Name == field.Name)) != null)
                {
                    field.Alias = rsField.Alias;
                }

                theService.Recordset.Fields.Add(field);

                // 2013.12.11 - COMMUNITY BUG - 341463 - data with empty cells displays incorrectly
                var data = path.SampleData.Split(new[] { GlobalConstants.AnytingToXmlCommaToken }, StringSplitOptions.None);
                var recordIndex = 0;

                foreach (var item in data)
                {
                    theService.Recordset.SetValue(recordIndex, recordsetIndex, item);
                    recordIndex++;
                }

                recordsetIndex++;
            }

        }
    }
}
