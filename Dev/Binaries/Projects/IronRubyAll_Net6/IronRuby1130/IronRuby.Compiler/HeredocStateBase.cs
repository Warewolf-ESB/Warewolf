namespace IronRuby.Compiler
{
	internal abstract class HeredocStateBase : TokenSequenceState
	{
		private readonly StringProperties _properties;

		private readonly string _label;

		public StringProperties Properties
		{
			get
			{
				return _properties;
			}
		}

		public string Label
		{
			get
			{
				return _label;
			}
		}

		public HeredocStateBase(StringProperties properties, string label)
		{
			_properties = properties;
			_label = label;
		}

		protected bool Equals(HeredocStateBase other)
		{
			if (_properties == other._properties)
			{
				return _label == other._label;
			}
			return false;
		}

		protected int GetBaseHashCode()
		{
			return (int)_properties ^ _label.GetHashCode();
		}

		internal abstract Tokens Finish(Tokenizer tokenizer, int labelStart);
	}
}
