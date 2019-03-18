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
using System.Activities;
using System.Activities.Presentation.View;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xaml;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Util;

namespace Dev2.DynamicServices.Objects
{
    /// <summary>
    ///     Created to break memory leak in ServiceAction ;)
    /// </summary>
    public class Dev2XamlLoader
    {
        /// <summary>
        ///     Loads the specified xaml definition.
        /// </summary>
        /// <param name="xamlDefinition">The xaml definition.</param>
        /// <param name="xamlStream">The xaml stream.</param>
        /// <param name="workflowPool">The workflow pool.</param>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <exception cref="System.ArgumentNullException">xamlDefinition</exception>
        public void Load(StringBuilder xamlDefinition, ref Stream xamlStream,
            ref Queue<PooledServiceActivity> workflowPool, ref Activity workflowActivity)
        {
            if (xamlDefinition == null || xamlDefinition.Length == 0)
            {
                throw new ArgumentNullException("xamlDefinition");
            }

            // Travis.Frisinger : 13.11.2012 - Remove bad namespaces
            
            if (GlobalConstants.RuntimeNamespaceClean)
                
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }
            // End Mods


            var generation = 0;

            using (xamlStream = xamlDefinition.EncodeForXmlDocument())
            {
                var settings = new XamlXmlReaderSettings
                {
                    LocalAssembly = System.Reflection.Assembly.GetAssembly(typeof(VirtualizedContainerService))
                };
                using (var reader = new XamlXmlReader(xamlStream, settings))
                {
                    workflowActivity = ActivityXamlServices.Load(reader);
                }

                xamlStream.Seek(0, SeekOrigin.Begin);
                workflowPool.Clear();

                generation++;

                for (int i = 0; i < GlobalConstants._xamlPoolSize; i++)
                {
                    var activity = ActivityXamlServices.Load(xamlStream);
                    xamlStream.Seek(0, SeekOrigin.Begin);
                    workflowPool.Enqueue(new PooledServiceActivity(generation, activity));
                }
            }
        }
    }
}