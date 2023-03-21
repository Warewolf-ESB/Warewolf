using System;
using System.Globalization;
using System.Text;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Hosting
{
	public class RubyCommandLine : CommandLine
	{
		internal new RubyConsoleOptions Options
		{
			get
			{
				return (RubyConsoleOptions)base.Options;
			}
		}

		protected override string Logo
		{
			get
			{
				return GetLogo();
			}
		}

		public static string GetLogo()
		{
			return string.Format(CultureInfo.InvariantCulture, "IronRuby {1} on {2}{0}Copyright (c) Microsoft Corporation. All rights reserved.{0}{0}", new object[3]
			{
				Environment.NewLine,
				RubyContext.IronRubyVersion,
				RubyContext.MakeRuntimeDesriptionString()
			});
		}

		protected override int? TryInteractiveAction()
		{
			try
			{
				return base.TryInteractiveAction();
			}
			catch (ThreadAbortException ex)
			{
				Exception visibleException = RubyUtils.GetVisibleException(ex);
				if (visibleException == ex || visibleException == null)
				{
					throw;
				}
				throw visibleException;
			}
			catch (SystemExit systemExit)
			{
				return systemExit.Status;
			}
		}

		protected override int RunFile(string fileName)
		{
			return RunFile(base.Engine.CreateScriptSourceFromFile(RubyUtils.CanonicalizePath(fileName), GetSourceCodeEncoding()));
		}

		protected override ScriptCodeParseResult GetCommandProperties(string code)
		{
			return CreateCommandSource(code, SourceCodeKind.InteractiveCode, "(ir)").GetCodeProperties(base.Engine.GetCompilerOptions(base.ScriptScope));
		}

		protected override void ExecuteCommand(string command)
		{
			ExecuteCommand(CreateCommandSource(command, SourceCodeKind.InteractiveCode, "(ir)"));
		}

		protected override int RunCommand(string command)
		{
			return RunFile(CreateCommandSource(command, SourceCodeKind.Statements, "-e"));
		}

		private ScriptSource CreateCommandSource(string command, SourceCodeKind kind, string sourceUnitId)
		{
			Encoding sourceCodeEncoding = GetSourceCodeEncoding();
			return base.Engine.CreateScriptSource(new BinaryContentProvider(sourceCodeEncoding.GetBytes(command)), sourceUnitId, sourceCodeEncoding, kind);
		}

		private Encoding GetSourceCodeEncoding()
		{
			return (((RubyContext)base.Language).RubyOptions.DefaultEncoding ?? RubyEncoding.Ascii).Encoding;
		}

		protected override Scope CreateScope()
		{
			Scope scope = base.CreateScope();
			RubyOps.ScopeSetMember(scope, "iron_ruby", base.Engine);
			return scope;
		}

		protected override void UnhandledException(Exception e)
		{
			((RubyContext)base.Language).CurrentException = e;
			base.UnhandledException(e);
		}

		protected override void Shutdown()
		{
			try
			{
				base.Engine.Runtime.Shutdown();
			}
			catch (SystemExit systemExit)
			{
				base.ExitCode = systemExit.Status;
			}
		}
	}
}
