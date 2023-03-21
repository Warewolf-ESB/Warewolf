using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	internal sealed class SpecialGlobalVariableInfo : GlobalVariable
	{
		private readonly GlobalVariableId _id;

		internal SpecialGlobalVariableInfo(GlobalVariableId id)
		{
			_id = id;
		}

		public override object GetValue(RubyContext context, RubyScope scope)
		{
			switch (_id)
			{
			case GlobalVariableId.MatchData:
				if (scope == null)
				{
					return null;
				}
				return scope.GetInnerMostClosureScope().CurrentMatch;
			case GlobalVariableId.MatchLastGroup:
				if (scope == null)
				{
					return null;
				}
				return scope.GetInnerMostClosureScope().GetCurrentMatchLastGroup();
			case GlobalVariableId.PreMatch:
				if (scope == null)
				{
					return null;
				}
				return scope.GetInnerMostClosureScope().GetCurrentPreMatch();
			case GlobalVariableId.PostMatch:
				if (scope == null)
				{
					return null;
				}
				return scope.GetInnerMostClosureScope().GetCurrentPostMatch();
			case GlobalVariableId.EntireMatch:
				if (scope == null)
				{
					return null;
				}
				return scope.GetInnerMostClosureScope().GetCurrentMatchGroup(0);
			case GlobalVariableId.CurrentException:
				return context.CurrentException;
			case GlobalVariableId.CurrentExceptionBacktrace:
				return context.GetCurrentExceptionBacktrace();
			case GlobalVariableId.InputContent:
				return context.InputProvider.Singleton;
			case GlobalVariableId.InputFileName:
				return context.InputProvider.CurrentFileName;
			case GlobalVariableId.LastInputLine:
				if (scope == null)
				{
					return null;
				}
				return scope.GetInnerMostClosureScope().LastInputLine;
			case GlobalVariableId.LastInputLineNumber:
				return context.InputProvider.LastInputLineNumber;
			case GlobalVariableId.CommandLineArguments:
				return context.InputProvider.CommandLineArguments;
			case GlobalVariableId.OutputStream:
				return context.StandardOutput;
			case GlobalVariableId.ErrorOutputStream:
				return context.StandardErrorOutput;
			case GlobalVariableId.InputStream:
				return context.StandardInput;
			case GlobalVariableId.InputSeparator:
				return context.InputSeparator;
			case GlobalVariableId.OutputSeparator:
				return context.OutputSeparator;
			case GlobalVariableId.StringSeparator:
				return context.StringSeparator;
			case GlobalVariableId.ItemSeparator:
				return context.ItemSeparator;
			case GlobalVariableId.LoadPath:
				return context.Loader.LoadPaths;
			case GlobalVariableId.LoadedFiles:
				return context.Loader.LoadedFiles;
			case GlobalVariableId.SafeLevel:
				return context.CurrentSafeLevel;
			case GlobalVariableId.Verbose:
				return context.Verbose;
			case GlobalVariableId.KCode:
				context.ReportWarning("variable $KCODE is no longer effective");
				return null;
			case GlobalVariableId.ChildProcessExitStatus:
				return context.ChildProcessExitStatus;
			case GlobalVariableId.CommandLineProgramPath:
				return context.CommandLineProgramPath;
			default:
				throw Assert.Unreachable;
			}
		}

		public override void SetValue(RubyContext context, RubyScope scope, string name, object value)
		{
			switch (_id)
			{
			case GlobalVariableId.MatchData:
				if (scope == null)
				{
					throw ReadOnlyError(name);
				}
				scope.GetInnerMostClosureScope().CurrentMatch = ((value != null) ? RequireType<MatchData>(value, name, "MatchData") : null);
				break;
			case GlobalVariableId.EntireMatch:
			case GlobalVariableId.MatchLastGroup:
			case GlobalVariableId.PreMatch:
			case GlobalVariableId.PostMatch:
				throw ReadOnlyError(name);
			case GlobalVariableId.CurrentException:
				context.SetCurrentException(value);
				break;
			case GlobalVariableId.CurrentExceptionBacktrace:
				context.SetCurrentExceptionBacktrace(value);
				break;
			case GlobalVariableId.LastInputLine:
				if (scope == null)
				{
					throw ReadOnlyError(name);
				}
				scope.GetInnerMostClosureScope().LastInputLine = value;
				break;
			case GlobalVariableId.LastInputLineNumber:
				context.InputProvider.LastInputLineNumber = RequireType<int>(value, name, "Fixnum");
				break;
			case GlobalVariableId.InputFileName:
			case GlobalVariableId.CommandLineArguments:
				throw ReadOnlyError(name);
			case GlobalVariableId.OutputStream:
				context.StandardOutput = RequireWriteProtocol(context, value, name);
				break;
			case GlobalVariableId.ErrorOutputStream:
				context.StandardErrorOutput = RequireWriteProtocol(context, value, name);
				break;
			case GlobalVariableId.InputStream:
				context.StandardInput = value;
				break;
			case GlobalVariableId.InputContent:
				throw ReadOnlyError(name);
			case GlobalVariableId.InputSeparator:
				context.InputSeparator = ((value != null) ? RequireType<MutableString>(value, name, "String") : null);
				break;
			case GlobalVariableId.OutputSeparator:
				context.OutputSeparator = ((value != null) ? RequireType<MutableString>(value, name, "String") : null);
				break;
			case GlobalVariableId.StringSeparator:
				context.StringSeparator = value;
				break;
			case GlobalVariableId.ItemSeparator:
				context.ItemSeparator = ((value != null) ? RequireType<MutableString>(value, name, "String") : null);
				break;
			case GlobalVariableId.LoadPath:
			case GlobalVariableId.LoadedFiles:
				throw ReadOnlyError(name);
			case GlobalVariableId.SafeLevel:
				context.SetSafeLevel(RequireType<int>(value, name, "Fixnum"));
				break;
			case GlobalVariableId.Verbose:
				context.Verbose = value;
				break;
			case GlobalVariableId.CommandLineProgramPath:
				context.CommandLineProgramPath = ((value != null) ? RequireType<MutableString>(value, name, "String") : null);
				break;
			case GlobalVariableId.KCode:
				context.ReportWarning("variable $KCODE is no longer effective");
				break;
			case GlobalVariableId.ChildProcessExitStatus:
				throw ReadOnlyError(name);
			default:
				throw Assert.Unreachable;
			}
		}

		private object RequireWriteProtocol(RubyContext context, object value, string variableName)
		{
			if (!context.RespondTo(value, "write"))
			{
				throw RubyExceptions.CreateTypeError(string.Format("${0} must have write method, {1} given", variableName, context.GetClassDisplayName(value)));
			}
			return value;
		}
	}
}
