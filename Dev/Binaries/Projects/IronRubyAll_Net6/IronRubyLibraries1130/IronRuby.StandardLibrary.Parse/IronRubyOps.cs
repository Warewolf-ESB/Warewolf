using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.ParseTree
{
	[RubyModule("IronRuby", Extends = typeof(Ruby))]
	public static class IronRubyOps
	{
		[RubyModule("ParseTree")]
		public static class ParseTreeOps
		{
			private enum NodeKind
			{
				Method,
				fbody,
				cfunc,
				scope,
				block,
				@if,
				@case,
				when,
				opt_n,
				@while,
				until,
				iter,
				@for,
				@break,
				next,
				redo,
				retry,
				begin,
				rescue,
				resbody,
				ensure,
				and,
				or,
				not,
				masgn,
				lasgn,
				dasgn,
				dasgn_curr,
				gasgn,
				iasgn,
				cdecl,
				cvasgn,
				cvdecl,
				op_asgn1,
				op_asgn2,
				op_asgn_and,
				op_asgn_or,
				call,
				fcall,
				vcall,
				super,
				zsuper,
				array,
				zarray,
				hash,
				@return,
				yield,
				lvar,
				dvar,
				gvar,
				ivar,
				@const,
				cvar,
				nth_ref,
				back_ref,
				match,
				match2,
				match3,
				lit,
				str,
				dstr,
				xstr,
				dxstr,
				evstr,
				dregx,
				dregx_once,
				args,
				argscat,
				argspush,
				splat,
				to_ary,
				svalue,
				block_arg,
				block_pass,
				defn,
				defs,
				alias,
				valias,
				undef,
				@class,
				module,
				sclass,
				colon2,
				colon3,
				cref,
				dot2,
				dot3,
				flip2,
				flip3,
				attrset,
				self,
				nil,
				@true,
				@false,
				defined,
				newline,
				postexe,
				alloca,
				dmethod,
				bmethod,
				memo,
				ifunc,
				dsym,
				attrasgn,
				last
			}

			private sealed class Rhs
			{
				public object Value { get; set; }

				public bool InBlockParameters { get; set; }

				public bool InCompoundLhs { get; set; }

				public bool InTopCompoundLhs { get; set; }

				public bool IsRhsArg { get; set; }
			}

			private sealed class AstVisitor : Walker
			{
				private static readonly Rhs BlockRhs = new Rhs
				{
					InBlockParameters = true
				};

				private static readonly object Skip = new object();

				private object _result;

				private RubyEncoding _encoding;

				private Rhs _rhs;

				private readonly RubyContext _context;

				private readonly RubyArray _nodeNames;

				private bool _isMethodAlias;

				public RubyArray Result
				{
					get
					{
						return (RubyArray)_result;
					}
				}

				public AstVisitor(RubyContext context, RubyArray nodeNames, bool isMethodAlias)
				{
					_nodeNames = nodeNames;
					_isMethodAlias = isMethodAlias;
					_context = context;
				}

				private RubySymbol CreateSymbol(string identifier)
				{
					return _context.CreateSymbol(identifier, _encoding);
				}

				private object GetNodeName(NodeKind nodeKind)
				{
					if ((int)nodeKind >= _nodeNames.Count)
					{
						return null;
					}
					return _nodeNames[(int)nodeKind];
				}

				private RubyArray MakeNode(NodeKind nodeKind, int valueCount)
				{
					RubyArray rubyArray = new RubyArray(1 + valueCount);
					rubyArray.Add(GetNodeName(nodeKind));
					return rubyArray;
				}

				private RubyArray MakeNode(NodeKind nodeKind)
				{
					return MakeNode(nodeKind, 0);
				}

				private RubyArray MakeNode(NodeKind nodeKind, object value1)
				{
					RubyArray rubyArray = MakeNode(nodeKind, 1);
					rubyArray.Add(value1);
					return rubyArray;
				}

				private RubyArray MakeNode(NodeKind nodeKind, object value1, object value2)
				{
					RubyArray rubyArray = MakeNode(nodeKind, 2);
					rubyArray.Add(value1);
					rubyArray.Add(value2);
					return rubyArray;
				}

				private RubyArray MakeNode(NodeKind nodeKind, object value1, object value2, object value3)
				{
					RubyArray rubyArray = MakeNode(nodeKind, 3);
					rubyArray.Add(value1);
					rubyArray.Add(value2);
					rubyArray.Add(value3);
					return rubyArray;
				}

				private bool TryGetRhsValue(out object value)
				{
					if (_rhs != null && !_rhs.InBlockParameters && !_rhs.InCompoundLhs)
					{
						value = _rhs.Value;
						return true;
					}
					value = null;
					return false;
				}

				private RubyArray AddRange<T>(RubyArray list, IEnumerable<T> nodes) where T : Node
				{
					if (nodes != null)
					{
						foreach (T node in nodes)
						{
							Walk(node);
							if (_result != Skip)
							{
								list.Add(_result);
							}
						}
						return list;
					}
					return list;
				}

				private void UsingRhs(Rhs rhs, Action region)
				{
					Rhs rhs2 = _rhs;
					_rhs = rhs;
					region();
					_rhs = rhs2;
				}

				public override bool Enter(SourceUnitTree node)
				{
					_encoding = node.Encoding;
					if (node.Statements == null || node.Statements.Count == 0)
					{
						_result = new RubyArray();
					}
					else if (node.Statements.Count == 1)
					{
						Walk(node.Statements.First);
						_result = RubyArray.Create(_result);
					}
					else
					{
						_result = RubyArray.Create(AddRange(MakeNode(NodeKind.block, node.Statements.Count), node.Statements));
					}
					return false;
				}

				public override bool Enter(Literal node)
				{
					if (node.Value == null)
					{
						_result = MakeNode(NodeKind.nil);
					}
					else if (node.Value is bool)
					{
						_result = MakeNode(((bool)node.Value) ? NodeKind.@true : NodeKind.@false);
					}
					else
					{
						_result = MakeNode(NodeKind.lit, node.Value);
					}
					return false;
				}

				private bool Enter(RangeExpression node, bool isCondition)
				{
					Literal literal = node.Begin as Literal;
					Literal literal2 = node.End as Literal;
					if (!isCondition && literal != null && literal2 != null && literal.Value is int && literal.Value is int)
					{
						_result = MakeNode(NodeKind.lit, new IronRuby.Builtins.Range((int)literal.Value, (int)literal2.Value, node.IsExclusive));
					}
					else
					{
						RubyArray rubyArray = MakeNode((!isCondition) ? (node.IsExclusive ? NodeKind.dot3 : NodeKind.dot2) : (node.IsExclusive ? NodeKind.flip3 : NodeKind.flip2), 2);
						Walk(node.Begin);
						rubyArray.Add(_result);
						Walk(node.End);
						rubyArray.Add(_result);
						_result = rubyArray;
					}
					return false;
				}

				public override bool Enter(RangeExpression node)
				{
					return Enter(node, false);
				}

				public override bool Enter(RangeCondition node)
				{
					return Enter(node.Range, true);
				}

				public override bool Enter(StringLiteral node)
				{
					_result = MakeNode(NodeKind.str, node.GetMutableString());
					return false;
				}

				public override bool Enter(SymbolLiteral node)
				{
					_result = MakeNode(NodeKind.lit, _context.CreateSymbol(node.GetMutableString()));
					return false;
				}

				public override bool Enter(FileLiteral node)
				{
					_result = MakeNode(NodeKind.lit, CreateSymbol("__FILE__"));
					return false;
				}

				public override bool Enter(StringConstructor node)
				{
					StringLiteral stringLiteral;
					if (node.Parts.Count == 1 && (stringLiteral = node.Parts[0] as StringLiteral) != null)
					{
						NodeKind nodeKind;
						object value;
						switch (node.Kind)
						{
						case StringKind.Symbol:
							nodeKind = NodeKind.lit;
							value = _context.CreateSymbol(stringLiteral.GetMutableString());
							break;
						case StringKind.Command:
							nodeKind = NodeKind.xstr;
							value = stringLiteral.GetMutableString();
							break;
						case StringKind.Mutable:
							nodeKind = NodeKind.str;
							value = stringLiteral.GetMutableString();
							break;
						default:
							throw Assert.Unreachable;
						}
						_result = MakeNode(nodeKind, value);
					}
					else
					{
						NodeKind kind;
						switch (node.Kind)
						{
						case StringKind.Command:
							kind = NodeKind.dxstr;
							break;
						case StringKind.Symbol:
							kind = NodeKind.dsym;
							break;
						case StringKind.Mutable:
							kind = NodeKind.dstr;
							break;
						default:
							throw Assert.Unreachable;
						}
						_result = VisitStringConstructor(node.Parts, kind);
					}
					return false;
				}

				private RubyArray VisitStringConstructor(List<Expression> parts, NodeKind kind)
				{
					RubyArray rubyArray = MakeNode(kind, parts.Count);
					if (parts.Count == 1)
					{
						rubyArray.Add(MutableString.FrozenEmpty);
					}
					for (int i = 0; i < parts.Count; i++)
					{
						Expression expression = parts[i];
						StringLiteral stringLiteral = expression as StringLiteral;
						if (stringLiteral != null)
						{
							object obj = stringLiteral.GetMutableString();
							if (i > 0)
							{
								obj = MakeNode(NodeKind.str, obj);
							}
							rubyArray.Add(obj);
						}
						else
						{
							Walk(expression);
							rubyArray.Add(MakeNode(NodeKind.evstr, _result));
						}
					}
					return rubyArray;
				}

				public override bool Enter(RegularExpression node)
				{
					StringLiteral stringLiteral;
					if (node.Pattern.Count == 0)
					{
						_result = MakeNode(NodeKind.lit, new RubyRegex(MutableString.CreateEmpty(), node.Options));
					}
					else if (node.Pattern.Count == 1 && (stringLiteral = node.Pattern[0] as StringLiteral) != null)
					{
						_result = MakeNode(NodeKind.lit, new RubyRegex(stringLiteral.GetMutableString(), node.Options));
					}
					else
					{
						RubyArray rubyArray = VisitStringConstructor(node.Pattern, NodeKind.dregx);
						if (node.Options != 0)
						{
							rubyArray.Add((int)node.Options);
						}
						_result = rubyArray;
					}
					return false;
				}

				public override bool Enter(RegularExpressionCondition node)
				{
					Walk(node.RegularExpression);
					_result = MakeNode(NodeKind.match, _result);
					return false;
				}

				public override bool Enter(MatchExpression node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.match2, 2);
					Walk(node.Regex);
					rubyArray.Add(_result);
					Walk(node.Expression);
					rubyArray.Add(_result);
					_result = rubyArray;
					return false;
				}

				public override bool Enter(HashConstructor node)
				{
					_result = MakeHash(node.Maplets);
					return false;
				}

				private RubyArray MakeHash(IList<Maplet> maplets)
				{
					RubyArray rubyArray = MakeNode(NodeKind.hash, maplets.Count * 2);
					foreach (Maplet maplet in maplets)
					{
						Walk(maplet.Key);
						rubyArray.Add(_result);
						Walk(maplet.Value);
						rubyArray.Add(_result);
					}
					return rubyArray;
				}

				public override bool Enter(ArrayConstructor node)
				{
					if (node.Arguments.IsEmpty)
					{
						_result = MakeNode(NodeKind.zarray);
					}
					else
					{
						Walk(node.Arguments);
						if (_result == Skip)
						{
							_result = MakeNode(NodeKind.zarray);
						}
					}
					return false;
				}

				public override bool Enter(MethodCall node)
				{
					RubyArray rubyArray = ((node.Target != null) ? MakeNode(NodeKind.call, 2 + SizeOf(node.Arguments)) : ((node.Arguments == null) ? MakeNode(NodeKind.vcall, 1) : MakeNode(NodeKind.fcall, 1 + SizeOf(node.Arguments))));
					if (node.Target != null)
					{
						Walk(node.Target);
						rubyArray.Add(_result);
					}
					rubyArray.Add(CreateSymbol(node.MethodName));
					AddArguments(rubyArray, node.Arguments);
					_result = MakeCallWithBlock(node.Block, rubyArray);
					return false;
				}

				public override bool Enter(SuperCall node)
				{
					RubyArray rubyArray;
					if (node.Arguments != null)
					{
						rubyArray = MakeNode(NodeKind.super, SizeOf(node.Arguments));
						AddArguments(rubyArray, node.Arguments);
					}
					else
					{
						rubyArray = MakeNode(NodeKind.zsuper);
					}
					_result = MakeCallWithBlock(node.Block, rubyArray);
					return false;
				}

				public override bool Enter(YieldCall node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.yield, SizeOf(node.Arguments));
					AddArguments(rubyArray, node.Arguments);
					_result = rubyArray;
					return false;
				}

				private static int SizeOf(Arguments args)
				{
					if (args == null || args.IsEmpty)
					{
						return 0;
					}
					return 1;
				}

				private void AddArguments(RubyArray list, Arguments args)
				{
					if (args != null && !args.IsEmpty)
					{
						Walk(args);
						if (_result != Skip)
						{
							list.Add(_result);
						}
					}
				}

				public override bool Enter(Arguments node)
				{
					throw new NotSupportedException("TODO: argument splatting");
				}

				private RubyArray MakeSplatArguments(RubyArray exprs, Expression splattedValue)
				{
					RubyArray rubyArray;
					if (exprs != null)
					{
						rubyArray = MakeNode(NodeKind.argscat, 2);
						rubyArray.Add(exprs);
					}
					else
					{
						rubyArray = MakeNode(NodeKind.splat, 1);
					}
					Walk(splattedValue);
					rubyArray.Add(_result);
					return rubyArray;
				}

				private RubyArray VisitExpressionsAndMaplets(Arguments node)
				{
					if (!node.IsEmpty)
					{
						RubyArray rubyArray = MakeNode(NodeKind.array, node.Expressions.Length);
						AddRange(rubyArray, node.Expressions);
						object value;
						if (TryGetRhsValue(out value))
						{
							rubyArray.Add(value);
						}
						return rubyArray;
					}
					return null;
				}

				public override bool Enter(Maplet node)
				{
					throw Assert.Unreachable;
				}

				private RubyArray MakeCallWithBlock(Block block, RubyArray call)
				{
					if (block != null)
					{
						BlockReference blockReference = block as BlockReference;
						if (blockReference != null)
						{
							RubyArray rubyArray = MakeNode(NodeKind.block_pass, 2);
							Walk(blockReference.Expression);
							rubyArray.Add(_result);
							rubyArray.Add(call);
							return rubyArray;
						}
						BlockDefinition blockDef = (BlockDefinition)block;
						RubyArray result = MakeNode(NodeKind.iter, 3);
						result.Add(call);
						UsingRhs(BlockRhs, delegate
						{
							Walk(blockDef.Parameters);
							result.Add(_result);
						});
						AddRange(result, blockDef.Body);
						return result;
					}
					return call;
				}

				public override bool Enter(BlockReference node)
				{
					throw Assert.Unreachable;
				}

				public override bool Enter(BlockDefinition node)
				{
					throw Assert.Unreachable;
				}

				public bool EnterVariable(string name, NodeKind read, NodeKind write)
				{
					RubySymbol value = CreateSymbol(name);
					RubyArray rubyArray = (RubyArray)(_result = ((_rhs == null || _rhs.IsRhsArg) ? MakeNode(read, value) : (_rhs.InBlockParameters ? MakeNode((write == NodeKind.lasgn) ? NodeKind.dasgn_curr : write, value) : ((!_rhs.InCompoundLhs) ? MakeNode(write, value, _rhs.Value) : MakeNode(write, value)))));
					return false;
				}

				public override bool Enter(ClassVariable node)
				{
					return EnterVariable(node.Name, NodeKind.cvar, NodeKind.cvdecl);
				}

				public override bool Enter(ConstantVariable node)
				{
					if (node.IsGlobal)
					{
						return EnterVariable(node.Name, NodeKind.colon3, NodeKind.cdecl);
					}
					if (node.IsBound)
					{
						RubyArray rubyArray = MakeNode(NodeKind.colon2, 2);
						Walk(node.Qualifier);
						rubyArray.Add(_result);
						rubyArray.Add(CreateSymbol(node.Name));
						_result = rubyArray;
						return false;
					}
					return EnterVariable(node.Name, NodeKind.@const, NodeKind.cdecl);
				}

				public override bool Enter(IronRuby.Compiler.Ast.GlobalVariable node)
				{
					return EnterVariable(node.FullName, NodeKind.gvar, NodeKind.gasgn);
				}

				public override bool Enter(InstanceVariable node)
				{
					return EnterVariable(node.Name, NodeKind.ivar, NodeKind.iasgn);
				}

				public override bool Enter(LocalVariable node)
				{
					return EnterVariable(node.Name, NodeKind.lvar, NodeKind.lasgn);
				}

				public override bool Enter(RegexMatchReference node)
				{
					if (node.FullName == "$~")
					{
						return EnterVariable(node.FullName, NodeKind.gvar, NodeKind.gasgn);
					}
					if (node.Index > 0)
					{
						_result = MakeNode(NodeKind.nth_ref, ScriptingRuntimeHelpers.Int32ToObject(node.Index));
					}
					else
					{
						_result = MakeNode(NodeKind.back_ref, CreateSymbol(node.VariableName));
					}
					return false;
				}

				public override bool Enter(Placeholder node)
				{
					_result = Skip;
					return false;
				}

				public override bool Enter(SimpleAssignmentExpression node)
				{
					bool flag = node.Operation == "&&";
					bool flag2 = node.Operation == "||";
					Rhs rhs = _rhs;
					_rhs = null;
					Walk(node.Right);
					object obj = _result;
					if (node.Operation != null && !flag && !flag2)
					{
						Walk(node.Left);
						obj = MakeNode(NodeKind.call, _result, CreateSymbol(node.Operation), MakeNode(NodeKind.array, obj));
					}
					_rhs = new Rhs
					{
						Value = obj
					};
					Walk(node.Left);
					if (flag || flag2)
					{
						object result = _result;
						_rhs = null;
						Walk(node.Left);
						_result = MakeNode(flag ? NodeKind.op_asgn_and : NodeKind.op_asgn_or, _result, result);
					}
					_rhs = rhs;
					return false;
				}

				public override bool Enter(MemberAssignmentExpression node)
				{
					throw new NotImplementedException(node.NodeType.ToString());
				}

				public override bool Enter(ParallelAssignmentExpression node)
				{
					throw new NotSupportedException("TODO: parallel assignment");
				}

				public override bool Enter(ArrayItemAccess node)
				{
					if (_rhs == null)
					{
						RubyArray rubyArray = MakeNode(NodeKind.call, 2 + SizeOf(node.Arguments));
						Walk(node.Array);
						rubyArray.Add(_result);
						rubyArray.Add(CreateSymbol("[]"));
						AddArguments(rubyArray, node.Arguments);
						_result = rubyArray;
					}
					else
					{
						bool isRhsArg = _rhs.IsRhsArg;
						_rhs.IsRhsArg = true;
						RubyArray assignment = MakeNode(NodeKind.attrasgn, 2 + SizeOf(node.Arguments));
						UsingRhs(null, delegate
						{
							Walk(node.Array);
							assignment.Add(_result);
						});
						assignment.Add(CreateSymbol("[]="));
						AddArguments(assignment, node.Arguments);
						_rhs.IsRhsArg = isRhsArg;
						_result = assignment;
					}
					return false;
				}

				public override bool Enter(AttributeAccess node)
				{
					RubyArray assignment = MakeNode(NodeKind.attrasgn, 3);
					UsingRhs(null, delegate
					{
						Walk(node.Qualifier);
						assignment.Add(_result);
					});
					assignment.Add(CreateSymbol(node.Name));
					object value;
					if (TryGetRhsValue(out value))
					{
						assignment.Add(MakeNode(NodeKind.array, value));
					}
					_result = assignment;
					return false;
				}

				public override bool Enter(AssignmentExpression node)
				{
					throw Assert.Unreachable;
				}

				public override bool Enter(CompoundLeftValue node)
				{
					if (node.UnsplattedValue == null)
					{
						if (node.LeftValues.Length == 0)
						{
							_result = ScriptingRuntimeHelpers.Int32ToObject(0);
							return false;
						}
						if (node.LeftValues.Length == 1)
						{
							Walk(node.LeftValues[0]);
							return false;
						}
					}
					bool inTopCompoundLhs = _rhs.InTopCompoundLhs;
					_rhs.InTopCompoundLhs = false;
					RubyArray rubyArray = MakeNode(NodeKind.masgn, ((node.LeftValues.Length > 1) ? 1 : 0) + ((node.UnsplattedValue != null) ? 1 : 0) + ((_rhs != null) ? 1 : 0) + (inTopCompoundLhs ? 1 : 0));
					if (node.LeftValues.Length > 1)
					{
						rubyArray.Add(AddRange(MakeNode(NodeKind.array, node.LeftValues.Length), node.LeftValues));
					}
					if (node.UnsplattedValue != null)
					{
						if (node.UnsplattedValue is Placeholder)
						{
							rubyArray.Add(MakeNode(NodeKind.splat));
						}
						else
						{
							Walk(node.UnsplattedValue);
							rubyArray.Add(_result);
						}
					}
					if (_rhs.InCompoundLhs && inTopCompoundLhs)
					{
						rubyArray.Add(_rhs.Value);
					}
					_rhs.InTopCompoundLhs = inTopCompoundLhs;
					_result = rubyArray;
					return false;
				}

				public override bool Enter(AliasStatement node)
				{
					if (node.IsGlobalVariableAlias)
					{
						_result = MakeNode(NodeKind.valias, CreateSymbol("$" + node.NewName), CreateSymbol("$" + node.OldName));
					}
					else
					{
						_result = MakeNode(NodeKind.alias, MakeNode(NodeKind.lit, CreateSymbol((string)node.NewName.Value)), MakeNode(NodeKind.lit, CreateSymbol((string)node.OldName.Value)));
					}
					return false;
				}

				public override bool Enter(UndefineStatement node)
				{
					if (node.Items.Count == 1)
					{
						_result = MakeNode(NodeKind.undef, MakeNode(NodeKind.lit, CreateSymbol((string)node.Items[0].Value)));
					}
					else
					{
						RubyArray rubyArray = MakeNode(NodeKind.block, node.Items.Count);
						foreach (ConstructedSymbol item in node.Items)
						{
							rubyArray.Add(MakeNode(NodeKind.undef, MakeNode(NodeKind.lit, CreateSymbol((string)item.Value))));
						}
						_result = rubyArray;
					}
					return false;
				}

				public override bool Enter(IsDefinedExpression node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.defined, 1);
					Walk(node.Expression);
					rubyArray.Add(_result);
					_result = rubyArray;
					return false;
				}

				public override bool Enter(SelfReference node)
				{
					_result = MakeNode(NodeKind.self);
					return false;
				}

				public override bool Enter(FileInitializerStatement node)
				{
					throw new NotImplementedException();
				}

				public override bool Enter(ShutdownHandlerStatement node)
				{
					throw new NotImplementedException();
				}

				private bool EnterBooleanExpression(Expression left, Expression right, NodeKind kind)
				{
					RubyArray rubyArray = MakeNode(kind, 2);
					Walk(left);
					rubyArray.Add(_result);
					Walk(right);
					rubyArray.Add(_result);
					_result = rubyArray;
					return false;
				}

				public override bool Enter(AndExpression node)
				{
					return EnterBooleanExpression(node.Left, node.Right, NodeKind.and);
				}

				public override bool Enter(OrExpression node)
				{
					return EnterBooleanExpression(node.Left, node.Right, NodeKind.or);
				}

				public override bool Enter(NotExpression node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.not, 1);
					Walk(node.Expression);
					rubyArray.Add(_result);
					_result = rubyArray;
					return false;
				}

				private bool EnterJumpStatement(JumpStatement node, NodeKind kind)
				{
					RubyArray rubyArray = MakeNode(kind, SizeOf(node.Arguments));
					AddArguments(rubyArray, node.Arguments);
					_result = rubyArray;
					return false;
				}

				public override bool Enter(BreakStatement node)
				{
					return EnterJumpStatement(node, NodeKind.@break);
				}

				public override bool Enter(NextStatement node)
				{
					return EnterJumpStatement(node, NodeKind.next);
				}

				public override bool Enter(ReturnStatement node)
				{
					return EnterJumpStatement(node, NodeKind.@return);
				}

				public override bool Enter(RetryStatement node)
				{
					return EnterJumpStatement(node, NodeKind.retry);
				}

				public override bool Enter(RedoStatement node)
				{
					return EnterJumpStatement(node, NodeKind.redo);
				}

				public override bool Enter(BlockExpression node)
				{
					_result = MakeBlock(node.Statements);
					return false;
				}

				public override bool Enter(Body node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.begin, 5);
					AddBody(rubyArray, node);
					_result = rubyArray;
					return false;
				}

				private RubyArray AddBody(RubyArray list, Body node)
				{
					RubyArray rubyArray;
					if (node.EnsureStatements != null)
					{
						list.Add(rubyArray = MakeNode(NodeKind.ensure, 2));
					}
					else
					{
						rubyArray = list;
					}
					if (node.RescueClauses != null)
					{
						RubyArray rubyArray2 = MakeNode(NodeKind.rescue);
						rubyArray2.Add(MakeBlock(node.Statements));
						AddRescueBody(rubyArray2, node);
						AddRange(rubyArray2, node.ElseStatements);
						rubyArray.Add(rubyArray2);
					}
					else
					{
						rubyArray.Add(MakeBlock(node.Statements));
					}
					AddRange(rubyArray, node.EnsureStatements);
					return list;
				}

				private void AddRescueBody(RubyArray current, Body node)
				{
					RescueClause clause;
					foreach (RescueClause rescueClause in node.RescueClauses)
					{
						clause = rescueClause;
						RubyArray rubyArray = MakeNode(NodeKind.resbody, 3);
						if (clause.Types != null)
						{
							rubyArray.Add(AddRange(MakeNode(NodeKind.array, clause.Types.Length), clause.Types));
						}
						else
						{
							rubyArray.Add(null);
						}
						if (clause.Target != null)
						{
							UsingRhs(new Rhs
							{
								Value = MakeNode(NodeKind.gvar, CreateSymbol("$!"))
							}, delegate
							{
								Walk(clause.Target);
							});
							object result = _result;
							RubyArray rubyArray2 = MakeNode(NodeKind.block, 1 + ((clause.Statements != null) ? clause.Statements.Count : 0));
							rubyArray2.Add(result);
							AddRange(rubyArray2, clause.Statements);
							rubyArray.Add(rubyArray2);
						}
						else
						{
							AddRange(rubyArray, clause.Statements);
						}
						current.Add(rubyArray);
						current = rubyArray;
					}
				}

				public override bool Enter(RescueClause node)
				{
					throw Assert.Unreachable;
				}

				public override bool Enter(RescueExpression node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.rescue, 2);
					Walk(node.GuardedExpression);
					rubyArray.Add(_result);
					RubyArray rubyArray2 = MakeNode(NodeKind.resbody, 2);
					rubyArray2.Add(null);
					Walk(node.RescueClauseStatement);
					rubyArray2.Add(_result);
					rubyArray.Add(rubyArray2);
					_result = rubyArray;
					return false;
				}

				private object MakeBlock(Statements statements)
				{
					object obj = MakeBlockOpt(statements);
					if (obj == Skip)
					{
						return MakeNode(NodeKind.nil);
					}
					return obj;
				}

				private RubyArray AddBlock(RubyArray list, Statements statements)
				{
					object obj = MakeBlockOpt(statements);
					if (obj != Skip)
					{
						list.Add(obj);
					}
					return list;
				}

				private object MakeBlockOpt(Statements statements)
				{
					if (statements == null || statements.Count == 0)
					{
						return Skip;
					}
					if (statements.Count == 1)
					{
						Walk(statements.First);
						return _result;
					}
					return AddRange(MakeNode(NodeKind.block, statements.Count), statements);
				}

				public override bool Enter(IfExpression node)
				{
					RubyArray result;
					RubyArray rubyArray = (result = MakeNode(NodeKind.@if, 3));
					Walk(node.Condition);
					rubyArray.Add(_result);
					rubyArray.Add(MakeBlock(node.Body));
					if (node.ElseIfClauses != null && node.ElseIfClauses.Count != 0)
					{
						foreach (ElseIfClause elseIfClause in node.ElseIfClauses)
						{
							if (elseIfClause.Condition != null)
							{
								rubyArray.Add(rubyArray = MakeNode(NodeKind.@if, 3));
								Walk(elseIfClause.Condition);
								rubyArray.Add(_result);
							}
							rubyArray.Add(MakeBlock(elseIfClause.Statements));
						}
						if (node.ElseIfClauses[node.ElseIfClauses.Count - 1].Condition != null)
						{
							rubyArray.Add(null);
						}
					}
					else
					{
						rubyArray.Add(null);
					}
					_result = result;
					return false;
				}

				public override bool Enter(UnlessExpression node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.@if, 3);
					Walk(node.Condition);
					rubyArray.Add(_result);
					if (node.ElseClause != null)
					{
						rubyArray.Add(MakeBlock(node.ElseClause.Statements));
					}
					else
					{
						rubyArray.Add(null);
					}
					rubyArray.Add(MakeBlock(node.Statements));
					_result = rubyArray;
					return false;
				}

				public override bool Enter(ElseIfClause node)
				{
					throw Assert.Unreachable;
				}

				private bool EnterTernary(NodeKind kind, Expression expr1, Expression expr2, Expression expr3)
				{
					RubyArray rubyArray = MakeNode(NodeKind.@if, 3);
					Walk(expr1);
					rubyArray.Add(_result);
					if (expr2 != null)
					{
						Walk(expr2);
						rubyArray.Add(_result);
					}
					else
					{
						rubyArray.Add(null);
					}
					if (expr3 != null)
					{
						Walk(expr3);
						rubyArray.Add(_result);
					}
					else
					{
						rubyArray.Add(null);
					}
					_result = rubyArray;
					return false;
				}

				public override bool Enter(ConditionalExpression node)
				{
					return EnterTernary(NodeKind.@if, node.Condition, node.TrueExpression, node.FalseExpression);
				}

				public override bool Enter(ConditionalJumpExpression node)
				{
					if (node.IsBooleanExpression)
					{
						if (node.NegateCondition)
						{
							return EnterBooleanExpression(node.Condition, node.JumpStatement, NodeKind.or);
						}
						return EnterBooleanExpression(node.Condition, node.JumpStatement, NodeKind.and);
					}
					if (node.NegateCondition)
					{
						return EnterTernary(NodeKind.@if, node.Condition, node.Value, node.JumpStatement);
					}
					return EnterTernary(NodeKind.@if, node.Condition, node.JumpStatement, node.Value);
				}

				public override bool Enter(ConditionalStatement node)
				{
					if (node.IsUnless)
					{
						return EnterTernary(NodeKind.@if, node.Condition, node.ElseStatement, node.Body);
					}
					return EnterTernary(NodeKind.@if, node.Condition, node.Body, node.ElseStatement);
				}

				public override bool Enter(WhileLoopExpression node)
				{
					RubyArray rubyArray = MakeNode(node.IsWhileLoop ? NodeKind.@while : NodeKind.until);
					Walk(node.Condition);
					rubyArray.Add(_result);
					rubyArray.Add(MakeBlock(node.Statements));
					rubyArray.Add(!node.IsPostTest);
					_result = rubyArray;
					return false;
				}

				public override bool Enter(ForLoopExpression node)
				{
					RubyArray loop = MakeNode(NodeKind.@for, 3);
					Walk(node.List);
					loop.Add(_result);
					UsingRhs(new Rhs
					{
						InCompoundLhs = true,
						InTopCompoundLhs = false
					}, delegate
					{
						Walk(node.Block.Parameters);
						loop.Add(_result);
						AddBlock(loop, node.Block.Body);
					});
					_result = loop;
					return false;
				}

				public override bool Enter(CaseExpression node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.@case, 1 + node.WhenClauses.Length + 1);
					if (node.Value != null)
					{
						Walk(node.Value);
						rubyArray.Add(_result);
					}
					else
					{
						rubyArray.Add(null);
					}
					if (node.WhenClauses != null)
					{
						WhenClause[] whenClauses = node.WhenClauses;
						foreach (WhenClause whenClause in whenClauses)
						{
							RubyArray rubyArray2 = MakeNode(NodeKind.when, 2);
							RubyArray rubyArray3 = MakeNode(NodeKind.array, whenClause.Comparisons.Length);
							AddRange(rubyArray3, whenClause.Comparisons);
							rubyArray2.Add(rubyArray3);
							rubyArray2.Add(MakeBlock(whenClause.Statements));
							rubyArray.Add(rubyArray2);
						}
					}
					rubyArray.Add(MakeBlock(node.ElseStatements));
					_result = rubyArray;
					return false;
				}

				public override bool Enter(WhenClause node)
				{
					throw Assert.Unreachable;
				}

				private void AddScope(RubyArray list, DefinitionExpression node)
				{
					list.Add(AddBody(MakeNode(NodeKind.scope), node.Body));
				}

				public override bool Enter(ModuleDefinition node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.module, 2);
					Walk(node.QualifiedName);
					rubyArray.Add(_result);
					AddScope(rubyArray, node);
					_result = rubyArray;
					return false;
				}

				public override bool Enter(ClassDefinition node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.@class, 3);
					Walk(node.QualifiedName);
					rubyArray.Add(_result);
					if (node.SuperClass != null)
					{
						Walk(node.SuperClass);
						rubyArray.Add(_result);
					}
					else
					{
						rubyArray.Add(null);
					}
					AddScope(rubyArray, node);
					_result = rubyArray;
					return false;
				}

				public override bool Enter(SingletonDefinition node)
				{
					RubyArray rubyArray = MakeNode(NodeKind.sclass, 2);
					Walk(node.Singleton);
					rubyArray.Add(_result);
					AddScope(rubyArray, node);
					_result = rubyArray;
					return false;
				}

				public override bool Enter(MethodDefinition node)
				{
					bool isMethodAlias = _isMethodAlias;
					_isMethodAlias = false;
					RubyArray rubyArray;
					if (node.Target != null)
					{
						rubyArray = MakeNode(NodeKind.defs, 3);
						Walk(node.Target);
						rubyArray.Add(_result);
					}
					else
					{
						rubyArray = MakeNode(NodeKind.defn, 2);
					}
					rubyArray.Add(CreateSymbol(node.Name));
					RubyArray rubyArray2 = MakeNode(NodeKind.scope, 1);
					RubyArray rubyArray3 = MakeNode(NodeKind.block, 5);
					RubyArray rubyArray4 = MakeNode(NodeKind.args, node.Parameters.Mandatory.Length + node.Parameters.Optional.Length + ((node.Parameters.Unsplat != null) ? 1 : 0));
					if (node.Parameters.Mandatory.Length > 0)
					{
						throw new NotSupportedException("TODO: compound parameters");
					}
					SimpleAssignmentExpression[] optional = node.Parameters.Optional;
					foreach (SimpleAssignmentExpression simpleAssignmentExpression in optional)
					{
						rubyArray4.Add(CreateSymbol(((LocalVariable)simpleAssignmentExpression.Left).Name));
					}
					if (node.Parameters.Unsplat != null)
					{
						rubyArray4.Add(CreateSymbol("*" + ((LocalVariable)node.Parameters.Unsplat).Name));
					}
					if (node.Parameters.Optional.Length > 0)
					{
						RubyArray rubyArray5 = MakeNode(NodeKind.block);
						SimpleAssignmentExpression[] optional2 = node.Parameters.Optional;
						foreach (SimpleAssignmentExpression node2 in optional2)
						{
							Walk(node2);
							rubyArray5.Add(_result);
						}
						rubyArray4.Add(rubyArray5);
					}
					rubyArray3.Add(rubyArray4);
					if (node.Parameters.Block != null)
					{
						rubyArray3.Add(MakeNode(NodeKind.block_arg, CreateSymbol(node.Parameters.Block.Name)));
					}
					AddBody(rubyArray3, node.Body);
					rubyArray2.Add(rubyArray3);
					if (isMethodAlias)
					{
						rubyArray.Add(MakeNode(NodeKind.fbody, rubyArray2));
					}
					else
					{
						rubyArray.Add(rubyArray2);
					}
					_isMethodAlias = isMethodAlias;
					_result = rubyArray;
					return false;
				}

				public override bool Enter(Parameters node)
				{
					throw Assert.Unreachable;
				}
			}

			[RubyMethod("parse_tree_for_meth")]
			public static RubyArray CreateParseTreeForMethod(object self, [NotNull] RubyModule module, [DefaultProtocol][NotNull] string methodName, bool isClassMethod)
			{
				if (isClassMethod)
				{
					module = module.ImmediateClass;
				}
				RubyMemberInfo method = module.GetMethod(methodName);
				RubyMethodInfo rubyMethodInfo = method as RubyMethodInfo;
				if (rubyMethodInfo == null)
				{
					return RubyArray.Create(null);
				}
				AstVisitor astVisitor = new AstVisitor(module.Context, GetNodeNames(module.Context, self), false);
				astVisitor.Walk(rubyMethodInfo.GetSyntaxTree());
				return astVisitor.Result;
			}

			[RubyMethod("parse_tree_for_str")]
			public static RubyArray CreateParseTreeForString(RubyScope scope, object self, [NotNull] MutableString code, [Optional][NotNull] MutableString file, int line)
			{
				SourceUnit sourceUnit = scope.RubyContext.CreateSnippet(code.ConvertToString(), (file != null) ? file.ConvertToString() : null, SourceCodeKind.Statements);
				RubyCompilerOptions options = RubyUtils.CreateCompilerOptionsForEval(scope, line);
				SourceUnitTree node = new Parser().Parse(sourceUnit, options, scope.RubyContext.RuntimeErrorSink);
				AstVisitor astVisitor = new AstVisitor(scope.RubyContext, GetNodeNames(scope.RubyContext, self), false);
				astVisitor.Walk(node);
				return astVisitor.Result;
			}

			private static RubyArray GetNodeNames(RubyContext context, object self)
			{
				object value;
				context.GetClassOf(self).TryGetConstant(null, "NODE_NAMES", out value);
				return (value as RubyArray) ?? new RubyArray();
			}
		}
	}
}
