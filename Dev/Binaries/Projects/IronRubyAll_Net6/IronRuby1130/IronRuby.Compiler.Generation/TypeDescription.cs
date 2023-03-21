using System;
using System.Collections.Generic;

namespace IronRuby.Compiler.Generation
{
	internal sealed class TypeDescription
	{
		private readonly Type _baseType;

		private readonly bool _noOverrides;

		private readonly IList<ITypeFeature> _features;

		private readonly int _hash;

		public Type BaseType
		{
			get
			{
				return _baseType;
			}
		}

		public bool NoOverrides
		{
			get
			{
				return _noOverrides;
			}
		}

		public IList<ITypeFeature> Features
		{
			get
			{
				return _features;
			}
		}

		public TypeDescription(Type baseType, IList<ITypeFeature> features, bool noOverrides)
		{
			_noOverrides = noOverrides;
			_baseType = baseType;
			_features = features;
			_hash = (_noOverrides ? 3 : 7) ^ _baseType.GetHashCode();
			for (int i = 0; i < features.Count; i++)
			{
				_hash ^= features[i].GetHashCode();
			}
		}

		public override int GetHashCode()
		{
			return _hash;
		}

		public static bool FeatureSetsMatch(IList<ITypeFeature> f1, IList<ITypeFeature> f2)
		{
			if (f1.Count != f2.Count || f1.GetHashCode() != f2.GetHashCode())
			{
				return false;
			}
			foreach (ITypeFeature item in f2)
			{
				if (!f1.Contains(item))
				{
					return false;
				}
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			TypeDescription typeDescription = obj as TypeDescription;
			if (typeDescription == null)
			{
				return false;
			}
			if (_baseType.Equals(typeDescription._baseType) && FeatureSetsMatch(Features, typeDescription.Features))
			{
				return true;
			}
			return false;
		}
	}
}
