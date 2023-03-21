namespace IronRuby.Runtime
{
	internal sealed class ReadOnlyGlobalVariableInfo : GlobalVariable
	{
		private object _value;

		public ReadOnlyGlobalVariableInfo(object value)
		{
			_value = value;
		}

		public override object GetValue(RubyContext context, RubyScope scope)
		{
			return _value;
		}

		public override void SetValue(RubyContext context, RubyScope scope, string name, object value)
		{
			throw ReadOnlyError(name);
		}
	}
}
