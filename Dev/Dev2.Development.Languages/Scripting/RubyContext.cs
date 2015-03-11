
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Scripting;
using IronRuby;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Dev2.Development.Languages.Scripting
{
    class RubyContext:IScriptingContext
    {
        #if WIN8
            public static readonly bool IsWin8 = true;
        #else
            public static readonly bool IsWin8 = false;
        #endif


        public string Execute(string scriptValue)
        {
            var rubyEngine = CreateRubyEngine();
            string rubyFunc = @"def __result__();" + scriptValue + "end; public :__result__";
            ScriptSource source = rubyEngine.CreateScriptSourceFromString(rubyFunc, SourceCodeKind.Statements);

            //create a scope to act as the context for the code
            ScriptScope scope = rubyEngine.CreateScope();

            //execute the source
            source.Execute(scope);

            //get a delegate to the ruby function
            var result = scope.GetVariable<Func<dynamic>>("__result__");

            return result.Invoke().ToString();
        }

        public ScriptEngine CreateRubyEngine()
        {
            var runtimeSetup = ScriptRuntimeSetup.ReadConfiguration();
            var languageSetup = runtimeSetup.AddRubySetup();

            runtimeSetup.PrivateBinding = true;
            runtimeSetup.HostType = typeof(TmpHost);
            runtimeSetup.HostArguments = new object[] { new OptionsAttribute() };

            languageSetup.Options["Verbosity"] = 2;

            var runtime = Ruby.CreateRuntime(runtimeSetup);
            return Ruby.GetEngine(runtime);
        }

        public enScriptType HandlesType()
        {
            return enScriptType.Ruby;
        }


        [AttributeUsage(AttributeTargets.Method)]
        [Serializable]
        public sealed class OptionsAttribute : Attribute
        {
            public bool PrivateBinding { get; set; }
            public bool NoRuntime { get; set; }
            public Type Pal { get; set; }
        }


        public class TmpHost : ScriptHost
        {
            private readonly OptionsAttribute/*!*/ _options;
            private readonly PlatformAdaptationLayer/*!*/ _pal;

            public TmpHost(OptionsAttribute/*!*/ options)
            {
                _options = options;
                _pal = options.Pal != null ? (PlatformAdaptationLayer)Activator.CreateInstance(options.Pal) :
                       IsWin8 ? new Win8PAL() :
                       PlatformAdaptationLayer.Default;
            }

            public override PlatformAdaptationLayer PlatformAdaptationLayer
            {
                get { return _pal; }
            }

            public class Win8PAL : PlatformAdaptationLayer
            {
                private string cwd;

                public Win8PAL()
                {
                    StringBuilder buffer = new StringBuilder(300);
                    if (GetCurrentDirectory(buffer.Capacity, buffer) == 0)
                    {
                        throw new IOException();
                    }

                    cwd = buffer.ToString();
                }

                [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
                internal static extern int GetCurrentDirectory(int nBufferLength, [Out] StringBuilder lpBuffer);

                public override Assembly LoadAssembly(string name)
                {
                    if (name.StartsWith("mscorlib"))
                    {
                        return IntrospectionExtensions.GetTypeInfo(typeof(object)).Assembly;
                    }

                    if (name == "IronRuby, Version=1.1.4.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1")
                    {
                        return IntrospectionExtensions.GetTypeInfo(typeof(Ruby)).Assembly;
                    }

                    if (name == "IronRuby.Libraries, Version=1.1.4.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1")
                    {
                        return IntrospectionExtensions.GetTypeInfo(typeof(IronRuby.Builtins.Integer)).Assembly;
                    }

                    return base.LoadAssembly(name);
                }

                public override string CurrentDirectory
                {
                    get { return cwd; }
                    set { cwd = value; }
                }

                public override bool FileExists(string path)
                {
                    return false;
                }

                public override bool DirectoryExists(string path)
                {
                    return false;
                }
            }
        }
    }
}
