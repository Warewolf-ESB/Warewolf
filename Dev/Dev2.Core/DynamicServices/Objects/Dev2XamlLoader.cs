/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (GlobalConstants.runtimeNamespaceClean)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }
            // End Mods


            int generation = 0;

            using (xamlStream = xamlDefinition.EncodeForXmlDocument())
            {
                workflowActivity = ActivityXamlServices.Load(xamlStream);
                xamlStream.Seek(0, SeekOrigin.Begin);
                workflowPool.Clear();

                generation++;

                for (int i = 0; i < GlobalConstants._xamlPoolSize; i++)
                {
                    Activity activity = ActivityXamlServices.Load(xamlStream);
                    xamlStream.Seek(0, SeekOrigin.Begin);
                    workflowPool.Enqueue(new PooledServiceActivity(generation, activity));
                }
            }
        }
    }
}