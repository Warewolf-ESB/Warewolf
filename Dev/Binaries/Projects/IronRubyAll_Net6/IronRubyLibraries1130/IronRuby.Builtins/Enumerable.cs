using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("Enumerable")]
	public static class Enumerable
	{
		internal static object Each(CallSiteStorage<Func<CallSite, object, Proc, object>> each, object self, Proc block)
		{
			CallSite<Func<CallSite, object, Proc, object>> callSite = each.GetCallSite("each", new RubyCallSignature(0, (RubyCallFlags)24));
			return callSite.Target(callSite, self, block);
		}

		internal static object Each(CallSiteStorage<Func<CallSite, object, Proc, IList, object>> each, object self, IList args, Proc block)
		{
			CallSite<Func<CallSite, object, Proc, IList, object>> callSite = each.GetCallSite("each", new RubyCallSignature(0, (RubyCallFlags)26));
			return callSite.Target(callSite, self, block, args);
		}

		[RubyMethod("all?")]
		public static object TrueForAll(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			return TrueForItems(each, predicate, self, false, false);
		}

		[RubyMethod("none?")]
		public static object TrueForNone(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			return TrueForItems(each, predicate, self, true, false);
		}

		[RubyMethod("any?")]
		public static object TrueForAny(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			return TrueForItems(each, predicate, self, true, true);
		}

		private static object TrueForItems(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self, bool stop, bool positiveResult)
		{
			object result = ScriptingRuntimeHelpers.BooleanToObject(!positiveResult);
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (predicate != null)
				{
					object blockResult;
					if (predicate.Yield(item, out blockResult))
					{
						result = blockResult;
						return selfBlock.PropagateFlow(predicate, blockResult);
					}
					item = blockResult;
				}
				bool flag = Protocols.IsTrue(item);
				if (flag == stop)
				{
					result = ScriptingRuntimeHelpers.BooleanToObject(positiveResult);
					return selfBlock.Break(result);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("map")]
		[RubyMethod("collect")]
		public static Enumerator GetMapEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam collector, object self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => Map(each, block, self));
		}

		[RubyMethod("map")]
		[RubyMethod("collect")]
		public static object Map(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam collector, object self)
		{
			RubyArray resultArray = new RubyArray();
			object result = resultArray;
			if (collector.Proc.Dispatcher.ParameterCount <= 1 && !collector.Proc.Dispatcher.HasUnsplatParameter && !collector.Proc.Dispatcher.HasProcParameter)
			{
				Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
				{
					object blockResult2;
					if (collector.Yield(item, out blockResult2))
					{
						result = blockResult2;
						return selfBlock.PropagateFlow(collector, blockResult2);
					}
					resultArray.Add(blockResult2);
					return null;
				}));
			}
			else
			{
				Each(each, self, Proc.Create(each.Context, 0, delegate(BlockParam selfBlock, object _, object[] __, RubyArray args)
				{
					object blockResult;
					if (collector.YieldSplat(args, out blockResult))
					{
						result = blockResult;
						return selfBlock.PropagateFlow(collector, blockResult);
					}
					resultArray.Add(blockResult);
					return null;
				}));
			}
			return result;
		}

		[RubyMethod("detect")]
		[RubyMethod("find")]
		public static Enumerator GetFindEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, CallSiteStorage<Func<CallSite, object, object>> callStorage, BlockParam predicate, object self, [Optional] object ifNone)
		{
			return new Enumerator((RubyScope _, BlockParam block) => Find(each, callStorage, block, self, ifNone));
		}

		[RubyMethod("detect")]
		[RubyMethod("find")]
		public static object Find(CallSiteStorage<Func<CallSite, object, Proc, object>> each, CallSiteStorage<Func<CallSite, object, object>> callStorage, [NotNull] BlockParam predicate, object self, [Optional] object ifNone)
		{
			object result = Missing.Value;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				object blockResult;
				if (predicate.Yield(item, out blockResult))
				{
					result = blockResult;
					return selfBlock.PropagateFlow(predicate, blockResult);
				}
				if (Protocols.IsTrue(blockResult))
				{
					result = item;
					return selfBlock.Break(item);
				}
				return null;
			}));
			if (result == Missing.Value)
			{
				if (ifNone == Missing.Value || ifNone == null)
				{
					return null;
				}
				CallSite<Func<CallSite, object, object>> callSite = callStorage.GetCallSite("call", 0);
				result = callSite.Target(callSite, ifNone);
			}
			return result;
		}

		[RubyMethod("find_index")]
		public static Enumerator GetFindIndexEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => FindIndex(each, block, self));
		}

		[RubyMethod("find_index")]
		public static object FindIndex(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam predicate, object self)
		{
			int index = 0;
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				object blockResult;
				if (predicate.Yield(item, out blockResult))
				{
					result = blockResult;
					return selfBlock.PropagateFlow(predicate, blockResult);
				}
				if (Protocols.IsTrue(blockResult))
				{
					result = ScriptingRuntimeHelpers.Int32ToObject(index);
					return selfBlock.Break(null);
				}
				index++;
				return null;
			}));
			return result;
		}

		[RubyMethod("find_index")]
		public static object FindIndex(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BinaryOpStorage equals, BlockParam predicate, object self, object value)
		{
			if (predicate != null)
			{
				each.Context.ReportWarning("given block not used");
			}
			int index = 0;
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (Protocols.IsEqual(equals, item, value))
				{
					result = ScriptingRuntimeHelpers.Int32ToObject(index);
					return selfBlock.Break(null);
				}
				index++;
				return null;
			}));
			return result;
		}

		[RubyMethod("each_with_index")]
		public static Enumerator GetEachWithIndexEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam block, object self)
		{
			return new Enumerator((RubyScope _, BlockParam innerBlock) => EachWithIndex(each, innerBlock, self));
		}

		[RubyMethod("each_with_index")]
		public static object EachWithIndex(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam block, object self)
		{
			int index = 0;
			object result = self;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				object blockResult;
				if (block.Yield(item, index, out blockResult))
				{
					result = blockResult;
					return selfBlock.PropagateFlow(block, blockResult);
				}
				index++;
				return null;
			}));
			return result;
		}

		[RubyMethod("to_a")]
		[RubyMethod("entries")]
		public static RubyArray ToArray(CallSiteStorage<Func<CallSite, object, Proc, object>> each, object self)
		{
			RubyArray data = new RubyArray();
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				data.Add(item);
				return null;
			}));
			return data;
		}

		[RubyMethod("entries")]
		[RubyMethod("to_a")]
		public static RubyArray ToArray(CallSiteStorage<Func<CallSite, object, Proc, IList, object>> each, object self, params object[] args)
		{
			RubyArray data = new RubyArray();
			Each(each, self, args, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				data.Add(item);
				return null;
			}));
			return data;
		}

		[RubyMethod("select")]
		[RubyMethod("find_all")]
		public static object Select(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			if (predicate == null)
			{
				return FilterEnum(each, predicate, self, true);
			}
			return FilterImpl(each, predicate, self, true);
		}

		[RubyMethod("reject")]
		public static object Reject(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			if (predicate == null)
			{
				return FilterEnum(each, predicate, self, false);
			}
			return FilterImpl(each, predicate, self, false);
		}

		private static Enumerator FilterEnum(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self, bool acceptingValue)
		{
			return new Enumerator((RubyScope _, BlockParam block) => FilterImpl(each, block, self, acceptingValue));
		}

		private static object FilterImpl(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self, bool acceptingValue)
		{
			RubyArray resultArray = new RubyArray();
			object result = resultArray;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				object blockResult;
				if (predicate.Yield(item, out blockResult))
				{
					result = blockResult;
					return selfBlock.PropagateFlow(predicate, blockResult);
				}
				if (Protocols.IsTrue(blockResult) == acceptingValue)
				{
					resultArray.Add(item);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("grep")]
		public static object Grep(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BinaryOpStorage caseEquals, BlockParam action, object self, object pattern)
		{
			RubyArray resultArray = new RubyArray();
			object result = resultArray;
			CallSite<Func<CallSite, object, object, object>> site = caseEquals.GetCallSite("===");
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (RubyOps.IsTrue(site.Target(site, pattern, item)))
				{
					if (action != null && action.Yield(item, out item))
					{
						result = item;
						return selfBlock.PropagateFlow(action, item);
					}
					resultArray.Add(item);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("include?")]
		[RubyMethod("member?")]
		public static object Contains(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BinaryOpStorage equals, object self, object value)
		{
			object result = ScriptingRuntimeHelpers.BooleanToObject(false);
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (Protocols.IsEqual(equals, item, value))
				{
					result = ScriptingRuntimeHelpers.BooleanToObject(true);
					return selfBlock.Break(result);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("inject")]
		[RubyMethod("reduce")]
		public static object Inject(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam operation, object self, [Optional] object initial)
		{
			return Inject(each, operation, null, null, self, initial);
		}

		[RubyMethod("reduce")]
		[RubyMethod("inject")]
		public static object Inject(CallSiteStorage<Func<CallSite, object, Proc, object>> each, RubyScope scope, object self, [Optional] object initial, [DefaultProtocol][NotNull] string operatorName)
		{
			return Inject(each, null, scope, operatorName, self, initial);
		}

		internal static object Inject(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam operation, RubyScope scope, string operatorName, object self, object initial)
		{
			CallSite<Func<CallSite, RubyScope, object, object, object>> site = ((operatorName == null) ? null : each.Context.GetOrCreateSendSite<Func<CallSite, RubyScope, object, object, object>>(operatorName, new RubyCallSignature(1, (RubyCallFlags)17)));
			Each(each, self, Proc.Create(each.Context, 0, delegate(BlockParam selfBlock, object _, object[] __, RubyArray args)
			{
				object obj = ((args.Count == 0) ? null : ((args.Count == 1) ? args[0] : args));
				if (initial == Missing.Value)
				{
					initial = obj;
					return null;
				}
				if (site != null)
				{
					initial = site.Target(site, scope, initial, obj);
				}
				else if (operation.Yield(initial, obj, out initial))
				{
					return selfBlock.PropagateFlow(operation, initial);
				}
				return null;
			}));
			if (initial == Missing.Value)
			{
				return null;
			}
			return initial;
		}

		[RubyMethod("max")]
		public static object GetMaximum(CallSiteStorage<Func<CallSite, object, Proc, object>> each, ComparisonStorage comparisonStorage, BlockParam comparer, object self)
		{
			return GetExtreme(each, comparisonStorage, comparer, self, 1);
		}

		[RubyMethod("min")]
		public static object GetMinimum(CallSiteStorage<Func<CallSite, object, Proc, object>> each, ComparisonStorage comparisonStorage, BlockParam comparer, object self)
		{
			return GetExtreme(each, comparisonStorage, comparer, self, -1);
		}

		private static object GetExtreme(CallSiteStorage<Func<CallSite, object, Proc, object>> each, ComparisonStorage comparisonStorage, BlockParam comparer, object self, int comparisonValue)
		{
			bool firstItem = true;
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (firstItem)
				{
					result = item;
					firstItem = false;
					return null;
				}
				object blockResult;
				int? num = CompareItems(comparisonStorage, item, result, comparer, out blockResult);
				if (!num.HasValue)
				{
					result = blockResult;
					return selfBlock.PropagateFlow(comparer, blockResult);
				}
				if (num == comparisonValue)
				{
					result = item;
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("minmax")]
		public static object GetExtremes(CallSiteStorage<Func<CallSite, object, Proc, object>> each, ComparisonStorage comparisonStorage, BlockParam comparer, object self)
		{
			bool hasOddItem = false;
			bool hasMinMax = false;
			bool blockJumped = false;
			object oddItem = null;
			object blockResult = null;
			object min = null;
			object max = null;
			Func<BlockParam, object, object, object> clrMethod = delegate(BlockParam selfBlock, object _, object item)
			{
				if (hasOddItem)
				{
					hasOddItem = false;
					int? num2 = CompareItems(comparisonStorage, oddItem, item, comparer, out blockResult);
					if (num2.HasValue)
					{
						if (num2 > 0)
						{
							object obj = item;
							item = oddItem;
							oddItem = obj;
						}
						if (!hasMinMax)
						{
							min = oddItem;
							max = item;
							hasMinMax = true;
							goto IL_0139;
						}
						num2 = CompareItems(comparisonStorage, oddItem, min, comparer, out blockResult);
						if (num2.HasValue)
						{
							if (num2 < 0)
							{
								min = oddItem;
							}
							num2 = CompareItems(comparisonStorage, item, max, comparer, out blockResult);
							if (num2.HasValue)
							{
								if (num2 > 0)
								{
									max = item;
								}
								goto IL_0139;
							}
						}
					}
					blockJumped = true;
					return selfBlock.PropagateFlow(comparer, blockResult);
				}
				hasOddItem = true;
				oddItem = item;
				goto IL_0139;
				IL_0139:
				return null;
			};
			Each(each, self, Proc.Create(each.Context, clrMethod));
			if (blockJumped)
			{
				return blockResult;
			}
			if (!hasMinMax)
			{
				if (!hasOddItem)
				{
					RubyArray rubyArray = new RubyArray(2);
					rubyArray.Add(null);
					rubyArray.Add(null);
					return rubyArray;
				}
				RubyArray rubyArray2 = new RubyArray(2);
				rubyArray2.Add(oddItem);
				rubyArray2.Add(oddItem);
				return rubyArray2;
			}
			if (hasOddItem)
			{
				int? num = CompareItems(comparisonStorage, oddItem, min, comparer, out blockResult);
				if (!num.HasValue)
				{
					return blockResult;
				}
				if (num < 0)
				{
					min = oddItem;
				}
				num = CompareItems(comparisonStorage, oddItem, max, comparer, out blockResult);
				if (!num.HasValue)
				{
					return blockResult;
				}
				if (num > 0)
				{
					max = oddItem;
				}
			}
			RubyArray rubyArray3 = new RubyArray(2);
			rubyArray3.Add(min);
			rubyArray3.Add(max);
			return rubyArray3;
		}

		private static int? CompareItems(ComparisonStorage comparisonStorage, object left, object right, BlockParam comparer, out object blockResult)
		{
			if (comparer != null)
			{
				if (comparer.Yield(left, right, out blockResult))
				{
					return null;
				}
				if (blockResult == null)
				{
					throw RubyExceptions.MakeComparisonError(comparisonStorage.Context, left, right);
				}
				return Protocols.ConvertCompareResult(comparisonStorage, blockResult);
			}
			blockResult = null;
			return Protocols.Compare(comparisonStorage, left, right);
		}

		[RubyMethod("partition")]
		public static Enumerator GetPartitionEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => Partition(each, block, self));
		}

		[RubyMethod("partition")]
		public static object Partition(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam predicate, object self)
		{
			RubyArray trueSet = new RubyArray();
			RubyArray falseSet = new RubyArray();
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(trueSet);
			rubyArray.Add(falseSet);
			object result = rubyArray;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				object blockResult;
				if (predicate.Yield(item, out blockResult))
				{
					result = blockResult;
					return selfBlock.PropagateFlow(predicate, blockResult);
				}
				if (Protocols.IsTrue(blockResult))
				{
					trueSet.Add(item);
				}
				else
				{
					falseSet.Add(item);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("sort")]
		public static object Sort(CallSiteStorage<Func<CallSite, object, Proc, object>> each, ComparisonStorage comparisonStorage, BlockParam keySelector, object self)
		{
			return ArrayOps.SortInPlace(comparisonStorage, keySelector, ToArray(each, self));
		}

		[RubyMethod("sort_by")]
		public static object SortBy(CallSiteStorage<Func<CallSite, object, Proc, object>> each, ComparisonStorage comparisonStorage, BlockParam keySelector, object self)
		{
			List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (keySelector == null)
				{
					throw RubyExceptions.NoBlockGiven();
				}
				object blockResult;
				if (keySelector.Yield(item, out blockResult))
				{
					keyValuePairs = null;
					result = blockResult;
					return selfBlock.PropagateFlow(keySelector, blockResult);
				}
				keyValuePairs.Add(new KeyValuePair<object, object>(blockResult, item));
				return null;
			}));
			if (keyValuePairs == null)
			{
				return result;
			}
			keyValuePairs.Sort((KeyValuePair<object, object> x, KeyValuePair<object, object> y) => Protocols.Compare(comparisonStorage, x.Key, y.Key));
			RubyArray rubyArray = new RubyArray(keyValuePairs.Count);
			foreach (KeyValuePair<object, object> item in keyValuePairs)
			{
				rubyArray.Add(item.Value);
			}
			return rubyArray;
		}

		[RubyMethod("zip")]
		public static object Zip(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam block, object self, [NotNullItems][DefaultProtocol] params IList[] args)
		{
			RubyArray results = ((block == null) ? new RubyArray() : null);
			object result = results;
			int index = 0;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				RubyArray rubyArray = new RubyArray(args.Length + 1) { item };
				IList[] array = args;
				foreach (IList list in array)
				{
					if (index < list.Count)
					{
						rubyArray.Add(list[index]);
					}
					else
					{
						rubyArray.Add(null);
					}
				}
				index++;
				if (block != null)
				{
					object blockResult;
					if (block.Yield(rubyArray, out blockResult))
					{
						result = blockResult;
						return selfBlock.PropagateFlow(block, blockResult);
					}
				}
				else
				{
					results.Add(rubyArray);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("count")]
		public static int Count(CallSiteStorage<Func<CallSite, object, Proc, object>> each, object self)
		{
			int result = 0;
			Each(each, self, Proc.Create(each.Context, (Func<BlockParam, object, object, object>)delegate
			{
				result++;
				return null;
			}));
			return result;
		}

		[RubyMethod("count")]
		public static int Count(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BinaryOpStorage equals, BlockParam comparer, object self, object value)
		{
			if (comparer != null)
			{
				each.Context.ReportWarning("given block not used");
			}
			int result = 0;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (Protocols.IsEqual(equals, item, value))
				{
					result++;
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("count")]
		public static object Count(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BinaryOpStorage equals, [NotNull] BlockParam comparer, object self)
		{
			int count = 0;
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				object blockResult;
				if (comparer.Yield(item, out blockResult))
				{
					count = -1;
					result = blockResult;
					return selfBlock.PropagateFlow(comparer, blockResult);
				}
				if (Protocols.IsTrue(blockResult))
				{
					count++;
				}
				return null;
			}));
			if (count < 0)
			{
				return result;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(count);
		}

		[RubyMethod("one?")]
		public static object One(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BinaryOpStorage equals, BlockParam comparer, object self)
		{
			int count = 0;
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				object blockResult;
				if (comparer == null)
				{
					blockResult = item;
				}
				else if (comparer.Yield(item, out blockResult))
				{
					count = -1;
					result = blockResult;
					return selfBlock.PropagateFlow(comparer, blockResult);
				}
				if (Protocols.IsTrue(blockResult) && ++count > 1)
				{
					selfBlock.Break(null);
				}
				return null;
			}));
			if (count < 0)
			{
				return result;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(count == 1);
		}

		[RubyMethod("first")]
		public static object First(CallSiteStorage<Func<CallSite, object, Proc, object>> each, object self)
		{
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				result = item;
				selfBlock.Break(null);
				return null;
			}));
			return result;
		}

		[RubyMethod("first")]
		[RubyMethod("take")]
		public static RubyArray Take(CallSiteStorage<Func<CallSite, object, Proc, object>> each, object self, [DefaultProtocol] int count)
		{
			if (count < 0)
			{
				throw RubyExceptions.CreateArgumentError("attempt to take negative size");
			}
			RubyArray result = new RubyArray(count);
			if (count == 0)
			{
				return result;
			}
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				result.Add(item);
				if (--count == 0)
				{
					selfBlock.Break(null);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("take_while")]
		public static Enumerator GetTakeWhileEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => TakeWhile(each, block, self));
		}

		[RubyMethod("take_while")]
		public static object TakeWhile(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam predicate, object self)
		{
			RubyArray resultArray = new RubyArray();
			object result = resultArray;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				object blockResult;
				if (predicate.Yield(item, out blockResult))
				{
					result = blockResult;
					return selfBlock.PropagateFlow(predicate, blockResult);
				}
				if (Protocols.IsTrue(blockResult))
				{
					resultArray.Add(item);
				}
				else
				{
					selfBlock.Break(null);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("drop")]
		public static RubyArray Drop(CallSiteStorage<Func<CallSite, object, Proc, object>> each, object self, [DefaultProtocol] int count)
		{
			if (count < 0)
			{
				throw RubyExceptions.CreateArgumentError("attempt to drop negative size");
			}
			RubyArray result = new RubyArray();
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (count > 0)
				{
					count--;
				}
				else
				{
					result.Add(item);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("drop_while")]
		public static Enumerator GetDropWhileEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, object self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => DropWhile(each, block, self));
		}

		[RubyMethod("drop_while")]
		public static object DropWhile(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam predicate, object self)
		{
			RubyArray resultArray = new RubyArray();
			bool dropping = true;
			object result = resultArray;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (dropping)
				{
					object blockResult;
					if (predicate.Yield(item, out blockResult))
					{
						result = blockResult;
						return selfBlock.PropagateFlow(predicate, blockResult);
					}
					dropping = Protocols.IsTrue(blockResult);
				}
				if (!dropping)
				{
					resultArray.Add(item);
				}
				return null;
			}));
			return result;
		}

		[RubyMethod("cycle")]
		public static Enumerator GetCycleEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam block, object self, [DefaultProtocol] int iterations)
		{
			return new Enumerator((RubyScope _, BlockParam innerBlock) => Cycle(each, innerBlock, self, iterations));
		}

		[RubyMethod("cycle")]
		public static object Cycle(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam block, object self, DynamicNull iterations)
		{
			if (block == null)
			{
				return GetCycleEnumerator(each, block, self, int.MaxValue);
			}
			return Cycle(each, block, self, int.MaxValue);
		}

		[RubyMethod("cycle")]
		public static object Cycle(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam block, object self, [DefaultProtocol] int iterations)
		{
			if (iterations <= 0)
			{
				return null;
			}
			List<object> items = ((iterations > 1) ? new List<object>() : null);
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (block.Yield(item, out result))
				{
					iterations = -1;
					return selfBlock.PropagateFlow(block, result);
				}
				if (items != null)
				{
					items.Add(item);
				}
				return null;
			}));
			if (items == null)
			{
				return result;
			}
			while (iterations == int.MaxValue || --iterations > 0)
			{
				foreach (object item in items)
				{
					if (block.Yield(item, out result))
					{
						return result;
					}
				}
			}
			return result;
		}

		[RubyMethod("each_cons")]
		public static Enumerator GetEachConsEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam block, object self, [DefaultProtocol] int sliceSize)
		{
			return new Enumerator((RubyScope _, BlockParam innerBlock) => EachCons(each, innerBlock, self, sliceSize));
		}

		[RubyMethod("each_cons")]
		public static object EachCons(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam block, object self, [DefaultProtocol] int sliceSize)
		{
			return EachSlice(each, block, self, sliceSize, false, delegate(RubyArray slice)
			{
				RubyArray rubyArray = new RubyArray(slice.Count);
				for (int i = 1; i < slice.Count; i++)
				{
					rubyArray.Add(slice[i]);
				}
				return rubyArray;
			});
		}

		[RubyMethod("each_slice")]
		public static Enumerator GetEachSliceEnumerator(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam block, object self, [DefaultProtocol] int sliceSize)
		{
			return new Enumerator((RubyScope _, BlockParam innerBlock) => EachSlice(each, innerBlock, self, sliceSize));
		}

		[RubyMethod("each_slice")]
		public static object EachSlice(CallSiteStorage<Func<CallSite, object, Proc, object>> each, [NotNull] BlockParam block, object self, [DefaultProtocol] int sliceSize)
		{
			return EachSlice(each, block, self, sliceSize, true, (RubyArray slice) => null);
		}

		private static object EachSlice(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam block, object self, int sliceSize, bool includeIncomplete, Func<RubyArray, RubyArray> newSliceFactory)
		{
			if (sliceSize <= 0)
			{
				throw RubyExceptions.CreateArgumentError("invalid slice size");
			}
			RubyArray slice = null;
			object result = null;
			Each(each, self, Proc.Create(each.Context, delegate(BlockParam selfBlock, object _, object item)
			{
				if (slice == null)
				{
					slice = new RubyArray(sliceSize);
				}
				slice.Add(item);
				if (slice.Count == sliceSize)
				{
					RubyArray arg = slice;
					slice = newSliceFactory(slice);
					object blockResult2;
					if (block.Yield(arg, out blockResult2))
					{
						result = blockResult2;
						return selfBlock.PropagateFlow(block, blockResult2);
					}
				}
				return null;
			}));
			object blockResult;
			if (slice != null && includeIncomplete && block.Yield(slice, out blockResult))
			{
				return blockResult;
			}
			return result;
		}

		[RubyMethod("enum_cons")]
		public static Enumerator GetConsEnumerator(object self, [DefaultProtocol] int sliceSize)
		{
			return new Enumerator(self, "each_cons", sliceSize);
		}

		[RubyMethod("enum_slice")]
		public static Enumerator GetSliceEnumerator(object self, [DefaultProtocol] int sliceSize)
		{
			return new Enumerator(self, "each_slice", sliceSize);
		}

		[RubyMethod("enum_with_index")]
		public static Enumerator GetEnumeratorWithIndex(object self)
		{
			return new Enumerator(self, "each_with_index", null);
		}
	}
}
