#pragma warning disable
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Scripting;
using IronRuby;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;









namespace Dev2.Development.Languages.Scripting
{
    public class RubyContext:IScriptingContext
    {
        readonly IStringScriptSources _sources;
#if WIN8
            public static readonly bool IsWin8 = true;
#else
        public static readonly bool IsWin8 = false;
#endif

        public RubyContext(IStringScriptSources sources)
        {
            _sources = sources;
        }

        public string Execute(string scriptValue)
        {
            var rubyEngine = CreateRubyEngine();
            var rubyFunc = @"class System::Object"+Environment.NewLine+"def initialize"+Environment.NewLine+"end"+Environment.NewLine+"end"+Environment.NewLine+"def __result__();" + Environment.NewLine +scriptValue+Environment.NewLine + "end;"+Environment.NewLine+" public :__result__";
            var scope = rubyEngine.CreateScope();
            if (_sources?.GetFileScriptSources() != null)
            {
                foreach(var fileScriptSource in _sources.GetFileScriptSources())
                {
                    rubyEngine.Execute(fileScriptSource.GetReader().ReadToEnd(), scope);
                }
            }
            var source = rubyEngine.CreateScriptSourceFromString(rubyFunc, SourceCodeKind.Statements);

            //execute the source
            source.Execute(scope);

            //get a delegate to the ruby function
            var result = scope.GetVariable<Func<dynamic>>("__result__");

            return result.Invoke().ToString();
        }

        ScriptEngine CreateRubyEngine()
        {
            RuntimeSetup = ScriptRuntimeSetup.ReadConfiguration();
            var languageSetup = RuntimeSetup.AddRubySetup();

            RuntimeSetup.PrivateBinding = true;
            RuntimeSetup.HostType = typeof(TmpHost);
            RuntimeSetup.HostArguments = new object[] { new OptionsAttribute() };
            languageSetup.Options["Verbosity"] = 2;

            var runtime = Ruby.CreateRuntime(RuntimeSetup);
            return Ruby.GetEngine(runtime);
        }

        public ScriptRuntimeSetup RuntimeSetup { get; set; }

        public enScriptType HandlesType() => enScriptType.Ruby;

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
            readonly OptionsAttribute/*!*/ _options;
            readonly PlatformAdaptationLayer/*!*/ _pal;

            public TmpHost(OptionsAttribute/*!*/ options)
            {
                _options = options;
                _pal = options.Pal != null ? (PlatformAdaptationLayer)Activator.CreateInstance(options.Pal) :
                       IsWin8 ? new Win8PAL() :
                       PlatformAdaptationLayer.Default;
            }

            public override PlatformAdaptationLayer PlatformAdaptationLayer => _pal;

            public class Win8PAL : PlatformAdaptationLayer
            {
                string cwd;

                public Win8PAL()
                {
                    var buffer = new StringBuilder(300);
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
                        return IntrospectionExtensions.GetTypeInfo(typeof(Integer)).Assembly;
                    }

                    return base.LoadAssembly(name);
                }

                public override string CurrentDirectory
                {
                    get { return cwd; }
                    set { cwd = value; }
                }

                public override bool FileExists(string path) => false;

                public override bool DirectoryExists(string path) => false;
            }
        }
    }
}
