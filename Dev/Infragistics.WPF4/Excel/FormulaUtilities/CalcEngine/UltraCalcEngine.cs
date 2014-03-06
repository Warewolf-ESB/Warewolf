using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Security;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;





namespace Infragistics.Documents.Excel.CalcEngine
{


#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

	internal class ExcelCalcEngine
	{
		#region Member Variables

		// Instance of the UltraCalcAncestorMap used to lookup ancestors of a given reference
		private UltraCalcAncestorMap ancestorMap;

		// Instance of the UCRecalcChange used to track dirty references whose formulas need to be recalculated
		private UCRecalcChain recalcChain;

		// Function factory containing built-in and user defined functions registered with the calc engine instance
		private UltraCalcFunctionFactory functionFactory;

		// Denotes that the recalc chain needs to be sorted due to formulas being dirtied
		private bool doTopologicalSort;

		// A list of references whose formulas contain always dirty functions
		private UltraCalcReferenceCollection alwaysDirtyList;

		private int ancestorMapVersion = 0;
		private int verifiedAncestorMapVersion = -1;

		// MD 8/12/08 - Excel formula solving
		// Added an anti-recursion flag to make sure we don't try to evaluate something on the recalc chain recursively.
		private bool isEvaluating;

		// MD 8/20/08 - Excel formula solving
		private Dictionary<IExcelCalcReference, object> circularReferences = new Dictionary<IExcelCalcReference, object>();

		// MD 8/21/08 - Excel formula solving
		private Dictionary<IExcelCalcReference, IExcelCalcFormula> formulasByReference = new Dictionary<IExcelCalcReference, IExcelCalcFormula>();

		// MD 1/25/12 - TFS100119
		private bool detectedNewCircularity;

		// MD 3/2/12 - TFS103729
		// We need to assign new sort numbers as dynamic references are added, so keep track of the next number to use.
		private int nextDependencySortNumber;

		// MD 3/1/12 - 12.1 - Table Support
		private Dictionary<WorksheetTable, List<IExcelCalcReference>> referencedTableMap =
			new Dictionary<WorksheetTable, List<IExcelCalcReference>>();

		// MD 4/2/12 - TFS99854
		// Store a dependency graph to quickly find ancestors when dynamic references are added.
		private DependencyGraph dependencyGraph;

		// MD 11/1/10 - TFS56976
		// Keep a flag which determines when we can skip the dirtying of parents.
		private bool preventDirtyParents = true;

		// MD 7/19/12 - TFS116808 (Table resizing)
		// Added a reference to the workbook so we don't need to store and sync duplicate values on this object.
		private Workbook workbook;

		#endregion // Member Variables

		#region Constructor

		public ExcelCalcEngine(Workbook workbook)
		{
			this.workbook = workbook;
			this.ancestorMap = new UltraCalcAncestorMap();
			this.recalcChain = new UCRecalcChain();
			this.functionFactory = new UltraCalcFunctionFactory();
			this.doTopologicalSort = false;
			this.alwaysDirtyList = new UltraCalcReferenceCollection();
		}

		#endregion //Constructor

		#region Methods

		#region Public Methods

		#region AddDynamicPredecessor



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public void AddDynamicPredecessor(IExcelCalcReference predecessor, IExcelCalcReference ancestor, out bool predecessorIsDirty)
		{
			predecessorIsDirty = false;

			ancestor = ExcelCalcEngine.GetResolvedReference(ancestor);
			predecessor = ExcelCalcEngine.GetResolvedReference(predecessor);

			UCReference predecessorWrapper = predecessor as UCReference;

			// Only UCReference instances can be added to the ancestor map, so wrap the predecessor is a UCReference is necessary.
			if (predecessorWrapper == null)
				predecessorWrapper = new UCReference(predecessor);

			// MD 1/12/12 - TFS99279
			// There is now a faster way to see if any recalc references are contained in the predecessor.
			#region Old Code

			//// If the predecessor is in the recalc chain, it is dirty and the ancestor and all its ancestors must be ignored
			//// until the predecessor is calculated.
			//foreach ( IUltraCalcReference reference in this.recalcChain )
			//{
			//    //if ( predecessor.ContainsReference( reference ) == false )
			//    if (predecessorExcel.ContainsReference(reference) == false)
			//        continue;
			//
			//    predecessorIsDirty = true;
			//
			//    // Recursively add the ancestor and all its ancestors to the ignore list if the predecessor is dirty.
			//    Dictionary<IUltraCalcReference, object> ancestorDictionary = new Dictionary<IExcelCalcReference, object>();
			//    this.GetAncestors( ancestor, ancestorDictionary );
			//
			//    foreach ( IExcelCalcReference ignoredAncestor in ancestorDictionary.Keys )
			//        this.IgnoreRecalcList[ ignoredAncestor ] = null;
			//
			//    break;
			//}

			#endregion  // Old Code
			// If the predecessor is in the recalc chain, it is dirty and the ancestor and all its ancestors must be ignored
			// until the predecessor is calculated.
			// MD 4/2/12 - TFS99854
			// For enumerable references, the reference will not be in the recalcChain, but one of its constituent references might be.
			//if (this.recalcChain.Contains(predecessor))
			bool isInRecalcChain = false;
			bool isPredecessorEnumerable = predecessor.IsEnumerable;
			if (isPredecessorEnumerable)
			{
				foreach (IExcelCalcReference childReference in predecessor.References)
				{
					if (this.recalcChain.Contains(childReference))
					{
						isInRecalcChain = true;
						break;
					}
				}
			}
			else
			{
				isInRecalcChain = this.recalcChain.Contains(predecessor);
			}

			if (isInRecalcChain)
			{
				predecessorIsDirty = true;

				// Recursively add the ancestor and all its ancestors to the ignore list if the predecessor is dirty.
				// MD 4/2/12 - TFS99854
				// We can get the ancestors quicker by using the dependency graph. Also, determine whether to remove and re-add
				// the ancestors to the end of the recalcChain or whether to just resort the recalcChain after assigning new
				// DependancySortNumber values.
				//Dictionary<IExcelCalcReference, object> ancestorDictionary = new Dictionary<IExcelCalcReference, object>();
				//this.GetAncestorsInRecalcChain(ancestor, ancestorDictionary);
				Dictionary<IExcelCalcReference, bool> ancestorDictionary = this.dependencyGraph.GetAncestors(ancestor);
				bool shouldRemoveAndReAdd = ancestorDictionary.Count < (this.recalcChain.Count / 10);

				// MD 1/25/12 - TFS100119
				// The IgnoreRecalcList was a relatively slow way of doing things when dynamic references were added. 
				// Now we will instead reorder the recalc chain when we add a dynamic reference to another reference 
				// marked for recalculation. But in some cases, we may detect a new circularity when the dynamic 
				// reference is added. In these cases, we really should resort all references and start another 
				// calculation loop.
				//foreach (IExcelCalcReference ignoredAncestor in ancestorDictionary.Keys)
				//    this.IgnoreRecalcList[ignoredAncestor] = null;
				SortedList<int, IExcelCalcReference> sortedAncestors = new SortedList<int, IExcelCalcReference>();
				foreach (IExcelCalcReference ancestorInRecalcChain in ancestorDictionary.Keys)
				{
					// If the dynamic predecessor actualy depends on the ancestor, there is a new circularity. We should
					// set the flag to indicate that the entire ancestor map should be resorted and recalculated.
					// MD 4/2/12 - TFS99854
					// For enumerable references, the reference will not be in the recalcChain, but one of its constituent references might be,
					// so check ContainsReference instead of checking for equality.
					//if (ancestorInRecalcChain == predecessor)
					bool isContained;
					if (isPredecessorEnumerable)
						isContained = (predecessor.ContainsReference(ancestorInRecalcChain));
					else
						isContained = (ancestorInRecalcChain == predecessor);

					if (isContained)
					{
						this.detectedNewCircularity = true;
						continue;
					}

					ExcelCalcFormula formula = ancestorInRecalcChain.Formula as ExcelCalcFormula;
					if (formula == null)
					{
						Utilities.DebugFail("We should have an ExcelCalcFormula here.");
						continue;
					}

					// Remove the ancestor from the recalc chain and add it to the sorted list. All other items which depend 
					// on this ancestor will be placed after it in the sorted list.
					// MD 4/2/12 - TFS99854
					// Only remove the reference from the recalcChain if the if we aren't going to sort it at the end of the process.
					//this.recalcChain.Remove(ancestorInRecalcChain);
					if (shouldRemoveAndReAdd)
						this.recalcChain.Remove(ancestorInRecalcChain);

					sortedAncestors.Add(formula.DependencySortNumber, ancestorInRecalcChain);
				}

				// Re-add the ancestor and all references which depend on it, in their relative sort order, into the recalc chain.

				// MD 3/2/12 - TFS103729
				// This is unnecessary because the ancestor is already in the sortedAncestors collection.
				//this.recalcChain.Add(ancestor);

				foreach (IExcelCalcReference ancestorInRecalcChain in sortedAncestors.Values)
				{
					// MD 3/2/12 - TFS103729
					// We were place the formulas at the end of the reclac chain, but they were not "logically" at the end 
					// because they still had their original sort numbers. So if another dynamic reference were added after 
					// this one and some of these ancestors need to be moved to the end again, we were sorting them back before 
					// the references we moved them after here. Now we will always give them the latest sort numbers so any
					// subsequent sorts will keep these at the end.
					ExcelCalcFormula formula = (ExcelCalcFormula)ancestorInRecalcChain.Formula;
					formula.DependencySortNumber = this.nextDependencySortNumber++;

					// MD 4/2/12 - TFS99854
					// Only add the reference from the recalcChain if the if we aren't going to sort it at the end of the process.
					//this.recalcChain.Add(ancestorInRecalcChain);
					if (shouldRemoveAndReAdd)
						this.recalcChain.Add(ancestorInRecalcChain);
				}

				// MD 4/2/12 - TFS99854
				// If we didn't remove and re-add the references to the end of the recalcChain, resort it now that the references
				// have new DependancySortNumber values.
				if (shouldRemoveAndReAdd == false)
				{
					// The ancestor might not be in the recalc chain if we have finished evaluation and already removed the ancestor 
					// and then when dereferencing the result to assign it to the base reference, it causes a dynamic reference to be added.
					if (this.recalcChain[0] != ancestor)
						this.recalcChain.Add(ancestor);

					this.recalcChain.Sort(new RecalcChainSortComparer());
				}
			}

			this.ancestorMap.AddAncestor(predecessorWrapper, ancestor);

			// MD 4/2/12 - TFS99854
			// Also add the new reference to the dependency graph.
			this.dependencyGraph.AddDependency(predecessor, ancestor);

			// MD 8/20/08 - Excel formula solving
			// If the ancestor map was changed, bump its version so the ancestor map is reverified
			this.BumpAncestorMapVersion();
		}

		#endregion AddDynamicPredecessor

		#region AddFormula

		public void AddFormula(IExcelCalcFormula formula)
		{
			this.ProcessAddFormula(formula, formula.BaseReference);
		}

		#endregion // AddFormula

		#region CompareReferences

		public static bool CompareReferences(IExcelCalcReference reference1, IExcelCalcReference reference2)
		{
			// SSP 10/7/04
			// Use Equals method on the references instead of the absolute name comparison. Now we are
			// requiring the reference implementors to override GetHashCode and Equals methods and 
			// implement them properly.
			//

			FormulaReference fr1 = reference1 as FormulaReference;
			FormulaReference fr2 = reference2 as FormulaReference;
			if (null != fr1)
				reference1 = fr1.UnderlyingReference;
			if (null != fr2)
				reference2 = fr2.UnderlyingReference;

			// If the second reference is an UCReference swap reference1 and reference2 to ensure that
			// we call Equals on the UCReference. UCReference.Equals has logic to compare it's underlying 
			// reference with the passed in reference.
			//
			if (reference2 is UCReference)
			{
				IExcelCalcReference tmp = reference1;
				reference1 = reference2;
				reference2 = tmp;
			}

			return reference1 == reference2 || null != reference1 && null != reference2 && reference1.Equals(reference2);
		}

		#endregion // CompareReferences

		#region DeleteFormula

		public void DeleteFormula(IExcelCalcFormula formula)
		{
			this.ProcessDeleteFormula(formula, formula.BaseReference);
		}

		#endregion // DeleteFormula

		#region DirtyAllFormulas






		public void DirtyAllFormulas()
		{
			foreach (ReferenceHolder rh in this.GetFormulas())
				this.recalcChain.Add(rh.Reference);
		}

		#endregion // DirtyAllFormulas

		#region GetResolvedReference






		public static IExcelCalcReference GetResolvedReference(IExcelCalcReference reference)
		{
			return ExcelCalcEngine.GetResolvedReferenceHelper(reference, false);
		}






		private static IExcelCalcReference GetResolvedReferenceHelper(IExcelCalcReference reference, bool unwrapOnlyUCReferences)
		{
			if (reference == null)
				return null;

			// MD 8/22/08 - Excel formula solving - Performance
			// This is a small performance enhancement for the Excel assembly.
			if (reference is ExcelRefBase)
				return reference;

			// MD 8/26/08 - BR35804
			// Certain wrappers contain inportant information for formula solving and shouldn't be unwrapped.
			if (unwrapOnlyUCReferences == false)
			{
				FormulaReference formulaReference = reference as FormulaReference;

				if (formulaReference != null)
					return ExcelCalcEngine.GetResolvedReference(formulaReference.UnderlyingReference);
			}

			UCReference ucReference = reference as UCReference;

			if (ucReference != null && ucReference.IsConnected)
				return ExcelCalcEngine.GetResolvedReferenceHelper(ucReference.Reference, unwrapOnlyUCReferences);

			return reference;
		}

		#endregion GetResolvedReference

		#region IsDirty



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public bool IsDirty(IExcelCalcReference reference, bool wait)
		{
			// MD 8/20/08 - Excel formula solving
			// Moved all code to the new overload
			return this.IsDirty(reference, wait, true);
		}

		#endregion // IsDirty

		#region IsReferenced






		public bool IsReferenced(IExcelCalcReference reference)
		{
			this.VerifyAncestorMap();
			UltraCalcReferenceCollection ancestors = this.ancestorMap.Ancestors(reference);
			return ancestors.Count != 0;
		}

		#endregion // IsReferenced

		#region NotifyTableDirtied

		public void NotifyTableDirtied(WorksheetTable table)
		{
			List<IExcelCalcReference> formulasReferencingTable;
			if (this.referencedTableMap.TryGetValue(table, out formulasReferencingTable) == false)
				return;

			for (int i = 0; i < formulasReferencingTable.Count; i++)
				this.DirtyParents(formulasReferencingTable[i], true);
		}

		#endregion // NotifyTableDirtied

		#region NotifyTableRemoved

		public void NotifyTableRemoved(WorksheetTable table)
		{
			this.referencedTableMap.Remove(table);
		}

		#endregion // NotifyTableRemoved

		#region NotifyTopologicalChange

		public void NotifyTopologicalChange(IExcelCalcReference changedReference, ReferenceActionCode action)
		{
			switch (action)
			{
				case ReferenceActionCode.Create:
					{
						// Connect all references contained by the event reference
						this.ConnectReferences(changedReference, true);

						// SSP 9/7/04
						// Since we are delaying the processing of the topological events, we need
						// to add the formulas of the reference and its descendant references when 
						// we process the Create topological event from the queue. Here is the rationale
						// for doing this. Let's say the user of the calc-engine does topo-Create,
						// AddFormula, topo-Delete, topo-Create, AddFormula. The problem is that the
						// AddFormula gets processed synchronously but the topological events gets
						// enqued. As a result, here's how things will actually get processed:
						// AddFormula, AddFormula, top-Create, topo-Delete, topo-Create. As a result
						// the formula will not exist in the calc net.
						// Added AddFormulas method.
						//
						this.AddFormulas(changedReference);
					}
					break;

				case ReferenceActionCode.Rename:
					{
						List<UCReference> renamedReferences = new List<UCReference>();
						foreach (UltraCalcAncestorMap.AncestorMapEntry entry in ancestorMap)
						{
							UCReference predecessor = entry.Predecessor;
							if (predecessor.IsConnected)
							{
								if (predecessor.Reference == changedReference)
									renamedReferences.Add(predecessor);
							}
							else
							{
								Debug.Assert(
									changedReference.ElementName.ToLower() != predecessor.ElementName,
									"A formula already references this name. We need to check for this and throw an exception.");
							}
						}

						for (int i = 0; i < renamedReferences.Count; i++)
							ancestorMap.OnPredecessorRenamed(renamedReferences[i]);
					}
					break;

				case ReferenceActionCode.Remove:
					{
						// Find all formulas that are contained by the changed reference and remove them them
						UltraCalcReferenceCollection references = new UltraCalcReferenceCollection();
						foreach (UltraCalcAncestorMap.AncestorMapEntry entry in ancestorMap)
						{
							// SSP 7/13/05
							// Use the FormulasSubsetOf instead of FormulasContainedBy. If a cell gets removed, since the
							// cell 'contains' its column we would end up removing the column formula as well.
							// Correct check here is the IsSubset.
							// 
							//foreach (IUltraCalcReference reference in FormulasContainedBy(item.Reference, entry.Ancestors))
							foreach (IExcelCalcReference reference in this.FormulasSubsetOf(changedReference, entry.Ancestors))
							{
								if (references.IndexOf(reference) == -1)
									references.Add(reference);
							}
						}

						foreach (IExcelCalcReference reference in references)
						{
							ProcessDeleteFormula(reference.Formula, ((FormulaReference)reference).UnderlyingReference);
						}

						// Disconnect all references contained by the event reference
						this.ConnectReferences(changedReference, false);
					}
					break;
			}
		}

		#endregion //NotifyTopologicalChange

		#region NotifyValueChanged



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		public bool NotifyValueChanged(IExcelCalcReference reference)
		{
			// MD 3/16/09 - TFS14252
			// The method now has a return value.
			bool returnValue = false;

			// SSP 10/15/04
			// Added a way to suspend processing of calc notifications because if there are a lot of
			// such notifications being sent from the grid then it may take a very long time to process
			// them. Workaround would be to suspend notification processing and then dirty all the 
			// formulas so every thing gets recalculated.
			//

			// MD 3/16/09 - TFS14252
			// This method should return True if any parents have been dirtied.
			//this.DirtyParents(reference, false);
			// MD 11/1/10 - TFS56976
			// If the ancestor map is empty, there's no reason to call the DirtyParents method, because nothing will happen.
			//returnValue = this.DirtyParents( reference, false );
			returnValue = this.ancestorMap.Count > 0 && this.DirtyParents(reference, false);

			// MD 7/23/08 - Excel formula solving
			// Moved this code to a helper method so it could be called from other places.
			//foreach (IUltraCalcReference alwaysDirtyItem in this.alwaysDirtyList)
			//	this.DirtyParents(alwaysDirtyItem, true);
			this.DirtyAlwaysDirtyList();


			// MD 3/16/09 - TFS14252
			// The method now has a return value.
			return returnValue;
		}

		#endregion //NotifyValueChanged

		#region Recalc



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		public void Recalc()
		{
			this.Recalc(true);
		}

		#endregion // Recalc

		#region RemoveAncestorMapEntry






		public void RemoveAncestorMapEntry(IExcelCalcReference predecessor, IExcelCalcReference ancestor)
		{
			this.ancestorMap.DeleteAncestor(predecessor, ancestor);

			// MD 4/2/12 - TFS99854
			// Also remove the reference from the dependency graph.
			this.dependencyGraph.RemoveDependency(predecessor, ancestor);

			// MD 8/20/08 - Excel formula solving
			// If the ancestor map was changed, bump its version so the ancestor map is reverified
			this.BumpAncestorMapVersion();
		}

		#endregion RemoveAncestorMapEntry

		#endregion // Public Methods

		#region Private Methods

		#region AddFormulas

		// SSP 9/7/04
		// Since we are delaying the processing of the topological events, we need
		// to add the formulas of the reference and its descendant references when 
		// we process the Create topological event from the queue. Here is the rationale
		// for doing this. Let's say the user of the calc-engine does topo-Create,
		// AddFormula, top-Delete, topo-Create, AddFormula. The problem is that the
		// AddFormula gets processed synchronously but the topological events gets
		// enqued. As a result, here's how things will actually get processed:
		// AddFormula, AddFormula, top-Create, topo-Delete, topo-Create. As a result
		// the formula will not exist in the calc net.
		// Added AddFormulas method.
		//
		private void AddFormulas(IExcelCalcReference addedReference)
		{
			if (null != addedReference)
			{
				IExcelCalcFormula formula = addedReference.Formula;

				// We shouldn't add the formula for cell references. The way we can check this
				// is by making sure that the base reference of the formula is the same as
				// the reference we are processing.
				//
				if (null != formula && formula.BaseReference == addedReference)
				{
					this.ProcessAddFormula(formula, formula.BaseReference);
				}
			}
		}

		#endregion //AddFormulas

		#region AfterFormulaEvaluationCompleted

		/// <summary>
		/// Occurs after a single evaluation of a formula has completed.
		/// </summary>
		private void AfterFormulaEvaluationCompleted(IExcelCalcReference reference, ExcelCalcFormula referenceFormula, ExcelCalcValue evaluatedValue)
		{
			// MD 10/25/10 - TFS36696
			// Refactored this code by moving the IsCircularFormula check from the if statement below to be around the whole block of code.
			// This is becasue we are only going to reset the iteration count on circular formulas now and so we should only increment it on
			// circular formulas as well.
			#region Refactored

			//// MD 8/20/08 - Excel formula solving
			//// Now that the formula has successfully completed, increment the iteration count.
			//referenceFormula.IncrementIterationCount();
			//
			//// We may need to add the reference back onto the recalc chain if this is a circular formula being solved. Only
			//// add the circular formula is iterative calcualtions are enabled and the formula has not gone over the maximum
			//// iteration count.
			//if ( this.IterativeCalculationsEnabled &&
			//    referenceFormula.IsCircularFormula &&
			//    referenceFormula.IterationCount < this.MaxRecursionIterations )
			//{
			//    bool isUnderMaxChangeInIteration = false;
			//
			//    // If this is the second iteration or more for the formula in this round of reclaculations, check to see if the 
			//    // newly calculated value differs from the last calculated value by more than the threshhold needed to continue
			//    // iterative calculations. If not, don't continue iterations.
			//    double currentValue;
			//    double nextValue;
			//    if ( referenceFormula.IterationCount > 1 && reference.Value.ToDouble( out currentValue ) && evaluatedValue.ToDouble( out nextValue ) )
			//    {
			//        if ( Math.Abs( nextValue - currentValue ) < Math.Abs( this.MaxChangeInIteration ) )
			//            isUnderMaxChangeInIteration = true;
			//    }
			//
			//    // If the maximum iteration count hasn't been reached and the maximum change threshhold has been passed so add the
			//    // reference on the recalc chain so it can be reclauclated for another round.
			//    if ( isUnderMaxChangeInIteration == false )
			//    {
			//        // If the reference being solved is not the only item on the recalc chain, the reference must be moved to the end 
			//        // of the recalc chain.
			//        if ( this.recalcChain.Count > 1 )
			//        {
			//            // Get the formula at the end of the recalc chain
			//            IUltraCalcReference lastReference = this.recalcChain[ this.recalcChain.Count - 1 ];
			//            UltraCalcFormulaBase lastFormula = lastReference.Formula as UltraCalcFormulaBase;
			//
			//            // Make its dependancy sort order temporarily greater than the last formula's so it will be at the end of the chain.
			//            if ( lastFormula != null )
			//                referenceFormula.SetDependancySortNumberForCircularReference( lastFormula.DependancySortNumber + 1 );
			//        }
			//
			//        // Re-add the reference to the recalc chain.
			//        this.recalcChain.Add( reference );
			//    }
			//} 

			#endregion // Refactored
			// MD 8/20/08 - Excel formula solving
			// Now that the formula has successfully completed, increment the iteration count.
			if (referenceFormula.IsCircularFormula)
			{
				referenceFormula.IncrementIterationCount();

				// We may need to add the reference back onto the recalc chain if this is a circular formula being solved. Only
				// add the circular formula is iterative calcualtions are enabled and the formula has not gone over the maximum
				// iteration count.
				if (this.workbook.IterativeCalculationsEnabled &&
					referenceFormula.IterationCount < this.workbook.MaxRecursionIterations)
				{
					bool isUnderMaxChangeInIteration = false;

					// If this is the second iteration or more for the formula in this round of reclaculations, check to see if the 
					// newly calculated value differs from the last calculated value by more than the threshhold needed to continue
					// iterative calculations. If not, don't continue iterations.
					double currentValue;
					double nextValue;
					if (referenceFormula.IterationCount > 1 && reference.Value.ToDouble(out currentValue) && evaluatedValue.ToDouble(out nextValue))
					{
						if (Math.Abs(nextValue - currentValue) < Math.Abs(this.workbook.MaxChangeInIteration))
							isUnderMaxChangeInIteration = true;
					}

					// If the maximum iteration count hasn't been reached and the maximum change threshhold has been passed so add the
					// reference on the recalc chain so it can be reclauclated for another round.
					if (isUnderMaxChangeInIteration == false)
					{
						// If the reference being solved is not the only item on the recalc chain, the reference must be moved to the end 
						// of the recalc chain.
						if (this.recalcChain.Count > 1)
						{
							// Get the formula at the end of the recalc chain
							IExcelCalcReference lastReference = this.recalcChain[this.recalcChain.Count - 1];
							ExcelCalcFormula lastFormula = lastReference.Formula as ExcelCalcFormula;

							// Make its dependancy sort order temporarily greater than the last formula's so it will be at the end of the chain.
							if (lastFormula != null)
								referenceFormula.SetDependancySortNumberForCircularReference(lastFormula.DependencySortNumber + 1);
						}

						// Re-add the reference to the recalc chain.
						this.recalcChain.Add(reference);
					}
				}
			}

			// Set the evaluated value of the reference.
			reference.Value = evaluatedValue;
		}

		#endregion AfterFormulaEvaluationCompleted

		#region BumpAncestorMapVersion






		private void BumpAncestorMapVersion()
		{
			this.ancestorMapVersion++;
		}

		#endregion // BumpAncestorMapVersion

		#region ClearDirtyChain



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private void ClearDirtyChain()
		{
			// SSP 10/6/04
			// Commented out the original code and added the new one.
			//
			if (this.recalcChain.HasDirtyItems)
			{
				for (int i = this.recalcChain.DirtyChainStartIndex; i < this.recalcChain.Count; i++)
					this.DirtyParents(this.recalcChain[i], false);

				// Clear the dirty chain start index.
				//
				this.recalcChain.ResetDirtyChainStartIndex();

				// Set doTopologicalSort to true so we resort the recalc chain before evaluating 
				// any items on it.
				//
				this.doTopologicalSort = true;
			}

			// MD 11/1/10 - TFS56976
			// Once the dirty chain is cleared, the formulas are now in place in the reclac chain. Any subsequent dirtyings of references 
			// should readd a calculated formula to the recalc chain, so allow the dirtying of parents again.
			this.preventDirtyParents = false;

			
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion //ClearDirtyChain

		#region ClearDirtyChainAndDoTopologicalSort

		private void ClearDirtyChainAndDoTopologicalSort(bool isStartOfRecalcOperation)
		{
			// Something on the dirty chain? If so ClearDirtyChain.
			ClearDirtyChain();

			// Need to sort the recalc chain?
			if (doTopologicalSort)
				TopologicalSort();

			// MD 8/20/08 - Excel formula solving
			if (isStartOfRecalcOperation)
			{
				foreach (KeyValuePair<IExcelCalcReference, object> entry in this.circularReferences)
				{
					ExcelCalcFormula formula = entry.Key.Formula as ExcelCalcFormula;

					if (formula != null)
						formula.ResetCircularFormula();
				}
			}
		}

		#endregion ClearDirtyChainAndDoTopologicalSort

		#region ConnectAncestorReferences



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void ConnectAncestorReferences(UCReference predecessor, UltraCalcReferenceCollection ancestors, bool connect)
		{
			// SSP 7/7/05
			//
			IExcelCalcReference predecessorBaseReference = null;

			foreach (IExcelCalcReference ancestor in ancestors)
			{
				Debug.Assert(ancestor is FormulaReference, "Unexpected member of the ancestors collection.");

				foreach (UCReference ancestorReference in ancestor.Formula.References)
				{
					// SSP 9/7/04
					// Perform a case insensitive comparison since absolute names are case insensitive.
					//
					//if (ancestorReference.AbsoluteName == predecessor.AbsoluteName)
					// SSP 7/7/05
					// Delay reconnecting of predecessor after all the tokens in the ancestors have 
					// been connected/disconnected/reconnected. The reason for this is that these actions
					// can change the predecessor in such a way that it will not be equal anymore to the
					// same tokens on which the action hasn't been performed yet. For example a predecessor
					// in disconnected state will be equal to all disconnected UCRefernece instances with
					// the same absolute name. However once the predecessor is connected, it's absolute name
					// can change in which case the CompareReferences will return false whereas it would
					// have returned true if the predecessor wasn't already connected. This change ofcourse
					// assumes that you can not have two or more UCReference instances that are supposed to 
					// be equal howeever they are in different connected state.
					// The commented out code is moved into the new ConnectReferenceHelper method.
					// 
					// --------------------------------------------------------------------------------------
					if (ancestorReference == predecessor)
						predecessorBaseReference = ancestor;
					else if (ExcelCalcEngine.CompareReferences(ancestorReference, predecessor))
						this.ConnectReferenceHelper(connect, ancestorReference, ancestor);
					
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

					// --------------------------------------------------------------------------------------
				}
			}

			// SSP 7/7/05
			// Related to the change above in the for loop.
			// 
			if (null != predecessorBaseReference)
				this.ConnectReferenceHelper(connect, predecessor, predecessorBaseReference);
		}

		#endregion //ConnectAncestorReferences

		#region ConnectReferenceHelper

		// SSP 7/7/05
		// Added ConnectReferenceHelper method. Code in there is moved from the the existing 
		// ConnectAncestorReferences method.
		// 
		private void ConnectReferenceHelper(bool connect, UCReference reference, IExcelCalcReference baseReference)
		{
			IExcelCalcReference baseReferenceResolved = baseReference is FormulaReference ? ((FormulaReference)baseReference).UnderlyingReference : baseReference;

			bool referenceChanged = false;
			if (connect)
				referenceChanged = reference.Connect(baseReferenceResolved);
			else
				referenceChanged = reference.Disconnect();

			// If the UCReference changed then bump the ancestor map version number.
			//
			if (referenceChanged)
				this.BumpAncestorMapVersion();
		}

		#endregion // ConnectReferenceHelper

		#region ConnectReferences



#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

		private void ConnectReferences(IExcelCalcReference reference, bool connect)
		{
			foreach (UltraCalcAncestorMap.AncestorMapEntry predecessorEntry in this.ancestorMap)
			{
				// Disconnecting?
				if (connect == false)
				{
					if (reference.ContainsReference(predecessorEntry.Predecessor.Reference) && predecessorEntry.Predecessor.IsConnected)
					{
						// Dirty any references to the disconnected reference.  We need to do this before disconnecting so we have the fully 
						// qualified absolute name of the soon to be disconnected references.
						//
						// Note we just dirty references to the resolved predecessor in order to minimally pick up only affected references to the 
						// disconnected row(s) collection.

						DirtyParents(predecessorEntry.Predecessor.Reference, false);

						// Disconnect all references to this predecessor within each its ancestor's formulas.
						ConnectAncestorReferences(predecessorEntry.Predecessor, predecessorEntry.Ancestors, connect);
					}
				}
				// Connecting?
				else
				{
					// Since we don't have the fully qualified absolute name of the disconnected predecessors, we need to attempt to 
					// connect any disconnected reference
					if (!predecessorEntry.Predecessor.IsConnected)
					{
						// First, connect the reference to this predecessor within each its ancestor's formulas.
						ConnectAncestorReferences(predecessorEntry.Predecessor, predecessorEntry.Ancestors, connect);

						// Dirty any references to the predecessor
						DirtyParents(predecessorEntry.Predecessor.Reference, false);
					}
				}
			}

			// Now remove any calculated references contained by the reference from the recalc chain
			if (recalcChain.Count > 0)
			{
				foreach (IExcelCalcReference formulaReference in FormulasSubsetOf(reference, recalcChain))
					recalcChain.Remove(formulaReference);
			}

			// Add any calculated columns contained by the connected or reconnected references to the recalc chain
			if (connect && this.ancestorMap.Count > 0)
			{
				foreach (UltraCalcAncestorMap.AncestorMapEntry predecessorEntry in this.ancestorMap)
				{
					foreach (FormulaReference formulaReference in FormulasContainedBy(reference, predecessorEntry.Ancestors))
					{
						// unwrap the reference first
						IExcelCalcReference formulaRefResolved = formulaReference.UnderlyingReference;

						// AS 9/17/04
						// If the reference in the ancestor map had a formula but the formula
						// on the reference has since been changed, skip this reference.
						if (formulaRefResolved.Formula != formulaReference.Formula)
							continue;

						IExcelCalcReference convReference = formulaRefResolved;

						// SSP 10/6/04
						// Got rid of RecalcListed, RecalcDirty and RecalcMarked flags.
						//
						//if (!convReference.RecalcDirty)
						if (!this.recalcChain.IsRecalcListed(convReference))
							DirtyParents(convReference, true);
					}
				}
			}

		}

		#endregion //ConnectReferences

		#region DirtyAlwaysDirtyList






		public void DirtyAlwaysDirtyList()
		{
			// MD 11/1/10 - TFS56976
			// Wrapped this foreach loop in an if statement. There's no point to creating and disposing an enumerator when there are no items.
			if (this.alwaysDirtyList.Count > 0)
			{
				foreach (IExcelCalcReference alwaysDirtyItem in this.alwaysDirtyList)
					this.DirtyParents(alwaysDirtyItem, true);
			}

			// MD 8/20/08 - Excel formula solving
			// Circular reference formulas are always dirty, so dirty them as well.
			// MD 11/1/10 - TFS56976
			// Wrapped this foreach loop in an if statement. There's no point to creating and disposing an enumerator when there are no items.
			if (this.circularReferences.Count > 0)
			{
				foreach (IExcelCalcReference reference in this.circularReferences.Keys)
					this.DirtyParents(reference, true);
			}
		}

		#endregion DirtyAlwaysDirtyList

		#region DirtyAncestors

		private void DirtyAncestors(IExcelCalcReference predecessor, UltraCalcReferenceCollection ancestors)
		{
			// Add each ancestor to the dirty chain
			foreach (IExcelCalcReference ancestor in ancestors)
			{
				// Resolve ancestor reference against its predecessor
				IExcelCalcReference convAncestor = ((FormulaReference)ancestor).UnderlyingReference;
				Debug.Assert(null != convAncestor, "Null returned from ResolveReference !");

				// Not already on the dirty chain?
				// SSP 10/6/04
				// Got rid of RecalcListed, RecalcDirty and RecalcMarked flags.
				//
				//if (!(convAncestor is UltraCalcReferenceError) && !convAncestor.RecalcDirty) 
				if (null != convAncestor && !(convAncestor is UltraCalcReferenceError))
				{
					if (recalcChain.Add(convAncestor))
					{
					}
				}
			}
		}

		#endregion DirtyAncestors

		#region DirtyParents



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private bool DirtyParents(IExcelCalcReference predecessor, bool inclusive)
		{
			// MD 3/16/09 - TFS14252
			// The method now has a return value.
			bool returnValue = false;

			// unwrap the reference if necessary
			IExcelCalcReference reference = predecessor is FormulaReference ? ((FormulaReference)predecessor).UnderlyingReference : predecessor;

			// AS 9/17/04
			// There is no reason to try and dirty ancestors based on a reference
			// error.
			//
			if (reference is UltraCalcReferenceError)
			{
				// MD 3/16/09 - TFS14252
				// The method now has a return value.
				//return;
				return returnValue;
			}

			// If (object itself should be dirtied)
			if (inclusive)
			{
				// Add it to the end of the dirty chain
				if (recalcChain.Add(reference))
				{
					// MD 3/16/09 - TFS14252
					// If the reference was added to the recalcChain, we should return true.
					returnValue = true;
				}
			}

			// MD 11/1/10 - TFS56976
			// If we can prevent dirtying parents, we can return here. We can't return at the beginning of the method though because when 
			// the inclusive parameter it passed in as True, we always want to add the reference to the recalc chain.
			if (this.preventDirtyParents)
				return false;

			UltraCalcReferenceCollection ancestors = this.ancestorMap.Ancestors(reference);
			returnValue = ancestors.Count != 0;
			this.DirtyAncestors(reference, ancestors);

			// MD 3/16/09 - TFS14252
			// The method now has a return value.
			return returnValue;
		}

		#endregion //DirtyParents

		#region Evaluate

		// SSP 9/27/04
		// Implemented being able to use relative indexes in a circular way like
		// in A=B(-1), B=A formulas and evaluating formulas across rows.
		// Rewrote the Evaluate method. Original code is commented out.
		//





		private void Evaluate()
		{
			// MD 8/12/08 - Excel formula solving
			// If we are already evaluating a formula, don't do anything.
			if (this.isEvaluating)
			{
				CalcManagerUtilities.DebugFail("Evaluate should not be called when we are already evaluating.");
				return;
			}

			// MD 8/12/08 - Excel formula solving
			// Set the anti-recursion flag. Wrapped the code below in a try...finally, so the flag could be reset after the method is done.
			this.isEvaluating = true;
			try
			{
				// MD 7/25/08 - Excel formula solving
				int referenceIndex = 0;

				// MD 1/25/12 - TFS100119
				// The IgnoreRecalcList was a relatively slow way of doing things when dynamic references were added. 
				// Now we will instead reorder the recalc chain when we add a dynamic reference to another reference 
				// marked for recalculation. But in some cases, we may detect a new circularity when the dynamic 
				// reference is added. In these cases, we really should resort all references and start another 
				// calculation loop.
				#region Old Code

				//// MD 7/25/08 - Excel formula solving
				//// If the ignore list is the same length as the recalc list, all items in the reclac list should be ignored.
				//// Since the ignore list tells us which items must be skipped for the new dependany order, no predecessors
				//// can be later in the recalc list, so they can all be processed again. Clear the ignore list.
				//if ( this.ignoreRecalcList != null && this.ignoreRecalcList.Count >= this.recalcChain.Count )
				//{

				//    Debug.Assert(this.ignoreRecalcList.Count <= this.recalcChain.Count + 1,
				//        "Since all items in the ignoreRecalcList should be from the recalcChain, there should never be more items in the ignore list (with the exception of the last item that was evaluated.");

				//    this.ignoreRecalcList.Clear();

				//    // MD 8/20/08 - Excel formula solving
				//    // If any new circular references were created as a result of an added dynamic reference, the ignore recalc list will containe
				//    // all the items in the circularity (as well as others possibly). Once the ignore list is cleared, any new circularities in the
				//    // reclac chain must be resorted.
				//    if ( this.VerifyAncestorMap() )
				//        this.TopologicalSort();
				//}

				//IUltraCalcReference reference = null;

				//// Determine which reference should be recalculated.
				//for ( ; referenceIndex < this.recalcChain.Count; referenceIndex++ )
				//{
				//    // Start with the first item in the recalc list.
				//    reference = this.recalcChain[ referenceIndex ];

				//    // If the reference is in the ignore list, ignore it and go to the next item in the recalc list.
				//    if ( this.IgnoreRecalcList.ContainsKey( UltraCalcEngine.GetResolvedReference( reference ) ) )
				//        continue;

				//    // Exit the loop if the reference was not in the ignore list.
				//    break;
				//}

				//// The reference should never be null.
				//if ( reference == null )
				//{
				//    Utilities.DebugFail( "The reference should never be null here." );
				//    return;
				//}

				#endregion // Old Code
				if (this.detectedNewCircularity)
				{
					if (this.VerifyAncestorMap())
						this.TopologicalSort();
				}

				IExcelCalcReference reference = this.recalcChain[referenceIndex];
				ExcelCalcFormula referenceFormula = (ExcelCalcFormula)reference.Formula;

				if (null == referenceFormula)
				{
					this.recalcChain.RemoveAt(referenceIndex);
					Utilities.DebugFail("A reference without a formula encountered in the recalc chain.");
					return;
				}

				// Is the reference to a cell?
				if (reference.IsEnumerable == false)
				{
					// MD 7/25/08 - Excel formula solving
					// Don't remove the item just yet. If the evaluation of the formula adds new dynamic references to the 
					// formula and some of them are dirty, the formula will need to be reevaluated later and should not be
					// removed from the recalc list.
					//this.recalcChain.RemoveAt( 0 );

					// Evaluate the reference and store its value 
					// MD 7/25/08 - Excel formula solving
					// Cache the result before setting it on the reference. If a new dynamic reference added to the formula is 
					// dirty, the returned value is junk and should not be used.
					//reference.Value = referenceFormula.Evaluate(reference);
					// MD 8/20/08 - Excel formula solving
					// We cannot just evaluate the formula. If it is a circular formula and iterative calculations are disabled, it must
					// get a circularity error.
					//UltraCalcValue value = referenceFormula.Evaluate( reference );
					ExcelCalcValue value;

					int originalCount = this.recalcChain.Count;
					Debug.Assert(referenceIndex < this.recalcChain.Count);

					try
					{
						// Notify the formula that it will begin a single evaluation iteration
						referenceFormula.OnBeforeEvaluate();

						Debug.Assert(originalCount == this.recalcChain.Count);

						// If the formula is a circular formula and iterative calculations are disabled, it must get a circularity error.
						// Otherwise, evaluate the formula.
						if (referenceFormula.IsCircularFormula && this.workbook.IterativeCalculationsEnabled == false)
						{
							// MD 3/2/12 - 12.1 - Table Support
							if (ExcelCalcEngine.GetResolvedReference(reference) is NamedCalcReferenceBase)
							{
								value = null;
							}
							else
							{
								value = new ExcelCalcValue(new ExcelCalcErrorValue(ExcelCalcErrorCode.Circularity));
							}
						}
						else
						{
							value = referenceFormula.Evaluate(reference);
						}

						// MD 4/2/12 - TFS99854
						// We actually check this below, so we don't need to check it again here.
						//Debug.Assert( originalCount == this.recalcChain.Count );

						// MD 12/1/11 - TFS96540
						// This has been moved below. We were calling OnAfterEvaluate too early. This method removes stale dynamic references
						// from the ancestor map, but we haven't yet dereferenced the return value (if it is a reference). That operation could
						// end up adding a new dynamica reference and if that reference was contained in the style dynamic references collection,
						// it shouldn't be removed from the ancestor map.
						// However, I did leave the debug assert here, because it will fail below.
						#region Moved Below

						//                    }
						//                    finally
						//                    {
						//#if EXCEL
						//                        // Notify the formula that it has finished a single evaluation iteration
						//                        referenceFormula.OnAfterEvaluate();

						//                        Debug.Assert( originalCount == this.recalcChain.Count );
						//#endif
						//                    }

						#endregion // Moved Below
						Debug.Assert(originalCount == this.recalcChain.Count, "The reclacChain is out of sync.");


						// If no new dynamic references added to the formula are dirty, the calculated value can safely be used.
						if (referenceFormula.DynamicReferenceIsDirty == false)
						{
							// MD 4/2/12 - TFS99854
							// Make sure the recalcChain wasn't re-ordered.
							Debug.Assert(this.recalcChain[referenceIndex] == reference, "The recalc chain order has changed.");

							// MD 7/25/08 - Excel formula solving
							// Remove the item at the index where the recalculation was occurring and set the calculated value on the 
							// reference.
							this.recalcChain.RemoveAt(referenceIndex);

							// MD 8/20/08 - Excel formula solving
							// The value will now be set in the AfterFormulaEvaluationCompleted method
							//reference.Value = value;
							this.AfterFormulaEvaluationCompleted(reference, referenceFormula, value);
						}

						// MD 12/1/11 - TFS96540
						// Moved this finally block from above:
						// We were calling OnAfterEvaluate too early. This method removes stale dynamic references from the ancestor map, but we didn't 
						// dereferenced the return value (if it is a reference) above. That operation could end up adding a new dynamica reference and if
						// that reference was contained in the style dynamic references collection, it shouldn't be removed from the ancestor map.
						// So we will do this now, after the call to AfterFormulaEvaluationCompleted, which will dereference and apply the evaluated value.
					}
					finally
					{
						// Notify the formula that it has finished a single evaluation iteration
						referenceFormula.OnAfterEvaluate(this);
					}
				}
				else
				{
					// MD 8/22/08 - Excel formula solving - Performance
					// This should never happen in the Excel assembly.
					Utilities.DebugFail("We should never try to evaluate enumerable references in Excel");
				}

				// MD 8/12/08 - Excel formula solving
				// Reset the anti-recursion flag when the method is done.
			}
			finally
			{
				this.isEvaluating = false;
			}
		}

		#endregion // Evaluate

		#region FormulasContainedBy



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private UltraCalcReferenceCollection FormulasContainedBy(IExcelCalcReference baseReference, UltraCalcReferenceCollection referenceCollection)
		{
			UltraCalcReferenceCollection referenceList = new UltraCalcReferenceCollection();

			foreach (IExcelCalcReference reference in referenceCollection)
			{
				// unwrap any ancestor references so we never call
				// the reference's contains reference with the wrapped object
				IExcelCalcReference referenceResolved = reference is FormulaReference ? ((FormulaReference)reference).UnderlyingReference : reference;

				if (baseReference.ContainsReference(referenceResolved))
					referenceList.Add(reference);
			}

			return referenceList;
		}

		#endregion // FormulasContainedBy

		#region FormulasSubsetOf



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private UltraCalcReferenceCollection FormulasSubsetOf(IExcelCalcReference baseReference, IExcelCalcReferenceCollection referenceCollection)
		{
			UltraCalcReferenceCollection referenceList = new UltraCalcReferenceCollection();
			foreach (IExcelCalcReference reference in referenceCollection)
			{
				// SSP 7/13/05
				// Just something I noticied. We need to get the UnderlyingReference if the reference
				// is a FormulaReference because that's what we should be passing into the 
				// IsSubsetReference.
				// 
				// --------------------------------------------------------------------------------------
				//if (baseReference.IsSubsetReference(reference))
				//	referenceList.Add(reference);

				// unwrap any ancestor references so we never call
				// the reference's contains reference with the wrapped object
				IExcelCalcReference referenceResolved = reference is FormulaReference ? ((FormulaReference)reference).UnderlyingReference : reference;

				if (baseReference.IsSubsetReference(referenceResolved))
					referenceList.Add(reference);
				// --------------------------------------------------------------------------------------
			}

			return referenceList;
		}

		#endregion //FormulasSubsetOf

		#region GetFormulas







		private ReferenceHolder[] GetFormulas()
		{
			HashSet formulas = new HashSet();

			foreach (UltraCalcAncestorMap.AncestorMapEntry entry in this.ancestorMap)
			{
				foreach (FormulaReference formulaReference in entry.Ancestors)
				{
					ExcelCalcFormula formula = (ExcelCalcFormula)formulaReference.Formula;

					Debug.Assert(null != formula && null != formula.BaseReference
						&& formula == formula.BaseReference.Formula);

					if (null != formula && null != formula.BaseReference && !formulas.Exists(formula))
						formulas.Add(formula);
				}
			}

			ReferenceHolder[] formulaReferences = new ReferenceHolder[formulas.Count];
			int index = 0;

			foreach (ExcelCalcFormula formula in formulas)
				formulaReferences[index++] = new ReferenceHolder(formula.BaseReference, formula);

			return formulaReferences;
		}

		#endregion // GetFormulas

		#region IsDirty

		private bool IsDirty(IExcelCalcReference reference, bool wait, bool isStartOfRecalcOperation)
		{
			this.VerifyAncestorMap();

			// MD 7/22/08
			// Found while implementing Excel formula solving
			// The recalcChain may be incomplete at this point. Moved some logic from Recalc into a helper and called
			// it from here so we complete the recalcChain before searching it for the passed in reference.
			// MD 8/17/08 - Excel formula solving
			// Performance - We should only clear and sort the dirty chain if the reclac chain has items in it.
			//this.ClearDirtyChainAndDoTopologicalSort();
			if (this.recalcChain.Count > 0 || this.circularReferences.Count != 0)
			{
				// MD 8/20/08 - Excel formula solving
				// Pass off the new parameter
				//this.ClearDirtyChainAndDoTopologicalSort();
				this.ClearDirtyChainAndDoTopologicalSort(isStartOfRecalcOperation);
			}

			if (wait)
			{
				// SSP 10/7/04
				//
				//while ( this.recalcChain.IsRecalcListed( reference ) && Recalc(recalcTaskTicks));
				// MD 8/20/08 - Excel formula solving
				// Pass off false as the new parameter to both new overloads of IsDirty and Recalc so a new recalc operation
				// is not started while iterating the while loop.
				//while ( this.IsDirty( reference, false ) && Recalc( UltraCalcEngine.RecalcTaskTicks ) );

				// MD 5/1/11
				// Do the full calculations in one shot for Excel since we never do time slicing and doing the calculations
				// in slices seems to cause problems with circular references at times.
				//while (this.IsDirty(reference, false, false) && this.Recalc(UltraCalcEngine.RecalcTaskTicks, false)) ; 
				while (this.IsDirty(reference, false, false) && this.Recalc(false)) ;

				return false;
			}
			else
			{
				// SSP 10/7/04
				// Instead of checking the flag call IsRecalcListed method.
				//
				return this.recalcChain.IsRecalcListed(reference);
			}
		}

		#endregion //IsDirty

		#region ProcessAddFormula

		private void ProcessAddFormula(IExcelCalcFormula formula, IExcelCalcReference baseReference)
		{
			// MD 8/21/08 - Excel formula solving
			IExcelCalcReference resolvedBaseReference = ExcelCalcEngine.GetResolvedReference(baseReference);

			// In excel, searching for the reference in the ancestor is much slower because each cell has its own formula and reference,
			// so instead use a dictionary.
			IExcelCalcFormula previousFormula;
			if (this.formulasByReference.TryGetValue(resolvedBaseReference, out previousFormula))
			{
				if (previousFormula == formula)
					return;

				this.ProcessDeleteFormula(previousFormula, baseReference);
			}

			// AS 9/17/04
			// If the formula has since changed, do not add it.
			//
			if (baseReference.Formula != formula)
				return;

			// MD 8/21/08 - Excel formula solving
			Debug.Assert(this.formulasByReference.ContainsKey(resolvedBaseReference) == false, "The formula should not be in the collection already.");
			this.formulasByReference[resolvedBaseReference] = formula;

			// MD 3/1/12 - 12.1 - Table Support
			this.UpdateReferencedTableMap(formula, baseReference, true);

			// Get the predecessor collection
			bool noReferences = true;
			foreach (UCReference reference in formula.References)
			{
				// SSP 7/12/05
				// We need to remark the tokens of a formula as whenever a token gets connected. This is because
				// UCReference when disconnected always returns false from HasRelativeIndex and HasAbsoluteIndex
				// causing the base reference' RefParser tuples to be not 'marked'. However when the token gets 
				// connected we need to mark the tuples connected again.
				// This is now being done VerifyAncestorMap since we need to re-do this every time a token
				// gets connected.
				// 
				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


				ancestorMap.AddAncestor(reference, baseReference);
				noReferences = false;
				// Force any predecessor who is currently on the recalc chain to be dirty so the topological sort will move this new ancestor
				// behind it.
				//??? NOW BEING DONE BY DIRTY CHILDREN IN DIRTYPARENT W/INCLUSIVE OPTION
				//if (reference.RecalcListed && !reference.RecalcDirty)
				//	reference.RecalcDirty = true;
			}

			// Calculated references without RHS references are made ancestors of a dummy entry so they can be found when needed.
			if (noReferences)
			{
				UCReference reference = new UCReference(".");
				ancestorMap.AddAncestor(reference, baseReference);
			}

			if (formula.HasAlwaysDirty)
				alwaysDirtyList.Add(new FormulaReference(baseReference));

			// Add object to the dirty chain
			DirtyParents(baseReference, true);

			this.BumpAncestorMapVersion();
		}

		#endregion // ProcessAddFormula

		#region ProcessDeleteFormula

		private void ProcessDeleteFormula(IExcelCalcFormula formula, IExcelCalcReference baseReference)
		{
			// MD 8/21/08 - Excel formula solving
			this.formulasByReference.Remove(ExcelCalcEngine.GetResolvedReference(baseReference));

			// MD 3/1/12 - 12.1 - Table Support
			this.UpdateReferencedTableMap(formula, baseReference, false);

			bool noReferences = false;
			// Remove the formula from each of its predecessor's ancestor list
			foreach (IExcelCalcReference reference in formula.References)
			{
				ancestorMap.DeleteAncestor(reference, baseReference);
				noReferences = true;
			}

			// MD 7/25/08 - Excel formula solving
			// Remove all dynamic references added to the formula on the last evaluation from the ancestor map.
			foreach (IExcelCalcReference reference in formula.DynamicReferences)
				ancestorMap.DeleteAncestor(reference, baseReference);

			// Calculated reference without RHS references are made ancestors of a dummy entry so they can be found when needed.
			if (!noReferences)
			{
				// Remove from this list.
				UCReference reference = new UCReference(".");
				ancestorMap.DeleteAncestor(reference, baseReference);
			}


			// Remove any always dirty reference
			if (formula.HasAlwaysDirty)
				alwaysDirtyList.Remove(baseReference);

			// SSP 10/7/04
			// Remove the reference and any subset references (column reference as well as cells belonging to it).
			//
			this.recalcChain.RemoveSubsetReferences(baseReference);

			this.circularReferences.Remove(baseReference);

			// Dirty the formula object's parents
			DirtyParents(baseReference, false);

			// Bump the acnestor map version number so we can reassign the formula sort numbers and other
			// info to all the formulas.
			//
			this.BumpAncestorMapVersion();
		}

		#endregion //ProcessDeleteFormula

		#region Recalc

		// MD 8/20/08 - Excel formula solving
		// Added new parameter to indicate whether this is the start if a new recalc operation.
		private bool Recalc(bool isStartOfRecalcOperation)
		{
			this.VerifyAncestorMap();
			this.ClearDirtyChainAndDoTopologicalSort(isStartOfRecalcOperation);

			while (this.HasMoreToCalc)
				Evaluate();

			// MD 2/3/12 - TFS100119
			// If any dynamic references were added while processing the formulas, the ancestor map may need to be resorted.
			// In the past, we were resorting and reevaluating everything when dynamic references were added. But as part of
			// a performance enhancement for TFS100119, we stopped doing that and now just move references taking on 
			// non-calculated dynamic references (and all their dependents) to the end of the recalculation chain. But this
			// does not reassign sort numbers to the references, so we need to do that here.
			this.VerifyAncestorMap();
			return false;
		}

		#endregion //Recalc

		#region TopologicalSort



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

		private void TopologicalSort()
		{
			this.recalcChain.Sort(new RecalcChainSortComparer());

			// Denote we've sorted the list.
			//
			this.doTopologicalSort = false;

			// MD 1/25/12 - TFS100119
			// Reset the flag indicating that a new circularity was added dynamically.
			this.detectedNewCircularity = false;
		}

		#endregion // TopologicalSort

		#region UpdateReferencedTableMap

		private void UpdateReferencedTableMap(IExcelCalcFormula formula, IExcelCalcReference baseReference, bool isAdding)
		{
			ExcelCalcFormula excelFormula = formula as ExcelCalcFormula;
			if (excelFormula == null)
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}

			Dictionary<WorksheetTable, bool> tablesFound = new Dictionary<WorksheetTable, bool>();

			foreach (UltraCalcFormulaToken token in excelFormula.Tokens)
			{
				if (token.Type != UltraCalcFormulaTokenType.Value)
					continue;

				UltraCalcValueToken valueToken = (UltraCalcValueToken)token;
				if (valueToken.Value.IsReference == false)
					continue;

				TableCalcReferenceBase tableReference =
					ExcelCalcEngine.GetResolvedReference(valueToken.Value.ToReference()) as TableCalcReferenceBase;
				if (tableReference == null)
					continue;

				WorksheetTable table = tableReference.Table;
				if (table == null || tablesFound.ContainsKey(table))
					continue;

				tablesFound.Add(table, true);

				List<IExcelCalcReference> formulasReferencingTable;
				if (this.referencedTableMap.TryGetValue(table, out formulasReferencingTable) == false)
				{
					if (isAdding == false)
						continue;

					formulasReferencingTable = new List<IExcelCalcReference>();
					this.referencedTableMap[table] = formulasReferencingTable;
				}

				if (isAdding)
					formulasReferencingTable.Add(baseReference);
				else
					formulasReferencingTable.Remove(baseReference);
			}
		}

		#endregion // UpdateReferencedTableMap

		#region VerifyAncestorMap



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private bool VerifyAncestorMap()
		{
			if (this.verifiedAncestorMapVersion == this.ancestorMapVersion)
			{
				// MD 8/20/08 - Excel formula solving
				// Changed the return type to a bool
				//return;
				return false;
			}

			this.verifiedAncestorMapVersion = ancestorMapVersion;

			// MD 4/2/12 - TFS99854
			// Cache another dependency graph so we can easily find ancestor formulas later.
			this.dependencyGraph = new DependencyGraph(this, false);

			// MD 4/21/11 - TFS64442
			// We can do things a lot faster by doing a topological sort on a directed graph of the dependencies.
			// This will find circularities as well.
			// MD 4/2/12 - TFS99854
			// Added another parameter to the constructor which indicates whether populating the graph should re-initialize 
			// the formula's DependancySortNumber and IsCircularReference values.
			//DependencyGraph graph = new DependencyGraph(this);
			DependencyGraph graph = new DependencyGraph(this, true);

			// MD 3/2/12 - TFS103729
			// This method now has an out parameter which indicates the next dependency sort number which would have 
			// been given out.
			//graph.TopologicalSort();
			// MD 4/2/12 - TFS99854
			// Set preventDirtyParents to True so we don't dirty ancestors when each reference is dirtied during the 
			// topological sort. We are going to dirty everything anyway.
			//graph.TopologicalSort(out this.nextDependencySortNumber);
			bool oldPreventDirtyParents = this.preventDirtyParents;
			try
			{
				this.preventDirtyParents = true;
				graph.TopologicalSort(out this.nextDependencySortNumber);
			}
			finally
			{
				this.preventDirtyParents = oldPreventDirtyParents;
			}

			// MD 8/20/08 - Excel formula solving
			// Changed the return type to a bool
			return true;
		}

		#endregion // VerifyAncestorMap

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region FunctionFactory



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		public UltraCalcFunctionFactory FunctionFactory
		{
			get { return this.functionFactory; }
		}

		#endregion // FunctionFactory

		#endregion // Public Properties

		#region Private Properties

		#region HasMoreToCalc







		private bool HasMoreToCalc
		{
			get { return this.recalcChain.Count > 0; }
		}

		#endregion // HasMoreToCalc

		#endregion // Private Properties

		#endregion // Properties


		#region DependencyGraph class

		private class DependencyGraph
		{
			#region Member Variables

			private ExcelCalcEngine calcEngine;
			private Dictionary<Vertex, byte> verticiesNotDependingOnOthers;
			private Dictionary<IExcelCalcReference, Vertex> vertices;

			#endregion  // Member Variables

			#region Constructor

			// MD 4/2/12 - TFS99854
			// Added another parameter to the constructor which indicates whether populating the graph should re-initialize 
			// the formula's DependancySortNumber and IsCircularReference values.
			//public DependencyGraph(UltraCalcEngine calcEngine)
			public DependencyGraph(ExcelCalcEngine calcEngine, bool resetFormulaState)
			{
				this.calcEngine = calcEngine;

				this.vertices = new Dictionary<IExcelCalcReference, Vertex>();
				this.verticiesNotDependingOnOthers = new Dictionary<Vertex, byte>();

				// Construct the directed graph from the ancestor map.
				// MD 4/2/12 - TFS99854
				// We didn't need to store the Vertex instances by Formula, because we already store them by reference and we
				// can get the reference from the formula.
				//Dictionary<UltraCalcFormulaBase, Vertex> formulaVertexCache = new Dictionary<UltraCalcFormulaBase, Vertex>();
				foreach (UltraCalcAncestorMap.AncestorMapEntry entry in this.calcEngine.ancestorMap)
				{
					// MD 9/27/11 - TFS89243
					// Enumerable references can't have formulas of their own in Excel. We should instead loop through their lazily 
					// created references and find formula for those instead, so this has been moved to a helper method which does that.
					//UltraCalcFormulaBase sourceFormula = (UltraCalcFormulaBase)entry.Predecessor.Formula;
					//Vertex sourceVertex = this.GetVertex(formulaVertexCache, sourceFormula);
					List<Vertex> sourceVertices = null;

					// MD 4/2/12 - TFS99854
					//this.GetSourceVertices(formulaVertexCache, entry.Predecessor, ref sourceVertices);
					this.GetSourceVertices(entry.Predecessor.Reference, ref sourceVertices, resetFormulaState);

					foreach (FormulaReference formulaReference in entry.Ancestors)
					{
						ExcelCalcFormula dependentFormula = (ExcelCalcFormula)formulaReference.Formula;

						Debug.Assert(null != dependentFormula && null != dependentFormula.BaseReference && dependentFormula == dependentFormula.BaseReference.Formula, "Something is wrong here.");
						if (dependentFormula == null || dependentFormula.BaseReference == null)
							continue;

						// MD 4/2/12 - TFS99854
						//Vertex dependentVertex = this.GetVertex(formulaVertexCache, dependentFormula);
						Vertex dependentVertex = this.GetVertex(formulaReference.Formula.BaseReference, resetFormulaState);

						// MD 9/27/11 - TFS89243
						// We may have multiple references, so add relationships for each.
						//if (sourceVertex != null)
						//    this.AddDependencyRelationship(sourceVertex, dependentVertex);
						if (sourceVertices != null)
						{
							for (int i = 0; i < sourceVertices.Count; i++)
								this.AddDependencyRelationship(sourceVertices[i], dependentVertex);
						}
					}
				}
			}

			#endregion  // Constructor

			#region Methods

			// MD 4/2/12 - TFS99854
			public void AddDependency(IExcelCalcReference predecessor, IExcelCalcReference ancestor)
			{
				List<Vertex> sourceVertices = null;
				this.GetSourceVertices(predecessor, ref sourceVertices, false);

				Vertex dependentVertex;
				if (sourceVertices != null && this.vertices.TryGetValue(ancestor, out dependentVertex))
				{
					for (int i = 0; i < sourceVertices.Count; i++)
						sourceVertices[i].AddDependent(dependentVertex);
				}
			}

			private void AddDependencyRelationship(Vertex source, Vertex dependent)
			{
				// If the dependant previously doesn't depend on others, remove it from the verticiesNotDependingOnOthers
				// collection because it is about to depend on another vertex.
				if (dependent.DoesDependOnOthers == false)
					this.verticiesNotDependingOnOthers.Remove(dependent);

				source.AddDependent(dependent);
			}

			// MD 10/7/11 - TFS91103
			private void AddDependentsRecursive(Vertex vertex, List<Vertex> dependents)
			{
				if (vertex.visited)
					return;

				vertex.visited = true;
				dependents.Add(vertex);

				List<Vertex> nestedDependents = vertex.Dependents;
				for (int i = 0; i < nestedDependents.Count; i++)
					this.AddDependentsRecursive(nestedDependents[i], dependents);
			}

			// MD 4/2/12 - TFS99854
			public Dictionary<IExcelCalcReference, bool> GetAncestors(IExcelCalcReference reference)
			{
				Dictionary<IExcelCalcReference, bool> ancestors = new Dictionary<IExcelCalcReference, bool>();

				Vertex vertex;
				if (this.vertices.TryGetValue(reference, out vertex))
					this.GetAncestors(vertex, ancestors);

				return ancestors;
			}

			// MD 4/2/12 - TFS99854
			private void GetAncestors(Vertex vertex, Dictionary<IExcelCalcReference, bool> ancestors)
			{
				IExcelCalcReference reference = vertex.ReferenceHolder.Reference;
				if (ancestors.ContainsKey(reference))
					return;

				ancestors[reference] = true;
				for (int i = 0; i < vertex.Dependents.Count; i++)
					this.GetAncestors(vertex.Dependents[i], ancestors);
			}

			// MD 9/27/11 - TFS89243
			// MD 4/2/12 - TFS99854
			//private void GetSourceVertices(Dictionary<UltraCalcFormulaBase, Vertex> formulaVertexCache, 
			private void GetSourceVertices(
				IExcelCalcReference reference,
				ref List<Vertex> sourceVertices,
				bool resetFormulaState)
			{
				// MD 4/2/12 - TFS99854
				if (reference == null)
					return;

				if (reference.IsEnumerable)
				{
					foreach (IExcelCalcReference subReference in reference.References)
					{
						// MD 4/2/12 - TFS99854
						//this.GetSourceVertices(formulaVertexCache, subReference, ref sourceVertices);
						this.GetSourceVertices(subReference, ref sourceVertices, resetFormulaState);
					}

					return;
				}

				// MD 4/2/12 - TFS99854
				//UltraCalcFormulaBase sourceFormula = (UltraCalcFormulaBase)reference.Formula;
				//Vertex sourceVertex = this.GetVertex(formulaVertexCache, sourceFormula);
				Vertex sourceVertex = this.GetVertex(reference, resetFormulaState);

				if (sourceVertex == null)
					return;

				if (sourceVertices == null)
					sourceVertices = new List<Vertex>();

				sourceVertices.Add(sourceVertex);
			}

			// MD 4/2/12 - TFS99854
			// These are no longer used.
			#region Removed

			//private Vertex GetVertex(ReferenceHolder holder)
			//{
			//    return this.GetVertex(holder.Reference);
			//}

			//private Vertex GetVertex(IUltraCalcReference reference)
			//{
			//    Vertex vertex;
			//    this.vertices.TryGetValue(reference, out vertex);
			//    return vertex;
			//}

			#endregion // Removed

			// MD 4/2/12 - TFS99854
			//private Vertex GetVertex(Dictionary<UltraCalcFormulaBase, Vertex> formulaVertexCache, UltraCalcFormulaBase formula)
			private Vertex GetVertex(IExcelCalcReference reference, bool resetFormulaState)
			{
				ExcelCalcFormula formula = (ExcelCalcFormula)reference.Formula;

				if (formula == null)
					return null;

				Vertex vertex;

				// MD 4/2/12 - TFS99854
				//if (formulaVertexCache.TryGetValue(formula, out vertex) == false)
				if (this.vertices.TryGetValue(reference, out vertex) == false)
				{
					ReferenceHolder formulaReferenceHolder = new ReferenceHolder(formula.BaseReference, formula);

					// MD 4/2/12 - TFS99854
					// Only do this if resetFormulaState is True.
					//// Reset the fields which the DependencyGraph will initialize.
					//formula.DependancySortNumber = -1;
					//formula.IsCircularFormula = false;
					if (resetFormulaState)
					{
						// Reset the fields which the DependencyGraph will initialize.
						formula.DependencySortNumber = -1;
						formula.IsCircularFormula = false;
					}

					// MD 3/2/12 - 12.1 - Table Support
					formula.IsNamedReferenceCircularFormula = false;

					vertex = new Vertex(formulaReferenceHolder);
					this.vertices.Add(formulaReferenceHolder.Reference, vertex);
					this.verticiesNotDependingOnOthers.Add(vertex, 0);

					// MD 4/2/12 - TFS99854
					//formulaVertexCache.Add(formula, vertex);
				}

				return vertex;
			}

			// MD 10/7/11 - TFS91103
			private static bool IsCycle(List<Vertex> stronglyConnectedComponent)
			{
				if (stronglyConnectedComponent.Count > 1)
					return true;

				if (stronglyConnectedComponent.Count == 0)
					return false;

				Vertex vertex = stronglyConnectedComponent[0];
				return vertex.Dependents.Contains(vertex);
			}

			// MD 4/2/12 - TFS99854
			public void RemoveDependency(IExcelCalcReference predecessor, IExcelCalcReference ancestor)
			{
				List<Vertex> sourceVertices = null;
				this.GetSourceVertices(predecessor, ref sourceVertices, false);

				Vertex dependentVertex;
				if (sourceVertices != null && this.vertices.TryGetValue(ancestor, out dependentVertex))
				{
					for (int i = 0; i < sourceVertices.Count; i++)
						sourceVertices[i].RemoveDependency(dependentVertex);
				}
			}

			// MD 10/7/11 - TFS91103
			private void RemoveVertex(Vertex vertex)
			{
				this.vertices.Remove(vertex.ReferenceHolder.Reference);
				this.calcEngine.DirtyParents(vertex.ReferenceHolder.Reference, true);
			}

			// MD 10/7/11 - TFS91103
			private static void SortAncestorsWithPostOrderTraversal(IEnumerable<Vertex> verticies, List<Vertex> sortedVertices)
			{
				foreach (Vertex vertex in verticies)
				{
					if (vertex.visited)
						continue;

					vertex.visited = true;

					if (vertex.DoesDependOnOthers)
						DependencyGraph.SortAncestorsWithPostOrderTraversal(vertex.DependsUponVertices, sortedVertices);

					sortedVertices.Add(vertex);
				}
			}

			// MD 10/7/11 - TFS91103
			// Made this an instance method.
			//private static void SortNonCircularReferences(Queue<Vertex> verticiesNotDependingOnOthers, ref int sortNumber)
			private void SortNonCircularReferences(Queue<Vertex> verticiesNotDependingOnOthers, ref int sortNumber)
			{
				while (verticiesNotDependingOnOthers.Count > 0)
				{
					Vertex vertex = verticiesNotDependingOnOthers.Dequeue();
					vertex.ReferenceHolder.Formula.DependencySortNumber = sortNumber++;

					// Now that this item is sorted, we can remove its edges from the graph to find the next set of vertices that don't depend 
					// on the rest of the unsorted vertices.
					for (int i = vertex.Dependents.Count - 1; i >= 0; i--)
					{
						Vertex target = vertex.RemoveDependencyAt(i);

						if (target.DoesDependOnOthers == false)
							verticiesNotDependingOnOthers.Enqueue(target);
					}

					// MD 10/7/11 - TFS91103
					// Since the vertex is no depending on others and we've removed all dependencies to this vertex, it can be removed from 
					// the graph.
					this.RemoveVertex(vertex);
				}
			}

			// MD 3/2/12 - TFS103729
			//public void TopologicalSort()
			public void TopologicalSort(out int nextDependencySortNumber)
			{
				this.calcEngine.circularReferences.Clear();

				Queue<Vertex> verticiesNotDependingOnOthersCopy = new Queue<Vertex>(this.verticiesNotDependingOnOthers.Keys);

				int sortNumber = 0;

				// MD 10/7/11 - TFS91103
				// Made this an instance method.
				//DependencyGraph.SortNonCircularReferences(verticiesNotDependingOnOthersCopy, ref sortNumber);
				this.SortNonCircularReferences(verticiesNotDependingOnOthersCopy, ref sortNumber);

				// MD 10/7/11 - TFS91103
				// Removed this code. It was sorting and marking circularities incorrectly (it would mark non-circularities which
				// depend on circular chains as circularities). Also, we were doing an extra loop to dirty all references at the 
				// end, but we can do this while pruning and sorting the graph.
				//
				#region Old Code

				//// Determine if there are any circularities
				//List<Vertex> circularities = new List<Vertex>();
				//foreach (Vertex vertex in this.vertices.Values)
				//{
				//    // If nothing depends on this vertex, it is not in a circular reference chain, so skip it.
				//    // (But it does depend on other references in the circular chain, maybe indirectly).
				//    if (vertex.Dependents.Count == 0)
				//        continue;

				//    circularities.Add(vertex);

				//    vertex.ReferenceHolder.Formula.IsCircularFormula = true;
				//    this.calcEngine.circularReferences[vertex.ReferenceHolder.Reference] = null;

				//    // The first circular reference should be cached
				//    if (this.calcEngine.circularReference == null)
				//        this.calcEngine.circularReference = vertex.ReferenceHolder.Formula.BaseReference;
				//}

				//if (circularities.Count != 0)
				//{
				//    // Sort the circularities
				//    circularities.Sort(new CircularitySorter());

				//    // Set the sort number for the circularities and remove their edges from the graph so the non-circular references
				//    // which depend on them can get sorted.
				//    foreach (Vertex vertex in circularities)
				//    {
				//        vertex.ReferenceHolder.Formula.DependancySortNumber = sortNumber++;

				//        for (int i = vertex.Dependents.Count - 1; i >= 0; i--)
				//        {
				//            Vertex target = vertex.RemoveDependencyAt(i);

				//            if (target.DoesDependOnOthers == false && circularities.Contains(target) == false)
				//                verticiesNotDependingOnOthersCopy.Enqueue(target);
				//        }
				//    }

				//    // Sort the non-circular references which dependeded on the circularities
				//    DependencyGraph.SortNonCircularReferences(verticiesNotDependingOnOthersCopy, ref sortNumber);
				//}

				//// Set the BaseReferenceHolder to null on all the formulas after the process of ancestor
				//// map verification is finished.
				//foreach (Vertex vertex in this.vertices.Values)
				//{
				//    // SSP 12/16/04 BR01209
				//    // We need to add formulas whose recalc deferred state goes from true to false
				//    // to the recalc chain so they get recalculated fully. The main reason for doing
				//    // this is that if a new formula gets added that refers to an existing recalc
				//    // deferred formula then we need to make sure that the whole of the existing
				//    // formula gets calculated before the added formula is calculated. For example,
				//    // if a summary is added that refers to an existing recalc deferred column formula
				//    // that has been cleared off the recalc chain, we need to readd it so all the 
				//    // cells of the column get calculated because only the ones that came into view 
				//    // would have gotton calculated.
				//    this.calcEngine.DirtyParents(vertex.ReferenceHolder.Reference, true);
				//}

				#endregion  // Old Code

				// MD 10/7/11 - TFS91103
				// The remaining vertices in the graph are either in cycles or they depend on cycles. We will now find the strong 
				// components (cycles) and by doing so, we will also find the dependency order between the cycles as the items which
				// depend on them.
				//
				// Start by doing a depth-first search through the nodes and ordering the nodes in a post-order traversal of the nodes.
				// This will use the DependsUponVertices collection because we want to traverse through the ancestor paths. This will
				// cause the ancestors to be earlier in the list than their dependents when possible. Obviously, with cycles, this is
				// never possible.
				List<Vertex> ancestorPriorityVertices = new List<Vertex>();
				DependencyGraph.SortAncestorsWithPostOrderTraversal(this.vertices.Values, ancestorPriorityVertices);

				// Reset the visited flag because we need it again when doing the second depth-first to find the cycles.
				for (int i = 0; i < ancestorPriorityVertices.Count; i++)
					ancestorPriorityVertices[i].visited = false;

				// Create a depth-first spanning forest of the reversed graph (we will do this by using the Dependents collection this 
				// time to traverse through the dependency paths), where each tree root is the greatest sorted vertex in the vertices 
				// not yet in the forest. All trees in the forest will be a strongly connected component (cycle) and due to the choice 
				// of using the greatest sorted vertices first, the earlier trees may depend on the later trees, but not the other way 
				// around. So we will push them onto a stack so that we can get them out in reverse order and assign the sort number in 
				// the correct order. Since we only need to find the vertices in the cycles, we don't actually need to store the trees 
				// in a tree-like structure. A list will suffice.
				Stack<List<Vertex>> stronglyConnectedComponents = new Stack<List<Vertex>>();
				for (int i = ancestorPriorityVertices.Count - 1; i >= 0; i--)
				{
					Vertex vertex = ancestorPriorityVertices[i];
					if (vertex.visited)
						continue;

					List<Vertex> stronglyConnectedComponent = new List<Vertex>();
					this.AddDependentsRecursive(vertex, stronglyConnectedComponent);

					// Sort the cycle in the order in which Excel gives cyclical cells priority.
					stronglyConnectedComponent.Sort(new CircularitySorter());

					stronglyConnectedComponents.Push(stronglyConnectedComponent);
				}

				// Now pop off each strongly connected component from the stack and give their references increasing sort numbers. And 
				// because of the stack's order, the references in each popped off component will never depend on references from the 
				// remaining components in the stack.
				while (stronglyConnectedComponents.Count != 0)
				{
					List<Vertex> stronglyConnectedComponent = stronglyConnectedComponents.Pop();
					bool isInCircularity = DependencyGraph.IsCycle(stronglyConnectedComponent);

					// MD 3/2/12 - 12.1 - Table Support
					// Check if this is a strongly connected component of all named references. If it is, they are not actually
					// treated like normal circularities. They just all return #NAME? errors when used.
					bool isInNamedReferenceCircularity = false;
					if (isInCircularity)
					{
						isInNamedReferenceCircularity = true;
						for (int i = 0; i < stronglyConnectedComponent.Count; i++)
						{
							Vertex vertex = stronglyConnectedComponent[i];
							if ((vertex.ReferenceHolder.Reference is NamedCalcReference) == false)
							{
								isInNamedReferenceCircularity = false;
								break;
							}
						}
					}

					for (int i = 0; i < stronglyConnectedComponent.Count; i++)
					{
						Vertex vertex = stronglyConnectedComponent[i];
						vertex.ReferenceHolder.Formula.DependencySortNumber = sortNumber++;

						// MD 3/2/12 - 12.1 - Table Support
						// First check if this is a named reference circularity.
						//if (isInCircularity)
						if (isInNamedReferenceCircularity)
						{
							vertex.ReferenceHolder.Formula.IsNamedReferenceCircularFormula = true;
						}
						else if (isInCircularity)
						{
							vertex.ReferenceHolder.Formula.IsCircularFormula = true;
							this.calcEngine.circularReferences[vertex.ReferenceHolder.Reference] = null;
						}

						this.RemoveVertex(vertex);
					}
				}

				Debug.Assert(this.vertices.Count == 0, "All vertices should have been sorted and removed from the tree.");

				// MD 3/2/12 - TFS103729
				nextDependencySortNumber = sortNumber;
			}

			#endregion  // Methods


			#region CircularitySorter class

			private class CircularitySorter : IComparer<Vertex>
			{
				int IComparer<Vertex>.Compare(Vertex x, Vertex y)
				{
					CellCalcReference cellReferenceX = x.ReferenceHolder.Reference as CellCalcReference;
					CellCalcReference cellReferenceY = y.ReferenceHolder.Reference as CellCalcReference;

					if (cellReferenceX == null || cellReferenceY == null)
						return 0;

					int result = cellReferenceX.Row.Worksheet.Index - cellReferenceY.Row.Worksheet.Index;

					if (result != 0)
						return result;

					result = cellReferenceX.Row.Index - cellReferenceY.Row.Index;
					if (result != 0)
						return result;

					result = cellReferenceX.ColumnIndex - cellReferenceY.ColumnIndex;
					if (result != 0)
						return result;

					return 0;
				}
			}

			#endregion  // CircularitySorter class

			#region Vertex class

			[DebuggerDisplay("Vertex: {this.ReferenceHolder.Reference.AbsoluteName,nq}")]
			private class Vertex
			{
				#region Member Variables

				private List<Vertex> dependsUpon;
				private List<Vertex> dependents;
				private readonly ReferenceHolder referenceHolder;

				// MD 10/7/11 - TFS91103
				public bool visited;

				#endregion  // Member Variables

				#region Constructor

				public Vertex(ReferenceHolder referenceHolder)
				{
					this.referenceHolder = referenceHolder;
				}

				#endregion  // Constructor

				#region Methods

				public void AddDependent(Vertex dependent)
				{
					if (this.HasDependent(dependent))
						return;

					this.Dependents.Add(dependent);

					// MD 10/7/11 - TFS91103
					// This is now lazily created by the property getter, so just use that.
					//if (dependent.dependsUpon == null)
					//    dependent.dependsUpon = new List<Vertex>();
					//
					//dependent.dependsUpon.Add(this);
					dependent.DependsUponVertices.Add(this);
				}

				public bool HasDependent(Vertex dependent)
				{
					return this.dependents != null && this.dependents.Contains(dependent);
				}

				// MD 4/2/12 - TFS99854
				public void RemoveDependency(Vertex dependent)
				{
					int index = this.Dependents.IndexOf(dependent);
					if (index < 0)
						return;

					this.RemoveDependencyAt(index);
				}

				public Vertex RemoveDependencyAt(int index)
				{
					Vertex dependent = this.Dependents[index];

					this.Dependents.RemoveAt(index);

					if (dependent.dependsUpon != null)
						dependent.dependsUpon.Remove(this);

					return dependent;
				}

				#endregion  // Methods

				#region Properties

				// MD 10/7/11 - TFS91103
				public List<Vertex> DependsUponVertices
				{
					get
					{
						if (this.dependsUpon == null)
							this.dependsUpon = new List<Vertex>();

						return this.dependsUpon;
					}
				}

				public List<Vertex> Dependents
				{
					get
					{
						if (this.dependents == null)
							this.dependents = new List<Vertex>();

						return this.dependents;
					}
				}

				public bool DoesDependOnOthers
				{
					get { return this.dependsUpon != null && this.dependsUpon.Count > 0; }
				}

				public ReferenceHolder ReferenceHolder
				{
					get { return this.referenceHolder; }
				}

				#endregion  // Properties
			}

			#endregion  // Vertex class
		}

		#endregion  // DependencyGraph class
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