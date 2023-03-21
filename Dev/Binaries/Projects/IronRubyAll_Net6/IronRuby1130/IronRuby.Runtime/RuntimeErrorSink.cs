using System;
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;

namespace IronRuby.Runtime
{
	public class RuntimeErrorSink : ErrorCounter
	{
		private readonly RubyContext _context;

		private CallSite<Func<CallSite, object, object, object>> _WriteSite;

		internal RuntimeErrorSink(RubyContext context)
		{
			_context = context;
		}

		internal void WriteMessage(MutableString message)
		{
			if (_WriteSite == null)
			{
				Interlocked.CompareExchange(ref _WriteSite, CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(_context, "write", 1)), null);
			}
			_WriteSite.Target(_WriteSite, _context.StandardErrorOutput, message);
		}

		public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity)
		{
			if (severity == Severity.Warning && !ReportWarning(_context.Verbose, errorCode))
			{
				return;
			}
			CountError(severity);
			int line = span.Start.Line;
			string file;
			string lineSourceCode;
			RubyEncoding encoding;
			if (sourceUnit != null)
			{
				file = sourceUnit.Path;
				using (SourceCodeReader sourceCodeReader = sourceUnit.GetReader())
				{
					if (line > 0)
					{
						try
						{
							sourceCodeReader.SeekLine(line);
							lineSourceCode = sourceCodeReader.ReadLine();
						}
						catch (Exception)
						{
							lineSourceCode = null;
						}
					}
					else
					{
						lineSourceCode = null;
					}
					encoding = ((sourceCodeReader.Encoding != null) ? RubyEncoding.GetRubyEncoding(sourceCodeReader.Encoding) : RubyEncoding.UTF8);
				}
			}
			else
			{
				file = null;
				lineSourceCode = null;
				encoding = RubyEncoding.UTF8;
			}
			if (severity == Severity.Error || severity == Severity.FatalError)
			{
				throw new SyntaxError(message, file, line, span.Start.Column, lineSourceCode);
			}
			WriteMessage(MutableString.Create(RubyContext.FormatErrorMessage(message, "warning", file, line, span.Start.Column, null), encoding));
		}

		private static bool ReportWarning(object verbose, int errorCode)
		{
			if (verbose is bool)
			{
				if (!(bool)verbose)
				{
					return !Errors.IsVerboseWarning(errorCode);
				}
				return true;
			}
			return false;
		}
	}
}
