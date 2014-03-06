using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using Infragistics.Controls.Interactions.Primitives;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Diagnostics;

namespace Infragistics.Controls.Interactions
{
	internal static class FormulaEditorUtilities
	{
		#region Methods

		#region Public Methods

		#region AddLocalizedString

		public static void AddLocalizedString(Dictionary<string, string> localizedStrings, string name)
		{
			localizedStrings.Add(name, FormulaEditorUtilities.GetString(name));
		}

		#endregion // AddLocalizedString

		#region CreateRun

		public static Run CreateRun(string text, FontWeight? fontWeight = null)
		{
			Run run = new Run();
			run.Text = text;

			if (fontWeight.HasValue)
				run.FontWeight = fontWeight.Value;

			return run;
		}

		#endregion  // CreateRun

		#region DoesRequireReferenceIndexing

		public static bool DoesRequireReferenceIndexing(IFormulaProvider targetProvider, 
			OperandInfo operandBeingInserted, 
			List<FunctionInfo> functions, 
			string formulaText, 
			int insertionIndex)
		{
			if (targetProvider == null)
				return false;

			if (operandBeingInserted.Reference == null)
				return false;

			if (operandBeingInserted.Reference.IsEnumerable == false)
				return false;

			if (targetProvider.Reference == null ||
				targetProvider.Reference.IsEnumerable == false ||
				targetProvider.Reference.IsSiblingReference(operandBeingInserted.Reference) == false)
			{
				List<FormulaElement> formulaSegments = FormulaParser.Parse(formulaText);

				bool isTokenBeforeInsertionIndex = true;
				FormulaElement element = FormulaParser.FindToken(formulaSegments, insertionIndex - 1);
				if (element == null)
				{
					isTokenBeforeInsertionIndex = false;
					element = FormulaParser.FindToken(formulaSegments, insertionIndex);
				}

				if (element != null)
				{
					List<FormulaElement> ancestorChainUnderFunction;
					FormulaProduction functionProduction = FormulaEditorUtilities.GetOwningFunctionProduction(element, isTokenBeforeInsertionIndex, out ancestorChainUnderFunction);

					FunctionInfo functionInfo = FormulaEditorUtilities.GetFunctionInfo(
						functions,
						FormulaEditorUtilities.GetFuncId(functionProduction));

					if (functionInfo != null)
					{
						int argumentCount;
						int characterIndex = FormulaEditorUtilities.GetArgumentIndex(functionProduction, element, ancestorChainUnderFunction, out argumentCount);

						if (functionInfo.Function.CanParameterBeEnumerable(characterIndex))
							return false;
					}
				}

				return true;
			}

			return false;
		}

		#endregion  // DoesRequireReferenceIndexing

		#region GetArgumentIndex

		public static int GetArgumentIndex(FormulaProduction functionProduction, FormulaElement currentElement, List<FormulaElement> ancestorChainUnderFunction, out int argumentCount)
		{
			int currentArgumentIndex = 0;
			argumentCount = 0;

			if (functionProduction.Children.Count > 2)
			{
				FormulaProduction funcArgsProduction = functionProduction.Children[2] as FormulaProduction;
				if (funcArgsProduction != null)
				{
					argumentCount = 1;

					// Keep track of whether or not we are past the current argument. If the current element is the opening parenthesis for the function call,
					// we are already after the current argument.
					bool isAfterCurrentPosition = currentElement.IsLeftParen && currentElement == functionProduction.Children[1];

					for (int i = 0; i < funcArgsProduction.Children.Count; i++)
					{
						FormulaElement child = funcArgsProduction.Children[i];

						if (child.IsArgSep)
						{
							argumentCount++;
							if (isAfterCurrentPosition == false)
								currentArgumentIndex++;
						}

						if (ancestorChainUnderFunction.Contains(child))
							isAfterCurrentPosition = true;
					}
				}
			}

			return currentArgumentIndex;
		}

		#endregion  // GetArgumentIndex

		// MD 6/25/12 - TFS113177
		#region GetFullRefernceString

		private static string GetFullRefernceString(bool[] shouldIncludeIndexes, ICalculationReference relativeReference, string indexString)
		{
			if (indexString.Length == 0 || shouldIncludeIndexes == null)
				return relativeReference.AbsoluteName;

			RefParser relativeRefParser = new RefParser(relativeReference.AbsoluteName, true);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < relativeRefParser.TupleCount; i++)
			{
				if (i == 0)
					sb.Append("//");
				else
					sb.Append("/");

				sb.Append(relativeRefParser[i].Name);
				if (shouldIncludeIndexes[i])
					sb.Append(indexString);
			}

			return sb.ToString();
		}

		#endregion // GetFullRefernceString

		#region GetFuncId

		public static string GetFuncId(FormulaProduction functionProduction)
		{
			if (functionProduction == null)
				return null;

			Debug.Assert(functionProduction.IsFunction, "The function production was not a function.");

			if (functionProduction.Children.Count == 0)
				return null;

			FormulaElement funcId = functionProduction.Children[0];
			if (funcId.IsFuncId)
				return funcId.GetText();

			return null;
		}

		#endregion  // GetFuncId

		#region GetFunctionCategories

		public static FilteredCollection<FunctionCategory> GetFunctionCategories(List<FunctionInfo> functions)
		{
			if (functions == null)
				return null;

			List<FunctionInfo> functionsWithoutCategory = new List<FunctionInfo>();
			Dictionary<string, List<FunctionInfo>> functionCategories =
				new Dictionary<string, List<FunctionInfo>>(StringComparer.CurrentCultureIgnoreCase);

			foreach (FunctionInfo functionInfo in functions)
			{
				string category = functionInfo.Function.Category;

				List<FunctionInfo> functionsInCategory;
				if (String.IsNullOrEmpty(category))
				{
					functionsInCategory = functionsWithoutCategory;
				}
				else if (functionCategories.TryGetValue(category, out functionsInCategory) == false)
				{
					functionsInCategory = new List<FunctionInfo>();
					functionCategories[category] = functionsInCategory;
				}

				functionsInCategory.Add(functionInfo);
			}

			var sortedCategories =
				from pair in functionCategories
				orderby pair.Key
				select new FunctionCategory(
					pair.Key,
					pair.Value.OrderBy(functionInfo => functionInfo.Name).ToList());

			List<FunctionCategory> result = sortedCategories.ToList();

			if (functionsWithoutCategory.Count > 0)
			{
				FunctionCategory nullCateogry = new FunctionCategory(
					FormulaEditorUtilities.GetString("MissingCategoryName"),
					functionsWithoutCategory.OrderBy(functionInfo => functionInfo.Name).ToList());

				result.Insert(0, nullCateogry);
			}

			return FormulaEditorUtilities.CreateFilteredCollection(result);
		}

		#endregion  // GetFunctionCategories

		#region GetFunctionInfo

		public static FunctionInfo GetFunctionInfo(List<FunctionInfo> functions, string functionName)
		{
			if (functions != null && functionName != null)
			{
				for (int i = 0; i < functions.Count; i++)
				{
					FunctionInfo currentFunction = functions[i];
					if (String.Equals(currentFunction.Name, functionName, StringComparison.CurrentCultureIgnoreCase))
						return currentFunction;
				}
			}

			return null;
		}

		#endregion // GetFunctionInfo

		#region GetFunctionSignature

		public static string GetFunctionSignature(CalculationFunction function, int argumentCount = -1)
		{
			if (function == null)
				return null;

			StringBuilder sb = new StringBuilder("(");

			bool endsInArgumentSeparator = false;

			const string ArgumentSeparator = ", ";
			for (int i = 0; i < function.MinArgs; i++)
			{
				sb.Append(FormulaEditorUtilities.GetArgumentName(function, i));
				sb.Append(ArgumentSeparator);
				endsInArgumentSeparator = true;
			}

			if (function.HasUnlimitedArguments())
			{
				if (argumentCount < 0)
				{
					endsInArgumentSeparator = false;
					sb.Append(FormulaEditorUtilities.GetArgumentName(function, function.MinArgs));
					sb.Append(ArgumentSeparator);
					sb.Append("…");
				}
				else
				{
					for (int i = function.MinArgs; i < argumentCount; i++)
					{
						sb.Append(FormulaEditorUtilities.GetArgumentName(function, i));
						sb.Append(ArgumentSeparator);
						endsInArgumentSeparator = true;
					}
				}
			}
			else
			{
				bool showOptionalIndicators = argumentCount < 0;
				int maxArgument = showOptionalIndicators ? function.MaxArgs : Math.Min(argumentCount, function.MaxArgs);

				for (int i = function.MinArgs; i < maxArgument; i++)
				{
					if (showOptionalIndicators)
						sb.Append("[");

					sb.Append(FormulaEditorUtilities.GetArgumentName(function, i));

					if (showOptionalIndicators)
						sb.Append("]");

					sb.Append(ArgumentSeparator);
					endsInArgumentSeparator = true;
				}
			}

			if (endsInArgumentSeparator)
				sb.Remove(sb.Length - ArgumentSeparator.Length, ArgumentSeparator.Length);

			sb.Append(")");

			return sb.ToString();
		}

		public static List<Inline> GetFunctionSignature(CalculationFunction function, int argumentCount, int currentArgumentIndex, out Span argumentDescription)
		{
			argumentDescription = null;

			List<Inline> signatureInlines = new List<Inline>();

			if (function == null)
				return signatureInlines;

			signatureInlines.Add(FormulaEditorUtilities.CreateRun("("));

			bool endsInArgumentSeparator = false;

			const string ArgumentSeparator = ", ";
			for (int i = 0; i < function.MinArgs; i++)
			{
				FormulaEditorUtilities.AddArgumentToSignature(signatureInlines, function, i, currentArgumentIndex, ref argumentDescription);

				signatureInlines.Add(FormulaEditorUtilities.CreateRun(ArgumentSeparator));
				endsInArgumentSeparator = true;
			}

			if (function.HasUnlimitedArguments())
			{
				endsInArgumentSeparator = false;
				for (int i = function.MinArgs; i < argumentCount; i++)
				{
					FormulaEditorUtilities.AddArgumentToSignature(signatureInlines, function, i, currentArgumentIndex, ref argumentDescription);
					signatureInlines.Add(FormulaEditorUtilities.CreateRun(ArgumentSeparator));					
				}

				signatureInlines.Add(FormulaEditorUtilities.CreateRun("…"));
			}
			else
			{
				for (int i = function.MinArgs; i < function.MaxArgs; i++)
				{
					signatureInlines.Add(FormulaEditorUtilities.CreateRun("["));
					FormulaEditorUtilities.AddArgumentToSignature(signatureInlines, function, i, currentArgumentIndex, ref argumentDescription);
					signatureInlines.Add(FormulaEditorUtilities.CreateRun("]"));

					signatureInlines.Add(FormulaEditorUtilities.CreateRun(ArgumentSeparator));
					endsInArgumentSeparator = true;
				}
			}

			if (endsInArgumentSeparator)
				signatureInlines.RemoveAt(signatureInlines.Count - 1);

			signatureInlines.Add(FormulaEditorUtilities.CreateRun(")"));

			return signatureInlines;
		}

		#endregion  // GetFunctionSignature

		// MD 6/25/12 - TFS113177
		#region GetIndexedTuples

		private static bool[] GetIndexedTuples(ICalculationManager manager, ICalculationReference relativeReference)
		{
			RefParser relativeRefParser = new RefParser(relativeReference.AbsoluteName);

			bool[] shouldIncludeIndexes = new bool[relativeRefParser.TupleCount];
			bool shouldIncludeIndexesFound = false;

			RefBase relativeRefBase = relativeReference as RefBase;
			if (relativeRefBase != null && relativeRefBase.ShouldFormulaEditorIncludeIndex)
			{
				shouldIncludeIndexes[shouldIncludeIndexes.Length - 1] = true;
				shouldIncludeIndexesFound = true;
			}

			string ancestorName = relativeReference.AbsoluteName;
			for (int i = relativeRefParser.TupleCount - 1; i >= 1; i--)
			{
				RefTuple tuple = relativeRefParser[i];
				ancestorName = ancestorName.Remove(ancestorName.Length - tuple.Name.Length);

				int slashIndex = ancestorName.LastIndexOf("/");
				if (slashIndex < 0)
				{
					Utilities.DebugFail("Could not find the slash.");
					return null;
				}

				ancestorName = ancestorName.Substring(0, slashIndex);

				ICalculationReference ancestorReference = manager.GetReference(ancestorName);
				if (ancestorReference == null)
				{
					Utilities.DebugFail("Could not find the ancestor reference.");
					return null;
				}

				RefBase ancestorRefBase = ancestorReference as RefBase;
				if (ancestorRefBase != null && ancestorRefBase.ShouldFormulaEditorIncludeIndex)
				{
					shouldIncludeIndexes[i - 1] = true;
					shouldIncludeIndexesFound = true;
				}
			}

			if (shouldIncludeIndexesFound == false)
				shouldIncludeIndexes[shouldIncludeIndexes.Length - 1] = true;

			return shouldIncludeIndexes;
		}

		#endregion // GetIndexedTuples

		#region GetOperands

		public static FilteredCollection<OperandInfo> GetOperands(FormulaEditorBase editor, XamCalculationManager calculationManager)
		{
			if (calculationManager == null)
				return null;

			object target = editor.Target;
			if (target == null)
				return null;

			return FormulaEditorUtilities.GetOperands(editor, calculationManager.GetReferenceTree(target));
		}

		public static FilteredCollection<OperandInfo> GetOperands(FormulaEditorBase editor, IEnumerable<CalculationReferenceNode> nodes)
		{
			if (nodes == null)
				return null;

			var sortedNodes =
				from node in nodes
				orderby node.SortPriority, node.DisplayNameResolved
				select new OperandInfo(editor, node);

			return FormulaEditorUtilities.CreateFilteredCollection(sortedNodes.ToList());
		}

		#endregion  // GetOperands

		#region GetOperandSignature

		public static string GetOperandSignature(IFormulaProvider targetProvider,
			OperandInfo operand,
			List<FunctionInfo> functions,
			out bool isUsingReferenceIndexing,
			out int indexOffset, // MD 6/25/12 - TFS113177
			string formulaText = null,
			int insertionIndex = 0,
			string relativeIndex = "*")
		{
			isUsingReferenceIndexing = false;

			// MD 6/25/12 - TFS113177
			indexOffset = 0;

			if (operand.Reference == null)
				return string.Empty;

			// MD 6/25/12 - TFS113177
			//string relativeName = FormulaEditorUtilities.GetRelativeReferenceName(targetProvider.Reference, operand.Reference);

			if (FormulaEditorUtilities.DoesRequireReferenceIndexing(targetProvider, operand, functions, formulaText, insertionIndex))
			{
				isUsingReferenceIndexing = true;

				// MD 6/25/12 - TFS113177
				//return string.Format("[{0}({1})]", relativeName, relativeIndex);
			}

			// MD 6/25/12 - TFS113177
			//return "[" + relativeName + "]";
			string indexString = string.Empty;
			if (isUsingReferenceIndexing)
				indexString = string.Format("({0})", relativeIndex);

			string relativeName = FormulaEditorUtilities.GetRelativeReferenceName(
				targetProvider.CalculationManager, targetProvider.Reference, operand.Reference, indexString);

			string finalName = "[" + relativeName + "]";

			if (isUsingReferenceIndexing)
			{
				int index = finalName.IndexOf(indexString);
				if (0 <= index)
					indexOffset = index - finalName.Length + 1;
				else
					indexOffset = -3;
			}

			return finalName;
		}

		#endregion  // GetOperandSignature

		#region GetOwningFunctionProduction

		public static FormulaProduction GetOwningFunctionProduction(FormulaElement element, bool isTokenBeforeInsertionIndex, out List<FormulaElement> ancestorChainUnderFunction)
		{
			FormulaProduction functionProduction = null;

			ancestorChainUnderFunction = new List<FormulaElement>();
			ancestorChainUnderFunction.Add(element);

			FormulaProduction parent = element as FormulaProduction;
			if (parent == null)
				parent = element.Parent;

			// If this cursor is right after the closing parenthesis or right before the opening parenthesis of the function 
			// being constructed, we are not in that function, so we will then skip over it and move to its parent.
			if (parent.IsFunction)
			{
				if (element.IsRightParen && isTokenBeforeInsertionIndex)
				{
					parent = parent.Parent;
					ancestorChainUnderFunction.Add(parent);
				}
				else if (element.IsLeftParen && isTokenBeforeInsertionIndex == false)
				{
					parent = parent.Parent;
					ancestorChainUnderFunction.Add(parent);
				}
			}

			while (parent != null)
			{
				if (parent.IsFunction)
				{
					functionProduction = parent;
					break;
				}

				ancestorChainUnderFunction.Add(parent);

				// If we are in a funcID, we don't want to show the construction help, so move up to the parent, which 
				// is a function, so we will then skip over it and move to its parent.
				if (parent.IsFuncId)
				{
					parent = parent.Parent;
					ancestorChainUnderFunction.Add(parent);
				}

				parent = parent.Parent;
			}

			return functionProduction;
		}

		#endregion  // GetOwningFunctionProduction

		#region GetRelativeReferenceName

		// MD 6/25/12 - TFS113177
		// Re-wrote this so the indexes could be added in the middle of the reference instead of just at the end.
		#region Old Code
      
		//public static string GetRelativeReferenceName(ICalculationReference baseReference, ICalculationReference relativeReference)
		//{
		//    if (baseReference == relativeReference)
		//        return baseReference.ElementName;

		//    if (baseReference == null)
		//        return relativeReference.AbsoluteName;

		//    string baseRefName = baseReference.AbsoluteName.ToLower();
		//    RefParser baseRefParser = new RefParser(baseRefName);

		//    string relativeRefName = relativeReference.AbsoluteName.ToLower();
		//    RefParser relativeRefParser = new RefParser(relativeRefName);

		//    if (baseRefParser.TupleCount > 1 &&
		//        relativeRefParser.TupleCount > 1 &&
		//        baseReference.Parent == relativeReference.Parent)
		//        return relativeReference.ElementName;

		//    int shortestReferenceTupleCount = Math.Min(baseRefParser.TupleCount, relativeRefParser.TupleCount);

		//    int lastMatchingTupleIndex = -1;
		//    for (int i = 0; i < shortestReferenceTupleCount; i++)
		//    {
		//        if (baseRefParser[i].Name == relativeRefParser[i].Name)
		//            lastMatchingTupleIndex = i;
		//        else
		//            break;
		//    }

		//    if (lastMatchingTupleIndex == -1)
		//        return relativeReference.AbsoluteName;

		//    StringBuilder stringBuilder = new StringBuilder();

		//    for (int i = 0; i <= lastMatchingTupleIndex; i++)
		//    {
		//        if (i == 0)
		//            stringBuilder.Append("//");
		//        else
		//            stringBuilder.Append("/");

		//        stringBuilder.Append(baseRefParser[i]);
		//    }
		//    string commonAncestor = stringBuilder.ToString().ToLower();

		//    baseRefName = baseRefName.Replace(commonAncestor, String.Empty);
		//    if (baseRefName.StartsWith("/"))
		//        baseRefName = baseRefName.Substring(1);

		//    relativeRefName = relativeRefName.Replace(commonAncestor, String.Empty);
		//    if (relativeRefName.StartsWith("/"))
		//        relativeRefName = relativeRefName.Substring(1);

		//    stringBuilder.Length = 0;
		//    if (baseRefName.Length > 0)
		//    {
		//        baseRefParser = new RefParser(baseRefName);
		//        for (int i = 0; i < baseRefParser.TupleCount; i++)
		//        {
		//            stringBuilder.Append("../");
		//        }
		//    }

		//    stringBuilder.Append(relativeRefName);

		//    return stringBuilder.ToString();
		//}
    
	#endregion // Old Code
		private static string GetRelativeReferenceName(ICalculationManager manager, ICalculationReference baseReference, ICalculationReference relativeReference, string indexString)
		{
			if (baseReference == relativeReference)
				return baseReference.ElementName + indexString;

			bool[] shouldIncludeIndexes = GetIndexedTuples(manager, relativeReference);

			if (baseReference == null)
				return GetFullRefernceString(shouldIncludeIndexes, relativeReference, indexString);

			RefParser relativeRefParser = new RefParser(relativeReference.AbsoluteName);
			RefParser baseRefParser = new RefParser(baseReference.AbsoluteName);

			if (baseRefParser.TupleCount > 1 &&
				relativeRefParser.TupleCount > 1 &&
				baseReference.Parent == relativeReference.Parent)
				return relativeReference.ElementName + indexString;

			int shortestReferenceTupleCount = Math.Min(baseRefParser.TupleCount, relativeRefParser.TupleCount);

			int lastMatchingTupleIndex = -1;
			for (int i = 0; i < shortestReferenceTupleCount; i++)
			{
				if (String.Equals(baseRefParser[i].Name, relativeRefParser[i].Name, StringComparison.CurrentCultureIgnoreCase))
					lastMatchingTupleIndex = i;
				else
					break;
			}

			if (lastMatchingTupleIndex == -1)
				return GetFullRefernceString(shouldIncludeIndexes, relativeReference, indexString);

			StringBuilder stringBuilder = new StringBuilder();

			if (lastMatchingTupleIndex == -1)
			{
				stringBuilder.Append("//");
			}
			else
			{
				for (int i = lastMatchingTupleIndex + 1; i < baseRefParser.TupleCount; i++)
				{
					if (i != 0)
						stringBuilder.Append("/");

					stringBuilder.Append("..");
				}
			}

			relativeRefParser = new RefParser(relativeReference.AbsoluteName, true);
			for (int i = lastMatchingTupleIndex + 1; i < relativeRefParser.TupleCount; i++)
			{
				if (i != 0)
					stringBuilder.Append("/");

				stringBuilder.Append(relativeRefParser[i].Name);
			}

			return stringBuilder.ToString();
		}

		#endregion GetRelativeReferenceName

		#region GetString

		public static string GetString(string name)
		{
#pragma warning disable 436
			return SR.GetString(name);
#pragma warning restore 436
		}

		public static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}

		#endregion  // GetString

		#region InsertFunction

		public static void InsertFunction(FormulaEditorBase editor, CalculationFunction function)
		{
			string functionName = function.Name.ToUpper();

			if (function.MaxArgs == 0)
			{
				FormulaEditorUtilities.InsertText(editor, string.Format("{0}()", functionName));
			}
			else
			{
				FormulaEditorUtilities.InsertText(editor, string.Format("{0}(  )", functionName), -2);

				if (editor.ContextualHelpHost != null)
					editor.ContextualHelpHost.ShowFunctionSignatureHelpDelayed(0.1);
			}
		}

		#endregion  // InsertFunction

		#region InsertOperand

		public static void InsertOperand(FormulaEditorBase editor, OperandInfo operand)
		{
			int selectionStartOffset;
			int selectionEndOffset;
			string referenceName = FormulaEditorUtilities.PrepareToInsertOperand(
				editor, 
				operand, 
				out selectionStartOffset, 
				out selectionEndOffset);

			FormulaEditorUtilities.InsertText(editor, referenceName, selectionStartOffset, selectionEndOffset);
		}

		#endregion  // InsertOperand

		#region InsertOperator

		public static void InsertOperator(FormulaEditorBase editor, string operatorValue)
		{
			FormulaEditorUtilities.InsertText(editor, operatorValue);
		}

		#endregion  // InsertOperator

		#region PrepareToInsertOperand

		public static string PrepareToInsertOperand(FormulaEditorBase editor, OperandInfo operand, out int selectionStartOffset, out int selectionEndOffset)
		{
			selectionStartOffset = 0;
			selectionEndOffset = 0;

			FormulaEditorTextBox textBox = editor.TextBox;
			string text = null;
			int insertionIndex = 0;
			if (textBox != null)
			{
				text = textBox.Text;
				insertionIndex = textBox.Selection.Start.GetCharacterIndex(text);
			}

			bool isUsingReferenceIndexing;

			// MD 6/25/12 - TFS113177
			int indexOffset;

			string signature = FormulaEditorUtilities.GetOperandSignature(
				editor.FormulaProvider,
				operand,
				editor.Functions,
				out isUsingReferenceIndexing,
				out indexOffset,	// MD 6/25/12 - TFS113177
				text,
				insertionIndex,
				"0");

			if (isUsingReferenceIndexing)
			{
				// MD 6/25/12 - TFS113177
				// The index may not always occur at the same relative offset anymore, so get it from the GetOperandSignature call.
				//selectionStartOffset = -3;
				//selectionEndOffset = -2;
				selectionStartOffset = indexOffset;
				selectionEndOffset = indexOffset + 1;
			}

			return signature;
		}

		#endregion  // PrepareToInsertOperand

		#region ReinitializeEnabledStateOfOperands

		public static void ReinitializeEnabledStateOfOperands(FilteredCollection<OperandInfo> operands)
		{
			if (operands == null)
				return;

			foreach (OperandInfo operandInfo in operands)
			{
				operandInfo.ReinitializeEnabled();
				FormulaEditorUtilities.ReinitializeEnabledStateOfOperands(operandInfo.Children);
			}
		}

		#endregion  // ReinitializeEnabledStateOfOperands

		#endregion // Public Methods

		#region Private Methods

		#region AddSignatureArgument

		private static void AddArgumentToSignature(List<Inline> signatureInlines, CalculationFunction function, int argumentIndex, int currentArgumentIndex, ref Span argumentDescription)
		{
			string functionName = FormulaEditorUtilities.GetArgumentName(function, argumentIndex);
			Run run = FormulaEditorUtilities.CreateRun(functionName);

			if (argumentIndex == currentArgumentIndex)
			{
				run.FontWeight = FontWeights.Bold;

				string description = FormulaEditorUtilities.GetArgumentDescription(function, argumentIndex);

				if (description != null)
				{
					// Probably should use a parsed localized string here.
					argumentDescription = new Span();
					argumentDescription.FontStyle = FontStyles.Italic;
					argumentDescription.Inlines.Add(FormulaEditorUtilities.CreateRun(functionName + ": ", FontWeights.Bold));
					argumentDescription.Inlines.Add(FormulaEditorUtilities.CreateRun(description));
				}
			}

			signatureInlines.Add(run);
		}

		#endregion // AddSignatureArgument

		#region CreateFilteredCollection

		private static FilteredCollection<T> CreateFilteredCollection<T>(List<T> allItems)
		{
			if (allItems == null)
				return null;

			return new FilteredCollection<T>(allItems);
		}

		#endregion  // CreateFilteredCollection

		#region GetArgumentDescription

		private static string GetArgumentDescription(CalculationFunction function, int index)
		{
			string argName = null;

			string[] argDescriptors = function.ArgDescriptors;
			if (argDescriptors != null)
			{
				int lastArgIndex = argDescriptors.Length - 1;

				if (function.HasUnlimitedArguments() && index >= lastArgIndex)
				{
					argName = argDescriptors[lastArgIndex];
				}
				else if (index < argDescriptors.Length)
				{
					argName = argDescriptors[index];
				}
			}

			if (String.IsNullOrEmpty(argName))
				return null;

			return argName;
		}

		#endregion // GetArgumentDescription

		#region GetArgumentName

		private static string GetArgumentName(CalculationFunction function, int index)
		{
			string argName = null;

			string[] argList = function.ArgList;
			if (argList != null)
			{
				int lastArgIndex = argList.Length - 1;

				if (function.HasUnlimitedArguments() && index >= lastArgIndex)
				{
					argName = argList[lastArgIndex];

					if (argName != null && argName.IndexOf("{0}") > 0)
					{
						int relativeIndex = index - lastArgIndex;

						// If the argument numbering is 1-based, increment the relative index
						if (FormulaEditorUtilities.IsArgumentNumberingOneBased(function, index))
							relativeIndex++;

						argName = string.Format(argName, relativeIndex);
					}
				}
				else if (index < argList.Length)
				{
					argName = argList[index];
				}
			}

			if (argName == null || argName.Length == 0)
				return FormulaEditorUtilities.GetString("MissingArgumentName", index);

			return argName;
		}

		#endregion // GetArgumentName

		#region InsertText

		private static void InsertText(FormulaEditorBase editor, string textToInsert, int selectionStartOffsetFromEnd = 0, int? selectionEndOffsetFromEnd = null)
		{
			if (editor.TextBox != null)
			{
				if (selectionEndOffsetFromEnd.HasValue == false)
					selectionEndOffsetFromEnd = selectionStartOffsetFromEnd;

				editor.TextBox.InsertText(textToInsert, selectionStartOffsetFromEnd, selectionEndOffsetFromEnd.Value);
				return;
			}

			string formula = editor.Formula;

			// If there is no whitespace at the end of the formula, insert a space before the inserted text.
			if (formula.TrimEnd() == formula)
				textToInsert = " " + textToInsert;

			// Add whitespace after the inserted text.
			textToInsert += " ";

			editor.Formula += textToInsert;
		}

		#endregion  // InsertText

		#region IsArgumentNumberingOneBased

		private static bool IsArgumentNumberingOneBased(CalculationFunction function, int index)
		{
			Type functionType = function.GetType();
			object[] customAttributes = functionType.GetCustomAttributes(typeof(OneBasedArgumentNumberingAttribute), true);

			foreach (OneBasedArgumentNumberingAttribute attribute in customAttributes)
			{
				if (attribute.ArgumentIndex == index)
					return true;

				// If a variable number of arguments are allowed at the end, we should honor the 1-based attribute for 
				// all items at the end.
				if (function.HasUnlimitedArguments() && attribute.ArgumentIndex == (function.MinArgs - 1))
					return true;
			}

			return false;
		}

		#endregion IsArgumentNumberingOneBased

		#endregion // Private Methods

		#region Extension Methods

		#region CalculationFunction extension methods

		#region HasUnlimitedArguments

		public static bool HasUnlimitedArguments(this CalculationFunction function)
		{
			return function.MaxArgs == int.MaxValue;
		}

		#endregion  // HasUnlimitedArguments

		#endregion  // CalculationFunction extension methods

		#region TextPointer extension methods

		#region GetCharacterIndex



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public static int GetCharacterIndex(this TextPointer characterPosition, string text)
		{
			TextPointer previous = characterPosition.GetNextInsertionPosition(LogicalDirection.Backward);

			int count;
			for (count = 0; previous != null; count++)
				previous = previous.GetNextInsertionPosition(LogicalDirection.Backward);

			if (text != null)
			{
				// A carriage return and line feed will occupy one insertion position, so we need to increate the count for 
				// each one we come across.
				int i = 0;
				while (i < text.Length)
				{
					i = text.IndexOf("\r\n", i);
					if (i < 0 || count <= i)
						break;

					count++;
					i += 2;
				}
			}

			return count;
		}

		#endregion  // GetCharacterIndex

		#endregion // TextPointer extension methods

		#endregion // Extension Methods

		#endregion // Methods
	}
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved