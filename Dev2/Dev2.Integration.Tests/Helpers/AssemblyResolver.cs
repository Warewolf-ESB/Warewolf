using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Dev2.Integration.Tests.Helpers {
    public class AssemblyResolver {

        public void RegisterAssemblyResolver() {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                string resourceName = "AssemblyLoadingAndReflection." +
                        new AssemblyName(args.Name).Name + ".dll";
                using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }
    }
}
