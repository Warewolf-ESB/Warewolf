using System.Reflection;

namespace IronRuby.Runtime
{
	public sealed class BlockUnwinder : StackUnwinder
	{
		public readonly bool IsRedo;

		internal static FieldInfo IsRedoField
		{
			get
			{
				return typeof(BlockUnwinder).GetField("IsRedo");
			}
		}

		internal BlockUnwinder(object returnValue, bool isRedo)
			: base(returnValue)
		{
			IsRedo = isRedo;
		}
	}
}
