namespace IronRuby.Runtime
{
	internal sealed class GlobalVariableInfo : GlobalVariable
	{
		private object _value;

		private bool _isDefined;

		public override bool IsDefined
		{
			get
			{
				return _isDefined;
			}
		}

		internal GlobalVariableInfo(object value)
			: this(value, true)
		{
		}

		internal GlobalVariableInfo(object value, bool isDefined)
		{
			_value = value;
			_isDefined = isDefined;
		}

		public override object GetValue(RubyContext context, RubyScope scope)
		{
			return _value;
		}

		public override void SetValue(RubyContext context, RubyScope scope, string name, object value)
		{
			_value = value;
			_isDefined = true;
		}
	}
}
