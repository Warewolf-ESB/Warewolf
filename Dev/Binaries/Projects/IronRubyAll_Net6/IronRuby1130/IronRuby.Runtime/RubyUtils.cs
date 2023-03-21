using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public static class RubyUtils
	{
		public class RecursionTracker
		{
			private class RecursionHandle : IDisposable
			{
				private readonly Dictionary<object, bool> _tracker;

				private readonly object _obj;

				internal RecursionHandle(Dictionary<object, bool> tracker, object obj)
				{
					_tracker = tracker;
					_obj = obj;
				}

				public void Dispose()
				{
					_tracker.Remove(_obj);
				}
			}

			[ThreadStatic]
			private Dictionary<object, bool> _infiniteTracker;

			private Dictionary<object, bool> TryPushInfinite(object obj)
			{
				if (_infiniteTracker == null)
				{
					_infiniteTracker = new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance);
				}
				Dictionary<object, bool> infiniteTracker = _infiniteTracker;
				if (infiniteTracker.ContainsKey(obj))
				{
					return null;
				}
				infiniteTracker.Add(obj, true);
				return infiniteTracker;
			}

			public IDisposable TrackObject(object obj)
			{
				obj = CustomStringDictionary.NullToObj(obj);
				Dictionary<object, bool> dictionary = TryPushInfinite(obj);
				if (dictionary != null)
				{
					return new RecursionHandle(dictionary, obj);
				}
				return null;
			}
		}

		private class AsyncExceptionMarker
		{
			internal Exception Exception { get; set; }

			internal AsyncExceptionMarker(Exception e)
			{
				Exception = e;
			}
		}

		private class ThreadExitMarker
		{
		}

		private class PathExpander
		{
			private List<string> _pathComponents = new List<string>();

			private string _root;

			internal PathExpander(PlatformAdaptationLayer platform, string absoluteBasePath)
			{
				string pathAfterRoot = null;
				_root = GetPathRoot(platform, absoluteBasePath, out pathAfterRoot);
				pathAfterRoot = pathAfterRoot.TrimStart('/');
				AddRelativePath(pathAfterRoot);
			}

			internal void AddRelativePath(string relPath)
			{
				string[] array = relPath.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (text == "..")
					{
						if (_pathComponents.Count != 0)
						{
							_pathComponents.RemoveAt(_pathComponents.Count - 1);
						}
					}
					else if (!(text == "."))
					{
						_pathComponents.Add(text);
					}
				}
			}

			internal string GetResult()
			{
				StringBuilder stringBuilder = new StringBuilder(_root);
				if (_pathComponents.Count >= 1)
				{
					string text = _pathComponents[_pathComponents.Count - 1];
					if (FileSystemUsesDriveLetters && !string.IsNullOrEmpty(text.TrimEnd('.')))
					{
						_pathComponents[_pathComponents.Count - 1] = text.TrimEnd('.');
					}
				}
				for (int i = 0; i < _pathComponents.Count; i++)
				{
					stringBuilder.Append(_pathComponents[i]);
					if (i < _pathComponents.Count - 1)
					{
						stringBuilder.Append('/');
					}
				}
				return stringBuilder.ToString();
			}
		}

		private const int CachedCharCount = 256;

		public static readonly int FalseObjectId = 0;

		public static readonly int TrueObjectId = 2;

		public static readonly int NilObjectId = 4;

		private static object[] _charCache = new object[256];

		internal static readonly HashSet<string> NotUnmangledObject = new HashSet<string>
		{
			"class", "clone", "display", "dup", "extend", "freeze", "hash", "initialize", "inspect", "instance_eval",
			"instance_exec", "instance_variable_get", "instance_variable_set", "instance_variables", "method", "methods", "object_id", "private_methods", "protected_methods", "public_methods",
			"send", "singleton_methods", "taint", "untaint"
		};

		internal static readonly HashSet<string> NotMangledObject = new HashSet<string>
		{
			"Class", "Clone", "Display", "Dup", "Extend", "Freeze", "Hash", "Initialize", "Inspect", "InstanceEval",
			"InstanceExec", "InstanceVariableGet", "InstanceVariableSet", "InstanceVariables", "Method", "Methods", "ObjectId", "PrivateMethods", "ProtectedMethods", "PublicMethods",
			"Send", "SingletonMethods", "Taint", "Untaint"
		};

		public static readonly MutableString InfiniteRecursionMarker = MutableString.CreateAscii("[...]").Freeze();

		private static readonly RecursionTracker _infiniteInspectTracker = new RecursionTracker();

		private static readonly RecursionTracker _infiniteToSTracker = new RecursionTracker();

		private static readonly Type[] _ccTypes1 = new Type[1] { typeof(RubyClass) };

		private static readonly Type[] _ccTypes2 = new Type[1] { typeof(RubyContext) };

		private static readonly Type[] _serializableTypeSignature = new Type[2]
		{
			typeof(SerializationInfo),
			typeof(StreamingContext)
		};

		public static readonly string SerializationInfoClassKey = "#immediateClass";

		public static RecursionTracker InfiniteInspectTracker
		{
			get
			{
				return _infiniteInspectTracker;
			}
		}

		public static RecursionTracker InfiniteToSTracker
		{
			get
			{
				return _infiniteToSTracker;
			}
		}

		public static bool FileSystemUsesDriveLetters
		{
			get
			{
				return Path.DirectorySeparatorChar == '\\';
			}
		}

		public static bool IsRubyValueType(object obj)
		{
			if (obj == null || obj is ValueType || obj is RubySymbol)
			{
				if (!(obj is float))
				{
					return !(obj is double);
				}
				return false;
			}
			return false;
		}

		public static bool HasObjectState(object obj)
		{
			return !IsRubyValueType(obj);
		}

		public static bool CanClone(object obj)
		{
			return !IsRubyValueType(obj);
		}

		public static bool CanDefineSingletonMethod(object obj)
		{
			if (obj is ValueType || obj is RubySymbol || obj is BigInteger)
			{
				return obj is bool;
			}
			return true;
		}

		public static bool HasSingletonClass(object obj)
		{
			if (!(obj is int))
			{
				return !(obj is RubySymbol);
			}
			return false;
		}

		public static MutableString InspectObject(UnaryOpStorage inspectStorage, ConversionStorage<MutableString> tosConversion, object obj)
		{
			RubyContext context = tosConversion.Context;
			using (IDisposable disposable = InfiniteInspectTracker.TrackObject(obj))
			{
				if (disposable == null)
				{
					return MutableString.CreateAscii("...");
				}
				MutableString mutableString = MutableString.CreateMutable(context.GetIdentifierEncoding());
				mutableString.Append("#<");
				mutableString.Append(context.GetClassDisplayName(obj));
				mutableString.Append(':');
				AppendFormatHexObjectId(mutableString, GetObjectId(context, obj));
				RubyInstanceData rubyInstanceData = context.TryGetInstanceData(obj);
				if (rubyInstanceData != null)
				{
					List<KeyValuePair<string, object>> instanceVariablePairs = rubyInstanceData.GetInstanceVariablePairs();
					bool flag = true;
					foreach (KeyValuePair<string, object> item in instanceVariablePairs)
					{
						if (flag)
						{
							mutableString.Append(' ');
							flag = false;
						}
						else
						{
							mutableString.Append(", ");
						}
						mutableString.Append(item.Key);
						mutableString.Append('=');
						CallSite<Func<CallSite, object, object>> callSite = inspectStorage.GetCallSite("inspect");
						object arg = callSite.Target(callSite, item.Value);
						CallSite<Func<CallSite, object, MutableString>> site = tosConversion.GetSite(ConvertToSAction.Make(context));
						mutableString.Append(site.Target(site, arg));
						mutableString.TaintBy(item.Value, context);
					}
				}
				mutableString.Append(">");
				mutableString.TaintBy(obj, context);
				return mutableString;
			}
		}

		public static MutableString FormatObjectPrefix(RubyContext context, string className, long objectId, bool isTainted, bool isUntrusted)
		{
			MutableString mutableString = MutableString.CreateMutable(context.GetIdentifierEncoding());
			mutableString.Append("#<");
			mutableString.Append(className);
			mutableString.Append(':');
			AppendFormatHexObjectId(mutableString, objectId);
			mutableString.IsTainted |= isTainted;
			mutableString.IsUntrusted |= isUntrusted;
			return mutableString;
		}

		public static MutableString FormatObject(RubyContext context, string className, long objectId, bool isTainted, bool isUntrusted)
		{
			return FormatObjectPrefix(context, className, objectId, isTainted, isUntrusted).Append(">");
		}

		public static MutableString ObjectToMutableString(RubyContext context, object obj)
		{
			bool tainted;
			bool untrusted;
			context.GetObjectTrust(obj, out tainted, out untrusted);
			return FormatObject(context, context.GetClassDisplayName(obj), GetObjectId(context, obj), tainted, untrusted);
		}

		public static MutableString ObjectToMutableStringPrefix(RubyContext context, object obj)
		{
			bool tainted;
			bool untrusted;
			context.GetObjectTrust(obj, out tainted, out untrusted);
			return FormatObjectPrefix(context, context.GetClassDisplayName(obj), GetObjectId(context, obj), tainted, untrusted);
		}

		public static MutableString AppendFormatHexObjectId(MutableString str, long objectId)
		{
			return str.AppendFormat("0x{0:x7}", 2 * objectId);
		}

		public static MutableString ObjectToMutableString(IRubyObject self)
		{
			return FormatObject(self.ImmediateClass.Context, self.ImmediateClass.GetNonSingletonClass().Name, self.GetInstanceData().ObjectId, self.IsTainted, self.IsUntrusted);
		}

		public static MutableString ObjectBaseToMutableString(IRubyObject self)
		{
			if (self is RubyObject)
			{
				return ObjectToMutableString(self);
			}
			return MutableString.CreateMutable(self.BaseToString(), RubyEncoding.UTF8);
		}

		public static bool TryDuplicateObject(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, object obj, bool cloneSemantics, out object copy)
		{
			if (!CanClone(obj))
			{
				copy = null;
				return false;
			}
			RubyContext context = allocateStorage.Context;
			IDuplicable duplicable = obj as IDuplicable;
			if (duplicable != null)
			{
				copy = duplicable.Duplicate(context, cloneSemantics);
			}
			else
			{
				CallSite<Func<CallSite, RubyClass, object>> callSite = allocateStorage.GetCallSite("allocate", 0);
				copy = callSite.Target(callSite, context.GetClassOf(obj));
				context.CopyInstanceData(obj, copy, cloneSemantics);
			}
			CallSite<Func<CallSite, object, object, object>> callSite2 = initializeCopyStorage.GetCallSite("initialize_copy", 1);
			callSite2.Target(callSite2, copy, obj);
			if (cloneSemantics)
			{
				context.FreezeObjectBy(copy, obj);
			}
			return true;
		}

		public static long GetFixnumId(int number)
		{
			return ((long)number << 1) + 1;
		}

		public static long GetObjectId(RubyContext context, object obj)
		{
			if (obj == null)
			{
				return NilObjectId;
			}
			if (obj is bool)
			{
				return ((bool)obj) ? TrueObjectId : FalseObjectId;
			}
			if (obj is int)
			{
				return GetFixnumId((int)obj);
			}
			return context.GetInstanceData(obj).ObjectId;
		}

		public static object CharToObject(char ch)
		{
			if (ch >= 'Ā')
			{
				return ch;
			}
			return _charCache[(uint)ch] ?? (_charCache[(uint)ch] = ch);
		}

		public static bool HasUnmangledName(string name)
		{
			return !NotUnmangledObject.Contains(name);
		}

		public static string TryUnmangleMethodName(string name)
		{
			if (!HasUnmangledName(name))
			{
				return null;
			}
			return TryUnmangleName(name);
		}

		public static string TryUnmangleName(string name)
		{
			ContractUtils.RequiresNotNull(name, "name");
			if (name.Length == 0)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			int num = 0;
			int i = 0;
			while (num < name.Length)
			{
				for (; i < name.Length; i++)
				{
					char c;
					if ((c = name[i]) == '_')
					{
						break;
					}
					if (char.IsUpper(c))
					{
						return null;
					}
				}
				if (i == num || i == name.Length - 1)
				{
					return null;
				}
				if (i - num == 1)
				{
					if (flag)
					{
						return null;
					}
					stringBuilder.Append(name[num].ToUpperInvariant());
					flag = false;
				}
				else
				{
					string text = MapSpecialWord(name, num, i - num);
					if (text != null)
					{
						if (flag)
						{
							return null;
						}
						stringBuilder.Append(text.ToUpperInvariant());
						flag = true;
					}
					else
					{
						stringBuilder.Append(name[num].ToUpperInvariant());
						stringBuilder.Append(name, num + 1, i - num - 1);
						flag = false;
					}
				}
				num = ++i;
			}
			return stringBuilder.ToString();
		}

		public static bool HasMangledName(string name)
		{
			return !NotMangledObject.Contains(name);
		}

		public static string TryMangleMethodName(string name)
		{
			if (!HasMangledName(name))
			{
				return null;
			}
			return TryMangleName(name);
		}

		public static string TryMangleName(string name)
		{
			ContractUtils.RequiresNotNull(name, "name");
			StringBuilder stringBuilder = null;
			int num = 0;
			while (num < name.Length)
			{
				char c = name[num];
				if (char.IsUpper(c))
				{
					int i;
					for (i = num + 1; i < name.Length && char.IsUpper(name, i); i++)
					{
					}
					if (i < name.Length)
					{
						i--;
					}
					if (stringBuilder == null)
					{
						stringBuilder = new StringBuilder();
						stringBuilder.Append(name, 0, num);
					}
					if (num > 0)
					{
						stringBuilder.Append('_');
					}
					int num2 = i - num;
					switch (num2)
					{
					case 0:
						if (num + 2 < name.Length && char.IsUpper(name[num + 2]) && (num + 3 == name.Length || (char.IsUpper(name[num + 3]) && num + 4 < name.Length && !char.IsUpper(name[num + 4]))))
						{
							return null;
						}
						stringBuilder.Append(c.ToLowerInvariant());
						num++;
						continue;
					case 1:
						stringBuilder.Append(c.ToLowerInvariant());
						num++;
						continue;
					}
					string text = MapSpecialWord(name, num, num2);
					if (text != null)
					{
						stringBuilder.Append(text.ToLowerInvariant());
						num = i;
						continue;
					}
					return null;
				}
				if (c == '_')
				{
					return null;
				}
				if (stringBuilder != null)
				{
					stringBuilder.Append(c);
				}
				num++;
			}
			if (stringBuilder == null)
			{
				return null;
			}
			return stringBuilder.ToString();
		}

		private static string MapSpecialWord(string name, int start, int count)
		{
			if (count == 2)
			{
				if (!IsTwoLetterWord(name, start))
				{
					return name.Substring(start, count);
				}
				return null;
			}
			return null;
		}

		private static bool IsTwoLetterWord(string str, int index)
		{
			switch (LetterPair(str, index))
			{
			case 24947:
			case 24948:
			case 25209:
			case 25711:
			case 26479:
			case 26980:
			case 26982:
			case 26990:
			case 26995:
			case 26996:
			case 28005:
			case 28025:
			case 28271:
			case 28518:
			case 28523:
			case 28526:
			case 28530:
			case 29807:
			case 30064:
				return true;
			default:
				return false;
			}
		}

		private static int LetterPair(string str, int index)
		{
			if ((str[index + 1] & 0xFF00u) != 0)
			{
				return -1;
			}
			return (int)(((uint)str[index].ToLowerInvariant() << 8) | str[index + 1].ToLowerInvariant());
		}

		public static void CheckConstantName(string name)
		{
			if (!Tokenizer.IsConstantName(name))
			{
				throw RubyExceptions.CreateNameError(string.Format("`{0}' is not allowed as a constant name", name));
			}
		}

		public static void CheckClassVariableName(string name)
		{
			if (!Tokenizer.IsClassVariableName(name))
			{
				throw RubyExceptions.CreateNameError(string.Format("`{0}' is not allowed as a class variable name", name));
			}
		}

		public static void CheckInstanceVariableName(string name)
		{
			if (!Tokenizer.IsInstanceVariableName(name))
			{
				throw RubyExceptions.CreateNameError(string.Format("`{0}' is not allowed as an instance variable name", name));
			}
		}

		public static object GetConstant(RubyGlobalScope globalScope, RubyModule owner, string name, bool lookupObject)
		{
			using (owner.Context.ClassHierarchyLocker())
			{
				ConstantStorage value;
				if (owner.TryResolveConstantNoLock(globalScope, name, out value))
				{
					return value.Value;
				}
				RubyClass objectClass = owner.Context.ObjectClass;
				if (owner != objectClass && lookupObject && objectClass.TryResolveConstantNoLock(globalScope, name, out value))
				{
					return value.Value;
				}
			}
			CheckConstantName(name);
			return owner.ConstantMissing(name);
		}

		public static void SetConstant(RubyModule owner, string name, object value)
		{
			if (owner.SetConstantChecked(name, value))
			{
				owner.Context.ReportWarning(string.Format("already initialized constant {0}", name));
			}
			RubyModule rubyModule = value as RubyModule;
			if (rubyModule != null)
			{
				if (rubyModule.Name == null)
				{
					rubyModule.Name = owner.MakeNestedModuleName(name);
				}
				if (owner.IsObjectClass)
				{
					rubyModule.Publish(name);
				}
			}
		}

		public static RubyMethodVisibility GetSpecialMethodVisibility(RubyMethodVisibility visibility, string methodName)
		{
			if (!(methodName == Symbols.Initialize) && !(methodName == Symbols.InitializeCopy))
			{
				return visibility;
			}
			return RubyMethodVisibility.Private;
		}

		internal static string ToClrOperatorName(string rubyName)
		{
			switch (rubyName)
			{
			case "+":
				return "op_Addition";
			case "-":
				return "op_Subtraction";
			case "/":
				return "op_Division";
			case "*":
				return "op_Multiply";
			case "%":
				return "op_Modulus";
			case "==":
				return "op_Equality";
			case "!=":
				return "op_Inequality";
			case ">":
				return "op_GreaterThan";
			case ">=":
				return "op_GreaterThanOrEqual";
			case "<":
				return "op_LessThan";
			case "<=":
				return "op_LessThanOrEqual";
			case "-@":
				return "op_UnaryNegation";
			case "+@":
				return "op_UnaryPlus";
			case "<<":
				return "op_LeftShift";
			case ">>":
				return "op_RightShift";
			case "^":
				return "op_ExclusiveOr";
			case "~":
				return "op_OnesComplement";
			case "&":
				return "op_BitwiseAnd";
			case "|":
				return "op_BitwiseOr";
			case "**":
				return "Power";
			case "<=>":
				return "Compare";
			default:
				return null;
			}
		}

		internal static string ToRubyOperatorName(string clrName)
		{
			switch (clrName)
			{
			case "op_Addition":
				return "+";
			case "op_Subtraction":
				return "-";
			case "op_Division":
				return "/";
			case "op_Multiply":
				return "*";
			case "op_Modulus":
				return "%";
			case "op_Equality":
				return "==";
			case "op_Inequality":
				return "!=";
			case "op_GreaterThan":
				return ">";
			case "op_GreaterThanOrEqual":
				return ">=";
			case "op_LessThan":
				return "<";
			case "op_LessThanOrEqual":
				return "<=";
			case "op_UnaryNegation":
				return "-@";
			case "op_UnaryPlus":
				return "+@";
			case "op_LeftShift":
				return "<<";
			case "op_RightShift":
				return ">>";
			case "op_BitwiseAnd":
				return "&";
			case "op_BitwiseOr":
				return "|";
			case "op_ExclusiveOr":
				return "^";
			case "op_OnesComplement":
				return "~";
			case "Power":
				return "**";
			case "Compare":
				return "<=>";
			default:
				return null;
			}
		}

		internal static string MapOperator(OverloadInfo method)
		{
			if (!method.IsStatic || !method.IsSpecialName)
			{
				return null;
			}
			return ToRubyOperatorName(method.Name);
		}

		internal static string MapOperator(MethodInfo method)
		{
			if (!method.IsStatic || !method.IsSpecialName)
			{
				return null;
			}
			return ToRubyOperatorName(method.Name);
		}

		internal static bool IsOperator(OverloadInfo method)
		{
			return MapOperator(method) != null;
		}

		internal static string MapOperator(ExpressionType op)
		{
			string methodName;
			TryMapOperator(op, out methodName);
			return methodName;
		}

		internal static int TryMapOperator(ExpressionType op, out string methodName)
		{
			switch (op)
			{
			case ExpressionType.Add:
				methodName = "+";
				return 2;
			case ExpressionType.Subtract:
				methodName = "-";
				return 2;
			case ExpressionType.Divide:
				methodName = "/";
				return 2;
			case ExpressionType.Multiply:
				methodName = "*";
				return 2;
			case ExpressionType.Modulo:
				methodName = "%";
				return 2;
			case ExpressionType.Equal:
				methodName = "==";
				return 2;
			case ExpressionType.NotEqual:
				methodName = "!=";
				return 2;
			case ExpressionType.GreaterThan:
				methodName = ">";
				return 2;
			case ExpressionType.GreaterThanOrEqual:
				methodName = ">=";
				return 2;
			case ExpressionType.LessThan:
				methodName = "<";
				return 2;
			case ExpressionType.LessThanOrEqual:
				methodName = "<=";
				return 2;
			case ExpressionType.LeftShift:
				methodName = "<<";
				return 2;
			case ExpressionType.RightShift:
				methodName = ">>";
				return 2;
			case ExpressionType.And:
				methodName = "&";
				return 2;
			case ExpressionType.Or:
				methodName = "|";
				return 2;
			case ExpressionType.ExclusiveOr:
				methodName = "^";
				return 2;
			case ExpressionType.Power:
				methodName = "**";
				return 2;
			case ExpressionType.Negate:
				methodName = "-@";
				return 1;
			case ExpressionType.UnaryPlus:
				methodName = "+@";
				return 1;
			case ExpressionType.OnesComplement:
				methodName = "~";
				return 1;
			case ExpressionType.Not:
				methodName = "!";
				return 1;
			default:
				methodName = null;
				return 0;
			}
		}

		internal static int TryMapOperator(string methodName, out ExpressionType op)
		{
			switch (methodName)
			{
			case "+":
				op = ExpressionType.Add;
				return 2;
			case "-":
				op = ExpressionType.Subtract;
				return 2;
			case "/":
				op = ExpressionType.Divide;
				return 2;
			case "*":
				op = ExpressionType.Multiply;
				return 2;
			case "%":
				op = ExpressionType.Modulo;
				return 2;
			case "==":
				op = ExpressionType.Equal;
				return 2;
			case "!=":
				op = ExpressionType.NotEqual;
				return 2;
			case ">":
				op = ExpressionType.GreaterThan;
				return 2;
			case ">=":
				op = ExpressionType.GreaterThanOrEqual;
				return 2;
			case "<":
				op = ExpressionType.LessThan;
				return 2;
			case "<=":
				op = ExpressionType.LessThanOrEqual;
				return 2;
			case "<<":
				op = ExpressionType.LeftShift;
				return 2;
			case ">>":
				op = ExpressionType.RightShift;
				return 2;
			case "&":
				op = ExpressionType.And;
				return 2;
			case "|":
				op = ExpressionType.Or;
				return 2;
			case "^":
				op = ExpressionType.ExclusiveOr;
				return 2;
			case "**":
				op = ExpressionType.Power;
				return 2;
			case "-@":
				op = ExpressionType.Negate;
				return 1;
			case "+@":
				op = ExpressionType.UnaryPlus;
				return 1;
			case "~":
				op = ExpressionType.OnesComplement;
				return 1;
			case "!":
				op = ExpressionType.Not;
				return 1;
			default:
				op = ExpressionType.Add;
				return 0;
			}
		}

		internal static RubyModule GetModuleFromObject(RubyScope scope, object obj)
		{
			RubyModule rubyModule = obj as RubyModule;
			if (rubyModule == null)
			{
				throw CreateNotModuleException(scope, obj);
			}
			return rubyModule;
		}

		internal static Exception CreateNotModuleException(RubyScope scope, object obj)
		{
			return RubyExceptions.CreateTypeError(string.Format("{0} is not a class/module", scope.RubyContext.GetClassOf(obj)));
		}

		public static void RequireMixins(RubyModule target, params RubyModule[] modules)
		{
			foreach (RubyModule rubyModule in modules)
			{
				if (rubyModule == null)
				{
					throw RubyExceptions.CreateTypeError("wrong argument type nil (expected Module)");
				}
				if (rubyModule == target)
				{
					throw RubyExceptions.CreateArgumentError("cyclic include detected");
				}
				if (rubyModule.IsClass)
				{
					throw RubyExceptions.CreateTypeError("wrong argument type Class (expected Module)");
				}
				if (rubyModule.Context != target.Context)
				{
					throw RubyExceptions.CreateTypeError(string.Format("cannot mix a foreign module `{0}' into `{1}' (runtime mismatch)", rubyModule.GetName(target.Context), target.GetName(rubyModule.Context)));
				}
			}
		}

		internal static RubyArray AsArrayOfStrings(object value)
		{
			RubyArray rubyArray = value as RubyArray;
			if (rubyArray != null)
			{
				foreach (object item in rubyArray)
				{
					MutableString mutableString = item as MutableString;
					if (mutableString == null)
					{
						return null;
					}
				}
				return rubyArray;
			}
			return null;
		}

		public static object SetHashElement(RubyContext context, IDictionary<object, object> obj, object key, object value)
		{
			MutableString mutableString = key as MutableString;
			key = ((mutableString == null) ? CustomStringDictionary.NullToObj(key) : mutableString.Duplicate(context, false, mutableString.Clone()).Freeze());
			return obj[key] = value;
		}

		public static Hash SetHashElements(RubyContext context, Hash hash, object[] items)
		{
			for (int i = 0; i < items.Length; i += 2)
			{
				SetHashElement(context, hash, items[i], items[i + 1]);
			}
			return hash;
		}

		public static RubyCompilerOptions CreateCompilerOptionsForEval(RubyScope targetScope, int line)
		{
			return CreateCompilerOptionsForEval(targetScope, targetScope.GetInnerMostMethodScope(), false, line);
		}

		private static RubyCompilerOptions CreateCompilerOptionsForEval(RubyScope targetScope, RubyMethodScope methodScope, bool isModuleEval, int line)
		{
			RubyCompilerOptions rubyCompilerOptions = new RubyCompilerOptions(targetScope.RubyContext.RubyOptions);
			rubyCompilerOptions.IsEval = true;
			rubyCompilerOptions.FactoryKind = (isModuleEval ? TopScopeFactoryKind.ModuleEval : TopScopeFactoryKind.None);
			rubyCompilerOptions.LocalNames = targetScope.GetVisibleLocalNames();
			rubyCompilerOptions.TopLevelMethodName = ((methodScope != null) ? methodScope.DefinitionName : null);
			rubyCompilerOptions.TopLevelParameterNames = ((methodScope != null) ? methodScope.GetVisibleParameterNames() : null);
			rubyCompilerOptions.TopLevelHasUnsplatParameter = methodScope != null && methodScope.HasUnsplatParameter;
			rubyCompilerOptions.InitialLocation = new SourceLocation(0, (line <= 0) ? 1 : line, 1);
			return rubyCompilerOptions;
		}

		private static SourceUnit CreateRubySourceUnit(RubyContext context, MutableString code, string path)
		{
			return context.CreateSourceUnit(new BinaryContentProvider(code.ToByteArray()), path, code.Encoding.Encoding, SourceCodeKind.File);
		}

		public static object Evaluate(MutableString code, RubyScope targetScope, object self, RubyModule module, MutableString file, int line)
		{
			RubyContext rubyContext = targetScope.RubyContext;
			RubyMethodScope innerMostMethodScope = targetScope.GetInnerMostMethodScope();
			RubyCompilerOptions options = CreateCompilerOptionsForEval(targetScope, innerMostMethodScope, module != null, line);
			SourceUnit sourceUnit = CreateRubySourceUnit(rubyContext, code, (file != null) ? file.ConvertToString() : "(eval)");
			Expression<Func<RubyScope, object, RubyModule, Proc, object>> lambda;
			try
			{
				lambda = rubyContext.ParseSourceCode<Func<RubyScope, object, RubyModule, Proc, object>>(sourceUnit, options, rubyContext.RuntimeErrorSink);
			}
			catch (SyntaxError)
			{
				throw;
			}
			if (module != null)
			{
				targetScope = CreateModuleEvalScope(targetScope, self, module);
			}
			return ((Func<RubyScope, object, RubyModule, Proc, object>)RubyScriptCode.CompileLambda(lambda, rubyContext))(targetScope, self, module, (innerMostMethodScope != null) ? innerMostMethodScope.BlockParameter : null);
		}

		private static RubyScope CreateModuleEvalScope(RubyScope parent, object self, RubyModule module)
		{
			return new RubyModuleEvalScope(parent, module, self);
		}

		public static object EvaluateInModule(RubyModule self, BlockParam block, object[] args)
		{
			object result;
			EvaluateBlock(block, self, self, args, out result);
			return result;
		}

		public static object EvaluateInModule(RubyModule self, BlockParam block, object[] args, object defaultReturnValue)
		{
			object result;
			if (!EvaluateBlock(block, self, self, args, out result))
			{
				return defaultReturnValue;
			}
			return result;
		}

		public static object EvaluateInSingleton(object self, BlockParam block, object[] args)
		{
			if (!CanDefineSingletonMethod(self))
			{
				throw RubyExceptions.CreateTypeError("can't define singleton method for literals");
			}
			object result;
			EvaluateBlock(block, block.RubyContext.GetOrCreateSingletonClass(self), self, args, out result);
			return result;
		}

		private static bool EvaluateBlock(BlockParam block, RubyModule module, object self, object[] args, out object result)
		{
			block.MethodLookupModule = module;
			if (args != null)
			{
				result = RubyOps.Yield(args, null, self, block);
			}
			else
			{
				result = RubyOps.Yield0(null, self, block);
			}
			return block.BlockJumped(result);
		}

		public static object CreateObject(RubyClass theclass, IEnumerable<KeyValuePair<string, object>> attributes)
		{
			Type underlyingSystemType = theclass.GetUnderlyingSystemType();
			if (typeof(ISerializable).IsAssignableFrom(underlyingSystemType))
			{
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				ConstructorInfo constructor = underlyingSystemType.GetConstructor(bindingAttr, null, _serializableTypeSignature, null);
				if (constructor == null)
				{
					string message = string.Format("Class {0} does not have a valid deserializing constructor", underlyingSystemType.FullName);
					throw new NotSupportedException(message);
				}
				SerializationInfo serializationInfo = new SerializationInfo(underlyingSystemType, new FormatterConverter());
				serializationInfo.AddValue(SerializationInfoClassKey, theclass);
				foreach (KeyValuePair<string, object> attribute in attributes)
				{
					serializationInfo.AddValue(attribute.Key, attribute.Value);
				}
				return constructor.Invoke(new object[2]
				{
					serializationInfo,
					new StreamingContext(StreamingContextStates.Other, theclass)
				});
			}
			object obj = CreateObject(theclass);
			foreach (KeyValuePair<string, object> attribute2 in attributes)
			{
				theclass.Context.SetInstanceVariable(obj, attribute2.Key, attribute2.Value);
			}
			return obj;
		}

		private static bool IsAvailable(MethodBase method)
		{
			if (method != null && !method.IsPrivate && !method.IsAssembly)
			{
				return !method.IsFamilyAndAssembly;
			}
			return false;
		}

		public static object CreateObject(RubyClass theClass)
		{
			Type underlyingSystemType = theClass.GetUnderlyingSystemType();
			if (underlyingSystemType == typeof(RubyStruct))
			{
				return RubyStruct.Create(theClass);
			}
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			ConstructorInfo constructor;
			if (IsAvailable(constructor = underlyingSystemType.GetConstructor(bindingAttr, null, Type.EmptyTypes, null)))
			{
				return constructor.Invoke(new object[0]);
			}
			if (IsAvailable(constructor = underlyingSystemType.GetConstructor(bindingAttr, null, _ccTypes1, null)))
			{
				return constructor.Invoke(new object[1] { theClass });
			}
			if (IsAvailable(constructor = underlyingSystemType.GetConstructor(bindingAttr, null, _ccTypes2, null)))
			{
				return constructor.Invoke(new object[1] { theClass.Context });
			}
			string message = string.Format("Class {0} does not have a valid constructor", theClass.Name);
			throw new NotSupportedException(message);
		}

		public static CallSite<TCallSiteFunc> GetCallSite<TCallSiteFunc>(ref CallSite<TCallSiteFunc> site, RubyContext context, string methodName, int argumentCount) where TCallSiteFunc : class
		{
			if (site == null)
			{
				Interlocked.CompareExchange(ref site, CallSite<TCallSiteFunc>.Create(RubyCallAction.Make(context, methodName, RubyCallSignature.WithImplicitSelf(argumentCount))), null);
			}
			return site;
		}

		public static CallSite<TCallSiteFunc> GetCallSite<TCallSiteFunc>(ref CallSite<TCallSiteFunc> site, RubyContext context, string methodName, RubyCallSignature signature) where TCallSiteFunc : class
		{
			if (site == null)
			{
				Interlocked.CompareExchange(ref site, CallSite<TCallSiteFunc>.Create(RubyCallAction.Make(context, methodName, signature)), null);
			}
			return site;
		}

		public static CallSite<Func<CallSite, object, TResult>> GetCallSite<TResult>(ref CallSite<Func<CallSite, object, TResult>> site, RubyConversionAction conversion)
		{
			if (site == null)
			{
				Interlocked.CompareExchange(ref site, CallSite<Func<CallSite, object, TResult>>.Create(conversion), null);
			}
			return site;
		}

		public static void RaiseAsyncException(Thread thread, Exception e)
		{
			thread.Abort(new AsyncExceptionMarker(e));
		}

		public static void ExitThread(Thread thread)
		{
			thread.Abort(new ThreadExitMarker());
		}

		public static bool IsRubyThreadExit(Exception e)
		{
			ThreadAbortException ex = e as ThreadAbortException;
			if (ex != null && ex.ExceptionState is ThreadExitMarker)
			{
				return true;
			}
			return false;
		}

		public static Exception GetVisibleException(Exception e)
		{
			ThreadAbortException ex = e as ThreadAbortException;
			if (ex != null)
			{
				if (IsRubyThreadExit(e))
				{
					return null;
				}
				AsyncExceptionMarker asyncExceptionMarker = ex.ExceptionState as AsyncExceptionMarker;
				if (asyncExceptionMarker != null)
				{
					return asyncExceptionMarker.Exception;
				}
			}
			return e;
		}

		public static MutableString CanonicalizePath(MutableString path)
		{
			for (int i = 0; i < path.Length; i++)
			{
				if (path.GetChar(i) == '\\')
				{
					path.SetChar(i, '/');
				}
			}
			return path;
		}

		public static string CanonicalizePath(string path)
		{
			return path.Replace('\\', '/');
		}

		public static string CombinePaths(string basePath, string path)
		{
			if (basePath.Length != 0 && !basePath.EndsWith("\\", StringComparison.Ordinal) && !basePath.EndsWith("/", StringComparison.Ordinal))
			{
				return basePath + "/" + path;
			}
			return basePath + path;
		}

		public static bool IsAbsolutePath(string path)
		{
			if (IsAbsoluteDriveLetterPath(path))
			{
				return true;
			}
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			return path[0] == '/';
		}

		public static bool IsAbsoluteDriveLetterPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			if (!FileSystemUsesDriveLetters)
			{
				return false;
			}
			if (Tokenizer.IsLetter(path[0]) && path.Length >= 2 && path[1] == ':')
			{
				return path[2] == '/';
			}
			return false;
		}

		public static string GetPathRoot(PlatformAdaptationLayer platform, string path, out string pathAfterRoot)
		{
			if (IsAbsoluteDriveLetterPath(path))
			{
				pathAfterRoot = path.Substring(3);
				return path.Substring(0, 3);
			}
			string text = path.TrimStart('/');
			int num = path.Length - text.Length;
			string result = path.Substring(0, num);
			pathAfterRoot = path.Substring(num);
			if (!FileSystemUsesDriveLetters || num > 1)
			{
				return result;
			}
			string path2 = CanonicalizePath(platform.CurrentDirectory);
			string pathAfterRoot2;
			return GetPathRoot(platform, path2, out pathAfterRoot2);
		}

		public static bool HasPartialDriveLetter(string path, out char partialDriveLetter, out string relativePath)
		{
			partialDriveLetter = '\0';
			relativePath = null;
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			if (!FileSystemUsesDriveLetters)
			{
				return false;
			}
			if (Tokenizer.IsLetter(path[0]) && path.Length >= 2 && path[1] == ':' && (path.Length == 2 || path[2] != '/'))
			{
				partialDriveLetter = path[0];
				relativePath = path.Substring(2);
				return true;
			}
			return false;
		}

		public static string GetHomeDirectory(PlatformAdaptationLayer pal)
		{
			string text = pal.GetEnvironmentVariable("HOME");
			if (text == null)
			{
				string environmentVariable = pal.GetEnvironmentVariable("HOMEDRIVE");
				string environmentVariable2 = pal.GetEnvironmentVariable("HOMEPATH");
				if (environmentVariable != null || environmentVariable2 != null)
				{
					text = ((environmentVariable == null) ? environmentVariable2 : ((environmentVariable2 != null) ? (environmentVariable + environmentVariable2) : (environmentVariable + Path.DirectorySeparatorChar)));
				}
				else
				{
					string environmentVariable3 = pal.GetEnvironmentVariable("USERPROFILE");
					text = ((environmentVariable3 != null) ? environmentVariable3 : Environment.GetFolderPath(Environment.SpecialFolder.Personal));
				}
				if (text != null)
				{
					text = ExpandPath(pal, text);
				}
			}
			return text;
		}

		public static string ExpandPath(PlatformAdaptationLayer platform, string path)
		{
			return ExpandPath(platform, path, platform.CurrentDirectory, true);
		}

		public static string ExpandPath(PlatformAdaptationLayer platform, string path, string basePath, bool expandHome)
		{
			return ExpandPath(platform, path, basePath, expandHome, true);
		}

		private static string ExpandPath(PlatformAdaptationLayer platform, string path, string basePath, bool expandHome, bool expandBase)
		{
			if (expandHome)
			{
				int length = path.Length;
				if (length > 0 && path[0] == '~')
				{
					if (length != 1 && path[1] != '/' && path[1] != '\\')
					{
						return path;
					}
					string environmentVariable = platform.GetEnvironmentVariable("HOME");
					if (environmentVariable == null)
					{
						throw RubyExceptions.CreateArgumentError("couldn't find HOME environment -- expanding `~'");
					}
					path = ((length > 2) ? Path.Combine(environmentVariable, path.Substring(2)) : environmentVariable);
				}
			}
			path = CanonicalizePath(path);
			basePath = CanonicalizePath(basePath);
			if (IsAbsolutePath(path))
			{
				return new PathExpander(platform, path).GetResult();
			}
			string text = (expandBase ? ExpandPath(platform, basePath, platform.CurrentDirectory, expandHome, false) : basePath);
			char partialDriveLetter;
			string relativePath;
			if (HasPartialDriveLetter(path, out partialDriveLetter, out relativePath))
			{
				string pathAfterRoot;
				string pathRoot = GetPathRoot(platform, text, out pathAfterRoot);
				if (pathRoot[0].ToLowerInvariant() != partialDriveLetter.ToLowerInvariant())
				{
					text = CanonicalizePath(platform.CurrentDirectory);
					pathRoot = GetPathRoot(platform, text, out pathAfterRoot);
					if (pathRoot[0].ToLowerInvariant() != partialDriveLetter.ToLowerInvariant())
					{
						return new PathExpander(platform, partialDriveLetter + ":/" + relativePath).GetResult();
					}
				}
				path = relativePath;
			}
			PathExpander pathExpander = new PathExpander(platform, text);
			pathExpander.AddRelativePath(path);
			return pathExpander.GetResult();
		}

		public static string GetExtension(string path)
		{
			if (path == null)
			{
				return null;
			}
			int length = path.Length;
			int num = length;
			while (--num >= 0)
			{
				char c = path[num];
				if (c == '.')
				{
					if (num != length - 1 && num > 0)
					{
						return path.Substring(num, length - num);
					}
					return string.Empty;
				}
				if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar)
				{
					break;
				}
			}
			return string.Empty;
		}

		public static void Write(this Stream stream, MutableString str, int start, int count)
		{
			int count2;
			byte[] byteArray = str.GetByteArray(out count2);
			if (start < count2)
			{
				stream.Write(byteArray, start, Math.Min(count2 - start, count));
			}
		}
	}
}
