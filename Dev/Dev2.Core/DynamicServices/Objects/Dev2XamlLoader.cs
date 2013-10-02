using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Util;

namespace Dev2.DynamicServices.Objects
{
    /// <summary>
    /// Created to break memory leak in ServiceAction ;)
    /// </summary>
    public class Dev2XamlLoader
    {
        /// <summary>
        /// Loads the specified xaml definition.
        /// </summary>
        /// <param name="xamlDefinition">The xaml definition.</param>
        /// <param name="xamlStream">The xaml stream.</param>
        /// <param name="workflowPool">The workflow pool.</param>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <exception cref="System.ArgumentNullException">xamlDefinition</exception>
        public void Load(string xamlDefinition, ref MemoryStream xamlStream, ref Queue<PooledServiceActivity> workflowPool, ref Activity workflowActivity)
        {
            // Travis.Frisinger : 13.11.2012 - Remove bad namespaces
            if(GlobalConstants.runtimeNamespaceClean)
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }
            // End Mods

            if(string.IsNullOrEmpty(xamlDefinition))
            {
                throw new ArgumentNullException("xamlDefinition");
            }

            int generation = 0;

            using(xamlStream = new MemoryStream(Encoding.UTF8.GetBytes(xamlDefinition)))
            {
                workflowActivity = ActivityXamlServices.Load(xamlStream);
                xamlStream.Seek(0, SeekOrigin.Begin);
                workflowPool.Clear();

                generation++;

                for(int i = 0; i < GlobalConstants._xamlPoolSize; i++)
                {
                    Activity activity = ActivityXamlServices.Load(xamlStream);
                    xamlStream.Seek(0, SeekOrigin.Begin);
                    workflowPool.Enqueue(new PooledServiceActivity(generation, activity));
                }
            }
        }
    }
}
