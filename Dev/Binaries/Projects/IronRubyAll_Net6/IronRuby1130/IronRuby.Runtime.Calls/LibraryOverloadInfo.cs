using System;

namespace IronRuby.Runtime.Calls
{
	internal sealed class LibraryOverloadInfo : LibraryOverload
	{
		public override bool IsVariadic
		{
			get
			{
				return false;
			}
		}

		internal LibraryOverloadInfo(Delegate overloadDelegate, short defaultProtocolAttrs, short notNullAttrs)
			: base(overloadDelegate, defaultProtocolAttrs, notNullAttrs)
		{
		}
	}
}
