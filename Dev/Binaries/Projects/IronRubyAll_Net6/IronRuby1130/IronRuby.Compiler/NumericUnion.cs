using System.Runtime.InteropServices;

namespace IronRuby.Compiler
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct NumericUnion
	{
		[FieldOffset(0)]
		public int Integer1;

		[FieldOffset(4)]
		public int Integer2;

		[FieldOffset(0)]
		public double Double;
	}
}
