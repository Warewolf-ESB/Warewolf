using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Module", Extends = typeof(RubyModule), Inherits = typeof(object), Restrictions = (ModuleRestrictions.Builtin | ModuleRestrictions.NoUnderlyingType))]
	public static class ModuleOps
	{
		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static object Reinitialize(BlockParam block, RubyModule self)
		{
			if (self.IsClass)
			{
				throw RubyExceptions.CreateTypeError("already initialized class");
			}
			if (block == null)
			{
				return null;
			}
			return RubyUtils.EvaluateInModule(self, block, null);
		}

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule InitializeCopy(RubyModule self, object other)
		{
			if (self.IsClass)
			{
				throw RubyExceptions.CreateTypeError("already initialized class");
			}
			RubyClass classOf = self.Context.GetClassOf(self);
			RubyClass classOf2 = self.Context.GetClassOf(other);
			if (classOf2 != classOf)
			{
				throw RubyExceptions.CreateTypeError("initialize_copy should take same class object");
			}
			self.InitializeModuleCopy((RubyModule)other);
			return self;
		}

		[RubyMethod("extend_object", RubyMethodAttributes.PrivateInstance)]
		public static object ExtendObject(RubyModule self, object extendedObject)
		{
			self.Context.GetOrCreateSingletonClass(extendedObject).IncludeModules(self);
			return extendedObject;
		}

		[RubyMethod("extended", RubyMethodAttributes.PrivateInstance)]
		public static void ObjectExtended(RubyModule self, object extendedObject)
		{
		}

		[RubyMethod("include", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule Include(CallSiteStorage<Func<CallSite, RubyModule, RubyModule, object>> appendFeaturesStorage, CallSiteStorage<Func<CallSite, RubyModule, RubyModule, object>> includedStorage, RubyModule self, [NotNullItems] params RubyModule[] modules)
		{
			RubyUtils.RequireMixins(self, modules);
			CallSite<Func<CallSite, RubyModule, RubyModule, object>> callSite = appendFeaturesStorage.GetCallSite("append_features", 1);
			CallSite<Func<CallSite, RubyModule, RubyModule, object>> callSite2 = includedStorage.GetCallSite("included", 1);
			for (int num = modules.Length - 1; num >= 0; num--)
			{
				callSite.Target(callSite, modules[num], self);
				callSite2.Target(callSite2, modules[num], self);
			}
			return self;
		}

		[RubyMethod("included", RubyMethodAttributes.PrivateInstance)]
		public static void Included(RubyModule self, RubyModule owner)
		{
		}

		[RubyMethod("append_features", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule AppendFeatures(RubyModule self, [NotNull] RubyModule owner)
		{
			owner.IncludeModules(self);
			return self;
		}

		[RubyMethod("private", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule SetPrivateVisibility(RubyScope scope, RubyModule self, [DefaultProtocol][NotNullItems] params string[] methodNames)
		{
			SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.PrivateInstance);
			return self;
		}

		[RubyMethod("protected", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule SetProtectedVisibility(RubyScope scope, RubyModule self, [NotNullItems][DefaultProtocol] params string[] methodNames)
		{
			SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.ProtectedInstance);
			return self;
		}

		[RubyMethod("public", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule SetPublicVisibility(RubyScope scope, RubyModule self, [NotNullItems][DefaultProtocol] params string[] methodNames)
		{
			SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.PublicInstance);
			return self;
		}

		[RubyMethod("private_class_method")]
		public static RubyModule MakeClassMethodsPrivate(RubyModule self, [NotNullItems][DefaultProtocol] params string[] methodNames)
		{
			SetMethodAttributes(self.GetOrCreateSingletonClass(), methodNames, RubyMethodAttributes.Private);
			return self;
		}

		[RubyMethod("public_class_method")]
		public static RubyModule MakeClassMethodsPublic(RubyModule self, [DefaultProtocol][NotNullItems] params string[] methodNames)
		{
			SetMethodAttributes(self.GetOrCreateSingletonClass(), methodNames, RubyMethodAttributes.Public);
			return self;
		}

		[RubyMethod("module_function", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule CopyMethodsToModuleSingleton(RubyScope scope, RubyModule self, [NotNullItems][DefaultProtocol] params string[] methodNames)
		{
			if (self.IsClass)
			{
				throw RubyExceptions.CreateTypeError("module_function must be called for modules");
			}
			SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.ModuleFunction);
			return self;
		}

		internal static void SetMethodAttributes(RubyScope scope, RubyModule module, string[] methodNames, RubyMethodAttributes attributes)
		{
			if (methodNames.Length == 0)
			{
				scope.GetMethodAttributesDefinitionScope().MethodAttributes = attributes;
			}
			else
			{
				SetMethodAttributes(module, methodNames, attributes);
			}
		}

		internal static void SetMethodAttributes(RubyModule module, string[] methodNames, RubyMethodAttributes attributes)
		{
			RubyContext context = module.Context;
			bool flag = (attributes & RubyMethodAttributes.ModuleFunction) == RubyMethodAttributes.ModuleFunction;
			RubyMethodVisibility rubyMethodVisibility = (RubyMethodVisibility)(flag ? RubyMethodAttributes.Private : (attributes & RubyMethodAttributes.VisibilityMask));
			foreach (string text in methodNames)
			{
				RubyMemberInfo info;
				using (context.ClassHierarchyLocker())
				{
					MethodLookup methodLookup = MethodLookup.FallbackToObject;
					if (!flag)
					{
						methodLookup |= MethodLookup.ReturnForwarder;
					}
					info = module.ResolveMethodNoLock(text, VisibilityContext.AllVisible, methodLookup).Info;
					if (info == null)
					{
						throw RubyExceptions.CreateUndefinedMethodError(module, text);
					}
					if (info.Visibility != rubyMethodVisibility)
					{
						module.SetVisibilityNoEventNoLock(context, text, info, rubyMethodVisibility);
					}
					if (flag)
					{
						module.SetModuleFunctionNoEventNoLock(context, text, info);
					}
				}
				if (info.Visibility != rubyMethodVisibility)
				{
					module.MethodAdded(text);
				}
				if (flag)
				{
					module.GetOrCreateSingletonClass().MethodAdded(text);
				}
			}
		}

		[RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
		public static RubyMethod DefineMethod(RubyScope scope, RubyModule self, [DefaultProtocol][NotNull] string methodName, [NotNull] RubyMethod method)
		{
			DefineMethod(scope, self, methodName, method.Info, method.GetTargetClass());
			return method;
		}

		[RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
		public static RubyMethod DefineMethod(RubyScope scope, RubyModule self, [NotNull] ClrName methodName, [NotNull] RubyMethod method)
		{
			RubyMethod result = DefineMethod(scope, self, methodName.MangledName, method);
			if (methodName.HasMangledName)
			{
				self.AddMethodAlias(methodName.ActualName, methodName.MangledName);
			}
			return result;
		}

		[RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
		public static UnboundMethod DefineMethod(RubyScope scope, RubyModule self, [DefaultProtocol][NotNull] string methodName, [NotNull] UnboundMethod method)
		{
			DefineMethod(scope, self, methodName, method.Info, method.TargetConstraint);
			return method;
		}

		[RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
		public static UnboundMethod DefineMethod(RubyScope scope, RubyModule self, [NotNull] ClrName methodName, [NotNull] UnboundMethod method)
		{
			UnboundMethod result = DefineMethod(scope, self, methodName.MangledName, method);
			if (methodName.HasMangledName)
			{
				self.AddMethodAlias(methodName.ActualName, methodName.MangledName);
			}
			return result;
		}

		private static void DefineMethod(RubyScope scope, RubyModule self, string methodName, RubyMemberInfo info, RubyModule targetConstraint)
		{
			RubyMethodVisibility definedMethodVisibility = GetDefinedMethodVisibility(scope, self, methodName);
			using (self.Context.ClassHierarchyLocker())
			{
				if (!self.HasAncestorNoLock(targetConstraint))
				{
					throw RubyExceptions.CreateTypeError("bind argument must be a subclass of {0}", targetConstraint.GetName(scope.RubyContext));
				}
				self.SetDefinedMethodNoEventNoLock(self.Context, methodName, info, definedMethodVisibility);
			}
			self.MethodAdded(methodName);
		}

		[RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
		public static Proc DefineMethod(RubyScope scope, [NotNull] BlockParam block, RubyModule self, [DefaultProtocol][NotNull] string methodName)
		{
			return DefineMethod(scope, self, methodName, block.Proc);
		}

		[RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
		public static Proc DefineMethod(RubyScope scope, [NotNull] BlockParam block, RubyModule self, [NotNull] ClrName methodName)
		{
			Proc result = DefineMethod(scope, block, self, methodName.MangledName);
			if (methodName.HasMangledName)
			{
				self.AddMethodAlias(methodName.ActualName, methodName.MangledName);
			}
			return result;
		}

		[RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
		public static Proc DefineMethod(RubyScope scope, RubyModule self, [NotNull][DefaultProtocol] string methodName, [NotNull] Proc block)
		{
			RubyMethodVisibility definedMethodVisibility = GetDefinedMethodVisibility(scope, self, methodName);
			RubyLambdaMethodInfo rubyLambdaMethodInfo = Proc.ToLambdaMethodInfo(block, methodName, definedMethodVisibility, self);
			self.AddMethod(scope.RubyContext, methodName, rubyLambdaMethodInfo);
			return rubyLambdaMethodInfo.Lambda;
		}

		[RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
		public static Proc DefineMethod(RubyScope scope, RubyModule self, [NotNull] ClrName methodName, [NotNull] Proc block)
		{
			Proc result = DefineMethod(scope, self, methodName.MangledName, block);
			if (methodName.HasMangledName)
			{
				self.AddMethodAlias(methodName.ActualName, methodName.MangledName);
			}
			return result;
		}

		private static RubyMethodVisibility GetDefinedMethodVisibility(RubyScope scope, RubyModule module, string methodName)
		{
			RubyMethodVisibility visibility;
			if (scope.RubyContext.RubyOptions.Compatibility < RubyCompatibility.Default)
			{
				RubyScope methodAttributesDefinitionScope = scope.GetMethodAttributesDefinitionScope();
				visibility = ((methodAttributesDefinitionScope.GetInnerMostModuleForMethodLookup() != module) ? RubyMethodVisibility.Public : (((methodAttributesDefinitionScope.MethodAttributes & RubyMethodAttributes.ModuleFunction) == RubyMethodAttributes.ModuleFunction) ? RubyMethodVisibility.Private : methodAttributesDefinitionScope.Visibility));
			}
			else
			{
				visibility = RubyMethodVisibility.Public;
			}
			return RubyUtils.GetSpecialMethodVisibility(visibility, methodName);
		}

		[RubyMethod("method_added", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static void MethodAdded(object self, object methodName)
		{
		}

		[RubyMethod("method_removed", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static void MethodRemoved(object self, object methodName)
		{
		}

		[RubyMethod("method_undefined", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static void MethodUndefined(object self, object methodName)
		{
		}

		private static void DefineAccessor(RubyScope scope, RubyModule self, string name, bool readable, bool writable)
		{
			if (!Tokenizer.IsVariableName(name))
			{
				throw RubyExceptions.CreateNameError("invalid attribute name `{0}'", name);
			}
			string text = "@" + name;
			RubyScope methodAttributesDefinitionScope = scope.GetMethodAttributesDefinitionScope();
			if (readable)
			{
				RubyMemberFlags specialMethodVisibility = (RubyMemberFlags)RubyUtils.GetSpecialMethodVisibility(methodAttributesDefinitionScope.Visibility, name);
				self.AddMethod(scope.RubyContext, name, new RubyAttributeReaderInfo(specialMethodVisibility, self, text));
			}
			if (writable)
			{
				self.AddMethod(scope.RubyContext, name + "=", new RubyAttributeWriterInfo((RubyMemberFlags)methodAttributesDefinitionScope.Visibility, self, text));
			}
		}

		[RubyMethod("attr", RubyMethodAttributes.PrivateInstance)]
		public static void Attr(RubyScope scope, RubyModule self, [NotNull][DefaultProtocol] string name, [Optional] bool writable)
		{
			DefineAccessor(scope, self, name, true, writable);
		}

		[RubyMethod("attr", RubyMethodAttributes.PrivateInstance)]
		public static void Attr(RubyScope scope, RubyModule self, [DefaultProtocol][NotNullItems] params string[] names)
		{
			foreach (string name in names)
			{
				DefineAccessor(scope, self, name, true, false);
			}
		}

		[RubyMethod("attr_accessor", RubyMethodAttributes.PrivateInstance)]
		public static void AttrAccessor(RubyScope scope, RubyModule self, [NotNull][DefaultProtocol] string name)
		{
			DefineAccessor(scope, self, name, true, true);
		}

		[RubyMethod("attr_accessor", RubyMethodAttributes.PrivateInstance)]
		public static void AttrAccessor(RubyScope scope, RubyModule self, [DefaultProtocol][NotNullItems] params string[] names)
		{
			foreach (string name in names)
			{
				DefineAccessor(scope, self, name, true, true);
			}
		}

		[RubyMethod("attr_reader", RubyMethodAttributes.PrivateInstance)]
		public static void AttrReader(RubyScope scope, RubyModule self, [NotNull][DefaultProtocol] string name)
		{
			DefineAccessor(scope, self, name, true, false);
		}

		[RubyMethod("attr_reader", RubyMethodAttributes.PrivateInstance)]
		public static void AttrReader(RubyScope scope, RubyModule self, [NotNullItems][DefaultProtocol] params string[] names)
		{
			foreach (string name in names)
			{
				DefineAccessor(scope, self, name, true, false);
			}
		}

		[RubyMethod("attr_writer", RubyMethodAttributes.PrivateInstance)]
		public static void AttrWriter(RubyScope scope, RubyModule self, [DefaultProtocol][NotNull] string name)
		{
			DefineAccessor(scope, self, name, false, true);
		}

		[RubyMethod("attr_writer", RubyMethodAttributes.PrivateInstance)]
		public static void AttrWriter(RubyScope scope, RubyModule self, [NotNullItems][DefaultProtocol] params string[] names)
		{
			foreach (string name in names)
			{
				DefineAccessor(scope, self, name, false, true);
			}
		}

		[RubyMethod("alias_method", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule AliasMethod(RubyContext context, RubyModule self, [NotNull][DefaultProtocol] string newName, [NotNull][DefaultProtocol] string oldName)
		{
			self.AddMethodAlias(newName, oldName);
			return self;
		}

		[RubyMethod("remove_method", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule RemoveMethod(RubyModule self, [DefaultProtocol][NotNullItems] params string[] methodNames)
		{
			foreach (string text in methodNames)
			{
				if (self.IsBasicObjectClass && text == Symbols.Initialize)
				{
					throw RubyExceptions.CreateNameError("Cannot remove BasicObject#initialize");
				}
				if (!self.RemoveMethod(text))
				{
					throw RubyExceptions.CreateUndefinedMethodError(self, text);
				}
			}
			return self;
		}

		[RubyMethod("undef_method", RubyMethodAttributes.PrivateInstance)]
		public static RubyModule UndefineMethod(RubyModule self, [NotNullItems][DefaultProtocol] params string[] methodNames)
		{
			foreach (string text in methodNames)
			{
				if (!self.ResolveMethod(text, VisibilityContext.AllVisible).Found)
				{
					throw RubyExceptions.CreateUndefinedMethodError(self, text);
				}
				self.UndefineMethod(text);
			}
			return self;
		}

		[RubyMethod("==")]
		public static bool Equals(RubyModule self, object other)
		{
			return object.ReferenceEquals(self, other);
		}

		[RubyMethod("===")]
		public static bool CaseEquals(RubyModule self, object other)
		{
			return self.Context.IsKindOf(other, self);
		}

		[RubyMethod("<")]
		public static object IsSubclassOrIncluded(RubyModule self, [NotNull] RubyModule module)
		{
			if (object.ReferenceEquals(self, module))
			{
				return ScriptingRuntimeHelpers.False;
			}
			if (!self.HasAncestor(module))
			{
				return null;
			}
			return ScriptingRuntimeHelpers.True;
		}

		[RubyMethod("<=")]
		public static object IsSubclassSameOrIncluded(RubyModule self, [NotNull] RubyModule module)
		{
			if (self.Context != module.Context)
			{
				return null;
			}
			using (self.Context.ClassHierarchyLocker())
			{
				if (self.HasAncestorNoLock(module))
				{
					return ScriptingRuntimeHelpers.True;
				}
				return module.HasAncestorNoLock(self) ? ScriptingRuntimeHelpers.False : null;
			}
		}

		[RubyMethod(">")]
		public static object IsNotSubclassOrIncluded(RubyModule self, [NotNull] RubyModule module)
		{
			if (object.ReferenceEquals(self, module))
			{
				return false;
			}
			if (!module.HasAncestor(self))
			{
				return null;
			}
			return ScriptingRuntimeHelpers.True;
		}

		[RubyMethod(">=")]
		public static object IsNotSubclassSameOrIncluded(RubyModule self, [NotNull] RubyModule module)
		{
			if (self.Context != module.Context)
			{
				return null;
			}
			using (self.Context.ClassHierarchyLocker())
			{
				if (module.HasAncestorNoLock(self))
				{
					return ScriptingRuntimeHelpers.True;
				}
				return self.HasAncestorNoLock(module) ? ScriptingRuntimeHelpers.False : null;
			}
		}

		[RubyMethod("<=>")]
		public static object Comparison(RubyModule self, [NotNull] RubyModule module)
		{
			if (object.ReferenceEquals(self, module))
			{
				return ClrInteger.Zero;
			}
			if (self.Context != module.Context)
			{
				return null;
			}
			using (self.Context.ClassHierarchyLocker())
			{
				if (self.HasAncestorNoLock(module))
				{
					return ClrInteger.MinusOne;
				}
				if (module.HasAncestorNoLock(self))
				{
					return ClrInteger.One;
				}
			}
			return null;
		}

		[RubyMethod("<=>")]
		public static object Comparison(RubyModule self, object module)
		{
			return null;
		}

		[RubyMethod(">=")]
		[RubyMethod(">")]
		[RubyMethod("<=")]
		[RubyMethod("<")]
		public static object InvalidComparison(RubyModule self, object module)
		{
			throw RubyExceptions.CreateTypeError("compared with non class/module");
		}

		[RubyMethod("ancestors")]
		public static RubyArray Ancestors(RubyModule self)
		{
			RubyArray ancestors = new RubyArray();
			using (self.Context.ClassHierarchyLocker())
			{
				self.ForEachAncestor(true, delegate(RubyModule module)
				{
					if (!module.IsSingletonClass)
					{
						ancestors.Add(module);
					}
					return false;
				});
			}
			return ancestors;
		}

		[RubyMethod("included_modules")]
		public static RubyArray GetIncludedModules(RubyModule self)
		{
			RubyArray ancestorModules = new RubyArray();
			using (self.Context.ClassHierarchyLocker())
			{
				self.ForEachAncestor(true, delegate(RubyModule module)
				{
					if (module != self && !module.IsClass && !ancestorModules.Contains(module))
					{
						ancestorModules.Add(module);
					}
					return false;
				});
			}
			return ancestorModules;
		}

		[RubyMethod("include?")]
		public static bool IncludesModule(RubyModule self, [NotNull] RubyModule other)
		{
			if (other.IsClass)
			{
				throw RubyExceptions.CreateTypeError("wrong argument type Class (expected Module)");
			}
			if (other != self)
			{
				return self.HasAncestor(other);
			}
			return false;
		}

		[RubyMethod("class_eval")]
		[RubyMethod("module_eval")]
		public static object Evaluate(RubyScope scope, BlockParam block, RubyModule self, [DefaultProtocol][NotNull] MutableString code, [Optional][NotNull] MutableString file, int line)
		{
			if (block != null)
			{
				throw RubyExceptions.CreateArgumentError("wrong number of arguments");
			}
			return RubyUtils.Evaluate(code, scope, self, self, file, line);
		}

		[RubyMethod("module_eval")]
		[RubyMethod("class_eval")]
		public static object Evaluate([NotNull] BlockParam block, RubyModule self)
		{
			return RubyUtils.EvaluateInModule(self, block, null);
		}

		[RubyMethod("class_exec")]
		[RubyMethod("module_exec")]
		public static object Execute([NotNull] BlockParam block, RubyModule self, params object[] args)
		{
			return RubyUtils.EvaluateInModule(self, block, args);
		}

		[RubyMethod("class_variables")]
		public static RubyArray ClassVariables(RubyModule self)
		{
			RubyArray result = new RubyArray();
			self.EnumerateClassVariables(delegate(RubyModule module, string name, object value)
			{
				result.Add(self.Context.StringifyIdentifier(name));
				return false;
			});
			return result;
		}

		[RubyMethod("class_variable_defined?")]
		public static bool IsClassVariableDefined(RubyModule self, [NotNull][DefaultProtocol] string variableName)
		{
			object value;
			if (self.TryResolveClassVariable(variableName, out value) == null)
			{
				RubyUtils.CheckClassVariableName(variableName);
				return false;
			}
			return true;
		}

		[RubyMethod("class_variable_get", RubyMethodAttributes.PrivateInstance)]
		public static object GetClassVariable(RubyModule self, [NotNull][DefaultProtocol] string variableName)
		{
			object value;
			if (self.TryResolveClassVariable(variableName, out value) == null)
			{
				RubyUtils.CheckClassVariableName(variableName);
				throw RubyExceptions.CreateNameError("uninitialized class variable {0} in {1}", variableName, self.Name);
			}
			return value;
		}

		[RubyMethod("class_variable_set", RubyMethodAttributes.PrivateInstance)]
		public static object ClassVariableSet(RubyModule self, [DefaultProtocol][NotNull] string variableName, object value)
		{
			RubyUtils.CheckClassVariableName(variableName);
			self.SetClassVariable(variableName, value);
			return value;
		}

		[RubyMethod("remove_class_variable", RubyMethodAttributes.PrivateInstance)]
		public static object RemoveClassVariable(RubyModule self, [NotNull][DefaultProtocol] string variableName)
		{
			object value;
			if (!self.TryGetClassVariable(variableName, out value))
			{
				RubyUtils.CheckClassVariableName(variableName);
				throw RubyExceptions.CreateNameError("class variable {0} not defined for {1}", variableName, self.Name);
			}
			self.RemoveClassVariable(variableName);
			return value;
		}

		[RubyMethod("constants", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetGlobalConstants(RubyModule self, bool inherited)
		{
			return GetDefinedConstants(self.Context.ObjectClass, inherited);
		}

		[RubyMethod("constants")]
		public static RubyArray GetDefinedConstants(RubyModule self, bool inherited)
		{
			RubyArray result = new RubyArray();
			if (inherited)
			{
				Dictionary<string, bool> visited = new Dictionary<string, bool>();
				bool hideGlobalConstants = !self.IsObjectClass;
				using (self.Context.ClassHierarchyLocker())
				{
					self.ForEachConstant(true, delegate(RubyModule module, string name, object value)
					{
						if (name == null)
						{
							if (hideGlobalConstants)
							{
								return module.IsObjectClass;
							}
							return false;
						}
						if (!visited.ContainsKey(name))
						{
							if (Tokenizer.IsConstantName(name))
							{
								result.Add(self.Context.StringifyIdentifier(name));
							}
							visited.Add(name, true);
						}
						return false;
					});
				}
			}
			else
			{
				using (self.Context.ClassHierarchyLocker())
				{
					self.EnumerateConstants(delegate(RubyModule module, string name, object value)
					{
						if (Tokenizer.IsConstantName(name))
						{
							result.Add(self.Context.StringifyIdentifier(name));
						}
						return false;
					});
				}
			}
			return result;
		}

		[RubyMethod("const_defined?")]
		public static bool IsConstantDefined(RubyModule self, [DefaultProtocol][NotNull] string constantName)
		{
			RubyUtils.CheckConstantName(constantName);
			object value;
			return self.TryGetConstant(null, constantName, out value);
		}

		[RubyMethod("const_get")]
		public static object GetConstantValue(RubyScope scope, RubyModule self, [NotNull][DefaultProtocol] string constantName)
		{
			return RubyUtils.GetConstant(scope.GlobalScope, self, constantName, true);
		}

		[RubyMethod("const_set")]
		public static object SetConstantValue(RubyModule self, [NotNull][DefaultProtocol] string constantName, object value)
		{
			RubyUtils.CheckConstantName(constantName);
			RubyUtils.SetConstant(self, constantName, value);
			return value;
		}

		[RubyMethod("remove_const", RubyMethodAttributes.PrivateInstance)]
		public static object RemoveConstant(RubyModule self, [DefaultProtocol][NotNull] string constantName)
		{
			object value;
			if (!self.TryRemoveConstant(constantName, out value))
			{
				RubyUtils.CheckConstantName(constantName);
				throw RubyExceptions.CreateNameError("constant {0}::{1} not defined", self.Name, constantName);
			}
			return value;
		}

		[RubyMethod("const_missing")]
		public static object ConstantMissing(RubyModule self, [NotNull][DefaultProtocol] string name)
		{
			return self.Context.ResolveMissingConstant(self, name);
		}

		[RubyMethod("autoload")]
		public static void SetAutoloadedConstant(RubyModule self, [NotNull][DefaultProtocol] string constantName, [NotNull][DefaultProtocol] MutableString path)
		{
			RubyUtils.CheckConstantName(constantName);
			if (path.IsEmpty)
			{
				throw RubyExceptions.CreateArgumentError("empty file name");
			}
			self.SetAutoloadedConstant(constantName, path);
		}

		[RubyMethod("autoload?")]
		public static MutableString GetAutoloadedConstantPath(RubyModule self, [DefaultProtocol][NotNull] string constantName)
		{
			return self.GetAutoloadedConstantPath(constantName);
		}

		[RubyMethod("instance_methods")]
		public static RubyArray GetInstanceMethods(RubyModule self)
		{
			return GetInstanceMethods(self, true);
		}

		[RubyMethod("instance_methods")]
		public static RubyArray GetInstanceMethods(RubyModule self, bool inherited)
		{
			return GetMethods(self, inherited, RubyMethodAttributes.PublicInstance | RubyMethodAttributes.Protected);
		}

		[RubyMethod("private_instance_methods")]
		public static RubyArray GetPrivateInstanceMethods(RubyModule self)
		{
			return GetPrivateInstanceMethods(self, true);
		}

		[RubyMethod("private_instance_methods")]
		public static RubyArray GetPrivateInstanceMethods(RubyModule self, bool inherited)
		{
			return GetMethods(self, inherited, RubyMethodAttributes.PrivateInstance);
		}

		[RubyMethod("protected_instance_methods")]
		public static RubyArray GetProtectedInstanceMethods(RubyModule self)
		{
			return GetProtectedInstanceMethods(self, true);
		}

		[RubyMethod("protected_instance_methods")]
		public static RubyArray GetProtectedInstanceMethods(RubyModule self, bool inherited)
		{
			return GetMethods(self, inherited, RubyMethodAttributes.ProtectedInstance);
		}

		[RubyMethod("public_instance_methods")]
		public static RubyArray GetPublicInstanceMethods(RubyModule self)
		{
			return GetPublicInstanceMethods(self, true);
		}

		[RubyMethod("public_instance_methods")]
		public static RubyArray GetPublicInstanceMethods(RubyModule self, bool inherited)
		{
			return GetMethods(self, inherited, RubyMethodAttributes.PublicInstance);
		}

		internal static RubyArray GetMethods(RubyModule self, bool inherited, RubyMethodAttributes attributes)
		{
			return GetMethods(self, inherited, attributes, null);
		}

		internal static RubyArray GetMethods(RubyModule self, bool inherited, RubyMethodAttributes attributes, IEnumerable<string> foreignMembers)
		{
			RubyArray result = new RubyArray();
			using (self.Context.ClassHierarchyLocker())
			{
				self.ForEachMember(inherited, attributes, foreignMembers, delegate(string name, RubyModule module, RubyMemberInfo member)
				{
					if (member.IsInteropMember && (module.Restrictions & ModuleRestrictions.NoNameMapping) == 0 && RubyUtils.HasMangledName(name))
					{
						if (Tokenizer.IsMethodName(name) || Tokenizer.IsOperatorName(name))
						{
							result.Add(new ClrName(name));
						}
					}
					else
					{
						result.Add(self.Context.StringifyIdentifier(name));
					}
				});
			}
			return result;
		}

		[RubyMethod("method_defined?")]
		public static bool MethodDefined(RubyModule self, [DefaultProtocol][NotNull] string methodName)
		{
			RubyMemberInfo info = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
			if (info != null)
			{
				return info.Visibility != RubyMethodVisibility.Private;
			}
			return false;
		}

		[RubyMethod("private_method_defined?")]
		public static bool PrivateMethodDefined(RubyModule self, [NotNull][DefaultProtocol] string methodName)
		{
			RubyMemberInfo info = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
			if (info != null)
			{
				return info.Visibility == RubyMethodVisibility.Private;
			}
			return false;
		}

		[RubyMethod("protected_method_defined?")]
		public static bool ProtectedMethodDefined(RubyModule self, [DefaultProtocol][NotNull] string methodName)
		{
			RubyMemberInfo info = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
			if (info != null)
			{
				return info.Visibility == RubyMethodVisibility.Protected;
			}
			return false;
		}

		[RubyMethod("public_method_defined?")]
		public static bool PublicMethodDefined(RubyModule self, [NotNull][DefaultProtocol] string methodName)
		{
			RubyMemberInfo info = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
			if (info != null)
			{
				return info.Visibility == RubyMethodVisibility.Public;
			}
			return false;
		}

		[RubyMethod("instance_method")]
		public static UnboundMethod GetInstanceMethod(RubyModule self, [NotNull][DefaultProtocol] string methodName)
		{
			RubyMemberInfo info = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
			if (info == null)
			{
				throw RubyExceptions.CreateUndefinedMethodError(self, methodName);
			}
			RubyModule targetConstraint = self;
			if (self.IsSingletonClass && info.DeclaringModule != self)
			{
				targetConstraint = ((RubyClass)self).SuperClass;
			}
			return new UnboundMethod(targetConstraint, methodName, info);
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(RubyContext context, RubyModule self)
		{
			return self.GetDisplayName(context, false);
		}

		[RubyMethod("name")]
		public static MutableString GetName(RubyContext context, RubyModule self)
		{
			return self.GetDisplayName(context, true);
		}

		[RubyMethod("freeze")]
		public static RubyModule Freeze(RubyContext context, RubyModule self)
		{
			self.Freeze();
			return self;
		}

		[RubyMethod("to_clr_type")]
		public static Type ToClrType(RubyModule self)
		{
			if (self.TypeTracker == null)
			{
				return null;
			}
			return self.TypeTracker.Type;
		}

		[RubyMethod("to_clr_ref")]
		public static RubyModule ToClrRef(RubyModule self)
		{
			try
			{
				return (self.TypeTracker != null) ? self.Context.GetClass(self.TypeTracker.Type.MakeByRefType()) : null;
			}
			catch (Exception)
			{
				throw RubyExceptions.CreateTypeError("Cannot create by-ref type for `{0}'", self.Context.GetTypeName(self.TypeTracker.Type, true));
			}
		}

		[RubyMethod("[]")]
		[RubyMethod("of")]
		public static RubyModule Of(RubyModule self, [NotNullItems] params object[] typeArgs)
		{
			if (self.TypeTracker == null)
			{
				throw RubyExceptions.CreateArgumentError("'{0}' is not a type", self.Name);
			}
			Type type = self.TypeTracker.Type;
			int num = typeArgs.Length;
			if (num == 1 && type == typeof(Array))
			{
				Type type2 = Protocols.ToType(self.Context, typeArgs[0]);
				Type type3;
				try
				{
					type3 = type2.MakeArrayType();
				}
				catch (Exception)
				{
					throw RubyExceptions.CreateTypeError("Cannot create array type for `{0}'", self.Context.GetTypeName(type2, true));
				}
				return self.Context.GetModule(type3);
			}
			if (!type.IsGenericTypeDefinition)
			{
				if (num > 0)
				{
					throw RubyExceptions.CreateArgumentError("`{0}' is not a generic type definition", self.Name);
				}
				return self;
			}
			int num2 = type.GetGenericArguments().Length;
			if (num2 != num)
			{
				throw RubyExceptions.CreateArgumentError("Type `{0}' requires {1} generic type arguments, {2} provided", self.Name, num2, num);
			}
			Type type4 = type.MakeGenericType(Protocols.ToTypes(self.Context, typeArgs));
			return self.Context.GetModule(type4);
		}

		[RubyMethod("[]")]
		[RubyMethod("of")]
		public static RubyModule Of(RubyModule self, int genericArity)
		{
			if (self.TypeTracker == null)
			{
				throw RubyExceptions.CreateArgumentError("`{0}' is not a type", self.Name);
			}
			Type type = self.TypeTracker.Type;
			if (!type.IsGenericTypeDefinition)
			{
				if (genericArity > 0)
				{
					throw RubyExceptions.CreateArgumentError("`{0}' is not a generic type definition", self.Name);
				}
				return self;
			}
			if (type.GetGenericArguments().Length != genericArity)
			{
				throw RubyExceptions.CreateArgumentError("`{0}' does not have generic arity {1}", self.Name, genericArity);
			}
			return self;
		}

		[RubyMethod("nesting", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetLexicalModuleNesting(RubyScope scope, RubyModule self)
		{
			RubyArray rubyArray = new RubyArray();
			do
			{
				if (scope.Module != null)
				{
					rubyArray.Add(scope.Module);
				}
				scope = scope.Parent;
			}
			while (scope != null);
			return rubyArray;
		}
	}
}
