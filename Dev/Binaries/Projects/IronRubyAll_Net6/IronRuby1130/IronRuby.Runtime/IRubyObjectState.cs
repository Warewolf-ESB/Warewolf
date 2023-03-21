namespace IronRuby.Runtime
{
	public interface IRubyObjectState
	{
		bool IsFrozen { get; }

		bool IsTainted { get; set; }

		bool IsUntrusted { get; set; }

		void Freeze();
	}
}
