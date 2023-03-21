using System;

namespace IronRuby.Runtime.Calls
{
	internal sealed class LibraryVariadicOverloadInfo : LibraryOverload
	{
		public override bool IsVariadic
		{
			get
			{
				return true;
			}
		}

		internal LibraryVariadicOverloadInfo(Delegate overloadDelegate, short defaultProtocolAttrs, short notNullAttrs)
			: base(overloadDelegate, defaultProtocolAttrs, notNullAttrs)
		{
		}
	}
}
