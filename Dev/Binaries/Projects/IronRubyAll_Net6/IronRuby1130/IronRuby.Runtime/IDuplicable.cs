namespace IronRuby.Runtime
{
	public interface IDuplicable
	{
		object Duplicate(RubyContext context, bool copySingletonMembers);
	}
}
