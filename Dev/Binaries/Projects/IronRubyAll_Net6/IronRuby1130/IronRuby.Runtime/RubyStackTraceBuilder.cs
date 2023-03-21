using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Interpreter;

namespace IronRuby.Runtime
{
	internal sealed class RubyStackTraceBuilder
	{
		internal const string TopLevelMethodName = "#";

		private const int NextFrameLine = int.MinValue;

		internal const string InterpretedCallSiteName = "CallSite.Target";

		private const char NamePartsSeparator = ':';

		private const string RubyMethodPrefix = "ℑℜ:";

		internal const int MaxDebugModePathSize = 256;

		private readonly RubyArray _trace;

		private readonly bool _hasFileAccessPermission;

		private readonly bool _exceptionDetail;

		private readonly RubyEncoding _encoding;

		private IList<InterpretedFrameInfo> _interpretedFrames;

		private int _interpretedFrameIndex;

		private string _nextFrameMethodName;

		private static int _Id;

		public RubyArray RubyTrace
		{
			get
			{
				return _trace;
			}
		}

		private RubyStackTraceBuilder(RubyContext context)
		{
			_hasFileAccessPermission = DetectFileAccessPermissions();
			_exceptionDetail = context.Options.ExceptionDetail;
			_encoding = context.GetPathEncoding();
			_trace = new RubyArray();
		}

		internal RubyStackTraceBuilder(RubyContext context, Exception exception, StackTrace catchSiteTrace, bool isCatchSiteInterpreted)
			: this(context)
		{
			StackTrace clrStackTrace = GetClrStackTrace(exception);
			_interpretedFrames = InterpretedFrame.GetExceptionStackTrace(exception);
			AddBacktrace(clrStackTrace.GetFrames(), 0, false);
			if (catchSiteTrace != null)
			{
				AddBacktrace(catchSiteTrace.GetFrames(), (!isCatchSiteInterpreted) ? 1 : 0, isCatchSiteInterpreted);
			}
		}

		internal RubyStackTraceBuilder(RubyContext context, int skipFrames)
			: this(context)
		{
			StackTrace clrStackTrace = GetClrStackTrace(null);
			_interpretedFrames = ((InterpretedFrame.CurrentFrame.Value != null) ? new List<InterpretedFrameInfo>(InterpretedFrame.CurrentFrame.Value.GetStackTraceDebugInfo()) : null);
			AddBacktrace(clrStackTrace.GetFrames(), skipFrames, false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static StackTrace GetClrStackTrace(Exception exception)
		{
			if (exception == null)
			{
				return new StackTrace(true);
			}
			return new StackTrace(exception, true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string GetFileName(StackFrame frame)
		{
			return frame.GetFileName();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string GetAssemblyName(Assembly assembly)
		{
			return assembly.GetName().Name;
		}

		private void InitializeInterpretedFrames()
		{
		}

		private void AddBacktrace(IEnumerable<StackFrame> stackTrace, int skipFrames, bool skipInterpreterRunMethod)
		{
			if (stackTrace == null)
			{
				return;
			}
			foreach (StackFrame item in InterpretedFrame.GroupStackFrames(stackTrace))
			{
				string fileName;
				int line;
				string methodName;
				if (_interpretedFrames != null && _interpretedFrameIndex < _interpretedFrames.Count && InterpretedFrame.IsInterpretedFrame(item.GetMethod()))
				{
					if (skipInterpreterRunMethod)
					{
						skipInterpreterRunMethod = false;
						continue;
					}
					InterpretedFrameInfo interpretedFrameInfo = _interpretedFrames[_interpretedFrameIndex++];
					if (interpretedFrameInfo.DebugInfo != null)
					{
						fileName = interpretedFrameInfo.DebugInfo.FileName;
						line = interpretedFrameInfo.DebugInfo.StartLine;
					}
					else
					{
						fileName = null;
						line = 0;
					}
					methodName = interpretedFrameInfo.MethodName;
					TryParseRubyMethodName(ref methodName, ref fileName, ref line);
					if (methodName == "CallSite.Target")
					{
						continue;
					}
				}
				else
				{
					if (!TryGetStackFrameInfo(item, out methodName, out fileName, out line))
					{
						continue;
					}
					if (line == int.MinValue)
					{
						_nextFrameMethodName = methodName;
						continue;
					}
				}
				if (_nextFrameMethodName != null)
				{
					if (skipFrames == 0)
					{
						_trace.Add(MutableString.Create(FormatFrame(fileName, line, _nextFrameMethodName), _encoding));
					}
					else
					{
						skipFrames--;
					}
					_nextFrameMethodName = null;
				}
				if (skipFrames == 0)
				{
					_trace.Add(MutableString.Create(FormatFrame(fileName, line, methodName), _encoding));
				}
				else
				{
					skipFrames--;
				}
			}
		}

		private static string FormatFrame(string file, int line, string methodName)
		{
			if (string.IsNullOrEmpty(methodName))
			{
				return string.Format("{0}:{1}", file, line);
			}
			return string.Format("{0}:{1}:in `{2}'", file, line, methodName);
		}

		private bool TryGetStackFrameInfo(StackFrame frame, out string methodName, out string fileName, out int line)
		{
			MethodBase method = frame.GetMethod();
			methodName = method.Name;
			fileName = (_hasFileAccessPermission ? GetFileName(frame) : null);
			int num = (line = ((!PlatformAdaptationLayer.IsCompactFramework) ? frame.GetFileLineNumber() : 0));
			if (TryParseRubyMethodName(ref methodName, ref fileName, ref line))
			{
				RubyMethodDebugInfo info;
				if (num == 0 && RubyMethodDebugInfo.TryGet(method, out info))
				{
					int iLOffset = frame.GetILOffset();
					if (iLOffset >= 0)
					{
						int num2 = info.Map(iLOffset);
						if (num2 != 0)
						{
							line = num2;
						}
					}
				}
				return true;
			}
			if (method.IsDefined(typeof(RubyStackTraceHiddenAttribute), false))
			{
				return false;
			}
			object[] customAttributes = method.GetCustomAttributes(typeof(RubyMethodAttribute), false);
			if (customAttributes.Length > 0)
			{
				methodName = ((RubyMethodAttribute)customAttributes[0]).Name;
				if (!_exceptionDetail)
				{
					fileName = null;
					line = int.MinValue;
				}
				return true;
			}
			if (_exceptionDetail || IsVisibleClrFrame(method))
			{
				if (string.IsNullOrEmpty(fileName) && method.DeclaringType != null)
				{
					fileName = (_hasFileAccessPermission ? GetAssemblyName(method.DeclaringType.Assembly) : null);
					line = 0;
				}
				return true;
			}
			return false;
		}

		private static bool IsVisibleClrFrame(MethodBase method)
		{
			if (DynamicSiteHelpers.IsInvisibleDlrStackFrame(method))
			{
				return false;
			}
			Type declaringType = method.DeclaringType;
			if (declaringType != null && (declaringType.Assembly == typeof(RubyOps).Assembly || (declaringType.Namespace != null && (declaringType.Namespace.StartsWith("IronRuby.StandardLibrary", StringComparison.Ordinal) || declaringType.Namespace.StartsWith("IronRuby.Builtins", StringComparison.Ordinal)))))
			{
				return false;
			}
			return true;
		}

		internal static string EncodeMethodName(string methodName, string sourcePath, SourceSpan location, bool debugMode)
		{
			return new StringBuilder().Append("ℑℜ:").Append(methodName).Append(':')
				.Append(location.IsValid ? location.Start.Line : 0)
				.Append(':')
				.Append(Interlocked.Increment(ref _Id))
				.Append(':')
				.Append((debugMode && sourcePath != null && sourcePath.Length > 256) ? sourcePath.Substring(0, 256) : sourcePath)
				.ToString();
		}

		internal static bool TryParseRubyMethodName(ref string methodName, ref string fileName, ref int line)
		{
			if (methodName != null && methodName.StartsWith("ℑℜ:", StringComparison.Ordinal))
			{
				string text = methodName;
				int length = "ℑℜ:".Length;
				int num = text.IndexOf(':', length);
				if (num < 0)
				{
					return false;
				}
				methodName = text.Substring(length, num - length);
				if (methodName == "#")
				{
					methodName = null;
				}
				length = num + 1;
				num = text.IndexOf(':', length);
				if (num < 0)
				{
					return false;
				}
				if (line == 0 && !int.TryParse(text.Substring(length, num - length), out line))
				{
					return false;
				}
				length = num + 1;
				num = text.IndexOf(':', length);
				if (num < 0)
				{
					return false;
				}
				if (fileName == null)
				{
					fileName = text.Substring(num + 1);
				}
				return true;
			}
			return false;
		}

		private static string ParseRubyMethodName(string lambdaName)
		{
			if (!lambdaName.StartsWith("ℑℜ:", StringComparison.Ordinal))
			{
				return lambdaName;
			}
			int num = lambdaName.IndexOf(':', "ℑℜ:".Length);
			string text = lambdaName.Substring("ℑℜ:".Length, num - "ℑℜ:".Length);
			if (!(text != "#"))
			{
				return null;
			}
			return text;
		}

		private static bool DetectFileAccessPermissions()
		{
			try
			{
				new FileIOPermission(PermissionState.Unrestricted).Demand();
				return true;
			}
			catch (SecurityException)
			{
				return false;
			}
		}
	}
}
