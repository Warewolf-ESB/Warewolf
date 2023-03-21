using System;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime.Conversions
{
	public interface IConvertibleRubyMetaObject : IConvertibleMetaObject
	{
		Convertibility IsConvertibleTo(Type type, bool isExplicit);
	}
}
