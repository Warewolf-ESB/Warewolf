using System;

namespace Log4Net.Async
{
    namespace MyNamespace
    {
        /// <summary>
        /// As Log4Net.Async is not directly referenced in code, MSBuild can exclude it from it's output. Use this attribute in your AssemblyInfo to force it to include it.
        /// </summary>
        [AttributeUsage(AttributeTargets.Assembly)]
        public class ReferencedLibraryAttribute : Attribute
        {
        }
    }
}
