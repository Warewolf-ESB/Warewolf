using System;
using System.Reflection.Emit;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Generation
{
	internal sealed class RubyTypeFeature : ITypeFeature
	{
		internal static readonly RubyTypeFeature Instance = new RubyTypeFeature();

		public bool CanInherit
		{
			get
			{
				return true;
			}
		}

		public bool IsImplementedBy(Type type)
		{
			return typeof(IRubyObject).IsAssignableFrom(type);
		}

		public IFeatureBuilder MakeBuilder(TypeBuilder tb)
		{
			return new RubyTypeBuilder(tb);
		}

		public override int GetHashCode()
		{
			return typeof(RubyTypeFeature).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(obj, Instance);
		}
	}
}
