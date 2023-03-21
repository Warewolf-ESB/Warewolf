using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Range = IronRuby.Builtins.Range;

namespace IronRuby.Compiler.Generation
{
	internal static class RubyTypeDispenser
	{
		private static readonly Publisher<TypeDescription, Type> _newTypes;

		private static readonly Dictionary<Type, IList<ITypeFeature>> _typeFeatures;

		private static readonly ITypeFeature[] _defaultFeatures;

		static RubyTypeDispenser()
		{
			_defaultFeatures = new ITypeFeature[2]
			{
				RubyTypeFeature.Instance,
				InterfaceImplFeature.Create(Type.EmptyTypes)
			};
			_newTypes = new Publisher<TypeDescription, Type>();
			_typeFeatures = new Dictionary<Type, IList<ITypeFeature>>();
			AddBuiltinType(typeof(object), typeof(RubyObject), false);
			AddBuiltinType(typeof(MutableString), typeof(MutableString.Subclass), true);
			AddBuiltinType(typeof(Proc), typeof(Proc.Subclass), true);
			AddBuiltinType(typeof(RubyRegex), typeof(RubyRegex.Subclass), true);
			AddBuiltinType(typeof(Range), typeof(Range.Subclass), true);
			AddBuiltinType(typeof(Hash), typeof(Hash.Subclass), true);
			AddBuiltinType(typeof(RubyArray), typeof(RubyArray.Subclass), true);
			AddBuiltinType(typeof(MatchData), typeof(MatchData.Subclass), true);
			AddBuiltinType(typeof(RubyIO), typeof(RubyIO.Subclass), true);
		}

		internal static Type GetOrCreateType(Type baseType, IList<Type> interfaces, bool noOverrides)
		{
			ITypeFeature[] features;
			if (interfaces.Count == 0)
			{
				features = _defaultFeatures;
			}
			else
			{
				features = new ITypeFeature[2]
				{
					RubyTypeFeature.Instance,
					InterfaceImplFeature.Create(interfaces)
				};
			}
			noOverrides |= typeof(IRubyType).IsAssignableFrom(baseType);
			TypeDescription typeInfo = new TypeDescription(baseType, features, noOverrides);
			return _newTypes.GetOrCreateValue(typeInfo, () => TypeImplementsFeatures(baseType, features) ? baseType : CreateType(typeInfo));
		}

		internal static bool TryGetFeatures(Type type, out IList<ITypeFeature> features)
		{
			lock (_typeFeatures)
			{
				return _typeFeatures.TryGetValue(type, out features);
			}
		}

		private static bool TypeImplementsFeatures(Type type, IList<ITypeFeature> features)
		{
			IList<ITypeFeature> features2;
			if (TryGetFeatures(type, out features2))
			{
				return TypeDescription.FeatureSetsMatch(features, features2);
			}
			foreach (ITypeFeature feature in features)
			{
				if (!feature.IsImplementedBy(type))
				{
					return false;
				}
			}
			return true;
		}

		private static Type CreateType(TypeDescription typeInfo)
		{
			Type baseType = typeInfo.BaseType;
			if (baseType.IsSealed)
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Can't inherit from a sealed type {0}.", new object[1] { RubyContext.GetQualifiedNameNoLock(baseType, null, false) }));
			}
			string name = GetName(baseType);
			TypeBuilder tb = Snippets.Shared.DefinePublicType(name, baseType);
			IFeatureBuilder[] array = new IFeatureBuilder[typeInfo.Features.Count];
			RubyTypeEmitter rubyTypeEmitter = new RubyTypeEmitter(tb);
			for (int i = 0; i < typeInfo.Features.Count; i++)
			{
				array[i] = typeInfo.Features[i].MakeBuilder(tb);
			}
			IFeatureBuilder[] array2 = array;
			foreach (IFeatureBuilder featureBuilder in array2)
			{
				featureBuilder.Implement(rubyTypeEmitter);
			}
			if (!typeInfo.NoOverrides)
			{
				rubyTypeEmitter.OverrideMethods(baseType);
			}
			Type type = rubyTypeEmitter.FinishType();
			lock (_typeFeatures)
			{
				_typeFeatures.Add(type, typeInfo.Features);
				return type;
			}
		}

		private static string GetName(Type baseType)
		{
			StringBuilder stringBuilder = new StringBuilder("IronRuby.Classes.");
			stringBuilder.Append(baseType.Name);
			return stringBuilder.ToString();
		}

		private static void AddBuiltinType(Type clsBaseType, Type rubyType, bool noOverrides)
		{
			AddBuiltinType(clsBaseType, rubyType, _defaultFeatures, noOverrides);
		}

		private static void AddBuiltinType(Type clsBaseType, Type rubyType, ITypeFeature[] features, bool noOverrides)
		{
			_newTypes.GetOrCreateValue(new TypeDescription(clsBaseType, features, noOverrides), () => rubyType);
			_typeFeatures[rubyType] = features;
		}
	}
}
