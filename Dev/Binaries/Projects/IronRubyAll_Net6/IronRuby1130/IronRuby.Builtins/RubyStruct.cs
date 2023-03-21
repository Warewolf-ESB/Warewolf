using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public sealed class RubyStruct : RubyObject
	{
		internal sealed class Info
		{
			private readonly Dictionary<string, int> _nameIndices;

			private readonly string[] _names;

			internal int Length
			{
				get
				{
					return _names.Length;
				}
			}

			internal Info(string[] names)
			{
				_names = ArrayUtils.Copy(names);
				_nameIndices = new Dictionary<string, int>(names.Length);
				for (int i = 0; i < names.Length; i++)
				{
					string text = names[i];
					if (!Tokenizer.IsVariableName(text))
					{
						throw RubyExceptions.CreateNameError(string.Format("invalid attribute name `{0}'", text));
					}
					_nameIndices[names[i]] = i;
				}
			}

			internal bool TryGetIndex(string name, out int index)
			{
				return _nameIndices.TryGetValue(name, out index);
			}

			internal string GetName(int index)
			{
				return _names[index];
			}

			internal RubyArray GetMembers(RubyContext context)
			{
				RubyArray rubyArray = new RubyArray(_names.Length);
				string[] names = _names;
				foreach (string identifier in names)
				{
					rubyArray.Add(context.StringifyIdentifier(identifier));
				}
				return rubyArray;
			}

			internal ReadOnlyCollection<string> GetNames()
			{
				return new ReadOnlyCollection<string>(_names);
			}
		}

		private readonly object[] _data;

		private Info StructInfo
		{
			get
			{
				return base.ImmediateClass.GetNonSingletonClass().StructInfo;
			}
		}

		public object[] Values
		{
			get
			{
				return _data;
			}
		}

		public object this[string name]
		{
			get
			{
				return _data[GetIndex(name)];
			}
			set
			{
				_data[GetIndex(name)] = value;
			}
		}

		public object this[int index]
		{
			get
			{
				return _data[index];
			}
			set
			{
				_data[index] = value;
			}
		}

		public int ItemCount
		{
			get
			{
				return _data.Length;
			}
		}

		public RubyStruct(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		private RubyStruct(RubyClass rubyClass, bool dummy)
			: base(rubyClass)
		{
			_data = new object[rubyClass.StructInfo.Length];
		}

		private RubyStruct(RubyClass rubyClass, object[] data)
			: base(rubyClass)
		{
			_data = ArrayUtils.Copy(data);
		}

		protected override RubyObject CreateInstance()
		{
			return new RubyStruct(base.ImmediateClass.NominalClass, _data);
		}

		public static RubyStruct Create(RubyClass rubyClass)
		{
			return new RubyStruct(rubyClass, true);
		}

		public static RubyClass DefineStruct(RubyClass owner, string className, string[] attributeNames)
		{
			RubyClass rubyClass = owner.Context.DefineClass(owner, className, owner, new Info(attributeNames));
			AddClassMembers(rubyClass, attributeNames);
			return rubyClass;
		}

		private static void AddClassMembers(RubyClass cls, string[] structMembers)
		{
			RuleGenerator ruleGenerator = RuleGenerators.InstanceConstructor;
			RubyContext context = cls.Context;
			RubyClass orCreateSingletonClass = cls.GetOrCreateSingletonClass();
			orCreateSingletonClass.AddMethod(context, "[]", new RubyCustomMethodInfo(ruleGenerator, RubyMemberFlags.Public, orCreateSingletonClass));
			orCreateSingletonClass.AddMethod(context, "new", new RubyCustomMethodInfo(ruleGenerator, RubyMemberFlags.Public, orCreateSingletonClass));
			orCreateSingletonClass.AddMethod(context, "members", new RubyLibraryMethodInfo(new LibraryOverload[1] { LibraryOverload.Create(new Func<RubyClass, RubyArray>(GetMembers), false, 0, 0) }, RubyMemberFlags.Public, orCreateSingletonClass));
			for (int i = 0; i < structMembers.Length; i++)
			{
				string text = structMembers[i];
				cls.AddMethod(context, text, new RubyCustomMethodInfo(CreateGetter(i), RubyMemberFlags.Public, cls));
				cls.AddMethod(context, text + '=', new RubyCustomMethodInfo(CreateSetter(i), RubyMemberFlags.Public, cls));
			}
		}

		public static RubyArray GetMembers(RubyClass self)
		{
			return self.StructInfo.GetMembers(self.Context);
		}

		public static RubyArray GetMembers(RubyStruct self)
		{
			return self.StructInfo.GetMembers(self.ImmediateClass.Context);
		}

		private static RuleGenerator CreateGetter(int index)
		{
			return delegate (MetaObjectBuilder metaBuilder, CallArguments args, string name)
			{
				RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
				if (!metaBuilder.Error)
				{
					metaBuilder.Result = Methods.RubyStruct_GetValue.OpCall(Expression.Convert(args.TargetExpression, typeof(RubyStruct)), Microsoft.Scripting.Ast.Utils.Constant(index));
				}
			};
		}

		private static RuleGenerator CreateSetter(int index)
		{
			return delegate (MetaObjectBuilder metaBuilder, CallArguments args, string name)
			{
				IList<DynamicMetaObject> list = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
				if (!metaBuilder.Error)
				{
					metaBuilder.Result = Methods.RubyStruct_SetValue.OpCall(Expression.Convert(args.TargetExpression, typeof(RubyStruct)), Microsoft.Scripting.Ast.Utils.Constant(index), Microsoft.Scripting.Ast.Utils.Box(list[0].Expression));
				}
			};
		}

		public bool TryGetIndex(string name, out int index)
		{
			return StructInfo.TryGetIndex(name, out index);
		}

		public int GetIndex(string name)
		{
			int index;
			if (TryGetIndex(name, out index))
			{
				return index;
			}
			throw RubyExceptions.CreateNameError(string.Format("no member `{0}' in struct", name));
		}

		public int GetHashCode(UnaryOpStorage hashStorage, ConversionStorage<int> fixnumCast)
		{
			return StructInfo.GetHashCode() ^ RubyArray.GetHashCode(hashStorage, fixnumCast, _data);
		}

		public bool Equals(BinaryOpStorage eqlStorage, object obj)
		{
			RubyStruct rubyStruct = obj as RubyStruct;
			if (!StructReferenceEquals(rubyStruct))
			{
				return false;
			}
			return RubyArray.Equals(eqlStorage, _data, rubyStruct._data);
		}

		public bool StructReferenceEquals(RubyStruct other)
		{
			if (!object.ReferenceEquals(this, other))
			{
				if (other != null)
				{
					return base.ImmediateClass.GetNonSingletonClass() == other.ImmediateClass.GetNonSingletonClass();
				}
				return false;
			}
			return true;
		}

		public static object GetValue(RubyStruct self, int index)
		{
			return self._data[index];
		}

		public static object SetValue(RubyStruct self, int index, object value)
		{
			return self._data[index] = value;
		}

		public IEnumerable<KeyValuePair<string, object>> GetItems()
		{
			for (int i = 0; i < _data.Length; i++)
			{
				yield return new KeyValuePair<string, object>(StructInfo.GetName(i), _data[i]);
			}
		}

		public ReadOnlyCollection<string> GetNames()
		{
			return StructInfo.GetNames();
		}

		public void SetValues(object[] items)
		{
			ContractUtils.RequiresNotNull(items, "items");
			if (items.Length > _data.Length)
			{
				throw RubyExceptions.CreateArgumentError("struct size differs");
			}
			Array.Copy(items, _data, items.Length);
		}
	}
}
