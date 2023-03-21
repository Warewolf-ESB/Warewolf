using System;
using System.Reflection.Emit;

namespace IronRuby.Compiler.Generation
{
	internal interface ITypeFeature
	{
		bool CanInherit { get; }

		bool IsImplementedBy(Type type);

		IFeatureBuilder MakeBuilder(TypeBuilder tb);
	}
}
