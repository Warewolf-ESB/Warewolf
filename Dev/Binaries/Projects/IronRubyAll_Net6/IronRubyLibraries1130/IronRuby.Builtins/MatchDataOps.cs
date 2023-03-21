using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[UndefineMethod("new", IsStatic = true)]
	[RubyClass("MatchData", Extends = typeof(MatchData), Inherits = typeof(object))]
	public static class MatchDataOps
	{
		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static MatchData InitializeCopy(MatchData self, [NotNull] MatchData other)
		{
			self.InitializeFrom(other);
			return self;
		}

		[RubyMethod("[]")]
		public static MutableString GetGroup(MatchData self, [DefaultProtocol] int index)
		{
			index = IListOps.NormalizeIndex(self.GroupCount, index);
			return self.GetGroupValue(index);
		}

		[RubyMethod("[]")]
		public static RubyArray GetGroup(MatchData self, [DefaultProtocol] int start, [DefaultProtocol] int length)
		{
			if (!IListOps.NormalizeRange(self.GroupCount, ref start, ref length))
			{
				return null;
			}
			RubyArray rubyArray = new RubyArray();
			for (int i = 0; i < length; i++)
			{
				rubyArray.Add(self.GetGroupValue(start + i));
			}
			return rubyArray;
		}

		[RubyMethod("[]")]
		public static RubyArray GetGroup(ConversionStorage<int> fixnumCast, MatchData self, [NotNull] Range range)
		{
			int begin;
			int count;
			if (!IListOps.NormalizeRange(fixnumCast, self.GroupCount, range, out begin, out count))
			{
				return null;
			}
			return GetGroup(self, begin, count);
		}

		[RubyMethod("begin")]
		public static object Begin(MatchData self, [DefaultProtocol] int groupIndex)
		{
			self.RequireExistingGroup(groupIndex);
			if (!self.GroupSuccess(groupIndex))
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(self.GetGroupStart(groupIndex));
		}

		[RubyMethod("end")]
		public static object End(MatchData self, [DefaultProtocol] int groupIndex)
		{
			self.RequireExistingGroup(groupIndex);
			if (!self.GroupSuccess(groupIndex))
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(self.GetGroupEnd(groupIndex));
		}

		[RubyMethod("size")]
		[RubyMethod("length")]
		public static int Length(MatchData self)
		{
			return self.GroupCount;
		}

		[RubyMethod("offset")]
		public static RubyArray Offset(MatchData self, [DefaultProtocol] int groupIndex)
		{
			self.RequireExistingGroup(groupIndex);
			RubyArray rubyArray = new RubyArray(2);
			if (self.GroupSuccess(groupIndex))
			{
				rubyArray.Add(self.GetGroupStart(groupIndex));
				rubyArray.Add(self.GetGroupEnd(groupIndex));
			}
			else
			{
				rubyArray.Add(null);
				rubyArray.Add(null);
			}
			return rubyArray;
		}

		[RubyMethod("pre_match")]
		public static MutableString PreMatch(MatchData self)
		{
			return self.GetPreMatch();
		}

		[RubyMethod("post_match")]
		public static MutableString PostMatch(MatchData self)
		{
			return self.GetPostMatch();
		}

		private static RubyArray ReturnMatchingGroups(MatchData self, int groupIndex)
		{
			if (self.GroupCount < groupIndex)
			{
				return new RubyArray();
			}
			RubyArray rubyArray = new RubyArray(self.GroupCount - groupIndex);
			for (int i = groupIndex; i < self.GroupCount; i++)
			{
				rubyArray.Add(self.GetGroupValue(i));
			}
			return rubyArray;
		}

		[RubyMethod("captures")]
		public static RubyArray Captures(MatchData self)
		{
			return ReturnMatchingGroups(self, 1);
		}

		[RubyMethod("to_a")]
		public static RubyArray ToArray(MatchData self)
		{
			return ReturnMatchingGroups(self, 0);
		}

		[RubyMethod("string")]
		public static MutableString ReturnFrozenString(RubyContext context, MatchData self)
		{
			return MutableString.Create(self.OriginalString).TaintBy(self, context).Freeze();
		}

		[RubyMethod("select")]
		public static object Select([NotNull] BlockParam block, MatchData self)
		{
			RubyArray rubyArray = new RubyArray();
			for (int i = 0; i < self.GroupCount; i++)
			{
				MutableString groupValue = self.GetGroupValue(i);
				object blockResult;
				if (block.Yield(groupValue, out blockResult))
				{
					return blockResult;
				}
				if (RubyOps.IsTrue(blockResult))
				{
					rubyArray.Add(groupValue);
				}
			}
			return rubyArray;
		}

		[RubyMethod("values_at")]
		public static RubyArray ValuesAt(ConversionStorage<int> conversionStorage, MatchData self, [DefaultProtocol] params int[] indices)
		{
			RubyArray rubyArray = new RubyArray();
			for (int i = 0; i < indices.Length; i++)
			{
				rubyArray.Add(GetGroup(self, indices[i]));
			}
			return rubyArray;
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, MatchData self)
		{
			return RubyUtils.ObjectToMutableString(context, self);
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(MatchData self)
		{
			return self.GetValue();
		}
	}
}
