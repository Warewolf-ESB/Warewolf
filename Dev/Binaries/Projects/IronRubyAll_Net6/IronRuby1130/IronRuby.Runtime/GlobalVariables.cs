using IronRuby.Compiler;

namespace IronRuby.Runtime
{
	public static class GlobalVariables
	{
		public static readonly GlobalVariable MatchData = new SpecialGlobalVariableInfo(GlobalVariableId.MatchData);

		public static readonly GlobalVariable EntireMatch = new SpecialGlobalVariableInfo(GlobalVariableId.EntireMatch);

		public static readonly GlobalVariable MatchLastGroup = new SpecialGlobalVariableInfo(GlobalVariableId.MatchLastGroup);

		public static readonly GlobalVariable PreMatch = new SpecialGlobalVariableInfo(GlobalVariableId.PreMatch);

		public static readonly GlobalVariable PostMatch = new SpecialGlobalVariableInfo(GlobalVariableId.PostMatch);

		public static readonly GlobalVariable CurrentException = new SpecialGlobalVariableInfo(GlobalVariableId.CurrentException);

		public static readonly GlobalVariable CurrentExceptionBacktrace = new SpecialGlobalVariableInfo(GlobalVariableId.CurrentExceptionBacktrace);

		public static readonly GlobalVariable CommandLineArguments = new SpecialGlobalVariableInfo(GlobalVariableId.CommandLineArguments);

		public static readonly GlobalVariable LastInputLine = new SpecialGlobalVariableInfo(GlobalVariableId.LastInputLine);

		public static readonly GlobalVariable InputFileName = new SpecialGlobalVariableInfo(GlobalVariableId.InputFileName);

		public static readonly GlobalVariable CommandLineProgramPath = new SpecialGlobalVariableInfo(GlobalVariableId.CommandLineProgramPath);

		public static readonly GlobalVariable InputContent = new SpecialGlobalVariableInfo(GlobalVariableId.InputContent);

		public static readonly GlobalVariable LastInputLineNumber = new SpecialGlobalVariableInfo(GlobalVariableId.LastInputLineNumber);

		public static readonly GlobalVariable InputSeparator = new SpecialGlobalVariableInfo(GlobalVariableId.InputSeparator);

		public static readonly GlobalVariable ItemSeparator = new SpecialGlobalVariableInfo(GlobalVariableId.ItemSeparator);

		public static readonly GlobalVariable StringSeparator = new SpecialGlobalVariableInfo(GlobalVariableId.StringSeparator);

		public static readonly GlobalVariable OutputSeparator = new SpecialGlobalVariableInfo(GlobalVariableId.OutputSeparator);

		public static readonly GlobalVariable LoadPath = new SpecialGlobalVariableInfo(GlobalVariableId.LoadPath);

		public static readonly GlobalVariable LoadedFiles = new SpecialGlobalVariableInfo(GlobalVariableId.LoadedFiles);

		public static readonly GlobalVariable OutputStream = new SpecialGlobalVariableInfo(GlobalVariableId.OutputStream);

		public static readonly GlobalVariable InputStream = new SpecialGlobalVariableInfo(GlobalVariableId.InputStream);

		public static readonly GlobalVariable ErrorOutputStream = new SpecialGlobalVariableInfo(GlobalVariableId.ErrorOutputStream);

		public static readonly GlobalVariable SafeLevel = new SpecialGlobalVariableInfo(GlobalVariableId.SafeLevel);

		public static readonly GlobalVariable Verbose = new SpecialGlobalVariableInfo(GlobalVariableId.Verbose);

		public static readonly GlobalVariable KCode = new SpecialGlobalVariableInfo(GlobalVariableId.KCode);

		public static readonly GlobalVariable ChildProcessExitStatus = new SpecialGlobalVariableInfo(GlobalVariableId.ChildProcessExitStatus);

		internal static void DefineVariablesNoLock(RubyContext context)
		{
			context.DefineGlobalVariableNoLock(Symbols.MatchData, MatchData);
			context.DefineGlobalVariableNoLock(Symbols.EntireMatch, EntireMatch);
			context.DefineGlobalVariableNoLock(Symbols.MatchLastGroup, MatchLastGroup);
			context.DefineGlobalVariableNoLock(Symbols.PreMatch, PreMatch);
			context.DefineGlobalVariableNoLock(Symbols.PostMatch, PostMatch);
			context.DefineGlobalVariableNoLock(Symbols.LastInputLine, LastInputLine);
			context.DefineGlobalVariableNoLock(Symbols.CommandLineProgramPath, CommandLineProgramPath);
			context.DefineGlobalVariableNoLock(Symbols.CurrentException, CurrentException);
			context.DefineGlobalVariableNoLock(Symbols.CurrentExceptionBacktrace, CurrentExceptionBacktrace);
			context.DefineGlobalVariableNoLock(Symbols.CommandLineArguments, CommandLineArguments);
			context.DefineGlobalVariableNoLock(Symbols.InputSeparator, InputSeparator);
			context.DefineGlobalVariableNoLock(Symbols.ItemSeparator, ItemSeparator);
			context.DefineGlobalVariableNoLock(Symbols.StringSeparator, StringSeparator);
			context.DefineGlobalVariableNoLock(Symbols.OutputSeparator, OutputSeparator);
			context.DefineGlobalVariableNoLock(Symbols.InputContent, InputContent);
			context.DefineGlobalVariableNoLock(Symbols.OutputStream, OutputStream);
			context.DefineGlobalVariableNoLock(Symbols.LoadedFiles, LoadedFiles);
			context.DefineGlobalVariableNoLock(Symbols.LoadPath, LoadPath);
			context.DefineGlobalVariableNoLock(Symbols.LastInputLineNumber, LastInputLineNumber);
			context.DefineGlobalVariableNoLock(Symbols.ChildProcessExitStatus, ChildProcessExitStatus);
		}
	}
}
