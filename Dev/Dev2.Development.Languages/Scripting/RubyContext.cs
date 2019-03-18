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
